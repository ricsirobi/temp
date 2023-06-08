using UnityEngine;

public class CoPropLoader : MonoBehaviour
{
	public GameObject _MessageObject;

	public PropData[] _PropList;

	public InstanceData[] _InstanceMap;

	private int mCurIndex;

	private void OnLevelReady()
	{
		StartLoadingSequnce();
	}

	public GameObject FindInstance(string iname)
	{
		if (_InstanceMap != null)
		{
			InstanceData[] instanceMap = _InstanceMap;
			foreach (InstanceData instanceData in instanceMap)
			{
				if (iname == instanceData._Name)
				{
					return instanceData._Object;
				}
			}
		}
		return null;
	}

	private void StartLoadingSequnce()
	{
		mCurIndex = 0;
		if (_PropList.Length != 0)
		{
			_PropList[mCurIndex].LoadProp(this);
		}
		else if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnPropLoaderDone", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void PropReady()
	{
		mCurIndex++;
		if (mCurIndex == _PropList.Length)
		{
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnPropLoaderDone", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			_PropList[mCurIndex].LoadProp(this);
		}
	}
}
