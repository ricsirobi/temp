using System;
using UnityEngine;

public class UiMessageBoardMenu : KAUIMenu
{
	public Vector2 _Offset = new Vector2(0f, -13f);

	public LocaleString _PagesText = new LocaleString("Pages");

	public LocaleString _PagesOfText = new LocaleString("of");

	public UiMessageBoard _MessageBoard;

	public override void LoadItem(KAWidget item)
	{
		KAWidgetUserData userData = item.GetUserData();
		if (userData == null)
		{
			return;
		}
		Type type = userData.GetType();
		if (type == typeof(MessageUserData))
		{
			MessageUserData messageUserData = (MessageUserData)userData;
			if (messageUserData != null && !messageUserData.pLoaded)
			{
				messageUserData.pLoaded = true;
				if (_MessageBoard != null)
				{
					_MessageBoard.GetPlayerNameByUserID(messageUserData._Data.Creator, item.FindChildItem("TxtName"));
				}
			}
		}
		else if (type == typeof(ChallengeUserData))
		{
			ChallengeUserData challengeUserData = (ChallengeUserData)userData;
			if (challengeUserData == null)
			{
				return;
			}
			if (challengeUserData.UpdateChallengeTimer())
			{
				challengeUserData.Load();
				return;
			}
			UiMessageBoard uiMessageBoard = (UiMessageBoard)_ParentUi;
			if ((bool)uiMessageBoard)
			{
				uiMessageBoard.RemoveFromList(CombinedMessageType.CHALLENGE, challengeUserData._Challenge);
			}
			RemoveWidget(item);
		}
		else if (type == typeof(UiMessageInfoUserData))
		{
			((UiMessageInfoUserData)userData)?.Load();
		}
	}

	public override void LoadItemsInView()
	{
		base.LoadItemsInView();
	}

	public override void OnClick(KAWidget inWidget)
	{
		_ParentUi.OnClick(inWidget);
	}
}
