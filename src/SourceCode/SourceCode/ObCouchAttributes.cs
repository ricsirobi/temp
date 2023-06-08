using System;
using UnityEngine;

[Serializable]
public class ObCouchAttributes
{
	public GameObject _SeatMarker;

	public int _ID;

	private ObCouchIcon mChairIcon;

	private GameObject mOccupiedAvatar;

	public ObCouchIcon pChairIcon
	{
		get
		{
			return mChairIcon;
		}
		set
		{
			mChairIcon = value;
		}
	}

	public GameObject pOccupiedAvatar
	{
		get
		{
			return mOccupiedAvatar;
		}
		set
		{
			mOccupiedAvatar = value;
		}
	}
}
