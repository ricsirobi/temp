using UnityEngine;

public static class GameObjectExtensions
{
	public delegate void ActivationDone(bool isActive);

	public static GameObject ChangeActiveStateAfterDelay(this GameObject obj, ActivationDone callback, bool inActive, float inSeconds, GameObject prevDelayObject = null)
	{
		if (prevDelayObject != null)
		{
			Object.Destroy(prevDelayObject);
		}
		GameObject gameObject = new GameObject("Temp");
		gameObject.AddComponent<DelayActivation>().ActivateGameObject(obj, callback, inActive, inSeconds);
		return gameObject;
	}
}
