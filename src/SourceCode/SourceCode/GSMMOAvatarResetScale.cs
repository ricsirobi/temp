using System;
using UnityEngine;

public class GSMMOAvatarResetScale : MonoBehaviour
{
	[NonSerialized]
	public AvatarData.InstanceInfo _InstanceInfo;

	public void Update()
	{
		if (_InstanceInfo != null && _InstanceInfo.pIsReady)
		{
			AvatarData.RemovePartScale(_InstanceInfo);
			UnityEngine.Object.Destroy(this);
		}
	}
}
