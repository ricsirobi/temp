using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiDebugInfo : KAUI
{
	public Color[] _Colors = new Color[5]
	{
		Color.red,
		Color.green,
		Color.blue,
		Color.white,
		Color.black
	};

	private KAWidget mTxtFPS;

	private KAWidget mTxtMemInfo;

	private int mColorIndex;

	public static UiDebugInfo Instance;

	private float mMBSize = 9.536743E-07f;

	private static bool mCreated;

	private int mStats_NumMeshes = -1;

	private int mStats_NumVertsTotal = -1;

	private bool mStats_InBudget = true;

	private int mBudget_NumVertsTotal = 800000;

	private float mLastUpdateTime;

	protected override void Start()
	{
		Application.backgroundLoadingPriority = ThreadPriority.Low;
		if (mCreated)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		Instance = this;
		base.Start();
		mTxtFPS = FindItem("TxtFPS");
		mTxtMemInfo = FindItem("TxtMemInfo");
		mCreated = true;
		Object.DontDestroyOnLoad(base.gameObject);
		UtMobileUtilities.AddToPersistentScriptList(this);
	}

	protected override void Update()
	{
		if (!GetVisibility())
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup - mLastUpdateTime < 1f)
		{
			return;
		}
		mLastUpdateTime = realtimeSinceStartup + Random.value * 0.1f;
		base.Update();
		mTxtFPS.SetText("FPS : " + GrFPS.pFrameRate.ToString("f2"));
		string text = "InUse : " + (float)UtMobileUtilities.GetMemoryInUse() * mMBSize;
		string bundleQuality = ProductConfig.GetBundleQuality();
		if (!string.IsNullOrEmpty(bundleQuality))
		{
			text = text + "\nBundles : " + bundleQuality;
		}
		if (MemoryManager.pInstance != null)
		{
			text = text + "\nMem Threshold : " + MemoryManager.pInstance.pMemoryThreshold;
		}
		if (MainStreetMMOClient.pInstance != null)
		{
			int num = 0;
			int num2 = 0;
			foreach (KeyValuePair<string, MMOAvatar> pPlayer in MainStreetMMOClient.pInstance.pPlayerList)
			{
				if (pPlayer.Value.pAvatarData.mInstance != null)
				{
					if (pPlayer.Value.pMMOAvatarType == MMOAvatarType.FULL)
					{
						num++;
					}
					else
					{
						num2++;
					}
				}
			}
			text += $"\nLite={num2}, Full={num}, All={num2 + num}, MinFPS={(int)MMOAvatarLite.FPSforMinFullAvatars}, Loading={MMOAvatarLite.LastOptimizationWasSkipped}";
		}
		text = text + "\nExcpt : " + LogConsole.pNumExceptions;
		text = ((mStats_NumMeshes >= 0) ? (text + string.Format("\n{0}: Meshes: {1}, Verts: {2}K, Budget: {3}K", mStats_InBudget ? "In Budget" : "OVER BUDGET", mStats_NumMeshes, mStats_NumVertsTotal / 1000, mBudget_NumVertsTotal / 1000)) : (text + "\nStats: WAITING..."));
		text = text + "\nQuality :" + QualitySettings.names[QualitySettings.GetQualityLevel()];
		text = text + "\nMax Available : " + (float)Caching.currentCacheForWriting.maximumAvailableStorageSpace * mMBSize;
		text = text + "\nFree Cache : " + (float)Caching.currentCacheForWriting.spaceFree * mMBSize;
		text = text + "\nCache in Use : " + (float)Caching.currentCacheForWriting.spaceOccupied * mMBSize;
		mTxtMemInfo.SetText(text);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		mStats_NumMeshes = (mStats_NumVertsTotal = -1);
		mStats_InBudget = true;
		Invoke("ComputeStats", 2f);
	}

	public void ComputeStats()
	{
		Mesh[] array = (Mesh[])Resources.FindObjectsOfTypeAll(typeof(Mesh));
		mStats_NumMeshes = 0;
		mStats_NumVertsTotal = 0;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (array[i] != null && array[i].boneWeights.Length == 0 && array[i].name != "TextMesh")
			{
				mStats_NumMeshes++;
				int vertexCount = array[i].vertexCount;
				mStats_NumVertsTotal += vertexCount;
			}
		}
		mStats_InBudget = true;
		if (mStats_NumVertsTotal > mBudget_NumVertsTotal)
		{
			mStats_InBudget = false;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		UILabel componentInChildren = mTxtMemInfo.GetComponentInChildren<UILabel>();
		if (componentInChildren != null)
		{
			componentInChildren.color = _Colors[mColorIndex];
		}
		componentInChildren = mTxtFPS.GetComponentInChildren<UILabel>();
		if (componentInChildren != null)
		{
			componentInChildren.color = _Colors[mColorIndex];
		}
		mColorIndex++;
		if (mColorIndex >= _Colors.Length)
		{
			mColorIndex = 0;
		}
	}
}
