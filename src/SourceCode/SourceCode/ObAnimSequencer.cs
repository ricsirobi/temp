using UnityEngine;

public class ObAnimSequencer : KAMonoBase
{
	public string[] _AnimStringSequence;

	public AnimationClip[] _AnimClipSequence;

	public bool _Playing = true;

	public bool _Randomize;

	public bool _Looping = true;

	public float _BlendTime = 0.25f;

	public float _RandomDelay;

	private int mCurIndex = -1;

	private int mNumAnims;

	private AnimationState mCurAnimState;

	private float mCurAnimLength;

	private void Start()
	{
		_RandomDelay = Random.Range(0f, _RandomDelay);
		if (_AnimClipSequence.Length != 0)
		{
			mNumAnims = _AnimClipSequence.Length;
		}
		else
		{
			mNumAnims = _AnimStringSequence.Length;
		}
	}

	private void OnEnable()
	{
		mCurAnimState = null;
	}

	public void StopSequence()
	{
		_Playing = false;
		base.animation.Stop();
	}

	private AnimationState GetAnimationState(int index)
	{
		AnimationState animationState = null;
		if (index < 0)
		{
			return animationState;
		}
		if (_AnimClipSequence.Length > index)
		{
			foreach (AnimationState item in base.animation)
			{
				if (item.clip == _AnimClipSequence[index])
				{
					animationState = item;
					break;
				}
			}
		}
		else if (_AnimStringSequence.Length > index && base.animation[_AnimStringSequence[index]] != null)
		{
			animationState = base.animation[_AnimStringSequence[index]];
		}
		if (animationState == null)
		{
			UtDebug.LogWarning("Missing animation in sequence " + index + " for " + base.gameObject.name);
		}
		return animationState;
	}

	private void PlayNextAnimation()
	{
		if (_Randomize)
		{
			mCurIndex = Random.Range(0, mNumAnims);
		}
		else
		{
			mCurIndex++;
		}
		if (mCurIndex >= mNumAnims)
		{
			if (!_Looping)
			{
				_Playing = false;
				return;
			}
			mCurIndex = 0;
		}
		mCurAnimState = GetAnimationState(mCurIndex);
		if (mCurAnimState != null)
		{
			mCurAnimState.wrapMode = WrapMode.ClampForever;
			mCurAnimState.time = 0f;
			mCurAnimLength = mCurAnimState.length;
			if (_BlendTime != 0f)
			{
				base.animation.CrossFade(mCurAnimState.name, _BlendTime);
			}
			else
			{
				base.animation.Play(mCurAnimState.name);
			}
		}
		else
		{
			mCurIndex = -1;
			_Playing = false;
			mCurAnimLength = 0f;
		}
	}

	private void Update()
	{
		_RandomDelay -= Time.deltaTime;
		if (_RandomDelay <= 0f)
		{
			_RandomDelay = 0f;
			if (_Playing && mNumAnims > 0 && (mCurAnimState == null || mCurAnimState.time >= mCurAnimLength))
			{
				PlayNextAnimation();
			}
		}
	}
}
