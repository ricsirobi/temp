using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zendesk.UI;

public class DragonsZendeskTicketResponseFader : MonoBehaviour
{
	public int _StartPosition;

	public int _EndPosition;

	[Tooltip("1 is default, set higher to slow down animation, set lower to speed animation up")]
	public float _AnimationTime = 1f;

	[Tooltip("Time to wait until the toast fades back up")]
	public float _ReverseAnimationDelay = 1f;

	[Tooltip("If true, this toast will destroy itself. If false, it will remain on the screen")]
	public bool _Destroyable;

	private HorizontalLayoutGroup mHorizontalLayoutGroup;

	private CanvasGroup mCanvasGroup;

	private void Start()
	{
		mHorizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
		mCanvasGroup = GetComponent<CanvasGroup>();
		mCanvasGroup.alpha = 0f;
		mHorizontalLayoutGroup.padding.top = _StartPosition;
		StartCoroutine(Animate(1, _EndPosition, _AnimationTime));
	}

	private IEnumerator Animate(int endAlphaValue, int endPositionValue, float timeUntilAnimationCompletes)
	{
		float currentAnimationTime = 0f;
		float oldTimeScale = Time.timeScale;
		int initialPaddingTop = mHorizontalLayoutGroup.padding.top;
		float startAlpha = mCanvasGroup.alpha;
		Time.timeScale = 1f;
		while (currentAnimationTime <= timeUntilAnimationCompletes)
		{
			currentAnimationTime += Time.deltaTime;
			Lerp(startAlpha, endAlphaValue, currentAnimationTime, _AnimationTime, initialPaddingTop, endPositionValue);
			yield return new WaitForEndOfFrame();
		}
		yield return new WaitForSeconds(_ReverseAnimationDelay);
		while (currentAnimationTime >= 0f)
		{
			currentAnimationTime -= Time.deltaTime;
			Lerp(startAlpha, endAlphaValue, currentAnimationTime, _AnimationTime, initialPaddingTop, endPositionValue);
			yield return new WaitForEndOfFrame();
		}
		Time.timeScale = oldTimeScale;
		if (_Destroyable)
		{
			ZendeskErrorUI.AfterDestroyToast(base.gameObject);
			Object.Destroy(base.gameObject);
		}
	}

	private void Lerp(float startAlpha, float endAlphaValue, float currentAnimationTime, float animationSpeed, int initialPaddingTop, int endPositionValue)
	{
		mCanvasGroup.alpha = Mathf.Lerp(startAlpha, endAlphaValue, currentAnimationTime / animationSpeed);
		RectOffset rectOffset = new RectOffset(mHorizontalLayoutGroup.padding.left, mHorizontalLayoutGroup.padding.right, mHorizontalLayoutGroup.padding.top, mHorizontalLayoutGroup.padding.bottom);
		rectOffset.top = (int)Mathf.Lerp(initialPaddingTop, endPositionValue, currentAnimationTime / animationSpeed);
		mHorizontalLayoutGroup.padding = rectOffset;
	}
}
