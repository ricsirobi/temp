public class ObFaceCameraPlane : KAMonoBase
{
	private void LateUpdate()
	{
		if (!(AvAvatar.pAvatarCam == null))
		{
			base.transform.rotation = AvAvatar.pAvatarCamTransform.rotation;
		}
	}
}
