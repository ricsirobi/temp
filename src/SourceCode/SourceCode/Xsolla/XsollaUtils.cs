using SimpleJSON;

namespace Xsolla;

public class XsollaUtils : IParseble
{
	private string accessToken;

	private XsollaUser user;

	private XsollaProject project;

	private XsollaPurchase purchase;

	private XsollaSettings settings;

	private XsollaTranslations translations;

	private XsollaApi api;

	public string GetAcceessToken()
	{
		return accessToken;
	}

	public XsollaUser GetUser()
	{
		return user;
	}

	public XsollaProject GetProject()
	{
		return project;
	}

	public XsollaPurchase GetPurchase()
	{
		return purchase;
	}

	public XsollaSettings GetSettings()
	{
		return settings;
	}

	public XsollaTranslations GetTranslations()
	{
		return translations;
	}

	public void SetAccessToken(string pToken)
	{
		accessToken = pToken;
	}

	public IParseble Parse(JSONNode utilsNode)
	{
		user = new XsollaUser().Parse(utilsNode["user"]) as XsollaUser;
		project = new XsollaProject().Parse(utilsNode["project"]) as XsollaProject;
		purchase = new XsollaPurchase().Parse(utilsNode["purchase"]) as XsollaPurchase;
		settings = new XsollaSettings().Parse(utilsNode["settings"]) as XsollaSettings;
		translations = new XsollaTranslations().Parse(utilsNode["translations"]) as XsollaTranslations;
		api = new XsollaApi().Parse(utilsNode["api"]) as XsollaApi;
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaUtils] \n\n user {user}\n\n project {project}\n\n purchase {purchase}\n\n settings {settings}\n\n project {project}\n\n api {api}";
	}
}
