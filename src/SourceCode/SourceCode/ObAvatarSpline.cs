using UnityEngine;

public class ObAvatarSpline : MonoBehaviour
{
	public float _Speed;

	public float _Distance = 2.5f;

	public GameObject _Spline;

	private void Update()
	{
		if (AvAvatar.pObject != null && (AvAvatar.position - base.transform.position).magnitude <= _Distance && AvAvatar.pInputEnabled && AvAvatar.pToolbar.activeInHierarchy)
		{
			AvAvatar.pInputEnabled = false;
			AvAvatar.SetUIActive(inActive: false);
			if ((bool)AvAvatar.pAvatarCam)
			{
				((CaAvatarCam)AvAvatar.pAvatarCam.GetComponent("CaAvatarCam"))._IgnoreCollision = true;
			}
			SplineControl splineControl = (SplineControl)_Spline.GetComponent("SplineControl");
			splineControl.mSpline.GetPosQuatByDist(0f, out var pos, out var quat);
			AvAvatar.SetPosition(pos);
			AvAvatar.mTransform.rotation = quat;
			AvAvatarController avAvatarController = (AvAvatarController)AvAvatar.pObject.GetComponent("AvAvatarController");
			avAvatarController.SetSpline(splineControl.mSpline);
			if (_Speed > 0f)
			{
				avAvatarController.Speed = _Speed;
			}
			else
			{
				avAvatarController.Speed = avAvatarController.pCurrentStateData._MaxForwardSpeed;
			}
		}
	}
}
