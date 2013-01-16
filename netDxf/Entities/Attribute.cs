#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a attribute <see cref="IEntityObject">entity</see>.
    /// </summary>
    /// <remarks>The attribute position and orientation are expressed in local coordinates of the <see cref="Insert">Insert</see> entity to which it belongs.</remarks>
    public class Attribute :
        DxfObject,
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.Attribute;
        private readonly AttributeDefinition definition;
        private object value;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private double rotation;
        private Vector3 normal;
        #endregion

        #region constructor

        /// <summary>
        /// Intitializes a new instance of the <c>Attribute</c> class.
        /// </summary>
        /// <param name="definition"><see cref="AttributeDefinition">Attribute definition</see>.</param>
        /// <remarks>
        /// Althought the attribute entity could override values defined in its definiton for simplicity the implementation has restricted this posibility.
        /// </remarks>
        public Attribute(AttributeDefinition definition)
            : this(definition, null)
        {
        }

        /// <summary>
        /// Intitializes a new instance of the <c>Attribute</c> class.
        /// </summary>
        /// <param name="definition"><see cref="AttributeDefinition">Attribute definition</see>.</param>
        /// <param name="value">Attribute value.</param>
        public Attribute(AttributeDefinition definition, object value)
            : base(DxfObjectCode.Attribute)
        {
            this.definition = definition;
            this.value = value;
            this.color = definition.Color;
            this.layer = definition.Layer;
            this.lineType = definition.LineType;
            this.rotation = definition.Rotation;
            this.normal = definition.Normal;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute definition.
        /// </summary>
        public AttributeDefinition Definition
        {
            get { return this.definition; }
        }

        /// <summary>
        /// Gets or sets the attribute text rotation in degrees.
        /// </summary>
        internal double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the attribute <see cref="netDxf.Vector3">normal</see>.
        /// </summary>
        internal Vector3 Normal
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

        /// <summary>
        /// Gets or sets the attribute value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
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
            get { return null; }
            set
            {
                throw new ArgumentException("Extended data not avaliable for attributes","value");
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
            return TYPE.ToString();
        }

        #endregion
    }
}