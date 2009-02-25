using System;
using System.Collections.Generic;
using netDxf.Tables;


namespace netDxf.Entities
{
    public class Solid:
        IEntityObject
    {
        #region private fields

        private const string DXF_NAME = DxfEntityCode.Solid;
        private const EntityType TYPE = EntityType.Solid;
        private Vector3 firstVertex;
        private Vector3 secondVertex;
        private Vector3 thirdVertex;
        private Vector3 fourthVertex;
        private float thickness;
        private Vector3 normal;
        private Layer layer;
        private AciColor color;
        private LineType lineType;
        private readonly List<XData> xData;

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the Face3D class.
        /// </summary>
        public Solid(Vector3 firstVertex, Vector3 secondVertex, Vector3 thirdVertex, Vector3 fourthVertex)
        {
            this.firstVertex = firstVertex;
            this.secondVertex = secondVertex;
            this.thirdVertex = thirdVertex;
            this.fourthVertex = fourthVertex;
            this.thickness = 0.0f;
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();
        }

        /// <summary>
        /// Initializes a new instance of the Face3D class.
        /// </summary>
        public Solid()
        {
            this.firstVertex = Vector3.Zero;
            this.secondVertex = Vector3.Zero;
            this.thirdVertex = Vector3.Zero;
            this.fourthVertex = Vector3.Zero;
            this.thickness = 0.0f;
            this.normal = Vector3.UnitZ;
            this.layer = Layer.Default;
            this.color = AciColor.Bylayer;
            this.lineType = LineType.ByLayer;
            this.xData = new List<XData>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the first 3D face vertex.
        /// </summary>
        public Vector3 FirstVertex
        {
            get { return this.firstVertex; }
            set { this.firstVertex = value; }
        }

        /// <summary>
        /// Gets or sets the second 3D face vertex.
        /// </summary>
        public Vector3 SecondVertex
        {
            get { return this.secondVertex; }
            set { this.secondVertex = value; }
        }

        /// <summary>
        /// Gets or sets the third 3D face vertex.
        /// </summary>
        public Vector3 ThirdVertex
        {
            get { return this.thirdVertex; }
            set { this.thirdVertex = value; }
        }

        /// <summary>
        /// Gets or sets the fourth 3D face vertex.
        /// </summary>
        public Vector3 FourthVertex
        {
            get { return this.fourthVertex; }
            set { this.fourthVertex = value; }
        }

       public float Thickness
        {
            get { return this.thickness ; }
            set { this.thickness  = value; }
        }

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
        /// Gets the entity type.
        /// </summary>
        public EntityType Type
        {
            get { return TYPE; }
        }

        /// <summary>
        /// Gets or sets the entity color.
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
        /// Gets or sets the entity layer.
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
        /// Gets or sets the entity line type.
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
        /// Gets or sets the entity extended data.
        /// </summary>
        public List<XData> XData
        {
            get { return this.xData; }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return TYPE.ToString();
        }

        #endregion
    }
}
