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
using System.Diagnostics;
using System.IO;
using System.Text;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Low level dxf reader
    /// </summary>
    internal sealed class DxfReader
    {

        #region private fields

        private CodeValuePair dxfPairInfo;
        private TextReader reader;
        // keeps track of the dimension blocks generated
        private int dimensionBlocksGenerated;
        // keeps track of the group names generated (this groups have the isUnnamed bool set to true)
        private int groupNamesGenerated;

        //header
        private List<string> comments;
        private HeaderVariables headerVariables;

        //entities
        //entity objects added to the document (key: handle, value: entity). This dictionary also includes entities that are part of a block.
        private Dictionary<string, EntityObject> addedEntity;
        private List<Arc> arcs;
        private List<Circle> circles;
        private List<Point> points;
        private List<Ellipse> ellipses;
        private List<Face3d> faces3d;
        private List<Solid> solids;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<MLine> mLines;
        private List<PolyfaceMesh> polyfaceMeshes;
        private List<LwPolyline> lightWeightPolylines;
        private List<Polyline> polylines;
        private List<Text> texts;
        private List<MText> mTexts;
        private List<Hatch> hatches;
        private List<Dimension> dimensions;
        private List<Spline> splines;
        private List<Image> images;
        private List<Ray> rays;
        private List<XLine> xlines;

        //tables
        private Dictionary<string, ApplicationRegistry> appRegistries;
        private Dictionary<string, List<DxfObject>> appRegistryRefs;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, List<DxfObject>> layerRefs;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, List<DxfObject>> lineTypeRefs;
        private Dictionary<string, TextStyle> textStyles;
        private Dictionary<string, List<DxfObject>> textStyleRefs;
        private Dictionary<string, DimensionStyle> dimStyles;
        private Dictionary<string, List<DxfObject>> dimStyleRefs;
        private Dictionary<string, UCS> ucss;
        private Dictionary<string, List<DxfObject>> ucsRefs;

        //blocks
        private Dictionary<string, BlockRecord> blockRecords;
        private Dictionary<string, Block> blocks;
        private Dictionary<string, List<DxfObject>> blockRefs;

        // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
        // temporarily this variables will store information to post process the nested block list
        private Dictionary<Insert, string> nestedBlocks;
        private Dictionary<Dimension, string> nestedDimBlocks;
        private Dictionary<Attribute, string> nestedBlocksAttributes;

        //objects
        private RasterVariables rasterVariables;
        private Dictionary<string, ImageDef> imageDefs;
        private Dictionary<string, List<DxfObject>> imageDefRefs;
        private Dictionary<string, DictionaryObject> dictionaries;
        private Dictionary<string, ImageDefReactor> imageDefReactors;
        private Dictionary<string, MLineStyle> mlineStyles;
        private Dictionary<string, List<DxfObject>> mlineStyleRefs;
        private Dictionary<string, Group> groups;
        private Dictionary<string, List<DxfObject>> groupRefs;

        // variables for post-processing

        // the MLineStyles are defined, in the objects section, AFTER the MLine that references them,
        // temporarily this variables will store information to post process the MLine list
        private Dictionary<MLine, string> mLineToStyleNames;

        // the ImageDef are defined, in the objects section AFTER the Image that references them,
        // temporarily this variables will store information to post process the Image list
        private Dictionary<Image, string> imgToImgDefHandles;
        private Dictionary<string, ImageDef> imgDefHandles;
        
        #endregion

        #region constructors

        #endregion

        #region public properties

        public List<string> Comments
        {
            get { return this.comments; }
        }

        public int DimensionBlocksGenerated
        {
            get { return this.dimensionBlocksGenerated; }
        }

        public int GroupNamesGenerated
        {
            get { return this.groupNamesGenerated; }
        }

        public Dictionary<string, EntityObject> AddedEntity
        {
            get { return this.addedEntity; }
        }

        #endregion

        #region public header properties

        public HeaderVariables HeaderVariables
        {
            get { return headerVariables; }
        }

        #endregion

        #region public entity properties

        public List<Arc> Arcs
        {
            get { return this.arcs; }
        }

        public List<Circle> Circles
        {
            get { return this.circles; }
        }

        public List<Dimension> Dimensions
        {
            get { return this.dimensions; }
        }

        public List<Ellipse> Ellipses
        {
            get { return this.ellipses; }
        }

        public List<Point> Points
        {
            get { return this.points; }
        }

        public List<Face3d> Faces3d
        {
            get { return this.faces3d; }
        }

        public List<Solid> Solids
        {
            get { return this.solids; }
        }

        public List<Spline> Splines
        {
            get { return this.splines; }
        }

        public List<Line> Lines
        {
            get { return this.lines; }
        }

        public List<MLine> MLines
        {
            get { return this.mLines; }
        }

        public List<LwPolyline> LightWeightPolyline
        {
            get { return this.lightWeightPolylines; }
        }

        public List<Polyline> Polylines
        {
            get { return this.polylines; }
        }

        public List<PolyfaceMesh> PolyfaceMeshes
        {
            get { return this.polyfaceMeshes; }
        }

        public List<Insert> Inserts
        {
            get { return this.inserts; }
        }

        public List<Text> Texts
        {
            get { return this.texts; }
        }

        public List<MText> MTexts
        {
            get { return this.mTexts; }
        }

        public List<Hatch> Hatches
        {
            get { return this.hatches; }
        }

        public List<Image> Images
        {
            get { return images; }
        }

        public List<Ray> Rays
        {
            get { return this.rays; }
        }

        public List<XLine> XLines
        {
            get { return this.xlines; }
        }

        #endregion

        #region public table properties

        public Dictionary<string, ApplicationRegistry> ApplicationRegistries
        {
            get { return this.appRegistries; }
        }

        public Dictionary<string, List<DxfObject>> ApplicationRegistryReferences
        {
            get { return this.appRegistryRefs; }
        }

        public Dictionary<string, Layer> Layers
        {
            get { return this.layers; }
        }

        public Dictionary<string, List<DxfObject>> LayerReferences
        {
            get { return this.layerRefs; }
        }

        public Dictionary<string, LineType> LineTypes
        {
            get { return this.lineTypes; }
        }

        public Dictionary<string, List<DxfObject>> LineTypeReferences
        {
            get { return this.lineTypeRefs; }
        }

        public Dictionary<string, TextStyle> TextStyles
        {
            get { return this.textStyles; }
        }

        public Dictionary<string, List<DxfObject>> TextStyleReferences
        {
            get { return this.textStyleRefs; }
        }

        public Dictionary<string, DimensionStyle> DimensionStyles
        {
            get { return this.dimStyles; }
        }

        public Dictionary<string, List<DxfObject>> DimensionStyleReferences
        {
            get { return this.dimStyleRefs; }
        }

        public Dictionary<string, Block> Blocks
        {
            get { return this.blocks; }
        }

        public Dictionary<string, List<DxfObject>> BlockReferences
        {
            get { return this.blockRefs; }
        }

        public Dictionary<string, MLineStyle> MLineStyles
        {
            get { return mlineStyles; }
        }

        public Dictionary<string, List<DxfObject>> MLineStyleReferences
        {
            get { return this.mlineStyleRefs; }
        }

        public Dictionary<string, UCS> UCSs
        {
            get { return this.ucss; }
        }

        public Dictionary<string, List<DxfObject>> UCSReferences
        {
            get { return this.ucsRefs; }
        }

        #endregion

        #region public object properties

        public Dictionary<string, ImageDef> ImageDefs
        {
            get { return imageDefs; }
        }

        public Dictionary<string, List<DxfObject>> ImageDefReferences
        {
            get { return this.imageDefRefs; }
        }

        public RasterVariables RasterVariables
        {
            get { return rasterVariables; }
        }

        public Dictionary<string, Group> Groups
        {
            get { return groups; }
        }

        public Dictionary<string, List<DxfObject>> GroupReferences
        {
            get { return this.groupRefs; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Reads the whole stream.
        /// </summary>
        /// <param name="stream">Stream</param>
        public void Read(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "The stream cannot be null");

            // in case of error trying to get the code page we will default to the most common one 1252 Latin 1; Western European (Windows)
            int codepage;
            string encoding = CheckHeaderVariable(stream, HeaderVariableCode.DwgCodePage) ?? "ANSI_1252";
            if (!int.TryParse(encoding.Split('_')[1], out codepage))
                codepage = 1252;

            try
            {
                this.reader = new StreamReader(stream, Encoding.GetEncoding(codepage));
            }
            catch (Exception ex)
            {
                throw (new DxfException("Unknow error opening the reader.", ex));
            }

            this.addedEntity = new Dictionary<string, EntityObject>();

            this.comments = new List<string>();

            // header
            this.headerVariables = new HeaderVariables();

            // tables
            this.appRegistries = new Dictionary<string, ApplicationRegistry>(StringComparer.InvariantCultureIgnoreCase);
            this.appRegistryRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.layers = new Dictionary<string, Layer>(StringComparer.InvariantCultureIgnoreCase);
            this.layerRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.lineTypes = new Dictionary<string, LineType>(StringComparer.InvariantCultureIgnoreCase);
            this.lineTypeRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.textStyles = new Dictionary<string, TextStyle>(StringComparer.InvariantCultureIgnoreCase);
            this.textStyleRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.dimStyles = new Dictionary<string, DimensionStyle>(StringComparer.InvariantCultureIgnoreCase);
            this.dimStyleRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.ucss = new Dictionary<string, UCS>(StringComparer.InvariantCultureIgnoreCase);
            this.ucsRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);

            // blocks
            this.nestedBlocks = new Dictionary<Insert, string>();
            this.nestedDimBlocks = new Dictionary<Dimension, string>();
            this.nestedBlocksAttributes = new Dictionary<Attribute, string>();
            this.blockRecords = new Dictionary<string, BlockRecord>(StringComparer.InvariantCultureIgnoreCase);
            this.blocks = new Dictionary<string, Block>(StringComparer.InvariantCultureIgnoreCase);
            this.blockRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);

            // entities
            this.arcs = new List<Arc>();
            this.circles = new List<Circle>();
            this.faces3d = new List<Face3d>();
            this.ellipses = new List<Ellipse>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.lines = new List<Line>();
            this.polyfaceMeshes = new List<PolyfaceMesh>();
            this.lightWeightPolylines = new List<LwPolyline>();
            this.polylines = new List<Polyline>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
            this.mTexts = new List<MText>();
            this.hatches = new List<Hatch>();
            this.dimensions = new List<Dimension>();
            this.splines = new List<Spline>();
            this.images = new List<Image>();
            this.mLines = new List<MLine>();
            this.rays = new List<Ray>();
            this.xlines = new List<XLine>();

            // objects
            this.dictionaries = new Dictionary<string, DictionaryObject>(StringComparer.InvariantCultureIgnoreCase);
            this.imageDefs = new Dictionary<string, ImageDef>(StringComparer.InvariantCultureIgnoreCase);
            this.imageDefRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.imageDefReactors = new Dictionary<string, ImageDefReactor>(StringComparer.InvariantCultureIgnoreCase);
            this.imgDefHandles = new Dictionary<string, ImageDef>(StringComparer.InvariantCultureIgnoreCase);
            this.imgToImgDefHandles = new Dictionary<Image, string>();
            this.mlineStyles = new Dictionary<string, MLineStyle>(StringComparer.InvariantCultureIgnoreCase);
            this.mlineStyleRefs = new Dictionary<string, List<DxfObject>>(StringComparer.InvariantCultureIgnoreCase);
            this.mLineToStyleNames = new Dictionary<MLine, string>();
            this.groups = new Dictionary<string, Group>(StringComparer.InvariantCultureIgnoreCase);
            this.groupRefs = new Dictionary<string, List<DxfObject>>();

            dxfPairInfo = this.ReadCodePair();

            // read the comments at the head of the file, any other comments will be ignored
            // they sometimes hold information about the program that has generated the dxf
            while (dxfPairInfo.Code == 999)
            {
                 this.comments.Add(dxfPairInfo.Value);
                 dxfPairInfo = this.ReadCodePair();
            }

            while (dxfPairInfo.Value != StringCode.EndOfFile)
            {
                if (dxfPairInfo.Value == StringCode.BeginSection)
                {
                    dxfPairInfo = this.ReadCodePair();
                    switch (dxfPairInfo.Value)
                    {
                        case StringCode.HeaderSection:
                            this.ReadHeader();
                            break;
                        case StringCode.ClassesSection:
                            this.ReadClasses();
                            break;
                        case StringCode.TablesSection:
                            this.ReadTables();
                            break;
                        case StringCode.BlocksSection:
                            this.ReadBlocks();
                            break;
                        case StringCode.EntitiesSection:
                            this.ReadEntities();
                            break;
                        case StringCode.ObjectsSection:
                            this.ReadObjects();
                            break;
                        default:
                            throw new InvalidDxfSectionException(dxfPairInfo.Value, "Unknown section " + dxfPairInfo.Value + ".");
                    }
                }
                dxfPairInfo = this.ReadCodePair();
            }
            stream.Position = 0;

            // postprocess the image list to assign their image definitions.
            foreach (KeyValuePair<Image, string> pair in this.imgToImgDefHandles)
            {
                Image image = pair.Key;
                image.Definition = this.imgDefHandles[pair.Value];
                this.imageDefRefs[image.Definition.Name].Add(image);
                image.Definition.Reactors.Add(image.Handle, this.imageDefReactors[image.Handle]);
            }

            // postprocess the MLines to assign their MLineStyle
            foreach (KeyValuePair<MLine, string> pair in this.mLineToStyleNames)
            {
                MLine mline = pair.Key;
                mline.Style = this.GetMLineStyle(pair.Value);
                this.mlineStyleRefs[pair.Value].Add(mline);
            }
        }

        public static string CheckHeaderVariable(Stream stream, string headerVariable)
        {
            StreamReader reader = new StreamReader(stream, Encoding.ASCII); // use the basic encoding to read only the header info
            CodeValuePair dxfPairInfo = ReadCodePair(reader);
            while (dxfPairInfo.Value != StringCode.EndOfFile)
            {
                dxfPairInfo = ReadCodePair(reader);
                if (dxfPairInfo.Value == StringCode.HeaderSection)
                {
                    dxfPairInfo = ReadCodePair(reader);
                    while (dxfPairInfo.Value != StringCode.EndSection)
                    {
                        if (HeaderVariable.Allowed.ContainsKey(dxfPairInfo.Value))
                        {
                            int codeGroup = HeaderVariable.Allowed[dxfPairInfo.Value];
                            string variableName = dxfPairInfo.Value;
                            dxfPairInfo = ReadCodePair(reader);
                            if (dxfPairInfo.Code != codeGroup)
                                throw new DxfHeaderVariableException(variableName, "Invalid variable name and code group convination");
                            if (variableName == headerVariable)
                            {
                                // we found the variable we are looking for
                                string output = dxfPairInfo.Value;
                                stream.Position = 0;
                                return output;
                            }
                        }
                        dxfPairInfo = ReadCodePair(reader);
                    }
                    // we only need to read the header section
                    stream.Position = 0;
                    return null;
                }
            }

            stream.Position = 0;
            return null;
        }

        #endregion

        #region sections methods

        private void ReadHeader()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                if (HeaderVariable.Allowed.ContainsKey(dxfPairInfo.Value))
                {
                    int codeGroup = HeaderVariable.Allowed[dxfPairInfo.Value];
                    string variableName = dxfPairInfo.Value;
                    dxfPairInfo = this.ReadCodePair();
                    if (dxfPairInfo.Code != codeGroup)
                        throw new DxfHeaderVariableException(variableName, "Invalid variable name and code group convination.");
                    switch (variableName)
                    {
                        case HeaderVariableCode.AcadVer:
                            DxfVersion acadVer = DxfVersion.Unknown;
                            if(StringEnum.IsStringDefined(typeof(DxfVersion), dxfPairInfo.Value))
                                acadVer = (DxfVersion) StringEnum.Parse(typeof(DxfVersion), dxfPairInfo.Value);
                            if (acadVer < DxfVersion.AutoCad2000)
                                throw new NotSupportedException("Only AutoCad2000 and higher dxf versions are supported.");
                            this.headerVariables.AcadVer = acadVer;
                            break;
                        case HeaderVariableCode.HandleSeed:
                            this.headerVariables.HandleSeed = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.Angbase:
                            this.headerVariables.Angbase = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.Angdir:
                            this.headerVariables.Angdir = int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.AttMode:
                            this.headerVariables.AttMode = (AttMode)int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.AUnits:
                            this.headerVariables.AUnits = int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.AUprec:
                            this.headerVariables.AUprec = int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.CeColor:
                            short colorIndex = short.Parse(dxfPairInfo.Value);
                            AciColor color;
                            switch (colorIndex)
                            {
                                case 0:
                                    color = AciColor.ByBlock;
                                    break;
                                case 256:
                                    color = AciColor.ByLayer;
                                    break;
                                default:
                                    color = new AciColor(colorIndex);
                                    break;
                            }
                            this.headerVariables.CeColor = color;
                            break;
                        case HeaderVariableCode.CeLtScale:
                            this.headerVariables.CeLtScale = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.CeLtype:
                            this.headerVariables.CeLtype = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.CeLweight:
                            short weightIndex = short.Parse(dxfPairInfo.Value);
                            Lineweight lineweight;
                            switch (weightIndex)
                            {
                                case -3:
                                    lineweight = Lineweight.Default;
                                    break;
                                case -2:
                                    lineweight = Lineweight.ByBlock;
                                    break;
                                case -1:
                                    lineweight = Lineweight.ByLayer;
                                    break;
                                default:
                                    lineweight = new Lineweight(weightIndex);
                                    break;
                            }
                            this.headerVariables.CeLweight = lineweight;
                            break;
                        case HeaderVariableCode.CLayer:
                            this.headerVariables.CLayer = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.CMLJust:
                            this.headerVariables.CMLJust = (MLineJustification) int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.CMLScale:
                            this.headerVariables.CMLScale = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.CMLStyle:
                            this.headerVariables.CMLStyle = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.DimStyle:
                            this.headerVariables.DimStyle = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.TextSize:
                            this.headerVariables.TextSize = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.TextStyle:
                            this.headerVariables.TextStyle = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.LastSavedBy:
                            this.headerVariables.LastSavedBy = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.LUnits:
                            this.headerVariables.LUnits = int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.LUprec:
                            this.headerVariables.LUprec = int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.DwgCodePage:
                            this.headerVariables.DwgCodePage = dxfPairInfo.Value;
                            break;
                        case HeaderVariableCode.Extnames:
                            this.headerVariables.Extnames = (int.Parse(dxfPairInfo.Value) != 0);
                            break;
                        case HeaderVariableCode.Insunits:
                            this.headerVariables.Insunits = (DrawingUnits)int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.LtScale:
                            this.headerVariables.LtScale = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.LwDisplay:
                            this.headerVariables.LwDisplay = (int.Parse(dxfPairInfo.Value) != 0);
                            break;
                        case HeaderVariableCode.PdMode:
                            this.headerVariables.PdMode = (PointShape)int.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.PdSize:
                            this.headerVariables.PdSize = double.Parse(dxfPairInfo.Value);
                            break;
                        case HeaderVariableCode.PLineGen:
                            this.headerVariables.PLineGen = int.Parse(dxfPairInfo.Value);
                            break;
                    }
                }
                dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadClasses()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadTables()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                dxfPairInfo = this.ReadCodePair();
                switch (dxfPairInfo.Value)
                {
                    case StringCode.ApplicationIDTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadApplicationsId();
                        break;
                    case StringCode.BlockRecordTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadBlockRecords();
                        break;
                    case StringCode.LayerTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadLayers();
                        break;
                    case StringCode.LineTypeTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadLineTypes();
                        break;
                    case StringCode.TextStyleTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadTextStyles();
                        break;
                    case StringCode.DimensionStyleTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadDimStyles();
                        break;
                    case StringCode.UcsTable:
                        Debug.Assert(dxfPairInfo.Code == 2);
                        this.ReadUCSs();
                        break;
                }
            }
        }

        private void ReadEntities()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                EntityObject entity = ReadEntity(false);
                // the ReadEntity() will return null if an unkown entity has been found
                if (entity == null)
                    continue;

                switch (entity.Type)
                {
                    case EntityType.Arc:
                        this.arcs.Add((Arc)entity);
                        break;
                    case EntityType.Circle:
                        this.circles.Add((Circle)entity);
                        break;
                    case EntityType.Dimension:
                        this.dimensions.Add((Dimension)entity);
                        break;
                    case EntityType.Point:
                        this.points.Add((Point)entity);
                        break;
                    case EntityType.Ellipse:
                        this.ellipses.Add((Ellipse)entity);
                        break;
                    case EntityType.Face3D:
                        this.faces3d.Add((Face3d)entity);
                        break;
                    case EntityType.Solid:
                        this.solids.Add((Solid)entity);
                        break;
                    case EntityType.Spline:
                        this.splines.Add((Spline)entity);
                        break;
                    case EntityType.Insert:
                        this.inserts.Add((Insert)entity);
                        break;
                    case EntityType.Line:
                        this.lines.Add((Line)entity);
                        break;
                    case EntityType.LightWeightPolyline:
                        this.lightWeightPolylines.Add((LwPolyline)entity);
                        break;
                    case EntityType.Polyline:
                        this.polylines.Add((Polyline)entity);
                        break;
                    case EntityType.PolyfaceMesh:
                        this.polyfaceMeshes.Add((PolyfaceMesh)entity);
                        break;
                    case EntityType.Text:
                        this.texts.Add((Text)entity);
                        break;
                    case EntityType.MText:
                        this.mTexts.Add((MText)entity);
                        break;
                    case EntityType.Hatch:
                        this.hatches.Add((Hatch)entity);
                        break;
                    case EntityType.Image:
                        this.images.Add((Image)entity);
                        break;
                    case EntityType.MLine:
                        this.mLines.Add((MLine)entity);
                        break;
                    case EntityType.Ray:
                        this.rays.Add((Ray)entity);
                        break;
                    case EntityType.XLine:
                        this.xlines.Add((XLine)entity);
                        break;
                }
            }
        }

        private void ReadObjects()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (dxfPairInfo.Value)
                {
                    case DxfObjectCode.Dictionary:
                        DictionaryObject dictionary = ReadDictionary();
                        if (this.dictionaries.ContainsKey(dictionary.Handle)) continue;
                        this.dictionaries.Add(dictionary.Handle, dictionary);
                        break;
                    case DxfObjectCode.RasterVariables:
                        this.rasterVariables = ReadRasterVariables();
                        break;
                    case DxfObjectCode.ImageDef:
                        ImageDef imageDef = ReadImageDefinition();
                        if (this.imageDefs.ContainsKey(imageDef.Name)) continue;
                        this.imageDefs.Add(imageDef.Name, imageDef);
                        break;
                    case DxfObjectCode.ImageDefReactor:
                        ImageDefReactor reactor = ReadImageDefReactor();
                        if (this.imageDefReactors.ContainsKey(reactor.ImageHandle)) continue;
                        this.imageDefReactors.Add(reactor.ImageHandle, reactor);
                        break;
                    case DxfObjectCode.MLineStyle:
                        MLineStyle style = ReadMLineStyle();
                        if (style == null) continue;
                        if (this.mlineStyles.ContainsKey(style.Name)) continue;
                        this.mlineStyles.Add(style.Name, style);
                        break;
                    case DxfObjectCode.Group:
                        Group group = ReadGroup();
                        if (this.groups.ContainsKey(group.Name)) continue;
                        this.groups.Add(group.Name, group);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
        }

        #endregion

        #region applicationId methods

        private void ReadApplicationsId()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.ApplicationIDTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    ApplicationRegistry appId = this.ReadApplicationId();
                    if (appId == null) continue;
                    if (this.appRegistries.ContainsKey(appId.Name)) continue;
                    this.appRegistries.Add(appId.Name, appId);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private ApplicationRegistry ReadApplicationId()
        {
            string appId = string.Empty;
            string handle = string.Empty;
            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        appId = dxfPairInfo.Value;
                        break;
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(appId)) return null;

            this.appRegistryRefs.Add(appId, new List<DxfObject>());

            return new ApplicationRegistry(appId)
            {
                Handle = handle
            };
        }

        #endregion

        #region blockrecord methods

        private void ReadBlockRecords()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.BlockRecordTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    BlockRecord blockRecord = this.ReadBlockRecord();
                    if (blockRecord == null) continue;
                    if (this.blockRecords.ContainsKey(blockRecord.Name)) continue;
                    this.blockRecords.Add(blockRecord.Name, blockRecord);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private BlockRecord ReadBlockRecord()
        {
            string handle = null;
            string name = null;
            DrawingUnits units = DrawingUnits.Unitless;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(dxfPairInfo.Value))
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "Invalid value " + dxfPairInfo.Value + " in code " + dxfPairInfo.Code + ".");
                        name = dxfPairInfo.Value;
                        break;
                    case 70:
                        units = (DrawingUnits) int.Parse(dxfPairInfo.Value);
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            // we need to check for generated blocks by dimensions, even if the dimension was deleted the block might persist in the drawing.
            CheckDimBlockName(name);

            return new BlockRecord(name)
                       {
                           Handle = handle,
                           Units = units
                       };
        }

        #endregion

        #region layer methods

        private void ReadLayers()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.LayerTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    Layer layer = this.ReadLayer();
                    if (layer == null) continue;
                    if(this.layers.ContainsKey(layer.Name)) continue;
                    this.layers.Add(layer.Name, layer);
                    this.layerRefs.Add(layer.Name, new List<DxfObject>());
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private Layer ReadLayer()
        {
            string handle = null;
            string name = null;
            bool isVisible = true;
            bool plot = true;
            AciColor color = AciColor.Default;
            LineType lineType = null;
            Lineweight lineweight = Lineweight.Default;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        break;
                    case 62:
                        short index = short.Parse(dxfPairInfo.Value);
                        if (index < 0)
                        {
                            isVisible = false;
                            index = Math.Abs(index);
                        }
                        if (!color.UseTrueColor) 
                            color = new AciColor(index);
                        break;
                    case 420: // the layer uses true color
                        color = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));
                        break;
                    case 6:
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        break;
                    case 290:
                        if(int.Parse(dxfPairInfo.Value)==0)
                            plot = false;
                        break;
                    case 370:
                        lineweight = Lineweight.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }

                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            Layer layer = new Layer(name)
                {
                    Color = color,
                    LineType = lineType,
                    IsVisible = isVisible,
                    Plot = plot,
                    Lineweight = lineweight,
                    Handle = handle
                };

            this.lineTypeRefs[layer.LineType.Name].Add(layer);
            return layer;
        }

        #endregion

        #region line type methods

        private void ReadLineTypes()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.LineTypeTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    LineType tl = this.ReadLineType();
                    if (tl == null) continue;
                    if (this.lineTypes.ContainsKey(tl.Name)) continue;
                    this.lineTypes.Add(tl.Name, tl);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private LineType ReadLineType()
        {
            string handle = null;
            string name = null;
            string description = null;
            List<double> segments = new List<double>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2: // line type name is case insensitive
                        name = dxfPairInfo.Value;
                        if (String.Compare(name, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            name = "ByLayer";
                        if (String.Compare(name, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            name = "ByBlock";
                        break;
                    case 3: // linetype description
                        description = dxfPairInfo.Value;
                        break;
                    case 73:
                        //number of segments (not needed)
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        break;
                    case 49:
                        segments.Add(double.Parse(dxfPairInfo.Value));
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            this.lineTypeRefs.Add(name, new List<DxfObject>());

            return new LineType(name)
                       {
                           Description = description,
                           Segments = segments,
                           Handle = handle
                       };
        }

        #endregion

        #region text style methods

        private void ReadTextStyles()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.TextStyleTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    TextStyle style = this.ReadTextStyle();
                    if (style == null) continue;
                    if(this.textStyles.ContainsKey(style.Name)) continue;
                    this.textStyles.Add(style.Name, style);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private TextStyle ReadTextStyle()
        {
            string handle = null;
            string name = null;
            string font = null;
            bool isVertical = false;
            bool isBackward = false;
            bool isUpsideDown = false;
            double height = 0.0f;
            double widthFactor = 0.0f;
            double obliqueAngle = 0.0f;

            dxfPairInfo = this.ReadCodePair();

            //leer los datos mientras no encontramos el código 0 que indicaría el final de la capa
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        break;
                    case 3:
                        font = dxfPairInfo.Value;
                        break;

                    case 70:
                        if (int.Parse(dxfPairInfo.Value) == 4)
                            isVertical = true;
                        break;
                    case 71:
                        if (int.Parse(dxfPairInfo.Value) == 6)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        else if (int.Parse(dxfPairInfo.Value) == 2)
                            isBackward = true;
                        else if (int.Parse(dxfPairInfo.Value) == 4)
                            isUpsideDown = true;
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        break;
                    case 41:
                        widthFactor = double.Parse(dxfPairInfo.Value);
                        break;
                    case 42:
                        //last text height used (not aplicable)
                        break;
                    case 50:
                        obliqueAngle = double.Parse(dxfPairInfo.Value);
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;
            if (string.IsNullOrEmpty(font)) font = TextStyle.Default.FontName;

            this.textStyleRefs.Add(name, new List<DxfObject>());

            return new TextStyle(name, font)
                       {
                           Height = height,
                           IsBackward = isBackward,
                           IsUpsideDown = isUpsideDown,
                           IsVertical = isVertical,
                           ObliqueAngle = obliqueAngle,
                           WidthFactor = widthFactor,
                           Handle = handle
                       };
        }

        #endregion

        #region dimension style methods

        private void ReadDimStyles()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.DimensionStyleTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0); //el código 0 indica el inicio de una nueva capa
                    DimensionStyle ds = this.ReadDimensionStyle();
                    if (ds == null) continue;
                    if (this.dimStyles.ContainsKey(ds.Name)) continue;
                    this.dimStyles.Add(ds.Name, ds);
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private DimensionStyle ReadDimensionStyle()
        {
            DimensionStyle defaultDim = DimensionStyle.Default;
            string handle = null;
            string name = null;
            string txtStyleHandle = null;

            // lines
            double dimexo = defaultDim.DIMEXO;
            double dimexe = defaultDim.DIMEXE;

            // symbols and arrows
            double dimasz = defaultDim.DIMASZ;
            double dimcen = defaultDim.DIMCEN;

            // text
            double dimtxt = defaultDim.DIMTXT;
            int dimjust = defaultDim.DIMJUST;
            int dimtad = defaultDim.DIMTAD;
            double dimgap = defaultDim.DIMGAP;
            int dimadec = defaultDim.DIMADEC;
            int dimdec = defaultDim.DIMDEC;
            string dimpost = defaultDim.DIMPOST;
            int dimtih = defaultDim.DIMTIH;
            int dimtoh = defaultDim.DIMTOH;
            int dimaunit = defaultDim.DIMAUNIT;
            string dimdsep = defaultDim.DIMDSEP;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 105:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        break;
                    case 3:
                        dimpost = dxfPairInfo.Value;
                        break;
                    case 41:
                        dimasz = double.Parse(dxfPairInfo.Value);
                        break;
                    case 42:
                        dimexo = double.Parse(dxfPairInfo.Value);
                        break;
                    case 44:
                        dimexe = double.Parse(dxfPairInfo.Value);
                        break;
                    case 73:
                        dimtih = int.Parse(dxfPairInfo.Value);
                        break;
                    case 74:
                        dimtoh = int.Parse(dxfPairInfo.Value);
                        break;
                    case 77:
                        dimtad = int.Parse(dxfPairInfo.Value);
                        break;
                    case 140:
                        dimtxt = double.Parse(dxfPairInfo.Value);
                        break;
                    case 141:
                        dimcen = double.Parse(dxfPairInfo.Value);
                        break;
                    case 147:
                        dimgap = double.Parse(dxfPairInfo.Value);
                        break;
                    case 149:
                        dimadec = int.Parse(dxfPairInfo.Value);
                        break;
                    case 271:
                        dimdec = int.Parse(dxfPairInfo.Value);
                        break;
                    case 275:
                        dimaunit = int.Parse(dxfPairInfo.Value);
                        break;
                    case 278:
                        dimdsep = dxfPairInfo.Value;
                        break;
                    case 280:
                        dimjust = int.Parse(dxfPairInfo.Value);
                        break;
                    case 340:
                        txtStyleHandle = dxfPairInfo.Value;
                        break;
                }
                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(txtStyleHandle)) return null;
            this.dimStyleRefs.Add(name, new List<DxfObject>());

            DimensionStyle dimStyle = new DimensionStyle(name)
                       {
                           Handle = handle,
                           DIMEXO = dimexo,
                           DIMEXE = dimexe,
                           DIMASZ = dimasz,
                           DIMTXT = dimtxt,
                           DIMCEN = dimcen,
                           DIMJUST = dimjust,
                           DIMTAD = dimtad,
                           DIMGAP = dimgap,
                           DIMADEC = dimadec,
                           DIMDEC = dimdec,
                           DIMPOST = dimpost,
                           DIMTIH = dimtih,
                           DIMTOH = dimtoh,
                           DIMAUNIT = dimaunit,
                           DIMDSEP = dimdsep,
                           TextStyle = GetTextStyleByHandle(txtStyleHandle)
                       };

            this.textStyleRefs[dimStyle.TextStyle.Name].Add(dimStyle);
            return dimStyle;
        }

        #endregion

        #region ucs methods

        private void ReadUCSs()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndTable)
            {
                if (dxfPairInfo.Value == StringCode.UcsTable)
                {
                    Debug.Assert(dxfPairInfo.Code == 0);
                    UCS ucs = this.ReadUCS();
                    if (ucs == null) continue;
                    if (this.layers.ContainsKey(ucs.Name)) continue;
                    this.ucss.Add(ucs.Name, ucs);
                    this.ucsRefs.Add(ucs.Name, new List<DxfObject>());
                }
                else
                {
                    dxfPairInfo = this.ReadCodePair();
                }
            }
        }

        private UCS ReadUCS()
        {
            string handle = null;
            string name = null;
            Vector3 origin = Vector3.Zero;
            Vector3 xDir = Vector3.UnitX;
            Vector3 yDir = Vector3.UnitY;
            double elevation = 0.0;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        break;
                    case 10:
                        origin.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 20:
                        origin.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 30:
                        origin.Z = double.Parse(dxfPairInfo.Value);
                        break;
                    case 11:
                        xDir.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 21:
                        xDir.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 31:
                        xDir.Z = double.Parse(dxfPairInfo.Value);
                        break;
                    case 12:
                        yDir.X = double.Parse(dxfPairInfo.Value);
                        break;
                    case 22:
                        yDir.Y = double.Parse(dxfPairInfo.Value);
                        break;
                    case 32:
                        yDir.Z = double.Parse(dxfPairInfo.Value);
                        break;
                    case 146:
                        elevation = double.Parse(dxfPairInfo.Value);
                        break;
                }

                dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            UCS ucs = new UCS(name, origin, xDir, yDir)
            {
                Handle = handle,
                Elevation = elevation
            };

            return ucs;
        }

        #endregion

        #region block methods

        private void ReadBlocks()
        {
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (dxfPairInfo.Value)
                {
                    case StringCode.BeginBlock:
                        Block block = this.ReadBlock();
                        this.blocks.Add(block.Name, block);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // post process the possible nested blocks,
            // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
            foreach (KeyValuePair<Insert, string> pair in this.nestedBlocks)
            {
                Insert insert = pair.Key;
                insert.Block = this.blocks[pair.Value];
                this.blockRefs[insert.Block.Name].Add(insert);
                foreach (Attribute att in insert.Attributes)
                {
                    string attDefId = this.nestedBlocksAttributes[att];
                    // attribute definitions might be null if an INSERT entity attribute has not been defined in the block
                    AttributeDefinition attDef;
                    insert.Block.Attributes.TryGetValue(attDefId, out attDef);
                    att.Definition = attDef;
                }
            }
            foreach (KeyValuePair<Dimension, string> pair in this.nestedDimBlocks)
            {
                Dimension dim = pair.Key;
                dim.Block = this.blocks[pair.Value];
            }
        }

        private Block ReadBlock()
        {
            BlockRecord blockRecord = null;
            Layer layer = null;
            string name = string.Empty;
            string handle = string.Empty;
            BlockTypeFlags type = BlockTypeFlags.None;
            Vector3 basePoint = Vector3.Zero;
            List<EntityObject> entities = new List<EntityObject>();
            Dictionary<string, AttributeDefinition> attdefs = new Dictionary<string, AttributeDefinition>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Value != StringCode.EndBlock)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        if (!this.blockRecords.TryGetValue(name, out blockRecord))
                            throw new ClosedDxfTableException(StringCode.BlockRecordTable, "The block record " + name + " is not defined");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        type = (BlockTypeFlags)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        //I don't know the reason of these duplicity since code 2 also contains the block name
                        //The program EASE exports code 3 with an empty string
                        //name = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 0: // entity
                        EntityObject entity = this.ReadEntity(true);
                        if (entity != null)
                            if (entity.Type == EntityType.AttributeDefinition)
                                attdefs.Add(((AttributeDefinition)entity).Id, (AttributeDefinition)entity);
                            else
                                entities.Add(entity);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // read the end bloc object until a new element is found
            dxfPairInfo = this.ReadCodePair();
            string endBlockHandle = string.Empty;
            Layer endBlockLayer = layer;
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        endBlockHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endBlockLayer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            this.blockRefs.Add(name, new List<DxfObject>());
            Block block = new Block(name, false)
            {
                Record = blockRecord,
                Position = basePoint,
                Layer = layer,
                Entities = entities,
                Attributes = attdefs,
                Handle = handle,
                TypeFlags = type
            };
            block.End.Handle = endBlockHandle;
            block.End.Layer = endBlockLayer;
            this.layerRefs[block.Layer.Name].Add(block);
            return block;
        }

        private AttributeDefinition ReadAttributeDefinition()
        {
            string id = string.Empty;
            string text = string.Empty;
            object value = null;
            AttributeFlags flags = AttributeFlags.Visible;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            TextStyle style = TextStyle.Default;
            double height = 0.0;
            double widthFactor = 0.0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        id = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        text = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        value = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (AttributeFlags)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        widthFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        verticalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 point = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            AttributeDefinition attDef = new AttributeDefinition(id)
            {
                Position = point,
                Normal = normal,
                Alignment = alignment,
                Text = text,
                Value = value,
                Flags = flags,
                Style = style,
                Height = height,
                WidthFactor = MathHelper.IsZero(widthFactor) ? style.WidthFactor : widthFactor,
                Rotation = rotation
            };

            this.textStyleRefs[style.Name].Add(attDef);

            return attDef;
        }

        private Attribute ReadAttribute(Block block, bool isBlockEntity = false)
        {
            string handle = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;

            AttributeFlags flags = AttributeFlags.Visible;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            TextStyle style = TextStyle.Default;
            double height = 0.0;
            double widthFactor = 0.0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;

            // DxfObject codes
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 100)
            {
                switch (dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(DxfObjectCode.Attribute, "Premature end of entity definition.");
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // AcDbEntity common codes
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 100)
            {
                switch (dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(DxfObjectCode.Attribute, "Premature end of entity definition.");
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: // the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 370: //lineweight code
                        lineweight = Lineweight.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 48: //linetype scale
                        linetypeScale = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            string attdefId = null;
            AttributeDefinition attdef = null;
            Object value = null;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        attdefId = dxfPairInfo.Value;
                        // seems that some programs (sketchup AFAIK) might export insert entities with attributtes which definitions are not defined in the block
                        if (!isBlockEntity)
                            block.Attributes.TryGetValue(attdefId, out attdef);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        value = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (AttributeFlags)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        widthFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        verticalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            Attribute att = new Attribute
            {
                Handle = handle,
                Color = color,
                Layer = layer,
                LineType = lineType,
                Lineweight = lineweight,
                LineTypeScale = linetypeScale,
                Definition = attdef,
                Id = attdefId,
                Position = ocsBasePoint,
                Normal = normal,
                Alignment = alignment,
                Value = value,
                Flags = flags,
                Style = style,
                Height = height,
                WidthFactor = MathHelper.IsZero(widthFactor) ? style.WidthFactor : widthFactor,
                Rotation = rotation
            };

            if (isBlockEntity)
                if (attdefId != null) this.nestedBlocksAttributes.Add(att, attdefId);

            this.layerRefs[att.Layer.Name].Add(att);
            this.lineTypeRefs[att.LineType.Name].Add(att);
            this.textStyleRefs[att.Style.Name].Add(att);

            return att;
        }

        #endregion

        #region entity methods

        private EntityObject ReadEntity(bool isBlockEntity)
        {
            string handle = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;
            bool isVisible = true;

            EntityObject entity;

            string dxfCode = dxfPairInfo.Value;
            dxfPairInfo = this.ReadCodePair();

            // DxfObject common codes
            while (dxfPairInfo.Code != 100)
            {
                switch (dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(dxfCode, "Premature end of entity definition.");
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // AcDbEntity common codes
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 100)
            {
                switch (dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(dxfCode, "Premature end of entity definition.");
                    case 8: //layer code
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 370: //lineweight code
                        lineweight = Lineweight.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 48: //linetype scale
                        linetypeScale = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 60: //object visibility
                        if (int.Parse(dxfPairInfo.Value) == 1) isVisible = false;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            switch (dxfCode)
            {
                case DxfObjectCode.Arc:
                    entity = this.ReadArc();
                    break;
                case DxfObjectCode.Circle:
                    entity = this.ReadCircle();
                    break;
                case DxfObjectCode.Dimension:
                    entity = this.ReadDimension(isBlockEntity);
                    break;
                case DxfObjectCode.Ellipse:
                    entity = this.ReadEllipse();
                    break;
                case DxfObjectCode.Face3d:
                    entity = this.ReadFace3d();
                    break;
                case DxfObjectCode.Hatch:
                    entity = this.ReadHatch();
                    break;
                case DxfObjectCode.Insert:
                    entity = this.ReadInsert(isBlockEntity);
                    break;
                case DxfObjectCode.Line:
                    entity = this.ReadLine();
                    break;
                case DxfObjectCode.LightWeightPolyline:
                    entity = this.ReadLwPolyline();
                    break;
                case DxfObjectCode.MText:
                    entity = this.ReadMText();
                    break;
                case DxfObjectCode.Point:
                    entity = this.ReadPoint();
                    break;
                case DxfObjectCode.Polyline:
                    entity = this.ReadPolyline();
                    break;
                case DxfObjectCode.Solid:
                    entity = this.ReadSolid();
                    break;
                case DxfObjectCode.Text:
                    entity = this.ReadText();
                    break;
                case DxfObjectCode.Spline:
                    entity = this.ReadSpline();
                    break;
                case DxfObjectCode.Image:
                    entity = this.ReadImage();
                    break;
                case DxfObjectCode.MLine:
                    entity = this.ReadMLine();
                    break;
                case DxfObjectCode.Ray:
                    entity = this.ReadRay();
                    break;
                case DxfObjectCode.XLine:
                    entity = this.ReadXLine();
                    break;
                case DxfObjectCode.AttributeDefinition:
                    entity = this.ReadAttributeDefinition();
                    break;
                default:
                    ReadUnknowEntity();
                    return null;
            }

            entity.Handle = handle;
            entity.Layer = layer;
            entity.Color = color;
            entity.LineType = lineType;
            entity.Lineweight = lineweight;
            entity.LineTypeScale = linetypeScale;
            entity.IsVisible = isVisible;
            
            this.addedEntity.Add(entity.Handle, entity);
            this.layerRefs[layer.Name].Add(entity);
            this.lineTypeRefs[entity.LineType.Name].Add(entity);
            if (entity.XData != null)
            {
                foreach (string registry in entity.XData.Keys)
                {
                    this.appRegistryRefs[registry].Add(entity);
                }
            }
            return entity;
        }

        private Image ReadImage()
        {
            double posX = 0.0, posY = 0.0, posZ = 0.0;
            double uX = 0.0, uY = 0.0, uZ = 0.0;
            double vX = 0.0, vY = 0.0, vZ = 0.0;
            double width = 0, height = 0;
            string imageDefHandle = null;
            ImageDisplayFlags displayOptions = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen | ImageDisplayFlags.UseClippingBoundary;
            bool clipping = false;
            float brightness = 50.0f;
            float contrast = 50.0f;
            float fade = 0.0f;
            ImageClippingBoundaryType boundaryType = ImageClippingBoundaryType.Rectangular;
            ImageClippingBoundary clippingBoundary = null;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        posX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        posY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        posZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        uX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        uY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        uZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        vX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        vY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        vZ = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        width = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        imageDefHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        displayOptions = (ImageDisplayFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        clipping = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        brightness = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 282:
                        contrast = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 283:
                        fade = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        boundaryType = (ImageClippingBoundaryType) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        int numVertexes = int.Parse(dxfPairInfo.Value);
                        List<Vector2> vertexes = new List<Vector2>();
                        dxfPairInfo = this.ReadCodePair();
                        for (int i = 0; i < numVertexes; i++)
                        {
                            double x = 0.0;
                            double y = 0.0;
                            if (dxfPairInfo.Code == 14) x = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            if (dxfPairInfo.Code == 24) y = double.Parse(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            vertexes.Add(new Vector2(x, y));
                        }
                        if(boundaryType == ImageClippingBoundaryType.Rectangular)
                            clippingBoundary = new ImageClippingBoundary(vertexes[0], vertexes[1]);
                        else if (boundaryType == ImageClippingBoundaryType.Polygonal)
                            clippingBoundary = new ImageClippingBoundary(vertexes);
                        else
                            clippingBoundary = null;
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 u = new Vector3(uX, uY, uZ);
            Vector3 v = new Vector3(vX, vY, vZ);
            Vector3 normal = Vector3.CrossProduct(u, v);
            Vector3 uOCS = MathHelper.Transform(u, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(uOCS.X, uOCS.Y)) * MathHelper.RadToDeg;
            double uLength = u.Modulus();
            double vLength = v.Modulus();

            Image image = new Image
                              {
                                  Width = width * uLength,
                                  Height = height * vLength,
                                  Position = new Vector3(posX, posY, posZ),
                                  Normal = normal,
                                  Rotation = rotation,
                                  DisplayOptions = displayOptions,
                                  Clipping = clipping,
                                  Brightness = brightness,
                                  Contrast = contrast,
                                  Fade = fade,
                                  ClippingBoundary = clippingBoundary,
                                  XData = xData
                              };

            this.imgToImgDefHandles.Add(image, imageDefHandle);

            return image;
        }

        private Arc ReadArc()
        {
            Vector3 center = Vector3.Zero;
            double radius = 1.0;
            double startAngle = 0.0;
            double endAngle = 180.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        radius = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        startAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        endAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 wcsCenter = MathHelper.Transform(center, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            return new Arc
                {
                    Center = wcsCenter,
                    Radius = radius,
                    StartAngle = startAngle,
                    EndAngle = endAngle,
                    Thickness = thickness,
                    Normal = normal,
                    XData = xData
                };
        }

        private Circle ReadCircle()
        {
            Vector3 center = Vector3.Zero;
            double radius = 1.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        radius = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 wcsCenter = MathHelper.Transform(center, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            return new Circle
                {
                    Center = wcsCenter,
                    Radius = radius,
                    Thickness = thickness,
                    Normal = normal,
                    XData = xData
                };
        }

        private Dimension ReadDimension(bool isBlockEntity = false)
        {
            string drawingBlockName = null;
            Block drawingBlock = null;
            Vector3 defPoint = Vector3.Zero;
            Vector3 midtxtPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            DimensionTypeFlag dimType = DimensionTypeFlag.Linear;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.BottomCenter;
            MTextLineSpacingStyle lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            DimensionStyle style = DimensionStyle.Default;
            double dimRot = 0.0;
            double lineSpacingFactor = 1.0;
            bool dimInfo = false;

            dxfPairInfo = this.ReadCodePair();
            while (!dimInfo)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        drawingBlockName = dxfPairInfo.Value;
                        if(!isBlockEntity) drawingBlock = this.GetBlock(drawingBlockName);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        style = this.GetDimensionStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        defPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        defPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        defPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        midtxtPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        midtxtPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        midtxtPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        dimType = (DimensionTypeFlag)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        lineSpacingStyle = (MTextLineSpacingStyle) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        lineSpacingFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly is this value is not provided
                        dimRot = 360 - double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 100:
                        if (dxfPairInfo.Value == SubclassMarker.AlignedDimension ||
                            dxfPairInfo.Value == SubclassMarker.RadialDimension ||
                            dxfPairInfo.Value == SubclassMarker.DiametricDimension ||
                            dxfPairInfo.Value == SubclassMarker.Angular3PointDimension ||
                            dxfPairInfo.Value == SubclassMarker.Angular2LineDimension ||
                            dxfPairInfo.Value == SubclassMarker.OrdinateDimension)
                            dimInfo = true; // we have finished reading the basic dimension info
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Dimension dim;
            OrdinateDimensionAxis axis = OrdinateDimensionAxis.X;
            // this is the result of the way the dxf use the DimensionTypeFlag enum, it is a mixture of a regular enum with flags
            DimensionTypeFlag type = dimType;
            if ((type & DimensionTypeFlag.BlockReference) == DimensionTypeFlag.BlockReference)
                type -= DimensionTypeFlag.BlockReference;
            if ((type & DimensionTypeFlag.OrdinteType) == DimensionTypeFlag.OrdinteType)
            {
                axis = OrdinateDimensionAxis.X;
                type -= DimensionTypeFlag.OrdinteType;
            }
            if ((type & DimensionTypeFlag.UserTextPosition) == DimensionTypeFlag.UserTextPosition)
                type -= DimensionTypeFlag.UserTextPosition;

            switch (type)
            {
                case (DimensionTypeFlag.Aligned):
                    dim = ReadAlignedDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Linear):
                    dim = ReadLinearDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Radius):
                    dim = ReadRadialDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Diameter):
                    dim = ReadDiametricDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Angular3Point):
                    dim = ReadAngular3PointDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Angular):
                    dim = ReadAngular2LineDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Ordinate):
                    dim = ReadOrdinateDimension(defPoint, axis, normal, dimRot);
                    break;
                default:
                    throw new ArgumentException("The dimension type: " + type + " is not implemented or unknown.");
            }

            if (dim == null) return null;
            if (drawingBlockName == null) return null;

            dim.Style = style;
            dim.Block = drawingBlock;
            dim.DefinitionPoint = defPoint;
            dim.MidTextPoint = midtxtPoint;
            dim.AttachmentPoint = attachmentPoint;
            dim.LineSpacingStyle = lineSpacingStyle;
            dim.LineSpacingFactor = lineSpacingFactor;
            dim.Normal = normal;

            if (isBlockEntity)
                this.nestedDimBlocks.Add(dim, drawingBlockName);
            else
                this.blockRefs[drawingBlockName].Add(dim);

            this.dimStyleRefs[style.Name].Add(dim);

            return dim;
        }

        private AlignedDimension ReadAlignedDimension(Vector3 defPoint)
        {
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            double offset = Vector3.Distance(secondRef, defPoint);
            return new AlignedDimension(firstRef, secondRef, offset) {XData = xData};
        }

        private LinearDimension ReadLinearDimension(Vector3 defPoint)
        {
            Vector3 firtRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            double rot = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firtRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firtRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firtRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rot = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 52:
                        // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the dxf documentation
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 midPoint = Vector3.MidPoint(firtRef, secondRef);
            Vector3 origin = defPoint;
            Vector3 dir = new Vector3(Math.Cos(rot*MathHelper.DegToRad), Math.Sin(rot*MathHelper.DegToRad), 0.0);
            dir.Normalize();
            double offset = MathHelper.PointLineDistance(midPoint, origin, dir);

            return new LinearDimension(firtRef, secondRef, offset, rot) {XData = xData};
        }

        private RadialDimension ReadRadialDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double radius = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Vector2.Angle(firstPoint, seconPoint) * MathHelper.RadToDeg;
            return new RadialDimension(defPoint, radius, rotation ) { XData = xData };
        }

        private DiametricDimension ReadDiametricDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double diameter = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Vector2.Angle(firstPoint, seconPoint) * MathHelper.RadToDeg;
            return new DiametricDimension(Vector3.MidPoint(defPoint, circunferenceRef), diameter, rotation) { XData = xData };
        }

        private Angular3PointDimension ReadAngular3PointDimension(Vector3 defPoint)
        {
            Vector3 center = Vector3.Zero;
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Angular3PointDimension(center, firstRef, secondRef, Vector3.Distance(center, defPoint));

        }

        private Angular2LineDimension ReadAngular2LineDimension(Vector3 defPoint)
        {      
            Vector3 startFirstLine = Vector3.Zero;
            Vector3 endFirstLine = Vector3.Zero;
            Vector3 startSecondLine = Vector3.Zero;
            Vector3 arcDefinitionPoint = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        startFirstLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        startFirstLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        startFirstLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        endFirstLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        endFirstLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        endFirstLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        startSecondLine.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        startSecondLine.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        startSecondLine.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 16:
                        arcDefinitionPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 26:
                        arcDefinitionPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 36:
                        arcDefinitionPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return new Angular2LineDimension(startFirstLine, endFirstLine, startSecondLine, defPoint, Vector3.Distance(startSecondLine, defPoint)) { ArcDefinitionPoint = arcDefinitionPoint };
        }

        private OrdinateDimension ReadOrdinateDimension(Vector3 defPoint, OrdinateDimensionAxis axis, Vector3 normal, double rotation)
        {
            Vector3 firstPoint = Vector3.Zero;
            Vector3 secondPoint = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 13:
                        firstPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            Vector3 localPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(firstPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            localPoint = MathHelper.Transform(secondPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 secondRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            double length = axis == OrdinateDimensionAxis.X ? secondRef.Y - firstRef.Y : secondRef.X - firstRef.X;

            return new OrdinateDimension(defPoint, firstRef, length, rotation, axis);
        }

        private Ellipse ReadEllipse()
        {
            Vector3 center = Vector3.Zero;
            Vector3 axisPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double[] param = new double[2];
            double ratio = 0.0;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        axisPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        axisPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        axisPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        ratio = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        param[0] = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        param[1] = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint,
                                                        normal,
                                                        MathHelper.CoordinateSystem.World,
                                                        MathHelper.CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y));

            double majorAxis = 2*axisPoint.Modulus();
            double minorAxis = majorAxis*ratio;
            Ellipse ellipse = new Ellipse
                {
                    MajorAxis = majorAxis,
                    MinorAxis = minorAxis,
                    Rotation = rotation*MathHelper.RadToDeg,
                    Center = center,
                    Normal = normal,
                    XData = xData
                };

            ellipse.SetParameters(param);
            return ellipse;
        }

        private Point ReadPoint()
        {
            Vector3 location = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            double rotation = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        location.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = 360.0 - double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Point
                {
                    Location = location,
                    Thickness = thickness,
                    Rotation = rotation,
                    Normal = normal,
                    XData = xData
                };
        }

        private Face3d ReadFace3d()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            EdgeFlags flags = EdgeFlags.Visibles;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        v0.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (EdgeFlags) (int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return new Face3d
                {
                    FirstVertex = v0,
                    SecondVertex = v1,
                    ThirdVertex = v2,
                    FourthVertex = v3,
                    EdgeFlags = flags,
                    XData = xData
                };
        }

        private Solid ReadSolid()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        v0.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Solid
                {
                    FirstVertex = v0,
                    SecondVertex = v1,
                    ThirdVertex = v2,
                    FourthVertex = v3,
                    Thickness = thickness,
                    Normal = normal,
                    XData = xData
                };
        }

        private Spline ReadSpline()
        {
            SplineTypeFlags flags = SplineTypeFlags.None;
            Vector3 normal = Vector3.UnitZ;
            short degree = 3;
            int numKnots = 0;
            int numCtrlPoints = 0;
            int ctrlPointIndex = -1;
            
            double[] knots = null;
            List<SplineVertex> ctrlPoints = new List<SplineVertex>();
            double ctrlX = 0;
            double ctrlY = 0;
            double ctrlZ = 0;
            double ctrlWeigth = -1;

            // tolerances (not used)
            double knotTolerance = 0.0000001;
            double ctrlPointTolerance = 0.0000001;
            double fitTolerance = 0.0000000001;

            // start and end tangents (not used)
            double stX = 0;
            double stY = 0;
            double stZ = 0;
            Vector3? startTangent = null;
            double etX = 0;
            double etY = 0;
            double etZ = 0;
            Vector3? endTangent = null;

            // fit points variable (not used)
            int numFitPoints = 0;
            List<Vector3> fitPoints = new List<Vector3>();
            double fitX = 0;
            double fitY = 0;
            double fitZ = 0;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (SplineTypeFlags)(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        degree = short.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        numKnots = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        numCtrlPoints = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        numFitPoints = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        knotTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        ctrlPointTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        fitTolerance = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        stX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        stY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        stZ = double.Parse(dxfPairInfo.Value);
                        startTangent = new Vector3(stX, stY, stZ);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        etX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        etY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        etZ = double.Parse(dxfPairInfo.Value);
                        endTangent = new Vector3(stX, stY, stZ);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        // multiple code 40 entries, one per knot value
                        knots = ReadSplineKnots(numKnots);
                        break;
                    case 10:
                        ctrlX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        ctrlY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        ctrlZ = double.Parse(dxfPairInfo.Value);
                        
                        if (ctrlWeigth.Equals(-1))
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ));
                            ctrlPointIndex = ctrlPoints.Count - 1;
                        }
                        else
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ, ctrlWeigth));
                            ctrlPointIndex = -1;
                        }
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        // code 41 might appear before or after the control point coordiantes.
                        // I am open to better ways to handling this.
                        if (ctrlPointIndex == -1)
                        {
                            ctrlWeigth = double.Parse(dxfPairInfo.Value);
                        }
                        else
                        {
                            ctrlPoints[ctrlPointIndex].Weigth = double.Parse(dxfPairInfo.Value);
                            ctrlWeigth = -1;
                        }
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        fitX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        fitY = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        fitZ = double.Parse(dxfPairInfo.Value);
                        fitPoints.Add(new Vector3(fitX, fitY, fitZ));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Spline(ctrlPoints, knots, degree);

        }

        private double[] ReadSplineKnots(int numKnots)
        {
            double[] knots = new double[numKnots];
            for (int i = 0; i < numKnots; i++)
            {
                if (dxfPairInfo.Code != 40)
                    throw new DxfException("The knot vector must have " + numKnots + " code 40 entries.");
                knots[i] = double.Parse(dxfPairInfo.Value);
                dxfPairInfo = this.ReadCodePair();
            }
            return knots;
        }

        private Insert ReadInsert(bool isBlockEntity = false)
        {
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 scale = new Vector3(1.0, 1.0, 1.0);
            double rotation = 0.0;
            string blockName = null;
            Block block = null;
            List<Attribute> attributes = new List<Attribute>();
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        blockName = dxfPairInfo.Value;
                        if(!isBlockEntity) block = this.GetBlock(blockName);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        scale.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        scale.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // if there are attributes
            string endSequenceHandle = string.Empty;
            Layer endSequenceLayer = Layer.Default;
            if (dxfPairInfo.Value == DxfObjectCode.Attribute)
            {
                while (dxfPairInfo.Value != StringCode.EndSequence)
                {
                    if (dxfPairInfo.Value != DxfObjectCode.Attribute) continue;

                    // read the attribute
                    Attribute attribute = this.ReadAttribute(block, isBlockEntity);
                    if(attribute !=null)
                        attributes.Add(attribute);
                }

                // read the end end sequence object until a new element is found
                dxfPairInfo = this.ReadCodePair();
                while (dxfPairInfo.Code != 0)
                {
                    switch (dxfPairInfo.Code)
                    {
                        case 5:
                            endSequenceHandle = dxfPairInfo.Value;
                            dxfPairInfo = this.ReadCodePair();
                            break;
                        case 8:
                            endSequenceLayer = this.GetLayer(dxfPairInfo.Value);
                            dxfPairInfo = this.ReadCodePair();
                            break;
                        default:
                            dxfPairInfo = this.ReadCodePair();
                            break;
                    }
                }
            }

            // It is a lot more intuitive to give the position in world coordinates and then define the orientation with the normal.
            Vector3 wcsBasePoint = MathHelper.Transform(basePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            Insert insert = new Insert
                                {
                                    Block = block,
                                    Position = wcsBasePoint,
                                    Rotation = rotation,
                                    Scale = scale,
                                    Normal = normal,
                                };

            insert.EndSequence.Handle = endSequenceHandle;
            insert.EndSequence.Layer = endSequenceLayer;
            insert.Attributes.Clear();
            insert.Attributes.AddRange(attributes);
            insert.XData = xData;

            if (blockName == null)
                throw new NullReferenceException("The insert block name cannot be null.");
            if (isBlockEntity)
                this.nestedBlocks.Add(insert, blockName);
            else
                this.blockRefs[blockName].Add(insert);

            return insert;
        }

        private Line ReadLine()
        {
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        start.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        start.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        start.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        end.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        end.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        end.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return new Line
                {
                    StartPoint = start,
                    EndPoint = end,
                    Normal = normal,
                    Thickness = thickness,
                    XData = xData
                };

        }

        private Ray ReadRay()
        {
            Vector3 origin = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        origin.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        origin.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        origin.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        direction.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Ray
                {
                    Origin = origin, 
                    Direction = direction, 
                    XData = xData
                };
        }

        private XLine ReadXLine()
        {
            Vector3 origin = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 10:
                        origin.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        origin.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        origin.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        direction.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new XLine
            {
                Origin = origin,
                Direction = direction,
                XData = xData
            };
        }

        private MLine ReadMLine()
        {
            string styleName = null;
            double scale = 1.0;
            MLineJustification justification = MLineJustification.Zero;
            MLineFlags flags = MLineFlags.Has;
            int numVertexes = 0;
            int numStyleElements = 0;
            double elevation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<MLineVertex> segments = new List<MLineVertex>();
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        // the MLineStyle is defined in the objects sections after the definition of the entity, something similar happens with the image entity
                        // the MLineStyle will be applied to the MLine after parsing the whole file
                        styleName = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        scale = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        justification = (MLineJustification) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        flags = (MLineFlags)int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        numVertexes = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        numStyleElements = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        // this info is not needed it is repeated in the vertexes list
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        // this info is not needed it is repeated in the vertexes list
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        // this info is not needed it is repeated in the vertexes list
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        // the info that follows contains the information on the vertexes of the MLine
                        segments = ReadMLineSegments(numVertexes, numStyleElements, normal, out elevation);
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            bool isClosed = false;
            bool noStartCaps = false;
            bool noEndCaps = false;
            if ((flags & MLineFlags.Closed) == MLineFlags.Closed) isClosed = true;
            if ((flags & MLineFlags.NoStartCaps) == MLineFlags.NoStartCaps) noStartCaps = true;
            if ((flags & MLineFlags.NoEndCaps) == MLineFlags.NoEndCaps) noEndCaps = true;
            MLine mline = new MLine
                              {
                                  IsClosed = isClosed,
                                  NoStartCaps = noStartCaps,
                                  NoEndCaps = noEndCaps,
                                  Elevation = elevation,
                                  Scale = scale,
                                  Justification = justification,
                                  Normal = normal,
                                  Vertexes = segments,
                                  XData = xData
                              };

            // save the referenced style name for postprocessing
            this.mLineToStyleNames.Add(mline, styleName);
            return mline;
        }

        private List<MLineVertex> ReadMLineSegments(int numVertexes, int numStyleElements, Vector3 normal, out double elevation)
        {
            elevation = 0.0;

            List<MLineVertex> segments = new List<MLineVertex>();
            Matrix3 trans = MathHelper.ArbitraryAxis(normal).Traspose();
            for (int i = 0; i < numVertexes; i++)
            {
                Vector3 vertex = new Vector3();
                vertex.X = double.Parse(dxfPairInfo.Value); // code 11
                dxfPairInfo = this.ReadCodePair();
                vertex.Y = double.Parse(dxfPairInfo.Value); // code 21
                dxfPairInfo = this.ReadCodePair();
                vertex.Z = double.Parse(dxfPairInfo.Value); // code 31
                dxfPairInfo = this.ReadCodePair();

                Vector3 dir = new Vector3();
                dir.X = double.Parse(dxfPairInfo.Value); // code 12
                dxfPairInfo = this.ReadCodePair();
                dir.Y = double.Parse(dxfPairInfo.Value); // code 22
                dxfPairInfo = this.ReadCodePair();
                dir.Z = double.Parse(dxfPairInfo.Value); // code 32
                dxfPairInfo = this.ReadCodePair();
                
                Vector3 mitter = new Vector3();
                mitter.X = double.Parse(dxfPairInfo.Value); // code 13
                dxfPairInfo = this.ReadCodePair();
                mitter.Y = double.Parse(dxfPairInfo.Value); // code 23
                dxfPairInfo = this.ReadCodePair();
                mitter.Z = double.Parse(dxfPairInfo.Value); // code 33
                dxfPairInfo = this.ReadCodePair();

                List<double>[] distances = new List<double>[numStyleElements];
                for (int j = 0; j < numStyleElements; j++)
                {
                    distances[j] = new List<double>();
                    int numDistances = int.Parse(dxfPairInfo.Value); // code 74
                    dxfPairInfo = this.ReadCodePair();
                    for (int k = 0; k < numDistances; k++)
                    {
                        distances[j].Add(double.Parse(dxfPairInfo.Value)); // code 41
                        dxfPairInfo = this.ReadCodePair();
                    }

                    // no more info is needed, fill params are not supported
                    int numFillParams = int.Parse(dxfPairInfo.Value); // code 75
                    dxfPairInfo = this.ReadCodePair();
                    for (int k = 0; k < numFillParams; k++)
                    {
                        double param = double.Parse(dxfPairInfo.Value); // code 42
                        dxfPairInfo = this.ReadCodePair();
                    }
                }

                // we need to convert wcs coordinates to ocs coordinates
                if (!normal.Equals(Vector3.UnitZ))
                {
                    vertex =  trans * vertex;
                    dir = trans * dir;
                    mitter = trans * mitter;
                }

                MLineVertex segment = new MLineVertex(new Vector2(vertex.X, vertex.Y),
                                                        new Vector2(dir.X, dir.Y),
                                                        new Vector2(mitter.X, mitter.Y),
                                                        distances);
                elevation = vertex.Z;
                segments.Add(segment);
            }

            return segments;
        }

        private LwPolyline ReadLwPolyline()
        {
            LwPolyline pol = new LwPolyline();
            double constantWidth = 0.0;
            LwPolylineVertex v = new LwPolylineVertex();
            double vX = 0.0;
            Vector3 normal = Vector3.UnitZ;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 38:
                        pol.Elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        pol.Thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        // constant width (optional; default = 0). Not used if variable width (codes 40 and/or 41) is set
                        constantWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        PolylineTypeFlags flags = (PolylineTypeFlags) int.Parse(dxfPairInfo.Value);
                        pol.IsClosed = (flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        v = new LwPolylineVertex
                                {
                                    BeginWidth = constantWidth,
                                    EndWidth = constantWidth
                                };
                        vX = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        double vY = double.Parse(dxfPairInfo.Value);
                        v.Location = new Vector2(vX, vY);
                        pol.Vertexes.Add(v);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        v.BeginWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        v.EndWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        v.Bulge = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            pol.Normal = normal;
            pol.XData = xData;

            return pol;
        }

        private EntityObject ReadPolyline()
        {
            // the entity Polyline in dxf can actually hold three kinds of entities
            // polyline 3d is the generic polyline
            // polyface mesh
            // polylines 2d is the old way of writing polylines the AutoCAD2000 and newer always use LightweightPolylines to define a polyline 2d
            // this way of reading polylines 2d is here for compatibility reasons with older dxf versions.
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            double elevation = 0.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<Vertex> vertexes = new List<Vertex>();
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 30:
                        elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) (int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numVertexes = int.Parse(code.Value); code = this.ReadCodePair();
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numFaces  = int.Parse(code.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            //begin to read the vertex list (althought it is not recommended the vertex list might have 0 entries)
            while (dxfPairInfo.Value != StringCode.EndSequence)
            {
                if (dxfPairInfo.Value == DxfObjectCode.Vertex)
                {
                    Vertex vertex = this.ReadVertex();
                    vertexes.Add(vertex);
                }
            }

            // read the end sequence object until a new element is found
            dxfPairInfo = this.ReadCodePair();
            string endSequenceHandle = null;
            Layer endSequenceLayer = Layer.Default;
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        endSequenceHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endSequenceLayer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            EntityObject pol;
            bool isClosed = (flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM;

            //to avoid possible error between the vertex type and the polyline type
            //the polyline type will decide which information to use from the read vertex
            if ((flags & PolylineTypeFlags.Polyline3D) == PolylineTypeFlags.Polyline3D)
            {
                List<PolylineVertex> polyline3dVertexes = new List<PolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    PolylineVertex vertex = new PolylineVertex
                                                {
                                                    Location = v.Location,
                                                    Handle = v.Handle,
                                                };
                    polyline3dVertexes.Add(vertex);
                }

                pol = new Polyline(polyline3dVertexes, isClosed)
                          {
                              Normal = normal
                          };
                ((Polyline) pol).EndSequence.Handle = endSequenceHandle;
                ((Polyline) pol).EndSequence.Layer = endSequenceLayer;
            }
            else if ((flags & PolylineTypeFlags.PolyfaceMesh) == PolylineTypeFlags.PolyfaceMesh)
            {
                //the vertex list created contains vertex and face information
                List<PolyfaceMeshVertex> polyfaceVertexes = new List<PolyfaceMeshVertex>();
                List<PolyfaceMeshFace> polyfaceFaces = new List<PolyfaceMeshFace>();
                foreach (Vertex v in vertexes)
                {
                    if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh)) == (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh))
                    {
                        PolyfaceMeshVertex vertex = new PolyfaceMeshVertex
                                                        {
                                                            Location = v.Location,
                                                            Handle = v.Handle,
                                                        };
                        polyfaceVertexes.Add(vertex);
                    }
                    else if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex)) == (VertexTypeFlags.PolyfaceMeshVertex))
                    {
                        PolyfaceMeshFace vertex = new PolyfaceMeshFace
                                                      {
                                                          VertexIndexes = v.VertexIndexes,
                                                          Handle = v.Handle,
                                                      };
                        polyfaceFaces.Add(vertex);
                    }
                }
                pol = new PolyfaceMesh(polyfaceVertexes, polyfaceFaces)
                          {
                              Normal = normal
                          };
                ((PolyfaceMesh) pol).EndSequence.Handle = endSequenceHandle;
                ((PolyfaceMesh) pol).EndSequence.Layer = endSequenceLayer;
            }
            else
            {
                List<LwPolylineVertex> polylineVertexes = new List<LwPolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    LwPolylineVertex vertex = new LwPolylineVertex
                                                  {
                                                      Location = new Vector2(v.Location.X, v.Location.Y),
                                                      BeginWidth = v.BeginThickness,
                                                      Bulge = v.Bulge,
                                                      EndWidth = v.EndThickness,
                                                  };

                    polylineVertexes.Add(vertex);
                }

                pol = new LwPolyline(polylineVertexes, isClosed)
                          {
                              Thickness = thickness,
                              Elevation = elevation,
                              Normal = normal,
                          };
            }

            pol.XData = xData;

            return pol;
        }

        private Text ReadText()
        {
            Text text = new Text();
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 1:
                        text.Value = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        text.Height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        text.WidthFactor = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        text.Rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        text.ObliqueAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        text.Style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        verticalAlignment = int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            // another example of this ocs vs wcs non sense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            text.Position = MathHelper.Transform(ocsBasePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            text.Normal = normal;
            text.Alignment = alignment;
            text.XData = xData;

            this.textStyleRefs[text.Style.Name].Add(text);

            return text;
        }

        private MText ReadMText()
        {
            Vector3 insertionPoint = Vector3.Zero;
            Vector2 direction = Vector2.UnitX;
            Vector3 normal = Vector3.UnitZ;
            double height = 0.0;
            double rectangleWidth = 0.0;
            double lineSpacing = 1.0;
            double rotation = 0.0;
            bool isRotationDefined = false;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.TopLeft;
            TextStyle style = TextStyle.Default;
            string text = string.Empty;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 1:
                        text += dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        text += dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        insertionPoint.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        insertionPoint.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        insertionPoint.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        // Z direction value (direction.Z = double.Parse(dxfPairInfo.Value);)
                        // we will alway defined the angle of the text on the plane where it is defined so Z value will be zero.
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        rectangleWidth = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        lineSpacing = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        isRotationDefined = true;
                        rotation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            MText mText = new MText(text, insertionPoint, height, rectangleWidth, style)
                              {
                                  LineSpacingFactor = lineSpacing,
                                  AttachmentPoint = attachmentPoint,
                                  Rotation = isRotationDefined
                                                 ? rotation
                                                 : Vector2.Angle(direction) * MathHelper.RadToDeg,
                                  Normal = normal,
                                  XData = xData
                              };

            this.textStyleRefs[style.Name].Add(mText);
            return mText;
        }

        private Hatch ReadHatch()
        {
            string name = string.Empty;
            FillType fill = FillType.SolidFill;
            double elevation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            HatchPattern pattern = HatchPattern.Line;
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 2:
                        name = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        elevation = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        // the next lines hold the information about the hatch boundary paths
                        int numPaths = int.Parse(dxfPairInfo.Value);
                        paths = ReadHatchBoundaryPaths(numPaths);
                        break;
                    case 70:
                        // Solid fill flag
                        fill = (FillType) int.Parse(dxfPairInfo.Value);
                        if (fill == FillType.SolidFill) name = PredefinedHatchPatternName.Solid;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        // Associativity flag (associative = 1; non-associative = 0); for MPolygon, solid-fill flag (has solid fill = 1; lacks solid fill = 0)
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        // the next lines hold the information about the hatch pattern
                        pattern = ReadHatchPattern(name);
                        pattern.Fill = fill;
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(dxfPairInfo.Code, dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Hatch hatch = new Hatch(pattern, paths)
                              {
                                  Elevation = elevation,
                                  Normal = normal,
                                  XData = xData
                              };

            return hatch;
        }

        private List<HatchBoundaryPath> ReadHatchBoundaryPaths(int numPaths)
        {
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();

            dxfPairInfo = this.ReadCodePair();

            while (paths.Count < numPaths)
            {
                switch (dxfPairInfo.Code)
                {
                    case 92:
                        BoundaryPathTypeFlag pathTypeFlag = (BoundaryPathTypeFlag) int.Parse(dxfPairInfo.Value);
                        if ((pathTypeFlag & BoundaryPathTypeFlag.Polyline) == BoundaryPathTypeFlag.Polyline)
                            paths.Add(ReadPolylineBoundaryPath());
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 93:
                        int numEdges = int.Parse(dxfPairInfo.Value);
                        paths.Add(ReadEdgeBoundaryPath(numEdges));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return paths;
        }

        private HatchBoundaryPath ReadPolylineBoundaryPath()
        {
            List<LwPolylineVertex> vertexes = new List<LwPolylineVertex>();
            dxfPairInfo = this.ReadCodePair();

            bool hasBulge = int.Parse(dxfPairInfo.Value) != 0;
            dxfPairInfo = this.ReadCodePair();

            // is polyline closed
            bool isClosed = int.Parse(dxfPairInfo.Value) == 1;
            dxfPairInfo = this.ReadCodePair();

            int numVertexes = int.Parse(dxfPairInfo.Value); // code 93
            dxfPairInfo = this.ReadCodePair();

            for (int i = 0; i < numVertexes; i++)
            {
                double bulge = 0.0;
                double x = double.Parse(dxfPairInfo.Value); // code 10
                dxfPairInfo = this.ReadCodePair();
                double y = double.Parse(dxfPairInfo.Value); // code 20
                dxfPairInfo = this.ReadCodePair();
                if (hasBulge)
                {
                    bulge = double.Parse(dxfPairInfo.Value);  // code 42
                    dxfPairInfo = this.ReadCodePair();
                }
                vertexes.Add(new LwPolylineVertex(x, y, bulge));
            }

            LwPolyline polyline = new LwPolyline(vertexes, isClosed);
            List<EntityObject> entities = isClosed ? new List<EntityObject> {polyline} : polyline.Explode();
            return new HatchBoundaryPath(entities);
        }

        private HatchBoundaryPath ReadEdgeBoundaryPath(int numEdges)
        {
            // the information of the boundary path data always appear exactly as it is readed
            List<EntityObject> entities = new List<EntityObject>();
            dxfPairInfo = this.ReadCodePair();

            while (entities.Count < numEdges)
            {
                // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline
                switch (int.Parse(dxfPairInfo.Value))
                {
                    case 1:
                        dxfPairInfo = this.ReadCodePair();
                        // line
                        double lX1 = double.Parse(dxfPairInfo.Value); // code 10
                        dxfPairInfo = this.ReadCodePair();
                        double lY1 = double.Parse(dxfPairInfo.Value); // code 20
                        dxfPairInfo = this.ReadCodePair();
                        double lX2 = double.Parse(dxfPairInfo.Value); // code 11
                        dxfPairInfo = this.ReadCodePair();
                        double lY2 = double.Parse(dxfPairInfo.Value); // code 21
                        dxfPairInfo = this.ReadCodePair();
                        entities.Add(new Line(new Vector3(lX1, lY1, 0.0), new Vector3(lX2, lY2, 0.0)));
                        break;
                    case 2:
                        dxfPairInfo = this.ReadCodePair();
                        // circular arc
                        double aX = double.Parse(dxfPairInfo.Value); // code 10
                        dxfPairInfo = this.ReadCodePair();
                        double aY = double.Parse(dxfPairInfo.Value); // code 40
                        dxfPairInfo = this.ReadCodePair();
                        double aR = double.Parse(dxfPairInfo.Value); // code 40
                        dxfPairInfo = this.ReadCodePair();
                        double aStart = double.Parse(dxfPairInfo.Value); // code 50
                        dxfPairInfo = this.ReadCodePair();
                        double aEnd = double.Parse(dxfPairInfo.Value); // code 51
                        dxfPairInfo = this.ReadCodePair();
                        bool aCCW = int.Parse(dxfPairInfo.Value) != 0; // code 73
                        dxfPairInfo = this.ReadCodePair();
                        // a full circle will never happen, AutoCAD exports circle boundary paths as two vertex polylines with bulges of 1 and -1
                        entities.Add(aCCW
                                         ? new Arc(new Vector3(aX, aY, 0.0), aR, aStart, aEnd)
                                         : new Arc(new Vector3(aX, aY, 0.0), aR, 360 - aEnd, 360 - aStart));
                        break;
                    case 3:
                        dxfPairInfo = this.ReadCodePair();
                        // elliptic arc
                        double eX = double.Parse(dxfPairInfo.Value); // code 10
                        dxfPairInfo = this.ReadCodePair();
                        double eY = double.Parse(dxfPairInfo.Value); // code 20
                        dxfPairInfo = this.ReadCodePair();
                        double eAxisX = double.Parse(dxfPairInfo.Value); // code 11
                        dxfPairInfo = this.ReadCodePair();
                        double eAxisY = double.Parse(dxfPairInfo.Value); // code 21
                        dxfPairInfo = this.ReadCodePair();
                        double eAxisRatio = double.Parse(dxfPairInfo.Value); // code 40
                        dxfPairInfo = this.ReadCodePair();
                        double eStart = double.Parse(dxfPairInfo.Value); // code 50
                        dxfPairInfo = this.ReadCodePair();
                        double eEnd = double.Parse(dxfPairInfo.Value); // code 51
                        dxfPairInfo = this.ReadCodePair();
                        bool eCCW = int.Parse(dxfPairInfo.Value) != 0; // code 73
                        dxfPairInfo = this.ReadCodePair();

                        Vector3 center = new Vector3(eX, eY, 0.0);
                        Vector3 axisPoint = new Vector3(eAxisX, eAxisY, 0.0);
                        Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint,
                                                                    Vector3.UnitZ,
                                                                    MathHelper.CoordinateSystem.World,
                                                                    MathHelper.CoordinateSystem.Object);
                        double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y))*MathHelper.RadToDeg;
                        double majorAxis = 2*axisPoint.Modulus();
                        Ellipse ellipse = new Ellipse
                                              {
                                                  MajorAxis = majorAxis,
                                                  MinorAxis = majorAxis*eAxisRatio,
                                                  Rotation = rotation,
                                                  Center = center,
                                                  StartAngle = eCCW ? eStart : 360 - eEnd,
                                                  EndAngle = eCCW ? eEnd : 360 - eStart,
                                                  Normal = Vector3.UnitZ
                                              };

                        entities.Add(ellipse);
                        break;
                    case 4:
                         dxfPairInfo = this.ReadCodePair();
                        // spline
                        List<SplineVertex> controlPoints = new List<SplineVertex>();
                        short degree = short.Parse(dxfPairInfo.Value); // code 94
                        dxfPairInfo = this.ReadCodePair();
                        int isRational = int.Parse(dxfPairInfo.Value); // code 73
                        dxfPairInfo = this.ReadCodePair();
                        int isPeriodic = int.Parse(dxfPairInfo.Value); // code 74
                        dxfPairInfo = this.ReadCodePair();
                        int numKnots = int.Parse(dxfPairInfo.Value); // code 95
                        dxfPairInfo = this.ReadCodePair();
                        int numControlPoints = int.Parse(dxfPairInfo.Value); // code 96
                        dxfPairInfo = this.ReadCodePair();
                        double[] knots = new double[numKnots];
                        for (int i = 0; i < numKnots; i++)
                        {
                            knots[i] = double.Parse(dxfPairInfo.Value);  // code 40
                            dxfPairInfo = this.ReadCodePair();
                        }

                        for (int i = 0; i < numControlPoints; i++)
                        {
                            double w = 1.0;
                            double x = double.Parse(dxfPairInfo.Value); // code 10
                            dxfPairInfo = this.ReadCodePair();
                            double y = double.Parse(dxfPairInfo.Value); // code 20
                            dxfPairInfo = this.ReadCodePair();
                            // control point weight might not be present
                            if (dxfPairInfo.Code == 42)
                            {
                                w = double.Parse(dxfPairInfo.Value); // code 42
                                dxfPairInfo = this.ReadCodePair();
                            }
                            
                            controlPoints.Add(new SplineVertex(x, y, 0.0, w));
                        }

                        // this information is only required for AutoCAD version 2010
                        // stores information about spline fit point (the spline entity does not make use of this information)
                        if (this.headerVariables.AcadVer >= DxfVersion.AutoCad2010)
                        {
                            int numFitData = int.Parse(dxfPairInfo.Value); // code 97
                            dxfPairInfo = this.ReadCodePair();
                            for (int i = 0; i < numFitData; i++)
                            {
                                double fitX = double.Parse(dxfPairInfo.Value); // code 11
                                dxfPairInfo = this.ReadCodePair();
                                double fitY = double.Parse(dxfPairInfo.Value); // code 21
                                dxfPairInfo = this.ReadCodePair();
                            }

                            // the info on start tangent might not appear
                            if (dxfPairInfo.Code == 12)
                            {
                                double startTanX = double.Parse(dxfPairInfo.Value); // code 12
                                dxfPairInfo = this.ReadCodePair();
                                double startTanY = double.Parse(dxfPairInfo.Value); // code 22
                                dxfPairInfo = this.ReadCodePair();
                            }
                            // the info on end tangent might not appear
                            if (dxfPairInfo.Code == 13)
                            {
                                double endTanX = double.Parse(dxfPairInfo.Value); // code 13
                                dxfPairInfo = this.ReadCodePair();
                                double endTanY = double.Parse(dxfPairInfo.Value); // code 23
                                dxfPairInfo = this.ReadCodePair();
                            }
                        }

                        Spline spline = new Spline(controlPoints, knots, degree);
                        entities.Add(spline);
                        break;
                }
            }

            return new HatchBoundaryPath(entities);
        }

        private HatchPattern ReadHatchPattern(string name)
        {
            HatchPattern hatch = null;
            double angle = 0.0;
            double scale = 1.0;
            bool isGradient = false;
            List<HatchPatternLineDefinition> lineDefinitions = new List<HatchPatternLineDefinition>();
            HatchType type = HatchType.UserDefined;
            HatchStyle style = HatchStyle.Normal;

            while (dxfPairInfo.Code != 0 && dxfPairInfo.Code != 1001)
            {
                switch (dxfPairInfo.Code)
                {
                    case 52:
                        angle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 47:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 98:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        style = (HatchStyle) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 76:
                        type = (HatchType) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 77:
                        // hatch pattern double flag (not used)
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 78:
                        // number of pattern definition lines
                        int numLines = int.Parse(dxfPairInfo.Value);
                        lineDefinitions = ReadHatchPatternDefinitionLine(scale, angle, numLines);
                        break;
                    case 450:
                        if (int.Parse(dxfPairInfo.Value) == 1)
                        {
                            isGradient = true; // gradient pattern
                            hatch = ReadHatchGradientPattern();
                        }
                        else
                            dxfPairInfo = this.ReadCodePair(); // solid hatch, we do not need to read anything else
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            if (!isGradient)
                hatch = new HatchPattern(name) {Angle = angle};

            hatch.Style = style;
            hatch.Scale = scale;
            hatch.Type = type;
            hatch.LineDefinitions = lineDefinitions;
            return hatch;
        }

        private HatchGradientPattern ReadHatchGradientPattern()
        {
            // the information for gradient pattern must follow an strict order
            //dxfPairInfo = this.ReadCodePair();  // code 450 not needed
            dxfPairInfo = this.ReadCodePair();  // code 451 not needed
            dxfPairInfo = this.ReadCodePair();
            double angle = double.Parse(dxfPairInfo.Value); // code 460
            dxfPairInfo = this.ReadCodePair();
            bool centered = ((int)double.Parse(dxfPairInfo.Value) == 0); // code 461
            dxfPairInfo = this.ReadCodePair();
            bool singleColor = (int.Parse(dxfPairInfo.Value) != 0); // code 452
            dxfPairInfo = this.ReadCodePair();
            double tint = double.Parse(dxfPairInfo.Value); // code 462
            dxfPairInfo = this.ReadCodePair();  // code 453 not needed

            dxfPairInfo = this.ReadCodePair();  // code 463 not needed (0.0)
            dxfPairInfo = this.ReadCodePair();  // code 63
            //AciColor color1 = new AciColor(short.Parse(dxfPairInfo.Value));
            dxfPairInfo = this.ReadCodePair(); // code 421
            AciColor color1 = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));

            dxfPairInfo = this.ReadCodePair();  // code 463 not needed (1.0)
            dxfPairInfo = this.ReadCodePair();  // code 63
            //AciColor color2 = new AciColor(short.Parse(dxfPairInfo.Value));
            dxfPairInfo = this.ReadCodePair();  // code 421
            AciColor color2 = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));

            dxfPairInfo = this.ReadCodePair();  // code 470
            string typeName = dxfPairInfo.Value;
            if (!StringEnum.IsStringDefined(typeof (HatchGradientPatternType), typeName))
                throw new DxfEntityException("HatchPatternGradient", "Unkown hatch gradient type: " + typeName);
            HatchGradientPatternType type = (HatchGradientPatternType) StringEnum.Parse(typeof(HatchGradientPatternType), typeName);

            if (singleColor)
                return new HatchGradientPattern(color1, tint, type)
                           {
                               Centered = centered,
                               Angle = angle * MathHelper.RadToDeg
                           };

            return new HatchGradientPattern(color1, color2, type)
                       {
                           Centered = centered,
                           Angle = angle * MathHelper.RadToDeg
                       };
        }

        private List<HatchPatternLineDefinition> ReadHatchPatternDefinitionLine(double patternScale, double patternAngle, int numLines)
        {
            List<HatchPatternLineDefinition> lineDefinitions = new List<HatchPatternLineDefinition>();

            dxfPairInfo = this.ReadCodePair();
            for (int i = 0; i < numLines; i++)
            {
                Vector2 origin = Vector2.Zero;
                Vector2 delta = Vector2.Zero;

                double angle = double.Parse(dxfPairInfo.Value); // code 53
                dxfPairInfo = this.ReadCodePair();

                origin.X = double.Parse(dxfPairInfo.Value); // code 43
                dxfPairInfo = this.ReadCodePair();

                origin.Y = double.Parse(dxfPairInfo.Value); // code 44
                dxfPairInfo = this.ReadCodePair();

                delta.X = double.Parse(dxfPairInfo.Value); // code 45
                dxfPairInfo = this.ReadCodePair();

                delta.Y = double.Parse(dxfPairInfo.Value); // code 46
                dxfPairInfo = this.ReadCodePair();

                int numSegments = int.Parse(dxfPairInfo.Value); // code 79
                dxfPairInfo = this.ReadCodePair();

                List<double> dashPattern = new List<double>();
                for (int j = 0; j < numSegments; j++)
                {
                    // positive values means solid segments and negative values means spaces (one entry per element)
                    dashPattern.Add(double.Parse(dxfPairInfo.Value) / patternScale); // code 49
                    dxfPairInfo = this.ReadCodePair();
                }

                // Pattern fill data. In theory this should hold the same information as the pat file but for unkown reason the dxf requires global data instead of local.
                // this means we have to convert the global data into local, since we are storing the pattern line definition as it appears in the acad.pat file.
                double sinOrigin = Math.Sin(patternAngle * MathHelper.DegToRad);
                double cosOrigin = Math.Cos(patternAngle * MathHelper.DegToRad);
                origin = new Vector2(cosOrigin * origin.X / patternScale + sinOrigin * origin.Y / patternScale, -sinOrigin * origin.X / patternScale + cosOrigin * origin.Y / patternScale);

                double sinDelta = Math.Sin(angle * MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle * MathHelper.DegToRad);
                delta = new Vector2(cosDelta * delta.X / patternScale + sinDelta * delta.Y / patternScale, -sinDelta * delta.X / patternScale + cosDelta * delta.Y / patternScale);

                lineDefinitions.Add(new HatchPatternLineDefinition
                                        {
                                            Angle = angle - patternAngle,
                                            Origin = origin,
                                            Delta = delta,
                                            DashPattern = dashPattern
                                        });
            }

            return lineDefinitions;
        }

        private Vertex ReadVertex()
        {
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Vector3 location = new Vector3();
            double endThickness = 0.0;
            double beginThickness = 0.0;
            double bulge = 0.0;
            List<int> vertexIndexes = new List<int>();
            VertexTypeFlags flags = VertexTypeFlags.PolylineVertex;

            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        beginThickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        endThickness = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        bulge = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        vertexIndexes.Add(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Vertex
                       {
                           Flags = flags,
                           Location = location,
                           BeginThickness = beginThickness,
                           Bulge = bulge,
                           Color = color,
                           EndThickness = endThickness,
                           Layer = layer,
                           LineType = lineType,
                           VertexIndexes = vertexIndexes.ToArray(),
                           Handle = handle
                       };
        }

        private void ReadUnknowEntity()
        {
            // if the entity is unknown keep reading until an end of section or a new entity is found
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                dxfPairInfo = this.ReadCodePair();
            }
        }

        #endregion

        #region object methods

        private DictionaryObject ReadDictionary()
        {
            string handle = null;
            string handleOwner = null;
            ClonningFlag clonning = ClonningFlag.KeepExisting;
            bool isHardOwner = false;
            int numEntries = 0;
            List<string> names = new List<string>();
            List<string> handlesToOwner = new List<string>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        handleOwner = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        isHardOwner = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        clonning = (ClonningFlag) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        numEntries += 1;
                        names.Add(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 350: // Soft-owner ID/handle to entry object 
                        handlesToOwner.Add(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 360:
                        // Hard-owner ID/handle to entry object
                        handlesToOwner.Add(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            DictionaryObject dictionary = new DictionaryObject(handleOwner)
                                              {
                                                  Handle = handle,
                                                  IsHardOwner = isHardOwner,
                                                  Clonning = clonning
                                              };
            for (int i = 0; i < numEntries; i++)
            {
                string id = handlesToOwner[i];
                if (id == null)
                    throw new NullReferenceException("Null handle in dictionary.");
                dictionary.Entries.Add(id, names[i]);
            }

            return dictionary;
        }

        private RasterVariables ReadRasterVariables()
        {
            RasterVariables variables = new RasterVariables();
            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        variables.Handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        variables.DisplayFrame = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        variables.DisplayQuality = (ImageDisplayQuality) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        variables.Units = (ImageUnits) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return variables;
        }

        private ImageDef ReadImageDefinition()
        {
            string handle = null;
            string ownerHandle = null;
            string fileName = null;
            string name = null;
            double width = 0, height = 0;
            float wPixel = 0.0f;
            float hPixel = 0.0f;
            ResolutionUnits units = ResolutionUnits.NoUnits;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        fileName = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        width = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        height = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        wPixel = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        hPixel = float.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        units = (ResolutionUnits) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        ownerHandle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            if (ownerHandle != null)
            {
                DictionaryObject imageDefDict = this.dictionaries[ownerHandle];
                if (handle == null)
                    throw new NullReferenceException("Null handle in ImageDef dictionary.");
                name = imageDefDict.Entries[handle];
            }

            ImageDef imageDef = new ImageDef(fileName, (int)width, 25.4f/wPixel, (int)height, 25.4f/hPixel, name, units)
                                    {
                                        Handle = handle
                                    };

            this.imageDefRefs.Add(imageDef.Name, new List<DxfObject>());
            this.imgDefHandles.Add(imageDef.Handle, imageDef);
            return imageDef;
        }

        private ImageDefReactor ReadImageDefReactor()
        {
            string handle = null;
            string imgOwner = null;

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        imgOwner = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                   default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new ImageDefReactor(imgOwner)
                       {
                           Handle = handle
                       };
        }

        private MLineStyle ReadMLineStyle()
        {
            string handle = null;
            string name = null;
            string description = null;
            AciColor fillColor = AciColor.ByLayer;
            double startAngle = 90.0;
            double endAngle = 90.0;
            MLineStyleFlags flags = MLineStyleFlags.None;
            List<MLineStyleElement> elements = new List<MLineStyleElement>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        name = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        description = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        if(!fillColor.UseTrueColor)
                            fillColor = AciColor.FromCadIndex(short.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420:
                        fillColor = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (MLineStyleFlags) int.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        startAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 52:
                        endAngle = double.Parse(dxfPairInfo.Value);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        int numElements = int.Parse(dxfPairInfo.Value);
                        elements = ReadMLineStyleElements(numElements);
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            if (string.IsNullOrEmpty(name)) return null;

            this.mlineStyleRefs.Add(name, new List<DxfObject>());
            MLineStyle style = new MLineStyle(name, description)
                                   {
                                       Handle = handle,
                                       FillColor = fillColor,
                                       Flags = flags,
                                       StartAngle = startAngle,
                                       EndAngle = endAngle,
                                       Elements = elements
                                   };

            foreach (MLineStyleElement e in elements)
            {
                this.lineTypeRefs[e.LineType.Name].Add(style);
            }   

            return style;
        }

        private List<MLineStyleElement> ReadMLineStyleElements(int numElements)
        {
            List<MLineStyleElement> elements = new List<MLineStyleElement>();

            dxfPairInfo = this.ReadCodePair();

            for (int i = 0; i < numElements; i++)
            {
                double offset = double.Parse(dxfPairInfo.Value); // code 49
                dxfPairInfo = this.ReadCodePair();

                AciColor color = AciColor.FromCadIndex(short.Parse(dxfPairInfo.Value));
                dxfPairInfo = this.ReadCodePair();

                if (dxfPairInfo.Code == 420)
                {
                    color = AciColor.FromTrueColor(int.Parse(dxfPairInfo.Value));   // code 420
                    dxfPairInfo = this.ReadCodePair();
                }

                // the linetype names ByLayer or ByBlock are case unsensitive
                string lineTypeName = dxfPairInfo.Value;   // code 6
                if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                    lineTypeName = "ByLayer";
                if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                    lineTypeName = "ByBlock";
                LineType lineType = GetLineType(lineTypeName);
                dxfPairInfo = this.ReadCodePair();

                MLineStyleElement element = new MLineStyleElement(offset)
                                                {
                                                    Color = color,
                                                    LineType = lineType
                                                };

                elements.Add(element);
            }

            return elements;
        }

        private Group ReadGroup()
        {
            string handle = null;
            string description = null;
            string name = null;
            bool isUnnamed = true;
            bool isSelectable = true;
            List<EntityObject> entities = new List<EntityObject>();

            dxfPairInfo = this.ReadCodePair();
            while (dxfPairInfo.Code != 0)
            {
                switch (dxfPairInfo.Code)
                {
                    case 5:
                        handle = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        string handleOwner = dxfPairInfo.Value;
                        DictionaryObject dict = this.dictionaries[handleOwner];
                        if (handle == null)
                            throw new NullReferenceException("Null handle in Group dictionary.");
                        name = dict.Entries[handle];
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 300:
                        description = dxfPairInfo.Value;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        isUnnamed = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        isSelectable = int.Parse(dxfPairInfo.Value) != 0;
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        // the groups are defined in the objects section after the entities section, any entity included in a group has been previously defined
                        EntityObject entity;
                        this.addedEntity.TryGetValue(dxfPairInfo.Value, out entity);
                        if(entity != null)
                            entities.Add(entity);
                        dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // we need to keep track of the group names generated
            if(isUnnamed)
                CheckGroupName(name);

            Group group = new Group
                {
                    Handle = handle,
                    Name = name,
                    Description = description,
                    IsUnnamed = isUnnamed,
                    IsSelectable = isSelectable,
                    Entities = entities
                };
            this.groupRefs.Add(group.Name, new List<DxfObject>());
            return group;

        }

        #endregion

        #region private methods

        private void CheckDimBlockName(string name)
        {
            // the autocad block names has the form *D#
            // we need to find which is the last available number, in case more dimensions are added
            if (!name.StartsWith("*D", StringComparison.InvariantCultureIgnoreCase)) return;
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num)) return;
            if (num > this.dimensionBlocksGenerated)
                this.dimensionBlocksGenerated = num;
        }

        private void CheckGroupName(string name)
        {
            // the autocad group names has the form *A#
            // we need to find which is the last available number, in case more groups are added
            if (!name.StartsWith("*A", StringComparison.InvariantCultureIgnoreCase)) return;
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num)) return;
            if (num > this.groupNamesGenerated)
                this.groupNamesGenerated = num;
        }

        private static TextAlignment ObtainAlignment(int horizontal, int vertical)
        {
            TextAlignment alignment = TextAlignment.BaselineLeft;

            if (horizontal == 0 && vertical == 3)
                alignment = TextAlignment.TopLeft;

            else if (horizontal == 1 && vertical == 3)
                alignment = TextAlignment.TopCenter;

            else if (horizontal == 2 && vertical == 3)
                alignment = TextAlignment.TopRight;

            else if (horizontal == 0 && vertical == 2)
                alignment = TextAlignment.MiddleLeft;

            else if (horizontal == 1 && vertical == 2)
                alignment = TextAlignment.MiddleCenter;

            else if (horizontal == 2 && vertical == 2)
                alignment = TextAlignment.MiddleRight;

            else if (horizontal == 0 && vertical == 1)
                alignment = TextAlignment.BottomLeft;

            else if (horizontal == 1 && vertical == 1)
                alignment = TextAlignment.BottomCenter;

            else if (horizontal == 2 && vertical == 1)
                alignment = TextAlignment.BottomRight;

            else if (horizontal == 0 && vertical == 0)
                alignment = TextAlignment.BaselineLeft;

            if (horizontal == 1 && vertical == 0)
                alignment = TextAlignment.BaselineCenter;

            else if (horizontal == 2 && vertical == 0)
                alignment = TextAlignment.BaselineRight;

            else if (horizontal == 3 && vertical == 0)
                alignment = TextAlignment.Aligned;

            else if (horizontal == 4 && vertical == 0)
                alignment = TextAlignment.Middle;

            else if (horizontal == 5 && vertical == 0)
                alignment = TextAlignment.Fit;

            return alignment;
        }

        private ApplicationRegistry GetApplicationRegistry(string name)
        {
            if (this.appRegistries.ContainsKey(name))
                return this.appRegistries[name];

            // if an entity references a table object not defined in the tables section a new one will be created
            ApplicationRegistry appReg = new ApplicationRegistry(name);
            int numHandle = appReg.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            this.appRegistries.Add(name, appReg);
            return appReg;
        }

        private Block GetBlock(string name)
        {
            Block block;
            if (this.blocks.TryGetValue(name, out block)) 
                return block;
            throw new ArgumentException("The block with name " + name + " does not exist.");
        }

        private Layer GetLayer(string name)
        {
            Layer layer;
            if (this.layers.TryGetValue(name, out layer))
                return this.layers[name];

            // if an entity references a table object not defined in the tables section a new one will be created
            layer = new Layer(name);
            layer.LineType = GetLineType(layer.LineType.Name);
            int numHandle = layer.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            this.layers.Add(name, layer);
            this.layerRefs.Add(name, new List<DxfObject>());
            return layer;
        }

        private LineType GetLineType(string name)
        {
            LineType lineType;
            if (this.lineTypes.TryGetValue(name, out lineType))
                return lineType;

            // if an entity references a table object not defined in the tables section a new one will be created
            lineType = new LineType(name);
            int numHandle = lineType.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            this.lineTypes.Add(name, lineType);
            this.lineTypeRefs.Add(name, new List<DxfObject>());
            return lineType;
        }

        private TextStyle GetTextStyle(string name)
        {
            TextStyle style;
            if (this.textStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            style = new TextStyle(name);
            int numHandle = style.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            this.textStyles.Add(name, style);
            this.textStyleRefs.Add(name, new List<DxfObject>());
            return style;
        }

        private TextStyle GetTextStyleByHandle(string handle)
        {
            foreach (TextStyle style in this.textStyles.Values)
            {
                if (style.Handle == handle)
                    return style;
            }
            throw new ArgumentException("The text style with handle " + handle + " does not exist.");
        }

        private DimensionStyle GetDimensionStyle(string name)
        {
            DimensionStyle style;
            if (this.dimStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            style = new DimensionStyle(name);
            style.TextStyle = GetTextStyle(style.TextStyle.Name);
            int numHandle = style.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            this.dimStyles.Add(name, style);
            this.dimStyleRefs.Add(name, new List<DxfObject>());
            return style;
        }

        private MLineStyle GetMLineStyle(string name)
        {

            MLineStyle style;
            if (this.mlineStyles.TryGetValue(name, out style))
                return style;
            throw new ArgumentException("The mline style with name " + name + " does not exist.");

            //if (this.mlineStyles.ContainsKey(name))
            //    return this.mlineStyles[name];

            //// if an entity references a table object not defined in the tables section a new one will be created
            //MLineStyle mlineStyle = new MLineStyle(name);
            //mlineStyle.Elements[0].LineType = GetLineType(mlineStyle.Elements[0].LineType.Name);
            //int numHandle = mlineStyle.AsignHandle(Convert.ToInt32(this.HeaderVariables.HandleSeed, 16));
            //this.headerVariables.HandleSeed = Convert.ToString(numHandle, 16);
            //this.mlineStyles.Add(name, mlineStyle);
            //this.mlineStyleRefs.Add(name, new List<DxfObject>());
            //return mlineStyle;
        }

        private static CodeValuePair ReadCodePair(TextReader reader)
        {
            int intCode;
            string readCode = reader.ReadLine();
            if (!int.TryParse(readCode, out intCode))
                throw (new DxfException("Invalid group code " + readCode));
            string value = reader.ReadLine();
            return new CodeValuePair(intCode, value);
        }

        private CodeValuePair ReadCodePair()
        {
            return ReadCodePair(this.reader);
        }

        private XData ReadXDataRecord(string appId)
        {
            ApplicationRegistry appReg = GetApplicationRegistry(appId);

            XData xData = new XData(appReg);
            dxfPairInfo = this.ReadCodePair();

            while (dxfPairInfo.Code >= 1000 && dxfPairInfo.Code <= 1071)
            {
                if (dxfPairInfo.Code == XDataCode.AppReg)
                    break;

                XDataRecord xDataRecord = new XDataRecord(dxfPairInfo.Code, dxfPairInfo.Value);
                xData.XDataRecord.Add(xDataRecord);
                dxfPairInfo = this.ReadCodePair();
            }

            return xData;
        }

        #endregion

    }
}