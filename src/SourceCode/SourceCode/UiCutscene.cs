using UnityEngine;

public class UiCutscene : KAUI
{
	public CoAnimController _CoAnimController;

	public KAButton _SkipBtn;

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnClick(inWidget);
			if (inWidget == _SkipBtn)
			{
				SkipCutscene();
			}
		}
	}

	public void SkipCutscene()
	{
		if ((bool)_CoAnimController && (bool)_CoAnimController._MessageObject)
		{
			_CoAnimController._MessageObject.SendMessage("OnCutSceneDone", null, SendMessageOptions.DontRequireReceiver);
		}
	}
}
