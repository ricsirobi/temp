using UnityEngine;

namespace KA.Framework;

public class LoadingTextData : ScriptableObject
{
	public TextDataByLoadType[] _TextDataByLoadType;

	private static LoadingTextData mInstance;

	private static LoadingTextData pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Resources.Load<LoadingTextData>("LoadingTextData");
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<LoadingTextData>();
				}
			}
			return mInstance;
		}
	}

	public static void GetLoadTextData(LoadingTextType inLoadType, string inSceneName, ref LocaleString[] inLoadingTexts, ref float inDelayBwLoadTextsInSecs)
	{
		TextDataByLoadType[] textDataByLoadType = pInstance._TextDataByLoadType;
		foreach (TextDataByLoadType textDataByLoadType2 in textDataByLoadType)
		{
			if (textDataByLoadType2._Type != inLoadType)
			{
				continue;
			}
			inDelayBwLoadTextsInSecs = textDataByLoadType2._DelayBwLoadTextsInSecs;
			TextData[] dataBySceneName = textDataByLoadType2._DataBySceneName;
			foreach (TextData textData in dataBySceneName)
			{
				if (textData._SceneName == inSceneName || string.IsNullOrEmpty(inSceneName))
				{
					inLoadingTexts = textData._Data;
					return;
				}
			}
		}
	}
}
