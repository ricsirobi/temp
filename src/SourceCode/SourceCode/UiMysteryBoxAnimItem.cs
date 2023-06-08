using UnityEngine;

public class UiMysteryBoxAnimItem : MonoBehaviour
{
	public UiMysteryBoxMenu _UiMysteryBoxMenu;

	private Vector3 mOriginalScale = Vector3.one;

	private Vector3 mOriginalPos = Vector3.zero;

	private Vector3 mOrginalShufflePos = Vector3.zero;

	private float mShuffleTimer;

	private void Start()
	{
		mOriginalScale = base.transform.localScale;
	}

	public void CacheOriginalPos()
	{
		mOriginalPos = GetComponent<KAWidget>().GetPosition();
	}

	public void MoveToOriginalPos(float moveTimer)
	{
		KAWidget component = GetComponent<KAWidget>();
		component.MoveTo(new Vector2(mOriginalPos.x, mOriginalPos.y), moveTimer);
		component.OnMoveToDone += ReachedOrgPosition;
	}

	public void ReachedOrgPosition(Object item)
	{
		KAWidget obj = (KAWidget)item;
		Vector3 localPosition = obj.gameObject.transform.localPosition;
		obj.gameObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, mOriginalPos.z);
		obj.SetInteractive(isInteractive: true);
		_UiMysteryBoxMenu.ReachedOrgPosition(this);
	}

	public void RotatedToFront()
	{
	}

	public void RotatedToBack()
	{
	}

	public void RotatedToFrontHalf()
	{
		KAWidget component = GetComponent<KAWidget>();
		component.FindChildItem("CardInfo").FindChildItem("CardFront").SetVisibility(inVisible: true);
		component.FindChildItem("CardInfo").FindChildItem("CardBack").SetVisibility(inVisible: false);
		TweenScale.Begin(base.gameObject, _UiMysteryBoxMenu._MysteryBoxUI._AnimTimes._PrizeRotate, mOriginalScale);
		UITweener component2 = base.gameObject.GetComponent<UITweener>();
		component2.eventReceiver = base.gameObject;
		component2.callWhenFinished = "RotatedToFront";
	}

	public void RotatedToBackHalf()
	{
		KAWidget component = GetComponent<KAWidget>();
		component.FindChildItem("CardInfo").FindChildItem("CardFront").SetVisibility(inVisible: false);
		component.FindChildItem("CardInfo").FindChildItem("CardBack").SetVisibility(inVisible: true);
		TweenScale.Begin(base.gameObject, _UiMysteryBoxMenu._MysteryBoxUI._AnimTimes._PrizeRotate, mOriginalScale);
		UITweener component2 = base.gameObject.GetComponent<UITweener>();
		component2.eventReceiver = base.gameObject;
		component2.callWhenFinished = "RotatedToBack";
	}

	public void ShuffleCard(float shuffleTimer)
	{
		float num = 40f;
		mShuffleTimer = shuffleTimer;
		KAWidget component = GetComponent<KAWidget>();
		mOrginalShufflePos = component.GetPosition();
		float num2 = Random.Range(0f - num, num);
		float num3 = Random.Range(0f - num, num);
		float num4 = Random.Range(0.1f, mShuffleTimer);
		component.OnMoveToDone += ShuffleCardBack;
		component.MoveTo(new Vector2(mOrginalShufflePos.x + num2, mOrginalShufflePos.y + num3), num4 / 2f);
	}

	public void ShuffleCardBack(Object item)
	{
		KAWidget obj = (KAWidget)item;
		float num = Random.Range(0.1f, mShuffleTimer);
		obj.OnMoveToDone += ShuffleCardBackDone;
		obj.MoveTo(new Vector2(mOrginalShufflePos.x, mOrginalShufflePos.y), num / 2f);
	}

	public void ShuffleCardBackDone(Object item)
	{
		_UiMysteryBoxMenu.ShuffleCardDone(this);
	}
}
