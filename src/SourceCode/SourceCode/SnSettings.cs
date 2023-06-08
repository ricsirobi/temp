using System;
using UnityEngine;

[Serializable]
public class SnSettings
{
	public string _Channel = "";

	public string _Pool = "";

	public int _Priority;

	public float _Volume = 1f;

	public float _Pitch = 1f;

	public float _MinVolume;

	public float _MaxVolume = 1f;

	public float _RolloffFactor = 0.1f;

	public float _RolloffBegin;

	public float _RolloffEnd = float.PositiveInfinity;

	public bool _UseRolloffDistance;

	public bool _Loop;

	public bool _ReleaseOnStop = true;

	public bool _FollowListener;

	public GameObject _EventTarget;

	public SnSettings()
	{
	}

	public SnSettings(SnSettings inSettings)
	{
		_Channel = inSettings._Channel;
		_Pool = inSettings._Pool;
		_Priority = inSettings._Priority;
		_Volume = inSettings._Volume;
		_Pitch = inSettings._Pitch;
		_MinVolume = inSettings._MinVolume;
		_MaxVolume = inSettings._MaxVolume;
		_RolloffFactor = inSettings._RolloffFactor;
		_RolloffBegin = inSettings._RolloffBegin;
		_RolloffEnd = inSettings._RolloffEnd;
		_UseRolloffDistance = inSettings._UseRolloffDistance;
		_Loop = inSettings._Loop;
		_ReleaseOnStop = inSettings._ReleaseOnStop;
		_FollowListener = inSettings._FollowListener;
		_EventTarget = inSettings._EventTarget;
	}
}
