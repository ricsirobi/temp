using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
[XmlRoot(ElementName = "GameDataConfig", Namespace = "")]
public class GameDataConfig
{
	[XmlElement]
	public MMOLevelData MMOLevelData;

	[XmlElement]
	public OptimizationData OptimizationData;

	[XmlElement(ElementName = "TerrainSettings")]
	public List<TerrainSettings> TerrainSettings;

	[XmlElement(ElementName = "GuestEnabled")]
	public bool GuestEnabled;

	[XmlElement(ElementName = "UiMemoryData")]
	public UiMemoryData[] UiMemoryData;

	[XmlElement(ElementName = "ShowLowMemoryWarning")]
	public bool ShowLowMemoryWarning;

	[XmlElement(ElementName = "ServerErrorMessages")]
	public ServerErrorMessages ServerErrorMessages;

	[XmlElement(ElementName = "ReloadOnMinimizeScenes")]
	public ReloadOnMinimizeScenes ReloadOnMinimizeScenes = new ReloadOnMinimizeScenes();

	[XmlElement]
	public AvatarHatsData AvatarHatsData;

	[XmlElement(ElementName = "AvatarHeadData")]
	public AvatarHeadData AvatarHeadData;

	public int MembershipRenewalWarningDays;

	public int TrialMembershipRenewalWarningDays;

	public bool ShowTaskSyncDBOnError = true;

	public bool ShowTaskSyncDBOnFail;

	private static GameDataConfig mInstance;

	public static GameDataConfig pInstance => mInstance;

	public static bool pIsReady => mInstance != null;

	public static void Init()
	{
		if (mInstance == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("GameDataConfigFile"), XmlLoadEventHandler);
		}
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			using (StringReader textReader = new StringReader((string)inObject))
			{
				try
				{
					mInstance = (GameDataConfig)new XmlSerializer(typeof(GameDataConfig)).Deserialize(textReader);
					return;
				}
				catch (Exception ex)
				{
					Debug.LogError("XML error : " + ex);
					mInstance = new GameDataConfig();
					return;
				}
			}
		}
		if (inEvent.Equals(RsResourceLoadEvent.ERROR))
		{
			Debug.LogError("Failed to load file : " + inURL);
			mInstance = new GameDataConfig();
		}
	}

	public static OptimizationData.LevelData GetLevelData(string sceneName)
	{
		if (pInstance?.OptimizationData?.SpecificLevelData == null)
		{
			return null;
		}
		OptimizationData.LevelData levelData = Array.Find(pInstance?.OptimizationData?.SpecificLevelData, (OptimizationData.LevelData t) => t.SceneName.ToLower() == sceneName.ToLower());
		if (levelData != null)
		{
			return levelData;
		}
		return pInstance?.OptimizationData?.GlobalLevelData;
	}

	public static int GetMinMemory(UiType inUiType)
	{
		if (mInstance != null && mInstance.UiMemoryData != null)
		{
			UiMemoryData[] uiMemoryData = mInstance.UiMemoryData;
			foreach (UiMemoryData uiMemoryData2 in uiMemoryData)
			{
				if (uiMemoryData2.UiType == inUiType)
				{
					return uiMemoryData2.MinMemory;
				}
			}
		}
		return 9999;
	}

	public static bool CanReloadOnMinimize()
	{
		if (pInstance?.ReloadOnMinimizeScenes?.SceneNames == null)
		{
			return false;
		}
		return Array.IndexOf(pInstance?.ReloadOnMinimizeScenes?.SceneNames, SceneManager.GetActiveScene().name) >= 0;
	}

	public static void OptimizeTerrain(int qualityLevel)
	{
		TerrainSettings terrainSettings = pInstance?.TerrainSettings?.Find((TerrainSettings setting) => setting.BundleType.Contains(ProductConfig.GetBundleQuality()));
		if (terrainSettings == null || terrainSettings.TerrainSetting == null)
		{
			UtDebug.Log("TerrainSettings are not available in GameDataConfig.xml for " + ProductConfig.GetBundleQuality());
			terrainSettings = pInstance?.TerrainSettings?.Find((TerrainSettings setting) => setting.BundleType.Contains("default"));
		}
		if (terrainSettings == null || terrainSettings.TerrainSetting == null)
		{
			UtDebug.Log("Default terrainSettings are also not available in GameDataConfig.xml");
			return;
		}
		string qualityName = QualitySettings.names[qualityLevel];
		TerrainSetting terrainSetting = terrainSettings.TerrainSetting.Find((TerrainSetting data) => data.Name.Equals(qualityName));
		Terrain[] activeTerrains = Terrain.activeTerrains;
		if (activeTerrains != null && terrainSetting != null)
		{
			Terrain[] array = activeTerrains;
			foreach (Terrain obj in array)
			{
				obj.basemapDistance = terrainSetting.BaseMapDistance;
				obj.heightmapPixelError = terrainSetting.HeightmapPixelError;
				obj.detailObjectDistance = terrainSetting.DetailObjectDistance;
				obj.detailObjectDensity = terrainSetting.DetailObjectDensity;
				obj.treeDistance = terrainSetting.TreeDistance;
				obj.treeBillboardDistance = terrainSetting.TreeBillboardDistance;
				obj.treeCrossFadeLength = terrainSetting.TreeCrossFadeLength;
				obj.treeMaximumFullLODCount = terrainSetting.TreeMaximumFullLODCount;
			}
		}
	}
}
