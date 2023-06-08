using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Lab", Namespace = "")]
public class LabData
{
	public class LabXMLLoader
	{
		private XMLLoaderCallback mCallback;

		public void LoadXML(XMLLoaderCallback inCallback)
		{
			mCallback = inCallback;
			RsResourceManager.Load(GameConfig.GetKeyData("LabDataFile"), XMLDownloaded);
		}

		private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
		{
			switch (inEvent)
			{
			case RsResourceLoadEvent.COMPLETE:
				if (!pIsReady)
				{
					using StringReader textReader = new StringReader((string)inFile);
					mInstance = (LabData)new XmlSerializer(typeof(LabData)).Deserialize(textReader);
					if (mInstance != null)
					{
						mInstance.Initialize();
					}
				}
				if (mCallback != null)
				{
					mCallback(pIsReady);
				}
				break;
			case RsResourceLoadEvent.ERROR:
				if (mCallback != null)
				{
					mCallback(pIsReady);
				}
				break;
			}
		}
	}

	public delegate void XMLLoaderCallback(bool inSuccess);

	[XmlElement(ElementName = "RoomTemperature")]
	public float RoomTemperature;

	[XmlElement(ElementName = "LabItem")]
	public LabItem[] Items;

	[XmlElement(ElementName = "LabExperiment")]
	public Experiment[] Experiments;

	private static LabData mInstance;

	private Dictionary<string, List<LabItemCombination>> mCombinations;

	public static LabData pInstance
	{
		get
		{
			return mInstance;
		}
		set
		{
			mInstance = value;
		}
	}

	public static bool pIsReady => mInstance != null;

	[XmlIgnore]
	public Dictionary<string, List<LabItemCombination>> pCombinations => mCombinations;

	public static void Load(XMLLoaderCallback inCallback)
	{
		if (pIsReady)
		{
			inCallback(inSuccess: true);
		}
		else
		{
			new LabXMLLoader().LoadXML(inCallback);
		}
	}

	public Experiment GetLabExperimentByID(int inID)
	{
		if (Experiments == null || Experiments.Length == 0)
		{
			return null;
		}
		Experiment[] experiments = Experiments;
		foreach (Experiment experiment in experiments)
		{
			if (experiment != null && experiment.ID == inID)
			{
				return experiment;
			}
		}
		return null;
	}

	public LabItem GetItem(string inName)
	{
		if (Items == null || Items.Length == 0)
		{
			return null;
		}
		LabItem[] items = Items;
		foreach (LabItem labItem in items)
		{
			if (labItem.Name == inName)
			{
				return labItem;
			}
		}
		return null;
	}

	public void ResetStartTemperature()
	{
		if (Items == null || Items.Length == 0)
		{
			return;
		}
		LabItem[] items = Items;
		foreach (LabItem labItem in items)
		{
			if (labItem == null)
			{
				labItem.pStartTemperature = RoomTemperature;
			}
		}
	}

	public void Initialize()
	{
		InitializeCombination("DEFAULT");
		InitializeCombination("PESTLE");
		InitializeCombination("FEATHER");
	}

	private void InitializeCombination(string inAction)
	{
		if (Items == null || Items.Length == 0 || string.IsNullOrEmpty(inAction))
		{
			return;
		}
		LabItem[] items = Items;
		foreach (LabItem labItem in items)
		{
			if (labItem == null || labItem.Combinations == null || labItem.Combinations.Length == 0)
			{
				continue;
			}
			LabItemCombination[] combinations = labItem.Combinations;
			foreach (LabItemCombination labItemCombination in combinations)
			{
				if (labItemCombination == null || !(labItemCombination.Action == inAction) || labItemCombination.ItemNames == null || labItemCombination.ItemNames.Length == 0)
				{
					continue;
				}
				LabItemCombination labItemCombination2 = new LabItemCombination();
				labItemCombination2.ItemNames = new string[labItemCombination.ItemNames.Length + 1];
				labItemCombination.ItemNames.CopyTo(labItemCombination2.ItemNames, 0);
				labItemCombination2.ItemNames[labItemCombination.ItemNames.Length] = labItem.Name;
				labItemCombination2.ParticleData = labItemCombination.ParticleData;
				labItemCombination2.ResultItemName = labItemCombination.ResultItemName;
				labItemCombination2.Action = labItemCombination.Action;
				if (!IsExistInCombination(labItemCombination2.ItemNames, inAction))
				{
					if (mCombinations == null)
					{
						mCombinations = new Dictionary<string, List<LabItemCombination>>();
					}
					if (!mCombinations.ContainsKey(inAction))
					{
						mCombinations.Add(inAction, new List<LabItemCombination>());
					}
					mCombinations[inAction].Add(labItemCombination2);
				}
			}
		}
	}

	public List<LabItemCombination> GetCombinationList(List<LabTestObject> inItems, string inAction)
	{
		if (inItems == null || inItems.Count == 0 || mCombinations == null || mCombinations.Count == 0 || !mCombinations.ContainsKey(inAction))
		{
			return null;
		}
		List<LabItemCombination> list = mCombinations[inAction];
		if (list == null || list.Count == 0)
		{
			return null;
		}
		List<LabItemCombination> list2 = null;
		foreach (LabItemCombination item in list)
		{
			if (item == null)
			{
				continue;
			}
			string[] array = new string[inItems.Count];
			for (int i = 0; i < inItems.Count; i++)
			{
				if (inItems[i] != null)
				{
					array[i] = inItems[i].pTestItem.Name;
				}
			}
			if (item.IsExistInCombination(array))
			{
				if (list2 == null)
				{
					list2 = new List<LabItemCombination>();
				}
				list2.Add(item);
			}
		}
		return list2;
	}

	public LabItemCombination GetCombination(List<LabTestObject> inItems, string inAction)
	{
		if (inItems == null || inItems.Count == 0 || mCombinations == null || mCombinations.Count == 0 || !mCombinations.ContainsKey(inAction))
		{
			return null;
		}
		List<LabItemCombination> list = mCombinations[inAction];
		if (list == null || list.Count == 0)
		{
			return null;
		}
		foreach (LabItemCombination item in list)
		{
			if (item == null)
			{
				continue;
			}
			string[] array = new string[inItems.Count];
			for (int i = 0; i < inItems.Count; i++)
			{
				if (inItems[i] != null)
				{
					array[i] = inItems[i].pTestItem.Name;
				}
			}
			if (item.IsExistInCombination(array))
			{
				return item;
			}
		}
		return null;
	}

	public List<LabItemCombination> GetItemCombination(LabTestObject inTestObject, List<LabTestObject> inTestObjectList, string inAction)
	{
		if (inTestObject == null || inTestObject.pTestItem == null || inTestObjectList == null || inTestObjectList.Count == 0 || string.IsNullOrEmpty(inAction) || mCombinations == null || !mCombinations.ContainsKey(inAction))
		{
			return null;
		}
		List<LabItemCombination> list = null;
		List<LabItemCombination> list2 = mCombinations[inAction];
		if (list2 == null || list2.Count == 0)
		{
			return null;
		}
		foreach (LabTestObject inTestObject2 in inTestObjectList)
		{
			if (inTestObject2 == null || inTestObject == inTestObject2)
			{
				continue;
			}
			foreach (LabItemCombination item in list2)
			{
				if (item.Contains(inTestObject.pTestItem.Name) && item.IsExistInCombination(inTestObjectList))
				{
					if (list == null)
					{
						list = new List<LabItemCombination>();
					}
					list.Add(item);
				}
			}
		}
		return list;
	}

	public bool IsExistInCombination(string[] inItems, string inAction)
	{
		if (mCombinations == null || mCombinations.Count == 0 || !mCombinations.ContainsKey(inAction) || inItems == null || inItems.Length == 0 || string.IsNullOrEmpty(inAction))
		{
			return false;
		}
		List<LabItemCombination> list = mCombinations[inAction];
		if (list == null || list.Count == 0)
		{
			return false;
		}
		foreach (LabItemCombination item in list)
		{
			if (item != null && item.IsExistInCombination(inItems))
			{
				return true;
			}
		}
		return false;
	}
}
