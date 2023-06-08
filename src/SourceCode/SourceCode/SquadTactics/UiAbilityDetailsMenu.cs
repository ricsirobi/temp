namespace SquadTactics;

public class UiAbilityDetailsMenu : KAUIMenu
{
	public void ShowEffectDetails(CharacterData character, Ability ability)
	{
		if (ability._Effects == null || ability._Effects.Length == 0)
		{
			return;
		}
		Effect[] effects = ability._Effects;
		foreach (Effect effect in effects)
		{
			KAWidget kAWidget = AddWidget(effect.name);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtAbilityDescription");
			if (kAWidget2 != null)
			{
				if (effect is Tick)
				{
					Tick tick = (Tick)effect;
					StStatInfo statInfoByName = Settings.pInstance.GetStatInfoByName(tick._InfluencingStat);
					string text = effect._InfoText.GetLocalizedString().Replace("{stat}", " [c]" + UtUtilities.GetKAUIColorString(statInfoByName._Color) + character._Stats.GetStatValue(tick._InfluencingStat) + " [/c][ffffff]");
					text = text.Replace("{base}", tick._BaseAmount.ToString());
					text = text.Replace("{mult}", tick._Multiplier.ToString());
					text = text.Replace("{amount}", (character._Stats.GetStatValue(tick._InfluencingStat) * tick._Multiplier + (float)tick._BaseAmount).ToString());
					kAWidget2.SetText(text);
				}
				else
				{
					kAWidget2.SetTextByID(effect._InfoText._ID, effect._InfoText._Text);
				}
			}
			kAWidget2 = kAWidget.FindChildItem("IcoAbility");
			if (effect is Tick)
			{
				Tick tick2 = (Tick)effect;
				kAWidget2.SetSprite(Settings.pInstance.GetEffectIcon(tick2._AppliedEffect));
			}
			else if (effect is Buff)
			{
				Buff buff = (Buff)effect;
				kAWidget2.SetSprite(Settings.pInstance.GetStatEffectIcon(buff._AffectedStat));
			}
			else if (effect is CrowdControl)
			{
				CrowdControl crowdControl = (CrowdControl)effect;
				kAWidget2.SetSprite(Settings.pInstance.GetEffectIcon(crowdControl._AppliedEffect));
			}
			else if (effect is Cleanse)
			{
				Cleanse cleanse = (Cleanse)effect;
				kAWidget2.SetSprite(Settings.pInstance.GetEffectIcon(cleanse._AppliedEffect));
			}
		}
	}
}
