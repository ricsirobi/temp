using UnityEngine;

public class ObProximitySplineNodeText : MonoBehaviour
{
	public LocaleString _Text;

	public GameObject _Object;

	public float _ProximityDistance = 0.5f;

	public void SetObject(GameObject obj)
	{
		_Object = obj;
	}

	public void Update()
	{
		if (_Object != null && Vector3.Distance(base.transform.position, _Object.transform.position) < _ProximityDistance)
		{
			_Object.SendMessage("ShowNodeText", _Text.GetLocalizedString(), SendMessageOptions.DontRequireReceiver);
			base.enabled = false;
		}
	}
}
