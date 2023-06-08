using UnityEngine;

public class ObGroupActivate : MonoBehaviour
{
	public GameObject[] _Objects;

	private GameObject mActiveObject;

	private void Start()
	{
		if (_Objects != null && _Objects.Length != 0)
		{
			mActiveObject = _Objects[0];
			mActiveObject.SetActive(value: true);
		}
	}

	private void Update()
	{
		if (_Objects != null)
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				ActivateObject(0);
			}
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				ActivateObject(1);
			}
			else if (Input.GetKey(KeyCode.Alpha3))
			{
				ActivateObject(2);
			}
			else if (Input.GetKey(KeyCode.Alpha4))
			{
				ActivateObject(3);
			}
			else if (Input.GetKey(KeyCode.Alpha5))
			{
				ActivateObject(4);
			}
			else if (Input.GetKey(KeyCode.Alpha6))
			{
				ActivateObject(5);
			}
			else if (Input.GetKey(KeyCode.Alpha7))
			{
				ActivateObject(6);
			}
			else if (Input.GetKey(KeyCode.Alpha8))
			{
				ActivateObject(7);
			}
			else if (Input.GetKey(KeyCode.Alpha9))
			{
				ActivateObject(8);
			}
			else if (Input.GetKey(KeyCode.Alpha0))
			{
				ActivateObject(9);
			}
		}
	}

	private void ActivateObject(int index)
	{
		if (_Objects.Length > index)
		{
			mActiveObject.SetActive(value: false);
			mActiveObject = _Objects[index];
			mActiveObject.SetActive(value: true);
		}
	}
}
