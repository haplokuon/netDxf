#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2013 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a attribute <see cref="EntityObject">entity</see>.
    /// </summary>
    /// <remarks>
    /// The attribute position, rotation, height and width factor values also includes the transformation of the <see cref="Insert">Insert</see> entity to which it belongs.<br />
    /// During the attribute initialization a copy of all attribute definition properties will be copied,
    /// so any changes made to the attribute definition will only be applied to new attribute instances and not to existing ones.
    /// This behaviour is to allow imported <see cref="Insert">Insert</see> entities to have attributes without definition in the block, 
    /// althought this might sound not totally correct it is allowed by AutoCad.
    /// </remarks>
    public class Attribute :
        EntityObject
    {
        #region private fields

        private AttributeDefinition definition;
        private string tag;
        private object value;
        private TextStyle style;
        private Vector3 position;
        private AttributeFlags flags;
        private double height;
        private double widthFactor;
        private double rotation;
        private TextAlignment alignment;

        #endregion

        #region constructor

        internal Attribute()
            : base(EntityType.Attribute, DxfObjectCode.Attribute)
        {
        }

        /// <summary>
        /// Intitializes a new instance of the <c>Attribute</c> class.
        /// </summary>
        /// <param name="definition"><see cref="AttributeDefinition">Attribute definition</see>.</param>
        /// <remarks>
        /// Althought the attribute entity could override values defined in its definiton for simplicity the implementation has restricted this posibility.
        /// </remarks>
        public Attribute(AttributeDefinition definition)
            : base(EntityType.Attribute, DxfObjectCode.Attribute)
        {
            if (definition == null)
                throw new ArgumentNullException("definition");
            this.definition = definition;
            this.tag = definition.Tag;
            this.value = definition.Value;
            this.style = definition.Style;
            this.position = definition.Position;
            this.flags = definition.Flags;
            this.height = definition.Height;
            this.widthFactor = definition.WidthFactor;
            this.rotation = definition.Rotation;
            this.alignment = definition.Alignment;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute definition.
        /// </summary>
        /// <remarks>If the insert attribute has no definition it will return null.</remarks>
        public AttributeDefinition Definition
        {
            get { return this.definition; }
            internal set { this.definition = value; }
        }

        /// <summary>
        /// Gets the attribute tag.
        /// </summary>
        public string Tag
        {
            get { return this.tag; }
            internal set { this.tag = value;}
        }

        /// <summary>
        /// Gets or sets the attribute text height.
        /// </summary>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The height should be greater than zero."));
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute text width factor.
        /// </summary>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                    throw (new ArgumentOutOfRangeException("value", value, "The width factor should be greater than zero."));
                this.widthFactor = value;
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

        /// <summary>
        /// Gets or sets the attribute text style.
        /// </summary>
        /// <remarks>
        /// The <see cref="TextStyle">text style</see> defines the basic properties of the information text.
        /// </remarks>
        public TextStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.style = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute <see cref="Vector3">position</see> in object coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the attribute flags.
        /// </summary>
        public AttributeFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        /// <summary>
        /// Gets or sets the attribute text rotation in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the text alignment.
        /// </summary>
        public TextAlignment Alignment
        {
            get { return alignment; }
            set { alignment = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Attribute that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Attribute that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new Attribute
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //Attribute properties
                Definition = this.definition,
                Tag = this.tag,
                Height = this.height,
                WidthFactor = this.widthFactor,
                Value = this.value,
                Style = this.style,
                Position = this.position,
                Flags = this.flags,
                Rotation = this.rotation,
                Alignment = this.alignment
            };
        }

        #endregion

    }
}