using JSGames.UI;
using UnityEngine;

public class KAUICursorManager : UICursorManager
{
	public static void SetCustomCursor(string text, Texture2D texture, UIFont font = null, int offX = 0, int offY = 0)
	{
		if (UICursorManager.pCursorManager == null)
		{
			return;
		}
		if (text == string.Empty && texture == null)
		{
			Debug.Log("Cannot set Custom Cursor without a Text or a Texture Field");
		}
		else if (texture != null)
		{
			JSGames.UI.UIWidget uIWidget = UICursorManager.pCursorManager.FindWidget("Custom");
			if (!uIWidget.gameObject.activeInHierarchy)
			{
				uIWidget.gameObject.SetActive(value: true);
			}
			uIWidget.mainTexture = texture;
			if (UtPlatform.IsMobile())
			{
				UICursorManager.pIsForceCursorVisibility = true;
			}
		}
		else
		{
			UICursorManager.pCursorManager.pCurrentCursor.gameObject.SetActive(value: false);
		}
	}

	public static void SetDefaultCursor()
	{
		SetDefaultCursor(UICursorManager.pCursorManager._DefaultCursorName);
	}

	public static void SetDefaultCursor(string cursorName, bool showHideSystemCursor = true)
	{
		UICursorManager.SetCursor(cursorName, showHideSystemCursor);
		UICursorManager.pCursorManager._DefaultCursorName = cursorName;
	}

	public static void SetExclusiveLoadingGear(bool status)
	{
		SetDefaultCursor(status ? "Loading" : "Arrow");
		if (status)
		{
			UICursorManager.pCursorManager.SetExclusive();
		}
		else
		{
			UICursorManager.pCursorManager.RemoveExclusive();
		}
	}
}
