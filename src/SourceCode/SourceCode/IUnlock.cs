public interface IUnlock
{
	void AddUnlockInfo();

	void RemoveUnlockInfo();

	bool IsSceneUnlocked(string sceneName, bool inShowUi, UnlockManager.OnSceneUnlockedCallBack onSceneUnlocked);
}
