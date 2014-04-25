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
using System.Collections.Generic;
using netDxf.Objects;

namespace netDxf.Entities
{
    /// <summary>
    /// Justification.
    /// </summary>
    public enum MLineJustification
    {
        /// <summary>
        /// Top.
        /// </summary>
        Top = 0,
        /// <summary>
        /// Zero.
        /// </summary>
        Zero = 1,
        /// <summary>
        /// Bottom.
        /// </summary>
        Bottom = 2
    }

    /// <summary>
    /// Flags (bit-coded values).
    /// </summary>
    [Flags]
    internal enum MLineFlags
    {
        /// <summary>
        /// Has at least one vertex (code 72 is greater than 0).
        /// </summary>
        Has = 1,
        /// <summary>
        /// Closed.
        /// </summary>
        Closed = 2,
        /// <summary>
        /// Suppress start caps.
        /// </summary>
        NoStartCaps = 4,
        /// <summary>
        /// Suppress end caps.
        /// </summary>
        NoEndCaps = 8
    }

    /// <summary>
    /// Represents a multiline <see cref="EntityObject">entity</see>.
    /// </summary>
    public class MLine:
        EntityObject
    {

        #region private fields

        private double scale;
        private MLineStyle style;
        private MLineJustification justification;
        private bool isClosed;
        private bool noStartCaps;
        private bool noEndCaps;
        private double elevation;
        private MLineFlags flags;
        private List<MLineVertex> vertexes;

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
        /// <param name="isClosed">Sets if the multiline is closed</param>
        public MLine(IEnumerable<Vector2> vertexes, bool isClosed = false)
            : this(vertexes, MLineStyle.Default, 1.0, isClosed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">Multiline <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="scale">Multiline scale.</param>
        /// <param name="isClosed">Sets if the multiline is closed</param>
        public MLine(IEnumerable<Vector2> vertexes, double scale, bool isClosed = false)
            : this(vertexes, MLineStyle.Default, scale, isClosed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>MLine</c> class.
        /// </summary>
        /// <param name="vertexes">MLine <see cref="Vector2">vertex</see> location list in object coordinates.</param>
        /// <param name="style">MLine <see cref="MLineStyle">style.</see></param>
        /// <param name="scale">MLine scale.</param>
        /// <param name="isClosed">Sets if the multiline is closed</param>
        public MLine(IEnumerable<Vector2> vertexes, MLineStyle style, double scale, bool isClosed = false)
            : base(EntityType.MLine, DxfObjectCode.MLine)
        {
            this.scale = scale;
            if (style == null)
                throw new ArgumentNullException("style", "The MLine style cannot be null.");
            if (isClosed)
                this.flags = MLineFlags.Has | MLineFlags.Closed;
            else
                this.flags = MLineFlags.Has;

            this.style = style;
            this.justification = MLineJustification.Zero;
            this.isClosed = isClosed;
            this.noStartCaps = false;
            this.noEndCaps = false;
            this.elevation = 0.0;
            if (vertexes == null)
                throw new ArgumentNullException("vertexes");
            this.SetVertexes(vertexes);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the multiline <see cref="MLineVertex">segments</see> list.
        /// </summary>
        public List<MLineVertex> Vertexes
        {
            get { return this.vertexes; }
            internal set { this.vertexes = value; }
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
        /// <remarks>AutoCad accepts negative scales, but it is not recommended. </remarks>
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
            get { return this.isClosed; }
            set
            {
                if((this.flags & MLineFlags.Closed) == MLineFlags.Closed)
                    this.flags -= MLineFlags.Closed;
                if (value) 
                    this.flags |= MLineFlags.Closed;
                this.isClosed = value;
            }
        }

        /// <summary>
        /// Gets or sets the suppression of start caps.
        /// </summary>
        public bool NoStartCaps
        {
            get { return this.noStartCaps; }
            set
            {
                if ((this.flags & MLineFlags.NoStartCaps) == MLineFlags.NoStartCaps)
                    this.flags -= MLineFlags.NoStartCaps;
                if (value)
                    this.flags |= MLineFlags.NoStartCaps;
                this.noStartCaps = value;
            }
        }

        /// <summary>
        /// Gets or sets the suppression of end caps.
        /// </summary>
        public bool NoEndCaps
        {
            get { return this.noEndCaps; }
            set
            {
                if ((this.flags & MLineFlags.NoEndCaps) == MLineFlags.NoEndCaps)
                    this.flags -= MLineFlags.NoEndCaps;
                if (value)
                    this.flags |= MLineFlags.NoEndCaps;
                this.noEndCaps = value;
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
                    throw new ArgumentNullException("value", "The MLine style cannot be null.");
                this.style = value;
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
        }

        #endregion

        #region public methods
        
        /// <summary>
        /// Calculates the internal information of the multiline vertexes.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This function will be called when the entity created or when setting its vertexes;
        /// but, afterwards, this function will need to be called manually if any modification are done to the multiline.
        /// </para>
        /// <para>
        /// If the vertex distance list needs to be edited to represent trimmed multilines this function needs to be called prior any modification.
        /// It will calculate the minimun information needed to build a correct multiline.
        /// </para>
        /// </remarks>
        public void CalculateVertexesInfo()
        {
            if (this.vertexes.Count == 0)
                return;

            double reference = 0.0;
            switch (this.justification)
            {
                case MLineJustification.Top:
                    reference = this.style.Elements[this.style.Elements.Count - 1].Offset;
                    break;
                case MLineJustification.Zero:
                    reference = 0.0;
                    break;
                case MLineJustification.Bottom:
                    reference = this.style.Elements[0].Offset;
                    break;
            }

            Vector2 prevDir;
            if (Equals(this.vertexes[0].Location, this.vertexes[this.vertexes.Count - 1].Location))
                prevDir = Vector2.UnitY;
            else
            {
                prevDir = this.vertexes[0].Location - this.vertexes[this.vertexes.Count - 1].Location;
                prevDir.Normalize();
            }

            for (int i = 0; i < this.vertexes.Count; i++)
            {
                Vector2 position = this.vertexes[i].Location;
                Vector2 mitter;
                Vector2 dir;
                if (i == 0)
                {
                    if (Equals(this.vertexes[i + 1].Location, position))
                        dir = Vector2.UnitY;
                    else
                    {
                        dir = this.vertexes[i + 1].Location - position;
                        dir.Normalize();
                    }
                    if (this.isClosed)
                    {
                        mitter = prevDir - dir;
                        mitter.Normalize();
                    }
                    else
                        mitter = MathHelper.Transform(dir, this.style.StartAngle * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

                }
                else if (i + 1 == this.vertexes.Count)
                {
                    if (this.isClosed)
                    {
                        if (Equals(this.vertexes[0].Location, position))
                            dir = Vector2.UnitY;
                        else
                        {
                            dir = this.vertexes[0].Location - position;
                            dir.Normalize();
                        }
                        mitter = prevDir - dir;
                        mitter.Normalize();
                    }
                    else
                    {
                        dir = prevDir;
                        mitter = MathHelper.Transform(dir, this.style.EndAngle * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
                    }

                }
                else
                {
                    if (Equals(this.vertexes[i + 1].Location, position))
                        dir = Vector2.UnitY;
                    else
                    {
                        dir = this.vertexes[i + 1].Location - position;
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
                    double distance = (this.style.Elements[j].Offset + reference) / cos;
                    distances[j] = new List<double>
                                               {
                                                   distance*this.scale,
                                                   0.0
                                               };
                }

                this.vertexes[i] = new MLineVertex(position, dir, mitter, distances);
            }
        }

        /// <summary>
        /// Sets the positions of the multiline vertexes. 
        /// </summary>
        /// <param name="points">A list of <see cref="Vector3">points</see> that make up the multiline vertex list.</param>
        public void SetVertexes(IEnumerable<Vector2> points)
        {
            this.vertexes = new List<MLineVertex>();
            foreach (Vector2 point in points)
            {
                this.vertexes.Add(new MLineVertex(point, Vector2.Zero, Vector2.Zero, null));
            }
            this.CalculateVertexesInfo();
        }
        
        #endregion

        #region overrides

        /// <summary>
        /// Creates a new MLine that is a copy of the current instance.
        /// </summary>
        /// <returns>A new MLine that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<MLineVertex> copyVertexes = new List<MLineVertex>();
            foreach (MLineVertex vertex in this.vertexes)
            {
                copyVertexes.Add((MLineVertex)vertex.Clone());
            }
            return new MLine
            {
                //EntityObject properties
                Color = this.color,
                Layer = this.layer,
                LineType = this.lineType,
                Lineweight = this.lineweight,
                LineTypeScale = this.lineTypeScale,
                Normal = this.normal,
                XData = this.xData,
                //MLine properties
                Vertexes = copyVertexes,
                Elevation = this.elevation,
                Scale = this.scale,
                IsClosed = this.isClosed,
                NoStartCaps = this.noStartCaps,
                NoEndCaps = this.noEndCaps,
                Justification = this.justification,
                Style = this.style
            };
        }

        #endregion

    }
}
