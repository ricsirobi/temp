using System;
using System.Text;
using UnityEngine;

public class UtStringEncoding
{
	public const uint ASCII = 0u;

	public const uint NATIVE = 1u;

	public const uint UTF8 = 2u;

	public const uint UTF16 = 3u;

	public const uint UNICODE = 3u;

	public const uint UTF32 = 4u;

	public const uint INVALID = uint.MaxValue;

	public static Encoding GetDotNetEncoding(uint inEncoding, UtEndian inEndian)
	{
		Encoding result = null;
		switch (inEncoding)
		{
		case 0u:
			result = Encoding.ASCII;
			break;
		case 2u:
			result = Encoding.UTF8;
			break;
		case 3u:
			switch (inEndian.pEndian)
			{
			case 1:
				result = Encoding.BigEndianUnicode;
				break;
			case 2:
				result = Encoding.Unicode;
				break;
			case 0:
				result = ((!BitConverter.IsLittleEndian) ? Encoding.BigEndianUnicode : Encoding.Unicode);
				break;
			default:
				Debug.LogError("ERROR: Unsupported encoding!!");
				break;
			}
			break;
		default:
			Debug.LogError("ERROR: Unsupported encoding!! " + inEncoding);
			break;
		}
		return result;
	}
}
