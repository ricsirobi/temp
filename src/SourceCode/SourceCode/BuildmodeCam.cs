using UnityEngine;

public class BuildmodeCam : MonoBehaviour
{
	public float _CamPaningSpeed = 0.05f;

	public float _CamHeight = 20f;

	private float mCamPanSpeed;

	public Vector3 _CamOrientation = Vector3.zero;

	protected bool mouseDown;

	protected Vector3 mFollowMePrevPos = Vector3.zero;

	protected GameObject mFollowMe;

	protected Vector3 mPreviousMousePosition = Vector3.zero;

	public GameObject pFollowMe
	{
		get
		{
			return mFollowMe;
		}
		set
		{
			mFollowMe = value;
			if (mFollowMe != null)
			{
				mFollowMePrevPos = mFollowMe.transform.position;
			}
		}
	}

	public virtual void Start()
	{
		mCamPanSpeed = _CamPaningSpeed * _CamHeight;
	}

	public virtual void OnEnable()
	{
		mCamPanSpeed = _CamPaningSpeed * _CamHeight;
	}

	public virtual void LateUpdate()
	{
		base.transform.eulerAngles = Vector3.Lerp(base.transform.eulerAngles, _CamOrientation, 0.05f);
		Vector3 position = base.transform.position;
		position.y = Mathf.Lerp(position.y, _CamHeight, 0.05f);
		base.transform.position = position;
		if (Input.GetMouseButtonDown(0))
		{
			mouseDown = true;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			mouseDown = false;
		}
		if (mFollowMe != null)
		{
			ProcessFollow();
		}
		else if (mouseDown)
		{
			ProcessPaning();
		}
		mPreviousMousePosition = Input.mousePosition;
	}

	protected virtual void ProcessFollow()
	{
		Vector3 position = mFollowMe.transform.position;
		if (!(position == mFollowMePrevPos))
		{
			Vector3 vector = mFollowMePrevPos - position;
			base.transform.position -= vector * 0.5f;
			mFollowMePrevPos = position;
		}
	}

	protected virtual void ProcessPaning()
	{
		if (!(mPreviousMousePosition == Input.mousePosition))
		{
			Vector3 vector = Input.mousePosition - mPreviousMousePosition;
			vector *= _CamPaningSpeed;
			vector.z = vector.y;
			vector.y = 0f;
			base.transform.position -= vector * mCamPanSpeed;
		}
	}
}
