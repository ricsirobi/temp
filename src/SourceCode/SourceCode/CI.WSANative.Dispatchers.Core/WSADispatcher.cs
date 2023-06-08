using System;
using System.Collections.Generic;
using UnityEngine;

namespace CI.WSANative.Dispatchers.Core;

public class WSADispatcher : MonoBehaviour
{
	private readonly Queue<Action> _queue = new Queue<Action>();

	private readonly object _lock = new object();

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Update()
	{
		lock (_lock)
		{
			while (_queue.Count > 0)
			{
				_queue.Dequeue()();
			}
		}
	}

	public void Enqueue(Action action)
	{
		lock (_lock)
		{
			_queue.Enqueue(action);
		}
	}
}
