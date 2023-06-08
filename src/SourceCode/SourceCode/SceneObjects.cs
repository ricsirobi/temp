using System.Collections.Generic;
using UnityEngine;

public class SceneObjects : MonoBehaviour
{
	public bool _AutoPopulate;

	public bool _Visible = true;

	public bool _Delete = true;

	public Rect _Position = new Rect(10f, 20f, 450f, 64f);

	public Rect _LayoutPosition = new Rect(10f, 10f, 450f, 300f);

	private Rect mPosition = new Rect(10f, 10f, 450f, 32f);

	public Vector2 mScrollPosition = Vector2.zero;

	private Rect mViewRect = new Rect(10f, 10f, 256f, 32f);

	public GameObject[] _Objects;

	private void Start()
	{
		_LayoutPosition.x = (float)(Screen.width / 2) - _LayoutPosition.width / 2f;
		_LayoutPosition.y = _Position.y + _Position.height + 10f;
		_LayoutPosition.height = Screen.height - 100;
	}

	private void Update()
	{
		if (!_AutoPopulate)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[];
		Debug.Log("Total Objects : " + array.Length);
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (!(gameObject.transform.root == base.transform) && !gameObject.name.Contains("@") && !gameObject.transform.root.gameObject.name.Contains("@") && !list.Contains(gameObject.transform.root.gameObject))
			{
				list.Add(gameObject.transform.root.gameObject);
			}
		}
		list.Sort(SortByName);
		_Objects = list.ToArray();
		_AutoPopulate = false;
		mViewRect.height = (float)(_Objects.Length + 20) * _Position.height;
	}

	private static int SortByName(GameObject go1, GameObject go2)
	{
		return go1.name.CompareTo(go2.name);
	}

	private void OnGUI()
	{
		mPosition = _Position;
		int num = 0;
		if (_Objects != null)
		{
			num = _Objects.Length;
		}
		if (GUI.Button(mPosition, "Refresh-" + num))
		{
			_AutoPopulate = true;
		}
		mPosition.y += _Position.height + 10f;
		if (GUI.Button(mPosition, "Visible"))
		{
			_Visible = !_Visible;
			UtMobileUtilities.DisableAllScripts(_Visible);
		}
		mPosition.y += _Position.height + 10f;
		if (GUI.Button(mPosition, "Delete"))
		{
			if (KAInput.touchCount >= 2 && AvAvatar.pAvatarCam != null)
			{
				AvAvatar.pAvatarCam.GetComponent<Camera>().enabled = !AvAvatar.pAvatarCam.GetComponent<Camera>().enabled;
			}
			else
			{
				_Delete = !_Delete;
			}
		}
		mPosition.y += _Position.height + 10f;
		if (_Objects == null || !_Visible)
		{
			return;
		}
		mScrollPosition = GUI.BeginScrollView(_LayoutPosition, mScrollPosition, mViewRect);
		num = 0;
		GameObject[] objects = _Objects;
		foreach (GameObject gameObject in objects)
		{
			if (!(gameObject == null))
			{
				num++;
				string text = num + "> " + gameObject.name;
				if (GUI.Button(text: (!gameObject.activeSelf) ? (text + "--Enable") : (text + "--Disable"), position: mPosition))
				{
					gameObject.SetActive(!gameObject.activeSelf);
				}
				mPosition.y += mPosition.height + 2f;
			}
		}
		GUI.EndScrollView();
	}
}
