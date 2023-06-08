using System;
using UnityEngine;

public class CoAnimController : KAMonoBase
{
	public string _MessageObjectName = "";

	public GameObject _MessageObject;

	public Transform _AvatarMarker;

	public Transform _PetMarker;

	public float _CrossFade = 0.2f;

	public float _AnimSpeed = 1f;

	public bool _DisplayTime;

	public Vector2 _TimePos = new Vector2(10f, 10f);

	public Transform _PostCSAvatarMarker;

	private float mTimer;

	private AnimationState mAState;

	private Vector3 mOriginalAvatarPosition = Vector3.zero;

	private Quaternion mOriginalAvatarRotation = Quaternion.identity;

	private bool mWasAvatarMounted;

	public static Action OnCutSceneStart;

	public static Action OnCutSceneDone;

	public virtual void Start()
	{
		if (_MessageObject == null && _MessageObjectName.Length > 0)
		{
			OnSetMessageObject(_MessageObjectName);
		}
	}

	public virtual void OnSetMessageObject(string oname)
	{
		_MessageObject = GameObject.Find(oname);
	}

	public virtual void OnSendMessage(string mname)
	{
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(mname, null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			base.gameObject.SendMessage(mname, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void OnSetCrossFade(float cf)
	{
		_CrossFade = cf;
	}

	public virtual void OnSetAnimSpeed(float speed)
	{
		_AnimSpeed = speed;
	}

	public virtual void OnPlayAnimChild(string aname)
	{
		string[] array = aname.Split('/');
		if (array.Length == 2)
		{
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, array[0]);
			if (transform != null)
			{
				TransformPlayAnimRecursively(transform, array[1], WrapMode.Loop);
			}
			else
			{
				Debug.LogError("GameObject not found : " + array[0]);
			}
		}
	}

	public virtual void OnPlayAnimChildOnce(string aname)
	{
		string[] array = aname.Split('/');
		if (array.Length == 2)
		{
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, array[0]);
			if (transform != null)
			{
				TransformPlayAnimRecursively(transform, array[1], WrapMode.Once);
			}
			else
			{
				Debug.LogError("GameObject not found : " + array[0]);
			}
		}
	}

	public virtual void OnPlayAnimSanctuaryPet(string aname)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			ObjectPlayAnimation(SanctuaryManager.pCurPetInstance.gameObject, aname, WrapMode.Loop);
		}
	}

	public virtual void OnPlayAnimSanctuaryPetOnce(string aname)
	{
		if (SanctuaryManager.pCurPetInstance != null)
		{
			ObjectPlayAnimation(SanctuaryManager.pCurPetInstance.gameObject, aname, WrapMode.Once);
		}
	}

	public virtual void ObjectPlayAnimation(GameObject obj, string aname, WrapMode wrap)
	{
		Animation component = obj.GetComponent<Animation>();
		component[aname].wrapMode = wrap;
		component[aname].speed = _AnimSpeed;
		if (_CrossFade > 0f)
		{
			component.CrossFade(aname, _CrossFade, PlayMode.StopAll);
		}
		else
		{
			component.Play(aname, PlayMode.StopAll);
		}
	}

	public virtual void TransformPlayAnimRecursively(Transform trans, string aname, WrapMode wrap)
	{
		Animation[] componentsInChildren = trans.GetComponentsInChildren<Animation>();
		foreach (Animation animation in componentsInChildren)
		{
			if (animation[aname] != null)
			{
				ObjectPlayAnimation(animation.gameObject, aname, wrap);
			}
		}
	}

	public virtual void SetAvatarStartLocation(string location)
	{
		AvAvatar.pStartLocation = location;
	}

	public virtual void OnPlayAnimOnce(string aname)
	{
		TransformPlayAnimRecursively(base.transform, aname, WrapMode.Once);
	}

	public virtual void OnPlayAnim(string aname)
	{
		TransformPlayAnimRecursively(base.transform, aname, WrapMode.Loop);
	}

	public virtual void OnPlayWave(AudioClip wave)
	{
		SnChannel component = base.gameObject.GetComponent<SnChannel>();
		if (component != null)
		{
			component.pClip = wave;
			component.Play();
		}
	}

	public virtual void OnDestroyLoadingScreen()
	{
		RsResourceManager.DestroyLoadScreen();
	}

	public virtual void OnLoadLevel(string level)
	{
		CutSceneDone();
		RsResourceManager.LoadLevel(level);
	}

	public virtual void Update()
	{
		if (!_DisplayTime)
		{
			return;
		}
		mTimer += Time.deltaTime;
		if (!(mAState == null))
		{
			return;
		}
		foreach (AnimationState item in base.animation)
		{
			if (item.enabled)
			{
				mAState = item;
				mTimer = mAState.time;
				break;
			}
		}
	}

	public virtual void CutSceneStart()
	{
		if (_PetMarker != null && SanctuaryManager.pCurPetInstance != null)
		{
			if (SanctuaryManager.pCurPetInstance.pIsMounted || SanctuaryManager.pMountedState)
			{
				mWasAvatarMounted = true;
				SanctuaryManager.pMountedState = false;
				SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
			}
			SanctuaryManager.pCurPetInstance.transform.parent = _PetMarker;
			SanctuaryManager.pCurPetInstance.transform.localPosition = Vector3.zero;
			SanctuaryManager.pCurPetInstance.transform.localRotation = Quaternion.identity;
			SanctuaryManager.pCurPetInstance.transform.localScale = Vector3.one;
			if (SanctuaryManager.pCurPetInstance.AIActor != null)
			{
				SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.CINEMATIC);
			}
		}
		if (UiAvatarControls.pInstance != null)
		{
			UiAvatarControls.pInstance.HideAvatar(hide: false);
		}
		if (_AvatarMarker != null)
		{
			mOriginalAvatarPosition = AvAvatar.position;
			mOriginalAvatarRotation = AvAvatar.mTransform.rotation;
			AvAvatar.SetDisplayNameVisible(inVisible: false);
			AvAvatar.pState = AvAvatarState.NONE;
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (AvAvatar.pSubState == AvAvatarSubState.GLIDING)
			{
				component.pIsPlayerGliding = false;
				component.OnGlideEnd();
			}
			if (component != null)
			{
				component.enabled = false;
				component._FlyingBone.localPosition = Vector3.zero;
				component._FlyingBone.localRotation = Quaternion.identity;
			}
			AvAvatar.SetParentTransform(_AvatarMarker);
			AvAvatar.mTransform.localPosition = Vector3.zero;
			AvAvatar.mTransform.localRotation = Quaternion.identity;
			AvAvatar.mTransform.localScale = Vector3.one;
			Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SetActive(value: false);
		}
		OnCutSceneStart?.Invoke();
	}

	public virtual void CutSceneDone()
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (AvAvatar.mTransform.parent != null)
		{
			AvAvatar.SetParentTransform(null);
			AvAvatar.SetPosition(mOriginalAvatarPosition);
			AvAvatar.mTransform.rotation = mOriginalAvatarRotation;
			AvAvatar.mTransform.localScale = Vector3.one;
			if (component != null)
			{
				component.enabled = true;
			}
			AvAvatar.pState = AvAvatarState.IDLE;
			Animator componentInChildren = AvAvatar.mTransform.GetComponentInChildren<Animator>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
			}
			AvAvatar.SetDisplayNameVisible(inVisible: true);
		}
		if (SanctuaryManager.pCurPetInstance != null && _PetMarker != null && !StableManager.pInstance)
		{
			SanctuaryManager.pCurPetInstance.transform.parent = null;
			SanctuaryManager.pCurPetInstance.transform.LookAt(AvAvatar.mTransform);
			if (SanctuaryManager.pCurPetInstance.AIActor != null)
			{
				SanctuaryManager.pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.NORMAL);
			}
		}
		else if (SanctuaryManager.pCurPetInstance != null && (bool)StableManager.pInstance)
		{
			SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
			if (pCurPetInstance == null)
			{
				return;
			}
			if (pCurPetInstance.pIsMounted)
			{
				pCurPetInstance.OnFlyDismount(AvAvatar.pObject);
			}
			if (StableManager.pInstance._ActivePetMarker != null)
			{
				UtDebug.Log("Moving active pet to marker at " + StableManager.pInstance._ActivePetMarker.transform.position.ToString());
				pCurPetInstance.transform.position = StableManager.pInstance._ActivePetMarker.transform.position;
				pCurPetInstance.transform.rotation = StableManager.pInstance._ActivePetMarker.transform.rotation;
				pCurPetInstance.pEnablePetAnim = true;
				pCurPetInstance.SetFollowAvatar(follow: false);
				pCurPetInstance.SetState(Character_State.idle);
				pCurPetInstance.AIActor.SetState(AISanctuaryPetFSM.IDLE);
				pCurPetInstance.PlayAnimation(pCurPetInstance._AnimNameIdle, WrapMode.Loop);
				pCurPetInstance.EnablePettingCollider(t: false);
			}
		}
		if (_PostCSAvatarMarker != null)
		{
			AvAvatar.SetPosition(_PostCSAvatarMarker.position);
			AvAvatar.mTransform.rotation = _PostCSAvatarMarker.rotation;
			if (SanctuaryManager.pCurPetInstance != null && (SanctuaryManager.pCurPetInstance.pIsMounted || mWasAvatarMounted))
			{
				if (!SanctuaryManager.pCurPetInstance.GetTypeInfo()._Flightless)
				{
					if (AvAvatar.pObject.transform.position.y - UtUtilities.GetGroundHeight(AvAvatar.pObject.transform.position, float.PositiveInfinity) > component._GroundSnapThreshold)
					{
						if (mWasAvatarMounted)
						{
							SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.FLY);
						}
						component.UpdateGliding();
						AvAvatar.pSubState = AvAvatarSubState.FLYING;
					}
					else if (mWasAvatarMounted)
					{
						SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
					}
					else if (SanctuaryManager.pCurPetInstance.pCurrentSkillType == PetSpecialSkillType.FLY)
					{
						SanctuaryManager.pCurPetInstance.OnFlyLanding(AvAvatar.pObject);
					}
				}
				else if (mWasAvatarMounted)
				{
					SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
				}
			}
		}
		else if (mWasAvatarMounted)
		{
			SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, SanctuaryManager.pCurPetInstance.pCurrentSkillType);
			if (SanctuaryManager.pCurPetInstance.pCurrentSkillType == PetSpecialSkillType.FLY)
			{
				component.SetFlyingState(FlyingState.Normal);
			}
		}
		mWasAvatarMounted = false;
		if (AvAvatar.pAvatarCam != null)
		{
			AvAvatar.pAvatarCam.SetActive(value: true);
		}
		OnCutSceneDone?.Invoke();
	}
}
