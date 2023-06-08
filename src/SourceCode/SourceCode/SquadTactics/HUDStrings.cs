using System;

namespace SquadTactics;

[Serializable]
public class HUDStrings
{
	public LocaleString _HealText = new LocaleString("healing");

	public LocaleString _DamageText = new LocaleString("damage");

	public LocaleString _LevelQuitConfirmText = new LocaleString("Do you want to quit the level?");

	public LocaleString _LevelRestartConfirmText = new LocaleString("Do you want to restart the level ?");

	public LocaleString _TurnsToUnlockText = new LocaleString("[Review] {0} Turns to unlock");

	public LocaleString _HiddenText = new LocaleString("[Review] ?????");
}
