using System;
using System.Collections.Generic;

namespace CI.WSANative.Tile;

public static class WSANativeTile
{
	public static void CreateSecondaryTile(string tileId, string displayName, Uri Square150x150Logo, bool ShowNameOnSquare150x150Logo, WSATileData additionalTilesSizes = null)
	{
	}

	public static void RemoveSecondaryTile(string tileId)
	{
	}

	public static IEnumerable<string> FindAllSecondaryTiles()
	{
		return new List<string>();
	}
}
