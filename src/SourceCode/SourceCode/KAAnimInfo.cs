using System;

[Serializable]
public class KAAnimInfo
{
	public string _Name;

	public KAAnimSpriteInfo[] _SpriteInfo;

	public float _Length;

	public int _LoopCount;

	public UISprite _Sprite;

	public string _ClipName;

	private string mRollBackSprite;

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
		if (!(_Sprite == null))
		{
			mRollBackSprite = _Sprite.spriteName;
		}
	}

	public void SetOriginalSprite()
	{
		if (!(_Sprite == null))
		{
			_Sprite.spriteName = mRollBackSprite;
		}
	}
}
