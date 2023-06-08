using System;

[Serializable]
public class GmGameManagerTimeBonus
{
	public float _Min;

	public float _Max;

	public int _Points;

	public bool IsInRange(float data)
	{
		if (_Min <= data)
		{
			return _Max > data;
		}
		return false;
	}
}
