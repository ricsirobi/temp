using UnityEngine;

public class KAUIFlightSchoolTutScreen : KAUI
{
	[HideInInspector]
	public ObstacleCourseGame _Game;

	public string _OkButtonName = "BtnOK";

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		KAInput.ResetInputAxes();
	}

	protected override void Update()
	{
		base.Update();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _OkButtonName)
		{
			_Game.SetupGame();
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
			_Game.pCurrentTutorialUI = null;
		}
	}
}
