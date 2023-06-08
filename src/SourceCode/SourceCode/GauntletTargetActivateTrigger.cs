using UnityEngine;

public class GauntletTargetActivateTrigger : MonoBehaviour
{
	public GameObject[] _ActivateTargets;

	public float _ActivateDelay;

	private float mActivateTimer;

	private bool mIsActivating;

	private bool mMathContentLoaded;

	public void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.GetComponent<GauntletController>() != null)
		{
			GauntletRailShootManager.pInstance._CurrentActiveTargets.Clear();
			mActivateTimer = _ActivateDelay;
			if ((bool)GauntletRailShootManager.pInstance && GauntletRailShootManager.pInstance.pIsMathContent && _ActivateTargets.Length != 0 && !mMathContentLoaded)
			{
				GauntletRailShootManager.pInstance.ReadTargets(_ActivateTargets);
				mMathContentLoaded = true;
			}
			mIsActivating = true;
		}
	}

	public void Update()
	{
		if (!mIsActivating || !(mActivateTimer >= 0f))
		{
			return;
		}
		mActivateTimer -= Time.deltaTime;
		if (!(mActivateTimer < 0f))
		{
			return;
		}
		mIsActivating = false;
		GameObject[] activateTargets = _ActivateTargets;
		foreach (GameObject gameObject in activateTargets)
		{
			if (gameObject == null)
			{
				UtDebug.LogError("Missing Reference in " + base.gameObject.name);
				continue;
			}
			GauntletRailShootManager.pInstance._CurrentActiveTargets.Add(gameObject);
			gameObject.BroadcastMessage("ActivateTarget", true, SendMessageOptions.DontRequireReceiver);
		}
	}
}
