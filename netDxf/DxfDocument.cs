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
    /// Represents a document to read and write dxf files.
    /// </summary>
    public class DxfDocument
    {
        #region private fields

        private string name;
        // keeps track of the number of handles generated
        private int handlesGenerated;
        // keeps track of the dimension blocks generated
        private int dimensionBlocksGenerated;
        // keeps track of the group names generated (this groups have the isUnnamed bool set to true)
        private int groupNamesGenerated;
        // during the save process new handles are needed for the table sections, this number should be enough
        private const int ReservedHandles = 10;

        #region header

        private List<string> comments;
        private HeaderVariables drawingVariables;

        #endregion

        #region tables

        private readonly Dictionary<string, ViewPort> viewports;

        // Key: table object name, Value: table object
        private Dictionary<string, ApplicationRegistry> appRegisterNames;
        private Dictionary<string, Layer> layers;
        private Dictionary<string, LineType> lineTypes;
        private Dictionary<string, TextStyle> textStyles;
        private Dictionary<string, DimensionStyle> dimStyles;

        #endregion

        #region blocks

        private Dictionary<string, Block> blocks;

        #endregion

        #region entities

        //entity objects added to the document (key: handle, value: entity). This dictionary also includes entities that are part of a block.
        private Dictionary<string, EntityObject> addedEntity;
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
        private List<MLine> mLines;

        #endregion

        #region objects

        private Dictionary<string, Group> groups;
        private Dictionary<string, MLineStyle> mLineStyles;
        private Dictionary<string, ImageDef> imageDefs;
        private RasterVariables rasterVariables;

        #endregion

        #endregion

        #region constructor

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <remarks>The default <see cref="HeaderVariables">drawing variables</see> of the document will be used.</remarks>
        public DxfDocument()
            : this(new HeaderVariables())
        {
        }

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="version">AutoCAD drawing database version number.</param>
        public DxfDocument(DxfVersion version)
            : this(new HeaderVariables{AcadVer = version})
        {
        }

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        public DxfDocument(HeaderVariables drawingVariables)
        {
            this.comments = new List<string> {"Dxf file generated by netDxf http://netdxf.codeplex.com, Copyright(C) 2013 Daniel Carvajal, Licensed under LGPL"};
            this.drawingVariables = drawingVariables;

            this.handlesGenerated = 1;
            this.dimensionBlocksGenerated = 0;
            this.groupNamesGenerated = 0;
            this.addedEntity = new Dictionary<string, EntityObject>(); // keeps track of the added object to avoid duplicates

            // tables
            this.viewports = new Dictionary<string, ViewPort>(StringComparer.InvariantCultureIgnoreCase);
            this.layers = new Dictionary<string, Layer>(StringComparer.InvariantCultureIgnoreCase);
            this.lineTypes = new Dictionary<string, LineType>(StringComparer.InvariantCultureIgnoreCase);
            this.textStyles = new Dictionary<string, TextStyle>(StringComparer.InvariantCultureIgnoreCase);
            this.blocks = new Dictionary<string, Block>(StringComparer.InvariantCultureIgnoreCase);
            this.appRegisterNames = new Dictionary<string, ApplicationRegistry>(StringComparer.InvariantCultureIgnoreCase);
            this.dimStyles = new Dictionary<string, DimensionStyle>(StringComparer.InvariantCultureIgnoreCase);

            // objects
            this.groups = new Dictionary<string, Group>(StringComparer.InvariantCultureIgnoreCase);
            this.mLineStyles = new Dictionary<string, MLineStyle>(StringComparer.InvariantCultureIgnoreCase);
            this.imageDefs = new Dictionary<string, ImageDef>(StringComparer.InvariantCultureIgnoreCase);
            
            AddDefaultObjects();

            // entities lists
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
            this.mLines = new List<MLine>();
        }

        #endregion

        #region public properties

        #region header

        /// <summary>
        /// Gets or sets the name of the document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public List<string> Comments
        {
            get { return comments; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                comments = value;
            }
        }

        /// <summary>
        /// Gets the dxf <see cref="HeaderVariables">drawing variables</see>.
        /// </summary>
        public HeaderVariables DrawingVariables
        {
            get { return this.drawingVariables; }
        }

        /// <summary>
        /// Gets or sets the name of the document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        #endregion

        #region table public properties

        /// <summary>
        /// Gets the <see cref="ApplicationRegistry">application registries</see> list.
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
        /// Gets the <see cref="PolyfaceMeshes">polyface meshes</see> list.
        /// </summary>
        public ReadOnlyCollection<PolyfaceMesh> PolyfaceMeshes
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

        /// <summary>
        /// Gets the <see cref="MLines">multilines</see> list.
        /// </summary>
        public ReadOnlyCollection<MLine> MLines
        {
            get { return this.mLines.AsReadOnly(); }
        }

        #endregion

        #region public object properties

        /// <summary>
        /// Gets the <see cref="RasterVariables">raster variables</see> applied to image entities.
        /// </summary>
        public RasterVariables RasterVariables
        {
            get { return rasterVariables; }
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

        /// <summary>
        /// Gets the <see cref="MLineStyle">MLine styles</see> list.
        /// </summary>
        public ReadOnlyCollection<MLineStyle> MLineStyles
        {
            get
            {
                List<MLineStyle> list = new List<MLineStyle>();
                list.AddRange(this.mLineStyles.Values);
                return list.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the <see cref="Group">group</see> list.
        /// </summary>
        public ReadOnlyCollection<Group> Groups
        {
            get
            {
                List<Group> list = new List<Group>();
                list.AddRange(this.groups.Values);
                return list.AsReadOnly();
            }
        }

        #endregion

        #endregion

        #region public table methods

        /// <summary>
        /// Adds an application registry to the dictionary.
        /// </summary>
        /// <param name="appReg"><see cref="ApplicationRegistry">Application registry</see> to add to the dictionary.</param>
        /// <returns>
        /// If an application registry already exists with the same name as the instance that is being added the method returns the existing application registry,
        /// if not it will return the new application registry.
        /// </returns>
        public ApplicationRegistry AddApplicationRegistry(ApplicationRegistry appReg)
        {
            ApplicationRegistry add;
            if (this.appRegisterNames.TryGetValue(appReg.Name, out add))
                return add;

            this.appRegisterNames.Add(appReg.Name, appReg);
            this.handlesGenerated = appReg.AsignHandle(this.handlesGenerated);
            return appReg;
        }

        /// <summary>
        /// Gets an application registry from the the dictionary.
        /// </summary>
        /// <param name="appRegName"><see cref="ApplicationRegistry">Application registry</see> name.</param>
        /// <returns>Application registry with the actual name, null if it does not exists.</returns>
        public ApplicationRegistry GetApplicationRegistry(string appRegName)
        {
            ApplicationRegistry appReg;
            return this.appRegisterNames.TryGetValue(appRegName, out appReg) ? appReg : null;
        }

        /// <summary>
        /// Adds a layer to the dictionary.
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
            this.handlesGenerated = layer.AsignHandle(this.handlesGenerated);
            return layer;
        }

        /// <summary>
        /// Gets a layer from the the dictionary.
        /// </summary>
        /// <param name="layerName"><see cref="Layer">Layer</see> name.</param>
        /// <returns>Layer with the actual name, null if it does not exists.</returns>
        public Layer GetLayer(string layerName)
        {
            Layer layer;
            return this.layers.TryGetValue(layerName, out layer) ? layer : null;
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
            this.handlesGenerated = lineType.AsignHandle(this.handlesGenerated);
            return lineType;

        }

        /// <summary>
        /// Gets a line type from the the dictionary.
        /// </summary>
        /// <param name="lineTypeName"><see cref="LineType">Line type</see> name.</param>
        /// <returns>Line type with the actual name, null if it does not exists.</returns>
        public LineType GetLineType(string lineTypeName)
        {
            LineType lineType;
            return this.lineTypes.TryGetValue(lineTypeName, out lineType) ? lineType : null;
        }

        /// <summary>
        /// Adds a text style to the dictionary.
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
            this.handlesGenerated = textStyle.AsignHandle(this.handlesGenerated);
            return textStyle;
        }

        /// <summary>
        /// Gets a text style from the the dictionary.
        /// </summary>
        /// <param name="textStyleName"><see cref="TextStyle">Text style</see> name.</param>
        /// <returns>Text style with the actual name, null if it does not exists.</returns>
        public TextStyle GetTextStyle(string textStyleName)
        {
            TextStyle textStyle;
            return this.textStyles.TryGetValue(textStyleName, out textStyle) ? textStyle : null;
        }

        /// <summary>
        /// Adds a dimension style to the dictionary.
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
            this.handlesGenerated = dimensionStyle.AsignHandle(this.handlesGenerated);
            return dimensionStyle;
        }

        /// <summary>
        /// Gets a dimension style from the the dictionary.
        /// </summary>
        /// <param name="dimensionStyleName"><see cref="DimensionStyle">Dimension style</see> name.</param>
        /// <returns>Dimension style with the actual name, null if it does not exists.</returns>
        public DimensionStyle GetDimensionStyle(string dimensionStyleName)
        {
            DimensionStyle dimStyle;
            return this.dimStyles.TryGetValue(dimensionStyleName, out dimStyle) ? dimStyle : null;
        }

        /// <summary>
        /// Adds a block to the dictionary.
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
            this.handlesGenerated = block.AsignHandle(this.handlesGenerated);
            this.blocks.Add(block.Name, block);
            block.Layer = AddLayer(block.Layer);
            
            //for new block definitions configure its entities
            foreach (EntityObject blockEntity in block.Entities)
            {
                AddEntity(blockEntity, true);
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
        /// Gets a block from the the dictionary.
        /// </summary>
        /// <param name="blockName"><see cref="Block">Block</see> name.</param>
        /// <returns>Block with the actual name, null if it does not exists.</returns>
        public Block GetBlock(string blockName)
        {
            Block block;
            return this.blocks.TryGetValue(blockName, out block) ? block : null;
        }

        #endregion

        #region public entity methods

        /// <summary>
        /// Gets an entity provided its handle.
        /// </summary>
        /// <param name="handle">Entity object handle.</param>
        /// <returns>The entity associated with the provided handle, null if it is not found.</returns>
        /// <remarks>This method will also return entities that are part of a block definition.</remarks>
        public EntityObject GetEntityByHandle(string handle)
        {
            return this.addedEntity[handle];
        }

        /// <summary>
        /// Adds a list of <see cref="EntityObject">entities</see> to the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to add to the document.</param>
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
        public void AddEntity(IEnumerable<EntityObject> entities)
        {
            foreach (EntityObject entity in entities)
            {
                this.AddEntity(entity);
            }
        }

        /// <summary>
        /// Adds an <see cref="EntityObject">entity</see> to the document.
        /// </summary>
        /// <param name="entity">An <see cref="EntityObject">entity</see> to add to the document.</param>
        /// <remarks>
        /// <returns>True if the entity has been added to the document, false otherwise.</returns>
        /// <para>
        /// Once an entity has been added to the dxf document a unique handle identifier (hexadecimal number) is assigned to them.
        /// </para>
        /// <para>
        /// The entities should not be modified. This is specially true in the case of dimensions. The block that represents the drawing of the dimension is built
        /// when it is added to the document. If a property is modified once it has been added this modifications will not be reflected in the saved dxf file.
        /// </para>
        /// </remarks>
        public bool AddEntity(EntityObject entity)
        {
            return AddEntity(entity, false);
        }

        /// <summary>
        /// Removes a list of <see cref="EntityObject">entities</see> from the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to remove from the document.</param>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.<br />
        /// This includes empity layers, blocks not referenced anymore, line types, text styles, dimension styles, and application registries.<br />
        /// Entities that are part of a block definition will not be removed.
        /// </remarks>
        public void RemoveEntity(IEnumerable<EntityObject> entities)
        {
            foreach (EntityObject entity in entities)
            {
                this.RemoveEntity(entity);
            }
        }

        /// <summary>
        /// Removes an <see cref="EntityObject">entity</see> from the document.
        /// </summary>
        /// <param name="entity">The <see cref="EntityObject">entity</see> to remove from the document.</param>
        /// <returns>True if item is successfully removed; otherwise, false. This method also returns false if item was not found.</returns>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.<br />
        /// This includes empity layers, blocks not referenced anymore, line types, text styles, dimension styles, multiline styles, groups, and application registries.<br />
        /// Entities that are part of a block definition will not be removed.
        /// </remarks>
        public bool RemoveEntity(EntityObject entity)
        {
            if (!this.addedEntity.ContainsKey(entity.Handle))
                return false;

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition
            // and they will not be removed from the drawing database
            bool removed;

            switch (entity.Type)
            {
                case EntityType.Arc:
                    removed = this.arcs.Remove((Arc)entity);
                    break;
                case EntityType.Circle:
                    removed = this.circles.Remove((Circle)entity);
                    break;
                case EntityType.Dimension:
                    removed = this.dimensions.Remove((Dimension)entity);
                    // we can safely remove the block associated with the dimension, each dimension has its own block.
                    if(removed)
                        this.blocks.Remove(((Dimension) entity).Block.Name);
                    break;
                case EntityType.Ellipse:
                    removed = this.ellipses.Remove((Ellipse)entity);
                    break;
                case EntityType.Face3D:
                    removed = this.faces3d.Remove((Face3d)entity);
                    break;
                case EntityType.Spline:
                    removed = this.splines.Remove((Spline)entity);
                    break;
                case EntityType.Hatch:
                    removed = this.hatches.Remove((Hatch)entity);
                    break;
                case EntityType.Insert:
                    removed = this.inserts.Remove((Insert)entity);
                    break;
                case EntityType.LightWeightPolyline:
                    removed = this.lwPolylines.Remove((LwPolyline)entity);
                    break;
                case EntityType.Line:
                    removed = this.lines.Remove((Line)entity);
                    break;
                case EntityType.Point:
                    removed = this.points.Remove((Point)entity);
                    break;
                case EntityType.PolyfaceMesh:
                    removed = this.polyfaceMeshes.Remove((PolyfaceMesh)entity);
                    break;
                case EntityType.Polyline:
                    removed = this.polylines.Remove((Polyline)entity);
                    break;
                case EntityType.Solid:
                    removed = this.solids.Remove((Solid)entity);
                    break;
                case EntityType.Text:
                    removed = this.texts.Remove((Text)entity);
                    break;
                case EntityType.MText:
                    removed = this.mTexts.Remove((MText)entity);
                    break;
                case EntityType.Image:
                    Image image = (Image)entity;
                    removed = this.images.Remove(image);
                    if (removed)
                    {
                        image.Definition.Reactors.Remove(image.Handle);
                        if (image.Definition.Reactors.Count == 0)
                            this.imageDefs.Remove(image.Definition.Name);
                    }
                    break;
                case EntityType.MLine:
                    removed = this.mLines.Remove((MLine)entity);
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }

            // only entities that are not part of a block definition will be removed
            if (removed)
                this.addedEntity.Remove(entity.Handle);

            return removed;
        }

        #endregion

        #region public object methods

        /// <summary>
        /// Adds an image definition to the dictionary.
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
            this.handlesGenerated = imageDef.AsignHandle(this.handlesGenerated);
            return imageDef;
        }

        /// <summary>
        /// Gets an image definition from the the dictionary.
        /// </summary>
        /// <param name="imageDefName"><see cref="ImageDef">Image definition</see> name.</param>
        /// <returns>Image definition with the actual name, null if it does not exists.</returns>
        public ImageDef GetImageDef(string imageDefName)
        {
            ImageDef imageDef;
            return this.imageDefs.TryGetValue(imageDefName, out imageDef) ? imageDef : null;
        }

        /// <summary>
        /// Adds a MLine style to the dictionary.
        /// </summary>
        /// <param name="mLineStyle">MLine <see cref="MLineStyle">style</see> to add to the dictionary.</param>
        /// <returns>
        /// If a MLine style already exists with the same name as the instance that is being added the method returns the existing MLine style,
        /// if not it will return the new MLine style.
        /// </returns>
        public MLineStyle AddMLineStyle(MLineStyle mLineStyle)
        {
            MLineStyle add;
            if (this.mLineStyles.TryGetValue(mLineStyle.Name, out add))
                return add;

            this.mLineStyles.Add(mLineStyle.Name, mLineStyle);
            foreach (MLineStyleElement element in mLineStyle.Elements)
            {
                element.LineType = AddLineType(element.LineType);
            }
            this.handlesGenerated = mLineStyle.AsignHandle(this.handlesGenerated);
            return mLineStyle;
        }

        /// <summary>
        /// Gets a MLine style from the the dictionary.
        /// </summary>
        /// <param name="mLineStyleName">MLine <see cref="MLineStyle">style</see> name.</param>
        /// <returns>MLine style with the actual name, null if it does not exists.</returns>
        public MLineStyle GetMLineStyle(string mLineStyleName)
        {
            MLineStyle mLineStyle;
            return this.mLineStyles.TryGetValue(mLineStyleName, out mLineStyle) ? mLineStyle : null;
        }

        /// <summary>
        /// Adds a group to the list.
        /// </summary>
        /// <param name="group"><see cref="Group">Group</see> to add to the list.</param>
        /// <returns>
        /// If a group already exists with the same name as the instance that is being added the method returns the existing group,
        /// if not it will return the new group.
        /// </returns>
        /// <remarks>All entities of the group will be automatically added to the document.</remarks>
        public Group AddGroup(Group group)
        {
            // if no name has been given to the group a generic name will be created
            if (group.IsUnnamed)
                group.Name = "*A" + ++groupNamesGenerated;

            Group add;
            if (this.groups.TryGetValue(group.Name, out add))
                return add;

            this.handlesGenerated = group.AsignHandle(this.handlesGenerated);
            this.groups.Add(group.Name, group);
            foreach (EntityObject entity in group.Entities)
            {
                this.AddEntity(entity);
            }

            return group;
        }

        /// <summary>
        /// Gets a group from the the dictionary.
        /// </summary>
        /// <param name="groupName"><see cref="Group">Group</see> name.</param>
        /// <returns>Group with the actual name, null if it does not exists.</returns>
        public Group GetGroup(string groupName)
        {
            Group group;
            return this.groups.TryGetValue(groupName, out group) ? group : null;
        }

        /// <summary>
        /// Removes a group from the list.
        /// </summary>
        /// <param name="group"><see cref="Group">Group</see> to remove from the list.</param>
        /// <param name="removeEntities">Defines if the entities contained in the group will be deleted.</param>
        /// <returns>True if group is successfully removed; otherwise, false.</returns>
        public bool RemoveGroup(Group group, bool removeEntities)
        {
            foreach ( EntityObject entity in group.Entities)
            {
                this.RemoveEntity(entity);
            }
            return this.groups.Remove(group.Name);
        }

        #endregion

        #region public methods

        /// <summary>
        /// Loads a dxf file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        /// <remarks>
        /// Loading dxf files prior to AutoCad 2000 is not supported.<br />
        /// The Load method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode it will raise any exception that migh occur during the whole process.
        /// </remarks>
        public static DxfDocument Load(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File " + fileInfo.FullName + " not found.", fileInfo.FullName);

            Stream stream;
            try
            {
                stream = File.OpenRead(file);
            }
            catch (Exception ex)
            {
                throw new IOException("Error trying to open the file " + fileInfo.FullName + " for reading.", ex);
            }

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

#if DEBUG
            DxfDocument document = InternalLoad(stream);
            stream.Close();
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            DxfDocument document;
            try
            {
                 document = InternalLoad(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                stream.Close();
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }

#endif
            document.name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
            return document;
        }

        /// <summary>
        /// Loads a dxf file.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <remarks>
        /// Loading dxf files prior to AutoCad 2000 is not supported.<br />
        /// On Debug mode it will raise any exception that might occur during the whole process.
        /// </remarks>
        public static DxfDocument Load(Stream stream)
        {
            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

#if DEBUG
            DxfDocument document = InternalLoad(stream);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            DxfDocument document;
            try
            {
                 document = InternalLoad(stream);
            }
            catch
            {
                return null;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }

#endif
            return document;
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a dxf file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Return true if the file has been succesfully save, false otherwise.</returns>
        /// <exception cref="IOException"></exception>
        /// <remarks>
        /// If the file already exists it will be overwritten.<br />
        /// The Save method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode they will raise any exception that migh occur during the whole process.
        /// </remarks>
        public bool Save(string file)
        {

            FileInfo fileInfo = new FileInfo(file);
            this.name = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            Stream stream;
            try
            {
                stream = File.Create(file);
            }
            catch (Exception ex)
            {
                throw new IOException("Error trying to create the file " + fileInfo.FullName + " for writing.", ex);
            }

#if DEBUG
            InternalSave(stream);
            stream.Close();
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            try
            {
                InternalSave(stream);
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Close();
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
                
#endif
            return true;
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>Return true if the stream has been succesfully saved, false otherwise.</returns>
        /// <remarks>
        /// On Debug mode it will raise any exception that might occur during the whole process.
        /// </remarks>
        public bool Save(Stream stream)
        {
            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

#if DEBUG
            InternalSave(stream);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            try
            {
                InternalSave(stream);
            }
            catch
            {
                return false;
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = cultureInfo;
            }
                
#endif
            return true;
        }

        /// <summary>
        /// Checks the AutoCAD dxf file database version.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>String that represents the dxf file version.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        public static DxfVersion CheckDxfFileVersion(Stream stream)
        {
            string value = DxfReader.CheckHeaderVariable(stream, HeaderVariableCode.AcadVer);
            return (DxfVersion) StringEnum.Parse(typeof (DxfVersion), value);
        }

        /// <summary>
        /// Checks the AutoCAD dxf file database version.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>String that represents the dxf file version.</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="IOException"></exception>
        public static DxfVersion CheckDxfFileVersion(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File " + fileInfo.FullName + " not found.", fileInfo.FullName);

            Stream stream;
            try
            {
                stream = File.OpenRead(file);
            }
            catch (Exception ex)
            {
                throw new IOException("Error trying to open the file " + fileInfo.FullName + " for reading.", ex);
            }

            string value;
            try
            {
                value = DxfReader.CheckHeaderVariable(stream, HeaderVariableCode.AcadVer);
            }
            catch
            {
                return DxfVersion.Unknown;
            }
            finally
            {
                stream.Close();
            }
            return (DxfVersion)StringEnum.Parse(typeof(DxfVersion), value);
        }

        #endregion

        #region private methods

        private bool AddEntity(EntityObject entity, bool isBlockEntity)
        {
            // no null entities allowed
            if (entity == null)
                throw new ArgumentNullException("entity", "The entity cannot be null.");

            if (entity.Handle != null)
            {
                // check if the entity handle has been assigned
                if (this.addedEntity.ContainsKey(entity.Handle))
                {
                    // if the handle is equal the entity might come from another document, check if it is exactly the same object
                    EntityObject existing = this.addedEntity[entity.Handle];
                    // if the entity is already in the document return false, do not add it again
                    if (existing.Equals(entity))
                        return false;
                }
            }

            this.handlesGenerated = entity.AsignHandle(this.handlesGenerated);
            this.addedEntity.Add(entity.Handle, entity);

            foreach (string appReg in entity.XData.Keys)
            {
                entity.XData[appReg].ApplicationRegistry = AddApplicationRegistry(entity.XData[appReg].ApplicationRegistry);
            }

            entity.Layer = AddLayer(entity.Layer);
            entity.LineType = AddLineType(entity.LineType);

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition.
            switch (entity.Type)
            {
                case EntityType.Arc:
                    if(!isBlockEntity) this.arcs.Add((Arc)entity);
                    break;
                case EntityType.Circle:
                    if (!isBlockEntity) this.circles.Add((Circle)entity);
                    break;
                case EntityType.Dimension:
                    if (!isBlockEntity) this.dimensions.Add((Dimension)entity);
                    // create the block that represent the dimension drawing
                    Block dimBlock = ((Dimension)entity).BuildBlock("*D" + ++dimensionBlocksGenerated);
                    if (this.blocks.ContainsKey(dimBlock.Name))
                        throw new ArgumentException("The list already contains the block: " + dimBlock.Name + ". The block names that start with *D are reserverd for dimensions");
                    ((Dimension)entity).Style = AddDimensionStyle(((Dimension)entity).Style);
                    dimBlock.TypeFlags = BlockTypeFlags.AnonymousBlock;
                    // add the dimension block to the document
                    AddBlock(dimBlock);
                    break;
                case EntityType.Ellipse:
                    if (!isBlockEntity) this.ellipses.Add((Ellipse)entity);
                    break;
                case EntityType.Face3D:
                    if (!isBlockEntity) this.faces3d.Add((Face3d)entity);
                    break;
                case EntityType.Spline:
                    if (!isBlockEntity) this.splines.Add((Spline)entity);
                    break;
                case EntityType.Hatch:
                    if (!isBlockEntity) this.hatches.Add((Hatch)entity);
                    break;
                case EntityType.Insert:
                    ((Insert)entity).Block = AddBlock(((Insert)entity).Block);
                    foreach (Attribute attribute in ((Insert)entity).Attributes)
                    {
                        attribute.Layer = AddLayer(attribute.Layer);
                        attribute.LineType = AddLineType(attribute.LineType);
                    }
                    if (!isBlockEntity) this.inserts.Add((Insert)entity);
                    break;
                case EntityType.LightWeightPolyline:
                    if (!isBlockEntity) this.lwPolylines.Add((LwPolyline)entity);
                    break;
                case EntityType.Line:
                    if (!isBlockEntity) this.lines.Add((Line)entity);
                    break;
                case EntityType.Point:
                    if (!isBlockEntity) this.points.Add((Point)entity);
                    break;
                case EntityType.PolyfaceMesh:
                    if (!isBlockEntity) this.polyfaceMeshes.Add((PolyfaceMesh)entity);
                    break;
                case EntityType.Polyline:
                    if (!isBlockEntity) this.polylines.Add((Polyline)entity);
                    break;
                case EntityType.Solid:
                    if (!isBlockEntity) this.solids.Add((Solid)entity);
                    break;
                case EntityType.Text:
                    ((Text)entity).Style = AddTextStyle(((Text)entity).Style);
                    if (!isBlockEntity) this.texts.Add((Text)entity);
                    break;
                case EntityType.MText:
                    ((MText)entity).Style = AddTextStyle(((MText)entity).Style);
                    if (!isBlockEntity) this.mTexts.Add((MText)entity);
                    break;
                case EntityType.Image:
                    Image image = (Image)entity;
                    image.Definition = AddImageDef(image.Definition);
                    ImageDefReactor reactor = new ImageDefReactor(image.Handle);
                    this.handlesGenerated = reactor.AsignHandle(this.handlesGenerated);
                    image.Definition.Reactors.Add(image.Handle, reactor);
                    if (!isBlockEntity) this.images.Add(image);
                    break;
                case EntityType.MLine:
                    ((MLine)entity).Style = AddMLineStyle(((MLine)entity).Style);
                    if (!isBlockEntity) this.mLines.Add((MLine)entity);
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of block definition.", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of block definition.", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown.");
            }

            return true;
        }

        private static DxfDocument InternalLoad(Stream stream)
        {
            DxfDocument document = new DxfDocument();
            
            DxfReader dxfReader = new DxfReader();

            dxfReader.Read(stream);

            document.addedEntity = dxfReader.AddedEntity;

            //header information
            document.comments = dxfReader.Comments;
            document.drawingVariables = dxfReader.HeaderVariables;
            document.handlesGenerated = Convert.ToInt32(dxfReader.HeaderVariables.HandleSeed, 16);

            //tables information
            document.appRegisterNames = dxfReader.ApplicationRegistrationIds;
            document.layers = dxfReader.Layers;
            document.lineTypes = dxfReader.LineTypes;
            document.textStyles = dxfReader.TextStyles;
            document.dimStyles = dxfReader.DimensionStyles;
            document.blocks = dxfReader.Blocks;
            document.dimensionBlocksGenerated = dxfReader.DimensionBlocksGenerated;

            //entities information
            document.arcs = dxfReader.Arcs;
            document.circles = dxfReader.Circles;
            document.ellipses = dxfReader.Ellipses;
            document.points = dxfReader.Points;
            document.faces3d = dxfReader.Faces3d;
            document.solids = dxfReader.Solids;
            document.lwPolylines = dxfReader.LightWeightPolyline;
            document.polylines = dxfReader.Polylines;
            document.polyfaceMeshes = dxfReader.PolyfaceMeshes;
            document.lines = dxfReader.Lines;
            document.inserts = dxfReader.Inserts;
            document.texts = dxfReader.Texts;
            document.mTexts = dxfReader.MTexts;
            document.hatches = dxfReader.Hatches;
            document.dimensions = dxfReader.Dimensions;
            document.splines = dxfReader.Splines;
            document.images = dxfReader.Images;
            document.mLines = dxfReader.MLines;

            // objects
            document.groups = dxfReader.Groups;
            document.groupNamesGenerated = dxfReader.GroupNamesGenerated;
            document.mLineStyles = dxfReader.MLineStyles;
            document.imageDefs = dxfReader.ImageDefs;
            // we will define a new RasterVariables object in case there is none in the dxf
            if (dxfReader.RasterVariables == null)
            {
                document.rasterVariables = new RasterVariables();
                document.handlesGenerated = document.rasterVariables.AsignHandle(document.handlesGenerated);
            }
            else
                document.rasterVariables = dxfReader.RasterVariables;

            return document;
        }

        private void InternalSave(Stream stream)
        {
            if (this.drawingVariables.AcadVer < DxfVersion.AutoCad2000)
                throw new NotSupportedException("Only AutoCad2000 and newer dxf versions are supported.");

            // dictionaries
            List<DictionaryObject> dictionaries = new List<DictionaryObject>();

            DictionaryObject namedObjectDictionary = new DictionaryObject("0");
            this.handlesGenerated = namedObjectDictionary.AsignHandle(this.handlesGenerated);
            DictionaryObject baseDictionary = new DictionaryObject(namedObjectDictionary.Handle);
            this.handlesGenerated = baseDictionary.AsignHandle(this.handlesGenerated);
            namedObjectDictionary.Entries.Add(baseDictionary.Handle, "ACAD_GROUP");
            dictionaries.Add(namedObjectDictionary);
            dictionaries.Add(baseDictionary);

            // create the Group dictionary
            DictionaryObject groupDictionary = new DictionaryObject(baseDictionary.Handle);
            if (this.groups.Count > 0)
            {
                this.handlesGenerated = groupDictionary.AsignHandle(this.handlesGenerated);
                foreach (Group group in this.groups.Values)
                {
                    groupDictionary.Entries.Add(group.Handle, group.Name);
                }
                dictionaries.Add(groupDictionary);
                namedObjectDictionary.Entries.Add(groupDictionary.Handle, "ACAD_GROUP");
            }

            // create the MLine style dictionary
            DictionaryObject mLineStyleDictionary = new DictionaryObject(baseDictionary.Handle);
            if (this.mLineStyles.Count > 0)
            {
                this.handlesGenerated = mLineStyleDictionary.AsignHandle(this.handlesGenerated);
                foreach (MLineStyle mLineStyle in this.mLineStyles.Values)
                {
                    mLineStyleDictionary.Entries.Add(mLineStyle.Handle, mLineStyle.Name);
                }
                dictionaries.Add(mLineStyleDictionary);
                namedObjectDictionary.Entries.Add(mLineStyleDictionary.Handle, "ACAD_MLINESTYLE");
            }

            // create the image dictionary
            DictionaryObject imageDefDictionary = new DictionaryObject(baseDictionary.Handle);
            if (this.imageDefs.Count > 0)
            {
                this.handlesGenerated = imageDefDictionary.AsignHandle(this.handlesGenerated);
                foreach (ImageDef imageDef in this.imageDefs.Values)
                    imageDefDictionary.Entries.Add(imageDef.Handle, imageDef.Name);

                dictionaries.Add(imageDefDictionary);

                namedObjectDictionary.Entries.Add(imageDefDictionary.Handle,"ACAD_IMAGE_DICT");
                namedObjectDictionary.Entries.Add(this.rasterVariables.Handle, "ACAD_IMAGE_VARS");
            }

            this.drawingVariables.HandleSeed = Convert.ToString(this.handlesGenerated + ReservedHandles, 16);

            DxfWriter dxfWriter = new DxfWriter(this.drawingVariables.AcadVer);
            dxfWriter.Open(stream);
            foreach (string comment in comments)
            {
                dxfWriter.WriteComment(comment);
            }
            

            //HEADER SECTION
            dxfWriter.BeginSection(StringCode.HeaderSection);
            foreach (HeaderVariable variable in this.drawingVariables.Values)
            {
                dxfWriter.WriteSystemVariable(variable);
            }
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
            dxfWriter.BeginTable(StringCode.ViewPortTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (ViewPort vport in this.viewports.Values)
            {
                dxfWriter.WriteViewPort(vport);
            }
            dxfWriter.EndTable();

            //line type tables
            dxfWriter.BeginTable(StringCode.LineTypeTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (LineType lineType in this.lineTypes.Values)
            {
                dxfWriter.WriteLineType(lineType);
            }
            dxfWriter.EndTable();

            //layer tables
            dxfWriter.BeginTable(StringCode.LayerTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (Layer layer in this.layers.Values)
            {
                dxfWriter.WriteLayer(layer);
            }
            dxfWriter.EndTable();

            //text style tables
            dxfWriter.BeginTable(StringCode.TextStyleTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (TextStyle style in this.textStyles.Values)
            {
                dxfWriter.WriteTextStyle(style);
            }
            dxfWriter.EndTable();

            //dimension style tables
            dxfWriter.BeginTable(StringCode.DimensionStyleTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (DimensionStyle style in this.dimStyles.Values)
            {
                dxfWriter.WriteDimensionStyle(style);
            }
            dxfWriter.EndTable();

            //view
            dxfWriter.BeginTable(StringCode.ViewTable, Convert.ToString(this.handlesGenerated++, 16));
            dxfWriter.EndTable();

            //ucs
            dxfWriter.BeginTable(StringCode.UcsTable, Convert.ToString(this.handlesGenerated++, 16));
            dxfWriter.EndTable();

            //registered application tables
            dxfWriter.BeginTable(StringCode.ApplicationIDTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (ApplicationRegistry id in this.appRegisterNames.Values)
            {
                dxfWriter.RegisterApplication(id);
            }
            dxfWriter.EndTable();

            //block reacord table
            dxfWriter.BeginTable(StringCode.BlockRecordTable, Convert.ToString(this.handlesGenerated++, 16));
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlockRecord(block.Record);
            }
            dxfWriter.EndTable();

            dxfWriter.EndSection(); //End section tables

            dxfWriter.BeginSection(StringCode.BlocksSection);
            foreach (Block block in this.blocks.Values)
            {
                dxfWriter.WriteBlock(block);
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
            foreach (Ellipse ellipse in this.ellipses)
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
            foreach (MLine mLine in this.mLines)
            {
                dxfWriter.WriteEntity(mLine);
            }
            dxfWriter.EndSection(); //End section entities

            //OBJECTS SECTION
            dxfWriter.BeginSection(StringCode.ObjectsSection);

            foreach (DictionaryObject dictionary in dictionaries)
            {
                dxfWriter.WriteDictionary(dictionary);
            }

            foreach (Group group in this.groups.Values)
            {
                dxfWriter.WriteGroup(group, groupDictionary.Handle);
            }
            foreach (MLineStyle style in this.mLineStyles.Values)
            {
                dxfWriter.WriteMLineStyle(style, mLineStyleDictionary.Handle);
            }

            dxfWriter.WriteRasterVariables(this.rasterVariables, imageDefDictionary.Handle);
            foreach (ImageDef imageDef in this.imageDefs.Values)
            {
                foreach (ImageDefReactor reactor in imageDef.Reactors.Values)
                {
                    dxfWriter.WriteImageDefReactor(reactor);
                }
                dxfWriter.WriteImageDef(imageDef, imageDefDictionary.Handle);
            }

            dxfWriter.EndSection(); //End section objects

            dxfWriter.Close();

            stream.Position = 0;
        }

        private void AddDefaultObjects()
        {
            //add default viewports
            ViewPort active = ViewPort.Active;
            this.handlesGenerated = active.AsignHandle(this.handlesGenerated);
            this.viewports.Add(active.Name, active);

            //add default layer
            Layer byDefault = Layer.Default;
            this.handlesGenerated = byDefault.AsignHandle(this.handlesGenerated);
            this.layers.Add(byDefault.Name, byDefault);

            // add default line types
            LineType byLayer = LineType.ByLayer;
            this.handlesGenerated = byLayer.AsignHandle(this.handlesGenerated);
            this.lineTypes.Add(byLayer.Name, byLayer);

            LineType byBlock = LineType.ByBlock;
            this.handlesGenerated = byBlock.AsignHandle(this.handlesGenerated);
            this.lineTypes.Add(byBlock.Name, byBlock);

            LineType continuous = LineType.Continuous;
            this.handlesGenerated = continuous.AsignHandle(this.handlesGenerated);
            this.lineTypes.Add(continuous.Name, continuous);

            // add default blocks
            Block modelSpace = Block.ModelSpace;
            this.handlesGenerated = modelSpace.AsignHandle(this.handlesGenerated);
            this.blocks.Add(modelSpace.Name, modelSpace);

            Block paperSpace = Block.PaperSpace;
            this.handlesGenerated = paperSpace.AsignHandle(this.handlesGenerated);
            this.blocks.Add(paperSpace.Name, paperSpace);

            // add default text style
            TextStyle defaultStyle = TextStyle.Default;
            this.handlesGenerated = defaultStyle.AsignHandle(this.handlesGenerated);
            this.textStyles.Add(defaultStyle.Name, defaultStyle);

            // add default application registry
            ApplicationRegistry defaultAppId = ApplicationRegistry.Default;
            this.handlesGenerated = defaultAppId.AsignHandle(this.handlesGenerated);
            this.appRegisterNames.Add(defaultAppId.Name, defaultAppId);

            // add default dimension style
            DimensionStyle defaultDimStyle = DimensionStyle.Default;
            this.handlesGenerated = defaultDimStyle.AsignHandle(this.handlesGenerated);
            defaultDimStyle.TextStyle = defaultStyle;
            this.dimStyles.Add(defaultDimStyle.Name, defaultDimStyle);

            // add default MLine style
            MLineStyle defaultMLineStyleStyle = MLineStyle.Default;
            this.handlesGenerated = defaultMLineStyleStyle.AsignHandle(this.handlesGenerated);
            this.mLineStyles.Add(defaultMLineStyleStyle.Name, defaultMLineStyleStyle);

            // raster variables
            this.rasterVariables = new RasterVariables();
            this.handlesGenerated = this.rasterVariables.AsignHandle(this.handlesGenerated);

        }

        #endregion

    }
}