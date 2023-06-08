using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace JSGames.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoController : MonoBehaviour
{
	public Action OnInitialized;

	public Action<VideoPlayer, string> OnErrorReceived;

	public Action<VideoPlayer> OnEndPointReached;

	public Action<VideoPlayer> OnSeekCompleted;

	public Action<VideoPlayer> OnStarted;

	public List<VideoClip> _VideoClips = new List<VideoClip>();

	public List<string> _VideoClipsUrl = new List<string>();

	public bool _InitializeOnAwake;

	private VideoPlayer mVideoPlayer;

	private AudioSource mAudioSource;

	private int mVideoClipIndex;

	protected int mCachedSleepTime;

	public bool pIsPlaying => mVideoPlayer.isPlaying;

	public bool pIsLooping => mVideoPlayer.isLooping;

	public bool pIsPrepared => mVideoPlayer.isPrepared;

	public float pTimeElapsed => (float)mVideoPlayer.time;

	public float pTimeRemains => pDuration - pTimeElapsed;

	public float pDuration => (ulong)((float)mVideoPlayer.frameCount / mVideoPlayer.frameRate);

	public float pNormalizedTime => Mathf.Clamp01((float)(mVideoPlayer.time / (double)pDuration));

	public int pVideoClipIndex
	{
		get
		{
			return mVideoClipIndex;
		}
		private set
		{
			mVideoClipIndex = value;
			int num = ((mVideoPlayer.source == VideoSource.VideoClip) ? _VideoClips.Count : _VideoClipsUrl.Count);
			if (mVideoClipIndex >= num)
			{
				mVideoClipIndex = 0;
			}
			else if (mVideoClipIndex < 0)
			{
				mVideoClipIndex = num - 1;
			}
		}
	}

	public VideoPlayer pVideoPlayer
	{
		get
		{
			if (mVideoPlayer == null)
			{
				mVideoPlayer = GetComponent<VideoPlayer>();
			}
			return mVideoPlayer;
		}
	}

	private void Awake()
	{
		mVideoPlayer = GetComponent<VideoPlayer>();
		mCachedSleepTime = -2;
		if (mVideoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
		{
			mAudioSource = GetComponent<AudioSource>();
		}
		if (_InitializeOnAwake)
		{
			Init(mVideoPlayer.source, mVideoPlayer.renderMode);
		}
		RegisterEvents();
	}

	private void OnDestroy()
	{
		Screen.sleepTimeout = mCachedSleepTime;
		UnRegisterEvents();
	}

	private void RegisterEvents()
	{
		mVideoPlayer.errorReceived += ErrorReceived;
		mVideoPlayer.loopPointReached += LoopPointReached;
		mVideoPlayer.prepareCompleted += OnPrepareCompleted;
		mVideoPlayer.seekCompleted += SeekCompleted;
		mVideoPlayer.started += Started;
	}

	private void UnRegisterEvents()
	{
		mVideoPlayer.errorReceived -= ErrorReceived;
		mVideoPlayer.loopPointReached -= LoopPointReached;
		mVideoPlayer.prepareCompleted -= OnPrepareCompleted;
		mVideoPlayer.seekCompleted -= SeekCompleted;
		mVideoPlayer.started -= Started;
	}

	public void Init(VideoRenderMode videoRenderMode, List<VideoClip> videoClips)
	{
		if (videoClips != null)
		{
			_VideoClips = videoClips;
			if (videoClips.Count > 0)
			{
				mVideoPlayer.clip = videoClips[0];
			}
		}
		Init(VideoSource.VideoClip, videoRenderMode);
	}

	public void Init(VideoRenderMode videoRenderMode, List<string> videoClipsUrl)
	{
		if (videoClipsUrl != null)
		{
			_VideoClipsUrl = videoClipsUrl;
			if (videoClipsUrl.Count > 0)
			{
				mVideoPlayer.url = videoClipsUrl[0];
			}
		}
		Init(VideoSource.Url, videoRenderMode);
	}

	private void Init(VideoSource videoSource, VideoRenderMode videoRenderMode)
	{
		Screen.sleepTimeout = -1;
		mVideoPlayer.source = videoSource;
		mVideoPlayer.renderMode = videoRenderMode;
		if (videoRenderMode == VideoRenderMode.RenderTexture)
		{
			mVideoPlayer.targetTexture.Release();
		}
		if (OnInitialized != null)
		{
			OnInitialized();
		}
	}

	public void Play(string name)
	{
		Play(GetClipIndex(name));
	}

	public void Play()
	{
		Play(pVideoClipIndex);
	}

	public void Play(int index)
	{
		if (CanResume(index))
		{
			if (!pIsPlaying)
			{
				pVideoPlayer.Play();
			}
			return;
		}
		pVideoClipIndex = index;
		if (mVideoPlayer.source == VideoSource.VideoClip)
		{
			mVideoPlayer.clip = GetClipAtIndex(pVideoClipIndex);
		}
		else
		{
			mVideoPlayer.url = GetUrlAtIndex(pVideoClipIndex);
		}
		Stop();
		PreparePlayer();
	}

	private bool CanResume(int index)
	{
		if (pVideoClipIndex != index)
		{
			return false;
		}
		if (!pIsPlaying)
		{
			if (mVideoPlayer.source == VideoSource.VideoClip)
			{
				return mVideoPlayer.clip != null;
			}
			return !string.IsNullOrEmpty(mVideoPlayer.url);
		}
		return false;
	}

	public void Stop()
	{
		if (mVideoPlayer.canSetPlaybackSpeed)
		{
			mVideoPlayer.playbackSpeed = 1f;
		}
		mVideoPlayer.Stop();
	}

	public void PlayNextClip()
	{
		Play(pVideoClipIndex + 1);
	}

	public void PlayPreviousClip()
	{
		Play(pVideoClipIndex - 1);
	}

	public void Pause()
	{
		if (pIsPlaying)
		{
			mVideoPlayer.Pause();
		}
	}

	public void Restart()
	{
		if (pIsPrepared)
		{
			Stop();
			mVideoPlayer.Play();
		}
	}

	private void PreparePlayer()
	{
		if (mVideoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource)
		{
			mVideoPlayer.SetTargetAudioSource(0, mAudioSource);
		}
		mVideoPlayer.Prepare();
	}

	public void Loop(bool isLooping)
	{
		if (pIsPrepared)
		{
			mVideoPlayer.isLooping = isLooping;
		}
	}

	public void Seek(float normalizedTime)
	{
		if (pIsPrepared && mVideoPlayer.canSetTime)
		{
			mVideoPlayer.time = Mathf.Clamp01(normalizedTime) * pDuration;
		}
	}

	public void IncreasePlaybackSpeed(float incrementfactor)
	{
		if (mVideoPlayer.canSetPlaybackSpeed)
		{
			mVideoPlayer.playbackSpeed = Mathf.Clamp(mVideoPlayer.playbackSpeed + incrementfactor, 0f, 10f);
		}
	}

	public void DecreasePlaybackSpeed(float decrementfactor)
	{
		if (mVideoPlayer.canSetPlaybackSpeed)
		{
			mVideoPlayer.playbackSpeed = Mathf.Clamp(mVideoPlayer.playbackSpeed - decrementfactor, 0f, 10f);
		}
	}

	public void Rewind(float rewindSpeed)
	{
		if (pIsPrepared && mVideoPlayer.canSetTime)
		{
			mVideoPlayer.time = Mathf.Clamp((float)(mVideoPlayer.time - (double)rewindSpeed), 0f, pDuration);
		}
	}

	public void IncreaseVolume(float incrementfactor)
	{
		if (mVideoPlayer.audioOutputMode == VideoAudioOutputMode.Direct && mVideoPlayer.canSetDirectAudioVolume)
		{
			mVideoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(mVideoPlayer.GetDirectAudioVolume(0) + incrementfactor));
		}
		else
		{
			mAudioSource.volume = Mathf.Clamp01(mAudioSource.volume + incrementfactor);
		}
	}

	public void DecreaseVolume(float decrementfactor)
	{
		if (mVideoPlayer.audioOutputMode == VideoAudioOutputMode.Direct && mVideoPlayer.canSetDirectAudioVolume)
		{
			mVideoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(mVideoPlayer.GetDirectAudioVolume(0) - decrementfactor));
		}
		else
		{
			mAudioSource.volume = Mathf.Clamp01(mAudioSource.volume - decrementfactor);
		}
	}

	public void SetVolume(float val)
	{
		if (pIsPrepared)
		{
			if (mVideoPlayer.audioOutputMode == VideoAudioOutputMode.Direct && mVideoPlayer.canSetDirectAudioVolume)
			{
				mVideoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(val));
			}
			else
			{
				mAudioSource.volume = Mathf.Clamp01(val);
			}
		}
	}

	private int GetClipIndex(string clipName)
	{
		if (mVideoPlayer.source == VideoSource.VideoClip)
		{
			return _VideoClips.FindIndex((VideoClip clip) => clip.name.Equals(clip));
		}
		return _VideoClipsUrl.FindIndex((string ele) => ele.Equals(clipName));
	}

	private VideoClip GetClipAtIndex(int index)
	{
		if (index != -1 && index < _VideoClips.Count)
		{
			return _VideoClips[index];
		}
		return null;
	}

	private string GetUrlAtIndex(int index)
	{
		if (index != -1 && index < _VideoClipsUrl.Count)
		{
			return _VideoClipsUrl[index];
		}
		return null;
	}

	private void OnPrepareCompleted(VideoPlayer player)
	{
		mVideoPlayer.Play();
	}

	private void ErrorReceived(VideoPlayer videoPlayer, string error)
	{
		if (OnErrorReceived != null)
		{
			OnErrorReceived(videoPlayer, error);
		}
	}

	private void LoopPointReached(VideoPlayer videoPlayer)
	{
		if (OnEndPointReached != null)
		{
			OnEndPointReached(videoPlayer);
		}
	}

	private void SeekCompleted(VideoPlayer videoPlayer)
	{
		if (OnSeekCompleted != null)
		{
			OnSeekCompleted(videoPlayer);
		}
	}

	private void Started(VideoPlayer videoPlayer)
	{
		if (OnStarted != null)
		{
			OnStarted(videoPlayer);
		}
	}
}
