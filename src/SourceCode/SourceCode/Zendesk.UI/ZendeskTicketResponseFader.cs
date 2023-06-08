using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskTicketResponseFader : MonoBehaviour
{
	public float speed = 3f;

	private HorizontalLayoutGroup horizontalLayoutGroup;

	private CanvasGroup canvasGroup;

	public int start;

	public int end;

	public bool destroyable;

	public float waitSeconds = 4f;

	private bool isRunning;

	private float lerpTime;

	private float timeLeft;

	private void Start()
	{
		timeLeft = waitSeconds;
		lerpTime = 0f;
		isRunning = true;
		horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		horizontalLayoutGroup.padding.top = start;
	}

	private void Update()
	{
		if (isRunning)
		{
			if (destroyable)
			{
				timeLeft -= Time.fixedUnscaledDeltaTime;
				if (timeLeft < 0f)
				{
					Animate(0, start, stopRunning: true);
				}
				else
				{
					Animate(1, end, stopRunning: false);
				}
			}
			else
			{
				Animate(1, end, stopRunning: true);
			}
		}
		else if (destroyable)
		{
			ZendeskErrorUI.AfterDestroyToast(base.gameObject);
			Object.Destroy(base.gameObject);
		}
	}

	private void Animate(int endAlphaValue, int endPositionValue, bool stopRunning)
	{
		lerpTime += Time.fixedUnscaledDeltaTime * speed;
		canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, endAlphaValue, lerpTime);
		RectOffset rectOffset = new RectOffset(horizontalLayoutGroup.padding.left, horizontalLayoutGroup.padding.right, horizontalLayoutGroup.padding.top, horizontalLayoutGroup.padding.bottom);
		rectOffset.top = (int)Mathf.Lerp(horizontalLayoutGroup.padding.top, endPositionValue, lerpTime);
		horizontalLayoutGroup.padding = rectOffset;
		if (lerpTime >= 1f)
		{
			lerpTime = 0f;
			horizontalLayoutGroup.padding.top = endPositionValue;
			canvasGroup.alpha = endAlphaValue;
			isRunning = !stopRunning;
		}
	}
}
