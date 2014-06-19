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

namespace netDxf
{

    /// <summary>
    /// Angular units format for creating objects.
    /// </summary>
    public enum AngleUnitType
    {
        /// <summary>
        /// Decimal degrees.
        /// </summary>
        DecimalDegrees = 0,
        /// <summary>
        /// Degrees/minutes/seconds.
        /// </summary>
        DegreesMinutesSeconds = 1,
        /// <summary>
        /// Gradians
        /// </summary>
        Gradians = 2,
        /// <summary>
        /// Radians
        /// </summary>
        Radians = 3,
        /// <summary>
        /// Surveyor's units.
        /// </summary>
        SurveyorUnits = 4
    }

    /// <summary>
    /// Linear units format for creating objects.
    /// </summary>
    public enum LinearUnitType
    {
        /// <summary>
        /// Scientific.
        /// </summary>
        Scientific = 1,
        /// <summary>
        /// Decimal.
        /// </summary>
        Decimal = 2,
        /// <summary>
        /// Engineering.
        /// </summary>
        Engineering = 3,
        /// <summary>
        /// Architectural.
        /// </summary>
        Architectural = 4,
        /// <summary>
        /// Fractional.
        /// </summary>
        Fractional = 5
    }

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
        Parsecs = 20
    }

    /// <summary>
    /// AutoCAD units for inserting images.
    /// This is what one AutoCAD unit is equal to for the purpose of inserting and scaling images with an associated resolution.
    /// </summary>
    public enum ImageUnits
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,
        /// <summary>
        /// Millimeters.
        /// </summary>
        Millimeters = 1,
        /// <summary>
        /// Centimeters.
        /// </summary>
        Centimeters = 2,
        /// <summary>
        /// Meters.
        /// </summary>
        Meters = 3,
        /// <summary>
        /// Kilometers.
        /// </summary>
        Kilometers = 4,
        /// <summary>
        /// Inches.
        /// </summary>
        Inches = 5,
        /// <summary>
        /// Feet.
        /// </summary>
        Feet = 6,
        /// <summary>
        /// Yards.
        /// </summary>
        Yards = 7,
        /// <summary>
        /// Miles.
        /// </summary>
        Miles = 8
    }

    /// <summary>
    /// Defines the image resolution units.
    /// </summary>
    public enum ImageResolutionUnits
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

}
