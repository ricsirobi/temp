using UnityEngine;

public class CheckPoint : MonoBehaviour
{
	public int _Index;

	public float _TimeToReachNext = 5f;

	private bool mVisited;

	private void Start()
	{
		mVisited = false;
	}

	private void OnEnable()
	{
		mVisited = false;
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.CompareTag("Player") || mVisited)
		{
			return;
		}
		ObstacleCourseGame component = base.transform.parent.parent.Find("PfRingRaceGame").GetComponent<ObstacleCourseGame>();
		if (!component.ReachedCheckpoint(_Index, _TimeToReachNext))
		{
			return;
		}
		Component[] componentsInChildren = base.transform.GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			((Renderer)componentsInChildren[i]).enabled = false;
		}
		mVisited = true;
		if (component._NextMarker != null)
		{
			if (_TimeToReachNext > 0.01f)
			{
				component._NextMarker.position = Vector3.zero;
				Transform child = base.transform.parent.GetChild(_Index);
				component._NextMarker.position = child.position;
			}
			else
			{
				component._NextMarker.parent = null;
				component._NextMarker.position = Vector3.up * 5000f;
			}
		}
	}
}
