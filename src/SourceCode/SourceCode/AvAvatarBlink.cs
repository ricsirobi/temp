using UnityEngine;

public class AvAvatarBlink : AvatarBlink
{
	protected override void Awake()
	{
		Transform transform = null;
		Transform transform2 = null;
		if (AvatarData.pGeneralSettings == null)
		{
			UtDebug.LogError("[--DevLog AvatarBlink] GeneralSettings is null");
		}
		if (AvatarData.GetGender() == Gender.Male)
		{
			transform = base.transform.Find(AvatarData.pGeneralSettings.BLINK_PLANE_MALE);
			transform2 = base.transform.Find(AvatarData.pGeneralSettings.BLINK_PLANE_FEMALE);
		}
		else
		{
			transform = base.transform.Find(AvatarData.pGeneralSettings.BLINK_PLANE_FEMALE);
			transform2 = base.transform.Find(AvatarData.pGeneralSettings.BLINK_PLANE_MALE);
		}
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(value: false);
		}
		if (transform != null)
		{
			mMeshRenderer = transform.GetComponent<MeshRenderer>();
		}
	}
}
