public class UiStableQuestLogMenu : KAUIMenu
{
	public string _LogPrefixSymbol = string.Empty;

	private int mLogsLength;

	public int pLogsLength => mLogsLength;

	public void PopulateItems(int slotID)
	{
		ClearItems();
		LocaleString[] missionLogs = TimedMissionManager.pInstance.GetMissionLogs(slotID);
		mLogsLength = missionLogs.Length;
		for (int i = 0; i < missionLogs.Length; i++)
		{
			KAWidget kAWidget = AddWidget(_Template.name, null);
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.SetText(_LogPrefixSymbol + missionLogs[i].GetLocalizedString());
		}
	}

	public void ShowLogsTillIndex(int maxLogIndex)
	{
		for (int i = 0; i < GetItemCount(); i++)
		{
			if (i <= maxLogIndex)
			{
				if (!GetItemAt(i).GetVisibility())
				{
					GetItemAt(i).SetVisibility(inVisible: true);
				}
			}
			else if (GetItemAt(i).GetVisibility())
			{
				GetItemAt(i).SetVisibility(inVisible: false);
			}
		}
	}
}
