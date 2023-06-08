using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class MinMax
{
	[XmlElement(ElementName = "Min")]
	public float Min;

	[XmlElement(ElementName = "Max")]
	public float Max;

	public MinMax()
	{
	}

	public MinMax(float inMin, float inMax)
	{
		Max = inMax;
		Min = inMin;
	}

	public float GetRandomValue()
	{
		return UnityEngine.Random.Range(Min * 1000f, Max * 1000f) / 1000f;
	}

	public float GetRandomFloat()
	{
		return GetRandomValue();
	}

	public int GetRandomInt()
	{
		return (int)Math.Round(GetRandomValue(), MidpointRounding.AwayFromZero);
	}

	public bool IsInRange(float inValue)
	{
		if (inValue < Min || inValue > Max)
		{
			return false;
		}
		return true;
	}

	public override string ToString()
	{
		return "Min : " + Min + ", Max : " + Max;
	}
}
