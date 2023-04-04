
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
using System.Collections.Generic;

using netDxf.Objects;
using netDxf.Tables;

namespace netDxf.Collections;

/// <summary>
/// Represents a collection of MLeader styles.
/// </summary>
public sealed class MLeaderStyles :
    TableObjects<MLeaderStyle>
{
    #region constructor

    internal MLeaderStyles(DxfDocument document)
        : this(document, null)
    {
    }

    internal MLeaderStyles(DxfDocument document, string handle)
        : base(document, DxfObjectCode.MLineStyleDictionary, handle)
    {
    }

    #endregion

    #region override methods

    /// <summary>
    /// Adds a Mleader style to the list.
    /// </summary>
    /// <param name="style"><see cref="MLeaderStyle">MLeaderStyle</see> to add to the list.</param>
    /// <param name="assignHandle">Specifies if a handle needs to be generated for the multiline style parameter.</param>
    /// <returns>
    /// If a MLeader style already exists with the same name as the instance that is being added the method returns the existing multiline style,
    /// if not it will return the new MLeader style.
    /// </returns>
    internal override MLeaderStyle Add(MLeaderStyle style, bool assignHandle)
    {
        if (style == null)
        {
            throw new ArgumentNullException(nameof(style));
        }

        if (this.list.TryGetValue(style.Name, out var add))
        {
            return add;
        }

        if (assignHandle || string.IsNullOrEmpty(style.Handle))
        {
            this.Owner.NumHandles = style.AssignHandle(this.Owner.NumHandles);
        }

        this.list.Add(style.Name, style);
        this.references.Add(style.Name, new List<DxfObject>());

        style.Owner = this;

        style.NameChanged += this.Item_NameChanged;

        this.Owner.AddedObjects.Add(style.Handle, style);

        return style;
    }


    public override bool Remove(string name)
    {
        return this.Remove(this[name]);
    }

    public override bool Remove(MLeaderStyle item)
    {
        if (item == null)
        {
            return false;
        }

        if (!this.Contains(item))
        {
            return false;
        }

        if (item.IsReserved)
        {
            return false;
        }

        if (this.references[item.Name].Count != 0)
        {
            return false;
        }

        this.Owner.AddedObjects.Remove(item.Handle);
        this.references.Remove(item.Name);
        this.list.Remove(item.Name);

        item.Handle = null;
        item.Owner = null;

        item.NameChanged -= this.Item_NameChanged;

        return true;
    }

    #endregion

    #region MLineStyle events

    private void Item_NameChanged(TableObject sender, TableObjectChangedEventArgs<string> e)
    {
        if (this.Contains(e.NewValue))
        {
            throw new ArgumentException("There is already another MLeader style with the same name.");
        }

        this.list.Remove(sender.Name);
        this.list.Add(e.NewValue, (MLeaderStyle)sender);

        var refs = this.references[sender.Name];
        this.references.Remove(sender.Name);
        this.references.Add(e.NewValue, refs);
    }

    private void MLineStyle_ElementLinetypeChanged(MLeaderStyle sender, TableObjectChangedEventArgs<Linetype> e)
    {
        this.Owner.Linetypes.References[e.OldValue.Name].Remove(sender);

        e.NewValue = this.Owner.Linetypes.Add(e.NewValue);
        this.Owner.Linetypes.References[e.NewValue.Name].Add(sender);
    }

    private void MLineStyle_ElementAdded(MLeaderStyle sender, MLineStyleElementChangeEventArgs e)
    {
        e.Item.Linetype = this.Owner.Linetypes.Add(e.Item.Linetype);
        this.Owner.Linetypes.References[e.Item.Linetype.Name].Add(sender);
    }

    private void MLineStyle_ElementRemoved(MLeaderStyle sender, MLineStyleElementChangeEventArgs e)
    {
        this.Owner.Linetypes.References[e.Item.Linetype.Name].Remove(sender);
    }

    #endregion
}
