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
using System.Threading;

namespace netDxf
{
    /// <summary>
    /// Represents an ACI color (Autocad Color Index) that also supports true color.
    /// </summary>
    public class AciColor
        : ICloneable
    {
        #region private fields

        private static readonly Dictionary<byte, byte[]> aciColors = IndexRgb();
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
        /// Defines a default dark grey color.
        /// </summary>
        public static AciColor DarkGrey
        {
            get { return new AciColor(8); }
        }

        /// <summary>
        /// Defines a default light grey color.
        /// </summary>
        public static AciColor LightGrey
        {
            get { return new AciColor(9); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class with black/white color index 7.
        /// </summary>
        public AciColor()
            :this(7)
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
            this.index = RGBtoACI(this.r, this.g, this.b);
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        /// <remarks>By default the UseTrueColor will be set to true.</remarks>
        public AciColor(float r, float g, float b)
            : this((byte)(r * 255), (byte)(g * 255), (byte)(b * 255))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <c>AciColor</c> class.
        /// </summary>
        ///<param name="color">A <see cref="Color">color</see>.</param>
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
        /// By default the UseTrueColor will be set to false.
        /// Accepted color index values range from 1 to 255.
        /// Indexes from 1 to 255 represents a color, the index 0 and 256 are reserved they define bylayer and byblock colors.
        /// </remarks>
        public AciColor(short index)
        {
            if (index <= 0 || index >= 256)
                throw new ArgumentOutOfRangeException("index", index, "Accepted color index values range from 1 to 255.");

            byte[] rgb = aciColors[(byte)index];
            this.r = rgb[0];
            this.g = rgb[1];
            this.b = rgb[2];
            this.useTrueColor = false;
            this.index = index;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets the red component of the AciColor.
        /// </summary>
        public byte R
        {
            get { return r; }
        }

        /// <summary>
        /// Gets the green component of the AciColor.
        /// </summary>
        public byte G
        {
            get { return g; }
        }

        /// <summary>
        /// Gets the blue component of the AciColor.
        /// </summary>
        public byte B
        {
            get { return b; }
        }

        /// <summary>
        /// Get or set if the AciColor should use true color values.
        /// </summary>
        /// <remarks>
        /// By default the constructors that use r, g, b values will set this boolean to true
        /// while the constants and constructor that use a short will set it to false.
        /// </remarks>
        public bool UseTrueColor
        {
            get { return useTrueColor; }
            set { useTrueColor = value; }
        }

        /// <summary>
        /// Gets or sets the color index.
        /// </summary>
        /// <remarks>
        /// Accepted color index values range from 1 to 255.
        /// Indexes from 1 to 255 represents a color, the index 0 and 256 are reserved they define bylayer and byblock colors.
        /// </remarks>
        public short Index
        {
            get { return this.index; }
            set
            {
                if (value <= 0 || value >= 256)
                    throw new ArgumentOutOfRangeException("value", value, "Accepted color index values range from 1 to 255.");

                byte[] rgb = aciColors[(byte)this.index];
                this.r = rgb[0];
                this.g = rgb[1];
                this.b = rgb[2];
                this.useTrueColor = false;
                this.index = value;
            }
        }

        #endregion

        #region public methods

        /// <summary>
        /// Converts the AciColor to a <see cref="Color">color</see>.
        /// </summary>
        /// <returns>A default color white will be used for byblock and bylayer colors.</returns>
        public Color ToColor()
        {
            if (this.index < 1 || this.index > 255) //default color definition for byblock and bylayer colors
                return Color.White;
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Converts the Color to a <see cref="Color">AciColor</see>.
        /// </summary>
        public void FromColor(Color color)
        {
            this.r = color.R;
            this.g = color.G;
            this.b = color.B;
            this.useTrueColor = true;
            this.index = RGBtoACI(this.r, this.g, this.b);
        }

        /// <summary>
        /// Gets the complete list of indexed colors.
        /// </summary>
        /// <returns>A dictionary that contains the indexed colors, the key represents the color index and the value the rgb components of the color.</returns>
        public static Dictionary<byte, byte[]> IndexRgb()
        {
            return new Dictionary<byte, byte[]>
                            {
                                {1, new byte[] {255, 0, 0}},
                                {2, new byte[] {255, 255, 0}},
                                {3, new byte[] {0, 255, 0}},
                                {4, new byte[] {0, 255, 255}},
                                {5, new byte[] {0, 0, 255}},
                                {6, new byte[] {255, 0, 255}},
                                {7, new byte[] {255, 255, 255}},
                                {8, new byte[] {128, 128, 128}},
                                {9, new byte[] {192, 192, 192}},
                                {10, new byte[] {255, 0, 0}},
                                {11, new byte[] {255, 127, 127}},
                                {12, new byte[] {165, 0, 0}},
                                {13, new byte[] {165, 82, 82}},
                                {14, new byte[] {127, 0, 0}},
                                {15, new byte[] {127, 63, 63}},
                                {16, new byte[] {76, 0, 0}},
                                {17, new byte[] {76, 38, 38}},
                                {18, new byte[] {38, 0, 0}},
                                {19, new byte[] {38, 19, 19}},
                                {20, new byte[] {255, 63, 0}},
                                {21, new byte[] {255, 159, 127}},
                                {22, new byte[] {165, 41, 0}},
                                {23, new byte[] {165, 103, 82}},
                                {24, new byte[] {127, 31, 0}},
                                {25, new byte[] {127, 79, 63}},
                                {26, new byte[] {76, 19, 0}},
                                {27, new byte[] {76, 47, 38}},
                                {28, new byte[] {38, 9, 0}},
                                {29, new byte[] {38, 23, 19}},
                                {30, new byte[] {255, 127, 0}},
                                {31, new byte[] {255, 191, 127}},
                                {32, new byte[] {165, 82, 0}},
                                {33, new byte[] {165, 124, 82}},
                                {34, new byte[] {127, 63, 0}},
                                {35, new byte[] {127, 95, 63}},
                                {36, new byte[] {76, 38, 0}},
                                {37, new byte[] {76, 57, 38}},
                                {38, new byte[] {38, 19, 0}},
                                {39, new byte[] {38, 28, 19}},
                                {40, new byte[] {255, 191, 0}},
                                {41, new byte[] {255, 223, 127}},
                                {42, new byte[] {165, 124, 0}},
                                {43, new byte[] {165, 145, 82}},
                                {44, new byte[] {127, 95, 0}},
                                {45, new byte[] {127, 111, 63}},
                                {46, new byte[] {76, 57, 0}},
                                {47, new byte[] {76, 66, 38}},
                                {48, new byte[] {38, 28, 0}},
                                {49, new byte[] {38, 33, 19}},
                                {50, new byte[] {255, 255, 0}},
                                {51, new byte[] {255, 255, 127}},
                                {52, new byte[] {165, 165, 0}},
                                {53, new byte[] {165, 165, 82}},
                                {54, new byte[] {127, 127, 0}},
                                {55, new byte[] {127, 127, 63}},
                                {56, new byte[] {76, 76, 0}},
                                {57, new byte[] {76, 76, 38}},
                                {58, new byte[] {38, 38, 0}},
                                {59, new byte[] {38, 38, 19}},
                                {60, new byte[] {191, 255, 0}},
                                {61, new byte[] {223, 255, 127}},
                                {62, new byte[] {124, 165, 0}},
                                {63, new byte[] {145, 165, 82}},
                                {64, new byte[] {95, 127, 0}},
                                {65, new byte[] {111, 127, 63}},
                                {66, new byte[] {57, 76, 0}},
                                {67, new byte[] {66, 76, 38}},
                                {68, new byte[] {28, 38, 0}},
                                {69, new byte[] {33, 38, 19}},
                                {70, new byte[] {127, 255, 0}},
                                {71, new byte[] {191, 255, 127}},
                                {72, new byte[] {82, 165, 0}},
                                {73, new byte[] {124, 165, 82}},
                                {74, new byte[] {63, 127, 0}},
                                {75, new byte[] {95, 127, 63}},
                                {76, new byte[] {38, 76, 0}},
                                {77, new byte[] {57, 76, 38}},
                                {78, new byte[] {19, 38, 0}},
                                {79, new byte[] {28, 38, 19}},
                                {80, new byte[] {63, 255, 0}},
                                {81, new byte[] {159, 255, 127}},
                                {82, new byte[] {41, 165, 0}},
                                {83, new byte[] {103, 165, 82}},
                                {84, new byte[] {31, 127, 0}},
                                {85, new byte[] {79, 127, 63}},
                                {86, new byte[] {19, 76, 0}},
                                {87, new byte[] {47, 76, 38}},
                                {88, new byte[] {9, 38, 0}},
                                {89, new byte[] {23, 38, 19}},
                                {90, new byte[] {0, 255, 0}},
                                {91, new byte[] {127, 255, 127}},
                                {92, new byte[] {0, 165, 0}},
                                {93, new byte[] {82, 165, 82}},
                                {94, new byte[] {0, 127, 0}},
                                {95, new byte[] {63, 127, 63}},
                                {96, new byte[] {0, 76, 0}},
                                {97, new byte[] {38, 76, 38}},
                                {98, new byte[] {0, 38, 0}},
                                {99, new byte[] {19, 38, 19}},
                                {100, new byte[] {0, 255, 63}},
                                {101, new byte[] {127, 255, 159}},
                                {102, new byte[] {0, 165, 41}},
                                {103, new byte[] {82, 165, 103}},
                                {104, new byte[] {0, 127, 31}},
                                {105, new byte[] {63, 127, 79}},
                                {106, new byte[] {0, 76, 19}},
                                {107, new byte[] {38, 76, 47}},
                                {108, new byte[] {0, 38, 9}},
                                {109, new byte[] {19, 38, 23}},
                                {110, new byte[] {0, 255, 127}},
                                {111, new byte[] {127, 255, 191}},
                                {112, new byte[] {0, 165, 82}},
                                {113, new byte[] {82, 165, 124}},
                                {114, new byte[] {0, 127, 63}},
                                {115, new byte[] {63, 127, 95}},
                                {116, new byte[] {0, 76, 38}},
                                {117, new byte[] {38, 76, 57}},
                                {118, new byte[] {0, 38, 19}},
                                {119, new byte[] {19, 38, 28}},
                                {120, new byte[] {0, 255, 191}},
                                {121, new byte[] {127, 255, 223}},
                                {122, new byte[] {0, 165, 124}},
                                {123, new byte[] {82, 165, 145}},
                                {124, new byte[] {0, 127, 95}},
                                {125, new byte[] {63, 127, 111}},
                                {126, new byte[] {0, 76, 57}},
                                {127, new byte[] {38, 76, 66}},
                                {128, new byte[] {0, 38, 28}},
                                {129, new byte[] {19, 38, 33}},
                                {130, new byte[] {0, 255, 255}},
                                {131, new byte[] {127, 255, 255}},
                                {132, new byte[] {0, 165, 165}},
                                {133, new byte[] {82, 165, 165}},
                                {134, new byte[] {0, 127, 127}},
                                {135, new byte[] {63, 127, 127}},
                                {136, new byte[] {0, 76, 76}},
                                {137, new byte[] {38, 76, 76}},
                                {138, new byte[] {0, 38, 38}},
                                {139, new byte[] {19, 38, 38}},
                                {140, new byte[] {0, 191, 255}},
                                {141, new byte[] {127, 223, 255}},
                                {142, new byte[] {0, 124, 165}},
                                {143, new byte[] {82, 145, 165}},
                                {144, new byte[] {0, 95, 127}},
                                {145, new byte[] {63, 111, 127}},
                                {146, new byte[] {0, 57, 76}},
                                {147, new byte[] {38, 66, 76}},
                                {148, new byte[] {0, 28, 38}},
                                {149, new byte[] {19, 33, 38}},
                                {150, new byte[] {0, 127, 255}},
                                {151, new byte[] {127, 191, 255}},
                                {152, new byte[] {0, 82, 165}},
                                {153, new byte[] {82, 124, 165}},
                                {154, new byte[] {0, 63, 127}},
                                {155, new byte[] {63, 95, 127}},
                                {156, new byte[] {0, 38, 76}},
                                {157, new byte[] {38, 57, 76}},
                                {158, new byte[] {0, 19, 38}},
                                {159, new byte[] {19, 28, 38}},
                                {160, new byte[] {0, 63, 255}},
                                {161, new byte[] {127, 159, 255}},
                                {162, new byte[] {0, 41, 165}},
                                {163, new byte[] {82, 103, 165}},
                                {164, new byte[] {0, 31, 127}},
                                {165, new byte[] {63, 79, 127}},
                                {166, new byte[] {0, 19, 76}},
                                {167, new byte[] {38, 47, 76}},
                                {168, new byte[] {0, 9, 38}},
                                {169, new byte[] {19, 23, 38}},
                                {170, new byte[] {0, 0, 255}},
                                {171, new byte[] {127, 127, 255}},
                                {172, new byte[] {0, 0, 165}},
                                {173, new byte[] {82, 82, 165}},
                                {174, new byte[] {0, 0, 127}},
                                {175, new byte[] {63, 63, 127}},
                                {176, new byte[] {0, 0, 76}},
                                {177, new byte[] {38, 38, 76}},
                                {178, new byte[] {0, 0, 38}},
                                {179, new byte[] {19, 19, 38}},
                                {180, new byte[] {63, 0, 255}},
                                {181, new byte[] {159, 127, 255}},
                                {182, new byte[] {41, 0, 165}},
                                {183, new byte[] {103, 82, 165}},
                                {184, new byte[] {31, 0, 127}},
                                {185, new byte[] {79, 63, 127}},
                                {186, new byte[] {19, 0, 76}},
                                {187, new byte[] {47, 38, 76}},
                                {188, new byte[] {9, 0, 38}},
                                {189, new byte[] {23, 19, 38}},
                                {190, new byte[] {127, 0, 255}},
                                {191, new byte[] {191, 127, 255}},
                                {192, new byte[] {82, 0, 165}},
                                {193, new byte[] {124, 82, 165}},
                                {194, new byte[] {63, 0, 127}},
                                {195, new byte[] {95, 63, 127}},
                                {196, new byte[] {38, 0, 76}},
                                {197, new byte[] {57, 38, 76}},
                                {198, new byte[] {19, 0, 38}},
                                {199, new byte[] {28, 19, 38}},
                                {200, new byte[] {191, 0, 255}},
                                {201, new byte[] {223, 127, 255}},
                                {202, new byte[] {124, 0, 165}},
                                {203, new byte[] {145, 82, 165}},
                                {204, new byte[] {95, 0, 127}},
                                {205, new byte[] {111, 63, 127}},
                                {206, new byte[] {57, 0, 76}},
                                {207, new byte[] {66, 38, 76}},
                                {208, new byte[] {28, 0, 38}},
                                {209, new byte[] {33, 19, 38}},
                                {210, new byte[] {255, 0, 255}},
                                {211, new byte[] {255, 127, 255}},
                                {212, new byte[] {165, 0, 165}},
                                {213, new byte[] {165, 82, 165}},
                                {214, new byte[] {127, 0, 127}},
                                {215, new byte[] {127, 63, 127}},
                                {216, new byte[] {76, 0, 76}},
                                {217, new byte[] {76, 38, 76}},
                                {218, new byte[] {38, 0, 38}},
                                {219, new byte[] {38, 19, 38}},
                                {220, new byte[] {255, 0, 191}},
                                {221, new byte[] {255, 127, 223}},
                                {222, new byte[] {165, 0, 124}},
                                {223, new byte[] {165, 82, 145}},
                                {224, new byte[] {127, 0, 95}},
                                {225, new byte[] {127, 63, 111}},
                                {226, new byte[] {76, 0, 57}},
                                {227, new byte[] {76, 38, 66}},
                                {228, new byte[] {38, 0, 28}},
                                {229, new byte[] {38, 19, 33}},
                                {230, new byte[] {255, 0, 127}},
                                {231, new byte[] {255, 127, 191}},
                                {232, new byte[] {165, 0, 82}},
                                {233, new byte[] {165, 82, 124}},
                                {234, new byte[] {127, 0, 63}},
                                {235, new byte[] {127, 63, 95}},
                                {236, new byte[] {76, 0, 38}},
                                {237, new byte[] {76, 38, 57}},
                                {238, new byte[] {38, 0, 19}},
                                {239, new byte[] {38, 19, 28}},
                                {240, new byte[] {255, 0, 63}},
                                {241, new byte[] {255, 127, 159}},
                                {242, new byte[] {165, 0, 41}},
                                {243, new byte[] {165, 82, 103}},
                                {244, new byte[] {127, 0, 31}},
                                {245, new byte[] {127, 63, 79}},
                                {246, new byte[] {76, 0, 19}},
                                {247, new byte[] {76, 38, 47}},
                                {248, new byte[] {38, 0, 9}},
                                {249, new byte[] {38, 19, 23}},
                                {250, new byte[] {0, 0, 0}},
                                {251, new byte[] {51, 51, 51}},
                                {252, new byte[] {102, 102, 102}},
                                {253, new byte[] {153, 153, 153}},
                                {254, new byte[] {204, 204, 204}},
                                {255, new byte[] {255, 255, 255}}
                            };

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
            return this.index.ToString(Thread.CurrentThread.CurrentCulture);
        }

        #endregion

        #region implements ICloneable

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
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

        #region internal methods

        internal static int ToTrueColor(byte r, byte g, byte b)
        {
            return b + (g * 256) + (r * 65536);
        }

        internal static AciColor FromTrueColor(int value)
        {
            int index = value;
            int r = index >> 16;
            index -= r * 65536;
            int g = index >> 8;
            index -= g * 256;
            int b = index;

            return new AciColor((byte) r, (byte) g, (byte) b);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Obtains a color index from the rgb components.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        /// <remarks>This conversion will never be accurate. It uses a simple distance method using RGB space.</remarks>
        private static byte RGBtoACI(byte r, byte g, byte b)
        {
            int prevDist = int.MaxValue;
            byte index = 0;
            foreach (byte key in aciColors.Keys)
            {
                byte[] color = aciColors[key];
                int dist = Math.Abs((r - color[0]) * (r - color[0]) + (g - color[1]) * (g - color[1]) + (b - color[2]) * (b - color[2]));
                if (dist < prevDist)
                {
                    prevDist = dist;
                    index = key;
                }
            }

            return index;
        }

        #endregion
    }
}