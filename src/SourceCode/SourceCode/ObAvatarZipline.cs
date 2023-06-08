using UnityEngine;

public class ObAvatarZipline : MonoBehaviour
{
	public GameObject _ZipHandle;

	public float _Distance = 1.8f;

	public GameObject _Spline;

	public float _Speed = 6f;

	public bool _Draw;

	public bool _NonMemberOnly;

	public bool _MemberOnly;

	public AudioClip _NonMemberVO;

	public AudioClip _NonRidezVO;

	public bool _NoPlayPlayed;

	private void Update()
	{
		if (!(AvAvatar.pObject != null))
		{
			return;
		}
		if ((AvAvatar.position - base.transform.position).magnitude <= _Distance && AvAvatar.pState != AvAvatarState.ZIPLINE)
		{
			if (_NoPlayPlayed)
			{
				return;
			}
			if (_MemberOnly && !SubscriptionInfo.pIsMember)
			{
				_NoPlayPlayed = true;
				SnChannel.Play(_NonMemberVO, "VO_Pool", inForce: false);
				return;
			}
			if (AvAvatar.IsPlayerOnRide())
			{
				_NoPlayPlayed = true;
				SnChannel.Play(_NonRidezVO, "VO_Pool", inForce: false);
				return;
			}
			if (_NonMemberOnly)
			{
				if (SubscriptionInfo.pIsMember)
				{
					return;
				}
				SnChannel.Play(_NonMemberVO, "VO_Pool", inForce: false);
			}
			Use(AvAvatar.pObject);
			AvAvatar.pInputEnabled = false;
			AvAvatar.SetUIActive(inActive: false);
			if ((bool)AvAvatar.pAvatarCam)
			{
				AvAvatar.pAvatarCam.SendMessage("ResetSpeed", null, SendMessageOptions.DontRequireReceiver);
				((CaAvatarCam)AvAvatar.pAvatarCam.GetComponent("CaAvatarCam"))._IgnoreCollision = true;
			}
			if (MainStreetMMOClient.pInstance != null)
			{
				MainStreetMMOClient.pInstance.SendZipline(base.gameObject.name);
			}
		}
		else
		{
			_NoPlayPlayed = false;
		}
	}

	public void Use(GameObject avatar)
	{
		SplineControl splineControl = (SplineControl)_Spline.GetComponent("SplineControl");
		splineControl.mSpline.GetPosQuatByDist(0f, out var pos, out var quat);
		AvAvatarController avAvatarController = (AvAvatarController)avatar.GetComponent("AvAvatarController");
		avAvatarController.pState = AvAvatarState.ZIPLINE;
		if ((bool)_ZipHandle)
		{
			GameObject gameObject = Object.Instantiate(_ZipHandle);
			avatar.transform.parent = gameObject.transform;
			avatar.transform.localPosition = new Vector3(0f, -1.988f, 0f);
			avatar.transform.localRotation = Quaternion.identity;
			ZiplineController ziplineController = (ZiplineController)gameObject.GetComponent("ZiplineController");
			ziplineController.SetSpline(splineControl.mSpline);
			ziplineController.Speed = _Speed;
			if (AvAvatar.IsCurrentPlayer(avatar))
			{
				ziplineController.StartMusic();
			}
			ziplineController.pAvatar = avatar;
		}
		else
		{
			avatar.transform.position = pos;
			avatar.transform.rotation = quat;
			avAvatarController.SetSpline(splineControl.mSpline);
			avAvatarController.Speed = _Speed;
		}
		if (avAvatarController.pPetObject != null)
		{
			avAvatarController.pPetObject.OnAvatarZiplineStateStarted();
		}
	}

	private void OnDrawGizmos()
	{
		if (_Draw)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.position, _Distance);
		}
	}
}
