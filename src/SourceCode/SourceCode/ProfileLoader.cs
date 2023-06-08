using UnityEngine;

public class ProfileLoader : MonoBehaviour
{
	private static string mUserID = string.Empty;

	private static bool mShowCloseButton = false;

	private void Start()
	{
		if (!string.IsNullOrEmpty(mUserID))
		{
			UiProfile.ShowProfile(mUserID);
			UiProfile.pShowCloseButton = mShowCloseButton;
		}
		else
		{
			Debug.LogError("Cannot load profile. mUserID is null.");
		}
	}

	public static void ShowProfile(string inUserID, UILoadOptions inLoadOption = UILoadOptions.AUTO)
	{
		mUserID = inUserID;
		mShowCloseButton = true;
		if (GameConfig.GetKeyData("ProfileScene") != RsResourceManager.pCurrentLevel && UtMobileUtilities.CanLoadInCurrentScene(UiType.ProfilePage, inLoadOption))
		{
			UiProfile.ShowProfile(mUserID);
			return;
		}
		AvAvatar.SetActive(inActive: false);
		AvAvatar.SetStartPositionAndRotation();
		if (GameConfig.GetKeyData("ProfileScene") != RsResourceManager.pCurrentLevel)
		{
			UiProfile.pLastLevel = RsResourceManager.pCurrentLevel;
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			MainStreetMMOClient.pInstance.Disconnect();
		}
		RsResourceManager.LoadLevel(GameConfig.GetKeyData("ProfileScene"));
	}
}
