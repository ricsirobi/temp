public class UserCommonInventoryData
{
	private CommonInventoryData mInstance;

	private int mContainerID;

	public CommonInventoryData pInstance => mInstance;

	public bool pIsReady => mInstance != null;

	public void Init(int containerid, bool loadItemStats)
	{
		mContainerID = containerid;
		WsWebService.GetCommonInventoryData(containerid, loadItemStats, ServiceEventHandler, null);
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		CommonInventoryData.ProcessEvent(ref mInstance, inType, inEvent, inProgress, inObject, inUserData);
		if (mInstance != null)
		{
			mInstance.mContainerId = mContainerID;
		}
	}
}
