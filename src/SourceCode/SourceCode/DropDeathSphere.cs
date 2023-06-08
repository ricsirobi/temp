using System.Collections;
using UnityEngine;

public class DropDeathSphere : MonoBehaviour
{
	public GameObject deathBall;

	public GameObject[] dropPoints;

	private void Start()
	{
		StartCoroutine(DropTheBall());
	}

	private void Update()
	{
	}

	private IEnumerator DropTheBall()
	{
		Debug.Log("Made it to wait");
		yield return new WaitForSeconds(2f);
		Object.Instantiate(deathBall, dropPoints[Random.Range(0, 2)].transform);
		StartCoroutine(DropTheBall());
	}
}
