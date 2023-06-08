using System;
using UnityEngine;

public class MouseEvent : KAMonoBase
{
	public bool pIsMouseOnTop;

	public float _CheckDistance = 100f;

	public Camera _Camera;

	[NonSerialized]
	public ObClickable _ObClickable;

	private void Start()
	{
		if (Application.isEditor)
		{
			UnityEngine.Object.Destroy(this);
		}
		else if (_Camera == null)
		{
			_Camera = Camera.main;
		}
	}

	public void SetCamera(Camera inCam)
	{
		_Camera = inCam;
	}

	private void Update()
	{
		if (_Camera == null || Input.touchCount == 0 || collider == null)
		{
			return;
		}
		if (pIsMouseOnTop && Input.GetMouseButtonUp(0))
		{
			pIsMouseOnTop = false;
		}
		if (_ObClickable != null && !_ObClickable.IsActive())
		{
			return;
		}
		Ray ray = _Camera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;
		bool flag = collider.Raycast(ray, out hitInfo, _CheckDistance);
		if (pIsMouseOnTop)
		{
			if (flag)
			{
				SendMessage("OnMouseOver", SendMessageOptions.DontRequireReceiver);
				if (Input.GetMouseButtonDown(0))
				{
					SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
				}
				else if (Input.GetMouseButtonUp(0))
				{
					pIsMouseOnTop = false;
					SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver);
					SendMessage("OnMouseUp", SendMessageOptions.DontRequireReceiver);
				}
				else if (Input.GetMouseButton(0))
				{
					SendMessage("OnMouseDrag", SendMessageOptions.DontRequireReceiver);
				}
			}
			else
			{
				pIsMouseOnTop = false;
				base.gameObject.SendMessage("OnMouseExit", SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (flag)
		{
			pIsMouseOnTop = true;
			base.gameObject.SendMessage("OnMouseEnter", SendMessageOptions.DontRequireReceiver);
			if (Input.GetMouseButtonDown(0))
			{
				SendMessage("OnMouseDown", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
