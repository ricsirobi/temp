using UnityEngine;

public class EnvironmentalEffects : MonoBehaviour
{
	public GrFog _Fog;

	public GameObject _Camera;

	public GameObject _CameraParticle;

	public Vector3 _CameraParticleOffset = new Vector3(0f, 2.5f, -2.5f);

	private GameObject mCurrentCamera;

	private GameObject mPreviousCamera;

	private GameObject mCurrentCameraParticle;

	private AvAvatarController mAvatarController;

	private void Start()
	{
		mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
	}

	public void EnableEffects(bool enable)
	{
		SetCamera(enable);
		SetCameraParticle(enable);
		SetFog(enable);
	}

	private void SetCamera(bool enable)
	{
		if (enable)
		{
			if (_Camera != null && !UtPlatform.IsMobile())
			{
				mPreviousCamera = AvAvatar.pAvatarCam;
				mPreviousCamera.SetActive(value: false);
				mCurrentCamera = Object.Instantiate(_Camera);
				mCurrentCamera.gameObject.name = AvAvatar.pAvatarCam.name;
				CaAvatarCam component = mCurrentCamera.GetComponent<CaAvatarCam>();
				if (component != null)
				{
					component.SetAvatarCamParams(mAvatarController.pCurrentStateData._AvatarCameraParams);
				}
				AvAvatar.pAvatarCam = mCurrentCamera;
				mCurrentCamera.SetActive(value: true);
			}
			else
			{
				mCurrentCamera = AvAvatar.pAvatarCam;
			}
		}
		else if (mPreviousCamera != null)
		{
			Object.Destroy(mCurrentCamera);
			mCurrentCamera = null;
			AvAvatar.pAvatarCam = mPreviousCamera;
			AvAvatar.pAvatarCam.SetActive(value: true);
			mPreviousCamera = null;
		}
	}

	private void SetCameraParticle(bool enable)
	{
		if (enable)
		{
			if (mCurrentCameraParticle == null && _CameraParticle != null)
			{
				mCurrentCameraParticle = Object.Instantiate(_CameraParticle, mCurrentCamera.transform.position, Quaternion.identity);
				mCurrentCameraParticle.transform.SetParent(mCurrentCamera.transform);
				mCurrentCameraParticle.transform.localPosition += _CameraParticleOffset;
				mCurrentCameraParticle.SetActive(value: true);
			}
		}
		else if (mCurrentCameraParticle != null)
		{
			Object.Destroy(mCurrentCameraParticle);
			mCurrentCameraParticle = null;
		}
	}

	private void SetFog(bool enable)
	{
		if (enable)
		{
			_Fog.Activate();
		}
		else
		{
			_Fog.Restore();
		}
	}
}
