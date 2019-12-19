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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

        // here we will store strings already decoded <string: original, string: decoded>
        private Dictionary<string, string> decodedStrings;

        // blocks records <string: name, string: blockRecord>
        private Dictionary<string, BlockRecord> blockRecords;

        // entities, they will be processed at the end <Entity: entity, string: owner handle>.
        private Dictionary<DxfObject, string> entityList;

        // Viewports, they will be processed at the end <Entity: Viewport, string: clipping boundary handle>.
        private Dictionary<Viewport, string> viewports;

        private Dictionary<Hatch, List<HatchBoundaryPath>> hatchToPaths;
        // HatchBoundaryPaths, they will be processed at the end <Entity: HatchBoundaryPath, string: entity contourn handle>.
        private Dictionary<HatchBoundaryPath, List<string>> hatchContourns;

        // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
        // temporarily these variables will store information to post process the nested block list
        private Dictionary<Insert, string> nestedInserts;
        private Dictionary<Dimension, string> nestedDimensions;

        // the named dictionary is the only one we are interested, it is always the first that appears in the section
        // It consists solely of associated pairs of entry names and hard ownership pointer references to the associated object.
        private DictionaryObject namedDictionary;
        // <string: dictionary handle, DictionaryObject: dictionary>
        private Dictionary<string, DictionaryObject> dictionaries;

        private Dictionary<string, ImageDefinitionReactor> imageDefReactors;

        // variables for post-processing
        private Dictionary<Leader, string> leaderAnnotation;
        private Dictionary<Block, List<EntityObject>> blockEntities;
        private Dictionary<Group, List<string>> groupEntities;

        // the order of each table group in the tables section may vary
        private Dictionary<DimensionStyle, string[]> dimStyleToHandles;

        // complex linetypes for post-processing
        private List<Linetype> complexLinetypes;
        private Dictionary<LinetypeSegment, string> linetypeSegmentStyleHandles;
        private Dictionary<LinetypeShapeSegment, short> linetypeShapeSegmentToNumber;

        // XData for table objects
        private Dictionary<IHasXData, List<XData>> hasXData;

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
            long startPosition = stream.Position;

            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            DxfVersion version = DxfDocument.CheckDxfFileVersion(stream, out isBinary);
            stream.Position = startPosition;

            if (version < DxfVersion.AutoCad2000)
            {
                throw new DxfVersionNotSupportedException(string.Format("DXF file version not supported : {0}.", version), version);
            }

            string dwgcodepage = CheckHeaderVariable(stream, HeaderVariableCode.DwgCodePage, out isBinary);
            stream.Position = startPosition;

            try
            {
                if (isBinary)
                {
                    Encoding encoding;

                    if (version >= DxfVersion.AutoCad2007)
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(dwgcodepage))
                        {
                            // use the default windows code page, if unable to read the code page header variable.
                            encoding = Encoding.GetEncoding(Encoding.Default.WindowsCodePage);
                        }
                        else
                        {
                            int codepage;
                            encoding = Encoding.GetEncoding(int.TryParse(dwgcodepage.Split('_')[1], out codepage) ? codepage : Encoding.Default.WindowsCodePage);
                        }
                    }
                    chunk = new BinaryCodeValueReader(new BinaryReader(stream), encoding);
                }
                else
                {
                    Encoding encoding;
                    Encoding encodingType = EncodingType.GetType(stream);
                    stream.Position = startPosition;

                    bool isUnicode = (encodingType.EncodingName == Encoding.UTF8.EncodingName) ||
                                     (encodingType.EncodingName == Encoding.BigEndianUnicode.EncodingName) ||
                                     (encodingType.EncodingName == Encoding.Unicode.EncodingName);

                    if (isUnicode)
                    {
                        encoding = Encoding.UTF8;
                    }
                    else
                    {
                        // if the file is not UTF-8 use the code page provided by the DXF file
                        if (string.IsNullOrEmpty(dwgcodepage))
                        {
                            // use the default windows code page, if unable to read the code page header variable.
                            encoding = Encoding.GetEncoding(Encoding.Default.WindowsCodePage);
                        }
                        else
                        {
                            int codepage;
                            encoding = Encoding.GetEncoding(!int.TryParse(dwgcodepage.Split('_')[1], out codepage) ? Encoding.Default.WindowsCodePage : codepage);
                        }
                    }

                    chunk = new TextCodeValueReader(new StreamReader(stream, encoding, true));
                }
            }
            catch (Exception ex)
            {
                throw new IOException("Unknown error opening the reader.", ex);
            }

            doc = new DxfDocument(new HeaderVariables(), false, supportFolders);

            entityList = new Dictionary<DxfObject, string>();
            viewports = new Dictionary<Viewport, string>();
            hatchToPaths = new Dictionary<Hatch, List<HatchBoundaryPath>>();
            hatchContourns = new Dictionary<HatchBoundaryPath, List<string>>();
            decodedStrings = new Dictionary<string, string>();
            leaderAnnotation = new Dictionary<Leader, string>();

            //tables
            hasXData = new Dictionary<IHasXData, List<XData>>();
            dimStyleToHandles = new Dictionary<DimensionStyle, string[]>();
            complexLinetypes = new List<Linetype>();
            linetypeSegmentStyleHandles = new Dictionary<LinetypeSegment, string>();
            linetypeShapeSegmentToNumber = new Dictionary<LinetypeShapeSegment, short>();

            // blocks
            nestedInserts = new Dictionary<Insert, string>();
            nestedDimensions = new Dictionary<Dimension, string>();
            blockRecords = new Dictionary<string, BlockRecord>(StringComparer.OrdinalIgnoreCase);
            blockEntities = new Dictionary<Block, List<EntityObject>>();

            // objects
            dictionaries = new Dictionary<string, DictionaryObject>(StringComparer.OrdinalIgnoreCase);
            groupEntities = new Dictionary<Group, List<string>>();
            imageDefReactors = new Dictionary<string, ImageDefinitionReactor>(StringComparer.OrdinalIgnoreCase);
            imgDefHandles = new Dictionary<string, ImageDefinition>(StringComparer.OrdinalIgnoreCase);
            imgToImgDefHandles = new Dictionary<Image, string>();
            mLineToStyleNames = new Dictionary<MLine, string>();
            underlayToDefinitionHandles = new Dictionary<Underlay, string>();
            underlayDefHandles = new Dictionary<string, UnderlayDefinition>();

            // for layouts errors workarounds
            blockRecordPointerToLayout = new Dictionary<string, BlockRecord>(StringComparer.OrdinalIgnoreCase);
            orphanLayouts = new List<Layout>();

            chunk.Next();

            // read the comments at the head of the file, any other comments will be ignored
            // they sometimes hold information about the program that has generated the DXF
            // binary files do not contain any comments
            doc.Comments.Clear();

            while (chunk.Code == 999)
            {
                doc.Comments.Add(chunk.ReadString());
                chunk.Next();
            }

            while (chunk.ReadString() != DxfObjectCode.EndOfFile)
            {
                if (chunk.ReadString() == DxfObjectCode.BeginSection)
                {
                    chunk.Next();

                    switch (chunk.ReadString())
                    {
                        case DxfObjectCode.HeaderSection:
                            ReadHeader();
                            break;
                        case DxfObjectCode.ClassesSection:
                            ReadClasses();
                            break;
                        case DxfObjectCode.TablesSection:
                            ReadTables();
                            break;
                        case DxfObjectCode.BlocksSection:
                            ReadBlocks();
                            break;
                        case DxfObjectCode.EntitiesSection:
                            ReadEntities();
                            break;
                        case DxfObjectCode.ObjectsSection:
                            ReadObjects();
                            break;
                        case DxfObjectCode.ThumbnailImageSection:
                            ReadThumbnailImage();
                            break;
                        case DxfObjectCode.AcdsDataSection:
                            ReadAcdsData();
                            break;
                        default:
                            throw new Exception(string.Format("Unknown section {0}.", chunk.ReadString()));
                    }
                }

                chunk.Next();
            }

            // perform all necessary post processes
            PostProcesses();

            // to play safe we will add the default table objects to the document in case they do not exist,
            // if they already present nothing is overridden
            // add default layer
            doc.Layers.Add(Layer.Default);

            // add default line types
            doc.Linetypes.Add(Linetype.ByLayer);
            doc.Linetypes.Add(Linetype.ByBlock);
            doc.Linetypes.Add(Linetype.Continuous);

            // add default text style
            doc.TextStyles.Add(TextStyle.Default);

            // add default application registry
            doc.ApplicationRegistries.Add(ApplicationRegistry.Default);

            // add default dimension style
            doc.DimensionStyles.Add(DimensionStyle.Default);

            // add default MLine style
            doc.MlineStyles.Add(MLineStyle.Default);

            doc.ActiveLayout = Layout.ModelSpaceName;

            return doc;
        }

        public static bool IsBinary(Stream stream)
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
            long startPosition = stream.Position;
            ICodeValueReader chunk;
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
                            // we found the variable we are looking for
                            return chunk.ReadString();
                        }

                        // some header variables have more than one entry
                        while (chunk.Code != 0 && chunk.Code != 9)
                        {
                            chunk.Next();
                        }
                    }

                    // we only need to read the header section
                    return null;
                }
            }

            return null;
        }

        #endregion

        #region sections methods

        private void ReadHeader()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.HeaderSection);

            chunk.Next();

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                string varName = chunk.ReadString();
                double julian;
                chunk.Next();

                switch (varName)
                {
                    case HeaderVariableCode.AcadMaintVer:
                        doc.DrawingVariables.AcadMaintVer = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.AcadVer:
                        string version = chunk.ReadString();
                        DxfVersion acadVer = StringEnum<DxfVersion>.Parse(version, StringComparison.OrdinalIgnoreCase);

                        if (acadVer < DxfVersion.AutoCad2000)
                        {
                            throw new NotSupportedException("Only AutoCad2000 and higher DXF versions are supported.");
                        }

                        doc.DrawingVariables.AcadVer = acadVer;
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Angbase:
                        doc.DrawingVariables.Angbase = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Angdir:
                        doc.DrawingVariables.Angdir = (AngleDirection)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.AttMode:
                        doc.DrawingVariables.AttMode = (AttMode)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.AUnits:
                        doc.DrawingVariables.AUnits = (AngleUnitType)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.AUprec:
                        doc.DrawingVariables.AUprec = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CeColor:
                        doc.DrawingVariables.CeColor = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CeLtScale:
                        doc.DrawingVariables.CeLtScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CeLtype:
                        doc.DrawingVariables.CeLtype = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CeLweight:
                        doc.DrawingVariables.CeLweight = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CePsnId:
                        doc.DrawingVariables.CePsnId = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CePsnType:
                        doc.DrawingVariables.CePsnType = (PlotStyleType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ChamferA:
                        doc.DrawingVariables.ChamferA = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ChamferB:
                        doc.DrawingVariables.ChamferB = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ChamferC:
                        doc.DrawingVariables.ChamferC = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ChamferD:
                        doc.DrawingVariables.ChamferD = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CLayer:
                        doc.DrawingVariables.CLayer = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CMLJust:
                        doc.DrawingVariables.CMLJust = (MLineJustification) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CMLScale:
                        doc.DrawingVariables.CMLScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.CMLStyle:
                        string mLineStyleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());

                        if (!string.IsNullOrEmpty(mLineStyleName))
                        {
                            doc.DrawingVariables.CMLStyle = mLineStyleName;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.CShadow:
                        doc.DrawingVariables.CShadow = (ShadowMode) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAdec:
                        doc.DrawingVariables.DimAdec = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAlt:
                        doc.DrawingVariables.DimAlt = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltD:
                        doc.DrawingVariables.DimAltD = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltF:
                        doc.DrawingVariables.DimAltF = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltRnd:
                        doc.DrawingVariables.DimAltRnd = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltTd:
                        doc.DrawingVariables.DimAltTd = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltTz:
                        doc.DrawingVariables.DimAltTz = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltU:
                        doc.DrawingVariables.DimAltU = (UnitFormatsDimStyle)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAltZ:
                        doc.DrawingVariables.DimAltZ = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAPost:
                        doc.DrawingVariables.DimAPost = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAso:
                        doc.DrawingVariables.DimAso = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAssoc:
                        doc.DrawingVariables.DimAssoc = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAsz:
                        doc.DrawingVariables.DimAsz = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAtFit:
                        doc.DrawingVariables.DimAtFit = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAUnit:
                        doc.DrawingVariables.DimAUnit = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimAZin:
                        doc.DrawingVariables.DimAZin = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimBlk:
                        doc.DrawingVariables.DimBlk = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimBlk1:
                        doc.DrawingVariables.DimBlk1 = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimBlk2:
                        doc.DrawingVariables.DimBlk2 = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimCen:
                        doc.DrawingVariables.DimCen = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimClrd:
                        doc.DrawingVariables.DimClrd = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimClRe:
                        doc.DrawingVariables.DimClRe = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimClRt:
                        doc.DrawingVariables.DimClRt = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimDec:
                        doc.DrawingVariables.DimDec = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimDle:
                        doc.DrawingVariables.DimDle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimDli:
                        doc.DrawingVariables.DimDli = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimDsep:
                        doc.DrawingVariables.DimDsep = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimExE:
                        doc.DrawingVariables.DimExE = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimExO:
                        doc.DrawingVariables.DimExO = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimFac:
                        doc.DrawingVariables.DimFac = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimGap:
                        doc.DrawingVariables.DimGap = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimJust:
                        doc.DrawingVariables.DimJust = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLdrBlk:
                        doc.DrawingVariables.DimLdrBlk = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLfAc:
                        doc.DrawingVariables.DimLfAc = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLim:
                        doc.DrawingVariables.DimLim = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLUnit:
                        doc.DrawingVariables.DimLUnit = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLwD:
                        doc.DrawingVariables.DimLwD = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimLwE:
                        doc.DrawingVariables.DimLwE = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimPost:
                        doc.DrawingVariables.DimPost = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimRnd:
                        doc.DrawingVariables.DimRnd = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSah:
                        doc.DrawingVariables.DimSah = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimScale:
                        doc.DrawingVariables.DimScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSd1:
                        doc.DrawingVariables.DimSd1 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSd2:
                        doc.DrawingVariables.DimSd2 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSe1:
                        doc.DrawingVariables.DimSe1 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSe2:
                        doc.DrawingVariables.DimSe2 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSho:
                        doc.DrawingVariables.DimSho = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimSoXD:
                        doc.DrawingVariables.DimSoXD = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimStyle:
                        string dimStyleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());

                        if (!string.IsNullOrEmpty(dimStyleName))
                        {
                            doc.DrawingVariables.DimStyle = dimStyleName;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTad:
                        doc.DrawingVariables.DimTad = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTDec:
                        doc.DrawingVariables.DimTDec = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTFac:
                        doc.DrawingVariables.DimTFac = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTih:
                        doc.DrawingVariables.DimTih = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTix:
                        doc.DrawingVariables.DimTix = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTm:
                        doc.DrawingVariables.DimTm = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTMove:
                        doc.DrawingVariables.DimTMove = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimToFl:
                        doc.DrawingVariables.DimToFl = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimToH:
                        doc.DrawingVariables.DimToH = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimToL:
                        doc.DrawingVariables.DimToL = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimToLj:
                        doc.DrawingVariables.DimToLj = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTp:
                        doc.DrawingVariables.DimTp = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTsz:
                        doc.DrawingVariables.DimTsz = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTvp:
                        doc.DrawingVariables.DimTvp = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTxSty:
                        string dimTextStyleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());

                        if (!string.IsNullOrEmpty(dimTextStyleName))
                        {
                            doc.DrawingVariables.DimTxSty = dimTextStyleName;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTxt:
                        double sizeDimTxt = chunk.ReadDouble();

                        if (sizeDimTxt > 0.0)
                        {
                            doc.DrawingVariables.DimTxt = sizeDimTxt;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimTzIn:
                        doc.DrawingVariables.DimTzIn = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimUpt:
                        doc.DrawingVariables.DimUpt = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DimZIn:
                        doc.DrawingVariables.DimZIn = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DispSiLh:
                        doc.DrawingVariables.DispSiLh = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DragVs:
                        doc.DrawingVariables.DragVs = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.DwgCodePage:
                        doc.DrawingVariables.DwgCodePage = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Elevation:
                        doc.DrawingVariables.Elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.EndCaps:
                        doc.DrawingVariables.EndCaps = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ExtMax:
                        doc.DrawingVariables.ExtMax = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.ExtMin:
                        doc.DrawingVariables.ExtMin = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.Extnames:
                        doc.DrawingVariables.Extnames = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.FilletRad:
                        doc.DrawingVariables.FilletRad = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.FillMode:
                        doc.DrawingVariables.FillMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.FingerprintGuid:
                        doc.DrawingVariables.FingerprintGuid = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.HaloGap:
                        doc.DrawingVariables.HaloGap = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.HandleSeed:
                        string handleSeed = chunk.ReadHex();
                        doc.DrawingVariables.HandleSeed = handleSeed;
                        doc.NumHandles = long.Parse(handleSeed, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                        chunk.Next();
                        break;
                    case HeaderVariableCode.HideText:
                        doc.DrawingVariables.HideText = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.HyperlinkBase:
                        doc.DrawingVariables.HyperlinkBase = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.IndexCtl:
                        doc.DrawingVariables.IndexCtl = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.InsBase:
                        doc.DrawingVariables.InsBase = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.InsUnits:
                        doc.DrawingVariables.InsUnits = (DrawingUnits)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.InterfereColor:
                        doc.DrawingVariables.InterfereColor = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.InterfereObjVs:
                        doc.DrawingVariables.InterfereObjVs = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.InterfereVpVs:
                        doc.DrawingVariables.InterfereVpVs = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.IntersectionColor:
                        doc.DrawingVariables.IntersectionColor = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.IntersectionDisplay:
                        doc.DrawingVariables.IntersectionDisplay = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.JoinStyle:
                        doc.DrawingVariables.JoinStyle = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LimCheck:
                        doc.DrawingVariables.LimCheck = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LimMax:
                        doc.DrawingVariables.LimMax = ReadHeaderVector2();
                        break;
                    case HeaderVariableCode.LimMin:
                        doc.DrawingVariables.LimMin = ReadHeaderVector2();
                        break;
                    case HeaderVariableCode.LtScale:
                        doc.DrawingVariables.LtScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LUnits:
                        doc.DrawingVariables.LUnits = (LinearUnitType)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LUprec:
                        doc.DrawingVariables.LUprec = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LwDisplay:
                        doc.DrawingVariables.LwDisplay = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.MaxActVp:
                        doc.DrawingVariables.MaxActVp = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Measurement:
                        doc.DrawingVariables.Measurement = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Menu:
                        doc.DrawingVariables.Menu = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.MirrText:
                        short mirrtext = chunk.ReadShort();
                        doc.DrawingVariables.MirrText = mirrtext != 0;
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ObsColor:
                        doc.DrawingVariables.ObsColor = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ObsLtype:
                        doc.DrawingVariables.ObsLtype = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.OrthoMode:
                        doc.DrawingVariables.OrthoMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PdMode:
                        doc.DrawingVariables.PdMode = (PointShape)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PdSize:
                        doc.DrawingVariables.PdSize = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PElevation:
                        doc.DrawingVariables.PElevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PExtMax:
                        doc.DrawingVariables.PExtMax = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PExtMin:
                        doc.DrawingVariables.PExtMin = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PinsBase:
                        doc.DrawingVariables.PinsBase = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PLimCheck:
                        doc.DrawingVariables.PLimCheck = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PLimMax:
                        doc.DrawingVariables.PLimMax = ReadHeaderVector2();
                        break;
                    case HeaderVariableCode.PLimMin:
                        doc.DrawingVariables.PLimMin = ReadHeaderVector2();
                        break;
                    case HeaderVariableCode.PLineGen:
                        doc.DrawingVariables.PLineGen = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PLineWid:
                        doc.DrawingVariables.PLineWid = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Projectname:
                        doc.DrawingVariables.Projectname = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ProxyGraphics:
                        doc.DrawingVariables.ProxyGraphics = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PsLtScale:
                        doc.DrawingVariables.PsLtScale = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PStyleMode:
                        doc.DrawingVariables.PStyleMode = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PSvpScale:
                        doc.DrawingVariables.PSvpScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PUcsBase:
                        doc.DrawingVariables.PUcsBase = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PUcsName:
                        doc.DrawingVariables.PUcsName = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PUcsOrg:
                        doc.DrawingVariables.PUcsOrg = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgBack:
                        doc.DrawingVariables.PUcsOrgBack = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgBottom:
                        doc.DrawingVariables.PUcsOrgBottom = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgFront:
                        doc.DrawingVariables.PUcsOrgFront = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgLeft:
                        doc.DrawingVariables.PUcsOrgLeft = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgRight:
                        doc.DrawingVariables.PUcsOrgRight = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrgTop:
                        doc.DrawingVariables.PUcsOrgTop = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsOrthoRef:
                        doc.DrawingVariables.PUcsOrthoRef = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PUcsOrthoView:
                        doc.DrawingVariables.PUcsOrthoView = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.PUcsXDir:
                        doc.DrawingVariables.PUcsXDir = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.PUcsYDir:
                        doc.DrawingVariables.PUcsYDir = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.QTextMode:
                        doc.DrawingVariables.QTextMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.RegenMode:
                        doc.DrawingVariables.RegenMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ShadeEdge:
                        doc.DrawingVariables.ShadeEdge = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ShadeDif:
                        doc.DrawingVariables.ShadeDif = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.ShadowPlaneLocation:
                        doc.DrawingVariables.ShadowPlaneLocation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SketchInc:
                        doc.DrawingVariables.SketchInc = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SkPoly:
                        doc.DrawingVariables.SkPoly = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Sortents:
                        doc.DrawingVariables.Sortents = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SplinesEgs:
                        doc.DrawingVariables.SplinesEgs = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SplineType:
                        doc.DrawingVariables.SplineType = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SurfTab1:
                        doc.DrawingVariables.SurfTab1 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SurfTab2:
                        doc.DrawingVariables.SurfTab2 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SurfType:
                        doc.DrawingVariables.SurfType = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SurfU:
                        doc.DrawingVariables.SurfU = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.SurfV:
                        doc.DrawingVariables.SurfV = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.TdCreate:
                        julian = chunk.ReadDouble();

                        if (julian < 1721426 || julian > 5373484)
                        {
                            doc.DrawingVariables.TdCreate = DateTime.Now;
                        }
                        else
                        {
                            doc.DrawingVariables.TdCreate = DrawingTime.FromJulianCalendar(julian);
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TdinDwg:
                        double elapsed = chunk.ReadDouble();

                        if (elapsed < 0 || elapsed > TimeSpan.MaxValue.TotalDays)
                        {
                            doc.DrawingVariables.TdinDwg = TimeSpan.Zero;
                        }
                        else
                        {
                            doc.DrawingVariables.TdinDwg = DrawingTime.EditingTime(elapsed);
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TduCreate:
                        julian = chunk.ReadDouble();

                        if (julian < 1721426 || julian > 5373484)
                        {
                            doc.DrawingVariables.TduCreate = DateTime.Now;
                        }
                        else
                        {
                            doc.DrawingVariables.TduCreate = DrawingTime.FromJulianCalendar(julian);
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TdUpdate:
                        julian = chunk.ReadDouble();

                        if (julian < 1721426 || julian > 5373484)
                        {
                            doc.DrawingVariables.TdUpdate = DateTime.Now;
                        }
                        else
                        {
                            doc.DrawingVariables.TdUpdate = DrawingTime.FromJulianCalendar(julian);
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TdUsrTimer:
                        doc.DrawingVariables.TdUsrTimer = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.TduUpdate:
                        julian = chunk.ReadDouble();

                        if (julian < 1721426 || julian > 5373484)
                        {
                            doc.DrawingVariables.TduUpdate = DateTime.Now;
                        }
                        else
                        {
                            doc.DrawingVariables.TduUpdate = DrawingTime.FromJulianCalendar(julian);
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TextSize:
                        double size = chunk.ReadDouble();

                        if (size > 0.0)
                        {
                            doc.DrawingVariables.TextSize = size;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.TextStyle:
                        string textStyleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());

                        if (!string.IsNullOrEmpty(textStyleName))
                        {
                            doc.DrawingVariables.TextStyle = textStyleName;
                        }

                        chunk.Next();
                        break;
                    case HeaderVariableCode.Thickness:
                        doc.DrawingVariables.Thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.TileMode:
                        doc.DrawingVariables.TileMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.TraceWid:
                        doc.DrawingVariables.TraceWid = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.TreeDepth:
                        doc.DrawingVariables.TreeDepth = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UcsBase:
                        doc.DrawingVariables.UcsBase = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UcsName:
                        doc.DrawingVariables.UcsName = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UcsOrg:
                        doc.DrawingVariables.UcsOrg = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgBack:
                        doc.DrawingVariables.UcsOrgBack = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgBottom:
                        doc.DrawingVariables.UcsOrgBottom = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgFront:
                        doc.DrawingVariables.UcsOrgFront = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgLeft:
                        doc.DrawingVariables.UcsOrgLeft = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgRight:
                        doc.DrawingVariables.UcsOrgRight = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrgTop:
                        doc.DrawingVariables.UcsOrgTop = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsOrthoRef:
                        doc.DrawingVariables.UcsOrthoRef = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UcsOrthoView:
                        doc.DrawingVariables.UcsOrthoView = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UcsXDir:
                        doc.DrawingVariables.UcsXDir = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UcsYDir:
                        doc.DrawingVariables.UcsYDir = ReadHeaderVector();
                        break;
                    case HeaderVariableCode.UnitMode:
                        doc.DrawingVariables.UnitMode = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserI1:
                        doc.DrawingVariables.UserI1 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserI2:
                        doc.DrawingVariables.UserI2 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserI3:
                        doc.DrawingVariables.UserI3 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserI4:
                        doc.DrawingVariables.UserI4 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserI5:
                        doc.DrawingVariables.UserI5 = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserR1:
                        doc.DrawingVariables.UserR1 = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserR2:
                        doc.DrawingVariables.UserR2 = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserR3:
                        doc.DrawingVariables.UserR3 = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserR4:
                        doc.DrawingVariables.UserR4 = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserR5:
                        doc.DrawingVariables.UserR5 = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.UserTimer:
                        doc.DrawingVariables.UserTimer = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.VersionGuid:
                        doc.DrawingVariables.VersionGuid = chunk.ReadString();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.VisRetain:
                        doc.DrawingVariables.VisRetain = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.Worldview:
                        doc.DrawingVariables.Worldview = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.XClipFrame:
                        doc.DrawingVariables.XClipFrame = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.XEdit:
                        doc.DrawingVariables.XEdit = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case HeaderVariableCode.LastSavedBy:
                        doc.DrawingVariables.LastSavedBy = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    default:
                        // not recognized header variables will be added to the custom list
                        if (!varName.StartsWith("$DIM", StringComparison.InvariantCultureIgnoreCase))
                        { 
                            // except those that correspond to the dimension style
                            HeaderVariable variable;

                            if (chunk.Code == 10)
                            {
                                double x = chunk.ReadDouble();
                                chunk.Next();
                                double y = chunk.ReadDouble();
                                chunk.Next();

                                // the code 30 might not exist
                                if (chunk.Code == 30)
                                {
                                    double z = chunk.ReadDouble();
                                    chunk.Next();
                                    variable = new HeaderVariable(varName, 30, new Vector3(x, y, z));
                                }
                                else
                                {
                                    variable = new HeaderVariable(varName, 20, new Vector2(x, y));
                                }
                            }
                            else
                            {
                                variable = new HeaderVariable(varName, chunk.Code, chunk.Value);
                                chunk.Next();
                            }

                            //avoid duplicate custom header variables
                            if (doc.DrawingVariables.ContainsCustomVariable(varName))
                            {
                                doc.DrawingVariables.RemoveCustomVariable(varName);
                            }

                            doc.DrawingVariables.AddCustomVariable(variable);
                        }
                        else
                        {
                            // some header variables have more than one entry
                            while (chunk.Code != 0 && chunk.Code != 9)
                            {
                                chunk.Next();
                            }
                        }
                        break;
                }
            }

            if (!Vector3.ArePerpendicular(doc.DrawingVariables.UcsXDir, doc.DrawingVariables.UcsYDir))
            {
                doc.DrawingVariables.UcsXDir = Vector3.UnitX;
                doc.DrawingVariables.UcsYDir = Vector3.UnitY;
            }
        }

        private Vector3 ReadHeaderVector()
        {
            Vector3 pos = Vector3.Zero;

            while (chunk.Code != 0 && chunk.Code != 9)
            {
                switch (chunk.Code)
                {
                    case 10:
                        pos.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        pos.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        pos.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    default:
                        throw new Exception("Invalid code in vector header variable.");
                }
            }

            return pos;
        }

        /// <summary>
        /// Vexctor 2 XY
        /// </summary>
        /// <returns></returns>
        private Vector2 ReadHeaderVector2()
        {
            Vector2 pos = Vector2.Zero;

            while (chunk.Code != 0 && chunk.Code != 9)
            {
                switch (chunk.Code)
                {
                    case 10:
                        pos.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        pos.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    default:
                        throw new Exception("Invalid code in vector header variable.");
                }
            }

            return pos;
        }

        private void ReadClasses()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.ClassesSection);

            chunk.Next();

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                //read the class
                do
                    chunk.Next(); while (chunk.Code != 0);
            }
        }

        private void ReadTables()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.TablesSection);

            chunk.Next();
            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                ReadTable();
            }

            // check if all table collections has been created
            if (doc.ApplicationRegistries == null)
            {
                doc.ApplicationRegistries = new ApplicationRegistries(doc);
            }

            if (doc.Blocks == null)
            {
                doc.Blocks = new BlockRecords(doc);
            }

            if (doc.DimensionStyles == null)
            {
                doc.DimensionStyles = new DimensionStyles(doc);
            }

            if (doc.Layers == null)
            {
                doc.Layers = new Layers(doc);}

            if (doc.Linetypes == null)
            {
                doc.Linetypes = new Linetypes(doc);
            }

            if (doc.TextStyles == null)
            {
                doc.TextStyles = new TextStyles(doc);
            }

            if (doc.ShapeStyles == null)
            {
                doc.ShapeStyles = new ShapeStyles(doc);}

            if (doc.UCSs == null)
            {
                doc.UCSs = new UCSs(doc);
            }

            if (doc.Views == null)
            {
                doc.Views = new Views(doc);
            }

            if (doc.VPorts == null)
            {
                doc.VPorts = new VPorts(doc);
            }

            // post process complex linetypes
            foreach (KeyValuePair<LinetypeSegment, string> pair in linetypeSegmentStyleHandles)
            {
                switch (pair.Key.Type)
                {
                    case LinetypeSegmentType.Shape:
                        var shape = doc.GetObjectByHandle(pair.Value) as ShapeStyle;

                        if (shape != null)
                        {
                            ((LinetypeShapeSegment)pair.Key).Style = shape;
                        }

                        break;
                    case LinetypeSegmentType.Text:
                        var style = doc.GetObjectByHandle(pair.Value) as TextStyle;

                        if (style != null)
                        {
                            ((LinetypeTextSegment)pair.Key).Style = style;
                        }

                        break;
                }
            }

            List<LinetypeSegment> remove = new List<LinetypeSegment>();

            foreach (KeyValuePair<LinetypeShapeSegment, short> pair in linetypeShapeSegmentToNumber)
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
            foreach (Linetype complexLinetype in complexLinetypes)
            {
                // remove invalid linetype shape segments
                foreach (LinetypeSegment s in remove)
                {
                    complexLinetype.Segments.Remove(s);
                }

                doc.Linetypes.Add(complexLinetype, false);
            }

            //post process XData
            foreach (KeyValuePair<IHasXData, List<XData>> pair in hasXData)
            {
                IHasXData o = pair.Key;
                var appReg = o as ApplicationRegistry;

                if (appReg == null)
                {
                    pair.Key.XData.AddRange(pair.Value);
                }
                else
                {
                    doc.ApplicationRegistries[appReg.Name].XData.AddRange(pair.Value);
                }
            }
        }

        private void ReadBlocks()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.BlocksSection);

            // the blocks list will be added to the document after reading the blocks section to handle possible nested insert cases.
            Dictionary<string, Block> blocks = new Dictionary<string, Block>(StringComparer.OrdinalIgnoreCase);

            chunk.Next();

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                switch (chunk.ReadString())
                {
                    case DxfObjectCode.BeginBlock:
                        Block block = ReadBlock();
                        blocks.Add(block.Name, block);
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            // post process the possible nested blocks,
            // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the insert that references them
            foreach (KeyValuePair<Insert, string> pair in nestedInserts)
            {
                Insert insert = pair.Key;
                insert.Block = blocks[pair.Value];

                foreach (Attribute att in insert.Attributes)
                {
                    // attribute definitions might be null if an INSERT entity attribute has not been defined in the block
                    AttributeDefinition attDef;

                    if (insert.Block.AttributeDefinitions.TryGetValue(att.Tag, out attDef))
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
            foreach (KeyValuePair<Dimension, string> pair in nestedDimensions)
            {
                Dimension dim = pair.Key;

                if (pair.Value == null)
                {
                    continue;
                }

                Block block;

                if(blocks.TryGetValue(pair.Value, out block))
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
                doc.Blocks.Add(block, false);
            }
        }

        private void ReadEntities()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.EntitiesSection);

            chunk.Next();

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                ReadEntity(false);
            }
        }

        private void ReadObjects()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.ObjectsSection);

            chunk.Next();

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                switch (chunk.ReadString())
                {
                    case DxfObjectCode.Dictionary:
                        DictionaryObject dictionary = ReadDictionary();
                        dictionaries.Add(dictionary.Handle, dictionary);
                        // the named dictionary is always the first in the objects section
                        if (namedDictionary == null)
                        {
                            CreateObjectCollection(dictionary);
                            namedDictionary = dictionary;
                        }

                        break;
                    case DxfObjectCode.RasterVariables:
                        doc.RasterVariables = ReadRasterVariables();
                        break;
                    case DxfObjectCode.ImageDef:
                        ImageDefinition imageDefinition = ReadImageDefinition();
                        doc.ImageDefinitions.Add(imageDefinition, false);
                        break;
                    case DxfObjectCode.ImageDefReactor:
                        ImageDefinitionReactor reactor = ReadImageDefReactor();
                        if (!imageDefReactors.ContainsKey(reactor.ImageHandle))
                            imageDefReactors.Add(reactor.ImageHandle, reactor);
                        break;
                    case DxfObjectCode.MLineStyle:
                        MLineStyle style = ReadMLineStyle();
                        doc.MlineStyles.Add(style, false);
                        break;
                    case DxfObjectCode.Group:
                        Group group = ReadGroup();
                        doc.Groups.Add(group, false);
                        break;
                    case DxfObjectCode.Layout:
                        Layout layout = ReadLayout();

                        if (layout.AssociatedBlock == null)
                        {
                            orphanLayouts.Add(layout);
                        }
                        else
                        {
                            doc.Layouts.Add(layout, false);
                        }

                        break;
                    case DxfObjectCode.UnderlayDgnDefinition:
                        UnderlayDgnDefinition underlayDgnDef = (UnderlayDgnDefinition) ReadUnderlayDefinition(UnderlayType.DGN);
                        doc.UnderlayDgnDefinitions.Add(underlayDgnDef, false);
                        break;
                    case DxfObjectCode.UnderlayDwfDefinition:
                        UnderlayDwfDefinition underlayDwfDef = (UnderlayDwfDefinition) ReadUnderlayDefinition(UnderlayType.DWF);
                        doc.UnderlayDwfDefinitions.Add(underlayDwfDef, false);
                        break;
                    case DxfObjectCode.UnderlayPdfDefinition:
                        UnderlayPdfDefinition underlayPdfDef = (UnderlayPdfDefinition) ReadUnderlayDefinition(UnderlayType.PDF);
                        doc.UnderlayPdfDefinitions.Add(underlayPdfDef, false);
                        break;
                    default:
                        do
                            chunk.Next(); while (chunk.Code != 0);
                        break;
                }
            }

            // this will try to fix problems with layouts and model/paper space blocks
            // nothing of this is be necessary in a well formed DXF
            RelinkOrphanLayouts();

            // raster variables
            if (doc.RasterVariables == null)
            {
                doc.RasterVariables = new RasterVariables();
            }
        }

        private void RelinkOrphanLayouts()
        {
            // add any additional layouts corresponding to the PaperSpace block records not referenced by any layout
            // these are fixes for possible errors with layouts and their associated blocks. This should never happen.
            foreach (BlockRecord r in blockRecordPointerToLayout.Values)
            {
                Layout layout = null;

                // the *ModelSpace block must be linked to a layout called "Model"
                if (string.Equals(r.Name, Block.DefaultModelSpaceName, StringComparison.OrdinalIgnoreCase))
                {
                    foreach (Layout l in orphanLayouts)
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
                        doc.Layouts.Add(layout);
                        continue;
                    }

                    // we have a suitable layout
                    layout.AssociatedBlock = doc.Blocks[r.Name];
                    orphanLayouts.Remove(layout);
                    doc.Layouts.Add(layout);
                    continue;
                }

                // the *PaperSpace block cannot be linked with a layout called "Model"
                // check if we have any orphan layouts
                if (orphanLayouts.Count > 0)
                {
                    foreach (Layout l in orphanLayouts)
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
                        layout = orphanLayouts[0];
                        layout.AssociatedBlock = doc.Blocks[r.Name];
                        doc.Layouts.Add(layout);
                        orphanLayouts.Remove(layout);
                        continue;
                    }
                }

                // create a new Layout in case there are now more suitable orphans
                short counter = 1;
                string layoutName = "Layout" + 1;

                while (doc.Layouts.Contains(layoutName))
                {
                    counter += 1;
                    layoutName = "Layout" + counter;
                }

                layout = new Layout(layoutName)
                {
                    TabOrder = (short) (doc.Layouts.Count + 1),
                    AssociatedBlock = doc.Blocks[r.Name]
                };

                doc.Layouts.Add(layout);
            }

            // if there are still orphan layouts add them to the list, it will create an associate block for them
            foreach (Layout orphan in orphanLayouts)
            {
                doc.Layouts.Add(orphan, false);
            }

            // add ModelSpace layout if it does not exist
            doc.Layouts.Add(Layout.ModelSpace);
        }

        private void ReadThumbnailImage()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.ThumbnailImageSection);

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                //read the thumbnail image
                do
                    chunk.Next(); while (chunk.Code != 0);
            }
        }

        private void ReadAcdsData()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.AcdsDataSection);

            while (chunk.ReadString() != DxfObjectCode.EndSection)
            {
                //read the ACDSSCHEMA and ACDSRECORD, multiple entries
                do
                    chunk.Next(); while (chunk.Code != 0);
            }
        }

        #endregion

        #region table methods

        private void ReadTable()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.Table);

            string handle = null;
            chunk.Next();
            string tableName = chunk.ReadString();
            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 330:
                        string owner = chunk.ReadHex();
                        // owner should be always, 0 handle of the document.
                        Debug.Assert(owner == "0");
                        chunk.Next();
                        break;
                    case 102:
                        ReadExtensionDictionaryGroup();
                        chunk.Next();
                        break;
                    case 100:
                        Debug.Assert(chunk.ReadString() == SubclassMarker.Table || chunk.ReadString() == SubclassMarker.DimensionStyleTable);
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            // create collection
            switch (tableName)
            {
                case DxfObjectCode.ApplicationIdTable:
                    doc.ApplicationRegistries = new ApplicationRegistries(doc, handle);
                    break;
                case DxfObjectCode.BlockRecordTable:
                    doc.Blocks = new BlockRecords(doc, handle);
                    break;
                case DxfObjectCode.DimensionStyleTable:
                    doc.DimensionStyles = new DimensionStyles(doc, handle);
                    break;
                case DxfObjectCode.LayerTable:
                    doc.Layers = new Layers(doc, handle);
                    break;
                case DxfObjectCode.LinetypeTable:
                    doc.Linetypes = new Linetypes(doc, handle);
                    break;
                case DxfObjectCode.TextStyleTable:
                    doc.TextStyles = new TextStyles(doc, handle);
                    doc.ShapeStyles = new ShapeStyles(doc);
                    break;
                case DxfObjectCode.UcsTable:
                    doc.UCSs = new UCSs(doc, handle);
                    break;
                case DxfObjectCode.ViewTable:
                    doc.Views = new Views(doc, handle);
                    break;
                case DxfObjectCode.VportTable:
                    doc.VPorts = new VPorts(doc, handle);
                    break;
                default:
                    throw new Exception(string.Format("Unknown Table name {0} at position {1}", tableName, chunk.CurrentPosition));
            }

            // read table entries
            while (chunk.ReadString() != DxfObjectCode.EndTable)
            {
                ReadTableEntry();
            }

            chunk.Next();
        }

        private void ReadTableEntry()
        {
            string dxfCode = chunk.ReadString();
            string handle = null;

            // only the first *Active VPort is supported, this one describes the current document view.
            VPort active = null;

            while (chunk.ReadString() != DxfObjectCode.EndTable)
            {
                // table entry common codes
                while (chunk.Code != 100)
                {
                    switch (chunk.Code)
                    {
                        case 5:
                            handle = chunk.ReadHex();
                            chunk.Next();
                            break;
                        case 105:
                            // this handle code is specific of dimension styles
                            handle = chunk.ReadHex();
                            chunk.Next();
                            break;
                        case 330:
                            //string owner = chunk.ReadHandle(); // owner should be always, the handle of the list to which the entry belongs.
                            chunk.Next();
                            break;
                        case 102:
                            ReadExtensionDictionaryGroup();
                            chunk.Next();
                            break;
                        default:
                            chunk.Next();
                            break;
                    }
                }

                chunk.Next();

                switch (dxfCode)
                {
                    case DxfObjectCode.ApplicationIdTable:
                        ApplicationRegistry appReg = ReadApplicationId();

                        if (appReg != null)
                        {
                            appReg.Handle = handle;
                            doc.ApplicationRegistries.Add(appReg, false);
                        }

                        break;
                    case DxfObjectCode.BlockRecordTable:
                        BlockRecord record = ReadBlockRecord();

                        if (record != null)
                        {
                            record.Handle = handle;
                            blockRecords.Add(record.Name, record);
                        }

                        break;
                    case DxfObjectCode.DimensionStyleTable:
                        DimensionStyle dimStyle = ReadDimensionStyle();

                        if (dimStyle != null)
                        {
                            dimStyle.Handle = handle;
                            doc.DimensionStyles.Add(dimStyle, false);
                        }

                        break;
                    case DxfObjectCode.LayerTable:
                        Layer layer = ReadLayer();

                        if (layer != null)
                        {
                            layer.Handle = handle;
                            doc.Layers.Add(layer, false);
                        }

                        break;
                    case DxfObjectCode.LinetypeTable:
                        bool isComplex;
                        Linetype linetype = ReadLinetype(out isComplex);

                        if (linetype != null)
                        {
                            linetype.Handle = handle;
                            // complex linetypes will be added after reading the style table
                            if(isComplex)
                                complexLinetypes.Add(linetype);
                            else
                                doc.Linetypes.Add(linetype, false);
                        }

                        break;
                    case DxfObjectCode.TextStyleTable:
                        // the DXF stores text and shape definitions in the same table
                        DxfObject style = ReadTextStyle();

                        if (style != null)
                        {
                            style.Handle = handle;
                            var textStyle = style as TextStyle;

                            if (textStyle != null)
                            {
                                doc.TextStyles.Add(textStyle, false);
                            }
                            else
                            {
                                doc.ShapeStyles.Add(style as ShapeStyle, false);
                            }
                        }

                        break;
                    case DxfObjectCode.UcsTable:
                        UCS ucs = ReadUCS();

                        if (ucs != null)
                        {
                            ucs.Handle = handle;
                            doc.UCSs.Add(ucs, false);
                        }

                        break;
                    case DxfObjectCode.ViewTable:
                        ReadView();
                        //doc.Views.Add((View) entry);
                        break;
                    case DxfObjectCode.VportTable:
                        VPort vport = ReadVPort();

                        if (vport != null && active == null)
                        {
                            // only the first *Active VPort is supported, this one describes the current document view.
                            if (vport.Name.Equals(VPort.DefaultName, StringComparison.OrdinalIgnoreCase))
                            {
                                active = doc.Viewport;
                                active.Handle = handle;
                                active.ViewCenter = vport.ViewCenter;
                                active.SnapBasePoint = vport.SnapBasePoint;
                                active.SnapSpacing = vport.SnapSpacing;
                                active.GridSpacing = vport.GridSpacing;
                                active.ViewDirection = vport.ViewDirection;
                                active.ViewTarget = vport.ViewTarget;
                                active.ViewHeight = vport.ViewHeight;
                                active.ViewAspectRatio = vport.ViewAspectRatio;
                                active.ShowGrid = vport.ShowGrid;
                                active.SnapMode = vport.SnapMode;
                            }
                        }                       
                        break;
                    default:
                        ReadUnkownTableEntry();
                        return;
                }
            }
        }

        private ApplicationRegistry ReadApplicationId()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.ApplicationId);

            string appId = string.Empty;
            List<XData> xData = new List<XData>();
            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 1001:
                        string id = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(id));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        chunk.Next();
                        break;
                }
            }

            if (!TableObject.IsValidName(appId))
            {
                return null;
            }

            ApplicationRegistry applicationRegistry = new ApplicationRegistry(appId, false);

            if (xData.Count > 0)
            {
                hasXData.Add(applicationRegistry, xData);
            }

            return applicationRegistry;
        }

        private BlockRecord ReadBlockRecord()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.BlockRecord);

            string name = string.Empty;
            DrawingUnits units = DrawingUnits.Unitless;
            bool allowExploding = true;
            bool scaleUniformly = false;
            List<XData> xData = new List<XData>();
            string pointerToLayout = null;

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        units = (DrawingUnits) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 280:
                        allowExploding = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 281:
                        scaleUniformly = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 340:
                        pointerToLayout = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            // we need to check for generated blocks by dimensions, even if the dimension was deleted the block might persist in the drawing.
            CheckDimBlockName(name);

            BlockRecord record = new BlockRecord(name)
            {
                Units = units,
                AllowExploding = allowExploding,
                ScaleUniformly = scaleUniformly
            };

            if (xData.Count > 0)
            {
                hasXData.Add(record, xData);
            }

            // here is where DXF versions prior to AutoCad2007 stores the block units
            // read the layer transparency from the extended data
            XData designCenterData;

            if (record.XData.TryGetValue(ApplicationRegistry.DefaultName, out designCenterData))
            {
                using (IEnumerator<XDataRecord> records = designCenterData.XDataRecord.GetEnumerator())
                {
                    while (records.MoveNext())
                    {
                        XDataRecord data = records.Current;

                        if (data == null)
                        {
                            // premature end
                            break; 
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
                                // premature end
                                break;
                            }

                            // all style overrides are enclosed between XDataCode.ControlString "{" and "}"
                            if (data == null)
                            {
                                // premature end
                                break;
                            }

                            if (data.Code != XDataCode.ControlString)
                            {
                                // premature end
                                break;
                            }

                            if (records.MoveNext())
                            {
                                data = records.Current;
                            }
                            else
                            {
                                // premature end
                                break;
                            }

                            if (data == null)
                            {
                                continue;
                            }

                            while (data.Code != XDataCode.ControlString)
                            {
                                if (records.MoveNext())
                                {
                                    data = records.Current;
                                }
                                else
                                {
                                    // premature end
                                    break;
                                }

                                // the second 1070 code is the one that stores the block units,
                                // it will override the first 1070 that stores the Autodesk Design Center version number
                                if (data == null)
                                {
                                    // premature end
                                    break;
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
                blockRecordPointerToLayout.Add(pointerToLayout, record);
            }

            return record;
        }

        private DimensionStyle ReadDimensionStyle()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.DimensionStyle);

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

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 3:
                        dimpost = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 4:
                        dimapost = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 40:
                        dimscale = chunk.ReadDouble();
                        
                        if (dimscale <= 0.0)
                        {
                            dimscale = defaultDim.DimScaleOverall;
                        }

                        chunk.Next();
                        break;
                    case 41:
                        dimasz = chunk.ReadDouble();
                        
                        if (dimasz < 0.0)
                        {
                            dimasz = defaultDim.ArrowSize;
                        }

                        chunk.Next();
                        break;
                    case 42:
                        dimexo = chunk.ReadDouble();
                        
                        if (dimexo < 0.0)
                        {
                            dimexo = defaultDim.ExtLineOffset;
                        }

                        chunk.Next();
                        break;
                    case 43:
                        dimdli = chunk.ReadDouble();

                        if (dimdli < 0.0)
                        {
                            dimdli = defaultDim.DimBaselineSpacing;
                        }

                        chunk.Next();
                        break;
                    case 44:
                        dimexe = chunk.ReadDouble();

                        if (dimexe < 0.0)
                        {
                            dimexe = defaultDim.ExtLineExtend;
                        }

                        chunk.Next();
                        break;
                    case 45:
                        dimrnd = chunk.ReadDouble();

                        if (dimrnd < 0.000001 && !MathHelper.IsZero(dimrnd, double.Epsilon))
                        {
                            dimrnd = defaultDim.DimRoundoff;
                        }

                        chunk.Next();
                        break;
                    case 46:
                        dimdle = chunk.ReadDouble();

                        if (dimdle < 0.0)
                        {
                            dimdle = defaultDim.DimLineExtend;
                        }

                        chunk.Next();
                        break;
                    case 47:
                        tolerances.UpperLimit = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 48:
                        tolerances.LowerLimit = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 49:
                        dimfxl = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 69:
                        dimtfill = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 70:
                        dimtfillclrt = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 71:
                        dimtol = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        dimlim = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        dimtih = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 74:
                        dimtoh = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 75:
                        dimse1 = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 76:
                        dimse2 = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 77:
                        dimtad = (DimensionStyleTextVerticalPlacement) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 78:
                        dimzin = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 79:
                        dimazin = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 140:
                        dimtxt = chunk.ReadDouble();

                        if (dimtxt <= 0.0)
                        {
                            dimtxt = defaultDim.TextHeight;
                        }

                        chunk.Next();
                        break;
                    case 141:
                        dimcen = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 143:
                        double dimaltf = chunk.ReadDouble();
                        
                        if (dimaltf <= 0.0)
                        {
                            dimaltf = defaultDim.AlternateUnits.Multiplier;
                        }

                        dimensionStyleAlternateUnits.Multiplier = dimaltf;
                        chunk.Next();
                        break;
                    case 144:
                        dimlfac = chunk.ReadDouble();
                        
                        if (MathHelper.IsZero(dimlfac))
                        {
                            dimlfac = defaultDim.DimScaleLinear;
                        }

                        chunk.Next();
                        break;
                    case 146:
                        dimtfac = chunk.ReadDouble();
                        
                        if (dimtfac <= 0)
                        {
                            dimtfac = defaultDim.TextFractionHeightScale;
                        }

                        chunk.Next();
                        break;
                    case 147:
                        dimgap = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 148:
                        double dimaltrnd = chunk.ReadDouble();
                        
                        if (dimaltrnd < 0.000001 && !MathHelper.IsZero(dimaltrnd, double.Epsilon))
                        {
                            dimaltrnd = defaultDim.AlternateUnits.Roundoff;
                        }

                        dimensionStyleAlternateUnits.Roundoff = dimaltrnd;
                        chunk.Next();
                        break;
                    case 170:
                        dimensionStyleAlternateUnits.Enabled = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 171:
                        short dimaltd = chunk.ReadShort();
                        
                        if (dimaltd < 0)
                        {
                            dimaltd = defaultDim.AlternateUnits.LengthPrecision;
                        }

                        dimensionStyleAlternateUnits.LengthPrecision = dimaltd;
                        chunk.Next();
                        break;
                    case 172:
                        dimtofl = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 173:
                        dimsah = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 174:
                        dimtix = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 175:
                        dimsoxd = chunk.ReadShort() == 0;
                        chunk.Next();
                        break;
                    case 176:
                        dimclrd = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 177:
                        dimclre = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 178:
                        dimclrt = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 179:
                        dimadec = chunk.ReadShort();
                        
                        if (dimadec < 0)
                        {
                            dimadec = defaultDim.AngularPrecision;
                        }

                        chunk.Next();
                        break;
                    case 271:
                        dimdec = chunk.ReadShort();
                        
                        if (dimdec < 0)
                        {
                            dimdec = defaultDim.LengthPrecision;
                        }

                        chunk.Next();
                        break;
                    case 272:
                        short dimtdec = chunk.ReadShort();
                        
                        if (dimtdec < 0)
                        {
                            dimtdec = defaultDim.Tolerances.Precision;
                        }

                        tolerances.Precision = dimtdec;
                        chunk.Next();
                        break;
                    case 273:
                        short dimaltu = chunk.ReadShort();

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

                        chunk.Next();
                        break;
                    case 274:
                        short dimalttd = chunk.ReadShort();
                        
                        if (dimalttd < 0)
                        {
                            dimalttd = defaultDim.Tolerances.AlternatePrecision;
                        }

                        tolerances.AlternatePrecision = dimalttd;
                        chunk.Next();
                        break;
                    case 275:
                        dimaunit = (AngleUnitType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 276:
                        dimfrac = (FractionFormatType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 277:
                        dimlunit = (LinearUnitType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 278:
                        dimdsep = (char) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 279:
                        dimtmove = (DimensionStyleFitTextMove) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 280:
                        dimjust = (DimensionStyleTextHorizontalPlacement) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 281:
                        dimsd1 = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 282:
                        dimsd2 = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 283:
                        tolerances.VerticalPlacement = (DimensionStyleTolerancesVerticalPlacement) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 284:
                        dimtzin = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 285:
                        dimaltz = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 286:
                        dimalttz = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 289:
                        dimatfit = (DimensionStyleFitOptions) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 290:
                        dimfxlon = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case 294:
                        dimtxtdirection = chunk.ReadBool() ? DimensionStyleTextDirection.RightToLeft : DimensionStyleTextDirection.LeftToRight;
                        chunk.Next();
                        break;
                    case 340:
                        dimtxsty = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 341:
                        dimldrblk = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 342:
                        dimblk = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 343:
                        dimblk1 = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 344:
                        dimblk2 = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 345:
                        dimltype = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 346:
                        dimltex1 = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 347:
                        dimltex2 = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 371:
                        dimlwd = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 372:
                        dimlwe = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 1001:
                        string id = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(id));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        chunk.Next();
                        break;
                }
            }

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

            bool[] supress = GetLinearZeroesSuppression(dimzin);
            style.SuppressLinearLeadingZeros = supress[0];
            style.SuppressLinearTrailingZeros = supress[1];
            style.SuppressZeroFeet = supress[2];
            style.SuppressZeroInches = supress[3];

            supress = GetLinearZeroesSuppression(dimaltz);
            style.AlternateUnits.SuppressLinearLeadingZeros = supress[0];
            style.AlternateUnits.SuppressLinearTrailingZeros = supress[1];
            style.AlternateUnits.SuppressZeroFeet = supress[2];
            style.AlternateUnits.SuppressZeroInches = supress[3];

            supress = GetLinearZeroesSuppression(dimtzin);
            style.Tolerances.SuppressLinearLeadingZeros = supress[0];
            style.Tolerances.SuppressLinearTrailingZeros = supress[1];
            style.Tolerances.SuppressZeroFeet = supress[2];
            style.Tolerances.SuppressZeroInches = supress[3];

            supress = GetLinearZeroesSuppression(dimalttz);
            style.Tolerances.AlternateSuppressLinearLeadingZeros = supress[0];
            style.Tolerances.AlternateSuppressLinearTrailingZeros = supress[1];
            style.Tolerances.AlternateSuppressZeroFeet = supress[2];
            style.Tolerances.AlternateSuppressZeroInches = supress[3];

            if (dimtol == 0 && dimlim == 0)
            {
                style.Tolerances.DisplayMethod =  DimensionStyleTolerancesDisplayMethod.None;
            }

            if (dimtol == 1 && dimlim == 0)
            {
                style.Tolerances.DisplayMethod =
                    MathHelper.IsEqual(style.Tolerances.UpperLimit, style.Tolerances.LowerLimit) ?
                        DimensionStyleTolerancesDisplayMethod.Symmetrical : DimensionStyleTolerancesDisplayMethod.Deviation;}

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

            if (xData.Count > 0)
            {
                hasXData.Add(style, xData);
            }

            // store information for post processing. The blocks, text styles, and line types definitions might appear after the dimension style
            if (!dimsah)
            {
                dimblk1 = dimblk;
                dimblk2 = dimblk;
            }

            string[] handles = {dimblk1, dimblk2, dimldrblk, dimtxsty, dimltype, dimltex1, dimltex2};
            dimStyleToHandles.Add(style, handles);

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
            Debug.Assert(chunk.ReadString() == SubclassMarker.Layer);

            string name = string.Empty;
            bool isVisible = true;
            bool plot = true;
            AciColor color = AciColor.Default;
            Linetype linetype = Linetype.ByLayer;
            Lineweight lineweight = Lineweight.Default;
            LayerFlags flags = LayerFlags.None;
            List<XData> xData = new List<XData>();

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        flags = (LayerFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 62:
                        short index = chunk.ReadShort();

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

                        chunk.Next();
                        break;
                    case 420: // the layer uses true color
                        color = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 6:
                        string linetypeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        linetype = GetLinetype(linetypeName);

                        // layer linetype cannot be ByLayer or ByBlock
                        if (linetype.IsByLayer || linetype.IsByBlock)
                        {
                            linetype = Linetype.Continuous;
                        }

                        chunk.Next();
                        break;
                    case 290:
                        plot = chunk.ReadBool();
                        chunk.Next();
                        break;
                    case 370:
                        lineweight = (Lineweight) chunk.ReadShort();

                        // layer lineweight cannot be ByLayer or ByBlock
                        if (lineweight == Lineweight.ByLayer || lineweight == Lineweight.ByBlock)
                        {
                            lineweight = Lineweight.Default;
                        }

                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }  

                        chunk.Next();
                        break;
                }
            }

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
                hasXData.Add(layer, xData);
            }

            // read the layer transparency from the extended data
            XData xDataTransparency;

            if (layer.XData.TryGetValue("AcCmTransparency", out xDataTransparency))
            {
                // there should be only one entry with the transparency value, the first 1071 code will be used
                foreach (XDataRecord record in xDataTransparency.XDataRecord)
                {
                    if (record.Code == XDataCode.Int32)
                    {
                        layer.Transparency = Transparency.FromAlphaValue((int) record.Value);
                    }
                }
            }
            
            return layer;
        }

        private Linetype ReadLinetype(out bool isComplex)
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.Linetype);

            isComplex = false;
            string name = null;
            string description = null;
            List<LinetypeSegment> segments = new List<LinetypeSegment>();
            List<XData> xData = new List<XData>();

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2: // line type name is case insensitive
                        name = chunk.ReadString();

                        if (string.Equals(name, Linetype.ByLayerName, StringComparison.OrdinalIgnoreCase))
                        {
                            name = Linetype.ByLayerName;
                        }
                        else if (string.Equals(name, Linetype.ByBlockName, StringComparison.OrdinalIgnoreCase))
                        {
                            name = Linetype.ByBlockName;
                        }

                        name = DecodeEncodedNonAsciiCharacters(name);
                        chunk.Next();
                        break;
                    case 3: // line type description
                        description = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 73:
                        //number of segments (not needed)
                        chunk.Next();
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        chunk.Next();
                        break;
                    case 49:
                        // read linetype segments multiple entries
                        double length = chunk.ReadDouble();
                        // code 49 should be followed by code 74 that defines if the linetype segment is simple, text or shape
                        chunk.Next();
                        Debug.Assert(chunk.Code == 74, "Bad formatted linetype data.");
                        short type = chunk.ReadShort();
                        chunk.Next();

                        LinetypeSegment segment;

                        if (type == 0)
                        {
                            segment = new LinetypeSimpleSegment(length);
                        }
                        else
                        {
                            isComplex = true;
                            segment = ReadLinetypeComplexSegment(type, length);
                        }

                        if (segment != null)
                        {
                            segments.Add(segment);
                        }

                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        chunk.Next();
                        break;
                }
            }

            if (!TableObject.IsValidName(name))
            {
                return null;
            }

            Linetype linetype = new Linetype(name, segments, description, false);

            if (xData.Count > 0)
            {
                hasXData.Add(linetype, xData);
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
            while (chunk.Code != 49 && chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 75:
                        shapeNumber = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 340:
                        handleToStyle = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 46:
                        scale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 44:
                        offset.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 45:
                        offset.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 9:
                        text = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
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
                linetypeShapeSegmentToNumber.Add((LinetypeShapeSegment) segment, shapeNumber);
            }
            else
            {
                return null;
            }

            linetypeSegmentStyleHandles.Add(segment, handleToStyle);
            
            return segment;
        }

        private DxfObject ReadTextStyle()
        {
            // this method will read both text and shape styles their definitions appear in the same table list
            Debug.Assert(chunk.ReadString() == SubclassMarker.TextStyle);

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

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 3:
                        file = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 4:
                        bigFont = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        int flag = chunk.ReadShort();

                        if ((flag & 1) == 1)
                        {
                            isShapeStyle = true;
                        }

                        if ((flag & 4) == 4)
                        {
                            isVertical = true;
                        }

                        chunk.Next();
                        break;
                    case 71:
                        int upDownBack = chunk.ReadShort();
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
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();

                        if (height < 0.0)
                        {
                            height = 0.0;
                        }

                        chunk.Next();
                        break;
                    case 41:
                        widthFactor = chunk.ReadDouble();

                        if (widthFactor < 0.01 || widthFactor > 100.0)
                        {
                            widthFactor = 1.0;
                        }

                        chunk.Next();
                        break;
                    case 42:
                        //last text height used (not applicable)
                        chunk.Next();
                        break;
                    case 50:
                        obliqueAngle = chunk.ReadDouble();

                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                        {
                            obliqueAngle = 0.0;
                        }

                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));

                        if (string.Equals(appId, ApplicationRegistry.DefaultName))
                        {
                            xDataFont = data;
                        }

                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }
                        chunk.Next();
                        break;
                }
            }

            // shape styles are handle in a separate list
            if (isShapeStyle)
            {
                ShapeStyle shapeStyle = new ShapeStyle(Path.GetFileNameWithoutExtension(file), file, height, widthFactor, obliqueAngle);
               
                if (xData.Count > 0)
                {
                    hasXData.Add(shapeStyle, xData);
                }

                return shapeStyle;
            }

            // text styles
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

            // the information stored in the extended data takes precedence before the font file (this is only applicable for true type fonts)
            if (string.IsNullOrEmpty(fontFamily))
            {
                if (string.IsNullOrEmpty(file))
                {
                    file = "simplex.shx";
                }
                else
                {
                    // only true type TTF fonts or compiled shape SHX fonts are allowed, the default "simplex.shx" font will be used in this case
                    if (!Path.GetExtension(file).Equals(".TTF", StringComparison.InvariantCultureIgnoreCase) &&
                        !Path.GetExtension(file).Equals(".SHX", StringComparison.InvariantCultureIgnoreCase))
                    {
                        file = "simplex.shx";
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
                hasXData.Add(style, xData);
            }

            return style;
        }

        private UCS ReadUCS()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.Ucs);

            string name = string.Empty;
            Vector3 origin = Vector3.Zero;
            Vector3 xDir = Vector3.UnitX;
            Vector3 yDir = Vector3.UnitY;
            double elevation = 0.0;
            List<XData> xData = new List<XData>();

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 10:
                        origin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        origin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        origin.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        xDir.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        xDir.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        xDir.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        yDir.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        yDir.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        yDir.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 146:
                        elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        chunk.Next();
                        break;
                }
            }

            if (!TableObject.IsValidName(name)) return null;

            UCS ucs = new UCS(name, origin, xDir, yDir, false) {Elevation = elevation};
            if (xData.Count > 0) hasXData.Add(ucs, xData);
            return ucs;
        }

        private View ReadView()
        {
            // placeholder method for view table objects
            Debug.Assert(chunk.ReadString() == SubclassMarker.View);

            chunk.Next();

            while (chunk.Code != 0)
            {
                chunk.Next();
            }

            return null;
        }

        private VPort ReadVPort()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.VPort);

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

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 12:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        snapBasePoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        snapBasePoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        snapSpacing.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        snapSpacing.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 15:
                        gridSpacing.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        gridSpacing.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 16:
                        direction.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 26:
                        direction.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 36:
                        direction.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 17:
                        target.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 27:
                        target.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 37:
                        target.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        ratio = chunk.ReadDouble();
                        if (ratio <= 0)
                            ratio = 1.0;
                        chunk.Next();
                        break;
                    case 75:
                        snapMode = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 76:
                        showGrid = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                        {
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        }

                        chunk.Next();
                        break;
                }
            }

            if (!(TableObject.IsValidName(name) || name.Equals(VPort.DefaultName, StringComparison.OrdinalIgnoreCase)))
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

            if (xData.Count > 0) hasXData.Add(vport, xData);
            return vport;
        }

        private void ReadUnkownTableEntry()
        {
            do
                chunk.Next();
            while (chunk.Code != 0);
        }

        #endregion

        #region block methods

        private Block ReadBlock()
        {
            Debug.Assert(chunk.ReadString() == DxfObjectCode.BeginBlock);

            BlockRecord blockRecord;
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 1:
                        xrefFile = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 4:
                        description = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 8:
                        string layerName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        layer = GetLayer(layerName);
                        chunk.Next();
                        break;
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        type = (BlockTypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 10:
                        basePoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 3:
                        //I don't know the reason of these duplicity since code 2 also contains the block name
                        //The program EASE exports code 3 with an empty string (use it or don't use it but do NOT mix information)
                        //name = dxfPairInfo.Value;
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            // read block entities
            while (chunk.ReadString() != DxfObjectCode.EndBlock)
            {
                DxfObject dxfObject = ReadEntity(true);
                if (dxfObject != null)
                {
                    AttributeDefinition attDef = dxfObject as AttributeDefinition;
                    if (attDef != null)
                        attDefs.Add(attDef);
                    EntityObject entity = dxfObject as EntityObject;
                    if(entity != null)
                        entities.Add(entity);
                }
            }

            // read the end block object until a new element is found
            chunk.Next();
            string endBlockHandle = string.Empty;
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        endBlockHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 8:
                        // the EndBlock layer and the Block layer are the same
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            if (!blockRecords.TryGetValue(name, out blockRecord))
                throw new Exception(string.Format("The block record {0} is not defined.", name));

            Block block;

            if (type.HasFlag(BlockTypeFlags.XRef))
            {
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
                    entityList.Add(entity, blockRecord.Handle);

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
                        block.AttributeDefinitions.Add(attDef);
                }
                // block entities for post processing (MLines and Images references other objects (MLineStyle and ImageDefinition) that will be defined later
                blockEntities.Add(block, entities);
            }

            return block;
        }

        private AttributeDefinition ReadAttributeDefinition()
        {
            string tag = string.Empty;
            string text = string.Empty;
            object value = null;
            AttributeFlags flags = AttributeFlags.Visible;
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
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        tag = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 3:
                        text = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 1:
                        value = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        flags = (AttributeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 7:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        style = GetTextStyle(styleName);
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        widthFactor = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = MathHelper.NormalizeAngle(chunk.ReadDouble());
                        if (obliqueAngle > 180)
                            obliqueAngle -= 360;
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                            obliqueAngle = 0.0;
                        chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 74:
                        verticalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            if (alignment == TextAlignment.BaselineLeft)
            {
                ocsBasePoint = firstAlignmentPoint;
            }
            else if (alignment == TextAlignment.Fit || alignment == TextAlignment.Aligned)
            {
                width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                ocsBasePoint = firstAlignmentPoint;
            }
            else
            {
                ocsBasePoint = secondAlignmentPoint;
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
                Rotation = rotation
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

            AttributeFlags flags = AttributeFlags.Visible;
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
            Vector3 normal = Vector3.UnitZ;

            // DxfObject codes
            chunk.Next();
            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", DxfObjectCode.Attribute));
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            // AcDbEntity common codes
            chunk.Next();
            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", DxfObjectCode.Attribute));
                    case 8: // layer code
                        string layerName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        layer = GetLayer(layerName);
                        chunk.Next();
                        break;
                    case 62: // ACI color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 440: //transparency
                        transparency = Transparency.FromAlphaValue(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 420: // the entity uses true color
                        color = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 6: // type line code
                        string linetypeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        linetype = GetLinetype(linetypeName);
                        chunk.Next();
                        break;
                    case 370: // line weight code
                        lineweight = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 48: // line type scale
                        linetypeScale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 60: //object visibility
                        isVisible = chunk.ReadShort() == 0;
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            string atttag = null;
            AttributeDefinition attdef = null;
            object value = null;

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        atttag = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        // seems that some programs might export insert entities with attributes which definitions are not defined in the block
                        // if it is not present the insert attribute will have a null definition
                        if (!isBlockEntity)
                            block.AttributeDefinitions.TryGetValue(atttag, out attdef);
                        chunk.Next();
                        break;
                    case 1:
                        value = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        flags = (AttributeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 7:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        style = GetTextStyle(styleName);
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        widthFactor = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = MathHelper.NormalizeAngle(chunk.ReadDouble());
                        if (obliqueAngle > 180)
                            obliqueAngle -= 360;
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                            obliqueAngle = 0.0;
                        chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 74:
                        verticalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            if (alignment == TextAlignment.BaselineLeft)
            {
                ocsBasePoint = firstAlignmentPoint;
            }
            else if (alignment == TextAlignment.Fit || alignment == TextAlignment.Aligned)
            {
                width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                ocsBasePoint = firstAlignmentPoint;
            }
            else
            {
                ocsBasePoint = secondAlignmentPoint;
            }

            Attribute attribute = new Attribute
            {
                Handle = handle,
                Color = color,
                Layer = layer,
                Linetype = linetype,
                Lineweight = lineweight,
                LinetypeScale = linetypeScale,
                Transparency = transparency,
                IsVisible = isVisible,
                Definition = attdef,
                Tag = atttag,
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
                Rotation = rotation
            };

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

            string dxfCode = chunk.ReadString();
            chunk.Next();

            // DxfObject common codes
            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", dxfCode));
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 102:
                        ReadExtensionDictionaryGroup();
                        chunk.Next();
                        break;
                    case 330:
                        owner = chunk.ReadHex();
                        if (owner == "0") owner = null;
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            // AcDbEntity common codes
            Debug.Assert(chunk.ReadString() == SubclassMarker.Entity);
            chunk.Next();
            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of entity {0} definition.", dxfCode));
                    case 8: // layer code
                        string layerName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        layer = GetLayer(layerName);
                        chunk.Next();
                        break;
                    case 62: // ACI color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 420: // the entity uses true color
                        color = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 440: // transparency
                        transparency = Transparency.FromAlphaValue(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 6: // type line code
                        string linetypeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        linetype = GetLinetype(linetypeName);
                        chunk.Next();
                        break;
                    case 370: // line weight code
                        lineweight = (Lineweight) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 48: // line type scale
                        linetypeScale = chunk.ReadDouble();
                        if (linetypeScale <= 0.0)
                            linetypeScale = 1.0;
                        chunk.Next();
                        break;
                    case 60: // object visibility
                        isVisible = chunk.ReadShort() == 0;
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            switch (dxfCode)
            {
                case DxfObjectCode.Arc:
                    dxfObject = ReadArc();
                    break;
                case DxfObjectCode.AttributeDefinition:
                    dxfObject = ReadAttributeDefinition();
                    break;
                case DxfObjectCode.Circle:
                    dxfObject = ReadCircle();
                    break;
                case DxfObjectCode.Dimension:
                    dxfObject = ReadDimension(isBlockEntity);
                    break;
                case DxfObjectCode.Ellipse:
                    dxfObject = ReadEllipse();
                    break;
                case DxfObjectCode.Face3d:
                    dxfObject = ReadFace3d();
                    break;
                case DxfObjectCode.Hatch:
                    dxfObject = ReadHatch();
                    break;
                case DxfObjectCode.Image:
                    dxfObject = ReadImage();
                    break;
                case DxfObjectCode.Insert:
                    dxfObject = ReadInsert(isBlockEntity);
                    break;
                case DxfObjectCode.Leader:
                    dxfObject = ReadLeader();
                    break;
                case DxfObjectCode.Line:
                    dxfObject = ReadLine();
                    break;
                case DxfObjectCode.LightWeightPolyline:
                    dxfObject = ReadLwPolyline();
                    break;
                case DxfObjectCode.Mesh:
                    dxfObject = ReadMesh();
                    break;
                case DxfObjectCode.MLine:
                    dxfObject = ReadMLine();
                    break;
                case DxfObjectCode.MText:
                    dxfObject = ReadMText();
                    break;
                case DxfObjectCode.Point:
                    dxfObject = ReadPoint();
                    break;
                case DxfObjectCode.Polyline:
                    dxfObject = ReadPolyline();
                    break;
                case DxfObjectCode.Ray:
                    dxfObject = ReadRay();
                    break;
                case DxfObjectCode.Text:
                    dxfObject = ReadText();
                    break;
                case DxfObjectCode.Tolerance:
                    dxfObject = ReadTolerance();
                    break;
                case DxfObjectCode.Trace:
                    dxfObject = ReadTrace();
                    break;
                case DxfObjectCode.Shape:
                    dxfObject = ReadShape();
                    break;
                case DxfObjectCode.Solid:
                    dxfObject = ReadSolid();
                    break;
                case DxfObjectCode.Spline:
                    dxfObject = ReadSpline();
                    break;
                case DxfObjectCode.UnderlayDgn:
                    dxfObject = ReadUnderlay();
                    break;
                case DxfObjectCode.UnderlayDwf:
                    dxfObject = ReadUnderlay();
                    break;
                case DxfObjectCode.UnderlayPdf:
                    dxfObject = ReadUnderlay();
                    break;
                case DxfObjectCode.Viewport:
                    dxfObject = ReadViewport();
                    break;
                case DxfObjectCode.XLine:
                    dxfObject = ReadXLine();
                    break;
                case DxfObjectCode.Wipeout:
                    dxfObject = ReadWipeout();
                    break;
                case DxfObjectCode.AcadTable:
                    dxfObject = ReadAcadTable(isBlockEntity);
                    break;
                default:
                    ReadUnknowEntity();
                    return null;
            }

            if (dxfObject == null || string.IsNullOrEmpty(handle))
                return null;

            dxfObject.Handle = handle;

            EntityObject entity = dxfObject as EntityObject;
            if (entity != null)
            {
                entity.Layer = layer;
                entity.Color = color;
                entity.Linetype = linetype;
                entity.Lineweight = lineweight;
                entity.LinetypeScale = linetypeScale;
                entity.IsVisible = isVisible;
                entity.Transparency = transparency;
            }

            AttributeDefinition attDef = dxfObject as AttributeDefinition;
            if (attDef != null)
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
                entityList.Add(dxfObject, owner);

            return dxfObject;
        }

        private Insert ReadAcadTable(bool isBlockEntity)
        {
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 direction = Vector3.UnitX;
            string blockName = null;
            Block block = null;
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        blockName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (!isBlockEntity)
                            block = GetBlock(blockName);
                        chunk.Next();
                        break;
                    case 10:
                        basePoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        direction.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        direction.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        direction.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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
            // since we are converting the table entity to an insert we also need to assign a new handle to the internal EndSequence
            doc.NumHandles = insert.EndSequence.AsignHandle(doc.NumHandles);
            insert.XData.AddRange(xData);

            //Vector3 ocsDirection = MathHelper.Transform(direction, normal, CoordinateSystem.World, CoordinateSystem.Object);
            //insert.Rotation = Vector2.Angle(new Vector2(ocsDirection.X, ocsDirection.Y))*MathHelper.RadToDeg;
            //insert.Scale = new Vector3(1.0);

            // post process nested inserts
            if (isBlockEntity)
                nestedInserts.Add(insert, blockName);

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        u.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        u.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        u.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        v.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        v.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        v.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 71:
                        boundaryType = (ClippingBoundaryType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 91:
                        // we cannot rely in this information it might or might not appear
                        chunk.Next();
                        break;
                    case 14:
                        x = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        vertexes.Add(new Vector2(x, chunk.ReadDouble()));
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            // for polygonal boundaries the last vertex is equal to the first, we will remove it
            if (boundaryType == ClippingBoundaryType.Polygonal)
                vertexes.RemoveAt(vertexes.Count - 1);

            Vector3 normal = Vector3.Normalize(Vector3.CrossProduct(u, v));
            List<Vector3> ocsPoints = MathHelper.Transform(new List<Vector3> {position, u, v}, normal, CoordinateSystem.World, CoordinateSystem.Object);
            double bx = ocsPoints[0].X;
            double by = ocsPoints[0].Y;
            double elevation = ocsPoints[0].Z;
            double max = ocsPoints[1].X;

            for (int i = 0; i < vertexes.Count; i++)
            {
                double vx = bx + max*(vertexes[i].X + 0.5);
                double vy = by + max*(0.5 - vertexes[i].Y);
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        scale.X = Math.Abs(chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        if (MathHelper.IsZero(scale.X)) scale.X = 1.0;
                        chunk.Next();
                        break;
                    case 42:
                        scale.Y = Math.Abs(chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        if (MathHelper.IsZero(scale.Y)) scale.Y = 1.0;
                        chunk.Next();
                        break;
                    case 43:
                        // the scale Z value has no use
                        //scale.Z = Math.Abs(chunk.ReadDouble()); // just in case, the underlay scale components must be positive
                        //if (MathHelper.IsZero(scale.Z)) scale.Z = 1.0;
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 340:
                        underlayDefHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 280:
                        displayOptions = (UnderlayDisplayFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 281:
                        contrast = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 282:
                        fade = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 11:
                        clippingVertex = new Vector2();
                        clippingVertex.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        clippingVertex.Y = chunk.ReadDouble();
                        clippingVertexes.Add(clippingVertex);
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            Vector3 wcsPosition = MathHelper.Transform(position, normal, CoordinateSystem.Object, CoordinateSystem.World);
            if (clippingVertexes.Count < 2)
                clippingBoundary = null;
            else if (clippingVertexes.Count == 2)
                clippingBoundary = new ClippingBoundary(clippingVertexes[0], clippingVertexes[1]);
            else
                clippingBoundary = new ClippingBoundary(clippingVertexes);

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
                return null;

            underlayToDefinitionHandles.Add(underlay, underlayDefHandle);

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 3:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.DimStyle;
                        style = GetDimensionStyle(styleName);
                        chunk.Next();
                        break;
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1:
                        value = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        xAxis.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        xAxis.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        xAxis.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            xAxis = MathHelper.Transform(xAxis, normal, CoordinateSystem.World, CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(xAxis.X, xAxis.Y));

            Tolerance entity = Tolerance.ParseStringRepresentation(value);
            entity.Style = style;
            entity.TextHeight = style.TextHeight;
            entity.Position = position;
            entity.Rotation = rotation*MathHelper.RadToDeg;
            entity.Normal = normal;
            entity.XData.AddRange(xData);

            // here is where DXF stores the tolerance text height
            XData heightXData;
            if (entity.XData.TryGetValue(ApplicationRegistry.DefaultName, out heightXData))
            {
                double textHeight = ReadToleranceTextHeightXData(heightXData);
                if(textHeight > 0) entity.TextHeight = textHeight;
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
                    if(data == null) return textHeight;

                    // the tolerance text height are stored under the string "DSTYLE"
                    if (data.Code == XDataCode.String && string.Equals((string) data.Value, "DSTYLE", StringComparison.OrdinalIgnoreCase))
                    {
                        if (records.MoveNext())
                        {
                            data = records.Current;
                            if(data == null) return textHeight;
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
                            if(data == null) return textHeight;
                        }
                        else
                        {
                            return textHeight; // premature end
                        }

                        while (!(data.Code == XDataCode.ControlString && (string) data.Value == "}"))
                        {
                            if (data.Code != XDataCode.Int16) return textHeight;

                            short textHeightCode = (short) data.Value;
                            if (records.MoveNext())
                            {
                                data = records.Current;
                                if(data == null) return textHeight;
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
                                    if (data.Code != XDataCode.Real) return textHeight; // premature end
                                    textHeight = (double) data.Value;
                                    break;
                            }

                            if (records.MoveNext())
                            {
                                data = records.Current;
                                if(data == null) return textHeight;
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
            LeaderPathType path = LeaderPathType.StraightLineSegements;
            bool hasHookline = false;
            List<Vector3> wcsVertexes = null;
            AciColor lineColor = AciColor.ByLayer;
            string annotation = string.Empty;
            Vector3 normal = Vector3.UnitZ;
            double elevation = 0.0;
            Vector3 offset = Vector3.Zero;

            List<XData> xData = new List<XData>();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 3:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.DimStyle;
                        style = GetDimensionStyle(styleName);
                        chunk.Next();
                        break;
                    case 71:
                        showArrowhead = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 72:
                        path = (LeaderPathType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        chunk.Next();
                        break;
                    case 74:
                        chunk.Next();
                        break;
                    case 75:
                        hasHookline = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 76:
                        chunk.Next();
                        break;
                    case 77:
                        lineColor = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 10:
                        wcsVertexes = ReadLeaderVertexes();
                        break;
                    case 340:
                        annotation = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 213:
                        offset.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 223:
                        offset.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 233:
                        offset.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            if (wcsVertexes == null)
                return null;

            if (hasHookline && wcsVertexes.Count >= 3)
            {
                wcsVertexes.RemoveAt(wcsVertexes.Count - 2);
            }
            List<Vector3> ocsVertexes = MathHelper.Transform(wcsVertexes, normal, CoordinateSystem.World, CoordinateSystem.Object);
            List<Vector2> vertexes = new List<Vector2>();
            foreach (Vector3 v in ocsVertexes)
            {
                vertexes.Add(new Vector2(v.X, v.Y));
                elevation = v.Z;
            }

            // The text vertical position is stored in the Leader extended data
            Vector3 ocsOffset = MathHelper.Transform(offset, normal, CoordinateSystem.World, CoordinateSystem.Object);
            Leader leader = new Leader(vertexes)
            {
                Style = style,
                ShowArrowhead = showArrowhead,
                PathType = path,
                LineColor = lineColor,
                Elevation = elevation,
                Normal = normal,
                Offset = new Vector2(ocsOffset.X, ocsOffset.Y),
                HasHookline = hasHookline
            };

            leader.XData.AddRange(xData);

            // this is for post-processing, the annotation entity might appear after the leader
            leaderAnnotation.Add(leader, annotation);

            return leader;
        }

        private List<Vector3> ReadLeaderVertexes()
        {
            List<Vector3> vertexes = new List<Vector3>();
            while (chunk.Code == 10)
            {
                Vector3 vertex = Vector3.Zero;
                vertex.X = chunk.ReadDouble();
                chunk.Next();
                vertex.Y = chunk.ReadDouble();
                chunk.Next();
                if (chunk.Code == 30)
                {
                    vertex.Z = chunk.ReadDouble();
                    chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 91:
                        subdivisionLevel = chunk.ReadInt();
                        if (subdivisionLevel < 0 || subdivisionLevel > 255)
                            subdivisionLevel = 0;
                        chunk.Next();
                        break;
                    case 92:
                        int numVertexes = chunk.ReadInt();
                        chunk.Next();
                        vertexes = ReadMeshVertexes(numVertexes);
                        break;
                    case 93:
                        int sizeFaceList = chunk.ReadInt();
                        chunk.Next();
                        faces = ReadMeshFaces(sizeFaceList);
                        break;
                    case 94:
                        int numEdges = chunk.ReadInt();
                        chunk.Next();
                        edges = ReadMeshEdges(numEdges);
                        break;
                    case 95:
                        int numCrease = chunk.ReadInt();
                        chunk.Next();
                        if (edges == null)
                            throw new NullReferenceException("The edges list is not initialized.");
                        if (numCrease != edges.Count)
                            throw new Exception("The number of edge creases must be the same as the number of edges.");
                        ReadMeshEdgeCreases(edges);
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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
                double x = chunk.ReadDouble();
                chunk.Next();
                double y = chunk.ReadDouble();
                chunk.Next();
                double z = chunk.ReadDouble();
                chunk.Next();

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
                int indexes = chunk.ReadInt();
                chunk.Next();
                int[] face = new int[indexes];
                for (int j = 0; j < indexes; j++)
                {
                    face[j] = chunk.ReadInt();
                    chunk.Next();
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
                int start = chunk.ReadInt();
                chunk.Next();
                int end = chunk.ReadInt();
                chunk.Next();

                vertexes.Add(new MeshEdge(start, end));
            }
            return vertexes;
        }

        private void ReadMeshEdgeCreases(IEnumerable<MeshEdge> edges)
        {
            foreach (MeshEdge edge in edges)
            {
                edge.Crease = chunk.ReadDouble();
                chunk.Next();
            }
        }

        private Viewport ReadViewport()
        {
            Debug.Assert(chunk.ReadString() == SubclassMarker.Viewport);

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

            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        center.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        viewport.Width = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        viewport.Height = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 68:
                        viewport.Stacking = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 69:
                        viewport.Id = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 12:
                        viewCenter.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        viewCenter.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        snapBase.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        snapBase.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        snapSpacing.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        snapSpacing.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 15:
                        gridSpacing.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        gridSpacing.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 16:
                        viewDirection.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 26:
                        viewDirection.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 36:
                        viewDirection.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 17:
                        viewTarget.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 27:
                        viewTarget.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 37:
                        viewTarget.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 42:
                        viewport.LensLength = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 43:
                        viewport.FrontClipPlane = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 44:
                        viewport.BackClipPlane = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 45:
                        viewport.ViewHeight = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        viewport.SnapAngle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 51:
                        viewport.TwistAngle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 72:
                        viewport.CircleZoomPercent = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 331:
                        Layer layer = (Layer) doc.GetObjectByHandle(chunk.ReadString());
                        viewport.FrozenLayers.Add(layer);
                        chunk.Next();
                        break;
                    case 90:
                        viewport.Status = (ViewportStatusFlags) chunk.ReadInt();
                        chunk.Next();
                        break;
                    case 340:
                        // we will post process the clipping boundary in case it has been defined before the viewport
                        viewports.Add(viewport, chunk.ReadHex());
                        chunk.Next();
                        break;
                    case 110:
                        ucsOrigin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 120:
                        ucsOrigin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 130:
                        ucsOrigin.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 111:
                        ucsXAxis.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 121:
                        ucsXAxis.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 131:
                        ucsXAxis.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 112:
                        ucsYAxis.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 122:
                        ucsYAxis.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 132:
                        ucsYAxis.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            return viewport;
        }

        private Image ReadImage()
        {
            Vector3 position = Vector3.Zero;
            Vector3 u = Vector3.Zero;
            Vector3 v = Vector3.Zero;
            double width = 1.0;
            double height = 1.0;
            string imageDefHandle = null;
            ImageDisplayFlags displayOptions = ImageDisplayFlags.ShowImage | ImageDisplayFlags.ShowImageWhenNotAlignedWithScreen | ImageDisplayFlags.UseClippingBoundary;
            bool clipping = false;
            short brightness = 50;
            short contrast = 50;
            short fade = 0;
            double x = 0.0;
            List<Vector2> vertexes = new List<Vector2>();
            ClippingBoundaryType boundaryType = ClippingBoundaryType.Rectangular;

            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        u.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        u.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        u.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        v.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        v.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        v.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        width = Math.Abs(chunk.ReadDouble()); // just in case, the image width must be always positive
                        chunk.Next();
                        break;
                    case 23:
                        height = Math.Abs(chunk.ReadDouble()); // just in case, the image height must be always positive
                        chunk.Next();
                        break;
                    case 340:
                        imageDefHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 70:
                        displayOptions = (ImageDisplayFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 280:
                        clipping = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 281:
                        brightness = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 282:
                        contrast = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 283:
                        fade = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        boundaryType = (ClippingBoundaryType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 91:
                        // we cannot rely in this information it might or might not appear
                        chunk.Next();
                        break;
                    case 14:
                        x = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        vertexes.Add(new Vector2(x, chunk.ReadDouble()));
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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
                return null;

            imgToImgDefHandles.Add(image, imageDefHandle);

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

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        center.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        radius = chunk.ReadDouble();
                        if (radius <= 0)
                            radius = 1.0;
                        chunk.Next();
                        break;
                    case 50:
                        startAngle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 51:
                        endAngle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        center.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        radius = chunk.ReadDouble();
                        if (radius <= 0)
                            radius = 1.0;
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            chunk.Next();
            while (!dimInfo)
            {
                switch (chunk.Code)
                {
                    case 1:
                        userText = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(userText.Trim(' ', '\t')))
                            userText = " ";
                        chunk.Next();
                        break;
                    case 2:
                        drawingBlockName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (!isBlockEntity)
                            drawingBlock = GetBlock(drawingBlockName);
                        chunk.Next();
                        break;
                    case 3:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.DimStyle;
                        style = GetDimensionStyle(styleName);
                        chunk.Next();
                        break;
                    case 10:
                        defPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        defPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        defPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        midtxtPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        midtxtPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        midtxtPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        dimType = (DimensionTypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        lineSpacingStyle = (MTextLineSpacingStyle) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 41:
                        lineSpacingFactor = chunk.ReadDouble();
                        if (lineSpacingFactor < 0.25 || lineSpacingFactor > 4.0)
                            lineSpacingFactor = 1.0;
                        chunk.Next();
                        break;
                    case 51:
                        dimRot = 360 - chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 53:
                        textRotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 100:
                        string marker = chunk.ReadString();
                        if (marker == SubclassMarker.AlignedDimension ||
                            marker == SubclassMarker.RadialDimension ||
                            marker == SubclassMarker.DiametricDimension ||
                            marker == SubclassMarker.Angular3PointDimension ||
                            marker == SubclassMarker.Angular2LineDimension ||
                            marker == SubclassMarker.OrdinateDimension)
                            dimInfo = true; // we have finished reading the basic dimension info
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
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
            switch (type)
            {
                case DimensionTypeFlags.Aligned:
                    dim = ReadAlignedDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Linear:
                    dim = ReadLinearDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Radius:
                    dim = ReadRadialDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Diameter:
                    dim = ReadDiametricDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Angular3Point:
                    dim = ReadAngular3PointDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Angular:
                    dim = ReadAngular2LineDimension(defPoint, normal);
                    break;
                case DimensionTypeFlags.Ordinate:
                    dim = ReadOrdinateDimension(defPoint, axis, normal, dimRot);
                    break;
                default:
                    throw new ArgumentException(string.Format("The dimension type: {0} is not implemented or unknown.", type));
            }

            if (dim == null)
                return null;

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
                nestedDimensions.Add(dim, drawingBlockName);

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
                            // premature end
                            return overrides; 
                        }

                        // all style overrides are enclosed between XDataCode.ControlString "{" and "}"
                        if (data.Code != XDataCode.ControlString && (string) data.Value != "{")
                        {
                            // premature end
                            return overrides;
                        }

                        if (records.MoveNext())
                        {
                            data = records.Current;
                        }
                        else
                            return overrides; // premature end

                        while (!(data.Code == XDataCode.ControlString && (string)data.Value == "}"))
                        {
                            if (data.Code != XDataCode.Int16)
                                return overrides;
                            short styleOverrideCode = (short)data.Value;
                            if (records.MoveNext())
                                data = records.Current;
                            else
                                return overrides; // premature end

                            //the xData overrides must be read in pairs.
                            //the first is the dimension style property to override, the second is the new value
                            switch (styleOverrideCode)
                            {
                                case 3: // DIMPOST
                                    if (data.Code != XDataCode.String)
                                        return overrides; // premature end
                                    string dimpost = DecodeEncodedNonAsciiCharacters((string) data.Value);
                                    string[] textPrefixSuffix = GetDimStylePrefixAndSuffix(dimpost, '<', '>');
                                    if (!string.IsNullOrEmpty(textPrefixSuffix[0]))
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimPrefix, textPrefixSuffix[0]));
                                    if (!string.IsNullOrEmpty(textPrefixSuffix[1]))
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimSuffix, textPrefixSuffix[1]));
                                    break;
                                case 4: // DIMAPOST
                                    if (data.Code != XDataCode.String)
                                        return overrides; // premature end
                                    string dimapost = DecodeEncodedNonAsciiCharacters((string) data.Value);
                                    string[] altTextPrefixSuffix = GetDimStylePrefixAndSuffix(dimapost, '[', ']');
                                    if (!string.IsNullOrEmpty(altTextPrefixSuffix[0]))
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimPrefix, altTextPrefixSuffix[0]));
                                    if (!string.IsNullOrEmpty(altTextPrefixSuffix[1]))
                                        overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimSuffix, altTextPrefixSuffix[1]));
                                    break;
                                case 40: // DIMSCALE
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimScaleOverall, (double) data.Value));
                                    break;
                                case 41: // DIMASZ:
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ArrowSize, (double) data.Value));
                                    break;
                                case 42: // DIMEXO
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineOffset, (double) data.Value));
                                    break;
                                case 43: // DIMDLI
                                    // not used in overrides
                                    break;
                                case 44: // DIMEXE
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineExtend, (double) data.Value));
                                    break;
                                case 45: // DIMRND
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimRoundoff, (short) data.Value));
                                    break;
                                case 46: // DIMDLE
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineExtend, (double) data.Value));
                                    break;
                                case 47: // DIMTP
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    dimtp = (double) data.Value;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesUpperLimit, dimtp));
                                    break;
                                case 48: // DIMTM
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
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
                                        return overrides; // premature end
                                    dimtfill = (short) data.Value;
                                    break;
                                case 70: // DIMTFILLCLRT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimtfillclrt = AciColor.FromCadIndex((short) data.Value);
                                    break;
                                case 71: // DIMTOLL
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimtol = (short) data.Value;
                                    break;
                                case 72: // DIMLIN
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimlim = (short) data.Value;
                                    break;
                                case 73: // DIMTIH
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(
                                        DimensionStyleOverrideType.TextInsideAlign, (short) data.Value != 0));
                                    break;
                                case 74: // DIMTOH
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(
                                        DimensionStyleOverrideType.TextOutsideAlign, (short) data.Value != 0));
                                    break;
                                case 75: // DIMSE1
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Off, (short) data.Value != 0));
                                    break;
                                case 76: // DIMSE2
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine2Off, (short) data.Value != 0));
                                    break;
                                case 77: // DIMTAD
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextVerticalPlacement, (DimensionStyleTextVerticalPlacement) (short) data.Value));
                                    break;
                                case 78: // DIMZIN
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimzin = (short) data.Value;
                                    break;
                                case 79: // DIMAZIN
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimazin = (short) data.Value;
                                    break;
                                case 140: // DIMTXT
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextHeight, (double) data.Value));
                                    break;
                                case 141: // DIMCEN
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.CenterMarkSize, (double) data.Value));
                                    break;
                                case 143: // DIMALTF
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsMultiplier, (double) data.Value));
                                    break;
                                case 144: // DIMLFAC:
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimScaleLinear, (double) data.Value));
                                    break;
                                case 145: // DIMTVP
                                    // not used
                                    break;
                                case 146: // DIMTFAC
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextFractionHeightScale, (double) data.Value));
                                    break;
                                case 147: // DIMGAP
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextOffset, (double) data.Value));
                                    break;
                                case 148: // DIMALTRND
                                    if (data.Code != XDataCode.Real)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsRoundoff, (double) data.Value));
                                    break;
                                case 170: // DIMALT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsEnabled, (short) data.Value != 0));
                                    break;
                                case 171: // DIMALTD
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AltUnitsLengthPrecision, (short) data.Value));
                                    break;
                                case 172: // DIMTOFL
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitDimLineForce, (short) data.Value != 0));
                                    break;
                                case 173: // DIMSAH
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimsah = (short) data.Value != 0;
                                    break;
                                case 174: // DIMTIX
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextInside, (short) data.Value != 0));
                                    break;
                                case 175: // DIMSOXD
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitDimLineInside, (short) data.Value == 0));
                                    break;
                                case 176: // DIMCLRD:
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 177: // DIMCLRE
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 178: // DIMCLRT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextColor, AciColor.FromCadIndex((short) data.Value)));
                                    break;
                                case 179: // DIMADEC
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.AngularPrecision, (short) data.Value));
                                    break;
                                case 271: // DIMDEC
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.LengthPrecision, (short) data.Value));
                                    break;
                                case 272: // DIMTDEC
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesPrecision, (short) data.Value));
                                    break;
                                case 273: // DIMALTU
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimaltu = (short) data.Value;
                                    break;
                                case 274: // DIMALTTD
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesAlternatePrecision, (short) data.Value));
                                    break;
                                case 275: // DIMAUNIT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimAngularUnits, (AngleUnitType) (short) data.Value));
                                    break;
                                case 276: // DIMFRAC
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FractionalType, (FractionFormatType) (short) data.Value));
                                    break;
                                case 277: // DIMLUNIT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLengthUnits, (LinearUnitType) (short) data.Value));
                                    break;
                                case 278: // DIMDSEP
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DecimalSeparator, (char) (short) data.Value));
                                    break;
                                case 279: // DIMTMOVE
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, (DimensionStyleFitTextMove) (short) data.Value));
                                    break;
                                case 280: // DIMJUST
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextHorizontalPlacement, (DimensionStyleTextHorizontalPlacement) (short) data.Value));
                                    break;
                                case 281: // DIMSD1
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine1Off, (short) data.Value != 0));
                                    break;
                                case 282: // DIMSD2
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine2Off, (short) data.Value != 0));
                                    break;
                                case 283: // DIMTOLJ
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TolerancesVerticalPlacement, (DimensionStyleTolerancesVerticalPlacement) (short) data.Value));
                                    break;
                                case 284: // DIMTZIN
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimtzin = (short) data.Value;
                                    break;
                                case 285: // DIMALTZ
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimaltz = (short) data.Value;
                                    break;
                                case 286: // DIMALTTZ
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    dimalttz = (short) data.Value;
                                    break;
                                case 288: // DIMUPT
                                    // not used
                                    break;
                                case 289: // AIMATFIT
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitOptions, (DimensionStyleFitOptions) (short) data.Value));
                                    break;
                                case 290: // DIMFXLON
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineFixed, (short) data.Value != 0));
                                    break;
                                case 294: // DIMTXTDIRECTION
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextDirection, (DimensionStyleTextDirection) (short) data.Value));
                                    break;
                                case 340: // DIMTXSTY
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    TextStyle dimtxtsty = doc.GetObjectByHandle((string) data.Value) as TextStyle;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextStyle, dimtxtsty == null ? doc.TextStyles[TextStyle.DefaultName] : dimtxtsty));
                                    break;
                                case 341: // DIMLDRBLK
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    BlockRecord dimldrblk = doc.GetObjectByHandle((string) data.Value) as BlockRecord;
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.LeaderArrow, dimldrblk == null ? null : doc.Blocks[dimldrblk.Name]));
                                    break;
                                case 342: // DIMBLK used if DIMSAH is false
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    handleDimblk = (string) data.Value;
                                    break;
                                case 343: // DIMBLK1
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    handleDimblk1 = (string) data.Value;
                                    break;
                                case 344: // DIMBLK2
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    handleDimblk2 = (string) data.Value;
                                    break;
                                case 345: // DIMLTYPE
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    Linetype dimltype = doc.GetObjectByHandle((string) data.Value) as Linetype;
                                    if (dimltype == null)
                                        dimltype = doc.Linetypes[Linetype.DefaultName];
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLinetype, dimltype));
                                    break;
                                case 346: // DIMLTEX1
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    Linetype dimltex1 = doc.GetObjectByHandle((string) data.Value) as Linetype;
                                    if (dimltex1 == null)
                                        dimltex1 = doc.Linetypes[Linetype.DefaultName];
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Linetype, dimltex1));
                                    break;
                                case 347: // DIMLTEX2
                                    if (data.Code != XDataCode.DatabaseHandle)
                                        return overrides; // premature end
                                    Linetype dimltex2 = doc.GetObjectByHandle((string) data.Value) as Linetype;
                                    if (dimltex2 == null)
                                        dimltex2 = doc.Linetypes[Linetype.DefaultName];
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine2Linetype, dimltex2));
                                    break;
                                case 371: // DIMLWD
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLineweight, (Lineweight) (short) data.Value));
                                    break;
                                case 372: // DIMLWE
                                    if (data.Code != XDataCode.Int16)
                                        return overrides; // premature end
                                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLineLineweight, (Lineweight) (short) data.Value));
                                    break;
                            }

                            if (records.MoveNext())
                                data = records.Current;
                            else
                                return overrides; // premature end
                        }
                    }
                }
            }

            // dimension text fill color
            if (dimtfill >= 0)
                overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextFillColor, dimtfill == 2 ? dimtfillclrt : null));

            // dim arrows
            if (dimsah)
            {
                if (!string.IsNullOrEmpty(handleDimblk1))
                {
                    BlockRecord dimblk1 = doc.GetObjectByHandle(handleDimblk1) as BlockRecord;
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, dimblk1 == null ? null : doc.Blocks[dimblk1.Name]));
                }
                if (!string.IsNullOrEmpty(handleDimblk2))
                {
                    BlockRecord dimblk2 = doc.GetObjectByHandle(handleDimblk2) as BlockRecord;
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow2, dimblk2 == null ? null : doc.Blocks[dimblk2.Name]));
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(handleDimblk))
                {
                    BlockRecord dimblk = doc.GetObjectByHandle(handleDimblk) as BlockRecord;
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, dimblk == null ? null : doc.Blocks[dimblk.Name]));
                    overrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow2, dimblk == null ? null : doc.Blocks[dimblk.Name]));
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 13:
                        firstRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        secondRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 13:
                        firstRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        secondRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        rot = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 52:
                        // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the DXF documentation
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 15:
                        circunferenceRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        circunferenceRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 35:
                        circunferenceRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 15:
                        circunferenceRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        circunferenceRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 35:
                        circunferenceRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 13:
                        firstRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        firstRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        firstRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        secondRef.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        secondRef.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        secondRef.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 15:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 35:
                        center.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 13:
                        startFirstLine.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        startFirstLine.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        startFirstLine.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        endFirstLine.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        endFirstLine.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        endFirstLine.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 15:
                        startSecondLine.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        startSecondLine.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 35:
                        startSecondLine.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 16:
                        arcDefinitionPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 26:
                        arcDefinitionPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 36:
                        arcDefinitionPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 13:
                        firstPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        firstPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        firstPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        secondPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        secondPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        secondPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

        private Ellipse ReadEllipse()
        {
            Vector3 center = Vector3.Zero;
            Vector3 axisPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double[] param = new double[2];
            double ratio = 0.0;

            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        center.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        center.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        center.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        axisPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        axisPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        axisPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        ratio = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        param[0] = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 42:
                        param[1] = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            Vector3 ocsAxisPoint = MathHelper.Transform(axisPoint, normal, CoordinateSystem.World, CoordinateSystem.Object);

            double rotation = Vector2.Angle(new Vector2(ocsAxisPoint.X, ocsAxisPoint.Y));
            double majorAxis = 2*axisPoint.Modulus();
            double minorAxis = majorAxis*ratio;

            Ellipse ellipse = new Ellipse
            {
                MajorAxis = majorAxis,
                MinorAxis = minorAxis,
                Rotation = rotation*MathHelper.RadToDeg,
                Center = center,
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
                double a = ellipse.MajorAxis*0.5;
                double b = ellipse.MinorAxis*0.5;

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        location.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        location.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        location.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 50:
                        rotation = 360.0 - chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

        private Face3d ReadFace3d()
        {
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            Face3dEdgeFlags flags = Face3dEdgeFlags.Visibles;
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        v0.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        v0.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        v0.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        v1.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        v1.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        v1.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        v2.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        v2.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        v2.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        v3.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        v3.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        v3.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (Face3dEdgeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            Face3d entity = new Face3d
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        size = chunk.ReadDouble();
                        if (MathHelper.IsZero(size)) size = 1.0;
                        if (size < 0.0)
                        {
                            size = Math.Abs(size);
                            rotateNegativeSize = 180;
                        }
                        chunk.Next();
                        break;
                    case 2:
                        name = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        widthFactor = chunk.ReadDouble();
                        if (MathHelper.IsZero(widthFactor)) widthFactor = 1.0;
                        chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            // if the shape has no name it will be skipped
            if (string.IsNullOrEmpty(name)) return null;

            // the shape definition does not store any information about the ShapeStyle where the geometry of the shape is stored
            // we will look for a shape with the specified name inside any of the shape styles defined in the document.
            // if none are found the shape will be skipped.
            ShapeStyle style = doc.ShapeStyles.ContainsShapeName(name);
            // if a shape style has not been found that contains a shape definition with the specified name, the shape will be skipped
            if (style == null) return null;

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        v0.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        v0.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        v0.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        v1.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        v1.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        v1.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        v2.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        v2.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        v2.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        v3.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        v3.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        v3.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        v0.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        v0.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        v0.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        v1.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        v1.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        v1.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        v2.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        v2.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        v2.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        v3.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        v3.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        v3.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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
            SplinetypeFlags flags = SplinetypeFlags.None;
            Vector3 normal = Vector3.UnitZ;
            short degree = 3;
            int ctrlPointIndex = -1;

            List<double> knots = new List<double>();
            List<SplineVertex> ctrlPoints = new List<SplineVertex>();
            double ctrlX = 0;
            double ctrlY = 0;
            double ctrlZ;
            double ctrlWeigth = -1;

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (SplinetypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        degree = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        // the spline entity can actually hold a larger number of knots, we cannot use this information it might be wrong.
                        //short numKnots = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        // the spline entity can actually hold a larger number of control points
                        //short numCtrlPoints = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 74:
                        // the spline entity can actually hold a larger number of fit points, we cannot use this information it might be wrong.
                        //numFitPoints = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 42:
                        knotTolerance = chunk.ReadDouble();
                        if (knotTolerance <= 0) knotTolerance = 0.0000001;
                        chunk.Next();
                        break;
                    case 43:
                        ctrlPointTolerance = chunk.ReadDouble();
                        if (ctrlPointTolerance <= 0) ctrlPointTolerance = 0.0000001;
                        chunk.Next();
                        break;
                    case 44:
                        fitTolerance = chunk.ReadDouble();
                        if (fitTolerance <= 0) fitTolerance = 0.0000000001;
                        chunk.Next();
                        break;
                    case 12:
                        stX = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        stY = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        stZ = chunk.ReadDouble();
                        startTangent = new Vector3(stX, stY, stZ);
                        chunk.Next();
                        break;
                    case 13:
                        etX = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        etY = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        etZ = chunk.ReadDouble();
                        endTangent = new Vector3(etX, etY, etZ);
                        chunk.Next();
                        break;
                    case 40:
                        // multiple code 40 entries, one per knot value
                        knots.Add(chunk.ReadDouble());
                        chunk.Next();
                        break;
                    case 10:
                        ctrlX = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        ctrlY = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        ctrlZ = chunk.ReadDouble();
                        if (ctrlWeigth <= 0)
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ));
                            ctrlPointIndex = ctrlPoints.Count - 1;
                        }
                        else
                        {
                            ctrlPoints.Add(new SplineVertex(ctrlX, ctrlY, ctrlZ, ctrlWeigth));
                            ctrlPointIndex = -1;
                        }
                        chunk.Next();
                        break;
                    case 41:
                        // code 41 might appear before or after the control point coordinates.
                        double weigth = chunk.ReadDouble();
                        if (weigth <= 0.0)
                            weigth = 1.0;

                        if (ctrlPointIndex == -1)
                        {
                            ctrlWeigth = weigth;
                        }
                        else
                        {
                            ctrlPoints[ctrlPointIndex].Weight = weigth;
                            ctrlWeigth = -1;
                        }
                        chunk.Next();
                        break;
                    case 11:
                        fitX = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        fitY = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        fitZ = chunk.ReadDouble();
                        fitPoints.Add(new Vector3(fitX, fitY, fitZ));
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            Spline entity;
            SplineCreationMethod method = flags.HasFlag(SplinetypeFlags.FitPointCreationMethod) ? SplineCreationMethod.FitPoints : SplineCreationMethod.ControlPoints;

            if (method == SplineCreationMethod.FitPoints && ctrlPoints.Count == 0)
            {
                entity = new Spline(fitPoints)
                {
                    KnotTolerance = knotTolerance,
                    CtrlPointTolerance = ctrlPointTolerance,
                    FitTolerance = fitTolerance,
                    StartTangent = startTangent,
                    EndTangent = endTangent
                };
            }
            else
            {
                bool isPeriodic = flags.HasFlag(SplinetypeFlags.ClosedPeriodicSpline) || flags.HasFlag(SplinetypeFlags.Periodic);
                entity = new Spline(ctrlPoints, knots, degree, fitPoints, method, isPeriodic)
                {
                    KnotTolerance = knotTolerance,
                    CtrlPointTolerance = ctrlPointTolerance,
                    FitTolerance = fitTolerance,
                    StartTangent = startTangent,
                    EndTangent = endTangent
                };
            }

            if (flags.HasFlag(SplinetypeFlags.FitChord))
                entity.KnotParameterization = SplineKnotParameterization.FitChord;
            else if (flags.HasFlag(SplinetypeFlags.FitSqrtChord))
                entity.KnotParameterization = SplineKnotParameterization.FitSqrtChord;
            else if (flags.HasFlag(SplinetypeFlags.FitUniform))
                entity.KnotParameterization = SplineKnotParameterization.FitUniform;
            else if (flags.HasFlag(SplinetypeFlags.FitCustom))
                entity.KnotParameterization = SplineKnotParameterization.FitCustom;

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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        blockName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (!isBlockEntity)
                            block = GetBlock(blockName);
                        chunk.Next();
                        break;
                    case 10:
                        basePoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        basePoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        basePoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        scale.X = chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.X)) scale.X = 1.0; // just in case, the insert scale components cannot be zero
                        chunk.Next();
                        break;
                    case 42:
                        scale.Y = chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.Y)) scale.Y = 1.0; // just in case, the insert scale components cannot be zero
                        chunk.Next();
                        break;
                    case 43:
                        scale.Z = chunk.ReadDouble();
                        if (MathHelper.IsZero(scale.Z)) scale.Z = 1.0; // just in case, the insert scale components cannot be zero
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            if (chunk.ReadString() == DxfObjectCode.Attribute)
            {
                while (chunk.ReadString() != DxfObjectCode.EndSequence)
                {
                    Attribute attribute = ReadAttribute(block, isBlockEntity);
                    if (attribute != null)
                        attributes.Add(attribute);
                }
            }

            string endSequenceHandle = string.Empty;
            if (chunk.ReadString() == DxfObjectCode.EndSequence)
            {
                // read the end sequence object until a new element is found
                chunk.Next();
                while (chunk.Code != 0)
                {
                    switch (chunk.Code)
                    {
                        case 5:
                            endSequenceHandle = chunk.ReadHex();
                            chunk.Next();
                            break;
                        case 8:
                            // the EndSquence layer and the Insert layer are the same
                            chunk.Next();
                            break;
                        default:
                            chunk.Next();
                            break;
                    }
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
            insert.EndSequence.Handle = endSequenceHandle;
            insert.XData.AddRange(xData);

            // post process nested inserts
            if (isBlockEntity)
                nestedInserts.Add(insert, blockName);

            return insert;
        }

        private Line ReadLine()
        {
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        start.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        start.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        start.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        end.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        end.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        end.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        origin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        origin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        origin.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        direction.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        direction.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        direction.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 10:
                        origin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        origin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        origin.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        direction.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        direction.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        direction.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        // the MLineStyle is defined in the objects sections after the definition of the entity, something similar happens with the image entity
                        // the MLineStyle will be applied to the MLine after parsing the whole file
                        styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.CMLStyle;
                        chunk.Next();
                        break;
                    case 40:
                        scale = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        justification = (MLineJustification) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        flags = (MLineFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        numVertexes = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        numStyleElements = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 10:
                        // this info is not needed it is repeated in the vertexes list
                        chunk.Next();
                        break;
                    case 20:
                        // this info is not needed it is repeated in the vertexes list
                        chunk.Next();
                        break;
                    case 30:
                        // this info is not needed it is repeated in the vertexes list
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        // the info that follows contains the information on the vertexes of the MLine
                        segments = ReadMLineSegments(numVertexes, numStyleElements, normal, out elevation);
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
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
            mLineToStyleNames.Add(mline, styleName);

            return mline;
        }

        private List<MLineVertex> ReadMLineSegments(int numVertexes, int numStyleElements, Vector3 normal, out double elevation)
        {
            elevation = 0.0;

            List<MLineVertex> segments = new List<MLineVertex>();
            for (int i = 0; i < numVertexes; i++)
            {
                Vector3 vertex = new Vector3();
                vertex.X = chunk.ReadDouble(); // code 11
                chunk.Next();
                vertex.Y = chunk.ReadDouble(); // code 21
                chunk.Next();
                vertex.Z = chunk.ReadDouble(); // code 31
                chunk.Next();

                Vector3 dir = new Vector3();
                dir.X = chunk.ReadDouble(); // code 12
                chunk.Next();
                dir.Y = chunk.ReadDouble(); // code 22
                chunk.Next();
                dir.Z = chunk.ReadDouble(); // code 32
                chunk.Next();

                Vector3 mitter = new Vector3();
                mitter.X = chunk.ReadDouble(); // code 13
                chunk.Next();
                mitter.Y = chunk.ReadDouble(); // code 23
                chunk.Next();
                mitter.Z = chunk.ReadDouble(); // code 33
                chunk.Next();

                List<double>[] distances = new List<double>[numStyleElements];
                for (int j = 0; j < numStyleElements; j++)
                {
                    distances[j] = new List<double>();
                    short numDistances = chunk.ReadShort(); // code 74
                    chunk.Next();
                    for (short k = 0; k < numDistances; k++)
                    {
                        distances[j].Add(chunk.ReadDouble()); // code 41
                        chunk.Next();
                    }

                    // no more info is needed, fill parameters are not supported
                    short numFillParams = chunk.ReadShort(); // code 75
                    chunk.Next();
                    for (short k = 0; k < numFillParams; k++)
                    {
                        //double param = chunk.ReadDouble(); // code 42
                        chunk.Next();
                    }
                }

                // we need to convert WCS coordinates to OCS coordinates
                Matrix3 trans = MathHelper.ArbitraryAxis(normal).Transpose();
                vertex = trans*vertex;
                dir = trans*dir;
                mitter = trans*mitter;

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
            double elevation = 0.0;
            double thickness = 0.0;
            PolylinetypeFlags flags = PolylinetypeFlags.OpenPolyline;
            double constantWidth = -1.0;
            List<LwPolylineVertex> polVertexes = new List<LwPolylineVertex>();
            LwPolylineVertex v = null;
            double vX = 0.0;
            Vector3 normal = Vector3.UnitZ;

            List<XData> xData = new List<XData>();

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 38:
                        elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 43:
                        // constant width (optional; default = 0). If present it will override any vertex width (codes 40 and/or 41)
                        constantWidth = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (PolylinetypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        chunk.Next();
                        break;
                    case 10:
                        vX = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        double vY = chunk.ReadDouble();
                        v = new LwPolylineVertex(vX, vY);
                        polVertexes.Add(v);
                        chunk.Next();
                        break;
                    case 40:
                        double startWidth = chunk.ReadDouble();
                        if (v != null)
                            if (startWidth >= 0.0)
                                v.StartWidth = startWidth;
                        chunk.Next();
                        break;
                    case 41:
                        double endWidth = chunk.ReadDouble();
                        if (v != null)
                            if (endWidth >= 0.0)
                                v.EndWidth = endWidth;
                        chunk.Next();
                        break;
                    case 42:
                        if (v != null)
                            v.Bulge = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            LwPolyline entity = new LwPolyline
            {
                Elevation = elevation,
                Thickness = thickness,
                Flags = flags,
                Normal = normal
            };

            entity.Vertexes.AddRange(polVertexes);

            if (constantWidth >= 0.0)
                entity.SetConstantWidth(constantWidth);

            entity.XData.AddRange(xData);

            return entity;
        }

        private EntityObject ReadPolyline()
        {
            // the entity Polyline in DXF can actually hold three kinds of entities
            // 3d polyline is the generic polyline
            // polyface mesh
            // polylines 2d is the old way of writing polylines the AutoCAD2000 and newer always use LwPolylines to define a 2d polyline
            // this way of reading 2d polylines is here for compatibility reasons with older DXF versions.
            PolylinetypeFlags flags = PolylinetypeFlags.OpenPolyline;
            PolylineSmoothType smoothType = PolylineSmoothType.NoSmooth;
            double elevation = 0.0;
            double thickness = 0.0;
            Vector3 normal = Vector3.UnitZ;
            List<Vertex> vertexes = new List<Vertex>();
            List<XData> xData = new List<XData>();

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 30:
                        elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 39:
                        thickness = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (PolylinetypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 75:
                        smoothType = (PolylineSmoothType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numVertexes = int.Parse(code.Value); code = ReadCodePair();
                        chunk.Next();
                        break;
                    case 72:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            //begin to read the vertex list (although it is not recommended the vertex list might have 0 entries)
            while (chunk.ReadString() != DxfObjectCode.EndSequence)
            {
                if (chunk.ReadString() == DxfObjectCode.Vertex)
                {
                    Vertex vertex = ReadVertex();
                    vertexes.Add(vertex);
                }
            }

            // read the end sequence object until a new element is found
            chunk.Next();
            string endSequenceHandle = null;
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        endSequenceHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 8:
                        // the polyline EndSequence layer should be the same as the polyline layer
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            EntityObject pol;
            bool isClosed = flags.HasFlag(PolylinetypeFlags.ClosedPolylineOrClosedPolygonMeshInM);

            //to avoid possible errors between the vertex type and the polyline type
            //the polyline type will decide which information to use from the read vertex
            if (flags.HasFlag(PolylinetypeFlags.Polyline3D) || flags.HasFlag(PolylinetypeFlags.SplineFit))
            {
                List<PolylineVertex> polyline3dVertexes = new List<PolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    PolylineVertex vertex = new PolylineVertex
                    {
                        Flags = v.Flags,
                        Position = v.Position,
                        Handle = v.Handle,
                    };
                    polyline3dVertexes.Add(vertex);
                }

                pol = new Polyline(polyline3dVertexes, isClosed)
                {
                    Flags = flags,
                    SmoothType = smoothType,
                    Normal = normal
                };
                ((Polyline) pol).EndSequence.Handle = endSequenceHandle;
            }
            else if (flags.HasFlag(PolylinetypeFlags.PolyfaceMesh))
            {
                //the vertex list created contains vertex and face information
                List<PolyfaceMeshVertex> polyfaceVertexes = new List<PolyfaceMeshVertex>();
                List<PolyfaceMeshFace> polyfaceFaces = new List<PolyfaceMeshFace>();
                foreach (Vertex v in vertexes)
                {
                    if (v.Flags.HasFlag(VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh))
                    {
                        PolyfaceMeshVertex vertex = new PolyfaceMeshVertex
                        {
                            Position = v.Position,
                            Handle = v.Handle,
                        };
                        polyfaceVertexes.Add(vertex);
                    }
                    else if (v.Flags.HasFlag(VertexTypeFlags.PolyfaceMeshVertex))
                    {
                        PolyfaceMeshFace vertex = new PolyfaceMeshFace(v.VertexIndexes)
                        {
                            Handle = v.Handle
                        };
                        polyfaceFaces.Add(vertex);
                    }
                }
                pol = new PolyfaceMesh(polyfaceVertexes, polyfaceFaces)
                {
                    Normal = normal
                };
                ((PolyfaceMesh) pol).EndSequence.Handle = endSequenceHandle;
            }
            else
            {
                List<LwPolylineVertex> polylineVertexes = new List<LwPolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    LwPolylineVertex vertex = new LwPolylineVertex
                    {
                        Position = new Vector2(v.Position.X, v.Position.Y),
                        StartWidth = v.StartWidth,
                        Bulge = v.Bulge,
                        EndWidth = v.EndWidth,
                    };

                    polylineVertexes.Add(vertex);
                }

                pol = new LwPolyline(polylineVertexes, isClosed)
                {
                    Flags = flags,
                    Thickness = thickness,
                    Elevation = elevation,
                    Normal = normal,
                };
            }

            pol.XData.AddRange(xData);

            return pol;
        }

        private Vertex ReadVertex()
        {
            string handle = string.Empty;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            Linetype linetype = Linetype.ByLayer;
            Vector3 position = new Vector3();
            double startWidth = 0.0;
            double endWidth = 0.0;
            double bulge = 0.0;
            List<short> vertexIndexes = new List<short>();
            VertexTypeFlags flags = VertexTypeFlags.PolylineVertex;

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 8:
                        string layerName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        layer = GetLayer(layerName);
                        chunk.Next();
                        break;
                    case 62: //ACI color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 6:
                        string linetypeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        linetype = GetLinetype(linetypeName);
                        chunk.Next();
                        break;
                    case 10:
                        position.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        position.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        position.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        startWidth = chunk.ReadDouble();
                        if (startWidth < 0.0)
                            startWidth = 0.0;
                        chunk.Next();
                        break;
                    case 41:
                        endWidth = chunk.ReadDouble();
                        if (endWidth < 0.0)
                            endWidth = 0.0;
                        chunk.Next();
                        break;
                    case 42:
                        bulge = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 71:
                        vertexIndexes.Add(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 72:
                        vertexIndexes.Add(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 73:
                        vertexIndexes.Add(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 74:
                        vertexIndexes.Add(chunk.ReadShort());
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 1:
                        textString = chunk.ReadString();
                        chunk.Next();
                        break;
                    case 10:
                        firstAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        secondAlignmentPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();
                        if (height <= 0.0)
                            height = doc.DrawingVariables.TextSize;
                        chunk.Next();
                        break;
                    case 41:
                        widthFactor = chunk.ReadDouble();
                        if (widthFactor < 0.01 || widthFactor > 100.0)
                            widthFactor = doc.DrawingVariables.TextSize;
                        chunk.Next();
                        break;
                    case 50:
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 51:
                        obliqueAngle = chunk.ReadDouble();
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                            obliqueAngle = 0.0;
                        chunk.Next();
                        break;
                    case 7:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.TextStyle;
                        style = GetTextStyle(styleName);
                        chunk.Next();
                        break;
                    case 71:
                        short textGeneration = chunk.ReadShort();
                        if (textGeneration - 2 == 0)
                            isBackward = true;
                        if (textGeneration - 4 == 0)
                            isUpsideDown = true;
                        if (textGeneration - 6 == 0)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        chunk.Next();
                        break;
                    case 72:
                        horizontalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        verticalAlignment = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            Vector3 ocsBasePoint;

            if (alignment == TextAlignment.BaselineLeft)
            {
                ocsBasePoint = firstAlignmentPoint;
            }
            else if (alignment == TextAlignment.Fit || alignment == TextAlignment.Aligned)
            {
                width = (secondAlignmentPoint - firstAlignmentPoint).Modulus();
                ocsBasePoint = firstAlignmentPoint;
            }
            else
            {
                ocsBasePoint = secondAlignmentPoint;
            }

            textString = DecodeEncodedNonAsciiCharacters(textString);
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

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 1:
                        textString = string.Concat(textString, chunk.ReadString());
                        chunk.Next();
                        break;
                    case 3:
                        textString = string.Concat(textString, chunk.ReadString());
                        chunk.Next();
                        break;
                    case 10:
                        insertionPoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        insertionPoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 30:
                        insertionPoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        direction.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        direction.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 31:
                        direction.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 40:
                        height = chunk.ReadDouble();
                        if (height <= 0.0)
                            height = doc.DrawingVariables.TextSize;
                        chunk.Next();
                        break;
                    case 41:
                        rectangleWidth = chunk.ReadDouble();
                        if (rectangleWidth < 0.0)
                            rectangleWidth = 0.0;
                        chunk.Next();
                        break;
                    case 44:
                        lineSpacing = chunk.ReadDouble();
                        if (lineSpacing < 0.25 || lineSpacing > 4.0)
                            lineSpacing = 1.0;
                        chunk.Next();
                        break;
                    case 50: // even if the AutoCAD DXF documentation says that the rotation is in radians, this is wrong this value is in degrees
                        isRotationDefined = true;
                        rotation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 7:
                        string styleName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        if (string.IsNullOrEmpty(styleName))
                            styleName = doc.DrawingVariables.TextStyle;
                        style = GetTextStyle(styleName);
                        chunk.Next();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        drawingDirection = (MTextDrawingDirection)chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        spacingStyle = (MTextLineSpacingStyle) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    case 101:
                        // once again Autodesk not documenting its own stuff.
                        // the code 101 was introduced in AutoCad 2018, as far as I know, it is not documented anywhere in the official DXF help.
                        // after this value, it seems that appears the definition of who knows what, therefore everything after this 101 code will be skipped
                        // until the end of the entity definition or the XData information
                        //string unknown = chunk.ReadString();
                        while (!(chunk.Code == 0 || chunk.Code == 1001))
                            chunk.Next();                                         
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
                        break;
                }
            }

            textString = DecodeEncodedNonAsciiCharacters(textString);
            if (!isBinary)
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

            chunk.Next();

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 30:
                        elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 210:
                        normal.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 220:
                        normal.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 230:
                        normal.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 91:
                        // the next lines hold the information about the hatch boundary paths
                        int numPaths = chunk.ReadInt();
                        paths = ReadHatchBoundaryPaths(numPaths);
                        break;
                    case 70:
                        // Solid fill flag
                        fill = (HatchFillType) chunk.ReadShort();
                        //if (fill == HatchFillType.SolidFill) name = PredefinedHatchPatternName.Solid;
                        chunk.Next();
                        break;
                    case 71:
                        // Associativity flag (associative = 1; non-associative = 0); for MPolygon, solid-fill flag (has solid fill = 1; lacks solid fill = 0)
                        if (chunk.ReadShort() != 0)
                            associative = true;
                        chunk.Next();
                        break;
                    case 75:
                        // the next lines hold the information about the hatch pattern
                        pattern = ReadHatchPattern(name);
                        pattern.Fill = fill;
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(GetApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");

                        chunk.Next();
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

            hatchToPaths.Add(entity, paths);

            entity.XData.AddRange(xData);

            // here is where DXF stores the pattern origin
            XData patternOrigin;
            Vector2 origin = Vector2.Zero;
            if (entity.XData.TryGetValue(ApplicationRegistry.DefaultName, out patternOrigin))
            {
                foreach (XDataRecord record in patternOrigin.XDataRecord)
                {
                    if (record.Code == XDataCode.RealX)
                        origin.X = (double) record.Value;
                    else if (record.Code == XDataCode.RealY)
                        origin.Y = (double) record.Value;
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

            chunk.Next();
            while (paths.Count < numPaths)
            {
                HatchBoundaryPath path;
                switch (chunk.Code)
                {
                    case 92:
                        pathType = (HatchBoundaryPathTypeFlags) chunk.ReadInt();
                        // adding External and Derived to all path type flags solves an strange problem with code 98 not found,
                        // it seems related to the code 47 that appears before, only some combinations of flags are affected
                        // this is what the documentation says about code 47:
                        // Pixel size used to determine the density to perform various intersection and ray casting operations
                        // in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
                        pathType = pathType | HatchBoundaryPathTypeFlags.External | HatchBoundaryPathTypeFlags.Derived;

                        if (pathType.HasFlag(HatchBoundaryPathTypeFlags.Polyline))
                        {
                            path = ReadEdgePolylineBoundaryPath();
                            path.PathType = pathType;
                            paths.Add(path);
                        }
                        else
                            chunk.Next();
                        break;
                    case 93:
                        int numEdges = chunk.ReadInt();
                        path = ReadEdgeBoundaryPath(numEdges);
                        path.PathType = pathType;
                        paths.Add(path);
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }
            return paths;
        }

        private HatchBoundaryPath ReadEdgePolylineBoundaryPath()
        {
            HatchBoundaryPath.Polyline poly = new HatchBoundaryPath.Polyline();

            chunk.Next();

            bool hasBulge = chunk.ReadShort() != 0; // code 72
            chunk.Next();

            // is polyline closed
            // poly.IsClosed = chunk.ReadShort() != 0; // code 73, this value must always be true
            chunk.Next();

            int numVertexes = chunk.ReadInt(); // code 93
            poly.Vertexes = new Vector3[numVertexes];
            chunk.Next();

            for (int i = 0; i < numVertexes; i++)
            {
                double bulge = 0.0;
                double x = chunk.ReadDouble(); // code 10
                chunk.Next();
                double y = chunk.ReadDouble(); // code 20
                chunk.Next();
                if (hasBulge)
                {
                    bulge = chunk.ReadDouble(); // code 42
                    chunk.Next();
                }
                poly.Vertexes[i] = new Vector3(x, y, bulge);
            }
            HatchBoundaryPath path = new HatchBoundaryPath(new List<HatchBoundaryPath.Edge> {poly});

            // read all referenced entities
            Debug.Assert(chunk.Code == 97, "The reference count code 97 was expected.");
            int numBoundaryObjects = chunk.ReadInt();
            hatchContourns.Add(path, new List<string>(numBoundaryObjects));
            chunk.Next();
            for (int i = 0; i < numBoundaryObjects; i++)
            {
                Debug.Assert(chunk.Code == 330, "The reference handle code 330 was expected.");
                hatchContourns[path].Add(chunk.ReadString());
                chunk.Next();
            }

            return path;
        }

        private HatchBoundaryPath ReadEdgeBoundaryPath(int numEdges)
        {
            // the information of the boundary path data always appear exactly as it is read
            List<HatchBoundaryPath.Edge> entities = new List<HatchBoundaryPath.Edge>();
            chunk.Next();

            while (entities.Count < numEdges)
            {
                // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline
                HatchBoundaryPath.EdgeType type = (HatchBoundaryPath.EdgeType) chunk.ReadShort();
                switch (type)
                {
                    case HatchBoundaryPath.EdgeType.Line:
                        chunk.Next();
                        // line
                        double lX1 = chunk.ReadDouble(); // code 10
                        chunk.Next();
                        double lY1 = chunk.ReadDouble(); // code 20
                        chunk.Next();
                        double lX2 = chunk.ReadDouble(); // code 11
                        chunk.Next();
                        double lY2 = chunk.ReadDouble(); // code 21
                        chunk.Next();

                        HatchBoundaryPath.Line line = new HatchBoundaryPath.Line
                        {
                            Start = new Vector2(lX1, lY1),
                            End = new Vector2(lX2, lY2)
                        };
                        entities.Add(line);
                        break;
                    case HatchBoundaryPath.EdgeType.Arc:
                        chunk.Next();
                        // circular arc
                        double aX = chunk.ReadDouble(); // code 10
                        chunk.Next();
                        double aY = chunk.ReadDouble(); // code 40
                        chunk.Next();
                        double aR = chunk.ReadDouble(); // code 40
                        chunk.Next();
                        double aStart = chunk.ReadDouble(); // code 50
                        chunk.Next();
                        double aEnd = chunk.ReadDouble(); // code 51
                        chunk.Next();
                        bool aCCW = chunk.ReadShort() != 0; // code 73
                        chunk.Next();

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
                        chunk.Next();
                        // elliptic arc
                        double eX = chunk.ReadDouble(); // code 10
                        chunk.Next();
                        double eY = chunk.ReadDouble(); // code 20
                        chunk.Next();
                        double eAxisX = chunk.ReadDouble(); // code 11
                        chunk.Next();
                        double eAxisY = chunk.ReadDouble(); // code 21
                        chunk.Next();
                        double eAxisRatio = chunk.ReadDouble(); // code 40
                        chunk.Next();
                        double eStart = chunk.ReadDouble(); // code 50
                        chunk.Next();
                        double eEnd = chunk.ReadDouble(); // code 51
                        chunk.Next();
                        bool eCCW = chunk.ReadShort() != 0; // code 73
                        chunk.Next();

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
                        chunk.Next();
                        // spline

                        short degree = (short) chunk.ReadInt(); // code 94
                        chunk.Next();

                        bool isRational = chunk.ReadShort() != 0; // code 73
                        chunk.Next();

                        bool isPeriodic = chunk.ReadShort() != 0; // code 74
                        chunk.Next();

                        int numKnots = chunk.ReadInt(); // code 95
                        double[] knots = new double[numKnots];
                        chunk.Next();

                        int numControlPoints = chunk.ReadInt(); // code 96
                        Vector3[] controlPoints = new Vector3[numControlPoints];
                        chunk.Next();

                        for (int i = 0; i < numKnots; i++)
                        {
                            knots[i] = chunk.ReadDouble(); // code 40
                            chunk.Next();
                        }

                        for (int i = 0; i < numControlPoints; i++)
                        {
                            double x = chunk.ReadDouble(); // code 10
                            chunk.Next();

                            double y = chunk.ReadDouble(); // code 20
                            chunk.Next();

                            // control point weight might not be present
                            double w = 1.0;
                            if (chunk.Code == 42)
                            {
                                w = chunk.ReadDouble(); // code 42
                                chunk.Next();
                            }

                            controlPoints[i] = new Vector3(x, y, w);
                        }

                        // this information is only required for AutoCAD version 2010 and newer
                        // stores information about spline fit point (the spline entity does not make use of this information)
                        if (doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2010)
                        {
                            int numFitData = chunk.ReadInt(); // code 97
                            chunk.Next();
                            for (int i = 0; i < numFitData; i++)
                            {
                                //double fitX = chunk.ReadDouble(); // code 11
                                chunk.Next();
                                //double fitY = chunk.ReadDouble(); // code 21
                                chunk.Next();
                            }

                            // the info on start tangent might not appear
                            if (chunk.Code == 12)
                            {
                                //double startTanX = chunk.ReadDouble(); // code 12
                                chunk.Next();
                                //double startTanY = chunk.ReadDouble(); // code 22
                                chunk.Next();
                            }
                            // the info on end tangent might not appear
                            if (chunk.Code == 13)
                            {
                                //double endTanX = chunk.ReadDouble(); // code 13
                                chunk.Next();
                                //double endTanY = chunk.ReadDouble(); // code 23
                                chunk.Next();
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
            Debug.Assert(chunk.Code == 97, "The reference count code 97 was expected.");
            int numBoundaryObjects = chunk.ReadInt();
            hatchContourns.Add(path, new List<string>(numBoundaryObjects));
            chunk.Next();
            for (int i = 0; i < numBoundaryObjects; i++)
            {
                Debug.Assert(chunk.Code == 330, "The reference handle code 330 was expected.");
                hatchContourns[path].Add(chunk.ReadString());
                chunk.Next();
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

            while (chunk.Code != 0 && chunk.Code != 1001)
            {
                switch (chunk.Code)
                {
                    case 52:
                        angle = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        scale = chunk.ReadDouble();
                        if (scale <= 0)
                            scale = 1.0;
                        chunk.Next();
                        break;
                    case 47:
                        chunk.Next();
                        break;
                    case 98:
                        chunk.Next();
                        break;
                    case 10:
                        chunk.Next();
                        break;
                    case 20:
                        chunk.Next();
                        break;
                    case 75:
                        style = (HatchStyle) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 76:
                        type = (HatchType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 77:
                        // hatch pattern double flag (not used)
                        chunk.Next();
                        break;
                    case 78:
                        // number of pattern definition lines
                        short numLines = chunk.ReadShort();
                        lineDefinitions = ReadHatchPatternDefinitionLine(scale, angle, numLines);
                        break;
                    case 450:
                        if (chunk.ReadInt() == 1)
                        {
                            isGradient = true; // gradient pattern
                            hatch = ReadHatchGradientPattern();
                        }
                        else
                            chunk.Next(); // solid hatch, we do not need to read anything else
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            if (!isGradient)
                hatch = new HatchPattern(name);

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
            //dxfPairInfo = ReadCodePair();  // code 450 not needed
            chunk.Next(); // code 451 not needed
            chunk.Next();
            double angle = chunk.ReadDouble(); // code 460
            chunk.Next();
            bool centered = (int) chunk.ReadDouble() == 0; // code 461
            chunk.Next();
            bool singleColor = chunk.ReadInt() != 0; // code 452
            chunk.Next();
            double tint = chunk.ReadDouble(); // code 462
            chunk.Next(); // code 453 not needed

            chunk.Next(); // code 463 not needed (0.0)
            chunk.Next(); // code 63
            chunk.Next(); // code 421
            AciColor color1 = AciColor.FromTrueColor(chunk.ReadInt());

            chunk.Next(); // code 463 not needed (1.0)
            chunk.Next(); // code 63
            chunk.Next(); // code 421
            AciColor color2 = AciColor.FromTrueColor(chunk.ReadInt());

            chunk.Next(); // code 470
            string typeName = chunk.ReadString();
            
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

            chunk.Next();
            for (int i = 0; i < numLines; i++)
            {
                Vector2 origin = Vector2.Zero;
                Vector2 delta = Vector2.Zero;

                double angle = chunk.ReadDouble(); // code 53
                chunk.Next();

                origin.X = chunk.ReadDouble(); // code 43
                chunk.Next();

                origin.Y = chunk.ReadDouble(); // code 44
                chunk.Next();

                delta.X = chunk.ReadDouble(); // code 45
                chunk.Next();

                delta.Y = chunk.ReadDouble(); // code 46
                chunk.Next();

                short numSegments = chunk.ReadShort(); // code 79
                chunk.Next();

                // Pattern fill data. In theory this should hold the same information as the pat file but for unknown reason the DXF requires global data instead of local.
                // this means we have to convert the global data into local, since we are storing the pattern line definition as it appears in the acad.pat file.
                double sinOrigin = Math.Sin(patternAngle*MathHelper.DegToRad);
                double cosOrigin = Math.Cos(patternAngle*MathHelper.DegToRad);
                origin = new Vector2(cosOrigin*origin.X/patternScale + sinOrigin*origin.Y/patternScale, -sinOrigin*origin.X/patternScale + cosOrigin*origin.Y/patternScale);

                double sinDelta = Math.Sin(angle*MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle*MathHelper.DegToRad);
                delta = new Vector2(cosDelta*delta.X/patternScale + sinDelta*delta.Y/patternScale, -sinDelta*delta.X/patternScale + cosDelta*delta.Y/patternScale);

                HatchPatternLineDefinition lineDefiniton = new HatchPatternLineDefinition
                {
                    Angle = angle - patternAngle,
                    Origin = origin,
                    Delta = delta,
                };

                for (int j = 0; j < numSegments; j++)
                {
                    // positive values means solid segments and negative values means spaces (one entry per element)
                    lineDefiniton.DashPattern.Add(chunk.ReadDouble()/patternScale); // code 49
                    chunk.Next();
                }

                lineDefinitions.Add(lineDefiniton);
            }

            return lineDefinitions;
        }

        private void ReadUnknowEntity()
        {
            // if the entity is unknown keep reading until an end of section or a new entity is found
            chunk.Next();
            while (chunk.Code != 0)
            {
                chunk.Next();
            }
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
            doc.Groups = new Groups(doc, groupsHandle);
            doc.Layouts = new Layouts(doc, layoutsHandle);
            doc.MlineStyles = new MLineStyles(doc, mlineStylesHandle);
            doc.ImageDefinitions = new ImageDefinitions(doc, imageDefsHandle);
            doc.UnderlayDgnDefinitions = new UnderlayDgnDefinitions(doc, underlayDgnDefsHandle);
            doc.UnderlayDwfDefinitions = new UnderlayDwfDefinitions(doc, underlayDwfDefsHandle);
            doc.UnderlayPdfDefinitions = new UnderlayPdfDefinitions(doc, underlayPdfDefsHandle);
        }

        private DictionaryObject ReadDictionary()
        {
            string handle = null;
            //string handleOwner = null;
            DictionaryCloningFlags clonning = DictionaryCloningFlags.KeepExisting;
            bool isHardOwner = false;
            int numEntries = 0;
            List<string> names = new List<string>();
            List<string> handlesToOwner = new List<string>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 330:
                        //handleOwner = chunk.ReadHandle();
                        chunk.Next();
                        break;
                    case 280:
                        isHardOwner = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 281:
                        clonning = (DictionaryCloningFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 3:
                        numEntries += 1;
                        names.Add(DecodeEncodedNonAsciiCharacters(chunk.ReadString()));
                        chunk.Next();
                        break;
                    case 350: // Soft-owner ID/handle to entry object 
                        handlesToOwner.Add(chunk.ReadHex());
                        chunk.Next();
                        break;
                    case 360:
                        // Hard-owner ID/handle to entry object
                        handlesToOwner.Add(chunk.ReadHex());
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            //DxfObject owner = null;
            //if (handleOwner != null)
            //    owner = doc.AddedObjects[handleOwner];

            DictionaryObject dictionary = new DictionaryObject(null)
            {
                Handle = handle,
                IsHardOwner = isHardOwner,
                Cloning = clonning
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
            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        variables.Handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 70:
                        variables.DisplayFrame = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 71:
                        variables.DisplayQuality = (ImageDisplayQuality) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        variables.Units = (ImageUnits) chunk.ReadShort();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            return variables;
        }

        private ImageDefinition ReadImageDefinition()
        {
            string handle = null;
            string ownerHandle = null;
            string fileName = null;
            string name = null;
            double width = 0, height = 0;
            double wPixel = 0.0;
            double hPixel = 0.0;
            ImageResolutionUnits units = ImageResolutionUnits.Unitless;
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 1:
                        fileName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 10:
                        width = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        height = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        wPixel = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        hPixel = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 102:
                        if (chunk.ReadString().Equals("{ACAD_REACTORS", StringComparison.OrdinalIgnoreCase))
                        {
                            chunk.Next();
                            while (chunk.Code != 102)
                            {
                                // the first entry is the ownerHandle,
                                // the rest of the 330 values are not needed the IMAGEDEF_REACTOR already holds the handle to its corresponding IMAGEDEF
                                if (chunk.Code == 330 && string.IsNullOrEmpty(ownerHandle))
                                {
                                    ownerHandle = chunk.ReadHex();
                                }
                                chunk.Next();
                            }
                        }
                        chunk.Next();
                        break;
                    case 281:
                        units = (ImageResolutionUnits) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 330:
                        ownerHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            if (ownerHandle != null)
            {
                DictionaryObject imageDefDict = dictionaries[ownerHandle];
                if (handle == null)
                    throw new NullReferenceException();
                name = imageDefDict.Entries[handle];
            }

            // The documentation says that this is the size of one pixel in AutoCAD units, but it seems that this is always the size of one pixel in millimeters
            // this value is used to calculate the image resolution in PPI or PPC, and the default image size.
            // The documentation in this regard and its relation with the final image size in drawing units is a complete nonsense
            double factor = UnitHelper.ConversionFactor((ImageUnits) units, DrawingUnits.Millimeters);
            ImageDefinition imageDefinition = new ImageDefinition(name, fileName, (int) width, factor/wPixel, (int) height, factor/hPixel, units)
            {
                Handle = handle
            };
            imageDefinition.XData.AddRange(xData);

            if(string.IsNullOrEmpty(imageDefinition.Handle))
                throw new NullReferenceException("Underlay reference definition handle.");

            imgDefHandles.Add(imageDefinition.Handle, imageDefinition);
            return imageDefinition;
        }

        private ImageDefinitionReactor ReadImageDefReactor()
        {
            string handle = null;
            string imgOwner = null;

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 330:
                        imgOwner = chunk.ReadHex();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
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
            string name = null;
            string description = null;
            AciColor fillColor = AciColor.ByLayer;
            double startAngle = 90.0;
            double endAngle = 90.0;
            MLineStyleFlags flags = MLineStyleFlags.None;
            List<MLineStyleElement> elements = new List<MLineStyleElement>();
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 2:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 3:
                        description = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 62:
                        if (!fillColor.UseTrueColor)
                            fillColor = AciColor.FromCadIndex(chunk.ReadShort());
                        chunk.Next();
                        break;
                    case 420:
                        fillColor = AciColor.FromTrueColor(chunk.ReadInt());
                        chunk.Next();
                        break;
                    case 70:
                        flags = (MLineStyleFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 51:
                        startAngle = chunk.ReadDouble();
                        if (startAngle < 10.0 || startAngle > 170.0)
                            startAngle = 90.0;
                        chunk.Next();
                        break;
                    case 52:
                        endAngle = chunk.ReadDouble();
                        if (endAngle < 10.0 || endAngle > 170.0)
                            endAngle = 90.0;
                        chunk.Next();
                        break;
                    case 71:
                        short numElements = chunk.ReadShort();                       
                        elements = ReadMLineStyleElements(numElements);
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }
            if (string.IsNullOrEmpty(name))
                return null;

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
            chunk.Next();

            if (numElements <= 0)
                return new List<MLineStyleElement> {new MLineStyleElement(0.0)};
            
            List<MLineStyleElement> elements = new List<MLineStyleElement>();

            for (short i = 0; i < numElements; i++)
            {
                double offset = chunk.ReadDouble(); // code 49
                chunk.Next();

                AciColor color = AciColor.FromCadIndex(chunk.ReadShort());
                chunk.Next();

                if (chunk.Code == 420)
                {
                    color = AciColor.FromTrueColor(chunk.ReadInt()); // code 420
                    chunk.Next();
                }

                string linetypeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString()); // code 6
                Linetype linetype = GetLinetype(linetypeName);
                chunk.Next();

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
            string description = null;
            string name = null;
            bool isUnnamed = true;
            bool isSelectable = true;
            List<string> entities = new List<string>();
            List<XData> xData = new List<XData>();

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 330:
                        string handleOwner = chunk.ReadHex();
                        DictionaryObject dict = dictionaries[handleOwner];
                        if (handle == null)
                            throw new NullReferenceException("Null handle in Group dictionary.");
                        name = dict.Entries[handle];
                        chunk.Next();
                        break;
                    case 300:
                        description = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 70:
                        isUnnamed = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 71:
                        isSelectable = chunk.ReadShort() != 0;
                        chunk.Next();
                        break;
                    case 340:
                        string entity = chunk.ReadHex();
                        entities.Add(entity);
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            // we need to keep track of the group names generated
            if (isUnnamed)
                CheckGroupName(name);

            Group group = new Group(name, false)
            {
                Handle = handle,
                Description = description,
                IsUnnamed = isUnnamed,
                IsSelectable = isSelectable
            };
            group.XData.AddRange(xData);

            // the group entities will be processed later
            groupEntities.Add(group, entities);

            return group;
        }

        private Layout ReadLayout()
        {
            PlotSettings plot = new PlotSettings();
            string handle = null;
            string name = null;
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

            string dxfCode = chunk.ReadString();
            chunk.Next();

            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 0:
                        throw new Exception(string.Format("Premature end of object {0} definition.", dxfCode));
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 102:
                        ReadExtensionDictionaryGroup();
                        chunk.Next();
                        break;
                    case 330:
                        //string owner = chunk.ReadHandle();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 100:
                        if (chunk.ReadString() == SubclassMarker.PlotSettings)
                            plot = ReadPlotSettings();
                        chunk.Next();
                        break;
                    case 1:
                        name = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 71:
                        tabOrder = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 10:
                        minLimit.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 20:
                        minLimit.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 11:
                        maxLimit.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 21:
                        maxLimit.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 12:
                        basePoint.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 22:
                        basePoint.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 32:
                        basePoint.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 14:
                        minExtents.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 24:
                        minExtents.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 34:
                        minExtents.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 15:
                        maxExtents.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 25:
                        maxExtents.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 35:
                        maxExtents.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 146:
                        elevation = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 13:
                        ucsOrigin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 23:
                        ucsOrigin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 33:
                        ucsOrigin.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 16:
                        ucsXAxis.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 26:
                        ucsXAxis.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 36:
                        ucsXAxis.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 17:
                        ucsYAxis.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 27:
                        ucsYAxis.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 37:
                        ucsYAxis.Z = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 330:
                        ownerRecordHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 1001:
                        string appId = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        XData data = ReadXDataRecord(new ApplicationRegistry(appId));
                        xData.Add(data);
                        break;
                    default:
                        if (chunk.Code >= 1000 && chunk.Code <= 1071)
                            throw new Exception("The extended data of an entity must start with the application registry code.");
                        chunk.Next();
                        break;
                }
            }

            BlockRecord ownerRecord;
            if (handle != null)
            {
                if (blockRecordPointerToLayout.TryGetValue(handle, out ownerRecord))
                    blockRecordPointerToLayout.Remove(handle);
                else
                    ownerRecord = doc.GetObjectByHandle(ownerRecordHandle) as BlockRecord;
            }
            else
                ownerRecord = doc.GetObjectByHandle(ownerRecordHandle) as BlockRecord;

            if (ownerRecord != null)
                if (doc.Blocks.GetReferences(ownerRecord.Name).Count > 0)
                    ownerRecord = null; // the block is already in use

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
                TabOrder = tabOrder > 0 ? tabOrder : (short) (doc.Layouts.Count + 1),
                AssociatedBlock = ownerRecord == null ? null : doc.Blocks[ownerRecord.Name]
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

            chunk.Next();
            while (chunk.Code != 100)
            {
                switch (chunk.Code)
                {
                    case 1:
                        pageName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 2:
                        plotterName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 4:
                        paperSizeName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 6:
                        viewName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 7:
                        styleSheet = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 40:
                        leftMargin = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 41:
                        bottomMargin = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 42:
                        rightMargin = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 43:
                        topMargin = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 44:
                        paperSize.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 45:
                        paperSize.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 46:
                        origin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 47:
                        origin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 48:
                        windowBottomLeft.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 49:
                        windowUpRight.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 140:
                        windowBottomLeft.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 141:
                        windowUpRight.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 142:
                        scaleNumerator = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 143:
                        scaleDenominator = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 70:
                        flags = (PlotFlags) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 72:
                        paperUnits = (PlotPaperUnits) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 73:
                        paperRotation = (PlotRotation) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 74:
                        plotType = (PlotType) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 75:
                        short plotScale = chunk.ReadShort();
                        scaleToFit = plotScale == 0;
                        chunk.Next();
                        break;
                    case 76:
                        shadePlotMode = (ShadePlotMode) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 77:
                        shadePlotResolutionMode = (ShadePlotResolutionMode) chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 78:
                        shadePlotDPI = chunk.ReadShort();
                        chunk.Next();
                        break;
                    case 148:
                        paperImageOrigin.X = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    case 149:
                        paperImageOrigin.Y = chunk.ReadDouble();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
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
            string page = string.Empty;
            string ownerHandle = null;
            string fileName = null;
            string name = null;

            chunk.Next();
            while (chunk.Code != 0)
            {
                switch (chunk.Code)
                {
                    case 5:
                        handle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    case 1:
                        fileName = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 2:
                        page = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        chunk.Next();
                        break;
                    case 330:
                        ownerHandle = chunk.ReadHex();
                        chunk.Next();
                        break;
                    default:
                        chunk.Next();
                        break;
                }
            }

            if (ownerHandle != null)
            {
                DictionaryObject underlayDefDict = dictionaries[ownerHandle];
                if (handle == null)
                    throw new NullReferenceException("Null handle in underlay definition dictionary.");
                name = underlayDefDict.Entries[handle];
            }

            UnderlayDefinition underlayDef = null;
            switch (type)
            {
                case UnderlayType.DGN:
                    underlayDef = new UnderlayDgnDefinition(name, fileName)
                    {
                        Handle = handle,
                        Layout = page
                    };
                    break;
                case UnderlayType.DWF:
                    underlayDef = new UnderlayDwfDefinition(name, fileName)
                    {
                        Handle = handle
                    };
                    break;
                case UnderlayType.PDF:
                    underlayDef = new UnderlayPdfDefinition(name, fileName)
                    {
                        Handle = handle,
                        Page = page
                    };
                    break;
            }

            if (underlayDef == null)
                throw new NullReferenceException("Underlay reference definition.");
            if(string.IsNullOrEmpty(underlayDef.Handle))
                throw new NullReferenceException("Underlay reference definition handle.");
            underlayDefHandles.Add(underlayDef.Handle, underlayDef);
            return underlayDef;
        }

        #endregion

        #region private methods

        private void PostProcesses()
        {           
            // post process the dimension style list to assign the variables DIMTXSTY, DIMBLK, DIMBLK1, DIMBLK2, DIMLTYPE, DILTEX1, and DIMLTEX2
            foreach (KeyValuePair<DimensionStyle, string[]> pair in dimStyleToHandles)
            {
                DimensionStyle defaultDim = DimensionStyle.Default;

                BlockRecord record;

                record = doc.GetObjectByHandle(pair.Value[0]) as BlockRecord;
                pair.Key.DimArrow1 = record == null ? null : doc.Blocks[record.Name];

                record = doc.GetObjectByHandle(pair.Value[1]) as BlockRecord;
                pair.Key.DimArrow2 = record == null ? null : doc.Blocks[record.Name];

                record = doc.GetObjectByHandle(pair.Value[2]) as BlockRecord;
                pair.Key.LeaderArrow = record == null ? null : doc.Blocks[record.Name];

                TextStyle txtStyle;

                txtStyle = doc.GetObjectByHandle(pair.Value[3]) as TextStyle;
                pair.Key.TextStyle = txtStyle == null ? doc.TextStyles[defaultDim.TextStyle.Name] : doc.TextStyles[txtStyle.Name];

                Linetype ltype;

                ltype = doc.GetObjectByHandle(pair.Value[4]) as Linetype;
                pair.Key.DimLineLinetype = ltype == null ? doc.Linetypes[defaultDim.DimLineLinetype.Name] : doc.Linetypes[ltype.Name];

                ltype = doc.GetObjectByHandle(pair.Value[5]) as Linetype;
                pair.Key.ExtLine1Linetype = ltype == null ? doc.Linetypes[defaultDim.ExtLine1Linetype.Name] : doc.Linetypes[ltype.Name];

                ltype = doc.GetObjectByHandle(pair.Value[6]) as Linetype;
                pair.Key.ExtLine2Linetype = ltype == null ? doc.Linetypes[defaultDim.ExtLine2Linetype.Name] : doc.Linetypes[ltype.Name];
            }

            // post process the image list to assign their image definitions.
            foreach (KeyValuePair<Image, string> pair in imgToImgDefHandles)
            {
                Image image = pair.Key;
                image.Definition = imgDefHandles[pair.Value];
                image.Definition.Reactors.Add(image.Handle, imageDefReactors[image.Handle]);

                // we still need to set the definitive image size, now that we know all units involved
                double factor = UnitHelper.ConversionFactor(doc.DrawingVariables.InsUnits, doc.RasterVariables.Units);
                image.Width *= factor;
                image.Height *= factor;
            }

            // post process the underlay definition list to assign their image definitions.
            foreach (KeyValuePair<Underlay, string> pair in underlayToDefinitionHandles)
            {
                Underlay underlay = pair.Key;
                underlay.Definition = underlayDefHandles[pair.Value];
            }

            // post process the MLines to assign their MLineStyle
            foreach (KeyValuePair<MLine, string> pair in mLineToStyleNames)
            {
                MLine mline = pair.Key;
                mline.Style = GetMLineStyle(pair.Value);
            }

            // post process the entities of the blocks
            foreach (KeyValuePair<Block, List<EntityObject>> pair in blockEntities)
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
            foreach (KeyValuePair<DxfObject, string> pair in entityList)
            {
                Layout layout;
                Block block;
                if (pair.Value == null)
                {
                    // the Model layout is the default in case the entity has not one defined
                    layout = doc.Layouts[Layout.ModelSpaceName];
                    block = layout.AssociatedBlock;
                }
                else
                {
                    block = GetBlock(((BlockRecord) doc.GetObjectByHandle(pair.Value)).Name);
                    layout = block.Record.Layout;
                }

                // the viewport with id 1 is stored directly in the layout since it has no graphical representation
                Viewport viewport = pair.Key as Viewport;
                if (viewport != null)
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
                        doc.Blocks[layout.AssociatedBlock.Name].Entities.Add(viewport);
                    }
                }
                else
                {
                    // apply the units scale to the insertion scale (this is for not nested blocks)
                    Insert insert = pair.Key as Insert;
                    if (insert != null)
                    {
                        double scale = UnitHelper.ConversionFactor(doc.DrawingVariables.InsUnits, insert.Block.Record.Units);
                        insert.Scale *= scale;
                    }

                    AttributeDefinition attDef = pair.Key as AttributeDefinition;
                    if (attDef != null)
                    {
                        if(!layout.AssociatedBlock.AttributeDefinitions.ContainsTag(attDef.Tag))
                            layout.AssociatedBlock.AttributeDefinitions.Add(attDef);
                    }

                    EntityObject entity = pair.Key as EntityObject;
                    if (entity != null)
                        layout.AssociatedBlock.Entities.Add(entity);
                }
            }

            // assign a handle to the default layout viewports in case there was no layout viewport with ID=1 in the DXF
            foreach (Layout layout in doc.Layouts)
            {
                if (layout.Viewport == null)
                    continue;

                if (string.IsNullOrEmpty(layout.Viewport.Handle))
                    doc.NumHandles = layout.Viewport.AsignHandle(doc.NumHandles);
            }

            // post process viewports clipping boundaries
            foreach (KeyValuePair<Viewport, string> pair in viewports)
            {
                EntityObject entity = doc.GetObjectByHandle(pair.Value) as EntityObject;
                if (entity != null)
                    pair.Key.ClippingBoundary = entity;
            }

            // post process the hatch boundary paths
            foreach (KeyValuePair<Hatch, List<HatchBoundaryPath>> pair in hatchToPaths)
            {
                Hatch hatch = pair.Key;
                foreach (HatchBoundaryPath path in pair.Value)
                {
                    List<string> entities = hatchContourns[path];
                    foreach (string handle in entities)
                    {
                        EntityObject entity = doc.GetObjectByHandle(handle) as EntityObject;
                        if (entity != null)
                            if (ReferenceEquals(hatch.Owner, entity.Owner))
                                path.AddContour(entity);
                    }
                    hatch.BoundaryPaths.Add(path);
                }
            }

            // post process group entities
            foreach (KeyValuePair<Group, List<string>> pair in groupEntities)
            {
                foreach (string handle in pair.Value)
                {
                    EntityObject entity = doc.GetObjectByHandle(handle) as EntityObject;
                    if (entity != null)
                        pair.Key.Entities.Add(entity);
                }
            }

            // post process dimension style overrides,
            // it is stored in the dimension XData and the information stored there might contain handles to Linetypes, TextStyles and/or Blocks,
            // therefore is better process it at the end, when everything has been created read dimension style overrides
            foreach (Dimension dim in doc.Dimensions)
            {
                XData xDataOverrides;
                if (dim.XData.TryGetValue(ApplicationRegistry.DefaultName, out xDataOverrides))
                    dim.StyleOverrides.AddRange(ReadDimensionStyleOverrideXData(xDataOverrides));
            }
            foreach (Leader leader in doc.Leaders)
            {
                XData xDataOverrides;
                if (leader.XData.TryGetValue(ApplicationRegistry.DefaultName, out xDataOverrides))
                    leader.StyleOverrides.AddRange(ReadDimensionStyleOverrideXData(xDataOverrides));
            }

            // post process leader annotations
            foreach (KeyValuePair<Leader, string> pair in leaderAnnotation)
            {
                EntityObject entity = doc.GetObjectByHandle(pair.Value) as EntityObject;
                if (entity != null)
                {
                    pair.Key.Annotation = entity;
                    pair.Key.Update(true);
                }
            }
        }

        private void ReadExtensionDictionaryGroup()
        {
            //string dictionaryGroup = chunk.ReadString().Remove(0, 1);
            chunk.Next();

            while (chunk.Code != 102)
            {
                switch (chunk.Code)
                {
                    case 330:
                        break;
                    case 360:
                        break;
                }
                chunk.Next();
            }
        }

        private XData ReadXDataRecord(ApplicationRegistry appReg)
        {
            XData xData = new XData(appReg);
            chunk.Next();

            while (chunk.Code >= 1000 && chunk.Code <= 1071)
            {
                if (chunk.Code == (short) XDataCode.AppReg)
                    break;

                XDataCode code = (XDataCode) chunk.Code;
                object value = null;
                switch (code)
                {
                    case XDataCode.String:
                        value = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        break;
                    case XDataCode.AppReg:
                        // Application name cannot appear inside the extended data, AutoCAD assumes it is the beginning of a new application extended data group
                        break;
                    case XDataCode.ControlString:
                        value = chunk.ReadString();
                        break;
                    case XDataCode.LayerName:
                        value = DecodeEncodedNonAsciiCharacters(chunk.ReadString());
                        break;
                    case XDataCode.BinaryData:
                        value = chunk.ReadBytes();
                        break;
                    case XDataCode.DatabaseHandle:
                        value = chunk.ReadString();
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
                        value = chunk.ReadDouble();
                        break;
                    case XDataCode.Int16:
                        value = chunk.ReadShort();
                        break;
                    case XDataCode.Int32:
                        value = chunk.ReadInt();
                        break;
                }

                XDataRecord xDataRecord = new XDataRecord(code, value);
                xData.XDataRecord.Add(xDataRecord);
                chunk.Next();
            }

            return xData;
        }

        private string DecodeEncodedNonAsciiCharacters(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string decoded;
            if (decodedStrings.TryGetValue(text, out decoded))
                return decoded;

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
                            int value;
                            string hex = text.Substring(i + 3, 4);
                            if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
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

            decodedStrings.Add(text, decoded);
            return decoded;
        }

        private void CheckDimBlockName(string name)
        {
            // the AutoCad block names has the form *D#
            // we need to find which is the last available number, in case more dimensions are added
            if (!name.StartsWith("*D", StringComparison.OrdinalIgnoreCase))
                return;

            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num))
                return;
            if (num > doc.DimensionBlocksIndex)
                doc.DimensionBlocksIndex = num;
        }

        private void CheckGroupName(string name)
        {
            // the AutoCad group names has the form *A#
            // we need to find which is the last available number, in case more groups are added
            if (!name.StartsWith("*A", StringComparison.OrdinalIgnoreCase))
                return;
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num))
                return;
            if (num > doc.GroupNamesIndex)
                doc.GroupNamesIndex = num;
        }

        private static TextAlignment ObtainAlignment(short horizontal, short vertical)
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
            ApplicationRegistry appReg;
            if (doc.ApplicationRegistries.TryGetValue(name, out appReg))
                return appReg;

            // if an entity references a table object not defined in the tables section a new one will be created
            return doc.ApplicationRegistries.Add(new ApplicationRegistry(name));
        }

        private Block GetBlock(string name)
        {
            Block block;
            if (doc.Blocks.TryGetValue(name, out block))
                return block;
            throw new ArgumentException("The block with name " + name + " does not exist.");
        }

        private Layer GetLayer(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
                name = Layer.DefaultName;

            Layer layer;
            if (doc.Layers.TryGetValue(name, out layer))
                return layer;

            // if an entity references a table object not defined in the tables section a new one will be created
            return doc.Layers.Add(new Layer(name));
        }

        private Linetype GetLinetype(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
                name = Linetype.DefaultName;

            Linetype linetype;
            if (doc.Linetypes.TryGetValue(name, out linetype))
                return linetype;

            // if an entity references a table object not defined in the tables section a new one will be created
            return doc.Linetypes.Add(new Linetype(name));
        }

        private TextStyle GetTextStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
                name = TextStyle.DefaultName;

            TextStyle style;
            if (doc.TextStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            return doc.TextStyles.Add(new TextStyle(name));
        }

        private DimensionStyle GetDimensionStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
                name = DimensionStyle.DefaultName;

            DimensionStyle style;
            if (doc.DimensionStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            return doc.DimensionStyles.Add(new DimensionStyle(name));
        }

        private MLineStyle GetMLineStyle(string name)
        {
            // invalid names or that are defined in external drawings will be given the default
            if (!TableObject.IsValidName(name))
                name = MLineStyle.DefaultName;

            MLineStyle style;
            if (doc.MlineStyles.TryGetValue(name, out style))
                return style;

            return doc.MlineStyles.Add(new MLineStyle(name));
        }

        #endregion
    }
}