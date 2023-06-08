using System.Collections;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
	public enum TargetState
	{
		DRAG,
		ROTATE,
		SLIDE
	}

	public ObjectBase _Target;

	public Collider2D _FeedbackCollider;

	public float _RotationSpeed;

	public Transform _Feedback;

	public Transform _SliderFeedback;

	private MeshRenderer mFeedbackRenderer;

	private MeshRenderer mSliderFeedbackRenderer;

	public Texture _DragGreenTexture;

	public Texture _DragRedTexture;

	public Texture _SliderTexture;

	public Texture _RotationGreenTexture;

	public Texture _RotationRedTexture;

	private static FeedbackManager myInstance;

	private TargetState mTargetState;

	private float mOriginalSpeed;

	public static FeedbackManager pInstance => myInstance;

	private void Start()
	{
		myInstance = this;
		mFeedbackRenderer = _Feedback.GetComponentInChildren<MeshRenderer>();
		mSliderFeedbackRenderer = _SliderFeedback.GetComponentInChildren<MeshRenderer>();
	}

	private void Update()
	{
		if ((bool)_Target)
		{
			UpdateTexture();
			_Feedback.position = _Target.GetFeedbackPos();
			if (_Target.GetPivotObject() != null)
			{
				_SliderFeedback.position = _Target.GetSliderFeedbackPos();
			}
			_Feedback.Rotate(Vector3.forward * _RotationSpeed * Time.deltaTime, Space.World);
		}
	}

	public void Setup(ObjectBase target, TargetState state)
	{
		mTargetState = state;
		_Target = target;
		mFeedbackRenderer.sortingOrder = 4;
		_FeedbackCollider.enabled = true;
		StopCoroutine("Rescale");
		StartCoroutine(Rescale(_Feedback, target.GetFeedbackSize(), 0.2f));
		StartCoroutine(Rescale(_SliderFeedback, target.GetSliderFeedbackSize(), 0.2f));
	}

	public void Disable(float time)
	{
		_Target = null;
		mFeedbackRenderer.sortingOrder = 1;
		_FeedbackCollider.enabled = false;
		StopCoroutine("Rescale");
		StartCoroutine(Rescale(_Feedback, new Vector3(0f, 0f, 0f), time));
		StartCoroutine(Rescale(_SliderFeedback, new Vector3(0f, 0f, 0f), time));
	}

	public void RotateWith(Transform item)
	{
		mOriginalSpeed = _RotationSpeed;
		_RotationSpeed = 0f;
		_Feedback.parent = item;
	}

	public void RotateAlone()
	{
		_Feedback.parent = base.transform;
		_RotationSpeed = mOriginalSpeed;
	}

	public bool InRotation()
	{
		return mTargetState == TargetState.ROTATE;
	}

	public void ChangeState(TargetState state)
	{
		mTargetState = state;
	}

	private void UpdateTexture()
	{
		if (_Target.GetValidPos())
		{
			if (mTargetState == TargetState.DRAG)
			{
				mFeedbackRenderer.material.mainTexture = _DragGreenTexture;
				if (!mFeedbackRenderer.enabled)
				{
					mFeedbackRenderer.enabled = true;
				}
				if (mSliderFeedbackRenderer.enabled)
				{
					mSliderFeedbackRenderer.enabled = false;
				}
			}
			else if (mTargetState == TargetState.ROTATE)
			{
				mFeedbackRenderer.material.mainTexture = _RotationGreenTexture;
				if (mSliderFeedbackRenderer.enabled)
				{
					mSliderFeedbackRenderer.enabled = false;
				}
				if (!mFeedbackRenderer.enabled)
				{
					mFeedbackRenderer.enabled = true;
				}
			}
			else if (mTargetState == TargetState.SLIDE)
			{
				mSliderFeedbackRenderer.material.mainTexture = _SliderTexture;
				if (mFeedbackRenderer.enabled)
				{
					mFeedbackRenderer.enabled = false;
				}
				if (!mSliderFeedbackRenderer.enabled)
				{
					mSliderFeedbackRenderer.enabled = true;
				}
			}
		}
		else
		{
			if (mSliderFeedbackRenderer.enabled)
			{
				mSliderFeedbackRenderer.enabled = false;
			}
			if (!mFeedbackRenderer.enabled)
			{
				mFeedbackRenderer.enabled = true;
			}
			if (mTargetState == TargetState.DRAG)
			{
				mFeedbackRenderer.material.mainTexture = _DragRedTexture;
			}
			else
			{
				mFeedbackRenderer.material.mainTexture = _RotationRedTexture;
			}
		}
	}

	private IEnumerator Rescale(Transform obj, Vector3 endScale, float time)
	{
		Vector3 startScale = obj.localScale;
		float rate = 1f / time;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			obj.localScale = Vector3.Lerp(startScale, endScale, t);
			yield return new WaitForEndOfFrame();
		}
	}
}
