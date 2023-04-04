using netDxf.IO;

namespace netDxf.Entities.MLeader.InsertedData;

internal interface IMLeaderInsertedContent
{
    internal bool ParseTag(ICodeValueReader tag);

}
