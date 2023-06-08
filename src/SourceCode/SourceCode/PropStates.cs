using System;
using UnityEngine;

[Serializable]
public class PropStates
{
	public string _Name = "";

	public EquipObject[] _Objects;

	public AudioClip _SoundClip;

	public string _AnimState = "";

	public bool _QuitOnAnimEnd;

	public bool _QuitOnPlayerMove;

	public bool _FreezePlayer;
}
