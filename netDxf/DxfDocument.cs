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
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Threading;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Represents a document to read and write dxf files.
    /// </summary>
    public class DxfDocument :
        DxfObject
    {
        #region private fields

        private string name;
        //dxf objects added to the document (key: handle, value: dxf object).
        internal Dictionary<string, DxfObject> AddedObjects;
        // keeps track of the number of handles generated
        internal long NumHandles;
        // keeps track of the dimension blocks generated
        internal int DimensionBlocksGenerated;
        // keeps track of the group names generated (this groups have the isUnnamed bool set to true)
        internal int GroupNamesGenerated;

        #region header

        private List<string> comments;
        private readonly HeaderVariables drawingVariables;

        #endregion

        #region tables

        private ApplicationRegistries appRegistries;
        private BlockRecords blocks;
        private DimensionStyles dimStyles;
        private Layers layers;
        private LineTypes lineTypes;
        private TextStyles textStyles;
        private UCSs ucss;
        private Views views;
        private VPorts vports;

        #endregion

        #region entities

        private readonly List<Arc> arcs;
        private readonly List<Circle> circles;
        private readonly List<Dimension> dimensions;
        private readonly List<Ellipse> ellipses;
        private readonly List<Solid> solids;
        private readonly List<Face3d> faces3d;
        private readonly List<Insert> inserts;
        private readonly List<Line> lines;
        private readonly List<Point> points;
        private readonly List<PolyfaceMesh> polyfaceMeshes;
        private readonly List<LwPolyline> lwPolylines;
        private readonly List<Polyline> polylines;
        private readonly List<Text> texts;
        private readonly List<MText> mTexts;
        private readonly List<Hatch> hatches;
        private readonly List<Spline> splines;
        private readonly List<Image> images;
        private readonly List<MLine> mLines;
        private readonly List<Ray> rays;
        private readonly List<XLine> xlines;
        private readonly List<Viewport> viewports;

        #endregion

        #region objects

        private MLineStyles mlineStyles;
        private ImageDefinitions imageDefs;
        private Groups groups;
        private Layouts layouts;
        private string activeLayout;
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
            : this(new HeaderVariables {AcadVer = version})
        {
        }

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        public DxfDocument(HeaderVariables drawingVariables)
            : this(drawingVariables, true)
        {
        }

        /// <summary>
        /// Initalizes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        internal DxfDocument(HeaderVariables drawingVariables, bool createDefaultObjects)
            : base("DOCUMENT")
        {
            this.comments = new List<string> {"Dxf file generated by netDxf http://netdxf.codeplex.com, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL"};
            this.drawingVariables = drawingVariables;
            this.NumHandles = base.AsignHandle(0);
            this.DimensionBlocksGenerated = 0;
            this.GroupNamesGenerated = 0;
            this.AddedObjects = new Dictionary<string, DxfObject>
                {
                    {this.handle, this}
                }; // keeps track of the added objects

            this.activeLayout = "Model";

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
            this.rays = new List<Ray>();
            this.xlines = new List<XLine>();
            this.viewports = new List<Viewport>();

            if(createDefaultObjects) this.AddDefaultObjects();

        }

        #endregion

        #region public properties

        #region header

        /// <summary>
        /// Gets or sets the name of the document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public List<string> Comments
        {
            get { return this.comments; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                this.comments = value;
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
        /// Gets the name of the document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        #endregion

        #region  public collection properties

        /// <summary>
        /// Gets the <see cref="ApplicationRegistries">application registries</see> collection.
        /// </summary>
        public ApplicationRegistries ApplicationRegistries
        {
            get { return this.appRegistries; }
            internal set { this.appRegistries = value; }
        }

        /// <summary>
        /// Gets the <see cref="Layers">layer</see> collection.
        /// </summary>
        public Layers Layers
        {
            get { return this.layers; }
            internal set { this.layers = value; }
        }

        /// <summary>
        /// Gets the <see cref="LineTypes">linetype</see> collection.
        /// </summary>
        public LineTypes LineTypes
        {
            get { return this.lineTypes; }
            internal set { this.lineTypes = value; }
        }

        /// <summary>
        /// Gets the <see cref="TextStyles">text style</see> collection.
        /// </summary>
        public TextStyles TextStyles
        {
            get { return this.textStyles; }
            internal set { this.textStyles = value; }
        }

        /// <summary>
        /// Gets the <see cref="DimensionStyles">dimension style</see> collection.
        /// </summary>
        public DimensionStyles DimensionStyles
        {
            get { return this.dimStyles; }
            internal set { this.dimStyles = value; }
        }

        /// <summary>
        /// Gets the <see cref="MLineStyles">MLine styles</see> collection.
        /// </summary>
        public MLineStyles MlineStyles
        {
            get { return this.mlineStyles; }
            internal set { this.mlineStyles = value; }
        }

        /// <summary>
        /// Gets the <see cref="UCSs">User coordinate system</see> collection.
        /// </summary>
        public UCSs UCSs
        {
            get { return this.ucss; }
            internal set { this.ucss = value; }
        }

        /// <summary>
        /// Gets the <see cref="BlockRecords">block</see> collection.
        /// </summary>
        public BlockRecords Blocks
        {
            get { return this.blocks; }
            internal set { this.blocks = value; }
        }

        /// <summary>
        /// Gets the <see cref="ImageDefinitions">image definitions</see> collection.
        /// </summary>
        public ImageDefinitions ImageDefinitions
        {
            get { return this.imageDefs; }
            internal set { this.imageDefs = value; }
        }

        /// <summary>
        /// Gets the <see cref="Groups">groups</see> collection.
        /// </summary>
        public Groups Groups
        {
            get { return this.groups; }
            internal set { this.groups = value; }
        }

        /// <summary>
        /// Gets the <see cref="Layouts">layouts</see> collection.
        /// </summary>
        public Layouts Layouts
        {
            get { return this.layouts; }
            internal set { this.layouts = value; }
        }

        /// <summary>
        /// Gets the <see cref="VPorts">vports</see> collection.
        /// </summary>
        public VPorts VPorts
        {
            get { return this.vports; }
            internal set { this.vports = value; }
        }

        /// <summary>
        /// Gets the <see cref="Views">views</see> collection.
        /// </summary>
        public Views Views
        {
            get { return this.views; }
            internal set { this.views = value; }
        }

        #endregion

        #region public entities properties

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

        /// <summary>
        /// Gets the <see cref="Dimension">dimensions</see> list.
        /// </summary>
        public ReadOnlyCollection<Dimension> Dimensions
        {
            get { return this.dimensions.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Splines">splines</see> list.
        /// </summary>
        public ReadOnlyCollection<Spline> Splines
        {
            get { return this.splines.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Ray">rays</see> list.
        /// </summary>
        public ReadOnlyCollection<Ray> Rays
        {
            get { return this.rays.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="Viewport">viewports</see> list.
        /// </summary>
        public ReadOnlyCollection<Viewport> Viewports
        {
            get { return this.viewports.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the <see cref="XLine">xlines</see> list.
        /// </summary>
        public ReadOnlyCollection<XLine> XLines
        {
            get { return this.xlines.AsReadOnly(); }
        }

        #endregion

        #region public object properties

        /// <summary>
        /// Gets or sets the name of the active layout.
        /// </summary>
        public string ActiveLayout
        {
            get { return this.activeLayout; }
            set
            {
                if(!this.layouts.Contains(value))
                    throw new ArgumentException("The layout " + value + " does not exist.", "value");       
                this.activeLayout = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="RasterVariables">raster variables</see> applied to image entities.
        /// </summary>
        public RasterVariables RasterVariables
        {
            get { return this.rasterVariables; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                if(string.IsNullOrEmpty(value.Handle))
                    this.NumHandles = value.AsignHandle(this.NumHandles);
                this.AddedObjects.Add(value.Handle, value);
                this.rasterVariables = value;
            }
        }

        #endregion

        #endregion

        #region public entity methods

        /// <summary>
        /// Gets a dxf object by its handle.
        /// </summary>
        /// <param name="objectHandle">DxfObject handle.</param>
        /// <returns>The DxfObject that has the provided handle, null otherwise.</returns>
        public DxfObject GetObjectByHandle(string objectHandle)
        {
            if(string.IsNullOrEmpty(objectHandle))
                return null;

            DxfObject o;
            this.AddedObjects.TryGetValue(objectHandle, out o);
            return o;
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
            return this.AddEntity(entity, false, true);
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
                this.RemoveEntity(entity, false);
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
            return this.RemoveEntity(entity, false);
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
                stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                throw new IOException("Error trying to open the file " + fileInfo.FullName + " for reading.", ex);
            }

            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfReader dxfReader = new DxfReader();

#if DEBUG
            DxfDocument document = dxfReader.Read(stream);
            stream.Close();
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            DxfDocument document;
            try
            {
                 document = dxfReader.Read(stream);
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
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public static DxfDocument Load(Stream stream)
        {
            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfReader dxfReader = new DxfReader();

#if DEBUG
            DxfDocument document = dxfReader.Read(stream);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            DxfDocument document;
            try
            {
                 document = dxfReader.Read(stream);
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

            DxfWriter dxfWriter = new DxfWriter();

            Stream stream = File.Create(file);

#if DEBUG
            dxfWriter.Write(stream, this);
            stream.Close();
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            try
            {
                dxfWriter.Save(stream, this);
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
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public bool Save(Stream stream)
        {
            // In dxf files the decimal point is always a dot. We have to make sure that this doesn't interfere with the system configuration.
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            DxfWriter dxfWriter = new DxfWriter();

#if DEBUG
            dxfWriter.Write(stream, this);
            Thread.CurrentThread.CurrentCulture = cultureInfo;
#else
            try
            {
                dxfWriter.Save(stream, this);
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
        /// <remarks>The caller will be responsible of closing the stream.</remarks>
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
        public static DxfVersion CheckDxfFileVersion(string file)
        {
            Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

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

            return (DxfVersion) StringEnum.Parse(typeof (DxfVersion), value);
        }

        #endregion

        #region private methods

        internal bool AddEntity(EntityObject entity, bool isBlockEntity, bool assignHandle)
        {
            // no null entities allowed
            if (entity == null)
                return false;

            if (!string.IsNullOrEmpty(entity.Handle) && !assignHandle)
            {
                // check if the entity handle has been assigned
                if (this.AddedObjects.ContainsKey(entity.Handle))
                {
                    // if the handle is equal the entity might come from another document, check if it is exactly the same object
                    DxfObject existing = this.AddedObjects[entity.Handle];
                    // if the entity is already in the document return false, do not add it again
                    if (existing.Equals(entity))
                        return false;
                }
            }
            else
            {
                this.NumHandles = entity.AsignHandle(this.NumHandles);
            }

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition.
            switch (entity.Type)
            {
                case EntityType.Arc:
                    if (!isBlockEntity) this.arcs.Add((Arc) entity);
                    break;
                case EntityType.Circle:
                    if (!isBlockEntity) this.circles.Add((Circle) entity);
                    break;
                case EntityType.Dimension:
                    if (!isBlockEntity) this.dimensions.Add((Dimension) entity);
                    ((Dimension) entity).Style = this.dimStyles.Add(((Dimension) entity).Style);
                    this.dimStyles.References[((Dimension) entity).Style.Name].Add(entity);

                    // create the block that represent the dimension drawing
                    Block dimBlock = ((Dimension) entity).Block;
                    if (dimBlock == null)
                    {
                        dimBlock = ((Dimension) entity).BuildBlock("*D" + ++this.DimensionBlocksGenerated);
                        if (this.blocks.Contains(dimBlock.Name))
                            throw new ArgumentException("The list already contains the block: " + dimBlock.Name + ". The block names that start with *D are reserverd for dimensions");
                        dimBlock.Flags = BlockTypeFlags.AnonymousBlock;
                    }
                    this.blocks.Add(dimBlock);
                    this.blocks.References[dimBlock.Name].Add(entity);
                    break;
                case EntityType.Ellipse:
                    if (!isBlockEntity) this.ellipses.Add((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    if (!isBlockEntity) this.faces3d.Add((Face3d) entity);
                    break;
                case EntityType.Spline:
                    if (!isBlockEntity) this.splines.Add((Spline) entity);
                    break;
                case EntityType.Hatch:
                    Hatch hatch = (Hatch) entity;
                    HatchPatternXData(hatch);
                    if (!isBlockEntity) this.hatches.Add(hatch);
                    break;
                case EntityType.Insert:
                    ((Insert) entity).Block = this.blocks.Add(((Insert) entity).Block);
                    this.blocks.References[((Insert) entity).Block.Name].Add(entity);
                    foreach (Attribute attribute in ((Insert) entity).Attributes.Values)
                    {
                        attribute.Layer = this.layers.Add(attribute.Layer);
                        this.layers.References[attribute.Layer.Name].Add(attribute);

                        attribute.LineType = this.lineTypes.Add(attribute.LineType);
                        this.lineTypes.References[attribute.LineType.Name].Add(attribute);

                        attribute.Style = this.textStyles.Add(attribute.Style);
                        this.textStyles.References[attribute.Style.Name].Add(attribute);
                    }
                    if (!isBlockEntity) this.inserts.Add((Insert) entity);
                    break;
                case EntityType.LightWeightPolyline:
                    if (!isBlockEntity) this.lwPolylines.Add((LwPolyline) entity);
                    break;
                case EntityType.Line:
                    if (!isBlockEntity) this.lines.Add((Line) entity);
                    break;
                case EntityType.Point:
                    if (!isBlockEntity) this.points.Add((Point) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    if (!isBlockEntity) this.polyfaceMeshes.Add((PolyfaceMesh) entity);
                    break;
                case EntityType.Polyline:
                    if (!isBlockEntity) this.polylines.Add((Polyline) entity);
                    break;
                case EntityType.Solid:
                    if (!isBlockEntity) this.solids.Add((Solid) entity);
                    break;
                case EntityType.Text:
                    ((Text) entity).Style = this.textStyles.Add(((Text) entity).Style);
                    this.textStyles.References[((Text) entity).Style.Name].Add(entity);
                    if (!isBlockEntity) this.texts.Add((Text) entity);
                    break;
                case EntityType.MText:
                    ((MText) entity).Style = this.textStyles.Add(((MText) entity).Style);
                    this.textStyles.References[((MText) entity).Style.Name].Add(entity);
                    if (!isBlockEntity) this.mTexts.Add((MText) entity);
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    image.Definition = this.imageDefs.Add(image.Definition);
                    this.imageDefs.References[image.Definition.Name].Add(image);
                    if(!image.Definition.Reactors.ContainsKey(image.Handle))
                    {
                        ImageDefReactor reactor = new ImageDefReactor(image.Handle);
                        this.NumHandles = reactor.AsignHandle(this.NumHandles);
                        image.Definition.Reactors.Add(image.Handle, reactor);
                    }
                    if (!isBlockEntity) this.images.Add(image);
                    break;
                case EntityType.MLine:
                    ((MLine) entity).Style = this.mlineStyles.Add(((MLine) entity).Style);
                    this.mlineStyles.References[((MLine) entity).Style.Name].Add(entity);
                    if (!isBlockEntity) this.mLines.Add((MLine) entity);
                    break;
                case EntityType.Ray:
                    if (!isBlockEntity) this.rays.Add((Ray) entity);
                    break;
                case EntityType.XLine:
                    if (!isBlockEntity) this.xlines.Add((XLine) entity);
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport) entity;
                    if (!isBlockEntity) this.viewports.Add(viewport);
                    this.AddEntity(viewport.ClippingBoundary, isBlockEntity, assignHandle);
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of block definition.", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of block definition.", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown.");
            }

            foreach (string appReg in entity.XData.Keys)
            {
                entity.XData[appReg].ApplicationRegistry = this.appRegistries.Add(entity.XData[appReg].ApplicationRegistry);
                this.appRegistries.References[appReg].Add(entity);
            }

            if (!isBlockEntity)
            {
                entity.Owner = this.layouts[this.activeLayout].AssociatedBlock;
                this.layouts.References[this.activeLayout].Add(entity);
            }

            this.AddedObjects.Add(entity.Handle, entity);

            entity.Layer = this.layers.Add(entity.Layer);
            this.layers.References[entity.Layer.Name].Add(entity);

            entity.LineType = this.lineTypes.Add(entity.LineType);
            this.lineTypes.References[entity.LineType.Name].Add(entity);
            return true;
        }

        internal bool RemoveEntity(EntityObject entity, bool isBlockEntity)
        {
            if (entity == null)
                return false;

            if (entity.Reactor != null)
                return false;

            if (!this.AddedObjects.ContainsKey(entity.Handle))
                return false;

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition
            // and they will not be removed from the drawing database
            bool removed;

            switch (entity.Type)
            {
                case EntityType.Arc:
                    removed = this.arcs.Remove((Arc) entity);
                    break;
                case EntityType.Circle:
                    removed = this.circles.Remove((Circle) entity);
                    break;
                case EntityType.Dimension:
                    removed = this.dimensions.Remove((Dimension) entity);
                    if (removed || isBlockEntity)
                    {
                        this.dimStyles.References[((Dimension) entity).Style.Name].Remove(entity);
                        this.blocks.References[((Dimension) entity).Block.Name].Remove(entity);
                    }
                    break;
                case EntityType.Ellipse:
                    removed = this.ellipses.Remove((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    removed = this.faces3d.Remove((Face3d) entity);
                    break;
                case EntityType.Spline:
                    removed = this.splines.Remove((Spline) entity);
                    break;
                case EntityType.Hatch:
                    removed = this.hatches.Remove((Hatch) entity);
                    break;
                case EntityType.Insert:
                    removed = this.inserts.Remove((Insert) entity);
                    if (removed || isBlockEntity)
                    {
                        this.blocks.References[((Insert) entity).Block.Name].Remove(entity);
                        foreach (Attribute att in ((Insert) entity).Attributes.Values)
                        {
                            this.layers.References[att.Layer.Name].Remove(att);
                            this.lineTypes.References[att.LineType.Name].Remove(att);
                            this.textStyles.References[att.Style.Name].Remove(att);
                        }
                    }
                    break;
                case EntityType.LightWeightPolyline:
                    removed = this.lwPolylines.Remove((LwPolyline) entity);
                    break;
                case EntityType.Line:
                    removed = this.lines.Remove((Line) entity);
                    break;
                case EntityType.Point:
                    removed = this.points.Remove((Point) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    removed = this.polyfaceMeshes.Remove((PolyfaceMesh) entity);
                    break;
                case EntityType.Polyline:
                    removed = this.polylines.Remove((Polyline) entity);
                    break;
                case EntityType.Solid:
                    removed = this.solids.Remove((Solid) entity);
                    break;
                case EntityType.Text:
                    this.textStyles.References[((Text) entity).Style.Name].Remove(entity);
                    removed = this.texts.Remove((Text) entity);
                    break;
                case EntityType.MText:
                    this.textStyles.References[((MText) entity).Style.Name].Remove(entity);
                    removed = this.mTexts.Remove((MText) entity);
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    removed = this.images.Remove(image);
                    if (removed || isBlockEntity)
                    {
                        this.imageDefs.References[image.Definition.Name].Remove(image);
                        image.Definition.Reactors.Remove(image.Handle);
                    }
                    break;
                case EntityType.MLine:
                    removed = this.mLines.Remove((MLine) entity);
                    if (removed || isBlockEntity)
                        this.mlineStyles.References[((MLine) entity).Style.Name].Remove(entity);
                    break;
                case EntityType.Ray:
                    removed = this.rays.Remove((Ray) entity);
                    break;
                case EntityType.XLine:
                    removed = this.xlines.Remove((XLine) entity);
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport) entity;
                    removed = this.viewports.Remove(viewport);
                    // delete the viewport boundary entity in case there is one
                    if (removed && viewport.ClippingBoundary != null)
                    {
                        viewport.ClippingBoundary.Reactor = null;
                        RemoveEntity(viewport.ClippingBoundary);
                    }
                    break;
                case EntityType.AttributeDefinition:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                case EntityType.Attribute:
                    throw new ArgumentException("The entity " + entity.Type + " is only allowed as part of another entity", "entity");

                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }

            if (removed || isBlockEntity)
            {
                this.layouts.References[entity.Owner.Record.Layout.Name].Remove(entity);
                this.layers.References[entity.Layer.Name].Remove(entity);
                this.lineTypes.References[entity.LineType.Name].Remove(entity);
                foreach (string appReg in entity.XData.Keys)
                {
                    this.appRegistries.References[appReg].Remove(entity);
                }
                this.AddedObjects.Remove(entity.Handle);

                entity.Owner = null;
            }

            return removed;
        }

        private void AddDefaultObjects()
        {
            // collections
            this.vports = new VPorts(this);
            this.views = new Views(this);
            this.appRegistries = new ApplicationRegistries(this);
            this.layers = new Layers(this);
            this.lineTypes = new LineTypes(this);
            this.textStyles = new TextStyles(this);
            this.dimStyles = new DimensionStyles(this);
            this.mlineStyles = new MLineStyles(this);
            this.ucss = new UCSs(this);
            this.blocks = new BlockRecords(this);
            this.imageDefs = new ImageDefinitions(this);
            this.groups = new Groups(this);
            this.layouts = new Layouts(this);

            //add default viewport
            this.vports.Add(VPort.Active);

            //add default layer
            this.layers.Add(Layer.Default);

            // add default line types
            this.lineTypes.Add(LineType.ByLayer);
            this.lineTypes.Add(LineType.ByBlock);
            this.lineTypes.Add(LineType.Continuous);

            // add default text style
            this.textStyles.Add(TextStyle.Default);

            // add default application registry
            this.appRegistries.Add(ApplicationRegistry.Default);

            // add default dimension style
            this.dimStyles.Add(DimensionStyle.Default);

            // add default MLine style
            this.mlineStyles.Add(MLineStyle.Default);

            // add ModelSpace layout
            this.layouts.Add(Layout.ModelSpace);

            // raster variables
            this.RasterVariables = new RasterVariables();

        }

        private void HatchPatternXData(Hatch hatch)
        {
            if (hatch.XData.ContainsKey(ApplicationRegistry.Default.Name))
            {
                XData xdataEntry = hatch.XData[ApplicationRegistry.Default.Name];
                xdataEntry.XDataRecord.Clear();
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealX, hatch.Pattern.Origin.X));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealY, hatch.Pattern.Origin.Y));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealZ, 0.0));
            }
            else
            {
                XData xdataEntry = new XData(new ApplicationRegistry(ApplicationRegistry.Default.Name));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealX, hatch.Pattern.Origin.X));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealY, hatch.Pattern.Origin.Y));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.RealZ, 0.0));
                hatch.XData.Add(xdataEntry.ApplicationRegistry.Name, xdataEntry);
            }

            if (!(hatch.Pattern is HatchGradientPattern)) return;

            HatchGradientPattern grad = (HatchGradientPattern)hatch.Pattern;
            if (hatch.XData.ContainsKey("GradientColor1ACI"))
            {
                XData xdataEntry = hatch.XData["GradientColor1ACI"];
                XDataRecord record = new XDataRecord(XDataCode.Integer, grad.Color1.Index);
                xdataEntry.XDataRecord.Clear();
                xdataEntry.XDataRecord.Add(record);
            }
            else
            {
                XData xdataEntry = new XData(new ApplicationRegistry("GradientColor1ACI"));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Integer, grad.Color1.Index));
                hatch.XData.Add(xdataEntry.ApplicationRegistry.Name, xdataEntry);
            }

            if (hatch.XData.ContainsKey("GradientColor2ACI"))
            {
                XData xdataEntry = hatch.XData["GradientColor2ACI"];
                XDataRecord record = new XDataRecord(XDataCode.Integer, grad.Color2.Index);
                xdataEntry.XDataRecord.Clear();
                xdataEntry.XDataRecord.Add(record);
            }
            else
            {
                XData xdataEntry = new XData(new ApplicationRegistry("GradientColor2ACI"));
                xdataEntry.XDataRecord.Add(new XDataRecord(XDataCode.Integer, grad.Color2.Index));
                hatch.XData.Add(xdataEntry.ApplicationRegistry.Name, xdataEntry);
            }
        }

        #endregion
    }
}