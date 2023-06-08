using UnityEngine;

public class NPCHiccup : NPCAvatar
{
	public int _RideToothlessTask = 242;

	public string _ManagerBundlePath = "RS_DATA/PfDragonTaxiManagerDO.unity3d/PfDragonTaxiManagerDO";

	public TaxiDragonData _TaxiDragonData;

	public TaxiUISceneData _TargetSceneToLoadData;

	public int _RideToothlessAchievement = 219;

	public string _TargetSceneForMobile;

	private SanctuaryPet mPet;

	private Task mRideTask;

	private ObClickable mPetClickableComponent;

	private CapsuleCollider mPetCollider;

	private bool mHandledTaxi;

	public override void Update()
	{
		base.Update();
		if (!(MissionManager.pInstance != null) || !(mPet != null))
		{
			return;
		}
		if (mRideTask == null)
		{
			mRideTask = MissionManager.pInstance.GetTask(_RideToothlessTask);
		}
		if (mRideTask == null)
		{
			return;
		}
		if (mPetClickableComponent == null)
		{
			mPetClickableComponent = mPet.GetComponent<ObClickable>();
		}
		if (!(mPetClickableComponent != null))
		{
			return;
		}
		if (!mHandledTaxi && mRideTask._Active)
		{
			if (mPetCollider == null)
			{
				mPetCollider = mPet.collider as CapsuleCollider;
				mPetCollider.center -= Vector3.forward;
			}
			mPetClickableComponent.enabled = true;
			mPetClickableComponent._Active = true;
			mPetClickableComponent._MessageObject = base.gameObject;
		}
		else
		{
			mPetClickableComponent.enabled = false;
		}
	}

	public void SetPetFollow(bool inFollow)
	{
		if (mPet != null)
		{
			mPet.SetFollowAvatar(inFollow);
		}
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		mPet = pet;
	}

	private void OnClick()
	{
		if (!mHandledTaxi && _TargetSceneToLoadData != null)
		{
			mHandledTaxi = true;
			if (mPet != null)
			{
				mPet.SetFollowAvatar(follow: false);
			}
			AvAvatar.SetUIActive(inActive: false);
			UserAchievementTask.Set(_RideToothlessAchievement);
			if (UtPlatform.IsMobile() && !string.IsNullOrEmpty(_TargetSceneForMobile))
			{
				_TargetSceneToLoadData._LoadLevel = _TargetSceneForMobile;
			}
			DragonTaxiManager.InitTaxi(_ManagerBundlePath, _TargetSceneToLoadData, isSelectableTrigger: true, _TaxiDragonData, this);
		}
	}
}
