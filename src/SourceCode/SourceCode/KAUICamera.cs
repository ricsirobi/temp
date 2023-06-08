using UnityEngine;

[RequireComponent(typeof(Camera))]
public class KAUICamera : UICamera
{
	public bool _ClearEventMask = true;

	public float mDragTimer;

	public bool mDragStarted;

	public float _DragDelay = 0.25f;

	public KAWidget pSelectedWidget
	{
		get
		{
			if (UICamera.selectedObject == null)
			{
				return null;
			}
			return UICamera.selectedObject.GetComponent<KAWidget>();
		}
	}

	public override void ProcessMouse()
	{
		base.ProcessMouse();
		bool flag = false;
		if (UICamera.currentTouchID != -100)
		{
			flag = UICamera.GetTouch(UICamera.currentTouchID, createIfMissing: true).delta.sqrMagnitude > 0.001f;
		}
		bool flag2 = false;
		bool flag3 = false;
		if (GetMouseButtonDown())
		{
			flag3 = true;
			flag2 = true;
		}
		else if (GetMouseButton())
		{
			flag2 = true;
		}
		if (!flag3 && flag2 && !flag)
		{
			UICamera.currentScheme = ControlScheme.Mouse;
			if (UICamera.mHover == UICamera.hoveredObject)
			{
				UICamera.Notify(UICamera.hoveredObject, "OnPressRepeated", true);
			}
		}
	}

	public override void ProcessTouch(bool pressed, bool unpressed)
	{
		base.ProcessTouch(pressed, unpressed);
		if (Input.touchCount > 0 && UICamera.currentTouch.pressed != null && UICamera.currentTouch.phase == TouchPhase.Stationary)
		{
			UICamera.Notify(UICamera.currentTouch.pressed, "OnPressRepeated", true);
		}
	}

	public static bool IsHovering()
	{
		if (UICamera.hoveredObject != null)
		{
			return UICamera.hoveredObject != UICamera.fallThrough;
		}
		return false;
	}

	protected override bool NotifyEvent(GameObject go, string funcName, object obj)
	{
		if (go != null)
		{
			KAWidget component = go.GetComponent<KAWidget>();
			KAUI kAUI = null;
			kAUI = ((!(component != null)) ? go.GetComponent<KAUI>() : component.pUI);
			if (kAUI != null && kAUI.pEvents != null)
			{
				if (KAUI._GlobalExclusiveUI != null && KAUI._GlobalExclusiveUI != kAUI)
				{
					KAUI[] componentsInChildren = KAUI._GlobalExclusiveUI.GetComponentsInChildren<KAUI>();
					bool flag = false;
					KAUI[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] == kAUI)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						array = KAUI._GlobalExclusiveUI._UiList;
						for (int i = 0; i < array.Length; i++)
						{
							if (array[i] == kAUI)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							KAUIMenu[] menuList = KAUI._GlobalExclusiveUI._MenuList;
							foreach (KAUIMenu kAUIMenu in menuList)
							{
								if (kAUI == kAUIMenu)
								{
									flag = true;
									break;
								}
							}
						}
					}
					if (!flag)
					{
						if (funcName == "OnHover" && !(bool)obj)
						{
							kAUI.pEvents.ProcessHoverEvent(component, (bool)obj);
						}
						return true;
					}
				}
				switch (funcName)
				{
				case "OnHover":
					kAUI.pEvents.ProcessHoverEvent(component, (bool)obj);
					break;
				case "OnClick":
					kAUI.pEvents.ProcessClickEvent(component);
					break;
				case "OnPress":
				{
					bool flag2 = (bool)obj;
					kAUI.pEvents.ProcessPressEvent(component, flag2);
					kAUI.pEvents.ProcessPressRepeatedEvent(component, flag2);
					if (flag2)
					{
						UICamera.mTooltipTime = RealTime.time + tooltipDelay;
						mDragTimer = RealTime.time + _DragDelay;
						break;
					}
					if (mDragStarted)
					{
						kAUI.pEvents.ProcessDragEndEvent(component);
					}
					mDragTimer = 0f;
					mDragStarted = false;
					UICamera.Notify(UICamera.currentTouch.pressed, "OnSwipe", UICamera.currentTouch.totalDelta);
					break;
				}
				case "OnPressRepeated":
					kAUI.pEvents.ProcessPressRepeatedEvent(component, (bool)obj);
					if (mDragTimer > 0f && mDragTimer < RealTime.time)
					{
						UICamera.Notify(go, "OnDragEvent", null);
					}
					break;
				case "OnInput":
					kAUI.pEvents.ProcessInputEvent(component, (string)obj);
					break;
				case "OnDragEvent":
					kAUI.pEvents.ProcessDragStartEvent(component);
					mDragTimer = 0f;
					mDragStarted = true;
					break;
				case "OnScroll":
					kAUI.pEvents.ProcessScrollEvent(component, (float)obj);
					break;
				case "OnDoubleClick":
					kAUI.pEvents.ProcessDoubleClickEvent(component);
					break;
				case "OnTooltip":
					kAUI.pEvents.ProcessToolTipEvent(component, (bool)obj);
					break;
				case "OnDrag":
				{
					Vector2 inVal = (Vector2)obj;
					mDragTimer = 0f;
					if (!mDragStarted)
					{
						kAUI.pEvents.ProcessDragEvent(component, inVal);
					}
					break;
				}
				case "OnDrop":
					if (mDragStarted)
					{
						kAUI.pEvents.ProcessDropEvent(component, (obj as GameObject).GetComponent<KAWidget>());
					}
					mDragTimer = 0f;
					mDragStarted = false;
					break;
				case "OnSelect":
					kAUI.pEvents.ProcessSelectEvent(component, (bool)obj);
					break;
				case "OnSwipe":
					kAUI.pEvents.ProcessSwipeEvent(component, (Vector2)obj);
					break;
				case "OnDragEnd":
					if (mDragStarted)
					{
						kAUI.pEvents.ProcessDragEndEvent(component);
					}
					mDragTimer = 0f;
					mDragStarted = false;
					break;
				}
				return true;
			}
			base.NotifyEvent(go, funcName, obj);
		}
		return false;
	}

	public virtual void ForceProcessTouch(bool pressed, bool unpressed)
	{
		ProcessTouch(pressed, unpressed);
	}

	protected override void Start()
	{
		int eventMask = -1;
		if (!_ClearEventMask)
		{
			eventMask = base.cachedCamera.eventMask;
		}
		base.Start();
		if (!_ClearEventMask)
		{
			base.cachedCamera.eventMask = eventMask;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (KAInput.pInstance != null && KAInput.pInstance.IsTouchInput() && UICamera.currentTouch != null && UICamera.currentTouch.pressStarted && UICamera.selectedObject != null && useTouch && UICamera.mTooltipTime != 0f && UICamera.mTooltipTime < RealTime.time)
		{
			UICamera.mTooltip = UICamera.selectedObject;
			UICamera.ShowTooltip(UICamera.mTooltip);
		}
	}
}
