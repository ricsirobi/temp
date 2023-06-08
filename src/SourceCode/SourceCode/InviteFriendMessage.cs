using System.Collections.Generic;
using UnityEngine;

internal class InviteFriendMessage : GenericMessage
{
	private const string PREFAB_KEY = "Prefab";

	private TaggedMessageHelper mMessageHelper;

	public InviteFriendMessage(MessageInfo messageInfo)
		: base(messageInfo)
	{
	}

	public override void Show()
	{
		mMessageHelper = new TaggedMessageHelper(mMessageInfo);
		if (mMessageHelper.SubType == "RegisteredFriend")
		{
			LoadInviteFriendDB();
			return;
		}
		_IsSystemMessage = true;
		base.Show();
	}

	public override void Yes()
	{
		mUiGenericDB.SetActive(value: false);
		InviteFriend.pUpdateUserMessageObj = true;
		InviteFriend.PopUpInviteFriend(null);
	}

	public void LoadInviteFriendDB(bool disableUI = false)
	{
		if (disableUI)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
		}
		mMessageHelper = new TaggedMessageHelper(mMessageInfo);
		RsResourceEventHandler @object = null;
		if (!mMessageHelper.MemberMessage.ContainsKey("Prefab"))
		{
			return;
		}
		string subType = mMessageHelper.SubType;
		if (!(subType == "Inviter"))
		{
			if (subType == "RegisteredFriend")
			{
				@object = RegisteredUserDBReady;
			}
		}
		else
		{
			@object = FriendRegisteredDBReady;
		}
		string[] array = mMessageHelper.MemberMessage["Prefab"].Split('/');
		RsResourceManager.Load(array[0] + "/" + array[1], @object.Invoke);
	}

	private void FriendRegisteredDBReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (mMessageHelper == null)
			{
				break;
			}
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(mMessageHelper.MemberMessage["Prefab"]);
			if (!(gameObject != null))
			{
				break;
			}
			mUiGenericDB = Object.Instantiate(gameObject);
			UiInviteFriendRegistered component = mUiGenericDB.GetComponent<UiInviteFriendRegistered>();
			if (component != null)
			{
				component._MessageObject = WsUserMessage.pInstance.gameObject;
				component._YesMessage = "OnYes";
				component._NoMessage = "OnClose";
				component._CloseMessage = "OnClose";
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mMessageInfo.Data);
				string key = "RUN";
				string newChar = "";
				if (dictionary.ContainsKey(key))
				{
					newChar = dictionary[key];
				}
				string key2 = "RCI";
				string newChar2 = "";
				if (dictionary.ContainsKey(key2))
				{
					newChar2 = dictionary[key2];
				}
				string key3 = "EMAIL";
				string text = "";
				if (dictionary.ContainsKey(key3))
				{
					text = dictionary[key3];
				}
				string friendRegisteredLine = ReplaceChar(mMessageHelper.MemberMessage["Line1"], "{{RUN}}", newChar);
				string text2 = ReplaceChar(mMessageHelper.MemberMessage["Line2"], "{{RCI}}", newChar2);
				if (text.Length > 3)
				{
					text2 = ReplaceChar(text2, "{{EMAIL}}", text);
				}
				string buddyLine = ReplaceChar(mMessageHelper.MemberMessage["Line3"], "{{RUN}}", newChar);
				component.SetFriendNameAndRewardText(friendRegisteredLine, text2, buddyLine);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			WsUserMessage.pInstance.OnClose();
			break;
		}
	}

	private void RegisteredUserDBReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (mMessageHelper == null)
			{
				break;
			}
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(mMessageHelper.MemberMessage["Prefab"]);
			if (!(gameObject != null))
			{
				break;
			}
			mUiGenericDB = Object.Instantiate(gameObject);
			UiInviteRegister component = mUiGenericDB.GetComponent<UiInviteRegister>();
			if (component != null)
			{
				component._MessageObject = WsUserMessage.pInstance.gameObject;
				component._OKMessage = "OnClose";
				component._CloseMessage = "OnClose";
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mMessageInfo.Data);
				string key = "RCF";
				string newChar = "";
				if (dictionary.ContainsKey(key))
				{
					newChar = dictionary[key];
				}
				string registeringLine = mMessageHelper.MemberMessage["Line1"];
				string rewardLine = ReplaceChar(mMessageHelper.MemberMessage["Line2"], "{{RCF}}", newChar);
				component.SetReward(registeringLine, rewardLine);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			WsUserMessage.pInstance.OnClose();
			break;
		}
	}

	private string ReplaceChar(string line, string oldChar, string newChar)
	{
		return line.Replace(oldChar, newChar);
	}
}
