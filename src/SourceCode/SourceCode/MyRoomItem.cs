using System.Collections.Generic;
using UnityEngine;

public class MyRoomItem : ObContextSensitive
{
	public ContextSensitiveState _DefaultContextSensitiveState;

	protected ObClickable mClickable;

	protected bool mCanShowContextMenu = true;

	protected bool mItemJustCreated;

	protected bool mItemPlacementDirty;

	protected UserItemData mUserItemData;

	public ObClickable pClickable
	{
		get
		{
			if (mClickable == null)
			{
				mClickable = GetComponent<ObClickable>();
			}
			return mClickable;
		}
	}

	public bool pCanShowContextMenu
	{
		get
		{
			return mCanShowContextMenu;
		}
		set
		{
			mCanShowContextMenu = value;
		}
	}

	public bool pItemJustCreated
	{
		get
		{
			return mItemJustCreated;
		}
		set
		{
			mItemJustCreated = value;
		}
	}

	public bool pItemPlacementDirty
	{
		get
		{
			return mItemPlacementDirty;
		}
		set
		{
			mItemPlacementDirty = value;
		}
	}

	public UserItemData pUserItemData
	{
		get
		{
			return mUserItemData;
		}
		set
		{
			mUserItemData = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		_UIFollowingTarget = base.gameObject;
	}

	public virtual void OnBuildModeChanged(bool inBuildMode)
	{
		HighlightObject(canShowHightlight: false);
		DestroyMenu(checkProximity: false);
		UpdateCollider();
	}

	protected void UpdateCollider()
	{
		BoxCollider component = GetComponent<BoxCollider>();
		MeshCollider component2 = GetComponent<MeshCollider>();
		if (component != null && component2 != null && MyRoomsIntMain.pInstance != null)
		{
			bool flag = (component.enabled = MyRoomsIntMain.pInstance.pIsBuildMode);
			component2.enabled = !flag;
			MyRoomObject component3 = base.gameObject.GetComponent<MyRoomObject>();
			if (component3 != null)
			{
				component3.collider = (flag ? ((Collider)component) : ((Collider)component2));
			}
		}
	}

	public virtual void HighlightObject(bool canShowHightlight)
	{
		ObClickable component = GetComponent<ObClickable>();
		if (component != null)
		{
			if (canShowHightlight && component._HighlightMaterial != null)
			{
				component.Highlight();
			}
			else
			{
				component.UnHighlight();
			}
		}
	}

	protected override void OnProximityEnter()
	{
		if (pCanShowContextMenu)
		{
			base.OnProximityEnter();
		}
	}

	public void SetCSMVisible(bool inVisibility)
	{
		if (base.pUI != null)
		{
			base.pUI.SetVisibility(inVisibility);
		}
	}

	protected virtual void UpdateScaleData(ref ContextSensitiveState[] inStatesArrData)
	{
		Vector3 zero = Vector3.zero;
		ContextSensitiveState[] array = inStatesArrData;
		foreach (ContextSensitiveState contextSensitiveState in array)
		{
			if (contextSensitiveState != null && contextSensitiveState._CurrentContextNamesList != null && contextSensitiveState._CurrentContextNamesList.Length != 0)
			{
				contextSensitiveState._UIScale = zero;
			}
		}
	}

	public virtual void UpdateContextItemState(bool inEnable)
	{
		if (inEnable != GetInteractiveEnabledData("Accept"))
		{
			SetInteractiveEnabledData("Accept", inEnable);
		}
	}

	protected virtual void OnContextAction(string inActionName)
	{
		UiMyRoomBuilder myRoomBuilder = MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder;
		switch (inActionName)
		{
		case "Accept":
			myRoomBuilder.OnClickAction("Accept");
			break;
		case "Move":
			myRoomBuilder.OnClickAction("ObjectMoveBtn");
			break;
		case "Rotate":
			KAInput.ResetInputAxes();
			myRoomBuilder.OnClickAction("ObjectRotateRtBtn");
			break;
		case "Pack Away":
			KAInput.ResetInputAxes();
			myRoomBuilder.OnClickAction("ObjectPickUpBtn");
			break;
		case "Destroy":
			KAInput.ResetInputAxes();
			myRoomBuilder.OnClickAction("ObjectDestroyBtn");
			break;
		}
	}

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		List<ContextSensitiveState> csData = new List<ContextSensitiveState>(inStatesArrData);
		inStatesArrData = GetSensitiveData(csData);
	}

	protected virtual ContextSensitiveState[] GetSensitiveData(List<ContextSensitiveState> csData)
	{
		if (MyRoomsIntMain.pInstance != null && MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			csData.Add(_DefaultContextSensitiveState);
		}
		return csData.ToArray();
	}

	protected virtual void ProcessSensitiveData(ref List<string> menuItemNames)
	{
	}

	protected virtual bool CanActivate()
	{
		return true;
	}

	public virtual void SetState(UserItemState state)
	{
	}
}
