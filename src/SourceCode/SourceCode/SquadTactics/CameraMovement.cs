using System;
using System.Collections;
using UnityEngine;

namespace SquadTactics;

public class CameraMovement : KAMonoBase
{
	[HideInInspector]
	public bool pForceStopCameraMovement;

	public float _MovementSpeed = 20f;

	public float _MinFov = 7f;

	public float _MaxFov = 20f;

	public float _FocusSpeed = 25f;

	public float _MaxFocusTime = 1.5f;

	public float _MovementEdgeOffset = 20f;

	public float _DragSpeed = 10f;

	public Transform _FollowTarget;

	public float _CameraTransitionTime = 0.25f;

	private Vector3 mFollowOffset;

	public float _Sensitivity = 3f;

	[NonSerialized]
	public bool pAllowDrag = true;

	private float mTouchManagerSensitivityOnStart;

	private bool mIsForceStopFocusAllowed;

	private static CameraMovement mInstance;

	public bool pIsCameraMoving { get; private set; }

	public static CameraMovement pInstance => mInstance;

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		TouchManager.OnDragEvent = (OnDrag)Delegate.Combine(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		if (TouchManager.pInstance != null)
		{
			mTouchManagerSensitivityOnStart = TouchManager.pInstance._TouchSensitivity;
			TouchManager.pInstance._TouchSensitivity = 0f;
		}
	}

	private void OnDestroy()
	{
		TouchManager.OnDragEvent = (OnDrag)Delegate.Remove(TouchManager.OnDragEvent, new OnDrag(OnDrag));
		if (TouchManager.pInstance != null)
		{
			TouchManager.pInstance._TouchSensitivity = mTouchManagerSensitivityOnStart;
		}
	}

	public bool ForceStopAutoFocus()
	{
		if (pIsCameraMoving)
		{
			if (!mIsForceStopFocusAllowed)
			{
				return false;
			}
			TweenPosition component = GetComponent<TweenPosition>();
			if (component != null)
			{
				component.Stop();
			}
		}
		return true;
	}

	public bool OnDrag(Vector2 inNewPosition, Vector2 inOldPosition, int inFingerID)
	{
		if (pForceStopCameraMovement || KAUI.GetGlobalMouseOverItem() != null || !pAllowDrag)
		{
			return false;
		}
		Vector2 vector = inNewPosition - inOldPosition;
		Vector3 offset = new Vector3(vector.x * _DragSpeed * Time.deltaTime, 0f, (0f - vector.y) * _DragSpeed * Time.deltaTime);
		UpdatePosition(offset);
		return false;
	}

	private void Update()
	{
		if (RsResourceManager.pLevelLoadingScreen || GameManager.pInstance._GameState == GameManager.GameState.ENEMY)
		{
			return;
		}
		if (KAInput.GetAxis("CameraZoom") != 0f)
		{
			if (!pForceStopCameraMovement && ForceStopAutoFocus())
			{
				float fieldOfView = Camera.main.fieldOfView;
				fieldOfView -= KAInput.GetAxis("CameraZoom") * _Sensitivity;
				fieldOfView = Mathf.Clamp(fieldOfView, _MinFov, _MaxFov);
				Camera.main.fieldOfView = fieldOfView;
			}
		}
		else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
		{
			UpdatePosition(base.transform.right * _MovementSpeed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
		{
			UpdatePosition(-base.transform.right * _MovementSpeed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
		{
			UpdatePosition(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized * _MovementSpeed * Time.deltaTime);
		}
		else if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
		{
			UpdatePosition(-new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized * _MovementSpeed * Time.deltaTime);
		}
		else if (Input.mousePosition.x > (float)Screen.width - _MovementEdgeOffset && Input.mousePosition.x < (float)Screen.width)
		{
			UpdatePosition(base.transform.right * _MovementSpeed * Time.deltaTime, ignoreOverUI: false);
		}
		else if (Input.mousePosition.x < _MovementEdgeOffset && Input.mousePosition.x > 0f)
		{
			UpdatePosition(-base.transform.right * _MovementSpeed * Time.deltaTime, ignoreOverUI: false);
		}
		else if (Input.mousePosition.y > (float)Screen.height - _MovementEdgeOffset && Input.mousePosition.y < (float)Screen.height)
		{
			UpdatePosition(new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized * _MovementSpeed * Time.deltaTime, ignoreOverUI: false);
		}
		else if (Input.mousePosition.y < _MovementEdgeOffset && Input.mousePosition.y > 0f)
		{
			UpdatePosition(-new Vector3(base.transform.forward.x, 0f, base.transform.forward.z).normalized * _MovementSpeed * Time.deltaTime, ignoreOverUI: false);
		}
	}

	private void LateUpdate()
	{
		if (_FollowTarget != null)
		{
			base.transform.position = _FollowTarget.position + mFollowOffset;
		}
	}

	public void SetFollowTarget(Transform target)
	{
		_FollowTarget = target;
		if (!(target == null))
		{
			Vector3 pos = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 1f);
			if (Physics.Raycast(Camera.main.ScreenPointToRay(pos), out var hitInfo, float.PositiveInfinity, GameManager.pInstance._GridLayerMask))
			{
				Vector3 vector = target.position - hitInfo.point;
				vector.y = 0f;
				Vector3 position = base.transform.position + vector;
				base.transform.position = position;
				mFollowOffset = base.transform.position - target.position;
			}
		}
	}

	private void UpdatePosition(Vector3 offset, bool ignoreOverUI = true)
	{
		if (!pForceStopCameraMovement && (ignoreOverUI || !(KAUI.GetGlobalMouseOverItem() != null)) && ForceStopAutoFocus())
		{
			Vector3 vector = base.transform.position + offset;
			if (GameManager.pInstance._Boundary != null && GameManager.pInstance._Boundary.bounds.Contains(vector))
			{
				base.transform.position = vector;
			}
		}
	}

	public void UpdateCameraFocus(Vector3 targetPos, bool isForceStopAllowed = false, bool overrideCurrentMove = false)
	{
		if (_FollowTarget != null)
		{
			_FollowTarget = null;
		}
		if ((pIsCameraMoving && !overrideCurrentMove) || InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			return;
		}
		Vector3 pos = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 1f);
		if (!Physics.Raycast(Camera.main.ScreenPointToRay(pos), out var hitInfo, float.PositiveInfinity, GameManager.pInstance._GridLayerMask))
		{
			return;
		}
		Vector3 vector = targetPos - hitInfo.point;
		vector.y = 0f;
		Vector3 vector2 = base.transform.position + vector;
		if (GameManager.pInstance._Boundary != null && !GameManager.pInstance._Boundary.bounds.Contains(vector2))
		{
			Vector3 center = GameManager.pInstance._Boundary.bounds.center;
			Vector3 extents = GameManager.pInstance._Boundary.bounds.extents;
			if (vector2.x < center.x - extents.x)
			{
				vector2.x = center.x - extents.x;
			}
			else if (vector2.x > center.x + extents.x)
			{
				vector2.x = center.x + extents.x;
			}
			if (vector2.z < center.z - extents.z)
			{
				vector2.z = center.z - extents.z;
			}
			else if (vector2.z > center.z + extents.z)
			{
				vector2.z = center.z + extents.z;
			}
		}
		pIsCameraMoving = true;
		mIsForceStopFocusAllowed = isForceStopAllowed;
		StartCoroutine(MoveCamera(base.gameObject, _CameraTransitionTime, vector2));
	}

	private IEnumerator MoveCamera(GameObject camera, float moveTime, Vector3 targetPos)
	{
		float currentTime = 0f;
		Vector3 startPos = camera.transform.position;
		while (currentTime <= moveTime)
		{
			if (currentTime > moveTime)
			{
				currentTime = moveTime;
			}
			float num = currentTime / moveTime;
			num = num * num * (3f - 2f * num);
			camera.transform.position = Vector3.Lerp(startPos, targetPos, num);
			currentTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		TweenPositionDone();
		yield return null;
	}

	private void TweenPositionDone()
	{
		pIsCameraMoving = false;
		mIsForceStopFocusAllowed = false;
	}
}
