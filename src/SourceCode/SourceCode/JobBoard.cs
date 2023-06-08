using UnityEngine;

public class JobBoard : NPCAvatar
{
	public GameObject _JobBoardTutorial;

	public override void OnActivate()
	{
		base.OnActivate();
		if (_JobBoardTutorial != null)
		{
			InteractiveTutManager component = _JobBoardTutorial.GetComponent<InteractiveTutManager>();
			if (component != null)
			{
				component.ShowTutorial();
			}
		}
	}
}
