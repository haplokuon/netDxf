#region netDxf library licensed under the MIT License, Copyright © 2009-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
//                        netDxf library
// Copyright © 2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software
// and associated documentation files (the “Software”), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Collections
{
    /// <summary>
    /// Manages the list of layer states in a drawing.
    /// </summary>
    public class LayerStateManager :
        TableObjects<LayerState>
    {
        #region private fields

        private LayerPropertiesRestoreFlags options;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>LayerStateManager</c> class.
        /// </summary>
        /// <param name="owner">Layers list associated with the current <c>LayerStateManager</c>.</param>
        internal LayerStateManager(DxfDocument owner)
            : this(owner, null)
        {
        }

        internal LayerStateManager(DxfDocument document, string handle)
            : base(document, DxfObjectCode.LayerStates, handle)
        {
            this.options = LayerPropertiesRestoreFlags.All;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the restoring options when updating the layer state from the layers list and vice versa.
        /// </summary>
        public LayerPropertiesRestoreFlags Options
        {
            get { return this.options; }
            set { this.options = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds a new LayerState from the current state of the layers.
        /// </summary>
        /// <param name="layerStateName">Layer state name.</param>
        public void AddNew(string layerStateName)
        {
            this.AddNew(layerStateName, String.Empty);
        }

        /// <summary>
        /// Adds a new LayerState from the current state of the layers.
        /// </summary>
        /// <param name="layerStateName">Layer state name.</param>
        /// <param name="layerStateDescription">Layer state description.</param>
        public void AddNew(string layerStateName, string layerStateDescription)
        {
            LayerState layerState = new LayerState(layerStateName, this.Owner.Layers) {Description = layerStateDescription};
            this.Add(layerState);
        }

        /// <summary>
        /// Restores the properties of the current layers list according to the specified layers state.
        /// </summary>
        /// <param name="layerStateName">Layer state name to restore.</param>
        public void Restore(string layerStateName)
        {
            LayerState ls = this.list[layerStateName];
            if (ls == null)
            {
                throw new ArgumentException("Invalid layer state name.", nameof(layerStateName));
            }

            this.Owner.DrawingVariables.CLayer = ls.CurrentLayer;

            foreach (LayerStateProperties layerProperties in ls.Properties.Values)
            {
                if (!this.Owner.Layers.Contains(layerProperties.Name))
                {
                    Layer layer = this.Owner.Layers.Add(new Layer(layerProperties.Name));
                    layerProperties.CopyTo(layer, this.options);
                }
                else
                {
                    layerProperties.CopyTo(this.Owner.Layers[layerProperties.Name], this.options);
                }
            }
        }

        /// <summary>
        /// Updates the specified layer state according to the properties of the current layers list.
        /// </summary>
        /// <param name="layerStateName">Layer state name to update.</param>
        public void Update(string layerStateName)
        {
            LayerState ls = this.list[layerStateName];
            if (ls == null)
            {
                throw new ArgumentException("Invalid layer state name.", nameof(layerStateName));
            }

            ls.CurrentLayer = this.Owner.DrawingVariables.CLayer;

            foreach (Layer layer in this.Owner.Layers.Items)
            {
                if (!ls.Properties.ContainsKey(layer.Name))
                {
                    ls.Properties.Add(layer.Name, new LayerStateProperties(layer));
                }
                else
                {
                    ls.Properties[layer.Name].CopyFrom(layer, this.options);
                }
            }
        }

        /// <summary>
        /// Imports a layer state from a LAS file.
        /// </summary>
        /// <param name="file">LAS file to import.</param>
        /// <param name="overwrite">Defines if the imported layer state will overwrite any existing one with the same name.</param>
        public void Import(string file, bool overwrite)
        {
            LayerState ls = LayerState.Load(file);
            if (ls == null)
            {
                throw new Exception("Unknown error when loading the LAS file: " + file);
            }

            if (this.list.ContainsKey(ls.Name))
            {
                if (overwrite)
                {
                    this.Remove(this.list[ls.Name]);
                    this.Add(ls);
                }
            }
            else
            {
                this.Add(ls);
            }

            this.Restore(ls.Name);
        }

        /// <summary>
        /// Exports a layer state to a LAS file.
        /// </summary>
        /// <param name="file">LAS file to export.</param>
        /// <param name="layerStateName">Layer state name to export.</param>
        public void Export(string file, string layerStateName)
        {
            LayerState ls = this.list[layerStateName];
            if (ls == null)
            {
                throw new ArgumentException("Invalid layer state name.", nameof(layerStateName));
            }

            ls.Save(file);
        }

        /// <summary>
        /// Removes all layers states.
        /// </summary>
        public void RemoveAll()
        {
            string[] names = new string[this.Names.Count];
            this.Names.CopyTo(names, 0);
            foreach (string name in names)
            {
                this.Remove(name);
            }
        }

        #endregion

        #region overrrides

        /// <summary>
        /// Adds a LayerState to the list.
        /// </summary>
        /// <param name="layerState"><see cref="LayerState">LayerState</see> to add to the list.</param>
        /// <param name="assignHandle">Specifies if a handle needs to be generated for the layerState parameter.</param>
        /// <returns>
        /// If a LayerState already exists with the same name as the instance that is being added the method returns the existing LayerState,
        /// if not it will return the new LayerState.
        /// </returns>
        internal override LayerState Add(LayerState layerState, bool assignHandle)
        {
            if (layerState == null)
            {
                throw new ArgumentNullException(nameof(layerState));
            }

            if (this.list.TryGetValue(layerState.Name, out LayerState add))
            {
                return add;
            }

            if (assignHandle || string.IsNullOrEmpty(layerState.Handle))
            {
                this.Owner.NumHandles = layerState.AssignHandle(this.Owner.NumHandles);
            }

            this.list.Add(layerState.Name, layerState);
            this.references.Add(layerState.Name, new List<DxfObject>());

            layerState.Owner = this;

            layerState.NameChanged += this.Item_NameChanged;

            this.Owner.AddedObjects.Add(layerState.Handle, layerState);

            foreach (LayerStateProperties prop in layerState.Properties.Values)
            {
                // if the layer state contains a layer name not found in the layer list of the document a new one will be created
                if (!this.Owner.Layers.Contains(prop.Name))
                {
                    this.Owner.Layers.Add(new Layer(prop.Name));
                }

                // if a layer state contains a linetype name not found in the linetype list of the document it will be override by the default "Continuous" linetype
                if (!this.Owner.Linetypes.Contains(prop.LinetypeName))
                {
                    prop.LinetypeName = Linetype.DefaultName;
                }
            }

            return layerState;
        }

        /// <summary>
        /// Removes a LayerState.
        /// </summary>
        /// <param name="name"><see cref="LayerState">LayerState</see> name to remove from the document.</param>
        /// <returns>True if the LayerState has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved LayerState or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(string name)
        {
            return this.Remove(this[name]);
        }

        /// <summary>
        /// Removes a LayerState.
        /// </summary>
        /// <param name="item"><see cref="LayerState">LayerState</see> to remove from the document.</param>
        /// <returns>True if the LayerState has been successfully removed, or false otherwise.</returns>
        /// <remarks>Reserved LayerState or any other referenced by objects cannot be removed.</remarks>
        public override bool Remove(LayerState item)
        {
            if (item == null)
            {
                return false;
            }

            if (!this.Contains(item))
            {
                return false;
            }

            if (item.IsReserved)
            {
                return false;
            }

            if (this.references[item.Name].Count != 0)
            {
                return false;
            }

            this.Owner.AddedObjects.Remove(item.Handle);
            this.references.Remove(item.Name);
            this.list.Remove(item.Name);

            item.Handle = null;
            item.Owner = null;

            item.NameChanged -= this.Item_NameChanged;

            return true;
        }

        #endregion
        
        #region Layer events

        private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
        {
            if (this.Contains(e.NewValue))
            {
                throw new ArgumentException("There is already another layer with the same name.");
            }

            this.list.Remove(sender.Name);
            this.list.Add(e.NewValue, (LayerState) sender);

            List<DxfObject> refs = this.references[sender.Name];
            this.references.Remove(sender.Name);
            this.references.Add(e.NewValue, refs);
        }

        #endregion
    }
}