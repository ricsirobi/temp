using UnityEngine;

public class KAUIFlightSchoolLevelFail : KAUI
{
	[HideInInspector]
	public ObstacleCourseGame _Game;

	public string _NoButtonName = "BtnNo";

	public string _YesButtonName = "BtnYes";

	protected override void Start()
	{
		base.Start();
		if (UtPlatform.IsiOS())
		{
			KAWidget kAWidget = FindItem(_YesButtonName);
			KAWidget kAWidget2 = FindItem(_NoButtonName);
			Vector3 localPosition = kAWidget2.transform.localPosition;
			kAWidget2.transform.localPosition = kAWidget.transform.localPosition;
			kAWidget.transform.localPosition = localPosition;
		}
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _NoButtonName)
		{
			_Game.RestartLevel(bFromLevelSelection: true);
		}
		else if (inWidget.name == _YesButtonName)
		{
			_Game.RestartLevel(bFromLevelSelection: false);
		}
		Object.Destroy(base.gameObject);
	}
}
