using System.Collections.Generic;

using netDxf.IO;
using netDxf.Tables;

namespace netDxf.Entities.MLeader.InsertedData;

public class MTextData : IMLeaderInsertedContent
{

    // 304: "default_content",
    // 11: "extrusion",
    // 340: "style_handle",
    // 12: "insert",
    // 13: "text_direction",
    // 42: "rotation",
    // 43: "width",
    // 44: "defined_height",
    // 45: "line_spacing_factor",
    // 170: "line_spacing_style",
    // 90: "color",
    // 171: "alignment",
    // 172: "flow_direction",
    // 91: "bg_color",
    // 141: "bg_scale_factor",
    // 92: "bg_transparency",
    // 291: "use_window_bg_color",
    // 292: "has_bg_fill",
    // 173: "column_type",
    // 293: "use_auto_height",
    // 142: "column_width",
    // 143: "column_gutter_width",
    // 294: "column_flow_reversed",
    // 144: "column_sizes",  # multiple values
    // 295: "use_word_break",

    public string DefaultContent = string.Empty;
    public Vector3 Extrusion = Vector3.UnitZ;
    public string StyleHandle = "0";  //handle of TextStyle() table entry
    public Vector3 Insert = Vector3.Zero;
    public Vector3 TextDirection = Vector3.UnitX; // text direction
    public double Rotation; //In radians
    public double Width; //MText width, not scaled
    public double DefinedHeight; //defined column height, not scaled
    public double LineSpacingFactor = 1.0;
    public short LineSpacingStyle = 1; //1=at least, 2=exactly
    public int Color = -1056964608;
    public MTextDataAlignment Alignment = MTextDataAlignment.Left; //1=left, 2=center, 3=right
    public short FlowDirection = 1; //1=horiz, 3=vert, 6=by style
    public int BgColor = -939524096;
    public double BgScaleFactor = 1.5;
    public int BgTransparency = 1;
    public bool UseWindowBgColor;
    public bool HasBgFill;
    public short ColumnType = 1; //Unknown values
    public bool UseAutoHeight;
    public double ColumnWidth; //NotScaled
    public double ColumnGutterWidth; //NotScaled
    public bool ColumnFlowReversed;
    public List<double> ColumnSizes = new(); // Heights? Not scaled
    public bool UseWordBreak = true;

    public TextStyle? TextStyle;  //Updated in DxfReader

    bool IMLeaderInsertedContent.ParseTag(ICodeValueReader tag)
    {
        switch (tag.Code)
        {
            case 144:
                ColumnSizes.Add(tag.ReadDouble());
                break;
            case 304:
                DefaultContent = tag.ReadString();
                break;
            case 11:
                Extrusion.X = tag.ReadDouble();
                break;
            case 21:
                Extrusion.Y = tag.ReadDouble();
                break;
            case 31:
                Extrusion.Z = tag.ReadDouble();
                break;
            case 340:
                StyleHandle = tag.ReadString();
                break;
            case 12:
                Insert.X = tag.ReadDouble();
                break;
            case 22:
                Insert.Y = tag.ReadDouble();
                break;
            case 32:
                Insert.Z = tag.ReadDouble();
                break;
            case 13:
                TextDirection.X = tag.ReadDouble();
                break;
            case 23:
                TextDirection.Y = tag.ReadDouble();
                break;
            case 33:
                TextDirection.Z = tag.ReadDouble();
                break;
            case 42:
                Rotation = tag.ReadDouble();
                break;
            case 43:
                Width = tag.ReadDouble();
                break;
            case 44:
                DefinedHeight = tag.ReadDouble();
                break;
            case 45:
                LineSpacingFactor = tag.ReadDouble();
                break;
            case 170:
                LineSpacingStyle = tag.ReadShort();
                break;
            case 90:
                Color = tag.ReadInt();
                break;
            case 171:
                Alignment = (MTextDataAlignment)tag.ReadShort();
                break;
            case 172:
                FlowDirection = tag.ReadShort();
                break;
            case 91:
                BgColor = tag.ReadInt();
                break;
            case 141:
                BgScaleFactor = tag.ReadDouble();
                break;
            case 92:
                BgTransparency = tag.ReadInt();
                break;
            case 291:
                UseWindowBgColor = tag.ReadBool();
                break;
            case 292:
                HasBgFill = tag.ReadBool();
                break;
            case 173:
                ColumnType = tag.ReadShort();
                break;
            case 293:
                UseAutoHeight = tag.ReadBool();
                break;
            case 142:
                ColumnWidth = tag.ReadDouble();
                break;
            case 143:
                ColumnGutterWidth = tag.ReadDouble();
                break;
            case 294:
                ColumnFlowReversed = tag.ReadBool();
                break;
            case 295:
                UseWordBreak = tag.ReadBool();
                break;
            default:
                return false;
        }
        //return True if data belongs to MText else False (end of Mtext section)
        return true;

    }



    public MTextData Clone()
    {
        return new MTextData()
        {
            DefaultContent = DefaultContent,
            Extrusion = new Vector3(Extrusion.X, Extrusion.Y, Extrusion.Z),
            StyleHandle = StyleHandle,
            Insert = new Vector3(Insert.X, Insert.Y, Insert.Z),
            TextDirection = new Vector3(TextDirection.X, TextDirection.Y, TextDirection.Z),
            Rotation = Rotation,
            Width = Width,
            DefinedHeight = DefinedHeight,
            LineSpacingFactor = LineSpacingFactor,
            LineSpacingStyle = LineSpacingStyle,
            Color = Color,
            Alignment = Alignment,
            FlowDirection = FlowDirection,
            BgColor = BgColor,
            BgScaleFactor = BgScaleFactor,
            BgTransparency = BgTransparency,
            UseWindowBgColor = UseWindowBgColor,
            HasBgFill = HasBgFill,
            ColumnType = ColumnType,
            UseAutoHeight = UseAutoHeight,
            ColumnWidth = ColumnWidth,
            ColumnGutterWidth = ColumnGutterWidth,
            ColumnFlowReversed = ColumnFlowReversed,
            ColumnSizes = ColumnSizes,
            UseWordBreak = UseWordBreak
        };
    }
}
