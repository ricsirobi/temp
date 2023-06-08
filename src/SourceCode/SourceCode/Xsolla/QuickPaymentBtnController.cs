using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class QuickPaymentBtnController : MonoBehaviour
{
	public Image _iconMethod;

	public Text _labelMethod;

	public Button _btnMethod;

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

	public void setLable(string pName)
	{
		_labelMethod.text = pName;
	}

	public void Hide()
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		component.alpha = 0f;
		component.blocksRaycasts = false;
	}

	public void Show()
	{
		CanvasGroup component = GetComponent<CanvasGroup>();
		component.alpha = 1f;
		component.blocksRaycasts = true;
	}

	public void setIcon(long pMethodID, ImageLoader pLoader = null)
	{
		switch (pMethodID)
		{
		case 1380L:
			_iconMethod.sprite = Resources.Load<Sprite>("Images/ic_cc");
			return;
		case 1738L:
			_iconMethod.sprite = Resources.Load<Sprite>("Images/ic_mobile");
			return;
		case 3012L:
			_iconMethod.sprite = Resources.Load<Sprite>("Images/ic_giftCard");
			return;
		}
		if (pLoader != null)
		{
			pLoader.LoadImage(_iconMethod, _method.GetImageUrl());
		}
	}
}
