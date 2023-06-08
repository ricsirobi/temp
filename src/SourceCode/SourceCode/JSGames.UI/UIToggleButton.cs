using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIToggleButton : UIButton
{
	public bool _Checked;

	public bool _Grouped;

	public string _GroupName;

	[Header("Unity Component References")]
	public Image _Checkmark;

	[Header("Effects")]
	public UIEffects _CheckedEffects;

	public UIEffects _CheckedDisabledEffects;

	private bool mChecked;

	public bool pChecked
	{
		get
		{
			return mChecked;
		}
		set
		{
			_Checked = value;
			if (mChecked != value)
			{
				mChecked = value;
				if (_Checkmark != null)
				{
					_Checkmark.gameObject.SetActive(mChecked);
				}
				SelectEffect();
				if (_Grouped && mChecked && base.pParentUI != null)
				{
					base.pParentUI.UncheckOtherToggleButtonsInGroup(_GroupName, this);
				}
				base.pEventTarget?.TriggerOnCheckedChanged(this, mChecked);
			}
		}
	}

	public override void OnPointerClick(PointerEventData eventData)
	{
		if ((bool)KAInput.pInstance && KAInput.pInstance.IsPointerValid(eventData))
		{
			if (base.pInteractableInHierarchy && !mIsDragging && !TouchManager.pInstance.pIsMultiTouch && (!_Grouped || !pChecked))
			{
				pChecked = !pChecked;
			}
			base.OnPointerClick(eventData);
		}
	}

	public override void Initialize(UI parentUI, UIWidget parentWidget)
	{
		UI uI = base.pParentUI;
		base.Initialize(parentUI, parentWidget);
		if (uI != null && _Grouped)
		{
			uI.UnregisterToggleButtonInGroup(_GroupName, this);
		}
		if (parentUI != null && _Grouped)
		{
			parentUI.RegisterToggleButtonInGroup(_GroupName, this);
		}
	}

	protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
	{
		base.OnStateInHierarchyChanged(previousStateInHierarchy, newStateInHierarchy);
		if (pChecked)
		{
			if ((previousStateInHierarchy == WidgetState.DISABLED && mStateInHierarchy != 0) || (previousStateInHierarchy != 0 && mStateInHierarchy == WidgetState.DISABLED))
			{
				SelectEffect();
			}
		}
		else
		{
			base.OnStateInHierarchyChanged(previousStateInHierarchy, newStateInHierarchy);
		}
	}

	protected override void SetParamsFromPublicVariables()
	{
		base.SetParamsFromPublicVariables();
		pChecked = _Checked;
	}

	protected override void SelectEffect()
	{
		if (mStateInHierarchy == WidgetState.DISABLED && pChecked)
		{
			ApplyEffect(_CheckedDisabledEffects);
		}
		else if (pChecked)
		{
			ApplyEffect(_CheckedEffects);
		}
		else
		{
			base.SelectEffect();
		}
	}

	[ContextMenu("Fill Effect References")]
	protected override void FillAllEffectReferences()
	{
		base.FillAllEffectReferences();
		FillEffectReferences(_CheckedEffects);
		FillEffectReferences(_CheckedDisabledEffects);
	}
}
