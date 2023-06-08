using System;
using JSGames.Tween;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JSGames.UI;

[Serializable]
public class UIEffects
{
	[Serializable]
	public class PositionEffectData
	{
		public float _Time = 0.1f;

		public Vector3 _Offset;

		public EaseType _PositionEffect = EaseType.EaseInOutBack;

		public UIBehaviour _Widget;
	}

	[Serializable]
	public class ScaleEffectData
	{
		public float _Time = 0.1f;

		public Vector3 _Scale = Vector2.one * 1.5f;

		public EaseType _ScaleEffect = EaseType.EaseInOutBack;

		public UIBehaviour _Widget;
	}

	[Serializable]
	public class RotationEffectData
	{
		public float _Time = 0.1f;

		public Vector3 _Rotate;

		public EaseType _RotateEffect = EaseType.EaseInOutBack;

		public UIBehaviour _Widget;
	}

	[Serializable]
	public class ColorEffectData
	{
		public float _Time = 0.1f;

		public Color _Color = Color.grey;

		public EaseType _ColorEffect = EaseType.EaseInOutBack;

		public UIBehaviour _Widget;
	}

	[Serializable]
	public class SpriteEffectData
	{
		public Sprite _Sprite;

		public UIBehaviour _Widget;
	}

	[Serializable]
	public class ParticleEffectData
	{
		public ParticleSystem _Particle;

		public float _StartSize;
	}

	public abstract class EffectBase<ModifyType, WidgetDataType>
	{
		public bool _UseEffect;

		public WidgetDataType[] _ApplyTo;

		protected ModifyType[] mOriginalValues;

		public virtual void CacheOriginalValues()
		{
			mOriginalValues = new ModifyType[_ApplyTo.Length];
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				CacheOriginalValue(i);
			}
		}

		public abstract void CacheOriginalValue(int index);

		public abstract void ShowEffect(bool showEffect);
	}

	[Serializable]
	public class PositionEffect : EffectBase<Vector3, PositionEffectData>
	{
		public override void CacheOriginalValue(int index)
		{
			if (_ApplyTo[index]._Widget != null)
			{
				mOriginalValues[index] = _ApplyTo[index]._Widget.transform.localPosition;
			}
		}

		public override void ShowEffect(bool showEffect)
		{
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				PositionEffectData positionEffectData = _ApplyTo[i];
				if (positionEffectData._Widget != null)
				{
					Vector3 to = (showEffect ? (mOriginalValues[i] + positionEffectData._Offset) : mOriginalValues[i]);
					JSGames.Tween.Tween.MoveLocalTo(positionEffectData._Widget.gameObject, positionEffectData._Widget.transform.localPosition, to, new TweenParam(positionEffectData._Time, positionEffectData._PositionEffect));
				}
			}
		}
	}

	[Serializable]
	public class ScaleEffect : EffectBase<Vector3, ScaleEffectData>
	{
		public override void CacheOriginalValue(int index)
		{
			if (_ApplyTo[index]._Widget != null)
			{
				mOriginalValues[index] = _ApplyTo[index]._Widget.transform.localScale;
			}
		}

		public override void ShowEffect(bool showEffect)
		{
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				ScaleEffectData scaleEffectData = _ApplyTo[i];
				if (scaleEffectData._Widget != null)
				{
					Vector3 to = (showEffect ? Vector3.Scale(mOriginalValues[i], scaleEffectData._Scale) : mOriginalValues[i]);
					JSGames.Tween.Tween.ScaleTo(scaleEffectData._Widget.gameObject, scaleEffectData._Widget.transform.localScale, to, new TweenParam(scaleEffectData._Time, scaleEffectData._ScaleEffect));
				}
			}
		}
	}

	[Serializable]
	public class RotateEffect : EffectBase<Vector3, RotationEffectData>
	{
		public override void CacheOriginalValue(int index)
		{
			if (_ApplyTo[index]._Widget != null)
			{
				mOriginalValues[index] = _ApplyTo[index]._Widget.transform.localPosition;
			}
		}

		public override void ShowEffect(bool showEffect)
		{
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				RotationEffectData rotationEffectData = _ApplyTo[i];
				if (rotationEffectData._Widget != null)
				{
					Vector3 to = (showEffect ? (mOriginalValues[i] + rotationEffectData._Rotate) : mOriginalValues[i]);
					JSGames.Tween.Tween.RotateLocalTo(rotationEffectData._Widget.gameObject, rotationEffectData._Widget.transform.localPosition, to, new TweenParam(rotationEffectData._Time, rotationEffectData._RotateEffect));
				}
			}
		}
	}

	[Serializable]
	public class ColorEffect : EffectBase<Color, ColorEffectData>
	{
		public override void CacheOriginalValue(int index)
		{
			Graphic graphic = _ApplyTo[index]._Widget as Graphic;
			if (graphic != null)
			{
				mOriginalValues[index] = graphic.color;
			}
		}

		public override void ShowEffect(bool showEffect)
		{
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				ColorEffectData colorEffectData = _ApplyTo[i];
				if (colorEffectData._Widget != null)
				{
					Graphic graphic = colorEffectData._Widget as Graphic;
					if (graphic != null)
					{
						Color to = (showEffect ? colorEffectData._Color : mOriginalValues[i]);
						JSGames.Tween.Tween.ColorTo(graphic.gameObject, graphic.color, to, new TweenParam(colorEffectData._Time, colorEffectData._ColorEffect));
					}
				}
			}
		}
	}

	[Serializable]
	public class SpriteEffect : EffectBase<Sprite, SpriteEffectData>
	{
		public override void CacheOriginalValue(int index)
		{
			Image image = _ApplyTo[index]._Widget as Image;
			if (image != null)
			{
				mOriginalValues[index] = image.sprite;
			}
		}

		public override void ShowEffect(bool showEffect)
		{
			for (int i = 0; i < _ApplyTo.Length; i++)
			{
				SpriteEffectData spriteEffectData = _ApplyTo[i];
				if (!(spriteEffectData._Widget != null))
				{
					continue;
				}
				Image image = spriteEffectData._Widget as Image;
				if (image != null)
				{
					if (showEffect)
					{
						image.sprite = spriteEffectData._Sprite;
					}
					else
					{
						image.sprite = mOriginalValues[i];
					}
				}
			}
		}
	}

	[Serializable]
	public class ParticleEffect
	{
		public ParticleEffectData[] _Particles;
	}

	public SnSound _Clip;

	public PositionEffect _PositionEffect = new PositionEffect();

	public ScaleEffect _ScaleEffect = new ScaleEffect();

	public RotateEffect _RotateEffect = new RotateEffect();

	public ColorEffect _ColorEffect = new ColorEffect();

	public SpriteEffect _SpriteEffect = new SpriteEffect();

	public ParticleEffect _ParticleEffect = new ParticleEffect();

	public float _MaxDuration = -1f;

	private SnChannel mChannel;

	public bool pIsOn { get; private set; }

	public void PlaySound(bool on)
	{
		if (on)
		{
			if (_Clip._AudioClip != null)
			{
				mChannel = SnChannel.Play(_Clip._AudioClip, _Clip._Settings, _Clip._Triggers, inForce: false);
			}
		}
		else if (mChannel != null)
		{
			if (SnUtility.SafeClipCompare(mChannel.pClip, _Clip._AudioClip))
			{
				mChannel.Stop();
			}
			mChannel = null;
		}
	}

	public void PlayParticle(bool isPlay)
	{
		if (_ParticleEffect == null || _ParticleEffect._Particles == null || _ParticleEffect._Particles.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _ParticleEffect._Particles.Length; i++)
		{
			if (!(_ParticleEffect._Particles[i]._Particle != null))
			{
				continue;
			}
			if (isPlay)
			{
				if (_ParticleEffect._Particles[i]._StartSize != 0f)
				{
					ParticleSystem.MainModule main = _ParticleEffect._Particles[i]._Particle.main;
					main.startSize = _ParticleEffect._Particles[i]._StartSize;
				}
				_ParticleEffect._Particles[i]._Particle.Play();
			}
			else
			{
				_ParticleEffect._Particles[i]._Particle.Stop();
			}
		}
	}
}
