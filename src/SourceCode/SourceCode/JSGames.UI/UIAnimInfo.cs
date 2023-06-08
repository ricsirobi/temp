using System;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI;

[Serializable]
public class UIAnimInfo
{
	public string _Name;

	public UIAnimSpriteInfo[] _SpriteInfo;

	public float _Length;

	public int _LoopCount;

	public Image _Image;

	public string _ClipName;

	private Sprite mOriginalSprite;

	private int mCachedLoopCount;

	public int pLoopCount
	{
		get
		{
			return mCachedLoopCount;
		}
		set
		{
			mCachedLoopCount = value;
		}
	}

	public void CacheOriginalSprite()
	{
		if (_Image != null)
		{
			mOriginalSprite = _Image.sprite;
		}
	}

	public void SetOriginalSprite()
	{
		if (_Image != null)
		{
			_Image.sprite = mOriginalSprite;
		}
	}
}
