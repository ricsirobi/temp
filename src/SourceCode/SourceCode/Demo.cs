using UnityEngine;

public class Demo : MonoBehaviour
{
	public GameObject _GameObject;

	private void Update()
	{
		if ((Input.touchCount >= 3 || ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.D))) && _GameObject != null)
		{
			_GameObject.SetActive(value: true);
		}
	}
}
