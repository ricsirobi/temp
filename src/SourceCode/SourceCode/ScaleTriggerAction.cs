using UnityEngine;

public class ScaleTriggerAction : MonoBehaviour
{
	public Vector3 _StartScale = Vector3.one;

	public Vector3 _EndScale = Vector3.one;

	public float _MaxTime = 1f;

	private float mElapsedTime;

	private void Update()
	{
		mElapsedTime += Time.deltaTime;
		base.transform.localScale = Vector3.Lerp(_StartScale, _EndScale, Mathf.Clamp(mElapsedTime / _MaxTime, 0f, 1f));
		base.enabled = !(mElapsedTime / _MaxTime >= 1f);
	}
}
