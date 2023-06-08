using UnityEngine;

namespace ShatterToolkit.Helpers;

public class MouseShatter : MonoBehaviour
{
	public void Update()
	{
		if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hitInfo))
		{
			hitInfo.collider.SendMessage("Shatter", hitInfo.point, SendMessageOptions.DontRequireReceiver);
		}
	}
}
