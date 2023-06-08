using System;
using UnityEngine;

[Serializable]
public class AvAvatarBounceParams
{
	public string[] _BouncingAreas;

	public float _BounceDissipatingFactor = 0.7f;

	public float _BouncingFactor = 1f;

	public float _BounceStartThreshold = 1f;

	public float _BounceStopThreshold = 1f;

	public SnRandomSound _BounceSound;

	public GameObject _BouncingObject;
}
