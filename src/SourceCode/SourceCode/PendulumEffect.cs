using UnityEngine;

public class PendulumEffect : MonoBehaviour
{
	public MinMax _RotSpeedRange;

	public MinMax _AngleRange;

	private float mTimer;

	private void Update()
	{
		mTimer += Time.deltaTime * Random.Range(_RotSpeedRange.Min, _RotSpeedRange.Max);
		base.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(mTimer) * Random.Range(_AngleRange.Min, _AngleRange.Max));
	}
}
