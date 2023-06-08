using System;
using UnityEngine;

public class UiGraphicSettingItem : KAWidget
{
	private KAUIDropDownMenu mOptionMenu;

	private KAWidget mCurrentOptionTxt;

	private UiGraphicSettings mGraphicSettings;

	private KAWidget mOptionWidget;

	public GraphicSettingType pGraphicSettingType { get; set; }

	public KAUIDropDownMenu pOptionMenu => mOptionMenu;

	public void Init(GraphicSettingType type, string settingName)
	{
		pGraphicSettingType = type;
		SetText(settingName);
		mCurrentOptionTxt = FindChildItem("TxtSetting");
		mOptionWidget = FindChildItem("BtnGraphicsDropDown");
		mOptionMenu = GetComponentInChildren<KAUIDropDownMenu>();
	}

	public void UpdateData(UiGraphicSettings graphicsUi, UiGraphicsOptionMenuWidgetData data)
	{
		if (mOptionMenu != null)
		{
			KAUIDropDownMenu kAUIDropDownMenu = mOptionMenu;
			kAUIDropDownMenu.onItemSelected = (KAUIDropDownMenu.OnItemSelected)Delegate.Combine(kAUIDropDownMenu.onItemSelected, new KAUIDropDownMenu.OnItemSelected(OnItemSelected));
		}
		if (mCurrentOptionTxt != null)
		{
			mCurrentOptionTxt.SetText(data._Item.GetText());
		}
		SetUserData(data);
		mGraphicSettings = graphicsUi;
	}

	public void UpdateWidget(string option)
	{
		foreach (KAWidget item in mOptionMenu.GetItems())
		{
			UiGraphicsOptionMenuWidgetData uiGraphicsOptionMenuWidgetData = item.GetUserData() as UiGraphicsOptionMenuWidgetData;
			if (uiGraphicsOptionMenuWidgetData._Option.Equals(option))
			{
				mCurrentOptionTxt.SetText(item.GetText());
				SetUserData(uiGraphicsOptionMenuWidgetData);
				break;
			}
		}
	}

	public void OnItemSelected(KAWidget widget, KAUIDropDownMenu dropDown)
	{
		if (mCurrentOptionTxt != null)
		{
			mCurrentOptionTxt.SetText(widget.GetText());
			UiGraphicsOptionMenuWidgetData userData = widget.GetUserData() as UiGraphicsOptionMenuWidgetData;
			SetUserData(userData);
		}
		if (mGraphicSettings != null)
		{
			mGraphicSettings.CheckButtonState();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mOptionMenu != null && mOptionMenu.IsActive() && (KAInput.GetButtonUp("Cancel") || (Input.GetMouseButtonUp(0) && mOptionWidget != null && KAUIManager.pInstance.pSelectedWidget != mOptionWidget)))
		{
			mOptionMenu.UpdateState(isDropped: false);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)mOptionMenu)
		{
			KAUIDropDownMenu kAUIDropDownMenu = mOptionMenu;
			kAUIDropDownMenu.onItemSelected = (KAUIDropDownMenu.OnItemSelected)Delegate.Remove(kAUIDropDownMenu.onItemSelected, new KAUIDropDownMenu.OnItemSelected(OnItemSelected));
		}
	}

	public void RefreshLocale(UiGraphicSettings.GraphicsMapper menuData, string currentSetting)
	{
		SetText(menuData._GraphicSettingsText.GetLocalizedString());
		UiGraphicSettings.GraphicOptions graphicOptions = menuData._Options.Find((UiGraphicSettings.GraphicOptions entry) => entry._Option.Equals(currentSetting));
		mCurrentOptionTxt.SetText(graphicOptions._OptionText.GetLocalizedString());
		foreach (KAWidget item in mOptionMenu.GetItems())
		{
			UiGraphicsOptionMenuWidgetData widgetData = item.GetUserData() as UiGraphicsOptionMenuWidgetData;
			graphicOptions = menuData._Options.Find((UiGraphicSettings.GraphicOptions entry) => entry._Option.Equals(widgetData._Option));
			item.SetText(graphicOptions._OptionText.GetLocalizedString());
		}
	}
}
