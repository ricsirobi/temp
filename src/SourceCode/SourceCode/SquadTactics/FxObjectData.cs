using System;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class FxObjectData
{
	public enum FXTYPE
	{
		DEFAULT,
		STARTPARTICLE,
		PROJECTILE,
		ENDPARTICLE
	}

	public FXTYPE _FXType;

	public GameObject _Object;

	public string _Name;

	public string _OverrideParentLocation;

	public float _ProjectileSpeed = 100f;

	private void SearchForObject(Transform parent)
	{
		if (!(_Object == null) || string.IsNullOrEmpty(_Name))
		{
			return;
		}
		Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (transform.name == _Name)
			{
				_Object = transform.gameObject;
				break;
			}
		}
	}

	public void EnableObject(bool enable, Transform parent)
	{
		if (_Object != null)
		{
			_Object.SetActive(enable);
		}
		else if (parent != null)
		{
			SearchForObject(parent);
			if (_Object != null)
			{
				_Object.SetActive(enable);
			}
		}
	}

	public FxObjectData(GameObject go)
	{
		_Object = go;
	}
}
