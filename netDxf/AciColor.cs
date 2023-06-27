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
using System.Drawing;
using System.Globalization;
using System.Threading;

namespace netDxf
{
    /// <summary>
    /// Represents an ACI color (AutoCAD Color Index) that also supports true color.
    /// </summary>
    public class AciColor :
        ICloneable,
        IEquatable<AciColor>
    {
        #region list of the indexed colors

        private static readonly IReadOnlyList<byte[]> indexRgb = new List<byte[]>
        {
            new byte[] { 255, 255, 255 },
            new byte[] { 255, 0, 0 },
            new byte[] { 255, 255, 0 },
            new byte[] { 0, 255, 0 },
            new byte[] { 0, 255, 255 },
            new byte[] { 0, 0, 255 },
            new byte[] { 255, 0, 255 },
            new byte[] { 255, 255, 255 },
            new byte[] { 128, 128, 128 },
            new byte[] { 192, 192, 192 },
            new byte[] { 255, 0, 0 },
            new byte[] { 255, 127, 127 },
            new byte[] { 165, 0, 0 },
            new byte[] { 165, 82, 82 },
            new byte[] { 127, 0, 0 },
            new byte[] { 127, 63, 63 },
            new byte[] { 76, 0, 0 },
            new byte[] { 76, 38, 38 },
            new byte[] { 38, 0, 0 },
            new byte[] { 38, 19, 19 },
            new byte[] { 255, 63, 0 },
            new byte[] { 255, 159, 127 },
            new byte[] { 165, 41, 0 },
            new byte[] { 165, 103, 82 },
            new byte[] { 127, 31, 0 },
            new byte[] { 127, 79, 63 },
            new byte[] { 76, 19, 0 },
            new byte[] { 76, 47, 38 },
            new byte[] { 38, 9, 0 },
            new byte[] { 38, 28, 19 },
            new byte[] { 255, 127, 0 },
            new byte[] { 255, 191, 127 },
            new byte[] { 165, 82, 0 },
            new byte[] { 165, 124, 82 },
            new byte[] { 127, 63, 0 },
            new byte[] { 127, 95, 63 },
            new byte[] { 76, 38, 0 },
            new byte[] { 76, 57, 38 },
            new byte[] { 38, 19, 0 },
            new byte[] { 38, 28, 19 },
            new byte[] { 255, 191, 0 },
            new byte[] { 255, 223, 127 },
            new byte[] { 165, 124, 0 },
            new byte[] { 165, 145, 82 },
            new byte[] { 127, 95, 0 },
            new byte[] { 127, 111, 63 },
            new byte[] { 76, 57, 0 },
            new byte[] { 76, 66, 38 },
            new byte[] { 38, 28, 0 },
            new byte[] { 38, 33, 19 },
            new byte[] { 255, 255, 0 },
            new byte[] { 255, 255, 127 },
            new byte[] { 165, 165, 0 },
            new byte[] { 165, 165, 82 },
            new byte[] { 127, 127, 0 },
            new byte[] { 127, 127, 63 },
            new byte[] { 76, 76, 0 },
            new byte[] { 76, 76, 38 },
            new byte[] { 38, 38, 0 },
            new byte[] { 38, 38, 19 },
            new byte[] { 191, 255, 0 },
            new byte[] { 223, 255, 127 },
            new byte[] { 124, 165, 0 },
            new byte[] { 145, 165, 82 },
            new byte[] { 95, 127, 0 },
            new byte[] { 111, 127, 63 },
            new byte[] { 57, 76, 0 },
            new byte[] { 66, 76, 38 },
            new byte[] { 28, 38, 0 },
            new byte[] { 33, 38, 19 },
            new byte[] { 127, 255, 0 },
            new byte[] { 191, 255, 127 },
            new byte[] { 82, 165, 0 },
            new byte[] { 124, 165, 82 },
            new byte[] { 63, 127, 0 },
            new byte[] { 95, 127, 63 },
            new byte[] { 38, 76, 0 },
            new byte[] { 57, 76, 38 },
            new byte[] { 19, 38, 0 },
            new byte[] { 28, 38, 19 },
            new byte[] { 63, 255, 0 },
            new byte[] { 159, 255, 127 },
            new byte[] { 41, 165, 0 },
            new byte[] { 103, 165, 82 },
            new byte[] { 31, 127, 0 },
            new byte[] { 79, 127, 63 },
            new byte[] { 19, 76, 0 },
            new byte[] { 47, 76, 38 },
            new byte[] { 9, 38, 0 },
            new byte[] { 23, 38, 19 },
            new byte[] { 0, 255, 0 },
            new byte[] { 125, 255, 127 },
            new byte[] { 0, 165, 0 },
            new byte[] { 82, 165, 82 },
            new byte[] { 0, 127, 0 },
            new byte[] { 63, 127, 63 },
            new byte[] { 0, 76, 0 },
            new byte[] { 38, 76, 38 },
            new byte[] { 0, 38, 0 },
            new byte[] { 19, 38, 19 },
            new byte[] { 0, 255, 63 },
            new byte[] { 127, 255, 159 },
            new byte[] { 0, 165, 41 },
            new byte[] { 82, 165, 103 },
            new byte[] { 0, 127, 31 },
            new byte[] { 63, 127, 79 },
            new byte[] { 0, 76, 19 },
            new byte[] { 38, 76, 47 },
            new byte[] { 0, 38, 9 },
            new byte[] { 19, 88, 23 },
            new byte[] { 0, 255, 127 },
            new byte[] { 127, 255, 191 },
            new byte[] { 0, 165, 82 },
            new byte[] { 82, 165, 124 },
            new byte[] { 0, 127, 63 },
            new byte[] { 63, 127, 95 },
            new byte[] { 0, 76, 38 },
            new byte[] { 38, 76, 57 },
            new byte[] { 0, 38, 19 },
            new byte[] { 19, 88, 28 },
            new byte[] { 0, 255, 191 },
            new byte[] { 127, 255, 223 },
            new byte[] { 0, 165, 124 },
            new byte[] { 82, 165, 145 },
            new byte[] { 0, 127, 95 },
            new byte[] { 63, 127, 111 },
            new byte[] { 0, 76, 57 },
            new byte[] { 38, 76, 66 },
            new byte[] { 0, 38, 28 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 255, 255 },
            new byte[] { 127, 255, 255 },
            new byte[] { 0, 165, 165 },
            new byte[] { 82, 165, 165 },
            new byte[] { 0, 127, 127 },
            new byte[] { 63, 127, 127 },
            new byte[] { 0, 76, 76 },
            new byte[] { 38, 76, 76 },
            new byte[] { 0, 38, 38 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 191, 255 },
            new byte[] { 127, 223, 255 },
            new byte[] { 0, 124, 165 },
            new byte[] { 82, 145, 165 },
            new byte[] { 0, 95, 127 },
            new byte[] { 63, 111, 217 },
            new byte[] { 0, 57, 76 },
            new byte[] { 38, 66, 126 },
            new byte[] { 0, 28, 38 },
            new byte[] { 19, 88, 88 },
            new byte[] { 0, 127, 255 },
            new byte[] { 127, 191, 255 },
            new byte[] { 0, 82, 165 },
            new byte[] { 82, 124, 165 },
            new byte[] { 0, 63, 127 },
            new byte[] { 63, 95, 127 },
            new byte[] { 0, 38, 76 },
            new byte[] { 38, 57, 126 },
            new byte[] { 0, 19, 38 },
            new byte[] { 19, 28, 88 },
            new byte[] { 0, 63, 255 },
            new byte[] { 127, 159, 255 },
            new byte[] { 0, 41, 165 },
            new byte[] { 82, 103, 165 },
            new byte[] { 0, 31, 127 },
            new byte[] { 63, 79, 127 },
            new byte[] { 0, 19, 76 },
            new byte[] { 38, 47, 126 },
            new byte[] { 0, 9, 38 },
            new byte[] { 19, 23, 88 },
            new byte[] { 0, 0, 255 },
            new byte[] { 127, 127, 255 },
            new byte[] { 0, 0, 165 },
            new byte[] { 82, 82, 165 },
            new byte[] { 0, 0, 127 },
            new byte[] { 63, 63, 127 },
            new byte[] { 0, 0, 76 },
            new byte[] { 38, 38, 126 },
            new byte[] { 0, 0, 38 },
            new byte[] { 19, 19, 88 },
            new byte[] { 63, 0, 255 },
            new byte[] { 159, 127, 255 },
            new byte[] { 41, 0, 165 },
            new byte[] { 103, 82, 165 },
            new byte[] { 31, 0, 127 },
            new byte[] { 79, 63, 127 },
            new byte[] { 19, 0, 76 },
            new byte[] { 47, 38, 126 },
            new byte[] { 9, 0, 38 },
            new byte[] { 23, 19, 88 },
            new byte[] { 127, 0, 255 },
            new byte[] { 191, 127, 255 },
            new byte[] { 165, 0, 82 },
            new byte[] { 124, 82, 165 },
            new byte[] { 63, 0, 127 },
            new byte[] { 95, 63, 127 },
            new byte[] { 38, 0, 76 },
            new byte[] { 57, 38, 126 },
            new byte[] { 19, 0, 38 },
            new byte[] { 28, 19, 88 },
            new byte[] { 191, 0, 255 },
            new byte[] { 223, 127, 255 },
            new byte[] { 124, 0, 165 },
            new byte[] { 142, 82, 165 },
            new byte[] { 95, 0, 127 },
            new byte[] { 111, 63, 127 },
            new byte[] { 57, 0, 76 },
            new byte[] { 66, 38, 76 },
            new byte[] { 28, 0, 38 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 255 },
            new byte[] { 255, 127, 255 },
            new byte[] { 165, 0, 165 },
            new byte[] { 165, 82, 165 },
            new byte[] { 127, 0, 127 },
            new byte[] { 127, 63, 127 },
            new byte[] { 76, 0, 76 },
            new byte[] { 76, 38, 76 },
            new byte[] { 38, 0, 38 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 191 },
            new byte[] { 255, 127, 223 },
            new byte[] { 165, 0, 124 },
            new byte[] { 165, 82, 145 },
            new byte[] { 127, 0, 95 },
            new byte[] { 127, 63, 111 },
            new byte[] { 76, 0, 57 },
            new byte[] { 76, 38, 66 },
            new byte[] { 38, 0, 28 },
            new byte[] { 88, 19, 88 },
            new byte[] { 255, 0, 127 },
            new byte[] { 255, 127, 191 },
            new byte[] { 165, 0, 82 },
            new byte[] { 165, 82, 124 },
            new byte[] { 127, 0, 63 },
            new byte[] { 127, 63, 95 },
            new byte[] { 76, 0, 38 },
            new byte[] { 76, 38, 57 },
            new byte[] { 38, 0, 19 },
            new byte[] { 88, 19, 28 },
            new byte[] { 255, 0, 63 },
            new byte[] { 255, 127, 159 },
            new byte[] { 165, 0, 41 },
            new byte[] { 165, 82, 103 },
            new byte[] { 127, 0, 31 },
            new byte[] { 127, 63, 79 },
            new byte[] { 76, 0, 19 },
            new byte[] { 76, 38, 47 },
            new byte[] { 38, 0, 9 },
            new byte[] { 88, 19, 23 },
            new byte[] { 0, 0, 0 },
            new byte[] { 101, 101, 101 },
            new byte[] { 102, 102, 102 },
            new byte[] { 153, 153, 153 },
            new byte[] { 204, 204, 204 },
            new byte[] { 255, 255, 255 }
        };

        #endregion

        #region private fields

        private short index;
        private byte r;
        private byte g;
        private byte b;
        private bool useTrueColor;

        #endregion

        #region constants

        /// <summary>
        /// Gets the ByLayer color.
        /// </summary>
        public static AciColor ByLayer
        {
            get { return new AciColor { index = 256 }; }
        }

        /// <summary>
        /// Gets the ByBlock color.
        /// </summary>
        public static AciColor ByBlock
        {
            get { return new AciColor { index = 0 }; }
        }

        /// <summary>
        /// Defines a default red color.
        /// </summary>
        public static AciColor Red
        {
            get { return new AciColor(1); }
        }

        /// <summary>
        /// Defines a default yellow color.
        /// </summary>
        public static AciColor Yellow
        {
            get { return new AciColor(2); }
        }

        /// <summary>
        /// Defines a default green color.
        /// </summary>
        public static AciColor Green
        {
            get { return new AciColor(3); }
        }

        /// <summary>
        /// Defines a default cyan color.
        /// </summary>
        public static AciColor Cyan
        {
            get { return new AciColor(4); }
        }

        /// <summary>
        /// Defines a default blue color.
        /// </summary>
        public static AciColor Blue
        {
            get { return new AciColor(5); }
        }

        /// <summary>
        /// Defines a default magenta color.
        /// </summary>
        public static AciColor Magenta
        {
            get { return new AciColor(6); }
        }

        /// <summary>
        /// Defines a default white/black color.
        /// </summary>
        public static AciColor Default
        {
            get { return new AciColor(7); }
        }

        /// <summary>
        /// Defines a default dark gray color.
        /// </summary>
        public static AciColor DarkGray
        {
            get { return new AciColor(8); }
        }

        /// <summary>
        /// Defines a default light gray color.
        /// </summary>
        public static AciColor LightGray
        {
            get { return new AciColor(9); }
        }

        /// <summary>
        /// A list that contains the indexed colors, the key represents the color index and the value the RGB components of the color.
        /// </summary>
        /// <remarks>
        /// This is the AutoCad default ACI color index to RGB values table.
        /// Changes in the actual view background color in AutoCad might produce changes in the RGB equivalents in some ACI color indexes,
        /// specially the darkest ones.<br />
        /// The color at index zero is not used, represents the RGB values for abstract colors such as ByLayer or ByBlock
        /// </remarks>
        public static IReadOnlyList<byte[]> IndexRgb
        {
            get { return indexRgb; }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class with black/white color index 7.
        /// </summary>
        public AciColor()
            : this(7)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class from an array of three values.
        /// </summary>
        /// <param name="rgb">RGB components (input values range from 0 to 255). The array must contain three values.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(byte[] rgb)
            : this(rgb[0], rgb[1], rgb[2])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        ///<param name="r">Red component (input values range from 0 to 255).</param>
        ///<param name="g">Green component (input values range from 0 to 255).</param>
        ///<param name="b">Blue component (input values range from 0 to 255).</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.useTrueColor = true;
            this.index = RgbToAci(this.r, this.g, this.b);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class from an array of three values.
        /// </summary>
        /// <param name="rgb">RGB components (input values range from 0 to 1). The array must contain three values.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(double[] rgb)
            : this(rgb[0], rgb[1], rgb[2])
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        /// <param name="r">Red component (input values range from 0 to 1).</param>
        /// <param name="g">Green component (input values range from 0 to 1).</param>
        /// <param name="b">Blue component (input values range from 0 to 1).</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(double r, double g, double b)
        {
            if (r < 0 || r > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(r), r, "Red component input values range from 0 to 1.");
            }
            if (g < 0 || g > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(g), g, "Green component input values range from 0 to 1.");
            }
            if (b < 0 || b > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(b), b, "Blue component input values range from 0 to 1.");
            }

            this.r = (byte) Math.Round(r * 255);
            this.g = (byte) Math.Round(g * 255);
            this.b = (byte) Math.Round(b * 255);
            this.useTrueColor = true;
            this.index = RgbToAci(this.r, this.g, this.b);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        /// <param name="color">A <see cref="Color">color</see>.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(Color color)
            : this(color.R, color.G, color.B)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        /// <param name="index">Color index.</param>
        /// <remarks>
        /// By default the UseTrueColor will be set to false.<br />
        /// Accepted color index values range from 1 to 255.<br />
        /// Indexes from 1 to 255 represents a color, the index 0 and 256 are reserved for ByLayer and ByBlock colors.
        /// </remarks>
        public AciColor(short index)
        {
            if (index <= 0 || index >= 256)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Accepted color index values range from 1 to 255.");
            }

            byte[] rgb = IndexRgb[(byte) index];
            this.r = rgb[0];
            this.g = rgb[1];
            this.b = rgb[2];
            this.useTrueColor = false;
            this.index = index;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Defines if the color is defined by layer.
        /// </summary>
        public bool IsByLayer
        {
            get { return this.index == 256; }
        }

        /// <summary>
        /// Defines if the color is defined by block.
        /// </summary>
        public bool IsByBlock
        {
            get { return this.index == 0; }
        }

        /// <summary>
        /// Gets the red component of the AciColor.
        /// </summary>
        public byte R
        {
            get { return this.r; }
        }

        /// <summary>
        /// Gets the green component of the AciColor.
        /// </summary>
        public byte G
        {
            get { return this.g; }
        }

        /// <summary>
        /// Gets the blue component of the AciColor.
        /// </summary>
        public byte B
        {
            get { return this.b; }
        }

        /// <summary>
        /// Get or set if the AciColor should use true color values.
        /// </summary>
        /// <remarks>
        /// By default, the constructors that use RGB values will set this boolean to true
        /// while the constants and the constructor that use a color index will set it to false.
        /// </remarks>
        public bool UseTrueColor
        {
            get { return this.useTrueColor; }
            set { this.useTrueColor = value; }
        }

        /// <summary>
        /// Gets or sets the color index.
        /// </summary>
        /// <remarks>
        /// Accepted color index values range from 1 to 255.
        /// Indexes from 1 to 255 represents a color, the index 0 and 256 are reserved for ByLayer and ByBlock colors.
        /// </remarks>
        public short Index
        {
            get { return this.index; }
            set
            {
                if (value <= 0 || value >= 256)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted color index values range from 1 to 255.");
                }

                this.index = value;
                byte[] rgb = IndexRgb[(byte) this.index];
                this.r = rgb[0];
                this.g = rgb[1];
                this.b = rgb[2];
                this.useTrueColor = false;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Obtains the approximate color index from the RGB components.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <returns>The approximate color index from the RGB components.</returns>
        public static byte RgbToAci(byte r, byte g, byte b)
        {
            int prevDist = int.MaxValue;
            byte index = 0;
            for (int i = 1; i < 256; i++)
            {
                byte[] color = IndexRgb[i];
                int red = r - color[0];
                int green = g - color[1];
                int blue = b - color[2];
                int dist = red * red + green * green + blue * blue;
                if (dist == 0) // the RGB components correspond to one of the indexed colors
                {
                    return (byte)i;
                }
                if (dist < prevDist)
                {
                    prevDist = dist;
                    index = (byte)i;
                }
            }

            return index;
        }

        /// <summary>
        /// Converts HSL (hue, saturation, lightness) value to an <see cref="AciColor">AciColor</see>.
        /// </summary>
        /// <param name="hsl">A Vector3 containing the hue, saturation, and lightness components.</param>
        /// <returns>An <see cref="Color">AciColor</see> that represents the actual HSL value.</returns>
        public static AciColor FromHsl(Vector3 hsl)
        {
            return FromHsl(hsl.X, hsl.Y, hsl.Z);
        }

        /// <summary>
        /// Converts HSL (hue, saturation, lightness) value to an <see cref="AciColor">AciColor</see>.
        /// </summary>
        /// <param name="hue">Hue (input values range from 0 to 1).</param>
        /// <param name="saturation">Saturation (input values range from 0 to 1).</param>
        /// <param name="lightness">Lightness (input values range from 0 to 1).</param>
        /// <returns>An <see cref="Color">AciColor</see> that represents the actual HSL value.</returns>
        public static AciColor FromHsl(double hue, double saturation, double lightness)
        {
            if (hue < 0 || hue > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(hue), hue, "Hue input values range from 0 to 1.");
            }
            if (saturation < 0 || saturation > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(saturation), saturation, "Saturation input values range from 0 to 1.");
            }
            if (lightness < 0 || lightness > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(lightness), lightness, "Lightness input values range from 0 to 1.");
            }

            double red = lightness;
            double green = lightness;
            double blue = lightness;
            double v = lightness <= 0.5 ? lightness * (1.0 + saturation) : lightness + saturation - lightness * saturation;
            if (v > 0)
            {
                double m = lightness + lightness - v;
                double sv = (v - m)/v;
                hue *= 6.0;
                int sextant = (int) hue;
                double fract = hue - sextant;
                double vsf = v * sv * fract;
                double mid1 = m + vsf;
                double mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        red = v;
                        green = mid1;
                        blue = m;
                        break;
                    case 1:
                        red = mid2;
                        green = v;
                        blue = m;
                        break;
                    case 2:
                        red = m;
                        green = v;
                        blue = mid1;
                        break;
                    case 3:
                        red = m;
                        green = mid2;
                        blue = v;
                        break;
                    case 4:
                        red = mid1;
                        green = m;
                        blue = v;
                        break;
                    case 5:
                        red = v;
                        green = m;
                        blue = mid2;
                        break;
                    case 6:
                        red = v;
                        green = mid1;
                        blue = m;
                        break;
                }
            }
            return new AciColor(red, green, blue);
        }

        /// <summary>
        /// Converts the RGB (red, green, blue) components of an <see cref="AciColor">AciColor</see> to HSL (hue, saturation, lightness) values.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <param name="hsl">A Vector3 containing the hue, saturation, and lightness components (output values range from 0 to 1).</param>
        public static void ToHsl(AciColor color, out Vector3 hsl)
        {
            ToHsl(color, out double h, out double s, out double l);
            hsl = new Vector3(h, s, l);
        }

        /// <summary>
        /// Converts the RGB (red, green, blue) components of an <see cref="AciColor">AciColor</see> to HSL (hue, saturation, lightness) values.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <param name="hue">Hue (output values range from 0 to 1).</param>
        /// <param name="saturation">Saturation (output values range from 0 to 1).</param>
        /// <param name="lightness">Lightness (output values range from 0 to 1).</param>
        public static void ToHsl(AciColor color, out double hue, out double saturation, out double lightness)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;

            hue = 0;
            saturation = 0;
            double v = Math.Max(red, green);
            v = Math.Max(v, blue);
            double m = Math.Min(red, green);
            m = Math.Min(m, blue);

            lightness = (m + v) / 2.0;
            if (lightness <= 0.0)
            {
                return;
            }

            double vm = v - m;
            saturation = vm;
            if (saturation > 0.0)
            {
                saturation /= (lightness <= 0.5) ? v + m : 2.0 - v - m;
            }
            else
            {
                return;
            }

            double red2 = (v - red) / vm;
            double green2 = (v - green) / vm;
            double blue2 = (v - blue) / vm;

            if (MathHelper.IsEqual(red, v))
            {
                hue = MathHelper.IsEqual(green, m) ? 5.0 + blue2 : 1.0 - green2;
            }
            else if (MathHelper.IsEqual(green, v))
            {
                hue = MathHelper.IsEqual(blue, m) ? 1.0 + red2 : 3.0 - blue2;
            }
            else
            {
                hue = MathHelper.IsEqual(red, m) ? 3.0 + green2 : 5.0 - red2;
            }

            hue /= 6.0;
        }

        /// <summary>
        /// Converts the RGB (red, green, blue) components of an <see cref="AciColor">AciColor</see> to HSL (hue, saturation, lightness) values.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <returns>
        /// A Vector3 where the three coordinates x, y, z represents the hue, saturation, and lightness components (output values range from 0 to 1).
        /// </returns>
        public static Vector3 ToHsl(AciColor color)
        {
            ToHsl(color, out double h, out double s, out double l);
            return new Vector3(h, s, l);
        }

        /// <summary>
        /// Converts the AciColor to a <see cref="Color">color</see>.
        /// </summary>
        /// <returns>A <see cref="Color">System.Drawing.Color</see> that represents the actual AciColor.</returns>
        /// <remarks>
        /// A default color white will be used for ByLayer and ByBlock colors.
        /// </remarks>
        public Color ToColor()
        {
            if (this.index < 1 || this.index > 255) //default color definition for ByLayer and ByBlock colors
            {
                return Color.White;
            }
            return Color.FromArgb(this.r, this.g, this.b);
        }

        /// <summary>
        /// Converts a <see cref="Color">color</see> to an <see cref="Color">AciColor</see>.
        /// </summary>
        /// <param name="color">A <see cref="Color">color</see>.</param>
        public void FromColor(Color color)
        {
            this.r = color.R;
            this.g = color.G;
            this.b = color.B;
            this.useTrueColor = true;
            this.index = RgbToAci(this.r, this.g, this.b);
        }

        /// <summary>
        /// Gets the <see cref="AciColor">color</see> from an index.
        /// </summary>
        /// <param name="index">A CAD indexed AciColor index.</param>
        /// <returns>A <see cref="AciColor">color</see>.</returns>
        /// <remarks>
        /// Accepted index values range from 0 to 256. An index 0 represents a ByBlock color and an index 256 is a ByLayer color;
        /// any other value will return one of the 255 indexed AciColors.
        /// </remarks>
        public static AciColor FromCadIndex(short index)
        {
            if (index < 0 || index > 256)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Accepted CAD indexed AciColor values range from 0 to 256.");
            }

            if (index == 0)
            {
                return ByBlock;
            }

            if (index == 256)
            {
                return ByLayer;
            }

            return new AciColor(index);
        }

        /// <summary>
        /// Gets the <see cref="AciColor">color</see> from a 24-bit color value.
        /// </summary>
        /// <param name="value">A 32-bit color value.</param>
        /// <returns>A <see cref="AciColor">color</see>.</returns>
        public static AciColor FromTrueColor(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return new AciColor(bytes[2], bytes[1], bytes[0]);
        }

        /// <summary>
        /// Gets the 32-bit color value from an AciColor.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <returns>A 32-bit color value.</returns>
        public static int ToTrueColor(AciColor color)
        {
            if (color == null)
            {
                throw new ArgumentNullException(nameof(color));
            }

            // AutoCad weirdness at its best
            // the forth byte seems to have no use,
            // when AutoCad saves a layer color as a true color this fourth byte is always 0,
            // when the layer color is read it seems that it doesn't care about the value of this fourth byte 
            // but if the fourth byte is not set as 194 the layer state color will be shown as an index color
            return BitConverter.ToInt32(new byte[] { color.B, color.G, color.R, 194 }, 0);
        }

        #endregion

        #region overrides

        /// <summary>
        /// Converts the value of this instance to its equivalent string representation.
        /// </summary>
        /// <returns>The string representation.</returns>
        public override string ToString()
        {
            if (this.index == 0)
            {
                return "ByBlock";
            }

            if (this.index == 256)
            {
                return "ByLayer";
            }

            if (this.useTrueColor)
            {
                return String.Format("{0}{3}{1}{3}{2}", this.r, this.g, this.b, Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);
            }

            return this.index.ToString(CultureInfo.CurrentCulture);
        }

        #endregion

        #region implements ICloneable

        /// <summary>
        /// Creates a new color that is a copy of the current instance.
        /// </summary>
        /// <returns>A new color that is a copy of this instance.</returns>
        public object Clone()
        {
            AciColor color = new AciColor
            {
                r = this.r,
                g = this.g,
                b = this.b,
                useTrueColor = this.useTrueColor,
                index = this.index
            };

            return color;
        }

        #endregion

        #region implements IEquatable

        /// <summary>
        /// Check if the components of two colors are equal.
        /// </summary>
        /// <param name="other">Another color to compare to.</param>
        /// <returns>True if the three components are equal or false in any other case.</returns>
        public bool Equals(AciColor other)
        {
            if (other == null)
            {
                return false;
            }

            return (other.r == this.r) && (other.g == this.g) && (other.b == this.b);
        }

        #endregion
    }
}