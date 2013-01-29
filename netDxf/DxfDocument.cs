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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using netDxf.Blocks;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute=netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Represents a document to read and write dxf ASCII files.
    /// </summary>
    public class DxfDocument
    {
        #region private fields

        #region header

        private FileInfo fileInfo;
        private string name;
        private string version;
        // keeps track of the number of handles generated
        private int handleCount;
        // keeps track of the dimension blocks generated
        private int dimCount;
        // during the save process new handles are needed, this number should be enough
        private const int ReservedHandles = 100;

        private DefaultDrawingUnits drawingUnits = DefaultDrawingUnits.Millimeters;

        #endregion

        #region tables

        private Dictionary<string, ApplicationRegistry> appRegisterNames;
        private readonly Dictionary<string, ViewPort> viewports;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;
        private Dictionary<string, DimensionStyle> dimStyles;

        #endregion

        #region blocks

        private Dictionary<string, Block> blocks;

        #endregion

        #region entities

        private readonly Hashtable addedObjects;
        private List<Arc> arcs;
        private List<Circle> circles;
        private List<Dimension> dimensions; 
        private List<Ellipse> ellipses;
        private List<Solid> solids;
        private List<Face3d> faces3d;
        private List<Insert> inserts;
        private List<Line> lines;
        private List<Point> points;
        private List<PolyfaceMesh> polyfaceMeshes;
        private List<LwPolyline> lwPolylines;
        private List<Polyline> polylines;
        private List<Text> texts;
        private List<MText> mTexts;
        private List<Hatch> hatches;
        private List<Spline> splines;
        private List<Image> images;

        #endregion

        #region objects
        
        private Dictionary<string, ImageDef> imageDefs;
        private RasterVariables rasterVariables;

        #endregion

        #endregion

        #region constructor

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        public DxfDocument()
        {
            this.handleCount = 1;
            this.dimCount = 0;
            this.addedObjects = new Hashtable(); // keeps track of the added object to avoid duplicates

            // tables
            this.viewports = new Dictionary<string, ViewPort>();
            this.layers = new Dictionary<string, Layer>();
            this.lineTypes = new Dictionary<string, LineType>();
            this.textStyles = new Dictionary<string, TextStyle>();
            this.blocks = new Dictionary<string, Block>();
            this.appRegisterNames = new Dictionary<string, ApplicationRegistry>();
            this.dimStyles = new Dictionary<string, DimensionStyle>();

            // objects
            this.imageDefs = new Dictionary<string, ImageDef>();

            AddDefaultObjects();

            this.arcs = new List<Arc>();
            this.ellipses = new List<Ellipse>();
            this.dimensions = new List<Dimension>();
            this.faces3d = new List<Face3d>();
            this.solids = new List<Solid>();
            this.inserts = new List<Insert>();
            this.lwPolylines = new List<LwPolyline>(); 
            this.polylines = new List<Polyline>();
            this.polyfaceMeshes = new List<PolyfaceMesh>();
            this.lines = new List<Line>();
            this.circles = new List<Circle>();
            this.points = new List<Point>();
            this.texts = new List<Text>();
            this.mTexts = new List<MText>();
            this.hatches = new List<Hatch>();
            this.splines = new List<Spline>();
            this.images = new List<Image>();
        }

        #endregion

        #region public properties

        #region header

        /// <summary>
        /// Gets the dxf file <see cref="DxfVersion">version</see>.
        /// </summary>
        public string Version
        {
            get { return version; }
        }

        /// <summary>
        /// Gets the name of the dxf document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        /// Gets information of the dxf file once it is saved or loaded.
        /// </summary>
        public FileInfo FileInfo
        {
            get { return fileInfo; }
        }

        /// <summary>
        /// Gets the default drawing units of the document.
        /// </summary>
        public DefaultDrawingUnits DefaultDrawingUnits
        {
            get { return drawingUnits; }
        }

        /// <summary>
        /// Gets the raster variables applied to image entities.
        /// </summary>
        public RasterVariables RasterVariables
        {
            get { return rasterVariables; }
        }

        #endregion

        #region table public properties

        /// <summary>
        /// Gets the application registered names.
        /// </summary>
        public ReadOnlyCollection<ApplicationRegistry> AppRegisterNames
        {
            get
            {
                List<ApplicationRegistry> list = new List<ApplicationRegistry>();
                list.AddRange(this.appRegisterNames.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Layer">layer</see> list.
        /// </summary>
        public ReadOnlyCollection<Layer> Layers
        {
            get
            {
                List<Layer> list = new List<Layer>();
                list.AddRange(this.layers.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="LineType">linetype</see> list.
        /// </summary>
        public ReadOnlyCollection<LineType> LineTypes
        {
            get
            {
                List<LineType> list = new List<LineType>();
                list.AddRange(this.lineTypes.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="TextStyle">text style</see> list.
        /// </summary>
        public ReadOnlyCollection<TextStyle> TextStyles
        {
            get
            {
                List<TextStyle> list = new List<TextStyle>();
                list.AddRange(this.textStyles.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Block">block</see> list.
        /// </summary>
        public ReadOnlyCollection<Block> Blocks
        {
            get
            {
                List<Block> list = new List<Block>();
                list.AddRange(this.blocks.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="ImageDef">image definitions</see> list.
        /// </summary>
        public ReadOnlyCollection<ImageDef> ImageDefs
        {
            get
            {
                List<ImageDef> list = new List<ImageDef>();
                list.AddRange(this.imageDefs.Values);
                return list.AsReadOnly();
            }
        }

        #endregion

        #region entities public properties

        /// <summary>
        /// Gets the <see cref="Arc">arcs</see> list.
        /// </summary>
        public ReadOnlyCollection<Arc> Arcs
        {
            get { return this.arcs.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Ellipse">ellipses</see> list.
        /// </summary>
        public ReadOnlyCollection<Ellipse> Ellipses
        {
            get { return this.ellipses.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Circle">circles</see> list.
        /// </summary>
        public ReadOnlyCollection<Circle> Circles
        {
            get { return this.circles.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Face3d">3d faces</see> list.
        /// </summary>
        public ReadOnlyCollection<Face3d> Faces3d
        {
            get { return this.faces3d.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Solid">solids</see> list.
        /// </summary>
        public ReadOnlyCollection<Solid> Solids
        {
            get { return this.solids.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Insert">inserts</see> list.
        /// </summary>
        public ReadOnlyCollection<Insert> Inserts
        {
            get { return this.inserts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Line">lines</see> list.
        /// </summary>
        public ReadOnlyCollection<Line> Lines
        {
            get { return this.lines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Polyline">polylines</see> list.
        /// </summary>
        public ReadOnlyCollection<Polyline> Polylines
        {
            get { return this.polylines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="LwPolyline">light weight polylines</see> list.
        /// </summary>
        public ReadOnlyCollection<LwPolyline> LwPolylines
        {
            get { return this.lwPolylines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="PolyfaceMesh">polyface meshes</see> list.
        /// </summary>
        public ReadOnlyCollection<PolyfaceMesh> PolyfaceMesh
        {
            get { return this.polyfaceMeshes.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Point">points</see> list.
        /// </summary>
        public ReadOnlyCollection<Point> Points
        {
            get { return this.points.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Text">texts</see> list.
        /// </summary>
        public ReadOnlyCollection<Text> Texts
        {
            get { return this.texts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="MText">multiline texts</see> list.
        /// </summary>
        public ReadOnlyCollection<MText> MTexts
        {
            get { return this.mTexts.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Hatch">hatches</see> list.
        /// </summary>
        public ReadOnlyCollection<Hatch> Hatches
        {
            get { return this.hatches.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Image">images</see> list.
        /// </summary>
        public ReadOnlyCollection<Image> Images
        {
            get { return this.images.AsReadOnly(); }
        }

        #endregion

        #endregion

        #region public table methods

        /// <summary>
        /// Adds a layer to the table.
        /// </summary>
        /// <param name="layer"><see cref="Layer">Layer</see> to add to the dictionary.</param>
        /// <returns>
        /// If a layer already exists with the same name as the instance that is being added the method returns the existing layer,
        /// if not it will return the new layer.
        /// </returns>
        public Layer AddLayer(Layer layer)
        {
            Layer add;
            if (this.layers.TryGetValue(layer.Name, out add))
                return add;

            this.layers.Add(layer.Name, layer);
            layer.LineType = AddLineType(layer.LineType);
            this.handleCount = layer.AsignHandle(this.handleCount);
            return layer;
        }

        /// <summary>
        /// Gets a layer from the the table.
        /// </summary>
        /// <param name="layerName"><see cref="Layer">Layer</see> name.</param>
        /// <returns>Layer with the actual name, null if it does not exists.</returns>
        public Layer GetLayer(string layerName)
        {
            Layer add;
            return this.layers.TryGetValue(layerName, out add) ? add : null;
        }

        /// <summary>
        /// Adds a line type to the table.
        /// </summary>
        /// <param name="lineType"><see cref="LineType">Line type</see> to add to the dictionary.</param>
        /// <returns>
        /// If a line type already exists with the same name as the instance that is being added the method returns the existing line type,
        /// if not it will return the new line type.
        /// </returns>
        public LineType AddLineType(LineType lineType)
        {
            LineType add;
            if (this.lineTypes.TryGetValue(lineType.Name, out add))
                return add;

            this.lineTypes.Add(lineType.Name, lineType);
            this.handleCount = lineType.AsignHandle(this.handleCount);
            return lineType;

        }

        /// <summary>
        /// Gets a line type from the the table.
        /// </summary>
        /// <param name="lineTypeName"><see cref="LineType">Line type</see> name.</param>
        /// <returns>Line type with the actual name, null if it does not exists.</returns>
        public LineType GetLineType(string lineTypeName)
        {
            LineType add;
            return this.lineTypes.TryGetValue(lineTypeName, out add) ? add : null;
        }

        /// <summary>
        /// Adds a text style to the table.
        /// </summary>
        /// <param name="textStyle"><see cref="TextStyle">Text style</see> to add to the dictionary.</param>
        /// <returns>
        /// If a text style already exists with the same name as the instance that is being added the method returns the existing text style,
        /// if not it will return the new text style.
        /// </returns>
        public TextStyle AddTextStyle(TextStyle textStyle)
        {
            TextStyle add;
            if (this.textStyles.TryGetValue(textStyle.Name, out add))
                return add;

            this.textStyles.Add(textStyle.Name, textStyle);
            this.handleCount = textStyle.AsignHandle(this.handleCount);
            return textStyle;
        }

        /// <summary>
        /// Gets a text style from the the table.
        /// </summary>
        /// <param name="textStyleName"><see cref="TextStyle">Text style</see> name.</param>
        /// <returns>Text style with the actual name, null if it does not exists.</returns>
        public TextStyle GetTextStyle(string textStyleName)
        {
            TextStyle add;
            return this.textStyles.TryGetValue(textStyleName, out add) ? add : null;
        }

        /// <summary>
        /// Adds a dimension style to the table.
        /// </summary>
        /// <param name="dimensionStyle"><see cref="DimensionStyle">Dimension style</see> to add to the dictionary.</param>
        /// <returns>
        /// If a dimension style already exists with the same name as the instance that is being added the method returns the existing dimension style,
        /// if not it will return the new dimension style.
        /// </returns>
        public DimensionStyle AddDimensionStyle(DimensionStyle dimensionStyle)
        {
            DimensionStyle add;
            if (this.dimStyles.TryGetValue(dimensionStyle.Name, out add))
                return add;

            this.dimStyles.Add(dimensionStyle.Name, dimensionStyle);
            dimensionStyle.TextStyle = AddTextStyle(dimensionStyle.TextStyle);
            this.handleCount = dimensionStyle.AsignHandle(this.handleCount);
            return dimensionStyle;
        }

        /// <summary>
        /// Gets a dimension style from the the table.
        /// </summary>
        /// <param name="dimensionStyleName"><see cref="DimensionStyle">Dimension style</see> name.</param>
        /// <returns>Dimension style with the actual name, null if it does not exists.</returns>
        public DimensionStyle GetDimensionStyle(string dimensionStyleName)
        {
            DimensionStyle add;
            return this.dimStyles.TryGetValue(dimensionStyleName, out add) ? add : null;
        }

        /// <summary>
        /// Adds a block to the table.
        /// </summary>
        /// <param name="block"><see cref="Block">Block</see> to add to the dictionary.</param>
        /// <returns>
        /// If a block already exists with the same name as the instance that is being added the method returns the existing block,
        /// if not it will return the new block.
        /// </returns>
        public Block AddBlock(Block block)
        {
            Block add;
            if (this.blocks.TryGetValue(block.Name, out add))
                return add;

            // if the block definition has not been added
            this.blocks.Add(block.Name, block);
            block.Layer = AddLayer(block.Layer);
            this.handleCount = block.AsignHandle(this.handleCount);

            //for new block definitions configure its entities
            foreach (IEntityObject blockEntity in block.Entities)
            {
                // check if the entity has not been added to the document
                if (this.addedObjects.ContainsKey(blockEntity))
                    throw new ArgumentException("The entity " + blockEntity.Type + " object of the block " + block.Name + " has already been added to the document.", "block");

                this.addedObjects.Add(blockEntity, blockEntity);

                if (blockEntity.XData != null)
                {
                    foreach (ApplicationRegistry appReg in blockEntity.XData.Keys)
                    {
                        if (this.appRegisterNames.ContainsKey(appReg.Name)) continue;
                        this.appRegisterNames.Add(appReg.Name, appReg);
                        this.handleCount = appReg.AsignHandle(this.handleCount);
                    }
                }
                blockEntity.Layer = AddLayer(blockEntity.Layer);
                blockEntity.LineType = AddLineType(blockEntity.LineType);

                switch (blockEntity.Type)
                {
                    case EntityType.Arc:
                        break;
                    case EntityType.Circle:
                        break;
                    case EntityType.Dimension:
                        // create the block that represent the dimension drawing
                        Block dimBlock = ((Dimension)blockEntity).BuildBlock("*D" + ++dimCount);
                        if (this.blocks.ContainsKey(dimBlock.Name))
                            throw new ArgumentException("The list already contains the block: " + dimBlock.Name + ". The block names that start with *D are reserverd for dimensions");
                        ((Dimension)blockEntity).Style = AddDimensionStyle(((Dimension)blockEntity).Style);
                        dimBlock.TypeFlags = BlockTypeFlags.AnonymousBlock;
                        foreach (IEntityObject entity in dimBlock.Entities)
                        {
                            entity.Layer = AddLayer(entity.Layer);
                            entity.LineType = AddLineType(entity.LineType);
                        }
                        this.handleCount = dimBlock.AsignHandle(this.handleCount);
                        this.blocks.Add(dimBlock.Name, dimBlock);
                        break;
                    case EntityType.Ellipse:
                        break;
                    case EntityType.Face3D:
                        break;
                    case EntityType.Spline:
                        break;
                    case EntityType.Hatch:
                        break;
                    case EntityType.Insert:
                        ((Insert)blockEntity).Block = AddBlock(((Insert)blockEntity).Block);
                        foreach (Attribute attribute in ((Insert)blockEntity).Attributes)
                        {
                            attribute.Layer = AddLayer(attribute.Layer);
                            attribute.LineType = AddLineType(attribute.LineType);
                        }
                        break;
                    case EntityType.LightWeightPolyline:
                        break;
                    case EntityType.Line:
                        break;
                    case EntityType.Point:
                        break;
                    case EntityType.PolyfaceMesh:
                        break;
                    case EntityType.Polyline3d:
                        break;
                    case EntityType.Solid:
                        break;
                    case EntityType.Text:
                        ((Text)blockEntity).Style = AddTextStyle(((Text)blockEntity).Style);
                        break;
                    case EntityType.MText:
                        ((MText)blockEntity).Style = AddTextStyle(((MText)blockEntity).Style);
                        break;
                    case EntityType.Image:
                        Image image = (Image)blockEntity;
                        image.Definition = AddImageDef(image.Definition);
                        ImageDefReactor reactor = new ImageDefReactor(image.Handle);
                        this.handleCount = reactor.AsignHandle(this.handleCount);
                        image.Definition.Reactors.Add(image.Handle, reactor);
                        break;
                    case EntityType.AttributeDefinition:
                        throw new ArgumentException("The entity " + blockEntity.Type + " is only allowed as part of another entity", "block");

                    case EntityType.Attribute:
                        throw new ArgumentException("The entity " + blockEntity.Type + " is only allowed as part of another entity", "block");

                    default:
                        throw new ArgumentException("The entity " + blockEntity.Type + " is not implemented or unknown");
                }
            }

            //for new block definitions configure its attributes
            foreach (AttributeDefinition attribute in block.Attributes.Values)
            {
                attribute.Layer = AddLayer(attribute.Layer);
                attribute.LineType = AddLineType(attribute.LineType);
                attribute.Style = AddTextStyle(attribute.Style);
            }

            return block;
        }

        /// <summary>
        /// Gets a block from the the table.
        /// </summary>
        /// <param name="blockName"><see cref="Block">Block</see> name.</param>
        /// <returns>Block with the actual name, null if it does not exists.</returns>
        public Block GetBlock(string blockName)
        {
            Block add;
            return this.blocks.TryGetValue(blockName, out add) ? add : null;
        }

        /// <summary>
        /// Adds a image definition to the table.
        /// </summary>
        /// <param name="imageDef"><see cref="ImageDef">Image definition</see> to add to the dictionary.</param>
        /// <returns>
        /// If an image definition already exists with the same name as the instance that is being added the method returns the existing image definition,
        /// if not it will return the new image definition.
        /// </returns>
        public ImageDef AddImageDef(ImageDef imageDef)
        {
            ImageDef add;
            if (this.imageDefs.TryGetValue(imageDef.Name, out add))
                return add;

            this.imageDefs.Add(imageDef.Name, imageDef);
            this.handleCount = imageDef.AsignHandle(this.handleCount);
            //this.imageDefToReactors.Add(imageDef, new List<string>());
            return imageDef;
        }

        /// <summary>
        /// Gets a image definition from the the table.
        /// </summary>
        /// <param name="imageDefName"><see cref="ImageDef">Image definition</see> name.</param>
        /// <returns>Image definition with the actual name, null if it does not exists.</returns>
        public ImageDef GetImageDef(string imageDefName)
        {
            ImageDef add;
            return this.imageDefs.TryGetValue(imageDefName, out add) ? add : null;
        }


        #endregion

        #region public methods

         /// <summary>
        /// Adds a list of <see cref="IEntityObject">entities</see> to the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="IEntityObject">entities</see> to add to the document.</param>
        /// <remarks>
        /// <para>
        /// Once an entity has been added to the dxf document, it should not be modified. A unique handle identifier is assigned to every entity.
        /// </para>
        /// <para>
        /// This is specially true in the case of dimensions. The block that represents the drawing of the dimension is built
        /// when it is added to the document. If a property is modified once it has been added this modifications will not be 
        /// reflected in the saved dxf file.
        /// </para>
        /// </remarks>
        public void AddEntity(IEnumerable<IEntityObject> entities)
         {
             foreach (IEntityObject entity in entities)
             {
                 this.AddEntity(entity);
             }
         }

        /// <summary>
        /// Adds an <see cref="IEntityObject">entity</see> to the document.
        /// </summary>
        /// <param name="entity">An <see cref="IEntityObject">entity</see> to add to the document.</param>
        /// <remarks>
        /// <para>
        /// Once an entity has been added to the dxf document a unique handle identifier (hexadecimal number) is assigned to them.
        /// </para>
        /// <para>
        /// The entities should not be modified. This is specially true in the case of dimensions. The block that represents the drawing of the dimension is built
        /// when it is added to the document. If a property is modified once it has been added this modifications will not be 
        /// reflected in the saved dxf file.
        /// </para>
        /// </remarks>
        public void AddEntity(IEntityObject entity)
        {
            // check if the entity has not been added to the document
            if (this.addedObjects.ContainsKey(entity))
                throw new ArgumentException("The entity " + entity.Type + " object has already been added to the document.", "entity");

            this.addedObjects.Add(entity, entity);
            this.handleCount = ((DxfObject)entity).AsignHandle(this.handleCount);

            if (entity.XData != null)
            {
                foreach (ApplicationRegistry appReg in entity.XData.Keys)
                {
                    if (this.appRegisterNames.ContainsKey(appReg.Name)) continue;
                    this.appRegisterNames.Add(appReg.Name, appReg);
                    this.handleCount = appReg.AsignHandle(this.handleCount);
                }
            }

            entity.Layer = AddLayer(entity.Layer);
            entity.LineType = AddLineType(entity.LineType);

            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.arcs.Add((Arc) entity);
                    break;
                case EntityType.Circle:
                    this.circles.Add((Circle) entity);
                    break;
                case EntityType.Dimension:
                    this.dimensions.Add((Dimension) entity);

                    // create the block that represent the dimension drawing
                    Block dimBlock = ((Dimension) entity).BuildBlock("*D" + ++dimCount);
                    if (this.blocks.ContainsKey(dimBlock.Name))
                        throw new ArgumentException("The list already contains the block: " + dimBlock.Name + ". The block names that start with *D are reserverd for dimensions");
                    ((Dimension) entity).Style = AddDimensionStyle(((Dimension) entity).Style);
                    dimBlock.TypeFlags = BlockTypeFlags.AnonymousBlock;
                    this.handleCount = dimBlock.AsignHandle(this.handleCount);

                    this.blocks.Add(dimBlock.Name, dimBlock);
                    break;
                case EntityType.Ellipse:
                    this.ellipses.Add((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    this.faces3d.Add((Face3d) entity);
                    break;
                case EntityType.Spline:
                    this.splines.Add((Spline)entity);
                    break;
                case EntityType.Hatch:
                    this.hatches.Add((Hatch)entity);
                    break;
                case EntityType.Insert:
                    ((Insert) entity).Block = AddBlock(((Insert) entity).Block);
                    foreach (Attribute attribute in ((Insert) entity).Attributes)
                    {
                        attribute.Layer = AddLayer(attribute.Layer);
                        attribute.LineType = AddLineType(attribute.LineType);
                    }
                    this.inserts.Add((Insert)entity);
                    break;
                case EntityType.LightWeightPolyline:
                    this.lwPolylines.Add((LwPolyline)entity);
                    break;
                case EntityType.Line:
                    this.lines.Add((Line) entity);
                    break;
                case EntityType.Point:
                    this.points.Add((Point) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.polyfaceMeshes.Add((PolyfaceMesh)entity);
                    break;
                case EntityType.Polyline3d:
                    this.polylines.Add((Polyline) entity);
                    break;
                case EntityType.Solid:
                    this.solids.Add((Solid) entity);
                    break;
                case EntityType.Text:
                    ((Text) entity).Style = AddTextStyle(((Text) entity).Style);
                    this.texts.Add((Text) entity);
                    break;
                case EntityType.MText:
                    ((MText)entity).Style = AddTextStyle(((MText)entity).Style);
                    this.mTexts.Add((MText) entity);
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    image.Definition = AddImageDef(image.Definition);
                    ImageDefReactor reactor = new ImageDefReactor(image.Handle);
                    this.handleCount = reactor.AsignHandle(this.handleCount);
                    image.Definition.Reactors.Add(image.Handle, reactor);
                    this.images.Add(image);
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }
        }

        /// <summary>
        /// Removes a list of <see cref="IEntityObject">entities</see> from the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="IEntityObject">entities</see> to remove from the document.</param>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.
        /// This includes empity layers, blocks not referenced anymore, line types, text styles, dimension styles, and application registries.
        /// </remarks>
        public void RemoveEntity(IEnumerable<IEntityObject> entities)
        {
            foreach (IEntityObject entity in entities)
            {
                this.RemoveEntity(entity);
            }
        }

        /// <summary>
        /// Removes an <see cref="IEntityObject">entity</see> from the document.
        /// </summary>
        /// <param name="entity">The <see cref="IEntityObject">entity</see> to remove from the document.</param>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.
        /// This includes empity layers, blocks not referenced anymore, line types, text styles, dimension styles, and application registries.
        /// </remarks>
        public void RemoveEntity(IEntityObject entity)
        {
            if (!this.addedObjects.ContainsKey(entity))
                return;

            this.addedObjects.Remove(entity);

            switch (entity.Type)
            {
                case EntityType.Arc:
                    this.arcs.Remove((Arc)entity);
                    break;
                case EntityType.Circle:
                    this.circles.Remove((Circle)entity);
                    break;
                case EntityType.Dimension:
                    this.dimensions.Remove((Dimension)entity);
                    break;
                case EntityType.Ellipse:
                    this.ellipses.Remove((Ellipse)entity);
                    break;
                case EntityType.Face3D:
                    this.faces3d.Remove((Face3d)entity);
                    break;
                case EntityType.Spline:
                    this.splines.Remove((Spline) entity);
                    break;
                case EntityType.Hatch:
                    this.hatches.Remove((Hatch)entity);
                    break;
                case EntityType.Insert:
                    this.inserts.Remove((Insert)entity);
                    break;
                case EntityType.LightWeightPolyline:
                    this.lwPolylines.Remove((LwPolyline)entity);
                    break;
                case EntityType.Line:
                    this.lines.Remove((Line)entity);
                    break;
                case EntityType.Point:
                    this.points.Remove((Point)entity);
                    break;
                case EntityType.PolyfaceMesh:
                    this.polyfaceMeshes.Remove((PolyfaceMesh)entity);
                    break;
                case EntityType.Polyline3d:
                    this.polylines.Remove((Polyline)entity);
                    break;
                case EntityType.Solid:
                    this.solids.Remove((Solid)entity);
                    break;
                case EntityType.Text:
                    this.texts.Remove((Text)entity);
                    break;
                case EntityType.MText:
                    this.mTexts.Remove((MText)entity);
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    image.Definition.Reactors.Remove(image.Handle);
                    if (image.Definition.Reactors.Count == 0)
                        this.imageDefs.Remove(image.Definition.Name);
                    this.images.Remove(image);
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }
        }

        /// <summary>
        /// Loads a dxf ASCII file.
        /// </summary>
        /// <param name="file">File name.</param>
        public void Load(string file)
        {
            this.fileInfo = new FileInfo(file);
            if (!this.fileInfo.Exists)
                throw new FileNotFoundException("File " + this.fileInfo.FullName + " not found.", this.fileInfo.FullName);

            this.name = Path.GetFileNameWithoutExtension(this.fileInfo.FullName);

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfReader dxfReader = new DxfReader(this.fileInfo.FullName);
            dxfReader.Open();
            dxfReader.Read();
            dxfReader.Close();

            //header information
            this.version = dxfReader.Version;
            this.handleCount = Convert.ToInt32(dxfReader.HandleSeed,16);
            this.drawingUnits = dxfReader.DrawingUnits;

            //tables information
            this.appRegisterNames = dxfReader.ApplicationRegistrationIds;
            this.layers = dxfReader.Layers;
            this.lineTypes = dxfReader.LineTypes;
            this.textStyles = dxfReader.TextStyles;
            this.dimStyles = dxfReader.DimensionStyles;
            this.blocks = dxfReader.Blocks;
            this.dimCount = dxfReader.DimBlockCount;

            //entities information
            this.arcs = dxfReader.Arcs;
            this.circles = dxfReader.Circles;
            this.ellipses = dxfReader.Ellipses;
            this.points = dxfReader.Points;
            this.faces3d = dxfReader.Faces3d;
            this.solids = dxfReader.Solids;
            this.lwPolylines = dxfReader.LightWeightPolyline;
            this.polylines = dxfReader.Polylines;
            this.polyfaceMeshes = dxfReader.PolyfaceMeshes;
            this.lines = dxfReader.Lines;
            this.inserts = dxfReader.Inserts;
            this.texts = dxfReader.Texts;
            this.mTexts = dxfReader.MTexts;
            this.hatches = dxfReader.Hatches;
            this.dimensions = dxfReader.Dimensions;
            this.splines = dxfReader.Splines;
            this.images = dxfReader.Images;

            // objects
            this.imageDefs = dxfReader.ImageDefs;
            // we will define a new RasterVariables object in case there is none in the dxf
            if (dxfReader.RasterVariables == null)
            {
                this.rasterVariables = new RasterVariables();
                this.handleCount = this.rasterVariables.AsignHandle(this.handleCount);
            }
            else
                this.rasterVariables = dxfReader.RasterVariables;

            Thread.CurrentThread.CurrentCulture = cultureInfo;

        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a dxf ASCII file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="dxfVersion">Dxf file <see cref="DxfVersion">version</see>.</param>
        /// <remarks>If the file already exists it will be overwritten.</remarks>
        public void Save(string file, DxfVersion dxfVersion)
        {
            this.fileInfo = new FileInfo(file);

            this.name = Path.GetFileNameWithoutExtension(this.fileInfo.FullName);

            this.version = StringEnum.GetStringValue(dxfVersion);

            // create the list for the block record table
            Dictionary<string, List<IEntityObject>> blockEntities = new Dictionary<string, List<IEntityObject>>();
            foreach (Block block in this.blocks.Values)
            {
                blockEntities.Add(block.Name, new List<IEntityObject>());
                foreach (IEntityObject entity in block.Entities)
                {
                    blockEntities[block.Name].Add(entity);
                }
            }

            // dictionaries
            List<DictionaryObject> dictionaries = new List<DictionaryObject>();

            DictionaryObject namedObjectDictionary = new DictionaryObject("0");
            this.handleCount = namedObjectDictionary.AsignHandle(this.handleCount);
            DictionaryObject baseDictionary = new DictionaryObject(namedObjectDictionary.Handle);
            this.handleCount = baseDictionary.AsignHandle(this.handleCount);
            namedObjectDictionary.Entries.Add(new DictionaryObjectEntry("ACAD_GROUP", baseDictionary.Handle));
            dictionaries.Add(namedObjectDictionary);
            dictionaries.Add(baseDictionary);

            // create the image dictionary
            DictionaryObject imageDefDictionary = new DictionaryObject(baseDictionary.Handle);
            if (this.imageDefs.Count > 0)
            {
                this.handleCount = imageDefDictionary.AsignHandle(this.handleCount);
                foreach (ImageDef imageDef in this.imageDefs.Values)
                    imageDefDictionary.Entries.Add(new DictionaryObjectEntry(imageDef.Name, imageDef.Handle));

                dictionaries.Add(imageDefDictionary);

                namedObjectDictionary.Entries.Add(new DictionaryObjectEntry("ACAD_IMAGE_DICT", imageDefDictionary.Handle));
                namedObjectDictionary.Entries.Add(new DictionaryObjectEntry("ACAD_IMAGE_VARS", this.rasterVariables.Handle));
            }

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfWriter dxfWriter = new DxfWriter(this.fileInfo.FullName, dxfVersion) {handleCount = this.handleCount};
            dxfWriter.Open();
            dxfWriter.WriteComment("Dxf file generated by netDxf http://netdxf.codeplex.com, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL");
            dxfWriter.WriteComment(string.Format("File {0} created on {1} by {2}", this.fileInfo.FullName, DateTime.Now.ToString(cultureInfo), Environment.UserName));

            //HEADER SECTION
            dxfWriter.BeginSection(StringCode.HeaderSection);
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.DatabaseVersion, this.version));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.DwgCodePage, "ANSI_1252"));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.HandSeed, Convert.ToString(this.handleCount + ReservedHandles, 16)));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.Angbase, 0));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.Angdir, 0));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.Extnames, 1));
            dxfWriter.WriteSystemVariable(new HeaderVariable(SystemVariable.Insunits, (int)this.drawingUnits));
            dxfWriter.EndSection();

            //CLASSES SECTION
            dxfWriter.BeginSection(StringCode.ClassesSection);
            dxfWriter.WriteRasterVariablesClass(1);
            if (this.imageDefs.Values.Count > 0)
            {
                dxfWriter.WriteImageDefClass(this.imageDefs.Count);
                dxfWriter.WriteImageDefRectorClass(this.images.Count);
                dxfWriter.WriteImageClass(this.images.Count);
            }
            dxfWriter.EndSection();

            //TABLES SECTION
            dxfWriter.BeginSection(StringCode.TablesSection);

            //viewport tables
            dxfWriter.BeginTable(StringCode.ViewPortTable);
            foreach (ViewPort vport in this.viewports.Values)
            {
                dxfWriter.WriteViewPort(vport);
            }
            dxfWriter.EndTable();

            //line type tables
            dxfWriter.BeginTable(StringCode.LineTypeTable);
            foreach (LineType lineType in this.lineTypes.Values)
            {
                dxfWriter.WriteLineType(lineType);
            }
            dxfWriter.EndTable();

            //layer tables
            dxfWriter.BeginTable(StringCode.LayerTable);
            foreach (Layer layer in this.layers.Values)
            {
                dxfWriter.WriteLayer(layer);
            }
            dxfWriter.EndTable();

            //text style tables
            dxfWriter.BeginTable(StringCode.TextStyleTable);
            foreach (TextStyle style in this.textStyles.Values)
            {
                dxfWriter.WriteTextStyle(style);
            }
            dxfWriter.EndTable();

            //dimension style tables
            dxfWriter.BeginTable(StringCode.DimensionStyleTable);
            foreach (DimensionStyle style in this.dimStyles.Values)
            {
                dxfWriter.WriteDimensionStyle(style);
            }
            dxfWriter.EndTable();   

            //view
            dxfWriter.BeginTable(StringCode.ViewTable);
            dxfWriter.EndTable();

            //ucs
            dxfWriter.BeginTable(StringCode.UcsTable);
            dxfWriter.EndTable();

            //registered application tables
            dxfWriter.BeginTable(StringCode.ApplicationIDTable);
            foreach (ApplicationRegistry id in this.appRegisterNames.Values)
            {
                dxfWriter.RegisterApplication(id);
            }
            dxfWriter.EndTable();

            //block reacord table
            dxfWriter.BeginTable(StringCode.BlockRecordTable);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlockRecord(block.Record);
            }
            dxfWriter.EndTable();

            dxfWriter.EndSection(); //End section tables

            dxfWriter.BeginSection(StringCode.BlocksSection);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlock(block, blockEntities[block.Name]);
            }

            dxfWriter.EndSection(); //End section blocks

            //ENTITIES SECTION
            dxfWriter.BeginSection(StringCode.EntitiesSection);
            
            foreach (Arc arc in this.arcs)
            {
                dxfWriter.WriteEntity(arc);
            }
            foreach (Circle circle in this.circles)
            {
                dxfWriter.WriteEntity(circle);
            }
            foreach (Ellipse ellipse  in this.ellipses)
            {
                dxfWriter.WriteEntity(ellipse);
            }
            foreach (Point point in this.points)
            {
                dxfWriter.WriteEntity(point);
            }
            foreach (Face3d face in this.faces3d)
            {
                dxfWriter.WriteEntity(face);
            }
            foreach (Spline spline in this.splines)
            {
                dxfWriter.WriteEntity(spline);
            }
            foreach (Solid solid in this.solids)
            {
                dxfWriter.WriteEntity(solid);
            }
            foreach (Insert insert in this.inserts)
            {
                dxfWriter.WriteEntity(insert);
            }
            foreach (Line line in this.lines)
            {
                dxfWriter.WriteEntity(line);
            }
            foreach (LwPolyline pol in this.lwPolylines)
            {
                dxfWriter.WriteEntity(pol);
            }
            foreach (PolyfaceMesh pol in this.polyfaceMeshes)
            {
                dxfWriter.WriteEntity(pol);
            }
            foreach (Polyline pol in this.polylines)
            {
               dxfWriter.WriteEntity(pol);
            }
            foreach (Text text in this.texts)
            {
                dxfWriter.WriteEntity(text);
            }
            foreach (MText mText in this.mTexts)
            {
                dxfWriter.WriteEntity(mText);
            }
            foreach (Hatch hatch in this.hatches)
            {
                dxfWriter.WriteEntity(hatch);
            }
            foreach (Dimension dim in this.dimensions)
            {
                dxfWriter.WriteEntity(dim);
            }
            foreach (Image image in this.images)
            {
                dxfWriter.WriteEntity(image);
            }
            dxfWriter.EndSection(); //End section entities

            //OBJECTS SECTION
            dxfWriter.BeginSection(StringCode.ObjectsSection);
            
            foreach (DictionaryObject dictionary in dictionaries)
            {
                dxfWriter.WriteDictionary(dictionary);
            }

            dxfWriter.WriteRasterVariables(this.rasterVariables);
            foreach (ImageDef imageDef in this.imageDefs.Values)
            {
                foreach (ImageDefReactor reactor in imageDef.Reactors.Values)
                {
                    dxfWriter.WriteImageDefReactor(reactor);
                }
                dxfWriter.WriteImageDef(imageDef, imageDefDictionary.Handle);
            }


            dxfWriter.EndSection();

            dxfWriter.Close();

            Thread.CurrentThread.CurrentCulture = cultureInfo;
        }
         		
        #endregion

        #region private methods

        private void AddDefaultObjects()
        {
            //add default viewports
            ViewPort active = ViewPort.Active;
            this.handleCount = active.AsignHandle(this.handleCount);
            this.viewports.Add(active.Name, active);

            //add default layer
            Layer byDefault = Layer.Default;
            this.handleCount = byDefault.AsignHandle(this.handleCount);
            this.layers.Add(byDefault.Name, byDefault);

            // add default line types
            LineType byLayer = LineType.ByLayer;
            this.handleCount = byLayer.AsignHandle(this.handleCount);
            this.lineTypes.Add(byLayer.Name, byLayer);

            LineType byBlock = LineType.ByBlock;
            this.handleCount = byBlock.AsignHandle(this.handleCount);
            this.lineTypes.Add(byBlock.Name, byBlock);

            LineType continuous = LineType.Continuous;
            this.handleCount = continuous.AsignHandle(this.handleCount);
            this.lineTypes.Add(continuous.Name, continuous);

            // add default blocks
            Block modelSpace = Block.ModelSpace;
            this.handleCount = modelSpace.AsignHandle(this.handleCount);
            this.blocks.Add(modelSpace.Name, modelSpace);

            Block paperSpace = Block.PaperSpace;
            this.handleCount = paperSpace.AsignHandle(this.handleCount);
            this.blocks.Add(paperSpace.Name, paperSpace);

            // add default text style
            TextStyle defaultStyle = TextStyle.Default;
            this.handleCount = defaultStyle.AsignHandle(this.handleCount);
            this.textStyles.Add(defaultStyle.Name, defaultStyle);

            // add default application registry
            ApplicationRegistry defaultAppId = ApplicationRegistry.Default;
            this.handleCount = defaultAppId.AsignHandle(this.handleCount);
            this.appRegisterNames.Add(defaultAppId.Name, defaultAppId);

            // add default dimension style
            DimensionStyle defaultDimStyle = DimensionStyle.Default;
            this.handleCount = defaultDimStyle.AsignHandle(this.handleCount);
            defaultDimStyle.TextStyle = defaultStyle;
            this.dimStyles.Add(defaultDimStyle.Name, defaultDimStyle);

            // raster variables
            this.rasterVariables = new RasterVariables();
            this.handleCount = this.rasterVariables.AsignHandle(this.handleCount);

        }

        #endregion
    }
}