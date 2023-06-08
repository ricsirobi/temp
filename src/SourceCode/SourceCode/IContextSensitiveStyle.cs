using UnityEngine;

public interface IContextSensitiveStyle
{
	void UpdatePositionData(ContextData[] inDataList);

	Vector2 GetCloseButtonPosition();

	Rect GetMenuBackgroundRect();
}
