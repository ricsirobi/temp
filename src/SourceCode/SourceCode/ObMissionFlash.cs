using UnityEngine;

public class ObMissionFlash : MonoBehaviour
{
	public int _TaskID;

	public ObMissionFlashData[] _Data;

	private bool mAnimPlaying;

	private Task mCurrentTask;

	private void Start()
	{
		mAnimPlaying = false;
		mCurrentTask = null;
	}

	private void Update()
	{
		if (MissionManager.pInstance != null && !mAnimPlaying)
		{
			if (_TaskID <= 0)
			{
				return;
			}
			Task task = MissionManager.pInstance.GetTask(_TaskID);
			if (task != null && task.pStarted)
			{
				mAnimPlaying = true;
				mCurrentTask = task;
				for (int i = 0; i < _Data.Length; i++)
				{
					_Data[i]._FlashWidget.PlayAnim("Flash");
				}
			}
		}
		else if (mAnimPlaying && mCurrentTask != null && mCurrentTask.pCompleted)
		{
			mCurrentTask = null;
			StopFlash();
		}
	}

	public void StopFlash()
	{
		mAnimPlaying = false;
		for (int i = 0; i < _Data.Length; i++)
		{
			_Data[i]._FlashWidget.StopAnim("Flash");
			if (!string.IsNullOrEmpty(_Data[i]._DefaultSprite))
			{
				_Data[i]._FlashWidget.SetSprite(_Data[i]._DefaultSprite);
			}
		}
		Object.Destroy(this);
	}
}
