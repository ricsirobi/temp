using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using KA.Framework;
using KA.Framework.ThirdParty;
using KnowledgeAdventure.Multiplayer.Utility;
using Microsoft.AppCenter.Unity.Crashes;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Networking;
using UnityEngine.UI;

public class KAConsole : MonoBehaviour
{
	public class Command
	{
		public string[] mCommandKeys;

		public string pName;

		public Action<string> OnWritingLine;

		public Command(string cstring)
		{
			pName = cstring;
			mCommandKeys = cstring.Split(' ');
		}

		public string[] IsMatch(string[] cks)
		{
			if (mCommandKeys.Length == 0)
			{
				return null;
			}
			int i;
			for (i = 0; i < mCommandKeys.Length; i++)
			{
				if (!mCommandKeys[i].StartsWith(cks[i], StringComparison.OrdinalIgnoreCase))
				{
					return null;
				}
			}
			string[] array = new string[cks.Length - i];
			for (int j = i; j < cks.Length; j++)
			{
				array[j - i] = cks[j];
			}
			return array;
		}

		public virtual string Help()
		{
			string text = "";
			string[] array = mCommandKeys;
			foreach (string text2 in array)
			{
				text = text + text2 + " ";
			}
			return text;
		}

		public virtual void Execute(string[] args)
		{
		}
	}

	private class SetConsoleSize : Command
	{
		public SetConsoleSize()
			: base("Set Console Size")
		{
		}

		public override string Help()
		{
			return "Set Console Size <width> < height>";
		}

		public override void Execute(string[] args)
		{
			if (args != null)
			{
				float w = 0f;
				float h = 0f;
				if (args.Length != 0)
				{
					w = float.Parse(args[0], NumberStyles.Number);
				}
				if (args.Length > 1)
				{
					h = float.Parse(args[1], NumberStyles.Number);
				}
				SetWindowSize(w, h);
			}
		}
	}

	private class CommandShowHelp : Command
	{
		public CommandShowHelp()
			: base("Help")
		{
		}

		public override string Help()
		{
			return "Help";
		}

		public override void Execute(string[] args)
		{
			ShowHelp();
		}
	}

	private class SetDebug : Command
	{
		public SetDebug()
			: base("Debug")
		{
		}

		public override string Help()
		{
			return "Debug <level>";
		}

		public override void Execute(string[] args)
		{
			if (args == null)
			{
				return;
			}
			if (args.Length == 1)
			{
				UtDebug._Level = int.Parse(args[0], NumberStyles.Number);
				WriteLine("Debug Level = " + UtDebug._Level);
			}
			else if (args[0].Equals("showerror", StringComparison.OrdinalIgnoreCase))
			{
				bool isOverridenToError = false;
				if (args.Length > 1)
				{
					isOverridenToError = UtStringUtil.Parse(args[1], inDefault: false);
				}
				UtDebug._IsOverridenToError = isOverridenToError;
				WriteLine("Debug Logs overriden to error: " + isOverridenToError);
			}
		}
	}

	private class DeletePlayerPref : Command
	{
		public DeletePlayerPref()
			: base("DeletePlayerPrefs")
		{
		}

		public override string Help()
		{
			return "DeletePlayerPrefs <keyname>";
		}

		public override void Execute(string[] args)
		{
			if (args != null && args.Length == 1)
			{
				if (PlayerPrefs.HasKey(args[0]))
				{
					PlayerPrefs.DeleteKey(args[0]);
					WriteLine("Deleted " + args[0] + " from player prefs.");
				}
				else
				{
					WriteLine(args[0] + " is not present in player prefs.");
				}
			}
		}
	}

	private class DebugMask : Command
	{
		public DebugMask()
			: base("DebugMask")
		{
		}

		public override string Help()
		{
			return "DebugMask <mask> <1|0>";
		}

		public override void Execute(string[] args)
		{
			if (args != null && args.Length > 1)
			{
				uint num = uint.Parse(args[0], NumberStyles.Number);
				int num2 = int.Parse(args[1], NumberStyles.Number);
				if (num2 != 0)
				{
					UtDebug._Mask |= num;
				}
				else
				{
					UtDebug._Mask &= ~num;
				}
				Log._Mask = UtDebug._Mask;
				WriteLine("DebugMask " + num + ((num2 != 0) ? " added" : " cleared"));
			}
			WriteLine("DebugMask is " + UtDebug._Mask);
		}
	}

	private class SaveDebug : Command
	{
		public SaveDebug()
			: base("SaveDebug")
		{
		}

		public override string Help()
		{
			return "SaveDebug mask|level <mask|level> ";
		}

		public override void Execute(string[] args)
		{
			if (args != null && args.Length == 2)
			{
				if (args[0].Equals("mask", StringComparison.OrdinalIgnoreCase))
				{
					PlayerPrefs.SetString("SAVEMASK", args[1]);
				}
				else if (args[0].Equals("level", StringComparison.OrdinalIgnoreCase))
				{
					PlayerPrefs.SetString("SAVELEVEL", args[1]);
				}
				WriteLine("Debug " + args[0] + " " + args[1] + " Saved");
			}
		}
	}

	private class Token : Command
	{
		public Token()
			: base("Token")
		{
		}

		public override string Help()
		{
			return "Token <token>";
		}

		public override void Execute(string[] args)
		{
			if (args != null)
			{
				PlayerPrefs.SetString("TOKEN", args[0]);
				WriteLine("Token set.  Will be used next time and removed.");
			}
		}
	}

	private class Crash : Command
	{
		public Crash()
			: base("Crash")
		{
		}

		public override string Help()
		{
			return "Crash";
		}

		public override void Execute(string[] args)
		{
			UtDebug.LogError("Forcefully crashing the game");
			ForceCrash();
		}
	}

	private class CrashMe : Command
	{
		public CrashMe()
			: base("CrashMe")
		{
		}

		public override string Help()
		{
			return "Crash with Diag Utils";
		}

		public override void Execute(string[] args)
		{
			UtDebug.LogError("Forcefully crashing the game using Utils.ForceCrash");
			ForceCrashUtils();
		}
	}

	private class CrashMeHarder : Command
	{
		public CrashMeHarder()
			: base("CrashMeHarder")
		{
		}

		public override string Help()
		{
			return "Crash with Diag Utils";
		}

		public override void Execute(string[] args)
		{
			UtDebug.LogError("Forcefully crashing the game using method from https://forum.unity.com/threads/how-to-force-crash-on-android-to-test-crash-reporting-systems.653845/");
			ForceCrashAndroid();
		}
	}

	private class BuildInfo : Command
	{
		public BuildInfo()
			: base("BuildInfo")
		{
		}

		public override string Help()
		{
			return "BuildInfo";
		}

		public override void Execute(string[] args)
		{
			string empty = string.Empty;
			empty += GetText("Appsflyer");
			empty = empty + "\n\n " + GetText("AppCenter");
			empty = empty + "\n\n " + GetText("UnityAds");
			empty = empty + "\n\n " + GetText("SuperAwesome");
			empty = empty + "\n\n " + GetText("OneSignal");
			empty = empty + "\n\n Unity Project Id - " + Application.cloudProjectId;
			empty = empty + "\n\n Unity Analytics UserID - " + AnalyticsService.Instance?.GetAnalyticsUserID();
			string changelistNumber = ProductSettings.pInstance.GetChangelistNumber();
			empty = empty + "\n\n " + (string.IsNullOrEmpty(changelistNumber) ? "P4 ChangeList not found" : changelistNumber);
			UILabel label = GameUtilities.DisplayGenericDB("PfKAUIGenericDBLg", empty, "BuildInfo", null, null, null, "OnCloseDB", "OnCloseDB", inDestroyOnClick: true, updatePriority: true).FindItem("TxtDialog").GetLabel();
			label.alignment = NGUIText.Alignment.Left;
			label.fontSize = 20;
			label.overflowMethod = UILabel.Overflow.ResizeHeight;
		}

		private string GetText(string requirement)
		{
			string text = string.Empty;
			switch (requirement)
			{
			case "UnityAds":
				text = GetPluginAppId(Plugin.UNITY_ADS) + " " + GetAdRegisteredInfo("UnityAds");
				break;
			case "SuperAwesome":
				text = PluginSettings.GetPluginParamValue(Plugin.SUPERAWESOME, PluginParamType.VIDEO_AD_KEY) + " " + GetAdRegisteredInfo("SuperAwesome");
				break;
			case "AppCenter":
				text = GetAppCenterSecret();
				break;
			case "OneSignal":
				text = "NA";
				break;
			}
			if (!string.IsNullOrEmpty(text))
			{
				return requirement + "  " + text;
			}
			return requirement + "Id is not available.";
		}

		private string GetAdRegisteredInfo(string providerName)
		{
			if (AdManager.pInstance.pAdProviders != null && AdManager.pInstance.pAdProviders.Find((AdPlugin p) => p._ProviderName == providerName) != null)
			{
				return "Active";
			}
			return "InActive";
		}

		private string GetPluginAppId(Plugin pluginType)
		{
			return PluginSettings.GetKeyData(pluginType)?._AppID;
		}

		private string GetAppCenterSecret()
		{
			GameObject gameObject = GameObject.Find("PfAppCenterSDK");
			string empty = string.Empty;
			if (gameObject != null)
			{
				_ = gameObject.GetComponent<AppCenterBehavior>() != null;
			}
			return empty;
		}
	}

	private const string TEXT_CONTROL_NAME = "COMMAND_ENTRY";

	public static List<string> mCommandHistory = new List<string>();

	public static List<Command> mCommandList = new List<Command>();

	public static Queue<string> mPendingCommands = new Queue<string>();

	public static int mMaxHistorySize = 100;

	public static GameObject mObject = null;

	private static string mUnlockCommand = "";

	private string[] mCommandKeys;

	private int mCurIDx;

	private bool mUpdateGameInputOnFocus;

	private bool mPrevGameInputState;

	[Header("UI")]
	public GameObject _ConsoleUI;

	public InputField _CommandInput;

	public GameObject _ShowConsoleBtn;

	public GameObject _ConsoleEntryTemplate;

	public Transform _ScrollListUI;

	public static bool pUnlocked
	{
		get
		{
			if (!Application.isEditor)
			{
				if (ProductConfig.pInstance != null)
				{
					return WsMD5Hash.GetMd5Hash(mUnlockCommand) == ProductConfig.pInstance.ConsolePassword;
				}
				return false;
			}
			return true;
		}
	}

	public virtual void Start()
	{
		AddCommand(new SetConsoleSize());
		AddCommand(new CommandShowHelp());
		AddCommand(new SetDebug());
		AddCommand(new DebugMask());
		AddCommand(new Token());
		AddCommand(new Crash());
		AddCommand(new CrashMe());
		AddCommand(new CrashMeHarder());
		AddCommand(new DeletePlayerPref());
		AddCommand(new SaveDebug());
		AddCommand(new BuildInfo());
	}

	public static void SetWindowSize(float w, float h)
	{
		if (!(w > 0f) && !(h > 0f))
		{
			return;
		}
		KAConsole component = mObject.GetComponent<KAConsole>();
		if (component != null && component._ConsoleUI != null)
		{
			RectTransform component2 = component._ConsoleUI.GetComponent<RectTransform>();
			if (component2 != null)
			{
				float x = ((w > 0f) ? w : component2.localScale.x);
				float y = ((h > 0f) ? h : component2.localScale.y);
				component2.localScale = new Vector3(x, y, component2.localScale.z);
			}
		}
	}

	public static void AddCommand(Command cmd)
	{
		if (!mCommandList.Contains(cmd))
		{
			mCommandList.Add(cmd);
		}
	}

	public static void RemoveCommand(Command cmd)
	{
		mCommandList.Remove(cmd);
	}

	public static void AddCommandHistory(string s)
	{
		if (mCommandHistory.Count == mMaxHistorySize)
		{
			mCommandHistory.RemoveAt(0);
		}
		mCommandHistory.Add(s);
	}

	public static void WriteLine(string s)
	{
		KAConsole component = mObject.GetComponent<KAConsole>();
		if (component != null && component._ConsoleEntryTemplate != null)
		{
			Text component2 = UnityEngine.Object.Instantiate(component._ConsoleEntryTemplate, component._ScrollListUI).GetComponent<Text>();
			if (component2 != null)
			{
				component2.text = s;
			}
		}
		else
		{
			UtDebug.LogError("Console ui or components are not configured properly");
		}
	}

	public static void ShowHelp()
	{
		WriteLine("===== Command List =====");
		foreach (Command mCommand in mCommandList)
		{
			WriteLine(mCommand.Help());
		}
	}

	public virtual void Awake()
	{
		if (mObject != null)
		{
			if (mObject.GetComponent<KAConsole>() != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			UnityEngine.Object.Destroy(mObject);
			mObject = null;
		}
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		mObject = base.gameObject;
	}

	private void Update()
	{
		if (!IsConsoleVisible() && Input.GetKeyUp(KeyCode.BackQuote) && ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl)) || Application.isEditor))
		{
			ShowConsole();
		}
		else if (IsConsoleVisible() && Input.GetKeyUp(KeyCode.BackQuote))
		{
			CloseConsole();
		}
		if (!IsConsoleVisible())
		{
			return;
		}
		if (mPendingCommands.Count > 0)
		{
			string s = mPendingCommands.Dequeue();
			AddCommandHistory(s);
			WriteLine(s);
			Execute(s);
			mCurIDx = mCommandHistory.Count;
		}
		else
		{
			if (Input.GetKeyUp(KeyCode.Return))
			{
				Execute();
			}
			if (Input.GetKeyUp(KeyCode.UpArrow) && mCurIDx > 0)
			{
				mCurIDx--;
				_CommandInput.text = mCommandHistory[mCurIDx];
			}
			if (Input.GetKeyUp(KeyCode.DownArrow) && mCurIDx < mCommandHistory.Count - 1)
			{
				mCurIDx++;
				_CommandInput.text = mCommandHistory[mCurIDx];
			}
		}
		if (_CommandInput.isFocused)
		{
			if (!mUpdateGameInputOnFocus)
			{
				mUpdateGameInputOnFocus = true;
				mPrevGameInputState = AvAvatar.pInputEnabled;
				AvAvatar.pInputEnabled = false;
			}
		}
		else if (mUpdateGameInputOnFocus)
		{
			AvAvatar.pInputEnabled = mPrevGameInputState;
			mUpdateGameInputOnFocus = false;
		}
	}

	private bool IsConsoleVisible()
	{
		if (_ConsoleUI != null)
		{
			return _ConsoleUI.activeInHierarchy;
		}
		return false;
	}

	public void CloseConsole()
	{
		if (_ConsoleUI != null)
		{
			_ConsoleUI.SetActive(value: false);
		}
		if (mUpdateGameInputOnFocus)
		{
			AvAvatar.pInputEnabled = mPrevGameInputState;
			mUpdateGameInputOnFocus = false;
		}
	}

	public void ShowConsole()
	{
		_ShowConsoleBtn.SetActive(value: false);
		if (_ConsoleUI != null)
		{
			_ConsoleUI.SetActive(value: true);
		}
	}

	public void HideConsole()
	{
		_ShowConsoleBtn.SetActive(value: true);
		CloseConsole();
	}

	public void Execute()
	{
		string text = _CommandInput.text;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (pUnlocked)
		{
			AddCommandHistory(text);
			WriteLine(text);
			Execute(text);
		}
		if (!pUnlocked)
		{
			mUnlockCommand = text;
			if (string.IsNullOrEmpty(ProductConfig.pInstance.ConsolePassword))
			{
				WriteLine("Console locked.  No password found.  Can't unlock");
			}
			if (pUnlocked)
			{
				WriteLine("Console unlocked.");
			}
			else
			{
				WriteLine("Console locked.  Please enter correct password.");
			}
		}
		_CommandInput.text = string.Empty;
		mCurIDx = mCommandHistory.Count;
	}

	private void Execute(string s)
	{
		if (s.Length == 0)
		{
			return;
		}
		mCommandKeys = s.Split(' ');
		if (mCommandKeys.Length == 0)
		{
			return;
		}
		if (mCommandKeys[0] == "?")
		{
			ShowHelp();
			return;
		}
		foreach (Command mCommand in mCommandList)
		{
			string[] array = mCommand.IsMatch(mCommandKeys);
			if (array != null)
			{
				mCommand.Execute(array);
				return;
			}
		}
		WriteLine("!!!ERROR!!! Invalid command.");
	}

	public static void ForceCrash()
	{
		mObject.GetComponent<KAConsole>().StartCoroutine("CrashGame");
	}

	private IEnumerator CrashGame()
	{
		string uri = "http://media.schoolofdragons.com/Content/DWAPromos/en-US/SoD-042418_TitanDeathSong.jpg";
		UnityWebRequest wwwTexture = UnityWebRequestTexture.GetTexture(uri);
		wwwTexture.SendWebRequest();
		while (!wwwTexture.isDone)
		{
			yield return null;
		}
		Resources.UnloadUnusedAssets();
		yield return Time.deltaTime;
		DownloadHandlerTexture.GetContent(wwwTexture);
	}

	public static void ForceCrashUtils()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Error Text", "Crash error log from ForceCrashUtils");
		Crashes.TrackError(new Exception(), dictionary);
		Utils.ForceCrash(ForcedCrashCategory.FatalError);
	}

	public static void ForceCrashAndroid()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("Error Text", "Crash error log from ForceCrashAndroid");
		Crashes.TrackError(new Exception(), dictionary);
		AndroidJavaObject androidJavaObject = new AndroidJavaObject("java.lang.String", "This is a test crash, ignore.");
		AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("java.lang.Exception", androidJavaObject);
		AndroidJavaObject androidJavaObject3 = new AndroidJavaClass("android.os.Looper").CallStatic<AndroidJavaObject>("getMainLooper", Array.Empty<object>()).Call<AndroidJavaObject>("getThread", Array.Empty<object>());
		androidJavaObject3.Call<AndroidJavaObject>("getUncaughtExceptionHandler", Array.Empty<object>()).Call("uncaughtException", androidJavaObject3, androidJavaObject2);
	}
}
