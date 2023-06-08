using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ShopPaymentBtnController : MonoBehaviour
{
	public Button _btn;

	public Image _icon;

	public GameObject _self;

	private XsollaPaymentMethod _method;

	public void setMethod(XsollaPaymentMethod pMethod)
	{
		_method = pMethod;
	}

	public XsollaPaymentMethod getMethod()
	{
		return _method;
	}

	public void setIcon(ImageLoader pLoader = null)
	{
		switch (_method.id)
		{
		case 1380L:
			_icon.sprite = Resources.Load<Sprite>("Images/ic_cc");
			return;
		case 1738L:
			_icon.sprite = Resources.Load<Sprite>("Images/ic_mobile");
			return;
		case 3012L:
			_icon.sprite = Resources.Load<Sprite>("Images/ic_giftCard");
			return;
		}
		if (pLoader != null)
		{
			pLoader.LoadImage(_icon, _method.GetImageUrl());
		}
	}

	public void Hide()
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		if (component != null)
		{
			component.alpha = 0f;
			component.blocksRaycasts = false;
		}
	}
}
