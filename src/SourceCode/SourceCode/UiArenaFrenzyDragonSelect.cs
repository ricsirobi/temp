using System.Collections.Generic;
using UnityEngine;

public class UiArenaFrenzyDragonSelect : UiLobbyBase
{
	public float _SnapTime = 1f;

	public int _MaxCardsShown = 3;

	public LocaleString _DragonBusyText = new LocaleString("Dragon is busy with stable quest");

	private KAWidget mBtnHead2Head;

	private KAWidget mBtnChallengeFriend;

	private KAWidget mBtnSinglePlayer;

	private KAWidget mBackBtn;

	private KAWidget mGemTotal;

	private KAWidget mTotalGemCost;

	private KAWidget mHighlightCard;

	private KAWidget mBtnArrowLeft;

	private KAWidget mBtnArrowRight;

	public LocaleString _QuitConfirmationText = new LocaleString("Are you sure you want to quit Bull's-Eye Lagoon?");

	public LocaleString _QuitConfirmationTitleText = new LocaleString("Quit Bull's-Eye Lagoon?");

	public LocaleString _EnableMMOText = new LocaleString("Please enable MMO option from settings menu");

	public LocaleString _EnableMMOTitleText = new LocaleString("MMO Disabled");

	private ArenaFrenzyGame.GAME_MODE mGameMode;

	private int mPreviousMoney = -1;

	private int mHighlightIdx = -1;

	protected override void Start()
	{
		base.Start();
		mBtnHead2Head = FindItem("BtnHead2Head");
		mBtnChallengeFriend = FindItem("BtnChallengeFriend");
		mBtnSinglePlayer = FindItem("BtnSinglePlayer");
		mBtnArrowLeft = FindItem("BtnArrowLeft");
		mBtnArrowRight = FindItem("BtnArrowRight");
		mBackBtn = FindItem("BackBtn");
		mGemTotal = FindItem("GemTotal");
		mTotalGemCost = FindItem("TxtTotalGems");
		mHighlightCard = FindItem("DragonSelectHighlight");
		mDragonSelectionMenu.pDragPanel.onDragFinished = UpdateHiglightIdx;
	}

	public void UpdateHiglightIdx()
	{
		mDragonSelectionMenu.pDragPanel.currentMomentum = Vector3.zero;
		float num = float.PositiveInfinity;
		int num2 = 0;
		foreach (KAWidget item in mDragonSelectionMenu.GetItems())
		{
			float num3 = Vector3.Distance(item.transform.position, mHighlightCard.transform.position);
			if (num3 < num)
			{
				num = num3;
				mHighlightIdx = num2;
			}
			num2++;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnHead2Head)
		{
			UiDragonsInfoCardItem uiDragonsInfoCardItem = (UiDragonsInfoCardItem)mDragonSelectionMenu.GetItemAt(mHighlightIdx);
			if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(uiDragonsInfoCardItem.pSelectedPetID))
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _DragonBusyText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			else if (CheckMMOEnabled())
			{
				SetGameMode(ArenaFrenzyGame.GAME_MODE.PLAYER_VS_PLAYER);
			}
		}
		else if (inWidget == mBtnChallengeFriend)
		{
			if (CheckMMOEnabled())
			{
				if (ArenaFrenzyGame.pInstance != null)
				{
					ArenaFrenzyGame.pInstance._UiBuddySelect.SetVisibility(t: true);
				}
				SetVisibility(inVisible: false);
			}
		}
		else if (inWidget == mBackBtn)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _QuitConfirmationText.GetLocalizedString(), _QuitConfirmationTitleText.GetLocalizedString(), base.gameObject, "QuitArenaFrenzy", "CancelQuitArenaFrenzy", "", "", inDestroyOnClick: true);
		}
		else if (inWidget == mGemTotal)
		{
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
		}
		else if (inWidget == mBtnSinglePlayer)
		{
			UiDragonsInfoCardItem uiDragonsInfoCardItem2 = (UiDragonsInfoCardItem)mDragonSelectionMenu.GetItemAt(mHighlightIdx);
			if (TimedMissionManager.pInstance != null && TimedMissionManager.pInstance.IsPetEngaged(uiDragonsInfoCardItem2.pSelectedPetID))
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _DragonBusyText.GetLocalizedString(), base.gameObject, "OnDBClose");
			}
			else
			{
				SetGameMode(ArenaFrenzyGame.GAME_MODE.SINGLE_PLAYER);
			}
		}
		else if (inWidget == mBtnArrowLeft)
		{
			mHighlightIdx--;
		}
		else if (inWidget == mBtnArrowRight)
		{
			mHighlightIdx++;
		}
		mHighlightIdx = Mathf.Clamp(mHighlightIdx, 0, mDragonSelectionMenu.GetItemCount() - 1);
	}

	private void SetGameMode(ArenaFrenzyGame.GAME_MODE gameMode)
	{
		if (ArenaFrenzyGame.pInstance != null && ArenaFrenzyGame.pInstance.pIsReady)
		{
			mGameMode = gameMode;
			LoadSelectedDragon();
		}
	}

	private void OnDBClose()
	{
	}

	private bool CheckMMOEnabled()
	{
		if (!MainStreetMMOClient.pIsMMOEnabled)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDBSm", _EnableMMOText.GetLocalizedString(), _EnableMMOTitleText.GetLocalizedString(), base.gameObject, "", "", "CancelQuitArenaFrenzy", "", inDestroyOnClick: true);
			return false;
		}
		return true;
	}

	protected override int GetActivePetShowIndex()
	{
		List<RaisedPetData> petDataList = GetPetDataList();
		if (petDataList == null || petDataList.Count < _MaxCardsShown)
		{
			return base.GetActivePetShowIndex();
		}
		return _MaxCardsShown / 2;
	}

	private void QuitArenaFrenzy()
	{
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.ExitGame();
		}
	}

	private void CancelQuitArenaFrenzy()
	{
	}

	private void LoadSelectedDragon()
	{
		KAWidget itemAt = mDragonSelectionMenu.GetItemAt(mHighlightIdx);
		if (itemAt != null)
		{
			LoadSelectedDragon(itemAt);
		}
		else
		{
			UtDebug.Log("No dragon selected!");
		}
	}

	private void OnSelectDragonFinish(int petID)
	{
		if (ArenaFrenzyGame.pInstance != null && SanctuaryManager.pCurPetInstance != null)
		{
			if (SanctuaryManager.pCurPetInstance.IsActionAllowed(PetActions.ARENAFRENZY))
			{
				if (mGameMode == ArenaFrenzyGame.GAME_MODE.SINGLE_PLAYER)
				{
					ArenaFrenzyGame.pInstance.InitSinglePlayer();
				}
				else if (mGameMode == ArenaFrenzyGame.GAME_MODE.PLAYER_VS_PLAYER)
				{
					ArenaFrenzyGame.pInstance.InitPlayerVsPlayer();
				}
				mGameMode = ArenaFrenzyGame.GAME_MODE.NONE;
				SanctuaryManager.pCurPetInstance.UpdateActionMeters(PetActions.ARENAFRENZY, 1f, doUpdateSkill: true);
			}
			else
			{
				UiPetEnergyGenericDB.Show(base.gameObject, "DestroyDB", "DestroyDB", isLowEnergy: true);
			}
		}
		SetVisibility(inVisible: false);
	}

	private void DestroyDB()
	{
		if (ArenaFrenzyGame.pInstance._GameHUD != null)
		{
			ArenaFrenzyGame.pInstance._GameHUD.DestroyKAUIDB();
		}
		if (ArenaFrenzyGame.pInstance != null)
		{
			ArenaFrenzyGame.pInstance.InitMainMenu();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mPreviousMoney != Money.pCashCurrency && mTotalGemCost != null)
		{
			mPreviousMoney = Money.pCashCurrency;
			mTotalGemCost.SetText(Money.pCashCurrency.ToString());
		}
		if (!Input.anyKey)
		{
			Vector3 position = mDragonSelectionMenu._DefaultGrid.transform.position;
			position.x = mHighlightCard.transform.position.x - (float)mHighlightIdx * mDragonSelectionMenu._DefaultGrid.cellWidth;
			mDragonSelectionMenu._DefaultGrid.transform.position = Vector3.Lerp(mDragonSelectionMenu._DefaultGrid.transform.position, position, _SnapTime * Time.deltaTime);
		}
	}

	public void EnableDragonSelectionMenu()
	{
		mDragonSelectionMenu._DefaultGrid.transform.localPosition = Vector3.zero;
		SetDragonSelectionMenu();
		mHighlightIdx = GetActivePetShowIndex();
	}

	private void OnSelectDragonFailed(int petID)
	{
		SetInteractive(interactive: true);
	}
}
