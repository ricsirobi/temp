using System;
using UnityEngine;

public class ObInventoryTrigger : ObSwitchBase
{
	[Serializable]
	public class UnlockItem
	{
		public int _ItemID;

		public int _ItemCount;

		private int mCollected;

		public int pCollected
		{
			get
			{
				return mCollected;
			}
			set
			{
				mCollected = value;
			}
		}
	}

	public UnlockItem[] _UnlockItems;

	private void Start()
	{
		CheckItemsCollected();
	}

	private bool IsItemsCollected()
	{
		bool result = true;
		UnlockItem[] unlockItems = _UnlockItems;
		foreach (UnlockItem unlockItem in unlockItems)
		{
			if (unlockItem.pCollected < unlockItem._ItemCount)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	private void Collect(GameObject obj)
	{
		if (obj != null)
		{
			ObCollect component = obj.GetComponent<ObCollect>();
			if (component != null)
			{
				UnlockItem[] unlockItems = _UnlockItems;
				foreach (UnlockItem unlockItem in unlockItems)
				{
					if (unlockItem._ItemID == component._InventoryID)
					{
						unlockItem.pCollected += component._InventoryQuantity;
						break;
					}
				}
			}
		}
		CheckItemsCollected();
	}

	private void CheckItemsCollected()
	{
		if (!mSwitchOn)
		{
			mSwitchOn = IsItemsCollected();
			if (mSwitchOn)
			{
				SwitchOn();
			}
		}
	}
}
