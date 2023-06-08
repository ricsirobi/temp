using UnityEngine;

public class MouseSplitContinous : MonoBehaviour
{
	public int _Distance;

	public int raycastCount = 10;

	private Vector3 start;

	private Vector3 end;

	public void Start()
	{
		start = Input.mousePosition;
	}

	public void Update()
	{
		if (Input.GetMouseButton(0))
		{
			end = Input.mousePosition;
			Camera main = Camera.main;
			float nearClipPlane = main.nearClipPlane;
			if ((start - end).magnitude > (float)_Distance)
			{
				Vector3 lhs = main.ScreenToWorldPoint(new Vector3(end.x, end.y, nearClipPlane)) - main.ScreenToWorldPoint(new Vector3(start.x, start.y, nearClipPlane));
				for (int i = 0; i < raycastCount; i++)
				{
					Ray ray = main.ScreenPointToRay(Vector3.Lerp(start, end, (float)i / (float)raycastCount));
					if (Physics.Raycast(ray, out var hitInfo))
					{
						Plane plane = new Plane(Vector3.Normalize(Vector3.Cross(lhs, ray.direction)), hitInfo.point);
						hitInfo.collider.SendMessage("Split", new Plane[1] { plane }, SendMessageOptions.DontRequireReceiver);
					}
				}
			}
		}
		start = Input.mousePosition;
	}
}
