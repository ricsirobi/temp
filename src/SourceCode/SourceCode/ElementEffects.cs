using System.Collections.Generic;
using UnityEngine;

public class ElementEffects : MonoBehaviour
{
	public ParticleSystem _TileBreak;

	public ParticleSystem _CompoundAppear;

	public ParticleSystem _WaterEffect;

	public ParticleSystem _CO2Effect;

	public ParticleSystem _SaltEffect;

	public ParticleSystem _NadderSpikeEffect;

	public ParticleSystem _TutorialTileHighlightEffect;

	private static ElementEffects mInstance;

	private List<ParticleSystem> mParticles = new List<ParticleSystem>();

	public static ElementEffects pInstance => mInstance;

	private void Awake()
	{
		mInstance = this;
	}

	private void Start()
	{
	}

	public void PlayTileBreakFX(Vector3 position)
	{
		if (null != _TileBreak)
		{
			ParticleManager.PlayParticle(_TileBreak, position, Quaternion.identity);
		}
	}

	public void PlayCompoundCreationFX(Vector3 position)
	{
		if (null != _CompoundAppear)
		{
			ParticleManager.PlayParticle(_CompoundAppear, position, Quaternion.identity);
		}
	}

	public void PlayWaterFX(Vector3 position)
	{
		if (null != _WaterEffect)
		{
			ParticleManager.PlayParticle(_WaterEffect, position, Quaternion.identity);
		}
	}

	public void PlayCO2FX(Vector3 position)
	{
		if (null != _CO2Effect)
		{
			ParticleManager.PlayParticle(_CO2Effect, position, Quaternion.identity);
		}
	}

	public void PlaySaltFX(Vector3 position)
	{
		if (null != _SaltEffect)
		{
			ParticleManager.PlayParticle(_SaltEffect, position, Quaternion.identity);
		}
	}

	public void PlayNadderSpikeFX(Vector3 position)
	{
		if (null != _NadderSpikeEffect)
		{
			ParticleManager.PlayParticle(_NadderSpikeEffect, position, Quaternion.identity);
		}
	}

	public void PlayTutorialHighLightFX(Vector3 position)
	{
		if (null != _TutorialTileHighlightEffect)
		{
			ParticleSystem item = ParticleManager.PlayParticle(_TutorialTileHighlightEffect, position, Quaternion.identity);
			mParticles.Add(item);
		}
	}

	public void ClearTutorialParticles()
	{
		foreach (ParticleSystem mParticle in mParticles)
		{
			ParticleManager.Despawn(mParticle.transform);
		}
		mParticles.Clear();
	}
}
