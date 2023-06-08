using System.Collections;
using UnityEngine;

public class ActionUseProp : ActionBase
{
	public int[] _RequiredItemIDs;

	public string _PropName = "Chop";

	public PropStates _Prop;

	public float _WaitTimeSeconds = 2.5f;

	public Animation _Animation;

	public string _DoneAnimation = "";

	public GameObject _DoneParticles;

	public bool _AffectColliders;

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
		StartCoroutine("TimerDone");
		if (!string.IsNullOrEmpty(_PropName))
		{
			AvAvatar.UseProp(_PropName);
		}
		else
		{
			AvAvatar.UseProp(_Prop);
		}
		if (!string.IsNullOrEmpty(_ActionName))
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _ActionName, base.gameObject.name);
		}
	}

	private IEnumerator TimerDone()
	{
		yield return new WaitForSeconds(_WaitTimeSeconds);
		Done();
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
				if (!CommonInventoryData.pIsReady)
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

	private void Done()
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
		if (!string.IsNullOrEmpty(_DoneAnimation) && _Animation != null)
		{
			_Animation.Play(_DoneAnimation);
		}
		if (_DoneParticles != null)
		{
			Object.Instantiate(_DoneParticles, base.transform.position, Quaternion.identity);
		}
	}
}
