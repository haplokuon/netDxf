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
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a multiline <see cref="EntityObject">entity</see>.
    /// </summary>
    public class MLine :
        EntityObject
    {
        #region delegates and events

        public delegate void MLineStyleChangedEventHandler(MLine sender, TableObjectChangedEventArgs<MLineStyle> e);
        public event MLineStyleChangedEventHandler MLineStyleChanged;
        protected virtual MLineStyle OnMLineStyleChangedEvent(MLineStyle oldMLineStyle, MLineStyle newMLineStyle)
        {
            MLineStyleChangedEventHandler ae = this.MLineStyleChanged;
            if (ae != null)
            {
                TableObjectChangedEventArgs<MLineStyle> eventArgs = new TableObjectChangedEventArgs<MLineStyle>(oldMLineStyle, newMLineStyle);
                ae(this, eventArgs);
                return eventArgs.NewValue;
            }
            return newMLineStyle;
        }

        #endregion

        #region private fields

        private double scale;
        private MLineStyle style;
        private MLineJustification justification;
        private double elevation;
        private MLineFlags flags;
        private readonly List<MLineVertex> vertexes;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        public MLine()
            : this(new List<Vector2>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">Multiline <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        public MLine(IEnumerable<Vector2> vertexes)
            : this(vertexes, MLineStyle.Default, 1.0, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">Multiline <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="isClosed">Sets if the multiline is closed  (default: false).</param>
        public MLine(IEnumerable<Vector2> vertexes, bool isClosed)
            : this(vertexes, MLineStyle.Default, 1.0, isClosed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">Multiline <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="scale">Multiline scale.</param>
        public MLine(IEnumerable<Vector2> vertexes, double scale)
            : this(vertexes, MLineStyle.Default, scale, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">Multiline <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="scale">Multiline scale.</param>
        /// <param name="isClosed">Sets if the multiline is closed (default: false).</param>
        public MLine(IEnumerable<Vector2> vertexes, double scale, bool isClosed)
            : this(vertexes, MLineStyle.Default, scale, isClosed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">MLine <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="style">MLine <see cref="MLineStyle">style.</see></param>
        /// <param name="scale">MLine scale.</param>
        public MLine(IEnumerable<Vector2> vertexes, MLineStyle style, double scale)
            : this(vertexes, style, scale, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">MLine <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="style">MLine <see cref="MLineStyle">style.</see></param>
        /// <param name="scale">MLine scale.</param>
        /// <param name="isClosed">Sets if the multiline is closed (default: false).</param>
        public MLine(IEnumerable<Vector2> vertexes, MLineStyle style, double scale, bool isClosed)
            : base(EntityType.MLine, DxfObjectCode.MLine)
        {
            this.scale = scale;
            if (style == null)
            {
                throw new ArgumentNullException(nameof(style));
            }

            if (isClosed)
            {
                this.flags = MLineFlags.Has | MLineFlags.Closed;
            }
            else
            {
                this.flags = MLineFlags.Has;
            }

            this.style = style;
            this.justification = MLineJustification.Zero;
            this.elevation = 0.0;
            if (vertexes == null)
            {
                throw new ArgumentNullException(nameof(vertexes));
            }
            this.vertexes = new List<MLineVertex>();
            foreach (Vector2 point in vertexes)
            {
                this.vertexes.Add(new MLineVertex(point, Vector2.Zero, Vector2.Zero, null));
            }
            this.Update();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the multiline <see cref="MLineVertex">vertexes</see> list.
        /// </summary>
        public List<MLineVertex> Vertexes
        {
            get { return this.vertexes; }
        }

        /// <summary>
        /// Gets or sets the multiline elevation.
        /// </summary>
        public double Elevation
        {
            get { return this.elevation; }
            set { this.elevation = value; }
        }

        /// <summary>
        /// Gets or sets the multiline scale.
        /// </summary>
        public double Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        /// <summary>
        /// Gets or sets if the multiline is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return this.flags.HasFlag(MLineFlags.Closed); }
            set
            {
                if (value)
                {
                    this.flags |= MLineFlags.Closed;
                }
                else
                {
                    this.flags &= ~MLineFlags.Closed;
                }
            }
        }

        /// <summary>
        /// Gets or sets the suppression of start caps.
        /// </summary>
        public bool NoStartCaps
        {
            get { return this.flags.HasFlag(MLineFlags.NoStartCaps); }
            set
            {
                if (value)
                {
                    this.flags |= MLineFlags.NoStartCaps;
                }
                else
                {
                    this.flags &= ~MLineFlags.NoStartCaps;
                }
            }
        }

        /// <summary>
        /// Gets or sets the suppression of end caps.
        /// </summary>
        public bool NoEndCaps
        {
            get { return this.flags.HasFlag(MLineFlags.NoEndCaps); }
            set
            {
                if (value)
                {
                    this.flags |= MLineFlags.NoEndCaps;
                }
                else
                {
                    this.flags &= ~MLineFlags.NoEndCaps;
                }
            }
        }

        /// <summary>
        /// Gets or sets the multiline justification.
        /// </summary>
        public MLineJustification Justification
        {
            get { return this.justification; }
            set { this.justification = value; }
        }

        /// <summary>
        /// Gets or set the multiline style.
        /// </summary>
        public MLineStyle Style
        {
            get { return this.style; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this.style = this.OnMLineStyleChangedEvent(this.style, value);
            }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// MLine flags.
        /// </summary>
        internal MLineFlags Flags
        {
            get { return this.flags; }
            set { this.flags = value; }
        }

        #endregion

        #region private methods

        private List<EntityObject> CreateRoundCap(Vector2 start, Vector2 end, Matrix3 transformation, AciColor color1, Linetype linetype1, AciColor color2, Linetype linetype2)
        {
            List<EntityObject> entities = new List<EntityObject>();

            Vector2 center = Vector2.MidPoint(start, end);
            double startAngle = Vector2.Angle(start - center) * MathHelper.RadToDeg;
            double endAngle = startAngle + 180.0;
            double radius = (start - center).Modulus();

            if (!MathHelper.IsZero(radius))
            {
                if (!color1.Equals(color2) || !linetype1.Equals(linetype2))
                {
                    double midAngle = startAngle + 90.0;
                    Arc arc1 = this.CreateArc(center, radius, startAngle, midAngle, color1, linetype1);
                    arc1.TransformBy(transformation, Vector3.Zero);
                    entities.Add(arc1);
                    Arc arc2 = this.CreateArc(center, radius, midAngle, endAngle, color2, linetype2);
                    arc2.TransformBy(transformation, Vector3.Zero);
                    entities.Add(arc2);
                }
                else
                {
                    Arc arc = this.CreateArc(center, radius, startAngle, endAngle, color1, linetype1);
                    arc.TransformBy(transformation, Vector3.Zero);
                    entities.Add(arc);
                }
            }

            return entities;
        }

        private List<EntityObject> CreateSquareCap(Vector2 start, Vector2 end, Matrix3 transformation, AciColor color1, Linetype linetype1, AciColor color2, Linetype linetype2)
        {
            List<EntityObject> entities = new List<EntityObject>();

            Vector2 midPoint = Vector2.MidPoint(start, end);

            if (!color1.Equals(color2) || !linetype1.Equals(linetype2))
            {
                Line line1 = this.CreateLine(start, midPoint, color1, linetype1);
                line1.TransformBy(transformation, Vector3.Zero);
                entities.Add(line1);
                Line line2 = this.CreateLine(midPoint, end, color2, linetype2);
                line2.TransformBy(transformation, Vector3.Zero);
                entities.Add(line2);
            }
            else
            {
                Line line = this.CreateLine(start, end, color1, linetype1);
                line.TransformBy(transformation, Vector3.Zero);
                entities.Add(line);
            }

            return entities;
        }

        private Line CreateLine(Vector2 start, Vector2 end, AciColor color, Linetype linetype)
        {
            return new Line(start, end)
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) linetype.Clone(),
                Color = (AciColor) color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal
            };
        }

        private Arc CreateArc(Vector2 center, double radius, double startAngle, double endAngle, AciColor color, Linetype linetype)
        {
            return new Arc(center, radius, startAngle, endAngle)
            {
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) linetype.Clone(),
                Color = (AciColor) color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
            };
        }

        #endregion

        #region public methods

        /// <summary>
        /// Calculates the internal information of the multiline vertexes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function needs to be called manually when any modification is done that affects the final shape of the multiline.
        /// </para>
        /// <para>
        /// If the vertex distance list needs to be edited to represent trimmed multilines this function needs to be called prior to any modification.
        /// It will calculate the minimum information needed to build a correct multiline.
        /// </para>
        /// </remarks>
        public void Update()
        {
            if (this.vertexes.Count == 0)
            {
                return;
            }

            double reference = 0.0;
            switch (this.justification)
            {
                case MLineJustification.Top:
                    reference = -this.style.Elements[0].Offset;
                    break;
                case MLineJustification.Zero:
                    reference = 0.0;
                    break;
                case MLineJustification.Bottom:
                    reference = -this.style.Elements[this.style.Elements.Count - 1].Offset;
                    break;
            }

            Vector2 prevDir;
            if (this.vertexes[0].Position.Equals(this.vertexes[this.vertexes.Count - 1].Position))
            {
                prevDir = Vector2.UnitY;
            }
            else
            {
                prevDir = this.vertexes[0].Position - this.vertexes[this.vertexes.Count - 1].Position;
                prevDir.Normalize();
            }

            for (int i = 0; i < this.vertexes.Count; i++)
            {
                Vector2 position = this.vertexes[i].Position;
                Vector2 miter;
                Vector2 dir;
                if (i == 0)
                {
                    if (this.vertexes[i + 1].Position.Equals(position))
                    {
                        dir = Vector2.UnitY;
                    }
                    else
                    {
                        dir = this.vertexes[i + 1].Position - position;
                        dir.Normalize();
                    }
                    if (this.IsClosed)
                    {
                        miter = dir - prevDir;
                        miter.Normalize();
                    }
                    else
                    {
                        miter = -MathHelper.Transform(dir, this.style.StartAngle*MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);
                        miter.Normalize();
                    }
                }
                else if (i + 1 == this.vertexes.Count)
                {
                    if (this.IsClosed)
                    {
                        if (this.vertexes[0].Position.Equals(position))
                        {
                            dir = Vector2.UnitY;
                        }
                        else
                        {
                            dir = this.vertexes[0].Position - position;
                            dir.Normalize();
                        }
                        miter = dir - prevDir;
                        miter.Normalize();
                    }
                    else
                    {
                        dir = prevDir;
                        miter = -MathHelper.Transform(dir, this.style.EndAngle*MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);
                        miter.Normalize();
                    }
                }
                else
                {
                    if (this.vertexes[i + 1].Position.Equals(position))
                    {
                        dir = Vector2.UnitY;
                    }
                    else
                    {
                        dir = this.vertexes[i + 1].Position - position;
                        dir.Normalize();
                    }

                    miter = dir - prevDir;
                    miter.Normalize();
                }
                prevDir = dir;

                List<double>[] distances = new List<double>[this.style.Elements.Count];
                double angleMiter = Vector2.Angle(miter);
                double angleDir = Vector2.Angle(dir);
                double cos = Math.Cos(angleMiter - (MathHelper.HalfPI + angleDir));
                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    double distance = (this.style.Elements[j].Offset + reference) / cos;
                    distances[j] = new List<double>
                    {
                        distance * this.scale,
                        0.0
                    };
                }

                this.vertexes[i] = new MLineVertex(position, dir, miter, distances);
            }
        }

        /// <summary>
        /// Decompose the actual multiline in its internal entities, <see cref="Line">lines</see> and <see cref="Arc">arcs</see>.
        /// </summary>
        /// <returns>A list of <see cref="Line">lines</see> and <see cref="Arc">arcs</see> that made up the multiline.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();

            Matrix3 transformation = MathHelper.ArbitraryAxis(this.Normal);

            // precomputed points at multiline vertexes for start and end caps calculations
            Vector2[][] cornerVertexes = new Vector2[this.vertexes.Count][];

            for (int i = 0; i < this.vertexes.Count; i++)
            {
                MLineVertex vertex = this.vertexes[i];
                MLineVertex nextVertex;

                if (this.IsClosed && i == this.vertexes.Count - 1)
                {
                    nextVertex = this.vertexes[0];
                }
                else if (!this.IsClosed && i == this.vertexes.Count - 1)
                {
                    continue;
                }
                else
                {
                    nextVertex = this.vertexes[i + 1];
                    cornerVertexes[i + 1] = new Vector2[this.style.Elements.Count];
                }

                cornerVertexes[i] = new Vector2[this.style.Elements.Count];

                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    if (vertex.Distances[j].Count == 0)
                    {
                        continue;
                    }

                    Vector2 refStart = vertex.Position + vertex.Miter * vertex.Distances[j][0];
                    cornerVertexes[i][j] = refStart;
                    for (int k = 1; k < vertex.Distances[j].Count; k++)
                    {
                        Vector2 start = refStart + vertex.Direction * vertex.Distances[j][k];
                        Vector2 end;
                        if (k >= vertex.Distances[j].Count - 1)
                        {
                            end = nextVertex.Position + nextVertex.Miter * nextVertex.Distances[j][0];                       
                            if(!this.IsClosed) cornerVertexes[i + 1][j] = end;
                        }
                        else
                        {
                            end = refStart + vertex.Direction * vertex.Distances[j][k + 1];
                            k++; // skip next segment it is a blank space
                        }

                        Line line = this.CreateLine(start, end, this.style.Elements[j].Color, this.style.Elements[j].Linetype);
                        line.TransformBy(transformation, Vector3.Zero);
                        entities.Add(line);
                    }
                }                
            }

            if (this.style.Flags.HasFlag(MLineStyleFlags.DisplayJoints))
            {
                AciColor color1 = this.style.Elements[0].Color;
                AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                Linetype linetype1 = this.style.Elements[0].Linetype;
                Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                for (int i = 0; i < cornerVertexes.Length; i++)
                {
                    if (!this.IsClosed && (i == 0 || i == cornerVertexes.Length - 1))
                    {
                        continue;
                    }

                    Vector2 start = cornerVertexes[i][0];
                    Vector2 end = cornerVertexes[i][cornerVertexes[0].Length - 1];

                    entities.AddRange(this.CreateSquareCap(start, end, transformation, color1, linetype1, color2, linetype2));
                }
            }

            // when the multiline is closed there are no caps
            if (this.IsClosed) return entities;

            if (!this.NoStartCaps)
            {
                if (this.style.Flags.HasFlag(MLineStyleFlags.StartRoundCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    Vector2 start = cornerVertexes[0][0];
                    Vector2 end = cornerVertexes[0][cornerVertexes[0].Length - 1];

                    entities.AddRange(this.scale >= 0 ?
                        this.CreateRoundCap(start, end, transformation, color1, linetype1, color2, linetype2) :
                        this.CreateRoundCap(end, start, transformation, color2, linetype2, color1, linetype1));
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.StartInnerArcsCap))
                {
                    int j = (int) (this.style.Elements.Count * 0.5); // Math.Floor

                    for (int i = 1; i < j; i++)
                    {
                        AciColor color1 = this.style.Elements[i].Color;
                        AciColor color2 = this.style.Elements[this.style.Elements.Count - 1 - i].Color;
                        Linetype linetype1 = this.style.Elements[i].Linetype;
                        Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1 - i].Linetype;

                        Vector2 start = cornerVertexes[0][i];
                        Vector2 end = cornerVertexes[0][cornerVertexes[0].Length - 1 - i];

                        entities.AddRange(this.scale >= 0 ?
                            this.CreateRoundCap(start, end, transformation, color1, linetype1, color2, linetype2) :
                            this.CreateRoundCap(end, start, transformation, color2, linetype2, color1, linetype1));
                    }
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.StartSquareCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    Vector2 start = cornerVertexes[0][0];
                    Vector2 end = cornerVertexes[0][cornerVertexes[0].Length - 1];

                    entities.AddRange(this.CreateSquareCap(start, end, transformation, color1, linetype1, color2, linetype2));
                }
            }

            if (!this.NoEndCaps)
            {
                if (this.style.Flags.HasFlag(MLineStyleFlags.EndRoundCap))
                {
                    AciColor color1 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    AciColor color2 = this.style.Elements[0].Color;
                    Linetype linetype1 = this.style.Elements[this.style.Elements.Count - 1].Linetype;
                    Linetype linetype2 = this.style.Elements[0].Linetype;
                   
                    Vector2 start = cornerVertexes[this.vertexes.Count - 1][cornerVertexes[0].Length - 1];
                    Vector2 end = cornerVertexes[this.vertexes.Count - 1][0];

                    entities.AddRange(this.scale >= 0 ?
                        this.CreateRoundCap(start, end, transformation, color1, linetype1, color2, linetype2) :
                        this.CreateRoundCap(end, start, transformation, color2, linetype2, color1, linetype1));
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.EndInnerArcsCap))
                {
                    int j = (int) (this.style.Elements.Count * 0.5); // Math.Floor

                    for (int i = 1; i < j; i++)
                    {
                        AciColor color1 = this.style.Elements[this.style.Elements.Count - 1 - i].Color;
                        AciColor color2 = this.style.Elements[i].Color;
                        Linetype linetype1 = this.style.Elements[this.style.Elements.Count - 1 - i].Linetype;
                        Linetype linetype2 = this.style.Elements[i].Linetype;

                        Vector2 start = cornerVertexes[this.vertexes.Count - 1][cornerVertexes[0].Length - 1 - i];
                        Vector2 end = cornerVertexes[this.vertexes.Count - 1][i];

                        entities.AddRange(this.scale >= 0 ?
                            this.CreateRoundCap(start, end, transformation, color1, linetype1, color2, linetype2) :
                            this.CreateRoundCap(end, start, transformation, color2, linetype2, color1, linetype1));
                    }
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.EndSquareCap))
                {
                    AciColor color1 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    AciColor color2 = this.style.Elements[0].Color;
                    Linetype linetype1 = this.style.Elements[this.style.Elements.Count - 1].Linetype;
                    Linetype linetype2 = this.style.Elements[0].Linetype;

                    Vector2 start = cornerVertexes[this.vertexes.Count - 1][cornerVertexes[0].Length - 1];
                    Vector2 end = cornerVertexes[this.vertexes.Count - 1][0];

                    entities.AddRange(this.CreateSquareCap(start, end, transformation, color1, linetype1, color2, linetype2));
                }
            }

            return entities;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>
        /// Non-uniform scaling is not supported for multilines.
        /// Explode the entity and, in case round end caps has been applied, convert the arcs into ellipse arcs and transform them instead.<br />
        /// Matrix3 adopts the convention of using column vectors to represent a transformation matrix.
        /// </remarks>
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

            Vector3 axis = transOW * Vector3.UnitX;
            axis = transformation * axis;
            axis = transWO * axis;
            Vector2 axisPoint = new Vector2(axis.X, axis.Y);
            double newScale = axisPoint.Modulus();

            for (int i = 0; i < this.Vertexes.Count; i++)
            {                
                Vector2 p = this.Vertexes[i].Position;
                Vector3 v = transOW * new Vector3(p.X, p.Y, this.Elevation);
                v = transformation * v + translation;
                v = transWO * v;
                Vector2 position = new Vector2(v.X, v.Y);
                newElevation = v.Z;

                Vector2 d = this.Vertexes[i].Direction;
                v = transOW * new Vector3(d.X, d.Y, 0.0);
                v = transformation * v;
                v = transWO * v;
                Vector2 direction = new Vector2(v.X, v.Y);

                Vector2 m = this.Vertexes[i].Miter;
                v = transOW * new Vector3(m.X, m.Y, 0.0);
                v = transformation * v;
                v = transWO * v;
                Vector2 miter = new Vector2(v.X, v.Y);

                List<double>[] newDistances = new List<double>[this.style.Elements.Count];
                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    newDistances[j] = new List<double>(); 
                    for (int k = 0; k < this.Vertexes[i].Distances[j].Count; k++)
                    {
                        newDistances[j].Add(this.Vertexes[i].Distances[j][k]*newScale);
                    }
                }
                this.vertexes[i] = new MLineVertex(position, direction, miter, newDistances);
            }

            if (Vector2.CrossProduct(this.Vertexes[0].Miter, this.Vertexes[0].Direction) < 0) newScale = -newScale;

            this.Elevation = newElevation;
            this.Normal = newNormal;
            this.Scale *= newScale;
        }

        /// <summary>
        /// Creates a new MLine that is a copy of the current instance.
        /// </summary>
        /// <returns>A new MLine that is a copy of this instance.</returns>
        public override object Clone()
        {
            MLine entity = new MLine
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
                //MLine properties
                Elevation = this.elevation,
                Scale = this.scale,
                Justification = this.justification,
                Style = (MLineStyle) this.style.Clone(),
                Flags = this.flags
            };

            foreach (MLineVertex vertex in this.vertexes)
            {
                entity.vertexes.Add((MLineVertex) vertex.Clone());
            }

            foreach (XData data in this.XData.Values)
            {
                entity.XData.Add((XData) data.Clone());
            }

            return entity;
        }

        #endregion
    }
}