public interface IAdResult
{
	void OnAdWatched();

	void OnAdFailed();

	void OnAdSkipped();

	void OnAdClosed();

	void OnAdFinished(string eventDataRewardString);

	void OnAdCancelled();
}
