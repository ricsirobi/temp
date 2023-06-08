using System.Collections.Generic;

namespace SquadTactics;

public class UiCharacterAbilitesMenu : KAUIMenu
{
	private UiCharactersInfo mCharacterInfo;

	private CharacterAbilityWidgetData mSelectedWidgetData;

	protected override void Awake()
	{
		base.Awake();
		mCharacterInfo = (UiCharactersInfo)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)inWidget.GetUserData();
		if (!IsUnderCoolDown(inWidget) && characterAbilityWidgetData._Ability.pCurrentCooldown <= 0)
		{
			base.OnClick(inWidget);
		}
		if (mSelectedWidgetData._Item == inWidget || characterAbilityWidgetData._Ability.pCurrentCooldown > 0)
		{
			return;
		}
		mSelectedWidgetData._Ability.OnAbilityUsed -= OnUseAbility;
		foreach (KAWidget item in GetItems())
		{
			CharacterAbilityWidgetData characterAbilityWidgetData2 = (CharacterAbilityWidgetData)item.GetUserData();
			PlayAbilityAnim(item);
			if (characterAbilityWidgetData2._AniCoolDown != null)
			{
				characterAbilityWidgetData2._AniCoolDown.SetText(characterAbilityWidgetData2._Ability.pCurrentCooldown.ToString());
				characterAbilityWidgetData2._AniCoolDown.SetVisibility(characterAbilityWidgetData2._Ability.pCurrentCooldown > 0);
			}
			if (item == inWidget && !IsUnderCoolDown(inWidget))
			{
				mSelectedWidgetData = characterAbilityWidgetData2;
				SetAbilityMaterial(item, isSelected: true);
				GameManager.pInstance.SetAbility(characterAbilityWidgetData2._Ability);
				mCharacterInfo.SetSelectedAbilityInfo(characterAbilityWidgetData2._Character, characterAbilityWidgetData2._Ability);
				mCharacterInfo.ShowSmallAbilityInfo(!UiAbilityInfo.pInstance.GetVisibility() && characterAbilityWidgetData2._Ability.pCurrentCooldown <= 0 && characterAbilityWidgetData2._Character._HasAbilityAction && GameManager.pInstance._GameState != GameManager.GameState.ENEMY);
				mSelectedWidgetData._Ability.OnAbilityUsed += OnUseAbility;
				if (InteractiveTutManager._CurrentActiveTutorialObject != null)
				{
					Tutorial component = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<Tutorial>();
					if (component != null)
					{
						component.TutorialManagerAsyncMessage(inWidget.name);
					}
				}
			}
			else
			{
				SetAbilityMaterial(item, isSelected: false);
			}
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)inWidget.GetUserData();
		if (!IsUnderCoolDown(inWidget) && characterAbilityWidgetData._Ability.pCurrentCooldown <= 0)
		{
			base.OnSelect(inWidget, inSelected);
		}
	}

	public void SetupAbilityMenu(Character character)
	{
		ClearItems();
		if (mSelectedWidgetData != null)
		{
			mSelectedWidgetData._Ability.OnAbilityUsed -= OnUseAbility;
		}
		if (character.pIsIncapacitated && character.pCurrentRevivalCountdown <= 0)
		{
			mCharacterInfo.ShowSmallAbilityInfo(show: false);
			return;
		}
		List<Ability> pAbilities = character.pAbilities;
		if (pAbilities == null || pAbilities.Count <= 0)
		{
			return;
		}
		foreach (Ability item in pAbilities)
		{
			KAWidget kAWidget = AddWidget(item.name);
			CharacterAbilityWidgetData characterAbilityWidgetData = new CharacterAbilityWidgetData(item, kAWidget, character);
			kAWidget.SetUserData(characterAbilityWidgetData);
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoAbility");
			if (kAWidget2 != null)
			{
				characterAbilityWidgetData._IcoAbility = kAWidget2;
				SetAbilityMaterial(kAWidget, isSelected: false);
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget2 = kAWidget.FindChildItem("AniCoolDown");
			if (kAWidget2 != null)
			{
				characterAbilityWidgetData._AniCoolDown = kAWidget2;
				kAWidget2.SetText(item.pCurrentCooldown.ToString());
				kAWidget2.SetVisibility(item.pCurrentCooldown > 0);
			}
			kAWidget2 = kAWidget.FindChildItem("BkgCoolDown");
			if (kAWidget2 != null)
			{
				characterAbilityWidgetData._BkgCoolDown = kAWidget2;
				kAWidget2.SetVisibility(item.pCurrentCooldown > 0);
			}
			if (character.pCurrentAbility == item && !IsUnderCoolDown(kAWidget))
			{
				mSelectedItem = kAWidget;
				mSelectedWidgetData = characterAbilityWidgetData;
				SetAbilityMaterial(kAWidget, isSelected: true);
				mSelectedWidgetData._Ability.OnAbilityUsed += OnUseAbility;
				mCharacterInfo.SetSelectedAbilityInfo(mSelectedWidgetData._Character, mSelectedWidgetData._Ability);
			}
		}
		KAToggleButton component = mSelectedItem.GetComponent<KAToggleButton>();
		if (component != null)
		{
			component.SetChecked(isChecked: true);
		}
		CheckAbilityAnims();
		mCharacterInfo.ShowSmallAbilityInfo(mSelectedWidgetData._Ability.pCurrentCooldown <= 0 && character._HasAbilityAction && GameManager.pInstance._GameState != GameManager.GameState.ENEMY);
	}

	private void SetAbilityMaterial(KAWidget widget, bool isSelected)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)widget.GetUserData();
		if (characterAbilityWidgetData != null && characterAbilityWidgetData._IcoAbility != null)
		{
			characterAbilityWidgetData._IcoAbility.GetUITexture().material = (isSelected ? characterAbilityWidgetData._Ability._Selected : characterAbilityWidgetData._Ability._Unselected);
		}
	}

	private void OnUseAbility()
	{
		if (mSelectedWidgetData != null)
		{
			if (mSelectedWidgetData._AniCoolDown != null)
			{
				mSelectedWidgetData._AniCoolDown.SetText(mSelectedWidgetData._Ability.pCurrentCooldown.ToString());
				mSelectedWidgetData._AniCoolDown.SetVisibility(mSelectedWidgetData._Ability.pCurrentCooldown > 0);
			}
			CheckAbilityAnims();
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)inWidget.GetUserData();
		if (!IsUnderCoolDown(inWidget) && characterAbilityWidgetData._Ability.pCurrentCooldown <= 0)
		{
			base.OnHover(inWidget, inIsHover);
		}
	}

	public override void OnShowUITooltip(KATooltip inTooltip, KAWidget inWidget, bool inShow)
	{
		base.OnShowUITooltip(inTooltip, inWidget, inShow);
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)inWidget.GetUserData();
		if (characterAbilityWidgetData != null)
		{
			UiAbilityInfo.pInstance.OnSetVisible(inShow, characterAbilityWidgetData._Character, characterAbilityWidgetData._Ability);
			mCharacterInfo.ShowSmallAbilityInfo(!inShow && characterAbilityWidgetData._Character.pHasAbilityAction);
			if (!inShow)
			{
				PlayAbilityAnim(inWidget);
				inTooltip.enabled = false;
			}
		}
	}

	private bool IsUnderCoolDown(KAWidget inWidget)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)inWidget.GetUserData();
		if (characterAbilityWidgetData != null && (GameManager.pInstance._GameState == GameManager.GameState.ENEMY || !characterAbilityWidgetData._Character.pHasAbilityAction || characterAbilityWidgetData._Character.pIsIncapacitated || GameManager.pInstance._TurnState == GameManager.TurnState.ABILITYONGOING))
		{
			return true;
		}
		return false;
	}

	public void CheckAbilityAnims()
	{
		foreach (KAWidget item in GetItems())
		{
			PlayAbilityAnim(item);
		}
	}

	private void PlayAbilityAnim(KAWidget widget)
	{
		CharacterAbilityWidgetData characterAbilityWidgetData = (CharacterAbilityWidgetData)widget.GetUserData();
		characterAbilityWidgetData._BkgCoolDown.SetVisibility(IsUnderCoolDown(widget) || characterAbilityWidgetData._Ability.pCurrentCooldown > 0);
		SetWidgetCooldownAlpha(IsUnderCoolDown(widget) || characterAbilityWidgetData._Ability.pCurrentCooldown > 0, characterAbilityWidgetData);
		SetAbilityMaterial(widget, widget == mSelectedWidgetData._Item && !IsUnderCoolDown(widget));
	}

	private void SetWidgetCooldownAlpha(bool isCooldown, CharacterAbilityWidgetData data)
	{
		data._BkgCoolDown.pBackground.alpha = 1f;
		data._Item.pBackground.alpha = 1f;
		data._IcoAbility.GetUITexture().alpha = 1f;
		if (isCooldown)
		{
			data._BkgCoolDown.pBackground.alpha = mCharacterInfo._CooldownAlpha;
			data._Item.pBackground.alpha = mCharacterInfo._CooldownAlpha;
			data._IcoAbility.GetUITexture().alpha = mCharacterInfo._CooldownAlpha;
		}
	}
}
