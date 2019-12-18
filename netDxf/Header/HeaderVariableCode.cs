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

namespace netDxf.Header
{
    using System.Reflection.Emit;
    using System.Runtime.Remoting.Metadata.W3cXsd2001;
    using System.Runtime.Serialization;

    /// <summary>
    /// Strings system variables
    /// </summary>
    public static class HeaderVariableCode
    {
        /// <summary>
        /// Maintenance version number (should be ignored)
        /// </summary>
        public const string AcadMainVer = "$ACADMAINTVER";

        /// <summary>
        /// The AutoCAD drawing database version number.
        /// </summary>
        public const string AcadVer = "$ACADVER";

        /// <summary>
        /// Next available handle.
        /// </summary>
        public const string HandleSeed = "$HANDSEED";

        /// <summary>
        /// Specifies HIDETEXT system variable:
        /// 0 = HIDE ignores text objects when producing the hidden view
        /// 1 = HIDE does not ignore text objects
        /// </summary>
        public const string HideText = "$HIDETEXT";

        /// <summary>
        /// Path for all relative hyperlinks in the drawing. If null, the drawing path is used
        /// </summary>
        public const string HyperlinkBase = "$HYPERLINKBASE";

        /// <summary>
        /// Controls whether layer and spatial indexes are created and saved in drawing files:
        /// 0 = No indexes are created
        /// 1 = Layer index is created
        /// 2 = Spatial index is created
        /// 3 = Layer and spatial indexes are created
        /// </summary>
        public const string IndexCtl = "$INDEXCTL";

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
        /// Plotstyle handle of new objects;
        /// if CEPSNTYPE is 3, then this value indicates the handle
        /// </summary>
        public const string CePsnId = "$CEPSNID";

        /// <summary>
        /// Plot style type of new objects:
        /// 0 = Plot style by layer
        /// 1 = Plot style by block
        /// 2 = Plot style by dictionary default
        /// 3 = Plot style by object ID/handle
        /// </summary>
        public const string CePsnType = "$CEPSNTYPE";

        /// <summary>
        /// First chamfer distance
        /// </summary>
        public const string ChamferA = "$CHAMFERA";

        /// <summary>
        /// Second chamfer distance
        /// </summary>
        public const string ChamferB = "$CHAMFERB";

        /// <summary>
        /// Chamfer length
        /// </summary>
        public const string ChamferC = "$CHAMFERC";

        /// <summary>
        /// Chamfer angle
        /// </summary>
        public const string ChamferD = "$CHAMFERD";

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
        /// Shadow mode for a 3D object:
        /// 0 = Casts and receives shadows
        /// 1 = Casts shadows
        /// 2 = Receives shadows
        /// 3 = Ignores shadows
        /// Note: Starting with AutoCAD 2016-based products,
        /// this variable is obsolete but still supported for backwards compatibility.
        /// </summary>
        public const string CShadow = "$CSHADOW";

        /// <summary>
        /// Number of precision places displayed in angular dimensions
        /// </summary>
        public const string DimAdec = "$DIMADEC";

        /// <summary>
        /// Alternate unit dimensioning performed if nonzero
        /// </summary>
        public const string DimAlt = "$DIMALT";

        /// <summary>
        /// Alternate unit decimal places
        /// </summary>
        public const string DimAltD = "$DIMALTD";

        /// <summary>
        /// Alternate unit scale factor
        /// </summary>
        public const string DimAltF = "$DIMALTF";

        /// <summary>
        /// Determines rounding of alternate units
        /// </summary>
        public const string DimAltRnd = "$DIMALTRND";

        /// <summary>
        /// Number of decimal places for tolerance values of an alternate units dimension
        /// </summary>
        public const string DimAltTd = "$DIMALTTD";

        /// <summary>
        /// Controls suppression of zeros for alternate tolerance values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero feet
        /// To suppress leading or trailing zeros, add the following values to one of the preceding values:
        /// 4 = Suppresses leading zeros
        /// 8 = Suppresses trailing zeros
        /// </summary>
        public const string DimAltTz = "$DIMALTTZ";

        /// <summary>
        /// Units format for alternate units of all dimension style family members except angular:
        /// 1 = Scientific
        /// 2 = Decimal
        /// 3 = Engineering
        /// 4 = Architectural (stacked)
        /// 5 = Fractional (stacked)
        /// 6 = Architectural
        /// 7 = Fractional
        /// 8 = Operating system defines the decimal separator and number grouping symbols
        /// </summary>
        public const string DimAltU = "$DIMALTU";

        /// <summary>
        /// Controls suppression of zeros for alternate unit dimension values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero feet
        /// 4 = Suppresses leading zeros in decimal dimensions
        /// 8 = Suppresses trailing zeros in decimal dimensions
        /// 12 = Suppresses both leading and trailing zeros
        /// </summary>
        public const string DimAltZ = "$DIMALTZ";

        /// <summary>
        /// Alternate dimensioning suffix
        /// </summary>
        public const string DimAPost = "$DIMAPOST";

        /// <summary>
        /// 1 = Create associative dimensioning
        /// 0 = Draw individual entities
        /// Note: Obsolete; see $DIMASSOC.
        /// </summary>
        public const string DimAso = "$DIMASO";

        /// <summary>
        /// Controls the associativity of dimension objects
        /// 0 = Creates exploded dimensions; there is no association between elements of the dimension, and the lines, arcs, arrowheads, and text of a dimension are drawn as separate objects
        /// 1 = Creates non-associative dimension objects; the elements of the dimension are formed into a single object, and if the definition point on the object moves, then the dimension value is updated
        /// 2 = Creates associative dimension objects; the elements of the dimension are formed into a single object and one or more definition points of the dimension are coupled with association points on geometric objects
        /// </summary>
        public const string DimAssoc = "$DIMASSOC";

        /// <summary>
        /// Dimensioning arrow size
        /// </summary>
        public const string DimAsz = "$DIMASZ";

        /// <summary>
        /// Controls dimension text and arrow placement when space is not sufficient to place both within the extension lines:
        /// 0 = Places both text and arrows outside extension lines
        /// 1 = Moves arrows first, then text
        /// 2 = Moves text first, then arrows
        /// 3 = Moves either text or arrows, whichever fits best
        /// AutoCAD adds a leader to moved dimension text when DIMTMOVE is set to 1
        /// </summary>
        public const string DimAtFit = "$DIMATFIT";

        /// <summary>
        /// Angle format for angular dimensions:
        /// 0 = Decimal degrees
        /// 1 = Degrees/minutes/seconds;
        /// 2 = Gradians
        /// 3 = Radians
        /// 4 = Surveyor's units
        /// </summary>
        public const string DimAUnit = "$DIMAUNIT";

        /// <summary>
        /// Controls suppression of zeros for angular dimensions:
        /// 0 = Displays all leading and trailing zeros
        /// 1 = Suppresses leading zeros in decimal dimensions
        /// 2 = Suppresses trailing zeros in decimal dimensions
        /// 3 = Suppresses leading and trailing zeros
        /// </summary>
        public const string DimAZin = "$DIMAZIN";

        /// <summary>
        /// Arrow block name
        /// </summary>
        public const string DimBlk = "$DIMBLK";

        /// <summary>
        /// First arrow block name
        /// </summary>
        public const string DimBlk1 = "$DIMBLK1";

        /// <summary>
        /// Second arrow block name
        /// </summary>
        public const string DimBlk2 = "$DIMBLK2";

        /// <summary>
        /// Size of center mark/lines
        /// </summary>
        public const string DimCen = "$DIMCEN";

        /// <summary>
        /// Dimension line color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public const string DimClrd = "$DIMCLRD";

        /// <summary>
        /// Dimension extension line color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public const string DimClRe = "$DIMCLRE";

        /// <summary>
        /// Dimension text color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public const string DimClRt = "$DIMCLRT";

        /// <summary>
        /// Number of decimal places for the tolerance values of a primary units dimension
        /// </summary>
        public const string DimDec = "$DIMDEC";

        /// <summary>
        /// Dimension line extension
        /// </summary>
        public const string DimDle = "$DIMDLE";

        /// <summary>
        /// Dimension line increment
        /// </summary>
        public const string DimDli = "$DIMDLI";

        /// <summary>
        /// Single-character decimal separator used when creating dimensions whose unit format is decimal
        /// </summary>
        public const string DimDsep = "$DIMDSEP";

        /// <summary>
        /// Extension line extension
        /// </summary>
        public const string DimExE = "$DIMEXE";

        /// <summary>
        /// Extension line offset
        /// </summary>
        public const string DimExO = "$DIMEXO";

        /// <summary>
        /// Scale factor used to calculate the height of text for dimension fractions and tolerances. AutoCAD multiplies DIMTXT by
        /// DIMTFAC to set the fractional or tolerance text height.
        /// </summary>
        public const string DimFac = "$DIMFAC";

        /// <summary>
        /// Text inside horizontal if nonzero
        /// </summary>
        public const string DimTih = "$DIMTIH";

        /// <summary>
        /// Force text inside extensions if nonzero
        /// </summary>
        public const string DimTix = "$DIMTIX";

        /// <summary>
        /// Minus tolerance
        /// </summary>
        public const string DimMtm = "$DIMTM";

        /// <summary>
        /// Dimension text movement rules:
        /// 0 = Moves the dimension line with dimension text
        /// 1 = Adds a leader when dimension text is moved
        /// 2 = Allows text to be moved freely without a leader
        /// </summary>
        public const string DimTMove = "$DIMTMOVE";

        /// <summary>
        /// If text is outside the extension lines,
        /// dimension lines are forced between the extension lines if nonzero
        /// </summary>
        public const string DimToFl = "$DIMTOFL";

        /// <summary>
        /// Text outside horizontal if nonzero
        /// </summary>
        public const string DimToH = "$DIMTOH";

        /// <summary>
        /// Dimension tolerances generated if nonzero
        /// </summary>
        public const string DimToL = "$DIMTOL";

        /// <summary>
        /// Vertical justification for tolerance values:
        /// 0 = Top
        /// 1 = Middle
        /// 2 = Bottom
        /// </summary>
        public const string DimToLj = "$DIMTOLJ";

        /// <summary>
        /// Plus tolerance
        /// </summary>
        public const string DimTp = "$DIMTP";

        /// <summary>
        /// Dimensioning tick size:
        /// 0 = Draws arrowheads
        /// >0 = Draws oblique strokes instead of arrowheads
        /// </summary>
        public const string DimTsz = "$DIMTSZ";

        /// <summary>
        /// Text vertical position
        /// </summary>
        public const string DimTvp = "$DIMTVP";

        /// <summary>
        /// Dimension text style
        /// </summary>
        public const string DimTxSty = "$DIMTXSTY";

        /// <summary>
        /// Dimensioning text height
        /// </summary>
        public const string DimTxt = "$DIMTXT";

        /// <summary>
        /// Controls suppression of zeros for tolerance values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero fee
        /// 4 = Suppresses leading zeros in decimal dimensions
        /// 8 = Suppresses trailing zeros in decimal dimensions
        /// 12 = Suppresses both leading and trailing zeros
        /// </summary>
        public const string DimTzIn = "$DIMTZIN";

        /// <summary>
        /// Cursor functionality for user-positioned text:
        /// 0 = Controls only the dimension line location
        /// 1 = Controls the text position as well as the dimension line location
        /// </summary>
        public const string DimUpt = "$DIMUPT";

        /// <summary>
        /// Controls suppression of zeros for primary unit values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero feet
        /// 4 = Suppresses leading zeros in decimal dimensions
        /// 8 = Suppresses trailing zeros in decimal dimensions
        /// 12 = Suppresses both leading and trailing zeros
        /// </summary>
        public const string DimZIn = "$DIMZIN";

        /// <summary>
        /// Controls the display of silhouette curves of body objects in Wireframe mode:
        /// 0 = Off
        /// 1 = On
        /// </summary>
        public const string DispSiLh = "$DISPSILH";

        /// <summary>
        /// Hard-pointer ID to visual style while creating 3D solid primitives. The default value is NULL
        /// </summary>
        public const string DragVs = "$DRAGVS";

        /// <summary>
        /// Dimension line gap
        /// </summary>
        public const string DimGap = "$DIMGAP";

        /// <summary>
        /// Horizontal dimension text position:
        /// 0 = Above dimension line and center-justified between extension lines
        /// 1 = Above dimension line and next to first extension line
        /// 2 = Above dimension line and next to second extension line
        /// 3 = Above and center-justified to first extension line
        /// 4 = Above and center-justified to second extension line
        /// </summary>
        public const string DimJust = "$DIMJUST";

        /// <summary>
        /// Arrow block name for leaders
        /// </summary>
        public const string DimLdrBlk = "$DIMLDRBLK";

        /// <summary>
        /// Linear measurements scale factor
        /// 0 = English
        /// 1 = Metric
        /// </summary>
        public const string DimLfAc = "$DIMLFAC";

        /// <summary>
        /// Name of menu file
        /// </summary>
        public const string Menu = "$MENU";

        /// <summary>
        /// Specifies the color of obscured lines.
        /// An obscured line is a hidden line made visible by changing its color and linetype and is visible only when the HIDE or SHADEMODE command is used.
        /// The OBSCUREDCOLOR setting is visible only if the OBSCUREDLTYPE is turned ON by setting it to a value other than 0.
        /// 0 and 256 = Entity color
        /// 1-255 = An AutoCAD color index (ACI)
        /// </summary>
        public const string ObsColor = "$OBSCOLOR";

        /// <summary>
        /// Specifies the linetype of obscured lines. Obscured linetypes are independent of zoom level, unlike standard object
        /// linetypes. Value 0 turns off display of obscured lines and is the default. Linetype values are defined as follows:
        /// 0 = Off
        /// 1 = Solid
        /// 2 = Dashed
        /// 3 = Dotted
        /// 4 = Short Dash
        /// 5 = Medium Dash
        /// 6 = Long Dash
        /// 7 = Double Short Dash
        /// 8 = Double Medium Dash
        /// 9 = Double Long Dash
        /// 10 = Medium Long Dash
        /// 11 = Sparse Dot
        /// </summary>
        public const string ObsLtype = "$OBSLTYPE";

        /// <summary>
        /// Ortho mode on if nonzero
        /// </summary>
        public const string OrthoMode = "$ORTHOMODE";

        /// <summary>
        /// Dimension limits generated if nonzero
        /// </summary>
        public const string DimLim = "$DIMLIM";

        /// <summary>
        /// Sets units for all dimension types except Angular:
        /// 1 = Scientific
        /// 2 = Decimal
        /// 3 = Engineering
        /// 4 = Architectural
        /// 5 = Fractional
        /// 6 = Operating system
        /// </summary>
        public const string DimLUnit = "$DIMLUNIT";

        /// <summary>
        /// Dimension line lineweight:
        /// -3 = Standard
        /// -2 = ByLayer
        /// -1 = ByBlock
        /// 0-211 = an integer representing 100th of mm
        /// </summary>
        public const string DimLwD = "$DIMLWD";

        /// <summary>
        /// Extension line lineweight:
        /// -3 = Standard
        /// -2 = ByLayer
        /// -1 = ByBlock
        /// 0-211 = an integer representing 100th of mm
        /// </summary>
        public const string DimLwE = "$DIMLWE";

        /// <summary>
        /// General dimensioning suffix
        /// </summary>
        public const string DimPost = "$DIMPOST";

        /// <summary>
        /// Rounding value for dimension distances
        /// </summary>
        public const string DimRnd = "$DIMRND";

        /// <summary>
        /// Use separate arrow blocks if nonzero
        /// </summary>
        public const string DimSah = "$DIMSAH";

        /// <summary>
        /// Overall dimensioning scale factor
        /// </summary>
        public const string DimScale = "$DIMSCALE";

        /// <summary>
        /// Suppression of first extension line:
        /// 0 = Not suppressed
        /// 1 = Suppressed
        /// </summary>
        public const string DimSd1 = "$DIMSD1";

        /// <summary>
        /// Suppression of second extension line:
        /// 0 = Not suppressed
        /// 1 = Suppressed
        /// </summary>
        public const string DimSd2 = "$DIMSD2";

        /// <summary>
        /// First extension line suppressed if nonzero
        /// </summary>
        public const string DimSe1 = "$DIMSE1";

        /// <summary>
        /// Second extension line suppressed if nonzero
        /// </summary>
        public const string DimSe2 = "$DIMSE2";

        /// <summary>
        /// 1 = Recompute dimensions while dragging
        /// 0 = Drag original image
        /// </summary>
        public const string DimSho = "$DIMSHO";

        /// <summary>
        /// Suppress outside-extensions dimension lines if nonzero
        /// </summary>
        public const string DimSoXD = "$DIMSOXD";

        /// <summary>
        /// Current dimension style name.
        /// </summary>
        public const string DimStyle = "$DIMSTYLE";

        /// <summary>
        /// Text above dimension line if nonzero
        /// </summary>
        public const string DimTad = "$DIMTAD";

        /// <summary>
        /// Number of decimal places to display the tolerance values
        /// </summary>
        public const string DimTDec = "$DIMTDEC";

        /// <summary>
        /// Default text height.
        /// </summary>
        public const string TextSize = "$TEXTSIZE";

        /// <summary>
        /// Current text style name.
        /// </summary>
        public const string TextStyle = "$TEXTSTYLE";

        /// <summary>
        /// Current thickness set by ELEV command
        /// </summary>
        public const string Thickness = "$THICKNESS";

        /// <summary>
        /// 1 for previous release compatibility mode;
        /// 0 otherwise
        /// </summary>
        public const string TileMode = "$TILEMODE";

        /// <summary>
        /// Default trace width
        /// </summary>
        public const string TraceWid = "$TRACEWID";

        /// <summary>
        /// Specifies the maximum depth of the spatial index
        /// </summary>
        public const string TreeDepth = "$TREEDEPTH";

        /// <summary>
        /// Name of the UCS that defines the origin and orientation of orthographic UCS settings
        /// </summary>
        public const string UcsBase = "$UCSBASE";

        /// <summary>
        /// Name of current UCS
        /// </summary>
        public const string UcsName = "$UCSNAME";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to BACK when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgBack = "$UCSORGBACK";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to BOTTOM when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgBottom = "$UCSORGBOTTOM";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to FRONT when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgFront = "$UCSORGFRONT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to LEFT when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgLeft = "$UCSORGLEFT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to RIGHT when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgRight = "$UCSORGRIGHT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to TOP when UCSBASE is set to WORLD
        /// </summary>
        public const string UcsOrgTop = "$UCSORGTOP";

        /// <summary>
        /// If model space UCS is orthographic (UCSORTHOVIEW not equal to 0),
        /// this is the name of the UCS that the orthographic UCS is relative to. If blank,
        /// UCS is relative to WORLD
        /// </summary>
        public const string UcsOrthoRef = "$UCSORTHOREF";

        /// <summary>
        /// Orthographic view type of model space UCS:
        /// 0 = UCS is not orthographic
        /// 1 = Top
        /// 2 = Bottom
        /// 3 = Front
        /// 4 = Back
        /// 5 = Left
        /// 6 = Right
        /// </summary>
        public const string UcsOrthoView = "$UCSORTHOVIEW";

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
        /// Current elevation set by ELEV command
        /// </summary>
        public const string Elevation = "$ELEVATION";

        /// <summary>
        /// Lineweight endcaps setting for new objects:
        /// 0 = None
        /// 1 = Round
        /// 2 = Angle
        /// 3 = Square
        /// </summary>
        public const string EndCaps = "$ENDCAPS";

        /// <summary>
        /// X, Y, and Z drawing extents upper-right corner (in WCS)
        /// </summary>
        public const string ExtMax = "$EXTMAX";

        /// <summary>
        /// X, Y, and Z drawing extents lower-left corner (in WCS)
        /// </summary>
        public const string ExtMin = "$EXTMIN";

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
        /// Fillet radius
        /// </summary>
        public const string FilletRad = "$FILLETRAD";

        /// <summary>
        /// Fill mode on if nonzero
        /// </summary>
        public const string FillMode = "$FILLMODE";

        /// <summary>
        /// Set at creation time, uniquely identifies a particular drawing
        /// </summary>
        public const string FingerprintGuid = "$FINGERPRINTGUID";

        /// <summary>
        /// Specifies a gap to be displayed where an object is hidden by another object;
        /// the value is specified as a percent of one unit and is independent of the zoom level.
        /// A haloed line is shortened at the point where it is hidden when HIDE or the Hidden option of SHADEMODE is used
        /// </summary>
        public const string HaloGap = "$HALOGAP";

        /// <summary>
        /// Insertion base point for the current drawing.
        /// </summary>
        /// <remarks>
        /// When you insert or externally reference the current drawing into other drawings, this base point is used as the insertion base point.
        /// </remarks>
        public const string InsBase = "$INSBASE";

        /// <summary>
        /// Default drawing units for AutoCAD DesignCenter blocks.
        /// 0 = Unitless
        /// 1 = Inches
        /// 2 = Feet
        /// 3 = Miles
        /// 4 = Millimeters
        /// 5 = Centimeters
        /// 6 = Meters
        /// 7 = Kilometers
        /// 8 = Microinches
        /// 9 = Mils
        /// 10 = Yards
        /// 11 = Angstroms
        /// 12 = Nanometers
        /// 13 = Microns
        /// 14 = Decimeters
        /// 15 = Decameters
        /// 16 = Hectometers
        /// 17 = Gigameters
        /// 18 = Astronomical units
        /// 19 = Light years
        /// 20 = Parsecs
        /// 21 = US Survey Feet
        /// 22 = US Survey Inch
        /// 23 = US Survey Yard
        /// 24 = US Survey Mile
        /// </summary>
        /// <remarks>
        /// Also applies to raster image units, even thought they have the RasterVariables object and units in ImageDefinition.
        /// </remarks>
        public const string InsUnits = "$INSUNITS";

        /// <summary>
        /// Represents the ACI color index of the "interference objects" created during the INTERFERE command. Default value is 1
        /// </summary>
        public const string InterfereColor = "$INTERFERECOLOR";

        /// <summary>
        /// Hard-pointer ID to the visual style for interference objects. Default visual style is Conceptual.
        /// </summary>
        public const string InterfereObjVs = "$INTERFEREOBJVS";

        /// <summary>
        /// Hard-pointer ID to the visual style for the viewport during interference checking. Default visual style is 3d Wireframe.
        /// </summary>
        public const string InterfereVpVs = "$INTERFEREVPVS";

        /// <summary>
        /// Specifies the entity color of intersection polylines:
        /// Values 1-255 designate an AutoCAD color index (ACI)
        /// 0 = Color BYBLOCK
        /// 256 = Color BYLAYER
        /// 257 = Color BYENTITY
        /// </summary>
        public const string IntersectionColor = "$INTERSECTIONCOLOR";

        /// <summary>
        /// Specifies the display of intersection polylines:
        /// 0 = Turns off the display of intersection polylines
        /// 1 = Turns on the display of intersection polylines
        /// </summary>
        public const string IntersectionDisplay = "$INTERSECTIONDISPLAY";

        /// <summary>
        /// Lineweight joint setting for new objects:
        /// 0 = None
        /// 1 = Round
        /// 2 = Angle
        /// 3 = Flat
        /// </summary>
        public const string JoinStyle = "$JOINSTYLE";

        /// <summary>
        /// Nonzero if limits checking is on
        /// </summary>
        public const string LimCheck = "$LIMCHECK";

        /// <summary>
        /// XY drawing limits upper-right corner(in WCS)
        /// </summary>
        public const string LimMax = "$LIMMAX";

        /// <summary>
        /// XY drawing limits lower-left corner (in WCS)
        /// </summary>
        public const string LimMin = "$LIMMIN";

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
        /// Sets maximum number of viewports to be regenerated
        /// </summary>
        public const string MaxActVp = "$MAXACTVP";

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
        /// Current paper space elevation
        /// </summary>
        public const string PElevation = "$PELEVATION";

        /// <summary>
        /// Maximum X, Y, and Z extents for paper space
        /// </summary>
        public const string PExtMan = "$PEXTMAX";

        /// <summary>
        /// Minimum X, Y, and Z extents for paper space
        /// </summary>
        public const string PExtMin = "$PEXTMIN";

        /// <summary>
        /// Paper space insertion base point
        /// </summary>
        public const string PinsBase = "$PINSBASE";

        /// <summary>
        /// Limits checking in paper space when nonzero
        /// </summary>
        public const string PLimCheck = "$PLIMCHECK";

        /// <summary>
        /// Maximum X and Y limits in paper space
        /// </summary>
        public const string PLimMax = "$PLIMMAX";

        /// <summary>
        /// Minimum X and Y limits in paper space
        /// </summary>
        public const string PLimMin = "$PLIMMin";

        /// <summary>
        /// Governs the generation of line type patterns around the vertexes of a 2D polyline.
        /// </summary>
        /// <remarks>
        /// 1 = Line type is generated in a continuous pattern around vertexes of the polyline.<br />
        /// 0 = Each segment of the polyline starts and ends with a dash.
        /// </remarks>
        public const string PLineGen = "$PLINEGEN";

        /// <summary>
        /// Default polyline width
        /// </summary>
        public const string PLineWid = "$PLINEWID";

        /// <summary>
        /// Assigns a project name to the current drawing.
        /// Used when an external reference or image is not found on its original path.
        /// The project name points to a section in the registry that can contain one or more search paths for each project name defined.
        /// Project names and their search directories are created from the Files tab of the Options dialog box
        /// </summary>
        public const string Projectname = "$PROJECTNAME";

        /// <summary>
        /// Controls the saving of proxy object images
        /// </summary>
        public const string ProxyGraphics = "$PROXYGRAPHICS";

        /// <summary>
        /// Controls paper space line type scaling.
        /// </summary>
        /// <remarks>
        /// 1 = No special line type scaling.<br />
        /// 0 = Viewport scaling governs line type scaling.
        /// </remarks>
        public const string PsLtScale = "$PSLTSCALE";

        /// <summary>
        /// Indicates whether the current drawing is in a Color-Dependent or Named Plot Style mode:
        /// 0 = Uses named plot style tables in the current drawing
        /// 1 = Uses color-dependent plot style tables in the current drawing
        /// </summary>
        public const string PStyleMode = "$PSTYLEMODE";

        /// <summary>
        /// View scale factor for new viewports:
        /// 0 = Scaled to fit
        /// >0 = Scale factor (a positive real value)
        /// </summary>
        public const string PSvpScale = "$PSVPSCALE";

        /// <summary>
        /// Name of the UCS that defines the origin and orientation of orthographic UCS settings (paper space only)
        /// </summary>
        public const string PUcsBase = "$PUCSBASE";

        /// <summary>
        /// Current paper space UCS name
        /// </summary>
        public const string PUcsName = "$PUCSNAME";

        /// <summary>
        /// Current paper space UCS origin
        /// </summary>
        public const string PUcsOrg = "$PUCSORG";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to BACK when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgBack = "$PUCSORGBACK";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to BOTTOM when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgBottom = "$PUCSORGBOTTOM";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to FRONT when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgFront = "$PUCSORGFRONT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to LEFT when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgLeft = "$PUCSORGLEFT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to RIGHT when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgRight = "$PUCSORGRIGHT";

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to TOP when PUCSBASE is set to WORLD
        /// </summary>
        public const string PUcsOrgTop = "$PUCSORGTOP";

        /// <summary>
        /// If paper space UCS is orthographic (PUCSORTHOVIEW not equal to 0),
        /// this is the name of the UCS that the orthographic UCS is relative to.
        /// If blank, UCS is relative to WORLD
        /// </summary>
        public const string PUcsOrthoRef = "$PUCSORTHOREF";

        /// <summary>
        /// Orthographic view type of paper space UCS:
        /// 0 = UCS is not orthographic
        /// 1 = Top
        /// 2 = Bottom
        /// 3 = Front
        /// 4 = Back
        /// 5 = Left
        /// 6 = Right
        /// </summary>
        public const string PUcsOrthoView = "$PUCSORTHOVIEW";

        /// <summary>
        /// Current paper space UCS X axis
        /// </summary>
        public const string PUcsXDir = "$PUCSXDIR";

        /// <summary>
        /// Current paper space UCS Y axis
        /// </summary>
        public const string PUcsYDir = "$PUCSYDIR";

        /// <summary>
        /// Quick Text mode on if nonzero
        /// </summary>
        public const string QTextMode = "$QTEXTMODE";

        /// <summary>
        /// REGENAUTO mode on if nonzero
        /// </summary>
        public const string RegenMode = "$REGENMODE";

        /// <summary>
        /// Controls the shading of edges:
        /// 0 = Faces shaded, edges not highlighted
        /// 1 = Faces shaded, edges highlighted in black
        /// 2 = Faces not filled, edges in entity color
        /// 3 = Faces in entity color, edges in black
        /// </summary>
        public const string ShadeEdge = "$SHADEDGE";

        /// <summary>
        /// Percent ambient/diffuse light; range 1-100; default 70
        /// </summary>
        public const string ShadeDif = "$SHADEDIF";

        /// <summary>
        /// Location of the ground shadow plane. This is a Z axis ordinate.
        /// </summary>
        public const string ShadowPlaneLocation = "$SHADOWPLANELOCATION";

        /// <summary>
        /// Sketch record increment
        /// </summary>
        public const string SketchInc = "$SKETCHINC";

        /// <summary>
        /// Determines the object type created by the SKETCH command:
        /// 0 = Generates lines
        /// 1 = Generates polylines
        /// 2 = Generates splines
        /// </summary>
        public const string SkPoly = "$SKPOLY";

        /// <summary>
        /// Controls the object sorting methods;
        /// accessible from the Options dialog box User Preferences tab.
        /// SORTENTS uses the following bitcodes:
        /// 0 = Disables SORTENTS
        /// 1 = Sorts for object selection
        /// 2 = Sorts for object snap
        /// 4 = Sorts for redraws; obsolete
        /// 8 = Sorts for MSLIDE command slide creation; obsolete
        /// 16 = Sorts for REGEN commands
        /// 32 = Sorts for plotting
        /// 64 = Sorts for PostScript output; obsolete
        /// </summary>
        public const string Sortents = "$SORTENTS";

        /// <summary>
        /// Number of line segments per spline patch
        /// </summary>
        public const string SplinesEgs = "$SPLINESEGS";

        /// <summary>
        /// Spline curve type for PEDIT Spline
        /// </summary>
        public const string SplineType = "$SPLINETYPE";

        /// <summary>
        /// Number of mesh tabulations in first direction
        /// </summary>
        public const string SurfTab1 = "$SURFTAB1";

        /// <summary>
        /// Number of mesh tabulations in second direction
        /// </summary>
        public const string SurfTab2 = "$SURFTAB2";

        /// <summary>
        /// Surface type for PEDIT Smooth
        /// </summary>
        public const string SurfType = "$SURFTYPE";

        /// <summary>
        /// Surface density (for PEDIT Smooth) in M direction
        /// </summary>
        public const string SurfU = "$SURFU";

        /// <summary>
        /// Surface density (for PEDIT Smooth) in N direction
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
        /// User-elapsed timer
        /// </summary>
        public const string TdUsrTimer = "$TDUSRTIMER";

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

        /// <summary>
        /// Low bit set = Display fractions, feet-and-inches, and surveyor's angles in input format
        /// </summary>
        public const string UnitMode = "$UNITMODE";

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public const string UserI1 = "USERI1";

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public const string UserI2 = "USERI2";

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public const string UserI3 = "USERI3";

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public const string UserI4 = "USERI4";

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public const string UserI5 = "USERI5";

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public const string UserR1 = "$USERR1";

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public const string UserR2 = "$USERR2";

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public const string UserR3 = "$USERR3";

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public const string UserR4 = "$USERR4";

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public const string UserR5 = "$USERR5";

        /// <summary>
        /// Controls the user timer for the drawing:
        /// 0 = Timer off
        /// 1 = Timer on
        /// </summary>
        public const string UserTimer = "$USRTIMER";

        /// <summary>
        /// Uniquely identifies a particular version of a drawing.
        /// Updated when the drawing is modified
        /// </summary>
        public const string VersionGuid = "$VERSIONGUID";

        /// <summary>
        /// Controls the properties of xref-dependent layers:
        /// 0 = Don't retain xref-dependent visibility settings
        /// 1 = Retain xref-dependent visibility settings
        /// </summary>
        public const string VisRetain = "$VISRETAIN";

        /// <summary>
        /// Determines whether input for the DVIEW and VPOINT command evaluated as relative to the WCS or current UCS:
        /// 0 = Don't change UCS
        /// 1 = Set UCS to WCS during DVIEW/VPOINT
        /// </summary>
        public const string Worldview = "$WORLDVIEW";

        /// <summary>
        /// Controls the visibility of xref clipping boundaries:
        /// 0 = Clipping boundary is not visible
        /// 1 = Clipping boundary is visible
        /// </summary>
        public const string XClipFrame = "$XCLIPFRAME";

        /// <summary>
        /// Controls whether the current drawing can be edited in-place when being referenced by another drawing:
        /// 0 = Can't use in-place reference editing
        /// 1 = Can use in-place reference editing
        /// </summary>
        public const string XEdit = "$XEDIT";
    }
}