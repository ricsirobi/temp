using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePetManager : MonoBehaviour
{
	private static RaisedPetData mCurPetData;

	public string _SanctuaryDataURL = "RS_DATA/PfSanctuaryData.unity3d/PfSanctuaryData";

	public Transform _PetStartMarker;

	public Vector3 _PetOffScreenPosition;

	public SanctuaryPet _CurPetInstance;

	public bool _DisablePetMoodParticles = true;

	public int _PairDataID = 1967;

	public float _DragonUiScale = 1.5f;

	public UiSelectProfile _ProfileSelectUi;

	public int _ObsCourseLevelPairDataID = 1000;

	public int _FSGameID = 14;

	private GetGameDataResponse mFlightSchoolGameData;

	private PairData mObsCourseLevelPairData;

	private PairData mObsCourseScorePairData;

	private int mCurrentPetType = -1;

	private bool mLoadData;

	private bool mCreateInstance;

	private bool mIsSanctuaryDataReady;

	private bool mIsRaisedPetDataReady;

	private bool mLoadSelectedRaisedPet;

	public static RaisedPetData pCurPetData => mCurPetData;

	public bool IsActivePetDataReady()
	{
		if (!mIsRaisedPetDataReady)
		{
			return false;
		}
		return RaisedPetData.IsActivePetDataReady(mCurrentPetType);
	}

	public void ResetData()
	{
		_CurPetInstance = null;
		if (_ProfileSelectUi != null)
		{
			_ProfileSelectUi.mIsPetLoadingDone = false;
		}
		if (mCurPetData != null)
		{
			if (mCurrentPetType == mCurPetData.PetTypeID)
			{
				mCurPetData = RaisedPetData.GetCurrentInstance(mCurrentPetType);
			}
			else
			{
				mLoadSelectedRaisedPet = true;
			}
		}
		else
		{
			mLoadSelectedRaisedPet = true;
		}
	}

	private void OnSanctuaryDataEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			string[] array = _SanctuaryDataURL.Split('/');
			RsResourceManager.SetDontDestroy(array[0] + "/" + array[1], inDontDestroy: false);
			mIsSanctuaryDataReady = true;
			UnityEngine.Object.Instantiate((GameObject)inObject);
		}
	}

	private void Start()
	{
		mIsSanctuaryDataReady = false;
		mLoadSelectedRaisedPet = true;
		if (SanctuaryData.pInstance == null && _SanctuaryDataURL.Length > 0)
		{
			string[] array = _SanctuaryDataURL.Split('/');
			if (array.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSanctuaryDataEventHandler, typeof(GameObject));
			}
		}
		else
		{
			mIsSanctuaryDataReady = true;
		}
		mCreateInstance = false;
		if (_ProfileSelectUi != null)
		{
			_ProfileSelectUi.mIsPetLoadingDone = false;
		}
	}

	public static SanctuaryPetItemDataLogin CreatePet(RaisedPetData pdata, Vector3 pos, Quaternion rot, GameObject msgObj)
	{
		SanctuaryPetItemDataLogin sanctuaryPetItemDataLogin = new SanctuaryPetItemDataLogin(pdata, pos, rot, msgObj);
		sanctuaryPetItemDataLogin.LoadResource();
		return sanctuaryPetItemDataLogin;
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if (mCurPetData == pet.pData)
		{
			_CurPetInstance = pet;
		}
		_CurPetInstance.SetMoodParticleIgnore(_DisablePetMoodParticles);
		ApplyPetPartColors(pet);
		if (_PetStartMarker != null)
		{
			pet.transform.position = _PetStartMarker.position;
			pet.transform.rotation = _PetStartMarker.rotation;
		}
		pet.pMeterPaused = true;
		if (_ProfileSelectUi != null)
		{
			_ProfileSelectUi.mIsPetLoadingDone = true;
			_ProfileSelectUi.mIsPetAvailable = true;
			if (_ProfileSelectUi._DragonStartMarkers != null)
			{
				DragonStartMarker dragonStartMarker = _ProfileSelectUi._DragonStartMarkers._Markers.Find((DragonStartMarker m) => m._TypeID == mCurPetData.PetTypeID);
				if (dragonStartMarker != null)
				{
					pet.transform.position = dragonStartMarker._Marker.position;
					pet.transform.rotation = dragonStartMarker._Marker.rotation;
				}
				else if (_ProfileSelectUi._DragonStartMarkers._DefaultMarker != null)
				{
					pet.transform.position = _ProfileSelectUi._DragonStartMarkers._DefaultMarker.position;
					pet.transform.rotation = _ProfileSelectUi._DragonStartMarkers._DefaultMarker.rotation;
				}
			}
		}
		pet.transform.localScale = Vector3.one * _DragonUiScale * pet.pCurAgeData._UiScale;
		pet.PlayPetMoodParticle(SanctuaryPetMeterType.HAPPINESS, isForcePlay: true);
	}

	private Renderer FindRenderer(SanctuaryPet pet)
	{
		Renderer componentInChildren = pet.transform.GetComponentInChildren<SkinnedMeshRenderer>();
		if (componentInChildren == null)
		{
			componentInChildren = pet.transform.GetComponentInChildren<MeshRenderer>();
		}
		return componentInChildren;
	}

	public void ApplyRaisedPetColorAttr(SanctuaryPet pet, string attrName, int matIdx, string attrParamName)
	{
		Renderer renderer = FindRenderer(pet);
		if (!(renderer == null) && pet.pData.FindAttrData(attrName) != null)
		{
			Color white = Color.white;
			if (renderer.materials[matIdx].HasProperty(attrParamName))
			{
				renderer.materials[matIdx].SetColor(attrParamName, white);
			}
		}
	}

	public void ApplyPetPartColors(SanctuaryPet pet)
	{
		ApplyRaisedPetColorAttr(pet, "_PetBodyColor", 0, "Color_Skin");
		ApplyRaisedPetColorAttr(pet, "_PetPatternColor", 0, "Color_Belly");
		Renderer renderer = FindRenderer(pet);
		if (!(renderer != null))
		{
			return;
		}
		RaisedPetAttribute raisedPetAttribute = pet.pData.FindAttrData("_PetPatternLuminosity");
		if (raisedPetAttribute != null)
		{
			float result = 0.5f;
			if (float.TryParse(raisedPetAttribute.Value, out result))
			{
				renderer.materials[0].SetFloat("Hue_Belly", result);
			}
		}
	}

	private bool CheckAllPetsLoaded()
	{
		RaisedPetData[] array = null;
		if (RaisedPetData.pActivePets.ContainsKey(mCurrentPetType))
		{
			array = RaisedPetData.pActivePets[mCurrentPetType];
		}
		if (array == null)
		{
			return true;
		}
		RaisedPetData[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].pObject == null)
			{
				return false;
			}
		}
		return true;
	}

	public void RaisedPetGetCallback(int ptype, RaisedPetData[] pdata, object inUserData)
	{
		mCurrentPetType = ptype;
		if (pdata != null)
		{
			foreach (RaisedPetData obj in pdata)
			{
				obj.Name = SanctuaryData.GetLocalizedPetName(obj);
				obj.DumpData();
			}
		}
		PetRankData.ResetCache();
		PetRankData.InitAchievementInfo(PetAchievementInfoReadyCallback);
	}

	public void PetAchievementInfoReadyCallback()
	{
		mIsRaisedPetDataReady = true;
	}

	private void Update()
	{
		if (!mIsSanctuaryDataReady)
		{
			return;
		}
		if (mLoadSelectedRaisedPet && UserInfo.pIsReady)
		{
			mLoadSelectedRaisedPet = false;
			mLoadData = true;
			RaisedPetData.Reset();
			RaisedPetData.GetAllActivePets(active: true, RaisedPetGetCallback, null);
			return;
		}
		if (mIsRaisedPetDataReady && mLoadData)
		{
			mLoadData = false;
			if (-1 == mCurrentPetType)
			{
				if (null != _ProfileSelectUi)
				{
					_ProfileSelectUi.mIsPetLoadingDone = true;
				}
				return;
			}
			mCurPetData = RaisedPetData.GetCurrentInstance(mCurrentPetType);
			if (mCurPetData != null)
			{
				mCreateInstance = true;
			}
			else if (null != _ProfileSelectUi)
			{
				_ProfileSelectUi.mIsPetLoadingDone = true;
			}
		}
		if (mCreateInstance)
		{
			if (IsActivePetDataReady())
			{
				mCreateInstance = false;
				CreatePet(mCurPetData, _PetOffScreenPosition, Quaternion.identity, base.gameObject);
			}
			else
			{
				UtDebug.Log(" !IsActivePetDataReady but mCreateInstance == true");
			}
		}
		else if (mObsCourseLevelPairData != null && mObsCourseScorePairData != null && mFlightSchoolGameData != null)
		{
			SaveToNewPairData();
			mObsCourseLevelPairData = null;
			mObsCourseScorePairData = null;
			mFlightSchoolGameData = null;
			_ProfileSelectUi.OnObsCoursePairDataSaveDone();
		}
	}

	public void UpdateObsCoursePairData()
	{
		mObsCourseLevelPairData = null;
		mObsCourseScorePairData = null;
		mFlightSchoolGameData = null;
		WsWebService.GetGameData(new GetGameDataRequest
		{
			GameID = _FSGameID,
			TopScoresOnly = true
		}, ServiceEventHandler, null);
		WsWebService.GetKeyValuePairByUserID(mCurPetData.EntityID.ToString(), _ObsCourseLevelPairDataID, ServiceEventHandler, null);
		PairData.Load(_ObsCourseLevelPairDataID, OnPairDataReady, null);
		if (_CurPetInstance != null)
		{
			ImageData.Init("EggColor", 512);
			SendPicture(_CurPetInstance.gameObject);
		}
	}

	public void OnPairDataReady(bool success, PairData pairData, object inUserData)
	{
		mObsCourseLevelPairData = pairData;
	}

	public void SaveToNewPairData()
	{
		if (mObsCourseScorePairData.KeyExists("FlightSchoolScoreData"))
		{
			return;
		}
		FlightSchoolScoreData flightSchoolScoreData = new FlightSchoolScoreData();
		flightSchoolScoreData.GlideModeScores = new List<int>();
		flightSchoolScoreData.FlightModeScores = new List<int>();
		foreach (GameDataSummary gameDataSummary in mFlightSchoolGameData.GameDataSummaryList)
		{
			if (gameDataSummary.Difficulty == 1)
			{
				for (int i = flightSchoolScoreData.GlideModeScores.Count; i < gameDataSummary.GameLevel; i++)
				{
					flightSchoolScoreData.GlideModeScores.Add(0);
				}
				flightSchoolScoreData.GlideModeScores[gameDataSummary.GameLevel - 1] = gameDataSummary.GameDataList[0].Value;
			}
			else if (gameDataSummary.Difficulty == 3)
			{
				for (int j = flightSchoolScoreData.FlightModeScores.Count; j < gameDataSummary.GameLevel; j++)
				{
					flightSchoolScoreData.FlightModeScores.Add(0);
				}
				flightSchoolScoreData.FlightModeScores[gameDataSummary.GameLevel - 1] = gameDataSummary.GameDataList[0].Value;
			}
		}
		string obsCoursePairDataForKey = GetObsCoursePairDataForKey("LastPlayedTime");
		if (string.IsNullOrEmpty(obsCoursePairDataForKey))
		{
			flightSchoolScoreData.GlideModeLastPlayedTime = ServerTime.pCurrentTime;
		}
		else
		{
			flightSchoolScoreData.GlideModeLastPlayedTime = DateTime.Parse(obsCoursePairDataForKey);
		}
		string obsCoursePairDataForKey2 = GetObsCoursePairDataForKey("LastUnLockedTeenLevel");
		if (string.IsNullOrEmpty(obsCoursePairDataForKey2))
		{
			flightSchoolScoreData.GlideModeLastUnlockedLevel = 0;
		}
		else
		{
			flightSchoolScoreData.GlideModeLastUnlockedLevel = int.Parse(obsCoursePairDataForKey2);
		}
		obsCoursePairDataForKey = GetObsCoursePairDataForKey("LastAdultPlayedTime");
		if (string.IsNullOrEmpty(obsCoursePairDataForKey))
		{
			flightSchoolScoreData.FlightModeLastPlayedTime = ServerTime.pCurrentTime;
		}
		else
		{
			flightSchoolScoreData.FlightModeLastPlayedTime = DateTime.Parse(obsCoursePairDataForKey);
		}
		obsCoursePairDataForKey2 = GetObsCoursePairDataForKey("LastUnLockedAdultLevel");
		if (string.IsNullOrEmpty(obsCoursePairDataForKey2))
		{
			flightSchoolScoreData.FlightModeLastUnlockedLevel = 0;
		}
		else
		{
			flightSchoolScoreData.FlightModeLastUnlockedLevel = int.Parse(obsCoursePairDataForKey2);
		}
		mObsCourseScorePairData.SetValue("FlightSchoolScoreData", UtUtilities.SerializeToXml(flightSchoolScoreData));
		mObsCourseScorePairData.PrepareArray();
		WsWebService.SetKeyValuePairByUserID(mCurPetData.EntityID.ToString(), _ObsCourseLevelPairDataID, mObsCourseScorePairData, ServiceEventHandler, null);
	}

	private string GetObsCoursePairDataForKey(string key)
	{
		string result = null;
		if (mObsCourseLevelPairData.KeyExists(key))
		{
			result = mObsCourseLevelPairData.GetValue(key);
			mObsCourseLevelPairData.RemoveByKey(key);
			WsWebService.DeleteKeyValuePairByKey(_ObsCourseLevelPairDataID, key, ServiceEventHandler, null);
		}
		return result;
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.GET_GAME_DATA:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mFlightSchoolGameData = (GetGameDataResponse)inObject;
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GET_GAME_DATA FAILED!!!");
				break;
			}
			break;
		case WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				if (inObject != null)
				{
					mObsCourseScorePairData = (PairData)inObject;
					mObsCourseScorePairData.Init();
				}
				else
				{
					mObsCourseScorePairData = new PairData();
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GET_GAME_DATA FAILED!!!");
				break;
			}
			break;
		}
	}

	public void SendPicture(GameObject inGameObject)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inGameObject);
		if (gameObject != null)
		{
			SanctuaryPet component = gameObject.GetComponent<SanctuaryPet>();
			component.SetMood(Character_Mood.firedup, t: false);
			component.animation.Play("PhotoPose");
			StartCoroutine(WaitForPicture(gameObject));
		}
	}

	private IEnumerator WaitForPicture(GameObject inGameObject)
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		SendPictureReal(inGameObject);
	}

	private void SendPictureReal(GameObject inGameObject)
	{
		SanctuaryPet component = inGameObject.GetComponent<SanctuaryPet>();
		if (component != null)
		{
			string headBoneName = component.GetHeadBoneName();
			if (!string.IsNullOrEmpty(headBoneName))
			{
				Transform transform = component.FindBoneTransform(headBoneName);
				if (transform != null)
				{
					AvPhotoManager avPhotoManager = AvPhotoManager.Init("PfPetPhotoMgr");
					Texture2D dstTexture = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
					avPhotoManager._HeadShotCamOffset = component.pCurAgeData._HUDPictureCameraOffset;
					avPhotoManager.TakeAShot(component.gameObject, ref dstTexture, transform);
					ImageData.Save("EggColor", mCurPetData.ImagePosition.Value, dstTexture);
					ImageData.UpdateImages("EggColor");
				}
				else
				{
					UtDebug.LogError("NO HEAD BONE FOUND!!!");
				}
			}
			else
			{
				UtDebug.LogError("NO HEAD BONE NAME PROVIDED!!");
			}
		}
		UnityEngine.Object.Destroy(inGameObject);
	}

	private void OnDestroy()
	{
		_ProfileSelectUi = null;
	}
}
