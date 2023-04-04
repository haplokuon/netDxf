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

using System;

using netDxf.Tables;

namespace netDxf.Entities;

/// <summary>
/// Represents a Ole2Frame entity.
/// https://help.autodesk.com/view/OARX/2023/ENU/?guid=GUID-77747CE6-82C6-4452-97ED-4CEEB38BE960
/// </summary>
public class Ole2Frame :
    EntityObject
{
    #region delegates and events

    // public delegate void StyleChangedEventHandler(Ole2Frame sender, TableObjectChangedEventArgs<ShapeStyle> e);
    // public event StyleChangedEventHandler StyleChanged;
    // protected virtual ShapeStyle OnStyleChangedEvent(ShapeStyle oldStyle, ShapeStyle newStyle)
    // {
    //     StyleChangedEventHandler ae = this.StyleChanged;
    //     if (ae != null)
    //     {
    //         TableObjectChangedEventArgs<ShapeStyle> eventArgs = new TableObjectChangedEventArgs<ShapeStyle>(oldStyle, newStyle);
    //         ae(this, eventArgs);
    //         return eventArgs.NewValue;
    //     }
    //     return newStyle;
    // }

    #endregion

    #region private fields

    private short oleVersionNumber;
    private Vector3 upperLeftCorner;
    private Vector3 lowerRightCorner;
    private Byte[] oleFrameBytes;
    private Ole2FrameType ole2FrameType;

    #endregion

    #region constructors


    /// <summary>
    /// Initializes a new instance of the <c>Ole2Frame</c> class.
    /// </summary>

    public Ole2Frame(short oleVersionNumber, Vector3 upperLeftCorner, Vector3 lowerRightCorner, Byte[] oleFrameBytes, Ole2FrameType ole2FrameType)
        : base(EntityType.Ole2Frame, DxfObjectCode.Ole2FrameSection)
    {
        this.oleVersionNumber = oleVersionNumber;
        this.upperLeftCorner = upperLeftCorner;
        this.lowerRightCorner = lowerRightCorner;
        this.oleFrameBytes = oleFrameBytes;
        this.ole2FrameType = ole2FrameType;
    }

    #endregion

    #region public properties


    public Vector3 UpperLeftCorner
    {
        get { return this.upperLeftCorner; }
        set { this.upperLeftCorner = value; }
    }

    public Vector3 LowerRightCorner
    {
        get
        {
            return this.lowerRightCorner;
        }
        set { this.lowerRightCorner = value; }
    }

    public short OleVersionNumber
    {
        get { return this.oleVersionNumber; }
        set { this.oleVersionNumber = value; }
    }

    public Byte[] OleFrameBytes
    {
        get { return this.oleFrameBytes; }
        set { this.oleFrameBytes = value; }
    }

    public Ole2FrameType Ole2FrameType
    {
        get { return this.ole2FrameType; }
        set { this.ole2FrameType = value; }
    }

    #endregion

    #region overrides

    /// <summary>
    /// Moves, scales, and/or rotates the current entity given a 3x3 transformation matrix and a translation vector.
    /// </summary>
    /// <param name="transformation">Transformation matrix.</param>
    /// <param name="translation">Translation vector.</param>
    /// <remarks>Matrix3 adopts the convention of using column vectors to represent a transformation matrix.</remarks>
    public override void TransformBy(Matrix3 transformation, Vector3 translation)
    {

        var newUpperLeftPosition = transformation * this.UpperLeftCorner + translation;
        var newLowerRightPosition = transformation * this.LowerRightCorner + translation;
        var newNormal = transformation * this.Normal;

        this.UpperLeftCorner = newUpperLeftPosition;
        this.LowerRightCorner = newLowerRightPosition;
        this.Normal = newNormal;
    }

    public override object Clone()
    {
        var entity = new Ole2Frame(this.OleVersionNumber, this.UpperLeftCorner, this.LowerRightCorner, this.OleFrameBytes, this.Ole2FrameType)
        {
            //EntityObject properties
            Layer = (Layer)this.Layer.Clone(),
            Linetype = (Linetype)this.Linetype.Clone(),
            Color = (AciColor)this.Color.Clone(),
            Lineweight = this.Lineweight,
            Transparency = (Transparency)this.Transparency.Clone(),
            LinetypeScale = this.LinetypeScale,
            Normal = this.Normal,
            IsVisible = this.IsVisible,
            //Ole2FrameProperties
            UpperLeftCorner = this.UpperLeftCorner,
            LowerRightCorner = this.LowerRightCorner,
            OleVersionNumber = this.OleVersionNumber,
            OleFrameBytes = this.OleFrameBytes,
            Ole2FrameType = this.Ole2FrameType
        };

        foreach (var data in this.XData.Values)
        {
            entity.XData.Add((XData)data.Clone());
        }

        return entity;
    }

    #endregion
}
