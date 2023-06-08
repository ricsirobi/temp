using System;
using UnityEngine;

public class MMOAvatarSimulator : MonoBehaviour
{
	[Serializable]
	public class MinMax
	{
		public float _Min;

		public float _Max;
	}

	public float _WalkSpeed = 5f;

	public float _DestinationDistanceThershold = 5f;

	public string _DefaultUserID = "f30c0d85-3d75-4c61-b302-f6669252c3d2";

	public MinMax _EventChangeMinMax;

	public MinMax _RandomPosOffsetMinMax;

	public MinMax _SplineMoveSpeedMinMax;

	public MinMax _PetRideMoveSpeedMinMax;

	private MMOAvatar mMMOAvatar;

	private AVATAR_SIMULATIOM_STATE mState;

	private float mTimeForEventChange;

	private SimulationManager mSimulationManager;

	private SanctuaryPet mMyPet;

	private SplineControl mSplineControl;

	private int mCurZone = -1;

	private AvAvatarController mAvatarController;

	private SanctuaryPetTypeInfo mPetTypeInfo;

	public MMOAvatar Init(SimulationManager manager, int curZone)
	{
		mCurZone = curZone;
		mSimulationManager = manager;
		mMMOAvatar = MMOAvatar.CreateAvatar(_DefaultUserID, "Simulated", Gender.Female);
		if ((bool)mMMOAvatar)
		{
			float randomOffset = UnityEngine.Random.Range(_RandomPosOffsetMinMax._Min, _RandomPosOffsetMinMax._Max);
			Vector3 vector = GetRandomPos(AvAvatar.position + Vector3.up * mSimulationManager._HeightOffset, randomOffset);
			float groundHeight = 0f;
			UtUtilities.GetGroundHeight(vector, 100f, out groundHeight);
			if (groundHeight < -10000f)
			{
				vector = AvAvatar.position;
			}
			else
			{
				vector.y = groundHeight;
			}
			mMMOAvatar.pController.SetPosition(vector);
			base.transform.position = mMMOAvatar.pObject.transform.position;
			mMMOAvatar.Load(AvatarData.CreateDefault(), reload: true);
			mMMOAvatar.SetLevel(RsResourceManager.pCurrentLevel);
			mMMOAvatar.Activate(active: true);
			mSimulationManager.mRandomAvatarGenerator.GenerateRandomAvatar(mMMOAvatar.pObject);
			SetState(AVATAR_SIMULATIOM_STATE.IDLE);
			RaisedPetStage raisedPetStage = (RaisedPetStage)UnityEngine.Random.Range(5, 9);
			if (raisedPetStage != RaisedPetStage.HATCHING)
			{
				SanctuaryData sanctuaryData = UnityEngine.Object.FindObjectOfType(typeof(SanctuaryData)) as SanctuaryData;
				if ((bool)sanctuaryData)
				{
					SanctuaryPetTypeInfo sanctuaryPetTypeInfo = sanctuaryData._PetTypes[UnityEngine.Random.Range(0, sanctuaryData._PetTypes.Length)];
					string resName = string.Empty;
					Gender gender = Gender.Unknown;
					if (raisedPetStage != RaisedPetStage.EGGINHAND)
					{
						int ageIndex = GetAgeIndex(raisedPetStage);
						SantuayPetResourceInfo obj = sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList[UnityEngine.Random.Range(0, sanctuaryPetTypeInfo._AgeData[ageIndex]._PetResList.Length)];
						resName = obj._Prefab;
						gender = obj._Gender;
					}
					SanctuaryManager.CreatePet(RaisedPetData.CreateCustomizedPetData(sanctuaryPetTypeInfo._TypeID, raisedPetStage, resName, gender, null, noColorMap: true), Vector3.zero, Quaternion.identity, base.gameObject, "Player");
				}
				else
				{
					Debug.LogError("Sanctuary data not present, Not Able to simulate pet");
				}
			}
			mAvatarController = mMMOAvatar.pObject.GetComponent<AvAvatarController>();
			mMMOAvatar.transform.parent = base.transform;
		}
		return mMMOAvatar;
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		if ((bool)mMMOAvatar)
		{
			pet.SetAvatar(mMMOAvatar.pObject.transform);
			pet.MoveToAvatar();
		}
		mPetTypeInfo = pet.GetTypeInfo();
		pet.pMeterPaused = true;
		pet.SetFollowAvatar(follow: true);
		pet.LoadAvatarMountAnimation();
		mMyPet = pet;
	}

	public void PlayAnim(GameObject inAvatar, string inAnimName, WrapMode inWrapMode)
	{
		if (!(inAvatar != null))
		{
			return;
		}
		Component[] componentsInChildren = inAvatar.GetComponentsInChildren<Animation>();
		componentsInChildren = componentsInChildren;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Animation animation = (Animation)componentsInChildren[i];
			if (!(animation[inAnimName] == null))
			{
				animation[inAnimName].wrapMode = inWrapMode;
				animation.Play(inAnimName);
			}
		}
	}

	private AVATAR_SIMULATIOM_STATE GetRandomState()
	{
		if ((bool)mMyPet && mMyPet.pData.pStage == RaisedPetStage.ADULT && UnityEngine.Random.value > 0.75f)
		{
			return AVATAR_SIMULATIOM_STATE.RIDE;
		}
		return (AVATAR_SIMULATIOM_STATE)UnityEngine.Random.Range(0, 2);
	}

	public AvAvatarStateData GetStateDataFromSubState(AvAvatarSubState subState)
	{
		if (mMMOAvatar == null || mMMOAvatar.pController == null || mMMOAvatar.pController._StateData == null)
		{
			return null;
		}
		AvAvatarStateData[] stateData = mMMOAvatar.pController._StateData;
		foreach (AvAvatarStateData avAvatarStateData in stateData)
		{
			if (avAvatarStateData._State == subState)
			{
				return avAvatarStateData;
			}
		}
		return null;
	}

	public void SetState(AVATAR_SIMULATIOM_STATE newState)
	{
		if (!mMMOAvatar)
		{
			return;
		}
		if (mState == AVATAR_SIMULATIOM_STATE.RIDE)
		{
			mMyPet.SendMessage("OnFlyDismount", mMMOAvatar.pObject);
			mMMOAvatar.pObject = mAvatarController.gameObject;
			mMMOAvatar.pController = mAvatarController;
		}
		switch (newState)
		{
		case AVATAR_SIMULATIOM_STATE.WALK:
		{
			Spline randomSpline2 = mSimulationManager.GetRandomSpline(mCurZone);
			if (randomSpline2 != null)
			{
				mMMOAvatar.pController.SetSpline(randomSpline2);
				mMMOAvatar.pController.Speed = UnityEngine.Random.Range(_SplineMoveSpeedMinMax._Min, _SplineMoveSpeedMinMax._Max);
				mMMOAvatar.pController.pState = AvAvatarState.ONSPLINE;
				AvAvatarStateData stateDataFromSubState2 = GetStateDataFromSubState(AvAvatarSubState.NORMAL);
				PlayAnim(mMMOAvatar.pObject, stateDataFromSubState2._StateAnims._Run, WrapMode.Loop);
				break;
			}
			SetState(GetRandomState());
			return;
		}
		case AVATAR_SIMULATIOM_STATE.IDLE:
		{
			mMMOAvatar.pController.pState = AvAvatarState.IDLE;
			mMMOAvatar.pController.ClearSpline();
			bool num = UnityEngine.Random.value > 0.5f;
			AvAvatarStateData stateDataFromSubState = GetStateDataFromSubState(AvAvatarSubState.NORMAL);
			string inAnimName = stateDataFromSubState._StateAnims._Idle;
			if (num)
			{
				int num2 = UnityEngine.Random.Range(0, stateDataFromSubState._StateAnims._Fidgets.Length);
				inAnimName = stateDataFromSubState._StateAnims._Fidgets[num2];
			}
			PlayAnim(mMMOAvatar.pObject, inAnimName, WrapMode.Loop);
			break;
		}
		case AVATAR_SIMULATIOM_STATE.RIDE:
		{
			Spline randomSpline = mSimulationManager.GetRandomSpline(mCurZone);
			if (randomSpline != null)
			{
				mMyPet.Mount(mMMOAvatar.pObject, mMyPet.GetTypeInfo()._SpecialSkill);
				if (mPetTypeInfo._SpecialSkill == PetSpecialSkillType.FLY)
				{
					mSplineControl = mMMOAvatar.pController;
					mMyPet.PlayFlyAnimation(Character_FlyStage.fly);
				}
				else
				{
					AvAvatarController component = mMyPet.transform.parent.GetComponent<AvAvatarController>();
					if ((bool)component)
					{
						component.pState = AvAvatarState.ONSPLINE;
						mSplineControl = component;
					}
				}
				mSplineControl.SetSpline(randomSpline);
				mSplineControl.Speed = UnityEngine.Random.Range(_PetRideMoveSpeedMinMax._Min, _PetRideMoveSpeedMinMax._Max);
				break;
			}
			SetState(GetRandomState());
			return;
		}
		}
		mState = newState;
		mTimeForEventChange = UnityEngine.Random.Range(_EventChangeMinMax._Min, _EventChangeMinMax._Max);
	}

	public Vector3 GetRandomPos(Vector3 inPosition, float randomOffset)
	{
		if (randomOffset > 0f)
		{
			float num = UnityEngine.Random.Range(0, 24) * 15;
			float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * randomOffset;
			float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * randomOffset;
			inPosition.x += num2;
			inPosition.z += num3;
		}
		return inPosition;
	}

	public void DestroyAvatar()
	{
		if ((bool)mMMOAvatar)
		{
			UnityEngine.Object.Destroy(mMMOAvatar.pObject);
			mMMOAvatar.Unload();
		}
		if ((bool)mMyPet)
		{
			UnityEngine.Object.Destroy(mMyPet.gameObject);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (!mMMOAvatar)
		{
			return;
		}
		switch (mState)
		{
		case AVATAR_SIMULATIOM_STATE.WALK:
			if (mMMOAvatar.pController.mEndReached)
			{
				mTimeForEventChange = 0f;
			}
			break;
		case AVATAR_SIMULATIOM_STATE.RIDE:
			if (mPetTypeInfo._SpecialSkill != PetSpecialSkillType.FLY)
			{
				AvAvatarStateData stateDataFromSubState = GetStateDataFromSubState(AvAvatarSubState.NORMAL);
				if (stateDataFromSubState != null)
				{
					string walk = stateDataFromSubState._StateAnims._Walk;
					if (!mMyPet.IsAnimationPlaying(walk))
					{
						mMyPet.PlayAnim(stateDataFromSubState._StateAnims._Walk, -1, 1f, 0);
					}
					if (mMMOAvatar.pController.pCurrentAnim != walk)
					{
						PlayAnim(mAvatarController.gameObject, walk, WrapMode.Loop);
					}
				}
			}
			mSplineControl.MoveOnSpline(Time.deltaTime * mSplineControl.Speed);
			if (mSplineControl.mEndReached)
			{
				mTimeForEventChange = 0f;
			}
			break;
		}
		mTimeForEventChange -= Time.deltaTime;
		if (mTimeForEventChange <= 0f)
		{
			SetState(GetRandomState());
		}
	}

	public int GetAgeIndex(RaisedPetStage rs)
	{
		int result = -1;
		if (rs == RaisedPetStage.BABY)
		{
			result = 0;
		}
		if (rs == RaisedPetStage.CHILD)
		{
			result = 1;
		}
		if (rs == RaisedPetStage.TEEN)
		{
			result = 2;
		}
		if (rs == RaisedPetStage.ADULT)
		{
			result = 3;
		}
		return result;
	}
}
