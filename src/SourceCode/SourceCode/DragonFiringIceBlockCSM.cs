using UnityEngine;

public class DragonFiringIceBlockCSM : DragonFiringCSM
{
	public GameObject _Water;

	public Vector3 _WaterMaxSize = Vector3.one;

	public float _IceBlockScaleDuration = 5f;

	public float _WaterScaleUpDuration = 5f;

	public GameObject[] _WaterShootObjects;

	public float _WaterShootStartTime = 0.5f;

	public float _WaterShootDuration = 5f;

	private ObScale mWaterScale;

	private float mTimer;

	private bool mCheckTime;

	protected override void ProcessOnHit()
	{
		base.ProcessOnHit();
		mCheckTime = true;
		mTimer = _WaterScaleUpDuration + _WaterShootStartTime;
		ObScale.Set(base.gameObject, 0f, _IceBlockScaleDuration, inStartScale: true, inRequireOnScaleUpdate: false);
		if (_Water != null)
		{
			_Water.transform.localScale = Vector3.zero;
			_Water.SetActive(value: true);
			mWaterScale = ObScale.Set(_Water, _WaterMaxSize, _WaterScaleUpDuration, inStartScale: true, inRequireOnScaleUpdate: false);
		}
	}

	protected override void Update()
	{
		base.Update();
		GameObject[] waterShootObjects;
		if (_WaterShootObjects != null && _WaterShootObjects.Length != 0 && _WaterShootObjects[0] != null && _WaterShootObjects[0].activeSelf)
		{
			ParticleSystem component = _WaterShootObjects[0].GetComponent<ParticleSystem>();
			if (component != null && component.isPlaying)
			{
				mTimer -= Time.deltaTime;
				if (mTimer <= 0f)
				{
					if (_Water != null)
					{
						_Water.SetActive(value: false);
					}
					waterShootObjects = _WaterShootObjects;
					foreach (GameObject gameObject in waterShootObjects)
					{
						if (gameObject != null)
						{
							gameObject.GetComponent<ParticleSystem>().Stop();
						}
					}
				}
			}
		}
		if (!mCheckTime)
		{
			return;
		}
		mTimer -= Time.deltaTime;
		if (!(mTimer <= 0f))
		{
			return;
		}
		mCheckTime = false;
		if (_WaterShootObjects == null || _WaterShootObjects.Length == 0)
		{
			return;
		}
		if (mWaterScale != null)
		{
			mWaterScale.Reset();
			mWaterScale.pOriginalScale = mWaterScale.transform.localScale;
		}
		Vector3 waterMaxSize = _WaterMaxSize;
		waterMaxSize.y = 0f;
		ObScale.Set(_Water, waterMaxSize, _WaterShootDuration, inStartScale: true, inRequireOnScaleUpdate: false);
		waterShootObjects = _WaterShootObjects;
		foreach (GameObject gameObject2 in waterShootObjects)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(value: true);
			}
			mTimer = _WaterShootDuration;
		}
	}
}
