using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileMatchPuzzleGame : MonoBehaviour
{
	public enum GameState
	{
		PAUSED,
		PLAYING
	}

	[Serializable]
	public class ScoreInfo : IComparable
	{
		public int _TileMatchCount;

		public float _Score;

		public GameObject _Effect;

		public int CompareTo(object inObj)
		{
			return (inObj as ScoreInfo)._TileMatchCount.CompareTo(_TileMatchCount);
		}
	}

	private struct TileInfo
	{
		public int _Row;

		public int _Col;

		public int _ID;
	}

	public GameObject _Grid;

	public TileGO[] _Tiles;

	public HintTile _Hint;

	public ScoreInfo[] _ScoreInfo;

	public int _RowCount = 5;

	public int _ColumnCount = 5;

	public float _SpawnPosY = 10f;

	public float _TileMoveSpeed = 2f;

	public float _TileMinSpeed = 0.5f;

	public float _TileSpeedDamp = 10f;

	public bool _DebugGenPredefineGrid;

	public float _TileSelectAnimateSpeed = 10f;

	public float _TileAnimateScalePer = 1.5f;

	public bool _DrawGridLine;

	public int _TileSwapDistance;

	public int _ClickCount = 2;

	public int _MinMatchCount = 3;

	protected TilePiece[,] mTilesMap;

	protected RandomProportional mRandGen;

	protected Bounds mGridBound;

	protected Vector2 mGridBlockSize;

	protected GameState mGameState;

	protected GridState mGridState;

	protected int mClickCount;

	protected SwapTilePiece mSwapInfo;

	protected List<TilePiece> mMovingTiles;

	protected OnGridStateChangeDelegate mOnGridStateChangeEvent;

	protected OnGameStateChangeDelegate mOnGameStateChangeEvent;

	protected OnScoreUpdateDelegate mOnScoreUpdateEvent;

	protected float mScore;

	protected float mElaspedTime;

	protected ComboTileGO[] mComboTiles;

	protected List<TilePiece> mMatchedTiles = new List<TilePiece>();

	protected TilePiece.State mFallingTileStateCheck = TilePiece.State.IDLE | TilePiece.State.FROZEN;

	protected TilePiece.State mTileClickStateCheck = TilePiece.State.IDLE;

	protected ShuffleBag<int> mTileIDBag;

	private static TileMatchPuzzleGame mInstance;

	public GameObject _DebugObj;

	private GameObject mDebugObject;

	private string mDebugMsg = "";

	public static TileMatchPuzzleGame pInstance => mInstance;

	protected virtual void Awake()
	{
		mInstance = this;
		mRandGen = new RandomProportional();
	}

	protected virtual void Start()
	{
		GetGridInfo();
		if (_ScoreInfo != null)
		{
			Array.Sort(_ScoreInfo);
		}
	}

	protected virtual void Update()
	{
		if (mGameState != 0)
		{
			switch (mGridState)
			{
			case GridState.INTERACTIVE:
			case GridState.TILE_SELECTED:
				OnTileClick();
				break;
			case GridState.SWAPPING:
				OnTileSwaping();
				break;
			case GridState.FALLING:
				OnTileFalling();
				break;
			case GridState.FILLING:
			case GridState.FREEZ:
				break;
			}
		}
	}

	public virtual void StartGame()
	{
		if (mGameState != GameState.PLAYING)
		{
			mElaspedTime = 0f;
			mScore = 0f;
			mGameState = GameState.PLAYING;
			if (mOnGridStateChangeEvent == null)
			{
				mOnGridStateChangeEvent = OnGridStateChange;
			}
			mMovingTiles = new List<TilePiece>();
			mSwapInfo = new SwapTilePiece(_ClickCount);
			AssignIDToNormalTiles();
			AssignUniqeIDToComboTiles();
			ResetGrid();
			DrawDebug();
		}
	}

	protected void AssignIDToNormalTiles()
	{
		for (int i = 0; i < _Tiles.Length; i++)
		{
			_Tiles[i]._Normal.pID = i;
		}
	}

	protected virtual void AssignUniqeIDToComboTiles()
	{
		if (mComboTiles == null)
		{
			return;
		}
		Dictionary<Type, int> dictionary = new Dictionary<Type, int>();
		int num = 100;
		for (int i = 0; i < mComboTiles.Length; i++)
		{
			for (int j = 0; j < mComboTiles[i]._Tiles.Length; j++)
			{
				TilePiece tile = mComboTiles[i]._Tiles[j]._Tile;
				if (tile != null)
				{
					Type type = tile.GetType();
					if (!dictionary.ContainsKey(type))
					{
						dictionary[type] = num;
						tile.pID = dictionary[type];
						num++;
					}
					else
					{
						tile.pID = dictionary[type];
					}
				}
			}
		}
		dictionary.Clear();
		dictionary = null;
	}

	protected void GetGridInfo()
	{
		if (_Grid != null)
		{
			mGridBound = _Grid.GetComponent<Renderer>().bounds;
			mGridBlockSize.x = (mGridBound.max.x - mGridBound.min.x) / (float)_ColumnCount;
			mGridBlockSize.y = (mGridBound.max.y - mGridBound.min.y) / (float)_RowCount;
		}
	}

	protected virtual void ResetGrid()
	{
		if (_DebugGenPredefineGrid)
		{
			GenerateTestGrid();
			return;
		}
		mTilesMap = new TilePiece[_RowCount, _ColumnCount];
		float num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
		float num2 = mGridBound.min.y;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				int num3 = -1;
				mTilesMap[i, j] = null;
				int horizontalMatchSize;
				int verticalMatchSize;
				do
				{
					num3 = GetRandomTileIdx();
					horizontalMatchSize = GetHorizontalMatchSize(i, j, num3);
					verticalMatchSize = GetVerticalMatchSize(i, j, num3);
				}
				while (horizontalMatchSize >= _MinMatchCount - 1 || verticalMatchSize >= _MinMatchCount - 1);
				Vector3 position = new Vector3(num, num2 + mGridBlockSize.y * 0.5f, 0f);
				TilePiece tilePiece = InstantiateTile(num3);
				if (!(tilePiece == null))
				{
					mTilesMap[i, j] = tilePiece;
					mTilesMap[i, j].SetGridParams(i, j, TileType.NORMAL, num3);
					tilePiece.transform.position = position;
					num += mGridBlockSize.x;
				}
			}
			num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
			num2 += mGridBlockSize.y;
		}
	}

	protected virtual TilePiece InstantiateTile(int inIdx)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(_Tiles[inIdx]._Normal.gameObject);
		TilePiece tilePiece = null;
		if (gameObject != null)
		{
			tilePiece = gameObject.GetComponent(typeof(TilePiece)) as TilePiece;
			if (tilePiece == null)
			{
				tilePiece = gameObject.AddComponent(typeof(TilePiece)) as TilePiece;
			}
			tilePiece.transform.parent = base.transform;
			tilePiece.SetState(TilePiece.State.IDLE);
			tilePiece.SetVisibilityMovement(mGridBound.min.y - 2.75f, _TileMoveSpeed, _TileMinSpeed, _TileSpeedDamp);
		}
		return tilePiece;
	}

	protected virtual TilePiece InstantiateComboTile(TilePiece inSourceTile)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(inSourceTile.gameObject);
		TilePiece tilePiece = null;
		if (gameObject != null)
		{
			tilePiece = gameObject.GetComponent(typeof(TilePiece)) as TilePiece;
			if (tilePiece == null)
			{
				tilePiece = gameObject.AddComponent(typeof(TilePiece)) as TilePiece;
			}
			tilePiece.pID = inSourceTile.pID;
			tilePiece.transform.parent = base.transform;
			tilePiece.SetVisibilityMovement(mGridBound.min.y - 2.75f, _TileMoveSpeed, _TileMinSpeed, _TileSpeedDamp);
		}
		return tilePiece;
	}

	protected bool IsTileAt(int inRow, int inCol)
	{
		if (mTilesMap == null || inRow > _RowCount || inRow < 0 || inCol > _ColumnCount || inCol < 0)
		{
			return false;
		}
		return mTilesMap[inRow, inCol] != null;
	}

	protected virtual int GetRandomTileIdx()
	{
		return GetRandomFromBag();
	}

	protected int GetRandomFromBag()
	{
		if (mTileIDBag == null)
		{
			mTileIDBag = new ShuffleBag<int>(_Tiles.Length);
			for (int i = 0; i < _Tiles.Length; i++)
			{
				mTileIDBag.Add(i, 3);
			}
		}
		return mTileIDBag.Next();
	}

	protected virtual void OnTileClick()
	{
		UpdateHint();
		if (Input.GetMouseButtonDown(0) && GetRowColFromMousePos(out var outRow, out var outCol))
		{
			mSwapInfo.Add(mTilesMap[outRow, outCol], mClickCount);
			mTilesMap[outRow, outCol].OnSelected(_TileSelectAnimateSpeed, _TileAnimateScalePer);
			SetGridState(GridState.TILE_SELECTED);
			mClickCount++;
			if (ValidateClick())
			{
				OnValidMove();
			}
		}
	}

	public bool GetRowColFromMousePos(out int outRow, out int outCol)
	{
		outRow = -1;
		outCol = -1;
		Vector3 mousePosition = Input.mousePosition;
		Vector3 worldPos = GetWorldPos(mousePosition);
		if (mGridBound.Contains(worldPos))
		{
			outRow = GetRow(worldPos.y);
			outCol = GetCol(worldPos.x);
			return mTilesMap[outRow, outCol].IsInState(mTileClickStateCheck);
		}
		return false;
	}

	public Vector3 GetWorldPos(Vector3 inScreenPos)
	{
		float z = _Grid.transform.position.z - Camera.main.transform.position.z;
		inScreenPos.z = z;
		Vector3 result = Camera.main.ScreenToWorldPoint(inScreenPos);
		result.z = mGridBound.min.z;
		return result;
	}

	protected virtual void OnValidMove()
	{
		if (IsValidMove())
		{
			SetGridState(GridState.SWAPPING);
		}
		else
		{
			ResetClick();
		}
	}

	protected void ResetClick()
	{
		if (mSwapInfo != null)
		{
			TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
			TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
			if (tileAtIndex != null)
			{
				tileAtIndex.OnUnSlect();
			}
			if (tileAtIndex2 != null)
			{
				tileAtIndex2.OnUnSlect();
			}
			mSwapInfo.Reset();
			mClickCount = 0;
		}
	}

	public virtual void ResetTileSelection()
	{
		SetGridState(GridState.INTERACTIVE);
		ResetClick();
	}

	protected virtual void UpdateHint()
	{
		if (_Hint == null || _Hint.pIsTileFound || !_Hint.DoSearch(ref mScore))
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				if (!(mTilesMap[i, j] == null))
				{
					if (GetHorizontalMatchSize(i, j, mTilesMap[i, j].pID) >= _MinMatchCount - 1)
					{
						flag = true;
						_Hint.SetTile(mTilesMap[i, j]);
						_Hint.ShowEffect(inShow: true);
						break;
					}
					if (GetVerticalMatchSize(i, j, mTilesMap[i, j].pID) >= _MinMatchCount - 1)
					{
						flag = true;
						_Hint.SetTile(mTilesMap[i, j]);
						_Hint.ShowEffect(inShow: true);
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
	}

	protected virtual bool IsValidMove()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		if (!IsInSameSlot() || tileAtIndex.IsInLimbo() || tileAtIndex2.IsInLimbo())
		{
			return false;
		}
		SwapSelectedTiles();
		if (GetHorizontalMatchSize(tileAtIndex.pRow, tileAtIndex.pColumn, tileAtIndex.pID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			return true;
		}
		if (GetVerticalMatchSize(tileAtIndex.pRow, tileAtIndex.pColumn, tileAtIndex.pID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			return true;
		}
		if (GetHorizontalMatchSize(tileAtIndex2.pRow, tileAtIndex2.pColumn, tileAtIndex2.pID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			return true;
		}
		if (GetVerticalMatchSize(tileAtIndex2.pRow, tileAtIndex2.pColumn, tileAtIndex2.pID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			return true;
		}
		SwapSelectedTiles();
		return false;
	}

	protected virtual bool IsFormingMatch(TilePiece inTile)
	{
		if (GetHorizontalMatchSize(inTile.pRow, inTile.pColumn, inTile.pID) >= _MinMatchCount)
		{
			return true;
		}
		if (GetVerticalMatchSize(inTile.pRow, inTile.pColumn, inTile.pID) >= _MinMatchCount)
		{
			return true;
		}
		return false;
	}

	protected virtual void GatherMatchingTiles(int inRow, int inCol, int inID, List<TilePiece> inMatchTileLst)
	{
		if (GetHorizontalMatchSize(inRow, inCol, inID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			StoreMatchingTile(inMatchTileLst);
		}
		mSwapInfo.SetMatchingTile(null);
		if (GetVerticalMatchSize(inRow, inCol, inID, inCacheMatching: true) >= _MinMatchCount || mSwapInfo.GetMatchingTileCount() >= _MinMatchCount)
		{
			StoreMatchingTile(inMatchTileLst);
		}
	}

	protected virtual bool IsInSameSlot()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		if (tileAtIndex.pRow != tileAtIndex2.pRow)
		{
			return tileAtIndex.pColumn == tileAtIndex2.pColumn;
		}
		return true;
	}

	public virtual bool IsGridInterActive()
	{
		if (mGridState != 0)
		{
			return mGridState == GridState.TILE_SELECTED;
		}
		return true;
	}

	protected virtual void SwapSelectedTiles()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		int pRow = tileAtIndex.pRow;
		int pColumn = tileAtIndex.pColumn;
		int pRow2 = tileAtIndex2.pRow;
		int pColumn2 = tileAtIndex2.pColumn;
		tileAtIndex.SetRowCol(pRow2, pColumn2);
		tileAtIndex2.SetRowCol(pRow, pColumn);
		mTilesMap[tileAtIndex.pRow, tileAtIndex.pColumn] = tileAtIndex;
		mTilesMap[tileAtIndex2.pRow, tileAtIndex2.pColumn] = tileAtIndex2;
	}

	protected virtual bool CanFormMatch(int inRow, int inColumn, int inTileID)
	{
		if (GetHorizontalMatchSize(inRow, inColumn, inTileID) >= _MinMatchCount)
		{
			return true;
		}
		if (GetVerticalMatchSize(inRow, inColumn, inTileID) >= _MinMatchCount)
		{
			return true;
		}
		return false;
	}

	protected virtual bool ValidateClick()
	{
		if (mClickCount != 0)
		{
			return mClickCount % _ClickCount == 0;
		}
		return false;
	}

	protected virtual void OnTileSwaping()
	{
		if (mSwapInfo.GetState(0) == TilePiece.State.IDLE && mSwapInfo.GetState(1) == TilePiece.State.IDLE)
		{
			ClearSwapMatch();
			SetGridState(GridState.FALLING);
		}
	}

	protected virtual void OnTileFalling()
	{
		if (!CheckMovingTile())
		{
			SetGridState(GridState.INTERACTIVE);
		}
	}

	protected bool CheckMovingTile()
	{
		if (mMovingTiles == null)
		{
			return false;
		}
		for (int num = mMovingTiles.Count - 1; num >= 0; num--)
		{
			TilePiece tilePiece = mMovingTiles[num];
			if (tilePiece == null || tilePiece.pState == TilePiece.State.DEAD)
			{
				mMovingTiles.RemoveAt(num);
			}
			else if ((tilePiece.pState & mFallingTileStateCheck) != 0)
			{
				GatherMatchingTiles(tilePiece.pRow, tilePiece.pColumn, tilePiece.pID, mMatchedTiles);
				mMovingTiles.RemoveAt(num);
			}
		}
		if (mMovingTiles.Count == 0)
		{
			ReplaceMatchedTiles(mMatchedTiles);
			mMatchedTiles.Clear();
		}
		return mMovingTiles.Count > 0;
	}

	protected virtual void StoreMatchingTile(List<TilePiece> inMatchTileLst)
	{
		TilePiece[] matchingTiles = mSwapInfo.GetMatchingTiles();
		for (int i = 0; i < matchingTiles.Length; i++)
		{
			inMatchTileLst.Add(matchingTiles[i]);
		}
	}

	protected virtual void ReplaceMatchedTiles(List<TilePiece> inMatchingTiles)
	{
		if (inMatchingTiles == null || inMatchingTiles.Count <= 0)
		{
			return;
		}
		if (_Hint != null)
		{
			_Hint.Reset();
		}
		inMatchingTiles = inMatchingTiles.Distinct().ToList();
		Dictionary<int, int> inTileCombo = new Dictionary<int, int>();
		mElaspedTime = 0f;
		for (int i = 0; i < inMatchingTiles.Count; i++)
		{
			TilePiece tilePiece = inMatchingTiles[i];
			if (!tilePiece.IsInLimbo())
			{
				if (!inTileCombo.ContainsKey(tilePiece.pID))
				{
					inTileCombo[tilePiece.pID] = 0;
				}
				Dictionary<int, int> dictionary = inTileCombo;
				int pID = tilePiece.pID;
				int value = dictionary[pID] + 1;
				dictionary[pID] = value;
				mTilesMap[tilePiece.pRow, tilePiece.pColumn] = null;
			}
		}
		for (int j = 0; j < inMatchingTiles.Count; j++)
		{
			TilePiece tilePiece2 = inMatchingTiles[j];
			ExecuteTileBehaviour(tilePiece2);
			DropTiles(tilePiece2.pRow, tilePiece2.pColumn);
			if (!tilePiece2.IsInLimbo())
			{
				tilePiece2.SetState(TilePiece.State.DEAD);
				tilePiece2 = null;
			}
		}
		inMatchingTiles = null;
		UpdateScore(ref inTileCombo);
		mDebugMsg = "";
		List<ComboTileInfo> list = new List<ComboTileInfo>();
		foreach (KeyValuePair<int, int> item in inTileCombo)
		{
			mDebugMsg = mDebugMsg + "Id: " + item.Key + " count: " + item.Value + "\n";
			OnTileCombination(item.Key, item.Value, list);
		}
		FillVoid(list);
		mDebugMsg = mDebugMsg + "ComboCount: " + inTileCombo.Count;
		list.Clear();
		inTileCombo.Clear();
		list = null;
		inTileCombo = null;
	}

	protected virtual void ExecuteTileBehaviour(TilePiece inTile)
	{
		if (inTile.pType == TileType.EXPLODE)
		{
			int[] pExplodeRange = inTile.pExplodeRange;
			int num = pExplodeRange.Length;
			for (int i = 0; i < num; i += 4)
			{
				ClearTilesInRange(inTile, pExplodeRange[i], pExplodeRange[i + 1], pExplodeRange[i + 2], pExplodeRange[i + 3]);
			}
			pExplodeRange = null;
		}
	}

	protected virtual void UpdateScore(ref Dictionary<int, int> inTileCombo)
	{
		foreach (KeyValuePair<int, int> item in inTileCombo)
		{
			OnUpdateScore(item.Key, item.Value);
		}
		if (mOnScoreUpdateEvent != null)
		{
			mOnScoreUpdateEvent(mScore);
		}
	}

	protected virtual void OnUpdateScore(int inTileID, int inMatchCount)
	{
		for (int i = 0; i < _ScoreInfo.Length; i++)
		{
			if (inMatchCount >= _ScoreInfo[i]._TileMatchCount)
			{
				mScore += _ScoreInfo[i]._Score;
				break;
			}
		}
	}

	protected virtual void OnTileCombination(int inTileID, int inMatchCount, List<ComboTileInfo> inComboTiles)
	{
		if (mComboTiles == null)
		{
			return;
		}
		ComboTileInfo comboTileInfo = null;
		for (int i = 0; i < mComboTiles.Length; i++)
		{
			if (inMatchCount < mComboTiles[i]._MatchCount)
			{
				continue;
			}
			for (int j = 0; j < mComboTiles[i]._SpawnCount; j++)
			{
				comboTileInfo = GetRandomComboTile(i);
				if (comboTileInfo != null)
				{
					inComboTiles.Add(comboTileInfo);
				}
			}
			break;
		}
	}

	public virtual void ClearTilesInRange(TilePiece inTile, int inStartRow, int inStartCol, int inEndRow, int inEndCol, GameObject inEffect = null)
	{
		inStartRow = Mathf.Clamp(inStartRow, 0, _RowCount - 1);
		inEndRow = Mathf.Clamp(inEndRow, 0, _RowCount - 1);
		inStartCol = Mathf.Clamp(inStartCol, 0, _ColumnCount - 1);
		inEndCol = Mathf.Clamp(inEndCol, 0, _ColumnCount - 1);
		int num = inEndRow - inStartRow;
		int num2 = inEndCol - inStartCol;
		List<TilePiece> list = new List<TilePiece>();
		num++;
		num2++;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				int num3 = inStartRow + i;
				int num4 = inStartCol + j;
				if (mTilesMap[num3, num4] != null)
				{
					TilePiece tilePiece = mTilesMap[num3, num4];
					if (tilePiece.pType != TileType.NORMAL && tilePiece != inTile)
					{
						list.Add(tilePiece);
						mTilesMap[num3, num4] = null;
						continue;
					}
					tilePiece.SetState(TilePiece.State.DEAD);
					tilePiece = null;
					mTilesMap[num3, num4] = null;
				}
				mTilesMap[num3, num4] = null;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			TilePiece tilePiece2 = list[k];
			ExecuteTileBehaviour(tilePiece2);
			tilePiece2.SetState(TilePiece.State.DEAD);
			tilePiece2 = null;
		}
		list.Clear();
		list = null;
		for (int l = 0; l < num; l++)
		{
			for (int m = 0; m < num2; m++)
			{
				int inRow = inStartRow + l;
				int inCol = inStartCol + m;
				DropTiles(inRow, inCol);
			}
		}
	}

	protected virtual void FillVoid(List<ComboTileInfo> inTileCombo)
	{
		if (mGridState == GridState.FILLING)
		{
			Debug.LogError("!!!! State Error:: Its arleady filling the Grid !!!! ");
			Debug.Break();
		}
		SetGridState(GridState.FILLING);
		int inIdx = 0;
		ComboTileInfo comboTileInfo = null;
		int num = 0;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				if (mTilesMap[i, j] == null)
				{
					comboTileInfo = GetNextFromList(inTileCombo, ref inIdx);
					TilePiece tilePiece = null;
					float yOffset = GetYOffset(i, j);
					if (comboTileInfo != null)
					{
						tilePiece = CreateComboTileAt(i, j, yOffset, comboTileInfo._Tile, inMove: true);
					}
					if (comboTileInfo == null || tilePiece == null)
					{
						tilePiece = CreateNewTileAt(i, j, yOffset);
					}
					if (tilePiece == null)
					{
						Debug.LogError("@@@@@@@@@@@@@@ Something went wrong tile at r: " + i + " c: " + j + " is not filled !!!  combo: " + (comboTileInfo == null));
					}
					mTilesMap[i, j] = tilePiece;
					mMovingTiles.Add(tilePiece);
					num++;
				}
			}
		}
		SetGridState(GridState.FALLING);
	}

	public float GetYOffset(int inRow, int inCol)
	{
		TilePiece tilePiece = null;
		for (int i = 0; i < _RowCount; i++)
		{
			if (mTilesMap[i, inCol] != null && mTilesMap[i, inCol].pState == TilePiece.State.MOVE_FALL)
			{
				tilePiece = mTilesMap[i, inCol];
			}
		}
		Vector3 vector = Vector3.up * 0.5f;
		if (tilePiece == null || mGridBound.Contains(tilePiece.transform.localPosition + vector))
		{
			return _SpawnPosY + mGridBlockSize.y * 0.5f;
		}
		float num = tilePiece.transform.localPosition.y + mGridBlockSize.y + _SpawnPosY;
		return Mathf.Abs(mGridBound.max.y - num);
	}

	private ComboTileInfo GetNextFromList(List<ComboTileInfo> inComboTile, ref int inIdx)
	{
		if (inComboTile == null || inComboTile.Count == 0 || inIdx >= inComboTile.Count)
		{
			return null;
		}
		ComboTileInfo result = inComboTile[inIdx];
		inIdx++;
		return result;
	}

	public virtual ComboTileInfo GetRandomComboTile(int inIdx)
	{
		if (mComboTiles == null)
		{
			return null;
		}
		float num = UnityEngine.Random.Range(0f, 1f);
		int num2 = UnityEngine.Random.Range(0, mComboTiles[inIdx]._Tiles.Length);
		ComboTileInfo comboTileInfo = mComboTiles[inIdx]._Tiles[num2];
		if (comboTileInfo._DistributionWeight > num)
		{
			return comboTileInfo;
		}
		return null;
	}

	protected virtual TilePiece GetComboTileByID(int inTileID)
	{
		TilePiece tilePiece = null;
		for (int i = 0; i < mComboTiles.Length; i++)
		{
			for (int j = 0; j < mComboTiles[i]._Tiles.Length; j++)
			{
				TilePiece tile = mComboTiles[i]._Tiles[j]._Tile;
				if (tile != null && tile.pID == inTileID)
				{
					tilePiece = tile;
					break;
				}
			}
			if (tilePiece != null)
			{
				break;
			}
		}
		return tilePiece;
	}

	protected virtual void DropTiles(int inRow, int inCol)
	{
		for (int i = inRow + 1; i < _RowCount; i++)
		{
			if (mTilesMap[i, inCol] != null && mTilesMap[inRow, inCol] == null)
			{
				TilePiece tilePiece = mTilesMap[i, inCol];
				Vector3 position = GetPosition(inRow, inCol);
				tilePiece.MoveTo(position);
				mTilesMap[i, inCol] = null;
				mTilesMap[inRow, inCol] = tilePiece;
				tilePiece.SetRowCol(inRow, inCol);
				mMovingTiles.Add(tilePiece);
				inRow = i;
			}
		}
	}

	protected int GetFreeSlot(int inRow, int inCol)
	{
		int result = -1;
		for (int i = 0; i < _RowCount; i++)
		{
			if (mTilesMap[i, inCol] == null)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	protected virtual TilePiece CreateNewTileAt(int inRow, int inCol, float yOff)
	{
		if (mTilesMap[inRow, inCol] != null)
		{
			Debug.LogWarning(" @@ Trying to create where object already exist !!!!r: " + inRow + " c: " + inCol + " ID: " + mTilesMap[inRow, inCol].pID);
			Debug.Break();
		}
		int randomTileIdx = GetRandomTileIdx();
		TilePiece tilePiece = InstantiateTile(randomTileIdx);
		tilePiece.SetGridParams(inRow, inCol, TileType.NORMAL, randomTileIdx);
		tilePiece.name = "r: " + inRow + ", c: " + inCol;
		mTilesMap[inRow, inCol] = tilePiece;
		Vector3 position = GetPosition(inRow, inCol);
		Vector3 zero = Vector3.zero;
		zero = position;
		zero.y = mGridBound.max.y + yOff;
		tilePiece.transform.position = zero;
		tilePiece.RegisterBehaviourCallback(OnTileBehaviourCompleted);
		tilePiece.MoveTo(position);
		return tilePiece;
	}

	protected virtual TilePiece CreateComboTileAt(int inRow, int inCol, float yOff, TilePiece inTile, bool inMove = false)
	{
		TilePiece tilePiece = InstantiateComboTile(inTile);
		tilePiece.SetRowCol(inRow, inCol);
		tilePiece.name = "r: " + inRow + ", c: " + inCol;
		mTilesMap[inRow, inCol] = tilePiece;
		Vector3 position = GetPosition(inRow, inCol);
		if (inMove)
		{
			Vector3 zero = Vector3.zero;
			zero = position;
			zero.y = mGridBound.max.y + yOff;
			tilePiece.transform.position = zero;
			tilePiece.MoveTo(position);
		}
		else
		{
			tilePiece.transform.position = position;
		}
		tilePiece.RegisterBehaviourCallback(OnTileBehaviourCompleted);
		return tilePiece;
	}

	public virtual void OnTileBehaviourCompleted(int inAction, TilePiece inTarget, params object[] inArray)
	{
		if (inAction >= 3)
		{
			return;
		}
		switch ((GridAction)inAction)
		{
		case GridAction.DROP_NEW_TILE:
		{
			int num = (int)inArray[0];
			int num2 = (int)inArray[1];
			TilePiece tilePiece2 = inArray[2] as TilePiece;
			if (mTilesMap[num, num2] != null && inTarget != null)
			{
				mTilesMap[num, num2].SetState(TilePiece.State.DEAD);
			}
			mTilesMap[num, num2] = null;
			if (tilePiece2 != null)
			{
				tilePiece2.SetState(TilePiece.State.DEAD);
			}
			TilePiece tilePiece3 = CreateNewTileAt(num, num2, _SpawnPosY);
			mTilesMap[num, num2] = tilePiece3;
			mMovingTiles.Add(tilePiece3);
			SetGridState(GridState.FALLING);
			break;
		}
		case GridAction.UNMASK:
		{
			TilePiece tilePiece = inArray[0] as TilePiece;
			if (tilePiece != null)
			{
				tilePiece.gameObject.SetActive(value: true);
				tilePiece.transform.localPosition = GetPosition(inTarget.pRow, inTarget.pColumn);
				mTilesMap[inTarget.pRow, inTarget.pColumn] = tilePiece;
				tilePiece.SetRowCol(inTarget.pRow, inTarget.pColumn);
				tilePiece.SetState(TilePiece.State.IDLE);
				if (IsFormingMatch(tilePiece))
				{
					mMovingTiles.Add(tilePiece);
					SetGridState(GridState.FALLING);
				}
			}
			break;
		}
		}
	}

	public virtual Vector3 GetPosition(int inRow, int inCol)
	{
		Vector3 zero = Vector3.zero;
		zero.x = mGridBound.min.x + mGridBlockSize.x * 0.5f + (float)inCol * mGridBlockSize.x;
		zero.y = mGridBound.min.y + (float)inRow * mGridBlockSize.y + mGridBlockSize.y * 0.5f;
		return zero;
	}

	protected virtual void SwapnTransition()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		if (!tileAtIndex.IsInLimbo() && !tileAtIndex2.IsInLimbo())
		{
			tileAtIndex.MoveTo(tileAtIndex2.transform.localPosition);
			tileAtIndex2.MoveTo(tileAtIndex.transform.localPosition);
		}
	}

	protected virtual void DropAdjacentTiles()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		int pRow = tileAtIndex.pRow;
		int pRow2 = tileAtIndex2.pRow;
		int pColumn = tileAtIndex.pColumn;
		int pColumn2 = tileAtIndex2.pColumn;
		if (pRow != 0 && pRow < _RowCount)
		{
			mTilesMap[pRow - 1, pColumn].SetState(TilePiece.State.MOVE_FALL);
		}
		if (pRow2 != 0 && pRow2 < _RowCount)
		{
			mTilesMap[pRow2 - 1, pColumn2].SetState(TilePiece.State.MOVE_FALL);
		}
	}

	public virtual void SetGridState(GridState inState)
	{
		GridState inPrvState = mGridState;
		mGridState = inState;
		mOnGridStateChangeEvent(inState, inPrvState);
	}

	public virtual void SetGameState(GameState inState)
	{
		GameState inPrvState = mGameState;
		mGameState = inState;
		if (mOnGameStateChangeEvent != null)
		{
			mOnGameStateChangeEvent(inState, inPrvState);
		}
	}

	protected virtual void OnGridStateChange(GridState inNewState, GridState inPrvState)
	{
		switch (inNewState)
		{
		case GridState.INTERACTIVE:
			ResetClick();
			break;
		case GridState.SWAPPING:
			SwapnTransition();
			break;
		case GridState.FALLING:
		case GridState.FILLING:
			break;
		}
	}

	public static void RegisterGridStateCallback(OnGridStateChangeDelegate inCallback)
	{
		if (!(mInstance == null))
		{
			if (mInstance.mOnGridStateChangeEvent == null)
			{
				mInstance.mOnGridStateChangeEvent = inCallback.Invoke;
				return;
			}
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnGridStateChangeEvent = (OnGridStateChangeDelegate)Delegate.Combine(tileMatchPuzzleGame.mOnGridStateChangeEvent, inCallback);
		}
	}

	public static void UnRegisterGridStateCallback(OnGridStateChangeDelegate inCallback)
	{
		if (!(mInstance == null) && mInstance.mOnGridStateChangeEvent != null)
		{
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnGridStateChangeEvent = (OnGridStateChangeDelegate)Delegate.Remove(tileMatchPuzzleGame.mOnGridStateChangeEvent, inCallback);
		}
	}

	public static void RegisterGameStateCallback(OnGameStateChangeDelegate inCallback)
	{
		if (!(mInstance == null))
		{
			if (mInstance.mOnGameStateChangeEvent == null)
			{
				mInstance.mOnGameStateChangeEvent = inCallback.Invoke;
				return;
			}
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnGameStateChangeEvent = (OnGameStateChangeDelegate)Delegate.Combine(tileMatchPuzzleGame.mOnGameStateChangeEvent, inCallback);
		}
	}

	public static void UnRegisterGameStateCallback(OnGameStateChangeDelegate inCallback)
	{
		if (!(mInstance == null) && mInstance.mOnGameStateChangeEvent != null)
		{
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnGameStateChangeEvent = (OnGameStateChangeDelegate)Delegate.Remove(tileMatchPuzzleGame.mOnGameStateChangeEvent, inCallback);
		}
	}

	public static void RegisterScoreChangeCallback(OnScoreUpdateDelegate inCallback)
	{
		if (!(mInstance == null))
		{
			if (mInstance.mOnScoreUpdateEvent == null)
			{
				mInstance.mOnScoreUpdateEvent = inCallback.Invoke;
				return;
			}
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnScoreUpdateEvent = (OnScoreUpdateDelegate)Delegate.Combine(tileMatchPuzzleGame.mOnScoreUpdateEvent, inCallback);
		}
	}

	public static void UnRegisterScoreChangeCallback(OnScoreUpdateDelegate inCallback)
	{
		if (!(mInstance == null) && mInstance.mOnScoreUpdateEvent != null)
		{
			TileMatchPuzzleGame tileMatchPuzzleGame = mInstance;
			tileMatchPuzzleGame.mOnScoreUpdateEvent = (OnScoreUpdateDelegate)Delegate.Remove(tileMatchPuzzleGame.mOnScoreUpdateEvent, inCallback);
		}
	}

	protected virtual void ClearSwapMatch()
	{
		TilePiece tileAtIndex = mSwapInfo.GetTileAtIndex(0);
		TilePiece tileAtIndex2 = mSwapInfo.GetTileAtIndex(1);
		List<TilePiece> list = new List<TilePiece>();
		if (tileAtIndex != null)
		{
			if (tileAtIndex.pType == TileType.EXPLODE)
			{
				ResetClick();
				ExecuteTileBehaviour(tileAtIndex);
				tileAtIndex = null;
				FillVoid(null);
				return;
			}
			GatherMatchingTiles(tileAtIndex.pRow, tileAtIndex.pColumn, tileAtIndex.pID, list);
		}
		if (tileAtIndex2 != null)
		{
			GatherMatchingTiles(tileAtIndex2.pRow, tileAtIndex2.pColumn, tileAtIndex2.pID, list);
		}
		ReplaceMatchedTiles(list);
		list.Clear();
		list = null;
	}

	public int GetHorizontalMatchSize(int inRow, int inColumn)
	{
		return GetMatchValue(inRow, 0, inColumn, 1, mTilesMap[inRow, inColumn].pID);
	}

	public int GetHorizontalMatchSize(int inRow, int inColumn, int inTileID, bool inCacheMatching = false)
	{
		return GetMatchValue(inRow, 0, inColumn, 1, inTileID, inCacheMatching);
	}

	public int GetVerticalMatchSize(int inRow, int inColumn)
	{
		return GetMatchValue(inRow, 1, inColumn, 0, mTilesMap[inRow, inColumn].pID);
	}

	public int GetVerticalMatchSize(int inRow, int inColumn, int inTileID, bool inCacheMatching = false)
	{
		return GetMatchValue(inRow, 1, inColumn, 0, inTileID, inCacheMatching);
	}

	protected int GetMatchValue(int inRow, int inRowDelta, int inColumn, int inColumnDelta, int inTileID, bool inCacheMatching = false)
	{
		int[] endItem = GetEndItem(inRow, -1 * inRowDelta, inColumn, -1 * inColumnDelta, inTileID);
		int[] endItem2 = GetEndItem(inRow, inRowDelta, inColumn, inColumnDelta, inTileID);
		int num = 0;
		int num2 = endItem2[0] - endItem[0];
		int num3 = endItem2[1] - endItem[1];
		if (num2 != 0)
		{
			num2++;
		}
		if (num3 != 0)
		{
			num3++;
		}
		if (inCacheMatching)
		{
			List<TilePiece> list = new List<TilePiece>();
			for (int i = 0; i < num2; i++)
			{
				list.Add(mTilesMap[endItem[0] + i, endItem[1]]);
			}
			for (int j = 0; j < num3; j++)
			{
				list.Add(mTilesMap[endItem[0], endItem[1] + j]);
			}
			mSwapInfo.SetMatchingTile(list.ToArray());
		}
		return num + (num2 + num3);
	}

	protected int[] GetEndItem(int inRow, int inRowDelta, int inColumn, int inColumnDelta, int inID, bool inEndReached = false)
	{
		int[] array = new int[2];
		int num = inRow + inRowDelta;
		int num2 = inColumn + inColumnDelta;
		if (num >= _RowCount || num < 0)
		{
			num = Mathf.Clamp(num, 0, _RowCount - 1);
			inEndReached = true;
		}
		if (num2 >= _ColumnCount || num2 < 0)
		{
			num2 = Mathf.Clamp(num2, 0, _ColumnCount - 1);
			inEndReached = true;
		}
		if (!inEndReached && mTilesMap[num, num2] != null && mTilesMap[num, num2].pID == inID && mTilesMap[num, num2].pType == TileType.NORMAL)
		{
			return GetEndItem(inRow + inRowDelta, inRowDelta, inColumn + inColumnDelta, inColumnDelta, inID, inEndReached);
		}
		array[0] = inRow;
		array[1] = inColumn;
		return array;
	}

	private int GetMatchCount(int inRow, int inRowDelta, int inColumn, int inColumnDelta, int inID)
	{
		int num = 0;
		if (mTilesMap == null || mTilesMap[inRow, inColumn] == null)
		{
			return num;
		}
		int num2 = inRow + inRowDelta;
		int num3 = inColumn + inColumnDelta;
		bool flag = false;
		while (!flag)
		{
			if (num2 >= _RowCount || num2 < 0)
			{
				num2 = Mathf.Clamp(num2, 0, _RowCount - 1);
				flag = true;
			}
			if (num3 >= _ColumnCount || num3 < 0)
			{
				num3 = Mathf.Clamp(num3, 0, _ColumnCount - 1);
				flag = true;
			}
			if (mTilesMap[num2, num3] != null && mTilesMap[num2, num3].pID == inID)
			{
				num++;
				num2 += inRowDelta;
				num3 += inColumnDelta;
			}
			else
			{
				flag = true;
			}
		}
		return num;
	}

	public TilePiece GetTileAt(int inRow, int inCol)
	{
		return mTilesMap[inRow, inCol];
	}

	public void SetTileAt(int inRow, int inCol, TilePiece inTile)
	{
		mTilesMap[inRow, inCol] = inTile;
	}

	public TilePiece GetRandomTileOfType(TileType inType = TileType.NORMAL)
	{
		int num = 1000;
		int inRow = mRandGen.Next(_RowCount);
		int inCol = mRandGen.Next(_ColumnCount);
		TilePiece tileAt = GetTileAt(inRow, inCol);
		while (tileAt == null || (tileAt.pType & inType) == 0)
		{
			num--;
			inRow = mRandGen.Next(_RowCount);
			inCol = mRandGen.Next(_ColumnCount);
			tileAt = GetTileAt(inRow, inCol);
			if (num < 0)
			{
				Debug.LogError("@@@@@@@@@@ Not found valid normal tile !!!!");
				Debug.Break();
				break;
			}
		}
		return tileAt;
	}

	public int GetRow(float inY)
	{
		return Mathf.Abs(Mathf.FloorToInt((inY - mGridBound.min.y) / mGridBlockSize.y));
	}

	public int GetCol(float inX)
	{
		return Mathf.Abs(Mathf.FloorToInt((inX - mGridBound.min.x) / mGridBlockSize.x));
	}

	public virtual bool IsLegalSwap(int inStartRow, int inStartCol, int inEndRow, int inEndCol)
	{
		if (_TileSwapDistance == int.MaxValue || _TileSwapDistance == 0)
		{
			return true;
		}
		int num = Mathf.Abs(inStartCol - inEndCol);
		int num2 = Mathf.Abs(inStartRow - inEndRow);
		if (num != _TileSwapDistance || num2 != 0)
		{
			if (num == 0)
			{
				return num2 == _TileSwapDistance;
			}
			return false;
		}
		return true;
	}

	public virtual bool IsTileVisible(TilePiece inTile)
	{
		return mGridBound.Contains(inTile.transform.localPosition);
	}

	protected void DrawDebug()
	{
		if (_DrawGridLine)
		{
			if (mDebugObject == null)
			{
				mDebugObject = new GameObject("DebugParent");
			}
			float num = mGridBound.min.x + mGridBlockSize.x;
			float num2 = mGridBound.max.y - mGridBlockSize.y;
			for (int i = 0; i < _ColumnCount; i++)
			{
				DebugLine.DrawLine(new Vector3(num, mGridBound.min.y, 0f), new Vector3(num, mGridBound.max.y, 0f), Color.yellow, float.MaxValue);
				num += mGridBlockSize.x;
			}
			for (int j = 0; j < _RowCount; j++)
			{
				DebugLine.DrawLine(new Vector3(mGridBound.min.x, num2, 0f), new Vector3(mGridBound.max.x, num2, 0f), Color.magenta, float.MaxValue);
				num2 -= mGridBlockSize.y;
			}
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(mGridBound.center, mGridBound.size);
	}

	public void DumpTileMat()
	{
		string text = "";
		for (int num = mTilesMap.GetLength(0) - 1; num >= 0; num--)
		{
			text = "";
			for (int i = 0; i < mTilesMap.GetLength(1); i++)
			{
				text = ((!IsTileAt(num, i)) ? (text + "- ") : (text + mTilesMap[num, i].pID + " "));
			}
			float num2 = mGridBound.min.y * (float)num + mGridBlockSize.y;
			Debug.Log("@ row: " + num + " ele: " + text + " posY: " + num2);
		}
	}

	private void GenerateTestGrid()
	{
		int[,] array = new int[3, 3]
		{
			{ 0, 1, 0 },
			{ 1, 0, 1 },
			{ 2, 1, 1 }
		};
		_RowCount = array.GetLength(0);
		_ColumnCount = array.GetLength(1);
		GetGridInfo();
		mTilesMap = new TilePiece[array.GetLength(0), array.GetLength(1)];
		float num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
		float num2 = mGridBound.min.y;
		for (int i = 0; i < _RowCount; i++)
		{
			for (int j = 0; j < _ColumnCount; j++)
			{
				int num3 = array[i, j];
				Vector3 position = new Vector3(num, num2 + mGridBlockSize.y * 0.5f, 0f);
				TilePiece tilePiece = InstantiateTile(num3);
				mTilesMap[i, j] = null;
				if (!(tilePiece == null))
				{
					mTilesMap[i, j] = tilePiece;
					mTilesMap[i, j].SetGridParams(i, j, TileType.NORMAL, num3);
					tilePiece.transform.position = position;
					num += mGridBlockSize.x;
				}
			}
			num = mGridBound.min.x + mGridBlockSize.x * 0.5f;
			num2 += mGridBlockSize.y;
		}
		DumpTileMat();
	}

	public virtual void OnDestroy()
	{
		mOnScoreUpdateEvent = null;
		mOnGameStateChangeEvent = null;
		mOnGridStateChangeEvent = null;
	}
}
