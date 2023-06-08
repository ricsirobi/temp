using System;
using UnityEngine;

public class KAAnim2D : KAMonoBase
{
	public KAAnimInfo[] _Anims;

	public int _StartUpIndex = -1;

	private KAAnimInfo mCurrentAnimInfo;

	private int mSpriteIndex;

	private bool mPlaying;

	private float mLastTime;

	private float mNextDuration;

	private int mLoopCount;

	private KAWidget mWidget;

	public KAAnimInfo pCurrentAnimInfo => mCurrentAnimInfo;

	public int pSpriteIndex => mSpriteIndex;

	public bool mIsPlaying => mPlaying;

	public event Action<KAAnim2D> OnUpdate;

	public void Awake()
	{
		mWidget = base.gameObject.GetComponent<KAWidget>();
		if (mWidget != null)
		{
			mWidget.SetAnim2DReference(this);
		}
		if (_Anims != null && _Anims.Length != 0)
		{
			KAAnimInfo[] anims = _Anims;
			for (int i = 0; i < anims.Length; i++)
			{
				anims[i].CacheOriginalSprite();
			}
		}
		if (_StartUpIndex >= 0)
		{
			Play(_StartUpIndex);
		}
	}

	public void Update()
	{
		if (!mPlaying || !(Time.realtimeSinceStartup - mLastTime > mNextDuration))
		{
			return;
		}
		if (mSpriteIndex >= mCurrentAnimInfo._SpriteInfo.Length)
		{
			mSpriteIndex = 0;
			if (mLoopCount == mCurrentAnimInfo.pLoopCount)
			{
				Stop();
				return;
			}
			mLoopCount++;
			UpdateSprite();
		}
		else
		{
			UpdateSprite();
		}
	}

	private void UpdateSprite()
	{
		if (!(mCurrentAnimInfo._Sprite == null) && mCurrentAnimInfo._SpriteInfo.Length != 0)
		{
			mCurrentAnimInfo._Sprite.spriteName = mCurrentAnimInfo._SpriteInfo[mSpriteIndex]._Sprite;
			mLastTime = Time.realtimeSinceStartup;
			if (mCurrentAnimInfo._Length <= 0f)
			{
				mNextDuration = mCurrentAnimInfo._SpriteInfo[mSpriteIndex]._Time;
			}
			if (this.OnUpdate != null)
			{
				this.OnUpdate(this);
			}
			mSpriteIndex++;
		}
	}

	public void Play(string animName, int loopCount)
	{
		int index = GetIndex(animName);
		Play(index, loopCount);
	}

	public void Play(int idx, int loopCount)
	{
		if (idx >= 0 && idx < _Anims.Length)
		{
			_Anims[idx].pLoopCount = loopCount;
			Play(idx);
		}
	}

	public void Play(string animName)
	{
		int index = GetIndex(animName);
		if (index >= 0)
		{
			Play(index, _Anims[index]._LoopCount);
		}
	}

	public void Play(int animIdx)
	{
		if (_Anims != null && _Anims.Length > animIdx)
		{
			mCurrentAnimInfo = _Anims[animIdx];
			_Anims[animIdx].pLoopCount = _Anims[animIdx]._LoopCount;
			mPlaying = true;
			mSpriteIndex = 0;
			mLoopCount = 0;
			mLastTime = 0f;
			if (mCurrentAnimInfo._Length > 0f)
			{
				mNextDuration = mCurrentAnimInfo._Length / (float)mCurrentAnimInfo._SpriteInfo.Length;
			}
			else if (mCurrentAnimInfo._SpriteInfo.Length != 0)
			{
				mNextDuration = mCurrentAnimInfo._SpriteInfo[0]._Time;
			}
			UpdateSprite();
			PlayAnim3D(_Anims[animIdx]._ClipName);
		}
	}

	public void Stop(string animName)
	{
		if (mPlaying && mCurrentAnimInfo != null && mCurrentAnimInfo._Name == animName)
		{
			Stop();
		}
	}

	public void Stop()
	{
		mPlaying = false;
		if (mWidget != null && mCurrentAnimInfo != null)
		{
			mWidget.OnAnimEnd(GetIndex(mCurrentAnimInfo._Name));
		}
	}

	private int GetIndex(string animName)
	{
		if (_Anims != null && _Anims.Length != 0)
		{
			for (int i = 0; i < _Anims.Length; i++)
			{
				if (_Anims[i]._Name == animName)
				{
					return i;
				}
			}
		}
		return -1;
	}

	private void PlayAnim3D(string clipName)
	{
		if (clipName != null && base.animation != null && base.animation.GetClip(clipName) != null)
		{
			base.animation.Play(clipName);
		}
	}
}
