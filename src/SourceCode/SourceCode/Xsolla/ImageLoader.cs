using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ImageLoader : MonoBehaviour
{
	public string imageUrl;

	public static Dictionary<string, Sprite> imageCashe;

	private static ImageLoader instance;

	private void Awake()
	{
		if (imageCashe == null)
		{
			imageCashe = new Dictionary<string, Sprite>();
		}
	}

	public void Start()
	{
		if (imageUrl != null && !imageUrl.Equals(""))
		{
			UploadImageToCurrentView(imageUrl);
		}
	}

	public void LoadImage(Image imageView, string url)
	{
		StartCoroutine(RealLoadImage(url, imageView));
	}

	public void UploadImageToCurrentView(string imageUrl)
	{
		StartCoroutine(RealLoadImage(imageUrl, GetComponent<Image>()));
	}

	private IEnumerator RealLoadImage(string url, Image imageView)
	{
		if (imageCashe.ContainsKey(url))
		{
			if (imageView != null)
			{
				imageView.sprite = imageCashe[url];
			}
		}
		else
		{
			WWW imageURLWWW = new WWW(url);
			yield return imageURLWWW;
			if (imageURLWWW.error == null && imageURLWWW.texture != null)
			{
				Sprite sprite = Sprite.Create(imageURLWWW.texture, new Rect(0f, 0f, imageURLWWW.texture.width, imageURLWWW.texture.height), Vector2.zero);
				if (!imageCashe.ContainsKey(url))
				{
					imageCashe.Add(url, sprite);
				}
				if (imageView != null)
				{
					imageView.sprite = sprite;
				}
			}
			imageURLWWW.Dispose();
		}
		yield return null;
	}
}
