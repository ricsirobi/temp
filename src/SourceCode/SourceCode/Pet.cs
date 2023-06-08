using System;
using UnityEngine;

public class Pet : ChCharacter
{
	[Serializable]
	public class PetFollowArray
	{
		public float Speed;

		public float FarDistance;

		public PetFollowArray(float speed, float distance)
		{
			Speed = speed;
			FarDistance = distance;
		}
	}

	public bool _FollowAvatar = true;

	public bool _FollowFront;

	public float _AvatarDistanceCheckInterval = 5f;

	public float _TeleportToAvatarDistanceThreshold = 20f;

	public float _MaxHeightDiffWithAvatar = 1.5f;

	public string _AnimNameIdle = "Idle";

	public string _AnimNameSwim = "Swim";

	public string _AnimNameRun = "Run";

	public string _AnimNameSleep = "Sleep";

	public string _AnimNameWalk = "Walk";

	[NonSerialized]
	public string _IdleAnimName = "";

	public float _IdleAnimSpeed = 1f;

	public static float pAvatarOffSetOffSetNow;

	public bool _MoveToDone;

	public float _HeightTweenSpeed = 1.5f;

	public float _StartOffsetFront;

	public float _StartOffsetRear = 100f;

	public float _FollowFrontDistance = 2.5f;

	public float _FollowRearDistance = 1.5f;

	public float _TweenTime = 1f;

	public PetFollowArray[] _PetFollowArray;

	public float _UpdateAvatarTargetPositionDelayRate = 0.2f;

	public float _AvatarTargetPositionDistanceOffset = 0.1f;

	public float _MinPettingTimer = 4f;

	[NonSerialized]
	public bool pIsPetting;

	public Transform mAvatar;

	public Transform mAvatarTarget;

	protected AvAvatarController mAvatarController;

	protected bool mMoveToAvatarPostponed;

	protected Vector3 mAvatarLookAt = Vector3.forward;

	protected float mAvatarOffsetSign = 1f;

	protected float mAvatarOffsetOffset;

	protected GameObject mWaterObject;

	protected bool mCanBePetted;

	protected bool mPettingAllowed;

	protected GameObject[] mPettingBones;

	protected int mNumPettingBones;

	protected float mPettingTimer;

	protected Vector3 mOldPettingMousePos = new Vector3(0f, 0f, 0f);

	protected bool mMouseOverPettingParts;

	protected string mPettingPartName = "";

	protected float mPrevHeight;

	private bool mTweenTimeSet;

	private int mPathIndex = -1;

	private float mTweenTime;

	protected Vector3 mCurrentGroundNormal = Vector3.up;

	private Vector3 mAvatarPrevPos = new Vector3(-100f, -100f, -100f);

	private float mNextUpdateAvatarTargetPostionTime;

	public GameObject pWaterObject
	{
		get
		{
			return mWaterObject;
		}
		set
		{
			mWaterObject = value;
		}
	}

	public virtual void Initialize(Camera cam, int numPettingBones)
	{
		Initialize(cam);
		mAvatarOffsetOffset = pAvatarOffSetOffSetNow;
		if (numPettingBones > 0)
		{
			mPettingBones = new GameObject[numPettingBones];
		}
		if (_PetFollowArray == null || _PetFollowArray.Length == 0)
		{
			_PetFollowArray = new PetFollowArray[3];
			_PetFollowArray[0] = new PetFollowArray(0.4f, 5f);
			_PetFollowArray[1] = new PetFollowArray(1f, 12f);
			_PetFollowArray[2] = new PetFollowArray(1.5f, 20f);
		}
	}

	public virtual void SetFollowAvatar(bool t)
	{
		_FollowAvatar = t;
		SetState(Character_State.idle);
	}

	public void HideCurrentPet(bool t, Vector3 pos)
	{
		base.transform.position = pos;
		HideCurrentPet(t);
	}

	public void HideCurrentPet(bool t)
	{
		if (t)
		{
			base.gameObject.SetActive(value: false);
			UtUtilities.HideObject(base.gameObject, t: true);
			return;
		}
		base.gameObject.SetActive(value: true);
		MoveToAvatar();
		UtUtilities.HideObject(base.gameObject, t: false);
		SetState(Character_State.idle);
	}

	public override void MoveToDone(Transform target)
	{
		mTweenTimeSet = false;
		mPathIndex = -1;
		_MoveToDone = true;
		base.MoveToDone(target);
		FallToGround();
		if (target == mAvatarTarget)
		{
			SetState(Character_State.idle);
		}
	}

	public override void SetPosOnSpline(float p)
	{
		Vector3 position = base.transform.position;
		mPrevHeight = position.y;
		base.SetPosOnSpline(p);
		position.x = base.transform.position.x;
		position.z = base.transform.position.z;
		base.transform.position = position;
		FallToGround();
	}

	public override float GetGroundHeight()
	{
		Vector3 position = base.transform.position;
		position.y += GroundCheckStartHeight;
		if (UtUtilities.GetGroundHeight(position, GroundCheckDist, out var groundHeight, out var _) != null)
		{
			return groundHeight;
		}
		return 0f;
	}

	protected float TweenY(float targetY, float tweenspeed)
	{
		float num = mPrevHeight;
		float num2 = targetY - num;
		float num3 = 1f;
		float num4 = num2;
		if (num4 < 0f)
		{
			num4 = 0f - num4;
		}
		if (num4 > 5f)
		{
			num3 += num4 / 10f;
		}
		if (targetY > num)
		{
			num += Time.deltaTime * _HeightTweenSpeed * num3;
			if (num > targetY)
			{
				num = targetY;
			}
		}
		else if (targetY < num)
		{
			num -= Time.deltaTime * _HeightTweenSpeed * num3;
			if (num < targetY)
			{
				num = targetY;
			}
		}
		return num;
	}

	public virtual void FallToGround()
	{
		Vector3 position = base.transform.position;
		position.y += GroundCheckStartHeight;
		if (mIsFlying && mMoveToTransform != null && mMoveToTransform.position.y > position.y + GroundCheckStartHeight)
		{
			position.y = GroundCheckStartHeight + mMoveToTransform.position.y;
		}
		float groundHeight;
		Vector3 normal;
		Collider groundHeight2 = UtUtilities.GetGroundHeight(position, GroundCheckDist, out groundHeight, out normal);
		if (groundHeight2 != null)
		{
			if (mIsFlying)
			{
				position.y = TweenY(groundHeight + mCurHeightOff, _HeightTweenSpeed);
			}
			else if (_CanFly && _Hover)
			{
				position.y = groundHeight + _HoverHeight;
			}
			else
			{
				position.y = groundHeight;
			}
			if (groundHeight2.gameObject.CompareTag("NoOrient"))
			{
				SetGroundNormal(Vector3.up);
			}
			else
			{
				SetGroundNormal(normal);
			}
		}
		else
		{
			SetGroundNormal(Vector3.up);
			if (mMoveToTransform != null)
			{
				position.y = TweenY(mMoveToTransform.position.y + mCurHeightOff, _HeightTweenSpeed);
			}
			else
			{
				position.y = base.transform.position.y - GroundCheckDist;
			}
		}
		base.transform.position = position;
		if (groundHeight2 != null)
		{
			CheckSwim(groundHeight2);
		}
	}

	public void SetGroundNormal(Vector3 norm)
	{
		mCurrentGroundNormal = norm;
	}

	public Vector3 GetGroundNormal()
	{
		return mCurrentGroundNormal;
	}

	public virtual void UpdateAvatarTargetPosition()
	{
		if (mAvatar != null && mAvatarTarget == null && mAvatar.gameObject.activeInHierarchy)
		{
			return;
		}
		float num = 1f;
		float num2 = 0f;
		if (_FollowFront)
		{
			num = _FollowFrontDistance;
			num2 = _StartOffsetFront;
			mAvatarLookAt = Vector3.zero;
		}
		else
		{
			num = _FollowRearDistance;
			num2 = _StartOffsetRear;
			mAvatarLookAt = Vector3.forward;
		}
		num2 += mAvatarOffsetOffset;
		if (mAvatar != null && mAvatarTarget != null)
		{
			if (UtUtilities.FindPosNextToObject(out var outPos, mAvatar.gameObject, num, 4f, ref num2, _AvatarDistanceCheckInterval, _MaxHeightDiffWithAvatar, 0f))
			{
				mAvatarTarget.position = outPos;
				Vector3 worldPosition = mAvatar.position + mAvatar.TransformDirection(mAvatarLookAt);
				worldPosition.y = base.transform.position.y;
				mAvatarTarget.LookAt(worldPosition);
			}
			else
			{
				mAvatarTarget.position = mAvatar.position;
				mAvatarTarget.rotation = mAvatar.rotation;
			}
		}
	}

	public virtual void SetAvatar(Transform av0, bool SpawnTeleportEffect = true, bool teleportPet = true)
	{
		if ((mAvatar = av0) != null)
		{
			if (mAvatarTarget == null)
			{
				mAvatarTarget = new GameObject().transform;
				mAvatarTarget.name = "AvatarFollower";
			}
			mAvatarController = (AvAvatarController)mAvatar.GetComponent(typeof(AvAvatarController));
			if (mAvatarController == null)
			{
				mAvatarController = (AvAvatarController)mAvatar.GetComponentInChildren(typeof(AvAvatarController));
			}
			UpdateAvatarTargetPosition();
		}
	}

	public virtual void OnDisable()
	{
		if (mAvatarTarget != null)
		{
			UnityEngine.Object.Destroy(mAvatarTarget.gameObject);
			mAvatarTarget = null;
		}
	}

	public void MoveToAvatar(bool postponed)
	{
		mMoveToAvatarPostponed = postponed;
		if (!postponed)
		{
			MoveToAvatar();
		}
	}

	public void MoveToAvatar()
	{
		UpdateAvatarTargetPosition();
		if (AvAvatar.pAvatarCam == null)
		{
			mMoveToAvatarPostponed = true;
			return;
		}
		if (mAvatarTarget == null)
		{
			mAvatarTarget = new GameObject().transform;
			mAvatarTarget.name = "AvatarFollower";
			UpdateAvatarTargetPosition();
		}
		if (_CanFly && _Hover)
		{
			Vector3 position = mAvatarTarget.position;
			position.y += _HoverHeight;
			base.transform.position = position;
		}
		else
		{
			base.transform.position = mAvatarTarget.position;
		}
		FallToGround();
		if (mAvatar != null)
		{
			base.transform.LookAt(base.transform.position + mAvatar.TransformDirection(mAvatarLookAt));
		}
	}

	public Vector3 GetHeadPosition()
	{
		return mHeadBonesLookAtData[0].mHeadBone.position;
	}

	public override bool CheckTargetMovedAway()
	{
		bool num = base.CheckTargetMovedAway();
		if (!num)
		{
			float dist = Vector3.Distance(mMoveToTransform.position, base.transform.position);
			int num2 = FindPathIndex(dist);
			if (num2 >= 0 && num2 != mPathIndex)
			{
				mTweenTime = 0f;
				mTweenTimeSet = true;
				mPathIndex = num2;
			}
		}
		return num;
	}

	public override Character_Action_Result DoMoveTo(Transform obj, bool endAlign, bool fly)
	{
		_MoveToDone = false;
		if (mAvatar != null)
		{
			float num = Vector3.Distance(obj.position, base.transform.position);
			AvAvatarController component = mAvatar.root.gameObject.GetComponent<AvAvatarController>();
			if (num > _PetFollowArray[_PetFollowArray.Length - 1].FarDistance)
			{
				if (!(component != null))
				{
					TeleportToAvatar();
					return Character_Action_Result.done;
				}
				if ((component.OnGround() || component.pSubState == AvAvatarSubState.SWIMMING) && !fly && _FollowAvatar)
				{
					TeleportToAvatar();
					return Character_Action_Result.done;
				}
			}
			int num2 = FindPathIndex(num);
			if (num2 >= 0 && num2 != mPathIndex)
			{
				mTweenTime = 0f;
				mTweenTimeSet = true;
				mPathIndex = num2;
			}
		}
		return base.DoMoveTo(obj, endAlign, fly);
	}

	public int FindPathIndex(float dist)
	{
		for (int i = 0; i < _PetFollowArray.Length; i++)
		{
			if (dist < _PetFollowArray[i].FarDistance)
			{
				return i;
			}
		}
		return -1;
	}

	public void TeleportToAvatar(bool SpawnTeleportEffect = true)
	{
		if (!(mAvatarTarget == null))
		{
			TeleportTo(mAvatarTarget.position, mAvatarTarget.rotation, SpawnTeleportEffect);
		}
	}

	public void TeleportTo(Vector3 Pos, Quaternion Rot, bool SpawnTeleportEffect = true)
	{
		SetPosition(Pos);
		FallToGround();
		base.transform.rotation = Rot;
		if (SpawnTeleportEffect)
		{
			TeleportFx.PlayAt(Pos, inPlaySound: false);
		}
		SetState(Character_State.idle);
		ResetOrientationToHorizontal();
	}

	public override void Update()
	{
		base.Update();
		switch (mState)
		{
		case Character_State.idle:
			if (mCanBePetted)
			{
				CheckPetting();
			}
			break;
		case Character_State.petting:
			if (mCanBePetted)
			{
				UpdatePetting();
			}
			break;
		}
		if (mAvatar != null && mTweenTimeSet)
		{
			AvAvatarController component = mAvatar.root.gameObject.GetComponent<AvAvatarController>();
			mTweenTime += Time.deltaTime;
			if (mTweenTime > _TweenTime)
			{
				mTweenTime = 0f;
				mTweenTimeSet = false;
				if (component != null)
				{
					SetMoveSpeed(_PetFollowArray[mPathIndex].Speed * component.pCurrentStateData._MaxForwardSpeed);
				}
				else
				{
					SetMoveSpeed(_PetFollowArray[mPathIndex].Speed * _RunSpeed);
				}
			}
			else if (component != null)
			{
				SetMoveSpeed(Mathf.Lerp(mOldSpeed, _PetFollowArray[mPathIndex].Speed * component.pCurrentStateData._MaxForwardSpeed, mTweenTime / _TweenTime));
			}
			else
			{
				SetMoveSpeed(Mathf.Lerp(mOldSpeed, _PetFollowArray[mPathIndex].Speed * _RunSpeed, mTweenTime / _TweenTime));
			}
		}
		if (mAvatarTarget != null && mAvatar != null && Time.time > mNextUpdateAvatarTargetPostionTime)
		{
			Vector3 position = mAvatar.gameObject.transform.position;
			if ((position - mAvatarPrevPos).sqrMagnitude > _AvatarTargetPositionDistanceOffset)
			{
				UpdateAvatarTargetPosition();
				mAvatarPrevPos = position;
			}
			mNextUpdateAvatarTargetPostionTime = Time.time + _UpdateAvatarTargetPositionDelayRate;
		}
	}

	public override string GetIdleAnimationName()
	{
		return _IdleAnimName;
	}

	public override bool IsAnimIdle(string aname, out bool lookatcam)
	{
		lookatcam = false;
		if (aname == GetIdleAnimationName())
		{
			return true;
		}
		return false;
	}

	private void OnTriggerEnter(Collider c)
	{
		CheckSwim(c);
	}

	private void OnTriggerStay(Collider c)
	{
		CheckSwim(c);
	}

	public virtual void CheckSwim(Collider c)
	{
		int num = LayerMask.NameToLayer("Water");
		if (c.gameObject.layer == num)
		{
			if (mWaterObject == null && base.transform.position.y - c.transform.position.y <= 0.001f)
			{
				mWaterObject = c.gameObject;
				if (mState == Character_State.idle)
				{
					PlayIdleAnimation();
				}
				else if (mState == Character_State.move)
				{
					PlayMoveAnimation();
				}
				ApplySwim(apply: true);
			}
		}
		else if (mWaterObject != null)
		{
			mWaterObject = null;
			if (mState == Character_State.idle)
			{
				PlayIdleAnimation();
			}
			else if (mState == Character_State.move)
			{
				PlayMoveAnimation();
			}
			ApplySwim(apply: false);
		}
	}

	public virtual void ApplySwim(bool apply)
	{
		_IdleAnimName = (apply ? _AnimNameSwim : _AnimNameIdle);
	}

	public bool FollowingAvatar()
	{
		if (_FollowAvatar && mAvatar != null)
		{
			return true;
		}
		return false;
	}

	public void ResetOrientationToHorizontal()
	{
		Vector3 position = base.transform.position;
		Vector3 worldPosition = position + base.transform.forward;
		worldPosition.y = position.y;
		base.transform.LookAt(worldPosition);
	}

	public void SetCanBePetted(bool t)
	{
		if (t)
		{
			mCanBePetted = t;
			if (mState == Character_State.idle)
			{
				EnablePetting(t: true);
			}
		}
		else
		{
			EnablePetting(t: false);
			mCanBePetted = t;
		}
	}

	public virtual void EnablePetting(bool t)
	{
		if (!mCanBePetted)
		{
			return;
		}
		if (!t)
		{
			if (pIsPetting)
			{
				ProcessPettingEnded(mPettingPartName);
			}
			if (mState == Character_State.petting)
			{
				SetState(Character_State.idle);
			}
		}
		mPettingAllowed = t;
		int num = 0;
		for (num = 0; num < mNumPettingBones; num++)
		{
			if (mPettingBones[num] != null)
			{
				mPettingBones[num].SetActive(t);
			}
		}
	}

	public void SetPettingBone(string bname, string cname, Vector3 offset, float r)
	{
		Transform transform = FindBoneTransform(bname);
		if (transform == null)
		{
			UtDebug.LogError("Dragon petting bone " + bname + " not found.");
			return;
		}
		GameObject gameObject = new GameObject(cname);
		gameObject.layer = LayerMask.NameToLayer("IgnoreGroundRay");
		SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
		mPettingBones[mNumPettingBones] = gameObject;
		mNumPettingBones++;
		sphereCollider.radius = r;
		AttachObject(transform, gameObject.transform, offset, Vector3.one);
		gameObject.SetActive(mPettingAllowed);
	}

	protected virtual bool DoPettingAction(string partname)
	{
		return true;
	}

	protected virtual void ProcessPettingEnded(string partname)
	{
		mPettingPartName = "";
		pIsPetting = false;
		if (base.animation != null)
		{
			base.animation.wrapMode = WrapMode.ClampForever;
		}
		SetState(Character_State.idle);
	}

	protected virtual void PettingStarted(string partname)
	{
		mOldPettingMousePos = Input.mousePosition;
		pIsPetting = true;
		SetState(Character_State.petting);
		mPettingTimer = _MinPettingTimer;
		mOldPettingMousePos = Input.mousePosition;
		EnablePetting(t: true);
	}

	protected virtual bool ProcessPetted(string partname)
	{
		bool num = mPettingPartName == "";
		mPettingPartName = partname;
		if (num)
		{
			PettingStarted(mPettingPartName);
		}
		if (DoPettingAction(partname))
		{
			return true;
		}
		return false;
	}

	protected virtual bool UpdatePetting()
	{
		if (!mCanBePetted)
		{
			return false;
		}
		if (CheckPetting())
		{
			if (mPettingTimer > 0f)
			{
				mPettingTimer -= Time.deltaTime;
				if (mPettingTimer <= 0f)
				{
					ProcessPettingEnded(mPettingPartName);
				}
				mOldPettingMousePos = Input.mousePosition;
			}
			return true;
		}
		ProcessPettingEnded(mPettingPartName);
		return false;
	}

	protected virtual bool CheckPetting()
	{
		if (mPettingAllowed && KAUI._GlobalExclusiveUI == null)
		{
			Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);
			mMouseOverPettingParts = false;
			if (Input.GetMouseButton(0))
			{
				GameObject[] array = mPettingBones;
				foreach (GameObject gameObject in array)
				{
					if ((bool)gameObject && gameObject.GetComponent<Collider>().Raycast(ray, out var hitInfo, 100f))
					{
						return ProcessPetted(hitInfo.collider.name);
					}
				}
			}
		}
		mPettingPartName = "";
		return false;
	}

	public override void SetState(Character_State newstate)
	{
		if (newstate != Character_State.petting && mLastAnimName.Length > 0 && !IsAnimIdle(mLastAnimName, out var _))
		{
			mNumLoops = 0;
		}
		switch (newstate)
		{
		case Character_State.idle:
			EnablePetting(t: true);
			break;
		case Character_State.action:
			EnablePetting(t: false);
			break;
		case Character_State.move:
			EnablePetting(t: false);
			break;
		}
		base.SetState(newstate);
	}
}
