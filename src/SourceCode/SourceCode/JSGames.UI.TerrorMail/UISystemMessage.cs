using System.Collections.Generic;
using JSGames.UI.Util;
using UnityEngine.UI;

namespace JSGames.UI.TerrorMail;

public class UISystemMessage : UIWidget
{
	public Text _TxtReceivedDate;

	public UIWidget _UDTIcon;

	public UIWidget _TrophyIcon;

	private string[] mKeys = new string[1] { "Line1" };

	public void InitFields(MessageInfo inMessageInfo, TagAndDefaultText[] inTagsAndDefaultTexts)
	{
		UiMessageInfoUserData uiMessageInfoUserData = new UiMessageInfoUserData(inMessageInfo, null, inTagsAndDefaultTexts);
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(inMessageInfo);
		base.pData = uiMessageInfoUserData;
		UIUtil.FormatTaggedMessage(ref inMessageInfo.MemberMessage, inMessageInfo, mKeys, inTagsAndDefaultTexts);
		UIUtil.ReplaceTagWithUserNameByID(ref inMessageInfo.MemberMessage, inMessageInfo.FromUserID, "{{BuddyUserName}}", ServiceEventHandler);
		if (inMessageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
		{
			Dictionary<string, string> dictionary = TaggedMessageHelper.Match(inMessageInfo.Data);
			if (dictionary.ContainsKey("OwnerID"))
			{
				UIUtil.ReplaceTagWithUserNameByID(ref inMessageInfo.MemberMessage, dictionary["OwnerID"], "{{OwnerUserName}}", ServiceEventHandler);
			}
		}
		if (inMessageInfo.MemberMessage.Contains("{{PetName}}"))
		{
			UIUtil.ReplaceTagWithPetData(ref inMessageInfo.MemberMessage, UtUtilities.DeserializeFromXml(inMessageInfo.Data, typeof(RewardData)) as RewardData);
		}
		if ((bool)_Text)
		{
			pText = inMessageInfo.MemberMessage;
		}
		if (inMessageInfo.MessageTypeID.Value == 9)
		{
			_UDTIcon.pVisible = false;
		}
		else
		{
			_TrophyIcon.pVisible = false;
		}
		if ((bool)_TxtReceivedDate)
		{
			_TxtReceivedDate.text = inMessageInfo.CreateDate.Date.ToShortDateString() + "\n" + inMessageInfo.CreateDate.ToLocalTime().ToShortTimeString();
		}
		if (taggedMessageHelper.MemberMessage.ContainsKey("Line2"))
		{
			uiMessageInfoUserData.Keys = new string[1] { "Line2" };
		}
		base.gameObject.SetActive(value: true);
	}

	protected void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && inUserData != null && !string.IsNullOrEmpty((string)inObject))
		{
			UIWidget uIWidget = (UIWidget)inUserData;
			if (uIWidget != null)
			{
				SetPlayerName(uIWidget, (string)inObject);
			}
		}
	}

	private void SetPlayerName(UIWidget item, string name)
	{
		if (!(item == null))
		{
			item.pText = name;
			item.pState = WidgetState.INTERACTIVE;
		}
	}
}
