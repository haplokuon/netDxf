using System;
using System.Collections.Generic;

using netDxf.IO;

namespace netDxf.Entities.MLeader.MLeaderGeometry;

public class LeaderLine
{
    public List<Vector3> Verticies = new(); // WCS coordinates
    public List<object> Breaks = new(); //TODO ???? how is it implemented?
    // # Breaks: 90, 11, 12, [11, 12, ...] [, 90, 11, 12 [11, 12, ...]]
    // # group code 90 = break index
    // # group code 11 = start vertex of break
    // # group code 12 = end vertex of break
    // # multiple breaks per index possible
    public int Index;  //group code 91
    public int Color = -1056964608; //group code 92


    public static LeaderLine Load(List<object> tags)
    {
        var firstTag = tags[0];

        if (!(firstTag is ICodeValueReader tcv && tcv.Code == DxfReader.START_LEADER_LINE && (string)tcv.Value == DxfReader.LEADER_LINE_STR))
        {
            if (firstTag is ICodeValueReader t)
            {
                throw new Exception($"Cannot load LeaderLine - wrong start code: {t.Code} - {t.Value}");
            }
            else
            {
                throw new Exception($"Cannot load LeaderLine - wrong start code: {firstTag.GetType().Name}");
            }
        }
        var line = new LeaderLine();

        var XCoords = new List<double>(); //we can have unknown number of points :')
        var YCoords = new List<double>();
        var ZCoords = new List<double>();

        foreach (var t in tags)
        {

            if (t is ICodeValueReader tagCodeValue)
            {
                switch (tagCodeValue.Code)
                {
                    case 10:
                        XCoords.Add(tagCodeValue.ReadDouble());
                        break;
                    case 20:
                        YCoords.Add(tagCodeValue.ReadDouble());
                        break;
                    case 30:
                        ZCoords.Add(tagCodeValue.ReadDouble());
                        break;
                    case 91:
                        line.Index = tagCodeValue.ReadInt();
                        break;
                    case 92:
                        line.Color = tagCodeValue.ReadInt();
                        break;
                }
            }
            else
            {
                throw new Exception("Something wrong with types!");
            }

        }

        if (XCoords.Count != YCoords.Count || YCoords.Count != ZCoords.Count)
        {
            throw new Exception($"Something wrong with LeaderLine points!: {XCoords.Count}, {YCoords.Count}, {ZCoords.Count}");
        }

        if (XCoords.Count > 0)
        {
            for (var i = 0; i < XCoords.Count; i++)
            {
                line.Verticies.Add(new Vector3(XCoords[i], YCoords[i], ZCoords[i]));
            }
        }



        return line;
    }

    public LeaderLine Clone()
    {

        var clonedVerticies = new List<Vector3>() { };

        foreach (var v in Verticies)
        {
            clonedVerticies.Add(new Vector3(v.X, v.Y, v.Z));
        }
        return new LeaderLine()
        {
            Verticies = clonedVerticies,
            Breaks = Breaks,
            Index = Index,
            Color = Color,
        };
    }

}
