using System;
using UnityEngine.UI.Tweens;

namespace UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(LayoutElement))]
public class UIAccordionElement : Toggle
{
	[SerializeField]
	private float m_MinHeight = 18f;

	private UIAccordion m_Accordion;

	private RectTransform m_RectTransform;

	private LayoutElement m_LayoutElement;

	[NonSerialized]
	private readonly TweenRunner<FloatTween> m_FloatTweenRunner;

	protected UIAccordionElement()
	{
		if (m_FloatTweenRunner == null)
		{
			m_FloatTweenRunner = new TweenRunner<FloatTween>();
		}
		m_FloatTweenRunner.Init(this);
	}

	protected override void Awake()
	{
		base.Awake();
		base.transition = Transition.None;
		toggleTransition = ToggleTransition.None;
		m_Accordion = base.gameObject.GetComponentInParent<UIAccordion>();
		m_RectTransform = base.transform as RectTransform;
		m_LayoutElement = base.gameObject.GetComponent<LayoutElement>();
		if (base.isOn)
		{
			m_Accordion.activeAccrodionElement = m_RectTransform;
		}
		onValueChanged.AddListener(OnValueChanged);
	}

	protected void OnValidate()
	{
		if (base.group == null)
		{
			ToggleGroup componentInParent = GetComponentInParent<ToggleGroup>();
			if (componentInParent != null)
			{
				base.group = componentInParent;
			}
		}
		LayoutElement component = base.gameObject.GetComponent<LayoutElement>();
		if (component != null)
		{
			if (base.isOn)
			{
				component.preferredHeight = -1f;
			}
			else
			{
				component.preferredHeight = m_MinHeight;
			}
		}
	}

	public void OnValueChanged(bool state)
	{
		if (m_LayoutElement == null)
		{
			return;
		}
		m_Accordion.activeAccrodionElement = m_RectTransform;
		switch ((m_Accordion != null) ? m_Accordion.transition : UIAccordion.Transition.Instant)
		{
		case UIAccordion.Transition.Instant:
			if (state)
			{
				m_LayoutElement.preferredHeight = -1f;
			}
			else
			{
				m_LayoutElement.preferredHeight = m_MinHeight;
			}
			break;
		case UIAccordion.Transition.Tween:
			if (state)
			{
				StartTween(m_MinHeight, GetExpandedHeight());
			}
			else
			{
				StartTween(m_RectTransform.rect.height, m_MinHeight);
			}
			break;
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
		float duration = ((m_Accordion != null) ? m_Accordion.transitionDuration : 0.3f);
		FloatTween floatTween = default(FloatTween);
		floatTween.duration = duration;
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
