using System.Collections;
using UnityEngine;

public class AIBeacon : MonoBehaviour
{
	public static ArrayList Beacons = new ArrayList();

	public AIEvaluator _EvaluatorRoot;

	public float _Radius = 10f;

	public bool _OnlyMainAvatar;

	private void Start()
	{
		if (_EvaluatorRoot == null)
		{
			_EvaluatorRoot = GetComponent<AIEvaluator>();
			if (_EvaluatorRoot == null)
			{
				_EvaluatorRoot = GetComponentInChildren<AIEvaluator>();
			}
		}
		if (_EvaluatorRoot != null)
		{
			_EvaluatorRoot.CheckForChildEvaluators();
			Beacons.Add(this);
		}
	}

	private void OnDestroy()
	{
		RemoveBeacon();
	}

	public void RemoveBeacon()
	{
		if (!(_EvaluatorRoot == null) && Beacons.Contains(this))
		{
			Beacons.Remove(this);
		}
	}

	public virtual AIEvaluator GetEvaluator()
	{
		return _EvaluatorRoot;
	}

	public virtual bool CanBePerceivedBy(AIActor Actor, AIBehavior_Arbiter_BeaconScanner Scanner)
	{
		float num = Vector3.Distance(Actor.Position, base.transform.position);
		if (_Radius > 0f && num > _Radius)
		{
			return false;
		}
		if (_OnlyMainAvatar && Actor.GetAvatar() != AvAvatar.mTransform)
		{
			return false;
		}
		return true;
	}
}
