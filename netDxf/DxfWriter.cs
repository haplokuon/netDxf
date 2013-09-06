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
using System.IO;
using System.Text;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute=netDxf.Entities.Attribute;

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
        private bool isHeader;
        private bool isClassesSection;
        private bool isTableSection;
        private bool isBlockDefinition;
        private bool isEntitiesSection;
        private bool isObjectsSection;
        private TextWriter writer;
        private readonly DxfVersion version;

        #endregion

        #region constructors

        public DxfWriter(DxfVersion version)
        {
            this.version = version;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the active section.
        /// </summary>
        public String ActiveSection
        {
            get { return this.activeSection; }
        }

        /// <summary>
        /// Gets the dxf file version.
        /// </summary>
        public DxfVersion Version
        {
            get { return this.version; }
        }

        #endregion

        #region public methods

        public void Open(Stream stream)
        {
            try
            {
                // Encoding with the actual system default, the most common one is 1252 Latin 1; Western European (Windows)
                this.writer = new StreamWriter(stream, Encoding.Default);
            }
            catch (Exception ex)
            {
                throw (new DxfException("Error when trying to create the dxf reader.", ex));
            }
        }

        /// <summary>
        /// Closes the dxf file.
        /// </summary>
        public void Close()
        {
            if (this.activeSection != StringCode.Unknown)
            {
                throw new OpenDxfSectionException(this.activeSection);
            }
            if (this.activeTable != StringCode.Unknown)
            {
                throw new OpenDxfTableException(this.activeTable);
            }
            this.WriteCodePair(0, StringCode.EndOfFile);

            this.writer.Flush();
            //this.writer.Close();
        }

        /// <summary>
        /// Opens a new section.
        /// </summary>
        /// <param name="section">Section type to open.</param>
        /// <remarks>There can be only one type section.</remarks>
        public void BeginSection(string section)
        {
            if (this.activeSection != StringCode.Unknown)
            {
                throw new OpenDxfSectionException(this.activeSection);
            }

            this.WriteCodePair(0, StringCode.BeginSection);

            if (section == StringCode.HeaderSection)
            {
                if (this.isHeader)
                {
                    throw (new ClosedDxfSectionException(StringCode.HeaderSection));
                }
                this.WriteCodePair(2, StringCode.HeaderSection);
                this.isHeader = true;
            }
            if (section == StringCode.ClassesSection)
            {
                if (this.isClassesSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.ClassesSection));
                }
                this.WriteCodePair(2, StringCode.ClassesSection);
                this.isClassesSection = true;
            }
            if (section == StringCode.TablesSection)
            {
                if (this.isTableSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.TablesSection));
                }
                this.WriteCodePair(2, StringCode.TablesSection);
                this.isTableSection = true;
            }
            if (section == StringCode.BlocksSection)
            {
                if (this.isBlockDefinition)
                {
                    throw (new ClosedDxfSectionException(StringCode.BlocksSection));
                }
                this.WriteCodePair(2, StringCode.BlocksSection);
                this.isBlockDefinition = true;
            }
            if (section == StringCode.EntitiesSection)
            {
                if (this.isEntitiesSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.EntitiesSection));
                }
                this.WriteCodePair(2, StringCode.EntitiesSection);
                this.isEntitiesSection = true;
            }
            if (section == StringCode.ObjectsSection)
            {
                if (this.isObjectsSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.ObjectsSection));
                }
                this.WriteCodePair(2, StringCode.ObjectsSection);
                this.isObjectsSection = true;
            }
            this.activeSection = section;
        }

        /// <summary>
        /// Closes the active section.
        /// </summary>
        public void EndSection()
        {
            if (this.activeSection == StringCode.Unknown)
            {
                throw new ClosedDxfSectionException(StringCode.Unknown);
            }
            this.WriteCodePair(0, StringCode.EndSection);
            switch (this.activeSection)
            {
                case StringCode.HeaderSection:
                    this.isEntitiesSection = false;
                    break;
                case StringCode.ClassesSection:
                    this.isEntitiesSection = false;
                    break;
                case StringCode.TablesSection:
                    this.isTableSection = false;
                    break;
                case StringCode.BlocksSection:
                    this.isBlockDefinition = true;
                    break;
                case StringCode.EntitiesSection:
                    this.isEntitiesSection = false;
                    break;
                case StringCode.ObjectsSection:
                    this.isEntitiesSection = false;
                    break;
            }
            this.activeSection = StringCode.Unknown;
        }

        /// <summary>
        /// Opens a new table.
        /// </summary>
        /// <param name="table">Table type to open.</param>
        /// <param name="handle">Handle assigned to this table</param>
        public void BeginTable(string table, string handle)
        {
            if (this.activeTable != StringCode.Unknown)
            {
                throw new OpenDxfTableException(table);
            }
            this.WriteCodePair(0, StringCode.Table);
            this.WriteCodePair(2, table);
            this.WriteCodePair(5, handle);
            this.WriteCodePair(100, SubclassMarker.Table);

            if (table == StringCode.DimensionStyleTable)
                this.WriteCodePair(100, SubclassMarker.DimensionStyleTable);
            this.activeTable = table;
        }

        /// <summary>
        /// Closes the active table.
        /// </summary>
        public void EndTable()
        {
            if (this.activeTable == StringCode.Unknown)
            {
                throw new ClosedDxfTableException(StringCode.Unknown);
            }

            this.WriteCodePair(0, StringCode.EndTable);
            this.activeTable = StringCode.Unknown;
        }

        #endregion

        #region methods for Header section

        public void WriteComment(string comment)
        {
            if (!string.IsNullOrEmpty(comment))
                this.WriteCodePair(999, comment);
        }

        public void WriteSystemVariable(HeaderVariable variable)
        {
            if (this.activeSection != StringCode.HeaderSection)
                throw new InvalidDxfSectionException(this.activeSection);

            // the LastSavedBy header variable is not recognized in AutoCad2000 dxf files, so it will not be written
            if (variable.Name == HeaderVariableCode.LastSavedBy && this.version <= DxfVersion.AutoCad2000)
                return;
            this.WriteCodePair(9, variable.Name);
            this.WriteCodePair(variable.CodeGroup, variable.Value);
        }

        #endregion

        #region methods for Classes section

        public void WriteImageClass(int count)
        {
            this.WriteCodePair(0, StringCode.Class);
            this.WriteCodePair(1, DxfObjectCode.Image);
            this.WriteCodePair(2, SubclassMarker.RasterImage);
            this.WriteCodePair(3, "ISM");

            // default codes as shown in the dxf documentation
            this.WriteCodePair(90, 127);
            if (this.version > DxfVersion.AutoCad2000) this.WriteCodePair(91, count);
            this.WriteCodePair(280, 0);
            this.WriteCodePair(281, 1);
        }

        public void WriteImageDefClass(int count)
        {
            this.WriteCodePair(0, StringCode.Class);
            this.WriteCodePair(1, DxfObjectCode.ImageDef);
            this.WriteCodePair(2, SubclassMarker.RasterImageDef);
            this.WriteCodePair(3, "ISM");

            // default codes as shown in the dxf documentation
            this.WriteCodePair(90, 0);
            if (this.version > DxfVersion.AutoCad2000) this.WriteCodePair(91, count);
            this.WriteCodePair(280, 0);
            this.WriteCodePair(281, 0);
        }

        public void WriteImageDefRectorClass(int count)
        {
            this.WriteCodePair(0, StringCode.Class);
            this.WriteCodePair(1, DxfObjectCode.ImageDefReactor);
            this.WriteCodePair(2, SubclassMarker.RasterImageDefReactor);
            this.WriteCodePair(3, "ISM");

            // default codes as shown in the dxf documentation
            this.WriteCodePair(90, 1);
            if (this.version > DxfVersion.AutoCad2000) this.WriteCodePair(91, count);
            this.WriteCodePair(280, 0);
            this.WriteCodePair(281, 0);
        }

        public void WriteRasterVariablesClass(int count)
        {
            this.WriteCodePair(0, StringCode.Class);
            this.WriteCodePair(1, DxfObjectCode.RasterVariables);
            this.WriteCodePair(2, SubclassMarker.RasterVariables);
            this.WriteCodePair(3, "ISM");

            // default codes as shown in the dxf documentation
            this.WriteCodePair(90, 0);
            if (this.version > DxfVersion.AutoCad2000) this.WriteCodePair(91, count);
            this.WriteCodePair(280, 0);
            this.WriteCodePair(281, 0);
        }

        #endregion

        #region methods for Table section

        /// <summary>
        /// Writes a new extended data application registry to the table section.
        /// </summary>
        /// <param name="appReg">Name of the application registry.</param>
        public void RegisterApplication(ApplicationRegistry appReg)
        {
            if (this.activeTable != StringCode.ApplicationIDTable)
            {
                throw new InvalidDxfTableException(StringCode.ApplicationIDTable);
            }

            this.WriteCodePair(0, StringCode.ApplicationIDTable);
            this.WriteCodePair(5, appReg.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);
            this.WriteCodePair(100, SubclassMarker.ApplicationId);
            this.WriteCodePair(2, appReg);
            this.WriteCodePair(70, 0);
        }

        /// <summary>
        /// Writes a new view port to the table section.
        /// </summary>
        /// <param name="vp">Viewport.</param>
        public void WriteViewPort(Viewport vp)
        {
            if (this.activeTable != StringCode.ViewPortTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }
            this.WriteCodePair(0, vp.CodeName);
            this.WriteCodePair(5, vp.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.ViewPort);
            this.WriteCodePair(2, vp);
            this.WriteCodePair(70, 0);

            this.WriteCodePair(10, vp.LowerLeftCorner.X);
            this.WriteCodePair(20, vp.LowerLeftCorner.Y);

            this.WriteCodePair(11, vp.UpperRightCorner.X);
            this.WriteCodePair(21, vp.UpperRightCorner.Y);

            this.WriteCodePair(12, vp.LowerLeftCorner.X - vp.UpperRightCorner.X);
            this.WriteCodePair(22, vp.UpperRightCorner.Y - vp.LowerLeftCorner.Y);

            this.WriteCodePair(13, vp.SnapBasePoint.X);
            this.WriteCodePair(23, vp.SnapBasePoint.Y);

            this.WriteCodePair(14, vp.SnapSpacing.X);
            this.WriteCodePair(24, vp.SnapSpacing.Y);

            this.WriteCodePair(15, vp.GridSpacing.X);
            this.WriteCodePair(25, vp.GridSpacing.Y);

            Vector3 dir = vp.Camera - vp.Target;
            this.WriteCodePair(16, dir.X);
            this.WriteCodePair(26, dir.Y);
            this.WriteCodePair(36, dir.Z);

            this.WriteCodePair(17, vp.Target.X);
            this.WriteCodePair(27, vp.Target.Y);
            this.WriteCodePair(37, vp.Target.Z);
        }

        /// <summary>
        /// Writes a new dimension style to the table section.
        /// </summary>
        /// <param name="style">DimensionStyle.</param>
        public void WriteDimensionStyle(DimensionStyle style)
        {
            if (this.activeTable != StringCode.DimensionStyleTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }
            this.WriteCodePair(0, style.CodeName);
            this.WriteCodePair(105, style.Handle);

            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.DimensionStyle);

            this.WriteCodePair(2, style);

            // flags
            this.WriteCodePair(70, 0);

            this.WriteCodePair(3, style.DIMPOST);
            this.WriteCodePair(41, style.DIMASZ);
            this.WriteCodePair(42, style.DIMEXO);
            this.WriteCodePair(44, style.DIMEXE);
            this.WriteCodePair(140, style.DIMTXT);
            this.WriteCodePair(147, style.DIMGAP);
            this.WriteCodePair(73, style.DIMTIH);
            this.WriteCodePair(74, style.DIMTOH);
            this.WriteCodePair(77, style.DIMTAD);
            this.WriteCodePair(271, style.DIMDEC);
            this.WriteCodePair(275, style.DIMAUNIT);
            this.WriteCodePair(278, (int)style.DIMDSEP[0]);
            this.WriteCodePair(280, style.DIMJUST);

            this.WriteCodePair(340, style.TextStyle.Handle);
        }

        /// <summary>
        /// Writes a new block record to the table section.
        /// </summary>
        /// <param name="blockRecord">Block.</param>
        public void WriteBlockRecord(BlockRecord blockRecord)
        {
            if (this.activeTable != StringCode.BlockRecordTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }
            this.WriteCodePair(0, blockRecord.CodeName);
            this.WriteCodePair(5, blockRecord.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.BlockRecord);

            this.WriteCodePair(2, blockRecord.Name);

            // Hard-pointer ID/handle to associated LAYOUT object
            this.WriteCodePair(340, 0);

            this.WriteCodePair(70, (int)blockRecord.Units);
        }

        /// <summary>
        /// Writes a new line type to the table section.
        /// </summary>
        /// <param name="tl">Line type.</param>
        public void WriteLineType(LineType tl)
        {
            if (this.activeTable != StringCode.LineTypeTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }

            this.WriteCodePair(0, tl.CodeName);
            this.WriteCodePair(5, tl.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.LineType);

            this.WriteCodePair(70, 0);
            this.WriteCodePair(2, tl);
            this.WriteCodePair(3, tl.Description);
            this.WriteCodePair(72, 65);
            this.WriteCodePair(73, tl.Segments.Count);
            this.WriteCodePair(40, tl.Length());
            foreach (double s in tl.Segments)
            {
                this.WriteCodePair(49, s);
                this.WriteCodePair(74, 0);
            }
        }

        /// <summary>
        /// Writes a new layer to the table section.
        /// </summary>
        /// <param name="layer">Layer.</param>
        public void WriteLayer(Layer layer)
        {
            if (this.activeTable != StringCode.LayerTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }

            this.WriteCodePair(0, layer.CodeName);
            this.WriteCodePair(5, layer.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.Layer);
            this.WriteCodePair(2, layer.Name);
            this.WriteCodePair(70, 0);
            //a negative color represents a hidden layer.
            if (layer.IsVisible)
                this.WriteCodePair(62, layer.Color.Index);
            else
                this.WriteCodePair(62, -layer.Color.Index);
            if (layer.Color.UseTrueColor)
                this.WriteCodePair(420, AciColor.ToTrueColor(layer.Color));
            this.WriteCodePair(6, layer.LineType.Name);
            this.WriteCodePair(290, layer.Plot ? 1 : 0);
            this.WriteCodePair(370, layer.Lineweight.Value);
            // Hard pointer ID/handle of PlotStyleName object
            this.WriteCodePair(390, 0);
        }

        /// <summary>
        /// Writes a new text style to the table section.
        /// </summary>
        /// <param name="style">TextStyle.</param>
        public void WriteTextStyle(TextStyle style)
        {
            if (this.activeTable != StringCode.TextStyleTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }

            this.WriteCodePair(0, style.CodeName);
            this.WriteCodePair(5, style.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.TextStyle);

            this.WriteCodePair(2, style);
            this.WriteCodePair(3, style.FontName);

            this.WriteCodePair(70, style.IsVertical ? 4 : 0);

            if (style.IsBackward && style.IsUpsideDown)
            {
                this.WriteCodePair(71, 6);
            }
            else if (style.IsBackward)
            {
                this.WriteCodePair(71, 2);
            }
            else if (style.IsUpsideDown)
            {
                this.WriteCodePair(71, 4);
            }
            else
            {
                this.WriteCodePair(71, 0);
            }

            this.WriteCodePair(40, style.Height);
            this.WriteCodePair(41, style.WidthFactor);
            this.WriteCodePair(42, style.Height);
            this.WriteCodePair(50, style.ObliqueAngle);
        }

        /// <summary>
        /// Writes a new user coordinate system to the table section.
        /// </summary>
        /// <param name="ucs">UCS.</param>
        public void WriteUCS(UCS ucs)
        {
            if (this.activeTable != StringCode.UcsTable)
            {
                throw new InvalidDxfTableException(this.activeTable);
            }

            this.WriteCodePair(0, ucs.CodeName);
            this.WriteCodePair(5, ucs.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.Ucs);

            this.WriteCodePair(2, ucs.Name);

            this.WriteCodePair(70, 0);

            this.WriteCodePair(10, ucs.Origin.X);
            this.WriteCodePair(20, ucs.Origin.Y);
            this.WriteCodePair(30, ucs.Origin.Z);

            this.WriteCodePair(11, ucs.XAxis.X);
            this.WriteCodePair(21, ucs.XAxis.Y);
            this.WriteCodePair(31, ucs.XAxis.Z);

            this.WriteCodePair(12, ucs.YAxis.X);
            this.WriteCodePair(22, ucs.YAxis.Y);
            this.WriteCodePair(32, ucs.YAxis.Z);

            this.WriteCodePair(79, 0);

            this.WriteCodePair(146, ucs.Elevation);
        }

        #endregion

        #region methods for Block section

        public void WriteBlock(Block block)
        {
            if (this.activeSection != StringCode.BlocksSection)
            {
                throw new InvalidDxfSectionException(this.activeSection);
            }

            this.WriteCodePair(0, block.CodeName);
            this.WriteCodePair(5, block.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, block.Layer);

            this.WriteCodePair(100, SubclassMarker.BlockBegin);

            this.WriteCodePair(2, block);

            //flags
            this.WriteCodePair(70, block.Attributes.Count == 0 ? (int)block.TypeFlags : (int)(block.TypeFlags | BlockTypeFlags.NonConstantAttributeDefinitions));

            this.WriteCodePair(10, block.Position.X);
            this.WriteCodePair(20, block.Position.Y);
            this.WriteCodePair(30, block.Position.Z);

            this.WriteCodePair(3, block);

            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.WriteAttributeDefinition(attdef);
            }

            foreach (EntityObject entity in block.Entities)
            {
                this.WriteEntity(entity, true);
            }

            this.WriteBlockEnd(block.End);
        }

        public void WriteBlockEnd(BlockEnd blockEnd)
        {
            this.WriteCodePair(0, blockEnd.CodeName);
            this.WriteCodePair(5, blockEnd.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, blockEnd.Layer);

            this.WriteCodePair(100, SubclassMarker.BlockEnd);
        }

        #endregion

        #region methods for Entity section

        public void WriteEntityCommonCodes(EntityObject entity)
        {
            this.WriteCodePair(0, entity.CodeName);
            this.WriteCodePair(5, entity.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, entity.Layer);
            this.WriteCodePair(62, entity.Color.Index);
            if (entity.Color.UseTrueColor)
                this.WriteCodePair(420, AciColor.ToTrueColor(entity.Color));
            this.WriteCodePair(6, entity.LineType);
            this.WriteCodePair(370, entity.Lineweight.Value);
            this.WriteCodePair(78, entity.LineTypeScale);
            this.WriteCodePair(60, entity.IsVisible ? 0 : 1);
        }

        public void WriteEntity(EntityObject entity, bool isBlockEntity = false)
        {
            if (this.activeSection != StringCode.EntitiesSection && !isBlockEntity)
                throw new InvalidDxfSectionException(this.activeSection);

            WriteEntityCommonCodes(entity);

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
                default:
                    throw new DxfEntityException(entity.Type.ToString(), "Entity unknown." );
                    
            }
        }

        private void WriteArc(Arc arc)
        {
            this.WriteCodePair(100, SubclassMarker.Circle);

            this.WriteCodePair(39, arc.Thickness);

            // this is just an example of the weird autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of an arc is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 ocsCenter = MathHelper.Transform(arc.Center, arc.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.WriteCodePair(10, ocsCenter.X);
            this.WriteCodePair(20, ocsCenter.Y);
            this.WriteCodePair(30, ocsCenter.Z);

            this.WriteCodePair(40, arc.Radius);

            this.WriteCodePair(210, arc.Normal.X);
            this.WriteCodePair(220, arc.Normal.Y);
            this.WriteCodePair(230, arc.Normal.Z);

            this.WriteCodePair(100, SubclassMarker.Arc);
            this.WriteCodePair(50, arc.StartAngle);
            this.WriteCodePair(51, arc.EndAngle);

            this.WriteXData(arc.XData);
        }

        private void WriteCircle(Circle circle)
        {
            this.WriteCodePair(100, SubclassMarker.Circle);
            
            // this is just an example of the stupid autodesk dxf way of doing things, while an ellipse the center is given in world coordinates,
            // the center of a circle is given in object coordinates (different rules for the same concept).
            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal..
            Vector3 ocsCenter = MathHelper.Transform(circle.Center, circle.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.WriteCodePair(10, ocsCenter.X);
            this.WriteCodePair(20, ocsCenter.Y);
            this.WriteCodePair(30, ocsCenter.Z);

            this.WriteCodePair(40, circle.Radius);

            this.WriteCodePair(39, circle.Thickness);

            this.WriteCodePair(210, circle.Normal.X);
            this.WriteCodePair(220, circle.Normal.Y);
            this.WriteCodePair(230, circle.Normal.Z);

            this.WriteXData(circle.XData);
        }

        private void WriteEllipse(Ellipse ellipse)
        {
            this.WriteCodePair(100, SubclassMarker.Ellipse);

            this.WriteCodePair(10, ellipse.Center.X);
            this.WriteCodePair(20, ellipse.Center.Y);
            this.WriteCodePair(30, ellipse.Center.Z);


            double sine = 0.5 * ellipse.MajorAxis*Math.Sin(ellipse.Rotation*MathHelper.DegToRad);
            double cosine = 0.5 * ellipse.MajorAxis * Math.Cos(ellipse.Rotation * MathHelper.DegToRad);
            Vector3 axisPoint = MathHelper.Transform(new Vector3(cosine, sine, 0),
                                                      ellipse.Normal,
                                                      MathHelper.CoordinateSystem.Object,
                                                      MathHelper.CoordinateSystem.World);

            this.WriteCodePair(11, axisPoint.X);
            this.WriteCodePair(21, axisPoint.Y);
            this.WriteCodePair(31, axisPoint.Z);

            this.WriteCodePair(210, ellipse.Normal.X);
            this.WriteCodePair(220, ellipse.Normal.Y);
            this.WriteCodePair(230, ellipse.Normal.Z);

            this.WriteCodePair(40, ellipse.MinorAxis/ellipse.MajorAxis);

            double[] paramaters = ellipse.GetParameters();
            this.WriteCodePair(41, paramaters[0]);
            this.WriteCodePair(42, paramaters[1]);

            this.WriteXData(ellipse.XData);
        }

        private void WriteSolid(Solid solid)
        {
            this.WriteCodePair(100, SubclassMarker.Solid);

            this.WriteCodePair(10, solid.FirstVertex.X);
            this.WriteCodePair(20, solid.FirstVertex.Y);
            this.WriteCodePair(30, solid.FirstVertex.Z);

            this.WriteCodePair(11, solid.SecondVertex.X);
            this.WriteCodePair(21, solid.SecondVertex.Y);
            this.WriteCodePair(31, solid.SecondVertex.Z);

            this.WriteCodePair(12, solid.ThirdVertex.X);
            this.WriteCodePair(22, solid.ThirdVertex.Y);
            this.WriteCodePair(32, solid.ThirdVertex.Z);

            this.WriteCodePair(13, solid.FourthVertex.X);
            this.WriteCodePair(23, solid.FourthVertex.Y);
            this.WriteCodePair(33, solid.FourthVertex.Z);

            this.WriteCodePair(39, solid.Thickness);

            this.WriteCodePair(210, solid.Normal.X);
            this.WriteCodePair(220, solid.Normal.Y);
            this.WriteCodePair(230, solid.Normal.Z);

            this.WriteXData(solid.XData);
        }

        private void WriteFace3D(Face3d face)
        {
            this.WriteCodePair(100, SubclassMarker.Face3d);

            this.WriteCodePair(10, face.FirstVertex.X);
            this.WriteCodePair(20, face.FirstVertex.Y);
            this.WriteCodePair(30, face.FirstVertex.Z);

            this.WriteCodePair(11, face.SecondVertex.X);
            this.WriteCodePair(21, face.SecondVertex.Y);
            this.WriteCodePair(31, face.SecondVertex.Z);

            this.WriteCodePair(12, face.ThirdVertex.X);
            this.WriteCodePair(22, face.ThirdVertex.Y);
            this.WriteCodePair(32, face.ThirdVertex.Z);

            this.WriteCodePair(13, face.FourthVertex.X);
            this.WriteCodePair(23, face.FourthVertex.Y);
            this.WriteCodePair(33, face.FourthVertex.Z);

            this.WriteCodePair(70, Convert.ToInt32(face.EdgeFlags));

            this.WriteXData(face.XData);
        }

        private void WriteSpline(Spline spline)
        {
            this.WriteCodePair(100, SubclassMarker.Spline);

            int flag = (int) spline.Flags;
            if (spline.IsClosed) flag += (int)SplineTypeFlags.Unknown4;
            this.WriteCodePair(70, flag);
            this.WriteCodePair(71, spline.Degree);
            this.WriteCodePair(72, spline.Knots.Length);
            this.WriteCodePair(73, spline.ControlPoints.Count);

            //this.WriteCodePair(74, 0); Number of fit points (if any).

            //this.WriteCodePair(42, 0); 42 Knot tolerance (default = 0.0000001)
            //this.WriteCodePair(43, 0); 43 Control-point tolerance (default = 0.0000001)
            //this.WriteCodePair(44, 0); 44 Fit tolerance (default = 0.0000000001)


            foreach (double knot in spline.Knots)
                this.WriteCodePair(40, knot);

            foreach (SplineVertex point in spline.ControlPoints)
            {
                this.WriteCodePair(41, point.Weigth);
                this.WriteCodePair(10, point.Location.X);
                this.WriteCodePair(20, point.Location.Y);
                this.WriteCodePair(30, point.Location.Z);
            }
            
            this.WriteXData(spline.XData);
        }

        private void WriteInsert(Insert insert)
        {
            this.WriteCodePair(100, SubclassMarker.Insert);

            this.WriteCodePair(2, insert.Block);

            // It is a lot more intuitive to give the center in world coordinates and then define the orientation with the normal.
            Vector3 ocsInsertion = MathHelper.Transform(insert.Position, insert.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.WriteCodePair(10, ocsInsertion.X);
            this.WriteCodePair(20, ocsInsertion.Y);
            this.WriteCodePair(30, ocsInsertion.Z);

            this.WriteCodePair(41, insert.Scale.X);
            this.WriteCodePair(42, insert.Scale.Y);
            this.WriteCodePair(43, insert.Scale.Z);

            this.WriteCodePair(50, insert.Rotation);

            this.WriteCodePair(210, insert.Normal.X);
            this.WriteCodePair(220, insert.Normal.Y);
            this.WriteCodePair(230, insert.Normal.Z);

            if (insert.Attributes.Count > 0)
            {
                //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
                //but its needed to load the dxf file in AutoCAD
                this.WriteCodePair(66, "1");

                this.WriteXData(insert.XData);

                foreach (Attribute attrib in insert.Attributes)
                {
                    this.WriteAttribute(attrib);
                }

                this.WriteCodePair(0, insert.EndSequence.CodeName);
                this.WriteCodePair(5, insert.EndSequence.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, insert.EndSequence.Layer);
            }
            else
            {
                this.WriteXData(insert.XData);
            }
        }

        private void WriteLine(Line line)
        {            
            this.WriteCodePair(100, SubclassMarker.Line);

            this.WriteCodePair(10, line.StartPoint.X);
            this.WriteCodePair(20, line.StartPoint.Y);
            this.WriteCodePair(30, line.StartPoint.Z);

            this.WriteCodePair(11, line.EndPoint.X);
            this.WriteCodePair(21, line.EndPoint.Y);
            this.WriteCodePair(31, line.EndPoint.Z);

            this.WriteCodePair(39, line.Thickness);

            this.WriteCodePair(210, line.Normal.X);
            this.WriteCodePair(220, line.Normal.Y);
            this.WriteCodePair(230, line.Normal.Z);

            this.WriteXData(line.XData);
        }

        private void WriteRay(Ray ray)
        {
            this.WriteCodePair(100, SubclassMarker.Ray);

            this.WriteCodePair(10, ray.Origin.X);
            this.WriteCodePair(20, ray.Origin.Y);
            this.WriteCodePair(30, ray.Origin.Z);

            this.WriteCodePair(11, ray.Direction.X);
            this.WriteCodePair(21, ray.Direction.Y);
            this.WriteCodePair(31, ray.Direction.Z);

            this.WriteXData(ray.XData);
        }

        private void WriteXLine(XLine xline)
        {
            this.WriteCodePair(100, SubclassMarker.XLine);

            this.WriteCodePair(10, xline.Origin.X);
            this.WriteCodePair(20, xline.Origin.Y);
            this.WriteCodePair(30, xline.Origin.Z);

            this.WriteCodePair(11, xline.Direction.X);
            this.WriteCodePair(21, xline.Direction.Y);
            this.WriteCodePair(31, xline.Direction.Z);

            this.WriteXData(xline.XData);
        }

        private void WriteLightWeightPolyline(LwPolyline polyline)
        {            
            this.WriteCodePair(100, SubclassMarker.LightWeightPolyline);
            this.WriteCodePair(90, polyline.Vertexes.Count);
            this.WriteCodePair(70, (int) polyline.Flags);

            this.WriteCodePair(38, polyline.Elevation);
            this.WriteCodePair(39, polyline.Thickness);


            foreach (LwPolylineVertex v in polyline.Vertexes)
            {
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(40, v.BeginWidth);
                this.WriteCodePair(41, v.EndWidth);
                this.WriteCodePair(42, v.Bulge);
            }

            this.WriteCodePair(210, polyline.Normal.X);
            this.WriteCodePair(220, polyline.Normal.Y);
            this.WriteCodePair(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);
        }

        private void WritePolyline3d(Polyline polyline)
        {
            this.WriteCodePair(100, SubclassMarker.Polyline3d);

            //dummy point
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);
            this.WriteCodePair(30, 0.0);

            this.WriteCodePair(70, (int)polyline.Flags);
            this.WriteCodePair(75, (int)polyline.SmoothType);

            this.WriteCodePair(210, polyline.Normal.X);
            this.WriteCodePair(220, polyline.Normal.Y);
            this.WriteCodePair(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);

            foreach (PolylineVertex v in polyline.Vertexes)
            {
                this.WriteCodePair(0, v.CodeName);
                this.WriteCodePair(5, v.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, polyline.Layer); // the vertex layer should be the same as the polyline layer
                this.WriteCodePair(62, polyline.Color.Index); // the vertex color should be the same as the polyline color
                if (polyline.Color.UseTrueColor)
                    this.WriteCodePair(420, AciColor.ToTrueColor(polyline.Color));
                this.WriteCodePair(100, SubclassMarker.Vertex);
                this.WriteCodePair(100, SubclassMarker.Polyline3dVertex);             
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);
                this.WriteCodePair(70, (int) v.Flags);
            }

            this.WriteCodePair(0, polyline.EndSequence.CodeName);
            this.WriteCodePair(5, polyline.EndSequence.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, polyline.EndSequence.Layer);
        }

        private void WritePolyfaceMesh(PolyfaceMesh mesh)
        {
            this.WriteCodePair(100, SubclassMarker.PolyfaceMesh);
            this.WriteCodePair(70, (int) mesh.Flags);
            this.WriteCodePair(71, mesh.Vertexes.Count);
            this.WriteCodePair(72, mesh.Faces.Count);

            //dummy point
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);
            this.WriteCodePair(30, 0.0);

            this.WriteCodePair(210, mesh.Normal.X);
            this.WriteCodePair(220, mesh.Normal.Y);
            this.WriteCodePair(230, mesh.Normal.Z);

            if (mesh.XData != null)
                this.WriteXData(mesh.XData);

            foreach (PolyfaceMeshVertex v in mesh.Vertexes)
            {
                this.WriteCodePair(0, v.CodeName);
                this.WriteCodePair(5, v.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, mesh.Layer); // the polyfacemesh vertex layer should be the same as the polyfacemesh layer
                this.WriteCodePair(62, mesh.Color.Index); // the polyfacemesh vertex color should be the same as the polyfacemesh color
                if (mesh.Color.UseTrueColor)
                    this.WriteCodePair(420, AciColor.ToTrueColor(mesh.Color));
                this.WriteCodePair(100, SubclassMarker.Vertex);
                this.WriteCodePair(100, SubclassMarker.PolyfaceMeshVertex);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);
            }

            foreach (PolyfaceMeshFace face in mesh.Faces)
            {
                this.WriteCodePair(0, face.CodeName);
                this.WriteCodePair(5, face.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, mesh.Layer); // the polyfacemesh face layer should be the same as the polyfacemesh layer
                this.WriteCodePair(62, mesh.Color.Index); // the polyfacemesh face color should be the same as the polyfacemesh color
                if (mesh.Color.UseTrueColor)
                    this.WriteCodePair(420, AciColor.ToTrueColor(mesh.Color));
                this.WriteCodePair(100, SubclassMarker.PolyfaceMeshFace);
                this.WriteCodePair(70, (int) VertexTypeFlags.PolyfaceMeshVertex);
                this.WriteCodePair(10, 0);
                this.WriteCodePair(20, 0);
                this.WriteCodePair(30, 0);

                this.WriteCodePair(71, face.VertexIndexes[0]);
                if (face.VertexIndexes.Length > 1) this.WriteCodePair(72, face.VertexIndexes[1]);
                if (face.VertexIndexes.Length > 2) this.WriteCodePair(73, face.VertexIndexes[2]);
                if (face.VertexIndexes.Length > 3) this.WriteCodePair(74, face.VertexIndexes[3]);
            }

            this.WriteCodePair(0, mesh.EndSequence.CodeName);
            this.WriteCodePair(5, mesh.EndSequence.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, mesh.EndSequence.Layer);
        }

        private void WritePoint(Point point)
        {
            this.WriteCodePair(100, SubclassMarker.Point);

            this.WriteCodePair(10, point.Location.X);
            this.WriteCodePair(20, point.Location.Y);
            this.WriteCodePair(30, point.Location.Z);

            this.WriteCodePair(39, point.Thickness);

            this.WriteCodePair(210, point.Normal.X);
            this.WriteCodePair(220, point.Normal.Y);
            this.WriteCodePair(230, point.Normal.Z);

            // for unknown reasons the dxf likes the point rotation inverted
            this.WriteCodePair(50, 360.0 - point.Rotation);

            this.WriteXData(point.XData);
        }

        private void WriteText(Text text)
        {
            this.WriteCodePair(100, SubclassMarker.Text);

            this.WriteCodePair(1, text.Value);

            // another example of this ocs vs wcs non sense.
            // while the MText position is written in WCS the position of the Text is written in OCS (different rules for the same concept).
            Vector3 ocsBasePoint = MathHelper.Transform(text.Position, text.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            this.WriteCodePair(10, ocsBasePoint.X);
            this.WriteCodePair(20, ocsBasePoint.Y);
            this.WriteCodePair(30, ocsBasePoint.Z);

            this.WriteCodePair(40, text.Height);

            this.WriteCodePair(41, text.WidthFactor);

            this.WriteCodePair(50, text.Rotation);

            this.WriteCodePair(51, text.ObliqueAngle);

            this.WriteCodePair(7, text.Style);

            this.WriteCodePair(11, ocsBasePoint.X);
            this.WriteCodePair(21, ocsBasePoint.Y);
            this.WriteCodePair(31, ocsBasePoint.Z);

            this.WriteCodePair(210, text.Normal.X);
            this.WriteCodePair(220, text.Normal.Y);
            this.WriteCodePair(230, text.Normal.Z);

            switch (text.Alignment)
            {
                case TextAlignment.TopLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.TopCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.TopRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.MiddleLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.MiddleCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.MiddleRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.BottomLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 1);
                    break;
                case TextAlignment.BottomCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 1);
                    break;

                case TextAlignment.BottomRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 1);
                    break;

                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.BaselineRight:
                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.Aligned:
                    this.WriteCodePair(72, 3);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.Middle:
                    this.WriteCodePair(72, 4);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.Fit:
                    this.WriteCodePair(72, 5);
                    this.WriteCodePair(100, SubclassMarker.Text);
                    this.WriteCodePair(73, 0);
                    break;
            }

            this.WriteXData(text.XData);
        }

        private void WriteMText(MText mText)
        {
            this.WriteCodePair(100, SubclassMarker.MText);

            this.WriteCodePair(10, mText.Position.X);
            this.WriteCodePair(20, mText.Position.Y);
            this.WriteCodePair(30, mText.Position.Z);

            this.WriteCodePair(210, mText.Normal.X);
            this.WriteCodePair(220, mText.Normal.Y);
            this.WriteCodePair(230, mText.Normal.Z);

            WriteMTextChunks(mText.Value);

            this.WriteCodePair(40, mText.Height);
            this.WriteCodePair(41, mText.RectangleWidth);
            this.WriteCodePair(44, mText.LineSpacingFactor);

            // even if the AutoCAD dxf documentation says that the rotation is in radians, this is wrong this value must be saved in degrees
            this.WriteCodePair(50, mText.Rotation);

            this.WriteCodePair(71, (int)mText.AttachmentPoint);

            // By style (the flow direction is inherited from the associated text style)
            this.WriteCodePair(72, 5);

            this.WriteCodePair(7, mText.Style);

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
                string chunk = text.Substring(0, 250);
                this.WriteCodePair(3, chunk);
                text = text.Remove(0, 250);
            }
            this.WriteCodePair(1, text);
        }

        private void WriteHatch(Hatch hatch)
        {
            this.WriteCodePair(100, SubclassMarker.Hatch);

            this.WriteCodePair(10, 0.0f);
            this.WriteCodePair(20, 0.0f);
            this.WriteCodePair(30, hatch.Elevation);

            this.WriteCodePair(210, hatch.Normal.X);
            this.WriteCodePair(220, hatch.Normal.Y);
            this.WriteCodePair(230, hatch.Normal.Z);

            this.WriteCodePair(2, hatch.Pattern.Name);

            this.WriteCodePair(70, (int)hatch.Pattern.Fill);

            this.WriteCodePair(71, 0);

            // boundary paths info
            WriteHatchBoundaryPaths(hatch.BoundaryPaths);

            // pattern info
            WriteHatchPattern(hatch.Pattern);

            // add or modifies xData information for GradientColor1ACI and GradientColor2ACI
            if (hatch.Pattern.GetType() == typeof (HatchGradientPattern))
                ((HatchGradientPattern) hatch.Pattern).GradientColorAciXData(hatch.XData);

            this.WriteXData(hatch.XData);   
        }

        private void WriteHatchBoundaryPaths(List<HatchBoundaryPath> boundaryPaths)
        {
            this.WriteCodePair(91, boundaryPaths.Count);

            // each hatch boundary paths are made of multiple closed loops
            foreach (HatchBoundaryPath path in boundaryPaths)
            {
                this.WriteCodePair(92, (int)path.PathTypeFlag);
                if ((path.PathTypeFlag & BoundaryPathTypeFlag.Polyline) != BoundaryPathTypeFlag.Polyline) this.WriteCodePair(93, path.NumberOfEdges);
                foreach (EntityObject entity in path.Data)
                {
                    WriteHatchBoundaryPathData(entity);
                }
                this.WriteCodePair(97, 0); // associative hatches not supported
            }
        }

        private void WriteHatchBoundaryPathData(EntityObject entity)
        {    
                switch (entity.Type)
                {
                    case EntityType.Arc:
                        this.WriteCodePair(72, 2);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                        Arc arc = (Arc)entity;
                        this.WriteCodePair(10, arc.Center.X);
                        this.WriteCodePair(20, arc.Center.Y);
                        this.WriteCodePair(40, arc.Radius);
                        this.WriteCodePair(50, arc.StartAngle);
                        this.WriteCodePair(51, arc.EndAngle);
                        this.WriteCodePair(73, Math.Sign(arc.EndAngle - arc.StartAngle) >= 0 ? 1 : 0); // Is counterclockwise flag
                        break;
                    case EntityType.Circle:
                        this.WriteCodePair(72, 2);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                        Circle circle = (Circle)entity;
                        this.WriteCodePair(10, circle.Center.X);
                        this.WriteCodePair(20, circle.Center.Y);
                        this.WriteCodePair(40, circle.Radius);
                        this.WriteCodePair(50, 0);
                        this.WriteCodePair(51, 360);
                        this.WriteCodePair(73, 1);   // Is counterclockwise flag   
                        break;

                    case EntityType.Ellipse:

                        this.WriteCodePair(72, 3);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                        Ellipse ellipse = (Ellipse)entity;

                        this.WriteCodePair(10, ellipse.Center.X);
                        this.WriteCodePair(20, ellipse.Center.Y);
                        double sine = 0.5 * ellipse.MajorAxis * Math.Sin(ellipse.Rotation * MathHelper.DegToRad);
                        double cosine = 0.5 * ellipse.MajorAxis * Math.Cos(ellipse.Rotation * MathHelper.DegToRad);
                        this.WriteCodePair(11, cosine);
                        this.WriteCodePair(21, sine);

                        this.WriteCodePair(40, ellipse.MinorAxis / ellipse.MajorAxis);
                        this.WriteCodePair(50, ellipse.StartAngle);
                        this.WriteCodePair(51, ellipse.EndAngle);

                        this.WriteCodePair(73, Math.Sign(ellipse.EndAngle - ellipse.StartAngle) >= 0 ? 1 : 0); // Is counterclockwise flag
                        break;

                    case EntityType.Line:
                        this.WriteCodePair(72, 1);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline

                        Line line = (Line)entity;
                        this.WriteCodePair(10, line.StartPoint.X);
                        this.WriteCodePair(20, line.StartPoint.Y);
                        this.WriteCodePair(11, line.EndPoint.X);
                        this.WriteCodePair(21, line.EndPoint.Y);

                        break;

                    case EntityType.LightWeightPolyline:
                        LwPolyline polyline = (LwPolyline)entity;
                        if (polyline.IsClosed)
                        {
                            this.WriteCodePair(72, 1);  // Has bulge flag
                            this.WriteCodePair(73, polyline.IsClosed ? 1 : 0);
                            this.WriteCodePair(93, polyline.Vertexes.Count);

                            foreach (LwPolylineVertex vertex in polyline.Vertexes)
                            {
                                this.WriteCodePair(10, vertex.Location.X);
                                this.WriteCodePair(20, vertex.Location.Y);
                                this.WriteCodePair(42, vertex.Bulge);
                            }
                        }
                        else
                        {
                            // open polylines will always exported as its internal entities lines and arcs when combined with other entities to make a closed loop.
                            // AutoCAD seems to like them exploded.
                            List<EntityObject> exploded = polyline.Explode();
                            foreach (EntityObject o in exploded)
                            {
                                WriteHatchBoundaryPathData(o);
                            }
                        }
                        break;
                    case EntityType.Spline:
                        Spline spline = (Spline)entity;
                        this.WriteCodePair(72, 4);  // Edge type (only if boundary is not a polyline): 1 = Line; 2 = Circular arc; 3 = Elliptic arc; 4 = Spline
                        this.WriteCodePair(94, spline.Degree);
                        this.WriteCodePair(73, (spline.Flags & SplineTypeFlags.Rational) == SplineTypeFlags.Rational ? 1 : 0);
                        this.WriteCodePair(74, spline.IsPeriodic ? 1 : 0);
                        this.WriteCodePair(95, spline.Knots.Length);
                        this.WriteCodePair(96, spline.ControlPoints.Count);
                        foreach (double knot in spline.Knots)
                            this.WriteCodePair(40, knot);

                        foreach (SplineVertex point in spline.ControlPoints)
                        {
                            this.WriteCodePair(10, point.Location.X);
                            this.WriteCodePair(20, point.Location.Y);
                            this.WriteCodePair(42, point.Weigth);
                        }

                        // this information is only required for AutoCAD version 2010
                        // stores information about spline fit points (the spline entity has no fit points and no tangent info)
                        if (this.version >= DxfVersion.AutoCad2010) this.WriteCodePair(97, 0);
                        
                        break;
                    default:
                        throw new NotSupportedException("Hatch boundary path not supported: " + entity.Type);
                }
        }

        private void WriteHatchPattern(HatchPattern pattern)
        {
            this.WriteCodePair(75, (int)pattern.Style); 
            this.WriteCodePair(76, (int)pattern.Type);

            if (pattern.Fill == FillType.PatternFill)
            {
                this.WriteCodePair(52, pattern.Angle);
                this.WriteCodePair(41, pattern.Scale);
                this.WriteCodePair(77, 0);  // Hatch pattern double flag
                this.WriteCodePair(78, pattern.LineDefinitions.Count);  // Number of pattern definition lines  
                WriteHatchPatternDefinitonLines(pattern);
            }

            // I don't know what is the purpose of these codes, it seems that it doesn't change anything but they are needed
            this.WriteCodePair(47, 0.0);
            this.WriteCodePair(98, 1);
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);

            if (pattern.GetType() == typeof (HatchGradientPattern) && this.version > DxfVersion.AutoCad2000)
                WriteGradientHatchPattern((HatchGradientPattern) pattern);
        }

        private void WriteGradientHatchPattern(HatchGradientPattern pattern)
        {
            // again the order of codes shown in the documentation will not work
            this.WriteCodePair(450, 1);
            this.WriteCodePair(451, 0);
            this.WriteCodePair(460, pattern.Angle * MathHelper.DegToRad);
            this.WriteCodePair(461, pattern.Centered ? 0.0 : 1.0);
            this.WriteCodePair(452, pattern.SingleColor ? 1 : 0);
            this.WriteCodePair(462, pattern.Tint);
            this.WriteCodePair(453, 2);
            this.WriteCodePair(463, 0.0);
            this.WriteCodePair(63, pattern.Color1.Index);
            this.WriteCodePair(421, AciColor.ToTrueColor(pattern.Color1));
            this.WriteCodePair(463, 1.0);
            this.WriteCodePair(63, pattern.Color2.Index);
            this.WriteCodePair(421, AciColor.ToTrueColor(pattern.Color2));
            this.WriteCodePair(470, StringEnum.GetStringValue(pattern.GradientType));

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
                this.WriteCodePair(53, angle);

                double sinOrigin = Math.Sin(pattern.Angle * MathHelper.DegToRad);
                double cosOrigin = Math.Cos(pattern.Angle * MathHelper.DegToRad);       
                Vector2 origin = new Vector2(cosOrigin * line.Origin.X * scale - sinOrigin * line.Origin.Y * scale, sinOrigin * line.Origin.X * scale + cosOrigin * line.Origin.Y * scale);
                this.WriteCodePair(43, origin.X);
                this.WriteCodePair(44, origin.Y);

                double sinDelta = Math.Sin(angle * MathHelper.DegToRad);
                double cosDelta = Math.Cos(angle * MathHelper.DegToRad);       
                Vector2 delta = new Vector2(cosDelta * line.Delta.X * scale - sinDelta * line.Delta.Y * scale, sinDelta * line.Delta.X * scale + cosDelta * line.Delta.Y * scale);
                this.WriteCodePair(45, delta.X);
                this.WriteCodePair(46, delta.Y);

                this.WriteCodePair(79, line.DashPattern.Count);
                foreach (double dash in line.DashPattern)
                {
                    this.WriteCodePair(49, dash * scale);
                }
            }
        }

        private void WriteDimension(Dimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.Dimension);

            this.WriteCodePair(2, dim.Block.Name);
            this.WriteCodePair(10, dim.DefinitionPoint.X);
            this.WriteCodePair(20, dim.DefinitionPoint.Y);
            this.WriteCodePair(30, dim.DefinitionPoint.Z);
            this.WriteCodePair(11, dim.MidTextPoint.X);
            this.WriteCodePair(21, dim.MidTextPoint.Y);
            this.WriteCodePair(31, dim.MidTextPoint.Z);

            int flags = (int) dim.DimensionType + (int) DimensionTypeFlag.BlockReference;
            if(dim.DimensionType==DimensionType.Ordinate)
            {
                // even if the documentation says that code 51 is optional, rotated ordinate dimensions will not work correctly is this value is not provided
                this.WriteCodePair(51, 360-((OrdinateDimension)dim).Rotation);
                if (((OrdinateDimension)dim).Axis==OrdinateDimensionAxis.X)
                    flags += (int)DimensionTypeFlag.OrdinteType;
            }
               
            this.WriteCodePair(70, flags);
            this.WriteCodePair(71, (int)dim.AttachmentPoint);
            this.WriteCodePair(72, (int)dim.LineSpacingStyle);
            this.WriteCodePair(41, dim.LineSpacingFactor);
            this.WriteCodePair(210, dim.Normal.X);
            this.WriteCodePair(220, dim.Normal.Y);
            this.WriteCodePair(230, dim.Normal.Z);

            this.WriteCodePair(3, dim.Style.Name);

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
            this.WriteCodePair(100, SubclassMarker.AlignedDimension);

            this.WriteCodePair(13, dim.FirstReferencePoint.X);
            this.WriteCodePair(23, dim.FirstReferencePoint.Y);
            this.WriteCodePair(33, dim.FirstReferencePoint.Z);

            this.WriteCodePair(14, dim.SecondReferencePoint.X);
            this.WriteCodePair(24, dim.SecondReferencePoint.Y);
            this.WriteCodePair(34, dim.SecondReferencePoint.Z);

            this.WriteXData(dim.XData);
        }

        private void WriteLinearDimension(LinearDimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.AlignedDimension);

            this.WriteCodePair(13, dim.FirstReferencePoint.X);
            this.WriteCodePair(23, dim.FirstReferencePoint.Y);
            this.WriteCodePair(33, dim.FirstReferencePoint.Z);

            this.WriteCodePair(14, dim.SecondReferencePoint.X);
            this.WriteCodePair(24, dim.SecondReferencePoint.Y);
            this.WriteCodePair(34, dim.SecondReferencePoint.Z);

            this.WriteCodePair(50, dim.Rotation);

            // AutoCAD is unable to recognized code 52 for oblique dimension line even though it appears as valid in the dxf documentation
            // this.WriteCodePair(52, dim.ObliqueAngle);

            this.WriteCodePair(100, SubclassMarker.LinearDimension);
            
            this.WriteXData(dim.XData);
        }

        private void WriteRadialDimension(RadialDimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.RadialDimension);

            this.WriteCodePair(15, dim.CircunferencePoint.X);
            this.WriteCodePair(25, dim.CircunferencePoint.Y);
            this.WriteCodePair(35, dim.CircunferencePoint.Z);

            this.WriteCodePair(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteDiametricDimension(DiametricDimension dim)
        {

            this.WriteCodePair(100, SubclassMarker.DiametricDimension);

            this.WriteCodePair(15, dim.CircunferencePoint.X);
            this.WriteCodePair(25, dim.CircunferencePoint.Y);
            this.WriteCodePair(35, dim.CircunferencePoint.Z);

            this.WriteCodePair(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular3PointDimension(Angular3PointDimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.Angular3PointDimension);

            this.WriteCodePair(13, dim.FirstPoint.X);
            this.WriteCodePair(23, dim.FirstPoint.Y);
            this.WriteCodePair(33, dim.FirstPoint.Z);

            this.WriteCodePair(14, dim.SecondPoint.X);
            this.WriteCodePair(24, dim.SecondPoint.Y);
            this.WriteCodePair(34, dim.SecondPoint.Z);

            this.WriteCodePair(15, dim.CenterPoint.X);
            this.WriteCodePair(25, dim.CenterPoint.Y);
            this.WriteCodePair(35, dim.CenterPoint.Z);

            this.WriteCodePair(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteAngular2LineDimension(Angular2LineDimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.Angular2LineDimension);

            this.WriteCodePair(13, dim.StartFirstLine.X);
            this.WriteCodePair(23, dim.StartFirstLine.Y);
            this.WriteCodePair(33, dim.StartFirstLine.Z);

            this.WriteCodePair(14, dim.EndFirstLine.X);
            this.WriteCodePair(24, dim.EndFirstLine.Y);
            this.WriteCodePair(34, dim.EndFirstLine.Z);

            this.WriteCodePair(15, dim.StartSecondLine.X);
            this.WriteCodePair(25, dim.StartSecondLine.Y);
            this.WriteCodePair(35, dim.StartSecondLine.Z);

            this.WriteCodePair(16, dim.ArcDefinitionPoint.X);
            this.WriteCodePair(26, dim.ArcDefinitionPoint.Y);
            this.WriteCodePair(36, dim.ArcDefinitionPoint.Z);

            this.WriteCodePair(40, 0.0);

            this.WriteXData(dim.XData);
        }

        private void WriteOrdinateDimension(OrdinateDimension dim)
        {
            this.WriteCodePair(100, SubclassMarker.OrdinateDimension);

            this.WriteCodePair(13, dim.FirstPoint.X);
            this.WriteCodePair(23, dim.FirstPoint.Y);
            this.WriteCodePair(33, dim.FirstPoint.Z);

            this.WriteCodePair(14, dim.SecondPoint.X);
            this.WriteCodePair(24, dim.SecondPoint.Y);
            this.WriteCodePair(34, dim.SecondPoint.Z);

            this.WriteXData(dim.XData);
        }

        private void WriteImage(Image image)
        {
            this.WriteCodePair(100, SubclassMarker.RasterImage);

            this.WriteCodePair(10, image.Position.X);
            this.WriteCodePair(20, image.Position.Y);
            this.WriteCodePair(30, image.Position.Z);

            Vector2 u = MathHelper.Transform(new Vector2(image.Width / image.Definition.Width, 0.0), image.Rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            Vector3 uWcs = MathHelper.Transform(new Vector3(u.X, u.Y, 0.0), image.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            this.WriteCodePair(11, uWcs.X);
            this.WriteCodePair(21, uWcs.Y);
            this.WriteCodePair(31, uWcs.Z);

            Vector2 v = MathHelper.Transform(new Vector2(0.0, image.Height / image.Definition.Height), image.Rotation * MathHelper.DegToRad, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            Vector3 vWcs = MathHelper.Transform(new Vector3(v.X, v.Y, 0.0), image.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            this.WriteCodePair(12, vWcs.X);
            this.WriteCodePair(22, vWcs.Y);
            this.WriteCodePair(32, vWcs.Z);

            this.WriteCodePair(13, image.Definition.Width);
            this.WriteCodePair(23, image.Definition.Height);

            this.WriteCodePair(340, image.Definition.Handle);

            this.WriteCodePair(70, (int)image.DisplayOptions);
            this.WriteCodePair(280, image.Clipping ? 1 : 0);
            this.WriteCodePair(281, image.Brightness);
            this.WriteCodePair(282, image.Contrast);
            this.WriteCodePair(283, image.Fade);
            this.WriteCodePair(360, image.Definition.Reactors[image.Handle].Handle);
            this.WriteCodePair(71, (int)image.ClippingBoundary.Type);
            this.WriteCodePair(91, image.ClippingBoundary.Vertexes.Count);
            foreach (Vector2 vertex in image.ClippingBoundary.Vertexes)
            {
                this.WriteCodePair(14, vertex.X);
                this.WriteCodePair(24, vertex.Y);
            }

            this.WriteXData(image.XData);

        }

        private void WriteMLine(MLine mLine)
        {
            this.WriteCodePair(100, SubclassMarker.MLine);

            this.WriteCodePair(2, mLine.Style.Name);
            this.WriteCodePair(340, mLine.Style.Handle);

            this.WriteCodePair(40, mLine.Scale);
            this.WriteCodePair(70, (int)mLine.Justification);
            this.WriteCodePair(71, (int)mLine.Flags);
            this.WriteCodePair(72, mLine.Vertexes.Count);
            this.WriteCodePair(73, mLine.Style.Elements.Count);

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
                this.WriteCodePair(10, 0.0);
                this.WriteCodePair(20, 0.0);
                this.WriteCodePair(30, 0.0);
            }
            else
            {
                this.WriteCodePair(10, wcsVertexes[0].X);
                this.WriteCodePair(20, wcsVertexes[0].Y);
                this.WriteCodePair(30, wcsVertexes[0].Z);
            }

            this.WriteCodePair(210, mLine.Normal.X);
            this.WriteCodePair(220, mLine.Normal.Y);
            this.WriteCodePair(230, mLine.Normal.Z);

            for (int i = 0; i < wcsVertexes.Count; i++)
            {
                this.WriteCodePair(11, wcsVertexes[i].X);
                this.WriteCodePair(21, wcsVertexes[i].Y);
                this.WriteCodePair(31, wcsVertexes[i].Z);

                // the directions are written in world coordinates
                Vector2 dir = mLine.Vertexes[i].Direction;
                Vector3 wcsDir = MathHelper.Transform(new Vector3(dir.X, dir.Y, mLine.Elevation), mLine.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
                this.WriteCodePair(12, wcsDir.X);
                this.WriteCodePair(22, wcsDir.Y);
                this.WriteCodePair(32, wcsDir.Z);
                Vector2 mitter = mLine.Vertexes[i].Miter;
                Vector3 wcsMitter = MathHelper.Transform(new Vector3(mitter.X, mitter.Y, mLine.Elevation), mLine.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
                this.WriteCodePair(13, wcsMitter.X);
                this.WriteCodePair(23, wcsMitter.Y);
                this.WriteCodePair(33, wcsMitter.Z);

                foreach (List<double> distances in mLine.Vertexes[i].Distances)
                {
                    this.WriteCodePair(74, distances.Count);
                    foreach (double distance in distances)
                    {
                        this.WriteCodePair(41, distance);
                    }
                    this.WriteCodePair(75, 0);
                }
            }

            this.WriteXData(mLine.XData);
        }

        #endregion

        #region methods for Object section

        public void WriteDictionary(DictionaryObject dictionary)
        {
            this.WriteCodePair(0, StringCode.Dictionary);
            this.WriteCodePair(5, dictionary.Handle);
            this.WriteCodePair(330, 0);
            this.WriteCodePair(100, SubclassMarker.Dictionary);
            this.WriteCodePair(280, dictionary.IsHardOwner ? 1 : 0);
            this.WriteCodePair(281, (int)dictionary.Clonning);

            foreach (KeyValuePair<string, string> entry in dictionary.Entries)
            {
                this.WriteCodePair(3, entry.Value);
                this.WriteCodePair(350, entry.Key);
            }
        }

        public void WriteImageDefReactor(ImageDefReactor reactor)
        {
            this.WriteCodePair(0, reactor.CodeName);
            this.WriteCodePair(5, reactor.Handle);

            this.WriteCodePair(100, SubclassMarker.RasterImageDefReactor);
            this.WriteCodePair(90, 2);
            this.WriteCodePair(330, reactor.ImageHandle);
        }

        public void WriteImageDef(ImageDef imageDef, string ownerHandle)
        {
            this.WriteCodePair(0, imageDef.CodeName);
            this.WriteCodePair(5, imageDef.Handle);

            this.WriteCodePair(102, "{ACAD_REACTORS");
            this.WriteCodePair(330, ownerHandle);
            foreach (ImageDefReactor reactor in imageDef.Reactors.Values)
            {
                this.WriteCodePair(330, reactor.Handle);
            }
            this.WriteCodePair(102, "}");

            this.WriteCodePair(330, ownerHandle);

            this.WriteCodePair(100, SubclassMarker.RasterImageDef);
            this.WriteCodePair(1, imageDef.FileName);

            this.WriteCodePair(10, imageDef.Width);
            this.WriteCodePair(20, imageDef.Height);

            this.WriteCodePair(11, imageDef.OnePixelSize.X);
            this.WriteCodePair(21, imageDef.OnePixelSize.Y);

            this.WriteCodePair(280, 1);
            this.WriteCodePair(281, (int)imageDef.ResolutionUnits);

        }

        public void WriteRasterVariables(RasterVariables variables, string ownerHandle)
        {
            this.WriteCodePair(0, variables.CodeName);
            this.WriteCodePair(5, variables.Handle);
            this.WriteCodePair(330, ownerHandle);

            this.WriteCodePair(100, SubclassMarker.RasterVariables);
            this.WriteCodePair(90, 0);
            this.WriteCodePair(70, variables.DisplayFrame ? 1 : 0);
            this.WriteCodePair(71, (int)variables.DisplayQuality);
            this.WriteCodePair(72, (int)variables.Units);
        }

        public void WriteMLineStyle(MLineStyle style, string ownerHandle)
        {
            this.WriteCodePair(0, style.CodeName);
            this.WriteCodePair(5, style.Handle);
            this.WriteCodePair(330, ownerHandle);

            this.WriteCodePair(100, SubclassMarker.MLineStyle);

            this.WriteCodePair(2, style.Name);
            this.WriteCodePair(70, (int)style.Flags);
            this.WriteCodePair(3, style.Description);
            this.WriteCodePair(62, style.FillColor.Index);
            if (style.FillColor.UseTrueColor && this.version > DxfVersion.AutoCad2000)
                this.WriteCodePair(420, AciColor.ToTrueColor(style.FillColor));
            this.WriteCodePair(51, style.StartAngle);
            this.WriteCodePair(52, style.EndAngle);
            this.WriteCodePair(71, style.Elements.Count);
            foreach (MLineStyleElement element in style.Elements)
            {
                this.WriteCodePair(49, element.Offset);
                this.WriteCodePair(62, element.Color.Index);
                if (element.Color.UseTrueColor && this.version > DxfVersion.AutoCad2000)
                    this.WriteCodePair(420, AciColor.ToTrueColor(element.Color));
                this.WriteCodePair(6, element.LineType.Name);
            }
        }

        public void WriteGroup(Group group, string ownerHandle)
        {
            this.WriteCodePair(0, group.CodeName);
            this.WriteCodePair(5, group.Handle);
            this.WriteCodePair(330, ownerHandle);

            this.WriteCodePair(100, SubclassMarker.Group);

            this.WriteCodePair(300, group.Description);
            this.WriteCodePair(70, group.IsUnnamed ? 1 : 0);
            this.WriteCodePair(71, group.IsSelectable ? 1 : 0);

            foreach (EntityObject entity in group.Entities)
            {
                this.WriteCodePair(340, entity.Handle);
            }
        }

        #endregion

        #region private methods

        private void WriteAttributeDefinition(AttributeDefinition def)
        {
            this.WriteEntityCommonCodes(def);

            this.WriteCodePair(100, SubclassMarker.Text);

            this.WriteCodePair(10, def.Position.X);
            this.WriteCodePair(20, def.Position.Y);
            this.WriteCodePair(30, def.Position.Z);
            this.WriteCodePair(40, def.Height);

            this.WriteCodePair(1, def.Value);

            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.TopCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.TopRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.MiddleLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.MiddleCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.MiddleRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.BottomCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.BottomRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.BaselineRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.Aligned:
                    this.WriteCodePair(72, 3);
                    break;
                case TextAlignment.Middle:
                    this.WriteCodePair(72, 4);
                    break;
                case TextAlignment.Fit:
                    this.WriteCodePair(72, 5);
                    break;
            }

            this.WriteCodePair(50, def.Rotation);
            this.WriteCodePair(41, def.WidthFactor);
            this.WriteCodePair(7, def.Style);

            this.WriteCodePair(11, def.Position.X);
            this.WriteCodePair(21, def.Position.Y);
            this.WriteCodePair(31, def.Position.Z);

            this.WriteCodePair(210, def.Normal.X);
            this.WriteCodePair(220, def.Normal.Y);
            this.WriteCodePair(230, def.Normal.Z);

            this.WriteCodePair(100, SubclassMarker.AttributeDefinition);

            this.WriteCodePair(3, def.Text);
            this.WriteCodePair(2, def.Id);
            this.WriteCodePair(70, (int)def.Flags);

            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopCenter:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopRight:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleRight:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomCenter:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomRight:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineRight:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Aligned:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Middle:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Fit:
                    this.WriteCodePair(74, 0);
                    break;
            }
        }

        private void WriteAttribute(Attribute attrib)
        {
            this.WriteEntityCommonCodes(attrib);

            this.WriteCodePair(100, SubclassMarker.Text);

            this.WriteCodePair(10, attrib.Position.X);
            this.WriteCodePair(20, attrib.Position.Y);
            this.WriteCodePair(30, attrib.Position.Z);

            this.WriteCodePair(40, attrib.Height);
            this.WriteCodePair(41, attrib.WidthFactor);
            this.WriteCodePair(7, attrib.Style);
            this.WriteCodePair(1, attrib.Value);

            switch (attrib.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.TopCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.TopRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.MiddleLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.MiddleCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.MiddleRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.BottomCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.BottomRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(72, 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(72, 1);
                    break;
                case TextAlignment.BaselineRight:
                    this.WriteCodePair(72, 2);
                    break;
                case TextAlignment.Aligned:
                    this.WriteCodePair(72, 3);
                    break;
                case TextAlignment.Middle:
                    this.WriteCodePair(72, 4);
                    break;
                case TextAlignment.Fit:
                    this.WriteCodePair(72, 5);
                    break;
            }

            this.WriteCodePair(11, attrib.Position.X);
            this.WriteCodePair(21, attrib.Position.Y);
            this.WriteCodePair(31, attrib.Position.Z);

            this.WriteCodePair(50, attrib.Rotation);

            this.WriteCodePair(210, attrib.Normal.X);
            this.WriteCodePair(220, attrib.Normal.Y);
            this.WriteCodePair(230, attrib.Normal.Z);

            this.WriteCodePair(100, SubclassMarker.Attribute);

            this.WriteCodePair(2, attrib.Id);

            this.WriteCodePair(70, (int) attrib.Flags);

            switch (attrib.Alignment)
            {
                case TextAlignment.TopLeft:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopCenter:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopRight:
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.MiddleLeft:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleCenter:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleRight:
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.BottomLeft:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomCenter:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomRight:
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineRight:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Aligned:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Middle:
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.Fit:
                    this.WriteCodePair(74, 0);
                    break;
            }
        }

        private void WriteXData(Dictionary<string, XData> xData)
        {
            foreach (string appReg in xData.Keys)
            {
                this.WriteCodePair(XDataCode.AppReg, appReg);
                foreach (XDataRecord x in xData[appReg].XDataRecord)
                {
                    this.WriteCodePair(x.Code, x.Value);
                }
            }
        }

        private void WriteCodePair(int codigo, object valor)
        {
            string nameConversion = valor == null ? string.Empty : valor.ToString();
            this.writer.WriteLine(codigo);
            this.writer.WriteLine(nameConversion);
        }

        #endregion
    }
}