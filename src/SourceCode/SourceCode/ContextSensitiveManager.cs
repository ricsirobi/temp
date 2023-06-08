using System;
using System.Collections.Generic;
using UnityEngine;

public class ContextSensitiveManager : MonoBehaviour
{
	public static Vector3 pCSMOffScreenPosition = new Vector3(0f, -1000f, 0f);

	private List<ObContextSensitive> mList = new List<ObContextSensitive>();

	private Vector3 mPrevAvatarPos = Vector3.zero;

	private ObContextSensitive mNearestObject;

	private void OnEnable()
	{
		ObClickable.AddActivatedEventHandler(RemoveAllMenus);
		KAUI.OnUIDisabled = (Action)Delegate.Combine(KAUI.OnUIDisabled, new Action(RemoveAllMenus));
	}

	private void OnDisable()
	{
		ObClickable.RemoveActivatedEventHandler(RemoveAllMenus);
		KAUI.OnUIDisabled = (Action)Delegate.Remove(KAUI.OnUIDisabled, new Action(RemoveAllMenus));
	}

	private void Update()
	{
		if (mList.Count <= 0)
		{
			return;
		}
		ObContextSensitive obContextSensitive = mNearestObject;
		float pProximityDistance = mList[0].pProximityDistance;
		mNearestObject = mList[0];
		int num = 0;
		foreach (ObContextSensitive m in mList)
		{
			if (m.pProximityDistance < pProximityDistance)
			{
				pProximityDistance = m.pProximityDistance;
				mNearestObject = m;
			}
			if (m.pProximityDistance < m._ProximityRange)
			{
				num++;
			}
		}
		if (!IsAvatarPositionChanged())
		{
			return;
		}
		List<ObContextSensitive> list = mList.FindAll((ObContextSensitive x) => x != mNearestObject);
		if (list.Count > 0)
		{
			foreach (ObContextSensitive item in list)
			{
				item.RemoveMenuOfType(ContextSensitiveStateType.PROXIMITY);
			}
		}
		if (num > 1 && mNearestObject != obContextSensitive)
		{
			mNearestObject.SetProximityAlreadyEntered(isEntered: false);
		}
	}

	private void LateUpdate()
	{
		mPrevAvatarPos = AvAvatar.GetPosition();
	}

	public void Add(ObContextSensitive inInstance)
	{
		if (!mList.Find((ObContextSensitive x) => x == inInstance))
		{
			mList.Add(inInstance);
		}
	}

	public void Remove(ObContextSensitive inInstance)
	{
		if ((bool)mList.Find((ObContextSensitive x) => x == inInstance))
		{
			mList.Remove(inInstance);
		}
	}

	public List<ObContextSensitive> GetList()
	{
		return mList;
	}

	public void RemoveOtherMenusOfType(ContextSensitiveStateType inType, ObContextSensitive inInstance)
	{
		foreach (ObContextSensitive m in mList)
		{
			if (m != inInstance)
			{
				m.RemoveMenuOfType(inType);
			}
		}
	}

	public void RemoveAllMenus()
	{
		RemoveAllMenus(null);
	}

	public void RemoveAllMenus(GameObject go)
	{
		foreach (ObContextSensitive m in mList)
		{
			m.RemoveMenuOfType(ContextSensitiveStateType.OBJECT_STATE);
			m.RemoveMenuOfType(ContextSensitiveStateType.PROXIMITY);
			m.RemoveMenuOfType(ContextSensitiveStateType.ONCLICK);
		}
	}

	public bool IsAvatarPositionChanged()
	{
		return Vector3.Distance(AvAvatar.GetPosition(), mPrevAvatarPos) != 0f;
	}

	public bool IsNearestProximityObject(ObContextSensitive inObj)
	{
		if (IsAvatarPositionChanged())
		{
			return inObj == mNearestObject;
		}
		return IsNoOtherObjectHasProximityUI(inObj);
	}

	public bool IsNoOtherObjectHasProximityUI(ObContextSensitive inObj)
	{
		if (mList.Count == 0)
		{
			return false;
		}
		ObContextSensitive obContextSensitive = mList.Find((ObContextSensitive x) => x != mNearestObject && x.pCurrentPriority != ContextSensitiveStateType.PROXIMITY);
		if (inObj == mNearestObject)
		{
			return obContextSensitive != null;
		}
		return false;
	}

	public void SetVisibility(bool isVisible)
	{
		foreach (ObContextSensitive m in mList)
		{
			if (m.pUI != null)
			{
				m.pUI.SetVisibility(isVisible);
			}
		}
	}
}
