using System.Collections.Generic;
using System.Text;

public class UtKey
{
	private static bool mTableInitialized = false;

	private static uint[] mCRCtable = new uint[256];

	private static Dictionary<uint, string> mValueTable = new Dictionary<uint, string>();

	private static void InitializeTable()
	{
		mTableInitialized = true;
		for (int i = 0; i < 256; i++)
		{
			uint num = (uint)i;
			for (int j = 0; j < 8; j++)
			{
				num = (((num & 1) == 0) ? (num >> 1) : (0xEDB88320u ^ (num >> 1)));
			}
			mCRCtable[i] = num;
		}
	}

	private static uint UpdateCRC(uint inCRC, byte[] inBuffer)
	{
		uint num = inCRC ^ 0xFFFFFFFFu;
		if (!mTableInitialized)
		{
			InitializeTable();
		}
		for (int i = 0; i < inBuffer.Length; i++)
		{
			num = mCRCtable[(num ^ inBuffer[i]) & 0xFF] ^ (num >> 8);
		}
		return num ^ 0xFFFFFFFFu;
	}

	public static uint Get(string inKeyName)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(inKeyName);
		uint num = UpdateCRC(0u, bytes);
		mValueTable[num] = inKeyName;
		return num;
	}

	public static string Name(uint inKey)
	{
		string value = null;
		mValueTable.TryGetValue(inKey, out value);
		return value;
	}
}
