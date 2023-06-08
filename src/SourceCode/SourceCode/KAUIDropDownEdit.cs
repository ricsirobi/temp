public class KAUIDropDownEdit : KAUIDropDown
{
	private string mOldText = "";

	public override void OpenDropDown()
	{
		base.OpenDropDown();
		mSelectionItem.SetInteractive(isInteractive: true);
	}

	public override KAWidget GetClickedItem()
	{
		if (((KAEditBox)mSelectionItem).HasFocus())
		{
			return mSelectionItem;
		}
		return base.GetClickedItem();
	}

	public override void ProcessClickedSelection()
	{
		if (!mIsOpen && mSelectionItem.GetText().Length > 0)
		{
			OpenDropDown();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mSelectionItem == null)
		{
			return;
		}
		if (mIsOpen)
		{
			if (mSelectionItem.GetText().Length == 0)
			{
				mOldText = "";
				ProcessEditChanged(mOldText);
				CloseDropDown();
			}
			else if (mSelectionItem.GetText() != mOldText)
			{
				mOldText = mSelectionItem.GetText();
				ProcessEditChanged(mOldText);
			}
		}
		else if (((KAEditBox)mSelectionItem).HasFocus() && mSelectionItem.GetText().Length > 0)
		{
			OpenDropDown();
			mOldText = mSelectionItem.GetText();
			ProcessEditChanged(mOldText);
		}
	}

	public virtual void ProcessEditChanged(string text)
	{
	}
}
