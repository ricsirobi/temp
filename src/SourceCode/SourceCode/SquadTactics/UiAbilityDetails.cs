using UnityEngine;

namespace SquadTactics;

public class UiAbilityDetails : KAUI
{
	public UiCharacterDetails _CharacterDetailsUI;

	public UiCharacterDetailsMenu _CharacterDetailsMenu;

	public GameObject _AbilityGrp;

	public int _ScaleY = 60;

	private UiAbilityDetailsMenu mMenu;

	private KAWidget mTxtSelectedAbilityDetail;

	private KAWidget mTxtSelectedAbilityName;

	private KAWidget mTxtSelectedAbilityType;

	private KAWidget mTxtSelectedAbilityRange;

	private KAWidget mTxtSelectedAbilityTurn;

	private KAWidget mMenuBkg;

	private Vector4 mOrgClip;

	private Vector3 mOrgMenuBkgPos;

	private Vector3 mOrgMenuGridPos;

	private Vector3 mOrgAbilityGrpPos;

	private int mOrgSpriteHeight;

	protected override void Awake()
	{
		base.Awake();
		mMenu = (UiAbilityDetailsMenu)GetMenuByIndex(0);
	}

	protected override void Start()
	{
		base.Start();
		mTxtSelectedAbilityDetail = FindItem("TxtAbilityDescription");
		mTxtSelectedAbilityName = FindItem("TxtAbilityName");
		mTxtSelectedAbilityType = FindItem("TxtAbility");
		mTxtSelectedAbilityRange = FindItem("TxtAbilityRange");
		mTxtSelectedAbilityTurn = FindItem("TxtAbilityCount");
		UIPanel component = mMenu.GetComponent<UIPanel>();
		mOrgClip = component.mClipRange;
		mMenuBkg = FindItem("BkgInfoMenu");
		mOrgMenuBkgPos = mMenuBkg.transform.localPosition;
		UISprite componentInChildren = mMenuBkg.GetComponentInChildren<UISprite>();
		mOrgSpriteHeight = componentInChildren.height;
		mOrgMenuGridPos = mMenu._DefaultGrid.transform.localPosition;
		mOrgAbilityGrpPos = _AbilityGrp.transform.localPosition;
	}

	private void ResetPosition()
	{
		mMenu.GetComponent<UIPanel>().mClipRange = mOrgClip;
		mMenuBkg.transform.localPosition = mOrgMenuBkgPos;
		mMenuBkg.GetComponentInChildren<UISprite>().height = mOrgSpriteHeight;
		mMenu._DefaultGrid.transform.localPosition = mOrgMenuGridPos;
		_AbilityGrp.transform.localPosition = mOrgAbilityGrpPos;
	}

	public void OnSetVisible(bool visible, CharacterData character, Ability ability)
	{
		SetVisibility(visible);
		if (!visible)
		{
			return;
		}
		mMenu.ClearItems();
		ResetPosition();
		StStatInfo statInfoByName = Settings.pInstance.GetStatInfoByName(ability._InfluencingStat);
		if (mTxtSelectedAbilityName != null)
		{
			mTxtSelectedAbilityName.SetText(ability._Name.GetLocalizedString());
		}
		int num = Mathf.RoundToInt(character._Stats.GetStatValue(ability._InfluencingStat) * ability._InfluencingStatMultiplier + (float)ability._BaseAmount);
		if (mTxtSelectedAbilityDetail != null)
		{
			string text = ability._InfoText.GetLocalizedString().Replace("{stat}", " [c]" + UtUtilities.GetKAUIColorString(statInfoByName._Color) + character._Stats.GetStatValue(ability._InfluencingStat) + " [/c][ffffff]");
			text = text.Replace("{base}", ability._BaseAmount.ToString());
			text = text.Replace("{mult}", ability._InfluencingStatMultiplier.ToString());
			text = text.Replace("{amount}", num.ToString());
			mTxtSelectedAbilityDetail.SetText(text);
		}
		string text2 = ((ability._TargetType == Ability.Team.SAME && character._Team == Character.Team.PLAYER) ? _CharacterDetailsUI._HealText.GetLocalizedString() : _CharacterDetailsUI._DamageText.GetLocalizedString());
		if (mTxtSelectedAbilityType != null)
		{
			if (num <= 0)
			{
				text2 = "";
			}
			else
			{
				string text3 = " [c]" + UtUtilities.GetKAUIColorString(statInfoByName._Color) + statInfoByName._AbbreviationText.GetLocalizedString() + " [/c][ffffff]";
				text2 = num + text3 + text2;
			}
			mTxtSelectedAbilityType.SetText(text2);
			mTxtSelectedAbilityType.SetVisibility(num > 0);
		}
		if (mTxtSelectedAbilityRange != null)
		{
			mTxtSelectedAbilityRange.SetText(_CharacterDetailsUI.GetRangeTextFromRange(ability._Range));
		}
		if (mTxtSelectedAbilityTurn != null)
		{
			mTxtSelectedAbilityTurn.SetText(ability._Cooldown.ToString());
		}
		mMenuBkg.SetVisibility(ability._Effects != null && ability._Effects.Length != 0);
		_AbilityGrp.transform.localPosition = new Vector3(_AbilityGrp.transform.localPosition.x, _ScaleY, _AbilityGrp.transform.localPosition.z);
		mMenu.ShowEffectDetails(character, ability);
	}

	public void OnSetVisible(AbilityInfo info)
	{
		SetVisibility(inVisible: true);
		mMenu.ClearItems();
		ResetPosition();
		if (mTxtSelectedAbilityName != null)
		{
			mTxtSelectedAbilityName.SetText(info._NameText.GetLocalizedString());
		}
		if (mTxtSelectedAbilityDetail != null)
		{
			mTxtSelectedAbilityDetail.SetText(info._DescriptionText.GetLocalizedString());
		}
		mTxtSelectedAbilityType.SetVisibility(inVisible: false);
		if (mTxtSelectedAbilityRange != null)
		{
			mTxtSelectedAbilityRange.SetVisibility(inVisible: false);
		}
		if (mTxtSelectedAbilityTurn != null)
		{
			mTxtSelectedAbilityTurn.SetVisibility(inVisible: false);
		}
		mMenuBkg.SetVisibility(inVisible: false);
		_CharacterDetailsMenu.ResetMenu();
	}
}
