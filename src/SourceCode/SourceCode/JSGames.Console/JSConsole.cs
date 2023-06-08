using System;
using System.Collections.Generic;
using JSGames.UI;
using UnityEngine;

namespace JSGames.Console;

public class JSConsole : JSGames.UI.UI
{
	private class ShowHelpCommand : KAConsole.Command
	{
		public ShowHelpCommand()
			: base("Help")
		{
		}

		public override string Help()
		{
			return "Help";
		}

		public override void Execute(string[] args)
		{
			mInstance._UIConsole.ShowHelp();
		}
	}

	public UIConsole _UIConsole;

	public UIConsoleLauncher _UIConsoleLauncher;

	public static JSConsole mInstance;

	private const string SPACE = " ";

	private const string HELP_COMMAND = "?";

	private int mCheatEntryPhase = -1;

	public float _TraceDeviationBuffer = 200f;

	public Vector2[] _CheatViewportPostions;

	public static bool pUnlocked { get; set; }

	public static bool pShowConsoleLauncher
	{
		get
		{
			if (UtPlatform.IsMobile())
			{
				return !Application.isEditor;
			}
			return false;
		}
	}

	public static bool pConsoleEnabled
	{
		get
		{
			return mInstance.pVisible;
		}
		set
		{
			mInstance.pVisible = value;
			if (!pShowConsoleLauncher)
			{
				pConsoleWindowVisible = true;
			}
		}
	}

	public static bool pConsoleWindowVisible
	{
		get
		{
			return mInstance._UIConsole.pVisible;
		}
		set
		{
			mInstance._UIConsole.pVisible = value;
			mInstance._UIConsoleLauncher.pVisible = !value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (UtPlatform.IsMobile())
		{
			TouchManager.OnDragStartEvent = (OnDragStart)Delegate.Combine(TouchManager.OnDragStartEvent, new OnDragStart(ProcessDragStart));
			TouchManager.OnDragEndEvent = (OnDragEnd)Delegate.Combine(TouchManager.OnDragEndEvent, new OnDragEnd(ProcessDragEnd));
			TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(ProcessDrag));
		}
	}

	private static string[] GetArguments(string[] commandKeys)
	{
		string[] array = new string[commandKeys.Length - 1];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = commandKeys[i + 1];
		}
		return array;
	}

	public static List<string> GetPrompts(string commandInput)
	{
		if (commandInput.Length == 0)
		{
			List<string> list = new List<string>();
			{
				foreach (KAConsole.Command pCommand in CommandsList.pCommands)
				{
					list.Add(pCommand.pName);
				}
				return list;
			}
		}
		string[] array = commandInput.Split(new string[1] { " " }, StringSplitOptions.None);
		if (array.Length == 0)
		{
			return null;
		}
		if (array.Length == 1)
		{
			List<string> list2 = new List<string>();
			{
				foreach (KAConsole.Command pCommand2 in CommandsList.pCommands)
				{
					if (pCommand2.pName.StartsWith(array[0], StringComparison.OrdinalIgnoreCase))
					{
						list2.Add(pCommand2.pName);
					}
				}
				return list2;
			}
		}
		foreach (KAConsole.Command pCommand3 in CommandsList.pCommands)
		{
			if (array[0].Equals(pCommand3.pName, StringComparison.OrdinalIgnoreCase))
			{
				GetArguments(array);
				return null;
			}
		}
		return null;
	}

	protected override void Awake()
	{
		base.Awake();
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		if (Application.isEditor)
		{
			pUnlocked = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyUp(KeyCode.BackQuote) && ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) || Application.isEditor))
		{
			pConsoleEnabled = !pConsoleEnabled;
		}
	}

	public static bool Execute(string commandText)
	{
		commandText = commandText.Trim();
		if (commandText.Length == 0)
		{
			return false;
		}
		string[] array = commandText.Split(' ');
		if (array[0] == "?")
		{
			mInstance._UIConsole.ShowHelp();
			return true;
		}
		foreach (KAConsole.Command pCommand in CommandsList.pCommands)
		{
			if (pCommand.pName.Equals(array[0], StringComparison.OrdinalIgnoreCase))
			{
				string[] arguments = GetArguments(array);
				pCommand.Execute(arguments);
				return true;
			}
		}
		return false;
	}

	private void ProcessDragStart(Vector2 position, int fingerID)
	{
		if (fingerID == 0 && _CheatViewportPostions.Length != 0 && mCheatEntryPhase == -1 && HasReachedDestination(position, 0))
		{
			mCheatEntryPhase = 0;
		}
	}

	private void ProcessDragEnd(Vector2 position, int fingerID)
	{
		if (fingerID == 0 && _CheatViewportPostions.Length != 0)
		{
			if (mCheatEntryPhase == _CheatViewportPostions.Length - 1 && HasReachedDestination(position, mCheatEntryPhase))
			{
				pConsoleEnabled = !pConsoleEnabled;
			}
			mCheatEntryPhase = -1;
		}
	}

	private bool ProcessDrag(Vector2 newPosition, Vector2 oldPosition, int fingerID)
	{
		if (fingerID == 0 && _CheatViewportPostions.Length != 0 && mCheatEntryPhase > -1 && mCheatEntryPhase < _CheatViewportPostions.Length - 1)
		{
			if (IsDeviatedFromPath(newPosition))
			{
				mCheatEntryPhase = -1;
				return false;
			}
			if (HasReachedDestination(newPosition, mCheatEntryPhase + 1))
			{
				mCheatEntryPhase++;
			}
		}
		return true;
	}

	private Vector2 GetScreenPoint(Vector2 point)
	{
		float x = (float)Screen.width * point.x;
		float y = (float)Screen.height - (float)Screen.height * point.y;
		return new Vector2(x, y);
	}

	private bool IsDeviatedFromPath(Vector2 position)
	{
		return GetSquaredDeviation(position) > _TraceDeviationBuffer * _TraceDeviationBuffer;
	}

	private bool HasReachedDestination(Vector2 currentPosition, int destinationIndex)
	{
		return Vector2.SqrMagnitude(GetScreenPoint(_CheatViewportPostions[destinationIndex]) - currentPosition) < _TraceDeviationBuffer * _TraceDeviationBuffer;
	}

	private float GetSquaredDeviation(Vector2 touchPosition)
	{
		if (mCheatEntryPhase >= _CheatViewportPostions.Length - 1)
		{
			return 0f;
		}
		Vector2 screenPoint = GetScreenPoint(_CheatViewportPostions[mCheatEntryPhase]);
		Vector2 screenPoint2 = GetScreenPoint(_CheatViewportPostions[mCheatEntryPhase + 1]);
		Vector2 normalized = (screenPoint - screenPoint2).normalized;
		float num = (0f - normalized.x) * (screenPoint.x - touchPosition.x) - normalized.y * (screenPoint.y - touchPosition.y) / (normalized.x * normalized.x + normalized.y * normalized.y);
		Vector2 vector = screenPoint + num * normalized;
		return Vector2.SqrMagnitude(touchPosition - vector);
	}
}
