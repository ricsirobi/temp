using UnityEngine;

public class ActionCarryObject : ActionBase
{
	public bool _DuplicateItem;

	public string _PickCSMButtonSprite;

	public string _DropCSMButtonSprite;

	private bool mInitRegisterEvent;

	private bool mPrevState;

	private bool mIsCSMAllowed;

	private AvAvatarController mAvController;

	private Vector3 mOrgPos = Vector3.zero;

	private Quaternion mOrgRot = Quaternion.identity;

	private Transform mOrgParent;

	public override void Start()
	{
		mOrgPos = base.transform.localPosition;
		mOrgRot = base.transform.localRotation;
		mOrgParent = base.transform.parent;
		base.Start();
	}

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
		mIsCSMAllowed = IsCSMAllowed();
		if (mIsCSMAllowed != mPrevState)
		{
			mPrevState = mIsCSMAllowed;
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

	private bool IsCSMAllowed()
	{
		if (mAvController.pSubState == AvAvatarSubState.NORMAL && (mAvController.pState == AvAvatarState.IDLE || mAvController.pState == AvAvatarState.MOVING || (mAvController.pPlayerCarrying && mAvController.pState == AvAvatarState.FALLING)))
		{
			return true;
		}
		return false;
	}

	public void DragonMounted(bool mount, PetSpecialSkillType skill)
	{
		if (mount)
		{
			mCSM.RefreshUI();
			if (mAvController.pPlayerCarrying)
			{
				DropObject();
			}
		}
		else
		{
			mCSM.SetProximityEntered();
		}
	}

	public override void ExecuteAction()
	{
		if (mAvController.pPlayerCarrying)
		{
			mAvController.RemoveCarriedObject();
			return;
		}
		mAvController.CarryObject(base.gameObject, _DuplicateItem);
		mCSM.UpdateButtonSprite(_ActionName, _DropCSMButtonSprite);
	}

	public void DropObject()
	{
		base.transform.parent = mOrgParent;
		base.transform.localPosition = mOrgPos;
		base.transform.localRotation = mOrgRot;
		base.gameObject.GetComponent<Collider>().enabled = true;
		base.enabled = true;
		mCSM.UpdateButtonSprite(_ActionName, _PickCSMButtonSprite);
		mCSM.CloseMenu();
	}

	public override bool IsActionAllowed()
	{
		if (base.IsActionAllowed())
		{
			if (mAvController != null && (!IsCSMAllowed() || mAvController.pPlayerMounted))
			{
				return false;
			}
			return true;
		}
		return false;
	}
}
