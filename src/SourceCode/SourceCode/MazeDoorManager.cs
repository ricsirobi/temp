using UnityEngine;

public class MazeDoorManager : MonoBehaviour
{
	[SerializeField]
	private ObEnable[] m_Keys;

	[SerializeField]
	private GameObject m_Door;

	[SerializeField]
	private float m_SecondsBeforeUnlock = 2f;

	[SerializeField]
	private float m_SecondsAfterUnlock = 2f;

	[SerializeField]
	private Camera m_CutsceneCam;

	private void Start()
	{
		ObEnable[] keys = m_Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			keys[i].SetMazeDoorManager(this);
		}
	}

	public void CheckLock()
	{
		bool flag = true;
		ObEnable[] keys = m_Keys;
		for (int i = 0; i < keys.Length; i++)
		{
			if (!keys[i].IsActive())
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			OpenDoor();
		}
		else
		{
			CloseDoor();
		}
	}

	public Camera GetAlternativeCamera()
	{
		return m_CutsceneCam;
	}

	public float GetBeforeDelay()
	{
		return m_SecondsBeforeUnlock;
	}

	public float GetAfterDelay()
	{
		return m_SecondsAfterUnlock;
	}

	private void OpenDoor()
	{
		m_Door.SetActive(value: false);
	}

	private void CloseDoor()
	{
		m_Door.SetActive(value: true);
	}
}
