using System;
using UnityEngine;

[Serializable]
public class MazeRoom
{
	[Serializable]
	public class Variation : MazeInfo
	{
		[Tooltip("Room bundle path to load Prefab")]
		public string _BundlePath;
	}

	public string _RoomType;

	[Tooltip("Variations of Room bundle paths")]
	public Variation[] _RoomVersions;
}
