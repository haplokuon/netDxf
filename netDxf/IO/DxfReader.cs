#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2023 Daniel Carvajal (haplokuon@gmail.com)
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using Image = netDxf.Entities.Image;
using Point = netDxf.Entities.Point;
using Trace = netDxf.Entities.Trace;

namespace netDxf.IO
{
    /// <summary>
    /// Low level DXF reader
    /// </summary>
    internal sealed class DxfReader
    {
        #region private fields

        private bool isBinary;

        private ICodeValueReader chunk;
        private DxfDocument doc;

        // counter to create unique names for ShapeStyles
        private int shapeStyleCounter;

        // default name for null reference files or containing invalid characters
        private const string FileNotValid = "FILE_NOT_VALID";

        // here we will store strings already decoded <string: original, string: decoded>
        private Dictionary<string, string> decodedStrings;

        // blocks records <string: name, string: blockRecord>
        private Dictionary<string, BlockRecord> blockRecords;

        // entities, they will be processed at the end <Entity: entity, string: owner handle>.
        private Dictionary<DxfObject, string> entityList;

        // Viewports, they will be processed at the end <Entity: Viewport, string: clipping boundary handle>.
        private Dictionary<Viewport, string> viewports;

        private Dictionary<Hatch, List<HatchBoundaryPath>> hatchToPaths;
        // HatchBoundaryPaths, they will be processed at the end <Entity: HatchBoundaryPath, string: entity contour handle>.
        private Dictionary<HatchBoundaryPath, List<string>> hatchContours;

        // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
        // temporarily these variables will store information to post process the nested block list
        private Dictionary<Insert, string> nestedInserts;
        private Dictionary<Dimension, string> nestedDimensions;

        // the named dictionary is the only one we are interested, it is always the first that appears in the section
        // It consists solely of associated pairs of entry names and hard ownership pointer references to the associated object.
        private DictionaryObject namedDictionary;
        // <string: dictionary handle, DictionaryObject: dictionary>
        private Dictionary<string, DictionaryObject> dictionaries;

        // variables for post-processing
        private Dictionary<Leader, string> leaderAnnotation;
        private Dictionary<Block, List<EntityObject>> blockEntities;
        private Dictionary<Group, List<string>> groupEntities;
        private Dictionary<Viewport, List<Layer>> viewportFrozenLayers;

        // the order of each table group in the tables section may vary
        private Dictionary<DimensionStyle, string[]> dimStyleToHandles;

        // layer for post-processing
        private List<Layer> layers;

        // complex linetypes for post-processing
        private List<Linetype> complexLinetypes;
        private Dictionary<LinetypeSegment, string> linetypeSegmentStyleHandles;
        private Dictionary<LinetypeShapeSegment, short> linetypeShapeSegmentToNumber;

        // XData for table objects
        private Dictionary<DxfObject, List<XData>> tableXData;
        private Dictionary<DxfObject, List<XData>> tableEntryXData;

        // the MLineStyles are defined, in the objects section, AFTER the MLine that references them,
        // temporarily this variables will store information to post process the MLine list
        private Dictionary<MLine, string> mLineToStyleNames;

        // the ImageDefinition are defined, in the objects section AFTER the Image that references them,
        // temporarily these variables will store information to post process the Image list
        private Dictionary<Image, string> imgToImgDefHandles;
        private Dictionary<string, ImageDefinition> imgDefHandles;

        // the UnderlayDefinitions are defined, in the objects section, AFTER the Underlay entity that references them,
        // temporarily this variables will store information to post process the Underlay list
        private Dictionary<Underlay, string> underlayToDefinitionHandles;
        private Dictionary<string, UnderlayDefinition> underlayDefHandles;

        // temporarily this dictionary will store information to check any possible errors on model and paper space layouts
        private Dictionary<string, BlockRecord> blockRecordPointerToLayout;
        private List<Layout> orphanLayouts;

        // layer state manager handle, this is stored in the Layer table and points to a dictionary in the Objects section
        private string layerStateManagerDictionaryHandle;

        // XRecords
        private Dictionary<string, XRecord> xRecords;

        #endregion

        #region constructors

        #endregion

        #region public methods

        /// <summary>
        /// Reads the whole stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        public DxfDocument Read(Stream stream, IEnumerable<string> supportFolders)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            DxfVersion version = DxfDocument.CheckDxfFileVersion(stream, out this.isBinary);

            if (version < DxfVersion.AutoCad2000)
            {
                throw new DxfVersionNotSupportedException(string.Format("DXF file version not supported : {0}.", version), version);
            }

            Encoding encoding;
            if (version < DxfVersion.AutoCad2007)
            {

#if !NET4X
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif

                string dwgCodePage = CheckHeaderVariable(stream, HeaderVariableCode.DwgCodePage, out this.isBinary);
                if (string.IsNullOrEmpty(dwgCodePage))
                {
                    encoding = Encoding.GetEncoding(Encoding.ASCII.WindowsCodePage);
                    Debug.Assert(false, "No code page defined in the DXF.");
                }
                else
                {
                    if (int.TryParse(dwgCodePage.Split('_')[1], out int codepage))
                    {
                        try
                        {
                            encoding = Encoding.GetEncoding(codepage);
                        }
                        catch
                        {
                            encoding = Encoding.GetEncoding(Encoding.ASCII.WindowsCodePage);
                            Debug.Assert(false, "Invalid or not compatible code page defined in the DXF.");
                        }
                    }
                    else
                    {
                        encoding = Encoding.GetEncoding(Encoding.ASCII.WindowsCodePage);
                        Debug.Assert(false, "Invalid code page defined in the DXF.");
                    }
                }

            }
            else
            {
                encoding = Encoding.UTF8;
            }

            if (this.isBinary)
            {
                this.chunk = new BinaryCodeValueReader(new BinaryReader(stream), encoding);
            }
            else
            {
                this.chunk = new TextCodeValueReader(new StreamReader(stream, encoding, true));
            }

            
            this.doc = new DxfDocument(new HeaderVariables(), false, supportFolders);
            this.shapeStyleCounter = 0;

            this.entityList = new Dictionary<DxfObject, string>();
            this.viewports = new Dictionary<Viewport, string>();
            this.hatchToPaths = new Dictionary<Hatch, List<HatchBoundaryPath>>();
            this.hatchContours = new Dictionary<HatchBoundaryPath, List<string>>();
            this.decodedStrings = new Dictionary<string, string>();
            this.leaderAnnotation = new Dictionary<Leader, string>();
            this.viewportFrozenLayers = new Dictionary<Viewport, List<Layer>>();

            //tables
            this.tableXData = new Dictionary<DxfObject, List<XData>>() ;
            this.tableEntryXData = new Dictionary<DxfObject, List<XData>>();
            this.dimStyleToHandles = new Dictionary<DimensionStyle, string[]>();
            this.layers = new List<Layer>();
            this.complexLinetypes = new List<Linetype>();
            this.linetypeSegmentStyleHandles = new Dictionary<LinetypeSegment, string>();
            this.linetypeShapeSegmentToNumber = new Dictionary<LinetypeShapeSegment, short>();

            // blocks
            this.nestedInserts = new Dictionary<Insert, string>();
            this.nestedDimensions = new Dictionary<Dimension, string>();
            this.blockRecords = new Dictionary<string, BlockRecord>(StringComparer.OrdinalIgnoreCase);
            this.blockEntities = new Dictionary<Block, List<EntityObject>>();

            // objects
            this.dictionaries = new Dictionary<string, DictionaryObject>(StringComparer.OrdinalIgnoreCase);
            this.groupEntities = new Dictionary<Group, List<string>>();
            this.imgDefHandles = new Dictionary<string, ImageDefinition>(StringComparer.OrdinalIgnoreCase);
            this.imgToImgDefHandles = new Dictionary<Image, string>();
            this.mLineToStyleNames = new Dictionary<MLine, string>();
            this.underlayToDefinitionHandles = new Dictionary<Underlay, string>();
            this.underlayDefHandles = new Dictionary<string, UnderlayDefinition>();
            this.layerStateManagerDictionaryHandle = string.Empty;
            this.xRecords = new Dictionary<string, XRecord>() ;


            // for layouts errors workarounds
            this.blockRecordPointerToLayout = new Dictionary<string, BlockRecord>(StringComparer.OrdinalIgnoreCase);
            this.orphanLayouts = new List<Layout>();

            this.chunk.Next();

            // read the comments at the head of the file, any other comments will be ignored
            // they sometimes hold information about the program that has generated the DXF
            // binary files do not contain any comments
            this.doc.Comments.Clear();
            while (this.chunk.Code == 999)
            {
                this.doc.Comments.Add(this.chunk.ReadString());
                this.chunk.Next();
            }

            while (this.chunk.ReadString() != DxfObjectCode.EndOfFile)
            {
                if (this.chunk.ReadString() == DxfObjectCode.BeginSection)
                {
                    this.chunk.Next();
                    switch (this.chunk.ReadString())
                    {
                        case DxfObjectCode.HeaderSection:
                            this.ReadHeader();
                            break;
                        case DxfObjectCode.ClassesSection:
                            this.ReadClasses();
                            break;
                        case DxfObjectCode.TablesSection:
                            this.ReadTables();
                            break;
                        case DxfObjectCode.BlocksSection:
                            this.ReadBlocks();
                            break;
                        case DxfObjectCode.EntitiesSection:
                            this.ReadEntities();
                            break;
                        case DxfObjectCode.ObjectsSection:
                            this.ReadObjects();
                            break;
                        case DxfObjectCode.ThumbnailImageSection:
                            this.ReadThumbnailImage();
                            break;
                        case DxfObjectCode.AcdsDataSection:
                            this.ReadAcdsData();
                            break;
                        default:
                            this.ReadUnknowData();
                            break;
                    }
                }
                this.chunk.Next();
            }

            // perform all necessary post processes
            this.PostProcesses();

            // to play safe we will add the default table objects to the document in case they do not exist,
            // if they already present nothing is overridden
            // add default layer
            this.doc.Layers.Add(Layer.Default);

            // add default line types
            this.doc.Linetypes.Add(Linetype.ByLayer);
            this.doc.Linetypes.Add(Linetype.ByBlock);
            this.doc.Linetypes.Add(Linetype.Continuous);

            // add default text style
            this.doc.TextStyles.Add(TextStyle.Default);

            // add default application registry
            this.doc.ApplicationRegistries.Add(ApplicationRegistry.Default);

            // add default dimension style
            this.doc.DimensionStyles.Add(DimensionStyle.Default);

            // add default MLine style
            this.doc.MlineStyles.Add(MLineStyle.Default);

            this.doc.Entities.ActiveLayout = Layout.ModelSpaceName;

            return this.doc;
        }

        private static bool IsBinary(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            byte[] sentinel = reader.ReadBytes(22);
            StringBuilder sb = new StringBuilder(18);
            for (int i = 0; i < 18; i++)
            {
                sb.Append((char) sentinel[i]);
            }

            return sb.ToString() == "AutoCAD Binary DXF";
        }

        public static string CheckHeaderVariable(Stream stream, string headerVariable, out bool isBinary)
        {
            long startPosition;

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            try
            {
                startPosition = stream.Position;
            }
            catch (NotSupportedException ex)
            {
                throw new ArgumentException("Streams with not accessible Position property are not supported.", nameof(stream), ex);
            }

            ICodeValueReader chunk;
            string variable = string.Empty;
            isBinary = IsBinary(stream);
            stream.Position = startPosition;

            if (isBinary)
            {
                chunk = new BinaryCodeValueReader(new BinaryReader(stream), Encoding.ASCII);
            }
            else
            {
                chunk = new TextCodeValueReader(new StreamReader(stream));
            }

            chunk.Next();
            while (chunk.ReadString() != DxfObjectCode.EndOfFile)
            {
                chunk.Next();
                if (chunk.ReadString() == DxfObjectCode.HeaderSection)
                {
                    chunk.Next();
                    while (chunk.ReadString() != DxfObjectCode.EndSection)
                    {
                        string varName = chunk.ReadString();
                        chunk.Next();

                        if (varName == headerVariable)
                        {
                            variable = chunk.ReadString(); // we found the variable we are looking for
                            break;
                        }
                        // some header variables have more than one entry
                        while (chunk.Code != 0 && chunk.Code != 9)
                        {
                            chunk.Next();
                        }
                    }
                    // we only need to read the header section
                    break;
                }
            }
            stream.Position = startPosition;
            return variable;
        }

        #endregion

        #region sections methods

        private void ReadHeader()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.HeaderSection);

            Vector3 ucsOrg = Vector3.Zero;
            Vector3 ucsXDir = Vector3.UnitX;
            Vector3 ucsYDir = Vector3.UnitY;

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                string varName = this.chunk.ReadString();
                double julian;
                this.chunk.Next();

                switch (varName)
                {
                    case HeaderVariableCode.AcadVer:
                        string version = this.chunk.ReadString();
                        DxfVersion acadVer = StringEnum<DxfVersion>.Parse(version, StringComparison.OrdinalIgnoreCase);
                        if (acadVer < DxfVersion.AutoCad2000)
                        {
                            throw new NotSupportedException("Only AutoCad2000 and higher DXF versions are supported.");
                        }
                        this.doc.DrawingVariables.AcadVer = acadVer;
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.HandleSeed:
                        string handleSeed = this.chunk.ReadHex();
                        this.doc.DrawingVariables.HandleSeed = handleSeed;
                        this.doc.NumHandles = long.Parse(handleSeed, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.Angbase:
                        this.doc.DrawingVariables.Angbase = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.Angdir:
                        this.doc.DrawingVariables.Angdir = (AngleDirection) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.AttMode:
                        this.doc.DrawingVariables.AttMode = (AttMode) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.AUnits:
                        this.doc.DrawingVariables.AUnits = (AngleUnitType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.AUprec:
                        this.doc.DrawingVariables.AUprec = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CeColor:
                        this.doc.DrawingVariables.CeColor = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CeLtScale:
                        this.doc.DrawingVariables.CeLtScale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CeLtype:
                        this.doc.DrawingVariables.CeLtype = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CeLweight:
                        this.doc.DrawingVariables.CeLweight = (Lineweight) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CLayer:
                        this.doc.DrawingVariables.CLayer = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CMLJust:
                        this.doc.DrawingVariables.CMLJust = (MLineJustification) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CMLScale:
                        this.doc.DrawingVariables.CMLScale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.CMLStyle:
                        string mLineStyleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!string.IsNullOrEmpty(mLineStyleName))
                        {
                            this.doc.DrawingVariables.CMLStyle = mLineStyleName;
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.DimStyle:
                        string dimStyleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!string.IsNullOrEmpty(dimStyleName))
                        {
                            this.doc.DrawingVariables.DimStyle = dimStyleName;
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TextSize:
                        double size = this.chunk.ReadDouble();
                        if (size > 0.0)
                        {
                            this.doc.DrawingVariables.TextSize = size;
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TextStyle:
                        string textStyleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!string.IsNullOrEmpty(textStyleName))
                        {
                            this.doc.DrawingVariables.TextStyle = textStyleName;
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.LastSavedBy:
                        this.doc.DrawingVariables.LastSavedBy = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.LUnits:
                        this.doc.DrawingVariables.LUnits = (LinearUnitType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.LUprec:
                        short luprec = this.chunk.ReadShort();
                        if (luprec < 0 || luprec > 8)
                        {
                            luprec = 4;
                        }
                        this.doc.DrawingVariables.LUprec = luprec;
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.DwgCodePage:
                        this.doc.DrawingVariables.DwgCodePage = this.chunk.ReadString();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.Extnames:
                        this.doc.DrawingVariables.Extnames = this.chunk.ReadBool();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.InsBase:
                        this.doc.DrawingVariables.InsBase = this.ReadHeaderVector();
                        break;
                    case HeaderVariableCode.InsUnits:
                        this.doc.DrawingVariables.InsUnits = (DrawingUnits) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.LtScale:
                        this.doc.DrawingVariables.LtScale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.LwDisplay:
                        this.doc.DrawingVariables.LwDisplay = this.chunk.ReadBool();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.MirrText:
                        short mirrText = this.chunk.ReadShort();
                        this.doc.DrawingVariables.MirrText = mirrText != 0;
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.PdMode:
                        this.doc.DrawingVariables.PdMode = (PointShape) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.PdSize:
                        this.doc.DrawingVariables.PdSize = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.PLineGen:
                        this.doc.DrawingVariables.PLineGen = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.PsLtScale:
                        this.doc.DrawingVariables.PsLtScale = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.SplineSegs:
                        short splineSegs = this.chunk.ReadShort();
                        if (splineSegs != 0)
                        {
                            // only positive values are supported
                            this.doc.DrawingVariables.SplineSegs = Math.Abs(splineSegs);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.SurfU:
                        short surfU = this.chunk.ReadShort();
                        if (surfU < 2 || surfU > 200)
                        {
                            surfU = 6;
                        }
                        this.doc.DrawingVariables.SplineSegs = surfU;
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.SurfV:
                        short surfV = this.chunk.ReadShort();
                        if (surfV < 2 || surfV > 200)
                        {
                            surfV = 6;
                        }
                        this.doc.DrawingVariables.SplineSegs = surfV;
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TdCreate:
                        julian = this.chunk.ReadDouble();
                        if (julian < 1721426 || julian > 5373484)
                        {
                            this.doc.DrawingVariables.TdCreate = DateTime.Now;
                        }
                        else
                        {
                            this.doc.DrawingVariables.TdCreate = DrawingTime.FromJulianCalendar(julian);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TduCreate:
                        julian = this.chunk.ReadDouble();
                        if (julian < 1721426 || julian > 5373484)
                        {
                            this.doc.DrawingVariables.TduCreate = DateTime.Now;
                        }
                        else
                        {
                            this.doc.DrawingVariables.TduCreate = DrawingTime.FromJulianCalendar(julian);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TdUpdate:
                        julian = this.chunk.ReadDouble();
                        if (julian < 1721426 || julian > 5373484)
                        {
                            this.doc.DrawingVariables.TdUpdate = DateTime.Now;
                        }
                        else
                        {
                            this.doc.DrawingVariables.TdUpdate = DrawingTime.FromJulianCalendar(julian);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TduUpdate:
                        julian = this.chunk.ReadDouble();
                        if (julian < 1721426 || julian > 5373484)
                        {
                            this.doc.DrawingVariables.TduUpdate = DateTime.Now;
                        }
                        else
                        {
                            this.doc.DrawingVariables.TduUpdate = DrawingTime.FromJulianCalendar(julian);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.TdinDwg:
                        double elapsed = this.chunk.ReadDouble();
                        if (elapsed < 0 || elapsed > TimeSpan.MaxValue.TotalDays)
                        {
                            this.doc.DrawingVariables.TdinDwg = TimeSpan.Zero;
                        }
                        else
                        {
                            this.doc.DrawingVariables.TdinDwg = DrawingTime.EditingTime(elapsed);
                        }
                        this.chunk.Next();
                        break;
                    case HeaderVariableCode.UcsOrg:
                        ucsOrg = this.ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsXDir:
                        ucsXDir = this.ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsYDir:
                        ucsYDir = this.ReadHeaderVector();
                        break;
                    default:

                        // avoid reading the header variables related with the current dimension style
                        // avoid reading the $ACADMAINTVER variable the official DXF documentation says "Maintenance version number (should be ignored)"
                        // avoid reading the $INTERFEREOBJVS and $INTERFEREVPVS variables, they are related to the visual style information that netDxf does not support
                        // and if present in a saved DXF file they, somehow, interfere with the copying to the clipboard inside AutoCAD
                        if (varName.StartsWith("$DIM", StringComparison.InvariantCultureIgnoreCase) ||
                            varName.Equals("$ACADMAINTVER", StringComparison.InvariantCultureIgnoreCase) ||
                            varName.Equals("$INTERFEREOBJVS", StringComparison.InvariantCultureIgnoreCase) ||
                            varName.Equals("$INTERFEREVPVS", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // some header variables have more than one entry
                            while (this.chunk.Code != 0 && this.chunk.Code != 9)
                            {
                                this.chunk.Next();
                            }
                        }
                        else 
                        {
                            // not recognized header variables will be added to the custom list
                            HeaderVariable variable;
                            if (this.chunk.Code == 10)
                            {
                                double x = this.chunk.ReadDouble();
                                this.chunk.Next();
                                double y = this.chunk.ReadDouble();
                                this.chunk.Next();
                                // the code 30 might not exist
                                if (this.chunk.Code == 30)
                                {
                                    double z = this.chunk.ReadDouble();
                                    this.chunk.Next();
                                    variable = new HeaderVariable(varName, 30, new Vector3(x, y, z));
                                }
                                else
                                {
                                    variable = new HeaderVariable(varName, 20, new Vector2(x, y));
                                }
                            }
                            else
                            {
                                variable = new HeaderVariable(varName, this.chunk.Code, this.chunk.Value);
                                this.chunk.Next();
                            }

                            //avoid duplicate custom header variables
                            if (this.doc.DrawingVariables.ContainsCustomVariable(varName))
                            {
                                this.doc.DrawingVariables.RemoveCustomVariable(varName);
                            }

                            this.doc.DrawingVariables.AddCustomVariable(variable);
                        }

                        break;
                }
            }

            if (!Vector3.ArePerpendicular(ucsXDir, ucsYDir))
            {
                ucsXDir = Vector3.UnitX;
                ucsYDir = Vector3.UnitY;
            }
            this.doc.DrawingVariables.CurrentUCS = new UCS("Unnamed", ucsOrg, ucsXDir, ucsYDir);
        }

        private Vector3 ReadHeaderVector()
        {
            Vector3 pos = Vector3.Zero;

            while (this.chunk.Code != 0 && this.chunk.Code != 9)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        pos.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        pos.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        pos.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    default:
                        throw new Exception("Invalid code in vector header variable.");
                }
            }
            return pos;
        }

        private void ReadClasses()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.ClassesSection);

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                this.ReadUnknowData();
            }
        }

        private void ReadTables()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.TablesSection);

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                this.ReadTable();
            }

            // check if all table collections has been created
            if (this.doc.ApplicationRegistries == null)
            {
                this.doc.ApplicationRegistries = new ApplicationRegistries(this.doc);
            }
            if (this.doc.Blocks == null)
            {
                this.doc.Blocks = new BlockRecords(this.doc);
            }
            if (this.doc.DimensionStyles == null)
            {
                this.doc.DimensionStyles = new DimensionStyles(this.doc);
            }
            if (this.doc.Layers == null)
            {
                this.doc.Layers = new Layers(this.doc);
            }
            if (this.doc.Linetypes == null)
            {
                this.doc.Linetypes = new Linetypes(this.doc);
            }
            if (this.doc.TextStyles == null)
            {
                this.doc.TextStyles = new TextStyles(this.doc);
            }
            if (this.doc.ShapeStyles == null)
            {
                this.doc.ShapeStyles = new ShapeStyles(this.doc);
            }
            if (this.doc.UCSs == null)
            {
                this.doc.UCSs = new UCSs(this.doc);
            }
            if (this.doc.Views == null)
            {
                this.doc.Views = new Views(this.doc);
            }
            if (this.doc.VPorts == null)
            {
                this.doc.VPorts = new VPorts(this.doc);
            }

            // post process complex linetypes
            foreach (KeyValuePair<LinetypeSegment, string> pair in this.linetypeSegmentStyleHandles)
            {
                switch (pair.Key.Type)
                {
                    case LinetypeSegmentType.Shape:
                        if (this.doc.GetObjectByHandle(pair.Value) is ShapeStyle shape)
                        {
                            ((LinetypeShapeSegment) pair.Key).Style = shape;
                        }
                        break;
                    case LinetypeSegmentType.Text:
                        if (this.doc.GetObjectByHandle(pair.Value) is TextStyle style)
                        {
                            ((LinetypeTextSegment) pair.Key).Style = style;
                        }
                        break;
                }
            }

            List<LinetypeSegment> remove = new List<LinetypeSegment>();
            foreach (KeyValuePair<LinetypeShapeSegment, short> pair in this.linetypeShapeSegmentToNumber)
            {
                string name = pair.Key.Style.ShapeName(pair.Value);
                if (string.IsNullOrEmpty(name))
                {
                    remove.Add(pair.Key);
                }
                else
                {
                    pair.Key.Name = name;
                }
            }

            // add the pending complex line types
            foreach (Linetype complexLinetype in this.complexLinetypes)
            {
                // remove invalid linetype shape segments
                foreach (LinetypeSegment s in remove)
                {
                    complexLinetype.Segments.Remove(s);
                }
                this.doc.Linetypes.Add(complexLinetype, false);
            }

            foreach (Linetype complexLinetype in this.complexLinetypes)
            {
                // remove invalid linetype shape segments
                foreach (LinetypeSegment s in remove)
                {
                    complexLinetype.Segments.Remove(s);
                }
                this.doc.Linetypes.Add(complexLinetype, false);
            }

            // now that the linetype list is fully initialized we can add the layer to the document
            foreach (Layer layer in this.layers)
            {
                this.doc.Layers.Add(layer, false);
            }

            // post process extended data information for table collections
            foreach (KeyValuePair<DxfObject, List<XData>> pair in this.tableXData)
            {
                DxfObject table = pair.Key;
                table.XData.AddRange(pair.Value);
            }

            //post process table entries XData
            foreach (KeyValuePair<DxfObject, List<XData>> pair in this.tableEntryXData)
            {
                if (pair.Key is ApplicationRegistry appReg)
                {
                    this.doc.ApplicationRegistries[appReg.Name].XData.AddRange(pair.Value);
                }
                else
                {
                    pair.Key.XData.AddRange(pair.Value);
                }

                if (pair.Key is Layer layer)
                {
                    // read the layer description from the extended data
                    if (layer.XData.TryGetValue("AcAecLayerStandard", out XData xDataDescription))
                    {
                        // there should be two entries the second holds the description value, the last 1000 code will be used
                        foreach (XDataRecord record in xDataDescription.XDataRecord)
                        {
                            if (record.Code == XDataCode.String)
                            {
                                layer.Description = (string) record.Value;
                            }
                        }
                    }

                    // read the layer transparency from the extended data
                    if (layer.XData.TryGetValue("AcCmTransparency", out XData xDataTransparency))
                    {
                        // there should be only one entry with the transparency value, the last 1071 code will be used
                        foreach (XDataRecord record in xDataTransparency.XDataRecord)
                        {
                            if (record.Code == XDataCode.Int32)
                            {
                                layer.Transparency = Transparency.FromAlphaValue((int) record.Value);
                            }
                        }
                    }
                }
            }
        }

        private void ReadBlocks()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.BlocksSection);

            // the blocks list will be added to the document after reading the blocks section to handle possible nested insert cases.
            Dictionary<string, Block> blocks = new Dictionary<string, Block>(StringComparer.OrdinalIgnoreCase);

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                switch (this.chunk.ReadString())
                {
                    case DxfObjectCode.BeginBlock:
                        Block block = this.ReadBlock();
                        blocks.Add(block.Name, block);
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            // post process the possible nested blocks,
            // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the insert that references them
            foreach (KeyValuePair<Insert, string> pair in this.nestedInserts)
            {
                Insert insert = pair.Key;
                insert.Block = blocks[pair.Value];
                foreach (Attribute att in insert.Attributes)
                {
                    // attribute definitions might be null if an INSERT entity attribute has not been defined in the block
                    if (insert.Block.AttributeDefinitions.TryGetValue(att.Tag, out AttributeDefinition attDef))
                    {
                        att.Definition = attDef;
                    }
                    att.Owner = insert;
                }
                // in the case the insert belongs to a *PaperSpace# the insert owner has not been assigned yet,
                // in this case the owner units are the document units and will be assigned at the end with the rest of the entities 
                if (insert.Owner != null)
                {
                    // apply the units scale to the insertion scale (this is for nested blocks)
                    double scale = UnitHelper.ConversionFactor(insert.Owner.Record.Units, insert.Block.Record.Units);
                    insert.Scale *= scale;
                }
            }
            foreach (KeyValuePair<Dimension, string> pair in this.nestedDimensions)
            {
                Dimension dim = pair.Key;
                if (pair.Value == null) continue;
                if (blocks.TryGetValue(pair.Value, out Block block))
                {
                    dim.Block = block;
                }
            }

            // add the blocks to the document
            // the block entities will not be added to the document at this point
            // entities like MLine and Image require information that is defined AFTER the block section,
            // this is the case of the MLineStyle and ImageDefinition that are described in the objects section
            foreach (Block block in blocks.Values)
            {
                this.doc.Blocks.Add(block, false);
            }
        }

        private void ReadEntities()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.EntitiesSection);

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                this.ReadEntity(false);
            }
        }

        private void ReadObjects()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.ObjectsSection);

            this.chunk.Next();
            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                switch (this.chunk.ReadString())
                {
                    case DxfObjectCode.Dictionary:
                        DictionaryObject dictionary = this.ReadDictionary();
                        this.dictionaries.Add(dictionary.Handle, dictionary);
                        // the named dictionary is always the first in the objects section
                        if (this.namedDictionary == null)
                        {
                            this.CreateObjectCollection(dictionary);
                            this.namedDictionary = dictionary;
                        }
                        break;
                    case DxfObjectCode.RasterVariables:
                        this.doc.RasterVariables = this.ReadRasterVariables();
                        break;
                    case DxfObjectCode.ImageDef:
                        ImageDefinition imageDefinition = this.ReadImageDefinition();
                        Debug.Assert(imageDefinition != null, "ImageDefinition cannot be null");
                        if (imageDefinition != null)
                        {
                            this.doc.ImageDefinitions.Add(imageDefinition, false);
                        }
                        break;
                    case DxfObjectCode.ImageDefReactor:
                        // this information is not needed
                        ImageDefinitionReactor reactor = this.ReadImageDefReactor();
                        break;
                    case DxfObjectCode.MLineStyle:
                        MLineStyle style = this.ReadMLineStyle();
                        Debug.Assert(style != null, "MLineStyle cannot be null");
                        if (style != null)
                        {
                            this.doc.MlineStyles.Add(style, false);
                        }
                        break;
                    case DxfObjectCode.Group:
                        Group group = this.ReadGroup();
                        Debug.Assert(group != null, "Group cannot be null");
                        if (group != null)
                        {
                            this.doc.Groups.Add(group, false);
                        }
                        break;
                    case DxfObjectCode.Layout:
                        Layout layout = this.ReadLayout();
                        Debug.Assert(layout != null, "Layout cannot be null");
                        if (layout.AssociatedBlock == null)
                        {
                            this.orphanLayouts.Add(layout);
                        }
                        else
                        {
                            this.doc.Layouts.Add(layout, false);
                        }
                        break;
                    case DxfObjectCode.UnderlayDgnDefinition:
                        UnderlayDgnDefinition underlayDgnDef = (UnderlayDgnDefinition) this.ReadUnderlayDefinition(UnderlayType.DGN);
                        Debug.Assert(underlayDgnDef != null, "UnderlayDgnDefinition cannot be null");
                        if (underlayDgnDef != null)
                        {
                            this.doc.UnderlayDgnDefinitions.Add(underlayDgnDef, false);
                        }
                        break;
                    case DxfObjectCode.UnderlayDwfDefinition:
                        UnderlayDwfDefinition underlayDwfDef = (UnderlayDwfDefinition) this.ReadUnderlayDefinition(UnderlayType.DWF);
                        Debug.Assert(underlayDwfDef != null, "UnderlayDwfDefinition cannot be null");
                        if (underlayDwfDef != null)
                        {
                            this.doc.UnderlayDwfDefinitions.Add(underlayDwfDef, false);
                        }
                        break;
                    case DxfObjectCode.UnderlayPdfDefinition:
                        UnderlayPdfDefinition underlayPdfDef = (UnderlayPdfDefinition) this.ReadUnderlayDefinition(UnderlayType.PDF);
                        Debug.Assert(underlayPdfDef != null, "UnderlayPdfDefinition cannot be null");
                        if (underlayPdfDef != null)
                        {
                            this.doc.UnderlayPdfDefinitions.Add(underlayPdfDef, false);
                        }
                        break;
                    case DxfObjectCode.XRecord:
                        XRecord xRecord = this.ReadXRecord();
                        Debug.Assert(xRecord != null, "XRecord cannot be null");
                        if (xRecord != null)
                        {
                            this.xRecords.Add(xRecord.Handle, xRecord);
                        }
                        break;
                    default:
                        do
                        {
                            this.chunk.Next();
                        } while (this.chunk.Code != 0);
                        break;
                }
            }

            this.BuildLayerStateManager();

            // this will try to fix problems with layouts and model/paper space blocks
            // nothing of this is be necessary in a well formed DXF
            this.RelinkOrphanLayouts();

            // raster variables
            if (this.doc.RasterVariables == null)
            {
                this.doc.RasterVariables = new RasterVariables(this.doc);
            }

            // assign XData to collections
            foreach (KeyValuePair<string, DictionaryObject> dictionary in this.dictionaries)
            {
                DxfObject table = this.doc.GetObjectByHandle(dictionary.Key);
                if (table != null)
                {
                    table.XData.AddRange(dictionary.Value.XData.Values);
                }

            }
        }

        private void ReadThumbnailImage()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.ThumbnailImageSection);

            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                //read the thumbnail image
                do
                {
                    this.chunk.Next();
                } while (this.chunk.Code != 0);
            }
        }

        private void ReadAcdsData()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.AcdsDataSection);

            while (this.chunk.ReadString() != DxfObjectCode.EndSection)
            {
                //read the ACDSSCHEMA and ACDSRECORD, multiple entries
                do
                {
                    this.chunk.Next();
                } while (this.chunk.Code != 0);
            }
        }

        #endregion

        #region table methods

        private void ReadTable()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.Table);
            List<XData> xData = new List<XData>();
            string handle = null;
            this.chunk.Next();
            string tableName = this.chunk.ReadString();
            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 330:
                        string owner = this.chunk.ReadHex();
                        // owner should be always, 0 handle of the document.
                        Debug.Assert(owner == "0");
                        this.chunk.Next();
                        break;
                    case 102:
                        string dictHandle = this.ReadExtensionDictionaryGroup();
                        if (string.Equals(tableName, DxfObjectCode.LayerTable))
                        {
                            this.layerStateManagerDictionaryHandle = dictHandle;
                        }
                        this.chunk.Next();
                        break;
                    case 100:
                        Debug.Assert(this.chunk.ReadString() == SubclassMarker.Table || this.chunk.ReadString() == SubclassMarker.DimensionStyleTable);
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // create collection
            switch (tableName)
            {
                case DxfObjectCode.ApplicationIdTable:
                    this.doc.ApplicationRegistries = new ApplicationRegistries(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.ApplicationRegistries, xData);
                    }
                    break;
                case DxfObjectCode.BlockRecordTable:
                    this.doc.Blocks = new BlockRecords(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.Blocks, xData);
                    }
                    break;
                case DxfObjectCode.DimensionStyleTable:
                    this.doc.DimensionStyles = new DimensionStyles(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.DimensionStyles, xData);
                    }
                    break;
                case DxfObjectCode.LayerTable:
                    this.doc.Layers = new Layers(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.Layers, xData);
                    }
                    break;
                case DxfObjectCode.LinetypeTable:
                    this.doc.Linetypes = new Linetypes(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.Linetypes, xData);
                    }
                    break;
                case DxfObjectCode.TextStyleTable:
                    this.doc.TextStyles = new TextStyles(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.TextStyles, xData);
                    }
                    this.doc.ShapeStyles = new ShapeStyles(this.doc);
                    break;
                case DxfObjectCode.UcsTable:
                    this.doc.UCSs = new UCSs(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.UCSs, xData);
                    }
                    break;
                case DxfObjectCode.ViewTable:
                    this.doc.Views = new Views(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.Views, xData);
                    }
                    break;
                case DxfObjectCode.VportTable:
                    this.doc.VPorts = new VPorts(this.doc, handle);
                    if (xData.Count > 0)
                    {
                        this.tableXData.Add(this.doc.VPorts, xData);
                    }
                    break;
                default:
                    throw new Exception(string.Format("Unknown Table name {0} at position {1}", tableName, this.chunk.CurrentPosition));
            }

            // read table entries
            while (this.chunk.ReadString() != DxfObjectCode.EndTable)
            {
                this.ReadTableEntry();
            }

            this.chunk.Next();
        }

        private void ReadTableEntry()
        {
            string dxfCode = this.chunk.ReadString();
            string handle = null;

            // only the first *Active VPort is supported, this one describes the current document view.
            VPort active = null;

            while (this.chunk.ReadString() != DxfObjectCode.EndTable)
            {
                // table entry common codes
                while (this.chunk.Code != 100)
                {
                    switch (this.chunk.Code)
                    {
                        case 5:
                            handle = this.chunk.ReadHex();
                            this.chunk.Next();
                            break;
                        case 105:
                            // this handle code is specific of dimension styles
                            handle = this.chunk.ReadHex();
                            this.chunk.Next();
                            break;
                        case 330:
                            //string owner = this.chunk.ReadHandle(); // owner should be, always, the handle of the list to which the entry belongs.
                            this.chunk.Next();
                            break;
                        case 102:
                            this.ReadExtensionDictionaryGroup();
                            this.chunk.Next();
                            break;
                        default:
                            this.chunk.Next();
                            break;
                    }
                }

                Debug.Assert(!string.IsNullOrEmpty(handle), "Table entry without handle.");

                this.chunk.Next();

                switch (dxfCode)
                {
                    case DxfObjectCode.ApplicationIdTable:
                        ApplicationRegistry appReg = this.ReadApplicationId();
                        if (appReg != null)
                        {
                            appReg.Handle = handle;
                            this.doc.ApplicationRegistries.Add(appReg, false);
                        }
                        break;
                    case DxfObjectCode.BlockRecordTable:
                        BlockRecord record = this.ReadBlockRecord();
                        if (record != null)
                        {
                            record.Handle = handle;
                            this.blockRecords.Add(record.Name, record);
                        }
                        break;
                    case DxfObjectCode.DimensionStyleTable:
                        DimensionStyle dimStyle = this.ReadDimensionStyle();
                        if (dimStyle != null)
                        {
                            dimStyle.Handle = handle;
                            this.doc.DimensionStyles.Add(dimStyle, false);
                        }
                        break;
                    case DxfObjectCode.LayerTable:
                        Layer layer = this.ReadLayer();
                        if (layer != null)
                        {
                            layer.Handle = handle;
                            this.layers.Add(layer);
                        }
                        break;
                    case DxfObjectCode.LinetypeTable:
                        Linetype linetype = this.ReadLinetype(out bool isComplex);
                        if (linetype != null)
                        {
                            linetype.Handle = handle;
                            // complex linetypes will be added after reading the style table
                            // they depend on TextStyles and/or ShapeStyles
                            if (isComplex)
                            {
                                this.complexLinetypes.Add(linetype);
                            }
                            else
                            {
                                this.doc.Linetypes.Add(linetype, false);
                            }
                        }
                        break;
                    case DxfObjectCode.TextStyleTable:
                        // the DXF stores text and shape definitions in the same table
                        DxfObject style = this.ReadTextStyle();
                        if (style != null)
                        {
                            style.Handle = handle;
                            if (style is TextStyle textStyle)
                            {
                                this.doc.TextStyles.Add(textStyle, false);
                            }
                            else
                            {
                                this.doc.ShapeStyles.Add(style as ShapeStyle, false);
                            }
                        }
                        break;
                    case DxfObjectCode.UcsTable:
                        UCS ucs = this.ReadUCS();
                        if (ucs != null)
                        {
                            ucs.Handle = handle;
                            this.doc.UCSs.Add(ucs, false);
                        }
                        break;
                    case DxfObjectCode.ViewTable:
                        this.ReadView();
                        //this.doc.Views.Add((View) entry);
                        break;
                    case DxfObjectCode.VportTable:
                        if (active == null)
                        {
                            VPort vport = this.ReadVPort();
                            if (vport != null)
                            {
                                // only the first *Active VPort is supported, this one describes the current document view.
                                if (vport.Name.Equals(VPort.DefaultName, StringComparison.OrdinalIgnoreCase))
                                {
                                    this.doc.Viewport.Handle = handle;
                                    this.doc.Viewport.ViewCenter = vport.ViewCenter;
                                    this.doc.Viewport.SnapBasePoint = vport.SnapBasePoint;
                                    this.doc.Viewport.SnapSpacing = vport.SnapSpacing;
                                    this.doc.Viewport.GridSpacing = vport.GridSpacing;
                                    this.doc.Viewport.ViewDirection = vport.ViewDirection;
                                    this.doc.Viewport.ViewTarget = vport.ViewTarget;
                                    this.doc.Viewport.ViewHeight = vport.ViewHeight;
                                    this.doc.Viewport.ViewAspectRatio = vport.ViewAspectRatio;
                                    this.doc.Viewport.ShowGrid = vport.ShowGrid;
                                    this.doc.Viewport.SnapMode = vport.SnapMode;
                                    active = this.doc.Viewport;
                                }
                            }
                        }
                        else
                        {
                            // there is only need to read the first active VPort
                            // multiple Model viewports are not supported
                            this.ReadUnknowData();
                        }                     
                        break;
                    default:
                        this.ReadUnknowData();
                        return;
                }
            }
        }

        private ApplicationRegistry ReadApplicationId()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.ApplicationId);

            string appId = string.Empty;
            List<XData> xData = new List<XData>();
            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 1001:
                        string id = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(id));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(appId), "Table object name is not valid.");
            if (!TableObject.IsValidName(appId))
            {
                return null;
            }

            ApplicationRegistry applicationRegistry = new ApplicationRegistry(appId, false);
            if(xData.Count>0) this.tableEntryXData.Add(applicationRegistry, xData);
            return applicationRegistry;
        }

        private BlockRecord ReadBlockRecord()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.BlockRecord);

            string name = string.Empty;
            DrawingUnits units = DrawingUnits.Unitless;
            bool allowExploding = true;
            bool scaleUniformly = false;
            List<XData> xData = new List<XData>();
            string pointerToLayout = string.Empty;

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        units = (DrawingUnits) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 280:
                        allowExploding = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 281:
                        scaleUniformly = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 340:
                        pointerToLayout = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // we need to check for generated blocks by dimensions, even if the dimension was deleted the block might persist in the drawing.
            this.CheckDimBlockName(name);

            BlockRecord record = new BlockRecord(name)
            {
                Units = units,
                AllowExploding = allowExploding,
                ScaleUniformly = scaleUniformly
            };

            if (xData.Count > 0) this.tableEntryXData.Add(record, xData);

            // here is where DXF versions prior to AutoCad2007 stores the block units
            // read the layer transparency from the extended data
            if (record.XData.TryGetValue(ApplicationRegistry.DefaultName, out XData designCenterData))
            {
                using (IEnumerator<XDataRecord> records = designCenterData.XDataRecord.GetEnumerator())
                {
                    while (records.MoveNext())
                    {
                        XDataRecord data = records.Current;
                        if (data == null)
                        {
                            break; // premature end
                        }

                        // the record units are stored under the string "DesignCenter Data"
                        if (data.Code == XDataCode.String && string.Equals((string)data.Value, "DesignCenter Data", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (records.MoveNext())
                            {
                                data = records.Current;
                            }
                            else
                            {
                                break; // premature end
                            }

                            // all style overrides are enclosed between XDataCode.ControlString "{" and "}"
                            if (data == null) break; // premature end
                            if (data.Code != XDataCode.ControlString)
                            {
                                break; // premature end
                            }

                            if (records.MoveNext())
                            {
                                data = records.Current;
                            }
                            else
                            {
                                break; // premature end
                            }

                            if (data == null) continue;
                            while (data.Code != XDataCode.ControlString)
                            {
                                if (records.MoveNext())
                                {
                                    data = records.Current;
                                }
                                else
                                {
                                    break; // premature end
                                }

                                // the second 1070 code is the one that stores the block units,
                                // it will override the first 1070 that stores the Autodesk Design Center version number
                                if (data == null)
                                {
                                    break;  // premature end
                                }

                                if (data.Code == XDataCode.Int16)
                                {
                                    record.Units = (DrawingUnits)(short)data.Value;
                                }
                            }
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(pointerToLayout) && pointerToLayout != "0")
            {
                this.blockRecordPointerToLayout.Add(pointerToLayout, record);
            }

            return record;
        }

        private DimensionStyle ReadDimensionStyle()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.DimensionStyle);

            DimensionStyle defaultDim = DimensionStyle.Default;
            string name = string.Empty;
            List<XData> xData = new List<XData>();

            // dimension lines
            AciColor dimclrd = defaultDim.DimLineColor;
            string dimltype = string.Empty; // handle for post processing
            Lineweight dimlwd = defaultDim.DimLineLineweight;
            bool dimsd1 = defaultDim.DimLine1Off;
            bool dimsd2 = defaultDim.DimLine2Off;
            double dimdle = defaultDim.DimLineExtend;
            double dimdli = defaultDim.DimBaselineSpacing;

            // extension lines
            AciColor dimclre = defaultDim.ExtLineColor;
            string dimltex1 = string.Empty; // handle for post processing
            string dimltex2 = string.Empty; // handle for post processing
            Lineweight dimlwe = defaultDim.ExtLineLineweight;
            bool dimse1 = defaultDim.ExtLine1Off;
            bool dimse2 = defaultDim.ExtLine2Off;
            double dimexo = defaultDim.ExtLineOffset;
            double dimexe = defaultDim.ExtLineExtend;
            bool dimfxlon = defaultDim.ExtLineFixed;
            double dimfxl = defaultDim.ExtLineFixedLength;

            // symbols and arrows
            double dimasz = defaultDim.ArrowSize;
            double dimcen = defaultDim.CenterMarkSize;
            bool dimsah = false;
            string dimblk = string.Empty; // handle for post processing
            string dimblk1 = string.Empty; // handle for post processing
            string dimblk2 = string.Empty; // handle for post processing
            string dimldrblk = string.Empty; // handle for post processing
                
            // text
            string dimtxsty = string.Empty; // handle for post processing
            AciColor dimclrt = defaultDim.TextColor;
            short dimtfill = 0;
            AciColor dimtfillclrt = defaultDim.TextFillColor;
            double dimtxt = defaultDim.TextHeight;
            DimensionStyleTextVerticalPlacement dimtad = DimensionStyleTextVerticalPlacement.Centered;
            DimensionStyleTextHorizontalPlacement dimjust = DimensionStyleTextHorizontalPlacement.Centered;
            double dimgap = defaultDim.TextOffset;
            bool dimtih = defaultDim.TextInsideAlign;
            bool dimtoh = defaultDim.TextOutsideAlign;
            DimensionStyleTextDirection dimtxtdirection = defaultDim.TextDirection;
            double dimtfac = defaultDim.TextFractionHeightScale;

            // fit
            bool dimtofl = defaultDim.FitDimLineForce;
            bool dimsoxd = defaultDim.FitDimLineInside;
            double dimscale = defaultDim.DimScaleOverall;
            DimensionStyleFitOptions dimatfit = defaultDim.FitOptions;
            bool dimtix = defaultDim.FitTextInside;
            DimensionStyleFitTextMove dimtmove = defaultDim.FitTextMove;

            // primary units
            short dimadec = defaultDim.AngularPrecision;
            short dimdec = defaultDim.LengthPrecision;
            char dimdsep = defaultDim.DecimalSeparator;
            double dimlfac = defaultDim.DimScaleLinear;
            LinearUnitType dimlunit = defaultDim.DimLengthUnits;
            AngleUnitType dimaunit = defaultDim.DimAngularUnits;
            FractionFormatType dimfrac = defaultDim.FractionType;
            double dimrnd = defaultDim.DimRoundoff;

            string dimpost = string.Empty;
            short dimzin = 0;
            short dimazin = 3;

            // alternate units
            DimensionStyleAlternateUnits dimensionStyleAlternateUnits = new DimensionStyleAlternateUnits();
            string dimapost = string.Empty;
            short dimaltz = 0;

            // tolerances
            DimensionStyleTolerances tolerances = new DimensionStyleTolerances();
            short dimtol = 0;
            short dimlim = 0;
            short dimtzin = 0;
            short dimalttz = 0;

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 3:
                        dimpost = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 4:
                        dimapost = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 40:
                        dimscale = this.chunk.ReadDouble();
                        if (dimscale <= 0.0)
                        {
                            dimscale = defaultDim.DimScaleOverall;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        dimasz = this.chunk.ReadDouble();
                        if (dimasz < 0.0)
                        {
                            dimasz = defaultDim.ArrowSize;
                        }
                        this.chunk.Next();
                        break;
                    case 42:
                        dimexo = this.chunk.ReadDouble();
                        if (dimexo < 0.0)
                        {
                            dimexo = defaultDim.ExtLineOffset;
                        }
                        this.chunk.Next();
                        break;
                    case 43:
                        dimdli = this.chunk.ReadDouble();
                        if (dimdli < 0.0)
                        {
                            dimdli = defaultDim.DimBaselineSpacing;
                        }
                        this.chunk.Next();
                        break;
                    case 44:
                        dimexe = this.chunk.ReadDouble();
                        if (dimexe < 0.0)
                        {
                            dimexe = defaultDim.ExtLineExtend;
                        }
                        this.chunk.Next();
                        break;
                    case 45:
                        dimrnd = this.chunk.ReadDouble();
                        if (dimrnd < 0.000001 && !MathHelper.IsZero(dimrnd, double.Epsilon))
                        {
                            dimrnd = defaultDim.DimRoundoff;
                        }
                        this.chunk.Next();
                        break;
                    case 46:
                        dimdle = this.chunk.ReadDouble();
                        if (dimdle < 0.0)
                        {
                            dimdle = defaultDim.DimLineExtend;
                        }
                        this.chunk.Next();
                        break;
                    case 47:
                        tolerances.UpperLimit = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 48:
                        tolerances.LowerLimit = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 49:
                        dimfxl = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 69:
                        dimtfill = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 70:
                        dimtfillclrt = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 71:
                        dimtol = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        dimlim = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        dimtih = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 74:
                        dimtoh = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 75:
                        dimse1 = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 76:
                        dimse2 = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 77:
                        dimtad = (DimensionStyleTextVerticalPlacement) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 78:
                        dimzin = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 79:
                        dimazin = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 140:
                        dimtxt = this.chunk.ReadDouble();
                        if (dimtxt <= 0.0)
                        {
                            dimtxt = defaultDim.TextHeight;
                        }
                        this.chunk.Next();
                        break;
                    case 141:
                        dimcen = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 143:
                        double dimaltf = this.chunk.ReadDouble();
                        if (dimaltf <= 0.0)
                        {
                            dimaltf = defaultDim.AlternateUnits.Multiplier;
                        }
                        dimensionStyleAlternateUnits.Multiplier = dimaltf;
                        this.chunk.Next();
                        break;
                    case 144:
                        dimlfac = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(dimlfac))
                        {
                            dimlfac = defaultDim.DimScaleLinear;
                        }
                        this.chunk.Next();
                        break;
                    case 146:
                        dimtfac = this.chunk.ReadDouble();
                        if (dimtfac <= 0)
                        {
                            dimtfac = defaultDim.TextFractionHeightScale;
                        }
                        this.chunk.Next();
                        break;
                    case 147:
                        dimgap = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 148:
                        double dimaltrnd = this.chunk.ReadDouble();
                        if (dimaltrnd < 0.000001 && !MathHelper.IsZero(dimaltrnd, double.Epsilon))
                        {
                            dimaltrnd = defaultDim.AlternateUnits.Roundoff;
                        }
                        dimensionStyleAlternateUnits.Roundoff = dimaltrnd;
                        this.chunk.Next();
                        break;
                    case 170:
                        dimensionStyleAlternateUnits.Enabled = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 171:
                        short dimaltd = this.chunk.ReadShort();
                        if (dimaltd < 0)
                        {
                            dimaltd = defaultDim.AlternateUnits.LengthPrecision;
                        }
                        dimensionStyleAlternateUnits.LengthPrecision = dimaltd;
                        this.chunk.Next();
                        break;
                    case 172:
                        dimtofl = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 173:
                        dimsah = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 174:
                        dimtix = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 175:
                        dimsoxd = this.chunk.ReadShort() == 0;
                        this.chunk.Next();
                        break;
                    case 176:
                        dimclrd = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 177:
                        dimclre = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 178:
                        dimclrt = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 179:
                        dimadec = this.chunk.ReadShort();
                        if (dimadec < 0)
                        {
                            dimadec = defaultDim.AngularPrecision;
                        }
                        this.chunk.Next();
                        break;
                    case 271:
                        dimdec = this.chunk.ReadShort();
                        if (dimdec < 0)
                        {
                            dimdec = defaultDim.LengthPrecision;
                        }
                        this.chunk.Next();
                        break;
                    case 272:
                        short dimtdec = this.chunk.ReadShort();
                        if (dimtdec < 0)
                        {
                            dimtdec = defaultDim.Tolerances.Precision;
                        }
                        tolerances.Precision = dimtdec;
                        this.chunk.Next();
                        break;
                    case 273:
                        short dimaltu = this.chunk.ReadShort();
                        switch (dimaltu)
                        {
                            case 1:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Scientific;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                            case 2:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Decimal;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                            case 3:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Engineering;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                            case 4:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Architectural;
                                dimensionStyleAlternateUnits.StackUnits = true;
                                break;
                            case 5:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Fractional;
                                dimensionStyleAlternateUnits.StackUnits = true;
                                break;
                            case 6:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Architectural;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                            case 7:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Fractional;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                            default:
                                dimensionStyleAlternateUnits.LengthUnits = LinearUnitType.Scientific;
                                dimensionStyleAlternateUnits.StackUnits = false;
                                break;
                        }
                        this.chunk.Next();
                        break;
                    case 274:
                        short dimalttd = this.chunk.ReadShort();
                        if (dimalttd < 0)
                        {
                            dimalttd = defaultDim.Tolerances.AlternatePrecision;
                        }
                        tolerances.AlternatePrecision = dimalttd;
                        this.chunk.Next();
                        break;
                    case 275:
                        dimaunit = (AngleUnitType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 276:
                        dimfrac = (FractionFormatType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 277:
                        dimlunit = (LinearUnitType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 278:
                        dimdsep = (char) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 279:
                        dimtmove = (DimensionStyleFitTextMove) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 280:
                        dimjust = (DimensionStyleTextHorizontalPlacement) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 281:
                        dimsd1 = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 282:
                        dimsd2 = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 283:
                        tolerances.VerticalPlacement = (DimensionStyleTolerancesVerticalPlacement) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 284:
                        dimtzin = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 285:
                        dimaltz = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 286:
                        dimalttz = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 289:
                        dimatfit = (DimensionStyleFitOptions) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 290:
                        dimfxlon = this.chunk.ReadBool();
                        this.chunk.Next();
                        break;
                    case 294:
                        dimtxtdirection = this.chunk.ReadBool() ? DimensionStyleTextDirection.RightToLeft : DimensionStyleTextDirection.LeftToRight;
                        this.chunk.Next();
                        break;
                    case 340:
                        dimtxsty = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 341:
                        dimldrblk = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 342:
                        dimblk = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 343:
                        dimblk1 = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 344:
                        dimblk2 = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 345:
                        dimltype = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 346:
                        dimltex1 = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 347:
                        dimltex2 = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 371:
                        dimlwd = (Lineweight) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 372:
                        dimlwe = (Lineweight) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string id = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(id));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            DimensionStyle style = new DimensionStyle(name, false)
            {
                // dimension lines
                DimLineColor = dimclrd,
                DimLineLineweight = dimlwd,
                DimLine1Off = dimsd1,
                DimLine2Off = dimsd2,
                DimBaselineSpacing = dimdli,
                DimLineExtend = dimdle,

                // extension lines
                ExtLineColor = dimclre,
                ExtLineLineweight = dimlwe,
                ExtLine1Off = dimse1,
                ExtLine2Off = dimse2,
                ExtLineOffset = dimexo,
                ExtLineExtend = dimexe,
                ExtLineFixed = dimfxlon,
                ExtLineFixedLength = dimfxl,

                // symbols and arrows
                ArrowSize = dimasz,
                CenterMarkSize = dimcen,

                // text
                TextHeight = dimtxt,
                TextColor = dimclrt,
                TextFillColor = dimtfill == 2 ? dimtfillclrt : null,
                TextVerticalPlacement = dimtad,
                TextHorizontalPlacement = dimjust,
                TextOffset = dimgap,
                TextInsideAlign = dimtih,
                TextOutsideAlign = dimtoh,
                TextDirection = dimtxtdirection,
                TextFractionHeightScale = dimtfac,

                // fit
                FitDimLineForce = dimtofl,
                FitDimLineInside = dimsoxd,
                DimScaleOverall = dimscale,
                FitOptions = dimatfit,
                FitTextInside = dimtix,
                FitTextMove = dimtmove,

                //primary units
                AngularPrecision = dimadec,
                LengthPrecision = dimdec,
                DecimalSeparator = dimdsep,
                DimScaleLinear = dimlfac,
                DimLengthUnits = dimlunit,
                DimAngularUnits = dimaunit,
                FractionType = dimfrac,
                DimRoundoff = dimrnd,

                // alternate units
                AlternateUnits = dimensionStyleAlternateUnits,

                // tolerances
                Tolerances = tolerances
            };

            // suppress angular leading and/or trailing zeros
            if (dimazin == 1)
            {
                style.SuppressAngularLeadingZeros = true;
                style.SuppressAngularTrailingZeros = false;
            }
            else if (dimazin == 2)
            {
                style.SuppressAngularLeadingZeros = false;
                style.SuppressAngularTrailingZeros = true;
            }
            else if (dimazin == 3)
            {
                style.SuppressAngularLeadingZeros = true;
                style.SuppressAngularTrailingZeros = true;
            }
            else
            {
                style.SuppressAngularLeadingZeros = false;
                style.SuppressAngularTrailingZeros = false;
            }

            bool[] suppress = GetLinearZeroesSuppression(dimzin);
            style.SuppressLinearLeadingZeros = suppress[0];
            style.SuppressLinearTrailingZeros = suppress[1];
            style.SuppressZeroFeet = suppress[2];
            style.SuppressZeroInches = suppress[3];

            suppress = GetLinearZeroesSuppression(dimaltz);
            style.AlternateUnits.SuppressLinearLeadingZeros = suppress[0];
            style.AlternateUnits.SuppressLinearTrailingZeros = suppress[1];
            style.AlternateUnits.SuppressZeroFeet = suppress[2];
            style.AlternateUnits.SuppressZeroInches = suppress[3];

            suppress = GetLinearZeroesSuppression(dimtzin);
            style.Tolerances.SuppressLinearLeadingZeros = suppress[0];
            style.Tolerances.SuppressLinearTrailingZeros = suppress[1];
            style.Tolerances.SuppressZeroFeet = suppress[2];
            style.Tolerances.SuppressZeroInches = suppress[3];

            suppress = GetLinearZeroesSuppression(dimalttz);
            style.Tolerances.AlternateSuppressLinearLeadingZeros = suppress[0];
            style.Tolerances.AlternateSuppressLinearTrailingZeros = suppress[1];
            style.Tolerances.AlternateSuppressZeroFeet = suppress[2];
            style.Tolerances.AlternateSuppressZeroInches = suppress[3];

            if (dimtol == 0 && dimlim == 0)
            {
                style.Tolerances.DisplayMethod =  DimensionStyleTolerancesDisplayMethod.None;
            }
            if (dimtol == 1 && dimlim == 0)
            {
                style.Tolerances.DisplayMethod =
                    MathHelper.IsEqual(style.Tolerances.UpperLimit, style.Tolerances.LowerLimit) ?
                    DimensionStyleTolerancesDisplayMethod.Symmetrical : DimensionStyleTolerancesDisplayMethod.Deviation;
            }
            if (dimtol == 0 && dimlim == 1)
            {
                style.Tolerances.DisplayMethod = DimensionStyleTolerancesDisplayMethod.Limits;
            }

            string[] textPrefixSuffix = GetDimStylePrefixAndSuffix(dimpost, '<', '>');
            style.DimPrefix = textPrefixSuffix[0];
            style.DimSuffix = textPrefixSuffix[1];

            textPrefixSuffix = GetDimStylePrefixAndSuffix(dimapost, '[', ']');
            style.AlternateUnits.Prefix = textPrefixSuffix[0];
            style.AlternateUnits.Suffix = textPrefixSuffix[1];

            if (xData.Count > 0) this.tableEntryXData.Add(style, xData);

            // store information for post processing. The blocks, text styles, and line types definitions might appear after the dimension style
            if (!dimsah)
            {
                dimblk1 = dimblk;
                dimblk2 = dimblk;
            }
            string[] handles = {dimblk1, dimblk2, dimldrblk, dimtxsty, dimltype, dimltex1, dimltex2};
            this.dimStyleToHandles.Add(style, handles);

            return style;
        }

        private static bool[] GetLinearZeroesSuppression(short token)
        {
            bool[] suppress = new bool[4]; // leading, trailing, feet inches

            // suppress leading and/or trailing zeros 
            if (12 - token <= 0)
            {
                suppress[0] = true;
                suppress[1] = true;
                token -= 12;
            }
            else if (8 - token <= 0)
            {
                suppress[0] = false;
                suppress[1] = true;
                token -= 8;
            }
            else if (4 - token <= 0)
            {
                suppress[0] = true;
                suppress[1] = false;
                token -= 4;
            }
            else
            {
                suppress[0] = false;
                suppress[1] = false;
            }

            // suppress feet and/or inches
            switch (token)
            {
                case 0:
                    suppress[2] = true;
                    suppress[3] = true;
                    break;
                case 1:
                    suppress[2] = false;
                    suppress[3] = false;
                    break;
                case 2:
                    suppress[2] = false;
                    suppress[3] = true;
                    break;
                case 3:
                    suppress[2] = true;
                    suppress[3] = false;
                    break;
                default:
                    suppress[2] = true;
                    suppress[3] = true;
                    break;
            }
            return suppress;
        }

        private static string[] GetDimStylePrefixAndSuffix(string text, char start, char end)
        {
            int index = -1; // first occurrence of '<>' or '[]'
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == start)
                {
                    if (i + 1 < text.Length)
                    {
                        if (text[i + 1] == end)
                        {
                            index = i;
                            break;
                        }
                    }
                }
            }

            string prefix;
            string suffix;
            if (index < 0)
            {
                prefix = string.Empty;
                suffix = text;
            }
            else
            {
                prefix = text.Substring(0, index);
                suffix = text.Substring(index + 2, text.Length - (index + 2));
            }

            return new[] {prefix, suffix};
        }

        private Layer ReadLayer()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.Layer);

            string name = string.Empty;
            bool isVisible = true;
            bool plot = true;
            AciColor color = AciColor.Default;
            Linetype linetype = Linetype.ByLayer;
            Lineweight lineweight = Lineweight.Default;
            LayerFlags flags = LayerFlags.None;
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (LayerFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 62:
                        short index = this.chunk.ReadShort();
                        if (index < 0)
                        {
                            isVisible = false;
                            index = Math.Abs(index);
                        }
                        if (!color.UseTrueColor)
                        {
                            color = AciColor.FromCadIndex(index);
                        }
                        // layer color cannot be ByLayer or ByBlock
                        if (color.IsByLayer || color.IsByBlock)
                        {
                            color = AciColor.Default;
                        }
                        this.chunk.Next();
                        break;
                    case 420: // the layer uses true color
                        color = AciColor.FromTrueColor(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 6:
                        string linetypeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        linetype = this.GetLinetype(linetypeName);
                        // layer linetype cannot be ByLayer or ByBlock
                        if (linetype.IsByLayer || linetype.IsByBlock)
                        {
                            linetype = Linetype.Continuous;
                        }
                        this.chunk.Next();
                        break;
                    case 290:
                        plot = this.chunk.ReadBool();
                        this.chunk.Next();
                        break;
                    case 370:
                        lineweight = (Lineweight) this.chunk.ReadShort();
                        // layer lineweight cannot be ByLayer or ByBlock
                        if (lineweight == Lineweight.ByLayer || lineweight == Lineweight.ByBlock)
                        {
                            lineweight = Lineweight.Default;
                        }
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(name), string.Format("Table object name \"{0}\" is not valid.", name));
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            Layer layer = new Layer(name, false)
            {
                Color = color,
                Linetype = linetype,
                IsVisible = isVisible,
                IsFrozen = flags.HasFlag(LayerFlags.Frozen),
                IsLocked = flags.HasFlag(LayerFlags.Locked),
                Plot = plot,
                Lineweight = lineweight
            };

            if (xData.Count > 0)
            {
                this.tableEntryXData.Add(layer, xData);
            }

            return layer;
        }

        private Linetype ReadLinetype(out bool isComplex)
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.Linetype);

            isComplex = false;
            string name = null;
            string description = null;
            List<LinetypeSegment> segments = new List<LinetypeSegment>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2: // line type name is case insensitive
                        name = this.chunk.ReadString();
                        if (string.Equals(name, Linetype.ByLayerName, StringComparison.OrdinalIgnoreCase))
                        {
                            name = Linetype.ByLayerName;
                        }
                        else if (string.Equals(name, Linetype.ByBlockName, StringComparison.OrdinalIgnoreCase))
                        {
                            name = Linetype.ByBlockName;
                        }
                        name = this.DecodeEncodedNonAsciiCharacters(name);
                        this.chunk.Next();
                        break;
                    case 3: // line type description
                        description = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 73:
                        //number of segments (not needed)
                        this.chunk.Next();
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        this.chunk.Next();
                        break;
                    case 49:
                        // read linetype segments multiple entries
                        double length = this.chunk.ReadDouble();
                        // code 49 should be followed by code 74 that defines if the linetype segment is simple, text or shape
                        this.chunk.Next();
                        Debug.Assert(this.chunk.Code == 74, "Bad formatted linetype data.");
                        short type = this.chunk.ReadShort();
                        this.chunk.Next();

                        LinetypeSegment segment;
                        if (type == 0)
                        {
                            segment = new LinetypeSimpleSegment(length);
                        }
                        else
                        {
                            isComplex = true;
                            segment = this.ReadLinetypeComplexSegment(type, length);
                        }
                        if(segment != null) segments.Add(segment);

                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            Linetype linetype = new Linetype(name, segments, description, false);
            if (xData.Count > 0)
            {
                this.tableEntryXData.Add(linetype, xData);
            }
            return linetype;
        }

        private LinetypeSegment ReadLinetypeComplexSegment(int type, double length)
        {
            string text = string.Empty;
            short shapeNumber = 0;
            string handleToStyle = string.Empty;
            Vector2 offset = Vector2.Zero;
            double rotation = 0.0;
            double scale = 0.0;

            // read until a new linetype segment is found or the end of the linetype definition
            while (this.chunk.Code != 49 && this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 75:
                        shapeNumber = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 340:
                        handleToStyle = this.chunk.ReadString();
                        this.chunk.Next();
                        break;
                    case 46:
                        scale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 44:
                        offset.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 45:
                        offset.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 9:
                        text = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            if (string.IsNullOrEmpty(handleToStyle))
            {
                return null;
            }

            LinetypeSegment segment;
            LinetypeSegmentRotationType rt = (type & 1) == 1 ? LinetypeSegmentRotationType.Absolute : LinetypeSegmentRotationType.Relative;
            if ((type & 2) == 2)
            {
                segment = new LinetypeTextSegment(text, TextStyle.Default, length, offset, rt, rotation, scale);
            }
            else if ((type & 4) == 4)
            {
                segment = new LinetypeShapeSegment("NOSHAPE", ShapeStyle.Default, length, offset, rt, rotation, scale);
                this.linetypeShapeSegmentToNumber.Add((LinetypeShapeSegment) segment, shapeNumber);
            }
            else
            {
                return null;
            }

            this.linetypeSegmentStyleHandles.Add(segment, handleToStyle);
            
            return segment;
        }

        private DxfObject ReadTextStyle()
        {
            // this method will read both text and shape styles their definitions appear in the same table list
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.TextStyle);

            string name = string.Empty;
            string file = string.Empty;
            string bigFont = string.Empty;
            bool isVertical = false;
            bool isBackward = false;
            bool isUpsideDown = false;
            double height = 0.0f;
            double widthFactor = 0.0f;
            double obliqueAngle = 0.0f;
            bool isShapeStyle = false;
            XData xDataFont = null;
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 3:
                        file = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 4:
                        bigFont = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        int flag = this.chunk.ReadShort();
                        if ((flag & 1) == 1)
                        {
                            isShapeStyle = true;
                        }

                        if ((flag & 4) == 4)
                        {
                            isVertical = true;
                        }
                        this.chunk.Next();
                        break;
                    case 71:
                        int upDownBack = this.chunk.ReadShort();
                        if (upDownBack == 6)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        else if (upDownBack == 2)
                        {
                            isBackward = true;
                        }
                        else if (upDownBack == 4)
                        {
                            isUpsideDown = true;
                        }
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        if (height < 0.0)
                        {
                            height = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        widthFactor = this.chunk.ReadDouble();
                        if (widthFactor < 0.01 || widthFactor > 100.0)
                        {
                            widthFactor = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 42:
                        //last text height used (not applicable)
                        this.chunk.Next();
                        break;
                    case 50:
                        obliqueAngle = this.chunk.ReadDouble();
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                        {
                            obliqueAngle = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        if (string.Equals(appId, ApplicationRegistry.DefaultName))
                        {
                            xDataFont = data;
                        }
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // shape styles are handle in a separate list
            if (isShapeStyle)
            {
                // file cannot be null or empty
                Debug.Assert(!string.IsNullOrEmpty(file), "File path is null or empty.");
                if (string.IsNullOrEmpty(file))
                {
                    file = FileNotValid + ".SHX";
                }

                // basic check if file is a file path
                Debug.Assert(file.IndexOfAny(Path.GetInvalidPathChars()) == -1, "File path contains invalid characters: " + file);
                if (file.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    file = FileNotValid + ".SHX";
                }

                //ShapeStyle shapeStyle = new ShapeStyle(Path.GetFileNameWithoutExtension(file), file, height, widthFactor, obliqueAngle);
                ShapeStyle shapeStyle = new ShapeStyle("ShapeStyle - " + ++this.shapeStyleCounter, file, height, widthFactor, obliqueAngle);
                if (xData.Count > 0)
                {
                    this.tableEntryXData.Add(shapeStyle, xData);
                }
                return shapeStyle;
            }

            // text styles
            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            // if it exists read the information stored in the extended data about the font family and font style, only applicable to true type fonts
            string fontFamily = string.Empty;
            FontStyle fontStyle = FontStyle.Regular;
            if (xDataFont != null)
            {
                foreach (XDataRecord record in xDataFont.XDataRecord)
                {
                    if (record.Code == XDataCode.String)
                    {
                        fontFamily = (string)record.Value;
                    }
                    else if (record.Code == XDataCode.Int32)
                    {
                        byte[] data = BitConverter.GetBytes((int) record.Value);
                        fontStyle = (FontStyle) data[3];
                    }
                }
            }

            TextStyle style;

            // basic check if file is a file path
            Debug.Assert(file.IndexOfAny(Path.GetInvalidPathChars()) == -1, "File path contains invalid characters.");
            if (file.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                file = FileNotValid + ".SHX";
            }

            // the information stored in the extended data takes precedence before the font file (this is only applicable for true type fonts)
            if (string.IsNullOrEmpty(fontFamily))
            {
                Debug.Assert(!string.IsNullOrEmpty(file), "File path is null or empty.");
                if (string.IsNullOrEmpty(file))
                {
                    file = "simplex.SHX";
                }
                else
                {
                    if (string.IsNullOrEmpty(Path.GetExtension(file)))
                    {
                        // if there is no extension the default SHX will be used instead
                        file += ".SHX";
                    }
                    else if (!Path.GetExtension(file).Equals(".TTF", StringComparison.InvariantCultureIgnoreCase) &&
                             !Path.GetExtension(file).Equals(".SHX", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // only true type TTF fonts or compiled shape SHX fonts are allowed, the default "simplex.shx" font will be used in this case
                        file = "simplex.SHX";
                    }
                }

                style = new TextStyle(name, file, false)
                {
                    Height = height,
                    IsBackward = isBackward,
                    IsUpsideDown = isUpsideDown,
                    IsVertical = isVertical,
                    ObliqueAngle = obliqueAngle,
                    WidthFactor = widthFactor,
                };

                if (Path.GetExtension(file).Equals(".SHX", StringComparison.InvariantCultureIgnoreCase) &&
                    Path.GetExtension(bigFont).Equals(".SHX", StringComparison.InvariantCultureIgnoreCase))
                {
                    style.BigFont = bigFont;
                }
            }
            else
            {
                style = new TextStyle(name, fontFamily, fontStyle, false)
                {
                    Height = height,
                    IsBackward = isBackward,
                    IsUpsideDown = isUpsideDown,
                    IsVertical = isVertical,
                    ObliqueAngle = obliqueAngle,
                    WidthFactor = widthFactor
                };
            }

            if (xData.Count > 0)
            {
                this.tableEntryXData.Add(style, xData);
            }

            return style;
        }

        private UCS ReadUCS()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.Ucs);

            string name = string.Empty;
            Vector3 origin = Vector3.Zero;
            Vector3 xDir = Vector3.UnitX;
            Vector3 yDir = Vector3.UnitY;
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 10:
                        origin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        origin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        origin.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        xDir.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        xDir.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        xDir.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        yDir.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        yDir.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        yDir.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 146:
                        //elevation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            UCS ucs = new UCS(name, origin, xDir, yDir, false);
            if (xData.Count > 0)
            {
                this.tableEntryXData.Add(ucs, xData);
            }
            return ucs;
        }

        private View ReadView()
        {
            // placeholder method for view table objects
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.View);

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                this.chunk.Next();
            }

            return null;
        }

        private VPort ReadVPort()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.VPort);

            string name = string.Empty;
            Vector2 center = Vector2.Zero;
            Vector2 snapBasePoint = Vector2.Zero;
            Vector2 snapSpacing = new Vector2(0.5);
            Vector2 gridSpacing = new Vector2(10.0);
            Vector3 target = Vector3.Zero;
            Vector3 direction = Vector3.UnitZ;
            double height = 10.0;
            double ratio = 1.0;
            bool showGrid = true;
            bool snapMode = false;
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 12:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        snapBasePoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        snapBasePoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        snapSpacing.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        snapSpacing.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        gridSpacing.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        gridSpacing.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 16:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 26:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 36:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 17:
                        target.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 27:
                        target.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 37:
                        target.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        ratio = this.chunk.ReadDouble();
                        if (ratio <= 0)
                        {
                            ratio = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 75:
                        snapMode = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 76:
                        showGrid = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (name.Equals(VPort.DefaultName, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            VPort vport = new VPort(name, false)
            {
                ViewCenter = center,
                SnapBasePoint = snapBasePoint,
                SnapSpacing = snapSpacing,
                GridSpacing = gridSpacing,
                ViewTarget = target,
                ViewDirection = direction,
                ViewHeight = height,
                ViewAspectRatio = ratio,
                ShowGrid = showGrid,
                SnapMode = snapMode,
            };

            if (xData.Count > 0)
            {
                this.tableEntryXData.Add(vport, xData);
            }

            return vport;
        }

        #endregion

        #region block methods

        private Block ReadBlock()
        {
            Debug.Assert(this.chunk.ReadString() == DxfObjectCode.BeginBlock);

            Layer layer = Layer.Default;
            string name = string.Empty;
            string description = String.Empty;
            string handle = string.Empty;
            string xrefFile = string.Empty;
            BlockTypeFlags type = BlockTypeFlags.None;
            Vector3 basePoint = Vector3.Zero;
            List<EntityObject> entities = new List<EntityObject>();
            List<AttributeDefinition> attDefs = new List<AttributeDefinition>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        xrefFile = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 4:
                        description = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 8:
                        string layerName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        layer = this.GetLayer(layerName);
                        this.chunk.Next();
                        break;
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        type = (BlockTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 10:
                        basePoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 3:
                        //I don't know the reason of these duplicity since code 2 also contains the block name
                        //The program EASE exports code 3 with an empty string (use it or don't use it but do NOT mix information)
                        //name = dxfPairInfo.Value;
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (this.chunk.Code >= 1000 && this.chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }
                        this.chunk.Next();
                        break;
                }
            }

            // read block entities
            while (this.chunk.ReadString() != DxfObjectCode.EndBlock)
            {
                DxfObject dxfObject = this.ReadEntity(true);
                if (dxfObject != null)
                {
                    if (dxfObject is AttributeDefinition attDef)
                    {
                        attDefs.Add(attDef);
                    }

                    if (dxfObject is EntityObject entity)
                    {
                        entities.Add(entity);
                    }
                }
            }

            // read the end block object until a new element is found
            this.chunk.Next();
            string endBlockHandle = string.Empty;
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        endBlockHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 8:
                        // the EndBlock layer and the Block layer are the same
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            // Workaround for Blocks without its associated BLOCK_RECORD, this may end up in a loose of information
            this.blockRecords.TryGetValue(name, out BlockRecord blockRecord);
            if (blockRecord == null)
            {
                Debug.Assert(false, string.Format("The block record {0} is not defined.", name));
                blockRecord = new BlockRecord(name);
                this.doc.NumHandles = blockRecord.AssignHandle(this.doc.NumHandles);
            }

            //if (!this.blockRecords.TryGetValue(name, out BlockRecord blockRecord))
            //{
            //    throw new Exception(string.Format("The block record {0} is not defined.", name));
            //}

            Block block;

            if (type.HasFlag(BlockTypeFlags.XRef))
            {
                Debug.Assert(!string.IsNullOrEmpty(xrefFile), "File path is null or empty.");
                if (string.IsNullOrEmpty(xrefFile))
                {
                    xrefFile = FileNotValid;
                }

                Debug.Assert(xrefFile.IndexOfAny(Path.GetInvalidPathChars()) == -1, "File path contains invalid characters: " + xrefFile);
                if (xrefFile.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    xrefFile = FileNotValid;
                }

                block = new Block(name, xrefFile)
                {
                    Handle = handle,
                    Description = description,
                    Owner = blockRecord,
                    Origin = basePoint,
                    Layer = layer,
                    Flags = type,
                };
            }
            else
            {
                block = new Block(name, null, null, false)
                {
                    Handle = handle,
                    Description = description,
                    Owner = blockRecord,
                    Origin = basePoint,
                    Layer = layer,
                    Flags = type,
                };
            }

            block.End.Handle = endBlockHandle;
            block.XData.AddRange(xData);

            if (name.StartsWith(Block.DefaultPaperSpaceName, StringComparison.OrdinalIgnoreCase))
            {
                // the DXF is not consistent with the way they handle entities that belong to different paper spaces.
                // While the entities of *Paper_Space block are stored in the ENTITIES section as the *Model_Space,
                // the list of entities in *Paper_Space# are stored in the block definition itself.
                // As all this entities do not need an insert entity to have a visual representation,
                // they will be stored in the global entities lists together with the rest of the entities of *Model_Space and *Paper_Space
                foreach (EntityObject entity in entities)
                {
                    this.entityList.Add(entity, blockRecord.Handle);
                }

                // this kind of blocks do not store attribute definitions
            }
            else
            {
                // add attribute definitions
                foreach (AttributeDefinition attDef in attDefs)
                {
                    // AutoCAD allows duplicate tags in attribute definitions, but this library does not having duplicate tags is not recommended in any way,
                    // since there will be no way to know which is the definition associated to the insert attribute
                    if (!block.AttributeDefinitions.ContainsTag(attDef.Tag))
                    {
                        block.AttributeDefinitions.Add(attDef);
                    }
                }
                // block entities for post processing (MLines and Images references other objects (MLineStyle and ImageDefinition) that will be defined later
                this.blockEntities.Add(block, entities);
            }

            return block;
        }

        private AttributeDefinition ReadAttributeDefinition()
        {
            string tag = string.Empty;
            string text = string.Empty;
            string value = string.Empty;
            AttributeFlags flags = AttributeFlags.None;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            TextStyle style = TextStyle.Default;
            double height = 0.0;
            double width = 1.0;
            double widthFactor = 0.0;
            short horizontalAlignment = 0;
            short verticalAlignment = 0;
            double rotation = 0.0;
            double obliqueAngle = 0.0;
            bool isBackward = false;
            bool isUpsideDown = false;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        value = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 2:
                        tag = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 3:
                        text = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (AttributeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 7:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        style = this.GetTextStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        widthFactor = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = MathHelper.NormalizeAngle(this.chunk.ReadDouble());
                        if (obliqueAngle > 180)
                        {
                            obliqueAngle -= 360;
                        }
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                        {
                            obliqueAngle = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 71:
                        short textGeneration = this.chunk.ReadShort();
                        if (textGeneration - 2 == 0)
                        {
                            isBackward = true;
                        }
                        if (textGeneration - 4 == 0)
                        {
                            isUpsideDown = true;
                        }
                        if (textGeneration - 6 == 0)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        this.chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 74:
                        verticalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            switch (alignment)
            {
                case TextAlignment.BaselineLeft:
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                case TextAlignment.Fit:
                case TextAlignment.Aligned:
                    width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                    if (width <= 0.0)
                    {
                        width = 1.0;
                    }
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                default:
                    ocsBasePoint = secondAlignmentPoint;
                    break;
            }

            AttributeDefinition attDef = new AttributeDefinition(tag)
            {
                Position = MathHelper.Transform(ocsBasePoint, normal, CoordinateSystem.Object, CoordinateSystem.World),
                Normal = normal,
                Alignment = alignment,
                Prompt = text,
                Value = value,
                Flags = flags,
                Style = style,
                Height = height,
                Width = width,
                WidthFactor = MathHelper.IsZero(widthFactor) ? style.WidthFactor : widthFactor,
                ObliqueAngle = obliqueAngle,
                Rotation = rotation,
                IsBackward = isBackward,
                IsUpsideDown = isUpsideDown
            };

            attDef.XData.AddRange(xData);
            return attDef;
        }

        private Attribute ReadAttribute(Block block, bool isBlockEntity = false)
        {
            string handle = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            Linetype linetype = Linetype.ByLayer;
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;
            bool isVisible = true;
            Transparency transparency = Transparency.ByLayer;

            AttributeFlags flags = AttributeFlags.None;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            TextStyle style = TextStyle.Default;
            double height = 0.0;
            double width = 1.0;
            double widthFactor = 0.0;
            double obliqueAngle = 0.0;
            short horizontalAlignment = 0;
            short verticalAlignment = 0;
            double rotation = 0.0;
            bool isBackward = false;
            bool isUpsideDown = false;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            // DxfObject codes
            this.chunk.Next();
            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", DxfObjectCode.Attribute));
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            // AcDbEntity common codes
            this.chunk.Next();
            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", DxfObjectCode.Attribute));
                    case 8: // layer code
                        string layerName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        layer = this.GetLayer(layerName);
                        this.chunk.Next();
                        break;
                    case 62: // ACI color code
                        if (!color.UseTrueColor)
                        {
                            color = AciColor.FromCadIndex(this.chunk.ReadShort());
                        }
                        this.chunk.Next();
                        break;
                    case 440: //transparency
                        transparency = Transparency.FromAlphaValue(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 420: // the entity uses true color
                        color = AciColor.FromTrueColor(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 6: // type line code
                        string linetypeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        linetype = this.GetLinetype(linetypeName);
                        this.chunk.Next();
                        break;
                    case 370: // line weight code
                        lineweight = (Lineweight) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 48: // line type scale
                        linetypeScale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 60: //object visibility
                        isVisible = this.chunk.ReadShort() == 0;
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            AttributeDefinition attDef = null;
            string attTag = string.Empty;
            string value = string.Empty;

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        attTag = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        // seems that some programs may export insert entities with attributes which definitions are not defined in the block
                        // if it is not present the insert attribute will have a null definition
                        if (!isBlockEntity)
                        {
                            block.AttributeDefinitions.TryGetValue(attTag, out attDef);
                        }
                        this.chunk.Next();
                        break;
                    case 1:
                        value = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (AttributeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 7:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        style = this.GetTextStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        widthFactor = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = MathHelper.NormalizeAngle(this.chunk.ReadDouble());
                        if (obliqueAngle > 180)
                        {
                            obliqueAngle -= 360;
                        }
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                        {
                            obliqueAngle = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 71:
                        short textGeneration = this.chunk.ReadShort();
                        if (textGeneration - 2 == 0)
                        {
                            isBackward = true;
                        }
                        if (textGeneration - 4 == 0)
                        {
                            isUpsideDown = true;
                        }
                        if (textGeneration - 6 == 0)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        this.chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 74:
                        verticalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            switch (alignment)
            {
                case TextAlignment.BaselineLeft:
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                case TextAlignment.Fit:
                case TextAlignment.Aligned:
                    width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                    if (width <= 0.0)
                    {
                        width = 1.0;
                    }
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                default:
                    ocsBasePoint = secondAlignmentPoint;
                    break;
            }

            Debug.Assert(!string.IsNullOrEmpty(attTag), "The attribute tag cannot be null or empty.");
            if (string.IsNullOrEmpty(attTag))
            {
                return null;
            }

            Attribute attribute = new Attribute(attTag)
            {
                Handle = handle,
                Color = color,
                Layer = layer,
                Linetype = linetype,
                Lineweight = lineweight,
                LinetypeScale = linetypeScale,
                Transparency = transparency,
                IsVisible = isVisible,
                Definition = attDef,
                Position = MathHelper.Transform(ocsBasePoint, normal, CoordinateSystem.Object, CoordinateSystem.World),
                Normal = normal,
                Alignment = alignment,
                Value = value,
                Flags = flags,
                Style = style,
                Height = height,
                Width = width,
                WidthFactor = MathHelper.IsZero(widthFactor) ? style.WidthFactor : widthFactor,
                ObliqueAngle = obliqueAngle,
                Rotation = rotation,
                IsBackward = isBackward,
                IsUpsideDown = isUpsideDown
            };

            attribute.XData.AddRange(xData);

            return attribute;
        }

        #endregion

        #region entity methods

        private DxfObject ReadEntity(bool isBlockEntity)
        {
            string handle = null;
            string owner = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            Linetype linetype = Linetype.ByLayer;
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;
            bool isVisible = true;
            Transparency transparency = Transparency.ByLayer;

            DxfObject dxfObject;

            string dxfCode = this.chunk.ReadString();
            this.chunk.Next();

            // DxfObject common codes
            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", dxfCode));
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.chunk.Next();
                        break;
                    case 330:
                        owner = this.chunk.ReadHex();
                        if (owner == "0")
                        {
                            owner = null;
                        }
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            // AcDbEntity common codes
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.Entity);
            this.chunk.Next();
            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", dxfCode));
                    case 8: //layer code
                        string layerName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        layer = this.GetLayer(layerName);
                        this.chunk.Next();
                        break;
                    case 62: //ACI color code
                        if (!color.UseTrueColor)
                        {
                            color = AciColor.FromCadIndex(this.chunk.ReadShort());
                        }
                        this.chunk.Next();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 440: //transparency
                        transparency = Transparency.FromAlphaValue(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 6: //type line code
                        string linetypeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        linetype = this.GetLinetype(linetypeName);
                        this.chunk.Next();
                        break;
                    case 370: //line weight code
                        lineweight = (Lineweight) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 48: //line type scale
                        linetypeScale = this.chunk.ReadDouble();
                        if (linetypeScale <= 0.0)
                        {
                            linetypeScale = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 60: //object visibility
                        isVisible = this.chunk.ReadShort() == 0;
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            switch (dxfCode)
            {
                case DxfObjectCode.Arc:
                    dxfObject = this.ReadArc();
                    break;
                case DxfObjectCode.AttributeDefinition:
                    dxfObject = this.ReadAttributeDefinition();
                    break;
                case DxfObjectCode.Circle:
                    dxfObject = this.ReadCircle();
                    break;
                case DxfObjectCode.Dimension:
                    dxfObject = this.ReadDimension(isBlockEntity);
                    break;
                case DxfObjectCode.ArcDimension:
                    dxfObject = this.ReadDimension(isBlockEntity);
                    break;
                case DxfObjectCode.Ellipse:
                    dxfObject = this.ReadEllipse();
                    break;
                case DxfObjectCode.Face3d:
                    dxfObject = this.ReadFace3d();
                    break;
                case DxfObjectCode.Hatch:
                    dxfObject = this.ReadHatch();
                    break;
                case DxfObjectCode.Image:
                    dxfObject = this.ReadImage();
                    break;
                case DxfObjectCode.Insert:
                    dxfObject = this.ReadInsert(isBlockEntity);
                    break;
                case DxfObjectCode.Leader:
                    dxfObject = this.ReadLeader();
                    break;
                case DxfObjectCode.Line:
                    dxfObject = this.ReadLine();
                    break;
                case DxfObjectCode.LwPolyline:
                    dxfObject = this.ReadLwPolyline();
                    break;
                case DxfObjectCode.Mesh:
                    dxfObject = this.ReadMesh();
                    break;
                case DxfObjectCode.MLine:
                    dxfObject = this.ReadMLine();
                    break;
                case DxfObjectCode.MText:
                    dxfObject = this.ReadMText();
                    break;
                case DxfObjectCode.Point:
                    dxfObject = this.ReadPoint();
                    break;
                case DxfObjectCode.Polyline:
                    dxfObject = this.ReadPolyline();
                    break;
                case DxfObjectCode.Ray:
                    dxfObject = this.ReadRay();
                    break;
                case DxfObjectCode.Text:
                    dxfObject = this.ReadText();
                    break;
                case DxfObjectCode.Tolerance:
                    dxfObject = this.ReadTolerance();
                    break;
                case DxfObjectCode.Trace:
                    dxfObject = this.ReadTrace();
                    break;
                case DxfObjectCode.Shape:
                    dxfObject = this.ReadShape();
                    break;
                case DxfObjectCode.Solid:
                    dxfObject = this.ReadSolid();
                    break;
                case DxfObjectCode.Spline:
                    dxfObject = this.ReadSpline();
                    break;
                case DxfObjectCode.UnderlayDgn:
                    dxfObject = this.ReadUnderlay();
                    break;
                case DxfObjectCode.UnderlayDwf:
                    dxfObject = this.ReadUnderlay();
                    break;
                case DxfObjectCode.UnderlayPdf:
                    dxfObject = this.ReadUnderlay();
                    break;
                case DxfObjectCode.Viewport:
                    dxfObject = this.ReadViewport();
                    break;
                case DxfObjectCode.XLine:
                    dxfObject = this.ReadXLine();
                    break;
                case DxfObjectCode.Wipeout:
                    dxfObject = this.ReadWipeout();
                    break;
                case DxfObjectCode.AcadTable:
                    dxfObject = this.ReadAcadTable(isBlockEntity);
                    break;
                default:
                    this.ReadUnknowData();
                    return null;
            }

            //if (dxfObject == null || string.IsNullOrEmpty(handle))
            //{
            //    return null;
            //}

            if (dxfObject == null)
            {
                return null;
            }

            //if (string.IsNullOrEmpty(handle))
            //{
            //    Debug.Assert(false, "Entity without handle.");
            //    this.doc.NumHandles = dxfObject.AssignHandle(this.doc.NumHandles);
            //}
            //else
            //{
            //    dxfObject.Handle = handle;
            //}
            
            Debug.Assert(!string.IsNullOrEmpty(handle), "Entity without handle.");
            dxfObject.Handle = handle;

            if (dxfObject is EntityObject entity)
            {
                entity.Layer = layer;
                entity.Color = color;
                entity.Linetype = linetype;
                entity.Lineweight = lineweight;
                entity.LinetypeScale = linetypeScale;
                entity.IsVisible = isVisible;
                entity.Transparency = transparency;
            }

            if (dxfObject is AttributeDefinition attDef)
            {
                attDef.Layer = layer;
                attDef.Color = color;
                attDef.Linetype = linetype;
                attDef.Lineweight = lineweight;
                attDef.LinetypeScale = linetypeScale;
                attDef.IsVisible = isVisible;
                attDef.Transparency = transparency;
            }

            // the entities list will be processed at the end
            // entities that belong to a block definition are added at the same time of the block 
            if (!isBlockEntity)
            {
                this.entityList.Add(dxfObject, owner);
            }

            return dxfObject;
        }

        private Insert ReadAcadTable(bool isBlockEntity)
        {
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 direction = Vector3.UnitX;
            string blockName = string.Empty;
            Block block = null;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        blockName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!isBlockEntity)
                        {
                            block = this.GetBlock(blockName);
                        }
                        this.chunk.Next();
                        break;
                    case 10:
                        basePoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // It is a lot more intuitive to give the position in world coordinates and then define the orientation with the normal.
            Vector3 wcsBasePoint = MathHelper.Transform(basePoint, normal, CoordinateSystem.Object, CoordinateSystem.World);
            Insert insert = new Insert(new List<Attribute>())
            {
                Block = block,
                Position = wcsBasePoint,
                Normal = normal
            };
            insert.XData.AddRange(xData);

            //Vector3 ocsDirection = MathHelper.Transform(direction, normal, CoordinateSystem.World, CoordinateSystem.Object);
            //insert.Rotation = Vector2.Angle(new Vector2(ocsDirection.X, ocsDirection.Y))*MathHelper.RadToDeg;
            //insert.Scale = new Vector3(1.0);

            // post process nested inserts
            if (isBlockEntity)
            {
                this.nestedInserts.Add(insert, blockName);
            }

            return insert;
        }

        private Wipeout ReadWipeout()
        {
            Vector3 position = Vector3.Zero;
            Vector3 u = Vector3.UnitX;
            Vector3 v = Vector3.UnitY;
            ClippingBoundaryType boundaryType = ClippingBoundaryType.Rectangular;
            double x = 0.0;
            List<Vector2> vertexes = new List<Vector2>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        u.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        u.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        u.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        v.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        v.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        v.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 71:
                        boundaryType = (ClippingBoundaryType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 91:
                        // we cannot rely in this information it may or may not appear
                        this.chunk.Next();
                        break;
                    case 14:
                        x = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        vertexes.Add(new Vector2(x, this.chunk.ReadDouble()));
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // for polygonal boundaries the last vertex is equal to the first, we will remove it
            if (boundaryType == ClippingBoundaryType.Polygonal)
            {
                vertexes.RemoveAt(vertexes.Count - 1);
            }

            Vector3 normal = Vector3.Normalize(Vector3.CrossProduct(u, v));
            List<Vector2> ocsPoints = MathHelper.Transform(new List<Vector3> {position, u, v}, normal, out double elevation);
            double bx = ocsPoints[0].X;
            double by = ocsPoints[0].Y;
            double max = ocsPoints[1].X;

            for (int i = 0; i < vertexes.Count; i++)
            {
                double vx = bx + max * (0.5 + vertexes[i].X);
                double vy = by + max * (0.5 - vertexes[i].Y);
                vertexes[i] = new Vector2(vx, vy);
            }

            ClippingBoundary clippingBoundary = boundaryType == ClippingBoundaryType.Rectangular ? new ClippingBoundary(vertexes[0], vertexes[1]) : new ClippingBoundary(vertexes);
            Wipeout entity = new Wipeout(clippingBoundary)
            {
                Normal = normal,
                Elevation = elevation
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Underlay ReadUnderlay()
        {
            string underlayDefHandle = null;
            Vector3 position = Vector3.Zero;
            Vector2 scale = new Vector2(1.0);
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            UnderlayDisplayFlags displayOptions = UnderlayDisplayFlags.ShowUnderlay;
            short contrast = 100;
            short fade = 0;
            Vector2 clippingVertex = Vector2.Zero;
            List<Vector2> clippingVertexes = new List<Vector2>();
            ClippingBoundary clippingBoundary;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        scale.X = Math.Abs(this.chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        if (MathHelper.IsZero(scale.X))
                        {
                            scale.X = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 42:
                        scale.Y = Math.Abs(this.chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        if (MathHelper.IsZero(scale.Y))
                        {
                            scale.Y = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 43:
                        // the scale Z value has no use
                        //scale.Z = Math.Abs(this.chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        //if (MathHelper.IsZero(scale.Z)) scale.Z = 1.0;
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 340:
                        underlayDefHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 280:
                        displayOptions = (UnderlayDisplayFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 281:
                        contrast = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 282:
                        fade = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 11:
                        clippingVertex = new Vector2();
                        clippingVertex.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        clippingVertex.Y = this.chunk.ReadDouble();
                        clippingVertexes.Add(clippingVertex);
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Vector3 wcsPosition = MathHelper.Transform(position, normal, CoordinateSystem.Object, CoordinateSystem.World);
            if (clippingVertexes.Count < 2)
            {
                clippingBoundary = null;
            }
            else if (clippingVertexes.Count == 2)
            {
                clippingBoundary = new ClippingBoundary(clippingVertexes[0], clippingVertexes[1]);
            }
            else
            {
                clippingBoundary = new ClippingBoundary(clippingVertexes);
            }

            Underlay underlay = new Underlay
            {
                Position = wcsPosition,
                Scale = scale,
                Normal = normal,
                Rotation = rotation,
                DisplayOptions = displayOptions,
                Contrast = contrast,
                Fade = fade,
                ClippingBoundary = clippingBoundary
            };

            underlay.XData.AddRange(xData);

            if (string.IsNullOrEmpty(underlayDefHandle) || underlayDefHandle == "0")
            {
                return null;
            }

            this.underlayToDefinitionHandles.Add(underlay, underlayDefHandle);

            return underlay;
        }

        private Tolerance ReadTolerance()
        {
            DimensionStyle style = DimensionStyle.Default;
            Vector3 position = Vector3.Zero;
            string value = string.Empty;
            Vector3 normal = Vector3.UnitZ;
            Vector3 xAxis = Vector3.UnitX;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 3:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.DimStyle;
                        }
                        style = this.GetDimensionStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1:
                        value = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        xAxis.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        xAxis.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        xAxis.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            xAxis = MathHelper.Transform(xAxis, normal, CoordinateSystem.World, CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(xAxis.X, xAxis.Y));

            if (!Tolerance.TryParseStringRepresentation(value, out Tolerance entity))
            {
                // if there is a problem creating the Tolerance entity from the string data, remove it
                Debug.Assert(false, "The tolerance string representation is not well formatted");
                return null;
            }

            entity.Style = style;
            entity.TextHeight = style.TextHeight;
            entity.Position = position;
            entity.Rotation = rotation * MathHelper.RadToDeg;
            entity.Normal = normal;
            entity.XData.AddRange(xData);

            // here is where DXF stores the tolerance text height
            if (entity.XData.TryGetValue(ApplicationRegistry.DefaultName, out XData heightXData))
            {
                double textHeight = this.ReadToleranceTextHeightXData(heightXData);
                if (textHeight > 0)
                {
                    entity.TextHeight = textHeight;
                }
            }

            return entity;
        }

        private double ReadToleranceTextHeightXData(XData xData)
        {
            double textHeight = -1;
            using (IEnumerator<XDataRecord> records = xData.XDataRecord.GetEnumerator())
            {
                while (records.MoveNext())
                {
                    XDataRecord data = records.Current;
                    if (data == null)
                    {
                        return textHeight;
                    }

                    // the tolerance text height are stored under the string "DSTYLE"
                    if (data.Code == XDataCode.String && string.Equals((string) data.Value, "DSTYLE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (records.MoveNext())
                        {
                            data = records.Current;
                            if (data == null)
                            {
                                return textHeight;
                            }
                        }
                        else
                        {
                            return textHeight; // premature end
                        }

                        // all style overrides are enclosed between XDataCode.ControlString "{" and "}"
                        if (data.Code != XDataCode.ControlString && (string) data.Value != "{")
                        {
                            return textHeight; // premature end
                        }

                        if (records.MoveNext())
                        {
                            data = records.Current;
                            if (data == null)
                            {
                                return textHeight;
                            }
                        }
                        else
                        {
                            return textHeight; // premature end
                        }

                        while (!(data.Code == XDataCode.ControlString && (string) data.Value == "}"))
                        {
                            if (data.Code != XDataCode.Int16)
                            {
                                return textHeight;
                            }

                            short textHeightCode = (short) data.Value;
                            if (records.MoveNext())
                            {
                                data = records.Current;
                                if (data == null)
                                {
                                    return textHeight;
                                }
                            }
                            else
                            {
                                return textHeight; // premature end
                            }

                            //the xData overrides must be read in pairs.
                            //the first is the dimension style property to override, the second is the new value
                            switch (textHeightCode)
                            {
                                case 140:
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return textHeight; // premature end
                                    }
                                    textHeight = (double) data.Value;
                                    break;
                            }

                            if (records.MoveNext())
                            {
                                data = records.Current;
                                if (data == null)
                                {
                                    return textHeight;
                                }
                            }
                            else
                            {
                                return textHeight; // premature end
                            }
                        }
                    }
                }
            }

            return textHeight;
        }

        private Leader ReadLeader()
        {
            DimensionStyle style = DimensionStyle.Default;
            bool showArrowhead = true;
            LeaderPathType path = LeaderPathType.StraightLineSegments;
            bool hasHookline = false;
            List<Vector3> wcsVertexes = null;
            AciColor lineColor = AciColor.ByLayer;
            string annotation = string.Empty;
            Vector3 normal = Vector3.UnitZ;
            Vector3 offset = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;

            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 3:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.DimStyle;
                        }
                        style = this.GetDimensionStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 71:
                        showArrowhead = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 72:
                        path = (LeaderPathType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        this.chunk.Next();
                        break;
                    case 74:
                        this.chunk.Next();
                        break;
                    case 75:
                        hasHookline = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 76:
                        this.chunk.Next();
                        break;
                    case 77:
                        lineColor = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 10:
                        wcsVertexes = this.ReadLeaderVertexes();
                        break;
                    case 340:
                        annotation = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 211:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 221:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 231:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 213:
                        offset.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 223:
                        offset.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 233:
                        offset.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (wcsVertexes == null)
            {
                return null;
            }
            if (wcsVertexes.Count < 2)
            {
                return null;
            }

            List<Vector2> vertexes = MathHelper.Transform(wcsVertexes, normal, out double elevation);

            Vector2 ocsOffset = MathHelper.Transform(offset, normal, out _);
            Vector2 ocsDirection = MathHelper.Transform(direction, normal, out _);

            Leader leader = new Leader(vertexes, style, hasHookline)
            {
                ShowArrowhead = showArrowhead,
                PathType = path,
                LineColor = lineColor,
                Elevation = elevation,
                Normal = normal,
                Offset = ocsOffset,
                Direction = ocsDirection
            };

            leader.XData.AddRange(xData);

            // this is for post-processing, the annotation entity might appear after the leader
            this.leaderAnnotation.Add(leader, annotation);

            return leader;
        }

        private List<Vector3> ReadLeaderVertexes()
        {
            List<Vector3> vertexes = new List<Vector3>();
            while (this.chunk.Code == 10)
            {
                Vector3 vertex = Vector3.Zero;
                vertex.X = this.chunk.ReadDouble();
                this.chunk.Next();
                vertex.Y = this.chunk.ReadDouble();
                this.chunk.Next();
                if (this.chunk.Code == 30)
                {
                    vertex.Z = this.chunk.ReadDouble();
                    this.chunk.Next();
                }
                vertexes.Add(vertex);
            }
            return vertexes;
        }

        private Mesh ReadMesh()
        {
            int subdivisionLevel = 0;
            List<Vector3> vertexes = null;
            List<int[]> faces = null;
            List<MeshEdge> edges = null;
            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 91:
                        subdivisionLevel = this.chunk.ReadInt();
                        if (subdivisionLevel < 0 || subdivisionLevel > 255)
                        {
                            subdivisionLevel = 0;
                        }
                        this.chunk.Next();
                        break;
                    case 92:
                        int numVertexes = this.chunk.ReadInt();
                        this.chunk.Next();
                        vertexes = this.ReadMeshVertexes(numVertexes);
                        break;
                    case 93:
                        int sizeFaceList = this.chunk.ReadInt();
                        this.chunk.Next();
                        faces = this.ReadMeshFaces(sizeFaceList);
                        break;
                    case 94:
                        int numEdges = this.chunk.ReadInt();
                        this.chunk.Next();
                        edges = this.ReadMeshEdges(numEdges);
                        break;
                    case 95:
                        int numCrease = this.chunk.ReadInt();
                        this.chunk.Next();
                        if (edges == null)
                        {
                            throw new NullReferenceException("The edges list is not initialized.");
                        }

                        if (numCrease != edges.Count)
                        {
                            throw new Exception("The number of edge creases must be the same as the number of edges.");
                        }
                        this.ReadMeshEdgeCreases(edges);
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }
            Mesh entity = new Mesh(vertexes, faces, edges)
            {
                SubdivisionLevel = (byte) subdivisionLevel,
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private List<Vector3> ReadMeshVertexes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), count, "The number of vertexes must be greater than zero.");

            List<Vector3> vertexes = new List<Vector3>(count);
            for (int i = 0; i < count; i++)
            {
                double x = this.chunk.ReadDouble();
                this.chunk.Next();
                double y = this.chunk.ReadDouble();
                this.chunk.Next();
                double z = this.chunk.ReadDouble();
                this.chunk.Next();

                vertexes.Add(new Vector3(x, y, z));
            }

            return vertexes;
        }

        private List<int[]> ReadMeshFaces(int size)
        {
            Debug.Assert(size > 0, "The size of face list must be greater than zero.");

            List<int[]> faces = new List<int[]>();

            for (int i = 0; i < size; i++)
            {
                int indexes = this.chunk.ReadInt();
                this.chunk.Next();
                int[] face = new int[indexes];
                for (int j = 0; j < indexes; j++)
                {
                    face[j] = this.chunk.ReadInt();
                    this.chunk.Next();
                }
                faces.Add(face);
                i += indexes;
            }

            return faces;
        }

        private List<MeshEdge> ReadMeshEdges(int count)
        {
            List<MeshEdge> vertexes = new List<MeshEdge>(count);

            for (int i = 0; i < count; i++)
            {
                int start = this.chunk.ReadInt();
                this.chunk.Next();
                int end = this.chunk.ReadInt();
                this.chunk.Next();

                vertexes.Add(new MeshEdge(start, end));
            }
            return vertexes;
        }

        private void ReadMeshEdgeCreases(IEnumerable<MeshEdge> edges)
        {
            foreach (MeshEdge edge in edges)
            {
                edge.Crease = this.chunk.ReadDouble();
                this.chunk.Next();
            }
        }

        private Viewport ReadViewport()
        {
            Debug.Assert(this.chunk.ReadString() == SubclassMarker.Viewport);

            Viewport viewport = new Viewport();
            Vector3 center = viewport.Center;
            Vector2 viewCenter = viewport.ViewCenter;
            Vector2 snapBase = viewport.SnapBase;
            Vector2 snapSpacing = viewport.SnapSpacing;
            Vector2 gridSpacing = viewport.GridSpacing;
            Vector3 viewDirection = viewport.ViewDirection;
            Vector3 viewTarget = viewport.ViewTarget;
            Vector3 ucsOrigin = Vector3.Zero;
            Vector3 ucsXAxis = Vector3.UnitX;
            Vector3 ucsYAxis = Vector3.UnitY;
            List<Layer> frozenLayers = new List<Layer>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        viewport.Width = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        viewport.Height = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 68:
                        viewport.Stacking = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 69:
                        viewport.Id = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 12:
                        viewCenter.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        viewCenter.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        snapBase.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        snapBase.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        snapSpacing.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        snapSpacing.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        gridSpacing.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        gridSpacing.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 16:
                        viewDirection.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 26:
                        viewDirection.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 36:
                        viewDirection.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 17:
                        viewTarget.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 27:
                        viewTarget.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 37:
                        viewTarget.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 42:
                        viewport.LensLength = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 43:
                        viewport.FrontClipPlane = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 44:
                        viewport.BackClipPlane = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 45:
                        viewport.ViewHeight = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        viewport.SnapAngle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 51:
                        viewport.TwistAngle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 72:
                        viewport.CircleZoomPercent = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 331:
                        // we will post process the frozen layers after the viewport is added to the document
                        frozenLayers.Add((Layer) this.doc.GetObjectByHandle(this.chunk.ReadHex()));
                        this.chunk.Next();
                        break;
                    case 90:
                        viewport.Status = (ViewportStatusFlags) this.chunk.ReadInt();
                        this.chunk.Next();
                        break;
                    case 340:
                        // we will post process the clipping boundary in case it has been defined before the viewport
                        this.viewports.Add(viewport, this.chunk.ReadHex());
                        this.chunk.Next();
                        break;
                    case 110:
                        ucsOrigin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 120:
                        ucsOrigin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 130:
                        ucsOrigin.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 111:
                        ucsXAxis.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 121:
                        ucsXAxis.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 131:
                        ucsXAxis.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 112:
                        ucsYAxis.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 122:
                        ucsYAxis.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 132:
                        ucsYAxis.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            viewport.Center = center;
            viewport.ViewCenter = viewCenter;
            viewport.SnapBase = snapBase;
            viewport.SnapSpacing = snapSpacing;
            viewport.GridSpacing = gridSpacing;
            viewport.ViewDirection = viewDirection;
            viewport.ViewTarget = viewTarget;
            viewport.UcsOrigin = ucsOrigin;
            viewport.UcsXAxis = ucsXAxis;
            viewport.UcsYAxis = ucsYAxis;
            viewport.XData.AddRange(xData);

            this.viewportFrozenLayers.Add(viewport, frozenLayers);

            return viewport;
        }

        private Image ReadImage()
        {
            Vector3 position = Vector3.Zero;
            Vector3 u = Vector3.Zero;
            Vector3 v = Vector3.Zero;
            double width = 1.0;
            double height = 1.0;
            string imageDefHandle = string.Empty;
            ImageDisplayFlags displayOptions = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen | ImageDisplayFlags.UseClippingBoundary;
            bool clipping = false;
            short brightness = 50;
            short contrast = 50;
            short fade = 0;
            double x = 0.0;
            List<Vector2> vertexes = new List<Vector2>();
            ClippingBoundaryType boundaryType = ClippingBoundaryType.Rectangular;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        u.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        u.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        u.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        v.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        v.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        v.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        width = Math.Abs(this.chunk.ReadDouble()); // just in case, the image width must be always positive
                        this.chunk.Next();
                        break;
                    case 23:
                        height = Math.Abs(this.chunk.ReadDouble()); // just in case, the image height must be always positive
                        this.chunk.Next();
                        break;
                    case 340:
                        imageDefHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 70:
                        displayOptions = (ImageDisplayFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 280:
                        clipping = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 281:
                        brightness = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 282:
                        contrast = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 283:
                        fade = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        boundaryType = (ClippingBoundaryType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 91:
                        // we cannot rely in this information it might or might not appear
                        this.chunk.Next();
                        break;
                    case 14:
                        x = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        vertexes.Add(new Vector2(x, this.chunk.ReadDouble()));
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (u == Vector3.Zero || v == Vector3.Zero) return null;

            Vector3 normal = Vector3.CrossProduct(u, v);
            List<Vector3> uvOCS = MathHelper.Transform(new [] {u, v}, normal, CoordinateSystem.World, CoordinateSystem.Object);
            //double rotation = Vector2.Angle(new Vector2(uOCS.X, uOCS.Y))*MathHelper.RadToDeg;
            double uLength = u.Modulus();
            double vLength = v.Modulus();

            // for polygonal boundaries the last vertex is equal to the first, we will remove it
            if (boundaryType == ClippingBoundaryType.Polygonal) vertexes.RemoveAt(vertexes.Count - 1);

            for (int i = 0; i < vertexes.Count; i++)
            {
                double vx = vertexes[i].X + 0.5;
                double vy = vertexes[i].Y + 0.5;
                vertexes[i] = new Vector2(vx, vy);
            }

            ClippingBoundary clippingBoundary = boundaryType == ClippingBoundaryType.Rectangular ? new ClippingBoundary(vertexes[0], vertexes[1]) : new ClippingBoundary(vertexes);

            Image image = new Image
            {
                Width = width*uLength,
                Height = height*vLength,
                Position = position,
                Normal = normal,
                Uvector = new Vector2(uvOCS[0].X, uvOCS[0].Y),
                Vvector = new Vector2(uvOCS[1].X, uvOCS[1].Y),
                //Rotation = rotation,
                DisplayOptions = displayOptions,
                Clipping = clipping,
                Brightness = brightness,
                Contrast = contrast,
                Fade = fade,
                ClippingBoundary = clippingBoundary
            };

            image.XData.AddRange(xData);

            if (string.IsNullOrEmpty(imageDefHandle) || imageDefHandle == "0")
            {
                return null;
            }

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
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        radius = this.chunk.ReadDouble();
                        if (radius <= 0)
                            radius = 1.0;
                        this.chunk.Next();
                        break;
                    case 50:
                        startAngle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 51:
                        endAngle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // this is just an example of the stupid DXF way of doing things, while an ellipse the center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 wcsCenter = MathHelper.Transform(center, normal, CoordinateSystem.Object, CoordinateSystem.World);
            Arc entity = new Arc
            {
                Center = wcsCenter,
                Radius = radius,
                StartAngle = startAngle,
                EndAngle = endAngle,
                Thickness = thickness,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Circle ReadCircle()
        {
            Vector3 center = Vector3.Zero;
            double radius = 1.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        radius = this.chunk.ReadDouble();
                        if (radius <= 0)
                            radius = 1.0;
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // this is just an example of the DXF way of doing things, while an ellipse the center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            Vector3 wcsCenter = MathHelper.Transform(center, normal, CoordinateSystem.Object, CoordinateSystem.World);

            Circle entity = new Circle
            {
                Center = wcsCenter,
                Radius = radius,
                Thickness = thickness,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Dimension ReadDimension(bool isBlockEntity)
        {
            string drawingBlockName = string.Empty;
            string subclassMarker = string.Empty;
            Block drawingBlock = null;
            Vector3 defPoint = Vector3.Zero;
            Vector3 midtxtPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            DimensionTypeFlags dimType = DimensionTypeFlags.Linear;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.BottomCenter;
            MTextLineSpacingStyle lineSpacingStyle = MTextLineSpacingStyle.AtLeast;
            DimensionStyle style = DimensionStyle.Default;
            double lineSpacingFactor = 1.0;
            bool dimInfo = false;
            double dimRot = 0.0;
            double textRotation = 0.0;
            string userText = null;
            bool userTextPosition = false;

            this.chunk.Next();
            while (!dimInfo)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        userText = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(userText.Trim(' ', '\t')))
                        {
                            userText = string.Empty;
                        }
                        this.chunk.Next();
                        break;
                    case 2:
                        drawingBlockName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!isBlockEntity)
                        {
                            drawingBlock = this.GetBlock(drawingBlockName);
                        }
                        this.chunk.Next();
                        break;
                    case 3:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.DimStyle;
                        }
                        style = this.GetDimensionStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 10:
                        defPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        defPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        defPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        midtxtPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        midtxtPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        midtxtPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        dimType = (DimensionTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        lineSpacingStyle = (MTextLineSpacingStyle) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 41:
                        lineSpacingFactor = this.chunk.ReadDouble();
                        if (lineSpacingFactor < 0.25 || lineSpacingFactor > 4.0)
                        {
                            lineSpacingFactor = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 51:
                        dimRot = 360 - this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 53:
                        textRotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 100:
                        subclassMarker = this.chunk.ReadString();
                        if (subclassMarker == SubclassMarker.AlignedDimension ||
                            subclassMarker == SubclassMarker.RadialDimension ||
                            subclassMarker == SubclassMarker.DiametricDimension ||
                            subclassMarker == SubclassMarker.Angular3PointDimension ||
                            subclassMarker == SubclassMarker.Angular2LineDimension ||
                            subclassMarker == SubclassMarker.OrdinateDimension ||
                            subclassMarker == SubclassMarker.ArcDimension)
                        {
                            // we have finished reading the basic dimension info
                            dimInfo = true; 
                        }
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            // this is the result of the way the DXF use the DimensionTypeFlag enumeration, it is a mixture of a regular enumeration with flags
            DimensionTypeFlags type = dimType;
            OrdinateDimensionAxis axis = OrdinateDimensionAxis.Y;
            if (type.HasFlag(DimensionTypeFlags.BlockReference))
            {
                type -= DimensionTypeFlags.BlockReference;
            }
            if (type.HasFlag(DimensionTypeFlags.OrdinateType))
            {
                axis = OrdinateDimensionAxis.X;
                type -= DimensionTypeFlags.OrdinateType;
            }
            if (type.HasFlag(DimensionTypeFlags.UserTextPosition))
            {
                userTextPosition = true;
                type -= DimensionTypeFlags.UserTextPosition;
            }

            Dimension dim;
            switch (subclassMarker)
            {
                case SubclassMarker.AlignedDimension:
                    dim = this.ReadAlignedDimension(defPoint, normal);
                    break;
                case SubclassMarker.LinearDimension:
                    dim = this.ReadLinearDimension(defPoint, normal);
                    break;
                case SubclassMarker.RadialDimension:
                    dim = this.ReadRadialDimension(defPoint, normal);
                    break;
                case SubclassMarker.DiametricDimension:
                    dim = this.ReadDiametricDimension(defPoint, normal);
                    break;
                case SubclassMarker.Angular3PointDimension:
                    dim = this.ReadAngular3PointDimension(defPoint, normal);
                    break;
                case SubclassMarker.Angular2LineDimension:
                    dim = this.ReadAngular2LineDimension(defPoint, normal);
                    break;
                case SubclassMarker.OrdinateDimension:
                    dim = this.ReadOrdinateDimension(defPoint, axis, normal, dimRot);
                    break;
                case SubclassMarker.ArcDimension:
                    dim = this.ReadArcLengthDimension(defPoint, normal);
                    break;
                default:
                    throw new ArgumentException(string.Format("The dimension type: {0} is not implemented or unknown.", type));
            }

            if (dim != null)
            {
                dim.Style = style;
                dim.Block = drawingBlock;
                dim.TextReferencePoint = new Vector2(midtxtPoint.X, midtxtPoint.Y);
                dim.TextPositionManuallySet = userTextPosition;
                dim.AttachmentPoint = attachmentPoint;
                dim.LineSpacingStyle = lineSpacingStyle;
                dim.LineSpacingFactor = lineSpacingFactor;
                dim.Normal = normal;
                dim.TextRotation = textRotation;
                dim.UserText = userText;

                if (isBlockEntity)
                {
                    this.nestedDimensions.Add(dim, drawingBlockName);
                }
            }

            return dim;
        }

        private List<DimensionStyleOverride> ReadDimensionStyleOverrideXData(XData xDataOverrides)
        {
            List<DimensionStyleOverride> overrides = new List<DimensionStyleOverride>();
            
            bool[] suppress;

            short dimtfill = -1;
            AciColor dimtfillclrt = null;
            short dimzin = -1;
            short dimazin = -1;

            short dimaltu = -1;
            short dimaltz = -1;

            short dimtol = -1;
            short dimlim = -1;
            double dimtm = 0.0;
            double dimtp = 0.0;
            short dimtzin = -1;
            short dimalttz = -1;

            bool dimsah = true;
            string handleDimblk = string.Empty;
            string handleDimblk1 = string.Empty;
            string handleDimblk2 = string.Empty;

            using (IEnumerator<XDataRecord> records = xDataOverrides.XDataRecord.GetEnumerator())
            {
                while (records.MoveNext())
                {
                    XDataRecord data = records.Current;

                    // the dimension style overrides are stored under the string "DSTYLE"
                    if (data.Code == XDataCode.String && string.Equals((string) data.Value, "DSTYLE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (records.MoveNext())
                        {
                            data = records.Current;
                        }
                        else
                        {
                            return overrides; // premature end
                        }

                        // all style overrides are enclosed between XDataCode.ControlString "{" and "}"
                        if (data.Code != XDataCode.ControlString && (string) data.Value != "{")
                        {
                            return overrides; // premature end
                        }

                        if (records.MoveNext())
                        {
                            data = records.Current;
                        }
                        else
                        {
                            return overrides; // premature end
                        }

                        while (!(data.Code == XDataCode.ControlString && (string)data.Value == "}"))
                        {
                            if (data.Code != XDataCode.Int16)
                            {
                                return overrides;
                            }
                            short styleOverrideCode = (short)data.Value;
                            if (records.MoveNext())
                            {
                                data = records.Current;
                            }
                            else
                            {
                                return overrides; // premature end
                            }

                            //the xData overrides must be read in pairs.
                            //the first is the dimension style property to override, the second is the new value
                            switch (styleOverrideCode)
                            {
                                case 3: // DIMPOST
                                    if (data.Code != XDataCode.String)
                                    {
                                        return overrides; // premature end
                                    }

                                    string dimpost = this.DecodeEncodedNonAsciiCharacters((string) data.Value);
                                    string[] textPrefixSuffix = GetDimStylePrefixAndSuffix(dimpost, '<', '>');
                                    if (!string.IsNullOrEmpty(textPrefixSuffix[0]))
                                    {
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimPrefix, textPrefixSuffix[0]));
                                    }

                                    if (!string.IsNullOrEmpty(textPrefixSuffix[1]))
                                    {
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimSuffix, textPrefixSuffix[1]));
                                    }

                                    break;
                                case 4: // DIMAPOST
                                    if (data.Code != XDataCode.String)
                                    {
                                        return overrides; // premature end
                                    }

                                    string dimapost = this.DecodeEncodedNonAsciiCharacters((string) data.Value);
                                    string[] altTextPrefixSuffix = GetDimStylePrefixAndSuffix(dimapost, '[', ']');
                                    if (!string.IsNullOrEmpty(altTextPrefixSuffix[0]))
                                    {
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsPrefix, altTextPrefixSuffix[0]));
                                    }

                                    if (!string.IsNullOrEmpty(altTextPrefixSuffix[1]))
                                    {
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsSuffix, altTextPrefixSuffix[1]));
                                    }

                                    break;
                                case 40: // DIMSCALE
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimScaleOverall, (double) data.Value));
                                    break;
                                case 41: // DIMASZ:
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ArrowSize, (double) data.Value));
                                    break;
                                case 42: // DIMEXO
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineOffset, (double) data.Value));
                                    break;
                                case 43: // DIMDLI
                                    // not used in overrides
                                    break;
                                case 44: // DIMEXE
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineExtend, (double) data.Value));
                                    break;
                                case 45: // DIMRND
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimRoundoff, (short) data.Value));
                                    break;
                                case 46: // DIMDLE
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineExtend, (double) data.Value));
                                    break;
                                case 47: // DIMTP
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtp = (double) data.Value;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesUpperLimit, dimtp));
                                    break;
                                case 48: // DIMTM
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtm = (double) data.Value;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesLowerLimit, dimtm));
                                    break;
                                case 49: // DIMFXL
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineFixedLength, (double) data.Value));
                                    break;
                                case 69: // DIMTFILL
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtfill = (short) data.Value;
                                    break;
                                case 70: // DIMTFILLCLRT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtfillclrt = AciColor.FromCadIndex((short) data.Value);
                                    break;
                                case 71: // DIMTOLL
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtol = (short) data.Value;
                                    break;
                                case 72: // DIMLIN
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimlim = (short) data.Value;
                                    break;
                                case 73: // DIMTIH
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextInsideAlign, (short) data.Value != 0));
                                    break;
                                case 74: // DIMTOH
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextOutsideAlign, (short) data.Value != 0));
                                    break;
                                case 75: // DIMSE1
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Off, (short) data.Value != 0));
                                    break;
                                case 76: // DIMSE2
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine2Off, (short) data.Value != 0));
                                    break;
                                case 77: // DIMTAD
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextVerticalPlacement, (DimensionStyleTextVerticalPlacement) (short) data.Value));
                                    break;
                                case 78: // DIMZIN
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimzin = (short) data.Value;
                                    break;
                                case 79: // DIMAZIN
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimazin = (short) data.Value;
                                    break;
                                case 140: // DIMTXT
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextHeight, (double) data.Value));
                                    break;
                                case 141: // DIMCEN
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.CenterMarkSize, (double) data.Value));
                                    break;
                                case 143: // DIMALTF
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsMultiplier, (double) data.Value));
                                    break;
                                case 144: // DIMLFAC:
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimScaleLinear, (double) data.Value));
                                    break;
                                case 145: // DIMTVP
                                    // not used
                                    break;
                                case 146: // DIMTFAC
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextFractionHeightScale, (double) data.Value));
                                    break;
                                case 147: // DIMGAP
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextOffset, (double) data.Value));
                                    break;
                                case 148: // DIMALTRND
                                    if (data.Code != XDataCode.Real)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsRoundoff, (double) data.Value));
                                    break;
                                case 170: // DIMALT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsEnabled, (short) data.Value != 0));
                                    break;
                                case 171: // DIMALTD
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthPrecision, (short) data.Value));
                                    break;
                                case 172: // DIMTOFL
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitDimLineForce, (short) data.Value != 0));
                                    break;
                                case 173: // DIMSAH
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }
                                    dimsah = (short) data.Value != 0;
                                    break;
                                case 174: // DIMTIX
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextInside, (short) data.Value != 0));
                                    break;
                                case 175: // DIMSOXD
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitDimLineInside, (short) data.Value == 0));
                                    break;
                                case 176: // DIMCLRD:
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 177: // DIMCLRE
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 178: // DIMCLRT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 179: // DIMADEC
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AngularPrecision, (short) data.Value));
                                    break;
                                case 271: // DIMDEC
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.LengthPrecision, (short) data.Value));
                                    break;
                                case 272: // DIMTDEC
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesPrecision, (short) data.Value));
                                    break;
                                case 273: // DIMALTU
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimaltu = (short) data.Value;
                                    break;
                                case 274: // DIMALTTD
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAlternatePrecision, (short) data.Value));
                                    break;
                                case 275: // DIMAUNIT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimAngularUnits, (AngleUnitType) (short) data.Value));
                                    break;
                                case 276: // DIMFRAC
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FractionalType, (FractionFormatType) (short) data.Value));
                                    break;
                                case 277: // DIMLUNIT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLengthUnits, (LinearUnitType) (short) data.Value));
                                    break;
                                case 278: // DIMDSEP
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DecimalSeparator, (char) (short) data.Value));
                                    break;
                                case 279: // DIMTMOVE
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, (DimensionStyleFitTextMove) (short) data.Value));
                                    break;
                                case 280: // DIMJUST
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextHorizontalPlacement, (DimensionStyleTextHorizontalPlacement) (short) data.Value));
                                    break;
                                case 281: // DIMSD1
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine1Off, (short) data.Value != 0));
                                    break;
                                case 282: // DIMSD2
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine2Off, (short) data.Value != 0));
                                    break;
                                case 283: // DIMTOLJ
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesVerticalPlacement, (DimensionStyleTolerancesVerticalPlacement) (short) data.Value));
                                    break;
                                case 284: // DIMTZIN
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimtzin = (short) data.Value;
                                    break;
                                case 285: // DIMALTZ
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimaltz = (short) data.Value;
                                    break;
                                case 286: // DIMALTTZ
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    dimalttz = (short) data.Value;
                                    break;
                                case 288: // DIMUPT
                                    // not used
                                    break;
                                case 289: // AIMATFIT
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitOptions, (DimensionStyleFitOptions) (short) data.Value));
                                    break;
                                case 290: // DIMFXLON
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineFixed, (short) data.Value != 0));
                                    break;
                                case 294: // DIMTXTDIRECTION
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextDirection, (DimensionStyleTextDirection) (short) data.Value));
                                    break;
                                case 340: // DIMTXSTY
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    TextStyle dimtxtsty = this.doc.GetObjectByHandle((string) data.Value) as TextStyle;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextStyle, dimtxtsty == null ? this.doc.TextStyles[TextStyle.DefaultName] : dimtxtsty));
                                    break;
                                case 341: // DIMLDRBLK
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    BlockRecord dimldrblk = this.doc.GetObjectByHandle((string) data.Value) as BlockRecord;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.LeaderArrow, dimldrblk == null ? null : this.doc.Blocks[dimldrblk.Name]));
                                    break;
                                case 342: // DIMBLK used if DIMSAH is false
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    handleDimblk = (string) data.Value;
                                    break;
                                case 343: // DIMBLK1
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    handleDimblk1 = (string) data.Value;
                                    break;
                                case 344: // DIMBLK2
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    handleDimblk2 = (string) data.Value;
                                    break;
                                case 345: // DIMLTYPE
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    Linetype dimltype = this.doc.GetObjectByHandle((string) data.Value) as Linetype ?? this.doc.Linetypes[Linetype.DefaultName];

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLinetype, dimltype));
                                    break;
                                case 346: // DIMLTEX1
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    Linetype dimltex1 = this.doc.GetObjectByHandle((string) data.Value) as Linetype ?? this.doc.Linetypes[Linetype.DefaultName];

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Linetype, dimltex1));
                                    break;
                                case 347: // DIMLTEX2
                                    if (data.Code != XDataCode.DatabaseHandle)
                                    {
                                        return overrides; // premature end
                                    }

                                    Linetype dimltex2 = this.doc.GetObjectByHandle((string) data.Value) as Linetype ?? this.doc.Linetypes[Linetype.DefaultName];

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine2Linetype, dimltex2));
                                    break;
                                case 371: // DIMLWD
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLineweight, (Lineweight) (short) data.Value));
                                    break;
                                case 372: // DIMLWE
                                    if (data.Code != XDataCode.Int16)
                                    {
                                        return overrides; // premature end
                                    }

                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineLineweight, (Lineweight) (short) data.Value));
                                    break;
                            }

                            if (records.MoveNext())
                            {
                                data = records.Current;
                            }
                            else
                            {
                                return overrides; // premature end
                            }
                        }
                    }
                }
            }

            // dimension text fill color
            if (dimtfill >= 0)
            {
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextFillColor, dimtfill == 2 ? dimtfillclrt : null));
            }

            // dim arrows
            if (dimsah)
            {
                if (!string.IsNullOrEmpty(handleDimblk1))
                {
                    if (this.doc.GetObjectByHandle(handleDimblk1) is BlockRecord dimblk1)
                    {
                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, this.doc.Blocks[dimblk1.Name]));
                    }
                }
                if (!string.IsNullOrEmpty(handleDimblk2))
                {
                    if (this.doc.GetObjectByHandle(handleDimblk2) is BlockRecord dimblk2)
                    {
                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow2, this.doc.Blocks[dimblk2.Name]));
                    }
                }
            }
            else
            {
                // just in case, but it seems that for dimension style overrides the variable DIMSAH seems to be always true,
                // therefore the DIMBLK1 and DIMBLK2 variables will be used 
                if (!string.IsNullOrEmpty(handleDimblk))
                {
                    if (this.doc.GetObjectByHandle(handleDimblk) is BlockRecord dimblk)
                    {
                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, this.doc.Blocks[dimblk.Name]));
                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow2, this.doc.Blocks[dimblk.Name]));
                    }
                }
            }

            // suppress linear zeros
            if (dimzin >= 0)
            {
                suppress = GetLinearZeroesSuppression(dimzin);
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressLinearLeadingZeros, suppress[0]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressLinearTrailingZeros, suppress[1]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressZeroFeet, suppress[2]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressZeroInches, suppress[3]));
            }

            // suppress angular leading and/or trailing zeros
            switch (dimazin)
            {
                case 0:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularLeadingZeros, false));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularTrailingZeros, false));
                    break;
                case 1:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularLeadingZeros, true));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularTrailingZeros, false));
                    break;
                case 2:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularLeadingZeros, false));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularTrailingZeros, true));
                    break;
                case 3:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularLeadingZeros, true));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressAngularTrailingZeros, true));
                    break;
            }

            // alternate units format
            switch (dimaltu)
            {
                case 1:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Scientific));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, false));
                    break;
                case 2:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Decimal));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, false));
                    break;
                case 3:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Engineering));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, false));
                    break;
                case 4:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Architectural));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, true));
                    break;
                case 5:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Fractional));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, true));
                    break;
                case 6:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Architectural));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, false));
                    break;
                case 7:
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthUnits, LinearUnitType.Fractional));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsStackedUnits, false));
                    break;
            }

            // suppress leading and/or trailing zeros
            if (dimaltz >= 0)
            {
                suppress = GetLinearZeroesSuppression(dimaltz);
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsSuppressLinearLeadingZeros, suppress[0]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsSuppressLinearTrailingZeros, suppress[1]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsSuppressZeroFeet, suppress[2]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsSuppressZeroInches, suppress[3]));
            }

            // suppress leading and/or trailing zeros
            if (dimtzin >= 0)
            {
                suppress = GetLinearZeroesSuppression(dimtzin);
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesSuppressLinearLeadingZeros, suppress[0]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesSuppressLinearTrailingZeros, suppress[1]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesSuppressZeroFeet, suppress[2]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesSuppressZeroInches, suppress[3]));
            }

            // suppress leading and/or trailing zeros
            if (dimalttz >= 0)
            {
                suppress = GetLinearZeroesSuppression(dimalttz);
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAltSuppressLinearLeadingZeros, suppress[0]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAltSuppressLinearTrailingZeros, suppress[1]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAltSuppressZeroFeet, suppress[2]));
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAltSuppressZeroInches, suppress[3]));
            }

            if (dimtol == 0 && dimlim == 0)
            {
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesDisplayMethod, DimensionStyleTolerancesDisplayMethod.None));
            }
            else if (dimtol == 1 && dimlim == 0)
            {
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesDisplayMethod,
                    MathHelper.IsEqual(dimtm, dimtp)
                        ? DimensionStyleTolerancesDisplayMethod.Symmetrical
                        : DimensionStyleTolerancesDisplayMethod.Deviation));
            }
            else if (dimtol == 0 && dimlim == 1)
            {
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesDisplayMethod, DimensionStyleTolerancesDisplayMethod.Limits));
            }

            return overrides;
        }

        private AlignedDimension ReadAlignedDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        firstRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        secondRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new List<Vector3>
                {
                    firstRef, secondRef, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            AlignedDimension entity = new AlignedDimension
            {
                FirstReferencePoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                SecondReferencePoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                Elevation = ocsPoints[2].Z,
                Normal = normal
            };

            entity.SetDimensionLinePosition(new Vector2(ocsPoints[2].X, ocsPoints[2].Y));

            entity.XData.AddRange(xData);

            return entity;
        }

        private LinearDimension ReadLinearDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            double rot = 0.0;
            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        firstRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        secondRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        rot = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 52:
                        // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the DXF documentation
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new List<Vector3>
                {
                    firstRef, secondRef, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            LinearDimension entity = new LinearDimension
            {
                FirstReferencePoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                SecondReferencePoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                Rotation = rot,
                Elevation = ocsPoints[2].Z,
                Normal = normal
            };

            entity.SetDimensionLinePosition(new Vector2(ocsPoints[2].X, ocsPoints[2].Y));

            entity.XData.AddRange(xData);

            return entity;
        }

        private RadialDimension ReadRadialDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 15:
                        circunferenceRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        circunferenceRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        circunferenceRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new List<Vector3>
                {
                    circunferenceRef, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            RadialDimension entity = new RadialDimension
            {
                ReferencePoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                CenterPoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                DefinitionPoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                Elevation = ocsPoints[1].Z,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private DiametricDimension ReadDiametricDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 15:
                        circunferenceRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        circunferenceRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        circunferenceRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new List<Vector3>
                {
                    circunferenceRef, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            Vector3 center = Vector3.MidPoint(ocsPoints[0], ocsPoints[1]);
            DiametricDimension entity = new DiametricDimension
            {
                CenterPoint = new Vector2(center.X, center.Y),
                ReferencePoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                DefinitionPoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                Elevation = ocsPoints[1].Z,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Angular3PointDimension ReadAngular3PointDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 center = Vector3.Zero;
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;

            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        firstRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        secondRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new[]
                {
                    center, firstRef, secondRef, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            Angular3PointDimension entity = new Angular3PointDimension
            {
                CenterPoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                StartPoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                EndPoint = new Vector2(ocsPoints[2].X, ocsPoints[2].Y),
                Elevation = ocsPoints[3].Z
            };

            entity.SetDimensionLinePosition(new Vector2(ocsPoints[3].X, ocsPoints[3].Y));

            entity.XData.AddRange(xData);

            return entity;
        }

        private Angular2LineDimension ReadAngular2LineDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 startFirstLine = Vector3.Zero;
            Vector3 endFirstLine = Vector3.Zero;
            Vector3 startSecondLine = Vector3.Zero;
            Vector3 arcDefinitionPoint = Vector3.Zero;

            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        startFirstLine.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        startFirstLine.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        startFirstLine.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        endFirstLine.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        endFirstLine.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        endFirstLine.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        startSecondLine.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        startSecondLine.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        startSecondLine.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 16:
                        arcDefinitionPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 26:
                        arcDefinitionPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 36:
                        arcDefinitionPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new[]
                {
                    startFirstLine, endFirstLine, startSecondLine, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            Vector2 startL0 = new Vector2(ocsPoints[0].X, ocsPoints[0].Y);
            Vector2 endL0 = new Vector2(ocsPoints[1].X, ocsPoints[1].Y);
            Vector2 startL1 = new Vector2(ocsPoints[2].X, ocsPoints[2].Y);
            Vector2 endL1 = new Vector2(ocsPoints[3].X, ocsPoints[3].Y);

            Vector2 dir1 = Vector2.Normalize(endL0 - startL0);
            Vector2 dir2 = Vector2.Normalize(endL1 - startL1);
            if (Vector2.AreParallel(dir1, dir2)) return null;
               
            Angular2LineDimension entity = new Angular2LineDimension
            {
                StartFirstLine = startL0,
                EndFirstLine = endL0,
                StartSecondLine = startL1,
                EndSecondLine = endL1,
                Elevation = ocsPoints[3].Z
            };

            entity.SetDimensionLinePosition(new Vector2(arcDefinitionPoint.X, arcDefinitionPoint.Y));

            entity.XData.AddRange(xData);

            return entity;
        }

        private OrdinateDimension ReadOrdinateDimension(Vector3 defPoint, OrdinateDimensionAxis axis, Vector3 normal, double rotation)
        {
            Vector3 firstPoint = Vector3.Zero;
            Vector3 secondPoint = Vector3.Zero;

            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        firstPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        firstPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        firstPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        secondPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        secondPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        secondPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new[]
                {
                    firstPoint, secondPoint, defPoint
                },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            OrdinateDimension entity = new OrdinateDimension
            {
                Origin = new Vector2(ocsPoints[2].X, ocsPoints[2].Y),
                Rotation = rotation,
                Axis = axis,
                FeaturePoint = new Vector2(ocsPoints[0].X, ocsPoints[0].Y),
                LeaderEndPoint = new Vector2(ocsPoints[1].X, ocsPoints[1].Y),
                DefinitionPoint = new Vector2(ocsPoints[2].X, ocsPoints[2].Y),
                Elevation = ocsPoints[2].Z,
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private ArcLengthDimension ReadArcLengthDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 center = Vector3.Zero;
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;

            List<XData> xData = new List<XData>();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 13:
                        firstRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        secondRef.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            List<Vector3> ocsPoints = MathHelper.Transform(
                new[] { center, firstRef, secondRef, defPoint },
                normal, CoordinateSystem.World, CoordinateSystem.Object);

            Vector2 arcCenter = new Vector2(ocsPoints[0].X, ocsPoints[0].Y);
            Vector2 arcStart = new Vector2(ocsPoints[1].X, ocsPoints[1].Y);
            Vector2 arcEnd = new Vector2(ocsPoints[2].X, ocsPoints[2].Y);

            ArcLengthDimension entity = new ArcLengthDimension
            {
                CenterPoint = arcCenter,
                Radius = Vector2.Distance(arcCenter, arcStart),
                StartAngle = Vector2.Angle(arcStart - arcCenter) * MathHelper.RadToDeg,
                EndAngle = Vector2.Angle(arcEnd - arcCenter) * MathHelper.RadToDeg,
                Elevation = ocsPoints[3].Z
            };

            entity.SetDimensionLinePosition(new Vector2(ocsPoints[3].X, ocsPoints[3].Y));

            entity.XData.AddRange(xData);

            return entity;
        }

        private Ellipse ReadEllipse()
        {
            Vector3 center = Vector3.Zero;
            Vector3 axisPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double[] param = new double[2];
            double ratio = 0.0;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        center.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        center.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        center.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        axisPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        axisPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        axisPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        ratio = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        param[0] = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 42:
                        param[1] = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint, normal, CoordinateSystem.World, CoordinateSystem.Object);

            double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y));
            double majorAxis = 2 * axisPoint.Modulus();
            double minorAxis = majorAxis * ratio;

            Ellipse ellipse = new Ellipse(center, majorAxis, minorAxis)
            {
                Rotation = rotation * MathHelper.RadToDeg,
                Normal = normal
            };

            ellipse.XData.AddRange(xData);

            SetEllipseParameters(ellipse, param);

            return ellipse;
        }

        private static void SetEllipseParameters(Ellipse ellipse, double[] param)
        {
            if (MathHelper.IsZero(param[0]) && MathHelper.IsEqual(param[1], MathHelper.TwoPI))
            {
                ellipse.StartAngle = 0.0;
                ellipse.EndAngle = 0.0;
            }
            else
            {
                double a = ellipse.MajorAxis * 0.5;
                double b = ellipse.MinorAxis * 0.5;

                Vector2 startPoint = new Vector2(a * Math.Cos(param[0]), b * Math.Sin(param[0]));
                Vector2 endPoint = new Vector2(a * Math.Cos(param[1]), b * Math.Sin(param[1]));

                if (Vector2.Equals(startPoint, endPoint))
                {
                    ellipse.StartAngle = 0.0;
                    ellipse.EndAngle = 0.0;
                }
                else
                {
                    ellipse.StartAngle = Vector2.Angle(startPoint) * MathHelper.RadToDeg;
                    ellipse.EndAngle = Vector2.Angle(endPoint) * MathHelper.RadToDeg;
                }
            }
        }

        private Point ReadPoint()
        {
            Vector3 location = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            double rotation = 0.0;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        location.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        location.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        location.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = 360.0 - this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Point entity = new Point
            {
                Position = location,
                Thickness = thickness,
                Rotation = rotation,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Face3D ReadFace3d()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            Face3DEdgeFlags flags = Face3DEdgeFlags.None;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        v0.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        v0.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        v0.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        v1.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        v1.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        v1.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        v2.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        v2.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        v2.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        v3.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        v3.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        v3.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (Face3DEdgeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Face3D entity = new Face3D
            {
                FirstVertex = v0,
                SecondVertex = v1,
                ThirdVertex = v2,
                FourthVertex = v3,
                EdgeFlags = flags
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Shape ReadShape()
        {
            string name = string.Empty;
            Vector3 position = Vector3.Zero;
            double size = 1.0;
            double rotation = 0.0;
            double obliqueAngle = 0.0;
            double widthFactor = 1.0;
            double thickness = 0.0;
            double rotateNegativeSize = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        size = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(size))
                        {
                            size = 1.0;
                        }
                        else if (size < 0.0)
                        {
                            size = Math.Abs(size);
                            rotateNegativeSize = 180;
                        }
                        this.chunk.Next();
                        break;
                    case 2:
                        name = this.chunk.ReadString();
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        widthFactor = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(widthFactor))
                        {
                            widthFactor = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            // if the shape has no name it will be skipped
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // the shape definition does not store any information about the ShapeStyle where the geometry of the shape is stored
            // we will look for a shape with the specified name inside any of the shape styles defined in the document.
            // if none are found the shape will be skipped.
            ShapeStyle style = this.doc.ShapeStyles.ContainsShapeName(name);
            // if a shape style has not been found that contains a shape definition with the specified name, the shape will be skipped
            if (style == null)
            {
                return null;
            }
            
            Shape entity = new Shape(name, style)
            {
                // The official DXF documentation is WRONG the SHAPE position is saved as OCS coordinates NOT as WCS
                Position = MathHelper.Transform(position, normal, CoordinateSystem.Object, CoordinateSystem.World),
                Size = size,
                Rotation = rotation + rotateNegativeSize,
                WidthFactor = widthFactor,
                ObliqueAngle = obliqueAngle,
                Thickness = thickness,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Solid ReadSolid()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        v0.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        v0.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        v0.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        v1.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        v1.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        v1.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        v2.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        v2.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        v2.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        v3.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        v3.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        v3.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Solid entity = new Solid
            {
                FirstVertex = new Vector2(v0.X, v0.Y),
                SecondVertex = new Vector2(v1.X, v1.Y),
                ThirdVertex = new Vector2(v2.X, v2.Y),
                FourthVertex = new Vector2(v3.X, v3.Y),
                Elevation = v0.Z,
                Thickness = thickness,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Trace ReadTrace()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        v0.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        v0.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        v0.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        v1.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        v1.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        v1.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        v2.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        v2.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        v2.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        v3.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        v3.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        v3.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Trace entity = new Trace
            {
                FirstVertex = new Vector2(v0.X, v0.Y),
                SecondVertex = new Vector2(v1.X, v1.Y),
                ThirdVertex = new Vector2(v2.X, v2.Y),
                FourthVertex = new Vector2(v3.X, v3.Y),
                Elevation = v0.Z,
                Thickness = thickness,
                Normal = normal
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Spline ReadSpline()
        {
            SplineTypeFlags flags = SplineTypeFlags.Open;
            Vector3 normal = Vector3.UnitZ;
            short degree = 3;

            List<double> knots = new List<double>();
            List<Vector3> ctrlPoints = new List<Vector3>();
            List<double> weights = new List<double>();
            double ctrlX = 0;
            double ctrlY = 0;
            double ctrlZ;

            // tolerances (not used)
            double knotTolerance = 0.0000001;
            double ctrlPointTolerance = 0.0000001;
            double fitTolerance = 0.0000000001;

            // start and end tangents
            double stX = 0;
            double stY = 0;
            double stZ;
            Vector3? startTangent = null;
            double etX = 0;
            double etY = 0;
            double etZ;
            Vector3? endTangent = null;

            // fit points variable
            List<Vector3> fitPoints = new List<Vector3>();
            double fitX = 0;
            double fitY = 0;
            double fitZ;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (SplineTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        degree = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        // the spline entity can actually hold a larger number of knots, we cannot use this information it might be wrong.
                        //short numKnots = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        // the spline entity can actually hold a larger number of control points
                        //short numCtrlPoints = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 74:
                        // the spline entity can actually hold a larger number of fit points, we cannot use this information it might be wrong.
                        //numFitPoints = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 42:
                        knotTolerance = this.chunk.ReadDouble();
                        if (knotTolerance <= 0) knotTolerance = 0.0000001;
                        this.chunk.Next();
                        break;
                    case 43:
                        ctrlPointTolerance = this.chunk.ReadDouble();
                        if (ctrlPointTolerance <= 0) ctrlPointTolerance = 0.0000001;
                        this.chunk.Next();
                        break;
                    case 44:
                        fitTolerance = this.chunk.ReadDouble();
                        if (fitTolerance <= 0) fitTolerance = 0.0000000001;
                        this.chunk.Next();
                        break;
                    case 12:
                        stX = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        stY = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        stZ = this.chunk.ReadDouble();
                        startTangent = new Vector3(stX, stY, stZ);
                        this.chunk.Next();
                        break;
                    case 13:
                        etX = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        etY = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        etZ = this.chunk.ReadDouble();
                        endTangent = new Vector3(etX, etY, etZ);
                        this.chunk.Next();
                        break;
                    case 40:
                        // multiple code 40 entries, one per knot value
                        knots.Add(this.chunk.ReadDouble());
                        this.chunk.Next();
                        break;
                    case 10:
                        ctrlX = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        ctrlY = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        ctrlZ = this.chunk.ReadDouble();
                        ctrlPoints.Add(new Vector3(ctrlX, ctrlY, ctrlZ));
                        this.chunk.Next();
                        break;
                    case 41:
                        // code 41 might appear before or after the control point coordinates.
                        double weight = this.chunk.ReadDouble();
                        weights.Add(weight);
                        this.chunk.Next();
                        break;
                    case 11:
                        fitX = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        fitY = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        fitZ = this.chunk.ReadDouble();
                        fitPoints.Add(new Vector3(fitX, fitY, fitZ));
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            SplineCreationMethod method;
            if (fitPoints.Count == 0)
            {
                method = SplineCreationMethod.ControlPoints;
            }
            else
            {
                method = flags.HasFlag(SplineTypeFlags.FitPointCreationMethod) ? SplineCreationMethod.FitPoints : SplineCreationMethod.ControlPoints;
            }

            bool isPeriodic;
            if (ctrlPoints[0].Equals(ctrlPoints[ctrlPoints.Count - 1]))
            {
                isPeriodic = false;
            }
            else
            {
                isPeriodic = flags.HasFlag(SplineTypeFlags.ClosedPeriodicSpline);
                if (isPeriodic)
                {
                    ctrlPoints.RemoveRange(0, degree);
                    weights.RemoveRange(0, degree);
                }
            }

            if (weights.Count == 0)
            {
                weights = null;
            }

            Spline entity = new Spline(ctrlPoints, weights, knots, degree, fitPoints, method, isPeriodic)
            {
                KnotTolerance = knotTolerance,
                CtrlPointTolerance = ctrlPointTolerance,
                FitTolerance = fitTolerance,
                StartTangent = startTangent,
                EndTangent = endTangent
            };

            if (flags.HasFlag(SplineTypeFlags.FitChord))
            {
                entity.KnotParameterization = SplineKnotParameterization.FitChord;
            }
            else if (flags.HasFlag(SplineTypeFlags.FitSqrtChord))
            {
                entity.KnotParameterization = SplineKnotParameterization.FitSqrtChord;
            }
            else if (flags.HasFlag(SplineTypeFlags.FitUniform))
            {
                entity.KnotParameterization = SplineKnotParameterization.FitUniform;
            }
            else if (flags.HasFlag(SplineTypeFlags.FitCustom))
            {
                entity.KnotParameterization = SplineKnotParameterization.FitCustom;
            }

            entity.XData.AddRange(xData);

            return entity;
        }

        private Insert ReadInsert(bool isBlockEntity)
        {
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 scale = new Vector3(1.0, 1.0, 1.0);
            double rotation = 0.0;
            string blockName = null;
            Block block = null;
            List<Attribute> attributes = new List<Attribute>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        blockName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (!isBlockEntity)
                        {
                            block = this.GetBlock(blockName);
                        }
                        this.chunk.Next();
                        break;
                    case 10:
                        basePoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        scale.X = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.X)) scale.X = 1.0; // just in case, the insert scale components cannot be zero
                        this.chunk.Next();
                        break;
                    case 42:
                        scale.Y = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.Y)) scale.Y = 1.0; // just in case, the insert scale components cannot be zero
                        this.chunk.Next();
                        break;
                    case 43:
                        scale.Z = this.chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.Z)) scale.Z = 1.0; // just in case, the insert scale components cannot be zero
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (this.chunk.ReadString() == DxfObjectCode.Attribute)
            {
                while (this.chunk.ReadString() != DxfObjectCode.EndSequence)
                {
                    Attribute attribute = this.ReadAttribute(block, isBlockEntity);
                    if (attribute != null)
                    {
                        attributes.Add(attribute);
                    }
                }
            }

            if (this.chunk.ReadString() == DxfObjectCode.EndSequence)
            {
                // read the end sequence object until a new element is found
                this.chunk.Next();
                // the EndSequence data is not needed
                while (this.chunk.Code != 0)
                {
                    this.chunk.Next();
                }
            }

            // It is a lot more intuitive to give the position in world coordinates and then define the orientation with the normal.
            Vector3 wcsBasePoint = MathHelper.Transform(basePoint, normal, CoordinateSystem.Object, CoordinateSystem.World);
            Insert insert = new Insert(attributes)
            {
                Block = block,
                Position = wcsBasePoint,
                Rotation = rotation,
                Scale = scale,
                Normal = normal
            };
            insert.XData.AddRange(xData);

            // post process nested inserts
            if (isBlockEntity)
            {
                this.nestedInserts.Add(insert, blockName);
            }

            return insert;
        }

        private Line ReadLine()
        {
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        start.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        start.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        start.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        end.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        end.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        end.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Line entity = new Line
            {
                StartPoint = start,
                EndPoint = end,
                Normal = normal,
                Thickness = thickness
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Ray ReadRay()
        {
            Vector3 origin = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        origin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        origin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        origin.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Ray entity = new Ray
            {
                Origin = origin,
                Direction = direction
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private XLine ReadXLine()
        {
            Vector3 origin = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 10:
                        origin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        origin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        origin.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            XLine entity = new XLine
            {
                Origin = origin,
                Direction = direction
            };

            entity.XData.AddRange(xData);

            return entity;
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
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        // the MLineStyle is defined in the objects sections after the definition of the entity, something similar happens with the image entity
                        // the MLineStyle will be applied to the MLine after parsing the whole file
                        styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.CMLStyle;
                        }
                        this.chunk.Next();
                        break;
                    case 40:
                        scale = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        justification = (MLineJustification) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        flags = (MLineFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        numVertexes = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        numStyleElements = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 10:
                        // this info is not needed it is repeated in the vertexes list
                        this.chunk.Next();
                        break;
                    case 20:
                        // this info is not needed it is repeated in the vertexes list
                        this.chunk.Next();
                        break;
                    case 30:
                        // this info is not needed it is repeated in the vertexes list
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        // the info that follows contains the information on the vertexes of the MLine
                        segments = this.ReadMLineSegments(numVertexes, numStyleElements, normal, out elevation);
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            MLine mline = new MLine
            {
                Elevation = elevation,
                Scale = scale,
                Justification = justification,
                Normal = normal,
                Flags = flags
            };
            mline.Vertexes.AddRange(segments);
            mline.XData.AddRange(xData);

            // save the referenced style name for post processing
            this.mLineToStyleNames.Add(mline, styleName);

            return mline;
        }

        private List<MLineVertex> ReadMLineSegments(int numVertexes, int numStyleElements, Vector3 normal, out double elevation)
        {
            elevation = 0.0;

            List<MLineVertex> segments = new List<MLineVertex>();
            for (int i = 0; i < numVertexes; i++)
            {
                Vector3 vertex = new Vector3();
                vertex.X = this.chunk.ReadDouble(); // code 11
                this.chunk.Next();
                vertex.Y = this.chunk.ReadDouble(); // code 21
                this.chunk.Next();
                vertex.Z = this.chunk.ReadDouble(); // code 31
                this.chunk.Next();

                Vector3 dir = new Vector3();
                dir.X = this.chunk.ReadDouble(); // code 12
                this.chunk.Next();
                dir.Y = this.chunk.ReadDouble(); // code 22
                this.chunk.Next();
                dir.Z = this.chunk.ReadDouble(); // code 32
                this.chunk.Next();

                Vector3 miter = new Vector3();
                miter.X = this.chunk.ReadDouble(); // code 13
                this.chunk.Next();
                miter.Y = this.chunk.ReadDouble(); // code 23
                this.chunk.Next();
                miter.Z = this.chunk.ReadDouble(); // code 33
                this.chunk.Next();

                List<double>[] distances = new List<double>[numStyleElements];
                for (int j = 0; j < numStyleElements; j++)
                {
                    distances[j] = new List<double>();
                    short numDistances = this.chunk.ReadShort(); // code 74
                    this.chunk.Next();
                    for (short k = 0; k < numDistances; k++)
                    {
                        distances[j].Add(this.chunk.ReadDouble()); // code 41
                        this.chunk.Next();
                    }

                    // no more info is needed, fill parameters are not supported
                    short numFillParams = this.chunk.ReadShort(); // code 75
                    this.chunk.Next();
                    for (short k = 0; k < numFillParams; k++)
                    {
                        //double param = this.chunk.ReadDouble(); // code 42
                        this.chunk.Next();
                    }
                }

                // we need to convert WCS coordinates to OCS coordinates
                Matrix3 trans = MathHelper.ArbitraryAxis(normal).Transpose();
                vertex = trans * vertex;
                dir = trans * dir;
                miter = trans * miter;

                MLineVertex segment = new MLineVertex(new Vector2(vertex.X, vertex.Y),
                    new Vector2(dir.X, dir.Y),
                    new Vector2(miter.X, miter.Y),
                    distances);

                elevation = vertex.Z;
                segments.Add(segment);
            }

            return segments;
        }

        private Polyline2D ReadLwPolyline()
        {
            double elevation = 0.0;
            double thickness = 0.0;
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            double constantWidth = -1.0;
            List<Polyline2DVertex> polVertexes = new List<Polyline2DVertex>();
            Polyline2DVertex v = new Polyline2DVertex();
            double vX = 0.0;
            Vector3 normal = Vector3.UnitZ;

            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 38:
                        elevation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 43:
                        // constant width (optional; default = 0). If present it will override any vertex width (codes 40 and/or 41)
                        constantWidth = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        this.chunk.Next();
                        break;
                    case 10:
                        vX = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        double vY = this.chunk.ReadDouble();
                        v = new Polyline2DVertex(vX, vY);
                        polVertexes.Add(v);
                        this.chunk.Next();
                        break;
                    case 40:
                        double startWidth = this.chunk.ReadDouble();
                        if (startWidth >= 0.0)
                        {
                            v.StartWidth = startWidth;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        double endWidth = this.chunk.ReadDouble();
                        if (endWidth >= 0.0)
                        {
                            v.EndWidth = endWidth;
                        }
                        this.chunk.Next();
                        break;
                    case 42:
                        v.Bulge = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Polyline2D entity = new Polyline2D(polVertexes)
            {
                Elevation = elevation,
                Thickness = thickness,
                Flags = flags,
                Normal = normal
            };

            if (constantWidth >= 0.0)
            {
                entity.SetConstantWidth(constantWidth);
            }

            entity.XData.AddRange(xData);

            return entity;
        }

        private Polyline2D ReadPolyline2D(Polyline polyline)
        {
            // Polyline2D, the vertexes are expressed in local coordinates
            List<Polyline2DVertex> polylineVertexes = new List<Polyline2DVertex>();

            if (polyline.Flags.HasFlag(PolylineTypeFlags.SplineFit))
            {
                foreach (Vertex v in polyline.Vertexes)
                {
                    // the vertex list will only store the vertexes of the original polyline
                    if (v.Flags.HasFlag(VertexTypeFlags.SplineFrameControlPoint))
                    {
                        Polyline2DVertex point = new Polyline2DVertex
                        {
                            Position = new Vector2(v.Position.X, v.Position.Y),
                            StartWidth = v.StartWidth,
                            EndWidth = v.EndWidth,
                        };
                        polylineVertexes.Add(point);
                    }
                }
            }
            else
            {
                foreach (Vertex v in polyline.Vertexes)
                {
                    Polyline2DVertex vertex = new Polyline2DVertex
                    {
                        Position = new Vector2(v.Position.X, v.Position.Y),
                        Bulge = v.Bulge,
                        StartWidth = v.StartWidth,
                        EndWidth = v.EndWidth,
                    };

                    polylineVertexes.Add(vertex);
                }
            }
            
            Polyline2D polyline2D = new Polyline2D(polylineVertexes, polyline.Flags.HasFlag(PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM))
            {
                SmoothType = polyline.SmoothType,
                Thickness = polyline.Thickness,
                Elevation = polyline.Elevation,
                Normal = polyline.Normal,
                Flags = polyline.Flags
            };

            polyline2D.XData.AddRange(polyline.XData.Values);

            return polyline2D;
        }

        private Polyline3D ReadPolyline3D(Polyline polyline)
        {
            // Polyline3D, the vertexes are expressed in world coordinates
            List<Vector3> polyline3dVertexes = new List<Vector3>();

            if (polyline.Flags.HasFlag(PolylineTypeFlags.SplineFit))
            {
                foreach (Vertex v in polyline.Vertexes)
                {
                    // the vertex list will only store the vertexes of the original polyline
                    if (v.Flags.HasFlag(VertexTypeFlags.SplineFrameControlPoint))
                    {
                        polyline3dVertexes.Add(v.Position);
                    }
                }
            }
            else
            {
                foreach (Vertex v in polyline.Vertexes)
                {
                    // the polyline vertex list will only store the vertexes of the original 
                    if (v.Flags.HasFlag(VertexTypeFlags.Polyline3DVertex))
                    {
                        polyline3dVertexes.Add(v.Position);
                    }
                }
            }
            
            Polyline3D polyline3D = new Polyline3D(polyline3dVertexes, polyline.Flags.HasFlag(PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM))
            {
                SmoothType = polyline.SmoothType,
                Normal = polyline.Normal,
                Flags = polyline.Flags
            };

            polyline3D.XData.AddRange(polyline.XData.Values);

            return polyline3D;
        }

        private PolyfaceMesh ReadPolyfaceMesh(Polyline polyline)
        {
            // PolyfaceMesh, the vertexes are expressed in world coordinates
            //the vertex list created contains vertex and face information
            List<Vector3> polyfaceVertexes = new List<Vector3>();
            List<PolyfaceMeshFace> polyfaceFaces = new List<PolyfaceMeshFace>();
            foreach (Vertex v in polyline.Vertexes)
            {
                if (v.Flags.HasFlag(VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3DMeshVertex))
                {
                    polyfaceVertexes.Add(v.Position);
                }
                else if (v.Flags.HasFlag(VertexTypeFlags.PolyfaceMeshVertex))
                {
                    polyfaceFaces.Add(new PolyfaceMeshFace(v.VertexIndexes)
                    {
                        Layer = v.Layer,
                        Color = v.Color
                    });
                }
            }
            
            if (polyfaceVertexes.Count < 3)
            {
                Debug.Assert(false, "The polyface must contain at least three vertexes.");
                return null;
            }

            if (polyfaceFaces.Count == 0)
            {
                Debug.Assert(false, "The polyface must contain at least one face.");
                return null;
            }

            PolyfaceMesh pMesh = new PolyfaceMesh(polyfaceVertexes, polyfaceFaces)
            {
                Normal = polyline.Normal,
                Flags = polyline.Flags
            };

            pMesh.XData.AddRange(polyline.XData.Values);

            return pMesh;
        }

        private PolygonMesh ReadPolygonMesh(Polyline polyline)
        {
            // the number of vertexes along the M direction must be between 2 and 256
            if(polyline.M < 2 || polyline.M > 256)
            {
                Debug.Assert(false, "The number of vertexes along the M direction must be between 2 and 256.");
                return null;
            }

            // the number of vertexes along the N direction must be between 2 and 256
            if(polyline.N < 2 || polyline.N > 256)
            {
                Debug.Assert(false, "The number of vertexes along the M direction must be between 2 and 256.");
                return null;
            }


            Vector3[] polygonMeshVertexes = new Vector3[polyline.M * polyline.N];

            PolygonMesh pMesh;
            int i;
            int j;

            if (polyline.Flags.HasFlag(PolylineTypeFlags.SplineFit))
            {
                i = 0;
                j = 0;
                foreach (Vertex vertex in polyline.Vertexes)
                {
                    if (vertex.Flags.HasFlag(VertexTypeFlags.SplineFrameControlPoint))
                    {
                        polygonMeshVertexes[i + j * polyline.M] = vertex.Position;
                        if (++j < polyline.N) continue;
                        j = 0;
                        i += 1;
                    }
                }

                if (polygonMeshVertexes.Length != polyline.M * polyline.N)
                {
                    Debug.Assert(false, "The number of vertexes must be equal to MxN.");
                    return null;
                }

                pMesh = new PolygonMesh(polyline.M, polyline.N, polygonMeshVertexes)
                {
                    SmoothType = polyline.SmoothType,
                    DensityU = polyline.DensityM,
                    DensityV = polyline.DensityN,
                    Normal = polyline.Normal,
                    Flags = polyline.Flags
                };

                pMesh.XData.AddRange(polyline.XData.Values);

                return pMesh;
            }

            i = 0;
            j = 0;
            foreach (Vertex vertex in polyline.Vertexes)
            {
                if (vertex.Flags.HasFlag(VertexTypeFlags.Polygon3DMeshVertex))
                {
                    polygonMeshVertexes[i + j * polyline.M] = vertex.Position;
                    if (++j < polyline.N) continue;
                    j = 0;
                    i += 1;
                }
            }
            pMesh = new PolygonMesh(polyline.M, polyline.N, polygonMeshVertexes)
            {
                SmoothType = polyline.SmoothType,
                Normal = polyline.Normal,
                Flags = polyline.Flags
            };

            pMesh.XData.AddRange(polyline.XData.Values);

            return pMesh;
        }

        private EntityObject ReadPolyline()
        {
            // the entity Polyline in DXF can actually hold four kinds of entities
            // 1. 3D polyline is the generic polyline
            // 2. Polygon mesh
            // 3. Polyface mesh
            // 4. 2D polylines is the old way of writing polylines the AutoCAD2000 and newer use LwPolylines to define a 2D polyline
            //    this way of reading 2d polylines is here for compatibility reasons with older DXF versions.
            //    SplineFit and CurveFit 2D polylines are always saved as a 2D polyline
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            PolylineSmoothType smoothType = PolylineSmoothType.NoSmooth;
            double elevation = 0.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            short m = 0;
            short n = 0;
            short densityM = 0;
            short densityN = 0;
            List<Vertex> vertexes = new List<Vertex>();
            List<XData> xData = new List<XData>();
            
            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 30:
                        elevation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 39:
                        thickness = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        m = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        n = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        densityM = this.chunk.ReadShort();
                        if (densityM < 3 || densityM > 201)
                        {
                            densityM = (short) (this.doc.DrawingVariables.SurfU + 1);
                        }
                        this.chunk.Next();
                        break;
                    case 74:
                        densityN = this.chunk.ReadShort();
                        if (densityN < 3 || densityN > 201)
                        {
                            densityN = (short) (this.doc.DrawingVariables.SurfV + 1);
                        }
                        this.chunk.Next();
                        break;
                    case 75:
                        short smooth = this.chunk.ReadShort();
                        // SmoothType BezierSurface not implemented, reset to default
                        smoothType = smooth == 8 ? PolylineSmoothType.NoSmooth : (PolylineSmoothType) smooth;
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            //begin to read the vertex list (although it is not recommended the vertex list might have 0 entries)
            while (this.chunk.ReadString() != DxfObjectCode.EndSequence)
            {
                if (this.chunk.ReadString() == DxfObjectCode.Vertex)
                {
                    Vertex vertex = this.ReadVertex();
                    vertexes.Add(vertex);
                }
            }

            // read the end sequence object until a new element is found
            // this information is not needed
            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        this.chunk.Next();
                        break;
                    case 8:
                        // the polyline EndSequence layer should be the same as the polyline layer
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            Polyline polyline = new Polyline
            {
                SubclassMarker = SubclassMarker.PolygonMesh,
                Vertexes = vertexes,
                Normal = normal,
                Elevation = elevation,
                Thickness = thickness,
                Flags = flags,
                SmoothType = smoothType,
                M = m,
                N = n,
                DensityM = densityM,
                DensityN = densityN
            };
            polyline.XData.AddRange(xData);

            if (flags.HasFlag(PolylineTypeFlags.Polyline3D))
            {
                return this.ReadPolyline3D(polyline);
            }
            if (flags.HasFlag(PolylineTypeFlags.PolyfaceMesh))
            {
                return this.ReadPolyfaceMesh(polyline);
            }
            if (flags.HasFlag(PolylineTypeFlags.PolygonMesh))
            {
                return this.ReadPolygonMesh(polyline);
            }
               
            return this.ReadPolyline2D(polyline); 
        }

        private Vertex ReadVertex()
        {
            string handle = string.Empty;
            Layer layer = null;
            AciColor color = null;
            Linetype linetype = Linetype.ByLayer;
            Vector3 position = new Vector3();
            double startWidth = 0.0;
            double endWidth = 0.0;
            double bulge = 0.0;
            List<short> vertexIndexes = new List<short>();
            VertexTypeFlags flags = VertexTypeFlags.Polyline2DVertex;

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 8:
                        string layerName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        layer = this.GetLayer(layerName);
                        this.chunk.Next();
                        break;
                    case 62: //ACI color code
                        color = AciColor.FromCadIndex(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 6:
                        string linetypeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        linetype = this.GetLinetype(linetypeName);
                        this.chunk.Next();
                        break;
                    case 10:
                        position.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        position.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        position.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        startWidth = this.chunk.ReadDouble();
                        if (startWidth < 0.0)
                        {
                            startWidth = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        endWidth = this.chunk.ReadDouble();
                        if (endWidth < 0.0)
                        {
                            endWidth = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 42:
                        bulge = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 71:
                        vertexIndexes.Add(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 72:
                        vertexIndexes.Add(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 73:
                        vertexIndexes.Add(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    case 74:
                        vertexIndexes.Add(this.chunk.ReadShort());
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            return new Vertex
            {
                Flags = flags,
                Position = position,
                StartWidth = startWidth,
                Bulge = bulge,
                Color = color,
                EndWidth = endWidth,
                Layer = layer,
                Linetype = linetype,
                VertexIndexes = vertexIndexes.ToArray(),
                Handle = handle
            };
        }

        private Text ReadText()
        {
            string textString = string.Empty;
            double height = 0.0;
            double width = 1.0;
            double widthFactor = 1.0;
            double rotation = 0.0;
            double obliqueAngle = 0.0;
            TextStyle style = TextStyle.Default;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            short horizontalAlignment = 0;
            short verticalAlignment = 0;
            bool isBackward = false;
            bool isUpsideDown = false;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        textString = this.chunk.ReadString();
                        this.chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        if (height <= 0.0)
                        {
                            height = this.doc.DrawingVariables.TextSize;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        widthFactor = this.chunk.ReadDouble();
                        if (widthFactor < 0.01 || widthFactor > 100.0)
                        {
                            widthFactor = this.doc.DrawingVariables.TextSize;
                        }
                        this.chunk.Next();
                        break;
                    case 50:
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = this.chunk.ReadDouble();
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                        {
                            obliqueAngle = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 7:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.TextStyle;
                        }
                        style = this.GetTextStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 71:
                        short textGeneration = this.chunk.ReadShort();
                        if (textGeneration - 2 == 0)
                        {
                            isBackward = true;
                        }
                        if (textGeneration - 4 == 0)
                        {
                            isUpsideDown = true;
                        }
                        if (textGeneration - 6 == 0)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        this.chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        verticalAlignment = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            switch (alignment)
            {
                case TextAlignment.BaselineLeft:
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                case TextAlignment.Fit:
                case TextAlignment.Aligned:
                    width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                    if (width <= 0.0)
                    {
                        width = 1.0;
                    }
                    ocsBasePoint = firstAlignmentPoint;
                    break;
                default:
                    ocsBasePoint = secondAlignmentPoint;
                    break;
            }

            textString = this.DecodeEncodedNonAsciiCharacters(textString);
            Text text = new Text
            {
                Value = textString,
                Height = height,
                Width = width,
                WidthFactor = widthFactor,
                Rotation = rotation,
                ObliqueAngle = obliqueAngle,
                IsBackward = isBackward,
                IsUpsideDown = isUpsideDown,
                Style = style,
                Position = MathHelper.Transform(ocsBasePoint, normal, CoordinateSystem.Object, CoordinateSystem.World),
                Normal = normal,
                Alignment = alignment
            };

            text.XData.AddRange(xData);

            return text;
        }

        private MText ReadMText()
        {
            Vector3 insertionPoint = Vector3.Zero;
            Vector3 direction = Vector3.UnitX;
            Vector3 normal = Vector3.UnitZ;
            double height = 0.0;
            double rectangleWidth = 0.0;
            double lineSpacing = 1.0;
            double rotation = 0.0;
            bool isRotationDefined = false;
            MTextAttachmentPoint attachmentPoint = MTextAttachmentPoint.TopLeft;
            MTextLineSpacingStyle spacingStyle = MTextLineSpacingStyle.AtLeast;
            MTextDrawingDirection drawingDirection = MTextDrawingDirection.ByStyle;
            TextStyle style = TextStyle.Default;
            string textString = string.Empty;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        textString = string.Concat(textString, this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 3:
                        textString = string.Concat(textString, this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 10:
                        insertionPoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        insertionPoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 30:
                        insertionPoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        direction.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        direction.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 31:
                        direction.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 40:
                        height = this.chunk.ReadDouble();
                        if (height <= 0.0)
                        {
                            height = this.doc.DrawingVariables.TextSize;
                        }
                        this.chunk.Next();
                        break;
                    case 41:
                        rectangleWidth = this.chunk.ReadDouble();
                        if (rectangleWidth < 0.0)
                        {
                            rectangleWidth = 0.0;
                        }
                        this.chunk.Next();
                        break;
                    case 44:
                        lineSpacing = this.chunk.ReadDouble();
                        if (lineSpacing < 0.25 || lineSpacing > 4.0)
                        {
                            lineSpacing = 1.0;
                        }
                        this.chunk.Next();
                        break;
                    case 50: // even if the AutoCAD DXF documentation says that the rotation is in radians, this is wrong this value is in degrees
                        isRotationDefined = true;
                        rotation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 7:
                        string styleName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                        {
                            styleName = this.doc.DrawingVariables.TextStyle;
                        }
                        style = this.GetTextStyle(styleName);
                        this.chunk.Next();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        drawingDirection = (MTextDrawingDirection)this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        spacingStyle = (MTextLineSpacingStyle) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    case 101:
                        // once again Autodesk not documenting its own stuff.
                        // the code 101 was introduced in AutoCad 2018, as far as I know, it is not documented anywhere in the official DXF help.
                        // after this value, it seems that appears the definition of who knows what, therefore everything after this 101 code will be skipped
                        // until the end of the entity definition or the XData information
                        //string unknown = this.chunk.ReadString();
                        while (!(this.chunk.Code == 0 || this.chunk.Code == 1001))
                        {
                            this.chunk.Next();
                        }                                         
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            textString = this.DecodeEncodedNonAsciiCharacters(textString);
            if (!this.isBinary)
            {
                // text DXF files stores the tabs as ^I in the MText texts, they will be replaced by the standard tab character
                textString = textString.Replace("^I", "\t");
                // "^J" undocumented code in MText, they will be replaced by the standard end paragraph command "\P"
                textString = textString.Replace("^J", "\\P");
            }

            Vector3 ocsDirection = MathHelper.Transform(direction, normal, CoordinateSystem.World, CoordinateSystem.Object);

            MText entity = new MText
            {
                Value = textString,
                Position = insertionPoint,
                Height = height,
                RectangleWidth = rectangleWidth,
                Style = style,
                LineSpacingFactor = lineSpacing,
                AttachmentPoint = attachmentPoint,
                LineSpacingStyle = spacingStyle,
                DrawingDirection = drawingDirection,
                Rotation = isRotationDefined ? rotation : Vector2.Angle(new Vector2(ocsDirection.X, ocsDirection.Y))*MathHelper.RadToDeg,
                Normal = normal,
            };

            entity.XData.AddRange(xData);

            return entity;
        }

        private Hatch ReadHatch()
        {
            string name = string.Empty;
            HatchFillType fill = HatchFillType.SolidFill;
            double elevation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            HatchPattern pattern = HatchPattern.Line;
            bool associative = false;
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 30:
                        elevation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 210:
                        normal.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 220:
                        normal.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 230:
                        normal.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 91:
                        // the next lines hold the information about the hatch boundary paths
                        int numPaths = this.chunk.ReadInt();
                        paths = this.ReadHatchBoundaryPaths(numPaths);
                        break;
                    case 70:
                        // Solid fill flag
                        fill = (HatchFillType) this.chunk.ReadShort();
                        //if (fill == HatchFillType.SolidFill) name = PredefinedHatchPatternName.Solid;
                        this.chunk.Next();
                        break;
                    case 71:
                        // Associativity flag (associative = 1; non-associative = 0); for MPolygon, solid-fill flag (has solid fill = 1; lacks solid fill = 0)
                        if (this.chunk.ReadShort() != 0)
                            associative = true;
                        this.chunk.Next();
                        break;
                    case 75:
                        // the next lines hold the information about the hatch pattern
                        pattern = this.ReadHatchPattern(name);
                        pattern.Fill = fill;
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (paths.Count == 0)
                return null;

            Hatch entity = new Hatch(pattern, new List<HatchBoundaryPath>(), associative)
            {
                Elevation = elevation,
                Normal = normal
            };

            this.hatchToPaths.Add(entity, paths);

            entity.XData.AddRange(xData);

            // here is where DXF stores the pattern origin
            Vector2 origin = Vector2.Zero;
            if (entity.XData.TryGetValue(ApplicationRegistry.DefaultName, out XData patternOrigin))
            {
                foreach (XDataRecord record in patternOrigin.XDataRecord)
                {
                    if (record.Code == XDataCode.RealX)
                    {
                        origin.X = (double) record.Value;
                    }
                    else if (record.Code == XDataCode.RealY)
                    {
                        origin.Y = (double) record.Value;
                    }
                    // record.Code == XDataCode.RealZ is always 0
                }
            }
            pattern.Origin = origin;

            return entity;
        }

        private List<HatchBoundaryPath> ReadHatchBoundaryPaths(int numPaths)
        {
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            HatchBoundaryPathTypeFlags pathType = HatchBoundaryPathTypeFlags.Derived | HatchBoundaryPathTypeFlags.External;

            this.chunk.Next();
            while (paths.Count < numPaths)
            {
                HatchBoundaryPath path;
                switch (this.chunk.Code)
                {
                    case 92:
                        pathType = (HatchBoundaryPathTypeFlags) this.chunk.ReadInt();
                        // adding External and Derived to all path type flags solves an strange problem with code 98 not found,
                        // it seems related to the code 47 that appears before, only some combinations of flags are affected
                        // this is what the documentation says about code 47:
                        // Pixel size used to determine the density to perform various intersection and ray casting operations
                        // in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
                        pathType = pathType | HatchBoundaryPathTypeFlags.External | HatchBoundaryPathTypeFlags.Derived;

                        if (pathType.HasFlag(HatchBoundaryPathTypeFlags.Polyline))
                        {
                            path = this.ReadEdgePolylineBoundaryPath();
                            path.PathType = pathType;
                            paths.Add(path);
                        }
                        else
                            this.chunk.Next();
                        break;
                    case 93:
                        int numEdges = this.chunk.ReadInt();
                        path = this.ReadEdgeBoundaryPath(numEdges);
                        path.PathType = pathType;
                        paths.Add(path);
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }
            return paths;
        }

        private HatchBoundaryPath ReadEdgePolylineBoundaryPath()
        {
            HatchBoundaryPath.Polyline poly = new HatchBoundaryPath.Polyline();

            this.chunk.Next();

            bool hasBulge = this.chunk.ReadShort() != 0; // code 72
            this.chunk.Next();

            // is polyline closed
            // poly.IsClosed = this.chunk.ReadShort() != 0; // code 73, this value must always be true
            this.chunk.Next();

            int numVertexes = this.chunk.ReadInt(); // code 93
            poly.Vertexes = new Vector3[numVertexes];
            this.chunk.Next();

            for (int i = 0; i < numVertexes; i++)
            {
                double bulge = 0.0;
                double x = this.chunk.ReadDouble(); // code 10
                this.chunk.Next();
                double y = this.chunk.ReadDouble(); // code 20
                this.chunk.Next();
                if (hasBulge)
                {
                    bulge = this.chunk.ReadDouble(); // code 42
                    this.chunk.Next();
                }
                poly.Vertexes[i] = new Vector3(x, y, bulge);
            }
            HatchBoundaryPath path = new HatchBoundaryPath(new List<HatchBoundaryPath.Edge> {poly});

            // read all referenced entities
            Debug.Assert(this.chunk.Code == 97, "The reference count code 97 was expected.");
            int numBoundaryObjects = this.chunk.ReadInt();
            this.hatchContours.Add(path, new List<string>(numBoundaryObjects));
            this.chunk.Next();
            for (int i = 0; i < numBoundaryObjects; i++)
            {
                Debug.Assert(this.chunk.Code == 330, "The reference handle code 330 was expected.");
                this.hatchContours[path].Add(this.chunk.ReadString());
                this.chunk.Next();
            }

            return path;
        }

        private HatchBoundaryPath ReadEdgeBoundaryPath(int numEdges)
        {
            // the information of the boundary path data always appear exactly as it is read
            List<HatchBoundaryPath.Edge> entities = new List<HatchBoundaryPath.Edge>();
            this.chunk.Next();

            while (entities.Count < numEdges)
            {
                // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline
                HatchBoundaryPath.EdgeType type = (HatchBoundaryPath.EdgeType) this.chunk.ReadShort();
                switch (type)
                {
                    case HatchBoundaryPath.EdgeType.Line:
                        this.chunk.Next();
                        // line
                        double lX1 = this.chunk.ReadDouble(); // code 10
                        this.chunk.Next();
                        double lY1 = this.chunk.ReadDouble(); // code 20
                        this.chunk.Next();
                        double lX2 = this.chunk.ReadDouble(); // code 11
                        this.chunk.Next();
                        double lY2 = this.chunk.ReadDouble(); // code 21
                        this.chunk.Next();

                        HatchBoundaryPath.Line line = new HatchBoundaryPath.Line
                        {
                            Start = new Vector2(lX1, lY1),
                            End = new Vector2(lX2, lY2)
                        };
                        entities.Add(line);
                        break;
                    case HatchBoundaryPath.EdgeType.Arc:
                        this.chunk.Next();
                        // circular arc
                        double aX = this.chunk.ReadDouble(); // code 10
                        this.chunk.Next();
                        double aY = this.chunk.ReadDouble(); // code 40
                        this.chunk.Next();
                        double aR = this.chunk.ReadDouble(); // code 40
                        this.chunk.Next();
                        double aStart = this.chunk.ReadDouble(); // code 50
                        this.chunk.Next();
                        double aEnd = this.chunk.ReadDouble(); // code 51
                        this.chunk.Next();
                        bool aCCW = this.chunk.ReadShort() != 0; // code 73
                        this.chunk.Next();

                        HatchBoundaryPath.Arc arc = new HatchBoundaryPath.Arc
                        {
                            Center = new Vector2(aX, aY),
                            Radius = aR,
                            StartAngle = aStart,
                            EndAngle = aEnd,
                            IsCounterclockwise = aCCW
                        };
                        entities.Add(arc);
                        break;
                    case HatchBoundaryPath.EdgeType.Ellipse:
                        this.chunk.Next();
                        // elliptic arc
                        double eX = this.chunk.ReadDouble(); // code 10
                        this.chunk.Next();
                        double eY = this.chunk.ReadDouble(); // code 20
                        this.chunk.Next();
                        double eAxisX = this.chunk.ReadDouble(); // code 11
                        this.chunk.Next();
                        double eAxisY = this.chunk.ReadDouble(); // code 21
                        this.chunk.Next();
                        double eAxisRatio = this.chunk.ReadDouble(); // code 40
                        this.chunk.Next();
                        double eStart = this.chunk.ReadDouble(); // code 50
                        this.chunk.Next();
                        double eEnd = this.chunk.ReadDouble(); // code 51
                        this.chunk.Next();
                        bool eCCW = this.chunk.ReadShort() != 0; // code 73
                        this.chunk.Next();

                        HatchBoundaryPath.Ellipse ellipse = new HatchBoundaryPath.Ellipse
                        {
                            Center = new Vector2(eX, eY),
                            EndMajorAxis = new Vector2(eAxisX, eAxisY),
                            MinorRatio = eAxisRatio,
                            StartAngle = eStart,
                            EndAngle = eEnd,
                            IsCounterclockwise = eCCW
                        };

                        entities.Add(ellipse);
                        break;
                    case HatchBoundaryPath.EdgeType.Spline:
                        this.chunk.Next();
                        // spline

                        short degree = (short) this.chunk.ReadInt(); // code 94
                        this.chunk.Next();

                        bool isRational = this.chunk.ReadShort() != 0; // code 73
                        this.chunk.Next();

                        bool isPeriodic = this.chunk.ReadShort() != 0; // code 74
                        this.chunk.Next();

                        int numKnots = this.chunk.ReadInt(); // code 95
                        double[] knots = new double[numKnots];
                        this.chunk.Next();

                        int numControlPoints = this.chunk.ReadInt(); // code 96
                        Vector3[] controlPoints = new Vector3[numControlPoints];
                        this.chunk.Next();

                        for (int i = 0; i < numKnots; i++)
                        {
                            knots[i] = this.chunk.ReadDouble(); // code 40
                            this.chunk.Next();
                        }

                        for (int i = 0; i < numControlPoints; i++)
                        {
                            double x = this.chunk.ReadDouble(); // code 10
                            this.chunk.Next();

                            double y = this.chunk.ReadDouble(); // code 20
                            this.chunk.Next();

                            // control point weight might not be present
                            double w = 1.0;
                            if (this.chunk.Code == 42)
                            {
                                w = this.chunk.ReadDouble(); // code 42
                                this.chunk.Next();
                            }

                            controlPoints[i] = new Vector3(x, y, w);
                        }

                        // this information is only required for AutoCAD version 2010 and newer
                        // stores information about spline fit point (the spline entity does not make use of this information)
                        if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2010)
                        {
                            int numFitData = this.chunk.ReadInt(); // code 97
                            this.chunk.Next();
                            for (int i = 0; i < numFitData; i++)
                            {
                                //double fitX = this.chunk.ReadDouble(); // code 11
                                this.chunk.Next();
                                //double fitY = this.chunk.ReadDouble(); // code 21
                                this.chunk.Next();
                            }

                            // the info on start tangent might not appear
                            if (this.chunk.Code == 12)
                            {
                                //double startTanX = this.chunk.ReadDouble(); // code 12
                                this.chunk.Next();
                                //double startTanY = this.chunk.ReadDouble(); // code 22
                                this.chunk.Next();
                            }
                            // the info on end tangent might not appear
                            if (this.chunk.Code == 13)
                            {
                                //double endTanX = this.chunk.ReadDouble(); // code 13
                                this.chunk.Next();
                                //double endTanY = this.chunk.ReadDouble(); // code 23
                                this.chunk.Next();
                            }
                        }

                        HatchBoundaryPath.Spline spline = new HatchBoundaryPath.Spline
                        {
                            Degree = degree,
                            IsPeriodic = isPeriodic,
                            IsRational = isRational,
                            ControlPoints = controlPoints,
                            Knots = knots
                        };

                        entities.Add(spline);
                        break;
                }
            }

            HatchBoundaryPath path = new HatchBoundaryPath(entities);

            // read all referenced entities
            Debug.Assert(this.chunk.Code == 97, "The reference count code 97 was expected.");
            int numBoundaryObjects = this.chunk.ReadInt();
            this.hatchContours.Add(path, new List<string>(numBoundaryObjects));
            this.chunk.Next();
            for (int i = 0; i < numBoundaryObjects; i++)
            {
                Debug.Assert(this.chunk.Code == 330, "The reference handle code 330 was expected.");
                this.hatchContours[path].Add(this.chunk.ReadString());
                this.chunk.Next();
            }

            return path;
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

            while (this.chunk.Code != 0 && this.chunk.Code != 1001)
            {
                switch (this.chunk.Code)
                {
                    case 52:
                        angle = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        scale = this.chunk.ReadDouble();
                        if (scale <= 0)
                            scale = 1.0;
                        this.chunk.Next();
                        break;
                    case 47:
                        this.chunk.Next();
                        break;
                    case 98:
                        this.chunk.Next();
                        break;
                    case 10:
                        this.chunk.Next();
                        break;
                    case 20:
                        this.chunk.Next();
                        break;
                    case 75:
                        style = (HatchStyle) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 76:
                        type = (HatchType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 77:
                        // hatch pattern double flag (not used)
                        this.chunk.Next();
                        break;
                    case 78:
                        // number of pattern definition lines
                        short numLines = this.chunk.ReadShort();
                        lineDefinitions = this.ReadHatchPatternDefinitionLine(scale, angle, numLines);
                        break;
                    case 450:
                        if (this.chunk.ReadInt() == 1)
                        {
                            isGradient = true; // gradient pattern
                            hatch = this.ReadHatchGradientPattern();
                        }
                        else
                            this.chunk.Next(); // solid hatch, we do not need to read anything else
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            if (!isGradient)
            {
                hatch = new HatchPattern(name);
            }

            hatch.Angle = angle;
            hatch.Style = style;
            hatch.Scale = scale;
            hatch.Type = type;
            hatch.LineDefinitions.AddRange(lineDefinitions);
            return hatch;
        }

        private HatchGradientPattern ReadHatchGradientPattern()
        {
            // the information for gradient pattern must follow an strict order
            //dxfPairInfo = this.ReadCodePair();  // code 450 not needed
            this.chunk.Next(); // code 451 not needed
            this.chunk.Next();
            double angle = this.chunk.ReadDouble(); // code 460
            this.chunk.Next();
            bool centered = (int) this.chunk.ReadDouble() == 0; // code 461
            this.chunk.Next();
            bool singleColor = this.chunk.ReadInt() != 0; // code 452
            this.chunk.Next();
            double tint = this.chunk.ReadDouble(); // code 462
            this.chunk.Next(); // code 453 not needed

            this.chunk.Next(); // code 463 not needed (0.0)
            this.chunk.Next(); // code 63
            this.chunk.Next(); // code 421
            AciColor color1 = AciColor.FromTrueColor(this.chunk.ReadInt());

            this.chunk.Next(); // code 463 not needed (1.0)
            this.chunk.Next(); // code 63
            this.chunk.Next(); // code 421
            AciColor color2 = AciColor.FromTrueColor(this.chunk.ReadInt());

            this.chunk.Next(); // code 470
            string typeName = this.chunk.ReadString();
            
            HatchGradientPatternType type = StringEnum<HatchGradientPatternType>.Parse(typeName, StringComparison.OrdinalIgnoreCase);

            if (singleColor)
                return new HatchGradientPattern(color1, tint, type)
                {
                    Centered = centered,
                    Angle = angle*MathHelper.RadToDeg
                };

            return new HatchGradientPattern(color1, color2, type)
            {
                Centered = centered,
                Angle = angle*MathHelper.RadToDeg
            };
        }

        private List<HatchPatternLineDefinition> ReadHatchPatternDefinitionLine(double patternScale, double patternAngle, short numLines)
        {
            List<HatchPatternLineDefinition> lineDefinitions = new List<HatchPatternLineDefinition>();

            this.chunk.Next();
            for (int i = 0; i < numLines; i++)
            {
                Vector2 origin = Vector2.Zero;
                Vector2 delta = Vector2.Zero;

                double angle = this.chunk.ReadDouble(); // code 53
                this.chunk.Next();

                origin.X = this.chunk.ReadDouble(); // code 43
                this.chunk.Next();

                origin.Y = this.chunk.ReadDouble(); // code 44
                this.chunk.Next();

                delta.X = this.chunk.ReadDouble(); // code 45
                this.chunk.Next();

                delta.Y = this.chunk.ReadDouble(); // code 46
                this.chunk.Next();

                short numSegments = this.chunk.ReadShort(); // code 79
                this.chunk.Next();

                // Pattern fill data. In theory this should hold the same information as the pat file but for unknown reason the DXF requires global data instead of local.
                // this means we have to convert the global data into local, since we are storing the pattern line definition as it appears in the acad.pat file.
                double sinOrigin = Math.Sin(patternAngle*MathHelper.DegToRad);
                double cosOrigin = Math.Cos(patternAngle*MathHelper.DegToRad);
                origin = new Vector2(cosOrigin*origin.X/patternScale + sinOrigin*origin.Y/patternScale, -sinOrigin*origin.X/patternScale + cosOrigin*origin.Y/patternScale);

                double sinDelta = Math.Sin(angle*MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle*MathHelper.DegToRad);
                delta = new Vector2(cosDelta*delta.X/patternScale + sinDelta*delta.Y/patternScale, -sinDelta*delta.X/patternScale + cosDelta*delta.Y/patternScale);

                HatchPatternLineDefinition lineDefinition = new HatchPatternLineDefinition
                {
                    Angle = angle - patternAngle,
                    Origin = origin,
                    Delta = delta,
                };

                for (int j = 0; j < numSegments; j++)
                {
                    // positive values means solid segments and negative values means spaces (one entry per element)
                    lineDefinition.DashPattern.Add(this.chunk.ReadDouble()/patternScale); // code 49
                    this.chunk.Next();
                }

                lineDefinitions.Add(lineDefinition);
            }

            return lineDefinitions;
        }

        #endregion

        #region object methods

        private void CreateObjectCollection(DictionaryObject namedDict)
        {
            string groupsHandle = null;
            string layoutsHandle = null;
            string mlineStylesHandle = null;
            string imageDefsHandle = null;
            string underlayDgnDefsHandle = null;
            string underlayDwfDefsHandle = null;
            string underlayPdfDefsHandle = null;
            foreach (KeyValuePair<string, string> entry in namedDict.Entries)
            {
                switch (entry.Value)
                {
                    case DxfObjectCode.GroupDictionary:
                        groupsHandle = entry.Key;
                        break;
                    case DxfObjectCode.LayoutDictionary:
                        layoutsHandle = entry.Key;
                        break;
                    case DxfObjectCode.MLineStyleDictionary:
                        mlineStylesHandle = entry.Key;
                        break;
                    case DxfObjectCode.ImageDefDictionary:
                        imageDefsHandle = entry.Key;
                        break;
                    case DxfObjectCode.UnderlayDgnDefinitionDictionary:
                        underlayDgnDefsHandle = entry.Key;
                        break;
                    case DxfObjectCode.UnderlayDwfDefinitionDictionary:
                        underlayDwfDefsHandle = entry.Key;
                        break;
                    case DxfObjectCode.UnderlayPdfDefinitionDictionary:
                        underlayPdfDefsHandle = entry.Key;
                        break;
                }
            }

            // create the collections with the provided handles
            this.doc.Groups = new Groups(this.doc, groupsHandle);
            this.doc.Layouts = new Layouts(this.doc, layoutsHandle);
            this.doc.Layouts.XData.AddRange(namedDict.XData.Values);
            this.doc.MlineStyles = new MLineStyles(this.doc, mlineStylesHandle);
            this.doc.ImageDefinitions = new ImageDefinitions(this.doc, imageDefsHandle);
            this.doc.UnderlayDgnDefinitions = new UnderlayDgnDefinitions(this.doc, underlayDgnDefsHandle);
            this.doc.UnderlayDwfDefinitions = new UnderlayDwfDefinitions(this.doc, underlayDwfDefsHandle);
            this.doc.UnderlayPdfDefinitions = new UnderlayPdfDefinitions(this.doc, underlayPdfDefsHandle);
        }

        private DictionaryObject ReadDictionary()
        {
            List<XData> xData = new List<XData>();
            string handle = string.Empty;
            //string handleOwner = null;
            DictionaryCloningFlags cloning = DictionaryCloningFlags.KeepExisting;
            bool isHardOwner = false;
            int numEntries = 0;
            List<string> names = new List<string>();
            List<string> handlesToOwner = new List<string>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 330:
                        //handleOwner = this.chunk.ReadHandle();
                        this.chunk.Next();
                        break;
                    case 280:
                        isHardOwner = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 281:
                        cloning = (DictionaryCloningFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 3:
                        numEntries += 1;
                        names.Add(this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString()));
                        this.chunk.Next();
                        break;
                    case 350: // Soft-owner ID/handle to entry object 
                        handlesToOwner.Add(this.chunk.ReadHex());
                        this.chunk.Next();
                        break;
                    case 360:
                        // Hard-owner ID/handle to entry object
                        handlesToOwner.Add(this.chunk.ReadHex());
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(this.GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (this.chunk.Code >= 1000 && this.chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        this.chunk.Next();
                        break;
                }
            }

            //DxfObject owner = null;
            //if (handleOwner != null)
            //    owner = this.doc.AddedObjects[handleOwner];

            DictionaryObject dictionary = new DictionaryObject(null)
            {
                Handle = handle,
                IsHardOwner = isHardOwner,
                Cloning = cloning
            };

            for (int i = 0; i < numEntries; i++)
            {
                string id = handlesToOwner[i];
                if (id == null)
                {
                    throw new NullReferenceException("Null handle in dictionary.");
                }
                dictionary.Entries.Add(id, names[i]);
            }

            dictionary.XData.AddRange(xData);

            return dictionary;
        }

        private RasterVariables ReadRasterVariables()
        {
            RasterVariables variables = new RasterVariables(this.doc);
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        variables.Handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 70:
                        variables.DisplayFrame = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 71:
                        variables.DisplayQuality = (ImageDisplayQuality) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        variables.Units = (ImageUnits) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (this.chunk.Code >= 1000 && this.chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            variables.XData.AddRange(xData);

            return variables;
        }

        private ImageDefinition ReadImageDefinition()
        {
            string handle = null;
            string ownerHandle = null;
            string file = string.Empty;
            string name = string.Empty;
            double width = 0, height = 0;
            double wPixel = 0.0;
            double hPixel = 0.0;
            ImageResolutionUnits units = ImageResolutionUnits.Unitless;
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1:
                        file = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 10:
                        width = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        height = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        wPixel = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        hPixel = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 102:
                        if (this.chunk.ReadString().Equals("{ACAD_REACTORS", StringComparison.OrdinalIgnoreCase))
                        {
                            this.chunk.Next();
                            while (this.chunk.Code != 102)
                            {
                                // the first entry is the ownerHandle,
                                // the rest of the 330 values are not needed the IMAGEDEF_REACTOR already holds the handle to its corresponding IMAGEDEF
                                if (this.chunk.Code == 330 && string.IsNullOrEmpty(ownerHandle))
                                {
                                    ownerHandle = this.chunk.ReadHex();
                                }
                                this.chunk.Next();
                            }
                        }
                        this.chunk.Next();
                        break;
                    case 281:
                        units = (ImageResolutionUnits) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 330:
                        ownerHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (this.chunk.Code >= 1000 && this.chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(!string.IsNullOrEmpty(file), "File path is null or empty.");
            if (string.IsNullOrEmpty(file))
            {
                file = FileNotValid + ".JPG";
            }

            Debug.Assert(file.IndexOfAny(Path.GetInvalidPathChars()) == -1, "File path contains invalid characters: " + file);
            if (file.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                file = FileNotValid + ".JPG";
            }

            if (ownerHandle != null)
            {
                DictionaryObject imageDefDict = this.dictionaries[ownerHandle];
                if (handle == null)
                {
                    throw new NullReferenceException();
                }
                name = imageDefDict.Entries[handle];
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            // The documentation says that this is the size of one pixel in AutoCAD units, but it seems that this is always the size of one pixel in millimeters
            // this value is used to calculate the image resolution in DPI or DPC, and the default image size.
            // The documentation in this regard and its relation with the final image size in drawing units is a complete nonsense
            double factor = UnitHelper.ConversionFactor((ImageUnits) units, DrawingUnits.Millimeters);
            ImageDefinition imageDefinition = new ImageDefinition(name, file, (int) width, factor/wPixel, (int) height, factor/hPixel, units)
            {
                Handle = handle
            };
            imageDefinition.XData.AddRange(xData);

            if (string.IsNullOrEmpty(imageDefinition.Handle))
            {
                throw new NullReferenceException("Underlay reference definition handle.");
            }

            this.imgDefHandles.Add(imageDefinition.Handle, imageDefinition);
            return imageDefinition;
        }

        private ImageDefinitionReactor ReadImageDefReactor()
        {
            string handle = null;
            string imgOwner = null;

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 330:
                        imgOwner = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            return new ImageDefinitionReactor(imgOwner)
            {
                Handle = handle
            };
        }

        private MLineStyle ReadMLineStyle()
        {
            string handle = null;
            string name = string.Empty;
            string description = string.Empty;
            AciColor fillColor = AciColor.ByLayer;
            double startAngle = 90.0;
            double endAngle = 90.0;
            MLineStyleFlags flags = MLineStyleFlags.None;
            List<MLineStyleElement> elements = new List<MLineStyleElement>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 3:
                        description = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 62:
                        if (!fillColor.UseTrueColor)
                        {
                            fillColor = AciColor.FromCadIndex(this.chunk.ReadShort());
                        }
                        this.chunk.Next();
                        break;
                    case 420:
                        fillColor = AciColor.FromTrueColor(this.chunk.ReadInt());
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (MLineStyleFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 51:
                        startAngle = this.chunk.ReadDouble();
                        if (startAngle < 10.0 || startAngle > 170.0)
                        {
                            startAngle = 90.0;
                        }
                        this.chunk.Next();
                        break;
                    case 52:
                        endAngle = this.chunk.ReadDouble();
                        if (endAngle < 10.0 || endAngle > 170.0)
                        {
                            endAngle = 90.0;
                        }
                        this.chunk.Next();
                        break;
                    case 71:
                        short numElements = this.chunk.ReadShort();                       
                        elements = this.ReadMLineStyleElements(numElements);
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            MLineStyle style = new MLineStyle(name, elements, description)
            {
                Handle = handle,
                FillColor = fillColor,
                Flags = flags,
                StartAngle = startAngle,
                EndAngle = endAngle
            };

            style.XData.AddRange(xData);
            return style;
        }

        private List<MLineStyleElement> ReadMLineStyleElements(short numElements)
        {
            this.chunk.Next();

            if (numElements <= 0)
            {
                return new List<MLineStyleElement> {new MLineStyleElement(0.0)};
            }
            
            List<MLineStyleElement> elements = new List<MLineStyleElement>();

            for (short i = 0; i < numElements; i++)
            {
                double offset = this.chunk.ReadDouble(); // code 49
                this.chunk.Next();

                AciColor color = AciColor.FromCadIndex(this.chunk.ReadShort());
                this.chunk.Next();

                if (this.chunk.Code == 420)
                {
                    color = AciColor.FromTrueColor(this.chunk.ReadInt()); // code 420
                    this.chunk.Next();
                }

                string linetypeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString()); // code 6
                Linetype linetype = this.GetLinetype(linetypeName);
                this.chunk.Next();

                MLineStyleElement element = new MLineStyleElement(offset)
                {
                    Color = color,
                    Linetype = linetype
                };

                elements.Add(element);
            }

            return elements;
        }

        private Group ReadGroup()
        {
            string handle = null;
            string ownerHandle = null;
            string name = string.Empty;
            string description = string.Empty;
            bool isUnnamed = true;
            bool isSelectable = true;
            List<string> entities = new List<string>();
            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 330:
                        ownerHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 300:
                        description = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 70:
                        isUnnamed = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 71:
                        isSelectable = this.chunk.ReadShort() != 0;
                        this.chunk.Next();
                        break;
                    case 340:
                        string entity = this.chunk.ReadHex();
                        entities.Add(entity);
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            if (!string.IsNullOrEmpty(ownerHandle))
            {
                DictionaryObject dict = this.dictionaries[ownerHandle];
                Debug.Assert(!string.IsNullOrEmpty(handle), "Null handle in group dictionary.");

                if (string.IsNullOrEmpty(handle))
                {
                    return null;
                }
                name = dict.Entries[handle];
            }

            // we need to keep track of the group names generated
            if (isUnnamed)
            {
                this.CheckGroupName(name);
                Debug.Assert(!string.IsNullOrEmpty(name), "The object name cannot be null.");
                if (string.IsNullOrEmpty(name))
                {
                    return null;
                }
            }
            else
            {
                Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
                if (!TableObject.IsValidName(name))
                {
                    return null;
                }
            }

            Group group = new Group(name, false)
            {
                Handle = handle,
                Description = description,
                IsUnnamed = isUnnamed,
                IsSelectable = isSelectable
            };
            group.XData.AddRange(xData);

            // the group entities will be processed later
            this.groupEntities.Add(group, entities);

            return group;
        }

        private Layout ReadLayout()
        {
            PlotSettings plot = new PlotSettings();
            string handle = null;
            string name = string.Empty;
            short tabOrder = 1;
            Vector2 minLimit = new Vector2(-20.0, -7.5);
            Vector2 maxLimit = new Vector2(277.0, 202.5);
            Vector3 basePoint = Vector3.Zero;
            Vector3 minExtents = new Vector3(25.7, 19.5, 0.0);
            Vector3 maxExtents = new Vector3(231.3, 175.5, 0.0);
            double elevation = 0;
            Vector3 ucsOrigin = Vector3.Zero;
            Vector3 ucsXAxis = Vector3.UnitX;
            Vector3 ucsYAxis = Vector3.UnitY;
            string ownerRecordHandle = null;
            List<XData> xData = new List<XData>();

            string dxfCode = this.chunk.ReadString();
            this.chunk.Next();

            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of object {0} definition.", dxfCode));
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.chunk.Next();
                        break;
                    case 330:
                        //string owner = this.chunk.ReadHandle();
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 100:
                        if (this.chunk.ReadString() == SubclassMarker.PlotSettings)
                        {
                            plot = this.ReadPlotSettings();
                        }
                        this.chunk.Next();
                        break;
                    case 1:
                        name = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 71:
                        tabOrder = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 10:
                        minLimit.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 20:
                        minLimit.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 11:
                        maxLimit.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 21:
                        maxLimit.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 12:
                        basePoint.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 22:
                        basePoint.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 32:
                        basePoint.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 14:
                        minExtents.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 24:
                        minExtents.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 34:
                        minExtents.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 15:
                        maxExtents.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 25:
                        maxExtents.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 35:
                        maxExtents.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 146:
                        elevation = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 13:
                        ucsOrigin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 23:
                        ucsOrigin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 33:
                        ucsOrigin.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 16:
                        ucsXAxis.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 26:
                        ucsXAxis.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 36:
                        ucsXAxis.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 17:
                        ucsYAxis.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 27:
                        ucsYAxis.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 37:
                        ucsYAxis.Z = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 330:
                        ownerRecordHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            BlockRecord ownerRecord;
            if (!string.IsNullOrEmpty(handle))
            {
                if (this.blockRecordPointerToLayout.TryGetValue(handle, out ownerRecord))
                {
                    this.blockRecordPointerToLayout.Remove(handle);
                }
                else
                {
                    ownerRecord = this.doc.GetObjectByHandle(ownerRecordHandle) as BlockRecord;
                }
            }
            else
            {
                ownerRecord = this.doc.GetObjectByHandle(ownerRecordHandle) as BlockRecord;
            }

            if (ownerRecord != null)
            {
                if (this.doc.Blocks.GetReferences(ownerRecord.Name).Count > 0)
                {
                    ownerRecord = null; // the block is already in use
                }
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            Layout layout = new Layout(name)
            {
                PlotSettings = plot,
                Handle = handle,
                MinLimit = minLimit,
                MaxLimit = maxLimit,
                BasePoint = basePoint,
                MinExtents = minExtents,
                MaxExtents = maxExtents,
                Elevation = elevation,
                UcsOrigin = ucsOrigin,
                UcsXAxis = ucsXAxis,
                UcsYAxis = ucsYAxis,
                TabOrder = tabOrder > 0 ? tabOrder : (short) (this.doc.Layouts.Count + 1),
                AssociatedBlock = ownerRecord == null ? null : this.doc.Blocks[ownerRecord.Name]
            };

            layout.XData.AddRange(xData);
            return layout;
        }

        private PlotSettings ReadPlotSettings()
        {
            string pageName = string.Empty;
            string plotterName = "none_device";
            string paperSizeName = "ISO_A4_(210.00_x_297.00_MM)";
            string viewName = string.Empty;
            string styleSheet = string.Empty;
            double leftMargin = 7.5;
            double bottomMargin = 20.0;
            double rightMargin = 7.5;
            double topMargin = 20.0;

            Vector2 paperSize = new Vector2(210.0, 297.0);            
            Vector2 origin = Vector2.Zero;
            Vector2 windowUpRight = Vector2.Zero;
            Vector2 windowBottomLeft = Vector2.Zero;

            bool scaleToFit = true;
            double scaleNumerator = 1.0;
            double scaleDenominator = 1.0;
            PlotFlags flags = PlotFlags.DrawViewportsFirst | PlotFlags.PrintLineweights | PlotFlags.PlotPlotStyles | PlotFlags.UseStandardScale;
            PlotType plotType = PlotType.DrawingExtents;

            PlotPaperUnits paperUnits = PlotPaperUnits.Milimeters;
            PlotRotation paperRotation = PlotRotation.Degrees90;

            ShadePlotMode shadePlotMode = ShadePlotMode.AsDisplayed;
            ShadePlotResolutionMode shadePlotResolutionMode = ShadePlotResolutionMode.Normal;
            short shadePlotDPI = 300;
            Vector2 paperImageOrigin = Vector2.Zero;

            this.chunk.Next();
            while (this.chunk.Code != 100)
            {
                switch (this.chunk.Code)
                {
                    case 1:
                        pageName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 2:
                        plotterName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 4:
                        paperSizeName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 6:
                        viewName = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 7:
                        styleSheet = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 40:
                        leftMargin = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 41:
                        bottomMargin = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 42:
                        rightMargin = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 43:
                        topMargin = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 44:
                        paperSize.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 45:
                        paperSize.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 46:
                        origin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 47:
                        origin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 48:
                        windowBottomLeft.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 49:
                        windowUpRight.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 140:
                        windowBottomLeft.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 141:
                        windowUpRight.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 142:
                        scaleNumerator = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 143:
                        scaleDenominator = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 70:
                        flags = (PlotFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 72:
                        paperUnits = (PlotPaperUnits) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 73:
                        paperRotation = (PlotRotation) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 74:
                        plotType = (PlotType) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 75:
                        short plotScale = this.chunk.ReadShort();
                        scaleToFit = plotScale == 0;
                        this.chunk.Next();
                        break;
                    case 76:
                        shadePlotMode = (ShadePlotMode) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 77:
                        shadePlotResolutionMode = (ShadePlotResolutionMode) this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 78:
                        shadePlotDPI = this.chunk.ReadShort();
                        this.chunk.Next();
                        break;
                    case 148:
                        paperImageOrigin.X = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    case 149:
                        paperImageOrigin.Y = this.chunk.ReadDouble();
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }

            PlotSettings plot = new PlotSettings
            {
                PageSetupName = pageName,
                PlotterName = plotterName,
                PaperSizeName = paperSizeName,
                ViewName = viewName,
                CurrentStyleSheet = styleSheet,
                Origin = origin,
                PaperMargin = new PaperMargin(leftMargin, bottomMargin, rightMargin, topMargin),
                PaperSize = paperSize,
                WindowUpRight = windowUpRight,
                WindowBottomLeft = windowBottomLeft,
                ScaleToFit = scaleToFit,
                PrintScaleNumerator = scaleNumerator,
                PrintScaleDenominator = scaleDenominator,
                Flags = flags,
                PlotType = plotType,
                PaperUnits = paperUnits,
                PaperRotation = paperRotation,
                ShadePlotMode = shadePlotMode,
                ShadePlotResolutionMode = shadePlotResolutionMode,
                ShadePlotDPI = shadePlotDPI,
                PaperImageOrigin = paperImageOrigin
            };

            return plot;
        }

        private UnderlayDefinition ReadUnderlayDefinition(UnderlayType type)
        {
            string handle = null;
            string ownerHandle = null;
            string file = string.Empty;
            string name = string.Empty;
            string page = string.Empty;

            List<XData> xData = new List<XData>();

            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1:
                        file = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 2:
                        page = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        this.chunk.Next();
                        break;
                    case 330:
                        ownerHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 1001:
                        string appId = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        XData data = this.ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        Debug.Assert(!(this.chunk.Code >= 1000 && this.chunk.Code <= 1071), "The extended data of an entity must start with the application registry code.");
                        this.chunk.Next();
                        break;
                }
            }

            Debug.Assert(!string.IsNullOrEmpty(file), "File path is null or empty.");
            if (string.IsNullOrEmpty(file))
            {
                file = FileNotValid;
            }

            Debug.Assert(file.IndexOfAny(Path.GetInvalidPathChars()) == -1, "File path contains invalid characters: " + file);
            if (file.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                file = FileNotValid;
            }

            if (!string.IsNullOrEmpty(ownerHandle))
            {
                DictionaryObject underlayDefDict = this.dictionaries[ownerHandle];
                Debug.Assert(!string.IsNullOrEmpty(handle), "Null handle in underlay definition dictionary.");

                if (string.IsNullOrEmpty(handle))
                {
                    return null;
                }
                name = underlayDefDict.Entries[handle];
            }

            Debug.Assert(TableObject.IsValidName(name), "Table object name is not valid.");
            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            UnderlayDefinition underlayDef;
            string ext = Path.GetExtension(file);
            switch (type)
            {
                case UnderlayType.DGN:
                    Debug.Assert(ext.Equals(".DGN", StringComparison.OrdinalIgnoreCase), "The underlay type and the file extension do not match.");
                    
                    if (!ext.Equals(".DGN", StringComparison.OrdinalIgnoreCase))
                    {
                        file = Path.ChangeExtension(FileNotValid, ".DGN");
                    }
                    underlayDef = new UnderlayDgnDefinition(name, file)
                    {
                        Handle = handle,
                        Layout = page
                    };
                    break;
                case UnderlayType.DWF:
                    Debug.Assert(ext.Equals(".DWF", StringComparison.OrdinalIgnoreCase), "The underlay type and the file extension do not match.");

                    if (!ext.Equals(".DWF", StringComparison.OrdinalIgnoreCase))
                    {
                        file = Path.ChangeExtension(FileNotValid, ".DWF");
                    }
                    underlayDef = new UnderlayDwfDefinition(name, file)
                    {
                        Handle = handle
                    };
                    break;
                case UnderlayType.PDF:
                    Debug.Assert(ext.Equals(".PDF", StringComparison.OrdinalIgnoreCase), "The underlay type and the file extension do not match.");

                    if (!ext.Equals(".PDF", StringComparison.OrdinalIgnoreCase))
                    {
                        file = Path.ChangeExtension(FileNotValid, ".PDF");
                    }
                    underlayDef = new UnderlayPdfDefinition(name, file)
                    {
                        Handle = handle,
                        Page = page
                    };
                    break;
                default:
                    throw new NullReferenceException("Underlay reference definition.");
            }

            if (string.IsNullOrEmpty(underlayDef.Handle))
            {
                throw new NullReferenceException("Underlay reference definition handle.");
            }

            this.underlayDefHandles.Add(underlayDef.Handle, underlayDef);

            underlayDef.XData.AddRange(xData);

            return underlayDef;
        }

        private XRecord ReadXRecord()
        {
            string handle = null;
            string ownerHandle = null;
            DictionaryCloningFlags flags = DictionaryCloningFlags.KeepExisting;
            List<XRecordEntry> entries = new List<XRecordEntry>();
            this.chunk.Next();
            while (this.chunk.Code != 0)
            {
                switch (this.chunk.Code)
                {
                    case 5:
                        handle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 330:
                        ownerHandle = this.chunk.ReadHex();
                        this.chunk.Next();
                        break;
                    case 100:
                        this.chunk.Next();
                        flags = (DictionaryCloningFlags) this.chunk.ReadShort();
                        this.chunk.Next();
                        entries = this.XRecordEntries();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.chunk.Next();
                        break;
                    default:
                        this.chunk.Next();
                        break;
                }
            }
            XRecord xRecord = new XRecord
            {
                Handle = handle,
                OwnerHandle = ownerHandle,
                Flags = flags
            };

            xRecord.Entries.AddRange(entries);

            return xRecord;
        }

        private List<XRecordEntry> XRecordEntries()
        {
            List<XRecordEntry> entries = new List<XRecordEntry>();
            while (this.chunk.Code != 0)
            {
                //if (this.chunk.Code >= 1 && this.chunk.Code <= 369 && this.chunk.Code != 5 && this.chunk.Code != 105)
                //{
                    entries.Add(new XRecordEntry(this.chunk.Code, this.chunk.Value));
                //}
                this.chunk.Next();
            }

            return entries;
        }

        #endregion

        #region private methods

        private void ReadUnknowData()
        {
            do
            {
                this.chunk.Next();
            }
            while (this.chunk.Code != 0);
        }

        private void PostProcesses()
        {           
            // post process the dimension style list to assign the variables DIMTXSTY, DIMBLK, DIMBLK1, DIMBLK2, DIMLTYPE, DILTEX1, and DIMLTEX2
            foreach (KeyValuePair<DimensionStyle, string[]> pair in this.dimStyleToHandles)
            {
                DimensionStyle defaultDim = DimensionStyle.Default;

                BlockRecord record;

                record = this.doc.GetObjectByHandle(pair.Value[0]) as BlockRecord;
                pair.Key.DimArrow1 = record == null ? null : this.doc.Blocks[record.Name];

                record = this.doc.GetObjectByHandle(pair.Value[1]) as BlockRecord;
                pair.Key.DimArrow2 = record == null ? null : this.doc.Blocks[record.Name];

                record = this.doc.GetObjectByHandle(pair.Value[2]) as BlockRecord;
                pair.Key.LeaderArrow = record == null ? null : this.doc.Blocks[record.Name];

                TextStyle txtStyle;

                txtStyle = this.doc.GetObjectByHandle(pair.Value[3]) as TextStyle;
                pair.Key.TextStyle = txtStyle == null ? this.doc.TextStyles[defaultDim.TextStyle.Name] : this.doc.TextStyles[txtStyle.Name];

                Linetype ltype;

                ltype = this.doc.GetObjectByHandle(pair.Value[4]) as Linetype;
                pair.Key.DimLineLinetype = ltype == null ? this.doc.Linetypes[defaultDim.DimLineLinetype.Name] : this.doc.Linetypes[ltype.Name];

                ltype = this.doc.GetObjectByHandle(pair.Value[5]) as Linetype;
                pair.Key.ExtLine1Linetype = ltype == null ? this.doc.Linetypes[defaultDim.ExtLine1Linetype.Name] : this.doc.Linetypes[ltype.Name];

                ltype = this.doc.GetObjectByHandle(pair.Value[6]) as Linetype;
                pair.Key.ExtLine2Linetype = ltype == null ? this.doc.Linetypes[defaultDim.ExtLine2Linetype.Name] : this.doc.Linetypes[ltype.Name];
            }

            // post process the image list to assign their image definitions.
            foreach (KeyValuePair<Image, string> pair in this.imgToImgDefHandles)
            {
                Image image = pair.Key;

                image.Definition = this.imgDefHandles[pair.Value];

                // we still need to set the definitive image size, now that we know all units involved
                double factor = UnitHelper.ConversionFactor(this.doc.DrawingVariables.InsUnits, this.doc.RasterVariables.Units);
                image.Width *= factor;
                image.Height *= factor;
            }

            // post process the underlay definition list to assign their image definitions.
            foreach (KeyValuePair<Underlay, string> pair in this.underlayToDefinitionHandles)
            {
                Underlay underlay = pair.Key;
                underlay.Definition = this.underlayDefHandles[pair.Value];
            }

            // post process the MLines to assign their MLineStyle
            foreach (KeyValuePair<MLine, string> pair in this.mLineToStyleNames)
            {
                MLine mline = pair.Key;
                mline.Style = this.GetMLineStyle(pair.Value);
            }

            // post process the entities of the blocks
            foreach (KeyValuePair<Block, List<EntityObject>> pair in this.blockEntities)
            {
                Block block = pair.Key;
                foreach (EntityObject entity in pair.Value)
                {
                    // now that we have all information required by the block entities we can add them to the document
                    // entities like MLine and Image require information that is defined AFTER the block section,
                    // this is the case of the MLineStyle and ImageDefinition that are described in the objects section
                    block.Entities.Add(entity);
                }
            }

            // add the DXF entities to the document
            foreach (KeyValuePair<DxfObject, string> pair in this.entityList)
            {
                Layout layout;
                Block block;
                if (pair.Value == null)
                {
                    // the Model layout is the default in case the entity has not one defined
                    layout = this.doc.Layouts[Layout.ModelSpaceName];
                    block = layout.AssociatedBlock;
                }
                else
                {
                    block = this.GetBlock(((BlockRecord) this.doc.GetObjectByHandle(pair.Value)).Name);
                    layout = block.Record.Layout;
                }

                // the viewport with id 1 is stored directly in the layout since it has no graphical representation
                if (pair.Key is Viewport viewport)
                {
                    if (viewport.Id == 1)
                    {
                        // the base layout viewport has always id = 1 and we will not add it to the entities list of the document.
                        // this viewport has no graphical representation, it is the view of the paper space layout itself and it does not show the model.
                        layout.Viewport = viewport;
                        layout.Viewport.Owner = block;
                    }
                    else
                    {
                        this.doc.Blocks[layout.AssociatedBlock.Name].Entities.Add(viewport);
                    }
                }
                else
                {
                    // apply the units scale to the insertion scale (this is for not nested blocks)
                    if (pair.Key is Insert insert)
                    {
                        double scale = UnitHelper.ConversionFactor(this.doc.DrawingVariables.InsUnits, insert.Block.Record.Units);
                        insert.Scale *= scale;
                    }

                    if (pair.Key is AttributeDefinition attDef)
                    {
                        if (!layout.AssociatedBlock.AttributeDefinitions.ContainsTag(attDef.Tag))
                        {
                            layout.AssociatedBlock.AttributeDefinitions.Add(attDef);
                        }
                    }

                    if (pair.Key is EntityObject entity)
                    {
                        layout.AssociatedBlock.Entities.Add(entity);
                    }
                }
            }

            // assign a handle to the default layout viewports in case there was no layout viewport with ID=1 in the DXF
            foreach (Layout layout in this.doc.Layouts)
            {
                if (layout.Viewport == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(layout.Viewport.Handle))
                {
                    this.doc.NumHandles = layout.Viewport.AssignHandle(this.doc.NumHandles);
                }
            }
            // After loading a DXF the layouts associated blocks will be renamed to strictly follow *Paper_Space, *Paper_Space0, *Paper_Space1,... 
            this.doc.Layouts.RenameAssociatedBlocks();
            
            // post process viewports clipping boundaries
            foreach (KeyValuePair<Viewport, string> pair in this.viewports)
            {
                if (this.doc.GetObjectByHandle(pair.Value) is EntityObject entity)
                {
                    pair.Key.ClippingBoundary = entity;
                }
            }

            // post process the hatch boundary paths
            foreach (KeyValuePair<Hatch, List<HatchBoundaryPath>> pair in this.hatchToPaths)
            {
                Hatch hatch = pair.Key;
                foreach (HatchBoundaryPath path in pair.Value)
                {
                    List<string> entities = this.hatchContours[path];
                    foreach (string handle in entities)
                    {
                        if (this.doc.GetObjectByHandle(handle) is EntityObject entity)
                        {
                            if (ReferenceEquals(hatch.Owner, entity.Owner))
                            {
                                path.AddContour(entity);
                            }
                        }
                    }
                    hatch.BoundaryPaths.Add(path);
                }
            }

            // post process group entities
            foreach (KeyValuePair<Group, List<string>> pair in this.groupEntities)
            {
                foreach (string handle in pair.Value)
                {
                    if (this.doc.GetObjectByHandle(handle) is EntityObject entity)
                    {
                        pair.Key.Entities.Add(entity);
                    }
                }
            }

            // post process dimension style overrides,
            // it is stored in the dimension XData and the information stored there might contain handles to Linetypes, TextStyles and/or Blocks,
            // therefore is better process it at the end, when everything has been created read dimension style overrides
            foreach (Dimension dim in this.doc.Entities.Dimensions)
            {
                if (dim.XData.TryGetValue(ApplicationRegistry.DefaultName, out XData xDataOverrides))
                {
                    dim.StyleOverrides.AddRange(this.ReadDimensionStyleOverrideXData(xDataOverrides));
                }
            }
            foreach (Leader leader in this.doc.Entities.Leaders)
            {
                if (leader.XData.TryGetValue(ApplicationRegistry.DefaultName, out XData xDataOverrides))
                {
                    leader.StyleOverrides.AddRange(this.ReadDimensionStyleOverrideXData(xDataOverrides));
                }
            }

            // post process leader annotations
            foreach (KeyValuePair<Leader, string> pair in this.leaderAnnotation)
            {
                if (this.doc.GetObjectByHandle(pair.Value) is EntityObject entity)
                {
                    pair.Key.Annotation = entity;
                    //pair.Key.Update(true);
                }
            }

            // post process viewport frozen layers
            foreach (KeyValuePair<Viewport, List<Layer>> pair in this.viewportFrozenLayers)
            {
                foreach (Layer layer in pair.Value)
                {
                    if (layer == null)
                    {
                        continue;
                    }
                    if(pair.Key.FrozenLayers.Contains(layer))
                    {
                       continue; 
                    }
                    pair.Key.FrozenLayers.Add(layer);
                }
            }
        }

        private void BuildLayerStateManager()
        {
            if (string.IsNullOrEmpty(this.layerStateManagerDictionaryHandle))
            {
                return;
            }

            DictionaryObject lsMangerDictionary = this.dictionaries[this.layerStateManagerDictionaryHandle];
            if (lsMangerDictionary == null)
            {
                return;
            }

            DictionaryObject lsDictionary = this.dictionaries[lsMangerDictionary.Entries.Keys.ElementAt(0)];
            foreach (KeyValuePair<string, string> entry in lsMangerDictionary.Entries)
            {
                if (string.Equals(entry.Value, DxfObjectCode.LayerStates))
                {
                    lsDictionary = this.dictionaries[entry.Key];
                }
            }

            if (lsDictionary == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> entry in lsDictionary.Entries)
            {
                LayerState layerState = new LayerState(entry.Value);
                this.doc.Layers.StateManager.Add(layerState);

                XRecord ls = this.xRecords[entry.Key];

                using (IEnumerator<XRecordEntry> enumerator = ls.Entries.GetEnumerator())
                {
                    enumerator.MoveNext();

                    while (enumerator.Current != null)
                    {
                        XRecordEntry recordEntry = enumerator.Current;

                        LayerStateProperties properties;
                        switch (recordEntry.Code)
                        {
                            case 301:
                                layerState.Description = this.DecodeEncodedNonAsciiCharacters((string) recordEntry.Value);
                                enumerator.MoveNext();
                                break;
                            case 302:
                                string currentLayerName = this.DecodeEncodedNonAsciiCharacters((string) recordEntry.Value);
                                if (this.doc.Layers.Contains(currentLayerName))
                                {
                                    layerState.CurrentLayer = currentLayerName;
                                }

                                enumerator.MoveNext();
                                break;
                            case 290:
                                layerState.PaperSpace = (bool) recordEntry.Value;
                                enumerator.MoveNext();
                                break;
                            case 330:
                                DxfObject o = this.doc.GetObjectByHandle((string) recordEntry.Value);
                                if (o is Layer layer)
                                {
                                    properties = this.ReadLayerStateProperties(layer.Name, enumerator);
                                    // if the linetype does not exist in the document, the default will be assigned
                                    if (!this.doc.Linetypes.Contains(properties.LinetypeName))
                                    {
                                        properties.LinetypeName = Linetype.DefaultName;
                                    }

                                    if (!layerState.Properties.ContainsKey(properties.Name))
                                    {
                                        layerState.Properties.Add(properties.Name, properties);
                                    }
                                }
                                else
                                {
                                    enumerator.MoveNext();
                                }

                                break;
                            case 8:
                                properties = this.ReadLayerStateProperties((string) recordEntry.Value, enumerator);
                                // only layer properties that exist in the actual document will be added
                                if (this.doc.Layers.Contains(properties.Name))
                                {
                                    // if the linetype does not exist in the document, the default will be assigned
                                    if (!this.doc.Linetypes.Contains(properties.LinetypeName))
                                    {
                                        properties.LinetypeName = Linetype.DefaultName;
                                    }
                                    
                                    if (!layerState.Properties.ContainsKey(properties.Name))
                                    {
                                        layerState.Properties.Add(properties.Name, properties);
                                    }
                                }
                                enumerator.MoveNext();
                                break;
                            default:
                                enumerator.MoveNext();
                                break;
                        }
                    }
                }
            }
        }

        private LayerStateProperties ReadLayerStateProperties(string name, IEnumerator<XRecordEntry> enumerator)
        {
            LayerPropertiesFlags flags = LayerPropertiesFlags.Plot;
            string lineTypeName = Linetype.DefaultName;
            //string plotStyle = string.Empty;
            AciColor color = AciColor.Default;
            Lineweight lineweight = Lineweight.Default;
            Transparency transparency = new Transparency(0);

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    break;
                }

                XRecordEntry recordEntry = enumerator.Current;
                if (recordEntry.Code == 8 || recordEntry.Code == 330)
                {
                    break;
                }

                switch (recordEntry.Code)
                {
                    case 90:
                        flags = (LayerPropertiesFlags) (int) recordEntry.Value;
                        break;
                    case 62:
                        color = AciColor.FromCadIndex((short) recordEntry.Value);
                        break;
                    case 370:
                        lineweight = (Lineweight) (short) recordEntry.Value;
                        break;
                    case 331:
                        Linetype linetype = this.doc.GetObjectByHandle((string) recordEntry.Value) as Linetype;
                        Debug.Assert(linetype != null);
                        lineTypeName = linetype == null ? Linetype.DefaultName : linetype.Name;
                        break;
                    case 6:
                        // bad decision of storing the same information in two different codes (6 and 331)
                        // if there is a discrepancy between the values which one should be chosen?
                        lineTypeName = (string) recordEntry.Value;
                        break;
                    case 1:
                        //plotStyle = (string) recordEntry.Value;
                        break;
                    case 440:
                        int alpha = (int) recordEntry.Value;
                        transparency = alpha == 0 ? new Transparency(0) : Transparency.FromAlphaValue(alpha);
                        break;
                    case 92:
                        color = AciColor.FromTrueColor((int) recordEntry.Value);
                        break;
                }
            }

            return new LayerStateProperties(name)
            {
                Flags = flags,
                Color = color,
                Lineweight = lineweight,
                LinetypeName = lineTypeName,
                //PlotStyleName = plotStyle,
                Transparency = transparency
            };
        }

        private void RelinkOrphanLayouts()
        {
            // add any additional layouts corresponding to the PaperSpace block records not referenced by any layout
            // these are fixes for possible errors with layouts and their associated blocks. This should never happen.
            foreach (BlockRecord r in this.blockRecordPointerToLayout.Values)
            {
                Layout layout = null;

                // the *ModelSpace block must be linked to a layout called "Model"
                if (string.Equals(r.Name, Block.DefaultModelSpaceName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Layout l in this.orphanLayouts)
                    {
                        if (string.Equals(l.Name, Layout.ModelSpaceName, StringComparison.OrdinalIgnoreCase))
                        {
                            layout = l;
                            break;
                        }
                    }

                    if (layout == null)
                    {
                        // we will create a "Model" layout since we haven't found any
                        layout = Layout.ModelSpace;
                        this.doc.Layouts.Add(layout);
                        continue;
                    }
                    // we have a suitable layout
                    layout.AssociatedBlock = this.doc.Blocks[r.Name];
                    this.orphanLayouts.Remove(layout);
                    this.doc.Layouts.Add(layout);
                    continue;
                }

                // the *PaperSpace block cannot be linked with a layout called "Model"
                // check if we have any orphan layouts
                if (this.orphanLayouts.Count > 0)
                {
                    foreach (Layout l in this.orphanLayouts)
                    {
                        // find the first occurrence of a layout not named "Model"
                        if (!string.Equals(l.Name, Layout.ModelSpaceName, StringComparison.OrdinalIgnoreCase))
                        {
                            layout = l;
                            break;
                        }
                    }
                    if (layout != null)
                    {
                        // we have a suitable layout
                        layout = this.orphanLayouts[0];
                        layout.AssociatedBlock = this.doc.Blocks[r.Name];
                        this.doc.Layouts.Add(layout);
                        this.orphanLayouts.Remove(layout);
                        continue;
                    }
                }

                // create a new Layout in case there are now more suitable orphans
                short counter = 1;
                string layoutName = "Layout" + 1;

                while (this.doc.Layouts.Contains(layoutName))
                {
                    counter += 1;
                    layoutName = "Layout" + counter;
                }
                layout = new Layout(layoutName)
                {
                    TabOrder = (short) (this.doc.Layouts.Count + 1),
                    AssociatedBlock = this.doc.Blocks[r.Name]
                };

                this.doc.Layouts.Add(layout);
            }

            // if there are still orphan layouts add them to the list, it will create an associate block for them
            foreach (Layout orphan in this.orphanLayouts)
            {
                this.doc.Layouts.Add(orphan, false);
            }

            // add ModelSpace layout if it does not exist
            this.doc.Layouts.Add(Layout.ModelSpace);
        }

        private string ReadExtensionDictionaryGroup()
        {
            string handle = string.Empty;
            //string dictionaryGroup = this.chunk.ReadString().Remove(0, 1);

            this.chunk.Next();
            
            while (this.chunk.Code != 102)
            {
                switch (this.chunk.Code)
                {
                    //case 330:
                    //    handle = this.chunk.ReadHex();
                    //    break;
                    case 360:
                        handle = this.chunk.ReadHex();
                        break;
                }
                this.chunk.Next();
            }

            return handle;
        }

        private XData ReadXDataRecord(ApplicationRegistry appReg)
        {
            XData xData = new XData(appReg);
            this.chunk.Next();

            while (this.chunk.Code >= 1000 && this.chunk.Code <= 1071)
            {
                if (this.chunk.Code == (short) XDataCode.AppReg)
                    break;

                XDataCode code = (XDataCode) this.chunk.Code;
                object value = null;
                switch (code)
                {
                    case XDataCode.String:
                        value = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        break;
                    case XDataCode.AppReg:
                        // Application name cannot appear inside the extended data, AutoCAD assumes it is the beginning of a new application extended data group
                        break;
                    case XDataCode.ControlString:
                        value = this.chunk.ReadString();
                        break;
                    case XDataCode.LayerName:
                        value = this.DecodeEncodedNonAsciiCharacters(this.chunk.ReadString());
                        break;
                    case XDataCode.BinaryData:
                        value = this.chunk.ReadBytes();
                        break;
                    case XDataCode.DatabaseHandle:
                        value = this.chunk.ReadString();
                        break;
                    case XDataCode.RealX:
                    case XDataCode.RealY:
                    case XDataCode.RealZ:
                    case XDataCode.WorldSpacePositionX:
                    case XDataCode.WorldSpacePositionY:
                    case XDataCode.WorldSpacePositionZ:
                    case XDataCode.WorldSpaceDisplacementX:
                    case XDataCode.WorldSpaceDisplacementY:
                    case XDataCode.WorldSpaceDisplacementZ:
                    case XDataCode.WorldDirectionX:
                    case XDataCode.WorldDirectionY:
                    case XDataCode.WorldDirectionZ:
                    case XDataCode.Real:
                    case XDataCode.Distance:
                    case XDataCode.ScaleFactor:
                        value = this.chunk.ReadDouble();
                        break;
                    case XDataCode.Int16:
                        value = this.chunk.ReadShort();
                        break;
                    case XDataCode.Int32:
                        value = this.chunk.ReadInt();
                        break;
                }

                XDataRecord xDataRecord = new XDataRecord(code, value);
                xData.XDataRecord.Add(xDataRecord);
                this.chunk.Next();
            }

            return xData;
        }

        private string DecodeEncodedNonAsciiCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            if (this.decodedStrings.TryGetValue(text, out string decoded))
            {
                return decoded;
            }

            int length = text.Length;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                char c = text[i];
                if (c == '\\')
                {
                    if (i + 6 < length)
                    {
                        // \U+#### where #### is a four digits hexadecimal number
                        if ((text[i + 1] == 'U' || text[i + 1] == 'u') && text[i + 2] == '+')
                        {
                            string hex = text.Substring(i + 3, 4);
                            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int value))
                            {
                                c = (char) value;
                                i += 6;
                            }
                        }
                    }
                }
                sb.Append(c);
            }
            decoded = sb.ToString();

            // decoding of encoded non ASCII characters using regular expressions,
            // this code is slower depends on the number of non ASCII characters in the string
            //decoded = Regex.Replace(
            //    text,
            //    @"\\U\+(?<hex>[a-zA-Z0-9]{4})",
            //    m => ((char)int.Parse(m.Groups["hex"].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

            this.decodedStrings.Add(text, decoded);
            return decoded;
        }

        private void CheckDimBlockName(string name)
        {
            // the AutoCad block names has the form *D#
            // we need to find which is the last available number, in case more dimensions are added
            if (!name.StartsWith("*D", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out int num))
            {
                return;
            }

            if (num > this.doc.DimensionBlocksIndex)
            {
                this.doc.DimensionBlocksIndex = num;
            }
        }

        private void CheckGroupName(string name)
        {
            // the AutoCad group names has the form *A#
            // we need to find which is the last available number, in case more groups are added
            if (!name.StartsWith("*A", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out int num))
            {
                return;
            }

            if (num > this.doc.GroupNamesIndex)
            {
                this.doc.GroupNamesIndex = num;
            }
        }

        private static TextAlignment ObtainAlignment(short horizontal, short vertical)
        {
            TextAlignment alignment = TextAlignment.BaselineLeft;

            if (horizontal == 0 && vertical == 3)
            {
                alignment = TextAlignment.TopLeft;
            }
            else if (horizontal == 1 && vertical == 3)
            {
                alignment = TextAlignment.TopCenter;
            }
            else if (horizontal == 2 && vertical == 3)
            {
                alignment = TextAlignment.TopRight;
            }
            else if (horizontal == 0 && vertical == 2)
            {
                alignment = TextAlignment.MiddleLeft;
            }
            else if (horizontal == 1 && vertical == 2)
            {
                alignment = TextAlignment.MiddleCenter;
            }
            else if (horizontal == 2 && vertical == 2)
            {
                alignment = TextAlignment.MiddleRight;
            }
            else if (horizontal == 0 && vertical == 1)
            {
                alignment = TextAlignment.BottomLeft;
            }
            else if (horizontal == 1 && vertical == 1)
            {
                alignment = TextAlignment.BottomCenter;
            }
            else if (horizontal == 2 && vertical == 1)
            {
                alignment = TextAlignment.BottomRight;
            }
            else if (horizontal == 0 && vertical == 0)
            {
                alignment = TextAlignment.BaselineLeft;
            }

            if (horizontal == 1 && vertical == 0)
            {
                alignment = TextAlignment.BaselineCenter;
            }
            else if (horizontal == 2 && vertical == 0)
            {
                alignment = TextAlignment.BaselineRight;
            }
            else if (horizontal == 3 && vertical == 0)
            {
                alignment = TextAlignment.Aligned;
            }
            else if (horizontal == 4 && vertical == 0)
            {
                alignment = TextAlignment.Middle;
            }
            else if (horizontal == 5 && vertical == 0)
            {
                alignment = TextAlignment.Fit;
            }

            return alignment;
        }

        private ApplicationRegistry GetApplicationRegistry(string name)
        {
            if (this.doc.ApplicationRegistries.TryGetValue(name, out ApplicationRegistry appReg))
            {
                return appReg;
            }

            // if an entity references a table object not defined in the tables section a new one will be created
            return this.doc.ApplicationRegistries.Add(new ApplicationRegistry(name));
        }

        private Block GetBlock(string name)
        {
            if (this.doc.Blocks.TryGetValue(name, out Block block))
            {
                return block;
            }

            return this.doc.Blocks.Add(new Block(name));
        }

        private Layer GetLayer(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
            {
                name = Layer.DefaultName;
            }

            if (this.doc.Layers.TryGetValue(name, out Layer layer))
            {
                return layer;
            }

            // if an entity references a table object not defined in the tables section a new one will be created
            return this.doc.Layers.Add(new Layer(name));
        }

        private Linetype GetLinetype(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
            {
                name = Linetype.DefaultName;
            }

            if (this.doc.Linetypes.TryGetValue(name, out Linetype linetype))
            {
                return linetype;
            }
            else
            {
                foreach (Linetype complexLinetype in this.complexLinetypes)
                {
                    if (string.Equals(name, complexLinetype.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return complexLinetype;
                    }
                }
            }
            // if an entity references a table object not defined in the tables section a new one will be created
            return this.doc.Linetypes.Add(new Linetype(name));
        }

        private TextStyle GetTextStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
            {
                name = TextStyle.DefaultName;
            }

            if (this.doc.TextStyles.TryGetValue(name, out TextStyle style))
            {
                return style;
            }

            // if an entity references a table object not defined in the tables section a new one will be created
            return this.doc.TextStyles.Add(new TextStyle(name));
        }

        private DimensionStyle GetDimensionStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
            {
                name = DimensionStyle.DefaultName;
            }

            if (this.doc.DimensionStyles.TryGetValue(name, out DimensionStyle style))
            {
                return style;
            }

            // if an entity references a table object not defined in the tables section a new one will be created
            return this.doc.DimensionStyles.Add(new DimensionStyle(name));
        }

        private MLineStyle GetMLineStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
            {
                name = MLineStyle.DefaultName;
            }

            if (this.doc.MlineStyles.TryGetValue(name, out MLineStyle style))
            {
                return style;
            }

            return this.doc.MlineStyles.Add(new MLineStyle(name));
        }

        #endregion
    }
}