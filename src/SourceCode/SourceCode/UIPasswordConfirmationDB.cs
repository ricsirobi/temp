using System;
using JSGames.UI;
using JSGames.UI.Util;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPasswordConfirmationDB : UI
{
	public Action OnSubmit;

	public Action OnClose;

	public UIWidget _TitleText;

	public UIButton _SubmitButton;

	public UIButton _CloseButton;

	public UIEditBox _PasswordField;

	public LocaleString _EmptyPasswordText = new LocaleString("[Review] Please enter the password!");

	public LocaleString _WrongPasswordText = new LocaleString("[Review] Password you have entered is incorrect!");

	private string mPassword;

	protected override void OnClick(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _SubmitButton)
		{
			if (string.IsNullOrEmpty(_PasswordField.pText))
			{
				UIUtil.DisplayGenericDB("PfUIGenericDB", _EmptyPasswordText.GetLocalizedString(), null, base.gameObject, "OnGenericDBConfirm");
			}
			else if (mPassword.Equals(_PasswordField.pText))
			{
				if (OnSubmit != null)
				{
					OnSubmit();
				}
				DestroyDB();
			}
			else
			{
				UIUtil.DisplayGenericDB("PfUIGenericDB", _WrongPasswordText.GetLocalizedString(), null, base.gameObject, "OnGenericDBConfirm");
			}
		}
		else if (widget == _CloseButton)
		{
			if (OnClose != null)
			{
				OnClose();
			}
			DestroyDB();
		}
	}

	private void DestroyDB()
	{
		RemoveExclusive();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public static void Init(string title, string password, Action onSubmit = null, Action onClose = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUIPasswordConfirmationDB"));
		if (!(gameObject == null))
		{
			UIPasswordConfirmationDB component = gameObject.GetComponent<UIPasswordConfirmationDB>();
			component.SetExclusive();
			component.OnSubmit = onSubmit;
			component.OnClose = onClose;
			component.mPassword = password;
		}
	}
}
