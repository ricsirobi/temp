using UnityEngine;

namespace JSGames.UI.Util;

public static class UIUtility
{
	public static Vector2 ScreenPointToLocalNormalized(this RectTransform rectTransform, Canvas canvas, Vector3 screenPoint)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, canvas.worldCamera, out var localPoint);
		return Rect.PointToNormalized(rectTransform.rect, localPoint);
	}

	public static Vector2 ScreenToLocalPoint(this RectTransform rectTransform, Canvas canvas, Vector3 screenPoint)
	{
		return rectTransform.ScreenToLocalPoint(canvas, rectTransform.rect.size, screenPoint);
	}

	public static Vector2 ScreenToLocalPoint(this RectTransform rectTransform, Canvas canvas, Vector2 rectSize, Vector3 screenPoint)
	{
		Vector2 vector = rectTransform.ScreenPointToLocalNormalized(canvas, screenPoint);
		return new Vector2(rectSize.x * vector.x, rectSize.y * vector.y);
	}
}
