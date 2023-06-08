using System;
using UnityEngine;

[Serializable]
public class MazeLayout : MazeInfo
{
	public MazeRoom[] _MazeRooms;

	[Tooltip("If set Random rooms are picked randomly by their weightage otherwise rooms are by the order they are entered")]
	public bool _Random;
}
