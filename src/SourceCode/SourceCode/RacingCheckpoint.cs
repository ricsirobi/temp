using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingCheckpoint : MonoBehaviour, IComparable<RacingCheckpoint>
{
	public int _CheckpointID;

	public List<RacingPositionSensor> _ValidRacingSensors = new List<RacingPositionSensor>();

	public GameObject _ActiveCheckpointEffect;

	public GameObject _CollectedCheckpointEffect;

	public float _CheckpointEffectTimer = 1.5f;

	public AudioClip _SoundEffect;

	public GameObject _MiniMapDisplay;

	public GameObject _CheckpointResetMarker;

	private bool mIsRunningEffect;

	public static event Action<RacingCheckpoint> OnHit;

	protected virtual void OnTriggerEnter(Collider coll)
	{
		if (coll.gameObject.CompareTag("Player") && RacingCheckpoint.OnHit != null)
		{
			RacingCheckpoint.OnHit(this);
		}
	}

	public static void Sort(RacingCheckpoint[] checkpoints)
	{
		Array.Sort(checkpoints);
	}

	public int CompareTo(RacingCheckpoint other)
	{
		return _CheckpointID.CompareTo(other._CheckpointID);
	}

	public void CollectCheckpoint()
	{
		StartCoroutine(RunCollectCheckpoint());
	}

	private IEnumerator RunCollectCheckpoint()
	{
		if (_ActiveCheckpointEffect != null)
		{
			_ActiveCheckpointEffect.SetActive(value: false);
		}
		if (_CollectedCheckpointEffect != null)
		{
			_CollectedCheckpointEffect.SetActive(value: true);
			mIsRunningEffect = true;
		}
		if ((bool)_SoundEffect)
		{
			SnChannel snChannel = SnChannel.Play(_SoundEffect);
			if (snChannel != null)
			{
				snChannel.pLoop = false;
			}
		}
		float timer = 0f;
		while (timer < _CheckpointEffectTimer)
		{
			timer += Time.deltaTime;
			yield return null;
		}
		if (_CollectedCheckpointEffect != null)
		{
			_CollectedCheckpointEffect.SetActive(value: false);
		}
		mIsRunningEffect = false;
		base.gameObject.SetActive(value: false);
	}

	public void SetAsActiveCheckpoint(bool active)
	{
		if (active && _ActiveCheckpointEffect != null)
		{
			_ActiveCheckpointEffect.SetActive(value: true);
		}
		if (_MiniMapDisplay != null)
		{
			_MiniMapDisplay.SetActive(active);
		}
		if (!mIsRunningEffect)
		{
			base.gameObject.SetActive(active);
		}
	}
}
