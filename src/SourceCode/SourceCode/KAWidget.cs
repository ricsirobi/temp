using System;
using System.Collections.Generic;
using UnityEngine;

public class KAWidget : KAMonoBase
{
	[SerializeField]
	private bool _Visible = true;

	[SerializeField]
	private KAUIState _State;

	[HideInInspector]
	public int _MenuItemIndex = -1;

	public bool _LogClickEvent;

	private bool mOriginalVisibility;

	private KAUIState mOriginalState;

	public Vector3 _Pivot = Vector3.zero;

	public string _RolloverCursorName;

	public KATooltipInfo _TooltipInfo = new KATooltipInfo();

	public KASkinInfo _HoverInfo = new KASkinInfo();

	public KASkinInfo _DisabledInfo = new KASkinInfo();

	public KASkinInfo _SelectedInfo = new KASkinInfo();

	public Vector2 _AttachCursorOffset = Vector2.zero;

	public KAOrientationInfo _ScreenInfo = new KAOrientationInfo();

	public float _RotateSpeed;

	public float _RotateAngle;

	public Vector3 _RotationAxis = new Vector3(0f, 0f, 1f);

	[SerializeField]
	private UISprite _ProgressBar;

	[SerializeField]
	private UIProgressBar _UIProgressBar;

	[SerializeField]
	private UISprite _Background;

	[SerializeField]
	protected UILabel _Label;

	protected KAUI mUI;

	protected Vector3 mScale;

	protected Vector3 mPosition;

	protected KAPulsateWidget mPulsate;

	protected KAAnim2D mAnim2D;

	protected KAWidget mParentWidget;

	protected UITexture mTexture;

	protected bool mIsTextResolved;

	protected bool mIsUITextureResolved;

	protected bool mAttachToCursor;

	[NonSerialized]
	protected KAWidgetUserData mUserData;

	protected bool mIsClipped;

	protected List<KAWidget> mChildWidgets = new List<KAWidget>();

	protected List<UIWidget> mNGUIWidgets = new List<UIWidget>();

	protected KAUIState mParentState;

	private bool mParentVisible = true;

	private bool mVisibility;

	private KAUIState mState;

	private string mName = "";

	private TweenPosition mPositionTweener;

	private UIAnchor mAnchor;

	private string mTextureURL = string.Empty;

	private KAAnimInfo mPrevAnim;

	private GameObject mMessageObject;

	private bool mRotationPaused;

	private Action<KAWidget, bool> OnTextureLoaded;

	private Vector3 mScaleFactor = Vector3.zero;

	private bool mReferenceAdded;

	public bool pReferenceAdded
	{
		get
		{
			return mReferenceAdded;
		}
		set
		{
			mReferenceAdded = value;
		}
	}

	public override Collider collider
	{
		get
		{
			return mCachedCollider;
		}
		set
		{
			mCachedCollider = value;
		}
	}

	public bool pIsTweening
	{
		get
		{
			if (mPositionTweener == null)
			{
				return false;
			}
			return mPositionTweener.enabled;
		}
	}

	public KAUI pUI
	{
		get
		{
			return mUI;
		}
		set
		{
			mUI = value;
		}
	}

	public UIAnchor pAnchor
	{
		get
		{
			return mAnchor;
		}
		set
		{
			mAnchor = value;
		}
	}

	public UISprite pBackground => _Background;

	public bool pAttachToCursor => mAttachToCursor;

	public string pTextureURL => mTextureURL;

	public KAAnim2D pAnim2D => mAnim2D;

	public KAPulsateWidget pPulsate => mPulsate;

	public Vector3 pOrgScale
	{
		get
		{
			return mScale;
		}
		set
		{
			mScale = value;
		}
	}

	public Vector3 pScaleFactor
	{
		get
		{
			return mScaleFactor;
		}
		set
		{
			mScaleFactor = value;
		}
	}

	public Vector3 pOrgPosition
	{
		get
		{
			return mPosition;
		}
		set
		{
			mPosition = value;
		}
	}

	public int pCurFrame
	{
		get
		{
			if (mAnim2D == null)
			{
				return -1;
			}
			return mAnim2D.pSpriteIndex;
		}
	}

	public KAWidget pParentWidget => mParentWidget;

	public List<UIWidget> pNGUIWidgets => mNGUIWidgets;

	public List<KAWidget> pChildWidgets => mChildWidgets;

	public event Action<KAWidget> OnMoveToDone;

	protected virtual void Awake()
	{
		mScale = base.transform.localScale;
		mPosition = base.transform.localPosition;
		mName = base.transform.name;
		mParentWidget = UtUtilities.GetComponentInParent(typeof(KAWidget), base.gameObject) as KAWidget;
		mUI = UtUtilities.GetComponentInParent(typeof(KAUI), base.gameObject) as KAUI;
		mOriginalState = _State;
		mOriginalVisibility = _Visible;
		CacheWidgets();
		SetVisibility(_Visible);
		collider = GetComponent<Collider>();
		mAnchor = UtUtilities.GetComponentInParent(typeof(UIAnchor), base.gameObject) as UIAnchor;
		if (mParentWidget == null)
		{
			if (mUI == null)
			{
				return;
			}
			mUI.AddWidget(this);
		}
		else
		{
			mParentWidget.AddChild(this);
		}
		if (!mIsTextResolved)
		{
			ProcessLocaleText();
		}
	}

	private void Start()
	{
		SetState(_State);
	}

	[ContextMenu("Play Pulse Effect")]
	public void PulsateWidget()
	{
		if (pPulsate != null)
		{
			pPulsate.Play();
		}
	}

	[ContextMenu("Stop Pulse Effect")]
	public void StopPulsateEffect()
	{
		if (pPulsate != null)
		{
			pPulsate.Stop();
		}
	}

	public void SetPulsateReference(KAPulsateWidget pulsate)
	{
		mPulsate = pulsate;
	}

	public void PauseAnim()
	{
		mRotationPaused = true;
	}

	public void ResumeAnim()
	{
		mRotationPaused = false;
	}

	protected virtual void Update()
	{
		if (_RotateSpeed != 0f && !mRotationPaused)
		{
			_RotateAngle += _RotateSpeed * Time.deltaTime;
			if (_RotateAngle > 360f)
			{
				_RotateAngle -= 360f;
			}
			else if (_RotateAngle < 0f)
			{
				_RotateAngle += 360f;
			}
			base.transform.localRotation = Quaternion.AngleAxis(_RotateAngle, _RotationAxis);
		}
		_HoverInfo.Update();
		_DisabledInfo.Update();
		_SelectedInfo.Update();
		if (pPulsate != null)
		{
			pPulsate.DoUpdate();
		}
		if (!mIsTextResolved)
		{
			ProcessLocaleText();
		}
		if (mVisibility != _Visible)
		{
			SetVisibility(_Visible);
		}
		if (mState != _State)
		{
			SetState(_State);
		}
		if (mAttachToCursor)
		{
			UpdateWithCursorPosition();
		}
	}

	protected void ProcessLocaleText()
	{
		if (!LocaleData.pIsReady)
		{
			return;
		}
		mIsTextResolved = true;
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			if (mNGUIWidget.GetType() == typeof(UILabel))
			{
				UILabel uILabel = mNGUIWidget as UILabel;
				if (uILabel.bitmapFont != null)
				{
					uILabel.bitmapFont = FontTable.GetFontAtlas(uILabel.bitmapFont.name, uILabel.bitmapFont);
				}
				if (UtUtilities.GetLocaleLanguage() == "en-US")
				{
					uILabel.text = (string.IsNullOrEmpty(uILabel.englishTxt) ? uILabel.text : uILabel.englishTxt);
					continue;
				}
				string stringData = StringTable.GetStringData(uILabel.textID, uILabel.text);
				uILabel.text = stringData;
			}
		}
		if (_TooltipInfo._Text._ID != 0)
		{
			SetToolTipText(StringTable.GetStringData(_TooltipInfo._Text._ID, _TooltipInfo._Text._Text));
		}
	}

	protected void ProcessLocaleSprite(UISprite inSprite)
	{
		if (inSprite == null || inSprite.atlas == null)
		{
			return;
		}
		string localeLanguage = UtUtilities.GetLocaleLanguage();
		if (localeLanguage != "en-US")
		{
			string text = inSprite.spriteName + "_" + localeLanguage;
			if (inSprite.atlas.GetListOfSprites().Contains(text))
			{
				inSprite.spriteName = text;
			}
		}
	}

	protected void ProcessLocaleUITexture(UITexture inUiTexture)
	{
		if (LocaleData.pIsReady)
		{
			mIsUITextureResolved = true;
			if (!(null == inUiTexture) && !(null == inUiTexture.mainTexture) && UtUtilities.GetLocaleLanguage() != "en-US")
			{
				inUiTexture.mainTexture = LoTextureSwapper.GetLocalizedTexture(inUiTexture.mainTexture);
			}
		}
	}

	private void CacheWidgets()
	{
		GameObject[] childObjects = GetChildObjects(base.gameObject);
		mNGUIWidgets.Clear();
		if (childObjects != null)
		{
			for (int i = 0; i < childObjects.Length; i++)
			{
				UIWidget uIWidget = childObjects[i].GetComponent(typeof(UIWidget)) as UIWidget;
				if (uIWidget != null)
				{
					mNGUIWidgets.Add(uIWidget);
					if (uIWidget.GetType() == typeof(UITexture))
					{
						mTexture = uIWidget as UITexture;
					}
				}
			}
		}
		if (!pReferenceAdded)
		{
			KAUI.UpdateReferences(base.gameObject, add: true);
		}
	}

	private GameObject[] GetChildObjects(GameObject go)
	{
		GameObject[] array = null;
		if (go != null)
		{
			array = new GameObject[go.transform.childCount];
			for (int i = 0; i < go.transform.childCount; i++)
			{
				Transform child = go.transform.GetChild(i);
				if (child != null)
				{
					array[i] = child.gameObject;
				}
			}
		}
		return array;
	}

	public void AddChild(KAWidget childWidget)
	{
		mChildWidgets.Add(childWidget);
		Vector3 localScale = childWidget.transform.localScale;
		childWidget.transform.parent = base.transform;
		childWidget.transform.localScale = localScale;
		childWidget.mParentWidget = this;
		childWidget.OnParentSetVisibility(mParentVisible);
	}

	public int GetNumChildren()
	{
		return mChildWidgets.Count;
	}

	public KAWidget GetRootItem()
	{
		if (mParentWidget == null)
		{
			return this;
		}
		KAWidget parentItem = mParentWidget;
		while (parentItem.GetParentItem() != null)
		{
			parentItem = parentItem.GetParentItem();
		}
		return parentItem;
	}

	public KAWidget GetParentItem()
	{
		return mParentWidget;
	}

	public UIWidget FindChildNGUIItem(string widgetName)
	{
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			if (mNGUIWidget != null && mNGUIWidget.name == widgetName)
			{
				return mNGUIWidget;
			}
		}
		return null;
	}

	public UIWidget FindChildNGUIItemAt(int idx)
	{
		if (idx >= 0 && idx < mNGUIWidgets.Count)
		{
			return mNGUIWidgets[idx];
		}
		return null;
	}

	public KAWidget FindChildItem(string widgetName, bool ShowWarning = true)
	{
		KAWidget kAWidget = FindChildItemInPath(widgetName);
		if (ShowWarning && kAWidget == null)
		{
			UtDebug.LogWarning("FindChildItem can't find item '" + widgetName + "'", 100);
		}
		return kAWidget;
	}

	private KAWidget FindChildItemInPath(string widgetPath)
	{
		if (widgetPath.IndexOf('/') < 0)
		{
			return FindChildItemInternal(widgetPath);
		}
		string[] array = widgetPath.Split('/');
		KAWidget kAWidget = this;
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			if (!(kAWidget != null))
			{
				break;
			}
			kAWidget = kAWidget.FindChildItemInternal(array[i]);
		}
		return kAWidget;
	}

	internal KAWidget FindChildItemInternal(string widgetName)
	{
		foreach (KAWidget mChildWidget in mChildWidgets)
		{
			if (mChildWidget != null)
			{
				if (mChildWidget.name == widgetName)
				{
					return mChildWidget;
				}
				KAWidget kAWidget = mChildWidget.FindChildItemInternal(widgetName);
				if (kAWidget != null)
				{
					return kAWidget;
				}
			}
		}
		return null;
	}

	public KAWidget FindChildItemAt(int idx)
	{
		if (idx >= 0 && idx < mChildWidgets.Count)
		{
			return mChildWidgets[idx];
		}
		return null;
	}

	public virtual void OnSelected(bool inSelected)
	{
		if (IsActive())
		{
			_SelectedInfo.DoEffect(inSelected, this);
		}
	}

	public void UpdateOrientation(bool isPortrait)
	{
		_ScreenInfo.DoEffect(isPortrait);
		foreach (KAWidget pChildWidget in pChildWidgets)
		{
			pChildWidget.UpdateOrientation(isPortrait);
		}
	}

	public virtual void OnHover(bool inIsHover)
	{
		if (!Input.mousePresent || !IsActive())
		{
			return;
		}
		_HoverInfo.DoEffect(inIsHover, this);
		if (inIsHover)
		{
			if (mAnim2D != null && mAnim2D.pCurrentAnimInfo != null && mAnim2D.pCurrentAnimInfo.pLoopCount < 0)
			{
				mPrevAnim = mAnim2D.pCurrentAnimInfo;
			}
			else
			{
				mPrevAnim = null;
			}
			PlayAnim("Hover");
		}
		else if (mPrevAnim != null)
		{
			PlayAnim(mPrevAnim._Name);
		}
		else
		{
			PlayAnim("Normal");
		}
	}

	public virtual void OnPressRepeated(bool inPressed)
	{
	}

	public virtual void OnPress(bool inPressed)
	{
		mPrevAnim = null;
	}

	public virtual void OnClick()
	{
		OnClick(this);
		if (_LogClickEvent)
		{
			LogButtonClickEvent();
		}
	}

	private void LogButtonClickEvent()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("name", base.name);
		string value = "";
		if (base.transform.parent != null)
		{
			value = base.transform.parent.name;
		}
		dictionary.Add("parentName", value);
		value = base.transform.root.name;
		dictionary.Add("rootName", value);
		AnalyticAgent.LogEvent(AnalyticEvent.BUTTON_CLICKED, dictionary);
	}

	public virtual void OnClick(KAWidget inWidget)
	{
		mPrevAnim = null;
		if (mParentWidget != null)
		{
			mParentWidget.OnClick(inWidget);
		}
		if (pPulsate != null)
		{
			pPulsate.OnWidgetClicked(inWidget);
		}
	}

	public virtual void OnDragStart()
	{
	}

	public virtual void OnDragEnd()
	{
	}

	public virtual void OnDoubleClick()
	{
	}

	public virtual void OnTooltip(bool inShow)
	{
	}

	public virtual void OnInput(string inText)
	{
	}

	public virtual void OnSelect(bool inSelected)
	{
	}

	public virtual void OnDrag(Vector2 del)
	{
	}

	public virtual void OnSubmit()
	{
	}

	public virtual void OnDrop()
	{
	}

	public virtual void OnSwipe()
	{
	}

	public virtual bool GetVisibility()
	{
		return _Visible;
	}

	public virtual void SetVisibility(bool inVisible)
	{
		_Visible = inVisible;
		mVisibility = _Visible;
		UpdateVisibility(inVisible);
		if (pPulsate != null)
		{
			pPulsate.OnVisibilityChange(_Visible && mParentVisible);
		}
	}

	public void SetVisibilityOnChild(string ChildName, bool inVisible, bool ShowWarning = true)
	{
		KAWidget kAWidget = FindChildItem(ChildName, ShowWarning);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible);
		}
	}

	protected virtual void ResetEffects()
	{
		if (_HoverInfo.pIsEffectOn)
		{
			_HoverInfo.DoEffect(inShowEffect: false, this);
			if (mPrevAnim != null)
			{
				PlayAnim(mPrevAnim._Name);
			}
			else
			{
				PlayAnim("Normal");
			}
		}
		if (_SelectedInfo.pIsEffectOn)
		{
			_SelectedInfo.DoEffect(inShowEffect: false, this);
		}
	}

	protected virtual void UpdateVisibility(bool inVisible)
	{
		if (inVisible)
		{
			inVisible = mParentVisible && _Visible;
		}
		else
		{
			ResetEffects();
		}
		for (int i = 0; i < mNGUIWidgets.Count; i++)
		{
			mNGUIWidgets[i].enabled = inVisible;
		}
		for (int j = 0; j < mChildWidgets.Count; j++)
		{
			mChildWidgets[j].OnParentSetVisibility(inVisible);
		}
		EnableCollider(inVisible);
		if (_Visible && mTexture != null && mTexture.mainTexture == null && mTexture.material == null)
		{
			mTexture.enabled = false;
		}
		if (_UIProgressBar != null)
		{
			_UIProgressBar.enabled = inVisible;
			_UIProgressBar.ForceUpdate();
		}
	}

	public void OnParentSetVisibility(bool inVisible)
	{
		mParentVisible = inVisible;
		UpdateVisibility(inVisible);
	}

	protected virtual void EnableCollider(bool inEnable)
	{
		if (inEnable)
		{
			inEnable = mParentState == KAUIState.INTERACTIVE && mParentVisible && IsActive();
		}
		if (collider != null)
		{
			collider.enabled = inEnable;
		}
	}

	public void SetClipped(bool isClipped)
	{
		mIsClipped = isClipped;
	}

	public bool GetClipped()
	{
		return mIsClipped;
	}

	public virtual KAUIState GetState()
	{
		return _State;
	}

	public virtual void SetInteractive(bool isInteractive)
	{
		if (isInteractive)
		{
			SetState(KAUIState.INTERACTIVE);
		}
		else
		{
			SetState(KAUIState.NOT_INTERACTIVE);
		}
	}

	public virtual void SetDisabled(bool isDisabled)
	{
		if (isDisabled)
		{
			SetState(KAUIState.DISABLED);
		}
		else
		{
			SetState(KAUIState.INTERACTIVE);
		}
	}

	public virtual void SetState(KAUIState inState)
	{
		_State = inState;
		mState = _State;
		OnSetState(inState);
	}

	protected virtual void OnSetState(KAUIState inState)
	{
		if (inState == KAUIState.INTERACTIVE)
		{
			inState = ((mParentState == KAUIState.INTERACTIVE) ? _State : mParentState);
		}
		switch (inState)
		{
		case KAUIState.NOT_INTERACTIVE:
		case KAUIState.DISABLED:
			EnableCollider(inEnable: false);
			break;
		case KAUIState.INTERACTIVE:
			EnableCollider(inEnable: true);
			break;
		}
		ResetEffects();
		if (inState == KAUIState.DISABLED || mParentState == KAUIState.DISABLED || (inState == KAUIState.NOT_INTERACTIVE && _State == KAUIState.DISABLED))
		{
			_DisabledInfo.DoEffect(inShowEffect: true, this);
		}
		else
		{
			_DisabledInfo.DoEffect(inShowEffect: false, this);
		}
		foreach (KAWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.OnParentSetState(inState);
		}
	}

	public void OnParentSetState(KAUIState inState)
	{
		mParentState = inState;
		OnSetState(inState);
	}

	public bool IsActive()
	{
		if (_State == KAUIState.INTERACTIVE)
		{
			return _Visible;
		}
		return false;
	}

	public Vector3 GetScale()
	{
		return base.transform.localScale;
	}

	public void SetScale(Vector3 inScale)
	{
		base.transform.localScale = inScale;
	}

	private void OnDisable()
	{
		ResetEffects();
	}

	public virtual void ResetWidget(bool resetPosition = true)
	{
		mUserData = null;
		_State = mOriginalState;
		_Visible = mOriginalVisibility;
		base.transform.localScale = mScale;
		base.transform.name = mName;
		if (resetPosition)
		{
			base.transform.localPosition = mPosition;
		}
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			mNGUIWidget.ResetToOriginal();
		}
		foreach (KAWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.ResetWidget();
		}
	}

	public void PlayAnim(string animName)
	{
		if (mAnim2D != null)
		{
			mAnim2D.Play(animName);
		}
	}

	public void PlayAnim(string animName, int loopTimes)
	{
		if (mAnim2D != null)
		{
			mAnim2D.Play(animName, loopTimes);
		}
	}

	public void PlayAnim(int idx, int loopTimes)
	{
		if (mAnim2D != null)
		{
			mAnim2D.Play(idx, loopTimes);
		}
	}

	public void StopAnim()
	{
		if (mAnim2D != null)
		{
			mAnim2D.Stop();
		}
	}

	public void StopAnim(string animName)
	{
		if (mAnim2D != null)
		{
			mAnim2D.Stop(animName);
		}
	}

	public void SetAnim2DReference(KAAnim2D anim2D)
	{
		mAnim2D = anim2D;
	}

	public string GetCurrentAnim()
	{
		return GetCurrentAnimInfo()?._Name;
	}

	public KAAnimInfo GetCurrentAnimInfo()
	{
		if (mAnim2D == null)
		{
			return null;
		}
		return mAnim2D.pCurrentAnimInfo;
	}

	public void OnAnimEnd(int idx)
	{
		if (mUI != null)
		{
			mUI.OnAnimEnd(this, idx);
		}
	}

	public void SetDepth(int depth)
	{
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			mNGUIWidget.depth = depth + mNGUIWidget.pOrgDepth;
		}
	}

	public int GetMinDepth()
	{
		if (mNGUIWidgets.Count == 0)
		{
			return 0;
		}
		int depth = mNGUIWidgets[0].depth;
		for (int i = 1; i < mNGUIWidgets.Count; i++)
		{
			if (mNGUIWidgets[i].depth < depth)
			{
				depth = mNGUIWidgets[i].depth;
			}
		}
		return depth;
	}

	public int GetMaxDepth()
	{
		if (mNGUIWidgets.Count == 0)
		{
			return 0;
		}
		int depth = mNGUIWidgets[0].depth;
		for (int i = 1; i < mNGUIWidgets.Count; i++)
		{
			if (mNGUIWidgets[i].depth > depth)
			{
				depth = mNGUIWidgets[i].depth;
			}
		}
		return depth;
	}

	public void ScaleTo(Vector3 start, Vector3 end, float duration)
	{
		SetScale(start);
		TweenScale tweenScale = TweenScale.Begin(base.gameObject, duration, end);
		tweenScale.style = UITweener.Style.Once;
		tweenScale.eventReceiver = base.gameObject;
		tweenScale.callWhenFinished = "EndScaleTo";
	}

	public void EndScaleTo()
	{
		mUI.EndScaleTo(this);
	}

	public void ColorBlendTo(Color start, Color end, float duration)
	{
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			mNGUIWidget.color = start;
			TweenColor.Begin(mNGUIWidget.gameObject, duration, end).style = UITweener.Style.Once;
		}
	}

	public void StopColorBlendTo()
	{
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			TweenColor tweenColor = mNGUIWidget.gameObject.GetComponent(typeof(TweenColor)) as TweenColor;
			if (tweenColor != null)
			{
				tweenColor.enabled = false;
			}
		}
	}

	public virtual string GetText()
	{
		if (_Label == null)
		{
			Debug.LogError("Text widget reference is missing for " + base.name);
			return "";
		}
		return _Label.text;
	}

	public virtual void SetText(string text)
	{
		if (_Label == null)
		{
			Debug.LogError("Text widget reference is missing for " + base.name);
			return;
		}
		_Label.textID = 0;
		_Label.text = text;
		_Label.ResetEnglishText();
	}

	public void SetTextByID(int id, string text)
	{
		text = StringTable.GetStringData(id, text);
		SetText(text);
	}

	public void SetTextOnChild(string WidgetName, string Text, bool ShowWarning = true)
	{
		KAWidget kAWidget = FindChildItem(WidgetName, ShowWarning);
		if (kAWidget != null)
		{
			kAWidget.SetText(Text);
		}
	}

	public float GetProgressLevel()
	{
		if (_ProgressBar == null && _UIProgressBar == null)
		{
			Debug.LogError("Progress bar reference is missing for " + base.name);
		}
		else
		{
			if (_UIProgressBar != null)
			{
				return _UIProgressBar.value;
			}
			if (_ProgressBar != null)
			{
				return _ProgressBar.fillAmount;
			}
		}
		return 0f;
	}

	public void SetProgressLevel(float inPercentage)
	{
		if (_ProgressBar == null && _UIProgressBar == null)
		{
			Debug.LogError("Progress bar reference is missing for " + base.name);
		}
		else if (_UIProgressBar != null)
		{
			_UIProgressBar.value = inPercentage;
		}
		else if (_ProgressBar != null)
		{
			_ProgressBar.fillAmount = inPercentage;
		}
	}

	public void SetProgressLevel(float inPercentage, UIBasicSprite.FillDirection inProgressDirection)
	{
		if (_ProgressBar == null)
		{
			Debug.LogError("Progress bar sprite reference is missing");
			return;
		}
		_ProgressBar.fillDirection = inProgressDirection;
		_ProgressBar.fillAmount = inPercentage;
	}

	public void SetProgressLevel(float inPercentage, UIProgressBar.FillDirection inProgressDirection)
	{
		if (_UIProgressBar == null)
		{
			Debug.LogError("UIProgressBar reference is missing");
			return;
		}
		_UIProgressBar.fillDirection = inProgressDirection;
		_UIProgressBar.value = inPercentage;
	}

	public void SetProgressLevel(float inPercentage, UIBasicSprite.FillDirection inProgressDirection, Color inColor)
	{
		if (_ProgressBar == null)
		{
			Debug.LogError("Progress bar sprite reference is missing");
			return;
		}
		_ProgressBar.fillDirection = inProgressDirection;
		_ProgressBar.fillAmount = inPercentage;
		_ProgressBar.color = inColor;
	}

	public void SetProgressLevel(float inPercentage, UIProgressBar.FillDirection inProgressDirection, Color inColor)
	{
		if (_UIProgressBar == null)
		{
			Debug.LogError("UIProgressBar reference is missing");
			return;
		}
		_UIProgressBar.fillDirection = inProgressDirection;
		_UIProgressBar.value = inPercentage;
		_UIProgressBar.foregroundWidget.color = inColor;
	}

	public Vector3 GetPosition()
	{
		return base.transform.localPosition;
	}

	public void SetPosition(float positionX, float positionY)
	{
		if (base.transform != null)
		{
			base.transform.localPosition = new Vector3(positionX, positionY, base.transform.localPosition.z);
		}
	}

	public void SetRotateSpeed(float rotSpeed)
	{
		_RotateSpeed = rotSpeed;
	}

	public void SetRotation(Quaternion rotation)
	{
		base.transform.localRotation = rotation;
		rotation.ToAngleAxis(out _RotateAngle, out _RotationAxis);
	}

	public void SetRotation(float angle, float x, float y, float z)
	{
		_RotationAxis = new Vector3(x, y, z);
		base.transform.localRotation = Quaternion.AngleAxis(angle, _RotationAxis);
		_RotateAngle = angle;
	}

	public void MoveTo(Vector2 start, Vector2 end, float duration)
	{
		Vector3 vector = new Vector3(start.x, start.y, base.transform.localPosition.z);
		if (mPositionTweener == null)
		{
			base.transform.localPosition = vector;
		}
		else
		{
			mPositionTweener.from = vector;
		}
		MoveTo(end, duration);
	}

	public void MoveTo(Vector2 end, float duration)
	{
		mPositionTweener = TweenPosition.Begin(pos: new Vector3(end.x, end.y, base.transform.localPosition.z), go: base.gameObject, duration: duration);
		mPositionTweener.eventReceiver = base.gameObject;
		mPositionTweener.callWhenFinished = "EndMoveTo";
	}

	public void EndMoveTo()
	{
		mUI.EndMoveTo(this);
		if (this.OnMoveToDone != null)
		{
			this.OnMoveToDone(this);
		}
	}

	public void StopMoveTo()
	{
		mPositionTweener.enabled = false;
		EndMoveTo();
	}

	public UILabel GetLabel()
	{
		return _Label;
	}

	public void SetBackground(UISprite sprite)
	{
		_Background = sprite;
	}

	public void SetLabel(UILabel label)
	{
		_Label = label;
	}

	public UISprite GetProgressBar()
	{
		return _ProgressBar;
	}

	public void SetProgressBar(UISprite inFilledSprite)
	{
		_ProgressBar = inFilledSprite;
	}

	public void SetFont(UIFont inFont)
	{
		if (_Label != null)
		{
			_Label.bitmapFont = inFont;
		}
	}

	public void SetToolTipTextByID(int id, string text)
	{
		_TooltipInfo._Text._ID = id;
		_TooltipInfo._Text._Text = text;
	}

	public void SetToolTipText(string text)
	{
		SetToolTipTextByID(0, text);
	}

	public void SetToolTipSound(SnSound inSound)
	{
		_TooltipInfo._Sound = inSound;
	}

	public Texture GetTexture()
	{
		if (mTexture != null)
		{
			return mTexture.mainTexture;
		}
		return null;
	}

	public UITexture GetUITexture()
	{
		if (mTexture != null)
		{
			return mTexture;
		}
		return null;
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (this != null)
			{
				SetTexture((Texture2D)inObject);
				mTextureURL = inURL;
				if (mMessageObject != null)
				{
					mMessageObject.SendMessage("OnTextureLoaded", this);
					mMessageObject = null;
				}
				if (OnTextureLoaded != null)
				{
					OnTextureLoaded(this, arg2: true);
					OnTextureLoaded = null;
				}
			}
			else
			{
				UtDebug.LogError("KAWidget is destroyed cant assign the texture");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Missing texture " + inURL);
			if (mMessageObject != null)
			{
				mMessageObject.SendMessage("OnTextureLoadFailed", this);
				mMessageObject = null;
			}
			if (OnTextureLoaded != null)
			{
				OnTextureLoaded(this, arg2: false);
				OnTextureLoaded = null;
			}
			break;
		}
	}

	public void SetTextureFromURL(string textureURL, GameObject messageGameObject = null, Action<KAWidget, bool> actionEvent = null, bool ignoreReferenceCount = false)
	{
		if (!string.IsNullOrEmpty(textureURL))
		{
			mMessageObject = messageGameObject;
			OnTextureLoaded = actionEvent;
			RsResourceManager.Load(textureURL, OnResLoadingEvent, RsResourceType.IMAGE, inDontDestroy: false, inDisableCache: false, inDownloadOnly: false, inIgnoreAssetVersion: false, null, ignoreReferenceCount);
		}
		else
		{
			Debug.LogError("Missing texture URL");
		}
	}

	public void SetTextureFromBundle(string assetPath, GameObject messageGameObject = null, Action<KAWidget, bool> actionEvent = null)
	{
		if (!string.IsNullOrEmpty(assetPath))
		{
			string[] array = assetPath.Split('/');
			SetTextureFromBundle(array[0] + "/" + array[1], array[2], messageGameObject, actionEvent);
		}
		else
		{
			Debug.LogError("Missing Asset Path or Asset Name");
		}
	}

	public void SetTextureFromBundle(string assetPath, string assetName, GameObject messageGameObject = null, Action<KAWidget, bool> actionEvent = null)
	{
		mMessageObject = messageGameObject;
		OnTextureLoaded = actionEvent;
		RsResourceManager.LoadAssetFromBundle(assetPath, assetName, OnResLoadingEvent, typeof(Texture2D));
	}

	public void SetTexture(Texture inTexture, bool inPixelPerfect = false, string textureURL = null)
	{
		if (mTexture != null)
		{
			mTextureURL = textureURL;
			mTexture.mainTexture = inTexture;
			mTexture.enabled = mParentVisible && _Visible && inTexture != null;
			if (mTexture.mainTexture != null)
			{
				mTexture.mainTexture.name = "WidgetTexture-" + base.name;
			}
			if (inPixelPerfect)
			{
				mTexture.MakePixelPerfect();
			}
		}
		else
		{
			Debug.LogError("You need a UITexture component attached to the KAWidget to do this operation " + base.name);
		}
	}

	public void SetSprite(string inSprite)
	{
		if (!string.IsNullOrEmpty(inSprite) && !(_Background == null))
		{
			_Background.spriteName = inSprite;
			_Background.pOrgSprite = inSprite;
		}
	}

	public void SetUserData(KAWidgetUserData ud)
	{
		mUserData = ud;
		if (ud != null)
		{
			ud._Item = this;
		}
	}

	public void CloneAndSetUserData(KAWidgetUserData ud)
	{
		mUserData = ud.CloneObject();
		if (mUserData != null)
		{
			mUserData._Item = this;
		}
	}

	public KAWidgetUserData GetUserData()
	{
		return mUserData;
	}

	public void SetUserDataInt(int i)
	{
		mUserData = new KAWidgetUserData();
		mUserData._Index = i;
	}

	public int GetUserDataInt()
	{
		return mUserData._Index;
	}

	protected virtual void OnDestroy()
	{
		if (mUserData != null)
		{
			mUserData.Destroy();
			mUserData = null;
		}
		ClearMaterialInstance();
		ClearReferences();
	}

	private void ClearMaterialInstance()
	{
		if (!(mTexture == null))
		{
			_ = mTexture.material == null;
		}
	}

	public void ClearChildItems()
	{
		foreach (KAWidget mChildWidget in mChildWidgets)
		{
			if (mChildWidget != null)
			{
				UnityEngine.Object.Destroy(mChildWidget.gameObject);
			}
		}
		mChildWidgets.Clear();
	}

	public void DetachFromCursor()
	{
		mAttachToCursor = false;
	}

	public void AttachToCursor(float offX, float offY)
	{
		mAttachToCursor = true;
		_AttachCursorOffset.x = offX;
		_AttachCursorOffset.y = offY;
	}

	public Vector2 GetLocalPosition(Vector2 pos)
	{
		Transform parent = base.gameObject.transform.parent;
		Vector2 result = pos;
		while (parent != null)
		{
			result -= new Vector2(parent.localPosition.x, parent.localPosition.y);
			parent = parent.parent;
		}
		return result;
	}

	public void UpdateWithCursorPosition()
	{
		if (KAUIManager.pInstance != null)
		{
			bool num = Input.mousePosition.x >= 0f && Input.mousePosition.x <= (float)KAUIManager.pInstance.camera.pixelWidth;
			bool flag = Input.mousePosition.y >= 0f && Input.mousePosition.y <= (float)KAUIManager.pInstance.camera.pixelHeight;
			if (num && flag)
			{
				Vector2 pos = KAUIManager.pInstance.camera.ScreenToWorldPoint(Input.mousePosition);
				pos += _AttachCursorOffset;
				Vector2 localPosition = GetLocalPosition(pos);
				SetPosition(localPosition.x, localPosition.y);
			}
		}
	}

	public virtual void SetToScreenPosition(float px, float py)
	{
		if (KAUIManager.pInstance != null)
		{
			Vector2 pos = KAUIManager.pInstance.camera.ScreenToWorldPoint(new Vector2(px, (float)Screen.height - py));
			Vector2 localPosition = GetLocalPosition(pos);
			SetPosition(localPosition.x, localPosition.y);
		}
	}

	public virtual Vector3 GetScreenPosition()
	{
		if (KAUIManager.pInstance != null)
		{
			return KAUIManager.pInstance.camera.WorldToScreenPoint(GetPosition());
		}
		return Vector3.zero;
	}

	public virtual bool RemoveChildItem(KAWidget item)
	{
		return mChildWidgets.Remove(item);
	}

	public virtual bool RemoveChildItem(KAWidget item, bool destroy)
	{
		bool result = RemoveChildItem(item);
		if (destroy && item != null)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		return result;
	}

	public float GetMaxZ()
	{
		float num = 0f;
		foreach (UIWidget mNGUIWidget in mNGUIWidgets)
		{
			if (mNGUIWidget.transform.localPosition.z > num)
			{
				num = mNGUIWidget.transform.localPosition.z;
			}
		}
		foreach (KAWidget mChildWidget in mChildWidgets)
		{
			if (mChildWidget.GetMaxZ() > num)
			{
				num = mChildWidget.GetMaxZ();
			}
		}
		return num;
	}

	private void ClearReferences()
	{
		if (pReferenceAdded)
		{
			KAUI.UpdateReferences(base.gameObject, add: false);
		}
		if (_Background != null)
		{
			_Background = null;
		}
		_Label = null;
		mUI = null;
		if (mTexture != null && mTexture.mainTexture != null && !string.IsNullOrEmpty(mTextureURL))
		{
			RsResourceManager.Unload(mTextureURL);
			mTextureURL = string.Empty;
		}
		mTexture = null;
		_ProgressBar = null;
		mParentWidget = null;
		if (mUserData != null && mUserData._Item == this)
		{
			mUserData._Item = null;
		}
		mUserData = null;
		mChildWidgets.Clear();
		mNGUIWidgets.Clear();
	}

	public bool GetScreenRect(ref Rect inRect)
	{
		if (_Background == null)
		{
			return false;
		}
		inRect.x = base.transform.localPosition.x;
		inRect.y = base.transform.localPosition.y;
		inRect.x -= pBackground.transform.localScale.x / 2f;
		inRect.y -= pBackground.transform.localScale.y / 2f;
		inRect.width = pBackground.transform.localScale.x;
		inRect.height = pBackground.transform.localScale.y;
		return true;
	}

	public virtual void SetFont(string inFontURL)
	{
		string[] array = inFontURL.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnFontDownloadComplete, typeof(GameObject));
	}

	private void OnFontDownloadComplete(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = inObject as GameObject;
			_Label.bitmapFont = gameObject.GetComponent<UIFont>();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("###### Could not load Font Bundle " + inURL);
			break;
		}
	}
}
