using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiSquadTacticsHUD : KAUI
{
	public string _PositiveArrowIcon = "AniDWDragonsSTArrowIncrease";

	public string _NegativeArrowIcon = "AniDWDragonsSTArrowDecrease";

	public string _AdvantageElementAnim = "Advantage";

	public string _PassiveElementAnim = "Passive";

	public float _TooltipDelay = 0.3f;

	[Header("Objectives")]
	public KAUIGenericDB _ObjectivesDB;

	public UISprite _ScaleBG;

	public int _ScaleValue = 40;

	public HUDStrings _HUDStrings;

	public HealthBarRange[] _HealthRanges;

	public RangeTextMap[] _RangeTextMap;

	public IconElementMap[] _IconElementMap;

	public AudioClip _RPSPositiveSFX;

	public AudioClip _RPSNegativeSFX;

	[SerializeField]
	private KAWidget m_TemplateObjective;

	[SerializeField]
	private KAWidget m_TemplateSubObjective;

	[SerializeField]
	private KAWidget m_TxtNewObjective;

	[SerializeField]
	private Transform m_NewObjectiveEndPos;

	[SerializeField]
	private float m_NewObjectiveTxtAnimDuration;

	[SerializeField]
	private float m_NewObjectiveTxtDisableDelay;

	private Vector2 mNewObjectiveStartPos;

	private KAUIMenu mObjectiveMenu;

	public UISprite[] _TimeScaleSprites;

	private bool mAllowAdvantageDisplay = true;

	private bool mLoadedAdvantageDisplay;

	private UiCharactersInfo mCharacterInfoUI;

	private UiEnemyInfo mEnemyInfoUI;

	private UiSettings mUiSettings;

	private KAWidget mBtnEndTurn;

	private KAWidget mBtnSettings;

	private KAWidget mBtnFastForward;

	private KAWidget mBtnObjective;

	private KAWidget mBtnChat;

	private KAWidget mTxtEnemyTurn;

	private KAWidget mTxtTurnCounter;

	private KAWidget mTxtWaitEndPlayerTurn;

	private KAWidget mAniAdvantages;

	private KAWidget mAniEffectArrow;

	private bool mTurnCounterVisibility;

	private float mCachedTooltipDelay = 0.5f;

	private ElementType mEnemyElement;

	private ElementType mPlayerElement;

	public bool pAllowAdvantageDisplay
	{
		get
		{
			if (!mLoadedAdvantageDisplay && UserInfo.pIsReady)
			{
				mAllowAdvantageDisplay = PlayerPrefs.GetInt("STADV_" + UserInfo.pInstance.ParentUserID, 0) == 0;
				mLoadedAdvantageDisplay = true;
			}
			return mAllowAdvantageDisplay;
		}
		set
		{
			if (UserInfo.pIsReady)
			{
				PlayerPrefs.SetInt("STADV_" + UserInfo.pInstance.ParentUserID, (!value) ? 1 : 0);
			}
			mAllowAdvantageDisplay = value;
			mLoadedAdvantageDisplay = true;
			mAniAdvantages.SetVisibility(mAllowAdvantageDisplay);
		}
	}

	public UiCharactersInfo pCharacterInfoUI => mCharacterInfoUI;

	public UiEnemyInfo pEnemyInfoUI => mEnemyInfoUI;

	protected override void Awake()
	{
		base.Awake();
		mEnemyInfoUI = (UiEnemyInfo)_UiList[0];
		mCharacterInfoUI = (UiCharactersInfo)_UiList[1];
		mUiSettings = (UiSettings)_UiList[2];
		mObjectiveMenu = _ObjectivesDB._MenuList[0];
	}

	protected override void Start()
	{
		base.Start();
		CinematicCamera pInstance = CinematicCamera.pInstance;
		pInstance._CinematicCapture = (CinematicCamera.Capture)Delegate.Combine(pInstance._CinematicCapture, new CinematicCamera.Capture(SetVisibility));
		UiChatHistory.HideChatHistory();
		mNewObjectiveStartPos = m_TxtNewObjective.GetPosition();
	}

	public void Initialize(List<Character> characters)
	{
		mBtnEndTurn = FindItem("BtnEndTurn");
		mBtnSettings = FindItem("BtnOptions");
		mBtnFastForward = FindItem("BtnFastFwd");
		mBtnObjective = FindItem("BtnObjectives");
		mBtnChat = FindItem("BtnChat");
		mTxtEnemyTurn = FindItem("TxtEnemyTurn");
		mTxtTurnCounter = FindItem("TxtTurnCounter");
		mTxtWaitEndPlayerTurn = FindItem("TxtWaitingEndPlayerTurn");
		mAniAdvantages = FindItem("AniAdvantages");
		mAniEffectArrow = mAniAdvantages.FindChildItem("AniEffectArrow");
		_TimeScaleSprites[0].gameObject.SetActive(value: true);
		if (_HealthRanges != null)
		{
			Array.Sort(_HealthRanges, (HealthBarRange a, HealthBarRange b) => a._Percentage.CompareTo(b._Percentage));
		}
		UpdateTooltipDelay();
		mCharacterInfoUI.Initialize(characters);
		IconElementMap[] iconElementMap = _IconElementMap;
		for (int i = 0; i < iconElementMap.Length; i++)
		{
			iconElementMap[i]._IconWidget.pAnim2D.Play(_PassiveElementAnim);
		}
		mAniAdvantages.SetVisibility(inVisible: false);
		mBtnObjective.SetDisabled(GameManager.pInstance._Tutorial);
	}

	private void UpdateTooltipDelay()
	{
		mCachedTooltipDelay = base.pUIManager.tooltipDelay;
		base.pUIManager.tooltipDelay = _TooltipDelay;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnEndTurn)
		{
			GameManager.pInstance.EndPlayerTurn();
		}
		else if (inWidget == mBtnSettings)
		{
			mUiSettings.ShowUi(show: true);
		}
		else if (inWidget == mBtnObjective)
		{
			StopCoroutine("CloseObjective");
			ToggleObjectivesDB();
		}
		else if (inWidget == mBtnFastForward)
		{
			GameManager.pInstance.UpdateTimeScale();
			int pTimeScaleIndex = GameManager.pInstance.pTimeScaleIndex;
			if (pTimeScaleIndex == 0)
			{
				for (int i = 0; i < _TimeScaleSprites.Length; i++)
				{
					_TimeScaleSprites[i].gameObject.SetActive(value: false);
				}
			}
			_TimeScaleSprites[pTimeScaleIndex].gameObject.SetActive(value: true);
		}
		else if (inWidget == mBtnChat)
		{
			if (!UiChatHistory._IsVisible)
			{
				UiChatHistory.ShowChatHistory();
			}
			else
			{
				UiChatHistory.HideChatHistory();
			}
		}
		if (GameManager.pInstance._Tutorial != null)
		{
			GameManager.pInstance._Tutorial.TutorialManagerAsyncMessage("Successfull_Move");
		}
	}

	public void SetCharactersMenuState(KAUIState inState)
	{
		if (mCharacterInfoUI != null && mCharacterInfoUI.pCharactersMenu != null)
		{
			mCharacterInfoUI.pCharactersMenu.SetState(inState);
		}
	}

	public void VisibleEndTurnBtn(bool visible)
	{
		mBtnEndTurn.SetVisibility(visible);
	}

	public void VisibleEnemyTurnText(bool visible)
	{
		if (visible)
		{
			_ObjectivesDB.SetVisibility(inVisible: false);
		}
		if (mTurnCounterVisibility)
		{
			mTxtTurnCounter.SetVisibility(!visible);
		}
		mTxtEnemyTurn.SetVisibility(visible);
		mCharacterInfoUI.ShowSmallAbilityInfo(!visible);
		mCharacterInfoUI.pCharacterAbilitiesMenu.SetVisibility(!visible);
		if (!visible && mEnemyInfoUI.pSelectedEnemy != null)
		{
			mEnemyInfoUI.UpdateEnemyDetails(mEnemyInfoUI.pSelectedEnemy, !visible);
		}
		else
		{
			mEnemyInfoUI.SetVisibility(inVisible: false);
		}
		if (!visible)
		{
			mCharacterInfoUI.pCharacterAbilitiesMenu.CheckAbilityAnims();
			VisiblePlayerTurnOnlyUIs(visible: true);
		}
		if (visible && UiAbilityInfo.pInstance.GetVisibility())
		{
			UiAbilityInfo.pInstance.SetVisibility(inVisible: false);
		}
	}

	public void VisibleWaitingPlayerTurnText(bool visible)
	{
		mTxtWaitEndPlayerTurn.SetVisibility(visible);
		if (visible)
		{
			VisiblePlayerTurnOnlyUIs(visible: false);
		}
	}

	public void VisiblePlayerTurnOnlyUIs(bool visible)
	{
		if (mTurnCounterVisibility)
		{
			mTxtTurnCounter.SetVisibility(visible);
		}
		mBtnSettings.SetVisibility(visible);
		mBtnObjective.SetVisibility(visible);
		mBtnEndTurn.SetVisibility(visible);
		VisibleAbilitiesMenu(visible);
	}

	public void VisibleAbilitiesMenu(bool visible)
	{
		if (mCharacterInfoUI != null && mCharacterInfoUI.pCharacterAbilitiesMenu != null)
		{
			mCharacterInfoUI.ShowSmallAbilityInfo(visible);
			mCharacterInfoUI.pCharacterAbilitiesMenu.SetVisibility(visible);
		}
	}

	public void UpdateStatus(Character character)
	{
		if (character.pCharacterData._Team == Character.Team.PLAYER)
		{
			mCharacterInfoUI.UpdatePlayerStat(character);
		}
		else
		{
			mEnemyInfoUI.UpdateEnemyStat(character);
		}
	}

	public void UpdateHealth(Character character)
	{
		if (character.pCharacterData._Team == Character.Team.PLAYER)
		{
			mCharacterInfoUI.UpdatePlayersInfoDisplay(character, updateAll: false);
			return;
		}
		if (mEnemyInfoUI.pSelectedEnemy == character)
		{
			mEnemyInfoUI.UpdateEnemyHealth(character);
		}
		if (character.pIsDead)
		{
			UpdateElementCounter(ElementType.NONE, ElementType.NONE, reset: true);
		}
	}

	public string GetRangeTextFromRange(float range)
	{
		if (_RangeTextMap == null || _RangeTextMap.Length == 0)
		{
			return string.Empty;
		}
		RangeTextMap[] rangeTextMap = _RangeTextMap;
		foreach (RangeTextMap rangeTextMap2 in rangeTextMap)
		{
			if (rangeTextMap2._Range.IsInRange(range))
			{
				return rangeTextMap2._RangeText.GetLocalizedString();
			}
		}
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.pUIManager.tooltipDelay = mCachedTooltipDelay;
		CinematicCamera pInstance = CinematicCamera.pInstance;
		pInstance._CinematicCapture = (CinematicCamera.Capture)Delegate.Remove(pInstance._CinematicCapture, new CinematicCamera.Capture(SetVisibility));
	}

	public void SetObjectives()
	{
		mObjectiveMenu.ClearItems();
		List<Objective> list = new List<Objective>();
		List<Objective> list2 = new List<Objective>();
		list.AddRange(GameManager.pInstance.GetFinalObjectives().FindAll((Objective x) => x._IsMandatory && x.pHiddenStatus != ObjectiveHiddenStatus.HIDDEN_BY_PARENT && (x.pObjectiveStatus == ObjectiveStatus.INPROGRESS || (x.pObjectiveStatus == ObjectiveStatus.COMPLETED && (!x.HasChildObjectives() || (x.HasChildObjectives() && x._ChildObjectives.FindAll((Objective y) => y._IsHiddenByParent).Count == 0))))));
		list2.AddRange(GameManager.pInstance.GetFinalObjectives().FindAll((Objective x) => !x._IsMandatory && x.pHiddenStatus != ObjectiveHiddenStatus.HIDDEN_BY_PARENT && (x.pObjectiveStatus == ObjectiveStatus.INPROGRESS || (x.pObjectiveStatus == ObjectiveStatus.COMPLETED && (!x.HasChildObjectives() || (x.HasChildObjectives() && x._ChildObjectives.FindAll((Objective y) => y._IsHiddenByParent).Count == 0))))));
		mTurnCounterVisibility = false;
		CheckAndAddObjectives(list);
		CheckAndAddObjectives(list2);
		mTxtTurnCounter.SetVisibility(mTurnCounterVisibility);
	}

	private void CheckAndAddObjectives(List<Objective> objectives)
	{
		if (objectives == null || objectives.Count < 1)
		{
			return;
		}
		for (int i = 0; i < objectives.Count; i++)
		{
			Objective objective = objectives[i];
			KAWidget widget = GetWidget(objective._IsMandatory ? "Mandatory" : "Optional", m_TemplateObjective, new ObjectiveData(objective));
			UpdateObjectiveDetails(objective, widget);
			if (!mTurnCounterVisibility && objective._TurnLimit > 0)
			{
				mTurnCounterVisibility = true;
			}
			if (objective.HasChildObjectives())
			{
				int childCount = 0;
				GetChildCount(ref childCount, objective._ChildObjectives);
				if (childCount > 0)
				{
					KAWidget widget2 = GetWidget("ChildObjective", m_TemplateSubObjective, null);
					KAWidget kAWidget = widget2.FindChildItem("TxtLevelsCount");
					widget2.FindChildItem("TxtName").SetText(_HUDStrings._HiddenText.GetLocalizedString());
					kAWidget.SetText($"x{childCount}");
				}
			}
		}
	}

	private void GetChildCount(ref int childCount, List<Objective> objectives)
	{
		foreach (Objective v_obj in objectives)
		{
			if (GameManager.pInstance.GetFinalObjectives().Find((Objective x) => x.pObjectiveId == v_obj.pObjectiveId && x._IsHiddenByParent) != null)
			{
				childCount++;
				if (v_obj.HasChildObjectives())
				{
					GetChildCount(ref childCount, v_obj._ChildObjectives);
				}
			}
		}
	}

	private KAWidget GetWidget(string widgetName, KAWidget widgetTemplate, ObjectiveData userData)
	{
		KAWidget kAWidget = mObjectiveMenu.DuplicateWidget(widgetTemplate);
		if (kAWidget != null)
		{
			kAWidget.gameObject.SetActive(value: true);
			kAWidget.transform.name = widgetName;
			kAWidget.transform.localScale = widgetTemplate.transform.localScale;
			kAWidget.SetUserData(userData);
			kAWidget._MenuItemIndex = mItemInfo.Count;
			mObjectiveMenu.pRecenterItem = true;
			mObjectiveMenu.AddWidget(kAWidget);
			kAWidget.SetVisibility(inVisible: true);
		}
		return kAWidget;
	}

	public void PlayNewObjectiveAnim()
	{
		StopCoroutine(DisableNewObjectiveText());
		m_TxtNewObjective.MoveTo(mNewObjectiveStartPos, m_NewObjectiveEndPos.position, m_NewObjectiveTxtAnimDuration);
		m_TxtNewObjective.SetVisibility(inVisible: true);
		StartCoroutine(DisableNewObjectiveText());
	}

	private IEnumerator DisableNewObjectiveText()
	{
		yield return new WaitForSeconds(m_NewObjectiveTxtDisableDelay);
		m_TxtNewObjective.SetVisibility(inVisible: false);
	}

	public void UpdateObjectivesDisplay()
	{
		foreach (KAWidget item in mObjectiveMenu.GetItems())
		{
			ObjectiveData objectiveData = (ObjectiveData)item.GetUserData();
			if (objectiveData != null)
			{
				UpdateObjectiveDetails(objectiveData._Objective, item);
			}
		}
	}

	private void UpdateObjectiveDetails(Objective ob, KAWidget widget)
	{
		if (ob != null && !(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("TxtName");
			KAWidget kAWidget2 = widget.FindChildItem("TxtUnlockTurns");
			kAWidget2.SetText(ob.IsTurnsCompleted() ? string.Empty : string.Format(_HUDStrings._TurnsToUnlockText.GetLocalizedString(), ob.pNoOfTurnsToUnlock));
			kAWidget2.SetVisibility(!ob.IsTurnsCompleted());
			kAWidget.SetText(ob.IsTurnsCompleted() ? GameManager.pInstance.GetSceneData().GetFinalObjectiveStr(ob) : _HUDStrings._HiddenText.GetLocalizedString());
			kAWidget.GetLabel().color = (ob._IsMandatory ? GameManager.pInstance._TxtColorMandatoryObjective : GameManager.pInstance._TxtColorOptionalObjective);
			widget.FindChildItem("DefSprite").SetVisibility((ob.pHiddenStatus != 0 && ob.pObjectiveStatus == ObjectiveStatus.COMPLETED) || (ob.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && ob.pObjectiveStatus != ObjectiveStatus.COMPLETED));
			widget.FindChildItem("CompletedSprite").SetVisibility(ob.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && ob.pObjectiveStatus == ObjectiveStatus.COMPLETED);
			widget.FindChildItem("FailedSprite").SetVisibility(ob.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN && ob.pObjectiveStatus == ObjectiveStatus.FAILED);
		}
	}

	public void ShowObjectives(float delay)
	{
		ToggleObjectivesDB();
		StartCoroutine("CloseObjective", delay);
	}

	private IEnumerator CloseObjective(float delay)
	{
		yield return new WaitForSeconds(delay);
		ToggleObjectivesDB();
	}

	private void ToggleObjectivesDB()
	{
		if (_ObjectivesDB != null)
		{
			if (_ObjectivesDB._MessageObject == null)
			{
				_ObjectivesDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
				_ObjectivesDB._MessageObject = base.gameObject;
			}
			_ObjectivesDB.SetVisibility(!_ObjectivesDB.GetVisibility());
		}
	}

	public void CloseSettingsUi()
	{
		if (mUiSettings.GetVisibility())
		{
			mUiSettings.ShowUi(show: false);
			KAUI.RemoveExclusive(mUiSettings);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		if (GameManager.pInstance._Tutorial == null)
		{
			CameraMovement.pInstance.pForceStopCameraMovement = !inVisible;
		}
		else
		{
			GameManager.pInstance._Tutorial.SetTutorialDBVisibility(inVisible);
		}
	}

	public void UpdateElementCounter(ElementType playerElement, ElementType enemyElement, bool reset)
	{
		if (!mAllowAdvantageDisplay)
		{
			return;
		}
		if (reset || _IconElementMap == null || _IconElementMap.Length == 0 || enemyElement == ElementType.NONE)
		{
			mEnemyElement = ElementType.NONE;
			mPlayerElement = ElementType.NONE;
			if (mAniAdvantages.GetVisibility())
			{
				mAniAdvantages.SetVisibility(inVisible: false);
			}
			return;
		}
		if (enemyElement != mEnemyElement || playerElement != mPlayerElement)
		{
			mEnemyElement = enemyElement;
			mPlayerElement = playerElement;
			SetElementDisplay();
			ElementCounterResult elementCounterResult = GameManager.pInstance.GetElementCounterResult(playerElement, enemyElement);
			mAniEffectArrow.SetVisibility(elementCounterResult == ElementCounterResult.POSITIVE);
			if (elementCounterResult == ElementCounterResult.POSITIVE)
			{
				if ((bool)_RPSPositiveSFX)
				{
					SnChannel.Play(_RPSPositiveSFX, "STSFX_Pool", inForce: true);
				}
				mAniEffectArrow.pAnim2D.Play(_PositiveArrowIcon);
				IconElementMap[] iconElementMap = _IconElementMap;
				foreach (IconElementMap iconElementMap2 in iconElementMap)
				{
					if (iconElementMap2._Type == playerElement)
					{
						iconElementMap2._IconWidget.pAnim2D.Play(_AdvantageElementAnim);
					}
					else
					{
						iconElementMap2._IconWidget.pAnim2D.Play(_PassiveElementAnim);
					}
				}
			}
			else
			{
				IconElementMap[] iconElementMap = _IconElementMap;
				for (int i = 0; i < iconElementMap.Length; i++)
				{
					iconElementMap[i]._IconWidget.pAnim2D.Play(_PassiveElementAnim);
				}
			}
		}
		if (!mAniAdvantages.GetVisibility())
		{
			mAniAdvantages.SetVisibility(inVisible: true);
		}
	}

	public void UpdateTurnCounter(int counter)
	{
		counter--;
		mTxtTurnCounter.SetText(counter.ToString());
	}

	public void SetElementDisplay()
	{
		ElementCounter[] elementCounterData = Settings.pInstance.GetElementCounterData(mEnemyElement.ToString());
		if (elementCounterData.Length != _IconElementMap.Length)
		{
			return;
		}
		for (int i = 0; i < elementCounterData.Length; i++)
		{
			_IconElementMap[i]._Type = elementCounterData[i]._Element;
			if (_IconElementMap[i]._Icon != null)
			{
				_IconElementMap[i]._Icon.UpdateSprite(Settings.pInstance.GetElementInfo(elementCounterData[i]._Element)._Icon);
			}
			if (_IconElementMap[i]._Arrow != null)
			{
				_IconElementMap[i]._Arrow.color = Settings.pInstance.GetElementInfo(elementCounterData[i]._Element)._Color;
			}
		}
	}
}
