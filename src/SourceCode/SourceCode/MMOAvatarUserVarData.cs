using KnowledgeAdventure.Multiplayer.Model;
using UnityEngine;

public class MMOAvatarUserVarData
{
	public int _Priority;

	public int _ID;

	public string _Data;

	public GameObject _GameObject;

	public Vector3 _Position = Vector3.zero;

	public Quaternion _Rotation = Quaternion.identity;

	public Vector3 _Velocity = Vector3.zero;

	public float _MaxSpeed;

	public MMOAvatarFlags _Flags;

	public int _AnimState;

	public double _TimeStamp;

	public double _ServerTimeStamp;

	public float _InterpolationTime;

	public MMOAvatarUserVarData()
	{
	}

	public MMOAvatarUserVarData(MMOUser user, bool sameClan = false)
	{
		if (sameClan)
		{
			_Priority++;
		}
		string value = user.UserVariables["UID"].ToString();
		if (BuddyList.pList == null)
		{
			return;
		}
		Buddy[] pList = BuddyList.pList;
		for (int i = 0; i < pList.Length; i++)
		{
			if (pList[i].UserID.Equals(value))
			{
				_Priority++;
				break;
			}
		}
	}
}
