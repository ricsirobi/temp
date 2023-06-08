using UnityEngine;

public class MyRoomsIntLevel : CoCommonLevel
{
	public string _DefaultRoomID = "Empty";

	private static MyRoomsIntLevel mInstance = null;

	private string mHouseExitMarker = "";

	private static string mExitMarkerTemp = "";

	public Transform _JoinMarker;

	private bool mIsMMOInitialized;

	private string mHostID = "UNDEFINED";

	public static MyRoomsIntLevel pInstance => mInstance;

	public static string pHouseExitMarker
	{
		get
		{
			if (!(mInstance != null))
			{
				return "";
			}
			return mInstance.mHouseExitMarker;
		}
	}

	public static void SetHouseExitMarker(string inMarker)
	{
		if (mInstance != null)
		{
			mInstance.mHouseExitMarker = inMarker;
			mExitMarkerTemp = "";
		}
		else
		{
			mExitMarkerTemp = inMarker;
		}
	}

	public string GetMyRoomsIntHostID()
	{
		if (mHostID == "UNDEFINED")
		{
			if (MainStreetMMOClient.pInstance != null)
			{
				mHostID = MainStreetMMOClient.pInstance.GetOwnerIDForLevel(RsResourceManager.pCurrentLevel);
			}
			if (mHostID == "")
			{
				mHostID = null;
			}
		}
		return mHostID;
	}

	public bool IsOthersMyRoomsInt()
	{
		bool result = true;
		string myRoomsIntHostID = GetMyRoomsIntHostID();
		if (string.IsNullOrEmpty(myRoomsIntHostID) || myRoomsIntHostID == UserInfo.pInstance.UserID)
		{
			result = false;
		}
		return result;
	}

	public override void Awake()
	{
		base.Awake();
		mInstance = this;
		mHouseExitMarker = mExitMarkerTemp;
		mExitMarkerTemp = "";
		if (!_DefaultRoomID.Equals("Empty"))
		{
			MainStreetMMOClient.UserRoomID = _DefaultRoomID;
		}
		MainStreetMMOClient.Init();
		CommonInventoryData.ReInit();
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.pBuddyTeleportMarker = _JoinMarker;
		}
	}

	public override void Update()
	{
		base.Update();
		if (!UserInfo.pIsReady || !UserInfo.pInstance.MultiplayerEnabled || mIsMMOInitialized)
		{
			return;
		}
		mIsMMOInitialized = true;
		if (!string.IsNullOrEmpty(mHouseExitMarker) && AvAvatar.pToolbar != null)
		{
			UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
			if (component != null)
			{
				component._BackBtnMarker = mHouseExitMarker;
			}
		}
	}

	public override void OnPropLoaderDone()
	{
	}
}
