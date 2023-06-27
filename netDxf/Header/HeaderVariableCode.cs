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

namespace netDxf.Header
{
    /// <summary>
    /// Strings system variables
    /// </summary>
    public static class HeaderVariableCode
    {
        /// <summary>
        /// The AutoCAD drawing database version number.
        /// </summary>
        public const string AcadVer = "$ACADVER";

        /// <summary>
        /// Next available handle.
        /// </summary>
        public const string HandleSeed = "$HANDSEED";

        /// <summary>
        /// Angle 0 direction.
        /// </summary>
        public const string Angbase = "$ANGBASE";

        /// <summary>
        /// 1 = Clockwise angles, 0 = Counterclockwise.
        /// </summary>
        public const string Angdir = "$ANGDIR";

        /// <summary>
        /// Attribute visibility.
        /// </summary>
        public const string AttMode = "$ATTMODE";

        /// <summary>
        /// Units format for angles.
        /// </summary>
        public const string AUnits = "$AUNITS";

        /// <summary>
        /// Units precision for angles.
        /// </summary>
        public const string AUprec = "$AUPREC";

        /// <summary>
        /// Current entity color.
        /// </summary>
        public const string CeColor = "$CECOLOR";

        /// <summary>
        /// Current entity line type scale.
        /// </summary>
        public const string CeLtScale = "$CELTSCALE";

        /// <summary>
        /// Current entity lineweight.
        /// </summary>
        public const string CeLweight = "$CELWEIGHT";

        /// <summary>
        /// Current entity line type name.
        /// </summary>
        public const string CeLtype = "$CELTYPE";

        /// <summary>
        /// Current layer name.
        /// </summary>
        public const string CLayer = "$CLAYER";

        /// <summary>
        /// Current multiline justification.
        /// </summary>
        public const string CMLJust = "$CMLJUST";

        /// <summary>
        /// Current multiline scale.
        /// </summary>
        public const string CMLScale = "$CMLSCALE";

        /// <summary>
        /// Current multiline style name.
        /// </summary>
        public const string CMLStyle = "$CMLSTYLE";

        /// <summary>
        /// Current dimension style name.
        /// </summary>
        public const string DimStyle = "$DIMSTYLE";

        /// <summary>
        /// Default text height.
        /// </summary>
        public const string TextSize = "$TEXTSIZE";

        /// <summary>
        /// Current text style name.
        /// </summary>
        public const string TextStyle = "$TEXTSTYLE";

        /// <summary>
        /// Units format for coordinates and distances.
        /// </summary>
        public const string LUnits = "$LUNITS";

        /// <summary>
        /// Units precision for coordinates and distances.
        /// </summary>
        public const string LUprec = "$LUPREC";

        /// <summary>
        /// Drawing code page; set to the system code page when a new drawing is created, but not otherwise maintained by AutoCAD.
        /// </summary>
        public const string DwgCodePage = "$DWGCODEPAGE";

        /// <summary>
        /// Controls symbol table naming.
        /// </summary>
        /// <remarks>
        /// Controls symbol table naming:<br />
        /// 0 = Release 14 compatibility. Limits names to 31 characters in length.<br />
        /// Names can include the letters A to Z, the numerals 0 to 9,
        /// and the special characters dollar sign ($), underscore (_), and hyphen (-).<br />
        /// 1 = AutoCAD 2000.<br />
        /// Names can be up to 255 characters in length, 
        /// and can include the letters A to Z, the numerals 0 to 9, spaces, 
        /// and any special characters not used for other purposes by Microsoft Windows and AutoCAD.
        /// </remarks>
        public const string Extnames = "$EXTNAMES";

        /// <summary>
        /// Insertion base point for the current drawing.
        /// </summary>
        /// <remarks>
        /// When you insert or externally reference the current drawing into other drawings, this base point is used as the insertion base point.
        /// </remarks>
        public const string InsBase = "$INSBASE";

        /// <summary>
        /// Default drawing units for AutoCAD DesignCenter blocks.
        /// </summary>
        /// <remarks>
        /// The US Surveyor Units were introduced in the AutoCad2018 DXF version (AC1032),
        /// they may be not supported if the file is loaded in earlier versions of AutoCad.
        /// </remarks>
        public const string InsUnits = "$INSUNITS";

        /// <summary>
        /// User name that saved the file.
        /// </summary>
        public const string LastSavedBy = "$LASTSAVEDBY";

        /// <summary>
        /// Controls the display of lineweights on the Model or Layout tab.
        /// </summary>
        /// <remarks>
        /// 0 = Lineweight is not displayed
        /// 1 = Lineweight is displayed
        /// </remarks>
        public const string LwDisplay = "$LWDISPLAY";

        /// <summary>
        /// Global line type scale.
        /// </summary>
        public const string LtScale = "$LTSCALE";

        /// <summary>
        /// Controls if the text will be mirrored during a symmetry.
        /// </summary>
        public const string MirrText  = "$MIRRTEXT";

        /// <summary>
        /// Controls the <see cref="PointShape">shape</see> to draw a point entity.
        /// </summary>
        public const string PdMode = "$PDMODE";

        /// <summary>
        /// Controls the size of the point figures, except for PDMODE values 0 (Dot) and 1 (Empty).
        /// </summary>
        /// <remarks>
        /// A setting of 0 generates the point at 5 percent of the drawing area height.<br />
        /// A positive PDSIZE value specifies an absolute size for the point figures.<br />
        /// A negative value is interpreted as a percentage of the viewport size.<br />
        /// </remarks>
        public const string PdSize = "$PDSIZE";

        /// <summary>
        /// Governs the generation of line type patterns around the vertexes of a 2D polyline.
        /// </summary>
        /// <remarks>
        /// 1 = Line type is generated in a continuous pattern around vertexes of the polyline.<br />
        /// 0 = Each segment of the polyline starts and ends with a dash.
        /// </remarks>
        public const string PLineGen = "$PLINEGEN";

        /// <summary>
        /// Controls paper space line type scaling.
        /// </summary>
        /// <remarks>
        /// 1 = No special line type scaling.<br />
        /// 0 = Viewport scaling governs line type scaling.
        /// </remarks>
        public const string PsLtScale = "$PSLTSCALE";

        /// <summary>
        /// Defines number of line segments generated for smoothed out polylines.
        /// </summary>
        public const string SplineSegs = "$SPLINESEGS";

        /// <summary>
        /// Define the number of segments generated for smoothed polygon meshes in M direction.
        /// </summary>
        public const string SurfU = "$SURFU";

        /// <summary>
        /// Define the number of segments generated for smoothed polygon meshes in N direction.
        /// </summary>
        public const string SurfV = "$SURFV";

        /// <summary>
        /// Local date/time of drawing creation.
        /// </summary>
        public const string TdCreate = "$TDCREATE";

        /// <summary>
        /// Universal date/time the drawing was created.
        /// </summary>
        public const string TduCreate = "$TDUCREATE";

        /// <summary>
        /// Local date/time of last drawing update.
        /// </summary>
        public const string TdUpdate = "$TDUPDATE";

        /// <summary>
        /// Universal date/time of the last update/save.
        /// </summary>
        public const string TduUpdate = "$TDUUPDATE";

        /// <summary>
        /// Cumulative editing time for this drawing.
        /// </summary>
        public const string TdinDwg = "$TDINDWG";

        /// <summary>
        /// Origin of current UCS (in WCS).
        /// </summary>
        public const string UcsOrg = "$UCSORG";

        /// <summary>
        /// Direction of the current UCS X axis (in WCS).
        /// </summary>
        public const string UcsXDir = "$UCSXDIR";

        /// <summary>
        /// Direction of the current UCS Y axis (in WCS).
        /// </summary>
        public const string UcsYDir = "$UCSYDIR";
    }
}