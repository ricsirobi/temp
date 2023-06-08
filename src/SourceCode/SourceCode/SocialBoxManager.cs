using System;
using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class SocialBoxManager : MonoBehaviour
{
	[NonSerialized]
	public GameObject _SocialBox;

	public string _SocialBoxPrefabPath = "RS_DATA/PfSocialBoxDO.unity3d/PfSocialBoxDO";

	public int _NumOfLocksToOpenBox = 3;

	public int _AmountOfRewardingCoins = 5;

	public InteractiveTutManager _Tutorial;

	public string _SocialBoxPairDataKey = "SocialBoxStatus";

	public int _SocialBoxStoreID = 102;

	public int _SocialBoxItemID = 8774;

	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public LocaleString _AlreadyClickedText = new LocaleString("You have already clicked");

	public LocaleString _NoRoomForSignatureText = new LocaleString("There is no more room for signatures!");

	public LocaleString _SignatureAcceptedText = new LocaleString("Your signature successfully added to the box");

	public LocaleString _SignatureFailedText = new LocaleString("Failed adding your signature to the box. Try again.");

	public LocaleString _SignatureHelpText = new LocaleString("{x} needs help unlocking his crate! Help him out?");

	public LocaleString _BoxReadyToBeOpenedText = new LocaleString("Your box is ready to be opened!");

	public float _RewardDBDelay = 1f;

	private UserActivityInstance mUserActivity;

	private string mRoomUserID;

	private string mCurrentUserID;

	private int mNumberOfLocksUnlocked;

	private KAUIGenericDB mKAUIGenericDB;

	private bool mInitDone;

	private bool mIsSocialBoxOpened;

	private bool mIsStoreLoaded;

	private const string SIGNATURE_ADDED = "SIGADD";

	private bool mIsBoxOpeningAnim;

	private float mShowRewardTimer;

	private int mUpdatedClickCount;

	public static SocialBoxManager pInstance;

	public string _ReadyToOpenEffect = "ReadyFx";

	public UserActivityInstance pUserActivity => mUserActivity;

	private void Init()
	{
		pInstance = this;
		if (_Tutorial != null)
		{
			InteractiveTutManager tutorial = _Tutorial;
			tutorial._TutorialCompleteEvent = (TutorialCompleteEvent)Delegate.Combine(tutorial._TutorialCompleteEvent, new TutorialCompleteEvent(TutorialCompleteEvent));
		}
		if (mUserActivity != null)
		{
			return;
		}
		mCurrentUserID = UserInfo.pInstance.UserID;
		if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
		{
			mRoomUserID = MyRoomsIntLevel.pInstance.GetMyRoomsIntHostID();
			if (string.IsNullOrEmpty(mRoomUserID))
			{
				mRoomUserID = UserInfo.pInstance.UserID.ToString();
			}
		}
		else
		{
			mRoomUserID = mCurrentUserID;
		}
		mUserActivity = UserActivityInstance.Init(mRoomUserID, InitUserActivityHandler);
	}

	public void TutorialCompleteEvent()
	{
		UiSocialCrate.Show(UiSocialCrate.SocialBoxUiType.CRATE_INFO);
	}

	private void InitUserActivityHandler(bool success)
	{
		if (!success)
		{
			return;
		}
		if (!mInitDone)
		{
			mInitDone = true;
			MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(SocialBoxMessageEventHandler);
			CalculateNumberOfLocksUnlocked();
			if (mCurrentUserID == mRoomUserID)
			{
				if (ProductData.pIsReady)
				{
					mIsSocialBoxOpened = ProductData.pPairData.GetBoolValue(_SocialBoxPairDataKey, defaultVal: false);
				}
			}
			else if (mNumberOfLocksUnlocked >= _NumOfLocksToOpenBox)
			{
				mIsSocialBoxOpened = true;
			}
			if (!mIsSocialBoxOpened && ((mCurrentUserID != mRoomUserID && CheckIsBuddy(mRoomUserID)) || mCurrentUserID == mRoomUserID))
			{
				string[] array = _SocialBoxPrefabPath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBundleReady, typeof(GameObject));
				ItemStoreDataLoader.Load(_SocialBoxStoreID, OnStoreLoaded);
			}
		}
		else
		{
			CalculateNumberOfLocksUnlocked();
			UiSocialCrate.Show(UiSocialCrate.SocialBoxUiType.SIGNATURE_LIST);
		}
	}

	private void Update()
	{
		if (pInstance == null && UserInfo.pIsReady)
		{
			if (BuddyList.pIsReady)
			{
				Init();
			}
			else
			{
				BuddyList.Init();
			}
		}
		if (!mIsBoxOpeningAnim)
		{
			return;
		}
		mShowRewardTimer -= Time.deltaTime;
		if (mShowRewardTimer <= 0f)
		{
			mIsBoxOpeningAnim = false;
			mShowRewardTimer = 0f;
			if (ProductData.pIsReady)
			{
				ProductData.pPairData.SetValueAndSave(_SocialBoxPairDataKey, mIsSocialBoxOpened.ToString());
			}
			UiSocialCrate.Show(UiSocialCrate.SocialBoxUiType.REWARD_GROUP);
		}
	}

	private void OnClick()
	{
		if (mRoomUserID == mCurrentUserID)
		{
			if (mIsSocialBoxOpened || (!(_Tutorial == null) && !_Tutorial.TutorialComplete()))
			{
				return;
			}
			if (mNumberOfLocksUnlocked >= _NumOfLocksToOpenBox && mIsStoreLoaded)
			{
				_SocialBox.GetComponent<Animation>().Play("Open");
				mIsSocialBoxOpened = true;
				mIsBoxOpeningAnim = true;
				mShowRewardTimer = _RewardDBDelay;
				return;
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			if (!BuddyList.pIsReady)
			{
				BuddyList.Init();
			}
			mUserActivity = UserActivityInstance.Init(mRoomUserID, InitUserActivityHandler);
		}
		else if (IsUserAlreadyClicked(new Guid(mCurrentUserID)))
		{
			ShowMessage(_AlreadyClickedText.GetLocalizedString());
		}
		else if (mNumberOfLocksUnlocked + mUpdatedClickCount >= _NumOfLocksToOpenBox)
		{
			ShowMessage(_NoRoomForSignatureText.GetLocalizedString());
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			UserActivity userActivity = new UserActivity();
			userActivity.UserID = new Guid(mRoomUserID);
			userActivity.RelatedUserID = new Guid(mCurrentUserID);
			userActivity.UserActivityTypeID = 3;
			mUserActivity.Save(userActivity, UserActivityHandler);
		}
	}

	private void UserActivityHandler(bool success, UserActivity inData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (success)
		{
			if (inData.UserID != new Guid(mCurrentUserID))
			{
				ReleaseLocks();
			}
			ShowMessage(_SignatureAcceptedText.GetLocalizedString());
			MainStreetMMOClient.pInstance.SendPublicMMOMessage("SIGADD");
		}
		else
		{
			ShowMessage(_SignatureFailedText.GetLocalizedString());
		}
	}

	private void ReleaseLocks()
	{
		Money.AddMoney(_AmountOfRewardingCoins, bForceUpdate: true);
	}

	private bool CheckIsBuddy(string userId)
	{
		bool result = false;
		if (BuddyList.pIsReady)
		{
			Buddy[] pList = BuddyList.pList;
			foreach (Buddy buddy in pList)
			{
				if (buddy.UserID == userId && buddy.Status == BuddyStatus.Approved)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	private void SetVisible(bool visible)
	{
		if (_SocialBox != null)
		{
			_SocialBox.SetActive(visible);
		}
	}

	private void OnBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			_SocialBox = UnityEngine.Object.Instantiate((GameObject)inObject);
			_SocialBox.name = "PfSocialBoxDO";
			_SocialBox.transform.parent = base.transform;
			_SocialBox.GetComponent<ObClickable>()._MessageObject = base.gameObject;
			SetReadyEffect();
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private bool IsUserAlreadyClicked(Guid clickedUserID)
	{
		foreach (UserActivity p in mUserActivity.pList)
		{
			if (p.UserActivityTypeID == 3 && p.RelatedUserID == clickedUserID)
			{
				return true;
			}
		}
		return false;
	}

	private void CalculateNumberOfLocksUnlocked()
	{
		if (mUserActivity.pList != null && mUserActivity.pList.Count >= 0)
		{
			mNumberOfLocksUnlocked = 0;
			foreach (UserActivity p in mUserActivity.pList)
			{
				if (p.UserActivityTypeID == 3 && p.RelatedUserID != new Guid(mRoomUserID))
				{
					mNumberOfLocksUnlocked++;
				}
			}
		}
		if (mNumberOfLocksUnlocked >= _NumOfLocksToOpenBox)
		{
			mNumberOfLocksUnlocked = _NumOfLocksToOpenBox;
			SetReadyEffect();
		}
	}

	public void ShowMessage(string text)
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "Message Box");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void OnStoreLoaded(StoreData sd)
	{
		if (sd.FindItem(_SocialBoxItemID) != null)
		{
			mIsStoreLoaded = true;
		}
	}

	public void OnLevelReady()
	{
		if (!mInitDone || mIsSocialBoxOpened)
		{
			return;
		}
		if (mCurrentUserID != mRoomUserID && mNumberOfLocksUnlocked < _NumOfLocksToOpenBox && CheckIsBuddy(mRoomUserID) && !IsUserAlreadyClicked(new Guid(mCurrentUserID)))
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			Buddy buddy = BuddyList.pInstance.GetBuddy(mRoomUserID.ToString());
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Signature Help");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			string text = _SignatureHelpText.GetLocalizedString();
			if (buddy != null)
			{
				text = text.Replace("{x}", buddy.DisplayName);
			}
			mKAUIGenericDB.SetText(text, interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._YesMessage = "SignatureHelpYes";
			mKAUIGenericDB._NoMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
			WsUserMessage.pBlockMessages = true;
		}
		else
		{
			if (!(mCurrentUserID == mRoomUserID) || (!(_Tutorial == null) && !_Tutorial.TutorialComplete()) || mUserActivity.pList == null || mUserActivity.pList.Count < 0)
			{
				return;
			}
			foreach (UserActivity p in mUserActivity.pList)
			{
				if (CheckIsBuddy(p.RelatedUserID.ToString()) && p.UserActivityTypeID == 3 && p.RelatedUserID != new Guid(mRoomUserID) && !ProductData.pPairData.GetBoolValue(_SocialBoxPairDataKey + "-" + p.RelatedUserID.ToString(), defaultVal: false))
				{
					UiSocialCrate.Show(UiSocialCrate.SocialBoxUiType.SIGNATURE_NOTIFICATION);
					break;
				}
			}
		}
	}

	private void SignatureHelpYes()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			AvAvatar.SetUIActive(inActive: false);
			component.pEndSplineMessageObject = base.gameObject;
			Vector3 vector = AvAvatar.pObject.transform.position - _SocialBox.transform.position;
			vector.Normalize();
			component.MoveTo(_SocialBox.transform.position + vector * 2f);
			AvAvatar.pInputEnabled = false;
		}
		else
		{
			AvAvatar.pState = AvAvatarState.IDLE;
		}
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	private void OnPathEndReached()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pInputEnabled = true;
		WsUserMessage.pBlockMessages = false;
	}

	private void KillGenericDB()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
		WsUserMessage.pBlockMessages = false;
	}

	private void SocialBoxMessageEventHandler(object sender, MMOMessageReceivedEventArgs args)
	{
		if (args.MMOMessage.MessageType == MMOMessageType.User && args.MMOMessage.MessageText == "SIGADD")
		{
			mUpdatedClickCount++;
			CalculateNumberOfLocksUnlocked();
			if (mNumberOfLocksUnlocked + mUpdatedClickCount >= _NumOfLocksToOpenBox && mRoomUserID == mCurrentUserID)
			{
				ShowMessage(_BoxReadyToBeOpenedText.GetLocalizedString());
			}
		}
	}

	public void SetReadyEffect()
	{
		if (_SocialBox != null && mNumberOfLocksUnlocked >= _NumOfLocksToOpenBox)
		{
			Transform transform = _SocialBox.transform.Find(_ReadyToOpenEffect);
			if (transform != null)
			{
				transform.gameObject.SetActive(value: true);
			}
		}
	}
}
