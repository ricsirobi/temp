using System;
using System.Collections.Generic;

public class ShuffleBag<T>
{
	private Random mRandom;

	private List<T> mDataList;

	private T mCurrentItem;

	private int mCurrentPos = -1;

	public int pSize => mDataList.Count;

	public ShuffleBag(int inCapacity)
	{
		mDataList = new List<T>(inCapacity);
	}

	public void Add(T inItem, int inAmount)
	{
		for (int i = 0; i < inAmount; i++)
		{
			mDataList.Add(inItem);
		}
		mCurrentPos = pSize - 1;
	}

	public T Next()
	{
		if (mCurrentPos < 1)
		{
			mCurrentPos = pSize - 1;
			mCurrentItem = mDataList[0];
			return mCurrentItem;
		}
		if (mRandom == null)
		{
			mRandom = new Random();
		}
		int index = mRandom.Next(mCurrentPos);
		mCurrentItem = mDataList[index];
		mDataList[index] = mDataList[mCurrentPos];
		mDataList[mCurrentPos] = mCurrentItem;
		mCurrentPos--;
		return mCurrentItem;
	}
}
