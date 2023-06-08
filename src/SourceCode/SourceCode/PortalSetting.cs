using System;
using System.Collections.Generic;
using UnityEngine;

public class PortalSetting : ScriptableObject
{
	[Serializable]
	public class PortalData
	{
		public string _DefaultPortal = "PfDragonTaxiSelectTrigger";

		public List<SceneData> _SceneMap = new List<SceneData>();
	}

	[Serializable]
	public class SceneData
	{
		public string _FromScene = "";

		[Header("Overrides Default Portal")]
		public string _Portal = "";

		public List<ScenePortal> PortalInfo = new List<ScenePortal>();

		public SceneData()
		{
		}

		public SceneData(string name)
		{
			_FromScene = name;
		}
	}

	[Serializable]
	public class ScenePortal
	{
		[Header("Portal in scene")]
		public string _Portal = "";

		[Header("Connected scenes")]
		public List<string> _ToScenes = new List<string>();
	}

	public PortalData _PortalData;

	private const string mNullPortal = "NULL";

	private const string mEmptyPortal = "NONE";

	public string FindPortalToScene(string sceneFrom, string sceneTo)
	{
		if (string.IsNullOrEmpty(sceneTo))
		{
			return null;
		}
		if (sceneFrom.Equals(sceneTo))
		{
			return null;
		}
		string text = _PortalData._DefaultPortal;
		if (_PortalData != null && _PortalData._SceneMap != null && _PortalData._SceneMap.Count > 0)
		{
			SceneData sceneData = _PortalData._SceneMap.Find((SceneData x) => x._FromScene == sceneFrom);
			if (sceneData != null)
			{
				ScenePortal scenePortal = sceneData.PortalInfo.Find((ScenePortal s) => s._ToScenes.Contains(sceneTo));
				if (scenePortal != null)
				{
					text = scenePortal._Portal;
				}
				else if (!string.IsNullOrEmpty(sceneData._Portal))
				{
					text = sceneData._Portal;
				}
			}
		}
		if (!string.IsNullOrEmpty(text) && text.Equals("NONE", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return text;
	}

	public HashSet<string> GetAllPortalsInScene(string scene)
	{
		HashSet<string> hashSet = new HashSet<string>();
		hashSet.Add(_PortalData._DefaultPortal);
		if (_PortalData != null && _PortalData._SceneMap != null && _PortalData._SceneMap.Count > 0)
		{
			SceneData sceneData = _PortalData._SceneMap.Find((SceneData x) => x._FromScene.Equals(scene));
			if (sceneData != null)
			{
				foreach (ScenePortal item in sceneData.PortalInfo)
				{
					hashSet.Add(item._Portal);
				}
				if (!string.IsNullOrEmpty(sceneData._Portal))
				{
					hashSet.Add(sceneData._Portal);
				}
			}
		}
		return hashSet;
	}
}
