using System;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class MMOAvatarLite : MMOAvatar
{
	private const float CROSSFADE_LENGTH = 0.25f;

	private static AssetBundle mAssetBundle = null;

	private static bool mAllowLite = true;

	public float _MoveThreshold = 0.3f;

	[Tooltip("How long does the mmo avatar have to be idle before the idle anim plays.")]
	public float _IdleTime = 0.2f;

	private Transform mAvatarShadow;

	private bool mVisible = true;

	private bool mFullAvatarLoading;

	private float mIdleTime;

	private Transform mBillBoard3D;

	private static int NumAllowedFullAvatars = 0;

	private static float TimeOfLastMMOOptimization = 0f;

	private static List<MMOAvatar> mAvatars = new List<MMOAvatar>();

	private static int mMaxDeviceFullMMO = -1;

	public static float FPSforMinFullAvatars = 5f;

	public static float FPSforMaxFullAvatars = 20f;

	public static bool LastOptimizationWasSkipped = false;

	public bool pIsMounted
	{
		get
		{
			if (base.pController != null)
			{
				return base.pController.pPlayerMounted;
			}
			return false;
		}
	}

	private void Start()
	{
		if (mAssetBundle == null)
		{
			RsResourceManager.Load(GameConfig.GetKeyData("MMOAvatarLiteAsset"), OnBundleReady, RsResourceType.NONE, inDontDestroy: true);
		}
		mAllowLite = MainStreetMMOClient.pInstance.IsMMOLiteAllowed(RsResourceManager.pCurrentLevel);
	}

	private void OnBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mAssetBundle = inObject as AssetBundle;
			if (base.pMMOAvatarType == MMOAvatarType.LITE)
			{
				Set3DBillboard();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Error downloading bundle: " + inURL);
			break;
		}
	}

	public override void OnCreated()
	{
		base.OnCreated();
		if (DataCache.Get<AvatarData>(base.pUserID + "_AvatarData", out var _))
		{
			DataCache.Remove(base.pUserID + "_AvatarData");
		}
		base.pMMOAvatarType = ((mAllowLite && (UtPlatform.IsMobile() || !(GrFPS.pFrameRate >= FPSforMaxFullAvatars))) ? MMOAvatarType.LITE : MMOAvatarType.FULL);
		mAvatarShadow = base.transform.Find("AvatarShadow");
		OnAvatarTypeChanged();
	}

	public override void Update()
	{
		base.Update();
		if (mMMOAvatarType != MMOAvatarType.LITE || !(mBillBoard3D != null))
		{
			return;
		}
		if (pIsMounted)
		{
			Animation component = mBillBoard3D.GetComponent<Animation>();
			if (!component.IsPlaying("FlyForward"))
			{
				component.CrossFade("FlyForward", 0.25f);
			}
			return;
		}
		switch (mController.pSubState)
		{
		case AvAvatarSubState.SWIMMING:
			PlayAnim("Swim", "SwimIdle");
			break;
		case AvAvatarSubState.UWSWIMMING:
			PlayAnim("UnderwaterSwim", "UnderwaterSwimIdle");
			break;
		default:
			PlayAnim("Run", "Idle");
			break;
		}
	}

	private void PlayAnim(string moveAnim, string idleAnim)
	{
		bool num = new Vector3(mController.pVelocity.x, 0f, mController.pVelocity.z).magnitude > _MoveThreshold;
		if (num)
		{
			mIdleTime = _IdleTime;
		}
		else
		{
			mIdleTime -= Time.deltaTime;
		}
		Animation component = mBillBoard3D.GetComponent<Animation>();
		string text = ((num || mIdleTime > 0f) ? moveAnim : idleAnim);
		if (!string.IsNullOrEmpty(text) && !component.IsPlaying(text))
		{
			component.CrossFade(text, 0.25f);
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (mAvatarData == null || mAvatarData.mInstance == null || (mMMOAvatarType == MMOAvatarType.FULL && mFullAvatarLoading))
		{
			SetVisible(Visible: false);
		}
	}

	public override void OnAvatarTypeChanged()
	{
		if (mAvatarShadow != null)
		{
			mAvatarShadow.gameObject.SetActive(mMMOAvatarType == MMOAvatarType.FULL);
		}
		bool flag = mMMOAvatarType == MMOAvatarType.LITE;
		if (mMMOAvatarType == MMOAvatarType.FULL)
		{
			mFullAvatarLoading = true;
			flag = DoLoad();
		}
		if (flag)
		{
			OnUpdateAvatar();
		}
	}

	public void OnUpdateAvatar()
	{
		mFullAvatarLoading = false;
		if (mMMOAvatarType == MMOAvatarType.LITE)
		{
			Set3DBillboard();
			base.pDisplayName.parent = base.transform.Find(AvatarSettings.pInstance._SpriteName);
			mSprite.gameObject.SetActive(value: true);
			mMeshObject.gameObject.SetActive(value: false);
		}
		else if (mMMOAvatarType == MMOAvatarType.FULL)
		{
			base.pDisplayName.parent = base.transform.Find(AvatarSettings.pInstance._AvatarName);
			mMeshObject.gameObject.SetActive(value: true);
			mSprite.gameObject.SetActive(value: false);
		}
		base.gameObject.GetComponent<AvAvatarController>()?.UpdateDisplayName(pIsMounted, mAvatarData.mInstance._Group != null);
		SetVisible(mVisible, bForced: true);
	}

	protected override bool UpdateOptimization()
	{
		RunMMOOptimization();
		return base.UpdateOptimization();
	}

	private static void RunMMOOptimization()
	{
		try
		{
			if (MainStreetMMOClient.pInstance == null)
			{
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			float num = 2f;
			if (realtimeSinceStartup - TimeOfLastMMOOptimization < num)
			{
				return;
			}
			Transform avatarT = AvAvatar.mTransform;
			if (avatarT == null)
			{
				return;
			}
			TimeOfLastMMOOptimization = realtimeSinceStartup;
			if (RsResourceManager.IsLoading())
			{
				LastOptimizationWasSkipped = true;
				return;
			}
			LastOptimizationWasSkipped = false;
			if (mMaxDeviceFullMMO < 0)
			{
				mMaxDeviceFullMMO = UtPlatform.GetDeviceMaxFullMMO();
			}
			float value = Mathf.InverseLerp(FPSforMinFullAvatars, FPSforMaxFullAvatars, GrFPS.pFrameRate);
			value = Mathf.Clamp01(value);
			NumAllowedFullAvatars = (int)Mathf.Lerp(0f, 50f, value);
			if (NumAllowedFullAvatars > mMaxDeviceFullMMO)
			{
				NumAllowedFullAvatars = mMaxDeviceFullMMO;
			}
			mAvatars.Clear();
			foreach (KeyValuePair<string, MMOAvatar> pPlayer in MainStreetMMOClient.pInstance.pPlayerList)
			{
				if (pPlayer.Value != null && pPlayer.Value.pTransform != null && pPlayer.Value.pAvatarData != null && pPlayer.Value.pAvatarData.mInstance != null)
				{
					mAvatars.Add(pPlayer.Value);
				}
			}
			mAvatars.Sort(delegate(MMOAvatar a, MMOAvatar b)
			{
				if (avatarT == null || a == null || a.pTransform == null || b == null || b.pTransform == null)
				{
					return int.MaxValue;
				}
				float sqrMagnitude = (a.pTransform.position - avatarT.position).sqrMagnitude;
				float sqrMagnitude2 = (b.pTransform.position - avatarT.position).sqrMagnitude;
				return (int)(sqrMagnitude - sqrMagnitude2);
			});
			int num2 = 0;
			int num3 = 0;
			int num4 = 0;
			for (int i = 0; i < mAvatars.Count; i++)
			{
				MMOAvatar mMOAvatar = mAvatars[i];
				MMOAvatarLite mMOAvatarLite = mMOAvatar as MMOAvatarLite;
				if (mMOAvatar == null || mMOAvatarLite == null)
				{
					continue;
				}
				bool flag = num2 < NumAllowedFullAvatars || !mAllowLite;
				MMOAvatarType mMOAvatarType = ((!flag) ? MMOAvatarType.LITE : MMOAvatarType.FULL);
				if ((flag && mMOAvatar.pMMOAvatarType == MMOAvatarType.LITE && num3 >= 1) || (!flag && mMOAvatar.pMMOAvatarType == MMOAvatarType.FULL && num4 >= 1))
				{
					mMOAvatarType = mMOAvatar.pMMOAvatarType;
				}
				if (!mMOAvatarLite.pIsMounted || (mMOAvatarLite.pIsMounted && mMOAvatarLite.pSanctuaryPet != null))
				{
					mMOAvatarLite.SetVisible(Visible: true);
					if (mMOAvatar.pMMOAvatarType != mMOAvatarType && mMOAvatar.pCanSwitch)
					{
						mMOAvatar.pMMOAvatarType = mMOAvatarType;
						if (mMOAvatar.pMMOAvatarType == MMOAvatarType.FULL)
						{
							num3++;
						}
						else
						{
							num4++;
						}
					}
					if (mMOAvatar.pMMOAvatarType == MMOAvatarType.FULL)
					{
						num2++;
					}
				}
				else
				{
					mMOAvatarLite.SetVisible(Visible: false);
				}
			}
			if (num4 > 0)
			{
				if (FPSforMinFullAvatars < FPSforMaxFullAvatars - 1f)
				{
					FPSforMinFullAvatars += 0.25f;
				}
			}
			else
			{
				FPSforMinFullAvatars = Mathf.Max(15f, FPSforMinFullAvatars - 0.25f);
			}
			mAvatars.Clear();
		}
		catch (Exception)
		{
		}
	}

	public override void SetVisible(bool Visible, bool bForced = false)
	{
		if ((mVisible != Visible || bForced) && (!Visible || base.pCanAvatarShow))
		{
			if (mMMOAvatarType == MMOAvatarType.FULL)
			{
				mSprite.gameObject.SetActive(value: false);
				mMeshObject.gameObject.SetActive(Visible);
			}
			else
			{
				mSprite.gameObject.SetActive(Visible);
				mMeshObject.gameObject.SetActive(value: false);
			}
			base.pDisplayName.gameObject.SetActive(Visible);
			mVisible = Visible;
		}
	}

	private bool SetIcon(bool isMounted = false)
	{
		if (mAssetBundle == null || mAvatarData == null || mAvatarData.mInstance == null)
		{
			return false;
		}
		mSprite.localScale = Vector3.one;
		Set3DBillboard();
		return true;
	}

	private void Set3DBillboard()
	{
		if (mAssetBundle == null || mAvatarData == null || mAvatarData.mInstance == null)
		{
			return;
		}
		string text = "Billboard3D_";
		text = ((mAvatarData.mInstance.GenderType != Gender.Male) ? (text + "Girl") : (text + "Boy"));
		if (mBillBoard3D != null && mBillBoard3D.name != text)
		{
			UnityEngine.Object.Destroy(mBillBoard3D.gameObject);
			mBillBoard3D = null;
		}
		if (mBillBoard3D == null)
		{
			UnityEngine.Object @object = mAssetBundle.LoadAsset(text);
			if (@object != null)
			{
				mBillBoard3D = (UnityEngine.Object.Instantiate(@object) as GameObject).transform;
				mBillBoard3D.parent = mSprite;
				mBillBoard3D.localPosition = Vector3.zero;
				mBillBoard3D.localRotation = Quaternion.identity;
				mBillBoard3D.gameObject.SetActive(value: true);
			}
			else
			{
				UtDebug.LogError("Error : Could not find object by name : " + text);
			}
		}
	}

	public override void SetRaisedPet(string pdata)
	{
		if (MainStreetMMOClient.pInstance.IsLevelMMO(RsResourceManager.pCurrentLevel))
		{
			base.SetRaisedPet(pdata);
		}
	}
}
