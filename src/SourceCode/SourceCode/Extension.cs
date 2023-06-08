using System;

public static class Extension
{
	public static T[] Add<T>(this T[] array, T obj)
	{
		if (array == null)
		{
			array = new T[0];
			array[0] = obj;
		}
		else
		{
			int num = array.Length;
			Array.Resize(ref array, num + 1);
			array[num] = obj;
		}
		return array;
	}
}
