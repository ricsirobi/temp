using UnityEngine;

public class ObFaceAvatar : KAMonoBase
{
	public bool _IgnoreY = true;

	private void LateUpdate()
	{
		Vector3 worldPosition = new Vector3(0f, 0f, 0f);
		if (AvAvatar.pObject != null)
		{
			worldPosition = AvAvatar.position;
			if (_IgnoreY)
			{
				worldPosition.y = base.transform.position.y;
			}
		}
		base.transform.LookAt(worldPosition);
	}
}
