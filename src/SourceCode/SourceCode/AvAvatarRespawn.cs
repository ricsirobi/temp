using System;
using UnityEngine;

[Serializable]
public class AvAvatarRespawn
{
	public GameObject _PrtDeath;

	public GameObject _PrtRevive;

	public float _DeathDelay = 1f;

	public float _SpawnDelay = 0.25f;

	public float _FlashTimer = 3f;

	public float _FlashInterval = 0.25f;

	public bool _DoTeleportFX = true;

	public bool _ImmuneAvatar = true;

	public AudioClip _DeathSFX;

	public AudioClip _ReviveSFX;

	public bool _EnablePet;
}
