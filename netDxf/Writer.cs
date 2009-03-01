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
        #endregion

        #region constructors

        public DxfWriter(string file)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            this.file = file;
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

        public void WriteComments(string comments)
        {
            //if (this.isHeader)
            //    throw new DxfException(this.file,"Comments are only allowed at the beginning of the dxf file.");
            if (!string.IsNullOrEmpty(comments))
                this.WriteCodePair(999, comments);
        }

        public void WriteSystemVariable(HeaderVariable variable)
        {
            if (this.activeSection != StringCode.HeaderSection)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }
            this.WriteCodePair(HeaderVariable.NAME_CODE_GROUP, variable.Name);
            this.WriteCodePair(variable.CodeGroup , variable.Value);

        }
        #endregion

        #region methods for Table section

        /// <summary>
        /// Writes a new extended data application registry to the table section.
        /// </summary>
        /// <param name="appReg">Nombre del registro de aplicación.</param>
        public void RegisterApplication(string appReg)
        {
            if (this.activeTable != StringCode.ApplicationIDTable)
            {
                throw new InvalidDxfTableException(StringCode.ApplicationIDTable, this.file);
            }
            
            this.WriteCodePair(0, StringCode.ApplicationIDTable);
            this.WriteCodePair(70, 0);
            this.WriteCodePair(2, appReg);

        }

        /// <summary>
        /// Writes a new line type to the table section.
        /// </summary>
        /// <param name="tl">Line type.</param>
        public void WriteLineType(LineType tl)
        {
            if (this.activeTable != StringCode.LineTypeTable)
            {
                throw new InvalidDxfTableException(this.activeTable, this.file);
            }
            this.WriteCodePair(0, StringCode.LineTypeTable);

            this.WriteCodePair(70, 0);
            this.WriteCodePair(2, tl.Name);
            this.WriteCodePair(3, tl.Description);
            this.WriteCodePair(72, 65);
            this.WriteCodePair(73, tl.Segments.Count);
            this.WriteCodePair(40, tl.Legth());
            foreach (float s in tl.Segments)
            {
                this.WriteCodePair(49, s);
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

            this.WriteCodePair(0, StringCode.LayerTable);
            this.WriteCodePair(70, 0);
            this.WriteCodePair(2, layer.Name);
            
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

            this.WriteCodePair(0, StringCode.TextStyleTable);

            this.WriteCodePair(2, style.Name);
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

        public void WriteBlock(Block block)
        {
            if (this.activeSection != StringCode.BlocksSection)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, StringCode.BegionBlock);

            this.WriteCodePair(8, block.Layer.Name);

            this.WriteCodePair(2, block.Name);

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

            this.WriteCodePair(3, block.Name);

            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                this.WriteAttributeDefinition(attdef);
            }

            //block entities
            this.isBlockEntities = true;
            foreach (IEntityObject entity in block.Entities)
            {
                this.WriteEntity(entity);
            }
            this.isBlockEntities = false;

            this.WriteCodePair(0, StringCode.EndBlock);
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
                case EntityType.Ellipse :
                    this.WriteEllipse((Ellipse)entity);
                    break;
                case EntityType.NurbsCurve:
                    this.WriteNurbsCurve((NurbsCurve)entity);
                    break;
                case EntityType.Point:
                    this.WritePoint((Point) entity);
                    break;
                case EntityType.Face3D:
                    this.WriteFace3D((Face3D) entity);
                    break;
                case EntityType.Solid:
                    this.WriteSolid((Solid)entity);
                    break;
                case EntityType.Insert:
                    this.WriteInsert((Insert) entity);
                    break;
                case EntityType.Line:
                    this.WriteLine((Line) entity);
                    break;
                case EntityType.Polyline:
                    this.WritePolyline((Polyline) entity);
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

            this.WriteCodePair(0, arc.DxfName);

            this.WriteEntityCommonCodes(arc);

            this.WriteCodePair(10, arc.Center.X);
            this.WriteCodePair(20, arc.Center.Y);
            this.WriteCodePair(30, arc.Center.Z);

            this.WriteCodePair(40, arc.Radius);

            this.WriteCodePair(50, arc.StartAngle);
            this.WriteCodePair(51, arc.EndAngle);

            this.WriteCodePair(39, arc.Thickness);

            this.WriteCodePair(210, arc.Normal.X);
            this.WriteCodePair(220, arc.Normal.Y);
            this.WriteCodePair(230, arc.Normal.Z);


            this.WriteXData(arc.XData);
        }

        private void WriteCircle(Circle circle)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, circle.DxfName);

            this.WriteEntityCommonCodes(circle);

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

            //this.WriteCodePair(0, ellipse.DxfName);

            //this.WriteEntityCommonCodes(ellipse);

            //// the ellipse is expressed in ocs
            //Vector3 center = MathHelper.Transform(ellipse.Center, ellipse.Normal, MathHelper.CoordinateSystem.Object , MathHelper.CoordinateSystem.World );
            //this.WriteCodePair(10, center.X );
            //this.WriteCodePair(20, center.Y);
            //this.WriteCodePair(30, center.Z);

            //// XAxis WCS point
            //float sine = (float) (ellipse.MajorAxis*0.5f*Math.Sin(ellipse.Rotation));
            //float cosine = (float) (ellipse.MajorAxis*0.5f*Math.Cos(ellipse.Rotation));
            //Vector3 ocsPoint = new Vector3(cosine + ellipse.Center.X, sine + ellipse.Center.Y,ellipse.Center.Z);
            //Vector3 point = MathHelper.Transform(ocsPoint, ellipse.Normal, MathHelper.CoordinateSystem.Object , MathHelper.CoordinateSystem.World );
            //this.WriteCodePair(11, point.X);
            //this.WriteCodePair(21, point.Y);
            //this.WriteCodePair(31, point.Z);

            //this.WriteCodePair(210, ellipse.Normal.X);
            //this.WriteCodePair(220, ellipse.Normal.Y);
            //this.WriteCodePair(230, ellipse.Normal.Z);

            //this.WriteCodePair(40, ellipse.MinorAxis/ellipse.MajorAxis);
            //this.WriteCodePair(41, ellipse.StartAngle*MathHelper.DEG_TO_RAD );
            //this.WriteCodePair(42, ellipse.EndAngle *MathHelper.DEG_TO_RAD);

            //we will draw the ellipse as a polyline, it is not supported in AutoCad12 dxf files
            this.WriteCodePair(0, DxfEntityCode.Polyline);

            this.WriteEntityCommonCodes(ellipse);

            //closed polyline
            this.WriteCodePair(70, 1);

            //dummy point
            this.WriteCodePair(10, 0.0f);
            this.WriteCodePair(20, 0.0f);
            this.WriteCodePair(30, ellipse.Center.Z);

            this.WriteCodePair(210, ellipse.Normal.X);
            this.WriteCodePair(220, ellipse.Normal.Y);
            this.WriteCodePair(230, ellipse.Normal.Z);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(ellipse.XData);

            List<Vector2> points = ellipse.PolygonalVertexes(32);
            foreach (Vector2 v in points)
            {
                this.WriteCodePair(0, DxfEntityCode.Vertex);
                this.WriteCodePair(8, ellipse.Layer.Name);
                this.WriteCodePair(70, 0);
                this.WriteCodePair(10, v.X);
                this.WriteCodePair(20, v.Y);
            }
            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WriteNurbsCurve(NurbsCurve  nurbsCurve )
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }


            //we will draw the nurbsCurve as a polyline, it is not supported in AutoCad12 dxf files
            this.WriteCodePair(0, DxfEntityCode.Polyline);

            this.WriteEntityCommonCodes(nurbsCurve);

            //open polyline
            this.WriteCodePair(70, 0);

            //dummy point
            this.WriteCodePair(10, 0.0f);
            this.WriteCodePair(20, 0.0f);
            this.WriteCodePair(30, 0.0f);

            this.WriteCodePair(210, nurbsCurve.Normal.X);
            this.WriteCodePair(220, nurbsCurve.Normal.Y);
            this.WriteCodePair(230, nurbsCurve.Normal.Z);

            //Obsolete; formerly an “entities follow flag” (optional; ignore if present)
            //but its needed to load the dxf file in AutoCAD
            this.WriteCodePair(66, "1");

            this.WriteXData(nurbsCurve.XData);

            List<Vector2> points = nurbsCurve.PolygonalVertexes(30);
            foreach (Vector2 v in points)
            {
                this.WriteCodePair(0, DxfEntityCode.Vertex);
                this.WriteCodePair(8, nurbsCurve.Layer.Name);
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

            this.WriteCodePair(0, solid.DxfName);

            this.WriteEntityCommonCodes(solid);

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

        private void WriteFace3D(Face3D face)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, face.DxfName);

            this.WriteEntityCommonCodes(face);

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

            this.WriteCodePair(0, insert.DxfName);

            this.WriteEntityCommonCodes(insert);

            this.WriteCodePair(2, insert.Block.Name);

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

                this.WriteCodePair(0, StringCode.EndSequence);
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

            this.WriteCodePair(0, line.DxfName);

            this.WriteEntityCommonCodes(line);

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

        private void WritePolyline(Polyline polyline)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, polyline.DxfName);

            this.WriteEntityCommonCodes(polyline);

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
                this.WriteCodePair(0, v.DxfName);
                this.WriteCodePair(8, v.Layer.Name);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(40, v.BeginThickness);
                this.WriteCodePair(41, v.EndThickness);
                this.WriteCodePair(42, v.Bulge);

                this.WriteXData(v.XData);
            }
            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WritePolyline3d(Polyline3d polyline)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, polyline.DxfName);

            this.WriteEntityCommonCodes(polyline);

            this.WriteCodePair(70, (int)polyline.Flags);

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
                this.WriteCodePair(0, v.DxfName);
                this.WriteCodePair(8, v.Layer.Name);
                this.WriteCodePair(70, (int)v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);
                
                this.WriteXData(v.XData);
            }
            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WritePolyfaceMesh(PolyfaceMesh mesh)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, mesh.DxfName);

            this.WriteEntityCommonCodes(mesh);

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
                this.WriteCodePair(0, v.DxfName);
                this.WriteCodePair(8, v.Layer);
                this.WriteCodePair(70, (int) v.Flags);
                this.WriteCodePair(10, v.Location.X);
                this.WriteCodePair(20, v.Location.Y);
                this.WriteCodePair(30, v.Location.Z);

                this.WriteXData(v.XData);
            }

            foreach (PolyfaceMeshFace face in mesh.Faces)
            {
                this.WriteCodePair(0, DxfEntityCode.Vertex);
                this.WriteCodePair(8, mesh.Layer);
                this.WriteCodePair(70, (int) VertexTypeFlags.PolyfaceMeshVertex);
                this.WriteCodePair(10, 0);
                this.WriteCodePair(20, 0);
                this.WriteCodePair(30, 0);

                this.WriteCodePair(71, face.VertexIndexes[0]);
                this.WriteCodePair(72, face.VertexIndexes[1]);
                this.WriteCodePair(73, face.VertexIndexes[2]);
            }

            this.WriteCodePair(0, StringCode.EndSequence);
        }

        private void WritePoint(Point point)
        {
            if (this.activeSection != StringCode.EntitiesSection && !this.isBlockEntities)
            {
                throw new InvalidDxfSectionException(this.activeSection, this.file);
            }

            this.WriteCodePair(0, point.DxfName);

            this.WriteEntityCommonCodes(point);

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

            this.WriteCodePair(0, text.DxfName);

            this.WriteEntityCommonCodes(text);

            this.WriteCodePair(1, text.Value);

            this.WriteCodePair(10, text.BasePoint.X);
            this.WriteCodePair(20, text.BasePoint.Y);
            this.WriteCodePair(30, text.BasePoint.Z);

            this.WriteCodePair(40, text.Height);

            this.WriteCodePair(50, text.Rotation);

            this.WriteCodePair(51, text.ObliqueAngle);

            this.WriteCodePair(7, text.Style.Name);


            switch (text.Alignment)
            {
                case TextAlignment.TopLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.TopCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.TopRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(73, 3);
                    break;

                case TextAlignment.MiddleLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.MiddleCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.MiddleRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(73, 2);
                    break;

                case TextAlignment.BottomLeft:

                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(73, 1);
                    break;
                case TextAlignment.BottomCenter:

                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(73, 1);
                    break;

                case TextAlignment.BottomRight:

                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(73, 1);
                    break;

                case TextAlignment.BaselineLeft:
                    this.WriteCodePair(72, 0);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.BaselineCenter:
                    this.WriteCodePair(72, 1);
                    this.WriteCodePair(73, 0);
                    break;

                case TextAlignment.BaselineRight:
                    this.WriteCodePair(72, 2);
                    this.WriteCodePair(73, 0);
                    break;
            }

            this.WriteCodePair(11, text.BasePoint.X);
            this.WriteCodePair(21, text.BasePoint.Y);
            this.WriteCodePair(31, text.BasePoint.Z);

            this.WriteCodePair(210, text.Normal.X);
            this.WriteCodePair(220, text.Normal.Y);
            this.WriteCodePair(230, text.Normal.Z);

            this.WriteXData(text.XData);
        }

        #endregion

        #region private methods

        private void WriteAttributeDefinition(AttributeDefinition def)
        {
            this.WriteCodePair(0, DxfEntityCode.AttributeDefinition);

            this.WriteCodePair(2, def.Id);

            this.WriteCodePair(3, def.Text);

            this.WriteCodePair(1, def.Value);

            this.WriteCodePair(70, (int) def.Flags);

            this.WriteEntityCommonCodes(def);

            this.WriteCodePair(10, def.BasePoint.X);
            this.WriteCodePair(20, def.BasePoint.Y);
            this.WriteCodePair(30, def.BasePoint.Z);

            this.WriteCodePair(7, def.Style.Name);

            this.WriteCodePair(40, def.Height);

            this.WriteCodePair(41, def.WidthFactor);

            this.WriteCodePair(50, def.Rotation);

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
            this.WriteCodePair(0, DxfEntityCode.Attribute);

            this.WriteCodePair(2, attrib.Definition.Id);

            this.WriteCodePair(1, attrib.Value);

            this.WriteCodePair(70, (int) attrib.Definition.Flags);

            this.WriteEntityCommonCodes(attrib);

            this.WriteCodePair(10, attrib.Definition.BasePoint.X + puntoInsercion.X);
            this.WriteCodePair(20, attrib.Definition.BasePoint.Y + puntoInsercion.Y);
            this.WriteCodePair(30, attrib.Definition.BasePoint.Z + puntoInsercion.Z);

            this.WriteCodePair(40, attrib.Definition.Height);

            this.WriteCodePair(41, attrib.Definition.WidthFactor);

            this.WriteCodePair(50, attrib.Definition.Rotation);

            this.WriteCodePair(7, attrib.Definition.Style.Name);

            this.WriteCodePair(11, attrib.Definition.BasePoint.X + puntoInsercion.X);
            this.WriteCodePair(21, attrib.Definition.BasePoint.Y + puntoInsercion.Y);
            this.WriteCodePair(31, attrib.Definition.BasePoint.Z + puntoInsercion.Z);
        }

        private void WriteXData(IEnumerable<XData> xData)
        {
            foreach (XData xDataEntry in xData)
            {
                this.WriteCodePair(xDataEntry.ApplicationRegistry.Code, xDataEntry.ApplicationRegistry.Value.ToString());
                foreach (XDataRecord x in xDataEntry.XDataRecord)
                {
                    if (x.Code != XDataCode.AppReg)
                        this.WriteCodePair(x.Code, x.Value.ToString());
                }
            }
        }

        private void WriteEntityCommonCodes(IEntityObject entity)
        {
            this.WriteCodePair(8, entity.Layer.Name);
            this.WriteCodePair(62, entity.Color.Index);
            this.WriteCodePair(6, entity.LineType.Name);
        }

        private void WriteCodePair(int codigo, object valor)
        {
            this.writer.WriteLine(codigo);
            this.writer.WriteLine(valor);
        }

        #endregion
    }
}