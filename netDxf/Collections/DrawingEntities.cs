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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using netDxf.Entities;
using netDxf.Objects;
using Trace = netDxf.Entities.Trace;

namespace netDxf.Collections
{
    /// <summary>
    /// Gives direct access to operations related with the entities in a drawing.
    /// These are no more than shortcuts to the real place where the entities are stored in a document (drawing.Layouts[layoutName].AssociatedBlock.Entities).
    /// The layout where the operations are performed is defined by the ActiveLayout property, by default the active layout is the Model.
    /// </summary>
    public sealed class DrawingEntities
    {
        #region private fields

        private readonly DxfDocument document;
        private string activeLayout;

        #endregion

        #region constructors

        internal DrawingEntities(DxfDocument document)
        {
            this.document = document;
            this.activeLayout = Layout.ModelSpaceName;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the name of the active layout.
        /// </summary>
        public string ActiveLayout
        {
            get { return this.activeLayout; }
            set
            {
                if (!this.document.Layouts.Contains(value))
                {
                    throw new ArgumentException(string.Format("The layout {0} does not exist.", value), nameof(value));
                }
                this.activeLayout = value;
            }
        }

        /// <summary>
        /// Gets the complete list <see cref="EntityObject">entities</see> contained in the active layout.
        /// </summary>
        public IEnumerable<EntityObject> All
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities; }
        }

        /// <summary>
        /// Gets the list of <see cref="Arc">arcs</see> contained in the active layout.
        /// </summary>
        public IEnumerable<Arc> Arcs
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Arc>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Ellipse">ellipses</see> in the active layout.
        /// </summary>
        public IEnumerable<Ellipse> Ellipses
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Ellipse>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Circle">circles</see> in the active layout.
        /// </summary>
        public IEnumerable<Circle> Circles
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Circle>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Face3D">3d faces</see> in the active layout.
        /// </summary>
        public IEnumerable<Face3D> Faces3D
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Face3D>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Solid">solids</see> in the active layout.
        /// </summary>
        public IEnumerable<Solid> Solids
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Solid>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Entities.Trace">traces</see> in the active layout.
        /// </summary>
        public IEnumerable<Trace> Traces
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Trace>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Insert">inserts</see> in the active layout.
        /// </summary>
        public IEnumerable<Insert> Inserts
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Insert>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Line">lines</see> in the active layout.
        /// </summary>
        public IEnumerable<Line> Lines
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Line>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Shape">shapes</see> in the active layout.
        /// </summary>
        public IEnumerable<Shape> Shapes
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Shape>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Polyline2D">polylines</see> in the active layout.
        /// </summary>
        public IEnumerable<Polyline2D> Polylines2D
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Polyline2D>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Polyline3D">polylines</see> in the active layout.
        /// </summary>
        public IEnumerable<Polyline3D> Polylines3D
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Polyline3D>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="PolyfaceMeshes">polyface meshes</see> in the active layout.
        /// </summary>
        public IEnumerable<PolyfaceMesh> PolyfaceMeshes
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<PolyfaceMesh>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="PolygonMeshes">polygon meshes</see> in the active layout.
        /// </summary>
        public IEnumerable<PolygonMesh> PolygonMeshes
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<PolygonMesh>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Point">points</see> in the active layout.
        /// </summary>
        public IEnumerable<Point> Points
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Point>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Text">texts</see> in the active layout.
        /// </summary>
        public IEnumerable<Text> Texts
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Text>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="MText">multiline texts</see> in the active layout.
        /// </summary>
        public IEnumerable<MText> MTexts
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<MText>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Hatch">hatches</see> in the active layout.
        /// </summary>
        public IEnumerable<Hatch> Hatches
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Hatch>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Image">images</see> in the active layout.
        /// </summary>
        public IEnumerable<Image> Images
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Image>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Mesh">mesh</see> in the active layout.
        /// </summary>
        public IEnumerable<Mesh> Meshes
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Mesh>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Leader">leader</see> in the active layout.
        /// </summary>
        public IEnumerable<Leader> Leaders
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Leader>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Tolerance">tolerance</see> in the active layout.
        /// </summary>
        public IEnumerable<Tolerance> Tolerances
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Tolerance>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Underlay">underlay</see> in the active layout.
        /// </summary>
        public IEnumerable<Underlay> Underlays
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Underlay>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="MLine">multilines</see> in the active layout.
        /// </summary>
        public IEnumerable<MLine> MLines
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<MLine>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Dimension">dimensions</see> in the active layout.
        /// </summary>
        public IEnumerable<Dimension> Dimensions
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Dimension>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Spline">splines</see> in the active layout.
        /// </summary>
        public IEnumerable<Spline> Splines
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Spline>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Ray">rays</see> in the active layout.
        /// </summary>
        public IEnumerable<Ray> Rays
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Ray>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Viewport">viewports</see> in the active layout.
        /// </summary>
        public IEnumerable<Viewport> Viewports
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Viewport>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="XLine">extension lines</see> in the active layout.
        /// </summary>
        public IEnumerable<XLine> XLines
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<XLine>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="Wipeout">wipeouts</see> in the active layout.
        /// </summary>
        public IEnumerable<Wipeout> Wipeouts
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.Entities.OfType<Wipeout>(); }
        }

        /// <summary>
        /// Gets the list of <see cref="AttributeDefinition">attribute definitions</see> in the active layout.
        /// </summary>
        public IEnumerable<AttributeDefinition> AttributeDefinitions
        {
            get { return this.document.Layouts[this.activeLayout].AssociatedBlock.AttributeDefinitions.Values; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Adds a list of <see cref="EntityObject">entities</see> to the active layout of the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to add to the document.</param>
        public void Add(IEnumerable<EntityObject> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (EntityObject entity in entities)
            {
                this.Add(entity);
            }
        }

        /// <summary>
        /// Adds an <see cref="EntityObject">entity</see> to the active layout of the document.
        /// </summary>
        /// <param name="entity">An <see cref="EntityObject">entity</see> to add to the document.</param>
        public void Add(EntityObject entity)
        {
            // entities already owned by another document are not allowed
            if (entity.Owner != null)
            {
                throw new ArgumentException("The entity already belongs to a document. Clone it instead.", nameof(entity));
            }

            this.document.Blocks[this.document.Layouts[this.activeLayout].AssociatedBlock.Name].Entities.Add(entity);
        }

        /// <summary>
        /// Removes a list of <see cref="EntityObject">entities</see> from the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to remove from the document.</param>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.<br />
        /// This includes empty layers, blocks not referenced anymore, line types, text styles, dimension styles, and application registries.<br />
        /// Entities that are part of a block definition will not be removed.
        /// </remarks>
        public void Remove(IEnumerable<EntityObject> entities)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            foreach (EntityObject entity in entities)
            {
                this.Remove(entity);
            }
        }

        /// <summary>
        /// Removes an <see cref="EntityObject">entity</see> from the document.
        /// </summary>
        /// <param name="entity">The <see cref="EntityObject">entity</see> to remove from the document.</param>
        /// <returns>True if item is successfully removed; otherwise, false.</returns>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.<br />
        /// This includes empty layers, blocks not referenced anymore, line types, text styles, dimension styles, multiline styles, groups, and application registries.<br />
        /// Entities that are part of a block definition will not be removed.
        /// </remarks>
        public bool Remove(EntityObject entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity.Owner == null)
            {
                return false;
            }

            if (entity.Owner.Record.Layout == null)
            {
                return false;
            }

            // this check is done when the entity is removed from the block
            //if (entity.Reactors.Count > 0)
            //{
            //    return false;
            //}

            if (!ReferenceEquals(this.document, entity.Owner.Owner.Owner.Owner))
            {
                return false;
            }

            // if an entity belongs to a document always has a handle
            Debug.Assert(entity.Handle != null, "The entity has no handle.");

            // if an entity belongs to a document its handle should have been stored
            Debug.Assert(this.document.AddedObjects.ContainsKey(entity.Handle), "The entity has no handle but belongs to a document.");

            return this.document.Blocks[entity.Owner.Name].Entities.Remove(entity);

        }

        #endregion
    }
}
