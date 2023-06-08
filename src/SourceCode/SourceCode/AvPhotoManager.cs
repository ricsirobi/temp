using System;
using System.Collections.Generic;
using UnityEngine;

public class AvPhotoManager : MonoBehaviour
{
	public AvPictureCamera _AvatarCam;

	public Light _AvatarLight;

	public Dictionary<string, Texture> pPictureCache = new Dictionary<string, Texture>();

	protected List<AvPhotoLoader> mLoaders = new List<AvPhotoLoader>();

	public bool _IsHeadShot;

	public int _WaitFrames = 3;

	public int _RenderTextureWidth = 64;

	public int _RenderTextureHeight = 64;

	public Vector3 _HeadShotCamOffset = new Vector3(0f, 0.1f, 1.3f);

	public bool _LiveShot;

	public bool _UseBounds = true;

	private GameObject mLiveAvatar;

	private AvatarData.InstanceInfo mAvatarInstance;

	private Camera mCameraComp;

	public static int mIdx;

	public virtual Texture TakeAShot(GameObject avatar, Texture2D dstTexture, string userID, bool isMovingAllowed = true)
	{
		if (avatar == null)
		{
			return null;
		}
		float y = avatar.transform.localScale.y;
		_AvatarLight.enabled = true;
		if (isMovingAllowed)
		{
			avatar.transform.parent = base.transform;
			avatar.transform.localPosition = Vector3.zero;
			avatar.transform.localRotation = Quaternion.identity;
		}
		if (_IsHeadShot)
		{
			Transform transform = avatar.transform.Find(AvatarData.pBoneSettings.HAT_PARENT_BONE);
			if (transform != null)
			{
				Vector3 headShotCamOffset = _HeadShotCamOffset;
				if (_UseBounds)
				{
					headShotCamOffset.z += GetOffsetFromRenderers(transform);
				}
				Vector3 position = transform.TransformPoint(new Vector3(headShotCamOffset.x, headShotCamOffset.y * y, headShotCamOffset.z));
				_AvatarCam.transform.position = position;
				_AvatarCam.transform.LookAt(transform.position + new Vector3(headShotCamOffset.x, headShotCamOffset.y * y, 0f), Vector3.up);
			}
		}
		if (_LiveShot)
		{
			mLiveAvatar = avatar;
			mCameraComp.enabled = true;
			return mCameraComp.targetTexture;
		}
		_AvatarCam.OnTakePicture(avatar, lookAtCamera: false);
		avatar.transform.localPosition = Vector3.right * 100f;
		UnityEngine.Object.Destroy(avatar);
		_AvatarLight.enabled = false;
		if (dstTexture == null)
		{
			return mCameraComp.targetTexture;
		}
		_AvatarCam.OnCopyRenderBuffer(dstTexture);
		pPictureCache[userID] = dstTexture;
		return dstTexture;
	}

	public virtual void TakeAShot(GameObject obj, ref Texture2D dstTexture, Transform headTransform)
	{
		if (dstTexture == null)
		{
			dstTexture = new Texture2D(_RenderTextureWidth, _RenderTextureHeight, TextureFormat.ARGB32, mipChain: false);
		}
		Vector3 localScale = obj.transform.localScale;
		_AvatarLight.enabled = true;
		Transform parent = obj.transform.parent;
		Vector3 localPosition = obj.transform.localPosition;
		Quaternion localRotation = obj.transform.localRotation;
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.transform.localRotation = Quaternion.identity;
		Vector3 localPosition2 = _AvatarCam.transform.localPosition;
		Quaternion localRotation2 = _AvatarCam.transform.localRotation;
		_AvatarCam.transform.parent = headTransform;
		Vector3 headShotCamOffset = _HeadShotCamOffset;
		if (_UseBounds)
		{
			headShotCamOffset.z += GetOffsetFromRenderers(headTransform);
		}
		_AvatarCam.transform.localPosition = headShotCamOffset;
		_AvatarCam.transform.LookAt(headTransform.position + new Vector3(headShotCamOffset.x * localScale.x, headShotCamOffset.y * localScale.y, 0f), Vector3.up);
		_AvatarCam.OnTakePicture(obj, lookAtCamera: false);
		obj.transform.localPosition = Vector3.right * 100f;
		_AvatarLight.enabled = false;
		_AvatarCam.OnCopyRenderBuffer(dstTexture);
		obj.transform.parent = parent;
		obj.transform.localPosition = localPosition;
		obj.transform.localRotation = localRotation;
		_AvatarCam.transform.parent = base.transform;
		_AvatarCam.transform.localPosition = localPosition2;
		_AvatarCam.transform.localRotation = localRotation2;
	}

	private float GetOffsetFromRenderers(Transform inTransform)
	{
		float result = 0f;
		if (inTransform != null)
		{
			Bounds bounds = new Bounds(inTransform.position, Vector3.zero);
			MeshRenderer[] componentsInChildren = inTransform.GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer meshRenderer in componentsInChildren)
			{
				bounds.Encapsulate(meshRenderer.bounds);
			}
			Vector3 vector = bounds.max - bounds.min;
			float num = Mathf.Max(vector.x, vector.y, vector.z);
			float num2 = 2f * Mathf.Tan(MathF.PI / 360f * _AvatarCam.camera.fieldOfView);
			result = num / num2;
		}
		return result;
	}

	public static AvatarData.InstanceInfo LoadAvatar(string userID)
	{
		AvatarData.InstanceInfo instanceInfo = new AvatarData.InstanceInfo();
		instanceInfo.mAvatar = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(AvAvatar.GetAvatarPrefabName()), new Vector3(0f, -5000f, 0f), Quaternion.identity);
		instanceInfo.mAvatar.name = "Photo Avatar " + userID;
		instanceInfo.mAvatar.tag = "Untagged";
		instanceInfo.mMergeWithDefault = true;
		AvAvatarController component = instanceInfo.mAvatar.GetComponent<AvAvatarController>();
		component._ControlMode = AvAvatarControlMode.NONE;
		component.pState = AvAvatarState.PAUSED;
		component.DoUpdate();
		AvAvatarBlink componentInChildren = instanceInfo.mAvatar.GetComponentInChildren<AvAvatarBlink>();
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		AvatarData.SetDisplayNameVisible(instanceInfo.mAvatar, inVisible: false, isMember: false);
		AvatarData.Load(instanceInfo, userID);
		return instanceInfo;
	}

	public virtual void Awake()
	{
		mCameraComp = _AvatarCam.GetComponent<Camera>();
		if (mCameraComp.targetTexture == null)
		{
			mCameraComp.targetTexture = new RenderTexture(_RenderTextureWidth, _RenderTextureHeight, 24);
			mCameraComp.targetTexture.name = "Render Texture for " + base.name;
		}
		else
		{
			_RenderTextureWidth = mCameraComp.targetTexture.width;
			_RenderTextureHeight = mCameraComp.targetTexture.height;
		}
	}

	public static AvPhotoManager Init(string profilename)
	{
		GameObject gameObject = GameObject.Find(profilename);
		if (gameObject != null)
		{
			return gameObject.GetComponent<AvPhotoManager>();
		}
		gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources(profilename));
		gameObject.name = profilename;
		return gameObject.GetComponent<AvPhotoManager>();
	}

	public void TakePhotoUI(string userID, AvPhotoCallback cbk, object udata)
	{
		TakePhotoUI(userID, null, cbk, udata);
	}

	public void TakePhotoUI(string userID, Texture2D defaultImage, AvPhotoCallback cbk, object udata)
	{
		if (pPictureCache.ContainsKey(userID))
		{
			cbk(pPictureCache[userID], udata);
			return;
		}
		Texture2D texture2D = null;
		foreach (AvPhotoLoader mLoader in mLoaders)
		{
			if (mLoader._UserID == userID)
			{
				texture2D = mLoader._DstTexture;
				break;
			}
		}
		if (texture2D == null)
		{
			texture2D = new Texture2D(_RenderTextureWidth, _RenderTextureHeight, TextureFormat.ARGB32, mipChain: false);
			texture2D.name = "T2D " + mIdx;
			mIdx++;
			if (defaultImage != null)
			{
				Color[] pixels = defaultImage.GetPixels();
				texture2D.SetPixels(pixels);
				texture2D.Apply();
			}
		}
		TakePhoto(userID, texture2D, cbk, udata);
	}

	public void TakePhoto(string userID, Texture2D dstTex, AvPhotoCallback cbk, object udata)
	{
		DestroyLiveShot();
		if (pPictureCache.ContainsKey(userID))
		{
			cbk(pPictureCache[userID], udata);
			return;
		}
		AvPhotoLoader avPhotoLoader = new AvPhotoLoader();
		avPhotoLoader._Callback = cbk;
		avPhotoLoader._UserData = udata;
		avPhotoLoader._Manager = this;
		avPhotoLoader._UserID = userID;
		avPhotoLoader._DstTexture = dstTex;
		if (avPhotoLoader._DstTexture != null)
		{
			avPhotoLoader._DstTexture.name = "PhotoManager-" + userID;
		}
		foreach (AvPhotoLoader mLoader in mLoaders)
		{
			if (avPhotoLoader._UserID == mLoader._UserID)
			{
				avPhotoLoader.pAvatarInst = mLoader.pAvatarInst;
				break;
			}
		}
		if (avPhotoLoader.pAvatarInst == null)
		{
			avPhotoLoader.pAvatarInst = LoadAvatar(userID);
			mAvatarInstance = avPhotoLoader.pAvatarInst;
		}
		avPhotoLoader.pFrameCounter = _WaitFrames;
		mLoaders.Add(avPhotoLoader);
	}

	public virtual void DestroyLiveShot()
	{
		_AvatarLight.enabled = false;
		if (mAvatarInstance != null)
		{
			mAvatarInstance.Release();
			mAvatarInstance = null;
		}
		if (mLiveAvatar != null)
		{
			UnityEngine.Object.Destroy(mLiveAvatar);
		}
		mLiveAvatar = null;
	}

	public virtual void Update()
	{
		int count = mLoaders.Count;
		if (count <= 0)
		{
			return;
		}
		for (int num = count - 1; num >= 0; num--)
		{
			if (mLoaders[num].Update())
			{
				mLoaders.Remove(mLoaders[num]);
			}
		}
	}
}
