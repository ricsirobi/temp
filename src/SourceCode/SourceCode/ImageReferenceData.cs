using UnityEngine;

public class ImageReferenceData
{
	private int mRefCount;

	private string mRefName = string.Empty;

	private UIAtlas mAtlas;

	private Texture mTexture;

	public Texture pTexture
	{
		get
		{
			return mTexture;
		}
		set
		{
			mTexture = value;
		}
	}

	public int pRefCount
	{
		get
		{
			return mRefCount;
		}
		set
		{
			mRefCount = value;
		}
	}

	public string pRefName
	{
		get
		{
			return mRefName;
		}
		set
		{
			mRefName = value;
		}
	}

	public UIAtlas pAtlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			mAtlas = value;
		}
	}
}
