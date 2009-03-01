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
using System.IO;
using System.Text;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Tables;
using Attribute=netDxf.Entities.Attribute;

namespace netDxf
{

   /// <summary>
    /// Low level dxf reader
    /// </summary>
    internal sealed class DxfReader
    {
        #region private fields

        private readonly string file;
        private int fileLine;
        private bool isFileOpen;
        private Stream input;
        private StreamReader reader;

        //header
       private DxfVersion version;

        //entities
        private List<Arc> arcs;
        private List<Circle> circles;
        private List<Point> points;
        private List<Face3D> faces3d;
       private List<Solid> solids;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<IPolyline> polylines;
        private List<Text> texts;

        //tables
        private List<string> appIds;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;

        //blocks
        private Dictionary<string, Block> blocks;

        #endregion

        #region constructors

        public DxfReader(string file)
        {
            this.file = file;
        }

        #endregion

        #region public properties

        public bool IsFileOpen
        {
            get { return this.isFileOpen; }
        }

        #endregion

        #region public header properties

       public DxfVersion Version
       {
           get{ return this.version;}
       }

        #endregion

        #region public entity properties

        public List<Arc> Arcs
        {
            get { return this.arcs; }
        }

        public List<Circle> Circles
        {
            get { return this.circles; }
        }

        public List<Point> Points
        {
            get { return this.points; }
        }

        public List<Face3D> Faces3d
        {
            get { return this.faces3d; }
        }

        public List<Solid> Solids
        {
            get { return this.solids; }
        }

        public List<Line> Lines
        {
            get { return this.lines; }
        }

        public List<IPolyline> Polylines
        {
            get { return this.polylines; }
        }

        public List<Insert> Inserts
        {
            get { return this.inserts; }
        }

        public List<Text> Texts
        {
            get { return this.texts; }
        }

        #endregion

        #region public table properties

        public List<string> ApplicationRegistrationIds
        {
            get { return this.appIds; }
        }

        public Dictionary<string, Layer> Layers
        {
            get { return this.layers; }
        }

        public Dictionary<string, LineType> LineTypes
        {
            get { return this.lineTypes; }
        }

        public Dictionary<string, TextStyle> TextStyles
        {
            get { return this.textStyles; }
        }

        public Dictionary<string, Block> Blocks
        {
            get { return this.blocks; }
        }

        #endregion

        #region public methods

        public void Open()
        {
            try
            {
                this.input = File.OpenRead(this.file);
                this.reader = new StreamReader(this.input, Encoding.ASCII);
                this.isFileOpen = true;
            }
            catch (Exception ex)
            {
                throw (new DxfException(this.file, "Error al intentar abrir el archivo.", ex));
            }
        }

        public void Close()
        {
            if (this.isFileOpen)
            {
                this.reader.Close();
                this.input.Close();
            }
            this.isFileOpen = false;
        }

        public void Read()
        {
            this.appIds = new List<string>();
            this.layers = new Dictionary<string, Layer>();
            this.lineTypes = new Dictionary<string, LineType>();
            this.textStyles = new Dictionary<string, TextStyle>();
            this.blocks = new Dictionary<string, Block>();

            this.arcs = new List<Arc>();
            this.circles = new List<Circle>();
            this.faces3d = new List<Face3D>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.lines = new List<Line>();
            this.polylines = new List<IPolyline>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
            this.fileLine = -1;

            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndOfFile)
            {
                if (code.Value == StringCode.BeginSection)
                {
                    code = this.ReadCodePair();
                    switch (code.Value)
                    {
                        case StringCode.HeaderSection:
                            this.ReadHeader();
                            break;
                        case StringCode.ClassesSection:
                            this.ReadClasses();
                            break;
                        case StringCode.TablesSection:
                            this.ReadTables();
                            break;
                        case StringCode.BlocksSection:
                            this.ReadBlocks();
                            break;
                        case StringCode.EntitiesSection:
                            this.ReadEntities();
                            break;
                        case StringCode.ObjectsSection:
                            this.ReadObjects();
                            break;
                        default:
                            throw new InvalidDxfSectionException(code.Value, this.file, "Unknown section " + code.Value + " line " + this.fileLine);
                    }
                }
                code = this.ReadCodePair();
            }

        }

        #endregion

        #region sections methods

        private void ReadHeader()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                if (HeaderVariable.Allowed.ContainsKey(code.Value))
                {
                    int codeGroup = HeaderVariable.Allowed[code.Value];
                    string variableName = code.Value;
                    code = this.ReadCodePair();
                    if (code.Code!=codeGroup)
                        throw new DxfHeaderVariableException(variableName,this.file, "Invalid variable name and code group convination in line " + this.fileLine );
                    switch (variableName)
                    {
                        case SystemVariable.DabaseVersion :
                            this.version = (DxfVersion) StringEnum.Parse(typeof(DxfVersion), code.Value);
                            break;
                    }
                }
                code = this.ReadCodePair();
            }
        }

        private void ReadClasses()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                code = this.ReadCodePair();
            }
        }

        private void ReadTables()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                code = this.ReadCodePair();
                switch (code.Value)
                {
                    case StringCode.ApplicationIDTable:
                        Debug.Assert(code.Code == 2);
                        this.ReadApplicationsId();
                        break;
                    case StringCode.LayerTable:
                        Debug.Assert(code.Code == 2);
                        this.ReadLayers();
                        break;
                    case StringCode.LineTypeTable:
                        Debug.Assert(code.Code == 2);
                        this.ReadLineTypes();
                        break;
                    case StringCode.TextStyleTable:
                        Debug.Assert(code.Code == 2);
                        this.ReadTextStyles();
                        break;
                }
            }
        }

        private void ReadApplicationsId()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndTable)
            {
                if (code.Value == StringCode.ApplicationIDTable)
                {
                    Debug.Assert(code.Code == 0);
                    string appId = this.ReadApplicationId(ref code);
                    this.appIds.Add(appId);
                }
                else
                {
                    code = this.ReadCodePair();
                }
            }
        }

        private string ReadApplicationId(ref CodeValuePair code)
        {
            string appId = string.Empty;

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        appId = code.Value;
                        break;
                }
                code = this.ReadCodePair();
            }

            return appId;
        }

        private void ReadBlocks()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                switch (code.Value)
                {
                    case StringCode.BegionBlock:
                        Block block = this.ReadBlock();
                        this.blocks.Add(block.Name, block);
                        break;
                }
                code = this.ReadCodePair();
            }
        }

        private Block ReadBlock()
        {
            CodeValuePair code = this.ReadCodePair();
            Layer layer = null;
            string name = string.Empty;
            Vector3 basePoint = Vector3.Zero;
            List<IEntityObject> entities = new List<IEntityObject>();
            Dictionary<string, AttributeDefinition> attdefs = new Dictionary<string, AttributeDefinition>();


            while (code.Value != StringCode.EndBlock)
            {
                switch (code.Code)
                {
                    case 8:
                        layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 2:
                        name = code.Value;
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 3:
                        name = code.Value;
                        code = this.ReadCodePair();
                        break;
                    case 0: // entity
                        IEntityObject entity;
                        entity = this.ReadBlockEntity(ref code);
                        if (entity != null)
                            if (entity.Type == EntityType.AttributeDefinition)
                                attdefs.Add(((AttributeDefinition) entity).Id, (AttributeDefinition) entity);
                            else
                                entities.Add(entity);
                        break;
                    default:
                        code = this.ReadCodePair();
                        break;
                }
            }
            return new Block(name)
                       {
                           BasePoint = basePoint,
                           Layer = layer,
                           Entities = entities,
                           Attributes = attdefs
                       };
        }

        private IEntityObject ReadBlockEntity(ref CodeValuePair code)
        {
            IEntityObject entity = null;

            switch (code.Value)
            {
                case DxfEntityCode.Arc:
                    entity = this.ReadArc(ref code);
                    break;
                case DxfEntityCode.Circle:
                    entity = this.ReadCircle(ref code);
                    break;
                case DxfEntityCode.Face3D:
                    entity = this.ReadFace3D(ref code);
                    break;
                case DxfEntityCode.Solid:
                    entity = this.ReadSolid(ref code);
                    break;
                case DxfEntityCode.Insert:
                    code = this.ReadCodePair();
                    break;
                case DxfEntityCode.Line:
                    entity = this.ReadLine(ref code);
                    break;
                case DxfEntityCode.LwPolyline:
                    entity = this.ReadLWPolyline(ref code);
                    break;
                case DxfEntityCode.Polyline:
                    entity = this.ReadPolyline(ref code);
                    break;
                case DxfEntityCode.Text:
                    entity = this.ReadText(ref code);
                    break;
                case DxfEntityCode.AttributeDefinition:
                    entity = this.ReadAttributeDefinition(ref code);
                    break;
                default:
                    code = this.ReadCodePair();
                    break;
            }

            return entity;
        }

        private AttributeDefinition ReadAttributeDefinition(ref CodeValuePair code)
        {
            string id = string.Empty;
            string text = string.Empty;
            object value = null;
            AttributeFlags flags = AttributeFlags.Visible;
            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            TextStyle style = TextStyle.Default;
            float height = 0;
            float widthFactor = 0;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            float rotation = 0;
            Vector3 normal = Vector3.UnitZ;

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        id = code.Value;
                        break;
                    case 3:
                        text = code.Value;
                        break;
                    case 1:
                        value = code.Value;
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(code.Value);
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(code.Value));
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(code.Value);
                        break;
                    case 70:
                        flags = (AttributeFlags) int.Parse(code.Value);
                        break;
                    case 10:
                        firstAlignmentPoint.X = float.Parse(code.Value);
                        break;
                    case 20:
                        firstAlignmentPoint.Y = float.Parse(code.Value);
                        break;
                    case 30:
                        firstAlignmentPoint.Z = float.Parse(code.Value);
                        break;
                    case 11:
                        secondAlignmentPoint.X = float.Parse(code.Value);
                        break;
                    case 21:
                        secondAlignmentPoint.Y = float.Parse(code.Value);
                        break;
                    case 31:
                        secondAlignmentPoint.Z = float.Parse(code.Value);
                        break;
                    case 7:
                        style = this.GetTextStyle(code.Value);
                        break;
                    case 40:
                        height = float.Parse(code.Value);
                        break;
                    case 41:
                        widthFactor = float.Parse(code.Value);
                        break;
                    case 50:
                        rotation = float.Parse(code.Value);
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(code.Value);
                        break;
                    case 74:
                        verticalAlignment = int.Parse(code.Value);
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        break;
                }

                code = this.ReadCodePair();
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            AttributeDefinition attdef = new AttributeDefinition(id)
                                             {
                                                 BasePoint = (alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint),
                                                 Normal = normal,
                                                 Alignment = alignment,
                                                 Text = text,
                                                 Value = value,
                                                 Flags = flags,
                                                 Layer = layer,
                                                 Color = color,
                                                 LineType = lineType,
                                                 Style = style,
                                                 Height = height,
                                                 WidthFactor = widthFactor,
                                                 Rotation = rotation
                                             };

            return attdef;
        }

        private Attribute ReadAttribute(Block block, ref CodeValuePair code)
        {
            AttributeDefinition attdef = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Object value = null;
            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        attdef = block.Attributes[code.Value];
                        break;
                    case 1:
                        value = code.Value;
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(code.Value);
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(code.Value));
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(code.Value);
                        break;
                }
                code = this.ReadCodePair();
            }

            return new Attribute(attdef)
                       {
                           Color = color,
                           Layer = layer,
                           LineType = lineType,
                           Value = value
                       };
        }

        private void ReadEntities()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                IEntityObject entity;
                switch (code.Value)
                {
                    case DxfEntityCode.Arc:
                        entity = this.ReadArc(ref code);
                        this.arcs.Add((Arc) entity);
                        break;
                    case DxfEntityCode.Circle:
                        entity = this.ReadCircle(ref code);
                        this.circles.Add((Circle) entity);
                        break;
                    case DxfEntityCode.Point:
                        entity = this.ReadPoint(ref code);
                        this.points.Add((Point) entity);
                        break;
                    case DxfEntityCode.Face3D:
                        entity = this.ReadFace3D(ref code);
                        this.faces3d.Add((Face3D) entity);
                        break;
                    case DxfEntityCode.Solid:
                        entity = this.ReadSolid(ref code);
                        this.solids.Add((Solid)entity);
                        break;
                    case DxfEntityCode.Insert:
                        entity = this.ReadInsert(ref code);
                        this.inserts.Add((Insert) entity);
                        break;
                    case DxfEntityCode.Line:
                        entity = this.ReadLine(ref code);
                        this.lines.Add((Line) entity);
                        break;
                    case DxfEntityCode.LwPolyline:
                        entity = this.ReadLWPolyline(ref code);
                        this.polylines.Add((IPolyline) entity);
                        break;
                    case DxfEntityCode.Polyline:
                        entity = this.ReadPolyline(ref code);
                        this.polylines.Add((IPolyline) entity);
                        break;
                    case DxfEntityCode.Text:
                        entity = this.ReadText(ref code);
                        this.texts.Add((Text) entity);
                        break;
                    default:
                        code = this.ReadCodePair();
                        break;
                }
            }
        }

        private void ReadObjects()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndSection)
            {
                code = this.ReadCodePair();
            }
        }

        #endregion

        #region layer methods

        private void ReadLayers()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndTable)
            {
                if (code.Value == StringCode.LayerTable)
                {
                    Debug.Assert(code.Code == 0);
                    Layer capa = this.ReadLayer(ref code);
                    this.layers.Add(capa.Name, capa);
                }
                else
                {
                    code = this.ReadCodePair();
                }
            }
        }

        private Layer ReadLayer(ref CodeValuePair code)
        {
            string name = string.Empty;
            bool isVisible = true;
            AciColor color = null;
            LineType lineType = null;

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        name = code.Value;
                        break;
                    case 62:
                        short index;
                        if (short.TryParse(code.Value, out index))
                        {
                            if (index < 0)
                            {
                                isVisible = false;
                                index = Math.Abs(index);
                            }
                            if (index > 256)
                            {
                                throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                             "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                            }
                        }
                        else
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }

                        color = new AciColor(short.Parse(code.Value));
                        break;
                    case 6:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        lineType = this.GetLineType(code.Value);
                        break;
                }


                code = this.ReadCodePair();
            }

            return new Layer(name)
                       {
                           Color = color,
                           LineType = lineType,
                           IsVisible = isVisible
                       };
        }

        #endregion

        #region line type methods

        private void ReadLineTypes()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndTable)
            {
                if (code.Value == StringCode.LineTypeTable)
                {
                    Debug.Assert(code.Code == 0); //el código 0 indica el inicio de una nueva capa
                    LineType tl = this.ReadLineType(ref code);
                    this.lineTypes.Add(tl.Name, tl);
                }
                else
                {
                    code = this.ReadCodePair();
                }
            }
        }

        private LineType ReadLineType(ref CodeValuePair code)
        {
            string name = string.Empty;
            string description = string.Empty;
            var segments = new List<float>();

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        name = code.Value;
                        break;
                    case 3: //descripción del tipo de línea
                        description = code.Value;
                        break;
                    case 73:
                        //number of segments (not needed)
                        break;
                    case 40:
                        //length of the line type segments (not needed)
                        break;
                    case 49:
                        segments.Add(float.Parse(code.Value));
                        break;
                }
                code = this.ReadCodePair();
            }

            return new LineType(name)
                       {
                           Description = description,
                           Segments = segments
                       };
        }

        #endregion

        #region text style methods

        private void ReadTextStyles()
        {
            CodeValuePair code = this.ReadCodePair();
            while (code.Value != StringCode.EndTable)
            {
                if (code.Value == StringCode.TextStyleTable)
                {
                    Debug.Assert(code.Code == 0); //el código 0 indica el inicio de una nueva capa
                    TextStyle style = this.ReadTextStyle(ref code);
                    this.textStyles.Add(style.Name, style);
                }
                else
                {
                    code = this.ReadCodePair();
                }
            }
        }

        private TextStyle ReadTextStyle(ref CodeValuePair code)
        {
            string name = string.Empty;
            string font = string.Empty;
            bool isVertical = false;
            bool isBackward = false;
            bool isUpsideDown = false;
            float height = 0.0f;
            float widthFactor = 0.0f;
            float obliqueAngle = 0.0f;

            code = this.ReadCodePair();

            //leer los datos mientras no encontramos el código 0 que indicaría el final de la capa
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        name = code.Value;
                        break;
                    case 3:
                        if (string.IsNullOrEmpty(code.Value))
                        {
                            throw new InvalidCodeValueEntityDxfException(code, this.file,
                                                                         "Invalid value " + code.Value + " in code " + code.Code + " line " + this.fileLine);
                        }
                        font = code.Value;
                        break;

                    case 70:
                        if (int.Parse(code.Value) == 4)
                        {
                            isVertical = true;
                        }
                        break;
                    case 71:
                        //orientación texto (normal)
                        if (int.Parse(code.Value) == 6)
                        {
                            isBackward = true;
                            isUpsideDown = true;
                        }
                        else if (int.Parse(code.Value) == 2)
                        {
                            isBackward = true;
                        }
                        else if (int.Parse(code.Value) == 4)
                        {
                            isUpsideDown = true;
                        }
                        break;
                    case 40:
                        height = float.Parse(code.Value);
                        break;
                    case 41:
                        widthFactor = float.Parse(code.Value);
                        break;
                    case 42:
                        //last text height used (not aplicable)
                        break;
                    case 50:
                        obliqueAngle = (float.Parse(code.Value));
                        break;
                }
                code = this.ReadCodePair();
            }

            return new TextStyle(name, font)
                       {
                           Height = height,
                           IsBackward = isBackward,
                           IsUpsideDown = isUpsideDown,
                           IsVertical = isVertical,
                           ObliqueAngle = obliqueAngle,
                           WidthFactor = widthFactor
                       };
        }

        #endregion

        #region entity methods

        private Arc ReadArc(ref CodeValuePair code)
        {
            var arc = new Arc();
            Vector3 center = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();
            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        arc.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        arc.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        arc.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        center.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 40:
                        arc.Radius = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 50:
                        arc.StartAngle = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 51:
                        arc.EndAngle = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        arc.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);
                        code = this.ReadCodePair();
                        break;
                }
            }

            arc.XData.AddRange(xData);
            arc.Center = center;
            arc.Normal = normal;
            return arc;
        }

        private Circle ReadCircle(ref CodeValuePair code)
        {
            var circle = new Circle();
            Vector3 center = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        circle.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        circle.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        circle.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        center.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        center.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        center.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 40:
                        circle.Radius = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        circle.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);
                        code = this.ReadCodePair();
                        break;
                }
            }

            circle.XData.AddRange(xData);
            circle.Center = center;
            circle.Normal = normal;
            return circle;
        }

        private Point ReadPoint(ref CodeValuePair code)
        {
            var point = new Point();
            Vector3 location = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        point.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        point.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        point.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        point.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);
                        code = this.ReadCodePair();
                        break;
                }
            }

            point.XData.AddRange(xData);
            point.Location = location;
            point.Normal = normal;
            return point;
        }

        private Face3D ReadFace3D(ref CodeValuePair code)
        {
            var face = new Face3D();
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        face.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        face.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        face.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        v0.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 70:
                        face.EdgeFlags = (EdgeFlags) (int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            face.FirstVertex = v0;
            face.SecondVertex = v1;
            face.ThirdVertex = v2;
            face.FourthVertex = v3;
            face.XData.AddRange(xData);
            return face;
        }

        private Solid ReadSolid(ref CodeValuePair code)
        {
            var solid = new Solid();
            Vector3 v0 = Vector3.Zero;
            Vector3 v1 = Vector3.Zero;
            Vector3 v2 = Vector3.Zero;
            Vector3 v3 = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        solid.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        solid.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        solid.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        v0.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        v0.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        v0.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 11:
                        v1.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 21:
                        v1.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 31:
                        v1.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 12:
                        v2.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 22:
                        v2.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 32:
                        v2.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 13:
                        v3.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 23:
                        v3.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 33:
                        v3.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 70:
                        solid.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            solid.FirstVertex = v0;
            solid.SecondVertex = v1;
            solid.ThirdVertex = v2;
            solid.FourthVertex = v3;
            solid.Normal = normal;
            solid.XData.AddRange(xData);
            return solid;
        }

        private Insert ReadInsert(ref CodeValuePair code)
        {
            Vector3 basePoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            Vector3 scale = new Vector3(1, 1, 1);
            float rotation = 0.0f;
            Block block = null;
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            List<Attribute> attributes = new List<Attribute>();
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 2:
                        block = this.GetBlock(code.Value);
                        if (block == null)
                            throw new EntityDxfException(DxfEntityCode.Insert, this.file, "Block " + code.Value + " not defined line " + this.fileLine);
                        code = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        lineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        basePoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        basePoint.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        basePoint.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 41:
                        scale.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 42:
                        scale.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 43:
                        scale.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 50:
                        rotation = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }

                if (code.Value == DxfEntityCode.Attribute)
                {
                    while (code.Value != StringCode.EndSequence)
                    {
                        if (code.Value == DxfEntityCode.Attribute)
                        {
                            Debug.Assert(code.Code == 0);
                            Attribute attribute = this.ReadAttribute(block, ref code);
                            attributes.Add(attribute);
                        }
                    }
                    code = this.ReadCodePair();
                }
            }

            Insert insert = new Insert(block)
                                {
                                    Color = color,
                                    Layer = layer,
                                    LineType = lineType,
                                    InsertionPoint = basePoint,
                                    Rotation = rotation,
                                    Scale = scale,
                                    Normal = normal,
                                };

            insert.Attributes.Clear();
            insert.Attributes.AddRange(attributes);
            insert.XData.AddRange(xData);

            return insert;
        }

        private Line ReadLine(ref CodeValuePair code)
        {
            var line = new Line();
            Vector3 start = Vector3.Zero;
            Vector3 end = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8: //layer code
                        line.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        line.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        line.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        start.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        start.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        start.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 11:
                        end.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 21:
                        end.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 31:
                        end.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        line.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            line.StartPoint = start;
            line.EndPoint = end;
            line.Normal = normal;
            line.XData.AddRange(xData);

            return line;
        }

        private Polyline ReadLWPolyline(ref CodeValuePair code)
        {
            var pol = new Polyline();
            //int numVertexes;
            float constantWidth = 0.0F;
            PolylineVertex v = new PolylineVertex();
            float vX = 0.0f;
            Vector3 normal = Vector3.UnitZ;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8:
                        pol.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62:
                        pol.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6:
                        pol.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 38:
                        pol.Elevation = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        pol.Thickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 43:
                        constantWidth = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 70:
                        if (int.Parse(code.Value) == 0)
                        {
                            pol.IsClosed = false;
                        }
                        else if (int.Parse(code.Value) == 1)
                        {
                            pol.IsClosed = true;
                        }
                        code = this.ReadCodePair();
                        break;
                    case 90:
                        //numVertexes = int.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        v = new PolylineVertex
                                {
                                    BeginThickness = constantWidth,
                                    EndThickness = constantWidth
                                };
                        vX = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        float vY = float.Parse(code.Value);
                        v.Location = new Vector2(vX, vY);
                        pol.Vertexes.Add(v);
                        code = this.ReadCodePair();
                        break;
                    case 40:
                        v.BeginThickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 41:
                        v.EndThickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 42:
                        v.Bulge = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            pol.Normal = normal;
            pol.XData.AddRange(xData);

            return pol;
        }

        private IPolyline ReadPolyline(ref CodeValuePair code)
        {
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            PolylineTypeFlags flags = PolylineTypeFlags.OpenPolyline;
            float elevation = 0.0f;
            float thickness = 0.0f;
            Vector3 normal = Vector3.UnitZ;
            List<Vertex> vertexes = new List<Vertex>();
            List<XData> xData = new List<XData>();
            //int numVertexes = -1;
            //int numFaces = -1;

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8:
                        layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        elevation = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 39:
                        thickness  = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (PolylineTypeFlags) (int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 71:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numVertexes = int.Parse(code.Value); code = this.ReadCodePair();
                        code = this.ReadCodePair();
                        break;
                    case 72:
                        //this field might not exist for polyface meshes, we cannot depend on it
                        //numFaces  = int.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }


                //begin to read the vertex list
                if (code.Value == DxfEntityCode.Vertex)
                {
                    while (code.Value != StringCode.EndSequence)
                    {
                        if (code.Value == DxfEntityCode.Vertex)
                        {
                            Debug.Assert(code.Code == 0);
                            Vertex vertex = this.ReadVertex(ref code);
                            vertexes.Add(vertex);
                        }
                    }
                    code = this.ReadCodePair();
                }
            }

            IPolyline pol;
            bool isClosed = false;

            if ((flags & PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM) == PolylineTypeFlags.ClosedPolylineOrClosedPolygonMeshInM)
            {
                isClosed = true;
            }

            //to avoid possible error between the vertex type and the polyline type
            //the polyline type will decide which information to use from the read vertex
            if ((flags & PolylineTypeFlags.Polyline3D) == PolylineTypeFlags.Polyline3D)
            {
                List<Polyline3dVertex> polyline3dVertexes = new List<Polyline3dVertex>();
                foreach (Vertex v in vertexes)
                {
                    Polyline3dVertex vertex = new Polyline3dVertex
                                                  {
                                                      Color = v.Color,
                                                      Layer = v.Layer,
                                                      LineType = v.LineType,
                                                      Location = v.Location
                                                  };
                    vertex.XData.AddRange(v.XData);
                    polyline3dVertexes.Add(vertex);
                }

                ////posible error avoidance, the polyline is marked as polyline3d code:(70,8) but the vertex is marked as PolylineVertex code:(70,0)
                //if (v.Type == EntityType.PolylineVertex)
                //{
                //    Polyline3dVertex polyline3dVertex = new Polyline3dVertex(((PolylineVertex)v).Location.X, ((PolylineVertex)v).Location.Y,0);
                //    polyline3dVertexes.Add(polyline3dVertex);
                //}
                //else
                //{
                //    polyline3dVertexes.Add((Polyline3dVertex)v);
                //}
                //}
                pol = new Polyline3d(polyline3dVertexes, isClosed);
            }
            else if ((flags & PolylineTypeFlags.PolyfaceMesh) == PolylineTypeFlags.PolyfaceMesh)
            {
                //the vertex list created contains vertex and face information
                List<PolyfaceMeshVertex> polyfaceVertexes = new List<PolyfaceMeshVertex>();
                List<PolyfaceMeshFace> polyfaceFaces = new List<PolyfaceMeshFace>();
                foreach (Vertex v in vertexes)
                {
                    if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh)) == (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh))
                    {
                        PolyfaceMeshVertex vertex = new PolyfaceMeshVertex
                                                        {
                                                            Color = v.Color,
                                                            Layer = v.Layer,
                                                            LineType = v.LineType,
                                                            Location = v.Location
                                                        };
                        vertex.XData.AddRange(xData);
                        polyfaceVertexes.Add(vertex);
                    }
                    else if ((v.Flags & (VertexTypeFlags.PolyfaceMeshVertex)) == (VertexTypeFlags.PolyfaceMeshVertex))
                    {
                        PolyfaceMeshFace vertex = new PolyfaceMeshFace
                                                      {
                                                          Color = v.Color,
                                                          Layer = v.Layer,
                                                          LineType = v.LineType,
                                                          VertexIndexes = v.VertexIndexes
                                                      };
                        vertex.XData.AddRange(xData);
                        polyfaceFaces.Add(vertex);
                    }

                    //if (v.Type == EntityType.PolyfaceMeshVertex)
                    //{
                    //    polyfaceVertexes.Add((PolyfaceMeshVertex) v);
                    //}
                    //else if (v.Type == EntityType.PolyfaceMeshFace)
                    //{
                    //    polyfaceFaces.Add((PolyfaceMeshFace) v);
                    //}
                    //else
                    //{
                    //    throw new EntityDxfException(v.Type.ToString(), this.file, "Error in vertex type.");
                    //}
                }
                pol = new PolyfaceMesh(polyfaceVertexes, polyfaceFaces);
            }
            else
            {
                List<PolylineVertex> polylineVertexes = new List<PolylineVertex>();
                foreach (Vertex v in vertexes)
                {
                    PolylineVertex vertex = new PolylineVertex
                                                {
                                                    Location = new Vector2(v.Location.X, v.Location.Y),
                                                    BeginThickness = v.BeginThickness,
                                                    Bulge = v.Bulge,
                                                    Color = v.Color,
                                                    EndThickness = v.EndThickness,
                                                    Layer = v.Layer,
                                                    LineType = v.LineType,
                                                };
                    vertex.XData.AddRange(xData);

                    ////posible error avoidance, the polyline is marked as polyline code:(70,0) but the vertex is marked as Polyline3dVertex code:(70,32)
                    //if (v.Type==EntityType.Polyline3dVertex)
                    //{
                    //    PolylineVertex polylineVertex = new PolylineVertex(((Polyline3dVertex)v).Location.X, ((Polyline3dVertex)v).Location.Y);
                    //    polylineVertexes.Add(polylineVertex);
                    //}
                    //else
                    //{
                    //    polylineVertexes.Add((PolylineVertex) v);
                    //}
                    polylineVertexes.Add(vertex);
                }

                pol = new Polyline(polylineVertexes, isClosed)
                          {
                              Thickness=thickness,
                              Elevation = elevation,
                              Normal = normal
                          };
            }

            pol.Color = color;
            pol.Layer = layer;
            pol.LineType = lineType;
            pol.XData.AddRange(xData);

            return pol;
        }

        private Text ReadText(ref CodeValuePair code)
        {
            var text = new Text();

            Vector3 firstAlignmentPoint = Vector3.Zero;
            Vector3 secondAlignmentPoint = Vector3.Zero;
            Vector3 normal = Vector3.UnitZ;
            int horizontalAlignment = 0;
            int verticalAlignment = 0;
            List<XData> xData = new List<XData>();

            code = this.ReadCodePair();
            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 1:
                        text.Value = code.Value;
                        code = this.ReadCodePair();
                        break;
                    case 8: //layer code
                        text.Layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62: //aci color code
                        text.Color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6: //type line code
                        text.LineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        firstAlignmentPoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        firstAlignmentPoint.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        firstAlignmentPoint.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 11:
                        secondAlignmentPoint.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 21:
                        secondAlignmentPoint.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 31:
                        secondAlignmentPoint.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 40:
                        text.Height = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 50:
                        text.Rotation = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 51:
                        text.ObliqueAngle = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 7:
                        text.Style = this.GetTextStyle(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 72:
                        horizontalAlignment = int.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 73:
                        verticalAlignment = int.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 210:
                        normal.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 220:
                        normal.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 230:
                        normal.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            TextAlignment alignment = ObtainAlignment(horizontalAlignment, verticalAlignment);

            text.BasePoint = alignment == TextAlignment.BaselineLeft ? firstAlignmentPoint : secondAlignmentPoint;
            text.Normal = normal;
            text.Alignment = alignment;
            text.XData.AddRange(xData);

            return text;
        }

        private Vertex ReadVertex(ref CodeValuePair code)
        {
            Layer layer = Layer.Default;
            AciColor color = AciColor.ByLayer;
            LineType lineType = LineType.ByLayer;
            Vector3 location = new Vector3();
            List<XData> xData = new List<XData>();
            float endThickness = 0.0f;
            float beginThickness = 0.0f;
            float bulge = 0.0f;
            List<int> vertexIndexes = new List<int>();
            VertexTypeFlags flags = VertexTypeFlags.PolylineVertex;

            code = this.ReadCodePair();

            while (code.Code != 0)
            {
                switch (code.Code)
                {
                    case 8:
                        layer = this.GetLayer(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 62:
                        color = new AciColor(short.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 6:
                        lineType = this.GetLineType(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 10:
                        location.X = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 20:
                        location.Y = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 30:
                        location.Z = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 40:
                        beginThickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 41:
                        endThickness = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 42:
                        bulge = float.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 70:
                        flags = (VertexTypeFlags) int.Parse(code.Value);
                        code = this.ReadCodePair();
                        break;
                    case 71:
                        vertexIndexes.Add(int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 72:
                        vertexIndexes.Add(int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 73:
                        vertexIndexes.Add(int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 74:
                        vertexIndexes.Add(int.Parse(code.Value));
                        code = this.ReadCodePair();
                        break;
                    case 1001:
                        xData.Add(this.ReadXDataRecord(code.Value, ref code));
                        break;
                    default:
                        if (code.Code >= 1000 && code.Code <= 1071)
                            throw new InvalidCodeValueEntityDxfException(code, this.file, "The extended data of an entity must start with the application registry code " + this.fileLine);

                        code = this.ReadCodePair();
                        break;
                }
            }

            return new Vertex
                       {
                           Flags = flags,
                           Location = location,
                           BeginThickness = beginThickness,
                           Bulge = bulge,
                           Color = color,
                           EndThickness = endThickness,
                           Layer = layer,
                           LineType = lineType,
                           VertexIndexes = vertexIndexes.ToArray(),
                           XData = xData
                       };

            //IVertex vertex;
            //if ((flags & (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh)) == (VertexTypeFlags.PolyfaceMeshVertex | VertexTypeFlags.Polygon3dMesh))
            //{
            //    vertex = new PolyfaceMeshVertex
            //                 {
            //                     Location=location
            //                 };
            //    vertex.XData.AddRange(xData);
            //}
            //else if ((flags & (VertexTypeFlags.PolyfaceMeshVertex)) == (VertexTypeFlags.PolyfaceMeshVertex))
            //{
            //    vertex = new PolyfaceMeshFace(vertexIndexes.ToArray());
            //}
            //else if ((flags & (VertexTypeFlags.Polyline3dVertex)) == (VertexTypeFlags.Polyline3dVertex))
            //{
            //    vertex = new Polyline3dVertex
            //    {
            //        Location = location,
            //    };

            //    vertex.XData.AddRange(xData);
            //}
            //else
            //{
            //    vertex = new PolylineVertex
            //                 {
            //                     Location =new Vector2f(location.X, location.Y),
            //                     Bulge = bulge,
            //                     BeginThickness = beginThickness,
            //                     EndThickness = endThickness
            //                 };

            //    vertex.XData.AddRange(xData);
            //}

            //return vertex;
        }

        #endregion

        #region private methods

        private static TextAlignment ObtainAlignment(int horizontal, int vertical)
        {
            TextAlignment alignment = TextAlignment.BaselineLeft;

            if (horizontal == 0 && vertical == 3)
                alignment = TextAlignment.TopLeft;

            else if (horizontal == 1 && vertical == 3)
                alignment = TextAlignment.TopCenter;

            else if (horizontal == 2 && vertical == 3)
                alignment = TextAlignment.TopRight;

            else if (horizontal == 0 && vertical == 2)
                alignment = TextAlignment.MiddleLeft;

            else if (horizontal == 1 && vertical == 2)
                alignment = TextAlignment.MiddleCenter;

            else if (horizontal == 2 && vertical == 2)
                alignment = TextAlignment.MiddleRight;

            else if (horizontal == 0 && vertical == 1)
                alignment = TextAlignment.BottomLeft;

            else if (horizontal == 1 && vertical == 1)
                alignment = TextAlignment.BottomCenter;

            else if (horizontal == 2 && vertical == 1)
                alignment = TextAlignment.BottomRight;

            else if (horizontal == 0 && vertical == 0)
                alignment = TextAlignment.BaselineLeft;

            if (horizontal == 1 && vertical == 0)
                alignment = TextAlignment.BaselineCenter;

            else if (horizontal == 2 && vertical == 0)
                alignment = TextAlignment.BaselineRight;

            return alignment;
        }

        private Block GetBlock(string name)
        {
            if (this.blocks.ContainsKey(name))
            {
                return this.blocks[name];
            }
            return null;
        }

        private Layer GetLayer(string name)
        {
            if (this.layers.ContainsKey(name))
            {
                return this.layers[name];
            }

            //just in case the layer has not been defined in the table section
            var layer = new Layer(name);
            this.layers.Add(layer.Name, layer);
            return layer;
        }

        private LineType GetLineType(string name)
        {
            if (this.lineTypes.ContainsKey(name))
            {
                return this.lineTypes[name];
            }

            //just in case the line type has not been defined in the table section
            var lineType = new LineType(name);
            this.lineTypes.Add(lineType.Name, lineType);
            return lineType;
        }

        private TextStyle GetTextStyle(string name)
        {
            if (this.textStyles.ContainsKey(name))
            {
                return this.textStyles[name];
            }

            //just in case the text style has not been defined in the table section
            var textStyle = new TextStyle(name, "Arial");
            this.textStyles.Add(textStyle.Name, textStyle);
            return textStyle;
        }

        private CodeValuePair ReadCodePair()
        {
            int code;
            string readCode = this.reader.ReadLine();
            this.fileLine += 1;
            if (!int.TryParse(readCode, out code))
            {
                throw (new DxfException("Invalid group code " + readCode + " in line " + this.fileLine));
            }
            string value = this.reader.ReadLine();
            this.fileLine += 1;
            return new CodeValuePair(code, value);
        }

       private XData ReadXDataRecord(string appId, ref CodeValuePair code)
        {
            XData xData = new XData(appId);
            code = this.ReadCodePair();

            while (code.Code >= 1000 && code.Code <= 1071)
            {
                if (code.Code == XDataCode.AppReg)
                    break;

                XDataRecord xDataRecord = new XDataRecord(code.Code, code.Value);
                xData.XDataRecord.Add(xDataRecord);
                code = this.ReadCodePair();
            }

            return xData;
        }

        #endregion
    }
}