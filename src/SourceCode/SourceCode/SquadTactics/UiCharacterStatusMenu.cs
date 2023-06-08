using UnityEngine;

namespace SquadTactics;

public class UiCharacterStatusMenu : KAUIMenu
{
	public void SetupStatMenu(Character character)
	{
		foreach (Effect pActiveStatusEffect in character.pActiveStatusEffects)
		{
			if (pActiveStatusEffect._Duration <= 0)
			{
				continue;
			}
			KAWidget kAWidget = AddWidget("Effect");
			KAWidget kAWidget2 = kAWidget.FindChildItem("IcoStat");
			KAWidget kAWidget3 = kAWidget.FindChildItem("TxtStatName");
			KAWidget kAWidget4 = kAWidget.FindChildItem("TxtTurnCount");
			KAWidget kAWidget5 = kAWidget.FindChildItem("AniEffectArrow");
			if (pActiveStatusEffect is Tick)
			{
				Tick tick = (Tick)pActiveStatusEffect;
				kAWidget.name = tick._AppliedEffect._Name;
				if (kAWidget2 != null)
				{
					kAWidget2.SetSprite(Settings.pInstance.GetEffectIcon(tick._AppliedEffect));
				}
				float pAmount = tick.pAmount;
				if (kAWidget3 != null)
				{
					kAWidget3.SetText(Mathf.Abs(pAmount).ToString());
				}
				if (kAWidget5 != null)
				{
					string animName = GameManager.pInstance._HUD._NegativeArrowIcon;
					if (tick.IsPositive())
					{
						animName = GameManager.pInstance._HUD._PositiveArrowIcon;
					}
					kAWidget5.pAnim2D.Play(animName);
				}
				if (kAWidget4 != null)
				{
					kAWidget4.SetText(tick._Duration.ToString());
				}
			}
			else if (pActiveStatusEffect is Buff)
			{
				Buff buff = (Buff)pActiveStatusEffect;
				kAWidget.name = buff._AffectedStat.ToString();
				if (kAWidget2 != null)
				{
					kAWidget2.SetSprite(Settings.pInstance.GetStatEffectIcon(buff._AffectedStat));
				}
				float pAmount2 = buff.pAmount;
				if (kAWidget3 != null)
				{
					kAWidget3.SetText(Mathf.Abs(pAmount2).ToString());
				}
				if (kAWidget5 != null)
				{
					string animName2 = GameManager.pInstance._HUD._NegativeArrowIcon;
					if (buff.IsPositive())
					{
						animName2 = GameManager.pInstance._HUD._PositiveArrowIcon;
					}
					kAWidget5.pAnim2D.Play(animName2);
				}
				if (kAWidget4 != null)
				{
					kAWidget4.SetText(buff._Duration.ToString());
				}
			}
			else
			{
				if (!(pActiveStatusEffect is CrowdControl))
				{
					continue;
				}
				CrowdControl crowdControl = (CrowdControl)pActiveStatusEffect;
				kAWidget.name = crowdControl._AppliedEffect._Name;
				if (kAWidget2 != null)
				{
					kAWidget2.SetSprite(Settings.pInstance.GetEffectIcon(crowdControl._AppliedEffect));
				}
				if (kAWidget3 != null)
				{
					kAWidget3.SetText(Settings.pInstance.GetEffectName(crowdControl._AppliedEffect));
				}
				if (kAWidget5 != null)
				{
					string animName3 = GameManager.pInstance._HUD._NegativeArrowIcon;
					if (crowdControl.IsPositive())
					{
						animName3 = GameManager.pInstance._HUD._PositiveArrowIcon;
					}
					kAWidget5.pAnim2D.Play(animName3);
				}
				if (kAWidget4 != null)
				{
					kAWidget4.SetText(crowdControl._Duration.ToString());
				}
			}
		}
	}
}
