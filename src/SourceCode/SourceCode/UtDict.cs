using System;
using System.Collections;

[Serializable]
public class UtDict : IEnumerable
{
	[Serializable]
	public class Entry
	{
		private uint mKey = uint.MaxValue;

		private object mValue;

		private Entry mNext;

		public uint Key
		{
			get
			{
				return mKey;
			}
			set
			{
				mKey = value;
			}
		}

		public object Value
		{
			get
			{
				return mValue;
			}
			set
			{
				mValue = value;
			}
		}

		public Entry Next
		{
			get
			{
				return mNext;
			}
			set
			{
				mNext = value;
			}
		}

		public Entry()
		{
		}

		public Entry(uint inKey, object inValue, Entry inNext)
		{
			mKey = inKey;
			mValue = inValue;
			mNext = inNext;
		}
	}

	public class Enumerator : IEnumerator
	{
		private Entry mEntryList;

		private Entry mCurrent;

		public object Current => mCurrent;

		public Enumerator(Entry inEntryList)
		{
			mEntryList = inEntryList;
		}

		public bool MoveNext()
		{
			if (mCurrent != null)
			{
				mCurrent = mCurrent.Next;
			}
			else
			{
				mCurrent = mEntryList;
			}
			return mCurrent != null;
		}

		public void Reset()
		{
			mCurrent = null;
		}
	}

	private Entry mEntryList;

	public Entry EntryList
	{
		get
		{
			return mEntryList;
		}
		set
		{
			mEntryList = value;
		}
	}

	public object this[uint inKey]
	{
		get
		{
			object outValue = null;
			if (TryGetValue(inKey, out outValue))
			{
				return outValue;
			}
			return null;
		}
		set
		{
			Entry entry = FindEntry(inKey);
			if (entry != null)
			{
				entry.Value = value;
			}
			else
			{
				AddEntry(inKey, value);
			}
		}
	}

	public IEnumerator GetEnumerator()
	{
		return new Enumerator(mEntryList);
	}

	public void Clear()
	{
		mEntryList = null;
	}

	public bool Remove(uint inKey)
	{
		if (mEntryList != null)
		{
			Entry next = mEntryList;
			Entry entry = null;
			while (next != null)
			{
				if (next.Key == inKey)
				{
					if (entry == null)
					{
						mEntryList = next.Next;
					}
					else
					{
						entry.Next = next.Next;
					}
					next = null;
					return true;
				}
				entry = next;
				next = next.Next;
			}
		}
		return false;
	}

	public bool ContainsKey(uint inKey)
	{
		return FindEntry(inKey) != null;
	}

	public bool TryGetValue(uint inKey, out object outValue)
	{
		Entry entry = FindEntry(inKey);
		if (entry != null)
		{
			outValue = entry.Value;
			return true;
		}
		outValue = null;
		return false;
	}

	private Entry FindEntry(uint inKey)
	{
		if (mEntryList != null)
		{
			for (Entry next = mEntryList; next != null; next = next.Next)
			{
				if (next.Key == inKey)
				{
					return next;
				}
			}
		}
		return null;
	}

	private Entry AddEntry(uint inKey, object inValue)
	{
		mEntryList = new Entry(inKey, inValue, mEntryList);
		return mEntryList;
	}
}
