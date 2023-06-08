using System.Collections;
using UnityEngine;

public class ObEnable : MonoBehaviour
{
	[SerializeField]
	private GameObject key;

	[SerializeField]
	private GameObject[] activateObjects;

	[SerializeField]
	private bool m_IsActive;

	[SerializeField]
	private bool showCutscene = true;

	[SerializeField]
	private MazeDoorManager m_MazeDoor;

	[SerializeField]
	private Camera mCamera;

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger && other.gameObject == key)
		{
			if (showCutscene && m_MazeDoor != null)
			{
				StartCoroutine(EnableObjectsWithCutscene());
			}
			else
			{
				EnableObjects();
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.isTrigger && other.gameObject == key)
		{
			StopCoroutine(EnableObjectsWithCutscene());
			DisableObjects();
		}
	}

	private IEnumerator EnableObjectsWithCutscene()
	{
		m_IsActive = true;
		if (showCutscene)
		{
			yield return new WaitForSeconds(1.5f);
			if (m_MazeDoor != null)
			{
				mCamera.enabled = true;
			}
			yield return new WaitForSeconds(m_MazeDoor.GetBeforeDelay());
			GameObject[] array = activateObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: true);
			}
			yield return new WaitForSeconds(m_MazeDoor.GetAfterDelay());
			mCamera.enabled = false;
		}
		m_MazeDoor.CheckLock();
	}

	private void EnableObjects()
	{
		m_IsActive = true;
		GameObject[] array = activateObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: true);
		}
		m_MazeDoor.CheckLock();
	}

	private void DisableObjects()
	{
		m_IsActive = false;
		GameObject[] array = activateObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(value: false);
		}
	}

	public void SetMazeDoorManager(MazeDoorManager mazeDoor)
	{
		m_MazeDoor = mazeDoor;
	}

	public bool IsActive()
	{
		return m_IsActive;
	}
}
