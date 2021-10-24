#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
#endregion

using System;
using netDxf.Tables;

namespace netDxf.Objects
{
    /// <summary>
    /// Represents the state of the properties of a layer.
    /// </summary>
    public class LayerStateProperties :
        ICloneable
    {
        #region private fields

        private readonly string name;
        private LayerPropertiesFlags flags;
        private string linetype;
        private AciColor color;
        private Lineweight lineweight;
        private Transparency transparency;
        //private string plotStyle;

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>LayerStateProperties</c> class.
        /// </summary>
        /// <param name="name">Name of the layer state properties.</param>
        public LayerStateProperties(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.name = name;
            this.flags = LayerPropertiesFlags.Plot;
            this.linetype = Linetype.DefaultName;
            this.color = AciColor.Default;
            this.lineweight = Lineweight.Default;
            this.transparency = new Transparency(0);
            //this.plotStyle = "Color_7";
        }

        /// <summary>
        /// Initializes a new instance of the <c>LayerStateProperties</c> class.
        /// </summary>
        /// <param name="layer">Layer from which copy the properties.</param>
        public LayerStateProperties(Layer layer)
        {
            this.name = layer.Name;
            if (!layer.IsVisible) this.flags |= LayerPropertiesFlags.Hidden;
            if (layer.IsFrozen) this.flags |= LayerPropertiesFlags.Frozen;
            if (layer.IsLocked) this.flags |= LayerPropertiesFlags.Locked;
            if (layer.Plot) this.flags |= LayerPropertiesFlags.Plot;
            this.linetype = layer.Linetype.Name;
            this.color = (AciColor) layer.Color.Clone();
            this.lineweight = layer.Lineweight;
            this.transparency = (Transparency) layer.Transparency.Clone();
            //this.plotStyle = "Color_" + layer.Color.Index;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the layer properties name.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// Layer properties flags.
        /// </summary>
        public LayerPropertiesFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Layer properties linetype name.
        /// </summary>
        public string LinetypeName
        {
            get { return this.linetype; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.linetype = value;
            }
        }

        /// <summary>
        /// Layer properties color.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        /// <summary>
        /// Layer properties lineweight.
        /// </summary>
        public Lineweight Lineweight
        {
            get { return this.lineweight; }
            set { this.lineweight = value; }
        }

        /// <summary>
        /// Layer properties transparency.
        /// </summary>
        public Transparency Transparency
        {
            get { return this.transparency; }
            set { this.transparency = value; }
        }

        ///// <summary>
        ///// Layer properties plot style name.
        ///// </summary>
        //public string PlotStyleName
        //{
        //    get { return this.plotStyle; }
        //    set { this.plotStyle = value; }
        //}

        #endregion

        #region public methods

        /// <summary>
        /// Copy the layer to the current layer state properties.
        /// </summary>
        /// <param name="layer">Layer from which copy the properties.</param>
        /// <param name="options">Layer properties to copy.</param>
        public void CopyFrom(Layer layer, LayerPropertiesRestoreFlags options)
        {
            if (!string.Equals(this.name, layer.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only a layer with the same name can be copied.", nameof(layer));
            }

            this.flags = LayerPropertiesFlags.None;

            if (options.HasFlag(LayerPropertiesRestoreFlags.Hidden))
            {
                if (!layer.IsVisible) this.flags |= LayerPropertiesFlags.Hidden;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Frozen))
            {
                if (layer.IsFrozen) this.flags |= LayerPropertiesFlags.Frozen;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Locked))
            {
                if (layer.IsLocked) this.flags |= LayerPropertiesFlags.Locked;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Plot))
            {
                if (layer.Plot) this.flags |= LayerPropertiesFlags.Plot;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Linetype))
            {
                this.linetype = layer.Linetype.Name;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Color))
            {
                this.color = (AciColor) layer.Color.Clone();
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Lineweight))
            {
                this.lineweight = layer.Lineweight;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Transparency))
            {
                this.transparency = (Transparency) layer.Transparency.Clone();
            }
        }

        /// <summary>
        /// Copy the current layer state properties to a layer.
        /// </summary>
        /// <param name="layer">Layer to which copy the properties.</param>
        /// <param name="options">Layer properties to copy.</param>
        public void CopyTo(Layer layer, LayerPropertiesRestoreFlags options)
        {
            if (!string.Equals(this.name, layer.Name, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only a layer with the same name can be copied.", nameof(layer));
            }

            if(options.HasFlag(LayerPropertiesRestoreFlags.Hidden))
            {
                layer.IsVisible = !this.flags.HasFlag(LayerPropertiesFlags.Hidden);
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Frozen))
            {
                layer.IsFrozen = this.flags.HasFlag(LayerPropertiesFlags.Frozen);
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Locked))
            {
                layer.IsLocked = this.flags.HasFlag(LayerPropertiesFlags.Locked);
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Plot))
            {
                layer.Plot = this.flags.HasFlag(LayerPropertiesFlags.Plot);
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Linetype))
            {
                Linetype line = null;
                if (layer.Owner != null)
                {
                    DxfDocument doc = layer.Owner.Owner;
                    line = doc.Linetypes[this.LinetypeName];
                }
                layer.Linetype = line ?? new Linetype(this.LinetypeName);
                
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Color))
            {
                layer.Color = (AciColor) this.Color.Clone();
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Lineweight))
            {
                layer.Lineweight = this.Lineweight;
            }
            if (options.HasFlag(LayerPropertiesRestoreFlags.Transparency))
            {
                layer.Transparency = (Transparency) this.Transparency.Clone();
            }
        }

        /// <summary>
        /// Compares the stored properties with the specified layer.
        /// </summary>
        /// <param name="layer">Layer to compare with.</param>
        /// <returns>If the stored properties are the same as the specified layer it returns true, false otherwise.</returns>
        public bool CompareWith(Layer layer)
        {
            if (!string.Equals(layer.Name, this.name, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (layer.IsVisible != !this.flags.HasFlag(LayerPropertiesFlags.Hidden))
            {
                return false;
            }

            if (layer.IsFrozen != this.flags.HasFlag(LayerPropertiesFlags.Frozen))
            {
                return false;
            }

            if (layer.IsLocked != this.flags.HasFlag(LayerPropertiesFlags.Locked))
            {
                return false;
            }

            if (layer.Plot != this.flags.HasFlag(LayerPropertiesFlags.Plot))
            {
                return false;
            }

            if (!string.Equals(layer.Linetype.Name, this.LinetypeName, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!layer.Color.Equals(this.color))
            {
                return false;
            }

            if (layer.Lineweight != this.lineweight)
            {
                return false;
            }

            if (!layer.Transparency.Equals(this.transparency))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            return new LayerStateProperties(this.name)
            {
                Flags = this.flags,
                LinetypeName = this.linetype,
                Color = (AciColor) this.color.Clone(),
                Lineweight = this.lineweight,
                Transparency = (Transparency) this.transparency.Clone(),
                //PlotStyleName = this.plotStyle
            };
        }

        #endregion

    }
}