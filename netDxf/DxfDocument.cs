#region netDxf library, Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2016 Daniel Carvajal (haplokuon@gmail.com)
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
using System.IO;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.IO;
using netDxf.Objects;
using netDxf.Tables;
using Attribute = netDxf.Entities.Attribute;

namespace netDxf
{
    /// <summary>
    /// Represents a document to read and write dxf files.
    /// </summary>
    public sealed class DxfDocument :
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

        private readonly List<string> comments;
        private readonly HeaderVariables drawingVariables;

        #endregion

        #region tables

        private ApplicationRegistries appRegistries;
        private BlockRecords blocks;
        private DimensionStyles dimStyles;
        private Layers layers;
        private Linetypes linetypes;
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
        private readonly List<Trace> traces;
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
        private readonly List<Mesh> meshes;
        private readonly List<Leader> leaders;
        private readonly List<Tolerance> tolerances;
        private readonly List<Underlay> underlays;
        private readonly List<Wipeout> wipeouts;
        private readonly List<AttributeDefinition> attributeDefinitions;

        #endregion

        #region objects

        private MLineStyles mlineStyles;
        private ImageDefinitions imageDefs;
        private UnderlayDgnDefinitions underlayDgnDefs;
        private UnderlayDwfDefinitions underlayDwfDefs;
        private UnderlayPdfDefinitions underlayPdfDefs;
        private Groups groups;
        private Layouts layouts;
        private string activeLayout;
        private RasterVariables rasterVariables;

        #endregion

        #endregion

        #region constructor

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <remarks>The default <see cref="HeaderVariables">drawing variables</see> of the document will be used.</remarks>
        public DxfDocument()
            : this(new HeaderVariables())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="version">AutoCAD drawing database version number.</param>
        public DxfDocument(DxfVersion version)
            : this(new HeaderVariables {AcadVer = version})
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        public DxfDocument(HeaderVariables drawingVariables)
            : this(drawingVariables, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        /// <param name="createDefaultObjects">Check if the default objects need to be created.</param>
        internal DxfDocument(HeaderVariables drawingVariables, bool createDefaultObjects)
            : base("DOCUMENT")
        {
            this.comments = new List<string> {"Dxf file generated by netDxf https://netdxf.codeplex.com, Copyright(C) 2009-2016 Daniel Carvajal, Licensed under LGPL"};
            this.Owner = null;
            this.drawingVariables = drawingVariables;
            this.NumHandles = this.AsignHandle(0);
            this.DimensionBlocksGenerated = 0;
            this.GroupNamesGenerated = 0;
            this.AddedObjects = new Dictionary<string, DxfObject>
            {
                {this.Handle, this}
            }; // keeps track of the added objects

            this.activeLayout = Layout.ModelSpaceName;

            // entities lists
            this.arcs = new List<Arc>();
            this.ellipses = new List<Ellipse>();
            this.dimensions = new List<Dimension>();
            this.faces3d = new List<Face3d>();
            this.solids = new List<Solid>();
            this.traces = new List<Trace>();
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
            this.meshes = new List<Mesh>();
            this.leaders = new List<Leader>();
            this.tolerances = new List<Tolerance>();
            this.underlays = new List<Underlay>();
            this.wipeouts = new List<Wipeout>();
            this.attributeDefinitions = new List<AttributeDefinition>();

            if (createDefaultObjects)
                this.AddDefaultObjects();
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
        }

        /// <summary>
        /// Gets the dxf <see cref="HeaderVariables">drawing variables</see>.
        /// </summary>
        public HeaderVariables DrawingVariables
        {
            get { return this.drawingVariables; }
        }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        /// <remarks>
        /// When a file is loaded this field is equals the file name without extension.<br />
        /// </remarks>
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
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
        /// Gets the <see cref="Linetypes">line type</see> collection.
        /// </summary>
        public Linetypes Linetypes
        {
            get { return this.linetypes; }
            internal set { this.linetypes = value; }
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
        /// Gets the <see cref="UnderlayDgnDefinitions">dgn underlay definitions</see> collection.
        /// </summary>
        public UnderlayDgnDefinitions UnderlayDgnDefinitions
        {
            get { return this.underlayDgnDefs; }
            internal set { this.underlayDgnDefs = value; }
        }

        /// <summary>
        /// Gets the <see cref="UnderlayDwfDefinitions">dwf underlay definitions</see> collection.
        /// </summary>
        public UnderlayDwfDefinitions UnderlayDwfDefinitions
        {
            get { return this.underlayDwfDefs; }
            internal set { this.underlayDwfDefs = value; }
        }

        /// <summary>
        /// Gets the <see cref="UnderlayPdfDefinitions">pdf underlay definitions</see> collection.
        /// </summary>
        public UnderlayPdfDefinitions UnderlayPdfDefinitions
        {
            get { return this.underlayPdfDefs; }
            internal set { this.underlayPdfDefs = value; }
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
        /// Gets the <see cref="VPorts">viewports</see> collection.
        /// </summary>
        public VPorts VPorts
        {
            get { return this.vports; }
            internal set { this.vports = value; }
        }

        /// <summary>
        /// Gets the <see cref="Views">views</see> collection.
        /// </summary>
        internal Views Views
        {
            get { return this.views; }
            set { this.views = value; }
        }

        #endregion

        #region public entities properties

        /// <summary>
        /// Gets the <see cref="Arc">arcs</see> list.
        /// </summary>
        public IReadOnlyList<Arc> Arcs
        {
            get { return this.arcs; }
        }

        /// <summary>
        /// Gets the <see cref="AttributeDefinition">attribute definitions</see> list.
        /// </summary>
        public IReadOnlyList<AttributeDefinition> AttributeDefinitions
        {
            get { return this.attributeDefinitions; }
        }

        /// <summary>
        /// Gets the <see cref="Ellipse">ellipses</see> list.
        /// </summary>
        public IReadOnlyList<Ellipse> Ellipses
        {
            get { return this.ellipses; }
        }

        /// <summary>
        /// Gets the <see cref="Circle">circles</see> list.
        /// </summary>
        public IReadOnlyList<Circle> Circles
        {
            get { return this.circles; }
        }

        /// <summary>
        /// Gets the <see cref="Face3d">3d faces</see> list.
        /// </summary>
        public IReadOnlyList<Face3d> Faces3d
        {
            get { return this.faces3d; }
        }

        /// <summary>
        /// Gets the <see cref="Solid">solids</see> list.
        /// </summary>
        public IReadOnlyList<Solid> Solids
        {
            get { return this.solids; }
        }

        /// <summary>
        /// Gets the <see cref="Trace">traces</see> list.
        /// </summary>
        public IReadOnlyList<Trace> Traces
        {
            get { return this.traces; }
        }

        /// <summary>
        /// Gets the <see cref="Insert">inserts</see> list.
        /// </summary>
        public IReadOnlyList<Insert> Inserts
        {
            get { return this.inserts; }
        }

        /// <summary>
        /// Gets the <see cref="Line">lines</see> list.
        /// </summary>
        public IReadOnlyList<Line> Lines
        {
            get { return this.lines; }
        }

        /// <summary>
        /// Gets the <see cref="Polyline">polylines</see> list.
        /// </summary>
        public IReadOnlyList<Polyline> Polylines
        {
            get { return this.polylines; }
        }

        /// <summary>
        /// Gets the <see cref="LwPolyline">light weight polylines</see> list.
        /// </summary>
        public IReadOnlyList<LwPolyline> LwPolylines
        {
            get { return this.lwPolylines; }
        }

        /// <summary>
        /// Gets the <see cref="PolyfaceMeshes">polyface meshes</see> list.
        /// </summary>
        public IReadOnlyList<PolyfaceMesh> PolyfaceMeshes
        {
            get { return this.polyfaceMeshes; }
        }

        /// <summary>
        /// Gets the <see cref="Point">points</see> list.
        /// </summary>
        public IReadOnlyList<Point> Points
        {
            get { return this.points; }
        }

        /// <summary>
        /// Gets the <see cref="Text">texts</see> list.
        /// </summary>
        public IReadOnlyList<Text> Texts
        {
            get { return this.texts; }
        }

        /// <summary>
        /// Gets the <see cref="MText">multiline texts</see> list.
        /// </summary>
        public IReadOnlyList<MText> MTexts
        {
            get { return this.mTexts; }
        }

        /// <summary>
        /// Gets the <see cref="Hatch">hatches</see> list.
        /// </summary>
        public IReadOnlyList<Hatch> Hatches
        {
            get { return this.hatches; }
        }

        /// <summary>
        /// Gets the <see cref="Image">images</see> list.
        /// </summary>
        public IReadOnlyList<Image> Images
        {
            get { return this.images; }
        }

        /// <summary>
        /// Gets the <see cref="Mesh">mesh</see> list.
        /// </summary>
        public IReadOnlyList<Mesh> Meshes
        {
            get { return this.meshes; }
        }

        /// <summary>
        /// Gets the <see cref="Leader">leader</see> list.
        /// </summary>
        public IReadOnlyList<Leader> Leaders
        {
            get { return this.leaders; }
        }

        /// <summary>
        /// Gets the <see cref="Tolerance">tolerance</see> list.
        /// </summary>
        public IReadOnlyList<Tolerance> Tolerances
        {
            get { return this.tolerances; }
        }

        /// <summary>
        /// Gets the <see cref="Underlay">underlay</see> list.
        /// </summary>
        public IReadOnlyList<Underlay> Underlays
        {
            get { return this.underlays; }
        }

        /// <summary>
        /// Gets the <see cref="MLine">multilines</see> list.
        /// </summary>
        public IReadOnlyList<MLine> MLines
        {
            get { return this.mLines; }
        }

        /// <summary>
        /// Gets the <see cref="Dimension">dimensions</see> list.
        /// </summary>
        public IReadOnlyList<Dimension> Dimensions
        {
            get { return this.dimensions; }
        }

        /// <summary>
        /// Gets the <see cref="Spline">splines</see> list.
        /// </summary>
        public IReadOnlyList<Spline> Splines
        {
            get { return this.splines; }
        }

        /// <summary>
        /// Gets the <see cref="Ray">rays</see> list.
        /// </summary>
        public IReadOnlyList<Ray> Rays
        {
            get { return this.rays; }
        }

        /// <summary>
        /// Gets the <see cref="Viewport">viewports</see> list.
        /// </summary>
        public IReadOnlyList<Viewport> Viewports
        {
            get { return this.viewports; }
        }

        /// <summary>
        /// Gets the <see cref="XLine">extension lines</see> list.
        /// </summary>
        public IReadOnlyList<XLine> XLines
        {
            get { return this.xlines; }
        }

        /// <summary>
        /// Gets the <see cref="Wipeout">wipeouts</see> list.
        /// </summary>
        public IReadOnlyList<Wipeout> Wipeouts
        {
            get { return this.wipeouts; }
        }

        #endregion

        #region public object properties

        /// <summary>
        /// Gets the document viewport.
        /// </summary>
        /// <remarks>
        /// This is the same as the *Active VPort in the VPorts list, it describes the current viewport.
        /// </remarks>
        public VPort Viewport
        {
            get { return this.vports["*Active"]; }
        }

        /// <summary>
        /// Gets or sets the name of the active layout.
        /// </summary>
        public string ActiveLayout
        {
            get { return this.activeLayout; }
            set
            {
                if (!this.layouts.Contains(value))
                    throw new ArgumentException("The layout " + value + " does not exist.", nameof(value));
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
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (string.IsNullOrEmpty(value.Handle))
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
            if (string.IsNullOrEmpty(objectHandle))
                return null;

            DxfObject o;
            this.AddedObjects.TryGetValue(objectHandle, out o);
            return o;
        }

        /// <summary>
        /// Adds a list of <see cref="EntityObject">entities</see> to the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to add to the document.</param>
        public void AddEntity(IEnumerable<EntityObject> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

            foreach (EntityObject entity in entities)
            {
                this.AddEntity(entity);
            }
        }

        /// <summary>
        /// Adds an <see cref="EntityObject">entity</see> to the document.
        /// </summary>
        /// <param name="entity">An <see cref="EntityObject">entity</see> to add to the document.</param>
        public void AddEntity(EntityObject entity)
        {
            this.AddEntity(entity, false, true);
        }

        /// <summary>
        /// Removes a list of <see cref="EntityObject">entities</see> from the document.
        /// </summary>
        /// <param name="entities">A list of <see cref="EntityObject">entities</see> to remove from the document.</param>
        /// <remarks>
        /// This function will not remove other tables objects that might be not in use as result from the elimination of the entity.<br />
        /// This includes empty layers, blocks not referenced anymore, line types, text styles, dimension styles, and application registries.<br />
        /// Entities that are part of a block definition will not be removed.
        /// </remarks>
        public void RemoveEntity(IEnumerable<EntityObject> entities)
        {
            if (entities == null)
                throw new ArgumentNullException(nameof(entities));

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
        /// This includes empty layers, blocks not referenced anymore, line types, text styles, dimension styles, multiline styles, groups, and application registries.<br />
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
        /// On Debug mode it will raise any exception that might occur during the whole process.
        /// </remarks>
        public static DxfDocument Load(string file)
        {
            FileInfo fileInfo = new FileInfo(file);
            if (!fileInfo.Exists)
                throw new FileNotFoundException(string.Format("File {0} not found.", fileInfo.FullName), fileInfo.FullName);

            Stream stream;
            try
            {
                stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                throw new IOException(string.Format("Error trying to open the file {0} for reading.", fileInfo.FullName), ex);
            }

            DxfReader dxfReader = new DxfReader();

#if DEBUG
            DxfDocument document = dxfReader.Read(stream);
            stream.Close();
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
            DxfReader dxfReader = new DxfReader();

#if DEBUG
            DxfDocument document = dxfReader.Read(stream);
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

#endif
            return document;
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a text dxf file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Return true if the file has been successfully save, false otherwise.</returns>
        /// <remarks>
        /// If the file already exists it will be overwritten.<br />
        /// The Save method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode they will raise any exception that might occur during the whole process.
        /// </remarks>
        public bool Save(string file)
        {
            return this.Save(file, false);
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a dxf file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="isBinary">Defines if the file will be saved as binary.</param>
        /// <returns>Return true if the file has been successfully save, false otherwise.</returns>
        /// <remarks>
        /// If the file already exists it will be overwritten.<br />
        /// The Save method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode they will raise any exception that might occur during the whole process.
        /// </remarks>
        public bool Save(string file, bool isBinary)
        {
            FileInfo fileInfo = new FileInfo(file);
            this.name = Path.GetFileNameWithoutExtension(fileInfo.FullName);

            DxfWriter dxfWriter = new DxfWriter();

            Stream stream = File.Create(file);

#if DEBUG
            dxfWriter.Write(stream, this, isBinary);
            stream.Close();
#else
            try
            {
                dxfWriter.Write(stream, this, isBinary);
            }
            catch
            {
                return false;
            }
            finally
            {
                stream.Close();
            }
                
#endif
            return true;
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a text stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>Return true if the stream has been successfully saved, false otherwise.</returns>
        /// <remarks>
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public bool Save(Stream stream)
        {
            return this.Save(stream, false);
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="isBinary">Defines if the file will be saved as binary.</param>
        /// <returns>Return true if the stream has been successfully saved, false otherwise.</returns>
        /// <remarks>
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public bool Save(Stream stream, bool isBinary)
        {
            DxfWriter dxfWriter = new DxfWriter();

#if DEBUG
            dxfWriter.Write(stream, this, isBinary);
#else
            try
            {
                dxfWriter.Write(stream, this, isBinary);
            }
            catch
            {
                return false;
            }
                
#endif
            return true;
        }

        /// <summary>
        /// Checks the AutoCAD dxf file database version.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="isBinary">Returns true if the dxf is a binary file.</param>
        /// <returns>String that represents the dxf file version.</returns>
        /// <remarks>The caller will be responsible of closing the stream.</remarks>
        public static DxfVersion CheckDxfFileVersion(Stream stream, out bool isBinary)
        {
            string value = DxfReader.CheckHeaderVariable(stream, HeaderVariableCode.AcadVer, out isBinary);
            return (DxfVersion) StringEnum.Parse(typeof (DxfVersion), value);
        }

        /// <summary>
        /// Checks the AutoCAD dxf file database version.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="isBinary">Returns true if the dxf is a binary file.</param>
        /// <returns>String that represents the dxf file version.</returns>
        public static DxfVersion CheckDxfFileVersion(string file, out bool isBinary)
        {
            Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            string value;

            isBinary = false;

            try
            {
                value = DxfReader.CheckHeaderVariable(stream, HeaderVariableCode.AcadVer, out isBinary);
            }
            catch
            {
                return DxfVersion.Unknown;
            }
            finally
            {
                stream.Close();
            }

            if (string.IsNullOrEmpty(value))
                return DxfVersion.Unknown;

            return (DxfVersion) StringEnum.Parse(typeof (DxfVersion), value);
        }

        #endregion

        #region private methods

        private void AddDimensionStyleOverrides(Dimension dim, bool assignHandle)
        {
            // add the style override referenced DxfObjects
            DimensionStyleOverride styleOverride;

            // add referenced text style
            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.TextStyle, out styleOverride))
            {
                TextStyle dimtxtsty = (TextStyle) styleOverride.Value;
                dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.textStyles.Add(dimtxtsty, assignHandle));
                this.textStyles.References[dimtxtsty.Name].Add(dim);
            }

            // add referenced blocks
            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.LeaderArrow, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(dim);
                }
            }

            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimArrow1, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(dim);
                }
            }

            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimArrow2, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(dim);
                }
            }

            // add referenced line types
            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.DimLineLinetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(dim);
            }

            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ExtLine1Linetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(dim);
            }

            if (dim.StyleOverrides.TryGetValue(DimensionStyleOverrideType.ExtLine2Linetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                dim.StyleOverrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(dim);
            }
        }

        private void AddStyleOverrides(EntityObject entity, bool assignHandle)
        {
            DimensionStyleOverrideDictionary overrides;
            switch (entity.Type)
            {
                case EntityType.Dimension:
                    overrides = ((Dimension) entity).StyleOverrides;
                    break;
                case EntityType.Leader:
                    overrides = ((Leader) entity).StyleOverrides;
                    break;
                default:
                    return;
            }

            // add the style override referenced DxfObjects
            DimensionStyleOverride styleOverride;

            // add referenced text style
            overrides.TryGetValue(DimensionStyleOverrideType.TextStyle, out styleOverride);
            if (styleOverride != null)
            {
                TextStyle dimtxtsty = (TextStyle) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.textStyles.Add(dimtxtsty, assignHandle));
                this.textStyles.References[dimtxtsty.Name].Add(entity);
            }

            // add referenced blocks
            overrides.TryGetValue(DimensionStyleOverrideType.LeaderArrow, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            overrides.TryGetValue(DimensionStyleOverrideType.DimArrow1, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            overrides.TryGetValue(DimensionStyleOverrideType.DimArrow2, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            // add referenced line types
            overrides.TryGetValue(DimensionStyleOverrideType.DimLineLinetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }

            overrides.TryGetValue(DimensionStyleOverrideType.ExtLine1Linetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }

            overrides.TryGetValue(DimensionStyleOverrideType.ExtLine2Linetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }
        }

        internal void AddEntity(EntityObject entity, bool isBlockEntity, bool assignHandle)
        {
            // null entities are not allowed
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            // entities already owned by another document are not allowed
            if (entity.Owner != null && !isBlockEntity)
                throw new ArgumentException("The entity already belongs to a document. Clone it instead.", nameof(entity));

            // assign a handle
            if (assignHandle || string.IsNullOrEmpty(entity.Handle))
                this.NumHandles = entity.AsignHandle(this.NumHandles);

            // assign the owner
            if (!isBlockEntity)
            {
                entity.Owner = this.layouts[this.activeLayout].AssociatedBlock;
                this.layouts.References[this.activeLayout].Add(entity);
            }

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition.
            switch (entity.Type)
            {
                case EntityType.Arc:
                    if (!isBlockEntity)
                        this.arcs.Add((Arc) entity);
                    break;
                case EntityType.Circle:
                    if (!isBlockEntity)
                        this.circles.Add((Circle) entity);
                    break;
                case EntityType.Dimension:
                    Dimension dim = (Dimension) entity;
                    dim.Style = this.dimStyles.Add(dim.Style, assignHandle);
                    this.dimStyles.References[dim.Style.Name].Add(dim);

                    this.AddDimensionStyleOverrides(dim, assignHandle);

                    // create the block that represent the dimension drawing
                    Block dimBlock = dim.Block;
                    if (dimBlock == null)
                        dimBlock = dim.BuildBlock("*D" + ++this.DimensionBlocksGenerated);
                    dim.Block = this.blocks.Add(dimBlock);
                    this.blocks.References[dimBlock.Name].Add(dim);

                    dim.DimensionStyleChanged += this.Dimension_DimStyleChanged;
                    dim.DimensionBlockChanged += this.Dimension_DimBlockChanged;
                    dim.DimensionStyleOverrideAdded += this.Dimension_DimStyleOverrideAdded;
                    dim.DimensionStyleOverrideRemoved += this.Dimension_DimStyleOverrideRemoved;

                    if (!isBlockEntity)
                        this.dimensions.Add(dim);
                    break;
                case EntityType.Leader:
                    Leader leader = (Leader) entity;
                    leader.Style = this.dimStyles.Add(leader.Style, assignHandle);
                    this.dimStyles.References[leader.Style.Name].Add(leader);
                    leader.LeaderStyleChanged += this.Leader_DimStyleChanged;
                    // add the annotation entity
                    if (leader.Annotation != null)
                        this.AddEntity(leader.Annotation, isBlockEntity, assignHandle);

                    this.AddStyleOverrides(leader, assignHandle);
                    leader.DimensionStyleOverrideAdded += this.Leader_DimStyleOverrideAdded;
                    leader.DimensionStyleOverrideRemoved += this.Leader_DimStyleOverrideRemoved;

                    if (!isBlockEntity)
                        this.leaders.Add(leader);
                    break;
                case EntityType.Tolerance:
                    Tolerance tol = (Tolerance) entity;
                    tol.Style = this.dimStyles.Add(tol.Style, assignHandle);
                    this.dimStyles.References[tol.Style.Name].Add(tol);
                    tol.ToleranceStyleChanged += this.Tolerance_DimStyleChanged;
                    if (!isBlockEntity)
                        this.tolerances.Add(tol);
                    break;
                case EntityType.Ellipse:
                    if (!isBlockEntity)
                        this.ellipses.Add((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    if (!isBlockEntity)
                        this.faces3d.Add((Face3d) entity);
                    break;
                case EntityType.Spline:
                    if (!isBlockEntity)
                        this.splines.Add((Spline) entity);
                    break;
                case EntityType.Hatch:
                    Hatch hatch = (Hatch) entity;

                    // the boundary entities of an associative hatch that belong to a block will be handle by that block
                    if (!isBlockEntity)
                    {
                        foreach (HatchBoundaryPath path in hatch.BoundaryPaths)
                            this.Hatch_BoundaryPathAdded(hatch, new ObservableCollectionEventArgs<HatchBoundaryPath>(path));

                        hatch.HatchBoundaryPathAdded += this.Hatch_BoundaryPathAdded;
                        hatch.HatchBoundaryPathRemoved += this.Hatch_BoundaryPathRemoved;
                        this.hatches.Add(hatch);
                    }
                    break;
                case EntityType.Insert:
                    Insert insert = (Insert) entity;
                    insert.Block = this.blocks.Add(insert.Block, assignHandle);
                    this.blocks.References[insert.Block.Name].Add(insert);
                    foreach (Attribute attribute in insert.Attributes)
                    {
                        attribute.Layer = this.layers.Add(attribute.Layer, assignHandle);
                        this.layers.References[attribute.Layer.Name].Add(attribute);
                        attribute.LayerChanged += this.Entity_LayerChanged;

                        attribute.Linetype = this.linetypes.Add(attribute.Linetype, assignHandle);
                        this.linetypes.References[attribute.Linetype.Name].Add(attribute);
                        attribute.LinetypeChanged += this.Entity_LinetypeChanged;

                        attribute.Style = this.textStyles.Add(attribute.Style, assignHandle);
                        this.textStyles.References[attribute.Style.Name].Add(attribute);
                        attribute.TextStyleChanged += this.Entity_TextStyleChanged;
                    }
                    insert.AttributeAdded += this.Insert_AttributeAdded;
                    insert.AttributeRemoved += this.Insert_AttributeRemoved;
                    if (!isBlockEntity)
                        this.inserts.Add(insert);
                    break;
                case EntityType.LightWeightPolyline:
                    if (!isBlockEntity)
                        this.lwPolylines.Add((LwPolyline) entity);
                    break;
                case EntityType.Line:
                    if (!isBlockEntity)
                        this.lines.Add((Line) entity);
                    break;
                case EntityType.Point:
                    if (!isBlockEntity)
                        this.points.Add((Point) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    if (!isBlockEntity)
                        this.polyfaceMeshes.Add((PolyfaceMesh) entity);
                    break;
                case EntityType.Polyline:
                    if (!isBlockEntity)
                        this.polylines.Add((Polyline) entity);
                    break;
                case EntityType.Solid:
                    if (!isBlockEntity)
                        this.solids.Add((Solid) entity);
                    break;
                case EntityType.Trace:
                    if (!isBlockEntity)
                        this.traces.Add((Trace) entity);
                    break;
                case EntityType.Mesh:
                    if (!isBlockEntity)
                        this.meshes.Add((Mesh) entity);
                    break;
                case EntityType.Text:
                    Text text = (Text) entity;
                    text.Style = this.textStyles.Add(text.Style, assignHandle);
                    this.textStyles.References[text.Style.Name].Add(text);
                    text.TextStyleChanged += this.Entity_TextStyleChanged;
                    if (!isBlockEntity)
                        this.texts.Add(text);
                    break;
                case EntityType.MText:
                    MText mText = (MText) entity;
                    mText.Style = this.textStyles.Add(mText.Style, assignHandle);
                    this.textStyles.References[mText.Style.Name].Add(mText);
                    mText.TextStyleChanged += this.Entity_TextStyleChanged;
                    if (!isBlockEntity)
                        this.mTexts.Add(mText);
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    image.Definition = this.imageDefs.Add(image.Definition, assignHandle);
                    this.imageDefs.References[image.Definition.Name].Add(image);
                    if (!image.Definition.Reactors.ContainsKey(image.Handle))
                    {
                        ImageDefinitionReactor reactor = new ImageDefinitionReactor(image.Handle);
                        this.NumHandles = reactor.AsignHandle(this.NumHandles);
                        image.Definition.Reactors.Add(image.Handle, reactor);
                    }
                    if (!isBlockEntity)
                        this.images.Add(image);
                    break;
                case EntityType.MLine:
                    MLine mline = (MLine) entity;
                    mline.Style = this.mlineStyles.Add(mline.Style, assignHandle);
                    this.mlineStyles.References[mline.Style.Name].Add(mline);
                    mline.MLineStyleChanged += this.MLine_MLineStyleChanged;
                    if (!isBlockEntity)
                        this.mLines.Add(mline);
                    break;
                case EntityType.Ray:
                    if (!isBlockEntity)
                        this.rays.Add((Ray) entity);
                    break;
                case EntityType.XLine:
                    if (!isBlockEntity)
                        this.xlines.Add((XLine) entity);
                    break;
                case EntityType.Underlay:
                    Underlay underlay = (Underlay) entity;
                    switch (underlay.Definition.Type)
                    {
                        case UnderlayType.DGN:
                            underlay.Definition = this.underlayDgnDefs.Add((UnderlayDgnDefinition) underlay.Definition, assignHandle);
                            this.underlayDgnDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                        case UnderlayType.DWF:
                            underlay.Definition = this.underlayDwfDefs.Add((UnderlayDwfDefinition) underlay.Definition, assignHandle);
                            this.underlayDwfDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                        case UnderlayType.PDF:
                            underlay.Definition = this.underlayPdfDefs.Add((UnderlayPdfDefinition) underlay.Definition, assignHandle);
                            this.underlayPdfDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                    }
                    if (!isBlockEntity)
                        this.underlays.Add(underlay);
                    break;
                case EntityType.Wipeout:
                    if (!isBlockEntity)
                        this.wipeouts.Add((Wipeout) entity);
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport) entity;
                    if (viewport.ClippingBoundary != null)
                        this.AddEntity(viewport.ClippingBoundary, isBlockEntity, assignHandle);
                    if (!isBlockEntity)
                        this.viewports.Add(viewport);
                    break;
                case EntityType.AttributeDefinition:
                    AttributeDefinition attDef = (AttributeDefinition) entity;
                    attDef.Style = this.textStyles.Add(attDef.Style, assignHandle);
                    this.textStyles.References[attDef.Style.Name].Add(attDef);
                    attDef.TextStyleChange += this.Entity_TextStyleChanged;
                    if (!isBlockEntity)
                        this.attributeDefinitions.Add(attDef);
                    break;
                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown.");
            }

            foreach (string appReg in entity.XData.AppIds)
            {
                entity.XData[appReg].ApplicationRegistry = this.appRegistries.Add(entity.XData[appReg].ApplicationRegistry, assignHandle);
                this.appRegistries.References[appReg].Add(entity);
            }

            entity.Layer = this.layers.Add(entity.Layer, assignHandle);
            this.layers.References[entity.Layer.Name].Add(entity);

            entity.Linetype = this.linetypes.Add(entity.Linetype, assignHandle);
            this.linetypes.References[entity.Linetype.Name].Add(entity);

            this.AddedObjects.Add(entity.Handle, entity);

            entity.LayerChanged += this.Entity_LayerChanged;
            entity.LinetypeChanged += this.Entity_LinetypeChanged;
            entity.XDataAddAppReg += this.Entity_XDataAddAppReg;
            entity.XDataRemoveAppReg += this.Entity_XDataRemoveAppReg;
        }

        private void RemoveDimensionStyleOverrides(DimensionStyleOverrideDictionary overrides, DxfObject entity)
        {
            // remove the style override referenced DxfObjects
            DimensionStyleOverride styleOverride;

            // add referenced text style
            overrides.TryGetValue(DimensionStyleOverrideType.TextStyle, out styleOverride);
            if (styleOverride != null)
            {
                TextStyle dimtxtsty = (TextStyle) styleOverride.Value;
                this.textStyles.References[dimtxtsty.Name].Remove(entity);
            }

            // add referenced blocks
            overrides.TryGetValue(DimensionStyleOverrideType.LeaderArrow, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    this.blocks.References[block.Name].Remove(entity);
                }
            }

            overrides.TryGetValue(DimensionStyleOverrideType.DimArrow1, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    this.blocks.References[block.Name].Remove(entity);
                }
            }

            overrides.TryGetValue(DimensionStyleOverrideType.DimArrow2, out styleOverride);
            if (styleOverride != null)
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    this.blocks.References[block.Name].Remove(entity);
                }
            }

            // add referenced line types
            overrides.TryGetValue(DimensionStyleOverrideType.DimLineLinetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                this.linetypes.References[linetype.Name].Remove(entity);
            }

            overrides.TryGetValue(DimensionStyleOverrideType.ExtLine1Linetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                this.linetypes.References[linetype.Name].Remove(entity);
            }

            overrides.TryGetValue(DimensionStyleOverrideType.ExtLine2Linetype, out styleOverride);
            if (styleOverride != null)
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                this.linetypes.References[linetype.Name].Remove(entity);
            }
        }

        internal bool RemoveEntity(EntityObject entity, bool isBlockEntity)
        {
            if (entity == null)
                return false;

            if (entity.Handle == null)
                return false;

            if (entity.Owner == null)
                return false;

            if (entity.Reactors.Count > 0)
                return false;

            if (entity.Owner.Record.Layout == null)
                return false;

            if (!this.AddedObjects.ContainsKey(entity.Handle))
                return false;

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition
            // and they will not be removed from the drawing database
            switch (entity.Type)
            {
                case EntityType.Arc:
                    if (!isBlockEntity)
                        this.arcs.Remove((Arc) entity);
                    break;
                case EntityType.Circle:
                    if (!isBlockEntity)
                        this.circles.Remove((Circle) entity);
                    break;
                case EntityType.Dimension:
                    Dimension dim = (Dimension) entity;
                    if (!isBlockEntity)
                        this.dimensions.Remove(dim);
                    this.blocks.References[dim.Block.Name].Remove(entity);
                    dim.DimensionBlockChanged -= this.Dimension_DimBlockChanged;
                    this.dimStyles.References[dim.Style.Name].Remove(entity);
                    dim.DimensionStyleChanged -= this.Dimension_DimStyleChanged;
                    dim.Block = null;

                    this.RemoveDimensionStyleOverrides(dim.StyleOverrides, dim);
                    dim.DimensionStyleOverrideAdded -= this.Dimension_DimStyleOverrideAdded;
                    dim.DimensionStyleOverrideRemoved -= this.Dimension_DimStyleOverrideRemoved;

                    break;
                case EntityType.Leader:
                    Leader leader = (Leader) entity;
                    if (!isBlockEntity)
                        this.leaders.Remove(leader);
                    this.dimStyles.References[leader.Style.Name].Remove(entity);
                    leader.LeaderStyleChanged -= this.Leader_DimStyleChanged;

                    if (leader.Annotation != null)
                        leader.Annotation.RemoveReactor(leader);

                    this.RemoveDimensionStyleOverrides(leader.StyleOverrides, leader);
                    leader.DimensionStyleOverrideAdded -= this.Leader_DimStyleOverrideAdded;
                    leader.DimensionStyleOverrideRemoved -= this.Leader_DimStyleOverrideRemoved;
                    break;
                case EntityType.Tolerance:
                    Tolerance tolerance = (Tolerance) entity;
                    if (!isBlockEntity)
                        this.tolerances.Remove(tolerance);
                    this.dimStyles.References[tolerance.Style.Name].Remove(entity);
                    tolerance.ToleranceStyleChanged -= this.Tolerance_DimStyleChanged;
                    break;
                case EntityType.Ellipse:
                    if (!isBlockEntity)
                        this.ellipses.Remove((Ellipse) entity);
                    break;
                case EntityType.Face3D:
                    if (!isBlockEntity)
                        this.faces3d.Remove((Face3d) entity);
                    break;
                case EntityType.Spline:
                    if (!isBlockEntity)
                        this.splines.Remove((Spline) entity);
                    break;
                case EntityType.Hatch:
                    Hatch hatch = (Hatch) entity;
                    hatch.UnLinkBoundary();
                    if (!isBlockEntity)
                    {
                        hatch.HatchBoundaryPathAdded -= this.Hatch_BoundaryPathAdded;
                        hatch.HatchBoundaryPathRemoved -= this.Hatch_BoundaryPathRemoved;
                        this.hatches.Remove(hatch);
                    }
                    break;
                case EntityType.Insert:
                    Insert insert = (Insert) entity;
                    if (!isBlockEntity)
                        this.inserts.Remove(insert);
                    this.blocks.References[insert.Block.Name].Remove(entity);
                    foreach (Attribute att in insert.Attributes)
                    {
                        this.layers.References[att.Layer.Name].Remove(att);
                        att.LayerChanged -= this.Entity_LayerChanged;
                        this.linetypes.References[att.Linetype.Name].Remove(att);
                        att.LinetypeChanged -= this.Entity_LinetypeChanged;
                        this.textStyles.References[att.Style.Name].Remove(att);
                        att.TextStyleChanged -= this.Entity_TextStyleChanged;
                    }
                    insert.AttributeAdded -= this.Insert_AttributeAdded;
                    insert.AttributeRemoved -= this.Insert_AttributeRemoved;
                    break;
                case EntityType.LightWeightPolyline:
                    if (!isBlockEntity)
                        this.lwPolylines.Remove((LwPolyline) entity);
                    break;
                case EntityType.Line:
                    if (!isBlockEntity)
                        this.lines.Remove((Line) entity);
                    break;
                case EntityType.Point:
                    if (!isBlockEntity)
                        this.points.Remove((Point) entity);
                    break;
                case EntityType.PolyfaceMesh:
                    if (!isBlockEntity)
                        this.polyfaceMeshes.Remove((PolyfaceMesh) entity);
                    break;
                case EntityType.Polyline:
                    if (!isBlockEntity)
                        this.polylines.Remove((Polyline) entity);
                    break;
                case EntityType.Solid:
                    if (!isBlockEntity)
                        this.solids.Remove((Solid) entity);
                    break;
                case EntityType.Trace:
                    if (!isBlockEntity)
                        this.traces.Remove((Trace) entity);
                    break;
                case EntityType.Mesh:
                    if (!isBlockEntity)
                        this.meshes.Remove((Mesh) entity);
                    break;
                case EntityType.Text:
                    Text text = (Text) entity;
                    if (!isBlockEntity)
                        this.texts.Remove(text);
                    this.textStyles.References[text.Style.Name].Remove(entity);
                    text.TextStyleChanged -= this.Entity_TextStyleChanged;
                    break;
                case EntityType.MText:
                    MText mText = (MText) entity;
                    if (!isBlockEntity)
                        this.mTexts.Remove(mText);
                    this.textStyles.References[mText.Style.Name].Remove(entity);
                    mText.TextStyleChanged -= this.Entity_TextStyleChanged;
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    if (!isBlockEntity)
                        this.images.Remove(image);
                    this.imageDefs.References[image.Definition.Name].Remove(image);
                    image.Definition.Reactors.Remove(image.Handle);
                    break;
                case EntityType.MLine:
                    MLine mline = (MLine) entity;
                    if (!isBlockEntity)
                        this.mLines.Remove(mline);
                    this.mlineStyles.References[mline.Style.Name].Remove(entity);
                    mline.MLineStyleChanged -= this.MLine_MLineStyleChanged;
                    break;
                case EntityType.Ray:
                    if (!isBlockEntity)
                        this.rays.Remove((Ray) entity);
                    break;
                case EntityType.XLine:
                    if (!isBlockEntity)
                        this.xlines.Remove((XLine) entity);
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport) entity;
                    if (!isBlockEntity)
                        this.viewports.Remove(viewport);
                    // delete the viewport boundary entity in case there is one
                    if (viewport.ClippingBoundary != null)
                    {
                        viewport.ClippingBoundary.RemoveReactor(viewport);
                        this.RemoveEntity(viewport.ClippingBoundary);
                    }
                    break;
                case EntityType.AttributeDefinition:
                    AttributeDefinition attDef = (AttributeDefinition) entity;
                    if (!isBlockEntity)
                        this.attributeDefinitions.Remove(attDef);
                    this.textStyles.References[attDef.Style.Name].Remove(entity);
                    break;
                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }

            if (!isBlockEntity)
                this.layouts.References[entity.Owner.Record.Layout.Name].Remove(entity);

            this.layers.References[entity.Layer.Name].Remove(entity);
            this.linetypes.References[entity.Linetype.Name].Remove(entity);
            foreach (string appReg in entity.XData.AppIds)
            {
                this.appRegistries.References[appReg].Remove(entity);
            }
            this.AddedObjects.Remove(entity.Handle);

            entity.Handle = null;
            entity.Owner = null;

            entity.LayerChanged -= this.Entity_LayerChanged;
            entity.LinetypeChanged -= this.Entity_LinetypeChanged;
            entity.XDataAddAppReg -= this.Entity_XDataAddAppReg;
            entity.XDataRemoveAppReg -= this.Entity_XDataRemoveAppReg;

            return true;
        }

        private void AddDefaultObjects()
        {
            // collections
            this.vports = new VPorts(this);
            this.views = new Views(this);
            this.appRegistries = new ApplicationRegistries(this);
            this.layers = new Layers(this);
            this.linetypes = new Linetypes(this);
            this.textStyles = new TextStyles(this);
            this.dimStyles = new DimensionStyles(this);
            this.mlineStyles = new MLineStyles(this);
            this.ucss = new UCSs(this);
            this.blocks = new BlockRecords(this);
            this.imageDefs = new ImageDefinitions(this);
            this.underlayDgnDefs = new UnderlayDgnDefinitions(this);
            this.underlayDwfDefs = new UnderlayDwfDefinitions(this);
            this.underlayPdfDefs = new UnderlayPdfDefinitions(this);
            this.groups = new Groups(this);
            this.layouts = new Layouts(this);

            //add default viewport (the active viewport is automatically added when the collection is created, is the only one supported)
            //this.vports.Add(VPort.Active);

            //add default layer
            this.layers.Add(Layer.Default);

            // add default line types
            this.linetypes.Add(Linetype.ByLayer);
            this.linetypes.Add(Linetype.ByBlock);
            this.linetypes.Add(Linetype.Continuous);

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

        #endregion

        #region entity events

        private void MLine_MLineStyleChanged(MLine sender, TableObjectChangedEventArgs<MLineStyle> e)
        {
            this.mlineStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.mlineStyles.Add(e.NewValue);
            this.mlineStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Dimension_DimStyleChanged(Dimension sender, TableObjectChangedEventArgs<DimensionStyle> e)
        {
            this.dimStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.dimStyles.Add(e.NewValue);
            this.dimStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Dimension_DimBlockChanged(Dimension sender, TableObjectChangedEventArgs<Block> e)
        {
            this.blocks.References[e.OldValue.Name].Remove(sender);
            this.blocks.Remove(e.OldValue);

            e.NewValue = this.blocks.Add(e.NewValue);
            this.blocks.References[e.NewValue.Name].Add(sender);
        }

        private void Dimension_DimStyleOverrideAdded(Dimension sender, DimensionStyleOverrideChangeEventArgs e)
        {
            switch (e.Item.Type)
            {
                case DimensionStyleOverrideType.DimLineLinetype:
                case DimensionStyleOverrideType.ExtLine1Linetype:
                case DimensionStyleOverrideType.ExtLine2Linetype:
                    Linetype linetype = (Linetype) e.Item.Value;
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.linetypes.Add(linetype));
                    this.linetypes.References[linetype.Name].Add(sender);
                    break;
                case DimensionStyleOverrideType.LeaderArrow:
                case DimensionStyleOverrideType.DimArrow1:
                case DimensionStyleOverrideType.DimArrow2:
                    Block block = (Block) e.Item.Value;
                    if (block == null)
                        return; // the block might be defined as null to indicate that the default arrowhead will be used
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.blocks.Add(block));
                    this.blocks.References[block.Name].Add(sender);
                    break;
                case DimensionStyleOverrideType.TextStyle:
                    TextStyle style = (TextStyle) e.Item.Value;
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.textStyles.Add(style));
                    this.textStyles.References[style.Name].Add(sender);
                    break;
            }
        }

        private void Dimension_DimStyleOverrideRemoved(Dimension sender, DimensionStyleOverrideChangeEventArgs e)
        {
            switch (e.Item.Type)
            {
                case DimensionStyleOverrideType.DimLineLinetype:
                case DimensionStyleOverrideType.ExtLine1Linetype:
                case DimensionStyleOverrideType.ExtLine2Linetype:
                    Linetype linetype = (Linetype) e.Item.Value;
                    this.linetypes.References[linetype.Name].Remove(sender);
                    break;
                case DimensionStyleOverrideType.LeaderArrow:
                case DimensionStyleOverrideType.DimArrow1:
                case DimensionStyleOverrideType.DimArrow2:
                    Block block = (Block) e.Item.Value;
                    if (block == null)
                        return; // the block might be defined as null to indicate that the default arrowhead will be used
                    this.blocks.References[block.Name].Remove(sender);
                    break;
                case DimensionStyleOverrideType.TextStyle:
                    TextStyle style = (TextStyle) e.Item.Value;
                    this.textStyles.References[style.Name].Remove(sender);
                    break;
            }
        }

        private void Leader_DimStyleChanged(Leader sender, TableObjectChangedEventArgs<DimensionStyle> e)
        {
            this.dimStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.dimStyles.Add(e.NewValue);
            this.dimStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Leader_DimStyleOverrideAdded(Leader sender, DimensionStyleOverrideChangeEventArgs e)
        {
            switch (e.Item.Type)
            {
                case DimensionStyleOverrideType.DimLineLinetype:
                case DimensionStyleOverrideType.ExtLine1Linetype:
                case DimensionStyleOverrideType.ExtLine2Linetype:
                    Linetype linetype = (Linetype) e.Item.Value;
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.linetypes.Add(linetype));
                    this.linetypes.References[linetype.Name].Add(sender);
                    break;
                case DimensionStyleOverrideType.LeaderArrow:
                case DimensionStyleOverrideType.DimArrow1:
                case DimensionStyleOverrideType.DimArrow2:
                    Block block = (Block) e.Item.Value;
                    if (block == null)
                        return; // the block might be defined as null to indicate that the default arrowhead will be used
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.blocks.Add(block));
                    this.blocks.References[block.Name].Add(sender);
                    break;
                case DimensionStyleOverrideType.TextStyle:
                    TextStyle style = (TextStyle) e.Item.Value;
                    sender.StyleOverrides[e.Item.Type] = new DimensionStyleOverride(e.Item.Type, this.textStyles.Add(style));
                    this.textStyles.References[style.Name].Add(sender);
                    break;
            }
        }

        private void Leader_DimStyleOverrideRemoved(Leader sender, DimensionStyleOverrideChangeEventArgs e)
        {
            switch (e.Item.Type)
            {
                case DimensionStyleOverrideType.DimLineLinetype:
                case DimensionStyleOverrideType.ExtLine1Linetype:
                case DimensionStyleOverrideType.ExtLine2Linetype:
                    Linetype linetype = (Linetype) e.Item.Value;
                    this.linetypes.References[linetype.Name].Remove(sender);
                    break;
                case DimensionStyleOverrideType.LeaderArrow:
                case DimensionStyleOverrideType.DimArrow1:
                case DimensionStyleOverrideType.DimArrow2:
                    Block block = (Block) e.Item.Value;
                    if (block == null)
                        return; // the block might be defined as null to indicate that the default arrowhead will be used
                    this.blocks.References[block.Name].Remove(sender);
                    break;
                case DimensionStyleOverrideType.TextStyle:
                    TextStyle style = (TextStyle) e.Item.Value;
                    this.textStyles.References[style.Name].Remove(sender);
                    break;
            }
        }

        private void Tolerance_DimStyleChanged(Tolerance sender, TableObjectChangedEventArgs<DimensionStyle> e)
        {
            this.dimStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.dimStyles.Add(e.NewValue);
            this.dimStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_TextStyleChanged(DxfObject sender, TableObjectChangedEventArgs<TextStyle> e)
        {
            this.textStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.textStyles.Add(e.NewValue);
            this.textStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_LinetypeChanged(DxfObject sender, TableObjectChangedEventArgs<Linetype> e)
        {
            this.linetypes.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.linetypes.Add(e.NewValue);
            this.linetypes.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_LayerChanged(DxfObject sender, TableObjectChangedEventArgs<Layer> e)
        {
            this.layers.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.layers.Add(e.NewValue);
            this.layers.References[e.NewValue.Name].Add(sender);
        }

        void Entity_XDataAddAppReg(EntityObject sender, ObservableCollectionEventArgs<ApplicationRegistry> e)
        {
            sender.XData[e.Item.Name].ApplicationRegistry = this.appRegistries.Add(sender.XData[e.Item.Name].ApplicationRegistry);
            this.appRegistries.References[e.Item.Name].Add(sender);
        }

        void Entity_XDataRemoveAppReg(EntityObject sender, ObservableCollectionEventArgs<ApplicationRegistry> e)
        {
            this.appRegistries.References[e.Item.Name].Remove(sender);
        }

        private void Insert_AttributeAdded(Insert sender, AttributeChangeEventArgs e)
        {
            this.NumHandles = e.Item.AsignHandle(this.NumHandles);

            e.Item.Layer = this.layers.Add(e.Item.Layer);
            this.layers.References[e.Item.Layer.Name].Add(e.Item);
            e.Item.LayerChanged += this.Entity_LayerChanged;

            e.Item.Linetype = this.linetypes.Add(e.Item.Linetype);
            this.linetypes.References[e.Item.Linetype.Name].Add(e.Item);
            e.Item.LinetypeChanged -= this.Entity_LinetypeChanged;

            e.Item.Style = this.textStyles.Add(e.Item.Style);
            this.textStyles.References[e.Item.Style.Name].Add(e.Item);
            e.Item.TextStyleChanged += this.Entity_TextStyleChanged;
        }

        private void Insert_AttributeRemoved(Insert sender, AttributeChangeEventArgs e)
        {
            this.layers.References[e.Item.Layer.Name].Remove(e.Item);
            e.Item.LayerChanged += this.Entity_LayerChanged;

            this.linetypes.References[e.Item.Linetype.Name].Remove(e.Item);
            e.Item.LinetypeChanged -= this.Entity_LinetypeChanged;

            this.textStyles.References[e.Item.Style.Name].Remove(e.Item);
            e.Item.TextStyleChanged += this.Entity_TextStyleChanged;
        }

        private void Hatch_BoundaryPathAdded(Hatch sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            Layout layout = sender.Owner.Record.Layout;
            foreach (EntityObject entity in e.Item.Entities)
            {
                // the hatch belongs to a layout
                if (entity.Owner != null)
                {
                    // the hatch and its entities must belong to the same document or block
                    if (!ReferenceEquals(entity.Owner.Record.Layout, layout))
                        throw new ArgumentException("The HatchBoundaryPath entity and the hatch must belong to the same layout and document. Clone it instead.");
                    // there is no need to do anything else we will not add the same entity twice
                }
                else
                {
                    // we will add the new entity to the same document and layout of the hatch
                    string active = this.ActiveLayout;
                    this.ActiveLayout = layout.Name;
                    // the entity does not belong to anyone
                    this.AddEntity(entity, false, true);
                    this.ActiveLayout = active;
                }
            }
        }

        private void Hatch_BoundaryPathRemoved(Hatch sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            foreach (EntityObject entity in e.Item.Entities)
            {
                this.RemoveEntity(entity);
            }
        }

        #endregion
    }
}