using System.Collections;
using SWS;
using UnityEngine;
using UnityEngine.AI;

public class EventReceiver : MonoBehaviour
{
	public void MyMethod()
	{
	}

	public void PrintText(string text)
	{
		Debug.Log(text);
	}

	public void RotateSprite(float newRot)
	{
		Vector3 eulerAngles = base.transform.eulerAngles;
		eulerAngles.y = newRot;
		base.transform.eulerAngles = eulerAngles;
	}

	public void SetDestination(Object target)
	{
		StartCoroutine(SetDestinationRoutine(target));
	}

	private IEnumerator SetDestinationRoutine(Object target)
	{
		NavMeshAgent agent = GetComponent<NavMeshAgent>();
		navMove myMove = GetComponent<navMove>();
		GameObject gameObject = (GameObject)target;
		myMove.ChangeSpeed(4f);
		agent.SetDestination(gameObject.transform.position);
		while (agent.pathPending)
		{
			yield return null;
		}
		float remain = agent.remainingDistance;
		while (remain == float.PositiveInfinity || remain - agent.stoppingDistance > float.Epsilon || agent.pathStatus != 0)
		{
			remain = agent.remainingDistance;
			yield return null;
		}
		yield return new WaitForSeconds(4f);
		myMove.ChangeSpeed(1.5f);
		myMove.moveToPath = true;
		myMove.StartMove();
	}

	public void ActivateForTime(Object target)
	{
		StartCoroutine(ActivateForTimeRoutine(target));
	}

	private IEnumerator ActivateForTimeRoutine(Object target)
	{
		GameObject tar = (GameObject)target;
		tar.SetActive(value: true);
		yield return new WaitForSeconds(6f);
		tar.SetActive(value: false);
	}
}
