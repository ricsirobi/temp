using System;

public class UiSettingsMenu : KAUIMenu
{
	[Serializable]
	public class SettingOption
	{
		public KAWidget _WidgetTemplate;

		public string _SettingButtonName;

		public LocaleString _SettingText;

		public bool _Show = true;
	}

	public UiOptions _ParentUI;

	public SettingOption[] _SettingOptions;

	public void Init()
	{
		SettingOption[] settingOptions = _SettingOptions;
		foreach (SettingOption settingOption in settingOptions)
		{
			if (settingOption._Show && IsButtonAllowed(settingOption._SettingButtonName))
			{
				KAWidget kAWidget = DuplicateWidget(settingOption._WidgetTemplate);
				kAWidget.name = settingOption._SettingButtonName;
				kAWidget.SetText(settingOption._SettingText.GetLocalizedString());
				AddWidget(kAWidget);
				kAWidget.SetVisibility(inVisible: true);
			}
		}
	}

	private bool IsButtonAllowed(string inButtonName)
	{
		switch (inButtonName)
		{
		case "BtnRegister":
			return UiLogin.pIsGuestUser;
		case "BtnHotKeys":
			if (!UtPlatform.IsStandAlone())
			{
				return UtPlatform.IsWSA();
			}
			return true;
		case "BtnRestorePurchases":
			return UtPlatform.IsiOS();
		case "BtnGraphicSettings":
			return !UtPlatform.IsXBox();
		case "BtnFullScreen":
			if (!UtPlatform.IsStandAlone())
			{
				return UtPlatform.IsWSA();
			}
			return true;
		case "BtnFlightControlTouch":
			if (!UtPlatform.IsiOS())
			{
				return UtPlatform.IsAndroid();
			}
			return true;
		default:
			return true;
		}
	}
}
