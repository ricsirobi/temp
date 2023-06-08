using UnityEngine;

public class ObCollectDragon : ObCollect
{
	public float _EnergyCost;

	public float _HappinessCost;

	[HideInInspector]
	public bool DragonIsPickingItUp;

	protected override void OnTriggerEnter(Collider coll)
	{
		if (!coll.transform.root.CompareTag("Player") || !DragonIsPickingItUp)
		{
			base.OnTriggerEnter(coll);
		}
	}

	protected override void Collected()
	{
		base.Collected();
		if (SanctuaryManager.pCurPetInstance != null)
		{
			if (_HappinessCost != 0f)
			{
				SanctuaryManager.pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, 0f - _HappinessCost);
			}
			if (_EnergyCost != 0f)
			{
				SanctuaryManager.pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.ENERGY, 0f - _EnergyCost);
			}
			SanctuaryManager.pCurPetInstance.ItemCollected();
		}
	}

	public virtual void OnItemCollected(AIActor inAI)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.mMountDisabled = false;
		}
		if (_MessageObject != null)
		{
			_MessageObject.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (AvAvatar.pObject != null)
			{
				AvAvatar.pObject.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (!mSendMessageOnly)
		{
			OnItemCollected();
		}
	}

	public override void OnItemCollected()
	{
		if (DragonIsPickingItUp && SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.mMountDisabled = false;
		}
		if (!base.pCollected)
		{
			base.OnItemCollected();
		}
		RemoveBeacon();
	}

	public void PickedByDragon()
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pCurPetInstance.mMountDisabled = true;
		}
		DragonIsPickingItUp = true;
	}

	private void RemoveBeacon()
	{
		AIBeacon componentInChildren = base.transform.GetComponentInChildren<AIBeacon>();
		if (componentInChildren != null)
		{
			componentInChildren.RemoveBeacon();
		}
	}
}
