using UnityEngine;

public class ObStatus : MonoBehaviour, ObIStatus
{
	private bool mReady;

	public bool pIsReady
	{
		get
		{
			return mReady;
		}
		set
		{
			mReady = value;
		}
	}
}
