using UnityEngine;

public class PsPet : Pet
{
	public Texture2D _SkinTexture;

	public string _AnimNameFidget01 = "Fidget01";

	public string _AnimNameFidget02 = "Fidget02";

	public string _AnimNameTrick360 = "Trick360";

	public string _AnimNameTrickDance = "TrickDance";

	public string _AnimNameTrickDoubleJump = "TrickDoubleJump";

	public string _AnimNameTrickForwardFlip = "TrickForwardFlip";

	public string _AnimNameTrickTurnStyle = "TrickTurnStyle";

	public string _Type;

	public string[] _AnimSeq;

	private int mNumAnimSeqLoop;

	public int mCurSeqIdx = -1;

	public string _PfName = "";

	public string _TexName = "";

	private bool mAvatarAirborne;

	public float _MaxHeightDiffWithAvatarGliding = 1500f;

	private AssetBundle mPetPfBundle;

	private AssetBundle mPetTxBundle;

	public PetDataPet mData;

	private bool mOnSpringBoardSpline;

	private bool mWaitingAtSpringBoard;

	private bool mbFollowingAvatarBeforeSlide = true;

	public float _SlideStartDelay = 0.25f;

	private bool mOnSlide;

	private float mSlideSpeed;

	private float mSlideEndFallDuration = 1f;

	private bool mTrickAnimPlaying;

	private float mLastTimeFallToGroundTest;

	private float mLastGroundHeight;

	private Collider mLastCollider;

	private void CreatePetFromBundle()
	{
		string[] array = mData.Geometry.Split('/');
		PsPet obj = (PsPet)Object.Instantiate(((GameObject)mPetPfBundle.LoadAsset(array[2])).transform, Vector3.zero, Quaternion.identity).gameObject.GetComponent(typeof(PsPet));
		obj._Name = mData.Name;
		obj._Type = mData.Type;
		obj._PfName = mData.Geometry;
		obj._TexName = mData.Texture;
		array = obj._TexName.Split('/');
		obj._SkinTexture = (Texture2D)mPetTxBundle.LoadAsset(array[2]);
		obj._FollowAvatar = true;
		obj.gameObject.name = "Pet";
		obj.SetHeadBone("MainRoot/Root/jChest/jShoulders/jHead");
		obj.SetAvatar(AvAvatar.mTransform);
		obj.mMoveToAvatarPostponed = false;
		obj.MoveToAvatar();
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			if (mPetPfBundle == null)
			{
				mPetPfBundle = (AssetBundle)inObject;
				string[] array = mData.Texture.Split('/');
				RsResourceManager.Load(array[0] + "/" + array[1], OnResLoadingEvent, RsResourceType.NONE, inDontDestroy: true);
			}
			else
			{
				mPetTxBundle = (AssetBundle)inObject;
				CreatePetFromBundle();
			}
		}
	}

	public void Init(PetDataPet pdata)
	{
		mData = pdata;
		mPetPfBundle = null;
		mPetTxBundle = null;
		string[] array = mData.Geometry.Split('/');
		RsResourceManager.Load(array[0] + "/" + array[1], OnResLoadingEvent, RsResourceType.NONE, inDontDestroy: true);
		if (_AvatarDistanceCheckInterval <= 0f)
		{
			_AvatarDistanceCheckInterval = 5f;
		}
	}

	public void SetData(PetDataPet petData)
	{
		mData = petData;
	}

	public void SetNoCurrentPet()
	{
		mData = null;
		PetData.SaveCurrent(null);
	}

	public void SaveAsCurrent()
	{
		if (mData == null)
		{
			mData = new PetDataPet();
		}
		mData.Geometry = _PfName;
		mData.Texture = _TexName;
		mData.Type = _Type;
		mData.Name = _Name;
		PetData.SaveCurrent(mData);
	}

	public override void Start()
	{
		if (_IdleAnimName.Length == 0)
		{
			_IdleAnimName = "NavStand";
		}
		Initialize(Camera.main, 0);
		if (_WalkSpeed == 1f)
		{
			_WalkSpeed = 5f;
			_RunSpeed = 10f;
		}
	}

	public override void UpdateAvatarTargetPosition()
	{
		if (!(mAvatar != null) || !(mAvatarTarget == null) || !mAvatar.gameObject.activeInHierarchy)
		{
			float num = 1f;
			float num2 = 0f;
			if (_FollowFront)
			{
				num = 1.5f;
				num2 = _StartOffsetFront;
				mAvatarLookAt = Vector3.zero;
			}
			else
			{
				num = 1.5f;
				num2 = _StartOffsetRear;
				mAvatarLookAt = Vector3.forward;
			}
			float inMaxHeightDiff = _MaxHeightDiffWithAvatar;
			if (mAvatarAirborne)
			{
				inMaxHeightDiff = _MaxHeightDiffWithAvatarGliding;
			}
			if (mAvatar != null && UtUtilities.FindPosNextToObject(out var outPos, mAvatar.gameObject, num, 4f, ref num2, 30f, inMaxHeightDiff, 0f))
			{
				mAvatarTarget.position = outPos;
				Vector3 worldPosition = mAvatar.position + mAvatar.TransformDirection(mAvatarLookAt);
				worldPosition.y = base.transform.position.y;
				mAvatarTarget.LookAt(worldPosition);
			}
		}
	}

	public void RemoveCurrentAvatarObserver()
	{
		if (mAvatar != null)
		{
			mAvatar.gameObject.GetComponent<AvAvatarController>().SetPet(null);
		}
	}

	public void AttachAvatarObserver()
	{
		if (mAvatar != null)
		{
			AvAvatarController component = mAvatar.gameObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.SetPet(this);
			}
		}
	}

	public override void SetAvatar(Transform av0, bool SpawnTeleportEffect = true, bool teleportPet = true)
	{
		RemoveCurrentAvatarObserver();
		base.SetAvatar(av0, SpawnTeleportEffect);
		AttachAvatarObserver();
	}

	public string GetPetType()
	{
		return _Type;
	}

	public override void DoFidget()
	{
		if (!(mWaterObject != null))
		{
			float value = Random.value;
			if (value < 0.3f)
			{
				PlayAnim(_AnimNameFidget01, 0, 1f, 1);
			}
			else if (value < 0.6f)
			{
				PlayAnim(_AnimNameFidget02, 0, 1f, 1);
			}
		}
	}

	public override void PlayMoveAnimation()
	{
		if (mWaterObject != null)
		{
			PlayAnim(_AnimNameSwim, -1, 1f, 0);
		}
		else if (mMoveSpeed >= _RunSpeed)
		{
			PlayAnim(_AnimNameRun, -1, 1f, 0);
		}
		else
		{
			PlayAnim(_AnimNameWalk, -1, 1f, 0);
		}
	}

	public override void PlayIdleAnimation()
	{
		mIdleAnimName = GetIdleAnimationName();
		_IdleAnimSpeed = Random.Range(0.8f, 1.2f);
		PlayAnim(mIdleAnimName, -1, _IdleAnimSpeed, 0);
	}

	public override void ChangeTexture(Texture2D t)
	{
		UtUtilities.SetObjectTexture(base.gameObject, 0, t);
	}

	public void DoAnimSeq(int nloop)
	{
		if (_AnimSeq.Length != 0)
		{
			mCurSeqIdx = 0;
			mNumAnimSeqLoop = nloop;
			DoAction(base.transform, Character_Action.userAction1);
			PlayAnim(_AnimSeq[0], 0, 1f, 1);
		}
	}

	public void StopAnimSeq()
	{
		mNumAnimSeqLoop = -1;
		mCurSeqIdx = -1;
		SetState(Character_State.idle);
	}

	public override void ActionDone(Character_Action actionid, bool ended, Transform actionObj)
	{
		if (actionid != Character_Action.userAction1 || mNumAnimSeqLoop == -1)
		{
			return;
		}
		mCurSeqIdx++;
		if (mCurSeqIdx < _AnimSeq.Length)
		{
			DoAction(base.transform, Character_Action.userAction1);
			PlayAnim(_AnimSeq[mCurSeqIdx], 0, 1f, 1);
			return;
		}
		mNumAnimSeqLoop--;
		if (mNumAnimSeqLoop > 0)
		{
			mCurSeqIdx = 0;
			DoAction(base.transform, Character_Action.userAction1);
			PlayAnim(_AnimSeq[mCurSeqIdx], 0, 1f, 1);
		}
		else
		{
			mCurSeqIdx = -1;
		}
	}

	public override void FallToGround()
	{
		Vector3 position = base.transform.position;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = 0.1f;
		Collider collider;
		float groundHeight;
		if (realtimeSinceStartup - mLastTimeFallToGroundTest < num)
		{
			collider = mLastCollider;
			groundHeight = mLastGroundHeight;
		}
		else
		{
			position.y += GroundCheckStartHeight;
			collider = (mLastCollider = UtUtilities.GetGroundHeight(position, GroundCheckDist, out groundHeight));
			mLastGroundHeight = groundHeight;
			mLastTimeFallToGroundTest = realtimeSinceStartup;
		}
		if (collider != null)
		{
			position.y = groundHeight;
			CheckSwim(collider);
		}
		else
		{
			CoCommonLevel component = GameObject.Find("PfCommonLevel").GetComponent<CoCommonLevel>();
			if (component != null)
			{
				position = component._AvatarStartMarker[0].position;
			}
			else
			{
				position.x = 0f;
				position.y = 0f;
				position.z = 0f;
			}
		}
		base.transform.position = position;
	}

	public override void SetPosOnSpline(float p)
	{
		Vector3 position = base.transform.position;
		base.SetPosOnSpline(p);
		if (!mOnSpringBoardSpline && !mOnSlide)
		{
			position.x = base.transform.position.x;
			position.z = base.transform.position.z;
			base.transform.position = position;
			FallToGround();
		}
	}

	public override void GenerateMoveToPath()
	{
		mAvatarOffsetSign *= -1f;
	}

	public override void MoveToDone(Transform target)
	{
		base.MoveToDone(target);
		FallToGround();
		if (target == mAvatarTarget)
		{
			SetState(Character_State.idle);
		}
	}

	public void LoadData()
	{
		if (_SkinTexture == null)
		{
			_SkinTexture = (Texture2D)UtUtilities.GetObjectTexture(base.gameObject, 0);
		}
	}

	private void OnProximity()
	{
		SetAvatar(AvAvatar.mTransform);
		_FollowAvatar = true;
		((ObProximity)base.gameObject.GetComponent(typeof(ObProximity))).enabled = false;
	}

	public void EnableProximity()
	{
		SetProximityEnabled(t: true);
	}

	public void DisableProximity()
	{
		SetProximityEnabled(t: false);
	}

	private void SetProximityEnabled(bool t)
	{
		ObProximity obProximity = (ObProximity)base.gameObject.GetComponent(typeof(ObProximity));
		if (obProximity != null)
		{
			obProximity.enabled = t;
		}
	}

	public void OnAvatarSpringBoardUse(ObSpringBoard inSB)
	{
		if (FollowingAvatar())
		{
			SetFollowAvatar(t: false);
			if (inSB._PetAllowed)
			{
				AvAvatarController component = mAvatar.gameObject.GetComponent<AvAvatarController>();
				Spline newSpline = inSB.GetNewSpline(base.transform.position, 0f - component.pGravity);
				SetSpline(newSpline);
				Speed = inSB._Speed;
				base.transform.forward = mAvatar.forward;
				mOnSpringBoardSpline = true;
			}
			else
			{
				mWaitingAtSpringBoard = true;
			}
			SetState(Character_State.idle);
		}
	}

	public void OnAvatarSpringBoardStateEnded()
	{
		if (mOnSpringBoardSpline || mWaitingAtSpringBoard)
		{
			mWaitingAtSpringBoard = false;
			mOnSpringBoardSpline = false;
			SetState(Character_State.idle);
			if (mAvatar != null)
			{
				SetFollowAvatar(t: true);
			}
		}
	}

	public void OnAvatarSetPositionDone(Vector3 targetPos)
	{
		if (FollowingAvatar())
		{
			SetState(Character_State.idle);
			MoveToAvatar();
		}
	}

	public void OnAvatarLaunchModeStarted()
	{
		mAvatarAirborne = true;
	}

	public void OnAvatarGlideModeEnded()
	{
		mAvatarAirborne = false;
	}

	public void OnAvatarDetachPets()
	{
		TeleportFx.PlayAt(base.transform.position);
		SetAvatar(null);
		Object.Destroy(base.gameObject);
	}

	public void OnAttachToAvatar(Transform playerTransform)
	{
		SetAvatar(playerTransform);
	}

	public void OnAvatarSlidingStateEnded(Vector3 avatarPos)
	{
		Invoke("EnableAvatarFollowingAfterSlide", mSlideEndFallDuration);
	}

	public void EnableAvatarFollowingAfterSlide()
	{
		if (mbFollowingAvatarBeforeSlide)
		{
			if (mSpline != null)
			{
				SetSpline(null);
			}
			SetState(Character_State.idle);
			SetFollowAvatar(t: true);
			mOnSlide = false;
		}
	}

	public void OnAvatarZiplineStateStarted()
	{
		SetState(Character_State.idle);
		SetFollowAvatar(t: false);
	}

	public void OnAvatarZiplineStateEnded()
	{
		SetState(Character_State.idle);
		SetFollowAvatar(t: true);
	}

	public void StartSliding()
	{
		mOnSlide = true;
		Speed = mSlideSpeed;
	}

	public void OnMouseUp()
	{
		if (!(mAvatar == null) && !(mAvatar.gameObject != AvAvatar.pObject) && !mTrickAnimPlaying)
		{
			string[] array = new string[5] { _AnimNameTrick360, _AnimNameTrickDance, _AnimNameTrickDoubleJump, _AnimNameTrickForwardFlip, _AnimNameTrickTurnStyle };
			int num = Random.Range(0, array.Length);
			string aname = array[num];
			PlayAnim(aname, 0, 1f, 1);
			mTrickAnimPlaying = true;
			float length = base.animation.GetClip(aname).length;
			Invoke("EnableTrickAnims", length);
		}
	}

	private void EnableTrickAnims()
	{
		mTrickAnimPlaying = false;
	}
}
