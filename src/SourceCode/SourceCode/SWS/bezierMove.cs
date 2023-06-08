using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace SWS;

[AddComponentMenu("Simple Waypoint System/bezierMove")]
public class bezierMove : MonoBehaviour
{
	public enum TimeValue
	{
		time,
		speed
	}

	public enum LoopType
	{
		none,
		loop,
		pingPong,
		yoyo
	}

	public BezierPathManager pathContainer;

	public bool onStart;

	public bool moveToPath;

	public bool reverse;

	public int startPoint;

	[HideInInspector]
	public int currentPoint;

	public float lookAhead;

	public float sizeToAdd;

	public TimeValue timeValue = TimeValue.speed;

	public float speed = 5f;

	private float originSpeed;

	public AnimationCurve animEaseType;

	public LoopType loopType;

	[HideInInspector]
	public Vector3[] waypoints;

	private Vector3[] wpPos;

	[HideInInspector]
	public List<UnityEvent> events = new List<UnityEvent>();

	public PathType pathType = PathType.CatmullRom;

	public PathMode pathMode = PathMode.Full3D;

	public Ease easeType = Ease.Linear;

	public AxisConstraint lockPosition;

	public AxisConstraint lockRotation;

	[HideInInspector]
	public Tweener tween;

	private void Start()
	{
		if (onStart)
		{
			StartMove();
		}
	}

	public void StartMove()
	{
		if (pathContainer == null)
		{
			Debug.LogWarning(base.gameObject.name + " has no path! Please set Path Container.");
			return;
		}
		waypoints = pathContainer.GetPathPoints();
		originSpeed = speed;
		startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
		int num = startPoint;
		if (reverse)
		{
			Array.Reverse(waypoints);
			num = waypoints.Length - 1 - num;
		}
		Initialize(num);
		Stop();
		CreateTween();
	}

	private void Initialize(int startAt = 0)
	{
		if (!moveToPath)
		{
			startAt = 0;
		}
		wpPos = new Vector3[waypoints.Length - startAt];
		for (int i = 0; i < wpPos.Length; i++)
		{
			wpPos[i] = waypoints[i + startAt] + new Vector3(0f, sizeToAdd, 0f);
		}
		for (int j = events.Count; j <= pathContainer.bPoints.Count - 1; j++)
		{
			events.Add(new UnityEvent());
		}
	}

	private void CreateTween()
	{
		TweenParams tweenParams = new TweenParams();
		if (timeValue == TimeValue.speed)
		{
			tweenParams.SetSpeedBased();
		}
		if (loopType == LoopType.yoyo)
		{
			tweenParams.SetLoops(-1, DG.Tweening.LoopType.Yoyo);
		}
		if (easeType == Ease.INTERNAL_Custom)
		{
			tweenParams.SetEase(animEaseType);
		}
		else
		{
			tweenParams.SetEase(easeType);
		}
		if (moveToPath)
		{
			tweenParams.OnWaypointChange(OnWaypointReached);
		}
		else
		{
			if (loopType == LoopType.yoyo)
			{
				tweenParams.OnStepComplete(ReachedEnd);
			}
			base.transform.position = wpPos[0];
			tweenParams.OnWaypointChange(OnWaypointChange);
			tweenParams.OnComplete(ReachedEnd);
		}
		tween = base.transform.DOPath(wpPos, originSpeed, pathType, pathMode, 1).SetAs(tweenParams).SetOptions(closePath: false, lockPosition, lockRotation)
			.SetLookAt(lookAhead);
		if (!moveToPath && startPoint > 0)
		{
			GoToWaypoint(startPoint);
			startPoint = 0;
		}
		if (originSpeed != speed)
		{
			ChangeSpeed(speed);
		}
	}

	private void OnWaypointReached(int index)
	{
		if (index > 0)
		{
			Stop();
			moveToPath = false;
			Initialize();
			CreateTween();
		}
	}

	private void OnWaypointChange(int index)
	{
		bool flag = false;
		for (int i = 0; i < pathContainer.bPoints.Count; i++)
		{
			if (pathContainer.bPoints[i].wp.position == waypoints[index])
			{
				flag = true;
				index = i;
				break;
			}
		}
		if (flag)
		{
			currentPoint = index;
			if (events != null && events.Count - 1 >= index && events[index] != null)
			{
				events[index].Invoke();
			}
		}
	}

	private void ReachedEnd()
	{
		switch (loopType)
		{
		case LoopType.none:
			break;
		case LoopType.loop:
			currentPoint = 0;
			CreateTween();
			break;
		case LoopType.pingPong:
			reverse = !reverse;
			Array.Reverse(waypoints);
			Initialize();
			CreateTween();
			break;
		case LoopType.yoyo:
			reverse = !reverse;
			break;
		}
	}

	public void GoToWaypoint(int index)
	{
		if (tween != null)
		{
			if (reverse)
			{
				index = waypoints.Length - 1 - index;
			}
			tween.ForceInit();
			tween.GotoWaypoint(index, andPlay: true);
		}
	}

	public void Pause(float seconds = 0f)
	{
		StopCoroutine(Wait());
		if (tween != null)
		{
			tween.Pause();
		}
		if (seconds > 0f)
		{
			StartCoroutine(Wait(seconds));
		}
	}

	private IEnumerator Wait(float secs = 0f)
	{
		yield return new WaitForSeconds(secs);
		Resume();
	}

	public void Resume()
	{
		if (tween != null)
		{
			tween.Play();
		}
	}

	public void SetPath(BezierPathManager newPath)
	{
		Stop();
		pathContainer = newPath;
		StartMove();
	}

	public void Stop()
	{
		StopAllCoroutines();
		if (tween != null)
		{
			tween.Kill();
		}
		tween = null;
	}

	public void ResetToStart()
	{
		currentPoint = 0;
		if ((bool)pathContainer)
		{
			base.transform.position = pathContainer.waypoints[currentPoint].position + new Vector3(0f, sizeToAdd, 0f);
		}
	}

	public void ChangeSpeed(float value)
	{
		float timeScale = ((timeValue != TimeValue.speed) ? (originSpeed / value) : (value / originSpeed));
		speed = value;
		if (tween != null)
		{
			tween.timeScale = timeScale;
		}
	}
}
