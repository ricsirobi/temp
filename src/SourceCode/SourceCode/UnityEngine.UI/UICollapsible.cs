using System;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class UICollapsible : MonoBehaviour
{
	public enum Transition
	{
		Instant,
		Tween
	}

	public enum State
	{
		Collapsed,
		Expanded
	}

	[SerializeField]
	private float m_MinHeight = 18f;

	[SerializeField]
	private Transition m_Transition = Transition.Tween;

	[SerializeField]
	private float m_TransitionDuration = 0.3f;

	[SerializeField]
	private State m_CurrentState = State.Expanded;

	private RectTransform m_RectTransform;

	private Toggle m_Toggle;

	private LayoutElement m_LayoutElement;

	[NonSerialized]
	private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

	public Transition transition
	{
		get
		{
			return m_Transition;
		}
		set
		{
			m_Transition = value;
		}
	}

	public float transitionDuration
	{
		get
		{
			return m_TransitionDuration;
		}
		set
		{
			m_TransitionDuration = value;
		}
	}

	protected UICollapsible()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
	}

	protected virtual void Awake()
	{
		m_RectTransform = base.transform as RectTransform;
		m_LayoutElement = base.gameObject.GetComponent<LayoutElement>();
		m_Toggle = base.gameObject.GetComponent<Toggle>();
		if (m_Toggle != null)
		{
			m_Toggle.onValueChanged.AddListener(OnValueChanged);
		}
	}

	protected virtual void OnValidate()
	{
		LayoutElement component = base.gameObject.GetComponent<LayoutElement>();
		if (component != null)
		{
			component.preferredHeight = ((m_CurrentState == State.Expanded) ? (-1f) : m_MinHeight);
		}
	}

	public void OnValueChanged(bool state)
	{
		if (base.enabled && base.gameObject.activeInHierarchy)
		{
			TransitionToState(state ? State.Expanded : State.Collapsed);
		}
	}

	public void TransitionToState(State state)
	{
		if (m_LayoutElement == null)
		{
			return;
		}
		m_CurrentState = state;
		if (m_Transition == Transition.Instant)
		{
			m_LayoutElement.preferredHeight = ((state == State.Expanded) ? (-1f) : m_MinHeight);
		}
		else if (m_Transition == Transition.Tween)
		{
			if (state == State.Expanded)
			{
				StartTween(m_LayoutElement.preferredHeight, GetExpandedHeight());
			}
			else
			{
				StartTween((m_LayoutElement.preferredHeight == -1f) ? m_RectTransform.rect.height : m_LayoutElement.preferredHeight, m_MinHeight);
			}
		}
	}

	protected float GetExpandedHeight()
	{
		if (m_LayoutElement == null)
		{
			return m_MinHeight;
		}
		float preferredHeight = m_LayoutElement.preferredHeight;
		m_LayoutElement.preferredHeight = -1f;
		float preferredHeight2 = LayoutUtility.GetPreferredHeight(m_RectTransform);
		m_LayoutElement.preferredHeight = preferredHeight;
		return preferredHeight2;
	}

	protected void StartTween(float startFloat, float targetFloat)
	{
		FloatTween floatTween = default(FloatTween);
		floatTween.duration = m_TransitionDuration;
		floatTween.startFloat = startFloat;
		floatTween.targetFloat = targetFloat;
		FloatTween info = floatTween;
		info.AddOnChangedCallback(SetHeight);
		info.ignoreTimeScale = true;
		m_FloatTweenRunner.StartTween(info);
	}

	protected void SetHeight(float height)
	{
		if (!(m_LayoutElement == null))
		{
			m_LayoutElement.preferredHeight = height;
		}
	}
}
