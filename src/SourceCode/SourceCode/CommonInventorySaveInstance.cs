public class CommonInventorySaveInstance
{
	public InventorySaveEventHandler mSaveCallback;

	public object mSaveUserData;

	public int mLoadItemCount;

	public CommonInventorySaveInstance(InventorySaveEventHandler callback, object userData)
	{
		mSaveCallback = callback;
		mSaveUserData = userData;
	}
}
