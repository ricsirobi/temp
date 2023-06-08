using System.Collections.Generic;
using KnowledgeAdventure.Multiplayer.Events;
using UnityEngine;

public class MultishotPowerUp : PowerUp
{
	public float _AmmoRange = 100f;

	public int _ShotCount = 3;

	private Bounds mTargetWindowBounds;

	private WeaponManager mWeaponManager;

	public override void Init(MonoBehaviour gameManager, PowerUpManager powerUpManager, MMOMessageReceivedEventArgs args)
	{
		base.Init(gameManager, powerUpManager, args);
	}

	public override void Activate()
	{
		base.Activate();
		if (!(SanctuaryManager.pCurPetInstance != null))
		{
			return;
		}
		if (mWeaponManager == null)
		{
			mWeaponManager = SanctuaryManager.pCurPetInstance.GetComponentInChildren<WeaponManager>();
		}
		List<Transform> list = SelectTargets();
		Debug.Log("count : " + list.Count);
		foreach (Transform item in list)
		{
			if (list != null)
			{
				SanctuaryManager.pCurPetInstance.Fire(item, useDirection: false, Vector3.zero, ignoreCoolDown: true);
			}
		}
	}

	protected virtual List<Transform> SelectTargets()
	{
		List<Transform> list = null;
		if (Camera.main != null)
		{
			float num = Screen.width;
			float num2 = Screen.height;
			float num3 = (float)(Screen.width / 2) - num / 2f;
			float num4 = (float)(Screen.height / 2) - num2 / 2f - (float)Screen.height;
			mTargetWindowBounds.SetMinMax(new Vector3(num3, (float)Screen.height - (num4 + num2), 0.1f), new Vector3(num3 + num, (float)Screen.height - num4, 1000f));
			list = new List<Transform>();
			Collider[] array = Physics.OverlapSphere(base.transform.position, layerMask: (1 << LayerMask.NameToLayer("IgnoreGroundRay")) | (1 << LayerMask.NameToLayer("Targetable")), radius: _AmmoRange);
			if (array == null)
			{
				Debug.Log("Didn't Find Any");
			}
			else
			{
				Debug.Log("SelectTargets: " + array.Length);
			}
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (IsValidTarget(collider.transform))
				{
					list.Add(collider.transform);
				}
			}
		}
		return list;
	}

	public virtual bool IsValidTarget(Transform inTarget)
	{
		if (inTarget != null && inTarget.gameObject != AvAvatar.pObject)
		{
			WeaponManager componentInChildren = SanctuaryManager.pCurPetInstance.GetComponentInChildren<WeaponManager>();
			if (componentInChildren == null)
			{
				return false;
			}
			if (Vector3.Dot(componentInChildren.pShootPoint, inTarget.position - componentInChildren.pShootPoint) > 0f)
			{
				ObTargetable component = inTarget.GetComponent<ObTargetable>();
				if (component != null && component._Active && (inTarget.transform.position - componentInChildren.pShootPoint).magnitude <= _AmmoRange)
				{
					Vector3 point = Camera.main.WorldToScreenPoint(inTarget.position);
					if (mTargetWindowBounds.Contains(point))
					{
						return true;
					}
				}
			}
		}
		return false;
	}
}
