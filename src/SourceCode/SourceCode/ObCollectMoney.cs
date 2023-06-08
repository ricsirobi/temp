using UnityEngine;

public class ObCollectMoney : MonoBehaviour
{
	public int _RewardAmount = 1;

	public void Collect()
	{
		Money.AddMoney(_RewardAmount, bForceUpdate: false);
		Object.Destroy(base.gameObject);
	}
}
