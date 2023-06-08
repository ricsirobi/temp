public interface IJournal
{
	void ActivateUI(int uiIndex, bool addToList = true);

	void Clear();

	void Exit();

	bool IsBusy();

	void ProcessClose();
}
