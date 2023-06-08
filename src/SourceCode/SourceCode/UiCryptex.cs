using System;
using System.Collections.Generic;
using UnityEngine;

public class UiCryptex : KAUI
{
	public GameObject _PfCryptex;

	private CryptexGameManager mCryptexGameManager;

	public List<KAWidget> _RuneWidgets = new List<KAWidget>();

	[Tooltip("Put these in order from 0 - 360 on the dial!")]
	public List<Texture> _RuneTextures = new List<Texture>();

	public float _XRuneOffset = 15f;

	public float _YRuneOffset = 20f;

	[Tooltip("Does the UI rune display have a random rotation applied.")]
	public bool _RuneHasRandomRotation = true;

	[Tooltip("Does the UI rune display have a chance to be mirrored.")]
	public bool _RuneHasRandomMirror = true;

	public LocaleString _QuitConfirmationText = new LocaleString("Are you sure you want to quit?");

	public LocaleString _QuitConfirmationTitleText = new LocaleString("Quit?");

	[Tooltip("If this is set tell the mission system the given game was completed")]
	public string _GameName;

	public GameObject _MessageObject;

	public string _CloseMessage;

	public UiMissionTaskTimer _UiTaskTimer;

	[NonSerialized]
	public string _GameLevel;

	private KAWidget mBtnBack;

	private KAWidget mBtnHelp;

	private KAWidget mBtnCloseHelp;

	private Task mCurrentTask;

	private string mNPCName;

	private bool mIsTimedTask;

	protected override void Start()
	{
		base.Start();
		AvAvatar.SetActive(inActive: false);
		GiveWidgetsRandomOffset();
		mBtnBack = FindItem("BtnBack");
		mBtnHelp = FindItem("BtnHelp");
		mBtnCloseHelp = FindItem("BtnCloseHelp");
		if (_UiTaskTimer != null)
		{
			mIsTimedTask = _UiTaskTimer.IsMissionTimedGame(_GameName, _GameLevel);
		}
		if (mCryptexGameManager != null)
		{
			mCryptexGameManager.InitializeRuneInfo(_RuneWidgets, _RuneTextures, this);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
		if (!ProductData.TutorialComplete("CryptexTutorial"))
		{
			ShowHelp(help: true);
			ProductData.AddTutorial("CryptexTutorial");
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mIsTimedTask && MissionManager.pInstance.pActiveTimedTask.Failed)
		{
			QuitGame();
		}
	}

	private void AbortTimerTask()
	{
		QuitGame();
	}

	public void CreateCryptexObject()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_PfCryptex);
		gameObject.transform.parent = base.transform;
		mCryptexGameManager = gameObject.GetComponent<CryptexGameManager>();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnBack)
		{
			if (UtPlatform.IsAndroid() && mBtnCloseHelp.GetVisibility())
			{
				ShowHelp(help: false);
				return;
			}
			GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _QuitConfirmationText.GetLocalizedString(), _QuitConfirmationTitleText.GetLocalizedString(), base.gameObject, "QuitGame", "CancelQuitCryptex", "", "", inDestroyOnClick: true);
			mCryptexGameManager.SetActive(active: false);
		}
		if (inWidget == mBtnHelp)
		{
			ShowHelp(help: true);
		}
		if (inWidget == mBtnCloseHelp)
		{
			ShowHelp(help: false);
		}
	}

	private void GiveWidgetsRandomOffset()
	{
		for (int i = 0; i < _RuneWidgets.Count; i++)
		{
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				_RuneWidgets[i].transform.Translate(_XRuneOffset, 0f, 0f);
				break;
			case 1:
				_RuneWidgets[i].transform.Translate(0f - _XRuneOffset, 0f, 0f);
				break;
			}
			switch (UnityEngine.Random.Range(0, 3))
			{
			case 0:
				_RuneWidgets[i].transform.Translate(0f, _YRuneOffset, 0f);
				break;
			case 1:
				_RuneWidgets[i].transform.Translate(0f, 0f - _YRuneOffset, 0f);
				break;
			}
			if (_RuneHasRandomRotation)
			{
				int num = UnityEngine.Random.Range(0, 360);
				_RuneWidgets[i].SetRotation(Quaternion.Euler(0f, 0f, num));
			}
			if (_RuneHasRandomMirror && UnityEngine.Random.Range(0, 2) == 0)
			{
				_RuneWidgets[i].SetRotation(Quaternion.Euler(0f, 180f, 0f));
			}
		}
	}

	private void CancelQuitCryptex()
	{
		mCryptexGameManager.SetActive(active: true);
	}

	private void QuitGame()
	{
		if (_MessageObject != null && !string.IsNullOrEmpty(_CloseMessage))
		{
			_MessageObject.SendMessage(_CloseMessage, SendMessageOptions.DontRequireReceiver);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
		mCryptexGameManager.QuitGame();
		AvAvatar.SetActive(inActive: true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void WonGame(bool solvedPuzzle)
	{
		if (solvedPuzzle)
		{
			if (mCurrentTask != null && !string.IsNullOrEmpty(mNPCName))
			{
				mCurrentTask.CheckForCompletion("Meet", mNPCName, "", "");
			}
			if (!string.IsNullOrEmpty(_GameName) && MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Game", _GameName, _GameLevel);
			}
		}
		QuitGame();
	}

	public void SetupScreen(Task inTask, string inNPCName)
	{
		mNPCName = inNPCName;
		mCurrentTask = inTask;
	}

	private void ShowHelp(bool help)
	{
		mBtnCloseHelp.SetVisibility(help);
		mCryptexGameManager.SetActive(!help);
	}
}
