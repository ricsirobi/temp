using UnityEngine;

public class UiFocusCamera : KAUI
{
	public GameObject _Camera;

	public string _MissionActionText = "UseTablet";

	private float mOldCameraSize;

	private Vector3 mDefaultEulerAngles;

	private Quaternion mInitialOrientation = Quaternion.identity;

	private void OnEnable()
	{
		StartFocus();
	}

	private void OnDisable()
	{
		EndFocus();
	}

	public void StartFocus()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.pAvatarCam.SetActive(value: false);
		_Camera.SetActive(value: true);
		mOldCameraSize = _Camera.GetComponent<Camera>().orthographicSize;
		mInitialOrientation = _Camera.transform.rotation;
	}

	public void EndFocus()
	{
		AvAvatar.pAvatarCam.SetActive(value: true);
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		_Camera.SetActive(value: false);
		_Camera.GetComponent<Camera>().orthographicSize = mOldCameraSize;
		_Camera.transform.rotation = mInitialOrientation;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnBack")
		{
			ProcessCloseUI();
		}
	}

	public void ProcessCloseUI()
	{
		base.gameObject.SetActive(value: false);
		if (!string.IsNullOrEmpty(_MissionActionText) && MissionManager.pInstance != null && MissionManager.pIsReady)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _MissionActionText);
		}
	}
}
