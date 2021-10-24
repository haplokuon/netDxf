#region netDxf library licensed under the MIT License
// 
//                       netDxf library
// Copyright (c) 2019-2021 Daniel Carvajal (haplokuon@gmail.com)
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

namespace netDxf.Units
{
    /// <summary>
    /// Default drawing units for AutoCAD DesignCenter blocks.
    /// </summary>
    public enum DrawingUnits
    {
        /// <summary>
        /// Unitless.
        /// </summary>
        Unitless = 0,

        /// <summary>
        /// Inches.
        /// </summary>
        Inches = 1,

        /// <summary>
        /// Feet.
        /// </summary>
        Feet = 2,

        /// <summary>
        /// Miles.
        /// </summary>
        Miles = 3,

        /// <summary>
        /// Millimeters.
        /// </summary>
        Millimeters = 4,

        /// <summary>
        /// Centimeters.
        /// </summary>
        Centimeters = 5,

        /// <summary>
        /// Meters.
        /// </summary>
        Meters = 6,

        /// <summary>
        /// Kilometers.
        /// </summary>
        Kilometers = 7,

        /// <summary>
        /// Microinches.
        /// </summary>
        Microinches = 8,

        /// <summary>
        /// Mils (a thousandth of an inch).
        /// </summary>
        Mils = 9,

        /// <summary>
        /// Yards.
        /// </summary>
        Yards = 10,

        /// <summary>
        /// Angstroms.
        /// </summary>
        Angstroms = 11,

        /// <summary>
        /// Nanometers.
        /// </summary>
        Nanometers = 12,

        /// <summary>
        /// Microns.
        /// </summary>
        Microns = 13,

        /// <summary>
        /// Decimeters.
        /// </summary>
        Decimeters = 14,

        /// <summary>
        /// Decameters
        /// </summary>
        Decameters = 15,

        /// <summary>
        /// Hectometers.
        /// </summary>
        Hectometers = 16,

        /// <summary>
        /// Gigameters.
        /// </summary>
        Gigameters = 17,

        /// <summary>
        /// AstronomicalUnits.
        /// </summary>
        AstronomicalUnits = 18,

        /// <summary>
        /// LightYears.
        /// </summary>
        LightYears = 19,

        /// <summary>
        /// Parsecs.
        /// </summary>
        Parsecs = 20,

        /// <summary>
        /// US Survey Feet
        /// </summary>
        USSurveyFeet=21,

        /// <summary>
        /// US Survey Inches
        /// </summary>
        USSurveyInches=22,

        /// <summary>
        /// US Survey Yards
        /// </summary>
        USSurveyYards=23,

        /// <summary>
        /// US Survey Miles
        /// </summary>
        USSurveyMiles=24
    }
}