using UnityEngine;

public class AttachToPlayersDragon : MonoBehaviour
{
	public string _Bone;

	private void Start()
	{
		if (SanctuaryManager.pCurPetInstance != null && !string.IsNullOrEmpty(_Bone))
		{
			Transform transform = UtUtilities.FindChildTransform(SanctuaryManager.pCurPetInstance.gameObject, _Bone);
			if (transform != null)
			{
				base.transform.parent = transform;
				base.transform.localPosition = Vector3.zero;
				base.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
