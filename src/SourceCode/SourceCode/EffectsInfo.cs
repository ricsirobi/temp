using System;
using UnityEngine;

[Serializable]
public class EffectsInfo
{
	public string _RerollType;

	public AudioClip _RerollSFX;

	public GameObject _ParticleFX;

	public float _EffectsStartDelay;

	public float _StatsUpdateDelay;

	public float _EffectsDuration;

	public bool _RestrictSFXDuration;

	public Action pAction { get; set; }

	public void EnableEffects(bool enable, bool restrictSFXDuration = true)
	{
		if (_ParticleFX != null)
		{
			_ParticleFX.SetActive(enable);
		}
		if (_RerollSFX != null)
		{
			if (enable)
			{
				SnChannel.Play(_RerollSFX, "SFX_Pool", inForce: true);
			}
			else if (restrictSFXDuration)
			{
				SnChannel.StopPool("SFX_Pool");
			}
		}
	}
}
