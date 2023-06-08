using System;
using System.Collections.Generic;

[Serializable]
public class AvatarStateBetweenTasks
{
	public string _AvatarState;

	public int _StartTask;

	public int _EndTask;

	public bool _SetOnReloadOnly = true;

	public List<AvatarActions> _Actions;

	public string _MountablePetName = string.Empty;

	public string _MountPillionNPC = string.Empty;
}
