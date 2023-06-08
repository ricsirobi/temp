using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot("UpsellGameData")]
public class UpsellGameData
{
	[XmlElement(ElementName = "UpsellGame")]
	public UpsellGame[] Games;

	public static UpsellGameData pUpsellGameData = null;

	private static string mLoadScreenDataFilename = string.Empty;

	private static UpsellGame mGame;

	private static UpsellRule mRule;

	private static int mCurrentRuleIndex;

	private static int mCurrentItemDataIndex;

	private static string mResult;

	public static bool pIsReady => pUpsellGameData != null;

	public static void Init(string inFilename)
	{
		mLoadScreenDataFilename = inFilename;
		if (pUpsellGameData == null)
		{
			RsResourceManager.Load(mLoadScreenDataFilename, XmlLoadEventHandler, RsResourceType.NONE, inDontDestroy: true);
		}
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			pUpsellGameData = UtUtilities.DeserializeFromXml((string)inObject, typeof(UpsellGameData)) as UpsellGameData;
		}
	}

	public static UpsellItemData GetUpsellData(string inGameType, string inResult)
	{
		UpsellItemData result = null;
		mResult = inResult;
		mGame = null;
		mCurrentRuleIndex = -1;
		GetUpsellGame(inGameType);
		if (mGame != null)
		{
			result = GetItemData();
		}
		return result;
	}

	public static UpsellItemData GetNextUpsellData()
	{
		UpsellItemData upsellItemData = null;
		upsellItemData = GetItemDataFromRule();
		if (upsellItemData == null)
		{
			upsellItemData = GetItemData();
		}
		return upsellItemData;
	}

	private static void GetUpsellGame(string inGameType)
	{
		if (mGame != null)
		{
			return;
		}
		for (int i = 0; i < pUpsellGameData.Games.Length; i++)
		{
			if (pUpsellGameData.Games[i].IsGameTypeMatching(inGameType))
			{
				mGame = pUpsellGameData.Games[i];
				break;
			}
		}
	}

	private static UpsellItemData GetItemData()
	{
		UpsellItemData result = null;
		GetNextMatchingRules();
		if (mRule != null)
		{
			result = GetItemDataFromRule();
		}
		return result;
	}

	private static void GetNextMatchingRules()
	{
		mRule = null;
		mCurrentItemDataIndex = -1;
		mCurrentRuleIndex++;
		while (mCurrentRuleIndex < mGame.Rule.Length)
		{
			if (mGame.Rule[mCurrentRuleIndex].IsResultMatching(mResult))
			{
				mRule = mGame.Rule[mCurrentRuleIndex];
				break;
			}
			mCurrentRuleIndex++;
		}
	}

	private static UpsellItemData GetItemDataFromRule()
	{
		mCurrentItemDataIndex++;
		if (mCurrentItemDataIndex < mRule.ItemData.Length)
		{
			return mRule.ItemData[mCurrentItemDataIndex];
		}
		return null;
	}
}
