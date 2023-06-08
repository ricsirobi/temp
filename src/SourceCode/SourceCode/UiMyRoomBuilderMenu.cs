using System;
using UnityEngine;

public class UiMyRoomBuilderMenu : KAUIMenu
{
	public Texture2D _LockedIcon;

	public AudioClip _LockVO;

	public bool _ShowItemID = true;

	public GUISkin _QuanSkin;

	[NonSerialized]
	public bool mNeedPageCheck = true;

	private UiMyRoomBuilder mUiMyRoomBuilder;

	protected override void Start()
	{
		base.Start();
		mUiMyRoomBuilder = (UiMyRoomBuilder)_ParentUi;
		if (mUiMyRoomBuilder == null)
		{
			Debug.LogError("Wrong parentUi type assigned to KAUIMyRoomBuilderMenu - it must be of type KAUIMyRoomBuilder");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!KAInput.pInstance.IsTouchInput() && !(inWidget == null))
		{
			base.OnClick(inWidget);
			int index = 0;
			mUiMyRoomBuilder.Selected(inWidget, index);
			mUiMyRoomBuilder.CheckForDecorateTaskCompletion();
		}
	}

	public override void LoadItem(KAWidget inWidget)
	{
		CoBundleItemData coBundleItemData = (CoBundleItemData)inWidget.GetUserData();
		if (coBundleItemData != null && coBundleItemData.IsNotLoaded())
		{
			coBundleItemData.LoadResource();
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta2)
	{
		base.OnDrag(inWidget, inDelta2);
		if (mUiMyRoomBuilder.pDragObject == null)
		{
			mUiMyRoomBuilder.Selected(inWidget, 0);
			mUiMyRoomBuilder.OnDragObject(isDragging: true);
		}
	}
}
