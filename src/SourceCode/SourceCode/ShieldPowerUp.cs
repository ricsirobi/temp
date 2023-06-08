using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class ShieldPowerUp : PowerUp
{
	public int _MaxHitSustain = 1;

	public GameObject _ShieldObject;

	private AvAvatarController mController;

	private float mTimer;

	private GameObject mShieldObj;

	private bool mIsMMO;

	public override void Init(MonoBehaviour gameManager, PowerUpManager manager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, manager, args);
		if (mPowerUpManager.pPowerUpHelper._GetParentObject != null && mPowerUpManager.pPowerUpHelper._IsMMOUser != null)
		{
			bool isMMO = mPowerUpManager.pPowerUpHelper._IsMMOUser(args);
			GameObject gameObject = mPowerUpManager.pPowerUpHelper._GetParentObject(args);
			if (gameObject != null)
			{
				ActivateShield(gameObject, isMMO);
			}
		}
	}

	public override void Activate()
	{
		SendMMOMessage("POWERUP:" + _Type);
	}

	private void ActivateShield(GameObject inParent, bool isMMO)
	{
		mIsMMO = isMMO;
		CheckShieldReset(inParent);
		mActive = true;
		mController = inParent.GetComponent<AvAvatarController>();
		base.transform.parent = mController.transform;
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if (!mIsMMO)
		{
			PowerUp.pImmune = true;
			PowerUp.pHitCount = 0;
			mController.SetImmune(PowerUp.pImmune, _Duration);
		}
		if (_ShieldObject != null)
		{
			if (mShieldObj == null)
			{
				mShieldObj = Object.Instantiate(_ShieldObject);
				mShieldObj.transform.parent = base.transform;
				Vector3 localPosition = new Vector3(0f, 0f, 0f);
				mShieldObj.transform.localPosition = localPosition;
				mShieldObj.transform.localRotation = Quaternion.identity;
			}
			else
			{
				mShieldObj.SetActive(value: true);
			}
		}
		if (_SndActivate != null)
		{
			SnChannel.Play(_SndActivate, "SFX_Pool", inForce: true, null);
		}
	}

	private void CheckShieldReset(GameObject inParent)
	{
		if (!mIsMMO && PowerUp.pImmune)
		{
			PowerUp.pHitCount = 0;
		}
		ShieldPowerUp componentInChildren = inParent.GetComponentInChildren<ShieldPowerUp>();
		if (componentInChildren != null)
		{
			componentInChildren.DeActivate();
		}
	}

	public override void Update()
	{
		if (!mActive)
		{
			return;
		}
		if (!mIsMMO && _MaxHitSustain > 0 && PowerUp.pHitCount >= _MaxHitSustain)
		{
			DeActivate();
		}
		if (_Duration > 0f)
		{
			if (mTimer >= _Duration)
			{
				DeActivate();
			}
			else
			{
				mTimer += Time.deltaTime;
			}
		}
	}

	public override void DeActivate()
	{
		base.DeActivate();
		if (mShieldObj != null && mShieldObj.activeSelf)
		{
			mShieldObj.SetActive(value: false);
		}
		mTimer = 0f;
		if (!mIsMMO)
		{
			PowerUp.pImmune = false;
			PowerUp.pHitCount = 0;
			mController.SetImmune(PowerUp.pImmune);
		}
		base.transform.parent = null;
	}
}
