using System;
using UnityEngine;

[Serializable]
public class KASkinInfo
{
	public SnSound _Clip;

	public KASkinPositionInfo _PositionInfo = new KASkinPositionInfo();

	public KASkinScaleInfo _ScaleInfo = new KASkinScaleInfo();

	public KASkinColorInfo _ColorInfo = new KASkinColorInfo();

	public KASkinSpriteInfo _SpriteInfo = new KASkinSpriteInfo();

	public ParticleInfo _ParticleInfo = new ParticleInfo();

	public float _Duration = -1f;

	private float mEffectStartTime;

	private bool mIsEffectOn;

	private SnChannel mChannel;

	public bool pIsEffectOn => mIsEffectOn;

	public void DoEffect(bool inShowEffect, KAWidget widget = null)
	{
		_PositionInfo.ShowPositionEffect(inShowEffect);
		_ScaleInfo.ShowScaleEffect(inShowEffect, widget);
		_ColorInfo.ShowColorEffect(inShowEffect, widget);
		_SpriteInfo.ChangeWidgetSprite(inShowEffect, widget);
		mIsEffectOn = inShowEffect;
		if (mIsEffectOn)
		{
			mEffectStartTime = Time.realtimeSinceStartup;
			if (_Clip != null && _Clip._AudioClip != null && !string.IsNullOrEmpty(_Clip._Settings._Pool))
			{
				mChannel = SnChannel.Play(_Clip._AudioClip, _Clip._Settings, _Clip._Triggers, inForce: false);
			}
		}
		else if (mChannel != null)
		{
			mChannel.Stop();
			mChannel = null;
		}
		PlayParticle(mIsEffectOn);
	}

	public void PlayParticle(bool isPlay)
	{
		if (_ParticleInfo == null || _ParticleInfo._Particles == null || _ParticleInfo._Particles.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _ParticleInfo._Particles.Length; i++)
		{
			if (!(_ParticleInfo._Particles[i]._Particle != null))
			{
				continue;
			}
			if (isPlay)
			{
				if (_ParticleInfo._Particles[i]._StartSize != 0f)
				{
					ParticleSystem.MainModule main = _ParticleInfo._Particles[i]._Particle.main;
					main.startSize = _ParticleInfo._Particles[i]._StartSize;
				}
				_ParticleInfo._Particles[i]._Particle.Play();
			}
			else
			{
				_ParticleInfo._Particles[i]._Particle.Stop();
			}
		}
	}

	public void Update()
	{
		if (mIsEffectOn && _Duration > 0f && Time.realtimeSinceStartup - mEffectStartTime > _Duration)
		{
			DoEffect(inShowEffect: false);
		}
	}
}
