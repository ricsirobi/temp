using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDPad : KAWidget
{
	public float _MaxTouchRange = 350f;

	public KAWidget _BtnUp;

	public KAWidget _BtnDown;

	public KAWidget _BtnLeft;

	public KAWidget _BtnRight;

	public KAWidget _BtnUpLeft;

	public KAWidget _BtnUpRight;

	public KAWidget _BtnDownLeft;

	public KAWidget _BtnDownRight;

	public KAWidget _BtnCenter;

	private Rect mRectWidgetUp;

	private Rect mRectWidgetDown;

	private Rect mRectWidgetLeft;

	private Rect mRectWidgetRight;

	private Rect mRectWidgetUpLeft;

	private Rect mRectWidgetDownLeft;

	private Rect mRectWidgetUpRight;

	private Rect mRectWidgetDownRight;

	private Rect mRectWidgetCenter;

	private Dictionary<int, KAWidget> mValidInput = new Dictionary<int, KAWidget>();

	private bool mIsEventHandlerRegistered;

	private bool mIsDiagonalInputEnabled;

	private Vector2 mDefaultPosition;

	private void Init(bool inStatus)
	{
		if (inStatus && !mIsEventHandlerRegistered)
		{
			mIsEventHandlerRegistered = true;
			TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Combine(TouchManager.OnFingerDownEvent, new OnFingerDown(OnTouchDown));
			TouchManager.OnTouchEvent = (OnTouch)Delegate.Combine(TouchManager.OnTouchEvent, new OnTouch(OnTouch));
			TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Combine(TouchManager.OnFingerUpEvent, new OnFingerUp(OnTouchEnd));
			ResetInput();
		}
		else if (!inStatus && mIsEventHandlerRegistered)
		{
			mIsEventHandlerRegistered = false;
			TouchManager.OnFingerDownEvent = (OnFingerDown)Delegate.Remove(TouchManager.OnFingerDownEvent, new OnFingerDown(OnTouchDown));
			TouchManager.OnTouchEvent = (OnTouch)Delegate.Remove(TouchManager.OnTouchEvent, new OnTouch(OnTouch));
			TouchManager.OnFingerUpEvent = (OnFingerUp)Delegate.Remove(TouchManager.OnFingerUpEvent, new OnFingerUp(OnTouchEnd));
		}
		mDefaultPosition = _BtnCenter.GetPosition();
	}

	protected override void OnDestroy()
	{
		Init(inStatus: false);
		base.OnDestroy();
	}

	private void OnTouchDown(int id, Vector2 pos)
	{
		KAWidget selectedWidget = GetSelectedWidget(pos);
		if (selectedWidget != null && IsValidInput(selectedWidget, id))
		{
			mValidInput.Add(id, selectedWidget);
			if (CanDisablePrevInput(id, selectedWidget))
			{
				SetInput(selectedWidget);
			}
			else
			{
				selectedWidget.OnPress(inPressed: false);
				mValidInput[id] = null;
			}
			UiJoystick.pInstance.pIsPressed = true;
		}
		else if (selectedWidget != null)
		{
			selectedWidget.OnPress(inPressed: false);
		}
	}

	private void OnTouch(int id, Vector2 pos)
	{
		if (!mValidInput.ContainsKey(id))
		{
			return;
		}
		CheckForTouchBoundary(id, pos);
		KAWidget selectedWidget = GetSelectedWidget(pos);
		if (selectedWidget != null && selectedWidget != mValidInput[id])
		{
			if (CanDisablePrevInput(id, selectedWidget))
			{
				DisablePrevInput(mValidInput[id]);
				SetInput(selectedWidget);
				mValidInput[id] = selectedWidget;
			}
			else
			{
				selectedWidget.OnPress(inPressed: false);
			}
		}
	}

	private void OnTouchEnd(int id, Vector2 pos)
	{
		if (mValidInput.ContainsKey(id))
		{
			DisablePrevInput(mValidInput[id]);
			mValidInput.Remove(id);
		}
		if (mValidInput.Count == 0 || Input.touchCount <= 1)
		{
			ResetInput();
		}
	}

	private void UpdateWidgetsRect()
	{
		mRectWidgetUp = GetWidgetRect(_BtnUp);
		mRectWidgetDown = GetWidgetRect(_BtnDown);
		mRectWidgetRight = GetWidgetRect(_BtnRight);
		mRectWidgetLeft = GetWidgetRect(_BtnLeft);
		mRectWidgetUpLeft = GetWidgetRect(_BtnUpLeft);
		mRectWidgetDownLeft = GetWidgetRect(_BtnDownLeft);
		mRectWidgetUpRight = GetWidgetRect(_BtnUpRight);
		mRectWidgetDownRight = GetWidgetRect(_BtnDownRight);
		mRectWidgetCenter = GetWidgetRect(_BtnCenter);
	}

	private Rect GetWidgetRect(KAWidget inWidget)
	{
		Rect result = default(Rect);
		if (inWidget == null)
		{
			return result;
		}
		Bounds bounds = inWidget.collider.bounds;
		Vector2 vector = new Vector2(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y);
		Vector2 vector2 = new Vector2(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y);
		if (UICamera.currentCamera == null)
		{
			return result;
		}
		Camera obj = KAUIManager.pInstance.camera;
		vector = obj.WorldToScreenPoint(new Vector3(vector.x, vector.y, 0f));
		vector2 = obj.WorldToScreenPoint(new Vector3(vector2.x, vector2.y, 0f));
		return new Rect(vector.x, (float)Screen.height - vector.y, vector2.x - vector.x, vector.y - vector2.y);
	}

	private KAWidget GetSelectedWidget(Vector2 inPos)
	{
		if (mRectWidgetUp.Contains(inPos))
		{
			return _BtnUp;
		}
		if (mRectWidgetDown.Contains(inPos))
		{
			return _BtnDown;
		}
		if (mRectWidgetLeft.Contains(inPos))
		{
			return _BtnLeft;
		}
		if (mRectWidgetRight.Contains(inPos))
		{
			return _BtnRight;
		}
		if (mRectWidgetUpLeft.Contains(inPos))
		{
			return _BtnUpLeft;
		}
		if (mRectWidgetUpRight.Contains(inPos))
		{
			return _BtnUpRight;
		}
		if (mRectWidgetDownLeft.Contains(inPos))
		{
			return _BtnDownLeft;
		}
		if (mRectWidgetDownRight.Contains(inPos))
		{
			return _BtnDownRight;
		}
		if (mRectWidgetCenter.Contains(inPos))
		{
			return _BtnCenter;
		}
		return null;
	}

	private void ResetInput()
	{
		if (UiJoystick.pInstance != null)
		{
			UiJoystick.pInstance._Position.x = 0f;
			UiJoystick.pInstance._Position.y = 0f;
			UiJoystick.pInstance.pIsPressed = false;
		}
		_BtnUp.OnPress(inPressed: false);
		_BtnDown.OnPress(inPressed: false);
		_BtnLeft.OnPress(inPressed: false);
		_BtnRight.OnPress(inPressed: false);
		mIsDiagonalInputEnabled = false;
	}

	private void SetInput(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			SetButtonStatus(inWidget, inStatus: true);
			if (inWidget == _BtnUp)
			{
				UiJoystick.pInstance._Position.y = 1f;
			}
			else if (inWidget == _BtnDown)
			{
				UiJoystick.pInstance._Position.y = -1f;
			}
			else if (inWidget == _BtnRight)
			{
				UiJoystick.pInstance._Position.x = 1f;
			}
			else if (inWidget == _BtnLeft)
			{
				UiJoystick.pInstance._Position.x = -1f;
			}
			else if (inWidget == _BtnUpLeft)
			{
				UiJoystick.pInstance._Position.y = 1f;
				UiJoystick.pInstance._Position.x = -1f;
			}
			else if (inWidget == _BtnUpRight)
			{
				UiJoystick.pInstance._Position.y = 1f;
				UiJoystick.pInstance._Position.x = 1f;
			}
			else if (inWidget == _BtnDownLeft)
			{
				UiJoystick.pInstance._Position.y = -1f;
				UiJoystick.pInstance._Position.x = -1f;
			}
			else if (inWidget == _BtnDownRight)
			{
				UiJoystick.pInstance._Position.y = -1f;
				UiJoystick.pInstance._Position.x = 1f;
			}
		}
	}

	private void DisablePrevInput(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			SetButtonStatus(inWidget, inStatus: false);
			if (inWidget == _BtnUp || inWidget == _BtnDown)
			{
				UiJoystick.pInstance._Position.y = 0f;
			}
			else if (inWidget == _BtnLeft || inWidget == _BtnRight)
			{
				UiJoystick.pInstance._Position.x = 0f;
			}
			else if (inWidget != _BtnCenter)
			{
				UiJoystick.pInstance._Position.x = 0f;
				UiJoystick.pInstance._Position.y = 0f;
			}
		}
	}

	private bool CanDisablePrevInput(int inKey, KAWidget inWidget)
	{
		bool result = true;
		if (mValidInput.Count > 1 && (inWidget == _BtnUpRight || inWidget == _BtnUpLeft || inWidget == _BtnDownRight || inWidget == _BtnDownLeft))
		{
			return false;
		}
		foreach (KeyValuePair<int, KAWidget> item in mValidInput)
		{
			if (inKey != item.Key && ((mValidInput[item.Key] == _BtnUp && inWidget == _BtnDown) || (mValidInput[item.Key] == _BtnDown && inWidget == _BtnUp) || (mValidInput[item.Key] == _BtnLeft && inWidget == _BtnRight) || (mValidInput[item.Key] == _BtnRight && inWidget == _BtnLeft)))
			{
				result = false;
			}
		}
		return result;
	}

	private void SetButtonStatus(KAWidget inWidget, bool inStatus)
	{
		mIsDiagonalInputEnabled = true;
		if (inWidget == _BtnUpRight)
		{
			_BtnUp.OnPress(inStatus);
			_BtnRight.OnPress(inStatus);
		}
		else if (inWidget == _BtnUpLeft)
		{
			_BtnUp.OnPress(inStatus);
			_BtnLeft.OnPress(inStatus);
		}
		else if (inWidget == _BtnDownLeft)
		{
			_BtnDown.OnPress(inStatus);
			_BtnLeft.OnPress(inStatus);
		}
		else if (inWidget == _BtnDownRight)
		{
			_BtnDown.OnPress(inStatus);
			_BtnRight.OnPress(inStatus);
		}
		else
		{
			mIsDiagonalInputEnabled = false;
			inWidget.OnPress(inStatus);
		}
	}

	private bool IsValidInput(KAWidget inWidget, int id)
	{
		if (inWidget == _BtnUpLeft || inWidget == _BtnUpRight || inWidget == _BtnDownLeft || inWidget == _BtnDownRight || inWidget == _BtnCenter)
		{
			return false;
		}
		if (mValidInput.Count < 2 && !mIsDiagonalInputEnabled && !mValidInput.ContainsKey(id))
		{
			return true;
		}
		return false;
	}

	private void CheckForTouchBoundary(int id, Vector2 inPos)
	{
		inPos.y = (float)Screen.height - inPos.y;
		Vector2 pos = KAUIManager.pInstance.camera.ScreenToWorldPoint(inPos);
		Vector2 localPosition = _BtnCenter.GetLocalPosition(pos);
		if ((mDefaultPosition - localPosition).sqrMagnitude >= _MaxTouchRange * _MaxTouchRange)
		{
			OnTouchEnd(id, inPos);
		}
	}

	public override void SetVisibility(bool inVisible)
	{
		base.SetVisibility(inVisible);
		Init(inVisible);
		if (inVisible)
		{
			UpdateWidgetsRect();
		}
	}
}
