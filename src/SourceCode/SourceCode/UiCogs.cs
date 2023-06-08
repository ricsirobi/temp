using UnityEngine;

public class UiCogs : KAUI
{
	public Camera _MainCamera;

	public UiCogsInventoryMenu _CogsItemMenu;

	private CogObject mObjectInHand;

	private Vector3 mObjectLastPosition;

	private bool mObjectInHandDiffective;

	private Vector3 mOffsetPosFromMousePtr;

	private bool mUpdateMoves;

	private float mPrevPos;

	private float mEndPos;

	private float mWinFlashMoveRateSetter;

	private float mWinMoveFlashRate;

	private float mWinFlashDuration;

	private KAWidget mTxtLevel;

	private KAWidget mBtnReset;

	private KAWidget mTxtTimer;

	private KAWidget mTxtMoves;

	private KAWidget mBtnBack;

	private KAWidget mBtnHelp;

	private bool mSnapCogPosition = true;

	public bool pIsReady => _CogsItemMenu.pIsReady;

	public int pSetLevelText
	{
		set
		{
			if (mTxtLevel == null)
			{
				mTxtLevel = FindItem("AniLevel");
			}
			if (mTxtLevel != null)
			{
				mTxtLevel.SetVisibility(value != 0);
				mTxtLevel.SetText(value.ToString());
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		mTxtTimer = FindItem("TxtTimer");
		mTxtMoves = FindItem("TxtMoves");
		mBtnReset = FindItem("BtnReset");
		mBtnBack = FindItem("BtnBack");
		mBtnHelp = FindItem("BtnHelp");
	}

	public void MenuItemDragged(KAUIMenu inMenu, KAWidget inWidget, Vector2 inDelta)
	{
		if (inWidget == null || mObjectInHand != null || CogsGameManager.pInstance.pIsPlaying)
		{
			return;
		}
		CogsWidgetData cogsWidgetData = (CogsWidgetData)inWidget.GetUserData();
		if (cogsWidgetData != null)
		{
			CogsGameManager.pInstance.pIsTimerStarted = true;
			GameObject gameObject = Object.Instantiate(((Transform)cogsWidgetData._ItemPrefabData._ResObject).gameObject, Vector3.up * 5000f, Quaternion.identity);
			if (gameObject != null)
			{
				mObjectInHand = gameObject.GetComponent<CogObject>();
				gameObject.name = gameObject.name + "_" + (CogsGameManager.pInstance._CogsContainer.Count - CogsGameManager.pInstance._StartCogs.Count - CogsGameManager.pInstance._VictoryCogs.Count);
				Vector3 vector = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
				mObjectInHand.transform.parent = CogsLevelManager.pInstance._ActiveContainer;
				mObjectInHand.transform.localPosition = new Vector3(vector.x, vector.y, 0f);
				mObjectInHand.Setup(cogsWidgetData._Cog);
				_CogsItemMenu.RemoveItem(cogsWidgetData._Cog);
			}
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		CogsGameManager.pInstance._StartLeverClickable._Active = inVisible;
	}

	protected override void Update()
	{
		base.Update();
		HandleObjectPickup();
		HandleInHandObject();
		if (!CogsGameManager.pInstance.pIsPlaying)
		{
			float pLapsedTime = CogsGameManager.pInstance.pLapsedTime;
			int num = (int)(pLapsedTime / 60f);
			string text = string.Concat(str2: $"{(int)(pLapsedTime % 60f):00}", str0: $"{num:00}", str1: ":");
			mTxtTimer.SetText(text);
			if (pLapsedTime > CogsGameManager.pInstance.pGoalTime)
			{
				mTxtTimer.GetLabel().color = Color.red;
			}
		}
		else
		{
			if (!CogsGameManager.pInstance.pIsGameCompleted)
			{
				return;
			}
			if (mWinFlashDuration > 0f)
			{
				mWinFlashDuration -= Time.deltaTime;
				mWinMoveFlashRate -= Time.deltaTime;
				if (mWinMoveFlashRate <= 0f)
				{
					mTxtMoves.SetVisibility(!mTxtMoves.GetVisibility());
					mWinMoveFlashRate = mWinFlashMoveRateSetter;
				}
			}
			else
			{
				mTxtMoves.SetVisibility(inVisible: true);
			}
		}
	}

	public void FlashMoveCount(float rate, float duration)
	{
		mWinFlashDuration = duration;
		mWinMoveFlashRate = rate;
		mWinFlashMoveRateSetter = rate;
	}

	private void HandleObjectPickup()
	{
		if (mObjectInHand != null || !KAInput.GetMouseButtonDown(0) || CogsGameManager.pInstance.pIsGameCompleted || (CogsGameManager.pInstance.pIsPlaying && CogsGameManager.pInstance._DefectiveMachines.Count == 0))
		{
			return;
		}
		int layerMask = 1 << LayerMask.NameToLayer("3DNGUI");
		if (!Physics.Raycast(_MainCamera.ScreenPointToRay(Input.mousePosition), out var hitInfo, 50f, layerMask))
		{
			return;
		}
		GameObject gameObject = hitInfo.collider.gameObject;
		mObjectInHand = gameObject.GetComponent<CogObject>();
		if (mObjectInHand == null || mObjectInHand._CogType != CogType.INVENTORY_COG)
		{
			mObjectInHand = null;
			return;
		}
		mPrevPos = mObjectInHand.transform.position.magnitude;
		if (CogsGameManager.pInstance._CogsContainer.Contains(mObjectInHand))
		{
			CogsGameManager.pInstance._CogsContainer.Remove(mObjectInHand);
		}
		Vector3 vector = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
		vector = new Vector3(vector.x, vector.y, 0f);
		mOffsetPosFromMousePtr = vector - mObjectInHand.transform.localPosition;
	}

	private void HandleInHandObject()
	{
		if (mObjectInHand == null)
		{
			return;
		}
		Vector3 vector = _MainCamera.ScreenToWorldPoint(Input.mousePosition);
		vector = new Vector3(vector.x, vector.y, 0f);
		Vector3 localPosition = vector - mOffsetPosFromMousePtr;
		mObjectInHand.transform.localPosition = localPosition;
		mEndPos = mObjectInHand.transform.position.magnitude;
		if (mEndPos != mPrevPos)
		{
			mUpdateMoves = true;
			CogsGameManager.pInstance.AlignCog(mObjectInHand, mSnapCogPosition);
			mPrevPos = mObjectInHand.transform.position.magnitude;
			if (CogsGameManager.pInstance._DefectiveMachines.Count > 0)
			{
				if (CogsGameManager.pInstance._ExplodeParticle != null)
				{
					CogsGameManager.pInstance._ExplodeParticle.Stop();
				}
				SnChannel.AcquireChannel("SFX_Pool", inForce: true).pLoop = false;
				SnChannel.StopPool("SFX_Pool");
				CogsGameManager.pInstance._DefectiveMachines.Clear();
				CogsGameManager.pInstance._StartLeverClickable._Active = true;
			}
		}
		if (!(mObjectInHand != null))
		{
			return;
		}
		if (mObjectLastPosition != mObjectInHand.transform.position)
		{
			mObjectInHandDiffective = false;
			Vector3 point = (mObjectLastPosition = mObjectInHand.transform.position);
			point.x += ((point.x < CogsGameManager.pInstance._GameBoard.transform.position.x) ? (0f - mObjectInHand.pGear.radius) : mObjectInHand.pGear.radius);
			point.y += ((point.y < CogsGameManager.pInstance._GameBoard.transform.position.y) ? (0f - mObjectInHand.pGear.radius) : mObjectInHand.pGear.radius);
			if (!CogsGameManager.pInstance._GameBoard.GetComponent<Collider>().bounds.Contains(point))
			{
				mObjectInHandDiffective = true;
			}
			else
			{
				for (int i = 0; i < CogsGameManager.pInstance._CogsContainer.Count; i++)
				{
					CogObject cogObject = CogsGameManager.pInstance._CogsContainer[i];
					if (!(mObjectInHand == cogObject) && (cogObject.transform.position - mObjectInHand.transform.position).magnitude - (mObjectInHand.pGear.radius - mObjectInHand.pGear.gearGen.tipLength + cogObject.pGear.radius) < CogsGameManager.pInstance._CollisionCheckOffset)
					{
						mObjectInHandDiffective = true;
						break;
					}
				}
			}
		}
		if (KAInput.GetMouseButtonUp(0))
		{
			if (mObjectInHandDiffective || mObjectInHand._InContactRatchetList.Count > 0)
			{
				if (CogsGameManager.pInstance._CogsContainer.Contains(mObjectInHand))
				{
					CogsGameManager.pInstance._CogsContainer.Remove(mObjectInHand);
				}
				_CogsItemMenu.AddItem(mObjectInHand.pCachedCog);
				CogsGameManager.pInstance.pIsResetRequired = true;
				Object.Destroy(mObjectInHand.gameObject);
			}
			else if (!CogsGameManager.pInstance._CogsContainer.Contains(mObjectInHand))
			{
				CogsGameManager.pInstance._CogsContainer.Add(mObjectInHand);
			}
			if (mUpdateMoves && !CogsGameManager.pInstance.pIsResetRequired)
			{
				mTxtMoves.SetText((++CogsGameManager.pInstance.pCurrentMoves).ToString());
				if (CogsGameManager.pInstance.pCurrentMoves > CogsGameManager.pInstance.pGoalMoves)
				{
					mTxtMoves.GetLabel().color = Color.red;
				}
			}
			mUpdateMoves = false;
			mObjectInHand = null;
		}
		else if (mObjectInHandDiffective || mObjectInHand._InContactRatchetList.Count > 0)
		{
			mObjectInHand.SetInvalidColor(invalid: true);
		}
		else
		{
			mObjectInHand.SetInvalidColor(invalid: false);
		}
	}

	public void UpdateMoveCounter()
	{
		mTxtMoves.SetText(CogsGameManager.pInstance.pCurrentMoves.ToString());
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnReset)
		{
			CogsGameManager.pInstance.OnReplayGame();
		}
		else if (inWidget == mBtnBack)
		{
			if (!string.IsNullOrEmpty(CogsLevelManager.pLevelToLoad))
			{
				CogsLevelManager.pInstance.QuitGame();
			}
			else
			{
				CogsGameManager.pInstance.OnMainMenu();
			}
		}
		else if (inWidget == mBtnHelp)
		{
			CogsGameManager.pInstance.ShowHelpScreen();
		}
	}

	public void ResetUI()
	{
		mTxtTimer.GetLabel().color = Color.white;
		mTxtMoves.GetLabel().color = Color.white;
		SetHelperButtons(inActive: true);
		mTxtMoves.SetVisibility(inVisible: true);
		CogsGameManager.pInstance._StartLeverClickable._Active = true;
	}

	public void SetHelperButtons(bool inActive)
	{
		mBtnHelp.SetVisibility(inActive);
		mBtnReset.SetVisibility(inActive);
		mBtnBack.SetVisibility(inActive);
	}
}
