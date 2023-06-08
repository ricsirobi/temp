using UnityEngine;

public class CaAvatarCam : KAMonoBase
{
	public enum CameraLayer
	{
		LAYER_AVATAR,
		LAYER_FLYING,
		LAYER_ENGAGEMENT,
		LAYER_FISHING,
		LAYER_COUNT
	}

	public enum CameraMode
	{
		MODE_RELATIVE,
		MODE_ABSOLUTE,
		MODE_SHAKE
	}

	public struct CamInterpData
	{
		public Vector3 offset;

		public float focusHeight;

		public float lookAtHeight;

		public float speed;

		public CameraMode mode;

		public Transform lookAt;

		public Quaternion lookQuat;

		public Transform pitchBone;

		public float relativePitch;

		public AvAvatarController lookAtController;

		public float curAccelRadius;

		public bool postLerp;

		public Vector3 mDesWorldPos;

		public Vector3 mCurWorldPos;

		public void Set(Vector3 aPosTo, float aFocusTo, float newT, float newLookAtHeight = 0f)
		{
			offset = aPosTo;
			focusHeight = aFocusTo;
			lookAtHeight = newLookAtHeight;
			Reset();
		}

		public void Reset()
		{
			mDesWorldPos = (mCurWorldPos = CalcWorldPos());
		}

		public void SetYaw(float yaw)
		{
			if (mode == CameraMode.MODE_RELATIVE)
			{
				offset.x = yaw;
			}
		}

		public void SetPitch(float pitch)
		{
			if (mode == CameraMode.MODE_RELATIVE)
			{
				offset.y = pitch;
			}
		}

		public void SetDistance(float dist)
		{
			if (mode == CameraMode.MODE_RELATIVE)
			{
				offset.z = dist;
			}
		}

		public void Update()
		{
			mDesWorldPos = CalcWorldPos();
			if (mode == CameraMode.MODE_RELATIVE && postLerp)
			{
				Vector3 vector = GetLookAt();
				Vector3 vector2 = mCurWorldPos;
				vector2 = Vector3.Slerp(vector2 - vector, mDesWorldPos - vector, 1f / 30f * speed * 6f);
				mCurWorldPos = vector2 + vector;
			}
			else
			{
				mCurWorldPos = mDesWorldPos;
			}
		}

		public Vector3 CalcWorldPos()
		{
			Vector3 vector;
			if (mode == CameraMode.MODE_RELATIVE && lookAt != null)
			{
				float b = ((lookAtController != null && lookAtController.pFlyingFlapCooldownTimer > 0f) ? _AccelRadius : 0f);
				curAccelRadius = Mathf.Lerp(curAccelRadius, b, 1f / 30f * speed / 2f);
				if ((bool)pitchBone)
				{
					float num = pitchBone.eulerAngles.x + relativePitch;
					if (num > 360f)
					{
						num -= 360f;
					}
					if (num < 0f)
					{
						num += 360f;
					}
					offset.y = Mathf.LerpAngle(offset.y, num, 1f / 30f);
				}
				vector = Quaternion.Euler(offset.y, offset.x, 0f) * new Vector3(0f, 0f, offset.z - curAccelRadius);
				vector = lookQuat * vector;
				vector += lookAt.transform.position;
				vector.y += GetLookAtHeight();
				lookQuat = Quaternion.Slerp(lookQuat, lookAt.rotation, 1f / 30f * speed * 2f);
			}
			else
			{
				vector = offset;
			}
			return vector;
		}

		public Vector3 GetWorldPos()
		{
			return mCurWorldPos;
		}

		public Vector3 GetLookAt()
		{
			if (lookAt != null)
			{
				return lookAt.position + new Vector3(0f, GetLookAtHeight(), 0f);
			}
			return Vector3.zero;
		}

		public void ResetLookAt()
		{
			if (lookAt != null)
			{
				lookQuat = lookAt.rotation;
			}
		}

		public float GetLookAtHeight()
		{
			if (lookAtHeight != 0f)
			{
				return lookAtHeight;
			}
			return focusHeight;
		}
	}

	public const float mMinCameraDistance = 2f;

	public static float _AccelRadius = 3f;

	public float _Speed;

	public bool _IgnoreCollision;

	public bool _FreeRotate = true;

	public float _CollisionRadius = 0.25f;

	public float _DefaultPitch = 25f;

	public float _DefaultDistance = 4f;

	public float mMaxCameraDistance = 12f;

	public bool mUpdateAvatarCamParam = true;

	private bool mForceFreeRotate;

	private Vector3 mOffset;

	private Transform mTarget;

	private float mSpeed;

	private bool mNoLerp;

	private float mCollideTween;

	private Vector3 mCollisionOffset = Vector3.zero;

	private Vector3 mDampLookAt = Vector3.zero;

	private float mLastTimeCameraWasRotatedByPlayer;

	private float mLastCollisionCheckDist;

	private bool mLastCollisionHitSomething;

	private float mLastCollisionCheckTime;

	private float mShakeAmount;

	private float mShakeDuration;

	private float mShakeDecay;

	private CameraMode mModeBeforeShake;

	private int mWhichLayer;

	private const float cFixedRate = 1f / 30f;

	[Range(0f, 1f)]
	public float _OptimizationFarClipScaleFactor = 1f;

	private const float mLerpMod = 6f;

	private Vector3 mMousePrev;

	private bool mFreeRotate;

	private Transform mEmptyObj;

	private CamInterpData[] mCamData;

	private CameraLayer mCurLayer;

	private void Start()
	{
		mCamData = new CamInterpData[4];
		mSpeed = _Speed;
		for (int i = 0; i < mCamData.Length; i++)
		{
			mCamData[i].speed = mSpeed;
			mCamData[i].Set(new Vector3(0f, _DefaultPitch, 0f - _DefaultDistance), 0f, 1f);
			mCamData[i].lookAtHeight = 0f;
			mCamData[i].mode = CameraMode.MODE_RELATIVE;
			mCamData[i].postLerp = true;
		}
		if (AvAvatar.pObject != null)
		{
			SetLookAt(AvAvatar.mTransform, null, 0f);
			AvAvatar.pAvatarCam = base.gameObject;
		}
		else
		{
			GameObject gameObject = GameObject.Find(AvAvatar.GetAvatarPrefabName());
			if (gameObject != null)
			{
				SetLookAt(gameObject.transform, null, 0f);
				AvAvatar.pAvatarCam = base.gameObject;
			}
		}
		ResetCamera();
	}

	private void LateUpdate()
	{
		if (mCamData == null)
		{
			return;
		}
		int num = (int)mCurLayer;
		mTarget = mCamData[num].lookAt;
		if (mCamData[num].mode == CameraMode.MODE_SHAKE)
		{
			if (mShakeDuration > 0f)
			{
				base.transform.position = mCamData[num].mCurWorldPos + Random.insideUnitSphere * mShakeAmount;
				mShakeDuration -= Time.deltaTime * mShakeDecay;
			}
			else
			{
				mCamData[mWhichLayer].mode = mModeBeforeShake;
				mShakeDuration = 0f;
				mUpdateAvatarCamParam = true;
			}
		}
		else
		{
			if (!(mTarget != null))
			{
				return;
			}
			for (int i = 0; i < mCamData.Length; i++)
			{
				mCamData[i].Update();
			}
			mOffset = mCamData[num].GetWorldPos();
			bool flag = true;
			float axis = KAInput.GetAxis("CameraRotationX");
			float axis2 = KAInput.GetAxis("CameraRotationY");
			if (AvAvatar.pState != AvAvatarState.PAUSED && (mForceFreeRotate || AvAvatar.pState != 0) && (KAInput.GetMouseButton(1) || axis != 0f || axis2 != 0f) && mCamData[num].mode != CameraMode.MODE_ABSOLUTE && flag)
			{
				if (!mFreeRotate)
				{
					mFreeRotate = true;
					mMousePrev = Input.mousePosition;
				}
				float num2 = 0f;
				float num3 = 0f;
				if (axis != 0f || axis2 != 0f)
				{
					num3 = axis;
					num2 = axis2;
				}
				else
				{
					num2 = Input.mousePosition.y - mMousePrev.y;
					num3 = Input.mousePosition.x - mMousePrev.x;
				}
				mCamData[num].offset.x += num3 * 0.2f;
				if (mCamData[num].offset.x > 180f)
				{
					mCamData[num].offset.x -= 360f;
				}
				else if (mCamData[num].offset.x < -180f)
				{
					mCamData[num].offset.x += 360f;
				}
				AvAvatarController component = mTarget.GetComponent<AvAvatarController>();
				if (AvAvatar.pLevelState == AvAvatarLevelState.NORMAL || AvAvatar.pLevelState == AvAvatarLevelState.WORLDEVENT || (AvAvatar.pLevelState == AvAvatarLevelState.FLIGHTSCHOOL && component != null && component.PlayingFlightSchoolFlightSuit))
				{
					mCamData[num].offset.y -= num2 * 0.2f;
					mCamData[num].offset.y = Mathf.Clamp(mCamData[num].offset.y, -45f, 85f);
				}
				mMousePrev = Input.mousePosition;
				mLastTimeCameraWasRotatedByPlayer = Time.realtimeSinceStartup;
			}
			else
			{
				mFreeRotate = false;
			}
			if (GameDataConfig.pIsReady && !IsFPSGood() && CameraLayer.LAYER_FISHING != mCurLayer)
			{
				float num4 = Time.realtimeSinceStartup - mLastTimeCameraWasRotatedByPlayer;
				num4 = Mathf.Clamp01((num4 - 5f) / 5f);
				mCamData[num].offset.y = Mathf.MoveTowards(mCamData[num].offset.y, 30f, num4 * 5f);
			}
			else
			{
				mLastTimeCameraWasRotatedByPlayer = Time.realtimeSinceStartup;
			}
			if (mCamData[num].mode != CameraMode.MODE_ABSOLUTE && mCamData[num].offset.z < 0f)
			{
				if (KAInput.GetAxis("CameraZoom") > 0f)
				{
					mCamData[num].offset.z += 0.25f;
					if (mCamData[num].offset.z > -2f)
					{
						mCamData[num].offset.z = -2f;
					}
				}
				else if (KAInput.GetAxis("CameraZoom") < 0f)
				{
					mCamData[num].offset.z -= 0.25f;
					if (mCamData[num].offset.z < 0f - mMaxCameraDistance)
					{
						mCamData[num].offset.z = 0f - mMaxCameraDistance;
					}
				}
				float b = Mathf.Clamp(mCamData[num].offset.z, 0f - mMaxCameraDistance, -2f);
				mCamData[num].offset.z = Mathf.Lerp(mCamData[num].offset.z, b, 1f / 30f * mSpeed);
			}
			Vector3 lookAt = mCamData[num].GetLookAt();
			Vector3 end = mOffset;
			if (CollisionCheck(lookAt, ref end))
			{
				mCollideTween = Mathf.Min(1f, mCollideTween + 2f / 15f);
				float num5 = mCollideTween;
				num5 = num5 * num5 * (3f - 2f * num5);
				if (num5 > 0f)
				{
					end = mOffset + (end - mOffset) * num5;
				}
				mCollisionOffset = end - mOffset;
			}
			else
			{
				mCollisionOffset = Vector3.Lerp(mCollisionOffset, Vector3.zero, 1f / 15f);
				mCollideTween = 0f;
			}
			bool flag2 = !mNoLerp && mCamData[(int)mCurLayer].postLerp;
			mDampLookAt = Vector3.Lerp(mDampLookAt, lookAt, flag2 ? (1f / 30f * mSpeed * 6f) : 1f);
			Vector3 b2 = mOffset + mCollisionOffset;
			if (AvAvatar.pSubState == AvAvatarSubState.UWSWIMMING)
			{
				AvAvatarController component2 = mTarget.GetComponent<AvAvatarController>();
				if (component2 != null && component2.pUWSwimZone != null && !component2.pIsTransiting)
				{
					float allowedYPosForCamera = component2.GetAllowedYPosForCamera();
					if (b2.y > allowedYPosForCamera)
					{
						b2.y = allowedYPosForCamera;
					}
				}
			}
			Vector3 position = Vector3.Lerp(base.transform.position, b2, flag2 ? (1f / 30f * mSpeed * 6f) : 1f);
			base.transform.position = position;
			base.transform.LookAt(mDampLookAt, Vector3.up);
			mNoLerp = false;
		}
	}

	private bool IsFPSGood()
	{
		return GrFPS.pFrameRate > (float)GameDataConfig.pInstance.OptimizationData.GoodFPS;
	}

	public CamInterpData GetCurrentLayer()
	{
		return mCamData[(int)mCurLayer];
	}

	public Vector3 GetWorldPosition()
	{
		return mCamData[(int)mCurLayer].GetWorldPos();
	}

	public Vector3 GetOffset()
	{
		return mCamData[(int)mCurLayer].offset;
	}

	public void SetPosition(Vector3 offset, float focusHeight, float newT = 0f, float lookAtHeight = 0f)
	{
		mCamData[(int)mCurLayer].Set(offset, focusHeight, newT, lookAtHeight);
	}

	public void SetPosition(CameraLayer layer, Vector3 offset, float focusHeight, float newT = 0f)
	{
		mCamData[(int)layer].Set(offset, focusHeight, newT);
	}

	public void SetYaw(float yaw)
	{
		mCamData[(int)mCurLayer].SetYaw(yaw);
	}

	public void SetPitch(float pitch)
	{
		mCamData[(int)mCurLayer].SetPitch(pitch);
	}

	public void SetDistance(float dist)
	{
		mCamData[(int)mCurLayer].SetDistance(dist);
	}

	public void SetLayer(CameraLayer layer, float newT = 0f)
	{
		mCurLayer = layer;
	}

	public void CopyLayer(CameraLayer layer)
	{
		mCamData[(int)mCurLayer] = mCamData[(int)layer];
	}

	public void SetLookAt(Transform lookAt, Transform pitchBone, float relativePitch)
	{
		if (lookAt != null)
		{
			CharacterController characterController = (CharacterController)lookAt.GetComponent(typeof(CharacterController));
			float lookAtHeight = ((characterController != null) ? (characterController.height * 1.4f) : 0f);
			mCamData[(int)mCurLayer].lookAt = lookAt;
			mCamData[(int)mCurLayer].lookQuat = lookAt.rotation;
			mCamData[(int)mCurLayer].pitchBone = pitchBone;
			mCamData[(int)mCurLayer].relativePitch = relativePitch;
			mCamData[(int)mCurLayer].lookAtController = lookAt.GetComponent<AvAvatarController>();
			mCamData[(int)mCurLayer].lookAtHeight = lookAtHeight;
			mCamData[(int)mCurLayer].Reset();
			for (int i = 0; i < mCamData.Length; i++)
			{
				if (mCamData[i].lookAt == null)
				{
					mCamData[i].lookAt = lookAt;
					mCamData[i].lookQuat = lookAt.rotation;
					mCamData[i].lookAtHeight = lookAtHeight;
					mCamData[i].Reset();
				}
			}
		}
		else if (mCamData != null)
		{
			for (int j = 0; j < mCamData.Length; j++)
			{
				mCamData[j].lookAt = lookAt;
			}
		}
	}

	public void SetSpeed(float aSpeed)
	{
		mCamData[(int)mCurLayer].speed = aSpeed;
	}

	public void SetMode(CameraMode aMode)
	{
		mCamData[(int)mCurLayer].mode = aMode;
	}

	public void ResetSpeed()
	{
		mCamData[(int)mCurLayer].speed = _Speed;
	}

	public void ResetCamera()
	{
		mCamData[(int)mCurLayer].Reset();
		mNoLerp = true;
	}

	public void ResetLookAt()
	{
		mCamData[(int)mCurLayer].ResetLookAt();
	}

	private bool CollisionCheck(Vector3 start, ref Vector3 end)
	{
		Vector3 normalized = (end - start).normalized;
		if (UtPlatform.IsMobile())
		{
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup - mLastCollisionCheckTime < 0.25f)
			{
				if (mLastCollisionHitSomething)
				{
					end = start + normalized * mLastCollisionCheckDist;
				}
				return mLastCollisionHitSomething;
			}
			mLastCollisionCheckTime = realtimeSinceStartup;
		}
		bool result = false;
		int layerMask = ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("MMOAvatar")) | (1 << LayerMask.NameToLayer("Collectibles")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("IgnoreGroundRay")) | (1 << LayerMask.NameToLayer("DraggedObject")) | (1 << LayerMask.NameToLayer("Marker")) | (1 << LayerMask.NameToLayer("Furniture")) | (1 << LayerMask.NameToLayer("2DNGUI")) | (1 << LayerMask.NameToLayer("3DNGUI")));
		mLastCollisionHitSomething = false;
		RaycastHit hitInfo = default(RaycastHit);
		if (!_IgnoreCollision && Physics.SphereCast(start, _CollisionRadius, normalized, out hitInfo, (end - start).magnitude, layerMask))
		{
			int num = LayerMask.NameToLayer("Water");
			Collider collider = hitInfo.collider;
			if (!collider.isTrigger || (collider.gameObject.layer == num && AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING))
			{
				end = start + normalized * hitInfo.distance;
				result = true;
				mLastCollisionHitSomething = true;
				mLastCollisionCheckDist = hitInfo.distance;
			}
		}
		return result;
	}

	public void SetAvatarCamParams(AvAvatarCamParams newParams, AvAvatarSubState subState = AvAvatarSubState.NORMAL)
	{
		if (mUpdateAvatarCamParam)
		{
			if (subState == AvAvatarSubState.FLYING)
			{
				SetLayer(CameraLayer.LAYER_FLYING);
				CopyLayer(CameraLayer.LAYER_AVATAR);
				mCamData[1].lookAtHeight += 1f;
				mCamData[1].postLerp = false;
			}
			else
			{
				SetLayer(CameraLayer.LAYER_AVATAR);
			}
		}
	}

	public void ForceFreeRotate(bool isForceRotation)
	{
		mForceFreeRotate = isForceRotation;
	}

	private bool AllowFreeRotate()
	{
		if (mForceFreeRotate)
		{
			return true;
		}
		if (AvAvatar.pSubState == AvAvatarSubState.WALLCLIMB)
		{
			return false;
		}
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA()) && UiJoystick.pInstance != null && UiJoystick.pInstance.pIsPressed)
		{
			return false;
		}
		if (_FreeRotate && (AvAvatar.pState == AvAvatarState.IDLE || AvAvatar.pState == AvAvatarState.MOVING || mFreeRotate))
		{
			return true;
		}
		return false;
	}

	public void Shake(float inShakeAmount = 0.17f, float inShakeDuration = 0.2f, float inShakeDecay = 1f)
	{
		int num = (int)mCurLayer;
		if (CameraMode.MODE_SHAKE != mCamData[num].mode)
		{
			mWhichLayer = num;
			mModeBeforeShake = mCamData[num].mode;
			mCamData[num].mode = CameraMode.MODE_SHAKE;
			mShakeAmount = inShakeAmount;
			mShakeDuration = inShakeDuration;
			mShakeDecay = inShakeDecay;
			mUpdateAvatarCamParam = false;
		}
	}
}
