using System;
using System.Text;
using UnityEngine;

namespace JSGames.UI.Util;

public class UIUtil
{
	public static UIGenericDB CreateUIGenericDB(string assetName, string dbName)
	{
		if (string.IsNullOrEmpty(assetName) || string.IsNullOrEmpty(dbName))
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(assetName));
		gameObject.name = dbName;
		return gameObject.GetComponent<UIGenericDB>();
	}

	public static UIGenericDB DisplayGenericDB(string prefabName, string text, string title, GameObject msgObj, string okMessage, string yesMessage = null, string noMessage = null, string closeMessage = null, bool destroyOnClick = true)
	{
		UIGenericDB uIGenericDB = CreateUIGenericDB(prefabName, prefabName);
		if (uIGenericDB != null)
		{
			uIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			uIGenericDB.SetText(text, interactive: false);
			if (title != null)
			{
				uIGenericDB.SetTitle(title);
			}
			uIGenericDB._MessageObject = msgObj;
			uIGenericDB._YesMessage = yesMessage;
			uIGenericDB._NoMessage = noMessage;
			uIGenericDB._OKMessage = okMessage;
			uIGenericDB._CloseMessage = closeMessage;
			uIGenericDB.SetDestroyOnClick(destroyOnClick);
			uIGenericDB.SetExclusive();
		}
		return uIGenericDB;
	}

	public static void ShowServerError(ServerErrorMessageGroupInfo inErrorMessageInfo, OnUIErrorActionEventHandler errorActionHandler = null, bool destroyOnClick = false)
	{
		if (inErrorMessageInfo != null)
		{
			ShowServerError(inErrorMessageInfo.ErrorMessageText, errorActionHandler, destroyOnClick);
		}
	}

	public static void ShowServerError(LocaleString errorText, OnUIErrorActionEventHandler errorActionHandler = null, bool destroyOnClick = false)
	{
		UIErrorHandler uIErrorHandler = UIErrorHandler.ShowErrorUI(UIErrorHandler.ErrorMessageType.CRITICAL_ERROR);
		if (uIErrorHandler != null)
		{
			uIErrorHandler._DestroyOnClick = destroyOnClick;
			if (errorText != null)
			{
				uIErrorHandler.SetText(errorText.GetLocalizedString());
			}
			else
			{
				Debug.LogError("Error text is null");
			}
			if (errorActionHandler != null)
			{
				uIErrorHandler.pOnUIErrorActionEventHandler = errorActionHandler;
			}
		}
	}

	public static void ShowServerError(string errorText, OnUIErrorActionEventHandler errorActionHandler = null)
	{
		ServerErrorMessageGroupInfo serverErrorMessageGroupInfo = null;
		ServerErrorMessageGroupInfo[] errors = GameDataConfig.pInstance.ServerErrorMessages.Errors;
		foreach (ServerErrorMessageGroupInfo serverErrorMessageGroupInfo2 in errors)
		{
			if (!string.IsNullOrEmpty(Array.Find(serverErrorMessageGroupInfo2.ErrorTexts, (string element) => errorText.IndexOf(element, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				serverErrorMessageGroupInfo = serverErrorMessageGroupInfo2;
				break;
			}
		}
		if (serverErrorMessageGroupInfo != null)
		{
			ShowServerError(serverErrorMessageGroupInfo, errorActionHandler);
		}
	}

	public static UIWidget GetLoadingItemStatic(CoBundleItemData ud)
	{
		UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
		UIWidget cursorItem = UICursorManager.GetCursorItem();
		cursorItem.pVisible = true;
		if (ud != null)
		{
			cursorItem.pData = ud;
		}
		cursorItem.pState = WidgetState.NOT_INTERACTIVE;
		return cursorItem;
	}

	public static void SetVisibleRangeStatic(UIMenu menu, int sIdx, int numItems)
	{
		int num = sIdx + numItems - 1;
		if (num >= menu.pChildWidgets.Count)
		{
			num = menu.pChildWidgets.Count - 1;
		}
		if (!(menu != null))
		{
			return;
		}
		for (int i = sIdx; i <= num; i++)
		{
			UIWidget uIWidget = menu.pChildWidgets[i];
			CoBundleItemData coBundleItemData = (CoBundleItemData)uIWidget.pData;
			if (coBundleItemData == null)
			{
				UtDebug.LogError("Item " + uIWidget.name + " at index = " + i + " missing user data");
			}
			else if (coBundleItemData.IsNotLoaded())
			{
				coBundleItemData.LoadResource();
			}
		}
	}

	public static string NGUIToUGUIConvert(string input)
	{
		if (input.Length < 4 || string.IsNullOrEmpty(input))
		{
			return input;
		}
		input = input.Replace("[c]", "").Replace("[/c]", "");
		input = input.Replace("[b]", "<b>").Replace("[/b]", "</b>");
		input = input.Replace("[i]", "<i>").Replace("[/i]", "</i>");
		input = input.Replace("[u]", "<u>").Replace("[/u]", "</u>");
		input = input.Replace("[s]", "<s>").Replace("[/s]", "</s>");
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		string text = "";
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == '[' && !flag)
			{
				if (i + 7 <= input.Length)
				{
					text = input.Substring(i + 1, 6);
					if (IsStringValidHexColor(text))
					{
						stringBuilder.Append("<color=#" + text + ">");
						flag = true;
					}
				}
			}
			else if (c == '[' && flag)
			{
				stringBuilder.Replace("[" + text + "]", "");
				stringBuilder.Append("</color>");
				flag = false;
				if (i + 7 <= input.Length)
				{
					text = input.Substring(i + 1, 6);
					if (IsStringValidHexColor(text))
					{
						stringBuilder.Append("<color=#" + text + ">");
						flag = true;
					}
				}
			}
			stringBuilder.Append(c);
		}
		if (flag)
		{
			stringBuilder.Replace("[" + text + "]", "");
			stringBuilder.Append("</color>");
		}
		return stringBuilder.ToString();
	}

	public static bool IsStringValidHexColor(string hexString)
	{
		bool result = true;
		for (int i = 0; i < 6; i++)
		{
			if (!IsValidHexChar(hexString[i]))
			{
				result = false;
				break;
			}
		}
		return result;
	}

	public static bool IsValidHexChar(char c)
	{
		if ((c < '0' || c > '9') && (c < 'A' || c > 'F'))
		{
			if (c >= 'a')
			{
				return c <= 'f';
			}
			return false;
		}
		return true;
	}

	public static void FormatTaggedMessage(ref string inMessage, MessageInfo inMessageInfo, string[] inKeys, TagAndDefaultText[] inTagAndDefaultTexts)
	{
		if (inKeys.Length == 0)
		{
			return;
		}
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(inMessageInfo);
		string text = "";
		for (int i = 0; i < inKeys.Length; i++)
		{
			if (taggedMessageHelper.MemberMessage.ContainsKey(inKeys[i]))
			{
				if (i > 0)
				{
					text += "  ";
				}
				string message = taggedMessageHelper.MemberMessage[inKeys[i]];
				text += ReplaceTagsWithDefault(ref message, inTagAndDefaultTexts);
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			inMessage = text;
		}
	}

	private static string ReplaceTagsWithDefault(ref string message, TagAndDefaultText[] inTagAndDefaultTexts)
	{
		foreach (TagAndDefaultText tagAndDefaultText in inTagAndDefaultTexts)
		{
			message = message.Replace(tagAndDefaultText._Tag, tagAndDefaultText._DefaultText.GetLocalizedString());
		}
		return message;
	}

	public static void ReplaceTagWithUserNameByID(ref string inMessage, string userID, string tagName, WsServiceEventHandler inEventHandler)
	{
		if (!inMessage.Contains(tagName))
		{
			return;
		}
		bool flag = false;
		if (BuddyList.pIsReady)
		{
			Buddy buddy = BuddyList.pInstance.GetBuddy(userID);
			if (buddy != null && !string.IsNullOrEmpty(buddy.DisplayName))
			{
				flag = true;
				inMessage = inMessage.Replace(tagName, buddy.DisplayName);
			}
			else if (userID.Equals(UserProfile.pProfileData.GetGroupID()))
			{
				flag = true;
				inMessage = inMessage.Replace(tagName, "{{GroupName}}");
			}
		}
		if (!flag)
		{
			WsWebService.GetDisplayNameByUserID(userID, inEventHandler, tagName);
		}
	}

	public static void ReplaceTagWithPetData(ref string inMessage, RewardData inRewardData)
	{
		if (inRewardData != null && !string.IsNullOrEmpty(inRewardData.EntityID))
		{
			RaisedPetData byEntityID = RaisedPetData.GetByEntityID(new Guid(inRewardData.EntityID));
			if (byEntityID != null)
			{
				string name = byEntityID.Name;
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byEntityID.PetTypeID);
				inMessage = inMessage.Replace("{{PetName}}", name);
				inMessage = inMessage.Replace("{{PetType}}", sanctuaryPetTypeInfo._NameText.GetLocalizedString());
			}
		}
	}
}
