using System;
using System.Collections;
using System.Collections.Generic;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.Console;

public class UIConsole : JSGames.UI.UI
{
	public Vector2 _MinimumSize = new Vector2(300f, 200f);

	public UIButton _ResizeButton;

	public UIWidget _ConsoleText;

	public UIEditBox _CommandInput;

	public UIButton _CloseButton;

	public UIButton _ExecuteButton;

	public UIButton _DeleteWordButton;

	public UIMenu _PromptListMenu;

	private RectTransform mRectTransform;

	private Vector2 mPointerDragStartPos;

	private Vector2 mInitialWindowSize;

	private Vector2 mInitialWindowPos;

	private bool mIsResizingWindow;

	private bool mIsMovingWindow;

	private List<string> mCommandHistory = new List<string>();

	private List<string> mDisplayHistory = new List<string>();

	private Queue<string> mPendingCommands = new Queue<string>();

	private int mCurrentCommandHistoryIdx;

	private const int mMaxHistorySize = 100;

	private const string MESSAGE_INVALID_COMMAND = "Invalid Command";

	private const string MESSAGE_PASSWORD_NOT_CONFIGURED = "Console locked.  No password found.  Can't unlock";

	private const string MESSAGE_CONSOLE_UNLOCKED = "Console unlocked.";

	private const string MESSAGE_CONSOLE_LOCKED = "Console locked.  Please enter correct password.";

	private const string MESSAGE_COMMAND_LIST = "Command List:";

	private const string SPACE = " ";

	private const string PROMPT_WIDGET_NAME = "PromptWidget";

	private string pCurrentCommand
	{
		get
		{
			return _CommandInput.pText;
		}
		set
		{
			_CommandInput.pText = value;
		}
	}

	public void WriteLine(string line)
	{
		if (mDisplayHistory.Count == 100)
		{
			mDisplayHistory.RemoveAt(0);
		}
		mDisplayHistory.Add(line);
		ScrollRect component = _ConsoleText.GetComponent<ScrollRect>();
		StartCoroutine(ScrollToEndInNextFrame(component));
	}

	protected override void Awake()
	{
		base.Awake();
		mRectTransform = (RectTransform)base.transform;
	}

	protected override void Start()
	{
		base.Start();
		foreach (KAConsole.Command pCommand in CommandsList.pCommands)
		{
			pCommand.OnWritingLine = (Action<string>)Delegate.Combine(pCommand.OnWritingLine, new Action<string>(WriteLine));
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!JSConsole.pConsoleWindowVisible)
		{
			return;
		}
		if (mPendingCommands.Count > 0)
		{
			pCurrentCommand = mPendingCommands.Dequeue();
			AddCommandHistory(pCurrentCommand);
			WriteLine(pCurrentCommand);
			if (!JSConsole.Execute(pCurrentCommand))
			{
				WriteLine("Invalid Command");
			}
			pCurrentCommand = string.Empty;
			mCurrentCommandHistoryIdx = mCommandHistory.Count;
		}
		else if (Input.GetKeyDown(KeyCode.Return))
		{
			ProcessCurrentCommand();
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			if (mCurrentCommandHistoryIdx > 0)
			{
				mCurrentCommandHistoryIdx--;
				pCurrentCommand = mCommandHistory[mCurrentCommandHistoryIdx];
			}
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow) && mCurrentCommandHistoryIdx < mCommandHistory.Count - 1)
		{
			mCurrentCommandHistoryIdx++;
			pCurrentCommand = mCommandHistory[mCurrentCommandHistoryIdx];
		}
	}

	public void AddCommandHistory(string commandText)
	{
		if (mCommandHistory.Count == 100)
		{
			mCommandHistory.RemoveAt(0);
		}
		mCommandHistory.Add(commandText);
	}

	private IEnumerator ScrollToEndInNextFrame(ScrollRect scrollrect)
	{
		yield return 0;
		scrollrect.normalizedPosition = Vector2.zero;
	}

	public void ShowHelp()
	{
		WriteLine("Command List:");
		foreach (KAConsole.Command pCommand in CommandsList.pCommands)
		{
			WriteLine(pCommand.Help());
		}
	}

	private IEnumerator SetFocusOnCommandInput()
	{
		if (!UtPlatform.IsMobile())
		{
			EventSystem.current.SetSelectedGameObject(_CommandInput.gameObject);
			_CommandInput.GetComponent<InputField>().Select();
			_CommandInput.GetComponent<InputField>().ActivateInputField();
			yield return new WaitForEndOfFrame();
			_CommandInput.GetComponent<InputField>().MoveTextEnd(shift: false);
		}
	}

	private void ProcessCurrentCommand()
	{
		if (JSConsole.pUnlocked)
		{
			AddCommandHistory(pCurrentCommand);
			WriteLine(pCurrentCommand);
			if (!JSConsole.Execute(pCurrentCommand))
			{
				WriteLine("Invalid Command");
			}
			StartCoroutine(SetFocusOnCommandInput());
		}
		else
		{
			string md5Hash = WsMD5Hash.GetMd5Hash(pCurrentCommand);
			if (string.IsNullOrEmpty(ProductConfig.pInstance.ConsolePassword))
			{
				WriteLine("Console locked.  No password found.  Can't unlock");
			}
			if (md5Hash == ProductConfig.pInstance.ConsolePassword)
			{
				JSConsole.pUnlocked = true;
				WriteLine("Console unlocked.");
			}
			else
			{
				WriteLine("Console locked.  Please enter correct password.");
			}
		}
		pCurrentCommand = string.Empty;
		mCurrentCommandHistoryIdx = mCommandHistory.Count;
	}

	protected override void OnDrag(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnDrag(widget, eventData);
		Vector2 vector = eventData.position - mPointerDragStartPos;
		vector.y /= base.transform.lossyScale.y;
		vector.x /= base.transform.lossyScale.x;
		if (mIsResizingWindow)
		{
			vector.y = 0f - vector.y;
			vector.x *= 2f;
			Vector2 sizeDelta = mInitialWindowSize + vector;
			sizeDelta.x = Mathf.Max(_MinimumSize.x, sizeDelta.x);
			sizeDelta.y = Mathf.Max(_MinimumSize.y, sizeDelta.y);
			mRectTransform.sizeDelta = sizeDelta;
		}
		else if (mIsMovingWindow)
		{
			mRectTransform.localPosition = mInitialWindowPos + vector;
		}
	}

	protected override void OnPress(JSGames.UI.UIWidget widget, bool isPressed, PointerEventData eventData)
	{
		base.OnPress(widget, isPressed, eventData);
		mPointerDragStartPos = eventData.position;
		if (widget == _ResizeButton)
		{
			mIsResizingWindow = isPressed;
			mInitialWindowSize = mRectTransform.sizeDelta;
		}
		else if (widget != _CloseButton)
		{
			mIsMovingWindow = isPressed;
			mInitialWindowPos = mRectTransform.localPosition;
		}
	}

	protected override void OnClick(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _CloseButton)
		{
			if (!JSConsole.pShowConsoleLauncher)
			{
				JSConsole.pConsoleEnabled = false;
			}
			else
			{
				JSConsole.pConsoleWindowVisible = false;
			}
		}
		else if (widget == _ExecuteButton)
		{
			ProcessCurrentCommand();
		}
		else if (widget == _DeleteWordButton)
		{
			if (pCurrentCommand.Length >= 2)
			{
				int num = pCurrentCommand.LastIndexOf(" ", pCurrentCommand.Length - 2);
				pCurrentCommand = pCurrentCommand.Substring(0, num + 1);
			}
			else
			{
				pCurrentCommand = string.Empty;
			}
			StartCoroutine(SetFocusOnCommandInput());
		}
	}

	protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
	{
		base.OnVisibleInHierarchyChanged(newVisibleInHierarchy);
		if (newVisibleInHierarchy)
		{
			ShowPrompts(JSConsole.GetPrompts(_CommandInput.pText));
			StartCoroutine(SetFocusOnCommandInput());
		}
	}

	protected override void OnSelected(JSGames.UI.UIWidget widget, JSGames.UI.UI fromUI)
	{
		base.OnSelected(widget, fromUI);
		if (!(widget == null) && widget.pParentUI == _PromptListMenu)
		{
			ApplyPrompt(widget.pText);
			StartCoroutine(SetFocusOnCommandInput());
		}
	}

	protected override void OnValueChanged(UIEditBox editBox, string text)
	{
		base.OnValueChanged(editBox, text);
		if (editBox == _CommandInput)
		{
			ShowPrompts(JSConsole.GetPrompts(text));
		}
	}

	private void ApplyPrompt(string prompt)
	{
		int length = pCurrentCommand.LastIndexOf(' ') + 1;
		pCurrentCommand = pCurrentCommand.Substring(0, length) + prompt + " ";
	}

	private void ShowPrompts(List<string> prompts)
	{
		_PromptListMenu.ClearChildren();
		if (prompts == null)
		{
			return;
		}
		foreach (string prompt in prompts)
		{
			JSGames.UI.UIButton obj = _PromptListMenu.AddWidget("PromptWidget") as JSGames.UI.UIButton;
			obj.pVisible = true;
			obj.pText = prompt;
		}
	}
}
