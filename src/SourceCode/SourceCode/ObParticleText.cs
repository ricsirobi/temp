using UnityEngine;

public class ObParticleText : KAMonoBase
{
	public GameObject _ParticleTextObj;

	public Vector3 _ParticleTextOffset = Vector3.zero;

	private GameObject mParticleTextRef;

	private string mPointsText;

	public string pPointsText
	{
		set
		{
			mPointsText = value;
		}
	}

	private void Awake()
	{
		Vector3 position = base.particleSystem.transform.position + _ParticleTextOffset;
		mParticleTextRef = Object.Instantiate(_ParticleTextObj, position, Quaternion.identity);
		mParticleTextRef.transform.parent = base.particleSystem.transform;
	}

	private void Start()
	{
		SetTextToParticle();
	}

	private void Update()
	{
		if (mParticleTextRef != null)
		{
			ParticleSystem.Particle[] array = new ParticleSystem.Particle[base.particleSystem.particleCount];
			for (int i = 0; i < array.Length; i++)
			{
				mParticleTextRef.transform.position = array[i].position + base.transform.TransformPoint(_ParticleTextOffset);
			}
		}
	}

	private void SetTextToParticle()
	{
		mParticleTextRef.GetComponent<TextMesh>().text = mPointsText;
	}
}
