using UnityEngine;

public class SanctuaryLevelReady : MonoBehaviour
{
	public void OnLevelReady()
	{
		SanctuaryManager.pLevelReady = true;
	}
}
