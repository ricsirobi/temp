using SimpleJSON;

namespace Xsolla;

public class XsollaHistoryVirtualItems : IParseble
{
	public XsollaHistoryVirtualItemsList items;

	public string virtualItemsOperationType { get; private set; }

	public IParseble Parse(JSONNode pNode)
	{
		virtualItemsOperationType = pNode["virtualItemsOperationType"];
		items = new XsollaHistoryVirtualItemsList().Parse(pNode) as XsollaHistoryVirtualItemsList;
		return this;
	}
}
