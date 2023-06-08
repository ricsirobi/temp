using System;
using UnityEngine;

[Serializable]
public class AvAvatarAnimation
{
	public const uint USE_ALL = uint.MaxValue;

	public const uint USE_OFFSET = 1u;

	public const uint USE_FADE_LENGTH = 2u;

	public const uint USE_WRAP_MODE = 4u;

	public const uint USE_SPEED = 8u;

	public string mName;

	public float mOffset;

	public float mFadeLength = 0.2f;

	public WrapMode mWrapMode = WrapMode.Once;

	public float mSpeed = 1f;

	public uint mEnabledSettings = uint.MaxValue;

	public float mWeight;

	public AnimationBlendMode mBlendMode;

	public AvAvatarAnimation(string name)
	{
		mName = name;
	}

	public AvAvatarAnimation(string name, float offset, float fadelength, WrapMode wrapmode, float speed)
	{
		mName = name;
		mOffset = offset;
		mFadeLength = fadelength;
		mWrapMode = wrapmode;
		mSpeed = speed;
	}

	public bool IsEnabled(uint inFlag)
	{
		return (inFlag & mEnabledSettings) == inFlag;
	}
}
