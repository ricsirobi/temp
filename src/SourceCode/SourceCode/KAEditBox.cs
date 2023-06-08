using System;
using System.Text.RegularExpressions;
using UnityEngine;

[RequireComponent(typeof(UIInput))]
public class KAEditBox : KAWidget
{
	private UIInput mInput;

	public KASkinInfo _EditInfo = new KASkinInfo();

	public LocaleString _DefaultText = new LocaleString("You can type here");

	public string _RegularExpression = "";

	public bool _CheckValidityOnInput = true;

	public bool _DisableUIMovement;

	public UIInput pInput
	{
		get
		{
			if (mInput == null)
			{
				mInput = GetComponent<UIInput>();
				if (mInput == null)
				{
					Debug.LogError("You need to have a UIInput component added to the transform " + base.transform.name + " where KAEditBox is");
				}
			}
			return mInput;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		UIInput uIInput = pInput;
		uIInput.onValidate = (UIInput.OnValidate)Delegate.Combine(uIInput.onValidate, new UIInput.OnValidate(ValidateText));
		EventDelegate.Add(pInput.onChange, OnChange);
		EventDelegate.Add(pInput.onSubmit, OnInputDone);
		pInput.defaultText = _DefaultText.GetLocalizedString();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UIInput uIInput = pInput;
		uIInput.onValidate = (UIInput.OnValidate)Delegate.Remove(uIInput.onValidate, new UIInput.OnValidate(ValidateText));
		EventDelegate.Remove(pInput.onChange, OnChange);
		EventDelegate.Remove(pInput.onSubmit, OnInputDone);
	}

	private void StartTweenPosition(GameObject go, float duration, Vector3 interfacePos)
	{
		TweenPosition tweenPosition = TweenPosition.Begin(go, duration, interfacePos);
		tweenPosition.callWhenFinished = "TweenPositionDone";
		tweenPosition.eventReceiver = base.gameObject;
	}

	public void TweenPositionDone(UITweener tween)
	{
		UtDebug.Log("tweening position done");
	}

	public void OnInputDone()
	{
		mUI.OnSubmit(this);
	}

	private void OnChange()
	{
		mUI.OnInput(this, pInput.value);
	}

	private char ValidateText(string text, int charIndex, char addedChar)
	{
		if (!_CheckValidityOnInput || _RegularExpression == null || Regex.IsMatch(text + addedChar, _RegularExpression))
		{
			return addedChar;
		}
		return '\0';
	}

	public bool IsValidText()
	{
		return Regex.IsMatch(GetText(), _RegularExpression);
	}

	public override void OnPress(bool inPressed)
	{
		base.OnPress(inPressed);
		pInput.OnPress(inPressed);
	}

	public override void OnClick()
	{
		base.OnClick();
		if (UtPlatform.IsWSA())
		{
			OnSelectTouchKeyboard(inSelected: true);
		}
	}

	public override void OnSelect(bool inSelected)
	{
		base.OnSelect(inSelected);
		_EditInfo.DoEffect(inSelected, this);
		OnSelectTouchKeyboard(inSelected);
	}

	private void OnSelectTouchKeyboard(bool inSelected)
	{
		pInput.OnSelect(inSelected);
	}

	public override string GetText()
	{
		return pInput.value;
	}

	public override void SetText(string text)
	{
		pInput.value = text;
	}

	public bool HasFocus()
	{
		return pInput.isSelected;
	}

	public void SetFocus(bool focus)
	{
		if (mUI != null && mUI.pUIManager != null)
		{
			UICamera.selectedObject = (focus ? base.gameObject : null);
		}
	}

	protected override void Update()
	{
		base.Update();
	}
}
