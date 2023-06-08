public abstract class CSVFileCommon
{
	protected char[] mSpecialChars = new char[4] { ',', '"', '\r', '\n' };

	private const int mDelimiterIndex = 0;

	private const int mQuoteIndex = 1;

	public char pDelimiter
	{
		get
		{
			return mSpecialChars[0];
		}
		set
		{
			mSpecialChars[0] = value;
		}
	}

	public char pQuote
	{
		get
		{
			return mSpecialChars[1];
		}
		set
		{
			mSpecialChars[1] = value;
		}
	}
}
