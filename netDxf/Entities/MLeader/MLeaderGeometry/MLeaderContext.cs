using System;
using System.Collections.Generic;

using netDxf.Entities.MLeader.InsertedData;
using netDxf.IO;

namespace netDxf.Entities.MLeader.MLeaderGeometry;

public class MLeaderContext
{
    // ATTRIBS = {
    //     40: "scale",
    //     10: "base_point",
    //     41: "char_height",
    //     140: "arrow_head_size",
    //     145: "landing_gap_size",
    //     174: "left_attachment",
    //     175: "right_attachment",
    //     176: "text_align_type",
    //     177: "attachment_type",
    //     110: "plane_origin",
    //     111: "plane_x_axis",
    //     112: "plane_y_axis",
    //     297: "plane_normal_reversed",
    //     272: "top_attachment",
    //     273: "bottom_attachment",
    // }

    // # MTEXT base point: is not the MTEXT insertion point!
    // # HORIZONTAL leader attachment:
    // # the "base_point" is always the start point of the leader on the LEFT side
    // # of the MTEXT, regardless of alignment and which side leaders are attached.
    // # VERTICAL leader attachment:
    // # the "base_point" is always the start point of the leader on the BOTTOM
    // # side of the MTEXT, regardless of alignment and which side leaders are
    // # attached.
    //
    // # BLOCK base point: is not the BLOCK insertion point!
    // # HORIZONTAL leader attachment:
    // # Strange results, setting the "base_point" to the left center of the BLOCK,
    // # regardless which side leaders are attached, seems to be reasonable.
    // # VERTICAL leader attachment: not supported by BricsCAD

    public List<LeaderData> Leaders = new();
    public double Scale = 1.0;
    public Vector3 BasePoint = Vector3.Zero;
    public double CharHeight = 4.0;
    public double ArrowHeadSize = 4.0;
    public double LandingGapSize = 2.0;
    public short LeftAttachment = 1;
    public short RightAttachment = 1;
    public short TextAlignType; //0=left, 1=center, 2=right
    public short AttachmentType; //0=content extents, 1=insertion point
    public MTextData? MText;
    public BlockData? Block;
    public Vector3 PlaneOrigin = Vector3.Zero;
    public Vector3 PlaneXAxis = Vector3.UnitX;
    public Vector3 PlaneYAxis = Vector3.UnitY;
    public bool PlaneNormalReversed;
    public int TopAttachment = 9;
    public int BottomAttachment = 9;

    public static MLeaderContext Load(List<object> contextTags)
    {
        var firstTag = contextTags[0];

        if (!(firstTag is ICodeValueReader tcv && tcv.Code == DxfReader.START_CONTEXT_DATA && (string)tcv.Value == DxfReader.CONTEXT_STR))
        {
            if (firstTag is ICodeValueReader t)
            {
                throw new Exception($"Cannot load LeaderContext - wrong start code: {t.Code} - {t.Value}");
            }
            else
            {
                throw new Exception($"Cannot load LeaderContext - wrong start code: {firstTag.GetType().Name}");
            }
        }

        var ctx = new MLeaderContext();
        IMLeaderInsertedContent? content = null;


        IMLeaderInsertedContent? insertedContent = null;

        var insertedContentFound = false;

        foreach (var tag in contextTags)
        {

            switch (tag)
            {
                case List<object> leaderDataTags:
                    ctx.Leaders.Add(LeaderData.Load(leaderDataTags));
                    break;
                case ICodeValueReader tagCodeValue:

                    if (insertedContentFound && insertedContent is not null) //We must add some properties to different, inserted object - it can be Text or Block
                    {
                        //Console.WriteLine($"This tag belongs to InsertedContent! {tagCodeValue.Code} - {tagCodeValue.Value}");
                        if (insertedContent.ParseTag(tagCodeValue)) //changing Content Values inside - if tag is not connected with content - next values should be applied here
                        {
                            //Console.WriteLine($"Tag added to content");
                            continue;
                        }
                        else
                        {
                            //Console.WriteLine($"Its time to stop!");
                            insertedContentFound = false;
                        }
                    }

                    switch (tagCodeValue.Code)
                    {

                        case 290 when tagCodeValue.ReadBool() == true:
                            {
                                ctx.MText = new MTextData();
                                insertedContent = ctx.MText;
                                insertedContentFound = true;
                                break;
                            }
                        case 296 when tagCodeValue.ReadBool() == true:
                            {
                                ctx.Block = new BlockData();
                                insertedContent = ctx.Block;
                                insertedContentFound = true;
                                break;
                            }
                        case 40:
                            {
                                ctx.Scale = tagCodeValue.ReadDouble();
                                break;
                            }

                        case 41:
                            {
                                ctx.CharHeight = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 10:
                            {
                                ctx.BasePoint.X = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 20:
                            {
                                ctx.BasePoint.Y = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 30:
                            {
                                ctx.BasePoint.Z = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 140:
                            {
                                ctx.ArrowHeadSize = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 145:
                            {
                                ctx.LandingGapSize = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 174:
                            {
                                ctx.LeftAttachment = tagCodeValue.ReadShort();
                                break;
                            }
                        case 175:
                            {
                                ctx.RightAttachment = tagCodeValue.ReadShort();
                                break;
                            }
                        case 176:
                            {
                                ctx.TextAlignType = tagCodeValue.ReadShort();
                                break;
                            }
                        case 177:
                            {
                                ctx.AttachmentType = tagCodeValue.ReadShort();
                                break;
                            }
                        case 110:
                            {
                                ctx.PlaneOrigin.X = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 120:
                            {
                                ctx.PlaneOrigin.Y = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 130:
                            {
                                ctx.PlaneOrigin.Z = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 111:
                            {
                                ctx.PlaneXAxis.X = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 121:
                            {
                                ctx.PlaneXAxis.Y = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 131:
                            {
                                ctx.PlaneXAxis.Z = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 112:
                            {
                                ctx.PlaneYAxis.X = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 122:
                            {
                                ctx.PlaneYAxis.Y = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 132:
                            {
                                ctx.PlaneYAxis.Z = tagCodeValue.ReadDouble();
                                break;
                            }
                        case 297:
                            {
                                ctx.PlaneNormalReversed = tagCodeValue.ReadBool();
                                break;
                            }
                        case 272:
                            {
                                ctx.TopAttachment = tagCodeValue.ReadInt();
                                break;
                            }
                        case 273:
                            {
                                ctx.BottomAttachment = tagCodeValue.ReadInt();
                                break;
                            }
                    }

                    break;
                default:
                    throw new Exception("Something wrong with types!");
                    break;
            }
        }

        return ctx;
    }
    public MLeaderContext Clone()
    {
        var clonedLeaders = new List<LeaderData>() { };

        foreach (var leader in Leaders)
        {
            clonedLeaders.Add(leader.Clone());
        }

        MTextData? clonedMTextData = null;

        if (MText is not null)
        {
            clonedMTextData = MText.Clone();
        }

        BlockData? clonedBlockData = null;

        if (Block is not null)
        {
            clonedBlockData = Block.Clone();
        }

        return new MLeaderContext()
        {
            Leaders = clonedLeaders,
            Scale = Scale,
            BasePoint = new Vector3(BasePoint.X, BasePoint.Y, BasePoint.Z),
            CharHeight = CharHeight,
            ArrowHeadSize = ArrowHeadSize,
            LandingGapSize = LandingGapSize,
            LeftAttachment = LeftAttachment,
            RightAttachment = RightAttachment,
            TextAlignType = TextAlignType,
            AttachmentType = AttachmentType,
            MText = clonedMTextData,
            Block = clonedBlockData,
            PlaneOrigin = new Vector3(PlaneOrigin.X, PlaneOrigin.Y, PlaneOrigin.Z),
            PlaneXAxis = new Vector3(PlaneXAxis.X, PlaneXAxis.Y, PlaneXAxis.Z),
            PlaneNormalReversed = PlaneNormalReversed,
            TopAttachment = TopAttachment,
            BottomAttachment = BottomAttachment
        };
    }

}
