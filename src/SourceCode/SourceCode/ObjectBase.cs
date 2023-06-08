using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBase : KAMonoBase
{
	public float animationStartScale;

	public float feedbackSize;

	public float _SliderFeedbackSize;

	public Vector3 _SliderFeedBackPositionOffset;

	public bool canRotate;

	public bool canSlide;

	public GoalManager.ObjectProperty _ObjectProperty;

	public ParticleSystem _ParticleHighlight;

	[Range(-1f, 1f)]
	public float _PivotPositionSlider;

	[Range(-1f, 1f)]
	public float _PivotRotationSlider;

	public float _PivotAngleLimit;

	public Transform _Pivot;

	public Transform feedbackPosition;

	public Collider2D[] childColliders;

	public Collider2D inputCollider;

	protected Vector3 originalPos;

	protected Vector3 originalRot;

	protected Vector3 lastPos;

	protected Vector3 lastRot;

	protected PhysicsObject physicsScript;

	protected bool validPos;

	protected List<Collider2D> colliders = new List<Collider2D>();

	private int childCollidedCount;

	protected virtual void Awake()
	{
		colliders = new List<Collider2D>();
		Collider2D[] components = GetComponents<Collider2D>();
		foreach (Collider2D item in components)
		{
			colliders.Add(item);
		}
		colliders.AddRange(childColliders);
	}

	private void OnTriggerStay2D(Collider2D other)
	{
		validPos = false;
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		validPos = true;
	}

	private void OnChildTriggerEnter2D()
	{
		childCollidedCount++;
		validPos = false;
	}

	private void OnChildTriggerExit2D()
	{
		childCollidedCount--;
		if (childCollidedCount <= 0)
		{
			childCollidedCount = 0;
			validPos = true;
		}
	}

	public virtual void Setup()
	{
		originalPos = base.transform.position;
		originalRot = base.transform.eulerAngles;
		lastPos = originalPos;
		lastRot = originalRot;
		if (_Pivot != null)
		{
			HingeJoint2D component = _Pivot.GetComponent<HingeJoint2D>();
			if (component != null && _PivotAngleLimit > 0f)
			{
				component.useLimits = true;
				JointAngleLimits2D limits = default(JointAngleLimits2D);
				limits.min = 0f - _PivotAngleLimit;
				limits.max = _PivotAngleLimit;
				component.limits = limits;
			}
		}
		physicsScript = GetComponent<PhysicsObject>();
	}

	public virtual void Enable()
	{
		if ((bool)physicsScript)
		{
			physicsScript.Enable();
		}
		else if ((bool)base.rigidbody2D)
		{
			base.rigidbody2D.isKinematic = true;
		}
		foreach (Collider2D collider in colliders)
		{
			collider.isTrigger = false;
		}
		if (_ParticleHighlight != null)
		{
			_ParticleHighlight.Stop();
		}
	}

	public virtual void Reset()
	{
		if ((bool)physicsScript)
		{
			physicsScript.Reset();
		}
		else if ((bool)base.rigidbody2D)
		{
			base.rigidbody2D.isKinematic = false;
		}
		foreach (Collider2D collider in colliders)
		{
			collider.isTrigger = true;
		}
		base.transform.position = lastPos;
		base.transform.eulerAngles = lastRot;
		if (_ParticleHighlight != null)
		{
			_ParticleHighlight.Play();
		}
	}

	public virtual void Restart()
	{
		base.transform.position = originalPos;
		base.transform.eulerAngles = originalRot;
	}

	public void DragMode()
	{
		base.transform.parent = null;
		validPos = true;
		if ((bool)inputCollider)
		{
			inputCollider.gameObject.SetActive(value: false);
		}
		foreach (Collider2D collider in colliders)
		{
			collider.isTrigger = true;
		}
	}

	public Transform GetPivotObject()
	{
		return _Pivot;
	}

	public virtual Collider2D GetCollider2D()
	{
		return base.collider2D;
	}

	public void Dropped()
	{
		lastPos = base.transform.position;
		if ((bool)inputCollider)
		{
			inputCollider.gameObject.SetActive(value: true);
		}
		foreach (Collider2D collider in colliders)
		{
			collider.isTrigger = false;
		}
	}

	public void RotationEnded()
	{
		if (validPos)
		{
			lastRot = base.transform.eulerAngles;
		}
		else
		{
			base.transform.eulerAngles = lastRot;
		}
		validPos = true;
		if ((bool)inputCollider)
		{
			inputCollider.gameObject.SetActive(value: true);
		}
		foreach (Collider2D collider in colliders)
		{
			collider.isTrigger = false;
		}
	}

	public void SetValidPos(bool newValue)
	{
		validPos = newValue;
	}

	public bool GetValidPos()
	{
		return validPos;
	}

	public Vector3 GetFeedbackSize()
	{
		return new Vector3(feedbackSize, feedbackSize, 1f);
	}

	public Vector3 GetSliderFeedbackSize()
	{
		return new Vector3(_SliderFeedbackSize, _SliderFeedbackSize, 1f);
	}

	public Vector3 GetFeedbackPos()
	{
		if (feedbackPosition == null)
		{
			return base.transform.position;
		}
		return feedbackPosition.transform.position;
	}

	public Vector3 GetSliderFeedbackPos()
	{
		return _Pivot.position + _SliderFeedBackPositionOffset;
	}

	public void PlayPickupAnimation()
	{
		StartCoroutine(Rescale(0.2f));
	}

	protected IEnumerator Rescale(float time)
	{
		Vector3 startScale = new Vector3(animationStartScale, animationStartScale, 1f);
		Vector3 endScale = new Vector3(1f, 1f, 1f);
		float rate = 1f / time;
		float t = 0f;
		while (t < 1f)
		{
			t += Time.deltaTime * rate;
			base.transform.localScale = Vector3.Lerp(startScale, endScale, t);
			yield return new WaitForEndOfFrame();
		}
	}
}
