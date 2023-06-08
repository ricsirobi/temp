using System.Collections;
using UnityEngine;

public class ObLastLevel : MonoBehaviour
{
	public ObLastLevelData[] _LevelData;

	public GameObject _DefaultLevelObject;

	private static ArrayList mPreviousLevelsList = new ArrayList();

	private UiToolbar mUiToolbar;

	private bool mLevelData;

	private void Start()
	{
		GameObject pToolbar = AvAvatar.pToolbar;
		if (pToolbar != null)
		{
			mUiToolbar = pToolbar.GetComponent(typeof(UiToolbar)) as UiToolbar;
		}
		if (_LevelData != null)
		{
			if (mPreviousLevelsList.Contains(RsResourceManager.pLastLevel))
			{
				mPreviousLevelsList.Remove(RsResourceManager.pLastLevel);
			}
			mPreviousLevelsList.Insert(0, RsResourceManager.pLastLevel);
			mLevelData = true;
		}
	}

	private void Update()
	{
		if (mLevelData)
		{
			SetLevelData();
		}
	}

	private void SetLevelData()
	{
		bool flag = false;
		foreach (string mPreviousLevels in mPreviousLevelsList)
		{
			ObLastLevelData[] levelData = _LevelData;
			foreach (ObLastLevelData obLastLevelData in levelData)
			{
				if (!(mPreviousLevels == obLastLevelData._Level))
				{
					continue;
				}
				flag = true;
				ObClickable component = GetComponent<ObClickable>();
				if (component != null)
				{
					component._LoadLevel = obLastLevelData._Level;
					if (obLastLevelData._Rollover != null)
					{
						component._RolloverSound._AudioClip = obLastLevelData._Rollover;
					}
					if (obLastLevelData._StartMarker != null)
					{
						component._StartMarker = obLastLevelData._StartMarker;
					}
				}
				ObProximity component2 = GetComponent<ObProximity>();
				if (component2 != null)
				{
					component2._LoadLevel = obLastLevelData._Level;
					if (obLastLevelData._StartMarker != null)
					{
						component2._StartMarker = obLastLevelData._StartMarker;
					}
				}
				ObTrigger component3 = GetComponent<ObTrigger>();
				if (component3 != null)
				{
					component3._LoadLevel = obLastLevelData._Level;
					if (obLastLevelData._StartMarker != null)
					{
						component3._StartMarker = obLastLevelData._StartMarker;
					}
				}
				if (mUiToolbar != null && !string.IsNullOrEmpty(mUiToolbar._BackBtnLoadLevel))
				{
					mUiToolbar._BackBtnLoadLevel = obLastLevelData._Level;
					if (obLastLevelData._StartMarker != null)
					{
						mUiToolbar._BackBtnMarker = obLastLevelData._StartMarker;
					}
				}
				if (obLastLevelData._LevelObject != null && _DefaultLevelObject != obLastLevelData._LevelObject)
				{
					_DefaultLevelObject.SetActive(value: false);
					obLastLevelData._LevelObject.SetActive(value: true);
				}
				mLevelData = false;
				break;
			}
			if (flag)
			{
				mLevelData = false;
				break;
			}
		}
	}
}
