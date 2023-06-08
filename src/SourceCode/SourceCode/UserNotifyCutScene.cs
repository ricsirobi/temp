using System.Collections.Generic;

public class UserNotifyCutScene : UserNotify
{
	public List<CutSceneInfo> _CutSceneData;

	private PlayCutScene mCutScene;

	public override void OnWaitBeginImpl()
	{
		mCutScene = new PlayCutScene();
		if (!mCutScene.LoadCutScene(_CutSceneData, base.gameObject))
		{
			OnWaitEnd();
		}
	}

	public void OnCutSceneDone()
	{
		mCutScene.OnCutSceneCompleted();
		OnWaitEnd();
	}
}
