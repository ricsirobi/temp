using UnityEngine;

public class ArenaFrenzyCatchableTarget : ArenaFrenzyTarget
{
	protected override bool ProcessTargetHit(Collider inOther)
	{
		return true;
	}
}
