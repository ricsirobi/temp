using System;
using UnityEngine;

public class BuildModeMenuScroller : MonoBehaviour
{
	[Serializable]
	public enum Direction
	{
		LEFT,
		RIGHT
	}

	public Direction _Direction;

	public UITexture _BackgroundTexture;

	public GameObject _MessageObject;

	public GameObject[] _Menus;

	public float _InitOffsetX = 20f;

	private Vector3 mTargetPos;

	private Vector3 mOriginalPos;

	private bool mShowMenu;

	private void Start()
	{
		mOriginalPos = new Vector3(_InitOffsetX, base.transform.position.y, base.transform.position.z);
	}

	private void OnEnable()
	{
		base.transform.localPosition = mOriginalPos;
		mShowMenu = true;
		ShowMenus(mShowMenu);
	}

	public void ToggleBuildModeMenu()
	{
		mShowMenu = !mShowMenu;
		ExpandBuildModeMenu(mShowMenu);
	}

	private void ExpandBuildModeMenu(bool isShowMenu)
	{
		if (!(_BackgroundTexture == null))
		{
			float num = ((_Direction == Direction.LEFT) ? (-_BackgroundTexture.width) : _BackgroundTexture.width);
			if (isShowMenu)
			{
				mTargetPos = mOriginalPos;
			}
			else
			{
				mTargetPos = new Vector3(num + _InitOffsetX, mOriginalPos.y, mOriginalPos.z);
			}
			TweenPosition tweenPosition = TweenPosition.Begin(base.gameObject, 0.1f, mTargetPos);
			tweenPosition.eventReceiver = base.gameObject;
			tweenPosition.callWhenFinished = "TweenDone";
			ShowMenus(isActive: false);
		}
	}

	private void TweenDone()
	{
		ShowMenus(mShowMenu);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnScrollDone", mShowMenu);
		}
	}

	private void ShowMenus(bool isActive)
	{
		GameObject[] menus = _Menus;
		for (int i = 0; i < menus.Length; i++)
		{
			menus[i].SetActive(isActive);
		}
	}
}
