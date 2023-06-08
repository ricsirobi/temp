using UnityEngine;

public class UiServerChange : KAUI
{
	private KAWidget mTxtInfo;

	private const string mBuildServerType = "BuildServerType";

	protected override void Start()
	{
		base.Start();
		mTxtInfo = FindItem("TxtInfo");
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		bool flag = false;
		if (inWidget.name == "BtnClose")
		{
			UiLogin.pInstance.SetVisibility(inVisible: true);
			SetVisibility(inVisible: false);
			return;
		}
		flag = true;
		PlayerPrefs.DeleteAll();
		if (inWidget.name == "BtnDev")
		{
			PlayerPrefs.SetString("BuildServerType", "Dev");
			mTxtInfo.SetText("Server type changed to Dev.\n Quit & launch the app");
		}
		else if (inWidget.name == "BtnQA")
		{
			PlayerPrefs.SetString("BuildServerType", "QA");
			mTxtInfo.SetText("Server type changed to QA.\n Quit & launch the app");
		}
		else if (inWidget.name == "BtnStaging")
		{
			PlayerPrefs.SetString("BuildServerType", "Staging");
			mTxtInfo.SetText("Server type changed to Staging.\n Quit & launch the app");
		}
	}
}
