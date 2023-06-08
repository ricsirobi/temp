using System;

public class CustomCircularArray<T>
{
	private T mInvalideValue;

	private int mFirstIndex;

	private int mLastIndex;

	private int mCount;

	private T[] mDataArray;

	public T pInvalideValue => mInvalideValue;

	public int pCount => mCount;

	public T this[int index]
	{
		get
		{
			if (index < 0 && index >= mCount)
			{
				throw new IndexOutOfRangeException("index out of range index" + index);
			}
			return mDataArray[ValidateIndex(mFirstIndex + index)];
		}
	}

	public CustomCircularArray(int capacity, T invalideValue)
	{
		mDataArray = new T[capacity];
		mFirstIndex = (mLastIndex = 0);
		mCount = 0;
		mInvalideValue = invalideValue;
	}

	~CustomCircularArray()
	{
		Clear();
		mDataArray = null;
	}

	public int ValidateIndex(int index)
	{
		int num = 0;
		if (index < 0)
		{
			index %= mDataArray.Length;
			return mDataArray.Length + index;
		}
		return index % mDataArray.Length;
	}

	public bool IsEmpty()
	{
		return mCount == 0;
	}

	public bool IsFull()
	{
		return mCount == mDataArray.Length;
	}

	public bool Add(T item)
	{
		if (IsFull())
		{
			return false;
		}
		if (IsEmpty())
		{
			mDataArray[mLastIndex] = item;
			mCount++;
			mFirstIndex = (mLastIndex = 0);
		}
		else
		{
			mLastIndex = ValidateIndex(mLastIndex + 1);
			mDataArray[mLastIndex] = item;
			mCount++;
		}
		return true;
	}

	public void Clear()
	{
		mCount = 0;
		mFirstIndex = (mLastIndex = 0);
	}

	public bool TryGetNextItem(out T item, bool remove = true)
	{
		if (IsEmpty())
		{
			item = mInvalideValue;
			return false;
		}
		item = mDataArray[mFirstIndex];
		if (remove)
		{
			mCount--;
			if (mFirstIndex != mLastIndex)
			{
				mFirstIndex = ValidateIndex(mFirstIndex + 1);
			}
		}
		return true;
	}

	public T GetNextItem(bool remove = true)
	{
		T result = mInvalideValue;
		if (IsEmpty())
		{
			return result;
		}
		result = mDataArray[mFirstIndex];
		if (remove)
		{
			mCount--;
			if (mFirstIndex != mLastIndex)
			{
				mFirstIndex = ValidateIndex(mFirstIndex + 1);
			}
		}
		return result;
	}

	public T GetFirstItem()
	{
		return mDataArray[mFirstIndex];
	}

	public T GetLastItem()
	{
		return mDataArray[mLastIndex];
	}

	public T GetItemFromLast(int index)
	{
		return mDataArray[ValidateIndex(mLastIndex - index)];
	}
}
