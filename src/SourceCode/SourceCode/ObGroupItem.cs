using UnityEngine;

public class ObGroupItem : MonoBehaviour
{
	public bool _Enable = true;

	private bool mRewardMoney = true;

	private int mRewardAmount = 5;

	private int mRewardItemID = -1;

	private bool mFound;

	public bool pEnable => _Enable;

	public int pRewardAmount
	{
		get
		{
			return mRewardAmount;
		}
		set
		{
			mRewardAmount = value;
		}
	}

	public int pRewardItemID
	{
		get
		{
			return mRewardItemID;
		}
		set
		{
			mRewardItemID = value;
			mRewardMoney = false;
		}
	}

	public bool pFound
	{
		get
		{
			return mFound;
		}
		set
		{
			mFound = value;
		}
	}

	public virtual void OnEnable()
	{
		if (mFound)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public virtual void OnActivate()
	{
		Collect();
	}

	public virtual void Collect()
	{
		if (!mFound)
		{
			mFound = true;
			SendMessageUpwards("ItemFound", base.gameObject, SendMessageOptions.DontRequireReceiver);
			if (mRewardMoney)
			{
				Money.AddMoney(mRewardAmount, bForceUpdate: true);
			}
			else
			{
				CommonInventoryData.pInstance.AddItem(mRewardItemID, updateServer: true);
			}
		}
	}

	public virtual void OnSetChestFoundDone()
	{
	}
}
