using SimpleJSON;

namespace Xsolla;

public interface IParseble
{
	IParseble Parse(JSONNode rootNode);
}
