using System;
using UnityEngine;

namespace JSGames.UI;

public class UIAnim2D : KAMonoBase
{
	public Action OnAnimationComplete;

	public UIAnimInfo[] _Anims;

	public int _StartUpIndex = -1;

	private bool mPlaying;

	private float mLastTime;

	private float mNextDuration;

	private int mLoopCount;

	private UIWidget mWidget;

	public UIAnimInfo pCurrentAnimInfo { get; private set; }

	public int pSpriteIndex { get; private set; }

	public bool mIsPlaying => mPlaying;

	public event Action<UIAnim2D> OnUpdate;

	private void Awake()
	{
		mWidget = base.gameObject.GetComponent<UIWidget>();
		if (mWidget != null)
		{
			mWidget.SetAnim2D(this);
		}
		if (_Anims != null && _Anims.Length != 0)
		{
			UIAnimInfo[] anims = _Anims;
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

	private void Update()
	{
		if (!mPlaying || !(Time.realtimeSinceStartup - mLastTime > mNextDuration))
		{
			return;
		}
		if (pSpriteIndex >= pCurrentAnimInfo._SpriteInfo.Length)
		{
			pSpriteIndex = 0;
			if (mLoopCount == pCurrentAnimInfo.pLoopCount)
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
		if (!(pCurrentAnimInfo._Image == null) && pCurrentAnimInfo._SpriteInfo.Length != 0)
		{
			pCurrentAnimInfo._Image.sprite = pCurrentAnimInfo._SpriteInfo[pSpriteIndex]._Sprite;
			mLastTime = Time.realtimeSinceStartup;
			if (pCurrentAnimInfo._Length <= 0f)
			{
				mNextDuration = pCurrentAnimInfo._SpriteInfo[pSpriteIndex]._Time;
			}
			if (this.OnUpdate != null)
			{
				this.OnUpdate(this);
			}
			pSpriteIndex++;
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
			pCurrentAnimInfo = _Anims[animIdx];
			_Anims[animIdx].pLoopCount = _Anims[animIdx]._LoopCount;
			mPlaying = true;
			pSpriteIndex = 0;
			mLoopCount = 0;
			mLastTime = 0f;
			if (pCurrentAnimInfo._Length > 0f)
			{
				mNextDuration = pCurrentAnimInfo._Length / (float)pCurrentAnimInfo._SpriteInfo.Length;
			}
			else if (pCurrentAnimInfo._SpriteInfo.Length != 0)
			{
				mNextDuration = pCurrentAnimInfo._SpriteInfo[0]._Time;
			}
			UpdateSprite();
			PlayLegacyAnim(_Anims[animIdx]._ClipName);
		}
	}

	public void Stop(string animName)
	{
		if (mPlaying && pCurrentAnimInfo != null && pCurrentAnimInfo._Name == animName)
		{
			Stop();
		}
	}

	public void Stop()
	{
		mPlaying = false;
		if (mWidget != null && pCurrentAnimInfo != null)
		{
			pCurrentAnimInfo.SetOriginalSprite();
			mWidget.Anim2DAnimEnded(GetIndex(pCurrentAnimInfo._Name));
			if (OnAnimationComplete != null)
			{
				OnAnimationComplete();
			}
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

	private void PlayLegacyAnim(string clipName)
	{
		if (clipName != null && base.animation != null && base.animation.GetClip(clipName) != null)
		{
			base.animation.Play(clipName);
		}
	}
}
