using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

public abstract class UIBase : MonoBehaviour
{
	[Serializable]
	public class GraphicState
	{
		public Graphic pGraphic { get; private set; }

		public bool pOriginallyEnabled { get; set; }

		public Vector3 pOriginalPosition { get; set; }

		public Color pOriginalColor { get; set; }

		public Vector3 pOriginalScale { get; set; }

		public Sprite pOriginalSprite { get; set; }

		public Vector3 pOriginalRotation { get; set; }

		public GraphicState(Graphic graphic)
		{
			pGraphic = graphic;
		}
	}

	[Tooltip("This is the transform on which all child elements are parented to")]
	public RectTransform _ProxyTransform;

	[SerializeField]
	protected bool _Visible = true;

	[SerializeField]
	protected WidgetState _State = WidgetState.INTERACTIVE;

	protected List<UIWidget> mChildWidgets = new List<UIWidget>();

	protected List<GraphicState> mGraphicsStates = new List<GraphicState>();

	protected bool mGraphicsStatesCached;

	protected bool mParentVisible = true;

	protected WidgetState mParentState = WidgetState.INTERACTIVE;

	protected bool mVisible = true;

	protected WidgetState mState = WidgetState.INTERACTIVE;

	protected bool mVisibleInHierarchy = true;

	protected WidgetState mStateInHierarchy = WidgetState.INTERACTIVE;

	public RectTransform pRectTransform { get; protected set; }

	public Vector3 pLocalPosition
	{
		get
		{
			return pRectTransform.localPosition;
		}
		set
		{
			pRectTransform.localPosition = value;
		}
	}

	public Vector3 pAnchoredPosition
	{
		get
		{
			return pRectTransform.anchoredPosition;
		}
		set
		{
			pRectTransform.anchoredPosition = value;
		}
	}

	public Vector3 pPosition
	{
		get
		{
			return pRectTransform.position;
		}
		set
		{
			pRectTransform.position = value;
		}
	}

	public Vector3 pLocalScale
	{
		get
		{
			return pRectTransform.localScale;
		}
		set
		{
			pRectTransform.localScale = value;
		}
	}

	public Vector2 pSize
	{
		get
		{
			return pRectTransform.sizeDelta;
		}
		set
		{
			pRectTransform.sizeDelta = value;
		}
	}

	public UIEvents pEventTarget { get; set; }

	public UI pParentUI { get; protected set; }

	public List<UIWidget> pChildWidgets => mChildWidgets;

	public bool pInteractable
	{
		get
		{
			if (mState == WidgetState.INTERACTIVE)
			{
				return mVisible;
			}
			return false;
		}
	}

	public bool pInteractableInHierarchy
	{
		get
		{
			if (mStateInHierarchy == WidgetState.INTERACTIVE)
			{
				return mVisibleInHierarchy;
			}
			return false;
		}
	}

	public bool pVisibleInHierarchy => mVisibleInHierarchy;

	public WidgetState pStateInHierarchy => mStateInHierarchy;

	public virtual bool pVisible
	{
		get
		{
			return mVisible;
		}
		set
		{
			_Visible = value;
			if (mVisible != value)
			{
				mVisible = value;
				UpdateVisibleInHierarchy();
				OnVisibleChanged(mVisible);
			}
		}
	}

	public virtual WidgetState pState
	{
		get
		{
			return mState;
		}
		set
		{
			_State = value;
			if (mState != value)
			{
				WidgetState previousState = mState;
				mState = value;
				OnStateChanged(previousState, mState);
				UpdateStateInHierarchy();
			}
		}
	}

	public void OnParentSetVisible(bool parentVisible)
	{
		if (mParentVisible != parentVisible)
		{
			mParentVisible = parentVisible;
			UpdateVisibleInHierarchy();
		}
	}

	public void OnParentSetState(WidgetState parentState)
	{
		if (mParentState != parentState)
		{
			mParentState = parentState;
			UpdateStateInHierarchy();
		}
	}

	public Vector2 GetScreenPosition()
	{
		return pPosition;
	}

	public UIWidget FindWidget(string widgetName, bool recursive = true)
	{
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			if (mChildWidget.name == widgetName)
			{
				return mChildWidget;
			}
			if (recursive)
			{
				UIWidget uIWidget = mChildWidget.FindWidget(widgetName, recursive);
				if (uIWidget != null)
				{
					return uIWidget;
				}
			}
		}
		return null;
	}

	public int FindWidgetIndex(UIWidget widget)
	{
		return pChildWidgets.IndexOf(widget);
	}

	public UIWidget GetWidgetAt(int index)
	{
		if (index < pChildWidgets.Count)
		{
			return pChildWidgets[index];
		}
		return null;
	}

	protected GraphicState FindGraphicState(Graphic graphic)
	{
		return mGraphicsStates.Find((GraphicState x) => x.pGraphic == graphic);
	}

	protected virtual void OnStateChanged(WidgetState previousState, WidgetState newState)
	{
	}

	protected virtual void OnVisibleChanged(bool newVisible)
	{
	}

	protected virtual void OnVisibleInHierarchyChanged(bool newVisibleInHierarchy)
	{
	}

	protected virtual void OnStateInHierarchyChanged(WidgetState previousStateInHierarchy, WidgetState newStateInHierarchy)
	{
	}

	protected virtual void UpdateVisibleInHierarchy()
	{
		bool num = mVisibleInHierarchy;
		mVisibleInHierarchy = (mParentVisible ? mVisible : mParentVisible);
		if (num == mVisibleInHierarchy)
		{
			return;
		}
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.OnParentSetVisible(mVisibleInHierarchy);
		}
		OnVisibleInHierarchyChanged(mVisibleInHierarchy);
	}

	protected virtual void UpdateStateInHierarchy()
	{
		WidgetState widgetState = mStateInHierarchy;
		mStateInHierarchy = ((mState < mParentState) ? mState : mParentState);
		if (widgetState == mStateInHierarchy)
		{
			return;
		}
		foreach (UIWidget mChildWidget in mChildWidgets)
		{
			mChildWidget.OnParentSetState(mStateInHierarchy);
		}
		OnStateInHierarchyChanged(widgetState, mStateInHierarchy);
	}

	protected virtual void Start()
	{
		SetParamsFromPublicVariables();
	}

	protected virtual void Update()
	{
	}

	protected virtual void SetParamsFromPublicVariables()
	{
		if (_State != pState)
		{
			pState = _State;
		}
		if (_Visible != pVisible)
		{
			pVisible = _Visible;
		}
	}

	protected virtual void UpdateUnityComponents()
	{
		if (!mVisibleInHierarchy)
		{
			foreach (GraphicState mGraphicsState in mGraphicsStates)
			{
				if (mGraphicsState != null && mGraphicsState.pGraphic != null)
				{
					if (!mGraphicsStatesCached)
					{
						mGraphicsState.pOriginallyEnabled = mGraphicsState.pGraphic.enabled;
					}
					mGraphicsState.pGraphic.enabled = false;
				}
			}
			mGraphicsStatesCached = true;
		}
		else
		{
			if (!mVisibleInHierarchy || !mGraphicsStatesCached)
			{
				return;
			}
			foreach (GraphicState mGraphicsState2 in mGraphicsStates)
			{
				if (mGraphicsState2 != null && mGraphicsState2.pGraphic != null)
				{
					mGraphicsState2.pGraphic.enabled = mGraphicsState2.pOriginallyEnabled;
				}
			}
			mGraphicsStatesCached = false;
		}
	}
}
