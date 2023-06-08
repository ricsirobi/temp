using UnityEngine;

public class PuzzlePattern
{
	public Color _Color;

	public Material _Shape;

	public PuzzlePattern(Color c, Material m)
	{
		_Color = c;
		_Shape = m;
	}

	public Color GetColor()
	{
		return _Color;
	}

	public Material GetShape()
	{
		return _Shape;
	}
}
