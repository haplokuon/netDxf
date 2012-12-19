#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE. 

#endregion

using System;
using System.Collections.Generic;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{

    /// <summary>
    /// Represents a block insertion <see cref="netDxf.Entities.IEntityObject">entity</see>.
    /// </summary>
    public class Insert :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private readonly EndSequence endSequence;
        private const EntityType TYPE = EntityType.Insert;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private Block block;
        private Vector3 insertionPoint;
        private Vector3 scale;
        private double rotation;
        private Vector3 normal;
        private readonly List<Attribute> attributes;
        private Dictionary<ApplicationRegistry, XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert block definition.</param>
        /// <param name="insertionPoint">Insert <see cref="Vector3">point</see>.</param>
        public Insert(Block block, Vector3 insertionPoint)
            : base (DxfObjectCode.Insert)
        {
            if (block == null)
                throw new ArgumentNullException("block");

            this.block = block;
            this.insertionPoint = insertionPoint;
            this.scale = new Vector3(1.0, 1.0, 1.0);
            this.rotation = 0.0f;
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.attributes = new List<Attribute>();
            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.attributes.Add(new Attribute(attdef));
            }
            this.endSequence = new EndSequence();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert block definition.</param>
        /// <param name="insertionPoint">Insert <see cref="Vector2">point</see>.</param>
        public Insert(Block block, Vector2 insertionPoint)
            : this(block, new Vector3(insertionPoint.X, insertionPoint.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert <see cref="Blocks.Block">block definition</see>.</param>
        public Insert(Block block)
            : this(block, Vector3.Zero)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the insert list of <see cref="Attribute">attributes</see>.
        /// </summary>
        public List<Attribute> Attributes
        {
            get { return this.attributes; }
        }

        /// <summary>
        /// Gets the insert <see cref="Blocks.Block">block definition</see>.
        /// </summary>
        public Block Block
        {
            get { return this.block; }
            internal set { this.block = value; }
        }

        /// <summary>
        /// Gets or sets the insert <see cref="Vector3">point</see>.
        /// </summary>
        public Vector3 InsertionPoint
        {
            get { return this.insertionPoint; }
            set { this.insertionPoint = value; }
        }

        /// <summary>
        /// Gets or sets the insert <see cref="Vector3">scale</see>.
        /// </summary>
        public Vector3 Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        /// <summary>
        /// Gets or sets the insert rotation along the normal vector in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the insert <see cref="Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the entity <see cref="netDxf.Entities.EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.LineType">line type</see>.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.XData">extende data</see>.
        /// </summary>
        public Dictionary<ApplicationRegistry, XData> XData
        {
            get { return this.xData; }
            set { this.xData = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override int AsignHandle(int entityNumber)
        {
            entityNumber = this.endSequence.AsignHandle(entityNumber);
            foreach(Attribute attrib in this.attributes )
            {
                entityNumber = attrib.AsignHandle(entityNumber);
            }
            
            return base.AsignHandle(entityNumber);
        }
        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}