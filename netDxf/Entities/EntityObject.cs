#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Represents a generic entity.
    /// </summary>
    public abstract class EntityObject
        : DxfObject, ICloneable
    {
        #region private fields

        private readonly EntityType type;
        protected AciColor color;
        protected Layer layer;
        protected LineType lineType;
        protected Lineweight lineweight;
        protected Transparency transparency;
        protected double lineTypeScale;
        protected bool isVisible;
        protected Vector3 normal;
        protected Dictionary<string, XData> xData;
        protected DxfObject reactor;


        #endregion

        #region constructors

        protected EntityObject(EntityType type, string dxfCode)
            : base(dxfCode)
        {
            this.type = type;
            this.color = AciColor.ByLayer;
            this.layer = Layer.Default;
            this.lineType = LineType.ByLayer;
            this.lineweight = Lineweight.ByLayer;
            this.transparency = Transparency.ByLayer;
            this.lineTypeScale = 1.0;
            this.isVisible = true;
            this.normal = Vector3.UnitZ;
            this.xData = new Dictionary<string, XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the object that has been attached to this entity.
        /// </summary>
        /// <remarks>
        /// This information is subject to change in the future to become a list, an entity can be attached to multiple objects;
        /// but at the moment only the viewport clipping boundary make use of this. Use this information with care.
        /// </remarks>
        public DxfObject Reactor
        {
            get { return this.reactor; }
            internal set { this.reactor = value; }
        }

        /// <summary>
        /// Gets the entity <see cref="EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="AciColor">color</see>.
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
        /// Gets or sets the entity <see cref="Layer">layer</see>.
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
        /// Gets or sets the entity <see cref="LineType">line type</see>.
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
        /// Gets or sets the entity line weight, one unit is always 1/100 mm (default = ByLayer).
        /// </summary>
        public Lineweight Lineweight
        {
            get { return this.lineweight; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineweight = value;
            }
        }

        /// <summary>
        /// Gets or sets layer transparency (default: ByLayer).
        /// </summary>
        public Transparency Transparency
        {
            get { return this.transparency; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.transparency = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity linetype scale.
        /// </summary>
        public double LineTypeScale
        {
            get { return this.lineTypeScale; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", value, "The linetype scale must be greater than zero.");
                this.lineTypeScale = value;
            }
        }

        /// <summary>
        /// Gets or set the entity visibility.
        /// </summary>
        public bool IsVisible
        {
            get { return this.isVisible; }
            set { this.isVisible = value; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (value == Vector3.Zero)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector.");
                this.normal = value;
                this.normal.Normalize();
            }
        }

        /// <summary>
        /// Gets the owner of the actual dxf object.
        /// </summary>
        public new Block Owner
        {
            get { return (Block) this.owner; }
            internal set { this.owner = value; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="XData">extende data</see> (key: ApplicationRegistry.Name, value: XData).
        /// </summary>
        public Dictionary<string, XData> XData
        {
            get { return this.xData; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.xData = value;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            return this.type.ToString();
        }

        #endregion

        #region ICloneable

        /// <summary>
        /// Creates a new entity that is a copy of the current instance.
        /// </summary>
        /// <returns>A new entity that is a copy of this instance.</returns>
        public abstract object Clone();

        #endregion
    }
}