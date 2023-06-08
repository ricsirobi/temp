using UnityEngine;

public class KAUIFlightSchoolQuit : KAUI
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
			if ((bool)kAWidget && (bool)kAWidget2)
			{
				Transform obj = kAWidget2.transform;
				Transform transform = kAWidget.transform;
				Vector3 localPosition = kAWidget.transform.localPosition;
				Vector3 localPosition2 = kAWidget2.transform.localPosition;
				Vector3 vector2 = (obj.localPosition = localPosition);
				vector2 = (transform.localPosition = localPosition2);
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _YesButtonName)
		{
			if (_Game != null)
			{
				_Game.OnQuitYes();
			}
			RsResourceManager.LoadLevel(RsResourceManager.pCurrentLevel);
		}
		else if (inWidget.name == _NoButtonName && _Game != null)
		{
			_Game.OnQuitNo();
		}
		KAUI.RemoveExclusive(GetComponent<KAUIFlightSchoolQuit>());
		Object.Destroy(base.gameObject);
	}
}
