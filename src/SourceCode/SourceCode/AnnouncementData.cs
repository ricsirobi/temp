using System;

public class AnnouncementData
{
	private static int mAnnouncementPairID = 2014;

	private const string ANNOUNCEMENT_KEY = "Ann";

	private const string READ_KEY = "Read";

	private const string VISIT_KEY = "Visit";

	private AnnouncementDataSaveCallback mAnnouncementDataSaveCallback;

	private static AnnouncementData mInstance;

	private bool mIsReady;

	public static AnnouncementData pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = new AnnouncementData();
				mInstance.Init();
			}
			return mInstance;
		}
	}

	public bool pIsReady => mIsReady;

	public void Init()
	{
		if (ParentData.pIsReady)
		{
			ParentData.pInstance.LoadPairData(mAnnouncementPairID, LoadParentPairDataEventHandler);
		}
	}

	private void SaveParentPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		if (success)
		{
			if (mAnnouncementDataSaveCallback != null)
			{
				mAnnouncementDataSaveCallback(success: true);
				mAnnouncementDataSaveCallback = null;
			}
			return;
		}
		if (mAnnouncementDataSaveCallback != null)
		{
			mAnnouncementDataSaveCallback(success: false);
			mAnnouncementDataSaveCallback = null;
		}
		UtDebug.LogError("Error Saving pair data");
	}

	private void LoadParentPairDataEventHandler(bool success, PairData pData, object inUserData)
	{
		mIsReady = true;
	}

	public bool IsRead(int announcementID)
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(mAnnouncementPairID);
		if (pairDataByID == null)
		{
			return false;
		}
		if (pairDataByID.GetStringValue("AnnRead" + announcementID, string.Empty) == string.Empty)
		{
			return false;
		}
		return true;
	}

	public DateTime GetReadTime(int announcementID)
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(mAnnouncementPairID);
		if (pairDataByID == null)
		{
			return DateTime.MinValue;
		}
		string stringValue = pairDataByID.GetStringValue("AnnRead" + announcementID, string.Empty);
		if (stringValue != string.Empty)
		{
			_ = DateTime.MinValue;
			return DateTime.Parse(stringValue, UtUtilities.GetCultureInfo("en-US"));
		}
		return DateTime.MinValue;
	}

	public void MarkAsRead(int announcementID)
	{
		string inValue = ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US"));
		ParentData.pInstance.UpdatePairData(mAnnouncementPairID, "AnnRead" + announcementID, inValue);
	}

	public bool IsVisited(int announcementID)
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(mAnnouncementPairID);
		if (pairDataByID == null)
		{
			return false;
		}
		if (pairDataByID.GetStringValue("AnnVisit" + announcementID, string.Empty) == string.Empty)
		{
			return false;
		}
		return true;
	}

	public DateTime GetVisitedTime(int announcementID)
	{
		PairData pairDataByID = ParentData.pInstance.GetPairDataByID(mAnnouncementPairID);
		if (pairDataByID == null)
		{
			return DateTime.MinValue;
		}
		string stringValue = pairDataByID.GetStringValue("AnnVisit" + announcementID, string.Empty);
		if (stringValue != string.Empty)
		{
			_ = DateTime.MinValue;
			return DateTime.Parse(stringValue, UtUtilities.GetCultureInfo("en-US"));
		}
		return DateTime.MinValue;
	}

	public void MarkAsVisited(int announcementID)
	{
		string inValue = ServerTime.pCurrentTime.ToString(UtUtilities.GetCultureInfo("en-US"));
		ParentData.pInstance.UpdatePairData(mAnnouncementPairID, "AnnVisit" + announcementID, inValue);
	}

	public void Save(AnnouncementDataSaveCallback callback)
	{
		mAnnouncementDataSaveCallback = callback;
		ParentData.pInstance.SavePairData(mAnnouncementPairID, SaveParentPairDataEventHandler);
	}
}
