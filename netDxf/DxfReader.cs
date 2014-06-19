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
        private DxfDocument doc;

        // here we will store strings already decoded <string: original, string: decoded>
        private Dictionary<string, string> decodedStrings;

        // blocks records
        private Dictionary<string, BlockRecord> blockRecords;

        // entities, they will be processed at the end <EntityObject: entity, string: owner handle>.
        private Dictionary<EntityObject, string> entityList;

        // viewports, they will be processed at the end <EntityObject: viewport, string: clipping boundary handle>.
        private Dictionary<Viewport, string> viewports;

        // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the block that references them
        // temporarily this variables will store information to post process the nested block list
        private Dictionary<Insert, string> nestedInserts;
        private Dictionary<Dimension, string> nestedDimensions;
        //private Dictionary<Attribute, string> nestedBlocksAttributes;

        // the named dictionary is the only one we are interested, it is always the first that appears in the section
        // It consists solely of associated pairs of entry names and hard ownership pointer references to the associated object.
        private DictionaryObject namedDictionary;
        // <string: dictionary handle, DictionaryObject: dictionary>
        private Dictionary<string, DictionaryObject> dictionaries;

        private Dictionary<string, ImageDefReactor> imageDefReactors;

        // variables for post-processing
        private Dictionary<Group, List<string>> groupEntities;

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

        #region public methods

        /// <summary>
        /// Reads the whole stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        public DxfDocument Read(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "The stream cannot be null");

            try
            {
                Encoding encodingType = EncodingType.GetType(stream);
                bool isUnicode = (encodingType.EncodingName == Encoding.UTF8.EncodingName) ||
                                 (encodingType.EncodingName == Encoding.BigEndianUnicode.EncodingName) ||
                                 (encodingType.EncodingName == Encoding.Unicode.EncodingName);

                if (isUnicode)
                    this.reader = new StreamReader(stream, true);
                else
                {
                    // if the file is no utf-8 use the codepage provided by the dxf file
                    string dwgcodepage = CheckHeaderVariable(stream, HeaderVariableCode.DwgCodePage);
                    if (string.IsNullOrEmpty(dwgcodepage))
                        throw (new DxfException("Unknown codepage for non unicode file."));
                    int codepage;
                    if (!int.TryParse(dwgcodepage.Split('_')[1], out codepage))
                        throw (new DxfException("Unknown codepage for non unicode file."));
                    this.reader = new StreamReader(stream, Encoding.GetEncoding(codepage));
                }
            }
            catch (Exception ex)
            {
                throw (new DxfException("Unknow error opening the reader.", ex));
            }

            this.doc = new DxfDocument(new HeaderVariables(), false);

            this.entityList = new Dictionary<EntityObject, string>();
            this.viewports = new Dictionary<Viewport, string>();

            this.decodedStrings = new Dictionary<string, string>();

            // blocks
            this.nestedInserts = new Dictionary<Insert, string>();
            this.nestedDimensions = new Dictionary<Dimension, string>();
            //this.nestedBlocksAttributes = new Dictionary<Attribute, string>();
            this.blockRecords = new Dictionary<string, BlockRecord>(StringComparer.OrdinalIgnoreCase);

            // objects
            this.dictionaries = new Dictionary<string, DictionaryObject>(StringComparer.OrdinalIgnoreCase);
            this.groupEntities = new Dictionary<Group, List<string>>();
            this.imageDefReactors = new Dictionary<string, ImageDefReactor>(StringComparer.OrdinalIgnoreCase);
            this.imgDefHandles = new Dictionary<string, ImageDef>(StringComparer.OrdinalIgnoreCase);
            this.imgToImgDefHandles = new Dictionary<Image, string>();
            this.mLineToStyleNames = new Dictionary<MLine, string>();

            this.dxfPairInfo = this.ReadCodePair();

            // read the comments at the head of the file, any other comments will be ignored
            // they sometimes hold information about the program that has generated the dxf
            while (this.dxfPairInfo.Code == 999)
            {
                this.doc.Comments.Add(this.dxfPairInfo.Value);
                this.dxfPairInfo = this.ReadCodePair();
            }

            while (this.dxfPairInfo.Value != StringCode.EndOfFile)
            {
                if (this.dxfPairInfo.Value == StringCode.BeginSection)
                {
                    this.dxfPairInfo = this.ReadCodePair();
                    switch (this.dxfPairInfo.Value)
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
                        case StringCode.ThumbnailImageSection:
                            this.ReadThumbnailImage();
                            break;
                        case StringCode.AcdsDataSection:
                            this.ReadAcdsData();
                            break;
                        default:
                            throw new InvalidDxfSectionException(this.dxfPairInfo.Value, "Unknown section " + this.dxfPairInfo.Value + ".");
                    }
                }
                this.dxfPairInfo = this.ReadCodePair();
            }
            stream.Position = 0;

            // postprocess the image list to assign their image definitions.
            foreach (KeyValuePair<Image, string> pair in this.imgToImgDefHandles)
            {
                Image image = pair.Key;
                image.Definition = this.imgDefHandles[pair.Value];
                image.Definition.Reactors.Add(image.Handle, this.imageDefReactors[image.Handle]);

                // we still need to set the definitive image size, now that we know all units involved
                double factor = MathHelper.ConversionFactor(this.doc.DrawingVariables.InsUnits, this.doc.RasterVariables.Units);
                image.Width *= factor;
                image.Height *= factor;
            }

            // postprocess the MLines to assign their MLineStyle
            foreach (KeyValuePair<MLine, string> pair in this.mLineToStyleNames)
            {
                MLine mline = pair.Key;
                mline.Style = this.GetMLineStyle(pair.Value);
            }

            foreach (KeyValuePair<EntityObject, string> pair in this.entityList)
            {
                // this is the default layout in case the entity has not defined layout
                Layout layout;
                Block block;
                if (pair.Value == null)
                {
                    layout = Layout.ModelSpace;
                    block = layout.AssociatedBlock;
                }
                else
                {
                    block = this.GetBlock(((BlockRecord) this.doc.GetObjectByHandle(pair.Value)).Name);
                    layout = block.Record.Layout;
                }

                // the viewport with id 1 is stored directly in the layout since it has no graphical representation
                if (pair.Key is Viewport)
                {
                    Viewport viewport = (Viewport) pair.Key;
                    if (viewport.Id == 1)
                    {
                        // the base layout viewport has always id = 1 and we will not add it to the entities list of the document.
                        // this viewport has no graphical representation, it is the view of the paper space layout itself and it does not show the model.
                        layout.Viewport = viewport;
                        layout.Viewport.Owner = block;
                    }
                    else
                    {
                        this.doc.ActiveLayout = layout.Name;
                        this.doc.AddEntity(pair.Key, false, false);
                    }
                }
                else
                {
                    this.doc.ActiveLayout = layout.Name;
                    this.doc.AddEntity(pair.Key, false, false);

                    // apply the units scale to the insertion scale (this is for not nested blocks)
                    if (pair.Key is Insert)
                    {
                        Insert insert = (Insert) pair.Key;
                        double scale = MathHelper.ConversionFactor(this.doc.DrawingVariables.InsUnits, insert.Block.Record.Units);
                        insert.Scale *= scale;
                    }
                }
            }

            // assign a handle to the default layout viewports
            foreach (Layout layout in this.doc.Layouts)
            {
                if (layout.Viewport != null)
                    if (string.IsNullOrEmpty(layout.Viewport.Handle))
                        this.doc.NumHandles = layout.Viewport.AsignHandle(this.doc.NumHandles);
            }

            // post process viewports clipping boundaries
            foreach (KeyValuePair<Viewport, string> pair in this.viewports)
            {
                DxfObject entity = this.doc.GetObjectByHandle(pair.Value);
                if (entity is EntityObject)
                    pair.Key.ClippingBoundary = (EntityObject) entity;
            }

            // post process group entities
            foreach (KeyValuePair<Group, List<string>> pair in this.groupEntities)
            {
                foreach (string handle in pair.Value)
                {
                    DxfObject entity = this.doc.GetObjectByHandle(handle);
                    if (entity is EntityObject)
                        pair.Key.Entities.Add((EntityObject) entity);
                }
            }

            this.doc.ActiveLayout = Layout.ModelSpace.Name;

            return this.doc;
        }

        public static string CheckHeaderVariable(Stream stream, string headerVariable)
        {
            StreamReader reader = new StreamReader(stream, true);
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
                                stream.Position = 0;
                                return dxfPairInfo.Value;
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
            Debug.Assert(this.dxfPairInfo.Value == StringCode.HeaderSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                if (HeaderVariable.Allowed.ContainsKey(this.dxfPairInfo.Value))
                {
                    int codeGroup = HeaderVariable.Allowed[this.dxfPairInfo.Value];
                    string variableName = this.dxfPairInfo.Value;
                    this.dxfPairInfo = this.ReadCodePair();
                    string value = this.dxfPairInfo.Value;
                    if (this.dxfPairInfo.Code != codeGroup)
                        throw new DxfHeaderVariableException(variableName, "Invalid variable name and code group convination.");
                    switch (variableName)
                    {
                        case HeaderVariableCode.AcadVer:
                            DxfVersion acadVer = DxfVersion.Unknown;
                            if (StringEnum.IsStringDefined(typeof (DxfVersion), value))
                                acadVer = (DxfVersion) StringEnum.Parse(typeof (DxfVersion), this.dxfPairInfo.Value);
                            if (acadVer < DxfVersion.AutoCad2000)
                                throw new NotSupportedException("Only AutoCad2000 and higher dxf versions are supported.");
                            this.doc.DrawingVariables.AcadVer = acadVer;
                            break;
                        case HeaderVariableCode.HandleSeed:
                            this.doc.DrawingVariables.HandleSeed = value;
                            this.doc.NumHandles = long.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                            break;
                        case HeaderVariableCode.Angbase:
                            this.doc.DrawingVariables.Angbase = double.Parse(value);
                            break;
                        case HeaderVariableCode.Angdir:
                            this.doc.DrawingVariables.Angdir = int.Parse(value);
                            break;
                        case HeaderVariableCode.AttMode:
                            this.doc.DrawingVariables.AttMode = (AttMode) int.Parse(value);
                            break;
                        case HeaderVariableCode.AUnits:
                            this.doc.DrawingVariables.AUnits = (AngleUnitType) int.Parse(value);
                            break;
                        case HeaderVariableCode.AUprec:
                            this.doc.DrawingVariables.AUprec = int.Parse(value);
                            break;
                        case HeaderVariableCode.CeColor:
                            this.doc.DrawingVariables.CeColor = AciColor.FromCadIndex(short.Parse(value));
                            break;
                        case HeaderVariableCode.CeLtScale:
                            this.doc.DrawingVariables.CeLtScale = double.Parse(value);
                            break;
                        case HeaderVariableCode.CeLtype:
                            this.doc.DrawingVariables.CeLtype = this.DecodeEncodedNonAsciiCharacters(value);
                            break;
                        case HeaderVariableCode.CeLweight:
                            this.doc.DrawingVariables.CeLweight = Lineweight.FromCadIndex(short.Parse(value));
                            break;
                        case HeaderVariableCode.CLayer:
                            this.doc.DrawingVariables.CLayer = this.DecodeEncodedNonAsciiCharacters(value);
                            break;
                        case HeaderVariableCode.CMLJust:
                            this.doc.DrawingVariables.CMLJust = (MLineJustification) int.Parse(value);
                            break;
                        case HeaderVariableCode.CMLScale:
                            this.doc.DrawingVariables.CMLScale = double.Parse(value);
                            break;
                        case HeaderVariableCode.CMLStyle:
                            string mLineStyleName = this.DecodeEncodedNonAsciiCharacters(value);
                            if (!string.IsNullOrEmpty(mLineStyleName))
                                this.doc.DrawingVariables.CMLStyle = mLineStyleName;
                            break;
                        case HeaderVariableCode.DimStyle:
                            string dimStyleName = this.DecodeEncodedNonAsciiCharacters(value);
                            if (!string.IsNullOrEmpty(dimStyleName))
                                this.doc.DrawingVariables.DimStyle = dimStyleName;
                            break;
                        case HeaderVariableCode.TextSize:
                            double size = double.Parse(value);
                            if (size > 0.0)
                                this.doc.DrawingVariables.TextSize = size;
                            break;
                        case HeaderVariableCode.TextStyle:
                            string textStyleName = this.DecodeEncodedNonAsciiCharacters(value);
                            if (!string.IsNullOrEmpty(textStyleName))
                                this.doc.DrawingVariables.TextStyle = textStyleName;
                            break;
                        case HeaderVariableCode.LastSavedBy:
                            this.doc.DrawingVariables.LastSavedBy = this.DecodeEncodedNonAsciiCharacters(value);
                            break;
                        case HeaderVariableCode.LUnits:
                            this.doc.DrawingVariables.LUnits = (LinearUnitType) int.Parse(value);
                            break;
                        case HeaderVariableCode.LUprec:
                            this.doc.DrawingVariables.LUprec = int.Parse(value);
                            break;
                        case HeaderVariableCode.DwgCodePage:
                            this.doc.DrawingVariables.DwgCodePage = value;
                            break;
                        case HeaderVariableCode.Extnames:
                            this.doc.DrawingVariables.Extnames = (int.Parse(value) != 0);
                            break;
                        case HeaderVariableCode.InsUnits:
                            this.doc.DrawingVariables.InsUnits = (DrawingUnits) int.Parse(value);
                            break;
                        case HeaderVariableCode.LtScale:
                            this.doc.DrawingVariables.LtScale = double.Parse(value);
                            break;
                        case HeaderVariableCode.LwDisplay:
                            this.doc.DrawingVariables.LwDisplay = (int.Parse(value) != 0);
                            break;
                        case HeaderVariableCode.PdMode:
                            this.doc.DrawingVariables.PdMode = (PointShape) int.Parse(value);
                            break;
                        case HeaderVariableCode.PdSize:
                            this.doc.DrawingVariables.PdSize = double.Parse(value);
                            break;
                        case HeaderVariableCode.PLineGen:
                            this.doc.DrawingVariables.PLineGen = int.Parse(value);
                            break;
                        case HeaderVariableCode.TdCreate:
                            this.doc.DrawingVariables.TdCreate = DrawingTime.FromJulianCalendar(double.Parse(value));
                            break;
                        case HeaderVariableCode.TduCreate:
                            this.doc.DrawingVariables.TduCreate = DrawingTime.FromJulianCalendar(double.Parse(value));
                            break;
                        case HeaderVariableCode.TdUpdate:
                            this.doc.DrawingVariables.TdUpdate = DrawingTime.FromJulianCalendar(double.Parse(value));
                            break;
                        case HeaderVariableCode.TduUpdate:
                            this.doc.DrawingVariables.TduUpdate = DrawingTime.FromJulianCalendar(double.Parse(value));
                            break;
                        case HeaderVariableCode.TdinDwg:
                            this.doc.DrawingVariables.TdinDwg = DrawingTime.EditingTime(double.Parse(value));
                            break;
                    }
                }
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadClasses()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.ClassesSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadTables()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.TablesSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                this.ReadTable();
            }

            // check if all table collections has been created
            if (this.doc.ApplicationRegistries == null)
                this.doc.ApplicationRegistries = new ApplicationRegistries(this.doc);
            if (this.doc.Blocks == null)
                this.doc.Blocks = new BlockRecords(this.doc);
            if (this.doc.DimensionStyles == null)
                this.doc.DimensionStyles = new DimensionStyles(this.doc);
            if (this.doc.Layers == null)
                this.doc.Layers = new Layers(this.doc);
            if (this.doc.LineTypes == null)
                this.doc.LineTypes = new LineTypes(this.doc);
            if (this.doc.TextStyles == null)
                this.doc.TextStyles = new TextStyles(this.doc);
            if (this.doc.UCSs == null)
                this.doc.UCSs = new UCSs(this.doc);
            if (this.doc.Views == null)
                this.doc.Views = new Views(this.doc);
            if (this.doc.VPorts == null)
                this.doc.VPorts = new VPorts(this.doc);
        }

        private void ReadBlocks()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.BlocksSection);
            
            // the blocks list will be added to the document after reading the blocks section to handle possible nested insert cases.
            Dictionary<string, Block> blocks = new Dictionary<string, Block>(StringComparer.OrdinalIgnoreCase);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (this.dxfPairInfo.Value)
                {
                    case StringCode.BeginBlock:
                        Block block = this.ReadBlock();
                        blocks.Add(block.Name, block);
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // post process the possible nested blocks,
            // in nested blocks (blocks that contains Insert entities) the block definition might be defined AFTER the insert that references them
            foreach (KeyValuePair<Insert, string> pair in this.nestedInserts)
            {
                Insert insert = pair.Key;
                insert.Block = blocks[pair.Value];
                foreach (Attribute att in insert.Attributes.Values)
                {
                    //string tag = this.nestedBlocksAttributes[att];
                    // attribute definitions might be null if an INSERT entity attribute has not been defined in the block
                    AttributeDefinition attDef;
                    if(insert.Block.AttributeDefinitions.TryGetValue(att.Tag, out attDef))
                        att.Definition = attDef;
                    att.Owner = insert.Block;
                }
                // in the case the insert belongs to a *PaperSpace# the insert owner has not been asigned yet,
                // in this case the owner units are the document units and will be assigned at the end with the rest of the entities 
                if (insert.Owner != null)
                {
                    // apply the units scale to the insertion scale (this is for nested blocks)
                    double scale = MathHelper.ConversionFactor(insert.Owner.Record.Units, insert.Block.Record.Units);
                    insert.Scale *= scale;
                }

            }
            foreach (KeyValuePair<Dimension, string> pair in this.nestedDimensions)
            {
                Dimension dim = pair.Key;
                dim.Block = blocks[pair.Value];
            }

            // add the the blocks to the document
            foreach (Block block in blocks.Values)
            {
                this.doc.Blocks.Add(block, false);
            }
        }

        private void ReadEntities()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.EntitiesSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                this.ReadEntity(false);
            }
        }

        private void ReadObjects()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.ObjectsSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                switch (this.dxfPairInfo.Value)
                {
                    case DxfObjectCode.Dictionary:
                        DictionaryObject dictionary = this.ReadDictionary();
                        this.dictionaries.Add(dictionary.Handle, dictionary);

                        // the named dictionary always appears the first in the objects section
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
                        ImageDef imageDef = this.ReadImageDefinition();
                        this.doc.ImageDefinitions.Add(imageDef, false);
                        break;
                    case DxfObjectCode.ImageDefReactor:
                        ImageDefReactor reactor = this.ReadImageDefReactor();
                        if (this.imageDefReactors.ContainsKey(reactor.ImageHandle)) continue;
                        this.imageDefReactors.Add(reactor.ImageHandle, reactor);
                        break;
                    case DxfObjectCode.MLineStyle:
                        MLineStyle style = this.ReadMLineStyle();
                        this.doc.MlineStyles.Add(style, false);
                        break;
                    case DxfObjectCode.Group:
                        Group group = this.ReadGroup();
                        this.doc.Groups.Add(group, false);
                        break;
                    case DxfObjectCode.Layout:
                        Layout layout = this.ReadLayout();
                        this.doc.Layouts.Add(layout, false);
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // add the default objects in case they have not been found in the file
            // add ModelSpace layout
            if(!this.doc.Layouts.Contains(Layout.ModelSpace.Name))
                this.doc.Layouts.Add(Layout.ModelSpace);

            // raster variables
            if (this.doc.RasterVariables == null)
                this.doc.RasterVariables = new RasterVariables();
        }

        private void ReadThumbnailImage()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.ThumbnailImageSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        private void ReadAcdsData()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.AcdsDataSection);

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndSection)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        #endregion

        #region table methods

        private void CreateTableCollection(string name, string handle)
        {
            switch (name)
            {
                case StringCode.ApplicationIDTable:
                    this.doc.ApplicationRegistries = new ApplicationRegistries(this.doc, handle);
                    break;
                case StringCode.BlockRecordTable:
                    this.doc.Blocks = new BlockRecords(this.doc, handle);
                    return;
                case StringCode.DimensionStyleTable:
                    this.doc.DimensionStyles = new DimensionStyles(this.doc, handle);
                    break;
                case StringCode.LayerTable:
                    this.doc.Layers = new Layers(this.doc, handle);
                    break;
                case StringCode.LineTypeTable:
                    this.doc.LineTypes = new LineTypes(this.doc, handle);
                    break;
                case StringCode.TextStyleTable:
                    this.doc.TextStyles = new TextStyles(this.doc, handle);
                    break;
                case StringCode.UcsTable:
                    this.doc.UCSs = new UCSs(this.doc, handle);
                    break;
                case StringCode.ViewTable:
                    this.doc.Views = new Views(this.doc, handle);
                    break;
                case StringCode.VportTable:
                    this.doc.VPorts = new VPorts(this.doc, handle);
                    break;
            }
        }

        private void ReadTable()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.Table);

            string handle = null;
            this.dxfPairInfo = this.ReadCodePair();
            string tableName = this.dxfPairInfo.Value;
            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Value != StringCode.EndTable)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        string owner = this.dxfPairInfo.Value;
                        // owner should be always, 0 handle of the document.
                        Debug.Assert(owner == "0");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 0:
                        this.CreateTableCollection(tableName, handle);
                        this.ReadTableEntry(handle);
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            this.dxfPairInfo = this.ReadCodePair();
        }

        private void ReadTableEntry(string ownerHandle)
        {
            string dxfCode = this.dxfPairInfo.Value;
            string handle = null;

            while (this.dxfPairInfo.Value != StringCode.EndTable)
            {
                // table entry common codes
                while (this.dxfPairInfo.Code != 100)
                {
                    switch (this.dxfPairInfo.Code)
                    {
                        case 5:
                            handle = this.dxfPairInfo.Value;
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        case 105:
                            // this handle code is specific of dimension styles
                            handle = this.dxfPairInfo.Value;
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        case 330:
                            string owner = this.dxfPairInfo.Value;
                            // owner should be always, the handle of the list to which the entry belongs.
                            Debug.Assert(owner == ownerHandle);
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        case 102:
                            this.ReadExtensionDictionaryGroup();
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        default:
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                    }
                }

                this.dxfPairInfo = this.ReadCodePair();

                switch (dxfCode)
                {
                    case StringCode.ApplicationIDTable:
                        ApplicationRegistry appReg = this.ReadApplicationId();
                        if (appReg != null)
                        {
                            appReg.Handle = handle;
                            this.doc.ApplicationRegistries.Add(appReg, false);
                        }
                        break;
                    case StringCode.BlockRecordTable:
                        BlockRecord record = this.ReadBlockRecord();
                        if (record != null)
                        {
                            record.Handle = handle;
                            this.blockRecords.Add(record.Name, record);
                        }
                        break;
                    case StringCode.DimensionStyleTable:
                        DimensionStyle dimStyle = this.ReadDimensionStyle();
                        if (dimStyle != null)
                        {
                            dimStyle.Handle = handle;
                            this.doc.DimensionStyles.Add(dimStyle, false);
                        }
                        break;
                    case StringCode.LayerTable:
                        Layer layer = this.ReadLayer();
                        if (layer != null)
                        {
                            layer.Handle = handle;
                            this.doc.Layers.Add(layer, false);
                        }
                        break;
                    case StringCode.LineTypeTable:
                        LineType lineType = this.ReadLineType();
                        if (lineType != null)
                        {
                            lineType.Handle = handle;
                            this.doc.LineTypes.Add(lineType, false);
                        }
                        break;
                    case StringCode.TextStyleTable:
                        TextStyle style = this.ReadTextStyle();
                        if (style != null)
                        {
                            style.Handle = handle;
                            this.doc.TextStyles.Add(style, false);
                        }
                        break;
                    case StringCode.UcsTable:
                        UCS ucs = this.ReadUCS();
                        if (ucs != null)
                        {
                            ucs.Handle = handle;
                            this.doc.UCSs.Add(ucs, false);
                        }
                        break;
                    case StringCode.ViewTable:
                        this.ReadView();
                        //this.doc.Views.Add((View) entry);
                        break;
                    case StringCode.VportTable:
                        this.ReadVPort();
                        //this.doc.Vports.Add((VPort) entry);
                        break;
                    default:
                        this.ReadUnkownTableEntry();
                        return;
                }
            }
        }

        private ApplicationRegistry ReadApplicationId()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.ApplicationId);

            string appId = string.Empty;
            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        appId = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                }
                this.dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(appId)) return null;

            return new ApplicationRegistry(appId);
        }

        private BlockRecord ReadBlockRecord()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.BlockRecord);

            string name = null;
            DrawingUnits units = DrawingUnits.Unitless;
            bool allowExploding = true;
            bool scaleUniformly = false;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        units = (DrawingUnits) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        allowExploding = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        scaleUniformly = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // here is where dxf versions prior to AutoCad2007 stores the block units
            XData designCenterData;
            if (xData.TryGetValue(ApplicationRegistry.Default.Name, out designCenterData))
            {     
                foreach (XDataRecord record in designCenterData.XDataRecord)
                {
                    // the second 1070 code is the one that stores the block units,
                    // it will override the first 1070 that stores the Autodesk Design Center version number
                    if (record.Code == XDataCode.Integer)
                        units = (DrawingUnits) record.Value;
                }    
            }

            if (string.IsNullOrEmpty(name)) return null;

            // we need to check for generated blocks by dimensions, even if the dimension was deleted the block might persist in the drawing.
            this.CheckDimBlockName(name);

            return new BlockRecord(name)
                {
                    Units = units,
                    AllowExploding = allowExploding,
                    ScaleUniformly = scaleUniformly
                };
        }

        private DimensionStyle ReadDimensionStyle()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.DimensionStyle);

            DimensionStyle defaultDim = DimensionStyle.Default;
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

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case 3:
                        dimpost = this.dxfPairInfo.Value;
                        break;
                    case 41:
                        dimasz = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 42:
                        dimexo = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 44:
                        dimexe = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 73:
                        dimtih = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 74:
                        dimtoh = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 77:
                        dimtad = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 140:
                        dimtxt = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 141:
                        dimcen = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 147:
                        dimgap = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 149:
                        dimadec = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 271:
                        dimdec = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 275:
                        dimaunit = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 278:
                        dimdsep = this.dxfPairInfo.Value;
                        break;
                    case 280:
                        dimjust = int.Parse(this.dxfPairInfo.Value);
                        break;
                    case 340:
                        txtStyleHandle = this.dxfPairInfo.Value;
                        break;
                }
                this.dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(txtStyleHandle)) return null;

            return new DimensionStyle(name)
                {
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
                    TextStyle = this.GetTextStyleByHandle(txtStyleHandle)
                };
        }

        private Layer ReadLayer()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.Layer);

            string name = null;
            bool isVisible = true;
            bool plot = true;
            AciColor color = AciColor.Default;
            LineType lineType = null;
            Lineweight lineweight = Lineweight.Default;
            LayerFlags flags = LayerFlags.None;
            Transparency transparency = new Transparency(0);

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (LayerFlags) (int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        short index = short.Parse(this.dxfPairInfo.Value);
                        if (index < 0)
                        {
                            isVisible = false;
                            index = Math.Abs(index);
                        }
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(index);

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: // the layer uses true color
                        color = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = this.dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        else if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 290:
                        if (int.Parse(this.dxfPairInfo.Value) == 0)
                            plot = false;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 370:
                        lineweight = Lineweight.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                         // layer transparency is stored in xdata
                        // the ApplicationRegistries might be defined after the objects that requieres them,
                        // same old story this time we don't need to store the information in the object xdata since it is only supported by entities.
                        if (this.dxfPairInfo.Value == "AcCmTransparency")
                        {
                            this.dxfPairInfo = this.ReadCodePair();
                            transparency = Transparency.FromAlphaValue(int.Parse(this.dxfPairInfo.Value));
                        }
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            if (string.IsNullOrEmpty(name)) return null;

            return new Layer(name)
                {
                    Color = color,
                    LineType = lineType,
                    IsVisible = isVisible,
                    IsFrozen = (flags & LayerFlags.Frozen) == LayerFlags.Frozen,
                    IsLocked = (flags & LayerFlags.Locked) == LayerFlags.Locked,
                    Plot = plot,
                    Lineweight = lineweight,
                    Transparency = transparency
                };
        }

        private LineType ReadLineType()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.LineType);

            string name = null;
            string description = null;
            List<double> segments = new List<double>();

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2: // line type name is case insensitive
                        name = this.dxfPairInfo.Value;
                        if (String.Compare(name, LineType.ByLayer.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            name = LineType.ByLayer.Name;
                        else if (String.Compare(name, LineType.ByBlock.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            name = LineType.ByBlock.Name;
                        name = this.DecodeEncodedNonAsciiCharacters(name);
                        break;
                    case 3: // linetype description
                        description = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case 73:
                        //number of segments (not needed)
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        break;
                    case 49:
                        segments.Add(double.Parse(this.dxfPairInfo.Value));
                        break;
                }
                this.dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            return new LineType(name)
                {
                    Description = description,
                    Segments = segments,
                };
        }

        private TextStyle ReadTextStyle()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.TextStyle);

            string name = null;
            string font = null;
            bool isVertical = false;
            bool isBackward = false;
            bool isUpsideDown = false;
            double height = 0.0f;
            double widthFactor = 0.0f;
            double obliqueAngle = 0.0f;

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case 3:
                        font = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;

                    case 70:
                        if (int.Parse(this.dxfPairInfo.Value) == 4)
                            isVertical = true;
                        break;
                    case 71:
                        if (int.Parse(this.dxfPairInfo.Value) == 6)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        else if (int.Parse(this.dxfPairInfo.Value) == 2)
                            isBackward = true;
                        else if (int.Parse(this.dxfPairInfo.Value) == 4)
                            isUpsideDown = true;
                        break;
                    case 40:
                        height = double.Parse(this.dxfPairInfo.Value);
                        if (height < 0.0)
                            height = 0.0;
                        break;
                    case 41:
                        widthFactor = double.Parse(this.dxfPairInfo.Value);
                        if (widthFactor < 0.01 || widthFactor > 100.0)
                            widthFactor = 1.0;
                        break;
                    case 42:
                        //last text height used (not aplicable)
                        break;
                    case 50:
                        obliqueAngle = double.Parse(this.dxfPairInfo.Value);
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                            obliqueAngle = 0.0;
                        break;
                }
                this.dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;
            if (string.IsNullOrEmpty(font)) font = TextStyle.Default.FontName;

            return new TextStyle(name, font)
                {
                    Height = height,
                    IsBackward = isBackward,
                    IsUpsideDown = isUpsideDown,
                    IsVertical = isVertical,
                    ObliqueAngle = obliqueAngle,
                    WidthFactor = widthFactor,
                };
        }

        private UCS ReadUCS()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.Ucs);

            string name = null;
            Vector3 origin = Vector3.Zero;
            Vector3 xDir = Vector3.UnitX;
            Vector3 yDir = Vector3.UnitY;
            double elevation = 0.0;

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case 10:
                        origin.X = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 20:
                        origin.Y = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 30:
                        origin.Z = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 11:
                        xDir.X = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 21:
                        xDir.Y = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 31:
                        xDir.Z = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 12:
                        yDir.X = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 22:
                        yDir.Y = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 32:
                        yDir.Z = double.Parse(this.dxfPairInfo.Value);
                        break;
                    case 146:
                        elevation = double.Parse(this.dxfPairInfo.Value);
                        break;
                }

                this.dxfPairInfo = this.ReadCodePair();
            }

            if (string.IsNullOrEmpty(name)) return null;

            return new UCS(name, origin, xDir, yDir)
                {
                    Elevation = elevation
                };
        }

        private View ReadView()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.View);

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }

            return null;
        }

        private VPort ReadVPort()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.VPort);

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }

            return null;
        }

        private void ReadUnkownTableEntry()
        {
            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        #endregion

        #region block methods

        private Block ReadBlock()
        {
            Debug.Assert(this.dxfPairInfo.Value == StringCode.BeginBlock);

            BlockRecord blockRecord;
            Layer layer = null;
            string name = string.Empty;
            string handle = string.Empty;
            BlockTypeFlags type = BlockTypeFlags.None;
            Vector3 basePoint = Vector3.Zero;
            List<EntityObject> blockEntities = new List<EntityObject>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Value != StringCode.EndBlock)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        type = (BlockTypeFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        //I don't know the reason of these duplicity since code 2 also contains the block name
                        //The program EASE exports code 3 with an empty string (use it or don't use it but do NOT mix information)
                        //name = dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 0: // entity
                        EntityObject entity = this.ReadEntity(true);
                        if (entity != null) blockEntities.Add(entity);
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // read the end block object until a new element is found
            this.dxfPairInfo = this.ReadCodePair();
            string endBlockHandle = string.Empty;
            Layer endBlockLayer = layer;
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        endBlockHandle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endBlockLayer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            if (!this.blockRecords.TryGetValue(name, out blockRecord))
                throw new DxfTableException(StringCode.BlockRecordTable, "The block record " + name + " is not defined");

            BlockEnd end = new BlockEnd
                {
                    Handle = endBlockHandle,
                    Owner = blockRecord,
                    Layer = endBlockLayer
                };

            Block block = new Block(name, false)
                {
                    Handle = handle,
                    Owner = blockRecord,
                    Position = basePoint,
                    Layer = layer,
                    Flags = type,
                    End = end
                };


            if (name.StartsWith(Block.PaperSpace.Name, StringComparison.OrdinalIgnoreCase))
            {
                // the dxf is not consistent with the way they handle entities that belong to different paper spaces.
                // While the entities of *Paper_Space block are stored in the ENTITIES section as the *Model_Space,
                // the list of entities in *Paper_Space# are stored in the block definition itself.
                // As all this entities do not need an insert entity to have a visual representation,
                // they will be stored in the global entities lists together with the rest of the entities of *Model_Space and *Paper_Space
                foreach (EntityObject entity in blockEntities)
                    this.entityList.Add(entity, blockRecord.Handle);

                // this kind of blocks do not store attribute definitions
            }
            else
            {
                // add attribute definitions and entities
                foreach (EntityObject entity in blockEntities)
                {
                    if (entity.Type == EntityType.AttributeDefinition)
                        // autocad allows duplicate tags in attribute definitions, but this library does not
                        // having duplicate tags is not recommended in any way, since there will be now way to know which is the definition
                        // associated to the insert attribute
                        block.AttributeDefinitions.Add((AttributeDefinition) entity);
                    else
                        block.Entities.Add(entity);
                }
            }

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
            TextStyle style = this.GetTextStyle(TextStyle.Default.Name);
            double height = 0.0;
            double widthFactor = 0.0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        id = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        text = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        value = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (AttributeFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        widthFactor = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        verticalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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

            return attDef;
        }

        private Attribute ReadAttribute(Block block, bool isBlockEntity = false)
        {
            string handle = null;
            Layer layer = this.GetLayer(Layer.Default.Name);
            AciColor color = AciColor.ByLayer;
            LineType lineType = this.GetLineType(LineType.ByLayer.Name);
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;

            AttributeFlags flags = AttributeFlags.Visible;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            TextStyle style = this.GetTextStyle(TextStyle.Default.Name);
            double height = 0.0;
            double widthFactor = 0.0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            double rotation = 0.0;
            Vector3 normal = Vector3.UnitZ;

            // DxfObject codes
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(DxfObjectCode.Attribute, "Premature end of entity definition.");
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // AcDbEntity common codes
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(DxfObjectCode.Attribute, "Premature end of entity definition.");
                    case 8: //layer code
                        layer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: // the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = this.dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 370: //lineweight code
                        lineweight = Lineweight.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 48: //linetype scale
                        linetypeScale = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            string atttag = null;
            AttributeDefinition attdef = null;
            Object value = null;

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        atttag = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        // seems that some programs (sketchup AFAIK) might export insert entities with attributtes which definitions are not defined in the block
                        // if it is not present the insert attribute will have a null definition
                        if (!isBlockEntity)
                            block.AttributeDefinitions.TryGetValue(atttag, out attdef);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        value = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (AttributeFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        style = this.GetTextStyle(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        widthFactor = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        verticalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            return new Attribute
                {
                    Owner = block,
                    Handle = handle,
                    Color = color,
                    Layer = layer,
                    LineType = lineType,
                    Lineweight = lineweight,
                    LineTypeScale = linetypeScale,
                    Definition = attdef,
                    Tag = atttag,
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
        }

        #endregion

        #region entity methods

        private EntityObject ReadEntity(bool isBlockEntity)
        {
            string handle = null;
            string owner = null;
            Layer layer = this.GetLayer(Layer.Default.Name);
            AciColor color = AciColor.ByLayer;
            LineType lineType = this.GetLineType(LineType.ByLayer.Name);
            Lineweight lineweight = Lineweight.ByLayer;
            double linetypeScale = 1.0;
            bool isVisible = true;
            Transparency transparency = Transparency.ByLayer;

            EntityObject entity;

            string dxfCode = this.dxfPairInfo.Value;
            this.dxfPairInfo = this.ReadCodePair();

            // DxfObject common codes
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(dxfCode, "Premature end of entity definition.");
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        owner = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // AcDbEntity common codes
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(dxfCode, "Premature end of entity definition.");
                    case 8: //layer code
                        layer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 440: //transparency
                        transparency = Transparency.FromAlphaValue(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = this.dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, LineType.ByLayer.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = LineType.ByLayer.Name;
                        else if (String.Compare(lineTypeName, LineType.ByBlock.Name, StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = LineType.ByBlock.Name;
                        lineType = this.GetLineType(lineTypeName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 370: //lineweight code
                        lineweight = Lineweight.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 48: //linetype scale
                        linetypeScale = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 60: //object visibility
                        if (int.Parse(this.dxfPairInfo.Value) == 1) isVisible = false;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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
                case DxfObjectCode.Viewport:
                    entity = this.ReadViewport();
                    break;
                default:
                    this.ReadUnknowEntity();
                    return null;
            }

            if (entity == null) return null;

            entity.Handle = handle;
            entity.Layer = layer;
            entity.Color = color;
            entity.LineType = lineType;
            entity.Lineweight = lineweight;
            entity.LineTypeScale = linetypeScale;
            entity.IsVisible = isVisible;
            entity.Transparency = transparency;

            // the entities list will be processed at the end
            if (!isBlockEntity) this.entityList.Add(entity, owner);

            return entity;
        }

        private Viewport ReadViewport()
        {
            Debug.Assert(this.dxfPairInfo.Value == SubclassMarker.Viewport);

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

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        viewport.Width = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        viewport.Height = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 68:
                        viewport.Stacking = short.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 69:
                        viewport.Id = short.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        viewCenter.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        viewCenter.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        snapBase.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        snapBase.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        snapSpacing.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        snapSpacing.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        gridSpacing.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        gridSpacing.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 16:
                        viewDirection.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 26:
                        viewDirection.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 36:
                        viewDirection.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 17:
                        viewTarget.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 27:
                        viewTarget.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 37:
                        viewTarget.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        viewport.LensLength = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        viewport.FrontClipPlane = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        viewport.BackClipPlane = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 45:
                        viewport.ViewHeight = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        viewport.SnapAngle = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        viewport.TwistAngle = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        viewport.CircleZoomPercent = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 331:
                        Layer layer = (Layer) this.doc.GetObjectByHandle(this.dxfPairInfo.Value);
                        viewport.FrozenLayers.Add(layer);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 90:
                        viewport.Status = (ViewportStatusFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        // we will postprocess the clipping boundary in case it has been defined before the viewport
                        this.viewports.Add(viewport, this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 110:
                        ucsOrigin.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 120:
                        ucsOrigin.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 130:
                        ucsOrigin.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 111:
                        ucsXAxis.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 121:
                        ucsXAxis.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 131:
                        ucsXAxis.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 112:
                        ucsYAxis.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 122:
                        ucsYAxis.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 132:
                        ucsYAxis.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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
            viewport.XData = xData;

            return viewport;
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
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        posX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        posY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        posZ = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        uX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        uY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        uZ = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        vX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        vY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        vZ = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        width = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        height = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        imageDefHandle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        displayOptions = (ImageDisplayFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        clipping = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        brightness = float.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 282:
                        contrast = float.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 283:
                        fade = float.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        boundaryType = (ImageClippingBoundaryType) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        int numVertexes = int.Parse(this.dxfPairInfo.Value);
                        List<Vector2> vertexes = new List<Vector2>();
                        this.dxfPairInfo = this.ReadCodePair();
                        for (int i = 0; i < numVertexes; i++)
                        {
                            double x = 0.0;
                            double y = 0.0;
                            if (this.dxfPairInfo.Code == 14) x = double.Parse(this.dxfPairInfo.Value);
                            this.dxfPairInfo = this.ReadCodePair();
                            if (this.dxfPairInfo.Code == 24) y = double.Parse(this.dxfPairInfo.Value);
                            this.dxfPairInfo = this.ReadCodePair();
                            vertexes.Add(new Vector2(x, y));
                        }
                        if (boundaryType == ImageClippingBoundaryType.Rectangular)
                            clippingBoundary = new ImageClippingBoundary(vertexes[0], vertexes[1]);
                        else if (boundaryType == ImageClippingBoundaryType.Polygonal)
                            clippingBoundary = new ImageClippingBoundary(vertexes);
                        else
                            clippingBoundary = null;
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 u = new Vector3(uX, uY, uZ);
            Vector3 v = new Vector3(vX, vY, vZ);
            Vector3 normal = Vector3.CrossProduct(u, v);
            Vector3 uOCS = MathHelper.Transform(u, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            double rotation = Vector2.Angle(new Vector2(uOCS.X, uOCS.Y))*MathHelper.RadToDeg;
            double uLength = u.Modulus();
            double vLength = v.Modulus();

            Image image = new Image
                {
                    Width = width*uLength,
                    Height = height*vLength,
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

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        radius = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        startAngle = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        endAngle = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        radius = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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
            DimensionStyle style = this.GetDimensionStyle(DimensionStyle.Default.Name);
            double dimRot = 0.0;
            double lineSpacingFactor = 1.0;
            bool dimInfo = false;

            this.dxfPairInfo = this.ReadCodePair();
            while (!dimInfo)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        drawingBlockName = this.dxfPairInfo.Value;
                        if (!isBlockEntity) drawingBlock = this.GetBlock(drawingBlockName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        string styleName = this.dxfPairInfo.Value;
                        if (string.IsNullOrEmpty(styleName))
                            styleName = this.doc.DrawingVariables.DimStyle;
                        style = this.GetDimensionStyle(styleName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        defPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        defPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        defPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        midtxtPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        midtxtPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        midtxtPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        dimType = (DimensionTypeFlag) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        lineSpacingStyle = (MTextLineSpacingStyle) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        lineSpacingFactor = double.Parse(this.dxfPairInfo.Value);
                        if (lineSpacingFactor < 0.25 || lineSpacingFactor > 4.0)
                            lineSpacingFactor = 1.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly is this value is not provided
                        dimRot = 360 - double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 100:
                        if (this.dxfPairInfo.Value == SubclassMarker.AlignedDimension ||
                            this.dxfPairInfo.Value == SubclassMarker.RadialDimension ||
                            this.dxfPairInfo.Value == SubclassMarker.DiametricDimension ||
                            this.dxfPairInfo.Value == SubclassMarker.Angular3PointDimension ||
                            this.dxfPairInfo.Value == SubclassMarker.Angular2LineDimension ||
                            this.dxfPairInfo.Value == SubclassMarker.OrdinateDimension)
                            dimInfo = true; // we have finished reading the basic dimension info
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

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

            Dimension dim;
            switch (type)
            {
                case (DimensionTypeFlag.Aligned):
                    dim = this.ReadAlignedDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Linear):
                    dim = this.ReadLinearDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Radius):
                    dim = this.ReadRadialDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Diameter):
                    dim = this.ReadDiametricDimension(defPoint, normal);
                    break;
                case (DimensionTypeFlag.Angular3Point):
                    dim = this.ReadAngular3PointDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Angular):
                    dim = this.ReadAngular2LineDimension(defPoint);
                    break;
                case (DimensionTypeFlag.Ordinate):
                    dim = this.ReadOrdinateDimension(defPoint, axis, normal, dimRot);
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
                this.nestedDimensions.Add(dim, drawingBlockName);

            return dim;
        }

        private AlignedDimension ReadAlignedDimension(Vector3 defPoint)
        {
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            double offset = Vector3.Distance(secondRef, defPoint);
            return new AlignedDimension
                {
                    FirstReferencePoint = firstRef,
                    SecondReferencePoint = secondRef,
                    Offset = offset, XData = xData
                };
        }

        private LinearDimension ReadLinearDimension(Vector3 defPoint)
        {
            Vector3 firtRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;
            double rot = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 13:
                        firtRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firtRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firtRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rot = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 52:
                        // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the dxf documentation
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            Vector3 midPoint = Vector3.MidPoint(firtRef, secondRef);
            Vector3 origin = defPoint;
            Vector3 dir = new Vector3(Math.Cos(rot*MathHelper.DegToRad), Math.Sin(rot*MathHelper.DegToRad), 0.0);
            dir.Normalize();
            double offset = MathHelper.PointLineDistance(midPoint, origin, dir);

            return new LinearDimension
                {
                    FirstReferencePoint = firtRef,
                    SecondReferencePoint = secondRef,
                    Offset = offset,
                    Rotation = rot,
                    XData = xData
                };
        }

        private RadialDimension ReadRadialDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double radius = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Vector2.Angle(firstPoint, seconPoint)*MathHelper.RadToDeg;
            return new RadialDimension(defPoint, radius, rotation)
                {
                    CenterPoint = defPoint,
                    Radius = radius,
                    Rotation = rotation,
                    XData = xData
                };
        }

        private DiametricDimension ReadDiametricDimension(Vector3 defPoint, Vector3 normal)
        {
            Vector3 circunferenceRef = Vector3.Zero;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 15:
                        circunferenceRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        circunferenceRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        circunferenceRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            double diameter = Vector3.Distance(defPoint, circunferenceRef);
            Vector3 refPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstPoint = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(circunferenceRef, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 seconPoint = new Vector2(refPoint.X, refPoint.Y);

            double rotation = Vector2.Angle(firstPoint, seconPoint)*MathHelper.RadToDeg;

            return new DiametricDimension
                {
                    CenterPoint = Vector3.MidPoint(defPoint, circunferenceRef),
                    Diameter = diameter,
                    Rotation = rotation,
                    XData = xData
                };
        }

        private Angular3PointDimension ReadAngular3PointDimension(Vector3 defPoint)
        {
            Vector3 center = Vector3.Zero;
            Vector3 firstRef = Vector3.Zero;
            Vector3 secondRef = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 13:
                        firstRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondRef.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondRef.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondRef.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        center.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        center.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        center.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new Angular3PointDimension
                {
                    CenterPoint = center,
                    FirstPoint = firstRef,
                    SecondPoint = secondRef,
                    Offset = Vector3.Distance(center, defPoint),
                    XData = xData
                };
        }

        private Angular2LineDimension ReadAngular2LineDimension(Vector3 defPoint)
        {
            Vector3 startFirstLine = Vector3.Zero;
            Vector3 endFirstLine = Vector3.Zero;
            Vector3 startSecondLine = Vector3.Zero;
            Vector3 arcDefinitionPoint = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 13:
                        startFirstLine.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        startFirstLine.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        startFirstLine.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        endFirstLine.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        endFirstLine.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        endFirstLine.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        startSecondLine.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        startSecondLine.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        startSecondLine.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 16:
                        arcDefinitionPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 26:
                        arcDefinitionPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 36:
                        arcDefinitionPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return new Angular2LineDimension
                {
                    StartFirstLine = startFirstLine,
                    EndFirstLine = endFirstLine,
                    StartSecondLine = startSecondLine,
                    EndSecondLine = defPoint,
                    Offset = Vector3.Distance(startSecondLine, defPoint),
                    ArcDefinitionPoint = arcDefinitionPoint,
                    XData = xData
                };
        }

        private OrdinateDimension ReadOrdinateDimension(Vector3 defPoint, OrdinateDimensionAxis axis, Vector3 normal, double rotation)
        {
            Vector3 firstPoint = Vector3.Zero;
            Vector3 secondPoint = Vector3.Zero;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 13:
                        firstPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        firstPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        firstPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        secondPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        secondPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        secondPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            Vector3 localPoint = MathHelper.Transform(defPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            localPoint = MathHelper.Transform(firstPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 firstRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation*MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            localPoint = MathHelper.Transform(secondPoint, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 secondRef = MathHelper.Transform(new Vector2(localPoint.X, localPoint.Y) - refCenter, rotation*MathHelper.DegToRad, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            double length = axis == OrdinateDimensionAxis.X ? secondRef.Y - firstRef.Y : secondRef.X - firstRef.X;

            return new OrdinateDimension
                {
                    Origin = defPoint,
                    ReferencePoint = firstRef,
                    Length = length,
                    Rotation = rotation,
                    Axis = axis,
                    XData = xData
                };
        }

        private Ellipse ReadEllipse()
        {
            Vector3 center = Vector3.Zero;
            Vector3 axisPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double[] param = new double[2];
            double ratio = 0.0;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        center.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        axisPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        axisPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        axisPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        ratio = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        param[0] = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        param[1] = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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

            this.SetEllipseParameters(ellipse, param);
            return ellipse;
        }

        private Point ReadPoint()
        {
            Vector3 location = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            double rotation = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        location.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = 360.0 - double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        v0.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (EdgeFlags) (int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        v0.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (SplineTypeFlags) (int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        degree = short.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        numKnots = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        numCtrlPoints = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        numFitPoints = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        knotTolerance = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        ctrlPointTolerance = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        fitTolerance = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        stX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        stY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        stZ = double.Parse(this.dxfPairInfo.Value);
                        startTangent = new Vector3(stX, stY, stZ);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        etX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        etY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        etZ = double.Parse(this.dxfPairInfo.Value);
                        endTangent = new Vector3(stX, stY, stZ);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        // multiple code 40 entries, one per knot value
                        knots = this.ReadSplineKnots(numKnots);
                        break;
                    case 10:
                        ctrlX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        ctrlY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        ctrlZ = double.Parse(this.dxfPairInfo.Value);

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
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        // code 41 might appear before or after the control point coordiantes.
                        // I am open to better ways to handling this.
                        if (ctrlPointIndex == -1)
                        {
                            ctrlWeigth = double.Parse(this.dxfPairInfo.Value);
                        }
                        else
                        {
                            ctrlPoints[ctrlPointIndex].Weigth = double.Parse(this.dxfPairInfo.Value);
                            ctrlWeigth = -1;
                        }
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        fitX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        fitY = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        fitZ = double.Parse(this.dxfPairInfo.Value);
                        fitPoints.Add(new Vector3(fitX, fitY, fitZ));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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
                if (this.dxfPairInfo.Code != 40)
                    throw new DxfException("The knot vector must have " + numKnots + " code 40 entries.");
                knots[i] = double.Parse(this.dxfPairInfo.Value);
                this.dxfPairInfo = this.ReadCodePair();
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
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        blockName = this.dxfPairInfo.Value;
                        if (!isBlockEntity) block = this.GetBlock(blockName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        scale.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        scale.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }


            if (this.dxfPairInfo.Value == DxfObjectCode.Attribute)
            {
                while (this.dxfPairInfo.Value != StringCode.EndSequence)
                {
                    Attribute attribute = this.ReadAttribute(block, isBlockEntity);
                    if (attribute != null)
                        attributes.Add(attribute);
                }
            }

            string endSequenceHandle = string.Empty;
            Layer endSequenceLayer = this.GetLayer(Layer.Default.Name);
            if (this.dxfPairInfo.Value == StringCode.EndSequence)
            {
                // read the end end sequence object until a new element is found
                this.dxfPairInfo = this.ReadCodePair();
                while (this.dxfPairInfo.Code != 0)
                {
                    switch (this.dxfPairInfo.Code)
                    {
                        case 5:
                            endSequenceHandle = this.dxfPairInfo.Value;
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        case 8:
                            endSequenceLayer = this.GetLayer(this.dxfPairInfo.Value);
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                        default:
                            this.dxfPairInfo = this.ReadCodePair();
                            break;
                    }
                }
            }

            // It is a lot more intuitive to give the position in world coordinates and then define the orientation with the normal.
            Vector3 wcsBasePoint = MathHelper.Transform(basePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            EndSequence end = new EndSequence
                {
                    Handle = endSequenceHandle,
                    Layer = endSequenceLayer
                };

            Insert insert = new Insert
                {
                    Block = block,
                    Position = wcsBasePoint,
                    Rotation = rotation,
                    Scale = scale,
                    Normal = normal,
                    Attributes = new AttributeDictionary(attributes),
                    EndSequence = end,
                    XData = xData

                };

            // post process nested inserts
            if (isBlockEntity)
                this.nestedInserts.Add(insert, blockName);

            return insert;
        }

        private Line ReadLine()
        {
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            double thickness = 0.0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        start.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        start.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        start.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        end.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        end.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        end.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        origin.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        origin.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        origin.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        direction.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 10:
                        origin.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        origin.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        origin.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        direction.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        // the MLineStyle is defined in the objects sections after the definition of the entity, something similar happens with the image entity
                        // the MLineStyle will be applied to the MLine after parsing the whole file
                        styleName = this.dxfPairInfo.Value;
                        if (string.IsNullOrEmpty(styleName))
                            styleName = this.doc.DrawingVariables.CMLStyle;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        scale = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        justification = (MLineJustification) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        flags = (MLineFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        numVertexes = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        numStyleElements = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        // this info is not needed it is repeated in the vertexes list
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        // this info is not needed it is repeated in the vertexes list
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        // this info is not needed it is repeated in the vertexes list
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        // the info that follows contains the information on the vertexes of the MLine
                        segments = this.ReadMLineSegments(numVertexes, numStyleElements, normal, out elevation);
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");
                        this.dxfPairInfo = this.ReadCodePair();
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
                vertex.X = double.Parse(this.dxfPairInfo.Value); // code 11
                this.dxfPairInfo = this.ReadCodePair();
                vertex.Y = double.Parse(this.dxfPairInfo.Value); // code 21
                this.dxfPairInfo = this.ReadCodePair();
                vertex.Z = double.Parse(this.dxfPairInfo.Value); // code 31
                this.dxfPairInfo = this.ReadCodePair();

                Vector3 dir = new Vector3();
                dir.X = double.Parse(this.dxfPairInfo.Value); // code 12
                this.dxfPairInfo = this.ReadCodePair();
                dir.Y = double.Parse(this.dxfPairInfo.Value); // code 22
                this.dxfPairInfo = this.ReadCodePair();
                dir.Z = double.Parse(this.dxfPairInfo.Value); // code 32
                this.dxfPairInfo = this.ReadCodePair();

                Vector3 mitter = new Vector3();
                mitter.X = double.Parse(this.dxfPairInfo.Value); // code 13
                this.dxfPairInfo = this.ReadCodePair();
                mitter.Y = double.Parse(this.dxfPairInfo.Value); // code 23
                this.dxfPairInfo = this.ReadCodePair();
                mitter.Z = double.Parse(this.dxfPairInfo.Value); // code 33
                this.dxfPairInfo = this.ReadCodePair();

                List<double>[] distances = new List<double>[numStyleElements];
                for (int j = 0; j < numStyleElements; j++)
                {
                    distances[j] = new List<double>();
                    int numDistances = int.Parse(this.dxfPairInfo.Value); // code 74
                    this.dxfPairInfo = this.ReadCodePair();
                    for (int k = 0; k < numDistances; k++)
                    {
                        distances[j].Add(double.Parse(this.dxfPairInfo.Value)); // code 41
                        this.dxfPairInfo = this.ReadCodePair();
                    }

                    // no more info is needed, fill params are not supported
                    int numFillParams = int.Parse(this.dxfPairInfo.Value); // code 75
                    this.dxfPairInfo = this.ReadCodePair();
                    for (int k = 0; k < numFillParams; k++)
                    {
                        double param = double.Parse(this.dxfPairInfo.Value); // code 42
                        this.dxfPairInfo = this.ReadCodePair();
                    }
                }

                // we need to convert wcs coordinates to ocs coordinates
                if (!normal.Equals(Vector3.UnitZ))
                {
                    vertex = trans*vertex;
                    dir = trans*dir;
                    mitter = trans*mitter;
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
            double elevation = 0.0;
            double thickness = 0.0;
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            double constantWidth = 0.0;
            List<LwPolylineVertex> polVertexes = new List<LwPolylineVertex>();
            LwPolylineVertex v = new LwPolylineVertex();
            double vX = 0.0;
            Vector3 normal = Vector3.UnitZ;

            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 38:
                        elevation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        // constant width (optional; default = 0). Not used if variable width (codes 40 and/or 41) is set
                        constantWidth = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        v = new LwPolylineVertex
                            {
                                BeginWidth = constantWidth,
                                EndWidth = constantWidth
                            };
                        vX = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        double vY = double.Parse(this.dxfPairInfo.Value);
                        v.Location = new Vector2(vX, vY);
                        polVertexes.Add(v);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        v.BeginWidth = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        v.EndWidth = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        v.Bulge = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            return new LwPolyline
                {
                    Vertexes = polVertexes,
                    Elevation = elevation,
                    Thickness = thickness,
                    Flags = flags,
                    Normal = normal,
                    XData = xData
                };
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

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 30:
                        elevation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 39:
                        thickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) (int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numVertexes = int.Parse(code.Value); code = this.ReadCodePair();
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numFaces  = int.Parse(code.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            //begin to read the vertex list (althought it is not recommended the vertex list might have 0 entries)
            while (this.dxfPairInfo.Value != StringCode.EndSequence)
            {
                if (this.dxfPairInfo.Value == DxfObjectCode.Vertex)
                {
                    Vertex vertex = this.ReadVertex();
                    vertexes.Add(vertex);
                }
            }

            // read the end sequence object until a new element is found
            this.dxfPairInfo = this.ReadCodePair();
            string endSequenceHandle = null;
            Layer endSequenceLayer = this.GetLayer(Layer.Default.Name);
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        endSequenceHandle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        endSequenceLayer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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
            string textString = string.Empty;
            double height = 0.0;
            double widthFactor = 1.0;
            double rotation = 0.0;
            double obliqueAngle = 0.0;
            TextStyle style = this.GetTextStyle(TextStyle.Default.Name);
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 1:
                        textString = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(this.dxfPairInfo.Value);
                        if (height <= 0.0)
                            height = this.doc.DrawingVariables.TextSize;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        widthFactor = double.Parse(this.dxfPairInfo.Value);
                        if (widthFactor < 0.01 || widthFactor > 100.0)
                            widthFactor = this.doc.DrawingVariables.TextSize;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        obliqueAngle = double.Parse(this.dxfPairInfo.Value);
                        if (obliqueAngle < -85.0 || obliqueAngle > 85.0)
                            obliqueAngle = 0.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        string styleName = this.dxfPairInfo.Value;
                        if (string.IsNullOrEmpty(styleName))
                            styleName = this.doc.DrawingVariables.TextStyle;
                        style = this.GetTextStyle(styleName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        verticalAlignment = int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);
            Vector3 ocsBasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;

            // another example of this ocs vs wcs non sense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            textString = this.DecodeEncodedNonAsciiCharacters(textString);
            Text text = new Text
                {
                    Value = textString,
                    Height = height,
                    WidthFactor = widthFactor,
                    Rotation = rotation,
                    ObliqueAngle = obliqueAngle,
                    Style = style,
                    Position = MathHelper.Transform(ocsBasePoint, normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World),
                    Normal = normal,
                    Alignment = alignment,
                    XData = xData
                };

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
            TextStyle style = this.GetTextStyle(TextStyle.Default.Name);
            string textString = string.Empty;
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 1:
                        textString += this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        textString += this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        insertionPoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        insertionPoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        insertionPoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        direction.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        direction.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 31:
                        // Z direction value (direction.Z = double.Parse(dxfPairInfo.Value);)
                        // we will always define the angle of the text on the plane where it is defined so Z value will be zero.
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        height = double.Parse(this.dxfPairInfo.Value);
                        if (height <= 0.0)
                            height = this.doc.DrawingVariables.TextSize;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        rectangleWidth = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        lineSpacing = double.Parse(this.dxfPairInfo.Value);
                        if (lineSpacing < 0.25 || lineSpacing > 4.0)
                            lineSpacing = 1.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 50:
                        isRotationDefined = true;
                        rotation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 7:
                        string styleName = this.dxfPairInfo.Value;
                        if (string.IsNullOrEmpty(styleName))
                            styleName = this.doc.DrawingVariables.TextStyle;
                        style = this.GetTextStyle(styleName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        attachmentPoint = (MTextAttachmentPoint) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            textString = this.DecodeEncodedNonAsciiCharacters(textString);

            MText mText = new MText
                {
                    Value = textString,
                    Position = insertionPoint,
                    Height = height,
                    RectangleWidth = rectangleWidth,
                    Style = style,
                    LineSpacingFactor = lineSpacing,
                    AttachmentPoint = attachmentPoint,
                    Rotation = isRotationDefined
                                   ? rotation
                                   : Vector2.Angle(direction)*MathHelper.RadToDeg,
                    Normal = normal,
                    XData = xData
                };

            return mText;
        }

        private Hatch ReadHatch()
        {
            string name = string.Empty;
            HatchFillType fill = HatchFillType.SolidFill;
            double elevation = 0.0;
            Vector3 normal = Vector3.UnitZ;
            HatchPattern pattern = HatchPattern.Line;
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            Dictionary<string, XData> xData = new Dictionary<string, XData>();

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 2:
                        name = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        elevation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 91:
                        // the next lines hold the information about the hatch boundary paths
                        int numPaths = int.Parse(this.dxfPairInfo.Value);
                        paths = this.ReadHatchBoundaryPaths(numPaths);
                        break;
                    case 70:
                        // Solid fill flag
                        fill = (HatchFillType) int.Parse(this.dxfPairInfo.Value);
                        //if (fill == HatchFillType.SolidFill) name = PredefinedHatchPatternName.Solid;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        // Associativity flag (associative = 1; non-associative = 0); for MPolygon, solid-fill flag (has solid fill = 1; lacks solid fill = 0)
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        // the next lines hold the information about the hatch pattern
                        pattern = this.ReadHatchPattern(name);
                        pattern.Fill = fill;
                        break;
                    case 1001:
                        XData xDataItem = this.ReadXDataRecord(this.dxfPairInfo.Value);
                        // to be safe if the xDataItem.ApplicationRegistry.Name already exists we will add the new entry to the existing one
                        if (xData.ContainsKey(xDataItem.ApplicationRegistry.Name))
                            xData[xDataItem.ApplicationRegistry.Name].XDataRecord.AddRange(xDataItem.XDataRecord);
                        else
                            xData.Add(xDataItem.ApplicationRegistry.Name, xDataItem);
                        break;
                    default:
                        if (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
                            throw new DxfInvalidCodeValueEntityException(this.dxfPairInfo.Code, this.dxfPairInfo.Value,
                                                                         "The extended data of an entity must start with the application registry code.");

                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // here is where dxf stores the pattern origin
            XData patternOrigin;
            Vector2 origin = Vector2.Zero;
            if (xData.TryGetValue(ApplicationRegistry.Default.Name, out patternOrigin))
            {
                foreach (XDataRecord record in patternOrigin.XDataRecord)
                {
                    if (record.Code == XDataCode.RealX)
                        origin.X = (double) record.Value;
                    else if (record.Code == XDataCode.RealY)
                        origin.Y = (double)record.Value;
                    // record.Code == XDataCode.RealZ is always 0
                }
            }
            pattern.Origin = origin;

            if (paths.Count == 0) return null;

            return new Hatch(pattern, paths)
                {
                    Elevation = elevation,
                    Normal = normal,
                    XData = xData
                };
        }

        private List<HatchBoundaryPath> ReadHatchBoundaryPaths(int numPaths)
        {
            List<HatchBoundaryPath> paths = new List<HatchBoundaryPath>();
            this.dxfPairInfo = this.ReadCodePair();
            HatchBoundaryPathTypeFlag pathTypeFlag = HatchBoundaryPathTypeFlag.Derived | HatchBoundaryPathTypeFlag.External;
            while (paths.Count < numPaths)
            {
                HatchBoundaryPath path;

                switch (this.dxfPairInfo.Code)
                {
                    case 92:
                        pathTypeFlag = (HatchBoundaryPathTypeFlag)int.Parse(this.dxfPairInfo.Value);
                        // adding External and Derived to all path type flags solves an strange problem with code 98 not found,
                        // it seems related to the code 47 that appears before, only some combinations of flags are affected
                        // this is what the documentation says about code 47:
                        // Pixel size used to determine the density to perform various intersection and ray casting operations
                        // in hatch pattern computation for associative hatches and hatches created with the Flood method of hatching
                        pathTypeFlag = pathTypeFlag | HatchBoundaryPathTypeFlag.External | HatchBoundaryPathTypeFlag.Derived;

                        if ((pathTypeFlag & HatchBoundaryPathTypeFlag.Polyline) == HatchBoundaryPathTypeFlag.Polyline)
                        {
                            path = this.ReadEdgePolylineBoundaryPath();
                            path.PathTypeFlag = pathTypeFlag;
                            paths.Add(path);
                        }
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 93:
                        int numEdges = int.Parse(this.dxfPairInfo.Value);
                        path = this.ReadEdgeBoundaryPath(numEdges);
                        path.PathTypeFlag = pathTypeFlag;
                        paths.Add(path);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        // references to boundary objects, not supported
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            return paths;
        }

        private HatchBoundaryPath ReadEdgePolylineBoundaryPath()
        {
            HatchBoundaryPath.Polyline poly = new HatchBoundaryPath.Polyline();

            this.dxfPairInfo = this.ReadCodePair();

            bool hasBulge = int.Parse(this.dxfPairInfo.Value) != 0;
            this.dxfPairInfo = this.ReadCodePair();

            // is polyline closed
            poly.IsClosed = int.Parse(this.dxfPairInfo.Value) == 1;
            this.dxfPairInfo = this.ReadCodePair();

            int numVertexes = int.Parse(this.dxfPairInfo.Value); // code 93
            poly.Vertexes = new Vector3[numVertexes];
            this.dxfPairInfo = this.ReadCodePair();

            for (int i = 0; i < numVertexes; i++)
            {
                double bulge = 0.0;
                double x = double.Parse(this.dxfPairInfo.Value); // code 10
                this.dxfPairInfo = this.ReadCodePair();
                double y = double.Parse(this.dxfPairInfo.Value); // code 20
                this.dxfPairInfo = this.ReadCodePair();
                if (hasBulge)
                {
                    bulge = double.Parse(this.dxfPairInfo.Value); // code 42
                    this.dxfPairInfo = this.ReadCodePair();
                }
                poly.Vertexes[i] = new Vector3(x, y, bulge);
            }
            return new HatchBoundaryPath(new List<HatchBoundaryPath.Edge> {poly});
        }

        private HatchBoundaryPath ReadEdgeBoundaryPath(int numEdges)
        {
            // the information of the boundary path data always appear exactly as it is readed
            List<HatchBoundaryPath.Edge> entities = new List<HatchBoundaryPath.Edge>();
            this.dxfPairInfo = this.ReadCodePair();

            while (entities.Count < numEdges)
            {
                // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline
                if (this.dxfPairInfo.Code != 72) throw new ArgumentException();
                HatchBoundaryPath.EdgeType type = (HatchBoundaryPath.EdgeType) int.Parse(this.dxfPairInfo.Value);
                switch (type)
                {
                    case HatchBoundaryPath.EdgeType.Line:
                        this.dxfPairInfo = this.ReadCodePair();
                        // line
                        double lX1 = double.Parse(this.dxfPairInfo.Value); // code 10
                        this.dxfPairInfo = this.ReadCodePair();
                        double lY1 = double.Parse(this.dxfPairInfo.Value); // code 20
                        this.dxfPairInfo = this.ReadCodePair();
                        double lX2 = double.Parse(this.dxfPairInfo.Value); // code 11
                        this.dxfPairInfo = this.ReadCodePair();
                        double lY2 = double.Parse(this.dxfPairInfo.Value); // code 21
                        this.dxfPairInfo = this.ReadCodePair();

                        HatchBoundaryPath.Line line = new HatchBoundaryPath.Line();
                        line.Start = new Vector2(lX1, lY1);
                        line.End = new Vector2(lX2, lY2);
                        entities.Add(line);
                        break;
                    case HatchBoundaryPath.EdgeType.Arc:
                        this.dxfPairInfo = this.ReadCodePair();
                        // circular arc
                        double aX = double.Parse(this.dxfPairInfo.Value); // code 10
                        this.dxfPairInfo = this.ReadCodePair();
                        double aY = double.Parse(this.dxfPairInfo.Value); // code 40
                        this.dxfPairInfo = this.ReadCodePair();
                        double aR = double.Parse(this.dxfPairInfo.Value); // code 40
                        this.dxfPairInfo = this.ReadCodePair();
                        double aStart = double.Parse(this.dxfPairInfo.Value); // code 50
                        this.dxfPairInfo = this.ReadCodePair();
                        double aEnd = double.Parse(this.dxfPairInfo.Value); // code 51
                        this.dxfPairInfo = this.ReadCodePair();
                        bool aCCW = int.Parse(this.dxfPairInfo.Value) != 0; // code 73
                        this.dxfPairInfo = this.ReadCodePair();

                        HatchBoundaryPath.Arc arc = new HatchBoundaryPath.Arc();
                        arc.Center = new Vector2(aX, aY);
                        arc.Radius = aR;
                        arc.StartAngle = aStart;
                        arc.EndAngle = aEnd;
                        arc.IsCounterclockwise = aCCW;
                        entities.Add(arc);
                        break;
                    case HatchBoundaryPath.EdgeType.Ellipse:
                        this.dxfPairInfo = this.ReadCodePair();
                        // elliptic arc
                        double eX = double.Parse(this.dxfPairInfo.Value); // code 10
                        this.dxfPairInfo = this.ReadCodePair();
                        double eY = double.Parse(this.dxfPairInfo.Value); // code 20
                        this.dxfPairInfo = this.ReadCodePair();
                        double eAxisX = double.Parse(this.dxfPairInfo.Value); // code 11
                        this.dxfPairInfo = this.ReadCodePair();
                        double eAxisY = double.Parse(this.dxfPairInfo.Value); // code 21
                        this.dxfPairInfo = this.ReadCodePair();
                        double eAxisRatio = double.Parse(this.dxfPairInfo.Value); // code 40
                        this.dxfPairInfo = this.ReadCodePair();
                        double eStart = double.Parse(this.dxfPairInfo.Value); // code 50
                        this.dxfPairInfo = this.ReadCodePair();
                        double eEnd = double.Parse(this.dxfPairInfo.Value); // code 51
                        this.dxfPairInfo = this.ReadCodePair();
                        bool eCCW = int.Parse(this.dxfPairInfo.Value) != 0; // code 73
                        this.dxfPairInfo = this.ReadCodePair();

                        HatchBoundaryPath.Ellipse ellipse = new HatchBoundaryPath.Ellipse();
                        ellipse.Center = new Vector2(eX, eY);
                        ellipse.EndMajorAxis = new Vector2(eAxisX, eAxisY);
                        ellipse.MinorRatio = eAxisRatio;
                        ellipse.StartAngle = eStart;
                        ellipse.EndAngle = eEnd;
                        ellipse.IsCounterclockwise = eCCW;
                        
                        entities.Add(ellipse);
                        break;
                    case HatchBoundaryPath.EdgeType.Spline:
                        this.dxfPairInfo = this.ReadCodePair();
                        // spline

                        short degree = short.Parse(this.dxfPairInfo.Value); // code 94
                        this.dxfPairInfo = this.ReadCodePair();

                        bool isRational = int.Parse(this.dxfPairInfo.Value) != 0; // code 73
                        this.dxfPairInfo = this.ReadCodePair();

                        bool isPeriodic = int.Parse(this.dxfPairInfo.Value) != 0; // code 74
                        this.dxfPairInfo = this.ReadCodePair();

                        int numKnots = int.Parse(this.dxfPairInfo.Value); // code 95
                        double[] knots = new double[numKnots];
                        this.dxfPairInfo = this.ReadCodePair();

                        int numControlPoints = int.Parse(this.dxfPairInfo.Value); // code 96
                        Vector3[] controlPoints = new Vector3[numControlPoints];
                        this.dxfPairInfo = this.ReadCodePair();
                     
                        for (int i = 0; i < numKnots; i++)
                        {
                            knots[i] = double.Parse(this.dxfPairInfo.Value); // code 40
                            this.dxfPairInfo = this.ReadCodePair();
                        }

                        for (int i = 0; i < numControlPoints; i++)
                        {
                            double x = double.Parse(this.dxfPairInfo.Value); // code 10
                            this.dxfPairInfo = this.ReadCodePair();

                            double y = double.Parse(this.dxfPairInfo.Value); // code 20
                            this.dxfPairInfo = this.ReadCodePair();

                            // control point weight might not be present
                            double w = 1.0;
                            if (this.dxfPairInfo.Code == 42)
                            {
                                w = double.Parse(this.dxfPairInfo.Value); // code 42
                                this.dxfPairInfo = this.ReadCodePair();
                            }

                            controlPoints[i] = new Vector3(x, y, w);
                        }

                        // this information is only required for AutoCAD version 2010
                        // stores information about spline fit point (the spline entity does not make use of this information)
                        if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2010)
                        {
                            int numFitData = int.Parse(this.dxfPairInfo.Value); // code 97
                            this.dxfPairInfo = this.ReadCodePair();
                            for (int i = 0; i < numFitData; i++)
                            {
                                double fitX = double.Parse(this.dxfPairInfo.Value); // code 11
                                this.dxfPairInfo = this.ReadCodePair();
                                double fitY = double.Parse(this.dxfPairInfo.Value); // code 21
                                this.dxfPairInfo = this.ReadCodePair();
                            }

                            // the info on start tangent might not appear
                            if (this.dxfPairInfo.Code == 12)
                            {
                                double startTanX = double.Parse(this.dxfPairInfo.Value); // code 12
                                this.dxfPairInfo = this.ReadCodePair();
                                double startTanY = double.Parse(this.dxfPairInfo.Value); // code 22
                                this.dxfPairInfo = this.ReadCodePair();
                            }
                            // the info on end tangent might not appear
                            if (this.dxfPairInfo.Code == 13)
                            {
                                double endTanX = double.Parse(this.dxfPairInfo.Value); // code 13
                                this.dxfPairInfo = this.ReadCodePair();
                                double endTanY = double.Parse(this.dxfPairInfo.Value); // code 23
                                this.dxfPairInfo = this.ReadCodePair();
                            }
                        }

                        //Spline spline = new Spline(controlPoints, knots, degree);
                        HatchBoundaryPath.Spline spline = new HatchBoundaryPath.Spline();
                        spline.Degree = degree;
                        spline.IsPeriodic = isPeriodic;
                        spline.IsRational = isRational;
                        spline.ControlPoints = controlPoints;
                        spline.Knots = knots;

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

            while (this.dxfPairInfo.Code != 0 && this.dxfPairInfo.Code != 1001)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 52:
                        angle = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        scale = double.Parse(this.dxfPairInfo.Value);
                        if (scale <= 0)
                            scale = 1.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 47:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 98:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 75:
                        style = (HatchStyle) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 76:
                        type = (HatchType) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 77:
                        // hatch pattern double flag (not used)
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 78:
                        // number of pattern definition lines
                        int numLines = int.Parse(this.dxfPairInfo.Value);
                        lineDefinitions = this.ReadHatchPatternDefinitionLine(scale, angle, numLines);
                        break;
                    case 450:
                        if (int.Parse(this.dxfPairInfo.Value) == 1)
                        {
                            isGradient = true; // gradient pattern
                            hatch = this.ReadHatchGradientPattern();
                        }
                        else
                            this.dxfPairInfo = this.ReadCodePair(); // solid hatch, we do not need to read anything else
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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
            this.dxfPairInfo = this.ReadCodePair(); // code 451 not needed
            this.dxfPairInfo = this.ReadCodePair();
            double angle = double.Parse(this.dxfPairInfo.Value); // code 460
            this.dxfPairInfo = this.ReadCodePair();
            bool centered = ((int) double.Parse(this.dxfPairInfo.Value) == 0); // code 461
            this.dxfPairInfo = this.ReadCodePair();
            bool singleColor = (int.Parse(this.dxfPairInfo.Value) != 0); // code 452
            this.dxfPairInfo = this.ReadCodePair();
            double tint = double.Parse(this.dxfPairInfo.Value); // code 462
            this.dxfPairInfo = this.ReadCodePair(); // code 453 not needed

            this.dxfPairInfo = this.ReadCodePair(); // code 463 not needed (0.0)
            this.dxfPairInfo = this.ReadCodePair(); // code 63
            this.dxfPairInfo = this.ReadCodePair(); // code 421
            AciColor color1 = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));

            this.dxfPairInfo = this.ReadCodePair(); // code 463 not needed (1.0)
            this.dxfPairInfo = this.ReadCodePair(); // code 63
            this.dxfPairInfo = this.ReadCodePair(); // code 421
            AciColor color2 = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));

            this.dxfPairInfo = this.ReadCodePair(); // code 470
            string typeName = this.dxfPairInfo.Value;
            if (!StringEnum.IsStringDefined(typeof (HatchGradientPatternType), typeName))
                throw new DxfEntityException("HatchPatternGradient", "Unkown hatch gradient type: " + typeName);
            HatchGradientPatternType type = (HatchGradientPatternType) StringEnum.Parse(typeof (HatchGradientPatternType), typeName);

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

        private List<HatchPatternLineDefinition> ReadHatchPatternDefinitionLine(double patternScale, double patternAngle, int numLines)
        {
            List<HatchPatternLineDefinition> lineDefinitions = new List<HatchPatternLineDefinition>();

            this.dxfPairInfo = this.ReadCodePair();
            for (int i = 0; i < numLines; i++)
            {
                Vector2 origin = Vector2.Zero;
                Vector2 delta = Vector2.Zero;

                double angle = double.Parse(this.dxfPairInfo.Value); // code 53
                this.dxfPairInfo = this.ReadCodePair();

                origin.X = double.Parse(this.dxfPairInfo.Value); // code 43
                this.dxfPairInfo = this.ReadCodePair();

                origin.Y = double.Parse(this.dxfPairInfo.Value); // code 44
                this.dxfPairInfo = this.ReadCodePair();

                delta.X = double.Parse(this.dxfPairInfo.Value); // code 45
                this.dxfPairInfo = this.ReadCodePair();

                delta.Y = double.Parse(this.dxfPairInfo.Value); // code 46
                this.dxfPairInfo = this.ReadCodePair();

                int numSegments = int.Parse(this.dxfPairInfo.Value); // code 79
                this.dxfPairInfo = this.ReadCodePair();

                List<double> dashPattern = new List<double>();
                for (int j = 0; j < numSegments; j++)
                {
                    // positive values means solid segments and negative values means spaces (one entry per element)
                    dashPattern.Add(double.Parse(this.dxfPairInfo.Value)/patternScale); // code 49
                    this.dxfPairInfo = this.ReadCodePair();
                }

                // Pattern fill data. In theory this should hold the same information as the pat file but for unkown reason the dxf requires global data instead of local.
                // this means we have to convert the global data into local, since we are storing the pattern line definition as it appears in the acad.pat file.
                double sinOrigin = Math.Sin(patternAngle*MathHelper.DegToRad);
                double cosOrigin = Math.Cos(patternAngle*MathHelper.DegToRad);
                origin = new Vector2(cosOrigin*origin.X/patternScale + sinOrigin*origin.Y/patternScale, -sinOrigin*origin.X/patternScale + cosOrigin*origin.Y/patternScale);

                double sinDelta = Math.Sin(angle*MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle*MathHelper.DegToRad);
                delta = new Vector2(cosDelta*delta.X/patternScale + sinDelta*delta.Y/patternScale, -sinDelta*delta.X/patternScale + cosDelta*delta.Y/patternScale);

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
            Layer layer = this.GetLayer(Layer.Default.Name);
            AciColor color = AciColor.ByLayer;
            LineType lineType = this.GetLineType(LineType.ByLayer.Name);
            Vector3 location = new Vector3();
            double endThickness = 0.0;
            double beginThickness = 0.0;
            double bulge = 0.0;
            List<int> vertexIndexes = new List<int>();
            VertexTypeFlags flags = VertexTypeFlags.PolylineVertex;

            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 8:
                        layer = this.GetLayer(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        if (!color.UseTrueColor)
                            color = AciColor.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420: //the entity uses true color
                        color = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        // the linetype names ByLayer or ByBlock are case unsensitive
                        string lineTypeName = this.dxfPairInfo.Value;
                        if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByLayer";
                        if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                            lineTypeName = "ByBlock";
                        lineType = this.GetLineType(lineTypeName);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        beginThickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        endThickness = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        bulge = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        vertexIndexes.Add(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        vertexIndexes.Add(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        vertexIndexes.Add(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 74:
                        vertexIndexes.Add(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                this.dxfPairInfo = this.ReadCodePair();
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

            foreach (KeyValuePair<string, string> entry in namedDict.Entries)
            {
                switch (entry.Value)
                {
                    case StringCode.GroupDictionary:
                        groupsHandle = entry.Key;
                        break;
                    case StringCode.LayoutDictionary:
                        layoutsHandle = entry.Key;
                        break;
                    case StringCode.MLineStyleDictionary:
                        mlineStylesHandle = entry.Key;
                        break;
                    case StringCode.ImageDefDictionary:
                        imageDefsHandle = entry.Key;
                        break;
                }
            }

            // create the collections with the provided handles
            this.doc.Groups = new Groups(this.doc, groupsHandle);
            this.doc.Layouts = new Layouts(this.doc, layoutsHandle);
            this.doc.MlineStyles = new MLineStyles(this.doc, mlineStylesHandle);
            this.doc.ImageDefinitions = new ImageDefinitions(this.doc, imageDefsHandle);
        }

        private DictionaryObject ReadDictionary()
        {
            string handle = null;
            string handleOwner = null;
            DictionaryClonningFlag clonning = DictionaryClonningFlag.KeepExisting;
            bool isHardOwner = false;
            int numEntries = 0;
            List<string> names = new List<string>();
            List<string> handlesToOwner = new List<string>();

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        handleOwner = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 280:
                        isHardOwner = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        clonning = (DictionaryClonningFlag) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        numEntries += 1;
                        names.Add(this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 350: // Soft-owner ID/handle to entry object 
                        handlesToOwner.Add(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 360:
                        // Hard-owner ID/handle to entry object
                        handlesToOwner.Add(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            DxfObject owner = null;
            //if (handleOwner != null)
            //    owner = this.doc.AddedObjects[handleOwner];

            DictionaryObject dictionary = new DictionaryObject(owner)
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
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        variables.Handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        variables.DisplayFrame = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        variables.DisplayQuality = (ImageDisplayQuality) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        variables.Units = (ImageUnits) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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
            ImageResolutionUnits units = ImageResolutionUnits.NoUnits;

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        fileName = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        width = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        height = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        wPixel = float.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        hPixel = float.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 281:
                        units = (ImageResolutionUnits) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        ownerHandle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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

            // The documentation says that this is the size of one pixel in AutoCAD units, but it seems that this is always the size of one pixel in milimeters
            // this value is used to calculate the image resolution in ppi or ppc, and the default image size.
            // The documentation in this regard and its relation with the final image size in drawing units is a complete nonsense
            double factor = MathHelper.ConversionFactor((ImageUnits)units, DrawingUnits.Millimeters);
            ImageDef imageDef = new ImageDef(fileName, name, (int)width, (float)factor / wPixel, (int)height, (float)factor / hPixel, units)
                {
                    Handle = handle
                };

            this.imgDefHandles.Add(imageDef.Handle, imageDef);
            return imageDef;
        }

        private ImageDefReactor ReadImageDefReactor()
        {
            string handle = null;
            string imgOwner = null;

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        imgOwner = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
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

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 3:
                        description = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 62:
                        if (!fillColor.UseTrueColor)
                            fillColor = AciColor.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 420:
                        fillColor = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value));
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (MLineStyleFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 51:
                        startAngle = double.Parse(this.dxfPairInfo.Value);
                        if (startAngle < 10.0 || startAngle > 170.0)
                            startAngle = 90.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 52:
                        endAngle = double.Parse(this.dxfPairInfo.Value);
                        if (endAngle < 10.0 || endAngle > 170.0)
                            endAngle = 90.0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        int numElements = int.Parse(this.dxfPairInfo.Value);
                        elements = this.ReadMLineStyleElements(numElements);
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }
            if (string.IsNullOrEmpty(name)) return null;

            MLineStyle style = new MLineStyle(name, description)
                {
                    Handle = handle,
                    FillColor = fillColor,
                    Flags = flags,
                    StartAngle = startAngle,
                    EndAngle = endAngle,
                    Elements = elements
                };

            return style;
        }

        private List<MLineStyleElement> ReadMLineStyleElements(int numElements)
        {
            List<MLineStyleElement> elements = new List<MLineStyleElement>();

            this.dxfPairInfo = this.ReadCodePair();

            for (int i = 0; i < numElements; i++)
            {
                double offset = double.Parse(this.dxfPairInfo.Value); // code 49
                this.dxfPairInfo = this.ReadCodePair();

                AciColor color = AciColor.FromCadIndex(short.Parse(this.dxfPairInfo.Value));
                this.dxfPairInfo = this.ReadCodePair();

                if (this.dxfPairInfo.Code == 420)
                {
                    color = AciColor.FromTrueColor(int.Parse(this.dxfPairInfo.Value)); // code 420
                    this.dxfPairInfo = this.ReadCodePair();
                }

                // the linetype names ByLayer or ByBlock are case unsensitive
                string lineTypeName = this.dxfPairInfo.Value; // code 6
                if (String.Compare(lineTypeName, "ByLayer", StringComparison.OrdinalIgnoreCase) == 0)
                    lineTypeName = "ByLayer";
                if (String.Compare(lineTypeName, "ByBlock", StringComparison.OrdinalIgnoreCase) == 0)
                    lineTypeName = "ByBlock";
                LineType lineType = this.GetLineType(lineTypeName);
                this.dxfPairInfo = this.ReadCodePair();

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
            List<string> entities = new List<string>();
            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        string handleOwner = this.dxfPairInfo.Value;
                        DictionaryObject dict = this.dictionaries[handleOwner];
                        if (handle == null)
                            throw new NullReferenceException("Null handle in Group dictionary.");
                        name = dict.Entries[handle];
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 300:
                        description = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        isUnnamed = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 71:
                        isSelectable = int.Parse(this.dxfPairInfo.Value) != 0;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 340:
                        string entity = this.dxfPairInfo.Value;
                        entities.Add(entity);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // we need to keep track of the group names generated
            if (isUnnamed)
                this.CheckGroupName(name);

            Group group = new Group
                {
                    Handle = handle,
                    Name = name,
                    Description = description,
                    IsUnnamed = isUnnamed,
                    IsSelectable = isSelectable
                };

            // the group entities will be processed later
            this.groupEntities.Add(group, entities);

            return group;
        }

        private Layout ReadLayout()
        {
            PlotSettings plot = new PlotSettings();
            string handle = null;
            string owner = null;
            string name = null;
            int tabOrder = 1;
            Vector2 minLimit = new Vector2(-20.0, -7.5);
            Vector2 maxLimit = new Vector2(277.0, 202.5);
            Vector3 basePoint = Vector3.Zero;
            Vector3 minExtents = new Vector3(25.7, 19.5, 0.0);
            Vector3 maxExtents = new Vector3(231.3, 175.5, 0.0);
            double elevation = 0;
            Vector3 ucsOrigin = Vector3.Zero;
            Vector3 ucsXAxis = Vector3.UnitX;
            Vector3 ucsYAxis = Vector3.UnitY;
            BlockRecord ownerRecord = null;
            //Block ownerBlock = null;

            string dxfCode = this.dxfPairInfo.Value;
            this.dxfPairInfo = this.ReadCodePair();

            // DxfObject common codes
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 0:
                        throw new DxfEntityException(dxfCode, "Premature end of object definition.");
                    case 5:
                        handle = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 102:
                        this.ReadExtensionDictionaryGroup();
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        owner = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            while (this.dxfPairInfo.Code != 0)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 100:
                        if (this.dxfPairInfo.Value == SubclassMarker.PlotSettings)
                            plot = this.ReadPlotSettings();
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 1:
                        name = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 10:
                        minLimit.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 20:
                        minLimit.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 11:
                        maxLimit.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 21:
                        maxLimit.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 12:
                        basePoint.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 22:
                        basePoint.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 32:
                        basePoint.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 14:
                        minExtents.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 24:
                        minExtents.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 34:
                        minExtents.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 15:
                        maxExtents.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 25:
                        maxExtents.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 35:
                        maxExtents.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 146:
                        elevation = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 13:
                        ucsOrigin.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 23:
                        ucsOrigin.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 33:
                        ucsOrigin.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 16:
                        ucsXAxis.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 26:
                        ucsXAxis.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 36:
                        ucsXAxis.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 17:
                        ucsYAxis.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 27:
                        ucsYAxis.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 37:
                        ucsYAxis.Z = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 330:
                        string blockHandle = this.dxfPairInfo.Value;
                        ownerRecord = (BlockRecord) this.doc.GetObjectByHandle(blockHandle);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            // if the layout has an invalid block record discard it
            if (ownerRecord == null)
                return null;

            Layout layout = new Layout(name)
                {
                    PlotSettings = plot,
                    Handle = handle,
                    TabOrder = tabOrder,
                    MinLimit = minLimit,
                    MaxLimit = maxLimit,
                    BasePoint = basePoint,
                    MinExtents = minExtents,
                    MaxExtents = maxExtents,
                    Elevation = elevation,
                    UcsOrigin = ucsOrigin,
                    UcsXAxis = ucsXAxis,
                    UcsYAxis = ucsYAxis,
                    AssociatedBlock = this.doc.Blocks[ownerRecord.Name]
                };

            return layout;
        }

        private PlotSettings ReadPlotSettings()
        {
            PlotSettings plot = new PlotSettings();
            Vector2 paperSize = plot.PaperSize;
            Vector2 windowBottomLeft = plot.WindowBottomLeft;
            Vector2 windowUpRight = plot.WindowUpRight;
            Vector2 paperImageOrigin = plot.PaperImageOrigin;

            this.dxfPairInfo = this.ReadCodePair();
            while (this.dxfPairInfo.Code != 100)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 1:
                        plot.PageSetupName = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 2:
                        plot.PlotterName = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 4:
                        plot.PaperSizeName = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 6:
                        plot.ViewName = this.dxfPairInfo.Value;
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 40:
                        plot.LeftMargin = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 41:
                        plot.BottomMargin = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 42:
                        plot.RightMargin = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 43:
                        plot.TopMargin = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 44:
                        paperSize.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 45:
                        paperSize.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 48:
                        windowBottomLeft.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 49:
                        windowUpRight.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 140:
                        windowBottomLeft.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 141:
                        windowUpRight.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 142:
                        plot.PrintScaleNumerator = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 143:
                        plot.PrintScaleDenominator = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 70:
                        plot.Flags = (PlotFlags) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 72:
                        plot.PaperUnits = (PlotPaperUnits) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 73:
                        plot.PaperRotation = (PlotRotation) int.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 148:
                        paperImageOrigin.X = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    case 149:
                        paperImageOrigin.Y = double.Parse(this.dxfPairInfo.Value);
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                    default:
                        this.dxfPairInfo = this.ReadCodePair();
                        break;
                }
            }

            plot.PaperSize = paperSize;
            plot.WindowBottomLeft = windowBottomLeft;
            plot.WindowUpRight = windowUpRight;
            plot.PaperImageOrigin = paperImageOrigin;

            return plot;
        }

        #endregion

        #region private methods

        private void ReadExtensionDictionaryGroup()
        {
            string dictionaryGroup = this.dxfPairInfo.Value.Remove(0, 1);
            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code != 102)
            {
                switch (this.dxfPairInfo.Code)
                {
                    case 330:
                        break;
                    case 360:
                        break;
                }
                this.dxfPairInfo = this.ReadCodePair();
            }
        }

        private string DecodeEncodedNonAsciiCharacters(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string decoded;
            if (this.decodedStrings.TryGetValue(text, out decoded)) return decoded;

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

            // dencoding of encoded non ASCII characters, including the extended chart, using regular expresions,
            // this code is slower depends on the number of non ASCII characters in the string
            //decoded = Regex.Replace(
            //    text,
            //    @"\\U\+(?<hex>[a-zA-Z0-9]{4})",
            //    m => ((char)int.Parse(m.Groups["hex"].Value, NumberStyles.HexNumber)).ToString(CultureInfo.InvariantCulture), RegexOptions.IgnoreCase);

            this.decodedStrings.Add(text, decoded);
            return decoded;
        }

        private void SetEllipseParameters(Ellipse ellipse, double[] param)
        {
            double a = ellipse.MajorAxis*0.5;
            double b = ellipse.MinorAxis*0.5;

            Vector2 startPoint = new Vector2(a*Math.Cos(param[0]), b*Math.Sin(param[0]));
            Vector2 endPoint = new Vector2(a*Math.Cos(param[1]), b*Math.Sin(param[1]));

            // trigonometry functions are very prone to round off errors
            if (startPoint.Equals(endPoint))
            {
                ellipse.StartAngle = 0.0;
                ellipse.EndAngle = 360.0;
            }
            else
            {
                ellipse.StartAngle = Vector2.Angle(startPoint)*MathHelper.RadToDeg;
                ellipse.EndAngle = Vector2.Angle(endPoint)*MathHelper.RadToDeg;
            }
        }

        private void CheckDimBlockName(string name)
        {
            // the autocad block names has the form *D#
            // we need to find which is the last available number, in case more dimensions are added
            if (!name.StartsWith("*D", StringComparison.OrdinalIgnoreCase)) return;
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num)) return;
            if (num > this.doc.DimensionBlocksGenerated)
                this.doc.DimensionBlocksGenerated = num;
        }

        private void CheckGroupName(string name)
        {
            // the autocad group names has the form *A#
            // we need to find which is the last available number, in case more groups are added
            if (!name.StartsWith("*A", StringComparison.OrdinalIgnoreCase)) return;
            int num;
            string token = name.Remove(0, 2);
            if (!int.TryParse(token, out num)) return;
            if (num > this.doc.GroupNamesGenerated)
                this.doc.GroupNamesGenerated = num;
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
            name = this.DecodeEncodedNonAsciiCharacters(name);

            ApplicationRegistry appReg;
            if (this.doc.ApplicationRegistries.TryGetValue(name, out appReg))
                return appReg;

            // if an entity references a table object not defined in the tables section a new one will be created
            appReg = new ApplicationRegistry(name);
            this.doc.ApplicationRegistries.Add(appReg);
            return appReg;
        }

        private Block GetBlock(string name)
        {
            Block block;
            if (this.doc.Blocks.TryGetValue(name, out block))
                return block;
            throw new ArgumentException("The block with name " + name + " does not exist.");
        }

        private Layer GetLayer(string name)
        {
            name = this.DecodeEncodedNonAsciiCharacters(name);

            Layer layer;
            if (this.doc.Layers.TryGetValue(name, out layer))
                return layer;

            // if an entity references a table object not defined in the tables section a new one will be created
            layer = new Layer(name);
            layer.LineType = this.GetLineType(layer.LineType.Name);
            this.doc.Layers.Add(layer);
            return layer;
        }

        private LineType GetLineType(string name)
        {
            name = this.DecodeEncodedNonAsciiCharacters(name);

            LineType lineType;
            if (this.doc.LineTypes.TryGetValue(name, out lineType))
                return lineType;

            // if an entity references a table object not defined in the tables section a new one will be created
            lineType = new LineType(name);
            this.doc.LineTypes.Add(lineType);
            return lineType;
        }

        private TextStyle GetTextStyle(string name)
        {
            name = this.DecodeEncodedNonAsciiCharacters(name);

            TextStyle style;
            if (this.doc.TextStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            style = new TextStyle(name);
            this.doc.TextStyles.Add(style);
            return style;
        }

        private TextStyle GetTextStyleByHandle(string handle)
        {
            TextStyle style = (TextStyle) this.doc.GetObjectByHandle(handle);
            if (style == null)
                throw new ArgumentException("The text style with handle " + handle + " does not exist.");
            return style;
        }

        private DimensionStyle GetDimensionStyle(string name)
        {
            name = this.DecodeEncodedNonAsciiCharacters(name);

            DimensionStyle style;
            if (this.doc.DimensionStyles.TryGetValue(name, out style))
                return style;

            // if an entity references a table object not defined in the tables section a new one will be created
            style = new DimensionStyle(name);
            style.TextStyle = this.GetTextStyle(style.TextStyle.Name);
            this.doc.DimensionStyles.Add(style);
            return style;
        }

        private MLineStyle GetMLineStyle(string name)
        {
            name = this.DecodeEncodedNonAsciiCharacters(name);

            MLineStyle style;
            if (this.doc.MlineStyles.TryGetValue(name, out style))
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

        private XData ReadXDataRecord(string appId)
        {
            ApplicationRegistry appReg = this.GetApplicationRegistry(appId);

            XData xData = new XData(appReg);
            this.dxfPairInfo = this.ReadCodePair();

            while (this.dxfPairInfo.Code >= 1000 && this.dxfPairInfo.Code <= 1071)
            {
                if (this.dxfPairInfo.Code == XDataCode.AppReg)
                    break;

                int code = this.dxfPairInfo.Code;
                object value = null;
                switch (code)
                {
                    case XDataCode.String:
                        value = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case XDataCode.AppReg:
                        // Application name cannot appear inside the extended data, AutoCAD assumes it is the beginning of a new application extended data group
                        break;
                    case XDataCode.ControlString:
                        value = this.dxfPairInfo.Value;
                        break;
                    case XDataCode.LayerName:
                        value = this.DecodeEncodedNonAsciiCharacters(this.dxfPairInfo.Value);
                        break;
                    case XDataCode.BinaryData:
                        value = this.dxfPairInfo.Value;
                        break;
                    case XDataCode.DatabaseHandle:
                        value = this.dxfPairInfo.Value;
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
                        value = double.Parse(this.dxfPairInfo.Value);
                        break;

                    case XDataCode.Integer:
                        value = int.Parse(this.dxfPairInfo.Value);
                        break;

                    case XDataCode.Long:
                        value = long.Parse(this.dxfPairInfo.Value);
                        break;
                }

                XDataRecord xDataRecord = new XDataRecord(code, value);
                xData.XDataRecord.Add(xDataRecord);
                this.dxfPairInfo = this.ReadCodePair();
            }

            return xData;
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

        #endregion

    }
}