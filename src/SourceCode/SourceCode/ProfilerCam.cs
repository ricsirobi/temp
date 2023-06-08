using UnityEngine;

public class ProfilerCam : MonoBehaviour
{
	private static ProfilerCam mInstance;

	public Transform _ProfilerfMarkersObject;

	public Transform _ProfilerCameraTransform;

	private Transform[] mAvatarProfilerMarkers;

	private Transform[] mCameraProfilerMarkers;

	public static ProfilerCam pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
		Transform inParentTransform = _ProfilerfMarkersObject.Find("_AvatarMarkers");
		mAvatarProfilerMarkers = GetMarkers(inParentTransform);
		Transform inParentTransform2 = _ProfilerfMarkersObject.Find("_CameraMarkers");
		mCameraProfilerMarkers = GetMarkers(inParentTransform2);
	}

	public void MoveToPositionIndex(int index)
	{
		MoveToMarkers(mCameraProfilerMarkers[index], mAvatarProfilerMarkers[index]);
	}

	private Transform[] GetMarkers(Transform inParentTransform)
	{
		Transform[] array = new Transform[inParentTransform.childCount];
		for (int i = 0; i < inParentTransform.childCount; i++)
		{
			array[i] = inParentTransform.GetChild(i);
		}
		return array;
	}

	public void MoveToMarkers(Transform inCameraMarker, Transform inAvatarTransform)
	{
		if (inCameraMarker != null && _ProfilerCameraTransform != null)
		{
			_ProfilerCameraTransform.position = inCameraMarker.position;
			_ProfilerCameraTransform.rotation = inCameraMarker.rotation;
		}
		if (inAvatarTransform != null)
		{
			AvAvatar.TeleportTo(inAvatarTransform.position, inAvatarTransform.forward, 0f, doTeleportFx: false);
		}
	}

	public void SetUIActive(bool inActive)
	{
		AvAvatar.SetUIActive(inActive);
	}

	public void SetAvatarActive(bool inActive)
	{
		AvAvatar.SetOnlyAvatarActive(inActive);
	}

	public void SetAvatarCameraActive(bool inActive)
	{
		AvAvatar.pAvatarCam.SetActive(inActive);
		if (_ProfilerCameraTransform != null)
		{
			_ProfilerCameraTransform.gameObject.SetActive(!inActive);
		}
	}
}
