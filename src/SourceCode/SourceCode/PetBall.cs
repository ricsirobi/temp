using UnityEngine;

public class PetBall : PetToy
{
	public AudioClip _SFXTossBall;

	public AudioClip _SFXPickupBall;

	public AudioClip _SFXReturnBall;

	public AudioClip _SFXBallHitWall;

	public float _ThrownScale = 3f;

	public Vector3 _InHandRotAngle = new Vector3(25f, 0f, 0f);

	private bool mBallWasThrown;

	protected Rigidbody mRigidBody;

	public virtual void Start()
	{
		mRigidBody = base.rigidbody;
		DisablePhysics();
		mBallWasThrown = false;
	}

	public void DisablePhysics()
	{
		mRigidBody.isKinematic = true;
	}

	public virtual void Throw(Vector3 v, Vector3 t)
	{
		mPlayUI.pPet._NoWalk = true;
		if (_SFXTossBall != null)
		{
			SnSettings inSettings = new SnSettings();
			SnChannel.Play(_SFXTossBall, inSettings, inForce: true);
		}
		base.rigidbody.isKinematic = false;
		base.rigidbody.velocity = Vector3.zero;
		base.rigidbody.AddForce(v, ForceMode.VelocityChange);
		base.rigidbody.AddTorque(t * (Random.value * 10f + 10f));
		mBallWasThrown = true;
	}

	public virtual void DoReturn()
	{
		base.transform.parent = null;
		mPlayUI.SetState(KAUIState.INTERACTIVE);
		if (_SFXReturnBall != null)
		{
			SnSettings inSettings = new SnSettings();
			SnChannel.Play(_SFXReturnBall, inSettings, inForce: true);
		}
		DisablePhysics();
		mPlayUI.pPet.SetState(Character_State.idle);
		mPlayUI.pPetPlayedAtleastOnce = true;
		base.transform.localScale = Vector3.one;
		mPlayUI.pPet.UpdateActionMeters(PetActions.FETCHBALL, 1f, doUpdateSkill: true);
		mPlayUI.pPet.CheckForTaskCompletion(PetActions.FETCHBALL);
		mPlayUI.ReAttachToy(PetActions.FETCHBALL);
		mBallWasThrown = false;
	}

	public virtual void DoPickup()
	{
		DisablePhysics();
		if (_SFXPickupBall != null)
		{
			SnSettings inSettings = new SnSettings();
			SnChannel.Play(_SFXPickupBall, inSettings, inForce: true);
		}
		mPlayUI.pPet.DoPickUp(base.gameObject, PetToyType.BALL);
	}

	public void WaitForThrowBall(AIActor Actor)
	{
		if (mBallWasThrown)
		{
			Actor.BehaviorFunctionCallResult = AIBehaviorState.COMPLETED;
		}
		else
		{
			Actor.BehaviorFunctionCallResult = AIBehaviorState.ACTIVE;
		}
	}

	public void Update()
	{
		if (!mBallWasThrown && KAUI._GlobalExclusiveUI == null && KAUI.GetGlobalMouseOverItem() == null && Input.GetMouseButtonUp(0) && Input.mousePosition.y >= 0f && Input.mousePosition.y <= (float)Screen.height && Input.mousePosition.x >= 0f && Input.mousePosition.x <= (float)Screen.width)
		{
			Camera camera = SanctuaryManager.pCurPetInstance.GetCamera();
			Vector3 position = base.transform.position;
			position.y += 0.3f;
			position -= camera.transform.position;
			position *= 56f;
			Throw(position, camera.transform.right);
			base.transform.localScale = Vector3.one * _ThrownScale;
			mPlayUI.pObjectAttachToMouse = false;
			if (mPlayUI.pObjectInHand != null)
			{
				Physics.IgnoreCollision(SanctuaryManager.pCurPetInstance.collider, mPlayUI.pObjectInHand.GetComponent<Collider>());
			}
			mBallWasThrown = true;
		}
	}

	public void OnCollisionEnter(Collision collision)
	{
		if (_SFXBallHitWall != null)
		{
			SnSettings inSettings = new SnSettings();
			SnChannel.Play(_SFXBallHitWall, inSettings, inForce: true);
		}
	}
}
