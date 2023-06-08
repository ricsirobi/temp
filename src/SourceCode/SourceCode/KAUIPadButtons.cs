using System.Collections.Generic;
using UnityEngine;

public class KAUIPadButtons : KAUI
{
	private List<string> mWidgetToCheck = new List<string>();

	private bool mAnyWidgetPressed;

	private bool mWidgetPressedThisFrame;

	private int mLastWidgetFrame;

	private string mLastAccessedWidget = "";

	public void AddButtonForCheck(string inName)
	{
		if (FindItem(inName) == null)
		{
			Debug.LogError("****************** Button " + inName + " Not Found *********************");
		}
		mWidgetToCheck.Add(inName);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (IsValidWidget(inWidget))
		{
			mLastAccessedWidget = inWidget.name;
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if (IsValidWidget(inWidget))
		{
			mWidgetPressedThisFrame = inPressed && (!mAnyWidgetPressed || mLastAccessedWidget != inWidget.name);
			mAnyWidgetPressed = inPressed;
			mLastAccessedWidget = inWidget.name;
			mLastWidgetFrame = Time.frameCount;
		}
	}

	private bool IsValidWidget(KAWidget inWidget)
	{
		return mWidgetToCheck.Contains(inWidget.name);
	}

	public bool IsWidgetPressed(string inWidgetName)
	{
		if (inWidgetName == mLastAccessedWidget)
		{
			return mAnyWidgetPressed;
		}
		return false;
	}

	public bool IsWidgetReleased(string inWidgetName)
	{
		if (inWidgetName == mLastAccessedWidget && !mAnyWidgetPressed)
		{
			return Time.frameCount - mLastWidgetFrame == 0;
		}
		return false;
	}

	public bool IsWidgetClicked(string inWidgetName)
	{
		if (inWidgetName == mLastAccessedWidget)
		{
			return mWidgetPressedThisFrame;
		}
		return false;
	}

	public bool IsAnyItemClicked()
	{
		return !string.IsNullOrEmpty(mLastAccessedWidget);
	}
}
