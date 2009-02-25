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
using System.Drawing;

namespace netDxf
{
    /// <summary>
    /// Represents a color.
    /// </summary>
    public class AciColor
    {
        #region private fields

        private static readonly Dictionary<byte, byte[]> aciColors = AciColors();
        private short index;

        #endregion

        #region constants

        /// <summary>
        /// Defines a color by layer.
        /// </summary>
        public static AciColor Bylayer
        {
            get { return new AciColor(256); }
        }

        /// <summary>
        /// Defines a color by layer.
        /// </summary>
        public static AciColor ByBlock
        {
            get { return new AciColor(0); }
        }

        /// <summary>
        /// Defines a default white/black color.
        /// </summary>
        public static AciColor Default
        {
            get { return new AciColor(7); }
        }

        #endregion

        #region constructors

        /// <summary>
        /// Initializes a color.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        /// <remarks>Since only 256 colors are posible the conversion won't be exact.</remarks>
        public AciColor(byte r, byte g, byte b)
        {
            this.index = RGBtoACI(r, g, b);
        }

        /// <summary>
        /// Initializes a color.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        /// <remarks>Since only 256 colors are posible the conversion won't be exact.</remarks>
        public AciColor(float r, float g, float b)
        {
            this.index = RGBtoACI((byte) (r*255), (byte) (g*255), (byte) (b*255));
        }

        /// <summary>
        /// Initializes a color.
        /// </summary>
        ///<param name="color">System.Drawing.Color.</param>
        public AciColor(Color color)
        {
            this.index = RGBtoACI(color.R, color.G, color.B);
        }

        /// <summary>
        /// Initializes a color.
        /// </summary>
        /// <param name="index">Color index.</param>
        /// <remarks>
        /// Accepted color index values range from 0 to 256.
        /// Indexes from 1 to 255 represents a color, the index 256 is reserved to define a color bylayer and the index 0 represents byblock.
        /// </remarks>
        public AciColor(short index)
        {
            if (index < 0 || index > 256)
            {
                throw (new ArgumentException("index"));
            }
            this.index = index;
        }

        #endregion

        #region public properties

        /// <summary>
        /// Gets or sets the color index.
        /// </summary>
        /// <remarks>
        /// Accepted color index values range from 0 to 256.
        /// Indexes from 1 to 255 represents a color, the index 256 is reserved to define a color bylayer and the index 0 represents byblock.
        /// </remarks>
        public short Index
        {
            get { return this.index; }
            set
            {
                if (value < 0 || value > 256)
                {
                    throw (new ArgumentException("value"));
                }
                this.index = value;
            }
        }

        #endregion

        #region overrides

        public override string ToString()
        {
            return this.index.ToString();
        }

        #endregion

        #region public methods

        /// <summary>
        /// Converts the AciColor to System.Drawing.Color.
        /// </summary>
        /// <returns>A default color white will be used for byblock and bylayer colors.</returns>
        public Color ToColor()
        {
            if (this.index < 1 || this.index > 255) //default color definition for byblock and bylayer colors
                return Color.White;

            byte[] rgb = aciColors[(byte) this.index];
            return Color.FromArgb(rgb[0], rgb[1], rgb[2]);
        }
        #endregion

        #region private methods

        /// <summary>
        /// Obtains a color index from the rgb components.
        /// </summary>
        ///<param name="r">Red component.</param>
        ///<param name="g">Green component.</param>
        ///<param name="b">Blue component.</param>
        private static byte RGBtoACI(byte r, byte g, byte b)
        {
            int prevDist = int.MaxValue;
            byte index = 0;
            foreach (byte key in aciColors.Keys)
            {
                byte[] color = aciColors[key];
                int dist = Math.Abs((r - color[0])*(r - color[0]) + (g - color[1])*(g - color[1]) + (b - color[2])*(b - color[2]));
                if (dist < prevDist)
                {
                    prevDist = dist;
                    index = key;
                }
            }

            return index;
        }

        private static Dictionary<byte, byte[]> AciColors()
        {
            var lista = new Dictionary<byte, byte[]>
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
                                {12, new byte[] {204, 0, 0}},
                                {13, new byte[] {204, 102, 102}},
                                {14, new byte[] {153, 0, 0}},
                                {15, new byte[] {153, 76, 76}},
                                {16, new byte[] {127, 0, 0}},
                                {17, new byte[] {127, 63, 63}},
                                {18, new byte[] {76, 0, 0}},
                                {19, new byte[] {76, 38, 38}},
                                {20, new byte[] {255, 63, 0}},
                                {21, new byte[] {255, 159, 127}},
                                {22, new byte[] {204, 51, 0}},
                                {23, new byte[] {204, 127, 102}},
                                {24, new byte[] {153, 38, 0}},
                                {25, new byte[] {153, 95, 76}},
                                {26, new byte[] {127, 31, 0}},
                                {27, new byte[] {127, 79, 63}},
                                {28, new byte[] {76, 19, 0}},
                                {29, new byte[] {76, 47, 38}},
                                {30, new byte[] {255, 127, 0}},
                                {31, new byte[] {255, 191, 127}},
                                {32, new byte[] {204, 102, 0}},
                                {33, new byte[] {204, 153, 102}},
                                {34, new byte[] {153, 76, 0}},
                                {35, new byte[] {153, 114, 76}},
                                {36, new byte[] {127, 63, 0}},
                                {37, new byte[] {127, 95, 63}},
                                {38, new byte[] {76, 38, 0}},
                                {39, new byte[] {76, 57, 38}},
                                {40, new byte[] {255, 191, 0}},
                                {41, new byte[] {255, 223, 127}},
                                {42, new byte[] {204, 153, 0}},
                                {43, new byte[] {204, 178, 102}},
                                {44, new byte[] {153, 114, 0}},
                                {45, new byte[] {153, 133, 76}},
                                {46, new byte[] {127, 95, 0}},
                                {47, new byte[] {127, 111, 63}},
                                {48, new byte[] {76, 57, 0}},
                                {49, new byte[] {76, 66, 38}},
                                {50, new byte[] {255, 255, 0}},
                                {51, new byte[] {255, 255, 127}},
                                {52, new byte[] {204, 204, 0}},
                                {53, new byte[] {204, 204, 102}},
                                {54, new byte[] {153, 153, 0}},
                                {55, new byte[] {153, 153, 76}},
                                {56, new byte[] {127, 127, 0}},
                                {57, new byte[] {127, 127, 63}},
                                {58, new byte[] {76, 76, 0}},
                                {59, new byte[] {76, 76, 38}},
                                {60, new byte[] {191, 255, 0}},
                                {61, new byte[] {223, 255, 127}},
                                {62, new byte[] {153, 204, 0}},
                                {63, new byte[] {178, 204, 102}},
                                {64, new byte[] {114, 153, 0}},
                                {65, new byte[] {133, 153, 76}},
                                {66, new byte[] {95, 127, 0}},
                                {67, new byte[] {111, 127, 63}},
                                {68, new byte[] {57, 76, 0}},
                                {69, new byte[] {66, 76, 38}},
                                {70, new byte[] {127, 255, 0}},
                                {71, new byte[] {191, 255, 127}},
                                {72, new byte[] {102, 204, 0}},
                                {73, new byte[] {153, 204, 102}},
                                {74, new byte[] {76, 153, 0}},
                                {75, new byte[] {114, 153, 76}},
                                {76, new byte[] {63, 127, 0}},
                                {77, new byte[] {95, 127, 63}},
                                {78, new byte[] {38, 76, 0}},
                                {79, new byte[] {57, 76, 38}},
                                {80, new byte[] {63, 255, 0}},
                                {81, new byte[] {159, 255, 127}},
                                {82, new byte[] {51, 204, 0}},
                                {83, new byte[] {127, 204, 102}},
                                {84, new byte[] {38, 153, 0}},
                                {85, new byte[] {95, 153, 76}},
                                {86, new byte[] {31, 127, 0}},
                                {87, new byte[] {79, 127, 63}},
                                {88, new byte[] {19, 76, 0}},
                                {89, new byte[] {47, 76, 38}},
                                {90, new byte[] {0, 255, 0}},
                                {91, new byte[] {127, 255, 127}},
                                {92, new byte[] {0, 204, 0}},
                                {93, new byte[] {102, 204, 102}},
                                {94, new byte[] {0, 153, 0}},
                                {95, new byte[] {76, 153, 76}},
                                {96, new byte[] {0, 127, 0}},
                                {97, new byte[] {63, 127, 63}},
                                {98, new byte[] {0, 76, 0}},
                                {99, new byte[] {38, 76, 38}},
                                {100, new byte[] {0, 255, 63}},
                                {101, new byte[] {127, 255, 159}},
                                {102, new byte[] {0, 204, 51}},
                                {103, new byte[] {102, 204, 127}},
                                {104, new byte[] {0, 153, 38}},
                                {105, new byte[] {76, 153, 95}},
                                {106, new byte[] {0, 127, 31}},
                                {107, new byte[] {63, 127, 79}},
                                {108, new byte[] {0, 76, 19}},
                                {109, new byte[] {38, 76, 47}},
                                {110, new byte[] {0, 255, 127}},
                                {111, new byte[] {127, 255, 191}},
                                {112, new byte[] {0, 204, 102}},
                                {113, new byte[] {102, 204, 153}},
                                {114, new byte[] {0, 153, 76}},
                                {115, new byte[] {76, 153, 114}},
                                {116, new byte[] {0, 127, 63}},
                                {117, new byte[] {63, 127, 95}},
                                {118, new byte[] {0, 76, 38}},
                                {119, new byte[] {38, 76, 57}},
                                {120, new byte[] {0, 255, 191}},
                                {121, new byte[] {127, 255, 223}},
                                {122, new byte[] {0, 204, 153}},
                                {123, new byte[] {102, 204, 178}},
                                {124, new byte[] {0, 153, 114}},
                                {125, new byte[] {76, 153, 133}},
                                {126, new byte[] {0, 127, 95}},
                                {127, new byte[] {63, 127, 111}},
                                {128, new byte[] {0, 76, 57}},
                                {129, new byte[] {38, 76, 66}},
                                {130, new byte[] {0, 255, 255}},
                                {131, new byte[] {127, 255, 255}},
                                {132, new byte[] {0, 204, 204}},
                                {133, new byte[] {102, 204, 204}},
                                {134, new byte[] {0, 153, 153}},
                                {135, new byte[] {76, 153, 153}},
                                {136, new byte[] {0, 127, 127}},
                                {137, new byte[] {63, 127, 127}},
                                {138, new byte[] {0, 76, 76}},
                                {139, new byte[] {38, 76, 76}},
                                {140, new byte[] {0, 191, 255}},
                                {141, new byte[] {127, 223, 255}},
                                {142, new byte[] {0, 153, 204}},
                                {143, new byte[] {102, 178, 204}},
                                {144, new byte[] {0, 114, 153}},
                                {145, new byte[] {76, 133, 153}},
                                {146, new byte[] {0, 95, 127}},
                                {147, new byte[] {63, 111, 127}},
                                {148, new byte[] {0, 57, 76}},
                                {149, new byte[] {38, 66, 76}},
                                {150, new byte[] {0, 127, 255}},
                                {151, new byte[] {127, 191, 255}},
                                {152, new byte[] {0, 102, 204}},
                                {153, new byte[] {102, 153, 204}},
                                {154, new byte[] {0, 76, 153}},
                                {155, new byte[] {76, 114, 153}},
                                {156, new byte[] {0, 63, 127}},
                                {157, new byte[] {63, 95, 127}},
                                {158, new byte[] {0, 38, 76}},
                                {159, new byte[] {38, 57, 76}},
                                {160, new byte[] {0, 63, 255}},
                                {161, new byte[] {127, 159, 255}},
                                {162, new byte[] {0, 51, 204}},
                                {163, new byte[] {102, 127, 204}},
                                {164, new byte[] {0, 38, 153}},
                                {165, new byte[] {76, 95, 153}},
                                {166, new byte[] {0, 31, 127}},
                                {167, new byte[] {63, 79, 127}},
                                {168, new byte[] {0, 19, 76}},
                                {169, new byte[] {38, 47, 76}},
                                {170, new byte[] {0, 0, 255}},
                                {171, new byte[] {127, 127, 255}},
                                {172, new byte[] {0, 0, 204}},
                                {173, new byte[] {102, 102, 204}},
                                {174, new byte[] {0, 0, 153}},
                                {175, new byte[] {76, 76, 153}},
                                {176, new byte[] {0, 0, 127}},
                                {177, new byte[] {63, 63, 127}},
                                {178, new byte[] {0, 0, 76}},
                                {179, new byte[] {38, 38, 76}},
                                {180, new byte[] {63, 0, 255}},
                                {181, new byte[] {159, 127, 255}},
                                {182, new byte[] {51, 0, 204}},
                                {183, new byte[] {127, 102, 204}},
                                {184, new byte[] {38, 0, 153}},
                                {185, new byte[] {95, 76, 153}},
                                {186, new byte[] {31, 0, 127}},
                                {187, new byte[] {79, 63, 127}},
                                {188, new byte[] {19, 0, 76}},
                                {189, new byte[] {47, 38, 76}},
                                {190, new byte[] {127, 0, 255}},
                                {191, new byte[] {191, 127, 255}},
                                {192, new byte[] {102, 0, 204}},
                                {193, new byte[] {153, 102, 204}},
                                {194, new byte[] {76, 0, 153}},
                                {195, new byte[] {114, 76, 153}},
                                {196, new byte[] {63, 0, 127}},
                                {197, new byte[] {95, 63, 127}},
                                {198, new byte[] {38, 0, 76}},
                                {199, new byte[] {57, 38, 76}},
                                {200, new byte[] {191, 0, 255}},
                                {201, new byte[] {223, 127, 255}},
                                {202, new byte[] {153, 0, 204}},
                                {203, new byte[] {178, 102, 204}},
                                {204, new byte[] {114, 0, 153}},
                                {205, new byte[] {133, 76, 153}},
                                {206, new byte[] {95, 0, 127}},
                                {207, new byte[] {111, 63, 127}},
                                {208, new byte[] {57, 0, 76}},
                                {209, new byte[] {66, 38, 76}},
                                {210, new byte[] {255, 0, 255}},
                                {211, new byte[] {255, 127, 255}},
                                {212, new byte[] {204, 0, 204}},
                                {213, new byte[] {204, 102, 204}},
                                {214, new byte[] {153, 0, 153}},
                                {215, new byte[] {153, 76, 153}},
                                {216, new byte[] {127, 0, 127}},
                                {217, new byte[] {127, 63, 127}},
                                {218, new byte[] {76, 0, 76}},
                                {219, new byte[] {76, 38, 76}},
                                {220, new byte[] {255, 0, 191}},
                                {221, new byte[] {255, 127, 223}},
                                {222, new byte[] {204, 0, 153}},
                                {223, new byte[] {204, 102, 178}},
                                {224, new byte[] {153, 0, 114}},
                                {225, new byte[] {153, 76, 133}},
                                {226, new byte[] {127, 0, 95}},
                                {227, new byte[] {127, 63, 111}},
                                {228, new byte[] {76, 0, 57}},
                                {229, new byte[] {76, 38, 66}},
                                {230, new byte[] {255, 0, 127}},
                                {231, new byte[] {255, 127, 191}},
                                {232, new byte[] {204, 0, 102}},
                                {233, new byte[] {204, 102, 153}},
                                {234, new byte[] {153, 0, 76}},
                                {235, new byte[] {153, 76, 114}},
                                {236, new byte[] {127, 0, 63}},
                                {237, new byte[] {127, 63, 95}},
                                {238, new byte[] {76, 0, 38}},
                                {239, new byte[] {76, 38, 57}},
                                {240, new byte[] {255, 0, 63}},
                                {241, new byte[] {255, 127, 159}},
                                {242, new byte[] {204, 0, 51}},
                                {243, new byte[] {204, 102, 127}},
                                {244, new byte[] {153, 0, 38}},
                                {245, new byte[] {153, 76, 95}},
                                {246, new byte[] {127, 0, 31}},
                                {247, new byte[] {127, 63, 79}},
                                {248, new byte[] {76, 0, 19}},
                                {249, new byte[] {76, 38, 47}},
                                {250, new byte[] {51, 51, 51}},
                                {251, new byte[] {91, 91, 91}},
                                {252, new byte[] {132, 132, 132}},
                                {253, new byte[] {173, 173, 173}},
                                {254, new byte[] {214, 214, 214}},
                                {255, new byte[] {255, 255, 255}}
                            };

            return lista;
        }

        #endregion
    }
}