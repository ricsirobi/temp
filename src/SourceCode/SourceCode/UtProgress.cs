using System;
using System.Collections.Generic;
using UnityEngine;

public class UtProgress
{
	public class Status
	{
		private float mProgress;

		public float pProgress
		{
			get
			{
				return mProgress;
			}
			set
			{
				mProgress = value;
			}
		}

		public static implicit operator float(Status inStatus)
		{
			return inStatus.pProgress;
		}

		public static implicit operator bool(Status inStatus)
		{
			return inStatus.pProgress == 1f;
		}
	}

	private bool mComplete;

	private float mProgress;

	private Dictionary<string, float> mTasks = new Dictionary<string, float>();

	public bool pComplete => mComplete;

	public float pProgress => mProgress;

	public Dictionary<string, float> pTasks => mTasks;

	public void AddTask(string inName)
	{
		AddTask(inName, inForceRecalculate: true);
	}

	public void AddTask(string inName, bool inForceRecalculate)
	{
		if (mComplete)
		{
			Debug.LogError("ERROR: PROGRESS TASK ADD ATTEMPT AFTER COMPLETION!!");
			return;
		}
		try
		{
			mTasks.Add(inName, 0f);
			if (inForceRecalculate)
			{
				Recalculate();
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: PROGRESS TASK ADD: " + ex.ToString());
		}
	}

	public void UpdateTask(string inName, float inProgress)
	{
		UpdateTask(inName, inProgress, inForceRecalculate: true);
	}

	public void UpdateTask(string inName, float inProgress, bool inForceRecalculate)
	{
		if (!mComplete)
		{
			mTasks[inName] = inProgress;
			if (inForceRecalculate)
			{
				Recalculate();
			}
		}
	}

	public void ForceCompletion()
	{
		mProgress = 1f;
		mComplete = true;
	}

	public void Recalculate()
	{
		int num = 0;
		float num2 = 1f / (float)mTasks.Count;
		mProgress = 0f;
		foreach (KeyValuePair<string, float> mTask in mTasks)
		{
			mProgress += mTask.Value * num2;
			if (mTask.Value == 1f)
			{
				num++;
			}
		}
		if (num == mTasks.Count)
		{
			mProgress = 1f;
			mComplete = true;
		}
	}
}
