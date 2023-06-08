using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LabelButtonsContextSensitiveStyle : IContextSensitiveStyle
{
	private ContextSensitiveUIStyleData mStyleData;

	private float mStartAlignX;

	private float mStartAlignY;

	private Vector2 mCloseBtnPos = Vector3.zero;

	private Rect mMenuBgRect = new Rect(0f, 0f, 1f, 1f);

	public LabelButtonsContextSensitiveStyle(ContextSensitiveUIStyleData inStyleData)
	{
		mStyleData = inStyleData;
	}

	public void UpdatePositionData(ContextData[] inDataList)
	{
		List<ContextData> list = inDataList.ToList();
		mStartAlignX = 0f;
		mStartAlignY = 0f;
		mMenuBgRect.x = mStartAlignX;
		mMenuBgRect.y = mStartAlignY;
		mMenuBgRect.width = GetWidgetsWidth(list.ToArray()) + mStyleData._MenuBackgroundExtraScalePixels.x;
		mMenuBgRect.height = GetMaxDimensionY(list.ToArray()) + mStyleData._MenuBackgroundExtraScalePixels.y;
		UpdateIconPositionData(list.ToArray(), mStartAlignX, mStartAlignY);
	}

	private void UpdateIconPositionData(ContextData[] inDataList, float inPosX, float inPosY)
	{
		float widgetsWidth = GetWidgetsWidth(inDataList);
		Vector2 vector = new Vector2(inPosX, inPosY);
		vector.x -= widgetsWidth / 2f;
		if (inDataList.Length != 0 && inDataList[0].pParent != null)
		{
			float maxDimensionY = GetMaxDimensionY(inDataList);
			maxDimensionY /= 2f;
			int num = ((mStyleData._SubMenuDirection != 0) ? 1 : (-1));
			maxDimensionY *= (float)num;
			vector.y += maxDimensionY;
		}
		foreach (ContextData contextData in inDataList)
		{
			vector.x += contextData._2DScaleInPixels.x / 2f;
			UpdateCloseButtonPos(vector, contextData);
			contextData.pPosition = new Vector3(vector.x, vector.y);
			if (contextData.pIsChildOpened)
			{
				Vector2 vector2 = vector;
				int num2 = ((mStyleData._SubMenuDirection != 0) ? 1 : (-1));
				vector.y += contextData._2DScaleInPixels.y / 2f * (float)num2;
				vector.y += mStyleData._WidgetOffsetInPixels.y * (float)num2;
				UpdateIconPositionData(contextData.pChildrenDataList.ToArray(), vector.x, vector.y);
				vector = vector2;
			}
			vector.x += contextData._2DScaleInPixels.x / 2f;
			vector.x += mStyleData._WidgetOffsetInPixels.x;
		}
	}

	private float GetMaxDimensionY(ContextData[] inDataList)
	{
		return inDataList.Select((ContextData data) => data._2DScaleInPixels.y).Prepend(0f).Max();
	}

	private float GetWidgetsWidth(ContextData[] inDataList)
	{
		return inDataList.Sum((ContextData data) => data._2DScaleInPixels.x);
	}

	private void UpdateCloseButtonPos(Vector2 inPos, ContextData inData)
	{
		mCloseBtnPos.x = ((inPos.x >= mCloseBtnPos.x) ? (inPos.x + inData._2DScaleInPixels.x / 2f) : mCloseBtnPos.x);
		mCloseBtnPos.y = ((inPos.y >= mCloseBtnPos.y) ? (inPos.y + inData._2DScaleInPixels.y) : mCloseBtnPos.y);
	}

	public Vector2 GetCloseButtonPosition()
	{
		return new Vector2(mCloseBtnPos.x, mCloseBtnPos.y);
	}

	public Rect GetMenuBackgroundRect()
	{
		return mMenuBgRect;
	}
}
