using UnityEngine;

public class ObStoreLoader : MonoBehaviour
{
	public string _StoreResource = "";

	public string _MainMenuSelection = "";

	public string _StoreSelection = "";

	public bool _SetDefaultMenuItem = true;

	public Transform _ExitMarker;

	public void OnActivate()
	{
		Launch();
	}

	public void OnProximity()
	{
		Launch();
	}

	public void OnTrigger()
	{
		Launch();
	}

	private void Launch()
	{
		if (!string.IsNullOrEmpty(_StoreResource))
		{
			StoreLoader.Load(_StoreResource, _SetDefaultMenuItem, _MainMenuSelection, _StoreSelection, base.gameObject, UILoadOptions.AUTO, "", _ExitMarker.name);
		}
		else
		{
			StoreLoader.Load(_SetDefaultMenuItem, _MainMenuSelection, _StoreSelection, base.gameObject, UILoadOptions.AUTO, "", _ExitMarker.name);
		}
	}

	private void OnStoreClosed()
	{
		if (_ExitMarker != null)
		{
			AvAvatar.SetPosition(_ExitMarker);
		}
	}
}
