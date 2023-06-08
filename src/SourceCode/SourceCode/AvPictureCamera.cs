using System;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AvPictureCamera : KAMonoBase
{
	public float _Aspect;

	private static Texture2D mSendTexture;

	private float mLastAspect;

	public Texture pPictTexture => base.camera.targetTexture;

	private void Awake()
	{
		if (_Aspect != 0f)
		{
			base.camera.aspect = _Aspect;
		}
		mLastAspect = _Aspect;
		if (base.camera.targetTexture != null && mSendTexture == null)
		{
			mSendTexture = new Texture2D(base.camera.targetTexture.width, base.camera.targetTexture.height, TextureFormat.RGB24, mipChain: false);
			if (mSendTexture != null)
			{
				UnityEngine.Object.DontDestroyOnLoad(mSendTexture);
			}
		}
	}

	private void Update()
	{
		if (_Aspect != mLastAspect)
		{
			mLastAspect = _Aspect;
			if (_Aspect != 0f)
			{
				base.camera.aspect = _Aspect;
			}
			else
			{
				base.camera.ResetAspect();
			}
		}
	}

	public void OnTakePicture(GameObject targetObject)
	{
		OnTakePicture(targetObject, lookAtCamera: true);
	}

	public void OnTakePicture(GameObject targetObject, bool lookAtCamera)
	{
		if (targetObject != null)
		{
			if (base.camera.targetTexture != null)
			{
				if (lookAtCamera)
				{
					targetObject.transform.LookAt(base.transform, Vector3.up);
				}
				base.camera.Render();
			}
			else
			{
				Debug.LogError("ERROR: FAILED TO GET RENDER TEXTURE!!");
			}
		}
		else
		{
			Debug.LogError("ERROR: TARGET OBJECT IS NULL!!");
		}
	}

	public bool OnCopyRenderBuffer()
	{
		return OnCopyRenderBuffer(mSendTexture);
	}

	public bool OnCopyRenderBuffer(Texture2D saveTexture)
	{
		bool flag = false;
		if (base.camera.targetTexture != null)
		{
			if (saveTexture != null)
			{
				RenderTexture.active = base.camera.targetTexture;
				flag = true;
				try
				{
					saveTexture.ReadPixels(new Rect(0f, 0f, base.camera.targetTexture.width, base.camera.targetTexture.height), 0, 0);
				}
				catch (Exception)
				{
					flag = false;
				}
				if (flag)
				{
					saveTexture.Apply();
				}
				RenderTexture.active = null;
			}
			else
			{
				Debug.LogError("ERROR: FAILED TO GET SEND TEXTURE!!");
			}
		}
		else
		{
			Debug.LogError("ERROR: FAILED TO GET RENDER TEXTURE!!");
		}
		return flag;
	}
}
