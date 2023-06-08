using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Core.PathCore;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.Events;

namespace SWS;

[AddComponentMenu("Simple Waypoint System/splineMove")]
public class splineMove : MonoBehaviour
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
		random,
		yoyo
	}

	public enum RotationType
	{
		none,
		all
	}

	public PathManager pathContainer;

	public bool onStart;

	public bool moveToPath;

	public bool reverse;

	public int startPoint;

	[HideInInspector]
	public int currentPoint;

	public bool closeLoop;

	public bool local;

	public float lookAhead;

	public float sizeToAdd;

	public bool _NegateSwitch;

	public TimeValue timeValue = TimeValue.speed;

	public float speed = 5f;

	public AnimationCurve animEaseType;

	public LoopType loopType;

	[HideInInspector]
	public Vector3[] waypoints;

	[HideInInspector]
	public List<UnityEvent> events = new List<UnityEvent>();

	public PathType pathType = PathType.CatmullRom;

	public PathMode pathMode = PathMode.Full3D;

	public Ease easeType = Ease.Linear;

	public AxisConstraint lockPosition;

	public AxisConstraint lockRotation;

	public RotationType waypointRotation;

	public Transform rotationTarget;

	[HideInInspector]
	public Tweener tween;

	private Vector3[] wpPos;

	private float originSpeed;

	private Quaternion originRot;

	private System.Random rand;

	private int[] rndArray;

	protected bool mIsMoving;

	protected bool mInitialized;

	private void Start()
	{
		rand = new System.Random();
		if (onStart)
		{
			StartMove();
		}
	}

	public virtual void StartMove()
	{
		if (pathContainer == null)
		{
			Debug.LogWarning(base.gameObject.name + " has no path! Please set Path Container.");
			return;
		}
		mInitialized = true;
		waypoints = pathContainer.GetPathPoints(local);
		originSpeed = speed;
		originRot = base.transform.rotation;
		startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
		int num = startPoint;
		if (reverse)
		{
			Array.Reverse(waypoints);
			num = waypoints.Length - 1 - num;
		}
		Initialize(num);
		Stop();
		mIsMoving = true;
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
		for (int j = events.Count; j <= pathContainer.GetWaypointCount() - 1; j++)
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
		if (easeType == Ease.Unset)
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
			if (loopType == LoopType.random)
			{
				RandomizeWaypoints();
			}
			else if (loopType == LoopType.yoyo)
			{
				tweenParams.OnStepComplete(ReachedEnd);
			}
			Vector3 position = wpPos[0];
			if (local)
			{
				position = pathContainer.transform.TransformPoint(position);
			}
			base.transform.position = position;
			tweenParams.OnWaypointChange(OnWaypointChange);
			tweenParams.OnComplete(ReachedEnd);
		}
		if (pathMode == PathMode.Ignore && waypointRotation != 0)
		{
			if (rotationTarget == null)
			{
				rotationTarget = base.transform;
			}
			tweenParams.OnUpdate(OnWaypointRotation);
		}
		if (local)
		{
			tween = base.transform.DOLocalPath(wpPos, originSpeed, pathType, pathMode).SetAs(tweenParams).SetOptions(closeLoop, lockPosition, lockRotation)
				.SetLookAt(lookAhead);
		}
		else
		{
			tween = base.transform.DOPath(wpPos, originSpeed, pathType, pathMode).SetAs(tweenParams).SetOptions(closeLoop, lockPosition, lockRotation)
				.SetLookAt(lookAhead);
		}
		tween.ForceInit();
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
		index = pathContainer.GetWaypointIndex(index);
		if (index != -1)
		{
			if (loopType != LoopType.yoyo && reverse)
			{
				index = waypoints.Length - 1 - index;
			}
			if (loopType == LoopType.random)
			{
				index = rndArray[index];
			}
			currentPoint = index;
			if (events != null && events.Count - 1 >= index && events[index] != null && (loopType != LoopType.random || index != rndArray[rndArray.Length - 1]))
			{
				events[index].Invoke();
			}
		}
	}

	private void OnWaypointRotation()
	{
		int num = currentPoint;
		num = Mathf.Clamp(pathContainer.GetWaypointIndex(currentPoint), 0, pathContainer.GetWaypointCount());
		if (!tween.IsInitialized() || tween.IsComplete())
		{
			ApplyWaypointRotation(pathContainer.GetWaypoint(num).rotation);
			return;
		}
		TweenerCore<Vector3, Path, PathOptions> tweenerCore = tween as TweenerCore<Vector3, Path, PathOptions>;
		float num2 = tweenerCore.PathLength() * tweenerCore.ElapsedPercentage();
		float num3 = 0f;
		float num4 = 0f;
		int num5 = currentPoint;
		if (moveToPath)
		{
			num3 = tweenerCore.changeValue.wpLengths[1];
			num4 = num2 / num3;
			ApplyWaypointRotation(Quaternion.Lerp(originRot, pathContainer.GetWaypoint(currentPoint).rotation, num4));
			return;
		}
		if (pathContainer is BezierPathManager)
		{
			BezierPathManager bezierPathManager = pathContainer as BezierPathManager;
			int num6 = currentPoint;
			if (reverse)
			{
				num5 = bezierPathManager.GetWaypointCount() - 2 - (waypoints.Length - currentPoint - 1);
				num6 = bezierPathManager.bPoints.Count - 2 - num5;
			}
			int num7 = (int)((float)num6 * bezierPathManager.pathDetail * 10f);
			if (bezierPathManager.customDetail)
			{
				num7 = 0;
				for (int i = 0; i < num5; i++)
				{
					num7 += (int)(bezierPathManager.segmentDetail[i] * 10f);
				}
			}
			if (reverse)
			{
				for (int j = 0; j <= num6 * 10; j++)
				{
					num2 -= tweenerCore.changeValue.wpLengths[j];
				}
			}
			else
			{
				for (int k = 0; k <= num7; k++)
				{
					num2 -= tweenerCore.changeValue.wpLengths[k];
				}
			}
			if (bezierPathManager.customDetail)
			{
				for (int l = num7 + 1; (float)l <= (float)num7 + bezierPathManager.segmentDetail[currentPoint] * 10f; l++)
				{
					num3 += tweenerCore.changeValue.wpLengths[l];
				}
			}
			else
			{
				for (int m = num7 + 1; m <= num7 + 10; m++)
				{
					num3 += tweenerCore.changeValue.wpLengths[m];
				}
			}
		}
		else
		{
			if (reverse)
			{
				num5 = waypoints.Length - currentPoint - 1;
			}
			for (int n = 0; n <= num5; n++)
			{
				num2 -= tweenerCore.changeValue.wpLengths[n];
			}
			num3 = tweenerCore.changeValue.wpLengths[num5 + 1];
		}
		num4 = num2 / num3;
		if (pathContainer is BezierPathManager)
		{
			num = num5;
			if (reverse)
			{
				num++;
			}
		}
		num4 = Mathf.Clamp01(num4);
		ApplyWaypointRotation(Quaternion.Lerp(pathContainer.GetWaypoint(num).rotation, pathContainer.GetWaypoint(reverse ? (num - 1) : (num + 1)).rotation, num4));
	}

	private void ApplyWaypointRotation(Quaternion rotation)
	{
		rotationTarget.rotation = rotation;
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
		case LoopType.random:
			RandomizeWaypoints();
			CreateTween();
			break;
		}
	}

	private void RandomizeWaypoints()
	{
		Initialize();
		rndArray = new int[wpPos.Length];
		for (int i = 0; i < rndArray.Length; i++)
		{
			rndArray[i] = i;
		}
		int num = wpPos.Length;
		while (num > 1)
		{
			int num2 = rand.Next(num--);
			Vector3 vector = wpPos[num];
			wpPos[num] = wpPos[num2];
			wpPos[num2] = vector;
			int num3 = rndArray[num];
			rndArray[num] = rndArray[num2];
			rndArray[num2] = num3;
		}
		Vector3 vector2 = wpPos[0];
		int num4 = rndArray[0];
		for (int j = 0; j < wpPos.Length; j++)
		{
			if (rndArray[j] == currentPoint)
			{
				rndArray[j] = num4;
				wpPos[0] = wpPos[j];
				wpPos[j] = vector2;
			}
		}
		rndArray[0] = currentPoint;
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

	public void OnStateChange(bool switchOn)
	{
		bool flag = switchOn;
		if (_NegateSwitch)
		{
			mIsMoving = !mIsMoving;
			flag = mIsMoving;
		}
		if (flag)
		{
			if (!mInitialized)
			{
				StartMove();
			}
			else
			{
				Resume();
			}
		}
		else if (mInitialized)
		{
			Pause();
		}
	}

	public void Pause(float seconds = 0f)
	{
		mIsMoving = false;
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
		mIsMoving = true;
		StopCoroutine(Wait());
		if (tween != null)
		{
			tween.Play();
		}
	}

	public void Reverse()
	{
		reverse = !reverse;
		float fullPosition = 0f;
		if (tween != null)
		{
			fullPosition = tween.Duration(includeLoops: false) - tween.Elapsed(includeLoops: false);
		}
		startPoint = waypoints.Length - 1 - currentPoint;
		StartMove();
		tween.ForceInit();
		tween.fullPosition = fullPosition;
	}

	public void SetPath(PathManager newPath)
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
		mIsMoving = false;
	}

	public void ResetToStart()
	{
		Stop();
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
