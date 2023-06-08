using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class IconMaker : MonoBehaviour
{
	public int _IconWidth = 128;

	public int _IconHeight = 128;

	public GameObject _Marker;

	public KeyCode _Key = KeyCode.Space;

	public string _ExportIconPath = "/zzCommon/IconMaker/Exported Icons";

	public GameObject[] _Targets;

	public string _IconName = "Icon";

	public bool _UseDate;

	public bool _AutoPosition = true;

	public bool _AutoRotate;

	public float _ZoomPer = 1f;

	public bool _UseRenderer = true;

	public bool _UseCollider;

	public float _Alpha = 0.8f;

	public bool _UseAlphaTweak;

	private RenderTexture mTargetTexture;

	private Texture2D mScreenShotTexure;

	private bool mProcessing;

	private Material mMaterial;

	private void OnPostRender()
	{
		if (_UseAlphaTweak)
		{
			GL.PushMatrix();
			GL.LoadOrtho();
			mMaterial.SetFloat("_Alpha", _Alpha);
			mMaterial.SetPass(0);
			GL.Begin(7);
			GL.Vertex3(0f, 0f, 0.1f);
			GL.Vertex3(1f, 0f, 0.1f);
			GL.Vertex3(1f, 1f, 0.1f);
			GL.Vertex3(0f, 1f, 0.1f);
			GL.End();
			GL.PopMatrix();
		}
	}

	private void Start()
	{
		mTargetTexture = new RenderTexture(_IconWidth, _IconHeight, 24, RenderTextureFormat.ARGB32);
		mTargetTexture.useMipMap = false;
		mTargetTexture.Create();
		mScreenShotTexure = new Texture2D(_IconWidth, _IconHeight, TextureFormat.ARGB32, mipChain: false);
		mMaterial = new Material(Shader.Find("IconMaker/Clear Alpha"));
		mMaterial = new Material(Shader.Find("IconMaker/Alpha"));
	}

	private void Update()
	{
		if (!mProcessing && Input.GetKeyDown(_Key))
		{
			mProcessing = true;
			StartCoroutine(TakeScreenShot());
		}
	}

	private void PreProcess()
	{
		for (int i = 0; i < _Targets.Length; i++)
		{
			GameObject gameObject = _Targets[i];
			if (GameObject.Find(gameObject.name) == null)
			{
				gameObject = UnityEngine.Object.Instantiate(_Targets[i], Vector3.one * -10000f, _Marker.transform.rotation);
				gameObject.name = _Targets[i].name;
				_Targets[i] = gameObject;
			}
		}
	}

	private void OnGUI()
	{
		GUI.enabled = !mProcessing;
		if (GUILayout.Button("TakeShot", GUILayout.Width(500f), GUILayout.Height(50f)))
		{
			mProcessing = true;
			Debug.Log("@@ TakeScreenShot @@");
			StartCoroutine(TakeScreenShot());
		}
	}

	private Bounds CalculateBounds(GameObject inGO)
	{
		Bounds result = new Bounds(inGO.transform.localPosition, Vector3.zero);
		if (_UseRenderer)
		{
			UnityEngine.Object[] componentsInChildren = inGO.GetComponentsInChildren(typeof(Renderer));
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Renderer renderer = (Renderer)componentsInChildren[i];
				result.Encapsulate(renderer.bounds);
			}
		}
		if (_UseCollider)
		{
			UnityEngine.Object[] componentsInChildren = inGO.GetComponentsInChildren(typeof(Collider));
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Collider collider = (Collider)componentsInChildren[i];
				result.Encapsulate(collider.bounds);
			}
		}
		return result;
	}

	public void FocusCameraOnGameObject(Camera inCam, GameObject inGO)
	{
		if (_AutoPosition)
		{
			Bounds bounds = CalculateBounds(inGO);
			Vector3 size = bounds.size;
			float orthographicSize = size.magnitude / 2f;
			inCam.transform.position = bounds.center + inCam.transform.forward * (0f - size.magnitude);
			if (inCam.orthographic)
			{
				inCam.orthographicSize = orthographicSize;
			}
			if (_AutoRotate)
			{
				Debug.LogWarning(" @@ Loook AT ");
				inCam.transform.LookAt(bounds.center);
			}
		}
	}

	private IEnumerator WaitForFrames(int inFrame)
	{
		while (inFrame > 0)
		{
			yield return 0;
			inFrame--;
		}
		yield return new WaitForEndOfFrame();
	}

	private IEnumerator TakeScreenShot()
	{
		mProcessing = true;
		int idx = 0;
		while (idx < _Targets.Length)
		{
			GameObject targetGO = _Targets[idx];
			if (GameObject.Find(targetGO.name) == null)
			{
				Debug.LogWarning("============= Object not in scene : " + targetGO.name);
				targetGO = ((!(_Marker != null)) ? UnityEngine.Object.Instantiate(_Targets[idx], Vector3.one * -1000f, Quaternion.identity) : UnityEngine.Object.Instantiate(_Targets[idx], _Marker.transform.position, _Marker.transform.rotation));
				targetGO.name = _Targets[idx].name;
				_Targets[idx] = targetGO;
				yield return new WaitForSeconds(0.1f);
			}
			yield return new WaitForSeconds(0.1f);
			if (_Marker != null)
			{
				targetGO.transform.localPosition = _Marker.transform.position;
				targetGO.transform.localRotation = _Marker.transform.rotation;
			}
			yield return new WaitForSeconds(1f);
			FocusCameraOnGameObject(Camera.main, targetGO);
			yield return new WaitForSeconds(0.5f);
			float prVTimeScale = Time.timeScale;
			float prVFixedDT = Time.fixedDeltaTime;
			Time.timeScale = 0f;
			Time.fixedDeltaTime = 0f;
			yield return 0;
			Camera camera = Camera.main;
			if (camera == null)
			{
				Camera[] allCameras = Camera.allCameras;
				foreach (Camera camera2 in allCameras)
				{
					if (camera2.enabled)
					{
						camera = camera2;
						break;
					}
				}
				if (camera == null)
				{
					Debug.Log("No camera found for screenshot.");
					Time.timeScale = prVTimeScale;
					Time.fixedDeltaTime = prVFixedDT;
				}
			}
			if (camera != null)
			{
				Debug.Log("TakeScreenShot " + targetGO.name);
				float farClipPlane = camera.farClipPlane;
				float nearClipPlane = camera.nearClipPlane;
				camera.targetTexture = mTargetTexture;
				GL.Clear(clearDepth: true, clearColor: true, Color.white);
				camera.Render();
				RenderTexture.active = mTargetTexture;
				mScreenShotTexure.ReadPixels(new Rect(0f, 0f, _IconWidth, _IconWidth), 0, 0);
				RenderTexture.active = null;
				camera.targetTexture = null;
				camera.nearClipPlane = nearClipPlane;
				camera.farClipPlane = farClipPlane;
				byte[] array = mScreenShotTexure.EncodeToPNG();
				string path = ((!_UseDate) ? (Application.dataPath + _ExportIconPath + "/" + targetGO.name + ".png") : (Application.dataPath + _ExportIconPath + "/" + _IconName + DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + ".png"));
				FileStream fileStream = new FileStream(path, FileMode.Create);
				fileStream.Write(array, 0, array.Length);
				fileStream.Close();
				yield return 0;
				Time.timeScale = prVTimeScale;
				Time.fixedDeltaTime = prVFixedDT;
				targetGO.transform.localPosition = Vector3.one * -1000f;
				yield return 0;
			}
			int i = idx + 1;
			idx = i;
		}
		mProcessing = false;
		Debug.LogWarning("@@@@@@@ TakeScreenShot :: Done @@@@@@@@@@@");
	}
}
