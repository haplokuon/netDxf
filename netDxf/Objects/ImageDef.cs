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
using System.Drawing;
using System.IO;
using netDxf.Tables;

namespace netDxf.Objects
{
    /// <summary>
    /// Defines the image resolution units.
    /// </summary>
    public enum ResolutionUnits
    {
        /// <summary>
        /// No units.
        /// </summary>
        NoUnits = 0,

        /// <summary>
        /// Centimeters.
        /// </summary>
        Centimeters = 2,

        /// <summary>
        /// Inches.
        /// </summary>
        Inches = 5
    }

    /// <summary>
    /// Supported image formats.
    /// </summary>
    /// <remarks>
    /// These are the image formats in common between the net framework and AutoCAD
    /// </remarks>
    public enum SupportedImageFormats
    {
        /// <summary>
        /// Bmp image format.
        /// </summary>
        Bmp,
        /// <summary>
        /// Jpg image format.
        /// </summary>
        Jpeg,
        /// <summary>
        /// Png image format.
        /// </summary>
        Png,
        /// <summary>
        /// Tiff image format.
        /// </summary>
        Tiff
    }

    /// <summary>
    /// Represents a image definition.
    /// </summary>
    public class ImageDef :
        TableObject
    {
        #region private fields

        private readonly string fileName;
        private readonly int width;
        private readonly int height;
        private readonly ResolutionUnits resolutionUnits;
        private readonly Vector2 onePixelSize;
        private readonly float horizontalResolution;
        private readonly float verticalResolution;

        // this will store the references to the images that makes use of this image definition (key: image handle, value: reactor)
        private readonly Dictionary<string, ImageDefReactor> reactors;

        #endregion

        #region contructors

        /// <summary>
        /// Initializes a new instance of the <c>ImageDef</c> class.
        /// </summary>
        /// <param name="fileName">Image file name with full or relative path.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="horizontalResolution">Image horizontal resolution in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="verticalResolution">Image vetical resolution in pixels.</param>
        /// <param name="units">Image resolution units.</param>
        /// <remarks>
        /// <para>
        /// The name of the file without extension will be used as the name of the image definition.
        /// </para>
        /// <para>
        /// This is a generic constructor for all image formats supported by AutoCAD, note that not all AutoCAD versions support the same image formats.
        /// </para>
        /// <para>
        /// Note (this is from the ACAD docs): AutoCAD 2000, AutoCAD LT 2000, and later releases do not support LZW-compressed TIFF files,
        /// with the exception of English language versions sold in the US and Canada.<br />
        /// If you have TIFF files that were created using LZW compression and want to insert them into a drawing 
        /// you must resave the TIFF files with LZW compression disabled.
        /// </para>
        /// </remarks>
        public ImageDef(string fileName, int width, float horizontalResolution, int height, float verticalResolution, ResolutionUnits units)
            : this(fileName, width, horizontalResolution, height, verticalResolution, Path.GetFileNameWithoutExtension(fileName), units)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>ImageDef</c> class.
        /// </summary>
        /// <param name="fileName">Image file name with full or relative path.</param>
        /// <param name="width">Image width in pixels.</param>
        /// <param name="horizontalResolution">Image horizontal resolution in pixels.</param>
        /// <param name="height">Image height in pixels.</param>
        /// <param name="verticalResolution">Image vertical resolution in pixels.</param>
        /// <param name="name">Image definition name, if null or empty the file name without the extension will be used.</param>
        /// <param name="units">Image resolution units.</param>
        /// <remarks>
        /// <para>
        /// The name assigned to the image definition must be unique.
        /// </para>
        /// <para>
        /// This is a generic constructor for all image formats supported by AutoCAD, note that not all AutoCAD versions support the same image formats.
        /// </para>
        /// <para>
        /// Note (this is from the ACAD docs): AutoCAD 2000, AutoCAD LT 2000, and later releases do not support LZW-compressed TIFF files,
        /// with the exception of English language versions sold in the US and Canada.<br />
        /// If you have TIFF files that were created using LZW compression and want to insert them into a drawing 
        /// you must resave the TIFF files with LZW compression disabled.
        /// </para>
        /// </remarks>
        public ImageDef(string fileName, int width, float horizontalResolution, int height, float verticalResolution, string name, ResolutionUnits units)
            : base(name, DxfObjectCode.ImageDef, true)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "The image file name cannot be empty or null.");

            //FileInfo info = new FileInfo(fileName);
            //if (!info.Exists)
            //    throw new FileNotFoundException("Image file not found", fileName);

            this.fileName = fileName;
            this.width = width;
            this.height = height;
            this.horizontalResolution = horizontalResolution;
            this.verticalResolution = verticalResolution;
            this.resolutionUnits = units;

            // pixel size use the units defined in the document, it is controlled by the header variable $INSUNITS and the RasterVariables units
            this.onePixelSize = new Vector2(25.4/horizontalResolution, 25.4/verticalResolution);

            this.reactors = new Dictionary<string, ImageDefReactor>();
        }

        ///  <summary>
        ///  Initializes a new instance of the <c>ImageDef</c> class.
        ///  </summary>
        ///  <param name="fileName">Image file name with full or relative path.</param>
        /// <param name="units">Image resolution units, by defult centimeters will be used.</param>
        /// <remarks>
        ///  <para>
        ///  The name of the file without extension will be used as the name of the image definition.
        ///  </para>
        ///  <para>
        ///  Supported image formats: BMP, JPG, PNG, TIFF.<br />
        ///  Eventhought AutoCAD supports more image formats, this constructor is restricted to the ones the net framework supports in common with AutoCAD.
        ///  Use the generic constructor instead.
        ///  </para>
        ///  <para>
        ///  Note (this is from the ACAD docs): AutoCAD 2000, AutoCAD LT 2000, and later releases do not support LZW-compressed TIFF files,
        ///  with the exception of English language versions sold in the US and Canada.<br />
        ///  If you have TIFF files that were created using LZW compression and want to insert them into a drawing 
        ///  you must resave the TIFF files with LZW compression disabled.
        ///  </para>
        /// </remarks>
        public ImageDef(string fileName, ResolutionUnits units = ResolutionUnits.Centimeters)
            : this(fileName, Path.GetFileNameWithoutExtension(fileName), units)
        {
        }

        ///  <summary>
        ///  Initializes a new instance of the <c>ImageDef</c> class.
        ///  </summary>
        ///  <param name="fileName">Image file name with full or relative path.</param>
        ///  <param name="name">Image definition name, if null or empty the file name without the extension will be used.</param>
        /// <param name="units">Image resolution units, by defult centimeters will be used.</param>
        /// <remarks>
        ///  <para>
        ///  The name assigned to the image definition must be unique.
        ///  </para>
        ///  <para>
        ///  Supported image formats: BMP, JPG, PNG, TIFF.<br />
        ///  Eventhought AutoCAD supports more image formats, this constructor is restricted to the ones the .net library supports in common with AutoCAD.
        ///  Use the generic constructor instead.
        ///  </para>
        ///  <para>
        ///  Note (this is from the ACAD docs): AutoCAD 2000, AutoCAD LT 2000, and later releases do not support LZW-compressed TIFF files,
        ///  with the exception of English language versions sold in the US and Canada.<br />
        ///  If you have TIFF files that were created using LZW compression and want to insert them into a drawing 
        ///  you must resave the TIFF files with LZW compression disabled.
        ///  </para>
        /// </remarks>
        public ImageDef(string fileName, string name, ResolutionUnits units = ResolutionUnits.Centimeters)
            : base(name, DxfObjectCode.ImageDef, true)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "The image file name cannot be empty or null.");

            FileInfo info = new FileInfo(fileName);
            if (!info.Exists)
                throw new FileNotFoundException("Image file not found", fileName);

            this.fileName = fileName;

            try
            {
                using (Image bitmap = Image.FromFile(fileName))
                {
                    this.width = bitmap.Width;
                    this.height = bitmap.Height;
                    if (units == ResolutionUnits.Centimeters)
                    {
                        // the System.Drawing.Image stores the image resolution in inches
                        this.horizontalResolution = bitmap.HorizontalResolution/2.54f;
                        this.verticalResolution = bitmap.VerticalResolution/2.54f;
                    }
                    else
                    {
                        this.horizontalResolution = bitmap.HorizontalResolution;
                        this.verticalResolution = bitmap.VerticalResolution;
                    }
                    this.resolutionUnits = units;

                    // pixel size use the units defined in the document, it is controlled by the header variable $INSUNITS
                    this.onePixelSize = new Vector2(25.4/bitmap.HorizontalResolution, 25.4/bitmap.VerticalResolution);
                }
            }
            catch (Exception)
            {
                throw new ArgumentException("Image file not supported.", fileName);
            }

            this.reactors = new Dictionary<string, ImageDefReactor>();
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the image path.
        /// </summary>
        public string FileName
        {
            get { return fileName; }
        }

        /// <summary>
        /// Gets the image width in pixels.
        /// </summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the image height in pixels.
        /// </summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        /// Gets the image resolution units used to calculate the one pixel size value.
        /// </summary>
        public ResolutionUnits ResolutionUnits
        {
            get { return resolutionUnits; }
        }

        /// <summary>
        /// Gets the default size of one pixel in AutoCAD units.
        /// </summary>
        public Vector2 OnePixelSize
        {
            get { return onePixelSize; }
        }

        /// <summary>
        /// Gets the image horizontal resolution in pixels per unit.
        /// </summary>
        public float HorizontalResolution
        {
            get { return horizontalResolution; }
        }

        /// <summary>
        /// Gets the image vertical resolution in pixels per unit.
        /// </summary>
        public float VerticalResolution
        {
            get { return verticalResolution; }
        }

        #endregion

        #region internal properties

        internal Dictionary<string, ImageDefReactor> Reactors
        {
            get { return reactors; }
        }

        #endregion
    }
}