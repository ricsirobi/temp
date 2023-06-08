using SOD.Event;
using UnityEngine;

public class UiHUDSnoggletog : UiHUDEvent
{
	[Header("Job Board")]
	public string _AssetName;

	private const float mLootBoxCheckDelay = 0.5f;

	protected override void Start()
	{
		base.Start();
		InvokeRepeating("LootBoxCheck", 0f, 0.5f);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == m_HUDEventBtn)
		{
			LoadJobBoard();
		}
	}

	private void LootBoxCheck()
	{
		if (SnoggletogManager.pInstance != null)
		{
			mShowHUDButton = SnoggletogManager.pInstance.IsEventInProgress();
			if (m_Alert != null)
			{
				m_Alert.SetVisibility(SnoggletogManager.pInstance.CanOpenMysteryBox());
			}
		}
	}

	private void LoadJobBoard()
	{
		if (!string.IsNullOrEmpty(_AssetName))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
		}
	}

	protected override void UpdateState()
	{
		if (SnoggletogManager.pInstance != null)
		{
			mRedeemReady = SnoggletogManager.pInstance.GetRewardsState();
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Object.Instantiate((GameObject)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			break;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		CancelInvoke("LootBoxCheck");
	}
}
