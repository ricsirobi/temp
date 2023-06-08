using UnityEngine;

public class InputManager : MonoBehaviour
{
	private enum InputState
	{
		scrolling,
		moving,
		sliding,
		rotating,
		waitingForInput
	}

	public LayerMask mask = -1;

	public bool useTouch = true;

	private static InputManager mInstance;

	private InputState inputState = InputState.waitingForInput;

	private RaycastHit2D hit;

	private Transform selectedItem;

	private ObjectBase selectedObject;

	private ObjectBase prevSelectedObject;

	private float scrollStartingPos;

	private bool itemSelectionValid;

	private bool hasFeedback;

	private Vector3 inputPos;

	private Vector3 offset;

	private Vector3 startingRot;

	private Vector2 startingVector;

	private Vector2 currentVector;

	private Vector2 lastPosition;

	private bool fromToolBox;

	private bool mItemSelected;

	public ObjectBase pSelectedObject
	{
		get
		{
			return selectedObject;
		}
		set
		{
			selectedObject = null;
		}
	}

	public bool pItemSlected
	{
		get
		{
			return mItemSelected;
		}
		set
		{
			mItemSelected = value;
		}
	}

	public static InputManager pInstance => mInstance;

	private void Start()
	{
		mInstance = this;
		offset = Vector3.zero;
	}

	private void Update()
	{
		if (useTouch)
		{
			TouchControls();
		}
		else
		{
			MouseControls();
		}
	}

	private void MouseControls()
	{
		inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		inputPos.z = 0f;
		hit = Physics2D.Raycast(inputPos, new Vector2(0f, 0f), 0.1f, mask);
		if (inputState == InputState.waitingForInput)
		{
			ScanForInput();
		}
		else if (inputState == InputState.scrolling)
		{
			ScrollToolbox();
			if (Input.GetMouseButtonUp(0))
			{
				itemSelectionValid = false;
				inputState = InputState.waitingForInput;
			}
		}
		else if (inputState == InputState.moving)
		{
			MoveItem();
			if (Input.GetMouseButtonUp(0))
			{
				DropItem();
			}
		}
		else if (inputState == InputState.rotating)
		{
			RotateItem();
			if (Input.GetMouseButtonUp(0))
			{
				FinaliseRotation();
			}
		}
		else if (inputState == InputState.sliding)
		{
			SlideItem();
			if (Input.GetMouseButtonUp(0))
			{
				FinaliseSlide();
			}
		}
	}

	private void TouchControls()
	{
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			inputPos = Camera.main.ScreenToWorldPoint(Input.touches[0].position);
			inputPos.z = 0f;
			hit = Physics2D.Raycast(inputPos, new Vector2(0f, 0f), 0.1f, mask);
			if (inputState == InputState.waitingForInput)
			{
				ScanForInput();
			}
			else if (inputState == InputState.scrolling)
			{
				ScrollToolbox();
				if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					itemSelectionValid = false;
					inputState = InputState.waitingForInput;
				}
			}
			else if (inputState == InputState.moving)
			{
				MoveItem();
				if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					DropItem();
				}
			}
			else if (inputState == InputState.rotating)
			{
				RotateItem();
				if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
				{
					FinaliseRotation();
				}
			}
			else if (inputState == InputState.sliding)
			{
				SlideItem();
				if (Input.GetMouseButtonUp(0))
				{
					FinaliseRotation();
				}
			}
		}
	}

	private void ScanForInput()
	{
		if (!HasInput())
		{
			return;
		}
		if (hit.collider != null)
		{
			ObjectBase component = hit.transform.GetComponent<ObjectBase>();
			if (component == null && hit.transform.parent != null)
			{
				component = hit.transform.parent.GetComponent<ObjectBase>();
			}
			if (component != null)
			{
				PrepareToDrag(hit.transform, fromTheToolbox: false);
			}
			else if (hit.transform.name == "Feedback")
			{
				PrepareToRotate();
			}
			else if (hit.transform.name == "SliderFeedback")
			{
				PrepareToSlide();
			}
		}
		else if (hasFeedback || (selectedObject != null && !selectedObject.canRotate))
		{
			selectedObject = null;
			HideFeedback();
		}
	}

	public void ProcessSelectedObject(Transform t)
	{
		PrepareToDrag(t, fromTheToolbox: true);
	}

	private void PrepareScrolling(Transform t)
	{
		if (hit.transform.name != "background" && hit.transform.name != "backgroundCap")
		{
			selectedItem = hit.transform;
			itemSelectionValid = true;
		}
		scrollStartingPos = inputPos.x;
		inputState = InputState.scrolling;
	}

	private void ScrollToolbox()
	{
		if (itemSelectionValid)
		{
			if (Mathf.Abs(inputPos.x - scrollStartingPos) < 0.5f && inputPos.y > -2.8f)
			{
				PrepareToDrag(selectedItem, fromTheToolbox: true);
			}
			else if (Mathf.Abs(inputPos.x - scrollStartingPos) > 0.5f)
			{
				itemSelectionValid = false;
				scrollStartingPos = inputPos.x;
			}
		}
	}

	private void PrepareToDrag(Transform item, bool fromTheToolbox)
	{
		if (IMGLevelManager.pInstance.InPlayMode() || IMGLevelManager.pInstance.pGamePaused)
		{
			return;
		}
		if (hasFeedback && selectedObject != null)
		{
			HideFeedback();
		}
		if (fromTheToolbox)
		{
			fromToolBox = fromTheToolbox;
			selectedObject = item.GetComponent<ObjectBase>();
			selectedObject.DragMode();
			selectedObject.PlayPickupAnimation();
			FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.DRAG);
			hasFeedback = true;
			ChangeSortingOrderBy(selectedObject.gameObject, 3);
			inputState = InputState.moving;
		}
		else if (CanDragged(item))
		{
			fromToolBox = false;
			selectedObject = GetParent(item);
			selectedObject.DragMode();
			offset = new Vector3(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y, 0f);
			FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.DRAG);
			hasFeedback = true;
			ChangeSortingOrderBy(selectedObject.gameObject, 3);
			inputState = InputState.moving;
			lastPosition = selectedObject.transform.position;
			if (selectedObject != null)
			{
				pItemSlected = true;
			}
		}
		else
		{
			selectedObject = null;
		}
	}

	private void MoveItem()
	{
		if (new Rect(0f, 0f, Screen.width, Screen.height).Contains(Input.mousePosition))
		{
			selectedObject.transform.position = new Vector3(inputPos.x - offset.x, inputPos.y - offset.y, 0f);
		}
	}

	private void SlideItem()
	{
		Rect rect = new Rect(0f, 0f, Screen.width, Screen.height);
		Transform pivotObject = selectedObject.GetPivotObject();
		Collider2D collider2D = selectedObject.GetCollider2D();
		if (!(collider2D == null))
		{
			float num = collider2D.bounds.size.x / 2f;
			Vector3 vector = Vector3.zero;
			if (rect.Contains(Input.mousePosition))
			{
				vector = new Vector3(inputPos.x - offset.x, pivotObject.transform.position.y, pivotObject.transform.position.z);
			}
			pivotObject.transform.position = new Vector3(Mathf.Clamp(vector.x, selectedObject.transform.position.x - num, selectedObject.transform.position.x + num), pivotObject.transform.position.y, pivotObject.transform.position.z);
		}
	}

	private void DropItem()
	{
		ChangeSortingOrderBy(selectedObject.gameObject, -3);
		if (selectedObject.canRotate)
		{
			FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.ROTATE);
		}
		else if (selectedObject.canSlide)
		{
			FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.SLIDE);
		}
		pItemSlected = false;
		if (selectedObject.GetValidPos())
		{
			selectedObject.Dropped();
			selectedObject.Setup();
			IMGLevelManager.pInstance.AddItemFromTool(selectedObject.GetComponent<ObjectBase>());
			IMGLevelManager.pInstance.AddItem(selectedObject.GetComponent<ObjectBase>());
			if (selectedObject.canRotate)
			{
				FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.ROTATE);
			}
			else if (selectedObject.canSlide)
			{
				FeedbackManager.pInstance.Setup(selectedObject, FeedbackManager.TargetState.SLIDE);
			}
			else
			{
				FeedbackManager.pInstance.Disable(0f);
				HideFeedback();
				if (prevSelectedObject != selectedObject)
				{
					prevSelectedObject = selectedObject;
					selectedObject = null;
				}
			}
		}
		else if (!fromToolBox)
		{
			selectedObject.transform.position = lastPosition;
			selectedObject.SetValidPos(newValue: true);
		}
		else if (fromToolBox)
		{
			IMGLevelManager.pInstance.RemoveItem(selectedObject.GetComponent<ObjectBase>());
			FeedbackManager.pInstance.Disable(0f);
			selectedObject = null;
			HideFeedback();
		}
		offset = Vector3.zero;
		selectedItem = null;
		itemSelectionValid = false;
		inputState = InputState.waitingForInput;
	}

	private void PrepareToRotate()
	{
		if (selectedObject == null)
		{
			UtDebug.Log("ERROR! InputManager.PrepareToRotate() selectedObject = null");
		}
		else if (!IMGLevelManager.pInstance.InPlayMode() && FeedbackManager.pInstance.InRotation())
		{
			selectedObject.DragMode();
			startingVector = new Vector2(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y);
			startingVector.Normalize();
			startingRot = selectedObject.transform.eulerAngles;
			FeedbackManager.pInstance.RotateWith(selectedObject.transform);
			ChangeSortingOrderBy(selectedObject.gameObject, 3);
			inputState = InputState.rotating;
		}
	}

	private void PrepareToSlide()
	{
		if (selectedObject == null)
		{
			UtDebug.Log("ERROR! InputManager.PrepareToSlide() selectedObject = null");
		}
		else if (selectedObject == null && selectedObject.GetPivotObject() == null)
		{
			UtDebug.Log("ERROR! InputManager.PrepareToSlide() Pivot Object = null");
		}
		else if (!IMGLevelManager.pInstance.InPlayMode())
		{
			offset = new Vector3(inputPos.x - selectedObject.GetPivotObject().transform.position.x, 0f, 0f);
			ChangeSortingOrderBy(selectedObject.gameObject, 3);
			inputState = InputState.sliding;
		}
	}

	private void RotateItem()
	{
		Vector3 vector = new Vector2(inputPos.x - selectedObject.transform.position.x, inputPos.y - selectedObject.transform.position.y);
		vector.Normalize();
		Vector3 eulerAngles = selectedObject.transform.eulerAngles;
		float num = Vector3.Angle(startingVector, vector);
		if (Vector3.Cross(startingVector, vector).z < 0f)
		{
			eulerAngles.z = startingRot.z - num;
		}
		else
		{
			eulerAngles.z = startingRot.z + num;
		}
		selectedObject.transform.eulerAngles = eulerAngles;
	}

	private void FinaliseRotation()
	{
		ChangeSortingOrderBy(selectedObject.gameObject, -3);
		FeedbackManager.pInstance.RotateAlone();
		selectedObject.RotationEnded();
		IMGLevelManager.pInstance.AddItem(selectedObject.GetComponent<ObjectBase>());
		offset = Vector3.zero;
		selectedItem = null;
		itemSelectionValid = false;
		inputState = InputState.waitingForInput;
	}

	private void FinaliseSlide()
	{
		IMGLevelManager.pInstance.AddItem(selectedObject.GetComponent<ObjectBase>());
		offset = Vector3.zero;
		selectedItem = null;
		itemSelectionValid = false;
		inputState = InputState.waitingForInput;
	}

	public void HideFeedback(float time = 0.2f)
	{
		FeedbackManager.pInstance.Disable(time);
		hasFeedback = false;
	}

	private bool HasInput()
	{
		if (useTouch)
		{
			return Input.touchCount > 0;
		}
		return Input.GetMouseButtonDown(0);
	}

	private bool CanDragged(Transform item)
	{
		while (item != null)
		{
			if (item.name.Contains("(Clone)") && !IMGLevelManager.pInstance.pGoalReached)
			{
				return true;
			}
			item = item.parent;
		}
		return false;
	}

	private void ChangeSortingOrderBy(GameObject obj, int by)
	{
		if ((bool)obj.GetComponent<SpriteRenderer>())
		{
			obj.GetComponent<SpriteRenderer>().sortingOrder += by;
		}
		foreach (Transform item in obj.transform)
		{
			ChangeSortingOrderBy(item.gameObject, by);
		}
	}

	private ObjectBase GetParent(Transform item)
	{
		while (item.GetComponent<ObjectBase>() == null)
		{
			item = item.parent;
		}
		return item.GetComponent<ObjectBase>();
	}

	public void RemoveSelectedLevelItem()
	{
		if (selectedObject != null && selectedObject.GetValidPos())
		{
			IMGLevelManager.pInstance.RemoveItem(selectedObject.GetComponent<ObjectBase>());
			FeedbackManager.pInstance.Disable(0f);
			selectedObject = null;
		}
		offset = Vector3.zero;
		selectedItem = null;
		itemSelectionValid = false;
		pItemSlected = false;
		inputState = InputState.waitingForInput;
		if (hasFeedback)
		{
			HideFeedback();
		}
	}

	public bool IsObjectSelected()
	{
		return selectedObject != null;
	}
}
