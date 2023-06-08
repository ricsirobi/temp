using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDragonQuestionnaire : KAUI
{
	[Serializable]
	public class Answer
	{
		public LocaleString _AnswerText = new LocaleString("");

		public int _DragonType;
	}

	[Serializable]
	public class SuggestionMessage
	{
		public int _DragonType;

		public LocaleString _MessageText = new LocaleString("");

		public Texture _Icon;
	}

	[Serializable]
	public class Question
	{
		public LocaleString _QuestionText = new LocaleString("");

		public List<Answer> _Answers;
	}

	[Serializable]
	public class PetScale
	{
		public int _PetTypeID;

		public Vector3 _PetScale;
	}

	private class DragonPreference
	{
		public int _DragonTypeID;

		public int _DragonPreference;

		public DragonPreference(int dragonType, int initialPreference)
		{
			_DragonTypeID = dragonType;
			_DragonPreference = initialPreference;
		}
	}

	public int _ThunderdrumTicketID = 8206;

	public int _WhisperingTicketID = 8205;

	public float _VerticalPadding = 75f;

	public LocaleString _SpecialDragonReminderText = new LocaleString(" Dont Forget! You have access to the special Dragons!");

	public LocaleString _ThunderdrumText = new LocaleString("Thunderdrum");

	public LocaleString _WhisperingText = new LocaleString("Whispering");

	public LocaleString _AndText = new LocaleString("and");

	public LocaleString _DragonText = new LocaleString("Dragon!");

	public LocaleString _DragonsText = new LocaleString("Dragons!");

	public int _MaxQuestionsToBeAsked = 5;

	public List<Question> _Questions;

	public bool _UseNextButton = true;

	public LocaleString _IntroMessage = new LocaleString("Welcome to the dragon hatchery! Let’s see which dragon is best for you!");

	public LocaleString _IntroMessageHeaderText = new LocaleString("Hatchery");

	public string _PersonalityTestTaskName = "PersonalityTest";

	public string _CloseMessage = "OnHypothesisDBClose";

	public GameObject _MessageObject;

	public PetScale[] _PetScales;

	public Vector3 _PicturePositionDragon = new Vector3(500f, 500f, 500f);

	public Vector3 _PicturePositionOffset = new Vector3(2f, 2f, 6f);

	public Vector3 _PictureLookAtOffset = new Vector3(0.2f, 0.8f, 0.5f);

	public List<SuggestionMessage> _SuggestionMessages;

	public bool _UseIcons = true;

	private GameObject mUiGenericDB;

	private KAWidget mQuestionText;

	private KAWidget mAnswerOptionTemplate;

	private KAWidget mNextBtn;

	private KAWidget mOkBtn;

	private KAWidget mInfoText;

	private KAWidget mSnapShot;

	private KAWidget mDragonEgg;

	private List<KAWidget> mAnswerItems;

	private List<DragonPreference> mDragonPreferences;

	private int mCurrentQuestion;

	private int mPrefferredDragonTypeID = -1;

	private string mNPCName = "";

	private Task mCurrentTask;

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	public void Initialize()
	{
		mQuestionText = FindItem("QuestionText");
		mAnswerOptionTemplate = FindItem("AnswerOptionTemplate");
		mNextBtn = FindItem("NextBtn");
		mOkBtn = FindItem("OkBtn");
		mInfoText = FindItem("InfoText");
		mSnapShot = FindItem("SnapShot");
		mDragonEgg = FindItem("DragonEgg");
		mAnswerItems = new List<KAWidget>();
		SetVisibility(inVisible: false);
		ShowQuestionnaire();
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mNextBtn)
		{
			NextQuestion();
		}
		else if (item == mOkBtn)
		{
			SuggestionMessageOK();
		}
		int answerClicked = GetAnswerClicked(item);
		if (answerClicked >= 0)
		{
			SelectAnswerOption(answerClicked);
			if (!_UseNextButton)
			{
				NextQuestion();
			}
		}
	}

	public void StartQuestionnaire()
	{
		mPrefferredDragonTypeID = -1;
		mCurrentQuestion = 0;
		ShowIntroMessageDB();
	}

	public void SetupScreen(Task inTask, string inNPCName)
	{
		mCurrentTask = inTask;
		mNPCName = inNPCName;
	}

	private void ShowQuestion(int questionIdx)
	{
		ClearAnswerItems();
		if (_Questions != null && questionIdx >= 0 && questionIdx < _Questions.Count)
		{
			UtUtilities.Shuffle(_Questions);
			mQuestionText.SetText(_Questions[questionIdx]._QuestionText.GetLocalizedString());
			mQuestionText.SetVisibility(inVisible: true);
			AddAnswerItems(_Questions[questionIdx]);
		}
	}

	private void NextQuestion()
	{
		if (!ProcessAnswer())
		{
			return;
		}
		mCurrentQuestion++;
		if (mCurrentQuestion < _MaxQuestionsToBeAsked)
		{
			ShowQuestion(mCurrentQuestion);
			return;
		}
		int num = -1;
		foreach (DragonPreference mDragonPreference in mDragonPreferences)
		{
			if (mDragonPreference._DragonPreference > num)
			{
				mPrefferredDragonTypeID = mDragonPreference._DragonTypeID;
				num = mDragonPreference._DragonPreference;
			}
		}
		ShowSuggestionMessageDB(mPrefferredDragonTypeID);
	}

	private bool ProcessAnswer()
	{
		int num = 0;
		foreach (KAWidget mAnswerItem in mAnswerItems)
		{
			if (((KAToggleButton)mAnswerItem.FindChildItem("AnswerToggle")).IsChecked())
			{
				foreach (DragonPreference mDragonPreference in mDragonPreferences)
				{
					if (mDragonPreference._DragonTypeID == _Questions[mCurrentQuestion]._Answers[num]._DragonType)
					{
						mDragonPreference._DragonPreference++;
						_Questions.RemoveAt(mCurrentQuestion);
						return true;
					}
				}
			}
			num++;
		}
		return false;
	}

	private int GetAnswerClicked(KAWidget item)
	{
		int num = 0;
		foreach (KAWidget mAnswerItem in mAnswerItems)
		{
			KAToggleButton kAToggleButton = (KAToggleButton)mAnswerItem.FindChildItem("AnswerToggle");
			KAWidget kAWidget = mAnswerItem.FindChildItem("AnswerText");
			if ((kAToggleButton != null && kAToggleButton == item) || (kAWidget != null && kAWidget == item))
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	private void SelectAnswerOption(int ansSelected)
	{
		if (ansSelected >= mAnswerItems.Count || ansSelected < 0)
		{
			return;
		}
		foreach (KAWidget mAnswerItem in mAnswerItems)
		{
			((KAToggleButton)mAnswerItem.FindChildItem("AnswerToggle")).SetChecked(isChecked: false);
		}
		((KAToggleButton)mAnswerItems[ansSelected].FindChildItem("AnswerToggle")).SetChecked(isChecked: true);
	}

	private void AddAnswerItems(Question questionInfo)
	{
		int num = 0;
		if (questionInfo == null)
		{
			return;
		}
		Vector3 position = mAnswerOptionTemplate.transform.position;
		UtUtilities.Shuffle(questionInfo._Answers);
		foreach (Answer answer in questionInfo._Answers)
		{
			KAWidget kAWidget = DuplicateWidget(mAnswerOptionTemplate);
			if (kAWidget != null)
			{
				kAWidget.transform.localPosition = position;
				position.y -= _VerticalPadding;
				kAWidget.SetVisibility(inVisible: true);
				KAWidget kAWidget2 = kAWidget.FindChildItem("AnswerText");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(answer._AnswerText.GetLocalizedString());
				}
				AddWidget(kAWidget);
				kAWidget.transform.parent = mAnswerOptionTemplate.transform.parent;
				mAnswerItems.Add(kAWidget);
				num++;
			}
		}
	}

	private void ClearAnswerItems()
	{
		if (mAnswerItems == null)
		{
			return;
		}
		foreach (KAWidget mAnswerItem in mAnswerItems)
		{
			RemoveWidget(mAnswerItem);
		}
		mAnswerItems.Clear();
	}

	private void CreatePet(SanctuaryPetTypeInfo petType)
	{
		string resName = "";
		if (petType._AgeData[0]._PetResList.Length != 0)
		{
			resName = petType._AgeData[0]._PetResList[0]._Prefab;
		}
		RaisedPetData raisedPetData = RaisedPetData.CreateCustomizedPetData(petType._TypeID, RaisedPetStage.BABY, resName, Gender.Male, null, noColorMap: true);
		raisedPetData.pNoSave = true;
		raisedPetData.Name = petType._Name;
		SanctuaryManager.CreatePet(raisedPetData, Vector3.zero, Quaternion.identity, base.gameObject, "Basic");
	}

	private IEnumerator TakeAShot(GameObject pet)
	{
		SanctuaryPet component = pet.GetComponent<SanctuaryPet>();
		if (component != null)
		{
			component.PlayAnimation("IdleSit", WrapMode.Loop);
			yield return new WaitForSeconds(0.5f);
		}
		pet.transform.position = _PicturePositionDragon;
		pet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfPictureCameraPet"));
		AvPictureCamera component2 = gameObject.GetComponent<AvPictureCamera>();
		gameObject.transform.Find("PfPicturePlane").gameObject.SetActive(value: false);
		int layer = pet.gameObject.layer;
		UtUtilities.SetLayerRecursively(pet.gameObject, LayerMask.NameToLayer("Avatar"));
		Vector3 position = pet.transform.TransformPoint(_PicturePositionOffset);
		component2.transform.position = position;
		component2.transform.LookAt(pet.transform.TransformPoint(_PictureLookAtOffset), Vector3.up);
		PetScale[] petScales = _PetScales;
		foreach (PetScale petScale in petScales)
		{
			if (petScale._PetTypeID == pet.GetComponent<SanctuaryPet>().pData.PetTypeID)
			{
				pet.transform.localScale = petScale._PetScale;
				break;
			}
		}
		component2.OnTakePicture(pet.gameObject, lookAtCamera: true);
		Texture2D texture2D = new Texture2D(256, 256, TextureFormat.ARGB32, mipChain: false);
		texture2D.name = "DragonQuestionnaireTexture-Dynamic";
		Texture2D texture2D2 = (Texture2D)mSnapShot.GetTexture();
		if (texture2D2 != null)
		{
			Color[] pixels = texture2D2.GetPixels();
			texture2D.SetPixels(pixels);
			texture2D.Apply();
		}
		component2.OnCopyRenderBuffer(texture2D);
		UtUtilities.SetLayerRecursively(pet.gameObject, layer);
		mSnapShot.SetTexture(texture2D);
		mOkBtn.SetVisibility(inVisible: true);
		UnityEngine.Object.Destroy(pet);
		UnityEngine.Object.Destroy(gameObject);
	}

	public virtual void OnPetReady(SanctuaryPet pet)
	{
		StartCoroutine(TakeAShot(pet.gameObject));
	}

	private void ShowSuggestionMessageDB(int suggestedDragonType)
	{
		mQuestionText.SetVisibility(inVisible: false);
		mNextBtn.SetVisibility(inVisible: false);
		foreach (KAWidget mAnswerItem in mAnswerItems)
		{
			mAnswerItem.SetVisibility(inVisible: false);
		}
		mInfoText.SetVisibility(inVisible: true);
		mSnapShot.SetVisibility(inVisible: true);
		mDragonEgg.SetVisibility(inVisible: true);
		string text = "ANY";
		SanctuaryPetTypeInfo[] petTypes = SanctuaryData.pInstance._PetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in petTypes)
		{
			if (suggestedDragonType != sanctuaryPetTypeInfo._TypeID)
			{
				continue;
			}
			if (!string.IsNullOrEmpty(sanctuaryPetTypeInfo._EggIconPath))
			{
				ShowDragonEgg(sanctuaryPetTypeInfo._EggIconPath);
			}
			text = GetSuggestionDragonText(suggestedDragonType);
			if (_UseIcons)
			{
				SuggestionMessage suggestionMessage = _SuggestionMessages.Find((SuggestionMessage t) => t._DragonType == suggestedDragonType);
				if (suggestionMessage != null && suggestionMessage._Icon != null)
				{
					mSnapShot.SetTexture(suggestionMessage._Icon);
				}
				mOkBtn.SetVisibility(inVisible: true);
			}
			else
			{
				CreatePet(sanctuaryPetTypeInfo);
			}
			break;
		}
		mInfoText.SetText(text);
	}

	private void ShowDragonEgg(string inAsset)
	{
		string[] array = inAsset.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadEvent, typeof(Texture));
	}

	public void OnAssetLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		if (inLoadEvent == RsResourceLoadEvent.COMPLETE)
		{
			Texture inTexture = (Texture)inObject;
			mDragonEgg.SetTexture(inTexture);
		}
	}

	private string GetSuggestionDragonText(int inID)
	{
		if (_SuggestionMessages != null)
		{
			foreach (SuggestionMessage suggestionMessage in _SuggestionMessages)
			{
				if (inID == suggestionMessage._DragonType)
				{
					return suggestionMessage._MessageText.GetLocalizedString();
				}
			}
		}
		return "";
	}

	private void SuggestionMessageOK()
	{
		if (ParentData.pIsReady)
		{
			string text = string.Empty;
			if (ParentData.pInstance.pInventory.pData.FindItem(_WhisperingTicketID) != null && ParentData.pInstance.pInventory.pData.FindItem(_ThunderdrumTicketID) != null)
			{
				text = _SpecialDragonReminderText.GetLocalizedString() + _ThunderdrumText.GetLocalizedString() + _AndText.GetLocalizedString() + _WhisperingText.GetLocalizedString() + _DragonsText.GetLocalizedString();
			}
			else if (ParentData.pInstance.pInventory.pData.FindItem(_WhisperingTicketID) != null)
			{
				text = _SpecialDragonReminderText.GetLocalizedString() + _WhisperingText.GetLocalizedString() + _DragonText.GetLocalizedString();
			}
			else if (ParentData.pInstance.pInventory.pData.FindItem(_ThunderdrumTicketID) != null)
			{
				text = _SpecialDragonReminderText.GetLocalizedString() + _ThunderdrumText.GetLocalizedString() + _DragonText.GetLocalizedString();
			}
			else if (ParentData.pInstance.pInventory.pData.FindItem(_WhisperingTicketID) == null && ParentData.pInstance.pInventory.pData.FindItem(_ThunderdrumTicketID) == null)
			{
				OnOK();
				return;
			}
			ShowDialog(text);
		}
		else
		{
			OnOK();
		}
	}

	public void OnEnable()
	{
	}

	private void ShowQuestionnaire()
	{
		base.gameObject.SetActive(value: true);
		KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		StartQuestionnaire();
		AvAvatar.pState = AvAvatarState.PAUSED;
	}

	private void CloseQuestionnaire()
	{
		KAUI.RemoveExclusive(this);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		if (_MessageObject != null && !string.IsNullOrEmpty(_CloseMessage))
		{
			_MessageObject.SendMessage(_CloseMessage, SendMessageOptions.DontRequireReceiver);
		}
		base.gameObject.SetActive(value: false);
	}

	private void ShowIntroMessageDB()
	{
		SetVisibility(inVisible: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Dragon Selection");
		kAUIGenericDB.SetText(_IntroMessage.GetLocalizedString(), interactive: false);
		kAUIGenericDB.SetTitle(_IntroMessageHeaderText.GetLocalizedString());
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "IntroMessageOK";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void IntroMessageOK()
	{
		mCurrentQuestion = 0;
		if (_UseNextButton)
		{
			mNextBtn.SetVisibility(inVisible: true);
		}
		mOkBtn.SetVisibility(inVisible: false);
		mInfoText.SetVisibility(inVisible: false);
		mSnapShot.SetVisibility(inVisible: false);
		mDragonEgg.SetVisibility(inVisible: false);
		if (mDragonPreferences == null)
		{
			mDragonPreferences = new List<DragonPreference>();
		}
		mDragonPreferences.Clear();
		SanctuaryPetTypeInfo[] petTypes = SanctuaryData.pInstance._PetTypes;
		foreach (SanctuaryPetTypeInfo sanctuaryPetTypeInfo in petTypes)
		{
			mDragonPreferences.Add(new DragonPreference(sanctuaryPetTypeInfo._TypeID, 0));
		}
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		ShowQuestion(mCurrentQuestion);
	}

	private void ShowDialog(string text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetVisibility(inVisible: false);
		mUiGenericDB = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		KAUIGenericDB component = mUiGenericDB.GetComponent<KAUIGenericDB>();
		component._MessageObject = base.gameObject;
		component._OKMessage = "OnOK";
		component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		component.SetText(text, interactive: false);
		KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void OnOK()
	{
		if (mCurrentTask != null && !string.IsNullOrEmpty(mNPCName))
		{
			mCurrentTask.CheckForCompletion("Meet", mNPCName, "", "");
		}
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
		CloseQuestionnaire();
	}
}
