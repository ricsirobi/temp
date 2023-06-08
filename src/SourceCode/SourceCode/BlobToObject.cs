using UnityEngine;

public class BlobToObject : MonoBehaviour
{
	public GameObject _ShadowObject;

	public float _DisplaceY;

	private void Update()
	{
		if (_ShadowObject != null)
		{
			if (_ShadowObject.activeInHierarchy)
			{
				Vector3 position = _ShadowObject.transform.position;
				position.y += _DisplaceY;
				base.transform.position = position;
			}
			else
			{
				base.transform.position = Vector3.up * -10000f;
			}
		}
	}
}
