using UnityEngine;

public class BoundaryTrigger : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnTriggerEnter(Collider col)
	{
		if (AvAvatar.IsCurrentPlayer(col.gameObject))
		{
			Transform transform = col.gameObject.transform;
			transform.Rotate(0f, 180f, 0f);
			col.gameObject.transform.rotation = transform.rotation;
		}
	}
}
