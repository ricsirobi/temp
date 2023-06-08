using System;
using UnityEngine;

public class UtTypeConvert
{
	public static UtType GetType(Type inType)
	{
		if (inType.Equals(typeof(int)))
		{
			return UtType.SLONG;
		}
		if (inType.Equals(typeof(uint)))
		{
			return UtType.ULONG;
		}
		if (inType.Equals(typeof(float)))
		{
			return UtType.FLOAT;
		}
		if (inType.Equals(typeof(string)))
		{
			return UtType.STRING;
		}
		if (inType.Equals(typeof(Vector3)))
		{
			return UtType.VECTOR3D;
		}
		if (inType.Equals(typeof(UtDict)))
		{
			return UtType.UTDICT;
		}
		if (inType.Equals(typeof(UtTable)))
		{
			return UtType.UTTABLE;
		}
		if (inType.Equals(typeof(UtData)))
		{
			return UtType.UTDATA;
		}
		if (inType.Equals(typeof(byte[])))
		{
			return UtType.VOIDPTR;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<int>)))
		{
			return UtType.SLONG_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<uint>)))
		{
			return UtType.ULONG_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<float>)))
		{
			return UtType.FLOAT_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<string>)))
		{
			return UtType.STRING_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<Vector3>)))
		{
			return UtType.VECTOR3D_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<UtDict>)))
		{
			return UtType.UTDICT_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<UtTable>)))
		{
			return UtType.UTTABLE_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<UtData>)))
		{
			return UtType.UTDATA_ARRAY;
		}
		if (inType.Equals(typeof(UtTable.TypedArray<byte[]>)))
		{
			return UtType.VOIDPTR_ARRAY;
		}
		return UtType.NONE;
	}

	public static Type GetType(UtType inType)
	{
		return inType switch
		{
			UtType.SLONG => typeof(int), 
			UtType.ULONG => typeof(uint), 
			UtType.FLOAT => typeof(float), 
			UtType.STRING => typeof(string), 
			UtType.VECTOR3D => typeof(Vector3), 
			UtType.UTDICT => typeof(UtDict), 
			UtType.UTTABLE => typeof(UtTable), 
			UtType.UTDATA => typeof(UtData), 
			UtType.VOIDPTR => typeof(byte[]), 
			UtType.SLONG_ARRAY => typeof(UtTable.TypedArray<int>), 
			UtType.ULONG_ARRAY => typeof(UtTable.TypedArray<uint>), 
			UtType.FLOAT_ARRAY => typeof(UtTable.TypedArray<float>), 
			UtType.STRING_ARRAY => typeof(UtTable.TypedArray<string>), 
			UtType.VECTOR3D_ARRAY => typeof(UtTable.TypedArray<Vector3>), 
			UtType.UTDICT_ARRAY => typeof(UtTable.TypedArray<UtDict>), 
			UtType.UTTABLE_ARRAY => typeof(UtTable.TypedArray<UtTable>), 
			UtType.UTDATA_ARRAY => typeof(UtTable.TypedArray<UtData>), 
			UtType.VOIDPTR_ARRAY => typeof(UtTable.TypedArray<byte[]>), 
			_ => null, 
		};
	}
}
