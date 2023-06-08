using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class VPItemViewAdapter : MonoBehaviour
{
	private ImageLoader imageLoader;

	public Image ItemImage;

	public Text ItemName;

	public void SetName(string coinsAmountText)
	{
		ItemName.text = coinsAmountText;
	}

	public void SetLoader(ImageLoader loader)
	{
		imageLoader = loader;
	}

	public void SetImage(string imgUrl)
	{
		if (imageLoader == null)
		{
			imageLoader = GetComponentInChildren<ImageLoader>();
		}
		if (imageLoader != null)
		{
			imageLoader.imageUrl = imgUrl;
		}
	}
}
