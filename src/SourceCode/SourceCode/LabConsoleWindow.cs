using UnityEngine;

public class LabConsoleWindow : MonoBehaviour
{
	public int _WindowID = 1000;

	public Rect _WindowRect = new Rect(0f, 0f, 350f, 350f);

	public bool _Visible = true;

	public string[] _HoldKeys;

	public string _ShowKey = "";

	private float mWindowHeight;

	private static GameObject mInstance;

	private ScientificExperiment mManager;

	public static void Show()
	{
		Show(Vector2.zero);
	}

	public static void Show(Vector2 inPosition)
	{
		LabConsoleWindow labConsoleWindow = null;
		if (mInstance == null)
		{
			mInstance = new GameObject();
			mInstance.name = "TaskStats";
			labConsoleWindow = mInstance.AddComponent<LabConsoleWindow>();
		}
		else
		{
			labConsoleWindow = mInstance.GetComponent<LabConsoleWindow>();
		}
		if (labConsoleWindow != null)
		{
			labConsoleWindow._Visible = true;
			labConsoleWindow._WindowRect.x = inPosition.x;
			labConsoleWindow._WindowRect.y = inPosition.y;
		}
	}

	public static void Hide()
	{
		if (mInstance != null)
		{
			LabConsoleWindow component = mInstance.GetComponent<LabConsoleWindow>();
			if (component != null)
			{
				component._Visible = false;
			}
		}
	}

	private void Awake()
	{
		mInstance = base.gameObject;
		Object.DontDestroyOnLoad(mInstance);
		mWindowHeight = _WindowRect.height;
	}

	private void Update()
	{
		if (_HoldKeys != null)
		{
			string[] holdKeys = _HoldKeys;
			for (int i = 0; i < holdKeys.Length; i++)
			{
				if (!Input.GetKey(holdKeys[i]))
				{
					return;
				}
			}
		}
		if (_ShowKey != "" && Input.GetKeyDown(_ShowKey))
		{
			_Visible = !_Visible;
		}
	}

	private void OnGUI()
	{
		GUI.skin = null;
		if (_Visible)
		{
			_WindowRect = GUI.Window(_WindowID, _WindowRect, OnWindowGUI, "Item temperatures");
			_WindowRect.height = mWindowHeight;
		}
	}

	private void OnWindowGUI(int inWindowID)
	{
		float x = 5f;
		float num = 15f;
		float height = 20f;
		float y = num;
		string text = "";
		if (mManager == null)
		{
			GameObject gameObject = GameObject.Find("PfScientificExperiment");
			if (gameObject != null)
			{
				mManager = gameObject.GetComponent<ScientificExperiment>();
			}
		}
		if (mManager != null && mManager.pCrucible != null && mManager.pCrucible.pTestItems != null)
		{
			text = "Crucible : " + mManager.pCrucible.pTemperature.ToString("F1");
			foreach (LabTestObject pTestItem in mManager.pCrucible.pTestItems)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text += "\n";
				}
				string text2 = string.Empty;
				if (pTestItem != null)
				{
					text2 = ((pTestItem.pState != null) ? (pTestItem.pTemperature.ToString("F1") + " State:" + pTestItem.pState.Name) : pTestItem.pTemperature.ToString("F1"));
				}
				text = text + pTestItem.name + ": " + text2;
			}
			Rect position = GUILayoutUtility.GetRect(new GUIContent(text), options: new GUILayoutOption[1] { GUILayout.Width(_WindowRect.width) }, style: GUI.skin.label);
			position.x = x;
			position.y = num;
			GUI.Label(position, text);
			y = position.y + position.height;
		}
		else
		{
			GUI.Label(new Rect(x, y, _WindowRect.width, height), "UNINITIALIZED");
		}
		if (GUI.Button(new Rect(2f, 2f, 50f, 15f), "close"))
		{
			_Visible = false;
		}
		if (Event.current.type == EventType.Repaint)
		{
			mWindowHeight = y;
		}
		GUI.DragWindow();
	}
}
