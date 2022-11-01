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
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a leader <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Leader :
        EntityObject
    {
        #region delegates and events

        public delegate void LeaderStyleChangedEventHandler(Leader sender, TableObjectChangedEventArgs<DimensionStyle> e);
        public event LeaderStyleChangedEventHandler LeaderStyleChanged;
        protected virtual DimensionStyle OnDimensionStyleChangedEvent(DimensionStyle oldStyle, DimensionStyle newStyle)
        {
            LeaderStyleChangedEventHandler ae = this.LeaderStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<DimensionStyle> eventArgs = new TableObjectChangedEventArgs<DimensionStyle>(oldStyle, newStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newStyle;
        }

        public delegate void AnnotationAddedEventHandler(Leader sender, EntityChangeEventArgs e);
        public event AnnotationAddedEventHandler AnnotationAdded;
        protected virtual void OnAnnotationAddedEvent(EntityObject item)
        {
            AnnotationAddedEventHandler ae = this.AnnotationAdded;
            if (ae != null)
                ae(this, new EntityChangeEventArgs(item));
        }

        public delegate void AnnotationRemovedEventHandler(Leader sender, EntityChangeEventArgs e);
        public event AnnotationRemovedEventHandler AnnotationRemoved;
        protected virtual void OnAnnotationRemovedEvent(EntityObject item)
        {
            AnnotationRemovedEventHandler ae = this.AnnotationRemoved;
            if (ae != null)
                ae(this, new EntityChangeEventArgs(item));
        }
        
        #endregion

        #region delegates and events for style overrides

        public delegate void DimensionStyleOverrideAddedEventHandler(Leader sender, DimensionStyleOverrideChangeEventArgs e);
        public event DimensionStyleOverrideAddedEventHandler DimensionStyleOverrideAdded;
        protected virtual void OnDimensionStyleOverrideAddedEvent(DimensionStyleOverride item)
        {
            DimensionStyleOverrideAddedEventHandler ae = this.DimensionStyleOverrideAdded;
            if (ae != null)
                ae(this, new DimensionStyleOverrideChangeEventArgs(item));
        }

        public delegate void DimensionStyleOverrideRemovedEventHandler(Leader sender, DimensionStyleOverrideChangeEventArgs e);
        public event DimensionStyleOverrideRemovedEventHandler DimensionStyleOverrideRemoved;
        protected virtual void OnDimensionStyleOverrideRemovedEvent(DimensionStyleOverride item)
        {
            DimensionStyleOverrideRemovedEventHandler ae = this.DimensionStyleOverrideRemoved;
            if (ae != null)
                ae(this, new DimensionStyleOverrideChangeEventArgs(item));
        }

        #endregion

        #region private fields

        private DimensionStyle style;
        private bool showArrowhead;
        private LeaderPathType pathType;
        private readonly List<Vector2> vertexes;
        private EntityObject annotation;
        private bool hasHookline;
        private AciColor lineColor;
        private double elevation;
        private Vector2 offset;
        private Vector2 direction;
        private readonly DimensionStyleOverrideDictionary styleOverrides;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        public Leader(IEnumerable<Vector2> vertexes)
            : this(vertexes, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        /// <param name="style">Leader style.</param>
        public Leader(IEnumerable<Vector2> vertexes, DimensionStyle style)
            : this(vertexes, style, false)
        {
        }

        internal Leader(IEnumerable<Vector2> vertexes, DimensionStyle style, bool hasHookline)
            : base(EntityType.Leader, DxfObjectCode.Leader)
        {
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }

            this.vertexes = new List<Vector2>(vertexes);
            if (this.vertexes.Count < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(vertexes), this.vertexes.Count, "The leader vertexes list requires at least two points.");
            }

            this.style = style ?? throw new ArgumentNullException(nameof(style));
            this.hasHookline = hasHookline;
            this.showArrowhead = true;
            this.pathType = LeaderPathType.StraightLineSegments;
            this.annotation = null;
            this.lineColor = AciColor.ByLayer;
            this.elevation = 0.0;
            this.offset = Vector2.Zero;
            this.direction = Vector2.UnitX;
            this.styleOverrides = new DimensionStyleOverrideDictionary();
            this.styleOverrides.BeforeAddItem += this.StyleOverrides_BeforeAddItem;
            this.styleOverrides.AddItem += this.StyleOverrides_AddItem;
            this.styleOverrides.BeforeRemoveItem += this.StyleOverrides_BeforeRemoveItem;
            this.styleOverrides.RemoveItem += this.StyleOverrides_RemoveItem;
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="text">Leader text annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        public Leader(string text, IEnumerable<Vector2> vertexes)
            : this(text, vertexes, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="text">Leader text annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        /// <param name="style">Leader style.</param>
        public Leader(string text, IEnumerable<Vector2> vertexes, DimensionStyle style)
            : this(vertexes, style)
        {
            this.Annotation = this.BuildAnnotation(text);
            this.CalculateAnnotationDirection();

        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="tolerance">Leader tolerance annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        public Leader(ToleranceEntry tolerance, IEnumerable<Vector2> vertexes)
            : this(tolerance, vertexes, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="tolerance">Leader tolerance annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        /// <param name="style">Leader style.</param>
        public Leader(ToleranceEntry tolerance, IEnumerable<Vector2> vertexes, DimensionStyle style)
            : this(vertexes, style)
        {
            this.Annotation = this.BuildAnnotation(tolerance);
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="block">Leader block annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        public Leader(Block block, IEnumerable<Vector2> vertexes)
            : this(block, vertexes, DimensionStyle.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Leader</c> class.
        /// </summary>
        /// <param name="block">Leader block annotation.</param>
        /// <param name="vertexes">List of leader vertexes in local coordinates.</param>
        /// <param name="style">Leader style.</param>
        public Leader(Block block, IEnumerable<Vector2> vertexes, DimensionStyle style)
            : this(vertexes, style)
        {
            this.Annotation = this.BuildAnnotation(block);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the leader style.
        /// </summary>
        public DimensionStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                this.style = this.OnDimensionStyleChangedEvent(this.style, value);
            }
        }

        /// <summary>
        /// Gets the dimension style overrides list.
        /// </summary>
        /// <remarks>
        /// Any dimension style value stored in this list will override its corresponding value in the assigned style.
        /// </remarks>
        public DimensionStyleOverrideDictionary StyleOverrides
        {
            get { return this.styleOverrides; }
        }

        /// <summary>
        /// Gets or sets if the arrowhead is drawn.
        /// </summary>
        public bool ShowArrowhead
        {
            get { return this.showArrowhead; }
            set { this.showArrowhead = value; }
        }

        /// <summary>
        /// Gets or sets the way the leader is drawn.
        /// </summary>
        public LeaderPathType PathType
        {
            get { return this.pathType; }
            set { this.pathType = value; }
        }

        /// <summary>
        /// Gets the leader vertexes list in local coordinates.
        /// </summary>
        /// <remarks>
        /// The leader vertexes list must have at least two points.
        /// </remarks>
        public List<Vector2> Vertexes
        {
            get { return this.vertexes; }
        }

        /// <summary>
        /// Gets or sets the leader annotation entity.
        /// </summary>
        /// <remarks>
        /// Only MText, Text, Tolerance, and Insert entities are supported as a leader annotation.
        /// Even if AutoCad allows a Text entity to be part of a Leader it is not recommended, always use a MText entity instead.
        /// <br />
        /// Set the annotation property to null to create a Leader without annotation.
        /// </remarks>
        public EntityObject Annotation
        {
            get { return this.annotation; }
            set
            {
                if (value != null)
                {
                    if (!(value.Type == EntityType.MText ||
                          value.Type == EntityType.Text ||
                          value.Type == EntityType.Insert ||
                          value.Type == EntityType.Tolerance))
                    {
                        throw new ArgumentException("Only MText, Text, Insert, and Tolerance entities are supported as a leader annotation.", nameof(value));
                    }
                }

                // nothing else to do if it is the same
                if (ReferenceEquals(this.annotation, value))
                {
                    return;
                }

                // remove the previous annotation
                if (this.annotation != null)
                {
                    this.annotation.RemoveReactor(this);
                    this.OnAnnotationRemovedEvent(this.annotation);
                }

                // add the new annotation
                if (value != null)
                {
                    value.AddReactor(this);
                    this.OnAnnotationAddedEvent(value);
                }

                this.annotation = value;
            }
        }

        /// <summary>
        /// Gets or sets the leader hook position (last leader vertex).
        /// </summary>
        /// <remarks>
        /// This property allows easy access to the last leader vertex, aka leader hook position.
        /// </remarks>
        public Vector2 Hook
        {
            get { return this.vertexes[this.vertexes.Count - 1]; }
            set { this.vertexes[this.vertexes.Count - 1] = value; }
        }

        /// <summary>
        /// Gets if the leader has a hook line.
        /// </summary>
        /// <remarks>
        /// If set to true an additional vertex point (StartHookLine) will be created before the leader end point (hook).
        /// By default, only leaders with text annotation have hook lines.
        /// </remarks>
        public bool HasHookline
        {
            get { return this.hasHookline; }
            set
            {
                if (this.vertexes.Count < 2)
                {
                    throw new Exception("The leader vertexes list requires at least two points.");
                }

                if (this.hasHookline != value)
                {
                    if (value)
                    {
                        this.vertexes.Insert(this.vertexes.Count - 1, this.CalculateHookLine());
                    }
                    else
                    {
                        this.vertexes.RemoveAt(this.vertexes.Count - 2);
                    }
                }
                this.hasHookline = value;
            }
        }

        /// <summary>
        /// Gets or sets the leader line color if the style parameter DIMCLRD is set as BYBLOCK.
        /// </summary>
        public AciColor LineColor
        {
            get { return this.lineColor; }
            set
            {
                this.lineColor = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="Vector3">normal</see>.
        /// </summary>
        public new Vector3 Normal
        {
            get { return base.Normal; }
            set { base.Normal = value; }
        }

        /// <summary>
        /// Gets or sets the leader elevation.
        /// </summary>
        /// <remarks>This is the distance from the origin to the plane of the leader.</remarks>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        /// <summary>
        /// Gets or sets the offset from the last leader vertex (hook) to the annotation position.
        /// </summary>
        public Vector2 Offset
        {
            get { return this.offset; }
            set { this.offset = value; }
        }

        /// <summary>
        /// Gets or sets the leader annotation direction.
        /// </summary>
        public Vector2 Direction
        {
            get { return this.direction; }
            set { this.direction = Vector2.Normalize(value); }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Updates the leader entity to reflect the latest changes made to its properties.
        /// </summary>
        /// <param name="resetAnnotationPosition">
        /// If true the annotation position will be modified according to the position of the leader hook (last leader vertex),
        /// otherwise the leader hook will be moved according to the actual annotation position.
        /// </param>
        /// <remarks>
        /// This method should be manually called if the annotation position is modified, or the leader properties like Style, Annotation, TextVerticalPosition, and/or Offset.
        /// </remarks>
        public void Update(bool resetAnnotationPosition)
        {
            if (this.vertexes.Count < 2)
            {
                throw new Exception("The leader vertexes list requires at least two points.");
            }

            if (this.annotation == null)
            {
                return;
            }

            this.CalculateAnnotationDirection();

            if (resetAnnotationPosition)
            {
                this.ResetAnnotationPosition();
            }
            else
            {
                this.ResetHookPosition();
            }

            if (this.hasHookline)
            {
                Vector2 vertex = this.CalculateHookLine();
                this.vertexes[this.vertexes.Count - 2] = vertex;
            }
        }

        #endregion

        #region private methods

        private void CalculateAnnotationDirection()
        {
            double angle = 0.0;

                if (this.annotation != null)
                {
                    switch (this.annotation.Type)
                    {
                        case EntityType.MText:
                            MText mText = (MText) this.annotation;
                            angle = mText.Rotation;
                            switch (mText.AttachmentPoint)
                            {
                                case MTextAttachmentPoint.TopRight:
                                case MTextAttachmentPoint.MiddleRight:
                                case MTextAttachmentPoint.BottomRight:
                                    angle += 180.0;
                                    break;
                            }
                            break;
                        case EntityType.Text:
                            Text text = (Text) this.annotation;
                            angle = text.Rotation;
                            switch (text.Alignment)
                            {
                                case TextAlignment.TopRight:
                                case TextAlignment.MiddleRight:
                                case TextAlignment.BottomRight:
                                case TextAlignment.BaselineRight:
                                    angle += 180.0;
                                    break;
                            }
                            break;
                        case EntityType.Insert:
                            angle = ((Insert) this.annotation).Rotation;
                            break;
                        case EntityType.Tolerance:
                            angle = ((Tolerance) this.annotation).Rotation;
                            break;
                        default:
                            throw new ArgumentException("Only MText, Text, Insert, and Tolerance entities are supported as a leader annotation.", nameof(this.annotation));
                    }
                }
                this.direction = Vector2.Rotate(Vector2.UnitX, angle * MathHelper.DegToRad);
        }

        private Vector2 CalculateHookLine()
        {
            DimensionStyleOverride styleOverride;

            double dimScale = this.Style.DimScaleOverall;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimScaleOverall, out styleOverride))
            {
                dimScale = (double) styleOverride.Value;
            }

            double arrowSize = this.Style.ArrowSize;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ArrowSize, out styleOverride))
            {
                arrowSize = (double) styleOverride.Value;
            }

            return  this.Hook - this.Direction * arrowSize * dimScale;
        }

        /// <summary>
        /// Resets the leader hook position according to the annotation position.
        /// </summary>
        private void ResetHookPosition()
        {
            DimensionStyleOverride styleOverride;

            DimensionStyleTextVerticalPlacement textVerticalPlacement = this.Style.TextVerticalPlacement;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextVerticalPlacement, out styleOverride))
            {
                textVerticalPlacement = (DimensionStyleTextVerticalPlacement) styleOverride.Value;
            }

            double textGap = this.Style.TextOffset;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextOffset, out styleOverride))
            {
                textGap = (double) styleOverride.Value;
            }

            double dimScale = this.Style.DimScaleOverall;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimScaleOverall, out styleOverride))
            {
                dimScale = (double) styleOverride.Value;
            }

            double textHeight = this.Style.TextHeight;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextHeight, out styleOverride))
            {
                textHeight = (double) styleOverride.Value;
            }

            AciColor textColor = this.Style.TextColor;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextColor, out styleOverride))
            {
                textColor = (AciColor) styleOverride.Value;
            }

            Vector2 position;
            Vector2 textOffset;
            Vector2 dir = this.Direction;
            int side;
            textGap *= dimScale;

            switch (this.annotation.Type)
            {
                case EntityType.MText:
                    MText mText = (MText) this.annotation;
                    side = MathHelper.Sign(dir.X);
                    if(side == 0) side = MathHelper.Sign(dir.Y);
                    if (mText.Rotation > 90.0 && mText.Rotation <= 270.0) side *= -1;

                    if (side >= 0)
                    {
                        switch (mText.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.TopLeft;
                                break;
                            case MTextAttachmentPoint.MiddleRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.MiddleLeft;
                                break;
                            case MTextAttachmentPoint.BottomRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.BottomLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (mText.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.TopRight;
                                break;
                            case MTextAttachmentPoint.MiddleLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.MiddleRight;
                                break;
                            case MTextAttachmentPoint.BottomLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.BottomRight;
                                break;
                        }
                    }

                    textOffset = textVerticalPlacement == DimensionStyleTextVerticalPlacement.Centered ?
                        new Vector2(side * textGap, 0.0) :
                        new Vector2(side * textGap, textGap);

                    position = MathHelper.Transform(mText.Position, this.Normal, out _);
                    this.Hook = position - this.offset - Vector2.Rotate(textOffset, mText.Rotation * MathHelper.DegToRad);

                    mText.Height = textHeight * dimScale;
                    mText.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Text:
                    Text text = (Text) this.annotation;
                    side = MathHelper.Sign(dir.X);
                    if(side == 0) side = MathHelper.Sign(dir.Y);
                    if (text.Rotation > 90.0 && text.Rotation <= 270.0) side *= -1;

                    if (side >= 0)
                    {
                        switch (text.Alignment)
                        {
                            case TextAlignment.TopRight:
                                text.Alignment = TextAlignment.TopLeft;
                                break;
                            case TextAlignment.MiddleRight:
                                text.Alignment = TextAlignment.MiddleLeft;
                                break;
                            case TextAlignment.BottomRight:
                                text.Alignment = TextAlignment.BottomLeft;
                                break;
                            case TextAlignment.BaselineRight:
                                text.Alignment = TextAlignment.BaselineLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (text.Alignment)
                        {
                            case TextAlignment.TopLeft:
                                text.Alignment = TextAlignment.TopRight;
                                break;
                            case TextAlignment.MiddleLeft:
                                text.Alignment = TextAlignment.MiddleRight;
                                break;
                            case TextAlignment.BottomLeft:
                                text.Alignment = TextAlignment.BottomRight;
                                break;
                            case TextAlignment.BaselineLeft:
                                text.Alignment = TextAlignment.BaselineRight;
                                break;
                        }
                    }

                    textOffset = textVerticalPlacement == DimensionStyleTextVerticalPlacement.Centered ?
                        new Vector2(side * textGap, 0.0) :
                        new Vector2(side * textGap, textGap);

                    position = MathHelper.Transform(text.Position, this.Normal, out _);
                    this.Hook = position - this.offset - Vector2.Rotate(textOffset, text.Rotation * MathHelper.DegToRad);

                    text.Height = textHeight * dimScale;
                    text.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Insert:
                    Insert ins = (Insert) this.annotation;
                    position = MathHelper.Transform(ins.Position, this.Normal, out _);
                    this.Hook = position - this.offset;
                    ins.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Tolerance:
                    Tolerance tol = (Tolerance) this.annotation;
                    position = MathHelper.Transform(tol.Position, this.Normal, out _);
                    this.Hook = position - this.offset;
                    tol.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                default:
                    throw new Exception(string.Format("The entity type: {0} not supported as a leader annotation.", this.annotation.Type));
            }
        }

        /// <summary>
        /// Resets the annotation position according to the leader hook.
        /// </summary>
        private void ResetAnnotationPosition()
        {
            DimensionStyleOverride styleOverride;

            DimensionStyleTextVerticalPlacement textVerticalPlacement = this.Style.TextVerticalPlacement;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextVerticalPlacement, out styleOverride))
            {
                textVerticalPlacement = (DimensionStyleTextVerticalPlacement) styleOverride.Value;
            }

            double textGap = this.Style.TextOffset;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextOffset, out styleOverride))
            {
                textGap = (double) styleOverride.Value;
            }

            double dimScale = this.Style.DimScaleOverall;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimScaleOverall, out styleOverride))
            {
                dimScale = (double) styleOverride.Value;
            }

            double textHeight = this.Style.TextHeight;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextHeight, out styleOverride))
            {
                textHeight = (double) styleOverride.Value;
            }

            AciColor textColor = this.Style.TextColor;
            if (this.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextColor, out styleOverride))
            {
                textColor = (AciColor) styleOverride.Value;
            }

            Vector2 hook = this.Hook;
            Vector2 position;
            Vector2 textOffset;
            Vector2 dir = this.Direction;
            int side;
            textGap *= dimScale;

            switch (this.annotation.Type)
            {
                case EntityType.MText:
                    MText mText = (MText) this.annotation;
                    side = MathHelper.Sign(dir.X);
                    if(side == 0) side = MathHelper.Sign(dir.Y);
                    if (mText.Rotation > 90.0 && mText.Rotation <= 270.0) side *= -1;

                    if (side >= 0)
                    {
                        switch (mText.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.TopLeft;
                                break;
                            case MTextAttachmentPoint.MiddleRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.MiddleLeft;
                                break;
                            case MTextAttachmentPoint.BottomRight:
                                mText.AttachmentPoint = MTextAttachmentPoint.BottomLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (mText.AttachmentPoint)
                        {
                            case MTextAttachmentPoint.TopLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.TopRight;
                                break;
                            case MTextAttachmentPoint.MiddleLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.MiddleRight;
                                break;
                            case MTextAttachmentPoint.BottomLeft:
                                mText.AttachmentPoint = MTextAttachmentPoint.BottomRight;
                                break;
                        }
                    }

                    textOffset = textVerticalPlacement == DimensionStyleTextVerticalPlacement.Centered ?
                        new Vector2(side * textGap, 0.0) :
                        new Vector2(side * textGap, textGap);

                    position = hook + this.offset + Vector2.Rotate(textOffset, mText.Rotation * MathHelper.DegToRad);

                    mText.Position = MathHelper.Transform(position, this.Normal, this.elevation);
                    mText.Height = textHeight * dimScale;
                    mText.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Text:
                    Text text = (Text) this.annotation;
                    side = MathHelper.Sign(dir.X);
                    if(side == 0) side = MathHelper.Sign(dir.Y);
                    if (text.Rotation > 90.0 && text.Rotation <= 270.0) side *= -1;

                    if (side >= 0)
                    {
                        switch (text.Alignment)
                        {
                            case TextAlignment.TopRight:
                                text.Alignment = TextAlignment.TopLeft;
                                break;
                            case TextAlignment.MiddleRight:
                                text.Alignment = TextAlignment.MiddleLeft;
                                break;
                            case TextAlignment.BottomRight:
                                text.Alignment = TextAlignment.BottomLeft;
                                break;
                            case TextAlignment.BaselineRight:
                                text.Alignment = TextAlignment.BaselineLeft;
                                break;
                        }
                    }
                    else
                    {
                        switch (text.Alignment)
                        {
                            case TextAlignment.TopLeft:
                                text.Alignment = TextAlignment.TopRight;
                                break;
                            case TextAlignment.MiddleLeft:
                                text.Alignment = TextAlignment.MiddleRight;
                                break;
                            case TextAlignment.BottomLeft:
                                text.Alignment = TextAlignment.BottomRight;
                                break;
                            case TextAlignment.BaselineLeft:
                                text.Alignment = TextAlignment.BaselineRight;
                                break;
                        }
                    }

                    textOffset = textVerticalPlacement == DimensionStyleTextVerticalPlacement.Centered ?
                        new Vector2(side * textGap, 0.0) :
                        new Vector2(side * textGap, textGap);

                    position = hook + this.offset + Vector2.Rotate(textOffset, text.Rotation * MathHelper.DegToRad);
                    text.Position = MathHelper.Transform(position, this.Normal, this.elevation);
                    text.Height = textHeight * dimScale;
                    text.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Insert:
                    Insert ins = (Insert) this.annotation;
                    position = hook + this.offset;
                    ins.Position = MathHelper.Transform(position, this.Normal, this.elevation);
                    ins.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                case EntityType.Tolerance:
                    Tolerance tol = (Tolerance) this.annotation;
                    position = hook + this.offset;
                    tol.Position = MathHelper.Transform(position, this.Normal, this.elevation);
                    tol.Color = textColor.IsByBlock ? AciColor.ByLayer : textColor;
                    break;

                default:
                    throw new Exception(string.Format("The entity type: {0} not supported as a leader annotation.", this.annotation.Type));
            }
        }

        private MText BuildAnnotation(string text)
        {
            int side = Math.Sign(this.vertexes[this.vertexes.Count - 1].X - this.vertexes[this.vertexes.Count - 2].X);
            MTextAttachmentPoint attachment;
            Vector2 textOffset;
            if (this.style.TextVerticalPlacement == DimensionStyleTextVerticalPlacement.Centered)
            {
                textOffset = new Vector2(side * this.style.TextOffset * this.style.DimScaleOverall, 0.0);
                attachment = side >= 0 ? MTextAttachmentPoint.MiddleLeft : MTextAttachmentPoint.MiddleRight;
            }
            else
            {
                textOffset = new Vector2(side * this.style.TextOffset * this.style.DimScaleOverall, this.style.TextOffset * this.style.DimScaleOverall);
                attachment = side >= 0 ? MTextAttachmentPoint.BottomLeft : MTextAttachmentPoint.BottomRight;
            }

            Vector2 position = this.Hook + textOffset;
            Vector3 mTextPosition = MathHelper.Transform(position, this.Normal, this.elevation);
            MText entity = new MText(text, mTextPosition, this.style.TextHeight * this.style.DimScaleOverall, 0.0, this.style.TextStyle)
            {
                Color = this.style.TextColor.IsByBlock ? AciColor.ByLayer : this.style.TextColor,
                AttachmentPoint = attachment
            };

            if (!MathHelper.IsZero(this.vertexes[this.vertexes.Count - 1].Y - this.vertexes[this.vertexes.Count - 2].Y))
            {
                this.HasHookline = true;
            }

            return entity;
        }

        private Insert BuildAnnotation(Block block)
        {
            return new Insert(block, this.vertexes[this.vertexes.Count - 1])
            {
                Color = this.style.TextColor.IsByBlock ? AciColor.ByLayer : this.style.TextColor
            };
        }

        private Tolerance BuildAnnotation(ToleranceEntry tolerance)
        {
            return new Tolerance(tolerance, this.vertexes[this.vertexes.Count - 1])
            {
                Color = this.style.TextColor.IsByBlock ? AciColor.ByLayer : this.style.TextColor,
                Style = this.style
            };
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal))
            {
                newNormal = this.Normal;
            }
            double newElevation = this.Elevation;

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            for (int i = 0; i < this.Vertexes.Count; i++)
            {
                Vector3 v = transOW * new Vector3(this.Vertexes[i].X, this.Vertexes[i].Y, this.Elevation);
                v = transformation * v + translation;
                v = transWO * v;
                this.Vertexes[i] = new Vector2(v.X, v.Y);
                newElevation = v.Z;
            }

            Vector3 newOffset = transOW * new Vector3(this.Offset.X, this.Offset.Y, this.Elevation);
            newOffset = transformation * newOffset;
            newOffset = transWO * newOffset;
            this.Offset = new Vector2(newOffset.X, newOffset.Y);

            this.Elevation = newElevation;
            this.Normal = newNormal;

            this.annotation?.TransformBy(transformation, translation);
        }

        /// <summary>
        /// Creates a new Leader that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Leader that is a copy of this instance.</returns>
        public override object Clone()
        {
            Leader entity = new Leader(this.vertexes)
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
                //Leader properties
                Elevation = this.elevation,
                Style = (DimensionStyle) this.style.Clone(),
                ShowArrowhead = this.showArrowhead,
                PathType = this.pathType,
                LineColor = this.lineColor,
                Annotation = (EntityObject) this.annotation?.Clone(),
                Offset = this.offset,
                hasHookline = this.hasHookline
            };

            foreach (DimensionStyleOverride styleOverride in this.StyleOverrides.Values)
            {
                object copy = styleOverride.Value is ICloneable value ? value.Clone() : styleOverride.Value;
                entity.StyleOverrides.Add(new DimensionStyleOverride(styleOverride.Type, copy));
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion

        #region Dimension style overrides events

        private void StyleOverrides_BeforeAddItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            if (sender.TryGetValue(e.Item.Type, out DimensionStyleOverride old))
            {
                if (ReferenceEquals(old.Value, e.Item.Value))
                {
                    e.Cancel = true;
                }
            }
        }

        private void StyleOverrides_AddItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            this.OnDimensionStyleOverrideAddedEvent(e.Item);
        }

        private void StyleOverrides_BeforeRemoveItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
        }

        private void StyleOverrides_RemoveItem(DimensionStyleOverrideDictionary sender, DimensionStyleOverrideDictionaryEventArgs e)
        {
            this.OnDimensionStyleOverrideRemovedEvent(e.Item);
        }

        #endregion
    }
}