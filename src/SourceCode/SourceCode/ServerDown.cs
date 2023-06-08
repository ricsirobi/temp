using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ServerDown", Namespace = "")]
public class ServerDown
{
	[XmlElement(ElementName = "Down")]
	public bool Down;

	[XmlElement(ElementName = "Message")]
	public List<ServerDownMessage> Messages;

	[XmlElement(ElementName = "Scheduled")]
	public ServerDownScheduled Scheduled;

	[XmlElement(ElementName = "ProductDown")]
	public List<int> ProductDown;

	public const string SERVER_DOWN_TIME_FORMAT = "MM/dd/yyyy hh:mm tt";

	public const string SERVER_DOWN = "SERVER_DOWN";

	public static bool pIsReady;

	private static ServerDown mInstance;

	public static ServerDown pInstance => mInstance;

	public static void Init()
	{
		string dataURL = ProductConfig.pInstance.GetDataURL("");
		dataURL = dataURL.Substring(0, dataURL.IndexOf(".com") + 4);
		dataURL += "/ServerDown.xml";
		RsResourceManager.Load(dataURL, XMLDownloaded, RsResourceType.XML, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: true);
	}

	private static void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inFile != null)
			{
				mInstance = UtUtilities.DeserializeFromXml<ServerDown>((string)inFile);
			}
			pIsReady = true;
			break;
		case RsResourceLoadEvent.ERROR:
			pIsReady = true;
			break;
		}
	}

	public ServerDownMessage GetMessage()
	{
		return GetMessage(Messages);
	}

	public ServerDownMessage GetMessage(List<ServerDownMessage> messages)
	{
		string localeLanguage = UtUtilities.GetLocaleLanguage();
		ServerDownMessage serverDownMessage = null;
		ServerDownMessage serverDownMessage2 = null;
		if (messages != null)
		{
			for (int i = 0; i < messages.Count; i++)
			{
				if (messages[i].ProductID.HasValue && messages[i].ProductID.Value == ProductConfig.pProductID && messages[i].Locale.Equals(localeLanguage, StringComparison.OrdinalIgnoreCase))
				{
					return messages[i];
				}
				if (serverDownMessage2 == null && messages[i].ProductGroupID.HasValue && messages[i].ProductGroupID.Value == ProductConfig.pProductGroupID && messages[i].Locale.Equals(localeLanguage, StringComparison.OrdinalIgnoreCase))
				{
					serverDownMessage2 = messages[i];
				}
				if (!messages[i].ProductGroupID.HasValue && !messages[i].ProductID.HasValue && (messages[i].Locale.Equals(localeLanguage, StringComparison.OrdinalIgnoreCase) || (serverDownMessage == null && localeLanguage == "en-US")))
				{
					serverDownMessage = messages[i];
				}
			}
			if (serverDownMessage2 != null)
			{
				return serverDownMessage2;
			}
			return serverDownMessage;
		}
		return null;
	}
}
