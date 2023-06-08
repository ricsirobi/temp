using System.Collections.Generic;
using UnityEngine;

public class ChCharacter : SplineControl
{
	public class LookAtHeadBoneData
	{
		public Transform mHeadBone;

		public Vector3 mPrevAt;

		public Quaternion mOldHeadBoneRotation;

		public Quaternion mOriginalRotation;

		public LookAtHeadBoneData(Transform headBone, Vector3 prevPos, Quaternion oldRotation)
		{
			mHeadBone = headBone;
			mPrevAt = prevPos;
			mOldHeadBoneRotation = oldRotation;
			mOriginalRotation = oldRotation;
		}
	}

	public SFXMap[] _SoundMapper;

	protected Camera mCamera;

	public string _Name = "";

	public float _CrossFadeTime = 0.5f;

	protected Character_State mState;

	protected int mMood;

	protected string mLastAnimName = "";

	protected float mAnimLength = 1f;

	protected int mNumLoops;

	protected string mIdleAnimName = "";

	public float _FidgetTime = 10f;

	protected float mFidgetTimer = 10f;

	protected bool mFidgetOn = true;

	protected Character_Action mAction = Character_Action.noAction;

	protected Transform mActionObject;

	public float _MaxAngle = 0.1f;

	public float _HeadRotateSpeed = 2f;

	protected Vector3 mLookAtPos;

	protected bool mLookAt;

	protected Transform mLookAtObj;

	protected Vector3 mLookAtOffset = new Vector3(0f, 0f, 0f);

	protected bool mDoTween;

	protected bool mTweenOutDone = true;

	protected Vector3 mTweenOutAt = new Vector3(0f, 0f, 0f);

	protected bool mResetHeadBone;

	public bool _Debug;

	public bool _CanFly;

	public float _WalkSpeed = 2f;

	public float _RunSpeed = 5f;

	public float _FlySpeed = 5f;

	public float _TakeOffSpeed = 2f;

	public float _FlyHeight = 2f;

	public float _JumpHeight = 0.5f;

	public float _MaxFlySpeed = 40f;

	public bool _VarSpeed;

	public float _VarMinDist = 3.2f;

	public float _MaxRunSpeed = 20f;

	public float _MovePathUpdateTimerInt = 0.5f;

	public bool _Move2D = true;

	public float _MoveEndRange = 0.3f;

	public bool _Hover;

	public float _HoverHeight = 1.5f;

	protected Transform mMoveToTransform;

	protected bool mMoveToReached;

	protected Vector3 mMoveFromPosition = new Vector3(0f, 0f, 0f);

	protected Vector3 mMoveToPosition = new Vector3(0f, 0f, 0f);

	protected float mMovePathUpdateTimer = 0.5f;

	protected bool mEndAlign = true;

	protected bool mIsFlying;

	protected Character_FlyStage mFlyStage;

	protected bool mTakingOff;

	protected float mCurHeightOff;

	protected bool mLanding;

	protected float mOldSpeed;

	protected float mMoveSpeed;

	protected List<LookAtHeadBoneData> mHeadBonesLookAtData;

	private AudioSource mAudio;

	public List<LookAtHeadBoneData> pHeadBonesLookAtData => mHeadBonesLookAtData;

	public virtual void Initialize(Camera cam)
	{
		mCamera = cam;
		SetState(Character_State.idle);
	}

	public void SetCamera(Camera cam)
	{
		mCamera = cam;
	}

	public Camera GetCamera()
	{
		return mCamera;
	}

	public void OnSFXResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("SFX load failed : " + inURL);
			break;
		case RsResourceLoadEvent.PROGRESS:
		case RsResourceLoadEvent.COMPLETE:
			break;
		}
	}

	public virtual void PlayAnimSFX(string aname, bool looping)
	{
		if (mAudio == null)
		{
			mAudio = base.audio;
		}
		if (mAudio == null)
		{
			return;
		}
		int i = 0;
		for (int num = _SoundMapper.Length; i < num; i++)
		{
			SFXMap sFXMap = _SoundMapper[i];
			if (!(aname == sFXMap._AnimName))
			{
				continue;
			}
			if (sFXMap._ClipRes == null && sFXMap._ClipResName != null && sFXMap._ClipResName.Length > 0)
			{
				sFXMap._ClipRes = (AudioClip)RsResourceManager.LoadAssetFromBundle(sFXMap._ClipResName);
				if (sFXMap._ClipRes == null)
				{
					string[] array = sFXMap._ClipResName.Split('/');
					if (array.Length == 3)
					{
						RsResourceManager.Load(array[0] + "/" + array[1], OnSFXResLoadingEvent, RsResourceType.NONE, inDontDestroy: true);
					}
				}
			}
			if (sFXMap._ClipRes != null)
			{
				mAudio.clip = sFXMap._ClipRes;
				mAudio.loop = looping;
				mAudio.Play();
			}
			else
			{
				mAudio.Stop();
			}
			return;
		}
		mAudio.Stop();
	}

	public void SetName(string n)
	{
		_Name = n;
	}

	public string GetName()
	{
		return _Name;
	}

	public void DisableAllEmitters()
	{
		ParticleSystem[] componentsInChildren = base.gameObject.GetComponentsInChildren<ParticleSystem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Stop();
		}
	}

	public void SetParticleColor(string emitterName, Color c, int maxIdx)
	{
		Transform transform = FindBoneTransform(emitterName);
		if (!(transform == null))
		{
			ParticleSystem.MainModule main = transform.GetComponent<ParticleSystem>().main;
			main.startColor = c;
		}
	}

	public void SetEmitter(string ename, bool et)
	{
		Transform transform = FindBoneTransform(ename);
		if (!(transform != null))
		{
			return;
		}
		ParticleSystem component = transform.GetComponent<ParticleSystem>();
		if (component != null)
		{
			if (et)
			{
				component.Play();
			}
			else
			{
				component.Stop();
			}
		}
	}

	public Transform GetMoveToTransform()
	{
		return mMoveToTransform;
	}

	public Transform FindBoneTransform(string bname)
	{
		return UtUtilities.FindChildTransform(base.gameObject, bname);
	}

	public void AttachObject(Transform boneT, Transform cobj, Vector3 offset, Vector3 scale)
	{
		cobj.parent = boneT;
		cobj.localScale = scale;
		cobj.localPosition = offset;
		cobj.localRotation = Quaternion.identity;
	}

	public virtual void ChangeTexture(Texture2D t)
	{
	}

	public virtual int GetMoodValue()
	{
		return mMood;
	}

	public virtual bool HasMood(int mood, Character_Mood m)
	{
		return ((uint)mood & (uint)m) != 0;
	}

	public virtual bool HasMood(Character_Mood m)
	{
		return ((uint)mMood & (uint)m) != 0;
	}

	public virtual string GetMoodString(int mood)
	{
		string text = "";
		if (HasMood(mood, Character_Mood.firedup))
		{
			text += "firedup ";
		}
		if (HasMood(mood, Character_Mood.happy))
		{
			text += "happy ";
		}
		if (HasMood(mood, Character_Mood.angry))
		{
			text += "angry ";
		}
		if (HasMood(mood, Character_Mood.hungry))
		{
			text += "hungry ";
		}
		if (HasMood(mood, Character_Mood.full))
		{
			text += "full ";
		}
		if (HasMood(mood, Character_Mood.tired))
		{
			text += "tired ";
		}
		return text;
	}

	public virtual void SetMood(Character_Mood m, bool t)
	{
		if (t)
		{
			switch (m)
			{
			case Character_Mood.firedup:
				mMood &= -3;
				mMood &= -5;
				break;
			case Character_Mood.happy:
				mMood &= -2;
				mMood &= -5;
				break;
			case Character_Mood.angry:
				mMood &= -2;
				mMood &= -3;
				break;
			}
			mMood |= (int)m;
		}
		else
		{
			mMood &= (int)(~m);
		}
	}

	public void SetBoneScaleFullPath(string bname, Vector3 s)
	{
		Transform transform = base.transform.Find(bname);
		if (transform != null)
		{
			transform.localScale = s;
		}
		else
		{
			Debug.LogError(base.name + " Bone not found " + bname);
		}
	}

	public void SetBoneScale0(string bname, Vector3 s)
	{
		Transform transform = FindBoneTransform(bname);
		if (transform != null)
		{
			transform.localScale = s;
		}
		else
		{
			Debug.LogError(base.name + " Bone not found " + bname);
		}
	}

	public virtual LookAtHeadBoneData SetHeadBoneFind(string bname)
	{
		Transform transform = FindBoneTransform(bname);
		if (transform == null)
		{
			UtDebug.LogError("Dragon head bone not found.");
			return null;
		}
		return SetHeadBone(transform);
	}

	public LookAtHeadBoneData SetHeadBone(string bname)
	{
		Transform transform = base.transform.Find(bname);
		if (transform == null)
		{
			UtDebug.LogError("Dragon head bone not found.");
			return null;
		}
		return SetHeadBone(transform);
	}

	public LookAtHeadBoneData SetHeadBone(Transform headBone)
	{
		if (mHeadBonesLookAtData == null)
		{
			mHeadBonesLookAtData = new List<LookAtHeadBoneData>();
		}
		LookAtHeadBoneData lookAtHeadBoneData = mHeadBonesLookAtData.Find((LookAtHeadBoneData inData) => (inData != null && inData.mHeadBone != null && inData.mHeadBone == headBone) ? true : false);
		if (lookAtHeadBoneData == null)
		{
			lookAtHeadBoneData = new LookAtHeadBoneData(headBone, Vector3.zero, headBone.localRotation);
			mHeadBonesLookAtData.Add(lookAtHeadBoneData);
		}
		return lookAtHeadBoneData;
	}

	public virtual void StopLookAtObject()
	{
		mLookAt = false;
		mLookAtObj = null;
		mResetHeadBone = true;
	}

	public virtual void SetLookAtObject(Transform obj, bool tween, Vector3 offset)
	{
		if ((obj != null && mLookAtObj == obj) || (obj == null && !mLookAt))
		{
			return;
		}
		mLookAtObj = obj;
		mLookAtOffset = offset;
		mDoTween = tween;
		if (!mLookAt && mHeadBonesLookAtData != null && mHeadBonesLookAtData.Count > 0)
		{
			foreach (LookAtHeadBoneData mHeadBonesLookAtDatum in mHeadBonesLookAtData)
			{
				if (mHeadBonesLookAtDatum != null && mHeadBonesLookAtDatum.mHeadBone != null)
				{
					mHeadBonesLookAtDatum.mPrevAt = mHeadBonesLookAtDatum.mHeadBone.forward;
				}
			}
		}
		mLookAt = obj != null;
		if (mDoTween)
		{
			if (obj == null)
			{
				mTweenOutDone = false;
				mResetHeadBone = true;
			}
			else
			{
				mTweenOutDone = true;
			}
		}
		else
		{
			mTweenOutDone = true;
		}
	}

	public virtual void SetLookAt(Vector3 pos, bool tween)
	{
		mLookAtObj = null;
		mLookAt = true;
		mLookAtPos = pos;
		mDoTween = tween;
		mTweenOutDone = true;
	}

	public virtual void SetHeadBoneLookAt(LookAtHeadBoneData headBoneData, Vector3 pos, Vector3 upv)
	{
		if (headBoneData != null && headBoneData.mHeadBone != null)
		{
			headBoneData.mHeadBone.LookAt(pos, upv);
		}
	}

	public virtual void TweenOutDone()
	{
	}

	public virtual void UpdateHeadBone(LookAtHeadBoneData headBoneData)
	{
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		if (mLookAt)
		{
			zero2 = ((!mLookAtObj) ? (mLookAtPos + mLookAtOffset) : (mLookAtObj.position + mLookAtOffset));
			zero = zero2 - headBoneData.mHeadBone.position;
			zero.Normalize();
			if (Vector3.Dot(zero, base.transform.forward) < _MaxAngle)
			{
				zero = headBoneData.mPrevAt;
			}
			else if (mDoTween)
			{
				zero = Vector3.Slerp(headBoneData.mPrevAt, zero, Time.deltaTime * _HeadRotateSpeed);
			}
		}
		else
		{
			if (mTweenOutDone)
			{
				headBoneData.mPrevAt = headBoneData.mHeadBone.forward;
				mTweenOutAt = Vector3.zero;
				if (mResetHeadBone)
				{
					mResetHeadBone = false;
					headBoneData.mHeadBone.localRotation = headBoneData.mOldHeadBoneRotation;
					TweenOutDone();
				}
				return;
			}
			mTweenOutDone = true;
			zero = Vector3.Slerp(headBoneData.mPrevAt, mTweenOutAt, Time.deltaTime * _HeadRotateSpeed);
			if ((double)Vector3.Dot(zero, mTweenOutAt) < 0.99)
			{
				mTweenOutDone = false;
			}
		}
		zero.Normalize();
		headBoneData.mPrevAt = zero;
		mTweenOutAt = headBoneData.mPrevAt;
		SetHeadBoneLookAt(headBoneData, headBoneData.mHeadBone.position + zero, Vector3.up);
	}

	public virtual void SetupAnimLayers()
	{
		bool lookatcam = false;
		foreach (AnimationState item in base.animation)
		{
			if (IsAnimIdle(item.name, out lookatcam))
			{
				item.layer = -1;
			}
		}
	}

	public bool IsAnimationPlaying(string aname)
	{
		if (base.animation != null)
		{
			if (base.animation.IsPlaying(aname))
			{
				if (base.animation[aname].wrapMode != WrapMode.Loop)
				{
					return base.animation[aname].normalizedTime <= 1f;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public string GetCurrentAnimation()
	{
		return mLastAnimName;
	}

	public void StopAnimAtLoopEnd(string aname)
	{
		if (IsAnimationPlaying(aname))
		{
			base.animation[aname].wrapMode = WrapMode.ClampForever;
		}
	}

	public virtual void PlayAnim(string aname)
	{
		PlayAnim(aname, 0, 1f, 1);
	}

	public virtual void PlayAnim(string aname, int numLoops, float aspeed, int alayer)
	{
		if (base.animation == null || string.IsNullOrEmpty(aname))
		{
			return;
		}
		if (base.animation[aname] == null)
		{
			UtDebug.LogWarning("Anim [" + aname + "] doesn't exist on " + base.name);
			return;
		}
		mLastAnimName = aname;
		mNumLoops = numLoops;
		mAnimLength = base.animation[aname].length;
		if (numLoops == 0)
		{
			base.animation[aname].wrapMode = WrapMode.ClampForever;
		}
		else
		{
			base.animation[aname].wrapMode = WrapMode.Loop;
		}
		if (aspeed > 0.001f)
		{
			base.animation[aname].speed = aspeed;
		}
		base.animation.CrossFade(aname, _CrossFadeTime);
		if (_SoundMapper != null)
		{
			PlayAnimSFX(aname, numLoops != 0);
		}
	}

	private void CheckAnimLoop()
	{
		if (base.animation == null || mNumLoops <= 0)
		{
			return;
		}
		float num = base.animation[mLastAnimName].time;
		while (num > mAnimLength)
		{
			num -= mAnimLength;
			mNumLoops--;
			if (num < mAnimLength)
			{
				base.animation[mLastAnimName].time = num;
			}
			if (mNumLoops == 0)
			{
				base.animation[mLastAnimName].wrapMode = WrapMode.ClampForever;
				base.animation[mLastAnimName].time = num;
				break;
			}
		}
	}

	protected virtual void UpdateAI()
	{
	}

	public void SetPosition(Vector3 pos)
	{
		base.transform.position = pos;
	}

	public virtual void GenerateMoveToPath()
	{
		if (mSpline == null)
		{
			mSpline = new Spline(10, looping: false, constSpeed: true, alignTangent: true, hasQ: false);
		}
		mSpline.GeneratePath(base.transform, mMoveToTransform, 0.5f, 0.2f, mEndAlign, base.transform.position.y);
		mSpline.GenerateTangents();
	}

	public virtual float GetGroundHeight()
	{
		return 0f;
	}

	public virtual Character_Action_Result DoMoveTo(Transform obj, bool endAlign, bool fly)
	{
		SetLookAtObject(null, tween: true, Vector3.zero);
		mMoveToTransform = obj;
		mEndAlign = endAlign;
		if (ObjectWithinRange(mMoveToTransform, 0f, infront: true))
		{
			MoveToDone(obj);
			return Character_Action_Result.done;
		}
		mMoveToReached = false;
		bool flag = mIsFlying;
		mIsFlying = fly;
		GenerateMoveToPath();
		mMovePathUpdateTimer = _MovePathUpdateTimerInt;
		mMoveToPosition = mMoveToTransform.position;
		if (fly && _Hover)
		{
			mMoveToPosition.y += _HoverHeight;
		}
		mMoveFromPosition = base.transform.position;
		Looping = false;
		CurrentPos = 0f;
		AlignTangent = false;
		if (mIsFlying)
		{
			float groundHeight = GetGroundHeight();
			if (!flag || mFlyStage != Character_FlyStage.fly)
			{
				if (groundHeight + _FlyHeight < base.transform.position.y || _Hover)
				{
					mFlyStage = Character_FlyStage.fly;
					Speed = _FlySpeed;
					mCurHeightOff = _FlyHeight;
					SetState(Character_State.move);
				}
				else
				{
					mFlyStage = Character_FlyStage.jump;
					mMoveFromPosition.y = groundHeight;
					mCurHeightOff = 0f;
					Speed = 0f;
					SetState(Character_State.move);
				}
			}
		}
		else
		{
			Speed = 0f;
			SetState(Character_State.move);
		}
		return Character_Action_Result.repeat;
	}

	public void SetMoveSpeed(float ms)
	{
		mOldSpeed = mMoveSpeed;
		mMoveSpeed = ms;
	}

	public virtual void MoveToDone(Transform target)
	{
		mIsFlying = false;
		mOldSpeed = 0f;
		mMoveSpeed = 0f;
		Speed = 0f;
		mEndReached = false;
		mMoveToTransform = null;
		SetLookAtObject(null, tween: true, Vector3.zero);
		if (mEndAlign && target != null)
		{
			base.transform.position = target.position;
			base.transform.rotation = target.rotation;
		}
	}

	public virtual bool ObjectWithinRange2D(Transform tform, float rangeModifier)
	{
		Vector3 position = tform.position;
		Vector3 position2 = base.transform.position;
		position.y = 0f;
		position2.y = 0f;
		return Vector3.Distance(position, position2) < _MoveEndRange + rangeModifier;
	}

	public virtual bool ObjectWithinRange(Transform tform, float rangeModifier, bool infront)
	{
		if (_Move2D && !mIsFlying)
		{
			Vector3 position = tform.position;
			Vector3 position2 = base.transform.position;
			position.y = 0f;
			position2.y = 0f;
			return Vector3.Distance(position, position2) < _MoveEndRange + rangeModifier;
		}
		Vector3 position3 = tform.position;
		if (_CanFly && _Hover)
		{
			position3.y += _HoverHeight;
		}
		return Vector3.Distance(position3, base.transform.position) < _MoveEndRange + rangeModifier;
	}

	public virtual string GetFlutterAnim()
	{
		return "Flutter02";
	}

	public virtual string GetFlyAnim()
	{
		return "Fly";
	}

	public virtual void PlayFlyAnimation(Character_FlyStage stage)
	{
		switch (stage)
		{
		case Character_FlyStage.jump:
			PlayAnim("Jump", 0, 1f, 0);
			break;
		case Character_FlyStage.flutter:
			PlayAnim(GetFlutterAnim(), -1, 1f, 0);
			break;
		case Character_FlyStage.fly:
			PlayAnim(GetFlyAnim(), -1, 1f, 0);
			break;
		case Character_FlyStage.landing:
			PlayAnim(GetFlutterAnim(), -1, 1f, 0);
			break;
		}
	}

	public virtual void PlayMoveAnimation()
	{
		if (mMoveSpeed >= _RunSpeed)
		{
			PlayAnim("Run", -1, 1f, 0);
		}
		else
		{
			PlayAnim("Walk", -1, 1f, 0);
		}
	}

	public virtual bool CheckTargetMovedAway()
	{
		mMovePathUpdateTimer -= Time.deltaTime;
		if (mMovePathUpdateTimer <= 0f)
		{
			mMovePathUpdateTimer = _MovePathUpdateTimerInt;
			if (Vector3.Distance(mMoveToPosition, mMoveToTransform.position) > _MoveEndRange)
			{
				DoMoveTo(mMoveToTransform, mEndAlign, mIsFlying);
				return true;
			}
		}
		return false;
	}

	private bool CheckMoveToDone()
	{
		if (mMoveToTransform != null && ObjectWithinRange(mMoveToTransform, 0f, infront: false))
		{
			MoveToDone(mMoveToTransform);
			return true;
		}
		return false;
	}

	public virtual void UpdateMoveToPosition()
	{
		Vector3 position = base.transform.position;
		if (!(mMoveToTransform != null))
		{
			return;
		}
		if (mIsFlying)
		{
			switch (mFlyStage)
			{
			case Character_FlyStage.jump:
				position = mMoveFromPosition;
				mCurHeightOff += Time.deltaTime * _TakeOffSpeed;
				if (mCurHeightOff >= _JumpHeight)
				{
					mCurHeightOff = _JumpHeight;
					mFlyStage = Character_FlyStage.flutter;
					PlayFlyAnimation(mFlyStage);
				}
				position.y += mCurHeightOff;
				base.transform.position = position;
				break;
			case Character_FlyStage.flutter:
				position = mMoveFromPosition;
				mCurHeightOff += Time.deltaTime * _TakeOffSpeed;
				if (mCurHeightOff >= _FlyHeight)
				{
					mCurHeightOff = _FlyHeight;
					mFlyStage = Character_FlyStage.fly;
					PlayFlyAnimation(mFlyStage);
					Speed = _FlySpeed;
				}
				position.y += mCurHeightOff;
				base.transform.position = position;
				break;
			case Character_FlyStage.fly:
			{
				bool move2D = _Move2D;
				_Move2D = true;
				mIsFlying = false;
				bool num3 = ObjectWithinRange2D(mMoveToTransform, 0f);
				_Move2D = move2D;
				mIsFlying = true;
				if (num3)
				{
					Speed = 0f;
					mFlyStage = Character_FlyStage.landing;
					PlayFlyAnimation(mFlyStage);
					mCurHeightOff = 0f;
					break;
				}
				if (_VarSpeed)
				{
					float magnitude2 = (mMoveToPosition - position).magnitude;
					Speed = _FlySpeed;
					if (magnitude2 > _VarMinDist)
					{
						Speed += (magnitude2 - _VarMinDist) * 2f;
					}
					if (Speed > _MaxFlySpeed)
					{
						Speed = _MaxFlySpeed;
					}
				}
				CheckTargetMovedAway();
				break;
			}
			case Character_FlyStage.landing:
			{
				Vector3 vector = mMoveToPosition - position;
				float magnitude = vector.magnitude;
				if (magnitude < _MoveEndRange)
				{
					base.transform.position = mMoveToPosition;
					if (!CheckMoveToDone())
					{
						DoMoveTo(mMoveToTransform, mEndAlign, fly: true);
					}
					break;
				}
				Vector3 normalized = vector.normalized;
				float num = 1f;
				if (magnitude > 5f)
				{
					num += (magnitude - 5f) * 2f;
				}
				float num2 = Time.deltaTime * _TakeOffSpeed * num;
				if (num2 > magnitude)
				{
					num2 = magnitude;
				}
				position += normalized * num2;
				base.transform.position = position;
				break;
			}
			}
			return;
		}
		if (_VarSpeed)
		{
			float magnitude3 = (mMoveToTransform.position - position).magnitude;
			Speed = _RunSpeed;
			if (magnitude3 > _VarMinDist)
			{
				Speed += (magnitude3 - _VarMinDist) * 2f;
			}
			if (Speed > _MaxRunSpeed)
			{
				Speed = _MaxRunSpeed;
			}
		}
		else
		{
			Speed = mMoveSpeed;
		}
		CheckTargetMovedAway();
		CheckMoveToDone();
	}

	public Character_State GetState()
	{
		return mState;
	}

	public virtual void SetState(Character_State newstate)
	{
		Character_State character_State = mState;
		if (newstate != Character_State.petting && mLastAnimName.Length > 0 && !IsAnimIdle(mLastAnimName, out var _))
		{
			mNumLoops = 0;
		}
		if (mState == Character_State.move && newstate != Character_State.move)
		{
			MoveToDone(null);
		}
		mState = newstate;
		switch (mState)
		{
		case Character_State.idle:
			PlayIdleAnimation();
			ResetFidgetTimer();
			break;
		case Character_State.move:
			if (mIsFlying && character_State != Character_State.move)
			{
				PlayFlyAnimation(mFlyStage);
			}
			else
			{
				PlayMoveAnimation();
			}
			break;
		}
		if (character_State == Character_State.action && mAction != Character_Action.noAction)
		{
			Character_Action actionid = mAction;
			mAction = Character_Action.noAction;
			ActionDone(actionid, ended: false, mActionObject);
		}
	}

	public override void Update()
	{
		base.Update();
		DebugOutput();
		switch (mState)
		{
		case Character_State.idle:
			UpdateAI();
			RepeatIdle();
			break;
		case Character_State.action:
			RepeatAction();
			break;
		case Character_State.move:
			UpdateMoveToPosition();
			break;
		}
	}

	public virtual void LateUpdate()
	{
		if (mHeadBonesLookAtData == null || mHeadBonesLookAtData.Count <= 0)
		{
			return;
		}
		int i = 0;
		for (int count = mHeadBonesLookAtData.Count; i < count; i++)
		{
			LookAtHeadBoneData lookAtHeadBoneData = mHeadBonesLookAtData[i];
			if (lookAtHeadBoneData != null)
			{
				UpdateHeadBone(lookAtHeadBoneData);
			}
		}
	}

	public virtual string GetIdleAnimationName()
	{
		return "Idle";
	}

	public void SetHover(bool t)
	{
		_Hover = t;
		_ = mState;
		_ = 1;
	}

	public virtual string GetHoverIdleAnimationName()
	{
		return GetFlutterAnim();
	}

	public virtual bool IsAnimIdle(string aname, out bool lookatcam)
	{
		lookatcam = true;
		if (aname == "Idle")
		{
			return true;
		}
		return false;
	}

	public virtual void PlayIdleAnimation()
	{
		if (_CanFly && _Hover)
		{
			mIdleAnimName = GetHoverIdleAnimationName();
		}
		else
		{
			mIdleAnimName = GetIdleAnimationName();
		}
		PlayAnim(mIdleAnimName, -1, 1f, 0);
	}

	public void ResetFidgetTimer()
	{
		mFidgetTimer = _FidgetTime;
	}

	public void SetFidgetOnOff(bool isOn)
	{
		mFidgetOn = isOn;
		ResetFidgetTimer();
	}

	public virtual void DoFidget()
	{
	}

	public virtual void RepeatIdle()
	{
		if (IsAnimIdle(mLastAnimName, out var lookatcam))
		{
			if (mCamera != null && lookatcam)
			{
				SetLookAtObject(mCamera.transform, tween: true, Vector3.zero);
			}
			if (mFidgetOn)
			{
				mFidgetTimer -= Time.deltaTime;
				if (mFidgetTimer <= 0f)
				{
					ResetFidgetTimer();
					DoFidget();
				}
			}
		}
		else
		{
			CheckAnimLoop();
			if (!IsAnimationPlaying(mLastAnimName))
			{
				string aname = mLastAnimName;
				mLastAnimName = mIdleAnimName;
				ReturnToIdle(aname);
			}
		}
	}

	public virtual void ReturnToIdle(string aname)
	{
		PlayIdleAnimation();
	}

	public virtual void DoAction(Transform obj, Character_Action actionid)
	{
		mActionObject = obj;
		SetLookAtObject(null, tween: true, Vector3.zero);
		SetState(Character_State.action);
		mAction = actionid;
	}

	public virtual void RepeatAction()
	{
		CheckAnimLoop();
		if (!IsAnimationPlaying(mLastAnimName))
		{
			Character_Action actionid = mAction;
			mAction = Character_Action.noAction;
			SetState(Character_State.idle);
			Transform actionObj = mActionObject;
			mActionObject = null;
			ActionDone(actionid, ended: true, actionObj);
		}
	}

	public virtual void DoEat(Transform obj)
	{
		DoAction(obj, Character_Action.eat);
	}

	public virtual void ActionDone(Character_Action actionid, bool ended, Transform actionObj)
	{
	}

	public virtual void DebugOutput()
	{
	}
}
