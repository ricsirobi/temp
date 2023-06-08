using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SanctuaryPetItemData : CoBundleItemData
{
	public AssetBundle mBundle;

	public ItemPrefabResData m3DData = new ItemPrefabResData();

	protected string mUserID;

	protected RaisedPetData mPetData;

	protected Vector3 mPos = Vector3.zero;

	protected Quaternion mRot = Quaternion.identity;

	protected GameObject mMessageObject;

	protected Texture mTexture;

	protected SanctuaryPet mPet;

	protected string mAnimSet = "";

	protected List<AssetBundle> mAnimBundles = new List<AssetBundle>();

	protected bool mPoolDragons;

	protected bool mApplyCustomSkin;

	public SanctuaryPetItemData(string UserID, RaisedPetData pdata, Vector3 pos, Quaternion rot, GameObject msgObj, string animSet, bool applyCustomSkin, bool poolDragons = false)
	{
		mUserID = UserID;
		mPetData = pdata;
		mPos = pos;
		mRot = rot;
		mMessageObject = msgObj;
		mAnimSet = animSet;
		mPoolDragons = poolDragons;
		_ItemRVOData.Init(null);
		m3DData.Init(mPetData.Geometry);
		mApplyCustomSkin = applyCustomSkin;
	}

	public override void OnBundleReady(string inURL, AssetBundle bd)
	{
		if (RsResourceManager.Compare(inURL, m3DData._ResBundleName))
		{
			mBundle = bd;
		}
		m3DData.OnBundleReady(inURL, bd);
		_ItemRVOData.OnBundleReady(inURL, bd);
		mCurLoadingBundleIdx++;
		int num = ((!string.IsNullOrEmpty(mAnimSet)) ? 1 : 0);
		if (mCurLoadingBundleIdx >= mNumBundles + num)
		{
			OnAllDownloaded();
		}
	}

	public override void LoadResource()
	{
		base.LoadResource();
		m3DData.LoadBundle(this);
		LoadAnimation();
	}

	private void LoadAnimation()
	{
		if (string.IsNullOrEmpty(mAnimSet))
		{
			return;
		}
		SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mPetData.PetTypeID);
		if (sanctuaryPetTypeInfo != null)
		{
			string text = ((mPetData != null && Array.Find(sanctuaryPetTypeInfo._AgeData, (PetAgeData p) => p._Name.ToLower().Contains(mPetData.pStage.ToString().ToLower()))._AgeSpecificAnim) ? mPetData.pStage.ToString().ToLower() : "");
			RsResourceManager.Load("RS_SHARED/DW" + sanctuaryPetTypeInfo._Name + text + "Anim" + mAnimSet, OnAnimLoadEvent, RsResourceType.ASSET_BUNDLE);
		}
	}

	public void ApplyAnimBundles(SanctuaryPet pet)
	{
		if (!(pet != null) || mAnimBundles == null || mAnimBundles.Count <= 0)
		{
			return;
		}
		foreach (AssetBundle mAnimBundle in mAnimBundles)
		{
			if (mAnimBundle == null)
			{
				continue;
			}
			UnityEngine.Object[] array = mAnimBundle.LoadAllAssets(typeof(AnimationClip));
			if (array != null && array.Length != 0)
			{
				UnityEngine.Object[] array2 = array;
				for (int i = 0; i < array2.Length; i++)
				{
					AnimationClip animationClip = (AnimationClip)array2[i];
					pet.animation.AddClip(animationClip, animationClip.name);
				}
			}
		}
	}

	public override void OnAllDownloaded()
	{
		GameObject gameObject = null;
		if (mPoolDragons)
		{
			if (!PoolManager.Pools.TryGetValue("_Dragons", out var spawnPool))
			{
				spawnPool = new GameObject("_DragonsPool").AddComponent<SpawnPool>();
			}
			Transform transform = null;
			if (spawnPool != null && m3DData != null && m3DData._Prefab != null && mPetData != null)
			{
				transform = spawnPool.Spawn(m3DData._Prefab, mPos, mRot);
			}
			if (transform != null)
			{
				gameObject = transform.gameObject;
			}
			PrefabPool prefabPool = spawnPool.GetPrefabPool(m3DData._Prefab);
			prefabPool.cullAbove = 2;
			prefabPool.cullDespawned = true;
		}
		else if (m3DData != null && m3DData._Prefab != null && mPetData != null && !mPetData.pAbortCreation)
		{
			gameObject = UnityEngine.Object.Instantiate(m3DData._Prefab.gameObject, mPos, mRot);
		}
		if (gameObject != null)
		{
			if (UtPlatform.IsEditor())
			{
				UtUtilities.ReAssignShader(gameObject);
			}
			mPet = gameObject.GetComponent<SanctuaryPet>();
			if (!(mPet != null))
			{
				return;
			}
			mPet._MessageObject = mMessageObject;
			mPet._AnimBundles = mAnimBundles;
			mPetData.pTexture = mTexture as Texture2D;
			mPetData.pTextureBMP = null;
			if (mApplyCustomSkin && SanctuaryData.pInstance != null)
			{
				mPet.Init(mPetData, SanctuaryManager.pInstance._NoHat, SanctuaryData.pInstance.GetSceneCustomSkin());
			}
			else
			{
				mPet.Init(mPetData, SanctuaryManager.pInstance._NoHat);
			}
			if (mMessageObject != null)
			{
				if (!mMessageObject.activeInHierarchy)
				{
					mMessageObject.SetActive(value: true);
					mMessageObject.SendMessage("OnPetCreated", mPet, SendMessageOptions.DontRequireReceiver);
					mMessageObject.SetActive(value: false);
				}
				else
				{
					mMessageObject.SendMessage("OnPetCreated", mPet, SendMessageOptions.DontRequireReceiver);
				}
			}
			NavMeshAgent component = mPet.GetComponent<NavMeshAgent>();
			if (component != null)
			{
				component.enabled = true;
			}
		}
		else
		{
			UtDebug.LogError("Pet instance cant create");
		}
	}

	public void OnAnimLoadEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AssetBundle assetBundle = inObject as AssetBundle;
			if (assetBundle != null)
			{
				mAnimBundles.Add(assetBundle);
			}
			OnBundleReady(inURL, null);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			OnBundleReady(inURL, null);
			Debug.LogError("Missing bundle: " + inURL);
			break;
		}
	}
}
