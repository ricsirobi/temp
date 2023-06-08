using System;
using UnityEngine;

[Serializable]
public class TaskOverride
{
	public AvAvatarState _StartStateOverride;

	public AvAvatarSubState _StartSubStateOverride;

	public TaskInfo _BeginTask;

	public TaskInfo _EndTask;

	public MountableNPCPet _MountableNPC;

	public PetSpecialSkillType _PetSpecialSkillType;

	public Transform[] _Markers;
}
