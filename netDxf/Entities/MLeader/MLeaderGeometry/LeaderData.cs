using System;
using System.Collections.Generic;

using netDxf.IO;

namespace netDxf.Entities.MLeader.MLeaderGeometry;

public class LeaderData
{
    public List<LeaderLine> Lines = new();
    public bool HasLastLeaderLine;  // group code 290, in AutoCAD the leader is invisible if set to 0, BricsCAD ignores this flag
    public bool HasDogLegVector;  // group code 291,
    public Vector3 LastLeaderPoint = Vector3.Zero; // group code (10, 20, 30) in WCS
    public Vector3 DogLegVector = Vector3.UnitX; // group code (11, 21, 31) in WCS
    public double DogLegLenght = 1.0; //group code 40
    public int Index; //group code 90
    public int AttachmentDirection; //group code 271
    public List<Vector3> Breaks = new(); // group code 12, 13 - multiple breaks possible!

    public static LeaderData Load(List<object> tags)
    {
        var firstTag = tags[0];

        if (!(firstTag is ICodeValueReader tcv && tcv.Code == DxfReader.START_LEADER && (string)tcv.Value == DxfReader.LEADER_STR))
        {
            if (firstTag is ICodeValueReader t)
            {
                throw new Exception($"Cannot load LeaderData - wrong start code: {t.Code} - {t.Value}");
            }
            else
            {
                throw new Exception("Cannot load LeaderData - wrong start code");
            }

        }


        var firstBreakVectorFound = false;
        var secondBreakVectorFound = false;
        var firstBreakVector = Vector3.Zero;
        var secondBreakVector = Vector3.Zero;

        var leaderData = new LeaderData();


        foreach (var t in tags)
        {
            switch (t)
            {
                case List<object> tList:
                    leaderData.Lines.Add(LeaderLine.Load(tList));
                    break;
                case ICodeValueReader tagCodeValue:
                    switch (tagCodeValue.Code)
                    {
                        case 10:
                            leaderData.LastLeaderPoint.X = tagCodeValue.ReadDouble();
                            break;
                        case 20:
                            leaderData.LastLeaderPoint.Y = tagCodeValue.ReadDouble();
                            break;
                        case 30:
                            leaderData.LastLeaderPoint.Z = tagCodeValue.ReadDouble();
                            break;
                        case 11:
                            leaderData.DogLegVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 21:
                            leaderData.DogLegVector.Y = tagCodeValue.ReadDouble();
                            break;
                        case 31:

                            leaderData.DogLegVector.Z = tagCodeValue.ReadDouble();
                            break;
                        case 12:

                            firstBreakVectorFound = true;
                            firstBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 22:
                            firstBreakVectorFound = true;
                            firstBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 32:
                            firstBreakVectorFound = true;
                            firstBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 13:

                            secondBreakVectorFound = true;
                            secondBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 23:
                            secondBreakVectorFound = true;
                            secondBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 33:
                            secondBreakVectorFound = true;
                            secondBreakVector.X = tagCodeValue.ReadDouble();
                            break;
                        case 290:
                            leaderData.HasLastLeaderLine = tagCodeValue.ReadBool();
                            break;
                        case 291:
                            leaderData.HasDogLegVector = tagCodeValue.ReadBool();
                            break;
                        case 40:
                            leaderData.DogLegLenght = tagCodeValue.ReadDouble();
                            break;
                        case 271:
                            leaderData.AttachmentDirection = tagCodeValue.ReadInt();
                            break;
                    }

                    break;
                default:
                    throw new Exception("Something wrong with types!");
                    break;
            }
        }

        if (firstBreakVectorFound)
        {
            leaderData.Breaks.Add(firstBreakVector);
        }
        if (secondBreakVectorFound)
        {
            leaderData.Breaks.Add(secondBreakVector);
        }

        return leaderData;
    }

    public LeaderData Clone()
    {
        var clonedLines = new List<LeaderLine>() { };
        var clonedBreaks = new List<Vector3>() { };

        foreach (var line in Lines)
        {
            clonedLines.Add(line.Clone());
        }

        foreach (var breakPoint in Breaks)
        {
            clonedBreaks.Add(new Vector3(breakPoint.X, breakPoint.Y, breakPoint.Z));
        }

        return new LeaderData()
        {
            Lines = clonedLines,
            HasLastLeaderLine = HasLastLeaderLine,
            HasDogLegVector = HasDogLegVector,
            LastLeaderPoint = new Vector3(LastLeaderPoint.X, LastLeaderPoint.Y, LastLeaderPoint.Z),
            DogLegVector = new Vector3(DogLegVector.X, DogLegVector.Y, DogLegVector.Z),
            DogLegLenght = DogLegLenght,
            Index = Index,
            AttachmentDirection = AttachmentDirection,
            Breaks = clonedBreaks,

        };
    }

}
