using System;
using System.Collections.Generic;

public class MagneticZoneHandler : KAMonoBase
{
	[NonSerialized]
	public List<MagneticZoneInfo> Zones = new List<MagneticZoneInfo>();

	public void AddToZoneList(MagneticZoneInfo magneticZoneInfo)
	{
		Zones.Add(magneticZoneInfo);
	}

	public void RemoveFromZoneList(MagneticZoneInfo magneticZoneInfo)
	{
		Zones.Remove(magneticZoneInfo);
	}
}
