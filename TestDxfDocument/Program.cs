#region netDxf, Copyright(C) 2012 Daniel Carvajal, Licensed under LGPL.

//                        netDxf library
// Copyright (C) 2012 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Drawing;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using Point = netDxf.Entities.Point;

namespace TestDxfDocument
{
    /// <summary>
    /// This is just a simple test of work in progress for the netDxf Library.
    /// </summary>
    internal class Program
    {
        private static void Main()
        {
            //CleanDrawing();
            TestDimensionDrawing();
            //WriteMText();
            //LineWidth();
            //HatchCircleBoundary();
            //ToPolyline();
            //FilesTest();
            //CustomHatchPattern();
            //LoadSaveHatchTest();
            //WriteDxfFile();
            //ReadDxfFile();
            //ExplodeTest();
            //HatchTestLinesBoundary();
            //HatchTest3();
            //BlockAttributes();
            //WritePolyfaceMesh();
            //Ellipse();
            //Solid();
            //Face3d();
            //LwPolyline();
            //Polyline();
            //NurbsCurve();
            //Dxf2000();
            //SpeedTest();
            //WritePolyline3d();
        }

        private static void CleanDrawing()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.Save("clean drawing.dxf", DxfVersion.AutoCad2004);
        }
        private static void TestDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();
            double offset = 0.9;
            Vector3 p1 = new Vector3(1, 2, 0);
            Vector3 p2 = new Vector3(2, 6, 0);
            Line line = new Line(p1, p2);
            Vector3 l1;
            Vector3 l2;
            MathHelper.OffsetLine(line.StartPoint, line.EndPoint, line.Normal, offset, out l1, out l2);
            Line parallel = new Line(l1,l2);
            dxf.AddEntity(line);
            dxf.AddEntity(parallel);

            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DIMPOST = "<>mm";
            AlignedDimension dim = new AlignedDimension(p1, p2, offset, myStyle);
            Vector2 perp = Vector2.Perpendicular(new Vector2(p2.X, p2.Y) - new Vector2(p1.X, p1.Y));
            dim.Normal = -new Vector3(perp.X, perp.Y, 0.0) ;

            XData xdata = new XData(new ApplicationRegistry("other application"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "string record"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Long, 350));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);
            dim.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata.ApplicationRegistry, xdata}
                             };

            dxf.AddEntity(dim);
            dxf.Save("test drawing dimension.dxf", DxfVersion.AutoCad2000);

            //dxf.Load("test drawing dimension from CAD 2004 - copia.dxf");

        }
        private static void WriteMText()
        {
            DxfDocument dxf = new DxfDocument();

            //xData sample
            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, 0));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            //text
            TextStyle style = new TextStyle("Times.ttf");
            //TextStyle style = TextStyle.Default;
            MText mText = new MText(new Vector3(3,2,0), 1.0f, 100.0f, style);
            mText.Layer = new Layer("Multiline Text");
            //mText.Layer.Color.Index = 8;
            mText.Rotation = 0;
            mText.LineSpacingFactor = 1.0;
            mText.ParagraphHeightFactor = 1.0;

            //mText.AttachmentPoint = MTextAttachmentPoint.TopCenter;
            //mText.Write("Hello World!");
            //mText.Write(" we keep writting on the same line.");
            //mText.WriteLine("This text is in a new line");

            //mText.Write("Hello World! ");
            //for (int i = 0; i < 50; i++)
            //{
            //    mText.Write("1234567890");
            //}
            //mText.Write(" This text is over the limit of the 250 character chunk");
            //mText.NewParagraph();
            //mText.Write("This is a text in a new paragraph");
            //mText.Write(" and we continue writing in the previous paragraph");
            //mText.NewParagraph();
            MTextFormattingOptions options = new MTextFormattingOptions(mText.Style);
            options.Bold = true;
            mText.Write("Bold text in mText.Style", options);
            mText.EndParagraph();
            options.Italic = true;
            mText.Write("Bold and italic text in mText.Style", options);
            mText.EndParagraph();
            options.Bold = false;
            options.FontName = "Arial";
            options.Color = AciColor.Blue;
            mText.ParagraphHeightFactor = 2;
            mText.Write("Italic text in Arial", options);
            mText.EndParagraph();
            options.Italic = false;
            options.Color = null; // back to the default text color
            mText.Write("Normal text in Arial with the default paragraph height factor", options);
            mText.EndParagraph();
            mText.ParagraphHeightFactor = 1;
            mText.Write("No formatted text uses mText.Style");
            mText.Write(" and the text continues in the same paragraph.");
            mText.EndParagraph();

            //options.HeightPercentage = 2.5;
            //options.Color = AciColor.Red;
            //options.Overstrike = true;
            //options.Underline = true;
            //options.FontFile = "times.ttf";
            //options.ObliqueAngle = 15;
            //options.CharacterSpacePercentage = 2.35;
            //options.WidthFactor = 1.8;
            
            //for unknown reasons the aligment doesn't seem to change anything
            //mText.Write("Formatted text", options);
            //options.Aligment = MTextFormattingOptions.TextAligment.Center;
            //mText.Write("Center", options);
            //options.Aligment = MTextFormattingOptions.TextAligment.Top;
            //mText.Write("Top", options);
            //options.Aligment = MTextFormattingOptions.TextAligment.Bottom;
            //mText.Write("Bottom", options);

            mText.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata.ApplicationRegistry, xdata}
                             };
            
            dxf.AddEntity(mText);

            dxf.Save("MText sample.dxf", DxfVersion.AutoCad2000);

        }
        private static void HatchCircleBoundary()
        {
            DxfDocument dxf = new DxfDocument();

            // create a circle that will be our hatch boundary in this case it is a circle with center (5.5, -5.5, 0.0) and a radius 10.0
            Circle circle = new Circle(new Vector3(5.5, -5.5, 0), 10);

            // create the hatch boundary path with only the circle (a circle is already a closed loop it is all we need to define a valid boundary path)
            // a hatch can have many boundaries (closed loops) and every boundary path can be made of several entities (lines, polylines, arcs, circles and ellipses)
            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
                                                        {
                                                            new HatchBoundaryPath(new List<IEntityObject>{circle})
                                                        };  

            // create the hatch in this case we will use the predefined Solid hatch pattern and our circle as the boundary path
            Hatch hatch = new Hatch(HatchPattern.Solid, boundary);

            // to give a color to the hatch, we have to options:

            // create a new layer with a color for the hatch (in this case by default the hatch will have a ByLayer color)
            //Layer hatchLayer = new Layer("HathLayer") {Color = AciColor.Green};
            //hatch.Layer = hatchLayer;

            // or give the hatch a color just for it
            // old AutoCAD versions only had 255 colors (indexed color), now in AutoCAD you can use true colors (8 bits per channel) but at the moment this is not supported.
            // if you try to give r, g, b values to define a color it will be converted to an indexed color
            // (I haven't tested this code a lot, so errors might appear and the result might not be what you expected).
            hatch.Color = AciColor.Red;

            // the hatch by itself will not show the boundary, but we can use the same entity to show the limits of the hatch, adding it to the document 
            dxf.AddEntity(circle);

            // add the hatch to the document
            dxf.AddEntity(hatch);

            dxf.Save("circle solid fill.dxf", DxfVersion.AutoCad2000);
        }
        private static void LineWidth()
        {
            // the line thickness works as expected, according to the AutoCAD way of doing things
            Line thickLine = new Line(new Vector3(0,10,0),  new Vector3(10,20,0));

            // when you assign a thickness to a line, the result is like a wall, it is like a 3d face whose vertexes are defined by the
            // start and end points of the line and the thickness along the normal of the line.
            thickLine.Thickness = 5;

            // maybe what you are trying to do is create a line with a width (something that we can read it as a line with thickness), the only way to do this is to create a polyline
            // the kind of result you will get if you give a width to a 2d polyline 
            // you can only give a width to a vertex of a Polyline or a LightweigthPolyline
            // I am planning to drop support to AutoCAD 12 dxf files, so to define a bidimensional polyline the only way will be to use lightweight polyline
            // (the Polyline class and the LightWeightPolyline are basically the same).
            LwPolyline widthLine = new LwPolyline();
            LwPolylineVertex startVertex = new LwPolylineVertex(new Vector2(0, 0));
            LwPolylineVertex endVertex = new LwPolylineVertex(new Vector2(10, 10));
            widthLine.Vertexes = new List<LwPolylineVertex> { startVertex, endVertex };

            // the easy way to give a constant width to a polyline, but you can also give a polyline width by vertex
            // there is a mistake on my part, following the AutoCAD documentation I should have called the PolylineVertex.StartThickness and PolylineVertex.EndThickness as
            // PolylineVertex.StartWidth and PolylineVertex.EndWidth
            // SetConstantWidth is a sort cut that will asign the given value to every start width and end width of every vertex of the polyline
            widthLine.SetConstantWidth(0.5);

            DxfDocument dxf = new DxfDocument();

            // add the entities to the document (both of them to see the difference)
            dxf.AddEntity(thickLine);
            dxf.AddEntity(widthLine);

            dxf.Save("line width.dxf", DxfVersion.AutoCad2000);

        }
        private static void ToPolyline()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 center = new Vector3(1, 8, -7);
            Vector3 normal = new Vector3(1, 1, 1);

            Circle circle = new Circle(center, 7.5);
            circle.Normal = normal;

            Arc arc = new Arc(center, 5, 30, 215);
            arc.Normal = normal;


            Ellipse ellipse = new Ellipse(center, 15, 7.5);
            ellipse.Rotation = 35;
            ellipse.Normal = normal;

            Ellipse ellipseArc = new Ellipse(center, 10, 5);
            ellipseArc.StartAngle = 30;
            ellipseArc.EndAngle = 325;
            ellipseArc.Rotation = 35;
            ellipseArc.Normal = normal;

            dxf.AddEntity(circle);
            dxf.AddEntity(circle.ToPolyline(10));

            dxf.AddEntity(arc);
            dxf.AddEntity(arc.ToPolyline(10));

            dxf.AddEntity(ellipse);
            dxf.AddEntity(ellipse.ToPolyline(10));

            dxf.AddEntity(ellipseArc);
            dxf.AddEntity(ellipseArc.ToPolyline(10));

            dxf.Save("to polyline.dxf", DxfVersion.AutoCad2000);

            dxf.Load("to polyline.dxf");

            dxf.Save("to polyline2.dxf", DxfVersion.AutoCad2000);
        }
        private static void CustomHatchPattern()
        {
            DxfDocument dxf = new DxfDocument();

            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(-10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, 10));
            poly.Vertexes.Add(new LwPolylineVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            LwPolyline poly2 = new LwPolyline();
            poly2.Vertexes.Add(new LwPolylineVertex(-5, -5));
            poly2.Vertexes.Add(new LwPolylineVertex(5, -5));
            poly2.Vertexes.Add(new LwPolylineVertex(5, 5));
            poly2.Vertexes.Add(new LwPolylineVertex(-5, 5));
            poly2.Vertexes[1].Bulge = -0.25;
            poly2.IsClosed = true;

            LwPolyline poly3 = new LwPolyline();
            poly3.Vertexes.Add(new LwPolylineVertex(-8, -8));
            poly3.Vertexes.Add(new LwPolylineVertex(-6, -8));
            poly3.Vertexes.Add(new LwPolylineVertex(-6, -6));
            poly3.Vertexes.Add(new LwPolylineVertex(-8, -6));
            poly3.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>{
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly2}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly3}),
                                                                          };

            HatchPattern pattern = new HatchPattern("MyPattern", "A custom hatch pattern");

            HatchPatternLineDefinition line1 = new HatchPatternLineDefinition();
            line1.Angle = 45;
            line1.Origin = Vector2.Zero;
            line1.Delta=new Vector2(4,4);
            line1.DashPattern.Add(12);
            line1.DashPattern.Add(-4);
            pattern.LineDefinitions.Add(line1);

            HatchPatternLineDefinition line2 = new HatchPatternLineDefinition();
            line2.Angle = 135;
            line2.Origin = new Vector2(2.828427125, 2.828427125);
            line2.Delta = new Vector2(4,-4);
            line2.DashPattern.Add(12);
            line2.DashPattern.Add(-4);
            pattern.LineDefinitions.Add(line2);

            Hatch hatch = new Hatch(pattern, boundary);
            hatch.Layer = new Layer("hatch")
            {
                Color = AciColor.Red,
                LineType = LineType.Continuous
            };
            hatch.Pattern.Angle = 0;
            hatch.Pattern.Scale = 1;
            dxf.AddEntity(poly);
            dxf.AddEntity(poly2);
            dxf.AddEntity(poly3);
            dxf.AddEntity(hatch);

            dxf.Save("hatchTest.dxf", DxfVersion.AutoCad2000);
        }
        private static void FilesTest()
        {
            LineType lineType = LineType.FromFile("acad.lin", "ACAD_ISO15W100");
            HatchPattern hatch = HatchPattern.FromFile("acad.pat", "zigzag");

        }
        private static void LoadSaveHatchTest()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.Load("Hatch2.dxf");
            dxf.Save("HatchTest.dxf", DxfVersion.AutoCad2000);
        }
        private static void ExplodeTest()
        {
            DxfDocument dxf = new DxfDocument();
            //polyline
            LwPolylineVertex polyVertex;
            List<LwPolylineVertex> polyVertexes = new List<LwPolylineVertex>();
            polyVertex = new LwPolylineVertex(new Vector2(-50, -23.5));
            polyVertex.Bulge = 1.33;
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(34.8, -42.7));
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(65.3, 54.7));
            polyVertex.Bulge = -0.47;
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(-48.2, 42.5));
            polyVertexes.Add(polyVertex);
            LwPolyline polyline2d = new LwPolyline(polyVertexes);
            polyline2d.Layer = new Layer("polyline2d");
            polyline2d.Layer.Color.Index = 5;
            polyline2d.Normal = new Vector3(1, 1, 1);
            polyline2d.Elevation = 100.0f;

            dxf.AddEntity(polyline2d);
            dxf.AddEntity(polyline2d.Explode());

            dxf.Save("explode.dxf", DxfVersion.AutoCad2000);
        }
        private static void HatchTestLinesBoundary()
        {
            DxfDocument dxf = new DxfDocument();

            Line line1 = new Line(new Vector3(-10,-10,0),new Vector3(10,-10,0));
            Line line2 = new Line(new Vector3(10, -10, 0), new Vector3(10, 10, 0));
            Line line3 = new Line(new Vector3(10, 10, 0), new Vector3(-10, 10, 0));
            Line line4 = new Line(new Vector3(-10, 10, 0), new Vector3(-10, -10, 0));


            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>{
                                                                            new HatchBoundaryPath(new List<IEntityObject>{line1, line2, line3, line4})
                                                                          };
            Hatch hatch = new Hatch(HatchPattern.Line, boundary);
            hatch.Layer = new Layer("hatch")
            {
                Color = AciColor.Red,
                LineType = LineType.Dashed
            };
            hatch.Pattern.Angle = 45;

            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "netDxf hatch"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Distance, hatch.Pattern.Scale));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, hatch.Pattern.Angle));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            hatch.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata.ApplicationRegistry, xdata},
                             };

            dxf.AddEntity(line1);
            dxf.AddEntity(line2);
            dxf.AddEntity(line3);
            dxf.AddEntity(line4);
            dxf.AddEntity(hatch);

            dxf.Save("hatchTest.dxf", DxfVersion.AutoCad2000);
        }
        private static void HatchTest1()
        {
            DxfDocument dxf = new DxfDocument();

            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(-10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, 10));
            poly.Vertexes.Add(new LwPolylineVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            LwPolyline poly2 = new LwPolyline();
            poly2.Vertexes.Add(new LwPolylineVertex(-5, -5));
            poly2.Vertexes.Add(new LwPolylineVertex(5, -5));
            poly2.Vertexes.Add(new LwPolylineVertex(5, 5));
            poly2.Vertexes.Add(new LwPolylineVertex(-5, 5));
            poly2.Vertexes[1].Bulge = -0.25;
            poly2.IsClosed = true;

            LwPolyline poly3 = new LwPolyline();
            poly3.Vertexes.Add(new LwPolylineVertex(-8, -8));
            poly3.Vertexes.Add(new LwPolylineVertex(-6, -8));
            poly3.Vertexes.Add(new LwPolylineVertex(-6, -6));
            poly3.Vertexes.Add(new LwPolylineVertex(-8, -6));
            poly3.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>{
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly2}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly3}),
                                                                          };
            Hatch hatch = new Hatch(HatchPattern.Net, boundary);
            hatch.Layer = new Layer("hatch")
                              {
                                  Color = AciColor.Red,
                                  LineType = LineType.Continuous
                              };
            hatch.Pattern.Angle = 30;

            hatch.Pattern.Scale = 1 / hatch.Pattern.LineDefinitions[0].Delta.Y;
            dxf.AddEntity(poly);
            dxf.AddEntity(poly2);
            dxf.AddEntity(poly3);
            dxf.AddEntity(hatch);

            dxf.Save("hatchTest.dxf", DxfVersion.AutoCad2000);
            dxf.Load("hatchTest.dxf");
        }
        private static void HatchTest2()
        {
            DxfDocument dxf = new DxfDocument();

            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(-10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, 10));
            poly.Vertexes.Add(new LwPolylineVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            Circle circle = new Circle(Vector3.Zero, 5);

            Ellipse ellipse = new Ellipse(Vector3.Zero,16,10);
            ellipse.Rotation = 30;
            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>{
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{ellipse})
                                                                          };

            Hatch hatch = new Hatch(HatchPattern.Line, boundary);
            hatch.Pattern.Angle = 150;
            dxf.AddEntity(poly);
            dxf.AddEntity(circle);
            dxf.AddEntity(ellipse);
            dxf.AddEntity(hatch);

            dxf.Save("hatchTest.dxf", DxfVersion.AutoCad2000);
        }
        private static void HatchTest3()
        {
            DxfDocument dxf = new DxfDocument();

            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(-10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, -10));
            poly.Vertexes.Add(new LwPolylineVertex(10, 10));
            poly.Vertexes.Add(new LwPolylineVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            Ellipse ellipse = new Ellipse(Vector3.Zero, 16, 10);
            ellipse.Rotation = 0;
            ellipse.StartAngle = 0;
            ellipse.EndAngle = 180;

            LwPolyline poly2 = new LwPolyline();
            poly2.Vertexes.Add(new LwPolylineVertex(-8, 0));
            poly2.Vertexes.Add(new LwPolylineVertex(0, -4));
            poly2.Vertexes.Add(new LwPolylineVertex(8, 0));

            Arc arc = new Arc(Vector3.Zero,8,180,0);
            Line line =new Line(new Vector3(8,0,0), new Vector3(-8,0,0));

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>{
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly}),
                                                                            new HatchBoundaryPath(new List<IEntityObject>{poly2, ellipse})
                                                                          };

            Hatch hatch = new Hatch(HatchPattern.Line, boundary);
            hatch.Pattern.Angle = 45;
            dxf.AddEntity(poly);
            dxf.AddEntity(ellipse);
            //dxf.AddEntity(arc);
            //dxf.AddEntity(line);
            dxf.AddEntity(poly2);
            dxf.AddEntity(hatch);

            dxf.Save("hatchTest.dxf", DxfVersion.AutoCad2010);
        }
        private static void Dxf2000()
        {
            DxfDocument dxf = new DxfDocument();
           //line
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(5, 5, 5));
            line.Layer = new Layer("line");
            line.Layer.Color.Index = 6;

            dxf.AddEntity(line);

            dxf.Save("test2000.dxf",DxfVersion.AutoCad2000);
        }
        private static void NurbsCurve()
        {
            DxfDocument dxf = new DxfDocument();

            NurbsCurve nurbs  = new NurbsCurve();
            nurbs.ControlPoints .Add(new NurbsVertex( 0, 0));
            nurbs.ControlPoints.Add(new NurbsVertex(10, 10));
            nurbs.ControlPoints.Add(new NurbsVertex(20, 0));
            nurbs.ControlPoints.Add(new NurbsVertex(30, 10));
            nurbs.ControlPoints.Add(new NurbsVertex(40, 0));
            nurbs.ControlPoints.Add(new NurbsVertex(50, 10));
            nurbs.ControlPoints.Add(new NurbsVertex(60, 0));
            nurbs.ControlPoints.Add(new NurbsVertex(70, 10));

            nurbs.Order = 3;

           dxf.AddEntity(nurbs);

           NurbsCurve nurbs2 = new NurbsCurve();
           nurbs2.ControlPoints.Add(new NurbsVertex(5, 0));
           nurbs2.ControlPoints.Add(new NurbsVertex(10, 0));
           nurbs2.ControlPoints.Add(new NurbsVertex(10, 5));
           nurbs2.ControlPoints.Add(new NurbsVertex(10, 10));
           nurbs2.ControlPoints.Add(new NurbsVertex(5, 10));
           nurbs2.ControlPoints.Add(new NurbsVertex(0, 10));
           nurbs2.ControlPoints.Add(new NurbsVertex(0, 5));
           nurbs2.ControlPoints.Add(new NurbsVertex(0, 0));
           nurbs2.ControlPoints.Add(new NurbsVertex(5, 0));

           nurbs2.Order = 3;

           nurbs2.SetUniformWeights(Math.Cos(MathHelper.HalfPI));
           dxf.AddEntity(nurbs2);

            dxf.Save("nurbs.dxf", DxfVersion.AutoCad2000);

        }
        private static void LwPolyline()
        {

            DxfDocument dxf = new DxfDocument();

            LwPolyline poly = new LwPolyline();
            poly.Vertexes.Add(new LwPolylineVertex(0, 0));
            poly.Vertexes.Add(new LwPolylineVertex(10, 10));
            poly.Vertexes.Add(new LwPolylineVertex(20, 0));
            poly.Vertexes.Add(new LwPolylineVertex(30, 10));
            dxf.AddEntity(poly);

            dxf.Save("polyline.dxf", DxfVersion.AutoCad2000);

        }
        private static void Polyline()
        {

            DxfDocument dxf = new DxfDocument();

            Polyline poly = new Polyline();
            poly.Vertexes.Add(new PolylineVertex(0, 0, 0));
            poly.Vertexes.Add(new PolylineVertex(10, 10, 0));
            poly.Vertexes.Add(new PolylineVertex(20, 0, 0));
            poly.Vertexes.Add(new PolylineVertex(30, 10, 0));
            dxf.AddEntity(poly);

            //dxf.Save("polyline.dxf", DxfVersion.AutoCad2010);

        }
        private static void Solid()
        {

            DxfDocument dxf = new DxfDocument();

            Solid solid = new Solid();
            solid.FirstVertex=new Vector3(0,0,0);
            solid.SecondVertex  = new Vector3(1, 0, 0);
            solid.ThirdVertex  = new Vector3(0, 1, 0);
            solid.FourthVertex  = new Vector3(1, 1, 0);
            dxf.AddEntity(solid);

            dxf.Save("solid.dxf", DxfVersion.AutoCad2000);
            //dxf.Load("solid.dxf");
            //dxf.Save("solid.dxf");

        }
        private static void Face3d()
        {

            DxfDocument dxf = new DxfDocument();

            Face3d face3d = new Face3d();
            face3d.FirstVertex = new Vector3(0, 0, 0);
            face3d.SecondVertex = new Vector3(1, 0, 0);
            face3d.ThirdVertex = new Vector3(1, 1, 0);
            face3d.FourthVertex = new Vector3(0, 1, 0);
            dxf.AddEntity(face3d);

            dxf.Save("face.dxf", DxfVersion.AutoCad2000);
            dxf.Load("face.dxf");
            dxf.Save("face return.dxf", DxfVersion.AutoCad2000);

        }
        private static void Ellipse()
        {
           
            DxfDocument dxf = new DxfDocument();

            Line line = new Line(new Vector3(0, 0, 0), new Vector3(2 * Math.Cos(Math.PI / 4),2 * Math.Cos(Math.PI / 4), 0));

            dxf.AddEntity(line);

            Line line2 = new Line(new Vector3(0, 0, 0), new Vector3(0, -2, 0));
            dxf.AddEntity(line2);

            Arc arc=new Arc(Vector3.Zero,2,45,270);
            dxf.AddEntity(arc);

            // ellipses are saved as polylines
            Ellipse ellipse = new Ellipse(new Vector3(2,2,0), 5,3);
            ellipse.Rotation = 30;
            ellipse.Normal=new Vector3(1,1,1);
            ellipse.Thickness = 2;
            dxf.AddEntity(ellipse);


            dxf.Save("ellipse.dxf", DxfVersion.AutoCad2000);
            dxf = new DxfDocument();
            dxf.Load("ellipse.dxf");
           
        }
        private static void SpeedTest()
        {
            Stopwatch crono = new Stopwatch();
            float totalTime=0;

            crono.Start();
            DxfDocument dxf = new DxfDocument();
            // create 100,000 lines
            for (int i=0; i<100000;i++)
            {
                 //line
                Line line = new Line(new Vector3(0, i, 0), new Vector3(5, i, 0));
                line.Layer = new Layer("line");
                line.Layer.Color.Index = 6;
                dxf.AddEntity(line);
            }

            Console.WriteLine("Time creating entities : " + crono.ElapsedMilliseconds / 1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Reset();

            crono.Start();
            dxf.Save("speedtest.dxf", DxfVersion.AutoCad2000);
            Console.WriteLine("Time saving file : " + crono.ElapsedMilliseconds / 1000.0f);
           totalTime += crono.ElapsedMilliseconds;
            crono.Reset();

            crono.Start();
            dxf.Load("speedtest.dxf");
            Console.WriteLine("Time loading file : " + crono.ElapsedMilliseconds / 1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Stop();

            Console.WriteLine("Total time : " + totalTime / 1000.0f);
            Console.ReadLine();

        }
        private static void BlockAttributes()
        {
            DxfDocument dxf = new DxfDocument( );
            Block block = new Block("BlockWithAttributes");
            block.Layer = new Layer("BlockSample");

            AttributeDefinition attdef = new AttributeDefinition("NewAttribute");
            attdef.Text = "InfoText";
            attdef.BasePoint = new Vector3(1, 1, 1);
            attdef.Style.IsVertical = true;
            attdef.Rotation = 45;

            block.Attributes.Add(attdef.Id, attdef);
            block.Entities.Add(new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0)));
            block.Entities.Add(new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0)));

            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");
            insert.Rotation = 45;
            insert.Layer.Color.Index = 4;
            insert.Attributes[0].Value = 24;

            Insert insert2 = new Insert(block, new Vector3(-5, -5, -5));
            insert2.Attributes[0].Value = 34;

            XData xdata1 = new XData(new ApplicationRegistry("netDxf"));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata1.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, 0));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, 0));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, 0));
            xdata1.XDataRecord.Add(XDataRecord.CloseControlString);

            XData xdata2 = new XData(new ApplicationRegistry("other application"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata2.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "string record"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Long, 350));
            xdata2.XDataRecord.Add(XDataRecord.CloseControlString);

            insert.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata1.ApplicationRegistry, xdata1},
                             };
            dxf.AddEntity(insert);
            dxf.AddEntity(insert2);

            Circle circle = new Circle(Vector3.Zero, 5);
            circle.Layer = new Layer("circle");
            circle.Layer.Color.Index = 2;
            circle.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata2.ApplicationRegistry, xdata2},
                             };
            dxf.AddEntity(circle);

            dxf.Save("Block with attributes.dxf", DxfVersion.AutoCad2000);
            //dxf.Load("Block with attributes.dxf");
            //dxf.Save("Block with attributes result.dxf", DxfVersion.AutoCad2000); // both results must be equal only the handles might be different
        }
        private static void WritePolyfaceMesh()
        {
            DxfDocument dxf = new DxfDocument();


            List<PolyfaceMeshVertex> vertexes = new List<PolyfaceMeshVertex>
                                                    {
                                                        new PolyfaceMeshVertex(0, 0, 0),
                                                        new PolyfaceMeshVertex(10, 0, 0),
                                                        new PolyfaceMeshVertex(10, 10, 0),
                                                        new PolyfaceMeshVertex(5, 15, 0),
                                                        new PolyfaceMeshVertex(0, 10, 0)
                                                    };
            List<PolyfaceMeshFace> faces = new List<PolyfaceMeshFace>
                                               {
                                                   new PolyfaceMeshFace(new[] {1, 2, -3}),
                                                   new PolyfaceMeshFace(new[] {-1, 3, -4}),
                                                   new PolyfaceMeshFace(new[] {-1, 4, 5})
                                               };

            PolyfaceMesh mesh = new PolyfaceMesh(vertexes, faces);
            dxf.AddEntity(mesh);

            dxf.Save("mesh.dxf", DxfVersion.AutoCad2000);
        }
        private static void ReadDxfFile()
        {
            DxfDocument dxf = new DxfDocument();
            //dxf.Load("AutoCad2007.dxf");
            //dxf.Load("AutoCad2004.dxf");
            //dxf.Load("AutoCad2000.dxf");
            //dxf.Save("AutoCad2000 result.dxf", DxfVersion.AutoCad2000);
            dxf.Load("AutoCad12.dxf");
            //dxf.Load("Tablet.dxf");

            //dxf.Save("Tablet result.dxf", DxfVersion.AutoCad2000);
        }
        private static void WriteDxfFile()
        {
            DxfDocument dxf = new DxfDocument();

            //arc
            Arc arc = new Arc(new Vector3(10, 10, 0), 10, 45, 135);
            arc.Layer = new Layer("arc");
            arc.Layer.Color.Index = 1;
            dxf.AddEntity(arc);

            //xData sample
            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, 0));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            XData xdata2 = new XData(new ApplicationRegistry("other application"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata2.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "string record"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Long, 350));
            xdata2.XDataRecord.Add(XDataRecord.CloseControlString);

            //circle
            Vector3 extrusion = new Vector3(1, 1, 1);
            Vector3 centerWCS = new Vector3(1, 1, 1);
            Vector3 centerOCS = MathHelper.Transform(centerWCS,
                                                      extrusion,
                                                      MathHelper.CoordinateSystem.World,
                                                      MathHelper.CoordinateSystem.Object);

            Circle circle = new Circle(centerOCS, 5);
            circle.Layer = new Layer("circle with spaces");
            circle.Layer.Color=AciColor.Yellow;
            circle.LineType = LineType.Dashed;
            circle.Normal = extrusion;
            circle.XData=new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata.ApplicationRegistry, xdata},
                                 {xdata2.ApplicationRegistry, xdata2}
                             };

            dxf.AddEntity(circle);

            //points
            Point point1 = new Point(new Vector3(-3, -3, 0));
            point1.Layer = new Layer("point");
            point1.Color = new AciColor(30);
            Point point2 = new Point(new Vector3(1, 1, 1));
            point2.Layer = point1.Layer;
            point2.Layer.Color.Index = 9;
            point2.Normal = new Vector3(1, 1, 1);
            dxf.AddEntity(point1);
            dxf.AddEntity(point2);

            //3dface
            Face3d face3D = new Face3d(new Vector3(-5, -5, 5),
                                       new Vector3(5, -5, 5),
                                       new Vector3(5, 5, 5),
                                       new Vector3(-5, 5, 5));
            face3D.Layer = new Layer("3dface");
            face3D.Layer.Color.Index = 3;
            dxf.AddEntity(face3D);
            
            //polyline
            LwPolylineVertex polyVertex;
            List<LwPolylineVertex> polyVertexes = new List<LwPolylineVertex>();
            polyVertex = new LwPolylineVertex(new Vector2(-50, -50));
            polyVertex.BeginWidth = 2;
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(50, -50));
            polyVertex.BeginWidth = 1;
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(50, 50));
            polyVertex.Bulge = 1;
            polyVertexes.Add(polyVertex);
            polyVertex = new LwPolylineVertex(new Vector2(-50, 50));
            polyVertexes.Add(polyVertex);
            LwPolyline polyline2d = new LwPolyline(polyVertexes, true);
            polyline2d.Layer = new Layer("polyline2d");
            polyline2d.Layer.Color.Index = 5;
            polyline2d.Normal = new Vector3(1, 1, 1);
            polyline2d.Elevation = 100.0f;
            dxf.AddEntity(polyline2d);

            //lightweight polyline
            LwPolylineVertex lwVertex;
            List<LwPolylineVertex> lwVertexes = new List<LwPolylineVertex>();
            lwVertex = new LwPolylineVertex(new Vector2(-25, -25));
            lwVertex.BeginWidth = 2;
            lwVertexes.Add(lwVertex);
            lwVertex = new LwPolylineVertex(new Vector2(25, -25));
            lwVertex.BeginWidth = 1;
            lwVertexes.Add(lwVertex);
            lwVertex = new LwPolylineVertex(new Vector2(25, 25));
            lwVertex.Bulge = 1;
            lwVertexes.Add(lwVertex);
            lwVertex = new LwPolylineVertex(new Vector2(-25, 25));
            lwVertexes.Add(lwVertex);
            LwPolyline lwPolyline = new LwPolyline(lwVertexes, true);
            lwPolyline.Layer = new Layer("lwpolyline");
            lwPolyline.Layer.Color.Index = 5;
            lwPolyline.Normal = new Vector3(1, 1, 1);
            lwPolyline.Elevation = 100.0f;
            dxf.AddEntity(lwPolyline);

            // polyfaceMesh
            List<PolyfaceMeshVertex> meshVertexes = new List<PolyfaceMeshVertex>
                                                    {
                                                        new PolyfaceMeshVertex(0, 0, 0),
                                                        new PolyfaceMeshVertex(10, 0, 0),
                                                        new PolyfaceMeshVertex(10, 10, 0),
                                                        new PolyfaceMeshVertex(5, 15, 0),
                                                        new PolyfaceMeshVertex(0, 10, 0)
                                                    };
            List<PolyfaceMeshFace> faces = new List<PolyfaceMeshFace>
                                               {
                                                   new PolyfaceMeshFace(new[] {1, 2, -3}),
                                                   new PolyfaceMeshFace(new[] {-1, 3, -4}),
                                                   new PolyfaceMeshFace(new[] {-1, 4, 5})
                                               };

            PolyfaceMesh mesh = new PolyfaceMesh(meshVertexes, faces);
            dxf.AddEntity(mesh);

            //line
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            line.Layer = new Layer("line");
            line.Layer.Color.Index = 6;
            dxf.AddEntity(line);

            //3d polyline
            PolylineVertex vertex;
            List<PolylineVertex> vertexes = new List<PolylineVertex>();
            vertex = new PolylineVertex(new Vector3(-50, -50, 0));
            vertexes.Add(vertex);
            vertex = new PolylineVertex(new Vector3(50, -50, 10));
            vertexes.Add(vertex);
            vertex = new PolylineVertex(new Vector3(50, 50, 25));
            vertexes.Add(vertex);
            vertex = new PolylineVertex(new Vector3(-50, 50, 50));
            vertexes.Add(vertex);
            Polyline polyline = new Polyline(vertexes, true);
            polyline.Layer = new Layer("polyline3d");
            polyline.Layer.Color.Index = 24;
            dxf.AddEntity(polyline);

            //block definition
            Block block = new Block("TestBlock");
            block.Entities.Add(new Line(new Vector3(-5, -5, 5), new Vector3(5, 5, 5)));
            block.Entities.Add(new Line(new Vector3(5, -5, 5), new Vector3(-5, 5, 5)));
           
            //insert
            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");
            insert.Layer.Color.Index = 4;
            dxf.AddEntity(insert);

            //text
            TextStyle style=new TextStyle("True type font","Arial.ttf");
            Text text = new Text("Hello world!", Vector3.Zero, 10.0f,style);
            text.Layer = new Layer("text");
            text.Layer.Color.Index = 8;
            text.Alignment = TextAlignment.TopRight;
            dxf.AddEntity(text);
            
            //dxf.Save("AutoCad2010.dxf", DxfVersion.AutoCad2010);
            //dxf.Save("AutoCad2007.dxf", DxfVersion.AutoCad2007);
            dxf.Save("AutoCad2004.dxf", DxfVersion.AutoCad2004);
            dxf.Save("AutoCad2000.dxf", DxfVersion.AutoCad2000);

           // dxf.Load("AutoCad2000.dxf");
            //dxf.Save("AutoCad2000 result.dxf", DxfVersion.AutoCad2000);
        }
        private static void WritePolyline3d()
        {
            DxfDocument dxf = new DxfDocument();

            List<PolylineVertex> vertexes = new List<PolylineVertex>{
                                                                        new PolylineVertex(0, 0, 0), 
                                                                        new PolylineVertex(10, 0, 10), 
                                                                        new PolylineVertex(10, 10, 20), 
                                                                        new PolylineVertex(0, 10, 30)
                                                                        };

            Polyline poly = new Polyline(vertexes, true);

            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "netDxf polyline3d"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Integer, poly.Vertexes.Count));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            poly.XData = new Dictionary<ApplicationRegistry, XData>
                             {
                                 {xdata.ApplicationRegistry, xdata},
                             }; 
            dxf.AddEntity(poly);

            dxf.Save("polyline.dxf", DxfVersion.AutoCad2000);

            
        }
    }
}