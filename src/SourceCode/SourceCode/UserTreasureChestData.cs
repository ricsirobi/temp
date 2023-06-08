using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "UserTreasureChestData", Namespace = "")]
public class UserTreasureChestData
{
	public int TreasureChestId;

	public string SceneName;

	public string GroupName;

	public string Time;

	[XmlElement(ElementName = "Chest")]
	public UserTreasureChestDataChest[] Chest;

	private List<UserTreasureChestDataChest> mChestList;

	[XmlIgnore]
	public List<UserTreasureChestDataChest> pChestList
	{
		get
		{
			return mChestList;
		}
		set
		{
			mChestList = value;
		}
	}

	public UserTreasureChestData()
	{
		Chest = null;
		TreasureChestId = -1;
		SceneName = "";
		GroupName = "";
		Time = "";
	}

	public void InitChestList()
	{
		if (Chest == null)
		{
			mChestList = new List<UserTreasureChestDataChest>();
		}
		else
		{
			mChestList = new List<UserTreasureChestDataChest>(Chest);
		}
	}

	public void Save(WsServiceEventHandler inCallback)
	{
		Chest = mChestList.ToArray();
		WsWebService.SetUserTreasureChest(SceneName, GroupName, this, inCallback, null);
	}

	public void NewChest(UserTreasureChestDataChest newChest)
	{
		mChestList.Add(newChest);
	}

	public bool IsChestFound(string chestName)
	{
		foreach (UserTreasureChestDataChest pChest in pChestList)
		{
			if (pChest.Name == chestName && pChest.Found)
			{
				return true;
			}
		}
		return false;
	}

	public void SetChestFound(string chestName, WsServiceEventHandler inCallback)
	{
		bool flag = false;
		foreach (UserTreasureChestDataChest pChest in pChestList)
		{
			if (pChest.Name == chestName)
			{
				pChest.Found = true;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			UserTreasureChestDataChest userTreasureChestDataChest = new UserTreasureChestDataChest();
			userTreasureChestDataChest.Found = true;
			userTreasureChestDataChest.Name = chestName;
			pChestList.Add(userTreasureChestDataChest);
		}
		WsWebService.SetUserChestFound(SceneName, GroupName, chestName, inCallback, null);
	}

	public void ClearChestList()
	{
		mChestList = new List<UserTreasureChestDataChest>();
	}
}
