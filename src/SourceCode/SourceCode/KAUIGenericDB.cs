using UnityEngine;

public class KAUIGenericDB : KAUI
{
	public delegate void MessageReceived(string inMessage);

	public GameObject _MessageObject;

	public string _YesMessage = "";

	public string _NoMessage = "";

	public string _OKMessage = "";

	public string _CloseMessage = "";

	public string _TextMessage = "";

	public string _MovieMessage = "";

	public string _ButtonClickedMessage = "";

	public int _MessageIdentifier = -1;

	private bool mDestroyOnClick;

	public event MessageReceived OnMessageReceived;

	protected override void Start()
	{
		base.Start();
		if (UtPlatform.IsiOS())
		{
			KAWidget kAWidget = FindItem("YesBtn");
			KAWidget kAWidget2 = FindItem("NoBtn");
			if (kAWidget != null && kAWidget2 != null)
			{
				Vector3 localPosition = kAWidget2.transform.localPosition;
				kAWidget2.transform.localPosition = kAWidget.transform.localPosition;
				kAWidget.transform.localPosition = localPosition;
			}
		}
		UIAnchor componentInChildren = GetComponentInChildren<UIAnchor>();
		if (componentInChildren != null && KAUIManager.pInstance != null)
		{
			componentInChildren.uiCamera = KAUIManager.pInstance.GetComponentInChildren<Camera>();
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "YesBtn" && _YesMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_YesMessage, null, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_YesMessage);
			}
		}
		else if (item.name == "NoBtn" && _NoMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_NoMessage, null, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_NoMessage);
			}
		}
		else if (item.name == "OKBtn" && _OKMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_OKMessage, null, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_OKMessage);
			}
		}
		else if (item.name == "CloseBtn" && _CloseMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_CloseMessage, null, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_CloseMessage);
			}
		}
		else if (item.name == "TxtDialog" && _TextMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_TextMessage, null, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_TextMessage);
			}
		}
		if (_ButtonClickedMessage.Length > 0)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage(_ButtonClickedMessage, item, SendMessageOptions.DontRequireReceiver);
			}
			if (this.OnMessageReceived != null)
			{
				this.OnMessageReceived(_ButtonClickedMessage);
			}
		}
		if (item != null)
		{
			Destroy(item.name);
		}
	}

	public void SetExclusive()
	{
		SetExclusive(new Color(0.5f, 0.5f, 0.5f, 0.5f));
	}

	public void SetExclusive(Color inMaskColor)
	{
		KAUI.SetExclusive(this, null, inMaskColor);
	}

	public void RemoveExclusive()
	{
		KAUI.RemoveExclusive(this);
	}

	public void SetDestroyOnClick(bool isDestroy)
	{
		mDestroyOnClick = isDestroy;
	}

	public void Destroy(string itemName)
	{
		if (mDestroyOnClick)
		{
			switch (itemName)
			{
			case "YesBtn":
			case "NoBtn":
			case "OKBtn":
			case "CloseBtn":
			case "TxtDialog":
				Object.Destroy(base.gameObject);
				break;
			}
		}
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}

	public void SetMessage(GameObject messageObject, string yesMessage, string noMessage, string okMessage, string closeMessage)
	{
		_MessageObject = messageObject;
		_YesMessage = yesMessage;
		_NoMessage = noMessage;
		_OKMessage = okMessage;
		_CloseMessage = closeMessage;
	}

	public void SetButtonVisibility(bool inYesBtn, bool inNoBtn, bool inOKBtn, bool inCloseBtn)
	{
		KAWidget kAWidget = FindItem("YesBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inYesBtn);
		}
		kAWidget = FindItem("NoBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inNoBtn);
		}
		kAWidget = FindItem("OKBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inOKBtn);
		}
		kAWidget = FindItem("CloseBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inCloseBtn);
		}
		if (inCloseBtn)
		{
			_BackButtonName = "CloseBtn";
		}
		else if (inOKBtn)
		{
			_BackButtonName = "OKBtn";
		}
		else if (inNoBtn)
		{
			_BackButtonName = "NoBtn";
		}
	}

	public void SetButtonLabel(string button, string text)
	{
		KAWidget kAWidget = FindItem(button);
		if (button != null)
		{
			kAWidget.SetText(text);
		}
	}

	public void SetButtonDisabled(bool inDisable, bool inYesBtn, bool inNoBtn, bool inOKBtn, bool inCloseBtn)
	{
		if (inYesBtn)
		{
			KAWidget kAWidget = FindItem("YesBtn");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(inDisable);
			}
		}
		if (inNoBtn)
		{
			KAWidget kAWidget2 = FindItem("NoBtn");
			if (kAWidget2 != null)
			{
				kAWidget2.SetDisabled(inDisable);
			}
			if (!inDisable && kAWidget2 != null)
			{
				_BackButtonName = "NoBtn";
			}
		}
		if (inOKBtn)
		{
			KAWidget kAWidget3 = FindItem("OKBtn");
			if (kAWidget3 != null)
			{
				kAWidget3.SetDisabled(inDisable);
			}
			if (!inDisable && kAWidget3 != null)
			{
				_BackButtonName = "OKBtn";
			}
		}
		if (inCloseBtn)
		{
			KAWidget kAWidget4 = FindItem("CloseBtn");
			if (kAWidget4 != null)
			{
				kAWidget4.SetDisabled(inDisable);
			}
			if (!inDisable && kAWidget4 != null)
			{
				_BackButtonName = "CloseBtn";
			}
		}
	}

	public void SetTitle(string inText)
	{
		KAWidget kAWidget = FindItem("TxtTitle");
		if (kAWidget != null)
		{
			kAWidget.SetText(inText);
		}
	}

	public void SetText(string inText, bool interactive)
	{
		KAWidget kAWidget = FindItem("TxtDialog");
		kAWidget.SetText(inText);
		if (interactive)
		{
			kAWidget.SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public void SetTextByID(int inTextID, string defaultText, bool interactive)
	{
		KAWidget kAWidget = FindItem("TxtDialog");
		kAWidget.SetTextByID(inTextID, defaultText);
		if (interactive)
		{
			kAWidget.SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		RemoveExclusive();
	}
}
