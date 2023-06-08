using UnityEngine;

public class UWSwimmingClickable : ObClickable
{
	public UWSwimmingZone _UWSwimZone;

	public GameObject _CutScene;

	public Transform _AvatarMarker;

	public UWBreathZoneCSM _BreathZone;

	private CoAnimController mCurrentCutscene;

	public override void OnActivate()
	{
		base.OnActivate();
		if (AvAvatar.pObject.GetComponent<AvAvatarController>().pSubState == AvAvatarSubState.UWSWIMMING)
		{
			_UWSwimZone.SetAvatarMarker(_AvatarMarker);
			_UWSwimZone.Exit();
		}
		else if (_CutScene != null)
		{
			ShowCutScene();
		}
		else
		{
			OnCutSceneDone();
		}
	}

	private void ShowCutScene()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pAvatarCam.SetActive(value: false);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.enabled = false;
		}
		GameObject gameObject = Object.Instantiate(_CutScene);
		if (gameObject != null)
		{
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			mCurrentCutscene = gameObject.GetComponentInChildren<CoAnimController>();
			mCurrentCutscene._MessageObject = base.gameObject;
		}
	}

	private void OnCutSceneDone()
	{
		if (mCurrentCutscene != null)
		{
			Object.Destroy(mCurrentCutscene.gameObject);
			mCurrentCutscene = null;
		}
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pAvatarCam.SetActive(value: true);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.enabled = true;
		}
		component.pUWSwimZone = _UWSwimZone;
		_UWSwimZone.SetAvatarMarker(_AvatarMarker);
		AvAvatar.pSubState = AvAvatarSubState.UWSWIMMING;
		if (_BreathZone != null)
		{
			_UWSwimZone.pLastUsedBreathZone = _BreathZone;
		}
	}
}
