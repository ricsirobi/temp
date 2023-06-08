using UnityEngine;

public class ObFaceAvatarCamera : KAMonoBase
{
	public bool _IgnoreY = true;

	private void LateUpdate()
	{
		if (!(AvAvatar.pAvatarCamTransform == null))
		{
			Vector3 avatarCamPosition = AvAvatar.AvatarCamPosition;
			Vector3 position = base.transform.position;
			if (_IgnoreY)
			{
				avatarCamPosition.y = position.y;
			}
			base.transform.rotation = Quaternion.LookRotation(avatarCamPosition - position);
		}
	}
}
