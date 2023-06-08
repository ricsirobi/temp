using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "SceneData", Namespace = "", IsNullable = true)]
public class SceneData
{
	public class InstanceInfo
	{
		public SceneData mInstance;

		public string mSceneName;

		public List<BuildSpot> mBuildSpots;

		public List<SceneModule> mModules;

		public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
		{
			if (inType != WsServiceType.GET_SCENE)
			{
				return;
			}
			switch (inEvent)
			{
			case WsServiceEvent.COMPLETE:
				mInstance = (SceneData)inObject;
				if (mInstance != null)
				{
					mBuildSpots = new List<BuildSpot>(mInstance.BuildSpots);
					mModules = new List<SceneModule>(mInstance.Modules);
				}
				else
				{
					InitDefault();
				}
				break;
			case WsServiceEvent.ERROR:
				UtDebug.LogError("WEB SERVICE CALL GetSceneData FAILED!!!");
				InitDefault();
				break;
			}
		}

		public void InitDefault()
		{
			mInstance = new SceneData();
			mBuildSpots = new List<BuildSpot>();
			mModules = new List<SceneModule>();
		}
	}

	[XmlArrayItem("BuildSpot", IsNullable = false)]
	public BuildSpot[] BuildSpots;

	[XmlArrayItem("Module", IsNullable = false)]
	public SceneModule[] Modules;

	private static InstanceInfo mInstanceInfo = new InstanceInfo();

	public static SceneData pInstance
	{
		get
		{
			return mInstanceInfo.mInstance;
		}
		set
		{
			mInstanceInfo.mInstance = value;
		}
	}

	public static bool pIsReady => mInstanceInfo.mInstance != null;

	public static void Init(string inSceneName)
	{
		mInstanceInfo.mInstance = null;
		mInstanceInfo.mSceneName = inSceneName;
		WsWebService.GetSceneData(inSceneName, mInstanceInfo.ServiceEventHandler, null);
	}

	public static void Init(string inSceneName, string inUserName)
	{
		mInstanceInfo.mInstance = null;
		mInstanceInfo.mSceneName = inSceneName;
		WsWebService.GetSceneDataByUserID(inUserName, inSceneName, mInstanceInfo.ServiceEventHandler, null);
	}

	public static void Load(InstanceInfo inInfo, string inSceneName)
	{
		inInfo.mSceneName = inSceneName;
		WsWebService.GetSceneData(inSceneName, inInfo.ServiceEventHandler, null);
	}

	public static void Save()
	{
		if (pIsReady)
		{
			Save(mInstanceInfo);
		}
	}

	public static void Save(InstanceInfo inInfo)
	{
		inInfo.mInstance.BuildSpots = inInfo.mBuildSpots.ToArray();
		inInfo.mInstance.Modules = inInfo.mModules.ToArray();
		WsWebService.SetSceneData(inInfo.mSceneName, inInfo.mInstance, inInfo.ServiceEventHandler, null);
	}

	public static void AddBuildSpot(string inSpotName, string inObjectName)
	{
		if (mInstanceInfo.mBuildSpots == null)
		{
			return;
		}
		for (int i = 0; i < mInstanceInfo.mBuildSpots.Count; i++)
		{
			if (mInstanceInfo.mBuildSpots[i].Name == inSpotName)
			{
				mInstanceInfo.mBuildSpots[i].Item = inObjectName;
				return;
			}
		}
		BuildSpot buildSpot = new BuildSpot();
		buildSpot.Name = inSpotName;
		buildSpot.Item = inObjectName;
		mInstanceInfo.mBuildSpots.Add(buildSpot);
	}

	public static void RemoveBuildSpot(string inSpotName)
	{
		if (mInstanceInfo.mBuildSpots == null)
		{
			return;
		}
		for (int i = 0; i < mInstanceInfo.mBuildSpots.Count; i++)
		{
			if (mInstanceInfo.mBuildSpots[i].Name == inSpotName)
			{
				mInstanceInfo.mBuildSpots.RemoveAt(i);
				break;
			}
		}
	}

	public static string GetBuildObject(string inSpotName)
	{
		if (mInstanceInfo.mBuildSpots != null)
		{
			for (int i = 0; i < mInstanceInfo.mBuildSpots.Count; i++)
			{
				if (mInstanceInfo.mBuildSpots[i].Name == inSpotName)
				{
					return mInstanceInfo.mBuildSpots[i].Item;
				}
			}
		}
		return null;
	}

	public static int AddGoldStar(string inModuleName)
	{
		return AddGoldStar(mInstanceInfo, inModuleName);
	}

	public static int AddGoldStar(InstanceInfo inInfo, string inModuleName)
	{
		if (inInfo.mModules != null && inModuleName.Length > 0)
		{
			for (int i = 0; i < inInfo.mModules.Count; i++)
			{
				if (inInfo.mModules[i].Name == inModuleName)
				{
					inInfo.mModules[i].Stars++;
					Save(inInfo);
					return inInfo.mModules[i].Stars;
				}
			}
			SceneModule sceneModule = new SceneModule();
			sceneModule.Name = inModuleName;
			sceneModule.Stars = 1;
			inInfo.mModules.Add(sceneModule);
			Save(inInfo);
			return 1;
		}
		return 0;
	}

	public static int GetGoldStars(string inModuleName)
	{
		return GetGoldStars(mInstanceInfo, inModuleName);
	}

	public static int GetGoldStars(InstanceInfo inInfo, string inModuleName)
	{
		if (inInfo.mModules != null)
		{
			for (int i = 0; i < inInfo.mModules.Count; i++)
			{
				if (inInfo.mModules[i].Name == inModuleName)
				{
					return inInfo.mModules[i].Stars;
				}
			}
		}
		return 0;
	}
}
