using System;
using System.Globalization;
using System.IO;
using System.Reflection;

public static class MissingFunctionExt
{
	public static MethodInfo GetMethodInfo(this Type inType, string name, BindingFlags bindingAttr)
	{
		return inType.GetMethod(name, bindingAttr);
	}

	public static FieldInfo[] GetFieldsInfo(this Type inType, BindingFlags bindingAttr)
	{
		return inType.GetFields(bindingAttr);
	}

	public static FieldInfo GetFieldInfo(this Type inType, string name, BindingFlags bindingAttr)
	{
		return inType.GetField(name, bindingAttr);
	}

	public static FieldInfo GetField(this Type inType, string name)
	{
		return inType.GetField(name);
	}

	public static string ToTitleCased(this TextInfo info, string value)
	{
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value);
	}

	public static Stream ToStream(this string inFileName)
	{
		return File.OpenRead(inFileName);
	}

	public static bool IsPrimitive(this Type inType)
	{
		return inType.IsPrimitive;
	}

	public static bool IsValueType(this Type inType)
	{
		return inType.IsValueType;
	}

	public static ConstructorInfo GetConstructor(this Type inType, Type[] inTypes)
	{
		return inType.GetConstructor(inTypes);
	}

	public static string[] GetStringInBetween(this string strSource, string strBegin, string strEnd, bool includeBegin, bool includeEnd)
	{
		string[] array = new string[2] { "", "" };
		int num = strSource.IndexOf(strBegin);
		if (num != -1)
		{
			if (includeBegin)
			{
				num -= strBegin.Length;
			}
			strSource = strSource.Substring(num + strBegin.Length);
			int num2 = strSource.IndexOf(strEnd);
			if (num2 != -1)
			{
				if (includeEnd)
				{
					num2 += strEnd.Length;
				}
				array[0] = strSource.Substring(0, num2);
				if (num2 + strEnd.Length < strSource.Length)
				{
					array[1] = strSource.Substring(num2 + strEnd.Length);
				}
			}
		}
		else
		{
			array[1] = strSource;
		}
		return array;
	}
}
