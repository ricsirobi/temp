using UnityEngine;

public class MMOActivation : MonoBehaviour
{
	public bool _OnStart = true;

	public bool _Activate;

	private void Start()
	{
		if (_OnStart)
		{
			Activate(_Activate);
		}
	}

	public void Activate(bool active)
	{
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active);
		}
	}

	private void OnDestroy()
	{
		Activate(!_Activate);
	}
}
