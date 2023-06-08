using System;
using System.Collections;
using System.Collections.Generic;

public class UtTable : ICloneable, IEnumerable
{
	public interface IArrayUntyped : ICloneable
	{
		int SizeOf();

		UtType ElementType();

		int AddElement();

		void CopyElement(IArrayUntyped inArray, int inSrcIdx, int inDestIdx);

		object GetElementAt(int inIdx);

		void RemoveElementAt(int inIdx);

		object CloneEmpty();

		void Clear();
	}

	public class TypedArray<TYPE> : List<TYPE>, IArrayUntyped, ICloneable
	{
		private TYPE mDefault;

		public TYPE pDefault
		{
			get
			{
				return mDefault;
			}
			set
			{
				mDefault = value;
			}
		}

		public TypedArray()
		{
		}

		public TypedArray(TYPE inDefault)
		{
			mDefault = inDefault;
		}

		public TYPE CloneElement(TYPE inElement)
		{
			if (inElement == null)
			{
				return inElement;
			}
			if (inElement.GetType().IsValueType())
			{
				return inElement;
			}
			return (TYPE)((ICloneable)(object)inElement).Clone();
		}

		public object Clone()
		{
			TypedArray<TYPE> typedArray = new TypedArray<TYPE>(pDefault);
			using Enumerator enumerator = GetEnumerator();
			while (enumerator.MoveNext())
			{
				TYPE current = enumerator.Current;
				typedArray.Add(CloneElement(current));
			}
			return typedArray;
		}

		public override string ToString()
		{
			string text = "[";
			for (int i = 0; i < SizeOf(); i++)
			{
				if (i > 0)
				{
					text += ",";
				}
				object elementAt = GetElementAt(i);
				text = ((elementAt == null) ? (text + "null") : (text + elementAt.ToString()));
			}
			return text + "]";
		}

		public int SizeOf()
		{
			return base.Count;
		}

		public UtType ElementType()
		{
			return UtTypeConvert.GetType(typeof(TYPE));
		}

		public int AddElement()
		{
			Add(CloneElement(pDefault));
			return base.Count - 1;
		}

		public void CopyElement(IArrayUntyped inArray, int inSrcIdx, int inDestIdx)
		{
			if (inArray.GetType().Equals(GetType()) && inSrcIdx < inArray.SizeOf() && inDestIdx < SizeOf())
			{
				TypedArray<TYPE> typedArray = (TypedArray<TYPE>)inArray;
				base[inDestIdx] = CloneElement(typedArray[inSrcIdx]);
			}
		}

		public object GetElementAt(int inIdx)
		{
			if (inIdx >= base.Count)
			{
				return null;
			}
			return base[inIdx];
		}

		public void RemoveElementAt(int inIdx)
		{
			RemoveAt(inIdx);
		}

		public object CloneEmpty()
		{
			return new TypedArray<TYPE>(pDefault);
		}
	}

	public class Record
	{
		private UtTable mTable;

		private int mRecordIdx = -1;

		public UtTable pTable => mTable;

		public int pIndex => mRecordIdx;

		public object this[string inField] => this[mTable.GetFieldIndex(inField)];

		public object this[int inFieldIdx]
		{
			get
			{
				if (inFieldIdx != -1)
				{
					IArrayUntyped arrayUntyped = (IArrayUntyped)mTable.pFieldList[inFieldIdx];
					if (mRecordIdx < arrayUntyped.SizeOf())
					{
						return arrayUntyped.GetElementAt(mRecordIdx);
					}
				}
				return null;
			}
		}

		public Record(UtTable inTable)
		{
			mTable = inTable;
		}

		public Record(UtTable inTable, int inRecordIdx)
		{
			mTable = inTable;
			mRecordIdx = inRecordIdx;
		}

		public Record(Record inRecord)
		{
			mTable = inRecord.pTable;
			mRecordIdx = inRecord.pIndex;
		}

		public override string ToString()
		{
			string text = "rec:" + mRecordIdx + "[";
			for (int i = 0; i < GetFieldCount(); i++)
			{
				if (i != 0)
				{
					text += ",";
				}
				object obj = this[i];
				text = ((obj == null) ? (text + "null") : (text + obj.ToString()));
			}
			return text + "]";
		}

		public bool Exists()
		{
			if (mRecordIdx != -1)
			{
				return mRecordIdx < mTable.GetRecordCount();
			}
			return false;
		}

		public bool Next()
		{
			if (mRecordIdx < mTable.GetRecordCount())
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

		public UtStatus Remove()
		{
			return mTable.RemoveRecord(mRecordIdx);
		}

		public UtStatus Copy(Record inRecord)
		{
			return mTable.CopyRecord(inRecord, mRecordIdx);
		}

		public int GetFieldCount()
		{
			return mTable.GetFieldCount();
		}

		public UtType GetFieldType(int inFieldIdx)
		{
			return mTable.GetFieldType(inFieldIdx);
		}

		public TYPE GetValue<TYPE>(int inFieldIdx)
		{
			return mTable.GetValue<TYPE>(inFieldIdx, mRecordIdx);
		}

		public UtStatus SetValue<TYPE>(int inFieldIdx, TYPE inValue)
		{
			return mTable.SetValue(inFieldIdx, mRecordIdx, inValue);
		}

		public int GetArraySize(int inFieldIdx)
		{
			return mTable.GetArraySize(inFieldIdx, mRecordIdx);
		}

		public TYPE[] GetArray<TYPE>(int inFieldIdx)
		{
			return mTable.GetArray<TYPE>(inFieldIdx, mRecordIdx);
		}

		public TYPE GetArrayElementValue<TYPE>(int inFieldIdx, int inArrayIdx)
		{
			return mTable.GetArrayElementValue<TYPE>(inFieldIdx, mRecordIdx, inArrayIdx);
		}

		public UtStatus SetArrayElementValue<TYPE>(int inFieldIdx, int inArrayIdx, TYPE inValue)
		{
			return mTable.SetArrayElementValue(inFieldIdx, mRecordIdx, inArrayIdx, inValue);
		}
	}

	public class Enumerator : IEnumerator
	{
		private Record mRecord;

		public object Current => mRecord;

		public Enumerator(UtTable inTable)
		{
			mRecord = new Record(inTable);
		}

		public bool MoveNext()
		{
			return mRecord.Next();
		}

		public void Reset()
		{
			mRecord = new Record(mRecord.pTable);
		}
	}

	protected ArrayList mDataList = new ArrayList();

	protected ArrayList mFieldList = new ArrayList();

	protected int mRecordCount;

	public ArrayList pDataList => mDataList;

	public ArrayList pFieldList => mFieldList;

	public int pRecordCount
	{
		get
		{
			return mRecordCount;
		}
		set
		{
			mRecordCount = value;
		}
	}

	public Record this[int inRecordIdx]
	{
		get
		{
			return new Record(this, inRecordIdx);
		}
		set
		{
			CopyRecord(value, inRecordIdx);
		}
	}

	public UtTable()
	{
	}

	public UtTable(UtTable inTable)
	{
		mDataList = (ArrayList)inTable.pDataList.Clone();
		foreach (IArrayUntyped pField in inTable.pFieldList)
		{
			mFieldList.Add(pField.Clone());
		}
		mRecordCount = inTable.pRecordCount;
	}

	public override string ToString()
	{
		string text = "Schema:[";
		for (int i = 0; i < GetFieldCount(); i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			object fieldUserData = GetFieldUserData(i);
			text = ((fieldUserData == null) ? (text + "null") : (text + fieldUserData.ToString()));
			text = text + ":" + GetFieldType(i);
		}
		return text + "]";
	}

	public IEnumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public object Clone()
	{
		return new UtTable(this);
	}

	public UtTable Assign(UtTable inTable)
	{
		return null;
	}

	public UtStatus Clear()
	{
		mDataList.Clear();
		mFieldList.Clear();
		return UtStatus.NO_ERROR;
	}

	public int GetFieldCount()
	{
		return mFieldList.Count;
	}

	public UtType GetFieldType(int inFieldIdx)
	{
		return ((IArrayUntyped)mFieldList[inFieldIdx]).ElementType();
	}

	public bool GetFieldTypeMatch(UtTable inTable, int inFieldIdx)
	{
		return mFieldList[inFieldIdx].GetType().Equals(inTable.mFieldList[inFieldIdx].GetType());
	}

	public int GetFieldIndex(object inFieldUserData)
	{
		return mDataList.IndexOf(inFieldUserData);
	}

	public object GetFieldUserData(int inFieldIdx)
	{
		return mDataList[inFieldIdx];
	}

	public UtStatus SetFieldUserData(int inFieldIdx, object inUserData)
	{
		mDataList[inFieldIdx] = inUserData;
		return UtStatus.NO_ERROR;
	}

	public UtStatus SetFieldDefaultValue<TYPE>(int inFieldIdx, TYPE inDefault)
	{
		((TypedArray<TYPE>)mFieldList[inFieldIdx]).pDefault = inDefault;
		return UtStatus.NO_ERROR;
	}

	public void AppendFields(UtTable inTable)
	{
		int fieldCount = inTable.GetFieldCount();
		for (int i = 0; i < fieldCount; i++)
		{
			object obj = inTable.pDataList[i];
			if (GetFieldIndex(obj) == -1)
			{
				IArrayUntyped arrayUntyped = (IArrayUntyped)((IArrayUntyped)inTable.pFieldList[i]).CloneEmpty();
				while (arrayUntyped.SizeOf() < mRecordCount)
				{
					arrayUntyped.AddElement();
				}
				mFieldList.Add(arrayUntyped);
				mDataList.Add(obj);
			}
		}
	}

	public int AddField<TYPE>()
	{
		return AddField(default(TYPE));
	}

	public int AddField<TYPE>(TYPE inDefault)
	{
		TypedArray<TYPE> typedArray = new TypedArray<TYPE>(inDefault);
		while (typedArray.SizeOf() < mRecordCount)
		{
			typedArray.AddElement();
		}
		mFieldList.Add(typedArray);
		mDataList.Add(null);
		return mFieldList.Count - 1;
	}

	public int AddArrayField<TYPE>()
	{
		return AddField(new TypedArray<TYPE>());
	}

	public int GetRecordCount()
	{
		return mRecordCount;
	}

	public bool GetRecordTypeMatch(UtTable inTable)
	{
		if (inTable.GetFieldCount() != GetFieldCount())
		{
			return false;
		}
		for (int i = 0; i < GetFieldCount(); i++)
		{
			if (!GetFieldTypeMatch(inTable, i))
			{
				return false;
			}
		}
		return true;
	}

	public Record GetRecord(int inRecIdx)
	{
		return new Record(this, inRecIdx);
	}

	public int AddRecord()
	{
		foreach (IArrayUntyped mField in mFieldList)
		{
			mField.AddElement();
		}
		mRecordCount++;
		return mRecordCount - 1;
	}

	public int AddRecord(Record inRecord)
	{
		int num = mRecordCount;
		if (CopyRecord(inRecord, num) == UtStatus.NO_ERROR)
		{
			return num;
		}
		return -1;
	}

	public UtStatus CopyRecord(Record inRecord, int inRecordIdx)
	{
		if (!inRecord.Exists())
		{
			return UtStatus.ERROR_OUT_OF_RANGE;
		}
		while (inRecordIdx >= mRecordCount)
		{
			AddRecord();
		}
		ArrayList arrayList = inRecord.pTable.pFieldList;
		for (int i = 0; i < arrayList.Count && i < mFieldList.Count; i++)
		{
			IArrayUntyped inArray = (IArrayUntyped)arrayList[i];
			((IArrayUntyped)mFieldList[i]).CopyElement(inArray, inRecord.pIndex, inRecordIdx);
		}
		return UtStatus.NO_ERROR;
	}

	public UtStatus RemoveRecord(int inRecordIdx)
	{
		if (inRecordIdx < mRecordCount)
		{
			foreach (IArrayUntyped mField in mFieldList)
			{
				mField.RemoveElementAt(inRecordIdx);
			}
			mRecordCount--;
			return UtStatus.NO_ERROR;
		}
		return UtStatus.ERROR_OUT_OF_RANGE;
	}

	public UtStatus ClearRecords()
	{
		foreach (IArrayUntyped mField in mFieldList)
		{
			mField.Clear();
		}
		mRecordCount = 0;
		return UtStatus.NO_ERROR;
	}

	public int FindRecord<TYPE>(int inStartRecIdx, int inFieldIdx, TYPE inMatchValue)
	{
		TypedArray<TYPE> typedArray = null;
		if (inFieldIdx >= mFieldList.Count)
		{
			return -1;
		}
		typedArray = (TypedArray<TYPE>)mFieldList[inFieldIdx];
		if (typedArray == null)
		{
			return -1;
		}
		for (int i = inStartRecIdx; i < mRecordCount; i++)
		{
			object obj = inMatchValue;
			object obj2 = typedArray[i];
			if (obj.Equals(obj2))
			{
				return i;
			}
		}
		return -1;
	}

	public TYPE GetValue<TYPE>(int inFieldIdx, int inRecordIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			return ((TypedArray<TYPE>)mFieldList[inFieldIdx])[inRecordIdx];
		}
		return default(TYPE);
	}

	public UtStatus SetValue<TYPE>(int inFieldIdx, int inRecordIdx, TYPE inValue)
	{
		if (inFieldIdx < mFieldList.Count)
		{
			while (inRecordIdx >= mRecordCount)
			{
				AddRecord();
			}
			((TypedArray<TYPE>)mFieldList[inFieldIdx])[inRecordIdx] = inValue;
			return UtStatus.NO_ERROR;
		}
		return UtStatus.ERROR_OUT_OF_RANGE;
	}

	public int AddArrayElement(int inFieldIdx, int inRecordIdx)
	{
		if (inFieldIdx < mFieldList.Count)
		{
			while (inRecordIdx >= mRecordCount)
			{
				AddRecord();
			}
			return ((IArrayUntyped)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx)).AddElement();
		}
		return -1;
	}

	public int AddArrayElement<TYPE>(int inFieldIdx, int inRecordIdx, TYPE inValue)
	{
		int arraySize = GetArraySize(inFieldIdx, inRecordIdx);
		if (SetArrayElementValue(inFieldIdx, inRecordIdx, arraySize, inValue) == UtStatus.NO_ERROR)
		{
			return arraySize;
		}
		return -1;
	}

	public UtStatus RemoveArrayElement(int inFieldIdx, int inRecordIdx, int inArrayIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			IArrayUntyped arrayUntyped = (IArrayUntyped)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx);
			if (inArrayIdx < arrayUntyped.SizeOf())
			{
				arrayUntyped.RemoveElementAt(inArrayIdx);
				return UtStatus.NO_ERROR;
			}
		}
		return UtStatus.ERROR_OUT_OF_RANGE;
	}

	public UtStatus ClearArray(int inFieldIdx, int inRecordIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			((IArrayUntyped)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx)).Clear();
			return UtStatus.NO_ERROR;
		}
		return UtStatus.ERROR_OUT_OF_RANGE;
	}

	public int GetArraySize(int inFieldIdx, int inRecordIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			return ((IArrayUntyped)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx)).SizeOf();
		}
		return -1;
	}

	public TYPE[] GetArray<TYPE>(int inFieldIdx, int inRecordIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			return ((TypedArray<TYPE>)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx)).ToArray();
		}
		return null;
	}

	public TYPE GetArrayElementValue<TYPE>(int inFieldIdx, int inRecordIdx, int inArrayIdx)
	{
		if (inFieldIdx < mFieldList.Count && inRecordIdx < mRecordCount)
		{
			TypedArray<TYPE> typedArray = (TypedArray<TYPE>)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx);
			if (inArrayIdx < typedArray.SizeOf())
			{
				return typedArray[inArrayIdx];
			}
		}
		return default(TYPE);
	}

	public UtStatus SetArrayElementValue<TYPE>(int inFieldIdx, int inRecordIdx, int inArrayIdx, TYPE inValue)
	{
		if (inFieldIdx < mFieldList.Count)
		{
			while (inRecordIdx >= mRecordCount)
			{
				AddRecord();
			}
			TypedArray<TYPE> typedArray = (TypedArray<TYPE>)((IArrayUntyped)mFieldList[inFieldIdx]).GetElementAt(inRecordIdx);
			while (inArrayIdx >= typedArray.SizeOf())
			{
				typedArray.AddElement();
			}
			typedArray[inArrayIdx] = inValue;
			return UtStatus.NO_ERROR;
		}
		return UtStatus.ERROR_OUT_OF_RANGE;
	}
}
