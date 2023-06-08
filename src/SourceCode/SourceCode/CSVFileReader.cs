using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class CSVFileReader : CSVFileCommon
{
	private StringReader mStringReader;

	private string mCurrLine;

	private int mCurrPos;

	public CSVFileReader(string stream)
	{
		mStringReader = new StringReader(stream);
	}

	public bool ReadRow(List<string> columns)
	{
		if (columns == null)
		{
			Debug.LogException(new Exception("columns"));
		}
		mCurrLine = mStringReader.ReadLine();
		mCurrPos = 0;
		if (mCurrLine == null)
		{
			return false;
		}
		int num = 0;
		while (true)
		{
			string text = ((mCurrPos >= mCurrLine.Length || mCurrLine[mCurrPos] != base.pQuote) ? ReadUnquotedColumn() : ReadQuotedColumn());
			if (num < columns.Count)
			{
				columns[num] = text;
			}
			else
			{
				columns.Add(text);
			}
			num++;
			if (mCurrLine == null || mCurrPos == mCurrLine.Length)
			{
				break;
			}
			mCurrPos++;
		}
		if (num < columns.Count)
		{
			columns.RemoveRange(num, columns.Count - num);
		}
		return true;
	}

	private string ReadQuotedColumn()
	{
		mCurrPos++;
		StringBuilder stringBuilder = new StringBuilder();
		while (true)
		{
			if (mCurrPos == mCurrLine.Length)
			{
				mCurrLine = mStringReader.ReadLine();
				mCurrPos = 0;
				if (mCurrLine == null)
				{
					return stringBuilder.ToString();
				}
				stringBuilder.Append("\r\n");
				continue;
			}
			if (mCurrLine[mCurrPos] == base.pQuote)
			{
				int num = mCurrPos + 1;
				if (num >= mCurrLine.Length || mCurrLine[num] != base.pQuote)
				{
					break;
				}
				mCurrPos++;
			}
			stringBuilder.Append(mCurrLine[mCurrPos++]);
		}
		if (mCurrPos < mCurrLine.Length)
		{
			mCurrPos++;
			stringBuilder.Append(ReadUnquotedColumn());
		}
		return stringBuilder.ToString();
	}

	private string ReadUnquotedColumn()
	{
		int num = mCurrPos;
		mCurrPos = mCurrLine.IndexOf(base.pDelimiter, mCurrPos);
		if (mCurrPos == -1)
		{
			mCurrPos = mCurrLine.Length;
		}
		if (mCurrPos > num)
		{
			return mCurrLine.Substring(num, mCurrPos - num);
		}
		return string.Empty;
	}

	public void Dispose()
	{
		mStringReader.Dispose();
	}
}
