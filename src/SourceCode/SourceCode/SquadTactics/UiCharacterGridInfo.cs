using UnityEngine;

namespace SquadTactics;

public class UiCharacterGridInfo : MonoBehaviour
{
	public enum SelectionType
	{
		ALLY,
		ENEMY,
		SELF
	}

	public KAWidget _HealthWidget;

	public KAWidget _DropShadowWidget;

	public KAWidget _SelectionWidget;

	public KAWidget _AlertWidget;

	public KAWidget _RangeWidget;

	public KAWidget _TargetWidget;

	private Character mCharacter;

	public void Initialize(Character character)
	{
		mCharacter = character;
		ShowTargetWidget(active: false);
		ShowRangeWidget(active: false);
		if (UtPlatform.IsMobile())
		{
			ShowDropShadow(active: true);
		}
		else
		{
			ShowDropShadow(active: false);
		}
		UpdateHealthBar();
	}

	public void ShowDropShadow(bool active)
	{
		if (_DropShadowWidget != null)
		{
			_DropShadowWidget.SetVisibility(active);
		}
	}

	public void UpdateHealthBar()
	{
		if (!(_HealthWidget != null))
		{
			return;
		}
		float num = mCharacter.pCharacterData._Stats._Health.pCurrentValue / mCharacter.pCharacterData._Stats._Health._Limits.Max;
		_HealthWidget.SetProgressLevel(num);
		HealthBarRange[] healthRanges = GameManager.pInstance._HUD._HealthRanges;
		foreach (HealthBarRange healthBarRange in healthRanges)
		{
			if (num <= healthBarRange._Percentage)
			{
				_HealthWidget.pBackground.color = healthBarRange._Color;
				break;
			}
		}
	}

	public void ShowRangeWidget(bool active)
	{
		if (_RangeWidget != null)
		{
			_RangeWidget.SetVisibility(active);
		}
	}

	public void ShowSelectionWidget(bool active)
	{
		if (_SelectionWidget != null)
		{
			_SelectionWidget.SetVisibility(active);
		}
	}

	public void ShowTargetWidget(bool active)
	{
		if (_TargetWidget != null)
		{
			_TargetWidget.SetVisibility(active);
		}
	}

	public void ShowAlertWidget(bool active)
	{
		if (_AlertWidget != null)
		{
			_AlertWidget.SetVisibility(active);
		}
	}
}
