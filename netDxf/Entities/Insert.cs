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
using netDxf.Blocks;
using netDxf.Collections;

namespace netDxf.Entities
{

    /// <summary>
    /// Represents a block insertion <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Insert :
        EntityObject
    {
        #region private fields

        private EndSequence endSequence;
        private Block block;
        private Vector3 position;
        private Vector3 scale;
        private double rotation;
        private AttributeDictionary attributes;

        #endregion

        #region constructors

        internal Insert()
            : base (EntityType.Insert, DxfObjectCode.Insert)
        {
            this.endSequence = new EndSequence();
            this.attributes = new AttributeDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert <see cref="Block">block definition</see>.</param>
        public Insert(Block block)
            : this(block, Vector3.Zero)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert block definition.</param>
        /// <param name="position">Insert <see cref="Vector2">position</see> in world coordinates.</param>
        public Insert(Block block, Vector2 position)
            : this(block, new Vector3(position.X, position.Y, 0.0))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert block definition.</param>
        /// <param name="position">Insert <see cref="Vector3">point</see> in world coordinates.</param>
        public Insert(Block block, Vector3 position)
            : base(EntityType.Insert, DxfObjectCode.Insert)
        {
            if (block == null)
                throw new ArgumentNullException("block");
            
            this.block = block;
            this.position = position;
            this.scale = new Vector3(1.0);
            this.rotation = 0.0;
            this.endSequence = new EndSequence();

            List<Attribute> atts = new List<Attribute>(block.AttributeDefinitions.Count);
            foreach (AttributeDefinition attdef in block.AttributeDefinitions.Values)
            {
                Attribute att = new Attribute(attdef)
                {
                    Position = attdef.Position + this.position - this.block.Position,
                    Owner = block
                };
                atts.Add(att);
            }

            this.attributes = new AttributeDictionary(atts);
        }


        #endregion

        #region public properties

        /// <summary>
        /// Gets the insert list of <see cref="Attribute">attributes</see>.
        /// </summary>
        public AttributeDictionary Attributes
        {
            get { return this.attributes; }
            internal set { this.attributes = value; }
        }

        /// <summary>
        /// Gets the insert <see cref="Block">block definition</see>.
        /// </summary>
        public Block Block
        {
            get { return this.block; }
            internal set { this.block = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Vector3">position</see> in world coordinates.
        /// </summary>
        public Vector3 Position
        {
            get { return this.position; }
            set { this.position = value; }
        }

        /// <summary>
        /// Gets or sets the insert <see cref="Vector3">scale</see>.
        /// </summary>
        public Vector3 Scale
        {
            get { return this.scale; }
            set { this.scale = value; }
        }

        /// <summary>
        /// Gets or sets the insert rotation along the normal vector in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        #endregion

        #region internal properties

        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
            set { this.endSequence = value; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Recalculate the attributes position, rotation, height and width factor with the values applied to the insertion.
        /// </summary>
        /// <remarks>
        /// The attributes position, rotation, height and width factor values includes the transformations applied to the insertion,
        /// if required this method will calculate the proper values according to the ones defined by the attribute definition.<br />
        /// Initially the attribute properties holds the same values as the attribute definition but once it belongs to an insertion its values can be changed manually
        /// independently to its definition, usually you will want that the position, rotation, height and/or width factor to be transformed with the insert
        /// as is the behaviour inside AutoCad.<br />
        /// This method only applies to attributes that have a definition, some dxf files might generate attributes that have no definition in the block.
        /// </remarks>
        public void TransformAttributes()
        {
            foreach (Attribute att in this.attributes.Values)
            {
                AttributeDefinition attdef = att.Definition;
                if (attdef == null)
                    continue;

                Vector3 ocsIns = MathHelper.Transform(this.position, this.Normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
                double sine =  Math.Sin(this.rotation * MathHelper.DegToRad);
                double cosine = Math.Cos(this.rotation * MathHelper.DegToRad);
                double x = this.block.Position.X - attdef.Position.X;
                double y = this.block.Position.Y - attdef.Position.Y;

                Vector3 point = new Vector3(x * cosine - y * sine, x * sine + y * cosine, this.block.Position.Z - attdef.Position.Z);
                att.Position = ocsIns - point;
                att.Rotation = attdef.Rotation + this.rotation;
                att.Height = attdef.Height * this.scale.Y;
                att.WidthFactor = attdef.WidthFactor * this.scale.X;
                att.Normal = this.Normal;
            }
        }

        #endregion

        #region overrides

        /// <summary>
        /// Asigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to asign.</param>
        /// <returns>Next avaliable entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will asign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AsignHandle(long entityNumber)
        {
            entityNumber = this.endSequence.AsignHandle(entityNumber);
            foreach (Attribute attrib in this.attributes.Values)
            {
                entityNumber = attrib.AsignHandle(entityNumber);
            }

            return base.AsignHandle(entityNumber);
        }


        /// <summary>
        /// Creates a new Insert that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Insert that is a copy of this instance.</returns>
        public override object Clone()
        {
            List<Attribute> copyAttributes = new List<Attribute>();
            foreach (Attribute att in this.attributes.Values)
                copyAttributes.Add((Attribute)att.Clone());

            return new Insert
                {
                    //EntityObject properties
                    Color = this.color,
                    Layer = this.layer,
                    LineType = this.lineType,
                    Lineweight = this.lineweight,
                    LineTypeScale = this.lineTypeScale,
                    Normal = this.normal,
                    XData = this.xData,
                    //Insert properties
                    Position = this.position,
                    Block = this.block,
                    Scale = this.scale,
                    Rotation = this.rotation,
                    Attributes = new AttributeDictionary(copyAttributes)
                };

        }

        #endregion

        #region Explode

        //public Matrix3 GetTransformation()
        //{
        //    Matrix3 trans = MathHelper.ArbitraryAxis(this.normal);
        //    trans *= Matrix3.RotationZ(this.rotation * MathHelper.DegToRad);
        //    trans *= Matrix3.Scale(this.scale);
        //    return trans;
        //}

        //public List<EntityObject> Explode()
        //{
        //    List<EntityObject> entities = new List<EntityObject>();
        //    Matrix3 trans = this.GetTransformation();
        //    Vector3 pos = this.position - trans*this.block.Position;

        //    foreach (EntityObject entity in this.block.Entities)
        //    {
        //        switch (entity.Type)
        //        {
        //            case (EntityType.Arc):
        //                entities.Add(ProcessArc((Arc)entity, trans, pos, this.scale, this.rotation));
        //                break;
        //            case (EntityType.Circle):
        //                entities.Add(ProcessCircle((Circle)entity, trans, pos, this.scale, this.rotation));
        //                break;
        //            case (EntityType.Ellipse):
        //                entities.Add(ProcessEllipse((Ellipse)entity, trans, pos, this.scale, this.rotation));
        //                break;
        //            case (EntityType.Face3D):
        //                entities.Add(ProcessFace3d((Face3d)entity, trans, pos));
        //                break;
        //            case(EntityType.Hatch):
        //                entities.Add(ProcessHatch((Hatch)entity, trans, pos, this.position, this.normal, this.scale, this.rotation));
        //                break;
        //            case (EntityType.Line):
        //                entities.Add(ProcessLine((Line)entity, trans, pos));
        //                break;
        //        }
        //    }

        //    return entities;
        //}

        //#region private methods

        //private static EntityObject ProcessArc(Arc arc, Matrix3 trans, Vector3 pos, Vector3 scale, double rotation)
        //{
        //    EntityObject copy;
        //    if (MathHelper.IsEqual(scale.X, scale.Y))
        //    {
        //        copy = (Arc)arc.Clone();
        //        ((Arc)copy).Center = trans * arc.Center + pos;
        //        ((Arc)copy).Radius = arc.Radius * scale.X;
        //        ((Arc)copy).StartAngle = arc.StartAngle + rotation;
        //        ((Arc)copy).EndAngle = arc.EndAngle + rotation;
        //        copy.Normal = trans * arc.Normal;
        //        return copy;
        //    }
        //    copy = new Ellipse
        //    {
        //        Center = trans * arc.Center + pos,
        //        MajorAxis = 2 * arc.Radius * scale.X,
        //        MinorAxis = 2 * arc.Radius * scale.Y,
        //        StartAngle = arc.StartAngle,
        //        EndAngle = arc.EndAngle,
        //        Rotation = rotation,
        //        Thickness = arc.Thickness,
        //        Color = arc.Color,
        //        Layer = arc.Layer,
        //        LineType = arc.LineType,
        //        LineTypeScale = arc.LineTypeScale,
        //        Lineweight = arc.Lineweight,
        //        Normal = trans * arc.Normal,
        //        XData = arc.XData
        //    };

        //    return copy;
        //}

        //private static EntityObject ProcessCircle(Circle circle, Matrix3 trans, Vector3 pos, Vector3 scale, double rotation)
        //{
        //    EntityObject copy;
        //    if (MathHelper.IsEqual(scale.X, scale.Y))
        //    {
        //        copy = (Circle)circle.Clone();
        //        ((Circle)copy).Center = trans * circle.Center + pos;
        //        ((Circle)copy).Radius = circle.Radius * scale.X;
        //        copy.Normal = trans * circle.Normal;
        //        return copy;
        //    }
        //    copy = new Ellipse
        //    {
        //        Center = trans * circle.Center + pos,
        //        MajorAxis = 2 * circle.Radius * scale.X,
        //        MinorAxis = 2 * circle.Radius * scale.Y,
        //        Rotation = rotation,
        //        Thickness = circle.Thickness,
        //        Color = circle.Color,
        //        Layer = circle.Layer,
        //        LineType = circle.LineType,
        //        LineTypeScale = circle.LineTypeScale,
        //        Lineweight = circle.Lineweight,
        //        Normal = trans * circle.Normal,
        //        XData = circle.XData
        //    };
        //    return copy;
        //}

        //private static Ellipse ProcessEllipse(Ellipse ellipse, Matrix3 trans, Vector3 pos, Vector3 scale, double rotation)
        //{
        //    Ellipse copy = (Ellipse)ellipse.Clone();
        //    copy.Center = trans * ellipse.Center + pos;
        //    copy.MajorAxis = ellipse.MajorAxis * scale.X;
        //    copy.MinorAxis = ellipse.MinorAxis * scale.Y;
        //    copy.Rotation = rotation;
        //    copy.Normal = trans * ellipse.Normal;

        //    return copy;
        //}

        //private static Face3d ProcessFace3d(Face3d face3d, Matrix3 trans, Vector3 pos)
        //{
        //    Face3d copy = (Face3d)face3d.Clone();
        //    copy.FirstVertex = trans * face3d.FirstVertex + pos;
        //    copy.SecondVertex = trans * face3d.SecondVertex + pos;
        //    copy.ThirdVertex = trans * face3d.ThirdVertex + pos;
        //    copy.FourthVertex = trans * face3d.FourthVertex + pos;

        //    return copy;
        //}

        //private static Hatch ProcessHatch(Hatch hatch, Matrix3 trans, Vector3 pos, Vector3 insertPos, Vector3 normal, Vector3 scale, double rotation)
        //{
        //    List<HatchBoundaryPath> boundary = new List<HatchBoundaryPath>();
        //    Matrix3 dataTrans = Matrix3.RotationZ(rotation * MathHelper.DegToRad) * Matrix3.Scale(scale);
        //    Vector3 localPos = MathHelper.Transform(insertPos, normal, MathHelper.CoordinateSystem.World, MathHelper.CoordinateSystem.Object);
        //    foreach (HatchBoundaryPath path in hatch.BoundaryPaths)
        //    {
        //        List<EntityObject> data = new List<EntityObject>();
        //        foreach (EntityObject entity in path.Data)
        //        {
        //            switch (entity.Type)
        //            {
        //                case (EntityType.Arc):
        //                    data.Add(ProcessArc((Arc)entity, trans, pos, scale, 0));
        //                    break;
        //                case (EntityType.Circle):
        //                    data.Add(ProcessCircle((Circle)entity, trans, pos, scale, 0));
        //                    break;
        //                case (EntityType.Ellipse):
        //                    data.Add(ProcessEllipse((Ellipse)entity, trans, pos, scale, 0));
        //                    break;
        //                case (EntityType.Line):
        //                    data.Add(ProcessLine((Line)entity, dataTrans, localPos));
        //                    break;
        //            }
        //        }

        //        boundary.Add(new HatchBoundaryPath(data));
        //    }

        //    // the insert scale will not modify the hatch pattern even thought AutoCad does
        //    Hatch copy = (Hatch)hatch.Clone();
        //    copy.BoundaryPaths = boundary;
        //    copy.Elevation = localPos.Z + hatch.Elevation;
        //    copy.Normal = trans * hatch.Normal;
        //    return copy;
        //}

        //private static Line ProcessLine(Line line, Matrix3 trans, Vector3 pos)
        //{
        //    Line copy = (Line)line.Clone();
        //    copy.StartPoint = trans * line.StartPoint + pos;
        //    copy.EndPoint = trans * line.EndPoint + pos;
        //    copy.Normal = trans * line.Normal;

        //    return copy;
        //}

        //#endregion

        #endregion
    }
}