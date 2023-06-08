using UnityEngine;

namespace StatsMonitor;

internal sealed class SMAnchor
{
	internal Vector2 position;

	internal Vector2 min;

	internal Vector2 max;

	internal Vector2 pivot;

	internal SMAnchor(float x, float y, float minX, float minY, float maxX, float maxY, float pivotX, float pivotY)
	{
		position = new Vector2(x, y);
		min = new Vector2(minX, minY);
		max = new Vector2(maxX, maxY);
		pivot = new Vector2(pivotX, pivotY);
	}
}
