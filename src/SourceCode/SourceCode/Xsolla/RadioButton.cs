using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class RadioButton : MonoBehaviour, IRadioButton
{
	public enum RadioType
	{
		SCREEN_GOODS,
		SCREEN_PRICEPOINT,
		SCREEN_SUBSCRIPTION,
		SCREEN_REDEEMCOUPON,
		SCREEN_FAVOURITE
	}

	public Graphic image;

	public Graphic text;

	public StyleManager.BaseColor activeImage;

	public StyleManager.BaseColor normalImage;

	public StyleManager.BaseColor activeText;

	public StyleManager.BaseColor normalText;

	private bool isSelected;

	private bool isStarted;

	private RadioType _typeRadioBtn;

	public void setType(RadioType pType)
	{
		_typeRadioBtn = pType;
	}

	public RadioType getType()
	{
		return _typeRadioBtn;
	}

	private void Start()
	{
		isStarted = true;
	}

	public void Select()
	{
		if (!isSelected)
		{
			if (image != null)
			{
				image.color = Singleton<StyleManager>.Instance.GetColor(activeImage);
			}
			if (text != null)
			{
				text.color = Singleton<StyleManager>.Instance.GetColor(activeText);
			}
			if (isStarted)
			{
				isSelected = true;
			}
			else
			{
				Invoke("Select", 1f);
			}
		}
	}

	public void Deselect()
	{
		if (isSelected)
		{
			if (image != null)
			{
				image.color = Singleton<StyleManager>.Instance.GetColor(normalImage);
			}
			if (text != null)
			{
				text.color = Singleton<StyleManager>.Instance.GetColor(normalText);
			}
			isSelected = false;
		}
	}

	private void Update()
	{
		if (isSelected)
		{
			if (image != null)
			{
				image.color = Singleton<StyleManager>.Instance.GetColor(activeImage);
			}
			if (text != null)
			{
				text.color = Singleton<StyleManager>.Instance.GetColor(activeText);
			}
		}
		else
		{
			if (image != null)
			{
				image.color = Singleton<StyleManager>.Instance.GetColor(normalImage);
			}
			if (text != null)
			{
				text.color = Singleton<StyleManager>.Instance.GetColor(normalText);
			}
		}
	}

	public void visibleBtn(bool pState)
	{
		base.gameObject.SetActive(pState);
	}
}
