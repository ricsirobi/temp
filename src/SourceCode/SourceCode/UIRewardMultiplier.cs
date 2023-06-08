using System.Collections.Generic;
using UnityEngine;

public class UIRewardMultiplier : KAUI
{
	private enum STATE
	{
		ShowDialOnly,
		ShowSingleTimer,
		ShowMultiTimer
	}

	public KAWidget _UIXPDial;

	public KAWidget _UISingleTimer;

	public KAWidget _UIMultiplierText;

	public string _FlashHexColor = "[FF0000]";

	public float _FlashDelay = 0.2f;

	public KAUIMenu _UIMultiTimerMenu;

	public KAWidget _UIMultiTimerTemplate;

	public LocaleString _MultiplierSymbolText = new LocaleString("{0}x");

	private List<RewardMultiplier> mRewardMultipliers;

	private float mCurrentFlashDelay;

	private STATE mShowState;

	[SerializeField]
	private UITable m_UITable;

	protected override void Start()
	{
		base.Start();
		ShowDialOnly();
		if (!m_UITable)
		{
			m_UITable = GetComponentInParent<UITable>();
			if (!m_UITable)
			{
				UtDebug.LogWarning("No UITable component found on parent gameobject of " + base.gameObject.name);
			}
		}
		m_UITable?.Reposition();
	}

	public void Show(List<RewardMultiplier> inRewardMultipliers)
	{
		base.gameObject.SetActive(value: true);
		mRewardMultipliers = inRewardMultipliers;
		if (mRewardMultipliers.Count != 0)
		{
			UpdateXPDialColor();
			_UIXPDial?.SetVisibility(inVisible: true);
			_UIMultiplierText?.SetVisibility(inVisible: true);
			_UIMultiTimerMenu.ClearItems();
			for (int i = 0; i < mRewardMultipliers.Count; i++)
			{
				_UIMultiTimerMenu.AddWidget("Timer " + i).FindChildItem("MultipleTimer")?.SetText((i + 1).ToString());
			}
			m_UITable?.Reposition();
		}
	}

	private void UpdateXPDialColor()
	{
		if ((bool)_UIXPDial?.pBackground)
		{
			_UIXPDial.pBackground.color = ((RewardMultiplierManager.pInstance._MultiplierColors.Count > 0) ? RewardMultiplierManager.pInstance._MultiplierColors[Mathf.Min(mRewardMultipliers.Count - 1, RewardMultiplierManager.pInstance._MultiplierColors.Count - 1)] : Color.white);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mRewardMultipliers == null)
		{
			return;
		}
		int count = mRewardMultipliers.Count;
		mRewardMultipliers.RemoveAll((RewardMultiplier t) => GetRewardMultiplierTimeRemaining(t) <= 0);
		if (mRewardMultipliers.Count == 0)
		{
			TurnOffUI();
			return;
		}
		if (count > 1 && mRewardMultipliers.Count == 1)
		{
			ToggleSingleTimer(toggle: true);
		}
		else if (mRewardMultipliers.Count < count)
		{
			for (int i = 0; i < count - mRewardMultipliers.Count; i++)
			{
				_UIMultiTimerMenu.RemoveWidget(_UIMultiTimerMenu.GetItemAt(0));
			}
			for (int j = 0; j < _UIMultiTimerMenu.GetItems().Count; j++)
			{
				_UIMultiTimerMenu.GetItemAt(j).FindChildItem("MultipleTimer").SetText((j + 1).ToString());
			}
		}
		for (int k = 0; k < mRewardMultipliers.Count; k++)
		{
			UpdateMultiplierWidget(k);
		}
		int num = 0;
		for (int l = 0; l <= mRewardMultipliers.Count - 1; l++)
		{
			num += mRewardMultipliers[l].MultiplierFactor;
		}
		_UIMultiplierText.SetText(string.Format(_MultiplierSymbolText.GetLocalizedString(), num.ToString()));
		UpdateXPDialColor();
	}

	private void UpdateMultiplierWidget(int inIndex)
	{
		int rewardMultiplierTimeRemaining = GetRewardMultiplierTimeRemaining(mRewardMultipliers[inIndex]);
		KAWidget itemAt = _UIMultiTimerMenu.GetItemAt(inIndex);
		if (inIndex == 0 && (bool)_UISingleTimer)
		{
			UpdateUIWidget(rewardMultiplierTimeRemaining, _UISingleTimer);
		}
		UpdateUIWidget(rewardMultiplierTimeRemaining, itemAt);
	}

	private void UpdateUIWidget(int remainingTime, KAWidget inWidget)
	{
		if ((float)remainingTime < RewardMultiplierManager.pInstance._FlashRemainingTime)
		{
			Flash(inWidget, GameUtilities.FormatTimeHHMMSS(remainingTime));
		}
		else
		{
			inWidget.SetText(GameUtilities.FormatTimeHHMMSS(remainingTime));
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		UpdateShowState();
	}

	private void UpdateShowState()
	{
		mShowState = ((mShowState != STATE.ShowMultiTimer) ? (++mShowState) : STATE.ShowDialOnly);
		switch (mShowState)
		{
		case STATE.ShowDialOnly:
			ShowDialOnly();
			break;
		case STATE.ShowSingleTimer:
			ToggleSingleTimer(toggle: true);
			break;
		case STATE.ShowMultiTimer:
			if (mRewardMultipliers.Count > 1)
			{
				ToggleSingleTimer(toggle: false);
			}
			else
			{
				UpdateShowState();
			}
			break;
		}
	}

	public void Flash(KAWidget inWidget, string inText)
	{
		if (mCurrentFlashDelay <= 0f)
		{
			inWidget.SetText(inWidget.GetText().Contains(_FlashHexColor) ? inText.Replace(_FlashHexColor, "") : (_FlashHexColor + inText));
			mCurrentFlashDelay = _FlashDelay;
		}
		else
		{
			mCurrentFlashDelay -= Time.deltaTime;
		}
	}

	private void ToggleSingleTimer(bool toggle)
	{
		_UISingleTimer?.SetVisibility(toggle);
		_UIMultiTimerMenu?.gameObject?.SetActive(!toggle);
		_UIMultiTimerMenu?.SetVisibility(!toggle);
		m_UITable?.Reposition();
	}

	private void ShowDialOnly()
	{
		_UISingleTimer?.SetVisibility(inVisible: false);
		_UIMultiTimerMenu?.gameObject?.SetActive(value: false);
		_UIMultiTimerMenu?.SetVisibility(inVisible: false);
		m_UITable?.Reposition();
	}

	public void TurnOffUI()
	{
		_UISingleTimer?.SetVisibility(inVisible: false);
		_UIMultiTimerMenu?.gameObject?.SetActive(value: false);
		_UIMultiTimerMenu?.SetVisibility(inVisible: false);
		_UIXPDial?.SetVisibility(inVisible: false);
		_UIMultiplierText?.SetVisibility(inVisible: false);
		base.gameObject.SetActive(value: false);
		m_UITable?.Reposition();
	}

	private static int GetRewardMultiplierTimeRemaining(RewardMultiplier inRewardMultiplier)
	{
		if (!ServerTime.pIsReady || !ServerTime.pServerTime.HasValue || !ServerTime.pServerTime.HasValue)
		{
			return 0;
		}
		if (inRewardMultiplier != null)
		{
			return Mathf.Max(0, (int)(inRewardMultiplier.MultiplierEffectTime - ServerTime.pCurrentTime).TotalSeconds);
		}
		return 0;
	}
}
