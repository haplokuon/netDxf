#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2014 Daniel Carvajal (haplokuon@gmail.com)
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
    /// Dxf string codes.
    /// </summary>
    public sealed class DxfObjectCode
    {
        /// <summary>
        /// Not defined.
        /// </summary>
        public const string Unknown = "";

        /// <summary>
        /// Header section.
        /// </summary>
        public const string HeaderSection = "HEADER";

        /// <summary>
        /// Classes section.
        /// </summary>
        public const string ClassesSection = "CLASSES";

        /// <summary>
        /// Class entry.
        /// </summary>
        public const string Class = "CLASS";

        /// <summary>
        /// Tables section.
        /// </summary>
        public const string TablesSection = "TABLES";

        /// <summary>
        /// Blocks section.
        /// </summary>
        public const string BlocksSection = "BLOCKS";

        /// <summary>
        /// Entities section.
        /// </summary>
        public const string EntitiesSection = "ENTITIES";

        /// <summary>
        /// Objects section.
        /// </summary>
        public const string ObjectsSection = "OBJECTS";

        /// <summary>
        /// Thumbnail section.
        /// </summary>
        public const string ThumbnailImageSection = "THUMBNAILIMAGE";

        /// <summary>
        /// AcdsData section. Currently it is used for storing the data for solids, regions, surfaces, and the preview image.
        /// </summary>
        public const string AcdsDataSection = "ACDSDATA";

        /// <summary>
        /// Begin section code.
        /// </summary>
        public const string BeginSection = "SECTION";

        /// <summary>
        /// End section code.
        /// </summary>
        public const string EndSection = "ENDSEC";

        /// <summary>
        /// Layers.
        /// </summary>
        public const string LayerTable = "LAYER";

        /// <summary>
        /// View ports.
        /// </summary>
        public const string VportTable = "VPORT";

        /// <summary>
        /// Views.
        /// </summary>
        public const string ViewTable = "VIEW";

        /// <summary>
        /// Ucs.
        /// </summary>
        public const string UcsTable = "UCS";

        /// <summary>
        /// Block records.
        /// </summary>
        public const string BlockRecordTable = "BLOCK_RECORD";

        /// <summary>
        /// Line types.
        /// </summary>
        public const string LineTypeTable = "LTYPE";

        /// <summary>
        /// text styles.
        /// </summary>
        public const string TextStyleTable = "STYLE";

        /// <summary>
        /// Dimension styles.
        /// </summary>
        public const string DimensionStyleTable = "DIMSTYLE";

        /// <summary>
        /// extended data application registry.
        /// </summary>
        public const string ApplicationIDTable = "APPID";

        /// <summary>
        /// Begin table code.
        /// </summary>
        public const string Table = "TABLE";

        /// <summary>
        /// End table code.
        /// </summary>
        public const string EndTable = "ENDTAB";

        /// <summary>
        /// Begin block code.
        /// </summary>
        public const string BeginBlock = "BLOCK";

        /// <summary>
        /// End block code.
        /// </summary>
        public const string EndBlock = "ENDBLK";

        /// <summary>
        /// Group dictionary
        /// </summary>
        public const string GroupDictionary = "ACAD_GROUP";

        /// <summary>
        /// Layouts dictionary
        /// </summary>
        public const string LayoutDictionary = "ACAD_LAYOUT";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string MLineStyleDictionary = "ACAD_MLINESTYLE";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string ImageDefDictionary = "ACAD_IMAGE_DICT";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string ImageVarsDictionary = "ACAD_IMAGE_VARS";

        /// <summary>
        /// End of file
        /// </summary>
        public const string EndOfFile = "EOF";

        /// <summary>
        /// Application registry.
        /// </summary>
        public const string AppId = "APPID";

        /// <summary>
        /// Dimension style.
        /// </summary>
        public const string DimStyle = "DIMSTYLE";

        /// <summary>
        /// Block record.
        /// </summary>
        public const string BlockRecord = "BLOCK_RECORD";

        /// <summary>
        /// Line type.
        /// </summary>
        public const string LineType = "LTYPE";

        /// <summary>
        /// Layer.
        /// </summary>
        public const string Layer = "LAYER";

        /// <summary>
        /// Viewport table object.
        /// </summary>
        public const string VPort = "VPORT";

        /// <summary>
        /// Text style.
        /// </summary>
        public const string TextStyle = "STYLE";

        /// <summary>
        /// MLine style.
        /// </summary>
        public const string MLineStyle = "MLINESTYLE";

        /// <summary>
        /// View.
        /// </summary>
        public const string View = "VIEW";

        /// <summary>
        /// Ucs.
        /// </summary>
        public const string Ucs = "UCS";

        /// <summary>
        /// Block.
        /// </summary>
        public const string Block = "BLOCK";

        /// <summary>
        /// End block.
        /// </summary>
        public const string BlockEnd = "ENDBLK";

        /// <summary>
        /// Line.
        /// </summary>
        public const string Line = "LINE";

        /// <summary>
        /// Ray.
        /// </summary>
        public const string Ray = "RAY";

        /// <summary>
        /// XLine.
        /// </summary>
        public const string XLine = "XLINE";

        /// <summary>
        /// Ellipse.
        /// </summary>
        public const string Ellipse = "ELLIPSE";

        /// <summary>
        /// Polyline.
        /// </summary>
        public const string Polyline = "POLYLINE";

        /// <summary>
        /// Lightweight polyline.
        /// </summary>
        public const string LightWeightPolyline = "LWPOLYLINE";

        /// <summary>
        /// Circle.
        /// </summary>
        public const string Circle = "CIRCLE";

        /// <summary>
        /// Point.
        /// </summary>
        public const string Point = "POINT";

        /// <summary>
        /// Arc.
        /// </summary>
        public const string Arc = "ARC";

        /// <summary>
        /// Spline (nonuniform rational B-splines NURBS).
        /// </summary>
        public const string Spline = "SPLINE";

        /// <summary>
        /// Solid.
        /// </summary>
        public const string Solid = "SOLID";

        /// <summary>
        /// Text string.
        /// </summary>
        public const string Text = "TEXT";

        /// <summary>
        /// Mesh.
        /// </summary>
        public const string Mesh = "MESH";

        /// <summary>
        /// Multiline text string.
        /// </summary>
        public const string MText = "MTEXT";

        /// <summary>
        /// MLine.
        /// </summary>
        public const string MLine = "MLINE";

        /// <summary>
        /// 3d face.
        /// </summary>
        public const string Face3d = "3DFACE";

        /// <summary>
        /// Helix.
        /// </summary>
        public const string Helix = "HELIX";

        /// <summary>
        /// Block insertion.
        /// </summary>
        public const string Insert = "INSERT";

        /// <summary>
        /// Hatch.
        /// </summary>
        public const string Hatch = "HATCH";

        /// <summary>
        /// Attribute definition.
        /// </summary>
        public const string AttributeDefinition = "ATTDEF";

        /// <summary>
        /// Attribute.
        /// </summary>
        public const string Attribute = "ATTRIB";

        /// <summary>
        /// Vertex.
        /// </summary>
        public const string Vertex = "VERTEX";

        /// <summary>
        /// End sequence.
        /// </summary>
        public const string EndSequence = "SEQEND";

        /// <summary>
        /// Dimension.
        /// </summary>
        public const string Dimension = "DIMENSION";

        /// <summary>
        /// Dictionary.
        /// </summary>
        public const string Dictionary = "DICTIONARY";

        /// <summary>
        /// Raster image.
        /// </summary>
        public const string Image = "IMAGE";

        /// <summary>
        /// Viewport entity.
        /// </summary>
        public const string Viewport = "VIEWPORT";

        /// <summary>
        /// Image definition.
        /// </summary>
        public const string ImageDef = "IMAGEDEF";

        /// <summary>
        /// Image definition reactor.
        /// </summary>
        public const string ImageDefReactor = "IMAGEDEF_REACTOR";

        /// <summary>
        /// Raster variables.
        /// </summary>
        public const string RasterVariables = "RASTERVARIABLES";

        /// <summary>
        /// Groups.
        /// </summary>
        public const string Group = "GROUP";

        /// <summary>
        /// Layouts.
        /// </summary>
        public const string Layout = "LAYOUT";
    }
}