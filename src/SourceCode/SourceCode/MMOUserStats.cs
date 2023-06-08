using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MMOUserStats : MonoBehaviour
{
	public int _WindowID = 1101;

	public Rect _Rect = new Rect(350f, 100f, 350f, 300f);

	public bool _Visible = true;

	private Vector2 mScrollPos = new Vector2(0f, 0f);

	private bool mShowEnd = true;

	public static bool _Sizing = false;

	public static int _LineSpaceInPixels = 20;

	public static string mKey;

	private static GameObject mInstance = null;

	public static void Show()
	{
		if (!(MainStreetMMOClient.pInstance == null))
		{
			MMOUserStats mMOUserStats = null;
			if (mInstance == null)
			{
				mInstance = new GameObject();
				mInstance.name = "MMOUserStats";
				mMOUserStats = mInstance.AddComponent<MMOUserStats>();
			}
			else
			{
				mMOUserStats = mInstance.GetComponent<MMOUserStats>();
			}
			if (mMOUserStats != null)
			{
				mMOUserStats._Visible = true;
			}
		}
	}

	public static void Hide()
	{
		if (mInstance != null)
		{
			MMOUserStats component = mInstance.GetComponent<MMOUserStats>();
			if (component != null)
			{
				component._Visible = false;
			}
		}
	}

	private void Awake()
	{
		mInstance = base.gameObject;
		Object.DontDestroyOnLoad(mInstance);
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
	}

	private void OnGUI()
	{
		GUI.skin = null;
		if (!_Visible)
		{
			return;
		}
		string displayName = mKey;
		Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
		if (pPlayerList.ContainsKey(mKey))
		{
			MMOAvatar mMOAvatar = pPlayerList[mKey];
			if (mMOAvatar != null)
			{
				displayName = mMOAvatar.pAvatarData.mInstance.DisplayName;
			}
		}
		_Rect = GUI.Window(_WindowID, _Rect, OnWindowGUI, "MMO Stats:" + displayName);
	}

	private void OnWindowGUI(int inWindowID)
	{
		int num = 5;
		int num2 = 20;
		int num3 = 0;
		Dictionary<string, MMOAvatar> pPlayerList = MainStreetMMOClient.pInstance.pPlayerList;
		MMOAvatar mMOAvatar = null;
		if (pPlayerList.ContainsKey(mKey))
		{
			mMOAvatar = pPlayerList[mKey];
		}
		if (GUI.Button(new Rect(_Rect.width - 20f, 0f, 20f, 16f), "X"))
		{
			_Visible = false;
		}
		if (GUI.Button(new Rect(_Rect.width - 20f, _Rect.height - (float)_LineSpaceInPixels, 20f, 20f), "#"))
		{
			_Sizing = true;
		}
		float num4;
		if ((bool)mMOAvatar)
		{
			num4 = _LineSpaceInPixels * (mMOAvatar.mUserVarData.Count + 1) * 4;
			if (mMOAvatar.mKeys != null)
			{
				num4 += (float)(_LineSpaceInPixels * mMOAvatar.mKeys.Count);
			}
		}
		else
		{
			num4 = _Rect.height;
		}
		float num5 = num4 - (_Rect.height - (float)_LineSpaceInPixels - (float)num2);
		if (mShowEnd)
		{
			mScrollPos.y = num5;
		}
		GUI.changed = false;
		float y = mScrollPos.y;
		mScrollPos = GUI.BeginScrollView(new Rect(0f, num2, _Rect.width - (float)num, _Rect.height - (float)_LineSpaceInPixels - (float)num2), mScrollPos, new Rect(0f, 0f, 1000f, num4));
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
		GUI.skin.label.wordWrap = false;
		if ((bool)mMOAvatar)
		{
			if (mMOAvatar.mKeys != null && mMOAvatar.mKeys.Count > 0)
			{
				IEnumerator enumerator = mMOAvatar.mKeys.GetEnumerator();
				while (enumerator.MoveNext())
				{
					KeyValuePair<string, string> keyValuePair = (KeyValuePair<string, string>)enumerator.Current;
					GUI.Label(new Rect(num, num3, 1000f, 20f), keyValuePair.Key + ": " + keyValuePair.Value);
					num3 += _LineSpaceInPixels;
				}
			}
			float pTime = mMOAvatar.pTime;
			int i;
			for (i = 0; i < mMOAvatar.mUserVarData.Count; i++)
			{
				if ((double)pTime < mMOAvatar.mUserVarData[i]._TimeStamp)
				{
					if (i > 0)
					{
						MMOAvatarUserVarData mMOAvatarUserVarData = mMOAvatar.mUserVarData[i - 1];
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 1:");
						num3 += _LineSpaceInPixels;
						string text = mMOAvatarUserVarData._Position.x.ToString("f2") + "," + mMOAvatarUserVarData._Position.y.ToString("f2") + "," + mMOAvatarUserVarData._Position.z.ToString("f2");
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "P:" + text);
						num3 += _LineSpaceInPixels;
						Rect position = new Rect(num, num3, _Rect.width - (float)num, 20f);
						Quaternion rotation = mMOAvatarUserVarData._Rotation;
						GUI.Label(position, "R:" + rotation.ToString());
						num3 += _LineSpaceInPixels;
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "F:" + mMOAvatarUserVarData._Flags);
						num3 += _LineSpaceInPixels;
						mMOAvatarUserVarData = mMOAvatar.mUserVarData[i];
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 2:");
						num3 += _LineSpaceInPixels;
						text = mMOAvatarUserVarData._Position.x.ToString("f2") + "," + mMOAvatarUserVarData._Position.y.ToString("f2") + "," + mMOAvatarUserVarData._Position.z.ToString("f2");
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "P:" + text);
						num3 += _LineSpaceInPixels;
						Rect position2 = new Rect(num, num3, _Rect.width - (float)num, 20f);
						rotation = mMOAvatarUserVarData._Rotation;
						GUI.Label(position2, "R:" + rotation.ToString());
						num3 += _LineSpaceInPixels;
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "F:" + mMOAvatarUserVarData._Flags);
						num3 += _LineSpaceInPixels;
					}
					else
					{
						MMOAvatarUserVarData mMOAvatarUserVarData2 = mMOAvatar.mUserVarData[i];
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 1:");
						num3 += _LineSpaceInPixels;
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "(NONE)");
						num3 += _LineSpaceInPixels;
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 2:");
						num3 += _LineSpaceInPixels;
						string text2 = mMOAvatarUserVarData2._Position.x.ToString("f2") + "," + mMOAvatarUserVarData2._Position.y.ToString("f2") + "," + mMOAvatarUserVarData2._Position.z.ToString("f2");
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "P:" + text2);
						num3 += _LineSpaceInPixels;
						Rect position3 = new Rect(num, num3, _Rect.width - (float)num, 20f);
						Quaternion rotation = mMOAvatarUserVarData2._Rotation;
						GUI.Label(position3, "R:" + rotation.ToString());
						num3 += _LineSpaceInPixels;
						GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "F:" + mMOAvatarUserVarData2._Flags);
						num3 += _LineSpaceInPixels;
					}
					break;
				}
			}
			if (i == mMOAvatar.mUserVarData.Count && mMOAvatar.mUserVarData.Count > 0)
			{
				MMOAvatarUserVarData mMOAvatarUserVarData3 = mMOAvatar.mUserVarData[mMOAvatar.mUserVarData.Count - 1];
				GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 1:");
				num3 += _LineSpaceInPixels;
				string text3 = mMOAvatarUserVarData3._Position.x.ToString("f2") + "," + mMOAvatarUserVarData3._Position.y.ToString("f2") + "," + mMOAvatarUserVarData3._Position.z.ToString("f2");
				GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "P:" + text3);
				num3 += _LineSpaceInPixels;
				Rect position4 = new Rect(num, num3, _Rect.width - (float)num, 20f);
				Quaternion rotation = mMOAvatarUserVarData3._Rotation;
				GUI.Label(position4, "R:" + rotation.ToString());
				num3 += _LineSpaceInPixels;
				GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "F:" + mMOAvatarUserVarData3._Flags);
				num3 += _LineSpaceInPixels;
				GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "POSITION 2:");
				num3 += _LineSpaceInPixels;
				GUI.Label(new Rect(num, num3, _Rect.width - (float)num, 20f), "(NONE)");
				num3 += _LineSpaceInPixels;
			}
		}
		GUI.EndScrollView();
		GUI.DragWindow();
	}
}
