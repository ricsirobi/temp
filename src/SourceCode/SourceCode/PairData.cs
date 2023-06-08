using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "Pairs", Namespace = "", IsNullable = false)]
public class PairData
{
	[XmlElement(ElementName = "Pair")]
	public Pair[] Pairs;

	public const string VALUE_NOT_DEFINED = "___VALUE_NOT_FOUND___";

	public const string LIST_NOT_VALID = "LIST_NOT_VALID";

	private static Dictionary<int, PairDataInstance> mPairData = new Dictionary<int, PairDataInstance>();

	[XmlIgnore]
	public int _DataID = -1;

	[XmlIgnore]
	public bool _IsDirty;

	private List<Pair> mPairList;

	[XmlIgnore]
	public List<Pair> pPairList
	{
		get
		{
			return mPairList;
		}
		set
		{
			mPairList = value;
		}
	}

	public PairData()
	{
		mPairList = null;
		Pairs = null;
		_IsDirty = true;
	}

	public static bool IsReady(int dataID)
	{
		if (mPairData.ContainsKey(dataID))
		{
			return mPairData[dataID]._DataReady;
		}
		return false;
	}

	public static PairDataInstance GetPairDataInstanceByID(int pairID)
	{
		PairDataInstance result = null;
		if (mPairData.ContainsKey(pairID))
		{
			result = mPairData[pairID];
		}
		return result;
	}

	public static void Reset()
	{
		mPairData = new Dictionary<int, PairDataInstance>();
	}

	public static void Load(int dataID, PairDataEventHandler callback, object inUserData, bool forceLoad = false, string userID = null)
	{
		PairDataInstance pairDataInstance = null;
		if (mPairData.ContainsKey(dataID) && (!forceLoad || mPairData[dataID].pIsLoading))
		{
			pairDataInstance = mPairData[dataID];
		}
		else
		{
			pairDataInstance = new PairDataInstance();
			mPairData[dataID] = pairDataInstance;
		}
		pairDataInstance.Load(dataID, callback, inUserData, userID);
	}

	public static void Save(int dataID, string userID = null)
	{
		if (mPairData.ContainsKey(dataID))
		{
			PairDataInstance pairDataInstance = mPairData[dataID];
			if (pairDataInstance._DataReady)
			{
				pairDataInstance.Save(userID);
			}
			else
			{
				UtDebug.LogError("PairData[" + dataID + "] not ready for save yet.");
			}
		}
		else
		{
			UtDebug.LogError("PairData[" + dataID + "] does not exist.");
		}
	}

	public static void Save(int dataID, PairDataEventHandler inCallback, object inUserData = null, string userID = null)
	{
		if (mPairData.ContainsKey(dataID))
		{
			PairDataInstance pairDataInstance = mPairData[dataID];
			if (pairDataInstance._DataReady)
			{
				pairDataInstance.Save(userID, inCallback, inUserData);
				return;
			}
			UtDebug.LogError("PairData[" + dataID + "] not ready for save yet.");
			inCallback?.Invoke(success: false, null, inUserData);
		}
		else
		{
			inCallback?.Invoke(success: false, null, inUserData);
			UtDebug.LogError("PairData[" + dataID + "] does not exist.");
		}
	}

	public static void DeleteData(int dataID)
	{
		if (mPairData.ContainsKey(dataID))
		{
			PairDataInstance pairDataInstance = new PairDataInstance();
			pairDataInstance._Data = new PairData();
			pairDataInstance._Data.Init();
			pairDataInstance._DataID = dataID;
			mPairData[dataID] = pairDataInstance;
		}
		WsWebService.DeleteKeyValuePair(dataID, null, null);
	}

	public void Init()
	{
		if (Pairs == null)
		{
			mPairList = new List<Pair>();
		}
		else
		{
			mPairList = new List<Pair>(Pairs);
		}
	}

	public void SaveAs(int dataID)
	{
		PairDataInstance pairDataInstance = null;
		if (mPairData.ContainsKey(dataID))
		{
			pairDataInstance = mPairData[dataID];
		}
		else
		{
			pairDataInstance = new PairDataInstance();
			pairDataInstance._DataID = dataID;
			mPairData[dataID] = pairDataInstance;
		}
		_IsDirty = true;
		pairDataInstance._Data = this;
		pairDataInstance._DataReady = true;
		pairDataInstance.Save();
	}

	public void PrepareArray()
	{
		Pairs = mPairList.ToArray();
	}

	public int GetIntValue(string inKey, int defaultVal)
	{
		Pair pair = FindByKey(inKey);
		if (pair == null)
		{
			return defaultVal;
		}
		return int.Parse(pair.PairValue);
	}

	public float GetFloatValue(string inKey, float defaultVal)
	{
		Pair pair = FindByKey(inKey);
		if (pair == null)
		{
			return defaultVal;
		}
		return float.Parse(pair.PairValue);
	}

	public bool GetBoolValue(string inKey, bool defaultVal)
	{
		Pair pair = FindByKey(inKey);
		if (pair == null)
		{
			return defaultVal;
		}
		return bool.Parse(pair.PairValue);
	}

	public string GetStringValue(string inKey, string defaultVal)
	{
		Pair pair = FindByKey(inKey);
		if (pair == null)
		{
			return defaultVal;
		}
		return pair.PairValue;
	}

	public string GetValue(string inKey)
	{
		if (mPairList == null)
		{
			return "LIST_NOT_VALID";
		}
		foreach (Pair mPair in mPairList)
		{
			if (mPair.PairKey == inKey)
			{
				return mPair.PairValue;
			}
		}
		return "___VALUE_NOT_FOUND___";
	}

	public void SetValue(string inKey, string inValue)
	{
		if (mPairList == null)
		{
			return;
		}
		foreach (Pair mPair in mPairList)
		{
			if (mPair.PairKey == inKey)
			{
				if (mPair.PairValue != inValue)
				{
					_IsDirty = true;
					mPair.PairValue = inValue;
				}
				return;
			}
		}
		AddNewItem(inKey, inValue);
	}

	public void SetValueAndSave(string inKey, string inValue)
	{
		if (mPairList != null && GetValue(inKey) != inValue)
		{
			SetValue(inKey, inValue);
			PairData pairData = new PairData();
			pairData.Init();
			pairData.AddNewItem(inKey, inValue);
			pairData.PrepareArray();
			WsWebService.SetKeyValuePair(_DataID, pairData, WsSetEventHandler, null);
		}
	}

	public bool KeyExists(string inKey)
	{
		if (mPairList == null)
		{
			return false;
		}
		foreach (Pair mPair in mPairList)
		{
			if (mPair.PairKey == inKey)
			{
				return true;
			}
		}
		return false;
	}

	public Pair FindByKey(string inKey)
	{
		if (mPairList == null)
		{
			return null;
		}
		foreach (Pair mPair in mPairList)
		{
			if (mPair.PairKey == inKey)
			{
				return mPair;
			}
		}
		return null;
	}

	public bool RemoveByKey(string inKey)
	{
		Pair pair = FindByKey(inKey);
		if (pair != null)
		{
			_IsDirty = true;
			mPairList.Remove(pair);
			return true;
		}
		return false;
	}

	private void AddNewItem(string inKey, string inValue)
	{
		Pair pair = new Pair();
		pair.PairKey = inKey;
		pair.PairValue = inValue;
		mPairList.Add(pair);
		_IsDirty = true;
	}

	public void WsSetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL SetPairData FAILED!!!");
			break;
		case WsServiceEvent.COMPLETE:
			_IsDirty = false;
			break;
		}
	}
}
