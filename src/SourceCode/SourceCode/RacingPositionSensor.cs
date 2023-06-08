using UnityEngine;

public class RacingPositionSensor : MonoBehaviour
{
	public RacingPositionSensor _LastSensor;

	public RacingPositionSensor _NextSensor;

	private float mDistanceBetweenLastNextSensor;

	private int mIndex = -1;

	public float pDistanceBetweenLastNextSensor => mDistanceBetweenLastNextSensor;

	public int pIndex => mIndex;

	public void Initialize(int index)
	{
		mIndex = index;
		mDistanceBetweenLastNextSensor = Vector3.Distance(_LastSensor.transform.position, _NextSensor.transform.position);
	}
}
