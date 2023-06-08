using System;
using UnityEngine;

[Serializable]
public class GSCameraOffsetData
{
	[Serializable]
	public class AvatarCameraOffset
	{
		public int _PetTypeID;

		public Vector3 _CameraOffset;
	}

	public AvatarCameraOffset[] _AvatarCameraOffset;

	public GSGameType _GameType;
}
