using UnityEngine;

public class OptimizeMonitor : MonoBehaviour
{
	public GameObject[] _OptimizedObject;

	private bool mReduced;

	private void Update()
	{
		if (!mReduced && GrFPS._IsBelowMinimum)
		{
			mReduced = true;
			GameObject[] optimizedObject = _OptimizedObject;
			foreach (GameObject obj in optimizedObject)
			{
				obj.SetActive(value: false);
				obj.GetComponent<ParticleSystem>().Stop();
			}
		}
	}
}
