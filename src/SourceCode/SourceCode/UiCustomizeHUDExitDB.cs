using UnityEngine;

public class UiCustomizeHUDExitDB : KAUI
{
	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		if (UtPlatform.IsiOS())
		{
			KAWidget kAWidget = FindItem("ApplyBtn");
			KAWidget kAWidget2 = FindItem("ResetBtn");
			if (kAWidget != null && kAWidget2 != null)
			{
				Vector3 localPosition = kAWidget2.transform.localPosition;
				kAWidget2.transform.localPosition = kAWidget.transform.localPosition;
				kAWidget.transform.localPosition = localPosition;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "ApplyBtn")
		{
			UiCustomizeHUD.pInstance.OnSaveAndQuit();
		}
		else if (inWidget.name == "ResetBtn")
		{
			UiCustomizeHUD.pInstance.ResetSession();
			UiCustomizeHUD.pInstance.OnQuitWithoutSaving();
		}
		else if (inWidget.name == "CloseBtn")
		{
			UiCustomizeHUD.pInstance.EnableDrag(isDragEnabled: true);
		}
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}
}
