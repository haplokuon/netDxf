using System.Collections.Generic;

using netDxf.IO;

namespace netDxf.Entities.MLeader.InsertedData;

public class BlockData : IMLeaderInsertedContent
{

    // 341: "block_record_handle",
    // 14: "extrusion",
    // 15: "insert",
    // 16: "scale",
    // 46: "rotation",
    // 93: "color",

    public string? BlockRecordHandle;
    public Vector3 Extrusion = Vector3.UnitZ;
    public Vector3 Insert = Vector3.Zero;
    public Vector3 Scale = new(1, 1, 1);
    public double Rotation; //in radians
    public int Color = -1056964608;

    // The transformation matrix is stored in transposed order
    private readonly List<double> _matrix = new();

    bool IMLeaderInsertedContent.ParseTag(ICodeValueReader tag)
    {
        switch (tag.Code)
        {
            case 341:
                BlockRecordHandle = tag.ReadString();
                break;
            case 14:
                Extrusion.X = tag.ReadDouble();
                break;
            case 24:
                Extrusion.Y = tag.ReadDouble();
                break;
            case 34:
                Extrusion.Z = tag.ReadDouble();
                break;
            case 15:
                Insert.X = tag.ReadDouble();
                break;
            case 25:
                Insert.Y = tag.ReadDouble();
                break;
            case 35:
                Insert.Z = tag.ReadDouble();
                break;
            case 16:
                Scale.X = tag.ReadDouble();
                break;
            case 26:
                Scale.Y = tag.ReadDouble();
                break;
            case 36:
                Scale.Z = tag.ReadDouble();
                break;
            case 47:
                _matrix.Add(tag.ReadDouble());
                break;
            case 46:
                Rotation = tag.ReadDouble();
                break;
            case 93:
                Color = tag.ReadInt();
                break;
            default:
                return false;
        }
        //return True if data belongs to block else False (end of block section)
        return true;
    }

    public BlockData Clone()
    {
        return new BlockData()
        {
            BlockRecordHandle = BlockRecordHandle,
            Extrusion = new Vector3(Extrusion.X, Extrusion.Y, Extrusion.Z),
            Insert = new Vector3(Insert.X, Insert.Y, Insert.Z),
            Scale = new Vector3(Scale.X, Scale.Y, Scale.Z),
            Rotation = Rotation,
            Color = Color,
        };
    }
}



