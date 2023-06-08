using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior_Arbiter_BeaconScanner : AIBehavior_Arbiter
{
	public float _TimeBetweenScans = 1f;

	private Dictionary<AIBeacon, AIEvaluator> mBeacons = new Dictionary<AIBeacon, AIEvaluator>();

	public override AIBehaviorState Think(AIActor Actor)
	{
		bool flag = true;
		if (Actor is AIActor_Pet && ((AIActor_Pet)Actor).SanctuaryPet.IsMMOPlayer())
		{
			flag = false;
		}
		if (flag && AIBeacon.Beacons != null && mBeacons.Count != AIBeacon.Beacons.Count)
		{
			ScanForNewBeacons(Actor);
		}
		return base.Think(Actor);
	}

	private void ScanForNewBeacons(AIActor Actor)
	{
		ArrayList arrayList = new ArrayList();
		foreach (KeyValuePair<AIBeacon, AIEvaluator> mBeacon in mBeacons)
		{
			if (!AIBeacon.Beacons.Contains(mBeacon.Key))
			{
				arrayList.Add(mBeacon.Key);
			}
		}
		foreach (AIBeacon item in arrayList)
		{
			DropBeacon(item, Actor);
		}
		foreach (AIBeacon beacon in AIBeacon.Beacons)
		{
			if (beacon != null && !mBeacons.ContainsKey(beacon) && CanPerceive(beacon, Actor) && beacon.CanBePerceivedBy(Actor, this))
			{
				PushBeacon(beacon, Actor);
			}
		}
	}

	public void DropBeacon(AIBeacon Beacon, AIActor Actor)
	{
		if (!mBeacons.ContainsKey(Beacon))
		{
			return;
		}
		AIEvaluator aIEvaluator = mBeacons[Beacon];
		if (aIEvaluator != null)
		{
			RemoveEvaluator(aIEvaluator, Actor);
			if (Beacon != null)
			{
				aIEvaluator.transform.parent = Beacon.transform;
			}
			else
			{
				Object.Destroy(aIEvaluator.gameObject);
			}
		}
		mBeacons.Remove(Beacon);
	}

	public void PushBeacon(AIBeacon Beacon, AIActor Actor)
	{
		AIEvaluator evaluator = Beacon.GetEvaluator();
		mBeacons[Beacon] = evaluator;
		PushEvaluator(evaluator);
	}

	public virtual bool CanPerceive(AIBeacon Beacon, AIActor Actor)
	{
		return true;
	}
}
