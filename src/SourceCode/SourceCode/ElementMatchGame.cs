using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ElementMatchGame : TileMatchPuzzleGame
{
	public enum PowerUpType
	{
		POWERUP_TTERROR,
		POWERUP_NADDERSPIKES
	}

	public enum ElementType
	{
		ALKALI_METAL,
		ALKALINE_EARTH_METALS,
		METALS,
		NONMETALS,
		HALOGENS,
		NOBLE_GASES,
		COMPOUND,
		SPECIAL
	}

	public enum BoosterType
	{
		BOOSTER_SPIKE,
		BOOSTER_TERROR,
		BOOSTER_MAX
	}

	[Serializable]
	public class ElementTile
	{
		public ElementType _Type;

		public LocaleString _NameText;

		public int _DropRate;

		public Color _Color;

		public Material _Material;

		public int _SpecialComboScore;

		public ElementMatchTilePiece[] _TilePiece;
	}

	[Serializable]
	public class ElementComboTileInfo : ComboTileInfo
	{
	}

	[Serializable]
	public class ElementComboTileGO : ComboTileGO
	{
		public ElementComboTileInfo[] _ComboTileInfo;
	}

	[Serializable]
	public class SpecialElementCombo
	{
		public string[] _ElementSymbols;

		public TilePiece _Resultant;

		public int _ResultTileID;

		public int _BonusPointsMultipler;
	}

	public enum GameModes
	{
		SINGLE_PLAYER,
		HEAD_TO_HEAD,
		SINGLE_PLAYER_CHALLENGE
	}

	[Serializable]
	public class DragonScaleData
	{
		[Serializable]
		public class StageScale
		{
			public RaisedPetStage _Stage = RaisedPetStage.BABY;

			public float _Scale = 1f;
		}

		public string _Name;

		public int _ID;

		public StageScale[] _StageScales;
	}

	[Serializable]
	public class Booster
	{
		public BoosterType _Type;

		public int _ItemID;
	}

	public ElementTile[] _ElementTiles;

	public ElementComboTileGO[] _ComboTiles;

	public SpecialElementCombo[] _SpecialCombos;

	public KAUiBejeweled _UI;

	private bool mSinglePlayer = true;

	private KAWidget mSelectedInfo;

	private KAWidget mSymbol;

	private KAWidget mName;

	private KAWidget mAtomicNo;

	private KAWidget mAtomicMass;

	private KAWidget mElementInfo;

	private KAWidget mCompoundInfo;

	public float _PlayAlertTime = 20f;

	public int _GameID;

	public string _GameModuleName;

	public UiDragonsEndDB _ResultUI;

	public LocaleString _NonMemberResultText = new LocaleString("");

	public LocaleString _MemberResultText = new LocaleString("");

	public LocaleString _ChallengeCompleteText = new LocaleString("");

	public LocaleString _ChallengeTryAgainText = new LocaleString("");

	private KAUIGenericDB mKAUIGenericDB;

	public LocaleString _LowDragonEnergyText;

	private int mSumProbabilities;

	private bool mWasDragging;

	private bool mIsDragging;

	private int mFingerID = -1;

	private const int mMarkerTileID = 1000;

	private ElementMatchTilePiece mStartMarker;

	private ElementMatchTilePiece mEndMarker;

	public int _PointPerTile = 2;

	public float _TotalTimeInSeconds = 600f;

	public float _MovementTimeInSeconds = 40f;

	public Booster[] _Boosters;

	public int _NadderSpikesMin = 4;

	public int _NadderSpikesMax = 8;

	public GameObject _NadderSpike;

	public ElementMatchTerribleTerror _Terror1;

	public ElementMatchTerribleTerror _Terror2;

	public float _BoosterCoolDownTime = 5f;

	private float mTotalTimeLeft;

	private float mTurnTimeLeft;

	private int mComponundsMade;

	private List<int> mNadderSpikeTile = new List<int>();

	private Dictionary<BoosterType, int> mBoosterTypeIDMap = new Dictionary<BoosterType, int>();

	private List<ElementType> mTerrorTypes = new List<ElementType>();

	private bool mActionCompleted = true;

	public GameObject _NadderAnim;

	public float _DragonScale = 2f;

	public InteractiveTutManager _TutManager;

	private float mTouchManagerSensitivityOnStart;

	public LocaleString _ComboText = new LocaleString("[[elem_count]] Element Combo with a [[elem_type]] boonus! [[points]] points!");

	public LocaleString _SpecialItemText = new LocaleString("Get the [[item_name]] to the bottom to win it");

	public LocaleString _SpecialItemCollectText = new LocaleString("The item has been added to your inventory");

	public LocaleString _CompoundCreateText = new LocaleString("You created a [[compound_name]] compound using [[c1]] [[elem1]] and [[c2]] [[elem2]]!");

	private bool mPlayedOrSkipped;

	private bool mTutGenerateH2OElements;

	public int[] _TutCompoundElement = new int[3] { 6, 7, 9 };

	private List<int> mTutCompoundIDs = new List<int>(3);

	private List<TilePiece> mAlreadyUsed = new List<TilePiece>();

	public bool _PreDefiniedTutBoard;

	private int mChallengePoints;

	public DragonScaleData[] _ScaleData;

	private bool mDoneUnmount;

	private GameModes mGameMode;

	public bool _GenCompoundTiles;

	public Transform _TerrorStart;

	public Transform _TerrorEnd;

	private int mTutBooster = 2;

	private bool mSetTutBoardDone;

	private int mTutTileRow1;

	private int mTutTileCol1;

	private int mTutTileRow2;

	private int mTutTileCol2;

	public Dictionary<BoosterType, int> pBoosterIDMap => mBoosterTypeIDMap;

	public GameModes pGameMode
	{
		get
		{
			return mGameMode;
		}
		set
		{
			mGameMode = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: false);
		}
	}

	protected override void Start()
	{
		mFingerID = -1;
		mGameState = GameState.PAUSED;
		Array.Sort(_ComboTiles);
		ComboTileGO[] comboTiles = _ComboTiles;
		mComboTiles = comboTiles;
		for (int i = 0; i < mComboTiles.Length; i++)
		{
			mComboTiles[i]._Tiles = _ComboTiles[i]._Tiles;
		}
		List<TileGO> list = new List<TileGO>();
		ElementTile[] elementTiles = _ElementTiles;
		foreach (ElementTile elementTile in elementTiles)
		{
			ElementMatchTilePiece[] tilePiece = elementTile._TilePiece;
			foreach (ElementMatchTilePiece elementMatchTilePiece in tilePiece)
			{
				TileGO tileGO = new TileGO();
				tileGO._Normal = elementMatchTilePiece;
				elementMatchTilePiece.pWeight = elementTile._DropRate + elementMatchTilePiece._DropRate;
				mSumProbabilities += elementMatchTilePiece.pWeight;
				elementMatchTilePiece._ElementType = elementTile._Type;
				list.Add(tileGO);
			}
		}
		for (int l = 0; l < _SpecialCombos.Length; l++)
		{
			_SpecialCombos[l]._Resultant.pID = _SpecialCombos[l]._ResultTileID;
		}
		for (int m = 0; m < mComboTiles.Length; m++)
		{
			mComboTiles[m]._Tiles = _ComboTiles[m]._Tiles;
		}
		_Tiles = new TileGO[list.Count];
		_Tiles = list.ToArray();
		_UI._UiCountDown.OnCountdownDone += HandleOnCountdownDone;
		GameObject gameObject = new GameObject();
		gameObject.name = "START_MARKER";
		mStartMarker = gameObject.AddComponent<ElementMatchTilePiece>();
		mStartMarker.pID = 1000;
		GameObject gameObject2 = new GameObject();
		gameObject2.name = "END_MARKER";
		mEndMarker = gameObject2.AddComponent<ElementMatchTilePiece>();
		mEndMarker.pID = 1000;
		mSelectedInfo = _UI.FindItem("SelectedTileInfo");
		mSymbol = _UI.FindItem("Symbol");
		mName = _UI.FindItem("ElementName");
		mAtomicNo = _UI.FindItem("AtmNo");
		mAtomicMass = _UI.FindItem("AtmMass");
		mCompoundInfo = _UI.FindItem("CompoundInfo");
		mElementInfo = _UI.FindItem("ElementTileInfo");
		mSelectedInfo.SetVisibility(inVisible: false);
		mComponundsMade = 0;
		if (CommonInventoryData.pIsReady && _Boosters != null)
		{
			for (int n = 0; n < _Boosters.Length; n++)
			{
				_UI.UpdateBooster(_Boosters[n]._Type, CommonInventoryData.pInstance.GetQuantity(_Boosters[n]._ItemID), forceDisable: true);
				mBoosterTypeIDMap.Add(_Boosters[n]._Type, _Boosters[n]._ItemID);
			}
		}
		ElementNadderSpike.OnActionComplete += HandleOnActionComplete;
		KAUiBejeweled.UseBooster += HandleUseBooster;
		if (null != _TutManager)
		{
			InteractiveTutManager tutManager = _TutManager;
			tutManager._StepStartedEvent = (StepStartedEvent)Delegate.Combine(tutManager._StepStartedEvent, new StepStartedEvent(TutorialStepStartEvent));
			InteractiveTutManager tutManager2 = _TutManager;
			tutManager2._StepEndedEvent = (StepEndedEvent)Delegate.Combine(tutManager2._StepEndedEvent, new StepEndedEvent(TutorialStepEndEvent));
			InteractiveTutManager tutManager3 = _TutManager;
			tutManager3._TutorialCompleteEvent = (TutorialCompleteEvent)Delegate.Combine(tutManager3._TutorialCompleteEvent, new TutorialCompleteEvent(TutorialCompleteEvent));
		}
		if (null != _UI._BoosterBuyUi)
		{
			UiBuyPopup boosterBuyUi = _UI._BoosterBuyUi;
			boosterBuyUi._OnPurchaseSuccessful = (UiBuyPopup.PurchaseSuccessfull)Delegate.Combine(boosterBuyUi._OnPurchaseSuccessful, new UiBuyPopup.PurchaseSuccessfull(OnBoosterPurchaseDone));
		}
		if (null != UiJoystick.pInstance)
		{
			KAInput.ShowJoystick(UiJoystick.pInstance.pPos, inShow: false);
		}
		if (CommonInventoryData.pIsReady && !IsTutorialComplete())
		{
			for (int num = 0; num < _Boosters.Length; num++)
			{
				if (0 >= CommonInventoryData.pInstance.GetQuantity(_Boosters[num]._ItemID))
				{
					TutAwardBooster(_Boosters[num]._Type);
				}
			}
			CommonInventoryData.pInstance.Save();
		}
		UpdateBoosterUI(forceDisable: true);
		ElementMatchSoundManager.pInstance.PlayAmbientMusic();
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.OnDragStartEvent = (OnDragStart)Delegate.Combine(TouchManager.OnDragStartEvent, new OnDragStart(OnDragStart));
		TouchManager.OnDragEndEvent = (OnDragEnd)Delegate.Combine(TouchManager.OnDragEndEvent, new OnDragEnd(OnDragEnd));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Combine(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		mTouchManagerSensitivityOnStart = TouchManager.pInstance._TouchSensitivity;
		TouchManager.pInstance._TouchSensitivity = 0f;
		base.Start();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.OnDragStartEvent = (OnDragStart)Delegate.Remove(TouchManager.OnDragStartEvent, new OnDragStart(OnDragStart));
		TouchManager.OnDragEndEvent = (OnDragEnd)Delegate.Remove(TouchManager.OnDragEndEvent, new OnDragEnd(OnDragEnd));
		TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnFingerUp));
		TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Remove(TouchManager.OnFingerDownEvent, new OnFingerDown(OnFingerDown));
		TouchManager.pInstance._TouchSensitivity = mTouchManagerSensitivityOnStart;
	}

	private void OnLevelReady()
	{
		AvAvatar.SetUIActive(inActive: false);
	}

	public override void StartGame()
	{
		if (!mPlayedOrSkipped)
		{
			base.StartGame();
		}
	}

	public void UpdateBoosterUI(bool forceDisable = false)
	{
		foreach (KeyValuePair<BoosterType, int> item in mBoosterTypeIDMap)
		{
			int quantity = CommonInventoryData.pInstance.GetQuantity(item.Value);
			_UI.UpdateBooster(item.Key, quantity, forceDisable);
		}
	}

	private void HandleUseBooster(BoosterType intype)
	{
		if (mGameState != GameState.PLAYING)
		{
			return;
		}
		UtDebug.Log("Match 3: Calling HandleUseBooster: " + intype);
		if (!mActionCompleted)
		{
			return;
		}
		if (0 < CommonInventoryData.pInstance.GetQuantity(mBoosterTypeIDMap[intype]))
		{
			if (mGridState == GridState.INTERACTIVE && !CheckMovingTile())
			{
				UtDebug.Log("Match 3: Calling HandleUseBooster: inside if");
				mActionCompleted = false;
				UtDebug.Log("Booster action started");
				switch (intype)
				{
				case BoosterType.BOOSTER_SPIKE:
					ActivateNadderSpike();
					break;
				case BoosterType.BOOSTER_TERROR:
					ActivateTerribleTerror();
					break;
				}
				if (CommonInventoryData.pIsReady)
				{
					CommonInventoryData.pInstance.RemoveItem(mBoosterTypeIDMap[intype], updateServer: true);
				}
				UpdateBoosterUI(forceDisable: true);
			}
		}
		else if (IsTutorialComplete())
		{
			mGameState = GameState.PAUSED;
			_UI.ShowBoosterBuy();
		}
	}

	private void RefreshBoosterUI()
	{
		UpdateBoosterUI();
		CancelInvoke("RefreshBoosterUI");
	}

	private void HandleOnActionComplete()
	{
		for (int i = 0; i < mNadderSpikeTile.Count; i++)
		{
			TilePiece tilePiece = mTilesMap[mNadderSpikeTile[i] / _RowCount, mNadderSpikeTile[i] % _ColumnCount];
			if (!(null == tilePiece))
			{
				mTilesMap[tilePiece.pRow, tilePiece.pColumn] = null;
				if (!tilePiece.IsInLimbo())
				{
					ElementEffects.pInstance.PlayNadderSpikeFX(tilePiece.transform.position);
					tilePiece.SetState(TilePiece.State.DEAD);
					AddAndUpdateScore(_PointPerTile);
					tilePiece = null;
				}
			}
		}
		for (int j = 0; j < mNadderSpikeTile.Count; j++)
		{
			DropTiles(mNadderSpikeTile[j] / _RowCount, mNadderSpikeTile[j] % _ColumnCount);
		}
		FillVoid(null);
		mNadderSpikeTile.Clear();
		UpdateBoosterAfterUse();
	}

	private void UpdateBoosterAfterUse()
	{
		UpdateBoosterUI(forceDisable: true);
		InvokeRepeating("BoosterActionCompleted", 2f, 0.33f);
		FillVoid(null);
	}

	private void BoosterActionCompleted()
	{
		if (mMovingTiles.Count <= 0 && !mActionCompleted)
		{
			UtDebug.Log("Booster action completed");
			mActionCompleted = true;
			UpdateBoosterUI();
			CancelInvoke("BoosterActionCompleted");
		}
	}

	protected void ActivateNadderSpike()
	{
		int num = UnityEngine.Random.Range(_NadderSpikesMin, _NadderSpikesMax);
		UtDebug.Log("Match 3: Calling ActivateNadderSpike: ");
		mNadderSpikeTile.Clear();
		while (mNadderSpikeTile.Count < num)
		{
			int item = UnityEngine.Random.Range(0, _RowCount * _ColumnCount);
			if (!mNadderSpikeTile.Contains(item))
			{
				mNadderSpikeTile.Add(item);
			}
		}
		if (mNadderSpikeTile.Count <= 0)
		{
			UtDebug.Log("Match 3: mNadderSpikeTile.Count is zero");
		}
		_NadderSpike.SetActive(value: true);
		ElementMatchSoundManager.pInstance.Play("NadderSpikeSFX");
		UpdateBoosterUI(forceDisable: true);
		if (!IsTutorialComplete())
		{
			mTutBooster--;
		}
		if (!IsTutorialComplete() && mTutBooster == 0)
		{
			_TutManager.TutorialManagerAsyncMessage("TutBoosterUsed");
		}
	}

	protected void ActivateTerribleTerror()
	{
		mTerrorTypes.Clear();
		Dictionary<ElementType, int> dictionary = new Dictionary<ElementType, int>();
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[i, j];
				if (!dictionary.ContainsKey(elementMatchTilePiece._ElementType))
				{
					dictionary.Add(elementMatchTilePiece._ElementType, 1);
				}
				else
				{
					dictionary[elementMatchTilePiece._ElementType] = dictionary[elementMatchTilePiece._ElementType] + 1;
				}
			}
		}
		int num = 2;
		foreach (ElementType key in dictionary.Keys)
		{
			ElementTile[] elementTiles = _ElementTiles;
			foreach (ElementTile elementTile in elementTiles)
			{
				if (key != elementTile._Type)
				{
					continue;
				}
				List<Transform> list = new List<Transform>();
				for (int l = 0; l < _RowCount; l++)
				{
					for (int m = 0; m < _ColumnCount; m++)
					{
						ElementMatchTilePiece elementMatchTilePiece2 = (ElementMatchTilePiece)mTilesMap[l, m];
						if (elementMatchTilePiece2._ElementType == key)
						{
							list.Add(elementMatchTilePiece2.transform);
						}
					}
				}
				mTerrorTypes.Add(key);
				if (num == 2)
				{
					_Terror1.Show(list, elementTile._Material, key);
				}
				else
				{
					_Terror2.Show(list, elementTile._Material, key);
				}
				num--;
				if (num == 0)
				{
					break;
				}
			}
			if (num == 0)
			{
				break;
			}
		}
		ElementMatchSoundManager.pInstance.Play("TerribleTerrorSFX");
		UpdateBoosterUI(forceDisable: true);
		InvokeRepeating("TerrorClearTiles", 1f, 0.33f);
		if (null != _TutManager)
		{
			mTutBooster--;
			if (mTutBooster == 0)
			{
				_TutManager.TutorialManagerAsyncMessage("TutBoosterUsed");
			}
		}
	}

	private void TerrorClearTiles()
	{
		if (_Terror1.pReachedEnd && _Terror2.pReachedEnd && mTerrorTypes.Count > 0)
		{
			ClearAllOfType(mTerrorTypes);
			UpdateBoosterAfterUse();
			mTerrorTypes.Clear();
			CancelInvoke("TerrorClearTiles");
		}
	}

	private void HandleOnCountdownDone()
	{
		StartGame();
		mGameState = GameState.PLAYING;
		_UI.pGamePlayTime = (int)_TotalTimeInSeconds;
		_UI.pTurnTime = (int)_MovementTimeInSeconds;
		mTotalTimeLeft = _TotalTimeInSeconds;
		_UI.SetTime(mTotalTimeLeft);
		mSelectedInfo.SetVisibility(inVisible: false);
		if (!IsTutorialComplete())
		{
			UpdateBoosterUI(forceDisable: true);
		}
		else
		{
			UpdateBoosterUI();
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
			}
		}
		if (ChallengeInfo.pActiveChallenge != null)
		{
			mChallengePoints = ChallengeInfo.pActiveChallenge.Points;
			if (mChallengePoints > 0)
			{
				_UI.ChallengeItemVisible(show: true);
				_UI.SetChallengeScore(mChallengePoints);
			}
		}
		else
		{
			_UI.ChallengeItemVisible(show: false);
		}
		SetGridState(GridState.INTERACTIVE);
		SetGameState(GameState.PLAYING);
	}

	public void StartTut()
	{
		if (null != _TutManager)
		{
			_TutManager.ShowTutorial();
		}
		_UI.EnableBtns();
		HandleOnCountdownDone();
	}

	private void PauseTut()
	{
		if (!CheckMovingTile())
		{
			mGridState = GridState.FREEZ;
			CancelInvoke("PauseTut");
		}
	}

	private void SetTutBoard()
	{
		if (mMovingTiles.Count != 0 || mSetTutBoardDone)
		{
			return;
		}
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				TilePiece tilePiece = mTilesMap[i, j];
				mTilesMap[i, j] = null;
				tilePiece.SetState(TilePiece.State.DEAD);
			}
		}
		GenerateTutorialBoard(1);
		mSetTutBoardDone = true;
		CancelInvoke("SetTutBoard");
	}

	public void TutorialStepStartEvent(int stepIdx, string stepName)
	{
		if (stepIdx == 0)
		{
			mTutTileRow1 = 4;
			mTutTileCol1 = 2;
			mTutTileRow2 = 3;
			mTutTileCol2 = 2;
			UpdateBoosterUI(forceDisable: true);
			string[] array = new string[3] { "O", "Ar", "Mg" };
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < _Tiles.Length; j++)
				{
					if (((ElementMatchTilePiece)_Tiles[j]._Normal)._Symbol == array[i])
					{
						mTutCompoundIDs.Add(j);
					}
				}
			}
		}
		else if (1 == stepIdx)
		{
			mTutTileRow1 = 3;
			mTutTileCol1 = 1;
			mTutTileRow2 = 3;
			mTutTileCol2 = 2;
			_UI.ShowTutorialArrow(Vector3.zero, left: false, right: false, up: false, down: false);
			ElementEffects.pInstance.ClearTutorialParticles();
		}
		else if (2 == stepIdx)
		{
			if (!IsTutorialComplete() && mTutTileRow1 != -1 && mTutTileRow2 != -1)
			{
				ElementEffects.pInstance.PlayTutorialHighLightFX(mTilesMap[mTutTileRow1, mTutTileCol1].gameObject.transform.position);
				ElementEffects.pInstance.PlayTutorialHighLightFX(mTilesMap[mTutTileRow2, mTutTileCol2].gameObject.transform.position);
				Vector3 position = mTilesMap[mTutTileRow2, mTutTileCol2].transform.position;
				_UI.ShowTutorialArrow(position, left: true, right: false, up: false, down: false);
			}
		}
		else if (3 == stepIdx)
		{
			_UI.ShowTutorialArrow(Vector3.zero, left: false, right: false, up: false, down: false);
			mTutTileRow1 = -1;
			mTutTileCol1 = -1;
			mTutTileRow2 = -1;
			mTutTileCol2 = -1;
			ElementEffects.pInstance.ClearTutorialParticles();
		}
	}

	public void TutorialStepEndEvent(int stepIdx, string stepName, bool tutQuit)
	{
		if (_TutManager != null && stepIdx >= 0)
		{
			switch (stepName)
			{
			case "DragToMatch":
				mTutGenerateH2OElements = true;
				mAlreadyUsed.Clear();
				_UI.PlayFlash(show: true);
				InvokeRepeating("PauseTut", 0.1f, 0.33f);
				break;
			case "MakeManyCombination":
				_UI.PlayFlash(show: false);
				SetGridState(GridState.INTERACTIVE);
				mGameState = GameState.PLAYING;
				break;
			case "CreatedWater":
				mTutGenerateH2OElements = false;
				InvokeRepeating("PauseTut", 0.1f, 0.33f);
				break;
			case "CreatedOther":
				SetGridState(GridState.INTERACTIVE);
				mGameState = GameState.PLAYING;
				UpdateBoosterUI();
				break;
			}
		}
	}

	protected void TutorialCompleteEvent()
	{
		mPlayedOrSkipped = true;
		mTutGenerateH2OElements = false;
		AddAndUpdateScore(-(int)mScore);
		ElementEffects.pInstance.ClearTutorialParticles();
		if (null != _UI)
		{
			_UI.ShowTutorialArrow(Vector3.zero, left: false, right: false, up: false, down: false);
		}
		OnReplayGame();
	}

	protected ElementMatchTilePiece InstantiateElementTile(TilePiece t)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(t.gameObject);
		ElementMatchTilePiece elementMatchTilePiece = null;
		if (gameObject != null)
		{
			elementMatchTilePiece = gameObject.GetComponent(typeof(ElementMatchTilePiece)) as ElementMatchTilePiece;
			if (elementMatchTilePiece == null)
			{
				elementMatchTilePiece = gameObject.AddComponent(typeof(ElementMatchTilePiece)) as ElementMatchTilePiece;
			}
			elementMatchTilePiece.transform.parent = base.transform;
			elementMatchTilePiece.SetState(TilePiece.State.IDLE);
			elementMatchTilePiece.SetVisibilityMovement(mGridBound.min.y - 2.75f, _TileMoveSpeed, _TileMinSpeed, _TileSpeedDamp);
		}
		return elementMatchTilePiece;
	}

	private void CheckTutCompoundElementsInBoard()
	{
		for (int i = 0; i < _TutCompoundElement.Length; i++)
		{
			for (int j = 0; j < _RowCount; j++)
			{
				for (int k = 0; k < _ColumnCount; k++)
				{
					if (null != mTilesMap[j, k])
					{
						ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[j, k];
						ElementMatchTilePiece elementMatchTilePiece2 = (ElementMatchTilePiece)_Tiles[_TutCompoundElement[i]]._Normal;
						if (elementMatchTilePiece._Symbol == elementMatchTilePiece2._Symbol && !mAlreadyUsed.Contains(elementMatchTilePiece))
						{
							mTutCompoundIDs.Remove(_TutCompoundElement[i]);
							mAlreadyUsed.Add(elementMatchTilePiece);
						}
					}
				}
			}
		}
	}

	private void GenTilesForCompound()
	{
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
		List<TilePiece> list = new List<TilePiece>();
		int num = 0;
		List<string> list2 = null;
		SpecialElementCombo[] specialCombos = _SpecialCombos;
		foreach (SpecialElementCombo specialElementCombo in specialCombos)
		{
			if (!IsTutorialComplete() && mTutGenerateH2OElements && specialElementCombo._ResultTileID != 9001)
			{
				continue;
			}
			List<string> list3 = new List<string>();
			list3 = specialElementCombo._ElementSymbols.ToList();
			list.Clear();
			for (int j = 0; j < specialElementCombo._ElementSymbols.Length; j++)
			{
				for (int k = 0; k < _RowCount; k++)
				{
					for (int l = 0; l < _ColumnCount; l++)
					{
						if (!(null != mTilesMap[k, l]))
						{
							continue;
						}
						ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[k, l];
						if (specialElementCombo._ElementSymbols[j] == elementMatchTilePiece._Symbol && !list.Contains(elementMatchTilePiece) && list3.Contains(elementMatchTilePiece._Symbol))
						{
							list3.Remove(elementMatchTilePiece._Symbol);
							list.Add(elementMatchTilePiece);
							if (list3.Count == 0)
							{
								break;
							}
						}
					}
					if (list3.Count == 0)
					{
						break;
					}
				}
				if (list3.Count == 0)
				{
					break;
				}
			}
			dictionary[num] = list3;
			num++;
		}
		if (dictionary.Count > 0)
		{
			int num2 = int.MaxValue;
			foreach (KeyValuePair<int, List<string>> item in dictionary)
			{
				if (item.Value.Count == 0)
				{
					num2 = item.Value.Count;
					list2 = item.Value;
					break;
				}
			}
			if (num2 != 0)
			{
				list2 = dictionary[UnityEngine.Random.Range(0, dictionary.Count)];
			}
		}
		for (int m = 0; m < _Tiles.Length; m++)
		{
			ElementMatchTilePiece elementMatchTilePiece2 = (ElementMatchTilePiece)_Tiles[m]._Normal;
			foreach (string item2 in list2)
			{
				if (elementMatchTilePiece2._Symbol == item2)
				{
					mTutCompoundIDs.Add(m);
				}
			}
		}
	}

	private TilePiece GetWeightedTile()
	{
		TilePiece result = null;
		float num = UnityEngine.Random.value * (float)mSumProbabilities;
		float num2 = 0f;
		if (mTutCompoundIDs.Count > 0)
		{
			TilePiece normal = _Tiles[mTutCompoundIDs[0]]._Normal;
			mTutCompoundIDs.RemoveAt(0);
			return normal;
		}
		TileGO[] tiles = _Tiles;
		foreach (TileGO tileGO in tiles)
		{
			ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)tileGO._Normal;
			if (tileGO._Normal.pWeight > 0)
			{
				num2 += (float)tileGO._Normal.pWeight;
				if (num < num2)
				{
					UtDebug.Log("Got: " + num + ", " + num2 + "  < " + elementMatchTilePiece._Symbol + " >");
					result = tileGO._Normal;
					break;
				}
			}
		}
		return result;
	}

	protected override TilePiece CreateNewTileAt(int inRow, int inCol, float yOff)
	{
		if (mTilesMap[inRow, inCol] != null)
		{
			Debug.LogWarning(" @@ Trying to create where object already exist !!!!r: " + inRow + " c: " + inCol + " ID: " + mTilesMap[inRow, inCol].pID);
			Debug.Break();
		}
		TilePiece weightedTile = GetWeightedTile();
		if (weightedTile == null)
		{
			return null;
		}
		ElementMatchTilePiece elementMatchTilePiece = InstantiateElementTile(weightedTile);
		elementMatchTilePiece.SetGridParams(inRow, inCol, TileType.NORMAL, (int)elementMatchTilePiece._ElementType);
		elementMatchTilePiece.name = "r_" + inRow + ", c_" + inCol;
		mTilesMap[inRow, inCol] = elementMatchTilePiece;
		elementMatchTilePiece.SetInfo();
		Vector3 position = GetPosition(inRow, inCol);
		Vector3 zero = Vector3.zero;
		zero = position;
		zero.y = mGridBound.max.y + yOff;
		elementMatchTilePiece.transform.position = zero;
		elementMatchTilePiece.RegisterBehaviourCallback(OnTileBehaviourCompleted);
		elementMatchTilePiece.MoveTo(position);
		return elementMatchTilePiece;
	}

	protected void GenerateElementsTestGrid()
	{
		int[,] array = new int[4, 4]
		{
			{ 11, 6, 12, 6 },
			{ 0, 9, 12, 9 },
			{ 9, 0, 3, 0 },
			{ 0, 9, 3, 9 }
		};
		_RowCount = array.GetLength(0);
		_ColumnCount = array.GetLength(1);
		GetGridInfo();
		mTilesMap = new TilePiece[array.GetLength(0), array.GetLength(1)];
		float num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
		float num2 = mGridBound.min.y;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				int inIdx = array[i, j];
				Vector3 position = new Vector3(num, num2 + mGridBlockSize.y * 0.5f, 0f);
				TilePiece tilePiece = InstantiateTile(inIdx);
				mTilesMap[i, j] = null;
				if (!(tilePiece == null))
				{
					mTilesMap[i, j] = tilePiece;
					ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)tilePiece;
					mTilesMap[i, j].SetGridParams(i, j, TileType.NORMAL, (int)elementMatchTilePiece._ElementType);
					tilePiece.transform.position = position;
					num += mGridBlockSize.x;
				}
			}
			num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
			num2 += mGridBlockSize.y;
		}
		DumpTileMat();
	}

	protected void GenerateTutorialBoard(int tutStep = 0)
	{
		string[,] array = new string[5, 5]
		{
			{ "Ti", "F", "Pb", "I", "Rn" },
			{ "K", "Pb", "H", "Hg", "F" },
			{ "Li", "H", "Li", "Ar", "Ni" },
			{ "Br", "Ti", "Ca", "Ag", "Xe" },
			{ "Zn", "Ne", "Ti", "Xe", "Sn" }
		};
		string[,] array2 = new string[5, 5]
		{
			{ "Ti", "O", "Pb", "H", "Rn" },
			{ "K", "Pb", "Mg", "Hg", "F" },
			{ "Li", "H", "Li", "Ar", "Ni" },
			{ "Br", "Ti", "Ca", "Ag", "Xe" },
			{ "Zn", "Ne", "Ti", "Xe", "Sn" }
		};
		if (tutStep == 1)
		{
			array = array2;
		}
		_RowCount = array.GetLength(0);
		_ColumnCount = array.GetLength(1);
		GetGridInfo();
		mTilesMap = new TilePiece[array.GetLength(0), array.GetLength(1)];
		float num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
		float num2 = mGridBound.min.y;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				int inIdx = 0;
				for (int k = 0; k < _Tiles.Length; k++)
				{
					if (((ElementMatchTilePiece)_Tiles[k]._Normal)._Symbol == array[_RowCount - (i + 1), j])
					{
						inIdx = k;
					}
				}
				Vector3 position = new Vector3(num, num2 + mGridBlockSize.y * 0.5f, 0f);
				TilePiece tilePiece = InstantiateTile(inIdx);
				mTilesMap[i, j] = null;
				if (!(tilePiece == null))
				{
					mTilesMap[i, j] = tilePiece;
					ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)tilePiece;
					mTilesMap[i, j].SetGridParams(i, j, TileType.NORMAL, (int)elementMatchTilePiece._ElementType);
					tilePiece.transform.position = position;
					num += mGridBlockSize.x;
				}
			}
			num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
			num2 += mGridBlockSize.y;
		}
		if (mTutTileRow1 != -1 && mTutTileRow2 != -1)
		{
			ElementEffects.pInstance.PlayTutorialHighLightFX(mTilesMap[mTutTileRow1, mTutTileCol1].gameObject.transform.position);
			ElementEffects.pInstance.PlayTutorialHighLightFX(mTilesMap[mTutTileRow2, mTutTileCol2].gameObject.transform.position);
		}
		Vector3 position2 = mTilesMap[mTutTileRow1, mTutTileCol1].transform.position;
		_UI.ShowTutorialArrow(position2, left: false, right: false, up: false, down: true);
	}

	protected override void ResetGrid()
	{
		if (_DebugGenPredefineGrid)
		{
			GenerateElementsTestGrid();
			return;
		}
		if (null != _TutManager && !_TutManager.TutorialComplete() && _PreDefiniedTutBoard)
		{
			GenerateTutorialBoard();
			return;
		}
		mTilesMap = new TilePiece[_RowCount, _ColumnCount];
		float num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
		float num2 = mGridBound.min.y;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				int num3 = -1;
				mTilesMap[i, j] = null;
				int horizontalMatchSize;
				int verticalMatchSize;
				do
				{
					num3 = GetRandomTileIdx();
					if (i == 0 && j == 0 && null != _TutManager && !_TutManager.TutorialComplete())
					{
						num3 = 7;
					}
					ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)_Tiles[num3]._Normal;
					horizontalMatchSize = GetHorizontalMatchSize(i, j, (int)elementMatchTilePiece._ElementType);
					verticalMatchSize = GetVerticalMatchSize(i, j, (int)elementMatchTilePiece._ElementType);
				}
				while (horizontalMatchSize >= _MinMatchCount - 1 || verticalMatchSize >= _MinMatchCount - 1);
				Vector3 position = new Vector3(num, num2 + mGridBlockSize.y * 0.5f, 0f);
				TilePiece tilePiece = InstantiateTile(num3);
				if (!(tilePiece == null))
				{
					mTilesMap[i, j] = tilePiece;
					ElementMatchTilePiece elementMatchTilePiece2 = (ElementMatchTilePiece)_Tiles[num3]._Normal;
					mTilesMap[i, j].SetGridParams(i, j, TileType.NORMAL, (int)elementMatchTilePiece2._ElementType);
					elementMatchTilePiece2.SetInfo();
					tilePiece.transform.position = position;
					num += mGridBlockSize.x;
				}
			}
			num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
			num2 += mGridBlockSize.y;
		}
	}

	private void SwapOverlapTile(ref int inRow, ref int inCol)
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tilePiece = mTilesMap[inRow, inCol];
		int pRow = tileAtIndex.pRow;
		int pColumn = tileAtIndex.pColumn;
		Vector3 position = GetPosition(pRow, pColumn);
		tilePiece.MoveTo(position);
		tileAtIndex.transform.localPosition = GetPosition(inRow, inCol);
		mMovingTiles.Add(tilePiece);
		mTilesMap[pRow, pColumn] = null;
		mTilesMap[inRow, inCol] = null;
		mTilesMap[pRow, pColumn] = tilePiece;
		mTilesMap[inRow, inCol] = tileAtIndex;
		tilePiece.SetRowCol(pRow, pColumn);
		tileAtIndex.SetRowCol(inRow, inCol);
	}

	private bool IsDragging()
	{
		return mIsDragging;
	}

	protected override void OnTileClick()
	{
		if (null != KAUI._GlobalExclusiveUI)
		{
			return;
		}
		if (ValidateClick())
		{
			OnValidMove();
		}
		base.OnTileClick();
		if (IsDragging())
		{
			return;
		}
		if (mWasDragging)
		{
			mSelectedInfo.SetVisibility(inVisible: false);
			_UI.SetTurnTime(0f);
			mWasDragging = false;
			TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
			if (tileAtIndex != null)
			{
				if (tileAtIndex.pState != TilePiece.State.MOVE_FALL)
				{
					tileAtIndex.MoveTo(GetPosition(tileAtIndex.pRow, tileAtIndex.pColumn));
				}
				SetGridState(GridState.SWAPPING);
			}
		}
		else if (mGridState != 0 && !Input.GetMouseButton(0))
		{
			ResetTileSelection();
			SetGridState(GridState.INTERACTIVE);
		}
	}

	protected override void SwapnTransition()
	{
	}

	protected override bool ValidateClick()
	{
		if (mClickCount < 1 || !IsDragging())
		{
			return base.ValidateClick();
		}
		return true;
	}

	public bool IsTutorialComplete()
	{
		if (null != _TutManager && !_TutManager.TutorialComplete())
		{
			return false;
		}
		return true;
	}

	protected override void OnValidMove()
	{
		if (base.ValidateClick())
		{
			base.OnValidMove();
			return;
		}
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		int outRow;
		int outCol;
		bool rowColFromMousePos = GetRowColFromMousePos(out outRow, out outCol);
		bool flag = CanMoveTutorialTile(tileAtIndex.pRow, tileAtIndex.pColumn) && CanMoveTutorialTile(outRow, outCol);
		if (rowColFromMousePos && (tileAtIndex.pRow != outRow || tileAtIndex.pColumn != outCol) && (IsTutorialComplete() || flag))
		{
			mWasDragging = true;
			SwapOverlapTile(ref outRow, ref outCol);
		}
	}

	protected override void OnTileFalling()
	{
		if (!CheckMovingTile())
		{
			SetGridState(GridState.INTERACTIVE);
		}
	}

	protected override bool IsInSameSlot()
	{
		return true;
	}

	protected override bool IsValidMove()
	{
		return false;
	}

	protected override void Update()
	{
		if (mGameMode == GameModes.HEAD_TO_HEAD && !mDoneUnmount && null != SanctuaryManager.pCurPetInstance)
		{
			SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.SCIENCE_LAB);
			if (SanctuaryManager.pCurPetInstance.pIsMounted)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
		}
		if (null != SanctuaryManager.pCurPetInstance && !SanctuaryManager.pCurPetInstance.pIsMounted)
		{
			DragonScaleData[] scaleData = _ScaleData;
			foreach (DragonScaleData dragonScaleData in scaleData)
			{
				if (dragonScaleData._ID != SanctuaryManager.pCurPetInstance.pData.PetTypeID)
				{
					continue;
				}
				DragonScaleData.StageScale[] stageScales = dragonScaleData._StageScales;
				foreach (DragonScaleData.StageScale stageScale in stageScales)
				{
					if (SanctuaryManager.pCurPetInstance.pData.pStage == stageScale._Stage)
					{
						SanctuaryManager.pCurPetInstance.transform.localScale = Vector3.one * stageScale._Scale;
						break;
					}
				}
			}
			mDoneUnmount = true;
		}
		if (mGameState == GameState.PLAYING && mTotalTimeLeft > 0.01f)
		{
			if (null != _UI)
			{
				_UI.SetTime(mTotalTimeLeft);
			}
			if (null == _TutManager || !_TutManager.IsShowingTutorial())
			{
				mTotalTimeLeft -= Time.smoothDeltaTime;
				if ((double)mTotalTimeLeft <= 0.01)
				{
					GameOver();
				}
				else if (mTotalTimeLeft <= _PlayAlertTime)
				{
					ElementMatchSoundManager.pInstance.PlayTimeLow();
				}
			}
			if (mIsDragging)
			{
				mTurnTimeLeft -= Time.deltaTime;
				_UI.SetTurnTime(mTurnTimeLeft);
				if (mTurnTimeLeft < 0.1f)
				{
					mIsDragging = false;
					OnDragExpire();
				}
			}
		}
		base.Update();
	}

	protected override void OnTileCombination(int inTileID, int inMatchCount, List<ComboTileInfo> inComboTiles)
	{
		if (mComboTiles == null)
		{
			return;
		}
		ComboTileInfo comboTileInfo = null;
		for (int i = 0; i < mComboTiles.Length; i++)
		{
			if (inMatchCount < mComboTiles[i]._MatchCount)
			{
				continue;
			}
			for (int j = 0; j < mComboTiles[i]._SpawnCount; j++)
			{
				comboTileInfo = GetRandomComboTile(i);
				if (comboTileInfo != null)
				{
					inComboTiles.Add(comboTileInfo);
				}
			}
			break;
		}
	}

	protected override void OnTileSwaping()
	{
		bool flag = false;
		for (int i = 0; i < mMovingTiles.Count; i++)
		{
			if (!mMovingTiles[i].IsInState(mFallingTileStateCheck))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			mMovingTiles.Clear();
			ClearSwapMatch();
			ResetClick();
			SetGridState(GridState.FALLING);
		}
	}

	public void OnDragExpire()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		if (tileAtIndex != null)
		{
			if (tileAtIndex.pState != TilePiece.State.MOVE_FALL)
			{
				tileAtIndex.MoveTo(GetPosition(tileAtIndex.pRow, tileAtIndex.pColumn));
			}
			SetGridState(GridState.SWAPPING);
			mSelectedInfo.SetVisibility(inVisible: false);
		}
		if (null != _UI)
		{
			_UI.SetTurnTime(0f);
		}
	}

	private void FindCompoundCombosVert(List<TilePiece> matchingTile)
	{
		List<string> list = new List<string>();
		SpecialElementCombo[] specialCombos = _SpecialCombos;
		foreach (SpecialElementCombo specialElementCombo in specialCombos)
		{
			int c = 0;
			while (c < _ColumnCount)
			{
				int num2;
				for (int k = 0; k < _RowCount; k++)
				{
					if (!(null != mTilesMap[k, c]) || mTilesMap[k, c].pType != TileType.NORMAL || !(((ElementMatchTilePiece)mTilesMap[k, c])._Symbol == specialElementCombo._ElementSymbols[0]))
					{
						continue;
					}
					int num = Mathf.Max(0, k - (specialElementCombo._ElementSymbols.Length - 1));
					for (int l = num; l < num + specialElementCombo._ElementSymbols.Length; l++)
					{
						if (l + (specialElementCombo._ElementSymbols.Length - 1) >= _ColumnCount)
						{
							continue;
						}
						list.Clear();
						for (int m = 0; m < specialElementCombo._ElementSymbols.Length; m++)
						{
							list.Add(specialElementCombo._ElementSymbols[m]);
						}
						List<TilePiece> list2 = new List<TilePiece>();
						int i;
						for (i = l; i < l + specialElementCombo._ElementSymbols.Length; i = num2)
						{
							if (null != mTilesMap[i, c] && mTilesMap[i, c].pType == TileType.NORMAL && null == matchingTile.Find((TilePiece item) => item.pID == mTilesMap[i, c].pID))
							{
								ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[i, c];
								list.Remove(elementMatchTilePiece._Symbol);
								list2.Add(mTilesMap[i, c]);
							}
							num2 = i + 1;
						}
						if (list.Count == 0)
						{
							matchingTile.Add(mStartMarker);
							matchingTile.AddRange(list2);
							matchingTile.Add(mEndMarker);
						}
					}
					list.Clear();
				}
				num2 = c + 1;
				c = num2;
			}
		}
	}

	private void FindCompoundCombosHorz(List<TilePiece> matchingTile)
	{
		List<string> list = new List<string>();
		SpecialElementCombo[] specialCombos = _SpecialCombos;
		foreach (SpecialElementCombo specialElementCombo in specialCombos)
		{
			int r = 0;
			while (r < _RowCount)
			{
				int num2;
				for (int k = 0; k < _ColumnCount; k++)
				{
					if (!(null != mTilesMap[r, k]) || mTilesMap[r, k].pType != TileType.NORMAL || !(((ElementMatchTilePiece)mTilesMap[r, k])._Symbol == specialElementCombo._ElementSymbols[0]))
					{
						continue;
					}
					int num = Mathf.Max(0, k - (specialElementCombo._ElementSymbols.Length - 1));
					for (int l = num; l < num + specialElementCombo._ElementSymbols.Length; l++)
					{
						if (l + (specialElementCombo._ElementSymbols.Length - 1) >= _ColumnCount)
						{
							continue;
						}
						list.Clear();
						for (int m = 0; m < specialElementCombo._ElementSymbols.Length; m++)
						{
							list.Add(specialElementCombo._ElementSymbols[m]);
						}
						List<TilePiece> list2 = new List<TilePiece>();
						int i;
						for (i = l; i < l + specialElementCombo._ElementSymbols.Length; i = num2)
						{
							if (null != mTilesMap[r, i] && mTilesMap[r, i].pType == TileType.NORMAL && null == matchingTile.Find((TilePiece item) => item.pID == mTilesMap[r, i].pID))
							{
								ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[r, i];
								list.Remove(elementMatchTilePiece._Symbol);
								list2.Add(mTilesMap[r, i]);
							}
							num2 = i + 1;
						}
						if (list.Count == 0)
						{
							matchingTile.Add(mStartMarker);
							matchingTile.AddRange(list2);
							matchingTile.Add(mEndMarker);
						}
					}
					list.Clear();
				}
				num2 = r + 1;
				r = num2;
			}
		}
	}

	protected override void StoreMatchingTile(List<TilePiece> inMatchTileLst)
	{
		TilePiece[] matchingTiles = mSwapInfo.GetMatchingTiles();
		inMatchTileLst.Add(mStartMarker);
		for (int i = 0; i < matchingTiles.Length; i++)
		{
			inMatchTileLst.Add(matchingTiles[i]);
		}
		inMatchTileLst.Add(mEndMarker);
	}

	protected override void ClearSwapMatch()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		List<TilePiece> list = new List<TilePiece>();
		if (tileAtIndex != null)
		{
			if (tileAtIndex.pType == TileType.EXPLODE)
			{
				ResetClick();
				ExecuteTileBehaviour(tileAtIndex);
				tileAtIndex = null;
				FillVoid(null);
			}
			else
			{
				GatherMatchingTiles(tileAtIndex.pRow, tileAtIndex.pColumn, tileAtIndex.pID, list);
			}
		}
		if (tileAtIndex2 != null)
		{
			GatherMatchingTiles(tileAtIndex2.pRow, tileAtIndex2.pColumn, tileAtIndex2.pID, list);
		}
		List<TilePiece> tileMatchList = GetTileMatchList();
		list.AddRange(tileMatchList);
		ReplaceMatchedTiles(list);
		tileMatchList.Clear();
		list.Clear();
		list = null;
	}

	private List<TilePiece> RemoveJunk(List<TilePiece> tileLst)
	{
		List<TilePiece> list = new List<TilePiece>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < tileLst.Count; i++)
		{
			if (tileLst[i].pID != mStartMarker.pID)
			{
				continue;
			}
			List<TilePiece> list3 = new List<TilePiece>();
			TilePiece item = tileLst[i];
			i++;
			string text = "";
			for (; tileLst[i].pID != mStartMarker.pID; i++)
			{
				list3.Add(tileLst[i]);
			}
			list3.Sort(delegate(TilePiece a, TilePiece b)
			{
				int num = a.pRow.CompareTo(b.pRow);
				return (num != 0) ? num : a.pColumn.CompareTo(b.pColumn);
			});
			foreach (TilePiece item2 in list3)
			{
				text = text + item2.pRow + item2.pColumn;
			}
			list3.Insert(0, item);
			list3.Add(tileLst[i]);
			if (!list2.Contains(text))
			{
				list2.Add(text);
				list.AddRange(list3);
			}
		}
		return list;
	}

	private List<TilePiece> MergeShapes(List<TilePiece> tileLst)
	{
		List<TilePiece> list = new List<TilePiece>();
		List<List<TilePiece>> list2 = new List<List<TilePiece>>();
		for (int i = 0; i < tileLst.Count; i++)
		{
			if (tileLst[i].pID != mStartMarker.pID)
			{
				continue;
			}
			List<TilePiece> list3 = new List<TilePiece>();
			for (i++; tileLst[i].pID != mStartMarker.pID; i++)
			{
				list3.Add(tileLst[i]);
			}
			bool flag = true;
			for (int j = 0; j < list2.Count; j++)
			{
				if (list3.Intersect(list2[j]).Count() > 0)
				{
					list2[j].Union(list3);
					flag = false;
				}
			}
			if (flag)
			{
				list2.Add(list3);
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			list.Add(mStartMarker);
			list.AddRange(list2[k]);
			list.Add(mStartMarker);
		}
		return list;
	}

	protected List<TilePiece> GetTileMatchList()
	{
		List<TilePiece> list = new List<TilePiece>();
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				TilePiece tilePiece = mTilesMap[i, j];
				if (tilePiece != null && !mMovingTiles.Contains(tilePiece))
				{
					GatherMatchingTiles(tilePiece.pRow, tilePiece.pColumn, tilePiece.pID, list);
				}
			}
		}
		return RemoveJunk(list);
	}

	private ComboTileInfo ProcessCompound(List<TilePiece> inCompoundTiles)
	{
		SpecialElementCombo[] specialCombos = _SpecialCombos;
		foreach (SpecialElementCombo specialElementCombo in specialCombos)
		{
			if (inCompoundTiles.Count != specialElementCombo._ElementSymbols.Length)
			{
				continue;
			}
			List<string> list = new List<string>();
			for (int j = 0; j < specialElementCombo._ElementSymbols.Length; j++)
			{
				list.Add(specialElementCombo._ElementSymbols[j]);
			}
			ElementMatchTilePiece elementMatchTilePiece = null;
			for (int k = 0; k < specialElementCombo._ElementSymbols.Length; k++)
			{
				elementMatchTilePiece = inCompoundTiles[k] as ElementMatchTilePiece;
				if (null != inCompoundTiles[k])
				{
					list.Remove(elementMatchTilePiece._Symbol);
				}
			}
			if (list.Count == 0 && elementMatchTilePiece != null)
			{
				return new ComboTileInfo
				{
					_Tile = specialElementCombo._Resultant
				};
			}
		}
		return null;
	}

	public bool CanClick()
	{
		if (mGameState == GameState.PLAYING)
		{
			return mSwapInfo != null;
		}
		return false;
	}

	protected override void ReplaceMatchedTiles(List<TilePiece> inMatchingTiles)
	{
		FindCompoundCombosHorz(inMatchingTiles);
		FindCompoundCombosVert(inMatchingTiles);
		if (inMatchingTiles == null || inMatchingTiles.Count <= 0)
		{
			return;
		}
		if (_Hint != null)
		{
			_Hint.Reset();
		}
		inMatchingTiles = RemoveJunk(inMatchingTiles);
		Dictionary<int, int> inTileCombo = new Dictionary<int, int>();
		mElaspedTime = 0f;
		List<ComboTileInfo> list = new List<ComboTileInfo>();
		for (int i = 0; i < inMatchingTiles.Count; i++)
		{
			TilePiece tilePiece = inMatchingTiles[i];
			if (tilePiece.IsInLimbo() && tilePiece.pID != 1000)
			{
				continue;
			}
			if (inMatchingTiles[i].pID == 1000)
			{
				if (inMatchingTiles[i].pID != 1000)
				{
					mTilesMap[inMatchingTiles[i].pRow, inMatchingTiles[i].pColumn] = null;
				}
				i++;
				List<TilePiece> list2 = new List<TilePiece>();
				for (; inMatchingTiles[i].pID != 1000; i++)
				{
					list2.Add(inMatchingTiles[i]);
					ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)inMatchingTiles[i];
					mTilesMap[elementMatchTilePiece.pRow, elementMatchTilePiece.pColumn] = null;
					if (!inTileCombo.ContainsKey(tilePiece.pID))
					{
						inTileCombo[tilePiece.pID] = 0;
					}
					Dictionary<int, int> dictionary = inTileCombo;
					int pID = tilePiece.pID;
					int value = dictionary[pID] + 1;
					dictionary[pID] = value;
				}
				if (inMatchingTiles[i].pID != 1000)
				{
					mTilesMap[inMatchingTiles[i].pRow, inMatchingTiles[i].pColumn] = null;
				}
				ComboTileInfo comboTileInfo = ProcessCompound(list2);
				if (list2.Count >= 2)
				{
					if (comboTileInfo == null)
					{
						ElementTile elementTile = null;
						ElementMatchTilePiece elementMatchTilePiece2 = (ElementMatchTilePiece)list2[0];
						for (int j = 0; j < _ElementTiles.Length; j++)
						{
							if (_ElementTiles[j]._Type == elementMatchTilePiece2._ElementType)
							{
								elementTile = _ElementTiles[j];
								break;
							}
						}
						string localizedString = _ComboText.GetLocalizedString();
						localizedString = localizedString.Replace("[[ELEM_COUNT]]", list2.Count.ToString());
						localizedString = localizedString.Replace("[[ELEM_TYPE]]", elementTile._NameText.GetLocalizedString());
						string newValue = "";
						for (int k = 0; k < _ScoreInfo.Length; k++)
						{
							if (list2.Count >= _ScoreInfo[k]._TileMatchCount)
							{
								newValue = _ScoreInfo[k]._Score.ToString();
								break;
							}
						}
						localizedString = localizedString.Replace("[[POINTS]]", newValue);
						if (IsTutorialComplete())
						{
							_UI.ShowInstructText(localizedString);
						}
						if (null != _TutManager && _TutManager.IsShowingTutorial())
						{
							_TutManager.SendMessage("TutorialManagerAsyncMessage", "MatchDone");
						}
						ElementMatchSoundManager.pInstance.Play("Match", ElementMatchSoundManager.pInstance.SFX_POOL);
					}
					else
					{
						string localizedString2 = _CompoundCreateText.GetLocalizedString();
						ElementMatchTilePiece elementMatchTilePiece3 = (ElementMatchTilePiece)comboTileInfo._Tile;
						localizedString2 = localizedString2.Replace("[[COMPOUND_NAME]]", elementMatchTilePiece3._NameText.GetLocalizedString());
						Dictionary<string, int> dictionary2 = new Dictionary<string, int>();
						for (int l = 0; l < list2.Count; l++)
						{
							ElementMatchTilePiece elementMatchTilePiece4 = (ElementMatchTilePiece)list2[l];
							if (dictionary2.ContainsKey(elementMatchTilePiece4._NameText.GetLocalizedString()))
							{
								dictionary2[elementMatchTilePiece4._NameText.GetLocalizedString()]++;
							}
							else
							{
								dictionary2.Add(elementMatchTilePiece4._NameText.GetLocalizedString(), 1);
							}
						}
						int num = 1;
						foreach (KeyValuePair<string, int> item in dictionary2)
						{
							string oldValue = "[[C" + num + "]]";
							string oldValue2 = "[[ELEM" + num + "]]";
							localizedString2 = localizedString2.Replace(oldValue, item.Value.ToString());
							localizedString2 = localizedString2.Replace(oldValue2, item.Key);
							num++;
						}
						if (IsTutorialComplete())
						{
							_UI.ShowInstructText(localizedString2);
						}
						ElementMatchSoundManager.pInstance.Play("CompoundMatch", ElementMatchSoundManager.pInstance.SFX_POOL);
						if (null != _TutManager && _TutManager.IsShowingTutorial() && 9001 == elementMatchTilePiece3.pID)
						{
							_TutManager.TutorialManagerAsyncMessage("TutCompoundCreated");
						}
						mComponundsMade++;
					}
				}
				list.Add(comboTileInfo);
			}
			if (tilePiece.pID != 1000)
			{
				mTilesMap[tilePiece.pRow, tilePiece.pColumn] = null;
			}
		}
		inMatchingTiles.RemoveAll((TilePiece item) => item.pID == 1000);
		for (int m = 0; m < inMatchingTiles.Count; m++)
		{
			TilePiece tilePiece2 = inMatchingTiles[m];
			if (!tilePiece2.IsInLimbo())
			{
				if (tilePiece2.pID != 1000)
				{
					ExecuteTileBehaviour(tilePiece2);
					DropTiles(tilePiece2.pRow, tilePiece2.pColumn);
				}
				if (!tilePiece2.IsInLimbo())
				{
					ElementEffects.pInstance.PlayTileBreakFX(tilePiece2.transform.position);
					tilePiece2.SetState(TilePiece.State.DEAD);
					tilePiece2 = null;
				}
			}
		}
		inMatchingTiles = null;
		UpdateScore(ref inTileCombo);
		foreach (KeyValuePair<int, int> item2 in inTileCombo)
		{
			OnTileCombination(item2.Key, item2.Value, list);
		}
		FillVoid(list);
		list.Clear();
		inTileCombo.Clear();
		list = null;
		inTileCombo = null;
		mTurnTimeLeft = 0f;
		_UI.SetTurnTime(mTurnTimeLeft);
	}

	protected void CheckGridForMatch()
	{
		List<TilePiece> list = new List<TilePiece>();
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				TilePiece tilePiece = mTilesMap[i, j];
				GatherMatchingTiles(tilePiece.pRow, tilePiece.pColumn, tilePiece.pID, list);
			}
		}
		ReplaceMatchedTiles(list);
		list.Clear();
		list = null;
	}

	public void OnFingerDown(int inFingerId, Vector2 inVecPosition)
	{
		if (CanClick() && mSwapInfo.GetTileAtIndex(0) != null && IsGridInterActive())
		{
			mIsDragging = true;
		}
	}

	public void OnFingerUp(int inFingerID, Vector2 inVecPosition)
	{
		if (CanClick())
		{
			if (mSwapInfo.GetTileAtIndex(0) != null && IsGridInterActive())
			{
				mIsDragging = false;
			}
			mTurnTimeLeft = 0f;
			_UI.SetTurnTime(mTurnTimeLeft);
		}
	}

	public void OnDragStart(Vector2 inPosition, int inFingerID)
	{
		if (mFingerID == -1)
		{
			mFingerID = inFingerID;
			mIsDragging = true;
		}
		if (!CanClick())
		{
			return;
		}
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		if (tileAtIndex != null && IsGridInterActive() && CanMoveTutorialTile(tileAtIndex.pRow, tileAtIndex.pColumn))
		{
			inPosition.y = (float)Screen.height - inPosition.y;
			Vector3 worldPos = TileMatchPuzzleGame.pInstance.GetWorldPos(inPosition);
			int row = GetRow(worldPos.y);
			int col = GetCol(worldPos.x);
			if (row == tileAtIndex.pRow && col == tileAtIndex.pColumn)
			{
				mWasDragging = true;
			}
		}
	}

	public virtual void OnDragEnd(Vector2 inPosition, int inFingerID)
	{
		if (inFingerID == mFingerID)
		{
			mFingerID = -1;
			mIsDragging = false;
		}
	}

	public bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (!CanClick())
		{
			return false;
		}
		if (IsDragging() && mWasDragging && (mGridState == GridState.INTERACTIVE || mGridState == GridState.TILE_SELECTED))
		{
			TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
			if (tileAtIndex != null)
			{
				Vector3 worldPos = GetWorldPos(Input.mousePosition);
				if (mGridBound.Contains(worldPos))
				{
					tileAtIndex.transform.localPosition = worldPos;
				}
			}
		}
		return false;
	}

	public override void SetGridState(GridState inState)
	{
		if (inState == GridState.TILE_SELECTED)
		{
			SetSelectedInfo();
			mTurnTimeLeft = _MovementTimeInSeconds;
		}
		base.SetGridState(inState);
	}

	private void SetSelectedInfo()
	{
		if (null != mSwapInfo.GetTileAtIndex(0))
		{
			if (mSwapInfo.GetTileAtIndex(0).pType == TileType.NORMAL)
			{
				ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mSwapInfo.GetTileAtIndex(0);
				mSelectedInfo.SetVisibility(inVisible: true);
				mCompoundInfo.SetVisibility(inVisible: false);
				mElementInfo.SetVisibility(inVisible: true);
				mSymbol.SetText(elementMatchTilePiece._Symbol);
				mName.SetText(elementMatchTilePiece._NameText.GetLocalizedString());
				mAtomicMass.SetText(elementMatchTilePiece._AtomicMass);
				mAtomicNo.SetText(elementMatchTilePiece._AtomicNumber);
				mCompoundInfo.SetText(string.Empty);
			}
			else if (mSwapInfo.GetTileAtIndex(0).pType == TileType.EXPLODE)
			{
				ComboTileBase comboTileBase = (ComboTileBase)mSwapInfo.GetTileAtIndex(0);
				mSelectedInfo.SetVisibility(inVisible: true);
				mElementInfo.SetVisibility(inVisible: false);
				mCompoundInfo.SetVisibility(inVisible: true);
				mCompoundInfo.SetText(comboTileBase._CompoundInfo.GetLocalizedString().Replace("\\n", "\n"));
				mSymbol.SetText(string.Empty);
				mName.SetText(string.Empty);
				mAtomicMass.SetText(string.Empty);
				mAtomicNo.SetText(string.Empty);
			}
		}
	}

	private void ClearAllOfType(List<ElementType> inTypes)
	{
		List<TilePiece> list = new List<TilePiece>();
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				if (mTilesMap[i, j].pType != TileType.NORMAL)
				{
					continue;
				}
				ElementMatchTilePiece elementMatchTilePiece = (ElementMatchTilePiece)mTilesMap[i, j];
				for (int k = 0; k < inTypes.Count; k++)
				{
					if (inTypes[k] == elementMatchTilePiece._ElementType)
					{
						list.Add(mTilesMap[i, j]);
						mTilesMap[i, j] = null;
					}
				}
			}
		}
		for (int l = 0; l < list.Count; l++)
		{
			TilePiece tilePiece = list[l];
			DropTiles(tilePiece.pRow, tilePiece.pColumn);
			if (!tilePiece.IsInLimbo())
			{
				AddAndUpdateScore(_PointPerTile);
				tilePiece.SetState(TilePiece.State.DEAD);
				tilePiece = null;
			}
		}
	}

	private void AddAndUpdateScore(int scoreDelta)
	{
		mScore += scoreDelta;
		if (mOnScoreUpdateEvent != null)
		{
			mOnScoreUpdateEvent(mScore);
		}
	}

	public override void ClearTilesInRange(TilePiece inTile, int inStartRow, int inStartCol, int inEndRow, int inEndCol, GameObject inEffect = null)
	{
		inStartRow = Mathf.Clamp(inStartRow, 0, _RowCount - 1);
		inEndRow = Mathf.Clamp(inEndRow, 0, _RowCount - 1);
		inStartCol = Mathf.Clamp(inStartCol, 0, _ColumnCount - 1);
		inEndCol = Mathf.Clamp(inEndCol, 0, _ColumnCount - 1);
		int num = inEndRow - inStartRow;
		int num2 = inEndCol - inStartCol;
		List<TilePiece> list = new List<TilePiece>();
		num++;
		num2++;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num4 = inStartRow + i;
				int num5 = inStartCol + j;
				if (mTilesMap[num4, num5] != null)
				{
					TilePiece tilePiece = mTilesMap[num4, num5];
					if (tilePiece.pType != TileType.NORMAL && tilePiece != inTile)
					{
						list.Add(tilePiece);
						mTilesMap[num4, num5] = null;
						continue;
					}
					if (9000 == inTile.pID)
					{
						ElementEffects.pInstance.PlaySaltFX(tilePiece.transform.position);
					}
					if (9001 == inTile.pID)
					{
						ElementEffects.pInstance.PlayWaterFX(tilePiece.transform.position);
					}
					if (9002 == inTile.pID)
					{
						ElementEffects.pInstance.PlayCO2FX(tilePiece.transform.position);
					}
					tilePiece.SetState(TilePiece.State.DEAD);
					tilePiece = null;
					mTilesMap[num4, num5] = null;
					num3++;
				}
				mTilesMap[num4, num5] = null;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			TilePiece tilePiece2 = list[k];
			ExecuteTileBehaviour(tilePiece2);
			tilePiece2.SetState(TilePiece.State.DEAD);
			tilePiece2 = null;
			num3++;
		}
		int num6 = 1;
		for (int l = 0; l < _SpecialCombos.Length; l++)
		{
			if (_SpecialCombos[l]._ResultTileID == inTile.pID)
			{
				num6 = _SpecialCombos[l]._BonusPointsMultipler;
			}
		}
		AddAndUpdateScore((num3 - 1) * _PointPerTile * num6);
		list.Clear();
		list = null;
		for (int m = 0; m < num; m++)
		{
			for (int n = 0; n < num2; n++)
			{
				int inRow = inStartRow + m;
				int inCol = inStartCol + n;
				DropTiles(inRow, inCol);
			}
		}
	}

	protected override void ExecuteTileBehaviour(TilePiece inTile)
	{
		if (inTile.pType != TileType.EXPLODE)
		{
			return;
		}
		int[] pExplodeRange = inTile.pExplodeRange;
		int num = pExplodeRange.Length;
		for (int i = 0; i < num; i += 4)
		{
			ClearTilesInRange(inTile, pExplodeRange[i], pExplodeRange[i + 1], pExplodeRange[i + 2], pExplodeRange[i + 3]);
		}
		if (9000 == inTile.pID)
		{
			ElementMatchSoundManager.pInstance.Play("SaltSFX");
		}
		if (9001 == inTile.pID)
		{
			ElementMatchSoundManager.pInstance.Play("WaterSFX");
			if (null != _TutManager && _TutManager.IsShowingTutorial())
			{
				_TutManager.TutorialManagerAsyncMessage("TutCompoundUsed");
			}
		}
		if (9002 == inTile.pID)
		{
			ElementMatchSoundManager.pInstance.Play("CO2SFX");
		}
		if ((bool)inTile)
		{
			pExplodeRange = null;
		}
	}

	private void GameOver()
	{
		ElementMatchSoundManager.pInstance.StopTimeLow();
		UpdateBoosterUI(forceDisable: true);
		_UI.DisableBtns();
		_UI.ClearMessageQueue();
		mSelectedInfo.SetVisibility(inVisible: false);
		InvokeRepeating("ShowEndScreen", 0.2f, 0.33f);
	}

	private void ShowEndScreen()
	{
		if (mActionCompleted)
		{
			EnableGameEndScreen();
			CancelInvoke("ShowEndScreen");
		}
	}

	private void SetChallengeText(string inText)
	{
		_ResultUI.SetRewardMessage(inText);
	}

	private void EnableGameEndScreen()
	{
		int num = (int)mScore;
		UpdateBoosterUI(forceDisable: true);
		mGameState = GameState.PAUSED;
		mTurnTimeLeft = 0f;
		_UI.SetTurnTime(mTurnTimeLeft);
		ElementMatchSoundManager.pInstance.Play("GameOverSFX");
		OnDragExpire();
		mGameState = GameState.PAUSED;
		UiChallengeInvite.SetData(_GameID, 1, 0, num);
		HighScores.SetCurrentGameSettings(_GameModuleName, _GameID, !mSinglePlayer, 0, 1);
		HighScores.AddGameData("highscore", num.ToString());
		_ResultUI.SetHighScoreData(num, "highscore");
		_ResultUI.SetVisibility(Visibility: true);
		_ResultUI.SetGameSettings(_GameModuleName, base.gameObject, "any");
		_ResultUI.SetResultData("Data0", null, mScore.ToString());
		_ResultUI.SetResultData("Data1", null, mComponundsMade.ToString());
		mComponundsMade = 0;
		if (SubscriptionInfo.pIsMember)
		{
			_ResultUI.SetRewardMessage(_MemberResultText.GetLocalizedString());
		}
		else
		{
			_ResultUI.SetRewardMessage(_NonMemberResultText.GetLocalizedString());
		}
		KAUICursorManager.SetDefaultCursor("Loading");
		string text = _GameModuleName;
		if (SubscriptionInfo.pIsMember)
		{
			text += "Member";
		}
		_ResultUI.SetAdRewardData(text, num);
		WsWebService.ApplyPayout(text, num, ServiceEventHandler, null);
		if (_UI != null)
		{
			_UI.ChallengeItemVisible(show: false);
		}
		ChallengeInfo pActiveChallenge = ChallengeInfo.pActiveChallenge;
		if (pActiveChallenge != null)
		{
			switch (ChallengeInfo.CheckForChallengeCompletion(_GameID, 1, 0, num, isTimerUsedAsPoints: false))
			{
			case ChallengeResultState.LOST:
				SetChallengeText(_ChallengeTryAgainText.GetLocalizedString());
				break;
			case ChallengeResultState.WON:
			{
				string localizedString = _ChallengeCompleteText.GetLocalizedString();
				if (localizedString.Contains("[Name]"))
				{
					bool flag = false;
					if (BuddyList.pIsReady)
					{
						Buddy buddy = BuddyList.pInstance.GetBuddy(pActiveChallenge.UserID.ToString());
						if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
						{
							localizedString = localizedString.Replace("[Name]", buddy.DisplayName);
							SetChallengeText(localizedString);
							flag = true;
						}
					}
					if (!flag)
					{
						SetChallengeText("");
						WsWebService.GetDisplayNameByUserID(pActiveChallenge.UserID.ToString(), ServiceEventHandler, null);
					}
				}
				else
				{
					SetChallengeText(localizedString);
				}
				break;
			}
			}
		}
		else
		{
			SetChallengeText("");
		}
		if (_ResultUI != null)
		{
			_ResultUI.AllowChallenge(num > 0);
		}
		ChallengeInfo.pActiveChallenge = null;
		UiChallengeInvite.SetData(_GameID, 1, 0, num);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inType)
		{
		case WsServiceType.APPLY_PAYOUT:
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
			{
				AchievementReward[] array = null;
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				if (inObject != null)
				{
					array = (AchievementReward[])inObject;
					if (array != null)
					{
						GameUtilities.AddRewards(array, inUseRewardManager: false, inImmediateShow: false);
					}
				}
				if (_ResultUI != null)
				{
					_ResultUI.SetRewardDisplay(array);
				}
				break;
			}
			case WsServiceEvent.ERROR:
				UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
				UtDebug.Log("reward data is null!!!");
				break;
			}
			break;
		case WsServiceType.GET_DISPLAYNAME_BY_USER_ID:
			if (inEvent == WsServiceEvent.COMPLETE && inObject != null && !string.IsNullOrEmpty((string)inObject))
			{
				string localizedString = _ChallengeCompleteText.GetLocalizedString();
				localizedString = localizedString.Replace("[Name]", (string)inObject);
				SetChallengeText(localizedString);
			}
			break;
		}
	}

	public void TutAwardBooster(BoosterType inType)
	{
		CommonInventoryData.pInstance.AddItem(mBoosterTypeIDMap[inType], updateServer: false);
	}

	public void ExitGame()
	{
		if (ChallengeInfo.pActiveChallenge != null)
		{
			ChallengeInfo.CheckForChallengeCompletion(_GameID, 1, 0, (int)mScore, isTimerUsedAsPoints: false);
			ChallengeInfo.pActiveChallenge = null;
		}
		KAUiBejeweled.UseBooster -= HandleUseBooster;
		_UI._UiCountDown.OnCountdownDone -= HandleOnCountdownDone;
		ElementNadderSpike.OnActionComplete -= HandleOnActionComplete;
		if (null != _UI._BoosterBuyUi)
		{
			UiBuyPopup boosterBuyUi = _UI._BoosterBuyUi;
			boosterBuyUi._OnPurchaseSuccessful = (UiBuyPopup.PurchaseSuccessfull)Delegate.Remove(boosterBuyUi._OnPurchaseSuccessful, new UiBuyPopup.PurchaseSuccessfull(OnBoosterPurchaseDone));
		}
		if (null != _TutManager)
		{
			InteractiveTutManager tutManager = _TutManager;
			tutManager._StepStartedEvent = (StepStartedEvent)Delegate.Remove(tutManager._StepStartedEvent, new StepStartedEvent(TutorialStepStartEvent));
			InteractiveTutManager tutManager2 = _TutManager;
			tutManager2._StepEndedEvent = (StepEndedEvent)Delegate.Remove(tutManager2._StepEndedEvent, new StepEndedEvent(TutorialStepEndEvent));
			InteractiveTutManager tutManager3 = _TutManager;
			tutManager3._TutorialCompleteEvent = (TutorialCompleteEvent)Delegate.Remove(tutManager3._TutorialCompleteEvent, new TutorialCompleteEvent(TutorialCompleteEvent));
		}
		_UI.ExitGame();
	}

	protected override void FillVoid(List<ComboTileInfo> inTileCombo)
	{
		base.FillVoid(inTileCombo);
	}

	private void OnEndDBClose()
	{
		ExitGame();
	}

	private void OnReplayGame()
	{
		mGameState = GameState.PAUSED;
		mPlayedOrSkipped = false;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				TilePiece tilePiece = mTilesMap[i, j];
				mTilesMap[i, j] = null;
				tilePiece.SetState(TilePiece.State.DEAD);
			}
		}
		SetChallengeText("");
		Invoke("RestartCountdown", 0.1f);
		mScore = 0f;
		mTotalTimeLeft = _TotalTimeInSeconds;
		mTurnTimeLeft = 0f;
	}

	private void RestartCountdown()
	{
		_UI.PlayAgain();
	}

	private void OnBoosterPurchaseDone()
	{
		if (_Boosters != null)
		{
			for (int i = 0; i < _Boosters.Length; i++)
			{
				UpdateBoosterUI();
			}
		}
	}

	private bool CanMoveTutorialTile(int row, int col)
	{
		if (!IsTutorialComplete())
		{
			if ((mTutTileRow1 == row && mTutTileCol1 == col) || (mTutTileRow2 == row && mTutTileCol2 == col))
			{
				return true;
			}
			if (mTutTileRow1 == -1 && mTutTileRow2 == -1)
			{
				return true;
			}
		}
		return false;
	}
}
