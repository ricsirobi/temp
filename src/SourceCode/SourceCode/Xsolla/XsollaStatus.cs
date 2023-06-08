using SimpleJSON;

namespace Xsolla;

public class XsollaStatus : IParseble
{
	public enum Group
	{
		INVOICE,
		DONE,
		TROUBLED,
		DELIVERING,
		UNKNOWN
	}

	public static string S_GROUP = "group";

	public static string S_INVOICE = "invoice";

	public static string S_DATA = "statusData";

	public static string S_TEXT = "text";

	private XsollaStatusData statusData;

	private string invoice;

	private string group;

	private XsollaStatusText text;

	private string country;

	private string returnRegion;

	private bool isCancelUser;

	private bool isPreloader;

	private bool showEmailRequest;

	private string titleClass;

	private bool needToCheck;

	public override string ToString()
	{
		return $"[XsollaStatus: S_GROUP={S_GROUP}, S_INVOICE={S_INVOICE}, S_DATA={S_DATA}, S_TEXT={S_TEXT}, statusData={statusData}, invoice={invoice}, group={group}, text={text}, country={country}, returnRegion={returnRegion}, isCancelUser={isCancelUser}, isPreloader={isPreloader}, showEmailRequest={showEmailRequest}, titleClass={titleClass}, needToCheck={needToCheck}]";
	}

	public Group GetGroup()
	{
		return group switch
		{
			"invoice" => Group.INVOICE, 
			"done" => Group.DONE, 
			"delivering" => Group.DELIVERING, 
			"troubled" => Group.TROUBLED, 
			_ => Group.UNKNOWN, 
		};
	}

	public bool IsDone()
	{
		return "done".Equals(group);
	}

	public string GetInvoice()
	{
		return invoice;
	}

	public IParseble Parse(JSONNode rootNode)
	{
		JSONNode jSONNode = rootNode["status"];
		if (jSONNode != null && !"null".Equals(jSONNode.ToString()))
		{
			group = jSONNode[S_GROUP];
			invoice = jSONNode[S_INVOICE];
			statusData = new XsollaStatusData(jSONNode[S_DATA]);
			text = new XsollaStatusText(jSONNode[S_TEXT]);
			country = jSONNode["country"];
			returnRegion = jSONNode["return_region"];
			isCancelUser = jSONNode["isCancelUser"].AsBool;
			isPreloader = jSONNode["isPreloader"].AsBool;
			showEmailRequest = jSONNode["showEmailRequest"].AsBool;
			titleClass = jSONNode["title_class"];
			needToCheck = jSONNode["needToCheck"].AsBool;
		}
		return this;
	}

	public XsollaStatusText GetStatusText()
	{
		return text;
	}

	public XsollaStatusData GetStatusData()
	{
		return statusData;
	}

	public bool GetNeedCheck()
	{
		return needToCheck;
	}

	public bool IsCancelUser()
	{
		return isCancelUser;
	}
}
