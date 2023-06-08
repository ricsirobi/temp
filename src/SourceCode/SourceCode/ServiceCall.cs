using System;

public class ServiceCall<DATA_TYPE>
{
	public delegate object ProcessDataCallback(string inData);

	public interface IProvider
	{
		ServiceCall<DATA_TYPE> pParent { get; set; }

		void ProcessGet();

		void ProcessSet();
	}

	private IProvider mProvider;

	private ServiceRequest mServiceRequest;

	private ProcessDataCallback mProcessCallback;

	public ServiceRequest pServiceRequest => mServiceRequest;

	public virtual void PostprocessCall(string inData)
	{
		if (mServiceRequest._EventDelegate != null)
		{
			object inObject = null;
			if (mProcessCallback != null)
			{
				inObject = mProcessCallback(inData);
			}
			else if (!string.IsNullOrEmpty(inData))
			{
				inObject = UtUtilities.DeserializeFromXml(inData, typeof(DATA_TYPE));
			}
			mServiceRequest._EventDelegate(mServiceRequest._Type, WsServiceEvent.COMPLETE, 1f, inObject, mServiceRequest._UserData);
		}
	}

	public object ProcessDecrypt(string inData)
	{
		return UtUtilities.DeserializeFromXml(TripleDES.DecryptUnicode(UtUtilities.DeserializeFromXml(inData, typeof(string)) as string, WsWebService.pSecret), typeof(DATA_TYPE));
	}

	public object ProcessChildData(string inData)
	{
		string ciphertext = UtUtilities.DeserializeFromXml(inData, typeof(string)) as string;
		ciphertext = TripleDES.DecryptUnicode(ciphertext, WsWebService.pSecret);
		UtDebug.Log("*****ProcessChildData\n" + ciphertext);
		return ciphertext;
	}

	public object ProcessNoResult(string inData)
	{
		return null;
	}

	public virtual void DoGet()
	{
		if (mProvider != null)
		{
			mProvider.ProcessGet();
		}
		else
		{
			OnEvent(null, UtAsyncEvent.ERROR);
		}
	}

	public virtual void DoSet()
	{
		if (mProvider != null)
		{
			mProvider.ProcessSet();
		}
		else
		{
			OnEvent(null, UtAsyncEvent.ERROR);
		}
	}

	public virtual void Process()
	{
		if (mServiceRequest._Type.ToString().StartsWith("GET", StringComparison.OrdinalIgnoreCase))
		{
			if (mProvider != null)
			{
				mProvider.ProcessGet();
			}
		}
		else if (mServiceRequest._Type.ToString().StartsWith("SET", StringComparison.OrdinalIgnoreCase) && mProvider != null)
		{
			mProvider.ProcessSet();
		}
	}

	public void OnEvent(UtIWWWAsync inFile, UtAsyncEvent inEvent)
	{
		switch (inEvent)
		{
		case UtAsyncEvent.COMPLETE:
			if (inFile != null)
			{
				PostprocessCall(inFile.pData);
			}
			break;
		case UtAsyncEvent.PROGRESS:
			if (mServiceRequest._EventDelegate != null)
			{
				mServiceRequest._EventDelegate(mServiceRequest._Type, WsServiceEvent.PROGRESS, inFile.pProgress, null, mServiceRequest._UserData);
			}
			break;
		case UtAsyncEvent.ERROR:
			if (mServiceRequest._EventDelegate != null)
			{
				mServiceRequest._EventDelegate(mServiceRequest._Type, WsServiceEvent.ERROR, 0f, null, mServiceRequest._UserData);
			}
			break;
		}
	}

	public static ServiceCall<DATA_TYPE> Create(ServiceRequest inRequest, ServiceCallType inType = ServiceCallType.NONE)
	{
		if (ServiceRequest.pBlockRequest)
		{
			UtDebug.Log("Service Request Blocked !!!");
			return null;
		}
		ServiceCall<DATA_TYPE> serviceCall = new ServiceCall<DATA_TYPE>();
		serviceCall.mServiceRequest = inRequest;
		serviceCall.mProvider = new OnlineWebProvider<DATA_TYPE>();
		switch (inType)
		{
		case ServiceCallType.DECRYPT:
			serviceCall.mProcessCallback = serviceCall.ProcessDecrypt;
			break;
		case ServiceCallType.CHILDDATA:
			serviceCall.mProcessCallback = serviceCall.ProcessChildData;
			break;
		case ServiceCallType.NORESULT:
			serviceCall.mProcessCallback = serviceCall.ProcessNoResult;
			break;
		}
		serviceCall.mProvider.pParent = serviceCall;
		return serviceCall;
	}

	public object GetValue(string inKey)
	{
		return pServiceRequest.GetValue(inKey);
	}
}
