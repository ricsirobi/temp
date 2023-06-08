using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UiCharactersMenu : KAUIMenu
{
	private UiCharactersInfo mCharacterInfo;

	private Character mSelectedCharacter;

	protected override void Awake()
	{
		base.Awake();
		mCharacterInfo = (UiCharactersInfo)_ParentUi;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		CharacterWidgetData characterWidgetData = (CharacterWidgetData)inWidget.GetUserData();
		if (inWidget.name == "BtnStatus")
		{
			characterWidgetData = (CharacterWidgetData)inWidget.pParentWidget.GetUserData();
			if (characterWidgetData != null)
			{
				characterWidgetData._UiStat.SetupStatMenu(characterWidgetData._Character);
				inWidget.SetDisabled(isDisabled: true);
			}
		}
		else if (characterWidgetData != null && characterWidgetData._Character != null && characterWidgetData._Character != mSelectedCharacter)
		{
			GameManager.pInstance.SelectCharacter(characterWidgetData._Character);
			CameraMovement.pInstance.UpdateCameraFocus(characterWidgetData._Character.transform.position, isForceStopAllowed: true, overrideCurrentMove: true);
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		CharacterWidgetData characterWidgetData = (CharacterWidgetData)inWidget.GetUserData();
		if (characterWidgetData != null)
		{
			if (!characterWidgetData._Character.pIsIncapacitated)
			{
				base.OnHover(inWidget, inIsHover);
			}
		}
		else
		{
			base.OnHover(inWidget, inIsHover);
		}
	}

	public override void OnSelect(KAWidget inWidget, bool inSelected)
	{
		CharacterWidgetData characterWidgetData = (CharacterWidgetData)inWidget.GetUserData();
		if (characterWidgetData != null)
		{
			if (!characterWidgetData._Character.pIsIncapacitated)
			{
				base.OnSelect(inWidget, inSelected);
			}
		}
		else
		{
			base.OnSelect(inWidget, inSelected);
		}
	}

	public void SetupCharacterMenu(List<Character> characters)
	{
		foreach (Character character in characters)
		{
			KAWidget kAWidget = AddWidget(character.name);
			UiCharacterStatus component = Object.Instantiate(mCharacterInfo._UiStatus.gameObject).GetComponent<UiCharacterStatus>();
			CharacterWidgetData characterWidgetData = new CharacterWidgetData(character, kAWidget, component);
			kAWidget.SetUserData(characterWidgetData);
			RaisedPetData raisedPetData = null;
			if (character.pCharacterData.pRaisedPetID > 0)
			{
				raisedPetData = RaisedPetData.GetByID(character.pCharacterData.pRaisedPetID);
			}
			KAWidget kAWidget2 = kAWidget.FindChildItem("AniPortrait");
			if (kAWidget2 != null)
			{
				if (character.tag == "Player" && UserInfo.pIsReady)
				{
					AvPhotoSetter @object = new AvPhotoSetter(kAWidget2);
					GameManager.pInstance.pStillPhotoManager.TakePhotoUI(UserInfo.pInstance.UserID, (Texture2D)kAWidget2.GetTexture(), @object.PhotoCallback, null);
				}
				else if (raisedPetData != null)
				{
					int slotIdx = (raisedPetData.ImagePosition.HasValue ? raisedPetData.ImagePosition.Value : 0);
					ImageData.Load("EggColor", slotIdx, base.gameObject);
				}
				else
				{
					kAWidget2.SetTextureFromBundle(character.pCharacterData._PortraitIcon);
				}
			}
			kAWidget2 = kAWidget.FindChildItem("IcoAttrib");
			if (kAWidget2 != null)
			{
				kAWidget2.SetSprite(Settings.pInstance.GetElementInfo(character.pCharacterData._WeaponData._ElementType)._Icon);
			}
			kAWidget2 = kAWidget.FindChildItem("TxtName");
			if (kAWidget2 != null)
			{
				if (character.tag == "Player" && AvatarData.pIsReady)
				{
					kAWidget2.SetText(AvatarData.pInstance.DisplayName);
				}
				else if (raisedPetData != null)
				{
					kAWidget2.SetText(raisedPetData.Name);
				}
				else
				{
					kAWidget2.SetText(character.pCharacterData._DisplayNameText.GetLocalizedString());
				}
			}
			kAWidget2 = kAWidget.FindChildItem("AniHealthBar");
			if (kAWidget2 != null)
			{
				characterWidgetData._HealthBar = kAWidget2;
				kAWidget2.SetVisibility(!character.pIsIncapacitated);
				if (kAWidget2.GetVisibility())
				{
					float pCurrentValue = character.pCharacterData._Stats._Health.pCurrentValue;
					float num = character.pCharacterData._Stats._Health._Limits.Max;
					if (num <= 0f)
					{
						num = 1f;
					}
					kAWidget2.SetProgressLevel(pCurrentValue / num);
					kAWidget2.SetText(Mathf.CeilToInt(pCurrentValue) + "/" + Mathf.CeilToInt(num));
				}
			}
			kAWidget2 = kAWidget.FindChildItem("BkgUnitCoolDown");
			if (kAWidget2 != null)
			{
				characterWidgetData._BkgCoolDown = kAWidget2;
				kAWidget2.SetVisibility(inVisible: false);
			}
			kAWidget2 = kAWidget.FindChildItem("AniMovement");
			if (kAWidget2 != null)
			{
				characterWidgetData._AniMovement = kAWidget2;
				kAWidget2.SetVisibility(character.CanMove());
			}
			kAWidget2 = kAWidget.FindChildItem("AniActions");
			if (kAWidget2 != null)
			{
				characterWidgetData._AniAction = kAWidget2;
				kAWidget2.SetVisibility(character.CanUseAbility());
			}
			kAWidget2 = kAWidget.FindChildItem("BtnStatus");
			if (kAWidget2 != null)
			{
				characterWidgetData._BtnStatus = kAWidget2;
				kAWidget2.SetDisabled(character.pActiveStatusEffects == null || character.pActiveStatusEffects.Count <= 0);
			}
			kAWidget2 = kAWidget.FindChildItem("IcoIncapacitated");
			if (kAWidget2 != null)
			{
				characterWidgetData._TxtRevive = kAWidget2;
				kAWidget2.SetVisibility(character.pIsIncapacitated);
				if (character.pIsIncapacitated)
				{
					kAWidget2.SetText(character.pCurrentRevivalCountdown.ToString());
				}
			}
			kAWidget2 = kAWidget.FindChildItem("TxtUnitLvl");
			if (kAWidget2 != null)
			{
				characterWidgetData._TxtUnitLvl = kAWidget2;
				kAWidget2.SetText(character.pCharacterData.pLevel.ToString());
			}
			kAWidget2 = kAWidget.FindChildItem("TxtStatus");
			if (!(kAWidget2 != null))
			{
				continue;
			}
			characterWidgetData._TxtStatus = kAWidget2;
			kAWidget2.SetVisibility(character.pIsIncapacitated);
			if (kAWidget2.GetVisibility())
			{
				if (character.pIsIncapacitated && character.pCurrentRevivalCountdown <= 0)
				{
					kAWidget2.SetText(GameManager.pInstance._FledText.GetLocalizedString());
				}
				else if (character.pIsIncapacitated)
				{
					kAWidget2.SetText(GameManager.pInstance._IncapacitatedText.GetLocalizedString());
				}
			}
		}
		StartCoroutine(PositionStatusUi());
	}

	public void OnImageLoaded(ImageDataInstance img)
	{
		if (img.mIconTexture == null)
		{
			return;
		}
		foreach (KAWidget item in GetItems())
		{
			RaisedPetData byID = RaisedPetData.GetByID(((CharacterWidgetData)item.GetUserData())._Character.pCharacterData.pRaisedPetID);
			if (byID != null && (byID.ImagePosition.HasValue ? byID.ImagePosition.Value : 0) == img.mSlotIndex)
			{
				item.FindChildItem("AniPortrait").SetTexture(img.mIconTexture);
				break;
			}
		}
	}

	private IEnumerator PositionStatusUi()
	{
		yield return new WaitForEndOfFrame();
		List<KAUI> list = new List<KAUI>(mCharacterInfo._UiList);
		foreach (KAWidget item in mItemInfo)
		{
			CharacterWidgetData characterWidgetData = (CharacterWidgetData)item.GetUserData();
			if (characterWidgetData != null)
			{
				characterWidgetData._UiStat.transform.parent = item.transform;
				characterWidgetData._UiStat.transform.localPosition = Vector3.zero;
				list.Add(characterWidgetData._UiStat);
			}
		}
		mCharacterInfo._UiList = list.ToArray();
	}

	public void UpdateCharacterInfo(Character character, bool updateAll)
	{
		foreach (KAWidget item in mItemInfo)
		{
			CharacterWidgetData characterWidgetData = (CharacterWidgetData)item.GetUserData();
			if (characterWidgetData._Character.pIsIncapacitated && characterWidgetData._Character.pCurrentRevivalCountdown <= 0)
			{
				if (characterWidgetData._BkgCoolDown != null)
				{
					characterWidgetData._BkgCoolDown.SetVisibility(characterWidgetData._Character.pIsIncapacitated);
				}
				characterWidgetData._Item.SetDisabled(isDisabled: true);
				characterWidgetData._BtnStatus.SetVisibility(inVisible: false);
			}
			if (updateAll)
			{
				if (character == characterWidgetData._Character)
				{
					mSelectedCharacter = character;
					mCharacterInfo.pCharacterAbilitiesMenu.SetupAbilityMenu(character);
				}
				UpdateCharacterDisplayInfo(characterWidgetData);
				KAToggleButton component = item.GetComponent<KAToggleButton>();
				if (component != null)
				{
					component.SetChecked(character == characterWidgetData._Character);
				}
			}
			else if (characterWidgetData._Character == character)
			{
				UpdateCharacterDisplayInfo(characterWidgetData);
				KAToggleButton component2 = item.GetComponent<KAToggleButton>();
				if (component2 != null)
				{
					component2.SetChecked(isChecked: true);
				}
				break;
			}
		}
	}

	private void UpdateCharacterDisplayInfo(CharacterWidgetData data)
	{
		if (data._AniMovement != null)
		{
			data._AniMovement.SetVisibility(data._Character.CanMove());
		}
		if (data._AniAction != null)
		{
			data._AniAction.SetVisibility(data._Character.CanUseAbility());
		}
		if (data._BtnStatus != null)
		{
			if (data._Character.pIsIncapacitated && data._Character.pCurrentRevivalCountdown <= 0)
			{
				data._BtnStatus.SetDisabled(isDisabled: true);
			}
			else
			{
				data._BtnStatus.SetDisabled(data._Character.pActiveStatusEffects == null || data._Character.pActiveStatusEffects.Count <= 0);
			}
		}
		if (data._HealthBar != null)
		{
			data._HealthBar.SetVisibility(!data._Character.pIsIncapacitated);
			if (data._HealthBar.GetVisibility())
			{
				float num = data._Character.pCharacterData._Stats._Health.pCurrentValue / data._Character.pCharacterData._Stats._Health._Limits.Max;
				data._HealthBar.SetProgressLevel(num);
				data._HealthBar.SetText(Mathf.CeilToInt(data._Character.pCharacterData._Stats._Health.pCurrentValue) + "/" + Mathf.CeilToInt(data._Character.pCharacterData._Stats._Health._Limits.Max));
				HealthBarRange[] healthRanges = GameManager.pInstance._HUD._HealthRanges;
				foreach (HealthBarRange healthBarRange in healthRanges)
				{
					if (num <= healthBarRange._Percentage)
					{
						data._HealthBar.GetProgressBar().color = healthBarRange._Color;
						break;
					}
				}
			}
		}
		if (data._TxtRevive != null)
		{
			data._TxtRevive.SetVisibility(data._Character.pIsIncapacitated);
			if (data._Character.pIsIncapacitated)
			{
				data._TxtRevive.SetText(data._Character.pCurrentRevivalCountdown.ToString());
			}
		}
		if (data._TxtStatus != null)
		{
			data._TxtStatus.SetVisibility(data._Character.pIsIncapacitated);
			if (data._TxtStatus.GetVisibility())
			{
				if (data._Character.pIsIncapacitated && data._Character.pCurrentRevivalCountdown <= 0)
				{
					data._TxtStatus.SetText(GameManager.pInstance._FledText.GetLocalizedString());
				}
				else if (data._Character.pIsIncapacitated)
				{
					data._TxtStatus.SetText(GameManager.pInstance._IncapacitatedText.GetLocalizedString());
				}
			}
		}
		if (data._BkgCoolDown != null)
		{
			data._BkgCoolDown.SetVisibility(data._Character.pIsIncapacitated);
		}
	}
}
