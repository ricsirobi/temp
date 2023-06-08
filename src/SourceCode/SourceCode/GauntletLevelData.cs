using System;
using UnityEngine;

[Serializable]
public class GauntletLevelData
{
	public GameObject _StartPiece;

	public GameObject _EndPiece;

	public GameObject[] _RotatingChamberPieces;

	public GameObject[] _BuildingPieces;

	public int _TrackPieceOffset;

	public GauntletMinMax _PreLoadPieceCount;

	public GameObject[] _TrackPieces;

	public GSGameType _GameType;
}
