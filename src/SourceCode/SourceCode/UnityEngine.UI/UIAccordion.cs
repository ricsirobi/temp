namespace UnityEngine.UI;

[RequireComponent(typeof(VerticalLayoutGroup))]
[RequireComponent(typeof(ContentSizeFitter))]
public class UIAccordion : MonoBehaviour
{
	public enum Transition
	{
		Instant,
		Tween
	}

	[SerializeField]
	private Transition m_Transition;

	[SerializeField]
	private float m_TransitionDuration = 0.3f;

	private Transform m_activeAccrodionElement;

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

	public Transform activeAccrodionElement
	{
		get
		{
			return m_activeAccrodionElement;
		}
		set
		{
			m_activeAccrodionElement = value;
		}
	}
}
