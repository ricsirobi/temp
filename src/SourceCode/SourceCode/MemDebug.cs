using System.Collections.Generic;
using UnityEngine;

public class MemDebug : MonoBehaviour
{
	public float _Offset = -40f;

	public GameObject _ObjectsGroup;

	public GameObject _SpawnPoint;

	public Rect _Rect = new Rect(100f, 50f, 256f, 32f);

	public Light _DirectionalLight;

	public int _MarkerIndex = 1;

	public GameObject[] _Objects;

	public List<GameObject> mObjects = new List<GameObject>();

	public int _Count = 13;

	private void Awake()
	{
	}

	private void Start()
	{
		_Rect.height = 32f;
		_Rect.x = (float)Screen.width - 165f;
		_Rect.width = 160f;
		mObjects.AddRange(_Objects);
		mObjects.Add(AvAvatar.pObject);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.mTransform.rotation = Quaternion.Euler(0f, 180f, 0f);
		RsResourceManager.DestroyLoadScreen();
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 1f, 100f, 32f), "Shadow-" + _DirectionalLight.shadows))
		{
			if (_DirectionalLight.shadows == LightShadows.None)
			{
				_DirectionalLight.shadows = LightShadows.Soft;
			}
			else if (_DirectionalLight.shadows == LightShadows.Soft)
			{
				_DirectionalLight.shadows = LightShadows.Hard;
			}
			else if (_DirectionalLight.shadows == LightShadows.Hard)
			{
				_DirectionalLight.shadows = LightShadows.None;
			}
		}
		if (GUI.Button(new Rect(0f, 64f, 100f, 64f), "Create - " + _Count))
		{
			GameObject obj = Object.Instantiate(_ObjectsGroup);
			obj.transform.position = _ObjectsGroup.transform.position + _ObjectsGroup.transform.forward * _Offset * _MarkerIndex;
			obj.transform.rotation = _ObjectsGroup.transform.rotation;
			_MarkerIndex++;
			_Count += 13;
		}
	}
}
