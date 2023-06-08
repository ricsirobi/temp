using System.Collections;
using UnityEngine;

public class MovieManager : MonoBehaviour
{
	public delegate void MovieCallback();

	public string _MoviePlayerBundleName = "RS_DATA/PfUiMoviePlayerDO.unity3d/PfUiMoviePlayerDO";

	public string _MovieDataFileName = "RS_DATA/MovieDataDO.xml";

	public float _MovieFinishDelayTime = 1f;

	private static MovieManager mInstance;

	private static bool mBackgroundColorSet;

	private static Color mBackgroundColor;

	private bool mSkipMovie;

	private bool mDelayedFinish;

	private bool? mPreviousCursorVisibility;

	private KAUIMoviePlayer mMoviePlayer;

	private string mType = "";

	private Movie mMovie;

	private bool mPlayMovie;

	private bool mTurnOffMusicGroup;

	private bool mTurnOffSoundGroup;

	public static MovieManager pInstance => mInstance;

	private event MovieCallback OnMoviePlayed;

	private event MovieCallback OnMovieStarted;

	public void Update()
	{
		if (MovieDataList.pIsReady && mPlayMovie)
		{
			mPlayMovie = false;
			mMovie = MovieDataList.GetMovie(mType);
			if (mMovie == null || string.IsNullOrEmpty(_MoviePlayerBundleName))
			{
				OnMovieFinished();
				return;
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _MoviePlayerBundleName.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MoviePlayerBundleReady, typeof(GameObject));
		}
		if (mMoviePlayer != null && !mMoviePlayer.IsPlaying() && !mDelayedFinish)
		{
			if (UtPlatform.IsMobile())
			{
				OnMovieFinished();
			}
			else
			{
				OnMovieFinished(_MovieFinishDelayTime);
			}
		}
	}

	private static void CreatInstance()
	{
		if (mInstance != null)
		{
			return;
		}
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfMovieManager");
		if (gameObject != null)
		{
			gameObject = Object.Instantiate(gameObject);
			if (gameObject != null)
			{
				mInstance = gameObject.GetComponent<MovieManager>();
			}
		}
	}

	public static void SetBackgroundColor(Color bgColor)
	{
		mBackgroundColor = bgColor;
		mBackgroundColorSet = true;
	}

	public static void Play(string inType, MovieCallback inStartedCallback, MovieCallback inPlayedCallback, bool skipMovie = false)
	{
		if (string.IsNullOrEmpty(inType))
		{
			inPlayedCallback?.Invoke();
			return;
		}
		CreatInstance();
		if (mInstance != null)
		{
			mInstance.mSkipMovie = skipMovie;
			mInstance.OnMovieStarted += inStartedCallback;
			mInstance.OnMoviePlayed += inPlayedCallback;
			if (inType.StartsWith("http") || inType.StartsWith("RS_MOVIES"))
			{
				mInstance.PlayFromURL(inType);
			}
			else
			{
				mInstance.PlayFromType(inType);
			}
		}
		else
		{
			inPlayedCallback?.Invoke();
		}
	}

	private void PlayFromType(string inType)
	{
		MovieDataList.Init(_MovieDataFileName);
		mType = inType;
		mPlayMovie = true;
	}

	public void PlayFromURL(string inURL)
	{
		mMovie = new Movie();
		mMovie.Name = inURL;
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _MoviePlayerBundleName.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], MoviePlayerBundleReady, typeof(GameObject));
	}

	private void MoviePlayerBundleReady(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (inObject != null)
			{
				GameObject gameObject = Object.Instantiate((GameObject)inObject);
				mMoviePlayer = gameObject.GetComponent<KAUIMoviePlayer>();
				mTurnOffMusicGroup = SnChannel.pTurnOffMusicGroup;
				mTurnOffSoundGroup = SnChannel.pTurnOffSoundGroup;
				SnChannel.pTurnOffMusicGroup = true;
				SnChannel.pTurnOffSoundGroup = true;
				if (mBackgroundColorSet)
				{
					KAUI.SetExclusive(mMoviePlayer, mBackgroundColor);
				}
				if (mMoviePlayer != null)
				{
					mMoviePlayer.Play(mMovie.Name, mSkipMovie);
					mPreviousCursorVisibility = UICursorManager.pVisibility;
					UICursorManager.pVisibility = true;
					if (this.OnMovieStarted != null)
					{
						this.OnMovieStarted();
						this.OnMovieStarted = null;
					}
				}
				else
				{
					OnMovieFinished();
				}
			}
			else
			{
				OnMovieFinished();
			}
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			OnMovieFinished();
			break;
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != WsServiceEvent.COMPLETE)
		{
			_ = 3;
		}
		else if (inObject != null)
		{
			AchievementReward[] array = (AchievementReward[])inObject;
			if (array != null)
			{
				GameUtilities.AddRewards(array);
			}
		}
	}

	private IEnumerator DelayMovieFinish(float delay)
	{
		yield return new WaitForSeconds(delay);
		mDelayedFinish = false;
		if (mMoviePlayer != null)
		{
			SnChannel.pTurnOffMusicGroup = mTurnOffMusicGroup;
			SnChannel.pTurnOffSoundGroup = mTurnOffSoundGroup;
			if (mBackgroundColorSet)
			{
				KAUI.RemoveExclusive(mMoviePlayer);
				mBackgroundColorSet = false;
			}
			Object.Destroy(mMoviePlayer.gameObject);
			mMoviePlayer = null;
			if (mMovie != null)
			{
				if (mMovie.Achievement.HasValue && mMovie.Achievement.Value > 0)
				{
					WsWebService.SetAchievementAndGetReward(mMovie.Achievement.Value, "", ServiceEventHandler, null);
				}
				if (!string.IsNullOrEmpty(mMovie.Name))
				{
					ProductData.OnMovieSeen(mMovie.Name);
				}
			}
			if (mPreviousCursorVisibility.HasValue)
			{
				UICursorManager.pVisibility = mPreviousCursorVisibility.Value;
			}
		}
		if (this.OnMoviePlayed != null)
		{
			this.OnMoviePlayed();
			this.OnMoviePlayed = null;
		}
	}

	public void OnMovieFinished(float delayTime = 0f)
	{
		mDelayedFinish = delayTime != 0f;
		StartCoroutine(DelayMovieFinish(delayTime));
	}
}
