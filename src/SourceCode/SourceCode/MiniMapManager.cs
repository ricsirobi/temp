using UnityEngine;

public class MiniMapManager : MonoBehaviour
{
	public GameObject _PlayerDisplay;

	public GameObject _MMOPlayerDisplay;

	public MiniMapPath[] _RacingPaths;

	public KAUICameraToWidget _MiniMapCameraToWidget;

	private void Start()
	{
		for (int i = 0; i < _RacingPaths.Length; i++)
		{
			_RacingPaths[i].InitializePath();
			_RacingPaths[i].DrawMiniMapPath();
		}
		if (_MiniMapCameraToWidget != null)
		{
			_MiniMapCameraToWidget.SetCameraRect();
		}
	}

	public void AddDisplayObject(GameObject inObject, AvatarRacing inAvatarRacing, bool isUser)
	{
		GameObject gameObject = null;
		if (isUser && _PlayerDisplay != null)
		{
			gameObject = Object.Instantiate(_PlayerDisplay, inObject.transform.position, Quaternion.identity);
		}
		else if (_MMOPlayerDisplay != null)
		{
			gameObject = Object.Instantiate(_MMOPlayerDisplay, inObject.transform.position, Quaternion.identity);
		}
		if (gameObject != null)
		{
			gameObject.transform.SetParent(inObject.transform);
			inAvatarRacing.pMiniMapDisplay = gameObject;
		}
	}

	public void EnableMiniMapCamera(bool active)
	{
		if (!(_MiniMapCameraToWidget._Camera == null))
		{
			_MiniMapCameraToWidget._Camera.enabled = active;
			_MiniMapCameraToWidget.enabled = active;
		}
	}
}
