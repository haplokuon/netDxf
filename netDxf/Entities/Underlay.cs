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
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents an underlay <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Underlay :
        EntityObject
    {
        #region delegates and events

        public delegate void UnderlayDefinitionChangedEventHandler(Underlay sender, TableObjectChangedEventArgs<UnderlayDefinition> e);
        public event UnderlayDefinitionChangedEventHandler UnderlayDefinitionChanged;
        protected virtual UnderlayDefinition OnUnderlayDefinitionChangedEvent(UnderlayDefinition oldUnderlayDefinition, UnderlayDefinition newUnderlayDefinition)
        {
            UnderlayDefinitionChangedEventHandler ae = this.UnderlayDefinitionChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<UnderlayDefinition> eventArgs = new TableObjectChangedEventArgs<UnderlayDefinition>(oldUnderlayDefinition, newUnderlayDefinition);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newUnderlayDefinition;
        }

        #endregion

        #region private fields

        private UnderlayDefinition definition;
        private Vector3 position;
        private Vector2 scale;
        private double rotation;
        private short contrast;
        private short fade;
        private UnderlayDisplayFlags displayOptions;
        private ClippingBoundary clippingBoundary;

        #endregion

        #region constructor

        internal Underlay()
            : base(EntityType.Underlay, DxfObjectCode.Underlay)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Underlay</c> class.
        /// </summary>
        /// <param name="definition"><see cref="UnderlayDefinition">Underlay definition</see>.</param>
        public Underlay(UnderlayDefinition definition)
            : this(definition, Vector3.Zero, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Underlay</c> class.
        /// </summary>
        /// <param name="definition"><see cref="UnderlayDefinition">Underlay definition</see>.</param>
        /// <param name="position">Underlay <see cref="Vector3">position</see> in world coordinates.</param>
        public Underlay(UnderlayDefinition definition, Vector3 position)
            : this(definition, position, 1.0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Underlay</c> class.
        /// </summary>
        /// <param name="definition"><see cref="UnderlayDefinition">Underlay definition</see>.</param>
        /// <param name="position">Underlay <see cref="Vector3">position</see> in world coordinates.</param>
        /// <param name="scale">Underlay scale.</param>
        public Underlay(UnderlayDefinition definition, Vector3 position, double scale)
            : base(EntityType.Underlay, DxfObjectCode.Underlay)
        {
            this.definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.position = position;
            if (scale <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), scale, "The Underlay scale must be greater than zero.");
            }
            this.scale = new Vector2(scale);
            this.rotation = 0.0;
            this.contrast = 100;
            this.fade = 0;
            this.displayOptions = UnderlayDisplayFlags.ShowUnderlay;
            this.clippingBoundary = null;
            switch (this.definition.Type)
            {
                case UnderlayType.DGN:
                    this.CodeName = DxfObjectCode.UnderlayDgn;
                    break;
                case UnderlayType.DWF:
                    this.CodeName = DxfObjectCode.UnderlayDwf;
                    break;
                case UnderlayType.PDF:
                    this.CodeName = DxfObjectCode.UnderlayPdf;
                    break;
            }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the underlay definition.
        /// </summary>
        public UnderlayDefinition Definition
        {
            get { return this.definition; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.definition = this.OnUnderlayDefinitionChangedEvent(this.definition, value);

                switch (value.Type)
                {
                    case UnderlayType.DGN:
                        this.CodeName = DxfObjectCode.UnderlayDgn;
                        break;
                    case UnderlayType.DWF:
                        this.CodeName = DxfObjectCode.UnderlayDwf;
                        break;
                    case UnderlayType.PDF:
                        this.CodeName = DxfObjectCode.UnderlayPdf;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets or sets the underlay position in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the underlay scale.
        /// </summary>
        /// <remarks>
        /// Any of the vector scale components cannot be zero.<br />
        /// Even thought the DXF has a code for the Z scale it seems that it has no use.
        /// The X and Y components multiplied by the original size of the PDF page represent the width and height of the final underlay.
        /// The Z component even thought it is present in the DXF it seems it has no use.
        /// </remarks>
        public Vector2 Scale
        {
            get { return this.scale; }
            set
            {
                if (MathHelper.IsZero(value.X) || MathHelper.IsZero(value.Y))
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Any of the vector scale components cannot be zero.");
                }
                this.scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the underlay rotation around its normal.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets the underlay contrast.
        /// </summary>
        /// <remarks>Valid values range from 20 to 100.</remarks>
        public short Contrast
        {
            get { return this.contrast; }
            set
            {
                if (value < 20 || value > 100)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted contrast values range from 20 to 100.");
                }
                this.contrast = value;
            }
        }

        /// <summary>
        /// Gets or sets the underlay fade.
        /// </summary>
        /// <remarks>Valid values range from 0 to 80.</remarks>
        public short Fade
        {
            get { return this.fade; }
            set
            {
                if (value < 0 || value > 80)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted fade values range from 0 to 80.");
                }
                this.fade = value;
            }
        }

        /// <summary>
        /// Gets or sets the underlay display options.
        /// </summary>
        public UnderlayDisplayFlags DisplayOptions
        {
            get { return this.displayOptions; }
            set { this.displayOptions = value; }
        }

        /// <summary>
        /// Gets or sets the underlay clipping boundary.
        /// </summary>
        /// <remarks>
        /// Set as null to restore the default clipping boundary, show the full underlay without clipping.
        /// </remarks>
        public ClippingBoundary ClippingBoundary
        {
            get { return this.clippingBoundary; }
            set { this.clippingBoundary = value; }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform scaling for rotated underlays is not supported.
        /// This is not a limitation of the code but the DXF format, unlike the Image there is no way to define the local UV vectors.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
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
                    this.Scale.X * Vector2.UnitX,
                    this.Scale.Y * Vector2.UnitY
                },
                this.rotation * MathHelper.DegToRad,
                CoordinateSystem.Object, CoordinateSystem.World);

            Vector3 v;
            v = transOW * new Vector3(uv[0].X , uv[0].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newUvector = new Vector2(v.X, v.Y);

            v = transOW * new Vector3(uv[1].X, uv[1].Y, 0.0);
            v = transformation * v;
            v = transWO * v;
            Vector2 newVvector = new Vector2(v.X, v.Y);

            int sign = Math.Sign(transformation.M11 * transformation.M22 * transformation.M33) < 0 ? -1 : 1;

            double scaleX = sign * newUvector.Modulus();
            scaleX = MathHelper.IsZero(scaleX) ? MathHelper.Epsilon : scaleX;
            double scaleY = newVvector.Modulus();
            scaleY = MathHelper.IsZero(scaleY) ? MathHelper.Epsilon : scaleY;

            Vector2 newScale = new Vector2(scaleX, scaleY);
            double newRotation = Vector2.Angle(sign * newUvector) * MathHelper.RadToDeg;

            this.Position = newPosition;
            this.Normal = newNormal;
            this.Rotation = newRotation;
            this.Scale = newScale;           
        }

        /// <summary>
        /// Creates a new Underlay that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Underlay that is a copy of this instance.</returns>
        public override object Clone()
        {
            Underlay entity = new Underlay
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Underlay properties
                Definition = (UnderlayDefinition) this.definition.Clone(),
                Position = this.position,
                Scale = this.scale,
                Rotation = this.rotation,
                Contrast = this.contrast,
                Fade = this.fade,
                DisplayOptions = this.displayOptions,
                ClippingBoundary = this.clippingBoundary != null ? (ClippingBoundary) this.clippingBoundary.Clone() : null
            };

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}