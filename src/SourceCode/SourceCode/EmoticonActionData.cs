using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class EmoticonActionData
{
	public class Emoticon
	{
		public int ID;

		public string Name;

		public string Icon;

		public string Emitter;

		public string Texture;

		public string Rollover;

		public bool Locked;
	}

	public class Action
	{
		public int ID;

		public string Name;

		public string Icon;

		public string Animation;

		public bool IsLoop;

		public string Rollover;

		public bool Locked;
	}

	public class Phrase
	{
		public int? ID;

		public string Panel;

		public string Bubble;

		[XmlElement(ElementName = "Phrase")]
		public Phrase[] Phrases;
	}

	public string BundleName;

	public Emoticon[] Emoticons;

	public Action[] Actions;

	public Phrase[] CannedChat;

	public static EmoticonActionData _EmoticonActionData;

	public static AssetBundle _Bundle;

	private static EmoticonActionDataReady mEventDelegate;

	public static bool pIsReady
	{
		get
		{
			if (_EmoticonActionData != null)
			{
				return _Bundle != null;
			}
			return false;
		}
	}

	public static void Init(string fileName, EmoticonActionDataReady callback)
	{
		mEventDelegate = callback;
		if (_EmoticonActionData == null)
		{
			RsResourceManager.Load(fileName, XmlLoadEventHandler);
		}
		else if (_Bundle == null)
		{
			RsResourceManager.Load(_EmoticonActionData.BundleName, BundleLoadEventHandler);
		}
		else if (callback != null)
		{
			mEventDelegate();
		}
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			using (StringReader textReader = new StringReader((string)inObject))
			{
				_EmoticonActionData = (EmoticonActionData)new XmlSerializer(typeof(EmoticonActionData)).Deserialize(textReader);
				RsResourceManager.Load(_EmoticonActionData.BundleName, BundleLoadEventHandler);
				RsResourceManager.SetDontDestroy(_EmoticonActionData.BundleName, inDontDestroy: true);
			}
		}
	}

	private static void BundleLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			_Bundle = (AssetBundle)inObject;
			mEventDelegate();
		}
	}

	public static Emoticon GetEmoticonFromId(int id)
	{
		if (pIsReady)
		{
			Emoticon[] emoticons = _EmoticonActionData.Emoticons;
			foreach (Emoticon emoticon in emoticons)
			{
				if (emoticon.ID == id)
				{
					return emoticon;
				}
			}
		}
		return null;
	}

	public static Action GetActionFromId(int id)
	{
		if (pIsReady)
		{
			Action[] actions = _EmoticonActionData.Actions;
			foreach (Action action in actions)
			{
				if (action.ID == id)
				{
					return action;
				}
			}
		}
		return null;
	}

	public static Phrase GetPhraseFromId(int id)
	{
		if (pIsReady)
		{
			return RecursiveSearchForPhrase(_EmoticonActionData.CannedChat, id);
		}
		return null;
	}

	public static Phrase RecursiveSearchForPhrase(Phrase[] phrases, int id)
	{
		foreach (Phrase phrase in phrases)
		{
			if (phrase.ID.HasValue && phrase.ID == id)
			{
				return phrase;
			}
			if (phrase.Phrases != null)
			{
				Phrase phrase2 = RecursiveSearchForPhrase(phrase.Phrases, id);
				if (phrase2 != null)
				{
					return phrase2;
				}
			}
		}
		return null;
	}
}
