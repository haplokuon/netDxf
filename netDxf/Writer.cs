#region netDxf, Copyright(C) 2009 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2009 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
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

        private int reservedHandles = 10;
        private readonly string file;
        private bool isFileOpen;
        private string activeSection = StringCode.Unknown;
        private string activeTable = StringCode.Unknown;
        private bool isHeader;
        private bool isClassesSection;
        private bool isTableSection;
        private bool isBlockDefinition;
        private bool isBlockEntities;
        private bool isEntitiesSection;
        private bool isObjectsSection;
        private Stream output;
        private StreamWriter writer;
        private readonly DxfVersion version;

        #endregion

        #region constructors

        public DxfWriter(string file, DxfVersion version)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            this.file = file;
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
        /// Gets if the file is opent.
        /// </summary>
        public bool IsFileOpen
        {
            get { return this.isFileOpen; }
        }

        #endregion

        #region public methods

        public void Open()
        {
            if (this.isFileOpen)
            {
                throw new DxfException(this.file, "The file is already open");
            }
            try
            {
                this.output = File.Create(this.file);
                this.writer = new StreamWriter(this.output, Encoding.ASCII);
                this.isFileOpen = true;
            }
            catch (Exception ex)
            {
                throw (new DxfException(this.file, "Error when trying to create the dxf file", ex));
            }
        }

        /// <summary>
        /// Closes the dxf file.
        /// </summary>
        public void Close()
        {
            if (this.activeSection != StringCode.Unknown)
            {
                throw new OpenDxfSectionException(this.activeSection, this.file);
            }
            if (this.activeTable != StringCode.Unknown)
            {
                throw new OpenDxfTableException(this.activeTable, this.file);
            }
            this.WriteCodePair(0, StringCode.EndOfFile);

            if (this.isFileOpen)
            {
                this.writer.Close();
                this.output.Close();
            }

            this.isFileOpen = false;
        }

        /// <summary>
        /// Opens a new section.
        /// </summary>
        /// <param name="section">Section type to open.</param>
        /// <remarks>There can be only one type section.</remarks>
        public void BeginSection(string section)
        {
            if (! this.isFileOpen)
            {
                throw new DxfException(this.file, "The file is not open");
            }
            if (this.activeSection != StringCode.Unknown)
            {
                throw new OpenDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, StringCode.BeginSection);

            if (section == StringCode.HeaderSection)
            {
                if (this.isHeader)
                {
                    throw (new ClosedDxfSectionException(StringCode.HeaderSection, this.file));
                }
                this.WriteCodePair(2, StringCode.HeaderSection);
                this.isHeader = true;
            }
            if (section == StringCode.ClassesSection)
            {
                if (this.isClassesSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.ClassesSection, this.file));
                }
                this.WriteCodePair(2, StringCode.ClassesSection);
                this.isClassesSection = true;
            }
            if (section == StringCode.TablesSection)
            {
                if (this.isTableSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.TablesSection, this.file));
                }
                this.WriteCodePair(2, StringCode.TablesSection);
                this.isTableSection = true;
            }
            if (section == StringCode.BlocksSection)
            {
                if (this.isBlockDefinition)
                {
                    throw (new ClosedDxfSectionException(StringCode.BlocksSection, this.file));
                }
                this.WriteCodePair(2, StringCode.BlocksSection);
                this.isBlockDefinition = true;
            }
            if (section == StringCode.EntitiesSection)
            {
                if (this.isEntitiesSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.EntitiesSection, this.file));
                }
                this.WriteCodePair(2, StringCode.EntitiesSection);
                this.isEntitiesSection = true;
            }
            if (section == StringCode.ObjectsSection)
            {
                if (this.isObjectsSection)
                {
                    throw (new ClosedDxfSectionException(StringCode.ObjectsSection, this.file));
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
                throw new ClosedDxfSectionException(StringCode.Unknown, this.file);
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
        public void BeginTable(string table)
        {
            if (! this.isFileOpen)
            {
                throw new DxfException(this.file, "The file is not open");
            }
            if (this.activeTable != StringCode.Unknown)
            {
                throw new OpenDxfTableException(table, this.file);
            }
            this.WriteCodePair(0, StringCode.Table);
            this.WriteCodePair(2, table);
            this.WriteCodePair(5, this.reservedHandles++);
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
                throw new ClosedDxfTableException(StringCode.Unknown, this.file);
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
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }
            this.WriteCodePair(HeaderVariable.NAME_CODE_GROUP, variable.Name);
            this.WriteCodePair(variable.CodeGroup, variable.Value);
        }

        #endregion

        #region methods for Table section

        /// <summary>
        /// Writes a new extended data application registry to the table section.
        /// </summary>
        /// <param name="appReg">Nombre del registro de aplicación.</param>
        public void RegisterApplication(ApplicationRegistry appReg)
        {
            if (this.activeTable != StringCode.ApplicationIDTable)
            {
                throw new InvalidDxfTableException(StringCode.ApplicationIDTable, this.file);
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
        public void WriteViewPort(ViewPort vp)
        {
            if (this.activeTable != StringCode.ViewPortTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
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
        /// <param name="dimStyle">DimensionStyle.</param>
        public void WriteDimensionStyle(DimensionStyle dimStyle)
        {
            if (this.activeTable != StringCode.DimensionStyleTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
            }
            this.WriteCodePair(0, dimStyle.CodeName);
            this.WriteCodePair(105, dimStyle.Handle);

            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.DimensionStyle);

            this.WriteCodePair(2, dimStyle);

            // flags
            this.WriteCodePair(70, 0);
        }

        /// <summary>
        /// Writes a new block record to the table section.
        /// </summary>
        /// <param name="blockRecord">Block.</param>
        public void WriteBlockRecord(BlockRecord blockRecord)
        {
            if (this.activeTable != StringCode.BlockRecordTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
            }
            this.WriteCodePair(0, blockRecord.CodeName);
            this.WriteCodePair(5, blockRecord.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.BlockRecord);

            this.WriteCodePair(2, blockRecord);
        }

        /// <summary>
        /// Writes a new line type to the table section.
        /// </summary>
        /// <param name="tl">Line type.</param>
        public void WriteLineType(LineType tl)
        {
            if (this.version == DxfVersion.AutoCad12)
                if (tl.Name == "ByLayer" || tl.Name == "ByBlock")
                    return;

            if (this.activeTable != StringCode.LineTypeTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
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
            this.WriteCodePair(40, tl.Legth());
            foreach (double s in tl.Segments)
            {
                this.WriteCodePair(49, s);
                if (this.version != DxfVersion.AutoCad12)
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
                throw new InvalidDxfTableException(this.activeTable, this.file);
            }

            this.WriteCodePair(0, layer.CodeName);
            this.WriteCodePair(5, layer.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.Layer);
            this.WriteCodePair(70, 0);
            this.WriteCodePair(2, layer);

            //a negative color represents a hidden layer.
            if (layer.IsVisible)
            {
                this.WriteCodePair(62, layer.Color.Index);
            }
            else
            {
                this.WriteCodePair(62, -layer.Color.Index);
            }

            this.WriteCodePair(6, layer.LineType.Name);
            if (this.version != DxfVersion.AutoCad12)
                this.WriteCodePair(390, Layer.PlotStyleHandle);
        }

        /// <summary>
        /// Writes a new text style to the table section.
        /// </summary>
        /// <param name="style">TextStyle.</param>
        public void WriteTextStyle(TextStyle style)
        {
            if (this.activeTable != StringCode.TextStyleTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
            }

            this.WriteCodePair(0, style.CodeName);
            this.WriteCodePair(5, style.Handle);
            this.WriteCodePair(100, SubclassMarker.TableRecord);

            this.WriteCodePair(100, SubclassMarker.TextStyle);

            this.WriteCodePair(2, style);
            this.WriteCodePair(3, style.Font);

            if (style.IsVertical)
            {
                this.WriteCodePair(70, 4);
            }
            else
            {
                this.WriteCodePair(70, 0);
            }

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

        #endregion

        #region methods for Block section

        public void WriteBlock(Block block, List<IEntityObject> entityObjects)
        {
            if (this.version == DxfVersion.AutoCad12)
                if (block.Name == "*Model_Space" || block.Name == "*Paper_Space")
                    return;

            if (this.activeSection != StringCode.BlocksSection)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, block.CodeName);
            this.WriteCodePair(5, block.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, block.Layer);

            this.WriteCodePair(100, SubclassMarker.BlockBegin);

            this.WriteCodePair(2, block);

            //flags
            if (block.Attributes.Count == 0)
            {
                this.WriteCodePair(70, 0);
            }
            else
            {
                this.WriteCodePair(70, 2);
            }

            this.WriteCodePair(10, block.BasePoint.X);
            this.WriteCodePair(20, block.BasePoint.Y);
            this.WriteCodePair(30, block.BasePoint.Z);

            this.WriteCodePair(3, block);

            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.WriteAttributeDefinition(attdef);
            }

            //block entities, if version is AutoCad12 we will write the converted entities
            this.isBlockEntities = true;
            foreach (IEntityObject entity in entityObjects)
            {
                this.WriteEntity(entity);
            }
            this.isBlockEntities = false;

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

        public void WriteEntity(IEntityObject entity)
        {
            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.WriteArc((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.WriteCircle((Circle) entity);
                    break;
                case EntityType.Ellipse:
                    this.WriteEllipse((Ellipse) entity);
                    break;
                case EntityType.NurbsCurve:
                    this.WriteNurbsCurve((NurbsCurve) entity);
                    break;
                case EntityType.Point:
                    this.WritePoint((Point) entity);
                    break;
                case EntityType.Face3D:
                    this.WriteFace3D((Face3d) entity);
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
                    this.WriteLightWeightPolyline((LightWeightPolyline) entity);
                    break;
                case EntityType.Polyline:
                    this.WritePolyline2d((Polyline) entity);
                    break;
                case EntityType.Polyline3d:
                    this.WritePolyline3d((Polyline3d) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.WritePolyfaceMesh((PolyfaceMesh) entity);
                    break;
                case EntityType.Text:
                    this.WriteText((Text) entity);
                    break;
                default:
                    throw new NotImplementedException(entity.Type.ToString());
            }
        }

        private void WriteArc(Arc arc)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, arc.CodeName);
            this.WriteCodePair(5, arc.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(arc);
            this.WriteCodePair(100, SubclassMarker.Circle);

            this.WriteCodePair(39, arc.Thickness);

            this.WriteCodePair(10, arc.Center.X);
            this.WriteCodePair(20, arc.Center.Y);
            this.WriteCodePair(30, arc.Center.Z);

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
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, circle.CodeName);
            this.WriteCodePair(5, circle.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(circle);
            this.WriteCodePair(100, SubclassMarker.Circle);


            this.WriteCodePair(10, circle.Center.X);
            this.WriteCodePair(20, circle.Center.Y);
            this.WriteCodePair(30, circle.Center.Z);

            this.WriteCodePair(40, circle.Radius);

            this.WriteCodePair(39, circle.Thickness);

            this.WriteCodePair(210, circle.Normal.X);
            this.WriteCodePair(220, circle.Normal.Y);
            this.WriteCodePair(230, circle.Normal.Z);

            this.WriteXData(circle.XData);
        }

        private void WriteEllipse(Ellipse ellipse)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            if (this.version == DxfVersion.AutoCad12)
            {
                this.WriteEllipseAsPolyline(ellipse);
                return;
            }

            this.WriteCodePair(0, ellipse.CodeName);
            this.WriteCodePair(5, ellipse.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(ellipse);
            this.WriteCodePair(100, SubclassMarker.Ellipse);


            this.WriteCodePair(10, ellipse.Center.X);
            this.WriteCodePair(20, ellipse.Center.Y);
            this.WriteCodePair(30, ellipse.Center.Z);


            double sine = 0.5*ellipse.MajorAxis*Math.Sin(ellipse.Rotation*MathHelper.DegToRad);
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
            this.WriteCodePair(41, ellipse.StartAngle*MathHelper.DegToRad);
            this.WriteCodePair(42, ellipse.EndAngle*MathHelper.DegToRad);

            this.WriteXData(ellipse.XData);
        }

        private void WriteEllipseAsPolyline(Ellipse ellipse)
        {
            //we will draw the ellipse as a polyline, it is not supported in AutoCad12 dxf files
            this.WriteCodePair(0, DxfObjectCode.Polyline);

            this.WriteEntityCommonCodes(ellipse);

            //closed polyline
            this.WriteCodePair(70, 1);

            //dummy point
            this.WriteCodePair(10, 0.0f);
            this.WriteCodePair(20, 0.0f);
            this.WriteCodePair(30, ellipse.Center.Z);

            this.WriteCodePair(39, ellipse.Thickness);

            this.WriteCodePair(210, ellipse.Normal.X);
            this.WriteCodePair(220, ellipse.Normal.Y);
            this.WriteCodePair(230, ellipse.Normal.Z);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(ellipse.XData);

            List<Vector2> points = ellipse.PolygonalVertexes(ellipse.CurvePoints);
            foreach (Vector2 v in points)
            {
                this.WriteCodePair(0, DxfObjectCode.Vertex);
                this.WriteCodePair(8, ellipse.Layer);
                this.WriteCodePair(70, 0);
                this.WriteCodePair(10, v.X);
                this.WriteCodePair(20, v.Y);
            }
            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WriteNurbsCurve(NurbsCurve nurbsCurve)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }


            //we will draw the nurbsCurve as a polyline, it is not supported in AutoCad12 dxf files
            this.WriteCodePair(0, DxfObjectCode.Polyline);

            this.WriteEntityCommonCodes(nurbsCurve);

            //open polyline
            this.WriteCodePair(70, 0);

            //dummy point
            this.WriteCodePair(10, 0.0f);
            this.WriteCodePair(20, 0.0f);
            this.WriteCodePair(30, nurbsCurve.Elevation);

            this.WriteCodePair(39, nurbsCurve.Thickness);

            this.WriteCodePair(210, nurbsCurve.Normal.X);
            this.WriteCodePair(220, nurbsCurve.Normal.Y);
            this.WriteCodePair(230, nurbsCurve.Normal.Z);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(nurbsCurve.XData);

            List<Vector2> points = nurbsCurve.PolygonalVertexes(nurbsCurve.CurvePoints);
            foreach (Vector2 v in points)
            {
                this.WriteCodePair(0, DxfObjectCode.Vertex);
                this.WriteCodePair(8, nurbsCurve.Layer);
                this.WriteCodePair(70, 0);
                this.WriteCodePair(10, v.X);
                this.WriteCodePair(20, v.Y);
            }
            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WriteSolid(Solid solid)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, solid.CodeName);
            this.WriteCodePair(5, solid.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(solid);
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
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, face.CodeName);
            this.WriteCodePair(5, face.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(face);
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

        private void WriteInsert(Insert insert)
        {
            if (this.activeSection != StringCode.EntitiesSection && ! this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, insert.CodeName);
            this.WriteCodePair(5, insert.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(insert);
            this.WriteCodePair(100, SubclassMarker.Insert);

            this.WriteCodePair(2, insert.Block);

            this.WriteCodePair(10, insert.InsertionPoint.X);
            this.WriteCodePair(20, insert.InsertionPoint.Y);
            this.WriteCodePair(30, insert.InsertionPoint.Z);

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
                    this.WriteAttribute(attrib, insert.InsertionPoint);
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
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, line.CodeName);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(line);
            this.WriteCodePair(5, line.Handle);
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

        private void WritePolyline2d(Polyline polyline)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, polyline.CodeName);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(polyline);
            this.WriteCodePair(5, polyline.Handle);
            this.WriteCodePair(100, SubclassMarker.Polyline);

            this.WriteCodePair(70, (int) polyline.Flags);

            //dummy point
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);

            this.WriteCodePair(30, polyline.Elevation);
            this.WriteCodePair(39, polyline.Thickness);

            this.WriteCodePair(210, polyline.Normal.X);
            this.WriteCodePair(220, polyline.Normal.Y);
            this.WriteCodePair(230, polyline.Normal.Z);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(polyline.XData);

            foreach (PolylineVertex v in polyline.Vertexes)
            {
               
                this.WriteCodePair(0, v.CodeName);
                this.WriteCodePair(5, v.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, v.Layer);
                this.WriteCodePair(100, SubclassMarker.Vertex);
                this.WriteCodePair(100, SubclassMarker.PolylineVertex);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(40, v.BeginThickness);
                this.WriteCodePair(41, v.EndThickness);
                this.WriteCodePair(42, v.Bulge);

                this.WriteXData(v.XData);
            }

            this.WriteCodePair(0, polyline.EndSequence.CodeName);
            this.WriteCodePair(5, polyline.EndSequence.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, polyline.EndSequence.Layer);
        }

        private void WriteLightWeightPolyline(LightWeightPolyline polyline)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, DxfObjectCode.LightWeightPolyline);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(polyline);
            this.WriteCodePair(5, polyline.Handle);
            this.WriteCodePair(100, SubclassMarker.LightWeightPolyline);
            this.WriteCodePair(90, polyline.Vertexes.Count);
            this.WriteCodePair(70, (int) polyline.Flags);

            this.WriteCodePair(38, polyline.Elevation);
            this.WriteCodePair(39, polyline.Thickness);


            foreach (LightWeightPolylineVertex v in polyline.Vertexes)
            {
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(40, v.BeginThickness);
                this.WriteCodePair(41, v.EndThickness);
                this.WriteCodePair(42, v.Bulge);
            }

            this.WriteCodePair(210, polyline.Normal.X);
            this.WriteCodePair(220, polyline.Normal.Y);
            this.WriteCodePair(230, polyline.Normal.Z);

            this.WriteXData(polyline.XData);
        }

        private void WritePolyline3d(Polyline3d polyline)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, polyline.CodeName);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(polyline);
            this.WriteCodePair(5, polyline.Handle);
            this.WriteCodePair(100, SubclassMarker.Polyline3d);

            this.WriteCodePair(70, (int) polyline.Flags);

            //dummy point
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);
            this.WriteCodePair(30, 0.0);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(polyline.XData);

            foreach (Polyline3dVertex v in polyline.Vertexes)
            {
                this.WriteCodePair(0, v.CodeName);
                this.WriteCodePair(5, v.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, v.Layer);
                this.WriteCodePair(100, SubclassMarker.Vertex);
                this.WriteCodePair(100, SubclassMarker.Polyline3dVertex);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);

                this.WriteXData(v.XData);
            }
            this.WriteCodePair(0, polyline.EndSequence.CodeName);
            this.WriteCodePair(5, polyline.EndSequence.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, polyline.EndSequence.Layer);
        }

        private void WritePolyfaceMesh(PolyfaceMesh mesh)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, mesh.CodeName);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(mesh);
            this.WriteCodePair(5, mesh.Handle);
            this.WriteCodePair(100, SubclassMarker.PolyfaceMesh);

            this.WriteCodePair(70, (int) mesh.Flags);

            this.WriteCodePair(71, mesh.Vertexes.Count);

            this.WriteCodePair(72, mesh.Faces.Count);

            //dummy point
            this.WriteCodePair(10, 0.0);
            this.WriteCodePair(20, 0.0);
            this.WriteCodePair(30, 0.0);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            if (mesh.XData != null)
            {
                this.WriteXData(mesh.XData);
            }

            foreach (PolyfaceMeshVertex v in mesh.Vertexes)
            {
                this.WriteCodePair(0, v.CodeName);
                this.WriteCodePair(5, v.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, v.Layer);
                this.WriteCodePair(100, SubclassMarker.Vertex);
                this.WriteCodePair(100, SubclassMarker.PolyfaceMeshVertex);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);

                this.WriteXData(v.XData);
            }

            foreach (PolyfaceMeshFace face in mesh.Faces)
            {
                this.WriteCodePair(0, face.CodeName);
                this.WriteCodePair(5, face.Handle);
                this.WriteCodePair(100, SubclassMarker.Entity);
                this.WriteCodePair(8, face.Layer);
                this.WriteCodePair(100, SubclassMarker.PolyfaceMeshFace);
                this.WriteCodePair(70, (int) VertexTypeFlags.PolyfaceMeshVertex);
                this.WriteCodePair(10, 0);
                this.WriteCodePair(20, 0);
                this.WriteCodePair(30, 0);

                this.WriteCodePair(71, face.VertexIndexes[0]);
                this.WriteCodePair(72, face.VertexIndexes[1]);
                this.WriteCodePair(73, face.VertexIndexes[2]);
            }

            this.WriteCodePair(0, mesh.EndSequence.CodeName);
            this.WriteCodePair(5, mesh.EndSequence.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteCodePair(8, mesh.EndSequence.Layer);
        }

        private void WritePoint(Point point)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, point.CodeName);
            this.WriteCodePair(5, point.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(point);
            this.WriteCodePair(100, SubclassMarker.Point);

            this.WriteCodePair(10, point.Location.X);
            this.WriteCodePair(20, point.Location.Y);
            this.WriteCodePair(30, point.Location.Z);

            this.WriteCodePair(39, point.Thickness);

            this.WriteCodePair(210, point.Normal.X);
            this.WriteCodePair(220, point.Normal.Y);
            this.WriteCodePair(230, point.Normal.Z);

            this.WriteXData(point.XData);
        }

        private void WriteText(Text text)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, text.CodeName);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(text);
            this.WriteCodePair(5, text.Handle);
            this.WriteCodePair(100, SubclassMarker.Text);

            this.WriteCodePair(1, text.Value);

            this.WriteCodePair(10, text.BasePoint.X);
            this.WriteCodePair(20, text.BasePoint.Y);
            this.WriteCodePair(30, text.BasePoint.Z);

            this.WriteCodePair(40, text.Height);

            this.WriteCodePair(41, text.WidthFactor);

            this.WriteCodePair(50, text.Rotation);

            this.WriteCodePair(51, text.ObliqueAngle);

            this.WriteCodePair(7, text.Style);

            this.WriteCodePair(11, text.BasePoint.X);
            this.WriteCodePair(21, text.BasePoint.Y);
            this.WriteCodePair(31, text.BasePoint.Z);

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
            }

            this.WriteXData(text.XData);
        }

        #endregion

        #region methods for Entity section

        public void WriteDictionary(Dictionary dictionary)
        {
            this.WriteCodePair(0, StringCode.Dictionary);
            this.WriteCodePair(5, Convert.ToString(10, 16));
            this.WriteCodePair(100, SubclassMarker.Dictionary);
            this.WriteCodePair(281, 1);
            this.WriteCodePair(3, dictionary);
            this.WriteCodePair(350, Convert.ToString(11, 16));

            this.WriteCodePair(0, StringCode.Dictionary);
            this.WriteCodePair(5, Convert.ToString(11, 16));
            this.WriteCodePair(100, SubclassMarker.Dictionary);
            this.WriteCodePair(281, 1);
        }

        #endregion

        #region private methods

        private void WriteAttributeDefinition(AttributeDefinition def)
        {
            this.WriteCodePair(0, DxfObjectCode.AttributeDefinition);
            this.WriteCodePair(5, def.Handle);

            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(def);
            this.WriteCodePair(100, SubclassMarker.Text);
            this.WriteCodePair(10, def.BasePoint.X);
            this.WriteCodePair(20, def.BasePoint.Y);
            this.WriteCodePair(30, def.BasePoint.Z);
            this.WriteCodePair(40, def.Height);
            this.WriteCodePair(1, def.Value);

            this.WriteCodePair(7, def.Style);

            this.WriteCodePair(41, def.WidthFactor);

            this.WriteCodePair(50, def.Rotation);

            this.WriteCodePair(100, SubclassMarker.AttributeDefinition);
            this.WriteCodePair(2, def.Id);

            this.WriteCodePair(3, def.Text);

            this.WriteCodePair(70, (int) def.Flags);

            switch (def.Alignment)
            {
                case TextAlignment.TopLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.TopRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(74, 3);
                    break;
                case TextAlignment.MiddleLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.MiddleRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(74, 2);
                    break;
                case TextAlignment.BottomLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BottomRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(74, 1);
                    break;
                case TextAlignment.BaselineLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(74, 0);
                    break;
                case TextAlignment.BaselineRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(74, 0);
                    break;
            }

            this.WriteCodePair(11, def.BasePoint.X);
            this.WriteCodePair(21, def.BasePoint.Y);
            this.WriteCodePair(31, def.BasePoint.Z);
        }

        private void WriteAttribute(Attribute attrib, Vector3 puntoInsercion)
        {
            this.WriteCodePair(0, DxfObjectCode.Attribute);
            this.WriteCodePair(5, attrib.Handle);
            this.WriteCodePair(100, SubclassMarker.Entity);
            this.WriteEntityCommonCodes(attrib);

            this.WriteCodePair(100, SubclassMarker.Text);
            this.WriteCodePair(10, attrib.Definition.BasePoint.X + puntoInsercion.X);
            this.WriteCodePair(20, attrib.Definition.BasePoint.Y + puntoInsercion.Y);
            this.WriteCodePair(30, attrib.Definition.BasePoint.Z + puntoInsercion.Z);

            this.WriteCodePair(1, attrib.Value);

            this.WriteCodePair(40, attrib.Definition.Height);

            this.WriteCodePair(41, attrib.Definition.WidthFactor);

            this.WriteCodePair(50, attrib.Definition.Rotation);

            this.WriteCodePair(7, attrib.Definition.Style);

            this.WriteCodePair(100, SubclassMarker.Attribute);

            this.WriteCodePair(2, attrib.Definition.Id);

            this.WriteCodePair(70, (int) attrib.Definition.Flags);


            this.WriteCodePair(11, attrib.Definition.BasePoint.X + puntoInsercion.X);
            this.WriteCodePair(21, attrib.Definition.BasePoint.Y + puntoInsercion.Y);
            this.WriteCodePair(31, attrib.Definition.BasePoint.Z + puntoInsercion.Z);
        }

        private void WriteXData(Dictionary<ApplicationRegistry, XData> xData)
        {
            if (xData == null)
                return;

            foreach (ApplicationRegistry appReg in xData.Keys)
            {
                this.WriteCodePair(XDataCode.AppReg, appReg);
                foreach (XDataRecord x in xData[appReg].XDataRecord)
                {
                    this.WriteCodePair(x.Code, x.Value.ToString());
                }
            }
        }

        private void WriteEntityCommonCodes(IEntityObject entity)
        {
            this.WriteCodePair(8, entity.Layer);
            this.WriteCodePair(62, entity.Color.Index);
            this.WriteCodePair(6, entity.LineType);
        }

        private void WriteCodePair(int codigo, object valor)
        {
            // AutoCad12 does not allow strings with spaces
            string nameConversion = valor == null ? string.Empty : valor.ToString();

            if (this.version == DxfVersion.AutoCad12 && valor is DxfObject) nameConversion = nameConversion.Replace(' ', '_');
            this.writer.WriteLine(codigo);
            this.writer.WriteLine(nameConversion);
        }

        #endregion
    }
}