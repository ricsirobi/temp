using System;
using System.Collections.Generic;
using System.Linq;
using JSGames.Tween;
using JSGames.UI.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

public class UIWidget : UIBase, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Vector3 _RotationPerSecond;

	public TooltipInfo _TooltipInfo;

	[Header("Unity Component References")]
	public Text _Text;

	public Image _Background;

	public RawImage _RawImageBackground;

	[Header("Effects")]
	public UIEffects _ClickEffects;

	public UIEffects _DisabledEffects;

	public UIEffects _HoverEffects;

	public UIEffects _PressEffects;

	protected Selectable mSelectable;

	private NonDrawableGraphic mRaycastCaptureGraphic;

	[NonSerialized]
	private bool mInitialized;

	private UIAnim2D mAnim2D;

	private bool mIsHovering;

	private bool mIsPressed;

	protected bool mIsDragging;

	private bool mIsAttachedToPointer;

	private Vector2 mPointerAttachOffset;

	private int mFingerIDAttachedTo = -1;

	private bool mPreviousCanReceiveInput;

	private UIEffects mCurrentEffect;

	private float mCurrentEffectStartTime;

	private UIEffects mCurrentOneTimeEffect;

	public UIAnim2D pAnim2D => mAnim2D;

	public UIWidget pParentWidget { get; protected set; }

	public bool pIsAttachedToPointer => mIsAttachedToPointer;

	public object pData { get; set; }

	public Color pColor
	{
		get
		{
			if (_Background != null)
			{
				return _Background.color;
			}
			UtDebug.LogError("Background is not set. Default value returned");
			return Color.white;
		}
		set
		{
			if (_Background != null)
			{
				_Background.color = value;
				GraphicState graphicState = FindGraphicState(_Background);
				if (graphicState != null)
				{
					graphicState.pOriginalColor = value;
				}
			}
			else
			{
				UtDebug.LogError("Background is not set");
			}
		}
	}

	public Color pTextureColor
	{
		get
		{
			if (_RawImageBackground != null)
			{
				return _RawImageBackground.color;
			}
			UtDebug.LogError("Background is not set. Default value returned");
			return Color.white;
		}
		set
		{
			if (_RawImageBackground != null)
			{
				_RawImageBackground.color = value;
				GraphicState graphicState = FindGraphicState(_RawImageBackground);
				if (graphicState != null)
				{
					graphicState.pOriginalColor = value;
				}
			}
			else
			{
				UtDebug.LogError("RawImageBackground is not set");
			}
		}
	}

	public float pAlpha
	{
		get
		{
			if (_Background != null)
			{
				return _Background.color.a;
			}
			UtDebug.LogError("Background is not set. Default value returned");
			return 1f;
		}
		set
		{
			if (_Background != null)
			{
				Color color = _Background.color;
				color.a = value;
				_Background.color = color;
				GraphicState graphicState = FindGraphicState(_Background);
				if (graphicState != null)
				{
					graphicState.pOriginalColor = color;
				}
			}
			else
			{
				UtDebug.LogError("Background is not set");
			}
		}
	}

	public Sprite pSprite
	{
		get
		{
			if (_Background != null)
			{
				return _Background.sprite;
			}
			UtDebug.LogError("Background is not set. Default value returned");
			return null;
		}
		set
		{
			if (_Background != null)
			{
				_Background.sprite = value;
				GraphicState graphicState = FindGraphicState(_Background);
				if (graphicState != null)
				{
					graphicState.pOriginalSprite = value;
				}
			}
			else
			{
				UtDebug.LogError("Background is not set");
			}
		}
	}

	public Texture mainTexture
	{
		get
		{
			if (_RawImageBackground != null)
			{
				return _RawImageBackground.texture;
			}
			UtDebug.LogError("RawImageBackground is not set. Default value returned");
			return null;
		}
		set
		{
			if (_RawImageBackground != null)
			{
				_RawImageBackground.texture = value;
			}
			else
			{
				UtDebug.LogError("RawImageBackground is not set");
			}
		}
	}

	public virtual string pText
	{
		get
		{
			if (_Text != null)
			{
				return _Text.text;
			}
			UtDebug.LogError("Text is not set. Default value returned");
			return "";
		}
		set
		{
			if (_Text != null)
			{
				_Text.supportRichText = true;
				_Text.text = UIUtil.NGUIToUGUIConvert(value);
			}
			else
			{
				UtDebug.LogError("Text is not set for " + base.name);
			}
		}
	}

	public Color pTextColor
	{
		get
		{
			if (_Text != null)
			{
				return _Text.color;
			}
			UtDebug.LogError("Text is not set. Default value returned");
			return Color.white;
		}
		set
		{
			if (_Text != null)
			{
				_Text.color = value;
				GraphicState graphicState = FindGraphicState(_Text);
				if (graphicState != null)
				{
					graphicState.pOriginalColor = value;
				}
			}
			else
			{
				UtDebug.LogError("Text is not set");
			}
		}
	}

	public bool pCanReceiveInput
	{
		get
		{
			if (mRaycastCaptureGraphic != null)
			{
				return mRaycastCaptureGraphic.raycastTarget;
			}
			UtDebug.LogError("RaycastCaptureGraphic is not set. Default value returned");
			return false;
		}
		set
		{
			if (mRaycastCaptureGraphic != null)
			{
				mRaycastCaptureGraphic.raycastTarget = value;
			}
			else
			{
				UtDebug.LogError("RaycastCaptureGraphic is not set");
			}
		}
	}

	public void SetAnim2D(UIAnim2D anim2D)
	{
		mAnim2D = anim2D;
	}

	public void Anim2DAnimEnded(int animIdx)
	{
		if (base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnAnimEnd(this, animIdx);
		}
	}

	public void AttachToPointer(int fingerID = -1)
	{
		AttachToPointer(Vector2.zero, fingerID);
	}

	public void AttachToPointer(Vector2 pointerAttachOffset, int fingerID = -1)
	{
		if (!mIsAttachedToPointer)
		{
			mPreviousCanReceiveInput = pCanReceiveInput;
		}
		mIsAttachedToPointer = true;
		mFingerIDAttachedTo = fingerID;
		mPointerAttachOffset = pointerAttachOffset;
		pCanReceiveInput = false;
	}

	public void DetachFromPointer()
	{
		if (mIsAttachedToPointer)
		{
			pCanReceiveInput = mPreviousCanReceiveInput;
		}
		mIsAttachedToPointer = false;
		mFingerIDAttachedTo = -1;
	}

	public void ResetEffects()
	{
		ApplyEffect(null);
	}

	public void Reset()
	{
	}

	public void AddWidget(UIWidget widget)
	{
		if (widget.pParentWidget == null && widget.pParentUI != null)
		{
			widget.pParentUI.RemoveWidget(widget, destroy: false, removeReferences: false);
		}
		else if (widget.pParentWidget != null)
		{
			widget.pParentWidget.RemoveWidget(widget, destroy: false, removeReferences: false);
		}
		mChildWidgets.Add(widget);
		widget.transform.SetParent(_ProxyTransform);
		widget.pEventTarget = base.pEventTarget;
		widget.Initialize(base.pParentUI, this);
	}

	public void RemoveWidget(UIWidget widget, bool destroy = false, bool removeReferences = true)
	{
		if (widget.pParentWidget == this)
		{
			mChildWidgets.Remove(widget);
			widget.pParentWidget = null;
			if (destroy)
			{
				UnityEngine.Object.Destroy(widget.gameObject);
			}
			else if (removeReferences)
			{
				widget.transform.SetParent(null);
				widget.pEventTarget = null;
				widget.Initialize(null, null);
			}
		}
	}

	public void ClearChildren()
	{
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			UnityEngine.Object.Destroy(mChildWidget.gameObject);
		}
		mChildWidgets.Clear();
	}

	public UIWidget Duplicate(bool autoAddToSameParent = false)
	{
		UIWidget component = UnityEngine.Object.Instantiate(base.gameObject, autoAddToSameParent ? base.transform.parent : null).GetComponent<UIWidget>();
		if (autoAddToSameParent)
		{
			if (pParentWidget == null)
			{
				base.pParentUI.AddWidget(component);
			}
			else
			{
				pParentWidget.AddWidget(component);
			}
		}
		else
		{
			component.pEventTarget = null;
			component.Initialize(null, null);
		}
		return component;
	}

	public virtual void Initialize(UI parentUI, UIWidget parentWidget)
	{
		if (!mInitialized)
		{
			mInitialized = true;
			base.pRectTransform = base.transform as RectTransform;
			if (_ProxyTransform == null)
			{
				_ProxyTransform = base.pRectTransform;
			}
			mSelectable = GetComponent<Selectable>();
			mChildWidgets.Clear();
			mGraphicsStates.Clear();
			CacheWidgets(base.transform);
			mGraphicsStatesCached = true;
		}
		base.pParentUI = parentUI;
		pParentWidget = parentWidget;
		mState = _State;
		mVisible = _Visible;
		if (parentWidget != null)
		{
			mParentState = parentWidget.pStateInHierarchy;
			mParentVisible = parentWidget.pVisibleInHierarchy;
		}
		else if (parentUI != null)
		{
			mParentState = parentUI.pStateInHierarchy;
			mParentVisible = parentUI.pVisibleInHierarchy;
		}
		WidgetState previousStateInHierarchy = mStateInHierarchy;
		mStateInHierarchy = ((mState < mParentState) ? mState : mParentState);
		mVisibleInHierarchy = (mParentVisible ? mVisible : mParentVisible);
		OnStateInHierarchyChanged(previousStateInHierarchy, mStateInHierarchy);
		OnVisibleInHierarchyChanged(mVisibleInHierarchy);
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.pEventTarget = base.pEventTarget;
			mChildWidget.Initialize(base.pParentUI, this);
		}
	}

	protected void CacheWidgets(Transform cacheTransform)
	{
		mRaycastCaptureGraphic = GetComponent<NonDrawableGraphic>();
		int childCount = cacheTransform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = cacheTransform.GetChild(i);
			UIWidget component = child.GetComponent<UIWidget>();
			if (component != null)
			{
				mChildWidgets.Add(component);
				continue;
			}
			Graphic component2 = child.GetComponent<Graphic>();
			if (component2 != null && !(component2 is NonDrawableGraphic))
			{
				RectTransform rectTransform = component2.rectTransform;
				Image image = component2 as Image;
				GraphicState graphicState = new GraphicState(component2);
				graphicState.pOriginallyEnabled = component2.enabled || mSelectable == null;
				graphicState.pOriginalPosition = rectTransform.localPosition;
				graphicState.pOriginalScale = rectTransform.localScale;
				graphicState.pOriginalColor = component2.color;
				graphicState.pOriginalSprite = ((image != null) ? image.sprite : null);
				graphicState.pOriginalRotation = rectTransform.rotation.eulerAngles;
				mGraphicsStates.Add(graphicState);
			}
			if (child.childCount > 0)
			{
				CacheWidgets(child);
			}
		}
	}

	protected override void UpdateUnityComponents()
	{
		base.UpdateUnityComponents();
		if (mSelectable != null)
		{
			mSelectable.enabled = mVisibleInHierarchy && (mStateInHierarchy == WidgetState.INTERACTIVE || mStateInHierarchy == WidgetState.DISABLED);
			mSelectable.interactable = mStateInHierarchy == WidgetState.INTERACTIVE;
		}
		if (mRaycastCaptureGraphic != null)
		{
			mRaycastCaptureGraphic.enabled = mVisibleInHierarchy;
		}
	}

	protected override void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
	{
		UpdateUnityComponents();
		if (previousStateInHierarchy == WidgetState.DISABLED && mStateInHierarchy != 0)
		{
			SelectEffect();
		}
		else if ((previousStateInHierarchy != 0 && mStateInHierarchy == WidgetState.DISABLED) || (previousStateInHierarchy != WidgetState.NOT_INTERACTIVE && mStateInHierarchy == WidgetState.NOT_INTERACTIVE))
		{
			mIsPressed = false;
			mIsHovering = false;
			SelectEffect();
		}
	}

	protected override void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
	{
		UpdateUnityComponents();
		SelectEffect();
	}

	[ContextMenu("Fill Effect References")]
	protected virtual void FillAllEffectReferences()
	{
		Initialize(base.pParentUI, pParentWidget);
		FillEffectReferences(_ClickEffects);
		FillEffectReferences(_DisabledEffects);
		FillEffectReferences(_HoverEffects);
		FillEffectReferences(_PressEffects);
	}

	protected void FillEffectReferences(UIEffects effects)
	{
		effects._PositionEffect._ApplyTo = new UIEffects.PositionEffectData[mGraphicsStates.Count];
		effects._ScaleEffect._ApplyTo = new UIEffects.ScaleEffectData[mGraphicsStates.Count];
		effects._ColorEffect._ApplyTo = new UIEffects.ColorEffectData[mGraphicsStates.Count];
		effects._SpriteEffect._ApplyTo = new UIEffects.SpriteEffectData[mGraphicsStates.Count];
		effects._RotateEffect._ApplyTo = new UIEffects.RotationEffectData[mGraphicsStates.Count];
		for (int i = 0; i < mGraphicsStates.Count; i++)
		{
			effects._PositionEffect._ApplyTo[i] = new UIEffects.PositionEffectData
			{
				_Widget = mGraphicsStates[i].pGraphic
			};
			effects._ScaleEffect._ApplyTo[i] = new UIEffects.ScaleEffectData
			{
				_Widget = mGraphicsStates[i].pGraphic
			};
			effects._ColorEffect._ApplyTo[i] = new UIEffects.ColorEffectData
			{
				_Widget = mGraphicsStates[i].pGraphic
			};
			effects._SpriteEffect._ApplyTo[i] = new UIEffects.SpriteEffectData
			{
				_Widget = mGraphicsStates[i].pGraphic
			};
			effects._RotateEffect._ApplyTo[i] = new UIEffects.RotationEffectData
			{
				_Widget = mGraphicsStates[i].pGraphic
			};
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_RotationPerSecond != Vector3.zero)
		{
			base.transform.Rotate(_RotationPerSecond * Time.deltaTime);
		}
		if (mCurrentEffect != null && mCurrentEffectStartTime > 0f && mCurrentEffect._MaxDuration > 0f && Time.realtimeSinceStartup - mCurrentEffectStartTime > mCurrentEffect._MaxDuration)
		{
			ApplyEffect(null);
		}
		if (mIsAttachedToPointer && KAInput.pInstance != null)
		{
			Vector2? vector = KAInput.pInstance.pPrevMousePos;
			if (vector.HasValue)
			{
				base.transform.position = ScreenToUIPoint(vector.Value) + mPointerAttachOffset;
			}
		}
		if (mIsPressed && mIsHovering && base.pInteractableInHierarchy && base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnPressRepeated(this);
		}
	}

	public Vector2 ScreenToUIPoint(Vector3 pointerPosition)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.pParentUI.pRectTransform, pointerPosition, base.pParentUI.pCanvas.worldCamera, out var localPoint);
		return base.pParentUI.transform.TransformPoint(localPoint);
	}

	protected void ApplyEffect(UIEffects effect)
	{
		if (mCurrentEffect == effect)
		{
			return;
		}
		UIEffects uIEffects = mCurrentEffect;
		mCurrentEffect = effect;
		foreach (GraphicState graphicState in mGraphicsStates)
		{
			if (graphicState == null || !(graphicState.pGraphic != null))
			{
				continue;
			}
			Graphic pGraphic = graphicState.pGraphic;
			RectTransform rectTransform = graphicState.pGraphic.rectTransform;
			MaskableGraphic maskableGraphic = graphicState.pGraphic as MaskableGraphic;
			Image image = graphicState.pGraphic as Image;
			Vector3 localPosition = rectTransform.localPosition;
			Vector3 localScale = rectTransform.localScale;
			Vector3 eulerAngles = rectTransform.localRotation.eulerAngles;
			Color from = ((maskableGraphic != null) ? maskableGraphic.color : Color.white);
			Vector3 to = graphicState.pOriginalPosition;
			Vector3 to2 = graphicState.pOriginalScale;
			Vector3 to3 = graphicState.pOriginalRotation;
			Color to4 = graphicState.pOriginalColor;
			Sprite sprite = graphicState.pOriginalSprite;
			float durationOrSpeed = 0f;
			float durationOrSpeed2 = 0f;
			float durationOrSpeed3 = 0f;
			float durationOrSpeed4 = 0f;
			EaseType type = EaseType.Linear;
			EaseType type2 = EaseType.Linear;
			EaseType type3 = EaseType.Linear;
			EaseType type4 = EaseType.Linear;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			if (uIEffects != null)
			{
				if (uIEffects._PositionEffect._UseEffect)
				{
					UIEffects.PositionEffectData positionEffectData = Array.Find(uIEffects._PositionEffect._ApplyTo, (UIEffects.PositionEffectData x) => x._Widget == graphicState.pGraphic);
					if (positionEffectData != null)
					{
						durationOrSpeed = positionEffectData._Time;
						type = positionEffectData._PositionEffect;
						flag = true;
					}
				}
				if (uIEffects._ScaleEffect._UseEffect)
				{
					UIEffects.ScaleEffectData scaleEffectData = Array.Find(uIEffects._ScaleEffect._ApplyTo, (UIEffects.ScaleEffectData x) => x._Widget == graphicState.pGraphic);
					if (scaleEffectData != null)
					{
						durationOrSpeed2 = scaleEffectData._Time;
						type2 = scaleEffectData._ScaleEffect;
						flag2 = true;
					}
				}
				if (uIEffects._RotateEffect._UseEffect)
				{
					UIEffects.RotationEffectData rotationEffectData = Array.Find(uIEffects._RotateEffect._ApplyTo, (UIEffects.RotationEffectData x) => x._Widget == graphicState.pGraphic);
					if (rotationEffectData != null)
					{
						if (rotationEffectData._Rotate.z < 0f)
						{
							eulerAngles.z -= 360f;
						}
						durationOrSpeed3 = rotationEffectData._Time;
						type3 = rotationEffectData._RotateEffect;
						flag3 = true;
					}
				}
				if (uIEffects._ColorEffect._UseEffect)
				{
					UIEffects.ColorEffectData colorEffectData = Array.Find(uIEffects._ColorEffect._ApplyTo, (UIEffects.ColorEffectData x) => x._Widget == graphicState.pGraphic);
					if (colorEffectData != null)
					{
						durationOrSpeed4 = colorEffectData._Time;
						type4 = colorEffectData._ColorEffect;
						flag4 = true;
					}
				}
				if (uIEffects._SpriteEffect._UseEffect)
				{
					flag5 = true;
				}
			}
			if (mCurrentEffect != null)
			{
				mCurrentEffectStartTime = Time.realtimeSinceStartup;
				if (mCurrentEffect._PositionEffect._UseEffect)
				{
					UIEffects.PositionEffectData positionEffectData2 = Array.Find(mCurrentEffect._PositionEffect._ApplyTo, (UIEffects.PositionEffectData x) => x._Widget == graphicState.pGraphic);
					if (positionEffectData2 != null)
					{
						to = positionEffectData2._Offset;
						durationOrSpeed = positionEffectData2._Time;
						type = positionEffectData2._PositionEffect;
						flag = true;
					}
				}
				if (mCurrentEffect._ScaleEffect._UseEffect)
				{
					UIEffects.ScaleEffectData scaleEffectData2 = Array.Find(mCurrentEffect._ScaleEffect._ApplyTo, (UIEffects.ScaleEffectData x) => x._Widget == graphicState.pGraphic);
					if (scaleEffectData2 != null)
					{
						to2 = scaleEffectData2._Scale;
						durationOrSpeed2 = scaleEffectData2._Time;
						type2 = scaleEffectData2._ScaleEffect;
						flag2 = true;
					}
				}
				if (mCurrentEffect._RotateEffect._UseEffect)
				{
					UIEffects.RotationEffectData rotationEffectData2 = Array.Find(mCurrentEffect._RotateEffect._ApplyTo, (UIEffects.RotationEffectData x) => x._Widget == graphicState.pGraphic);
					if (rotationEffectData2 != null)
					{
						to3 = rotationEffectData2._Rotate;
						durationOrSpeed3 = rotationEffectData2._Time;
						type3 = rotationEffectData2._RotateEffect;
						flag3 = true;
					}
				}
				if (mCurrentEffect._ColorEffect._UseEffect)
				{
					UIEffects.ColorEffectData colorEffectData2 = Array.Find(mCurrentEffect._ColorEffect._ApplyTo, (UIEffects.ColorEffectData x) => x._Widget == graphicState.pGraphic);
					if (colorEffectData2 != null)
					{
						to4 = colorEffectData2._Color;
						durationOrSpeed4 = colorEffectData2._Time;
						type4 = colorEffectData2._ColorEffect;
						flag4 = true;
					}
				}
				if (mCurrentEffect._SpriteEffect._UseEffect)
				{
					UIEffects.SpriteEffectData spriteEffectData = Array.Find(mCurrentEffect._SpriteEffect._ApplyTo, (UIEffects.SpriteEffectData x) => x._Widget == graphicState.pGraphic);
					if (spriteEffectData != null)
					{
						sprite = spriteEffectData._Sprite;
						flag5 = true;
					}
				}
			}
			if (flag || flag2 || flag4 || flag5 || flag3)
			{
				JSGames.Tween.Tween.Stop(graphicState.pGraphic.gameObject);
			}
			if (flag)
			{
				JSGames.Tween.Tween.MoveLocalTo(pGraphic.gameObject, localPosition, to, new TweenParam(durationOrSpeed, type));
			}
			if (flag2)
			{
				JSGames.Tween.Tween.ScaleTo(pGraphic.gameObject, localScale, to2, new TweenParam(durationOrSpeed2, type2));
			}
			if (flag3)
			{
				JSGames.Tween.Tween.RotateLocalTo(pGraphic.gameObject, eulerAngles, to3, new TweenParam(durationOrSpeed3, type3));
			}
			if (flag4)
			{
				JSGames.Tween.Tween.ColorTo(pGraphic.gameObject, from, to4, new TweenParam(durationOrSpeed4, type4));
			}
			if (flag5 && image != null)
			{
				image.sprite = sprite;
			}
		}
	}

	protected void PlayOneTimeEffect(UIEffects effect)
	{
		if (mCurrentOneTimeEffect != null)
		{
			mCurrentOneTimeEffect.PlayParticle(isPlay: false);
			mCurrentOneTimeEffect.PlaySound(on: false);
		}
		if (effect != null)
		{
			effect.PlayParticle(isPlay: true);
			effect.PlaySound(on: true);
		}
		mCurrentOneTimeEffect = effect;
	}

	protected void StopOneTimeEffectIfPlaying(UIEffects effect)
	{
		if (mCurrentOneTimeEffect != null && mCurrentOneTimeEffect == effect)
		{
			mCurrentOneTimeEffect.PlayParticle(isPlay: false);
			mCurrentOneTimeEffect.PlaySound(on: false);
			mCurrentOneTimeEffect = null;
		}
	}

	protected virtual void SelectEffect()
	{
		if (mStateInHierarchy == WidgetState.DISABLED)
		{
			ApplyEffect(_DisabledEffects);
		}
		else if (mIsPressed)
		{
			ApplyEffect(_PressEffects);
		}
		else if (mIsHovering)
		{
			ApplyEffect(_HoverEffects);
		}
		else
		{
			ApplyEffect(null);
		}
	}

	public virtual void OnPointerClick(PointerEventData eventData)
	{
		if ((bool)KAInput.pInstance && KAInput.pInstance.IsPointerValid(eventData) && base.pInteractableInHierarchy && !mIsDragging)
		{
			PlayOneTimeEffect(_ClickEffects);
			if (base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnClick(this, eventData);
			}
		}
	}

	public virtual void OnPointerDown(PointerEventData eventData)
	{
		if ((bool)KAInput.pInstance && KAInput.pInstance.IsPointerValid(eventData) && base.pInteractableInHierarchy)
		{
			Tooltip.Show(this, show: false);
			mIsPressed = true;
			PlayOneTimeEffect(_PressEffects);
			SelectEffect();
			if (base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnPress(this, isPressed: true, eventData);
			}
		}
	}

	public virtual void OnPointerUp(PointerEventData eventData)
	{
		if ((bool)KAInput.pInstance && KAInput.pInstance.IsPointerValid(eventData))
		{
			mIsPressed = false;
			StopOneTimeEffectIfPlaying(_PressEffects);
			SelectEffect();
			if (base.pInteractableInHierarchy && base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnPress(this, isPressed: false, eventData);
			}
		}
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		if (base.pInteractableInHierarchy)
		{
			Tooltip.Show(this, show: true);
			mIsHovering = true;
			PlayOneTimeEffect(_HoverEffects);
			SelectEffect();
			if (base.pEventTarget != null)
			{
				base.pEventTarget.TriggerOnHover(this, isHovering: true, eventData);
			}
		}
		if (Singleton<UIManager>.pInstance != null)
		{
			Singleton<UIManager>.pInstance.SetGlobalMouseOverItem(eventData.pointerId, this);
		}
	}

	public virtual void OnPointerExit(PointerEventData eventData)
	{
		Tooltip.Show(this, show: false);
		mIsHovering = false;
		StopOneTimeEffectIfPlaying(_HoverEffects);
		SelectEffect();
		if (base.pInteractableInHierarchy && base.pEventTarget != null)
		{
			base.pEventTarget.TriggerOnHover(this, isHovering: false, eventData);
		}
		if (Singleton<UIManager>.pInstance != null)
		{
			Singleton<UIManager>.pInstance.SetGlobalMouseOverItem(eventData.pointerId, null);
		}
	}

	public void SetTextByID(int id, string defaultText)
	{
		pText = StringTable.GetStringData(id, defaultText);
	}

	public void SetImage(string assetPath)
	{
		if (_Background == null && _RawImageBackground == null)
		{
			UtDebug.LogError("_Background or _RawImageBackground must be set for SetImage to work on " + base.name);
		}
		else if (!string.IsNullOrEmpty(assetPath))
		{
			RsResourceManager.LoadAssetFromBundle(assetPath, ResourceLoadedEvent, typeof(Texture));
		}
		else
		{
			UtDebug.LogError("Missing Asset Path or Asset Name");
		}
	}

	private void ResourceLoadedEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Texture2D texture2D = (Texture2D)inObject;
			if (_Background != null)
			{
				_Background.sprite = Sprite.Create(texture2D, new Rect(0f, 0f, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 100f);
			}
			else if (_RawImageBackground != null)
			{
				_RawImageBackground.texture = texture2D;
			}
		}
	}

	public void SetDragging(bool isDragging)
	{
		mIsDragging = isDragging;
	}

	private void OnDisable()
	{
		if (Singleton<UIManager>.pInstance != null && Singleton<UIManager>.pInstance.pGlobalMouseOverItem != null && Singleton<UIManager>.pInstance.pGlobalMouseOverItem.Count > 0 && Singleton<UIManager>.pInstance.pGlobalMouseOverItem.ContainsValue(this))
		{
			int key = Singleton<UIManager>.pInstance.pGlobalMouseOverItem.First((KeyValuePair<int, UIWidget> x) => x.Value == this).Key;
			Singleton<UIManager>.pInstance.SetGlobalMouseOverItem(key, null);
		}
	}
}
