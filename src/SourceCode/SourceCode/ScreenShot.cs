using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ScreenShot : KAMonoBase
{
	public int resWidth = 4000;

	public int resHeight = 3000;

	public KeyCode key = KeyCode.KeypadEnter;

	public bool recordByFrames = true;

	public int frameCount = 30;

	public float recordTime = 1f;

	public float movieFrameRate = 30f;

	public bool beginMovie;

	public float beginDelay;

	private RenderTexture rt;

	private Texture2D screenShot;

	public Camera cam;

	public float farClip = 25000f;

	public int curFrameCount;

	private float curFrameDelay;

	private float movieFrameDelay;

	private int movieFrameCount;

	private float curTimeCount;

	[HideInInspector]
	public bool bTakingScreenShot;

	[HideInInspector]
	public bool bTakingMovie;

	public string savePath;

	public bool alpha;

	private void Start()
	{
		if (string.IsNullOrEmpty(savePath))
		{
			savePath = Application.dataPath;
			if (!savePath.EndsWith("Assets/"))
			{
				savePath = savePath.Substring(0, savePath.Length - 6);
				savePath += "Screenshots/";
			}
		}
	}

	private void RefreshSize()
	{
		if ((bool)rt)
		{
			UnityEngine.Object.Destroy(rt);
		}
		if ((bool)screenShot)
		{
			UnityEngine.Object.Destroy(screenShot);
		}
		if (alpha)
		{
			rt = new RenderTexture(resWidth, resHeight, 24, RenderTextureFormat.ARGB32);
		}
		else
		{
			rt = new RenderTexture(resWidth, resHeight, 24);
		}
		rt.Create();
		if (alpha)
		{
			screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, mipChain: false);
		}
		else
		{
			screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, mipChain: false);
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(key) && !bTakingScreenShot && !bTakingMovie)
		{
			bTakingScreenShot = true;
			StartCoroutine("DoShot");
		}
	}

	private void LateUpdate()
	{
		if (beginMovie && !bTakingScreenShot && !bTakingMovie)
		{
			Debug.Log("Movie!");
			bTakingMovie = true;
			curFrameCount = 0;
			movieFrameDelay = 1f / movieFrameRate;
			curFrameDelay = movieFrameDelay - beginDelay + Time.deltaTime;
			if (recordByFrames)
			{
				movieFrameCount = frameCount;
			}
			else
			{
				movieFrameCount = (int)(recordTime * movieFrameRate);
			}
			StartCoroutine("DoMovie");
		}
	}

	private IEnumerator DoMovie()
	{
		bTakingMovie = true;
		float savedMaxDeltaTime = Time.maximumDeltaTime;
		Time.maximumDeltaTime = Mathf.Max(0.01666f, movieFrameDelay);
		while (curFrameCount < movieFrameCount)
		{
			curFrameDelay += Time.deltaTime;
			curTimeCount += Time.deltaTime;
			if (curFrameDelay >= movieFrameDelay)
			{
				curFrameDelay -= movieFrameDelay;
				yield return StartCoroutine("DoShot");
				curFrameCount++;
			}
			yield return new WaitForEndOfFrame();
		}
		bTakingMovie = false;
		beginMovie = false;
		curTimeCount = 0f;
		Time.maximumDeltaTime = savedMaxDeltaTime;
	}

	private IEnumerator DoShot()
	{
		Time.timeScale = 0f;
		float oldDeltaTime = Time.fixedDeltaTime;
		Time.fixedDeltaTime = 0f;
		bTakingScreenShot = true;
		yield return new WaitForEndOfFrame();
		if (cam == null)
		{
			Camera[] allCameras = Camera.allCameras;
			foreach (Camera camera in allCameras)
			{
				if (camera.enabled)
				{
					cam = camera;
					break;
				}
			}
			if (cam == null)
			{
				Debug.Log("No camera found for screenshot.");
				Time.timeScale = 1f;
				Time.fixedDeltaTime = 1f;
				bTakingScreenShot = false;
				yield return null;
			}
		}
		int qualityLevel = QualitySettings.GetQualityLevel();
		float farClipPlane = cam.farClipPlane;
		float nearClipPlane = cam.nearClipPlane;
		cam.nearClipPlane = 0.1f;
		cam.farClipPlane = farClip;
		RefreshSize();
		cam.targetTexture = rt;
		RenderTexture.active = rt;
		GL.Clear(clearDepth: true, clearColor: true, Color.white);
		cam.Render();
		screenShot.ReadPixels(new Rect(0f, 0f, resWidth, resHeight), 0, 0);
		RenderTexture.active = null;
		cam.targetTexture = null;
		cam.nearClipPlane = nearClipPlane;
		cam.farClipPlane = farClipPlane;
		QualitySettings.SetQualityLevel(qualityLevel);
		byte[] bytes = screenShot.EncodeToPNG();
		if (!Directory.Exists(savePath))
		{
			Directory.CreateDirectory(savePath);
		}
		if (bTakingMovie)
		{
			string text = $"_M{curFrameCount:D4}";
			string text2 = savePath + "screen" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + text + ".png";
			Debug.Log("Writing file: " + text2);
			File.WriteAllBytes(text2, bytes);
		}
		else
		{
			string text3 = savePath + "screen" + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".png";
			Debug.Log("Writing file: " + text3);
			File.WriteAllBytes(text3, bytes);
		}
		yield return new WaitForEndOfFrame();
		Time.timeScale = 1f;
		Time.fixedDeltaTime = oldDeltaTime;
		curTimeCount = 0f;
		bTakingScreenShot = false;
	}
}
