using System;
using System.Collections.Generic;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
	public int _SimulationCount;

	public float _HeightOffset = 5f;

	public GameObject _AvatarSimulatiorPrefab;

	public MMOAvatarSimulator.MinMax _SplineCountMinMax;

	public MMOAvatarSimulator.MinMax _SplineOffsetMinMax;

	public MMOAvatarSimulator.MinMax _NodeCountMinMax;

	public MMOAvatarSimulator.MinMax _NodeOffsetMinMax;

	[NonSerialized]
	public bool pIsReady;

	[NonSerialized]
	public AvAvatarGenerator mRandomAvatarGenerator;

	private List<MMOAvatarSimulator> mSimulatedAvatars = new List<MMOAvatarSimulator>();

	private Dictionary<int, List<Spline>> mSplineDictionary = new Dictionary<int, List<Spline>>();

	private int mCurZone;

	private AutoGenerateSpline mGenerateSpline;

	public void Awake()
	{
		mRandomAvatarGenerator = GetComponent<AvAvatarGenerator>();
	}

	public void Simulate(int simulateCount)
	{
		CreateSplines(simulateCount);
		for (int i = 0; i < simulateCount; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_AvatarSimulatiorPrefab);
			if ((bool)gameObject)
			{
				MMOAvatarSimulator component = gameObject.GetComponent<MMOAvatarSimulator>();
				if ((bool)component)
				{
					component.Init(this, mCurZone);
					component.name = "Avatar Simulated " + (i + 1);
					mSimulatedAvatars.Add(component);
				}
				gameObject.transform.parent = base.transform;
			}
		}
		mCurZone++;
	}

	public void ClearSimulation()
	{
		mCurZone = 0;
		foreach (MMOAvatarSimulator mSimulatedAvatar in mSimulatedAvatars)
		{
			mSimulatedAvatar.DestroyAvatar();
		}
		mSimulatedAvatars.Clear();
		mSplineDictionary.Clear();
	}

	public int GetTotalCount()
	{
		return mSimulatedAvatars.Count;
	}

	private void CreateSplines(int simulateCount)
	{
		mGenerateSpline = new AutoGenerateSpline();
		MMOAvatarSimulator.MinMax splineCountMinMax = _SplineCountMinMax;
		if (simulateCount <= 15)
		{
			splineCountMinMax._Min *= 0.5f;
			splineCountMinMax._Max *= 0.5f;
		}
		int num = (int)UnityEngine.Random.Range(splineCountMinMax._Min, splineCountMinMax._Max);
		if (num < 1)
		{
			num = 1;
		}
		if (!mSplineDictionary.ContainsKey(mCurZone))
		{
			mSplineDictionary[mCurZone] = new List<Spline>();
		}
		for (int i = 0; i < num; i++)
		{
			Vector3 randomPos = GetRandomPos(AvAvatar.position, UnityEngine.Random.Range(_SplineOffsetMinMax._Min, _SplineOffsetMinMax._Max));
			Spline item = mGenerateSpline.GenerateSpline(randomPos, (int)UnityEngine.Random.Range(_NodeCountMinMax._Min, _NodeCountMinMax._Max), _NodeOffsetMinMax, "Spline " + (i + 1), _HeightOffset);
			mSplineDictionary[mCurZone].Add(item);
		}
	}

	public Spline GetRandomSpline(int zone)
	{
		if (mSplineDictionary.Count == 0 || !mSplineDictionary.ContainsKey(zone))
		{
			return null;
		}
		List<Spline> list = mSplineDictionary[zone];
		int index = UnityEngine.Random.Range(0, list.Count);
		return list[index];
	}

	private Vector3 GetRandomPos(Vector3 inPosition, float randomOffset)
	{
		if (randomOffset > 0f)
		{
			float num = UnityEngine.Random.Range(0, 24) * 15;
			float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * randomOffset;
			float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * randomOffset;
			inPosition.x += num2;
			inPosition.z += num3;
		}
		return inPosition;
	}

	private void Update()
	{
		if (!pIsReady && (bool)MainStreetMMOClient.pInstance && (bool)mRandomAvatarGenerator && mRandomAvatarGenerator.pIsReady)
		{
			pIsReady = true;
		}
	}
}
