using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiChest : KAButton
{
	public string _OpenAnimation = "Open";

	public string _WiggleAnimation = "Wiggle";

	public string _CloseAnimation = "Closed";

	public KAWidget _LockIcon;

	public Transform _Chest;

	public KAWidget _BtnAds;

	private Animation mChestBoxAnimation;

	private bool mIsOpenAnimationPlayed;

	public List<Renderer> _ChestMesh;

	public bool pIsLockedChest { get; set; }

	public bool pIsRewardChest { get; set; }

	public bool pHasTriviaQuestion { get; set; }

	public QuizQuestion pQuestion { get; set; }

	public string pMissedObjectiveText { get; set; }

	public void InitChest()
	{
		if (_Chest != null)
		{
			mChestBoxAnimation = _Chest.GetComponent<Animation>();
		}
		if (mChestBoxAnimation == null)
		{
			mChestBoxAnimation = _Chest.GetComponentInChildren<Animation>();
		}
		if (pIsRewardChest)
		{
			OpenChest();
			return;
		}
		if (pIsLockedChest)
		{
			foreach (Renderer item in _ChestMesh)
			{
				item.material.color = Color.black;
				LockChest();
			}
			return;
		}
		WiggleChest();
	}

	public void ShowAdBtn(bool enable)
	{
		if (_BtnAds != null)
		{
			_BtnAds.SetVisibility(enable);
			if (enable)
			{
				SetState(KAUIState.NOT_INTERACTIVE);
			}
		}
	}

	public void InitChestLock()
	{
		LockChest();
		((KAUIMenu)base.pUI)._ParentUi.SendMessage("OnChestLocked", this);
	}

	public void OnChestOpened()
	{
		((KAUIMenu)base.pUI)._ParentUi.SendMessage("OnChestOpened", this);
	}

	public void InitChestOpen()
	{
		OpenChest();
		((KAUIMenu)base.pUI)._ParentUi.SendMessage("OnInitChestOpen", this);
	}

	protected override void Update()
	{
		if (mChestBoxAnimation != null && mIsOpenAnimationPlayed && !mChestBoxAnimation.isPlaying)
		{
			mIsOpenAnimationPlayed = false;
			OnChestOpened();
		}
		base.Update();
	}

	private void OpenChest()
	{
		SetInteractive(isInteractive: false);
		if (mChestBoxAnimation == null)
		{
			OnChestOpened();
		}
		else
		{
			mChestBoxAnimation.Play(_OpenAnimation);
			if (mChestBoxAnimation.IsPlaying(_OpenAnimation))
			{
				mIsOpenAnimationPlayed = true;
			}
			else
			{
				OnChestOpened();
			}
		}
		ShowAdBtn(enable: false);
	}

	private void LockChest()
	{
		if (_LockIcon != null)
		{
			_LockIcon.SetVisibility(inVisible: true);
		}
		if (mChestBoxAnimation != null)
		{
			mChestBoxAnimation.Play(_CloseAnimation);
		}
		ShowAdBtn(enable: false);
	}

	private void WiggleChest()
	{
		SetInteractive(isInteractive: true);
		if (mChestBoxAnimation != null)
		{
			mChestBoxAnimation.Play(_WiggleAnimation);
		}
	}
}
