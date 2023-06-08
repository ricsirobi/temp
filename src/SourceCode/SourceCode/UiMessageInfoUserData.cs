using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMessageInfoUserData : KAWidgetUserData
{
	private MessageInfo mMessageInfo;

	private bool mLoaded;

	private AvPhotoManager mPhotoManager;

	private TagAndDefaultText[] mTagAndDefaultText;

	private KAWidget mTxtMessage;

	private string[] mKeys = new string[1] { "Line1" };

	public string[] Keys
	{
		get
		{
			return mKeys;
		}
		set
		{
			mKeys = value;
		}
	}

	public UiMessageInfoUserData(MessageInfo messageInfo, AvPhotoManager inPhotoManager, TagAndDefaultText[] tagAndDefaultText)
	{
		mMessageInfo = messageInfo;
		mPhotoManager = inPhotoManager;
		mTagAndDefaultText = tagAndDefaultText;
	}

	public MessageInfo GetMessageInfo()
	{
		return mMessageInfo;
	}

	public string GetText()
	{
		if (mTxtMessage != null)
		{
			return mTxtMessage.GetText();
		}
		return "";
	}

	public void SetText()
	{
		TaggedMessageHelper taggedMessageHelper = new TaggedMessageHelper(mMessageInfo);
		string text = "";
		for (int i = 0; i < mKeys.Length; i++)
		{
			if (taggedMessageHelper.MemberMessage.ContainsKey(mKeys[i]))
			{
				if (i > 0)
				{
					text += "  ";
				}
				text += ReplaceTagsWithDefault(taggedMessageHelper.MemberMessage[mKeys[i]]);
			}
		}
		mTxtMessage.SetText(text);
	}

	public void Load()
	{
		if (mLoaded || mMessageInfo == null)
		{
			return;
		}
		mLoaded = true;
		mTxtMessage = _Item.FindChildItem("TxtMessage");
		if (mTxtMessage != null)
		{
			ReplaceTagWithUserNameByID(mMessageInfo.FromUserID, "{{BuddyUserName}}");
			if (mMessageInfo.MemberMessage.Contains("{{OwnerUserName}}"))
			{
				Dictionary<string, string> dictionary = TaggedMessageHelper.Match(mMessageInfo.Data);
				if (dictionary.ContainsKey("OwnerID"))
				{
					ReplaceTagWithUserNameByID(dictionary["OwnerID"], "{{OwnerUserName}}");
				}
			}
			if (mMessageInfo.MemberMessage.Contains("{{PetName}}"))
			{
				ReplaceTagWithPetData();
			}
			SetText();
		}
		KAWidget kAWidget = _Item.FindChildItem("Picture");
		if (kAWidget != null && mPhotoManager != null)
		{
			mPhotoManager.TakePhotoUI(mMessageInfo.FromUserID, (Texture2D)kAWidget.GetTexture(), OnPhotoLoaded, kAWidget);
		}
	}

	public void ReplaceTagWithPetData()
	{
		RewardData rewardData = null;
		if (!string.IsNullOrEmpty(mMessageInfo.Data))
		{
			rewardData = UtUtilities.DeserializeFromXml(mMessageInfo.Data, typeof(RewardData)) as RewardData;
		}
		if (rewardData != null && !string.IsNullOrEmpty(rewardData.EntityID))
		{
			RaisedPetData byEntityID = RaisedPetData.GetByEntityID(new Guid(rewardData.EntityID));
			if (byEntityID != null)
			{
				string name = byEntityID.Name;
				SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(byEntityID.PetTypeID);
				mMessageInfo.MemberMessage = mMessageInfo.MemberMessage.Replace("{{PetName}}", name);
				mMessageInfo.MemberMessage = mMessageInfo.MemberMessage.Replace("{{PetType}}", sanctuaryPetTypeInfo._NameText.GetLocalizedString());
			}
		}
	}

	public void ReplaceTagWithUserNameByID(string userID, string tagName)
	{
		if (!mMessageInfo.MemberMessage.Contains(tagName))
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
				mMessageInfo.MemberMessage = mMessageInfo.MemberMessage.Replace(tagName, buddy.DisplayName);
			}
			else if (userID.Equals(UserProfile.pProfileData.GetGroupID()))
			{
				flag = true;
				mMessageInfo.MemberMessage = mMessageInfo.MemberMessage.Replace(tagName, "{{GroupName}}");
			}
		}
		if (!flag)
		{
			WsWebService.GetDisplayNameByUserID(userID, ServiceEventHandler, tagName);
		}
	}

	public string ReplaceTagsWithDefault(string message)
	{
		TagAndDefaultText[] array = mTagAndDefaultText;
		foreach (TagAndDefaultText tagAndDefaultText in array)
		{
			message = message.Replace(tagAndDefaultText._Tag, tagAndDefaultText._DefaultText.GetLocalizedString());
		}
		return message;
	}

	private void OnPhotoLoaded(Texture tex, object inUserData)
	{
		KAWidget kAWidget = (KAWidget)inUserData;
		if (kAWidget != null)
		{
			kAWidget.SetTexture(tex);
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == WsServiceEvent.COMPLETE && mMessageInfo != null && inUserData != null && !string.IsNullOrEmpty((string)inObject))
		{
			string oldValue = (string)inUserData;
			if (mTxtMessage != null)
			{
				mMessageInfo.MemberMessage = mMessageInfo.MemberMessage.Replace(oldValue, (string)inObject);
				SetText();
			}
		}
	}
}
