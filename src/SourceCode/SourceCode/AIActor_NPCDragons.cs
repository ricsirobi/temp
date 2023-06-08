using UnityEngine;

public class AIActor_NPCDragons : AIActor_NPC
{
	public string _FlyAnim = "FlyForward";

	protected bool mIsFlying;

	protected SanctuaryPet mSanctuaryPet;

	protected Transform mParentTransform;

	public void Start()
	{
		mSanctuaryPet = GetComponent<SanctuaryPet>();
		if (mSanctuaryPet == null)
		{
			NPCPetManager component = GetComponent<NPCPetManager>();
			if (component != null && component._PetData.Count > 0)
			{
				component.OnPetReadyEvent += OnPetReady;
			}
		}
	}

	public override void DoAction(string action, params object[] values)
	{
		switch (action)
		{
		case "MissionComplete":
			SetState(NPC_FSM.NORMAL);
			break;
		case "InitFlying":
			InitFlying();
			break;
		case "InitLanding":
			InitLanding();
			break;
		}
		base.DoAction(action, values);
	}

	public override void Update()
	{
		base.Update();
		if (mIsFlying && mSanctuaryPet != null)
		{
			if (mSanctuaryPet.gameObject != base.gameObject)
			{
				_Animation.Play(_FlyAnim);
			}
			mSanctuaryPet.animation.CrossFade(mSanctuaryPet.GetFlyAnim());
		}
	}

	public void InitFlying()
	{
		if (mSanctuaryPet != null && mSanctuaryPet.gameObject != base.gameObject)
		{
			mParentTransform = base.transform.parent;
			Transform transform = UtUtilities.FindChildTransform(mSanctuaryPet.gameObject, mSanctuaryPet._Bone_Seat);
			base.transform.position = transform.position;
			base.transform.rotation = mSanctuaryPet.transform.rotation;
			Vector3 position = base.transform.TransformPoint(mSanctuaryPet._PillionOffset);
			base.transform.position = position;
			base.transform.parent = transform.transform;
			mSanctuaryPet.SetFollowAvatar(follow: false);
		}
		mIsFlying = true;
	}

	public void InitLanding()
	{
		if (mSanctuaryPet != null && mSanctuaryPet.gameObject != base.gameObject)
		{
			mSanctuaryPet.SetFollowAvatar(follow: true);
			base.transform.parent = mParentTransform;
		}
		mIsFlying = false;
	}

	public override bool IsFlying()
	{
		return mIsFlying;
	}

	public override Transform GetTransformOnFlying()
	{
		if (mIsFlying && mSanctuaryPet != null)
		{
			return mSanctuaryPet.transform;
		}
		return base.transform;
	}

	public void OnPetReady(SanctuaryPet pet)
	{
		mSanctuaryPet = pet;
	}
}
