using System.Collections;
using UnityEngine;

public class DelayActivation : MonoBehaviour
{
	public void ActivateGameObject(GameObject obj, GameObjectExtensions.ActivationDone callback, bool isActive, float inSeconds)
	{
		StartCoroutine(ActivateCoroutineWait(obj, callback, isActive, inSeconds));
	}

	private IEnumerator ActivateCoroutineWait(GameObject obj, GameObjectExtensions.ActivationDone callback, bool inActive, float inSeconds)
	{
		if (inSeconds == 0f)
		{
			yield return new WaitForEndOfFrame();
		}
		else
		{
			yield return new WaitForSeconds(inSeconds);
		}
		obj.SetActive(inActive);
		callback?.Invoke(inActive);
		Object.Destroy(base.gameObject);
	}
}
