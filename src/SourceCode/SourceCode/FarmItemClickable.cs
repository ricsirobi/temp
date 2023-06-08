using UnityEngine;

public class FarmItemClickable : MyRoomItemClickable
{
	public override void Start()
	{
		base.Start();
		mDisableMouseOverNotInBuildMode = GetComponent<DecorationFarmItem>() != null || GetComponent<FarmHouseItem>() != null;
	}

	public override void OnMouseDown()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (!(farmManager != null) || farmManager.pBuilder.pDragObject == null || !((GameObject)farmManager.pBuilder.pDragObject != base.gameObject))
		{
			base.OnMouseDown();
		}
	}

	public override void ProcessMouseUp()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (!(farmManager != null) || farmManager.pBuilder.pDragObject == null || !((GameObject)farmManager.pBuilder.pDragObject != base.gameObject))
		{
			base.ProcessMouseUp();
		}
	}

	public override void UnHighlight()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (farmManager != null && farmManager.pIsBuildMode && farmManager.pBuilder != null)
		{
			if (base.gameObject != farmManager.pBuilder.pSelectedObject)
			{
				ProcessUnHighlight();
			}
		}
		else
		{
			ProcessUnHighlight();
		}
	}

	public override bool WithinRange()
	{
		if (AvAvatar.pObject == null)
		{
			return false;
		}
		if (_Range != 0f && (AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_RangeOffset))).magnitude > _Range)
		{
			return false;
		}
		return true;
	}

	public override void ProcessMouseEnter()
	{
		if (WithinRange())
		{
			base.ProcessMouseEnter();
		}
	}
}
