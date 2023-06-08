using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiRewards : KAUI
{
	[Serializable]
	public class RewardInfo
	{
		public int _TypeID;

		public int _RewardItemID;

		public Reward3D _Reward3D;

		public Reward2D _Reward2D;

		public AudioClip _SFX;

		public int _PointsPerParticle;

		public int _MaxParticleCount;

		public Vector3 _StartPosition = Vector2.zero;

		public Transform _End;

		public Vector3 _EndPosition = Vector2.zero;

		public Vector2 _PositionOffset = Vector2.zero;

		public RewardCounter _Counter;

		[NonSerialized]
		public Vector2 _DynamicStartPosition = Vector2.zero;
	}

	[Serializable]
	public class Reward3D
	{
		public GameObject _PrtObject;

		public string _PlaceholderObjectName;
	}

	[Serializable]
	public class Reward2D
	{
		[Serializable]
		public enum Type
		{
			SHOW,
			FLASH,
			SCALE,
			BLEND
		}

		public Type _Type;

		public string _RewardTemplateItem;

		public string _TxtAmount;

		public string _IconReward;

		public int _Lifetime;

		public Vector3 _Scale;

		public float _FlashInterval;

		public int _FlashCount;

		public Color _BlendColorStart;

		public Color _BlendColorFinal;
	}

	[Serializable]
	public class RewardCounter
	{
		public enum Type
		{
			COUNTER,
			SCALE
		}

		public enum ScaleState
		{
			NONE,
			SCALEUP,
			SCALEDOWN
		}

		public Type _Type;

		public string _CounterUI;

		public string _CounterItem;

		public string _CounterAmountItem;

		public float _Duration;

		public Vector2 _MinScale = new Vector2(0.01f, 0.01f);

		public Vector2 _MaxScale;

		public bool _UseAvatarPosition;

		public Vector2 _PositionOffset;

		public bool _UpdateRewardAfterPrtEffect;

		private AchievementReward mReward;

		private float mTimer;

		private KAWidget mCounter;

		private KAWidget mCounterAmount;

		private bool mCounterWasVisible = true;

		private int mAmount;

		private int mPrevAmount;

		private int mRewardPrtCount;

		private bool mCanUpdateCounter;

		private ScaleState mScaleState;

		private Vector3 mMinScale = Vector3.zero;

		private Vector3 mMaxScale = Vector3.zero;

		private int mPointType;

		private int mPrevUpdateAmount;

		public void Show(int inTotalAmount, int deltaAmount, int rewardPrtCount, KAUI inInterface, int pointType)
		{
			if (string.IsNullOrEmpty(_CounterItem))
			{
				return;
			}
			if (!string.IsNullOrEmpty(_CounterUI))
			{
				string[] array = _CounterUI.Split('/');
				if (array.Length == 2)
				{
					GameObject gameObject = GameObject.Find(array[0]);
					if (gameObject != null)
					{
						KAUI kAUI = gameObject.GetComponent(array[1]) as KAUI;
						if (kAUI != null)
						{
							inInterface = kAUI;
						}
					}
				}
				else if (array[0] == "UiToolbar" && AvAvatar.pToolbar != null)
				{
					inInterface = AvAvatar.pToolbar.GetComponent<UiToolbar>();
				}
			}
			if (mCounter == null && inInterface != null)
			{
				mCounter = inInterface.FindItem(_CounterItem);
				if (mCounter != null)
				{
					mCounterWasVisible = mCounter.GetVisibility();
					if (!string.IsNullOrEmpty(_CounterAmountItem))
					{
						mCounterAmount = inInterface.FindItem(_CounterAmountItem);
					}
				}
				mMinScale = new Vector3(_MinScale.x, _MinScale.y, 0f);
				mMaxScale = new Vector3(_MaxScale.x, _MaxScale.y, 0f);
			}
			mPointType = pointType;
			mRewardPrtCount += rewardPrtCount;
			mAmount += deltaAmount;
			mPrevAmount = inTotalAmount - mAmount;
			if (_Type == Type.SCALE)
			{
				mPrevAmount = inTotalAmount;
				if (mScaleState == ScaleState.NONE)
				{
					SetScaleTo(mCounter, mMinScale, mMinScale, 0f);
				}
			}
			if (_UseAvatarPosition && AvAvatar.pObject != null)
			{
				Vector3 center = AvAvatar.pObject.GetComponent<Collider>().bounds.center;
				center = Camera.main.WorldToScreenPoint(center);
				center.y = (float)Screen.height - center.y;
				center += new Vector3(_PositionOffset.x, _PositionOffset.y, 0f);
				if (mCounter != null)
				{
					mCounter.SetPosition(center.x, center.y);
				}
			}
			if (mCounterAmount != null)
			{
				if (mPointType == 2)
				{
					mCounterAmount.SetText(Money.pGameCurrency.ToString());
				}
				else if (mPointType == 5)
				{
					mCounterAmount.SetText(Money.pCashCurrency.ToString());
				}
				else
				{
					mCounterAmount.SetText(mPrevAmount.ToString());
				}
			}
			if (mCounter != null)
			{
				mCounter.SetVisibility(inVisible: true);
			}
		}

		public void OnRewardPrtDone()
		{
			mRewardPrtCount--;
			if (_UpdateRewardAfterPrtEffect)
			{
				if (mRewardPrtCount <= 0)
				{
					mCanUpdateCounter = true;
				}
			}
			else if (!mCanUpdateCounter)
			{
				mCanUpdateCounter = true;
			}
		}

		public void Update(GameObject inMessageObject)
		{
			if (!mCanUpdateCounter || !(_Duration > 0f))
			{
				return;
			}
			if (_Type == Type.COUNTER)
			{
				int num = Mathf.FloorToInt((float)mAmount * (mTimer / _Duration));
				if (num > mAmount)
				{
					num = mAmount;
				}
				if (mPointType == 2)
				{
					num = Mathf.Min(num, mAmount);
					Money.AddToGameCurrency(num - mPrevUpdateAmount);
					if (mCounterAmount != null)
					{
						mCounterAmount.SetText(Money.pGameCurrency.ToString());
					}
					mPrevUpdateAmount = num;
				}
				else if (mPointType == 5)
				{
					Money.AddToCashCurrency(num - mPrevUpdateAmount);
					if (mCounterAmount != null)
					{
						mCounterAmount.SetText(Money.pCashCurrency.ToString());
					}
					mPrevUpdateAmount = num;
				}
				else if (mCounterAmount != null)
				{
					mCounterAmount.SetText((mPrevAmount + num).ToString());
				}
			}
			else if (_Type == Type.SCALE)
			{
				if (mScaleState == ScaleState.NONE)
				{
					SetScaleTo(mCounter, mMinScale, mMaxScale, _Duration / 2f);
					mScaleState = ScaleState.SCALEUP;
				}
				else if (mScaleState == ScaleState.SCALEUP && mTimer > _Duration / 2f)
				{
					SetScaleTo(mCounter, mMaxScale, mMinScale, _Duration / 2f);
					mScaleState = ScaleState.SCALEDOWN;
				}
			}
			if (mTimer >= _Duration)
			{
				if (mCounter != null)
				{
					mCounter.SetVisibility(mCounterWasVisible);
				}
				if (inMessageObject != null)
				{
					inMessageObject.SendMessage("RewardUpdated", SendMessageOptions.DontRequireReceiver);
				}
				Reset();
			}
			else
			{
				mTimer += Time.deltaTime;
			}
		}

		private void SetScaleTo(KAWidget inItem, Vector3 start, Vector3 end, float duration)
		{
			if (inItem != null)
			{
				inItem.ScaleTo(start, end, duration);
				for (int i = 0; i < inItem.GetNumChildren(); i++)
				{
					inItem.FindChildItemAt(i).ScaleTo(start, end, duration);
				}
			}
		}

		private void Reset()
		{
			mCanUpdateCounter = false;
			mAmount = 0;
			mTimer = 0f;
			mScaleState = ScaleState.NONE;
			mPrevUpdateAmount = 0;
		}
	}

	public class RewardsItemDataLoader
	{
		private KAWidget mItem;

		private GameObject mObject;

		private bool mIsReady;

		public bool pIsReady => mIsReady;

		public RewardsItemDataLoader(int itemID, KAWidget item, GameObject inObject)
		{
			mItem = item;
			mObject = inObject;
			ItemData.Load(itemID, ItemDataEventHandler, null);
		}

		private void ItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
		{
			if (mItem != null && !string.IsNullOrEmpty(dataItem.IconName))
			{
				string[] array = dataItem.IconName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], RewardItemEventHandler, typeof(Texture));
			}
			if (mObject != null && !string.IsNullOrEmpty(dataItem.AssetName))
			{
				string[] array2 = dataItem.AssetName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array2[0] + "/" + array2[1], array2[2], RewardItemEventHandler, typeof(GameObject));
			}
		}

		private void RewardItemEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			switch (inEvent)
			{
			case RsResourceLoadEvent.COMPLETE:
				mIsReady = true;
				if (inObject == null)
				{
					break;
				}
				if (mItem != null)
				{
					mItem.SetTexture((Texture)inObject);
				}
				if (mObject != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
					Collider component = gameObject.GetComponent<Collider>();
					if (component != null)
					{
						component.enabled = false;
					}
					Rigidbody component2 = gameObject.GetComponent<Rigidbody>();
					if (component2 != null)
					{
						component2.isKinematic = true;
					}
					ObBouncyCoin component3 = gameObject.GetComponent<ObBouncyCoin>();
					if (component3 != null)
					{
						component3.enabled = false;
					}
					ObCollect component4 = gameObject.GetComponent<ObCollect>();
					if (component4 != null)
					{
						component4.enabled = false;
					}
					ObPrefabLoader component5 = gameObject.GetComponent<ObPrefabLoader>();
					if (component5 != null)
					{
						component5.enabled = false;
					}
					gameObject.transform.position = mObject.transform.position;
					gameObject.transform.rotation = mObject.transform.rotation;
					gameObject.SendMessage("ApplyViewInfo", "Reward", SendMessageOptions.DontRequireReceiver);
					gameObject.transform.parent = mObject.transform.parent;
					UnityEngine.Object.Destroy(mObject);
				}
				break;
			case RsResourceLoadEvent.ERROR:
				mIsReady = true;
				break;
			}
		}
	}

	public class RewardItemLoadContainer
	{
		public GameObject _Reward3D;

		public KAWidget _Reward2D;

		public RewardInfo _RewardInfo;

		public RewardItemLoadContainer(GameObject reward3D, KAWidget reward2D, RewardInfo rewardInfo)
		{
			_Reward3D = reward3D;
			_Reward2D = reward2D;
			_RewardInfo = rewardInfo;
		}
	}

	public RewardInfo[] _Rewards;

	private Dictionary<KAWidget, float> mReward2D = new Dictionary<KAWidget, float>();

	private List<GameObject> mReward3D = new List<GameObject>();

	private Dictionary<RewardsItemDataLoader, RewardItemLoadContainer> mRewardItemLoading = new Dictionary<RewardsItemDataLoader, RewardItemLoadContainer>();

	private List<AchievementReward> mPendingRewards = new List<AchievementReward>();

	private bool mIsAvatarCamDisabled;

	private GameObject mMessageObject;

	private static bool mForceShowRewards;

	public static bool pForceShowRewards
	{
		get
		{
			return mForceShowRewards;
		}
		set
		{
			mForceShowRewards = value;
		}
	}

	private bool pCanShowRewards
	{
		get
		{
			if (!mForceShowRewards)
			{
				if (!RsResourceManager.pLevelLoadingScreen && AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pInputEnabled && AvAvatar.pToolbar != null)
				{
					return AvAvatar.pToolbar.activeInHierarchy;
				}
				return false;
			}
			return true;
		}
	}

	public virtual void SetDynamicStartPosition(List<KeyValuePair<int, Vector2>> inList)
	{
		foreach (KeyValuePair<int, Vector2> @in in inList)
		{
			int key = @in.Key;
			RewardInfo[] rewards = _Rewards;
			foreach (RewardInfo rewardInfo in rewards)
			{
				if (rewardInfo._TypeID == key)
				{
					rewardInfo._DynamicStartPosition = @in.Value;
					break;
				}
			}
		}
	}

	public virtual IEnumerator DisplayRewards(AchievementReward[] rewards)
	{
		if (rewards == null || rewards.Length == 0)
		{
			yield break;
		}
		yield return new WaitForEndOfFrame();
		if (RewardManager.pMoneyToDeduct > 0)
		{
			Money.AddToGameCurrency(RewardManager.pMoneyToDeduct * -1);
			RewardManager.pMoneyToDeduct = 0;
		}
		foreach (AchievementReward achievementReward in rewards)
		{
			int num = -1;
			for (int j = 0; j < _Rewards.Length; j++)
			{
				if (achievementReward.PointTypeID == _Rewards[j]._TypeID)
				{
					if (achievementReward.ItemID == 0 || achievementReward.ItemID == _Rewards[j]._RewardItemID)
					{
						num = j;
						break;
					}
					if (_Rewards[j]._RewardItemID == 0)
					{
						num = j;
					}
				}
			}
			if (num < 0)
			{
				continue;
			}
			int amount = GetAmount(achievementReward);
			if (amount != -1)
			{
				DisplayReward(num, achievementReward.Amount.Value, achievementReward.ItemID, amount);
				if (mPendingRewards.Contains(achievementReward))
				{
					mPendingRewards.Remove(achievementReward);
				}
			}
			else if (!mPendingRewards.Contains(achievementReward))
			{
				mPendingRewards.Add(achievementReward);
			}
		}
	}

	protected virtual void DisplayReward(int rewardIndex, int amount, int itemID, int currentAmount)
	{
		if (rewardIndex < 0 || rewardIndex >= _Rewards.Length)
		{
			return;
		}
		RewardInfo rewardInfo = _Rewards[rewardIndex];
		int num = Mathf.Clamp(amount / rewardInfo._PointsPerParticle, 1, rewardInfo._MaxParticleCount);
		if (rewardInfo._SFX != null)
		{
			SnChannel.Play(rewardInfo._SFX, "SFX_Pool", 0, inForce: true);
		}
		if (rewardInfo._Counter != null)
		{
			rewardInfo._Counter.Show(currentAmount, amount, num, this, rewardInfo._TypeID);
		}
		float num2 = 360f / (float)num;
		float num3 = 0f;
		Vector3 position = rewardInfo._StartPosition;
		if (rewardInfo._DynamicStartPosition != Vector2.zero)
		{
			position = new Vector3(rewardInfo._DynamicStartPosition.x, rewardInfo._DynamicStartPosition.y, rewardInfo._StartPosition.z);
			rewardInfo._DynamicStartPosition = Vector2.zero;
		}
		for (int i = 0; i < num; i++)
		{
			if (rewardInfo._Reward3D != null && rewardInfo._Reward3D._PrtObject != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(rewardInfo._Reward3D._PrtObject);
				if (gameObject != null)
				{
					mReward3D.Add(gameObject);
					gameObject.name = rewardIndex.ToString();
					if (itemID > 0 && !string.IsNullOrEmpty(rewardInfo._Reward3D._PlaceholderObjectName))
					{
						Transform transform = gameObject.transform.Find(rewardInfo._Reward3D._PlaceholderObjectName);
						RewardsItemDataLoader key = new RewardsItemDataLoader(itemID, null, transform.gameObject);
						RewardItemLoadContainer value = new RewardItemLoadContainer(gameObject, null, null);
						mRewardItemLoading.Add(key, value);
					}
					ObMotionBezier component = gameObject.GetComponent<ObMotionBezier>();
					if (component != null)
					{
						float num4 = UnityEngine.Random.Range(-1f * rewardInfo._PositionOffset.x, rewardInfo._PositionOffset.x);
						float num5 = UnityEngine.Random.Range(-1f * rewardInfo._PositionOffset.y, rewardInfo._PositionOffset.y);
						float num6 = (float)Screen.width / 1024f;
						float num7 = (float)Screen.height / 768f;
						Vector3 vector = KAUIManager.pInstance.camera.WorldToScreenPoint(position) + new Vector3(num4 * num6, num5 * num7, 0f);
						vector.z = position.z;
						Vector3 position2 = rewardInfo._EndPosition;
						if (rewardInfo._End != null)
						{
							position2 = rewardInfo._End.position;
						}
						position2 = KAUIManager.pInstance.camera.WorldToScreenPoint(position2);
						position2.z = rewardInfo._EndPosition.z;
						Vector3 position3 = vector + (position2 - vector) / 2f;
						vector = Camera.main.ScreenToWorldPoint(vector);
						position2 = Camera.main.ScreenToWorldPoint(position2);
						position3 = Camera.main.ScreenToWorldPoint(position3);
						vector = Camera.main.transform.InverseTransformPoint(vector);
						position2 = Camera.main.transform.InverseTransformPoint(position2);
						position3 = Camera.main.transform.InverseTransformPoint(position3);
						gameObject.transform.position = vector;
						gameObject.transform.parent = Camera.main.transform;
						Vector3 position4 = Camera.main.transform.position;
						position4.y = vector.y;
						gameObject.transform.LookAt(position4);
						if (!GetVisibility())
						{
							gameObject.SetActive(value: false);
						}
						ObFading component2 = gameObject.GetComponent<ObFading>();
						if (component2 != null)
						{
							component2._AlphaEnd = 0f;
							component2._AlphaStart = 1f;
						}
						component._MoveOnLocalAxis = true;
						component._MessageObject = base.gameObject;
						component.SetEndPoints(vector, position2, position3);
					}
					ObOrbit component3 = gameObject.GetComponent<ObOrbit>();
					if (component3 != null)
					{
						float num8 = UnityEngine.Random.Range(-1f * rewardInfo._PositionOffset.x, rewardInfo._PositionOffset.x);
						Vector3 position5 = AvAvatar.position;
						position5 += new Vector3(Mathf.Sin(num3 * (MathF.PI / 180f)), 0f, Mathf.Cos(num3 * (MathF.PI / 180f)));
						num3 += num2;
						gameObject.transform.position = position5;
						component3._OffsetAngle = num3;
						component3._MessageObject = base.gameObject;
						component3._ReduceRadiusWithTime = true;
						if (num8 > 0f)
						{
							component3._RevolveReverese = true;
						}
						else
						{
							component3._IncreaseHeightWithTime = true;
						}
						component3.pTarget = AvAvatar.mTransform;
					}
				}
			}
			if (rewardInfo._Reward2D == null)
			{
				continue;
			}
			Reward2D reward2D = rewardInfo._Reward2D;
			KAWidget kAWidget = DuplicateWidget(reward2D._RewardTemplateItem);
			if (kAWidget != null)
			{
				AddWidget(kAWidget);
				KAWidget kAWidget2 = null;
				if (!string.IsNullOrEmpty(reward2D._TxtAmount))
				{
					kAWidget2 = kAWidget.FindChildItem(reward2D._TxtAmount);
				}
				KAWidget kAWidget3 = null;
				if (!string.IsNullOrEmpty(reward2D._IconReward))
				{
					kAWidget3 = kAWidget.FindChildItem(reward2D._IconReward);
				}
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(amount.ToString());
				}
				if (itemID >= 0 && kAWidget3 != null)
				{
					RewardsItemDataLoader key2 = new RewardsItemDataLoader(itemID, kAWidget3, null);
					RewardItemLoadContainer value2 = new RewardItemLoadContainer(null, kAWidget, rewardInfo);
					mRewardItemLoading.Add(key2, value2);
				}
				else
				{
					ApplyReward2dEffects(kAWidget, rewardInfo);
				}
			}
		}
	}

	private void ApplyReward2dEffects(KAWidget rewardItem, RewardInfo rewardInfo)
	{
		Reward2D reward2D = rewardInfo._Reward2D;
		mReward2D.Add(rewardItem, reward2D._Lifetime);
		rewardItem.SetVisibility(inVisible: true);
		KAWidget kAWidget = null;
		if (!string.IsNullOrEmpty(reward2D._TxtAmount))
		{
			kAWidget = rewardItem.FindChildItem(reward2D._TxtAmount);
		}
		KAWidget kAWidget2 = null;
		if (!string.IsNullOrEmpty(reward2D._IconReward))
		{
			kAWidget2 = rewardItem.FindChildItem(reward2D._IconReward);
		}
		switch (reward2D._Type)
		{
		case Reward2D.Type.BLEND:
			rewardItem.ColorBlendTo(reward2D._BlendColorStart, reward2D._BlendColorFinal, reward2D._Lifetime);
			if (kAWidget != null)
			{
				kAWidget.ColorBlendTo(reward2D._BlendColorStart, reward2D._BlendColorFinal, reward2D._Lifetime);
			}
			if (kAWidget2 != null)
			{
				kAWidget2.ColorBlendTo(reward2D._BlendColorStart, reward2D._BlendColorFinal, reward2D._Lifetime);
			}
			break;
		case Reward2D.Type.SCALE:
		{
			Vector2 vector = rewardItem.GetScale();
			rewardItem.ScaleTo(new Vector3(vector.x, vector.y, 0f), reward2D._Scale, reward2D._Lifetime);
			if (kAWidget != null)
			{
				vector = kAWidget.GetScale();
				kAWidget.ScaleTo(new Vector3(vector.x, vector.y, 0f), reward2D._Scale, reward2D._Lifetime);
			}
			if (kAWidget2 != null)
			{
				vector = kAWidget2.GetScale();
				kAWidget2.ScaleTo(new Vector3(vector.x, vector.y, 0f), reward2D._Scale, reward2D._Lifetime);
			}
			break;
		}
		}
		Vector2 vector2 = new Vector2(rewardInfo._StartPosition.x, rewardInfo._StartPosition.y);
		Vector2 end = new Vector2(rewardInfo._EndPosition.x, rewardInfo._EndPosition.y);
		if (rewardInfo._End != null)
		{
			end = rewardInfo._End.position;
		}
		float x = UnityEngine.Random.Range(0f - rewardInfo._PositionOffset.x, rewardInfo._PositionOffset.x);
		float y = UnityEngine.Random.Range(0f - rewardInfo._PositionOffset.y, rewardInfo._PositionOffset.y);
		vector2 += new Vector2(x, y);
		end += new Vector2(x, y);
		rewardItem.SetPosition(vector2.x, vector2.y);
		rewardItem.MoveTo(end, reward2D._Lifetime);
	}

	protected virtual void OnBezierMotionDone(GameObject inObject)
	{
		if (inObject != null)
		{
			int result = 0;
			if (int.TryParse(inObject.name, out result) && result >= 0 && result < _Rewards.Length && _Rewards[result]._Counter != null)
			{
				_Rewards[result]._Counter.OnRewardPrtDone();
			}
			UnityEngine.Object.Destroy(inObject);
			mReward3D.Remove(inObject);
		}
	}

	protected virtual void OnOrbitMotionDone(GameObject inObject)
	{
		if (inObject != null)
		{
			int result = 0;
			if (int.TryParse(inObject.name, out result) && result >= 0 && result < _Rewards.Length && _Rewards[result]._Counter != null)
			{
				_Rewards[result]._Counter.OnRewardPrtDone();
			}
			UnityEngine.Object.Destroy(inObject);
			mReward3D.Remove(inObject);
		}
	}

	protected virtual int GetAmount(AchievementReward reward)
	{
		switch (reward.PointTypeID.Value)
		{
		case 2:
			if (Money.pIsReady)
			{
				return Money.pGameCurrency;
			}
			break;
		case 5:
			if (Money.pIsReady)
			{
				return Money.pCashCurrency;
			}
			break;
		case 1:
			if (UserRankData.pIsReady)
			{
				return UserRankData.pInstance.AchievementPointTotal.Value;
			}
			break;
		case 6:
			if (CommonInventoryData.pIsReady)
			{
				return CommonInventoryData.pInstance.GetQuantity(reward.ItemID);
			}
			break;
		}
		return -1;
	}

	protected virtual void HideRewards(bool hide)
	{
		SetVisibility(!hide);
		foreach (GameObject item in mReward3D)
		{
			item.SetActive(!hide);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (AvAvatar.pAvatarCam != null)
		{
			if (!mIsAvatarCamDisabled && !AvAvatar.pAvatarCam.activeInHierarchy)
			{
				mIsAvatarCamDisabled = true;
			}
			else if (mIsAvatarCamDisabled && AvAvatar.pAvatarCam.activeInHierarchy)
			{
				mIsAvatarCamDisabled = false;
				if (!GetVisibility())
				{
					HideRewards(hide: true);
				}
			}
		}
		if (!pCanShowRewards)
		{
			if (GetVisibility())
			{
				HideRewards(hide: true);
			}
			return;
		}
		if (!GetVisibility())
		{
			HideRewards(hide: false);
		}
		if (!GetVisibility())
		{
			return;
		}
		if (mReward2D.Count > 0)
		{
			List<KAWidget> list = new List<KAWidget>(mReward2D.Keys);
			foreach (KAWidget item in list)
			{
				float num = mReward2D[item] - Time.deltaTime;
				if (num <= 0f)
				{
					RemoveWidget(item);
					mReward2D.Remove(item);
				}
				else
				{
					mReward2D[item] = num;
				}
			}
			list.Clear();
			list = null;
		}
		RewardInfo[] rewards = _Rewards;
		foreach (RewardInfo rewardInfo in rewards)
		{
			if (rewardInfo._Counter != null)
			{
				rewardInfo._Counter.Update(mMessageObject);
			}
		}
		List<RewardsItemDataLoader> list2 = new List<RewardsItemDataLoader>();
		foreach (KeyValuePair<RewardsItemDataLoader, RewardItemLoadContainer> item2 in mRewardItemLoading)
		{
			if (item2.Key.pIsReady)
			{
				if (pCanShowRewards)
				{
					if (item2.Value._Reward3D != null)
					{
						item2.Value._Reward3D.SetActive(value: true);
					}
					if (item2.Value._Reward2D != null)
					{
						ApplyReward2dEffects(item2.Value._Reward2D, item2.Value._RewardInfo);
					}
				}
				list2.Add(item2.Key);
			}
			else if (item2.Value._Reward3D != null && item2.Value._Reward3D.activeInHierarchy)
			{
				item2.Value._Reward3D.SetActive(value: false);
			}
		}
		foreach (RewardsItemDataLoader item3 in list2)
		{
			mRewardItemLoading.Remove(item3);
		}
		list2.Clear();
		list2 = null;
		if (mPendingRewards.Count > 0)
		{
			DisplayRewards(mPendingRewards.ToArray());
		}
	}

	private void SetMessageObject(GameObject inMessageObject)
	{
		mMessageObject = inMessageObject;
	}
}
