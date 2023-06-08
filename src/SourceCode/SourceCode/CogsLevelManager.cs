using System.Collections.Generic;
using UnityEngine;

public class CogsLevelManager : KAMonoBase
{
	public UiCogsLevelSelection _LevelSelection;

	public bool _DestroyPreviousObjects = true;

	public Transform _ActiveContainer;

	public bool _UnlockAllLevels;

	public bool _LoadLevelFromScene;

	public bool _LoadLastLevel;

	public Material _ShaderMaterial;

	public float _MenuRotateSpeed = 2f;

	public static string pLevelToLoad;

	private bool mShowLevelSelection;

	private int mStaticItemBundleCount = -1;

	private AvAvatarState mPrevAvatarState;

	private int mCurrentLevelIndex;

	private CogsLevelDetails mCurrentLevelData;

	private static CogsLevelManager mInstance;

	private static CogsLevelData mLevelData;

	public int pCurrentLevelIndex => mCurrentLevelIndex;

	public CogsLevelDetails pCurrentLevelData => mCurrentLevelData;

	public static CogsLevelManager pInstance => mInstance;

	public static CogsLevelData pLevelData => mLevelData;

	public bool pIsReady
	{
		get
		{
			if (CogsGameManager.pInstance._MainUI.pIsReady)
			{
				return mStaticItemBundleCount == 0;
			}
			return false;
		}
	}

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
		RsResourceManager.DestroyLoadScreen();
		AvAvatar.SetActive(inActive: false);
		mPrevAvatarState = AvAvatar.pState;
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (!UtPlatform.IsRealTimeShadowEnabled())
		{
			UtUtilities.SetRealTimeShadowDisabled();
		}
		if (_ActiveContainer != null)
		{
			GFMachine component = _ActiveContainer.GetComponent<GFMachine>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
		if (_LoadLevelFromScene)
		{
			SetupLevelFromSceneObjects();
		}
		else
		{
			CogsGameManager.pInstance._GameEndDBUI.gameObject.SetActive(value: true);
			KAUICursorManager.SetDefaultCursor("Loading");
			if (mLevelData == null)
			{
				RsResourceManager.Load(GameConfig.GetKeyData("CogsLevelDataFile"), XMLDownloaded);
			}
			else
			{
				InitGame();
			}
		}
		CogsLevelProgress.pInstance.Init();
	}

	private void InitGame()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (string.IsNullOrEmpty(pLevelToLoad))
		{
			mShowLevelSelection = true;
			return;
		}
		int num = 0;
		CogsLevelDetails[] levels = mLevelData.Levels;
		for (int i = 0; i < levels.Length; i++)
		{
			if (levels[i].LevelName.Equals(pLevelToLoad))
			{
				LoadLevel(num, rotateMenu: false);
				break;
			}
			num++;
		}
		if (num == mLevelData.Levels.Length)
		{
			LoadLevel(0, rotateMenu: false);
			Debug.Log("LevelToLoad not Found in LevelData");
		}
	}

	public void LoadLevel(int levelIdx, bool rotateMenu = true)
	{
		mCurrentLevelIndex = levelIdx;
		if (rotateMenu)
		{
			RotateMenu("OnLevelSelected");
		}
		else
		{
			LoadLevelData();
		}
	}

	public void RotateMenu(string inCallBack)
	{
		float y = CogsGameManager.pInstance._GameBoard.transform.eulerAngles.y;
		UITweener uITweener = TweenRotation.Begin(CogsGameManager.pInstance._GameBoard, _MenuRotateSpeed, Quaternion.Euler(new Vector3(0f, y - 180f, 0f)));
		uITweener.eventReceiver = base.gameObject;
		SnChannel.Play(CogsGameManager.pInstance._BoardFlipSFX);
		if (!string.IsNullOrEmpty(inCallBack))
		{
			uITweener.callWhenFinished = inCallBack;
		}
	}

	private void LoadLevelData()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		mCurrentLevelData = mLevelData.Levels[mCurrentLevelIndex];
		if (mCurrentLevelData != null)
		{
			if (_DestroyPreviousObjects)
			{
				DestroyPreviousObjects();
			}
			SetUpInventory();
			SetupStaticItems();
		}
		CogsGameManager.pInstance.pIsInitialized = false;
		CogsGameManager.pInstance._MainUI.SetVisibility(inVisible: true);
		CogsGameManager.pInstance._MainUI.pSetLevelText = (string.IsNullOrEmpty(pLevelToLoad) ? GetLevelNumber(pCurrentLevelIndex) : 0);
	}

	public int GetLevelNumber(int idx)
	{
		int num = 1;
		for (int i = 0; i < idx && i < mLevelData.Levels.Length; i++)
		{
			if (!mLevelData.Levels[i].IsMissionLevel)
			{
				num++;
			}
		}
		return num;
	}

	private void OnLevelSelected()
	{
		LoadLevelData();
	}

	private void OnLevelSelectionMenu()
	{
		mShowLevelSelection = true;
	}

	private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mLevelData = UtUtilities.DeserializeFromXml<CogsLevelData>((string)inFile);
			InitGame();
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.Log("Failed to Load IM LEVEL DATA XML");
			break;
		}
	}

	private void Update()
	{
		if (mShowLevelSelection && (!UserInfo.pIsReady || CogsLevelProgress.pInstance.pIsReady))
		{
			mShowLevelSelection = false;
			ShowLevelMenu();
		}
	}

	private void ShowLevelMenu()
	{
		_LevelSelection.PopulateLevels(mLevelData);
		SetCurrentPage();
	}

	private void SetCurrentPage()
	{
		List<KAWidget> items = _LevelSelection._UiCogsLevelSelectionMenu.GetItems();
		int num = 1;
		for (int i = 0; i < items.Count; i++)
		{
			CogsLevelUserData cogsLevelUserData = (CogsLevelUserData)items[i].GetUserData();
			if (mCurrentLevelIndex == cogsLevelUserData._Index)
			{
				num = cogsLevelUserData._Level;
				break;
			}
		}
		int inPageNumber = Mathf.CeilToInt((float)num / (float)_LevelSelection._UiCogsLevelSelectionMenu.GetNumItemsPerPage());
		_LevelSelection._UiCogsLevelSelectionMenu.GoToPage(inPageNumber, instant: true);
		_LevelSelection.SetVisibility(inVisible: true);
	}

	public void SetUpInventory()
	{
		if (mCurrentLevelData.InventoryItems != null)
		{
			CogsGameManager.pInstance._MainUI._CogsItemMenu.SetupInventory(mCurrentLevelData);
		}
	}

	public void ResetLevelData()
	{
		mCurrentLevelData = null;
		mStaticItemBundleCount = -1;
		CogsGameManager.pInstance._MainUI.SetVisibility(inVisible: false);
	}

	public void SetupStaticItems()
	{
		List<string> list = new List<string>();
		if (mCurrentLevelData.StaticItems != null)
		{
			if (mCurrentLevelData.StaticItems.StartCogs != null)
			{
				Cog[] startCogs = mCurrentLevelData.StaticItems.StartCogs;
				foreach (Cog cog in startCogs)
				{
					if (cog != null)
					{
						list.Add(cog.Asset);
					}
				}
			}
			if (mCurrentLevelData.StaticItems.VictoryCogs != null)
			{
				Cog[] startCogs = mCurrentLevelData.StaticItems.VictoryCogs;
				foreach (Cog cog2 in startCogs)
				{
					if (cog2 != null)
					{
						list.Add(cog2.Asset);
					}
				}
			}
			if (mCurrentLevelData.StaticItems.StaticCogs != null)
			{
				Cog[] startCogs = mCurrentLevelData.StaticItems.StaticCogs;
				foreach (Cog cog3 in startCogs)
				{
					if (cog3 != null)
					{
						list.Add(cog3.Asset);
					}
				}
			}
		}
		if (list.Count > 0)
		{
			mStaticItemBundleCount = list.Count;
			new RsAssetLoader().Load(list.ToArray(), null, BundleLoadEventHandler);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	private void CreateStaticItems()
	{
		if (mCurrentLevelData.StaticItems == null)
		{
			return;
		}
		Cog[] startCogs;
		if (mCurrentLevelData.StaticItems.StartCogs != null)
		{
			startCogs = mCurrentLevelData.StaticItems.StartCogs;
			foreach (Cog cog in startCogs)
			{
				if (cog != null)
				{
					SetupCog(cog);
				}
			}
		}
		if (mCurrentLevelData.StaticItems.VictoryCogs != null)
		{
			startCogs = mCurrentLevelData.StaticItems.VictoryCogs;
			foreach (Cog cog2 in startCogs)
			{
				if (cog2 != null)
				{
					SetupCog(cog2);
				}
			}
		}
		if (mCurrentLevelData.StaticItems.StaticCogs == null)
		{
			return;
		}
		startCogs = mCurrentLevelData.StaticItems.StaticCogs;
		foreach (Cog cog3 in startCogs)
		{
			if (cog3 != null)
			{
				SetupCog(cog3);
			}
		}
	}

	private void SetupCog(Cog cog)
	{
		object obj = RsResourceManager.LoadAssetFromBundle(cog.Asset, typeof(GameObject));
		if (_ActiveContainer != null)
		{
			GameObject obj2 = Object.Instantiate((GameObject)obj);
			obj2.name = cog.AssetName;
			obj2.transform.parent = _ActiveContainer;
			CogObject component = obj2.GetComponent<CogObject>();
			if (component != null)
			{
				mStaticItemBundleCount--;
				component.Setup(cog);
			}
		}
	}

	private void DestroyPreviousObjects()
	{
		Transform[] componentsInChildren = _ActiveContainer.GetComponentsInChildren<Transform>();
		if (componentsInChildren == null)
		{
			return;
		}
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (transform != null && transform != _ActiveContainer.transform)
			{
				Object.Destroy(transform.gameObject);
			}
		}
	}

	private void BundleLoadEventHandler(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			CreateStaticItems();
		}
	}

	public void LoadNextLevel()
	{
		int nextLevel = GetNextLevel();
		if (mLevelData.Levels[nextLevel].MemberOnly && !UnlockManager.IsSceneUnlocked(CogsGameManager.pInstance._GameModuleName, inShowUi: false, delegate(bool success)
		{
			if (success)
			{
				LoadNextLevel();
			}
		}))
		{
			ShowLevelMenu();
			return;
		}
		if (_LevelSelection.GetVisibility())
		{
			_LevelSelection.SetVisibility(inVisible: false);
		}
		DestroyPreviousObjects();
		mCurrentLevelIndex = nextLevel;
		LoadLevel(mCurrentLevelIndex);
	}

	public int GetNextLevel()
	{
		for (int i = mCurrentLevelIndex + 1; i < mLevelData.Levels.Length; i++)
		{
			if (!mLevelData.Levels[i].IsMissionLevel)
			{
				return i;
			}
		}
		return -1;
	}

	public void SetupLevelFromSceneObjects()
	{
		if (_ActiveContainer == null)
		{
			Debug.Log("No _ActiveContainer object found in scene");
			return;
		}
		CogObject[] componentsInChildren = _ActiveContainer.GetComponentsInChildren<CogObject>();
		if (componentsInChildren == null)
		{
			Debug.Log("No cogs Object found in scene under ActiveContainer");
			return;
		}
		CogObject[] array = componentsInChildren;
		foreach (CogObject cogObject in array)
		{
			Cog cog = new Cog();
			cog.Transform = null;
			cog.RotateDirection = cogObject._RotateDirection;
			cog.RatchetAttached = cogObject._IsRachetAttached;
			cog.AngularSpeed = cogObject._AngularSpeed;
			cog.CogType = cogObject._CogType;
			cogObject.Setup(cog);
		}
	}

	public void QuitGame()
	{
		pLevelToLoad = null;
		if (_LoadLastLevel)
		{
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
			return;
		}
		AvAvatar.SetActive(inActive: true);
		AvAvatar.pState = mPrevAvatarState;
		Object.Destroy(base.transform.root.gameObject);
	}

	public static void UnlockAllLevels()
	{
		int num = -1;
		if (mLevelData != null)
		{
			num = mLevelData.Levels.Length - 1;
			if (num != -1)
			{
				CogsLevelProgress.pInstance.SetLevelUnlocked(num);
			}
			if (pInstance != null && pInstance._LevelSelection != null && pInstance._LevelSelection.GetVisibility())
			{
				pInstance.ShowLevelMenu();
			}
		}
	}
}
