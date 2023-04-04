using System;
using System.Collections.Generic;

using netDxf.Entities.MLeader.MLeaderGeometry;
using netDxf.IO;
using netDxf.Objects;

namespace netDxf.Entities.MLeader;

/// <summary>
/// Represents a Mleader <see cref="EntityObject">entity</see>.
/// </summary>
/// How to render MLEADER: https://atlight.github.io/formats/dxf-leader.html
public class MLeader :
    EntityObject
{

    public MLeaderContext Context = new();

    public short Version = 2;
    public string? StyleHandle;
    public MLeaderType LeaderType = MLeaderType.StraightLine;

    public int LeaderLineColor = -1056964608;
    public string? LeaderLinetypeHandle;
    public short LeaderLineweight = -2;
    public bool HasLanding = true;
    public bool HasDogLeg = true;
    public double DogLegLenght = 8;
    public string? ArrowHeadHandle;
    public double ArrowHeadSize = 4;
    public MLeaderContentType ContentType = MLeaderContentType.MTextContent;

    //====works for MTextData=====// THIS DATA IS DUPLICATED?! Can be found also in Context tags
    public string? TextStyleHandle;
    public short TextLeftAttachemntType = 1;
    public int TextRightAttachmentType = 1;
    public short TextAngleType = 1;
    public short TextAlignmentType = 2;
    public int TextColor = -1056964608;
    public bool HasTextFrame;
    public bool IsTextDirectionNegative;
    public short TextIPEAlign;
    public short TextAttachmentPoint = 1;
    public double Scale = 1;
    public short TextAttachmentDirection;
    public short TextBottomAttachmentType = 9;
    public short TextTopAttachmentType = 9;
    public bool LeaderExtendToText;
    //====works for BlockData =====// THIS DATA IS DUPLICATED?! Can be found also in Context tags
    public string? BlockRecordHandle;
    public int BlockColor = -1056964608;
    public Vector3 BlockScaleVector = new(1, 1, 1);
    public double BlockRotation;
    public short BlockConnectionType;
    public bool IsAnnotative;
    // ===== //

    public MLeaderStyle MLeaderStyle; //APPLIED IN POSTPROCESSING ( SEE DxfReader)

    public MLeader() : base(EntityType.MLeader, DxfObjectCode.MLeader)
    {

    }

    internal static MLeader Load(List<ICodeValueReader> normalTags, List<object> contextTags)
    {
        var mleader = new MLeader();

        mleader.Context = MLeaderContext.Load(contextTags);

        foreach (var tag in normalTags)
        {
            switch (tag.Code)
            {
                case 270:
                    {
                        mleader.Version = tag.ReadShort();
                        break;
                    }
                case 340:
                    {
                        mleader.StyleHandle = tag.ReadString();
                        break;
                    }
                case 170:
                    {
                        mleader.LeaderType = (MLeaderType)tag.ReadShort();
                        break;
                    }
                case 91:
                    {
                        mleader.LeaderLineColor = tag.ReadInt();
                        break;
                    }
                case 341:
                    {
                        mleader.LeaderLinetypeHandle = tag.ReadString();
                        break;
                    }
                case 171:
                    {
                        mleader.LeaderLineweight = tag.ReadShort();
                        break;
                    }
                case 290:
                    {
                        mleader.HasLanding = tag.ReadBool();
                        break;
                    }
                case 291:
                    {
                        mleader.HasDogLeg = tag.ReadBool();
                        break;
                    }
                case 41:
                    {
                        mleader.DogLegLenght = tag.ReadDouble();
                        break;
                    }
                case 342:
                    {
                        mleader.ArrowHeadHandle = tag.ReadString();
                        break;
                    }
                case 42:
                    {
                        mleader.ArrowHeadSize = tag.ReadDouble();
                        break;
                    }
                case 172:
                    {
                        mleader.ContentType = (MLeaderContentType)tag.ReadShort();
                        break;
                    }
                case 343:
                    {
                        mleader.StyleHandle = tag.ReadString();
                        break;
                    }
                case 173:
                    {
                        mleader.TextLeftAttachemntType = tag.ReadShort();
                        break;
                    }
                case 95:
                    {
                        mleader.TextRightAttachmentType = tag.ReadInt();
                        break;
                    }
                case 174:
                    {
                        mleader.TextAngleType = tag.ReadShort();
                        break;
                    }
                case 175:
                    {
                        mleader.TextAlignmentType = tag.ReadShort();
                        break;
                    }
                case 92:
                    {
                        mleader.TextColor = tag.ReadInt();
                        break;
                    }
                case 292:
                    {
                        mleader.HasTextFrame = tag.ReadBool();
                        break;
                    }
                case 344:
                    {
                        mleader.BlockRecordHandle = tag.ReadString();
                        break;
                    }
                case 93:
                    {
                        mleader.BlockColor = tag.ReadInt();
                        break;
                    }
                case 10:
                    {
                        mleader.BlockScaleVector.X = tag.ReadDouble();
                        break;
                    }
                case 20:
                    {
                        mleader.BlockScaleVector.Y = tag.ReadDouble();
                        break;
                    }
                case 30:
                    {
                        mleader.BlockScaleVector.Z = tag.ReadDouble();
                        break;
                    }
                case 43:
                    {
                        mleader.BlockRotation = tag.ReadDouble();
                        break;
                    }
                case 176:
                    {
                        mleader.BlockConnectionType = tag.ReadShort();
                        break;
                    }
                case 293:
                    {
                        mleader.IsAnnotative = tag.ReadBool();
                        break;
                    }
                case 294:
                    {
                        mleader.IsTextDirectionNegative = tag.ReadBool();
                        break;
                    }
                case 178:
                    {
                        mleader.TextIPEAlign = tag.ReadShort();
                        break;
                    }
                case 179:
                    {
                        mleader.TextAttachmentPoint = tag.ReadShort();
                        break;
                    }
                case 45:
                    {
                        mleader.Scale = tag.ReadDouble();
                        break;
                    }
                case 271:
                    {
                        mleader.TextAttachmentDirection = tag.ReadShort();
                        break;
                    }
                case 272:
                    {
                        mleader.TextBottomAttachmentType = tag.ReadShort();
                        break;
                    }
                case 273:
                    {
                        mleader.TextTopAttachmentType = tag.ReadShort();
                        break;
                    }
                case 295:
                    {
                        mleader.LeaderExtendToText = tag.ReadBool();
                        break;
                    }
            }
        }


        return mleader;
    }



    public override void TransformBy(Matrix3 transformation, Vector3 translation)
    {
        throw new NotImplementedException();
    }



    public override object Clone()
    {
        return new MLeader()
        {
            Context = Context.Clone(),
            Version = Version,
            StyleHandle = StyleHandle,
            LeaderType = LeaderType,
            LeaderLineColor = LeaderLineColor,
            LeaderLinetypeHandle = LeaderLinetypeHandle,
            LeaderLineweight = LeaderLineweight,
            HasLanding = HasLanding,
            HasDogLeg = HasDogLeg,
            DogLegLenght = DogLegLenght,
            ArrowHeadHandle = ArrowHeadHandle,
            ArrowHeadSize = ArrowHeadSize,
            ContentType = ContentType,
            TextStyleHandle = TextStyleHandle,
            TextLeftAttachemntType = TextLeftAttachemntType,
            TextAngleType = TextAngleType,
            TextAlignmentType = TextAlignmentType,
            TextColor = TextColor,
            HasTextFrame = HasTextFrame,
            IsTextDirectionNegative = IsTextDirectionNegative,
            TextIPEAlign = TextIPEAlign,

            TextAttachmentPoint = TextAttachmentPoint,
            Scale = Scale,
            TextAttachmentDirection = TextAttachmentDirection,
            TextBottomAttachmentType = TextBottomAttachmentType,
            TextTopAttachmentType = TextTopAttachmentType,
            LeaderExtendToText = LeaderExtendToText,
            BlockRecordHandle = BlockRecordHandle,
            BlockColor = BlockColor,
            BlockScaleVector = BlockScaleVector,

            BlockRotation = BlockRotation,
            BlockConnectionType = BlockConnectionType,
            IsAnnotative = IsAnnotative
        };
    }

}

