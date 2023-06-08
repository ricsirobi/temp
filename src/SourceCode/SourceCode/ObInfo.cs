using UnityEngine;

public class ObInfo : MonoBehaviour
{
	public ObViewInfo[] _ViewInfo;

	public bool ApplyViewInfo(string vname)
	{
		ObViewInfo[] viewInfo = _ViewInfo;
		foreach (ObViewInfo obViewInfo in viewInfo)
		{
			if (obViewInfo._Name == vname)
			{
				obViewInfo.ApplyViewInfo(base.transform);
				return true;
			}
		}
		return false;
	}

	public void ApplyViewInfo()
	{
		if (_ViewInfo.Length != 0)
		{
			_ViewInfo[0].ApplyViewInfo(base.transform);
		}
	}

	public void ApplyViewInfo(int idx)
	{
		if (_ViewInfo != null && idx < _ViewInfo.Length)
		{
			_ViewInfo[idx].ApplyViewInfo(base.transform);
		}
	}
}
