using UnityEngine;

public class ObTurnOffPools : MonoBehaviour
{
	public PoolInfo[] _TurnOffPoolInfo;

	public virtual void Start()
	{
		SnChannel.AddToTurnOffPools(_TurnOffPoolInfo);
	}
}
