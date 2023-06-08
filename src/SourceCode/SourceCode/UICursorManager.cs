using JSGames.UI;
using UnityEngine;

public class UICursorManager : UI
{
	private static GameObject mLoadingGearGO;

	private static UI mLoadingGearInterface;

	private static int mLoadingStackDepth;

	private static UICursorManager mCursorManager;

	private static bool mVisibility;

	public bool _GlobalAutoHideCursor = true;

	public bool _AutoHideCursor;

	public float _AutoHideCursorTime = 2f;

	public string _DefaultCursorName;

	private float mHideCursorTimer = 10f;

	private Vector3 mPrevMousePos = new Vector3(0f, 0f, 0f);

	private bool mMouseMoved;

	private bool mHoverAutoHideCursor = true;

	private JSGames.UI.UIWidget mCurrentCursor;

	private bool mCursorHidden;

	private bool mIsSystemCursorVisible;

	private static bool mIsForceCursorVisibility;

	public static UICursorManager pCursorManager => mCursorManager;

	public static bool pVisibility
	{
		get
		{
			return mVisibility;
		}
		set
		{
			mVisibility = value;
			if (mCursorManager != null)
			{
				mCursorManager.pVisible = value;
			}
		}
	}

	public JSGames.UI.UIWidget pCurrentCursor
	{
		get
		{
			return mCurrentCursor;
		}
		set
		{
			mCurrentCursor = value;
		}
	}

	public static bool pIsForceCursorVisibility
	{
		get
		{
			return mIsForceCursorVisibility;
		}
		set
		{
			mIsForceCursorVisibility = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mCursorManager = this;
		mHideCursorTimer = _AutoHideCursorTime;
		pVisibility = mVisibility;
	}

	protected override void Start()
	{
		base.Start();
		foreach (JSGames.UI.UIWidget mChildWidget in mChildWidgets)
		{
			pState = WidgetState.NOT_INTERACTIVE;
			int count = mChildWidget.pChildWidgets.Count;
			for (int i = 0; i < count; i++)
			{
				mChildWidget.GetWidgetAt(i).pState = WidgetState.NOT_INTERACTIVE;
			}
			mChildWidget.pVisible = false;
		}
		if (string.IsNullOrEmpty(_DefaultCursorName))
		{
			_DefaultCursorName = "Arrow";
		}
		if (!Application.isEditor)
		{
			Cursor.visible = false;
		}
		SetCursor(_DefaultCursorName, showHideSystemCursor: true);
	}

	protected override void Update()
	{
		base.Update();
		mMouseMoved = Input.mousePosition.x != mPrevMousePos.x || Input.mousePosition.y != mPrevMousePos.y;
		mPrevMousePos = Input.mousePosition;
		if (!KAInput.pInstance)
		{
			return;
		}
		if (KAInput.pInstance.pInputMode == KAInputMode.TOUCH)
		{
			HideCursor(!mIsForceCursorVisibility);
		}
		else
		{
			if (KAInput.pInstance.pInputMode != KAInputMode.MOUSE)
			{
				return;
			}
			if (!_AutoHideCursor || !mHoverAutoHideCursor || !_GlobalAutoHideCursor)
			{
				mHideCursorTimer = _AutoHideCursorTime;
				HideCursor(t: false);
			}
			if (mMouseMoved || Input.GetMouseButtonDown(0))
			{
				HideCursor(t: false);
				mHideCursorTimer = _AutoHideCursorTime;
				return;
			}
			mHideCursorTimer -= Time.deltaTime;
			if (mHideCursorTimer < 0f)
			{
				mHideCursorTimer = _AutoHideCursorTime;
				HideCursor(t: true);
			}
		}
	}

	public static JSGames.UI.UIWidget FindCursorItem(string cName)
	{
		if (mCursorManager == null)
		{
			return null;
		}
		return mCursorManager.FindWidget(cName);
	}

	public static void SetAutoHide(bool t)
	{
		if (mCursorManager != null)
		{
			mCursorManager._AutoHideCursor = t;
		}
	}

	public static void SetHoverAutoHide(bool t)
	{
		if (mCursorManager != null)
		{
			mCursorManager.mHoverAutoHideCursor = t;
		}
	}

	public void HideCursor(bool t)
	{
		if (mCurrentCursor != null && mCurrentCursor.pVisible != !t)
		{
			mCurrentCursor.pVisible = !t;
		}
	}

	public static bool IsCursorHidden()
	{
		if (UtPlatform.IsMobile())
		{
			return false;
		}
		if (mCursorManager != null)
		{
			return mCursorManager.mCursorHidden;
		}
		return false;
	}

	public static bool MouseMoved()
	{
		if (mCursorManager != null)
		{
			return mCursorManager.mMouseMoved;
		}
		return true;
	}

	public static void SetCustomCursor(string text, Sprite sprite, Font font = null, int offX = 0, int offY = 0)
	{
		if (mCursorManager == null)
		{
			return;
		}
		if (text == string.Empty && sprite == null)
		{
			Debug.Log("Cannot set Custom Cursor without a Text or a Texture Field");
			return;
		}
		if (!string.IsNullOrEmpty(text))
		{
			SetCursorText(text, font, offX, offY);
		}
		else
		{
			SetCursorText(string.Empty, font, offX, offY);
		}
		if (sprite != null)
		{
			JSGames.UI.UIWidget uIWidget = mCursorManager.FindWidget("Custom");
			if (!uIWidget._Background.enabled)
			{
				uIWidget._Background.enabled = true;
			}
			SetCursorTexture(sprite, offX, offY);
		}
		else
		{
			mCursorManager.mCurrentCursor._Background.enabled = false;
		}
	}

	private static void SetCursorText(string text, Font font, int offX = 0, int offY = 0)
	{
		if (!(mCursorManager == null) && !string.IsNullOrEmpty(text))
		{
			SetCursor("Custom", showHideSystemCursor: true);
			if (mCursorManager.mCurrentCursor != null)
			{
				mCursorManager.mCurrentCursor.pText = text;
				mCursorManager.mCurrentCursor.AttachToPointer(new Vector2(offX, offY));
			}
		}
	}

	private static void SetCursorTexture(Sprite sprite, int offX = 0, int offY = 0)
	{
		if (!(mCursorManager == null) && !(sprite == null))
		{
			SetCursor("Custom", showHideSystemCursor: true);
			if (mCursorManager.mCurrentCursor != null)
			{
				mCursorManager.mCurrentCursor.pSprite = sprite;
				mCursorManager.mCurrentCursor.AttachToPointer(new Vector2(offX, offY));
			}
		}
	}

	public static void HandleCursor(string cursorName)
	{
	}

	public void SetVisibility(bool inHide)
	{
		mCursorManager.HideCursor(inHide);
	}

	public static void SetCursor(string cursorName, bool showHideSystemCursor)
	{
		HandleCursor(cursorName);
		if (mCursorManager == null)
		{
			return;
		}
		bool flag = true;
		if (mCursorManager.mCurrentCursor != null)
		{
			flag = mCursorManager.mCurrentCursor.pVisible;
			if (mCursorManager.mCurrentCursor.name == cursorName)
			{
				return;
			}
			mCursorManager.mCurrentCursor.pVisible = false;
			mCursorManager.mCurrentCursor.DetachFromPointer();
		}
		if (cursorName.Length == 0)
		{
			mCursorManager.mCurrentCursor = null;
			if (!Application.isEditor)
			{
				Cursor.visible = !showHideSystemCursor;
			}
			return;
		}
		mCursorManager.mCurrentCursor = mCursorManager.FindWidget(cursorName);
		if (mCursorManager.mCurrentCursor == null)
		{
			Debug.LogWarning("Cursor [" + cursorName + "] doesn't exist!");
		}
		else
		{
			mCursorManager.mCurrentCursor.pVisible = flag;
			mCursorManager.mCurrentCursor.AttachToPointer(Vector2.zero);
		}
		if (showHideSystemCursor && !Application.isEditor)
		{
			Cursor.visible = mCursorManager.mCurrentCursor == null;
		}
	}

	private static void InitLoadingGear()
	{
		if (mLoadingGearGO == null)
		{
			mLoadingGearGO = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUILoadingGears"));
			if (mLoadingGearGO != null)
			{
				Object.DontDestroyOnLoad(mLoadingGearGO);
				mLoadingGearInterface = mLoadingGearGO.GetComponent<UI>();
			}
		}
	}

	private static void ShowLoadingGear(bool inShow)
	{
		InitLoadingGear();
		if (mLoadingGearGO != null)
		{
			mLoadingGearGO.SetActive(inShow);
		}
	}

	public static void ShowExclusiveLoadingGear(bool inShow, bool useStack = false)
	{
		InitLoadingGear();
		if (useStack)
		{
			if (inShow)
			{
				mLoadingStackDepth++;
			}
			else if (mLoadingStackDepth > 0)
			{
				mLoadingStackDepth--;
			}
		}
		if (mLoadingGearGO != null && mLoadingGearInterface != null)
		{
			mLoadingGearGO.SetActive(inShow);
			if (inShow)
			{
				mLoadingGearInterface.SetExclusive(new Color(1f, 1f, 1f, 0f));
			}
			else if (mLoadingStackDepth == 0)
			{
				mLoadingGearInterface.RemoveExclusive();
			}
		}
	}

	public static Vector2 GetCursorPosition()
	{
		if (mCursorManager != null)
		{
			_ = mCursorManager.mCurrentCursor != null;
			return Vector2.zero;
		}
		return Vector2.zero;
	}

	public static string GetCursorName()
	{
		if (mCursorManager != null && mCursorManager.mCurrentCursor != null)
		{
			return mCursorManager.mCurrentCursor.name;
		}
		return "";
	}

	public static JSGames.UI.UIWidget GetCursorItem()
	{
		if (mCursorManager == null)
		{
			return null;
		}
		return mCursorManager.mCurrentCursor;
	}

	public static void SetToDefaultCursor(bool set)
	{
	}

	public void OnApplicationFocus(bool focus)
	{
		if (!Application.isEditor)
		{
			if (focus)
			{
				Cursor.visible = mIsSystemCursorVisible;
			}
			else
			{
				mIsSystemCursorVisible = Cursor.visible;
			}
		}
	}
}
