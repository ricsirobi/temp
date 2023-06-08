using UnityEngine;
using UnityEngine.EventSystems;

namespace JSGames.UI;

public class UIGenericDB : UI
{
	public delegate void MessageReceived(string message);

	public UIButton _BtnYes;

	public UIButton _BtnNo;

	public UIButton _BtnOK;

	public UIButton _BtnClose;

	public UIWidget _TxtDialog;

	public UIWidget _TxtTitle;

	[SerializeField]
	private AudioClip _DialogVO;

	public string _YesMessage = "";

	public string _NoMessage = "";

	public string _OKMessage = "";

	public string _CloseMessage = "";

	public string _TextMessage = "";

	public string _MovieMessage = "";

	public string _ButtonClicked = "";

	public GameObject _MessageObject;

	public int _MessageIdentifier = -1;

	private bool mDestroyOnClick;

	private SnChannel mSoundChannel;

	public event MessageReceived OnMessageReceived;

	protected override void Start()
	{
		base.Start();
		if (UtPlatform.IsiOS() && _BtnYes != null && _BtnNo != null)
		{
			Vector3 localPosition = _BtnNo.transform.localPosition;
			_BtnNo.transform.localPosition = _BtnYes.transform.localPosition;
			_BtnYes.transform.localPosition = localPosition;
		}
		if (_DialogVO != null)
		{
			mSoundChannel = SnChannel.Play(_DialogVO, "VO_Pool", inForce: true, base.gameObject);
		}
		SetExclusive();
	}

	protected override void OnClick(UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _BtnYes && !string.IsNullOrEmpty(_YesMessage))
		{
			SetMessage(_YesMessage);
		}
		else if (widget == _BtnNo && !string.IsNullOrEmpty(_NoMessage))
		{
			SetMessage(_NoMessage);
		}
		else if (widget == _BtnOK && !string.IsNullOrEmpty(_OKMessage))
		{
			SetMessage(_OKMessage);
		}
		else if (widget == _BtnClose && !string.IsNullOrEmpty(_CloseMessage))
		{
			SetMessage(_CloseMessage);
		}
		else if (widget == _TxtDialog && !string.IsNullOrEmpty(_TextMessage))
		{
			SetMessage(_TextMessage);
		}
		if (!string.IsNullOrEmpty(_ButtonClicked))
		{
			SetMessage(_ButtonClicked);
		}
		if (widget != null)
		{
			Destroy(widget.name);
		}
	}

	public void Destroy(string widgetName)
	{
		if (mDestroyOnClick && (widgetName == _BtnYes.name || widgetName == _BtnNo.name || widgetName == _BtnOK.name || widgetName == _BtnClose.name || widgetName == _TxtDialog.name))
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Destroy()
	{
		Object.Destroy(base.gameObject);
	}

	private void SetMessage(string msg)
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(msg, null, SendMessageOptions.DontRequireReceiver);
		}
		if (this.OnMessageReceived != null)
		{
			this.OnMessageReceived(msg);
		}
	}

	public void SetDestroyOnClick(bool destroy)
	{
		mDestroyOnClick = destroy;
	}

	public void SetMessage(GameObject messageObject, string yesMessage, string noMessage, string okMessage, string closeMessage)
	{
		_MessageObject = messageObject;
		_YesMessage = yesMessage;
		_NoMessage = noMessage;
		_OKMessage = okMessage;
		_CloseMessage = closeMessage;
	}

	public void SetButtonVisibility(bool yesBtn, bool noBtn, bool okBtn, bool closeBtn)
	{
		if (_BtnYes != null)
		{
			_BtnYes.pVisible = yesBtn;
		}
		if (_BtnNo != null)
		{
			_BtnNo.pVisible = noBtn;
		}
		if (_BtnOK != null)
		{
			_BtnOK.pVisible = okBtn;
		}
		if (_BtnClose != null)
		{
			_BtnClose.pVisible = closeBtn;
		}
		SetBackButton(noBtn, okBtn, closeBtn);
	}

	public void SetButtonDisabled(bool disable, bool yesBtn, bool noBtn, bool okBtn, bool closeBtn)
	{
		if (yesBtn)
		{
			_BtnYes.pState = ((!disable) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		}
		if (noBtn)
		{
			_BtnNo.pState = ((!disable) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		}
		if (okBtn)
		{
			_BtnOK.pState = ((!disable) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		}
		if (closeBtn)
		{
			_BtnClose.pState = ((!disable) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		}
		if (!disable)
		{
			SetBackButton(noBtn, okBtn, closeBtn);
		}
	}

	public void SetTitle(string text)
	{
		if (_TxtTitle != null)
		{
			_TxtTitle.pText = text;
		}
	}

	public void SetText(string text, bool interactive)
	{
		UIWidget txtDialog = _TxtDialog;
		txtDialog.pText = text;
		txtDialog.pState = ((!interactive) ? WidgetState.NOT_INTERACTIVE : WidgetState.INTERACTIVE);
	}

	public void SetClip(AudioClip clip)
	{
		_DialogVO = clip;
	}

	public void SetTextByID(int inTextID, string defaultText, bool interactive)
	{
		UIWidget txtDialog = _TxtDialog;
		txtDialog.SetTextByID(inTextID, defaultText);
		txtDialog.pState = ((!interactive) ? WidgetState.NOT_INTERACTIVE : WidgetState.INTERACTIVE);
	}

	private void OnDisable()
	{
		RemoveExclusive();
	}

	protected void OnDestroy()
	{
		if (mSoundChannel != null)
		{
			mSoundChannel.Stop();
		}
	}

	private void SetBackButton(bool noBtn, bool okBtn, bool closeBtn)
	{
		if (closeBtn)
		{
			_BackButton = _BtnClose;
		}
		else if (okBtn)
		{
			_BackButton = _BtnOK;
		}
		else if (noBtn)
		{
			_BackButton = _BtnNo;
		}
	}
}
