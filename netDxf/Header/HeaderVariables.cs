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
using System.Collections.ObjectModel;
using System.Text;
using netDxf.Entities;

namespace netDxf.Header
{
    /// <summary>
    /// Represents the header variables of a dxf document.
    /// </summary>
    /// <remarks>
    /// The names of header variables are the same as they appear in the official dxf documentation but without the $,
    /// check it in case of doubt on what they represent.
    /// </remarks>
    public class HeaderVariables
    {
        #region private fields

        private readonly Dictionary<string, HeaderVariable> variables;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>SystemVariables</c>.
        /// </summary>
        /// <remarks>The default values are the same ones that are apply to a new AutoCad drawing.</remarks>
        public HeaderVariables()
        {
            variables = new Dictionary<string, HeaderVariable>
                           {
                               {HeaderVariableCode.AcadVer, new HeaderVariable(HeaderVariableCode.AcadVer, StringEnum.GetStringValue(DxfVersion.AutoCad2000))},
                               {HeaderVariableCode.DwgCodePage, new HeaderVariable(HeaderVariableCode.DwgCodePage, "ANSI_" + Encoding.Default.WindowsCodePage)},
                               {HeaderVariableCode.HandleSeed, new HeaderVariable(HeaderVariableCode.HandleSeed, Convert.ToString(1, 16))},
                               {HeaderVariableCode.Angbase, new HeaderVariable(HeaderVariableCode.Angbase, 0)},
                               {HeaderVariableCode.Angdir, new HeaderVariable(HeaderVariableCode.Angdir, 0)},
                               {HeaderVariableCode.AttMode, new HeaderVariable(HeaderVariableCode.AttMode, 1)},
                               {HeaderVariableCode.AUnits, new HeaderVariable(HeaderVariableCode.AUnits, 0)},
                               {HeaderVariableCode.AUprec, new HeaderVariable(HeaderVariableCode.AUprec, 0)},
                               {HeaderVariableCode.CeColor, new HeaderVariable(HeaderVariableCode.CeColor, 256)},
                               {HeaderVariableCode.CeLtScale, new HeaderVariable(HeaderVariableCode.CeLtScale, 1.0)},
                               {HeaderVariableCode.CeLtype, new HeaderVariable(HeaderVariableCode.CeLtype, "ByLayer")},
                               {HeaderVariableCode.CeLweight, new HeaderVariable(HeaderVariableCode.CeLweight, -1)},
                               {HeaderVariableCode.CLayer, new HeaderVariable(HeaderVariableCode.CLayer, "0")},
                               {HeaderVariableCode.CMLJust, new HeaderVariable(HeaderVariableCode.CMLJust, 0)},
                               {HeaderVariableCode.CMLScale, new HeaderVariable(HeaderVariableCode.CMLScale, 20)},
                               {HeaderVariableCode.CMLStyle, new HeaderVariable(HeaderVariableCode.CMLStyle, "Standard")},
                               {HeaderVariableCode.DimStyle, new HeaderVariable(HeaderVariableCode.DimStyle, "Standard")},
                               {HeaderVariableCode.TextSize, new HeaderVariable(HeaderVariableCode.TextSize, 2.5)},
                               {HeaderVariableCode.TextStyle, new HeaderVariable(HeaderVariableCode.TextStyle, "Standard")},
                               {HeaderVariableCode.LUnits, new HeaderVariable(HeaderVariableCode.LUnits, 2)},
                               {HeaderVariableCode.LUprec, new HeaderVariable(HeaderVariableCode.LUprec, 4)},
                               {HeaderVariableCode.Extnames, new HeaderVariable(HeaderVariableCode.Extnames, 1)},
                               {HeaderVariableCode.Insunits, new HeaderVariable(HeaderVariableCode.Insunits, (int) DefaultDrawingUnits.Millimeters)},
                               {HeaderVariableCode.LastSavedBy, new HeaderVariable(HeaderVariableCode.LastSavedBy, Environment.UserName)},
                               {HeaderVariableCode.LtScale, new HeaderVariable(HeaderVariableCode.LtScale, 1.0)},
                               {HeaderVariableCode.LwDisplay, new HeaderVariable(HeaderVariableCode.LwDisplay, 0)},
                               {HeaderVariableCode.PdMode, new HeaderVariable(HeaderVariableCode.PdMode, (int) PointShape.Dot)},
                               {HeaderVariableCode.PdSize, new HeaderVariable(HeaderVariableCode.PdSize, 0)},
                               {HeaderVariableCode.PLineGen, new HeaderVariable(HeaderVariableCode.PLineGen, 0)}
                           };
        }

        #endregion

        #region public properties

        /// <summary>
        /// The AutoCAD drawing database version number.
        /// </summary>
        /// <remarks>Only AutoCad2000 and higher dxf versions are supported.</remarks>
        /// <exception cref="NotSupportedException">Only AutoCad2000 and higher dxf versions are supported.</exception>
        public DxfVersion AcadVer
        {
            get
            {
                string code = (string) variables[HeaderVariableCode.AcadVer].Value;
                return (DxfVersion)StringEnum.Parse(typeof(DxfVersion), code);
            }
            set
            {
                if (value < DxfVersion.AutoCad2000)
                    throw new NotSupportedException("Only AutoCad2000 and newer dxf versions are supported.");
                variables[HeaderVariableCode.AcadVer].Value = StringEnum.GetStringValue(value);
            }
        }

        /// <summary>
        /// Next available handle.
        /// </summary>
        public string HandleSeed
        {
            get { return (string)variables[HeaderVariableCode.HandleSeed].Value; }
            internal set { variables[HeaderVariableCode.HandleSeed].Value = value; }
        }

        /// <summary>
        /// Angle 0 direction.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public double Angbase
        {
            get { return (double)variables[HeaderVariableCode.Angbase].Value; }
            internal set { variables[HeaderVariableCode.Angbase].Value = value; }
        }

        /// <summary>
        /// 1 = Clockwise angles, 0 = Counterclockwise.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public int Angdir
        {
            get { return (int)variables[HeaderVariableCode.Angdir].Value; }
            internal set { variables[HeaderVariableCode.Angdir].Value = value; }
        }

        /// <summary>
        /// Attribute visibility.
        /// </summary>
        /// <remarks>Default value: AttMode.Normal.</remarks>
        public AttMode AttMode
        {
            get { return (AttMode) variables[HeaderVariableCode.AttMode].Value; }
            set { variables[HeaderVariableCode.AttMode].Value = (int)value; }
        }

        /// <summary>
        /// Units format for angles.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public int AUnits
        {
            get { return (int)variables[HeaderVariableCode.AUnits].Value; }
            internal set { variables[HeaderVariableCode.AUnits].Value = value; }
        }

        /// <summary>
        /// Units precision for angles.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public int AUprec
        {
            get { return (int)variables[HeaderVariableCode.AUprec].Value; }
            set { variables[HeaderVariableCode.AUprec].Value = value; }
        }

        /// <summary>
        /// Current entity color.
        /// </summary>
        /// <remarks>Default value: 256 (ByLayer). This header variable only supports indexed colors.</remarks>
        public AciColor CeColor
        {
            get { return AciColor.FromCadIndex((short) variables[HeaderVariableCode.CeColor].Value); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                variables[HeaderVariableCode.CeColor].Value = value.Index;
            }
        }

        /// <summary>
        /// Current entity linetype scale.
        /// </summary>
        /// <remarks>Default value: 1.0.</remarks>
        public double CeLtScale
        {
            get { return (double) variables[HeaderVariableCode.CeLtScale].Value; }
            set
            {
                if (value <= 0 )
                    throw new ArgumentOutOfRangeException("value", value, "The linetype scale must be greater than zero.");
                variables[HeaderVariableCode.CeLtScale].Value = value;
            }
        }

        /// <summary>
        /// Current entity lineweight.
        /// </summary>
        /// <remarks>Default value: -1 (ByLayer).</remarks>
        public Lineweight CeLweight
        {
            get { return Lineweight.FromCadIndex((short) variables[HeaderVariableCode.CeLweight].Value); }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                variables[HeaderVariableCode.CeLweight].Value = value.Value;
            }
        }

        /// <summary>
        /// Current entity linetype name.
        /// </summary>
        /// <remarks>Default value: ByLayer.</remarks>
        public string CeLtype
        {
            get { return (string)variables[HeaderVariableCode.CeLtype].Value; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                variables[HeaderVariableCode.CeLtype].Value = value;
            }
        }

        /// <summary>
        /// Current layer name.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public string CLayer
        {
            get { return (string)variables[HeaderVariableCode.CLayer].Value; }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException("value");
                variables[HeaderVariableCode.CLayer].Value = value;
            }
        }              

        /// <summary>
        /// Current multiline justification.
        /// </summary>
        /// <remarks>Default value: 0 (Top).</remarks>
        public MLineJustification CMLJust
        {
            get { return (MLineJustification)variables[HeaderVariableCode.CMLJust].Value; }
            set { variables[HeaderVariableCode.CMLJust].Value = (int)value; }
        }

        /// <summary>
        /// Current multiline scale.
        /// </summary>
        /// <remarks>Default value: 20.</remarks>
        public double CMLScale
        {
            get { return (double) variables[HeaderVariableCode.CMLScale].Value; }
            set { variables[HeaderVariableCode.CMLScale].Value = value; }
        }

        /// <summary>
        /// Current multiline style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string CMLStyle
        {
            get { return (string)variables[HeaderVariableCode.CMLStyle].Value; }
            set { variables[HeaderVariableCode.CMLStyle].Value = value; }
        }

        /// <summary>
        /// Current dimension style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string DimStyle
        {
            get { return (string)variables[HeaderVariableCode.DimStyle].Value; }
            set { variables[HeaderVariableCode.DimStyle].Value = value; }
        }

        /// <summary>
        /// Default text height.
        /// </summary>
        /// <remarks>Default value: 2.5.</remarks>
        public double TextSize
        {
            get { return (double)variables[HeaderVariableCode.TextSize].Value; }
            set { variables[HeaderVariableCode.TextSize].Value = value; }
        }

        /// <summary>
        /// Current text style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string TextStyle
        {
            get { return (string)variables[HeaderVariableCode.TextStyle].Value; }
            set { variables[HeaderVariableCode.TextStyle].Value = value; }
        }

        /// <summary>
        /// Units format for coordinates and distances.
        /// </summary>
        /// <remarks>Default value: 2.</remarks>
        public int LUnits
        {
            get { return (int)variables[HeaderVariableCode.LUnits].Value; }
            internal set { variables[HeaderVariableCode.LUnits].Value = value; }
        }

        /// <summary>
        /// Units precision for coordinates and distances.
        /// </summary>
        /// <remarks>Default value: 4.</remarks>
        public int LUprec
        {
            get { return (int)variables[HeaderVariableCode.LUprec].Value; }
            set { variables[HeaderVariableCode.LUprec].Value = value; }
        }

        /// <summary>
        /// Drawing code page; set to the system code page when a new drawing is created, but not otherwise maintained by AutoCAD.
        /// </summary>
        public string DwgCodePage
        {
            get { return (string)variables[HeaderVariableCode.DwgCodePage].Value; }
            internal set { variables[HeaderVariableCode.DwgCodePage].Value = value; }
        }

        /// <summary>
        /// Controls symbol table naming.
        /// </summary>
        /// <remarks>
        /// Default value: 1.<br />
        /// Controls symbol table naming:<br />
        /// 0 = Release 14 compatibility. Limits names to 31 characters in length.<br />
        /// Names can include the letters A to Z, the numerals 0 to 9,
        /// and the special characters dollar sign ($), underscore (_), and hyphen (-).<br />
        /// 1 = AutoCAD 2000.<br />
        /// Names can be up to 255 characters in length, 
        /// and can include the letters A to Z, the numerals 0 to 9, spaces, 
        /// and any special characters not used for other purposes by Microsoft Windows and AutoCAD.
        /// </remarks>
        public bool Extnames
        {
            get { return (int)variables[HeaderVariableCode.Extnames].Value != 0; }
            internal set { variables[HeaderVariableCode.Extnames].Value = value ? 1 : 0; }
        }

        /// <summary>
        /// Default drawing units for AutoCAD DesignCenter blocks.
        /// </summary>
        /// <remarks>
        /// Default value: 0.<br />
        /// Also applies to raster image units, eventhought they have the RasterVariables object and units in ImageDef.
        /// </remarks>
        public DefaultDrawingUnits Insunits
        {
            get { return (DefaultDrawingUnits)variables[HeaderVariableCode.Insunits].Value; }
            internal set { variables[HeaderVariableCode.Insunits].Value = (int)value; }
        }

        /// <summary>
        /// User name that saved the file.
        /// </summary>
        /// <remarks>
        /// By default it uses the user name of the person who is currently logged on to the Windows operating system.
        /// </remarks>
        public string LastSavedBy
        {
            get { return (string)variables[HeaderVariableCode.LastSavedBy].Value; }
            set { variables[HeaderVariableCode.LwDisplay].Value = value; }
        }

        /// <summary>
        /// Global linetype scale.
        /// </summary>
        /// <remarks>Default value: 1.0.</remarks>
        public double LtScale
        {
            get { return (double) variables[HeaderVariableCode.LtScale].Value; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("value", value, "The linetype scale must be greater than zero.");
                variables[HeaderVariableCode.LtScale].Value = value;
            }
        }

        /// <summary>
        /// Controls the display of lineweights on the Model or Layout tab.
        /// </summary>
        /// <remarks>
        /// Default value: false.<br />
        /// false = Lineweight is not displayed.<br />
        /// true = Lineweight is displayed.<br />
        /// </remarks>
        public bool LwDisplay
        {
            get { return (int)variables[HeaderVariableCode.LwDisplay].Value != 0; }
            set { variables[HeaderVariableCode.LwDisplay].Value = value ? 1 : 0; }
        }

        /// <summary>
        /// Controls the <see cref="PointShape">shape</see> to draw a point entity.
        /// </summary>
        /// <remarks>Default value: PointShape.Dot.</remarks>
        public PointShape PdMode
        {
            get { return (PointShape) variables[HeaderVariableCode.PdMode].Value; }
            set { variables[HeaderVariableCode.PdMode].Value = (int)value; }
        }

        /// <summary>
        /// Controls the size of the point figures, except for PDMODE values 0 (Dot) and 1 (Empty).
        /// </summary>
        /// <remarks>
        /// Default value: 0.<br />
        /// A setting of 0 generates the point at 5 percent of the drawing area height.<br />
        /// A positive PDSIZE value specifies an absolute size for the point figures.<br />
        /// A negative value is interpreted as a percentage of the viewport size. <br />
        /// </remarks>
        public double PdSize
        {
            get { return (double)variables[HeaderVariableCode.PdSize].Value; }
            set { variables[HeaderVariableCode.PdSize].Value = value; }
        }

        /// <summary>
        /// Governs the generation of linetype patterns around the vertices of a 2D polyline.
        /// </summary>
        /// <remarks>
        /// Default value: 0.<br />
        /// 1 = Linetype is generated in a continuous pattern around vertices of the polyline.<br />
        /// 0 = Each segment of the polyline starts and ends with a dash.
        /// </remarks>
        public int PLineGen
        {
            get { return (int)variables[HeaderVariableCode.PLineGen].Value; }
            set
            {
                if (value == 0 || value == 1)
                    variables[HeaderVariableCode.PLineGen].Value = value;
                else
                    throw new ArgumentOutOfRangeException("value", value, "Accepted values are 0 or 1.");
            }
        }

        #endregion

        #region internal properties

        internal ReadOnlyCollection<HeaderVariable> Values
        {
            get
            {
                List<HeaderVariable> list = new List<HeaderVariable>();
                list.AddRange(this.variables.Values);
                return list.AsReadOnly();
            }
        }
 
        #endregion

    }
}
