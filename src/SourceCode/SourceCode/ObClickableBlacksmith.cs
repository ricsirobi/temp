using UnityEngine;

public class ObClickableBlacksmith : ObClickableCreateInstance
{
	public override void OnCreateInstance(GameObject inObject)
	{
		base.OnCreateInstance(inObject);
		UiBlacksmith.OnClosed = delegate
		{
			OnBlacksmithClosed();
		};
	}

	private void OnBlacksmithClosed()
	{
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "ClickObject", base.gameObject.name);
		}
	}
}
