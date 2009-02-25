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
using System.Diagnostics;
using netDxf;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;

namespace TestDxfDocument
{
  /// <summary>
    /// This is just a simple test of work in progress for the netDxf Library.
    /// </summary>
    internal class Program
    {
        private static void Main()
        {
            
           WriteDxfFile();
            //BlockAttributes();
            //WritePolyfaceMesh();
            //ReadDxfFile();
            //Ellipse();
            //Solid();
            //Polyline();
        }

        private static void Polyline()
        {

            DxfDocument dxf = new DxfDocument();

            Polyline poly = new Polyline();
            poly.Vertexes.Add(new PolylineVertex(0,0));
            poly.Vertexes.Add(new PolylineVertex(10, 10));
            poly.Vertexes.Add(new PolylineVertex(20, 0));
            poly.Vertexes.Add(new PolylineVertex(30, 10));

            dxf.AddEntity(poly);

            dxf.Save("polyline.dxf", DxfVersion.AutoCad12);

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

            dxf.Save("solid.dxf", DxfVersion.AutoCad12);
            dxf.Load("solid.dxf");
            //dxf.Save("solid.dxf");

        }

        private static void Ellipse()
        {
           
            DxfDocument dxf = new DxfDocument();


            Line line = new Line(new Vector3(0, 0, 0), new Vector3((float)(2 * Math.Cos(Math.PI / 4)), (float)(2 * Math.Cos(Math.PI / 4)), 0));
            dxf.AddEntity(line);

            Line line2 = new Line(new Vector3(0, 0, 0), new Vector3(0, -2, 0));
            dxf.AddEntity(line2);

            Arc arc=new Arc(Vector3.Zero,2,45,270);
            dxf.AddEntity(arc);

            // ellipses are saved as polylines
            Ellipse ellipse = new Ellipse();
            dxf.AddEntity(ellipse);


            dxf.Save("ellipse.dxf", DxfVersion.AutoCad12);
           
        }
        private static void SpeedTest()
        {
            Stopwatch crono = new Stopwatch();
            float totalTime=0;

            crono.Start();
            DxfDocument dxf = new DxfDocument();
            /// create 100,000 lines
            for (int i=0; i<100000;i++)
            {
                 //line
                Line line = new Line(new Vector3(0, i, 0), new Vector3(5, i, 0));
                line.Layer = new Layer("line");
                line.Layer.Color.Index = 6;
                dxf.AddEntity(line);
            }

            Ellipse ellipse = new Ellipse();
            dxf.AddEntity(ellipse);

            Console.WriteLine("Time creating entities : " + crono.ElapsedMilliseconds / 1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Reset();

            crono.Start();
            dxf.Save("speedtest.dxf", DxfVersion.AutoCad12);
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

            XData xdata = new XData("netDxf");
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, 0));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, 0));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);
            insert.XData.Add(xdata);

            dxf.AddEntity(insert);

            Circle circle = new Circle(Vector3.Zero, 5);
            circle.Layer = new Layer("circle");
            circle.Layer.Color.Index = 2;
            circle.XData.Add(xdata);
            circle.XData.Add(xdata);
            dxf.AddEntity(circle);

            dxf.Save("Test.dxf", DxfVersion.AutoCad12);
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

            dxf.Save("Test.dxf", DxfVersion.AutoCad12);
        }

        private static void ReadDxfFile()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.Load("Test.dxf");
            //dxf.Save("Test.dxf", DxfVersion.AutoCad12);
        }

        private static void WriteDxfFile()
        {
            DxfDocument dxf = new DxfDocument();

            //arc
            Arc arc = new Arc(new Vector3(10, 10, 0), 10, 45, 135);
            arc.Layer = new Layer("arc");
            arc.Layer.Color.Index = 1;
            dxf.AddEntity(arc);

            //circle
            Vector3 extrusion = new Vector3(1, 1, 1);
            Vector3 centerWCS = new Vector3(1, 1, 1);
            Vector3 centerOCS = MathHelper.Transform(centerWCS, extrusion,
                                                      MathHelper.CoordinateSystem.World,
                                                      MathHelper.CoordinateSystem.Object);

            Circle circle = new Circle(centerOCS, 5);
            circle.Layer = new Layer("circle");
            circle.Layer.Color.Index = 2;
            circle.Normal = extrusion;
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
            Face3D face3D = new Face3D(new Vector3(-5, -5, 5),
                                       new Vector3(5, -5, 5),
                                       new Vector3(5, 5, 5),
                                       new Vector3(-5, 5, 5));
            face3D.Layer = new Layer("3dface");
            face3D.Layer.Color.Index = 3;
            dxf.AddEntity(face3D);

            //block definition
            Block block = new Block("TestBlock");
            block.Entities.Add(new Line(new Vector3(-5, -5, 5), new Vector3(5, 5, 5)));
            block.Entities.Add(new Line(new Vector3(5, -5, 5), new Vector3(-5, 5, 5)));
            //insert
            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");
            insert.Layer.Color.Index = 4;
            dxf.AddEntity(insert);

            //polyline
            PolylineVertex lwVertex;
            List<PolylineVertex> lwVertexes = new List<PolylineVertex>();
            lwVertex = new PolylineVertex(new Vector2(-50, -50));
            lwVertex.BeginThickness = 2;
            lwVertexes.Add(lwVertex);
            lwVertex = new PolylineVertex(new Vector2(50, -50));
            lwVertex.BeginThickness = 1;
            lwVertexes.Add(lwVertex);
            lwVertex = new PolylineVertex(new Vector2(50, 50));
            lwVertex.Bulge = 1;
            lwVertexes.Add(lwVertex);
            lwVertex = new PolylineVertex(new Vector2(-50, 50));
            lwVertexes.Add(lwVertex);
            Polyline lwpolyline = new Polyline(lwVertexes, true);
            lwpolyline.Layer = new Layer("lwpolyline");
            lwpolyline.Layer.Color.Index = 5;
            lwpolyline.Normal = new Vector3(1, 1, 1);
            lwpolyline.Elevation = 100.0f;
            dxf.AddEntity(lwpolyline);

            //line
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            line.Layer = new Layer("line");
            line.Layer.Color.Index = 6;
            dxf.AddEntity(line);

            //3d polyline
            Polyline3dVertex vertex;
            List<Polyline3dVertex> vertexes = new List<Polyline3dVertex>();
            vertex = new Polyline3dVertex(new Vector3(-50, -50, 0));
            vertexes.Add(vertex);
            vertex = new Polyline3dVertex(new Vector3(50, -50, 10));
            vertexes.Add(vertex);
            vertex = new Polyline3dVertex(new Vector3(50, 50, 25));
            vertexes.Add(vertex);
            vertex = new Polyline3dVertex(new Vector3(-50, 50, 50));
            vertexes.Add(vertex);
            Polyline3d polyline = new Polyline3d(vertexes, true);
            polyline.Layer = new Layer("polyline");
            polyline.Layer.Color.Index = 7;
            dxf.AddEntity(polyline);

            //text
            Text text = new Text("Hello world!", Vector3.Zero, 10.0f);
            text.Layer = new Layer("text");
            text.Layer.Color.Index = 8;
            text.Alignment = TextAlignment.TopRight;
            dxf.AddEntity(text);

            dxf.Save("Test.dxf", DxfVersion.AutoCad12);
        }
    }
}