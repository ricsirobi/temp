using System.Collections.Generic;

public class CmParsers
{
	public static CmMCManyToMany GetMCContentMany(UtData dataFile, int numCorrect, int numDistract, int numGroups, string[] imgSize, string[] ActorName, bool UseDisplay, bool bAllowDups)
	{
		UtContainer container = dataFile.GetContainer("tsMCContent");
		int recordCount = dataFile.GetRecordCount(container);
		if (numGroups > recordCount)
		{
			numGroups = recordCount;
		}
		CmMCManyToMany cmMCManyToMany = new CmMCManyToMany();
		cmMCManyToMany.pGroupList = new CmQuestionItem[numGroups];
		for (int i = 0; i < numGroups; i++)
		{
			cmMCManyToMany.pGroupList[i] = new CmQuestionItem();
			UtData value = dataFile.GetValue<UtData>(container, "tsContentParams", i);
			int value2 = dataFile.GetValue<int>(value, "CPAnswerAssetType", 0);
			if (((uint)value2 & 8u) != 0)
			{
				value2 = dataFile.GetValue<int>(value, "CPDisplayAreaAssetType", 0);
			}
			List<CmAnswerItem> correctAnswers = CmContentReaders.getAnswerItems(dataFile, bCorrect: true, imgSize, ActorName, i);
			cmMCManyToMany.pGroupList[i].pPrompt = CmContentReaders.getPromptData(dataFile, i, imgSize, ActorName, "tsMCContent");
			if (UseDisplay)
			{
				cmMCManyToMany.pGroupList[i].pDisplayArea = CmContentReaders.getDisplayArea(dataFile, i, ref correctAnswers, cmMCManyToMany.pGroupList[i].pPrompt, imgSize, ActorName);
			}
			cmMCManyToMany.pGroupList[i].pAnswerList = CmContentReaders.getManyAnswers(dataFile, i, correctAnswers, numCorrect, numDistract, imgSize, ActorName, value2, bAllowDups, out var bWasDuped);
			if (bWasDuped)
			{
				cmMCManyToMany.pbDuped = true;
			}
		}
		return cmMCManyToMany;
	}

	public static CmMCContent GetMCContentTraditional(UtData dataFile, int numDistract, int numQuestions, string[] imgSize, string[] ActorName)
	{
		CmMCContent cmMCContent = new CmMCContent();
		UtContainer container = dataFile.GetContainer("tsMCContent");
		int recordCount = dataFile.GetRecordCount(container);
		if (numQuestions > recordCount)
		{
			numQuestions = recordCount;
		}
		cmMCContent.pQuestionList = new CmQuestionItem[numQuestions];
		for (int i = 0; i < numQuestions; i++)
		{
			cmMCContent.pQuestionList[i] = new CmQuestionItem();
			UtData value = dataFile.GetValue<UtData>(container, "tsContentParams", i);
			int value2 = dataFile.GetValue<int>(value, "CPAnswerAssetType", 0);
			if (((uint)value2 & 8u) != 0)
			{
				value2 = dataFile.GetValue<int>(value, "CPDisplayAreaAssetType", 0);
			}
			cmMCContent.pQuestionList[i].pPrompt = CmContentReaders.getPromptData(dataFile, i, imgSize, ActorName, "tsMCContent");
			List<CmAnswerItem> correctAnswers = CmContentReaders.getAnswerItems(dataFile, bCorrect: true, imgSize, ActorName, i);
			cmMCContent.pQuestionList[i].pDisplayArea = CmContentReaders.getDisplayArea(dataFile, i, ref correctAnswers, cmMCContent.pQuestionList[i].pPrompt, imgSize, ActorName);
			string value3 = dataFile.GetValue<string>(container, "SkillAddress", i);
			uint value4 = dataFile.GetValue<uint>(container, "SubLevelID", i);
			uint value5 = dataFile.GetValue<uint>(container, "Level", i);
			CmRK pRK = new CmRK(dataFile.GetValue<uint>(container, "BasicContentID", i), value3, value4, value5);
			cmMCContent.pQuestionList[i].pDisplayArea.pRK = pRK;
			if (dataFile.GetValue<uint>(container, "AnswerSequence", i) == 1)
			{
				cmMCContent.pQuestionList[i].pAnswerList = CmContentReaders.getMCContentTraditionalSeq(dataFile, i, imgSize, ActorName, value2);
			}
			else
			{
				cmMCContent.pQuestionList[i].pAnswerList = CmContentReaders.getManyAnswers(dataFile, i, correctAnswers, 1, numDistract, imgSize, ActorName, value2, bAllowDups: false, out var bWasDuped);
				if (bWasDuped)
				{
					cmMCContent.bDuped = true;
				}
			}
			cmMCContent.pQuestionList[i].pPrompt.pVisual = null;
		}
		return cmMCContent;
	}

	public static CmMCOneToOne GetMCContentOneToOne(UtData dataFile, int numPairs, string[] imgSize, string[] ActorName)
	{
		CmMCOneToOne cmMCOneToOne = new CmMCOneToOne();
		UtContainer container = dataFile.GetContainer("tsMCContent");
		int recordCount = dataFile.GetRecordCount(container);
		if (numPairs > recordCount)
		{
			numPairs = recordCount;
		}
		cmMCOneToOne.pPairList = new CmPair[numPairs];
		for (int i = 0; i < numPairs; i++)
		{
			UtData value = dataFile.GetValue<UtData>(container, "tsContentParams", i);
			string value2 = dataFile.GetValue<string>(value, "CPMatchContentType", 0);
			int value3 = dataFile.GetValue<int>(value, "CPMatchContentAssetType", 0);
			_ = dataFile.GetValue<int>(value, "CPAnswerAssetType", 0) & 8;
			List<CmAnswerItem> answerList = CmContentReaders.getAnswerItems(dataFile, bCorrect: true, imgSize, ActorName, i);
			cmMCOneToOne.pPairList[i] = new CmPair();
			cmMCOneToOne.pPairList[i].pPrompt = CmContentReaders.getPromptData(dataFile, i, imgSize, ActorName, "tsMCContent");
			CmDisplayArea baseItem;
			if (value2 == "MCBase")
			{
				baseItem = CmContentReaders.getBaseItem(dataFile, i, imgSize, ActorName, value3);
				cmMCOneToOne.pPairList[i].pMatchContent = new CmPairItem(baseItem);
			}
			else
			{
				baseItem = (CmPairItem)(object)CmContentReaders.getSingleAnswerItem(dataFile, ref answerList, imgSize, ActorName, value3);
				cmMCOneToOne.pPairList[i].pMatchContent = new CmPairItem(baseItem);
			}
			baseItem = CmContentReaders.getSingleAnswerItem(dataFile, ref answerList, imgSize, ActorName, value3);
			cmMCOneToOne.pPairList[i].pAnswer = new CmPairItem(baseItem);
			cmMCOneToOne.pPairList[i].pMatchContent.pPairID = i;
			cmMCOneToOne.pPairList[i].pAnswer.pPairID = i;
			cmMCOneToOne.pPairList[i].pPrompt.pVisual = null;
		}
		return cmMCOneToOne;
	}

	public static CmSeqContent GetSeqContent(UtData dataFile, int maxSeqLength, string[] imgSize, string[] ActorName)
	{
		CmSeqContent cmSeqContent = new CmSeqContent();
		UtContainer container = dataFile.GetContainer("tsSeqContent");
		int recordCount = dataFile.GetRecordCount(container);
		cmSeqContent.pSeqList = new CmSequence[recordCount];
		for (int i = 0; i < recordCount; i++)
		{
			cmSeqContent.pSeqList[i] = new CmSequence();
			cmSeqContent.pSeqList[i].pPrompt = CmContentReaders.getPromptData(dataFile, i, imgSize, ActorName, "tsSeqContent");
			cmSeqContent.pSeqList[i].pSeq = CmContentReaders.buildSequence(dataFile, i, maxSeqLength, imgSize, ActorName);
			cmSeqContent.pSeqList[i].pPrompt.pVisual = null;
		}
		return cmSeqContent;
	}

	public static CmBasicContent GetBasicContent(UtData dataFile, string[] imgSize, string[] ActorName, string[] Attribute)
	{
		CmBasicContent cmBasicContent = new CmBasicContent();
		UtContainer container = dataFile.GetContainer("tsBasicContent");
		int num = (cmBasicContent.pNumItems = dataFile.GetRecordCount(container));
		cmBasicContent.pBasicList = new CmBasicItem[num];
		for (int i = 0; i < num; i++)
		{
			string value = dataFile.GetValue<string>(container, "SkillAddress", i);
			uint value2 = dataFile.GetValue<uint>(container, "SubLevelID", i);
			uint value3 = dataFile.GetValue<uint>(container, "Level", i);
			CmRK pRK = new CmRK(dataFile.GetValue<uint>(container, "BasicContentID", i), value, value2, value3);
			CmBasicItem cmBasicItem = new CmBasicItem();
			cmBasicItem.pRK = pRK;
			cmBasicItem.pPrompt = CmContentReaders.getPromptData(dataFile, i, imgSize, ActorName, "tsBasicContent");
			UtData value4 = dataFile.GetValue<UtData>(container, "tsBasicParams", i);
			int recordCount = dataFile.GetRecordCount(value4);
			cmBasicItem.pParamList = new CmBasicDictionary();
			for (int j = 0; j < recordCount; j++)
			{
				string value5 = dataFile.GetValue<string>(value4, "Parameter", j);
				switch (dataFile.GetValue<string>(value4, "ValueType", j))
				{
				case "Number":
				{
					int value10 = dataFile.GetValue<int>(value4, "ValueNumber", j);
					cmBasicItem.pParamList.Add(value5, value10);
					break;
				}
				case "String":
				{
					string value9 = dataFile.GetValue<string>(value4, "ValueString", j);
					cmBasicItem.pParamList.Add(value5, value9);
					break;
				}
				case "Decimal":
				{
					double value8 = dataFile.GetValue<double>(value4, "ValueNumber", j);
					cmBasicItem.pParamList.Add(value5, value8);
					break;
				}
				case "CI":
				{
					CmAnswerItem value7 = CmContentReaders.readNestedContentItem(dataFile, i, j, imgSize, ActorName, Attribute);
					cmBasicItem.pParamList.Add(value5, value7);
					break;
				}
				case "CIList":
				case "CI List":
				{
					CmAnswerItem[] value6 = CmContentReaders.readNestedContentItemList(dataFile, i, j, imgSize, ActorName, Attribute);
					cmBasicItem.pParamList.Add(value5, value6);
					break;
				}
				}
			}
			cmBasicContent.pBasicList[i] = cmBasicItem;
		}
		return cmBasicContent;
	}
}
