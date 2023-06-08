using UnityEngine;

public class UiCard : KAUI
{
	public class Transition
	{
		public enum Type
		{
			SlideIn,
			SlideOut,
			FadeIn,
			FadeOut,
			PushParents,
			HideParents,
			Appear
		}

		public enum Direction
		{
			Left,
			Right,
			Top,
			Bottom,
			None
		}

		public enum ZOrder
		{
			Top,
			Bottom,
			Equal
		}

		public Type mTransitionType = Type.SlideOut;

		public Direction mDirection;

		public ZOrder mOrder = ZOrder.Bottom;

		public Transition(Type type, Direction dir, ZOrder order)
		{
			mTransitionType = type;
			mDirection = dir;
			mOrder = order;
		}
	}

	public Vector3 _CardSize;

	public UiCardParent _UiCardParent;

	protected UiCard mParent;

	protected UiCard mChild;

	protected bool mIsTransition;

	private bool mPopOutCard;

	private Transition mParentTransition;

	private Transition mTransition;

	private KAWidget mExitBtn;

	private KAWidget mMessageTxt;

	protected GameObject mMessageObject;

	private bool mPopoutTransitionDone;

	protected bool mParentPopoutTransitionDone;

	private bool mReleaseParentLink;

	private bool mExitBtnVisibility = true;

	private int mCachedDepth;

	private UIPanel mBasePanel;

	public bool pPopOutCard => mPopOutCard;

	public int pDepth => mBasePanel.depth;

	protected override void Awake()
	{
		base.Awake();
		mBasePanel = GetComponent<UIPanel>();
		mCachedDepth = mBasePanel.depth;
	}

	protected override void Start()
	{
		base.Start();
		mExitBtn = FindItem("ExitBtn");
		mMessageTxt = FindItem("TxtCardsListTip");
		if (mExitBtn == null)
		{
			Debug.Log("Exit button not available " + base.name);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mExitBtn == inWidget)
		{
			OnExitClicked();
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	public virtual void OnExitClicked()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnCardExit", SendMessageOptions.DontRequireReceiver);
		}
		_UiCardParent.Exit();
	}

	public virtual void HideMessage()
	{
		if (mMessageTxt != null)
		{
			mMessageTxt.SetVisibility(inVisible: false);
		}
	}

	public virtual void ShowMessage(LocaleString message)
	{
		if (mMessageTxt == null)
		{
			mMessageTxt = FindItem("TxtCardsListTip");
		}
		if (mMessageTxt != null)
		{
			mMessageTxt.SetText(message.GetLocalizedString());
			mMessageTxt.SetVisibility(inVisible: true);
		}
	}

	public bool IsMessageVisible()
	{
		return mMessageTxt.GetVisibility();
	}

	public void SetMessageObject(GameObject msg)
	{
		mMessageObject = msg;
	}

	public void PushCard(UiCard parent, Transition parentTrans, Transition childTrans)
	{
		if (!mIsTransition)
		{
			SetInteractive(interactive: false);
			mPopoutTransitionDone = false;
			mParentPopoutTransitionDone = false;
			mPopOutCard = false;
			mParent = parent;
			mParentTransition = parentTrans;
			if (mParent != null)
			{
				mParent.SetInteractive(interactive: false);
				mParent.mChild = this;
			}
			mTransition = childTrans;
			mIsTransition = true;
			StartTransition();
			StartParentTransition(mParentTransition);
			if (mParent != null && mParent.mExitBtn != null)
			{
				mParent.mExitBtn.SetVisibility(inVisible: false);
			}
			if (mExitBtn != null)
			{
				mExitBtn.SetVisibility(mExitBtnVisibility);
			}
		}
	}

	public void PopOutCard(bool releaseParentLink = true)
	{
		if (!mIsTransition)
		{
			SetInteractive(interactive: false);
			mReleaseParentLink = releaseParentLink;
			mIsTransition = true;
			mPopOutCard = true;
			if (mChild != null)
			{
				mChild.PopOutCard(releaseParentLink: false);
			}
			else
			{
				ReverseTransition();
			}
			if (mParentTransition != null)
			{
				ReverseParentTransition();
			}
			else
			{
				mParentPopoutTransitionDone = true;
			}
		}
	}

	public int GetPositionDepth(Transition.ZOrder order)
	{
		float num = pDepth;
		switch (order)
		{
		case Transition.ZOrder.Top:
		{
			float num2 = 0f;
			if (mParent != null)
			{
				num2 = mParent.pDepth;
				num2 = mParent.GetParentThickness(order, num2);
			}
			float b = 0f;
			if (mChild != null)
			{
				b = mChild.pDepth;
				b = mChild.GetChildThickness(order, b);
			}
			num2 += _CardSize.z;
			num = Mathf.Max(num2, b);
			break;
		}
		case Transition.ZOrder.Bottom:
			if (mParent != null)
			{
				num = (float)mParent.pDepth - (mParent._CardSize.z + _CardSize.z);
			}
			break;
		default:
			if (mParent != null)
			{
				num = mParent.pDepth;
			}
			break;
		}
		return (int)num;
	}

	public float GetParentThickness(Transition.ZOrder order, float value)
	{
		if (mParent != null)
		{
			value = mParent.GetParentThickness(order, value);
		}
		switch (order)
		{
		case Transition.ZOrder.Bottom:
			value = Mathf.Min(value, (float)pDepth - _CardSize.z);
			break;
		case Transition.ZOrder.Top:
			value = Mathf.Max(value, (float)pDepth + _CardSize.z);
			break;
		}
		return value;
	}

	public float GetChildThickness(Transition.ZOrder order, float value)
	{
		if (mChild != null)
		{
			return mChild.GetChildThickness(order, value);
		}
		switch (order)
		{
		case Transition.ZOrder.Bottom:
			value = Mathf.Min(value, (float)pDepth - _CardSize.z);
			break;
		case Transition.ZOrder.Top:
			value = Mathf.Max(value, (float)pDepth + _CardSize.z);
			break;
		}
		return value;
	}

	public void StartParentTransition(Transition parentTrans)
	{
		float transTime = 0.3f;
		if (parentTrans == null)
		{
			return;
		}
		switch (parentTrans.mTransitionType)
		{
		case Transition.Type.PushParents:
			if (mParent != null)
			{
				float x = 0f;
				if (parentTrans.mDirection == Transition.Direction.Right)
				{
					x = _CardSize.x;
				}
				else if (parentTrans.mDirection == Transition.Direction.Left)
				{
					x = 0f - _CardSize.x;
				}
				Vector3 transMagnitude = new Vector3(x, 0f, 0f);
				UiCard uiCard2 = mParent;
				while (uiCard2 != null)
				{
					uiCard2.SetInteractive(interactive: false);
					uiCard2.MoveCard(transMagnitude, transTime, "OnParentMoveCardDone");
					uiCard2 = uiCard2.mParent;
				}
			}
			break;
		case Transition.Type.HideParents:
			if (mParent != null)
			{
				UiCard uiCard = mParent;
				while (uiCard != null)
				{
					uiCard.SetVisibility(inVisible: false);
					uiCard = uiCard.mParent;
				}
			}
			break;
		}
	}

	public void ReverseParentTransition()
	{
		float transTime = 0.3f;
		if (mParentTransition == null)
		{
			return;
		}
		switch (mParentTransition.mTransitionType)
		{
		case Transition.Type.PushParents:
			if (mParent != null)
			{
				float x = 0f;
				if (mParentTransition.mDirection == Transition.Direction.Right)
				{
					x = 0f - _CardSize.x;
				}
				else if (mParentTransition.mDirection == Transition.Direction.Left)
				{
					x = _CardSize.x;
				}
				Vector3 transMagnitude = new Vector3(x, 0f, 0f);
				UiCard uiCard2 = mParent;
				while (uiCard2 != null)
				{
					uiCard2.SetInteractive(interactive: false);
					uiCard2.MoveCard(transMagnitude, transTime, "OnParentReverseMoveCardDone");
					uiCard2 = uiCard2.mParent;
				}
			}
			break;
		case Transition.Type.HideParents:
			if (mParent != null)
			{
				UiCard uiCard = mParent;
				while (uiCard != null)
				{
					uiCard.SetVisibility(inVisible: true);
					uiCard = uiCard.mParent;
				}
			}
			break;
		}
	}

	public void MoveCard(Vector3 transMagnitude, float transTime, string callBack)
	{
		Vector2 end = new Vector2(transMagnitude.x + base.transform.position.x, transMagnitude.y + base.transform.position.y);
		SlideToPos(this, end, transTime, callBack);
	}

	public void OnParentMoveCardDone()
	{
		ParentTransitionDone();
	}

	public void OnParentReverseMoveCardDone()
	{
		ParentReverseTransitionDone();
		if (mChild != null)
		{
			mChild.ParentReverseTransitionDone();
		}
	}

	public void SetDepth(int newDepth)
	{
		int num = newDepth - pDepth;
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth += num;
		}
	}

	public void RevertDepth()
	{
		int num = pDepth - mCachedDepth;
		UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].depth -= num;
		}
	}

	public void StartTransition()
	{
		float duration = 0.3f;
		switch (mTransition.mTransitionType)
		{
		case Transition.Type.SlideOut:
		{
			Vector2 end = Vector2.zero;
			Vector3 zero2 = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			float z2 = base.transform.localPosition.z;
			int depth = GetPositionDepth(mTransition.mOrder);
			if (mParent != null)
			{
				zero2 = mParent.transform.position;
				vector2 = mParent._CardSize;
			}
			else
			{
				zero2 = _UiCardParent._CardStartPos;
				depth = (int)_UiCardParent._CardStartPos.z;
			}
			if (mTransition.mDirection == Transition.Direction.Right)
			{
				base.transform.localPosition = new Vector3(zero2.x, zero2.y, z2);
				SetDepth(depth);
				end = ((!(mParent != null)) ? new Vector2(_UiCardParent._CardEndPos.x, _UiCardParent._CardEndPos.y) : new Vector2(zero2.x + vector2.x, base.transform.position.y));
			}
			else if (mTransition.mDirection == Transition.Direction.Left)
			{
				base.transform.localPosition = new Vector3(zero2.x + vector2.x, zero2.y, z2);
				SetDepth(depth);
				end = ((!(mParent != null)) ? new Vector2(_UiCardParent._CardEndPos.x, _UiCardParent._CardEndPos.y) : new Vector2(zero2.x - _CardSize.x, base.transform.position.y));
			}
			SlideToPos(this, end, duration, "OnSlideEndCurrentCard");
			SetVisibility(inVisible: true);
			break;
		}
		case Transition.Type.Appear:
		{
			Vector3 zero = Vector3.zero;
			Vector3 vector = Vector3.zero;
			float z = base.transform.localPosition.z;
			int positionDepth = GetPositionDepth(mTransition.mOrder);
			if (mParent != null)
			{
				zero = mParent.transform.position;
				vector = mParent._CardSize;
			}
			else
			{
				zero = _UiCardParent._CardEndPos;
			}
			if (mTransition.mDirection == Transition.Direction.Right)
			{
				base.transform.localPosition = new Vector3(zero.x + vector.x, base.transform.position.y, z);
			}
			else if (mTransition.mDirection == Transition.Direction.Left)
			{
				base.transform.localPosition = new Vector3(zero.x - _CardSize.x, base.transform.position.y, z);
			}
			else if (mTransition.mDirection == Transition.Direction.None)
			{
				base.transform.localPosition = new Vector3(zero.x, base.transform.position.y, z);
			}
			SetDepth(positionDepth);
			mIsTransition = false;
			TransitionDone();
			SetVisibility(inVisible: true);
			break;
		}
		}
	}

	public void ReverseTransition()
	{
		float duration = 0.3f;
		if (mTransition == null)
		{
			return;
		}
		switch (mTransition.mTransitionType)
		{
		case Transition.Type.SlideOut:
		{
			Vector2 end = Vector2.zero;
			Vector3 zero = Vector3.zero;
			Vector3 vector = Vector3.zero;
			if (mParent != null)
			{
				mParent.SetInteractive(interactive: false);
				zero = mParent.transform.position;
				vector = mParent._CardSize;
			}
			else
			{
				zero = _UiCardParent._CardStartPos;
			}
			if (mTransition.mDirection == Transition.Direction.Right)
			{
				end = new Vector3(zero.x, zero.y, zero.z);
			}
			else if (mTransition.mDirection == Transition.Direction.Left)
			{
				end = new Vector3(zero.x + vector.x, zero.y, zero.z);
			}
			SlideToPos(this, end, duration, "OnSlideEndCurrentCardReverse");
			break;
		}
		case Transition.Type.Appear:
			mIsTransition = false;
			ReverseTransitionDone();
			if ((bool)mParent)
			{
				mParent.ChildReverseTransitionDone();
			}
			SetVisibility(inVisible: false);
			break;
		}
	}

	public void SlideToPos(UiCard card, Vector2 end, float duration, string callBack)
	{
		TweenPosition obj = TweenPosition.Begin(pos: new Vector3(end.x, end.y, card.transform.localPosition.z), go: card.gameObject, duration: duration);
		obj.eventReceiver = base.gameObject;
		obj.callWhenFinished = callBack;
	}

	public void OnSlideEndCurrentCard()
	{
		mIsTransition = false;
		TransitionDone();
		if ((bool)mParent)
		{
			mParent.ChildTransitionDone();
		}
	}

	public void OnSlideEndCurrentCardReverse()
	{
		mIsTransition = false;
		UiCard uiCard = mParent;
		ReverseTransitionDone();
		if ((bool)uiCard)
		{
			uiCard.ChildReverseTransitionDone();
		}
	}

	public virtual void TransitionDone()
	{
		SetInteractive(interactive: true);
	}

	public virtual void ReverseTransitionDone()
	{
		mPopoutTransitionDone = true;
		RevertDepth();
		SetVisibility(inVisible: false);
		if (mParent != null && mParent.mExitBtn != null)
		{
			mParent.mExitBtn.SetVisibility(mParent.mExitBtnVisibility);
		}
		if (mExitBtn != null)
		{
			mExitBtn.SetVisibility(inVisible: false);
		}
		if (mParent == null)
		{
			_UiCardParent.Exit();
		}
		ReleaseChildparentLink();
	}

	public virtual void ParentTransitionDone()
	{
		SetInteractive(interactive: true);
	}

	public virtual void ParentReverseTransitionDone()
	{
		SetInteractive(interactive: true);
		mParentPopoutTransitionDone = true;
		ReleaseChildparentLink();
	}

	public virtual void ChildTransitionDone()
	{
		SetInteractive(interactive: true);
	}

	public virtual void ChildReverseTransitionDone()
	{
		SetInteractive(interactive: true);
		if (mPopOutCard)
		{
			ReverseTransition();
		}
	}

	public void ReleaseChildparentLink()
	{
		if (mReleaseParentLink && mPopoutTransitionDone && mParentPopoutTransitionDone && mParent != null)
		{
			if (mParent.mChild != null)
			{
				mParent.mChild = null;
			}
			mParent = null;
		}
	}

	public void SetCloseButtonVisibility(bool visible)
	{
		mExitBtnVisibility = visible;
	}
}
