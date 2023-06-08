using System;
using UnityEngine;

[Serializable]
public class HintTile
{
	public float _HintTimer = 1f;

	public float _HintScore;

	public GameObject _Effect;

	private float mElapsedTime;

	private TilePiece mHintTile;

	public bool pIsTileFound => mHintTile != null;

	public bool DoSearch(ref float inAfterScore)
	{
		if (mHintTile != null)
		{
			return false;
		}
		mElapsedTime += Time.deltaTime;
		if (mElapsedTime > _HintTimer)
		{
			return inAfterScore >= _HintScore;
		}
		return false;
	}

	public void ShowEffect(bool inShow)
	{
		if (_Effect != null)
		{
			_Effect.SetActive(inShow);
		}
	}

	public void Reset()
	{
		mElapsedTime = 0f;
		mHintTile = null;
		ShowEffect(inShow: false);
	}

	public void SetTile(TilePiece inTile)
	{
		mHintTile = inTile;
		if (mHintTile != null)
		{
			mHintTile.OnHint(inAnimate: true);
		}
	}
}
