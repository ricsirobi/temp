using UnityEngine;

public class CTRemoteKnob : MonoBehaviour
{
	private CTRocketController mRemoteObj;

	private void Start()
	{
		mRemoteObj = Object.FindObjectOfType<CTRocketController>();
	}

	public void OnCollisionEnter2D(Collision2D other)
	{
		if (!other.gameObject.name.Contains("Controller"))
		{
			mRemoteObj.SendSignal();
		}
	}
}
