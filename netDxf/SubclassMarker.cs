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

namespace netDxf
{
    /// <summary>
    /// Dxf object subclass string markers (code 100).
    /// </summary>
    internal static class SubclassMarker
    {
        public const string ApplicationId = "AcDbRegAppTableRecord";
        public const string Table = "AcDbSymbolTable";
        public const string TableRecord = "AcDbSymbolTableRecord";
        public const string Layer = "AcDbLayerTableRecord";
        public const string VPort = "AcDbViewportTableRecord";
        public const string View = "AcDbViewTableRecord";
        public const string Linetype = "AcDbLinetypeTableRecord";
        public const string TextStyle = "AcDbTextStyleTableRecord";
        public const string MLineStyle = "AcDbMlineStyle";
        public const string DimensionStyleTable = "AcDbDimStyleTable";
        public const string DimensionStyle = "AcDbDimStyleTableRecord";
        public const string Ucs = "AcDbUCSTableRecord";
        public const string Dimension = "AcDbDimension";
        public const string AlignedDimension = "AcDbAlignedDimension";
        public const string LinearDimension = "AcDbRotatedDimension";
        public const string RadialDimension = "AcDbRadialDimension";
        public const string DiametricDimension = "AcDbDiametricDimension";
        public const string Angular3PointDimension = "AcDb3PointAngularDimension";
        public const string Angular2LineDimension = "AcDb2LineAngularDimension";
        public const string OrdinateDimension = "AcDbOrdinateDimension";
        public const string BlockRecord = "AcDbBlockTableRecord";
        public const string BlockBegin = "AcDbBlockBegin";
        public const string BlockEnd = "AcDbBlockEnd";
        public const string Entity = "AcDbEntity";
        public const string Arc = "AcDbArc";
        public const string Circle = "AcDbCircle";
        public const string Ellipse = "AcDbEllipse";
        public const string Spline = "AcDbSpline";
        public const string Face3d = "AcDbFace";
        public const string Helix = "AcDbHelix";
        public const string Insert = "AcDbBlockReference";
        public const string Line = "AcDbLine";
        public const string Ray = "AcDbRay";
        public const string XLine = "AcDbXline";
        public const string MLine = "AcDbMline";
        public const string Point = "AcDbPoint";
        public const string Vertex = "AcDbVertex";
        public const string Polyline = "AcDb2dPolyline";
        public const string Leader = "AcDbLeader";
        public const string LightWeightPolyline = "AcDbPolyline";
        public const string PolylineVertex = "AcDb2dVertex ";
        public const string Polyline3d = "AcDb3dPolyline";
        public const string Polyline3dVertex = "AcDb3dPolylineVertex";
        public const string PolyfaceMesh = "AcDbPolyFaceMesh";
        public const string PolyfaceMeshVertex = "AcDbPolyFaceMeshVertex";
        public const string PolyfaceMeshFace = "AcDbFaceRecord";
        public const string Shape = "AcDbShape";
        public const string Solid = "AcDbTrace";
        public const string Trace = "AcDbTrace";
        public const string Text = "AcDbText";
        public const string Tolerance = "AcDbFcf";
        public const string Wipeout = "AcDbWipeout";
        public const string Mesh = "AcDbSubDMesh";
        public const string MText = "AcDbMText";
        public const string Hatch = "AcDbHatch";
        public const string Underlay = "AcDbUnderlayReference";
        public const string UnderlayDefinition = "AcDbUnderlayDefinition";
        public const string Viewport = "AcDbViewport";
        public const string Attribute = "AcDbAttribute";
        public const string AttributeDefinition = "AcDbAttributeDefinition";
        public const string Dictionary = "AcDbDictionary";
        public const string RasterImage = "AcDbRasterImage";
        public const string RasterImageDef = "AcDbRasterImageDef";
        public const string RasterImageDefReactor = "AcDbRasterImageDefReactor";
        public const string RasterVariables = "AcDbRasterVariables";
        public const string Group = "AcDbGroup";
        public const string Layout = "AcDbLayout";
        public const string PlotSettings = "AcDbPlotSettings";
        public const string ModelerGeometry = "AcDbModelerGeometry";
        public const string Solid3d = "AcDb3dSolid";
        public const string Xrecord = "AcDbXrecord";
        public const string NavisworksModel = "AcDbNavisworksModel";
        public const string Light = "AcDbLight";
        public const string OleFrame = "AcDbOleFrame";
        public const string Ole2Frame = "AcDbOle2Frame";
        public const string Section = "AcDbSection";
        public const string Sun = "AcDbSun";
        public const string Surface = "AcDbSurface";
        public const string VbaProject = "AcDbVbaProject";
        public const string ProxyObject = "AcDbProxyObject";
        public const string DictionaryWithDefault = "AcDbDictionaryWithDefault";
        public const string NavisworksModelDef = "AcDbNavisworksModelDef";
        public const string DataTable = "AcDbDataTable";
        public const string DimAssoc = "AcDbDimAssoc";
        public const string IdBuffer = "AcDbIdBuffer";
        public const string Filter = "AcDbFilter";
        public const string LayerFilter = "AcDbLayerFilter";
        public const string Index = "AcDbIndex";
        public const string LayerIndex = "AcDbLayerIndex";
        public const string LightList = "AcDbLightList";
        public const string Material = "AcDbMaterial";
        public const string MentalRayRenderSettings = "AcDbMentalRayRenderSettings";
        public const string RenderEnvironment = "AcDbRenderEnvironment";
        public const string RenderGlobal = "AcDbRenderGlobal";
        public const string IBLBackground = "AcDbIBLBackground";
        public const string RenderSettings = "AcDbRenderSettings";
        public const string RapidRTRenderSettings = "AcDbRapidRTRenderSettings";
        public const string SectionManager = "AcDbSectionManager";
        public const string SectionSettings = "AcDbSectionSettings";
        public const string SortentsTable = "AcDbSortentsTable";
        public const string SpatialFilter = "AcDbSpatialFilter";
        public const string SpatialIndex = "AcDbSpatialIndex";
        public const string SunStudy = "AcDbSunStudy";
        public const string TableStyle = "AcDbTableStyle";
        public const string VisualStyle = "AcDbVisualStyle";
        public const string ProxyEntity = "AcDbProxyEntity";
        public const string ExtrudedSurface = "AcDbExtrudedSurface";
        public const string LoftedSurface = "AcDbLoftedSurface";
        public const string RevolvedSurface = "AcDbRevolvedSurface";
        public const string SweptSurface = "AcDbSweptSurface";
    }
}