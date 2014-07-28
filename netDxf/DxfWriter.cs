#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

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
using Attribute=netDxf.Entities.Attribute;
using Group = netDxf.Objects.Group;

namespace netDxf
{
    /// <summary>
    /// Low level dxf writer.
    /// </summary>
    internal sealed class DxfWriter
    {
        #region private fields

        private string activeSection = StringCode.Unknown;
        private string activeTable = StringCode.Unknown;
        private ICodeValueWriter chunk;
        private DxfDocument doc;
        // here we will store strings already encoded <string: original, string: encoded>
        private Dictionary<string, string> encodedStrings;

        #endregion

        #region constructors

        #endregion

        #region public methods

        public void Write(Stream stream, DxfDocument document, bool isBinary)
        {
            this.doc = document;

            if (this.doc.DrawingVariables.AcadVer < DxfVersion.AutoCad2000)
                throw new NotSupportedException("Only AutoCad2000 and newer dxf versions are supported.");

            this.encodedStrings = new Dictionary<string, string>();

            // create the default PaperSpace layout in case it does not exist. The ModelSpace layout always exists
            if (this.doc.Layouts.Count == 1)
                this.doc.Layouts.Add(new Layout("Layout1"));

            // create the application registry AcCmTransparency in case it doesn't exists, it is requiered by the layer and entity transparency
            this.doc.ApplicationRegistries.Add(new ApplicationRegistry("AcCmTransparency"));

            // dictionaries
            List<DictionaryObject> dictionaries = new List<DictionaryObject>();

            // Named dictionary it is always the first to appear in the object section
            DictionaryObject namedObjectDictionary = new DictionaryObject(this.doc);
            this.doc.NumHandles = namedObjectDictionary.AsignHandle(this.doc.NumHandles);
            dictionaries.Add(namedObjectDictionary);

            // create the Group dictionary, this dictionary always appear even if there are no groups in the drawing
            DictionaryObject groupDictionary = new DictionaryObject(namedObjectDictionary);
            this.doc.NumHandles = groupDictionary.AsignHandle(this.doc.NumHandles);
            foreach (Group group in this.doc.Groups.Items)
            {
                groupDictionary.Entries.Add(group.Handle, group.Name);
            }
            dictionaries.Add(groupDictionary);
            namedObjectDictionary.Entries.Add(groupDictionary.Handle, StringCode.GroupDictionary);

            // Layout dictionary
            DictionaryObject layoutDictionary = new DictionaryObject(namedObjectDictionary);
            this.doc.NumHandles = layoutDictionary.AsignHandle(this.doc.NumHandles);
            if (this.doc.Layouts.Count > 0)
            {
                foreach (Layout layout in this.doc.Layouts.Items)
                {
                    layoutDictionary.Entries.Add(layout.Handle, layout.Name);
                }
                dictionaries.Add(layoutDictionary);
                namedObjectDictionary.Entries.Add(layoutDictionary.Handle, StringCode.LayoutDictionary);
            }

            // create the MLine style dictionary
            DictionaryObject mLineStyleDictionary = new DictionaryObject(namedObjectDictionary);
            this.doc.NumHandles = mLineStyleDictionary.AsignHandle(this.doc.NumHandles);
            if (this.doc.MlineStyles.Count > 0)
            {
                foreach (MLineStyle mLineStyle in this.doc.MlineStyles.Items)
                {
                    mLineStyleDictionary.Entries.Add(mLineStyle.Handle, mLineStyle.Name);
                }
                dictionaries.Add(mLineStyleDictionary);
                namedObjectDictionary.Entries.Add(mLineStyleDictionary.Handle, StringCode.MLineStyleDictionary);
            }

            // create the image dictionary
            DictionaryObject imageDefDictionary = new DictionaryObject(namedObjectDictionary);
            this.doc.NumHandles = imageDefDictionary.AsignHandle(this.doc.NumHandles);
            if (this.doc.ImageDefinitions.Count > 0)
            {
                foreach (ImageDef imageDef in this.doc.ImageDefinitions.Items)
                {
                    imageDefDictionary.Entries.Add(imageDef.Handle, imageDef.Name);
                }

                dictionaries.Add(imageDefDictionary);

                namedObjectDictionary.Entries.Add(imageDefDictionary.Handle, StringCode.ImageDefDictionary);
                namedObjectDictionary.Entries.Add(this.doc.RasterVariables.Handle, StringCode.ImageVarsDictionary);
            }

            this.doc.DrawingVariables.HandleSeed = this.doc.NumHandles.ToString("X");

            this.Open(stream, this.doc.DrawingVariables.AcadVer < DxfVersion.AutoCad2007 ? Encoding.Default : null, isBinary);

            // The comment group, 999, is not used in binary DXF files.
            if (!isBinary)
            {
                foreach (string comment in this.doc.Comments)
                    this.WriteComment(comment);
            }

            //HEADER SECTION
            this.BeginSection(StringCode.HeaderSection);
            foreach (HeaderVariable variable in this.doc.DrawingVariables.Values)
            {
                this.WriteSystemVariable(variable);
            }
            this.EndSection();

            //CLASSES SECTION
            this.BeginSection(StringCode.ClassesSection);
            this.WriteRasterVariablesClass(1);
            if (this.doc.ImageDefinitions.Items.Count > 0)
            {
                this.WriteImageDefClass(this.doc.ImageDefinitions.Count);
                this.WriteImageDefRectorClass(this.doc.Images.Count);
                this.WriteImageClass(this.doc.Images.Count);
            }
            this.EndSection();

            //TABLES SECTION
            this.BeginSection(StringCode.TablesSection);

            //registered application tables
            this.BeginTable(this.doc.ApplicationRegistries.CodeName, (short)this.doc.ApplicationRegistries.Count, this.doc.ApplicationRegistries.Handle);
            foreach (ApplicationRegistry id in this.doc.ApplicationRegistries.Items)
            {
                this.RegisterApplication(id);
            }
            this.EndTable();

            //viewport tables
            this.BeginTable(this.doc.VPorts.CodeName, (short)this.doc.VPorts.Count, this.doc.VPorts.Handle);
            foreach (VPort vport in this.doc.VPorts)
            {
                this.WriteVPort(vport);
            }
            this.EndTable();

            //line type tables
            this.BeginTable(this.doc.LineTypes.CodeName, (short)this.doc.LineTypes.Count, this.doc.LineTypes.Handle);
            foreach (LineType lineType in this.doc.LineTypes.Items)
            {
                this.WriteLineType(lineType);
            }
            this.EndTable();

            //layer tables
            this.BeginTable(this.doc.Layers.CodeName, (short)this.doc.Layers.Count, this.doc.Layers.Handle);
            foreach (Layer layer in this.doc.Layers.Items)
            {
                this.WriteLayer(layer);
            }
            this.EndTable();

            //text style tables
            this.BeginTable(this.doc.TextStyles.CodeName, (short)this.doc.TextStyles.Count, this.doc.TextStyles.Handle);
            foreach (TextStyle style in this.doc.TextStyles.Items)
            {
                this.WriteTextStyle(style);
            }
            this.EndTable();

            //dimension style tables
            this.BeginTable(this.doc.DimensionStyles.CodeName, (short)this.doc.DimensionStyles.Count, this.doc.DimensionStyles.Handle);
            foreach (DimensionStyle style in this.doc.DimensionStyles.Items)
            {
                this.WriteDimensionStyle(style);
            }
            this.EndTable();

            //view
            this.BeginTable(this.doc.Views.CodeName, (short)this.doc.Views.Count, this.doc.Views.Handle);
            foreach (View view in this.doc.Views)
            {
                throw new NotImplementedException("this.WriteView(view)");
            }
            this.EndTable();

            //ucs
            this.BeginTable(this.doc.UCSs.CodeName, (short)this.doc.UCSs.Count, this.doc.UCSs.Handle);
            foreach (UCS ucs in this.doc.UCSs.Items)
            {
                this.WriteUCS(ucs);
            }
            this.EndTable();

            //block reacord table
            this.BeginTable(this.doc.Blocks.CodeName, (short)this.doc.Blocks.Count, this.doc.Blocks.Handle);
            foreach (Block block in this.doc.Blocks.Items)
            {
                this.WriteBlockRecord(block.Record);
            }
            this.EndTable();

            this.EndSection(); //End section tables

            //BLOCKS SECTION
            this.BeginSection(StringCode.BlocksSection);
            foreach (Block block in this.doc.Blocks.Items)
            {
                Layout layout = null;

                if (block.Name.StartsWith("*Paper_Space"))
                {
                    string index = block.Name.Remove(0, 12);
                    if (!string.IsNullOrEmpty(index))
                    {
                        layout = block.Record.Layout;
                    }
                }

                this.WriteBlock(block, layout);

            }
            this.EndSection(); //End section blocks

            //ENTITIES SECTION
            this.BeginSection(StringCode.EntitiesSection);
            foreach (Layout layout in this.doc.Layouts)
            {
                if (layout.AssociatedBlock.Name.StartsWith("*Paper_Space"))
                {
                    string index = layout.AssociatedBlock.Name.Remove(0, 12);
                    if (string.IsNullOrEmpty(index))
                    {
                        this.WriteEntity(layout.Viewport, layout);

                        List<DxfObject> entities = this.doc.Layouts.GetReferences(layout);
                        foreach (DxfObject o in entities)
                        {
                            this.WriteEntity(o as EntityObject, layout);
                        }
                    }
                }
                else
                {
                    if (layout.Viewport != null)
                        this.WriteEntity(layout.Viewport, layout);

                    List<DxfObject> entities = this.doc.Layouts.GetReferences(layout);
                    foreach (DxfObject o in entities)
                    {
                        this.WriteEntity(o as EntityObject, layout);
                    }

                }
            }
            this.EndSection(); //End section entities

            //OBJECTS SECTION
            this.BeginSection(StringCode.ObjectsSection);

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

            // the raster variables dictionary is only needed when the drawing has image entities
            if (this.doc.ImageDefinitions.Count > 0)
                this.WriteRasterVariables(this.doc.RasterVariables, imageDefDictionary.Handle);

            foreach (ImageDef imageDef in this.doc.ImageDefinitions.Items)
            {
                foreach (ImageDefReactor reactor in imageDef.Reactors.Values)
                {
                    this.WriteImageDefReactor(reactor);
                }
                this.WriteImageDef(imageDef, imageDefDictionary.Handle);
            }

            this.EndSection(); //End section objects

            this.Close();

            stream.Position = 0;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Open the dxf writer.
        /// </summary>
        private void Open(Stream stream, Encoding encoding, bool isBinary)
        {
            if (isBinary)
                this.chunk = new BinaryCodeValueWriter(encoding == null ? new BinaryWriter(stream) : new BinaryWriter(stream, encoding));
            else
                this.chunk = new TextCodeValueWriter(encoding == null ? new StreamWriter(stream) : new StreamWriter(stream, encoding));
        }

        /// <summary>
        /// Closes the dxf writer.
        /// </summary>
        private void Close()
        {
            this.chunk.Write(0, StringCode.EndOfFile);
            this.chunk.Flush();
        }

        /// <summary>
        /// Opens a new section.
        /// </summary>
        /// <param name="section">Section type to open.</param>
        /// <remarks>There can be only one type section.</remarks>
        private void BeginSection(string section)
        {
            Debug.Assert(this.activeSection == StringCode.Unknown);

            this.chunk.Write(0, StringCode.BeginSection);
            this.chunk.Write(2, section);          
            this.activeSection = section;
        }

        /// <summary>
        /// Closes the active section.
        /// </summary>
        private void EndSection()
        {
            Debug.Assert(this.activeSection != StringCode.Unknown);

            this.chunk.Write(0, StringCode.EndSection);
            this.activeSection = StringCode.Unknown;
        }

        /// <summary>
        /// Opens a new table.
        /// </summary>
        /// <param name="table">Table type to open.</param>
        /// <param name="handle">Handle assigned to this table</param>
        private void BeginTable(string table, short numEntries, string handle)
        {
            Debug.Assert(this.activeSection == StringCode.TablesSection);

            this.chunk.Write(0, StringCode.Table);
            this.chunk.Write(2, table);
            this.chunk.Write(5, handle);
            this.chunk.Write(330, "0");

            this.chunk.Write(100, SubclassMarker.Table);
            this.chunk.Write(70, numEntries);

            if (table == StringCode.DimensionStyleTable)
                this.chunk.Write(100, SubclassMarker.DimensionStyleTable);

            this.activeTable = table;
        }

        /// <summary>
        /// Closes the active table.
        /// </summary>
        private void EndTable()
        {
            Debug.Assert(this.activeSection != StringCode.Unknown);

            this.chunk.Write(0, StringCode.EndTable);
            this.activeTable = StringCode.Unknown;
        }

        #endregion

        #region methods for Header section

        private void WriteComment(string comment)
        {
            
            if (!string.IsNullOrEmpty(comment))
                this.chunk.Write(999, comment);
        }

        private void WriteSystemVariable(HeaderVariable variable)
        {
            Debug.Assert(this.activeSection == StringCode.HeaderSection);

            string name = variable.Name;
            object value = variable.Value;

            switch (name)
            {
                case HeaderVariableCode.AcadVer:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, StringEnum.GetStringValue((DxfVersion)value));
                    break;
                case HeaderVariableCode.HandleSeed:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.Angbase:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.Angdir:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.AttMode:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(AttMode)value);
                    break;
                case HeaderVariableCode.AUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(AngleUnitType)value);
                    break;
                case HeaderVariableCode.AUprec:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.CeColor:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, ((AciColor)value).Index);
                    break;
                case HeaderVariableCode.CeLtScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.CeLtype:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.CeLweight:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, ((Lineweight)value).Value);
                    break;
                case HeaderVariableCode.CLayer:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.CMLJust:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(MLineJustification)value);
                    break;
                case HeaderVariableCode.CMLScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.CMLStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.DimStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.TextSize:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.TextStyle:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.LastSavedBy:
                    if (this.doc.DrawingVariables.AcadVer <= DxfVersion.AutoCad2000) break;
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, EncodeNonAsciiCharacters((string)value));
                    break;
                case HeaderVariableCode.LUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(LinearUnitType)value);
                    break;
                case HeaderVariableCode.LUprec:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.DwgCodePage:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.Extnames:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.InsUnits:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(DrawingUnits)value);
                    break;
                case HeaderVariableCode.LtScale:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.LwDisplay:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.PdMode:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, (short)(PointShape)value);
                    break;
                case HeaderVariableCode.PdSize:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.PLineGen:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, value);
                    break;
                case HeaderVariableCode.TdCreate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TduCreate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TdUpdate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TduUpdate:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, DrawingTime.ToJulianCalendar((DateTime) value));
                    break;
                case HeaderVariableCode.TdinDwg:
                    this.chunk.Write(9, name);
                    this.chunk.Write(variable.CodeGroup, ((TimeSpan) value).TotalDays);
                    break;
            }
        }

        #endregion

        #region methods for Classes section

        private void WriteImageClass(int count)
        {
            this.chunk.Write(0, StringCode.Class);
            this.chunk.Write(1, DxfObjectCode.Image);
            this.chunk.Write(2, SubclassMarker.RasterImage);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the dxf documentation
            this.chunk.Write(90, 127);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000) this.chunk.Write(91, count);
            this.chunk.Write(280, (short)0);
            this.chunk.Write(281, (short)1);
        }

        private void WriteImageDefClass(int count)
        {
            this.chunk.Write(0, StringCode.Class);
            this.chunk.Write(1, DxfObjectCode.ImageDef);
            this.chunk.Write(2, SubclassMarker.RasterImageDef);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the dxf documentation
            this.chunk.Write(90, 0);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000) this.chunk.Write(91, count);
            this.chunk.Write(280, (short)0);
            this.chunk.Write(281, (short)0);
        }

        private void WriteImageDefRectorClass(int count)
        {
            this.chunk.Write(0, StringCode.Class);
            this.chunk.Write(1, DxfObjectCode.ImageDefReactor);
            this.chunk.Write(2, SubclassMarker.RasterImageDefReactor);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the dxf documentation
            this.chunk.Write(90, 1);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000) this.chunk.Write(91, count);
            this.chunk.Write(280, (short)0);
            this.chunk.Write(281, (short)0);
        }

        private void WriteRasterVariablesClass(int count)
        {
            this.chunk.Write(0, StringCode.Class);
            this.chunk.Write(1, DxfObjectCode.RasterVariables);
            this.chunk.Write(2, SubclassMarker.RasterVariables);
            this.chunk.Write(3, "ISM");

            // default codes as shown in the dxf documentation
            this.chunk.Write(90, 0);
            if (this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000) this.chunk.Write(91, count);
            this.chunk.Write(280, (short)0);
            this.chunk.Write(281, (short)0);
        }

        #endregion

        #region methods for Table section

        /// <summary>
        /// Writes a new extended data application registry to the table section.
        /// </summary>
        /// <param name="appReg">Name of the application registry.</param>
        private void RegisterApplication(ApplicationRegistry appReg)
        {
            Debug.Assert(this.activeTable == StringCode.ApplicationIDTable);

            this.chunk.Write(0, StringCode.ApplicationIDTable);
            this.chunk.Write(5, appReg.Handle);
            this.chunk.Write(330, appReg.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);
            this.chunk.Write(100, SubclassMarker.ApplicationId);

            this.chunk.Write(2, EncodeNonAsciiCharacters(appReg.Name));

            this.chunk.Write(70, (short)0);
        }

        /// <summary>
        /// Writes a new view port to the table section.
        /// </summary>
        /// <param name="vp">Viewport.</param>
        private void WriteVPort(VPort vp)
        {
            Debug.Assert(this.activeTable == StringCode.VportTable);

            this.chunk.Write(0, vp.CodeName);
            this.chunk.Write(5, vp.Handle);
            this.chunk.Write(330, vp.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.VPort);

            this.chunk.Write(2, EncodeNonAsciiCharacters(vp.Name));

            this.chunk.Write(70, (short)0);

            this.chunk.Write(10, vp.LowerLeftCorner.X);
            this.chunk.Write(20, vp.LowerLeftCorner.Y);

            this.chunk.Write(11, vp.UpperRightCorner.X);
            this.chunk.Write(21, vp.UpperRightCorner.Y);

            this.chunk.Write(12, vp.LowerLeftCorner.X - vp.UpperRightCorner.X);
            this.chunk.Write(22, vp.UpperRightCorner.Y - vp.LowerLeftCorner.Y);

            this.chunk.Write(13, vp.SnapBasePoint.X);
            this.chunk.Write(23, vp.SnapBasePoint.Y);

            this.chunk.Write(14, vp.SnapSpacing.X);
            this.chunk.Write(24, vp.SnapSpacing.Y);

            this.chunk.Write(15, vp.GridSpacing.X);
            this.chunk.Write(25, vp.GridSpacing.Y);

            Vector3 dir = vp.Camera - vp.Target;
            this.chunk.Write(16, dir.X);
            this.chunk.Write(26, dir.Y);
            this.chunk.Write(36, dir.Z);

            this.chunk.Write(17, vp.Target.X);
            this.chunk.Write(27, vp.Target.Y);
            this.chunk.Write(37, vp.Target.Z);
        }

        /// <summary>
        /// Writes a new dimension style to the table section.
        /// </summary>
        /// <param name="style">DimensionStyle.</param>
        private void WriteDimensionStyle(DimensionStyle style)
        {
            Debug.Assert(this.activeTable == StringCode.DimensionStyleTable);

            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(105, style.Handle);
            this.chunk.Write(330, style.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.DimensionStyle);

            this.chunk.Write(2, EncodeNonAsciiCharacters(style.Name));

            // flags
            this.chunk.Write(70, (short)0);

            this.chunk.Write(3, style.DIMPOST);
            this.chunk.Write(41, style.DIMASZ);
            this.chunk.Write(42, style.DIMEXO);
            this.chunk.Write(44, style.DIMEXE);
            this.chunk.Write(73, style.DIMTIH);
            this.chunk.Write(74, style.DIMTOH);
            this.chunk.Write(77, style.DIMTAD);
            this.chunk.Write(140, style.DIMTXT);
            this.chunk.Write(141, style.DIMCEN);
            this.chunk.Write(147, style.DIMGAP);
            this.chunk.Write(179, style.DIMADEC);
            this.chunk.Write(271, style.DIMDEC);
            this.chunk.Write(275, style.DIMAUNIT);
            this.chunk.Write(278, (short)style.DIMDSEP);
            this.chunk.Write(280, style.DIMJUST);

            this.chunk.Write(340, style.TextStyle.Handle);
        }

        /// <summary>
        /// Writes a new block record to the table section.
        /// </summary>
        /// <param name="blockRecord">Block.</param>
        private void WriteBlockRecord(BlockRecord blockRecord)
        {
            Debug.Assert(this.activeTable == StringCode.BlockRecordTable);

            this.chunk.Write(0, blockRecord.CodeName);
            this.chunk.Write(5, blockRecord.Handle);
            this.chunk.Write(330, blockRecord.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.BlockRecord);

            this.chunk.Write(2, EncodeNonAsciiCharacters(blockRecord.Name));

            // Hard-pointer ID/handle to associated LAYOUT object
            this.chunk.Write(340, blockRecord.Layout == null ? "0" : blockRecord.Layout.Handle);

            // internal blocks do not need more information
            if (blockRecord.IsForInternalUseOnly) return;

            // The next three values will only work for dxf version AutoCad2007 and upwards
            this.chunk.Write(70, (short)blockRecord.Units);
            this.chunk.Write(280, blockRecord.AllowExploding ? (short)1 : (short)0);
            this.chunk.Write(281, blockRecord.ScaleUniformly ? (short)1 : (short)0);

            //if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2007)
            //    return;

            // for dxf versions prior to AutoCad2007 the block record units is stored in an extended data block
            this.chunk.Write(1001, ApplicationRegistry.Default.Name); // the default application registry is always present in the document
            this.chunk.Write(1000, "DesignCenter Data");
            this.chunk.Write(1002, "{");
            this.chunk.Write(1070, (short)1); // Autodesk Design Center version number.
            this.chunk.Write(1070, (short)blockRecord.Units);
            this.chunk.Write(1002, "}");
        }

        /// <summary>
        /// Writes a new line type to the table section.
        /// </summary>
        /// <param name="tl">Line type.</param>
        private void WriteLineType(LineType tl)
        {
            Debug.Assert(this.activeTable == StringCode.LineTypeTable);

            this.chunk.Write(0, tl.CodeName);
            this.chunk.Write(5, tl.Handle);
            this.chunk.Write(330, tl.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.LineType);

            this.chunk.Write(70, (short)0);

            this.chunk.Write(2, EncodeNonAsciiCharacters(tl.Name));

            this.chunk.Write(3, EncodeNonAsciiCharacters(tl.Description));

            this.chunk.Write(72, (short)65);
            this.chunk.Write(73, (short)tl.Segments.Count);
            this.chunk.Write(40, tl.Length());
            foreach (double s in tl.Segments)
            {
                this.chunk.Write(49, s);
                this.chunk.Write(74, (short)0);
            }
        }

        /// <summary>
        /// Writes a new layer to the table section.
        /// </summary>
        /// <param name="layer">Layer.</param>
        private void WriteLayer(Layer layer)
        {
            Debug.Assert(this.activeTable == StringCode.LayerTable);

            this.chunk.Write(0, layer.CodeName);
            this.chunk.Write(5, layer.Handle);
            this.chunk.Write(330, layer.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.Layer);

            this.chunk.Write(2, EncodeNonAsciiCharacters(layer.Name));

            LayerFlags flags = LayerFlags.None;
            if (layer.IsFrozen)
                flags = flags | LayerFlags.Frozen;
            if (layer.IsLocked)
                flags = flags | LayerFlags.Locked;
            this.chunk.Write(70, (short)flags);

            //a negative color represents a hidden layer.
            if (layer.IsVisible)
                this.chunk.Write(62, layer.Color.Index);
            else
                this.chunk.Write(62, (short)-layer.Color.Index);
            if (layer.Color.UseTrueColor)
                this.chunk.Write(420, AciColor.ToTrueColor(layer.Color));


            this.chunk.Write(6, EncodeNonAsciiCharacters(layer.LineType.Name));

            this.chunk.Write(290, layer.Plot);
            this.chunk.Write(370, layer.Lineweight.Value);
            // Hard pointer ID/handle of PlotStyleName object
            this.chunk.Write(390, "0");

            // transparency is stored in xdata
            if (layer.Transparency.Value > 0)
            {
                int alpha = Transparency.ToAlphaValue(layer.Transparency);
                this.chunk.Write(1001, "AcCmTransparency");
                this.chunk.Write(1071, alpha);
            }

        }

        /// <summary>
        /// Writes a new text style to the table section.
        /// </summary>
        /// <param name="style">TextStyle.</param>
        private void WriteTextStyle(TextStyle style)
        {
            Debug.Assert(this.activeTable == StringCode.TextStyleTable);

            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(5, style.Handle);
            this.chunk.Write(330, style.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.TextStyle);

            this.chunk.Write(2, EncodeNonAsciiCharacters(style.Name));

            this.chunk.Write(3, EncodeNonAsciiCharacters(style.FontName));

            this.chunk.Write(70, style.IsVertical ? (short)4 : (short)0);

            if (style.IsBackward && style.IsUpsideDown)
            {
                this.chunk.Write(71, (short)6);
            }
            else if (style.IsBackward)
            {
                this.chunk.Write(71, (short)2);
            }
            else if (style.IsUpsideDown)
            {
                this.chunk.Write(71, (short)4);
            }
            else
            {
                this.chunk.Write(71, (short)0);
            }

            this.chunk.Write(40, style.Height);
            this.chunk.Write(41, style.WidthFactor);
            this.chunk.Write(42, style.Height);
            this.chunk.Write(50, style.ObliqueAngle);
        }

        /// <summary>
        /// Writes a new user coordinate system to the table section.
        /// </summary>
        /// <param name="ucs">UCS.</param>
        private void WriteUCS(UCS ucs)
        {
            Debug.Assert(this.activeTable == StringCode.UcsTable);

            this.chunk.Write(0, ucs.CodeName);
            this.chunk.Write(5, ucs.Handle);
            this.chunk.Write(330, ucs.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.TableRecord);

            this.chunk.Write(100, SubclassMarker.Ucs);

            this.chunk.Write(2, EncodeNonAsciiCharacters(ucs.Name));

            this.chunk.Write(70, (short)0);

            this.chunk.Write(10, ucs.Origin.X);
            this.chunk.Write(20, ucs.Origin.Y);
            this.chunk.Write(30, ucs.Origin.Z);

            this.chunk.Write(11, ucs.XAxis.X);
            this.chunk.Write(21, ucs.XAxis.Y);
            this.chunk.Write(31, ucs.XAxis.Z);

            this.chunk.Write(12, ucs.YAxis.X);
            this.chunk.Write(22, ucs.YAxis.Y);
            this.chunk.Write(32, ucs.YAxis.Z);

            this.chunk.Write(79, (short)0);

            this.chunk.Write(146, ucs.Elevation);
        }

        #endregion

        #region methods for Block section

        private void WriteBlock(Block block, Layout layout)
        {
            Debug.Assert(this.activeSection == StringCode.BlocksSection);

            string name = EncodeNonAsciiCharacters(block.Name);

            this.chunk.Write(0, block.CodeName);
            this.chunk.Write(5, block.Handle);
            this.chunk.Write(330, block.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            if (layout != null)
                this.chunk.Write(67, layout.IsPaperSpace ? (short)1 : (short)0);

            this.chunk.Write(8, EncodeNonAsciiCharacters(block.Layer.Name));

            this.chunk.Write(100, SubclassMarker.BlockBegin);

            this.chunk.Write(2, name);

            //flags
            this.chunk.Write(70, block.AttributeDefinitions.Count == 0 ? (short)block.Flags : (short)(block.Flags | BlockTypeFlags.NonConstantAttributeDefinitions));

            this.chunk.Write(10, block.Position.X);
            this.chunk.Write(20, block.Position.Y);
            this.chunk.Write(30, block.Position.Z);

            this.chunk.Write(3, name);

            foreach (AttributeDefinition attdef in block.AttributeDefinitions.Values)
            {
                this.WriteAttributeDefinition(attdef);
            }

            if (layout == null)
            {
                foreach (EntityObject entity in block.Entities)
                {
                    this.WriteEntity(entity, null);
                }
            }
            else
            {
                this.WriteEntity(layout.Viewport, layout);

                List<DxfObject> entities = this.doc.Layouts.GetReferences(layout);
                foreach (DxfObject entity in entities)
                {
                    this.WriteEntity(entity as EntityObject, layout);
                }
            }

            // EndBlock entity
            this.chunk.Write(0, block.End.CodeName);
            this.chunk.Write(5, block.End.Handle);
            this.chunk.Write(330, block.Owner.Handle);
            this.chunk.Write(100, SubclassMarker.Entity);

            this.chunk.Write(8, EncodeNonAsciiCharacters(block.End.Layer.Name));

            this.chunk.Write(100, SubclassMarker.BlockEnd);
        }

        #endregion

        #region methods for Entity section

        private void WriteEntity(EntityObject entity, Layout layout)
        {
            Debug.Assert(this.activeSection == StringCode.EntitiesSection || this.activeSection == StringCode.BlocksSection);

            WriteEntityCommonCodes(entity, layout);

            switch (entity.Type)
            {
                case EntityType.Ray:
                    this.WriteRay((Ray)entity);
                    break;
                case EntityType.XLine:
                    this.WriteXLine((XLine)entity);
                    break;
                case EntityType.Arc:
                    this.WriteArc((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.WriteCircle((Circle) entity);
                    break;
                case EntityType.Ellipse:
                    this.WriteEllipse((Ellipse) entity);
                    break;
                case EntityType.Point:
                    this.WritePoint((Point) entity);
                    break;
                case EntityType.Face3D:
                    this.WriteFace3D((Face3d) entity);
                    break;
                case EntityType.Spline:
                    this.WriteSpline((Spline)entity);
                    break;
                case EntityType.Solid:
                    this.WriteSolid((Solid) entity);
                    break;
                case EntityType.Insert:
                    this.WriteInsert((Insert) entity);
                    break;
                case EntityType.Line:
                    this.WriteLine((Line) entity);
                    break;
                case EntityType.LightWeightPolyline:
                    this.WriteLightWeightPolyline((LwPolyline) entity);
                    break;
                case EntityType.Polyline:
                    this.WritePolyline3d((Polyline) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.WritePolyfaceMesh((PolyfaceMesh) entity);
                    break;
                case EntityType.Text:
                    this.WriteText((Text) entity);
                    break;
                case EntityType.MText:
                    this.WriteMText((MText) entity);
                    break;
                case EntityType.Hatch:
                    this.WriteHatch((Hatch)entity);
                    break;
                case EntityType.Dimension:
                    this.WriteDimension((Dimension) entity);
                    break;
                case EntityType.Image:
                    this.WriteImage((Image)entity);
                    break;
                case EntityType.MLine:
                    this.WriteMLine((MLine)entity);
                    break;
                case EntityType.Viewport:
                    this.WriteViewport((Viewport)entity);
                    break;
                case EntityType.Mesh:
                    this.WriteMesh((Mesh)entity);
                    break;
                default:
                    throw new ArgumentException("Entity unknown.", "entity");
                    
            }
        }

        private void WriteEntityCommonCodes(EntityObject entity, Layout layout)
        {
            this.chunk.Write(0, entity.CodeName);
            this.chunk.Write(5, entity.Handle);

            if (entity.Reactor != null)
            {
                this.chunk.Write(102, "{ACAD_REACTORS");
                this.chunk.Write(330, entity.Reactor.Handle);
                this.chunk.Write(102, "}");
            }

            this.chunk.Write(330, entity.Owner.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Entity);

            if (layout != null)
                this.chunk.Write(67, layout.IsPaperSpace ? (short)1 : (short)0);

            this.chunk.Write(8, EncodeNonAsciiCharacters(entity.Layer.Name));

            this.chunk.Write(62, entity.Color.Index);
            if (entity.Color.UseTrueColor)
                this.chunk.Write(420, AciColor.ToTrueColor(entity.Color));

            if (entity.Transparency.Value >= 0)
                this.chunk.Write(440, Transparency.ToAlphaValue(entity.Transparency));

            this.chunk.Write(6, EncodeNonAsciiCharacters(entity.LineType.Name));

            this.chunk.Write(370, entity.Lineweight.Value);
            this.chunk.Write(48, entity.LineTypeScale);
            this.chunk.Write(60, entity.IsVisible ? (short)0 : (short)1);

        }

        private void WriteMesh(Mesh mesh)
        {
            this.chunk.Write(100, SubclassMarker.Mesh);

            this.chunk.Write(71, (short)2);
            this.chunk.Write(72, (short)0);

            this.chunk.Write(91, (int)mesh.SubdivisionLevel);

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

            // the edges information is optional, and ony really useful when it is required to assign creases values to edges
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
        }

        private void WriteArc(Arc arc)
        {
            this.chunk.Write(100, SubclassMarker.Circle);

            this.chunk.Write(39, arc.Thickness);

            // this is just an example of the weird autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 ocsCenter = MathHelper.Transform(arc.Center, arc.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

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
            
            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 ocsCenter = MathHelper.Transform(circle.Center, circle.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

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


            double sine = 0.5 * ellipse.MajorAxis*Math.Sin(ellipse.Rotation*MathHelper.DegToRad);
            double cosine = 0.5 * ellipse.MajorAxis * Math.Cos(ellipse.Rotation * MathHelper.DegToRad);
            Vector3 axisPoint = MathHelper.Transform(new Vector3(cosine, sine, 0),
                                                      ellipse.Normal,
                                                      MathHelper.CoordinateSystem.Object,
                                                      MathHelper.CoordinateSystem.World);

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

        private void WriteSolid(Solid solid)
        {
            this.chunk.Write(100, SubclassMarker.Solid);

            this.chunk.Write(10, solid.FirstVertex.X);
            this.chunk.Write(20, solid.FirstVertex.Y);
            this.chunk.Write(30, solid.FirstVertex.Z);

            this.chunk.Write(11, solid.SecondVertex.X);
            this.chunk.Write(21, solid.SecondVertex.Y);
            this.chunk.Write(31, solid.SecondVertex.Z);

            this.chunk.Write(12, solid.ThirdVertex.X);
            this.chunk.Write(22, solid.ThirdVertex.Y);
            this.chunk.Write(32, solid.ThirdVertex.Z);

            this.chunk.Write(13, solid.FourthVertex.X);
            this.chunk.Write(23, solid.FourthVertex.Y);
            this.chunk.Write(33, solid.FourthVertex.Z);

            this.chunk.Write(39, solid.Thickness);

            this.chunk.Write(210, solid.Normal.X);
            this.chunk.Write(220, solid.Normal.Y);
            this.chunk.Write(230, solid.Normal.Z);

            this.WriteXData(solid.XData);
        }

        private void WriteFace3D(Face3d face)
        {
            this.chunk.Write(100, SubclassMarker.Face3d);

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

            this.chunk.Write(70, (short)face.EdgeFlags);

            this.WriteXData(face.XData);
        }

        private void WriteSpline(Spline spline)
        {
            this.chunk.Write(100, SubclassMarker.Spline);

            short flag = (short)spline.Flags;
            if (spline.IsClosed) flag += (short)SplineTypeFlags.Unknown4;
            this.chunk.Write(70, flag);
            this.chunk.Write(71, spline.Degree);

            // the next two codes are purely cosmetic and writting them causes more bad than good.
            // internally AutoCad allows for an INT number of knots and control points,
            // but for some dumb decision Autodesk decided to define them in the dxf with codes 72 and 73 (16-bit integer value), this is a SHORT in net.
            // I guess this is the result of legacy code, AutoCad do not use those values when importing the Spline entity
            //this.chunk.Write(72, (short)spline.Knots.Length);
            //this.chunk.Write(73, (short)spline.ControlPoints.Count);

            //this.chunk.Write(74, 0); Number of fit points (if any).

            //this.chunk.Write(42, 0); 42 Knot tolerance (default = 0.0000001)
            //this.chunk.Write(43, 0); 43 Control-point tolerance (default = 0.0000001)
            //this.chunk.Write(44, 0); 44 Fit tolerance (default = 0.0000000001)


            foreach (double knot in spline.Knots)
                this.chunk.Write(40, knot);

            foreach (SplineVertex point in spline.ControlPoints)
            {
                this.chunk.Write(41, point.Weigth);
                this.chunk.Write(10, point.Location.X);
                this.chunk.Write(20, point.Location.Y);
                this.chunk.Write(30, point.Location.Z);
            }
            
            this.WriteXData(spline.XData);
        }

        private void WriteInsert(Insert insert)
        {
            this.chunk.Write(100, SubclassMarker.Insert);

            this.chunk.Write(2, EncodeNonAsciiCharacters(insert.Block.Name));

            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 ocsInsertion = MathHelper.Transform(insert.Position, insert.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.chunk.Write(10, ocsInsertion.X);
            this.chunk.Write(20, ocsInsertion.Y);
            this.chunk.Write(30, ocsInsertion.Z);

            // we need to apply the scaling factor between the block and the document or the block that owns it in case of nested blocks
            double scale = MathHelper.ConversionFactor(insert.Block.Record.Units, insert.Owner.Record.IsForInternalUseOnly ? this.doc.DrawingVariables.InsUnits : insert.Owner.Record.Units);

            this.chunk.Write(41, insert.Scale.X * scale);
            this.chunk.Write(42, insert.Scale.Y * scale);
            this.chunk.Write(43, insert.Scale.Z * scale);

            this.chunk.Write(50, insert.Rotation);

            this.chunk.Write(210, insert.Normal.X);
            this.chunk.Write(220, insert.Normal.Y);
            this.chunk.Write(230, insert.Normal.Z);

            if (insert.Attributes.Count > 0)
            {
                //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
                //AutoCAD will fail loading the file if it is not there, more dxf voodoo
                this.chunk.Write(66, (short)1);

                this.WriteXData(insert.XData);

                foreach (Attribute attrib in insert.Attributes.Values)
                    this.WriteAttribute(attrib);

                this.chunk.Write(0, insert.EndSequence.CodeName);
                this.chunk.Write(5, insert.EndSequence.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);

                this.chunk.Write(8, EncodeNonAsciiCharacters(insert.EndSequence.Layer.Name));
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

        private void WriteLightWeightPolyline(LwPolyline polyline)
        {            
            this.chunk.Write(100, SubclassMarker.LightWeightPolyline);
            this.chunk.Write(90, polyline.Vertexes.Count);
            this.chunk.Write(70, (short)polyline.Flags);

            this.chunk.Write(38, polyline.Elevation);
            this.chunk.Write(39, polyline.Thickness);


            foreach (LwPolylineVertex v in polyline.Vertexes)
            {
                this.chunk.Write(10, v.Location.X);
                this.chunk.Write(20, v.Location.Y);
                this.chunk.Write(40, v.BeginWidth);
                this.chunk.Write(41, v.EndWidth);
                this.chunk.Write(42, v.Bulge);
            }

            this.chunk.Write(210, polyline.Normal.X);
            this.chunk.Write(220, polyline.Normal.Y);
            this.chunk.Write(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);
        }

        private void WritePolyline3d(Polyline polyline)
        {
            this.chunk.Write(100, SubclassMarker.Polyline3d);

            //dummy point
            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);
            this.chunk.Write(30, 0.0);

            this.chunk.Write(70, (short)polyline.Flags);
            this.chunk.Write(75, (short)polyline.SmoothType);

            this.chunk.Write(210, polyline.Normal.X);
            this.chunk.Write(220, polyline.Normal.Y);
            this.chunk.Write(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);

            string layerName = EncodeNonAsciiCharacters(polyline.Layer.Name);

            foreach (PolylineVertex v in polyline.Vertexes)
            {
                this.chunk.Write(0, v.CodeName);
                this.chunk.Write(5, v.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);

                this.chunk.Write(8, layerName); // the vertex layer should be the same as the polyline layer

                this.chunk.Write(62, polyline.Color.Index); // the vertex color should be the same as the polyline color
                if (polyline.Color.UseTrueColor)
                    this.chunk.Write(420, AciColor.ToTrueColor(polyline.Color));
                this.chunk.Write(100, SubclassMarker.Vertex);
                this.chunk.Write(100, SubclassMarker.Polyline3dVertex);             
                this.chunk.Write(10, v.Location.X);
                this.chunk.Write(20, v.Location.Y);
                this.chunk.Write(30, v.Location.Z);
                this.chunk.Write(70, (short)v.Flags);
            }

            this.chunk.Write(0, polyline.EndSequence.CodeName);
            this.chunk.Write(5, polyline.EndSequence.Handle);
            this.chunk.Write(100, SubclassMarker.Entity);

            this.chunk.Write(8, EncodeNonAsciiCharacters(polyline.EndSequence.Layer.Name));

        }

        private void WritePolyfaceMesh(PolyfaceMesh mesh)
        {
            this.chunk.Write(100, SubclassMarker.PolyfaceMesh);
            this.chunk.Write(70, (short)mesh.Flags);

            this.chunk.Write(71, (short)mesh.Vertexes.Count);
            this.chunk.Write(72, (short)mesh.Faces.Count);

            //dummy point
            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);
            this.chunk.Write(30, 0.0);

            this.chunk.Write(210, mesh.Normal.X);
            this.chunk.Write(220, mesh.Normal.Y);
            this.chunk.Write(230, mesh.Normal.Z);

            if (mesh.XData != null)
                this.WriteXData(mesh.XData);

            string layerName = EncodeNonAsciiCharacters(mesh.Layer.Name);

            foreach (PolyfaceMeshVertex v in mesh.Vertexes)
            {
                this.chunk.Write(0, v.CodeName);
                this.chunk.Write(5, v.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);

                this.chunk.Write(8, layerName); // the polyfacemesh vertex layer should be the same as the polyfacemesh layer

                this.chunk.Write(62, mesh.Color.Index); // the polyfacemesh vertex color should be the same as the polyfacemesh color
                if (mesh.Color.UseTrueColor)
                    this.chunk.Write(420, AciColor.ToTrueColor(mesh.Color));
                this.chunk.Write(100, SubclassMarker.Vertex);
                this.chunk.Write(100, SubclassMarker.PolyfaceMeshVertex);
                this.chunk.Write(70, (short)v.Flags);
                this.chunk.Write(10, v.Location.X);
                this.chunk.Write(20, v.Location.Y);
                this.chunk.Write(30, v.Location.Z);
            }

            foreach (PolyfaceMeshFace face in mesh.Faces)
            {
                this.chunk.Write(0, face.CodeName);
                this.chunk.Write(5, face.Handle);
                this.chunk.Write(100, SubclassMarker.Entity);

                this.chunk.Write(8, layerName); // the polyfacemesh face layer should be the same as the polyfacemesh layer
                this.chunk.Write(62, mesh.Color.Index); // the polyfacemesh face color should be the same as the polyfacemesh color
                if (mesh.Color.UseTrueColor)
                    this.chunk.Write(420, AciColor.ToTrueColor(mesh.Color));
                this.chunk.Write(100, SubclassMarker.PolyfaceMeshFace);
                this.chunk.Write(70, (short)VertexTypeFlags.PolyfaceMeshVertex);
                this.chunk.Write(10, 0.0);
                this.chunk.Write(20, 0.0);
                this.chunk.Write(30, 0.0);

                this.chunk.Write(71, face.VertexIndexes[0]);
                if (face.VertexIndexes.Length > 1) this.chunk.Write(72, face.VertexIndexes[1]);
                if (face.VertexIndexes.Length > 2) this.chunk.Write(73, face.VertexIndexes[2]);
                if (face.VertexIndexes.Length > 3) this.chunk.Write(74, face.VertexIndexes[3]);
            }

            this.chunk.Write(0, mesh.EndSequence.CodeName);
            this.chunk.Write(5, mesh.EndSequence.Handle);
            this.chunk.Write(100, SubclassMarker.Entity);

            this.chunk.Write(8, EncodeNonAsciiCharacters(mesh.EndSequence.Layer.Name));
        }

        private void WritePoint(Point point)
        {
            this.chunk.Write(100, SubclassMarker.Point);

            this.chunk.Write(10, point.Location.X);
            this.chunk.Write(20, point.Location.Y);
            this.chunk.Write(30, point.Location.Z);

            this.chunk.Write(39, point.Thickness);

            this.chunk.Write(210, point.Normal.X);
            this.chunk.Write(220, point.Normal.Y);
            this.chunk.Write(230, point.Normal.Z);

            // for unknown reasons the dxf likes the point rotation inverted
            this.chunk.Write(50, 360.0 - point.Rotation);

            this.WriteXData(point.XData);
        }

        private void WriteText(Text text)
        {
            this.chunk.Write(100, SubclassMarker.Text);

            this.chunk.Write(1, EncodeNonAsciiCharacters(text.Value));

            // another example of this ocs vs wcs non sense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            Vector3 ocsBasePoint = MathHelper.Transform(text.Position, text.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.chunk.Write(10, ocsBasePoint.X);
            this.chunk.Write(20, ocsBasePoint.Y);
            this.chunk.Write(30, ocsBasePoint.Z);

            this.chunk.Write(40, text.Height);

            this.chunk.Write(41, text.WidthFactor);

            this.chunk.Write(50, text.Rotation);

            this.chunk.Write(51, text.ObliqueAngle);

            this.chunk.Write(7, EncodeNonAsciiCharacters(text.Style.Name));

            this.chunk.Write(11, ocsBasePoint.X);
            this.chunk.Write(21, ocsBasePoint.Y);
            this.chunk.Write(31, ocsBasePoint.Z);

            this.chunk.Write(210, text.Normal.X);
            this.chunk.Write(220, text.Normal.Y);
            this.chunk.Write(230, text.Normal.Z);

            switch (text.Alignment)
            {
                case TextAlignment.TopLeft:

                    this.chunk.Write(72, (short)0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)3);
                    break;

                case TextAlignment.TopCenter:

                    this.chunk.Write(72, (short)1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)3);
                    break;

                case TextAlignment.TopRight:

                    this.chunk.Write(72, (short)2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)3);
                    break;

                case TextAlignment.MiddleLeft:

                    this.chunk.Write(72, (short)0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)2);
                    break;

                case TextAlignment.MiddleCenter:

                    this.chunk.Write(72, (short)1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)2);
                    break;

                case TextAlignment.MiddleRight:

                    this.chunk.Write(72, (short)2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)2);
                    break;

                case TextAlignment.BottomLeft:

                    this.chunk.Write(72, (short)0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)1);
                    break;
                case TextAlignment.BottomCenter:

                    this.chunk.Write(72, (short)1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)1);
                    break;

                case TextAlignment.BottomRight:

                    this.chunk.Write(72, (short)2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)1);
                    break;

                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short)0);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
                    break;

                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short)1);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
                    break;

                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short)2);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
                    break;

                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short)3);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
                    break;

                case TextAlignment.Middle:
                    this.chunk.Write(72, (short)4);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
                    break;

                case TextAlignment.Fit:
                    this.chunk.Write(72, (short)5);
                    this.chunk.Write(100, SubclassMarker.Text);
                    this.chunk.Write(73, (short)0);
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

            WriteMTextChunks(EncodeNonAsciiCharacters(mText.Value));

            this.chunk.Write(40, mText.Height);
            this.chunk.Write(41, mText.RectangleWidth);
            this.chunk.Write(44, mText.LineSpacingFactor);

            // even if the AutoCAD dxf documentation says that the rotation is in radians, this is wrong this value must be saved in degrees
            this.chunk.Write(50, mText.Rotation);

            this.chunk.Write(71, (short)mText.AttachmentPoint);

            // By style (the flow direction is inherited from the associated text style)
            this.chunk.Write(72, (short)5);

            this.chunk.Write(7, EncodeNonAsciiCharacters(mText.Style.Name));

            this.WriteXData(mText.XData);
        }

        private void WriteMTextChunks(string text)
        {
            //Text string. If the text string is less than 250 characters, all characters
            //appear in group 1. If the text string is greater than 250 characters, the
            //string is divided into 250 character chunks, which appear in one or
            //more group 3 codes. If group 3 codes are used, the last group is a
            //group 1 and has fewer than 250 characters
            while(text.Length>250)
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

            this.chunk.Write(2, hatch.Pattern.Name);

            this.chunk.Write(70, (short)hatch.Pattern.Fill);

            this.chunk.Write(71, (short)0);

            // boundary paths info
            WriteHatchBoundaryPaths(hatch.BoundaryPaths);

            // pattern info
            WriteHatchPattern(hatch.Pattern);

            this.WriteXData(hatch.XData);   
        }

        private void WriteHatchBoundaryPaths(List<HatchBoundaryPath> boundaryPaths)
        {
            this.chunk.Write(91, boundaryPaths.Count);

            // each hatch boundary paths are made of multiple closed loops
            foreach (HatchBoundaryPath path in boundaryPaths)
            {
                this.chunk.Write(92, (int) path.PathTypeFlag);

                if ((path.PathTypeFlag & HatchBoundaryPathTypeFlag.Polyline) != HatchBoundaryPathTypeFlag.Polyline)
                    this.chunk.Write(93, path.Edges.Count);

                foreach (HatchBoundaryPath.Edge entity in path.Edges)
                    WriteHatchBoundaryPathData(entity);

                this.chunk.Write(97, 0); // associative hatches not supported
            }
        }

        private void WriteHatchBoundaryPathData(HatchBoundaryPath.Edge entity)
        {
            if (entity is HatchBoundaryPath.Arc)
            {
                this.chunk.Write(72, (short)2);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Arc arc = (HatchBoundaryPath.Arc)entity;

                this.chunk.Write(10, arc.Center.X);
                this.chunk.Write(20, arc.Center.Y);
                this.chunk.Write(40, arc.Radius);
                this.chunk.Write(50, arc.StartAngle);
                this.chunk.Write(51, arc.EndAngle);
                this.chunk.Write(73, arc.IsCounterclockwise ? (short)1 : (short)0); 
            }
            else if (entity is HatchBoundaryPath.Ellipse)
            {
                this.chunk.Write(72, (short)3);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Ellipse ellipse = (HatchBoundaryPath.Ellipse)entity;

                this.chunk.Write(10, ellipse.Center.X);
                this.chunk.Write(20, ellipse.Center.Y);
                this.chunk.Write(11, ellipse.EndMajorAxis.X);
                this.chunk.Write(21, ellipse.EndMajorAxis.Y);
                this.chunk.Write(40, ellipse.MinorRatio);
                this.chunk.Write(50, ellipse.StartAngle);
                this.chunk.Write(51, ellipse.EndAngle);
                this.chunk.Write(73, ellipse.IsCounterclockwise ? (short)1 : (short)0);
            }
            else if(entity is HatchBoundaryPath.Line)
            {
                this.chunk.Write(72, (short)1);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Line line = (HatchBoundaryPath.Line)entity;

                this.chunk.Write(10, line.Start.X);
                this.chunk.Write(20, line.Start.Y);
                this.chunk.Write(11, line.End.X);
                this.chunk.Write(21, line.End.Y);
            }
            else if (entity is HatchBoundaryPath.Polyline)
            {
                HatchBoundaryPath.Polyline poly = (HatchBoundaryPath.Polyline) entity;
                this.chunk.Write(72, (short)1);  // Has bulge flag
                this.chunk.Write(73, poly.IsClosed ? (short)1 : (short)0);
                this.chunk.Write(93, poly.Vertexes.Length);

                foreach (Vector3 vertex in poly.Vertexes)
                {
                    this.chunk.Write(10, vertex.X);
                    this.chunk.Write(20, vertex.Y);
                    this.chunk.Write(42, vertex.Z);
                }
            }
            else if (entity is HatchBoundaryPath.Spline)
            {
                this.chunk.Write(72, (short)4);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                HatchBoundaryPath.Spline spline = (HatchBoundaryPath.Spline)entity;

                // another dxf inconsistency!; while the Spline entity degree is written as a short (code 71)
                // the degree of a hatch boundary path spline is written as an int (code 94)
                this.chunk.Write(94, (int)spline.Degree);
                this.chunk.Write(73, spline.IsRational ? (short)1 : (short)0);
                this.chunk.Write(74, spline.IsPeriodic ? (short)1 : (short)0);

                // now the number of knots and control points of a spline are written as an ints, as it should be.
                // but in the Spline entities they are defined as shorts. Guess what, while you can avoid writting these two codes for the Spline entity, now they are required.
                this.chunk.Write(95, spline.Knots.Length);
                this.chunk.Write(96, spline.ControlPoints.Length);

                foreach (double knot in spline.Knots)
                    this.chunk.Write(40, knot);
                foreach (Vector3 point in spline.ControlPoints)
                {
                    this.chunk.Write(10, point.X);
                    this.chunk.Write(20, point.Y);
                    if(spline.IsRational) this.chunk.Write(42, point.Z);
                }

                // this information is only required for AutoCAD version 2010
                // stores information about spline fit points (the spline entity has no fit points and no tangent info)
                // another dxf inconsistency!; while the number of fit points of Spline entity is written as a short (code 74)
                // the number of fit points of a hatch boundary path spline is written as an int (code 97)
                if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2010) this.chunk.Write(97, 0);
            }            
        }

        private void WriteHatchPattern(HatchPattern pattern)
        {
            this.chunk.Write(75, (short)pattern.Style);
            this.chunk.Write(76, (short)pattern.Type);

            if (pattern.Fill == HatchFillType.PatternFill)
            {
                this.chunk.Write(52, pattern.Angle);
                this.chunk.Write(41, pattern.Scale);
                this.chunk.Write(77, (short)0);  // Hatch pattern double flag
                this.chunk.Write(78, (short)pattern.LineDefinitions.Count);  // Number of pattern definition lines  
                WriteHatchPatternDefinitonLines(pattern);
            }

            // I don't know what is the purpose of these codes, it seems that it doesn't change anything but they are needed
            this.chunk.Write(47, 0.0);
            this.chunk.Write(98, 1);
            this.chunk.Write(10, 0.0);
            this.chunk.Write(20, 0.0);

            // dxf AutoCad2000 does not support hatch gradient patterns
            if (pattern is HatchGradientPattern && this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
                WriteGradientHatchPattern((HatchGradientPattern) pattern);
        }

        private void WriteGradientHatchPattern(HatchGradientPattern pattern)
        {
            // again the order of codes shown in the documentation will not work
            this.chunk.Write(450, 1);
            this.chunk.Write(451, 0);
            this.chunk.Write(460, pattern.Angle * MathHelper.DegToRad);
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
            this.chunk.Write(470, StringEnum.GetStringValue(pattern.GradientType));

        }

        private void WriteHatchPatternDefinitonLines(HatchPattern pattern)
        {
            foreach (HatchPatternLineDefinition line in pattern.LineDefinitions)
            {
                double scale = pattern.Scale;
                double angle = line.Angle + pattern.Angle;
                // Pattern fill data.
                // In theory this should hold the same information as the pat file but for unkown reason the dxf requires global data instead of local,
                // it's a guess the documentation is kind of obscure.
                // This means we have to apply the pattern rotation and scale to the line definitions
                this.chunk.Write(53, angle);

                double sinOrigin = Math.Sin(pattern.Angle * MathHelper.DegToRad);
                double cosOrigin = Math.Cos(pattern.Angle * MathHelper.DegToRad);       
                Vector2 origin = new Vector2(cosOrigin * line.Origin.X * scale - sinOrigin * line.Origin.Y * scale, sinOrigin * line.Origin.X * scale + cosOrigin * line.Origin.Y * scale);
                this.chunk.Write(43, origin.X);
                this.chunk.Write(44, origin.Y);

                double sinDelta = Math.Sin(angle * MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle * MathHelper.DegToRad);       
                Vector2 delta = new Vector2(cosDelta * line.Delta.X * scale - sinDelta * line.Delta.Y * scale, sinDelta * line.Delta.X * scale + cosDelta * line.Delta.Y * scale);
                this.chunk.Write(45, delta.X);
                this.chunk.Write(46, delta.Y);

                this.chunk.Write(79, (short)line.DashPattern.Count);
                foreach (double dash in line.DashPattern)
                {
                    this.chunk.Write(49, dash * scale);
                }
            }
        }

        private void WriteDimension(Dimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Dimension);

            this.chunk.Write(2, EncodeNonAsciiCharacters(dim.Block.Name));

            this.chunk.Write(10, dim.DefinitionPoint.X);
            this.chunk.Write(20, dim.DefinitionPoint.Y);
            this.chunk.Write(30, dim.DefinitionPoint.Z);
            this.chunk.Write(11, dim.MidTextPoint.X);
            this.chunk.Write(21, dim.MidTextPoint.Y);
            this.chunk.Write(31, dim.MidTextPoint.Z);

            short flags = (short)(dim.DimensionType + (short)DimensionTypeFlag.BlockReference);
            if(dim.DimensionType==DimensionType.Ordinate)
            {
                // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly is this value is not provided
                this.chunk.Write(51, 360-((OrdinateDimension)dim).Rotation);
                if (((OrdinateDimension)dim).Axis==OrdinateDimensionAxis.X)
                    flags += (short)DimensionTypeFlag.OrdinteType;
            }

            this.chunk.Write(70, flags);
            this.chunk.Write(71, (short)dim.AttachmentPoint);
            this.chunk.Write(72, (short)dim.LineSpacingStyle);
            this.chunk.Write(41, dim.LineSpacingFactor);
            this.chunk.Write(210, dim.Normal.X);
            this.chunk.Write(220, dim.Normal.Y);
            this.chunk.Write(230, dim.Normal.Z);

            this.chunk.Write(3, EncodeNonAsciiCharacters(dim.Style.Name));

            switch (dim.DimensionType)
            {
                case (DimensionType.Aligned):
                    WriteAlignedDimension((AlignedDimension)dim);
                    break;
                case (DimensionType.Linear):
                    WriteLinearDimension((LinearDimension)dim);
                    break;
                case (DimensionType.Radius):
                    WriteRadialDimension((RadialDimension)dim);
                    break;
                case (DimensionType.Diameter):
                    WriteDiametricDimension((DiametricDimension)dim);
                    break;
                case (DimensionType.Angular3Point):
                    WriteAngular3PointDimension((Angular3PointDimension)dim);
                    break;
                case (DimensionType.Angular):
                    WriteAngular2LineDimension((Angular2LineDimension)dim);
                    break;
                case (DimensionType.Ordinate):
                    WriteOrdinateDimension((OrdinateDimension)dim);
                    break;
            }
        }

        private void WriteAlignedDimension(AlignedDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.AlignedDimension);

            this.chunk.Write(13, dim.FirstReferencePoint.X);
            this.chunk.Write(23, dim.FirstReferencePoint.Y);
            this.chunk.Write(33, dim.FirstReferencePoint.Z);

            this.chunk.Write(14, dim.SecondReferencePoint.X);
            this.chunk.Write(24, dim.SecondReferencePoint.Y);
            this.chunk.Write(34, dim.SecondReferencePoint.Z);

            this.WriteXData(dim.XData);
        }

        private void WriteLinearDimension(LinearDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.AlignedDimension);

            this.chunk.Write(13, dim.FirstReferencePoint.X);
            this.chunk.Write(23, dim.FirstReferencePoint.Y);
            this.chunk.Write(33, dim.FirstReferencePoint.Z);

            this.chunk.Write(14, dim.SecondReferencePoint.X);
            this.chunk.Write(24, dim.SecondReferencePoint.Y);
            this.chunk.Write(34, dim.SecondReferencePoint.Z);

            this.chunk.Write(50, dim.Rotation);

            // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the dxf documentation
            // this.chunk.Write(52, dim.ObliqueAngle);

            this.chunk.Write(100, SubclassMarker.LinearDimension);
            
            this.WriteXData(dim.XData);
        }

        private void WriteRadialDimension(RadialDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.RadialDimension);

            this.chunk.Write(15, dim.CircunferencePoint.X);
            this.chunk.Write(25, dim.CircunferencePoint.Y);
            this.chunk.Write(35, dim.CircunferencePoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteDiametricDimension(DiametricDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.DiametricDimension);

            this.chunk.Write(15, dim.CircunferencePoint.X);
            this.chunk.Write(25, dim.CircunferencePoint.Y);
            this.chunk.Write(35, dim.CircunferencePoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular3PointDimension(Angular3PointDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Angular3PointDimension);

            this.chunk.Write(13, dim.FirstPoint.X);
            this.chunk.Write(23, dim.FirstPoint.Y);
            this.chunk.Write(33, dim.FirstPoint.Z);

            this.chunk.Write(14, dim.SecondPoint.X);
            this.chunk.Write(24, dim.SecondPoint.Y);
            this.chunk.Write(34, dim.SecondPoint.Z);

            this.chunk.Write(15, dim.CenterPoint.X);
            this.chunk.Write(25, dim.CenterPoint.Y);
            this.chunk.Write(35, dim.CenterPoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular2LineDimension(Angular2LineDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.Angular2LineDimension);

            this.chunk.Write(13, dim.StartFirstLine.X);
            this.chunk.Write(23, dim.StartFirstLine.Y);
            this.chunk.Write(33, dim.StartFirstLine.Z);

            this.chunk.Write(14, dim.EndFirstLine.X);
            this.chunk.Write(24, dim.EndFirstLine.Y);
            this.chunk.Write(34, dim.EndFirstLine.Z);

            this.chunk.Write(15, dim.StartSecondLine.X);
            this.chunk.Write(25, dim.StartSecondLine.Y);
            this.chunk.Write(35, dim.StartSecondLine.Z);

            this.chunk.Write(16, dim.ArcDefinitionPoint.X);
            this.chunk.Write(26, dim.ArcDefinitionPoint.Y);
            this.chunk.Write(36, dim.ArcDefinitionPoint.Z);

            this.chunk.Write(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteOrdinateDimension(OrdinateDimension dim)
        {
            this.chunk.Write(100, SubclassMarker.OrdinateDimension);

            this.chunk.Write(13, dim.FirstPoint.X);
            this.chunk.Write(23, dim.FirstPoint.Y);
            this.chunk.Write(33, dim.FirstPoint.Z);

            this.chunk.Write(14, dim.SecondPoint.X);
            this.chunk.Write(24, dim.SecondPoint.Y);
            this.chunk.Write(34, dim.SecondPoint.Z);

            this.WriteXData(dim.XData);
        }

        private void WriteImage(Image image)
        {
            this.chunk.Write(100, SubclassMarker.RasterImage);

            this.chunk.Write(10, image.Position.X);
            this.chunk.Write(20, image.Position.Y);
            this.chunk.Write(30, image.Position.Z);

            double factor = MathHelper.ConversionFactor(this.doc.RasterVariables.Units, this.doc.DrawingVariables.InsUnits);
            Vector2 u = MathHelper.Transform(new Vector2(image.Width / image.Definition.Width, 0.0), image.Rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            Vector3 uWcs = MathHelper.Transform(new Vector3(u.X, u.Y, 0.0), image.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            uWcs *= factor;
            this.chunk.Write(11, uWcs.X);
            this.chunk.Write(21, uWcs.Y);
            this.chunk.Write(31, uWcs.Z);

            Vector2 v = MathHelper.Transform(new Vector2(0.0, image.Height / image.Definition.Height), image.Rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            Vector3 vWcs = MathHelper.Transform(new Vector3(v.X, v.Y, 0.0), image.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            vWcs *= factor;
            this.chunk.Write(12, vWcs.X);
            this.chunk.Write(22, vWcs.Y);
            this.chunk.Write(32, vWcs.Z);

            this.chunk.Write(13, (double)image.Definition.Width);
            this.chunk.Write(23, (double)image.Definition.Height);

            this.chunk.Write(340, image.Definition.Handle);

            this.chunk.Write(70, (short)image.DisplayOptions);
            this.chunk.Write(280, image.Clipping ? (short)1 : (short)0);
            this.chunk.Write(281, image.Brightness);
            this.chunk.Write(282, image.Contrast);
            this.chunk.Write(283, image.Fade);
            this.chunk.Write(360, image.Definition.Reactors[image.Handle].Handle);
            this.chunk.Write(71, (short)image.ClippingBoundary.Type);
            this.chunk.Write(91, image.ClippingBoundary.Vertexes.Count);
            foreach (Vector2 vertex in image.ClippingBoundary.Vertexes)
            {
                this.chunk.Write(14, vertex.X);
                this.chunk.Write(24, vertex.Y);
            }

            this.WriteXData(image.XData);

        }

        private void WriteMLine(MLine mLine)
        {
            this.chunk.Write(100, SubclassMarker.MLine);

            this.chunk.Write(2, EncodeNonAsciiCharacters(mLine.Style.Name));

            this.chunk.Write(340, mLine.Style.Handle);

            this.chunk.Write(40, mLine.Scale);
            this.chunk.Write(70, (short)mLine.Justification);
            this.chunk.Write(71, (short)mLine.Flags);
            this.chunk.Write(72, (short)mLine.Vertexes.Count);
            this.chunk.Write(73, (short)mLine.Style.Elements.Count);

            // the MLine information is in OCS we need to saved in WCS
            // this behaviour is similar to the LWPolyline, the info is in OCS because these entities are strictly 2d. Normally they are used in the XY plane whose
            // normal is (0, 0, 1) so no transformation is needed, OCS are equal to WCS
            List<Vector3> ocsVertexes = new List<Vector3>();
            foreach (MLineVertex segment in mLine.Vertexes)
            {
                ocsVertexes.Add(new Vector3(segment.Location.X, segment.Location.Y, mLine.Elevation));
            }
            List<Vector3> wcsVertexes = MathHelper.Transform(ocsVertexes, mLine.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            // Althought it is not recommended the vertex list might have 0 entries
            if (wcsVertexes.Count == 0)
            {
                this.chunk.Write(10, 0.0);
                this.chunk.Write(20, 0.0);
                this.chunk.Write(30, 0.0);
            }
            else
            {
                this.chunk.Write(10, wcsVertexes[0].X);
                this.chunk.Write(20, wcsVertexes[0].Y);
                this.chunk.Write(30, wcsVertexes[0].Z);
            }

            this.chunk.Write(210, mLine.Normal.X);
            this.chunk.Write(220, mLine.Normal.Y);
            this.chunk.Write(230, mLine.Normal.Z);

            for (int i = 0; i < wcsVertexes.Count; i++)
            {
                this.chunk.Write(11, wcsVertexes[i].X);
                this.chunk.Write(21, wcsVertexes[i].Y);
                this.chunk.Write(31, wcsVertexes[i].Z);

                // the directions are written in world coordinates
                Vector2 dir = mLine.Vertexes[i].Direction;
                Vector3 wcsDir = MathHelper.Transform(new Vector3(dir.X, dir.Y, mLine.Elevation), mLine.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
                this.chunk.Write(12, wcsDir.X);
                this.chunk.Write(22, wcsDir.Y);
                this.chunk.Write(32, wcsDir.Z);
                Vector2 mitter = mLine.Vertexes[i].Miter;
                Vector3 wcsMitter = MathHelper.Transform(new Vector3(mitter.X, mitter.Y, mLine.Elevation), mLine.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
                this.chunk.Write(13, wcsMitter.X);
                this.chunk.Write(23, wcsMitter.Y);
                this.chunk.Write(33, wcsMitter.Z);

                foreach (List<double> distances in mLine.Vertexes[i].Distances)
                {
                    this.chunk.Write(74, (short)distances.Count);
                    foreach (double distance in distances)
                    {
                        this.chunk.Write(41, distance);
                    }
                    this.chunk.Write(75, (short)0);
                }
            }

            this.WriteXData(mLine.XData);
        }

        private void WriteAttributeDefinition(AttributeDefinition def)
        {
            this.WriteEntityCommonCodes(def, null);

            this.chunk.Write(100, SubclassMarker.Text);

            this.chunk.Write(10, def.Position.X);
            this.chunk.Write(20, def.Position.Y);
            this.chunk.Write(30, def.Position.Z);
            this.chunk.Write(40, def.Height);

            object value = def.Value;
            if (value == null)
                this.chunk.Write(1, string.Empty);
            else if (value is string)
                this.chunk.Write(1, this.EncodeNonAsciiCharacters((string)value));
            else
                this.chunk.Write(1, value.ToString());

            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short)3);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(72, (short)4);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(72, (short)5);
                    break;
            }

            this.chunk.Write(50, def.Rotation);
            this.chunk.Write(41, def.WidthFactor);

            this.chunk.Write(7, EncodeNonAsciiCharacters(def.Style.Name));

            this.chunk.Write(11, def.Position.X);
            this.chunk.Write(21, def.Position.Y);
            this.chunk.Write(31, def.Position.Z);

            this.chunk.Write(210, def.Normal.X);
            this.chunk.Write(220, def.Normal.Y);
            this.chunk.Write(230, def.Normal.Z);

            this.chunk.Write(100, SubclassMarker.AttributeDefinition);

            this.chunk.Write(3, EncodeNonAsciiCharacters(def.Text));

            this.chunk.Write(2, EncodeNonAsciiCharacters(def.Tag));

            this.chunk.Write(70, (short)def.Flags);

            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(74, (short)0);
                    break;
            }
        }

        private void WriteAttribute(Attribute attrib)
        {
            this.WriteEntityCommonCodes(attrib, null);

            this.chunk.Write(100, SubclassMarker.Text);

            this.chunk.Write(10, attrib.Position.X);
            this.chunk.Write(20, attrib.Position.Y);
            this.chunk.Write(30, attrib.Position.Z);

            this.chunk.Write(40, attrib.Height);
            this.chunk.Write(41, attrib.WidthFactor);

            this.chunk.Write(7, EncodeNonAsciiCharacters(attrib.Style.Name));

            object value = attrib.Value;
            if (value == null)
                this.chunk.Write(1, string.Empty);
            else if (value is string)
                this.chunk.Write(1, this.EncodeNonAsciiCharacters((string)value));
            else
                this.chunk.Write(1, value.ToString());

            switch (attrib.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(72, (short)0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(72, (short)1);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(72, (short)2);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(72, (short)3);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(72, (short)4);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(72, (short)5);
                    break;
            }

            this.chunk.Write(11, attrib.Position.X);
            this.chunk.Write(21, attrib.Position.Y);
            this.chunk.Write(31, attrib.Position.Z);

            this.chunk.Write(50, attrib.Rotation);

            this.chunk.Write(210, attrib.Normal.X);
            this.chunk.Write(220, attrib.Normal.Y);
            this.chunk.Write(230, attrib.Normal.Z);

            this.chunk.Write(100, SubclassMarker.Attribute);

            this.chunk.Write(2, EncodeNonAsciiCharacters(attrib.Tag));

            this.chunk.Write(70, (short)attrib.Flags);

            switch (attrib.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.TopCenter:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.TopRight:
                    this.chunk.Write(74, (short)3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.MiddleRight:
                    this.chunk.Write(74, (short)2);
                    break;
                case TextAlignment.BottomLeft:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BottomCenter:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BottomRight:
                    this.chunk.Write(74, (short)1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.BaselineRight:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Aligned:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Middle:
                    this.chunk.Write(74, (short)0);
                    break;
                case TextAlignment.Fit:
                    this.chunk.Write(74, (short)0);
                    break;
            }
        }

        private void WriteViewport(Viewport viewport)
        {
            this.chunk.Write(100, SubclassMarker.Viewport);

            this.chunk.Write(10, viewport.Center.X);
            this.chunk.Write(20, viewport.Center.Y);
            this.chunk.Write(30, viewport.Center.Z);

            this.chunk.Write(40, viewport.Width);
            this.chunk.Write(41, viewport.Height);
            this.chunk.Write(68, viewport.Stacking);
            this.chunk.Write(69, viewport.Id);

            this.chunk.Write(12, viewport.ViewCenter.X);
            this.chunk.Write(22, viewport.ViewCenter.Y);

            this.chunk.Write(13, viewport.SnapBase.X);
            this.chunk.Write(23, viewport.SnapBase.Y);

            this.chunk.Write(14, viewport.SnapSpacing.X);
            this.chunk.Write(24, viewport.SnapSpacing.Y);

            this.chunk.Write(15, viewport.GridSpacing.X);
            this.chunk.Write(25, viewport.GridSpacing.Y);

            this.chunk.Write(16, viewport.ViewDirection.X);
            this.chunk.Write(26, viewport.ViewDirection.Y);
            this.chunk.Write(36, viewport.ViewDirection.Z);

            this.chunk.Write(17, viewport.ViewTarget.X);
            this.chunk.Write(27, viewport.ViewTarget.Y);
            this.chunk.Write(37, viewport.ViewTarget.Z);

            this.chunk.Write(42, viewport.LensLength);

            this.chunk.Write(43, viewport.FrontClipPlane);
            this.chunk.Write(44, viewport.BackClipPlane);
            this.chunk.Write(45, viewport.ViewHeight);

            this.chunk.Write(50, viewport.SnapAngle);
            this.chunk.Write(51, viewport.TwistAngle);
            this.chunk.Write(72, viewport.CircleZoomPercent);

            foreach (Layer layer in viewport.FrozenLayers)
                this.chunk.Write(331, layer.Handle);        

            this.chunk.Write(90, (int)viewport.Status);

            if(viewport.ClippingBoundary != null) this.chunk.Write(340, viewport.ClippingBoundary.Handle);

            this.chunk.Write(110, viewport.UcsOrigin.X);
            this.chunk.Write(120, viewport.UcsOrigin.Y);
            this.chunk.Write(130, viewport.UcsOrigin.Z);

            this.chunk.Write(111, viewport.UcsXAxis.X);
            this.chunk.Write(121, viewport.UcsXAxis.Y);
            this.chunk.Write(131, viewport.UcsXAxis.Z);

            this.chunk.Write(112, viewport.UcsYAxis.X);
            this.chunk.Write(122, viewport.UcsYAxis.Y);
            this.chunk.Write(132, viewport.UcsYAxis.Z);

            this.WriteXData(viewport.XData);
        }

        #endregion

        #region methods for Object section

        private void WriteDictionary(DictionaryObject dictionary)
        {
            this.chunk.Write(0, StringCode.Dictionary);
            this.chunk.Write(5, dictionary.Handle);
            this.chunk.Write(330, dictionary.Owner.Handle);

            this.chunk.Write(100, SubclassMarker.Dictionary);
            this.chunk.Write(280, dictionary.IsHardOwner ? (short)1 : (short)0);
            this.chunk.Write(281, (short)dictionary.Clonning);

            if (dictionary.Entries == null) return;

            foreach (KeyValuePair<string, string> entry in dictionary.Entries)
            {
                this.chunk.Write(3, EncodeNonAsciiCharacters(entry.Value));
                this.chunk.Write(350, entry.Key);
            }
        }

        private void WriteImageDefReactor(ImageDefReactor reactor)
        {
            this.chunk.Write(0, reactor.CodeName);
            this.chunk.Write(5, reactor.Handle);
            this.chunk.Write(330, reactor.ImageHandle);

            this.chunk.Write(100, SubclassMarker.RasterImageDefReactor);
            this.chunk.Write(90, 2);
            this.chunk.Write(330, reactor.ImageHandle);
        }

        private void WriteImageDef(ImageDef imageDef, string ownerHandle)
        {
            this.chunk.Write(0, imageDef.CodeName);
            this.chunk.Write(5, imageDef.Handle);

            this.chunk.Write(102, "{ACAD_REACTORS");
            this.chunk.Write(330, ownerHandle);
            foreach (ImageDefReactor reactor in imageDef.Reactors.Values)
            {
                this.chunk.Write(330, reactor.Handle);
            }
            this.chunk.Write(102, "}");

            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.RasterImageDef);
            this.chunk.Write(1, imageDef.FileName);

            this.chunk.Write(10, (double)imageDef.Width);
            this.chunk.Write(20, (double)imageDef.Height);

            // The documentation says that this is the size of one pixel in AutoCAD units, but it seems that this is always the size of one pixel in milimeters
            // this value is used to calculate the image resolution in ppi or ppc, and the default image size.
            double factor = MathHelper.ConversionFactor((ImageUnits)imageDef.ResolutionUnits, DrawingUnits.Millimeters);
            this.chunk.Write(11, factor / imageDef.HorizontalResolution);
            this.chunk.Write(21, factor / imageDef.VerticalResolution);

            this.chunk.Write(280, (short)1);
            this.chunk.Write(281, (short)imageDef.ResolutionUnits);

        }

        private void WriteRasterVariables(RasterVariables variables, string ownerHandle)
        {
            this.chunk.Write(0, variables.CodeName);
            this.chunk.Write(5, variables.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.RasterVariables);
            this.chunk.Write(90, 0);
            this.chunk.Write(70, variables.DisplayFrame ? (short)1 : (short)0);
            this.chunk.Write(71, (short)variables.DisplayQuality);
            this.chunk.Write(72, (short)variables.Units);
        }

        private void WriteMLineStyle(MLineStyle style, string ownerHandle)
        {
            this.chunk.Write(0, style.CodeName);
            this.chunk.Write(5, style.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.MLineStyle);

            this.chunk.Write(2, EncodeNonAsciiCharacters(style.Name));

            this.chunk.Write(70, (short)style.Flags);

            this.chunk.Write(3, EncodeNonAsciiCharacters(style.Description));

            this.chunk.Write(62, style.FillColor.Index);
            if (style.FillColor.UseTrueColor) // && this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
                this.chunk.Write(420, AciColor.ToTrueColor(style.FillColor));
            this.chunk.Write(51, style.StartAngle);
            this.chunk.Write(52, style.EndAngle);
            this.chunk.Write(71, (short)style.Elements.Count);
            foreach (MLineStyleElement element in style.Elements)
            {
                this.chunk.Write(49, element.Offset);
                this.chunk.Write(62, element.Color.Index);
                if (element.Color.UseTrueColor) // && this.doc.DrawingVariables.AcadVer > DxfVersion.AutoCad2000)
                    this.chunk.Write(420, AciColor.ToTrueColor(element.Color));

                this.chunk.Write(6, EncodeNonAsciiCharacters(element.LineType.Name));
            }
        }

        private void WriteGroup(Group group, string ownerHandle)
        {
            this.chunk.Write(0, group.CodeName);
            this.chunk.Write(5, group.Handle);
            this.chunk.Write(330, ownerHandle);

            this.chunk.Write(100, SubclassMarker.Group);

            this.chunk.Write(300, EncodeNonAsciiCharacters(group.Description));
            this.chunk.Write(70, group.IsUnnamed ? (short)1 : (short)0);
            this.chunk.Write(71, group.IsSelectable ? (short)1 : (short)0);

            foreach (EntityObject entity in group.Entities)
            {
                this.chunk.Write(340, entity.Handle);
            }
        }

        private void WriteLayout(Layout layout, string ownerHandle)
        {
            this.chunk.Write(0, layout.CodeName);
            this.chunk.Write(5, layout.Handle);
            this.chunk.Write(330, ownerHandle);

            PlotSettings plot = layout.PlotSettings;
            this.chunk.Write(100, SubclassMarker.PlotSettings);
            this.chunk.Write(1, plot.PageSetupName);
            this.chunk.Write(2, plot.PlotterName);
            this.chunk.Write(4, plot.PaperSizeName);
            this.chunk.Write(6, plot.ViewName);

            this.chunk.Write(40, plot.LeftMargin);
            this.chunk.Write(41, plot.BottomMargin);
            this.chunk.Write(42, plot.RightMargin);
            this.chunk.Write(43, plot.TopMargin);
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
            this.chunk.Write(70, (short)plot.Flags);
            this.chunk.Write(72, (short)plot.PaperUnits);
            this.chunk.Write(73, (short)plot.PaperRotation);
            this.chunk.Write(74, (short)5);
            this.chunk.Write(7, "");
            this.chunk.Write(75, (short)16);

            this.chunk.Write(147, plot.PrintScale);
            this.chunk.Write(148, plot.PaperImageOrigin.X);
            this.chunk.Write(149, plot.PaperImageOrigin.Y);

            this.chunk.Write(100, SubclassMarker.Layout);
            this.chunk.Write(1, this.EncodeNonAsciiCharacters(layout.Name));
            this.chunk.Write(70, (short)1);
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

            this.chunk.Write(76, (short)0);

            this.chunk.Write(330, layout.AssociatedBlock.Owner.Handle);
        }

        #endregion

        #region private methods

        private string EncodeNonAsciiCharacters(string text)
        {
            // for dxf database version prior to AutoCad 2007 non ASCII characters, including the extended chart, must be encoded to the template \U+####,
            // where #### is the for digits hexadecimal number that represent that character.
            if (this.doc.DrawingVariables.AcadVer >= DxfVersion.AutoCad2007) return text;

            if (string.IsNullOrEmpty(text)) return string.Empty;

            string encoded;
            if(this.encodedStrings.TryGetValue(text, out encoded)) return encoded;

            StringBuilder sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 255)
                    sb.Append(string.Concat("\\U+", String.Format("{0:X4}", Convert.ToInt32(c))));
                else
                    sb.Append(c);
            }

            encoded = sb.ToString();
            this.encodedStrings.Add(text, encoded);
            return encoded;

            // encoding of non ASCII characters, including the extended chart, using regular expresions, this code is slower
            //return Regex.Replace(
            //    text,
            //    @"(?<char>[^\u0000-\u00ff]{1})",
            //    m => "\\U+" + String.Format("{0:X4}", Convert.ToInt32(m.Groups["char"].Value[0])));

        }

        private double[] GetEllipseParameters(Ellipse ellipse)
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
                Vector2 startPoint = new Vector2(ellipse.Center.X, ellipse.Center.Y) + ellipse.PolarCoordinateRelativeToCenter(ellipse.StartAngle);
                Vector2 endPoint = new Vector2(ellipse.Center.X, ellipse.Center.Y) + ellipse.PolarCoordinateRelativeToCenter(ellipse.EndAngle);
                double a = ellipse.MajorAxis * 0.5;
                double b = ellipse.MinorAxis * 0.5;
                double px1 = ((startPoint.X - ellipse.Center.X) / a);
                double py1 = ((startPoint.Y - ellipse.Center.Y) / b);
                double px2 = ((endPoint.X - ellipse.Center.X) / a);
                double py2 = ((endPoint.Y - ellipse.Center.Y) / b);

                atan1 = Math.Atan2(py1, px1);
                atan2 = Math.Atan2(py2, px2);
            }
            return new[] { atan1, atan2 };
        }

        private void WriteXData(Dictionary<string, XData> xData)
        {
            foreach (string appReg in xData.Keys)
            {
                this.chunk.Write(XDataCode.AppReg, EncodeNonAsciiCharacters(appReg));

                foreach (XDataRecord x in xData[appReg].XDataRecord)
                {
                    short code = x.Code;
                    object value = x.Value;
                    if (code == 1000 || code == 1003)
                    {
                        this.chunk.Write(code, EncodeNonAsciiCharacters((string)value));

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
                        this.chunk.Write(code, value);
                }
            }
        }

        #endregion
    }
}