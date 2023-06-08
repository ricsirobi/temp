using System;
using UnityEngine;

public class LabStringUtil
{
	public static TYPE Parse<TYPE>(string inString, TYPE inDefault)
	{
		Type typeFromHandle = typeof(TYPE);
		if (typeFromHandle.Equals(typeof(int)) || typeFromHandle.Equals(typeof(float)) || typeFromHandle.Equals(typeof(bool)))
		{
			return UtStringUtil.Parse(inString, inDefault);
		}
		if (typeFromHandle.Equals(typeof(Color)))
		{
			Color color = (Color)(object)inDefault;
			string[] array = inString.Split(',');
			if (array.Length >= 1)
			{
				color.r = Parse(array[0], color.r);
			}
			if (array.Length >= 2)
			{
				color.g = Parse(array[1], color.g);
			}
			if (array.Length >= 3)
			{
				color.b = Parse(array[2], color.b);
			}
			if (array.Length >= 4)
			{
				color.a = Parse(array[3], color.a);
			}
			return (TYPE)(object)color;
		}
		return inDefault;
	}
}
