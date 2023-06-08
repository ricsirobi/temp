using System.Collections.Generic;
using UnityEngine;

internal class OnlineWebProvider<DATA_TYPE> : ServiceCall<DATA_TYPE>.IProvider
{
	private ServiceCall<DATA_TYPE> mParent;

	public ServiceCall<DATA_TYPE> pParent
	{
		get
		{
			return mParent;
		}
		set
		{
			mParent = value;
		}
	}

	public void ProcessGet()
	{
		DoCall();
	}

	public void ProcessSet()
	{
		DoCall();
	}

	public void DoCall()
	{
		WWWForm wWWForm = new WWWForm();
		if (mParent == null || mParent.pServiceRequest == null || mParent.pServiceRequest._Params == null)
		{
			Debug.LogError("Error!! Unable to post on WWW. Required data field is null");
			return;
		}
		foreach (KeyValuePair<string, object> param in mParent.pServiceRequest._Params)
		{
			if (param.Value != null)
			{
				wWWForm.AddField(param.Key, param.Value.ToString());
			}
		}
		UtWWWAsync.Post(mParent.pServiceRequest._URL, wWWForm, EventHandler, inSendProgressEvents: true);
	}

	public void EventHandler(UtAsyncEvent inEvent, UtIWWWAsync inFileReader)
	{
		switch (inEvent)
		{
		case UtAsyncEvent.PROGRESS:
			mParent.OnEvent(inFileReader, inEvent);
			break;
		case UtAsyncEvent.COMPLETE:
		{
			string inData = inFileReader.pData.ToString();
			mParent.PostprocessCall(inData);
			break;
		}
		case UtAsyncEvent.ERROR:
			mParent.OnEvent(inFileReader, inEvent);
			break;
		}
	}
}
