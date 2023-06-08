using UnityEngine;

public class UiMyRoomBuilderCategoryMenu : KAUIMenu
{
	private UiMyRoomBuilder mUiMyRoomBuilder;

	private int mSelectedItemIndex = -1;

	public int pSelectedItemIndex
	{
		get
		{
			return mSelectedItemIndex;
		}
		set
		{
			mSelectedItemIndex = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		mUiMyRoomBuilder = (UiMyRoomBuilder)_ParentUi;
		if (mUiMyRoomBuilder == null)
		{
			Debug.LogError("Wrong parentUi type assigned to KAUIMyRoomBuilderCategoryMenu - it must be of type KAUIMyRoomBuilder");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnClick(inWidget);
			int index = 0;
			if (mUiMyRoomBuilder != null)
			{
				mUiMyRoomBuilder.CategorySelected(inWidget, index);
			}
			string value = "RoomBuilderCategoryMenu_Selected_" + inWidget.name;
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", value);
			}
		}
	}

	public override void SetSelectedItem(KAWidget inWidget)
	{
		base.SetSelectedItem(inWidget);
		pSelectedItemIndex = GetSelectedItemIndex();
	}
}
