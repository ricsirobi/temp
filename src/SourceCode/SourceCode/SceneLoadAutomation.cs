using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SceneLoadAutomation : KAMonoBase
{
	private List<SceneDataContainer> mSceneDataList = new List<SceneDataContainer>();

	private int mSceneCounter;

	private readonly string mFirstSceneName = "ProfileSelectionDO";

	private StreamWriter mOutputStream;

	private DateTime mStartTime;

	private int mLanguageIndex;

	private int mQualityIndex;

	private string mXMLPath = "RS_DATA/AutoLoadSceneDataDO.xml";

	private BundlesData mBundleData;

	private const float mSceneDelayTime = 1f;

	private static SceneLoadAutomation mInstance;

	private List<string> mTextureQualityList = new List<string> { "Low", "Mid", "High" };

	public bool pRunThroughAllLangauages { get; set; }

	public bool pRunThroughAllTextures { get; set; }

	public static SceneLoadAutomation pInstance
	{
		get
		{
			if (mInstance == null)
			{
				GameObject obj = new GameObject();
				mInstance = obj.AddComponent<SceneLoadAutomation>();
				obj.name = "(singleton) SceneLoadAutomation";
			}
			return mInstance;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		mOutputStream = new StreamWriter(Application.persistentDataPath + "/log.txt", append: false);
		RsResourceManager.Load(mXMLPath, XMLDownloaded);
		string currentQuality = mTextureQualityList.Find((string x) => x == ProductConfig.GetBundleQuality());
		int index = mTextureQualityList.FindIndex((string x) => x == currentQuality);
		mTextureQualityList.RemoveAt(index);
		mTextureQualityList.Insert(0, currentQuality);
	}

	private void OnEnable()
	{
		RsResourceManager.LoadLevelCompleted += OnLoadLevelCompleted;
	}

	private void OnDisable()
	{
		RsResourceManager.LoadLevelCompleted -= OnLoadLevelCompleted;
	}

	private void XMLDownloaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UtDebug.Log("XML Downloaded");
			mBundleData = UtUtilities.DeserializeFromXml<BundlesData>((string)inFile);
			if (mBundleData != null)
			{
				ParseXML();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("Failed to Load AUTO LOAD SCENEDATA XML");
			break;
		}
	}

	private void ParseXML()
	{
		BundlesData.Category[] categories = mBundleData.Categories;
		foreach (BundlesData.Category category in categories)
		{
			if (!(category.Name == "Scenes"))
			{
				continue;
			}
			foreach (BundlesData.Category.Bundle b in category.Bundles)
			{
				if (!mSceneDataList.Exists((SceneDataContainer x) => x._SceneName == b.Value))
				{
					mSceneDataList.Add(new SceneDataContainer(b.Value));
				}
			}
			mSceneDataList.ForEach(delegate(SceneDataContainer x)
			{
				x._SceneName = x._SceneName.Substring(x._SceneName.LastIndexOf("/") + 1, x._SceneName.LastIndexOf(".") - (x._SceneName.LastIndexOf("/") + 1));
			});
		}
		mSceneDataList = mSceneDataList.Where((SceneDataContainer x) => !string.IsNullOrEmpty(x._SceneName)).ToList();
	}

	public void StartAutomation()
	{
		StartCoroutine(StartAutomationIE());
	}

	private IEnumerator StartAutomationIE()
	{
		while (mBundleData == null)
		{
			yield return new WaitForSeconds(1f);
		}
		mStartTime = DateTime.Now;
		Trace("[*****] Start of File [*****]" + mStartTime);
		Trace("");
		Trace("Texture Quality: " + ProductConfig.GetBundleQuality());
		LoadAllScenes();
	}

	private void LoadAllScenes()
	{
		OnTextureQualityChanged();
		ChangeLanguage();
		LoadScene();
	}

	private void OnTextureQualityChanged()
	{
		if (!ProductConfig.GetBundleQuality().Equals(mTextureQualityList[mQualityIndex]))
		{
			ProductConfig.SetBundleQuality(mTextureQualityList[mQualityIndex]);
			Trace("");
			Trace("Texture Quality: " + ProductConfig.GetBundleQuality());
		}
	}

	private void ChangeLanguage()
	{
		if (mLanguageIndex < ProductConfig.pInstance.Locale.Length)
		{
			UtUtilities.SetLocaleLanguage(ProductConfig.pInstance.Locale[mLanguageIndex].ID);
			Trace("Language: " + UtUtilities.GetLocaleLanguage());
			Trace("");
		}
	}

	private void LoadScene()
	{
		if (mSceneDataList.Count > mSceneCounter)
		{
			Trace("Loading " + mSceneDataList[mSceneCounter]._SceneName + " Start Time: " + DateTime.Now);
			StartCoroutine("SceneTimeOut");
			RsResourceManager.LoadLevel(mSceneDataList[mSceneCounter]._SceneName);
		}
		else if (pRunThroughAllLangauages && mLanguageIndex + 1 < ProductConfig.pInstance.Locale.Length)
		{
			SetNextLanguage();
		}
		else if (pRunThroughAllTextures && mQualityIndex + 1 < mTextureQualityList.Count)
		{
			SetNextQualityLevel();
		}
		else
		{
			EndReport();
		}
	}

	private void SetNextLanguage()
	{
		mSceneCounter = 0;
		mLanguageIndex++;
		LoadAllScenes();
	}

	private void SetNextQualityLevel()
	{
		mSceneCounter = 0;
		mLanguageIndex = 0;
		mQualityIndex++;
		LoadAllScenes();
	}

	private void EndReport()
	{
		DateTime now = DateTime.Now;
		Trace("");
		Trace("[*****] End of File [*****]" + now);
		Trace("Generation Time: " + now.Subtract(mStartTime));
		mOutputStream.Dispose();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void OnLoadLevelCompleted(string inLevelName)
	{
		if (inLevelName != mFirstSceneName && inLevelName != "Transition")
		{
			Trace("Loaded " + inLevelName + " Complete Time: " + DateTime.Now);
			mSceneCounter++;
			StopCoroutine("SceneTimeOut");
			StartCoroutine(LoadDelayScene(1f));
		}
	}

	private void Trace(string message)
	{
		if (mOutputStream != null && message != null)
		{
			Debug.Log(message);
			mOutputStream.WriteLine(message);
			mOutputStream.Flush();
		}
	}

	private IEnumerator LoadDelayScene(float delay)
	{
		yield return new WaitForSeconds(delay);
		LoadScene();
	}

	private IEnumerator SceneTimeOut()
	{
		yield return new WaitForSeconds(float.Parse(mBundleData.SceneTimeOut));
		Trace("Scene was not loaded in given duration hence, we are trying to load next scene");
		RsResourceManager.pLevelToLoad = "";
		mSceneCounter++;
		LoadScene();
	}

	private void OnApplicationPause(bool pause)
	{
		Trace("Application Pause: " + pause);
	}

	private void OnApplicationFocus(bool focus)
	{
		Trace("Application Focus: " + focus);
	}

	private void OnApplicationQuit()
	{
		Trace("OnApplicationQuit");
	}
}
