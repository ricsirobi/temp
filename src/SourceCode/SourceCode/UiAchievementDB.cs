using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class UiAchievementDB : KAUI
{
	[Serializable]
	public class RewardIconData
	{
		public string Type;

		public Texture Texture;
	}

	public UiAchievementDialogClosedCallback pDialogClosedCallback;

	private MessageInfo mMessageInfo;

	private KAWidget mRewardItem;

	public RewardIconData[] _Icons;

	protected override void Start()
	{
		base.Start();
		if (FUEManager.pInstance != null)
		{
			FUEManager.pInstance.SetupHUD(base.gameObject.name);
		}
	}

	public void ShowTrophy()
	{
		KAWidget kAWidget = FindItem("Trophy");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		SetReward();
	}

	public void ShowGems()
	{
		KAWidget kAWidget = FindItem("ViewBtn");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		SetReward();
	}

	public void SetReward()
	{
		using StringReader textReader = new StringReader(mMessageInfo.Data);
		if (!(new XmlSerializer(typeof(RewardData)).Deserialize(textReader) is RewardData rewardData) || rewardData.Rewards == null)
		{
			return;
		}
		Reward reward = rewardData.Rewards[0];
		mRewardItem = FindItem("Rewards");
		if (reward == null || !(mRewardItem != null))
		{
			return;
		}
		if (reward.ItemID.HasValue)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			ItemData.Load(reward.ItemID.Value, OnLoadItemDataReady, null);
			return;
		}
		RewardIconData rewardIconData = Array.Find(_Icons, (RewardIconData t) => t.Type.Equals(reward.Type));
		if (rewardIconData != null)
		{
			mRewardItem.SetTexture(rewardIconData.Texture);
		}
		mRewardItem.SetText(reward.Amount.ToString());
	}

	public void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			mRewardItem.SetText(dataItem.ItemName);
			ItemTextureResData itemTextureResData = new ItemTextureResData();
			itemTextureResData.Init(ItemTextureEventHandler, dataItem.IconName);
			itemTextureResData.LoadData();
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void ItemTextureEventHandler(ItemResNameData resData)
	{
		if (resData is ItemTextureResData itemTextureResData)
		{
			mRewardItem.SetTexture(itemTextureResData._Texture);
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
	}

	public void ShowMedal()
	{
	}

	public void SetMessageInfo(MessageInfo inMessageInfo)
	{
		mMessageInfo = inMessageInfo;
	}

	public void SetText(string title, string achievement, string prompt)
	{
		KAWidget kAWidget = FindItem("TxtTitle");
		if (kAWidget != null)
		{
			kAWidget.SetText(title);
		}
		KAWidget kAWidget2 = FindItem("TxtAchievement");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(achievement);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		string text = inWidget.name;
		if (!(text == "OKBtn"))
		{
			if (!(text == "ViewBtn") || mMessageInfo == null || (mMessageInfo.MessageTypeID.Value != 9 && mMessageInfo.MessageTypeID.Value != 30))
			{
				return;
			}
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
			using (StringReader textReader = new StringReader(mMessageInfo.Data))
			{
				if (new XmlSerializer(typeof(RewardData)).Deserialize(textReader) is RewardData rewardData && rewardData.TaskGroupID.HasValue)
				{
					UiAchievements.SelectedAchievementGroupID = rewardData.TaskGroupID.Value;
				}
			}
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.pLastPositionOnGround = AvAvatar.pObject.transform.position;
			}
			if (mMessageInfo.MessageTypeID.Value == 9)
			{
				JournalLoader.Load("BtnAchievements", "", setDefaultMenuItem: true, base.gameObject);
			}
			else if (mMessageInfo.MessageTypeID.Value == 30)
			{
				UiClans.ShowClan(UserInfo.pInstance.UserID, null, ClanTabs.ACHIEVEMENTS);
			}
		}
		else if (pDialogClosedCallback != null)
		{
			pDialogClosedCallback();
		}
	}

	public void JournalActivated(GameObject inObject)
	{
		if (inObject != null)
		{
			UiJournal component = inObject.GetComponent<UiJournal>();
			if (component != null)
			{
				component._ExitMessageObject = base.gameObject;
			}
		}
	}

	public void OnJournalExit()
	{
		AvAvatar.SetUIActive(inActive: false);
		if (pDialogClosedCallback != null)
		{
			pDialogClosedCallback();
		}
	}
}
