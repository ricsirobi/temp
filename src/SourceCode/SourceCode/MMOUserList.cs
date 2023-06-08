using System;
using System.Collections.Generic;
using UnityEngine;

public class MMOUserList : MonoBehaviour
{
	public int _WindowID = 1100;

	public Rect _Rect = new Rect(0f, 0f, 350f, 350f);

	public bool _Visible = true;

	private string mCommand = "";

	private string mSelected = "";

	private KeyCode mKeyCode;

	private Vector2 mScrollPos = new Vector2(0f, 0f);

	private bool mShowEnd = true;

	public static bool _Sizing = false;

	public static int _LineSpaceInPixels = 20;

	private static GameObject mInstance = null;

	public static void Show()
	{
		Show(Vector2.zero);
	}

	public static void Show(Vector2 inPosition)
	{
		if (!(MainStreetMMOClient.pInstance == null))
		{
			MMOUserList mMOUserList = null;
			if (mInstance == null)
			{
				mInstance = new GameObject();
				mInstance.name = "MMOUserList";
				mMOUserList = (MMOUserList)mInstance.AddComponent(typeof(MMOUserList));
			}
			else
			{
				mMOUserList = (MMOUserList)mInstance.GetComponent(typeof(MMOUserList));
			}
			if (mMOUserList != null)
			{
				mMOUserList._Visible = true;
				mMOUserList._Rect.x = inPosition.x;
				mMOUserList._Rect.y = inPosition.y;
			}
		}
	}

	public static void Hide()
	{
		if (mInstance != null)
		{
			MMOUserList mMOUserList = (MMOUserList)mInstance.GetComponent(typeof(MMOUserList));
			if (mMOUserList != null)
			{
				mMOUserList._Visible = false;
			}
		}
	}

	private void Awake()
	{
		mInstance = base.gameObject;
		UnityEngine.Object.DontDestroyOnLoad(mInstance);
	}

	private void Update()
	{
		if (_Sizing)
		{
			int num = (int)Input.mousePosition.x;
			int num2 = Screen.height - (int)Input.mousePosition.y;
			_Rect.width = (float)num - _Rect.x;
			_Rect.height = (float)num2 - _Rect.y;
			if (Input.GetMouseButtonDown(0))
			{
				_Sizing = false;
			}
		}
		if (!_Visible || mKeyCode != KeyCode.Return || mCommand.Length == 0)
		{
			return;
		}
		if ("watch".StartsWith(mCommand, StringComparison.OrdinalIgnoreCase))
		{
			Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
			if (pPlayerList.ContainsKey(mSelected))
			{
				MMOAvatar mMOAvatar = pPlayerList[mSelected];
				if (mMOAvatar != null)
				{
					((CaAvatarCam)AvAvatar.pAvatarCam.GetComponent("CaAvatarCam")).SetLookAt(mMOAvatar.pObject.transform, null, 0f);
				}
			}
		}
		else if ("stats".StartsWith(mCommand, StringComparison.OrdinalIgnoreCase))
		{
			MMOUserStats.mKey = mSelected;
			MMOUserStats.Show();
		}
		else if ("reset".StartsWith(mCommand, StringComparison.OrdinalIgnoreCase))
		{
			((CaAvatarCam)AvAvatar.pAvatarCam.GetComponent("CaAvatarCam")).SetLookAt(AvAvatar.mTransform, null, 0f);
		}
		mCommand = "";
	}

	private void OnGUI()
	{
		GUI.skin = null;
		if (_Visible)
		{
			_Rect = GUI.Window(_WindowID, _Rect, OnWindowGUI, "MMO Player List");
			if (Event.current != null && Event.current.type == EventType.KeyUp)
			{
				mKeyCode = Event.current.keyCode;
			}
		}
	}

	private void OnWindowGUI(int inWindowID)
	{
		int num = 5;
		int num2 = 20;
		int num3 = 0;
		Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
		if (GUI.Button(new Rect(_Rect.width - 20f, 0f, 20f, 16f), "X"))
		{
			_Visible = false;
		}
		if (GUI.Button(new Rect(_Rect.width - 20f, (int)_Rect.height - _LineSpaceInPixels, 20f, 20f), "#"))
		{
			_Sizing = true;
		}
		mCommand = GUI.TextField(new Rect(num, (int)_Rect.height - _LineSpaceInPixels, _Rect.width - (float)num - 30f, 20f), mCommand);
		float num4 = _LineSpaceInPixels * pPlayerList.Count;
		float num5 = num4 - (float)((int)_Rect.height - _LineSpaceInPixels - num2);
		if (mShowEnd)
		{
			mScrollPos.y = num5;
		}
		GUI.changed = false;
		float y = mScrollPos.y;
		mScrollPos = GUI.BeginScrollView(new Rect(0f, num2, _Rect.width - (float)num, (int)_Rect.height - _LineSpaceInPixels - num2), mScrollPos, new Rect(0f, 0f, _Rect.width - 40f, num4));
		if (GUI.changed)
		{
			if (mScrollPos.y >= y)
			{
				mShowEnd = mScrollPos.y >= num5 - 10f;
			}
			else
			{
				mShowEnd = false;
			}
		}
		GUIStyle label = GUI.skin.label;
		label.wordWrap = false;
		foreach (KeyValuePair<string, MMOAvatar> item in pPlayerList)
		{
			MMOAvatar value = item.Value;
			if (value != null)
			{
				if (item.Key == mSelected)
				{
					label.normal.textColor = Color.yellow;
				}
				if (GUI.Button(new Rect(num, num3, _Rect.width - (float)num, 20f), ((value.pAvatarData.mInstance != null) ? value.pAvatarData.mInstance.DisplayName : "") + ":" + item.Key, label))
				{
					mSelected = item.Key;
				}
				num3 += _LineSpaceInPixels;
				label.normal.textColor = Color.white;
			}
		}
		GUI.EndScrollView();
		GUI.DragWindow();
	}
}
