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
using System.Globalization;
using netDxf.Blocks;
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Holds methods to build the dimension blocks. TODO: Find a better place?
    /// </summary>
    internal class DimensionBlock
    {
        #region private methods

        private static string FormatDimensionText(double measure, bool angular, DimensionStyle style)
        {
            string format;
            string text;

            NumberFormatInfo numberFormat = new NumberFormatInfo {NumberDecimalSeparator = style.DIMDSEP.ToString(CultureInfo.InvariantCulture)};
            
            if (angular)
            {
                format = "F" + style.DIMADEC;
                text = measure.ToString(format, numberFormat) + Symbols.Degree;
            }
            else
            {
                format = "F" + style.DIMDEC;
                text = measure.ToString(format, numberFormat);
            }
            text = style.DIMPOST.Replace("<>", text);
            return text;
        }

        private static Line DimensionLine(Vector2 start, Vector2 end, double rotation, short reversed, DimensionStyle style)
        {
            double ext1 = style.DIMASZ*style.DIMSCALE;
            double ext2 = -style.DIMASZ*style.DIMSCALE;

            Block block;

            block = style.DIMSAH ? style.DIMBLK1 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                    ext1 = -style.DIMDLE*style.DIMSCALE;
            }

            block = style.DIMSAH ? style.DIMBLK2 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                    ext2 = style.DIMDLE*style.DIMSCALE;
            }

            start = Vector2.Polar(start, reversed*ext1, rotation);
            end = Vector2.Polar(end, reversed*ext2, rotation);

            return new Line(start, end)
            {
                Color = style.DIMCLRD,
                LineType = style.DIMLTYPE,
                Lineweight = style.DIMLWD
            };
        }

        private static Arc DimensionArc(Vector2 center, Vector2 start, Vector2 end, double startAngle, double endAngle, double radius, double rotation, short reversed, DimensionStyle style, out double e1, out double e2)
        {
            double ext1 = style.DIMASZ*style.DIMSCALE;
            double ext2 = -style.DIMASZ*style.DIMSCALE;
            e1 = ext1;
            e2 = ext2;

            Block block;

            block = style.DIMSAH ? style.DIMBLK1 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                {
                    ext1 = 0.0;
                    e1 = 0.0;
                }
            }

            block = style.DIMSAH ? style.DIMBLK2 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                {
                    ext2 = 0.0;
                    e2 = 0.0;
                }
            }

            start = Vector2.Polar(start, reversed*ext1, startAngle + MathHelper.HalfPI);
            end = Vector2.Polar(end, reversed*ext2, endAngle + MathHelper.HalfPI);

            startAngle = Vector2.Angle(center, start)*MathHelper.RadToDeg;
            endAngle = Vector2.Angle(center, end)*MathHelper.RadToDeg;
            return new Arc(center, radius, startAngle, endAngle)
            {
                Color = style.DIMCLRD,
                LineType = style.DIMLTYPE,
                Lineweight = style.DIMLWD
            };
        }

        private static Line DimensionRadialLine(Vector2 start, Vector2 end, double rotation, short reversed, DimensionStyle style)
        {
            double ext1 = style.DIMASZ * style.DIMSCALE;
            double ext2 = -style.DIMASZ * style.DIMSCALE;

            Block block;

            block = style.DIMSAH ? style.DIMBLK1 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                    ext1 = -style.DIMDLE * style.DIMSCALE;
            }

            block = style.DIMSAH ? style.DIMBLK2 : style.DIMBLK;
            if (block != null)
            {
                if (block.Name.Equals("_OBLIQUE", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_ARCHTICK", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_INTEGRAL", StringComparison.OrdinalIgnoreCase) ||
                    block.Name.Equals("_NONE", StringComparison.OrdinalIgnoreCase))
                    ext2 = style.DIMDLE * style.DIMSCALE;
            }

            //start = Vector2.Polar(start, reversed * ext1, rotation);
            end = Vector2.Polar(end, reversed * ext2, rotation);

            return new Line(start, end)
            {
                Color = style.DIMCLRD,
                LineType = style.DIMLTYPE,
                Lineweight = style.DIMLWD
            };
        }

        private static Line ExtensionLine(Vector2 start, Vector2 end, DimensionStyle style, LineType linetype)
        {
            return new Line(start, end)
            {
                Color = style.DIMCLRE,
                LineType = linetype,
                Lineweight = style.DIMLWE
            };
        }

        private static EntityObject StartArrowHead(Vector2 position, double rotation, DimensionStyle style)
        {
            Block block = style.DIMSAH ? style.DIMBLK1 : style.DIMBLK;

            if (block == null)
            {
                Vector2 arrowRef = Vector2.Polar(position, -style.DIMASZ*style.DIMSCALE, rotation);
                Solid arrow = new Solid(position,
                    Vector2.Polar(arrowRef, -(style.DIMASZ/6)*style.DIMSCALE, rotation + MathHelper.HalfPI),
                    Vector2.Polar(arrowRef, (style.DIMASZ/6)*style.DIMSCALE, rotation + MathHelper.HalfPI))
                {
                    Color = style.DIMCLRD
                };
                return arrow;
            }
            else
            {
                Insert arrow = new Insert(block, position)
                {
                    Color = style.DIMCLRD,
                    Scale = new Vector3(style.DIMASZ*style.DIMSCALE),
                    Rotation = rotation*MathHelper.RadToDeg
                };
                return arrow;
            }
        }

        private static EntityObject EndArrowHead(Vector2 position, double rotation, DimensionStyle style)
        {
            Block block = style.DIMSAH ? style.DIMBLK2 : style.DIMBLK;

            if (block == null)
            {
                Vector2 arrowRef = Vector2.Polar(position, -style.DIMASZ*style.DIMSCALE, rotation);
                Solid arrow = new Solid(position,
                    Vector2.Polar(arrowRef, -(style.DIMASZ/6)*style.DIMSCALE, rotation + MathHelper.HalfPI),
                    Vector2.Polar(arrowRef, (style.DIMASZ/6)*style.DIMSCALE, rotation + MathHelper.HalfPI))
                {
                    Color = style.DIMCLRD
                };
                return arrow;
            }
            else
            {
                Insert arrow = new Insert(block, position)
                {
                    Color = style.DIMCLRD,
                    Scale = new Vector3(style.DIMASZ*style.DIMSCALE),
                    Rotation = rotation*MathHelper.RadToDeg
                };
                return arrow;
            }
        }

        private static MText DimensionText(Vector2 position, double rotation, string text, DimensionStyle style)
        {
            MText mText = new MText(text, position, style.DIMTXT*style.DIMSCALE, 0.0, style.DIMTXSTY)
            {
                Color = style.DIMCLRT,
                AttachmentPoint = MTextAttachmentPoint.BottomCenter,
                Rotation = rotation * MathHelper.RadToDeg
            };

            return mText;
        }

        private static List<EntityObject> CenterCross(Vector2 center, double radius, DimensionStyle style)
        {
            List<EntityObject> lines = new List<EntityObject>();
            if (MathHelper.IsZero(style.DIMCEN))
                return lines;

            Vector2 c1;
            Vector2 c2;
            double dist = Math.Abs(style.DIMCEN * style.DIMSCALE);

            // center mark
            c1 = new Vector2(0.0, -dist) + center;
            c2 = new Vector2(0.0, dist) + center;
            lines.Add(new Line(c1, c2) { Color = style.DIMCLRE, Lineweight = style.DIMLWE });
            c1 = new Vector2(-dist, 0.0) + center;
            c2 = new Vector2(dist, 0.0) + center;
            lines.Add(new Line(c1, c2) { Color = style.DIMCLRE, Lineweight = style.DIMLWE });

            // center lines
            if (style.DIMCEN < 0)
            {
                c1 = new Vector2(2 * dist, 0.0) + center;
                c2 = new Vector2(radius + dist, 0.0) + center;
                lines.Add(new Line(c1, c2) { Color = style.DIMCLRE, Lineweight = style.DIMLWE });

                c1 = new Vector2(-2 * dist, 0.0) + center;
                c2 = new Vector2(-radius - dist, 0.0) + center;
                lines.Add(new Line(c1, c2){Color = style.DIMCLRE,Lineweight = style.DIMLWE});

                c1 = new Vector2(0.0, 2 * dist) + center;
                c2 = new Vector2(0.0, radius + dist) + center;
                lines.Add(new Line(c1, c2) { Color = style.DIMCLRE, Lineweight = style.DIMLWE });

                c1 = new Vector2(0.0, -2 * dist) + center;
                c2 = new Vector2(0.0, -radius - dist) + center;
                lines.Add(new Line(c1, c2) { Color = style.DIMCLRE, Lineweight = style.DIMLWE });
            }
            return lines;
        }

        private static short ReverseEnds(Vector2 u, Vector2 v, double dimRot, double angleRef)
        {
            short side = 1;
            double rot = (dimRot + angleRef)*MathHelper.RadToDeg;
            rot = MathHelper.NormalizeAngle(side*rot);
            if (rot >= 180 && rot < 360)
                side *= -1;

            Vector2 dir = v - u;
            if (dir.X*dir.Y < 0)
                side *= -1;

            return side;
        }

        private static Vector2 FindIntersection(Vector2 point0, Vector2 dir0, Vector2 point1, Vector2 dir1, double threshold = MathHelper.Epsilon)
        {
            // test for parallelism.
            if (Vector2.AreParallel(dir0, dir1, threshold))
                return new Vector2(double.NaN, double.NaN);

            // lines are not parallel
            Vector2 vect = point1 - point0;
            double cross = Vector2.CrossProduct(dir0, dir1);
            double s = (vect.X*dir1.Y - vect.Y*dir1.X)/cross;
            return point0 + s*dir0;
        }

        #endregion

        #region public methods

        public static Block Build(LinearDimension dim, string name)
        {
            Vector2 ref1;
            Vector2 ref2;
            Vector2 dimRef1;
            Vector2 dimRef2;
            short reversed;
            double measure = dim.Value;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.FirstReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            ref1 = new Vector2(refPoint.X, refPoint.Y);
            double elevation = refPoint.Z;

            refPoint = MathHelper.Transform(dim.SecondReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            ref2 = new Vector2(refPoint.X, refPoint.Y);

            double dimRotation = dim.Rotation*MathHelper.DegToRad;
            double refAngle = Vector2.Angle(ref1, ref2);
            reversed = ReverseEnds(ref1, ref2, dimRotation, refAngle);

            Vector2 midRef = Vector2.MidPoint(ref1, ref2);
            Vector2 midDim = Vector2.Polar(midRef, dim.Offset, dimRotation + MathHelper.HalfPI);

            dimRef1 = Vector2.Polar(midDim, -reversed*measure*0.5, dimRotation);
            dimRef2 = Vector2.Polar(midDim, reversed*measure*0.5, dimRotation);

            // reference points
            Layer defPointLayer = new Layer("Defpoints") {Plot = false};
            Point ref1Point = new Point(ref1) {Layer = defPointLayer};
            Point ref2Point = new Point(ref2) {Layer = defPointLayer};
            Point defPoint = new Point(dimRef2) {Layer = defPointLayer};

            // dimension line
            Line dimLine = DimensionLine(dimRef1, dimRef2, dimRotation, reversed, style);

            // extension lines
            double dimexo = Math.Sign(dim.Offset)*style.DIMEXO*style.DIMSCALE;
            double dimexe = Math.Sign(dim.Offset)*style.DIMEXE*style.DIMSCALE;
            double extRot = dimRotation + MathHelper.HalfPI;
            Line ext1Line = null;
            if (!style.DIMSE1)
                ext1Line = ExtensionLine(Vector2.Polar(ref1, dimexo, extRot), Vector2.Polar(dimRef1, dimexe, extRot), style, style.DIMLTEX1);

            Line ext2Line = null;
            if (!style.DIMSE2)
                ext2Line = ExtensionLine(Vector2.Polar(ref2, dimexo, extRot), Vector2.Polar(dimRef2, dimexe, extRot), style, style.DIMLTEX2);

            EntityObject arrow1 = StartArrowHead(dimRef1, Vector2.Angle(dimRef2, dimRef1), style);
            EntityObject arrow2 = EndArrowHead(dimRef2, Vector2.Angle(dimRef1, dimRef2), style);

            // dimension text
            string text = FormatDimensionText(measure, false, style);
            MText mText = DimensionText(Vector2.Polar(midDim, style.DIMGAP*style.DIMSCALE, extRot), dimRotation, text, style);

            dim.DefinitionPoint = MathHelper.Transform(new Vector3(dimRef2.X, dimRef2.Y, elevation), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.MidTextPoint = new Vector3(midDim.X, midDim.Y, elevation); // this value is in OCS

            // drawing block
            Block block = new Block(name, false) {Flags = BlockTypeFlags.AnonymousBlock};
            block.Entities.Add(ref1Point);
            block.Entities.Add(ref2Point);
            block.Entities.Add(defPoint);
            block.Entities.Add(ext1Line);
            block.Entities.Add(ext2Line);
            block.Entities.Add(dimLine);
            block.Entities.Add(arrow1);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(AlignedDimension dim, string name)
        {
            Vector2 ref1;
            Vector2 ref2;
            Vector2 dimRef1;
            Vector2 dimRef2;
            short reversed;
            double measure = dim.Value;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.FirstReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            ref1 = new Vector2(refPoint.X, refPoint.Y);
            double elevation = refPoint.Z;

            refPoint = MathHelper.Transform(dim.SecondReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            ref2 = new Vector2(refPoint.X, refPoint.Y);

            double refAngle = Vector2.Angle(ref1, ref2);

            reversed = ReverseEnds(ref1, ref2, 0.0, refAngle);

            dimRef1 = Vector2.Polar(ref1, dim.Offset, refAngle + MathHelper.HalfPI);
            dimRef2 = Vector2.Polar(ref2, dim.Offset, refAngle + MathHelper.HalfPI);

            Vector2 midDim = Vector2.MidPoint(dimRef1, dimRef2);

            // reference points
            Layer defPointLayer = new Layer("Defpoints") {Plot = false};
            Point ref1Point = new Point(ref1) {Layer = defPointLayer};
            Point ref2Point = new Point(ref2) {Layer = defPointLayer};
            Point defPoint = new Point(dimRef2) {Layer = defPointLayer};

            // dimension lines
            Line dimLine = DimensionLine(dimRef1, dimRef2, refAngle, 1, style);

            // extension lines    
            double dimexo = Math.Sign(dim.Offset)*style.DIMEXO*style.DIMSCALE;
            double dimexe = Math.Sign(dim.Offset)*style.DIMEXE*style.DIMSCALE;
            double extRot = refAngle + MathHelper.HalfPI;
            Line ext1Line = null;
            if (!style.DIMSE1)
                ext1Line = ExtensionLine(Vector2.Polar(ref1, dimexo, extRot), Vector2.Polar(dimRef1, dimexe, extRot), style, style.DIMLTEX1);

            Line ext2Line = null;
            if (!style.DIMSE2)
                ext2Line = ExtensionLine(Vector2.Polar(ref2, dimexo, extRot), Vector2.Polar(dimRef2, dimexe, extRot), style, style.DIMLTEX2);

            // dimension arrows
            EntityObject arrow1 = StartArrowHead(dimRef1, Vector2.Angle(dimRef2, dimRef1), style);
            EntityObject arrow2 = EndArrowHead(dimRef2, Vector2.Angle(dimRef1, dimRef2), style);

            // dimension text
            string text = FormatDimensionText(measure, false, style);
            double textRot = (refAngle + (1 - reversed)*MathHelper.HalfPI);
            MText mText = DimensionText(Vector2.Polar(midDim, reversed*style.DIMGAP*style.DIMSCALE, extRot), textRot, text, style);

            dim.DefinitionPoint = MathHelper.Transform(new Vector3(dimRef2.X, dimRef2.Y, elevation), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.MidTextPoint = new Vector3(midDim.X, midDim.Y, elevation); // this value is in OCS

            // drawing block
            Block block = new Block(name, false) {Flags = BlockTypeFlags.AnonymousBlock};
            block.Entities.Add(ref1Point);
            block.Entities.Add(ref2Point);
            block.Entities.Add(defPoint);
            block.Entities.Add(ext1Line);
            block.Entities.Add(ext2Line);
            block.Entities.Add(dimLine);
            block.Entities.Add(arrow1);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(Angular2LineDimension dim, string name)
        {
            double offset = dim.Offset;
            double measure = dim.Value;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.StartFirstLine, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref1Start = new Vector2(refPoint.X, refPoint.Y);
            double elevation = refPoint.Z;

            refPoint = MathHelper.Transform(dim.EndFirstLine, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref1End = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(dim.StartSecondLine, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref2Start = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(dim.EndSecondLine, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref2End = new Vector2(refPoint.X, refPoint.Y);

            Vector2 dirRef1 = ref1End - ref1Start;
            Vector2 dirRef2 = ref2End - ref2Start;
            if (Vector2.AreParallel(dirRef1, dirRef2))
                throw new ArgumentException("The two lines that define the dimension are parallel.");

            Vector2 center = FindIntersection(ref1Start, dirRef1, ref2Start, dirRef2);
            double startAngle = Vector2.Angle(ref1Start, ref1End);
            double endAngle = Vector2.Angle(ref2Start, ref2End);
            double cross = Vector2.CrossProduct(dirRef1, dirRef2);
            double aperture = Vector2.AngleBetween(dirRef1, dirRef2);

            if (cross < 0)
            {
                Vector2 tmp1 = ref1Start;
                Vector2 tmp2 = ref1End;
                ref1Start = ref2Start;
                ref1End = ref2End;
                ref2Start = tmp1;
                ref2End = tmp2;
                double tmp = startAngle;
                startAngle = endAngle;
                endAngle = tmp;
            }
            short reversed = 1;
            if (offset < 0)
            {
                Vector2 tmp1 = ref1Start;
                Vector2 tmp2 = ref2Start;
                ref1Start = ref1End;
                ref1End = tmp1;
                ref2Start = ref2End;
                ref2End = tmp2;
                reversed = -1;
            }

            Vector2 dimRef1 = Vector2.Polar(center, offset, startAngle);
            Vector2 dimRef2 = Vector2.Polar(center, offset, endAngle);
            double refAngle = Vector2.Angle(dimRef1, dimRef2);
            Vector2 midDim = Vector2.Polar(center, offset, startAngle + aperture*0.5);

            // reference points
            Layer defPoints = new Layer("Defpoints") {Plot = false};
            Point startFirstPoint = new Point(ref1Start) {Layer = defPoints};
            Point endFirstPoint = new Point(ref1End) {Layer = defPoints};
            Point startSecondPoint = new Point(ref2Start) {Layer = defPoints};
            Point endSecondPoint = new Point(ref2End) {Layer = defPoints};

            // dimension lines
            double ext1;
            double ext2;
            Arc dimArc = DimensionArc(center, dimRef1, dimRef2, startAngle, endAngle, Math.Abs(offset), refAngle, reversed, style, out ext1, out ext2);

            // dimension arrows
            double angle1 = Math.Asin(ext1*0.5/Math.Abs(offset));
            double angle2 = Math.Asin(ext2*0.5/Math.Abs(offset));
            EntityObject arrow1 = StartArrowHead(dimRef1, (1 - reversed)*MathHelper.HalfPI + angle1 + startAngle - MathHelper.HalfPI, style);
            EntityObject arrow2 = EndArrowHead(dimRef2, (1 - reversed)*MathHelper.HalfPI + angle2 + endAngle + MathHelper.HalfPI, style);

            // dimension lines
            double dimexo = Math.Sign(offset)*style.DIMEXO*style.DIMSCALE;
            double dimexe = Math.Sign(offset)*style.DIMEXE*style.DIMSCALE;

            Line ext1Line = null;
            if (!style.DIMSE1)
                ext1Line = ExtensionLine(Vector2.Polar(ref1End, dimexo, startAngle), Vector2.Polar(dimRef1, dimexe, startAngle), style, style.DIMLTEX1);

            Line ext2Line = null;
            if (!style.DIMSE2)
                ext2Line = ExtensionLine(Vector2.Polar(ref2End, dimexo, endAngle), Vector2.Polar(dimRef2, dimexe, endAngle), style, style.DIMLTEX1);

            // dimension text
            string text = FormatDimensionText(measure, true, style);
            double extRot = startAngle + aperture*0.5;
            double textRot = (extRot - MathHelper.HalfPI);
            MText mText = DimensionText(Vector2.Polar(midDim, style.DIMGAP*style.DIMSCALE, extRot), textRot, text, style);

            dim.DefinitionPoint = dim.EndSecondLine;
            dim.MidTextPoint = new Vector3(midDim.X, midDim.Y, elevation); // this value is in OCS
            dim.ArcDefinitionPoint = dim.MidTextPoint; // this value is in OCS

            // drawing block
            Block block = new Block(name, false) {Flags = BlockTypeFlags.AnonymousBlock};
            block.Entities.Add(startFirstPoint);
            block.Entities.Add(endFirstPoint);
            block.Entities.Add(startSecondPoint);
            block.Entities.Add(endSecondPoint);
            block.Entities.Add(ext1Line);
            block.Entities.Add(ext2Line);
            block.Entities.Add(dimArc);
            block.Entities.Add(arrow1);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(Angular3PointDimension dim, string name)
        {
            double offset = dim.Offset;
            double measure = dim.Value;
            double aperture = measure*MathHelper.DegToRad;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.CenterPoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(dim.StartPoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref1 = new Vector2(refPoint.X, refPoint.Y);

            refPoint = MathHelper.Transform(dim.EndPoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref2 = new Vector2(refPoint.X, refPoint.Y);

            double elevation = refPoint.Z;

            double startAngle = Vector2.Angle(refCenter, ref1);
            double endAngle = Vector2.Angle(refCenter, ref2);
            Vector2 dimRef1 = Vector2.Polar(refCenter, offset, startAngle);
            Vector2 dimRef2 = Vector2.Polar(refCenter, offset, endAngle);
            double refAngle = Vector2.Angle(dimRef1, dimRef2);
            double midRot = startAngle + aperture*0.5;
            Vector2 midDim = Vector2.Polar(refCenter, offset, midRot);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(ref1) { Layer = defPoints };
            Point endRef = new Point(ref2) { Layer = defPoints };
            Point centerPoint = new Point(refCenter) { Layer = defPoints };

            // dimension lines
            double ext1;
            double ext2;
            Arc dimArc = DimensionArc(refCenter, dimRef1, dimRef2, startAngle, endAngle, Math.Abs(offset), refAngle, 1, style, out ext1, out ext2);

            // dimension arrows
            double angle1 = Math.Asin(ext1 * 0.5 / Math.Abs(offset));
            double angle2 = Math.Asin(ext2 * 0.5 / Math.Abs(offset));
            EntityObject arrow1 = StartArrowHead(dimRef1,angle1 + startAngle - MathHelper.HalfPI, style);
            EntityObject arrow2 = EndArrowHead(dimRef2, angle2 + endAngle + MathHelper.HalfPI, style);

            // dimension lines
            double dimexo = Math.Sign(offset) * style.DIMEXO * style.DIMSCALE;
            double dimexe = Math.Sign(offset) * style.DIMEXE * style.DIMSCALE;

            Line ext1Line = null;
            if (!style.DIMSE1)
                ext1Line = ExtensionLine(Vector2.Polar(ref1, dimexo, startAngle), Vector2.Polar(dimRef1, dimexe, startAngle), style, style.DIMLTEX1);

            Line ext2Line = null;
            if (!style.DIMSE2)
                ext2Line = ExtensionLine(Vector2.Polar(ref2, dimexo, endAngle), Vector2.Polar(dimRef2, dimexe, endAngle), style, style.DIMLTEX1);

            // dimension text
            string text = FormatDimensionText(measure, true, style);
            double textRot = midRot - MathHelper.HalfPI;
            MText mText = DimensionText(Vector2.Polar(midDim, style.DIMGAP * style.DIMSCALE, midRot), textRot, text, style);

            dim.DefinitionPoint = MathHelper.Transform(new Vector3(midDim.X, midDim.Y, elevation), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.MidTextPoint = new Vector3(midDim.X, midDim.Y, elevation); // this value is in OCS

            // drawing block
            Block block = new Block(name, false);
            block.Entities.Add(startRef);
            block.Entities.Add(endRef);
            block.Entities.Add(centerPoint);
            block.Entities.Add(ext1Line);
            block.Entities.Add(ext2Line);
            block.Entities.Add(dimArc);
            block.Entities.Add(arrow1);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(DiametricDimension dim, string name)
        {
            double offset = dim.Offset;
            double measure = dim.Value;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.CenterPoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            Vector2 centerRef = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;

            refPoint = MathHelper.Transform(dim.ReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref1 = new Vector2(refPoint.X, refPoint.Y);

            double angleRef = Vector2.Angle(centerRef, ref1);
            Vector2 ref2 = Vector2.Polar(ref1, -measure, angleRef);

            short reverse = 1;
            if(angleRef>MathHelper.HalfPI && angleRef<=MathHelper.ThreeHalfPI)
                reverse = -1;

            short side = 1;
            double minOffset = 2 * style.DIMASZ + style.DIMGAP * style.DIMSCALE;
            if (offset > (measure*0.5 - minOffset) && offset < measure*0.5)
            {
                offset = measure * 0.5 - minOffset;
                side = 1;
            }
            else if (offset >= measure*0.5 && offset < (measure*0.5 + minOffset))
            {
                offset = measure * 0.5 + minOffset;
                side = -1;
            }

            Vector2 dimRef = Vector2.Polar(centerRef, offset, angleRef);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(ref1) { Layer = defPoints };

            // dimension lines
            Line dimLine = DimensionRadialLine(dimRef, ref1, angleRef, side, style);

            // center cross
            List<EntityObject> centerCross = CenterCross(centerRef, measure * 0.5, style);

            // dimension arrows
            //EntityObject arrow1 = StartArrowHead(ref1, angleRef, style);
            EntityObject arrow2 = EndArrowHead(ref1, (1 - side) * MathHelper.HalfPI + angleRef, style);

            // dimension text
            string text = Symbols.Diameter + FormatDimensionText(measure, false, style);
            double textRot = angleRef;
            if (textRot > MathHelper.HalfPI && textRot <= MathHelper.ThreeHalfPI)
                textRot += MathHelper.PI;
            MText mText = DimensionText(Vector2.Polar(dimRef, -reverse * side * style.DIMGAP * style.DIMSCALE, textRot), textRot, text, style);
            mText.AttachmentPoint = reverse*side<0 ? MTextAttachmentPoint.MiddleLeft : MTextAttachmentPoint.MiddleRight;

            dim.DefinitionPoint = MathHelper.Transform(new Vector3(ref2.X, ref2.Y, elev), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.MidTextPoint = new Vector3(dimRef.X, dimRef.Y, elev); // this value is in OCS

            Block block = new Block(name, false);
            block.Entities.Add(startRef);
            //block.Entities.Add(endRef);
            block.Entities.Add(dimLine);
            block.Entities.AddRange(centerCross);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(RadialDimension dim, string name)
        {
            double offset = dim.Offset;
            double measure = dim.Value;
            DimensionStyle style = dim.Style;

            // we will build the dimension block in object coordinates with normal the dimension normal
            Vector3 refPoint;

            refPoint = MathHelper.Transform(dim.CenterPoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);

            Vector2 centerRef = new Vector2(refPoint.X, refPoint.Y);
            double elev = refPoint.Z;

            refPoint = MathHelper.Transform(dim.ReferencePoint, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 ref1 = new Vector2(refPoint.X, refPoint.Y);

            double angleRef = Vector2.Angle(centerRef, ref1);
            Vector2 ref2 = Vector2.Polar(ref1, -measure, angleRef);

            short reverse = 1;
            if (angleRef > MathHelper.HalfPI && angleRef <= MathHelper.ThreeHalfPI)
                reverse = -1;

            short side = 1;
            double minOffset = 2 * style.DIMASZ + style.DIMGAP * style.DIMSCALE;
            if (offset > (measure - minOffset) && offset < measure)
            {
                offset = measure - minOffset;
                side = 1;
            }
            else if (offset >= measure && offset < (measure + minOffset))
            {
                offset = measure + minOffset;
                side = -1;
            }

            Vector2 dimRef = Vector2.Polar(centerRef, offset, angleRef);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(ref1) { Layer = defPoints };

            // dimension lines
            Line dimLine = DimensionRadialLine(dimRef, ref1, angleRef, side, style);

            // center cross
            List<EntityObject> centerCross = CenterCross(centerRef, measure, style);

            // dimension arrows
            //EntityObject arrow1 = StartArrowHead(ref1, angleRef, style);
            EntityObject arrow2 = EndArrowHead(ref1, (1 - side) * MathHelper.HalfPI + angleRef, style);

            // dimension text
            string text = "R" + FormatDimensionText(measure, false, style);
            double textRot = angleRef;
            if (textRot > MathHelper.HalfPI && textRot <= MathHelper.ThreeHalfPI)
                textRot += MathHelper.PI;
            MText mText = DimensionText(Vector2.Polar(dimRef, -reverse * side * style.DIMGAP * style.DIMSCALE, textRot), textRot, text, style);

            mText.AttachmentPoint = reverse * side < 0 ? MTextAttachmentPoint.MiddleLeft : MTextAttachmentPoint.MiddleRight;
            dim.DefinitionPoint = MathHelper.Transform(new Vector3(ref2.X, ref2.Y, elev), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.MidTextPoint = new Vector3(dimRef.X, dimRef.Y, elev); // this value is in OCS

            Block block = new Block(name, false);
            block.Entities.Add(startRef);
            block.Entities.Add(dimLine);
            block.Entities.AddRange(centerCross);
            block.Entities.Add(arrow2);
            block.Entities.Add(mText);
            return block;
        }

        public static Block Build(OrdinateDimension dim, string name)
        {
            DimensionStyle style = dim.Style;
            double measure = dim.Value;

            dim.DefinitionPoint = dim.Origin;
            double angle = dim.Rotation * MathHelper.DegToRad;

            Vector3 localPoint = MathHelper.Transform(dim.Origin, dim.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
            Vector2 refCenter = new Vector2(localPoint.X, localPoint.Y);

            double elev = localPoint.Z;

            Vector2 startPoint = refCenter + MathHelper.Transform(dim.ReferencePoint, angle, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);
            dim.FirstPoint = MathHelper.Transform(new Vector3(startPoint.X, startPoint.Y, elev), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            if (dim.Axis == OrdinateDimensionAxis.X)
                angle += MathHelper.HalfPI;
            Vector2 endPoint = Vector2.Polar(startPoint, dim.Length, angle);
            dim.SecondPoint = MathHelper.Transform(new Vector3(endPoint.X, endPoint.Y, elev), dim.Normal, MathHelper.CoordinateSystem.Object, MathHelper.CoordinateSystem.World);

            // reference points
            Layer defPoints = new Layer("Defpoints") { Plot = false };
            Point startRef = new Point(startPoint) { Layer = defPoints };
            Point endRef = new Point(endPoint) { Layer = defPoints };

            short side = 1;
            if (dim.Length < 0) side = -1;

            // dimension lines
            Line dimLine = new Line(Vector2.Polar(startPoint, style.DIMEXO * style.DIMSCALE, angle), endPoint);

            // dimension text
            Vector2 midText = Vector2.Polar(startPoint, dim.Length + side * style.DIMGAP * style.DIMSCALE, angle);
            dim.MidTextPoint = new Vector3(midText.X, midText.Y, elev); // this value is in OCS

            string text = FormatDimensionText(measure, false, style);

            MText mText = DimensionText(midText, angle, text, style);
            mText.AttachmentPoint = side<0 ? MTextAttachmentPoint.MiddleRight : MTextAttachmentPoint.MiddleLeft;

            // drawing block
            Block block = new Block(name, false);
            block.Entities.Add(startRef);
            block.Entities.Add(endRef);
            block.Entities.Add(dimLine);
            block.Entities.Add(mText);

            return block;
        }

        #endregion
    }
}