using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine.SceneManagement;

[Serializable]
[XmlRoot("MovieDataList")]
public class MovieDataList
{
	[XmlElement(ElementName = "MovieData")]
	public MovieData[] MovieData;

	private static MovieDataList mMovieDataList = null;

	private static string mMovieDataFilename = string.Empty;

	public static bool pIsReady => mMovieDataList != null;

	public static void Init(string inFilename)
	{
		if (mMovieDataList == null)
		{
			mMovieDataFilename = inFilename;
			RsResourceManager.Load(mMovieDataFilename, XmlLoadEventHandler, RsResourceType.NONE, inDontDestroy: true);
		}
	}

	private static void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			mMovieDataList = UtUtilities.DeserializeFromXml((string)inObject, typeof(MovieDataList)) as MovieDataList;
		}
		else if (inEvent.Equals(RsResourceLoadEvent.ERROR))
		{
			UtDebug.LogError("Could not load xml: " + mMovieDataFilename);
		}
	}

	public static Movie GetMovie(string inType)
	{
		List<Movie> allMatchingMovies = mMovieDataList.GetAllMatchingMovies(inType);
		Movie result = null;
		int num = int.MaxValue;
		foreach (Movie item in allMatchingMovies)
		{
			int numberOfTimesMovieSeen = ProductData.GetNumberOfTimesMovieSeen(item.Name);
			if (numberOfTimesMovieSeen == 0)
			{
				result = item;
				break;
			}
			int num2 = -1;
			if (item.Repeat.HasValue)
			{
				num2 = item.Repeat.Value;
			}
			if ((num2 == -1 || numberOfTimesMovieSeen < num2) && numberOfTimesMovieSeen < num)
			{
				num = numberOfTimesMovieSeen;
				result = item;
			}
		}
		return result;
	}

	private List<Movie> GetAllMatchingMovies(string inType)
	{
		List<Movie> list = new List<Movie>();
		if (mMovieDataList.MovieData != null)
		{
			for (int i = 0; i < mMovieDataList.MovieData.Length; i++)
			{
				if (!IsTypeMatching(mMovieDataList.MovieData[i], inType))
				{
					continue;
				}
				if (inType.ToLower() == "enterscene")
				{
					Movie[] movie = mMovieDataList.MovieData[i].Movie;
					foreach (Movie movie2 in movie)
					{
						if (IsCorrectScene(movie2.SceneName))
						{
							list.Add(movie2);
						}
					}
				}
				else
				{
					for (int k = 0; k < mMovieDataList.MovieData[i].Movie.Length; k++)
					{
						list.Add(mMovieDataList.MovieData[i].Movie[k]);
					}
				}
				break;
			}
		}
		return list;
	}

	private bool IsCorrectScene(string movieSceneName)
	{
		if (!string.IsNullOrEmpty(movieSceneName))
		{
			string text = movieSceneName.ToLower();
			string text2 = SceneManager.GetActiveScene().name.ToLower();
			return text == text2;
		}
		return false;
	}

	private bool IsTypeMatching(MovieData movieData, string inType)
	{
		string text = movieData.Type.ToLower();
		string text2 = inType.ToLower();
		return text == text2;
	}
}
