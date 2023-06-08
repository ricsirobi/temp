using System;
using System.Collections.Generic;
using UnityEngine;

public class CmContentReaders
{
	private static System.Random rand;

	public static List<CmAnswerItem> getMCContentTraditionalSeq(UtData dataFile, int n, string[] imgSize, string[] ActorName, int answerAssetType)
	{
		List<CmAnswerItem> list = new List<CmAnswerItem>();
		for (int i = 0; i < 4; i++)
		{
			list.Add(null);
		}
		UtContainer container = dataFile.GetContainer("tsMCContent");
		string value = dataFile.GetValue<string>(container, "SkillAddress", n);
		uint value2 = dataFile.GetValue<uint>(container, "SubLevelID", n);
		uint value3 = dataFile.GetValue<uint>(container, "Level", n);
		CmRK pRK = new CmRK(dataFile.GetValue<uint>(container, "BasicContentID", n), value, value2, value3);
		UtData value4 = dataFile.GetValue<UtData>(container, "tsAnswers", n);
		int recordCount = dataFile.GetRecordCount(value4);
		if (recordCount == 0)
		{
			Debug.Log("No answer available...PROBLEM!!!");
			return list;
		}
		for (int i = 0; i < recordCount; i++)
		{
			int index = (int)(dataFile.GetValue<uint>(value4, "SequenceNum", i) - 1);
			list[index] = getAnswerItemByIndex(dataFile, value4, i, imgSize, ActorName, answerAssetType);
			list[index].pRK = pRK;
		}
		return list;
	}

	public static CmAnswerItem getAnswerItemByIndex(UtData dataFile, UtData tsAnswers, int index, string[] imgSize, string[] ActorName, int answerAssetType)
	{
		bool pbCorrect = ((dataFile.GetValue<uint>(tsAnswers, "CorrectAnswer", index) != 0) ? true : false);
		CmAnswerItem cmAnswerItem = new CmAnswerItem();
		cmAnswerItem.pID = dataFile.GetValue<uint>(tsAnswers, "ContentItemID", index);
		cmAnswerItem.pbCorrect = pbCorrect;
		cmAnswerItem.pText = new List<string>();
		string value = dataFile.GetValue<string>(tsAnswers, "Text", index);
		cmAnswerItem.pText.Add(value);
		if (imgSize != null && imgSize.Length != 0)
		{
			UtData value2 = dataFile.GetValue<UtData>(tsAnswers, "tsImage", index);
			cmAnswerItem.pImage = new CmImageList();
			for (int i = 0; i < imgSize.Length; i++)
			{
				int num = dataFile.FindRecord(value2, 0, "ImageAttributeName", imgSize[i]);
				if (num != -1)
				{
					cmAnswerItem.pImage.Add(imgSize[i], dataFile.GetValue<string>(value2, "ImageFilename", num));
				}
			}
		}
		if (ActorName != null && ActorName.Length != 0)
		{
			UtData value3 = dataFile.GetValue<UtData>(tsAnswers, "tsVO", index);
			cmAnswerItem.pVo = getAudioList(dataFile, value3, 0, ActorName);
		}
		return cmAnswerItem;
	}

	public static List<CmAnswerItem> getManyAnswers(UtData dataFile, int n, List<CmAnswerItem> correctAnswers, int numCorrect, int numDistract, string[] imgSize, string[] ActorName, int answerAssetType, bool bAllowDups, out bool bWasDuped)
	{
		List<CmAnswerItem> list = new List<CmAnswerItem>();
		bWasDuped = false;
		int count = correctAnswers.Count;
		if (numCorrect > count)
		{
			if (bAllowDups)
			{
				correctAnswers.Sort(RandomCompare);
				int num = 0;
				while (correctAnswers.Count < numCorrect)
				{
					correctAnswers.Add(correctAnswers[num]);
					num++;
				}
				bWasDuped = true;
			}
			else
			{
				numCorrect = count;
			}
		}
		else
		{
			count = numCorrect;
		}
		for (int num = 0; num < numCorrect; num++)
		{
			CmAnswerItem singleAnswerItem = getSingleAnswerItem(dataFile, ref correctAnswers, imgSize, ActorName, answerAssetType);
			list.Add(singleAnswerItem);
		}
		List<CmAnswerItem> answerList = getAnswerItems(dataFile, bCorrect: false, imgSize, ActorName, n);
		count = answerList.Count;
		if (numDistract > count)
		{
			if (bAllowDups)
			{
				answerList.Sort(RandomCompare);
				int num = 0;
				while (answerList.Count < numDistract)
				{
					answerList.Add(answerList[num]);
					num++;
				}
				bWasDuped = true;
			}
			else
			{
				numDistract = count;
			}
		}
		else
		{
			count = numDistract;
		}
		for (int num = 0; num < numDistract; num++)
		{
			CmAnswerItem singleAnswerItem2 = getSingleAnswerItem(dataFile, ref answerList, imgSize, ActorName, answerAssetType);
			list.Add(singleAnswerItem2);
		}
		list.Sort(RandomCompare);
		return list;
	}

	public static List<CmAnswerItem> getAnswerItems(UtData dataFile, bool bCorrect, string[] imgSize, string[] ActorName, int n)
	{
		List<CmAnswerItem> list = new List<CmAnswerItem>();
		UtContainer container = dataFile.GetContainer("tsMCContent");
		string value = dataFile.GetValue<string>(container, "SkillAddress", n);
		uint value2 = dataFile.GetValue<uint>(container, "SubLevelID", n);
		uint value3 = dataFile.GetValue<uint>(container, "Level", n);
		CmRK pRK = new CmRK((uint)n, value, value2, value3);
		UtData value4 = dataFile.GetValue<UtData>(container, "tsAnswers", n);
		int recordCount = dataFile.GetRecordCount(value4);
		for (int i = 0; i < recordCount; i++)
		{
			bool flag = ((dataFile.GetValue<uint>(value4, "CorrectAnswer", i) != 0) ? true : false);
			if (flag != bCorrect)
			{
				continue;
			}
			CmAnswerItem cmAnswerItem = new CmAnswerItem();
			cmAnswerItem.pID = dataFile.GetValue<uint>(value4, "ContentItemID", i);
			cmAnswerItem.pRK = pRK;
			cmAnswerItem.pbCorrect = bCorrect;
			cmAnswerItem.pText = new List<string>();
			string value5 = dataFile.GetValue<string>(value4, "Text", i);
			cmAnswerItem.pText.Add(value5);
			if (imgSize != null && imgSize.Length != 0)
			{
				UtData value6 = dataFile.GetValue<UtData>(value4, "tsImage", i);
				cmAnswerItem.pImage = new CmImageList();
				for (int j = 0; j < imgSize.Length; j++)
				{
					int num = dataFile.FindRecord(value6, 0, "ImageAttributeName", imgSize[j]);
					if (num != -1)
					{
						cmAnswerItem.pImage.Add(imgSize[j], dataFile.GetValue<string>(value6, "ImageFilename", num));
					}
				}
			}
			if (ActorName != null && ActorName.Length != 0)
			{
				UtData value7 = dataFile.GetValue<UtData>(value4, "tsVO", i);
				cmAnswerItem.pVo = getAudioList(dataFile, value7, 0, ActorName);
			}
			list.Add(cmAnswerItem);
		}
		return list;
	}

	private static CmAudioList getAudioList(UtData dataFile, UtData tsVo, int startIndex, string[] ActorName)
	{
		CmAudioList cmAudioList = new CmAudioList();
		if (ActorName != null && ActorName.Length != 0)
		{
			for (int i = 0; i < ActorName.Length; i++)
			{
				CmStringList value = new CmStringList();
				cmAudioList.Add(ActorName[i], value);
			}
			for (int i = 0; i < ActorName.Length; i++)
			{
				int num = dataFile.FindRecord(tsVo, startIndex, "ActorName", ActorName[i]);
				if (num != -1)
				{
					cmAudioList[ActorName[i]].Add(dataFile.GetValue<string>(tsVo, "VOFilename", num));
				}
			}
		}
		return cmAudioList;
	}

	public static CmPrompt getPromptData(UtData dataFile, int n, string[] imgSize, string[] ActorName, string rootTable)
	{
		return new CmPrompt
		{
			pVisual = getVisualPrompt(dataFile, n, imgSize, rootTable),
			pAudio = getMainAudioPrompt(dataFile, n, ActorName, rootTable),
			pPosFeedback = getFeedbackPrompt(dataFile, n, ActorName, "tsPositiveFeedback", rootTable),
			pNegFeedback = getFeedbackPrompt(dataFile, n, ActorName, "tsNegativeFeedback", rootTable)
		};
	}

	private static CmVisPrompt getVisualPrompt(UtData dataFile, int n, string[] imgSize, string rootTable)
	{
		CmVisPrompt cmVisPrompt = new CmVisPrompt();
		UtContainer container = dataFile.GetContainer(rootTable);
		UtData value = dataFile.GetValue<UtData>(container, "tsContentPrompts", n);
		UtData value2 = dataFile.GetValue<UtData>(value, "tsVisualPrompt", 0);
		UtData value3 = dataFile.GetValue<UtData>(value2, "tsPromptItem", 0);
		cmVisPrompt.pImg = new CmImageList();
		cmVisPrompt.pText = new List<string>();
		if (imgSize != null && imgSize.Length != 0)
		{
			for (int i = 0; i < imgSize.Length; i++)
			{
				cmVisPrompt.pImg.Add(imgSize[i], "");
			}
		}
		int recordCount = dataFile.GetRecordCount(value3);
		for (int j = 0; j < recordCount; j++)
		{
			cmVisPrompt.pText.Add(dataFile.GetValue<string>(value3, "Text", j));
			if (imgSize == null || imgSize.Length == 0)
			{
				continue;
			}
			UtData value4 = dataFile.GetValue<UtData>(value3, "tsImage", j);
			for (int i = 0; i < imgSize.Length; i++)
			{
				int num = dataFile.FindRecord(value4, 0, "ImageAttributeName", imgSize[i]);
				if (num != -1)
				{
					cmVisPrompt.pImg[imgSize[i]] = dataFile.GetValue<string>(value4, "ImageFilename", num);
				}
			}
		}
		return cmVisPrompt;
	}

	private static CmAudioList getMainAudioPrompt(UtData dataFile, int n, string[] ActorName, string rootTable)
	{
		return getAudioPrompt(dataFile, 0, ActorName, rootTable, n, "tsAudioPrompt");
	}

	private static CmAudioList[] getFeedbackPrompt(UtData dataFile, int n, string[] ActorName, string feedbackTable, string rootTable)
	{
		UtContainer container = dataFile.GetContainer(rootTable);
		UtData value = dataFile.GetValue<UtData>(container, "tsContentPrompts", n);
		UtData value2 = dataFile.GetValue<UtData>(value, feedbackTable, 0);
		int recordCount = dataFile.GetRecordCount(value2);
		CmAudioList[] array = new CmAudioList[recordCount];
		for (int i = 0; i < recordCount; i++)
		{
			array[i] = getAudioPrompt(dataFile, 0, ActorName, rootTable, n, feedbackTable);
		}
		return array;
	}

	private static CmAudioList getAudioPrompt(UtData dataFile, int n, string[] ActorName, string rootTable, int promptIndex, string tableName)
	{
		CmAudioList cmAudioList = new CmAudioList();
		if (ActorName != null && ActorName.Length != 0)
		{
			for (int i = 0; i < ActorName.Length; i++)
			{
				CmStringList value = new CmStringList();
				cmAudioList.Add(ActorName[i], value);
			}
		}
		UtContainer container = dataFile.GetContainer(rootTable);
		UtData value2 = dataFile.GetValue<UtData>(container, "tsContentPrompts", promptIndex);
		UtData value3 = dataFile.GetValue<UtData>(value2, tableName, 0);
		UtData value4 = dataFile.GetValue<UtData>(value3, "tsPromptItem", n);
		int recordCount = dataFile.GetRecordCount(value4);
		if (ActorName != null && ActorName.Length != 0)
		{
			for (int j = 0; j < recordCount; j++)
			{
				UtData value5 = dataFile.GetValue<UtData>(value4, "tsVO", j);
				for (int i = 0; i < ActorName.Length; i++)
				{
					int num = dataFile.FindRecord(value5, 0, "ActorName", ActorName[i]);
					if (num != -1)
					{
						cmAudioList[ActorName[i]].Add(dataFile.GetValue<string>(value5, "VOFilename", num));
					}
				}
			}
		}
		return cmAudioList;
	}

	public static CmDisplayArea getDisplayArea(UtData dataFile, int n, ref List<CmAnswerItem> correctAnswers, CmPrompt Prompt, string[] imgSize, string[] ActorName)
	{
		UtContainer container = dataFile.GetContainer("tsMCContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsContentParams", n);
		string value2 = dataFile.GetValue<string>(value, "CPDisplayAreaType", 0);
		int value3 = dataFile.GetValue<int>(value, "CPDisplayAreaAssetType", 0);
		CmDisplayArea cmDisplayArea = value2 switch
		{
			"MCBase" => getBaseItem(dataFile, n, imgSize, ActorName, value3), 
			"MCCorrectAnswer" => getSingleAnswerItem(dataFile, ref correctAnswers, imgSize, ActorName, value3), 
			"MCPrompt" => getDisplayPrompt(Prompt, imgSize, ActorName, value3), 
			_ => getBaseItem(dataFile, n, imgSize, ActorName, value3), 
		};
		if (cmDisplayArea.pRK == null)
		{
			UtData value4 = dataFile.GetValue<UtData>(container, "tsBase", n);
			string value5 = dataFile.GetValue<string>(container, "SkillAddress", n);
			uint value6 = dataFile.GetValue<uint>(container, "SubLevelID", n);
			uint value7 = dataFile.GetValue<uint>(container, "Level", n);
			CmRK pRK = new CmRK(dataFile.GetValue<uint>(value4, "ContentItemID", 0), value5, value6, value7);
			cmDisplayArea.pRK = pRK;
		}
		return cmDisplayArea;
	}

	public static CmDisplayArea getBaseItem(UtData dataFile, int n, string[] imgSize, string[] ActorName, int daAssetType)
	{
		CmDisplayArea cmDisplayArea = new CmDisplayArea();
		UtContainer container = dataFile.GetContainer("tsMCContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsBase", n);
		string value2 = dataFile.GetValue<string>(container, "SkillAddress", n);
		uint value3 = dataFile.GetValue<uint>(container, "SubLevelID", n);
		uint value4 = dataFile.GetValue<uint>(container, "Level", n);
		CmRK pRK = new CmRK(dataFile.GetValue<uint>(value, "ContentItemID", 0), value2, value3, value4);
		cmDisplayArea.pRK = pRK;
		if (((uint)daAssetType & 0x10u) != 0 && ActorName != null && ActorName.Length != 0)
		{
			UtData value5 = dataFile.GetValue<UtData>(value, "tsVO", 0);
			cmDisplayArea.pVo = getAudioList(dataFile, value5, 0, ActorName);
		}
		List<string> list = new List<string>();
		string value6 = dataFile.GetValue<string>(value, "Text", 0);
		list.Add(value6);
		CmImageList cmImageList = new CmImageList();
		if (imgSize != null && imgSize.Length != 0)
		{
			UtData value7 = dataFile.GetValue<UtData>(value, "tsImage", 0);
			for (int i = 0; i < imgSize.Length; i++)
			{
				int num = dataFile.FindRecord(value7, 0, "ImageAttributeName", imgSize[i]);
				if (num != -1)
				{
					cmImageList.Add(imgSize[i], dataFile.GetValue<string>(value7, "ImageFilename", num));
				}
			}
		}
		if (((uint)daAssetType & 0x40u) != 0)
		{
			if (list.Count == 0)
			{
				cmDisplayArea.pImage = cmImageList;
				cmDisplayArea.pText = null;
			}
			else
			{
				cmDisplayArea.pText = list;
				cmDisplayArea.pImage = null;
			}
		}
		if (((uint)daAssetType & 0x20u) != 0)
		{
			if (cmImageList.Count == 0)
			{
				cmDisplayArea.pText = list;
				cmDisplayArea.pImage = null;
			}
			else
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> item in cmImageList)
				{
					if (item.Value != "0")
					{
						flag = true;
					}
				}
				if (flag)
				{
					cmDisplayArea.pImage = cmImageList;
					cmDisplayArea.pText = null;
				}
				else
				{
					cmDisplayArea.pText = list;
					cmDisplayArea.pImage = null;
				}
			}
		}
		if (((uint)daAssetType & 4u) != 0)
		{
			daAssetType = new System.Random().Next(1, 3);
		}
		if (((uint)daAssetType & (true ? 1u : 0u)) != 0)
		{
			cmDisplayArea.pText = list;
			cmDisplayArea.pImage = null;
		}
		if (((uint)daAssetType & 2u) != 0)
		{
			cmDisplayArea.pImage = cmImageList;
			cmDisplayArea.pText = null;
		}
		return cmDisplayArea;
	}

	public static CmAnswerItem getSingleAnswerItem(UtData dataFile, ref List<CmAnswerItem> answerList, string[] imgSize, string[] ActorName, int daAssetType)
	{
		CmAnswerItem cmAnswerItem = new CmAnswerItem();
		System.Random random = new System.Random();
		int num = -1;
		int num2 = daAssetType;
		int num3 = 0;
		while (num == -1)
		{
			num3++;
			num = random.Next(answerList.Count);
			num2 = daAssetType;
			cmAnswerItem.pRK = answerList[num].pRK;
			cmAnswerItem.pbCorrect = answerList[num].pbCorrect;
			if (((uint)num2 & 0x10u) != 0)
			{
				cmAnswerItem.pVo = answerList[num].pVo;
			}
			if (((uint)num2 & 0x40u) != 0)
			{
				if (answerList[num].pText.Count == 0)
				{
					if (answerList[num].pImage.Count == 0)
					{
						num = -1;
						continue;
					}
					cmAnswerItem.pImage = answerList[num].pImage;
					cmAnswerItem.pText = null;
				}
				else
				{
					cmAnswerItem.pText = answerList[num].pText;
					cmAnswerItem.pImage = null;
				}
			}
			if (((uint)num2 & 0x20u) != 0)
			{
				if (answerList[num].pImage.Count == 0)
				{
					if (answerList[num].pText.Count == 0)
					{
						num = -1;
						continue;
					}
					cmAnswerItem.pText = answerList[num].pText;
					cmAnswerItem.pImage = null;
				}
				else
				{
					bool flag = false;
					foreach (KeyValuePair<string, string> item in answerList[num].pImage)
					{
						if (item.Value != "0")
						{
							flag = true;
						}
					}
					if (flag)
					{
						cmAnswerItem.pImage = answerList[num].pImage;
						cmAnswerItem.pText = null;
					}
					else
					{
						cmAnswerItem.pText = answerList[num].pText;
						cmAnswerItem.pImage = null;
					}
				}
			}
			if (((uint)num2 & 4u) != 0)
			{
				num2 = random.Next(1, 3);
			}
			if (((uint)num2 & (true ? 1u : 0u)) != 0)
			{
				if (answerList[num].pText.Count == 0)
				{
					num = -1;
					continue;
				}
				cmAnswerItem.pText = answerList[num].pText;
				cmAnswerItem.pImage = null;
			}
			if ((num2 & 2) == 0)
			{
				continue;
			}
			bool flag2 = false;
			foreach (KeyValuePair<string, string> item2 in answerList[num].pImage)
			{
				if (item2.Value != "0")
				{
					flag2 = true;
				}
			}
			if (flag2)
			{
				cmAnswerItem.pImage = answerList[num].pImage;
				cmAnswerItem.pText = null;
			}
			else if (num3 > 100)
			{
				cmAnswerItem.pText = answerList[num].pText;
				cmAnswerItem.pImage = null;
				if (cmAnswerItem.pText != null)
				{
					Debug.LogError("Had to take text, which is: " + cmAnswerItem.pText[0]);
				}
			}
			else
			{
				num = -1;
			}
		}
		answerList.RemoveAt(num);
		return cmAnswerItem;
	}

	private static CmDisplayArea getDisplayPrompt(CmPrompt Prompt, string[] imgSize, string[] ActorName, int daAssetType)
	{
		CmDisplayArea cmDisplayArea = new CmDisplayArea();
		if (((uint)daAssetType & 0x10u) != 0)
		{
			cmDisplayArea.pVo = Prompt.pAudio;
		}
		if (((uint)daAssetType & 0x40u) != 0)
		{
			if (Prompt.pVisual.pText.Count == 0)
			{
				cmDisplayArea.pImage = Prompt.pVisual.pImg;
				cmDisplayArea.pText = null;
			}
			else
			{
				cmDisplayArea.pText = Prompt.pVisual.pText;
				cmDisplayArea.pImage = null;
			}
		}
		if (((uint)daAssetType & 0x20u) != 0)
		{
			if (Prompt.pVisual.pImg.Count == 0)
			{
				cmDisplayArea.pText = Prompt.pVisual.pText;
				cmDisplayArea.pImage = null;
			}
			else
			{
				bool flag = false;
				foreach (KeyValuePair<string, string> item in Prompt.pVisual.pImg)
				{
					if (item.Value != "0")
					{
						flag = true;
					}
				}
				if (flag)
				{
					cmDisplayArea.pImage = Prompt.pVisual.pImg;
					cmDisplayArea.pText = null;
				}
				else
				{
					cmDisplayArea.pText = Prompt.pVisual.pText;
					cmDisplayArea.pImage = null;
				}
			}
		}
		if (((uint)daAssetType & 4u) != 0)
		{
			daAssetType = new System.Random().Next(1, 3);
		}
		if (((uint)daAssetType & (true ? 1u : 0u)) != 0)
		{
			cmDisplayArea.pText = Prompt.pVisual.pText;
			cmDisplayArea.pImage = null;
		}
		if (((uint)daAssetType & 2u) != 0)
		{
			cmDisplayArea.pImage = Prompt.pVisual.pImg;
			cmDisplayArea.pText = null;
		}
		return cmDisplayArea;
	}

	public static CmAnswerItem readNestedContentItem(UtData dataFile, int n, int i, string[] imgSize, string[] ActorName, string[] Attribute)
	{
		CmAnswerItem cmAnswerItem = new CmAnswerItem();
		UtContainer container = dataFile.GetContainer("tsBasicContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsBasicParams", n);
		cmAnswerItem.pID = dataFile.GetValue<uint>(value, "ContentItemID", i);
		if (ActorName != null && ActorName.Length != 0)
		{
			UtData value2 = dataFile.GetValue<UtData>(value, "tsVO", i);
			cmAnswerItem.pVo = getAudioList(dataFile, value2, 0, ActorName);
		}
		cmAnswerItem.pText = new List<string>();
		string value3 = dataFile.GetValue<string>(value, "Text", i);
		cmAnswerItem.pText.Add(value3);
		cmAnswerItem.pImage = new CmImageList();
		if (imgSize != null && imgSize.Length != 0)
		{
			UtData value4 = dataFile.GetValue<UtData>(value, "tsImage", i);
			for (int j = 0; j < imgSize.Length; j++)
			{
				int num = dataFile.FindRecord(value4, 0, "ImageAttributeName", imgSize[j]);
				if (num != -1)
				{
					cmAnswerItem.pImage.Add(imgSize[j], dataFile.GetValue<string>(value4, "ImageFilename", num));
				}
			}
		}
		return cmAnswerItem;
	}

	public static CmAnswerItem[] readNestedContentItemList(UtData dataFile, int n, int i, string[] imgSize, string[] ActorName, string[] Attribute)
	{
		UtContainer container = dataFile.GetContainer("tsBasicContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsBasicParams", n);
		UtData value2 = dataFile.GetValue<UtData>(value, "tsList", i);
		int recordCount = dataFile.GetRecordCount(value2);
		CmAnswerItem[] array = new CmAnswerItem[recordCount];
		for (int j = 0; j < recordCount; j++)
		{
			CmAnswerItem cmAnswerItem = new CmAnswerItem();
			cmAnswerItem.pID = dataFile.GetValue<uint>(value2, "ContentItemID", j);
			if (ActorName != null && ActorName.Length != 0)
			{
				UtData value3 = dataFile.GetValue<UtData>(value2, "tsVO", j);
				cmAnswerItem.pVo = getAudioList(dataFile, value3, 0, ActorName);
			}
			cmAnswerItem.pText = new List<string>();
			string value4 = dataFile.GetValue<string>(value2, "Text", j);
			cmAnswerItem.pText.Add(value4);
			cmAnswerItem.pImage = new CmImageList();
			if (imgSize != null && imgSize.Length != 0)
			{
				UtData value5 = dataFile.GetValue<UtData>(value2, "tsImage", j);
				for (int k = 0; k < imgSize.Length; k++)
				{
					int num = dataFile.FindRecord(value5, 0, "ImageAttributeName", imgSize[k]);
					if (num != -1)
					{
						cmAnswerItem.pImage.Add(imgSize[k], dataFile.GetValue<string>(value5, "ImageFilename", num));
					}
				}
			}
			array[j] = cmAnswerItem;
		}
		return array;
	}

	public static List<CmSeqItem> buildSequence(UtData dataFile, int n, int maxSeqLength, string[] imgSize, string[] ActorName)
	{
		UtContainer container = dataFile.GetContainer("tsSeqContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsContentParams", n);
		int value2 = dataFile.GetValue<int>(value, "CPSeqAssetType", 0);
		string value3 = dataFile.GetValue<string>(value, "CPSeqDirection", 0);
		int num = dataFile.GetValue<int>(value, "CPNumRepeats", 0);
		int value4 = dataFile.GetValue<int>(value, "CPNumExamples", 0);
		CmSeqItem.slotType pDisplayType = ((dataFile.GetValue<int>(value, "CPGhostBlanks", 0) == 1) ? CmSeqItem.slotType.ghost : CmSeqItem.slotType.blank);
		List<CmSeqItem> rootSeq = getRootSeq(dataFile, imgSize, ActorName, value2, n);
		switch (value3)
		{
		case "SeqBackward":
			rootSeq.Reverse();
			break;
		case "SeqRandom":
			rootSeq.Sort(RandomCompare);
			break;
		}
		while (rootSeq.Count * num > maxSeqLength)
		{
			num--;
		}
		if (num == 0)
		{
			num = 1;
		}
		bool[] blankPositions = getBlankPositions(dataFile, n, rootSeq.Count);
		List<CmSeqItem> list = new List<CmSeqItem>();
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < rootSeq.Count; j++)
			{
				CmSeqItem item = (CmSeqItem)rootSeq[j].Clone();
				list.Add(item);
				int num2 = list.Count - 1;
				list[num2].pPos = num2;
				if (i < value4)
				{
					list[num2].pDisplayType = CmSeqItem.slotType.display;
				}
				else if (blankPositions[j])
				{
					list[num2].pDisplayType = pDisplayType;
				}
			}
		}
		return list;
	}

	private static List<CmSeqItem> getRootSeq(UtData dataFile, string[] imgSize, string[] ActorName, int seqAssetType, int n)
	{
		UtContainer container = dataFile.GetContainer("tsSeqContent");
		string value = dataFile.GetValue<string>(container, "SkillAddress", n);
		uint value2 = dataFile.GetValue<uint>(container, "SubLevelID", n);
		uint value3 = dataFile.GetValue<uint>(container, "Level", n);
		CmRK pRK = new CmRK((uint)n, value, value2, value3);
		UtData value4 = dataFile.GetValue<UtData>(container, "tsSeqItems", n);
		int recordCount = dataFile.GetRecordCount(value4);
		new List<CmAnswerItem>();
		List<CmSeqItem> list = new List<CmSeqItem>();
		for (int i = 0; i < recordCount; i++)
		{
			CmSeqItem cmSeqItem = new CmSeqItem();
			cmSeqItem.pContentItemID = dataFile.GetValue<uint>(value4, "ContentItemID", i);
			cmSeqItem.pRootPos = dataFile.GetValue<uint>(value4, "SequenceNum", i);
			cmSeqItem.pRK = pRK;
			cmSeqItem.pDisplayType = CmSeqItem.slotType.display;
			if (((uint)seqAssetType & 0x10u) != 0 && ActorName != null && ActorName.Length != 0)
			{
				UtData value5 = dataFile.GetValue<UtData>(value4, "tsVO", i);
				cmSeqItem.pVo = getAudioList(dataFile, value5, 0, ActorName);
			}
			List<string> list2 = new List<string>();
			string value6 = dataFile.GetValue<string>(value4, "Text", i);
			list2.Add(value6);
			CmImageList cmImageList = new CmImageList();
			if (imgSize != null && imgSize.Length != 0)
			{
				UtData value7 = dataFile.GetValue<UtData>(value4, "tsImage", i);
				for (int j = 0; j < imgSize.Length; j++)
				{
					int num = dataFile.FindRecord(value7, 0, "ImageAttributeName", imgSize[j]);
					if (num != -1)
					{
						cmImageList.Add(imgSize[j], dataFile.GetValue<string>(value7, "ImageFilename", num));
					}
				}
			}
			if (((uint)seqAssetType & 0x40u) != 0)
			{
				if (list2.Count == 0)
				{
					cmSeqItem.pImage = cmImageList;
					cmSeqItem.pText = null;
				}
				else
				{
					cmSeqItem.pText = list2;
					cmSeqItem.pImage = null;
				}
			}
			if (((uint)seqAssetType & 0x20u) != 0)
			{
				if (cmImageList.Count == 0)
				{
					cmSeqItem.pText = list2;
					cmSeqItem.pImage = null;
				}
				else
				{
					bool flag = false;
					foreach (KeyValuePair<string, string> item in cmImageList)
					{
						if (item.Value != "0")
						{
							flag = true;
						}
					}
					if (flag)
					{
						cmSeqItem.pImage = cmImageList;
						cmSeqItem.pText = null;
					}
					else
					{
						cmSeqItem.pText = list2;
						cmSeqItem.pImage = null;
					}
				}
			}
			if (((uint)seqAssetType & 4u) != 0)
			{
				seqAssetType = new System.Random().Next(1, 3);
			}
			if (((uint)seqAssetType & (true ? 1u : 0u)) != 0)
			{
				cmSeqItem.pText = list2;
				cmSeqItem.pImage = null;
			}
			if (((uint)seqAssetType & 2u) != 0)
			{
				cmSeqItem.pImage = cmImageList;
				cmSeqItem.pText = null;
			}
			list.Add(cmSeqItem);
		}
		list.Sort(compareRootPos);
		return list;
	}

	private static int compareRootPos(CmSeqItem p, CmSeqItem q)
	{
		if (p.pRootPos == q.pRootPos)
		{
			return 0;
		}
		if (p.pRootPos < q.pRootPos)
		{
			return -1;
		}
		return 1;
	}

	private static bool[] getBlankPositions(UtData dataFile, int n, int numPositions)
	{
		UtContainer container = dataFile.GetContainer("tsSeqContent");
		UtData value = dataFile.GetValue<UtData>(container, "tsSeqBlankPos", n);
		bool[] array = new bool[numPositions];
		for (int i = 0; i < numPositions; i++)
		{
			array[i] = false;
		}
		int recordCount = dataFile.GetRecordCount(value);
		for (int i = 0; i < recordCount; i++)
		{
			uint value2 = dataFile.GetValue<uint>(value, "SequenceNum", i);
			array[value2 - 1] = true;
		}
		return array;
	}

	public static int RandomCompare(object p, object q)
	{
		if (rand == null)
		{
			rand = new System.Random();
		}
		int num = rand.Next(0, 2);
		if (num == 0)
		{
			num = -1;
		}
		return num;
	}
}
