using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBehavior_SearchZoneDetectPlayer : AIBehavior
{
	public enum DetectedState
	{
		None,
		SafeZone,
		SearchZone,
		TransitionToAlert,
		AlertZone,
		Reset
	}

	public AIBehavior_DrawSearchZone _SearchZoneVisual;

	public AIBehavior_SearchZonePatrol _SearchZonePatrol;

	public float _LookAtRate = 6f;

	private AIActor_NPC.SearchZoneData mSearchZoneData;

	private UIProgressBar mAlertProgress;

	private UIProgressBar mSearchProgress;

	private DetectedState mState;

	private DetectedState mPreviousState;

	private AIActor mActor;

	private Quaternion mPreviousRotation;

	private static bool mPlayerFrozen;

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		mActor = Actor;
		mSearchZoneData = ((AIActor_NPC)Actor)._SearchZoneData;
		if (mSearchZoneData != null)
		{
			if (mSearchZoneData._InnerZone._MeterObject != null)
			{
				mAlertProgress = mSearchZoneData._InnerZone._MeterObject.GetComponentInChildren<UIProgressBar>();
			}
			if (mSearchZoneData._OuterZone._MeterObject != null)
			{
				mSearchProgress = mSearchZoneData._OuterZone._MeterObject.GetComponentInChildren<UIProgressBar>();
			}
			SetState(DetectedState.SafeZone);
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mSearchZoneData != null)
		{
			switch (mState)
			{
			case DetectedState.SafeZone:
				if (DetectPlayer(mSearchZoneData._OuterZone, Actor.transform))
				{
					SetState(DetectedState.SearchZone);
				}
				else if (mSearchProgress != null && mSearchProgress.value > 0f)
				{
					mSearchProgress.Set(mSearchProgress.value - Time.deltaTime * mSearchZoneData._OuterZone._MeterDepletionRate);
					if (mSearchProgress.value <= 0f && mSearchZoneData._OuterZone._MeterObject != null)
					{
						SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Empty, mSearchZoneData._OuterZone._EventData);
						mSearchZoneData._OuterZone._MeterObject.SetActive(value: false);
					}
				}
				break;
			case DetectedState.SearchZone:
				if (DetectPlayer(mSearchZoneData._InnerZone, Actor.transform))
				{
					SetState(DetectedState.TransitionToAlert);
				}
				else if (!DetectPlayer(mSearchZoneData._OuterZone, Actor.transform))
				{
					SetState(DetectedState.SafeZone);
				}
				if (mSearchProgress != null && mSearchProgress.value < 1f)
				{
					mSearchProgress.Set(mSearchProgress.value + Time.deltaTime * mSearchZoneData._OuterZone._MeterFillRate);
					if (mSearchProgress.value >= 1f)
					{
						SetState(DetectedState.TransitionToAlert);
					}
				}
				break;
			case DetectedState.TransitionToAlert:
				if ((AvAvatar.pState == AvAvatarState.PAUSED || AvAvatar.pState == AvAvatarState.NONE || (AvAvatar.pToolbar != null && !AvAvatar.pToolbar.activeInHierarchy)) && !mPlayerFrozen)
				{
					LookAtAvatar();
				}
				else
				{
					SetState(DetectedState.AlertZone);
				}
				break;
			case DetectedState.AlertZone:
				if (!mPlayerFrozen)
				{
					SetState(DetectedState.Reset);
					break;
				}
				if (mAlertProgress != null && mAlertProgress.value < 1f)
				{
					mAlertProgress.Set(mAlertProgress.value + Time.deltaTime * mSearchZoneData._InnerZone._MeterFillRate);
					if (mAlertProgress.value >= 1f)
					{
						SetState(DetectedState.Reset);
					}
				}
				LookAtAvatar();
				break;
			case DetectedState.Reset:
				LookAtAvatar();
				break;
			}
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	private void LookAtAvatar()
	{
		Vector3 normalized = (AvAvatar.OriginalObject.transform.position - mActor.transform.position).normalized;
		normalized.y = 0f;
		Transform mainRoot = ((AIActor_NPC)mActor).MainRoot;
		if (mainRoot != null)
		{
			mainRoot.rotation = Quaternion.Lerp(mainRoot.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * _LookAtRate);
		}
	}

	private void SetState(DetectedState state)
	{
		if (mState == state || mSearchZoneData == null)
		{
			return;
		}
		switch (state)
		{
		case DetectedState.SafeZone:
			if (_SearchZonePatrol != null)
			{
				_SearchZonePatrol.Patrol(run: true);
			}
			if (mPreviousState != DetectedState.SearchZone || !(mSearchProgress != null))
			{
				break;
			}
			if (mSearchProgress.value <= 0f)
			{
				if (mSearchZoneData._OuterZone._MeterObject != null)
				{
					mSearchZoneData._OuterZone._MeterObject.SetActive(value: false);
				}
			}
			else
			{
				SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Drop, mSearchZoneData._OuterZone._EventData);
			}
			break;
		case DetectedState.SearchZone:
			if (mSearchZoneData._OuterZone._MeterObject != null)
			{
				mSearchZoneData._OuterZone._MeterObject.SetActive(value: true);
			}
			SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Fill, mSearchZoneData._OuterZone._EventData);
			break;
		case DetectedState.TransitionToAlert:
		{
			if (AvAvatar.pToolbar != null)
			{
				UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
				if (component != null && component._UiAvatarCSM != null && component._UiAvatarCSM.GetVisibility())
				{
					component._UiAvatarCSM.Close(resetAvatarState: true);
				}
			}
			Transform mainRoot = ((AIActor_NPC)mActor).MainRoot;
			if (mainRoot != null)
			{
				mPreviousRotation = mainRoot.transform.localRotation;
			}
			if (_SearchZonePatrol != null)
			{
				_SearchZonePatrol.Patrol(run: false);
			}
			SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Fill, mSearchZoneData._InnerZone._EventData);
			if (_SearchZoneVisual != null)
			{
				_SearchZoneVisual.SetAlert(alerted: true);
			}
			if (mSearchZoneData._InnerZone._MeterObject != null)
			{
				mSearchZoneData._InnerZone._MeterObject.SetActive(value: true);
			}
			if (mSearchZoneData._OuterZone._MeterObject != null)
			{
				mSearchZoneData._OuterZone._MeterObject.SetActive(value: false);
			}
			break;
		}
		case DetectedState.AlertZone:
			if (AvAvatar.pState == AvAvatarState.MOVING)
			{
				AvAvatar.OriginalObject.GetComponent<AvAvatarController>().pVelocity = Vector3.zero;
			}
			AvAvatar.EnableAllInputs(inActive: false);
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			mPlayerFrozen = true;
			break;
		case DetectedState.Reset:
			SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Full, mSearchZoneData._InnerZone._EventData);
			StartCoroutine(Reset());
			break;
		}
		mPreviousState = state;
		mState = state;
	}

	private IEnumerator Reset()
	{
		if (mPlayerFrozen)
		{
			yield return new WaitForSeconds(mSearchZoneData._AvatarRespawnDelay);
		}
		Transform mainRoot = ((AIActor_NPC)mActor).MainRoot;
		if (mainRoot != null)
		{
			mainRoot.transform.localRotation = mPreviousRotation;
		}
		if (mSearchZoneData._InnerZone._MeterObject != null)
		{
			mSearchZoneData._InnerZone._MeterObject.SetActive(value: false);
		}
		if (mSearchZoneData._OuterZone._MeterObject != null)
		{
			mSearchZoneData._OuterZone._MeterObject.SetActive(value: false);
		}
		if (mSearchProgress != null)
		{
			mSearchProgress.value = 0f;
		}
		if (mAlertProgress != null)
		{
			mAlertProgress.value = 0f;
		}
		if (_SearchZoneVisual != null)
		{
			_SearchZoneVisual.SetAlert(alerted: false);
		}
		if (mSearchZoneData._AvatarRespawnMarker != null && mPlayerFrozen)
		{
			Vector3 inDirection = Vector3.zero;
			if (mSearchZoneData._UseRotationOnRespawnAvatar)
			{
				inDirection = mSearchZoneData._AvatarRespawnMarker.transform.forward;
			}
			AvAvatar.TeleportTo(mSearchZoneData._AvatarRespawnMarker.position, inDirection, 0f, doTeleportFx: true, 0.5f);
		}
		SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType.Empty, mSearchZoneData._InnerZone._EventData);
		yield return new WaitForEndOfFrame();
		if (mPlayerFrozen)
		{
			AvAvatar.EnableAllInputs(inActive: true);
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			mPlayerFrozen = false;
		}
		SetState(DetectedState.SafeZone);
	}

	private void SetMeterEvent(AIActor_NPC.ZoneData.MeterEventType type, List<AIActor_NPC.ZoneData.EventData> eventData)
	{
		AIActor_NPC.ZoneData.EventData eventData2 = eventData.Find((AIActor_NPC.ZoneData.EventData e) => e._MeterEventType == type);
		if (eventData2 != null)
		{
			mActor.PlayEffect(eventData2._EffectData);
		}
	}

	private bool DetectPlayer(AIActor_NPC.ZoneData zoneData, Transform source)
	{
		if (AvAvatar.OriginalObject != null)
		{
			Vector3 position = AvAvatar.OriginalObject.transform.position;
			if (mSearchZoneData._HeightRange.Max != 0f || mSearchZoneData._HeightRange.Min != 0f)
			{
				MinMax minMax = new MinMax(mActor.transform.position.y + mSearchZoneData._HeightRange.Min, mActor.transform.position.y + mSearchZoneData._HeightRange.Max);
				if (position.y > minMax.Max || position.y < minMax.Min)
				{
					return false;
				}
			}
			Vector3 vector = source.rotation * zoneData._Offset + source.position;
			if (Vector2.Distance(new Vector2(vector.x, vector.z), new Vector2(position.x, position.z)) <= zoneData._Length)
			{
				Vector3 from = position - vector;
				from.y = 0f;
				if (Vector3.Angle(from, source.forward) <= zoneData._Angle / 2f)
				{
					return true;
				}
			}
		}
		return false;
	}
}
