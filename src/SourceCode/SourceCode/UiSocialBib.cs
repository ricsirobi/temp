using KnowledgeAdventure.Multiplayer.Events;
using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class UiSocialBib : KAUI
{
	public class AvatarCountryGenderImage
	{
		private KAWidget mCountryItem;

		private KAWidget mGenderItem;

		public AvatarCountryGenderImage(KAWidget country, KAWidget gender)
		{
			mCountryItem = country;
			mGenderItem = gender;
		}

		public void CountryImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			if (inEvent == RsResourceLoadEvent.COMPLETE)
			{
				Texture inTexture = (Texture)inObject;
				mCountryItem.SetTexture(inTexture);
			}
		}

		public void GenderImageLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			if (inEvent == RsResourceLoadEvent.COMPLETE)
			{
				Texture inTexture = (Texture)inObject;
				mGenderItem.SetTexture(inTexture);
			}
		}
	}

	public class AvatarPic
	{
		private KAWidget mItem;

		public AvatarPic(KAWidget item)
		{
			mItem = item;
		}

		public void PhotoCallback(Texture tex, object inUserData)
		{
			mItem.SetTexture(tex);
		}
	}

	public Taunts[] _Taunts;

	public string _IgnoreKey;

	public int _IgnoreKeyID;

	public KeyCode _IgnoreKeyKeyCode;

	public LocaleString _IgnoredText = new LocaleString("IGNORED");

	public GameObject[] _WebOnlyControls;

	public GameObject[] _MobileOnlyControls;

	private static UiSocialBib mInstance;

	private bool mMMOInitDone;

	private bool mIsIgnoreUser;

	private bool mIsProfileLoaded;

	private string mOpponentMessage = "";

	protected const char MESSAGE_SEPARATOR = ':';

	private const string TAUNT_KEY = "TK";

	private const string SCORE_KEY = "SK";

	private KAWidget mIgnoreBtn;

	private KAWidget mTxtIgnoreCtrl;

	private KAWidget mTxtPlayer01Name;

	private KAWidget mTxtPlayer02Name;

	private KAWidget mTxtPlayer01Score;

	private KAWidget mTxtPlayer02Score;

	private KAWidget mTxtPlayer01Chat;

	private KAWidget mTxtPlayer02Chat;

	private KAWidget mPlayer1Headshot;

	private KAWidget mPlayer2Headshot;

	private KAWidget mTxtPlayer1Rank;

	private KAWidget mPlayer1GenderIcon;

	private KAWidget mPlayer1CountryIcon;

	private KAWidget mTxtPlayer2Rank;

	private KAWidget mPlayer2GenderIcon;

	private KAWidget mPlayer2CountryIcon;

	private AvPhotoManager mStillPhotoManager;

	private UserProfile mOpponentUserProfile;

	private UserProfileData mMyUserProfileData;

	private string mOpponentUserID;

	private GameObject mAvatarObject;

	public static UiSocialBib pInstance => mInstance;

	protected override void Awake()
	{
		base.Awake();
		bool flag = true;
		GameObject[] mobileOnlyControls;
		if (_MobileOnlyControls != null)
		{
			mobileOnlyControls = _MobileOnlyControls;
			foreach (GameObject gameObject in mobileOnlyControls)
			{
				if (gameObject != null)
				{
					gameObject.SetActive(flag);
				}
			}
		}
		if (_WebOnlyControls == null)
		{
			return;
		}
		mobileOnlyControls = _WebOnlyControls;
		foreach (GameObject gameObject2 in mobileOnlyControls)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(!flag);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mInstance = this;
		ShowUI(isShow: false);
		mStillPhotoManager = AvPhotoManager.Init("PfMessagePhotoMgr");
		mIgnoreBtn = FindItem("IgnoreBtnMobile");
		mTxtIgnoreCtrl = FindItem("TxtIgnoreCtrl");
		mTxtPlayer01Name = FindItem("TxtPlayer01Name");
		mTxtPlayer02Name = FindItem("TxtPlayer02Name");
		mTxtPlayer01Score = FindItem("TxtPlayer01Score");
		mTxtPlayer02Score = FindItem("TxtPlayer02Score");
		mTxtPlayer01Chat = FindItem("TxtPlayer01Chat");
		mTxtPlayer02Chat = FindItem("TxtPlayer02Chat");
		mPlayer1Headshot = FindItem("Player1Headshot");
		mPlayer2Headshot = FindItem("Player2Headshot");
		mTxtPlayer1Rank = FindItem("TxtPlayer1Rank");
		mPlayer1GenderIcon = FindItem("Player1GenderIcon");
		mPlayer1CountryIcon = FindItem("Player1CountryIcon");
		mTxtPlayer2Rank = FindItem("TxtPlayer2Rank");
		mPlayer2GenderIcon = FindItem("Player2GenderIcon");
		mPlayer2CountryIcon = FindItem("Player2CountryIcon");
		SetDefault();
		InitializeUI();
	}

	private void InitializeUI()
	{
		for (int i = 0; i < _Taunts.Length; i++)
		{
			string inWidgetName = "Taunt" + (i + 1).ToString("d02") + "Btn";
			KAWidget kAWidget = FindItem(inWidgetName);
			if (kAWidget != null)
			{
				KAWidget kAWidget2 = kAWidget.FindChildItemAt(0);
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(_Taunts[i]._TauntText.GetLocalizedString());
				}
			}
		}
	}

	private void SetDefault()
	{
		if (mTxtIgnoreCtrl != null)
		{
			if (string.IsNullOrEmpty(_IgnoreKey))
			{
				mTxtIgnoreCtrl.SetText("CTRL");
			}
			else
			{
				mTxtIgnoreCtrl.SetTextByID(_IgnoreKeyID, _IgnoreKey);
			}
		}
		mTxtPlayer01Name.SetText("");
		mTxtPlayer02Name.SetText("");
		mTxtPlayer01Score.SetText("0");
		mTxtPlayer02Score.SetText("0");
		mTxtPlayer01Chat.SetText("");
		mTxtPlayer02Chat.SetText("");
		mTxtPlayer1Rank.SetText("0");
		mTxtPlayer2Rank.SetText("0");
	}

	protected override void Update()
	{
		base.Update();
		if (!mMMOInitDone && MainStreetMMOClient.pInstance != null && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			MainStreetMMOClient.pInstance.AddMessageReceivedEventHandler(SocialBibMessageHandler);
			mMMOInitDone = true;
		}
		if (!mIsProfileLoaded && mOpponentUserProfile != null && mMyUserProfileData != null && mOpponentUserProfile.pIsReady)
		{
			mIsProfileLoaded = true;
			SetName(mMyUserProfileData.AvatarInfo.GetDisplayName(), isOpponent: false);
			SetName(mOpponentUserProfile.GetDisplayName(), isOpponent: true);
			SetRankCountryGender(null, 0, isOpponent: false);
			SetRankCountryGender(mOpponentUserProfile, 0, isOpponent: true);
		}
		CheckUserInput();
	}

	private void CheckUserInput()
	{
		if (!GetVisibility())
		{
			return;
		}
		for (int i = 0; i < _Taunts.Length; i++)
		{
			if (Input.GetKeyUp(_Taunts[i]._Key.ToLower()) || Input.GetKeyUp(_Taunts[i]._KeyCode))
			{
				SetTaunt(i, isOpponent: false);
				break;
			}
		}
		if (string.IsNullOrEmpty(_IgnoreKey))
		{
			if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
			{
				IgnoreUser();
			}
		}
		else if (Input.GetKeyUp(_IgnoreKey.ToLower()) || Input.GetKeyUp(_IgnoreKeyKeyCode))
		{
			IgnoreUser();
		}
	}

	public override void OnClick(KAWidget item)
	{
		string text = item.name;
		if (item == mIgnoreBtn)
		{
			IgnoreUser();
		}
		if (text != null && text.Contains("Taunt"))
		{
			if (text.Length > 6)
			{
				int index = int.Parse(item.name.Substring(5, 2)) - 1;
				SetTaunt(index, isOpponent: false);
			}
			else
			{
				Debug.LogError("Taunt length is invalid or null");
			}
		}
	}

	protected virtual void SocialBibMessageHandler(object sender, MMOMessageReceivedEventArgs args)
	{
		if (args.MMOMessage.MessageType != MMOMessageType.User)
		{
			return;
		}
		string[] array = args.MMOMessage.MessageText.Split(':');
		if (array.Length > 1)
		{
			int result;
			if (array[0] == "SK")
			{
				SetScore(int.Parse(array[1]), isOpponent: true);
			}
			else if (!mIsIgnoreUser && array[0] == "TK" && int.TryParse(array[1], out result))
			{
				SetTaunt(result, isOpponent: true);
			}
		}
	}

	public static void Init(string opponentUserID, GameObject avatarObject)
	{
		if (!(mInstance == null))
		{
			mInstance.mAvatarObject = avatarObject;
			mInstance.SetDefault();
			mInstance.mOpponentUserID = opponentUserID;
			mInstance.mIsProfileLoaded = false;
			mInstance.BeginAvatarShot();
			mInstance.LoadUserProfiles();
		}
	}

	public static void Init(string opponentUserID)
	{
		Init(opponentUserID, AvAvatar.pObject);
	}

	private void BeginAvatarShot()
	{
		AvatarPic @object = new AvatarPic(mPlayer1Headshot);
		mStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)mPlayer1Headshot.GetTexture(), @object.PhotoCallback, null);
		@object = new AvatarPic(mPlayer2Headshot);
		mStillPhotoManager.TakePhotoUI(mOpponentUserID, (Texture2D)mPlayer2Headshot.GetTexture(), @object.PhotoCallback, null);
	}

	private void LoadUserProfiles()
	{
		mOpponentUserProfile = UserProfile.LoadUserProfile(mOpponentUserID);
		mMyUserProfileData = UserProfile.pProfileData;
	}

	public static void ShowUI(bool isShow)
	{
		if (!(mInstance == null))
		{
			mInstance.SetVisibility(isShow);
		}
	}

	private void IgnoreUser()
	{
		mIsIgnoreUser = !mIsIgnoreUser;
		if (!mIsIgnoreUser)
		{
			mTxtPlayer02Chat.SetText(mOpponentMessage);
		}
		else
		{
			mTxtPlayer02Chat.SetText(_IgnoredText.GetLocalizedString());
		}
	}

	private string GetTauntFromIndex(int index)
	{
		if (index >= 0 && index < _Taunts.Length)
		{
			return _Taunts[index]._TauntText.GetLocalizedString();
		}
		return "";
	}

	public void SetName(string name, bool isOpponent)
	{
		if (isOpponent)
		{
			mTxtPlayer02Name.SetText(name);
		}
		else
		{
			mTxtPlayer01Name.SetText(name);
		}
	}

	public void SetTaunt(int index, bool isOpponent)
	{
		if (isOpponent)
		{
			mOpponentMessage = GetTauntFromIndex(index);
			mTxtPlayer02Chat.SetText(mOpponentMessage);
			return;
		}
		mTxtPlayer01Chat.SetText(GetTauntFromIndex(index));
		if (MainStreetMMOClient.pIsReady)
		{
			string message = "TK:" + index;
			MainStreetMMOClient.pInstance.SendPublicExtensionMessage(message);
		}
	}

	public static void SetScore(int score, bool isOpponent)
	{
		if (mInstance == null)
		{
			return;
		}
		if (isOpponent)
		{
			mInstance.mTxtPlayer02Score.SetText(score.ToString());
			return;
		}
		mInstance.mTxtPlayer01Score.SetText(score.ToString());
		if (MainStreetMMOClient.pIsReady)
		{
			string message = "SK:" + score;
			MainStreetMMOClient.pInstance.SendPublicExtensionMessage(message);
		}
	}

	public void SetRankCountryGender(UserProfile userProfile, int country, bool isOpponent)
	{
		int num = 0;
		if (isOpponent)
		{
			if (userProfile != null)
			{
				if (userProfile.AvatarInfo != null)
				{
					num = UserRankData.GetUserAchievementInfoByType(userProfile.AvatarInfo.Achievements, 1).RankID;
				}
				mTxtPlayer2Rank.SetText(num.ToString());
				string countryURL = userProfile.GetCountryURL(mInstance.mAvatarObject);
				string genderURL = userProfile.GetGenderURL(mInstance.mAvatarObject);
				LoadImage(mPlayer2CountryIcon, mPlayer2GenderIcon, countryURL, genderURL);
			}
		}
		else if (mMyUserProfileData != null && mMyUserProfileData.AvatarInfo != null)
		{
			num = UserRankData.GetUserAchievementInfoByType(mMyUserProfileData.AvatarInfo.Achievements, 1).RankID;
			mTxtPlayer1Rank.SetText(num.ToString());
			string countryURL2 = UserProfile.pProfileData.GetCountryURL(mInstance.mAvatarObject);
			string genderURL2 = UserProfile.pProfileData.GetGenderURL(mInstance.mAvatarObject);
			LoadImage(mPlayer1CountryIcon, mPlayer1GenderIcon, countryURL2, genderURL2);
		}
	}

	private void LoadImage(KAWidget countryItem, KAWidget genderItem, string countryURL, string genderURL)
	{
		AvatarCountryGenderImage @object = new AvatarCountryGenderImage(countryItem, genderItem);
		if (!string.IsNullOrEmpty(countryURL))
		{
			RsResourceManager.Load(countryURL, @object.CountryImageLoadingEvent);
		}
		if (!string.IsNullOrEmpty(genderURL))
		{
			RsResourceManager.Load(genderURL, @object.GenderImageLoadingEvent);
		}
	}
}
