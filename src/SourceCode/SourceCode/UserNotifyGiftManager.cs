public class UserNotifyGiftManager : UserNotify
{
	public override void OnWaitBeginImpl()
	{
		if (GiftManager.pInstance != null)
		{
			GiftManager.pInstance.Init(this);
		}
	}

	public void OnValidationComplete()
	{
		base.OnWaitEnd();
	}
}
