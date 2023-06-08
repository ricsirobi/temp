using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "TMSD", Namespace = "")]
public class TimedMissionSlotData
{
	[XmlElement(ElementName = "SID")]
	public int SlotID;

	[XmlElement(ElementName = "TP")]
	public SlotType Type;

	[XmlElement(ElementName = "IID")]
	public int ItemID;

	[XmlElement(ElementName = "MID")]
	public int MissionID;

	[XmlElement(ElementName = "SD")]
	public DateTime StartDate;

	[XmlElement(ElementName = "ST")]
	public TimedMissionState State;

	[XmlElement(ElementName = "PID")]
	public List<int> PetIDs;

	[XmlElement(ElementName = "LID")]
	public string LogID;

	[XmlElement(ElementName = "LOGS")]
	public int LogSeed;

	private int mCoolDownDuration;

	private bool mIsReady;

	private TimedMission mMission;

	[XmlIgnore]
	public int pCoolDownDuration
	{
		get
		{
			return mCoolDownDuration;
		}
		set
		{
			mCoolDownDuration = value;
		}
	}

	[XmlIgnore]
	public bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	[XmlIgnore]
	public TimedMission pMission
	{
		get
		{
			return mMission;
		}
		set
		{
			mMission = value;
		}
	}

	public bool pAdWatched { get; set; }

	public void Init()
	{
		if (ItemID > 0)
		{
			ItemData.Load(ItemID, OnItemDataLoaded, null);
		}
	}

	public void LoadFromItemData(ItemData itemData, int ID)
	{
		SlotID = ID;
		mCoolDownDuration = itemData.GetAttribute("TM_CooldownTime", 0);
		ItemID = itemData.ItemID;
		Type = (SlotType)itemData.GetAttribute("TM_SlotType", 0);
		mIsReady = true;
	}

	private void OnItemDataLoaded(int itemID, ItemData itemData, object inUserData)
	{
		if (itemData != null)
		{
			mCoolDownDuration = itemData.GetAttribute("TM_CooldownTime", 0);
			mIsReady = true;
		}
	}
}
