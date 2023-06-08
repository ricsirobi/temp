using System;
using System.Collections.Generic;
using UnityEngine;

public class WaterRise : MonoBehaviour
{
	[Serializable]
	public class ParticleData
	{
		public Vector3 _PreviousPosition = Vector3.zero;

		public Transform _Transform;
	}

	public float _RiseSpeed = 0.1f;

	public float _MinimumDistance = 0.1f;

	public GameObject _SplashFx;

	public float _FxDuration = 1f;

	private Vector3 mStartPosition = Vector3.zero;

	private Transform mTransform;

	private Task mTask;

	private List<ParticleData> mParticleDataList = new List<ParticleData>();

	private void Start()
	{
		mTransform = base.transform;
		mStartPosition = mTransform.position;
	}

	public void Reset()
	{
		if (mTransform != null)
		{
			mTransform.position = mStartPosition;
		}
		mParticleDataList.Clear();
	}

	private void Update()
	{
		if (mTask != null && mTask._Active)
		{
			mTransform.position += mTransform.TransformDirection(Vector3.up) * _RiseSpeed * Time.deltaTime;
		}
		if (!(_SplashFx != null))
		{
			return;
		}
		foreach (ParticleData mParticleData in mParticleDataList)
		{
			if (Vector3.Distance(mParticleData._PreviousPosition, mParticleData._Transform.position) > _MinimumDistance)
			{
				PlayOneShotFx(_SplashFx, mParticleData._Transform.position);
				mParticleData._PreviousPosition = mParticleData._Transform.position;
			}
		}
	}

	private void PlayOneShotFx(GameObject fxPrefab, Vector3 position, Transform parentT = null)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(fxPrefab, position, Quaternion.identity);
		if (gameObject != null)
		{
			gameObject.transform.parent = parentT;
			gameObject.SetActive(value: true);
			UnityEngine.Object.Destroy(gameObject, _FxDuration);
		}
	}

	private void SetupForTask(Task task)
	{
		mTask = task;
		Reset();
	}

	private void OnTriggerEnter(Collider obj)
	{
		if (mParticleDataList.Find((ParticleData t) => t._Transform == obj.transform) == null && (obj.gameObject == AvAvatar.pObject || obj.GetComponent<ChCharacter>() != null))
		{
			ParticleData particleData = new ParticleData();
			particleData._Transform = obj.transform;
			mParticleDataList.Add(particleData);
		}
	}

	private void OnTriggerExit(Collider obj)
	{
		ParticleData particleData = mParticleDataList.Find((ParticleData t) => t._Transform == obj.transform);
		if (particleData != null)
		{
			mParticleDataList.Remove(particleData);
		}
	}
}
