using UnityEngine;

public class DragonsZendeskCursorManager : MonoBehaviour
{
	public RectTransform _Cursor;

	private void LateUpdate()
	{
		_Cursor.position = Input.mousePosition;
	}

	private void OnEnable()
	{
		_Cursor.gameObject.SetActive(!UtPlatform.IsMobile());
	}

	private void OnDisable()
	{
	}
}
