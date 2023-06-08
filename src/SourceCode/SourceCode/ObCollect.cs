using UnityEngine;

public class ObCollect : MonoBehaviour
{
	public AudioClip _Sound;

	public Transform _ParticleEffect;

	public GameObject _MessageObject;

	public bool _ProjectileCollectable;

	public float _RegenTime;

	public GameObject _CarryObject;

	public int _InventoryID = -1;

	public int _InventoryQuantity = 1;

	public bool _InventoryLocal;

	public ParticleSystem _GlowParticleSystem;

	public MinMax _GlowControlDistanceRange;

	public MinMax _GlowSizeRange;

	public GameObject _CollectorObject;

	public float _OverlapRadius;

	public float _AttractSpeed;

	private Transform mParticle;

	protected bool mCollected;

	protected bool mSendMessageOnly;

	private float mRegenTime;

	protected bool mMagnetEnabled;

	private bool mMoving;

	private Transform mMoveTarget;

	private SphereCollider mMagnetCollider;

	private Vector3 mOrgPos = new Vector3(0f, 0f, 0f);

	private Quaternion mOrgRot = Quaternion.identity;

	public bool pCollected => mCollected;

	protected virtual void OnTriggerEnter(Collider coll)
	{
		if (((!_ProjectileCollectable || !coll.gameObject.CompareTag("Projectile")) && !coll.gameObject.CompareTag("Player") && coll.transform.root != null && !coll.transform.root.CompareTag("Player")) || mCollected)
		{
			return;
		}
		if (_OverlapRadius > 0f && mMagnetEnabled && mMagnetCollider != null && coll.gameObject.CompareTag("Player"))
		{
			mMoveTarget = coll.transform;
			mMoving = true;
			CreateMagnetCollider(create: false);
			return;
		}
		if (_MessageObject != null)
		{
			_MessageObject.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.pToolbar.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			if (AvAvatar.pObject != null)
			{
				AvAvatar.pObject.BroadcastMessage("Collect", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (!mSendMessageOnly)
		{
			if (coll.gameObject.CompareTag("Projectile"))
			{
				coll.gameObject.GetComponent<PrProjectile>().DestroyMe();
			}
			OnItemCollected();
		}
	}

	public virtual void OnItemCollected()
	{
		if ((bool)_Sound)
		{
			SnChannel snChannel = SnChannel.Play(_Sound);
			if (snChannel != null)
			{
				snChannel.pLoop = false;
			}
		}
		if ((bool)_ParticleEffect)
		{
			mParticle = Object.Instantiate(_ParticleEffect, base.transform.position, base.transform.rotation);
			mParticle.parent = base.transform.parent;
		}
		if (CommonInventoryData.pInstance != null && _InventoryID != -1)
		{
			if (_InventoryLocal)
			{
				if (MissionManager.pInstance != null)
				{
					MissionManager.pInstance.Collect(base.gameObject);
				}
			}
			else
			{
				CommonInventoryData.pInstance.AddItem(_InventoryID, _InventoryQuantity);
				CommonInventoryData.pInstance.Save(InventorySaveEventHandler, base.gameObject.name);
			}
		}
		else if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.Collect(base.gameObject);
		}
		Collected();
	}

	private void InventorySaveEventHandler(bool success, object inUserData)
	{
		if (success && MissionManager.pInstance != null)
		{
			MissionManager.pInstance.Collect((string)inUserData);
		}
		if (this != null && _RegenTime <= 0f && base.transform.parent == null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected virtual void Collected()
	{
		mCollected = true;
		mRegenTime = _RegenTime;
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		mMoving = false;
		mMagnetEnabled = false;
		if (_OverlapRadius > 0f)
		{
			base.transform.localPosition = mOrgPos;
			base.transform.localRotation = mOrgRot;
		}
		ObClickable component = GetComponent<ObClickable>();
		if (component != null)
		{
			component.OnMouseExit();
			component.enabled = false;
		}
		ObBouncyCoin component2 = GetComponent<ObBouncyCoin>();
		if (component2 != null)
		{
			component2.Collected();
		}
		if (_GlowParticleSystem != null)
		{
			_GlowParticleSystem.Clear();
			_GlowParticleSystem.Stop();
		}
		if (_RegenTime <= 0f && _InventoryID <= 0 && base.transform.parent == null)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public virtual void Update()
	{
		if (mMoving && mMoveTarget != null)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			vector = Vector3.Lerp(base.transform.position, mMoveTarget.position, Time.deltaTime * _AttractSpeed);
			base.transform.position = vector;
		}
		if (mCollected && _RegenTime > 0f)
		{
			mRegenTime -= Time.deltaTime;
			if (mRegenTime < 0f)
			{
				Restart();
			}
		}
		UpdateGlowSize();
	}

	private void OnEnable()
	{
		mMoving = false;
		mMagnetEnabled = false;
		if (_OverlapRadius > 0f)
		{
			mOrgPos = base.transform.localPosition;
			mOrgRot = base.transform.localRotation;
		}
		Restart();
	}

	private void OnDisable()
	{
		if (_OverlapRadius > 0f)
		{
			base.transform.localPosition = mOrgPos;
			base.transform.localRotation = mOrgRot;
		}
		if (mParticle != null)
		{
			Object.Destroy(mParticle.gameObject);
			mParticle = null;
		}
	}

	public void Restart()
	{
		mCollected = false;
		Renderer[] componentsInChildren = base.transform.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		if ((bool)mParticle)
		{
			ParticleSystem component = mParticle.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Clear();
			}
		}
		ObClickable component2 = GetComponent<ObClickable>();
		if (component2 != null)
		{
			component2.enabled = true;
		}
		if (_GlowParticleSystem != null)
		{
			_GlowParticleSystem.Play();
		}
	}

	private void UpdateGlowSize()
	{
		if (!(_GlowParticleSystem != null) || mCollected)
		{
			return;
		}
		GameObject gameObject = ((_CollectorObject != null) ? _CollectorObject : AvAvatar.pObject);
		if (!(gameObject == null))
		{
			ParticleSystem.MainModule main = _GlowParticleSystem.main;
			float num = Vector3.Distance(gameObject.transform.position, base.transform.position);
			if (num < _GlowControlDistanceRange.Min)
			{
				main.startSize = _GlowSizeRange.Max;
				return;
			}
			if (num > _GlowControlDistanceRange.Max)
			{
				main.startSize = _GlowSizeRange.Min;
				return;
			}
			num -= _GlowControlDistanceRange.Min;
			float num2 = num / (_GlowControlDistanceRange.Max - _GlowControlDistanceRange.Min);
			num2 = 1f - num2;
			main.startSize = num2 * (_GlowSizeRange.Max - _GlowSizeRange.Min) + _GlowSizeRange.Min;
		}
	}

	public void OnSetMessageOnly(bool flag)
	{
		mSendMessageOnly = flag;
	}

	public void OnEnableMagnet(bool flag)
	{
		mMagnetEnabled = flag;
		CreateMagnetCollider(flag);
	}

	public void CreateMagnetCollider(bool create)
	{
		if (create && mMagnetCollider == null)
		{
			mMagnetCollider = base.gameObject.AddComponent<SphereCollider>();
			mMagnetCollider.radius = _OverlapRadius;
			mMagnetCollider.isTrigger = true;
		}
		else if (mMagnetCollider != null)
		{
			Object.Destroy(mMagnetCollider);
		}
	}
}
