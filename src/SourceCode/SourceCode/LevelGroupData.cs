using System;
using UnityEngine;

[Serializable]
public class LevelGroupData
{
	public string _GroupName;

	public Texture2D _Icon;

	public LocaleString _LevelNameText;

	public int _MaxScore;

	public bool _UnLocked = true;

	public AudioClip _LevelGroupVO;

	public bool _TimeTrial;

	public GradeSystem[] _FSGrades;
}
