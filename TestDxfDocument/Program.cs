using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using GTE = netDxf.GTE;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using FontStyle = netDxf.Tables.FontStyle;
using Image = netDxf.Entities.Image;
using Point = netDxf.Entities.Point;
using Trace = netDxf.Entities.Trace;
using Vector2 = netDxf.Vector2;
using Vector3 = netDxf.Vector3;

namespace TestDxfDocument
{
    /// <summary>
    /// This is just a simple test of work in progress for the netDxf library.
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            DxfDocument doc = Test(@"sample.dxf"); 

            #region Samples for GTE classes

            //SplineToPolylineWithGTE();
            //TextAlongAPath();
            //LengthOfASpline();
            //SplineControlsReduction();
            //SplineCurveFitSampleData();
            //SplineSurfaceFitSampleData();
            //NURBSCurve();
            //NURBSSurface();

            #endregion

            #region Samples for new and modified features 3.0.0

            //ReflectionMatrix();
            //PolygonMesh();
            //UcsTransform();
            //SmoothPolyline2D();
            //SmoothPolyline3D();
            //SplitBezierCurve();
            //FitBezierCurve();
            //BezierCurve();
            //PolyfaceMesh();
            //AssignAnnotationToLeader();
            //CreateImageDefinition();

            #endregion

            #region Samples for new and modified features 2.4.1

            //LayerStateManager();

            #endregion

            #region Samples for new and modified features 2.3.0

            //ExplodeInsert();
            //TransformArc();
            //TransformCircle();
            //TransformLwPolyline();
            //TransformEllipse();
            //AddHeaderVariable();
            //MLineMirrorAndExplode();
            //ShapeMirror();
            //TextMirror();
            //MTextMirror();
            //InsertMirror();
            //LeaderMirror();

            #endregion

            #region Samples for new and modified features 2.2.1

            //MTextParagraphFormatting();
            //MTextCharacterFormatting();

            #endregion

            #region Samples for new and modified features 2.2.0

            //AccessModelBlock();
            //DimensionBlockGeneration();

            #endregion

            #region Samples for new and modified features 2.1.0

            //Shape();
            //ComplexLineType();
            //TextStyle();
            //ReadWriteFromStream();
            //ReadWriteFromStream2();

            #endregion

            #region Samples for new and modified features 2.0.1

            //DimensionUserTextWithTwoLines();

            #endregion

            #region Samples for new and modified features 2.0

            //Polyline3dAddVertex();
            //AcadTable();
            //DimensionStyleOverrides();
            //ResetLeaderAnnotationPosition();

            #endregion

            #region Samples for new and modified features 1.1.2

            //LinearDimensionTest();
            //AlignedDimensionTest();
            //Angular2LineDimensionTest();
            //Angular3PointDimensionTest();
            //DiametricDimensionTest();
            //RadialDimensionTest();
            //OrdinateDimensionTest();

            #endregion

            #region Samples for new and modified features 1.1.0

            //WipeoutEntity();
            //ToleranceEntity();
            //LeaderEntity();
            //UnderlayEntity();
            //SplineFitPoints();
            //ImageClippingBoundary();

            #endregion

            #region Samples for new and modified features 1.0.2

            //AssociativeHatches();
            //TraceEntity();
            //SolidEntity();

            #endregion

            #region Samples for new and modified features 1.0.0

            //ModifyingDocumentEntities();
            //ModifyingBlockProperties();
            //ModifyingMLineStyles();
            //DimensionsLinearAndAngularUnits();
            //ModifyingDimensionGeometryAndStyle();
            //ModifyingGroups();
            //ModifyingXData();
            //DimensionUserText();

            #endregion

            #region Samples for fixes, new and modified features 0.9.3

            //RemoveBlock();
            //LinearDimension();
            //AlignedDimension();
            //Angular2LineDimension();
            //Angular3PointDimension();
            //DiametricDimension();
            //RadialDimension();
            //OrdinateDimension();

            #endregion

            #region Samples for fixes, new and modified features 0.9.2

            //NurbsEvaluator();
            //XDataInformation();
            //DynamicBlocks();

            #endregion

            #region Samples for fixes, new and modified features 0.9.1

            //LoadAndSaveBlocks();

            #endregion

            #region Samples for fixes, new and modified features 0.9.0

            //MakingGroups();
            //CombiningTwoDrawings();
            //BinaryChunkXData();
            //BinaryDxfFiles();
            //MeshEntity();

            #endregion

            #region Samples for new and modified features 0.8.0

            //MTextEntity();
            //TransparencySample();
            //DocumentUnits();
            //PaperSpace();
            //BlockWithAttributes();

            #endregion

            #region other

            //NestedBlock();
            //DimensionNestedBlock();
            //EncodingTest();
            //CheckReferences();
            //ComplexHatch();
            //RayAndXLine();
            //UserCoordinateSystems();
            //ExplodeInsert();
            //ImageUsesAndRemove();
            //LayerAndLinetypesUsesAndRemove();
            //TextAndDimensionStyleUsesAndRemove();
            //MLineStyleUsesAndRemove();
            //AppRegUsesAndRemove();
            //ExplodePolyfaceMesh(); 
            //ApplicationRegistries();
            //TestOCStoWCS();
            //WriteGradientPattern();
            //WriteGroup();
            //WriteMLine();
            //ObjectVisibility();
            //EntityTrueColor();
            //EntityLineWeight();
            //Text();
            //WriteNoAsciiText();
            //WriteSplineBoundaryHatch();
            //WriteNoInsertBlock();
            //WriteImage();
            //AddAndRemove();
            //LoadAndSave();
            //CleanDrawing();
            //OrdinateDimensionDrawing();
            //Angular2LineDimensionDrawing();
            //Angular3PointDimensionDrawing();
            //DiametricDimensionDrawing();
            //RadialDimensionDrawing();
            //LinearDimensionDrawing();
            //AlignedDimensionDrawing();
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
            //HatchTest1();
            //HatchTest2();
            //HatchTest3();
            //HatchTest4();
            //WriteNestedInsert();
            //WritePolyfaceMesh();
            //Ellipse();
            //Solid();
            //Face3d();
            //Polyline2D();
            //Polyline3D();
            //Dxf2000();
            //SpeedTest();
            //WritePolyline3d();
            //WriteInsert();

            #endregion
        }

        #region Samples for GTE classes

        public static void SplineToPolylineWithGTE()
        {
            short degree = 2;
            bool closed = true;
            
            List<Vector3> controls = new List<Vector3>
            {
                new Vector3(0,0,0), 
                new Vector3(10,10,0), 
                new Vector3(20,0,0), 
                new Vector3(30,10,0), 
                new Vector3(40,0,0)
            };

            // periodic splines does not seems to be handled automatically in the Geometric Tools Library
            // we need to change manually the knot vector of the input and wrap around the controls 
            if (closed)
            {
                for (int i = 0; i < degree; i++)
                {
                    controls.Add(controls[i]);
                }
            }

            GTE.BasisFunctionInput input = new GTE.BasisFunctionInput(controls.Count, degree);
            GTE.BSplineCurve curve = new GTE.BSplineCurve(input, controls.ToArray());
           
            // change the knot vector to handle periodic splines
            if (closed)
            {
                double factor = 1.0 / (controls.Count - curve.BasisFunction.Degree);
                for (int i = 0; i < curve.BasisFunction.NumKnots; i++)
                {
                    curve.BasisFunction.Knots[i] = (i - curve.BasisFunction.Degree) * factor;
                }
            }
            
            int precision =  100;
            double step = closed ? 1.0 / precision : 1.0 / (precision - 1);
            Vector3[] vertexes = new Vector3[precision];
            for (int i = 0; i < precision; i++)
            {
                vertexes[i] = curve.GetPosition(i * step);
            }

            Polyline3D polylineCurve = new Polyline3D(vertexes)
            {
                IsClosed = closed,
                Color = AciColor.Blue
            };

            Vector3[] polVerts;
            if (closed)
            {
                polVerts = new Vector3[controls.Count - curve.BasisFunction.Degree];
                Array.Copy(controls.ToArray(), polVerts, controls.Count - curve.BasisFunction.Degree);
            }
            else
            {
                polVerts = new Vector3[controls.Count];
                Array.Copy(controls.ToArray(), polVerts, controls.Count);
            }

            Polyline3D polyline = new Polyline3D(polVerts)
            {
                IsClosed = closed,
                Color = AciColor.Red
            };
            if (degree == 2)
            {
                polyline.SmoothType = PolylineSmoothType.Quadratic;
            }

            if (degree == 3)
            {
                polyline.SmoothType = PolylineSmoothType.Cubic;
            }

            Spline spline = new Spline(polVerts, null, degree, closed) {Color = AciColor.Yellow};

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(polylineCurve);
            doc.Entities.Add(polyline);
            doc.Entities.Add(spline);
            doc.Save("test.dxf");
        }

        public static void TextAlongAPath()
        {
            string sampleText = "How to distribute a text along a path?";
            short degree = 3;
            Vector3[] controls =
            {
                new Vector3(0,0,0), 
                new Vector3(10,40,0), 
                new Vector3(20,0,0), 
                new Vector3(30,-10,0), 
                new Vector3(40,20,0),
                new Vector3(50,-10,0),
                new Vector3(60,0,0),
                new Vector3(70,40,0),
                new Vector3(80,0,0)
            };

            GTE.BasisFunctionInput input = new GTE.BasisFunctionInput(controls.Length, degree);
            GTE.BSplineCurve curve = new GTE.BSplineCurve(input, controls);

            Vector3[] positions = curve.SubdivideByLength(sampleText.Length, out double[] tParameters);
            Vector3[] tangents = new Vector3[positions.Length];

            for (int i = 0; i < tParameters.Length; i++)
            {
                tangents[i] = curve.GetTangent(tParameters[i]);
            }

            Spline spline = new Spline(controls, null, degree){Color = AciColor.Blue};

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(spline);

            for (int i = 0; i < sampleText.Length; i++)
            {
                Text character = new Text(sampleText[i].ToString())
                {
                    Position = positions[i],
                    Rotation = MathHelper.RadToDeg * Vector2.Angle(new Vector2(tangents[i].X, tangents[i].Y)),
                    Height = 2.0,
                    Alignment = TextAlignment.BaselineCenter,
                    Color = AciColor.Yellow
                };
                doc.Entities.Add(character);
            }

            doc.Save("test.dxf");
        }

        public static void LengthOfASpline()
        {
            short degree = 2;

            Vector3[] controls =
            {
                new Vector3(0,0,0), 
                new Vector3(10,40,0), 
                new Vector3(20,0,0), 
                new Vector3(30,-10,0), 
                new Vector3(40,20,0),
                new Vector3(50,-10,0),
                new Vector3(60,0,0),
                new Vector3(70,40,0),
                new Vector3(80,0,0)
            };

            GTE.BasisFunctionInput input = new GTE.BasisFunctionInput(controls.Length, degree);
            GTE.BSplineCurve curve = new GTE.BSplineCurve(input, controls);
            double totalLength = curve.GetTotalLength();


            int precision = 100;
            double step = 1.0 / (precision - 1);
            double approxLengthBySubdivision = 0.0;
            Vector3[] vertexes = new Vector3[precision];
            Vector3 prevVertex = curve.GetPosition(0);
            for (int i = 1; i < precision; i++)
            {
                Vector3 vertex = curve.GetPosition(i * step);
                approxLengthBySubdivision += Vector3.Distance(prevVertex, vertex);
                prevVertex = vertex;
                vertexes[i] = vertex;
            }

            double diffSum = totalLength - approxLengthBySubdivision;

            Polyline3D polyline = new Polyline3D(vertexes)
            {
                Color = AciColor.Blue
            };

            Spline spline = new Spline(controls, null, degree)
            {
                Color = AciColor.Yellow
            };

            DxfDocument doc = new DxfDocument();

            doc.Entities.Add(spline);
            doc.Entities.Add(polyline);

            doc.Save("test.dxf");
        }
        
        public static void SplineControlsReduction()
        {
            short degree = 3;
            double radius = 20.0;
            Vector3[] sampleData = new Vector3[100];

            double step = Math.PI / (sampleData.Length - 1);
            for (int i = 0; i < sampleData.Length; i++)
            {
                double angle = i * step;
                double sin = radius * Math.Sin(angle);
                double cos =  radius * Math.Cos(angle);
                sampleData[i] = new Vector3(cos, sin, 0.0);
            }

            Spline splineFull = new Spline(sampleData, null, degree)
            {
                Color = AciColor.Blue
            };

            // the control points will be reduced to a 10% of the original sampleData
            double fraction = 0.1;
            GTE.BSplineReduction reduceBSpline = new GTE.BSplineReduction(sampleData, degree, fraction);
            Spline splineReduced = new Spline(reduceBSpline.ControlData, null, degree)
            {
                Color = AciColor.Red
            };

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(splineFull);
            doc.Entities.Add(splineReduced);
            doc.Save("test.dxf");
        }

        public static void SplineCurveFitSampleData()
        {
            short degree = 3;
            double radius = 20.0;
            Vector3[] sampleData = new Vector3[100];
            Random randomDesviation = new Random();

            double step = Math.PI / (sampleData.Length - 1);
            for (int i = 0; i < sampleData.Length; i++)
            {
                double angle = i * step;
                double desviation = randomDesviation.Next(-2, 2);
                double sin = (radius + desviation) * Math.Sin(angle);
                double cos = (radius + desviation) * Math.Cos(angle);
                sampleData[i] = new Vector3(cos, sin, 0.0);
            }

            Spline splineFull = new Spline(sampleData, null, degree)
            {
                Color = AciColor.Blue
            };

            // will use 10 controls to approximate a curve that averages the points of the sample data
            int numOutControls = 10;
            GTE.BSplineCurveFit curveFit = new GTE.BSplineCurveFit(sampleData, degree, numOutControls);
            Spline splineFit = new Spline(curveFit.ControlData, null, degree)
            {
                Color = AciColor.Red
            };

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(splineFull);
            doc.Entities.Add(splineFit);
            doc.Save("test.dxf");
        }

        public static void SplineSurfaceFitSampleData()
        {
            int degree = 2;
            short u = 20;
            double stepU = 10.0;
            short v = 10;
            double stepV = 10.0;
            Random deviation = new Random();
            Vector3[] sampleData = new Vector3[u * v];
            for (int i = 0; i < v; i++)
            {
                for (int j = 0; j < u; j++)
                {
                    sampleData[i * u + j] = new Vector3(j * stepU, i * stepV, deviation.Next(-1, 1) * deviation.NextDouble() * 10);
                }
            }

            GTE.BSplineSurfaceFit surfaceFit = new GTE.BSplineSurfaceFit(degree, 10, u, degree, 10, v, sampleData);

            PolygonMesh controlMesh = new PolygonMesh(u, v, sampleData)
            {
                Color = AciColor.Blue
            };

            PolygonMesh pMesh = new PolygonMesh(10, 10, surfaceFit.ControlData)
            {
                Color = AciColor.Red,
                SmoothType = PolylineSmoothType.Quadratic
            };

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(controlMesh);
            doc.Entities.Add(pMesh);
            doc.Save("test.dxf");

        }

        public static void NURBSCurve()
        {
            short degree = 2;
            
            List<Vector3> controls = new List<Vector3>
            {
                new Vector3(0,0,0), 
                new Vector3(10,10,0), 
                new Vector3(20,0,0), 
                new Vector3(30,10,0), 
                new Vector3(40,0,0)
            };

            double[] weights = new double[controls.Count];
            double weigth = 0.1;
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = weigth;
                weigth *= 5;
            }

            GTE.BasisFunctionInput input = new GTE.BasisFunctionInput(controls.Count, degree);
            GTE.NURBSCurve curve = new GTE.NURBSCurve(input, controls.ToArray(), weights);

            int precision = 100;
            double step = 1.0 / (precision - 1.0);
            Vector3[] vertexes = new Vector3[precision];
            for (int i = 0; i < precision; i++)
            {
                vertexes[i] = curve.GetPosition(i * step);
            }

            Spline spline = new Spline(controls, weights, degree) {Color = AciColor.Blue};
            Polyline3D polyline = new Polyline3D(vertexes){Color = AciColor.Red};

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(spline);
            doc.Entities.Add(polyline);
            doc.Save("test.dxf");

        }

        public static void NURBSSurface()
        {
            // number of vertexes along the mesh local X axis
            short u = 6;
            // number of vertexes along the mesh local Y axis
            short v = 4;

            // array of vertexes
            Vector3[] controls = new Vector3[u * v];
            // first row (local X axis)
            controls[0] = new Vector3(0, 0, 0);
            controls[1] = new Vector3(10, 0, 10);
            controls[2] = new Vector3(20, 0, 0);
            controls[3] = new Vector3(30, 0, 10);
            controls[4] = new Vector3(40, 0, 0);
            controls[5] = new Vector3(50, 0, 10);

            // second row (local X axis)
            controls[6] = new Vector3(0, 10, 10);
            controls[7] = new Vector3(10, 10, 0);
            controls[8] = new Vector3(20, 10, 10);
            controls[9] = new Vector3(30, 10, 0);
            controls[10] = new Vector3(40, 10, 10);
            controls[11] = new Vector3(50, 10, 0);

            // third row (local X axis)
            controls[12] = new Vector3(0, 20, 0);
            controls[13] = new Vector3(10, 20, 10);
            controls[14] = new Vector3(20, 20, 0);
            controls[15] = new Vector3(30, 20, 10);
            controls[16] = new Vector3(40, 20, 0);
            controls[17] = new Vector3(50, 20, 10);

            // fourth row (local X axis)
            controls[18] = new Vector3(0, 30, 10);
            controls[19] = new Vector3(10, 30, 0);
            controls[20] = new Vector3(20, 30, 10);
            controls[21] = new Vector3(30, 30, 0);
            controls[22] = new Vector3(40, 30, 10);
            controls[23] = new Vector3(50, 30, 0);

            // array of weights
            double[] weights = new double[controls.Length];
            for (int i = 0; i < weights.Length; i++)
            {
                weights[i] = 1.0;
            }

            GTE.BasisFunctionInput input0 = new GTE.BasisFunctionInput(u, 3);
            GTE.BasisFunctionInput input1 = new GTE.BasisFunctionInput(v, 3);
            GTE.NURBSSurface surface = new GTE.NURBSSurface(input0, input1, controls, weights);


            short precisionU = (short) (10 * u);
            short precisionV = (short) (10 * v);
            double stepU = 1.0 / (precisionU - 1);
            double stepV = 1.0 / (precisionV - 1);

            Vector3[] vertexes = new Vector3[precisionU * precisionV];
            for (int i = 0; i < precisionV; i++)
            {
                for (int j = 0; j < precisionU; j++)
                {
                    vertexes[i * precisionU + j] = surface.GetPosition( j * stepU, i * stepV);
                }
            }

            PolygonMesh controlMesh = new PolygonMesh(u, v, controls) {Color = AciColor.Blue};

            Vector3 test1 = controlMesh.GetVertex(1, 2);
            Vector3 test2 = surface.GetControl(1, 2);


            PolygonMesh pMesh = new PolygonMesh(precisionU, precisionV, vertexes) {Color = AciColor.Red};

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(controlMesh);
            doc.Entities.Add(pMesh);
            doc.Save("test.dxf");
        }

        #endregion

        #region Samples for new and modified features 3.0.0

        public static void ReflectionMatrix()
        {
            Polyline2D polyline = new Polyline2D(new []{
                new Polyline2DVertex(-5,-5), 
                new Polyline2DVertex(-5,5), 
                new Polyline2DVertex(0,0),
                new Polyline2DVertex(5,5), 
                new Polyline2DVertex(5,-5)}, true) {Color = AciColor.Blue};

            polyline.TransformBy(Matrix3.Identity, new Vector3(20,20,0));

            Line mirrorLine = new Line(new Vector3(20,5,0), new Vector3(0,15,0)) {Color = AciColor.Yellow};
            Vector3 mirrorNormal = Vector3.CrossProduct(mirrorLine.Direction, Vector3.UnitZ);

            Polyline2D reflection = (Polyline2D) polyline.Clone();
            reflection.Color = AciColor.Red;

            // reflection matrix of a mirror plane given its normal and a point on the plane
            Matrix4 reflectionMatrix = Matrix4.Reflection(mirrorNormal, mirrorLine.StartPoint);

            // for a mirror plane that passes through the origin, you can also use
            //Matrix3 reflectionMatrix = Matrix3.Reflection(mirrorNormal);
            reflection.TransformBy(reflectionMatrix);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(polyline);
            doc.Entities.Add(mirrorLine);
            doc.Entities.Add(reflection);

            doc.Save("test.dxf");
        }

        public static void PolygonMesh()
        {
            short u = 4;
            short v = 4;

            //Vector3[] controls =
            //{
            //    new Vector3(0, 0, 0),
            //    new Vector3(10, 0, 10),
            //    new Vector3(20, 0, 10),
            //    new Vector3(30, 0, 0),

            //    new Vector3(0, 10, 0),
            //    new Vector3(10, 10, 10),
            //    new Vector3(20, 10, 10),
            //    new Vector3(30, 10, 0),

            //    new Vector3(0, 20, 0),
            //    new Vector3(10, 20, 10),
            //    new Vector3(20, 20, 10),
            //    new Vector3(30, 20, 0),

            //    new Vector3(0, 30, 0),
            //    new Vector3(10, 30, 10),
            //    new Vector3(20, 30, 10),
            //    new Vector3(30, 30, 0),
            //};

            //Vector3[] controls =
            //{
            //    new Vector3(0, 0, 0),
            //    new Vector3(10, 0, 0),
            //    new Vector3(20, 0, 0),
            //    new Vector3(30, 0, 0),

            //    new Vector3(0, 10, 10),
            //    new Vector3(10, 10, 10),
            //    new Vector3(20, 10, 10),
            //    new Vector3(30, 10, 10),

            //    new Vector3(0, 20, 10),
            //    new Vector3(10, 20, 10),
            //    new Vector3(20, 20, 10),
            //    new Vector3(30, 20, 10),

            //    new Vector3(0, 30, 0),
            //    new Vector3(10, 30, 0),
            //    new Vector3(20, 30, 0),
            //    new Vector3(30, 30, 0),
            //};

            Vector3[] controls =
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 10),
                new Vector3(20, 0, 10),
                new Vector3(30, 0, 0),

                new Vector3(0, 10, 10),
                new Vector3(10, 10, 10),
                new Vector3(20, 10, 10),
                new Vector3(30, 10, 10),

                new Vector3(0, 20, 10),
                new Vector3(10, 20, 10),
                new Vector3(20, 20, 10),
                new Vector3(30, 20, 10),

                new Vector3(0, 30, 0),
                new Vector3(10, 30, 10),
                new Vector3(20, 30, 10),
                new Vector3(30, 30, 0),
            };

            //// number of vertexes along the mesh local X axis
            //short u = 6;
            //// number of vertexes along the mesh local Y axis
            //short v = 4;

            //// array of vertexes
            //Vector3[] vertexes = new Vector3[u * v];
            //// first row (local X axis)
            //vertexes[0] = new Vector3(0,0,0);
            //vertexes[1] = new Vector3(10,0,10);
            //vertexes[2] = new Vector3(20,0,0);
            //vertexes[3] = new Vector3(30,0,10);
            //vertexes[4] = new Vector3(40,0,0);
            //vertexes[5] = new Vector3(50,0,10);

            //// second row (local X axis)
            //vertexes[6] = new Vector3(0,10,10);
            //vertexes[7] = new Vector3(10,10,0);
            //vertexes[8] = new Vector3(20,10,10);
            //vertexes[9] = new Vector3(30,10,0);
            //vertexes[10] = new Vector3(40,10,10);
            //vertexes[11] = new Vector3(50,10,0);

            //// third row (local X axis)
            //vertexes[12] = new Vector3(0,20,0);
            //vertexes[13] = new Vector3(10,20,10);
            //vertexes[14] = new Vector3(20,20,0);
            //vertexes[15] = new Vector3(30,20,10);
            //vertexes[16] = new Vector3(40,20,0);
            //vertexes[17] = new Vector3(50,20,10);

            //// fourth row (local X axis)
            //vertexes[18] = new Vector3(0,30,10);
            //vertexes[19] = new Vector3(10,30,0);
            //vertexes[20] = new Vector3(20,30,10);
            //vertexes[21] = new Vector3(30,30,0);
            //vertexes[22] = new Vector3(40,30,10);
            //vertexes[23] = new Vector3(50,30,0);

            PolygonMesh pMesh = new PolygonMesh(u, v, controls)
            {
                Color = AciColor.Blue,
                DensityU = (short) (5 * u),
                DensityV = (short) (5 * v),
                IsClosedInU = true,
                IsClosedInV = true,
                SmoothType = PolylineSmoothType.Quadratic
            };

            // the Mesh entity doesn't have the restrictions the PolygonMesh has
            // you can create smoothed polygon meshes with higher densities that the ones allowed by the polygon mesh
            Mesh mesh = pMesh.ToMesh(60 * u, 60 * v);
            mesh.Color = AciColor.Red;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(pMesh);
            doc.Entities.Add(mesh);
            doc.Save("test.dxf");
        }

        public static void UcsTransform()
        {
            // start and end point of a line expressed in local coordinates
            Vector3 start = new Vector3(5,2.5,0);
            Vector3 end = new Vector3(8, 6.5, 0);

            // local UCS
            //UCS ucs = new UCS("MyUCS", new Vector3(-2, -4, 1), Vector3.UnitX, Vector3.UnitZ);
            UCS ucs = UCS.FromNormal("MyUCS", new Vector3(-2, -4, 1), new Vector3(1), 30 * MathHelper.DegToRad);

            // but the Line entity is always expressed in world coordinates
            List<Vector3> wcsPoints = ucs.Transform(new List<Vector3> {start, end}, CoordinateSystem.Object, CoordinateSystem.World);
            Line line = new Line(wcsPoints[0], wcsPoints[1]);

            DxfDocument doc = new DxfDocument();

            // add the UCS to the document
            doc.UCSs.Add(ucs);
            // (optional) make ucs the current/active UCS of the drawing
            doc.DrawingVariables.CurrentUCS = ucs;

            // add the line to the document
            doc.Entities.Add(line);

            doc.Save("test.dxf");

        }

        public static void SmoothPolyline2D()
        {
            // polyline points
            List<Vector2> points = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0,5),
                new Vector2(5,2.5),
                new Vector2(10, 5),
                new Vector2(10, 0),
            };

            // create the polyline
            Polyline2D poly = new Polyline2D(points);

            // polyline smooth type
            // the resulting smoothed polyline is the same as a Spline of second (Quadratic) or third (Cubic) degree
            // it is recommended to use a Spline entity instead, if you are planning to smooth the polyline
            poly.SmoothType = PolylineSmoothType.Cubic;
            poly.SetConstantWidth(0.5);
            //poly.Elevation = 5;
            //poly.Normal = new Vector3(0,1,0);

            // closing the polyline will generate a closed periodic spline
            poly.IsClosed = true;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(poly);

            // for testing purposes
            // AutoCad uses the $SPLINESEGS header variable as a base number to generate the spline curve that represents the smoothed polyline
            // I do not know how exactly this variable is applied, at the moment, when the DXF file is saved,
            // the precision used to generate the vertexes that represent the smoothed polyline is
            // for open polylines: Precision = $SPLINESEGS * (Number of Vertices - 1),
            // for closed polylines: Precision = $SPLINESEGS * Number of Vertices.
            Polyline2D testPoly = new Polyline2D(poly.PolygonalVertexes(doc.DrawingVariables.SplineSegs * poly.Vertexes.Count))
            {
                IsClosed = poly.IsClosed,
                Color = AciColor.Yellow
            };
            //doc.Entities.Add(testPoly);

            doc.Save("test.dxf");

            DxfDocument dxf = DxfDocument.Load("test.dxf");
            dxf.Save("test.dxf");
        }

        public static void SmoothPolyline3D()
        {
            // polyline points
            List<Vector3> points = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(0,5,0),
                new Vector3(5,2.5,0),
                new Vector3(10, 5, 0),
                new Vector3(10, 0, 0),
            };

            // create the polyline
            Polyline3D poly = new Polyline3D(points);

            // polyline smooth type
            // the resulting smoothed polyline is the same as a Spline of second (Quadratic) or third (Cubic) degree
            // it is recommended to use a Spline entity instead, if you are planning to smooth the polyline
            poly.SmoothType = PolylineSmoothType.Quadratic;

            // closing the polyline will generate a closed periodic spline
            //poly.IsClosed = true;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(poly);

            // for testing purposes
            // AutoCad uses the $SPLINESEGS header variable as a base number to generate the spline curve that represents the smoothed polyline
            // I do not know how exactly this variable is applied, at the moment, when the DXF file is saved,
            // the precision used to generate the vertexes that represent the smoothed polyline is
            // For open polylines: Precision = $SPLINESEGS * (Number of Vertices - 1),
            // For closed polylines: Precision = $SPLINESEGS * Number of Vertices.
            Polyline3D testPoly = new Polyline3D(poly.PolygonalVertexes(doc.DrawingVariables.SplineSegs * poly.Vertexes.Count))
            {
                IsClosed = poly.IsClosed,
                Color = AciColor.Yellow
            };
            //doc.Entities.Add(testPoly);

            doc.Save("test.dxf");
        }

        public static void SplitBezierCurve()
        {
            // this same procedure can be applied to quadratic bezier curves

            // cubic bezier curve control points
            List<Vector3> points = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(5,2,5),
                new Vector3(7,5,-5),
                new Vector3(15, -10, 0)
            };

            // create the bezier curve
            BezierCurveCubic curveCubic = new BezierCurveCubic(points);

            // split the curve at parameter t = 0.25
            BezierCurveCubic[] splitCurves = curveCubic.Split(0.25);

            DxfDocument doc = new DxfDocument();

            doc.Entities.Add(new Spline(new[] {splitCurves[0]}));
            doc.Entities.Add(new Spline(new[] {splitCurves[1]}));

            // original curve for testing
            doc.Entities.Add(new Spline(new[] {curveCubic}) {Color = AciColor.Yellow});

            doc.Save("test.dxf");
        }

        public static void FitBezierCurve()
        {
            // list of point that the Spline must pass through
            List<Vector3> fitPoints = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(5, 5, 0),
                new Vector3(10, 0, 0),
                new Vector3(15, -5, 0),
                new Vector3(20, 0, 0)
            };

            // initializing a Spline from a set of fit points will create as set of cubic bezier curves that passes through those points
            // the resulting curves will be used to create the Spline (see the BezierCurve() sample).
            Spline spline = new Spline(fitPoints);
            //Spline spline = new Spline(curves);
            spline.Color = AciColor.Blue;
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(spline);

            // testing that the result is the same
            List<BezierCurveCubic> curves = BezierCurveCubic.CreateFromFitPoints(fitPoints);
            foreach (BezierCurveCubic curve in curves)
            {
                doc.Entities.Add(new Polyline3D(curve.PolygonalVertexes(10)) {Color = AciColor.Yellow});
            }

            doc.Save("test.dxf");

            DxfDocument dxf = DxfDocument.Load("test.dxf");
            dxf.Save("test.dxf");
        }

        public static void BezierCurve()
        {
            // this sample uses cubic bezier curves
            // this same procedure can be done with a quadratic bezier curves

            // cubic bezier control points (degree = number of control points - 1)
            List<Vector3> points1 = new List<Vector3>
            {
                new Vector3(0,0,0),
                new Vector3(5,5,0),
                new Vector3(10,5,0),
                new Vector3(15,0,0)
            };

            // cubic bezier control points (degree = number of control points - 1)
            List<Vector3> points2 = new List<Vector3>
            {
                new Vector3(15,0,0),
                new Vector3(20,-5,0),
                new Vector3(25,-5,0),
                new Vector3(30,0,0)
            };

            // create the cubic bezier curves
            BezierCurveCubic curve1 = new BezierCurveCubic(points1);
            BezierCurveCubic curve2 = new BezierCurveCubic(points2);
            
            // create a polyline from a bezier curve given a precision of 10
            Polyline3D polyline1 = new Polyline3D(curve1.PolygonalVertexes(10));
            polyline1.Color = AciColor.Blue;

            // create a polyline from a bezier curve given a precision of 10
            Polyline3D polyline2 = new Polyline3D(curve2.PolygonalVertexes(10));
            polyline2.Color = AciColor.Red;

            // create a Spline from a set of concatenated cubic bezier curves
            // the end point point of the previous curve should be the start point of the next
            Spline spline1 = new Spline(new []{curve1, curve2});
            spline1.Color = AciColor.Yellow;
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(spline1);
            doc.Entities.Add(polyline1);
            doc.Entities.Add(polyline2);
            doc.Save("test.dxf");
        }

        private static void PolyfaceMesh()
        {
            // list of vertices of the PolyfaceMesh
            List<Vector3> vertexes = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
                new Vector3(5, 15, 0),
                new Vector3(0, 10, 0)
            };

            // list of face indexes of the PolyfaceMesh
            List<short[]> faces = new List<short[]>
            {
                new short[] {1, 2, -3},
                new short[] {-1, 3, -4},
                new short[] {-1, 4, 5}
            };

            // directly initializing the faces of the PolyfaceMesh
            //List<PolyfaceMeshFace> faces = new List<PolyfaceMeshFace>
            //{
            //    new PolyfaceMeshFace(new short[] {1, 2, -3, 5}) {Color = AciColor.Blue},
            //    new PolyfaceMeshFace(new short[] {-5, 3, 4}) {Color = AciColor.Red}
            //};

            DxfDocument doc = new DxfDocument();
            Layer layer2 = doc.Layers.Add(new Layer("layer2")
            {
                Description = "One face of a polyface mesh.",
                Color = AciColor.Red
            });
            PolyfaceMesh mesh = new PolyfaceMesh(vertexes, faces);
            mesh.Faces[0].Layer = new Layer("layer1")
            {
                Description = "Another face of the polyface mesh",
                Color = AciColor.Blue
            };
            
            // assign a new layer to the face
            mesh.Faces[1].Layer = layer2; // assign an existing layer to the face
            //mesh.Faces[0].Color = AciColor.Yellow; // PolyfaceMesh faces can also have their own color
            //mesh.Faces[1].Color = AciColor.Green; // PolyfaceMesh faces can also have their own color
            doc.Entities.Add(mesh);

            doc.Save("test.dxf");

            // Test
            doc = DxfDocument.Load("test.dxf");
            doc.Save("test.dxf");
        }

        private static void AssignAnnotationToLeader()
        {
            // We will create a Leader entity creating the annotation manually and assigning to it.
            // This will be the same as:
             Leader original = new Leader("Sample annotation", new[] {new Vector2(0, 0), new Vector2(2.5, 2.5)});
            
            // Create a leader with no annotation
            Leader leader = new Leader(new[] {new Vector2(0, 0), new Vector2(2.5, 2.5)});
            //leader.Offset = new Vector2(0.1, 0.1);

            // Create the text that will become the leader annotation
            MText text = new MText("Sample annotation", new Vector2(5.0,5.0), .5) {Rotation = 0};
            text.AttachmentPoint = MTextAttachmentPoint.MiddleLeft;
            // Assign the text annotation to the leader
            leader.Annotation = text;
            // The next two code lines are the important ones, that use to be automatically applied but now it has to be done manually.
            // When assigning a text annotation to a leader it will not automatically add a hook line to it.
            // Usually Text and MText annotations have hook lines, but Insert and Tolerance annotations do not.
            leader.HasHookline = true;
            // Also a call to the Update method is not done automatically.
            // It is needed to reflect the actual properties and style of the Leader,
            // but it might be special cases when it is needed to have control over it.
            leader.Update(true);
            
            // Assign the leader to a new layer, keep in mind that the annotation will maintain its original layer
            leader.Layer = new Layer("Layer1") {Color = AciColor.Blue};

            // test cloning and transformation
            Leader copy1 = (Leader)leader.Clone();
            copy1.TransformBy(Matrix3.RotationZ(90 * MathHelper.DegToRad), Vector3.Zero);
            // after transforming a Leader entity is not necessary to call the Update method
            // this is just a check to ensure that the shape of the leader is kept, regardless
            copy1.Update(true);

            Leader copy2 = (Leader)leader.Clone();
            copy2.TransformBy(Matrix3.Scale(1, -1, 1), Vector3.Zero);
            copy2.Update(true);

            Leader copy3 = (Leader) leader.Clone();
            copy3.TransformBy(Matrix3.RotationZ(200 * MathHelper.DegToRad), Vector3.Zero);
            copy3.Update(true);

            // And the typical create document, add entity, and save file.
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(original);
            doc.Entities.Add(leader);
            doc.Entities.Add(copy1);
            doc.Entities.Add(copy2);
            doc.Entities.Add(copy3);
            doc.Save("test.dxf");
        }

        private static void CreateImageDefinition()
        {
            // The Image entity constructors "public ImageDefinition(string file)" and "public ImageDefinition(string name, string file)"
            // to avoid the use of the additional System.Drawing.Common.dll library.
            // In this case the System.Drawing.Common.dll has been used, but you can opt for other of your choice.
            // Load an external bitmap file to fill up the parameters required by the generic netDxf.Entities.Image constructor,
            // Remember to use bitmap formats compatible with AutoCad.
            string imgFile = "image.jpg";
            System.Drawing.Image img = System.Drawing.Image.FromFile(imgFile);
            ImageDefinition imageDefinition = new ImageDefinition("MyImage", imgFile, img.Width, img.HorizontalResolution, img.Height, img.VerticalResolution, ImageResolutionUnits.Inches);
            Image image = new Image(imageDefinition, Vector2.Zero, 100, 100);

            // The old constructors for the ImageDefinition class will only be available when using the Net Framework 4.5
            //ImageDefinition imgDef = new ImageDefinition(imgFile);

            DxfDocument test = new DxfDocument();
            // All related shortcuts to the place where the entities are really stored are done through the Entities property of the DxfDocument
            test.Entities.Add(image);

            test.Save("test.dxf");
        }

        #endregion

        #region Samples for new and modified features 2.4.1

        private static void LayerStateManager()
        {
            //Autodesk may have changed the LAS format through the years, if you find a problem with them open an issue at Github

            // a list of layer as a basis to create a layer state
            List<Layer> layers = new List<Layer>();
            layers.Add(new Layer("Layer1")
            {
                Color = AciColor.Red,
                Lineweight = Lineweight.W15
            });
            layers.Add(new Layer("Layer2")
            {
                Color = AciColor.Green,
                Lineweight = Lineweight.W35
            });
            layers.Add(new Layer("Layer3")
            {
                Color = AciColor.Blue,
                Lineweight = Lineweight.W70
            });

            // this will create a new layer state from the properties of the layers in the list
            LayerState layerState = new LayerState("LayerState1", layers);
            // now we can save it to an external LAS file for later use
            layerState.Save("LayerStateTest.las");

            // we can also create a layer state from an external LAS file
            LayerState layerStateLoad = LayerState.Load("LayerStateTest.las");

            // lets apply this into a new document
            DxfDocument doc = new DxfDocument();

            // add the newly loaded layer state
            doc.Layers.StateManager.Add(layerStateLoad);

            // the main methods to work with layer states are done through the StateManger accessible through the Layers of the document
            // import the external LAS file into the document
            // you can set if the imported layer state will overwrite any existing layer state with the same name
            // in this case there is one and we want to overwrite it, if it is set to false in this case the imported LAS file will not be added
            doc.Layers.StateManager.Import("LayerStateTest.las", true);

            // create a new layer state from the actual layer list of the document
            doc.Layers.StateManager.AddNew("LayerState2");
            // this is equivalent to this, it is a shortcut
            // doc.Layers.StateManager.Add(new LayerState("LayerState2", doc.Layers));

            doc.Layers.StateManager["LayerState2"].Properties["Layer1"].Color = AciColor.Yellow;
            doc.Layers.StateManager["LayerState2"].Properties["Layer2"].Color = AciColor.Magenta;
            doc.Layers.StateManager["LayerState2"].Properties["Layer3"].Color = AciColor.Cyan;

            // this will reset the actual layer properties with the ones stored in the specified layer state
            // optionally you can set the which properties you want to restore with the LayerStateManager.Options
            doc.Layers.StateManager.Options = LayerPropertiesRestoreFlags.Color;
            doc.Layers.StateManager.Restore("LayerState2");

            LayerStateProperties prop = new LayerStateProperties("OtherLayer");
            // this will generate an error,
            // when a layer state belongs to a document the newly added layer state properties must refer to one of the already existing layers
            //doc.Layers.StateManager["LayerState2"].Properties.Add(prop.Name, prop);

            // it is not a problem for not owned layer states
            LayerState otherLayerState = new LayerState("OtherLayerState");
            otherLayerState.Properties.Add(prop.Name, prop);
            // when adding it to the layer state manager of a document the missing "OtherLayer" will be created
            doc.Layers.StateManager.Add(otherLayerState);

            // the layer states will be stored in the DXF
            doc.Save("test.dxf");

            DxfDocument loaded = DxfDocument.Load("test.dxf");
            loaded.Save("test.dxf");
        }

        #endregion

        #region Samples for new and modified features 2.3.0

        public static void ExplodeInsert()
        {
            // create a block with all the drawing objects in the ModelSpace of the sample.dxf file
            Block block = Block.Load("sample.dxf", new List<string> {@".\Support"});
            // create an Insert from that block
            Insert insert = new Insert(block);

            // some transformation
            insert.Position = new Vector3(500, 250, 0);
            insert.Scale = new Vector3(2);
            insert.Rotation = 30;
            insert.Normal = new Vector3(0, -1, 0);

            // the above transformation also can be achieve using the TransformBy method
            // keep in mind that the transformation uses the column major convention.
            //insert.TransformBy(Matrix3.RotationX(MathHelper.HalfPI) * Matrix3.RotationZ(30 * MathHelper.DegToRad) * Matrix3.Scale(2), new Vector3(500,250,0));
            // using row major convention is equivalent to (A*B)t = At*Bt, where t means transpose
            //insert.TransformBy((Matrix3.Scale(2).Transpose() * Matrix3.RotationZ(30 * MathHelper.DegToRad).Transpose() * Matrix3.RotationX(MathHelper.HalfPI).Transpose()).Transpose(), new Vector3(500,250,0));

            // explode the block, this will decompose the insert and apply the insert transformation
            List<EntityObject> explode = insert.Explode();
            // create a document
            DxfDocument doc = new DxfDocument(DxfVersion.AutoCad2010, new List<string> {@".\Support"});
            // add the insert to the document as reference
            doc.Entities.Add(insert);
            // add the entities from the exploded insert
            doc.Entities.Add(explode);
            // save
            doc.Save("test.dxf");

            DxfDocument loaded = DxfDocument.Load("test.dxf", new List<string> {@".\Support"});
            loaded.Save("test compare.dxf");
        }

        public static void TransformArc()
        {
            Arc arc = new Arc {Center = new Vector3(0,0,10), Radius = 2, StartAngle = 30, EndAngle = 160};
            Block block = new Block("MyBlock");
            block.Entities.Add(arc);

            Insert insert = new Insert(block);
            //insert.Position = new Vector3(12,22,4);
            //insert.Scale = new Vector3(-1.25,0.75,1);
            insert.Scale = new Vector3(1,1,2);
            //insert.Rotation = 30;
            //insert.Normal = new Vector3(1,1,1);

            Arc circle2 = (Arc) arc.Clone();
            circle2.Color = AciColor.Blue;
            circle2.TransformBy(Matrix3.Scale(4,4,0), Vector3.Zero);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(insert);
            List<EntityObject> entities = insert.Explode();
            doc.Entities.Add(entities);
            doc.Entities.Add(circle2);
            doc.Save("test.dxf");
        }

        public static void TransformCircle()
        {
            Circle circle = new Circle {Center = new Vector3(0,0,10), Radius = 2};
            Block block = new Block("MyBlock");
            block.Entities.Add(circle);

            Insert insert = new Insert(block);
            //insert.Position = new Vector3(12,22,4);
            //insert.Scale = new Vector3(-1.25,0.75,1);
            insert.Scale = new Vector3(1,1,2);
            //insert.Rotation = 30;
            //insert.Normal = new Vector3(1,1,1);

            Circle circle2 = (Circle) circle.Clone();
            circle2.Color = AciColor.Blue;
            circle2.TransformBy(Matrix3.Scale(4,4,0), Vector3.Zero);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(insert);
            List<EntityObject> entities = insert.Explode();
            doc.Entities.Add(entities);
            doc.Entities.Add(circle2);
            doc.Save("test.dxf");
        }

        public static void TransformLwPolyline()
        {
            double bulge = -Math.Tan(Math.PI / 8);
            Polyline2DVertex p1 = new Polyline2DVertex(-100, 75, bulge);
            Polyline2DVertex p2 = new Polyline2DVertex(-75, 100, 0);
            Polyline2DVertex p3 = new Polyline2DVertex(75, 100, bulge);
            Polyline2DVertex p4 = new Polyline2DVertex(100, 75, 0);
            Polyline2DVertex p5 = new Polyline2DVertex(100, -75, bulge);
            Polyline2DVertex p6 = new Polyline2DVertex(75, -100, 0);
            Polyline2DVertex p7 = new Polyline2DVertex(-75, -100, bulge);
            Polyline2DVertex p8 = new Polyline2DVertex(-100, -75, 0);

            Polyline2D poly = new Polyline2D(new[] {p1,p2,p3,p4,p5,p6,p7,p8}, true);
            //poly.Normal = Vector3.UnitX;
            poly.TransformBy(Matrix3.RotationZ(30*MathHelper.DegToRad), Vector3.Zero);

            Block block = new Block("MyBlock");
            block.Entities.Add(poly);

            Insert insert = new Insert(block);
            //insert.Position = new Vector3(12,22,4);
            //insert.Scale = new Vector3(1.25,0.75,1);
            insert.Scale = new Vector3(1,2,3);
            insert.Rotation = 30;
            //insert.Normal = new Vector3(1,1,1);


            DxfDocument doc = new DxfDocument();
            //doc.Entities.Add(poly);
            doc.Entities.Add(insert);
            List<EntityObject> entities = insert.Explode();
            doc.Entities.Add(entities);
            doc.Save("test.dxf");

        }

        public static void TransformEllipse()
        {
            Ellipse ellipse = new Ellipse(Vector3.Zero, 2, 1)
            {
                StartAngle = 350, 
                EndAngle = 80, 
                Rotation = 30
            };

            //Ellipse ellipse = new Ellipse {Center = new Vector3( 0.616, 0.933, 0.0), MajorAxis = 2, MinorAxis = 1, StartAngle = 30, EndAngle = 160, Rotation = 30};
            Block block = new Block("MyBlock");
            block.Entities.Add(ellipse);

            Insert insert = new Insert(block);
            //insert.Position = new Vector3(12,22,4);
            //insert.Scale = new Vector3(-1.25,0.75,1);
            insert.Scale = new Vector3(1,-1,2);
            insert.Rotation = 30;
            //insert.Normal = new Vector3(1,1,1);
           
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(insert);
            List<EntityObject> entities = insert.Explode();
            doc.Entities.Add(entities);
            doc.Save("test.dxf");

        }

        public static void AddHeaderVariable()
        {
            //DxfDocument doc = DxfDocument.Load(@"sample.dxf");

            DxfDocument doc = new DxfDocument();

            HeaderVariable headerVariable;      

            // The ExtMin and ExtMax header variables cannot be directly accessed, now they will be added as custom header variables if the DXF has them
            // they have been deleted since netDxf does not calculate them
            Vector3 extMin;
            if (doc.DrawingVariables.TryGetCustomVariable("$EXTMIN", out headerVariable))
            {
                extMin = (Vector3) headerVariable.Value;
            }
            Vector3 extMax;
            if (doc.DrawingVariables.TryGetCustomVariable("$EXTMAX", out headerVariable))
            {
                extMax = (Vector3) headerVariable.Value;
            }

            // you can try to get a header variable and modify it or create a new one if it does not exists
            if (doc.DrawingVariables.TryGetCustomVariable("$SPLINESEGS", out headerVariable))
            {
                headerVariable.Value = (short) 5; // make sure you pass the correct value type, the code group 70 corresponds to a short
            }
            else
            {
                doc.DrawingVariables.AddCustomVariable(new HeaderVariable("$SPLINESEGS", 70, (short) 5));
            }            

            // or you can remove a header variable, even if it does not exist and add a new one
            doc.DrawingVariables.RemoveCustomVariable("$MEASUREMENT");
            doc.DrawingVariables.AddCustomVariable(new HeaderVariable("$MEASUREMENT", 70, (short) 0));

            doc.Save("test.dxf");
        }

        public static void MTextMirror()
        {
            MText text1 = new MText(new Vector2(50,20), 10);
            text1.Write("Sample text");
            text1.EndParagraph();
            text1.Write("Sample text");

            //text1.IsBackward = true;
            //text1.Rotation = 30;
            //text1.ObliqueAngle = 10;

            MText text2 = (MText)text1.Clone();
            text2.Color = AciColor.Yellow;

            //Matrix3 trans = Matrix3.Identity;
            //trans[0, 0] = -1;
            //text2.TransformBy(trans, Vector3.Zero);

            //Matrix3 trans = Matrix3.RotationZ(210 * MathHelper.DegToRad);
            //text2.TransformBy(trans, Vector3.Zero);


            DxfDocument doc = new DxfDocument();
            doc.DrawingVariables.MirrText = true;
            doc.Entities.Add(text1);
            doc.Entities.Add(text2);

            Matrix3 trans = Matrix3.Identity;
            trans[0, 0] = -1;
            //trans = Matrix3.RotationZ(30 * MathHelper.DegToRad) * trans;

            text2.TransformBy(trans, Vector3.Zero);

            doc.Save("test.dxf");
        }

        public static void TextMirror()
        {
            netDxf.Entities.Text.DefaultMirrText = true;

            Text text1 = new Text("Sample text", new Vector2(30,10), 10);
            text1.Alignment = TextAlignment.BaselineLeft;
            text1.Width = 200;

            //text1.IsBackward = true;
            //text1.Rotation = 30;
            text1.ObliqueAngle = 10;

            Text text2 = (Text)text1.Clone();
            text2.Color = AciColor.Yellow;
            Matrix3 trans = Matrix3.Identity;
            trans[1, 1] = -1;
            text2.TransformBy(trans, Vector3.Zero);

            //Matrix3 trans = Matrix3.RotationZ(210 * MathHelper.DegToRad);
            //Text text2 = (Text)text1.Clone();
            //text2.TransformBy(trans, Vector3.Zero);

            DxfDocument doc = new DxfDocument();
            //doc.DrawingVariables.MirrText = true;

            doc.Entities.Add(text1);
            doc.Entities.Add(text2);


            doc.Save("test.dxf");

            DxfDocument dxf = DxfDocument.Load("test.dxf");
        }

        public static void ShapeMirror()
        {
            ShapeStyle style = new ShapeStyle("shape.shx");
            Shape shape1 = new Shape("MyShape", style);
            shape1.ObliqueAngle = 20;

            Matrix3 trans = Matrix3.Identity;
            //trans[0, 0] = -1;
            trans[1, 1] = -1;
            Shape shape2 = (Shape)shape1.Clone();
            shape2.Color = AciColor.Yellow;
            shape2.TransformBy(trans, Vector3.Zero);

            DxfDocument doc = new DxfDocument();
            doc.SupportFolders.Add(@".\Support");
            doc.Entities.Add(shape1);
            doc.Entities.Add(shape2);
            doc.Save("test.dxf");

        }

        public static void MLineMirrorAndExplode()
        {
            MLineStyle style = new MLineStyle("MyStyle", "Personalized style.");
            style.Elements[0].Color = AciColor.Cyan;
            style.Elements[1].Color = AciColor.Yellow;
            style.Elements.Add(new MLineStyleElement(0.25) { Color = AciColor.Blue });
            style.Elements.Add(new MLineStyleElement(0.15) { Color = AciColor.Blue });
            style.Elements.Add(new MLineStyleElement(0.0) { Color = AciColor.Red });
            style.Elements.Add(new MLineStyleElement(-0.15) { Color = AciColor.Green });
            style.Elements.Add(new MLineStyleElement(-0.25) { Color = AciColor.Green });

            style.Elements.Sort();
            style.Flags = MLineStyleFlags.EndInnerArcsCap |
                          MLineStyleFlags.EndRoundCap |
                          MLineStyleFlags.StartInnerArcsCap |
                          MLineStyleFlags.StartRoundCap |
                          MLineStyleFlags.StartSquareCap |
                          MLineStyleFlags.EndSquareCap |
                          MLineStyleFlags.DisplayJoints;

            //style.StartAngle = 120;
            //style.EndAngle = 30;
            List<Vector2> vertexes = new List<Vector2>
            {
                new Vector2(50, 0),
                new Vector2(50, 150),
                new Vector2(200, 150),
                new Vector2(300, 250)
            };

            MLine mline1 = new MLine(vertexes, style, 20);
            //mline.NoStartCaps = true;
            //mline.Normal = new Vector3(1);
            mline1.Layer = new Layer("Layer1") { Color = AciColor.Blue };
            mline1.Justification = MLineJustification.Bottom;
            mline1.Update();

            MLine mline2 = (MLine)mline1.Clone();
            //mline2.Color = AciColor.Yellow;

            //Matrix3 trans = Matrix3.RotationZ(Math.PI);
            //Matrix3 trans = Matrix3.Identity;
            //trans[0, 0] = -1;
            ////trans[1, 1] = -1;

            Matrix3 trans = Matrix3.RotationX(MathHelper.HalfPI);
            mline2.TransformBy(trans, Vector3.Zero);

            DxfDocument doc = new DxfDocument(DxfVersion.AutoCad2010);
            doc.DrawingVariables.LtScale = 10;

            doc.Entities.Add(mline1);
            doc.Entities.Add(mline2);
            //List<EntityObject> entities = mline2.Explode();
            //doc.Entities.Add(entities);

            doc.Save("test.dxf");
        }

        public static void InsertMirror()
        {
            DxfDocument doc = DxfDocument.Load("BlockSample.dxf");
            doc.DrawingVariables.MirrText = true;
            Insert insert = doc.Entities.Inserts.ElementAt(0);

            Insert copy = (Insert) insert.Clone();
            Matrix3 trans = Matrix3.Identity;
            trans[1, 1] = -1;
            copy.TransformBy(trans, Vector3.Zero);
            //doc.Entities.Add(copy);
            doc.Entities.Add(copy.Explode());
            doc.Save("test.dxf");

        }

        public static void LeaderMirror()
        {
            // a text annotation with style
            DimensionStyle style = new DimensionStyle("MyStyle");
            style.DimLineColor = AciColor.Green;
            style.TextColor = AciColor.Blue;
            //style.LeaderArrow = DimensionArrowhead.DotBlank;
            style.DimScaleOverall = 2.0;

            // a basic text annotation
            List<Vector2> vertexes1 = new List<Vector2>();
            vertexes1.Add(new Vector2(0, 0));
            vertexes1.Add(new Vector2(7, 7));
            //Leader leader1 = new Leader("Sample annotation", vertexes1, style);


            Leader leader1 = new Leader(vertexes1, style);
            leader1.Annotation = new MText("Sample annotation");
            leader1.HasHookline = true;
            leader1.Update(true);
            //leader1.Annotation = new Text("Sample annotation", style.TextHeight);

            //// a tolerance annotation
            //List<Vector2> vertexes3 = new List<Vector2>();
            //vertexes3.Add(new Vector2(0));
            //vertexes3.Add(new Vector2(5, 5));
            //vertexes3.Add(new Vector2(7.5, 5));
            //ToleranceEntry entry = new ToleranceEntry
            //{
            //    GeometricSymbol = ToleranceGeometricSymbol.Symmetry,
            //    Tolerance1 = new ToleranceValue(true, "12.5", ToleranceMaterialCondition.Maximum)
            //};
            //Leader leader1 = new Leader(entry, vertexes3);
            //((Tolerance) leader1.Annotation).TextHeight = 0.35;

            // a block annotation
            Block block = new Block("BlockAnnotation");
            block.Entities.Add(new Line(new Vector2(-1, -1), new Vector2(1, 1)));
            block.Entities.Add(new Line(new Vector2(-1, 1), new Vector2(1, -1)));
            block.Entities.Add(new Circle(Vector2.Zero, 0.5));

            Insert ins = new Insert(block);

            //List<Vector2> vertexes4 = new List<Vector2>();
            //vertexes4.Add(new Vector2(0));
            //vertexes4.Add(new Vector2(-5, -5));
            //vertexes4.Add(new Vector2(-7.5, -5));
            //Leader leader1 = new Leader(block, vertexes4);

            // add entities to the document
            DxfDocument doc = new DxfDocument();
            //doc.Entities.Add((EntityObject) leader1.Clone());
            doc.Entities.Add(leader1);


            Leader leader2 = (Leader)leader1.Clone();
            //Matrix3 trans = Matrix3.Scale(2);
            Matrix3 trans = Matrix3.Identity;
            trans[1, 1] *= -1;
            //Matrix3 trans = Matrix3.RotationZ(210 * MathHelper.DegToRad);
            leader2.TransformBy(trans, Vector3.Zero);
            //leader2.TransformBy(trans, new Vector3(0, -10, 0));
            doc.Entities.Add(leader2);


            //leader1.Annotation = ins;

            //leader2.Annotation = null;

            doc.Entities.Remove(leader1);

            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Save("test.dxf");
        }

        #endregion

        #region Samples for new and modified features 2.2.1

        private static void MTextParagraphFormatting()
        {
            MText text = new MText();
            text.RectangleWidth = 50;

            // The text formatting of an MText entity is done in two levels, at paragraph level and at character, word, line level.

            // this class holds the properties of the text at character, word, line level
            MTextFormattingOptions opText = new MTextFormattingOptions();

            // this class holds the properties of the text at paragraph level
            MTextParagraphOptions opPara = new MTextParagraphOptions();

            opPara.Alignment = MTextParagraphAlignment.Center;
            opPara.HeightFactor = 1.5;
            opPara.FirstLineIndent = 1.5;
            opPara.SpacingAfter = 3.0;

            // applies the paragraph options defined by the MTextParagraphOptions to the new paragraph
            text.StartParagraph(opPara);
            opText.FontName = "Tahoma";
            text.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
                       " Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                       " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
                       " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                       opText);

            opPara.Alignment = MTextParagraphAlignment.Center;
            opPara.HeightFactor = 1.5;
            opPara.FirstLineIndent = 1.5;
            opPara.SpacingAfter = 3.0;
            // adds the end paragraph code \P, that marks the end of the current paragraph
            text.EndParagraph();

            opPara.Alignment = MTextParagraphAlignment.Justified;
            opPara.HeightFactor = 1.0;
            opPara.FirstLineIndent = 0.0;
            opPara.LeftIndent = 2.5;
            opPara.RightIndent = 2.5;
            opPara.SpacingBefore = 1.8;
            opPara.SpacingAfter = 1.8;
            opPara.LineSpacingStyle = MTextLineSpacingStyle.Exact;
            opPara.LineSpacingFactor = 1.8;

            text.StartParagraph(opPara);
            opText.FontName = "Times New Roman";
            text.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
                       " Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                       " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
                       " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.",
                       opText);
            text.EndParagraph();

            // when no instance of MTextParagraphOptions, the current paragraph and it will inherit the previous ones
            text.StartParagraph();
            // writes text with the default text options defined by the TextStyle of the MText entity
            text.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
                       " Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                       " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
                       " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            text.EndParagraph();

            text.StartParagraph(new MTextParagraphOptions());
            text.Write("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
                       " Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
                       " Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
                       " Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
            // the call to the EndParagraph method for the last paragraph, strictly speaking, is not needed
            text.EndParagraph();

            string data = text.PlainText();
            Console.Write(data);
            Console.ReadLine();
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(text);
            doc.Save("test1.dxf");
        }

        private static void MTextCharacterFormatting()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.Layers.Add(new Layer("Layer1") {Color = new AciColor(Color.SkyBlue)});
            MText text = new MText();
            text.Color = AciColor.Yellow;
            text.RectangleWidth = 45;
            MTextFormattingOptions opText = new MTextFormattingOptions();
            MTextParagraphOptions opPara = new MTextParagraphOptions();

            // when no instance of MTextParagraphOptions, the current paragraph and it will inherit the previous ones
            text.StartParagraph();
            text.Write("Text line with the default formatting options", opText);
            text.EndParagraph();

            text.StartParagraph();
            opText.FontName = "Tahoma";
            text.Write("Sample text with a fraction ", opText);
            text.WriteFraction("12.54", "abc", FractionFormatType.Diagonal, opText);
            text.EndParagraph();

            opPara.Alignment = MTextParagraphAlignment.Right;
            opPara.VerticalAlignment = MTextParagraphVerticalAlignment.Bottom;
            text.StartParagraph(opPara);
            opText.FontName = "Times New Roman";
            opText.Color = AciColor.Red;
            text.Write("New", opText);
            opText.HeightFactor = 1.5;
            opText.WidthFactor = 1.5;
            text.Write(" line ", opText);
            opText.HeightFactor = 1.0;
            opText.WidthFactor = 1.0;
            text.Write("of text, with right paragraph alignment.", opText);
            
            text.EndParagraph();

            opPara.Alignment = MTextParagraphAlignment.Center;
            opPara.VerticalAlignment = MTextParagraphVerticalAlignment.Center;
            text.StartParagraph(opPara);
            opText.Color = AciColor.ByLayer;
            text.Write("Another line of text, x", opText);
            opText.Superscript = true;
            text.Write("12", opText);
            opText.Superscript = false;
            text.Write(" y", opText);
            opText.Subscript = true;
            text.Write("34", opText);
            opText.Subscript = false;
            text.Write(". Paragraph aligned to center.", opText);
            text.EndParagraph();

            text.StartParagraph(new MTextParagraphOptions());
            opText = new MTextFormattingOptions();
            opText.FontName = "Times New Roman";
            opText.Bold = true;
            opText.Color = new AciColor(Color.SkyBlue);
            opText.Italic = true;
            opText.Underline = true;
            opText.HeightFactor = 1.6;
            text.Write("Text line with the default paragraph options", opText);
            text.EndParagraph();

            dxf.Entities.Add(text);

            string data = text.PlainText();
            Console.Write(data);
            Console.ReadLine();
            dxf.Save("test2.dxf");
        }

        #endregion

        #region Samples for new and modified features 2.2.0

        private static void AccessModelBlock()
        {
            Layer layer = new Layer("Layer1") { Color = AciColor.Green };
            Line line1 = new Line(new Vector2(0, 0), new Vector2(50, 50)) { Layer = layer };
            Line line2 = new Line(new Vector2(0, 0), new Vector2(50, -50)) { Layer = layer };
            Line line3 = new Line(new Vector2(0, 0), new Vector2(-50, -50)) { Layer = layer };

            DxfDocument doc = new DxfDocument();

            // this specifies the layout where the entities will be added when using the Entities.Add() method of the DxfDocument (by default is the "Model" layout)
            doc.Entities.ActiveLayout = Layout.ModelSpaceName;
            // this was the only way to add entities to the document
            doc.Entities.Add(line1);

            // internally in the DXF all entities belongs to a block one way or the other,
            // an entity might be part of a block created by the user,
            // or one of the internal block created to represent the Model Space or the multiple Paper Spaces that may appear
            
            // now you can add entities directly through the model space block
            doc.Blocks[Block.DefaultModelSpaceName].Entities.Add(line2);

            // this is also valid, accessing the model space block through the layout
            doc.Layouts[Layout.ModelSpaceName].AssociatedBlock.Entities.Add(line3);

            // either way will end up with the same result
            doc.Save("test.dxf");


            // making a copy of the model space block, you must manually give a valid name, otherwise an ArgumentException will be risen
            Block cloned = (Block)doc.Blocks[Block.DefaultModelSpaceName].Clone("FullCopy");
            DxfDocument copy = new DxfDocument();
            Insert ins = new Insert(cloned);
            copy.Entities.Add(ins);
            copy.Save("clone.dxf");

            // something similar can be done when reading entities from a document
            DxfDocument loaded = DxfDocument.Load("test.dxf");

            // this two list will return the same entities, this is how to access the full list of entities that belongs to a layout
            EntityCollection entities1 = loaded.Layouts[Layout.ModelSpaceName].AssociatedBlock.Entities;
            EntityCollection entities2 = loaded.Blocks[Block.DefaultModelSpaceName].Entities;

            // getting the layout references not only include the entities of the associated block but also its attribute definitions
            List<DxfObject> entities3 = loaded.Layouts.GetReferences(Layout.ModelSpaceName);

            // this will iterate through the lines we previously added to the doc DxfDocument (using Linq)
            foreach (Line line in entities1.OfType<Line>())
            {
                line.Color = AciColor.Blue;
            }

            // this specifies the active layout from where we will pick up the entities
            // when using the properties Lines, Circles, Arcs,... of the DxfDocument (by default is the "Model" layout)
            doc.Entities.ActiveLayout = Layout.ModelSpaceName;
            // accessing the list of entities through the properties Lines, Circles, Arcs,... of the DxfDocument
            // will return the list of entities of that specific type contained in the active layout
            foreach (Line line in loaded.Entities.Lines)
            {
                line.Color = AciColor.Blue;
            }

            // both ways will end up with the same result
            loaded.Save("test compare 1.dxf");


            // Something similar can be done when removing entities from a document
            // pick up the first entity of the list, we now in advance it will be a line (in reality it might contain any kind of entity)
            Line delete = (Line) loaded.Blocks[Block.DefaultModelSpaceName].Entities[0];

            bool isDeleted;
            // this was the way of removing entities from the document ( isDeleted = true)
            isDeleted = loaded.Entities.Remove(delete);

            // but now we can do the same directly through the block,
            // this way we need to specify the layout, we might know it in advance, like now we are always working in the model layout
            isDeleted = loaded.Layouts[Layout.ModelSpaceName].AssociatedBlock.Entities.Remove(delete); // (isDeleted = false)
            isDeleted = loaded.Blocks[Block.DefaultModelSpaceName].Entities.Remove(delete); // (isDeleted = false)

            // or we can access the owner of the line 
            // pick up another line from the document, the old one has now owner anymore
            delete = (Line)loaded.Blocks[Block.DefaultModelSpaceName].Entities[0];
            isDeleted = loaded.Blocks[delete.Owner.Name].Entities.Remove(delete); // (isDeleted = true)

            // only one more line remains in the document
            loaded.Save("test compare 2.dxf");
        }

        private static void DimensionBlockGeneration()
        {
            DimensionStyle style = DimensionStyle.Iso25;
            Vector2 ref1 = Vector2.Zero;
            Vector2 ref2 = new Vector2(50, 10);
            double offset = 10;

            // create a dimension
            AlignedDimension dim = new AlignedDimension(ref1, ref2, offset, style);

            DxfDocument doc = new DxfDocument();
            // this will specify that we want to generate the associated blocks that represent the dimension drawing.
            // by default it is set to false, it will be the responsibility of the program reading the DXF to generate them
            // keep in mind that the generation of the dimension drawing blocks is limited an do not support the full range of options defined by the dimension style
            // it uses the DimenionBlock class for that purpose
            doc.BuildDimensionBlocks = true;
            doc.Entities.Add(dim);
            doc.Save("test.dxf");


            DxfDocument loaded = DxfDocument.Load("test.dxf");
            // if the imported DXF has dimension with blocks drawing blocks, we can erase them setting the dimension Block property to null
            foreach (Dimension dimension in loaded.Entities.Dimensions)
            {
                dimension.Block = null;
            }

            // now that the dimensions have no drawing blocks it will be responsibility of the program that loads the DXF to generate them, if it needs it
            doc.Save("test compare 1.dxf");


            // we can also associate the dimension with our own blocks
            // the initial name of the block is irrelevant it will be renamed to accommodate the nomenclature used by the DXF ("*D#" where # is a number)
            foreach (Dimension dimension in loaded.Entities.Dimensions)
            {
                dimension.Block = DimensionBlock.Build(dimension);
            }
            doc.Save("test compare 2.dxf");
        }

        #region tests

        private static void TestDoc()
        {
            DxfDocument doc = DxfDocument.Load("sample.dxf");

            Angular2LineDimension d = null;
            foreach (Dimension dim in doc.Entities.Dimensions)
            {
                dim.Block = DimensionBlock.Build(dim);
                if (dim.DimensionType == DimensionType.Angular)
                    d = (Angular2LineDimension)dim;
            }
            doc.Save("test compare.dxf");

            Angular2LineDimension angDim = (Angular2LineDimension)d.Clone();

            Angular2LineDimension angDim2 = new Angular2LineDimension(angDim.StartFirstLine, angDim.EndFirstLine, angDim.StartSecondLine, angDim.EndSecondLine, 20);



            angDim.SetDimensionLinePosition(new Vector2(360, -126));
            angDim2.SetDimensionLinePosition(new Vector2(360, -126));



            DxfDocument dxf = new DxfDocument();
            //dxf.BuildDimensionBlocks = true;
            dxf.Entities.Add(angDim);
            dxf.Entities.Add(angDim2);
            dxf.Save("test.dxf");
        }

        private static void TestDimAligned()
        {
            //DxfDocument doc = DxfDocument.Load("DimAligned check.dxf");

            DimensionStyle style = DimensionStyle.Iso25;
            style.DimLengthUnits = LinearUnitType.Architectural;
            style.LengthPrecision = 8;
            style.TextFractionHeightScale = 0.5;
            style.FractionType = FractionFormatType.Horizontal;
            style.AngularPrecision = 0;
            style.AngularPrecision = 4;
            style.AngularPrecision = -1;
            style.AngularPrecision = -2;

            Vector2 ref1 = Vector2.Zero;
            Vector2 ref2 = new Vector2(50, 10);
            Vector2 ref3 = new Vector2(-10, 50);
            double offset = 10;

            AlignedDimension dim = new AlignedDimension(ref2, ref1, offset, style);
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextFractionHeightScale, 0.9));
            //dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextDirection, DimensionStyleTextDirection.RightToLeft));
            //style.TextDirection = DimensionStyleTextDirection.RightToLeft;

            //dim.SetDimensionLinePosition(new Vector2(40, 30));
            //dim.SetDimensionLinePosition(new Vector2(9, -20));
            //dim.SetDimensionLinePosition(new Vector2(-9, -20));

            //dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithLeader));
            //dim.TextReferencePoint = new Vector2(50, 30);

            //dim.Offset = 10;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithLeader;
            //dim.TextPositionManuallySet = false;

            //dim.Update();

            //dim.TextPositionManuallySet = false;
            //dim.Update();

            DxfDocument doc = new DxfDocument();
            doc.BuildDimensionBlocks = true;
            doc.Entities.Add(dim);
            doc.Save("test.dxf", true);

            DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions[0].Block = null;
            dxf.Save("test compare.dxf");
        }

        private static void TestDimLinear()
        {
            DxfDocument doc = DxfDocument.Load("DimLinear check.dxf");

            DimensionStyle style = DimensionStyle.Iso25;
            //style.TextInsideAlign = false;
            Vector2 ref1 = Vector2.Zero;
            Vector2 ref2 = new Vector2(50, 10);
            Vector2 ref3 = new Vector2(0, 50);
            double offset = 20;

            LinearDimension dim = new LinearDimension(ref1, ref2, offset, 20, style);
            LinearDimension dim2 = new LinearDimension(ref2, ref1, offset, 180 + 20, style);

            //LinearDimension dim = new LinearDimension(ref1, ref2, offset, 0, style);
            //dim.TextReferencePoint = new Vector2(0, 30);
            //dim.TextRotation = 30;
            //dim.Offset = 20;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithLeader;
            //dim.TextPositionManuallySet = false;
            //dim.Update();
            //dim.SetDimensionLinePosition(new Vector2(-30, -20));

            //dim.SetDimensionLinePosition(new Vector2(20, 0));
            //LinearDimension dim1p = (LinearDimension) dim.Clone();
            //dim.SetDimensionLinePosition(new Vector2(30, -20));
            //dim.SetDimensionLinePosition(new Vector2(-30, -20));
            //dim.Block = DimensionBlock.Build(dim, "DimBlock");

            //DxfDocument doc = new DxfDocument();
            doc.BuildDimensionBlocks = true;
            //doc.DrawingVariables.DimStyle = style.Name;
            doc.Entities.Add(dim);
            doc.Entities.Add(dim2);
            //doc.Entities.Add(dim1p);
            //dim.Rotation = 0;
            //dim.Update();
            //doc.Entities.Add(new Line(ref1, ref2));
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            ////dxf.Dimensions[0].Block = null;
            //dxf.Save("test compare.dxf");
        }

        private static void TestDim2LineAngular()
        {

            DxfDocument doc = DxfDocument.Load("DimAng2L check.dxf");

            DimensionStyle style = DimensionStyle.Iso25;
            style.TextInsideAlign = false;

            Layer layer = new Layer("Layer1") { Color = AciColor.Blue };
            Vector2 start1 = new Vector2(-20, 20);
            Vector2 end1 = new Vector2(20, -20);
            Vector2 start2 = new Vector2(-10, -30);
            Vector2 end2 = new Vector2(10, 30);

            Line line1 = new Line(start1, end1) { Layer = layer };
            Line line2 = new Line(start2, end2) { Layer = layer };

            Angular2LineDimension dim = new Angular2LineDimension(line1, line2, 10, style);

            //dim.SetDimensionLinePosition(new Vector2(20, 0));
            dim.SetDimensionLinePosition(new Vector2(0, 20));
            //dim.SetDimensionLinePosition(new Vector2(0, -20));
            //dim.SetDimensionLinePosition(new Vector2(-20, 0));

            //dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithLeader));
            //dim.Update();

            dim.TextReferencePoint = new Vector2(10, 30);
            //dim.TextPositionManuallySet = false;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.BesideDimLine;
            dim.Update();

            //DxfDocument doc = new DxfDocument();
            doc.BuildDimensionBlocks = true;
            //doc.DrawingVariables.DimStyle = style.Name;
            doc.Entities.Add(dim);
            //dim.SetDimensionLinePosition(MathHelper.FindIntersection(start1, end1-start1, start2, end2-start2));
            //doc.Entities.Add(line1);
            //doc.Entities.Add(line2);
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions.ElementAt(0).Block = null;
            //dxf.Save("test compare.dxf");
        }

        private static void TestDim3PointAngular()
        {
            DxfDocument doc = DxfDocument.Load("DimAng3P check.dxf");
            DimensionStyle style = DimensionStyle.Iso25;
            style.TextInsideAlign = false;

            Layer layer = new Layer("Layer1") { Color = AciColor.Blue };
            Vector2 start1 = new Vector2(-20, 20);
            Vector2 end1 = new Vector2(20, -20);
            Vector2 start2 = new Vector2(-10, -30);
            Vector2 end2 = new Vector2(10, 30);

            Line line1 = new Line(start1, end1) { Layer = layer };
            Line line2 = new Line(start2, end2) { Layer = layer };
            Vector2 center = MathHelper.FindIntersection(start1, end1 - start1, start2, end2 - start2);
            Angular3PointDimension dim = new Angular3PointDimension(center, end1, end2, 10, style);

            dim.SetDimensionLinePosition(new Vector2(-20, 10));
            //dim.SetDimensionLinePosition(new Vector2(4, 16));
            //dim.SetDimensionLinePosition(new Vector2(-30, 20));
            //dim.SetDimensionLinePosition(new Vector2(0, -20));
            dim.TextReferencePoint = new Vector2(30, 20);
            //dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithLeader));
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithLeader;
            //dim.Update();

            //dim.TextPositionManuallySet = false;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.BesideDimLine;
            //dim.Offset = -20;
            dim.Update();

            //DxfDocument doc = new DxfDocument();
            //doc.DrawingVariables.DimStyle = style.Name;
            doc.Entities.Add(dim);
            //dim.SetDimensionLinePosition(center);
            //doc.Entities.Add(line1);
            //doc.Entities.Add(line2);
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions.ElementAt(0).Block = null;
            //dxf.Save("test compare.dxf");

        }

        private static void TestDimDiametric()
        {
            DxfDocument doc = DxfDocument.Load("DimDia check.dxf");
            DimensionStyle style = DimensionStyle.Iso25;
            style.CenterMarkSize = 0;
            //style.TextInsideAlign = false;

            Layer layer = new Layer("Layer1") { Color = AciColor.Blue };

            Vector2 center = new Vector2(1, 2);
            double radius = 30;
            Circle circle = new Circle(center, radius) { Layer = layer };

            DiametricDimension dim = new DiametricDimension(circle, 15, style);
            //dim.SetDimensionLinePosition(new Vector2(radius + 30, 50));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithoutLeader));
            dim.TextReferencePoint = new Vector2(-40, 30);
            //dim.TextReferencePoint = center;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithoutLeader;
            dim.Update();

            dim.TextPositionManuallySet = false;
            dim.Update();

            //DxfDocument doc = new DxfDocument();
            doc.BuildDimensionBlocks = true;
            //doc.DrawingVariables.DimStyle = style.Name;

            doc.Entities.Add(dim);
            //doc.Entities.Add(circle);
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions.ElementAt(0).Block = null;
            //dxf.Save("test compare.dxf");

        }

        private static void TestDimRadial()
        {
            DxfDocument doc = DxfDocument.Load("DimRad check.dxf");

            DimensionStyle style = DimensionStyle.Iso25;
            //style.TextInsideAlign = false;

            Layer layer = new Layer("Layer1") { Color = AciColor.Blue };

            Vector2 center = new Vector2(1, 2);
            double radius = 30;
            Circle circle = new Circle(center, radius) { Layer = layer };

            RadialDimension dim = new RadialDimension(circle, 15, style);
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithoutLeader));
            dim.TextReferencePoint = new Vector2(-40, 30);

            //dim.SetDimensionLinePosition(new Vector2(radius + 30, 50));
            //dim.TextReferencePoint = new Vector2(radius + 30, 50);
            //dim.TextReferencePoint = center;
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.OverDimLineWithoutLeader;
            dim.Update();

            //DxfDocument doc = new DxfDocument();
            //doc.BuildDimensionBlocks = true;
            //doc.DrawingVariables.DimStyle = style.Name;
            doc.Entities.Add(dim);
            //doc.Entities.Add(circle);
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions.ElementAt(0).Block = null;
            //dxf.Save("test compare.dxf");

        }

        private static void TestDimOrdinate()
        {
            DimensionStyle style = DimensionStyle.Iso25;

            Vector2 origin = new Vector2(10, 5);
            Vector2 refX = new Vector2(20, 10);
            Vector2 refY = new Vector2(0, 20);
            double length = 30;
            double angle = 30;

            //OrdinateDimension dimX = new OrdinateDimension(origin, refX, length, OrdinateDimensionAxis.X, 0, style);
            //OrdinateDimension dimY = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, 0, style);

            //Vector2 leaderEnd = new Vector2(5,20);
            //OrdinateDimension dimX = new OrdinateDimension(origin, refX, leaderEnd, style);
            //OrdinateDimension dimY = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, 0, style);

            //OrdinateDimension dim = new OrdinateDimension(Vector2.Zero, new Vector2(10, 20), new Vector2(30, 10), style)
            //{
            //    //Rotation = 30
            //};
            OrdinateDimension dim1 = new OrdinateDimension(Vector2.Zero, new Vector2(10, 10), new Vector2(30, 30), OrdinateDimensionAxis.Y, style);
            dim1.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.FitTextMove, DimensionStyleFitTextMove.OverDimLineWithoutLeader));
            dim1.TextReferencePoint = new Vector2(-20, -20);
            dim1.Update();

            OrdinateDimension dim2 = new OrdinateDimension(Vector2.Zero, new Vector2(10, 10), new Vector2(-5, 30), OrdinateDimensionAxis.Y, style);
            //dim1.Rotation = 30;
            //dim2.Rotation = 30;

            OrdinateDimension dim3 = new OrdinateDimension(Vector2.Zero, new Vector2(10, 10), new Vector2(30, 30), OrdinateDimensionAxis.Y, style);
            OrdinateDimension dim4 = new OrdinateDimension(Vector2.Zero, new Vector2(10, 10), new Vector2(-30, -5), OrdinateDimensionAxis.Y, style);

            OrdinateDimension dim5 = new OrdinateDimension(Vector2.Zero, new Vector2(-10, -10), -20, OrdinateDimensionAxis.Y, style);

            //dim.TextReferencePoint = new Vector2(40, 30);
            ////dim.LeaderEndPoint = new Vector2(40,30);
            //dim.Style.FitTextMove = DimensionStyleFitTextMove.BesideDimLine;
            //dim.AttachmentPoint = MTextAttachmentPoint.MiddleCenter;
            //dim.Update();

            DxfDocument doc = new DxfDocument();
            doc.BuildDimensionBlocks = true;
            doc.Entities.Add(dim1);
            //doc.Entities.Add(dim2);
            //doc.Entities.Add(dim3);
            //doc.Entities.Add(dim4);
            //doc.Entities.Add(dim5);
            doc.Save("test.dxf");

            //DxfDocument dxf = DxfDocument.Load("test.dxf");
            //dxf.Dimensions.ElementAt(0).Block = null;
            //dxf.Save("test compare.dxf");

        }

        private static void TestHatch()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.IsClosed = true;
            HatchBoundaryPath boundary1 = new HatchBoundaryPath(new List<EntityObject> { poly });

            Circle circle = new Circle(Vector2.Zero, 5);
            HatchBoundaryPath boundary2 = new HatchBoundaryPath(new List<EntityObject> { circle });

            HatchPattern pattern = HatchPattern.Line;
            pattern.Scale = 10;
            pattern.Angle = 45;

            Hatch hatch = new Hatch(pattern, true);

            hatch.BoundaryPaths.Add(boundary1);

            dxf.Entities.Add(hatch);
            hatch.BoundaryPaths.Add(boundary2);

            //dxf.Entities.Remove(hatch);

            //hatch.BoundaryPaths.Add(boundary2);
            //hatch.UnLinkBoundary();

            //dxf.Blocks[Block.DefaultModelSpaceName].Entities.Remove(poly);
            //dxf.Blocks[Block.DefaultModelSpaceName].Entities.Remove(circle);
            //dxf.Blocks[Block.DefaultModelSpaceName].AttributeDefinitions.Add(new AttributeDefinition("MyTag"));
            //hatch.CreateBoundary(true);

            //dxf.Entities.Add(poly);
            //dxf.Entities.Add(circle);

            //Block myBlock = new Block("MyBlock");
            //dxf.Blocks.Add(myBlock);
            //myBlock.Entities.Add(circle);
            //myBlock.AttributeDefinitions.Add(new AttributeDefinition("MyTag"));

            dxf.Save("Hatch.dxf");

            //DxfDocument doc = DxfDocument.Load("Hatch.dxf");
            DxfDocument doc = new DxfDocument();
            Block block = Block.Create(dxf, "FullDrawing");
            doc.Entities.Add(new Insert(block, new Vector2(10, 10)));
            doc.Save("hatch compare.dxf");

            Block test = (Block)doc.Blocks[Block.DefaultModelSpaceName].Clone("CopyModelSpace");
        }

        private static void TestLeader()
        {
            // a basic text annotation
            List<Vector2> vertexes1 = new List<Vector2>();
            vertexes1.Add(new Vector2(0, 0));
            vertexes1.Add(new Vector2(-5, 5));
            //vertexes1.Add(new Vector2(-5.5,4.5));
            Leader leader1 = new Leader("Sample annotation", vertexes1);
            //leader1.HasHookline = true;
            //leader1.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextVerticalPlacement, DimensionStyleTextVerticalPlacement.Above));

            //leader1.Offset = new Vector2(0.5, 0.5);
            //((MText) leader1.Annotation).Position = new Vector3(-5, 5, 0);
            //((MText) leader1.Annotation).Rotation = 10;
            //leader1.Update(false);

            //leader1.Normal = Vector3.UnitX;

            //leader1.Offset = new Vector2(-0.5, -0.5);

            //leader1.HasHookline = false;

            //leader1.TextVerticalPosition = LeaderTextVerticalPosition.Centered;
            //leader1.Update(false);

            //leader1.TextVerticalPosition = LeaderTextVerticalPosition.Centered;
            // We need to call manually the method Update if the annotation position is modified,
            // or the Leader properties like Style, Normal, Elevation, Annotation, TextVerticalPosition, and/or Offset.


            // a block annotation
            Block block = new Block("BlockAnnotation");
            block.Entities.Add(new Line(new Vector2(-1, -1), new Vector2(1, 1)));
            block.Entities.Add(new Line(new Vector2(-1, 1), new Vector2(1, -1)));
            block.Entities.Add(new Circle(Vector2.Zero, 0.5));

            List<Vector2> vertexes4 = new List<Vector2>();
            vertexes4.Add(new Vector2(0));
            vertexes4.Add(new Vector2(-5, -5));
            vertexes4.Add(new Vector2(-7.5, -5));
            Leader leader4 = new Leader(block, vertexes4);
            // change the leader offset to move  the leader hook (the last vertex of the leader vertexes list) in relation to the annotation position.
            //leader4.Offset = new Vector2(-0.5, 0);
            leader4.Update(true);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(leader1);
            doc.Entities.Add(leader4);
            doc.Save("test.dxf");

            //DxfDocument loaded = DxfDocument.Load("test.dxf");
            //loaded.Save("test compare.dxf");


            //loaded = DxfDocument.Load("Drawing3.dxf");
            //loaded.Save("test compare.dxf");

            //DxfDocument clonned = new DxfDocument();
            //foreach (Leader leader in loaded.Leaders)
            //{
            //    Leader clone = (Leader) leader.Clone();
            //    //clone.HasHookline = false;
            //    ((MText) clone.Annotation).Rotation = 10;
            //    clone.Update(true);
            //    clonned.Entities.Add(clone);
            //}

            //clonned.Save("test compare.dxf");

        }

        private static void TestModelSpaceBlock()
        {
            // Create a block to be used as sample
            Block baseBlk = new Block("BaseBlock");
            baseBlk.Record.Units = DrawingUnits.Millimeters;
            baseBlk.Entities.Add(new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0)));
            baseBlk.Entities.Add(new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0)));
            AttributeDefinition attdef = new AttributeDefinition("MyAttribute")
            {
                Prompt = "Enter a value:",
                Value = "0",
                Position = Vector3.Zero,
                Layer = new Layer("MyLayer")
                {
                    Color = AciColor.Red
                }
            };
            baseBlk.AttributeDefinitions.Add(attdef);

            // Blocks are saved in a similar way as other dxf, just pass the dxf file name, the DxfVersion, and optionally if the dxf needs to be saved in binary format
            // Only AutoCad2000 and newer versions are supported.
            // The block entities and attribute definitions will be added to the Model layout.
            // The drawing header units will be the ones defined in the block record.
            baseBlk.Save(baseBlk.Name + ".dxf", DxfVersion.AutoCad2000);

            DxfDocument dxf = new DxfDocument();

            // Blocks are loaded as any other dxf, just pass the dxf file name,
            // optionally you can also give it a name, by default the file name without extension will be used.
            // Only AutoCad2000 and newer versions are supported,
            // Only the entities contained in the Model layout will be used.
            // The block units will be the ones defined in the dxf header.
            Block block = Block.Load(baseBlk.Name + ".dxf", "MyBlock");

            // in case the loading process has failed check for null
            // In DEBUG mode the loading process will raise exceptions while in RELEASE it will just return null, the same as loading a DxfDocument
            if (block == null)
            {
                Console.WriteLine("Error loading the block dxf file.");
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                return;
            }

            // once the block is loaded we can use it in insert entities
            Insert insert = new Insert(block, new Vector2(10));

            // the block might also contain attribute definitions
            int attdefCount = block.AttributeDefinitions.Count;

            // this is the list of attribute definition tags
            // remember netDxf does not allow the use of duplicate tag names, although AutoCad allows it, it is not recommended
            ICollection<string> tags = block.AttributeDefinitions.Tags;

            // we can assign values to the insert attributes
            foreach (Attribute att in insert.Attributes)
            {
                att.Value = string.Format("{0} value", att.Tag);
            }

            // optionally we can manually add the block definition to the document
            dxf.Blocks.Add(block);

            // we add the insert entity to the document, if the block associated with the block has not been added this method will do it automatically
            dxf.Entities.Add(insert);

            // also it is possible to manually add attribute definitions to a document
            AttributeDefinition def = new AttributeDefinition("AttDefOutsideBlock")
            {
                Prompt = "Enter value:",
                Value = "0",
                Color = AciColor.Blue,
                Position = new Vector3(0, 30, 0)
            };

            // we will add the attribute definition to the document just like any other entity
            dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions.Add(def);

            // now we can save our new document
            dxf.Save("CreateBlockFromDxf.dxf");

            DxfDocument load = Test("CreateBlockFromDxf.dxf");
        }

        #endregion

        #endregion
        
        #region Samples for new and modified features 2.1.0

        private static void Shape()
        {
            // create the shape style from the file where the shape definitions are stored
            ShapeStyle style = new ShapeStyle("shape.shx");
            // create the shape entity from the style where the same named "MyShape" is stored (name case is ignored)
            Shape shape = new Shape("MyShape", style);

            DxfDocument doc = new DxfDocument();
            // when a shape is added to a DxfDocument it will be checked that the style associated with the shape,
            // actually contains a shape definition with the name specified in the shape constructor, in this case "MyShape"
            // this is done reading the SHP file associated with the SHX (compiled SHP) file.
            // this SHP must be present in the same folder as the SHX or in one of the support folder defined in the DxfDocument.
            // in this case a sub-folder of the executable
            doc.SupportFolders.Add(@".\Support");
            doc.Entities.Add(shape);
            doc.Save("sample shape.dxf");

            // when loading a dxf file that contains shape entities, we need to deal with the same problem when adding a shape entity to the document.
            // we need to specified the folder or folder where to look for the SHP file
            DxfDocument loaded = DxfDocument.Load("sample shape.dxf", new[] { @".\Support" });

        }

        private static void ComplexLineType()
        {
            // complex line types might contain segments with shapes and/or text
            // when shape segments are used we need to deal with the same problem as the Shape entity,
            // we need a way to find the SHP file equivalent to the SHX file (see Shape() method)
            // in this case, for simplicity, the LIN file and the SHP file that contains the definitions for the shapes referenced by the line types 

            // create two linetypes from their definitions in the LIN file
            // this line type contains a text segment
            Linetype lt1 = Linetype.Load(@".\Support\acadiso.lin", "GAS_LINE");
            // this line type contains two shape segments
            Linetype lt2 = Linetype.Load(@".\Support\acad.lin", "BATTING");

            Line line1 = new Line(Vector2.Zero, new Vector2(10, 10));
            line1.Linetype = lt1;
            line1.LinetypeScale = 0.1;

            Line line2 = new Line(new Vector2(5,0), new Vector2(15,10));
            line2.Linetype = lt2;
            line2.LinetypeScale = 1.5;

            // create a DxfDocument specifying the support folders where we the SHP file is present.
            // the line type definitions in the LIN file that contains shape segments stores the name in it,
            // but the dxf requires to save the number associated with that shape, and it needs to be found in the SHP file 
            DxfDocument doc = new DxfDocument(new[] { @".\Support" });
            doc.Entities.Add(line1);
            doc.Entities.Add(line2);
            doc.Save("sample complex linetypes.dxf");

            // more functionality has been added for the linetypes
            // similar methods are also present in the Linetype class, the big difference is that, since the linetype list belongs to a document,
            // the DxfDocument support folders will be used to find the specified LIN file 
            // add an individual linetype from a file, it gives you the option to redefine the actual definition in case another with the same name already exists
            doc.Linetypes.AddFromFile("acad.lin", "DIVIDE", true);
            // add the all linetypes contained in the specified LIN file to the document list, 
            // it gives you the option to redefine the actual definition in case another with the same name already exists
            doc.Linetypes.AddFromFile("acad.lin", false);
            // save all linetypes defined in the DxfDocument to a file
            doc.Linetypes.Save(@"MyLinetypes.lin", true);          
            // gets the list of linetype names contained in a LIN file
            List<string> names = doc.Linetypes.NamesFromFile("acadiso.lin");

            // again same problem when loading the SHP files need to be found in one of the support folders
            DxfDocument loaded = DxfDocument.Load("sample complex linetypes.dxf", new[] { @".\Support" });

        }

        private static void TextStyle()
        {
            // now we can work with text style without specifying the associated file.
            // this is applicable only for true type fonts that are installed in the system font folder
            // when text styles are created this way the font information, in the dxf, is stored in the extended data of the style
            TextStyle style = new TextStyle("MyStyle", "Helvetica", FontStyle.Italic | FontStyle.Bold);

            // create a new text with the new style
            MText text = new MText("This is a sample text")
            {
                Position = new Vector3(5.0, 5.0, 0.0),
                Height = 5.0,
                Style = style
            };

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(text);
            doc.Save("TextStyle.dxf");

            DxfDocument loaded = DxfDocument.Load("TextStyle.dxf");
        }

        private static void ReadWriteFromStream()
        {
            // They will return true or false if the operation has been carried out successfully or not.
            // The Save(string file, DxfVersion dxfVersion) and Load(string file) methods will still raise an exception if they are unable to create the FileStream.
            // On Debug mode they will raise any exception that might occur during the whole process.
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line);
            dxf.Save("test.dxf");

            // saving to memory stream always use the default constructor, a fixed size stream will not work.
            MemoryStream memoryStream = new MemoryStream();
            if (!dxf.Save(memoryStream))
                throw new Exception("Error saving to memory stream.");
            // after the save method we need to rewind the stream to its start position
            memoryStream.Position = 0;

            bool isBinary;
            DxfVersion version1 = DxfDocument.CheckDxfFileVersion(memoryStream, out isBinary);
            // DxfDocument.CheckDxfFileVersion is a read operation therefore we will need to rewind the stream to its start position
            memoryStream.Position = 0;

            // loading from memory stream
            DxfDocument dxf2 = DxfDocument.Load(memoryStream);
            // DxfDocument.CheckDxfFileVersion is a read operation therefore we will need to rewind the stream to its start position
            memoryStream.Position = 0;

            DxfVersion version2 = DxfDocument.CheckDxfFileVersion(memoryStream, out isBinary);

            memoryStream.Close(); // once the stream is not need anymore we need to close the stream

            // saving to file stream
            FileStream fileStream = new FileStream("test fileStream.dxf", FileMode.Create);
            if (!dxf2.Save(fileStream, true))
                throw new Exception("Error saving to file stream.");

            fileStream.Close(); // you will need to close the stream manually to avoid file already open conflicts

            FileStream fileStreamLoad = new FileStream("test fileStream.dxf", FileMode.Open, FileAccess.Read);
            DxfDocument dxf3 = DxfDocument.Load(fileStreamLoad);
            fileStreamLoad.Close();

            DxfDocument dxf4 = DxfDocument.Load("test fileStream.dxf");
        }

        private static void ReadWriteFromStream2()
        {
            // Now it should be possible to hold multiple documents in the same stream.
            // The only thing we need to keep track its the start position of the documents in the stream
            long dxf1StartPosition;
            long dxf2StartPosition;

            // first document
            DxfDocument dxf1 = new DxfDocument();
            dxf1.Entities.Add(new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0)));

            // second document
            DxfDocument dxf2 = new DxfDocument();
            dxf2.Entities.Add(new Line(new Vector3(0, 100, 0), new Vector3(100, 0, 0)));

            // both documents will be stored in the same memory stream
            // saving to memory stream always use the default constructor, a fixed size stream will not work.
            MemoryStream memoryStream = new MemoryStream();

            // save first document and its start position
            dxf1StartPosition = memoryStream.Position;
            if (!dxf1.Save(memoryStream))
                throw new Exception("Error saving to memory stream.");

            // save second document and its start position
            dxf2StartPosition = memoryStream.Position;
            if (!dxf2.Save(memoryStream))
                throw new Exception("Error saving to memory stream.");

            // testing reading each document from the memory stream and saving them in external dxf files
            // we must correctly set the stream position to the beginning of each document

            // set the start position of the first document and read
            memoryStream.Position = dxf1StartPosition;
            DxfDocument doc1 = DxfDocument.Load(memoryStream);
            doc1.Save("doc1.dxf");

            // set the start position of the second document and read
            memoryStream.Position = dxf2StartPosition;
            DxfDocument doc2 = DxfDocument.Load(memoryStream);
            doc2.Save("doc2.dxf");

        }
        
        #endregion
       
        #region Samples for new and modified features 2.0.1

        private static void DimensionUserTextWithTwoLines()
        {
            DxfDocument doc = new DxfDocument();

            // You can add the code "\X" to the dimension UserText (the X must be uppercase).
            // It will split the dimension text into two lines, one above the dimension line and other under.
            // Bear in mind that only one "\X" is accepted, if there are more than one the first appearance will split the line,
            // any subsequent appearance will be shown as "\X".
            LinearDimension dim1 = new LinearDimension(new Vector2(-5, 5), new Vector2(5, -5), 6, 0);
            dim1.UserText = "My Value: <>\\XSecondLine\\XThis is still the second line";
            doc.Entities.Add(dim1);
           
            AlignedDimension dim2 = new AlignedDimension(new Vector2(10, 6), new Vector2(1, 3), 1);
            // to properly add the code use the scape sequence "\\" or use a verbatim string
            dim2.UserText = @"My Value: <>\XSecondLine";
            doc.Entities.Add(dim2);


            // For angular dimensions what AutoCad does when the code \X is present is to create a single MText with two lines and is attachment point the MiddleCenter,
            // instead of creating two MTexts, one on each side of the dimension line, as is the case of the linear dimensions.
            // In these cases the dimension style property TextOffset has no effect.
            Line line1 = new Line(new Vector2(-2, 2), new Vector2(2, -2));
            Line line2 = new Line(new Vector2(-1, -3), new Vector2(1, 3));
            Angular2LineDimension dim3 = new Angular2LineDimension(line2, line1, 4);
            dim3.UserText = "My Value: <>\\XSecondLine";
            doc.Entities.Add(dim3);

            // When AutoCad reads a dimension entity from the drawing it uses its associated block to draw it, but once the dimension is modified its block will be rebuild.
            // For unknown reasons the block of the Angular3PointDimensions is always rebuild when it is read from the file,
            // making the associated block that is present in the file obsolete.
            // If this would be the default behavior for all dimensions it would make the presence of its block unnecessary and a lot easier to draw dimensions.
            // Something similar happen with the hatches in the old formats, it use to require to build a block to define all lines of the hatch,
            // making them a lot more complex to define and the file a lot bigger than necessary.
            // This limitation is no longer present, all hatch representations are build when the drawing is loaded.
            Vector2 center1 = new Vector2(0, 0);
            Vector2 start = new Vector2(7, 1);
            Vector2 end = new Vector2(1, 7);
            Angular3PointDimension dim4 = new Angular3PointDimension(center1, start, end, 4);
            dim4.UserText = "My Value: <>\\XSecondLine";
            doc.Entities.Add(dim4);

            // for diametric, radial, and ordinate dimensions instead of creating two MText entities,
            // it will write the text in two lines within the same MText.
            // The MText attachment point for these cases is always MiddleRight or MiddleLeft, instead of TopCenter or BottomCenter,
            // this is a limitation since, at the moment, I have no way of properly measuring the length of a text.
            Vector3 center2 = new Vector3(1, 2, 0);
            double radius = 3;
            Circle circle = new Circle(center2, radius);
            DiametricDimension dim5 = new DiametricDimension(circle, 0);
            dim5.UserText = "My Value: <>\\XSecondLine";
            doc.Entities.Add(dim5);

            RadialDimension dim6 = new RadialDimension(circle, 0);
            dim6.UserText = "My Value: <>\\XSecondLine";
            doc.Entities.Add(dim6);

            OrdinateDimension dim7 = new OrdinateDimension(new Vector2(2, 1), new Vector2(1, 0), 3, OrdinateDimensionAxis.X);
            dim7.UserText = "My Value: <>\\XSecondLine";
            doc.Entities.Add(dim7);

            doc.Save("UserText.dxf");

        }

        #endregion

        #region Samples for new and modified features 2.0

        private static void Polyline3dAddVertex()
        {
            List<Vector3> vertexes = new List<Vector3>
            {
                new Vector3(-1,-1,-1),
                new Vector3(-1,1,-0.5),
                new Vector3(1,1,0),
                new Vector3(1,-1,0.5)
            };

            Polyline3D poly = new Polyline3D(vertexes);

            Block block = new Block("MyBlock");
            block.Entities.Add(poly);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(new Insert(block));
            Polyline3D poly2 = (Polyline3D) poly.Clone();
            doc.Entities.Add(poly2);

            // both polylines were added to the document now it is safe to add new vertexes to the polyline without the document failing to save.
            poly.Vertexes.Add(new Vector3(-1,-1,1));
            poly2.Vertexes.Add(new Vector3(-1, -1, 1));

            doc.Save("test.dxf");

        }

        private static void AcadTable()
        {
            // netDxf does not support tables made of rows and columns
            // they will be imported as an Insert entity
            // AutoCad uses anonymous blocks, with name "*T#", to represent tables
            DxfDocument doc = DxfDocument.Load(@"sample.dxf");
            // these are the block definitions imported from table entities
            List<Block> tableBlocks = new List<Block>();
            foreach (Block block in doc.Blocks)
            {
                if(block.Name.StartsWith("*T"))
                    tableBlocks.Add(block);
            }

            // these are the tables imported as Insert entities
            List<Insert> tables = new List<Insert>();
            foreach (Block block in tableBlocks)
            {
                // this is the list of users of the block,
                // this list should only contain one entity, the Insert that represents the table
                List<DxfObject> refs = doc.Blocks.GetReferences(block.Name);
                Debug.Assert(refs.Count == 1);
                tables.Add((Insert) refs[0]);
            }

            // Renaming and cloning anonymous blocks
            DxfDocument test = new DxfDocument();
            test.Entities.Add((EntityObject) tables[0].Clone());
            Block blk = (Block) tableBlocks[0].Clone("table");
            test.Entities.Add(new Insert(blk));
            test.Save("test.dxf");
        }

        private static void DimensionStyleOverrides()
        {
            // NOTE: Please keep in mind that I have not tested all possibilities and combinations.
            // If you find any issue or weird behavior with them let me know.
            DxfDocument doc = new DxfDocument();
            doc.DrawingVariables.LwDisplay = true;

            DimensionStyle style = new DimensionStyle("MyStyle");
            // the style properties DIMBLK and DIMSAH has been deleted, now they will be handle automatically when the style DimArrow1 and DimArrow2 point to the same block,
            // remember two blocks are considered equal if both have the same name
            // sample:
            style.DimArrow1 = DimensionArrowhead.Dot;
            style.DimArrow2 = style.DimArrow1;
            style.LeaderArrow = DimensionArrowhead.Box;
            doc.DimensionStyles.Add(style);
            doc.DrawingVariables.DimStyle = style.Name;
            
            Line line1 = new Line(new Vector2(-5, 0), new Vector2(5, 0))
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            doc.Entities.Add(line1);

            // any style override added to the list will override the default value defined in the dimension or leader style.
            // they work as way as the DimensionStyle and there is one DimensionStyleOverrideType per each DimensionStyle property
            // there is one exception:
            // the dimension style properties DIMBLK and DIMSAH are not available.
            // the overrides always make use of the DIMBLK1 and DIMBLK2 setting the DIMSAH to true even when both arrow ends are the same.
            AlignedDimension dim = new AlignedDimension(line1, 2.5);
            // we can add a dimension style override from its type and value, internally a new DimensionStyleOverride will be created from both arguments
            dim.StyleOverrides.Add(DimensionStyleOverrideType.DimLineColor, AciColor.Blue);
            // or creating the DimensionStyleOverride first and adding it
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ArrowSize, style.ArrowSize*5.0));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Off, true));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine2Off, true));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimPrefix, "My value "));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimSuffix, "mm"));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DecimalSeparator, ':'));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressLinearLeadingZeros, true));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.SuppressLinearTrailingZeros, false));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine1Off, true));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLine2Off, true));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, null));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow2, DimensionArrowhead.ArchitecturalTick));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLinetype, Linetype.Dashed));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineLineweight, Lineweight.W140));
            dim.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimAngularUnits, AngleUnitType.Radians));

            doc.Entities.Add(dim);

            Leader leader = new Leader("This is a line", new List<Vector2> {new Vector2(0, 0), new Vector2(2.5, -2.5)});
            leader.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.LeaderArrow, DimensionArrowhead.BoxFilled));
            leader.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineColor, AciColor.Red));
            doc.Entities.Add(leader);

            doc.Save("test.dxf");

            // modifying existing style overrides
            DxfDocument dxf = DxfDocument.Load("test.dxf");
            Dimension dim1 = dxf.Entities.Dimensions.ElementAt(0);
            // remove existing style overrides
            dim1.StyleOverrides.Remove(DimensionStyleOverrideType.DimPrefix);
            dim1.StyleOverrides.Remove(DimensionStyleOverrideType.DecimalSeparator);

            // modify an existing style override, if you are sure that type already exists
            dim1.StyleOverrides[DimensionStyleOverrideType.ExtLine1Off] = new DimensionStyleOverride(DimensionStyleOverrideType.ExtLine1Off, false);

            // to play safe we can check if the style override already exists
            DimensionStyleOverride styleOverride;
            if (dim1.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ExtLine2Off, out styleOverride))
                dim1.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, false);

            // but we can always try to delete the existing one before adding our new style override
            // trying to add two style overrides of the same type it not allowed
            dim1.StyleOverrides.Remove(DimensionStyleOverrideType.DimArrow1);
            dim1.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimArrow1, DimensionArrowhead.DotBlank));

            // if we modify the style overrides of an existing dimension we need to update it, so the block that represents is also updated with the new changes 
            dim1.Update();

            Leader leader1 = dxf.Entities.Leaders.ElementAt(0);
            // we can always remove all exiting style overrides to add new ones
            leader1.StyleOverrides.Clear();
            leader1.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.DimLineColor, AciColor.Yellow));

            dxf.Save("test compare.dxf");

            dxf = DxfDocument.Load("test compare.dxf");
        }

        private static void ResetLeaderAnnotationPosition()
        {
            List<Vector2> vertexes1 = new List<Vector2>
            {
                new Vector2(0, 0), new Vector2(5, 5)
            };

            Leader leader1 = new Leader("Sample annotation", vertexes1);
            //leader1.Normal = new Vector3(1);
            //leader1.Update(false);

            Leader leader2 = (Leader)leader1.Clone();
            //leader2.Normal = new Vector3(1,1,0);
            // a text annotation with style
            DimensionStyle style = new DimensionStyle("MyStyle")
            {
                DimLineColor = AciColor.Green,
                TextColor = AciColor.Blue,
                LeaderArrow = DimensionArrowhead.DotBlank,
                DimScaleOverall = 2.0
            };
            leader2.Style = style;
            // We need to call manually the method Update if the annotation position is modified,
            // or the Leader properties like Style, Normal, Elevation, Annotation, TextVerticalPosition, and/or Offset.
            // normally the leader hook (last leader vertex) is modified according to the position of the annotation.
            ((MText)leader2.Annotation).Position = new Vector3(5,10,0);
            //((MText)leader2.Annotation).Position = MathHelper.Transform(new Vector3(5, 10, 0), leader2.Normal, CoordinateSystem.Object, CoordinateSystem.World);
            leader2.Update(false);

            Leader leader3 = (Leader)leader1.Clone();
            leader3.Hook = new Vector2(-5,5);
            // if we need to move the annotation position according to the leader hook (last leader vertex),
            // we will set the Update argument to true, otherwise the leader hook will be modified according to the annotation position
            leader3.Update(true);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(leader1);
            doc.Entities.Add(leader2);
            doc.Entities.Add(leader3);
            doc.Save("test.dxf");

            DxfDocument dxf = DxfDocument.Load("test.dxf");
            dxf.Save("test compare.dxf");

        }

        #endregion

        #region Samples for new and modified features 1.1.2

        private static void LinearDimensionTest()
        {
            DxfDocument doc = new DxfDocument();

            Vector3 p1 = new Vector3(-5, 5, 0);
            Vector3 p2 = new Vector3(5, -5, 0);

            //Vector3 p1 = new Vector3(-5, -5, 0);
            //Vector3 p2 = new Vector3(5, 5, 0);

            Line line1 = new Line(p1, p2)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            doc.Entities.Add(line1);

            double offset = 6;
            LinearDimension dimX1 = new LinearDimension(line1, offset, 0);
            dimX1.SetDimensionLinePosition(new Vector2(6, 6));
            dimX1.UserText = "My Value: <>\\XSecondLine";
            LinearDimension dimY1 = new LinearDimension(line1, offset, 90);
            LinearDimension dim5 = new LinearDimension(line1, offset, 180);
            LinearDimension dim6 = new LinearDimension(line1, offset, 270);

            doc.Entities.Add(dimX1);
            doc.Entities.Add(dimY1);
            doc.Entities.Add(dim5);
            doc.Entities.Add(dim6);
            doc.Save("test.dxf");

            doc = DxfDocument.Load("test.dxf");
            foreach (Dimension d in doc.Entities.Dimensions)
            {
                d.Update();
            }
            doc.Entities.Add((EntityObject) dimX1.Clone());
            doc.Entities.Add((EntityObject) dimY1.Clone());
            doc.Entities.Add((EntityObject) dim5.Clone());
            doc.Entities.Add((EntityObject) dim6.Clone());
            doc.Save("test.dxf");
        }

        private static void AlignedDimensionTest()
        {
            DxfDocument doc = new DxfDocument();
            double offset = 1;
            Line line = new Line(new Vector3(10, 6, 0), new Vector3(1, 3, 0));
            AlignedDimension dim = new AlignedDimension(line, -offset);

            Line line1 = new Line(new Vector3(0, 0, 0), new Vector3(10, 0, 0));
            AlignedDimension dim1 = new AlignedDimension(line1, offset);
            dim1.SetDimensionLinePosition(new Vector2(20, -2));

            Line line2 = new Line(new Vector3(0, 10, 0), new Vector3(0, 0, 0));
            AlignedDimension dim2 = new AlignedDimension(line2, offset);

            Line line3 = new Line(new Vector3(0, 0, 0), new Vector3(0, 0, 10));
            AlignedDimension dim3 = new AlignedDimension(line3, offset, Vector3.UnitY);
            dim3.Elevation = 1;

            doc.Entities.Add(new List<EntityObject> {line, dim, line1, dim1, line2, dim2, line3, dim3});

            doc.Save("test1.dxf");

            doc = DxfDocument.Load("test1.dxf");
            foreach (Dimension d in doc.Entities.Dimensions)
            {
                d.Update();
            }
            doc.Entities.Add((EntityObject) dim.Clone());
            doc.Entities.Add((EntityObject) dim1.Clone());
            doc.Entities.Add((EntityObject) dim2.Clone());
            doc.Entities.Add((EntityObject) dim3.Clone());
            doc.Save("test2.dxf");
        }

        private static void Angular2LineDimensionTest()
        {
            double offset = 4;

            Layer layer = new Layer("Layer1") {Color = AciColor.Blue};
            Vector2 start1 = new Vector2(-2, 2);
            Vector2 end1 = new Vector2(2, -2);
            Vector2 start2 = new Vector2(-1, -3);
            Vector2 end2 = new Vector2(1, 3);

            Vector2 p1 = new Vector2(2, 0);
            Vector2 p2 = new Vector2(0, 2);
            Vector2 p3 = new Vector2(-2, 0);
            Vector2 p4 = new Vector2(0, -2);

            Line line1 = new Line(start1, end1) {Layer = layer};
            Line line2 = new Line(start2, end2) {Layer = layer};

            Angular2LineDimension dim1 = new Angular2LineDimension(line2, line1, offset);
            dim1.SetDimensionLinePosition(p1);

            Angular2LineDimension dim2 = new Angular2LineDimension(line2, line1, offset);
            dim2.SetDimensionLinePosition(p2);

            Angular2LineDimension dim3 = new Angular2LineDimension(line1, line2, offset);
            dim3.SetDimensionLinePosition(p3);

            Angular2LineDimension dim4 = new Angular2LineDimension(line1, line2, offset);
            dim4.SetDimensionLinePosition(p4);

            DxfDocument dxf = new DxfDocument();
            dxf.BuildDimensionBlocks = true;
            dxf.Entities.Add(line1);
            dxf.Entities.Add(line2);
            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim2);
            dxf.Entities.Add(dim3);
            dxf.Entities.Add(dim4);
            dxf.Save("test.dxf");

            //dxf = DxfDocument.Load("test1.dxf");
            //foreach (var d in dxf.Dimensions)
            //    d.Update();
            //dxf.Entities.Add((EntityObject) dim1.Clone());
            //dxf.Entities.Add((EntityObject) dim2.Clone());
            //dxf.Entities.Add((EntityObject) dim3.Clone());
            //dxf.Entities.Add((EntityObject) dim4.Clone());
            //dxf.Save("test2.dxf");
        }

        private static void Angular3PointDimensionTest()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.BuildDimensionBlocks = true;
            Vector2 center = new Vector2(0, 0);
            Vector2 start = new Vector2(7, 1);
            Vector2 end = new Vector2(1, 7);
            Angular3PointDimension dim1 = new Angular3PointDimension(center,start,end,  8);
            //dim1.SetDimensionLinePosition(new Vector2(2, 2));
            //dim1.SetDimensionLinePosition(new Vector2(-2, 0));

            Arc arc = new Arc(center, 2, Vector2.Angle(start)*MathHelper.RadToDeg, Vector2.Angle(end)*MathHelper.RadToDeg) {Layer = new Layer("Layer1") {Color = AciColor.Blue} };
            Angular3PointDimension dim2 = new Angular3PointDimension(arc, 4);
            dxf.Entities.Add(arc);
            dxf.Entities.Add(dim1);
            //dxf.Entities.Add(dim2);
            dxf.Save("test1.dxf");

            //dxf = DxfDocument.Load("test1.dxf");

            //foreach (var d in dxf.Dimensions)
            //    d.Update();

            //dxf.Entities.Add((EntityObject) dim1.Clone());
            //dxf.Entities.Add((EntityObject) dim2.Clone());
            //dxf.Save("test2.dxf");
        }

        private static void DiametricDimensionTest()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.BuildDimensionBlocks = true;
            Vector2 center = new Vector2(1, 2);
            double radius = 3;
            Circle circle = new Circle(center, radius);
            double offset = 1;
            DiametricDimension dim1 = new DiametricDimension(circle, 0);
            //dim1.TextReferencePoint = new Vector2(2, 4);
            //dim1.SetDimensionLinePosition(new Vector2(1, 2));
            //dim1.Update();

            DiametricDimension dim1p = (DiametricDimension) dim1.Clone();
            //dim1p.Update();
            DiametricDimension dim2 = new DiametricDimension(circle, 45);
            DiametricDimension dim3 = new DiametricDimension(circle, 90);
            DiametricDimension dim4 = new DiametricDimension(circle, 120);
            DiametricDimension dim5 = new DiametricDimension(circle, 180);
            DiametricDimension dim6 = new DiametricDimension(circle, 220);
            DiametricDimension dim7 = new DiametricDimension(circle, 270);
            DiametricDimension dim8 = new DiametricDimension(circle, 330);

            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim1p);
            dxf.Entities.Add(dim2);
            dxf.Entities.Add(dim3);
            dxf.Entities.Add(dim4);
            dxf.Entities.Add(dim5);
            dxf.Entities.Add(dim6);
            dxf.Entities.Add(dim7);
            dxf.Entities.Add(dim8);
            dxf.Save("test1.dxf");

            dxf = DxfDocument.Load("test1.dxf");
            dxf.BuildDimensionBlocks = true;
            foreach (var d in dxf.Entities.Dimensions)
            {
                d.Update();
            }
            dxf.Entities.Add((EntityObject)dim1.Clone());
            dxf.Entities.Add((EntityObject)dim2.Clone());
            dxf.Entities.Add((EntityObject)dim3.Clone());
            dxf.Entities.Add((EntityObject)dim4.Clone());
            dxf.Entities.Add((EntityObject)dim5.Clone());
            dxf.Entities.Add((EntityObject)dim6.Clone());
            dxf.Entities.Add((EntityObject)dim7.Clone());
            dxf.Entities.Add((EntityObject)dim8.Clone());

            dxf.Save("test2.dxf");
        }

        private static void RadialDimensionTest()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.BuildDimensionBlocks = true;
            Vector3 center = new Vector3(1, 2, 0);
            double radius = 3;
            Circle circle = new Circle(center, radius);

            RadialDimension dim1 = new RadialDimension(circle, 0);
            //dim1.SetDimensionLinePosition(new Vector2(5, 2));

            RadialDimension dim2 = new RadialDimension(circle, 45);
            RadialDimension dim3 = new RadialDimension(circle, 90);
            RadialDimension dim4 = new RadialDimension(circle, 120);
            RadialDimension dim5 = new RadialDimension(circle, 180);
            RadialDimension dim6 = new RadialDimension(circle, 220);
            RadialDimension dim7 = new RadialDimension(circle, 270);
            RadialDimension dim8 = new RadialDimension(circle, 330);

            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim1);
            //dxf.Entities.Add(dim2);
            //dxf.Entities.Add(dim3);
            //dxf.Entities.Add(dim4);
            //dxf.Entities.Add(dim5);
            //dxf.Entities.Add(dim6);
            //dxf.Entities.Add(dim7);
            //dxf.Entities.Add(dim8);
            dxf.Save("test1.dxf");

            dxf = DxfDocument.Load("test1.dxf");
            dxf.BuildDimensionBlocks = true;
            foreach (var d in dxf.Entities.Dimensions)
            {
                d.Update();
            }
            dxf.Entities.Add((EntityObject) dim1.Clone());
            dxf.Entities.Add((EntityObject)dim2.Clone());
            dxf.Entities.Add((EntityObject)dim3.Clone());
            dxf.Entities.Add((EntityObject)dim4.Clone());
            dxf.Entities.Add((EntityObject)dim5.Clone());
            dxf.Entities.Add((EntityObject)dim6.Clone());
            dxf.Entities.Add((EntityObject)dim7.Clone());
            dxf.Entities.Add((EntityObject)dim8.Clone());

            dxf.Save("test2.dxf");
        }

        private static void OrdinateDimensionTest()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.BuildDimensionBlocks = true;

            Vector2 origin = new Vector2(2, 1);
            Vector2 refX = new Vector2(1, 0);
            Vector2 refY = new Vector2(0, 2);
            double length = 3;
            double angle = 30;
            DimensionStyle myStyle = CreateDimStyle();

            OrdinateDimension dimX1 = new OrdinateDimension(origin, origin + refX, length, OrdinateDimensionAxis.X, 0, myStyle);
            OrdinateDimension dimX2 = new OrdinateDimension(origin, origin + refX, length, OrdinateDimensionAxis.X, angle, myStyle);
            OrdinateDimension dimY1 = new OrdinateDimension(origin, origin + refY, length, OrdinateDimensionAxis.Y, 0, myStyle);
            OrdinateDimension dimY2 = new OrdinateDimension(origin, origin + refY, length, OrdinateDimensionAxis.Y, angle, myStyle);

            //dxf.Entities.Add(dimX1);
            dxf.Entities.Add(dimX2);
            //dxf.Entities.Add(dimY1);
            dxf.Entities.Add(dimY2);

            Line lineX = new Line(origin, origin + 5*Vector2.UnitX);
            Line lineY = new Line(origin, origin + 5*Vector2.UnitY);

            Vector2 point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad);
            Line lineXRotate = new Line(origin, new Vector2(point.X, point.Y));

            point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad + MathHelper.HalfPI);
            Line lineYRotate = new Line(origin, new Vector2(point.X, point.Y));

            //dxf.Entities.Add(lineX);
            //dxf.Entities.Add(lineY);
            dxf.Entities.Add(lineXRotate);
            dxf.Entities.Add(lineYRotate);

            dxf.Save("test1.dxf");

            dxf = DxfDocument.Load("test1.dxf");
            //foreach (var d in dxf.Dimensions)
            //{
            //    d.Update();
            //}
            //dxf.Entities.Add((EntityObject) dimX1.Clone());
            //dxf.Entities.Add((EntityObject)dimX2.Clone());
            //dxf.Entities.Add((EntityObject)dimY1.Clone());
            //dxf.Entities.Add((EntityObject)dimY2.Clone());

            dxf.Save("test2.dxf");
        }

        #endregion

        #region Samples for new and modified features 1.1.0

        private static void WipeoutEntity()
        {
            Line line1 = new Line(new Vector2(-1, -1), new Vector2(1, 1));
            Line line2 = new Line(new Vector2(-1, 1), new Vector2(1, -1));
            Circle circle = new Circle(Vector2.Zero, 0.5);

            Wipeout wipeout1 = new Wipeout(new Vector2(-1.5, -0.25), new Vector2(1.5, 0.25)); // a rectangular wipeout defined from two opposite corners
            //Wipeout wipeout1 = new Wipeout(-1.5, -0.25, 3.0, 0.5); // a rectangular wipeout defined by its bottom-left corner and its width and height
            //Wipeout wipeout1 = new Wipeout(new List<Vector2>{new Vector2(-1.5, 0.25), new Vector2(1.5, 0.25), new Vector2(1.5, -0.25), new Vector2(-1.5, -0.25)}); // a polygonal wipeout

            List<Vector2> vertexes = new List<Vector2>
            {
                new Vector2(-30, 30),
                new Vector2(-20, 60),
                new Vector2(-10, 40),
                new Vector2(10, 70),
                new Vector2(30, 20)
            };

            Wipeout wipeout2 = new Wipeout(vertexes);
            // optionally you can set the normal and elevation
            //wipeout1.Normal = new Vector3(1.0);
            //wipeout1.Elevation = 10;
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(line1);
            doc.Entities.Add(line2);
            doc.Entities.Add(circle);
            doc.Entities.Add(wipeout1);
            doc.Entities.Add(wipeout2);

            doc.Save("wipeout.dxf");

            DxfDocument test = DxfDocument.Load("wipeout.dxf");
            test.Save("test.dxf");
        }

        private static void ToleranceEntity()
        {
            ToleranceEntry entry = new ToleranceEntry
            {
                GeometricSymbol = ToleranceGeometricSymbol.Symmetry,
                Tolerance1 = new ToleranceValue(true, "12.5", ToleranceMaterialCondition.Maximum)
            };
            Tolerance tolerance = new Tolerance();
            tolerance.Entry1 = entry;
            tolerance.Rotation = 30;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(tolerance);
            doc.Save("Tolerance.dxf");

            DxfDocument dxf = DxfDocument.Load("Tolerance.dxf");
            dxf.Save("Test.dxf");
        }

        private static void LeaderEntity()
        {
            // a basic text annotation
            List<Vector2> vertexes1 = new List<Vector2>();
            vertexes1.Add(new Vector2(0, 0));
            vertexes1.Add(new Vector2(-5, 5));
            Leader leader1 = new Leader("Sample annotation", vertexes1);
            //leader1.TextVerticalPosition = LeaderTextVerticalPosition.Above;
            //((MText) leader1.Annotation).Position = new Vector3(5, 5, 0);
            leader1.Update(false);

            //leader1.TextVerticalPosition = LeaderTextVerticalPosition.Centered;
            //leader1.Offset = new Vector2(0, -0.5);
            // We need to call manually the method Update if the annotation position is modified,
            // or the Leader properties like Style, Normal, Elevation, Annotation, TextVerticalPosition, and/or Offset.
            //leader1.Update(true);


            //DxfDocument compare = DxfDocument.Load("Leader compare.dxf");
            //Leader l = compare.Entities.Leaders.ElementAt(0);

            // leader not in the XY plane
            Leader cloned = (Leader) leader1.Clone();
            cloned.Normal = new Vector3(1);
            cloned.Elevation = 5;
            cloned.Update(true);

            // a text annotation with style
            DimensionStyle style = new DimensionStyle("MyStyle");
            style.DimLineColor = AciColor.Green;
            style.TextColor = AciColor.Blue;
            style.LeaderArrow = DimensionArrowhead.DotBlank;
            style.DimScaleOverall = 2.0;

            List<Vector2> vertexes2 = new List<Vector2>();
            vertexes2.Add(new Vector2(0, 0));
            vertexes2.Add(new Vector2(-5, -5));
            Leader leader2 = new Leader("Sample annotation", vertexes2, style);
            ((MText) leader2.Annotation).AttachmentPoint = MTextAttachmentPoint.MiddleLeft;
            leader2.StyleOverrides.Add(new DimensionStyleOverride(DimensionStyleOverrideType.TextVerticalPlacement, DimensionStyleTextVerticalPlacement.Centered));
            leader2.Normal = new Vector3(1);
            leader2.Elevation = 5;
            leader2.Update(true);

            // a tolerance annotation,
            // its hook point will always appear at the left since I have no way of measuring the real length of a tolerance,
            // and it has no alignment options as the MText does.
            List<Vector2> vertexes3 = new List<Vector2>();
            vertexes3.Add(new Vector2(0));
            vertexes3.Add(new Vector2(5, -5));
            vertexes3.Add(new Vector2(7.5, -5));
            ToleranceEntry entry = new ToleranceEntry
            {
                GeometricSymbol = ToleranceGeometricSymbol.Symmetry,
                Tolerance1 = new ToleranceValue(true, "12.5", ToleranceMaterialCondition.Maximum)
            };
            Leader leader3 = new Leader(entry, vertexes3);

            // a block annotation
            Block block = new Block("BlockAnnotation");
            block.Entities.Add(new Line(new Vector2(-1, -1), new Vector2(1, 1)));
            block.Entities.Add(new Line(new Vector2(-1, 1), new Vector2(1, -1)));
            block.Entities.Add(new Circle(Vector2.Zero, 0.5));

            List<Vector2> vertexes4 = new List<Vector2>();
            vertexes4.Add(new Vector2(0));
            vertexes4.Add(new Vector2(-5, -5));
            vertexes4.Add(new Vector2(-7.5, -5));
            Leader leader4 = new Leader(block, vertexes4);
            // change the leader offset to move the leader hook (the last vertex of the leader vertexes list) in relation to the annotation position.
            //leader4.Offset = new Vector2(1, 1);
            leader4.Update(true);

            // add entities to the document
            DxfDocument doc = new DxfDocument();
            //doc.Entities.Add(cloned);
            //doc.Entities.Add(leader1);
            //doc.Entities.Add(leader2);
            //doc.Entities.Add(leader3);
            doc.Entities.Add(leader4);
            doc.Save("Leader.dxf");

            DxfDocument test = DxfDocument.Load("Leader.dxf");
            test.Save("test.dxf");
        }

        private static void UnderlayEntity()
        {
            UnderlayDgnDefinition underlayDef1 = new UnderlayDgnDefinition("DgnUnderlay.dgn");
            Underlay underlay1 = new Underlay(underlayDef1);
            underlay1.Normal = new Vector3(1, 0, 0);
            underlay1.Position = new Vector3(0, 1, 0);
            underlay1.Scale = new Vector2(0.001);

            UnderlayDwfDefinition underlayDef2 = new UnderlayDwfDefinition("DwfUnderlay.dwf");
            Underlay underlay2 = new Underlay(underlayDef2);
            underlay2.Rotation = 45;
            underlay2.Scale = new Vector2(0.01);

            UnderlayPdfDefinition underlayDef3 = new UnderlayPdfDefinition("PdfUnderlay.pdf");
            underlayDef3.Page = "3";
            Underlay underlay3 = new Underlay(underlayDef3);

            DxfDocument doc = new DxfDocument(DxfVersion.AutoCad2013);
            doc.Entities.Add(underlay1);
            doc.Entities.Add(underlay2);
            doc.Entities.Add(underlay3);
            doc.Save("UnderlayEntity.dxf");

            DxfDocument load = DxfDocument.Load("UnderlayEntity.dxf");
            load.Save("UnderlayEntity2.dxf");
        }

        private static void SplineFitPoints()
        {
            // this will be the list of fit points, the resulting spline will pass through them.
            List<Vector3> points = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(5, 5, 0),
                new Vector3(10, 0, 0),
                new Vector3(15, 5, 0),
                new Vector3(20, 0, 0),
                new Vector3(0, 0, 0)
            };

            // splines crated from a set of fit points are cubic curves (degree 3) and non periodic.
            // to close the spline just repeat the last fit point.
            // note: at the moment splines created this way cannot be used as a boundary path in a hatch, or converted to a polyline or to a list of vertexes,
            // the control point information is not calculated when an spline is created using a list of fit points.
            Spline spline = new Spline(points);

            // to make the spline continuous at the end points we can adjust the start and end tangents.
            spline.StartTangent = new Vector3(0, 1, 0);
            spline.EndTangent = new Vector3(0, 1, 0);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(spline);

            Spline cloned = (Spline) spline.Clone();
            cloned.Reverse();
            doc.Entities.Add(cloned);


            // and this is a spline created with control points
            Vector3[] ctrlPoints =
            {
                new Vector3(0, 0, 0),
                new Vector3(25, 50, 50),
                new Vector3(50, 0, 100),
                new Vector3(75, 50, 50),
                new Vector3(100, 0, 0)
            };

            double[] weights =
            {
                1.0, 2.0, 3.0, 4.0, 5.0
            };

            // the constructor will generate a uniform knot vector 
            Spline openSpline = new Spline(ctrlPoints, weights, 3);
            Spline cloned2 = (Spline) openSpline.Clone();

            cloned2.Reverse();
            doc.Entities.Add(openSpline);
            doc.Entities.Add(cloned2);

            doc.Save("SplineFitPoints.dxf");

            DxfDocument dxf = DxfDocument.Load("SplineFitPoints.dxf");
            dxf.Save("SplineFitPoints.dxf");
        }

        private static void ImageClippingBoundary()
        {
            string imgFile = @".\img\image02.jpg";
            System.Drawing.Image img = System.Drawing.Image.FromFile(imgFile);
            ImageDefinition imageDef = new ImageDefinition("MyImage", imgFile, img.Width, img.HorizontalResolution, img.Height, img.VerticalResolution, ImageResolutionUnits.Inches);

            imageDef.ResolutionUnits = ImageResolutionUnits.Centimeters;
            double width = imageDef.Width/imageDef.HorizontalResolution;
            double height = imageDef.Height/imageDef.VerticalResolution;
            Image image = new Image(imageDef, new Vector2(0, 0), width, height);
            image.Rotation = 30;


            // the coordinates of the clipping boundary are relative to the image with its actual dimensions and not to the width and height of its definition.
            // this clipping boundary will only show the middle center of the image.
            double x = width/4;
            double y = height/4;
            ClippingBoundary clip = new ClippingBoundary(x, y, 2*x, 2*y);
            image.ClippingBoundary = clip;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(image);
            doc.Save("image.dxf");

            DxfDocument test = DxfDocument.Load("image.dxf");
            test.Save("test.dxf");
        }

        #endregion

        #region Samples for new and modified features 1.0.2

        private static void AssociativeHatches()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            // optionally you can the normal of the polyline, by default it is the UnitZ vector
            //poly.Normal = new Vector3(1.0);
            poly.IsClosed = true;


            HatchBoundaryPath boundary = new HatchBoundaryPath(new List<EntityObject> {poly});
            HatchPattern pattern = HatchPattern.Line;
            pattern.Scale = 10;
            pattern.Angle = 45;

            // the hatch boundary can be set in the hatch constructor or it can be added later
            //Hatch hatch = new Hatch(pattern, new[]{boundary}, true);

            Hatch hatch = new Hatch(pattern, true);
            // you will need to manually set the hatch normal to the boundary normal if it is not the UnitZ,
            // to work properly all boundary entities must belong to the same plane
            //hatch.Normal = poly.Normal;

            // the hatch boundary can be set in the hatch constructor or it can be added later, remember hatches with no boundaries will not be saved
            hatch.BoundaryPaths.Add(boundary);
            Circle circle = new Circle(Vector2.Zero, 5);
            // all boundary entities should have the same normal, by default it is the UnitZ
            // the hatch will not handle the normals of the different boundary path, you will have to make sure they all lay on the same plane
            //circle.Normal = poly.Normal;

            hatch.BoundaryPaths.Add(new HatchBoundaryPath(new List<EntityObject> {circle}));
            // when an associative hatch is added to a document the referenced boundary entities will be added too
            dxf.Entities.Add(hatch);
            dxf.Save("Hatch.dxf");


            DxfDocument dxf2 = DxfDocument.Load("Hatch.dxf");
            // you can remove boundaries from a hatch
            dxf2.Entities.Hatches.ElementAt(0).BoundaryPaths.Remove(dxf2.Entities.Hatches.ElementAt(0).BoundaryPaths[1]);
            // and add new ones
            Polyline2D p = new Polyline2D();
            p.Vertexes.Add(new Polyline2DVertex(-20, -20));
            p.Vertexes.Add(new Polyline2DVertex(20, -20));
            p.Vertexes.Add(new Polyline2DVertex(20, 20));
            p.Vertexes.Add(new Polyline2DVertex(-20, 20));
            p.IsClosed = true;
            dxf2.Entities.Hatches.ElementAt(0).BoundaryPaths.Add(new HatchBoundaryPath(new List<EntityObject> {p}));
            dxf2.Save("Hatch add and remove boundaries.dxf");


            DxfDocument dxf3 = DxfDocument.Load("Hatch.dxf");
            // unlinking the boundary entities from a hatch will not automatically remove them from the document, you can use the returned list to delete them
            // unlinking the boundary will make the hatch non-associative 
            List<EntityObject> oldBoundary = dxf3.Entities.Hatches.ElementAt(0).UnLinkBoundary();
            dxf3.Entities.Remove(oldBoundary);

            // we can recreate the hatch boundary and optionally linking it, thus making it associative,
            // if the hatch is associative and belongs to a document the new entities will also be automatically added to the same document
            List<EntityObject> newBoundary = dxf3.Entities.Hatches.ElementAt(0).CreateBoundary(true);

            dxf3.Save("Hatch new contour.dxf");

            DxfDocument dxf4 = DxfDocument.Load("Hatch.dxf");
            // if the hatch is associative, it is possible to modify the entities that make the boundary
            // for non-associative the list of entities will contain zero items
            if (dxf4.Entities.Hatches.ElementAt(0).Associative)
            {
                // this will only work for associative hatches
                HatchBoundaryPath path = dxf4.Entities.Hatches.ElementAt(0).BoundaryPaths[0];
                Polyline2D entity = (Polyline2D) path.Entities[0];
                entity.Vertexes[2].Position = new Vector2(15, 15);
                // after modifying the boundary entities, it is necessary to rebuild the edges
                path.Update();
                dxf4.Save("Hatch change boundary.dxf");
            }
        }

        private static void TraceEntity()
        {
            // The Trace entity behaves exactly as the Solid, they have the same properties and have the same graphical representation.
            // They are a different entity since in AutoCad they show as two distinct types of objects.
            // In any case, it is recommended to always use the Solid in its place.
            Vector2 a = new Vector2(-1, -1);
            Vector2 b = new Vector2(1, -1);
            Vector2 c = new Vector2(-1, 1);
            Vector2 d = new Vector2(1, 1);

            Trace trace = new Trace(a, b, c, d);
            trace.Normal = new Vector3(1, 1, 0);
            trace.Elevation = 2;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(trace);
            doc.Save("TraceEntity.dxf");
        }

        private static void SolidEntity()
        {
            // The solid vertexes are expressed in OCS (object coordinate system)
            // Now they are stored as Vector2 to force all vertexes to lay on a plane, this is similar as how the Polyline2D works.
            // The Z coordinate is controlled by the elevation property of the Solid.
            Vector2 a = new Vector2(-1, -1);
            Vector2 b = new Vector2(1, -1);
            Vector2 c = new Vector2(-1, 1);
            Vector2 d = new Vector2(1, 1);

            Solid solid = new Solid(a, b, c, d);
            solid.Normal = new Vector3(1, 1, 0);

            solid.Elevation = 2;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(solid);
            doc.Save("SolidEntity.dxf");
        }

        #endregion

        #region Samples for new and modified features 1.0.0

        public static void ModifyingDocumentEntities()
        {
            Layer layer1 = new Layer("layer1");
            Layer layer2 = new Layer("layer2");
            Layer layer3 = new Layer("layer3");

            Linetype linetype1 = Linetype.Dot;
            Linetype linetype2 = Linetype.Dashed;

            Line line = new Line(Vector2.Zero, Vector2.UnitX);
            line.Layer = layer1;
            line.Linetype = linetype1;

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(line);

            // if the layer does not exist in the document it will be added automatically
            line.Layer = layer2;
            Debug.Assert(ReferenceEquals(line.Layer, doc.Layers[line.Layer.Name]), "References are not equal.");

            // you can always add it first
            doc.Layers.Add(layer3);
            // layer3 is defined in the document
            line.Layer = layer3;
            Debug.Assert(ReferenceEquals(line.Layer, doc.Layers[line.Layer.Name]), "References are not equal.");

            // same thing is applicable to line types
            line.Linetype = linetype2;
            Debug.Assert(ReferenceEquals(line.Linetype, doc.Linetypes[line.Linetype.Name]), "References are not equal.");

            doc.Save("entity.dxf");

            // it is also possible to rename table objects
            layer1.Name = "New layer1 name";
            linetype1.Name = "DotDot";

            // this operation is illegal, you cannot rename reserved table objects.
            //doc.Layers[Layer.DefaultName].Name = "NewName";

            doc.Save("test.dxf");
        }

        public static void ModifyingBlockProperties()
        {
            DxfDocument doc = new DxfDocument();
            doc.DrawingVariables.InsUnits = DrawingUnits.Centimeters;
            Line existingLine = new Line(new Vector2(-10, 10), new Vector2(10, -10));
            doc.Entities.Add(existingLine);

            AttributeDefinition attDef4 = new AttributeDefinition("MyAttribute4");
            attDef4.Value = "MyValue4";
            attDef4.Alignment = TextAlignment.TopCenter;
            Block block = new Block("MyBlock", null, new List<AttributeDefinition> {attDef4});
            block.Record.Units = DrawingUnits.Millimeters;

            // this is incorrect we cannot add an entity that belongs to a document when the block does not belong to anyone.
            //block.Entities.Add(existingLine);
            doc.Blocks.Add(block);
            // you cannot add an entity that belongs to a different document or block. Clone it instead or removed first from its previous owner.

            existingLine.Owner.Entities.Remove(existingLine);
            block.Entities.Add(existingLine);

            // now we can modify the block properties even if it has been already added to the document
            Line line = new Line(new Vector2(-10, -10), new Vector2(10, 10));

            // when new entities that do not belong to anyone are added to an existing block, they will also be added to the document
            block.Entities.Add(line);

            DxfDocument doc2 = new DxfDocument();
            Circle circle = new Circle(Vector2.Zero, 5);
            doc2.Entities.Add(circle);

            // this is incorrect the circle already belongs to another document
            //block.Entities.Add(circle);
            // we need to clone it first
            Circle circle2 = (Circle) circle.Clone();
            circle2.Radius = 2.5;
            block.Entities.Add(circle2);

            //you could also remove circle2 from doc2 and add it to the block
            doc2.Entities.Remove(circle);
            block.Entities.Add(circle);

            AttributeDefinition attDef = new AttributeDefinition("MyAttribute1");
            attDef.Value = "MyValue1";
            block.AttributeDefinitions.Add(attDef);

            // the same that is applicable to entities is also true to attribute definitions
            AttributeDefinition attDef2 = new AttributeDefinition("MyAttribute2");
            attDef2.Value = "MyValue2";
            attDef2.Alignment = TextAlignment.BaselineRight;
            block.AttributeDefinitions.Add(attDef2);

            Insert ins = new Insert(block);
            doc.Entities.Add(ins);

            // if the insert has been added to a document, any new attribute definitions added to the block will not be reflected in the insert
            // this mimics the behavior in AutoCad
            AttributeDefinition attDef3 = new AttributeDefinition("MyAttribute3");
            attDef3.Value = "MyValue3";
            attDef3.Alignment = TextAlignment.TopCenter;
            block.AttributeDefinitions.Add(attDef3);
            ins.Rotation = 30;

            // to update the insert attributes call the method Sync, this method will also call the method TransformAttributes
            ins.Sync();

            // the ins2 will have all three attributes
            Insert ins2 = new Insert(block, new Vector2(20, 0));
            doc.Entities.Add(ins2);

            doc.Save("Test.dxf");

            block.Name = "MyBlockRenamed";

            doc.Save("BlockRename.dxf");

            doc = Test("BlockRename.dxf");
        }

        public static void ModifyingMLineStyles()
        {
            DxfDocument doc = new DxfDocument(DxfVersion.AutoCad2010);
            doc.DrawingVariables.LtScale = 10;

            List<Vector2> vertexes = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 150),
                new Vector2(150, 150),
                new Vector2(150, 0)
            };

            MLine mline = new MLine(vertexes);
            mline.Scale = 20;
            mline.Justification = MLineJustification.Zero;

            MLineStyle style = new MLineStyle("MyStyle", "Personalized style.");
            style.Elements.Add(new MLineStyleElement(0.25));
            style.Elements.Add(new MLineStyleElement(-0.25));

            // if we add new elements directly to the list we need to sort the list,
            style.Elements.Sort();
            style.Flags = MLineStyleFlags.EndInnerArcsCap | MLineStyleFlags.EndRoundCap | MLineStyleFlags.StartInnerArcsCap | MLineStyleFlags.StartRoundCap;

            // AutoCad2000 dxf version does not support true colors for MLineStyle elements
            style.Elements[0].Color = new AciColor(180, 230, 147);

            doc.Entities.Add(mline);

            // change the multi line style after it has been added to the document
            mline.Style = style;
            Debug.Assert(ReferenceEquals(mline.Style, doc.MlineStyles[mline.Style.Name]), "Reference not equals.");

            // VERY IMPORTANT: We have modified the MLine after setting its vertexes so we need to manually call this method.
            // It is also necessary when manually editing the vertex distances.
            mline.Update();

            // the line type will be automatically added to the document
            foreach (MLineStyleElement e in style.Elements)
            {
                // making changes after the MLineStyle has been added to the document
                e.Linetype = Linetype.Dashed;
                Debug.Assert(ReferenceEquals(e.Linetype, doc.Linetypes[e.Linetype.Name]), "Reference not equals.");
            }

            MLine copy = (MLine) mline.Clone();
            copy.Scale = 100;
            doc.Entities.Add(copy);
            // once the entity has been added to the document, changing its style requires that the new style is also present in the document.
            copy.Style = doc.MlineStyles["standard"];
            // VERY IMPORTANT: We have modified the MLine after setting its vertexes so we need to manually call this method.
            // It is also necessary when manually editing the vertex distances.
            copy.Update();

            doc.Save("ModifyingMLineStyle.dxf");
            Test("ModifyingMLineStyle.dxf");
        }

        public static void ModifyingDimensionGeometryAndStyle()
        {
            DimensionStyle style = new DimensionStyle("MyStyle");
            Vector2 p1 = new Vector2(-2.5, 0);
            Vector2 p2 = new Vector2(2.5, 0);

            LinearDimension dim = new LinearDimension(p1, p2, 4, 0, style);

            // This is illegal. Trying to rebuild the dimension block before it has been added to a document will throw an exception
            //dim.RebuildBlock();

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(dim);

            // modifying the dimension style
            dim.Style.DimArrow1 = DimensionArrowhead.ArchitecturalTick;
            dim.Style.DimArrow2 = dim.Style.DimArrow1;
            // if we make any change to the dimension style, we need to manually call the RebuildBlock method to reflect the new changes
            // since we will also modify the geometry of the dimension we will rebuild the block latter
            //dim.RebuildBlock();

            // the same kind of procedure needs to be done when modifying the geometry of a dimension
            dim.FirstReferencePoint = new Vector2(-5.0, 0);
            dim.SecondReferencePoint = new Vector2(5.0, 0);
            // now that all necessary changes has been made, we will rebuild the block.
            // this is an expensive operation, use it only when need it.

            dim.Style.DimArrow1 = DimensionArrowhead.Box;
            dim.Style.DimArrow2 = DimensionArrowhead.ArchitecturalTick;
            Debug.Assert(ReferenceEquals(dim.Style.DimArrow1, doc.Blocks[dim.Style.DimArrow1.Name]), "References are not equal.");
            Debug.Assert(ReferenceEquals(style.DimArrow2, doc.Blocks[style.DimArrow2.Name]), "References are not equal.");
            //dim.Style.DIMBLK = null;

            // VERY IMPORTANT: If any change is made to the dimension geometry and/or its style, we need to rebuild the drawing representation
            // so the dimension block will reflect the new changes. This is only necessary for dimension that already belongs to a document.
            // This process is automatically called when a new dimension is added to a document.
            dim.Update();
            Debug.Assert(ReferenceEquals(dim.Block, doc.Blocks[dim.Block.Name]));

            doc.Save("dimension.dxf");
            Test("dimension.dxf");
        }

        public static void ModifyingGroups()
        {
            Line line1 = new Line(new Vector2(0, 0), new Vector2(100, 100));
            line1.Color = AciColor.Red;
            Line line2 = new Line(new Vector2(100, 0), new Vector2(200, 100));
            line2.Color = AciColor.Yellow;
            Line line3 = new Line(new Vector2(200, 0), new Vector2(300, 100));
            line3.Color = AciColor.Magenta;

            DxfDocument doc = new DxfDocument();

            Block blk = new Block("MyBlock");
            blk.Entities.Add(line1);
            Insert ins = new Insert(blk);
            doc.Entities.Add(ins);

            doc.Entities.Add(line2);

            Layout layout = new Layout("Layout1");
            doc.Layouts.Add(layout);
            doc.Entities.ActiveLayout = layout.Name;
            doc.Entities.Add(line3);

            // group
            Group group = new Group("MyGroup");
            doc.Groups.Add(group);

            // the Add method will also add the entities contained in a group to the document (in the active layout).
            doc.Groups.Add(group);

            // when the group belongs to a document, all entities must belong to the same document.
            // even if it does not sound very useful, a group can contain entities that belongs to different layouts and even blocks.
            group.Entities.Add(line1);
            group.Entities.Add(line2);
            group.Entities.Add(line3);

            Line line4 = new Line(new Vector2(300, 0), new Vector2(400, 100));
            line4.Color = AciColor.Blue;
            // if a new entity, that does not belong to any document, is added to the group, it will be added to the group document active layout.
            doc.Entities.ActiveLayout = Layout.ModelSpaceName;
            group.Entities.Add(line4);

            Line line5 = new Line(new Vector2(400, 0), new Vector2(500, 100));
            line5.Color = AciColor.Green;
            DxfDocument doc2 = new DxfDocument();
            doc2.Entities.Add(line5);

            // this is illegal, line5 belongs to another document.
            //group.Entities.Add(line5);
            // you need to clone the entity before adding it to the group. This is also the common practice to copy entities between documents.
            group.Entities.Add((EntityObject) line5.Clone());

            // remember removing a group only deletes it from the collection not the entities
            //doc.Groups.Remove(group);
            doc.Save("group.dxf");

            doc = DxfDocument.Load("group.dxf");
        }

        public static void ModifyingXData()
        {
            Line line = new Line(Vector2.Zero, Vector2.UnitX);

            ApplicationRegistry appReg = new ApplicationRegistry("netDxf");
            XData xdata = new XData(appReg);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "Length"));
            line.XData.Add(xdata);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(line);

            // modifying existing extended data
            line.XData[appReg.Name].XDataRecord.Add(new XDataRecord(XDataCode.Real, Vector3.Distance(line.StartPoint, line.EndPoint)));

            // adding new extended data entry to an existing entity
            ApplicationRegistry appReg2 = new ApplicationRegistry("newXData");
            XData xdata2 = new XData(appReg2);
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "XData entries"));
            line.XData.Add(xdata2);

            Debug.Assert(ReferenceEquals(line.XData[appReg2.Name].ApplicationRegistry, doc.ApplicationRegistries[appReg2.Name]));

            // deleting existing extended data
            line.XData.Remove(appReg.Name);

            // we can also change the name of the application registry name
            doc.ApplicationRegistries["newXData"].Name = "netDxfRenamed";

            doc.RasterVariables.XData.Add(xdata);
            doc.Save("xData.dxf");

            doc = DxfDocument.Load("xData.dxf");
        }

        public static void DimensionsLinearAndAngularUnits()
        {
            DimensionStyle style = new DimensionStyle("MyStyle")
            {
                // DIMDEC defines the number of decimal places.
                // For Architectural and Fractional units the minimum fraction is defined by 1/2^DIMDEC.
                LengthPrecision = 4,
                FractionType = FractionFormatType.Horizontal,
                DimLengthUnits = LinearUnitType.Engineering,
                SuppressLinearTrailingZeros = true,
                SuppressZeroFeet = false,
                SuppressZeroInches = false,
                DimScaleLinear = 10.0,
                // the round off to nearest DIMRND is applied to the linear dimension measurement after applying the scale DIMLFAC
                DimRoundoff = 0.025,
                AngularPrecision = 2,
                DimAngularUnits = AngleUnitType.DegreesMinutesSeconds
            };

            Layer layer = new Layer("Layer1") {Color = AciColor.Blue};

            Vector2 p1 = new Vector2(0, 0);
            Vector2 p2 = new Vector2(21.2548, 02);

            LinearDimension dim = new LinearDimension(p1, p2, 4, 0, style);

            Vector2 s1 = new Vector2(-2, 2);
            Vector2 s2 = new Vector2(2, -2);

            Vector2 e1 = new Vector2(-1, -3);
            Vector2 e2 = new Vector2(1, 3);

            Line line1 = new Line(s1, s2) {Layer = layer};
            Line line2 = new Line(e1, e2) {Layer = layer};
            Angular2LineDimension dim1 = new Angular2LineDimension(line2, line1, 4, style);

            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(dim);
            doc.Entities.Add(dim1);
            doc.Save("DimensionsLinearAndAngularUnits.dxf");

            DxfDocument dxf = DxfDocument.Load("DimensionsLinearAndAngularUnits.dxf");
        }

        public static void DimensionUserText()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Vector3 p2 = new Vector3(0, 0, 0);
            Vector3 p1 = new Vector3(5, 0, 0);

            Line line1 = new Line(p1, p2)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            dxf.Entities.Add(line1);

            DimensionStyle style = new DimensionStyle("MyStyle");

            double offset = 0.75;
            LinearDimension dim = new LinearDimension(line1, offset, 0, Vector3.UnitZ, style);
            dim.UserText = null; // 5.00 (this is the default behavior)
            dxf.Entities.Add(dim);

            dim = new LinearDimension(line1, 2*offset, 0, Vector3.UnitZ, style);
            dim.UserText = string.Empty; // 5.00 (same behavior as null)
            dxf.Entities.Add(dim);

            dim = new LinearDimension(line1, 3*offset, 0, Vector3.UnitZ, style);
            dim.UserText = " "; // No dimension text will be drawn (one blank space)
            dxf.Entities.Add(dim);

            dim = new LinearDimension(line1, 4*offset, 0, Vector3.UnitZ, style);
            dim.UserText = "<>"; // 5.00 (the characters <> will be substituted with the style.DIMPOST property)
            dxf.Entities.Add(dim);

            dim = new LinearDimension(line1, 5*offset, 0, Vector3.UnitZ, style);
            dim.UserText = "Length: <> mm"; // Length: 5.00 mm (the characters <> will be substituted with the style.DIMPOST property)
            dxf.Entities.Add(dim);

            dim = new LinearDimension(line1, 6*offset, 0, Vector3.UnitZ, style);
            dim.UserText = "User text"; // User text
            dxf.Entities.Add(dim);

            dxf.Save("DimensionUserText.dxf");
        }

        #endregion

        #region Samples for new and modified features 0.9.3

        private static void RemoveBlock()
        {
            DxfDocument dxf = new DxfDocument();
            Block block = new Block("MyBlock");

            Line line1 = new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0));
            Line line2 = new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0));
            block.Entities.Add(line1);
            block.Entities.Add(line2);

            Insert insert = new Insert(block);

            dxf.Entities.Add(insert);

            bool ok;
            // line1 is used by block and cannot be removed (ok = false)
            ok = dxf.Entities.Remove(line1);
            // block is used by insert and cannot be removed (ok = false)
            ok = dxf.Blocks.Remove(block);
            // it is safe to remove insert, it doesn't belong to anybody (ok = true)
            ok = dxf.Entities.Remove(insert);
            // it is safe to remove block, it doesn't belong to anybody (ok = true)
            // at the same time, all entities that were part of the block have been also removed
            ok = dxf.Blocks.Remove(block);
            // obj is null the line1 does not exist in the document, the block was removed
            DxfObject obj = dxf.GetObjectByHandle(line1.Handle);

            Console.WriteLine("Press a key...");
            Console.ReadKey();
        }

        private static DimensionStyle CreateDimStyle()
        {
            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimLineExtend = 0.18;
            myStyle.LengthPrecision = 2;
            myStyle.DimScaleOverall = 2.0;
            //myStyle.DIMLTEX1 = Linetype.Dot;
            myStyle.DimLineColor = AciColor.Yellow;
            //myStyle.DIMBLK = DimensionArrowhead.ArchitecturalTick;
            myStyle.DimArrow1 = DimensionArrowhead.Box;
            myStyle.DimArrow2 = DimensionArrowhead.DotBlank;
            //myStyle.DIMSE1 = true;
            //myStyle.DIMSE2 = true;

            myStyle.TextColor = AciColor.Red;

            return myStyle;
        }

        private static void LinearDimension()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);

            DimensionStyle myStyle = CreateDimStyle();

            Vector3 p1 = new Vector3(0, -5, 0);
            Vector3 p2 = new Vector3(-5, 0, 0);

            Line line1 = new Line(p1, p2)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            dxf.Entities.Add(line1);

            double offset = 4;
            LinearDimension dimX1 = new LinearDimension(line1, offset, 0, Vector3.UnitZ, myStyle);
            LinearDimension dimY1 = new LinearDimension(line1, offset, 90, Vector3.UnitZ, myStyle);
            LinearDimension dim5 = new LinearDimension(line1, offset, -30, Vector3.UnitZ, myStyle);
            LinearDimension dim6 = new LinearDimension(line1, offset, -60, Vector3.UnitZ, myStyle);

            Vector3 p3 = new Vector3(6, -5, 0);
            Vector3 p4 = new Vector3(11, 0, 0);
            Line line2 = new Line(p3, p4)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            dxf.Entities.Add(line2);
            LinearDimension dimX2 = new LinearDimension(line2, offset, -30.0, Vector3.UnitZ, myStyle);
            LinearDimension dimY2 = new LinearDimension(line2, offset, -60.0, Vector3.UnitZ, myStyle);
            LinearDimension dim3 = new LinearDimension(line2, offset, 30.0, Vector3.UnitZ, myStyle);
            LinearDimension dim4 = new LinearDimension(line2, offset, 60.0, Vector3.UnitZ, myStyle);

            dxf.Entities.Add(dimX1);
            dxf.Entities.Add(dimY1);
            dxf.Entities.Add(dimX2);
            dxf.Entities.Add(dimY2);
            dxf.Entities.Add(dim3);
            dxf.Entities.Add(dim4);
            dxf.Entities.Add(dim5);
            dxf.Entities.Add(dim6);
            dxf.Save("dimension drawing.dxf");

            //DxfDocument dwg = DxfDocument.Load("dimension drawing.dxf");
            //dwg.Save("dimension drawing saved.dxf");

            //Console.WriteLine("Press a key...");
            //Console.ReadKey();
        }

        private static void AlignedDimension()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);
            DimensionStyle myStyle = CreateDimStyle();

            Vector3 p1 = new Vector3(0, -5, 0);
            Vector3 p2 = new Vector3(-5, 0, 0);

            Line line1 = new Line(p1, p2)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            dxf.Entities.Add(line1);

            double offset = 4;
            AlignedDimension dim1 = new AlignedDimension(line1, offset, Vector3.UnitZ, myStyle);
            AlignedDimension dim11 = new AlignedDimension(line1, -offset, Vector3.UnitZ, myStyle);

            Vector3 p3 = new Vector3(6, -5, 0);
            Vector3 p4 = new Vector3(11, 0, 0);
            Line line2 = new Line(p3, p4)
            {
                Layer = new Layer("Reference line")
                {
                    Color = AciColor.Green
                }
            };
            dxf.Entities.Add(line2);
            AlignedDimension dim2 = new AlignedDimension(line2, offset, Vector3.UnitZ, myStyle);
            AlignedDimension dim21 = new AlignedDimension(line2, -offset, Vector3.UnitZ, myStyle);

            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim11);
            dxf.Entities.Add(dim2);
            dxf.Entities.Add(dim21);

            dxf.Save("dimension drawing.dxf");

            //Console.WriteLine("Press a key...");
            //Console.ReadKey();

            //DxfDocument dwg = DxfDocument.Load("dimension drawing.dxf");
            //dwg.Save("dimension drawing saved.dxf");
        }

        private static void Angular2LineDimension()
        {
            DimensionStyle myStyle = CreateDimStyle();

            double offset = 4;

            Layer layer = new Layer("Layer1") {Color = AciColor.Blue};
            Vector2 s1 = new Vector2(-2, 2);
            Vector2 s2 = new Vector2(2, -2);

            Vector2 e1 = new Vector2(-1, -3);
            Vector2 e2 = new Vector2(1, 3);

            Line line1 = new Line(s1, s2) {Layer = layer};
            Line line2 = new Line(e1, e2) {Layer = layer};
            Angular2LineDimension dim1 = new Angular2LineDimension(line1, line2, offset, myStyle);
            Angular2LineDimension dim2 = new Angular2LineDimension(line1, line2, -offset, myStyle);
            line1.Reverse();
            Angular2LineDimension dim3 = new Angular2LineDimension(line2, line1, offset, myStyle);
            Angular2LineDimension dim4 = new Angular2LineDimension(line2, line1, -offset, myStyle);

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line1);
            dxf.Entities.Add(line2);
            dxf.Entities.Add(dim1);
            //dxf.Entities.Add(dim2);
            //dxf.Entities.Add(dim3);
            //dxf.Entities.Add(dim4);
            dxf.Save("dimension drawing.dxf");
            dxf = DxfDocument.Load("dimension drawing.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("dimension drawing saved.dxf");
        }

        private static void Angular3PointDimension()
        {
            DxfDocument dxf = new DxfDocument();
            DimensionStyle myStyle = CreateDimStyle();
            myStyle.AngularPrecision = 4;
            myStyle.DimAngularUnits = AngleUnitType.DegreesMinutesSeconds;
            Vector3 center = new Vector3(1, 2, 0);
            double radius = 2.5;
            Arc arc = new Arc(center, radius, -32.8, 160.5);
            Angular3PointDimension dim1 = new Angular3PointDimension(arc, 5, myStyle);
            Angular3PointDimension dim2 = new Angular3PointDimension(arc, -5, myStyle);
            dxf.Entities.Add(arc);
            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim2);
            dxf.Save("dimension drawing.dxf");

            dxf = DxfDocument.Load("dimension drawing.dxf");

            DxfDocument doc = new DxfDocument();
            foreach (var c in dxf.Entities.Circles)
            {
                doc.Entities.Add((EntityObject) c.Clone());
            }
            foreach (var d in dxf.Entities.Dimensions)
            {
                doc.Entities.Add((EntityObject) d.Clone());
            }
            doc.Save("dimension drawing saved.dxf");
        }

        private static void DiametricDimension()
        {
            DxfDocument dxf = new DxfDocument();
            DimensionStyle myStyle = CreateDimStyle();

            Vector3 center = new Vector3(1, 2, 0);
            double radius = 3;
            Circle circle = new Circle(center, radius);
            //circle.Normal = new Vector3(1, 1, 1);
            double angle = MathHelper.HalfPI*0.5;
            Vector3 refPoint = center + new Vector3(radius*Math.Cos(angle), radius*Math.Cos(angle), 0);

            //DiametricDimension dim = new DiametricDimension(center, refPoint, -1.0, myStyle);
            DiametricDimension dim1 = new DiametricDimension(circle, 0, myStyle);
            DiametricDimension dim2 = new DiametricDimension(circle, 45, myStyle);
            DiametricDimension dim3 = new DiametricDimension(circle, 90, myStyle);
            DiametricDimension dim4 = new DiametricDimension(circle, 120, myStyle);
            DiametricDimension dim5 = new DiametricDimension(circle, 180, myStyle);
            DiametricDimension dim6 = new DiametricDimension(circle, 220, myStyle);
            DiametricDimension dim7 = new DiametricDimension(circle, 270, myStyle);
            DiametricDimension dim8 = new DiametricDimension(circle, 330, myStyle);

            // if the dimension normal is not equal to the circle normal strange things might happen at the moment
            //dim1.Normal = circle.Normal;
            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim2);
            dxf.Entities.Add(dim3);
            dxf.Entities.Add(dim4);
            dxf.Entities.Add(dim5);
            dxf.Entities.Add(dim6);
            dxf.Entities.Add(dim7);
            dxf.Entities.Add(dim8);
            dxf.Save("dimension drawing.dxf");

            dxf = DxfDocument.Load("dimension drawing.dxf");

            DxfDocument doc = new DxfDocument();
            foreach (var c in dxf.Entities.Circles)
            {
                doc.Entities.Add((EntityObject) c.Clone());
            }
            foreach (var d in dxf.Entities.Dimensions)
            {
                doc.Entities.Add((EntityObject) d.Clone());
            }
            doc.Save("dimension drawing saved.dxf");
        }

        private static void RadialDimension()
        {
            DxfDocument dxf = new DxfDocument();
            DimensionStyle myStyle = CreateDimStyle();

            Vector3 center = new Vector3(1, 2, 0);
            double radius = 3;
            Circle circle = new Circle(center, radius);
            //circle.Normal = new Vector3(1, 1, 1);
            double angle = MathHelper.HalfPI*0.5;
            Vector3 refPoint = center + new Vector3(radius*Math.Cos(angle), radius*Math.Cos(angle), 0);

            //DiametricDimension dim = new DiametricDimension(center, refPoint, -1.0, myStyle);
            RadialDimension dim1 = new RadialDimension(circle, 0, myStyle);
            RadialDimension dim2 = new RadialDimension(circle, 45, myStyle);
            RadialDimension dim3 = new RadialDimension(circle, 90, myStyle);
            RadialDimension dim4 = new RadialDimension(circle, 120, myStyle);
            RadialDimension dim5 = new RadialDimension(circle, 180, myStyle);
            RadialDimension dim6 = new RadialDimension(circle, 220, myStyle);
            RadialDimension dim7 = new RadialDimension(circle, 270, myStyle);
            RadialDimension dim8 = new RadialDimension(circle, 330, myStyle);
            // if the dimension normal is not equal to the circle normal strange things might happen at the moment
            //dim1.Normal = circle.Normal;
            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim1);
            dxf.Entities.Add(dim2);
            dxf.Entities.Add(dim3);
            dxf.Entities.Add(dim4);
            dxf.Entities.Add(dim5);
            dxf.Entities.Add(dim6);
            dxf.Entities.Add(dim7);
            dxf.Entities.Add(dim8);
            dxf.Save("dimension drawing.dxf");

            dxf = DxfDocument.Load("dimension drawing.dxf");

            DxfDocument doc = new DxfDocument();
            foreach (var c in dxf.Entities.Circles)
            {
                doc.Entities.Add((EntityObject) c.Clone());
            }
            foreach (var d in dxf.Entities.Dimensions)
            {
                doc.Entities.Add((EntityObject) d.Clone());
            }
            doc.Save("dimension drawing saved.dxf");
        }

        private static void OrdinateDimension()
        {
            DxfDocument dxf = new DxfDocument();

            Vector2 origin = new Vector2(2, 1);
            Vector2 refX = new Vector2(1, 0);
            Vector2 refY = new Vector2(0, 2);
            double length = 3;
            double angle = 30;
            DimensionStyle myStyle = CreateDimStyle();

            OrdinateDimension dimX1 = new OrdinateDimension(origin, refX, length, OrdinateDimensionAxis.X, 0, myStyle);
            OrdinateDimension dimX2 = new OrdinateDimension(origin, refX, length, OrdinateDimensionAxis.X, angle, myStyle);
            OrdinateDimension dimY1 = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, 0, myStyle);
            OrdinateDimension dimY2 = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, angle, myStyle);

            dxf.Entities.Add(dimX1);
            //dxf.Entities.Add(dimY1);
            //dxf.Entities.Add(dimX2);
            //dxf.Entities.Add(dimY2);

            Line lineX = new Line(origin, origin + 5*Vector2.UnitX);
            Line lineY = new Line(origin, origin + 5*Vector2.UnitY);

            Vector2 point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad);
            Line lineXRotate = new Line(origin, new Vector2(point.X, point.Y));

            point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad + MathHelper.HalfPI);
            Line lineYRotate = new Line(origin, new Vector2(point.X, point.Y));

            //dxf.Entities.Add(lineX);
            //dxf.Entities.Add(lineY);
            //dxf.Entities.Add(lineXRotate);
            //dxf.Entities.Add(lineYRotate);

            dxf.Save("dimension drawing.dxf");

            dxf = DxfDocument.Load("dimension drawing.dxf");
            dxf.Save("dimension drawing saved.dxf");

            //DxfDocument doc = new DxfDocument();
            //foreach (var c in dxf.Circles)
            //{
            //    doc.Entities.Add((EntityObject) c.Clone());
            //}
            //foreach (var d in dxf.Dimensions)
            //{
            //    doc.Entities.Add((EntityObject) d.Clone());
            //}
            //doc.Save("dimension drawing saved.dxf");
        }

        #endregion

        #region Samples for new and modified features 0.9.2

        public static void NurbsEvaluator()
        {
            Layer splines = new Layer("Splines");
            splines.Color = AciColor.Blue;

            Layer result = new Layer("Nurbs evaluator");
            result.Color = AciColor.Red;

            Vector3[] ctrlPoints =
            {
                new Vector3(0, 0, 0),
                new Vector3(25, 50, 50),
                new Vector3(50, 0, 100),
                new Vector3(75, 50, 50),
                new Vector3(100, 0, 0)
            };

            double[] weigths1 =
            {
                1.0, 2.0, 3.0, 4.0, 5.0
            };

            // the constructor will generate a uniform knot vector 
            Spline openSpline = new Spline(ctrlPoints, weigths1, 3) {Layer = splines};

            Vector3[] ctrlPointsClosed =
            {
                new Vector3(0, 0, 0),
                new Vector3(25, 50, 0),
                new Vector3(50, 0, 0),
                new Vector3(75, 50, 0),
                new Vector3(100, 0, 0),
                new Vector3(0, 0, 0) // closed spline non periodic we repeat the last control point
            };
            Spline closedNonPeriodicSpline = new Spline(ctrlPointsClosed, null, 3) {Layer = splines};

            // the periodic spline will generate a periodic (unclamped) closed curve,
            // as far as my tests have gone not all programs handle them correctly, most of them only handle clamped splines
            Spline closedPeriodicSpline = new Spline(ctrlPoints, null, 4, true) {Layer = splines};
            // always use spline vertex weights of 1.0 (default value) looks like that AutoCAD does not handle them correctly for periodic splines,
            // but they work fine for non periodic splines
            closedPeriodicSpline.SetUniformWeights(1.0);

            // manually defining the control points and the knot vector (example a circle created with nurbs)
            Vector3[] circle =
            {
                new Vector3(50, 0, 0),
                new Vector3(100, 0, 0),
                new Vector3(100, 100, 0),
                new Vector3(50, 100, 0),
                new Vector3(0, 100, 0),
                new Vector3(0, 0, 0),
                new Vector3(50, 0, 0) // repeat the first point to close the circle
            };

            double[] weigths2 =
            {
                1.0, 0.5, 0.5, 1.0, 0.5, 0.5, 1.0
            };

            // the number of knots must be control points number + degree + 1
            // Conics are 2nd degree curves
            List<double> knots = new List<double> {0.0, 0.0, 0.0, 1.0/4.0, 1.0/2.0, 1.0/2.0, 3.0/4.0, 1.0, 1.0, 1.0};
            Spline splineCircle = new Spline(circle, weigths2, knots, 2, false) {Layer = splines};

            DxfDocument dxf = new DxfDocument();
            // we will convert the Spline to a Polyline
            Polyline3D pol;

            //dxf.Entities.Add(openSpline);
            //pol = openSpline.ToPolyline3D(100);
            //pol.Layer = result;
            //dxf.Entities.Add(pol);

            //dxf.Entities.Add(closedNonPeriodicSpline);
            //pol = closedNonPeriodicSpline.ToPolyline3D(100);
            //pol.Layer = result;
            //dxf.Entities.Add(pol);

            dxf.Entities.Add(closedPeriodicSpline);
            pol = closedPeriodicSpline.ToPolyline3D(100);
            pol.Layer = result;
            dxf.Entities.Add(pol);

            //dxf.Entities.Add(splineCircle);
            //pol = splineCircle.ToPolyline3D(100);
            //pol.Layer = result;
            //dxf.Entities.Add(pol);

            dxf.Save("test.dxf");

            DxfDocument doc = DxfDocument.Load("test.dxf");
            doc.Save("test.dxf");
        }

        public static void XDataInformation()
        {
            Line line = new Line(Vector2.Zero, Vector2.UnitY);
            XData xdata = new XData(new ApplicationRegistry("a"));
            //xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);
            //xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "a"));
            //xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 1));
            //xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, 1.1));
            //xdata.XDataRecord.Add(new XDataRecord(XDataCode.WorldDirectionX, 1.2));
            line.XData.Add(xdata);
            DxfDocument dwg = new DxfDocument();
            dwg.Entities.Add(line);
            dwg.Save("Xdata.dxf");
        }

        public static void DynamicBlocks()
        {
            // netDxf can read dynamic blocks but you will loose the information related with the parameters, actions and constrains
            // for every insert AutoCad creates a new block definition with the name *U#, where # is a positive integer
            // except in the case where the dynamic block parameters are not modified, in this case the original block will be used instead

            DxfDocument drawing;
            drawing = DxfDocument.Load("..//samples//DynamicBlock.dxf");

            Insert insert;
            Block block;
            string name;
            EntityCollection entities;

            // let's take a look at the block associated with the inserts
            // the dynamic parameter of this insert was modified so the block name will be called *U#
            insert = drawing.Entities.Inserts.ElementAt(0);
            block = insert.Block;
            // this is the block name
            name = block.Name;
            // the list of entities contained in the block are the ones defined in the original block definition modified by the dynamic parameter
            entities = block.Entities;

            // to access the original dynamic block we need to get first the extended data associated with the BlockRecord,
            // the application registry for this extended data always has the name "AcDbBlockRepBTag"
            XData xdata = block.Record.XData["AcDbBlockRepBTag"];
            string handle = null;
            // the original dynamic block record handle is stored in the extended data
            foreach (XDataRecord data in xdata.XDataRecord)
            {
                if (data.Code == XDataCode.DatabaseHandle)
                    handle = (string) data.Value;
            }

            // now we can get the original dynamic block record
            BlockRecord originalDynamicBlockRecord = (BlockRecord) drawing.GetObjectByHandle(handle);
            string dynamicBlockName = originalDynamicBlockRecord.Name;
            // if we need the original block instead of just the record, we can get it from the list of block since we know now its name
            Block originalBlock = drawing.Blocks[dynamicBlockName];

            // the dynamic parameter of this insert was NOT modified so the block will be the original
            insert = drawing.Entities.Inserts.ElementAt(1);
            block = insert.Block;
            // this is the block name
            name = block.Name;
            // the list of entities contained in the block are the ones defined in the original
            entities = block.Entities;

            // remember all dynamic parameters information will be lost
            drawing.Save("Saved sample.dxf");
        }

        #endregion

        #region Samples for new and modified features 0.9.1

        public static void LoadAndSaveBlocks()
        {
            // Create a block to be used as sample
            Block baseBlk = new Block("BaseBlock");
            baseBlk.Record.Units = DrawingUnits.Millimeters;
            baseBlk.Entities.Add(new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0)));
            baseBlk.Entities.Add(new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0)));
            AttributeDefinition attdef = new AttributeDefinition("MyAttribute")
            {
                Prompt = "Enter a value:",
                Value = "0",
                Position = Vector3.Zero,
                Layer = new Layer("MyLayer")
                {
                    Color = AciColor.Red
                }
            };
            baseBlk.AttributeDefinitions.Add(attdef);

            // Blocks are saved in a similar way as other dxf, just pass the dxf file name, the DxfVersion, and optionally if the dxf needs to be saved in binary format
            // Only AutoCad2000 and newer versions are supported.
            // The block entities and attribute definitions will be added to the Model layout.
            // The drawing header units will be the ones defined in the block record.
            baseBlk.Save(baseBlk.Name + ".dxf", DxfVersion.AutoCad2000);

            DxfDocument dxf = new DxfDocument();

            // Blocks are loaded as any other dxf, just pass the dxf file name,
            // optionally you can also give it a name, by default the file name without extension will be used.
            // Only AutoCad2000 and newer versions are supported,
            // Only the entities contained in the Model layout will be used.
            // The block units will be the ones defined in the dxf header.
            Block block = Block.Load(baseBlk.Name + ".dxf", "MyBlock");

            // in case the loading process has failed check for null
            // In DEBUG mode the loading process will raise exceptions while in RELEASE it will just return null, the same as loading a DxfDocument
            if (block == null)
            {
                Console.WriteLine("Error loading the block dxf file.");
                Console.WriteLine("Press a key to continue...");
                Console.ReadKey();
                return;
            }

            // once the block is loaded we can use it in insert entities
            Insert insert = new Insert(block, new Vector2(10));

            // the block might also contain attribute definitions
            int attdefCount = block.AttributeDefinitions.Count;

            // this is the list of attribute definition tags
            // remember netDxf does not allow the use of duplicate tag names, although AutoCad allows it, it is not recommended
            ICollection<string> tags = block.AttributeDefinitions.Tags;

            // we can assign values to the insert attributes
            foreach (Attribute att in insert.Attributes)
            {
                att.Value = string.Format("{0} value", att.Tag);
            }

            // optionally we can manually add the block definition to the document
            dxf.Blocks.Add(block);

            // we add the insert entity to the document, if the block associated with the block has not been added this method will do it automatically
            dxf.Entities.Add(insert);

            // also it is possible to manually add attribute definitions to a document
            AttributeDefinition def = new AttributeDefinition("AttDefOutsideBlock")
            {
                Prompt = "Enter value:",
                Value = "0",
                Color = AciColor.Blue,
                Position = new Vector3(0, 30, 0)
            };

            // we will add the attribute definition to the document just like any other entity
            dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions.Add(def);

            // now we can save our new document
            dxf.Save("CreateBlockFromDxf.dxf");

            DxfDocument load = DxfDocument.Load("CreateBlockFromDxf.dxf");
        }

        #endregion

        #region Samples for new and modified features 0.9.0

        private static void MakingGroups()
        {
            Line line1 = new Line(Vector2.Zero, Vector2.UnitX);
            Line line2 = new Line(Vector2.Zero, Vector2.UnitY);
            Group group = new Group();
            group.Entities.Add(line1);
            group.Entities.Add(line2);

            DxfDocument dxf = new DxfDocument();
            // when we add a group to the document all the entities contained in the group will be automatically added to the document
            dxf.Groups.Add(group);

            // adding the group entities to the document will raise an exception since they are already added to the document
            //dxf.Entities.Add(line1);
            //dxf.Entities.Add(line2);

            dxf.Save("group.dxf");

            DxfDocument load = DxfDocument.Load("group.dxf");

            Console.WriteLine("Press a key to finish...");
            Console.ReadKey();
        }

        private static void CombiningTwoDrawings()
        {
            // create first drawing
            Line line1 = new Line(Vector2.Zero, Vector2.UnitX);
            line1.Layer = new Layer("Layer01");
            line1.Layer.Color = AciColor.Blue;
            DxfDocument dxf1 = new DxfDocument();
            dxf1.Entities.Add(line1);
            dxf1.Save("drawing01.dxf");

            // create second drawing
            Line line2 = new Line(Vector2.Zero, Vector2.UnitY);
            line2.Layer = new Layer("Layer02");
            line2.Layer.Color = AciColor.Red;
            DxfDocument dxf2 = new DxfDocument();
            dxf2.Entities.Add(line2);
            dxf2.Save("drawing02.dxf");

            // load the drawings that will be combined
            DxfDocument source01 = DxfDocument.Load("drawing01.dxf");
            DxfDocument source02 = DxfDocument.Load("drawing02.dxf");

            // our destination drawing
            DxfDocument combined = new DxfDocument();
            foreach (Line l in source01.Entities.Lines)
            {
                // It is recommended to make a copy of the source line before we can added to the destination drawing
                // if we do not make a copy weird things might happen if we save the original drawing again
                Line copy = (Line) l.Clone();
                combined.Entities.Add(copy);
            }

            // Another safe way is removing the entity from the original drawing before adding it to the destination drawing
            Line line = source02.Entities.Lines.ElementAt(0);
            source02.Entities.Remove(line);
            combined.Entities.Add(line);

            combined.Save("CombinedDrawing.dxf");
        }

        private static void BinaryChunkXData()
        {
            Line line = new Line(Vector2.Zero, Vector2.UnitX);

            ApplicationRegistry appId = new ApplicationRegistry("TestBinaryChunk");

            // the extended data binary data (code 1004) is stored in a different way depending if the dxf file is text or binary.
            // in text based files as a string of hexadecimal digits, two per binary byte,
            // while in binary files the data is stored in chunks of 127 bytes, preceding a byte that defines the number of bytes in the chunk
            byte[] data = new byte[325];

            // fill up the array with some random data
            Random rnd = new Random();
            rnd.NextBytes(data);

            XData xdata = new XData(appId);

            // the XDataRecord will store the binary data as a byte array and not as a string as it use to be
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.BinaryData, data));
            line.XData.Add(xdata);

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line);

            dxf.Save("BinaryChunkXData.dxf");
            dxf.Save("BinaryChunkXData binary.dxf", true);

            // some testing
            DxfDocument test = DxfDocument.Load("BinaryChunkXData binary.dxf");
            Line lineTest = test.Entities.Lines.ElementAt(0);
            XDataRecord recordTest = lineTest.XData[appId.Name].XDataRecord[0];
            Debug.Assert(recordTest.Code == XDataCode.BinaryData);
            byte[] dataText = (byte[]) recordTest.Value;

            byte[] compare = new byte[127];
            Array.Copy(data, compare, 127);

            for (int i = 0; i < 127; i++)
            {
                Console.WriteLine(dataText[i] == compare[i]);
            }

            // this is the string as it is saved in text based dxf files
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < dataText.Length; i++)
            {
                sb.Append(string.Format("{0:X2}", data[i]));
            }
            Console.WriteLine(sb.ToString());

            Console.WriteLine();
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
        }

        private static void BinaryDxfFiles()
        {
            // Binary dxf files preserve the accuracy of the drawing while text dxf files are saved with 17 decimals.
            // Binary dxf files are 4-5 times faster to write, while reading is just a little faster.
            // Binary dxf files take about 20% less file space.
            DxfDocument dxf = new DxfDocument();
            // optionally you can give a name to de document
            dxf.Name = "Binary dxf";
            Line line = new Line(Vector3.Zero, new Vector3(10));
            dxf.Entities.Add(line);


            // To save a document as a binary dxf just set the isBinary parameter to true, by default it will always be saved as a text based dxf 
            // you can use the document name as tha file name, or just give another one.
            string file = dxf.Name + ".dxf";

            // Handling with error checking of the saving process
            bool ok = dxf.Save(file, true);
            if (ok)
                Console.WriteLine("The file \"{0}\" has been correctly saved.", file);
            else
                Console.WriteLine("Fatal error while saving \"{0}\".", file);

            Console.WriteLine();

            // Handling with error checking of the loading process

            // check if the file exists
            if (!File.Exists(file))
            {
                Console.WriteLine("The file \"{0}\" does not exists.", file);
            }
            else
            {
                bool isBinary;
                DxfVersion version = DxfDocument.CheckDxfFileVersion(file, out isBinary);

                // netDxf only supports AutoCad2000 and above.
                if (version >= DxfVersion.AutoCad2000)
                {
                    // To load a binary dxf nothing needs to be done, the reader will detect the correct type.
                    DxfDocument load = DxfDocument.Load(file);
                    if (load == null)
                        Console.WriteLine("Fatal error while loading \"{0}\".", file);
                    else
                    //when a document is loaded the file name without extension is used as the document name
                        Console.WriteLine("The file \"{0}\" version {1} has been correctly loaded.\n\tBinary? {2}", file, version, isBinary);
                }
                else
                {
                    if (version == DxfVersion.Unknown)
                        Console.WriteLine("The file \"{0}\" is not a dxf.", file);
                    else
                        Console.WriteLine("Dxf file \"{0}\" version {1} is not supported, only AutoCad2000 and above.", file, version);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
        }

        private static void MeshEntity()
        {
            // construct a simple cube (see the AutoCad documentation for more information about creating meshes)

            // the mesh data is always defined at level 0 (no subdivision)
            // 8 vertexes
            List<Vector3> vertexes = new List<Vector3>
            {
                new Vector3(-5, -5, -5),
                new Vector3(5, -5, -5),
                new Vector3(5, 5, -5),
                new Vector3(-5, 5, -5),
                new Vector3(-5, -5, 5),
                new Vector3(5, -5, 5),
                new Vector3(5, 5, 5),
                new Vector3(-5, 5, 5)
            };

            //6 faces
            List<int[]> faces = new List<int[]>
            {
                new[] {0, 3, 2, 1},
                new[] {0, 1, 5, 4},
                new[] {1, 2, 6, 5},
                new[] {2, 3, 7, 6},
                new[] {0, 4, 7, 3},
                new[] {4, 5, 6, 7}
            };


            // the list of edges is optional and only really needed when applying creases values to them
            // crease negative values will sharpen the edge independently of the subdivision level. Any negative crease value will be reseted as -1.
            // by default edge creases are set to 0.0 (no edge sharpening)
            List<MeshEdge> edges = new List<MeshEdge>
            {
                new MeshEdge(0, 1),
                new MeshEdge(1, 2),
                new MeshEdge(2, 3),
                new MeshEdge(3, 0),
                new MeshEdge(4, 5, -1.0),
                new MeshEdge(5, 6, -1.0),
                new MeshEdge(6, 7, -1.0),
                new MeshEdge(7, 4, -1.0),
                new MeshEdge(0, 4),
                new MeshEdge(1, 5),
                new MeshEdge(2, 6),
                new MeshEdge(3, 7)
            };
            Mesh mesh = new Mesh(vertexes, faces, edges);
            mesh.SubdivisionLevel = 3;

            DxfDocument dxf = new DxfDocument();

            ApplicationRegistry newAppReg = dxf.ApplicationRegistries.Add(new ApplicationRegistry("netDxf"));

            XData xdata = new XData(newAppReg);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "xdata string sample"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 50000));
            mesh.XData.Add(xdata);

            dxf.Entities.Add(mesh);

            dxf.Save("mesh.dxf");

            //dxf = DxfDocument.Load("mesh.dxf");

            dxf = DxfDocument.Load(@"..\samples\RenderMode2.dxf");
            //dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            //dxf.Viewport.RenderMode = RenderMode.HiddenLine;
            //dxf.Viewports[0].RenderMode = RenderMode.GouraudShadedWithWireframe;

            dxf.Save("mesh2.dxf");
        }

        #endregion

        #region Samples for new and modified features 0.8.0

        private static void MTextEntity()
        {
            TextStyle style = new TextStyle("Arial");

            MText text1 = new MText(Vector2.Zero, 10, 0, style);
            // you can set manually the text value with all available formatting commands
            text1.Value = "{\\C71;\\c10938556;Text} with true color\\P{\\C140;Text} with indexed color";

            MText text2 = new MText(new Vector2(0, 30), 10, 0, style);
            // or use the Write() method
            MTextFormattingOptions op = new MTextFormattingOptions();
            op.Color = new AciColor(188, 232, 166); // using true color
            text2.Write("Text", op);
            op.Color = null; // set color to the default defined in text2.Style
            text2.Write(" with true color");
            text2.EndParagraph();
            op.Color = new AciColor(140); // using index color
            text2.Write("Text", op); // set color to the default defined in text2.Style
            op.Color = null;
            text2.Write(" with indexed color");

            // both text1 and text2 should yield to the same result
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);
            dxf.Entities.Add(text1);
            dxf.Entities.Add(text2);

            dxf.Save("MText format.dxf");

            // now you can retrieve the MText text value without any formatting codes, control characters like tab '\t' will be preserved in the result,
            // the new paragraph command "\P" will be converted to new line feed '\r\n'.
            Console.WriteLine(text1.PlainText());
            Console.WriteLine();
            Console.WriteLine(text2.PlainText());
            Console.WriteLine();
            Console.WriteLine("Press a key to finish...");
            Console.ReadKey();
        }

        private static void TransparencySample()
        {
            // transparencies can only be applied to entities and layer
            Layer layer = new Layer("Layer with transparency");
            layer.Color = new AciColor(Color.MediumVioletRed);
            // the transparency is expressed in percentage. Initially all Transparency values are initialized as ByLayer.
            layer.Transparency.Value = 50;
            // You cannot use the reserved values 0 and 100 that represents ByLayer and ByBlock. Use Transparency.ByLayer and Transparency.ByBlock
            // this behavior is similar to the index in AciColor or the weight in Lineweight
            // this is wrong and will rise and exception
            //layer.Transparency.Value = 0;
            // this is ok
            //layer.Transparency = Transparency.ByLayer;

            // this line will use the transparency defined in the layer to which it belongs
            Line line1 = new Line(new Vector2(-5, -5), new Vector2(5, 5));
            line1.Layer = layer;

            // this line will use its own transparency
            Line line2 = new Line(new Vector2(-5, 5), new Vector2(5, -5));
            line2.Transparency.Value = 80;

            // transparency as the true color is not supported by AutoCad2000 database version
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2004);
            dxf.Entities.Add(line1);
            dxf.Entities.Add(line2);

            dxf.Save("TransparencySample.dxf");

            dxf = DxfDocument.Load("TransparencySample.dxf");
        }

        private static void DocumentUnits()
        {
            DxfDocument dxf = new DxfDocument();

            // setting the LUnit variable to engineering or architectural will also set the InsUnits variable to Inches,
            // this need to be this way since AutoCad will show those units in feet and inches and will always consider the drawing base units as inches.
            // You can change again the InsUnits it at your own risk.
            // its main purpose is at the user interface level
            //dxf.DrawingVariables.LUnits = LinearUnitType.Engineering;

            // this is the recommended document unit type
            dxf.DrawingVariables.LUnits = LinearUnitType.Decimal;

            // this is the real important unit,
            // it is used when inserting blocks or images into the drawing as this and the block units will give the scale of the resulting Insert
            dxf.DrawingVariables.InsUnits = DrawingUnits.Millimeters;

            // the angle unit type is purely cosmetic as it has no influence on how the angles are stored in the dxf 
            // its purpose is only at the user interface level
            dxf.DrawingVariables.AUnits = AngleUnitType.Radians;

            // even though we have set the drawing angles in radians the dxf always stores angle data in degrees,
            // this arc goes from 45 to 270 degrees and not radians or whatever the AUnits header variable says.
            Arc arc = new Arc(Vector2.Zero, 5, 45, 270);
            // Remember, at the moment, once the entity has been added to the document is not safe to modify it, changes in some of their properties might generate problems
            dxf.Entities.Add(arc);

            // the units of this line will correspond to the ones set in InsUnits
            Line lineM = new Line(new Vector2(-5, -5), new Vector2(5, 5));
            dxf.Entities.Add(lineM);

            // All entities added to a block are expressed in the coordinates defined by the block
            // You can set a default unit so all new blocks will use them, the default value is Unitless
            // You might want to use the same units as the drawing, this is just a convenient way to make sure all blocks share the same units 
            BlockRecord.DefaultUnits = dxf.DrawingVariables.InsUnits;

            // In this case the line will be 10 cm long
            Line lineCm = new Line(new Vector2(-5, 0), new Vector2(5, 0));
            Block blockCm = new Block("CmBlock");
            // You can override the default units changing the block.Record.Units value
            blockCm.Record.Units = DrawingUnits.Centimeters;
            blockCm.Entities.Add(lineCm);
            Insert insCm = new Insert(blockCm);

            // In this case the line will be 10 dm long
            Line lineDm = new Line(new Vector2(0, 5), new Vector2(0, -5));
            Block blockDm = new Block("DmBlock");
            blockDm.Record.Units = DrawingUnits.Decimeters;
            // AllowExploding and ScaleUniformy properties will only be recognized by dxf version AutoCad2007 and upwards
            blockDm.Record.AllowExploding = false;
            blockDm.Record.ScaleUniformly = true;
            blockDm.Entities.Add(lineDm);
            blockDm.Entities.Add(insCm);
            Insert insDm = new Insert(blockDm);

            dxf.Entities.Add(insDm);

            // the image units are stored in the raster variables units, it is recommended to use the same units as the document to avoid confusions
            dxf.RasterVariables.Units = ImageUnits.Millimeters;
            // Sometimes AutoCad does not like image file relative paths, in any case reloading the references will fix the problem
            string imgFile = "image.jpg";
            System.Drawing.Image img = System.Drawing.Image.FromFile(imgFile);
            ImageDefinition imageDefinition = new ImageDefinition("MyImage", imgFile, img.Width, img.HorizontalResolution, img.Height, img.VerticalResolution, ImageResolutionUnits.Inches);

            // the resolution units is only used to calculate the image resolution that will return pixels per inch or per centimeter (the use of NoUnits is not recommended).
            imageDefinition.ResolutionUnits = ImageResolutionUnits.Inches;
            // this image will be 10x10 mm in size
            Image image = new Image(imageDefinition, Vector3.Zero, 10, 10);
            dxf.Entities.Add(image);

            dxf.Save("Document Units.dxf");

            DxfDocument dxfLoad = DxfDocument.Load("Document Units.dxf");
        }

        private static void PaperSpace()
        {
            // Sample on how to work with Layouts
            DxfDocument dxf = new DxfDocument();
            // A new DxfDocument will create the default "Model" layout that is associated with the ModelSpace block. This layout cannot be erased or renamed.
            Line line = new Line(new Vector2(0), new Vector2(100));
            // The line will be added to the "Model" layout since this is the active one by default.
            dxf.Entities.Add(line);

            // Create a new Layout, all new layouts will be associated with different PaperSpace blocks,
            // while there can be only one ModelSpace multiple PaperSpace blocks might exist in the document
            Layout layout1 = new Layout("Layout1");

            // When the layout is added to the list, a new PaperSpace block will be created automatically
            dxf.Layouts.Add(layout1);
            // Set this new Layout as the active one. All entities will now be added to this layout.
            dxf.Entities.ActiveLayout = layout1.Name;

            // Create a viewport, this is the window to the ModelSpace
            Viewport viewport1 = new Viewport
            {
                Width = 100,
                Height = 100,
                Center = new Vector3(50, 50, 0),
            };

            // Add it to the "Layout1" since this is the active one
            dxf.Entities.Add(viewport1);
            // Also add a circle
            Circle circle = new Circle(new Vector2(150), 25);
            dxf.Entities.Add(circle);

            // Create a second Layout, add it to the list, and set it as the active one.
            Layout layout2 = new Layout("Layout2");
            dxf.Layouts.Add(layout2);
            dxf.Entities.ActiveLayout = layout2.Name;

            // viewports might have a non rectangular boundary, in this case we will use an ellipse.
            Ellipse ellipse = new Ellipse(new Vector2(100), 200, 150);
            Viewport viewport2 = new Viewport
            {
                ClippingBoundary = ellipse,
            };

            // Add the viewport to the document. This will also add the ellipse to the document.
            dxf.Entities.Add(viewport2);


            Layout layout3 = new Layout("AnyName");
            dxf.Layouts.Add(layout3);
            //layout can also be renamed
            layout3.Name = "Layout3";
            dxf.Entities.ActiveLayout = layout3.Name;
            Viewport test = (Viewport) viewport2.Clone();
            dxf.Entities.Add(test);


            // Save the document as always.
            dxf.Save("PaperSpace.dxf");

            #region CAUTION - This is subject to change in the future, use it with care

            // You cannot directly remove the ellipse from the document since it has been attached to a viewport
            bool ok = dxf.Entities.Remove(ellipse); // OK = false

            // If an entity has been attached to another, its reactor will point to its owner
            // This information is subject to change in the future to become a list, an entity can be attached to multiple objects;
            // but at the moment only the viewport clipping boundary make use of this.
            // This is the way AutoCad also handles hatch and dimension associativity, that I might implement in the future
            DxfObject reactor = ellipse.Reactors[0]; // in this case reactor points to viewport2

            // You need to delete the viewport instead. This deletes the viewport and the ellipse
            //dxf.Entities.Remove(viewport2);

            // another way of deleting the ellipse, is first to assign another clipping boundary to the viewport or just set it to null
            viewport2.ClippingBoundary = null;
            // now it will be possible to delete the ellipse. This will not delete the viewport.
            ok = dxf.Entities.Remove(ellipse); // OK = true

            // Save the document if you want to test the changes
            dxf.Save("PaperSpace2.dxf");

            #endregion

            DxfDocument dxfLoad = DxfDocument.Load("PaperSpace.dxf");

            // For every entity you can check its layout
            // The entity Owner will return the block to which it belongs, it can be a *Model_Space, *Paper_Space, ... or a common block if the entity is part of its geometry.
            // The block record stores information about the block and one of them is the layout, this mimics the way the dxf stores this information.
            // Remember only the internal blocks *Model_Space, *Paper_Space, *Paper_Space0, *Paper_Space1, ... have an associated layout,
            // all other blocks will return null is asked for block.Record.Layout
            Layout associatedLayout = dxfLoad.Entities.Lines.ElementAt(0).Owner.Record.Layout;

            // or you can get the complete list of entities of a layout
            foreach (Layout layout in dxfLoad.Layouts)
            {
                List<DxfObject> entities = dxfLoad.Layouts.GetReferences(layout.Name);
            }

            // You can also remove any layout from the list, except the "Model".
            // Remember all entities that has been added to this layout will also be removed.
            // This mimics the behavior in AutoCad, when a layout is deleted all entities in it will also be deleted.
            dxfLoad.Layouts.Remove(layout1.Name);

            Layout layout4 = (Layout) layout3.Clone("Layout4");
            dxfLoad.Layouts.Add(layout4);
            // when cloning a PaperSpace layout its contents will not be cloned it needs to be done manually after adding the layout to the DXF
            foreach (EntityObject entity in layout3.AssociatedBlock.Entities)
            {
                dxfLoad.Entities.ActiveLayout = layout4.Name;
                dxfLoad.Entities.Add((EntityObject) entity.Clone());
            }

            dxfLoad.Save("PaperSpace removed.dxf");
        }

        private static void BlockWithAttributes()
        {
            DxfDocument dxf = new DxfDocument();
            Block block = new Block("BlockWithAttributes");
            block.Layer = new Layer("BlockSample");
            // It is possible to change the block position, even though it is recommended to keep it at Vector3.Zero,
            // since the block geometry is expressed in local coordinates of the block.
            // The block position defines the base point when inserting an Insert entity.
            block.Origin = new Vector3(10, 5, 0);

            // create an attribute definition, the attDef tag must be unique as it is the way to identify the attribute.
            // even thought AutoCad allows multiple attribute definition in block definitions, it is not recommended
            AttributeDefinition attdef = new AttributeDefinition("NewAttribute");
            // this is the text prompt shown to introduce the attribute value when a new Insert entity is inserted into the drawing
            attdef.Prompt = "InfoText";
            // optionally we can set a default value for new Insert entities
            attdef.Value = "0";
            // the attribute definition position is in local coordinates to the Insert entity to which it belongs
            attdef.Position = new Vector3(1, 1, 0);

            // modifying directly the text style might not get the desired results. Create one or get one from the text style table, modify it and assign it to the attribute text style.
            // one thing to note, if there is already a text style with the assigned name, the existing one in the text style table will override the new one.
            //attdef.Style.IsVertical = true;

            TextStyle txt = new TextStyle("MyStyle", "Arial.ttf");
            txt.IsVertical = true;
            attdef.Style = txt;
            attdef.WidthFactor = 2;
            // not all alignment options are available for TTF fonts 
            attdef.Alignment = TextAlignment.MiddleCenter;
            attdef.Rotation = 90;

            // remember, netDxf does not allow adding attribute definitions with the same tag, even thought AutoCad allows this behavior, it is not recommended in anyway.
            // internally attributes and their associated attribute definitions are handled through dictionaries,
            // and the tags work as ids to easily identify the information stored in the attribute value.
            // When reading a file the attributes or attribute definitions with duplicate tags will be automatically removed.
            // This is subject to change on public demand, it is possible to reimplement this behavior with simple collections to allow for duplicate tags.
            block.AttributeDefinitions.Add(attdef);

            // The entities list defines the actual geometry of the block, they are expressed in th block local coordinates
            Line line1 = new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0));
            Line line2 = new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0));
            block.Entities.Add(line1);
            block.Entities.Add(line2);

            // You can check the entity ownership with:
            Block line1Owner = line1.Owner;
            Block line2Owner = line2.Owner;
            // in this example line1Oner = line2Owner = block
            // As explained in the PaperSpace() sample, the layout associated with a common block will always be null
            Layout associatedLayout = line1.Owner.Record.Layout;
            // associatedLayout = null

            // create an Insert entity with the block definition, during the initialization the Insert attributes list will be created with the default attdef properties
            Insert insert1 = new Insert(block)
            {
                Position = new Vector3(5, 5, 5),
                Normal = new Vector3(1, 1, 1),
                Rotation = 45
            };

            // When the insert position, rotation, normal and/or scale are modified we need to transform the attributes.
            // It is not recommended to manually change the attribute position and orientation and let the Insert entity handle the transformations to maintain them in the same local position.
            // In this particular case we have changed the position, normal and rotation.
            insert1.TransformAttributes();

            // Once the insert has been created we can modify the attributes properties, the list cannot be modified only the items stored in it
            insert1.Attributes[0].Value = 24.ToString();

            // Modifying directly the layer might not get the desired results. Create one or get one from the layers table, modify it and assign it to the insert
            // One thing to note, if there is already a layer with the same name, the existing one in the layers table will override the new one, when the entity is added to the document.
            Layer layer = new Layer("MyInsertLayer");
            layer.Color.Index = 4;

            // optionally we can add the new layer to the document, if not the new layer will be added to the Layers collection when the insert entity is added to the document
            // in case a new layer is found in the list the add method will return the layer already stored in the list
            // this behavior is similar for all TableObject elements, all table object names must be unique (case insensitive)
            layer = dxf.Layers.Add(layer);

            // assign the new layer to the insert
            insert1.Layer = layer;

            // add the entity to the document
            dxf.Entities.Add(insert1);

            // create a second insert entity
            // the constructor will automatically reposition the insert2 attributes to the insert local position
            Insert insert2 = new Insert(block, new Vector3(10, 5, 0));

            // as before now we can change the insert2 attribute value
            insert2.Attributes[0].Value = 34.ToString();

            // additionally we can insert extended data information
            XData xdata1 = new XData(new ApplicationRegistry("netDxf"));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata1.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, 0.0));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, 0.0));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, 0.0));
            xdata1.XDataRecord.Add(XDataRecord.CloseControlString);

            insert2.XData.Add(xdata1);
            dxf.Entities.Add(insert2);

            // all entities support this feature
            XData xdata2 = new XData(new ApplicationRegistry("MyApplication1"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata2.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.String, "string record"));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 350));
            xdata2.XDataRecord.Add(XDataRecord.CloseControlString);

            // multiple extended data entries might be added
            XData xdata3 = new XData(new ApplicationRegistry("MyApplication2"));
            xdata3.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata3.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata3.XDataRecord.Add(new XDataRecord(XDataCode.String, "string record"));
            xdata3.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata3.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 350));
            xdata3.XDataRecord.Add(XDataRecord.CloseControlString);

            Circle circle = new Circle(Vector3.Zero, 5);
            circle.Layer = new Layer("MyCircleLayer");
            // AutoCad 2000 does not support true colors, in that case an approximated color index will be used instead
            circle.Layer.Color = new AciColor(Color.MediumSlateBlue);
            circle.XData.Add(xdata2);
            circle.XData.Add(xdata3);

            dxf.Entities.Add(circle);

            dxf.Save("BlockWithAttributes.dxf");
            DxfDocument dxfLoad = DxfDocument.Load("BlockWithAttributes.dxf");
        }

        #endregion

        private static void ShowDxfDocumentInformation(DxfDocument dxf)
        {
            Console.WriteLine("FILE VERSION: {0}", dxf.DrawingVariables.AcadVer);
            Console.WriteLine();
            Console.WriteLine("FILE COMMENTS: {0}", dxf.Comments.Count);
            foreach (var o in dxf.Comments)
            {
                Console.WriteLine("\t{0}", o);
            }
            Console.WriteLine();
            Console.WriteLine("FILE TIME:");
            Console.WriteLine("\tdrawing created (UTC): {0}.{1}", dxf.DrawingVariables.TduCreate, dxf.DrawingVariables.TduCreate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing last update (UTC): {0}.{1}", dxf.DrawingVariables.TduUpdate, dxf.DrawingVariables.TduUpdate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing edition time: {0}", dxf.DrawingVariables.TdinDwg);
            Console.WriteLine();
            Console.WriteLine("APPLICATION REGISTRIES: {0}", dxf.ApplicationRegistries.Count);
            foreach (var o in dxf.ApplicationRegistries)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ApplicationRegistries.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("LAYERS: {0}", dxf.Layers.Count);
            foreach (var o in dxf.Layers)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layers.GetReferences(o).Count);
                Debug.Assert(ReferenceEquals(o.Linetype, dxf.Linetypes[o.Linetype.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("LINE TYPES: {0}", dxf.Linetypes.Count);
            foreach (var o in dxf.Linetypes)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Linetypes.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("TEXT STYLES: {0}", dxf.TextStyles.Count);
            foreach (var o in dxf.TextStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.TextStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("SHAPE STYLES: {0}", dxf.ShapeStyles.Count);
            foreach (var o in dxf.ShapeStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ShapeStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DIMENSION STYLES: {0}", dxf.DimensionStyles.Count);
            foreach (var o in dxf.DimensionStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.DimensionStyles.GetReferences(o.Name).Count);
                Debug.Assert(ReferenceEquals(o.TextStyle, dxf.TextStyles[o.TextStyle.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.DimLineLinetype, dxf.Linetypes[o.DimLineLinetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine1Linetype, dxf.Linetypes[o.ExtLine1Linetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine2Linetype, dxf.Linetypes[o.ExtLine2Linetype.Name]), "Object reference not equal.");
                if (o.DimArrow1 != null) Debug.Assert(ReferenceEquals(o.DimArrow1, dxf.Blocks[o.DimArrow1.Name]), "Object reference not equal.");
                if (o.DimArrow2 != null) Debug.Assert(ReferenceEquals(o.DimArrow2, dxf.Blocks[o.DimArrow2.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("MLINE STYLES: {0}", dxf.MlineStyles.Count);
            foreach (var o in dxf.MlineStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.MlineStyles.GetReferences(o.Name).Count);
                foreach (var e in o.Elements)
                {
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                }
            }
            Console.WriteLine();

            Console.WriteLine("UCSs: {0}", dxf.UCSs.Count);
            foreach (var o in dxf.UCSs)
            {
                Console.WriteLine("\t{0}", o.Name);
            }
            Console.WriteLine();

            Console.WriteLine("BLOCKS: {0}", dxf.Blocks.Count);
            foreach (var o in dxf.Blocks)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Blocks.GetReferences(o.Name).Count);
                Debug.Assert(ReferenceEquals(o.Layer, dxf.Layers[o.Layer.Name]), "Object reference not equal.");

                foreach (var e in o.Entities)
                {
                    Debug.Assert(ReferenceEquals(e.Layer, dxf.Layers[e.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Owner, dxf.Blocks[o.Name]), "Object reference not equal.");
                    foreach (var x in e.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }

                    if (e is Text txt) Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");

                    if (e is MText mtxt) Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline) Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");

                    if (e is Image img) Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }

                foreach (var a in o.AttributeDefinitions.Values)
                {
                    Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                    foreach (var x in a.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("LAYOUTS: {0}", dxf.Layouts.Count);
            foreach (var o in dxf.Layouts)
            {
                Debug.Assert(ReferenceEquals(o.AssociatedBlock, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");

                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layouts.GetReferences(o.Name).Count);
                List<DxfObject> entities = dxf.Layouts.GetReferences(o.Name);
                foreach (var e in entities)
                {
                    if (e is EntityObject entity)
                    {
                        Debug.Assert(ReferenceEquals(entity.Layer, dxf.Layers[entity.Layer.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(entity.Linetype, dxf.Linetypes[entity.Linetype.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(entity.Owner, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");
                        foreach (var x in entity.XData.Values)
                        {
                            Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                        }
                    }

                    if (e is Text txt) Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");

                    if (e is MText mtxt) Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline) Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");

                    if (e is Image img) Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("IMAGE DEFINITIONS: {0}", dxf.ImageDefinitions.Count);
            foreach (var o in dxf.ImageDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.ImageDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DGN UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDgnDefinitions.Count);
            foreach (var o in dxf.UnderlayDgnDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDgnDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DWF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDwfDefinitions.Count);
            foreach (var o in dxf.UnderlayDwfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDwfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("PDF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayPdfDefinitions.Count);
            foreach (var o in dxf.UnderlayPdfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayPdfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("GROUPS: {0}", dxf.Groups.Count);
            foreach (var o in dxf.Groups)
            {
                Console.WriteLine("\t{0}; Entities count: {1}", o.Name, o.Entities.Count);
            }
            Console.WriteLine();

            // the entities lists contain the geometry that has a graphical representation in the drawing across all layouts,
            // to get the entities that belongs to a specific layout you can get the references through the Layouts.GetReferences(name)
            // or check the EntityObject.Owner.Record.Layout property
            Console.WriteLine("ENTITIES:");
            Console.WriteLine("\t{0}; count: {1}", EntityType.Arc, dxf.Entities.Arcs.Count());
            //Console.WriteLine("\t{0}; count: {1}", EntityType.AttributeDefinition, dxf.AttributeDefinitions.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Circle, dxf.Entities.Circles.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Dimension, dxf.Entities.Dimensions.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ellipse, dxf.Entities.Ellipses.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Face3D, dxf.Entities.Faces3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Hatch, dxf.Entities.Hatches.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Image, dxf.Entities.Images.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Insert, dxf.Entities.Inserts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Leader, dxf.Entities.Leaders.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Line, dxf.Entities.Lines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Mesh, dxf.Entities.Meshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MLine, dxf.Entities.MLines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MText, dxf.Entities.MTexts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Point, dxf.Entities.Points.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolyfaceMesh, dxf.Entities.PolyfaceMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolygonMesh, dxf.Entities.PolygonMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline2D, dxf.Entities.Polylines2D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline3D, dxf.Entities.Polylines3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Shape, dxf.Entities.Shapes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Solid, dxf.Entities.Solids.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Spline, dxf.Entities.Splines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Text, dxf.Entities.Texts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ray, dxf.Entities.Rays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Underlay, dxf.Entities.Underlays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Viewport, dxf.Entities.Viewports.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Wipeout, dxf.Entities.Wipeouts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.XLine, dxf.Entities.XLines.Count());
            Console.WriteLine();

            Console.WriteLine("Press a key to continue...");
            Console.ReadLine();
        }

        private static DxfDocument Test(string file, string output = null)
        {
            // optionally you can save the information to a text file
            bool outputLog = !string.IsNullOrEmpty(output);
            TextWriter writer = null;
            if (outputLog)
            {
                writer = new StreamWriter(File.Create(output));
                Console.SetOut(writer);
            }

            // check if the dxf actually exists
            FileInfo fileInfo = new FileInfo(file);

            if (!fileInfo.Exists)
            {
                Console.WriteLine("THE FILE {0} DOES NOT EXIST", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }
            bool isBinary;
            DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file, out isBinary);

            // check if the file is a dxf
            if (dxfVersion == DxfVersion.Unknown)
            {
                Console.WriteLine("THE FILE {0} IS NOT A VALID DXF OR THE DXF DOES NOT INCLUDE VERSION INFORMATION IN THE HEADER SECTION", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // check if the dxf file version is supported
            if (dxfVersion < DxfVersion.AutoCad2000)
            {
                Console.WriteLine("THE FILE {0} IS NOT A SUPPORTED DXF", file);
                Console.WriteLine();

                Console.WriteLine("FILE VERSION: {0}", dxfVersion);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            DxfDocument dxf = DxfDocument.Load(file, new List<string> {@".\Support"});
            watch.Stop();

            // check if there has been any problems loading the file,
            // this might be the case of a corrupt file or a problem in the library
            if (dxf == null)
            {
                Console.WriteLine("ERROR LOADING {0}", file);
                Console.WriteLine();

                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // the dxf has been properly loaded, let's show some information about it
            Console.WriteLine("FILE NAME: {0}", file);
            Console.WriteLine("\tbinary DXF: {0}", isBinary);
            Console.WriteLine("\tloading time: {0} seconds", watch.ElapsedMilliseconds / 1000.0);
            Console.WriteLine();
            Console.WriteLine("FILE VERSION: {0}", dxf.DrawingVariables.AcadVer);
            Console.WriteLine();
            Console.WriteLine("FILE COMMENTS: {0}", dxf.Comments.Count);
            foreach (var o in dxf.Comments)
            {
                Console.WriteLine("\t{0}", o);
            }
            Console.WriteLine();
            Console.WriteLine("FILE TIME:");
            Console.WriteLine("\tdrawing created (UTC): {0}.{1}", dxf.DrawingVariables.TduCreate, dxf.DrawingVariables.TduCreate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing last update (UTC): {0}.{1}", dxf.DrawingVariables.TduUpdate, dxf.DrawingVariables.TduUpdate.Millisecond.ToString("000"));
            Console.WriteLine("\tdrawing edition time: {0}", dxf.DrawingVariables.TdinDwg);
            Console.WriteLine();
            Console.WriteLine("APPLICATION REGISTRIES: {0}", dxf.ApplicationRegistries.Count);
            foreach (var o in dxf.ApplicationRegistries)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ApplicationRegistries.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("LAYERS: {0}", dxf.Layers.Count);
            foreach (var o in dxf.Layers)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layers.GetReferences(o).Count);
                Debug.Assert(ReferenceEquals(o.Linetype, dxf.Linetypes[o.Linetype.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("LINE TYPES: {0}", dxf.Linetypes.Count);
            foreach (var o in dxf.Linetypes)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Linetypes.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("TEXT STYLES: {0}", dxf.TextStyles.Count);
            foreach (var o in dxf.TextStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.TextStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("SHAPE STYLES: {0}", dxf.ShapeStyles.Count);
            foreach (var o in dxf.ShapeStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.ShapeStyles.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DIMENSION STYLES: {0}", dxf.DimensionStyles.Count);
            foreach (var o in dxf.DimensionStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.DimensionStyles.GetReferences(o.Name).Count);
                Debug.Assert(ReferenceEquals(o.TextStyle, dxf.TextStyles[o.TextStyle.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.DimLineLinetype, dxf.Linetypes[o.DimLineLinetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine1Linetype, dxf.Linetypes[o.ExtLine1Linetype.Name]), "Object reference not equal.");
                Debug.Assert(ReferenceEquals(o.ExtLine2Linetype, dxf.Linetypes[o.ExtLine2Linetype.Name]), "Object reference not equal.");
                if (o.DimArrow1 != null) Debug.Assert(ReferenceEquals(o.DimArrow1, dxf.Blocks[o.DimArrow1.Name]), "Object reference not equal.");
                if (o.DimArrow2 != null) Debug.Assert(ReferenceEquals(o.DimArrow2, dxf.Blocks[o.DimArrow2.Name]), "Object reference not equal.");
            }
            Console.WriteLine();

            Console.WriteLine("MLINE STYLES: {0}", dxf.MlineStyles.Count);
            foreach (var o in dxf.MlineStyles)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.MlineStyles.GetReferences(o.Name).Count);
                foreach (var e in o.Elements)
                {
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                }
            }
            Console.WriteLine();

            Console.WriteLine("UCSs: {0}", dxf.UCSs.Count);
            foreach (var o in dxf.UCSs)
            {
                Console.WriteLine("\t{0}", o.Name);
            }
            Console.WriteLine();

            Console.WriteLine("BLOCKS: {0}", dxf.Blocks.Count);
            foreach (var o in dxf.Blocks)
            {
                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Blocks.GetReferences(o.Name).Count);
                Debug.Assert(ReferenceEquals(o.Layer, dxf.Layers[o.Layer.Name]), "Object reference not equal.");

                foreach (var e in o.Entities)
                {
                    Debug.Assert(ReferenceEquals(e.Layer, dxf.Layers[e.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Linetype, dxf.Linetypes[e.Linetype.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(e.Owner, dxf.Blocks[o.Name]), "Object reference not equal.");
                    foreach (var x in e.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }

                    if (e is Text txt)
                        Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");

                    if (e is MText mtxt)
                        Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline)                                   
                        Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");

                    if (e is Image img)
                        Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }

                foreach (var a in o.AttributeDefinitions.Values)
                {
                    Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                    Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                    foreach (var x in a.XData.Values)
                    {
                        Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("LAYOUTS: {0}", dxf.Layouts.Count);
            foreach (var o in dxf.Layouts)
            {
                Debug.Assert(ReferenceEquals(o.AssociatedBlock, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");

                Console.WriteLine("\t{0}; References count: {1}", o.Name, dxf.Layouts.GetReferences(o.Name).Count);
                List<DxfObject> entities = dxf.Layouts.GetReferences(o.Name);
                foreach (var e in entities)
                {
                    if (e is EntityObject entity)
                    {
                        Debug.Assert(ReferenceEquals(entity.Layer, dxf.Layers[entity.Layer.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(entity.Linetype, dxf.Linetypes[entity.Linetype.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(entity.Owner, dxf.Blocks[o.AssociatedBlock.Name]), "Object reference not equal.");
                        foreach (var x in entity.XData.Values)
                        {
                            Debug.Assert(ReferenceEquals(x.ApplicationRegistry, dxf.ApplicationRegistries[x.ApplicationRegistry.Name]), "Object reference not equal.");
                        }
                    }

                    if (e is Text txt)
                        Debug.Assert(ReferenceEquals(txt.Style, dxf.TextStyles[txt.Style.Name]), "Object reference not equal.");

                    if (e is MText mtxt)
                        Debug.Assert(ReferenceEquals(mtxt.Style, dxf.TextStyles[mtxt.Style.Name]), "Object reference not equal.");

                    if (e is Dimension dim)
                    {
                        Debug.Assert(ReferenceEquals(dim.Style, dxf.DimensionStyles[dim.Style.Name]), "Object reference not equal.");
                        Debug.Assert(ReferenceEquals(dim.Block, dxf.Blocks[dim.Block.Name]), "Object reference not equal.");
                    }

                    if (e is MLine mline)
                        Debug.Assert(ReferenceEquals(mline.Style, dxf.MlineStyles[mline.Style.Name]), "Object reference not equal.");

                    if (e is Image img)
                        Debug.Assert(ReferenceEquals(img.Definition, dxf.ImageDefinitions[img.Definition.Name]), "Object reference not equal.");

                    if (e is Insert ins)
                    {
                        Debug.Assert(ReferenceEquals(ins.Block, dxf.Blocks[ins.Block.Name]), "Object reference not equal.");
                        foreach (var a in ins.Attributes)
                        {
                            Debug.Assert(ReferenceEquals(a.Layer, dxf.Layers[a.Layer.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Linetype, dxf.Linetypes[a.Linetype.Name]), "Object reference not equal.");
                            Debug.Assert(ReferenceEquals(a.Style, dxf.TextStyles[a.Style.Name]), "Object reference not equal.");
                        }
                    }
                }
            }
            Console.WriteLine();

            Console.WriteLine("IMAGE DEFINITIONS: {0}", dxf.ImageDefinitions.Count);
            foreach (var o in dxf.ImageDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.ImageDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DGN UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDgnDefinitions.Count);
            foreach (var o in dxf.UnderlayDgnDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDgnDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("DWF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayDwfDefinitions.Count);
            foreach (var o in dxf.UnderlayDwfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayDwfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("PDF UNDERLAY DEFINITIONS: {0}", dxf.UnderlayPdfDefinitions.Count);
            foreach (var o in dxf.UnderlayPdfDefinitions)
            {
                Console.WriteLine("\t{0}; File name: {1}; References count: {2}", o.Name, o.File, dxf.UnderlayPdfDefinitions.GetReferences(o.Name).Count);
            }
            Console.WriteLine();

            Console.WriteLine("GROUPS: {0}", dxf.Groups.Count);
            foreach (var o in dxf.Groups)
            {
                Console.WriteLine("\t{0}; Entities count: {1}", o.Name, o.Entities.Count);
            }
            Console.WriteLine();

            Console.WriteLine("ATTRIBUTE DEFINITIONS for the \"Model\" Layout: {0}", dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions.Count);
            foreach (var o in dxf.Layouts[Layout.ModelSpaceName].AssociatedBlock.AttributeDefinitions)
            {
                Console.WriteLine("\tTag: {0}", o.Value.Tag);
            }
            Console.WriteLine();

            // the entities lists contain the geometry that has a graphical representation in the drawing across all layouts,
            // to get the entities that belongs to a specific layout you can get the references through the Layouts.GetReferences(name)
            // or check the EntityObject.Owner.Record.Layout property
            Console.WriteLine("ENTITIES for the Active Layout = {0}:", dxf.Entities.ActiveLayout);
            Console.WriteLine("\t{0}; count: {1}", EntityType.Arc, dxf.Entities.Arcs.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Circle, dxf.Entities.Circles.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Dimension, dxf.Entities.Dimensions.Count());
            foreach (var a in dxf.Entities.Dimensions)
            {
                foreach (var styleOverride in a.StyleOverrides.Values)
                {
                    switch (styleOverride.Type)
                    {
                        case DimensionStyleOverrideType.DimLineLinetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine1Linetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine2Linetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.TextStyle:
                            Debug.Assert(ReferenceEquals((TextStyle) styleOverride.Value, dxf.TextStyles[((TextStyle) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.LeaderArrow:
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow1:
                            if(styleOverride.Value == null) break;
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow2:
                            if (styleOverride.Value == null) break;
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                    }
                }
            }
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ellipse, dxf.Entities.Ellipses.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Face3D, dxf.Entities.Faces3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Hatch, dxf.Entities.Hatches.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Image, dxf.Entities.Images.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Insert, dxf.Entities.Inserts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Leader, dxf.Entities.Leaders.Count());
            foreach (var a in dxf.Entities.Leaders)
            {
                foreach (var styleOverride in a.StyleOverrides.Values)
                {
                    switch (styleOverride.Type)
                    {
                        case DimensionStyleOverrideType.DimLineLinetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine1Linetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.ExtLine2Linetype:
                            Debug.Assert(ReferenceEquals((Linetype) styleOverride.Value, dxf.Linetypes[((Linetype) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.TextStyle:
                            Debug.Assert(ReferenceEquals((TextStyle) styleOverride.Value, dxf.TextStyles[((TextStyle) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.LeaderArrow:
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow1:
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                        case DimensionStyleOverrideType.DimArrow2:
                            Debug.Assert(ReferenceEquals((Block) styleOverride.Value, dxf.Blocks[((Block) styleOverride.Value).Name]), "Object reference not equal.");
                            break;
                    }
                }
            }
            Console.WriteLine("\t{0}; count: {1}", EntityType.Line, dxf.Entities.Lines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Mesh, dxf.Entities.Meshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MLine, dxf.Entities.MLines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.MText, dxf.Entities.MTexts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Point, dxf.Entities.Points.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolyfaceMesh, dxf.Entities.PolyfaceMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.PolygonMesh, dxf.Entities.PolygonMeshes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline2D, dxf.Entities.Polylines2D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Polyline3D, dxf.Entities.Polylines3D.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Shape, dxf.Entities.Shapes.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Solid, dxf.Entities.Solids.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Spline, dxf.Entities.Splines.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Text, dxf.Entities.Texts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Ray, dxf.Entities.Rays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Underlay, dxf.Entities.Underlays.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Viewport, dxf.Entities.Viewports.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.Wipeout, dxf.Entities.Wipeouts.Count());
            Console.WriteLine("\t{0}; count: {1}", EntityType.XLine, dxf.Entities.XLines.Count());
            Console.WriteLine();

            // the dxf version is controlled by the DrawingVariables property of the dxf document,
            // also a HeaderVariables instance or a DxfVersion can be passed to the constructor to initialize a new DxfDocument.
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2018;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2018.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2018 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2013;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2013.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2013 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2010.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2010 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2007;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2007.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2007 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2004.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2004 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            watch.Reset();
            watch.Start();
            dxf.Save("sample 2000.dxf");
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("DXF version AutoCad2000 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            // saving to binary dxf
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            watch.Reset();
            watch.Start();
            dxf.Save("binary test.dxf", true);
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("Binary DXF version AutoCad2010 saved in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            watch.Reset();
            watch.Start();
            DxfDocument test = DxfDocument.Load("binary test.dxf", new List<string> { @".\Support" });
            watch.Stop();
            Console.WriteLine();
            Console.WriteLine("Binary DXF version AutoCad2010 loaded in {0} seconds", watch.ElapsedMilliseconds/1000.0);

            if (outputLog)
            {
                writer.Flush();
                writer.Close();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();
            }
            return dxf;
        }

        private static void ImageAndClipBoundary()
        {
            // a square bitmap
            string imgFile = "image.jpg";
            System.Drawing.Image img = System.Drawing.Image.FromFile(imgFile);
            ImageDefinition imageDefinition = new ImageDefinition("MyImage", imgFile, img.Width, img.HorizontalResolution, img.Height, img.VerticalResolution, ImageResolutionUnits.Inches);

            // image with the same aspect ratio as the original bitmap
            Image image1 = new Image(imageDefinition, Vector2.Zero, 360, 360 * (imageDefinition.Height / imageDefinition.Width));
            // image stretched not the same aspect ratio as the original bitmap
            Image image2 = new Image(imageDefinition, new Vector2(0, 500), 200, 100);
            image2.ClippingBoundary = new ClippingBoundary(new Vector2(0, 0), new Vector2(image2.Definition.Width * 0.5, image2.Definition.Height * 0.5));
            image2.Clipping = true;
            // image copy of image1, later we will change its position and size
            Image image3 = (Image)image1.Clone();
            DxfDocument doc = new DxfDocument();
            doc.Entities.Add(image1);
            doc.Entities.Add(image2);
            // change the position of image3
            image3.Position = new Vector3(500, 0, 0);
            // resize image3, double the size
            image3.Width *= 2;
            image3.Height *= 2;

            // the image boundary vertexes are in local coordinates in pixels
            Vector2[] wcsBoundary =
            {
                new Vector2(image3.Definition.Width*0.5,0),
                new Vector2(image3.Definition.Width,image3.Definition.Height*0.5),
                new Vector2(image3.Definition.Width*0.5,image3.Definition.Height),
                new Vector2(0,image3.Definition.Height*0.5)
            };

            image3.ClippingBoundary = new ClippingBoundary(wcsBoundary);
            image3.Clipping = true;
            doc.Entities.Add(image3);

            //Vector2[] woVertexes =
            //{
            //    new Vector2(180,0),
            //    new Vector2(360,180),
            //    new Vector2(180,360),
            //    new Vector2(0,180)
            //};
            //Wipeout wipeout = new Wipeout(woVertexes);
            Wipeout wipeout = new Wipeout(0, 0, 180, 180);
            doc.Entities.Add(wipeout);
            doc.Save("test.dxf");
        }

        private static void LinearDimensionTests()
        {
            //DxfDocument dxf1 = new DxfDocument();
            //Vector2 pt1 = new Vector2(15, -5);
            //Vector2 pt2 = new Vector2(5, 5);
            //double offset = 10;

            //LinearDimension ld1z = new LinearDimension(pt1, pt2, offset, 30);
            //LinearDimension ld2z = new LinearDimension(pt1, pt2, offset, 45);
            //LinearDimension ld3z = new LinearDimension(pt1, pt2, offset, 90);
            //LinearDimension ld4z = new LinearDimension(pt1, pt2, offset, 135);
            //LinearDimension ld5z = new LinearDimension(pt1, pt2, offset, 180);
            //LinearDimension ld6z = new LinearDimension(pt1, pt2, offset, 220);
            //LinearDimension ld7z = new LinearDimension(pt2, pt1, offset, 270);

            //dxf1.Entities.Add(ld1z);
            //dxf1.Entities.Add(ld2z);
            //dxf1.Entities.Add(ld3z);
            //dxf1.Entities.Add(ld4z);
            //dxf1.Entities.Add(ld5z);
            //dxf1.Entities.Add(ld6z);
            //dxf1.Entities.Add(ld7z);

            //Line line = new Line(pt1, pt2);
            //line.Color = AciColor.Yellow;
            //dxf1.Entities.Add(line);

            //dxf1.Save("test2.dxf");

            DxfDocument dxf2 = new DxfDocument();

            //LinearDimension ld1 = new LinearDimension(new Vector2(0, 0), new Vector2(0, 15), 1, 90);
            //LinearDimension ld1b = new LinearDimension(new Vector2(0, 0), new Vector2(0, 15), 1, 100);
            //LinearDimension ld1c = new LinearDimension(new Vector2(0, 0), new Vector2(0, 15), 1, 80);

            //LinearDimension ld2 = new LinearDimension(new Vector2(5, 15), new Vector2(5, 0), 1, 90);
            LinearDimension ld3 = new LinearDimension(new Vector2(10, 15), new Vector2(10, 0), 1, 90);
            //ld3.SetDimensionLinePosition(new Vector3(9, 1, 0));
            //LinearDimension ld4 = new LinearDimension(new Vector2(15, 0), new Vector2(15, 15), 1, 270);
            //LinearDimension ld4b = new LinearDimension(new Vector2(15, 0), new Vector2(15, 15), 1, 300);
            //LinearDimension ld4c = new LinearDimension(new Vector2(15, 0), new Vector2(15, 15), 1, 240);

            //LinearDimension ld5 = new LinearDimension(new Vector2(15, 0), new Vector2(0, 0), 1, 0);
            //LinearDimension ld6 = new LinearDimension(new Vector2(15, 0),new Vector2(0, 0),  1, 0);

            //AlignedDimension ld1a = new AlignedDimension(new Vector2(0, 0), new Vector2(0, 15), 1);
            //ld1a.Color = AciColor.Yellow;
            //AlignedDimension ld2a = new AlignedDimension(new Vector2(5, 15), new Vector2(5, 0), 1);
            //ld2a.Color = AciColor.Yellow;
            //AlignedDimension ld3a = new AlignedDimension(new Vector2(10, 0), new Vector2(10, 15), -1);
            //ld3a.Color = AciColor.Yellow;
            //AlignedDimension ld4a = new AlignedDimension(new Vector2(15, 0), new Vector2(15, 15), 1);
            //ld4a.Color = AciColor.Yellow;
            //AlignedDimension ld5a = new AlignedDimension(new Vector2(15, 0), new Vector2(0, 0), 1);
            //ld5a.Color = AciColor.Yellow;
            //AlignedDimension ld6a = new AlignedDimension(new Vector2(0, 0), new Vector2(15, 0), -1);
            //ld6a.Color = AciColor.Yellow;

            //dxf2.Entities.Add(ld1);
            //dxf2.Entities.Add(ld1b);
            //dxf2.Entities.Add(ld1c);

            //dxf2.Entities.Add(ld2);
            dxf2.Entities.Add(ld3);

            //dxf2.Entities.Add(ld4);
            //dxf2.Entities.Add(ld4b);
            //dxf2.Entities.Add(ld4c);

            //dxf2.Entities.Add(ld5);
            //dxf2.Entities.Add(ld6);

            //dxf2.Entities.Add(ld1a);
            //dxf2.Entities.Add(ld2a);
            //dxf2.Entities.Add(ld3a);
            //dxf2.Entities.Add(ld4a);
            //dxf2.Entities.Add(ld5a);
            //dxf2.Entities.Add(ld6a);

            dxf2.Save("test1.dxf");

            DxfDocument load = DxfDocument.Load("test1.dxf");
            load.Entities.Dimensions.ElementAt(0).Update();
            //load.Dimensions[1].Update();
            load.Entities.Add((EntityObject) ld3.Clone());
            //load.Entities.Add((EntityObject)ld6.Clone());
            load.Save("test2.dxf");
        }

        private static void TestingTrueTypeFonts()
        {
            DxfDocument dxfText = new DxfDocument();
            TextStyle textStyle1 = new TextStyle(@"arial.ttf");
            TextStyle textStyle2 = new TextStyle("arialbi.ttf");
            TextStyle textStyle3 = new TextStyle(@"C:\Windows\Fonts\91118.ttf");
            textStyle3.Name = textStyle3.FontFamilyName;
            Text text1 = new Text("testing", Vector2.Zero, 6, textStyle1);
            Text text2 = new Text("testing", Vector2.Zero, 6, textStyle2);
            Text text3 = new Text("testing", Vector2.Zero, 6, textStyle3);
            dxfText.Entities.Add(text1);
            dxfText.Entities.Add(text2);
            dxfText.Entities.Add(text3);

            dxfText.Save("test.dxf");
            DxfDocument load = DxfDocument.Load("test.dxf");
            load.Save("test compare.dxf");
        }

        private static void EncodingTest()
        {
            DxfDocument dxf;
            dxf = DxfDocument.Load("tests//EncodeDecodeProcess (cad 2010).dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            dxf.Save("EncodeDecodeProcess (netDxf 2000).dxf");

            dxf = DxfDocument.Load("tests//EncodeDecodeProcess (cad 2000).dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("EncodeDecodeProcess (netDxf 2010).dxf");
        }

        private static void CheckReferences()
        {
            DxfDocument dxf = new DxfDocument();

            Layer layer1 = new Layer("Layer1");
            layer1.Color = AciColor.Blue;
            layer1.Linetype = Linetype.Center;

            Layer layer2 = new Layer("Layer2");
            layer2.Color = AciColor.Red;

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(0, 0));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(20, 0));
            poly.Vertexes.Add(new Polyline2DVertex(30, 10));
            poly.Layer = layer1;
            dxf.Entities.Add(poly);

            Ellipse ellipse = new Ellipse(new Vector3(2, 2, 0), 5, 3);
            ellipse.Rotation = 30;
            ellipse.Layer = layer1;
            dxf.Entities.Add(ellipse);

            Line line = new Line(new Vector2(10, 5), new Vector2(-10, -5));
            line.Layer = layer2;
            line.Linetype = Linetype.DashDot;
            dxf.Entities.Add(line);

            dxf.Save("test.dxf");

            dxf = DxfDocument.Load("sample.dxf");

            foreach (ApplicationRegistry registry in dxf.ApplicationRegistries)
            {
                foreach (DxfObject o in dxf.ApplicationRegistries.GetReferences(registry))
                {
                    if (o is EntityObject entityObject)
                    {
                        foreach (KeyValuePair<string, XData> data in entityObject.XData)
                        {
                            if (data.Key == registry.Name)
                                if (!ReferenceEquals(registry, data.Value.ApplicationRegistry))
                                    Console.WriteLine("Application registry {0} not equal entity to {1}", registry.Name, entityObject.CodeName);
                        }
                    }
                }
            }

            foreach (Block block in dxf.Blocks)
            {
                foreach (DxfObject o in dxf.Blocks.GetReferences(block))
                {
                    if (o is Insert insert)
                        if (!ReferenceEquals(block, insert.Block))
                            Console.WriteLine("Block {0} not equal entity to {1}", block.Name, insert.CodeName);
                }
            }

            foreach (ImageDefinition def in dxf.ImageDefinitions)
            {
                foreach (DxfObject o in dxf.ImageDefinitions.GetReferences(def))
                {
                    if (o is Image image)
                        if (!ReferenceEquals(def, image.Definition))
                            Console.WriteLine("Image definition {0} not equal entity to {1}", def.Name, image.CodeName);
                }
            }

            foreach (DimensionStyle dimStyle in dxf.DimensionStyles)
            {
                foreach (DxfObject o in dxf.DimensionStyles.GetReferences(dimStyle))
                {
                    if (o is Dimension dimension)
                        if (!ReferenceEquals(dimStyle, dimension.Style))
                            Console.WriteLine("Dimension style {0} not equal entity to {1}", dimStyle.Name, dimension.CodeName);
                }
            }

            foreach (Group g in dxf.Groups)
            {
                foreach (DxfObject o in dxf.Groups.GetReferences(g))
                {
                    // no references
                }
            }

            foreach (UCS u in dxf.UCSs)
            {
                foreach (DxfObject o in dxf.UCSs.GetReferences(u))
                {
                    // no references
                }
            }

            foreach (TextStyle style in dxf.TextStyles)
            {
                foreach (DxfObject o in dxf.TextStyles.GetReferences(style))
                {
                    if (o is Text text)
                        if (!ReferenceEquals(style, text.Style))
                            Console.WriteLine("Text style {0} not equal entity to {1}", style.Name, text.CodeName);

                    if (o is MText mText)
                        if (!ReferenceEquals(style, mText.Style))
                            Console.WriteLine("Text style {0} not equal entity to {1}", style.Name, mText.CodeName);

                    if (o is DimensionStyle dimensionStyle)
                        if (!ReferenceEquals(style, dimensionStyle.TextStyle))
                            Console.WriteLine("Text style {0} not equal entity to {1}", style.Name, dimensionStyle.CodeName);
                }
            }

            foreach (Layer layer in dxf.Layers)
            {
                foreach (DxfObject o in dxf.Layers.GetReferences(layer))
                {
                    if (o is Block block)
                        if (!ReferenceEquals(layer, block.Layer))
                            Console.WriteLine("Layer {0} not equal entity to {1}", layer.Name, block.CodeName);
                    if (o is EntityObject entityObject)
                        if (!ReferenceEquals(layer, entityObject.Layer))
                            Console.WriteLine("Layer {0} not equal entity to {1}", layer.Name, entityObject.CodeName);
                }
            }

            foreach (Linetype lType in dxf.Linetypes)
            {
                foreach (DxfObject o in dxf.Linetypes.GetReferences(lType))
                {
                    if (o is Layer layer)
                        if (!ReferenceEquals(lType, layer.Linetype))
                            Console.WriteLine("Line type {0} not equal to {1}", lType.Name, layer.CodeName);
                    if (o is MLineStyle style)
                    {
                        foreach (MLineStyleElement e in style.Elements)
                        {
                            if (!ReferenceEquals(lType, e.Linetype))
                                Console.WriteLine("Line type {0} not equal to {1}", lType.Name, style.CodeName);
                        }
                    }
                    if (o is EntityObject entityObject)
                        if (!ReferenceEquals(lType, entityObject.Linetype))
                            Console.WriteLine("Line type {0} not equal entity to {1}", lType.Name, entityObject.CodeName);
                }
            }

            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
        }

        private static void DimensionNestedBlock()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = new Vector3(5, 5, 0);
            Line line = new Line(p1, p2);

            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;
            LinearDimension dim = new LinearDimension(line, 7, 0.0, Vector3.UnitZ, myStyle);

            Block nestedBlock = new Block("NestedBlock");
            nestedBlock.Entities.Add(line);
            Insert nestedIns = new Insert(nestedBlock);

            Block block = new Block("MyBlock");
            block.Entities.Add(dim);
            block.Entities.Add(nestedIns);

            Insert ins = new Insert(block);
            ins.Position = new Vector3(10, 10, 0);
            dxf.Entities.Add(ins);

            Circle circle = new Circle(p2, 5);
            Block block2 = new Block("MyBlock2");
            block2.Entities.Add(circle);

            Insert ins2 = new Insert(block2);
            ins2.Position = new Vector3(-10, -10, 0);
            dxf.Entities.Add(ins2);

            Block block3 = new Block("MyBlock3");
            block3.Entities.Add((EntityObject) ins.Clone());
            block3.Entities.Add((EntityObject) ins2.Clone());

            Insert ins3 = new Insert(block3);
            ins3.Position = new Vector3(-10, 10, 0);
            dxf.Entities.Add(ins3);

            dxf.Save("nested blocks.dxf");

            dxf = DxfDocument.Load("nested blocks.dxf");

            dxf.Save("nested blocks.dxf");
        }

        private static void ComplexHatch()
        {
            HatchPattern pattern = HatchPattern.Load("hatch\\acad.pat", "ESCHER");
            pattern.Scale = 1.5;
            pattern.Angle = 30;

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {poly})
            };
            Hatch hatch = new Hatch(pattern, boundary, true);

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(poly);
            dxf.Entities.Add(hatch);
            dxf.Save("complexhatch.dxf");

            DxfDocument dxf2 = DxfDocument.Load("complexhatch.dxf");
            dxf2.Save("complexhatch2.dxf");
        }

        private static void RayAndXLine()
        {
            Ray ray = new Ray(new Vector3(1, 1, 1), new Vector3(1, 1, 1));
            XLine xline = new XLine(Vector2.Zero, new Vector2(1, 1));

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(ray);
            dxf.Entities.Add(xline);
            dxf.Save("RayAndXLine.dxf");


            dxf = DxfDocument.Load("RayAndXLine.dxf");
        }

        private static void UserCoordinateSystems()
        {
            DxfDocument dxf = new DxfDocument();
            UCS ucs1 = new UCS("user1", Vector3.Zero, Vector3.UnitX, Vector3.UnitZ);
            UCS ucs2 = UCS.FromXAxisAndPointOnXYplane("user2", Vector3.Zero, new Vector3(1, 1, 0), new Vector3(1, 1, 1));
            UCS ucs3 = UCS.FromNormal("user3", Vector3.Zero, new Vector3(1, 1, 1));
            dxf.UCSs.Add(ucs1);
            dxf.UCSs.Add(ucs2);
            dxf.UCSs.Add(ucs3);

            dxf.Save("ucs.dxf");

            dxf = DxfDocument.Load("ucs.dxf");
        }

        private static void ImageUsesAndRemove()
        {
            string imgFile1 = @"img\image01.jpg";
            System.Drawing.Image img1 = System.Drawing.Image.FromFile(imgFile1);
            ImageDefinition imageDef1 = new ImageDefinition("MyImage", imgFile1, img1.Width, img1.HorizontalResolution, img1.Height, img1.VerticalResolution, ImageResolutionUnits.Inches);
            Image image1 = new Image(imageDef1, Vector3.Zero, 10, 10);

            string imgFile2 = @"img\image02.jpg";
            System.Drawing.Image img2 = System.Drawing.Image.FromFile(imgFile2);
            ImageDefinition imageDef2 = new ImageDefinition("MyImage", imgFile2, img2.Width, img2.HorizontalResolution, img2.Height, img2.VerticalResolution, ImageResolutionUnits.Inches);

            Image image2 = new Image(imageDef2, new Vector3(0, 220, 0), 10, 10);
            Image image3 = new Image(imageDef2, image2.Position + new Vector3(280, 0, 0), 10, 10);

            Block block = new Block("MyImageBlock");
            block.Entities.Add(image1);

            Insert insert = new Insert(block);

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(insert);
            dxf.Entities.Add(image2);
            dxf.Entities.Add(image3);
            dxf.Save("test netDxf.dxf");


            dxf.Entities.Remove(insert);
            dxf.Blocks.Remove(insert.Block.Name);
            // imageDef1 has no references in the document
            List<DxfObject> uses = dxf.ImageDefinitions.GetReferences(imageDef1.Name);
            dxf.Save("test netDxf with unreferenced imageDef.dxf");
            dxf = DxfDocument.Load("test netDxf with unreferenced imageDef.dxf");

            // once we have removed the insert and then the block that contained image1 we don't have more references to imageDef1
            dxf.ImageDefinitions.Remove(imageDef1.Name);
            dxf.Save("test netDxf with deleted imageDef.dxf");
        }

        private static void LayerAndLinetypesUsesAndRemove()
        {
            DxfDocument dxf = new DxfDocument();

            Layer layer1 = new Layer("Layer1");
            layer1.Color = AciColor.Blue;
            layer1.Linetype = Linetype.Center;

            Layer layer2 = new Layer("Layer2");
            layer2.Color = AciColor.Red;

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(0, 0));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(20, 0));
            poly.Vertexes.Add(new Polyline2DVertex(30, 10));
            poly.Layer = layer1;
            dxf.Entities.Add(poly);

            Ellipse ellipse = new Ellipse(new Vector3(2, 2, 0), 5, 3);
            ellipse.Rotation = 30;
            ellipse.Layer = layer1;
            dxf.Entities.Add(ellipse);

            Line line = new Line(new Vector2(10, 5), new Vector2(-10, -5));
            line.Layer = layer2;
            line.Linetype = Linetype.DashDot;
            dxf.Entities.Add(line);


            bool ok;

            // this will return false since layer1 is not empty
            ok = dxf.Layers.Remove(layer1.Name);

            List<DxfObject> entities = dxf.Layers.GetReferences(layer1.Name);
            foreach (DxfObject o in entities)
            {
                dxf.Entities.Remove(o as EntityObject);
            }

            // now this should return true since layer1 is empty
            ok = dxf.Layers.Remove(layer1.Name);

            // blocks needs an special attention
            Layer layer3 = new Layer("Layer3");
            layer3.Color = AciColor.Yellow;

            Circle circle = new Circle(Vector3.Zero, 15);
            // it is always recommended that all block entities will be located in layer 0, but this is up to the user.
            circle.Layer = new Layer("circle");
            circle.Layer.Color = AciColor.Green;

            Block block = new Block("MyBlock");
            block.Entities.Add(circle);
            block.Layer = new Layer("blockLayer");
            AttributeDefinition attdef = new AttributeDefinition("NewAttribute");
            attdef.Layer = new Layer("attDefLayer");
            attdef.Linetype = Linetype.Center;
            block.AttributeDefinitions.Add(attdef);

            Insert insert = new Insert(block, new Vector2(5, 5));
            insert.Layer = layer3;
            insert.Attributes[0].Layer = new Layer("attLayer");
            insert.Attributes[0].Linetype = Linetype.Dashed;
            dxf.Entities.Add(insert);

            dxf.Save("test.dxf");

            DxfDocument dxf2 = DxfDocument.Load("test.dxf");

            // this list will contain the circle entity
            List<DxfObject> dxfObjects;
            dxfObjects = dxf.Layers.GetReferences("circle");

            // but we cannot removed since it is part of a block
            ok = dxf.Entities.Remove(circle);
            // we need to remove first the block, but to do this we need to make sure there are no references of that block in the document
            dxfObjects = dxf.Blocks.GetReferences(block.Name);
            foreach (DxfObject o in dxfObjects)
            {
                dxf.Entities.Remove(o as EntityObject);
            }


            // now it is safe to remove the block since we do not have more references in the document
            ok = dxf.Blocks.Remove(block.Name);
            // now it is safe to remove the layer "circle", the circle entity was removed with the block since it was part of it
            ok = dxf.Layers.Remove("circle");

            // purge all document layers, only empty layers will be removed
            dxf.Layers.Clear();

            // purge all document line types, only line types without references will be removed
            dxf.Linetypes.Clear();

            dxf.Save("test2.dxf");
        }

        private static void TextAndDimensionStyleUsesAndRemove()
        {
            DxfDocument dxf = new DxfDocument();

            Layer layer1 = new Layer("Layer1");
            layer1.Color = AciColor.Blue;
            layer1.Linetype = Linetype.Center;

            Layer layer2 = new Layer("Layer2");
            layer2.Color = AciColor.Red;

            // blocks needs an special attention
            Layer layer3 = new Layer("Layer3");
            layer3.Color = AciColor.Yellow;

            Circle circle = new Circle(Vector3.Zero, 15);
            // it is always recommended that all block entities will be located in layer 0, but this is up to the user.
            circle.Layer = new Layer("circle");
            circle.Layer.Color = AciColor.Green;

            Block block = new Block("MyBlock");
            block.Entities.Add(circle);
            AttributeDefinition attdef = new AttributeDefinition("NewAttribute");

            block.AttributeDefinitions.Add(attdef);

            Insert insert = new Insert(block, new Vector2(5, 5));
            insert.Attributes[0].Style = new TextStyle("Arial.ttf");

            dxf.Entities.Add(insert);

            dxf.Save("style.dxf");
            DxfDocument dxf2;
            dxf2 = DxfDocument.Load("style.dxf");

            dxf.Entities.Remove(circle);

            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = new Vector3(5, 5, 0);
            Line line = new Line(p1, p2);

            dxf.Entities.Add(line);

            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.TextStyle = new TextStyle("Tahoma.ttf");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;
            double offset = 7;
            LinearDimension dimX = new LinearDimension(line, offset, 0.0, Vector3.UnitZ, myStyle);
            dimX.Rotation += 30.0;
            LinearDimension dimY = new LinearDimension(line, offset, 90.0, Vector3.UnitZ, myStyle);
            dimY.Rotation += 30.0;

            dxf.Entities.Add(dimX);
            dxf.Entities.Add(dimY);

            dxf.Save("style2.dxf");
            dxf2 = DxfDocument.Load("style2.dxf");


            dxf.Entities.Remove(dimX);
            dxf.Entities.Remove(dimY);

            bool ok;

            // we can remove myStyle it was only referenced by dimX and dimY
            ok = dxf.DimensionStyles.Remove(myStyle.Name);

            // we cannot remove myStyle.TextStyle since it is in use by the internal blocks created by the dimension entities
            ok = dxf.Blocks.Remove(dimX.Block.Name);
            ok = dxf.Blocks.Remove(dimY.Block.Name);

            // no we can remove the unreferenced textStyle
            ok = dxf.TextStyles.Remove(myStyle.TextStyle.Name);

            dxf.Save("style3.dxf");
            dxf2 = DxfDocument.Load("style3.dxf");
        }

        private static void MLineStyleUsesAndRemove()
        {
            DxfDocument dxf = new DxfDocument();
            //MLineStyle style = MLineStyle.Default;
            //dxf.AddMLineStyle(style);

            List<Vector2> vertexes = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 150),
                new Vector2(150, 150),
                new Vector2(150, 0)
            };

            MLine mline = new MLine(vertexes);
            mline.Scale = 20;
            mline.Justification = MLineJustification.Zero;
            //mline.IsClosed = true;

            MLineStyle style = new MLineStyle("MyStyle", "Personalized style.");
            style.Elements.Add(new MLineStyleElement(0.25));
            style.Elements.Add(new MLineStyleElement(-0.25));
            // if we add new elements directly to the list we need to sort the list,
            style.Elements.Sort();
            style.Flags = MLineStyleFlags.EndInnerArcsCap | MLineStyleFlags.EndRoundCap | MLineStyleFlags.StartInnerArcsCap | MLineStyleFlags.StartRoundCap;
            //style.StartAngle = 25.0;
            //style.EndAngle = 160.0;
            // AutoCad2000 dxf version does not support true colors for MLineStyle elements
            style.Elements[0].Color = new AciColor(180, 230, 147);
            mline.Style = style;
            // we have modified the mline after setting its vertexes so we need to manually call this method.
            // also when manually editting the vertex distances
            mline.Update();

            // we can manually create cuts or gaps in the individual elements that made the multiline.
            // the cuts are defined as distances from the start point of the element along its direction.
            mline.Vertexes[0].Distances[0].Add(50);
            mline.Vertexes[0].Distances[0].Add(100);
            mline.Vertexes[0].Distances[mline.Style.Elements.Count - 1].Add(50);
            mline.Vertexes[0].Distances[mline.Style.Elements.Count - 1].Add(100);
            dxf.Entities.Add(mline);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            dxf.Save("MLine.dxf");

            DxfDocument dxf2 = DxfDocument.Load("MLine.dxf");

            // "MyStyle" is used only once
            List<DxfObject> uses;
            uses = dxf.MlineStyles.GetReferences(mline.Style.Name);

            // if we try to get the LinetypeUses, we will find out that "MyStyle" appears several times,
            // this is due to that each MLineStyleElement of a MLineStyle has an associated Linetype
            uses = dxf.Linetypes.GetReferences(Linetype.ByLayerName);

            bool ok;
            ok = dxf.Entities.Remove(mline);

            // "MyStyle" is not used its reference has been deleted
            uses = dxf.MlineStyles.GetReferences(mline.Style.Name);
            // we can safely remove it
            dxf.MlineStyles.Remove(mline.Style.Name);

            dxf.Save("MLine2.dxf");

            dxf.Layers.Clear();

            dxf.Save("MLine2.dxf");
        }

        private static void AppRegUsesAndRemove()
        {
            DxfDocument dxf = new DxfDocument();

            List<Vector3> vertexes = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 10),
                new Vector3(10, 10, 20),
                new Vector3(0, 10, 30)
            };

            Polyline3D poly = new Polyline3D(vertexes, true);

            XData xdata1 = new XData(new ApplicationRegistry("netDxf"));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));

            poly.XData.Add(xdata1);

            dxf.Entities.Add(poly);

            Line line = new Line(new Vector2(10, 5), new Vector2(-10, -5));

            ApplicationRegistry myAppReg = new ApplicationRegistry("MyAppReg");
            XData xdata2 = new XData(myAppReg);
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Distance, Vector3.Distance(line.StartPoint, line.EndPoint)));
            line.XData.Add(xdata2);

            dxf.Entities.Add(line);

            Circle circle = new Circle(Vector3.Zero, 15);
            XData xdata3 = new XData(myAppReg);
            xdata3.XDataRecord.Add(new XDataRecord(XDataCode.Real, circle.Radius));
            circle.XData.Add(xdata3);

            dxf.Entities.Add(circle);

            dxf.Save("appreg.dxf");

            DxfDocument dxf2 = DxfDocument.Load("appreg.dxf");

            // will return false the "MyAppReg" is in use
            bool ok;
            ok = dxf.ApplicationRegistries.Remove(myAppReg.Name);
            dxf.Entities.Remove(line);
            dxf.Entities.Remove(circle);
            // "MyAppReg" is not used anymore
            List<DxfObject> uses = dxf.ApplicationRegistries.GetReferences(myAppReg.Name);
            // it is safe to delete it
            ok = dxf.ApplicationRegistries.Remove(myAppReg.Name);

            // we can even make a full cleanup
            dxf.ApplicationRegistries.Clear();

            dxf.Save("appreg2.dxf");
        }

        private static void ExplodePolyfaceMesh()
        {
            DxfDocument dxf = DxfDocument.Load("polyface mesh.dxf");
            DxfDocument dxfOut = new DxfDocument(dxf.DrawingVariables);
            foreach (PolyfaceMesh polyfaceMesh in dxf.Entities.PolyfaceMeshes)
            {
                List<EntityObject> entities = polyfaceMesh.Explode();
                dxfOut.Entities.Add(entities);
            }

            dxfOut.Save("polyface mesh exploded.dxf");
        }

        private static void ApplicationRegistries()
        {
            DxfDocument dxf = new DxfDocument();
            // add a new application registry to the document (optional), if not present it will be added when the entity is passed to the document
            ApplicationRegistry newAppReg = dxf.ApplicationRegistries.Add(new ApplicationRegistry("NewAppReg"));

            Line line = new Line(Vector2.Zero, 100*Vector2.UnitX);
            XData xdata = new XData(newAppReg);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "string of the new application registry"));
            line.XData.Add(xdata);

            dxf.Entities.Add(line);
            dxf.Save("ApplicationRegistryTest.dxf");

            // gets the complete application registries present in the document
            ICollection<ApplicationRegistry> appRegs = dxf.ApplicationRegistries.Items;

            // get an application registry by name
            //ApplicationRegistry netDxfAppReg = dxf.ApplicationRegistries[appRegs[dxf.ApplicationRegistries.Count - 1].Name];
        }

        private static void TestOCStoWCS()
        {
            // vertexes of the light weight polyline, they are defined in OCS (Object Coordinate System)
            Polyline2DVertex v1 = new Polyline2DVertex(1, -5);
            Polyline2DVertex v2 = new Polyline2DVertex(-3, 2);
            Polyline2DVertex v3 = new Polyline2DVertex(8, 15);

            Polyline2D lwp = new Polyline2D(new List<Polyline2DVertex> {v1, v2, v3}, false);
            // the normal will define the plane where the polyline is defined
            lwp.Normal = new Vector3(1, 1, 0);
            // the entity elevation defines the z vector of the vertexes along the entity normal
            lwp.Elevation = 2.5;

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(lwp);
            dxf.Save("OCStoWCS.dxf");

            // if you want to convert the vertexes of the polyline to WCS (World Coordinate System), you can
            Vector3 v1OCS = new Vector3(v1.Position.X, v1.Position.Y, lwp.Elevation);
            Vector3 v2OCS = new Vector3(v2.Position.X, v2.Position.Y, lwp.Elevation);
            Vector3 v3OCS = new Vector3(v3.Position.X, v3.Position.Y, lwp.Elevation);
            List<Vector3> vertexesWCS = MathHelper.Transform(new List<Vector3> {v1OCS, v2OCS, v3OCS}, lwp.Normal, CoordinateSystem.Object, CoordinateSystem.World);
        }

        private static void WriteGradientPattern()
        {
            List<Polyline2DVertex> vertexes = new List<Polyline2DVertex>
            {
                new Polyline2DVertex(new Vector2(0, 0)),
                new Polyline2DVertex(new Vector2(0, 150)),
                new Polyline2DVertex(new Vector2(150, 150)),
                new Polyline2DVertex(new Vector2(150, 0))
            };
            Polyline2D pol = new Polyline2D(vertexes, true);


            Line line1 = new Line(new Vector2(0, 0), new Vector2(0, 150));
            Line line2 = new Line(new Vector2(0, 150), new Vector2(150, 150));
            Line line3 = new Line(new Vector2(150, 150), new Vector2(150, 0));
            Line line4 = new Line(new Vector2(150, 0), new Vector2(0, 0));

            AciColor color = new AciColor(63, 79, 127);
            HatchGradientPattern gradient = new HatchGradientPattern(color, AciColor.Blue, HatchGradientPatternType.Linear);
            //HatchGradientPattern gradient = new HatchGradientPattern(AciColor.Red, 0.75, HatchGradientPatternType.Linear);
            gradient.Angle = 30;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {pol})
            };
            Hatch hatch = new Hatch(gradient, boundary, true);

            // gradients are only supported for AutoCad2004 and later
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2004);
            dxf.Entities.Add(hatch);
            dxf.Save("gradient test.dxf");

            //DxfDocument dxf2 = DxfDocument.Load("gradient test.dxf");

            //dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;

            //dxf.Save("gradient test 2000.dxf");
        }

        private static void WriteGroup()
        {
            Line line1 = new Line(new Vector2(0, 0), new Vector2(100, 100));
            Line line2 = new Line(new Vector2(100, 0), new Vector2(200, 100));
            Line line3 = new Line(new Vector2(200, 0), new Vector2(300, 100));

            // named group
            Group group1 = new Group("MyGroup", new EntityObject[] {line1, line2});

            //unnamed group
            Group group2 = new Group(new EntityObject[] {line1, line3});

            DxfDocument dxf = new DxfDocument();
            // the AddGroup method will also add the entities contained in a group to the document.
            dxf.Groups.Add(group1);
            dxf.Groups.Add(group2);

            List<DxfObject> list = dxf.Groups.GetReferences(group1);
            dxf.Save("group.dxf");

            dxf = DxfDocument.Load("group.dxf");

            group1 = dxf.Groups[group1.Name];
            group2 = dxf.Groups[group2.Name];
            dxf.Groups.Remove(group1);
            dxf.Groups.Remove(group2);
            dxf.Save("group copy.dxf");
        }

        private static void WriteMLine()
        {
            DxfDocument dxf = new DxfDocument();
            //MLineStyle style = MLineStyle.Default;
            //dxf.AddMLineStyle(style);

            List<Vector2> vertexes = new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 150),
                new Vector2(150, 150),
                new Vector2(150, 0)
            };

            MLine mline = new MLine(vertexes);
            mline.Scale = 20;
            mline.Justification = MLineJustification.Zero;
            //mline.IsClosed = true;

            MLineStyle style = new MLineStyle("MyStyle", "Personalized style.");
            style.Elements.Add(new MLineStyleElement(0.25));
            style.Elements.Add(new MLineStyleElement(-0.25));
            // if we add new elements directly to the list we need to sort the list,
            style.Elements.Sort();
            style.Flags = MLineStyleFlags.EndInnerArcsCap | MLineStyleFlags.EndRoundCap | MLineStyleFlags.StartInnerArcsCap | MLineStyleFlags.StartRoundCap;
            //style.StartAngle = 25.0;
            //style.EndAngle = 160.0;
            // AutoCad2000 dxf version does not support true colors for MLineStyle elements
            style.Elements[0].Color = new AciColor(180, 230, 147);
            mline.Style = style;
            // we have modified the multiline after setting its vertexes so we need to manually call this method.
            // also when manually editing the vertex distances
            mline.Update();

            // we can manually create cuts or gaps in the individual elements that made the multiline.
            // the cuts are defined as distances from the start point of the element along its direction.
            mline.Vertexes[0].Distances[0].Add(50);
            mline.Vertexes[0].Distances[0].Add(100);
            mline.Vertexes[0].Distances[mline.Style.Elements.Count - 1].Add(50);
            mline.Vertexes[0].Distances[mline.Style.Elements.Count - 1].Add(100);

            dxf.Entities.Add(mline);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            dxf.Save("MLine.dxf");

            //dxf = DxfDocument.Load("Drawing1.dxf");
            //dxf.Save("Drawing1 copy.dxf");

            //dxf = DxfDocument.Load("Drawing3.dxf");
            //dxf.Save("Drawing3 copy.dxf");

            //dxf = DxfDocument.Load("Drawing2.dxf");
            //dxf.Save("Drawing2 copy.dxf");

            // empty mline
            //List<Vector2> vertexes2 = new List<Vector2>
            //                             {
            //                                 new Vector2(0, 0),
            //                                 new Vector2(100, 100),
            //                                 new Vector2(100, 100),
            //                                 new Vector2(200, 0)
            //                             };

            //MLine mline2 = new MLine(vertexes2){Scale = 20};
            //mline2.CalculateVertexesInfo();

            //DxfDocument dxf2 = new DxfDocument();
            //dxf2.Entities.Add(mline2);
            ////dxf2.Save("void mline.dxf");

            //MLine mline3 = new MLine();
            //dxf2.Entities.Add(mline3);
            ////dxf2.Save("void mline.dxf");

            //Polyline3D pol = new Polyline3D();
            //Polyline2D lwPol = new Polyline2D();
            //dxf2.Entities.Add(pol);
            //dxf2.Entities.Add(lwPol);
            //dxf2.Save("void mline.dxf");
            //dxf2 = DxfDocument.Load("void mline.dxf");
        }

        private static void ObjectVisibility()
        {
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0))
            {
                Color = AciColor.Yellow
            };

            Line line2 = new Line(new Vector3(0, 100, 0), new Vector3(100, 0, 0))
            {
                IsVisible = false
            };

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line);
            dxf.Entities.Add(line2);
            dxf.Save("object visibility.dxf");
            dxf = DxfDocument.Load("object visibility.dxf");
            dxf.Save("object visibility 2.dxf");
        }

        private static void WriteInsert()
        {
            // nested blocks
            DxfDocument dxf = new DxfDocument();

            Block nestedBlock = new Block("Nested block");
            nestedBlock.Entities.Add(new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0)));
            nestedBlock.Entities.Add(new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0)));

            Insert nestedInsert = new Insert(nestedBlock, new Vector3(0, 0, 0)); // the position will be relative to the position of the insert that nest it

            Circle circle = new Circle(Vector3.Zero, 5);
            circle.Layer = new Layer("circle");
            circle.Layer.Color.Index = 2;
            Block block = new Block("MyBlock");
            block.Entities.Add(circle);
            block.Entities.Add(nestedInsert);

            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");

            dxf.Entities.Add(insert);

            dxf.Save("insert.dxf");
            dxf = DxfDocument.Load("insert.dxf");
        }

        private static void EntityTrueColor()
        {
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));
            line.Color = new AciColor(152, 103, 136);
            // by default a color initialized with rgb components will be exported as true color
            // you can override this behavior with
            // line.Color.UseTrueColor = false;

            Layer layer = new Layer("MyLayer");
            layer.Color = new AciColor(157, 238, 17);
            Line line2 = new Line(new Vector3(0, 100, 0), new Vector3(100, 0, 0));
            line2.Layer = layer;
            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line);
            dxf.Entities.Add(line2);
            dxf.Save("line true color.dxf");
            dxf = DxfDocument.Load("line true color.dxf");
        }

        private static void EntityLineWeight()
        {
            // the lineweight is always defined as 1/100 mm, this property is the equivalent of stroke width, outline width in other programs. Do not confuse with line.Thickness
            // it follow the AutoCAD naming style, check the documentation in case of doubt
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));
            line.Lineweight = Lineweight.W100; // 1.0 mm
            Text text = new Text("Text with lineweight", Vector3.Zero, 10);
            text.Lineweight = Lineweight.W50; // 0.5 mm

            Layer layer = new Layer("MyLayer");
            layer.Lineweight = Lineweight.W200; // 2 mm all entities in the layer with Color.ByLayer will inherit this value
            layer.Color = AciColor.Green;
            Line line2 = new Line(new Vector3(0, 100, 0), new Vector3(100, 0, 0));
            line2.Layer = layer;

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(line);
            dxf.Entities.Add(line2);
            dxf.Entities.Add(text);
            dxf.Save("line weight.dxf");
            dxf = DxfDocument.Load("line weight.dxf");
        }

        private static void Text()
        {
            // use a font that has support for Chinese characters
            TextStyle textStyle = new TextStyle("Chinese text", "simsun.ttf");

            // for dxf database version 2007 and later you can use directly the characters,
            DxfDocument dxf1 = new DxfDocument(DxfVersion.AutoCad2010);
            Text text1 = new Text("这是中国文字", Vector2.Zero, 10, textStyle);
            MText mtext1 = new MText("这是中国文字", new Vector2(0, 30), 10, 0, textStyle);
            Text text2 = new Text("àèìòùáéíóúü", new Vector2(0, 60), 10);
            dxf1.Entities.Add(text1);
            dxf1.Entities.Add(mtext1);
            dxf1.Entities.Add(text2);
            dxf1.Save("textCad2010.dxf");
            dxf1.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            dxf1.Save("textCad2000.dxf");

            foreach (Text text in dxf1.Entities.Texts)
            {
                Console.WriteLine(text.Value);
            }
            foreach (MText text in dxf1.Entities.MTexts)
            {
                Console.WriteLine(text.Value);
            }

            Console.WriteLine("Press a key to continue...");
            Console.ReadLine();

            DxfDocument loadDxf = DxfDocument.Load("textCad2010.dxf");

            // for previous version (this method will also work for later ones) you will need to supply the unicode value (U+value),
            // you can get this value with the Windows Character Map application
            DxfDocument dxf2 = new DxfDocument(DxfVersion.AutoCad2010);
            Text text3 = new Text("\\U+8FD9\\U+662F\\U+4E2D\\U+56FD\\U+6587\\U+5B57", Vector2.Zero, 10, textStyle);
            MText mtext3 = new MText("\\U+8FD9\\U+662F\\U+4E2D\\U+56FD\\U+6587\\U+5B57", new Vector2(0, 30), 10, 0, textStyle);
            dxf2.Entities.Add(text3);
            dxf2.Entities.Add(mtext3);
            //dxf2.Save("textCad2000.dxf");
        }

        private static void WriteNoAsciiText()
        {
            TextStyle textStyle = new TextStyle("Arial.ttf");
            DxfDocument dxf = new DxfDocument();
            dxf.DrawingVariables.LastSavedBy = "ЉЊЋЌЍжзицрлЯ";
            //Text text = new Text("ÁÉÍÓÚ áéíóú Ññ àèìòù âêîôû", Vector2.Zero,10);
            Text text = new Text("ЉЊЋЌЍжзицрлЯ", Vector2.Zero, 10, textStyle);
            MText mtext = new MText("ЉЊЋЌЍжзицрлЯ", new Vector2(0, 50), 10, 0, textStyle);

            dxf.Entities.Add(text);
            dxf.Entities.Add(mtext);
            foreach (Text t in dxf.Entities.Texts)
            {
                Console.WriteLine(t.Value);
            }
            foreach (MText t in dxf.Entities.MTexts)
            {
                Console.WriteLine(t.Value);
            }
            Console.WriteLine("Press a key to continue...");
            Console.ReadLine();
            dxf.Save("text1.dxf");

            dxf = DxfDocument.Load("text1.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            dxf.Save("text2.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2007;
            dxf.Save("text3.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("text4.dxf");
        }

        private static void WriteSplineBoundaryHatch()
        {
            Vector3[] ctrlPoints =
            {
                new Vector3(0, 0, 0),
                new Vector3(25, 50, 0),
                new Vector3(50, 0, 0),
                new Vector3(75, 50, 0),
                new Vector3(100, 0, 0)
            };

            // hatch with single closed spline boundary path
            Spline spline = new Spline(ctrlPoints, null, 3, true); // closed periodic

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>();

            HatchBoundaryPath path = new HatchBoundaryPath(new List<EntityObject> {spline});
            boundary.Add(path);
            Hatch hatch = new Hatch(HatchPattern.Line, boundary, true);
            hatch.Pattern.Angle = 45;
            hatch.Pattern.Scale = 10;

            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(hatch);
            dxf.Entities.Add(spline);
            dxf.Save("hatch closed spline.dxf");
            dxf = DxfDocument.Load("hatch closed spline.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("hatch closed spline 2010.dxf");

            // hatch boundary path with spline and line
            Spline openSpline = new Spline(ctrlPoints, null, 3);
            Line line = new Line(ctrlPoints[0], ctrlPoints[ctrlPoints.Length - 1]);

            List<HatchBoundaryPath> boundary2 = new List<HatchBoundaryPath>();
            HatchBoundaryPath path2 = new HatchBoundaryPath(new List<EntityObject> {openSpline, line});
            boundary2.Add(path2);
            Hatch hatch2 = new Hatch(HatchPattern.Line, boundary2, true);
            hatch2.Pattern.Angle = 45;
            hatch2.Pattern.Scale = 10;

            DxfDocument dxf2 = new DxfDocument();
            dxf2.Entities.Add(hatch2);
            dxf2.Entities.Add(openSpline);
            dxf2.Entities.Add(line);
            dxf2.Save("hatch open spline.dxf");
            dxf2 = DxfDocument.Load("hatch open spline.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf2.Save("hatch open spline 2010.dxf");
        }

        private static void WriteNoInsertBlock()
        {
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(100, 100, 0));
            Line line2 = new Line(new Vector3(0, 100, 0), new Vector3(100, 0, 0));
            // create the block definition
            Block block = new Block("MyBlock");
            // add the entities that you want in the block
            block.Entities.Add(line);
            block.Entities.Add(line2);


            // create the document
            DxfDocument dxf = new DxfDocument();
            // add the block definition to the block table list (this is the function that was private in earlier versions, check the changelog.txt)
            dxf.Blocks.Add(block);

            // and save file, no visible entities will appear if you try to open the drawing but the block will be there
            dxf.Save("Block definiton.dxf");
        }

        private static void WriteImage()
        {
            string imgFile1 = @"img\image01.jpg";
            System.Drawing.Image img1 = System.Drawing.Image.FromFile(imgFile1);
            ImageDefinition imageDef1 = new ImageDefinition("MyImage", imgFile1, img1.Width, img1.HorizontalResolution, img1.Height, img1.VerticalResolution, ImageResolutionUnits.Inches);
            Image image = new Image(imageDef1, Vector3.Zero, 10, 10);

            XData xdata1 = new XData(new ApplicationRegistry("netDxf"));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.String, "xData image position"));
            xdata1.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionX, image.Position.X));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionY, image.Position.Y));
            xdata1.XDataRecord.Add(new XDataRecord(XDataCode.WorldSpacePositionZ, image.Position.Z));
            xdata1.XDataRecord.Add(XDataRecord.CloseControlString);
            image.XData.Add(xdata1);

            //image.Normal = new Vector3(1, 1, 1);
            //image.Rotation = 30;
            
            // you can pass a name that must be unique for the image definiton, by default it will use the file name without the extension
            string imgFile2 = @"img\image02.jpg";
            System.Drawing.Image img2 = System.Drawing.Image.FromFile(imgFile2);
            ImageDefinition imageDef2 = new ImageDefinition("MyImage", imgFile2, img2.Width, img2.HorizontalResolution, img2.Height, img2.VerticalResolution, ImageResolutionUnits.Inches);

            Image image2 = new Image(imageDef2, new Vector3(0, 150, 0), 10, 10);
            Image image3 = new Image(imageDef2, new Vector3(150, 150, 0), 10, 10);

            // clipping boundary definition in local coordinates
            ClippingBoundary clip = new ClippingBoundary(100, 100, 500, 300);
            image.ClippingBoundary = clip;
            // set to null to restore the default clipping boundary (full image)
            image.ClippingBoundary = null;

            // images can be part of a block definition
            Block block = new Block("ImageBlock");
            block.Entities.Add(image2);
            block.Entities.Add(image3);
            Insert insert = new Insert(block, new Vector3(0, 100, 0));

            DxfDocument dxf = new DxfDocument();

            dxf.Entities.Add(image);
            //dxf.Entities.Add(image2);
            //dxf.Entities.Add(image3);
            dxf.Entities.Add(insert);

            dxf.Save("image.dxf");
            dxf = DxfDocument.Load("image.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("test.dxf");

            //dxf.Entities.Remove(image2);
            //dxf.Save("image2.dxf");
            //dxf.Entities.Remove(image3);
            //dxf.Entities.Remove(image);
            //dxf.Save("image3.dxf");
        }

        private static void AddAndRemove()
        {
            Layer layer1 = new Layer("layer1") {Color = AciColor.Blue};
            Layer layer2 = new Layer("layer2") {Color = AciColor.Green};

            Line line = new Line(new Vector2(0, 0), new Vector2(10, 10));
            line.Layer = layer1;
            Circle circle = new Circle(new Vector2(0, 0), 10);
            circle.Layer = layer2;

            double offset = -0.9;
            Vector2 p1 = new Vector2(1, 2);
            Vector2 p2 = new Vector2(2, 6);

            DimensionStyle myStyle = new DimensionStyle("MyDimStyle");
            myStyle.DimSuffix = "mm";
            AlignedDimension dim1 = new AlignedDimension(p1, p2, offset, myStyle);

            //text
            TextStyle style = new TextStyle("MyTextStyle", "Arial.ttf");
            Text text = new Text("Hello world!", Vector3.Zero, 10.0f, style)
            {
                Layer = new Layer("text")
                {
                    Color = {Index = 8}
                }
            };
            text.Alignment = TextAlignment.TopRight;

            HeaderVariables variables = new HeaderVariables
            {
                AcadVer = DxfVersion.AutoCad2004
            };
            DxfDocument dxf = new DxfDocument();
            dxf.Entities.Add(new EntityObject[] {line, circle, dim1, text});
            dxf.Save("before remove.dxf");

            dxf.Entities.Remove(circle);
            dxf.Save("after remove.dxf");

            dxf.Entities.Add(circle);
            dxf.Save("after remove and add.dxf");

            dxf.Entities.Remove(dim1);
            dxf.Save("remove dim.dxf");

            dxf.Entities.Add(dim1);
            dxf.Save("add dim.dxf");

            DxfDocument dxf2 = DxfDocument.Load("dim block names.dxf");
            dxf2.Entities.Add(dim1);
            dxf2.Save("dim block names2.dxf");
        }

        private static void LoadAndSave()
        {
            DxfDocument dxf = DxfDocument.Load("block sample.dxf");
            dxf.Save("block sample1.dxf");

            DxfDocument dxf2 = new DxfDocument();
            dxf2.Entities.Add(dxf.Entities.Inserts.ElementAt(0));
            dxf2.Save("block sample2.dxf");

            dxf.Save("clean2.dxf");
            dxf = DxfDocument.Load("clean.dxf");
            dxf.Save("clean1.dxf");

            // open a dxf saved with autocad
            dxf = DxfDocument.Load("sample.dxf");
            dxf.Save("sample4.dxf");

            Line cadLine = dxf.Entities.Lines.ElementAt(0);
            Layer layer = new Layer("netLayer");
            layer.Color = AciColor.Yellow;

            Line line = new Line(new Vector2(20, 40), new Vector2(100, 200));
            line.Layer = layer;
            // add a new entity to the document
            dxf.Entities.Add(line);

            dxf.Save("sample2.dxf");

            DxfDocument dxf3 = new DxfDocument();
            dxf3.Entities.Add(cadLine);
            dxf3.Entities.Add(line);
            dxf3.Save("sample3.dxf");
        }

        private static void CleanDrawing()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.Save("clean drawing.dxf");
        }

        private static void OrdinateDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();

            Vector2 origin = new Vector2(2, 1);
            Vector2 refX = new Vector2(1, 0);
            Vector2 refY = new Vector2(0, 2);
            double length = 3;
            double angle = 30;
            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;

            OrdinateDimension dimX1 = new OrdinateDimension(origin, refX, length, OrdinateDimensionAxis.X, 0, myStyle);
            OrdinateDimension dimX2 = new OrdinateDimension(origin, refX, length, OrdinateDimensionAxis.X, angle, myStyle);
            OrdinateDimension dimY1 = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, 0, myStyle);
            OrdinateDimension dimY2 = new OrdinateDimension(origin, refY, length, OrdinateDimensionAxis.Y, angle, myStyle);

            dxf.Entities.Add(dimX1);
            dxf.Entities.Add(dimY1);
            dxf.Entities.Add(dimX2);
            dxf.Entities.Add(dimY2);

            Line lineX = new Line(origin, origin + 5*Vector2.UnitX);
            Line lineY = new Line(origin, origin + 5*Vector2.UnitY);

            Vector2 point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad);
            Line lineXRotate = new Line(origin, new Vector2(point.X, point.Y));

            point = Vector2.Polar(new Vector2(origin.X, origin.Y), 5, angle*MathHelper.DegToRad + MathHelper.HalfPI);
            Line lineYRotate = new Line(origin, new Vector2(point.X, point.Y));

            dxf.Entities.Add(lineX);
            dxf.Entities.Add(lineY);
            dxf.Entities.Add(lineXRotate);
            dxf.Entities.Add(lineYRotate);

            dxf.Save("ordinate dimension.dxf");

            dxf = DxfDocument.Load("ordinate dimension.dxf");
        }

        private static void Angular2LineDimensionDrawing()
        {
            double offset = 7.5;

            Line line1 = new Line(new Vector2(1, 2), new Vector2(6, 0));
            Line line2 = new Line(new Vector2(2, 1), new Vector2(4, 5));

            Angular2LineDimension dim = new Angular2LineDimension(line1, line2, offset);

            DxfDocument dxf = new DxfDocument();
            //dxf.Entities.Add(line1);
            //dxf.Entities.Add(line2);
            //dxf.Entities.Add(dim);

            Block block = new Block("DimensionBlock");
            block.Entities.Add(line1);
            block.Entities.Add(line2);
            block.Entities.Add(dim);
            Insert insert = new Insert(block);

            dxf.Entities.Add(insert);

            dxf.Save("angular 2 line dimension.dxf");
            dxf = DxfDocument.Load("angular 2 line dimension.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("angular 2 line dimension.dxf");
        }

        private static void Angular3PointDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 center = new Vector3(1, 2, 0);
            double radius = 2.42548;
            Arc arc = new Arc(center, radius, -30, 60);
            //arc.Normal = new Vector3(1, 1, 1);
            DimensionStyle myStyle = new DimensionStyle("MyStyle");

            Angular3PointDimension dim = new Angular3PointDimension(arc, 5, myStyle);
            dxf.Entities.Add(arc);
            dxf.Entities.Add(dim);
            dxf.Save("angular 3 point dimension.dxf");

            dxf = DxfDocument.Load("angular 3 point dimension.dxf");
        }

        private static void DiametricDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 center = new Vector3(1, 2, 0);
            double radius = 2.42548;
            Circle circle = new Circle(center, radius);
            //circle.Normal = new Vector3(1, 1, 1);
            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;
            myStyle.DecimalSeparator = ',';

            DiametricDimension dim = new DiametricDimension(circle, 30.0, myStyle);
            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim);
            dxf.Save("diametric dimension.dxf");

            dxf.Entities.Remove(dim);
            dxf.Save("diametric dimension removed.dxf");

            dxf = DxfDocument.Load("diametric dimension.dxf");
            // remove entity with a handle
            Dimension dimLoaded = (Dimension)dxf.GetObjectByHandle(dim.Handle);
            dxf.Entities.Remove(dimLoaded);
            dxf.Save("diametric dimension removed 2.dxf");
        }

        private static void RadialDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 center = new Vector3(1, 2, 0);
            double radius = 2.42548;
            Circle circle = new Circle(center, radius);
            circle.Normal = new Vector3(1, 1, 1);
            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;
            myStyle.DecimalSeparator = ',';

            RadialDimension dim = new RadialDimension(circle, 30.0, myStyle);
            dxf.Entities.Add(circle);
            dxf.Entities.Add(dim);
            dxf.Save("radial dimension.dxf");

            dxf = DxfDocument.Load("radial dimension.dxf");
        }

        private static void LinearDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 p1 = new Vector3(0, 0, 0);
            Vector3 p2 = new Vector3(5, 5, 0);
            Line line = new Line(p1, p2);

            dxf.Entities.Add(line);

            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            myStyle.LengthPrecision = 2;
            double offset = 7;
            LinearDimension dimX = new LinearDimension(line, offset, 0.0, Vector3.UnitZ, myStyle);
            dimX.Rotation += 30.0;
            LinearDimension dimY = new LinearDimension(line, offset, 90.0, Vector3.UnitZ, myStyle);
            dimY.Rotation += 30.0;

            XData xdata = new XData(new ApplicationRegistry("other application"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "Linear Dimension"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 350));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);
            dimX.XData.Add(xdata);
            dimY.XData.Add(xdata);
            dxf.Entities.Add(dimX);
            dxf.Entities.Add(dimY);
            dxf.Save("linear dimension.dxf");
            // dxf = DxfDocument.Load("linear dimension.dxf");
        }

        private static void AlignedDimensionDrawing()
        {
            DxfDocument dxf = new DxfDocument();
            double offset = -0.9;
            Vector2 p1 = new Vector2(1, 2);
            Vector2 p2 = new Vector2(2, 6);
            Line line1 = new Line(p1, p2);

            DimensionStyle myStyle = new DimensionStyle("MyStyle");
            myStyle.DimSuffix = "mm";
            AlignedDimension dim1 = new AlignedDimension(p1, p2, offset, myStyle);

            Vector2 p3 = p1 + new Vector2(4, 0);
            Vector2 p4 = p2 + new Vector2(4, 0);
            Line line2 = new Line(p3, p4);
            AlignedDimension dim2 = new AlignedDimension(p3, p4, -offset, myStyle);


            Vector2 perp = Vector2.Perpendicular(new Vector2(p2.X, p2.Y) - new Vector2(p1.X, p1.Y));
            //dim.Normal = -new Vector3(perp.X, perp.Y, 0.0) ;

            XData xdata = new XData(new ApplicationRegistry("other application"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "Aligned Dimension"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, 15.5));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 350));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);
            dim1.XData.Add(xdata);

            //dxf.Entities.Add(line1);
            //dxf.Entities.Add(line2);
            //dxf.Entities.Add(dim1);
            //dxf.Entities.Add(dim2);


            Block block = new Block("DimensionBlock");
            block.Entities.Add(line1);
            block.Entities.Add(line2);
            block.Entities.Add(dim1);
            block.Entities.Add(dim2);
            Insert insert = new Insert(block);
            dxf.Entities.Add(insert);

            dxf.Save("aligned dimension.dxf");

            dxf = DxfDocument.Load("aligned dimension.dxf");
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
            MText mText = new MText(new Vector3(3, 2, 0), 1.0f, 100.0f, style);
            mText.Layer = new Layer("Multiline Text");
            //mText.Layer.Color.Index = 8;
            mText.Rotation = 0;
            mText.LineSpacingFactor = 1.0;

            MTextFormattingOptions options = new MTextFormattingOptions();
            options.Bold = true;
            mText.Write("Bold text in mText.Style", options);
            mText.EndParagraph();
            options.Italic = true;
            mText.Write("Bold and italic text in mText.Style", options);
            mText.EndParagraph();
            options.Bold = false;
            options.FontName = "Arial";
            options.Color = AciColor.Blue;
            mText.Write("Italic text in Arial", options);
            mText.EndParagraph();
            options.Italic = false;
            options.Color = null; // back to the default text color
            mText.Write("Normal text in Arial with the default paragraph height factor", options);
            mText.EndParagraph();
            mText.Write("No formatted text uses mText.Style");
            mText.Write(" and the text continues in the same paragraph.");
            mText.EndParagraph();

            mText.XData.Add(xdata);

            dxf.Entities.Add(mText);

            dxf.Save("MText sample.dxf");
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
                new HatchBoundaryPath(new List<EntityObject> {circle})
            };

            // create the hatch in this case we will use the predefined Solid hatch pattern and our circle as the boundary path
            //Hatch hatch = new Hatch(HatchPattern.Solid, boundary);
            Hatch hatch = new Hatch(HatchPattern.Line, boundary, true);
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
            dxf.Entities.Add(circle);

            // add the hatch to the document
            dxf.Entities.Add(hatch);

            dxf.Save("circle solid fill.dxf");
        }

        private static void LineWidth()
        {
            // the line thickness works as expected, according to the AutoCAD way of doing things
            Line thickLine = new Line(new Vector3(0, 10, 0), new Vector3(10, 20, 0));

            // when you assign a thickness to a line, the result is like a wall, it is like a 3d face whose vertexes are defined by the
            // start and end points of the line and the thickness along the normal of the line.
            thickLine.Thickness = 5;

            // maybe what you are trying to do is create a line with a width (something that we can read it as a line with thickness), the only way to do this is to create a polyline
            // the kind of result you will get if you give a width to a 2d polyline 
            // you can only give a width to a vertex of a Polyline2D
            Polyline2D widthLine = new Polyline2D();
            Polyline2DVertex startVertex = new Polyline2DVertex(new Vector2(0, 0));
            Polyline2DVertex endVertex = new Polyline2DVertex(new Vector2(10, 10));
            widthLine.Vertexes.AddRange(new[] {startVertex, endVertex});

            // the easy way to give a constant width to a polyline, but you can also give a polyline width by vertex
            // SetConstantWidth is a sort cut that will assign the given value to every start width and end width of every vertex of the polyline
            widthLine.SetConstantWidth(0.5);

            DxfDocument dxf = new DxfDocument();

            // add the entities to the document (both of them to see the difference)
            dxf.Entities.Add(thickLine);
            dxf.Entities.Add(widthLine);

            dxf.Save("line width.dxf");
        }

        private static void ToPolyline2D()
        {
            DxfDocument dxf = new DxfDocument();

            Vector3 center = new Vector3(1, 8, -7);
            Vector3 normal = new Vector3(1, 1, 1);

            Circle circle = new Circle(center, 7.5);
            circle.Normal = normal;

            Arc arc = new Arc(center, 5, -45, 45);
            arc.Normal = normal;

            Ellipse ellipse = new Ellipse(center, 15, 7.5);
            ellipse.Rotation = 35;
            ellipse.Normal = normal;

            Ellipse ellipseArc = new Ellipse(center, 10, 5);
            ellipseArc.StartAngle = 315;
            ellipseArc.EndAngle = 45;
            ellipseArc.Rotation = 35;
            ellipseArc.Normal = normal;

            dxf.Entities.Add(circle);
            dxf.Entities.Add(circle.ToPolyline2D(10));

            dxf.Entities.Add(arc);
            dxf.Entities.Add(arc.ToPolyline2D(10));

            dxf.Entities.Add(ellipse);
            dxf.Entities.Add(ellipse.ToPolyline2D(10));

            dxf.Entities.Add(ellipseArc);
            dxf.Entities.Add(ellipseArc.ToPolyline2D(10));

            dxf.Save("to polyline.dxf");

            dxf = DxfDocument.Load("to polyline.dxf");

            dxf.Save("to polyline2.dxf");
        }

        private static void CustomHatchPattern()
        {
            DxfDocument dxf = new DxfDocument();

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            Polyline2D poly2 = new Polyline2D();
            poly2.Vertexes.Add(new Polyline2DVertex(-5, -5));
            poly2.Vertexes.Add(new Polyline2DVertex(5, -5));
            poly2.Vertexes.Add(new Polyline2DVertex(5, 5));
            poly2.Vertexes.Add(new Polyline2DVertex(-5, 5));
            poly2.Vertexes[1].Bulge = -0.25;
            poly2.IsClosed = true;

            Polyline2D poly3 = new Polyline2D();
            poly3.Vertexes.Add(new Polyline2DVertex(-8, -8));
            poly3.Vertexes.Add(new Polyline2DVertex(-6, -8));
            poly3.Vertexes.Add(new Polyline2DVertex(-6, -6));
            poly3.Vertexes.Add(new Polyline2DVertex(-8, -6));
            poly3.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {poly}),
                new HatchBoundaryPath(new List<EntityObject> {poly2}),
                new HatchBoundaryPath(new List<EntityObject> {poly3}),
            };

            HatchPattern pattern = new HatchPattern("MyPattern", "A custom hatch pattern");
            //HatchPattern pattern = new HatchPattern(null);

            HatchPatternLineDefinition line1 = new HatchPatternLineDefinition();
            line1.Angle = 45;
            line1.Origin = Vector2.Zero;
            line1.Delta = new Vector2(4, 4);
            line1.DashPattern.Add(12);
            line1.DashPattern.Add(-4);
            pattern.LineDefinitions.Add(line1);

            HatchPatternLineDefinition line2 = new HatchPatternLineDefinition();
            line2.Angle = 135;
            line2.Origin = new Vector2(2.828427125, 2.828427125);
            line2.Delta = new Vector2(4, -4);
            line2.DashPattern.Add(12);
            line2.DashPattern.Add(-4);
            pattern.LineDefinitions.Add(line2);

            Hatch hatch = new Hatch(pattern, boundary, true);
            hatch.Layer = new Layer("hatch")
            {
                Color = AciColor.Red,
                Linetype = Linetype.Continuous
            };
            hatch.Pattern.Angle = 0;
            hatch.Pattern.Scale = 1;
            dxf.Entities.Add(poly);
            dxf.Entities.Add(poly2);
            dxf.Entities.Add(poly3);
            dxf.Entities.Add(hatch);

            dxf.Save("hatchTest.dxf");
        }

        private static void FilesTest()
        {
            Linetype linetype = Linetype.Load("acad.lin", "ACAD_ISO15W100");
            HatchPattern hatch = HatchPattern.Load("acad.pat", "zigzag");
        }

        private static void LoadSaveHatchTest()
        {
            DxfDocument dxf = DxfDocument.Load("Hatch2.dxf");
            dxf.Save("HatchTest.dxf");
        }

        private static void ExplodeTest()
        {
            DxfDocument dxf = new DxfDocument();
            //polyline
            Polyline2DVertex polyVertex;
            List<Polyline2DVertex> polyVertexes = new List<Polyline2DVertex>();
            polyVertex = new Polyline2DVertex(new Vector2(-50, -23.5));
            polyVertex.Bulge = 1.33;
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(34.8, -42.7));
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(65.3, 54.7));
            polyVertex.Bulge = -0.47;
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(-48.2, 42.5));
            polyVertexes.Add(polyVertex);
            Polyline2D polyline2d = new Polyline2D(polyVertexes, false);
            polyline2d.Layer = new Layer("polyline2d");
            polyline2d.Layer.Color.Index = 5;
            polyline2d.Normal = new Vector3(1, 1, 1);
            polyline2d.Elevation = 100.0f;

            dxf.Entities.Add(polyline2d);
            dxf.Entities.Add(polyline2d.Explode());

            dxf.Save("explode.dxf");
        }

        private static void HatchTestLinesBoundary()
        {
            DxfDocument dxf = new DxfDocument();

            Line line1 = new Line(new Vector3(-10, -10, 0), new Vector3(10, -10, 0));
            Line line2 = new Line(new Vector3(10, -10, 0), new Vector3(10, 10, 0));
            Line line3 = new Line(new Vector3(10, 10, 0), new Vector3(-10, 10, 0));
            Line line4 = new Line(new Vector3(-10, 10, 0), new Vector3(-10, -10, 0));


            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {line1, line2, line3, line4})
            };
            Hatch hatch = new Hatch(HatchPattern.Line, boundary, true);
            hatch.Layer = new Layer("hatch")
            {
                Color = AciColor.Red,
                Linetype = Linetype.Dashed
            };
            hatch.Elevation = 52;
            hatch.Pattern.Angle = 45;
            hatch.Pattern.Scale = 10;
            hatch.Normal = new Vector3(1, 1, 1);

            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "netDxf hatch"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Distance, hatch.Pattern.Scale));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Real, hatch.Pattern.Angle));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            hatch.XData.Add(xdata);

            //dxf.Entities.Add(line1);
            //dxf.Entities.Add(line2);
            //dxf.Entities.Add(line3);
            //dxf.Entities.Add(line4);
            dxf.Entities.Add(hatch);
            dxf.Entities.Add(hatch.CreateBoundary(true));

            dxf.Save("hatchTest.dxf");
        }

        private static void HatchTest1()
        {
            DxfDocument dxf = new DxfDocument();

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            //poly.IsClosed = true;

            Polyline2D poly2 = new Polyline2D();
            poly2.Vertexes.Add(new Polyline2DVertex(-5, -5));
            poly2.Vertexes.Add(new Polyline2DVertex(5, -5));
            poly2.Vertexes.Add(new Polyline2DVertex(5, 5));
            poly2.Vertexes.Add(new Polyline2DVertex(-5, 5));
            poly2.Vertexes[1].Bulge = -0.25;
            poly2.IsClosed = true;

            Polyline2D poly3 = new Polyline2D();
            poly3.Vertexes.Add(new Polyline2DVertex(-8, -8));
            poly3.Vertexes.Add(new Polyline2DVertex(-6, -8));
            poly3.Vertexes.Add(new Polyline2DVertex(-6, -6));
            poly3.Vertexes.Add(new Polyline2DVertex(-8, -6));
            poly3.IsClosed = true;

            Line line = new Line(new Vector2(-10, -10), new Vector2(-10, 10));
            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {line, poly}),
                //new HatchBoundaryPath(new List<EntityObject> {poly2}),
                //new HatchBoundaryPath(new List<EntityObject> {poly3}),
            };

            Hatch hatch = new Hatch(HatchPattern.Net, boundary, true);
            //Hatch hatch = new Hatch(HatchPattern.Line, boundary, true);
            hatch.Layer = new Layer("hatch")
            {
                Color = AciColor.Red,
                Linetype = Linetype.Continuous
            };
            hatch.Pattern.Angle = 30;
            //hatch.Elevation = 52;
            //hatch.Normal = new Vector3(1, 1, 0);
            hatch.Pattern.Scale = 1/hatch.Pattern.LineDefinitions[0].Delta.Y;
            dxf.Entities.Add(hatch);
            List<EntityObject> entities = hatch.CreateBoundary(true);

            // if the hatch is associative DO NOT add the entities that make the contourn to the document it will be done automatically
            //dxf.Entities.Add(entities);

            dxf.Save("hatchTest1.dxf");
            dxf = DxfDocument.Load("hatchTest1.dxf");
        }

        private static void HatchTest2()
        {
            DxfDocument dxf = new DxfDocument();

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            Circle circle = new Circle(Vector3.Zero, 5);

            Ellipse ellipse = new Ellipse(Vector3.Zero, 16, 10);
            ellipse.Rotation = 30;
            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {poly}),
                new HatchBoundaryPath(new List<EntityObject> {circle}),
                new HatchBoundaryPath(new List<EntityObject> {ellipse})
            };

            Hatch hatch = new Hatch(HatchPattern.Line, boundary, false);
            hatch.Pattern.Angle = 150;
            hatch.Pattern.Scale = 5;
            //hatch.Normal = new Vector3(1,1,1);
            //hatch.Elevation = 23;
            //dxf.Entities.Add(poly);
            //dxf.Entities.Add(circle);
            //dxf.Entities.Add(ellipse);
            dxf.Entities.Add(hatch);
            hatch.CreateBoundary(true);
            dxf.Save("hatchTest2.dxf");
            dxf = DxfDocument.Load("hatchTest2.dxf");
            dxf.Save("hatchTest2 copy.dxf");
        }

        private static void HatchTest3()
        {
            DxfDocument dxf = new DxfDocument();

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.Vertexes[2].Bulge = 1;
            poly.IsClosed = true;

            Ellipse ellipse = new Ellipse(Vector3.Zero, 16, 10);
            ellipse.Rotation = 0;
            ellipse.StartAngle = 0;
            ellipse.EndAngle = 180;

            Polyline2D poly2 = new Polyline2D();
            poly2.Vertexes.Add(new Polyline2DVertex(-8, 0));
            poly2.Vertexes.Add(new Polyline2DVertex(0, -4));
            poly2.Vertexes.Add(new Polyline2DVertex(8, 0));

            //Arc arc = new Arc(Vector3.Zero,8,180,0);
            //Line line =new Line(new Vector3(8,0,0), new Vector3(-8,0,0));

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>
            {
                new HatchBoundaryPath(new List<EntityObject> {poly}),
                new HatchBoundaryPath(new List<EntityObject> {poly2, ellipse})
            };

            Hatch hatch = new Hatch(HatchPattern.Line, boundary, true);
            hatch.Pattern.Angle = 45;
            //dxf.Entities.Add(poly);
            //dxf.Entities.Add(ellipse);
            ////dxf.Entities.Add(arc);
            ////dxf.Entities.Add(line);
            //dxf.Entities.Add(poly2);
            dxf.Entities.Add(hatch);


            dxf.Save("hatchTest3.dxf");
        }

        private static void HatchTest4()
        {
            DxfDocument dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(-10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, -10));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(-10, 10));
            poly.IsClosed = true;

            List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath> {new HatchBoundaryPath(new List<EntityObject> {poly})};
            HatchGradientPattern pattern = new HatchGradientPattern(AciColor.Yellow, AciColor.Blue, HatchGradientPatternType.Linear);
            pattern.Origin = new Vector2(120, -365);
            Hatch hatch = new Hatch(pattern, boundary, true);
            dxf.Entities.Add(hatch);

            dxf.Save("HatchTest4.dxf");
            dxf = DxfDocument.Load("HatchTest4.dxf");
            dxf.Save("HatchTest4 copy.dxf");
        }

        private static void Dxf2000()
        {
            DxfDocument dxf = new DxfDocument();
            //line
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(5, 5, 5));
            line.Layer = new Layer("line");
            line.Layer.Color.Index = 6;

            dxf.Entities.Add(line);

            dxf.Save("test2000.dxf");
        }

        private static void Polyline2D()
        {
            DxfDocument dxf = new DxfDocument();

            Polyline2D poly = new Polyline2D();
            poly.Vertexes.Add(new Polyline2DVertex(0, 0));
            poly.Vertexes.Add(new Polyline2DVertex(10, 10));
            poly.Vertexes.Add(new Polyline2DVertex(20, 0));
            poly.Vertexes.Add(new Polyline2DVertex(30, 10));
            poly.SetConstantWidth(2);
            //poly.IsClosed = true;
            dxf.Entities.Add(poly);

            dxf.Save("polyline2D.dxf");

            dxf = DxfDocument.Load("polyline2D.dxf");
        }

        private static void Polyline3D()
        {
            DxfDocument dxf = new DxfDocument();
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            Polyline3D poly = new Polyline3D();
            poly.Vertexes.Add(new Vector3(0, 0, 0));
            poly.Vertexes.Add(new Vector3(10, 10, 0));
            poly.Vertexes.Add(new Vector3(20, 0, 0));
            poly.Vertexes.Add(new Vector3(30, 10, 0));
            dxf.Entities.Add(poly);

            dxf.Save("polyline3D.dxf");
        }

        private static void Solid()
        {
            DxfDocument dxf = new DxfDocument();

            Solid solid = new Solid();
            solid.FirstVertex = new Vector2(0, 0);
            solid.SecondVertex = new Vector2(1, 0);
            solid.ThirdVertex = new Vector2(0, 1);
            solid.FourthVertex = new Vector2(1, 1);
            dxf.Entities.Add(solid);

            dxf.Save("solid.dxf");
            //dxf = DxfDocument.Load("solid.dxf");
            //dxf.Save("solid.dxf");
        }

        private static void Face3d()
        {
            DxfDocument dxf = new DxfDocument();

            Face3D face3D = new Face3D();
            face3D.FirstVertex = new Vector3(0, 0, 0);
            face3D.SecondVertex = new Vector3(1, 0, 0);
            face3D.ThirdVertex = new Vector3(1, 1, 0);
            face3D.FourthVertex = new Vector3(0, 1, 0);
            dxf.Entities.Add(face3D);

            dxf.Save("face.dxf");
            dxf = DxfDocument.Load("face.dxf");
            dxf.Save("face return.dxf");
        }

        private static void Ellipse()
        {
            DxfDocument dxf = new DxfDocument();

            //Line line = new Line(new Vector3(0, 0, 0), new Vector3(2 * Math.Cos(Math.PI / 4),2 * Math.Cos(Math.PI / 4), 0));

            //dxf.Entities.Add(line);

            //Line line2 = new Line(new Vector3(0, 0, 0), new Vector3(0, -2, 0));
            //dxf.Entities.Add(line2);

            //Arc arc=new Arc(Vector3.Zero,2,45,270);
            //dxf.Entities.Add(arc);

            Ellipse ellipse = new Ellipse(new Vector3(2, 2, 0), 5, 3);
            ellipse.Rotation = 30;
            ellipse.Normal = new Vector3(1, 1, 1);
            ellipse.Thickness = 2;
            dxf.Entities.Add(ellipse);

            Ellipse ellipseArc = new Ellipse(new Vector3(2, 10, 0), 5, 3);
            ellipseArc.StartAngle = -45;
            ellipseArc.EndAngle = 45;
            dxf.Entities.Add(ellipseArc);

            dxf.Save("ellipse.dxf");
            dxf = new DxfDocument();
            dxf = DxfDocument.Load("ellipse.dxf");

            DxfDocument load = DxfDocument.Load("test ellipse.dxf");
            load.Save("saved test ellipse.dxf");
        }

        private static void SpeedTest()
        {
            Stopwatch crono = new Stopwatch();
            const int numLines = (int) 1e6; // create # lines
            string layerName = "MyLayer";
            float totalTime = 0;

            List<EntityObject> lines = new List<EntityObject>(numLines);
            //List<EntityObject> pols = new List<EntityObject>(numLines);
            DxfDocument dxf = new DxfDocument();

            crono.Start();
            for (int i = 0; i < numLines; i++)
            {
                //List<Vector2> vertexes = new List<Vector2>()
                //{
                //    new Vector2(0,0),
                //    new Vector2(10,0),
                //    new Vector2(10,10),
                //    new Vector2(0,10),
                //    new Vector2(0,20),
                //    new Vector2(10,20),
                //    new Vector2(10,30),
                //    new Vector2(10,40),
                //    new Vector2(0,40),
                //    new Vector2(0,50)
                //};
                //Polyline2D pol = new Polyline2D(vertexes);
                //pol.Layer = new Layer(layerName);
                //pol.Layer.Color.Index = 6;
                //pols.Add(pol);

                //List<Vector3> vertexes = new List<Vector3>()
                //{
                //    new Vector3(0,0,0),
                //    new Vector3(10,0,10),
                //    new Vector3(10,10,20),
                //    new Vector3(0,10,30),
                //    new Vector3(0,20,40),
                //    new Vector3(10,20,50),
                //    new Vector3(10,30,60),
                //    new Vector3(10,40,70),
                //    new Vector3(0,40,80),
                //    new Vector3(0,50,90)
                //};
                //Polyline3D pol = new Polyline3D(vertexes);
                //pol.Layer = new Layer(layerName);
                //pol.Layer.Color.Index = 6;
                //pols.Add(pol);

                //line
                Line line = new Line(new Vector3(0, i, 0), new Vector3(5, i, 0));
                line.Layer = new Layer(layerName);
                line.Layer.Color.Index = 6;
                lines.Add(line);
            }

            Console.WriteLine("Time creating entities : " + crono.ElapsedMilliseconds/1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Reset();

            crono.Start();
            dxf.Entities.Add(lines);
            //dxf.Entities.Add(pols);
            Console.WriteLine("Time adding entities to document : " + crono.ElapsedMilliseconds/1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Reset();

            //crono.Start();
            //dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            //dxf.Save("speedtest (netDxf 2000).dxf");
            //Console.WriteLine("Time saving file 2000 : " + crono.ElapsedMilliseconds/1000.0f);
            //totalTime += crono.ElapsedMilliseconds;
            //crono.Reset();

            //crono.Start();
            //dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            //dxf.Save("speedtest (binary netDxf 2000).dxf", true);
            //Console.WriteLine("Time saving binary file 2000 : " + crono.ElapsedMilliseconds/1000.0f);
            //totalTime += crono.ElapsedMilliseconds;
            //crono.Reset();

            crono.Start();
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("speedtest (netDxf 2010).dxf");
            Console.WriteLine("Time saving file 2010 : " + crono.ElapsedMilliseconds / 1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Reset();


            //crono.Start();
            //dxf = DxfDocument.Load("speedtest (netDxf 2000).dxf");
            //Console.WriteLine("Time loading file 2000: " + crono.ElapsedMilliseconds/1000.0f);
            //totalTime += crono.ElapsedMilliseconds;
            //crono.Stop();
            //crono.Reset();

            //crono.Start();
            //dxf = DxfDocument.Load("speedtest (binary netDxf 2000).dxf");
            //Console.WriteLine("Time loading binary file 2000: " + crono.ElapsedMilliseconds/1000.0f);
            //totalTime += crono.ElapsedMilliseconds;
            //crono.Stop();
            //crono.Reset();

            crono.Start();
            dxf = DxfDocument.Load("speedtest (netDxf 2010).dxf");
            Console.WriteLine("Time loading file 2010: " + crono.ElapsedMilliseconds / 1000.0f);
            totalTime += crono.ElapsedMilliseconds;
            crono.Stop();
            crono.Reset();

            Console.WriteLine("Total time : " + totalTime/1000.0f);
            Console.ReadLine();
        }

        private static void WriteNestedInsert()
        {
            // nested blocks
            DxfDocument dxf = new DxfDocument();

            Block nestedBlock = new Block("Nested block");
            Circle circle = new Circle(Vector3.Zero, 5);
            circle.Layer = new Layer("circle");
            circle.Layer.Color.Index = 2;
            nestedBlock.Entities.Add(circle);

            AttributeDefinition attdef = new AttributeDefinition("NewAttribute");
            attdef.Prompt = "InfoText";
            attdef.Alignment = TextAlignment.MiddleCenter;
            nestedBlock.AttributeDefinitions.Add(attdef);

            Insert nestedInsert = new Insert(nestedBlock, new Vector3(0, 0, 0)); // the position will be relative to the position of the insert that nest it
            nestedInsert.Attributes[0].Value = 24.ToString();

            Insert nestedInsert2 = new Insert(nestedBlock, new Vector3(-20, 0, 0)); // the position will be relative to the position of the insert that nest it
            nestedInsert2.Attributes[0].Value = (-20).ToString();

            Block block = new Block("MyBlock");
            block.Entities.Add(new Line(new Vector3(-5, -5, 0), new Vector3(5, 5, 0)));
            block.Entities.Add(new Line(new Vector3(5, -5, 0), new Vector3(-5, 5, 0)));
            block.Entities.Add(nestedInsert);
            block.Entities.Add(nestedInsert2);

            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");

            dxf.Entities.Add(insert);
            //dxf.Entities.Add(circle); // this is not allowed the circle is already part of a block

            dxf.Save("nested insert.dxf");
            dxf = DxfDocument.Load("nested insert.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("nested insert copy.dxf");
        }

        private static void WriteDxfFile()
        {
            DxfDocument dxf = new DxfDocument();

            //arc
            Arc arc = new Arc(new Vector3(10, 10, 0), 10, 45, 135);
            arc.Layer = new Layer("arc");
            arc.Layer.Color.Index = 1;
            dxf.Entities.Add(arc);

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
            xdata2.XDataRecord.Add(new XDataRecord(XDataCode.Int32, 350));
            xdata2.XDataRecord.Add(XDataRecord.CloseControlString);

            //circle
            Vector3 extrusion = new Vector3(1, 1, 1);
            Vector3 centerWCS = new Vector3(1, 1, 1);
            Vector3 centerOCS = MathHelper.Transform(centerWCS,
                extrusion,
                CoordinateSystem.World,
                CoordinateSystem.Object);

            Circle circle = new Circle(centerOCS, 5);
            circle.Layer = new Layer("circle with spaces");
            circle.Layer.Color = AciColor.Yellow;
            circle.Linetype = Linetype.Dashed;
            circle.Normal = extrusion;
            circle.XData.Add(xdata);
            circle.XData.Add(xdata2);

            dxf.Entities.Add(circle);

            //points
            Point point1 = new Point(new Vector3(-3, -3, 0));
            point1.Layer = new Layer("point");
            point1.Color = new AciColor(30);
            Point point2 = new Point(new Vector3(1, 1, 1));
            point2.Layer = point1.Layer;
            point2.Layer.Color.Index = 9;
            point2.Normal = new Vector3(1, 1, 1);
            dxf.Entities.Add(point1);
            dxf.Entities.Add(point2);

            //3dface
            Face3D face3D = new Face3D(new Vector3(-5, -5, 5),
                new Vector3(5, -5, 5),
                new Vector3(5, 5, 5),
                new Vector3(-5, 5, 5));
            face3D.Layer = new Layer("3dface");
            face3D.Layer.Color.Index = 3;
            dxf.Entities.Add(face3D);

            //polyline
            Polyline2DVertex polyVertex;
            List<Polyline2DVertex> polyVertexes = new List<Polyline2DVertex>();
            polyVertex = new Polyline2DVertex(new Vector2(-50, -50));
            polyVertex.StartWidth = 2;
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(50, -50));
            polyVertex.StartWidth = 1;
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(50, 50));
            polyVertex.Bulge = 1;
            polyVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(-50, 50));
            polyVertexes.Add(polyVertex);
            Polyline2D polyline2d = new Polyline2D(polyVertexes, true);
            polyline2d.Layer = new Layer("polyline2D");
            polyline2d.Layer.Color.Index = 5;
            polyline2d.Normal = new Vector3(1, 1, 1);
            polyline2d.Elevation = 100.0f;
            dxf.Entities.Add(polyline2d);

            //lightweight polyline
            List<Polyline2DVertex> lwVertexes = new List<Polyline2DVertex>();
            polyVertex = new Polyline2DVertex(new Vector2(-25, -25));
            polyVertex.StartWidth = 2;
            lwVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(25, -25));
            polyVertex.StartWidth = 1;
            lwVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(25, 25));
            polyVertex.Bulge = 1;
            lwVertexes.Add(polyVertex);
            polyVertex = new Polyline2DVertex(new Vector2(-25, 25));
            lwVertexes.Add(polyVertex);
            Polyline2D polyline2D = new Polyline2D(lwVertexes, true);
            polyline2D.Layer = new Layer("polyline2D");
            polyline2D.Layer.Color.Index = 5;
            polyline2D.Normal = new Vector3(1, 1, 1);
            polyline2D.Elevation = 100.0f;
            dxf.Entities.Add(polyline2D);

            // polyfaceMesh
            List<Vector3> meshVertexes = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(10, 10, 0),
                new Vector3(5, 15, 0),
                new Vector3(0, 10, 0)
            };
            List<short[]> faces = new List<short[]>
            {
                new short[] {1, 2, -3},
                new short[] {-1, 3, -4},
                new short[] {-1, 4, 5}
            };

            PolyfaceMesh mesh = new PolyfaceMesh(meshVertexes, faces);
            mesh.Layer = new Layer("polyface mesh");
            mesh.Layer.Color.Index = 104;
            dxf.Entities.Add(mesh);

            //line
            Line line = new Line(new Vector3(0, 0, 0), new Vector3(10, 10, 10));
            line.Layer = new Layer("line");
            line.Layer.Color.Index = 6;
            dxf.Entities.Add(line);

            //3d polyline
            Vector3 vertex;
            List<Vector3> vertexes = new List<Vector3>();
            vertex = new Vector3(-50, -50, 0);
            vertexes.Add(vertex);
            vertex = new Vector3(50, -50, 10);
            vertexes.Add(vertex);
            vertex = new Vector3(50, 50, 25);
            vertexes.Add(vertex);
            vertex = new Vector3(-50, 50, 50);
            vertexes.Add(vertex);
            Polyline3D polyline3D = new Polyline3D(vertexes, true);
            polyline3D.Layer = new Layer("polyline3D");
            polyline3D.Layer.Color.Index = 24;
            dxf.Entities.Add(polyline3D);

            //block definition
            Block block = new Block("TestBlock");
            block.Entities.Add(new Line(new Vector3(-5, -5, 5), new Vector3(5, 5, 5)));
            block.Entities.Add(new Line(new Vector3(5, -5, 5), new Vector3(-5, 5, 5)));

            //insert
            Insert insert = new Insert(block, new Vector3(5, 5, 5));
            insert.Layer = new Layer("insert");
            insert.Layer.Color.Index = 4;
            dxf.Entities.Add(insert);

            //text
            TextStyle style = new TextStyle("True type font", "Arial.ttf");
            Text text = new Text("Hello world!", Vector3.Zero, 10.0f, style);
            text.Layer = new Layer("text");
            text.Layer.Color.Index = 8;
            text.Alignment = TextAlignment.TopRight;
            dxf.Entities.Add(text);

            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2010;
            dxf.Save("AutoCad2010.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2007;
            dxf.Save("AutoCad2007.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2004;
            dxf.Save("AutoCad2004.dxf");
            dxf.DrawingVariables.AcadVer = DxfVersion.AutoCad2000;
            dxf.Save("AutoCad2000.dxf");
            dxf = DxfDocument.Load("AutoCad2000.dxf");
            dxf.Save("AutoCad2000 result.dxf");
        }

        private static void WritePolyline3d()
        {
            DxfDocument dxf = new DxfDocument();

            List<Vector3> vertexes = new List<Vector3>
            {
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 10),
                new Vector3(10, 10, 20),
                new Vector3(0, 10, 30)
            };

            Polyline3D poly = new Polyline3D(vertexes, true);

            XData xdata = new XData(new ApplicationRegistry("netDxf"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "extended data with netDxf"));
            xdata.XDataRecord.Add(XDataRecord.OpenControlString);
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.String, "netDxf polyline3d"));
            xdata.XDataRecord.Add(new XDataRecord(XDataCode.Int16, poly.Vertexes.Count));
            xdata.XDataRecord.Add(XDataRecord.CloseControlString);

            poly.XData.Add(xdata);

            dxf.Entities.Add(poly);

            dxf.Save("polyline.dxf");
        }

        private static void TestLayoutRemoval()
        {
            Line line1 = new Line(new Vector2(0, 0), new Vector2(100, 100));
            Line line2 = new Line(new Vector2(100, 0), new Vector2(200, 100));
            Line line3 = new Line(new Vector2(200, 0), new Vector2(300, 100));
            Line line4 = new Line(new Vector2(200, 0), new Vector2(300, 100));
            Line line5 = new Line(new Vector2(200, 0), new Vector2(300, 100));

            Layout layout1 = new Layout("Layout1");
            Layout layout2 = new Layout("Layout2");
            Layout layout3 = new Layout("Layout3");
            Layout layout4 = new Layout("Layout4");

            DxfDocument doc = new DxfDocument();
            doc.Layouts.Add(layout1);
            doc.Layouts.Add(layout2);
            doc.Layouts.Add(layout3);
            doc.Layouts.Add(layout4);

            doc.Entities.Add(line1); // *Model_Space

            doc.Entities.ActiveLayout = layout1.Name;
            doc.Entities.Add(line2); // *Paper_Space

            doc.Entities.ActiveLayout = layout2.Name;
            doc.Entities.Add(line3); // *Paper_Space0

            doc.Entities.ActiveLayout = layout3.Name;
            doc.Entities.Add(line4); // *Paper_Space1

            doc.Entities.ActiveLayout = layout4.Name;
            doc.Entities.Add(line5); // *Paper_Space2

            doc.Layouts.Remove(layout1);

            Console.WriteLine(line2.Owner);
            Console.WriteLine(ReferenceEquals(line3.Owner, layout2.AssociatedBlock));
            Console.WriteLine(ReferenceEquals(line4.Owner, layout3.AssociatedBlock));
            Console.WriteLine(ReferenceEquals(line5.Owner, layout4.AssociatedBlock));

            Console.ReadKey();
        }

        private static void NestedBlock()
        {
            Block blockMM = new Block("BlockMM");
            blockMM.Record.Units = DrawingUnits.Millimeters;
            AttributeDefinition attDefMM = new AttributeDefinition("MyAttributeMM");
            attDefMM.Height = 1.0;
            attDefMM.Value = "This is block mm";
            blockMM.AttributeDefinitions.Add(attDefMM);
            Line line1MM = new Line(Vector2.Zero, Vector2.UnitX);
            blockMM.Entities.Add(line1MM);
            Insert insMM = new Insert(blockMM);
            insMM.TransformAttributes();

            Block blockCM = new Block("BlockCM");
            blockCM.Record.Units = DrawingUnits.Centimeters;
            AttributeDefinition attDefCM = new AttributeDefinition("MyAttributeCM");
            attDefCM.Height = 1.0;
            attDefCM.Value = "This is block cm";
            blockCM.AttributeDefinitions.Add(attDefCM);
            Line line1CM = new Line(Vector2.Zero, Vector2.UnitY);
            blockCM.Entities.Add(line1CM);
            blockCM.Entities.Add(insMM);
            Insert insCM = new Insert(blockCM);

            DxfDocument doc = new DxfDocument();
            doc.DrawingVariables.InsUnits = DrawingUnits.Meters;
            //doc.Entities.Add(insMM);
            doc.Entities.Add(insCM);

            doc.Save("test.dxf");
        }

        private static void BlockAttributeTransformation()
        {
            DxfDocument doc = DxfDocument.Load("Drawing1.dxf");
            Insert ins = doc.Entities.Inserts.ElementAt(0);
            Console.WriteLine(ins.Attributes[0].Position);
            Console.WriteLine(ins.Attributes[0].Rotation);
            Console.WriteLine(ins.Attributes[0].Normal);
            Console.WriteLine(ins.Attributes[0].Height);
            Console.WriteLine(ins.Attributes[0].WidthFactor);
            Console.WriteLine(ins.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            ins.TransformAttributes();
            Console.WriteLine(ins.Attributes[0].Position);
            Console.WriteLine(ins.Attributes[0].Rotation);
            Console.WriteLine(ins.Attributes[0].Normal);
            Console.WriteLine(ins.Attributes[0].Height);
            Console.WriteLine(ins.Attributes[0].WidthFactor);
            Console.WriteLine(ins.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            Console.WriteLine("...");
            Console.WriteLine("...");
            Console.WriteLine("...");
            DxfDocument doc2 = DxfDocument.Load("Drawing2.dxf");
            Insert ins2 = doc2.Entities.Inserts.ElementAt(0);
            Console.WriteLine(ins2.Attributes[0].Position);
            Console.WriteLine(ins2.Attributes[0].Rotation);
            Console.WriteLine(ins2.Attributes[0].Normal);
            Console.WriteLine(ins2.Attributes[0].Height);
            Console.WriteLine(ins2.Attributes[0].WidthFactor);
            Console.WriteLine(ins2.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            ins2.TransformAttributes();
            Console.WriteLine(ins2.Attributes[0].Position);
            Console.WriteLine(ins2.Attributes[0].Rotation);
            Console.WriteLine(ins2.Attributes[0].Normal);
            Console.WriteLine(ins2.Attributes[0].Height);
            Console.WriteLine(ins2.Attributes[0].WidthFactor);
            Console.WriteLine(ins2.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            Console.WriteLine("...");
            Console.WriteLine("...");
            Console.WriteLine("...");
            DxfDocument doc3 = DxfDocument.Load("Drawing3.dxf");
            Insert ins3 = doc3.Entities.Inserts.ElementAt(0);
            Console.WriteLine(ins3.Attributes[0].Position);
            Console.WriteLine(ins3.Attributes[0].Rotation);
            Console.WriteLine(ins3.Attributes[0].Normal);
            Console.WriteLine(ins3.Attributes[0].Height);
            Console.WriteLine(ins3.Attributes[0].WidthFactor);
            Console.WriteLine(ins3.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            ins3.TransformAttributes();
            Console.WriteLine(ins3.Attributes[0].Position);
            Console.WriteLine(ins3.Attributes[0].Rotation);
            Console.WriteLine(ins3.Attributes[0].Normal);
            Console.WriteLine(ins3.Attributes[0].Height);
            Console.WriteLine(ins3.Attributes[0].WidthFactor);
            Console.WriteLine(ins3.Attributes[0].ObliqueAngle);
            Console.WriteLine("...");
            Console.ReadKey();


            doc3.Save("test.dxf");

            //Block block = new Block("MyBlock");
            //block.Entities.Add(new Line(new Vector2(-5, 0), new Vector2(5, 0)));

            //AttributeDefinition attDef = new AttributeDefinition("MyAttribute");
            //attDef.Prompt = "Enter a value:";
            //attDef.Value = 0.0;
            ////attDef.Normal = Vector3.UnitX;
            //block.AttributeDefinitions.Add(attDef);

            //Insert ins1 = new Insert(block);
            //ins1.Attributes["MyAttribute"].Value = "Text";

            //Insert ins2 = new Insert(block);
            //ins2.Position = new Vector3(5, 7.5, 1);
            //ins2.Rotation = 30;
            //ins2.TransformAttributes();

            //Insert ins3 = new Insert(block);
            //ins3.Position = new Vector3(-5, -7.5, -1);
            //ins3.Rotation = 30;
            ////ins3.Normal = Vector3.UnitX;
            //ins3.TransformAttributes();

            //DxfDocument doc = new DxfDocument();
            //doc.Entities.Add(ins1);
            ////doc.Entities.Add(ins2);
            ////doc.Entities.Add(ins3);

            //doc.Save("BlockAttributeTransformation.dxf");
        }
    }
}