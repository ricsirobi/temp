public class ObCollectHealth : ObCollect
{
	public int _Health = 1;

	public void OnSetHealthMessageOnly(bool flag)
	{
		mSendMessageOnly = flag;
	}
}
