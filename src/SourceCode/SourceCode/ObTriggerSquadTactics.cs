using SquadTactics;
using UnityEngine;

public class ObTriggerSquadTactics : ObTrigger
{
	public string _Realm = "";

	public override void DoTriggerAction(GameObject other)
	{
		SquadTactics.LevelManager.pRealmToSelect = _Realm;
		base.DoTriggerAction(other);
	}
}
