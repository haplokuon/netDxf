#region netDxf, Copyright(C) 2014 Daniel Carvajal, Licensed under LGPL.

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

namespace netDxf
{
    /// <summary>
    /// Dxf sections.
    /// </summary>
    internal static class StringCode
    {
        /// <summary>
        /// not defined.
        /// </summary>
        public const string Unknown = "";

        /// <summary>
        /// header.
        /// </summary>
        public const string HeaderSection = "HEADER";

        /// <summary>
        /// classes.
        /// </summary>
        public const string ClassesSection = "CLASSES";

        /// <summary>
        /// class.
        /// </summary>
        public const string Class = "CLASS";

        /// <summary>
        /// tables.
        /// </summary>
        public const string TablesSection = "TABLES";

        /// <summary>
        /// blocks.
        /// </summary>
        public const string BlocksSection = "BLOCKS";

        /// <summary>
        /// entities.
        /// </summary>
        public const string EntitiesSection = "ENTITIES";

        /// <summary>
        /// objects.
        /// </summary>
        public const string ObjectsSection = "OBJECTS";

        /// <summary>
        /// objects.
        /// </summary>
        public const string ThumbnailImageSection = "THUMBNAILIMAGE";

        /// <summary>
        /// Undocumented section. Currently it is used for storing the data for solids, regions, surfaces, and the preview image.
        /// </summary>
        public const string AcdsDataSection = "ACDSDATA";

        /// <summary>
        /// dxf name string.
        /// </summary>
        public const string BeginSection = "SECTION";

        /// <summary>
        /// end secction code.
        /// </summary>
        public const string EndSection = "ENDSEC";

        /// <summary>
        /// layers.
        /// </summary>
        public const string LayerTable = "LAYER";

        /// <summary>
        /// view ports.
        /// </summary>
        public const string VportTable = "VPORT";

        /// <summary>
        /// views.
        /// </summary>
        public const string ViewTable = "VIEW";

        /// <summary>
        /// ucs.
        /// </summary>
        public const string UcsTable = "UCS";
        
        /// <summary>
        /// block records.
        /// </summary>
        public const string BlockRecordTable = "BLOCK_RECORD";

        /// <summary>
        /// line types.
        /// </summary>
        public const string LineTypeTable = "LTYPE";

        /// <summary>
        /// text styles.
        /// </summary>
        public const string TextStyleTable = "STYLE";

        /// <summary>
        /// dim styles.
        /// </summary>
        public const string DimensionStyleTable = "DIMSTYLE";

        /// <summary>
        /// extended data application registry.
        /// </summary>
        public const string ApplicationIDTable = "APPID";

        /// <summary>
        /// end table code.
        /// </summary>
        public const string EndTable = "ENDTAB";

        /// <summary>
        /// dxf name string.
        /// </summary>
        public const string Table = "TABLE";

        /// <summary>
        /// dxf name string.
        /// </summary>
        public const string BeginBlock = "BLOCK";

        /// <summary>
        /// end table code.
        /// </summary>
        public const string EndBlock = "ENDBLK";

        /// <summary>
        /// end of an element sequence
        /// </summary>
        public const string EndSequence = "SEQEND";

        /// <summary>
        /// Generic dictionary
        /// </summary>
        public const string Dictionary = "DICTIONARY";

        /// <summary>
        /// Group dictionary
        /// </summary>
        public const string GroupDictionary = "ACAD_GROUP";

        /// <summary>
        /// Layouts dictionary
        /// </summary>
        public const string LayoutDictionary = "ACAD_LAYOUT";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string MLineStyleDictionary = "ACAD_MLINESTYLE";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string ImageDefDictionary = "ACAD_IMAGE_DICT";

        /// <summary>
        /// MLine styles dictionary
        /// </summary>
        public const string ImageVarsDictionary = "ACAD_IMAGE_VARS";

        /// <summary>
        /// end of file
        /// </summary>
        public const string EndOfFile = "EOF";
    }
}