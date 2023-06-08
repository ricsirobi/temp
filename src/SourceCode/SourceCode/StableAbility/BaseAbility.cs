using System;
using UnityEngine;

namespace StableAbility;

[Serializable]
public class BaseAbility : MonoBehaviour
{
	public ABILITY _Ability;

	public LocaleString _NameText = new LocaleString("Ability name");

	public LocaleString _DescriptionText = new LocaleString("Ability description");

	public LocaleString _RewardText = new LocaleString("Reward: XX amount of XX for XX time");

	public Texture _Image;

	[Tooltip("Cooldown time in minutes.")]
	public float _CooldownTime;

	public int _TicketItemID;

	public int _TicketStoreID;

	public GameObject _SceneObject;

	public GameObject _CutSceneObject;

	public string _ActionName;

	private CoAnimController mCurrentCutscene;

	private PairData mPairData;

	public PairData pPairData
	{
		get
		{
			return mPairData;
		}
		set
		{
			mPairData = value;
		}
	}

	public virtual void OnPairDataLoaded(bool success, PairData pData, object inUserData)
	{
		if (success)
		{
			mPairData = pData;
		}
		else
		{
			UtDebug.LogError("PairData failed to load!!");
		}
	}

	public virtual void ActivateAbility()
	{
		UtDebug.Log("Activating ability: " + _Ability);
	}

	public virtual bool CanActivate()
	{
		return true;
	}

	protected void ShowCutScene()
	{
		if (_CutSceneObject == null)
		{
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(_CutSceneObject);
		if (!(gameObject == null))
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatarController avAvatarController = AvAvatar.pObject?.GetComponent<AvAvatarController>();
			if (avAvatarController != null)
			{
				avAvatarController.enabled = false;
			}
			Transform obj = gameObject.transform;
			obj.parent = obj;
			obj.localPosition = Vector3.zero;
			obj.localRotation = Quaternion.identity;
			mCurrentCutscene = gameObject.GetComponentInChildren<CoAnimController>();
			if (!mCurrentCutscene)
			{
				OnCutSceneDone();
				return;
			}
			mCurrentCutscene._MessageObject = base.gameObject;
			mCurrentCutscene.CutSceneStart();
		}
	}

	protected virtual void OnCutSceneDone()
	{
		if (mCurrentCutscene != null)
		{
			mCurrentCutscene.CutSceneDone();
			UnityEngine.Object.Destroy(mCurrentCutscene.gameObject);
			mCurrentCutscene = null;
		}
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pAvatarCam.SetActive(value: true);
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.enabled = true;
		}
	}
}
