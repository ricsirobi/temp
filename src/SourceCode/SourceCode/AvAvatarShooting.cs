using System;
using UnityEngine;

[Serializable]
public class AvAvatarShooting
{
	public Vector3 _ProjectileStartOffset = Vector3.zero;

	public GameObject _Projectile;

	public int _ShotsFiredUntilReload = 5;

	public float _ReloadTime = 5f;

	public float _RateOfFire = 5f;

	public string _Animation;

	public float _FireDelay;

	public bool _MembersOnly = true;

	public AudioClip _MembersOnlyVO;
}
