using System.Collections.Generic;
using UnityEngine;

public class NPCAvatarLite : NPCAvatar
{
	public float _MinDistanceFromAvatar = 50f;

	public float _UpdateFrequency = 2f;

	public string _BundlePath = string.Empty;

	public string _SpriteName = "Sprite";

	private GameObject mMeshObject;

	private GameObject mSpriteObject;

	private Transform mAvatarTransform;

	private Transform mTransform;

	private bool mLoadingBundle;

	private List<Transform> mMeshObjectTransforms;

	private ObStatus mStatus;

	private bool mDelay3DMesh;

	private object m3DMeshObject;

	private bool mIsLiteVersion = true;

	public virtual void Awake()
	{
		mStatus = GetComponent<ObStatus>();
	}

	public override void Start()
	{
		base.Start();
		Transform transform = base.transform.Find(_SpriteName);
		if (transform != null)
		{
			mSpriteObject = transform.gameObject;
		}
		else
		{
			UtDebug.LogError("Sprite object not found under : " + base.name);
		}
		mTransform = base.transform;
		Load3DObject();
		mIsLiteVersion = false;
	}

	public virtual void OnEnable()
	{
		if (mDelay3DMesh && m3DMeshObject != null)
		{
			mDelay3DMesh = false;
			CreateNPC(m3DMeshObject);
		}
	}

	public void Unload3DObject()
	{
		if (mMeshObject != null && !mLoadingBundle)
		{
			DestroyNPC();
		}
	}

	public void Load3DObject()
	{
		if (!string.IsNullOrEmpty(_BundlePath))
		{
			if (mMeshObject == null && !mLoadingBundle)
			{
				mLoadingBundle = true;
				string[] array = _BundlePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBundleLoadingEvent, typeof(GameObject));
			}
		}
		else
		{
			UtDebug.LogError("Bundle name not provided for " + base.name);
			if (mStatus != null)
			{
				mStatus.pIsReady = true;
			}
		}
	}

	private void OnBundleLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (base.gameObject.activeSelf)
			{
				CreateNPC(inObject);
				break;
			}
			mDelay3DMesh = true;
			m3DMeshObject = inObject;
			if (mStatus != null)
			{
				mStatus.pIsReady = true;
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load bundle : " + inURL);
			if (mStatus != null)
			{
				mStatus.pIsReady = true;
			}
			break;
		}
	}

	private void OnMemoryThresholdChanged(MemoryThreshold inMemThreshold)
	{
		if (IsWithinRange())
		{
			Load3DObject();
		}
		else
		{
			Unload3DObject();
		}
	}

	private bool IsWithinRange()
	{
		if (mAvatarTransform == null)
		{
			return false;
		}
		return Vector3.Distance(mAvatarTransform.position, mTransform.position) <= _MinDistanceFromAvatar;
	}

	private bool CanLoad3DObject()
	{
		if (mIsLiteVersion && IsWithinRange())
		{
			return true;
		}
		return false;
	}

	private void EnableProximityAnimate(bool inEnable)
	{
		if (mProximityAnimate != null)
		{
			mProximityAnimate.enabled = inEnable && GetState() == Character_State.idle;
		}
	}

	public void CreateNPC(object inObject)
	{
		mMeshObject = Object.Instantiate(inObject as GameObject, new Vector3(0f, 0f, -5000f), Quaternion.identity);
		mMeshObject.name = "MeshObject";
		mMeshObject.transform.parent = base.transform;
		mMeshObject.transform.localPosition = Vector3.zero;
		mMeshObject.transform.localRotation = Quaternion.identity;
		Renderer[] componentsInChildren = mMeshObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null)
		{
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				if (renderer != null)
				{
					renderer.enabled = false;
				}
			}
		}
		mIsLiteVersion = false;
		mMeshObjectTransforms = new List<Transform>();
		while (mMeshObject.transform.childCount != 0)
		{
			Transform child = mMeshObject.transform.GetChild(0);
			child.parent = base.transform;
			mMeshObjectTransforms.Add(child);
		}
		if (componentsInChildren != null)
		{
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer2 in array)
			{
				if (renderer2 != null)
				{
					renderer2.enabled = true;
				}
			}
		}
		base.animation.enabled = true;
		mLoadingBundle = false;
		if (mSpriteObject != null)
		{
			mSpriteObject.SetActive(value: false);
		}
		EnableProximityAnimate(inEnable: true);
		m3DMeshObject = null;
		if (mStatus != null)
		{
			mStatus.pIsReady = true;
		}
	}

	public void DestroyNPC()
	{
		EnableProximityAnimate(inEnable: false);
		if (mSpriteObject != null)
		{
			mSpriteObject.SetActive(value: true);
		}
		base.animation.enabled = false;
		mIsLiteVersion = true;
		foreach (Transform mMeshObjectTransform in mMeshObjectTransforms)
		{
			mMeshObjectTransform.parent = mMeshObject.transform;
		}
		mMeshObjectTransforms.Clear();
		Object.Destroy(mMeshObject);
		mMeshObject = null;
		RsResourceManager.Unload(_BundlePath);
		RsResourceManager.UnloadUnusedAssets();
	}

	public override void SetState(Character_State newstate)
	{
		base.SetState(newstate);
		if (mIsLiteVersion)
		{
			EnableProximityAnimate(inEnable: false);
		}
	}
}
