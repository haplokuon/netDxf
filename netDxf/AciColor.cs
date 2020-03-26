#region netDxf library, Copyright (C) 2009-2018 Daniel Carvajal (haplokuon@gmail.com)

//                        netDxf library
// Copyright (C) 2009-2018 Daniel Carvajal (haplokuon@gmail.com)
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
using System.Globalization;
using System.Threading;

namespace netDxf
{
    /// <summary>
    /// Represents an ACI color (AutoCAD Color Index) that also supports true color.
    /// </summary>
    public partial class AciColor :
        ICloneable,
        IEquatable<AciColor>
    {

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
            get { return new AciColor {index = 256}; }
        }

        /// <summary>
        /// Gets the ByBlock color.
        /// </summary>
        public static AciColor ByBlock
        {
            get { return new AciColor {index = 0}; }
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
        /// A dictionary that contains the indexed colors, the key represents the color index and the value the RGB components of the color.
        /// </summary>
        public static IReadOnlyDictionary<byte, byte[]> IndexRgb
        {
            get { return new AciColorDictionary(); }
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
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.useTrueColor = true;
            this.index = LookUpAci(this.r, this.g, this.b);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(float r, float g, float b)
            : this((byte) (r*255), (byte) (g*255), (byte) (b*255))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(double r, double g, double b)
            : this((byte) (r*255), (byte) (g*255), (byte) (b*255))
        {
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
                throw new ArgumentOutOfRangeException(nameof(index), index, "Accepted color index values range from 1 to 255.");

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
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Accepted color index values range from 1 to 255.");

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
        /// <param name="saturation">Saturation percentage (input values range from 0 to 1).</param>
        /// <param name="lightness">Lightness percentage (input values range from 0 to 1).</param>
        /// <returns>An <see cref="Color">AciColor</see> that represents the actual HSL value.</returns>
        public static AciColor FromHsl(double hue, double saturation, double lightness)
        {
            double red = lightness;
            double green = lightness;
            double blue = lightness;
            double v = lightness <= 0.5 ? lightness*(1.0 + saturation) : lightness + saturation - lightness*saturation;
            if (v > 0)
            {
                double m = lightness + lightness - v;
                double sv = (v - m)/v;
                hue *= 6.0;
                int sextant = (int) hue;
                double fract = hue - sextant;
                double vsf = v*sv*fract;
                double mid1 = m + vsf;
                double mid2 = v - vsf;
                switch (sextant)
                {
                    case 0 | 6:
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
                }
            }
            return new AciColor(red, green, blue);
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
                throw new ArgumentNullException(nameof(color));

            double red = color.R/255.0;
            double green = color.G/255.0;
            double blue = color.B/255.0;

            hue = 0;
            saturation = 0;
            double v = Math.Max(red, green);
            v = Math.Max(v, blue);
            double m = Math.Min(red, green);
            m = Math.Min(m, blue);

            lightness = (m + v)/2.0;
            if (lightness <= 0.0)
                return;

            double vm = v - m;
            saturation = vm;
            if (saturation > 0.0)
                saturation /= (lightness <= 0.5) ? v + m : 2.0 - v - m;
            else
                return;

            double red2 = (v - red)/vm;
            double green2 = (v - green)/vm;
            double blue2 = (v - blue)/vm;

            if (MathHelper.IsEqual(red, v))
                hue = MathHelper.IsEqual(green, m) ? 5.0 + blue2 : 1.0 - green2;
            else if (MathHelper.IsEqual(green, v))
                hue = MathHelper.IsEqual(blue, m) ? 1.0 + red2 : 3.0 - blue2;
            else
                hue = MathHelper.IsEqual(red, m) ? 3.0 + green2 : 5.0 - red2;

            hue /= 6.0;
        }

        /// <summary>
        /// Converts the RGB (red, green, blue) components of an <see cref="AciColor">AciColor</see> to HSL (hue, saturation, lightness) values.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <returns>A Vector3 where the three coordinates x, y, z represents the hue, saturation, and lightness components (output values range from 0 to 1).</returns>
        public static Vector3 ToHsl(AciColor color)
        {
            double h, s, l;
            ToHsl(color, out h, out s, out l);
            return new Vector3(h, s, l);
        }

        /// <summary>
        /// Converts the AciColor to a <see cref="Color">color</see>.
        /// </summary>
        /// <returns>A <see cref="Color">System.Drawing.Color</see> that represents the actual AciColor.</returns>
        /// <remarks>A default color white will be used for ByLayer and ByBlock colors.</remarks>
        public Color ToColor()
        {
            if (this.index < 1 || this.index > 255) //default color definition for ByLayer and ByBlock colors
                return Color.White;
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
            this.index = LookUpAci(this.r, this.g, this.b);
        }

        /// <summary>
        /// Gets the 24-bit color value from an AciColor.
        /// </summary>
        /// <param name="color">A <see cref="AciColor">color</see>.</param>
        /// <returns>A 24-bit color value (BGR order).</returns>
        public static int ToTrueColor(AciColor color)
        {
            if (color == null)
                throw new ArgumentNullException(nameof(color));

            return BitConverter.ToInt32(new byte[] {color.B, color.G, color.R, 0}, 0);
        }

        /// <summary>
        /// Gets the <see cref="AciColor">color</see> from a 24-bit color value.
        /// </summary>
        /// <param name="value">A 24-bit color value (BGR order).</param>
        /// <returns>A <see cref="AciColor">color</see>.</returns>
        public static AciColor FromTrueColor(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return new AciColor(bytes[2], bytes[1], bytes[0]);
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
                throw new ArgumentOutOfRangeException(nameof(index), index, "Accepted CAD indexed AciColor values range from 0 to 256.");

            if (index == 0)
                return ByBlock;
            if (index == 256)
                return ByLayer;

            return new AciColor(index);
        }

        /// <summary>
        /// Searches RGB value in Autocad color palette (ACI).
        /// </summary>
        /// <param name="colorIndex">Index of color in ACI palette.</param>
        /// <returns>
        /// RGB value: integer with blue in less significant byte,
        /// green the second byte, and Red the third byte.
        /// </returns>
        public static int LookUpRgb(byte colorIndex)
        {
            // Autocad returns 0xC5000000 (kForeground) for both - ByLayer and for white/black
            const int FOREGROUND = unchecked((int)0xC5000000);

            // legacy palette
            switch (colorIndex)
            {
                case 0: return FOREGROUND; // 000-BYBLOCK
                case 1: return 0x00FF0000; // 001-red
                case 2: return 0x00FFFF00; // 002-yellow
                case 3: return 0x0000FF00; // 003-green
                case 4: return 0x0000FFFF; // 004-cyan
                case 5: return 0x000000FF; // 005-blue
                case 6: return 0x00FF00FF; // 006-magenta
                case 7: return FOREGROUND | 0x00FFFFFF; // 007-white
                case 8: return 0x00808080; // 008-grey
                case 9: return 0x00C0C0C0; // 009-lt grey
            }

            if (colorIndex >= 250)
            {
                // grayscale, step HSV value by 16 from 20 to 100% 
                int c = 255 * (20 + ((colorIndex - 250) << 4)) / 100;
                return (c << 16) | (c << 8) | c;
            }

            // perform HSV -> RGB conversion. 

            /*  The chromatic colors.  The  hue  resulting  from  an
                AutoCAD color 10-249 will be determined by its first
                two digits, and the saturation and  value  from  the
                last digit, as follows: 
            */

            // first two digits - hue degree in range 0-24
            int h = (colorIndex - 10) / 10;

            // value in 100, 80, 60, 50 or 30% of byte.MaxValue
            // for each two rows of ACI palette
            int v = 0;

            // last digit of ACI - HCV value component
            switch ((colorIndex % 10) >> 1) {
                case 0: v = 255;            break; // 100%
                case 1: v = 255 * 80 / 100; break; // 80%
                case 2: v = 255 * 60 / 100; break; // 60%
                case 3: v = 255 * 50 / 100; break; // 50%
                case 4: v = 255 * 30 / 100; break; // 30%
            }

            int f = (h % 4) << 6; // fraction
            
            // saturation: 1 - for even indexes, 1/2 - for odd ones
            // we will use left shif bitwise division
            int s = colorIndex & 1;

            int p = (v * (0x100 - (0x100       >> s))) >> 8;
            int q = (v * (0x100 - (         f  >> s))) >> 8;
            int t = (v * (0x100 - ((0x100 - f) >> s))) >> 8;

            switch (h >> 2)
            {
                case 0: return (v << 16) | (t << 8) | p;
                case 1: return (q << 16) | (v << 8) | p;
                case 2: return (p << 16) | (v << 8) | t;
                case 3: return (p << 16) | (q << 8) | v;
                case 4: return (t << 16) | (p << 8) | v;
                case 5: return (v << 16) | (p << 8) | q;
                default: return -1; // can't happen, just make compiler happy
            }

        }

        /// <summary>
        /// Obtains the approximate color index from the RGB components.
        /// </summary>
        /// <param name="r">Red component.</param>
        /// <param name="g">Green component.</param>
        /// <param name="b">Blue component.</param>
        /// <returns>The approximate color index from the RGB components</returns>
        /// <remarks>This conversion will never be accurate.</remarks>
        public static byte LookUpAci(byte r, byte g, byte b)
        {
            //now get color using shortest dist calc and see if it matches
            int minDist = int.MaxValue;
            byte match = 0; // this will end up with our answer

            for (int i = 1; i <= byte.MaxValue; ++i)
            {

                int rgb = LookUpRgb((byte)i);
                int rDiff = r - (byte)(rgb >> 16);
                int gDiff = g - (byte)(rgb >> 8);
                int bDiff = b - (byte)rgb;

                int dist = rDiff * rDiff + gDiff * gDiff + bDiff * bDiff;

                if (dist < minDist)
                {
                    match = (byte)i;
                    minDist = dist;

                    // Exact match, no reason to lookup more
                    if (minDist == 0)
                        return match;
                }
            }

            return match;
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
                return "ByBlock";
            if (this.index == 256)
                return "ByLayer";
            if (this.useTrueColor)
                return string.Format("{0}{3}{1}{3}{2}", this.r, this.g, this.b, Thread.CurrentThread.CurrentCulture.TextInfo.ListSeparator);

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
                return false;

            return (other.r == this.r) && (other.g == this.g) && (other.b == this.b);
        }

        #endregion
    }
}