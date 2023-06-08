using System;
using System.Collections.Generic;
using System.Linq;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class WorldEventScoutAttack : WorldEventManager, IConsumable
{
	public float _AnnouncementMessageDuration = 3f;

	public float _WarnTime = 15f;

	public AudioClip _WarnSFX;

	public AudioClip _BeginSFX;

	public AudioClip _DefeatSFX;

	public AudioClip _WinSFX;

	public AudioClip _PlayerHealthZeroSFX;

	public AudioClip _EventBgMusic;

	public string _AmbMusicPool = "AmbMusic_Pool";

	public string _AmbMusicChannel = "AMB_Music";

	public KAWidget _AnnounceTxt;

	public LocaleString _EventStartedText = new LocaleString("A world event has started!");

	public LocaleString _ShipDestroyedText = new LocaleString("The enemy ship has been sunk!");

	public LocaleString _ShipEscapedText = new LocaleString("The boat has escaped!");

	public LocaleString _RewardsNotReceivedText = new LocaleString("Sorry! Unable to show rewards this time!");

	public AvAvatarStats _NormalRegenStats;

	public float[] _HealthReductionOnRespawn;

	public UiWorldEventConsumables _UiConsumable;

	public PowerUpManager _PowerUpManager;

	private UiWorldEventEndDB mEndResultDB;

	public string _RespawnAnimState = "KnockOut";

	public GameObject _RespawnParticles;

	public int _PointsOnTargetHit = 10;

	public int _PointsLostOnRespawn = 50;

	public int _GroupPointAchievementID;

	public float _DestroyShipAfter = 10f;

	public float _CameraShakeAmount = 0.17f;

	public float _CameraShakeDuration = 0.2f;

	public float _CameraShakeDecay = 1f;

	public float _EventEndMsgDuration = 3f;

	public float _HealthBarFlashDuration = 1f;

	public float _HealthBarFlashRate = 0.25f;

	public float _MinCountdownTimeToBlowhorn = 5f;

	public float _MaxWaitTimeForRewards = 30f;

	private DateTime mEndTime;

	private int mRespawnCount;

	private bool mDismountPet;

	private float mDismountTime;

	private const float mDelayInDismount = 0.15f;

	private AvAvatarController mAvatarController;

	private AvAvatarAnim mAvatarAnim;

	private float mCachedAvatarHealthRegenRate;

	private List<WorldEventShip> mActiveShipObjects = new List<WorldEventShip>();

	private int mPointOnHitByShip;

	private Action mAction;

	private Action mActionEndResult;

	private bool mEndResultDBLoading;

	private bool mWarnSoundPlayed;

	private float mEventEndMsgTimer;

	private AchievementReward[] mEventRewards;

	private string mEventRewardTierName;

	protected string[] mPlayersData;

	protected WorldEventAchievementRewardInfo[] mCurrentRewardInfo;

	private AudioClip mDefaultSceneMusic;

	private KAWidget mHealthbar;

	private float mLastFlash;

	private float mFlashTimer;

	private float mRewardWaitTimer;

	private KAUIGenericDB mKAUIGenericDB;

	private const string WEAPON_ATTACK = "WA";

	private int mToRewardCount;

	private bool pIsRewardsReady => mToRewardCount == 0;

	public void OnConsumableUpdated(Consumable inConsumable)
	{
		PowerUp powerUp = null;
		if (_PowerUpManager != null)
		{
			powerUp = _PowerUpManager.InitPowerUp(inConsumable.name, isMMO: false);
		}
		if (powerUp == null)
		{
			UtDebug.LogError("Couldn't find the power up :" + inConsumable.name);
		}
		else
		{
			powerUp.Activate();
		}
	}

	protected override void Start()
	{
		base.Start();
		_UiConsumable.SetGameData(this, "WorldEvent");
		if (AvAvatar.pObject != null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (_PowerUpManager != null)
		{
			_PowerUpManager.Init(this, new PowerUpHelper());
		}
		ShowAnnouncementMessage("", show: false);
	}

	protected override void InitializeMMO()
	{
		base.InitializeMMO();
		if (mMMOClient != null)
		{
			mMMOClient.AddListener("WEF_", FlareUpdate);
			MainStreetMMOClient.pInstance.AddExtensionResponseEventHandler("WA", ResponseEventHandler);
		}
	}

	protected override void ShowMessage(LocaleString message)
	{
		mAction = (Action)Delegate.Combine(mAction, (Action)delegate
		{
			ShowAnnouncementMessage(message.GetLocalizedString(), show: true);
		});
	}

	public override void OnEventBegin(WorldEvent we)
	{
		mDefaultSceneMusic = SnChannel.FindChannel(_AmbMusicPool, _AmbMusicChannel).pClip;
		SnChannel.Play(_EventBgMusic, _AmbMusicPool, inForce: true);
		mEventWon = false;
		mRewardWaitTimer = 0f;
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarHealth = (AvAvatarController.OnAvatarHealthDelegate)Delegate.Combine(avAvatarController.OnAvatarHealth, new AvAvatarController.OnAvatarHealthDelegate(PlayerHealthOnAmmoHit));
		mWarnSoundPlayed = false;
		mEndTime = we.mEndTime;
		base.OnEventBegin(we);
		mCachedAvatarHealthRegenRate = mAvatarController._Stats._HealthRegenRate;
		mAvatarController._Stats._HealthRegenRate = _NormalRegenStats._HealthRegenRate;
		mAction = (Action)Delegate.Combine(mAction, (Action)delegate
		{
			_UiConsumable.gameObject.SetActive(value: true);
			_UiConsumable.pIsEventOn = true;
		});
		AvAvatar.pLevelState = AvAvatarLevelState.WORLDEVENT;
		WorldEventTarget worldEventTarget = mWorldEventTarget;
		worldEventTarget.OnHit = (WorldEventTarget.OnWorldEventTargetHit)Delegate.Combine(worldEventTarget.OnHit, new WorldEventTarget.OnWorldEventTargetHit(OnTargetHit));
		Flare.OnHit += OnTargetHit;
		mPlayerScore = 0;
		mPlayerParticipated = false;
		SnChannel.Play(_BeginSFX);
		mEventRewards = null;
		mToRewardCount = 0;
		ShowAnnouncementMessage(_EventStartedText.GetLocalizedString(), show: true);
	}

	public override void OnEventEnd(WorldEvent we, bool destroyObject, string[] scores)
	{
		UtDebug.Log("On event end : " + ((scores != null) ? scores.Length.ToString() : "null"));
		for (int i = 0; i < mActiveShipObjects.Count; i++)
		{
			WorldEventShip worldEventShip = mActiveShipObjects[i];
			if (worldEventShip != null)
			{
				worldEventShip.DeactivateWeapons();
			}
		}
		mActiveShipObjects.Clear();
		base.OnEventEnd(we, destroyObject, scores);
		mAvatarController._Stats._HealthRegenRate = mCachedAvatarHealthRegenRate;
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarHealth = (AvAvatarController.OnAvatarHealthDelegate)Delegate.Remove(avAvatarController.OnAvatarHealth, new AvAvatarController.OnAvatarHealthDelegate(PlayerHealthOnAmmoHit));
		mRespawnCount = 0;
		_UiConsumable.SetVisibility(inVisible: false);
		_UiConsumable.pIsEventOn = false;
		if (mHealthbar != null)
		{
			mHealthbar.SetVisibility(inVisible: true);
		}
		bool eventWon = mEventWon;
		if (mPlayerParticipated)
		{
			mActionEndResult = (Action)Delegate.Combine(mActionEndResult, (Action)delegate
			{
				PopulateScore(mPlayersData, eventWon);
				mEventEndMsgTimer = _EventEndMsgDuration;
				mEndResultDB._EventCompleteUi.SetVisibility(inVisible: true);
				if (_DefeatSFX != null && _WinSFX != null)
				{
					SnChannel.Play(eventWon ? _WinSFX : _DefeatSFX);
				}
				mEndResultDB.ShowEventCompletedMsg(eventWon);
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
			});
		}
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		WorldEventTarget worldEventTarget = mWorldEventTarget;
		worldEventTarget.OnHit = (WorldEventTarget.OnWorldEventTargetHit)Delegate.Remove(worldEventTarget.OnHit, new WorldEventTarget.OnWorldEventTargetHit(OnTargetHit));
		Flare.OnHit -= OnTargetHit;
		mAvatarController.pCanRegenHealth = true;
	}

	protected override void OnEventEndFromServer(bool isEventWon, string scoreString, string objectString)
	{
		SnChannel.Play(mDefaultSceneMusic, _AmbMusicPool, inForce: true);
		string localizedString = (isEventWon ? _ShipDestroyedText : _ShipEscapedText).GetLocalizedString();
		ShowAnnouncementMessage(localizedString, show: true);
		base.OnEventEndFromServer(isEventWon, scoreString, objectString);
		if (isEventWon)
		{
			if (mWorldEventTarget != null)
			{
				mWorldEventTarget.Kill();
			}
			mPreviousObjectExitTimer = _DestroyShipAfter;
			UtDebug.Log("Event Won!!!");
		}
	}

	protected override void DestroyEventObjects(WorldEvent we)
	{
		base.DestroyEventObjects(we);
		mEventWon = false;
		mActiveShipObjects.Clear();
	}

	private void ShowAnnouncementMessage(string inMessage, bool show)
	{
		if (_AnnounceTxt != null)
		{
			_AnnounceTxt.SetText(inMessage);
			_AnnounceTxt.SetVisibility(show);
		}
		Invoke("HideAnnouncement", _AnnouncementMessageDuration);
	}

	private void HideAnnouncement()
	{
		if (_AnnounceTxt != null)
		{
			_AnnounceTxt.SetVisibility(inVisible: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mWarnSoundPlayed && null != MMOTimeManager.pInstance && MMOTimeManager.pInstance.pIsTimeSynced && Math.Floor((float)(mEndTime - MMOTimeManager.pInstance.GetServerDateTimeMilliseconds()).TotalSeconds) == (double)_WarnTime)
		{
			SnChannel.Play(_WarnSFX);
			mWarnSoundPlayed = true;
		}
		if (mDismountPet && Time.realtimeSinceStartup > mDismountTime)
		{
			DismountPet();
			mDismountPet = false;
		}
		if (mAction != null && AvAvatar.pToolbar.activeInHierarchy && KAUI._GlobalExclusiveUI == null && (AdRewardManager.pInstance == null || !AdRewardManager.pInstance.pBusy))
		{
			mAction();
			mAction = null;
		}
		if (mActionEndResult != null && AvAvatar.pToolbar.activeInHierarchy && KAUI._GlobalExclusiveUI == null && (AdRewardManager.pInstance == null || !AdRewardManager.pInstance.pBusy))
		{
			if (mEndResultDB == null && !mEndResultDBLoading)
			{
				LoadResultUI();
			}
			else if (mEndResultDB != null && mEndResultDB.pInitialized)
			{
				mActionEndResult();
				mActionEndResult = null;
			}
		}
		if (mEventEndMsgTimer > 0f)
		{
			mEventEndMsgTimer -= Time.deltaTime;
		}
		if (mRewardWaitTimer > 0f)
		{
			if (pIsRewardsReady && mEventEndMsgTimer <= 0f)
			{
				ShowResultsScreen();
			}
			else
			{
				mRewardWaitTimer -= Time.deltaTime;
				if (mRewardWaitTimer < 0f)
				{
					UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
					ShowErrorDB(_RewardsNotReceivedText.GetLocalizedString());
					mEndResultDB._EventCompleteUi.SetVisibility(inVisible: false);
				}
			}
		}
		if (mFlashTimer <= _HealthBarFlashDuration)
		{
			if (Time.time - mLastFlash >= _HealthBarFlashRate)
			{
				if (mHealthbar != null)
				{
					mHealthbar.SetVisibility(!mHealthbar.GetVisibility());
				}
				mLastFlash = Time.time;
				mFlashTimer += _HealthBarFlashRate;
			}
		}
		else
		{
			mFlashTimer = float.MaxValue;
			mLastFlash = 0f;
			if (mHealthbar != null)
			{
				mHealthbar.SetVisibility(inVisible: true);
			}
		}
	}

	private void ShowErrorDB(string errorText)
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		if (mKAUIGenericDB != null)
		{
			mKAUIGenericDB.SetMessage(base.gameObject, string.Empty, string.Empty, "CloseErrorDB", string.Empty);
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mKAUIGenericDB.SetText(errorText, interactive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	private void CloseErrorDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
		mKAUIGenericDB = null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mAvatarController._Stats._HealthRegenRate = mCachedAvatarHealthRegenRate;
		AvAvatarController avAvatarController = mAvatarController;
		avAvatarController.OnAvatarHealth = (AvAvatarController.OnAvatarHealthDelegate)Delegate.Remove(avAvatarController.OnAvatarHealth, new AvAvatarController.OnAvatarHealthDelegate(PlayerHealthOnAmmoHit));
		mAvatarController.pCanRegenHealth = true;
		if (mMMOClient != null)
		{
			mMMOClient.RemoveListener("WEF_");
			MainStreetMMOClient.pInstance.RemoveExtensionResponseEventHandler("WA", ResponseEventHandler);
		}
	}

	private void PlayerHealthOnAmmoHit(float currentHealth)
	{
		if ((SanctuaryManager.pCurPetInstance == null && currentHealth <= 0f) || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HEALTH) <= SanctuaryManager.pCurPetInstance._MinPetMeterValue))
		{
			if (_UiConsumable._UIPowerUpBuyPopUp.GetVisibility())
			{
				_UiConsumable.SendMessage("OnCloseUI");
			}
			OnPlayerHealthZero();
		}
		if (null != AvAvatar.pAvatarCam)
		{
			CaAvatarCam component = AvAvatar.pAvatarCam.GetComponent<CaAvatarCam>();
			if (null != component)
			{
				component.Shake(_CameraShakeAmount, _CameraShakeDuration, _CameraShakeDecay);
			}
		}
		mPointOnHitByShip++;
		if (mPointOnHitByShip == 3)
		{
			mPlayerScore += mPointOnHitByShip;
			mPointOnHitByShip = 0;
			SubmitPlayerScore();
		}
		if (null == mHealthbar)
		{
			UiToolbar component2 = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (null != component2)
			{
				mHealthbar = component2.FindItem("MeterBarHPs");
			}
		}
		if (null != mHealthbar && mLastFlash <= 0f)
		{
			mFlashTimer = 0f;
		}
	}

	private void OnPlayerHealthZero()
	{
		SnChannel.Play(_PlayerHealthZeroSFX);
		mAvatarController.pCanRegenHealth = false;
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			SanctuaryManager.pCurPetInstance.OnFlyLanding(AvAvatar.pObject);
			mDismountPet = true;
			mDismountTime = Time.realtimeSinceStartup + 0.15f;
		}
		else
		{
			if (mAvatarController != null && mAvatarController.pIsPlayerGliding)
			{
				mAvatarController.OnGlideLanding();
			}
			else
			{
				AvAvatar.pState = AvAvatarState.IDLE;
				AvAvatar.pSubState = AvAvatarSubState.NORMAL;
			}
			ShowRespawnEffects();
		}
		AvAvatar.pInputEnabled = false;
		mPlayerScore -= _PointsLostOnRespawn;
		if (mPlayerScore < 0)
		{
			mPlayerScore = 0;
		}
		SubmitPlayerScore();
	}

	private void OnRespawnAnimEnd(string animName)
	{
		if (!(animName != _RespawnAnimState))
		{
			int num = ((mRespawnCount < _HealthReductionOnRespawn.Length) ? mRespawnCount : (_HealthReductionOnRespawn.Length - 1));
			if (SanctuaryManager.pCurPetInstance != null)
			{
				float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.HEALTH, SanctuaryManager.pCurPetInstance.pData);
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, _HealthReductionOnRespawn[num] * maxMeter);
			}
			else
			{
				mAvatarController._Stats._CurrentHealth = _HealthReductionOnRespawn[num] * mAvatarController._Stats._MaxHealth;
			}
			mRespawnCount++;
			mAvatarController.pCanRegenHealth = true;
			AvAvatar.pInputEnabled = true;
		}
	}

	private void DismountPet()
	{
		SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
		ShowRespawnEffects();
	}

	private void ShowRespawnEffects()
	{
		if (mAvatarAnim == null)
		{
			mAvatarAnim = AvAvatar.pObject.GetComponentInChildren<AvAvatarAnim>();
		}
		if (mAvatarAnim == null)
		{
			UtDebug.Log("AvAvatar Anim not found in PfAvatar");
			OnRespawnAnimEnd(_RespawnAnimState);
			return;
		}
		mAvatarAnim.PlayCannedAnim(_RespawnAnimState, bQuitOnMovement: false, bQuitOnEnd: true, bFreezePlayer: false, OnRespawnAnimEnd);
		if (UtPlatform.IsWSA())
		{
			UtUtilities.ReAssignShader(UnityEngine.Object.Instantiate(_RespawnParticles, AvAvatar.pObject.transform.position, AvAvatar.pObject.transform.rotation));
		}
		else
		{
			UnityEngine.Object.Instantiate(_RespawnParticles, AvAvatar.pObject.transform.position, AvAvatar.pObject.transform.rotation);
		}
	}

	private void OnTargetHit(ObAmmo inAmmo, int? pointsOnHit = null)
	{
		if (inAmmo.pCreator == SanctuaryManager.pCurPetInstance.transform && mWorldEvent != null)
		{
			mPlayerScore += (pointsOnHit.HasValue ? pointsOnHit.Value : _PointsOnTargetHit);
			SubmitPlayerScore();
			mPlayerParticipated = true;
		}
	}

	private void SubmitPlayerScore()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("id", "ScoutAttack");
		dictionary.Add("ScoreData", AvatarData.pInstance.DisplayName + "/" + mPlayerScore);
		MainStreetMMOClient.pInstance.SendExtensionMessage("wex.PS", dictionary);
	}

	protected override void OnPlayersScoreReady(string[] playersData)
	{
		base.OnPlayersScoreReady(playersData);
		mPlayersData = playersData;
		if (mParticipatedEventObjs.Count > 0)
		{
			mCurrentRewardInfo = mParticipatedEventObjs[0]._RewardInfo;
		}
	}

	private void PopulateScore(string[] playersData, bool eventWon)
	{
		if (!mPlayerParticipated)
		{
			return;
		}
		int num = -1;
		mEndResultDB.ClearScores();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (playersData != null && playersData.Length != 0)
		{
			for (int i = 0; i < playersData.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(playersData[i]))
				{
					string[] array = playersData[i].Split('/');
					int value = int.Parse(array[1]);
					dictionary.Add(array[0], value);
				}
			}
		}
		int num2 = 1;
		if (mCurrentRewardInfo == null)
		{
			return;
		}
		float num3 = mCurrentRewardInfo.Length - 1;
		foreach (KeyValuePair<string, int> item in dictionary.OrderByDescending((KeyValuePair<string, int> key) => key.Value))
		{
			int num4 = -1;
			string rewardName = "";
			if (eventWon)
			{
				float num5 = num3 / (float)dictionary.Count;
				num4 = ((!((float)dictionary.Count < num3)) ? Mathf.Clamp(Mathf.CeilToInt((float)num2 * num5), 0, (int)num3) : num2);
				rewardName = mCurrentRewardInfo[num4]._RewardNameText.GetLocalizedString();
			}
			else
			{
				num4 = 0;
			}
			mEndResultDB.AddPlayerScore(num2++, rewardName, item.Key, item.Value);
			if (item.Key == AvatarData.pInstance.DisplayName)
			{
				num = num4;
			}
		}
		if (mCurrentRewardInfo.Length != 0 && num >= 0)
		{
			string text = "";
			WorldEventAchievementRewardInfo worldEventAchievementRewardInfo = mCurrentRewardInfo[num];
			mEventRewardTierName = worldEventAchievementRewardInfo._RewardNameText.GetLocalizedString();
			mRewardWaitTimer = _MaxWaitTimeForRewards;
			mToRewardCount = 1;
			if (AvatarData.pInstance._Group != null)
			{
				text = AvatarData.pInstance._Group.GroupID;
				if (num == 1)
				{
					mToRewardCount++;
					WsWebService.SetAchievementAndGetReward(_GroupPointAchievementID, text, ServiceEventHandler, false);
				}
			}
			mEndResultDB._UiRewards.pAdRewardAchievementID = worldEventAchievementRewardInfo._AdRewardAchievementID;
			WsWebService.SetAchievementAndGetReward(worldEventAchievementRewardInfo._AchievementID, "", ServiceEventHandler, true);
		}
		else if (playersData != null && dictionary != null)
		{
			UtDebug.Log("Fail to get rewards!!! Player count : " + playersData.Length + " score count : " + dictionary.Count + " Reward tier : " + num);
		}
		else
		{
			UtDebug.Log("Fail to get rewards!!! Player data or score data NULL. Reward tier : " + num);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				if ((bool)inUserData)
				{
					mEventRewards = (AchievementReward[])inObject;
				}
				mToRewardCount--;
			}
			break;
		case WsServiceEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatar.pPrevState;
			UtDebug.LogError("Getting achievement reward for  World Event Scout Attack failed");
			break;
		}
	}

	private void ShowResultsScreen()
	{
		mEndResultDB._EventCompleteUi.SetVisibility(inVisible: false);
		mEndResultDB.Show(mEventRewards, mEventRewardTierName, mEventWon);
		mRewardWaitTimer = 0f;
	}

	private void LoadResultUI()
	{
		mEndResultDBLoading = true;
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		string[] array = GameConfig.GetKeyData("WorldEventResultUI").Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], ResultUiEventHandler, typeof(GameObject));
	}

	private void ResultUiEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = "PfUiWorldEventEndDB";
			mEndResultDB = gameObject.GetComponent<UiWorldEventEndDB>();
			KAUICursorManager.SetDefaultCursor("Arrow");
			mEndResultDBLoading = false;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("World Event End DB not laoded!");
			mEndResultDB = null;
			mActionEndResult = null;
			KAUICursorManager.SetDefaultCursor("Arrow");
			mEndResultDBLoading = false;
			break;
		}
	}

	public void SetRoomVariable(int weaponID, string targetMMOID)
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			string key = "WE_Weapon_" + weaponID;
			string value = weaponID + "," + targetMMOID + "," + UserInfo.pInstance.UserID;
			MainStreetMMOClient.pInstance.SetRoomVariable(key, value);
		}
	}

	private void FlareUpdate(string name, string value)
	{
		string[] array = name.Split(',');
		if (array.Length < 2)
		{
			return;
		}
		string text = array[0].Substring("WEF_".Length);
		int num = -1;
		for (int i = 0; i < mActiveShipObjects.Count; i++)
		{
			if (mActiveShipObjects[i].pShipUID.Equals(text))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			GameObject activeLiveObject = GetActiveLiveObject(text);
			if (activeLiveObject == null)
			{
				return;
			}
			mActiveShipObjects.Add(activeLiveObject.GetComponent<WorldEventShip>());
			num = mActiveShipObjects.Count - 1;
		}
		int result = 0;
		int result2 = 0;
		if (int.TryParse(array[1], out result) && int.TryParse(array[2], out result2))
		{
			mActiveShipObjects[num]._Weapons[result].UpdateWeaponData(result2, value);
		}
	}

	protected override void ResponseEventHandler(object sender, MMOExtensionResponseReceivedEventArgs args)
	{
		if (args == null || sender == null)
		{
			return;
		}
		base.ResponseEventHandler(sender, args);
		Dictionary<string, object> responseDataObject = args.ResponseDataObject;
		if (responseDataObject == null || !responseDataObject.ContainsKey("0") || !(responseDataObject["0"].ToString() == "WA") || responseDataObject.Count < 5 || responseDataObject["1"].ToString().Equals(MainStreetMMOClient.pInstance.GetMMOUserName()))
		{
			return;
		}
		string s = responseDataObject["2"].ToString();
		string avatarID = responseDataObject["3"].ToString();
		string text = responseDataObject["4"].ToString();
		int num = -1;
		for (int i = 0; i < mActiveShipObjects.Count; i++)
		{
			if (!string.IsNullOrEmpty(mActiveShipObjects[i].pShipUID) && mActiveShipObjects[i].pShipUID.Equals(text))
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			GameObject activeLiveObject = GetActiveLiveObject(text);
			if (activeLiveObject == null)
			{
				return;
			}
			mActiveShipObjects.Add(activeLiveObject.GetComponent<WorldEventShip>());
			num = mActiveShipObjects.Count - 1;
		}
		int result = 0;
		if (int.TryParse(s, out result) && num < mActiveShipObjects.Count)
		{
			mActiveShipObjects[num].Fire(result, avatarID);
		}
	}

	protected override void InstantiateEventObjects(WorldEvent we, float inCountDownTime)
	{
		base.InstantiateEventObjects(we, inCountDownTime);
		if (inCountDownTime > 0f && inCountDownTime >= _MinCountdownTimeToBlowhorn)
		{
			SnChannel.Play(_BeginSFX);
		}
	}
}
