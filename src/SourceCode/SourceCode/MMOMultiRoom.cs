using System.Collections.Generic;
using UnityEngine;

public class MMOMultiRoom : MonoBehaviour
{
	public List<MMOZone> _Area;

	public string _DefaultZone = "";

	private Vector3 mLastPosition = new Vector3(0f, -5000f, 0f);

	private void Update()
	{
		if (MainStreetMMOClient.pInstance == null || RsResourceManager.pLevelLoading || MainStreetMMOClient.pInstance.pState < MMOClientState.CONNECTING)
		{
			mLastPosition = new Vector3(0f, -5000f, 0f);
			return;
		}
		Vector3 position = AvAvatar.position;
		if (!((position - mLastPosition).magnitude > 1f))
		{
			return;
		}
		mLastPosition = position;
		for (int i = 0; i < _Area.Count; i++)
		{
			Collider component = _Area[i]._Area.GetComponent<Collider>();
			if (component != null)
			{
				Vector3 direction = component.bounds.center - position;
				Ray ray = new Ray(position, direction);
				if (!component.Raycast(ray, out var _, direction.magnitude))
				{
					CheckLogin(i);
					return;
				}
			}
			else if (_Area[i]._Area.GetComponent<Renderer>().bounds.Contains(position))
			{
				CheckLogin(i);
				return;
			}
		}
		if (!string.IsNullOrEmpty(_DefaultZone) && _DefaultZone != MainStreetMMOClient.pInstance.pZone)
		{
			MainStreetMMOClient.pInstance.LoginToZone(_DefaultZone);
		}
	}

	private void CheckLogin(int i)
	{
		if (_Area[i]._Zone != MainStreetMMOClient.pInstance.pZone)
		{
			MainStreetMMOClient.pInstance.LoginToZone(_Area[i]._Zone);
			if (i != 0)
			{
				MMOZone item = _Area[i];
				_Area.RemoveAt(i);
				_Area.Insert(0, item);
			}
		}
	}
}
