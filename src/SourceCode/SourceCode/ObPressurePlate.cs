using UnityEngine;

public class ObPressurePlate : ObSwitchBase
{
	public bool _AvatarTrigger;

	public bool _PetTrigger;

	public bool _IgnoreTriggerColliders;

	public string _ObjectTriggerTag = string.Empty;

	public GameObject _TriggerObject;

	public float _ReturnFade = 0.3f;

	public AnimationClip _Animation;

	public Material _MaterialSwitchOn;

	public SnSound _SoundOn;

	public SnSound _SoundOff;

	private Material mDefaultMaterial;

	private int mTriggerObjectsCount;

	private string mDefaultAnimation;

	private string mAnimToPlay;

	private SnChannel mChannel;

	private ObPressurePlateGroup mParentObject;

	public virtual void Awake()
	{
		mParentObject = base.transform.parent.GetComponent<ObPressurePlateGroup>();
		if (base.animation != null && base.animation.clip != null)
		{
			mDefaultAnimation = base.animation.clip.name;
			if (_Animation != null)
			{
				mAnimToPlay = _Animation.name;
			}
		}
	}

	private void OnTriggerEnter(Collider inCollider)
	{
		if (!_IgnoreTriggerColliders || !inCollider.isTrigger)
		{
			GameObject gameObject = inCollider.gameObject;
			SanctuaryPet component = gameObject.GetComponent<SanctuaryPet>();
			if ((_AvatarTrigger && AvAvatar.IsCurrentPlayer(gameObject)) || (_PetTrigger && component != null && component == SanctuaryManager.pCurPetInstance) || (!string.IsNullOrEmpty(_ObjectTriggerTag) && gameObject.tag.Equals(_ObjectTriggerTag)) || _TriggerObject == gameObject)
			{
				Enter();
			}
		}
	}

	private void OnTriggerExit(Collider inCollider)
	{
		if ((!_IgnoreTriggerColliders || !inCollider.isTrigger) && mSwitchOn)
		{
			GameObject gameObject = inCollider.gameObject;
			SanctuaryPet component = gameObject.GetComponent<SanctuaryPet>();
			if ((_AvatarTrigger && AvAvatar.IsCurrentPlayer(gameObject)) || (_PetTrigger && component != null && component == SanctuaryManager.pCurPetInstance) || (!string.IsNullOrEmpty(_ObjectTriggerTag) && gameObject.tag.Equals(_ObjectTriggerTag)) || _TriggerObject == gameObject)
			{
				Exit();
			}
		}
	}

	public void Enter()
	{
		mTriggerObjectsCount++;
		if (!mSwitchOn)
		{
			SwitchOn();
		}
		if (mParentObject != null)
		{
			mParentObject.TriggerObjectsCount++;
		}
	}

	public void Exit()
	{
		mTriggerObjectsCount--;
		if (mParentObject != null)
		{
			mParentObject.TriggerObjectsCount--;
			if (mParentObject.TriggerObjectsCount <= 0 && mTriggerObjectsCount <= 0)
			{
				SwitchOff();
			}
			else if (mTriggerObjectsCount <= 0)
			{
				SwitchOff(message: false);
			}
		}
		else if (mTriggerObjectsCount <= 0)
		{
			SwitchOff();
		}
	}

	protected override void SwitchOn()
	{
		if (_OneWaySwitch && mOneWayDone)
		{
			return;
		}
		UtDebug.Log("PressurePlate SwithOn");
		if (base.animation != null)
		{
			if (!base.animation.IsPlaying(mAnimToPlay))
			{
				AnimationState animationState = base.animation[mAnimToPlay];
				if (_SoundOff._AudioClip != null && mChannel != null && mChannel.pAudioSource.clip == _SoundOff._AudioClip)
				{
					mChannel.Stop();
					mChannel = null;
				}
				if (_SoundOn._AudioClip != null)
				{
					mChannel = _SoundOn.Play();
				}
				base.animation.Play(mAnimToPlay);
				animationState.speed = 1f;
				animationState.wrapMode = WrapMode.Once;
			}
			else if (base.animation[mAnimToPlay].speed < 0f)
			{
				base.animation[mAnimToPlay].speed = 1f;
			}
		}
		if (_MaterialSwitchOn != null && base.renderer != null)
		{
			if (mDefaultMaterial == null)
			{
				mDefaultMaterial = base.renderer.material;
			}
			base.renderer.material = _MaterialSwitchOn;
		}
		base.SwitchOn();
	}

	protected override void SwitchOff(bool message = true)
	{
		if (_OneWaySwitch && mOneWayDone)
		{
			return;
		}
		UtDebug.Log("PressurePlate SwithOff");
		if (base.animation != null && !base.animation.IsPlaying(mDefaultAnimation))
		{
			if (_SoundOn._AudioClip != null && mChannel != null && mChannel.pAudioSource.clip == _SoundOn._AudioClip)
			{
				mChannel.Stop();
				mChannel = null;
			}
			if (_SoundOff._AudioClip != null)
			{
				mChannel = _SoundOff.Play();
			}
			base.animation.Play(mDefaultAnimation);
			base.animation[mDefaultAnimation].wrapMode = WrapMode.Once;
		}
		if (_MaterialSwitchOn != null && base.renderer != null && mDefaultMaterial != null)
		{
			base.renderer.material = mDefaultMaterial;
		}
		base.SwitchOff(message);
	}
}
