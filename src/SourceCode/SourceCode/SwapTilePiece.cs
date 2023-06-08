public class SwapTilePiece
{
	private TilePiece[] mSelectedTiles;

	private TilePiece[] mMatchingTiles;

	public SwapTilePiece(int inClickCount)
	{
		Init(inClickCount);
	}

	public void Init(int inClickCount)
	{
		mSelectedTiles = new TilePiece[inClickCount];
		Reset();
	}

	public void Reset()
	{
		mMatchingTiles = null;
		if (mSelectedTiles != null)
		{
			for (int i = 0; i < mSelectedTiles.Length; i++)
			{
				mSelectedTiles[i] = null;
			}
		}
	}

	public void Add(TilePiece inInfo, int inAtIndex)
	{
		mSelectedTiles[inAtIndex] = inInfo;
	}

	public TilePiece GetTileAtIndex(int inIndex)
	{
		if (mSelectedTiles == null || inIndex >= mSelectedTiles.Length)
		{
			return null;
		}
		return mSelectedTiles[inIndex];
	}

	public TilePiece.State GetState(int inIdx)
	{
		return mSelectedTiles[inIdx].pState;
	}

	public void SetMatchingTile(TilePiece[] inMatchingTiles)
	{
		mMatchingTiles = null;
		mMatchingTiles = inMatchingTiles;
	}

	public TilePiece[] GetMatchingTiles()
	{
		return mMatchingTiles;
	}

	public int GetMatchingTileCount()
	{
		if (mMatchingTiles == null)
		{
			return 0;
		}
		return mMatchingTiles.Length;
	}
}
