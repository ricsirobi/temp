using System;
using System.Collections.Generic;

[Serializable]
public class MissionGroupSettings
{
	public int _GroupID;

	public bool _AutoStart;

	public List<MissionHelp> _HelpVOs = new List<MissionHelp>();
}
