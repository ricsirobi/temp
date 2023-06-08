using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
	public enum ObjectProperty
	{
		NONE,
		SOURCE,
		GOAL
	}

	public bool onTrigger;

	private static GoalManager mInstance;

	private List<GameObject> mSourceList = new List<GameObject>();

	private List<GameObject> mGoalList = new List<GameObject>();

	private List<GameObject> mCurrentGoalList = new List<GameObject>();

	private int mGoalsToAchieve;

	private int mCurrentGoalsAchieved;

	public static GoalManager pInstance => mInstance;

	private void Start()
	{
		mInstance = this;
	}

	public void ResetGoal()
	{
		mCurrentGoalsAchieved = 0;
		mCurrentGoalList.Clear();
		mCurrentGoalList.AddRange(mGoalList);
	}

	public void ClearGoals()
	{
		mSourceList.Clear();
		mGoalList.Clear();
		mCurrentGoalList.Clear();
		mGoalsToAchieve = 0;
		mCurrentGoalsAchieved = 0;
	}

	public void CollisionEvent(GameObject sender, GameObject collidedWith)
	{
		if (!onTrigger && mSourceList.Contains(sender) && mCurrentGoalList.Contains(collidedWith))
		{
			mCurrentGoalList.Remove(collidedWith);
			mCurrentGoalsAchieved++;
			if (mGoalsToAchieve == mCurrentGoalsAchieved)
			{
				IMGLevelManager.pInstance.GoalReached();
			}
		}
	}

	public void TriggerEvent(GameObject sender, GameObject triggeredWith)
	{
		if (onTrigger && mSourceList.Contains(sender) && mCurrentGoalList.Contains(triggeredWith))
		{
			mCurrentGoalList.Remove(triggeredWith);
			mCurrentGoalsAchieved++;
			if (mGoalsToAchieve == mCurrentGoalsAchieved)
			{
				IMGLevelManager.pInstance.GoalReached();
			}
		}
	}

	public void BalloonExloded(GameObject sender)
	{
		if (mSourceList.Contains(sender) && mGoalList.Count == 0)
		{
			IMGLevelManager.pInstance.GoalReached();
		}
	}

	public void AssignToGoalInfoList(ObjectBase objectToAdd)
	{
		switch (objectToAdd._ObjectProperty)
		{
		case ObjectProperty.SOURCE:
			mSourceList.Add(objectToAdd.gameObject);
			if (IMGLevelManager.pInstance._ParticleSource != null)
			{
				ParticleSystem particleSystem2 = Object.Instantiate(IMGLevelManager.pInstance._ParticleSource);
				particleSystem2.transform.parent = objectToAdd.transform;
				particleSystem2.transform.localPosition = Vector3.zero;
				objectToAdd._ParticleHighlight = particleSystem2;
				particleSystem2.Play();
			}
			break;
		case ObjectProperty.GOAL:
			mGoalsToAchieve++;
			mGoalList.Add(objectToAdd.gameObject);
			mCurrentGoalList.Add(objectToAdd.gameObject);
			if (IMGLevelManager.pInstance._ParticleGoal != null)
			{
				ParticleSystem particleSystem = Object.Instantiate(IMGLevelManager.pInstance._ParticleGoal);
				particleSystem.transform.parent = objectToAdd.transform;
				particleSystem.transform.localPosition = Vector3.zero;
				objectToAdd._ParticleHighlight = particleSystem;
				particleSystem.Play();
			}
			break;
		}
	}

	public bool IsSourceObject(GameObject obj)
	{
		return mSourceList.Contains(obj);
	}
}
