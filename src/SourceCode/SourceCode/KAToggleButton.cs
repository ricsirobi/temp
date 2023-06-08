public class KAToggleButton : KAButton
{
	public KASkinInfo _CheckedInfo = new KASkinInfo();

	public KASkinInfo _CheckedDisabledInfo = new KASkinInfo();

	public KATooltipInfo _CheckedTooltipInfo = new KATooltipInfo();

	public bool _Grouped;

	public bool _StartChecked;

	public bool _AllowCheckedHover;

	public bool _AllowCheckedOnPress;

	private KAToggleButton[] mToggleButtons;

	private KATooltipInfo mCachedTooltipInfo;

	protected bool mChecked;

	protected override void Awake()
	{
		base.Awake();
		mCachedTooltipInfo = _TooltipInfo;
	}

	private void Start()
	{
		if (base.transform.parent != null)
		{
			mToggleButtons = base.transform.parent.GetComponentsInChildren<KAToggleButton>();
		}
		if (_StartChecked)
		{
			SetChecked(_StartChecked);
		}
	}

	public bool IsChecked()
	{
		return mChecked;
	}

	public KAToggleButton GetCheckedItemInGroup()
	{
		if (mChecked)
		{
			return this;
		}
		if (mToggleButtons == null && base.transform.parent != null)
		{
			mToggleButtons = base.transform.parent.GetComponentsInChildren<KAToggleButton>();
		}
		KAToggleButton[] array = mToggleButtons;
		foreach (KAToggleButton kAToggleButton in array)
		{
			if (kAToggleButton != this && kAToggleButton.IsChecked())
			{
				return kAToggleButton;
			}
		}
		return null;
	}

	public void SetChecked(bool isChecked)
	{
		if ((mChecked ^ isChecked) && _Grouped && isChecked)
		{
			UnCheckOtherGroupMembers();
		}
		mChecked = isChecked;
		OnChecked();
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (inVisible && mChecked)
		{
			SetChecked(isChecked: true);
		}
	}

	private void UnCheckOtherGroupMembers()
	{
		if (mToggleButtons == null && base.transform.parent != null)
		{
			mToggleButtons = base.transform.parent.GetComponentsInChildren<KAToggleButton>();
		}
		KAToggleButton[] array = mToggleButtons;
		foreach (KAToggleButton kAToggleButton in array)
		{
			if (kAToggleButton != this && kAToggleButton._Grouped)
			{
				kAToggleButton.SetChecked(isChecked: false);
			}
		}
	}

	public override void OnHover(bool inIsHover)
	{
		if (!mChecked || _AllowCheckedHover)
		{
			base.OnHover(inIsHover);
		}
	}

	public virtual void OnChecked()
	{
		PlayAnim("Normal");
		_CheckedInfo.DoEffect(mChecked, this);
		UpdateTooltipInfo(mChecked);
	}

	public void UpdateTooltipInfo(bool isChecked)
	{
		if (!string.IsNullOrEmpty(_CheckedTooltipInfo._Text.GetLocalizedString()))
		{
			if (isChecked)
			{
				_TooltipInfo = _CheckedTooltipInfo;
			}
			else
			{
				_TooltipInfo = mCachedTooltipInfo;
			}
		}
	}

	public override void OnPress(bool inPressed)
	{
		if (!mChecked || _AllowCheckedOnPress)
		{
			base.OnPress(inPressed);
		}
	}

	public override void OnClick()
	{
		base.OnClick();
		if (IsActive())
		{
			bool flag = _Grouped || !mChecked;
			if (flag != mChecked)
			{
				SetChecked(flag);
			}
		}
	}

	public override void ResetWidget(bool resetPosition = true)
	{
		base.ResetWidget(resetPosition);
		OnChecked();
	}

	protected override void OnSetState(KAUIState inState)
	{
		if (mChecked)
		{
			if (inState == KAUIState.INTERACTIVE)
			{
				inState = ((mParentState == KAUIState.INTERACTIVE) ? GetState() : mParentState);
			}
			switch (inState)
			{
			case KAUIState.NOT_INTERACTIVE:
			case KAUIState.DISABLED:
				EnableCollider(inEnable: false);
				break;
			case KAUIState.INTERACTIVE:
				EnableCollider(inEnable: true);
				break;
			}
			ResetEffects();
			if (inState == KAUIState.DISABLED || mParentState == KAUIState.DISABLED)
			{
				_CheckedDisabledInfo.DoEffect(inShowEffect: true, this);
			}
			else
			{
				_CheckedDisabledInfo.DoEffect(inShowEffect: false, this);
			}
			if (inState == KAUIState.INTERACTIVE || inState == KAUIState.NOT_INTERACTIVE)
			{
				_CheckedInfo.DoEffect(inShowEffect: true, this);
			}
			{
				foreach (KAWidget mChildWidget in mChildWidgets)
				{
					mChildWidget.OnParentSetState(inState);
				}
				return;
			}
		}
		base.OnSetState(inState);
	}
}
