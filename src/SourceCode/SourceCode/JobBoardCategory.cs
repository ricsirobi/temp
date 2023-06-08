using System;
using UnityEngine;

[Serializable]
public class JobBoardCategory
{
	public LocaleString _Name;

	public int _MissionGroupID;

	public Texture _Icon;

	public int _WaitTimeForTaskInSec;

	public int _TrashTimeForTaskInSec;
}
