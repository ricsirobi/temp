using UnityEngine;

public class CTMetalCage : PhysicsObject
{
	public GameObject _MetalCage;

	public GameObject _StuffedAnimal;

	public ParticleSystem _Particle;

	private bool mActivated;

	private ParticleSystem.EmissionModule mEmissionModule;

	private bool mIsBoxOpened;

	public bool pIsBoxOpened => mIsBoxOpened;

	private void Start()
	{
		UtDebug.Assert(_MetalCage != null, " _MetalCage  gameObject is null");
		UtDebug.Assert(_StuffedAnimal != null, " _StuffedAnimal gameObject is null");
		UtDebug.Assert(_Particle != null, "No particle for metal cage");
		base.rigidbody2D.isKinematic = true;
		_StuffedAnimal.GetComponent<Rigidbody2D>().isKinematic = true;
		if ((bool)_Particle)
		{
			mEmissionModule = _Particle.emission;
			mEmissionModule.enabled = false;
			_Particle.Stop();
		}
	}

	public override void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.name.Contains("Key") && mActivated)
		{
			OpenBox();
			other.gameObject.SetActive(value: false);
		}
	}

	public override void Enable()
	{
		mActivated = true;
		base.rigidbody2D.isKinematic = false;
		base.rigidbody2D.gravityScale = gravity;
		base.rigidbody2D.freezeRotation = false;
		_StuffedAnimal.GetComponent<Rigidbody2D>().isKinematic = false;
		_StuffedAnimal.GetComponent<Rigidbody2D>().gravityScale = gravity;
	}

	public override void Reset()
	{
		CloseBox();
		base.gameObject.SetActive(value: true);
		mActivated = false;
		base.rigidbody2D.gravityScale = 0f;
		base.rigidbody2D.velocity = new Vector2(0f, 0f);
		base.rigidbody2D.freezeRotation = true;
		base.rigidbody2D.isKinematic = true;
		_StuffedAnimal.transform.position = base.transform.position;
		_StuffedAnimal.transform.eulerAngles = base.transform.eulerAngles;
		_StuffedAnimal.GetComponent<Rigidbody2D>().gravityScale = 0f;
		_StuffedAnimal.GetComponent<Rigidbody2D>().isKinematic = true;
		_StuffedAnimal.GetComponent<Rigidbody2D>().freezeRotation = true;
	}

	private void OpenBox()
	{
		if (!mIsBoxOpened)
		{
			mIsBoxOpened = true;
			BoxCollider2D[] components = base.gameObject.GetComponents<BoxCollider2D>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].enabled = false;
			}
			base.gameObject.GetComponent<Rigidbody2D>().gravityScale = 0f;
			base.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
			_MetalCage.SetActive(value: false);
			_StuffedAnimal.GetComponent<Rigidbody2D>().gravityScale = gravity;
			_StuffedAnimal.GetComponent<Rigidbody2D>().isKinematic = false;
			_StuffedAnimal.GetComponent<Rigidbody2D>().freezeRotation = false;
			PlaySound(inForce: true);
			if ((bool)_Particle)
			{
				mEmissionModule.enabled = true;
				_Particle.Play();
			}
		}
	}

	private void CloseBox()
	{
		mIsBoxOpened = false;
		BoxCollider2D[] components = base.gameObject.GetComponents<BoxCollider2D>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].enabled = true;
		}
		_MetalCage.SetActive(value: true);
		if ((bool)_Particle)
		{
			mEmissionModule.enabled = false;
			_Particle.Stop();
		}
	}
}
