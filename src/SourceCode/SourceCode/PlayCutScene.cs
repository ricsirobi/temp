using System.Collections.Generic;
using UnityEngine;

public class PlayCutScene
{
	private CoAnimController mCurrentCutscene;

	private GameObject mMessageObject;

	private CutSceneInfo mCutSceneInfo;

	public bool LoadCutScene(List<CutSceneInfo> cutSceneList, GameObject notify)
	{
		mMessageObject = notify;
		foreach (CutSceneInfo cutScene in cutSceneList)
		{
			if (cutScene._SceneName.Equals(RsResourceManager.pCurrentLevel) && !ProductData.pPairData.KeyExists(cutScene._Key))
			{
				mCutSceneInfo = cutScene;
				KAUICursorManager.SetDefaultCursor("Loading");
				string[] array = mCutSceneInfo._CutScenePath.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], CutSceneLoadEvent, typeof(GameObject));
				return true;
			}
		}
		return false;
	}

	private void CutSceneLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inObject != null)
			{
				GameObject gameObject = Object.Instantiate((GameObject)inObject);
				if (gameObject != null)
				{
					ProductData.pPairData.SetValueAndSave(mCutSceneInfo._Key, "True");
					mCurrentCutscene = gameObject.GetComponentInChildren<CoAnimController>();
					mCurrentCutscene._MessageObject = mMessageObject;
					AvAvatar.SetUIActive(inActive: false);
					mCurrentCutscene.CutSceneStart();
				}
				else
				{
					mMessageObject.SendMessage("OnCutSceneDone");
				}
			}
			else
			{
				UtDebug.Log("CutSceneLoadEvent inObject is null!", Mission.LOG_MASK);
				mMessageObject.SendMessage("OnCutSceneDone");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.Log("CutSceneLoadEvent inObject is null!", Mission.LOG_MASK);
			mMessageObject.SendMessage("OnCutSceneDone");
			break;
		}
	}

	public void OnCutSceneCompleted()
	{
		if (mCurrentCutscene != null)
		{
			AvAvatar.SetUIActive(inActive: true);
			mCurrentCutscene.CutSceneDone();
			Object.Destroy(mCurrentCutscene.gameObject);
			mCurrentCutscene = null;
		}
	}
}
