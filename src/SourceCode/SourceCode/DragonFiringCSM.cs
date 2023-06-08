using System.Collections;
using UnityEngine;

public class DragonFiringCSM : ObContextSensitive
{
	public ContextSensitiveState[] _Menus;

	public GameObject _RenderingObject;

	public Material _MaterialSwap;

	public GameObject _FireObject;

	public float _FlameTimer = 5f;

	public bool _DisableWhenMounted;

	public bool _FlashFireButton;

	private KAWidget mFireButton;

	private Material mMaterialSave;

	protected override void Update()
	{
		if (_DisableWhenMounted && SanctuaryManager.pCurPetInstance != null && (SanctuaryManager.pCurPetInstance.pIsMounted || !AvAvatar.GetUIActive()))
		{
			CloseMenu();
		}
		else
		{
			base.Update();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		SetProximityAlreadyEntered(isEntered: false);
		DestroyMenu(checkProximity: false);
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		SetInteractiveEnabledData("Fire", SanctuaryManager.pCurPetInstance != null);
		inStatesArrData = _Menus;
	}

	public void OnContextAction(string inName)
	{
		if (inName == "Fire")
		{
			if (SanctuaryManager.pCurPetInstance != null)
			{
				CloseMenu(checkProximity: true);
				MakePetLookAt(base.transform);
				SanctuaryManager.pCurPetInstance.Fire(base.transform, useDirection: false, Vector3.zero);
				if (SanctuaryManager.pInstance._FirePitFireAchievementID > 0)
				{
					WsWebService.SetUserAchievementAndGetReward(SanctuaryManager.pInstance._FirePitFireAchievementID, AchievementEventHandler, null);
				}
			}
		}
		else
		{
			UtDebug.Log(inName + " Not defined in _DataList");
		}
	}

	private void AchievementEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			UtDebug.LogError("!!!" + inType.ToString() + " failed!!!!");
			break;
		case WsServiceEvent.COMPLETE:
			if (inObject != null)
			{
				GameUtilities.AddRewards((AchievementReward[])inObject, inUseRewardManager: false, inImmediateShow: false);
			}
			else
			{
				UtDebug.LogError("!!!" + inType.ToString() + " did not return valid object!!!!");
			}
			break;
		}
	}

	private void OnAmmoHit(ObAmmo ammo)
	{
		ProcessOnHit();
	}

	protected virtual void ProcessOnHit()
	{
		if (_FireObject != null)
		{
			if (_FireObject.activeSelf)
			{
				return;
			}
			_FireObject.SetActive(value: true);
			StartCoroutine(ExtinguishFlame());
		}
		else if (_RenderingObject != null)
		{
			_RenderingObject.SetActive(value: false);
		}
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "LightFire", base.gameObject.name);
		}
		if (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pInstance._PetCollectItemAchievementID > 0)
		{
			WsWebService.SetUserAchievementAndGetReward(SanctuaryManager.pInstance._PetCollectItemAchievementID, AchievementEventHandler, null);
		}
		if (_RenderingObject != null && _MaterialSwap != null)
		{
			Renderer component = _RenderingObject.GetComponent<Renderer>();
			if (component != null)
			{
				mMaterialSave = component.material;
				component.material = _MaterialSwap;
			}
		}
	}

	private void MakePetLookAt(Transform transform)
	{
		Vector3 forward = transform.position - SanctuaryManager.pCurPetInstance.transform.position;
		forward.Normalize();
		SanctuaryManager.pCurPetInstance.transform.rotation = Quaternion.LookRotation(forward);
		SanctuaryManager.pCurPetInstance.transform.eulerAngles = new Vector3(0f, SanctuaryManager.pCurPetInstance.transform.eulerAngles.y, 0f);
	}

	protected IEnumerator ExtinguishFlame()
	{
		if (!(_FlameTimer > 0f))
		{
			yield break;
		}
		yield return new WaitForSeconds(_FlameTimer);
		if (_FireObject != null)
		{
			_FireObject.SetActive(value: false);
		}
		if (_RenderingObject != null && _MaterialSwap != null)
		{
			Renderer component = _RenderingObject.GetComponent<Renderer>();
			if (component != null)
			{
				component.material = mMaterialSave;
			}
		}
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		base.OnMenuActive(inMenuType);
		if (_FlashFireButton && base.pUI != null)
		{
			if (mFireButton == null)
			{
				mFireButton = base.pUI.FindItem("Fire");
			}
			if ((bool)mFireButton)
			{
				mFireButton.PlayAnim("Flash");
			}
			else
			{
				UtDebug.LogError("mFireButton is null!");
			}
		}
	}
}
