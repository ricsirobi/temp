using System;
using System.Xml.Serialization;
using UnityEngine;

public class LabAdditionObjectData
{
	public enum Status
	{
		NOT_STARTED,
		IN_PROGRESSS,
		COMPLETED
	}

	public delegate void Callback(LabAdditionObjectData inObjectData);

	[XmlElement(ElementName = "Addition")]
	public string DefaultAdditionObject;

	[XmlElement(ElementName = "UsePosition")]
	public bool UsePosition;

	[XmlElement(ElementName = "PosX")]
	public float PosX;

	[XmlElement(ElementName = "PosY")]
	public float PosY;

	[XmlElement(ElementName = "PosZ")]
	public float PosZ;

	[XmlIgnore]
	public Callback mCallback;

	private Status mStatus;

	[XmlIgnore]
	public GameObject pAdditionObject { get; set; }

	[XmlIgnore]
	public Vector3 pPosition { get; set; }

	[XmlIgnore]
	public bool pSuccess => mStatus == Status.COMPLETED;

	public void Initialize(Callback inCallback)
	{
		mCallback = (Callback)Delegate.Combine(mCallback, inCallback);
		switch (mStatus)
		{
		case Status.COMPLETED:
			mCallback(this);
			break;
		case Status.NOT_STARTED:
		{
			mStatus = Status.IN_PROGRESSS;
			string[] array = DefaultAdditionObject.Split('/');
			if (array.Length == 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], DefaultAdditionLoaded, typeof(GameObject));
			}
			break;
		}
		case Status.IN_PROGRESSS:
			break;
		}
	}

	private void DefaultAdditionLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mStatus = Status.COMPLETED;
			if (inObject == null)
			{
				mCallback(this);
				break;
			}
			pAdditionObject = inObject as GameObject;
			if (pAdditionObject != null)
			{
				pPosition = new Vector3(PosX, PosY, PosZ);
			}
			mCallback(this);
			break;
		case RsResourceLoadEvent.ERROR:
			mCallback(this);
			break;
		}
	}
}
