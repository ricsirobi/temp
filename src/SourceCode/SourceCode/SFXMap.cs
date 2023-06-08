using System;
using UnityEngine;

[Serializable]
public class SFXMap
{
	public string _AnimName = "";

	public string _ClipResName;

	public AudioClip _ClipRes;

	[Range(0f, 1f)]
	[Tooltip("Adjust the value to make SFX from full 2D to full 3D")]
	public float _SpatialBlend = 1f;
}
