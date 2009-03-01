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
using netDxf.Tables;

namespace netDxf.Entities
{
    /// <summary>
    /// Represents a nurbs curve <see cref="netDxf.Entities.IEntityObject">entity.
    /// </summary>
    public class NurbsCurve :
        IEntityObject
    {
        #region private fields

        private const EntityType TYPE = EntityType.NurbsCurve;
        private const string DXF_NAME = DxfEntityCode.Polyline;
        private AciColor color;
        private Layer layer;
        private LineType lineType;
        private readonly List<XData> xData;

        private readonly List<NurbsVertex> controlPoints;
        private float[] knotVector;
        private byte degree;
        private Vector3 normal;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>NurbsCurve</c> class.
        /// </summary>
        public NurbsCurve()
        {
            this.controlPoints = new List<NurbsVertex>();
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.ByLayer;
            this.lineType = LineType.ByLayer;
            this.degree = 3;
            this.normal = Vector3.UnitZ;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the nurbs curve <see cref="NurbsVertex">control point</see> list.
        /// </summary>
        public List<NurbsVertex> ControlPoints
        {
            get { return this.controlPoints; }
        }

        /// <summary>
        /// Gets or sets the nurbs curve degree.
        /// </summary>
        public byte Degree
        {
            get { return this.degree; }
            set { this.degree = value; }
        }

        /// <summary>
        /// Gets or sets the nurbs curve <see cref="netDxf.Vector3">normal</see>.
        /// </summary>
        public Vector3 Normal
        {
            get { return this.normal; }
            set
            {
                value.Normalize();
                this.normal = value;
            }
        }

        #endregion

        #region IEntityObject Members

        /// <summary>
        /// Gets the dxf code that represents the entity.
        /// </summary>
        public string DxfName
        {
            get { return DXF_NAME; }
        }

        /// <summary>
        /// Gets the entity <see cref="netDxf.Entities.EntityType">type</see>.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.AciColor">color</see>.
        /// </summary>
        public AciColor Color
        {
            get { return this.color; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.color = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.Layer">layer</see>.
        /// </summary>
        public Layer Layer
        {
            get { return this.layer; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.layer = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.Tables.LineType">line type</see>.
        /// </summary>
        public LineType LineType
        {
            get { return this.lineType; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.lineType = value;
            }
        }

        /// <summary>
        /// Gets or sets the entity <see cref="netDxf.XData">extende data</see>.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Obtains a list of vertexes that represent the nurbs curve.
        /// </summary>
        /// <param name="precision">Number of point to approximate the curve to a polyline.</param>
        /// <returns>The vertexes are expresed in object coordinate system.</returns>
        public List<Vector2> PolygonalVertexes(byte precision)
        {
            this.knotVector = this.SetKnotVector();
            float[][][] nurbsBasisFunctions = this.DefineBasisFunctions(precision);

            List<Vector2> vertexes = new List<Vector2>();

            for (int i = 0; i < precision; i++)
            {
                float x = 0.0f;
                float y = 0.0f;
                for (int ctrlPointIndex = 0; ctrlPointIndex < this.controlPoints.Count; ctrlPointIndex++)
                {
                    x += this.controlPoints[ctrlPointIndex].Location.X*nurbsBasisFunctions[i][ctrlPointIndex][this.degree - 1];
                    y += this.controlPoints[ctrlPointIndex].Location.Y*nurbsBasisFunctions[i][ctrlPointIndex][this.degree - 1];
                }

                vertexes.Add(new Vector2(x, y));
            }

            return vertexes;
        }

        /// <summary>
        /// Sets a constant weight for all the nurbs curve <see cref="NurbsVertex">vertex</see> list.
        /// </summary>
        /// <param name="weight">Nurbs vertex weight.</param>
        public void SetUniformWeights(float weight)
        {
            foreach (NurbsVertex v in this.controlPoints)
            {
                v.Weight = weight;
            }
        }

        #endregion

        #region private methods

        private float[][][] DefineBasisFunctions(byte precision)
        {
            float[][][] nurbsBasisFunctions;
            float[][][] basisFunctions;

            basisFunctions = new float[precision][][];

            nurbsBasisFunctions = new float[precision][][];

            for (int vertexIndex = 0; vertexIndex < precision; vertexIndex++)
            {
                basisFunctions[vertexIndex] = new float[this.controlPoints.Count + 1][];
                nurbsBasisFunctions[vertexIndex] = new float[this.controlPoints.Count + 1][];

                float t = vertexIndex/(float) (precision - 1);

                if (t == 1.0f) t = 1.0f - MathHelper.EpsilonF;

                for (int ctrlPointIndex = 0; ctrlPointIndex < this.controlPoints.Count + 1; ctrlPointIndex++)
                {
                    basisFunctions[vertexIndex][ctrlPointIndex] = new float[this.degree];
                    nurbsBasisFunctions[vertexIndex][ctrlPointIndex] = new float[this.degree];

                    if (t >= this.knotVector[ctrlPointIndex] && t < this.knotVector[ctrlPointIndex + 1])
                        basisFunctions[vertexIndex][ctrlPointIndex][0] = 1.0f;
                    else
                        basisFunctions[vertexIndex][ctrlPointIndex][0] = 0.0f;
                }
            }

            for (int order = 1; order < this.degree; order++)
            {
                for (int ctrlPointIndex = 0; ctrlPointIndex < this.controlPoints.Count; ctrlPointIndex++)
                {
                    for (int vertexIndex = 0; vertexIndex < precision; vertexIndex++)
                    {
                        float t = vertexIndex/(float) (precision - 1);

                        float Nikm1 = basisFunctions[vertexIndex][ctrlPointIndex][order - 1];
                        float Nip1km1 = basisFunctions[vertexIndex][ctrlPointIndex + 1][order - 1];

                        float xi = this.knotVector[ctrlPointIndex];
                        float xikm1 = this.knotVector[ctrlPointIndex + order - 1 + 1];
                        float xik = this.knotVector[ctrlPointIndex + order + 1];
                        float xip1 = this.knotVector[ctrlPointIndex + 1];

                        float FirstTermBasis;
                        if (Math.Abs(xikm1 - xi) < MathHelper.EpsilonF)
                            FirstTermBasis = 0.0f;
                        else
                            FirstTermBasis = ((t - xi)*Nikm1)/(xikm1 - xi);

                        float SecondTermBasis;
                        if (Math.Abs(xik - xip1) < MathHelper.EpsilonF)
                            SecondTermBasis = 0.0f;
                        else
                            SecondTermBasis = ((xik - t)*Nip1km1)/(xik - xip1);

                        basisFunctions[vertexIndex][ctrlPointIndex][order] = FirstTermBasis + SecondTermBasis;
                    }
                }
            }

            for (int Order = 1; Order < this.degree; Order++)
            {
                for (int ControlPoint = 0; ControlPoint < this.controlPoints.Count; ControlPoint++)
                {
                    for (int Vertex = 0; Vertex < precision; Vertex++)
                    {
                        float Denominator = 0.0f;
                        for (int ControlWeight = 0; ControlWeight < this.controlPoints.Count; ControlWeight++)
                        {
                            Denominator += this.controlPoints[ControlWeight].Weight*basisFunctions[Vertex][ControlWeight][Order];
                        }

                        nurbsBasisFunctions[Vertex][ControlPoint][Order] = this.controlPoints[ControlPoint].Weight*
                                                                           basisFunctions[Vertex][ControlPoint][Order]/
                                                                           Denominator;
                    }
                }
            }

            return nurbsBasisFunctions;
        }

        private float[] SetKnotVector()
        {
            //This code creates an open uniform knot vector
            float[] knots = new float[this.controlPoints.Count + this.degree];
            int knotValue = 0;
            for (int i = 0; i < this.degree + this.controlPoints.Count; i++)
            {
                if (i <= this.controlPoints.Count && i >= this.degree)
                    knotValue++;

                knots[i] = knotValue/(float) (this.controlPoints.Count - this.degree + 1);
            }
            return knots;
        }

        #endregion
    }
}