public class KAUIMoviePlayer : KAUI
{
	private KAMovie mMovieWidget;

	private KAWidget mSkipBtn;

	public void Play(string movieName, bool skipMovie = false)
	{
		if (mMovieWidget == null)
		{
			mMovieWidget = (KAMovie)FindItem("Movie");
		}
		if (mSkipBtn == null)
		{
			mSkipBtn = FindItem("SkipBtn");
			if (mSkipBtn != null)
			{
				mSkipBtn.SetVisibility(skipMovie);
			}
		}
		if (mMovieWidget != null)
		{
			mMovieWidget.pMovieName = movieName;
			mMovieWidget.PlayVideoPlayer(movieName);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mMovieWidget != null && inWidget == mSkipBtn)
		{
			mSkipBtn.SetVisibility(inVisible: false);
			mMovieWidget.VideoPlayerStop();
		}
	}

	public bool IsPlaying()
	{
		if (mMovieWidget != null)
		{
			return mMovieWidget.IsPlaying();
		}
		return false;
	}
}
