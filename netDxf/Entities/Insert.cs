﻿#region netDxf library, Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2019 Daniel Carvajal (haplokuon@gmail.com)
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
using netDxf.Tables;
using netDxf.Units;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a block insertion <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Insert :
        EntityObject
    {
        #region delegates and events

        public delegate void AttributeAddedEventHandler(Insert sender, AttributeChangeEventArgs e);

        public event AttributeAddedEventHandler AttributeAdded;

        protected virtual void OnAttributeAddedEvent(Attribute item)
        {
            AttributeAddedEventHandler ae = this.AttributeAdded;
            if (ae != null)
                ae(this, new AttributeChangeEventArgs(item));
        }

        public delegate void AttributeRemovedEventHandler(Insert sender, AttributeChangeEventArgs e);

        public event AttributeRemovedEventHandler AttributeRemoved;

        protected virtual void OnAttributeRemovedEvent(Attribute item)
        {
            AttributeRemovedEventHandler ae = this.AttributeRemoved;
            if (ae != null)
                ae(this, new AttributeChangeEventArgs(item));
        }

        #endregion

        #region private fields

        private readonly EndSequence endSequence;
        private Block block;
        private Vector3 position;
        private Vector3 scale;
        private double rotation;
        private AttributeCollection attributes;
        private int columnCount=1;
        private int rowCount=1;
        private double columnSpacing;
        private double rowSpacing;

        #endregion

        #region constructors

        internal Insert(List<Attribute> attributes)
            : base(EntityType.Insert, DxfObjectCode.Insert)
        {
            if(attributes == null)
                throw new ArgumentNullException(nameof(attributes));
            this.attributes = new AttributeCollection(attributes);
            foreach (Attribute att in this.attributes)
            {
                if(att.Owner!=null)
                    throw new ArgumentException("The attributes list contains at least an attribute that already has an owner.", nameof(attributes));
                att.Owner = this;
            }

            this.block = null;
            this.position = Vector3.Zero;
            this.scale = new Vector3(1.0);
            this.rotation = 0.0;
            this.endSequence = new EndSequence(this);

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
            : this(block, position, 1.0)
        {           
        }

        /// <summary>
        /// Initializes a new instance of the <c>Insert</c> class.
        /// </summary>
        /// <param name="block">Insert block definition.</param>
        /// <param name="position">Insert <see cref="Vector3">point</see> in world coordinates.</param>
        public Insert(Block block, Vector3 position, double scale)
            : base(EntityType.Insert, DxfObjectCode.Insert)
        {
            if (block == null)
                throw new ArgumentNullException(nameof(block));

            this.block = block;
            this.position = position;
            if (scale <= 0)
                throw new ArgumentOutOfRangeException(nameof(scale), scale, "The Insert scale must be greater than zero.");
            this.scale = new Vector3(scale);
            this.rotation = 0.0;
            this.endSequence = new EndSequence(this);

            List<Attribute> atts = new List<Attribute>(block.AttributeDefinitions.Count);
            foreach (AttributeDefinition attdef in block.AttributeDefinitions.Values)
            {
                Attribute att = new Attribute(attdef)
                {
                    Position = attdef.Position + this.position - this.block.Origin,
                    Owner = this
                };
                atts.Add(att);
            }

            this.attributes = new AttributeCollection(atts);
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the default drawing units to obtain the Insert transformation matrix, when the current Insert entity does not belong to a DXF document.
        /// </summary>
        public static DrawingUnits DefaultInsUnits
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the insert list of <see cref="Attribute">attributes</see>.
        /// </summary>
        public AttributeCollection Attributes
        {
            get { return this.attributes; }
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
        /// <remarks>None of the vector scale components can be zero.</remarks>
        public Vector3 Scale
        {
            get { return this.scale; }
            set
            {
                if (MathHelper.IsZero(value.X) || MathHelper.IsZero(value.Y) || MathHelper.IsZero(value.Z))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "None of the vector scale components can be zero.");
                this.scale = value;
            }
        }

        /// <summary>
        /// Gets or sets the insert rotation along the normal vector in degrees.
        /// </summary>
        public double Rotation
        {
            get { return this.rotation; }
            set { this.rotation = MathHelper.NormalizeAngle(value); }
        }

        /// <summary>
        /// Gets or sets column count for multiple insert (default=1)
        /// </summary>
        public int ColumnCount
        {
            get { return this.columnCount; }
            set { this.columnCount = value; }
        }

        /// <summary>
        /// Gets or sets row count for multiple insert (default=1)
        /// </summary>
        public int RowCount
        {
            get { return this.rowCount; }
            set { this.rowCount = value; }
        }

        /// <summary>
        /// Gets or sets column spacing for multiple insert
        /// </summary>
        public double ColumnSpacing
        {
            get { return this.columnSpacing; }
            set { this.columnSpacing = value; }
        }

        /// <summary>
        /// Gets or sets row spacing for multiple insert
        /// </summary>
        public double RowSpacing
        {
            get { return this.rowSpacing; }
            set { this.rowSpacing = value; }
        }


        #endregion

        #region internal properties

        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Updates the actual insert with the attribute properties currently defined in the block. This does not affect any values assigned to attributes in each block.
        /// </summary>
        /// <remarks>This method will automatically call the TransformAttributes method, to keep all attributes position and orientation up to date.</remarks>
        /// <remarks></remarks>
        public void Sync()
        {
            List<Attribute> atts = new List<Attribute>(this.block.AttributeDefinitions.Count);

            // remove all attributes in the actual insert
            foreach (Attribute att in this.attributes)
            {
                this.OnAttributeRemovedEvent(att);
                att.Handle = null;
                att.Owner = null;
            }

            // add any new attributes from the attribute definitions of the block
            foreach (AttributeDefinition attdef in this.block.AttributeDefinitions.Values)
            {
                Attribute att = new Attribute(attdef)
                {
                    Owner = this
                };

                atts.Add(att);
                this.OnAttributeAddedEvent(att);
            }
            this.attributes = new AttributeCollection(atts);

            this.TransformAttributes();
        }

        /// <summary>
        /// Calculates the insertion rotation matrix.
        /// </summary>
        /// <returns>The insert transformation matrix.</returns>
        /// <remarks>
        /// This method uses the DefaultInsUnits defined by the Insert class to obtain the scale between the drawing and the block units.
        /// Additionally, if the insert belongs to a block the units to use are the ones defined in the BlockRecord,
        /// and if the insert belongs to a layout the units to use are the ones defined in the document drawing variable InsUnits.
        /// </remarks>
        public Matrix3 GetTransformation()
        {
            DrawingUnits insUnits;
            if (this.Owner == null)
                insUnits = DefaultInsUnits;
            else
                insUnits = this.Owner.Record.Layout == null ? this.Owner.Record.Units : this.Owner.Record.Owner.Owner.DrawingVariables.InsUnits;

            double docScale = UnitHelper.ConversionFactor(this.Block.Record.Units, insUnits);
            Matrix3 trans = MathHelper.ArbitraryAxis(this.Normal);
            trans *= Matrix3.RotationZ(this.rotation * MathHelper.DegToRad);
            trans *= Matrix3.Scale(this.scale * docScale);

            return trans;
        }

        /// <summary>
        /// Calculates the insertion rotation matrix.
        /// </summary>
        /// <param name="insertionUnits">The insertion units.</param>
        /// <returns>The insert transformation matrix.</returns>
        public Matrix3 GetTransformation(DrawingUnits insertionUnits)
        {
            double docScale = UnitHelper.ConversionFactor(this.Block.Record.Units, insertionUnits);
            Matrix3 trans = MathHelper.ArbitraryAxis(this.Normal);
            trans *= Matrix3.RotationZ(this.rotation * MathHelper.DegToRad);
            trans *= Matrix3.Scale(this.scale * docScale);

            return trans;
        }

        /// <summary>
        /// Recalculate the attributes position, normal, rotation, text height, width factor, and oblique angle from the values applied to the insertion.
        /// </summary>
        /// <remarks>
        /// Changes to the insert, the block, or the document insertion units will require this method to be called manually.<br />
        /// The attributes position, normal, rotation, text height, width factor, and oblique angle values includes the transformations applied to the insertion,
        /// if required this method will calculate the proper values according to the ones defined by the attribute definition.<br />
        /// All the attribute values can be changed manually independently to its definition,
        /// but, usually, you will want them to be transformed with the insert based on the local values defined by the attribute definition.<br />
        /// This method only applies to attributes that have a definition, some DXF files might generate attributes that have no definition in the block.<br />
        /// At the moment the attribute width factor and oblique angle are not calculated, this is applied to inserts with non uniform scaling.
        /// </remarks>
        public void TransformAttributes()
        {
            // if the insert does not contain attributes there is nothing to do
            if (this.attributes.Count == 0)
                return;

            Matrix3 transformation = this.GetTransformation();
            Vector3 translation = this.Position - transformation * this.block.Origin;

            foreach (Attribute att in this.attributes)
            {
                AttributeDefinition attdef = att.Definition;
                if (attdef == null)
                    continue;

                // reset the attribute to its default values
                att.Position = attdef.Position;
                att.Height = attdef.Height;
                att.Width = attdef.Width;
                att.WidthFactor = attdef.WidthFactor;
                att.ObliqueAngle = attdef.ObliqueAngle;
                att.Rotation = attdef.Rotation;
                att.Normal = attdef.Normal;
                att.IsBackward = attdef.IsBackward;
                att.IsUpsideDown = attdef.IsUpsideDown;

                att.TransformBy(transformation, translation);
            }
        }

        /// <summary>
        /// Explodes the current insert.
        /// </summary>
        /// <returns>A list of entities.</returns>
        public List<EntityObject> Explode()
        {
            List<EntityObject> entities = new List<EntityObject>();
            Matrix3 transformation = this.GetTransformation();
            Vector3 translation = this.Position - transformation * this.block.Origin;

            foreach (EntityObject entity in this.block.Entities)
            {
                Vector3 localScale = MathHelper.Transform(this.Scale, entity.Normal, CoordinateSystem.World, CoordinateSystem.Object);
                bool isUniformScale = MathHelper.IsEqual(localScale.X, localScale.Y);

                // entities with reactors are associated with other entities they will handle the transformation
                if (entity.Reactors.Count > 0)
                    continue;

                if(!isUniformScale)
                {
                    switch (entity.Type)
                    {
                        case EntityType.Circle:
                        {
                            Circle circle = (Circle) entity;
                            Ellipse ellipse = new Ellipse
                            {
                                //EntityObject properties
                                Layer = (Layer) entity.Layer.Clone(),
                                Linetype = (Linetype) entity.Linetype.Clone(),
                                Color = (AciColor) entity.Color.Clone(),
                                Lineweight = entity.Lineweight,
                                Transparency = (Transparency) entity.Transparency.Clone(),
                                LinetypeScale = entity.LinetypeScale,
                                Normal = entity.Normal,
                                IsVisible = entity.IsVisible,
                                //Ellipse properties
                                Center = circle.Center,
                                MajorAxis = 2 * circle.Radius,
                                MinorAxis = 2 * circle.Radius,
                                Thickness = circle.Thickness
                            };

                            ellipse.TransformBy(transformation, translation);
                            entities.Add(ellipse);
                            break;
                        }
                        case EntityType.Arc:
                        {
                            Arc arc = (Arc) entity;
                            Ellipse ellipse = new Ellipse
                            {
                                //EntityObject properties
                                Layer = (Layer) entity.Layer.Clone(),
                                Linetype = (Linetype) entity.Linetype.Clone(),
                                Color = (AciColor) entity.Color.Clone(),
                                Lineweight = entity.Lineweight,
                                Transparency = (Transparency) entity.Transparency.Clone(),
                                LinetypeScale = entity.LinetypeScale,
                                Normal = entity.Normal,
                                IsVisible = entity.IsVisible,
                                //Ellipse properties
                                Center = arc.Center,
                                MajorAxis = 2 * arc.Radius,
                                MinorAxis = 2 * arc.Radius,
                                StartAngle = arc.StartAngle,
                                EndAngle = arc.EndAngle,
                                Thickness = arc.Thickness
                            };

                            ellipse.TransformBy(transformation, translation);
                            entities.Add(ellipse);
                            break;
                        }
                        case EntityType.LwPolyline:
                        {
                            List<EntityObject> newEntities = ((LwPolyline) entity).Explode();
                            foreach (EntityObject newEntity in newEntities)
                            {
                                if (newEntity.Type == EntityType.Arc)
                                {
                                    Arc arc = (Arc) newEntity;
                                    Ellipse ellipse = new Ellipse
                                    {
                                        //EntityObject properties
                                        Layer = (Layer) entity.Layer.Clone(),
                                        Linetype = (Linetype) entity.Linetype.Clone(),
                                        Color = (AciColor) entity.Color.Clone(),
                                        Lineweight = entity.Lineweight,
                                        Transparency = (Transparency) entity.Transparency.Clone(),
                                        LinetypeScale = entity.LinetypeScale,
                                        Normal = entity.Normal,
                                        IsVisible = entity.IsVisible,
                                        //Ellipse properties
                                        Center = arc.Center,
                                        MajorAxis = 2 * arc.Radius,
                                        MinorAxis = 2 * arc.Radius,
                                        StartAngle = arc.StartAngle,
                                        EndAngle = arc.EndAngle,
                                        Thickness = arc.Thickness
                                    };

                                    ellipse.TransformBy(transformation, translation);
                                    entities.Add(ellipse);
                                }
                                else
                                {
                                    newEntity.TransformBy(transformation, translation);
                                    entities.Add(newEntity);
                                }                                
                            }
                            break;
                        }
                        case EntityType.MLine:
                        {
                            List<EntityObject> newEntities = ((MLine)entity).Explode();
                            foreach (EntityObject newEntity in newEntities)
                            {
                                if (newEntity.Type == EntityType.Arc)
                                {
                                    Arc arc = (Arc) newEntity;
                                    Ellipse ellipse = new Ellipse
                                    {
                                        //EntityObject properties
                                        Layer = (Layer) entity.Layer.Clone(),
                                        Linetype = (Linetype) entity.Linetype.Clone(),
                                        Color = (AciColor) entity.Color.Clone(),
                                        Lineweight = entity.Lineweight,
                                        Transparency = (Transparency) entity.Transparency.Clone(),
                                        LinetypeScale = entity.LinetypeScale,
                                        Normal = entity.Normal,
                                        IsVisible = entity.IsVisible,
                                        //Ellipse properties
                                        Center = arc.Center,
                                        MajorAxis = 2 * arc.Radius,
                                        MinorAxis = 2 * arc.Radius,
                                        StartAngle = arc.StartAngle,
                                        EndAngle = arc.EndAngle,
                                        Thickness = arc.Thickness
                                    };

                                    ellipse.TransformBy(transformation, translation);
                                    entities.Add(ellipse);
                                }
                                else
                                {
                                    newEntity.TransformBy(transformation, translation);
                                    entities.Add(newEntity);
                                }                             
                            }
                            break;
                        }
                        default:
                        {
                            EntityObject newEntity = (EntityObject) entity.Clone();
                            newEntity.TransformBy(transformation, translation);
                            entities.Add(newEntity);
                            break;
                        }
                    }
                }
                else
                {
                    EntityObject newEntity = (EntityObject) entity.Clone();
                    newEntity.TransformBy(transformation, translation);
                    entities.Add(newEntity);
                }
            }

            foreach (Attribute attribute in this.attributes)
            {
                // the attributes will be exploded as a Text entity
                Text text = new Text
                {
                    //Attribute properties
                    Layer = (Layer) attribute.Layer.Clone(),
                    Linetype = (Linetype) attribute.Linetype.Clone(),
                    Color = (AciColor) attribute.Color.Clone(),
                    Lineweight = attribute.Lineweight,
                    Transparency = (Transparency) attribute.Transparency.Clone(),
                    LinetypeScale = attribute.LinetypeScale,
                    Normal = attribute.Normal,
                    IsVisible = attribute.IsVisible,
                    Height = attribute.Height,
                    WidthFactor = attribute.WidthFactor,
                    ObliqueAngle = attribute.ObliqueAngle,
                    Value = attribute.Value.ToString(),
                    Style = (TextStyle) attribute.Style.Clone(),
                    Position = attribute.Position,
                    Rotation = attribute.Rotation,
                    Alignment = attribute.Alignment,
                    IsBackward = attribute.IsBackward,
                    IsUpsideDown = attribute.IsUpsideDown
                };
                entities.Add(text);
            }

            return entities;
        }

        #endregion

        #region overrides

        /// <summary>
        /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
        /// </summary>
        /// <param name="transformation">Transformation matrix.</param>
        /// <param name="translation">Translation vector.</param>
        /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
        public override void TransformBy(Matrix3 transformation, Vector3 translation)
        {
            Vector3 newPosition = transformation * this.Position + translation;
            Vector3 newNormal = transformation * this.Normal;
            if (Vector3.Equals(Vector3.Zero, newNormal)) newNormal = this.Normal;

            Matrix3 transOW = MathHelper.ArbitraryAxis(this.Normal);
            transOW *= Matrix3.RotationZ(this.Rotation * MathHelper.DegToRad);

            Matrix3 transWO = MathHelper.ArbitraryAxis(newNormal);
            transWO = transWO.Transpose();

            Vector3 v = transOW * Vector3.UnitX;
            v = transformation * v;
            v = transWO * v;
            double newRotation = Vector2.Angle(new Vector2(v.X, v.Y))* MathHelper.RadToDeg;

            transWO = Matrix3.RotationZ(newRotation * MathHelper.DegToRad).Transpose() * transWO;

            Vector3 s = transOW * this.Scale;
            s = transformation * s;
            s = transWO * s;
            Vector3 newScale = new Vector3(
                MathHelper.IsZero(s.X) ? MathHelper.Epsilon : s.X,
                MathHelper.IsZero(s.Y) ? MathHelper.Epsilon : s.Y,
                MathHelper.IsZero(s.Z) ? MathHelper.Epsilon : s.Z);

            this.Normal = newNormal;
            this.Position = newPosition;
            this.Scale = newScale;
            this.Rotation = newRotation;

            this.TransformAttributes();
        }

        /// <summary>
        /// Assigns a handle to the object based in a integer counter.
        /// </summary>
        /// <param name="entityNumber">Number to assign.</param>
        /// <returns>Next available entity number.</returns>
        /// <remarks>
        /// Some objects might consume more than one, is, for example, the case of polylines that will assign
        /// automatically a handle to its vertexes. The entity number will be converted to an hexadecimal number.
        /// </remarks>
        internal override long AssignHandle(long entityNumber)
        {
            entityNumber = this.endSequence.AssignHandle(entityNumber);
            foreach (Attribute attrib in this.attributes)
            {
                entityNumber = attrib.AssignHandle(entityNumber);
            }
            return base.AssignHandle(entityNumber);
        }


        /// <summary>
        /// Creates a new Insert that is a copy of the current instance.
        /// </summary>
        /// <returns>A new Insert that is a copy of this instance.</returns>
        public override object Clone()
        {
            // copy attributes
            List<Attribute> copyAttributes = new List<Attribute>();
            foreach (Attribute att in this.attributes)
                copyAttributes.Add((Attribute)att.Clone());

            Insert entity = new Insert(copyAttributes)
            {
                //EntityObject properties
                Layer = (Layer) this.Layer.Clone(),
                Linetype = (Linetype) this.Linetype.Clone(),
                Color = (AciColor) this.Color.Clone(),
                Lineweight = this.Lineweight,
                Transparency = (Transparency) this.Transparency.Clone(),
                LinetypeScale = this.LinetypeScale,
                Normal = this.Normal,
                IsVisible = this.IsVisible,
                //Insert properties
                Position = this.position,
                Block = (Block) this.block.Clone(),
                Scale = this.scale,
                Rotation = this.rotation,
            };

            // copy extended data
            foreach (XData data in this.XData.Values)
                entity.XData.Add((XData) data.Clone());

            return entity;
        }

        #endregion
    }
}