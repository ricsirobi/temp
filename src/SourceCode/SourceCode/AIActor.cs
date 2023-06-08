using System;
using System.Collections.Generic;
using SWS;
using UnityEngine;

public class AIActor : KAMonoBase
{
	[Serializable]
	public class AnimationData
	{
		public string _Action;

		public string _Name;
	}

	[Serializable]
	public class EffectData
	{
		public AudioClip _Sound;

		public string _Animation;

		public ParticleData _ParticleInfo;
	}

	[Serializable]
	public class ParticleData
	{
		public GameObject _Particle;

		public Vector3 _Position;
	}

	public AIBehavior _BehaviorsRoot;

	public Animation _Animation;

	public bool _CanFly;

	public float _Speed = 5f;

	public float _Acceleration = 5f;

	public float _DecelerateRange = 4f;

	public float _StoppingDistance = 2f;

	public ChCharacter _Character;

	public SoundMapper _SoundMapper = new SoundMapper();

	public float _GroundCheckStartHeight = 2f;

	public float _GroundCheckDist = 20f;

	public List<AnimationData> _AnimationDataList;

	[HideInInspector]
	public AIBehaviorState BehaviorFunctionCallResult = AIBehaviorState.FAILED;

	[HideInInspector]
	public float DeltaTime;

	[HideInInspector]
	public Vector3 Position;

	protected AIBehavior_FSM mFSMCharacter;

	protected int mState = -1;

	private float mLastUpdateTime = -1f;

	private AIBehavior_PlayAnim mCustomPlayAnim;

	private GameObject mParticleEffect;

	[Header("State/Speeds")]
	public List<MoveStateData> _MoveStates;

	public float _MoveCheckDistance = 1f;

	protected AIActor mController;

	public float Velocity { get; set; }

	public bool CanAccelerate { get; set; }

	public MoveStateData pMoveState { get; set; }

	public AIActor pController => mController;

	public bool pHasController => mController != null;

	public AIBehavior_PlayAnim pCustomPlayAnim => mCustomPlayAnim;

	private void Start()
	{
		if ((_MoveStates != null) & (_MoveStates.Count > 0))
		{
			pMoveState = _MoveStates[0];
			_Speed = pMoveState._Speed;
			if (_Character != null)
			{
				_Character.Speed = pMoveState._Speed;
			}
		}
		if (!_Animation)
		{
			_Animation = GetComponent<Animation>();
		}
	}

	public virtual void SetState(int newState)
	{
		mState = newState;
		UpdateCharacterFSM();
		if (mFSMCharacter != null)
		{
			mFSMCharacter.MoveToState(newState, this);
		}
	}

	public virtual void UpdateCharacterFSM()
	{
		if (!(mFSMCharacter != null) && !(_BehaviorsRoot == null))
		{
			mFSMCharacter = _BehaviorsRoot.transform.GetComponentInChildren<AIBehavior_FSM>();
			if (!(mFSMCharacter == null))
			{
				UpdateStateMap();
			}
		}
	}

	protected virtual void UpdateStateMap()
	{
		SetState(mState);
	}

	public virtual void Update()
	{
		Position = base.transform.position;
		float time = Time.time;
		if (mLastUpdateTime <= 0f)
		{
			DeltaTime = 0f;
		}
		else
		{
			DeltaTime = Mathf.Max(0f, time - mLastUpdateTime);
		}
		mLastUpdateTime = time;
		if (_BehaviorsRoot != null)
		{
			_BehaviorsRoot.Think(this);
		}
		UpdateCharacterFSM();
	}

	public void PlayCustomAnim(string AnimationName, WrapMode inWrapMode = WrapMode.Once, int inNumLoop = 0)
	{
		if (mCustomPlayAnim == null)
		{
			Transform transform = UtUtilities.FindChildTransform(_BehaviorsRoot.gameObject, "PlayCustomAnim");
			if (transform == null)
			{
				return;
			}
			mCustomPlayAnim = transform.GetComponent<AIBehavior_PlayAnim>();
		}
		if (mCustomPlayAnim != null)
		{
			mCustomPlayAnim._AnimName = AnimationName;
			mCustomPlayAnim._NumLoops = inNumLoop;
			mCustomPlayAnim.OnStart(this);
			mCustomPlayAnim.SetState(AIBehaviorState.INACTIVE);
			mCustomPlayAnim._Mode = inWrapMode;
		}
	}

	public void PlayAnimation(string clipName, bool force = true, float crossFadeTime = 0.3f, PlayMode playMode = PlayMode.StopSameLayer)
	{
		if (_Animation != null && (force || !_Animation.IsPlaying(clipName)))
		{
			if (_Animation.GetClip(clipName) != null)
			{
				_Animation.CrossFade(clipName, crossFadeTime, playMode);
			}
			else
			{
				UtDebug.LogError(clipName + " animation not present on " + base.name);
			}
		}
	}

	public void PlaySound(string SoundName)
	{
		AudioClip audioClip = _SoundMapper.GetAudioClip(SoundName);
		if (audioClip != null)
		{
			AudioSource.PlayClipAtPoint(audioClip, base.transform.position);
		}
	}

	public void PlayRandomSound(string SoundName)
	{
		AudioClip randomAudioClip = _SoundMapper.GetRandomAudioClip(SoundName);
		if (randomAudioClip != null)
		{
			AudioSource.PlayClipAtPoint(randomAudioClip, base.transform.position);
		}
	}

	public virtual void PlayEffect(List<EffectData> effectData)
	{
		foreach (EffectData effectDatum in effectData)
		{
			if (effectDatum._Sound != null)
			{
				SnChannel.Play(effectDatum._Sound, "SFX_Pool", inForce: true);
			}
			if (!string.IsNullOrEmpty(effectDatum._Animation))
			{
				PlayAnimation(effectDatum._Animation);
			}
			if (effectDatum._ParticleInfo != null && effectDatum._ParticleInfo._Particle != null)
			{
				if (mParticleEffect != null)
				{
					UnityEngine.Object.DestroyImmediate(mParticleEffect);
				}
				mParticleEffect = GameUtilities.PlayAt(Position + effectDatum._ParticleInfo._Position, effectDatum._ParticleInfo._Particle);
			}
		}
	}

	public virtual void DoAction(string action, params object[] values)
	{
	}

	public virtual Transform GetTransformOnFlying()
	{
		return base.transform;
	}

	public virtual bool CanFly()
	{
		if (_CanFly)
		{
			return AvAvatar.IsFlying();
		}
		return false;
	}

	public virtual bool IsFlying()
	{
		return false;
	}

	public virtual string GetAnimationName(string action)
	{
		AnimationData animationData = _AnimationDataList.Find((AnimationData t) => t._Action.Equals(action));
		if (animationData == null)
		{
			return string.Empty;
		}
		return animationData._Name;
	}

	public virtual Transform GetAvatar()
	{
		return AvAvatar.mTransform;
	}

	public virtual Transform GetAvatarTarget()
	{
		return null;
	}

	public virtual Camera GetCamera()
	{
		return Camera.main;
	}

	public virtual void TeleportTo(Vector3 position, Quaternion rotation, bool spawnTeleportEffect = true)
	{
		base.transform.position = position;
	}

	public virtual void FallToGround()
	{
		Vector3 position = base.transform.position;
		position.y += _GroundCheckStartHeight;
		if (UtUtilities.GetGroundHeight(position, _GroundCheckDist, out var groundHeight, out var _) != null)
		{
			position.y = groundHeight;
		}
		else
		{
			position.y = base.transform.position.y - _GroundCheckDist;
		}
		base.transform.position = position;
	}

	public MoveStateData GetMoveState(MoveStates state)
	{
		if (_MoveStates != null)
		{
			return _MoveStates.Find((MoveStateData item) => item._State == state);
		}
		return null;
	}

	public bool CanOnlyFly()
	{
		if (_MoveStates != null && GetMoveState(MoveStates.Air) != null)
		{
			return _MoveStates.Count == 1;
		}
		return false;
	}

	public bool HasSplineMovement()
	{
		navMove component = GetComponent<navMove>();
		if (component != null && component.pathContainer != null)
		{
			return true;
		}
		splineMove component2 = GetComponent<splineMove>();
		if (component2 != null && component2.pathContainer != null)
		{
			return true;
		}
		SplineControl component3 = GetComponent<SplineControl>();
		if (component3 != null && component3.SplineObject != null)
		{
			return true;
		}
		return false;
	}

	public void RunSplineMovement(bool run)
	{
		navMove component = GetComponent<navMove>();
		if (component != null && component.pathContainer != null)
		{
			if (run)
			{
				component.StartMove();
			}
			else
			{
				component.Stop();
			}
			return;
		}
		splineMove component2 = GetComponent<splineMove>();
		if (component2 != null && component2.pathContainer != null)
		{
			if (run)
			{
				component2.StartMove();
			}
			else
			{
				component2.Stop();
			}
			return;
		}
		SplineControl component3 = GetComponent<SplineControl>();
		if (component3 != null && component3.SplineObject != null)
		{
			if (run)
			{
				component3.ResetSpline();
			}
			else
			{
				component3.SetSpline(null);
			}
		}
	}
}
