using UnityEngine;

public class MyRotation : MonoBehaviour
{
	private bool isRotating = true;

	private Animator anmator;

	private void Awake()
	{
		anmator = GetComponent<Animator>();
		if (anmator != null)
		{
			SetLoading(isRotating);
		}
	}

	public void SetLoading(bool isLoading)
	{
		base.gameObject.SetActive(isLoading);
		anmator.SetBool("IsRotating", isLoading);
	}

	public void StartRotation()
	{
		base.gameObject.SetActive(value: true);
		anmator.SetBool("IsRotating", value: true);
	}

	public void StopRotation()
	{
		base.gameObject.SetActive(value: false);
		anmator.SetBool("IsRotating", value: false);
	}
}
