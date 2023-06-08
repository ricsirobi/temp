using UnityEngine;

public class ObParticleHandler : MonoBehaviour
{
	public enum ImportanceLevel
	{
		HIGHT,
		MEDIUM,
		LOW
	}

	public ImportanceLevel _ImportanceLevel = ImportanceLevel.MEDIUM;

	public float _MaxViewDistance = -1f;

	public float _Timer = 5f;

	public bool _CheckAvatarDistance;

	private float mMaxEmissionRate;
}
