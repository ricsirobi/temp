using System;

public class UtEndian
{
	public const byte NATIVE = 0;

	public const byte BIG = 1;

	public const byte LITTLE = 2;

	private byte mEndian;

	private static UtEndian[] mEndianList = new UtEndian[3]
	{
		new UtEndian(0),
		new UtEndian(1),
		new UtEndian(2)
	};

	public static byte pNative
	{
		get
		{
			if (!BitConverter.IsLittleEndian)
			{
				return 1;
			}
			return 2;
		}
	}

	public byte pEndian => mEndian;

	public bool pSwap => mEndian != 0;

	public static UtEndian Get(byte inTarget)
	{
		if (inTarget < mEndianList.Length)
		{
			return mEndianList[inTarget];
		}
		return null;
	}

	private UtEndian(byte inTarget)
	{
		if (BitConverter.IsLittleEndian)
		{
			if (inTarget != 2)
			{
				mEndian = inTarget;
			}
		}
		else if (inTarget != 1)
		{
			mEndian = inTarget;
		}
	}

	private void ReverseBytes(byte[] ioBuffer, int inCount)
	{
		int num = 0;
		int num2 = inCount - 1;
		while (num < num2)
		{
			byte b = ioBuffer[num];
			ioBuffer[num] = ioBuffer[num2];
			ioBuffer[num2] = b;
			num++;
			num2--;
		}
	}

	public void Swap(ref short ioValue)
	{
		if (pSwap)
		{
			byte[] bytes = BitConverter.GetBytes(ioValue);
			Array.Reverse(bytes);
			ioValue = BitConverter.ToInt16(bytes, 0);
		}
	}

	public void Swap(byte[] inBuffer, out short outValue)
	{
		if (pSwap)
		{
			ReverseBytes(inBuffer, 2);
		}
		outValue = BitConverter.ToInt16(inBuffer, 0);
	}

	public void Swap(ref ushort ioValue)
	{
		if (pSwap)
		{
			byte[] bytes = BitConverter.GetBytes(ioValue);
			Array.Reverse(bytes);
			ioValue = BitConverter.ToUInt16(bytes, 0);
		}
	}

	public void Swap(byte[] inBuffer, out ushort outValue)
	{
		if (pSwap)
		{
			ReverseBytes(inBuffer, 2);
		}
		outValue = BitConverter.ToUInt16(inBuffer, 0);
	}

	public void Swap(ref int ioValue)
	{
		if (pSwap)
		{
			byte[] bytes = BitConverter.GetBytes(ioValue);
			Array.Reverse(bytes);
			ioValue = BitConverter.ToInt32(bytes, 0);
		}
	}

	public void Swap(byte[] inBuffer, out int outValue)
	{
		if (pSwap)
		{
			ReverseBytes(inBuffer, 4);
		}
		outValue = BitConverter.ToInt32(inBuffer, 0);
	}

	public void Swap(ref uint ioValue)
	{
		if (pSwap)
		{
			byte[] bytes = BitConverter.GetBytes(ioValue);
			Array.Reverse(bytes);
			ioValue = BitConverter.ToUInt32(bytes, 0);
		}
	}

	public void Swap(byte[] inBuffer, out uint outValue)
	{
		if (pSwap)
		{
			ReverseBytes(inBuffer, 4);
		}
		outValue = BitConverter.ToUInt32(inBuffer, 0);
	}

	public void Swap(ref float ioValue)
	{
		if (pSwap)
		{
			byte[] bytes = BitConverter.GetBytes(ioValue);
			Array.Reverse(bytes);
			ioValue = BitConverter.ToSingle(bytes, 0);
		}
	}

	public void Swap(byte[] inBuffer, out float outValue)
	{
		if (pSwap)
		{
			ReverseBytes(inBuffer, 4);
		}
		outValue = BitConverter.ToSingle(inBuffer, 0);
	}
}
