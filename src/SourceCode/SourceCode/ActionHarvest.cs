using System.Collections;
using UnityEngine;

public class ActionHarvest : ActionBase
{
	public float _HarvestWaitTimeSeconds = 2.5f;

	public Animation _Animation;

	public string _PropName = "Chop";

	public int _HarvestAmount = 1;

	public string _HarvestAnimation = "";

	public int[] _RequiredItemIDs;

	public GameObject _HarvestedObject;

	public GameObject _Particles;

	public GameObject _PostHarvestObject;

	public float _PostHarvestDestroyTime;

	public GameObject _PostHarvestDestroyObject;

	public float _RespawnTime = 10f;

	public string _RespawnAnimation = "";

	public bool _AffectColliders;

	private GameObject mParticles;

	private GameObject mPostHarvestObject;

	private bool mInitRegisterEvent;

	private bool mPrevState;

	private bool mOnGround;

	private AvAvatarController mAvController;

	public void Update()
	{
		if (mAvController == null && AvAvatar.pObject != null)
		{
			mAvController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (SanctuaryManager.pCurPetInstance != null && !mInitRegisterEvent)
		{
			mInitRegisterEvent = true;
			SanctuaryPet.AddMountEvent(SanctuaryManager.pCurPetInstance, DragonMounted);
		}
		if (!(mAvController != null))
		{
			return;
		}
		mOnGround = mAvController.OnGround();
		if (mOnGround != mPrevState)
		{
			mPrevState = mOnGround;
			if (!mPrevState)
			{
				mCSM.RefreshUI();
			}
			else
			{
				mCSM.SetProximityEntered();
			}
		}
	}

	public void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mount)
		{
			mCSM.RefreshUI();
		}
		else
		{
			mCSM.SetProximityEntered();
		}
	}

	public override void ExecuteAction()
	{
		AvAvatar.mTransform.LookAt(base.transform);
		AvAvatar.mTransform.eulerAngles = new Vector3(0f, AvAvatar.mTransform.eulerAngles.y, 0f);
		StartCoroutine("HarvestTimerDone");
		AvAvatar.UseProp(_PropName);
		if (!string.IsNullOrEmpty(_ActionName))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _ActionName, base.gameObject.name);
		}
	}

	private IEnumerator HarvestTimerDone()
	{
		yield return new WaitForSeconds(_HarvestWaitTimeSeconds);
		HarvestDone();
	}

	public override bool IsActionAllowed()
	{
		if (base.IsActionAllowed())
		{
			if (mAvController != null && (!mAvController.OnGround() || mAvController.pPlayerMounted))
			{
				return false;
			}
			if (_RequiredItemIDs != null && _RequiredItemIDs.Length != 0)
			{
				if (CommonInventoryData.pInstance == null)
				{
					return false;
				}
				int[] requiredItemIDs = _RequiredItemIDs;
				foreach (int itemID in requiredItemIDs)
				{
					if (CommonInventoryData.pInstance.FindItem(itemID) != null)
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void HarvestDone()
	{
		AvAvatar.StopUseProp();
		if (mCSM != null && mCSM._RenderingObject != null)
		{
			mCSM._RenderingObject.SetActive(value: false);
		}
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null)
		{
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}
		if (_AffectColliders)
		{
			Collider[] componentsInChildren2 = base.transform.GetComponentsInChildren<Collider>();
			if (componentsInChildren2 != null)
			{
				Collider[] array2 = componentsInChildren2;
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i].enabled = false;
				}
			}
		}
		if (_PostHarvestObject != null)
		{
			mPostHarvestObject = Object.Instantiate(_PostHarvestObject, base.transform.position, Quaternion.identity);
		}
		GenerateItems(_HarvestAmount);
		if (!string.IsNullOrEmpty(_HarvestAnimation) && _Animation != null)
		{
			_Animation.Play(_HarvestAnimation);
		}
		if (_Particles != null)
		{
			mParticles = Object.Instantiate(_Particles, base.transform.position, Quaternion.identity);
		}
		if (_RespawnTime > 0f)
		{
			Invoke("Respawn", _RespawnTime);
		}
		if (_PostHarvestDestroyTime > 0f && _PostHarvestDestroyObject != null)
		{
			Invoke("PostHarvestDestroy", _PostHarvestDestroyTime);
		}
	}

	private void Respawn()
	{
		if (mParticles != null)
		{
			Object.Destroy(mParticles);
		}
		if (mPostHarvestObject != null)
		{
			Object.Destroy(mPostHarvestObject);
		}
		if (!string.IsNullOrEmpty(_RespawnAnimation) && _Animation != null)
		{
			_Animation.Play(_RespawnAnimation);
		}
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		if (componentsInChildren != null)
		{
			Renderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}
		if (!_AffectColliders)
		{
			return;
		}
		Collider[] componentsInChildren2 = base.transform.GetComponentsInChildren<Collider>();
		if (componentsInChildren2 != null)
		{
			Collider[] array2 = componentsInChildren2;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].enabled = true;
			}
		}
	}

	private void PostHarvestDestroy()
	{
		if (AvAvatar.pToolbar != null)
		{
			AvAvatar.pToolbar.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		if (_PostHarvestDestroyObject != null)
		{
			Object.Destroy(_PostHarvestDestroyObject);
		}
	}

	private void GenerateItems(int numItemsToEmit)
	{
		if (_HarvestedObject != null)
		{
			ObBouncyCoinEmitter component = GetComponent<ObBouncyCoinEmitter>();
			if (component != null)
			{
				component._Coin = _HarvestedObject;
				component._CoinsToEmit._Min = (component._CoinsToEmit._Max = numItemsToEmit);
				component.GenerateCoins();
			}
		}
	}
}
