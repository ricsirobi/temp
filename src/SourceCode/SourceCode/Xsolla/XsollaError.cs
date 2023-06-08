using SimpleJSON;

namespace Xsolla;

public class XsollaError : IParseble
{
	public enum Source
	{
		XSOLLA_API,
		APP_API,
		HTTP,
		NETWORK,
		CANCEL,
		UNEXPECTED
	}

	public Source errorSource { get; private set; }

	public int errorCode { get; private set; }

	public string elementName { get; private set; }

	public string errorMessage { get; private set; }

	public XsollaError()
	{
		errorSource = Source.APP_API;
	}

	public XsollaError(int errorCode)
	{
		this.errorCode = errorCode;
	}

	public XsollaError(string errorMessage)
	{
		errorSource = Source.APP_API;
		this.errorMessage = errorMessage;
	}

	public XsollaError(int errorCode, string errorMessage)
	{
		errorSource = Source.UNEXPECTED;
		this.errorCode = errorCode;
		this.errorMessage = errorMessage;
	}

	private XsollaError(Source source)
	{
		errorSource = source;
	}

	private XsollaError(Source source, string errorMessage)
	{
		errorSource = source;
		this.errorMessage = errorMessage;
	}

	public IParseble ParseOld(JSONNode errorNode)
	{
		errorSource = Source.XSOLLA_API;
		errorCode = errorNode["attributes"]["code"].AsInt;
		elementName = errorNode["name"];
		errorMessage = errorNode["value"];
		return this;
	}

	public IParseble ParseNew(JSONNode errorNode)
	{
		errorSource = Source.XSOLLA_API;
		errorCode = errorNode["code"].AsInt;
		elementName = errorNode["element_name"];
		errorMessage = errorNode["message"];
		return this;
	}

	public IParseble Parse(JSONNode errorNode)
	{
		errorSource = Source.XSOLLA_API;
		if (errorNode.Count > 2)
		{
			errorCode = errorNode["code"].AsInt;
			elementName = errorNode["element_name"];
			errorMessage = errorNode["message"];
			return this;
		}
		if (errorNode.Count == 0)
		{
			return null;
		}
		errorCode = errorNode["error_code"].AsInt;
		string text = errorNode["error"].Value;
		if (text == null || "".Equals(text))
		{
			text = errorNode["message"];
		}
		errorMessage = text;
		return this;
	}

	public static XsollaError GetUnsuportedError()
	{
		return new XsollaError(Source.XSOLLA_API, "Wrong Payment System");
	}

	public static XsollaError GetCancelError()
	{
		return new XsollaError(Source.CANCEL, "Canceled");
	}

	public string GetMessage()
	{
		return errorCode + " : " + errorMessage;
	}

	public override string ToString()
	{
		return string.Format("[XsollaError]\n errorSource= " + errorSource.ToString() + "\n errorCode= " + errorCode + "\n errorMessage= " + errorMessage);
	}
}
