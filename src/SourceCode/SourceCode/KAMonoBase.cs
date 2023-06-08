using UnityEngine;

public class KAMonoBase : MonoBehaviour
{
	protected Collider mCachedCollider;

	private Transform mCachedTransform;

	private GameObject mCachedGameObject;

	private Rigidbody mCachedRigidBody;

	private Rigidbody2D mCachedRigidBody2D;

	private Collider2D mCachedCollider2D;

	private Renderer mCachedRenderer;

	private Camera mCachedCamera;

	private Light mCachedLight;

	private AudioSource mCachedAudio;

	private Animation mCachedAnimation;

	private ParticleSystem mCachedParticleSystem;

	public new Transform transform
	{
		get
		{
			if (mCachedTransform == null)
			{
				mCachedTransform = base.transform;
			}
			return mCachedTransform;
		}
	}

	public new GameObject gameObject
	{
		get
		{
			if (mCachedGameObject == null)
			{
				mCachedGameObject = base.gameObject;
			}
			return mCachedGameObject;
		}
	}

	public Rigidbody rigidbody
	{
		get
		{
			if (mCachedRigidBody == null)
			{
				mCachedRigidBody = GetComponent<Rigidbody>();
			}
			return mCachedRigidBody;
		}
	}

	public Rigidbody2D rigidbody2D
	{
		get
		{
			if (mCachedRigidBody2D == null)
			{
				mCachedRigidBody2D = GetComponent<Rigidbody2D>();
			}
			return mCachedRigidBody2D;
		}
	}

	public virtual Collider collider
	{
		get
		{
			if (mCachedCollider == null)
			{
				mCachedCollider = GetComponent<Collider>();
			}
			return mCachedCollider;
		}
		set
		{
			mCachedCollider = value;
		}
	}

	public Collider2D collider2D
	{
		get
		{
			if (mCachedCollider2D == null)
			{
				mCachedCollider2D = GetComponent<Collider2D>();
			}
			return mCachedCollider2D;
		}
	}

	public Renderer renderer
	{
		get
		{
			if (mCachedRenderer == null)
			{
				mCachedRenderer = GetComponent<Renderer>();
			}
			return mCachedRenderer;
		}
	}

	public Camera camera
	{
		get
		{
			if (mCachedCamera == null)
			{
				mCachedCamera = GetComponent<Camera>();
			}
			return mCachedCamera;
		}
	}

	public Light light
	{
		get
		{
			if (mCachedLight == null)
			{
				mCachedLight = GetComponent<Light>();
			}
			return mCachedLight;
		}
	}

	public AudioSource audio
	{
		get
		{
			if (mCachedAudio == null)
			{
				mCachedAudio = GetComponent<AudioSource>();
			}
			return mCachedAudio;
		}
	}

	public Animation animation
	{
		get
		{
			if (mCachedAnimation == null)
			{
				mCachedAnimation = GetComponent<Animation>();
			}
			return mCachedAnimation;
		}
	}

	public ParticleSystem particleSystem
	{
		get
		{
			if (mCachedParticleSystem == null)
			{
				mCachedParticleSystem = GetComponent<ParticleSystem>();
			}
			return mCachedParticleSystem;
		}
	}
}
