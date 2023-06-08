using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class FxAbilityData : FxData
{
	public List<FxObjectData> _DisableObjects = new List<FxObjectData>();

	public string _Location;

	public float _ResetTimer = 3f;

	public string _Name;

	private Transform mLocation;

	public override void Initialize(Transform parent)
	{
		foreach (FxObjectData enableObject in _EnableObjects)
		{
			enableObject.EnableObject(enable: false, parent);
		}
	}

	public override void PlayFx(Transform parent)
	{
		base.PlayFx(parent);
		if (mLocation == null && !string.IsNullOrEmpty(_Location))
		{
			Transform[] componentsInChildren = parent.GetComponentsInChildren<Transform>(includeInactive: true);
			foreach (Transform transform in componentsInChildren)
			{
				if (transform.name == _Location)
				{
					mLocation = transform;
					break;
				}
			}
		}
		if (mLocation != null)
		{
			foreach (FxObjectData enableObject in _EnableObjects)
			{
				if (enableObject._Object != null)
				{
					Vector3 localScale = enableObject._Object.transform.localScale;
					Quaternion localRotation = enableObject._Object.transform.localRotation;
					enableObject._Object.transform.parent = mLocation;
					enableObject._Object.transform.localPosition = Vector3.zero;
					enableObject._Object.transform.localRotation = localRotation;
					enableObject._Object.transform.localScale = localScale;
				}
			}
		}
		foreach (FxObjectData disableObject in _DisableObjects)
		{
			disableObject.EnableObject(enable: false, parent);
		}
	}

	public override void ResetFX(Transform parent)
	{
		base.ResetFX(parent);
		foreach (FxObjectData disableObject in _DisableObjects)
		{
			disableObject.EnableObject(enable: false, parent);
		}
	}
}
