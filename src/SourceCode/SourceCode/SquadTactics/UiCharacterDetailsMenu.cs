using System.Collections.Generic;

namespace SquadTactics;

public class UiCharacterDetailsMenu : KAUIMenu
{
	private UiCharacterDetails mCharacterInfo;

	public UiAbilityDetails _AbilityDetails;

	protected override void Awake()
	{
		base.Awake();
		mCharacterInfo = (UiCharacterDetails)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		foreach (KAWidget item in GetItems())
		{
			UICharacterAbilityWidgetData uICharacterAbilityWidgetData = (UICharacterAbilityWidgetData)item.GetUserData();
			if (uICharacterAbilityWidgetData._AniCoolDown != null)
			{
				uICharacterAbilityWidgetData._AniCoolDown.SetText(uICharacterAbilityWidgetData._Ability.pCurrentCooldown.ToString());
				uICharacterAbilityWidgetData._AniCoolDown.SetVisibility(uICharacterAbilityWidgetData._Ability.pCurrentCooldown > 0);
			}
			if (item == inWidget)
			{
				mCharacterInfo.ResetAbilitySelection();
				SetAbilityMaterial(item, isSelected: true);
				ShowAbility(item);
			}
			else
			{
				SetAbilityMaterial(item, isSelected: false);
			}
		}
	}

	public void ResetMenu()
	{
		foreach (KAWidget item in GetItems())
		{
			SetAbilityMaterial(item, isSelected: false);
			((KAToggleButton)item).SetChecked(isChecked: false);
		}
	}

	public void SetupAbilityMenu(Weapon weapon, CharacterData character)
	{
		ClearItems();
		List<Ability> abilities = weapon._Abilities;
		if (abilities == null || abilities.Count <= 0)
		{
			return;
		}
		mCharacterInfo.pCurrentAbility = abilities[0];
		foreach (Ability item in abilities)
		{
			KAWidget kAWidget = AddWidget(item.name);
			UICharacterAbilityWidgetData uICharacterAbilityWidgetData = new UICharacterAbilityWidgetData(item, kAWidget, character);
			kAWidget.SetUserData(uICharacterAbilityWidgetData);
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoAbility");
			if (kAWidget2 != null)
			{
				uICharacterAbilityWidgetData._IcoAbility = kAWidget2;
				SetAbilityMaterial(kAWidget, isSelected: false);
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget2 = kAWidget.FindChildItem("AniCoolDown");
			if (kAWidget2 != null)
			{
				uICharacterAbilityWidgetData._AniCoolDown = kAWidget2;
				kAWidget2.SetText(item.pCurrentCooldown.ToString());
				kAWidget2.SetVisibility(item.pCurrentCooldown > 0);
			}
			kAWidget2 = kAWidget.FindChildItem("BkgCoolDown");
			if (kAWidget2 != null)
			{
				uICharacterAbilityWidgetData._BkgCoolDown = kAWidget2;
				kAWidget2.SetVisibility(item.pCurrentCooldown > 0);
			}
			if (mCharacterInfo.pCurrentAbility == item)
			{
				mSelectedItem = kAWidget;
				SetAbilityMaterial(kAWidget, isSelected: true);
				ShowAbility(kAWidget);
			}
		}
		if (mSelectedItem != null)
		{
			KAToggleButton component = mSelectedItem.GetComponent<KAToggleButton>();
			if (component != null)
			{
				component.SetChecked(isChecked: true);
			}
		}
	}

	private void SetAbilityMaterial(KAWidget widget, bool isSelected)
	{
		UICharacterAbilityWidgetData uICharacterAbilityWidgetData = (UICharacterAbilityWidgetData)widget.GetUserData();
		if (uICharacterAbilityWidgetData != null && uICharacterAbilityWidgetData._IcoAbility != null)
		{
			uICharacterAbilityWidgetData._IcoAbility.GetUITexture().material = (isSelected ? uICharacterAbilityWidgetData._Ability._Selected : uICharacterAbilityWidgetData._Ability._Unselected);
		}
	}

	private void ShowAbility(KAWidget widget)
	{
		if (widget != null)
		{
			UICharacterAbilityWidgetData uICharacterAbilityWidgetData = (UICharacterAbilityWidgetData)widget.GetUserData();
			if (uICharacterAbilityWidgetData != null)
			{
				_AbilityDetails.OnSetVisible(visible: true, uICharacterAbilityWidgetData._Character, uICharacterAbilityWidgetData._Ability);
			}
		}
	}
}
