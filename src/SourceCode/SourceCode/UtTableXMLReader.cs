using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class UtTableXMLReader : UtTableXML, IEnumerable
{
	public List<KeyValuePair<string, UtTable>> mTableList;

	public UtTable this[string tableName]
	{
		get
		{
			if (mTableList != null)
			{
				foreach (KeyValuePair<string, UtTable> mTable in mTableList)
				{
					if (mTable.Key == tableName)
					{
						return mTable.Value;
					}
				}
			}
			return null;
		}
	}

	public new string ToString()
	{
		string text = "XML contains {";
		if (mTableList != null)
		{
			foreach (KeyValuePair<string, UtTable> mTable in mTableList)
			{
				text = text + "[" + mTable.Key + "],";
			}
		}
		return text + "}";
	}

	public UtTableXMLReader()
	{
		mTableList = new List<KeyValuePair<string, UtTable>>();
	}

	public Vector3 ParseVector3(string inValue)
	{
		string[] array = ParseVector(inValue);
		Vector3 result = new Vector3(0f, 0f, 0f);
		for (int i = 0; i < array.Length; i++)
		{
			switch (i)
			{
			case 0:
				result.x = float.Parse(array[i], NumberStyles.Number);
				break;
			case 1:
				result.y = float.Parse(array[i], NumberStyles.Number);
				break;
			case 2:
				result.z = float.Parse(array[i], NumberStyles.Number);
				break;
			}
		}
		return result;
	}

	public IEnumerator GetEnumerator()
	{
		if (mTableList != null)
		{
			return mTableList.GetEnumerator();
		}
		return null;
	}

	public override int OnTableBegin(string id, List<Field> schema)
	{
		UtTable utTable = new UtTable();
		foreach (Field item in schema)
		{
			int num = -1;
			switch (item.type)
			{
			case UtType.SLONG:
				num = utTable.AddField<int>();
				break;
			case UtType.SLONG_ARRAY:
				num = utTable.AddArrayField<int>();
				break;
			case UtType.ULONG:
				num = utTable.AddField<uint>();
				break;
			case UtType.ULONG_ARRAY:
				num = utTable.AddArrayField<uint>();
				break;
			case UtType.FLOAT:
				num = utTable.AddField<float>();
				break;
			case UtType.FLOAT_ARRAY:
				num = utTable.AddArrayField<float>();
				break;
			case UtType.STRING:
				num = utTable.AddField<string>();
				break;
			case UtType.STRING_ARRAY:
				num = utTable.AddArrayField<string>();
				break;
			case UtType.VECTOR3D:
				num = utTable.AddField<Vector3>();
				break;
			case UtType.VECTOR3D_ARRAY:
				num = utTable.AddArrayField<Vector3>();
				break;
			case UtType.IDSTRING:
			case UtType.KEYSTRING:
				num = utTable.AddField<string>();
				break;
			case UtType.IDSTRING_ARRAY:
			case UtType.KEYSTRING_ARRAY:
				num = utTable.AddArrayField<string>();
				break;
			}
			if (num != -1)
			{
				utTable.SetFieldUserData(num, item.name);
				continue;
			}
			Debug.Log("ERROR: Field " + item.name + " type =" + item.type.ToString() + " to table " + id + "!");
		}
		mTableList.Add(new KeyValuePair<string, UtTable>(id, utTable));
		return 0;
	}

	public override int OnPropertyValue(string key, string value)
	{
		return 0;
	}

	public override int OnRecordBegin()
	{
		int num = mTableList.Count - 1;
		if (num == -1)
		{
			return -1;
		}
		mTableList[num].Value.AddRecord();
		return 0;
	}

	public override int OnFieldValue(Field field, string value)
	{
		int num = mTableList.Count - 1;
		if (num == -1)
		{
			return -1;
		}
		UtTable value2 = mTableList[num].Value;
		int fieldIndex = value2.GetFieldIndex(field.name);
		int num2 = value2.GetRecordCount() - 1;
		if (fieldIndex == -1)
		{
			return -1;
		}
		if (num2 == -1)
		{
			return -1;
		}
		switch (field.type)
		{
		case UtType.SLONG:
			value2.SetValue(fieldIndex, num2, int.Parse(value, NumberStyles.Number));
			break;
		case UtType.SLONG_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), int.Parse(value, NumberStyles.Number));
			break;
		case UtType.ULONG:
			value2.SetValue(fieldIndex, num2, uint.Parse(value, NumberStyles.Number));
			break;
		case UtType.ULONG_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), uint.Parse(value, NumberStyles.Number));
			break;
		case UtType.FLOAT:
			value2.SetValue(fieldIndex, num2, float.Parse(value, NumberStyles.Number));
			break;
		case UtType.FLOAT_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), float.Parse(value, NumberStyles.Number));
			break;
		case UtType.STRING:
			value2.SetValue(fieldIndex, num2, value);
			break;
		case UtType.STRING_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), value);
			break;
		case UtType.VECTOR3D:
			value2.SetValue(fieldIndex, num2, ParseVector3(value));
			break;
		case UtType.VECTOR3D_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), ParseVector3(value));
			break;
		case UtType.IDSTRING:
		case UtType.KEYSTRING:
			value2.SetValue(fieldIndex, num2, value);
			break;
		case UtType.IDSTRING_ARRAY:
		case UtType.KEYSTRING_ARRAY:
			value2.SetArrayElementValue(fieldIndex, num2, value2.GetArraySize(fieldIndex, num2), value);
			break;
		}
		return 0;
	}

	public override int OnTableEnd()
	{
		return 0;
	}
}
