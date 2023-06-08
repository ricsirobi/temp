using System;
using System.Collections.Generic;
using UnityEngine;

public class GauntletController : SplineControl
{
	[Serializable]
	public class DragonTiltData
	{
		public int _PetTypeID;

		public float _TiltRatio = 1f;
	}

	[Serializable]
	public class RailGunData
	{
		public GameObject _Gun;

		public Vector3 _GunOffset;

		public Vector3 _Scale;

		public float _AmmoSpeed;

		public bool _IsDefaut;

		public float _ShootInterval = 1f;

		public float _ShootIntervalMobile = 1f;

		public GameObject _Light;

		public GSGameType _GameType;
	}

	public delegate void PetReadyEvent();

	public Transform _RailGunMarker;

	public Vector3 _RailGunOffset = new Vector3(0.03f, 0.3f, 0.4f);

	public Vector3 _LightOffset = Vector3.zero;

	public Camera _AvatarCamera;

	public float _GunFocusDepth = 3f;

	public float _CrossHairDepth = 2f;

	public float _CrossHairRotSpeed = 15f;

	public bool _FollowOriginalSetup;

	public GameObject _MessageObject;

	public bool _Pause;

	public DragonTiltData[] _TiltData;

	public float _PetLoadMaxWaitTime = 5f;

	public List<RailGunData> _RailGunList;

	public bool _ExtraTime;

	[NonSerialized]
	public bool _IsToothless;

	public int _ToothlessID = 17;

	public string _ToothlessDataPath = "RS_SHARED/DWNightFuryDO.unity3d/PfDWNightFury";

	private Vector2 mScreenPoint = Vector2.zero;

	private Vector2 mDragDir = Vector2.zero;

	private float mDragMag = 1f;

	private bool mShoot;

	private int mDragFingerId = -1;

	[NonSerialized]
	public bool _CanShoot;

	private float mPreviousHeadParticleSize;

	private ParticleSystem mHeadParticle;

	private GameObject mGameLight;

	private GameObject mGauntletRailGun;

	private float mShootTimer;

	private float mPathDiversionDistance;

	private Transform mNextSplineObject;

	private int mNextSplineNodeIndex;

	private GameObject mCrossHair;

	private bool mIsEnabled = true;

	private DragonTiltData mTiltData;

	private float mCrossHairRotSpeed;

	private float mCrossHairRotValue;

	private bool mPetLoaded;

	private float mTouchManagerSensitivityOnStart;

	private bool mPetMounted;

	private float mPetLoadTimer;

	private GauntletControlModifier mCurrentControlModifier;

	private RailGunData mRailGunData;

	private WeaponManager mWeaponsManager;

	protected RaisedPetData mPlayerPetData;

	protected PetReadyEvent mOnPetReady;

	public bool pPetLoaded => mPetLoaded;

	public bool pPetMounted => mPetMounted;

	public GauntletControlModifier pCurrentControlModifier => mCurrentControlModifier;

	public float pShootInterval
	{
		get
		{
			if (mRailGunData != null)
			{
				if (UtPlatform.IsMobile())
				{
					return mRailGunData._ShootIntervalMobile;
				}
				return mRailGunData._ShootInterval;
			}
			return 1f;
		}
	}

	public void Awake()
	{
		mPlayerPetData = SanctuaryManager.pCurPetData;
	}

	public override void Start()
	{
		base.Start();
		mScreenPoint = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
		TouchManager.OnDragStartEvent = (OnDragStart)Delegate.Combine(TouchManager.OnDragStartEvent, new OnDragStart(OnDragStart));
		TouchManager.OnDragEndEvent = (OnDragEnd)Delegate.Combine(TouchManager.OnDragEndEvent, new OnDragEnd(OnDragEnd));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		mTouchManagerSensitivityOnStart = TouchManager.pInstance._TouchSensitivity;
		TouchManager.pInstance._TouchSensitivity = 0f;
	}

	public void InstantiateRailGun(GameObject inGunObject, GameObject inCrossHair)
	{
		mRailGunData = GetRailGunData(GauntletRailShootManager.pInstance.pGameType);
		if (_RailGunMarker != null)
		{
			if (GauntletRailShootManager.pIsCrossbowLevel)
			{
				GameObject obj = UnityEngine.Object.Instantiate(mRailGunData._Gun);
				InitGauntletGun(obj, mRailGunData._GunOffset, mRailGunData._Scale);
				mWeaponsManager = mGauntletRailGun.GetComponent<WeaponManager>();
			}
			if (mRailGunData._Light != null)
			{
				if (mGameLight != null)
				{
					UnityEngine.Object.Destroy(mGameLight);
				}
				mGameLight = UnityEngine.Object.Instantiate(mRailGunData._Light);
				mGameLight.transform.parent = _RailGunMarker.parent;
				mGameLight.transform.position = _RailGunMarker.position + _LightOffset;
			}
		}
		if (inCrossHair != null)
		{
			mCrossHair = UnityEngine.Object.Instantiate(inCrossHair);
		}
	}

	public void SetDisabled(bool isDisabled)
	{
		if (mCrossHair != null)
		{
			mCrossHair.SetActive(!isDisabled);
		}
		mIsEnabled = !isDisabled;
		if (isDisabled)
		{
			mShoot = false;
		}
	}

	public void InitGauntletGun(GameObject obj, Vector3 offset, Vector3 scale)
	{
		if (mGauntletRailGun != null && mGauntletRailGun != AvAvatar.pObject)
		{
			UnityEngine.Object.Destroy(mGauntletRailGun);
		}
		mGauntletRailGun = obj;
		mGauntletRailGun.transform.localScale = scale;
		mGauntletRailGun.transform.parent = _RailGunMarker.parent;
		mGauntletRailGun.transform.localEulerAngles = Vector3.zero;
		mGauntletRailGun.transform.localPosition = _RailGunMarker.localPosition + offset;
	}

	public void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mount)
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.pObject.GetComponent<AvAvatarController>().enabled = false;
			InitGauntletGun(AvAvatar.pObject, Vector3.zero, Vector3.one * 0.2f);
			GameObject partObject = AvatarData.GetPartObject(AvAvatar.pObject.transform, AvatarData.pBoneSettings.HEAD_PARENT_BONE, 0);
			if (partObject != null)
			{
				ParticleSystem componentInChildren = partObject.GetComponentInChildren<ParticleSystem>();
				if (componentInChildren != null)
				{
					mHeadParticle = componentInChildren;
					mPreviousHeadParticleSize = mHeadParticle.main.startSize.constant;
					ParticleSystem.MainModule main = mHeadParticle.main;
					main.startSize = 0.02f;
				}
			}
			GetComponent<ObStatus>().pIsReady = true;
			DragonTiltData[] tiltData = _TiltData;
			foreach (DragonTiltData dragonTiltData in tiltData)
			{
				if (dragonTiltData._PetTypeID == SanctuaryManager.pCurrentPetType)
				{
					mTiltData = dragonTiltData;
					break;
				}
			}
			Transform transform = SanctuaryManager.pCurPetInstance.transform.Find(SanctuaryManager.pCurPetInstance._RootBone);
			if (transform != null)
			{
				transform.localScale *= SanctuaryManager.pCurPetInstance.pCurAgeData._TPScale;
			}
		}
		mPetMounted = mount;
	}

	private void OnDestroy()
	{
		TouchManager.OnDragStartEvent = (OnDragStart)Delegate.Remove(TouchManager.OnDragStartEvent, new OnDragStart(OnDragStart));
		TouchManager.OnDragEndEvent = (OnDragEnd)Delegate.Remove(TouchManager.OnDragEndEvent, new OnDragEnd(OnDragEnd));
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		TouchManager.pInstance._TouchSensitivity = mTouchManagerSensitivityOnStart;
		if (mHeadParticle != null)
		{
			ParticleSystem.MainModule main = mHeadParticle.main;
			main.startSize = mPreviousHeadParticleSize;
		}
	}

	private void OnDisable()
	{
		SanctuaryPet.RemoveMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
	}

	public void Init()
	{
		int petTypeID = -1;
		if (!GauntletRailShootManager.pIsCrossbowLevel)
		{
			SanctuaryManager.pMountedState = true;
			AvAvatar.SetDisplayNameVisible(inVisible: false);
			AvAvatar.pObject.GetComponent<AvAvatarController>().enabled = false;
			AvAvatar.mTransform.localPosition = _RailGunMarker.localPosition + _RailGunOffset;
			AvAvatar.SetUIActive(inActive: true);
			if (SanctuaryManager.pCurPetData != null)
			{
				petTypeID = SanctuaryManager.pCurPetData.PetTypeID;
			}
		}
		else
		{
			AvAvatar.SetActive(inActive: false);
			SanctuaryManager.pMountedState = false;
		}
		Vector3 cameraOffsetForPet = GauntletRailShootManager.pInstance.GetCameraOffsetForPet(petTypeID);
		if (cameraOffsetForPet != Vector3.zero)
		{
			_AvatarCamera.transform.localPosition = cameraOffsetForPet;
		}
		InstantiateRailGun(GauntletRailShootManager.pInstance._GauntletArm, GauntletRailShootManager.pInstance._CrossHairPrefab);
	}

	public override void Update()
	{
		if (_Pause)
		{
			if (mOnPetReady != null && mPetMounted)
			{
				mOnPetReady();
				mOnPetReady = null;
			}
			return;
		}
		if (!mPetLoaded)
		{
			mPetLoadTimer += Time.deltaTime;
			if (mPetLoadTimer > _PetLoadMaxWaitTime)
			{
				GetComponent<ObStatus>().pIsReady = true;
				return;
			}
		}
		if (!_IsToothless && ((SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge < SanctuaryManager.pCurPetInstance.pTypeInfo._MinAgeToMount) || SanctuaryManager.pCurPetData == null))
		{
			_IsToothless = true;
			LoadToothless(null);
		}
		else if (!mPetLoaded && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetData != null && SanctuaryManager.pCurPetData.pStage > RaisedPetStage.CHILD)
		{
			mPetLoaded = true;
			SanctuaryManager.pCurPetInstance.gameObject.GetComponent<ObClickable>().enabled = false;
			AvAvatar.SetActive(inActive: true);
			SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
			SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
		}
		if (!_ExtraTime)
		{
			base.Update();
		}
		if (_AvatarCamera != null && mGauntletRailGun != null && mIsEnabled)
		{
			Vector3 mousePosition = Input.mousePosition;
			if (KAInput.pInstance.pInputMode == KAInputMode.TOUCH)
			{
				float num = 1.5f;
				float num2 = mScreenPoint.x + mDragDir.x * num * mDragMag;
				float num3 = mScreenPoint.y + mDragDir.y * num * (0f - mDragMag);
				if (num2 < (float)Screen.width && num2 > 0f)
				{
					mScreenPoint.x = num2;
				}
				if (num3 < (float)Screen.height && num3 > 0f)
				{
					mScreenPoint.y = num3;
				}
				mousePosition.x = mScreenPoint.x;
				mousePosition.y = mScreenPoint.y;
				mDragDir = Vector2.zero;
			}
			if (new Rect(0f, 0f, Screen.width, Screen.height).Contains(new Vector2(mousePosition.x, (float)Screen.height - mousePosition.y)))
			{
				Vector3 normalized = (_AvatarCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, _GunFocusDepth)) - _AvatarCamera.transform.position).normalized;
				Vector3 vector = _AvatarCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _GunFocusDepth));
				Vector3 normalized2 = (vector - _AvatarCamera.transform.position).normalized;
				normalized.x = normalized2.x;
				normalized.z = normalized2.z;
				float t = ((mTiltData != null) ? mTiltData._TiltRatio : 1f);
				mGauntletRailGun.transform.forward = Vector3.Lerp(normalized, normalized2, t);
				if (mCrossHair != null)
				{
					vector = _AvatarCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, _CrossHairDepth));
					mCrossHair.transform.position = vector;
					mCrossHair.transform.forward = -_AvatarCamera.transform.forward;
					if (mCrossHairRotSpeed > 0f && pShootInterval > 0f)
					{
						mCrossHairRotValue += mCrossHairRotSpeed;
						mCrossHairRotSpeed -= _CrossHairRotSpeed * Time.deltaTime / pShootInterval;
					}
					mCrossHair.transform.Rotate(0f, 0f, mCrossHairRotValue);
					mCrossHair.SetActive(_CanShoot);
				}
			}
		}
		if (mNextSplineObject != null && ((Speed > 0f && CurrentPos >= mPathDiversionDistance) || (Speed < 0f && CurrentPos <= mPathDiversionDistance)))
		{
			SplineObject = mNextSplineObject;
			CurrentPos = 0f;
			ResetSpline();
			if (mNextSplineNodeIndex < mSpline.mNodes.Length)
			{
				SetPosOnSpline(mSpline.mNodes[mNextSplineNodeIndex].mDistance);
			}
			mNextSplineObject = null;
		}
		if (mShootTimer > 0f)
		{
			mShootTimer -= Time.deltaTime;
		}
		else if (_CanShoot && mGauntletRailGun != null && (mShoot || (KAUI.GetGlobalMouseOverItem() == null && (Input.GetKey(KeyCode.Space) || (KAInput.pInstance.pInputMode != 0 && Input.GetMouseButton(0))))))
		{
			mShoot = false;
			mShootTimer = pShootInterval;
			mCrossHairRotSpeed = _CrossHairRotSpeed;
			FireAProjectile();
		}
		if (mSpline != null && mEndReached)
		{
			SetSpline(null);
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnSplineEndReached", SplineObject.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void LoadToothless(PetReadyEvent onPetReady)
	{
		LoadPet(RaisedPetData.CreateCustomizedPetData(_ToothlessID, RaisedPetStage.ADULT, _ToothlessDataPath, Gender.Male, null, noColorMap: true), onPetReady);
	}

	public void LoadPet(RaisedPetData petData, PetReadyEvent onPetReady)
	{
		mPetLoaded = false;
		mPetMounted = false;
		mOnPetReady = onPetReady;
		_Pause = true;
		SanctuaryPet pCurPetInstance = SanctuaryManager.pCurPetInstance;
		if (pCurPetInstance != null)
		{
			GauntletRailShootManager.pInstance.ResetAvatarAndDragon();
			UnityEngine.Object.Destroy(pCurPetInstance.gameObject);
		}
		base.gameObject.SetActive(value: true);
		SanctuaryManager.CreatePet(petData, base.transform.position, Quaternion.identity, base.gameObject, "Player");
	}

	private void OnPetReady(SanctuaryPet pet)
	{
		mPetLoaded = true;
		pet.SetAvatar(AvAvatar.mTransform);
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(follow: false);
		if ((bool)pet.AIActor)
		{
			pet.AIActor.SetState(AISanctuaryPetFSM.MOUNTED);
		}
		SanctuaryManager.pCurPetInstance = pet;
		SanctuaryManager.pCurPetData = pet.pData;
		SanctuaryManager.pCurPetData.pNoSave = true;
		SanctuaryManager.pCurPetInstance.gameObject.GetComponent<ObClickable>().enabled = false;
		AvAvatar.SetActive(inActive: true);
		SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		SanctuaryManager.pCurPetInstance.Mount(AvAvatar.pObject, PetSpecialSkillType.RUN);
	}

	public void ChangeSplinePath(Vector3 inDestPosition, Transform inNextSplineObj, int inNodeIndex)
	{
		int num = 0;
		SplineNode[] mNodes = mSpline.mNodes;
		foreach (SplineNode splineNode in mNodes)
		{
			if ((inDestPosition - splineNode.mPoint).magnitude < 0.1f)
			{
				break;
			}
			num++;
		}
		if (num < mSpline.mNodes.Length)
		{
			mPathDiversionDistance = mSpline.mNodes[num].mDistance;
			mNextSplineObject = inNextSplineObj;
			mNextSplineNodeIndex = inNodeIndex;
		}
	}

	public void SetSplineObject(GameObject inSplineObject)
	{
		SplineObject = inSplineObject.transform;
		CurrentPos = 0f;
		ResetSpline();
	}

	private RailGunData GetRailGunData(GSGameType gameType)
	{
		return _RailGunList.Find((RailGunData data) => data._GameType == gameType) ?? _RailGunList.Find((RailGunData data) => data._IsDefaut);
	}

	public void FireAProjectile(bool isPowerUp = false)
	{
		Ray ray = default(Ray);
		ray = ((KAInput.pInstance.pInputMode != 0) ? Camera.main.ScreenPointToRay(KAInput.mousePosition) : Camera.main.ScreenPointToRay(new Vector3(mScreenPoint.x, mScreenPoint.y, 0f)));
		float maxDistance = 200f;
		if (isPowerUp)
		{
			Vector2 vector = new Vector2((float)Screen.width * 0.5f, (float)Screen.height * 0.5f);
			ray = Camera.main.ScreenPointToRay(new Vector3(vector.x, vector.y, 0f));
			maxDistance = 50f;
		}
		if (Physics.Raycast(ray, out var hitInfo, maxDistance, UtUtilities.GetGroundRayCheckLayers()))
		{
			if (GauntletRailShootManager.pInstance != null)
			{
				GauntletRailShootManager.pInstance.ProjectileShot();
			}
			if (!GauntletRailShootManager.pIsCrossbowLevel)
			{
				SanctuaryManager.pCurPetInstance.Fire(null, useDirection: true, hitInfo.point, ignoreCoolDown: true, SanctuaryManager.pCurPetInstance.pData.PetTypeID != 19);
			}
			else if (mWeaponsManager != null)
			{
				mWeaponsManager.FireWeapon("Crossbow", null, useDirection: true, hitInfo.point, mRailGunData._AmmoSpeed);
			}
		}
	}

	public override void SetPosOnSpline(float p)
	{
		if (_FollowOriginalSetup)
		{
			base.SetPosOnSpline(p);
			return;
		}
		CurrentPos = p;
		if (mSpline != null)
		{
			CurrentPos = mSpline.GetPosQuatByDist(CurrentPos, out var pos, out var quat);
			if (Speed < 0f && FlipDirOnBackward && (mSpline.mHasQ || mSpline.mAlignTangent))
			{
				quat = Quaternion.Euler(quat.eulerAngles.x, quat.eulerAngles.y + 180f, quat.eulerAngles.z);
			}
			if (IsLocal)
			{
				pos = base.transform.parent.localToWorldMatrix.MultiplyPoint3x4(pos);
			}
			if (GroundCheck)
			{
				pos.y += GroundCheckStartHeight;
				UtUtilities.GetGroundHeight(pos, GroundCheckDist, out var groundHeight);
				pos.y = groundHeight;
			}
			ValidateVector(ref pos);
			base.transform.position = pos;
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		GauntletControlModifier component = collider.GetComponent<GauntletControlModifier>();
		GauntletRailShootManager pInstance = GauntletRailShootManager.pInstance;
		if (component != null && pInstance != null)
		{
			if (component._Slowdown._MessageFunctionName.Length > 0)
			{
				component._Slowdown._MessageObject = pInstance.gameObject;
			}
			if (component._Halt._MessageFunctionName.Length > 0)
			{
				component._Halt._MessageObject = pInstance.gameObject;
			}
			if (component._Accelerate._MessageFunctionName.Length > 0)
			{
				component._Accelerate._MessageObject = pInstance.gameObject;
			}
			mCurrentControlModifier = component;
		}
	}

	public void ProcessExit(PetReadyEvent onPetReady)
	{
		if (SanctuaryManager.pCurPetData != mPlayerPetData || GauntletRailShootManager.pIsCrossbowLevel)
		{
			LoadPet(mPlayerPetData, onPetReady);
		}
		else
		{
			onPetReady?.Invoke();
		}
	}

	public void ShootFireBall()
	{
		if (_CanShoot && mShootTimer <= 0f)
		{
			mShoot = true;
		}
	}

	public void OnDragStart(Vector2 inVecPosition, int inFingerID)
	{
		mDragFingerId = inFingerID;
	}

	public bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (mDragFingerId != inFingerID)
		{
			return false;
		}
		mDragDir = inNewPosition - inOldPosition;
		mDragMag = mDragDir.magnitude;
		mDragDir.Normalize();
		return false;
	}

	public void OnDragEnd(Vector2 inVecPosition, int inFingerID)
	{
		if (mDragFingerId == inFingerID)
		{
			mDragFingerId = -1;
		}
	}
}
