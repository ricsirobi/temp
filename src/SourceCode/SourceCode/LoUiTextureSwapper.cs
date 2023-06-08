using System.Collections;
using UnityEngine;

public class LoUiTextureSwapper : MonoBehaviour
{
	public string _ImagePath = "";

	public KAWidget _TextureWidget;

	public bool _AllLanguages;

	private Texture mTexture;

	private bool mCanShow;

	private bool mStartedDownload;

	private void Start()
	{
		if (null != _TextureWidget)
		{
			_TextureWidget.SetVisibility(inVisible: false);
		}
	}

	private void Update()
	{
		if (!mStartedDownload && ProductConfig.pIsReady)
		{
			StartCoroutine("LoadImage");
		}
		if (mCanShow)
		{
			if (null != _TextureWidget)
			{
				_TextureWidget.SetVisibility(inVisible: true);
			}
			base.enabled = false;
		}
	}

	private IEnumerator LoadImage()
	{
		yield return null;
		string localeLanguage = UtUtilities.GetLocaleLanguage();
		string imagePath = _ImagePath;
		if (!string.IsNullOrEmpty(localeLanguage) && (localeLanguage != "en-US" || _AllLanguages))
		{
			if (imagePath.StartsWith("http"))
			{
				imagePath = imagePath.Replace("en-US", localeLanguage);
				mCanShow = false;
				RsResourceManager.Load(imagePath, OnImageLoaded, RsResourceType.IMAGE);
			}
			else
			{
				string[] array = imagePath.Split('/');
				if (array.Length >= 3)
				{
					RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnImageLoaded, typeof(Texture));
				}
			}
		}
		else
		{
			mCanShow = true;
		}
		mStartedDownload = true;
	}

	private void OnImageLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent.Equals(RsResourceLoadEvent.COMPLETE))
		{
			if (_TextureWidget != null)
			{
				RsResourceManager.SetDontDestroy(inURL, inDontDestroy: true);
				mTexture = (Texture2D)inObject;
				if (null != mTexture)
				{
					_TextureWidget.SetTexture(mTexture);
				}
				mCanShow = true;
			}
			else
			{
				UtDebug.Log("Widget Destroyed before the texture downloaded " + inURL);
			}
		}
		else if (inEvent.Equals(RsResourceLoadEvent.ERROR))
		{
			Debug.Log("LoPre : Error downloading");
			mCanShow = true;
		}
	}
}
