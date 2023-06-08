using UnityEngine;

namespace SquadTactics;

public class UiAbilityInfo : KAUI
{
	public GameObject _AbilityGrp;

	public int _ScaleY = 60;

	private UiAbilityInfoMenu mMenu;

	public static UiAbilityInfo pInstance;

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
		if (pInstance == null)
		{
			pInstance = this;
		}
		else
		{
			Object.DestroyObject(base.gameObject);
		}
		mMenu = (UiAbilityInfoMenu)GetMenuByIndex(0);
	}

	protected override void Start()
	{
		base.Start();
		mTxtSelectedAbilityDetail = FindItem("TxtAbilityDescription");
		mTxtSelectedAbilityName = FindItem("TxtAbilityName");
		mTxtSelectedAbilityType = FindItem("TxtAbility");
		mTxtSelectedAbilityRange = FindItem("TxtAbilityRange");
		mTxtSelectedAbilityTurn = FindItem("TxtAbilityCount");
		SetVisibility(inVisible: false);
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

	public void OnSetVisible(bool visible, Character character, Ability ability)
	{
		mMenu.ClearItems();
		ResetPosition();
		StStatInfo statInfoByName = Settings.pInstance.GetStatInfoByName(ability._InfluencingStat);
		if (mTxtSelectedAbilityName != null)
		{
			mTxtSelectedAbilityName.SetText(ability._Name.GetLocalizedString());
		}
		int num = Mathf.RoundToInt(character.pCharacterData._Stats.GetStatValue(ability._InfluencingStat) * ability._InfluencingStatMultiplier + (float)ability._BaseAmount);
		if (mTxtSelectedAbilityDetail != null)
		{
			string text = ability._InfoText.GetLocalizedString().Replace("{stat}", " [c]" + UtUtilities.GetKAUIColorString(statInfoByName._Color) + character.pCharacterData._Stats.GetStatValue(ability._InfluencingStat) + " [/c][ffffff]");
			text = text.Replace("{base}", ability._BaseAmount.ToString());
			text = text.Replace("{mult}", ability._InfluencingStatMultiplier.ToString());
			text = text.Replace("{amount}", num.ToString());
			mTxtSelectedAbilityDetail.SetText(text);
		}
		string text2 = ((ability._TargetType == Ability.Team.SAME && character.pCharacterData._Team == Character.Team.PLAYER) ? GameManager.pInstance._HUD._HUDStrings._HealText.GetLocalizedString() : GameManager.pInstance._HUD._HUDStrings._DamageText.GetLocalizedString());
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
			mTxtSelectedAbilityRange.SetText(GameManager.pInstance._HUD.GetRangeTextFromRange(ability._Range));
		}
		if (mTxtSelectedAbilityTurn != null)
		{
			mTxtSelectedAbilityTurn.SetText(ability._Cooldown.ToString());
		}
		SetVisibility(visible);
		if (visible)
		{
			DynamicUpdatePanel(ability);
			mMenu.ShowEffectDetails(character, ability);
		}
	}

	public void DynamicUpdatePanel(Ability ability)
	{
		if (ability._Effects != null && ability._Effects.Length != 0)
		{
			mMenuBkg.SetVisibility(inVisible: true);
			int num = ability._Effects.Length;
			int num2 = num * _ScaleY;
			if (num > 1)
			{
				UISprite componentInChildren = mMenuBkg.GetComponentInChildren<UISprite>();
				float num3 = (float)((num - 1) * _ScaleY) * 0.5f;
				componentInChildren.height = num2;
				UIPanel component = mMenu.GetComponent<UIPanel>();
				component.mClipRange = new Vector4(component.mClipRange.x, component.mClipRange.y + num3, component.mClipRange.z, num2);
				mMenu._DefaultGrid.transform.localPosition = new Vector3(mMenu._DefaultGrid.transform.localPosition.x, mMenu._DefaultGrid.transform.localPosition.y + num3 * 2f, mMenu._DefaultGrid.transform.localPosition.z);
				mMenuBkg.transform.localPosition = new Vector3(mMenuBkg.transform.localPosition.x, mMenuBkg.transform.localPosition.y + num3, mMenuBkg.transform.localPosition.z);
				_AbilityGrp.transform.localPosition = new Vector3(_AbilityGrp.transform.localPosition.x, _AbilityGrp.transform.localPosition.y + num3 * 2f, _AbilityGrp.transform.localPosition.z);
			}
		}
		else
		{
			mMenuBkg.SetVisibility(inVisible: false);
			_AbilityGrp.transform.localPosition = new Vector3(_AbilityGrp.transform.localPosition.x, _AbilityGrp.transform.localPosition.y - (float)_ScaleY, _AbilityGrp.transform.localPosition.z);
		}
	}
}
