using UnityEngine;
using UnityEngine.UI;

namespace JSGames.Tween;

public static class Extensions
{
	public static void SetPosition(this Transform transform, Vector3 position)
	{
		transform.position = position;
	}

	public static void SetPosition(this RectTransform transform, Vector3 position)
	{
		transform.anchoredPosition3D = position;
	}

	public static void SetPosition(this Transform transform, Vector2 position)
	{
		Vector3 position2 = position;
		position2.z = transform.position.z;
		transform.position = position2;
	}

	public static void SetPosition(this RectTransform rectTransform, Vector2 position)
	{
		rectTransform.anchoredPosition = position;
	}

	public static void SetLocalPosition(this Transform transform, Vector3 position)
	{
		transform.localPosition = position;
	}

	public static void SetLocalPosition(this Transform transform, Vector2 position)
	{
		Vector3 localPosition = position;
		localPosition.z = transform.localPosition.z;
		transform.localPosition = localPosition;
	}

	public static void SetLocalScale(this Transform transform, Vector3 scale)
	{
		transform.localScale = scale;
	}

	public static void SetLocalScale(this Transform transform, Vector2 scale)
	{
		Vector3 localScale = scale;
		localScale.z = transform.localScale.z;
		transform.localScale = localScale;
	}

	public static void SetRotation(this Transform transform, Vector3 angle)
	{
		transform.rotation = Quaternion.Euler(angle);
	}

	public static void SetLocalRotation(this Transform transform, Vector3 angle)
	{
		transform.localRotation = Quaternion.Euler(angle);
	}

	public static void SetColor(this Renderer renderer, Vector4 color)
	{
		renderer.material.color = color;
	}

	public static void SetColor(this TextMesh textMesh, Vector4 color)
	{
		textMesh.color = color;
	}

	public static void SetColor(this Graphic uiGraphic, Vector4 color)
	{
		uiGraphic.color = color;
	}

	public static void SetAlpha(this Renderer renderer, float val)
	{
		for (int i = 0; i < renderer.materials.Length; i++)
		{
			Color color = renderer.material.color;
			color.a = val;
			renderer.material.color = color;
		}
	}

	public static void SetText(this TextMesh textMesh, string text)
	{
		textMesh.text = text;
	}

	public static void SetText(this Text uiText, string text)
	{
		uiText.text = text;
	}
}
