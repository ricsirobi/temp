public class UiJobBoardCategoryMenu : KAUIMenu
{
	private UiJobBoard mParentUi;

	public void Init(UiJobBoard inParent)
	{
		mParentUi = inParent;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mParentUi != null)
		{
			JobBoardCategory jobCategoryFromName = mParentUi.GetJobCategoryFromName(inWidget.name);
			mParentUi.SetSelectedCategory(jobCategoryFromName);
		}
	}
}
