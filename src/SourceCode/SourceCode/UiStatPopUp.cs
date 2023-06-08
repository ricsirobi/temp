using System.Collections.Generic;
using System.Linq;
using SquadTactics;

public class UiStatPopUp : KAUI
{
	private KAUIMenu mContentMenuCombat;

	private KAUIMenu mContentMenuRacing;

	public LocaleString _PopupHeadingText = new LocaleString("[Review]'s Stats");

	protected override void Start()
	{
		mContentMenuCombat = _MenuList[0];
		mContentMenuRacing = _MenuList[1];
		base.Start();
	}

	public void ShowCombatStats(CharacterData characterData, int rank, string name, ItemStat[] stats = null)
	{
		KAUI.SetExclusive(this);
		KAWidget kAWidget = FindItem("DragonTitle");
		if (!string.IsNullOrEmpty(name))
		{
			kAWidget.SetText(name + _PopupHeadingText.GetLocalizedString());
		}
		else
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		FindItem("CombatStatTitle").SetVisibility(inVisible: true);
		if (characterData == null || characterData._Stats == null)
		{
			return;
		}
		characterData._Stats.SetInitialValues(rank, stats);
		List<StStatInfo> list = Settings.pInstance._StatInfo.OrderBy((StStatInfo x) => x._StatID).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i]._Display)
			{
				KAWidget kAWidget2 = mContentMenuCombat.AddWidget(mContentMenuCombat._Template.name);
				kAWidget2.FindChildItem("CombatStatWidget").SetText(list[i]._DisplayText.GetLocalizedString());
				KAWidget kAWidget3 = kAWidget2.FindChildItem("CombatStatValueWidget");
				if (list[i]._Stat == SquadTactics.Stat.CRITICALCHANCE || list[i]._Stat == SquadTactics.Stat.DODGE)
				{
					kAWidget3.SetText(characterData._Stats.GetStat(list[i]._Stat).GetMultipliedValue().ToString());
				}
				else
				{
					kAWidget3.SetText(characterData._Stats.GetStat(list[i]._Stat).pCurrentValue.ToString());
				}
			}
		}
	}

	public void ShowRacingStats(SanctuaryPetTypeInfo sanctuaryPetInfo)
	{
		FindItem("RacingStatTitle").SetVisibility(inVisible: true);
		SanctuaryPetStats stats = sanctuaryPetInfo._Stats;
		for (int i = 0; i < SanctuaryData.pInstance._StatSettings.Length; i++)
		{
			if (SanctuaryData.pInstance._StatSettings[i]._Display)
			{
				float num = (float)stats.GetType().GetField(SanctuaryData.pInstance._StatSettings[i]._Name).GetValue(stats);
				KAWidget kAWidget = mContentMenuRacing.AddWidget(mContentMenuRacing._Template.name);
				kAWidget.FindChildItem("RacingStatWidget").SetText(SanctuaryData.pInstance._StatSettings[i]._StatText.GetLocalizedString());
				kAWidget.FindChildItem("RacingStatValueWidget").SetText(num.ToString());
			}
		}
	}

	public void ShowDescription(ItemData item)
	{
		if (item != null && !string.IsNullOrEmpty(item.Description))
		{
			FindItem("Description").SetText(item.Description);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			KAUI.RemoveExclusive(this);
			mContentMenuCombat.ClearItems();
			mContentMenuRacing.ClearItems();
			SetVisibility(inVisible: false);
		}
	}
}
