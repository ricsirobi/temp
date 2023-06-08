using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GrFPS : KAMonoBase
{
	public static bool _Display = false;

	public static bool _IsBelowMinimum = false;

	public static bool _AutoDegradeActive = true;

	public float _UpdateInterval = 0.3f;

	public float _DegradeTime = 10f;

	public float _MaximumFrameRate = 25f;

	public float _MinimumFrameRate = 15f;

	private float mFrameRate = 100f;

	private float mTimeAtLastFrameRateSample;

	private int mFrames;

	private string mText = "";

	private float mDegradeTime;

	private float mUpgradeTime;

	private bool mUpgraded;

	private float[] mAccumulatedFPS;

	private float[] mSortedAccumulatedFPS;

	private int mNextAccumulatedIndex;

	private int mCurrentQualityLevel = -1;

	private Camera mAvatarCam;

	private static GrFPS mInstance;

	private static Rect incRect = new Rect(10f, 140f, 50f, 50f);

	private static Rect decRect = new Rect(10f, 200f, 50f, 50f);

	public static float pFrameRate
	{
		get
		{
			if (mInstance == null)
			{
				return 30f;
			}
			return mInstance.mFrameRate;
		}
	}

	private Camera pAvatarCam
	{
		get
		{
			if (mAvatarCam == null && AvAvatar.pAvatarCam != null)
			{
				mAvatarCam = AvAvatar.pAvatarCam.GetComponent<Camera>();
			}
			return mAvatarCam;
		}
	}

	private void Start()
	{
		mTimeAtLastFrameRateSample = Time.realtimeSinceStartup;
		mFrames = 0;
		mDegradeTime = _DegradeTime;
		mUpgradeTime = _DegradeTime;
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
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
		mCurrentQualityLevel = -1;
	}

	private void Update()
	{
		if (RsResourceManager.pLevelLoadingScreen || !GameDataConfig.pIsReady)
		{
			return;
		}
		mFrames++;
		if (mFrames >= 5)
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = realtimeSinceStartup - mTimeAtLastFrameRateSample;
			mTimeAtLastFrameRateSample = realtimeSinceStartup;
			mFrameRate = (float)mFrames / num;
			mFrames = 0;
			if (mAccumulatedFPS == null)
			{
				mAccumulatedFPS = new float[15];
				mSortedAccumulatedFPS = new float[15];
				for (int i = 0; i < mAccumulatedFPS.Length; i++)
				{
					mAccumulatedFPS[i] = mFrameRate;
				}
			}
			mAccumulatedFPS[mNextAccumulatedIndex] = mFrameRate;
			mNextAccumulatedIndex = (mNextAccumulatedIndex + 1) % mAccumulatedFPS.Length;
			Array.Copy(mAccumulatedFPS, mSortedAccumulatedFPS, mAccumulatedFPS.Length);
			Array.Sort(mSortedAccumulatedFPS, (float a, float b) => (int)(a - b));
			mFrameRate = mSortedAccumulatedFPS[mSortedAccumulatedFPS.Length / 2];
			mText = mFrameRate.ToString("f2");
		}
		if (_AutoDegradeActive)
		{
			if (mFrameRate > _MaximumFrameRate)
			{
				mUpgradeTime -= Time.deltaTime;
				if ((double)mUpgradeTime <= 0.0)
				{
					QualitySettings.IncreaseLevel();
					mUpgraded = true;
					mUpgradeTime = _DegradeTime;
				}
			}
			else if (mFrameRate <= _MinimumFrameRate)
			{
				mDegradeTime -= Time.deltaTime;
				if ((double)mDegradeTime <= 0.0)
				{
					if (QualitySettings.GetQualityLevel() == UtUtilities.GetQualityByName("Very Low"))
					{
						_IsBelowMinimum = true;
					}
					else
					{
						QualitySettings.DecreaseLevel();
						if (mUpgraded)
						{
							_MaximumFrameRate += 5f;
						}
						mDegradeTime = _DegradeTime;
						if ((QualitySettings.GetQualityLevel() == UtUtilities.GetQualityByName("Low") || QualitySettings.GetQualityLevel() == UtUtilities.GetQualityByName("Very Low")) && pAvatarCam != null && pAvatarCam.renderingPath == RenderingPath.DeferredLighting)
						{
							pAvatarCam.renderingPath = RenderingPath.Forward;
						}
					}
				}
			}
			else
			{
				mDegradeTime = _DegradeTime;
				mUpgradeTime = _DegradeTime;
			}
		}
		if (mCurrentQualityLevel != QualitySettings.GetQualityLevel())
		{
			mCurrentQualityLevel = QualitySettings.GetQualityLevel();
			GameDataConfig.OptimizeTerrain(mCurrentQualityLevel);
		}
	}

	private void OnGUI()
	{
		if (!_Display)
		{
			return;
		}
		if (Input.GetKeyUp(KeyCode.Equals) || GUI.Button(incRect, "Up"))
		{
			QualitySettings.IncreaseLevel();
			Input.ResetInputAxes();
		}
		else if (Input.GetKeyUp(KeyCode.Minus) || GUI.Button(decRect, "Down"))
		{
			QualitySettings.DecreaseLevel();
			Input.ResetInputAxes();
		}
		else if (Input.GetKeyUp(KeyCode.O))
		{
			if ((bool)pAvatarCam)
			{
				pAvatarCam.farClipPlane -= 50f;
				if (pAvatarCam.farClipPlane < 50f)
				{
					pAvatarCam.farClipPlane = 50f;
				}
			}
			Input.ResetInputAxes();
		}
		else if (Input.GetKeyUp(KeyCode.P))
		{
			if (pAvatarCam != null)
			{
				pAvatarCam.farClipPlane += 50f;
			}
			Input.ResetInputAxes();
		}
		else if (Input.GetKeyUp(KeyCode.L) && pAvatarCam != null)
		{
			if (!RenderSettings.fog)
			{
				RenderSettings.fogDensity = 0.015f;
				RenderSettings.fogColor = new Color(0.57254905f, 0.4117647f, 77f / 85f, 1f);
				pAvatarCam.backgroundColor = RenderSettings.fogColor;
				pAvatarCam.clearFlags = CameraClearFlags.Color;
				RenderSettings.fog = true;
			}
			else
			{
				RenderSettings.fog = false;
				pAvatarCam.clearFlags = CameraClearFlags.Skybox;
			}
			Input.ResetInputAxes();
		}
		GUI.contentColor = Color.blue;
		GUI.Label(new Rect(0f, 0f, 100f, 20f), mText);
		GUI.Label(new Rect(0f, 10f, 100f, 20f), QualitySettings.names[QualitySettings.GetQualityLevel()]);
		if (pAvatarCam != null)
		{
			GUI.Label(new Rect(0f, 20f, 100f, 20f), "FarClip " + pAvatarCam.farClipPlane);
		}
		GUI.Label(new Rect(0f, 30f, 200f, 20f), "Width = " + Screen.width + ", Height = " + Screen.height);
		GUI.contentColor = Color.white;
	}
}
