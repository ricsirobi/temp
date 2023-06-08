using System.Collections.Generic;
using UnityEngine;

public class IMGLevelManager : KAMonoBase
{
	public string levelNumber;

	public Transform activeContainer;

	public Transform toolboxContainer;

	public AudioClip _SoundGoalReached;

	protected static IMGLevelManager mInstance;

	public ParticleSystem _ParticleSource;

	public ParticleSystem _ParticleGoal;

	protected List<ObjectBase> activeObjects;

	protected List<ObjectBase> placedObjects;

	protected int starsCollected;

	protected bool goalReached;

	protected bool inPlayMode;

	private bool mGamePaused;

	public static IMGLevelManager pInstance => mInstance;

	public bool pGoalReached => goalReached;

	public bool pGamePaused
	{
		get
		{
			return mGamePaused;
		}
		set
		{
			mGamePaused = value;
		}
	}

	protected virtual void Start()
	{
		mInstance = this;
		activeObjects = new List<ObjectBase>();
		placedObjects = new List<ObjectBase>();
		foreach (Transform item in activeContainer)
		{
			if ((bool)item.GetComponent<ObjectBase>())
			{
				activeObjects.Add(item.GetComponent<ObjectBase>());
				item.GetComponent<ObjectBase>().Setup();
			}
		}
	}

	public virtual void EnableScene()
	{
		inPlayMode = true;
		foreach (ObjectBase activeObject in activeObjects)
		{
			activeObject.Enable();
		}
	}

	public virtual void ResetScene()
	{
		inPlayMode = false;
		mGamePaused = false;
		starsCollected = 0;
		foreach (ObjectBase activeObject in activeObjects)
		{
			activeObject.Reset();
		}
		goalReached = false;
		GoalManager.pInstance.ResetGoal();
	}

	public virtual void RestartScene()
	{
		inPlayMode = false;
		mGamePaused = false;
		goalReached = false;
	}

	public void RestartFromFinish()
	{
		inPlayMode = false;
		ResetScene();
		goalReached = false;
	}

	public void StarCollected()
	{
		starsCollected++;
	}

	public int GetStarsCollected()
	{
		return starsCollected;
	}

	public virtual void GoalReached()
	{
		if (!goalReached)
		{
			if (_SoundGoalReached != null)
			{
				SnChannel.Play(_SoundGoalReached, "SFX_Pool", inForce: true);
			}
			goalReached = true;
		}
	}

	public void AddItem(ObjectBase item)
	{
		if (!activeObjects.Contains(item))
		{
			activeObjects.Add(item);
		}
		item.transform.parent = activeContainer;
	}

	public void AddItemFromTool(ObjectBase item)
	{
		if (!placedObjects.Contains(item))
		{
			placedObjects.Add(item);
		}
	}

	public virtual void RemoveItem(ObjectBase item)
	{
		activeObjects.Remove(item);
		placedObjects.Remove(item);
	}

	public virtual void SaveData()
	{
		if (PlayerPrefs.GetInt(levelNumber) < starsCollected)
		{
			PlayerPrefs.SetInt(levelNumber, starsCollected);
			PlayerPrefs.Save();
		}
	}

	public bool InPlayMode()
	{
		return inPlayMode;
	}
}
