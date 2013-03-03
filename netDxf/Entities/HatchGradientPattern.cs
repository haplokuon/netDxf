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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Gradient pattern types.
    /// </summary>
    public enum HatchGradientPatternType
    {
        /// <summary>
        /// Linear.
        /// </summary>
        [StringValue("LINEAR")]
        Linear,
        /// <summary>
        /// Cylinder.
        /// </summary>
        [StringValue("CYLINDER")]
        Cylinder,
        /// <summary>
        /// Inverser cylinder.
        /// </summary>
        [StringValue("INVCYLINDER")]
        InvCylinder,
        /// <summary>
        /// Spherical.
        /// </summary>
        [StringValue("SPHERICAL")]
        Spherical,
        /// <summary>
        /// Inverse spherical.
        /// </summary>
        [StringValue("INVSPHERICAL")]
        InvSpherical,
        /// <summary>
        /// Hemispherical.
        /// </summary>
        [StringValue("HEMISPHERICAL")]
        Hemispherical,
        /// <summary>
        /// Inverse hemispherical.
        /// </summary>
        [StringValue("INVHEMISPHERICAL")]
        InvHemispherical,
        /// <summary>
        /// Curved.
        /// </summary>
        [StringValue("CURVED")]
        Curved,
        /// <summary>
        /// Inverse curved.
        /// </summary>
        [StringValue("INVCURVED")]
        InvCurved
    }

    /// <summary>
    /// Represents the hatch gradient pattern style.
    /// </summary>
    /// <remarks>
    /// Grandient patterns are only supported by AutoCad2004 and higher dxf versions. It will default to a solid pattern if saved as AutoCad2000.
    /// </remarks>
    public class HatchGradientPattern :
        HatchPattern
    {

        #region private fields

        private HatchGradientPatternType gradientType;
        private AciColor color1;
        private AciColor color2;
        private bool singleColor;
        private double tint;
        private bool centered;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>HatchGradientPattern</c> class as a default linear gradient. 
        /// </summary>
        /// <param name="description">Description of the pattern (optional, this information is not saved in the dxf file). By default it will use the supplied name.</param>
        public HatchGradientPattern(string description = null)
            : base("SOLID", description)
        {
            this.color1 = AciColor.Blue;
            this.color2 = AciColor.Yellow;
            this.singleColor = false;
            this.gradientType = HatchGradientPatternType.Linear;
            this.tint = 1.0;
            this.centered = true;
        }

        /// <summary>
        /// Initializes a new instance of the <c>HatchGradientPattern</c> class as a single color gradient. 
        /// </summary>
        /// <param name="color">Gradient <see cref="AciColor">color</see>.</param>
        /// <param name="tint">Gradient tint.</param>
        /// <param name="type">Gradient <see cref="HatchGradientPatternType">type</see>.</param>
        /// <param name="description">Description of the pattern (optional, this information is not saved in the dxf file). By default it will use the supplied name.</param>
        public HatchGradientPattern(AciColor color, double tint, HatchGradientPatternType type, string description = null)
            : base("SOLID", description)
        {
            if (color == null)
                throw new ArgumentNullException("color");
            this.color1 = color;
            this.color2 = Color2FromTint(tint);
            this.singleColor = true;
            this.gradientType = type;
            this.tint = tint;
            this.centered = true;
        }

        /// <summary>
        /// Initializes a new instance of the <c>HatchGradientPattern</c> class as a two color gradient. 
        /// </summary>
        /// <param name="color1">Gradient <see cref="AciColor">color</see> 1.</param>
        /// <param name="color2">Gradient <see cref="AciColor">color</see> 2.</param>
        /// <param name="type">Gradient <see cref="HatchGradientPatternType">type</see>.</param>
        /// <param name="description">Description of the pattern (optional, this information is not saved in the dxf file). By default it will use the supplied name.</param>
        public HatchGradientPattern(AciColor color1, AciColor color2, HatchGradientPatternType type, string description = null)
            : base("SOLID", description)
        {
            if (color1 == null)
                throw new ArgumentNullException("color1");
            this.color1 = color1;
            if (color2 == null)
                throw new ArgumentNullException("color2");
            this.color2 = color2;
            this.singleColor = false;
            this.gradientType = type;
            this.tint = 1.0;
            this.centered = true;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or set the gradient pattern <see cref="HatchGradientPatternType">type</see>.
        /// </summary>
        public HatchGradientPatternType GradientType
        {
            get { return gradientType; }
            set { gradientType = value; }
        }

        /// <summary>
        /// Gets or sets the gradient <see cref="AciColor">color</see> 1.
        /// </summary>
        public AciColor Color1
        {
            get { return color1; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                color1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the gradient <see cref="AciColor">color</see> 2.
        /// </summary>
        /// <remarks>
        /// If color 2 is defined, automatically the single color property will be set to false.  
        /// </remarks>
        public AciColor Color2
        {
            get { return color2; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.singleColor = false;
                color2 = value;
            }
        }

        /// <summary>
        /// Gets or sets the gradient pattern color type.
        /// </summary>
        public bool SingleColor
        {
            get { return singleColor; }
            set
            {
                if (value)
                    this.Color2 = Color2FromTint(this.tint);
                singleColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the gradient pattern tint.
        /// </summary>
        /// <remarks>It only applies to single color gradient patterns.</remarks>
        public double Tint
        {
            get { return tint; }
            set
            {
                if(singleColor)
                    this.Color2 = Color2FromTint(value);
                tint = value;
            }
        }

        /// <summary>
        /// Gets or sets the gradient definition; corresponds to the centered option on the gradient.
        /// </summary>
        /// <remarks>
        /// Each gradient has two definitions, shifted and unshifted. A shift value describes the blend of the two definitions that should be used.
        /// A value of 0.0 (false) means only the unshifted version should be used, and a value of 1.0 (true) means that only the shifted version should be used.
        /// </remarks>
        public bool Centered
        {
            get { return centered; }
            set { centered = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Modifies or creates the xData hatch information entries that hold the values for the gradient color indexes.
        /// </summary>
        /// <param name="xdata">Hatch <see cref="XData">XData</see></param>
        public void GradientColorAciXData(Dictionary<string, XData> xdata)
        {
            if (xdata.ContainsKey("GradientColor1ACI"))
            {
                XData xdataEntry = xdata["GradientColor1ACI"];
                XDataRecord record = new XDataRecord(XDataCode.Integer, this.color1.Index);
                xdataEntry.XDataRecord.Clear();
                xdataEntry.XDataRecord.Add(record);
            }
            else
            {
                XData xdataEntry = new XData(new ApplicationRegistry("GradientColor1ACI"));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Integer, this.Color1.Index));
                xdata.Add(xdataEntry.ApplicationRegistry.Name, xdataEntry);
            }

            if (xdata.ContainsKey("GradientColor2ACI"))
            {
                XData xdataEntry = xdata["GradientColor2ACI"];
                XDataRecord record = new XDataRecord(XDataCode.Integer, this.color2.Index);
                xdataEntry.XDataRecord.Clear();
                xdataEntry.XDataRecord.Add(record);
            }
            else
            {
                XData xdataEntry = new XData(new ApplicationRegistry("GradientColor2ACI"));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Integer, this.color2.Index));
                xdata.Add(xdataEntry.ApplicationRegistry.Name, xdataEntry);
            }
        }

        #endregion

        #region private methods

        private AciColor Color2FromTint(double value)
        {
            double h, s, l;
            AciColor.ToHsl(color1, out h, out s, out l);
            return AciColor.FromHsl(h, s, value);
        }

        #endregion
    }
}
