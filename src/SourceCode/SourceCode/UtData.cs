using System.Collections.Generic;
using UnityEngine;

public class UtData : UtContainer, UtIData
{
	public class Record
	{
		private UtData mData;

		private UtContainer mContainer;

		private int mRecordIdx = -1;

		public UtData pData => mData;

		public UtContainer pContainer => mContainer;

		public int pIndex => mRecordIdx;

		public object this[string inKey] => mData.GetValue(mContainer, inKey, mRecordIdx);

		public Record(UtData inData, UtContainer inContainer)
		{
			mData = inData;
			mContainer = inContainer;
		}

		public Record(UtData inData, UtContainer inContainer, int inRecordIdx)
		{
			mData = inData;
			mContainer = inContainer;
			mRecordIdx = inRecordIdx;
		}

		public Record(Record inRecord)
		{
			mData = inRecord.pData;
			mContainer = inRecord.pContainer;
			mRecordIdx = inRecord.pIndex;
		}

		public bool Exists()
		{
			if (mRecordIdx != -1)
			{
				return mRecordIdx < mData.GetRecordCount(mContainer);
			}
			return false;
		}

		public bool Next()
		{
			if (mRecordIdx < mData.GetRecordCount(mContainer))
			{
				mRecordIdx++;
			}
			return Exists();
		}

		public static Record operator ++(Record inRecord)
		{
			inRecord.Next();
			return inRecord;
		}

		public bool Previous()
		{
			if (mRecordIdx > -1)
			{
				mRecordIdx--;
			}
			return Exists();
		}

		public static Record operator --(Record inRecord)
		{
			inRecord.Previous();
			return inRecord;
		}

		public bool HasKey(string inKey)
		{
			return mData.HasKey(mContainer, inKey);
		}

		public object GetValue(string inKey)
		{
			return mData.GetValue(mContainer, inKey, mRecordIdx);
		}

		public TYPE GetValue<TYPE>(string inKey)
		{
			return mData.GetValue<TYPE>(mContainer, inKey, mRecordIdx);
		}

		public int GetArraySize(string inKey)
		{
			return mData.GetArrayElementCount(mContainer, inKey, mRecordIdx);
		}

		public TYPE GetArrayElementValue<TYPE>(string inKey, int inArrayIdx)
		{
			return mData.GetArrayElementValue<TYPE>(mContainer, inKey, mRecordIdx, inArrayIdx);
		}
	}

	public const string CONTAINER_DELIMITER = ":";

	public const string SCHEMA_KEY = "__schema";

	public const string STRINGS_KEY = "__strings";

	private static uint mSchema = UtKey.Get("__schema");

	private static uint mStrings = UtKey.Get("__strings");

	private bool mReadOnly;

	private bool mUseStringTable;

	private UtStatus mLastStatus;

	public UtStatus pLastStatus => mLastStatus;

	public UtData()
	{
	}

	public UtData(UtData inData)
	{
		Debug.Log("ERROR: Copy constructor not implemented!!");
	}

	public UtData(byte[] inResourceData, bool inReadOnly)
	{
		mLastStatus = ReadFromResource(inResourceData, inReadOnly);
	}

	~UtData()
	{
	}

	public new void Clear()
	{
		base.Clear();
		mReadOnly = false;
		mUseStringTable = false;
		mLastStatus = UtStatus.NO_ERROR;
		mResolved = false;
	}

	public UtContainer AddContainer(UtContainer inParentContainer, string inNewContainerName)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return null;
		}
		if (inParentContainer == null)
		{
			inParentContainer = this;
		}
		UtContainer utContainer = new UtContainer();
		if (InitContainer(inParentContainer, utContainer, inNewContainerName))
		{
			return utContainer;
		}
		return null;
	}

	public UtStatus DeleteContainer(UtContainer inParentContainer, string inContainerName)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return mLastStatus;
		}
		if (inParentContainer == null)
		{
			inParentContainer = this;
		}
		if (inParentContainer.Remove(UtKey.Get(inContainerName)))
		{
			mLastStatus = UtStatus.NO_ERROR;
		}
		else
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
		}
		return mLastStatus;
	}

	public UtContainer AppendContainer(UtContainer inParentContainer, UtData inData)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return null;
		}
		if (inParentContainer == null)
		{
			inParentContainer = this;
		}
		inParentContainer[UtKey.Get(inData.pName)] = inData;
		inData[UtContainer.mParent] = inParentContainer;
		return inData;
	}

	public UtData CopyContainer(UtContainer inContainer)
	{
		return null;
	}

	public UtContainer GetContainer(string inPathStr)
	{
		return GetContainer(null, inPathStr);
	}

	public UtContainer GetContainer(UtContainer inParentContainer, string inSubPathStr)
	{
		if (inParentContainer == null)
		{
			inParentContainer = this;
		}
		if (inSubPathStr == null)
		{
			return inParentContainer;
		}
		string[] array = inSubPathStr.Split(":"[0]);
		UtContainer result = null;
		string[] array2 = array;
		foreach (string inKeyName in array2)
		{
			object outValue = null;
			if (!inParentContainer.TryGetValue(UtKey.Get(inKeyName), out outValue))
			{
				mLastStatus = UtStatus.ERROR_NOT_FOUND;
				return null;
			}
			result = (UtContainer)outValue;
		}
		return result;
	}

	public string GetContainerName(UtContainer inContainer)
	{
		return inContainer.pName;
	}

	public string GetContainerPath(UtContainer inContainer)
	{
		return ConstructPathToRoot(inContainer);
	}

	public UtStatus SetValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return UtStatus.ERROR_INVALID_OPERATION;
		}
		UtContainer utContainer = ResolveContainer(inContainer);
		uint inKey2 = UtKey.Get(inKey);
		utContainer[inKey2] = inValue;
		mLastStatus = UtStatus.NO_ERROR;
		return mLastStatus;
	}

	public UtStatus SetValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue, int inRecordIdx)
	{
		if (inRecordIdx == -1)
		{
			return SetValue(inContainer, inKey, inValue);
		}
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return UtStatus.ERROR_INVALID_OPERATION;
		}
		UtContainer utContainer = ResolveContainer(inContainer);
		UtTable utTable = utContainer.pTable;
		if (utTable == null)
		{
			if (!InitContainer(null, utContainer, "Root"))
			{
				mLastStatus = UtStatus.ERROR_NOT_FOUND;
				return mLastStatus;
			}
			utTable = utContainer.pTable;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1)
		{
			uint inKey2 = UtKey.Get(inKey);
			num = AddFieldToTable<TYPE>(inContainer, inKey, inKey2);
			if (num == -1)
			{
				mLastStatus = UtStatus.ERROR_UNKNOWN;
				return mLastStatus;
			}
		}
		mLastStatus = utTable.SetValue(num, inRecordIdx, inValue);
		return mLastStatus;
	}

	public UtStatus SetArrayElementValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue, int inRecordIdx, int inArrayIdx)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return UtStatus.ERROR_INVALID_OPERATION;
		}
		UtContainer utContainer = ResolveContainer(inContainer);
		UtTable utTable = utContainer.pTable;
		if (utTable == null)
		{
			if (!InitContainer(null, utContainer, "Root"))
			{
				mLastStatus = UtStatus.ERROR_NOT_FOUND;
				return mLastStatus;
			}
			utTable = utContainer.pTable;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1)
		{
			uint inKey2 = UtKey.Get(inKey);
			num = AddFieldToTable<UtTable.TypedArray<TYPE>>(inContainer, inKey, inKey2);
			if (num == -1)
			{
				mLastStatus = UtStatus.ERROR_UNKNOWN;
				return mLastStatus;
			}
		}
		mLastStatus = utTable.SetArrayElementValue(num, inRecordIdx, inArrayIdx, inValue);
		return mLastStatus;
	}

	public TYPE GetValue<TYPE>(UtContainer inContainer, string inKey)
	{
		UtContainer utContainer = ResolveContainer(inContainer);
		uint inKey2 = UtKey.Get(inKey);
		if (!utContainer.TryGetValue(inKey2, out var outValue))
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return default(TYPE);
		}
		return (TYPE)outValue;
	}

	public object GetValue(UtContainer inContainer, string inKey, int inRecordIdx)
	{
		if (inRecordIdx == -1)
		{
			mLastStatus = UtStatus.ERROR_OUT_OF_RANGE;
			return null;
		}
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return null;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1 || inRecordIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return null;
		}
		return ((UtTable.IArrayUntyped)utTable.pFieldList[num]).GetElementAt(inRecordIdx);
	}

	public TYPE GetValue<TYPE>(UtContainer inContainer, string inKey, int inRecordIdx)
	{
		if (inRecordIdx == -1)
		{
			return GetValue<TYPE>(inContainer, inKey);
		}
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return default(TYPE);
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1 || inRecordIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return default(TYPE);
		}
		return utTable.GetValue<TYPE>(num, inRecordIdx);
	}

	public TYPE GetArrayElementValue<TYPE>(UtContainer inContainer, string inKey, int inRecordIdx, int inArrayIdx)
	{
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return default(TYPE);
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1 || inRecordIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return default(TYPE);
		}
		return utTable.GetArrayElementValue<TYPE>(num, inRecordIdx, inArrayIdx);
	}

	public int GetArrayElementCount(UtContainer inContainer, string inKey, int inRecordIdx)
	{
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return 0;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1 || inRecordIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return 0;
		}
		return utTable.GetArraySize(num, inRecordIdx);
	}

	public bool HasKey(UtContainer inContainer, string inKey)
	{
		mLastStatus = UtStatus.NO_ERROR;
		UtContainer utContainer = ResolveContainer(inContainer);
		uint inKey2 = UtKey.Get(inKey);
		if (utContainer.ContainsKey(inKey2))
		{
			return true;
		}
		UtTable utTable = utContainer.pTable;
		if (utTable == null)
		{
			return false;
		}
		if (ResolveKeyNameToField(utTable, inKey) == -1)
		{
			return false;
		}
		return true;
	}

	public UtType GetKeyType(UtContainer inContainer, string inKey)
	{
		UtContainer utContainer = ResolveContainer(inContainer);
		uint inKey2 = UtKey.Get(inKey);
		object outValue = null;
		if (utContainer.TryGetValue(inKey2, out outValue))
		{
			return UtTypeConvert.GetType(outValue.GetType());
		}
		UtTable utTable = utContainer.pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return UtType.NONE;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return UtType.NONE;
		}
		return utTable.GetFieldType(num);
	}

	public int GetRecordCount(UtContainer inContainer)
	{
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return 0;
		}
		return utTable.GetRecordCount();
	}

	public int AddRecord(UtContainer inContainer)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return -1;
		}
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return -1;
		}
		return utTable.AddRecord();
	}

	public int FindRecord<TYPE>(UtContainer inContainer, int inStartRecIdx, string inKey, TYPE inMatchValue)
	{
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return -1;
		}
		int num = ResolveKeyNameToField(utTable, inKey);
		if (num == -1 || inStartRecIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return -1;
		}
		int num2 = utTable.FindRecord(inStartRecIdx, num, inMatchValue);
		if (num2 == -1)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return -1;
		}
		mLastStatus = UtStatus.NO_ERROR;
		return num2;
	}

	public UtStatus CopyRecord(UtContainer inSrcContainer, int inSrcRecordIdx, UtData inDstData, UtContainer inDstContainer, int inDstRecordIdx)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return mLastStatus;
		}
		UtContainer utContainer = ResolveContainer(inSrcContainer);
		UtContainer utContainer2 = ((inDstContainer != null) ? inDstContainer : inDstData);
		UtTable utTable = utContainer.pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
			return mLastStatus;
		}
		UtTable utTable2 = utContainer2.pTable;
		if (utTable2 == null)
		{
			if (!InitContainer(null, utContainer2, "Root"))
			{
				mLastStatus = UtStatus.ERROR_UNKNOWN;
				return mLastStatus;
			}
			utTable2 = utContainer2.pTable;
		}
		if (inSrcRecordIdx >= utTable.GetRecordCount())
		{
			mLastStatus = UtStatus.ERROR_OUT_OF_RANGE;
			return mLastStatus;
		}
		object outValue = null;
		utContainer.TryGetValue(mStrings, out outValue);
		object outValue2 = null;
		utContainer2.TryGetValue(mStrings, out outValue2);
		if (outValue == null && outValue2 != null)
		{
			mLastStatus = UtStatus.ERROR_PARAMS;
			return mLastStatus;
		}
		UtDict utDict = (UtDict)utContainer2[mSchema];
		UtDict utDict2 = (UtDict)outValue2;
		utTable2.AppendFields(utTable);
		int fieldCount = utTable.GetFieldCount();
		for (int i = 0; i < fieldCount; i++)
		{
			UtTable.IArrayUntyped arrayUntyped = (UtTable.IArrayUntyped)utTable.pFieldList[i];
			UtTable.IArrayUntyped arrayUntyped2 = null;
			uint num = uint.MaxValue;
			object obj = null;
			int num2 = -1;
			if (outValue != null)
			{
				obj = utTable.pDataList[i];
				num = UtKey.Get((string)obj);
				num2 = ((utDict2 == null) ? utTable2.GetFieldIndex(num) : utTable2.GetFieldIndex(obj));
			}
			else
			{
				num = (uint)utTable.pDataList[i];
				num2 = utTable2.GetFieldIndex(num);
			}
			if (num2 == -1)
			{
				arrayUntyped2 = (UtTable.IArrayUntyped)arrayUntyped.CloneEmpty();
				while (arrayUntyped2.SizeOf() < utTable2.GetRecordCount())
				{
					arrayUntyped2.AddElement();
				}
				num2 = utTable2.pFieldList.Count;
				utTable2.pFieldList.Add(arrayUntyped2);
				utDict[num] = num2;
				if (utDict2 != null)
				{
					utDict2[num] = obj;
					utTable2.pDataList.Add(obj);
				}
				else
				{
					utTable2.pDataList.Add(num);
				}
			}
			else
			{
				arrayUntyped2 = (UtTable.IArrayUntyped)utTable2.pFieldList[num2];
			}
			while (inDstRecordIdx >= utTable2.GetRecordCount())
			{
				utTable2.AddRecord();
			}
			arrayUntyped2.CopyElement(arrayUntyped, inSrcRecordIdx, inDstRecordIdx);
		}
		mLastStatus = UtStatus.NO_ERROR;
		return mLastStatus;
	}

	public UtStatus RemoveRecord(UtContainer inContainer, int inRecordIdx)
	{
		if (mReadOnly)
		{
			mLastStatus = UtStatus.ERROR_INVALID_OPERATION;
			return mLastStatus;
		}
		UtTable utTable = ResolveContainer(inContainer).pTable;
		if (utTable == null)
		{
			mLastStatus = UtStatus.ERROR_NOT_FOUND;
		}
		else
		{
			mLastStatus = utTable.RemoveRecord(inRecordIdx);
		}
		return mLastStatus;
	}

	public UtStatus ReadFromResource(byte[] inResource, bool inReadOnly)
	{
		Clear();
		mLastStatus = UtDataIO.DataResAssign(inResource, this);
		mReadOnly = inReadOnly;
		return mLastStatus;
	}

	public UtStatus WriteToResource(out byte[] outResource)
	{
		UtEndian inEndian = UtEndian.Get(1);
		outResource = UtDataIO.DictResCreate(this, inEndian);
		if (outResource != null)
		{
			mLastStatus = UtStatus.NO_ERROR;
		}
		else
		{
			mLastStatus = UtStatus.ERROR_UNKNOWN;
		}
		return mLastStatus;
	}

	public UtStatus Resolve()
	{
		if (!base.pResolved)
		{
			UtTable utTable = base.pTable;
			if (utTable != null)
			{
				object outValue = null;
				object outValue2 = null;
				if (!TryGetValue(mSchema, out outValue))
				{
					Debug.Log("ERROR: UtData " + base.pName + " has no schema!!");
					mLastStatus = UtStatus.ERROR_NOT_FOUND;
					return mLastStatus;
				}
				if (TryGetValue(mStrings, out outValue2))
				{
					mUseStringTable = true;
				}
				foreach (Entry item in (UtDict)outValue)
				{
					uint inFieldIdx = (uint)item.Value;
					uint key = item.Key;
					if (mUseStringTable)
					{
						string inUserData = (string)((UtDict)outValue2)[key];
						utTable.SetFieldUserData((int)inFieldIdx, inUserData);
					}
					else
					{
						utTable.SetFieldUserData((int)inFieldIdx, key);
					}
				}
			}
			base.pResolved = true;
		}
		mLastStatus = UtStatus.NO_ERROR;
		return mLastStatus;
	}

	private bool InitContainer(UtContainer inParentContainer, UtContainer inContainer, string inName)
	{
		inContainer[UtContainer.mTable] = new UtTable();
		inContainer[mSchema] = new UtDict();
		inContainer[UtContainer.mName] = inName;
		if (inParentContainer != null)
		{
			inContainer[UtContainer.mParent] = inParentContainer;
		}
		return true;
	}

	private UtContainer ResolveContainer(UtContainer inContainer)
	{
		if (inContainer == null)
		{
			inContainer = this;
		}
		if (!inContainer.pResolved)
		{
			Debug.Log("ERROR: UtContainer " + inContainer.pName + " - NOT RESOLVED!");
		}
		return inContainer;
	}

	private int ResolveKeyNameToField(UtTable inTable, string inKeyName)
	{
		if (mUseStringTable)
		{
			return inTable.GetFieldIndex(inKeyName);
		}
		return inTable.GetFieldIndex(UtKey.Get(inKeyName));
	}

	private int AddFieldToTable<TYPE>(UtContainer inContainer, string inKeyName, uint inKey)
	{
		int num = inContainer.pTable.AddField<TYPE>();
		if (num != -1)
		{
			if (mUseStringTable)
			{
				inContainer.pTable.SetFieldUserData(num, inKeyName);
				((UtDict)inContainer[mStrings])[inKey] = inKeyName;
			}
			else
			{
				inContainer.pTable.SetFieldUserData(num, inKey);
			}
			((UtDict)inContainer[mSchema])[inKey] = num;
		}
		return num;
	}

	private string ConstructPathToRoot(UtContainer inContainer)
	{
		UtContainer utContainer = ResolveContainer(inContainer);
		List<string> list = new List<string>();
		while (utContainer.pParent != null)
		{
			list.Insert(0, utContainer.pParent.pName);
			utContainer = utContainer.pParent;
		}
		string text = "";
		for (int i = 0; i < list.Count; i++)
		{
			if (i != 0)
			{
				text += ":";
			}
			text += list[i];
		}
		return text;
	}
}
