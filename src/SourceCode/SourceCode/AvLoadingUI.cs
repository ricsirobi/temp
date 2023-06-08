using UnityEngine;

public class AvLoadingUI : MonoBehaviour
{
	public delegate void LoadProcessor(string inResName);

	public string _AvatarAnimation = "Idle";

	private int _FrameDelayCounter = 5;

	private bool mIdlePlayed;

	private Transform mMarker;

	private string mLoadResName = "";

	private LoadProcessor mLoadProcessor;

	public void SetupAvatar(Transform marker)
	{
		mMarker = marker;
		AvAvatar.SetDisplayNameVisible(inVisible: false);
		AvAvatar.pObject.SetActive(value: true);
		AvAvatar.SetParentTransform(null);
		AvAvatar.mTransform.localScale = Vector3.one;
		AvAvatar.pState = AvAvatarState.NONE;
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		if (mMarker != null)
		{
			AvAvatar.SetPosition(marker);
			AvAvatar.position -= AvAvatar.forward * 10f;
		}
	}

	public void SetLoadProcessor(LoadProcessor inProcessor, string inResName)
	{
		mLoadResName = inResName;
		mLoadProcessor = inProcessor;
	}

	private void Start()
	{
		mIdlePlayed = false;
	}

	private void Update()
	{
		if (!mIdlePlayed)
		{
			if (AvAvatar.pObject != null)
			{
				AvAvatar.PlayAnim(AvAvatar.pObject, _AvatarAnimation, 0f, 0f, WrapMode.Loop, 1f);
			}
			mIdlePlayed = true;
		}
		if (mLoadProcessor == null)
		{
			return;
		}
		if (_FrameDelayCounter == 0)
		{
			if (AvAvatar.pObject != null && mMarker != null)
			{
				AvAvatar.position += AvAvatar.forward * 10f;
			}
			mLoadProcessor(mLoadResName);
			mLoadProcessor = null;
		}
		else
		{
			_FrameDelayCounter--;
		}
	}
}
