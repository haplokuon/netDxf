#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an attribute.
    /// </summary>
    /// <remarks>
    /// The attribute position, rotation, height and width factor values also includes the transformation of the <see cref="Insert">Insert</see> entity to which it belongs.<br />
    /// During the attribute initialization a copy of all attribute definition properties will be copied,
    /// so any changes made to the attribute definition will only be applied to new attribute instances and not to existing ones.
    /// This behavior is to allow imported <see cref="Insert">Insert</see> entities to have attributes without definition in the block, 
    /// although this might sound not totally correct it is allowed by AutoCad.
    /// </remarks>
    public class Attribute :
        DxfObject,
        ICloneable
    {
        #region delegates and events

        public delegate void LayerChangedEventHandler(Attribute sender, TableObjectChangedEventArgs<Layer> e);

        public event LayerChangedEventHandler LayerChanged;

        protected virtual Layer OnLayerChangedEvent(Layer oldLayer, Layer newLayer)
        {
            LayerChangedEventHandler ae = this.LayerChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Layer> eventArgs = new TableObjectChangedEventArgs<Layer>(oldLayer, newLayer);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newLayer;
        }

        public delegate void LinetypeChangedEventHandler(Attribute sender, TableObjectChangedEventArgs<Linetype> e);

        public event LinetypeChangedEventHandler LinetypeChanged;

        protected virtual Linetype OnLinetypeChangedEvent(Linetype oldLinetype, Linetype newLinetype)
        {
            LinetypeChangedEventHandler ae = this.LinetypeChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<Linetype> eventArgs = new TableObjectChangedEventArgs<Linetype>(oldLinetype, newLinetype);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newLinetype;
        }

        public delegate void TextStyleChangedEventHandler(Attribute sender, TableObjectChangedEventArgs<TextStyle> e);

        public event TextStyleChangedEventHandler TextStyleChanged;

        protected virtual TextStyle OnTextStyleChangedEvent(TextStyle oldTextStyle, TextStyle newTextStyle)
        {
            TextStyleChangedEventHandler ae = this.TextStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<TextStyle> eventArgs = new TableObjectChangedEventArgs<TextStyle>(oldTextStyle, newTextStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newTextStyle;
        }

        #endregion

        #region private fields

        private AciColor color;
        private Layer layer;
        private Linetype linetype;
        private Lineweight lineweight;
        private Transparency transparency;
        private double linetypeScale;
        private bool isVisible;
        private Vector3 normal;

        private AttributeDefinition definition;
        private readonly string tag;
        private string attValue;
        private TextStyle style;
        private Vector3 position;
        private AttributeFlags flags;
        private double height;
        private double widthFactor;
        private double width;
        private double obliqueAngle;
        private double rotation;
        private TextAlignment alignment;
        private bool isBackward;
        private bool isUpsideDown;

        #endregion

        #region constructor

        internal Attribute(string tag)
            : base(DxfObjectCode.Attribute)
        {
            this.tag = string.IsNullOrEmpty(tag) ? string.Empty : tag;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Attribute</c> class.
        /// </summary>
        /// <param name="definition"><see cref="AttributeDefinition">Attribute definition</see>.</param>
        public Attribute(AttributeDefinition definition)
            : base(DxfObjectCode.Attribute)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            this.color = definition.Color;
            this.layer = definition.Layer;
            this.linetype = definition.Linetype;
            this.lineweight = definition.Lineweight;
            this.linetypeScale = definition.LinetypeScale;
            this.transparency = definition.Transparency;
            this.isVisible = definition.IsVisible;
            this.normal = definition.Normal;

            this.definition = definition;
            this.tag = definition.Tag;
            this.attValue = definition.Value;
            this.style = definition.Style;
            this.position = definition.Position;
            this.flags = definition.Flags;
            this.height = definition.Height;
            this.width = definition.Width;
            this.widthFactor = definition.WidthFactor;
            this.obliqueAngle = definition.ObliqueAngle;
            this.rotation = definition.Rotation;
            this.alignment = definition.Alignment;
            this.isBackward = definition.IsBackward;
            this.isUpsideDown = definition.IsUpsideDown;
        }

        #endregion

        #region public property

        /// <summary>
        /// Gets or sets the entity <see cref="AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                this.color = value ?? throw new ArgumentNullException(nameof(value));
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
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.layer = this.OnLayerChangedEvent(this.layer, value);
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="Linetype">line type</see>.
        /// </summary>
        public Linetype Linetype
        {
            get { return this.linetype; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.linetype = this.OnLinetypeChangedEvent(this.linetype, value);
            }
        }

        /// <summary>
        /// Gets or sets the entity line weight, one unit is always 1/100 mm (default = ByLayer).
        /// </summary>
        public Lineweight Lineweight
        {
            get { return this.lineweight; }
            set { this.lineweight = value; }
        }

        /// <summary>
        /// Gets or sets layer transparency (default: ByLayer).
        /// </summary>
        public Transparency Transparency
        {
            get { return this.transparency; }
            set
            {
                this.transparency = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the entity line type scale.
        /// </summary>
        public double LinetypeScale
        {
            get { return this.linetypeScale; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The line type scale must be greater than zero.");
                }
                this.linetypeScale = value;
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
                this.normal = Vector3.Normalize(value);
                if (Vector3.IsZero(this.normal))
                {
                    throw new ArgumentException("The normal can not be the zero vector.", nameof(value));
                }
            }
        }

        /// <summary>
        /// Gets the owner of the actual DXF object.
        /// </summary>
        public new Insert Owner
        {
            get { return (Insert) base.Owner; }
            internal set { base.Owner = value; }
        }

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
        }

        /// <summary>
        /// Gets or sets the text height.
        /// </summary>
        /// <remarks>
        /// Valid values must be greater than zero. Default: 1.0.<br />
        /// When Alignment.Aligned is used this value is not applicable, it will be automatically adjusted so the text will fit in the specified width.
        /// </remarks>
        public double Height
        {
            get { return this.height; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The height should be greater than zero.");
                }
                this.height = value;
            }
        }

        /// <summary>
        /// Gets or sets the text width, only applicable for text Alignment.Fit and Alignment.Align.
        /// </summary>
        /// <remarks>Valid values must be greater than zero. Default: 1.0.</remarks>
        public double Width
        {
            get { return this.width; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The Text width must be greater than zero.");
                }
                this.width = value;
            }
        }

        /// <summary>
        /// Gets or sets the width factor.
        /// </summary>
        /// <remarks>
        /// Valid values range from 0.01 to 100. Default: 1.0.<br />
        /// When Alignment.Fit is used this value is not applicable, it will be automatically adjusted so the text will fit in the specified width.
        /// </remarks>
        public double WidthFactor
        {
            get { return this.widthFactor; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The width factor should be greater than zero.");
                }
                this.widthFactor = value;
            }
        }

        /// <summary>
        /// Gets or sets the font oblique angle.
        /// </summary>
        /// <remarks>Valid values range from -85 to 85. Default: 0.0.</remarks>
        public double ObliqueAngle
        {
            get { return this.obliqueAngle; }
            set
            {
                if (value < -85.0 || value > 85.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The oblique angle valid values range from -85 to 85.");
                }
                this.obliqueAngle = value;
            }
        }

        /// <summary>
        /// Gets or sets the attribute value.
        /// </summary>
        public string Value
        {
            get { return this.attValue; }
            set { this.attValue = string.IsNullOrEmpty(value) ? string.Empty : value; }
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
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.style = this.OnTextStyleChangedEvent(this.style, value);
            }
        }

        /// <summary>
        /// Gets or sets the attribute <see cref="Vector3">position</see>.
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
            get { return this.alignment; }
            set { this.alignment = value; }
        }

        /// <summary>
        /// Gets or sets if the attribute text is backward (mirrored in X).
        /// </summary>
        public bool IsBackward
        {
            get { return this.isBackward; }
            set { this.isBackward = value; }
        }

        /// <summary>
        /// Gets or sets if the attribute text is upside down (mirrored in Y).
        /// </summary>
        public bool IsUpsideDown
        {
            get { return this.isUpsideDown; }
            set { this.isUpsideDown = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Moves, scales, and/or rotates the current attribute given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            bool mirrText;
            if (this.Owner == null)
            {
                mirrText = Text.DefaultMirrText;
            }
            else if (this.Owner.Owner == null)
            {
                mirrText = Text.DefaultMirrText;
            }
            else
            {
                mirrText = this.Owner.Owner.Record.Owner.Owner.DrawingVariables.MirrText;
            }

            Vector3 newPosition = transformation * this.Position + translation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);

            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal);
            transWO = transWO.Transpose();

            List<Vector2> uv = MathHelper.Transform(
                new[]
                {
                    this.WidthFactor * this.Height * Vector2.UnitX,
                    new Vector2(this.Height * Math.Tan(this.ObliqueAngle * MathHelper.DegToRad), this.Height)
                },
                this.Rotation * MathHelper.DegToRad,
                CoordinateSystem.Object, CoordinateSystem.World);

            Vector3 v;
            v = transOW * new Vector3(uv[0].X, uv[0].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newUvector = new Vector2(v.X, v.Y);

            v = transOW * new Vector3(uv[1].X, uv[1].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newVvector = new Vector2(v.X, v.Y);

            double newRotation = Vector2.Angle(newUvector) * MathHelper.RadToDeg;
            double newObliqueAngle = Vector2.Angle(newVvector) * MathHelper.RadToDeg;

            if (mirrText)
            {
                if (Vector2.CrossProduct(newUvector, newVvector) < 0.0)
                {
                    newObliqueAngle = 90 - (newRotation - newObliqueAngle);
                    if (!(this.Alignment == TextAlignment.Fit || this.Alignment == TextAlignment.Aligned))
                    {
                        newRotation += 180;
                    }
                    this.IsBackward = !this.IsBackward;
                }
                else
                {
                    newObliqueAngle = 90 + (newRotation - newObliqueAngle);
                }              
            }
            else
            {
                if (Vector2.CrossProduct(newUvector, newVvector) < 0.0)
                {
                    newObliqueAngle = 90 - (newRotation - newObliqueAngle);

                    if (Vector2.DotProduct(newUvector, uv[0]) < 0.0)
                    {
                        newRotation += 180;

                        switch (this.Alignment)
                        {
                            case TextAlignment.TopLeft:
                                this.Alignment = TextAlignment.TopRight;
                                break;
                            case TextAlignment.TopRight:
                                this.Alignment = TextAlignment.TopLeft;
                                break;
                            case TextAlignment.MiddleLeft:
                                this.Alignment = TextAlignment.MiddleRight;
                                break;
                            case TextAlignment.MiddleRight:
                                this.Alignment = TextAlignment.MiddleLeft;
                                break;
                            case TextAlignment.BaselineLeft:
                                this.Alignment = TextAlignment.BaselineRight;
                                break;
                            case TextAlignment.BaselineRight:
                                this.Alignment = TextAlignment.BaselineLeft;
                                break;
                            case TextAlignment.BottomLeft:
                                this.Alignment = TextAlignment.BottomRight;
                                break;
                            case TextAlignment.BottomRight:
                                this.Alignment = TextAlignment.BottomLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (this.Alignment)
                        {
                            case TextAlignment.TopLeft:
                                this.Alignment = TextAlignment.BottomLeft;
                                break;
                            case TextAlignment.TopCenter:
                                this.Alignment = TextAlignment.BottomCenter;
                                break;
                            case TextAlignment.TopRight:
                                this.Alignment = TextAlignment.BottomRight;
                                break;
                            case TextAlignment.BottomLeft:
                                this.Alignment = TextAlignment.TopLeft;
                                break;
                            case TextAlignment.BottomCenter:
                                this.Alignment = TextAlignment.TopCenter;
                                break;
                            case TextAlignment.BottomRight:
                                this.Alignment = TextAlignment.TopRight;
                                break;
                        }
                    }
                }
                else
                {
                    newObliqueAngle = 90 + (newRotation - newObliqueAngle);
                }
            }

            // the oblique angle is defined between -85 and 85 degrees
            newObliqueAngle = MathHelper.NormalizeAngle(newObliqueAngle);
            if (newObliqueAngle > 180)
            {
                newObliqueAngle = 180 - newObliqueAngle;
            }

            if (newObliqueAngle < -85)
            {
                newObliqueAngle = -85;
            }
            else if (newObliqueAngle > 85)
            {
                newObliqueAngle = 85;
            }

            // the height must be greater than zero, the cos is always positive between -85 and 85
            double newHeight = newVvector.Modulus() * Math.Cos(newObliqueAngle * MathHelper.DegToRad);
            newHeight = MathHelper.IsZero(newHeight) ? MathHelper.Epsilon : newHeight;

            // the width factor is defined between 0.01 and 100
            double newWidthFactor = newUvector.Modulus() / newHeight;
            if (newWidthFactor < 0.01)
            {
                newWidthFactor = 0.01;
            }
            else if (newWidthFactor > 100)
            {
                newWidthFactor = 100;
            }

            this.Position = newPosition;
            this.Normal = newNormal;
            this.Rotation = newRotation;
            this.Height = newHeight;
            this.WidthFactor = newWidthFactor;
            this.ObliqueAngle = newObliqueAngle;
        }

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 4x4 transformation matrix.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <remarks>Matrix4 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public void TransformBy(Matrix4 transformation)
        {
            Matrix3 m = new Matrix3(transformation.M11, transformation.M12, transformation.M13,
                transformation.M21, transformation.M22, transformation.M23,
                transformation.M31, transformation.M32, transformation.M33);
            Vector3 v = new Vector3(transformation.M14, transformation.M24, transformation.M34);

            this.TransformBy(m, v);
        }

        #endregion

        #region overrides

        /// <summary>
        /// Creates a new Attribute that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Attribute that is a copy of this instance.</returns>
        public object Clone()
        {
            Attribute entity = new Attribute(this.tag)
            {
                //Attribute properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.isVisible,
                Definition = (AttributeDefinition) this.definition?.Clone(),
                Height = this.height,
                Width = this.width,
                WidthFactor = this.widthFactor,
                ObliqueAngle = this.obliqueAngle,
                Value = this.attValue,
                Style = (TextStyle) this.style.Clone(),
                Position = this.position,
                Flags = this.flags,
                Rotation = this.rotation,
                Alignment = this.alignment,
                IsBackward = this.isBackward,
                IsUpsideDown = this.isUpsideDown
            };

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData)data.Clone());
            }

            return entity;
        }

        #endregion
    }
}