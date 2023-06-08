using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Xsolla;

public class ShopItemViewAdapter : MonoBehaviour
{
	private ImageLoader imageLoader;

	public GameObject ItemLable;

	public Text ItemLableText;

	public Image ItemImage;

	public Text ItemName;

	public Text ItemDesc;

	public Text ItemDescFull;

	public Text ItemBonus;

	public Text ItemPrice;

	public Text LearnMore;

	public Text TextBuy;

	public Toggle FavoriteToggle;

	public ColorController colorController;

	private Button button;

	private void Awake()
	{
		button = GetComponentInChildren<Button>();
	}

	public void SetLabel(AXsollaShopItem.AdType type, string text)
	{
		StyleManager.BaseColor color = StyleManager.BaseColor.b_normal;
		switch (type)
		{
		case AXsollaShopItem.AdType.NONE:
			color = StyleManager.BaseColor.b_normal;
			break;
		case AXsollaShopItem.AdType.SPECIAL_OFFER:
			color = StyleManager.BaseColor.b_special_offer;
			break;
		case AXsollaShopItem.AdType.BEST_DEAL:
			color = StyleManager.BaseColor.b_best_deal;
			break;
		case AXsollaShopItem.AdType.RECCOMENDED:
			color = StyleManager.BaseColor.b_recomended;
			break;
		}
		colorController.ChangeColor(0, color);
		colorController.ChangeColor(2, color);
		if (type == AXsollaShopItem.AdType.NONE)
		{
			ItemLable.SetActive(value: false);
			return;
		}
		ItemLableText.text = text;
		ItemLable.SetActive(value: true);
	}

	public void SetName(string coinsAmountText)
	{
		ItemName.text = coinsAmountText;
	}

	public void SetSpecial(string bonusText)
	{
		ItemBonus.text = bonusText;
	}

	public void SetDesc(string coinsText)
	{
		ItemDesc.text = coinsText;
	}

	public void SetFullDesc(string fullDesc)
	{
		if (fullDesc != null && !"".Equals(fullDesc))
		{
			ItemDescFull.text = fullDesc;
		}
		else
		{
			ItemDescFull.transform.parent.transform.parent.gameObject.SetActive(value: false);
		}
	}

	public void SetPrice(string price)
	{
		ItemPrice.text = price;
	}

	public void SetBuyText(string buyText)
	{
		TextBuy.text = buyText;
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

	public void SetFavorite(bool isFavorite)
	{
		if (FavoriteToggle != null)
		{
			FavoriteToggle.isOn = isFavorite;
		}
	}

	public void SetLoader(ImageLoader loader)
	{
		imageLoader = loader;
	}

	public void SetOnClickListener(UnityAction onItemClick)
	{
		button.onClick.AddListener(onItemClick);
	}

	public void SetOnFavoriteChanged(UnityAction<bool> onValueChanged)
	{
		FavoriteToggle.onValueChanged.AddListener(delegate(bool b)
		{
			onValueChanged(b);
		});
	}
}
