using System.Collections;
using UnityEngine;

public class AvSpellCast : MonoBehaviour
{
	public float _MorphDuration = 10f;

	public float _SpellCastTime = 1.5f;

	public float _ParticleEffectTime = 2f;

	public string[] _CastAnimNames;

	public string _GauntletBundleName = "RS_DATA/DNAGauntlet.unity3d/PfGauntletDNA";

	public string _Anim_Shoot = "CadetDNAShoot";

	public string _Anim_Shoot_Run = "CadetDNAShootRun";

	public Vector3 _GauntletPositionOffset = Vector3.zero;

	public GameObject _ParticleEffect;

	public Vector3 _ParticleOffset;

	private float mMorphTimer;

	private AvAvatarController mAvatarController;

	private GameObject mActualAvatar;

	private GameObject mMorphAvatar;

	private GameObject mPreviousFZCollectObject;

	private GameObject mGauntlet;

	private GameObject mDefaultGauntlet;

	private GameObject mParticleEffect;

	private bool mIsSpellCastPending;

	private bool mDetatchGauntlet;

	public void Start()
	{
		mAvatarController = GetComponent<AvAvatarController>();
	}

	public void AttachGauntlet()
	{
		string[] array = _GauntletBundleName.Split('/');
		RsResourceManager.Load(array[0] + "/" + array[1], OnGauntletBundleReady);
	}

	private void OnGauntletBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			return;
		}
		AssetBundle assetBundle = (AssetBundle)inObject;
		if (assetBundle != null)
		{
			string[] array = _GauntletBundleName.Split('/');
			GameObject gameObject = (GameObject)assetBundle.LoadAsset(array[2]);
			if (gameObject != null)
			{
				DetatchGauntlets(mDetatchGauntlet);
				mGauntlet = Object.Instantiate(gameObject);
				Transform parent = UtUtilities.FindChildTransform(base.gameObject, "R_Elb_J");
				mGauntlet.transform.parent = parent;
				mGauntlet.transform.localPosition = _GauntletPositionOffset;
				mGauntlet.transform.localRotation = Quaternion.identity;
			}
			AttachDefaultGauntlet();
			if (mIsSpellCastPending)
			{
				mIsSpellCastPending = false;
				PlaySpellCastAnim();
			}
		}
	}

	public void DetatchGauntletLater()
	{
		mDetatchGauntlet = true;
	}

	public void DetatchGauntlets(bool canDetatch)
	{
		if (canDetatch)
		{
			mDetatchGauntlet = false;
			if (mGauntlet != null)
			{
				Object.Destroy(mGauntlet);
			}
			if (mDefaultGauntlet != null)
			{
				Object.Destroy(mDefaultGauntlet);
			}
		}
	}

	private void AttachDefaultGauntlet()
	{
	}

	public void PlaySpellCastAnim()
	{
		if (mGauntlet == null)
		{
			AttachGauntlet();
			mIsSpellCastPending = true;
			return;
		}
		if (mAvatarController != null)
		{
			string pCurrentAnim = mAvatarController.pCurrentAnim;
			AvAvatarStateAnims stateAnims = mAvatarController.pCurrentStateData._StateAnims;
			if (mAvatarController._MainRoot.localEulerAngles.y != 0f)
			{
				mAvatarController.pCurrentAnim = stateAnims._Idle;
				AvAvatar.PlayAnimInstant(base.gameObject, mAvatarController.pCurrentAnim);
				mAvatarController.pState = AvAvatarState.IDLE;
			}
			mAvatarController.pFidgetTime = mAvatarController.pCurrentStateData._FidgetTimeMax;
			if (pCurrentAnim == stateAnims._Run || pCurrentAnim == stateAnims._RunLeft || pCurrentAnim == stateAnims._RunRight)
			{
				AvAvatar.PlayAnim(base.gameObject, _Anim_Shoot_Run, WrapMode.Once);
			}
			else
			{
				AvAvatar.PlayAnim(base.gameObject, _Anim_Shoot, WrapMode.Once);
			}
		}
		StartCoroutine("StartParticleAfterEndOfFrame");
		Invoke("ResetSpellCastAnim", _SpellCastTime);
		Invoke("DestroyParticleEffects", _ParticleEffectTime);
	}

	private IEnumerator StartParticleAfterEndOfFrame()
	{
		yield return new WaitForEndOfFrame();
		if (_ParticleEffect != null)
		{
			if (mParticleEffect != null)
			{
				Object.Destroy(mParticleEffect);
			}
			mParticleEffect = Object.Instantiate(_ParticleEffect);
			mParticleEffect.transform.position = mGauntlet.transform.TransformPoint(_ParticleOffset);
			mParticleEffect.transform.up = -base.transform.forward;
		}
	}

	private void DestroyParticleEffects()
	{
		if (mParticleEffect != null)
		{
			Object.Destroy(mParticleEffect);
		}
	}

	public void ResetSpellCastAnim()
	{
		DetatchGauntlets(mDetatchGauntlet);
		mAvatarController.pCurrentAnim = mAvatarController.pCurrentStateData._StateAnims._Idle;
		AvAvatar.PlayAnim(base.gameObject, mAvatarController.pCurrentAnim, WrapMode.Loop);
	}

	public void Morph(int spellItemID)
	{
	}

	private void Morph(GameObject inMorphObject)
	{
		if (inMorphObject != null)
		{
			UnMorph();
			DisableSpellCastBtn(isDisable: true);
			mMorphAvatar = Object.Instantiate(inMorphObject);
			mMorphAvatar.transform.parent = base.transform;
			mMorphAvatar.transform.localPosition = Vector3.zero;
			mMorphAvatar.transform.localRotation = Quaternion.identity;
			SetActualAvatarVisible(isVisible: false);
		}
	}

	public void SpellMorphItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			string[] array = dataItem.AssetName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSpellMorphLoaded, typeof(GameObject));
		}
	}

	public void OnSpellMorphLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Morph((GameObject)inObject);
		}
		if (inEvent == RsResourceLoadEvent.ERROR)
		{
			Debug.LogError("Error !!! Morph item not available");
		}
	}

	public void UnMorph()
	{
		DisableSpellCastBtn(isDisable: false);
		SetActualAvatarVisible(isVisible: true);
		if (mMorphAvatar != null)
		{
			Object.Destroy(mMorphAvatar);
			mAvatarController.pCurrentAnim = mAvatarController.pCurrentStateData._StateAnims._Idle;
			AvAvatar.PlayAnim(base.gameObject, mAvatarController.pCurrentStateData._StateAnims._Idle, WrapMode.Loop);
		}
	}

	private void DisableSpellCastBtn(bool isDisable)
	{
		if (!(AvAvatar.pToolbar != null) || !AvAvatar.IsCurrentPlayer(base.gameObject))
		{
			return;
		}
		UiActions componentInChildren = AvAvatar.pToolbar.GetComponentInChildren<UiActions>();
		if (componentInChildren != null)
		{
			KAWidget kAWidget = componentInChildren.FindItem("ActionsBtn");
			if (kAWidget != null)
			{
				kAWidget.SetDisabled(isDisable);
			}
		}
	}

	private void SetActualAvatarVisible(bool isVisible)
	{
		if (!mAvatarController)
		{
			return;
		}
		mActualAvatar = mAvatarController._MainRoot.gameObject;
		if (mActualAvatar != null)
		{
			Component[] componentsInChildren = mActualAvatar.GetComponentsInChildren<Renderer>();
			componentsInChildren = componentsInChildren;
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Renderer)componentsInChildren[i]).enabled = isVisible;
			}
		}
	}

	public void Update()
	{
		if (mMorphTimer > 0f)
		{
			if (mAvatarController != null && mAvatarController.pCarriedObject != mPreviousFZCollectObject)
			{
				mPreviousFZCollectObject = mAvatarController.pCarriedObject;
				mPreviousFZCollectObject.SetActive(value: false);
			}
			mMorphTimer -= Time.deltaTime;
			if (mMorphTimer <= 0f)
			{
				UnMorph();
				ResetCarryObject();
			}
		}
	}

	private void ResetCarryObject()
	{
		if (mAvatarController != null && mAvatarController.pCarriedObject != null)
		{
			mAvatarController.pCarriedObject.SetActive(value: true);
			mAvatarController.Collect(mAvatarController.pCarriedObject);
		}
	}
}
