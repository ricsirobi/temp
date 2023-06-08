using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace SWS;

[RequireComponent(typeof(NavMeshAgent))]
[AddComponentMenu("Simple Waypoint System/navMove")]
public class navMove : MonoBehaviour
{
	public enum LoopType
	{
		none,
		loop,
		pingPong,
		random
	}

	public PathManager pathContainer;

	public bool onStart;

	public bool moveToPath;

	public bool reverse;

	public int startPoint;

	[HideInInspector]
	public int currentPoint;

	public bool closeLoop;

	public bool updateRotation = true;

	[HideInInspector]
	public List<UnityEvent> events = new List<UnityEvent>();

	public LoopType loopType;

	[HideInInspector]
	public Transform[] waypoints;

	private bool repeat;

	private NavMeshAgent agent;

	private System.Random rand = new System.Random();

	private int rndIndex;

	private bool waiting;

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();
	}

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
		waypoints = new Transform[pathContainer.waypoints.Length];
		Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);
		startPoint = Mathf.Clamp(startPoint, 0, waypoints.Length - 1);
		int num = startPoint;
		if (reverse)
		{
			Array.Reverse(waypoints);
			num = waypoints.Length - 1 - num;
		}
		currentPoint = num;
		for (int i = events.Count; i <= waypoints.Length - 1; i++)
		{
			events.Add(new UnityEvent());
		}
		Stop();
		StartCoroutine(Move());
	}

	private IEnumerator Move()
	{
		agent.isStopped = false;
		agent.updateRotation = updateRotation;
		if (moveToPath)
		{
			agent.SetDestination(waypoints[currentPoint].position);
			yield return StartCoroutine(WaitForDestination());
		}
		if (loopType == LoopType.random)
		{
			StartCoroutine(ReachedEnd());
			yield break;
		}
		if (moveToPath)
		{
			StartCoroutine(NextWaypoint());
		}
		else
		{
			GoToWaypoint(startPoint);
		}
		moveToPath = false;
	}

	private IEnumerator NextWaypoint()
	{
		OnWaypointChange(currentPoint);
		yield return new WaitForEndOfFrame();
		while (waiting)
		{
			yield return null;
		}
		Transform transform = null;
		if (loopType == LoopType.pingPong && repeat)
		{
			currentPoint--;
		}
		else if (loopType == LoopType.random)
		{
			rndIndex++;
			currentPoint = int.Parse(waypoints[rndIndex].name.Replace("Waypoint ", ""));
			transform = waypoints[rndIndex];
		}
		else
		{
			currentPoint++;
		}
		currentPoint = Mathf.Clamp(currentPoint, 0, waypoints.Length - 1);
		if (transform == null)
		{
			transform = waypoints[currentPoint];
		}
		agent.SetDestination(transform.position);
		yield return StartCoroutine(WaitForDestination());
		if ((loopType != LoopType.random && currentPoint == waypoints.Length - 1) || rndIndex == waypoints.Length - 1 || (repeat && currentPoint == 0))
		{
			StartCoroutine(ReachedEnd());
		}
		else
		{
			StartCoroutine(NextWaypoint());
		}
	}

	private IEnumerator WaitForDestination()
	{
		yield return new WaitForEndOfFrame();
		while (agent.pathPending)
		{
			yield return null;
		}
		yield return new WaitForEndOfFrame();
		float remain = agent.remainingDistance;
		while (remain == float.PositiveInfinity || remain - agent.stoppingDistance > float.Epsilon || agent.pathStatus != 0)
		{
			remain = agent.remainingDistance;
			yield return null;
		}
	}

	private void OnWaypointChange(int index)
	{
		if (reverse)
		{
			index = waypoints.Length - 1 - index;
		}
		if (events != null && events.Count - 1 >= index && events[index] != null)
		{
			events[index].Invoke();
		}
	}

	private IEnumerator ReachedEnd()
	{
		switch (loopType)
		{
		case LoopType.none:
			OnWaypointChange(waypoints.Length - 1);
			yield break;
		case LoopType.loop:
			OnWaypointChange(waypoints.Length - 1);
			if (closeLoop)
			{
				agent.SetDestination(waypoints[0].position);
				yield return StartCoroutine(WaitForDestination());
			}
			else
			{
				agent.Warp(waypoints[0].position);
			}
			currentPoint = 0;
			break;
		case LoopType.pingPong:
			repeat = !repeat;
			break;
		case LoopType.random:
			RandomizeWaypoints();
			break;
		}
		StartCoroutine(NextWaypoint());
	}

	private void RandomizeWaypoints()
	{
		Array.Copy(pathContainer.waypoints, waypoints, pathContainer.waypoints.Length);
		int num = waypoints.Length;
		while (num > 1)
		{
			int num2 = rand.Next(num--);
			Transform transform = waypoints[num];
			waypoints[num] = waypoints[num2];
			waypoints[num2] = transform;
		}
		Transform transform2 = pathContainer.waypoints[currentPoint];
		for (int i = 0; i < waypoints.Length; i++)
		{
			if (waypoints[i] == transform2)
			{
				Transform transform3 = waypoints[0];
				waypoints[0] = waypoints[i];
				waypoints[i] = transform3;
				break;
			}
		}
		rndIndex = 0;
	}

	public void GoToWaypoint(int index)
	{
		if (reverse)
		{
			index = waypoints.Length - 1 - index;
		}
		Stop();
		currentPoint = index;
		agent.Warp(waypoints[index].position);
		StartCoroutine(NextWaypoint());
	}

	public void Pause(float seconds = 0f)
	{
		StopCoroutine(Wait());
		waiting = true;
		agent.isStopped = true;
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
		StopCoroutine(Wait());
		waiting = false;
		agent.isStopped = false;
	}

	public void Reverse()
	{
		reverse = !reverse;
		if (reverse)
		{
			startPoint = currentPoint - 1;
		}
		else
		{
			Array.Reverse(waypoints);
			startPoint = waypoints.Length - currentPoint;
		}
		moveToPath = true;
		StartMove();
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
		if (agent.enabled)
		{
			agent.isStopped = true;
		}
	}

	public void ResetToStart()
	{
		Stop();
		currentPoint = 0;
		if ((bool)pathContainer)
		{
			agent.Warp(pathContainer.waypoints[currentPoint].position);
		}
	}

	public void ChangeSpeed(float value)
	{
		agent.speed = value;
	}
}
