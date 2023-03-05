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
using TextAlignment = netDxf.Entities.TextAlignment;
using Trace = netDxf.Entities.Trace;

namespace netDxf.IO
{
    /// <summary>
    /// Low level DXF writer.
    /// </summary>
    internal sealed class DxfWriter
    {
        #region private fields

        private bool isBinary;
        private string activeSection = DxfObjectCode.Unknown;
        private string activeTable = DxfObjectCode.Unknown;
        private ICodeValueWriter chunk;
        private DxfDocument doc;

        // here we will store strings already encoded <string: original, string: encoded>
        private Dictionary<string, string> encodedStrings;

        // preprocess smoothed polyline2D, polyline3D, polyface meshes, and polygon meshes
        private Dictionary<string, Polyline> polylines;

        // preprocess image definition reactors
        private Dictionary<string, Dictionary<string, ImageDefinitionReactor>> imageDefReactors;

        // preprocess insert entities
        private Dictionary<string, EndSequence> insertEndSequences;

        #endregion

        #region constructors

        #endregion

        #region public methods

        public void Write(Stream stream, DxfDocument document, bool binary)
        {
            this.doc = document;
            this.isBinary = binary;
            DxfVersion version = this.doc.DrawingVariables.AcadVer;
            if (version < DxfVersion.AutoCad2000)
            {
                throw new DxfVersionNotSupportedException(string.Format("DXF file version not supported : {0}.", version), version);
            }

            this.encodedStrings = new Dictionary<string, string>();
            this.polylines = new Dictionary<string, Polyline>();
            this.imageDefReactors = new Dictionary<string, Dictionary<string, ImageDefinitionReactor>>();
            this.insertEndSequences = new Dictionary<string, EndSequence>();

            // create the default PaperSpace layout in case it does not exist. The ModelSpace layout always exists
            if (this.doc.Layouts.Count == 1)
            {
                this.doc.Layouts.Add(new Layout("Layout1"));
            }
            
            // Smoothed Polylines2d, polylines3d, and PolyfaceMesh entities are all saved as a DXF Polyline
            // Create the EndSequence object of Insert entities
            this.PreprocessEntities();

            // Create the IMAGEDEF_REACTOR objects
            this.PreprocessImageDefReactors();

            // create the application registry AcCmTransparency in case it doesn't exists, it is required by the layer transparency
            this.doc.ApplicationRegistries.Add(new ApplicationRegistry("AcCmTransparency"));

            // create the application registry AcCmTransparency in case it doesn't exists, it is required by the layer description
            this.doc.ApplicationRegistries.Add(new ApplicationRegistry("AcAecLayerStandard"));

            // create the application registry GradientColor1ACI and GradientColor2ACI in case they don't exists, they are required by the hatch gradient pattern
            this.doc.ApplicationRegistries.Add(new ApplicationRegistry("GradientColor1ACI"));
            this.doc.ApplicationRegistries.Add(new ApplicationRegistry("GradientColor2ACI"));

            // dictionaries
            List<DictionaryObject> dictionaries = new List<DictionaryObject>();

            // Named dictionary it is always the first to appear in the object section
            DictionaryObject namedObjectDictionary = new DictionaryObject(this.doc);
            this.doc.NumHandles = namedObjectDictionary.AssignHandle(this.doc.NumHandles);
            dictionaries.Add(namedObjectDictionary);

            // create the Group dictionary
            DictionaryObject groupDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.Groups.Handle};
            groupDictionary.XData.AddRange(this.doc.Groups.XData.Values);
            foreach (Group group in this.doc.Groups.Items)
            {
                groupDictionary.Entries.Add(group.Handle, group.Name);
            }
            dictionaries.Add(groupDictionary);
            namedObjectDictionary.Entries.Add(groupDictionary.Handle, DxfObjectCode.GroupDictionary);

            // Layout dictionary
            DictionaryObject layoutDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.Layouts.Handle};
            layoutDictionary.XData.AddRange(this.doc.Layouts.XData.Values);
            foreach (Layout layout in this.doc.Layouts.Items)
            {
                layoutDictionary.Entries.Add(layout.Handle, layout.Name);
            }
            dictionaries.Add(layoutDictionary);
            namedObjectDictionary.Entries.Add(layoutDictionary.Handle, DxfObjectCode.LayoutDictionary);

            // create the UnderlayDgnDefinition dictionary
            DictionaryObject dgnDefinitionDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.UnderlayDgnDefinitions.Handle};
            dgnDefinitionDictionary.XData.AddRange(this.doc.UnderlayDgnDefinitions.XData.Values);
            foreach (UnderlayDgnDefinition underlayDef in this.doc.UnderlayDgnDefinitions.Items)
            {
                dgnDefinitionDictionary.Entries.Add(underlayDef.Handle, underlayDef.Name);
            }
            dictionaries.Add(dgnDefinitionDictionary);
            namedObjectDictionary.Entries.Add(dgnDefinitionDictionary.Handle, DxfObjectCode.UnderlayDgnDefinitionDictionary);

            // create the UnderlayDwfDefinition dictionary
            DictionaryObject dwfDefinitionDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.UnderlayDwfDefinitions.Handle};
            dwfDefinitionDictionary.XData.AddRange(this.doc.UnderlayDwfDefinitions.XData.Values);
            foreach (UnderlayDwfDefinition underlayDef in this.doc.UnderlayDwfDefinitions.Items)
            {
                dwfDefinitionDictionary.Entries.Add(underlayDef.Handle, underlayDef.Name);
            }
            dictionaries.Add(dwfDefinitionDictionary);
            namedObjectDictionary.Entries.Add(dwfDefinitionDictionary.Handle, DxfObjectCode.UnderlayDwfDefinitionDictionary);

            // create the UnderlayPdfDefinition dictionary
            DictionaryObject pdfDefinitionDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.UnderlayPdfDefinitions.Handle};
            pdfDefinitionDictionary.XData.AddRange(this.doc.UnderlayPdfDefinitions.XData.Values);
            foreach (UnderlayPdfDefinition underlayDef in this.doc.UnderlayPdfDefinitions.Items)
            {
                pdfDefinitionDictionary.Entries.Add(underlayDef.Handle, underlayDef.Name);
            }
            dictionaries.Add(pdfDefinitionDictionary);
            namedObjectDictionary.Entries.Add(pdfDefinitionDictionary.Handle, DxfObjectCode.UnderlayPdfDefinitionDictionary);

            // create the MLine style dictionary
            DictionaryObject mLineStyleDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.MlineStyles.Handle};
            mLineStyleDictionary.XData.AddRange(this.doc.MlineStyles.XData.Values);
            foreach (MLineStyle mLineStyle in this.doc.MlineStyles.Items)
            {
                mLineStyleDictionary.Entries.Add(mLineStyle.Handle, mLineStyle.Name);
            }
            dictionaries.Add(mLineStyleDictionary);
            namedObjectDictionary.Entries.Add(mLineStyleDictionary.Handle, DxfObjectCode.MLineStyleDictionary);

            // create the image dictionary
            DictionaryObject imageDefDictionary = new DictionaryObject(namedObjectDictionary) {Handle = this.doc.ImageDefinitions.Handle};
            imageDefDictionary.XData.AddRange(this.doc.ImageDefinitions.XData.Values);
            foreach (ImageDefinition imageDef in this.doc.ImageDefinitions.Items)
            {
                imageDefDictionary.Entries.Add(imageDef.Handle, imageDef.Name);
            }
            dictionaries.Add(imageDefDictionary);
            namedObjectDictionary.Entries.Add(imageDefDictionary.Handle, DxfObjectCode.ImageDefDictionary);
            namedObjectDictionary.Entries.Add(this.doc.RasterVariables.Handle, DxfObjectCode.ImageVarsDictionary);

            // Layer states dictionary
            DictionaryObject layerStatesDictionary = new DictionaryObject(this.doc.Layers)
            {
                Handle = this.doc.Layers.StateManager.Handle
            };
            dictionaries.Add(layerStatesDictionary);

            DictionaryObject layerStates = new DictionaryObject(layerStatesDictionary);
            this.doc.NumHandles = layerStates.AssignHandle(this.doc.NumHandles);
            foreach (LayerState ls in this.doc.Layers.StateManager.Items)
            {
                layerStates.Entries.Add(ls.Handle, ls.Name);
            }
            dictionaries.Add(layerStates);
            layerStatesDictionary.Entries.Add(layerStates.Handle, DxfObjectCode.LayerStates);

            this.doc.DrawingVariables.HandleSeed = this.doc.NumHandles.ToString("X");

            if (this.isBinary)
            {
                this.chunk = new BinaryCodeValueWriter(new BinaryWriter(stream, new UTF8Encoding(false)));
            }
            else
            {
                this.chunk = new TextCodeValueWriter(new StreamWriter(stream, new UTF8Encoding(false)));

                // The comment group, 999, is not used in binary DXF files.
                foreach (string comment in this.doc.Comments)
                {
                    this.WriteComment(comment);
                }
            }

            //HEADER SECTION
            this.BeginSection(DxfObjectCode.HeaderSection);
            foreach (HeaderVariable variable in this.doc.DrawingVariables.KnownValues())
            {
                this.WriteSystemVariable(variable);
            }

            // write the current UCS header variables
            this.chunk.Write(9, HeaderVariableCode.UcsOrg);
            Vector3 org = this.doc.DrawingVariables.CurrentUCS.Origin;
            this.chunk.Write(10, org.X);
            this.chunk.Write(20, org.Y);
            this.chunk.Write(30, org.Z);

            this.chunk.Write(9, HeaderVariableCode.UcsXDir);
            Vector3 xdir = this.doc.DrawingVariables.CurrentUCS.XAxis;
            this.chunk.Write(10, xdir.X);
            this.chunk.Write(20, xdir.Y);
            this.chunk.Write(30, xdir.Z);

            this.chunk.Write(9, HeaderVariableCode.UcsYDir);
            Vector3 ydir = this.doc.DrawingVariables.CurrentUCS.YAxis;
            this.chunk.Write(10, ydir.X);
            this.chunk.Write(20, ydir.Y);
            this.chunk.Write(30, ydir.Z);


            // writing a copy of the active dimension style variables in the header section will avoid to be displayed as <style overrides> in AutoCAD
            if (this.doc.DimensionStyles.TryGetValue(this.doc.DrawingVariables.DimStyle, out DimensionStyle activeDimStyle))
            {
                this.WriteActiveDimensionStyleSystemVariables(activeDimStyle);
            }

            // write the custom header values
            foreach (HeaderVariable variable in this.doc.DrawingVariables.CustomValues())
            {
                if (string.Equals(variable.Name, "$ACADMAINTVER", StringComparison.InvariantCultureIgnoreCase))
                {
                    // for unknown reasons if this header variable appears in the DXF version AutoCAD2018 generated by netDxf it causes problems when loaded in AutoCAD
                    // in any case, according to the official DXF documentation it saves that this variable should be ignored
                    continue;
                }
                this.chunk.Write(9, variable.Name);
                if (variable.GroupCode == 30)
                {
                    Vector3 vector = (Vector3) variable.Value;
                    this.chunk.Write(10, vector.X);
                    this.chunk.Write(20, vector.Y);
                    this.chunk.Write(30, vector.Z);
                }
                else if (variable.GroupCode == 20)
                {
                    Vector2 vector = (Vector2) variable.Value;
                    this.chunk.Write(10, vector.X);
                    this.chunk.Write(20, vector.Y);
                }
                else
                {
                    this.chunk.Write(variable.GroupCode, variable.Value);
                }               
            }
            this.EndSection();

            //CLASSES SECTION
            this.BeginSection(DxfObjectCode.ClassesSection);

            this.WriteRasterVariablesClass(1);
            if (this.doc.ImageDefinitions.Items.Count > 0)
            {
                this.WriteImageDefClass(this.doc.ImageDefinitions.Count);
                this.WriteImageDefRectorClass(this.doc.Entities.Images.Count());
                this.WriteImageClass(this.doc.Entities.Images.Count());
            }
            this.EndSection();

            //TABLES SECTION
            this.BeginSection(DxfObjectCode.TablesSection);

            //registered application tables
            this.BeginTable(this.doc.ApplicationRegistries.CodeName, this.doc.ApplicationRegistries.Handle, (short) this.doc.ApplicationRegistries.Count, this.doc.ApplicationRegistries.XData);
            foreach (ApplicationRegistry id in this.doc.ApplicationRegistries.Items)
            {
                this.WriteApplicationRegistry(id);
            }
            this.EndTable();

            //viewport tables
            this.BeginTable(this.doc.VPorts.CodeName, this.doc.VPorts.Handle, (short) this.doc.VPorts.Count, this.doc.VPorts.XData);
            foreach (VPort vport in this.doc.VPorts)
            {
                this.WriteVPort(vport);
            }
            this.EndTable();

            //line type tables
            //The LTYPE table always precedes the LAYER table. I guess because the layers reference the line types,
            //why this same rule is not applied to DIMSTYLE tables is a mystery, since they also reference text styles and block records
            this.BeginTable(this.doc.Linetypes.CodeName, this.doc.Linetypes.Handle, (short) this.doc.Linetypes.Count, this.doc.Linetypes.XData);
            foreach (Linetype linetype in this.doc.Linetypes.Items)
            {
                this.WriteLinetype(linetype);
            }
            this.EndTable();

            //layer tables
            this.BeginTable(this.doc.Layers.CodeName, this.doc.Layers.Handle, (short) this.doc.Layers.Count, this.doc.Layers.XData);
            foreach (Layer layer in this.doc.Layers.Items)
            {
                this.WriteLayer(layer);
            }
            this.EndTable();

            //style tables text and shapes
            //the TextStyles and ShapeStyles extended data information will be combined into the DXF STYLES table
            XDataDictionary xdata = new XDataDictionary();
            xdata.AddRange(this.doc.TextStyles.XData.Values);
            xdata.AddRange(this.doc.ShapeStyles.XData.Values);
            this.BeginTable(this.doc.TextStyles.CodeName, this.doc.TextStyles.Handle, (short) this.doc.TextStyles.Count, xdata);
            foreach (TextStyle style in this.doc.TextStyles.Items)
            {
                this.WriteTextStyle(style);
            }
            foreach (ShapeStyle style in this.doc.ShapeStyles)
            {
                this.WriteShapeStyle(style);
            }
            this.EndTable();

            //dimension style tables
            this.BeginTable(this.doc.DimensionStyles.CodeName, this.doc.DimensionStyles.Handle, (short) this.doc.DimensionStyles.Count, this.doc.DimensionStyles.XData);
            foreach (DimensionStyle style in this.doc.DimensionStyles.Items)
            {
                this.WriteDimensionStyle(style);
            }
            this.EndTable();

            //view
            this.BeginTable(this.doc.Views.CodeName, this.doc.Views.Handle, (short) this.doc.Views.Count, this.doc.Views.XData);
            this.EndTable();

            //UCS
            this.BeginTable(this.doc.UCSs.CodeName, this.doc.UCSs.Handle, (short) this.doc.UCSs.Count, this.doc.Blocks.XData);
            foreach (UCS ucs in this.doc.UCSs.Items)
            {
                this.WriteUCS(ucs);
            }
            this.EndTable();

            //block record table
            this.BeginTable(this.doc.Blocks.CodeName, this.doc.Blocks.Handle, (short) this.doc.Blocks.Count, this.doc.Blocks.XData);
            foreach (Block block in this.doc.Blocks.Items)
            {
                this.WriteBlockRecord(block.Record);
            }
            this.EndTable();

            this.EndSection(); //End section tables

            //BLOCKS SECTION
            this.BeginSection(DxfObjectCode.BlocksSection);
            foreach (Block block in this.doc.Blocks.Items)
            {
                this.WriteBlock(block);
            }
            this.EndSection(); //End section blocks

            //ENTITIES SECTION
            this.BeginSection(DxfObjectCode.EntitiesSection);

            foreach (Layout layout in this.doc.Layouts)
            {
                if (layout.IsPaperSpace)
                {
                    // only the entities of the layout associated with the block "*Paper_Space" are included in the Entities Section
                    string index = layout.AssociatedBlock.Name.Remove(0, 12);
                    if (string.IsNullOrEmpty(index))
                    {
                        // write the layout viewport with ID=1, only for PaperSpace layouts
                        this.WriteEntity(layout.Viewport, layout);

                        foreach (AttributeDefinition attDef in layout.AssociatedBlock.AttributeDefinitions.Values)
                        {
                            this.WriteAttributeDefinition(attDef, layout);
                        }

                        foreach (EntityObject entity in layout.AssociatedBlock.Entities)
                        {
                            this.WriteEntity(entity, layout);
                        }
                    }                  
                }
                else 
                {
                    // ModelSpace
                    foreach (AttributeDefinition attDef in layout.AssociatedBlock.AttributeDefinitions.Values)
                    {
                        this.WriteAttributeDefinition(attDef, layout);
                    }

                    foreach (EntityObject entity in layout.AssociatedBlock.Entities)
                    {
                        this.WriteEntity(entity, layout);
                    }
                }
            }
            this.EndSection(); //End section entities

            //OBJECTS SECTION
            this.BeginSection(DxfObjectCode.ObjectsSection);

            foreach (DictionaryObject dictionary in dictionaries)
            {
                this.WriteDictionary(dictionary);
            }

            foreach (Group group in this.doc.Groups.Items)
            {
                this.WriteGroup(group, groupDictionary.Handle);
            }

            foreach (Layout layout in this.doc.Layouts)
            {
                this.WriteLayout(layout, layoutDictionary.Handle);
            }

            foreach (MLineStyle style in this.doc.MlineStyles.Items)
            {
                this.WriteMLineStyle(style, mLineStyleDictionary.Handle);
            }

            foreach (UnderlayDgnDefinition underlayDef in this.doc.UnderlayDgnDefinitions.Items)
            {
                this.WriteUnderlayDefinition(underlayDef, dgnDefinitionDictionary.Handle);
            }

            foreach (UnderlayDwfDefinition underlayDef in this.doc.UnderlayDwfDefinitions.Items)
            {
                this.WriteUnderlayDefinition(underlayDef, dwfDefinitionDictionary.Handle);
            }

            foreach (UnderlayPdfDefinition underlayDef in this.doc.UnderlayPdfDefinitions.Items)
            {
                this.WriteUnderlayDefinition(underlayDef, pdfDefinitionDictionary.Handle);
            }

            this.WriteRasterVariables(this.doc.RasterVariables, imageDefDictionary.Handle);
            foreach (ImageDefinition imageDef in this.doc.ImageDefinitions.Items)
            {
                foreach (ImageDefinitionReactor reactor in this.imageDefReactors[imageDef.Handle].Values)
                {
                    this.WriteImageDefReactor(reactor);
                }
                this.WriteImageDef(imageDef, imageDefDictionary.Handle);
            }

            foreach (LayerState layerState in this.doc.Layers.StateManager.Items)
            {
                this.WriteLayerState(layerState, layerStates.Handle);
            }

            this.EndSection(); //End section objects

            this.Close();

        }

        #endregion

        #region private methods
        
        /// <summary>
        /// Closes the DXF writer.
        /// </summary>
        private void Close()
        {
            this.chunk.Write(0, DxfObjectCode.EndOfFile);
            this.chunk.Flush();
        }

        /// <summary>
        /// Opens a new section.
        /// </summary>
        /// <param name="section">Section type to open.</param>
        /// <remarks>There can be only one type section.</remarks>
        private void BeginSection(string section)
        {
            Debug.Assert(this.activeSection == DxfObjectCode.Unknown);

            this.chunk.Write(0, DxfObjectCode.BeginSection);
            this.chunk.Write(2, section);
            this.activeSection = section;
        }

        /// <summary>
        /// Closes the active section.
        /// </summary>
        private void EndSection()
        {
            Debug.Assert(this.activeSection != DxfObjectCode.Unknown);

            this.chunk.Write(0, DxfObjectCode.EndSection);
            this.activeSection = DxfObjectCode.Unknown;
        }

        /// <summary>
        /// Opens a new table.
        /// </summary>
        /// <param name="table">Table type to open.</param>
        /// <param name="handle">Handle assigned to this table</param>
        /// <param name="numEntries">Number of entries in the table (obsolete).</param>
        /// <param name="xdata">Extended data information of the table.</param>
        private void BeginTable(string table, string handle, short numEntries, XDataDictionary xdata)
        {
            Debug.Assert(this.activeSection == DxfObjectCode.TablesSection);

            this.chunk.Write(0, DxfObjectCode.Table);
            this.chunk.Write(2, table);
            this.chunk.Write(5, handle);
            if (table == DxfObjectCode.Layer)
            {
                this.chunk.Write(102, "{ACAD_XDICTIONARY");
                this.chunk.Write(360, this.doc.Layers.StateManager.Handle);
                this.chunk.Write(102, "}");
            }
            this.chunk.Write(330, "0");

            this.chunk.Write(100, SubclassMarker.Table);
            this.chunk.Write(70, numEntries); // this code is obsolete, there is no limit for the number of entries except for layouts according to the AutoCad ActiveX documentation

            if (table == DxfObjectCode.DimensionStyleTable)
            {
                this.chunk.Write(100, SubclassMarker.DimensionStyleTable);
            }

            this.WriteXData(xdata);

            this.activeTable = table;
        }

        /// <summary>
        /// Closes the active table.
        /// </summary>
        private void EndTable()
        {
            Debug.Assert(this.activeSection != DxfObjectCode.Unknown);

            this.chunk.Write(0, DxfObjectCode.EndTable);
            this.activeTable = DxfObjectCode.Unknown;
        }

        #endregion

        #region methods for Header section

        private void WriteComment(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
            {
                this.chunk.Write(999, comment);
            }
        }

        private void WriteSystemVariable(HeaderVariable variable)
        {
            Debug.Assert(this.activeSection == DxfObjectCode.HeaderSection);

            string name = variable.Name;
            object value = variable.Value;

            switch (name)
            {
                case HeaderVariableCode.AcadVer:
                    this.chunk.Write(9, name);
                    this.chunk.Write(1, StringEnum<DxfVersion>.GetStringValue((DxfVersion) value));
                    break;
                case HeaderVariableCode.HandleSeed:
                    this.chunk.Write(9, name);
                    this.chunk.Write(5, value);
                    break;
                case HeaderVariableCode.Angbase:
                    this.chunk.Write(9, name);
                    this.chunk.Write(50, value);
                    break;
                case HeaderVariableCode.Angdir:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (AngleDirection) value);
                    break;
                case HeaderVariableCode.AttMode:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (AttMode) value);
                    break;
                case HeaderVariableCode.AUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (AngleUnitType) value);
                    break;
                case HeaderVariableCode.AUprec:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.CeColor:
                    this.chunk.Write(9, name);
                    this.chunk.Write(62, ((AciColor) value).Index);
                    break;
                case HeaderVariableCode.CeLtScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, value);
                    break;
                case HeaderVariableCode.CeLtype:
                    this.chunk.Write(9, name);
                    this.chunk.Write(6, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.CeLweight:
                    this.chunk.Write(9, name);
                    this.chunk.Write(370, (short) (Lineweight) value);
                    break;
                case HeaderVariableCode.CLayer:
                    this.chunk.Write(9, name);
                    this.chunk.Write(8, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.CMLJust:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (MLineJustification) value);
                    break;
                case HeaderVariableCode.CMLScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, value);
                    break;
                case HeaderVariableCode.CMLStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(2, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.DimStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(2, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.TextSize:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, value);
                    break;
                case HeaderVariableCode.TextStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(7, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.LastSavedBy:
                    if (this.doc.DrawingVariables.AcadVer <= DxfVersion.AutoCad2000)
                    {
                        break;
                    }
                    this.chunk.Write(9, name);
                    this.chunk.Write(1, this.EncodeNonAsciiCharacters((string) value));
                    break;
                case HeaderVariableCode.LUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (LinearUnitType) value);
                    break;
                case HeaderVariableCode.LUprec:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.DwgCodePage:
                    this.chunk.Write(9, name);
                    this.chunk.Write(3, value);
                    break;
                case HeaderVariableCode.Extnames:
                    this.chunk.Write(9, name);
                    this.chunk.Write(290, value);
                    break;
                case HeaderVariableCode.InsBase:
                    this.chunk.Write(9, name);
                    Vector3 pos = (Vector3) value;
                    this.chunk.Write(10, pos.X);
                    this.chunk.Write(20, pos.Y);
                    this.chunk.Write(30, pos.Z);
                    break;
                case HeaderVariableCode.InsUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (DrawingUnits) value);
                    break;
                case HeaderVariableCode.LtScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, value);
                    break;
                case HeaderVariableCode.LwDisplay:
                    this.chunk.Write(9, name);
                    this.chunk.Write(290, value);
                    break;
                case HeaderVariableCode.MirrText:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (bool) value ? (short) 1 : (short) 0);
                    break;
                case HeaderVariableCode.PdMode:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, (short) (PointShape) value);
                    break;
                case HeaderVariableCode.PdSize:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, value);
                    break;
                case HeaderVariableCode.PLineGen:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.PsLtScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.SplineSegs:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.SurfU:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.SurfV:
                    this.chunk.Write(9, name);
                    this.chunk.Write(70, value);
                    break;
                case HeaderVariableCode.TdCreate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TduCreate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TdUpdate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TduUpdate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TdinDwg:
                    this.chunk.Write(9, name);
                    this.chunk.Write(40, ((TimeSpan) value).TotalDays);
                    break;
            }
        }

        private void WriteActiveDimensionStyleSystemVariables(DimensionStyle style)
        {
            this.chunk.Write(9, "$DIMADEC");
            this.chunk.Write(70, style.AngularPrecision);

            this.chunk.Write(9, "$DIMALT");
            this.chunk.Write(70, style.AlternateUnits.Enabled ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMALTD");
            this.chunk.Write(70, style.AlternateUnits.LengthPrecision);

            this.chunk.Write(9, "$DIMALTF");
            this.chunk.Write(40, style.AlternateUnits.Multiplier);

            this.chunk.Write(9, "$DIMALTRND");
            this.chunk.Write(40, style.AlternateUnits.Roundoff);

            this.chunk.Write(9, "$DIMALTTD");
            this.chunk.Write(70, style.Tolerances.AlternatePrecision);

            this.chunk.Write(9, "$DIMALTTZ");
            this.chunk.Write(70, GetSuppressZeroesValue(
                    style.Tolerances.AlternateSuppressLinearLeadingZeros,
                    style.Tolerances.AlternateSuppressLinearTrailingZeros,
                    style.Tolerances.AlternateSuppressZeroFeet,
                    style.Tolerances.AlternateSuppressZeroInches));

            this.chunk.Write(9, "$DIMALTU");
            switch (style.AlternateUnits.LengthUnits)
            {
                case LinearUnitType.Scientific:
                    this.chunk.Write(70, (short) 1);
                    break;
                case LinearUnitType.Decimal:
                    this.chunk.Write(70, (short) 2);
                    break;
                case LinearUnitType.Engineering:
                    this.chunk.Write(70, (short) 3);
                    break;
                case LinearUnitType.Architectural:
                    this.chunk.Write(70, style.AlternateUnits.StackUnits ? (short) 4 : (short) 6);
                    break;
                case LinearUnitType.Fractional:
                    this.chunk.Write(70, style.AlternateUnits.StackUnits ? (short) 5 : (short) 7);
                    break;
            }

            this.chunk.Write(9, "$DIMALTZ");
            this.chunk.Write(70, GetSuppressZeroesValue(
                    style.AlternateUnits.SuppressLinearLeadingZeros,
                    style.AlternateUnits.SuppressLinearTrailingZeros,
                    style.AlternateUnits.SuppressZeroFeet,
                    style.AlternateUnits.SuppressZeroInches));

            this.chunk.Write(9, "$DIMAPOST");
            string altUnits = string.IsNullOrEmpty(style.DimPrefix) ? "" : "[]";
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.AlternateUnits.Prefix + altUnits + style.AlternateUnits.Suffix));

            this.chunk.Write(9, "$DIMATFIT");
            this.chunk.Write(70, (short) style.FitOptions);

            this.chunk.Write(9, "$DIMAUNIT");
            this.chunk.Write(70, (short) style.DimAngularUnits);

            this.chunk.Write(9, "$DIMASZ");
            this.chunk.Write(40, style.ArrowSize);

            short angSupress;
            if (style.SuppressAngularLeadingZeros && style.SuppressAngularTrailingZeros)
            {
                angSupress = 3;
            }
            else if (!style.SuppressAngularLeadingZeros && !style.SuppressAngularTrailingZeros)
            {
                angSupress = 0;
            }
            else if (!style.SuppressAngularLeadingZeros && style.SuppressAngularTrailingZeros)
            {
                angSupress = 2;
            }
            else if (style.SuppressAngularLeadingZeros && !style.SuppressAngularTrailingZeros)
            {
                angSupress = 1;
            }
            else
            {
                angSupress = 3;
            }

            this.chunk.Write(9, "$DIMAZIN");
            this.chunk.Write(70, angSupress);

            if (style.DimArrow1 == null && style.DimArrow2 == null)
            {
                this.chunk.Write(9, "$DIMSAH");
                this.chunk.Write(70, (short) 0);

                this.chunk.Write(9, "$DIMBLK");
                this.chunk.Write(1, "");
            }
            else if (style.DimArrow1 == null)
            {
                this.chunk.Write(9, "$DIMSAH");
                this.chunk.Write(70, (short) 1);

                this.chunk.Write(9, "$DIMBLK1");
                this.chunk.Write(1, "");

                this.chunk.Write(9, "$DIMBLK2");
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimArrow2.Name));
            }
            else if (style.DimArrow2 == null)
            {
                this.chunk.Write(9, "$DIMSAH");
                this.chunk.Write(70, (short) 1);

                this.chunk.Write(9, "$DIMBLK1");
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimArrow1.Name));

                this.chunk.Write(9, "$DIMBLK2");
                this.chunk.Write(1, "");
            }
            else if (string.Equals(style.DimArrow1.Name, style.DimArrow2.Name, StringComparison.OrdinalIgnoreCase))
            {
                this.chunk.Write(9, "$DIMSAH");
                this.chunk.Write(70, (short) 0);

                this.chunk.Write(9, "$DIMBLK");
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimArrow1.Name));
            }
            else
            {
                this.chunk.Write(9, "$DIMSAH");
                this.chunk.Write(70, (short) 1);

                this.chunk.Write(9, "$DIMBLK1");
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimArrow1.Name));

                this.chunk.Write(9, "$DIMBLK2");
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimArrow2.Name));
            }

            this.chunk.Write(9, "$DIMLDRBLK");
            this.chunk.Write(1, style.LeaderArrow == null ? "" : this.EncodeNonAsciiCharacters(style.LeaderArrow.Name));

            this.chunk.Write(9, "$DIMCEN");
            this.chunk.Write(40, style.CenterMarkSize);

            this.chunk.Write(9, "$DIMCLRD");
            this.chunk.Write(70, style.DimLineColor.Index);

            this.chunk.Write(9, "$DIMCLRE");
            this.chunk.Write(70, style.ExtLineColor.Index);

            this.chunk.Write(9, "$DIMCLRT");
            this.chunk.Write(70, style.TextColor.Index);

            this.chunk.Write(9, "$DIMDEC");
            this.chunk.Write(70, style.LengthPrecision);

            this.chunk.Write(9, "$DIMDLE");
            this.chunk.Write(40, style.DimLineExtend);

            this.chunk.Write(9, "$DIMDLI");
            this.chunk.Write(40, style.DimBaselineSpacing);

            this.chunk.Write(9, "$DIMDSEP");
            this.chunk.Write(70, (short) style.DecimalSeparator);

            this.chunk.Write(9, "$DIMEXE");
            this.chunk.Write(40, style.ExtLineExtend);

            this.chunk.Write(9, "$DIMEXO");
            this.chunk.Write(40, style.ExtLineOffset);

            this.chunk.Write(9, "$DIMFXLON");
            this.chunk.Write(70, style.ExtLineFixed ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMFXL");
            this.chunk.Write(40, style.ExtLineFixedLength);

            this.chunk.Write(9, "$DIMGAP");
            this.chunk.Write(40, style.TextOffset);

            this.chunk.Write(9, "$DIMJUST");
            this.chunk.Write(70, (short) style.TextHorizontalPlacement);

            this.chunk.Write(9, "$DIMLFAC");
            this.chunk.Write(40, style.DimScaleLinear);

            this.chunk.Write(9, "$DIMLUNIT");
            this.chunk.Write(70, (short) style.DimLengthUnits);

            this.chunk.Write(9, "$DIMLWD");
            this.chunk.Write(70, (short) style.DimLineLineweight);

            this.chunk.Write(9, "$DIMLWE");
            this.chunk.Write(70, (short) style.ExtLineLineweight);

            this.chunk.Write(9, "$DIMPOST");
            string units = string.IsNullOrEmpty(style.DimPrefix) ? "" : "<>";
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(style.DimPrefix + units + style.DimSuffix));

            this.chunk.Write(9, "$DIMRND");
            this.chunk.Write(40, style.DimRoundoff);

            this.chunk.Write(9, "$DIMSCALE");
            this.chunk.Write(40, style.DimScaleOverall);

            this.chunk.Write(9, "$DIMSD1");
            this.chunk.Write(70, style.DimLine1Off ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMSD2");
            this.chunk.Write(70, style.DimLine2Off ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMSE1");
            this.chunk.Write(70, style.ExtLine1Off ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMSE2");
            this.chunk.Write(70, style.ExtLine2Off ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMSOXD");
            this.chunk.Write(70, style.FitDimLineInside ? (short) 0 : (short) 1);

            this.chunk.Write(9, "$DIMTAD");
            this.chunk.Write(70, (short) style.TextVerticalPlacement);

            this.chunk.Write(9, "$DIMTDEC");
            this.chunk.Write(70, style.Tolerances.Precision);

            this.chunk.Write(9, "$DIMTFAC");
            this.chunk.Write(40, style.TextFractionHeightScale);

            if (style.TextFillColor != null)
            {
                this.chunk.Write(9, "$DIMTFILL");
                this.chunk.Write(70, (short) 2);

                this.chunk.Write(9, "$DIMTFILLCLR");
                this.chunk.Write(70, style.TextFillColor.Index);
            }

            this.chunk.Write(9, "$DIMTIH");
            this.chunk.Write(70, style.TextInsideAlign ? (short) 1 : (short) 0);

            this.chunk.Write(9, "$DIMTIX");
            this.chunk.Write(70, style.FitTextInside ? (short)1 : (short)0);

            if (style.Tolerances.DisplayMethod == DimensionStyleTolerancesDisplayMethod.Deviation)
            {
                this.chunk.Write(9, "$DIMTM");
                this.chunk.Write(40, MathHelper.IsZero(style.Tolerances.LowerLimit) ? MathHelper.Epsilon : style.Tolerances.LowerLimit);
            }
            else
            {
                this.chunk.Write(9, "$DIMTM");
                this.chunk.Write(40, style.Tolerances.LowerLimit);
            }

            this.chunk.Write(9, "$DIMTMOVE");
            this.chunk.Write(70, (short) style.FitTextMove);

            this.chunk.Write(9, "$DIMTOFL");
            this.chunk.Write(70, style.FitDimLineForce ? (short)1 : (short)0);

            this.chunk.Write(9, "$DIMTOH");
            this.chunk.Write(70, style.TextOutsideAlign ? (short) 1 : (short) 0);

            switch (style.Tolerances.DisplayMethod)
            {
                case DimensionStyleTolerancesDisplayMethod.None:
                    this.chunk.Write(9, "$DIMTOL");
                    this.chunk.Write(70, (short) 0);
                    this.chunk.Write(9, "$DIMLIM");
                    this.chunk.Write(70, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Symmetrical:
                    this.chunk.Write(9, "$DIMTOL");
                    this.chunk.Write(70, (short) 1);
                    this.chunk.Write(9, "$DIMLIM");
                    this.chunk.Write(70, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Deviation:
                    this.chunk.Write(9, "$DIMTOL");
                    this.chunk.Write(70, (short) 1);
                    this.chunk.Write(9, "$DIMLIM");
                    this.chunk.Write(70, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Limits:
                    this.chunk.Write(9, "$DIMTOL");
                    this.chunk.Write(70, (short) 0);
                    this.chunk.Write(9, "$DIMLIM");
                    this.chunk.Write(70, (short) 1);
                    break;
            }

            this.chunk.Write(9, "$DIMTOLJ");
            this.chunk.Write(70, (short) style.Tolerances.VerticalPlacement);

            this.chunk.Write(9, "$DIMTP");
            this.chunk.Write(40, style.Tolerances.UpperLimit);

            this.chunk.Write(9, "$DIMTXT");
            this.chunk.Write(40, style.TextHeight);

            this.chunk.Write(9, "$DIMTXTDIRECTION");
            this.chunk.Write(70, (short) style.TextDirection);

            this.chunk.Write(9, "$DIMTZIN");
            this.chunk.Write(70, GetSuppressZeroesValue(
                    style.Tolerances.SuppressLinearLeadingZeros,
                    style.Tolerances.SuppressLinearTrailingZeros,
                    style.Tolerances.SuppressZeroFeet,
                    style.Tolerances.SuppressZeroInches));

            this.chunk.Write(9, "$DIMZIN");
            this.chunk.Write(70, GetSuppressZeroesValue(
                    style.SuppressLinearLeadingZeros,
                    style.SuppressLinearTrailingZeros,
                    style.SuppressZeroFeet,
                    style.SuppressZeroInches));

            // CAUTION: The next four codes are not documented in the official DXF docs
            this.chunk.Write(9, "$DIMFRAC");
            this.chunk.Write(70, (short) style.FractionType);

            this.chunk.Write(9, "$DIMLTYPE");
            this.chunk.Write(6, this.EncodeNonAsciiCharacters(style.DimLineLinetype.Name));

            this.chunk.Write(9, "$DIMLTEX1");
            this.chunk.Write(6, this.EncodeNonAsciiCharacters(style.ExtLine1Linetype.Name));

            this.chunk.Write(9, "$DIMLTEX2");
            this.chunk.Write(6, this.EncodeNonAsciiCharacters(style.ExtLine2Linetype.Name));
    }

        #endregion

        #region methods for Classes section

        private void WriteImageClass(int count)
        {
            this.chunk.Write(0, DxfObjectCode.Class);
            this.chunk.Write(1, DxfObjectCode.Image);
            this.chunk.Write(2, SubclassMarker.RasterImage);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the DXF documentation
            this.chunk.Write(90, 127);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
            {
                this.chunk.Write(91, count);
            }

            this.chunk.Write(280, (short) 0);
            this.chunk.Write(281, (short) 1);
        }
        
        private void WriteImageDefClass(int count)
        {
            this.chunk.Write(0, DxfObjectCode.Class);
            this.chunk.Write(1, DxfObjectCode.ImageDef);
            this.chunk.Write(2, SubclassMarker.RasterImageDef);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the DXF documentation
            this.chunk.Write(90, 0);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
            {
                this.chunk.Write(91, count);
            }

            this.chunk.Write(280, (short) 0);
            this.chunk.Write(281, (short) 0);
        }

        private void WriteImageDefRectorClass(int count)
        {
            this.chunk.Write(0, DxfObjectCode.Class);
            this.chunk.Write(1, DxfObjectCode.ImageDefReactor);
            this.chunk.Write(2, SubclassMarker.RasterImageDefReactor);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the DXF documentation
            this.chunk.Write(90, 1);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
            {
                this.chunk.Write(91, count);
            }

            this.chunk.Write(280, (short) 0);
            this.chunk.Write(281, (short) 0);
        }

        private void WriteRasterVariablesClass(int count)
        {
            this.chunk.Write(0, DxfObjectCode.Class);
            this.chunk.Write(1, DxfObjectCode.RasterVariables);
            this.chunk.Write(2, SubclassMarker.RasterVariables);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the DXF documentation
            this.chunk.Write(90, 0);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
            {
                this.chunk.Write(91, count);
            }

            this.chunk.Write(280, (short) 0);
            this.chunk.Write(281, (short) 0);
        }

        #endregion

        #region methods for Table section

        /// <summary>
        /// Writes a new extended data application registry to the table section.
        /// </summary>
        /// <param name="appReg">Name of the application registry.</param>
        private void WriteApplicationRegistry(ApplicationRegistry appReg)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.ApplicationIdTable);

            this.chunk.Write(0, DxfObjectCode.ApplicationIdTable);
            this.chunk.Write(5, appReg.Handle);
            this.chunk.Write(330, appReg.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);
            this.chunk.Write(100, SubclassMarker.ApplicationId);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(appReg.Name));

            this.chunk.Write(70, (short) 0);

            this.WriteXData(appReg.XData);
        }

        /// <summary>
        /// Writes a new viewport to the table section.
        /// </summary>
        /// <param name="vp">viewport.</param>
        private void WriteVPort(VPort vp)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.VportTable);

            this.chunk.Write(0, vp.CodeName);
            this.chunk.Write(5, vp.Handle);
            this.chunk.Write(330, vp.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.VPort);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(vp.Name));

            this.chunk.Write(70, (short) 0);

            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);

            this.chunk.Write(11, 1.0);
            this.chunk.Write(21, 1.0);

            this.chunk.Write(12, vp.ViewCenter.X);
            this.chunk.Write(22, vp.ViewCenter.Y);

            this.chunk.Write(13, vp.SnapBasePoint.X);
            this.chunk.Write(23, vp.SnapBasePoint.Y);

            this.chunk.Write(14, vp.SnapSpacing.X);
            this.chunk.Write(24, vp.SnapSpacing.Y);

            this.chunk.Write(15, vp.GridSpacing.X);
            this.chunk.Write(25, vp.GridSpacing.Y);

            this.chunk.Write(16, vp.ViewDirection.X);
            this.chunk.Write(26, vp.ViewDirection.Y);
            this.chunk.Write(36, vp.ViewDirection.Z);

            this.chunk.Write(17, vp.ViewTarget.X);
            this.chunk.Write(27, vp.ViewTarget.Y);
            this.chunk.Write(37, vp.ViewTarget.Z);

            this.chunk.Write(40, vp.ViewHeight);
            this.chunk.Write(41, vp.ViewAspectRatio);

            this.chunk.Write(75, vp.SnapMode ? (short) 1 : (short) 0);
            this.chunk.Write(76, vp.ShowGrid ? (short) 1 : (short) 0);

            this.WriteXData(vp.XData);
        }

        /// <summary>
        /// Writes a new dimension style to the table section.
        /// </summary>
        /// <param name="style">DimensionStyle.</param>
        private void WriteDimensionStyle(DimensionStyle style)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.DimensionStyleTable);

            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(105, style.Handle);
            this.chunk.Write(330, style.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.DimensionStyle);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(style.Name));

            string units = string.IsNullOrEmpty(style.DimPrefix) ? "" : "<>";
            this.chunk.Write(3, this.EncodeNonAsciiCharacters(style.DimPrefix + units + style.DimSuffix));

            string altUnits = string.IsNullOrEmpty(style.DimPrefix) ? "" : "[]";
            this.chunk.Write(4, this.EncodeNonAsciiCharacters(style.AlternateUnits.Prefix + altUnits + style.AlternateUnits.Suffix));

            this.chunk.Write(40, style.DimScaleOverall);
            this.chunk.Write(41, style.ArrowSize);
            this.chunk.Write(42, style.ExtLineOffset);
            this.chunk.Write(43, style.DimBaselineSpacing);
            this.chunk.Write(44, style.ExtLineExtend);
            this.chunk.Write(45, style.DimRoundoff);
            this.chunk.Write(46, style.DimLineExtend);
            this.chunk.Write(47, style.Tolerances.UpperLimit);
            this.chunk.Write(48, style.Tolerances.LowerLimit);
            this.chunk.Write(49, style.ExtLineFixedLength);

            if (style.TextFillColor != null)
            {
                this.chunk.Write(69, (short) 2);
                this.chunk.Write(70, style.TextFillColor.Index);
            }
            else
            {
                this.chunk.Write(70, (short) 0);
            }

            switch (style.Tolerances.DisplayMethod)
            {
                case DimensionStyleTolerancesDisplayMethod.None:
                    this.chunk.Write(71, (short) 0);
                    this.chunk.Write(72, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Symmetrical:
                    this.chunk.Write(71, (short) 1);
                    this.chunk.Write(72, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Deviation:
                    this.chunk.Write(71, (short) 1);
                    this.chunk.Write(72, (short) 0);
                    break;
                case DimensionStyleTolerancesDisplayMethod.Limits:
                    this.chunk.Write(71, (short) 0);
                    this.chunk.Write(72, (short) 1);
                    break;
            }

            this.chunk.Write(73, style.TextInsideAlign ? (short) 1 : (short) 0);
            this.chunk.Write(74, style.TextOutsideAlign ? (short) 1 : (short) 0);
            this.chunk.Write(75, style.ExtLine1Off ? (short) 1 : (short) 0);
            this.chunk.Write(76, style.ExtLine2Off ? (short) 1 : (short) 0);

            this.chunk.Write(77, (short) style.TextVerticalPlacement);
            this.chunk.Write(78, GetSuppressZeroesValue(
                    style.SuppressLinearLeadingZeros,
                    style.SuppressLinearTrailingZeros,
                    style.SuppressZeroFeet,
                    style.SuppressZeroInches));

            short angSupress = 3;
            if (style.SuppressAngularLeadingZeros && style.SuppressAngularTrailingZeros)
            {
                angSupress = 3;
            }
            else if (!style.SuppressAngularLeadingZeros && !style.SuppressAngularTrailingZeros)
            {
                angSupress = 0;
            }
            else if (!style.SuppressAngularLeadingZeros && style.SuppressAngularTrailingZeros)
            {
                angSupress = 2;
            }
            else if (style.SuppressAngularLeadingZeros && !style.SuppressAngularTrailingZeros)
            {
                angSupress = 1;
            }

            this.chunk.Write(79, angSupress);

            this.chunk.Write(140, style.TextHeight);
            this.chunk.Write(141, style.CenterMarkSize);
            this.chunk.Write(143, style.AlternateUnits.Multiplier);
            this.chunk.Write(144, style.DimScaleLinear);
            this.chunk.Write(146, style.TextFractionHeightScale);
            this.chunk.Write(147, style.TextOffset);
            this.chunk.Write(148, style.AlternateUnits.Roundoff);
            this.chunk.Write(170, style.AlternateUnits.Enabled ? (short) 1 : (short) 0);
            this.chunk.Write(171, style.AlternateUnits.LengthPrecision);
            this.chunk.Write(172, style.FitDimLineForce ? (short) 1 : (short) 0);
            // code 173 is written later
            this.chunk.Write(174, style.FitTextInside ? (short) 1 : (short) 0);
            this.chunk.Write(175, style.FitDimLineInside ? (short) 0 : (short) 1);
            this.chunk.Write(176, style.DimLineColor.Index);
            this.chunk.Write(177, style.ExtLineColor.Index);
            this.chunk.Write(178, style.TextColor.Index);
            this.chunk.Write(179, style.AngularPrecision);
            this.chunk.Write(271, style.LengthPrecision);
            this.chunk.Write(272, style.Tolerances.Precision);
            switch (style.AlternateUnits.LengthUnits)
            {
                case LinearUnitType.Scientific:
                    this.chunk.Write(273, (short) 1);
                    break;
                case LinearUnitType.Decimal:
                    this.chunk.Write(273, (short) 2);
                    break;
                case LinearUnitType.Engineering:
                    this.chunk.Write(273, (short) 3);
                    break;
                case LinearUnitType.Architectural:
                    this.chunk.Write(273, style.AlternateUnits.StackUnits ? (short) 4 : (short) 6);
                    break;
                case LinearUnitType.Fractional:
                    this.chunk.Write(273, style.AlternateUnits.StackUnits ? (short) 5 : (short) 7);
                    break;
            }       
            this.chunk.Write(274, style.Tolerances.AlternatePrecision);              
            this.chunk.Write(275, (short) style.DimAngularUnits);
            this.chunk.Write(276, (short) style.FractionType);
            this.chunk.Write(277, (short) style.DimLengthUnits);
            this.chunk.Write(278, (short) style.DecimalSeparator);
            this.chunk.Write(279, (short) style.FitTextMove);
            this.chunk.Write(280, (short) style.TextHorizontalPlacement);
            this.chunk.Write(281, style.DimLine1Off ? (short) 1 : (short) 0);
            this.chunk.Write(282, style.DimLine2Off ? (short) 1 : (short) 0);
            this.chunk.Write(283, (short) style.Tolerances.VerticalPlacement);
            this.chunk.Write(284, GetSuppressZeroesValue(
                    style.Tolerances.SuppressLinearLeadingZeros,
                    style.Tolerances.SuppressLinearTrailingZeros,
                    style.Tolerances.SuppressZeroFeet,
                    style.Tolerances.SuppressZeroInches));
            this.chunk.Write(285, GetSuppressZeroesValue(
                    style.AlternateUnits.SuppressLinearLeadingZeros,
                    style.AlternateUnits.SuppressLinearTrailingZeros,
                    style.AlternateUnits.SuppressZeroFeet,
                    style.AlternateUnits.SuppressZeroInches));
            this.chunk.Write(286, GetSuppressZeroesValue(
                    style.Tolerances.AlternateSuppressLinearLeadingZeros,
                    style.Tolerances.AlternateSuppressLinearTrailingZeros,
                    style.Tolerances.AlternateSuppressZeroFeet,
                    style.Tolerances.AlternateSuppressZeroInches));
            this.chunk.Write(289, (short) style.FitOptions);
            this.chunk.Write(290, style.ExtLineFixed);
            this.chunk.Write(294, style.TextDirection == DimensionStyleTextDirection.RightToLeft);
            this.chunk.Write(340, style.TextStyle.Handle);

            // CAUTION: The documentation says that the next values are the handles of referenced BLOCK,
            // but they are the handles of referenced BLOCK_RECORD
            if (style.LeaderArrow != null)
            {
                this.chunk.Write(341, style.LeaderArrow.Record.Handle);
            }

            if (style.DimArrow1 == null && style.DimArrow2 == null)
            {
                this.chunk.Write(173, (short) 0);
            }
            else if (style.DimArrow1 == null)
            {
                this.chunk.Write(173, (short) 1);
                if (style.DimArrow2 != null)
                {
                    this.chunk.Write(344, style.DimArrow2.Record.Handle);
                }
            }
            else if (style.DimArrow2 == null)
            {
                this.chunk.Write(173, (short) 1);
                if (style.DimArrow1 != null)
                {
                    this.chunk.Write(343, style.DimArrow1.Record.Handle);
                }
            }
            else if (string.Equals(style.DimArrow1.Name, style.DimArrow2.Name, StringComparison.OrdinalIgnoreCase))
            {
                this.chunk.Write(173, (short) 0);
                this.chunk.Write(342, style.DimArrow1.Record.Handle);
            }
            else
            {
                this.chunk.Write(173, (short) 1);
                this.chunk.Write(343, style.DimArrow1.Record.Handle);
                this.chunk.Write(344, style.DimArrow2.Record.Handle);
            }

            // CAUTION: The next three codes are undocumented in the official DXF docs
            this.chunk.Write(345, style.DimLineLinetype.Handle);
            this.chunk.Write(346, style.ExtLine1Linetype.Handle);
            this.chunk.Write(347, style.ExtLine2Linetype.Handle);

            this.chunk.Write(371, (short) style.DimLineLineweight);
            this.chunk.Write(372, (short) style.ExtLineLineweight);

            this.WriteXData(style.XData);
        }      

        /// <summary>
        /// Writes a new block record to the table section.
        /// </summary>
        /// <param name="blockRecord">Block.</param>
        private void WriteBlockRecord(BlockRecord blockRecord)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.BlockRecordTable);

            this.chunk.Write(0, blockRecord.CodeName);
            this.chunk.Write(5, blockRecord.Handle);
            this.chunk.Write(330, blockRecord.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.BlockRecord);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(blockRecord.Name));

            // Hard-pointer ID/handle to associated LAYOUT object
            this.chunk.Write(340, blockRecord.Layout == null ? "0" : blockRecord.Layout.Handle);

            // internal blocks do not need more information
            if (blockRecord.IsForInternalUseOnly)
            {
                return;
            }

            // The next three values will only work for DXF version AutoCad2007 and upwards
            this.chunk.Write(70, (short) blockRecord.Units);
            this.chunk.Write(280, blockRecord.AllowExploding ? (short) 1 : (short) 0);
            this.chunk.Write(281, blockRecord.ScaleUniformly ? (short) 1 : (short) 0);

            AddBlockRecordUnitsXData(blockRecord);

            this.WriteXData(blockRecord.XData);
        }

        private static void AddBlockRecordUnitsXData(BlockRecord record)
        {
            // for DXF versions prior to AutoCad2007 the block record units is stored in an extended data block
            XData xdataEntry;
            if (record.XData.ContainsAppId(ApplicationRegistry.DefaultName))
            {
                xdataEntry = record.XData[ApplicationRegistry.DefaultName];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.DefaultName));
                record.XData.Add(xdataEntry);
            }

            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, "DesignCenter Data"));
            xdataEntry.XDataRecord.Add(XDataRecord.OpenControlString);
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 1));
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) record.Units));
            xdataEntry.XDataRecord.Add(XDataRecord.CloseControlString);
        }

        /// <summary>
        /// Writes a new line type to the table section.
        /// </summary>
        /// <param name="linetype">Line type.</param>
        private void WriteLinetype(Linetype linetype)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.LinetypeTable);

            this.chunk.Write(0, linetype.CodeName);
            this.chunk.Write(5, linetype.Handle);
            this.chunk.Write(330, linetype.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.Linetype);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(linetype.Name));

            this.chunk.Write(70, (short) 0);

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(linetype.Description));

            this.chunk.Write(72, (short) 65);
            this.chunk.Write(73, (short) linetype.Segments.Count);
            this.chunk.Write(40, linetype.Length());

            foreach (LinetypeSegment s in linetype.Segments)
            {
                this.chunk.Write(49, s.Length);
                switch (s.Type)
                {
                    case LinetypeSegmentType.Simple:
                        this.chunk.Write(74, (short)0);
                        break;

                    case LinetypeSegmentType.Text:
                        LinetypeTextSegment textSegment = (LinetypeTextSegment)s;
                        if (textSegment.RotationType == LinetypeSegmentRotationType.Absolute)
                        {
                            this.chunk.Write(74, (short)3);
                        }
                        else
                        {
                            this.chunk.Write(74, (short)2);
                        }

                        this.chunk.Write(75, (short)0);
                        this.chunk.Write(340, textSegment.Style.Handle);
                        this.chunk.Write(46, textSegment.Scale);
                        this.chunk.Write(50, textSegment.Rotation); // the DXF documentation is wrong the rotation value is stored in degrees not radians
                        this.chunk.Write(44, textSegment.Offset.X);
                        this.chunk.Write(45, textSegment.Offset.Y);
                        this.chunk.Write(9, this.EncodeNonAsciiCharacters(textSegment.Text));

                        break;
                    case LinetypeSegmentType.Shape:
                        LinetypeShapeSegment shapeSegment = (LinetypeShapeSegment) s;
                        if (shapeSegment.RotationType == LinetypeSegmentRotationType.Absolute)
                        {
                            this.chunk.Write(74, (short)5);
                        }
                        else
                        {
                            this.chunk.Write(74, (short)4);
                        }

                        this.chunk.Write(75, shapeSegment.Style.ShapeNumber(shapeSegment.Name));
                        this.chunk.Write(340, shapeSegment.Style.Handle);
                        this.chunk.Write(46, shapeSegment.Scale);
                        this.chunk.Write(50, shapeSegment.Rotation); // the DXF documentation is wrong the rotation value is stored in degrees not radians
                        this.chunk.Write(44, shapeSegment.Offset.X);
                        this.chunk.Write(45, shapeSegment.Offset.Y);

                        break;
                }
            }

            this.WriteXData(linetype.XData);
        }

        /// <summary>
        /// Writes a new layer to the table section.
        /// </summary>
        /// <param name="layer">Layer.</param>
        private void WriteLayer(Layer layer)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.LayerTable);

            this.chunk.Write(0, layer.CodeName);
            this.chunk.Write(5, layer.Handle);
            this.chunk.Write(330, layer.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.Layer);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(layer.Name));

            LayerFlags flags = LayerFlags.None;
            if (layer.IsFrozen)
            {
                flags = flags | LayerFlags.Frozen;
            }
            if (layer.IsLocked)
            {
                flags = flags | LayerFlags.Locked;
            }
            this.chunk.Write(70, (short) flags);

            //a negative color represents a hidden layer.
            if (layer.IsVisible)
            {
                this.chunk.Write(62, layer.Color.Index);
            }
            else
            {
                this.chunk.Write(62, (short) -layer.Color.Index);
            }

            if (layer.Color.UseTrueColor)
            {
                this.chunk.Write(420, AciColor.ToTrueColor(layer.Color));
            }

            this.chunk.Write(6, this.EncodeNonAsciiCharacters(layer.Linetype.Name));

            this.chunk.Write(290, layer.Plot);
            this.chunk.Write(370, (short) layer.Lineweight);
            // Hard pointer ID/handle of PlotStyleName object
            this.chunk.Write(390, "0");

            // description is stored in XData
            if (!string.IsNullOrEmpty(layer.Description))
            {
                AddLayerDescriptionXData(layer);
            }

            // transparency is stored in XData
            if (layer.Transparency.Value > 0)
            {
                AddLayerTransparencyXData(layer);
            }

            this.WriteXData(layer.XData);
        }

        private static void AddLayerDescriptionXData(Layer layer)
        {
            XData xdataEntry;
            if (layer.XData.ContainsAppId("AcAecLayerStandard"))
            {
                xdataEntry = layer.XData["AcAecLayerStandard"];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry("AcAecLayerStandard"));
                layer.XData.Add(xdataEntry);
            }

            // the first entry seems to be always empty, its use is unknown
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, string.Empty));
            // the second entry holds the layer description
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, layer.Description));
        }

        private static void AddLayerTransparencyXData(Layer layer)
        {
            XData xdataEntry;
            if (layer.XData.ContainsAppId("AcCmTransparency"))
            {
                xdataEntry = layer.XData["AcCmTransparency"];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry("AcCmTransparency"));
                layer.XData.Add(xdataEntry);
            }

            int alpha = Transparency.ToAlphaValue(layer.Transparency);
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int32, alpha));
        }

        /// <summary>
        /// Writes a new text style to the table section.
        /// </summary>
        /// <param name="style">TextStyle.</param>
        private void WriteTextStyle(TextStyle style)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.TextStyleTable);

            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(5, style.Handle);
            this.chunk.Write(330, style.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.TextStyle);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(style.Name));

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(style.FontFile));

            if (!string.IsNullOrEmpty(style.BigFont))
            {
                this.chunk.Write(4, this.EncodeNonAsciiCharacters(style.BigFont));
            }

            this.chunk.Write(70, style.IsVertical ? (short) 4 : (short) 0);

            if (style.IsBackward && style.IsUpsideDown)
            {
                this.chunk.Write(71, (short) 6);
            }
            else if (style.IsBackward)
            {
                this.chunk.Write(71, (short) 2);
            }
            else if (style.IsUpsideDown)
            {
                this.chunk.Write(71, (short) 4);
            }
            else
            {
                this.chunk.Write(71, (short) 0);
            }

            this.chunk.Write(40, style.Height);
            this.chunk.Write(41, style.WidthFactor);
            this.chunk.Write(42, style.Height);
            this.chunk.Write(50, style.ObliqueAngle);

            // when a true type font file is present the font information is defined by the file and this information is not needed
            if (!string.IsNullOrEmpty(style.FontFamilyName))
            {
               this.AddTextStyleFontXData(style);
            }
            this.WriteXData(style.XData);
        }

        private void AddTextStyleFontXData(TextStyle style)
        {
            // for DXF versions prior to AutoCad2007 the block record units is stored in an extended data block
            XData xdataEntry;
            if (style.XData.ContainsAppId(ApplicationRegistry.DefaultName))
            {
                xdataEntry = style.XData[ApplicationRegistry.DefaultName];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.DefaultName));
                style.XData.Add(xdataEntry);
            }
            byte[] st = new byte[4];
            st[3] = (byte)style.FontStyle;
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, this.EncodeNonAsciiCharacters(style.FontFamilyName)));
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int32, BitConverter.ToInt32(st, 0)));
        }

        /// <summary>
        /// Writes a new shape style to the table section.
        /// </summary>
        /// <param name="style">ShapeStyle.</param>
        private void WriteShapeStyle(ShapeStyle style)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.TextStyleTable);

            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(5, style.Handle);
            this.chunk.Write(330, style.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.TextStyle);

            this.chunk.Write(2, string.Empty);

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(style.File));
            this.chunk.Write(70, (short)1);
            this.chunk.Write(40, style.Size);
            this.chunk.Write(41, style.WidthFactor);
            this.chunk.Write(42, style.Size);
            this.chunk.Write(50, style.ObliqueAngle);

            this.WriteXData(style.XData);
        }

        /// <summary>
        /// Writes a new user coordinate system to the table section.
        /// </summary>
        /// <param name="ucs">UCS.</param>
        private void WriteUCS(UCS ucs)
        {
            Debug.Assert(this.activeTable == DxfObjectCode.UcsTable);

            this.chunk.Write(0, ucs.CodeName);
            this.chunk.Write(5, ucs.Handle);
            this.chunk.Write(330, ucs.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.Ucs);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(ucs.Name));

            this.chunk.Write(70, (short) 0);

            this.chunk.Write(10, ucs.Origin.X);
            this.chunk.Write(20, ucs.Origin.Y);
            this.chunk.Write(30, ucs.Origin.Z);

            this.chunk.Write(11, ucs.XAxis.X);
            this.chunk.Write(21, ucs.XAxis.Y);
            this.chunk.Write(31, ucs.XAxis.Z);

            this.chunk.Write(12, ucs.YAxis.X);
            this.chunk.Write(22, ucs.YAxis.Y);
            this.chunk.Write(32, ucs.YAxis.Z);

            this.chunk.Write(79, (short) 0);

            this.WriteXData(ucs.XData);
        }

        #endregion

        #region methods for Block section

        private void WriteBlock(Block block)
        {
            Debug.Assert(this.activeSection == DxfObjectCode.BlocksSection);

            string name = this.EncodeNonAsciiCharacters(block.Name);
            string blockLayer = this.EncodeNonAsciiCharacters(block.Layer.Name);
            Layout layout = block.Record.Layout;

            this.chunk.Write(0, block.CodeName);
            this.chunk.Write(5, block.Handle);
            this.chunk.Write(330, block.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            if (layout != null)
                this.chunk.Write(67, layout.IsPaperSpace ? (short) 1 : (short) 0);

            this.chunk.Write(8, blockLayer);

            this.chunk.Write(100, SubclassMarker.BlockBegin);
            if (block.IsXRef)
                this.chunk.Write(1, this.EncodeNonAsciiCharacters(block.XrefFile));
            this.chunk.Write(2, name);
            this.chunk.Write(70, (short) block.Flags);
            this.chunk.Write(10, block.Origin.X);
            this.chunk.Write(20, block.Origin.Y);
            this.chunk.Write(30, block.Origin.Z);
            this.chunk.Write(3, name);
            this.chunk.Write(4, this.EncodeNonAsciiCharacters(block.Description));

            if (layout == null)
            {
                foreach (AttributeDefinition attdef in block.AttributeDefinitions.Values)
                {
                    this.WriteAttributeDefinition(attdef, null);
                }

                foreach (EntityObject entity in block.Entities)
                {
                    this.WriteEntity(entity, null);
                }
            }
            else
            {
                // the entities of the model space and the first paper space are written in the entities section,
                // the rest are written in the Block
                if (!(string.Equals(layout.AssociatedBlock.Name, Block.DefaultModelSpaceName, StringComparison.OrdinalIgnoreCase) ||
                      string.Equals(layout.AssociatedBlock.Name, Block.DefaultPaperSpaceName, StringComparison.OrdinalIgnoreCase)))
                {
                    this.WriteEntity(layout.Viewport, layout);

                    foreach (AttributeDefinition attdef in layout.AssociatedBlock.AttributeDefinitions.Values)
                    {
                        this.WriteAttributeDefinition(attdef, layout);
                    }

                    foreach (EntityObject entity in layout.AssociatedBlock.Entities)
                    {
                        this.WriteEntity(entity, layout);
                    }
                }               
            }

            // EndBlock entity
            this.chunk.Write(0, block.End.CodeName);
            this.chunk.Write(5, block.End.Handle);
            this.chunk.Write(330, block.Owner.Handle);
            this.chunk.Write(100, SubclassMarker.Entity);
            this.chunk.Write(8, blockLayer);
            this.chunk.Write(100, SubclassMarker.BlockEnd);

            this.WriteXData(block.XData);
        }

        #endregion

        #region methods for Entity section

        private void WriteEntity(EntityObject entity, Layout layout)
        {
            Debug.Assert(this.activeSection == DxfObjectCode.EntitiesSection || this.activeSection == DxfObjectCode.BlocksSection);
            Debug.Assert(entity != null);

            if (entity.Type == EntityType.Hatch && ((Hatch)entity).BoundaryPaths.Count == 0)
            {
                Debug.Assert(false, "Hatches with zero boundaries are not allowed." + "Entity handle: " + entity.Handle);
                return;
            }

            if (entity.Type == EntityType.Leader && ((Leader)entity).Vertexes.Count < 2)
            {
                Debug.Assert(false, "Leader entities with less than two vertexes are not allowed." + "Entity handle: " + entity.Handle);
                return;
            }

            if (entity.Type == EntityType.Polyline3D && ((Polyline3D)entity).Vertexes.Count < 2)
            {
                Debug.Assert(false, "Polyline3D entities with less than two vertexes are not allowed." + "Entity handle: " + entity.Handle);
                return;
            }

            if (entity.Type == EntityType.Polyline2D && ((Polyline2D)entity).Vertexes.Count < 2)
            {
                Debug.Assert(false, "Polyline2D entities with less than two vertexes are not allowed." + "Entity handle: " + entity.Handle);
                return;
            }

            this.WriteEntityCommonCodes(entity, layout);

            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.WriteArc((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.WriteCircle((Circle) entity);
                    break;
                case EntityType.Dimension:
                    this.WriteDimension((Dimension) entity);
                    break;
                case EntityType.Ellipse:
                    this.WriteEllipse((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    this.WriteFace3D((Face3D) entity);
                    break;
                case EntityType.Hatch:
                    this.WriteHatch((Hatch) entity);
                    break;
                case EntityType.Image:
                    this.WriteImage((Image) entity);
                    break;
                case EntityType.Insert:
                    this.WriteInsert((Insert) entity);
                    break;
                case EntityType.Leader:
                    this.WriteLeader((Leader) entity);
                    break;
                case EntityType.Line:
                    this.WriteLine((Line) entity);
                    break;
                case EntityType.Mesh:
                    this.WriteMesh((Mesh) entity);
                    break;
                case EntityType.MLine:
                    this.WriteMLine((MLine) entity);
                    break;
                case EntityType.MText:
                    this.WriteMText((MText) entity);
                    break;
                case EntityType.Point:
                    this.WritePoint((Point) entity);
                    break;
                case EntityType.Polyline2D:
                    Polyline2D polyline2D = (Polyline2D) entity;
                    if (polyline2D.SmoothType == PolylineSmoothType.NoSmooth)
                    {
                        this.WriteLwPolyline(polyline2D);
                    }
                    else
                    {
                        this.WritePolyline(this.polylines[polyline2D.Handle]);
                    }
                    break;
                case EntityType.Polyline3D:
                    this.WritePolyline(this.polylines[entity.Handle]);
                    break;
                case EntityType.PolyfaceMesh:
                    this.WritePolyline(this.polylines[entity.Handle]);
                    break;
                case EntityType.PolygonMesh:
                    this.WritePolyline(this.polylines[entity.Handle]);
                    break;
                case EntityType.Ray:
                    this.WriteRay((Ray) entity);
                    break;
                case EntityType.Shape:
                    this.WriteShape((Shape) entity);
                    break;
                case EntityType.Solid:
                    this.WriteSolid((Solid) entity);
                    break;
                case EntityType.Spline:
                    this.WriteSpline((Spline) entity);
                    break;
                case EntityType.Text:
                    this.WriteText((Text) entity);
                    break;
                case EntityType.Tolerance:
                    this.WriteTolerance((Tolerance) entity);
                    break;
                case EntityType.Trace:
                    this.WriteTrace((Trace) entity);
                    break;
                case EntityType.Underlay:
                    this.WriteUnderlay((Underlay) entity);
                    break;
                case EntityType.Viewport:
                    this.WriteViewport((Viewport) entity);
                    break;
                case EntityType.Wipeout:
                    this.WriteWipeout((Wipeout) entity);
                    break;
                case EntityType.XLine:
                    this.WriteXLine((XLine) entity);
                    break;
                default:
                    throw new ArgumentException("Entity unknown.", nameof(entity));
            }
        }

        private void WriteEntityCommonCodes(EntityObject entity, Layout layout)
        {
            this.chunk.Write(0, entity.CodeName);

            this.chunk.Write(5, entity.Handle);

            if (entity.Reactors.Count > 0)
            {
                this.chunk.Write(102, "{ACAD_REACTORS");
                foreach (DxfObject o in entity.Reactors)
                {
                    Debug.Assert(!string.IsNullOrEmpty(o.Handle), "The handle cannot be null or empty.");
                    this.chunk.Write(330, o.Handle);
                }
                this.chunk.Write(102, "}");
            }

            this.chunk.Write(330, entity.Owner.Record.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            if (layout != null)
            {
                this.chunk.Write(67, layout.IsPaperSpace ? (short) 1 : (short) 0);
            }

            this.chunk.Write(8, this.EncodeNonAsciiCharacters(entity.Layer.Name));

            this.chunk.Write(62, entity.Color.Index);
            if (entity.Color.UseTrueColor)
            {
                this.chunk.Write(420, AciColor.ToTrueColor(entity.Color));
            }

            if (entity.Transparency.Value >= 0)
            {
                this.chunk.Write(440, Transparency.ToAlphaValue(entity.Transparency));
            }

            this.chunk.Write(6, this.EncodeNonAsciiCharacters(entity.Linetype.Name));

            this.chunk.Write(370, (short) entity.Lineweight);
            this.chunk.Write(48, entity.LinetypeScale);
            this.chunk.Write(60, entity.IsVisible ? (short) 0 : (short) 1);
        }

        private void WriteWipeout(Wipeout wipeout)
        {
            this.chunk.Write(100, SubclassMarker.Wipeout);

            BoundingRectangle br = new BoundingRectangle(wipeout.ClippingBoundary.Vertexes);

            Vector3 ocsInsPoint = new Vector3(br.Min.X, br.Min.Y, wipeout.Elevation);
            double w = br.Width;
            double h = br.Height;
            double max = w >= h ? w : h;
            Vector3 ocsUx = new Vector3(max, 0.0, 0.0);
            Vector3 ocsUy = new Vector3(0.0, max, 0.0);

            List<Vector3> wcsPoints = MathHelper.Transform(new List<Vector3> {ocsInsPoint, ocsUx, ocsUy}, wipeout.Normal, CoordinateSystem.Object, CoordinateSystem.World);

            // Insertion point in WCS
            this.chunk.Write(10, wcsPoints[0].X);
            this.chunk.Write(20, wcsPoints[0].Y);
            this.chunk.Write(30, wcsPoints[0].Z);

            // U vector in WCS
            this.chunk.Write(11, wcsPoints[1].X);
            this.chunk.Write(21, wcsPoints[1].Y);
            this.chunk.Write(31, wcsPoints[1].Z);

            // V vector in WCS
            this.chunk.Write(12, wcsPoints[2].X);
            this.chunk.Write(22, wcsPoints[2].Y);
            this.chunk.Write(32, wcsPoints[2].Z);

            this.chunk.Write(13, 1.0);
            this.chunk.Write(23, 1.0);

            //this.chunk.Write(280, wipeout.ShowClippingFrame ? (short) 1 : (short) 0);
            this.chunk.Write(280, (short) 1);
            this.chunk.Write(281, (short) 50);
            this.chunk.Write(282, (short) 50);
            this.chunk.Write(283, (short) 0);

            this.chunk.Write(71, (short) wipeout.ClippingBoundary.Type);

            // for unknown reasons the wipeout with a polygonal clipping boundary requires to repeat the first vertex
            if (wipeout.ClippingBoundary.Type == ClippingBoundaryType.Polygonal)
            {
                this.chunk.Write(91, wipeout.ClippingBoundary.Vertexes.Count + 1);
                foreach (Vector2 vertex in wipeout.ClippingBoundary.Vertexes)
                {
                    double x = (vertex.X - ocsInsPoint.X)/max - 0.5;
                    double y = -((vertex.Y - ocsInsPoint.Y)/max - 0.5);
                    this.chunk.Write(14, x);
                    this.chunk.Write(24, y);
                }

                this.chunk.Write(14, (wipeout.ClippingBoundary.Vertexes[0].X - ocsInsPoint.X) / max - 0.5);
                this.chunk.Write(24, -((wipeout.ClippingBoundary.Vertexes[0].Y - ocsInsPoint.Y) / max - 0.5));
            }
            else
            {
                this.chunk.Write(91, wipeout.ClippingBoundary.Vertexes.Count);
                foreach (Vector2 vertex in wipeout.ClippingBoundary.Vertexes)
                {
                    double x = (vertex.X - ocsInsPoint.X) / max - 0.5;
                    double y = -((vertex.Y - ocsInsPoint.Y) / max - 0.5);
                    this.chunk.Write(14, x);
                    this.chunk.Write(24, y);
                }
            }
            this.WriteXData(wipeout.XData);
        }

        private void WriteUnderlay(Underlay underlay)
        {
            this.chunk.Write(100, SubclassMarker.Underlay);

            this.chunk.Write(340, underlay.Definition.Handle);

            Vector3 ocsPosition = MathHelper.Transform(underlay.Position, underlay.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.chunk.Write(10, ocsPosition.X);
            this.chunk.Write(20, ocsPosition.Y);
            this.chunk.Write(30, ocsPosition.Z);

            this.chunk.Write(41, underlay.Scale.X);
            this.chunk.Write(42, underlay.Scale.Y);
            this.chunk.Write(43, 1.0);

            this.chunk.Write(50, underlay.Rotation);

            this.chunk.Write(210, underlay.Normal.X);
            this.chunk.Write(220, underlay.Normal.Y);
            this.chunk.Write(230, underlay.Normal.Z);

            this.chunk.Write(280, (short) underlay.DisplayOptions);

            this.chunk.Write(281, underlay.Contrast);
            this.chunk.Write(282, underlay.Fade);

            if (underlay.ClippingBoundary != null)
            {
                foreach (Vector2 vertex in underlay.ClippingBoundary.Vertexes)
                {
                    this.chunk.Write(11, vertex.X);
                    this.chunk.Write(21, vertex.Y);
                }
            }

            this.WriteXData(underlay.XData);
        }

        private void WriteTolerance(Tolerance tolerance)
        {
            this.chunk.Write(100, SubclassMarker.Tolerance);

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(tolerance.Style.Name));

            this.chunk.Write(10, tolerance.Position.X);
            this.chunk.Write(20, tolerance.Position.Y);
            this.chunk.Write(30, tolerance.Position.Z);

            string rep = tolerance.ToStringRepresentation();
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(rep));

            this.chunk.Write(210, tolerance.Normal.X);
            this.chunk.Write(220, tolerance.Normal.Y);
            this.chunk.Write(230, tolerance.Normal.Z);

            double angle = tolerance.Rotation * MathHelper.DegToRad;
            Vector3 xAxis = new Vector3(Math.Cos(angle), Math.Sin(angle), 0.0);
            xAxis = MathHelper.Transform(xAxis, tolerance.Normal, CoordinateSystem.Object, CoordinateSystem.World);

            this.chunk.Write(11, xAxis.X);
            this.chunk.Write(21, xAxis.Y);
            this.chunk.Write(31, xAxis.Z);

            this.AddToleranceTextHeightXData(tolerance.XData, tolerance.TextHeight);

            this.WriteXData(tolerance.XData);
        }

        private void AddToleranceTextHeightXData(XDataDictionary xdata, double textHeight)
        {
            XData xdataEntry;
            if (xdata.ContainsAppId(ApplicationRegistry.DefaultName))
            {
                xdataEntry = xdata[ApplicationRegistry.DefaultName];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.DefaultName));
                xdata.Add(xdataEntry);
            }

            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, "DSTYLE"));
            xdataEntry.XDataRecord.Add(XDataRecord.OpenControlString);
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 140));
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, textHeight));
            xdataEntry.XDataRecord.Add(XDataRecord.CloseControlString);
        }

        private void WriteLeader(Leader leader)
        {
            this.chunk.Write(100, SubclassMarker.Leader);

            this.chunk.Write(3, leader.Style.Name);

            if (leader.ShowArrowhead)
            {
                this.chunk.Write(71, (short) 1);
            }
            else
            {
                this.chunk.Write(71, (short) 0);
            }

            this.chunk.Write(72, (short) leader.PathType);

            if (leader.Annotation != null)
            {
                switch (leader.Annotation.Type)
                {
                    case EntityType.Text:
                    case EntityType.MText:
                        this.chunk.Write(73, (short) 0);
                        break;
                    case EntityType.Tolerance:
                        this.chunk.Write(73, (short) 1);
                        break;
                    case EntityType.Insert:
                        this.chunk.Write(73, (short) 2);
                        break;
                }
            }
            else
            {
                this.chunk.Write(73, (short) 3);
            }

            this.chunk.Write(74, (short) 0);
            this.chunk.Write(75, leader.HasHookline ? (short) 1 : (short) 0);

            //this.chunk.Write(40, 0.0);
            //this.chunk.Write(41, 0.0);

            List<Vector3> ocsVertexes = new List<Vector3>();
            foreach (Vector2 vector in leader.Vertexes)
            {
                ocsVertexes.Add(new Vector3(vector.X, vector.Y, leader.Elevation));
            }

            List<Vector3> wcsVertexes = MathHelper.Transform(ocsVertexes, leader.Normal, CoordinateSystem.Object, CoordinateSystem.World);
            this.chunk.Write(76, (short) wcsVertexes.Count);
            foreach (Vector3 vertex in wcsVertexes)
            {
                this.chunk.Write(10, vertex.X);
                this.chunk.Write(20, vertex.Y);
                this.chunk.Write(30, vertex.Z);
            }

            this.chunk.Write(77, leader.LineColor.Index);

            if (leader.Annotation != null)
            {
                this.chunk.Write(340, leader.Annotation.Handle);
            }

            this.chunk.Write(210, leader.Normal.X);
            this.chunk.Write(220, leader.Normal.Y);
            this.chunk.Write(230, leader.Normal.Z);

            Vector3 xDir = MathHelper.Transform(leader.Direction, leader.Normal, 0.0);
            xDir.Normalize();
            this.chunk.Write(211, xDir.X);
            this.chunk.Write(221, xDir.Y);
            this.chunk.Write(231, xDir.Z);

            Vector3 wcsOffset = MathHelper.Transform(leader.Offset, leader.Normal, leader.Elevation);
            this.chunk.Write(213, wcsOffset.X);
            this.chunk.Write(223, wcsOffset.Y);
            this.chunk.Write(233, wcsOffset.Z);

            // dimension style overrides info
            if (leader.StyleOverrides.Count > 0)
            {
                this.AddDimensionStyleOverridesXData(leader.XData, leader.StyleOverrides);
            }

            this.WriteXData(leader.XData);
        }

        private void WriteMesh(Mesh mesh)
        {
            this.chunk.Write(100, SubclassMarker.Mesh);

            this.chunk.Write(71, (short) 2);
            this.chunk.Write(72, (short) 0);

            this.chunk.Write(91, (int) mesh.SubdivisionLevel);

            //vertexes
            this.chunk.Write(92, mesh.Vertexes.Count);
            foreach (Vector3 vertex in mesh.Vertexes)
            {
                this.chunk.Write(10, vertex.X);
                this.chunk.Write(20, vertex.Y);
                this.chunk.Write(30, vertex.Z);
            }

            //faces
            int sizeFaceList = mesh.Faces.Count;
            foreach (int[] face in mesh.Faces)
            {
                sizeFaceList += face.Length;
            }
            this.chunk.Write(93, sizeFaceList);
            foreach (int[] face in mesh.Faces)
            {
                this.chunk.Write(90, face.Length);
                foreach (int index in face)
                {
                    this.chunk.Write(90, index);
                }
            }

            // the edges information is optional, and only really useful when it is required to assign creases values to edges
            if (mesh.Edges != null)
            {
                //edges
                this.chunk.Write(94, mesh.Edges.Count);
                foreach (MeshEdge edge in mesh.Edges)
                {
                    this.chunk.Write(90, edge.StartVertexIndex);
                    this.chunk.Write(90, edge.EndVertexIndex);
                }

                //creases
                this.chunk.Write(95, mesh.Edges.Count);
                foreach (MeshEdge edge in mesh.Edges)
                {
                    this.chunk.Write(140, edge.Crease);
                }
            }

            this.chunk.Write(90, 0);

            this.WriteXData(mesh.XData);
        }

        private void WriteShape(Shape shape)
        {
            this.chunk.Write(100, SubclassMarker.Shape);

            this.chunk.Write(39, shape.Thickness);

            // The official DXF documentation is WRONG the SHAPE position is saved as OCS coordinates NOT as WCS
            Vector3 ocsBasePoint = MathHelper.Transform(shape.Position, shape.Normal, CoordinateSystem.World, CoordinateSystem.Object);
            this.chunk.Write(10, ocsBasePoint.X);
            this.chunk.Write(20, ocsBasePoint.Y);
            this.chunk.Write(30, ocsBasePoint.Z);
            this.chunk.Write(40, shape.Size);
            this.chunk.Write(2, shape.Name);
            this.chunk.Write(50, shape.Rotation);
            this.chunk.Write(41, shape.WidthFactor);
            this.chunk.Write(51, shape.ObliqueAngle);

            this.chunk.Write(210, shape.Normal.X);
            this.chunk.Write(220, shape.Normal.Y);
            this.chunk.Write(230, shape.Normal.Z);

            this.WriteXData(shape.XData);
        }

        private void WriteArc(Arc arc)
        {
            this.chunk.Write(100, SubclassMarker.Circle);

            this.chunk.Write(39, arc.Thickness);

            // this is just an example of the weird Autodesk DXF way of doing things, while an ellipse center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 ocsCenter = MathHelper.Transform(arc.Center, arc.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsCenter.X);
            this.chunk.Write(20, ocsCenter.Y);
            this.chunk.Write(30, ocsCenter.Z);

            this.chunk.Write(40, arc.Radius);

            this.chunk.Write(210, arc.Normal.X);
            this.chunk.Write(220, arc.Normal.Y);
            this.chunk.Write(230, arc.Normal.Z);

            this.chunk.Write(100, SubclassMarker.Arc);
            this.chunk.Write(50, arc.StartAngle);
            this.chunk.Write(51, arc.EndAngle);

            this.WriteXData(arc.XData);
        }

        private void WriteCircle(Circle circle)
        {
            this.chunk.Write(100, SubclassMarker.Circle);

            // this is just an example of the stupid autodesk DXF way of doing things, while an ellipse center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 ocsCenter = MathHelper.Transform(circle.Center, circle.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsCenter.X);
            this.chunk.Write(20, ocsCenter.Y);
            this.chunk.Write(30, ocsCenter.Z);

            this.chunk.Write(40, circle.Radius);

            this.chunk.Write(39, circle.Thickness);

            this.chunk.Write(210, circle.Normal.X);
            this.chunk.Write(220, circle.Normal.Y);
            this.chunk.Write(230, circle.Normal.Z);

            this.WriteXData(circle.XData);
        }

        private void WriteEllipse(Ellipse ellipse)
        {
            this.chunk.Write(100, SubclassMarker.Ellipse);

            this.chunk.Write(10, ellipse.Center.X);
            this.chunk.Write(20, ellipse.Center.Y);
            this.chunk.Write(30, ellipse.Center.Z);

            Vector2 axis = Vector2.Rotate(new Vector2(0.5*ellipse.MajorAxis, 0.0), ellipse.Rotation * MathHelper.DegToRad);

            Vector3 axisPoint = MathHelper.Transform(new Vector3(axis.X, axis.Y, 0.0), ellipse.Normal, CoordinateSystem.Object, CoordinateSystem.World);

            this.chunk.Write(11, axisPoint.X);
            this.chunk.Write(21, axisPoint.Y);
            this.chunk.Write(31, axisPoint.Z);

            this.chunk.Write(210, ellipse.Normal.X);
            this.chunk.Write(220, ellipse.Normal.Y);
            this.chunk.Write(230, ellipse.Normal.Z);

            this.chunk.Write(40, ellipse.MinorAxis/ellipse.MajorAxis);

            double[] paramaters = GetEllipseParameters(ellipse);
            this.chunk.Write(41, paramaters[0]);
            this.chunk.Write(42, paramaters[1]);

            this.WriteXData(ellipse.XData);
        }

        private static double[] GetEllipseParameters(Ellipse ellipse)
        {
            double atan1;
            double atan2;
            if (ellipse.IsFullEllipse)
            {
                atan1 = 0.0;
                atan2 = MathHelper.TwoPI;
            }
            else
            {
                Vector2 startPoint = ellipse.PolarCoordinateRelativeToCenter(ellipse.StartAngle);
                Vector2 endPoint = ellipse.PolarCoordinateRelativeToCenter(ellipse.EndAngle);
                double a = 1 / (0.5 * ellipse.MajorAxis);
                double b = 1 / (0.5 * ellipse.MinorAxis);
                atan1 = Math.Atan2(startPoint.Y * b, startPoint.X * a);
                atan2 = Math.Atan2(endPoint.Y * b, endPoint.X * a);
            }
            return new[] {atan1, atan2};
        }

        private void WriteSolid(Solid solid)
        {
            this.chunk.Write(100, SubclassMarker.Solid);

            // the vertexes are stored in OCS
            this.chunk.Write(10, solid.FirstVertex.X);
            this.chunk.Write(20, solid.FirstVertex.Y);
            this.chunk.Write(30, solid.Elevation);

            this.chunk.Write(11, solid.SecondVertex.X);
            this.chunk.Write(21, solid.SecondVertex.Y);
            this.chunk.Write(31, solid.Elevation);

            this.chunk.Write(12, solid.ThirdVertex.X);
            this.chunk.Write(22, solid.ThirdVertex.Y);
            this.chunk.Write(32, solid.Elevation);

            this.chunk.Write(13, solid.FourthVertex.X);
            this.chunk.Write(23, solid.FourthVertex.Y);
            this.chunk.Write(33, solid.Elevation);

            this.chunk.Write(39, solid.Thickness);

            this.chunk.Write(210, solid.Normal.X);
            this.chunk.Write(220, solid.Normal.Y);
            this.chunk.Write(230, solid.Normal.Z);

            this.WriteXData(solid.XData);
        }

        private void WriteTrace(Trace trace)
        {
            this.chunk.Write(100, SubclassMarker.Trace);

            // the vertexes are stored in OCS
            this.chunk.Write(10, trace.FirstVertex.X);
            this.chunk.Write(20, trace.FirstVertex.Y);
            this.chunk.Write(30, trace.Elevation);

            this.chunk.Write(11, trace.SecondVertex.X);
            this.chunk.Write(21, trace.SecondVertex.Y);
            this.chunk.Write(31, trace.Elevation);

            this.chunk.Write(12, trace.ThirdVertex.X);
            this.chunk.Write(22, trace.ThirdVertex.Y);
            this.chunk.Write(32, trace.Elevation);

            this.chunk.Write(13, trace.FourthVertex.X);
            this.chunk.Write(23, trace.FourthVertex.Y);
            this.chunk.Write(33, trace.Elevation);

            this.chunk.Write(39, trace.Thickness);

            this.chunk.Write(210, trace.Normal.X);
            this.chunk.Write(220, trace.Normal.Y);
            this.chunk.Write(230, trace.Normal.Z);

            this.WriteXData(trace.XData);
        }

        private void WriteFace3D(Face3D face)
        {
            this.chunk.Write(100, SubclassMarker.Face3D);

            this.chunk.Write(10, face.FirstVertex.X);
            this.chunk.Write(20, face.FirstVertex.Y);
            this.chunk.Write(30, face.FirstVertex.Z);

            this.chunk.Write(11, face.SecondVertex.X);
            this.chunk.Write(21, face.SecondVertex.Y);
            this.chunk.Write(31, face.SecondVertex.Z);

            this.chunk.Write(12, face.ThirdVertex.X);
            this.chunk.Write(22, face.ThirdVertex.Y);
            this.chunk.Write(32, face.ThirdVertex.Z);

            this.chunk.Write(13, face.FourthVertex.X);
            this.chunk.Write(23, face.FourthVertex.Y);
            this.chunk.Write(33, face.FourthVertex.Z);

            this.chunk.Write(70, (short) face.EdgeFlags);

            this.WriteXData(face.XData);
        }

        private void WriteSpline(Spline spline)
        {
            this.chunk.Write(100, SubclassMarker.Spline);

            short flags = (short) SplineTypeFlags.Rational;
            if (spline.IsClosed) flags += (short) SplineTypeFlags.Closed;
            if (spline.IsClosedPeriodic) flags = (short) SplineTypeFlags.ClosedPeriodicSpline + (short) SplineTypeFlags.Closed;
            
            flags += (short) spline.CreationMethod;
            flags += (short) spline.KnotParameterization;

            this.chunk.Write(70, flags);
            this.chunk.Write(71, spline.Degree);

            // the next three codes are purely cosmetic and writing them causes more bad than good.
            // internally AutoCad allows for an integer number of knots, control points, and fit points;
            // but for some weird decision they decided to define them in the DXF with codes 72, 73, and 74 (16-bit integer value), a short.
            // I guess this is the result of legacy code, nevertheless AutoCad do not use those values when importing Spline entities
            //this.chunk.Write(72, numberOfKnots);
            //this.chunk.Write(73, numberOfControlPoints);
            //this.chunk.Write(74, numberOfFitPoints);

            this.chunk.Write(42, spline.KnotTolerance);
            this.chunk.Write(43, spline.CtrlPointTolerance);
            this.chunk.Write(44, spline.FitTolerance);

            if (spline.StartTangent != null)
            {
                this.chunk.Write(12, spline.StartTangent.Value.X);
                this.chunk.Write(22, spline.StartTangent.Value.Y);
                this.chunk.Write(32, spline.StartTangent.Value.Z);
            }

            if (spline.EndTangent != null)
            {
                this.chunk.Write(13, spline.EndTangent.Value.X);
                this.chunk.Write(23, spline.EndTangent.Value.Y);
                this.chunk.Write(33, spline.EndTangent.Value.Z);
            }

            foreach (double knot in spline.Knots)
            {
                this.chunk.Write(40, knot);
            }

            if (spline.IsClosedPeriodic)
            {
                for (int i = 0; i < spline.Degree; i++)
                {
                    Vector3 point = spline.ControlPoints[spline.ControlPoints.Length - spline.Degree + i];
                    this.chunk.Write(10, point.X);
                    this.chunk.Write(20, point.Y);
                    this.chunk.Write(30, point.Z);
                    this.chunk.Write(41, spline.Weights[spline.Weights.Length - spline.Degree + i]);
                }
            }
            for (int i = 0; i < spline.ControlPoints.Length; i++)
            {
                this.chunk.Write(10, spline.ControlPoints[i].X);
                this.chunk.Write(20, spline.ControlPoints[i].Y);
                this.chunk.Write(30, spline.ControlPoints[i].Z);
                this.chunk.Write(41, spline.Weights[i]);
            }
            foreach (Vector3 point in spline.FitPoints)
            {
                this.chunk.Write(11, point.X);
                this.chunk.Write(21, point.Y);
                this.chunk.Write(31, point.Z);
            }

            this.WriteXData(spline.XData);
        }

        private void WriteInsert(Insert insert)
        {
            this.chunk.Write(100, SubclassMarker.Insert);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(insert.Block.Name));

            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 ocsInsertion = MathHelper.Transform(insert.Position, insert.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsInsertion.X);
            this.chunk.Write(20, ocsInsertion.Y);
            this.chunk.Write(30, ocsInsertion.Z);

            // we need to apply the scaling factor between the block and the document, or the block that owns it in case of nested blocks
            double scale = UnitHelper.ConversionFactor(insert.Block.Record.Units, insert.Owner.Record.IsForInternalUseOnly ? this.doc.DrawingVariables.InsUnits : insert.Owner.Record.Units);

            this.chunk.Write(41, insert.Scale.X*scale);
            this.chunk.Write(42, insert.Scale.Y*scale);
            this.chunk.Write(43, insert.Scale.Z*scale);

            this.chunk.Write(50, insert.Rotation);

            this.chunk.Write(210, insert.Normal.X);
            this.chunk.Write(220, insert.Normal.Y);
            this.chunk.Write(230, insert.Normal.Z);

            if (insert.Attributes.Count > 0)
            {
                //Obsolete; formerly an entities follow flag (optional; ignore if present)
                //AutoCAD will fail loading the file if it is not there, more DXF voodoo
                this.chunk.Write(66, (short) 1);

                this.WriteXData(insert.XData);

                foreach (Attribute attrib in insert.Attributes)
                {
                    this.WriteAttribute(attrib);
                }

                EndSequence endSequence = this.insertEndSequences[insert.Handle];
                this.chunk.Write(0, endSequence.CodeName);
                this.chunk.Write(5, endSequence.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);
                this.chunk.Write(8, this.EncodeNonAsciiCharacters(insert.Layer.Name));
            }
            else
            {
                this.WriteXData(insert.XData);
            }
        }

        private void WriteLine(Line line)
        {
            this.chunk.Write(100, SubclassMarker.Line);

            this.chunk.Write(10, line.StartPoint.X);
            this.chunk.Write(20, line.StartPoint.Y);
            this.chunk.Write(30, line.StartPoint.Z);

            this.chunk.Write(11, line.EndPoint.X);
            this.chunk.Write(21, line.EndPoint.Y);
            this.chunk.Write(31, line.EndPoint.Z);

            this.chunk.Write(39, line.Thickness);

            this.chunk.Write(210, line.Normal.X);
            this.chunk.Write(220, line.Normal.Y);
            this.chunk.Write(230, line.Normal.Z);

            this.WriteXData(line.XData);
        }

        private void WriteRay(Ray ray)
        {
            this.chunk.Write(100, SubclassMarker.Ray);

            this.chunk.Write(10, ray.Origin.X);
            this.chunk.Write(20, ray.Origin.Y);
            this.chunk.Write(30, ray.Origin.Z);

            this.chunk.Write(11, ray.Direction.X);
            this.chunk.Write(21, ray.Direction.Y);
            this.chunk.Write(31, ray.Direction.Z);

            this.WriteXData(ray.XData);
        }

        private void WriteXLine(XLine xline)
        {
            this.chunk.Write(100, SubclassMarker.XLine);

            this.chunk.Write(10, xline.Origin.X);
            this.chunk.Write(20, xline.Origin.Y);
            this.chunk.Write(30, xline.Origin.Z);

            this.chunk.Write(11, xline.Direction.X);
            this.chunk.Write(21, xline.Direction.Y);
            this.chunk.Write(31, xline.Direction.Z);

            this.WriteXData(xline.XData);
        }

        private void WriteLwPolyline(Polyline2D polyline2D)
        {
            this.chunk.Write(100, SubclassMarker.Polyline);
            this.chunk.Write(90, polyline2D.Vertexes.Count);
            this.chunk.Write(70, (short) polyline2D.Flags);

            this.chunk.Write(38, polyline2D.Elevation);
            this.chunk.Write(39, polyline2D.Thickness);


            foreach (Polyline2DVertex v in polyline2D.Vertexes)
            {
                this.chunk.Write(10, v.Position.X);
                this.chunk.Write(20, v.Position.Y);
                this.chunk.Write(40, v.StartWidth);
                this.chunk.Write(41, v.EndWidth);
                this.chunk.Write(42, v.Bulge);
            }

            this.chunk.Write(210, polyline2D.Normal.X);
            this.chunk.Write(220, polyline2D.Normal.Y);
            this.chunk.Write(230, polyline2D.Normal.Z);

            this.WriteXData(polyline2D.XData);
        }

        private void WritePolyline(Polyline polyline)
        {
            this.chunk.Write(100, polyline.SubclassMarker);

            //dummy point
            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);
            this.chunk.Write(30, polyline.Elevation);

            if (polyline.SubclassMarker == SubclassMarker.PolygonMesh)
            {
                this.chunk.Write(71, polyline.M);
                this.chunk.Write(72, polyline.N);
                this.chunk.Write(73, polyline.DensityM);
                this.chunk.Write(74, polyline.DensityN);
            }
            
            this.chunk.Write(70, (short) polyline.Flags);
            this.chunk.Write(75, (short) polyline.SmoothType);

            this.chunk.Write(210, polyline.Normal.X);
            this.chunk.Write(220, polyline.Normal.Y);
            this.chunk.Write(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);

            string layerName = this.EncodeNonAsciiCharacters(polyline.Layer.Name);

            // More DXF weirdness, why polyline vertexes are considered as an entity? Legacy code?
            foreach (Vertex v in polyline.Vertexes)
            {
                this.chunk.Write(0, v.CodeName);
                this.chunk.Write(5, v.Handle);
                this.chunk.Write(330, v.Owner.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);

                if (v.VertexIndexes == null)
                {
                    this.chunk.Write(8, layerName); // the vertex layer should be the same as the polyline layer
                    this.chunk.Write(62, polyline.Color.Index); // the vertex color should be the same as the polyline color
                    if (polyline.Color.UseTrueColor)
                    {
                        this.chunk.Write(420, AciColor.ToTrueColor(polyline.Color));
                    }
                    this.chunk.Write(100, SubclassMarker.Vertex);
                    this.chunk.Write(100, v.SubclassMarker);
                }
                else
                {
                    // each face in a polyface mesh can have its own layer
                    if (v.Layer != null)
                    {
                        this.chunk.Write(8, this.EncodeNonAsciiCharacters(v.Layer.Name));
                    }

                    // each face in a polyface mesh can have its own color
                    if (v.Color != null)
                    {
                        this.chunk.Write(62, v.Color.Index);
                        if (v.Color.UseTrueColor)
                        {
                            this.chunk.Write(420, AciColor.ToTrueColor(v.Color));
                        }
                    }

                    this.chunk.Write(100, v.SubclassMarker);
                    short code = 71;
                    foreach (short index in v.VertexIndexes)
                    {
                        this.chunk.Write(code++, index);
                    }
                }

                this.chunk.Write(10, v.Position.X);
                this.chunk.Write(20, v.Position.Y);
                this.chunk.Write(30, v.Position.Z);
                this.chunk.Write(70, (short) v.Flags);
                this.chunk.Write(40, v.StartWidth);
                this.chunk.Write(41, v.EndWidth);
            }

            // More DXF weirdness, why polyline end sequence are considered as an entity, or why it even exists? Legacy code?
            this.chunk.Write(0, polyline.EndSequence.CodeName);
            this.chunk.Write(5, polyline.EndSequence.Handle);
            this.chunk.Write(330, polyline.EndSequence.Owner.Handle);
            this.chunk.Write(100, SubclassMarker.Entity);
            this.chunk.Write(8, layerName); // the polyline EndSequence layer should be the same as the polyline layer
        }

        private void WritePoint(Point point)
        {
            this.chunk.Write(100, SubclassMarker.Point);

            this.chunk.Write(10, point.Position.X);
            this.chunk.Write(20, point.Position.Y);
            this.chunk.Write(30, point.Position.Z);

            this.chunk.Write(39, point.Thickness);

            this.chunk.Write(210, point.Normal.X);
            this.chunk.Write(220, point.Normal.Y);
            this.chunk.Write(230, point.Normal.Z);

            // for unknown reasons the DXF likes the point rotation inverted, NONSENSE
            this.chunk.Write(50, 360.0 - point.Rotation);

            this.WriteXData(point.XData);
        }

        private void WriteText(Text text)
        {
            this.chunk.Write(100, SubclassMarker.Text);

            this.chunk.Write(1, this.EncodeNonAsciiCharacters(text.Value));

            // another example of this OCS vs WCS nonsense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            Vector3 ocsBasePoint = MathHelper.Transform(text.Position, text.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsBasePoint.X);
            this.chunk.Write(20, ocsBasePoint.Y);
            this.chunk.Write(30, ocsBasePoint.Z);

            this.chunk.Write(40, text.Height);

            this.chunk.Write(41, text.WidthFactor);

            this.chunk.Write(50, text.Rotation);

            this.chunk.Write(51, text.ObliqueAngle);

            this.chunk.Write(7, this.EncodeNonAsciiCharacters(text.Style.Name));

            if (text.Alignment == TextAlignment.Fit || text.Alignment == TextAlignment.Aligned)
            {
                Vector2 endPoint = Vector2.Rotate(text.Width * Vector2.UnitX, text.Rotation * MathHelper.DegToRad);
                Vector3 ocsEndPoint = ocsBasePoint + new Vector3(endPoint.X, endPoint.Y, 0.0);
                this.chunk.Write(11, ocsEndPoint.X);
                this.chunk.Write(21, ocsEndPoint.Y);
                this.chunk.Write(31, ocsEndPoint.Z);
            }
            else
            {
                this.chunk.Write(11, ocsBasePoint.X);
                this.chunk.Write(21, ocsBasePoint.Y);
                this.chunk.Write(31, ocsBasePoint.Z);
            }

            this.chunk.Write(210, text.Normal.X);
            this.chunk.Write(220, text.Normal.Y);
            this.chunk.Write(230, text.Normal.Z);

            short textGeneration = 0;
            if (text.IsBackward)
            {
                textGeneration += 2;
            }

            if (text.IsUpsideDown)
            {
                textGeneration += 4;
            }

            this.chunk.Write(71, textGeneration);

            switch (text.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 3);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 3);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 2);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 1);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short) 3);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(72, (short) 4);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(72, (short) 5);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short) 0);
                    break;
            }

            this.WriteXData(text.XData);
        }

        private void WriteMText(MText mText)
        {
            this.chunk.Write(100, SubclassMarker.MText);

            this.chunk.Write(10, mText.Position.X);
            this.chunk.Write(20, mText.Position.Y);
            this.chunk.Write(30, mText.Position.Z);

            this.chunk.Write(210, mText.Normal.X);
            this.chunk.Write(220, mText.Normal.Y);
            this.chunk.Write(230, mText.Normal.Z);

            this.WriteMTextChunks(this.EncodeNonAsciiCharacters(mText.Value));

            this.chunk.Write(40, mText.Height);
            this.chunk.Write(41, mText.RectangleWidth);
            this.chunk.Write(44, mText.LineSpacingFactor);

            //even if the AutoCAD DXF documentation says that the rotation is in radians, this is wrong this value must be saved in degrees
            //this.chunk.Write(50, mText.Rotation);

            //the other option for the rotation is to store the horizontal vector of the text
            //this is what happens when duplicate information is stored, always ends in trouble
            //it will be used just in case other programs read the rotation as radians, QCAD seems to do that.
            Vector2 direction = Vector2.Rotate(Vector2.UnitX, mText.Rotation * MathHelper.DegToRad);
            direction.Normalize();
            Vector3 ocsDirection = MathHelper.Transform(new Vector3(direction.X, direction.Y, 0.0), mText.Normal, CoordinateSystem.Object, CoordinateSystem.World);

            this.chunk.Write(11, ocsDirection.X);
            this.chunk.Write(21, ocsDirection.Y);
            this.chunk.Write(31, ocsDirection.Z);

            this.chunk.Write(71, (short) mText.AttachmentPoint);

            this.chunk.Write(72, (short) mText.DrawingDirection);

            this.chunk.Write(73, (short) mText.LineSpacingStyle);

            this.chunk.Write(7, this.EncodeNonAsciiCharacters(mText.Style.Name));

            this.WriteXData(mText.XData);
        }

        private void WriteMTextChunks(string text)
        {
            //Text string. If the text string is less than 250 characters, all characters
            //appear in group 1. If the text string is greater than 250 characters, the
            //string is divided into 250 character chunks, which appear in one or
            //more group 3 codes. If group 3 codes are used, the last group is a
            //group 1 and has fewer than 250 characters
            while (text.Length > 250)
            {
                string part = text.Substring(0, 250);
                this.chunk.Write(3, part);
                text = text.Remove(0, 250);
            }
            this.chunk.Write(1, text);
        }

        private void WriteHatch(Hatch hatch)
        {
            this.chunk.Write(100, SubclassMarker.Hatch);

            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);
            this.chunk.Write(30, hatch.Elevation);

            this.chunk.Write(210, hatch.Normal.X);
            this.chunk.Write(220, hatch.Normal.Y);
            this.chunk.Write(230, hatch.Normal.Z);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(hatch.Pattern.Name));

            this.chunk.Write(70, (short) hatch.Pattern.Fill);

            this.chunk.Write(71, hatch.Associative ? (short) 1 : (short) 0);

            // boundary paths info
            this.WriteHatchBoundaryPaths(hatch.BoundaryPaths);

            // pattern info
            this.WriteHatchPattern(hatch.Pattern);

            // add the required extended data entries to the hatch XData
            AddHatchPatternXData(hatch);

            this.WriteXData(hatch.XData);
        }

        private static void AddHatchPatternXData(Hatch hatch)
        {
            XData xdataEntry;
            if (hatch.XData.ContainsAppId(ApplicationRegistry.DefaultName))
            {
                xdataEntry = hatch.XData[ApplicationRegistry.DefaultName];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.DefaultName));
                hatch.XData.Add(xdataEntry);
            }
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealX, hatch.Pattern.Origin.X));
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealY, hatch.Pattern.Origin.Y));
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealZ, 0.0));

            HatchGradientPattern grad = hatch.Pattern as HatchGradientPattern;

            if (grad == null) return;

            if (hatch.XData.ContainsAppId("GradientColor1ACI"))
            {
                xdataEntry = hatch.XData["GradientColor1ACI"];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry("GradientColor1ACI"));
                hatch.XData.Add(xdataEntry);
            }
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, grad.Color1.Index));


            if (hatch.XData.ContainsAppId("GradientColor2ACI"))
            {
                xdataEntry = hatch.XData["GradientColor2ACI"];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry("GradientColor2ACI"));
                hatch.XData.Add(xdataEntry);
            }
            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, grad.Color2.Index));
        }

        private void WriteHatchBoundaryPaths(ObservableCollection<HatchBoundaryPath> boundaryPaths)
        {
            this.chunk.Write(91, boundaryPaths.Count);

            // each hatch boundary paths are made of multiple closed loops
            foreach (HatchBoundaryPath path in boundaryPaths)
            {
                this.chunk.Write(92, (int) path.PathType);

                if (!path.PathType.HasFlag(HatchBoundaryPathTypeFlags.Polyline))
                {
                    this.chunk.Write(93, path.Edges.Count);
                }

                foreach (HatchBoundaryPath.Edge entity in path.Edges)
                {
                    this.WriteHatchBoundaryPathData(entity);
                }

                this.chunk.Write(97, path.Entities.Count);
                foreach (EntityObject entity in path.Entities)
                {
                    this.chunk.Write(330, entity.Handle);
                }
            }
        }

        private void WriteHatchBoundaryPathData(HatchBoundaryPath.Edge entity)
        {
            if (entity.Type == HatchBoundaryPath.EdgeType.Arc)
            {
                this.chunk.Write(72, (short) 2); // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Arc arc = (HatchBoundaryPath.Arc) entity;

                this.chunk.Write(10, arc.Center.X);
                this.chunk.Write(20, arc.Center.Y);
                this.chunk.Write(40, arc.Radius);
                this.chunk.Write(50, arc.StartAngle);
                this.chunk.Write(51, arc.EndAngle);
                this.chunk.Write(73, arc.IsCounterclockwise ? (short) 1 : (short) 0);
            }
            else if (entity.Type == HatchBoundaryPath.EdgeType.Ellipse)
            {
                this.chunk.Write(72, (short) 3); // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Ellipse ellipse = (HatchBoundaryPath.Ellipse) entity;

                this.chunk.Write(10, ellipse.Center.X);
                this.chunk.Write(20, ellipse.Center.Y);
                this.chunk.Write(11, ellipse.EndMajorAxis.X);
                this.chunk.Write(21, ellipse.EndMajorAxis.Y);
                this.chunk.Write(40, ellipse.MinorRatio);
                this.chunk.Write(50, ellipse.StartAngle);
                this.chunk.Write(51, ellipse.EndAngle);
                this.chunk.Write(73, ellipse.IsCounterclockwise ? (short) 1 : (short) 0);
            }
            else if (entity.Type == HatchBoundaryPath.EdgeType.Line)
            {
                this.chunk.Write(72, (short) 1); // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Line line = (HatchBoundaryPath.Line) entity;

                this.chunk.Write(10, line.Start.X);
                this.chunk.Write(20, line.Start.Y);
                this.chunk.Write(11, line.End.X);
                this.chunk.Write(21, line.End.Y);
            }
            else if (entity.Type == HatchBoundaryPath.EdgeType.Polyline)
            {
                HatchBoundaryPath.Polyline poly = (HatchBoundaryPath.Polyline) entity;
                this.chunk.Write(72, (short) 1); // Has bulge flag
                this.chunk.Write(73, poly.IsClosed ? (short) 1 : (short) 0);
                this.chunk.Write(93, poly.Vertexes.Length);

                foreach (Vector3 vertex in poly.Vertexes)
                {
                    this.chunk.Write(10, vertex.X);
                    this.chunk.Write(20, vertex.Y);
                    this.chunk.Write(42, vertex.Z);
                }
            }
            else if (entity.Type == HatchBoundaryPath.EdgeType.Spline)
            {
                this.chunk.Write(72, (short) 4); // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Spline spline = (HatchBoundaryPath.Spline) entity;

                // another DXF inconsistency!; while the Spline entity degree is written as a short (code 71)
                // the degree of a hatch boundary path spline is written as an integer (code 94)
                this.chunk.Write(94, (int) spline.Degree);
                this.chunk.Write(73, spline.IsRational ? (short) 1 : (short) 0);
                this.chunk.Write(74, spline.IsPeriodic ? (short) 1 : (short) 0);

                // now the number of knots and control points of a spline are written as an integer, as it should be.
                // but in the Spline entities they are defined as shorts. Guess what, while you can avoid writing these two codes for the Spline entity, now they are required.
                this.chunk.Write(95, spline.Knots.Length);
                this.chunk.Write(96, spline.ControlPoints.Length);

                foreach (double knot in spline.Knots)
                {
                    this.chunk.Write(40, knot);
                }

                foreach (Vector3 point in spline.ControlPoints)
                {
                    this.chunk.Write(10, point.X);
                    this.chunk.Write(20, point.Y);
                    if (spline.IsRational)
                    {
                        this.chunk.Write(42, point.Z);
                    }
                }

                // this information is only required for AutoCAD version 2010
                // stores information about spline fit points (the spline entity has no fit points and no tangent info)
                // another DXF inconsistency!; while the number of fit points of Spline entity is written as a short (code 74)
                // the number of fit points of a hatch boundary path spline is written as an int (code 97)
                if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2010)
                {
                    this.chunk.Write(97, 0);
                }
            }
        }

        private void WriteHatchPattern(HatchPattern pattern)
        {
            this.chunk.Write(75, (short) pattern.Style);
            this.chunk.Write(76, (short) pattern.Type);

            if (pattern.Fill == HatchFillType.PatternFill)
            {
                this.chunk.Write(52, pattern.Angle);
                this.chunk.Write(41, pattern.Scale);
                this.chunk.Write(77, (short) 0); // Hatch pattern double flag
                this.chunk.Write(78, (short) pattern.LineDefinitions.Count); // Number of pattern definition lines  
                this.WriteHatchPatternDefinitionLines(pattern);
            }

            // I don't know what is the purpose of these codes, it seems that it doesn't change anything but they are needed
            this.chunk.Write(47, 0.0);
            this.chunk.Write(98, 1);
            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);

            // DXF AutoCad2000 does not support hatch gradient patterns
            if (this.doc.DrawingVariables.AcadVer <= DxfVersion.AutoCad2000)
            {
                return;
            }

            if (pattern is HatchGradientPattern gradientPattern)
            {
                this.WriteGradientHatchPattern(gradientPattern);
            }
        }

        private void WriteGradientHatchPattern(HatchGradientPattern pattern)
        {
            // again the order of codes shown in the documentation will not work
            this.chunk.Write(450, 1);
            this.chunk.Write(451, 0);
            this.chunk.Write(460, pattern.Angle*MathHelper.DegToRad);
            this.chunk.Write(461, pattern.Centered ? 0.0 : 1.0);
            this.chunk.Write(452, pattern.SingleColor ? 1 : 0);
            this.chunk.Write(462, pattern.Tint);
            this.chunk.Write(453, 2);
            this.chunk.Write(463, 0.0);
            this.chunk.Write(63, pattern.Color1.Index);
            this.chunk.Write(421, AciColor.ToTrueColor(pattern.Color1));
            this.chunk.Write(463, 1.0);
            this.chunk.Write(63, pattern.Color2.Index);
            this.chunk.Write(421, AciColor.ToTrueColor(pattern.Color2));
            this.chunk.Write(470, StringEnum<HatchGradientPatternType>.GetStringValue(pattern.GradientType));
        }

        private void WriteHatchPatternDefinitionLines(HatchPattern pattern)
        {
            foreach (HatchPatternLineDefinition line in pattern.LineDefinitions)
            {
                double scale = pattern.Scale;
                double angle = line.Angle + pattern.Angle;
                // Pattern fill data.
                // In theory this should hold the same information as the pat file but for unknown reason the DXF requires global data instead of local,
                // it's a guess the documentation is kinda obscure.
                // This means we have to apply the pattern rotation and scale to the line definitions
                this.chunk.Write(53, angle);

                double sinOrigin = Math.Sin(pattern.Angle*MathHelper.DegToRad);
                double cosOrigin = Math.Cos(pattern.Angle*MathHelper.DegToRad);
                Vector2 origin = new Vector2(cosOrigin*line.Origin.X*scale - sinOrigin*line.Origin.Y*scale, sinOrigin*line.Origin.X*scale + cosOrigin*line.Origin.Y*scale);
                this.chunk.Write(43, origin.X);
                this.chunk.Write(44, origin.Y);

                double sinDelta = Math.Sin(angle*MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle*MathHelper.DegToRad);
                Vector2 delta = new Vector2(cosDelta*line.Delta.X*scale - sinDelta*line.Delta.Y*scale, sinDelta*line.Delta.X*scale + cosDelta*line.Delta.Y*scale);
                this.chunk.Write(45, delta.X);
                this.chunk.Write(46, delta.Y);

                this.chunk.Write(79, (short) line.DashPattern.Count);
                foreach (double dash in line.DashPattern)
                {
                    this.chunk.Write(49, dash*scale);
                }
            }
        }

        private void WriteDimension(Dimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Dimension);

            if (dim.Block != null)
            {
                this.chunk.Write(2, this.EncodeNonAsciiCharacters(dim.Block.Name));
            }

            Vector3 wcsDef = MathHelper.Transform(dim.DefinitionPoint, dim.Normal, dim.Elevation);
            this.chunk.Write(10, wcsDef.X);
            this.chunk.Write(20, wcsDef.Y);
            this.chunk.Write(30, wcsDef.Z);
            this.chunk.Write(11, dim.TextReferencePoint.X);
            this.chunk.Write(21, dim.TextReferencePoint.Y);
            this.chunk.Write(31, dim.Elevation);

            DimensionTypeFlags flags = (DimensionTypeFlags) dim.DimensionType;
            flags |= DimensionTypeFlags.BlockReference;
            if (dim.TextPositionManuallySet)
            {
                flags |= DimensionTypeFlags.UserTextPosition;
            }

            if (dim is OrdinateDimension ordinateDim)
            {
                // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly if this value is not provided
                this.chunk.Write(51, 360.0 - ordinateDim.Rotation);
                if (ordinateDim.Axis == OrdinateDimensionAxis.X)
                {
                    flags |= DimensionTypeFlags.OrdinateType;
                }
            }
            this.chunk.Write(53, dim.TextRotation);
            this.chunk.Write(70, (short) flags);
            this.chunk.Write(71, (short) dim.AttachmentPoint);
            this.chunk.Write(72, (short) dim.LineSpacingStyle);
            this.chunk.Write(41, dim.LineSpacingFactor);
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(dim.UserText));

            this.chunk.Write(210, dim.Normal.X);
            this.chunk.Write(220, dim.Normal.Y);
            this.chunk.Write(230, dim.Normal.Z);

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(dim.Style.Name));

            // add dimension style overrides info
            if (dim.StyleOverrides.Count > 0)
            {
                this.AddDimensionStyleOverridesXData(dim.XData, dim.StyleOverrides);
            }

            switch (dim.DimensionType)
            {
                case DimensionType.Aligned:
                    this.WriteAlignedDimension((AlignedDimension)dim);
                    break;
                case DimensionType.Linear:
                    this.WriteLinearDimension((LinearDimension)dim);
                    break;
                case DimensionType.Radius:
                    this.WriteRadialDimension((RadialDimension)dim);
                    break;
                case DimensionType.Diameter:
                    this.WriteDiametricDimension((DiametricDimension)dim);
                    break;
                case DimensionType.Angular3Point:
                    this.WriteAngular3PointDimension((Angular3PointDimension)dim);
                    break;
                case DimensionType.Angular:
                    this.WriteAngular2LineDimension((Angular2LineDimension)dim);
                    break;
                case DimensionType.Ordinate:
                    this.WriteOrdinateDimension((OrdinateDimension)dim);
                    break;
                case DimensionType.ArcLength:
                    this.WriteArcLengthDimension((ArcLengthDimension)dim);
                    break;
            }
        }

        private void AddDimensionStyleOverridesXData(XDataDictionary xdata, DimensionStyleOverrideDictionary overrides)
        {
            bool writeDIMPOST = false;
            string prefix = string.Empty;
            string suffix = string.Empty;
            bool writeDIMSAH = false;
            bool writeDIMZIN = false;
            bool writeDIMAZIN = false;
            bool suppressLinearLeadingZeros = false;
            bool suppressLinearTrailingZeros = false;
            bool suppressAngularLeadingZeros = false;
            bool suppressAngularTrailingZeros = false;
            bool suppressZeroFeet = true;
            bool suppressZeroInches = true;

            bool writeDIMALTU = false;
            LinearUnitType altLinearUnitType = LinearUnitType.Decimal;
            bool altStackedUnits = false;
            bool writeDIMAPOST = false;
            string altPrefix = string.Empty;
            string altSuffix = string.Empty;
            bool writeDIMALTZ = false;
            bool altSuppressLinearLeadingZeros = false;
            bool altSuppressLinearTrailingZeros = false;
            bool altSuppressZeroFeet = true;
            bool altSuppressZeroInches = true;

            bool writeDIMTZIN = false;
            bool tolSuppressLinearLeadingZeros = false;
            bool tolSuppressLinearTrailingZeros = false;
            bool tolSuppressZeroFeet = true;
            bool tolSuppressZeroInches = true;

            bool writeDIMALTTZ = false;
            bool tolAltSuppressLinearLeadingZeros = false;
            bool tolAltSuppressLinearTrailingZeros = false;
            bool tolAltSuppressZeroFeet = true;
            bool tolAltSuppressZeroInches = true;

            XData xdataEntry;
            if (xdata.ContainsAppId(ApplicationRegistry.DefaultName))
            {
                xdataEntry = xdata[ApplicationRegistry.DefaultName];
                xdataEntry.XDataRecord.Clear();
            }
            else
            {
                xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.DefaultName));
                xdata.Add(xdataEntry);
            }

            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String, "DSTYLE"));
            xdataEntry.XDataRecord.Add(XDataRecord.OpenControlString);

            foreach (DimensionStyleOverride styleOverride in overrides.Values)
            {
                switch (styleOverride.Type)
                {
                    case DimensionStyleOverrideType.DimLineColor:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 176));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, ((AciColor) styleOverride.Value).Index));
                        break;
                    case DimensionStyleOverrideType.DimLineLinetype:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 345));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Linetype) styleOverride.Value).Handle));
                        break;
                    case DimensionStyleOverrideType.DimLineLineweight:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 371));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (Lineweight) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.DimLine1Off:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 281));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.DimLine2Off:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 282));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.DimLineExtend:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 46));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.ExtLineColor:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 177));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, ((AciColor) styleOverride.Value).Index));
                        break;
                    case DimensionStyleOverrideType.ExtLine1Linetype:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 346));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Linetype) styleOverride.Value).Handle));
                        break;
                    case DimensionStyleOverrideType.ExtLine2Linetype:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 347));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Linetype) styleOverride.Value).Handle));
                        break;
                    case DimensionStyleOverrideType.ExtLineLineweight:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 372));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (Lineweight) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.ExtLine1Off:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 75));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.ExtLine2Off:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 76));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.ExtLineOffset:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 42));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.ExtLineExtend:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 44));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.ExtLineFixed:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 290));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.ExtLineFixedLength:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 49));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.ArrowSize:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 41));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.CenterMarkSize:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 141));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.LeaderArrow:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 341));
                        if (styleOverride.Value != null)
                        {
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Block) styleOverride.Value).Record.Handle));
                        }
                        break;
                    case DimensionStyleOverrideType.DimArrow1:
                        writeDIMSAH = true;
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 343));
                        if (styleOverride.Value != null)
                        {
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Block) styleOverride.Value).Record.Handle));
                        }
                        break;
                    case DimensionStyleOverrideType.DimArrow2:
                        writeDIMSAH = true;
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 344));
                        if (styleOverride.Value != null)
                        {
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((Block) styleOverride.Value).Record.Handle));
                        }
                        break;
                    case DimensionStyleOverrideType.TextStyle:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 340));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.DatabaseHandle, ((TextStyle) styleOverride.Value).Handle));
                        break;
                    case DimensionStyleOverrideType.TextColor:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 178));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, ((AciColor) styleOverride.Value).Index));
                        break;
                    case DimensionStyleOverrideType.TextFillColor:
                        if (styleOverride.Value != null)
                        {
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 70));
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, ((AciColor) styleOverride.Value).Index));

                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 69));
                            xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 2));
                        }
                        break;
                    case DimensionStyleOverrideType.TextHeight:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 140));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TextOffset:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 147));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TextVerticalPlacement:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 77));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleTextVerticalPlacement) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TextHorizontalPlacement:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 280));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleTextHorizontalPlacement) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TextInsideAlign:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 73));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.TextOutsideAlign:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 74));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.TextDirection:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 294));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleTextDirection) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.FitDimLineForce:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 172));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.FitDimLineInside:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 175));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 0 : (short) 1));
                        break;
                    case DimensionStyleOverrideType.DimScaleOverall:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 40));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.FitOptions:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 289));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleFitOptions) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.FitTextInside:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 174));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 1 : (short) 0));
                        break;
                    case DimensionStyleOverrideType.FitTextMove:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 279));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleFitTextMove) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.AngularPrecision:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 179));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.LengthPrecision:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 271));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.DimPrefix:
                        writeDIMPOST = true;
                        prefix = (string) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.DimSuffix:
                        writeDIMPOST = true;
                        suffix = (string) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.DecimalSeparator:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 278));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (char) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.DimScaleLinear:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 144));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.DimLengthUnits:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 277));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (LinearUnitType) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.DimAngularUnits:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 275));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (AngleUnitType) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.FractionalType:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 276));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (FractionFormatType) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.SuppressZeroFeet:
                        writeDIMZIN = true;
                        suppressZeroFeet = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.SuppressZeroInches:
                        writeDIMZIN = true;
                        suppressZeroInches = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.SuppressLinearLeadingZeros:
                        writeDIMZIN = true;
                        suppressLinearLeadingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.SuppressLinearTrailingZeros:
                        writeDIMZIN = true;
                        suppressLinearTrailingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.SuppressAngularLeadingZeros:
                        writeDIMAZIN = true;
                        suppressAngularLeadingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.SuppressAngularTrailingZeros:
                        writeDIMAZIN = true;
                        suppressAngularTrailingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.DimRoundoff:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 45));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.AltUnitsEnabled:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 170));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (bool) styleOverride.Value ? (short) 0 : (short) 1));
                        break;
                    case DimensionStyleOverrideType.AltUnitsLengthUnits:
                        writeDIMALTU = true;
                        altLinearUnitType = (LinearUnitType) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsStackedUnits:
                        altStackedUnits = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsLengthPrecision:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 171));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.AltUnitsMultiplier:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 143));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.AltUnitsRoundoff:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 148));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.AltUnitsPrefix:
                        writeDIMAPOST = true;
                        altPrefix = (string) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsSuffix:
                        writeDIMAPOST = true;
                        altSuffix = (string) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsSuppressLinearLeadingZeros:
                        writeDIMALTZ = true;
                        altSuppressLinearLeadingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsSuppressLinearTrailingZeros:
                        writeDIMALTZ = true;
                        altSuppressLinearTrailingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsSuppressZeroFeet:
                        writeDIMALTZ = true;
                        altSuppressZeroFeet = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.AltUnitsSuppressZeroInches:
                        writeDIMALTZ = true;
                        altSuppressZeroInches = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesDisplayMethod:
                        short dimtol = 0;
                        short dimlin = 0;
                        switch ((DimensionStyleTolerancesDisplayMethod) styleOverride.Value)
                        {
                            case DimensionStyleTolerancesDisplayMethod.None:
                                break;
                            case DimensionStyleTolerancesDisplayMethod.Symmetrical:
                            case DimensionStyleTolerancesDisplayMethod.Deviation:
                                dimtol = 1;
                                break;
                            case DimensionStyleTolerancesDisplayMethod.Limits:
                                dimlin = 1;
                                break;
                        }
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 71));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, dimtol));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 72));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, dimlin));
                        break;
                    case DimensionStyleOverrideType.TolerancesLowerLimit:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 48));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesUpperLimit:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 47));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesVerticalPlacement:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 283));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) (DimensionStyleTolerancesVerticalPlacement) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesPrecision:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 272));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesSuppressLinearLeadingZeros:
                        writeDIMTZIN = true;
                        tolSuppressLinearLeadingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesSuppressLinearTrailingZeros:
                        writeDIMTZIN = true;
                        tolSuppressLinearTrailingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesSuppressZeroFeet:
                        writeDIMTZIN = true;
                        tolSuppressZeroFeet = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesSuppressZeroInches:
                        writeDIMTZIN = true;
                        tolSuppressZeroInches = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TextFractionHeightScale:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 146));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Real, (double) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesAlternatePrecision:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 274));
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) styleOverride.Value));
                        break;
                    case DimensionStyleOverrideType.TolerancesAltSuppressLinearLeadingZeros:
                        writeDIMALTTZ = true;
                        tolAltSuppressLinearLeadingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesAltSuppressLinearTrailingZeros:
                        writeDIMALTTZ = true;
                        tolAltSuppressLinearTrailingZeros = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesAltSuppressZeroFeet:
                        writeDIMALTTZ = true;
                        tolAltSuppressZeroFeet = (bool) styleOverride.Value;
                        break;
                    case DimensionStyleOverrideType.TolerancesAltSuppressZeroInches:
                        writeDIMALTTZ = true;
                        tolAltSuppressZeroInches = (bool) styleOverride.Value;
                        break;
                }
            }

            if (writeDIMSAH)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 173));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 1));
            }

            if (writeDIMPOST)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 3));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String,
                    this.EncodeNonAsciiCharacters(string.Format("{0}<>{1}", prefix, suffix))));
            }

            if (writeDIMZIN)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 78));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                    GetSuppressZeroesValue(suppressLinearLeadingZeros, suppressLinearTrailingZeros, suppressZeroFeet, suppressZeroInches)));
            }

            if (writeDIMAZIN)
            {
                short angSupress = 3;
                if (suppressAngularLeadingZeros && suppressAngularTrailingZeros)
                {
                    angSupress = 3;
                }
                else if (!suppressAngularLeadingZeros && !suppressAngularTrailingZeros)
                {
                    angSupress = 0;
                }
                else if (!suppressAngularLeadingZeros && suppressAngularTrailingZeros)
                {
                    angSupress = 2;
                }
                else if (suppressAngularLeadingZeros && !suppressAngularTrailingZeros)
                {
                    angSupress = 1;
                }

                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 79));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, angSupress));
            }

            // alternate units
            if (writeDIMAPOST)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 4));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.String,
                    this.EncodeNonAsciiCharacters(string.Format("{0}[]{1}", altPrefix, altSuffix))));
            }

            if (writeDIMALTU)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 273));
                switch (altLinearUnitType)
                {
                    case LinearUnitType.Scientific:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 1));
                        break;
                    case LinearUnitType.Decimal:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 2));
                        break;
                    case LinearUnitType.Engineering:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 3));
                        break;
                    case LinearUnitType.Architectural:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                            altStackedUnits ? (short) 4 : (short) 6));
                        break;
                    case LinearUnitType.Fractional:
                        xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                            altStackedUnits ? (short) 5 : (short) 7));
                        break;
                }
            }

            if (writeDIMALTZ)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 285));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                    GetSuppressZeroesValue(altSuppressLinearLeadingZeros, altSuppressLinearTrailingZeros, altSuppressZeroFeet, altSuppressZeroInches)));
            }

            if (writeDIMTZIN)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 284));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                    GetSuppressZeroesValue(tolSuppressLinearLeadingZeros, tolSuppressLinearTrailingZeros, tolSuppressZeroFeet, tolSuppressZeroInches)));
            }

            if (writeDIMALTTZ)
            {
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16, (short) 286));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Int16,
                    GetSuppressZeroesValue(tolAltSuppressLinearLeadingZeros, tolAltSuppressLinearTrailingZeros, tolAltSuppressZeroFeet, tolAltSuppressZeroInches)));
            }

            xdataEntry.XDataRecord.Add(XDataRecord.CloseControlString);
        }

        private void WriteAlignedDimension(AlignedDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.AlignedDimension);

            List<Vector3> wcsPoints = MathHelper.Transform(new[] {dim.FirstReferencePoint, dim.SecondReferencePoint}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.WriteXData(dim.XData);
        }

        private void WriteLinearDimension(LinearDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.AlignedDimension);

            List<Vector3> wcsPoints = MathHelper.Transform(new[] {dim.FirstReferencePoint, dim.SecondReferencePoint}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.chunk.Write(50, dim.Rotation);

            // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the DXF documentation
            // this.chunk.Write(52, dim.ObliqueAngle);

            this.chunk.Write(100, SubclassMarker.LinearDimension);

            this.WriteXData(dim.XData);
        }

        private void WriteRadialDimension(RadialDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.RadialDimension);

            Vector3 wcsPoint = MathHelper.Transform(dim.ReferencePoint, dim.Normal , dim.Elevation);

            this.chunk.Write(15, wcsPoint.X);
            this.chunk.Write(25, wcsPoint.Y);
            this.chunk.Write(35, wcsPoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteDiametricDimension(DiametricDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.DiametricDimension);

            Vector3 wcsPoint = MathHelper.Transform(dim.ReferencePoint, dim.Normal, dim.Elevation);

            this.chunk.Write(15, wcsPoint.X);
            this.chunk.Write(25, wcsPoint.Y);
            this.chunk.Write(35, wcsPoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular3PointDimension(Angular3PointDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Angular3PointDimension);

            List<Vector3> wcsPoints = MathHelper.Transform(new[] {dim.StartPoint, dim.EndPoint, dim.CenterPoint}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.chunk.Write(15, wcsPoints[2].X);
            this.chunk.Write(25, wcsPoints[2].Y);
            this.chunk.Write(35, wcsPoints[2].Z);

            //this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular2LineDimension(Angular2LineDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Angular2LineDimension);

            List<Vector3> wcsPoints = MathHelper.Transform(new[] {dim.StartFirstLine, dim.EndFirstLine, dim.StartSecondLine}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.chunk.Write(15, wcsPoints[2].X);
            this.chunk.Write(25, wcsPoints[2].Y);
            this.chunk.Write(35, wcsPoints[2].Z);

            this.chunk.Write(16, dim.ArcDefinitionPoint.X);
            this.chunk.Write(26, dim.ArcDefinitionPoint.Y);
            this.chunk.Write(36, dim.Elevation);

            //this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteOrdinateDimension(OrdinateDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.OrdinateDimension);

            List<Vector3> wcsPoints = MathHelper.Transform(new[] {dim.FeaturePoint, dim.LeaderEndPoint}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.WriteXData(dim.XData);
        }

        private void WriteArcLengthDimension(ArcLengthDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.ArcDimension);

            Vector2 refStart = Vector2.Polar(dim.CenterPoint, dim.Radius, dim.StartAngle * MathHelper.DegToRad);
            Vector2 refEnd = Vector2.Polar(dim.CenterPoint, dim.Radius, dim.EndAngle * MathHelper.DegToRad);
            List<Vector3> wcsPoints = MathHelper.Transform(new[] {refStart, refEnd, dim.CenterPoint}, dim.Normal, dim.Elevation);

            this.chunk.Write(13, wcsPoints[0].X);
            this.chunk.Write(23, wcsPoints[0].Y);
            this.chunk.Write(33, wcsPoints[0].Z);

            this.chunk.Write(14, wcsPoints[1].X);
            this.chunk.Write(24, wcsPoints[1].Y);
            this.chunk.Write(34, wcsPoints[1].Z);

            this.chunk.Write(15, wcsPoints[2].X);
            this.chunk.Write(25, wcsPoints[2].Y);
            this.chunk.Write(35, wcsPoints[2].Z);

            //this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteImage(Image image)
        {
            this.chunk.Write(100, SubclassMarker.RasterImage);

            this.chunk.Write(10, image.Position.X);
            this.chunk.Write(20, image.Position.Y);
            this.chunk.Write(30, image.Position.Z);

            Vector2 u = image.Uvector * (image.Width / image.Definition.Width);
            Vector2 v = image.Vvector * (image.Height / image.Definition.Height);
            List<Vector3> wcsUV = MathHelper.Transform(new[] {u, v}, image.Normal, 0.0);

            double factor = UnitHelper.ConversionFactor(this.doc.RasterVariables.Units, this.doc.DrawingVariables.InsUnits);

            Vector3 wcsU = wcsUV[0]*factor;
            this.chunk.Write(11, wcsU.X);
            this.chunk.Write(21, wcsU.Y);
            this.chunk.Write(31, wcsU.Z);

            Vector3 wcsV = wcsUV[1]*factor;
            this.chunk.Write(12, wcsV.X);
            this.chunk.Write(22, wcsV.Y);
            this.chunk.Write(32, wcsV.Z);

            this.chunk.Write(13, (double) image.Definition.Width);
            this.chunk.Write(23, (double) image.Definition.Height);

            this.chunk.Write(340, image.Definition.Handle);

            this.chunk.Write(70, (short) image.DisplayOptions);
            this.chunk.Write(280, image.Clipping ? (short) 1 : (short) 0);
            this.chunk.Write(281, image.Brightness);
            this.chunk.Write(282, image.Contrast);
            this.chunk.Write(283, image.Fade);
            this.chunk.Write(360, this.imageDefReactors[image.Definition.Handle][image.Handle].Handle);

            this.chunk.Write(71, (short) image.ClippingBoundary.Type);
            if (image.ClippingBoundary.Type == ClippingBoundaryType.Rectangular)
            {
                this.chunk.Write(91, image.ClippingBoundary.Vertexes.Count);
                foreach (Vector2 vertex in image.ClippingBoundary.Vertexes)
                {
                    this.chunk.Write(14, vertex.X-0.5);
                    this.chunk.Write(24, vertex.Y-0.5);
                }
            }
            else
            {
                // for polygonal clipping boundaries the last vertex must be duplicated
                this.chunk.Write(91, image.ClippingBoundary.Vertexes.Count+1);
                foreach (Vector2 vertex in image.ClippingBoundary.Vertexes)
                {
                    this.chunk.Write(14, vertex.X - 0.5);
                    this.chunk.Write(24, vertex.Y - 0.5);
                }
                this.chunk.Write(14, image.ClippingBoundary.Vertexes[0].X - 0.5);
                this.chunk.Write(24, image.ClippingBoundary.Vertexes[0].Y - 0.5);
            }

            this.WriteXData(image.XData);
        }

        private void WriteMLine(MLine mLine)
        {
            this.chunk.Write(100, SubclassMarker.MLine);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(mLine.Style.Name));

            this.chunk.Write(340, mLine.Style.Handle);

            this.chunk.Write(40, mLine.Scale);
            this.chunk.Write(70, (short) mLine.Justification);
            this.chunk.Write(71, (short) mLine.Flags);
            this.chunk.Write(72, (short) mLine.Vertexes.Count);
            this.chunk.Write(73, (short) mLine.Style.Elements.Count);

            // the MLine information is in OCS we need to save it in WCS
            // this behavior is similar to the LWPolyline, the info is in OCS because these entities are strictly 2d.
            // Normally they are used in the XY plane whose normal is (0, 0, 1) so no transformation is needed, OCS are equal to WCS
            List<Vector2> ocsVertexes = new List<Vector2>();
            foreach (MLineVertex segment in mLine.Vertexes)
            {
                ocsVertexes.Add(segment.Position);
            }
            List<Vector3> vertexes = MathHelper.Transform(ocsVertexes, mLine.Normal, mLine.Elevation);

            // Although it is not recommended the vertex list can have 0 entries
            if (vertexes.Count == 0)
            {
                this.chunk.Write(10, 0.0);
                this.chunk.Write(20, 0.0);
                this.chunk.Write(30, 0.0);
            }
            else
            {
                this.chunk.Write(10, vertexes[0].X);
                this.chunk.Write(20, vertexes[0].Y);
                this.chunk.Write(30, vertexes[0].Z);
            }

            this.chunk.Write(210, mLine.Normal.X);
            this.chunk.Write(220, mLine.Normal.Y);
            this.chunk.Write(230, mLine.Normal.Z);

            for (int i = 0; i < vertexes.Count; i++)
            {
                this.chunk.Write(11, vertexes[i].X);
                this.chunk.Write(21, vertexes[i].Y);
                this.chunk.Write(31, vertexes[i].Z);

                // the direction and miter vectors are written in world coordinates, but do NOT apply the MLINE elevation
                Vector2 dir = mLine.Vertexes[i].Direction;
                Vector3 wcsDir = MathHelper.Transform(dir, mLine.Normal, 0.0);
                this.chunk.Write(12, wcsDir.X);
                this.chunk.Write(22, wcsDir.Y);
                this.chunk.Write(32, wcsDir.Z);
                Vector2 miter = mLine.Vertexes[i].Miter;
                Vector3 wcsMiter = MathHelper.Transform(miter, mLine.Normal, 0.0);
                this.chunk.Write(13, wcsMiter.X);
                this.chunk.Write(23, wcsMiter.Y);
                this.chunk.Write(33, wcsMiter.Z);

                foreach (List<double> distances in mLine.Vertexes[i].Distances)
                {
                    this.chunk.Write(74, (short) distances.Count);
                    foreach (double distance in distances)
                    {
                        this.chunk.Write(41, distance);
                    }
                    this.chunk.Write(75, (short) 0);
                }
            }

            this.WriteXData(mLine.XData);
        }

        private void WriteAttributeDefinition(AttributeDefinition def, Layout layout)
        {
            this.chunk.Write(0, def.CodeName);
            this.chunk.Write(5, def.Handle);

            //if (def.Reactors.Count > 0)
            //{
            //    this.chunk.Write(102, "{ACAD_REACTORS");
            //    foreach (DxfObject o in def.Reactors)
            //    {
            //        if (!string.IsNullOrEmpty(o.Handle)) this.chunk.Write(330, o.Handle);
            //    }
            //    this.chunk.Write(102, "}");
            //}

            this.chunk.Write(330, def.Owner.Record.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            if (layout != null)
            {
                this.chunk.Write(67, layout.IsPaperSpace ? (short)1 : (short)0);
            }

            this.chunk.Write(8, this.EncodeNonAsciiCharacters(def.Layer.Name));

            this.chunk.Write(62, def.Color.Index);
            if (def.Color.UseTrueColor)
            {
                this.chunk.Write(420, AciColor.ToTrueColor(def.Color));
            }

            if (def.Transparency.Value >= 0)
            {
                this.chunk.Write(440, Transparency.ToAlphaValue(def.Transparency));
            }

            this.chunk.Write(6, this.EncodeNonAsciiCharacters(def.Linetype.Name));

            this.chunk.Write(370, (short)def.Lineweight);
            this.chunk.Write(48, def.LinetypeScale);
            this.chunk.Write(60, def.IsVisible ? (short)0 : (short)1);

            this.chunk.Write(100, SubclassMarker.Text);

            Vector3 ocsBasePoint = MathHelper.Transform(def.Position, def.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsBasePoint.X);
            this.chunk.Write(20, ocsBasePoint.Y);
            this.chunk.Write(30, ocsBasePoint.Z);

            this.chunk.Write(40, def.Height);

            this.chunk.Write(1, this.EncodeNonAsciiCharacters(def.Value));

            this.chunk.Write(50, def.Rotation);
            this.chunk.Write(51, def.ObliqueAngle);
            this.chunk.Write(41, def.WidthFactor);

            this.chunk.Write(7, this.EncodeNonAsciiCharacters(def.Style.Name));

            if (def.Alignment == TextAlignment.Fit || def.Alignment == TextAlignment.Aligned)
            {
                Vector2 endPoint = Vector2.Rotate(def.Width * Vector2.UnitX, def.Rotation * MathHelper.DegToRad);
                Vector3 ocsEndPoint = ocsBasePoint + new Vector3(endPoint.X, endPoint.Y, 0.0);
                this.chunk.Write(11, ocsEndPoint.X);
                this.chunk.Write(21, ocsEndPoint.Y);
                this.chunk.Write(31, ocsEndPoint.Z);
            }
            else
            {
                this.chunk.Write(11, ocsBasePoint.X);
                this.chunk.Write(21, ocsBasePoint.Y);
                this.chunk.Write(31, ocsBasePoint.Z);
            }

            this.chunk.Write(210, def.Normal.X);
            this.chunk.Write(220, def.Normal.Y);
            this.chunk.Write(230, def.Normal.Z);

            short textGeneration = 0;
            if (def.IsBackward)
            {
                textGeneration += 2;
            }

            if (def.IsUpsideDown)
            {
                textGeneration += 4;
            }

            this.chunk.Write(71, textGeneration);
            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short) 3);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(72, (short) 4);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(72, (short) 5);
                    this.chunk.Write(100, SubclassMarker.AttributeDefinition);
                    this.chunk.Write(74, (short) 0);
                    break;
            }

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(def.Tag));
            this.chunk.Write(3, this.EncodeNonAsciiCharacters(def.Prompt));
            this.chunk.Write(70, (short) def.Flags);

            this.WriteXData(def.XData);
        }

        private void WriteAttribute(Attribute attrib)
        {
            this.chunk.Write(0, attrib.CodeName);
            this.chunk.Write(5, attrib.Handle);

            this.chunk.Write(330, attrib.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            this.chunk.Write(8, this.EncodeNonAsciiCharacters(attrib.Layer.Name));

            this.chunk.Write(62, attrib.Color.Index);
            if (attrib.Color.UseTrueColor)
            {
                this.chunk.Write(420, AciColor.ToTrueColor(attrib.Color));
            }

            if (attrib.Transparency.Value >= 0)
            {
                this.chunk.Write(440, Transparency.ToAlphaValue(attrib.Transparency));
            }

            this.chunk.Write(6, this.EncodeNonAsciiCharacters(attrib.Linetype.Name));

            this.chunk.Write(370, (short) attrib.Lineweight);
            this.chunk.Write(48, attrib.LinetypeScale);
            this.chunk.Write(60, attrib.IsVisible ? (short) 0 : (short) 1);

            this.chunk.Write(100, SubclassMarker.Text);

            Vector3 ocsBasePoint = MathHelper.Transform(attrib.Position, attrib.Normal, CoordinateSystem.World, CoordinateSystem.Object);

            this.chunk.Write(10, ocsBasePoint.X);
            this.chunk.Write(20, ocsBasePoint.Y);
            this.chunk.Write(30, ocsBasePoint.Z);

            this.chunk.Write(40, attrib.Height);
            this.chunk.Write(41, attrib.WidthFactor);

            this.chunk.Write(7, this.EncodeNonAsciiCharacters(attrib.Style.Name));

            this.chunk.Write(1, this.EncodeNonAsciiCharacters(attrib.Value));
            
            if (attrib.Alignment == TextAlignment.Fit || attrib.Alignment == TextAlignment.Aligned)
            {
                Vector2 endPoint = Vector2.Rotate(attrib.Width * Vector2.UnitX, attrib.Rotation * MathHelper.DegToRad);
                Vector3 ocsEndPoint = ocsBasePoint + new Vector3(endPoint.X, endPoint.Y, 0.0);
                this.chunk.Write(11, ocsEndPoint.X);
                this.chunk.Write(21, ocsEndPoint.Y);
                this.chunk.Write(31, ocsEndPoint.Z);
            }
            else
            {
                this.chunk.Write(11, ocsBasePoint.X);
                this.chunk.Write(21, ocsBasePoint.Y);
                this.chunk.Write(31, ocsBasePoint.Z);
            }

            this.chunk.Write(50, attrib.Rotation);
            this.chunk.Write(51, attrib.ObliqueAngle);

            this.chunk.Write(210, attrib.Normal.X);
            this.chunk.Write(220, attrib.Normal.Y);
            this.chunk.Write(230, attrib.Normal.Z);

            short textGeneration = 0;
            if (attrib.IsBackward)
            {
                textGeneration += 2;
            }

            if (attrib.IsUpsideDown)
            {
                textGeneration += 4;
            }

            this.chunk.Write(71, textGeneration);

            switch (attrib.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short) 0);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short) 1);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short) 2);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short) 3);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(72, (short) 4);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(72, (short) 5);
                    this.chunk.Write(100, SubclassMarker.Attribute);
                    this.chunk.Write(74, (short) 0);
                    break;
            }

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(attrib.Tag));
            this.chunk.Write(70, (short) attrib.Flags);

            this.WriteXData(attrib.XData);
        }

        private void WriteViewport(Viewport vp)
        {
            this.chunk.Write(100, SubclassMarker.Viewport);

            this.chunk.Write(10, vp.Center.X);
            this.chunk.Write(20, vp.Center.Y);
            this.chunk.Write(30, vp.Center.Z);

            this.chunk.Write(40, vp.Width);
            this.chunk.Write(41, vp.Height);
            this.chunk.Write(68, vp.Stacking);
            this.chunk.Write(69, vp.Id);

            this.chunk.Write(12, vp.ViewCenter.X);
            this.chunk.Write(22, vp.ViewCenter.Y);

            this.chunk.Write(13, vp.SnapBase.X);
            this.chunk.Write(23, vp.SnapBase.Y);

            this.chunk.Write(14, vp.SnapSpacing.X);
            this.chunk.Write(24, vp.SnapSpacing.Y);

            this.chunk.Write(15, vp.GridSpacing.X);
            this.chunk.Write(25, vp.GridSpacing.Y);

            this.chunk.Write(16, vp.ViewDirection.X);
            this.chunk.Write(26, vp.ViewDirection.Y);
            this.chunk.Write(36, vp.ViewDirection.Z);

            this.chunk.Write(17, vp.ViewTarget.X);
            this.chunk.Write(27, vp.ViewTarget.Y);
            this.chunk.Write(37, vp.ViewTarget.Z);

            this.chunk.Write(42, vp.LensLength);

            this.chunk.Write(43, vp.FrontClipPlane);
            this.chunk.Write(44, vp.BackClipPlane);
            this.chunk.Write(45, vp.ViewHeight);

            this.chunk.Write(50, vp.SnapAngle);
            this.chunk.Write(51, vp.TwistAngle);
            this.chunk.Write(72, vp.CircleZoomPercent);

            foreach (Layer layer in vp.FrozenLayers)
            {
                this.chunk.Write(331, layer.Handle);
            }

            this.chunk.Write(90, (int) vp.Status);

            if (vp.ClippingBoundary != null)
            {
                this.chunk.Write(340, vp.ClippingBoundary.Handle);
            }

            this.chunk.Write(110, vp.UcsOrigin.X);
            this.chunk.Write(120, vp.UcsOrigin.Y);
            this.chunk.Write(130, vp.UcsOrigin.Z);

            this.chunk.Write(111, vp.UcsXAxis.X);
            this.chunk.Write(121, vp.UcsXAxis.Y);
            this.chunk.Write(131, vp.UcsXAxis.Z);

            this.chunk.Write(112, vp.UcsYAxis.X);
            this.chunk.Write(122, vp.UcsYAxis.Y);
            this.chunk.Write(132, vp.UcsYAxis.Z);

            this.WriteXData(vp.XData);
        }

        #endregion

        #region methods for Object section

        private void WriteDictionary(DictionaryObject dictionary)
        {
            this.chunk.Write(0, DxfObjectCode.Dictionary);
            this.chunk.Write(5, dictionary.Handle);
            this.chunk.Write(330, dictionary.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Dictionary);
            this.chunk.Write(280, dictionary.IsHardOwner ? (short) 1 : (short) 0);
            this.chunk.Write(281, (short) dictionary.Cloning);

            if (dictionary.Entries == null)
            {
                this.WriteXData(dictionary.XData);
                return;
            }

            foreach (KeyValuePair<string, string> entry in dictionary.Entries)
            {
                this.chunk.Write(3, this.EncodeNonAsciiCharacters(entry.Value));
                this.chunk.Write(entry.Value.Equals(DxfObjectCode.LayerStates, StringComparison.InvariantCultureIgnoreCase) ? (short) 360 : (short) 350, entry.Key);
            }

            this.WriteXData(dictionary.XData);
        }

        private void WriteUnderlayDefinition(UnderlayDefinition underlayDef, string ownerHandle)
        {
            this.chunk.Write(0, underlayDef.CodeName);
            this.chunk.Write(5, underlayDef.Handle);
            this.chunk.Write(102, "{ACAD_REACTORS");
            List<DxfObject> objects = null;
            switch (underlayDef.Type)
            {
                case UnderlayType.DGN:
                    objects = this.doc.UnderlayDgnDefinitions.References[underlayDef.Name];
                    break;
                case UnderlayType.DWF:
                    objects = this.doc.UnderlayDwfDefinitions.References[underlayDef.Name];
                    break;
                case UnderlayType.PDF:
                    objects = this.doc.UnderlayPdfDefinitions.References[underlayDef.Name];
                    break;
            }

            if (objects == null)
            {
                throw new NullReferenceException("Underlay references list cannot be null");
            }
            foreach (DxfObject o in objects)
            {
                if (o is Underlay underlay)
                {
                    this.chunk.Write(330, underlay.Handle);
                }
            }
            this.chunk.Write(102, "}");
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.UnderlayDefinition);
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(underlayDef.File));
            switch (underlayDef.Type)
            {
                case UnderlayType.DGN:
                    this.chunk.Write(2, this.EncodeNonAsciiCharacters(((UnderlayDgnDefinition) underlayDef).Layout));
                    break;
                case UnderlayType.DWF:
                    this.chunk.Write(2, string.Empty);
                    break;
                case UnderlayType.PDF:
                    this.chunk.Write(2, this.EncodeNonAsciiCharacters(((UnderlayPdfDefinition) underlayDef).Page));
                    break;
            }

            this.WriteXData(underlayDef.XData);
        }

        private void WriteImageDefReactor(ImageDefinitionReactor reactor)
        {
            this.chunk.Write(0, reactor.CodeName);
            this.chunk.Write(5, reactor.Handle);
            this.chunk.Write(330, reactor.ImageHandle);

            this.chunk.Write(100, SubclassMarker.RasterImageDefReactor);
            this.chunk.Write(90, 2);
            this.chunk.Write(330, reactor.ImageHandle);
        }

        private void WriteImageDef(ImageDefinition imageDefinition, string ownerHandle)
        {
            this.chunk.Write(0, imageDefinition.CodeName);
            this.chunk.Write(5, imageDefinition.Handle);

            this.chunk.Write(102, "{ACAD_REACTORS");
            this.chunk.Write(330, ownerHandle);
            foreach (ImageDefinitionReactor reactor in this.imageDefReactors[imageDefinition.Handle].Values)
            {
                this.chunk.Write(330, reactor.Handle);
            }
            this.chunk.Write(102, "}");

            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.RasterImageDef);
            this.chunk.Write(1, imageDefinition.File);

            this.chunk.Write(10, (double) imageDefinition.Width);
            this.chunk.Write(20, (double) imageDefinition.Height);

            // The documentation says that this is the size of one pixel in AutoCAD units, but it seems that this is always the size of one pixel in millimeters
            // this value is used to calculate the image resolution in PPI or PPC, and the default image size.
            double factor = UnitHelper.ConversionFactor((ImageUnits) imageDefinition.ResolutionUnits, DrawingUnits.Millimeters);
            this.chunk.Write(11, factor/imageDefinition.HorizontalResolution);
            this.chunk.Write(21, factor/imageDefinition.VerticalResolution);

            this.chunk.Write(280, (short) 1);
            this.chunk.Write(281, (short) imageDefinition.ResolutionUnits);

            this.WriteXData(imageDefinition.XData);
        }

        private void WriteRasterVariables(RasterVariables variables, string ownerHandle)
        {
            this.chunk.Write(0, variables.CodeName);
            this.chunk.Write(5, variables.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.RasterVariables);
            this.chunk.Write(90, 0);
            this.chunk.Write(70, variables.DisplayFrame ? (short) 1 : (short) 0);
            this.chunk.Write(71, (short) variables.DisplayQuality);
            this.chunk.Write(72, (short) variables.Units);

            this.WriteXData(variables.XData);
        }

        private void WriteMLineStyle(MLineStyle style, string ownerHandle)
        {
            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(5, style.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.MLineStyle);

            this.chunk.Write(2, this.EncodeNonAsciiCharacters(style.Name));

            this.chunk.Write(70, (short) style.Flags);

            this.chunk.Write(3, this.EncodeNonAsciiCharacters(style.Description));

            this.chunk.Write(62, style.FillColor.Index);
            if (style.FillColor.UseTrueColor) // && this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
            {
                this.chunk.Write(420, AciColor.ToTrueColor(style.FillColor));
            }
            this.chunk.Write(51, style.StartAngle);
            this.chunk.Write(52, style.EndAngle);
            this.chunk.Write(71, (short) style.Elements.Count);
            foreach (MLineStyleElement element in style.Elements)
            {
                this.chunk.Write(49, element.Offset);
                this.chunk.Write(62, element.Color.Index);
                if (element.Color.UseTrueColor) // && this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
                {
                    this.chunk.Write(420, AciColor.ToTrueColor(element.Color));
                }

                this.chunk.Write(6, this.EncodeNonAsciiCharacters(element.Linetype.Name));
            }

            this.WriteXData(style.XData);
        }

        private void WriteGroup(Group group, string ownerHandle)
        {
            this.chunk.Write(0, group.CodeName);
            this.chunk.Write(5, group.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.Group);

            this.chunk.Write(300, this.EncodeNonAsciiCharacters(group.Description));
            this.chunk.Write(70, group.IsUnnamed ? (short) 1 : (short) 0);
            this.chunk.Write(71, group.IsSelectable ? (short) 1 : (short) 0);

            foreach (EntityObject entity in group.Entities)
            {
                this.chunk.Write(340, entity.Handle);
            }

            this.WriteXData(group.XData);
        }

        private void WriteLayout(Layout layout, string ownerHandle)
        {
            this.chunk.Write(0, layout.CodeName);
            this.chunk.Write(5, layout.Handle);
            this.chunk.Write(330, ownerHandle);

            this.WritePlotSettings(layout.PlotSettings);

            this.chunk.Write(100, SubclassMarker.Layout);
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(layout.Name));
            //this.chunk.Write(70, (short) 1);
            this.chunk.Write(71, layout.TabOrder);

            this.chunk.Write(10, layout.MinLimit.X);
            this.chunk.Write(20, layout.MinLimit.Y);
            this.chunk.Write(11, layout.MaxLimit.X);
            this.chunk.Write(21, layout.MaxLimit.Y);

            this.chunk.Write(12, layout.BasePoint.X);
            this.chunk.Write(22, layout.BasePoint.Y);
            this.chunk.Write(32, layout.BasePoint.Z);

            this.chunk.Write(14, layout.MinExtents.X);
            this.chunk.Write(24, layout.MinExtents.Y);
            this.chunk.Write(34, layout.MinExtents.Z);

            this.chunk.Write(15, layout.MaxExtents.X);
            this.chunk.Write(25, layout.MaxExtents.Y);
            this.chunk.Write(35, layout.MaxExtents.Z);

            this.chunk.Write(146, layout.Elevation);

            this.chunk.Write(13, layout.UcsOrigin.X);
            this.chunk.Write(23, layout.UcsOrigin.Y);
            this.chunk.Write(33, layout.UcsOrigin.Z);


            this.chunk.Write(16, layout.UcsXAxis.X);
            this.chunk.Write(26, layout.UcsXAxis.Y);
            this.chunk.Write(36, layout.UcsXAxis.Z);

            this.chunk.Write(17, layout.UcsYAxis.X);
            this.chunk.Write(27, layout.UcsYAxis.Y);
            this.chunk.Write(37, layout.UcsYAxis.Z);

            this.chunk.Write(76, (short) 0);

            this.chunk.Write(330, layout.AssociatedBlock.Owner.Handle);

            this.WriteXData(layout.XData);
        }

        private void WritePlotSettings(PlotSettings plot)
        {
            this.chunk.Write(100, SubclassMarker.PlotSettings);
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(plot.PageSetupName));
            this.chunk.Write(2, this.EncodeNonAsciiCharacters(plot.PlotterName));
            this.chunk.Write(4, this.EncodeNonAsciiCharacters(plot.PaperSizeName));
            this.chunk.Write(6, this.EncodeNonAsciiCharacters(plot.ViewName));

            this.chunk.Write(40, plot.PaperMargin.Left);
            this.chunk.Write(41, plot.PaperMargin.Bottom);
            this.chunk.Write(42, plot.PaperMargin.Right);
            this.chunk.Write(43, plot.PaperMargin.Top);
            this.chunk.Write(44, plot.PaperSize.X);
            this.chunk.Write(45, plot.PaperSize.Y);
            this.chunk.Write(46, plot.Origin.X);
            this.chunk.Write(47, plot.Origin.Y);
            this.chunk.Write(48, plot.WindowBottomLeft.X);
            this.chunk.Write(49, plot.WindowUpRight.X);
            this.chunk.Write(140, plot.WindowBottomLeft.Y);
            this.chunk.Write(141, plot.WindowUpRight.Y);
            this.chunk.Write(142, plot.PrintScaleNumerator);
            this.chunk.Write(143, plot.PrintScaleDenominator);

            this.chunk.Write(70, (short) plot.Flags);
            this.chunk.Write(72, (short) plot.PaperUnits);
            this.chunk.Write(73, (short) plot.PaperRotation);
            this.chunk.Write(74, (short) plot.PlotType);

            this.chunk.Write(7, this.EncodeNonAsciiCharacters(plot.CurrentStyleSheet));
            this.chunk.Write(75, plot.ScaleToFit ? (short) 0 : (short) 16);

            this.chunk.Write(76, (short) plot.ShadePlotMode);
            this.chunk.Write(77, (short) plot.ShadePlotResolutionMode);
            this.chunk.Write(78, plot.ShadePlotDPI);
            this.chunk.Write(147, plot.PrintScale);

            this.chunk.Write(148, plot.PaperImageOrigin.X);
            this.chunk.Write(149, plot.PaperImageOrigin.Y);
        }

        private void WriteLayerState(LayerState layerState, string ownerHandle)
        {
            this.chunk.Write(0, DxfObjectCode.XRecord);
            this.chunk.Write(5, layerState.Handle);

            // for who-knows-why reason the ACAD_REACTORS thing is necessary, it will not work without it
            // even though there is already a separated 330 code that stores the same information
            // and most of the time it is not necessary in similar cases
            // since Autodesk doesn't know how to document its own crap consider everything I say about the DXF format as a guess
            this.chunk.Write(102, "{ACAD_REACTORS");
            this.chunk.Write(330, ownerHandle);
            this.chunk.Write(102, "}");
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.XRecord);
            this.chunk.Write(280, (short) 1); // Duplicate record cloning flag
            this.chunk.Write(91, 2047); // unknown code functionality <- 32-bit integer value
            this.chunk.Write(301, this.EncodeNonAsciiCharacters(layerState.Description));
            this.chunk.Write(290, layerState.PaperSpace);
            this.chunk.Write(302, this.EncodeNonAsciiCharacters(layerState.CurrentLayer));

            foreach (LayerStateProperties properties in layerState.Properties.Values)
            {
                this.WriteLayerStateProperties(properties);
            }
        }

        private void WriteLayerStateProperties(LayerStateProperties properties)
        {
            // both options seems to work storing the handle (code 330) or the name (code 8) of the layer
            this.chunk.Write(330, this.doc.Layers[properties.Name].Handle);
            //this.chunk.Write(8, this.EncodeNonAsciiCharacters(properties.Name));

            this.chunk.Write(90, (int) properties.Flags);
            this.chunk.Write(62, properties.Color.Index);
            this.chunk.Write(370, (short) properties.Lineweight);

            //this.chunk.Write(6, properties.LinetypeName);
            this.chunk.Write(331, this.doc.Linetypes[properties.LinetypeName].Handle);

            //this.chunk.Write(1, properties.PlotStyleName);
            this.chunk.Write(440, properties.Transparency.Value == 0 ? 0 : Transparency.ToAlphaValue(properties.Transparency));

            if (properties.Color.UseTrueColor)
            {
                // this code only appears if the layer color has been defined as true color
                this.chunk.Write(92, AciColor.ToTrueColor(properties.Color));
            }
        }

        #endregion

        #region private methods

        private void PreprocessEntities()
        {
            foreach (Block block in this.doc.Blocks)
            {
                foreach (EntityObject entity in block.Entities)
                {
                    switch (entity.Type)
                    {
                        case EntityType.Polyline3D:
                            this.PreProcessPolyline3D(entity as Polyline3D);
                            break;
                        case EntityType.Polyline2D:
                            this.PreProcessPolyline2D(entity as Polyline2D);
                            break;
                        case EntityType.PolyfaceMesh:
                            this.PreProcessPolyfaceMesh(entity as PolyfaceMesh);
                            break;
                        case EntityType.PolygonMesh:
                            this.PreProcessPolygonMesh(entity as PolygonMesh);
                            break;
                        case EntityType.Insert:
                            this.PreprocessInserts(entity as Insert);
                            break;
                    }
                }
            }
        }

        private void PreProcessPolyline3D(Polyline3D poly3D)
        {
            List<Vertex> vertexes = new List<Vertex>();

            // first create the polyline vertexes
            foreach (Vector3 vertex in poly3D.Vertexes)
            {
                Vertex v = new Vertex
                {
                    Position = vertex,
                    Owner = poly3D,
                    Flags = poly3D.SmoothType != PolylineSmoothType.NoSmooth ? VertexTypeFlags.SplineFrameControlPoint | VertexTypeFlags.Polyline3DVertex : VertexTypeFlags.Polyline3DVertex,
                    SubclassMarker = SubclassMarker.Polyline3DVertex
                };
                this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                vertexes.Add(v);
            }

            // if the polyline is smooth then create the additional vertexes
            if (poly3D.SmoothType != PolylineSmoothType.NoSmooth)
            {
                short splineSegs = this.doc.DrawingVariables.SplineSegs;
                int precision = poly3D.IsClosed ? splineSegs * poly3D.Vertexes.Count : splineSegs * (poly3D.Vertexes.Count - 1);
                List<Vector3> points = poly3D.PolygonalVertexes(precision);
                foreach (Vector3 p in points)
                {
                    Vertex v = new Vertex
                    {
                        Position = p,
                        Owner = poly3D,
                        Flags = VertexTypeFlags.SplineVertexFromSplineFitting | VertexTypeFlags.Polyline3DVertex,
                        SubclassMarker = SubclassMarker.Polyline3DVertex
                    };
                    this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                    vertexes.Add(v);
                }
            }

            EndSequence endSequence = new EndSequence
            {
                Owner = poly3D
            };
            this.doc.NumHandles = endSequence.AssignHandle(this.doc.NumHandles);

            Polyline polyline = new Polyline
            {
                Handle = poly3D.Handle,
                SubclassMarker = SubclassMarker.Polyline3D,
                Layer = poly3D.Layer,
                Normal = poly3D.Normal,
                Color = poly3D.Color,
                EndSequence = endSequence,
                Vertexes = vertexes,
                Flags = poly3D.Flags,
                SmoothType = poly3D.SmoothType
            };

            polyline.XData.AddRange(poly3D.XData.Values);

            this.polylines.Add(polyline.Handle, polyline);
        }

        private void PreProcessPolyline2D(Polyline2D poly2D)
        {
            // only for smoothed polylines
            if (poly2D.SmoothType == PolylineSmoothType.NoSmooth)
            {
                return;
            }

            // smoothed polyline2D can only have a constant width, the first vertex width will be used.
            double width = poly2D.Vertexes[0].StartWidth;

            List<Vertex> vertexes = new List<Vertex>();

            // first create the polyline vertexes
            foreach (Polyline2DVertex vertex in poly2D.Vertexes)
            {
                Vertex v = new Vertex
                {
                    Position = new Vector3(vertex.Position.X, vertex.Position.Y, poly2D.Elevation),
                    Owner = poly2D,
                    StartWidth = width,
                    EndWidth = width,
                    Flags = VertexTypeFlags.SplineFrameControlPoint,
                    SubclassMarker = SubclassMarker.Polyline2DVertex
                };
                this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                vertexes.Add(v);
            }

            // if the polyline is smooth then create the additional vertexes
            short splineSegs = this.doc.DrawingVariables.SplineSegs;
            int precision = poly2D.IsClosed ? splineSegs * poly2D.Vertexes.Count : splineSegs * (poly2D.Vertexes.Count - 1);
            List<Vector2> points = poly2D.PolygonalVertexes(precision);
            foreach (Vector2 p in points)
            {
                Vertex v = new Vertex
                {
                    Position = new Vector3(p.X, p.Y, poly2D.Elevation),
                    Owner = poly2D,
                    StartWidth = width,
                    EndWidth = width,
                    Flags = VertexTypeFlags.SplineVertexFromSplineFitting,
                    SubclassMarker = SubclassMarker.Polyline2DVertex
                };
                this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                vertexes.Add(v);
            }

            EndSequence endSequence = new EndSequence
            {
                Owner = poly2D
            };
            this.doc.NumHandles = endSequence.AssignHandle(this.doc.NumHandles);

            Polyline polyline = new Polyline
            {
                Handle = poly2D.Handle,
                SubclassMarker = SubclassMarker.Polyline2D,
                Layer = poly2D.Layer,
                Elevation = poly2D.Elevation,
                Normal = poly2D.Normal,
                Color = poly2D.Color,
                EndSequence = endSequence,
                Vertexes = vertexes,
                Flags = poly2D.Flags,
                SmoothType = poly2D.SmoothType
            };

            polyline.XData.AddRange(poly2D.XData.Values);

            this.polylines.Add(polyline.Handle, polyline);
        }

        private void PreProcessPolyfaceMesh(PolyfaceMesh pMesh)
        {
            List<Vertex> vertexes = new List<Vertex>();

            // first create the polyface mesh vertexes
            foreach (Vector3 vertex in pMesh.Vertexes)
            {
                Vertex v = new Vertex
                {
                    Position = vertex,
                    Owner = pMesh,
                    Flags = VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3DMeshVertex,
                    SubclassMarker = SubclassMarker.PolyfaceMeshVertex
                };
                this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                vertexes.Add(v);
            }

            // second create the polyface mesh faces
            foreach (PolyfaceMeshFace face in pMesh.Faces)
            {
                Vertex v = new Vertex
                {
                    Owner = pMesh,
                    Layer = face.Layer,
                    Color = face.Color,
                    VertexIndexes = face.VertexIndexes,
                    Flags = VertexTypeFlags.PolyfaceMeshVertex,
                    SubclassMarker = SubclassMarker.PolyfaceMeshFace
                };
                this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                vertexes.Add(v);
            }

            EndSequence endSequence = new EndSequence
            {
                Owner = pMesh
            };
            this.doc.NumHandles = endSequence.AssignHandle(this.doc.NumHandles);

            Polyline polyline = new Polyline
            {
                Handle = pMesh.Handle,
                SubclassMarker = SubclassMarker.PolyfaceMesh,
                Layer = pMesh.Layer,
                Normal = pMesh.Normal,
                Color = pMesh.Color,
                EndSequence = endSequence,
                Vertexes = vertexes,
                Flags = pMesh.Flags,
            };

            polyline.XData.AddRange(pMesh.XData.Values);

            this.polylines.Add(pMesh.Handle, polyline);
        }

        private void PreProcessPolygonMesh(PolygonMesh pMesh)
        {
            List<Vertex> vertexes = new List<Vertex>();
            short precisionU = 0;
            short precisionV = 0;

            // first create the polygon mesh vertexes
            for (int i = 0; i < pMesh.U; i++)
            {
                for (int j = 0; j < pMesh.V; j++)
                {
                    Vertex v = new Vertex
                    {
                        Position = pMesh.Vertexes[i + j * pMesh.U],
                        Owner = pMesh,
                        Flags = pMesh.SmoothType != PolylineSmoothType.NoSmooth ? VertexTypeFlags.SplineFrameControlPoint | VertexTypeFlags.Polygon3DMeshVertex : VertexTypeFlags.Polygon3DMeshVertex,
                        SubclassMarker = SubclassMarker.PolygonMeshVertex
                    };
                    this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                    vertexes.Add(v);
                }
            }

            if (pMesh.SmoothType != PolylineSmoothType.NoSmooth)
            {
                precisionU = pMesh.DensityU == 0 ? (short) (this.doc.DrawingVariables.SurfU + 1) : pMesh.DensityU;
                precisionV = pMesh.DensityV == 0 ? (short) (this.doc.DrawingVariables.SurfV + 1) : pMesh.DensityV;

                // the minimum vertexes generated is 3.
                if (precisionU < 3)
                {
                    precisionU = 3;
                }
                if (precisionV < 3)
                {
                    precisionV = 3;
                }

                List<Vector3> points = pMesh.MeshVertexes(precisionU, precisionV);
                for (int i = 0; i < precisionU; i++)
                {
                    for (int j = 0; j < precisionV; j++)
                    {
                        Vertex v = new Vertex
                        {
                            Position = points[i + j * precisionU],
                            Owner = pMesh,
                            Flags = VertexTypeFlags.SplineVertexFromSplineFitting | VertexTypeFlags.Polygon3DMeshVertex,
                            SubclassMarker = SubclassMarker.PolygonMeshVertex
                        };
                        this.doc.NumHandles = v.AssignHandle(this.doc.NumHandles);
                        vertexes.Add(v);
                    }
                }
            }

            EndSequence endSequence = new EndSequence
            {
                Owner = pMesh
            };
            this.doc.NumHandles = endSequence.AssignHandle(this.doc.NumHandles);

            Polyline polyline = new Polyline
            {
                Handle = pMesh.Handle,
                SubclassMarker = SubclassMarker.PolygonMesh,
                Layer = pMesh.Layer,
                Normal = pMesh.Normal,
                Color = pMesh.Color,
                EndSequence = endSequence,
                Vertexes = vertexes,
                Flags = pMesh.Flags,
                SmoothType = pMesh.SmoothType,
                M = pMesh.U,
                N = pMesh.V,
                DensityM = precisionU,
                DensityN = precisionV
            };

            polyline.XData.AddRange(pMesh.XData.Values);

            this.polylines.Add(pMesh.Handle, polyline);
        }

        private void PreprocessInserts(Insert insert)
        {
            EndSequence endSequence = new EndSequence
            {
                Owner = insert
            };
            this.doc.NumHandles = endSequence.AssignHandle(this.doc.NumHandles);

            this.insertEndSequences.Add(insert.Handle, endSequence);
        }

        private void PreprocessImageDefReactors()
        {
            foreach (ImageDefinition imageDef in this.doc.ImageDefinitions)
            {
                if (!this.imageDefReactors.ContainsKey(imageDef.Handle))
                {
                    this.imageDefReactors.Add(imageDef.Handle, new Dictionary<string, ImageDefinitionReactor>());
                }

                List<DxfObject> images = this.doc.ImageDefinitions.References[imageDef.Name];
                foreach (DxfObject o in images)
                {
                    // only Image entities are referenced by an ImageDefinition
                    Debug.Assert(o is Image, "Only Image entities can be referenced by an ImageDefinition.");
                    Image image = (Image) o;
                    Dictionary<string, ImageDefinitionReactor> reactors = this.imageDefReactors[image.Definition.Handle];
                    ImageDefinitionReactor reactor = new ImageDefinitionReactor(image.Handle);
                    this.doc.NumHandles = reactor.AssignHandle(this.doc.NumHandles);
                    reactors.Add(image.Handle, reactor);
                }
            }
        }

        private static short GetSuppressZeroesValue(bool leading, bool trailing, bool feet, bool inches)
        {
            short rtn = 0;
            if (feet && inches)
            {
                rtn = 0;
            }
            if (!feet && !inches)
            {
                rtn += 1;
            }
            if (!feet && inches)
            {
                rtn += 2;
            }
            if (feet && !inches)
            {
                rtn += 3;
            }

            if (!leading && !trailing)
            {
                rtn += 0;
            }
            if (leading && !trailing)
            {
                rtn += 4;
            }
            if (!leading && trailing)
            {
                rtn += 8;
            }
            if (leading && trailing)
            {
                rtn += 12;
            }

            return rtn;
        }

        private string EncodeNonAsciiCharacters(string text)
        {
            // for DXF database version prior to AutoCad 2007 non ASCII characters must be encoded to the template \U+####,
            // where #### is the for digits hexadecimal number that represent that character.
            if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2007)
            {
                return text;
            }

            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (this.encodedStrings.TryGetValue(text, out string encoded))
            {
                return encoded;
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 127)
                {
                    sb.Append(string.Concat("\\U+", string.Format("{0:X4}", Convert.ToInt32(c))));
                }
                else
                {
                    sb.Append(c);
                }
            }

            encoded = sb.ToString();
            this.encodedStrings.Add(text, encoded);
            return encoded;

            // encoding of non ASCII characters, including the extended chart, using regular expressions, this code is slower
            //return Regex.Replace(
            //    text,
            //    @"(?<char>[^\u0000-\u00ff]{1})",
            //    m => "\\U+" + string.Format("{0:X4}", Convert.ToInt32(m.Groups["char"].Value[0])));
        }

        private void WriteXData(XDataDictionary xData)
        {
            foreach (string appReg in xData.AppIds)
            {
                this.chunk.Write((short) XDataCode.AppReg, this.EncodeNonAsciiCharacters(appReg));

                foreach (XDataRecord x in xData[appReg].XDataRecord)
                {
                    short code = (short) x.Code;
                    object value = x.Value;
                    if (code == 1000 || code == 1003)
                    {
                        this.chunk.Write(code, this.EncodeNonAsciiCharacters((string) value));
                    }
                    else if (code == 1004) // binary extended data is written in chunks of 127 bytes
                    {
                        byte[] bytes = (byte[]) value;
                        byte[] data;
                        int count = bytes.Length;
                        int index = 0;
                        while (count > 127)
                        {
                            data = new byte[127];
                            Array.Copy(bytes, index, data, 0, 127);
                            this.chunk.Write(code, data);
                            count -= 127;
                            index += 127;
                        }
                        data = new byte[bytes.Length - index];
                        Array.Copy(bytes, index, data, 0, bytes.Length - index);
                        this.chunk.Write(code, data);
                    }
                    else
                    {
                        this.chunk.Write(code, value);
                    }
                }
            }
        }

        #endregion
    }
}