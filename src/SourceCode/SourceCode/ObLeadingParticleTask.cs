using UnityEngine;

public class ObLeadingParticleTask : MonoBehaviour
{
	public string _Name;

	public LeadingParticleData _Data;

	private void Start()
	{
		LeadingParticleManager.Start(_Name, AvAvatar.pObject.transform, _Data);
	}

	private void OnDestroy()
	{
		LeadingParticleManager.Stop(_Name);
	}
}
