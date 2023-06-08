using System.IO;
using UnityEngine.Video;

public class KAMovie : KAWidget
{
	public bool _Offscreen;

	public KAWidget[] _DownLoadingSprites;

	private bool mMovieStarted;

	private bool mVideoPlayerPlaying;

	private string mMovieName = "";

	public string pMovieName
	{
		get
		{
			return mMovieName;
		}
		set
		{
			mMovieName = value;
		}
	}

	public void PlayVideoPlayer(string url)
	{
		VideoPlayer component = base.gameObject.GetComponent<VideoPlayer>();
		if (component != null)
		{
			url = Path.ChangeExtension(url, ".mp4");
			bool useLocalAsset = false;
			AssetVersion av = UtWWWAsync.WWWProcess.MakeGetVersionCall(url, out useLocalAsset);
			string url2 = UtWWWAsync.ProcessLocale(url, av, useLocalAsset);
			component.source = VideoSource.Url;
			if (UtUtilities.GetOSVersion() >= 14f)
			{
				component.audioOutputMode = VideoAudioOutputMode.Direct;
			}
			component.url = url2;
			component.loopPointReached += EndReached;
			component.prepareCompleted += Prepared;
			component.targetCameraAlpha = 1f;
			component.Prepare();
			mVideoPlayerPlaying = true;
			for (int i = 0; i < _DownLoadingSprites.Length; i++)
			{
				_DownLoadingSprites[i].SetVisibility(inVisible: true);
			}
		}
	}

	private void Prepared(VideoPlayer vPlayer)
	{
		vPlayer.Play();
		mMovieStarted = true;
		for (int i = 0; i < _DownLoadingSprites.Length; i++)
		{
			_DownLoadingSprites[i].SetVisibility(inVisible: false);
		}
	}

	private void EndReached(VideoPlayer vp)
	{
		mVideoPlayerPlaying = false;
	}

	public void VideoPlayerStop()
	{
		mVideoPlayerPlaying = false;
	}

	public bool IsPlaying()
	{
		return mVideoPlayerPlaying;
	}
}
