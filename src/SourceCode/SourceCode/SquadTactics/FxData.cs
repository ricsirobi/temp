using System;
using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

[Serializable]
public class FxData
{
	public enum TimerType
	{
		ANIMATION,
		PARTICLE
	}

	public AudioClip _Sound;

	public List<FxObjectData> _EnableObjects = new List<FxObjectData>();

	public TimerType _TimerType = TimerType.PARTICLE;

	public bool _AlwaysShow;

	private float mDuration;

	public float pDuration => mDuration;

	public FxData()
	{
	}

	public FxData(AudioClip sound, List<FxObjectData> enableObjects, TimerType timerType, bool alwaysShow, Transform parent)
	{
		_Sound = sound;
		_TimerType = timerType;
		_AlwaysShow = alwaysShow;
		mDuration = 1f;
		foreach (FxObjectData enableObject in enableObjects)
		{
			if (!(enableObject._Object != null))
			{
				continue;
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(enableObject._Object, parent);
			gameObject.SetActive(value: false);
			_EnableObjects.Add(new FxObjectData(gameObject));
			switch (_TimerType)
			{
			case TimerType.ANIMATION:
			{
				Animation[] componentsInChildren2 = enableObject._Object.GetComponentsInChildren<Animation>();
				foreach (Animation animation in componentsInChildren2)
				{
					if (animation != null && animation.clip.length > mDuration)
					{
						mDuration = animation.clip.length;
					}
				}
				break;
			}
			case TimerType.PARTICLE:
			{
				ParticleSystem[] componentsInChildren = enableObject._Object.GetComponentsInChildren<ParticleSystem>();
				foreach (ParticleSystem particleSystem in componentsInChildren)
				{
					if (particleSystem.main.duration > mDuration)
					{
						mDuration = particleSystem.main.duration;
					}
				}
				break;
			}
			}
		}
	}

	public FxData(FxData fxData, Transform parent)
		: this(fxData._Sound, fxData._EnableObjects, fxData._TimerType, fxData._AlwaysShow, parent)
	{
	}

	public virtual void Initialize(Transform parent)
	{
		foreach (FxObjectData enableObject in _EnableObjects)
		{
			enableObject.EnableObject(enable: false, parent);
		}
	}

	public virtual void PlayFx(Transform parent)
	{
		foreach (FxObjectData enableObject in _EnableObjects)
		{
			enableObject.EnableObject(enable: true, parent);
		}
		if (_Sound != null)
		{
			SnChannel.Play(_Sound, "STEffect_Pool", inForce: true);
		}
	}

	public virtual void ResetFX(Transform parent)
	{
		foreach (FxObjectData enableObject in _EnableObjects)
		{
			enableObject.EnableObject(enable: false, parent);
		}
	}
}
