using System;

public class PairDataInstance
{
	public int _DataID = -1;

	public PairData _Data;

	public bool _DataReady;

	public PairDataEventHandler mLoadCallback;

	public PairDataEventHandler mSaveCallback;

	public object mUserData;

	private bool mLoadInProgress;

	private bool mSaveInProgress;

	public bool pIsLoading => mLoadInProgress;

	public bool pIsSaving => mSaveInProgress;

	public void WsGetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL GetPairData FAILED!!!");
			_Data = null;
			OnDataLoaded(success: false);
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				_Data = (PairData)inObject;
				_Data.Init();
				_Data._DataID = _DataID;
			}
			else
			{
				_Data = new PairData();
				_Data._DataID = _DataID;
				_Data.Init();
			}
			OnDataLoaded(success: true);
			break;
		}
	}

	public void WsSetEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("-----WEB SERVICE CALL SetPairData FAILED!!!");
			OnDataSave(success: false, inUserData);
			break;
		case WsServiceEvent.COMPLETE:
			_Data._IsDirty = false;
			OnDataSave(success: true, inUserData);
			break;
		}
	}

	public void Load(int dataID, PairDataEventHandler callback, object inUserData, string userID = null)
	{
		_DataReady = false;
		_DataID = dataID;
		mLoadCallback = (PairDataEventHandler)Delegate.Combine(mLoadCallback, callback);
		mUserData = inUserData;
		if (_Data != null)
		{
			OnDataLoaded(success: true);
		}
		else if (!mLoadInProgress)
		{
			if (string.IsNullOrEmpty(userID))
			{
				WsWebService.GetKeyValuePair(_DataID, WsGetEventHandler, null);
			}
			else
			{
				WsWebService.GetKeyValuePairByUserID(userID, _DataID, WsGetEventHandler, null);
			}
			mLoadInProgress = true;
		}
	}

	public void Save(string userID = null, PairDataEventHandler inCallback = null, object inUserData = null)
	{
		mSaveCallback = (PairDataEventHandler)Delegate.Combine(mSaveCallback, inCallback);
		if (_Data != null && _Data._IsDirty)
		{
			_Data.PrepareArray();
			if (!mSaveInProgress)
			{
				if (string.IsNullOrEmpty(userID))
				{
					WsWebService.SetKeyValuePair(_DataID, _Data, WsSetEventHandler, inUserData);
				}
				else
				{
					WsWebService.SetKeyValuePairByUserID(userID, _DataID, _Data, WsSetEventHandler, inUserData);
				}
				mSaveInProgress = true;
			}
			else
			{
				OnDataSave(success: false, inUserData);
			}
		}
		else
		{
			OnDataSave(success: false, inUserData);
		}
	}

	private void OnDataSave(bool success, object inUserData)
	{
		mSaveInProgress = false;
		if (mSaveCallback != null)
		{
			mSaveCallback(success, _Data, inUserData);
		}
		mSaveCallback = null;
	}

	public void OnDataLoaded(bool success)
	{
		_DataReady = true;
		mLoadInProgress = false;
		if (mLoadCallback != null)
		{
			mLoadCallback(success, _Data, mUserData);
		}
		mLoadCallback = null;
	}
}
