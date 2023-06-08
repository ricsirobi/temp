using System;
using System.Collections.Generic;
using UnityEngine;

public class UserNotifyWaitList : KAMonoBase
{
	[Serializable]
	public class UserNotifyData
	{
		public string _Description;

		public List<string> _Scenes;

		public List<GameObject> _NotifyObjects;
	}

	public List<UserNotifyData> _UserNotifyData;

	private void Awake()
	{
		UserNotifyData userNotifyData = _UserNotifyData.Find((UserNotifyData item) => item._Scenes.Contains(RsResourceManager.pCurrentLevel));
		if (userNotifyData == null)
		{
			userNotifyData = _UserNotifyData.Find((UserNotifyData item) => item._Scenes.Count == 0);
		}
		if (userNotifyData == null)
		{
			return;
		}
		GameObject gameObject = GameObject.Find("PfCommonLevel");
		if (!(gameObject != null))
		{
			return;
		}
		CoCommonLevel component = gameObject.GetComponent<CoCommonLevel>();
		if (component != null && userNotifyData._NotifyObjects != null)
		{
			if (component._WaitList != null)
			{
				userNotifyData._NotifyObjects.AddRange(component._WaitList);
			}
			component._WaitList = userNotifyData._NotifyObjects.ToArray();
			GameObject[] waitList = component._WaitList;
			for (int i = 0; i < waitList.Length; i++)
			{
				waitList[i].SetActive(value: true);
			}
		}
	}
}
