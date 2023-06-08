using System;
using UnityEngine;

[Serializable]
public class AvAvatarAnimEvent
{
	public string _Animation;

	public AnimData[] _Times;

	public string _Function;

	public GameObject _Target;

	public GameObject _Parent;

	[NonSerialized]
	public AnimData mData;
}
