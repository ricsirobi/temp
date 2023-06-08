using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "QuizData", Namespace = "")]
public class QuizData
{
	public Dictionary<string, string> TypeToPrefabMap;

	[XmlElement(ElementName = "Questions")]
	public QuizQuestion[] Questions;

	private GameObject messageCallbackObject;

	public void LoadQuizDB(QuizQuestion question, GameObject messageObject)
	{
		string text = string.Empty;
		messageCallbackObject = messageObject;
		if (TypeToPrefabMap != null && TypeToPrefabMap.ContainsKey(question.Type))
		{
			string text2 = TypeToPrefabMap[question.Type];
			text = "RS_DATA/" + text2 + ".unity3d/" + text2;
		}
		if (!string.IsNullOrEmpty(text))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(text, OnQuizDBLoaded, typeof(GameObject), inDontDestroy: false, question);
		}
	}

	private void OnQuizDBLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				UiQuizPopupDB component = UnityEngine.Object.Instantiate((GameObject)inObject).GetComponent<UiQuizPopupDB>();
				component._MessageObject = messageCallbackObject;
				KAUI.SetExclusive(component);
				if (component != null && inUserData != null)
				{
					component._QuizQuestions = new QuizQuestion[1] { (QuizQuestion)inUserData };
				}
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			messageCallbackObject = null;
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			messageCallbackObject = null;
			break;
		}
	}
}
