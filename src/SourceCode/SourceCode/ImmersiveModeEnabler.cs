using UnityEngine;

public class ImmersiveModeEnabler : MonoBehaviour
{
	private void Awake()
	{
		if (!Application.isEditor)
		{
			HideNavigationBar();
		}
	}

	private void HideNavigationBar()
	{
	}

	private void OnApplicationPause(bool pausedState)
	{
	}
}
