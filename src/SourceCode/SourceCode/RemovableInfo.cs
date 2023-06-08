using System;
using UnityEngine;

[Serializable]
public class RemovableInfo
{
	public RemovableType _Type = RemovableType.ROCK;

	private string mBundleName = "RS_DATA/Removables";

	private string mDefaultTree = "PfTree";

	private string mDefaultRock = "PfRock01";

	public string _AssetName;

	public int _ID;

	public int _RequiredFarmHouseLevel = 1;

	public int _CostToRemove = 10;

	public float _DestroyInSecs = 10f;

	public Vector3 _Position = Vector3.zero;

	public float yRotation;

	private string mDeserializeString = string.Empty;

	private FarmManager mFarmManager;

	public string pBundleName => mBundleName;

	public string pDeserializeString
	{
		get
		{
			return mDeserializeString;
		}
		set
		{
			mDeserializeString = value;
		}
	}

	public RemovableInfo(RemovableType inRemovableType, int inID, Vector3 inPosition)
	{
		_Type = inRemovableType;
		if (_Type == RemovableType.ROCK)
		{
			_AssetName = mDefaultRock;
		}
		else
		{
			_AssetName = mDefaultTree;
		}
		_ID = inID;
		_RequiredFarmHouseLevel = 1;
		_CostToRemove = 10;
		_DestroyInSecs = 10f;
		_Position = inPosition;
	}

	public void Load(FarmManager inFarmManager)
	{
		mFarmManager = inFarmManager;
		mBundleName = "RS_DATA/Removables";
		mDefaultTree = "PfTree";
		mDefaultRock = "PfRock01";
		if (_AssetName == null || _AssetName.Length == 0)
		{
			_AssetName = ((_Type == RemovableType.ROCK) ? mDefaultRock : mDefaultTree);
		}
		RsResourceManager.LoadAssetFromBundle(pBundleName, _AssetName, EventHandler, typeof(GameObject));
	}

	private void EventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mFarmManager.RemovablesHandler(_ID, inObject, this);
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("object is null");
			break;
		}
	}
}
