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
    #region usings

    using System;
    using System.Collections.Generic;
    using System.Text;
    using netDxf.Units;
    using netDxf.Objects;
    using MLineJustification = Entities.MLineJustification;

    #endregion

    /// <summary>
    /// Represents the header variables of a DXF document.
    /// </summary>
    public class HeaderVariables
    {
        #region private fields

        private readonly Dictionary<string, HeaderVariable> variables;
        private readonly Dictionary<string, HeaderVariable> customVariables;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>SystemVariables</c>.
        /// </summary>
        /// <remarks>The default values are the same ones that are apply to a new AutoCad drawing.</remarks>
        public HeaderVariables()
        {
            variables = new Dictionary<string, HeaderVariable>(StringComparer.OrdinalIgnoreCase)
            {
                {HeaderVariableCode.AcadMaintVer, new HeaderVariable(HeaderVariableCode.AcadMaintVer, 70, (short) 0)},
                {HeaderVariableCode.AcadVer, new HeaderVariable(HeaderVariableCode.AcadVer, 1, DxfVersion.AutoCad2000)},
                {HeaderVariableCode.Angbase, new HeaderVariable(HeaderVariableCode.Angbase, 50, 0.0)},
                {HeaderVariableCode.Angdir, new HeaderVariable(HeaderVariableCode.Angdir, 70, AngleDirection.CCW)},
                {HeaderVariableCode.AttMode, new HeaderVariable(HeaderVariableCode.AttMode, 70, AttMode.Normal)},
                {HeaderVariableCode.AUnits, new HeaderVariable(HeaderVariableCode.AUnits, 70, AngleUnitType.DecimalDegrees)},
                {HeaderVariableCode.AUprec, new HeaderVariable(HeaderVariableCode.AUprec, 70, (short) 0)},
                {HeaderVariableCode.CeColor, new HeaderVariable(HeaderVariableCode.CeColor, 62, AciColor.ByLayer)},
                {HeaderVariableCode.CeLtScale, new HeaderVariable(HeaderVariableCode.CeLtScale, 40, 1.0)},
                {HeaderVariableCode.CeLtype, new HeaderVariable(HeaderVariableCode.CeLtype, 6, "ByLayer")},
                {HeaderVariableCode.CeLweight, new HeaderVariable(HeaderVariableCode.CeLweight, 370, Lineweight.ByLayer)},
                {HeaderVariableCode.CePsnId, new HeaderVariable(HeaderVariableCode.CePsnId, 390, "")},
                {HeaderVariableCode.CePsnType, new HeaderVariable(HeaderVariableCode.CePsnType, 380, PlotStyleType.Dictionary)},
                {HeaderVariableCode.ChamferA, new HeaderVariable(HeaderVariableCode.ChamferA, 40, 0.0)},
                {HeaderVariableCode.ChamferB, new HeaderVariable(HeaderVariableCode.ChamferB, 40, 0.0)},
                {HeaderVariableCode.ChamferC, new HeaderVariable(HeaderVariableCode.ChamferC, 40, 0.0)},
                {HeaderVariableCode.ChamferD, new HeaderVariable(HeaderVariableCode.ChamferD, 40, 0.0)},
                {HeaderVariableCode.CLayer, new HeaderVariable(HeaderVariableCode.CLayer,  8, "0")},
                {HeaderVariableCode.CMLJust, new HeaderVariable(HeaderVariableCode.CMLJust, 70, MLineJustification.Top)},
                {HeaderVariableCode.CMLScale, new HeaderVariable(HeaderVariableCode.CMLScale, 40, 20.0)},
                {HeaderVariableCode.CMLStyle, new HeaderVariable(HeaderVariableCode.CMLStyle, 2, "Standard")},
                {HeaderVariableCode.CShadow, new HeaderVariable(HeaderVariableCode.CShadow, 280, ShadowMode.CastsAndRecives)},
                {HeaderVariableCode.DimAdec, new HeaderVariable(HeaderVariableCode.DimAdec, 70, (short) 0)},
                {HeaderVariableCode.DimAlt, new HeaderVariable(HeaderVariableCode.DimAlt, 70, (short) 0)},
                {HeaderVariableCode.DimAltD, new HeaderVariable(HeaderVariableCode.DimAltD, 70, (short) 0)},
                {HeaderVariableCode.DimAltF, new HeaderVariable(HeaderVariableCode.DimAltF, 40, 0.0)},
                {HeaderVariableCode.DimAltRnd, new HeaderVariable(HeaderVariableCode.DimAltRnd, 40, 0.0)},
                {HeaderVariableCode.DimAltTd, new HeaderVariable(HeaderVariableCode.DimAltTd, 70, (short) 0)},
                {HeaderVariableCode.DimAltTz, new HeaderVariable(HeaderVariableCode.DimAltTz, 70, (short) 0)},
                {HeaderVariableCode.DimAltU, new HeaderVariable(HeaderVariableCode.DimAltU, 70, UnitFormatsDimStyle.Decimal)},
                {HeaderVariableCode.DimAltZ, new HeaderVariable(HeaderVariableCode.DimAltZ, 70, (short) 0)},
                {HeaderVariableCode.DimAPost, new HeaderVariable(HeaderVariableCode.DimAPost, 1, string.Empty)},
                {HeaderVariableCode.DimAso, new HeaderVariable(HeaderVariableCode.DimAso, 70, (short) 0)},
                {HeaderVariableCode.DimAssoc, new HeaderVariable(HeaderVariableCode.DimAssoc, 280, (short) 1)},
                {HeaderVariableCode.DimAsz, new HeaderVariable(HeaderVariableCode.DimAsz, 40, 2.5)},
                {HeaderVariableCode.DimAtFit, new HeaderVariable(HeaderVariableCode.DimAtFit, 70, (short) 0)},
                {HeaderVariableCode.DimAUnit, new HeaderVariable(HeaderVariableCode.DimAUnit, 70, (short) 0)},
                {HeaderVariableCode.DimAZin, new HeaderVariable(HeaderVariableCode.DimAZin, 70, (short) 0)},
                {HeaderVariableCode.DimBlk, new HeaderVariable(HeaderVariableCode.DimBlk, 1, string.Empty)},
                {HeaderVariableCode.DimBlk1, new HeaderVariable(HeaderVariableCode.DimBlk1, 1, string.Empty)},
                {HeaderVariableCode.DimBlk2, new HeaderVariable(HeaderVariableCode.DimBlk2, 1, string.Empty)},
                {HeaderVariableCode.DimCen, new HeaderVariable(HeaderVariableCode.DimCen, 40, 0.0)},
                {HeaderVariableCode.DimClrd, new HeaderVariable(HeaderVariableCode.DimClrd, 70, (short) 0)},
                {HeaderVariableCode.DimClRe, new HeaderVariable(HeaderVariableCode.DimClRe, 70, (short) 0)},
                {HeaderVariableCode.DimClRt, new HeaderVariable(HeaderVariableCode.DimClRt, 70, (short) 0)},
                {HeaderVariableCode.DimDec, new HeaderVariable(HeaderVariableCode.DimDec, 70, (short) 0)},
                {HeaderVariableCode.DimDle, new HeaderVariable(HeaderVariableCode.DimDle, 40, 0.0)},
                {HeaderVariableCode.DimDli, new HeaderVariable(HeaderVariableCode.DimDli, 40, 0.0)},
                {HeaderVariableCode.DimDsep, new HeaderVariable(HeaderVariableCode.DimDsep, 70, (short) 0)},
                {HeaderVariableCode.DimExE, new HeaderVariable(HeaderVariableCode.DimExE, 40, 0.0)},
                {HeaderVariableCode.DimExO, new HeaderVariable(HeaderVariableCode.DimExO, 40, 0.0)},
                {HeaderVariableCode.DimFac, new HeaderVariable(HeaderVariableCode.DimFac, 40, 0.0)},
                {HeaderVariableCode.DimGap, new HeaderVariable(HeaderVariableCode.DimGap, 40, 0.0)},
                {HeaderVariableCode.DimJust, new HeaderVariable(HeaderVariableCode.DimJust, 70, (short) 0)},
                {HeaderVariableCode.DimLdrBlk, new HeaderVariable(HeaderVariableCode.DimLdrBlk, 1, string.Empty)},
                {HeaderVariableCode.DimLfAc, new HeaderVariable(HeaderVariableCode.DimLfAc, 40, 0.0)},
                {HeaderVariableCode.DimLim, new HeaderVariable(HeaderVariableCode.DimLim, 70, (short) 0)},
                {HeaderVariableCode.DimLUnit, new HeaderVariable(HeaderVariableCode.DimLUnit, 70, (short) 0)},
                {HeaderVariableCode.DimLwD, new HeaderVariable(HeaderVariableCode.DimLwD, 70, (short) 0)},
                {HeaderVariableCode.DimLwE, new HeaderVariable(HeaderVariableCode.DimLwE, 70, (short) 0)},
                {HeaderVariableCode.DimPost, new HeaderVariable(HeaderVariableCode.DimPost, 1, string.Empty)},
                {HeaderVariableCode.DimRnd, new HeaderVariable(HeaderVariableCode.DimRnd, 40, 0.0)},
                {HeaderVariableCode.DimSah, new HeaderVariable(HeaderVariableCode.DimSah, 70, (short) 0)},
                {HeaderVariableCode.DimScale, new HeaderVariable(HeaderVariableCode.DimScale, 40, 0.0)},
                {HeaderVariableCode.DimSd1, new HeaderVariable(HeaderVariableCode.DimSd1, 70, (short) 0)},
                {HeaderVariableCode.DimSd2, new HeaderVariable(HeaderVariableCode.DimSd2, 70, (short) 0)},
                {HeaderVariableCode.DimSe1, new HeaderVariable(HeaderVariableCode.DimSe1, 70, (short) 0)},
                {HeaderVariableCode.DimSe2, new HeaderVariable(HeaderVariableCode.DimSe2, 70, (short) 0)},
                {HeaderVariableCode.DimSho, new HeaderVariable(HeaderVariableCode.DimSho, 70, (short) 0)},
                {HeaderVariableCode.DimSoXD, new HeaderVariable(HeaderVariableCode.DimSoXD, 70, (short) 0)},
                {HeaderVariableCode.DimStyle, new HeaderVariable(HeaderVariableCode.DimStyle, 2, "Standard")},
                {HeaderVariableCode.DimTad, new HeaderVariable(HeaderVariableCode.DimTad, 70, (short) 0)},
                {HeaderVariableCode.DimTDec, new HeaderVariable(HeaderVariableCode.DimTDec, 70, (short) 0)},
                {HeaderVariableCode.DimTFac, new HeaderVariable(HeaderVariableCode.DimTFac, 40, 0.0)},
                {HeaderVariableCode.DimTih, new HeaderVariable(HeaderVariableCode.DimTih, 70, (short) 0)},
                {HeaderVariableCode.DimTix, new HeaderVariable(HeaderVariableCode.DimTix, 70, (short) 0)},
                {HeaderVariableCode.DimTm, new HeaderVariable(HeaderVariableCode.DimTm, 40, 0.0)},
                {HeaderVariableCode.DimTMove, new HeaderVariable(HeaderVariableCode.DimTMove, 70, (short) 0)},
                {HeaderVariableCode.DimToFl, new HeaderVariable(HeaderVariableCode.DimToFl, 70, (short) 0)},
                {HeaderVariableCode.DimToH, new HeaderVariable(HeaderVariableCode.DimToH, 70, (short) 0)},
                {HeaderVariableCode.DimToL, new HeaderVariable(HeaderVariableCode.DimToL, 70, (short) 0)},
                {HeaderVariableCode.DimToLj, new HeaderVariable(HeaderVariableCode.DimToLj, 70, (short) 0)},
                {HeaderVariableCode.DimTp, new HeaderVariable(HeaderVariableCode.DimTp, 40, 0.0)},
                {HeaderVariableCode.DimTsz, new HeaderVariable(HeaderVariableCode.DimTsz, 40, 0.0)},
                {HeaderVariableCode.DimTvp, new HeaderVariable(HeaderVariableCode.DimTvp, 40, 0.0)},
                {HeaderVariableCode.DimTxSty, new HeaderVariable(HeaderVariableCode.DimTxSty, 7, "Standard")},
                {HeaderVariableCode.DimTxt, new HeaderVariable(HeaderVariableCode.DimTxt, 40, 2.5)},
                {HeaderVariableCode.DimTzIn, new HeaderVariable(HeaderVariableCode.DimTzIn, 70, (short) 0)},
                {HeaderVariableCode.DimUpt, new HeaderVariable(HeaderVariableCode.DimUpt, 70, (short) 0)},
                {HeaderVariableCode.DimZIn, new HeaderVariable(HeaderVariableCode.DimZIn, 70, (short) 0)},
                {HeaderVariableCode.DispSiLh, new HeaderVariable(HeaderVariableCode.DispSiLh, 70, (short) 0)},
                {HeaderVariableCode.DragVs, new HeaderVariable(HeaderVariableCode.DragVs, 349, null)},
                {HeaderVariableCode.DwgCodePage, new HeaderVariable(HeaderVariableCode.DwgCodePage, 3, "ANSI_" + Encoding.Default.WindowsCodePage)},
                {HeaderVariableCode.Elevation, new HeaderVariable(HeaderVariableCode.Elevation, 40, 0.0)},
                {HeaderVariableCode.EndCaps, new HeaderVariable(HeaderVariableCode.EndCaps, 280, (short) 0)},
                {HeaderVariableCode.ExtMax, new HeaderVariable(HeaderVariableCode.ExtMax, 10, Vector3.Zero)},
                {HeaderVariableCode.ExtMin, new HeaderVariable(HeaderVariableCode.ExtMin, 10, Vector3.Zero)},
                {HeaderVariableCode.Extnames, new HeaderVariable(HeaderVariableCode.Extnames, 290, true)},
                {HeaderVariableCode.FilletRad, new HeaderVariable(HeaderVariableCode.FilletRad, 40, 0.0)},
                {HeaderVariableCode.FillMode, new HeaderVariable(HeaderVariableCode.FillMode, 70, (short) 0)},
                {HeaderVariableCode.FingerprintGuid, new HeaderVariable(HeaderVariableCode.FingerprintGuid, 2, null)},
                {HeaderVariableCode.HaloGap, new HeaderVariable(HeaderVariableCode.HaloGap, 280, (short) 0)},
                {HeaderVariableCode.HandleSeed, new HeaderVariable(HeaderVariableCode.HandleSeed, 5, "1")},
                {HeaderVariableCode.HideText, new HeaderVariable(HeaderVariableCode.HideText, 280, (short) 0)},
                {HeaderVariableCode.HyperlinkBase, new HeaderVariable(HeaderVariableCode.HyperlinkBase, 1, null)},
                {HeaderVariableCode.IndexCtl, new HeaderVariable(HeaderVariableCode.IndexCtl, 280, (short) 0)},
                {HeaderVariableCode.InsBase, new HeaderVariable(HeaderVariableCode.InsBase, 10, Vector3.Zero)},
                {HeaderVariableCode.InsUnits, new HeaderVariable(HeaderVariableCode.InsUnits, 70, DrawingUnits.Unitless)},
                {HeaderVariableCode.InterfereColor, new HeaderVariable(HeaderVariableCode.InterfereColor, 62, (short) 1)},
                {HeaderVariableCode.InterfereObjVs, new HeaderVariable(HeaderVariableCode.InterfereObjVs, 345, null)},
                {HeaderVariableCode.InterfereVpVs, new HeaderVariable(HeaderVariableCode.InterfereVpVs, 346, null)},
                {HeaderVariableCode.IntersectionColor, new HeaderVariable(HeaderVariableCode.IntersectionColor, 70, (short) 256)},
                {HeaderVariableCode.IntersectionDisplay, new HeaderVariable(HeaderVariableCode.IntersectionDisplay, 280, (short) 0)},
                {HeaderVariableCode.JoinStyle, new HeaderVariable(HeaderVariableCode.JoinStyle, 280, (short) 0)},
                {HeaderVariableCode.LimCheck, new HeaderVariable(HeaderVariableCode.LimCheck, 70, (short) 0)},
                {HeaderVariableCode.LimMax, new HeaderVariable(HeaderVariableCode.LimMax, 10, Vector2.Zero)},
                {HeaderVariableCode.LimMin, new HeaderVariable(HeaderVariableCode.LimMin, 10, Vector2.Zero)},
                {HeaderVariableCode.LtScale, new HeaderVariable(HeaderVariableCode.LtScale, 40, 1.0)},
                {HeaderVariableCode.LUnits, new HeaderVariable(HeaderVariableCode.LUnits, 70, LinearUnitType.Decimal)},
                {HeaderVariableCode.LUprec, new HeaderVariable(HeaderVariableCode.LUprec, 70, (short) 4)},
                {HeaderVariableCode.LwDisplay, new HeaderVariable(HeaderVariableCode.LwDisplay, 290, false)},
                {HeaderVariableCode.MaxActVp, new HeaderVariable(HeaderVariableCode.MaxActVp, 70, (short) 1)},
                {HeaderVariableCode.Measurement, new HeaderVariable(HeaderVariableCode.Measurement, 70, (short) 1)},
                {HeaderVariableCode.Menu, new HeaderVariable(HeaderVariableCode.Menu, 1, string.Empty)},
                {HeaderVariableCode.MirrText, new HeaderVariable(HeaderVariableCode.MirrText, 70, false)},
                {HeaderVariableCode.ObsColor, new HeaderVariable(HeaderVariableCode.ObsColor, 70, false)},
                {HeaderVariableCode.ObsLtype, new HeaderVariable(HeaderVariableCode.ObsLtype, 280, (short) 0)},
                {HeaderVariableCode.OrthoMode, new HeaderVariable(HeaderVariableCode.OrthoMode, 70, (short) 0)},
                {HeaderVariableCode.PdMode, new HeaderVariable(HeaderVariableCode.PdMode, 70, PointShape.Dot)},
                {HeaderVariableCode.PdSize, new HeaderVariable(HeaderVariableCode.PdSize, 40, 0.0)},
                {HeaderVariableCode.PElevation, new HeaderVariable(HeaderVariableCode.PElevation, 40, 0.0)},
                {HeaderVariableCode.PExtMax, new HeaderVariable(HeaderVariableCode.PExtMax, 10, Vector3.Zero)},
                {HeaderVariableCode.PExtMin, new HeaderVariable(HeaderVariableCode.PExtMin, 10, Vector3.Zero)},
                {HeaderVariableCode.PinsBase, new HeaderVariable(HeaderVariableCode.PinsBase, 10, Vector3.Zero)},
                {HeaderVariableCode.PLimCheck, new HeaderVariable(HeaderVariableCode.PLimCheck, 70, (short) 0)},
                {HeaderVariableCode.PLimMax, new HeaderVariable(HeaderVariableCode.PLimMax, 10, Vector2.Zero)},
                {HeaderVariableCode.PLimMin, new HeaderVariable(HeaderVariableCode.PLimMin, 10, Vector2.Zero)},
                {HeaderVariableCode.PLineGen, new HeaderVariable(HeaderVariableCode.PLineGen, 70, (short) 0)},
                {HeaderVariableCode.PLineWid, new HeaderVariable(HeaderVariableCode.PLineWid, 40, 1.0)},
                {HeaderVariableCode.Projectname, new HeaderVariable(HeaderVariableCode.Projectname, 1, string.Empty)},
                {HeaderVariableCode.ProxyGraphics, new HeaderVariable(HeaderVariableCode.ProxyGraphics, 70, (short) 0)},
                {HeaderVariableCode.PsLtScale, new HeaderVariable(HeaderVariableCode.PsLtScale, 70, (short) 1)},
                {HeaderVariableCode.PStyleMode, new HeaderVariable(HeaderVariableCode.PStyleMode, 290, false)},
                {HeaderVariableCode.PSvpScale, new HeaderVariable(HeaderVariableCode.PSvpScale, 40, 0.0)},
                {HeaderVariableCode.PUcsBase, new HeaderVariable(HeaderVariableCode.PUcsBase, 2, string.Empty)},
                {HeaderVariableCode.PUcsName, new HeaderVariable(HeaderVariableCode.PUcsName, 2, string.Empty)},
                {HeaderVariableCode.PUcsOrg, new HeaderVariable(HeaderVariableCode.PUcsOrg, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgBack, new HeaderVariable(HeaderVariableCode.PUcsOrgBack, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgBottom, new HeaderVariable(HeaderVariableCode.PUcsOrgBottom, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgFront, new HeaderVariable(HeaderVariableCode.PUcsOrgFront, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgLeft, new HeaderVariable(HeaderVariableCode.PUcsOrgLeft, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgRight, new HeaderVariable(HeaderVariableCode.PUcsOrgRight, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrgTop, new HeaderVariable(HeaderVariableCode.PUcsOrgTop, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsOrthoRef, new HeaderVariable(HeaderVariableCode.PUcsOrthoRef, 2, string.Empty)},
                {HeaderVariableCode.PUcsOrthoView, new HeaderVariable(HeaderVariableCode.PUcsOrthoView, 70, (short) 0)},
                {HeaderVariableCode.PUcsXDir, new HeaderVariable(HeaderVariableCode.PUcsXDir, 10, Vector3.Zero)},
                {HeaderVariableCode.PUcsYDir, new HeaderVariable(HeaderVariableCode.PUcsYDir, 10, Vector3.Zero)},
                {HeaderVariableCode.QTextMode, new HeaderVariable(HeaderVariableCode.QTextMode, 70, (short) 0)},
                {HeaderVariableCode.RegenMode, new HeaderVariable(HeaderVariableCode.RegenMode, 70, (short) 0)},
                {HeaderVariableCode.ShadeEdge, new HeaderVariable(HeaderVariableCode.ShadeEdge, 70, (short) 0)},
                {HeaderVariableCode.ShadeDif, new HeaderVariable(HeaderVariableCode.ShadeDif, 70, (short) 70)},
                {HeaderVariableCode.ShadowPlaneLocation, new HeaderVariable(HeaderVariableCode.ShadowPlaneLocation, 40, 0.0)},
                {HeaderVariableCode.SketchInc, new HeaderVariable(HeaderVariableCode.SketchInc, 40, 0.0)},
                {HeaderVariableCode.SkPoly, new HeaderVariable(HeaderVariableCode.SkPoly, 70, (short) 70)},
                {HeaderVariableCode.Sortents, new HeaderVariable(HeaderVariableCode.Sortents, 280, (short) 0)},
                {HeaderVariableCode.SplinesEgs, new HeaderVariable(HeaderVariableCode.SplinesEgs, 70, (short) 0)},
                {HeaderVariableCode.SplineType, new HeaderVariable(HeaderVariableCode.SplineType, 70, (short) 0)},
                {HeaderVariableCode.SurfTab1, new HeaderVariable(HeaderVariableCode.SurfTab1, 70, (short) 0)},
                {HeaderVariableCode.SurfTab2, new HeaderVariable(HeaderVariableCode.SurfTab2, 70, (short) 0)},
                {HeaderVariableCode.SurfType, new HeaderVariable(HeaderVariableCode.SurfType, 70, (short) 0)},
                {HeaderVariableCode.SurfU, new HeaderVariable(HeaderVariableCode.SurfU, 70, (short) 0)},
                {HeaderVariableCode.SurfV, new HeaderVariable(HeaderVariableCode.SurfV, 70, (short) 0)},
                {HeaderVariableCode.TdCreate, new HeaderVariable(HeaderVariableCode.TdCreate, 40, DateTime.Now)},
                {HeaderVariableCode.TdinDwg, new HeaderVariable(HeaderVariableCode.TdinDwg, 40, new TimeSpan())},
                {HeaderVariableCode.TduCreate, new HeaderVariable(HeaderVariableCode.TduCreate, 40, DateTime.UtcNow)},
                {HeaderVariableCode.TdUpdate, new HeaderVariable(HeaderVariableCode.TdUpdate, 40, DateTime.Now)},
                {HeaderVariableCode.TdUsrTimer, new HeaderVariable(HeaderVariableCode.TdUsrTimer, 40, (short) 0)},
                {HeaderVariableCode.TduUpdate, new HeaderVariable(HeaderVariableCode.TduUpdate, 40, DateTime.UtcNow)},
                {HeaderVariableCode.TextSize, new HeaderVariable(HeaderVariableCode.TextSize, 40, 2.5)},
                {HeaderVariableCode.TextStyle, new HeaderVariable(HeaderVariableCode.TextStyle, 7, "Standard")},
                {HeaderVariableCode.Thickness, new HeaderVariable(HeaderVariableCode.Thickness, 40, 1.0)},
                {HeaderVariableCode.TileMode, new HeaderVariable(HeaderVariableCode.TileMode, 70, (short) 0)},
                {HeaderVariableCode.TraceWid, new HeaderVariable(HeaderVariableCode.TraceWid, 40, 1.0)},
                {HeaderVariableCode.TreeDepth, new HeaderVariable(HeaderVariableCode.TreeDepth, 70, (short) 0)},
                {HeaderVariableCode.UcsBase, new HeaderVariable(HeaderVariableCode.UcsBase, 2, string.Empty)},
                {HeaderVariableCode.UcsName, new HeaderVariable(HeaderVariableCode.UcsName, 2, string.Empty)},
                {HeaderVariableCode.UcsOrg, new HeaderVariable(HeaderVariableCode.UcsOrg, 30, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgBack, new HeaderVariable(HeaderVariableCode.UcsOrgBack, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgBottom, new HeaderVariable(HeaderVariableCode.UcsOrgBottom, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgFront, new HeaderVariable(HeaderVariableCode.UcsOrgFront, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgLeft, new HeaderVariable(HeaderVariableCode.UcsOrgLeft, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgRight, new HeaderVariable(HeaderVariableCode.UcsOrgRight, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrgTop, new HeaderVariable(HeaderVariableCode.UcsOrgTop, 10, Vector3.Zero)},
                {HeaderVariableCode.UcsOrthoRef, new HeaderVariable(HeaderVariableCode.UcsOrthoRef, 2, string.Empty)},
                {HeaderVariableCode.UcsOrthoView, new HeaderVariable(HeaderVariableCode.UcsOrthoView, 70, (short) 0)},
                {HeaderVariableCode.UcsXDir, new HeaderVariable(HeaderVariableCode.UcsXDir, 30, Vector3.UnitX)},
                {HeaderVariableCode.UcsYDir, new HeaderVariable(HeaderVariableCode.UcsYDir, 30, Vector3.UnitY)},
                {HeaderVariableCode.UnitMode, new HeaderVariable(HeaderVariableCode.UnitMode, 70, (short) 0)},
                {HeaderVariableCode.UserI1, new HeaderVariable(HeaderVariableCode.UserI1, 70, (short) 0)},
                {HeaderVariableCode.UserI2, new HeaderVariable(HeaderVariableCode.UserI2, 70, (short) 0)},
                {HeaderVariableCode.UserI3, new HeaderVariable(HeaderVariableCode.UserI3, 70, (short) 0)},
                {HeaderVariableCode.UserI4, new HeaderVariable(HeaderVariableCode.UserI4, 70, (short) 0)},
                {HeaderVariableCode.UserI5, new HeaderVariable(HeaderVariableCode.UserI5, 70, (short) 0)},
                {HeaderVariableCode.UserR1, new HeaderVariable(HeaderVariableCode.UserR1, 40, 0.0)},
                {HeaderVariableCode.UserR2, new HeaderVariable(HeaderVariableCode.UserR1, 40, 0.0)},
                {HeaderVariableCode.UserR3, new HeaderVariable(HeaderVariableCode.UserR1, 40, 0.0)},
                {HeaderVariableCode.UserR4, new HeaderVariable(HeaderVariableCode.UserR1, 40, 0.0)},
                {HeaderVariableCode.UserR5, new HeaderVariable(HeaderVariableCode.UserR1, 40, 0.0)},
                {HeaderVariableCode.UserTimer, new HeaderVariable(HeaderVariableCode.UserTimer, 70, (short) 0)},
                {HeaderVariableCode.VersionGuid, new HeaderVariable(HeaderVariableCode.VersionGuid, 2, string.Empty)},
                {HeaderVariableCode.VisRetain, new HeaderVariable(HeaderVariableCode.VisRetain, 70, (short) 0)},
                {HeaderVariableCode.Worldview, new HeaderVariable(HeaderVariableCode.Worldview, 70, (short) 0)},
                {HeaderVariableCode.XClipFrame, new HeaderVariable(HeaderVariableCode.XClipFrame, 280, (short) 0)},
                {HeaderVariableCode.XEdit, new HeaderVariable(HeaderVariableCode.XEdit, 290, false)},
                {HeaderVariableCode.LastSavedBy, new HeaderVariable(HeaderVariableCode.LastSavedBy, 1, Environment.UserName)}
            };

            customVariables = new Dictionary<string, HeaderVariable>(StringComparer.OrdinalIgnoreCase);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Units precision for coordinates and distances.
        /// </summary>
        /// <remarks>Valid values are integers from 0 to 8. Default value: 4.</remarks>
        public short AcadMaintVer
        {
            get => (short)variables[HeaderVariableCode.AcadMaintVer].Value;
            set
            {
                variables[HeaderVariableCode.AcadMaintVer].Value = value;
            }
        }

        /// <summary>
        /// The AutoCAD drawing database version number.
        /// </summary>
        /// <remarks>Only AutoCad2000 and higher dxf versions are supported.</remarks>
        /// <exception cref="NotSupportedException">Only AutoCad2000 and higher dxf versions are supported.</exception>
        public DxfVersion AcadVer
        {
            get => (DxfVersion) variables[HeaderVariableCode.AcadVer].Value;
            set
            {
                if (value < DxfVersion.AutoCad2000)
                {
                    throw new NotSupportedException("Only AutoCad2000 and newer DXF versions are supported.");
                }

                variables[HeaderVariableCode.AcadVer].Value = value;
            }
        }

        /// <summary>
        /// Angle 0 base.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public double Angbase
        {
            get => (double)variables[HeaderVariableCode.Angbase].Value;
            internal set { variables[HeaderVariableCode.Angbase].Value = value; }
        }

        /// <summary>
        /// The angle direction.
        /// </summary>
        /// <remarks>Default value: CCW.</remarks>
        public AngleDirection Angdir
        {
            get => (AngleDirection)variables[HeaderVariableCode.Angdir].Value;
            internal set { variables[HeaderVariableCode.Angdir].Value = value; }
        }

        /// <summary>
        /// Attribute visibility.
        /// </summary>
        /// <remarks>Default value: Normal.</remarks>
        public AttMode AttMode
        {
            get => (AttMode)variables[HeaderVariableCode.AttMode].Value;
            set => variables[HeaderVariableCode.AttMode].Value = value;
        }

        /// <summary>
        /// Units format for angles.
        /// </summary>
        /// <remarks>Default value: Decimal degrees.</remarks>
        public AngleUnitType AUnits
        {
            get => (AngleUnitType)variables[HeaderVariableCode.AUnits].Value;
            set { variables[HeaderVariableCode.AUnits].Value = value; }
        }

        /// <summary>
        /// Units precision for angles.
        /// </summary>
        /// <remarks>Valid values are integers from 0 to 8. Default value: 0.</remarks>
        public short AUprec
        {
            get => (short)variables[HeaderVariableCode.AUprec].Value;
            set
            {
                if (value < 0 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Valid values are integers from 0 to 8.");
                }

                variables[HeaderVariableCode.AUprec].Value = value;
            }
        }

        /// <summary>
        /// Current entity color.
        /// </summary>
        /// <remarks>Default value: 256 (ByLayer). This header variable only supports indexed colors.</remarks>
        public AciColor CeColor
        {
            get => (AciColor)variables[HeaderVariableCode.CeColor].Value;
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                variables[HeaderVariableCode.CeColor].Value = value;
            }
        }

        /// <summary>
        /// Current entity line type scale.
        /// </summary>
        /// <remarks>Default value: 1.0.</remarks>
        public double CeLtScale
        {
            get => (double)variables[HeaderVariableCode.CeLtScale].Value;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The current entity line type scale must be greater than zero.");
                }

                variables[HeaderVariableCode.CeLtScale].Value = value;
            }
        }

        /// <summary>
        /// Current entity line type name.
        /// </summary>
        /// <remarks>Default value: ByLayer.</remarks>
        public string CeLtype
        {
            get => (string)variables[HeaderVariableCode.CeLtype].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current entity line type name should be at least one character long.");
                }

                variables[HeaderVariableCode.CeLtype].Value = value;
            }
        }

        /// <summary>
        /// Current entity line weight.
        /// </summary>
        /// <remarks>Default value: -1 (ByLayer).</remarks>
        public Lineweight CeLweight
        {
            get => (Lineweight)variables[HeaderVariableCode.CeLweight].Value;
            set => variables[HeaderVariableCode.CeLweight].Value = value;
        }

        /// <summary>
        /// Plotstyle handle of new objects;
        /// if CEPSNTYPE is 3,
        /// then this value indicates the handle
        /// </summary>
        public string CePsnId
        {
            get => (string)variables[HeaderVariableCode.CePsnId].Value;
            internal set { variables[HeaderVariableCode.CePsnId].Value = value; }
        }

        /// <summary>
        /// Plot style type of new objects:
        /// 0 = Plot style by layer
        /// 1 = Plot style by block
        /// 2 = Plot style by dictionary default
        /// 3 = Plot style by object ID/handle
        /// </summary>
        public PlotStyleType CePsnType
        {
            get => (PlotStyleType)variables[HeaderVariableCode.CePsnType].Value;
            internal set { variables[HeaderVariableCode.CePsnType].Value = value; }
        }

        /// <summary>
        /// First chamfer distance
        /// </summary>
        public double ChamferA
        {
            get => (double)variables[HeaderVariableCode.ChamferA].Value;
            set => variables[HeaderVariableCode.ChamferA].Value = value;
        }

        /// <summary>
        /// Second chamfer distance
        /// </summary>
        public double ChamferB
        {
            get => (double)variables[HeaderVariableCode.ChamferB].Value;
            set => variables[HeaderVariableCode.ChamferB].Value = value;
        }

        /// <summary>
        /// Chamfer length
        /// </summary>
        public double ChamferC
        {
            get => (double)variables[HeaderVariableCode.ChamferC].Value;
            set => variables[HeaderVariableCode.ChamferC].Value = value;
        }

        /// <summary>
        /// Chamfer angle
        /// </summary>
        public double ChamferD
        {
            get => (double)variables[HeaderVariableCode.ChamferD].Value;
            set => variables[HeaderVariableCode.ChamferD].Value = value;
        }

        /// <summary>
        /// Current layer name.
        /// </summary>
        /// <remarks>Default value: 0.</remarks>
        public string CLayer
        {
            get => (string)variables[HeaderVariableCode.CLayer].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current layer name should be at least one character long.");
                }

                variables[HeaderVariableCode.CLayer].Value = value;
            }
        }

        /// <summary>
        /// Current multiline justification.
        /// </summary>
        /// <remarks>Default value: 0 (Top).</remarks>
        public MLineJustification CMLJust
        {
            get => (MLineJustification)variables[HeaderVariableCode.CMLJust].Value;
            set => variables[HeaderVariableCode.CMLJust].Value = value;
        }

        /// <summary>
        /// Current multiline scale.
        /// </summary>
        /// <remarks>Default value: 20.</remarks>
        public double CMLScale
        {
            get => (double)variables[HeaderVariableCode.CMLScale].Value;
            set => variables[HeaderVariableCode.CMLScale].Value = value;
        }

        /// <summary>
        /// Current multiline style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string CMLStyle
        {
            get => (string)variables[HeaderVariableCode.CMLStyle].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current multiline style name should be at least one character long.");
                }

                variables[HeaderVariableCode.CMLStyle].Value = value;
            }
        }

        /// <summary>
        /// Shadow mode for a 3D object:
        /// 0 = Casts and receives shadows
        /// 1 = Casts shadows
        /// 2 = Receives shadows
        /// 3 = Ignores shadows
        /// Note: Starting with AutoCAD 2016-based products, this variable is obsolete
        ///       but still supported for backwards compatibility.
        /// </summary>
        public ShadowMode CShadow
        {
            get => (ShadowMode)variables[HeaderVariableCode.CShadow].Value;
            set => variables[HeaderVariableCode.CShadow].Value = value;
        }

        /// <summary>
        /// Number of precision places displayed in angular dimensions
        /// </summary>
        public short DimAdec
        {
            get => (short)variables[HeaderVariableCode.DimAdec].Value;
            set => variables[HeaderVariableCode.DimAdec].Value = value;
        }

        /// <summary>
        /// Alternate unit dimensioning performed if nonzero
        /// </summary>
        public short DimAlt
        {
            get => (short)variables[HeaderVariableCode.DimAlt].Value;
            set => variables[HeaderVariableCode.DimAlt].Value = value;
        }

        /// <summary>
        /// Alternate unit decimal places
        /// </summary>
        public short DimAltD
        {
            get => (short)variables[HeaderVariableCode.DimAltD].Value;
            set => variables[HeaderVariableCode.DimAltD].Value = value;
        }

        /// <summary>
        /// Alternate unit scale factor
        /// </summary>
        public double DimAltF
        {
            get => (double)variables[HeaderVariableCode.DimAltF].Value;
            set => variables[HeaderVariableCode.DimAltF].Value = value;
        }

        /// <summary>
        /// Determines rounding of alternate units
        /// </summary>
        public double DimAltRnd
        {
            get => (double)variables[HeaderVariableCode.DimAltRnd].Value;
            set => variables[HeaderVariableCode.DimAltRnd].Value = value;
        }

        /// <summary>
        /// Number of decimal places for tolerance values of an alternate units dimension
        /// </summary>
        public short DimAltTd
        {
            get => (short)variables[HeaderVariableCode.DimAltTd].Value;
            set => variables[HeaderVariableCode.DimAltTd].Value = value;
        }

        /// <summary>
        /// Controls suppression of zeros for alternate tolerance values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero feet
        /// 
        /// To suppress leading or trailing zeros, add the following values to one of
        /// the preceding values:
        /// 4 = Suppresses leading zeros
        /// 8 = Suppresses trailing zeros
        /// </summary>
        public short DimAltTz
        {
            get => (short)variables[HeaderVariableCode.DimAltTz].Value;
            set => variables[HeaderVariableCode.DimAltTz].Value = value;
        }

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
        public UnitFormatsDimStyle DimAltU
        {
            get => (UnitFormatsDimStyle)variables[HeaderVariableCode.DimAltU].Value;
            set
            {
                variables[HeaderVariableCode.DimAltU].Value = value;
            }
        }

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
        public short DimAltZ
        {
            get => (short)variables[HeaderVariableCode.DimAltZ].Value;
            set => variables[HeaderVariableCode.DimAltZ].Value = value;
        }

        /// <summary>
        /// Alternate dimensioning suffix
        /// </summary>
        public string DimAPost
        {
            get => (string)variables[HeaderVariableCode.DimAPost].Value;
            set => variables[HeaderVariableCode.DimAPost].Value = value;
        }

        /// <summary>
        /// 1 = Create associative dimensioning
        /// 0 = Draw individual entities
        /// Note: Obsolete; see $DIMASSOC.
        /// </summary>
        public short DimAso
        {
            get => (short)variables[HeaderVariableCode.DimAso].Value;
            set => variables[HeaderVariableCode.DimAso].Value = value;
        }

        /// <summary>
        /// Controls the associativity of dimension objects
        /// 0 = Creates exploded dimensions; there is no association between elements of the dimension, and the lines, arcs, arrowheads, and text of a dimension are drawn as separate objects
        /// 1 = Creates non-associative dimension objects; the elements of the dimension are formed into a single object, and if the definition point on the object moves, then the dimension value is updated
        /// 2 = Creates associative dimension objects; the elements of the dimension are formed into a single object and one or more definition points of the dimension are coupled with association points on geometric objects
        /// </summary>
        public short DimAssoc
        {
            get => (short)variables[HeaderVariableCode.DimAssoc].Value;
            set => variables[HeaderVariableCode.DimAssoc].Value = value;
        }

        /// <summary>
        /// Dimensioning arrow size
        /// </summary>
        public double DimAsz
        {
            get => (double)variables[HeaderVariableCode.DimAsz].Value;
            set => variables[HeaderVariableCode.DimAsz].Value = value;
        }

        /// <summary>
        /// Controls dimension text and arrow placement when space is not sufficient to place both within the extension lines:
        /// 0 = Places both text and arrows outside extension lines
        /// 1 = Moves arrows first, then text
        /// 2 = Moves text first, then arrows
        /// 3 = Moves either text or arrows, whichever fits best
        /// AutoCAD adds a leader to moved dimension text when DIMTMOVE is set to 1
        /// </summary>
        public short DimAtFit
        {
            get => (short)variables[HeaderVariableCode.DimAtFit].Value;
            set => variables[HeaderVariableCode.DimAtFit].Value = value;
        }

        /// <summary>
        /// Angle format for angular dimensions:
        /// 0 = Decimal degrees
        /// 1 = Degrees/minutes/seconds;
        /// 2 = Gradians
        /// 3 = Radians
        /// 4 = Surveyor's units
        /// </summary>
        public short DimAUnit
        {
            get => (short)variables[HeaderVariableCode.DimAUnit].Value;
            set => variables[HeaderVariableCode.DimAUnit].Value = value;
        }

        /// <summary>
        /// Controls suppression of zeros for angular dimensions:
        /// 0 = Displays all leading and trailing zeros
        /// 1 = Suppresses leading zeros in decimal dimensions
        /// 2 = Suppresses trailing zeros in decimal dimensions
        /// 3 = Suppresses leading and trailing zeros
        /// </summary>
        public short DimAZin
        {
            get => (short)variables[HeaderVariableCode.DimAZin].Value;
            set => variables[HeaderVariableCode.DimAZin].Value = value;
        }

        /// <summary>
        /// Arrow block name
        /// </summary>
        public string DimBlk
        {
            get => (string)variables[HeaderVariableCode.DimBlk].Value;
            set => variables[HeaderVariableCode.DimBlk].Value = value;
        }

        /// <summary>
        /// First arrow block name
        /// </summary>
        public string DimBlk1
        {
            get => (string)variables[HeaderVariableCode.DimBlk1].Value;
            set => variables[HeaderVariableCode.DimBlk1].Value = value;
        }

        /// <summary>
        /// Second arrow block name
        /// </summary>
        public string DimBlk2
        {
            get => (string)variables[HeaderVariableCode.DimBlk2].Value;
            set => variables[HeaderVariableCode.DimBlk2].Value = value;
        }

        /// <summary>
        /// Size of center mark/lines
        /// </summary>
        public double DimCen
        {
            get => (double)variables[HeaderVariableCode.DimCen].Value;
            set => variables[HeaderVariableCode.DimCen].Value = value;
        }

        /// <summary>
        /// Dimension text color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public short DimClrd
        {
            get => (short)variables[HeaderVariableCode.DimClrd].Value;
            set => variables[HeaderVariableCode.DimClrd].Value = value;
        }

        /// <summary>
        /// Dimension extension line color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public short DimClRe
        {
            get => (short)variables[HeaderVariableCode.DimClRe].Value;
            set => variables[HeaderVariableCode.DimClRe].Value = value;
        }

        /// <summary>
        /// Dimension text color:
        /// range is 0 = BYBLOCK; 256 = BYLAYER
        /// </summary>
        public short DimClRt
        {
            get => (short)variables[HeaderVariableCode.DimClRt].Value;
            set => variables[HeaderVariableCode.DimClRt].Value = value;
        }

        /// <summary>
        /// Number of decimal places for the tolerance values of a primary units dimension
        /// </summary>
        public short DimDec
        {
            get => (short)variables[HeaderVariableCode.DimDec].Value;
            set => variables[HeaderVariableCode.DimDec].Value = value;
        }

        /// <summary>
        /// Dimension line extension
        /// </summary>
        public double DimDle
        {
            get => (double)variables[HeaderVariableCode.DimDle].Value;
            set => variables[HeaderVariableCode.DimDle].Value = value;
        }

        /// <summary>
        /// Dimension line increment
        /// </summary>
        public double DimDli
        {
            get => (double)variables[HeaderVariableCode.DimDli].Value;
            set => variables[HeaderVariableCode.DimDli].Value = value;
        }

        /// <summary>
        /// Single-character decimal separator used when creating dimensions whose unit format is decimal
        /// </summary>
        public short DimDsep
        {
            get => (short)variables[HeaderVariableCode.DimDsep].Value;
            set => variables[HeaderVariableCode.DimDsep].Value = value;
        }

        /// <summary>
        /// Extension line extension
        /// </summary>
        public double DimExE
        {
            get => (double)variables[HeaderVariableCode.DimExE].Value;
            set => variables[HeaderVariableCode.DimExE].Value = value;
        }

        /// <summary>
        /// Extension line offset
        /// </summary>
        public double DimExO
        {
            get => (double)variables[HeaderVariableCode.DimExO].Value;
            set => variables[HeaderVariableCode.DimExO].Value = value;
        }

        /// <summary>
        /// Scale factor used to calculate the height of text for dimension fractions and tolerances.
        /// AutoCAD multiplies DIMTXT by DIMTFAC to set the fractional or tolerance text height.
        /// </summary>
        public double DimFac
        {
            get => (double)variables[HeaderVariableCode.DimFac].Value;
            set => variables[HeaderVariableCode.DimFac].Value = value;
        }

        /// <summary>
        /// Dimension line gap
        /// </summary>
        public double DimGap
        {
            get => (double)variables[HeaderVariableCode.DimGap].Value;
            set => variables[HeaderVariableCode.DimGap].Value = value;
        }

        /// <summary>
        /// Horizontal dimension text position:
        /// 0 = Above dimension line and center-justified between extension lines
        /// 1 = Above dimension line and next to first extension line
        /// 2 = Above dimension line and next to second extension line
        /// 3 = Above and center-justified to first extension line
        /// 4 = Above and center-justified to second extension line
        /// </summary>
        public short DimJust
        {
            get => (short)variables[HeaderVariableCode.DimJust].Value;
            set => variables[HeaderVariableCode.DimJust].Value = value;
        }

        /// <summary>
        /// Arrow block name for leaders
        /// </summary>
        public string DimLdrBlk
        {
            get => (string)variables[HeaderVariableCode.DimLdrBlk].Value;
            set => variables[HeaderVariableCode.DimLdrBlk].Value = value;
        }

        /// <summary>
        /// Linear measurements scale factor
        /// </summary>
        public double DimLfAc
        {
            get => (double)variables[HeaderVariableCode.DimLfAc].Value;
            set => variables[HeaderVariableCode.DimLfAc].Value = value;
        }

        /// <summary>
        /// Dimension limits generated if nonzero
        /// </summary>
        public short DimLim
        {
            get => (short)variables[HeaderVariableCode.DimLim].Value;
            set => variables[HeaderVariableCode.DimLim].Value = value;
        }

        /// <summary>
        /// Sets units for all dimension types except Angular:
        /// 1 = Scientific
        /// 2 = Decimal
        /// 3 = Engineering
        /// 4 = Architectural
        /// 5 = Fractional
        /// 6 = Operating system
        /// </summary>
        public short DimLUnit
        {
            get => (short)variables[HeaderVariableCode.DimLUnit].Value;
            set => variables[HeaderVariableCode.DimLUnit].Value = value;
        }

        /// <summary>
        /// Dimension line lineweight:
        /// -3 = Standard
        /// -2 = ByLayer
        /// -1 = ByBlock
        /// 0-211 = an integer representing 100th of mm
        /// </summary>
        public short DimLwD
        {
            get => (short)variables[HeaderVariableCode.DimLwD].Value;
            set => variables[HeaderVariableCode.DimLwD].Value = value;
        }

        /// <summary>
        /// Extension line lineweight:
        /// -3 = Standard
        /// -2 = ByLayer
        /// -1 = ByBlock
        /// 0-211 = an integer representing 100th of mm
        /// </summary>
        public short DimLwE
        {
            get => (short)variables[HeaderVariableCode.DimLwE].Value;
            set => variables[HeaderVariableCode.DimLwE].Value = value;
        }

        /// <summary>
        /// General dimensioning suffix
        /// </summary>
        public string DimPost
        {
            get => (string)variables[HeaderVariableCode.DimPost].Value;
            internal set { variables[HeaderVariableCode.DimPost].Value = value; }
        }

        /// <summary>
        /// Rounding value for dimension distances
        /// </summary>
        public double DimRnd
        {
            get => (double)variables[HeaderVariableCode.DimRnd].Value;
            set => variables[HeaderVariableCode.DimRnd].Value = value;
        }

        /// <summary>
        /// Use separate arrow blocks if nonzero
        /// </summary>
        public short DimSah
        {
            get => (short)variables[HeaderVariableCode.DimSah].Value;
            set => variables[HeaderVariableCode.DimSah].Value = value;
        }

        /// <summary>
        /// Overall dimensioning scale factor
        /// </summary>
        public double DimScale
        {
            get => (double)variables[HeaderVariableCode.DimScale].Value;
            set => variables[HeaderVariableCode.DimScale].Value = value;
        }

        /// <summary>
        /// Suppression of first extension line:
        /// 0 = Not suppressed
        /// 1 = Suppressed
        /// </summary>
        public short DimSd1
        {
            get => (short)variables[HeaderVariableCode.DimSd1].Value;
            set => variables[HeaderVariableCode.DimSd1].Value = value;
        }

        /// <summary>
        /// Suppression of second extension line:
        /// 0 = Not suppressed
        /// 1 = Suppressed
        /// </summary>
        public short DimSd2
        {
            get => (short)variables[HeaderVariableCode.DimSd2].Value;
            set => variables[HeaderVariableCode.DimSd2].Value = value;
        }

        /// <summary>
        /// First extension line suppressed if nonzero
        /// </summary>
        public short DimSe1
        {
            get => (short)variables[HeaderVariableCode.DimSe1].Value;
            set => variables[HeaderVariableCode.DimSe1].Value = value;
        }

        /// <summary>
        /// Second extension line suppressed if nonzero
        /// </summary>
        public short DimSe2
        {
            get => (short)variables[HeaderVariableCode.DimSe2].Value;
            set => variables[HeaderVariableCode.DimSe2].Value = value;
        }

        /// <summary>
        /// 1 = Recompute dimensions while dragging
        /// 0 = Drag original image
        /// </summary>
        public short DimSho
        {
            get => (short)variables[HeaderVariableCode.DimSho].Value;
            set => variables[HeaderVariableCode.DimSho].Value = value;
        }

        /// <summary>
        /// Suppress outside-extensions dimension lines if nonzero
        /// </summary>
        public short DimSoXD
        {
            get => (short)variables[HeaderVariableCode.DimSoXD].Value;
            set => variables[HeaderVariableCode.DimSoXD].Value = value;
        }

        /// <summary>
        /// Current dimension style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string DimStyle
        {
            get => (string)variables[HeaderVariableCode.DimStyle].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current dimension style name should be at least one character long.");
                }

                variables[HeaderVariableCode.DimStyle].Value = value;
            }
        }

        /// <summary>
        /// Text above dimension line if nonzero
        /// </summary>
        public short DimTad
        {
            get => (short)variables[HeaderVariableCode.DimTad].Value;
            set => variables[HeaderVariableCode.DimTad].Value = value;
        }

        /// <summary>
        /// Number of decimal places to display the tolerance values
        /// </summary>
        public short DimTDec
        {
            get => (short)variables[HeaderVariableCode.DimTDec].Value;
            set => variables[HeaderVariableCode.DimTDec].Value = value;
        }

        /// <summary>
        /// Dimension tolerance display scale factor
        /// </summary>
        public double DimTFac
        {
            get => (double)variables[HeaderVariableCode.DimTFac].Value;
            set => variables[HeaderVariableCode.DimTFac].Value = value;
        }

        /// <summary>
        /// Text inside horizontal if nonzero
        /// </summary>
        public short DimTih
        {
            get => (short)variables[HeaderVariableCode.DimTih].Value;
            set => variables[HeaderVariableCode.DimTih].Value = value;
        }

        /// <summary>
        /// Force text inside extensions if nonzero
        /// </summary>
        public short DimTix
        {
            get => (short)variables[HeaderVariableCode.DimTix].Value;
            set => variables[HeaderVariableCode.DimTix].Value = value;
        }

        /// <summary>
        /// Minus tolerance
        /// </summary>
        public double DimTm
        {
            get => (double)variables[HeaderVariableCode.DimTm].Value;
            set => variables[HeaderVariableCode.DimTm].Value = value;
        }

        /// <summary>
        /// Dimension text movement rules:
        /// 0 = Moves the dimension line with dimension text
        /// 1 = Adds a leader when dimension text is moved
        /// 2 = Allows text to be moved freely without a leader
        /// </summary>
        public short DimTMove
        {
            get => (short)variables[HeaderVariableCode.DimTMove].Value;
            set => variables[HeaderVariableCode.DimTMove].Value = value;
        }

        /// <summary>
        /// If text is outside the extension lines, dimension lines are forced between the extension lines if nonzero
        /// </summary>
        public short DimToFl
        {
            get => (short)variables[HeaderVariableCode.DimToFl].Value;
            set => variables[HeaderVariableCode.DimToFl].Value = value;
        }

        /// <summary>
        /// Text outside horizontal if nonzero
        /// </summary>
        public short DimToH
        {
            get => (short)variables[HeaderVariableCode.DimToH].Value;
            set => variables[HeaderVariableCode.DimToH].Value = value;
        }

        /// <summary>
        /// Dimension tolerances generated if nonzero
        /// </summary>
        public short DimToL
        {
            get => (short)variables[HeaderVariableCode.DimToL].Value;
            set => variables[HeaderVariableCode.DimToL].Value = value;
        }

        /// <summary>
        /// Vertical justification for tolerance values:
        /// 0 = Top
        /// 1 = Middle
        /// 2 = Bottom
        /// </summary>
        public short DimToLj
        {
            get => (short)variables[HeaderVariableCode.DimToLj].Value;
            set => variables[HeaderVariableCode.DimToLj].Value = value;
        }

        /// <summary>
        /// Plus tolerance
        /// </summary>
        public double DimTp
        {
            get => (double)variables[HeaderVariableCode.DimTp].Value;
            set => variables[HeaderVariableCode.DimTp].Value = value;
        }

        /// <summary>
        /// Dimensioning tick size:
        /// 0 = Draws arrowheads
        /// >0 = Draws oblique strokes instead of arrowheads
        /// </summary>
        public double DimTsz
        {
            get => (double)variables[HeaderVariableCode.DimTsz].Value;
            set => variables[HeaderVariableCode.DimTsz].Value = value;
        }

        /// <summary>
        /// Text vertical position
        /// </summary>
        public double DimTvp
        {
            get => (double)variables[HeaderVariableCode.DimTvp].Value;
            set => variables[HeaderVariableCode.DimTvp].Value = value;
        }

        /// <summary>
        /// Dimension text style
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string DimTxSty
        {
            get => (string)variables[HeaderVariableCode.DimTxSty].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current text style name should be at least one character long.");
                }

                variables[HeaderVariableCode.DimTxSty].Value = value;
            }
        }

        /// <summary>
        /// Dimensioning text height.
        /// </summary>
        /// <remarks>Default value: 2.5.</remarks>
        public double DimTxt
        {
            get => (double)variables[HeaderVariableCode.DimTxt].Value;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The default text height must be greater than zero.");
                }

                variables[HeaderVariableCode.DimTxt].Value = value;
            }
        }

        /// <summary>
        /// Controls suppression of zeros for tolerance values:
        /// 0 = Suppresses zero feet and precisely zero inches
        /// 1 = Includes zero feet and precisely zero inches
        /// 2 = Includes zero feet and suppresses zero inches
        /// 3 = Includes zero inches and suppresses zero feet
        /// 4 = Suppresses leading zeros in decimal dimensions
        /// 8 = Suppresses trailing zeros in decimal dimensions
        /// 12 = Suppresses both leading and trailing zeros
        /// </summary>
        public short DimTzIn
        {
            get => (short)variables[HeaderVariableCode.DimTzIn].Value;
            set => variables[HeaderVariableCode.DimTzIn].Value = value;
        }

        /// <summary>
        /// Cursor functionality for user-positioned text:
        /// 0 = Controls only the dimension line location
        /// 1 = Controls the text position as well as the dimension line location
        /// </summary>
        public short DimUpt
        {
            get => (short)variables[HeaderVariableCode.DimUpt].Value;
            set => variables[HeaderVariableCode.DimUpt].Value = value;
        }

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
        public short DimZIn
        {
            get => (short)variables[HeaderVariableCode.DimZIn].Value;
            set => variables[HeaderVariableCode.DimZIn].Value = value;
        }

        /// <summary>
        /// Controls the display of silhouette curves of body objects in Wireframe mode:
        /// 0 = Off
        /// 1 = On
        /// </summary>
        public short DispSiLh
        {
            get => (short)variables[HeaderVariableCode.DispSiLh].Value;
            set => variables[HeaderVariableCode.DispSiLh].Value = value;
        }

        /// <summary>
        /// Hard-pointer ID to visual style while creating 3D solid primitives.
        /// </summary>
        /// <remarks>Default value: null.</remarks>
        public string DragVs
        {
            get => (string)variables[HeaderVariableCode.DragVs].Value;
            set => variables[HeaderVariableCode.DragVs].Value = value;
        }

        /// <summary>
        /// Drawing code page; set to the system code page when a new drawing is created, but not otherwise maintained by AutoCAD.
        /// </summary>
        public string DwgCodePage
        {
            get => (string)variables[HeaderVariableCode.DwgCodePage].Value;
            internal set => variables[HeaderVariableCode.DwgCodePage].Value = value;
        }

        /// <summary>
        /// Current elevation set by ELEV command
        /// </summary>
        public double Elevation
        {
            get => (double)variables[HeaderVariableCode.Elevation].Value;
            internal set => variables[HeaderVariableCode.Elevation].Value = value;
        }

        /// <summary>
        /// Lineweight endcaps setting for new objects:
        /// 0 = None
        /// 1 = Round
        /// 2 = Angle
        /// 3 = Square
        /// </summary>
        public short EndCaps
        {
            get => (short)variables[HeaderVariableCode.EndCaps].Value;
            set => variables[HeaderVariableCode.EndCaps].Value = value;
        }

        /// <summary>
        /// X, Y, and Z drawing extents upper-right corner (in WCS)
        /// </summary>
        public Vector3 ExtMax
        {
            get => (Vector3)variables[HeaderVariableCode.ExtMax].Value;
            set => variables[HeaderVariableCode.ExtMax].Value = value;
        }

        /// <summary>
        /// X, Y, and Z drawing extents lower-left corner (in WCS)
        /// </summary>
        public Vector3 ExtMin
        {
            get => (Vector3)variables[HeaderVariableCode.ExtMin].Value;
            set => variables[HeaderVariableCode.ExtMin].Value = value;
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
            get => (bool)variables[HeaderVariableCode.Extnames].Value;
            internal set => variables[HeaderVariableCode.Extnames].Value = value;
        }

        /// <summary>
        /// Fillet radius
        /// </summary>
        public double FilletRad
        {
            get => (double)variables[HeaderVariableCode.FilletRad].Value;
            internal set => variables[HeaderVariableCode.FilletRad].Value = value;
        }

        /// <summary>
        /// Fill mode on if nonzero
        /// </summary>
        public short FillMode
        {
            get => (short)variables[HeaderVariableCode.FillMode].Value;
            set => variables[HeaderVariableCode.FillMode].Value = value;
        }

        /// <summary>
        /// Set at creation time, uniquely identifies a particular drawing
        /// </summary>
        public string FingerprintGuid
        {
            get => (string)variables[HeaderVariableCode.FingerprintGuid].Value;
            set => variables[HeaderVariableCode.FingerprintGuid].Value = value;
        }

        /// <summary>
        /// Specifies a gap to be displayed where an object is hidden by another object;
        /// the value is specified as a percent of one unit and is independent of the zoom level.
        /// A haloed line is shortened at the point where it is hidden when HIDE or the Hidden option of SHADEMODE is used
        /// </summary>
        public short HaloGap
        {
            get => (short)variables[HeaderVariableCode.HaloGap].Value;
            set => variables[HeaderVariableCode.HaloGap].Value = value;
        }

        /// <summary>
        /// Next available handle.
        /// </summary>
        public string HandleSeed
        {
            get => (string)variables[HeaderVariableCode.HandleSeed].Value;
            internal set { variables[HeaderVariableCode.HandleSeed].Value = value; }
        }

        /// <summary>
        /// Specifies HIDETEXT system variable:
        /// 0 = HIDE ignores text objects when producing the hidden view
        /// 1 = HIDE does not ignore text objects
        /// </summary>
        public short HideText
        {
            get => (short)variables[HeaderVariableCode.HideText].Value;
            set => variables[HeaderVariableCode.HideText].Value = value;
        }

        /// <summary>
        /// Path for all relative hyperlinks in the drawing.
        /// If null, the drawing path is used
        /// </summary>
        public string HyperlinkBase
        {
            get => (string)variables[HeaderVariableCode.HyperlinkBase].Value;
            set => variables[HeaderVariableCode.HyperlinkBase].Value = value;
        }

        /// <summary>
        /// Controls whether layer and spatial indexes are created and saved in drawing files:
        /// 0 = No indexes are created
        /// 1 = Layer index is created
        /// 2 = Spatial index is created
        /// 3 = Layer and spatial indexes are created
        /// </summary>
        public short IndexCtl
        {
            get => (short)variables[HeaderVariableCode.IndexCtl].Value;
            set => variables[HeaderVariableCode.IndexCtl].Value = value;
        }

        /// <summary>
        /// Insertion base set by BASE command (in WCS)
        /// </summary>
        /// <remarks>
        /// When you insert or externally reference the current drawing into other drawings, this base point is used as the insertion base point.
        /// </remarks>
        public Vector3 InsBase
        {
            get => (Vector3)variables[HeaderVariableCode.InsBase].Value;
            set => variables[HeaderVariableCode.InsBase].Value = value;
        }

        /// <summary>
        /// Default drawing units for AutoCAD DesignCenter blocks:
        /// Specifies a drawing units value for automatic scaling of blocks,
        /// images, or xRefs when inserted or attached to a drawing.
        /// </summary>
        /// <remarks>
        /// Default value: Unitless.<br />
        /// It is not recommend to change this value, if the LUnits variable has been set to Architectural or Engineering, they require the InsUnits to be set at Inches.
        /// </remarks>
        public DrawingUnits InsUnits
        {
            get => (DrawingUnits)variables[HeaderVariableCode.InsUnits].Value;
            set => variables[HeaderVariableCode.InsUnits].Value = value;
        }

        /// <summary>
        /// Represents the ACI color index of the "interference objects" created during the INTERFERE command.
        /// <remarks>Default value: 1.</remarks>
        /// </summary>
        public short InterfereColor
        {
            get => (short)variables[HeaderVariableCode.InterfereColor].Value;
            set => variables[HeaderVariableCode.InterfereColor].Value = value;
        }

        /// <summary>
        /// Hard-pointer ID to the visual style for interference objects.
        /// Default visual style is Conceptual.
        /// </summary>
        public string InterfereObjVs
        {
            get => (string)variables[HeaderVariableCode.InterfereObjVs].Value;
            set => variables[HeaderVariableCode.InterfereObjVs].Value = value;
        }

        /// <summary>
        /// Hard-pointer ID to the visual style for the viewport during interference checking.
        /// Default visual style is 3d Wireframe
        /// </summary>
        public string InterfereVpVs
        {
            get => (string)variables[HeaderVariableCode.InterfereVpVs].Value;
            set => variables[HeaderVariableCode.InterfereVpVs].Value = value;
        }

        /// <summary>
        /// Specifies the entity color of intersection polylines:
        /// Values 1-255 designate an AutoCAD color index (ACI)
        /// 0 = Color BYBLOCK
        /// 256 = Color BYLAYER
        /// 257 = Color BYENTITY
        /// </summary>
        public short IntersectionColor
        {
            get => (short)variables[HeaderVariableCode.IntersectionColor].Value;
            set => variables[HeaderVariableCode.IntersectionColor].Value = value;
        }

        /// <summary>
        /// Specifies the display of intersection polylines:
        /// 0 = Turns off the display of intersection polylines
        /// 1 = Turns on the display of intersection polylines
        /// </summary>
        public short IntersectionDisplay
        {
            get => (short)variables[HeaderVariableCode.IntersectionDisplay].Value;
            set => variables[HeaderVariableCode.IntersectionDisplay].Value = value;
        }

        /// <summary>
        /// Lineweight joint setting for new objects:
        /// 0 = None
        /// 1 = Round
        /// 2 = Angle
        /// 3 = Flat
        /// </summary>
        public short JoinStyle
        {
            get => (short)variables[HeaderVariableCode.JoinStyle].Value;
            set => variables[HeaderVariableCode.JoinStyle].Value = value;
        }

        /// <summary>
        /// Nonzero if limits checking is on
        /// </summary>
        public short LimCheck
        {
            get => (short)variables[HeaderVariableCode.LimCheck].Value;
            set => variables[HeaderVariableCode.LimCheck].Value = value;
        }

        /// <summary>
        /// XY drawing limits upper-right corner (in WCS)
        /// </summary>
        public Vector2 LimMax
        {
            get => (Vector2)variables[HeaderVariableCode.LimMax].Value;
            set => variables[HeaderVariableCode.LimMax].Value = value;
        }

        /// <summary>
        /// XY drawing limits lower-left corner (in WCS)
        /// </summary>
        public Vector2 LimMin
        {
            get => (Vector2)variables[HeaderVariableCode.LimMin].Value;
            set => variables[HeaderVariableCode.LimMin].Value = value;
        }

        /// <summary>
        /// Global line type scale.
        /// </summary>
        /// <remarks>Default value: 1.0.</remarks>
        public double LtScale
        {
            get => (double)variables[HeaderVariableCode.LtScale].Value;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The global line type scale must be greater than zero.");
                }

                variables[HeaderVariableCode.LtScale].Value = value;
            }
        }

        /// <summary>
        /// Units format for coordinates and distances.
        /// </summary>
        /// <remarks>
        /// Default value: Decimal.<br />
        /// If the LUnits is set to Architectural or Engineering the InsUnits variable will be set to Inches automatically.
        /// </remarks>
        public LinearUnitType LUnits
        {
            get => (LinearUnitType)variables[HeaderVariableCode.LUnits].Value;
            set
            {
                if (value == LinearUnitType.Architectural || value == LinearUnitType.Engineering)
                {
                    InsUnits = DrawingUnits.Inches;
                }

                variables[HeaderVariableCode.LUnits].Value = value;
            }
        }

        /// <summary>
        /// Units precision for coordinates and distances.
        /// </summary>
        /// <remarks>Valid values are integers from 0 to 8. Default value: 4.</remarks>
        public short LUprec
        {
            get => (short)variables[HeaderVariableCode.LUprec].Value;
            set
            {
                if (value < 0 || value > 8)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Valid values are integers from 0 to 8.");
                }

                variables[HeaderVariableCode.LUprec].Value = value;
            }
        }

        /// <summary>
        /// Controls the display of line weights on the Model or Layout tab.
        /// </summary>
        /// <remarks>
        /// Default value: false.<br />
        /// false = Line weight is not displayed.<br />
        /// true = Line weight is displayed.<br />
        /// </remarks>
        public bool LwDisplay
        {
            get => (bool)variables[HeaderVariableCode.LwDisplay].Value;
            set => variables[HeaderVariableCode.LwDisplay].Value = value;
        }

        /// <summary>
        /// Sets maximum number of viewports to be regenerated
        /// </summary>
        public short MaxActVp
        {
            get => (short)variables[HeaderVariableCode.MaxActVp].Value;
            set => variables[HeaderVariableCode.MaxActVp].Value = value;
        }

        /// <summary>
        /// Sets drawing units:
        /// 0 = English
        /// 1 = Metric
        /// </summary>
        public short Measurement
        {
            get => (short)variables[HeaderVariableCode.Measurement].Value;
            set => variables[HeaderVariableCode.Measurement].Value = value;
        }

        /// <summary>
        /// Name of menu file
        /// </summary>
        public string Menu
        {
            get => (string)variables[HeaderVariableCode.Menu].Value;
            set => variables[HeaderVariableCode.Menu].Value = value;
        }

        /// <summary>
        /// Controls if the text will be mirrored during a symmetry.
        /// </summary>
        public bool MirrText
        {
            get => (bool)variables[HeaderVariableCode.MirrText].Value;
            set => variables[HeaderVariableCode.MirrText].Value = value;
        }

        /// <summary>
        /// Specifies the color of obscured lines.
        /// An obscured line is a hidden line made visible by changing its color and linetype and is visible
        /// only when the HIDE or SHADEMODE command is used. The OBSCUREDCOLOR setting is visible
        /// only if the OBSCUREDLTYPE is turned ON by setting it to a value other than 0.
        /// 0 and 256 = Entity color
        /// 1-255 = An AutoCAD color index (ACI)
        /// </summary>
        public short ObsColor
        {
            get => (short)variables[HeaderVariableCode.ObsColor].Value;
            set => variables[HeaderVariableCode.ObsColor].Value = value;
        }

        /// <summary>
        /// Specifies the linetype of obscured lines. Obscured linetypes are independent of zoom level,
        /// unlike standard object linetypes.
        /// Value 0 turns off display of obscured lines and is the default.
        /// Linetype values are defined as follows:
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
        public short ObsLtype
        {
            get => (short)variables[HeaderVariableCode.ObsLtype].Value;
            set => variables[HeaderVariableCode.ObsLtype].Value = value;
        }

        /// <summary>
        /// Ortho mode on if nonzero
        /// </summary>
        public short OrthoMode
        {
            get => (short)variables[HeaderVariableCode.OrthoMode].Value;
            set => variables[HeaderVariableCode.OrthoMode].Value = value;
        }

        /// <summary>
        /// Controls the <see cref="PointShape">shape</see> to draw a point entity.
        /// </summary>
        /// <remarks>Default value: PointShape.Dot.</remarks>
        public PointShape PdMode
        {
            get => (PointShape)variables[HeaderVariableCode.PdMode].Value;
            set => variables[HeaderVariableCode.PdMode].Value = value;
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
            get => (double)variables[HeaderVariableCode.PdSize].Value;
            set => variables[HeaderVariableCode.PdSize].Value = value;
        }

        /// <summary>
        /// Current paper space elevation
        /// </summary>
        public double PElevation
        {
            get => (double)variables[HeaderVariableCode.PElevation].Value;
            set => variables[HeaderVariableCode.PElevation].Value = value;
        }

        /// <summary>
        /// Maximum X, Y, and Z extents for paper space
        /// </summary>
        public Vector3 PExtMax
        {
            get => (Vector3)variables[HeaderVariableCode.PExtMax].Value;
            set => variables[HeaderVariableCode.PExtMax].Value = value;
        }

        /// <summary>
        /// Minimum X, Y, and Z extents for paper space
        /// </summary>
        public Vector3 PExtMin
        {
            get => (Vector3)variables[HeaderVariableCode.PExtMin].Value;
            set => variables[HeaderVariableCode.PExtMin].Value = value;
        }

        /// <summary>
        /// Paper space insertion base point
        /// </summary>
        public Vector3 PinsBase
        {
            get => (Vector3)variables[HeaderVariableCode.PinsBase].Value;
            set => variables[HeaderVariableCode.PinsBase].Value = value;
        }

        /// <summary>
        /// Limits checking in paper space when nonzero
        /// </summary>
        public short PLimCheck
        {
            get => (short)variables[HeaderVariableCode.PLimCheck].Value;
            set => variables[HeaderVariableCode.PLimCheck].Value = value;
        }

        /// <summary>
        /// Maximum X and Y limits in paper space
        /// </summary>
        public Vector2 PLimMax
        {
            get => (Vector2)variables[HeaderVariableCode.PLimMax].Value;
            set => variables[HeaderVariableCode.PLimMax].Value = value;
        }

        /// <summary>
        /// Minimum X and Y limits in paper space
        /// </summary>
        public Vector2 PLimMin
        {
            get => (Vector2)variables[HeaderVariableCode.PLimMin].Value;
            set => variables[HeaderVariableCode.PLimMin].Value = value;
        }

        /// <summary>
        /// Governs the generation of line type patterns around the vertexes of a 2D polyline.
        /// </summary>
        /// <remarks>
        /// Default value: 0.<br />
        /// 1 = Line type is generated in a continuous pattern around vertexes of the polyline.<br />
        /// 0 = Each segment of the polyline starts and ends with a dash.
        /// </remarks>
        public short PLineGen
        {
            get => (short)variables[HeaderVariableCode.PLineGen].Value;
            set
            {
                if (value != 0 && value != 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted values are 0 or 1.");
                }

                variables[HeaderVariableCode.PLineGen].Value = value;
            }
        }

        /// <summary>
        /// Default polyline width
        /// </summary>
        public double PLineWid
        {
            get => (double)variables[HeaderVariableCode.PLineWid].Value;
            set => variables[HeaderVariableCode.PLineWid].Value = value;
        }

        /// <summary>
        /// Assigns a project name to the current drawing.
        /// Used when an external reference or image is not found on its original path.
        /// The project name points to a section in the registry that can contain one or more
        /// search paths for each project name defined. Project names and their
        /// search directories are created from the Files tab of the Options dialog box
        /// </summary>
        public string Projectname
        {
            get => (string)variables[HeaderVariableCode.Projectname].Value;
            set => variables[HeaderVariableCode.Projectname].Value = value;
        }

        /// <summary>
        /// Controls the saving of proxy object images
        /// </summary>
        public short ProxyGraphics
        {
            get => (short)variables[HeaderVariableCode.ProxyGraphics].Value;
            set => variables[HeaderVariableCode.ProxyGraphics].Value = value;
        }

        /// <summary>
        /// Controls paper space line type scaling.
        /// </summary>
        /// <remarks>
        /// Default value: 1.<br />
        /// 1 = No special line type scaling.<br />
        /// 0 = Viewport scaling governs line type scaling.
        /// </remarks>
        public short PsLtScale
        {
            get => (short)variables[HeaderVariableCode.PsLtScale].Value;
            set
            {
                if (value != 0 && value != 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted values are 0 or 1.");
                }

                variables[HeaderVariableCode.PsLtScale].Value = value;
            }
        }

        /// <summary>
        /// Indicates whether the current drawing is in a Color-Dependent or Named Plot Style mode:
        /// 0 = Uses named plot style tables in the current drawing
        /// 1 = Uses color-dependent plot style tables in the current drawing
        /// </summary>
        public bool PStyleMode
        {
            get => (bool)variables[HeaderVariableCode.PStyleMode].Value;
            set => variables[HeaderVariableCode.PStyleMode].Value = value;
        }

        /// <summary>
        /// View scale factor for new viewports:
        /// 0 = Scaled to fit
        /// 0 = Scale factor (a positive real value)
        /// </summary>
        public double PSvpScale
        {
            get => (double)variables[HeaderVariableCode.PSvpScale].Value;
            set => variables[HeaderVariableCode.PSvpScale].Value = value;
        }

        /// <summary>
        /// Name of the UCS that defines the origin and orientation of orthographic UCS settings (paper space only
        /// </summary>
        public string PUcsBase
        {
            get => (string)variables[HeaderVariableCode.PUcsBase].Value;
            set => variables[HeaderVariableCode.PUcsBase].Value = value;
        }

        /// <summary>
        /// Current paper space UCS name
        /// </summary>
        public string PUcsName
        {
            get => (string)variables[HeaderVariableCode.PUcsName].Value;
            set => variables[HeaderVariableCode.PUcsName].Value = value;
        }

        /// <summary>
        /// Current paper space UCS origin
        /// </summary>
        public Vector3 PUcsOrg
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrg].Value;
            set => variables[HeaderVariableCode.PUcsOrg].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to BACK when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgBack
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgBack].Value;
            set => variables[HeaderVariableCode.PUcsOrgBack].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to BOTTOM when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgBottom
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgBottom].Value;
            set => variables[HeaderVariableCode.PUcsOrgBottom].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to FRONT when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgFront
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgFront].Value;
            set => variables[HeaderVariableCode.PUcsOrgFront].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to LEFT when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgLeft
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgLeft].Value;
            set => variables[HeaderVariableCode.PUcsOrgLeft].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to RIGHT when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgRight
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgRight].Value;
            set => variables[HeaderVariableCode.PUcsOrgRight].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing paper space UCS to TOP when PUCSBASE is set to WORLD
        /// </summary>
        public Vector3 PUcsOrgTop
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsOrgTop].Value;
            set => variables[HeaderVariableCode.PUcsOrgTop].Value = value;
        }

        /// <summary>
        /// If paper space UCS is orthographic(PUCSORTHOVIEW not equal to 0),
        /// this is the name of the UCS that the orthographic UCS is relative to.If blank,
        /// UCS is relative to WORLD
        /// </summary>
        public string PUcsOrthoRef
        {
            get => (string)variables[HeaderVariableCode.PUcsOrthoRef].Value;
            set => variables[HeaderVariableCode.PUcsOrthoRef].Value = value;
        }

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
        public short PUcsOrthoView
        {
            get => (short)variables[HeaderVariableCode.PUcsOrthoView].Value;
            set => variables[HeaderVariableCode.PUcsOrthoView].Value = value;
        }

        /// <summary>
        /// Current paper space UCS X axis
        /// </summary>
        public Vector3 PUcsXDir
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsXDir].Value;
            set => variables[HeaderVariableCode.PUcsXDir].Value = value;
        }

        /// <summary>
        /// Current paper space UCS Y axis
        /// </summary>
        public Vector3 PUcsYDir
        {
            get => (Vector3)variables[HeaderVariableCode.PUcsYDir].Value;
            set => variables[HeaderVariableCode.PUcsYDir].Value = value;
        }

        /// <summary>
        /// Quick Text mode on if nonzero
        /// </summary>
        public short QTextMode
        {
            get => (short)variables[HeaderVariableCode.QTextMode].Value;
            set => variables[HeaderVariableCode.QTextMode].Value = value;
        }

        /// <summary>
        /// REGENAUTO mode on if nonzero
        /// </summary>
        public short RegenMode
        {
            get => (short)variables[HeaderVariableCode.RegenMode].Value;
            set => variables[HeaderVariableCode.RegenMode].Value = value;
        }

        /// <summary>
        /// Controls the shading of edges:
        /// 0 = Faces shaded, edges not highlighted
        /// 1 = Faces shaded, edges highlighted in black
        /// 2 = Faces not filled, edges in entity color
        /// 3 = Faces in entity color, edges in black
        /// </summary>
        public short ShadeEdge
        {
            get => (short)variables[HeaderVariableCode.ShadeEdge].Value;
            set => variables[HeaderVariableCode.ShadeEdge].Value = value;
        }

        /// <summary>
        /// Percent ambient/diffuse light; range 1-100;
        /// </summary>
        /// <remarks>Default value: 70.</remarks>
        public short ShadeDif
        {
            get => (short)variables[HeaderVariableCode.ShadeDif].Value;
            set => variables[HeaderVariableCode.ShadeDif].Value = value;
        }

        /// <summary>
        /// Location of the ground shadow plane. This is a Z axis ordinate.
        /// </summary>
        public double ShadowPlaneLocation
        {
            get => (double)variables[HeaderVariableCode.ShadowPlaneLocation].Value;
            set => variables[HeaderVariableCode.ShadowPlaneLocation].Value = value;
        }

        /// <summary>
        /// Sketch record increment
        /// </summary>
        public double SketchInc
        {
            get => (double)variables[HeaderVariableCode.SketchInc].Value;
            set => variables[HeaderVariableCode.SketchInc].Value = value;
        }

        /// <summary>
        /// Determines the object type created by the SKETCH command:
        /// 0 = Generates lines
        /// 1 = Generates polylines
        /// 2 = Generates splines
        /// </summary>
        public short SkPoly
        {
            get => (short)variables[HeaderVariableCode.SkPoly].Value;
            set => variables[HeaderVariableCode.SkPoly].Value = value;
        }

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
        public short Sortents
        {
            get => (short)variables[HeaderVariableCode.Sortents].Value;
            set => variables[HeaderVariableCode.Sortents].Value = value;
        }

        /// <summary>
        /// Number of line segments per spline patch
        /// </summary>
        public short SplinesEgs
        {
            get => (short)variables[HeaderVariableCode.SplinesEgs].Value;
            set => variables[HeaderVariableCode.SplinesEgs].Value = value;
        }

        /// <summary>
        /// Spline curve type for PEDIT Spline
        /// </summary>
        public short SplineType
        {
            get => (short)variables[HeaderVariableCode.SplineType].Value;
            set => variables[HeaderVariableCode.SplineType].Value = value;
        }

        /// <summary>
        /// Number of mesh tabulations in first direction
        /// </summary>
        public short SurfTab1
        {
            get => (short)variables[HeaderVariableCode.SurfTab1].Value;
            set => variables[HeaderVariableCode.SurfTab1].Value = value;
        }

        /// <summary>
        /// Number of mesh tabulations in second direction
        /// </summary>
        public short SurfTab2
        {
            get => (short)variables[HeaderVariableCode.SurfTab2].Value;
            set => variables[HeaderVariableCode.SurfTab2].Value = value;
        }

        /// <summary>
        /// Surface type for PEDIT Smooth
        /// </summary>
        public short SurfType
        {
            get => (short)variables[HeaderVariableCode.SurfType].Value;
            set => variables[HeaderVariableCode.SurfType].Value = value;
        }

        /// <summary>
        /// Surface density (for PEDIT Smooth) in M direction
        /// </summary>
        public short SurfU
        {
            get => (short)variables[HeaderVariableCode.SurfU].Value;
            set => variables[HeaderVariableCode.SurfU].Value = value;
        }

        /// <summary>
        /// Surface density (for PEDIT Smooth) in N direction
        /// </summary>
        public short SurfV
        {
            get => (short)variables[HeaderVariableCode.SurfV].Value;
            set => variables[HeaderVariableCode.SurfV].Value = value;
        }

        /// <summary>
        /// Local date/time of drawing creation.
        /// </summary>
        /// <remarks>This date/time is local to the time zone where the file was created.</remarks>
        public DateTime TdCreate
        {
            get => (DateTime)variables[HeaderVariableCode.TdCreate].Value;
            set => variables[HeaderVariableCode.TdCreate].Value = value;
        }

        /// <summary>
        /// Cumulative editing time for this drawing.
        /// </summary>
        public TimeSpan TdinDwg
        {
            get => (TimeSpan)variables[HeaderVariableCode.TdinDwg].Value;
            set => variables[HeaderVariableCode.TdinDwg].Value = value;
        }

        /// <summary>
        /// Universal date/time the drawing was created.
        /// </summary>
        public DateTime TduCreate
        {
            get => (DateTime)variables[HeaderVariableCode.TduCreate].Value;
            set => variables[HeaderVariableCode.TduCreate].Value = value;
        }

        /// <summary>
        /// Local date/time of last drawing update.
        /// </summary>
        /// <remarks>This date/time is local to the time zone where the file was created.</remarks>
        public DateTime TdUpdate
        {
            get => (DateTime)variables[HeaderVariableCode.TdUpdate].Value;
            set => variables[HeaderVariableCode.TdUpdate].Value = value;
        }

        /// <summary>
        /// User-elapsed timer
        /// </summary>
        public double TdUsrTimer
        {
            get => (double)variables[HeaderVariableCode.TdUsrTimer].Value;
            set => variables[HeaderVariableCode.TdUsrTimer].Value = value;
        }

        /// <summary>
        /// Universal date/time the drawing was created.
        /// </summary>
        public DateTime TduUpdate
        {
            get => (DateTime)variables[HeaderVariableCode.TduUpdate].Value;
            set => variables[HeaderVariableCode.TduUpdate].Value = value;
        }

        /// <summary>
        /// Default text height.
        /// </summary>
        /// <remarks>Default value: 2.5.</remarks>
        public double TextSize
        {
            get => (double)variables[HeaderVariableCode.TextSize].Value;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The default text height must be greater than zero.");
                }

                variables[HeaderVariableCode.TextSize].Value = value;
            }
        }

        /// <summary>
        /// Current text style.
        /// </summary>
        /// <remarks>Default value: Standard.</remarks>
        public string TextStyle
        {
            get => (string)variables[HeaderVariableCode.TextStyle].Value;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value), "The current text style name should be at least one character long.");
                }

                variables[HeaderVariableCode.TextStyle].Value = value;
            }
        }

        /// <summary>
        /// Current thickness set by ELEV command
        /// </summary>
        public double Thickness
        {
            get => (double)variables[HeaderVariableCode.Thickness].Value;
            set => variables[HeaderVariableCode.Thickness].Value = value;
        }

        /// <summary>
        /// 1 = for previous release compatibility mode
        /// 0 = otherwise
        /// </summary>
        public short TileMode
        {
            get => (short)variables[HeaderVariableCode.TileMode].Value;
            set => variables[HeaderVariableCode.TileMode].Value = value;
        }

        /// <summary>
        /// Default trace width
        /// </summary>
        public double TraceWid
        {
            get => (double)variables[HeaderVariableCode.TraceWid].Value;
            set => variables[HeaderVariableCode.TraceWid].Value = value;
        }

        /// <summary>
        /// Specifies the maximum depth of the spatial index
        /// </summary>
        public short TreeDepth
        {
            get => (short)variables[HeaderVariableCode.TreeDepth].Value;
            set => variables[HeaderVariableCode.TreeDepth].Value = value;
        }

        /// <summary>
        /// Name of the UCS that defines the origin and orientation of orthographic UCS settings
        /// </summary>
        public string UcsBase
        {
            get => (string)variables[HeaderVariableCode.UcsBase].Value;
            set => variables[HeaderVariableCode.UcsBase].Value = value;
        }

        /// <summary>
        /// Name of current UCS
        /// </summary>
        public string UcsName
        {
            get => (string)variables[HeaderVariableCode.UcsName].Value;
            set => variables[HeaderVariableCode.UcsName].Value = value;
        }

        /// <summary>
        /// Origin of current UCS (in WCS).
        /// </summary>
        public Vector3 UcsOrg
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrg].Value;
            set => variables[HeaderVariableCode.UcsOrg].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to BACK when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgBack
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgBack].Value;
            set => variables[HeaderVariableCode.UcsOrgBack].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to BOTTOM when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgBottom
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgBottom].Value;
            set => variables[HeaderVariableCode.UcsOrgBottom].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to FRONT when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgFront
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgFront].Value;
            set => variables[HeaderVariableCode.UcsOrgFront].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to LEFT when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgLeft
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgLeft].Value;
            set => variables[HeaderVariableCode.UcsOrgLeft].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to RIGHT when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgRight
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgRight].Value;
            set => variables[HeaderVariableCode.UcsOrgRight].Value = value;
        }

        /// <summary>
        /// Point which becomes the new UCS origin after changing model space UCS to TOP when UCSBASE is set to WORLD
        /// </summary>
        public Vector3 UcsOrgTop
        {
            get => (Vector3)variables[HeaderVariableCode.UcsOrgTop].Value;
            set => variables[HeaderVariableCode.UcsOrgTop].Value = value;
        }

        /// <summary>
        /// If model space UCS is orthographic (UCSORTHOVIEW not equal to 0),
        /// this is the name of the UCS that the orthographic UCS is relative to.
        /// If blank, UCS is relative to WORLD
        /// </summary>
        public string UcsOrthoRef
        {
            get => (string)variables[HeaderVariableCode.UcsOrthoRef].Value;
            set => variables[HeaderVariableCode.UcsOrthoRef].Value = value;
        }

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
        public short UcsOrthoView
        {
            get => (short)variables[HeaderVariableCode.UcsOrthoView].Value;
            set => variables[HeaderVariableCode.UcsOrthoView].Value = value;
        }

        /// <summary>
        /// Direction of the current UCS X axis (in WCS).
        /// </summary>
        /// <remarks>
        /// The vectors UcsXDir and UcsYDir must be perpendicular.
        /// </remarks>
        public Vector3 UcsXDir
        {
            get => (Vector3)variables[HeaderVariableCode.UcsXDir].Value;
            set => variables[HeaderVariableCode.UcsXDir].Value = value;
        }

        /// <summary>
        /// Direction of the current UCS Y axis (in WCS).
        /// </summary>
        /// <remarks>
        /// The vectors UcsXDir and UcsYDir must be perpendicular.
        /// </remarks>
        public Vector3 UcsYDir
        {
            get => (Vector3)variables[HeaderVariableCode.UcsYDir].Value;
            set => variables[HeaderVariableCode.UcsYDir].Value = value;
        }

        /// <summary>
        /// Low bit set = Display fractions,
        /// feet-and-inches,
        /// and surveyor's angles in input format
        /// </summary>
        public short UnitMode
        {
            get => (short)variables[HeaderVariableCode.UnitMode].Value;
            set => variables[HeaderVariableCode.UnitMode].Value = value;
        }

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public short UserI1
        {
            get => (short)variables[HeaderVariableCode.UserI1].Value;
            set => variables[HeaderVariableCode.UserI1].Value = value;
        }

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public short UserI2
        {
            get => (short)variables[HeaderVariableCode.UserI2].Value;
            set => variables[HeaderVariableCode.UserI2].Value = value;
        }

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public short UserI3
        {
            get => (short)variables[HeaderVariableCode.UserI3].Value;
            set => variables[HeaderVariableCode.UserI3].Value = value;
        }

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public short UserI4
        {
            get => (short)variables[HeaderVariableCode.UserI4].Value;
            set => variables[HeaderVariableCode.UserI4].Value = value;
        }

        /// <summary>
        /// Five integer variables intended for use by third-party developers
        /// </summary>
        public short UserI5
        {
            get => (short)variables[HeaderVariableCode.UserI5].Value;
            set => variables[HeaderVariableCode.UserI5].Value = value;
        }

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public double UserR1
        {
            get => (double)variables[HeaderVariableCode.UserR1].Value;
            set => variables[HeaderVariableCode.UserR1].Value = value;
        }

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public double UserR2
        {
            get => (double)variables[HeaderVariableCode.UserR2].Value;
            set => variables[HeaderVariableCode.UserR2].Value = value;
        }

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public double UserR3
        {
            get => (double)variables[HeaderVariableCode.UserR3].Value;
            set => variables[HeaderVariableCode.UserR3].Value = value;
        }

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public double UserR4
        {
            get => (double)variables[HeaderVariableCode.UserR4].Value;
            set => variables[HeaderVariableCode.UserR4].Value = value;
        }

        /// <summary>
        /// Five real variables intended for use by third-party developers
        /// </summary>
        public double UserR5
        {
            get => (double)variables[HeaderVariableCode.UserR5].Value;
            set => variables[HeaderVariableCode.UserR5].Value = value;
        }

        /// <summary>
        /// Controls the user timer for the drawing:
        /// 0 = Timer off
        /// 1 = Timer on
        /// </summary>
        public short UserTimer
        {
            get => (short)variables[HeaderVariableCode.UserTimer].Value;
            set => variables[HeaderVariableCode.UserTimer].Value = value;
        }

        /// <summary>
        /// Uniquely identifies a particular version of a drawing.
        /// Updated when the drawing is modified
        /// </summary>
        public string VersionGuid
        {
            get => (string)variables[HeaderVariableCode.VersionGuid].Value;
            set => variables[HeaderVariableCode.VersionGuid].Value = value;
        }

        /// <summary>
        /// Controls the properties of xref-dependent layers:
        /// 0 = Don't retain xref-dependent visibility settings
        /// 1 = Retain xref-dependent visibility settings
        /// </summary>
        public short VisRetain
        {
            get => (short)variables[HeaderVariableCode.VisRetain].Value;
            set => variables[HeaderVariableCode.VisRetain].Value = value;
        }

        /// <summary>
        /// Determines whether input for the DVIEW and VPOINT command
        /// evaluated as relative to the WCS or current UCS:
        /// 0 = Don't change UCS
        /// 1 = Set UCS to WCS during DVIEW/VPOINT
        /// </summary>
        public short Worldview
        {
            get => (short)variables[HeaderVariableCode.Worldview].Value;
            set => variables[HeaderVariableCode.Worldview].Value = value;
        }

        /// <summary>
        /// Controls the visibility of xref clipping boundaries:
        /// 0 = Clipping boundary is not visible
        /// 1 = Clipping boundary is visible
        /// </summary>
        public short XClipFrame
        {
            get => (short)variables[HeaderVariableCode.XClipFrame].Value;
            set => variables[HeaderVariableCode.XClipFrame].Value = value;
        }

        /// <summary>
        /// Controls whether the current drawing can be edited in-place
        /// when being referenced by another drawing:
        /// 0 = Can't use in-place reference editing
        /// 1 = Can use in-place reference editing
        /// </summary>
        public bool XEdit
        {
            get => (bool)variables[HeaderVariableCode.XEdit].Value;
            set => variables[HeaderVariableCode.XEdit].Value = value;
        }

        /// <summary>
        /// User name that saved the file.
        /// </summary>
        /// <remarks>
        /// By default it uses the user name of the person who is currently logged on to the Windows operating system.<br />
        /// This header variable is not compatible with AutoCad2000 or lower versions.
        /// </remarks>
        public string LastSavedBy
        {
            get => (string) variables[HeaderVariableCode.LastSavedBy].Value;
            set => variables[HeaderVariableCode.LastSavedBy].Value = value;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Gets a collection of the known header variables.
        /// </summary>
        /// <returns>A list with the known header variables.</returns>
        public List<HeaderVariable> KnownValues()
        {
            return new List<HeaderVariable>(variables.Values);
        }

        /// <summary>
        /// Gets a collection of the known header variables names.
        /// </summary>
        /// <returns>A list with the known header variables names.</returns>
        public List<string> KnownNames()
        {
            return new List<string>(variables.Keys);
        }

        /// <summary>
        /// Gets a collection of the custom header variables.
        /// </summary>
        /// <returns>A list with the custom header variables.</returns>
        public List<HeaderVariable> CustomValues()
        {
            return new List<HeaderVariable>(customVariables.Values);
        }

        /// <summary>
        /// Gets a collection of the custom header variables names.
        /// </summary>
        /// <returns>A list with the custom header variables names.</returns>
        public List<string> CustomNames()
        {
            return new List<string>(customVariables.Keys);
        }

        /// <summary>
        /// Adds a custom <see cref="HeaderVariable">HeaderVariable</see> to the list.
        /// </summary>
        /// <param name="variable">Header variable to add to the list.</param>
        /// <remarks>
        /// All header variable names must start with the character '$'.<br />
        /// Header variable names that already exists in the known list cannot be added.
        /// </remarks>
        public void AddCustomVariable(HeaderVariable variable)
        {
            if (variable == null)
            {
                throw new ArgumentNullException(nameof(variable), "A custom header variable cannot be null.");
            }

            if (!variable.Name.StartsWith("$"))
            {
                throw new ArgumentException("A header variable name must start with '$'.", nameof(variable));
            }

            if (variables.ContainsKey(variable.Name))
            {
                throw new ArgumentException("A known header variable with the same name already exists.", nameof(variable));
            }

            customVariables.Add(variable.Name, variable);
        }

        /// <summary>
        /// Checks if a custom <see cref="HeaderVariable">HeaderVariable</see> name exits in the list.
        /// </summary>
        /// <param name="name">Header variable name.</param>
        /// <returns>True if a header variable name exits in the list; otherwise, false.</returns>
        /// <remarks>The header variable name is case insensitive.</remarks>
        public bool ContainsCustomVariable(string name)
        {
            return customVariables.ContainsKey(name);
        }

        /// <summary>Gets the header variable associated with the specified name.</summary>
        /// <param name="name">The name of the header variable to get.</param>
        /// <param name="variable">When this method returns, contains the header variable associated with the specified name, if the name is found; otherwise, it contains null.</param>
        /// <returns>True if the list contains a header variable with the specified name; otherwise, false.</returns>
        public bool TryGetCustomVariable(string name, out HeaderVariable variable)
        {
            return customVariables.TryGetValue(name, out variable);
        }

        /// <summary>
        /// Removes a custom <see cref="HeaderVariable">HeaderVariable</see> from the list.
        /// </summary>
        /// <param name="name">Header variable to add to the list.</param>
        /// <returns>True if the element is successfully found and removed; otherwise, false.</returns>
        /// <remarks>The header variable name is case insensitive.</remarks>
        public bool RemoveCustomVariable(string name)
        {
            return customVariables.Remove(name);
        }

        /// <summary>
        /// Removes all custom <see cref="HeaderVariable">HeaderVariable</see> from the list.
        /// </summary>
        public void ClearCustomVariables()
        {
            customVariables.Clear();
        }

        #endregion
    }
}