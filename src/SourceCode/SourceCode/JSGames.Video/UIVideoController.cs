using System;
using JSGames.Tween;
using JSGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace JSGames.Video;

public class UIVideoController : JSGames.UI.UI
{
	public UIToggleButton _PlayPauseToggleButton;

	public JSGames.UI.UIButton _PlaybackSpeedUp;

	public JSGames.UI.UIButton _PlaybackSpeedDown;

	public JSGames.UI.UIButton _NextButton;

	public JSGames.UI.UIButton _PrevButton;

	public UIToggleButton _FullScreenButton;

	public JSGames.UI.UIButton _CloseButton;

	public VideoSlider _ProgressSlider;

	public Slider _VolumeSlider;

	public Text _TimeElapsedText;

	public Text _TimeRemainText;

	public GameObject _DefaultScreenDisplay;

	public GameObject _FullScreenDisplay;

	public VideoController _VideoController;

	public Action<VideoPlayer> OnVideoPlayerClosed;

	public Action OnNextButtonClicked;

	public Action OnPrevButtonClicked;

	public bool _SetExclusive;

	[SerializeField]
	private GameObject m_ControlsAnchor;

	[SerializeField]
	private GameObject m_ControlsAnchorFullScreen;

	[SerializeField]
	private GameObject m_ControlsAnchorOffscreen;

	[SerializeField]
	private GameObject m_ControlsBackground;

	[SerializeField]
	private TweenParam m_ControlsTweenParam;

	[SerializeField]
	private float m_ControlsHideDuration = 5f;

	private bool mFullScreen;

	private Vector3 mControlsLocalPositionNormalScreen;

	private float mControlsHideTimer;

	private bool mControlsVisible = true;

	private float mControlHeight;

	protected override void OnInitialize()
	{
		base.OnInitialize();
		VideoController videoController = _VideoController;
		videoController.OnInitialized = (Action)Delegate.Combine(videoController.OnInitialized, new Action(VideoControllerInitialized));
		if (_VolumeSlider != null)
		{
			_VolumeSlider.onValueChanged.AddListener(delegate
			{
				UpdateVolume();
			});
		}
		VideoSlider progressSlider = _ProgressSlider;
		progressSlider.pOnSliderDrag = (Action<float>)Delegate.Combine(progressSlider.pOnSliderDrag, new Action<float>(_VideoController.Seek));
		if (_SetExclusive)
		{
			SetExclusive();
		}
	}

	protected override void Start()
	{
		base.Start();
		mControlsLocalPositionNormalScreen = m_ControlsAnchor.transform.localPosition;
		mControlHeight = (m_ControlsBackground.transform as RectTransform).sizeDelta.y;
	}

	protected override void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape) && JSGames.UI.UI.mOnClickFrameCount != Time.frameCount && (JSGames.UI.UI._GlobalExclusiveUI == null || JSGames.UI.UI._GlobalExclusiveUI == this) && _BackButton != null && _BackButton.pInteractableInHierarchy && _BackButton.pVisible && _BackButton.pState == WidgetState.INTERACTIVE && _BackButton.gameObject.activeSelf)
		{
			if (_BackButton == _FullScreenButton)
			{
				_FullScreenButton.pChecked = !_FullScreenButton.pChecked;
			}
			JSGames.UI.UI.mOnClickFrameCount = Time.frameCount;
			OnClick(_BackButton, null);
		}
		if (_VideoController.pIsPrepared)
		{
			if (_ProgressSlider != null)
			{
				_ProgressSlider.pSliderPosition = _VideoController.pNormalizedTime;
			}
			if (_TimeElapsedText != null)
			{
				_ = _TimeRemainText != null;
			}
		}
		if (mFullScreen && mControlsVisible && _PlayPauseToggleButton.pChecked)
		{
			mControlsHideTimer -= Time.deltaTime;
			if (mControlsHideTimer <= 0f)
			{
				mControlsVisible = false;
				UpdateControlPosition(showControls: false);
			}
		}
		if (Input.anyKey || Input.touchCount > 0 || KAInput.pInstance.pMouseMoved)
		{
			mControlsHideTimer = m_ControlsHideDuration;
			if (!mControlsVisible)
			{
				mControlsVisible = true;
				UpdateControlPosition();
			}
		}
	}

	protected override void OnClick(JSGames.UI.UIWidget widget, PointerEventData eventData)
	{
		base.OnClick(widget, eventData);
		if (widget == _PlayPauseToggleButton)
		{
			if (_PlayPauseToggleButton.pChecked)
			{
				_VideoController.Play();
			}
			else
			{
				_VideoController.Pause();
			}
		}
		else if (widget == _FullScreenButton)
		{
			DoFullScreen(_FullScreenButton.pChecked);
		}
		else if (widget == _PlaybackSpeedUp)
		{
			_VideoController.IncreasePlaybackSpeed(0.15f);
		}
		else if (widget == _PlaybackSpeedDown)
		{
			_VideoController.DecreasePlaybackSpeed(0.15f);
		}
		else if (widget == _NextButton)
		{
			if (GetClipCount() > 1)
			{
				_VideoController.PlayNextClip();
				UpdateVideoNavigationButtons();
			}
			OnNextButtonClicked?.Invoke();
		}
		else if (widget == _PrevButton)
		{
			if (GetClipCount() > 1)
			{
				_VideoController.PlayPreviousClip();
				UpdateVideoNavigationButtons();
			}
			OnPrevButtonClicked?.Invoke();
		}
		else if (widget == _CloseButton)
		{
			StopVideoPlayer();
		}
	}

	public void DoFullScreen(bool fullScreen)
	{
		mFullScreen = fullScreen;
		mControlsVisible = true;
		_FullScreenDisplay.SetActive(fullScreen);
		_FullScreenButton.pChecked = fullScreen;
		_DefaultScreenDisplay.SetActive(!fullScreen);
		_BackButton = (fullScreen ? _FullScreenButton : _CloseButton);
		m_ControlsBackground.SetActive(mFullScreen);
		UpdateControlPosition();
		if (mFullScreen)
		{
			mControlsHideTimer = m_ControlsHideDuration;
		}
		if (!_SetExclusive)
		{
			if (mFullScreen)
			{
				SetExclusive();
			}
			else
			{
				RemoveExclusive();
			}
		}
	}

	private void UpdateControlPosition(bool showControls = true)
	{
		if (!mFullScreen)
		{
			JSGames.Tween.Tween.Stop(m_ControlsAnchor);
			m_ControlsAnchor.transform.localPosition = mControlsLocalPositionNormalScreen;
		}
		else
		{
			JSGames.Tween.Tween.MoveTo(m_ControlsAnchor, m_ControlsAnchor.transform.position, showControls ? (m_ControlsAnchorFullScreen.transform.position + new Vector3(0f, mControlHeight / 2f, 0f)) : (m_ControlsAnchorOffscreen.transform.position - new Vector3(0f, mControlHeight, 0f)), m_ControlsTweenParam);
		}
	}

	protected override void OnVisibleChanged(bool visible)
	{
		base.OnVisibleChanged(visible);
		if (_SetExclusive)
		{
			if (visible)
			{
				SetExclusive();
			}
			else
			{
				RemoveExclusive();
			}
		}
	}

	private void StopVideoPlayer()
	{
		_VideoController.Stop();
		pVisible = false;
		OnVideoPlayerClosed?.Invoke(_VideoController.pVideoPlayer);
	}

	private void UpdateVolume()
	{
		_VideoController.SetVolume(_VolumeSlider.normalizedValue);
	}

	private void VideoControllerInitialized()
	{
		if (_VideoController.pVideoPlayer.playOnAwake)
		{
			if (_PlayPauseToggleButton != null)
			{
				_PlayPauseToggleButton.pChecked = true;
			}
			_VideoController.Play();
		}
		if (GetClipCount() > 1)
		{
			UpdateVideoNavigationButtons();
		}
	}

	private void UpdateVideoNavigationButtons()
	{
		int clipCount = GetClipCount();
		_NextButton.pState = ((_VideoController.pVideoClipIndex < clipCount - 1) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_PrevButton.pState = ((_VideoController.pVideoClipIndex > 0) ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
	}

	public void UpdateVideoNavigationButtons(bool enablePrev, bool enableNext)
	{
		_NextButton.pState = (enableNext ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
		_PrevButton.pState = (enablePrev ? WidgetState.INTERACTIVE : WidgetState.DISABLED);
	}

	private int GetClipCount()
	{
		int result = 0;
		switch (_VideoController.pVideoPlayer.source)
		{
		case VideoSource.Url:
			result = _VideoController._VideoClipsUrl.Count;
			break;
		case VideoSource.VideoClip:
			result = _VideoController._VideoClips.Count;
			break;
		}
		return result;
	}

	public bool IsFullScreen()
	{
		return mFullScreen;
	}
}
