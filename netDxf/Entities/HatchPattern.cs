#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Entities
{

    /// <summary>
    /// Predefined hatch pattern name.
    /// </summary>
    public sealed class PredefinedHatchPatternName
    {
        /// <summary>
        /// Solid.
        /// </summary>
        public const string Solid = "SOLID";
        /// <summary>
        /// Line.
        /// </summary>
        public const string Line = "LINE";
        /// <summary>
        /// Net.
        /// </summary>
        public const string Net = "NET";
        /// <summary>
        /// Dots.
        /// </summary>
        public const string Dots = "DOTS";
    }

    /// <summary>
    /// Hatch style.
    /// </summary>
    public enum HatchStyle
    {
        /// <summary>
        /// Hatch “odd parity” area.
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Hatch outermost area only.
        /// </summary>
        Outer = 1,
        /// <summary>
        /// Hatch through entire area.
        /// </summary>
        Ignore = 2
    }

    /// <summary>
    /// Hatch pattern type.
    /// </summary>
    public enum HatchType
    {
        /// <summary>
        /// User defined.
        /// </summary>
        UserDefined = 0,
        /// <summary>
        /// Predefined.
        /// </summary>
        Predefined = 1,
        /// <summary>
        /// Custom.
        /// </summary>
        Custom = 2
    }

    /// <summary>
    /// Solid fill flag (solid fill = 1; pattern fill = 0); for MPolygon, the version of MPolygon.
    /// </summary>
    public enum FillFlag
    {  
        /// <summary>
        /// Pattern fill.
        /// </summary>
        PatternFill = 0,
        /// <summary>
        /// Solid fill.
        /// </summary>
        SolidFill = 1    
    }
    /// <summary>
    /// Represents the hatch pattern style.
    /// </summary>
    public class HatchPattern
    {
        #region private fields
        private readonly string name;
        private readonly HatchStyle style;
        private readonly HatchType type;
        private readonly FillFlag fill;
        private double angle;
        private double lineSeparation;
        private Vector2 lineBasePoint;
        #endregion

        #region constructor
        /// <summary>
        /// For private use, only the predefined patterns are supported.
        /// </summary>
        /// <param name="name">Pattern name.</param>
        private HatchPattern(string name)
        {
            this.name = name;
            this.style = HatchStyle.Normal;
            this.fill = this.name == PredefinedHatchPatternName.Solid ? FillFlag.SolidFill : FillFlag.PatternFill;
            this.type = HatchType.Predefined;
            this.angle = 0.0;
            this.lineSeparation = 1.0;
            this.lineBasePoint = Vector2.Zero;
        }
        #endregion

        #region predefined patterns
        /// <summary>
        /// Solid hatch pattern.
        /// </summary>
        public static HatchPattern Solid
        {
            get { return new HatchPattern(PredefinedHatchPatternName.Solid); }
        }
        /// <summary>
        /// Lines hatch pattern.
        /// </summary>
        public static HatchPattern Line
        {
            get{ return new HatchPattern(PredefinedHatchPatternName.Line);}
        }
        /// <summary>
        /// Net or squares hatch pattern.
        /// </summary>
        public static HatchPattern Net
        {
            get { return new HatchPattern(PredefinedHatchPatternName.Net); }
        }
        /// <summary>
        /// Dots hatch patter.
        /// </summary>
        public static HatchPattern Dots
        {
            get { return new HatchPattern(PredefinedHatchPatternName.Dots); }
        }
        #endregion

        #region public properties

        /// <summary>
        /// Gets the hatch pattern name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets the hatch style.
        /// </summary>
        public HatchStyle Style
        {
            get { return style; }
        }

        /// <summary>
        /// Hatch pattern type.
        /// </summary>
        /// <remarks> Only predefined types are supported.</remarks>
        public HatchType Type
        {
            get { return type; }
        }

        /// <summary>
        /// Solid fill flag.
        /// </summary>
        public FillFlag Fill
        {
            get { return fill; }
        }

        /// <summary>
        /// Gets or sets the pattern angle between 0 and 180 degrees (not aplicable in Solid fills).
        /// </summary>
        public double Angle
        {
            get { return angle; }
            set
            {
                if (value < 0 || value > 180)
                    throw new ArgumentOutOfRangeException("value", value.ToString());
                angle = value;
            }
        }

        /// <summary>
        /// Gets or sets the pattern line separation (not aplicable in Solid fills).
        /// </summary>
        public double LineSeparation
        {
            get { return lineSeparation; }
            set { lineSeparation = value; }

        }

        /// <summary>
        /// Gets or sets the pattern line base point (not aplicable in Solid fills).
        /// </summary>
        public Vector2 LineBasePoint
        {
            get { return lineBasePoint; }
            set { lineBasePoint = value; }
        }
        #endregion

    }
}
