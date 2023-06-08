using UnityEngine;

public class AiTrigger : MonoBehaviour
{
	public enum TriggerType
	{
		ENTRY,
		EXIT
	}

	public TriggerType _Type;

	public float _MinDrag = 0.4f;

	public float _MaxDrag = 0.5f;

	public float _PathWidth;

	public float _BrakeForce = 15f;

	public bool _ForcePathWidthToZero = true;
}
