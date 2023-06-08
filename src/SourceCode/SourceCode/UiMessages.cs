using System.Collections;
using UnityEngine;

public class UiMessages : KAUI
{
	private static bool mShowCloseButton;

	public static UiMessages pInstance;

	public static string pLastLevel;

	private KAWidget mExitBtn;

	public static bool pShowCloseButton
	{
		get
		{
			return mShowCloseButton;
		}
		set
		{
			mShowCloseButton = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		pInstance = this;
		mExitBtn = FindItem("ExitBtn");
		if (mExitBtn != null)
		{
			mExitBtn.SetVisibility(mShowCloseButton);
		}
		StartCoroutine(GetClanData());
	}

	public IEnumerator GetClanData()
	{
		if (UserInfo.pInstance.UserID == UiProfile.pUserProfile.UserID)
		{
			Group group;
			if (Group.pIsReady)
			{
				group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			}
			else
			{
				Group.Reset();
				Group.Init(includeMemberCount: true);
				yield return new WaitUntil(() => Group.pIsReady);
				group = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			}
			if (group != null)
			{
				base.gameObject.BroadcastMessage("OnClan", group, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			Group.Get(UiProfile.pUserProfile.UserID, OnGroupGet);
		}
	}

	private void OnGroupGet(GetGroupsResult result, object inUserData)
	{
		if (result != null && result.Success)
		{
			if (UserProfile.pProfileData.InGroup(result.Groups[0].GroupID))
			{
				base.gameObject.BroadcastMessage("OnClan", result.Groups[0], SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			base.gameObject.BroadcastMessage("OnClanFailed", SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mExitBtn)
		{
			Close();
		}
	}

	public void Close()
	{
		base.gameObject.BroadcastMessage("OnCloseUI", SendMessageOptions.DontRequireReceiver);
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		Object.Destroy(base.gameObject);
		pInstance = null;
		RsResourceManager.Unload(GameConfig.GetKeyData("MessageBoardAsset"));
		RsResourceManager.UnloadUnusedAssets();
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("MessageBoardScene"))
		{
			AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			RsResourceManager.LoadLevel(pLastLevel);
		}
	}

	public void OnActive(bool inVisible)
	{
		base.gameObject.SetActive(inVisible);
	}
}
