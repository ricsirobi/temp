public class UiMembershipExpiry : KAUIGenericDB
{
	private KAWidget mTxtWarningDays;

	private KAWidget mTxtWarningDate;

	public void SetUpDB(string daysWarning, string dateWarning)
	{
		mTxtWarningDays = FindItem("TxtWarningDays");
		if (mTxtWarningDays != null)
		{
			mTxtWarningDays.SetText(daysWarning);
		}
		mTxtWarningDate = FindItem("TxtWarningDate");
		if (mTxtWarningDate != null)
		{
			mTxtWarningDate.SetText(dateWarning);
		}
	}
}
