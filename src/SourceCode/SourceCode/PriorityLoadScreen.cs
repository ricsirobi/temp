using System;
using System.Xml.Serialization;

[Serializable]
public class PriorityLoadScreen
{
	[XmlElement("LoadScreen")]
	public LoadScreen[] LoadScreens;

	[XmlAttribute("Frequency")]
	public int Frequency;

	private int mCurrentFrequency;

	[XmlAttribute("EnterScene")]
	public string EnterScene = "";

	[XmlAttribute("ExitScene")]
	public string ExitScene = "";

	private int mCurrentLoadScreenIndex;

	public int pCurrentFrequency
	{
		get
		{
			return mCurrentFrequency;
		}
		set
		{
			mCurrentFrequency = value;
		}
	}

	public int pCurrentLoadScreenIndex
	{
		get
		{
			return mCurrentLoadScreenIndex;
		}
		set
		{
			mCurrentLoadScreenIndex = value;
			if (LoadScreens != null && LoadScreens.Length != 0 && mCurrentLoadScreenIndex == LoadScreens.Length)
			{
				mCurrentLoadScreenIndex = 0;
			}
		}
	}

	public bool IsSceneNameMatching(bool inIsExitScene, string inSceneName)
	{
		bool flag = false;
		bool num = !string.IsNullOrEmpty(EnterScene) || !string.IsNullOrEmpty(ExitScene);
		string text = (inIsExitScene ? ExitScene : EnterScene);
		if (!num)
		{
			return !inIsExitScene;
		}
		return text.Equals(inSceneName);
	}
}
