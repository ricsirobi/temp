using System.Collections.Generic;
using KA.Framework;
using Newtonsoft.Json;
using SerializableDictionary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GiftManager : MonoBehaviour
{
	[SerializeField]
	private string m_InitSceneName = "ProfileSelectionDO";

	private const string GIFT_KEY = "Gift";

	public const string READ_KEY = "Read";

	public const string CLAIM_KEY = "Claim";

	private PairData mProfilePairData;

	public List<GiftData> _Gifts = new List<GiftData>();

	private UserNotifyGiftManager mUNGiftManager;

	private bool mInitialized;

	private static GiftManager mInstance;

	public static bool pIsReady { get; private set; }

	public static GiftManager pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Object.FindObjectOfType<GiftManager>();
				if (mInstance == null)
				{
					GameObject obj = new GameObject
					{
						name = "PfGiftManager"
					};
					mInstance = obj.AddComponent<GiftManager>();
					Object.DontDestroyOnLoad(obj);
				}
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public void Init(UserNotifyGiftManager unGiftManager)
	{
		mUNGiftManager = unGiftManager;
		if (mInitialized)
		{
			mUNGiftManager.OnValidationComplete();
			return;
		}
		mInitialized = true;
		PairData.Load(ProductSettings.pInstance._PairDataID, LocalPairDataEventHandler, null);
	}

	public void Reset()
	{
		mInitialized = false;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (newScene.name == m_InitSceneName)
		{
			Reset();
		}
	}

	private void LocalPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		mProfilePairData = pData;
		if (GiftContainAccountOnly())
		{
			ParentData.pInstance.LoadPairData(ProductSettings.pInstance._PairDataID, ParentPairDataEventHandler);
			return;
		}
		pIsReady = true;
		InitGifts();
	}

	private void ParentPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		pIsReady = true;
		InitGifts();
	}

	public void InitGifts()
	{
		if (_Gifts == null || _Gifts.Count == 0)
		{
			mUNGiftManager.OnValidationComplete();
			return;
		}
		foreach (GiftData gift in _Gifts)
		{
			if (!gift.InActive)
			{
				gift.ValidateGiftExpiry();
				if (!GetMessageTag(gift, "Read") && gift.ValidatePrerequisites())
				{
					SendGiftMessage(gift);
				}
			}
		}
		mUNGiftManager.OnValidationComplete();
	}

	public void SendGiftMessage(GiftData gift, WsServiceEventHandler inCallback = null)
	{
		if (inCallback == null)
		{
			inCallback = ServiceEventHandler;
		}
		if (gift.AchievementID != 0)
		{
			SerializableDictionary.KeyValuePair<string, string> keyValuePair = new SerializableDictionary.KeyValuePair<string, string>();
			keyValuePair.Add("name", gift.GiftName);
			WsWebService.SendMessage(UserInfo.pInstance.UserID, gift.MessageID, keyValuePair, inCallback, gift);
		}
	}

	public bool GiftContainAccountOnly()
	{
		if (_Gifts == null || _Gifts.Count == 0)
		{
			return false;
		}
		return _Gifts.Exists((GiftData gift) => gift.AccountOnly);
	}

	public GiftData GetGiftDataByAchievementID(int achievementID)
	{
		if (_Gifts == null || _Gifts.Count == 0)
		{
			return null;
		}
		return _Gifts.Find((GiftData gift) => gift.AchievementID == achievementID);
	}

	public GiftData GetGiftDataByMessageID(int messageID)
	{
		if (_Gifts == null || _Gifts.Count == 0)
		{
			return null;
		}
		return _Gifts.Find((GiftData gift) => gift.MessageID == messageID && !gift.InActive);
	}

	public GiftData GetGiftDataByName(string giftName)
	{
		if (_Gifts == null || _Gifts.Count == 0)
		{
			return null;
		}
		return _Gifts.Find((GiftData gift) => gift.GiftName == giftName);
	}

	public GiftData GetGiftDataByIndex(int index)
	{
		if (_Gifts == null || _Gifts.Count == 0 || _Gifts.Count <= index)
		{
			return null;
		}
		return _Gifts[index];
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inUserData != null)
		{
			GiftData giftData = (GiftData)inUserData;
			UpdateMessageTag(giftData, "Read");
		}
	}

	public void UpdateMessageTag(GiftData giftData, string key)
	{
		PairData pairData = mProfilePairData;
		string stringValue = pairData.GetStringValue("Gift" + key, string.Empty);
		GiftMessageData giftMessageData = null;
		if (string.IsNullOrEmpty(stringValue))
		{
			giftMessageData = new GiftMessageData();
			giftMessageData.MessageTagData.Add(giftData.GiftName);
		}
		else
		{
			giftMessageData = JsonConvert.DeserializeObject<GiftMessageData>(stringValue);
			if (!giftMessageData.MessageTagData.Contains(giftData.GiftName))
			{
				giftMessageData.MessageTagData.Add(giftData.GiftName);
			}
		}
		string inValue = JsonConvert.SerializeObject(giftMessageData);
		if (giftData.AccountOnly)
		{
			ParentData.pInstance.UpdatePairData(ProductSettings.pInstance._PairDataID, "Gift" + key, inValue);
		}
		else
		{
			pairData.SetValue("Gift" + key, inValue);
		}
		Save(giftData.AccountOnly);
	}

	public bool GetMessageTag(GiftData giftData, string key)
	{
		string stringValue = mProfilePairData.GetStringValue("Gift" + key, string.Empty);
		if (giftData.AccountOnly)
		{
			stringValue = ParentData.pInstance.GetPairDataByID(ProductSettings.pInstance._PairDataID).GetStringValue("Gift" + key, string.Empty);
		}
		if (stringValue == string.Empty)
		{
			return false;
		}
		if (JsonConvert.DeserializeObject<GiftMessageData>(stringValue).MessageTagData.Contains(giftData.GiftName))
		{
			return true;
		}
		return false;
	}

	public bool RemoveMessageTag(GiftData giftData, string key)
	{
		if (mProfilePairData == null)
		{
			return false;
		}
		PairData pairDataByID = mProfilePairData;
		if (giftData.AccountOnly)
		{
			pairDataByID = ParentData.pInstance.GetPairDataByID(ProductSettings.pInstance._PairDataID);
		}
		string stringValue = pairDataByID.GetStringValue("Gift" + key, string.Empty);
		if (!string.IsNullOrEmpty(stringValue))
		{
			GiftMessageData giftMessageData = JsonConvert.DeserializeObject<GiftMessageData>(stringValue);
			if (giftMessageData.MessageTagData.Contains(giftData.GiftName))
			{
				giftMessageData.MessageTagData.Remove(giftData.GiftName);
				string inValue = JsonConvert.SerializeObject(giftMessageData);
				if (giftData.AccountOnly)
				{
					ParentData.pInstance.UpdatePairData(ProductSettings.pInstance._PairDataID, "Gift" + key, inValue);
				}
				else
				{
					pairDataByID.SetValue("Gift" + key, inValue);
				}
				return true;
			}
		}
		return false;
	}

	[ContextMenu("ClearPairData")]
	public void ClearPairData()
	{
		if (pIsReady)
		{
			string inValue = JsonConvert.SerializeObject(new GiftMessageData());
			mProfilePairData.SetValue("GiftRead", inValue);
			mProfilePairData.SetValue("GiftClaim", inValue);
			Save(accountOnly: false);
			ParentData.pInstance.GetPairDataByID(ProductSettings.pInstance._PairDataID).SetValue("GiftRead", inValue);
			ParentData.pInstance.GetPairDataByID(ProductSettings.pInstance._PairDataID).SetValue("GiftClaim", inValue);
			Save(accountOnly: true);
		}
	}

	public void Save()
	{
		mProfilePairData.PrepareArray();
		mProfilePairData.SaveAs(ProductSettings.pInstance._PairDataID);
	}

	public void Save(bool accountOnly)
	{
		if (accountOnly)
		{
			ParentData.pInstance.SavePairData(ProductSettings.pInstance._PairDataID);
		}
		else
		{
			Save();
		}
	}
}
