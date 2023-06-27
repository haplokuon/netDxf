#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) Daniel Carvajal (haplokuon@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
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
    /// Represents a document to read and write DXF files.
    /// </summary>
    /// <remarks>
    /// The DxfDocument class derives from DxfObject for convenience of this library not because of the DXF structure.
    /// It can contain external data (XData) information, but it is not saved in the DXF.
    /// </remarks>
    public sealed class DxfDocument :
        DxfObject
    {
        #region private fields

        private string name;
        private readonly SupportFolders supportFolders;
        private bool buildDimensionBlocks;
        private long numHandles;

        // DXF objects added to the document (key: handle, value: DXF object).
        internal ObservableDictionary<string, DxfObject> AddedObjects;
        // keeps track of the dimension blocks generated
        internal int DimensionBlocksIndex;
        // keeps track of the group names generated (this groups have the isUnnamed set to true)
        internal int GroupNamesIndex;

        #region header

        private readonly List<string> comments;
        private readonly HeaderVariables drawingVariables;

        #endregion

        #region collections

        private ApplicationRegistries appRegistries;
        private BlockRecords blocks;
        private DimensionStyles dimStyles;
        private Layers layers;
        private Linetypes linetypes;
        private TextStyles textStyles;
        private ShapeStyles shapeStyles;
        private UCSs ucss;
        private Views views;
        private VPorts vports;
        private readonly DrawingEntities entities;
        private MLineStyles mlineStyles;
        private ImageDefinitions imageDefs;
        private UnderlayDgnDefinitions underlayDgnDefs;
        private UnderlayDwfDefinitions underlayDwfDefs;
        private UnderlayPdfDefinitions underlayPdfDefs;
        private Groups groups;
        private Layouts layouts;
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
        /// <param name="supportFolders">List of the document support folders.</param>
        /// <remarks>The default <see cref="HeaderVariables">drawing variables</see> of the document will be used.</remarks>
        public DxfDocument(IEnumerable<string> supportFolders)
            : this(new HeaderVariables(), supportFolders)
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
        /// <param name="version">AutoCAD drawing database version number.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        public DxfDocument(DxfVersion version, IEnumerable<string> supportFolders)
            : this(new HeaderVariables { AcadVer = version }, supportFolders)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        public DxfDocument(HeaderVariables drawingVariables)
            : this(drawingVariables, new List<string>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        public DxfDocument(HeaderVariables drawingVariables, IEnumerable<string> supportFolders)
            : this(drawingVariables, true, new SupportFolders(supportFolders))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>DxfDocument</c> class.
        /// </summary>
        /// <param name="drawingVariables"><see cref="HeaderVariables">Drawing variables</see> of the document.</param>
        /// <param name="createDefaultObjects">Check if the default objects need to be created.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        internal DxfDocument(HeaderVariables drawingVariables, bool createDefaultObjects, SupportFolders supportFolders)
            : base("DOCUMENT")
        {
            this.supportFolders = supportFolders;
            this.buildDimensionBlocks = false;
            this.comments = new List<string> { "DXF file generated by netDxf https://github.com/haplokuon/netDxf, Copyright(C) Daniel Carvajal, Licensed under MIT" };
            this.Owner = null;
            this.drawingVariables = drawingVariables;
            this.NumHandles = this.AssignHandle(0);
            this.DimensionBlocksIndex = -1;
            this.GroupNamesIndex = 0;
            this.entities = new DrawingEntities(this);

            this.AddedObjects = new ObservableDictionary<string, DxfObject>(StringComparer.InvariantCultureIgnoreCase);
            this.AddedObjects.BeforeAddItem += this.AddedObjects_BeforeAddItem;
            this.AddedObjects.AddItem += this.AddedObjects_AddItem;
            this.AddedObjects.BeforeRemoveItem += this.AddedObjects_BeforeRemoveItem;
            this.AddedObjects.RemoveItem += this.AddedObjects_RemoveItem;
            this.AddedObjects.Add(this.Handle, this);

            if (createDefaultObjects)
            {
                this.AddDefaultObjects();
            }
        }

        #endregion

        #region internal properties

        /// <summary>
        /// Gets or sets the number of handles generated, this value is saved as an hexadecimal in the drawing variables HandleSeed property.
        /// </summary>
        internal long NumHandles
        {
            get { return this.numHandles; }
            set
            {
                this.DrawingVariables.HandleSeed = value.ToString("X");
                this.numHandles = value;
            }
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the list of folders where the drawing support files are present.
        /// </summary>
        /// <remarks>
        /// When shape linetype segments are used, the shape number will be obtained reading the .shp file equivalent to the .shx file,
        /// that file will be looked for in the same folder as the .shx file or one of the document support folders.
        /// </remarks>
        public SupportFolders SupportFolders
        {
            get { return this.supportFolders; }
        }

        //// <summary>
        //// Gets or sets if the blocks that represents dimension entities will be created when added to the document.
        //// </summary>
        /// <remarks>
        /// By default this value is set to false, no dimension blocks will be generated when adding dimension entities to the document.
        /// It will be the responsibility of the program importing the DXF to generate the drawing that represent the dimensions.<br />
        /// When set to true the block that represents the dimension will be generated,
        /// keep in mind that this process is limited and not all options available in the dimension style will be reflected in the final result.<br />
        /// When importing a file if the dimension block is present it will be read, regardless of this value.
        /// If, later, the dimension is modified all updates will be done with the limited dimension drawing capabilities of the library,
        /// in this case, if you want that the new modifications to be reflected when the file is saved again you can set the dimension block to null,
        /// and the program reading the resulting file will regenerate the block with the new modifications.
        /// </remarks>
        public bool BuildDimensionBlocks
        {
            get { return this.buildDimensionBlocks; }
            set { this.buildDimensionBlocks = value; }
        }

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
        /// Gets or sets the <see cref="RasterVariables">RasterVariables</see> applied to image entities.
        /// </summary>
        public RasterVariables RasterVariables
        {
            get { return this.rasterVariables; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (string.IsNullOrEmpty(value.Handle))
                {
                    this.NumHandles = value.AssignHandle(this.NumHandles);
                }
                this.AddedObjects.Add(value.Handle, value);
                this.rasterVariables = value;
            }
        }

        #region header

        /// <summary>
        /// Gets or sets the name of the document, once a file is saved or loaded this field is equals the file name without extension.
        /// </summary>
        public List<string> Comments
        {
            get { return this.comments; }
        }

        /// <summary>
        /// Gets the DXF <see cref="HeaderVariables">drawing variables</see>.
        /// </summary>
        public HeaderVariables DrawingVariables
        {
            get { return this.drawingVariables; }
        }

        /// <summary>
        /// Gets or sets the name of the document.
        /// </summary>
        /// <remarks>
        /// When a file is loaded this field is equals the file name without extension.
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
        /// Gets the <see cref="Layers">layers</see> collection.
        /// </summary>
        public Layers Layers
        {
            get { return this.layers; }
            internal set { this.layers = value; }
        }

        /// <summary>
        /// Gets the <see cref="Linetypes">line types</see> collection.
        /// </summary>
        public Linetypes Linetypes
        {
            get { return this.linetypes; }
            internal set { this.linetypes = value; }
        }

        /// <summary>
        /// Gets the <see cref="TextStyles">text styles</see> collection.
        /// </summary>
        public TextStyles TextStyles
        {
            get { return this.textStyles; }
            internal set { this.textStyles = value; }
        }

        /// <summary>
        /// Gets the <see cref="ShapeStyles">shape styles</see> collection.
        /// </summary>
        /// <remarks>
        /// The DXF stores the TextStyles and ShapeStyles in the same table list, here, they are separated since they serve a different role.
        /// Under normal circumstances you should not need to access this list.
        /// </remarks>
        public ShapeStyles ShapeStyles
        {
            get { return this.shapeStyles; }
            internal set { this.shapeStyles = value; }
        }

        /// <summary>
        /// Gets the <see cref="DimensionStyles">dimension styles</see> collection.
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
        /// Gets the <see cref="UCSs">User coordinate systems</see> collection.
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

        /// <summary>
        /// Gets the <see cref="DrawingEntities">entities</see> shortcuts.
        /// </summary>
        public DrawingEntities Entities
        {
            get { return this.entities; }
        }

        #endregion

        #endregion

        #region public methods

        /// <summary>
        /// Loads a DXF file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
        /// <remarks>
        /// Loading DXF files prior to AutoCad 2000 is not supported.<br />
        /// The Load method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode it will raise any exception that might occur during the whole process.
        /// </remarks>
        public static DxfDocument Load(string file)
        {
            return Load(file, new List<string>());
        }

        /// <summary>
        /// Loads a DXF file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
        /// <remarks>
        /// Loading DXF files prior to AutoCad 2000 is not supported.<br />
        /// The Load method will still raise an exception if they are unable to create the FileStream.<br />
        /// On Debug mode it will raise any exception that might occur during the whole process.
        /// </remarks>
        public static DxfDocument Load(string file, IEnumerable<string> supportFolders)
        {
            Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            FileInfo fileInfo = new FileInfo(file);
            SupportFolders folders = new SupportFolders(supportFolders) { WorkingFolder = fileInfo.DirectoryName };
            
            DxfReader dxfReader = new DxfReader();
            
#if DEBUG
            DxfDocument document = dxfReader.Read(stream, folders);
            stream.Close();
#else
            DxfDocument document;
            try
            {
                document = dxfReader.Read(stream, folders);
            }
            catch (DxfVersionNotSupportedException)
            {
                throw;
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
            document.name = Path.GetFileNameWithoutExtension(file);
            return document;
        }

        /// <summary>
        /// Loads a DXF file.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
        /// <remarks>
        /// Loading DXF files prior to AutoCad 2000 is not supported.<br />
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public static DxfDocument Load(Stream stream)
        {
            return Load(stream, new List<string>());
        }

        /// <summary>
        /// Loads a DXF file.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="supportFolders">List of the document support folders.</param>
        /// <returns>Returns a DxfDocument. It will return null if the file has not been able to load.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
        /// <remarks>
        /// Loading DXF files prior to AutoCad 2000 is not supported.<br />
        /// On Debug mode it will raise any exception that might occur during the whole process.<br />
        /// The caller will be responsible of closing the stream.
        /// </remarks>
        public static DxfDocument Load(Stream stream, IEnumerable<string> supportFolders)
        {
            DxfReader dxfReader = new DxfReader();
            
#if DEBUG
            DxfDocument document = dxfReader.Read(stream, new SupportFolders(supportFolders));
#else
            DxfDocument document;
            try
            {
                 document = dxfReader.Read(stream, new SupportFolders(supportFolders));
            }
            catch (DxfVersionNotSupportedException)
            {
                throw;
            }
            catch
            {
                return null;
            }

#endif
            return document;
        }

        /// <summary>
        /// Saves the database of the actual DxfDocument to a text DXF file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>Return true if the file has been successfully save, false otherwise.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
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
        /// Saves the database of the actual DxfDocument to a DXF file.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="isBinary">Defines if the file will be saved as binary.</param>
        /// <returns>Returns true if the file has been successfully saved, false otherwise.</returns>
        /// <exception cref="DxfVersionNotSupportedException"></exception>
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

            string workingFolder = fileInfo.DirectoryName;
            if (!string.IsNullOrEmpty(workingFolder))
            {
                this.supportFolders.WorkingFolder = workingFolder;
            }

#if DEBUG
            dxfWriter.Write(stream, this, isBinary);
            stream.Close();
#else
            try
            {
                dxfWriter.Write(stream, this, isBinary);
            }
            catch (DxfVersionNotSupportedException)
            {
                throw;
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
        /// <exception cref="DxfVersionNotSupportedException"></exception>
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
        /// <exception cref="DxfVersionNotSupportedException"></exception>
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
            catch (DxfVersionNotSupportedException)
            {
                throw;
            }
            catch
            {
                return false;
            }
                
#endif
            return true;
        }

        /// <summary>
        /// Checks the AutoCAD DXF file database version.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <returns>String that represents the DXF file version.</returns>
        public static DxfVersion CheckDxfFileVersion(string file)
        {
            return CheckDxfFileVersion(file, out bool _);
        }

        /// <summary>
        /// Checks the AutoCAD DXF file database version.
        /// </summary>
        /// <param name="file">File name.</param>
        /// <param name="isBinary">Returns true if the DXF is a binary file.</param>
        /// <returns>String that represents the DXF file version.</returns>
        public static DxfVersion CheckDxfFileVersion(string file, out bool isBinary)
        {
            Stream stream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            DxfVersion version = CheckDxfFileVersion(stream, out isBinary);
            stream.Close();
            return version;
        }

        /// <summary>
        /// Checks the AutoCAD DXF file database version.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <returns>String that represents the DXF file version.</returns>
        /// <remarks>The caller will be responsible of closing the stream.</remarks>
        public static DxfVersion CheckDxfFileVersion(Stream stream)
        {
            return CheckDxfFileVersion(stream, out bool _);
        }

        /// <summary>
        /// Checks the AutoCAD DXF file database version.
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="isBinary">Returns true if the DXF is a binary file.</param>
        /// <returns>String that represents the DXF file version.</returns>
        /// <remarks>The caller will be responsible of closing the stream.</remarks>
        public static DxfVersion CheckDxfFileVersion(Stream stream, out bool isBinary)
        {
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

            return StringEnum<DxfVersion>.Parse(value, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets a DXF object by its handle.
        /// </summary>
        /// <param name="objectHandle">DxfObject handle.</param>
        /// <returns>The DxfObject that has the provided handle, null otherwise.</returns>
        public DxfObject GetObjectByHandle(string objectHandle)
        {
            if (string.IsNullOrEmpty(objectHandle))
            {
                return null;
            }

            this.AddedObjects.TryGetValue(objectHandle, out DxfObject o);
            return o;
        }

        #endregion

        #region internal methods

        internal void AddEntityToDocument(EntityObject entity, bool assignHandle)
        {
            // null entities are not allowed
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            // assign a handle
            if (assignHandle || string.IsNullOrEmpty(entity.Handle))
            {
                this.NumHandles = entity.AssignHandle(this.NumHandles);
            }

            // the entities that are part of a block do not belong to any of the entities lists but to the block definition.
            switch (entity.Type)
            {
                case EntityType.Arc:
                    break;
                case EntityType.Circle:
                    break;
                case EntityType.Dimension:
                    Dimension dim = (Dimension)entity;
                    dim.Style = this.dimStyles.Add(dim.Style, assignHandle);
                    this.dimStyles.References[dim.Style.Name].Add(dim);
                    this.AddDimensionStyleOverridesReferencedDxfObjects(dim, dim.StyleOverrides, assignHandle);
                    if (this.buildDimensionBlocks)
                    {
                        Block dimBlock = DimensionBlock.Build(dim, "DimBlock");
                        dimBlock.SetName("*D" + ++this.DimensionBlocksIndex, false);
                        dim.Block = this.blocks.Add(dimBlock);
                        this.blocks.References[dimBlock.Name].Add(dim);
                    }
                    else if (dim.Block != null)
                    {
                        if (!this.blocks.Contains(dim.Block) || !dim.Block.Name.StartsWith("*D", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // if a block is not present or have a wrong name give it a proper name
                            dim.Block.SetName("*D" + ++this.DimensionBlocksIndex, false);
                        }
                        //dim.Block.SetName("*D" + ++this.DimensionBlocksIndex, false);
                        dim.Block = this.blocks.Add(dim.Block);
                        this.blocks.References[dim.Block.Name].Add(dim);
                    }
                    dim.DimensionStyleChanged += this.Dimension_DimStyleChanged;
                    dim.DimensionBlockChanged += this.Dimension_DimBlockChanged;
                    dim.DimensionStyleOverrideAdded += this.Dimension_DimStyleOverrideAdded;
                    dim.DimensionStyleOverrideRemoved += this.Dimension_DimStyleOverrideRemoved;
                    break;
                case EntityType.Leader:
                    Leader leader = (Leader)entity;
                    leader.Style = this.dimStyles.Add(leader.Style, assignHandle);
                    this.dimStyles.References[leader.Style.Name].Add(leader);
                    leader.LeaderStyleChanged += this.Leader_DimStyleChanged;
                    this.AddDimensionStyleOverridesReferencedDxfObjects(leader, leader.StyleOverrides, assignHandle);
                    leader.DimensionStyleOverrideAdded += this.Leader_DimStyleOverrideAdded;
                    leader.DimensionStyleOverrideRemoved += this.Leader_DimStyleOverrideRemoved;
                    leader.AnnotationAdded += this.Leader_AnnotationAdded;
                    leader.AnnotationRemoved += this.Leader_AnnotationRemoved;
                    break;
                case EntityType.Tolerance:
                    Tolerance tol = (Tolerance)entity;
                    tol.Style = this.dimStyles.Add(tol.Style, assignHandle);
                    this.dimStyles.References[tol.Style.Name].Add(tol);
                    tol.ToleranceStyleChanged += this.Tolerance_DimStyleChanged;
                    break;
                case EntityType.Ellipse:
                    break;
                case EntityType.Face3D:
                    break;
                case EntityType.Hatch:
                    Hatch hatch = (Hatch)entity;
                    hatch.HatchBoundaryPathAdded += this.Hatch_BoundaryPathAdded;
                    hatch.HatchBoundaryPathRemoved += this.Hatch_BoundaryPathRemoved;
                    break;
                case EntityType.Insert:
                    Insert insert = (Insert)entity;
                    insert.Block = this.blocks.Add(insert.Block, assignHandle);
                    this.blocks.References[insert.Block.Name].Add(insert);
                    //DrawingUnits insUnits = this.DrawingVariables.InsUnits;
                    //double docScale = UnitHelper.ConversionFactor(insert.Block.Record.Units, insUnits);
                    foreach (Attribute attribute in insert.Attributes)
                    {
                        //if (assignHandle && attribute.Definition != null)
                        //{
                        //    attribute.Height = docScale * attribute.Definition.Height;
                        //    attribute.Position = docScale * attribute.Definition.Position + insert.Position - insert.Block.Origin;
                        //}

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
                    //insert.BlockChanged += this.Insert_BlockChanged;
                    break;
                case EntityType.Line:
                    break;
                case EntityType.Shape:
                    Shape shape = (Shape) entity;
                    shape.Style = this.shapeStyles.Add(shape.Style, assignHandle);
                    this.shapeStyles.References[shape.Style.Name].Add(shape);
                    //TODO: shape names and indexes, require check to external SHX file
                    //if (!shape.Style.ContainsShapeName(shape.Name))
                    //{
                    //    throw new ArgumentException("The shape name " + shape.Name + " is not defined in the associated shape style " + shape.Style.Name + ".");
                    //}
                    shape.StyleChanged += this.Shape_StyleChanged;
                    break;
                case EntityType.Point:
                    break;
                case EntityType.PolyfaceMesh:
                    PolyfaceMesh mesh = (PolyfaceMesh) entity;
                    foreach (PolyfaceMeshFace face in mesh.Faces)
                    {
                        if (face.Layer != null)
                        {
                            face.Layer = this.layers.Add(face.Layer, assignHandle);
                            this.layers.References[face.Layer.Name].Add(mesh);
                        }
                    }
                    mesh.PolyfaceMeshFaceLayerChanged += this.Entity_LayerChanged;
                    break;
                case EntityType.PolygonMesh:
                    break;
                case EntityType.Polyline2D:
                    break;
                case EntityType.Polyline3D:
                    break;
                case EntityType.Solid:
                    break;
                case EntityType.Spline:
                    break;
                case EntityType.Trace:
                    break;
                case EntityType.Mesh:
                    break;
                case EntityType.Text:
                    Text text = (Text) entity;
                    text.Style = this.textStyles.Add(text.Style, assignHandle);
                    this.textStyles.References[text.Style.Name].Add(text);
                    text.TextStyleChanged += this.Entity_TextStyleChanged;
                    break;
                case EntityType.MText:
                    MText mText = (MText) entity;
                    mText.Style = this.textStyles.Add(mText.Style, assignHandle);
                    this.textStyles.References[mText.Style.Name].Add(mText);
                    mText.TextStyleChanged += this.Entity_TextStyleChanged;
                    break;
                case EntityType.Image:
                    Image image = (Image) entity;
                    image.Definition = this.imageDefs.Add(image.Definition, assignHandle);
                    this.imageDefs.References[image.Definition.Name].Add(image);
                    image.ImageDefinitionChanged += this.Image_ImageDefinitionChanged;
                    break;
                case EntityType.MLine:
                    MLine mline = (MLine) entity;
                    mline.Style = this.mlineStyles.Add(mline.Style, assignHandle);
                    this.mlineStyles.References[mline.Style.Name].Add(mline);
                    mline.MLineStyleChanged += this.MLine_MLineStyleChanged;
                    break;
                case EntityType.Ray:
                    break;
                case EntityType.XLine:
                    break;
                case EntityType.Underlay:
                    Underlay underlay = (Underlay) entity;
                    switch (underlay.Definition.Type)
                    {
                        case UnderlayType.DGN:
                            underlay.Definition = this.underlayDgnDefs.Add((UnderlayDgnDefinition)underlay.Definition, assignHandle);
                            this.underlayDgnDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                        case UnderlayType.DWF:
                            underlay.Definition = this.underlayDwfDefs.Add((UnderlayDwfDefinition)underlay.Definition, assignHandle);
                            this.underlayDwfDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                        case UnderlayType.PDF:
                            underlay.Definition = this.underlayPdfDefs.Add((UnderlayPdfDefinition)underlay.Definition, assignHandle);
                            this.underlayPdfDefs.References[underlay.Definition.Name].Add(underlay);
                            break;
                    }
                    underlay.UnderlayDefinitionChanged += this.Underlay_UnderlayDefinitionChanged;
                    break;
                case EntityType.Wipeout:
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport) entity;
                    for (int i = 0; i < viewport.FrozenLayers.Count; i++)
                    {
                        viewport.FrozenLayers[i] = this.layers.Add(viewport.FrozenLayers[i], assignHandle);
                    }
                    viewport.ClippingBoundaryAdded += this.Viewport_ClippingBoundaryAdded;
                    viewport.ClippingBoundaryRemoved += this.Viewport_ClippingBoundaryRemoved;
                    break;
                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown.");
            }

            entity.Layer = this.layers.Add(entity.Layer, assignHandle);
            this.layers.References[entity.Layer.Name].Add(entity);

            entity.Linetype = this.linetypes.Add(entity.Linetype, assignHandle);
            this.linetypes.References[entity.Linetype.Name].Add(entity);

            this.AddedObjects.Add(entity.Handle, entity);

            entity.LayerChanged += this.Entity_LayerChanged;
            entity.LinetypeChanged += this.Entity_LinetypeChanged;
        }

        internal void AddAttributeDefinitionToDocument(AttributeDefinition attDef, bool assignHandle)
        {
            // null entities are not allowed
            if (attDef == null)
            {
                throw new ArgumentNullException(nameof(attDef));
            }

            // assign a handle
            if (assignHandle || string.IsNullOrEmpty(attDef.Handle))
            {
                this.NumHandles = attDef.AssignHandle(this.NumHandles);
            }

            attDef.Style = this.textStyles.Add(attDef.Style, assignHandle);
            this.textStyles.References[attDef.Style.Name].Add(attDef);
            attDef.TextStyleChange += this.Entity_TextStyleChanged;

            attDef.Layer = this.layers.Add(attDef.Layer, assignHandle);
            this.layers.References[attDef.Layer.Name].Add(attDef);

            attDef.Linetype = this.linetypes.Add(attDef.Linetype, assignHandle);
            this.linetypes.References[attDef.Linetype.Name].Add(attDef);

            this.AddedObjects.Add(attDef.Handle, attDef);

            attDef.LayerChanged += this.Entity_LayerChanged;
            attDef.LinetypeChanged += this.Entity_LinetypeChanged;

        }

        internal bool RemoveEntityFromDocument(EntityObject entity)
        {
            // the entities that are part of a block do not belong to any of the entities lists but to the block definition
            // and they will not be removed from the drawing database
            switch (entity.Type)
            {
                case EntityType.Arc:
                    break;
                case EntityType.Circle:
                    break;
                case EntityType.Dimension:
                    Dimension dim = (Dimension)entity;
                    if (dim.Block != null)
                    {
                        this.blocks.References[dim.Block.Name].Remove(entity);
                        dim.Block = null;
                    }

                    dim.DimensionBlockChanged -= this.Dimension_DimBlockChanged;
                    this.dimStyles.References[dim.Style.Name].Remove(entity);
                    dim.DimensionStyleChanged -= this.Dimension_DimStyleChanged;

                    this.RemoveDimensionStyleOverridesReferencedDxfObjects(dim, dim.StyleOverrides);
                    dim.DimensionStyleOverrideAdded -= this.Dimension_DimStyleOverrideAdded;
                    dim.DimensionStyleOverrideRemoved -= this.Dimension_DimStyleOverrideRemoved;
                    break;
                case EntityType.Leader:
                    Leader leader = (Leader)entity;
                    this.dimStyles.References[leader.Style.Name].Remove(entity);
                    leader.LeaderStyleChanged -= this.Leader_DimStyleChanged;
                    if (leader.Annotation != null)
                    {
                        leader.Annotation.RemoveReactor(leader);
                        this.Entities.Remove(leader.Annotation);
                    }
                    this.RemoveDimensionStyleOverridesReferencedDxfObjects(leader, leader.StyleOverrides);
                    leader.DimensionStyleOverrideAdded -= this.Leader_DimStyleOverrideAdded;
                    leader.DimensionStyleOverrideRemoved -= this.Leader_DimStyleOverrideRemoved;
                    leader.AnnotationAdded -= this.Leader_AnnotationAdded;
                    leader.AnnotationRemoved -= this.Leader_AnnotationRemoved;
                    break;
                case EntityType.Tolerance:
                    Tolerance tolerance = (Tolerance)entity;
                    this.dimStyles.References[tolerance.Style.Name].Remove(entity);
                    tolerance.ToleranceStyleChanged -= this.Tolerance_DimStyleChanged;
                    break;
                case EntityType.Ellipse:
                    break;
                case EntityType.Face3D:
                    break;
                case EntityType.Spline:
                    break;
                case EntityType.Hatch:
                    Hatch hatch = (Hatch)entity;
                    hatch.UnLinkBoundary(); // remove reactors, the entities that made the hatch boundary will not be automatically deleted                   
                    hatch.HatchBoundaryPathAdded -= this.Hatch_BoundaryPathAdded;
                    hatch.HatchBoundaryPathRemoved -= this.Hatch_BoundaryPathRemoved;
                    break;
                case EntityType.Insert:
                    Insert insert = (Insert)entity;
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
                    //insert.BlockChanged -= this.Insert_BlockChanged;
                    break;
                case EntityType.Line:
                    break;
                case EntityType.Shape:
                    Shape shape = (Shape)entity;
                    this.shapeStyles.References[shape.Style.Name].Remove(entity);
                    shape.StyleChanged -= this.Shape_StyleChanged;
                    break;
                case EntityType.Point:
                    break;
                case EntityType.PolyfaceMesh:
                    PolyfaceMesh mesh = (PolyfaceMesh) entity;
                    foreach (PolyfaceMeshFace face in mesh.Faces)
                    {
                        this.layers.References[face.Layer.Name].Remove(mesh);
                    }
                    mesh.PolyfaceMeshFaceLayerChanged -= this.Entity_LayerChanged;
                    break;
                case EntityType.PolygonMesh:
                    break;
                case EntityType.Polyline2D:
                    break;
                case EntityType.Polyline3D:
                    break;
                case EntityType.Solid:
                    break;
                case EntityType.Trace:
                    break;
                case EntityType.Mesh:
                    break;
                case EntityType.Text:
                    Text text = (Text)entity;
                    this.textStyles.References[text.Style.Name].Remove(entity);
                    text.TextStyleChanged -= this.Entity_TextStyleChanged;
                    break;
                case EntityType.MText:
                    MText mText = (MText)entity;
                    this.textStyles.References[mText.Style.Name].Remove(entity);
                    mText.TextStyleChanged -= this.Entity_TextStyleChanged;
                    break;
                case EntityType.Image:
                    Image image = (Image)entity;
                    this.imageDefs.References[image.Definition.Name].Remove(image);
                    image.ImageDefinitionChanged -= this.Image_ImageDefinitionChanged;
                    break;
                case EntityType.MLine:
                    MLine mline = (MLine)entity;
                    this.mlineStyles.References[mline.Style.Name].Remove(entity);
                    mline.MLineStyleChanged -= this.MLine_MLineStyleChanged;
                    break;
                case EntityType.Ray:
                    break;
                case EntityType.XLine:
                    break;
                case EntityType.Underlay:
                    Underlay underlay = (Underlay)entity;
                    switch (underlay.Definition.Type)
                    {
                        case UnderlayType.DGN:
                            this.underlayDgnDefs.References[underlay.Definition.Name].Remove(underlay);
                            break;
                        case UnderlayType.DWF:
                            this.underlayDwfDefs.References[underlay.Definition.Name].Remove(underlay);
                            break;
                        case UnderlayType.PDF:
                            this.underlayPdfDefs.References[underlay.Definition.Name].Remove(underlay);
                            break;
                    }
                    underlay.UnderlayDefinitionChanged -= this.Underlay_UnderlayDefinitionChanged;
                    break;
                case EntityType.Wipeout:
                    break;
                case EntityType.Viewport:
                    Viewport viewport = (Viewport)entity;
                    // delete the viewport boundary entity in case there is one
                    if (viewport.ClippingBoundary != null)
                    {
                        viewport.ClippingBoundary.RemoveReactor(viewport);
                        this.Entities.Remove(viewport.ClippingBoundary);
                    }
                    viewport.ClippingBoundaryAdded -= this.Viewport_ClippingBoundaryAdded;
                    viewport.ClippingBoundaryRemoved -= this.Viewport_ClippingBoundaryRemoved;

                    break;
                default:
                    throw new ArgumentException("The entity " + entity.Type + " is not implemented or unknown");
            }

            this.layers.References[entity.Layer.Name].Remove(entity);
            this.linetypes.References[entity.Linetype.Name].Remove(entity);
            this.AddedObjects.Remove(entity.Handle);

            entity.LayerChanged -= this.Entity_LayerChanged;
            entity.LinetypeChanged -= this.Entity_LinetypeChanged;

            entity.Handle = null;
            entity.Owner = null;

            return true;
        }

        internal bool RemoveAttributeDefinitionFromDocument(AttributeDefinition attDef)
        {
            this.textStyles.References[attDef.Style.Name].Remove(attDef);
            attDef.TextStyleChange -= this.Entity_TextStyleChanged;

            this.layers.References[attDef.Layer.Name].Remove(attDef);
            this.linetypes.References[attDef.Linetype.Name].Remove(attDef);
            this.AddedObjects.Remove(attDef.Handle);

            attDef.LayerChanged -= this.Entity_LayerChanged;
            attDef.LinetypeChanged -= this.Entity_LinetypeChanged;

            attDef.Handle = null;
            attDef.Owner = null;

            return true;
        }

        #endregion

        #region private methods

        private void AddDimensionStyleOverridesReferencedDxfObjects(EntityObject entity, DimensionStyleOverrideDictionary overrides, bool assignHandle)
        {
            // add the style override referenced DxfObjects

            // add referenced text style
            if (overrides.TryGetValue(DimensionStyleOverrideType.TextStyle, out DimensionStyleOverride styleOverride))
            {
                TextStyle txtStyle = (TextStyle) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.textStyles.Add(txtStyle, assignHandle));
                this.textStyles.References[txtStyle.Name].Add(entity);
            }

            // add referenced blocks
            if (overrides.TryGetValue(DimensionStyleOverrideType.LeaderArrow, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            if (overrides.TryGetValue(DimensionStyleOverrideType.DimArrow1, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            if (overrides.TryGetValue(DimensionStyleOverrideType.DimArrow2, out styleOverride))
            {
                Block block = (Block) styleOverride.Value;
                if (block != null)
                {
                    overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.blocks.Add(block, assignHandle));
                    this.blocks.References[block.Name].Add(entity);
                }
            }

            // add referenced line types
            if (overrides.TryGetValue(DimensionStyleOverrideType.DimLineLinetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }

            if (overrides.TryGetValue(DimensionStyleOverrideType.ExtLine1Linetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }

            if (overrides.TryGetValue(DimensionStyleOverrideType.ExtLine2Linetype, out styleOverride))
            {
                Linetype linetype = (Linetype) styleOverride.Value;
                overrides[styleOverride.Type] = new DimensionStyleOverride(styleOverride.Type, this.linetypes.Add(linetype, assignHandle));
                this.linetypes.References[linetype.Name].Add(entity);
            }
        }

        private void RemoveDimensionStyleOverridesReferencedDxfObjects(EntityObject entity, DimensionStyleOverrideDictionary overrides)
        {
            // remove the style override referenced DxfObjects
            DimensionStyleOverride styleOverride;

            // remove referenced text style
            overrides.TryGetValue(DimensionStyleOverrideType.TextStyle, out styleOverride);
            if (styleOverride != null)
            {
                TextStyle txtStyle = (TextStyle) styleOverride.Value;
                this.textStyles.References[txtStyle.Name].Remove(entity);
            }

            // remove referenced blocks
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

            // remove referenced line types
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

        private void AddDefaultObjects()
        {
            // collections
            this.vports = new VPorts(this);
            this.views = new Views(this);
            this.appRegistries = new ApplicationRegistries(this);
            this.layers = new Layers(this);
            this.linetypes = new Linetypes(this);
            this.textStyles = new TextStyles(this);
            this.shapeStyles = new ShapeStyles(this);
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
            this.RasterVariables = new RasterVariables(this);
        }

        #endregion

        #region EntityObject events

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
            if (e.OldValue != null)
            {
                this.blocks.References[e.OldValue.Name].Remove(sender);
                this.blocks.Remove(e.OldValue);
            }

            if (e.NewValue != null)
            {
                if(!e.NewValue.Name.StartsWith("*D")) e.NewValue.SetName("*D" + ++this.DimensionBlocksIndex, false);
                e.NewValue = this.blocks.Add(e.NewValue);
                this.blocks.References[e.NewValue.Name].Add(sender);
            }
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
                    {
                        // the block might be defined as null to indicate that the default arrowhead will be used
                        return;
                    }
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

        private void Leader_AnnotationAdded(Leader sender, EntityChangeEventArgs e)
        {
            Layout layout = sender.Owner.Record.Layout;
            // the viewport belongs to a layout
            if (e.Item.Owner != null)
            {
                // the viewport clipping boundary and its entities must belong to the same document or block
                if (!ReferenceEquals(e.Item.Owner.Record.Layout, layout))
                    throw new ArgumentException("The leader annotation entity and the Leader entity must belong to the same layout and document. Clone it instead.");
                // there is no need to do anything else we will not add the same entity twice
            }
            else
            {
                // we will add the new entity to the same document and layout of the hatch
                this.blocks[layout.AssociatedBlock.Name].Entities.Add(e.Item);
            }
        }

        private void Leader_AnnotationRemoved(Leader sender, EntityChangeEventArgs e)
        {
            this.Entities.Remove(e.Item);
        }

        private void Tolerance_DimStyleChanged(Tolerance sender, TableObjectChangedEventArgs<DimensionStyle> e)
        {
            if (e.OldValue != null)
            {
                this.dimStyles.References[e.OldValue.Name].Remove(sender);
            }
            if (e.NewValue == null)
            {
                return;
            }
            e.NewValue = this.dimStyles.Add(e.NewValue);
            this.dimStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_TextStyleChanged(DxfObject sender, TableObjectChangedEventArgs<TextStyle> e)
        {
            if (e.OldValue != null)
            {
                this.textStyles.References[e.OldValue.Name].Remove(sender);
            }
            if (e.NewValue == null)
            {
                return;
            }
            e.NewValue = this.textStyles.Add(e.NewValue);
            this.textStyles.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_LinetypeChanged(DxfObject sender, TableObjectChangedEventArgs<Linetype> e)
        {
            if (e.OldValue != null)
            {
                this.linetypes.References[e.OldValue.Name].Remove(sender);
            }
            if (e.NewValue == null)
            {
                return;
            }
            e.NewValue = this.linetypes.Add(e.NewValue);
            this.linetypes.References[e.NewValue.Name].Add(sender);
        }

        private void Entity_LayerChanged(DxfObject sender, TableObjectChangedEventArgs<Layer> e)
        {
            if (e.OldValue != null)
            {
                this.layers.References[e.OldValue.Name].Remove(sender);
            }
            if (e.NewValue == null)
            {
                return;
            }
            e.NewValue = this.layers.Add(e.NewValue);
            this.layers.References[e.NewValue.Name].Add(sender);
        }

        private void Insert_AttributeAdded(Insert sender, AttributeChangeEventArgs e)
        {
            this.NumHandles = e.Item.AssignHandle(this.NumHandles);

            e.Item.Layer = this.layers.Add(e.Item.Layer);
            this.layers.References[e.Item.Layer.Name].Add(e.Item);
            e.Item.LayerChanged += this.Entity_LayerChanged;

            e.Item.Linetype = this.linetypes.Add(e.Item.Linetype);
            this.linetypes.References[e.Item.Linetype.Name].Add(e.Item);
            e.Item.LinetypeChanged += this.Entity_LinetypeChanged;

            e.Item.Style = this.textStyles.Add(e.Item.Style);
            this.textStyles.References[e.Item.Style.Name].Add(e.Item);
            e.Item.TextStyleChanged += this.Entity_TextStyleChanged;
        }

        private void Insert_AttributeRemoved(Insert sender, AttributeChangeEventArgs e)
        {
            this.layers.References[e.Item.Layer.Name].Remove(e.Item);
            e.Item.LayerChanged -= this.Entity_LayerChanged;

            this.linetypes.References[e.Item.Linetype.Name].Remove(e.Item);
            e.Item.LinetypeChanged -= this.Entity_LinetypeChanged;

            this.textStyles.References[e.Item.Style.Name].Remove(e.Item);
            e.Item.TextStyleChanged -= this.Entity_TextStyleChanged;
        }

        //private void Insert_BlockChanged(Insert sender, TableObjectChangedEventArgs<Block> e)
        //{
        //    this.blocks.References[e.OldValue.Name].Remove(sender);

        //    e.NewValue = this.blocks.Add(e.NewValue);
        //    this.blocks.References[e.NewValue.Name].Add(sender);
        //}

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
                    {
                        throw new ArgumentException("The HatchBoundaryPath entity and the Hatch entity must belong to the same layout and document. Clone it instead.");
                    }
                    // there is no need to do anything else we will not add the same entity twice
                }
                else
                {
                    // we will add the new entity to the same document and layout of the hatch
                    this.blocks[layout.AssociatedBlock.Name].Entities.Add(entity);
                }
            }
        }

        private void Hatch_BoundaryPathRemoved(Hatch sender, ObservableCollectionEventArgs<HatchBoundaryPath> e)
        {
            foreach (EntityObject entity in e.Item.Entities)
            {
                this.Entities.Remove(entity);
            }
        }

        private void Viewport_ClippingBoundaryAdded(Viewport sender, EntityChangeEventArgs e)
        {
            Layout layout = sender.Owner.Record.Layout;
            // the viewport belongs to a layout
            if (e.Item.Owner != null)
            {
                // the viewport clipping boundary and its entities must belong to the same document or block
                if (!ReferenceEquals(e.Item.Owner.Record.Layout, layout))
                {
                    throw new ArgumentException("The viewport clipping boundary entity and the Viewport entity must belong to the same layout and document. Clone it instead.");
                }
                // there is no need to do anything else we will not add the same entity twice
            }
            else
            {
                // we will add the new entity to the same document and layout of the hatch
                this.blocks[layout.AssociatedBlock.Name].Entities.Add(e.Item);
            }
        }

        private void Viewport_ClippingBoundaryRemoved(Viewport sender, EntityChangeEventArgs e)
        {
            this.Entities.Remove(e.Item);
        }

        private void Image_ImageDefinitionChanged(Image sender, TableObjectChangedEventArgs<ImageDefinition> e)
        {
            this.imageDefs.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.imageDefs.Add(e.NewValue);
            this.imageDefs.References[e.OldValue.Name].Add(sender);
        }

        private void Underlay_UnderlayDefinitionChanged(Underlay sender, TableObjectChangedEventArgs<UnderlayDefinition> e)
        {
            switch (e.OldValue.Type)
            {
                case UnderlayType.DGN:
                    this.underlayDgnDefs.References[e.OldValue.Name].Remove(sender);
                    break;
                case UnderlayType.DWF:
                    this.underlayDwfDefs.References[e.OldValue.Name].Remove(sender);
                    break;
                case UnderlayType.PDF:
                    this.underlayPdfDefs.References[e.OldValue.Name].Remove(sender);
                    break;
            }


            switch (e.NewValue.Type)
            {
                case UnderlayType.DGN:
                    e.NewValue = this.underlayDgnDefs.Add((UnderlayDgnDefinition) e.NewValue);
                    this.underlayDgnDefs.References[e.NewValue.Name].Add(sender);
                    break;
                case UnderlayType.DWF:
                    e.NewValue = this.underlayDwfDefs.Add((UnderlayDwfDefinition) e.NewValue);
                    this.underlayDwfDefs.References[e.NewValue.Name].Add(sender);
                    break;
                case UnderlayType.PDF:
                    e.NewValue = this.underlayPdfDefs.Add((UnderlayPdfDefinition) e.NewValue);
                    this.underlayPdfDefs.References[e.NewValue.Name].Add(sender);
                    break;
            }
        }

        private void Shape_StyleChanged(Shape sender, TableObjectChangedEventArgs<ShapeStyle> e)
        {
            this.shapeStyles.References[e.OldValue.Name].Remove(sender);

            e.NewValue = this.shapeStyles.Add(e.NewValue);
            this.shapeStyles.References[e.NewValue.Name].Add(sender);
        }

        #endregion

        #region DxfObject events

        private void AddedObjects_BeforeAddItem(ObservableDictionary<string, DxfObject> sender, ObservableDictionaryEventArgs<string, DxfObject> e)
        {
        }

        private void AddedObjects_AddItem(ObservableDictionary<string, DxfObject> sender, ObservableDictionaryEventArgs<string, DxfObject> e)
        {
            DxfObject o = e.Item.Value;
            if (o != null)
            {
                foreach (string appReg in o.XData.AppIds)
                {
                    o.XData[appReg].ApplicationRegistry = this.appRegistries.Add(o.XData[appReg].ApplicationRegistry);
                    this.appRegistries.References[appReg].Add(e.Item.Value);
                }

                o.XDataAddAppReg += this.DxfObject_XDataAddAppReg;
                o.XDataRemoveAppReg += this.DxfObject_XDataRemoveAppReg;
            }
        }

        private void AddedObjects_BeforeRemoveItem(ObservableDictionary<string, DxfObject> sender, ObservableDictionaryEventArgs<string, DxfObject> e)
        {           
        }

        private void AddedObjects_RemoveItem(ObservableDictionary<string, DxfObject> sender, ObservableDictionaryEventArgs<string, DxfObject> e)
        {
            DxfObject o = e.Item.Value;
            if (o != null)
            {
                foreach (string appReg in o.XData.AppIds)
                {
                    this.appRegistries.References[appReg].Remove(e.Item.Value);
                }
                o.XDataAddAppReg -= this.DxfObject_XDataAddAppReg;
                o.XDataRemoveAppReg -= this.DxfObject_XDataRemoveAppReg;
            }
        }

        private void DxfObject_XDataAddAppReg(DxfObject sender, ObservableCollectionEventArgs<ApplicationRegistry> e)
        {
            sender.XData[e.Item.Name].ApplicationRegistry = this.appRegistries.Add(sender.XData[e.Item.Name].ApplicationRegistry);
            this.appRegistries.References[e.Item.Name].Add(sender);
        }

        private void DxfObject_XDataRemoveAppReg(DxfObject sender, ObservableCollectionEventArgs<ApplicationRegistry> e)
        {
            this.appRegistries.References[e.Item.Name].Remove(sender);
        }

        #endregion
    }
}