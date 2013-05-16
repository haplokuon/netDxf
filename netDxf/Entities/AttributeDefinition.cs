#region netDxf, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL.

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
    ///<summary>Attribute flags.</summary>
    [Flags]
    public enum AttributeFlags
    {
        /// <summary>
        /// Attribute is visible.
        /// </summary>
        Visible = 0,
        /// <summary>
        /// Attribute is invisible (does not appear).
        /// </summary>
        Hidden = 1,
        /// <summary>
        /// This is a constant attribute.
        /// </summary>
        Constant = 2,
        /// <summary>
        /// Verification is required on input of this attribute.
        /// </summary>
        Verify = 4,
        /// <summary>
        /// Attribute is preset (no prompt during insertion).
        /// </summary>
        Predefined = 8
    }

    /// <summary>
    /// Represents a attribute definition <see cref="EntityObject">entity</see>.
    /// </summary>
    public class AttributeDefinition :
        EntityObject
    {
        #region private fields

        private readonly string id;
        private string text;
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

        /// <summary>
        /// Intitializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="id">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        public AttributeDefinition(string id)
            : this(id, TextStyle.Default)
        {
        }

        /// <summary>
        /// Intitializes a new instance of the <c>AttributeDefiniton</c> class.
        /// </summary>
        /// <param name="id">Attribute identifier, the parameter <c>id</c> string cannot contain spaces.</param>
        /// <param name="style">Attribute <see cref="TextStyle">text style</see>.</param>
        public AttributeDefinition(string id, TextStyle style)
            : base(EntityType.AttributeDefinition, DxfObjectCode.AttributeDefinition)
        {
            if (id.Contains(" "))
                throw new ArgumentException("The id string cannot contain spaces", "id");
            this.id = id;
            this.flags = AttributeFlags.Visible;
            this.text = string.Empty;
            this.value = null;
            this.position = Vector3.Zero;
            this.style = style;
            this.height = MathHelper.IsZero(this.style.Height) ? 1.0 : style.Height;
            this.widthFactor = style.WidthFactor;
            this.rotation = 0.0;
            this.alignment = TextAlignment.BaselineLeft;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets the attribute identifier.
        /// </summary>
        public string Id
        {
            get { return this.id; }
        }

        /// <summary>
        /// Gets or sets the attribute information text.
        /// </summary>
        public string Text
        {
            get { return this.text; }
            set { this.text = value; }
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
        /// Gets or sets the attribute default value.
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or sets  the attribute text style.
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
                    throw new NullReferenceException("value");
               this.style = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute <see cref="Vector3">position</see> in local coordinates.
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
            set { this.rotation = value; }
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

    }
}