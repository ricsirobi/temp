using System;

[Serializable]
public class QuickLaunchBtnInfo
{
	public bool _CheckAllConditions = true;

	public KAWidget _UnLockedButton;

	public KAWidget _LockedButton;

	public UnlockInfo[] _UnlockInfo;

	public LoadInfo _LoadInfo;

	private string mLockedButtonText;

	public string pLockedButtonText
	{
		get
		{
			return mLockedButtonText;
		}
		set
		{
			mLockedButtonText = value;
		}
	}
}
