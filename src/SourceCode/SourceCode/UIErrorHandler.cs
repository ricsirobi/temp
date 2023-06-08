using System;
using System.Collections.Generic;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIErrorHandler : UI
{
	public enum ErrorMessageType
	{
		NONE,
		VALIDATE_TOKEN_FAILED,
		PREFETCH_ERROR,
		CONNECTION_UNAVAILABLE,
		CRITICAL_ERROR
	}

	public enum Action
	{
		NONE,
		PUSH_TO_LOGIN,
		REPORT_SERVER
	}

	[Serializable]
	public class ErrorMessageInfo
	{
		public ErrorMessageType _MessageType;

		public LocaleString _MessageText;

		public int _Priority;
	}

	public OnUIErrorActionEventHandler pOnUIErrorActionEventHandler;

	public OnUIErrorExitEventHandler pOnUIErrorExitEventHandler;

	protected static UIErrorHandler mInstance = null;

	public static bool pHandleErrors = true;

	public bool _DestroyOnClick;

	public LocaleString _DefaultErrorMessageText = new LocaleString("Oops! Something went wrong! Try again later.");

	public List<ErrorMessageInfo> _ErrorMessages;

	private ErrorMessageType mErrorMessageType;

	private int mErrorMessagePriority = -1;

	protected JSGames.UI.UIWidget mOkBtn;

	protected string mErrorMessage;

	public static UIErrorHandler pInstance => mInstance;

	public ErrorMessageType pErrorMessageType
	{
		get
		{
			return mErrorMessageType;
		}
		set
		{
			mErrorMessageType = value;
		}
	}

	protected override void Awake()
	{
		if (mInstance != null)
		{
			if (pOnUIErrorExitEventHandler != null)
			{
				pOnUIErrorExitEventHandler();
			}
			ExitDB();
		}
		else
		{
			base.Awake();
			mInstance = this;
		}
	}

	protected override void Start()
	{
		base.Start();
		mOkBtn = FindWidget("OKBtn");
		SetExclusive(new Color(0.5f, 0.5f, 0.5f, 0.5f));
		if (!string.IsNullOrEmpty(mErrorMessage))
		{
			JSGames.UI.UIWidget uIWidget = FindWidget("TxtDialog");
			if (uIWidget != null)
			{
				uIWidget.pText = mErrorMessage;
			}
		}
	}

	private void OnDisable()
	{
		RemoveExclusive();
	}

	protected void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	protected override void OnClick(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (!(widget.name == "OKBtn"))
		{
			return;
		}
		if (pOnUIErrorActionEventHandler != null)
		{
			pOnUIErrorActionEventHandler();
			if (_DestroyOnClick)
			{
				ExitDB();
				pOnUIErrorActionEventHandler = null;
			}
		}
		else
		{
			HandleError();
			ExitDB();
		}
	}

	protected virtual void HandleError()
	{
		pHandleErrors = false;
		if (GetHandleAction(pErrorMessageType) == Action.PUSH_TO_LOGIN && RsResourceManager.pCurrentLevel != GameConfig.GetKeyData("LoginScene"))
		{
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			GameUtilities.LoadLoginLevel(showRegstration: false, fullReset: false);
		}
		pHandleErrors = true;
	}

	public void SetText(string inErrorMessage)
	{
		mErrorMessage = inErrorMessage;
		JSGames.UI.UIWidget uIWidget = FindWidget("TxtDialog");
		if (uIWidget != null)
		{
			uIWidget.pText = mErrorMessage;
		}
	}

	public void SetText(ErrorMessageType inErrorType)
	{
		pErrorMessageType = inErrorType;
		mErrorMessage = _DefaultErrorMessageText.GetLocalizedString();
		if (_ErrorMessages != null && _ErrorMessages.Count > 0)
		{
			ErrorMessageInfo errorMessageInfo = _ErrorMessages.Find((ErrorMessageInfo sem) => sem._MessageType == inErrorType);
			if (errorMessageInfo != null && !string.IsNullOrEmpty(errorMessageInfo._MessageText._Text))
			{
				mErrorMessage = errorMessageInfo._MessageText.GetLocalizedString();
			}
		}
		JSGames.UI.UIWidget uIWidget = FindWidget("TxtDialog");
		if (uIWidget != null)
		{
			uIWidget.pText = mErrorMessage;
		}
	}

	public void SetButtonVisibility(bool visible)
	{
		if (mOkBtn != null)
		{
			mOkBtn.pVisible = visible;
		}
	}

	public static UIErrorHandler ShowErrorUI(ErrorMessageType inErrorType, int priority = -1)
	{
		if (pHandleErrors)
		{
			if (mInstance != null)
			{
				if (mInstance.pErrorMessageType == inErrorType)
				{
					return null;
				}
				if (priority == -1)
				{
					priority = mInstance.GetErrorPriority(inErrorType);
				}
				if (mInstance.mErrorMessagePriority >= priority && mInstance.pErrorMessageType != 0)
				{
					return null;
				}
				if (mInstance.pOnUIErrorExitEventHandler != null)
				{
					mInstance.pOnUIErrorExitEventHandler();
				}
				mInstance.ExitDB();
			}
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			UICursorManager.pVisibility = true;
			UIErrorHandler component = GetUIHandlerObject().GetComponent<UIErrorHandler>();
			component.Initialize();
			component.SetText(inErrorType);
			component.pVisible = true;
			if (priority == -1)
			{
				priority = component.GetErrorPriority(inErrorType);
			}
			component.mErrorMessagePriority = priority;
			return component;
		}
		return null;
	}

	protected static Action GetHandleAction(ErrorMessageType errType)
	{
		Action action = Action.NONE;
		if (errType == ErrorMessageType.CONNECTION_UNAVAILABLE)
		{
			return Action.NONE;
		}
		return Action.PUSH_TO_LOGIN;
	}

	protected int GetErrorPriority(ErrorMessageType inErrorType)
	{
		if (_ErrorMessages != null && _ErrorMessages.Count > 0)
		{
			ErrorMessageInfo errorMessageInfo = _ErrorMessages.Find((ErrorMessageInfo sem) => sem._MessageType == inErrorType);
			if (errorMessageInfo != null)
			{
				return errorMessageInfo._Priority;
			}
		}
		return 0;
	}

	private void Initialize()
	{
		pHandleErrors = true;
		mErrorMessageType = ErrorMessageType.NONE;
	}

	private static GameObject GetUIHandlerObject()
	{
		if (mInstance != null)
		{
			return mInstance.gameObject;
		}
		GameObject gameObject = GameObject.Find("PfUIErrorHandler");
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUIErrorHandler"));
		}
		if (gameObject != null)
		{
			gameObject.name = "PfUIErrorHandler";
		}
		return gameObject;
	}

	public void ExitDB()
	{
		mErrorMessageType = ErrorMessageType.NONE;
		pVisible = false;
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
public class UiErrorHandler : KAUI
{
	public enum ErrorMessageType
	{
		NONE,
		VALIDATE_TOKEN_FAILED,
		PREFETCH_ERROR,
		CONNECTION_UNAVAILABLE,
		CRITICAL_ERROR,
		RECHECK_TOKEN
	}

	public enum Action
	{
		NONE,
		PUSH_TO_LOGIN,
		REPORT_SERVER,
		RECHECK_TOKEN
	}

	[Serializable]
	public class ErrorMessageInfo
	{
		public ErrorMessageType _MessageType;

		public LocaleString _MessageText;

		public int _Priority;
	}

	public OnUiErrorActionEventHandler pOnUiErrorActionEventHandler;

	public OnUiErrorExitEventHandler pOnUiErrorExitEventHandler;

	public int _AllowedRetryAttempts = 3;

	public bool _DestroyOnClick;

	public LocaleString _DefaultErrorMessageText = new LocaleString("Oops! Something went wrong! But dont worry, the dragons are hard at work trying to fix it. Try again later.");

	public List<ErrorMessageInfo> _ErrorMessages;

	private ErrorMessageType mErrorMessageType;

	private int mErrorMessagePriority = -1;

	protected static UiErrorHandler mInstance = null;

	protected KAWidget mOkBtn;

	public static bool pHandleErrors = true;

	protected string mErrorMessage;

	public ErrorMessageType pErrorMessageType
	{
		get
		{
			return mErrorMessageType;
		}
		set
		{
			mErrorMessageType = value;
		}
	}

	public static UiErrorHandler pInstance => mInstance;

	protected override void Awake()
	{
		base.Awake();
		mInstance = this;
	}

	protected override void Start()
	{
		base.Start();
		mOkBtn = FindItem("OKBtn");
		KAUI.SetExclusive(this, new Color(0.5f, 0.5f, 0.5f, 0.5f), updatePriority: true);
		if (!string.IsNullOrEmpty(mErrorMessage))
		{
			KAWidget kAWidget = FindItem("TxtDialog");
			if (kAWidget != null)
			{
				kAWidget.SetText(mErrorMessage);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (mInstance == this)
		{
			mInstance = null;
		}
		KAUI.RemoveExclusive(this);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!(inWidget.name == "OKBtn"))
		{
			return;
		}
		if (pOnUiErrorActionEventHandler != null)
		{
			pOnUiErrorActionEventHandler();
			if (_DestroyOnClick)
			{
				ExitDB();
				pOnUiErrorActionEventHandler = null;
			}
		}
		else
		{
			HandleError();
			ExitDB();
		}
	}

	protected virtual void HandleError()
	{
		pHandleErrors = true;
	}

	public void SetText(string inErrorMessage)
	{
		mErrorMessage = inErrorMessage;
		KAWidget kAWidget = FindItem("TxtDialog");
		if (kAWidget != null)
		{
			kAWidget.SetText(mErrorMessage);
		}
	}

	public void SetText(ErrorMessageType inErrorType)
	{
		pErrorMessageType = inErrorType;
		mErrorMessage = _DefaultErrorMessageText.GetLocalizedString();
		if (_ErrorMessages != null && _ErrorMessages.Count > 0)
		{
			ErrorMessageInfo errorMessageInfo = _ErrorMessages.Find((ErrorMessageInfo sem) => sem._MessageType == inErrorType);
			if (errorMessageInfo != null && !string.IsNullOrEmpty(errorMessageInfo._MessageText._Text))
			{
				mErrorMessage = errorMessageInfo._MessageText.GetLocalizedString();
			}
		}
		KAWidget kAWidget = FindItem("TxtDialog");
		if (kAWidget != null)
		{
			kAWidget.SetText(mErrorMessage);
		}
	}

	public string GetErrorText(ErrorMessageType inErrorType)
	{
		return _ErrorMessages.Find((ErrorMessageInfo t) => t._MessageType == inErrorType)._MessageText.GetLocalizedString();
	}

	public void SetButtonVisibility(bool visible)
	{
		if (mOkBtn != null)
		{
			mOkBtn.SetVisibility(visible);
		}
	}

	public static UiErrorHandler ShowErrorUI(ErrorMessageType inErrorType, int priority = -1)
	{
		if (pHandleErrors)
		{
			if (mInstance != null)
			{
				if (mInstance.pErrorMessageType == inErrorType)
				{
					return null;
				}
				if (priority == -1)
				{
					priority = mInstance.GetErrorPriority(inErrorType);
				}
				if (mInstance.mErrorMessagePriority >= priority && mInstance.pErrorMessageType != 0)
				{
					return null;
				}
				if (mInstance.pOnUiErrorExitEventHandler != null)
				{
					mInstance.pOnUiErrorExitEventHandler();
				}
				mInstance.ExitDB();
			}
			AvAvatar.pState = AvAvatarState.PAUSED;
			UiErrorHandler component = GetUiHandlerObject().GetComponent<UiErrorHandler>();
			component.Initialize();
			component.SetText(inErrorType);
			UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
			component.SetVisibility(inVisible: true);
			if (priority == -1)
			{
				priority = component.GetErrorPriority(inErrorType);
			}
			component.mErrorMessagePriority = priority;
			return component;
		}
		return null;
	}

	protected static Action GetHandleAction(ErrorMessageType errType)
	{
		Action action = Action.NONE;
		switch (errType)
		{
		case ErrorMessageType.CONNECTION_UNAVAILABLE:
			return Action.NONE;
		case ErrorMessageType.VALIDATE_TOKEN_FAILED:
		case ErrorMessageType.RECHECK_TOKEN:
			return Action.RECHECK_TOKEN;
		default:
			return Action.PUSH_TO_LOGIN;
		}
	}

	protected int GetErrorPriority(ErrorMessageType inErrorType)
	{
		if (_ErrorMessages != null && _ErrorMessages.Count > 0)
		{
			ErrorMessageInfo errorMessageInfo = _ErrorMessages.Find((ErrorMessageInfo sem) => sem._MessageType == inErrorType);
			if (errorMessageInfo != null)
			{
				return errorMessageInfo._Priority;
			}
		}
		return 0;
	}

	private void Initialize()
	{
		pHandleErrors = true;
		mErrorMessageType = ErrorMessageType.NONE;
	}

	private static GameObject GetUiHandlerObject()
	{
		if (mInstance != null)
		{
			return mInstance.gameObject;
		}
		GameObject gameObject = GameObject.Find("PfUiServerErrorHandler");
		if (gameObject == null)
		{
			gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiServerErrorHandler"));
		}
		if (gameObject != null)
		{
			gameObject.name = "PfUiServerErrorHandler";
		}
		return gameObject;
	}

	public void ExitDB()
	{
		mErrorMessageType = ErrorMessageType.NONE;
		SetVisibility(inVisible: false);
		UnityEngine.Object.DestroyImmediate(base.gameObject);
	}
}
