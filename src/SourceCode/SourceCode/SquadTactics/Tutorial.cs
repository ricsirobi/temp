using System;
using UnityEngine;

namespace SquadTactics;

public class Tutorial : InteractiveTutManager
{
	public CharacterInputEnableData[] _CharacterInputEnableData;

	public StepTargetCoordinate[] _TargetCoordinatePerStep;

	public StepPointers[] _StepPointers;

	public DirectionalArrowData[] _DirectionalArrowData;

	public KAWidget _DirectionArrow;

	public float _DirectionArrowSpeed;

	public TapToContinueSteps[] _TapToContinueSteps;

	public StepCharacterSelected[] _StepCharacterSelected;

	public StepCharacterAbilitySelected[] _StepCharacterAbilitySelected;

	public LocaleString _TutorialCompleteMessage = new LocaleString("Tutorial Complete!");

	[HideInInspector]
	public Node _TargetNode;

	[HideInInspector]
	public bool _ForceDisableMove;

	private Character[] mCharacters;

	private Pointer[] mPointers;

	private DirectionalArrowData mArrowData;

	private TapToContinueSteps mTapToContinueStep;

	private bool mShowDirection;

	private Vector3 mStartPosition;

	private Vector3 mEndPosition;

	public override void Start()
	{
		base.Start();
		_StepStartedEvent = (StepStartedEvent)Delegate.Combine(_StepStartedEvent, new StepStartedEvent(OnStepStarted));
		_StepEndedEvent = (StepEndedEvent)Delegate.Combine(_StepEndedEvent, new StepEndedEvent(OnStepEnded));
	}

	public override void Update()
	{
		if (Input.GetMouseButtonDown(0) && mTapToContinueStep != null)
		{
			ProcessInput();
		}
		if (mShowDirection && _DirectionArrow != null)
		{
			_DirectionArrow.transform.position = Vector3.MoveTowards(_DirectionArrow.transform.position, mEndPosition, _DirectionArrowSpeed);
			if ((double)Vector3.Distance(_DirectionArrow.transform.position, mEndPosition) < 0.001)
			{
				_DirectionArrow.transform.position = mStartPosition;
			}
		}
	}

	public void Init()
	{
		for (int i = 0; i < _CharacterInputEnableData.Length; i++)
		{
			_CharacterInputEnableData[i]._Characters = new Character[_CharacterInputEnableData[i]._CharacterName.Length];
			for (int j = 0; j < _CharacterInputEnableData[i]._Characters.Length; j++)
			{
				_CharacterInputEnableData[i]._Characters[j] = GameManager.pInstance.GetCharacter(_CharacterInputEnableData[i]._CharacterName[j]);
			}
		}
		for (int k = 0; k < _StepCharacterSelected.Length; k++)
		{
			_StepCharacterSelected[k]._Character = GameManager.pInstance.GetCharacter(_StepCharacterSelected[k]._CharacterName);
		}
		for (int l = 0; l < _StepPointers.Length; l++)
		{
			for (int m = 0; m < _StepPointers[l]._Pointers.Length; m++)
			{
				if (_StepPointers[l]._Pointers[m]._CharacterName != "")
				{
					_StepPointers[l]._Pointers[m]._Character = GameManager.pInstance.GetCharacter(_StepPointers[l]._Pointers[m]._CharacterName);
				}
			}
		}
	}

	private void ProcessInput()
	{
		DisablePointer();
		if (mTapToContinueStep._AutoMoveToTarget && _TargetNode != null)
		{
			GameManager.pInstance.AutoMove(_TargetNode);
		}
		else
		{
			base.StartNextTutorial();
		}
	}

	private void DisablePointer()
	{
		if (mPointers != null)
		{
			Pointer[] array = mPointers;
			foreach (Pointer pointer in array)
			{
				if (pointer._Pointer != null)
				{
					pointer._Pointer.SetVisibility(inVisible: false);
					pointer._Pointer.pAnim2D.Stop();
				}
			}
		}
		mPointers = null;
	}

	public override bool TutorialComplete()
	{
		return false;
	}

	protected virtual void OnStepStarted(int stepIdx, string stepName)
	{
		CharacterInputEnableData characterInputEnableData = Array.Find(_CharacterInputEnableData, (CharacterInputEnableData x) => x._StepIndex == stepIdx);
		if (characterInputEnableData != null)
		{
			mCharacters = characterInputEnableData._Characters;
		}
		if (mCharacters != null && mCharacters.Length != 0)
		{
			for (int i = 0; i < mCharacters.Length; i++)
			{
				mCharacters[i].pCanProcessClick = true;
			}
		}
		StepTargetCoordinate stepTargetCoordinate = Array.Find(_TargetCoordinatePerStep, (StepTargetCoordinate x) => x._StepIndex == stepIdx);
		if (stepTargetCoordinate != null)
		{
			_TargetNode = GameManager.pInstance._Grid.GetNodeByPosition(stepTargetCoordinate._Node._GridX, stepTargetCoordinate._Node._GridY);
			_ForceDisableMove = stepTargetCoordinate._DisableMove;
		}
		mTapToContinueStep = Array.Find(_TapToContinueSteps, (TapToContinueSteps x) => x._StepIndex == stepIdx);
		StepPointers stepPointers = Array.Find(_StepPointers, (StepPointers x) => x._StepIndex == stepIdx);
		if (stepPointers != null)
		{
			mPointers = stepPointers._Pointers;
		}
		if (mPointers != null)
		{
			Pointer[] array = mPointers;
			foreach (Pointer pointer in array)
			{
				if (pointer._Pointer != null)
				{
					if (pointer._Character != null)
					{
						Vector3 position = Camera.main.WorldToScreenPoint(pointer._Character.transform.position + pointer._Offset);
						pointer._Pointer.transform.position = KAUIManager.pInstance.camera.ScreenToWorldPoint(position);
					}
					pointer._Pointer.SetVisibility(inVisible: true);
					pointer._Pointer.pAnim2D.Play(0);
				}
			}
		}
		mArrowData = Array.Find(_DirectionalArrowData, (DirectionalArrowData x) => x._StepIndex == stepIdx);
		if (mArrowData != null && _DirectionArrow != null)
		{
			mShowDirection = true;
			mStartPosition = GameManager.pInstance._Grid.GetNodeByPosition(mArrowData._StartNode._GridX, mArrowData._StartNode._GridY)._WorldPosition;
			mEndPosition = GameManager.pInstance._Grid.GetNodeByPosition(mArrowData._EndNode._GridX, mArrowData._EndNode._GridY)._WorldPosition;
			_DirectionArrow.transform.rotation = Quaternion.LookRotation(_DirectionArrow.transform.forward, Vector3.Cross(Vector3.up, mEndPosition - mStartPosition));
			_DirectionArrow.transform.position = mStartPosition;
			_DirectionArrow.transform.localScale = Vector3.one * mArrowData._Scale;
			_DirectionArrow.SetVisibility(inVisible: true);
		}
		if (stepIdx == 0)
		{
			CameraMovement.pInstance.pForceStopCameraMovement = true;
		}
		Invoke("SetCharacterAbilityDelayed", Time.deltaTime * 4f);
	}

	private void SetCharacterAbilityDelayed()
	{
		StepCharacterAbilitySelected stepCharacterAbilitySelected = Array.Find(_StepCharacterAbilitySelected, (StepCharacterAbilitySelected x) => x._StepIndex == mCurrentTutIndex);
		if (stepCharacterAbilitySelected != null && stepCharacterAbilitySelected._AbilityName != null)
		{
			GameManager.pInstance.SelectCharacterAbility(stepCharacterAbilitySelected._AbilityName);
		}
	}

	protected virtual void OnStepEnded(int stepIdx, string stepName, bool tutQuit)
	{
		if (mCharacters != null && mCharacters.Length != 0)
		{
			for (int i = 0; i < mCharacters.Length; i++)
			{
				mCharacters[i].pCanProcessClick = false;
			}
		}
		DisablePointer();
		mCharacters = null;
		mArrowData = null;
		_TargetNode = null;
		mShowDirection = false;
		mTapToContinueStep = null;
		if (_DirectionArrow != null)
		{
			_DirectionArrow.SetVisibility(inVisible: false);
		}
	}

	public override void SetupTutorialStep()
	{
		StepCharacterSelected stepCharacterSelected = Array.Find(_StepCharacterSelected, (StepCharacterSelected x) => x._StepIndex == mCurrentTutIndex);
		if (stepCharacterSelected != null && stepCharacterSelected._Character != null)
		{
			GameManager.pInstance.SelectCharacter(stepCharacterSelected._Character);
		}
		base.SetupTutorialStep();
	}

	public override void Exit()
	{
		ShowTutorialCompleteMessage();
	}

	private void ShowTutorialCompleteMessage()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfUiSTGenericDB", "PfUiStGenericDB");
		kAUIGenericDB.SetText(_TutorialCompleteMessage.GetLocalizedString(), interactive: false);
		kAUIGenericDB._MessageObject = base.gameObject;
		kAUIGenericDB._OKMessage = "DestroyMessageDB";
		kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		KAUI.SetExclusive(kAUIGenericDB, new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void DestroyMessageDB()
	{
		CameraMovement.pInstance.pForceStopCameraMovement = false;
		RsResourceManager.LoadLevel("STLevelSelectionDO");
		base.Exit();
	}

	public void SetTutorialDBVisibility(bool isVisible)
	{
		if (mTutorialDB != null && !string.IsNullOrEmpty(_TutSteps[mCurrentTutIndex]._StepText.GetLocalizedString()))
		{
			mTutorialDB.Show(isVisible);
		}
	}
}
