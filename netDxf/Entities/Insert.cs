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
using netDxf.Blocks;

namespace netDxf.Entities
{

    /// <summary>
    /// Represents a block insertion <see cref="EntityObject">entity</see>.
    /// </summary>
    public class Insert :
        EntityObject
    {
        #region private fields

        private readonly EndSequence endSequence = new EndSequence();
        private Block block;
        private Vector3 position;
        private Vector3 scale;
        private double rotation;
        private Vector3 normal;
        private readonly List<Attribute> attributes = new List<Attribute>();

        #endregion

        #region constructors

        internal Insert()
            : base (EntityType.Insert, DxfObjectCode.Insert)
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
            this.scale = new Vector3(1.0, 1.0, 1.0);
            this.rotation = 0.0f;
            this.normal = Vector3.UnitZ;
            foreach (AttributeDefinition attdef in block.Attributes.Values)
            {
                Attribute att = new Attribute(attdef)
                                    {
                                        Position = attdef.Position + this.position - this.block.Position
                                    };
                this.attributes.Add(att);
            }
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
        /// <param name="block">Insert <see cref="Block">block definition</see>.</param>
        public Insert(Block block)
            : this(block, Vector3.Zero)
        {
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the insert list of <see cref="Attribute">attributes</see>.
        /// </summary>
        public List<Attribute> Attributes
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
            set { this.rotation = value; }
        }

        /// <summary>
        /// Gets or sets the insert <see cref="Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                if (Vector3.Zero == value)
                    throw new ArgumentNullException("value", "The normal can not be the zero vector");
                value.Normalize();
                this.normal = value;
            }
        }

        internal EndSequence EndSequence
        {
            get { return this.endSequence; }
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
        /// independently to its definition, usually you will want that the position, rotation, height and/or width factor are transformed with the insert
        /// as is the behaviour inside AutoCad.<br />
        /// This method only applies to attributes that have a definition, some dxf files might generate attributes that have no definition in the block.
        /// </remarks>
        public void TransformAttributes()
        {
            foreach (Attribute att in this.attributes)
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
                att.Normal = this.normal;

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
        internal override int AsignHandle(int entityNumber)
        {
            entityNumber = this.endSequence.AsignHandle(entityNumber);
            foreach(Attribute attrib in this.attributes )
            {
                entityNumber = attrib.AsignHandle(entityNumber);
            }
            
            return base.AsignHandle(entityNumber);
        }

        #endregion
    }
}