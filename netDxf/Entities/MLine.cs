#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a multiline <see cref="T:netDxf.Entities.EntityObject">entity</see>.
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
        /// <param name="isClosed">Sets if the multiline is closed  (default: false).</param>
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
        /// <param name="isClosed">Sets if the multiline is closed  (default: false).</param>
        public MLine(IEnumerable<Vector2> vertexes, MLineStyle style, double scale, bool isClosed)
            : base(EntityType.MLine, DxfObjectCode.MLine)
        {
            this.scale = scale;
            if (style == null)
                throw new ArgumentNullException(nameof(style));
            if (isClosed)
                this.flags = MLineFlags.Has | MLineFlags.Closed;
            else
                this.flags = MLineFlags.Has;

            this.style = style;
            this.justification = MLineJustification.Zero;
            this.elevation = 0.0;
            if (vertexes == null)
                throw new ArgumentNullException(nameof(vertexes));
            this.vertexes = new List<MLineVertex>();
            foreach (Vector2 point in vertexes)
                this.vertexes.Add(new MLineVertex(point, Vector2.Zero, Vector2.Zero, null));
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
        /// <remarks>AutoCad accepts negative scales, but it is not recommended.</remarks>
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
                    this.flags |= MLineFlags.Closed;
                else
                    this.flags &= ~MLineFlags.Closed;
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
                    this.flags |= MLineFlags.NoStartCaps;
                else
                    this.flags &= ~MLineFlags.NoStartCaps;
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
                    this.flags |= MLineFlags.NoEndCaps;
                else
                    this.flags &= ~MLineFlags.NoEndCaps;
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
                    throw new ArgumentNullException(nameof(value));
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
                return;

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
                prevDir = Vector2.UnitY;
            else
            {
                prevDir = this.vertexes[0].Position - this.vertexes[this.vertexes.Count - 1].Position;
                prevDir.Normalize();
            }

            for (int i = 0; i < this.vertexes.Count; i++)
            {
                Vector2 position = this.vertexes[i].Position;
                Vector2 mitter;
                Vector2 dir;
                if (i == 0)
                {
                    if (this.vertexes[i + 1].Position.Equals(position))
                        dir = Vector2.UnitY;
                    else
                    {
                        dir = this.vertexes[i + 1].Position - position;
                        dir.Normalize();
                    }
                    if (this.IsClosed)
                    {
                        mitter = prevDir - dir;
                        mitter.Normalize();
                    }
                    else
                    {
                        mitter = MathHelper.Transform(dir, this.style.StartAngle*MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);
                        mitter.Normalize();
                    }
                }
                else if (i + 1 == this.vertexes.Count)
                {
                    if (this.IsClosed)
                    {
                        if (this.vertexes[0].Position.Equals(position))
                            dir = Vector2.UnitY;
                        else
                        {
                            dir = this.vertexes[0].Position - position;
                            dir.Normalize();
                        }
                        mitter = prevDir - dir;
                        mitter.Normalize();
                    }
                    else
                    {
                        dir = prevDir;
                        mitter = MathHelper.Transform(dir, this.style.EndAngle*MathHelper.DegToRad, CoordinateSystem.Object, CoordinateSystem.World);
                        mitter.Normalize();
                    }
                }
                else
                {
                    if (this.vertexes[i + 1].Position.Equals(position))
                        dir = Vector2.UnitY;
                    else
                    {
                        dir = this.vertexes[i + 1].Position - position;
                        dir.Normalize();
                    }

                    mitter = prevDir - dir;
                    mitter.Normalize();
                }
                prevDir = dir;

                List<double>[] distances = new List<double>[this.style.Elements.Count];
                double angleMitter = Vector2.Angle(mitter);
                double angleDir = Vector2.Angle(dir);
                double cos = Math.Cos(angleMitter + (MathHelper.HalfPI - angleDir));
                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    double distance = (this.style.Elements[j].Offset + reference)/cos;
                    distances[j] = new List<double>
                    {
                        distance*this.scale,
                        0.0
                    };
                }

                this.vertexes[i] = new MLineVertex(position, dir, -mitter, distances);
            }
        }

        /// <summary>
        /// Decompose the actual multiline in its internal entities, <see cref="Line">lines</see> and <see cref="Arc">arcs</see>.
        /// </summary>
        /// <returns>A list of <see cref="Line">lines</see> and <see cref="Arc">arcs</see> that made up the multiline.</returns>
        /// <exception cref="InvalidOperationException">An exception will be thrown if the number of distances for a given MLineStyleElement is not an even number.</exception>
        public List<EntityObject> Explode()
        {
            Matrix3 transformation = MathHelper.ArbitraryAxis(this.Normal);

            List<EntityObject> entities = new List<EntityObject>();

            // precomputed points at mline vertexes for start and end caps calculations
            Vector2[][] cornerVextexes = new Vector2[this.vertexes.Count][];

            for (int i = 0; i < this.vertexes.Count; i++)
            {

                MLineVertex vertex = this.vertexes[i];
                MLineVertex nextVertex;

                if (this.IsClosed && i==this.vertexes.Count - 1)
                    nextVertex = this.vertexes[0];
                else if (!this.IsClosed && i == this.vertexes.Count - 1)
                    continue;
                else
                {
                    nextVertex = this.vertexes[i + 1];
                    cornerVextexes[i + 1] = new Vector2[this.style.Elements.Count];
                }

                cornerVextexes[i] = new Vector2[this.style.Elements.Count];

                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    if (this.style.Elements.Count % 2 != 0)
                        throw new InvalidOperationException("The number of distances for a given MLineStyleElement must be an even number.");

                    Vector2 refStart = vertex.Position + vertex.Miter * vertex.Distances[j][0];
                    cornerVextexes[i][j] = refStart;
                    for (int k = 1; k < vertex.Distances[j].Count; k++)
                    {
                        Vector2 start = refStart + vertex.Direction * vertex.Distances[j][k];
                        Vector2 end;
                        if (k >= vertex.Distances[j].Count - 1)
                        {
                            end = nextVertex.Position + nextVertex.Miter * nextVertex.Distances[j][0];                       
                            if(!this.IsClosed) cornerVextexes[i + 1][j] = end;
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

                bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                for (int i = 0; i < cornerVextexes.Length; i++)
                {
                    if (!this.IsClosed && (i == 0 || i == cornerVextexes.Length - 1)) continue;

                    Vector2 start = cornerVextexes[i][0];
                    Vector2 end = cornerVextexes[i][cornerVextexes[0].Length - 1];
                    Vector2 midPoint = Vector2.MidPoint(start, end);
                    if (trim)
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
                }
            }

            // when the mline is closed there are no caps
            if (this.IsClosed) return entities;

            if (!this.NoStartCaps)
            {
                if (this.style.Flags.HasFlag(MLineStyleFlags.StartRoundCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                    Vector2 center = Vector2.MidPoint(cornerVextexes[0][0], cornerVextexes[0][cornerVextexes.Length - 1]);
                    Vector2 start = cornerVextexes[0][0];
                    //Vector2 end = cornerVextexes[0][cornerVextexes[0].Length - 1];

                    double startAngle = Vector2.Angle(start - center) * MathHelper.RadToDeg;
                    //double endAngle = Vector2.Angle(end - center) * MathHelper.RadToDeg;
                    double endAngle = startAngle + 180.0;
                    double radius = (start - center).Modulus();

                    if (trim)
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

                if (this.style.Flags.HasFlag(MLineStyleFlags.StartInnerArcsCap))
                {
                    Vector2 center = Vector2.MidPoint(cornerVextexes[0][0], cornerVextexes[0][cornerVextexes.Length - 1]); ;

                    int j = (int) Math.Floor(this.style.Elements.Count / 2.0);

                    for (int i = 1; i < j; i++)
                    {
                        AciColor color1 = this.style.Elements[i].Color;
                        AciColor color2 = this.style.Elements[this.style.Elements.Count - 1 - i].Color;
                        Linetype linetype1 = this.style.Elements[i].Linetype;
                        Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1 - i].Linetype;

                        bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                        Vector2 start = cornerVextexes[0][i];
                        //Vector2 end = cornerVextexes[0][cornerVextexes[0].Length - 1 - i];

                        double startAngle = Vector2.Angle(start - center) * MathHelper.RadToDeg;
                        //double endAngle = Vector2.Angle(end - center) * MathHelper.RadToDeg;
                        double endAngle = startAngle + 180.0;
                        double radius = (start - center).Modulus();

                        if (trim)
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
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.StartSquareCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                    Vector2 start = cornerVextexes[0][0];
                    Vector2 end = cornerVextexes[0][cornerVextexes[0].Length - 1];
                    Vector2 midPoint = Vector2.MidPoint(start, end);

                    if (trim)
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
                }
            }

            if (!this.NoEndCaps)
            {
                if (this.style.Flags.HasFlag(MLineStyleFlags.EndRoundCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                    Vector2 center = Vector2.MidPoint(cornerVextexes[this.vertexes.Count - 1][0], cornerVextexes[this.vertexes.Count - 1][cornerVextexes.Length - 1]);
                    Vector2 start = cornerVextexes[this.vertexes.Count - 1][cornerVextexes[0].Length - 1];
                    //Vector2 end = cornerVextexes[this.vertexes.Count - 1][0];

                    double startAngle = Vector2.Angle(start - center) * MathHelper.RadToDeg;
                    //double endAngle = Vector2.Angle(end - center) * MathHelper.RadToDeg;
                    double endAngle = startAngle + 180.0;
                    double radius = (start - center).Modulus();

                    if (trim)
                    {
                        double midAngle = startAngle + 90.0;

                        Arc arc1 = this.CreateArc(center, radius, midAngle, endAngle, color1, linetype1);
                        arc1.TransformBy(transformation, Vector3.Zero);
                        entities.Add(arc1);

                        Arc arc2 = this.CreateArc(center, radius, startAngle, midAngle, color2, linetype2);
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

                if (this.style.Flags.HasFlag(MLineStyleFlags.EndInnerArcsCap))
                {
                    Vector2 center = Vector2.MidPoint(cornerVextexes[this.vertexes.Count - 1][0], cornerVextexes[this.vertexes.Count - 1][cornerVextexes.Length - 1]);

                    int j = (int)Math.Floor(this.style.Elements.Count / 2.0);
                    for (int i = 1; i < j; i++)
                    {
                        AciColor color1 = this.style.Elements[i].Color;
                        AciColor color2 = this.style.Elements[this.style.Elements.Count - 1 - i].Color;
                        Linetype linetype1 = this.style.Elements[i].Linetype;
                        Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1 - i].Linetype;

                        bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                        Vector2 start = cornerVextexes[this.vertexes.Count - 1][cornerVextexes[0].Length - 1 - i];
                        //Vector2 end = cornerVextexes[this.vertexes.Count - 1][i];

                        double startAngle = Vector2.Angle(start - center) * MathHelper.RadToDeg;
                        //double endAngle = Vector2.Angle(end - center) * MathHelper.RadToDeg;
                        double endAngle = startAngle + 180.0;
                        double radius = (start - center).Modulus();

                        if (trim)
                        {
                            double midAngle = startAngle + 90.0;

                            Arc arc1 = this.CreateArc(center, radius, midAngle, endAngle, color1, linetype1);
                            arc1.TransformBy(transformation, Vector3.Zero);
                            entities.Add(arc1);

                            Arc arc2 = this.CreateArc(center, radius, startAngle, midAngle, color2, linetype2);
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
                }

                if (this.style.Flags.HasFlag(MLineStyleFlags.EndSquareCap))
                {
                    AciColor color1 = this.style.Elements[0].Color;
                    AciColor color2 = this.style.Elements[this.style.Elements.Count - 1].Color;
                    Linetype linetype1 = this.style.Elements[0].Linetype;
                    Linetype linetype2 = this.style.Elements[this.style.Elements.Count - 1].Linetype;

                    bool trim = !color1.Equals(color2) || !linetype1.Equals(linetype2);

                    Vector2 start = cornerVextexes[this.vertexes.Count - 1][cornerVextexes[0].Length - 1];
                    Vector2 end = cornerVextexes[this.vertexes.Count - 1][0];
                    Vector2 midPoint = Vector2.MidPoint(start, end);

                    if (trim)
                    {
                        Line line1 = this.CreateLine(midPoint, end, color1, linetype1);
                        line1.TransformBy(transformation, Vector3.Zero);
                        entities.Add(line1);

                        Line line2 = this.CreateLine(start, midPoint, color2, linetype2);
                        line2.TransformBy(transformation, Vector3.Zero);
                        entities.Add(line2);
                    }
                    else
                    {
                        Line line = this.CreateLine(start, end, color1, linetype1);
                        line.TransformBy(transformation, Vector3.Zero);
                        entities.Add(line);
                    }
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
        /// Explode the entity and, in case round end caps has been applied, convert the arcs into ellipse arcs and transform them instead.
        /// </remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newNormal;
            double newElevation;
            double newScale;

            newNormal = transformation * this.Normal;
            newElevation = this.Elevation;
            newScale = newNormal.Modulus();

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal).Transpose();

            for (int i = 0; i < this.Vertexes.Count; i++)
            {
                Vector2 position;
                Vector2 direction;
                Vector2 mitter;

                Vector3 v = transOW * new Vector3(this.Vertexes[i].Position.X, this.Vertexes[i].Position.Y, this.Elevation);
                v = transformation * v + translation;
                v = transWO * v;
                position = new Vector2(v.X, v.Y);
                newElevation = v.Z;

                v = transOW * new Vector3(this.Vertexes[i].Direction.X, this.Vertexes[i].Direction.Y, this.Elevation);
                v = transformation * v;
                v = transWO * v;
                direction = new Vector2(v.X, v.Y);

                v = transOW * new Vector3(this.Vertexes[i].Miter.X, this.Vertexes[i].Miter.Y, this.Elevation);
                v = transformation * v;
                v = transWO * v;
                mitter = new Vector2(v.X, v.Y);

                List<double>[] newDistances = new List<double>[this.style.Elements.Count];
                for (int j = 0; j < this.style.Elements.Count; j++)
                {
                    newDistances[j] = new List<double>(); 
                    for (int k = 0; k < this.Vertexes[i].Distances[j].Count; k++)
                    {
                        newDistances[j].Add(this.Vertexes[i].Distances[j][k]*newScale);
                    }
                }
                this.vertexes[i] = new MLineVertex(position, direction, mitter, newDistances);
            }

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
                entity.vertexes.Add((MLineVertex) vertex.Clone());

            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}