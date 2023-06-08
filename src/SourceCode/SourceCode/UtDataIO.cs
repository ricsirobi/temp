using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class UtDataIO
{
	public class BinaryResourceReader : BinaryReader
	{
		public const int READ_BUFFER_SIZE = 4;

		public byte mVersion;

		public byte mAttributes;

		public byte[] mReadBuffer = new byte[4];

		public BinaryResourceReader(MemoryStream inStream)
			: base(inStream)
		{
		}

		public BinaryResourceReader(FileStream inStream)
			: base(inStream)
		{
		}

		public void Advance(int inCount)
		{
			if (inCount < 4)
			{
				Read(mReadBuffer, 0, inCount);
				return;
			}
			for (int i = 0; i < inCount; i++)
			{
				ReadByte();
			}
		}

		public override short ReadInt16()
		{
			Read(mReadBuffer, 0, 2);
			return BitConverter.ToInt16(mReadBuffer, 0);
		}

		public override ushort ReadUInt16()
		{
			Read(mReadBuffer, 0, 2);
			return BitConverter.ToUInt16(mReadBuffer, 0);
		}

		public override int ReadInt32()
		{
			Read(mReadBuffer, 0, 4);
			return BitConverter.ToInt32(mReadBuffer, 0);
		}

		public override uint ReadUInt32()
		{
			Read(mReadBuffer, 0, 4);
			return BitConverter.ToUInt32(mReadBuffer, 0);
		}

		public override float ReadSingle()
		{
			Read(mReadBuffer, 0, 4);
			return BitConverter.ToSingle(mReadBuffer, 0);
		}

		public short ReadInt16(UtEndian inEndian)
		{
			Read(mReadBuffer, 0, 2);
			inEndian.Swap(mReadBuffer, out short outValue);
			return outValue;
		}

		public ushort ReadUInt16(UtEndian inEndian)
		{
			Read(mReadBuffer, 0, 2);
			inEndian.Swap(mReadBuffer, out ushort outValue);
			return outValue;
		}

		public int ReadInt32(UtEndian inEndian)
		{
			Read(mReadBuffer, 0, 4);
			inEndian.Swap(mReadBuffer, out int outValue);
			return outValue;
		}

		public uint ReadUInt32(UtEndian inEndian)
		{
			Read(mReadBuffer, 0, 4);
			inEndian.Swap(mReadBuffer, out uint outValue);
			return outValue;
		}

		public float ReadSingle(UtEndian inEndian)
		{
			Read(mReadBuffer, 0, 4);
			inEndian.Swap(mReadBuffer, out float outValue);
			return outValue;
		}
	}

	public class ResourceReadStream
	{
		public const int STRING_BUFFER_SIZE = 256;

		public BinaryResourceReader mResReader;

		public UtEndian mEndian;

		public byte[] mStringBuffer = new byte[256];

		public ResourceReadStream(BinaryResourceReader inResReader, UtEndian inEndian)
		{
			mResReader = inResReader;
			mEndian = inEndian;
		}

		public byte[] ReadBytes(int inCount)
		{
			return mResReader.ReadBytes(inCount);
		}

		public int ReadInt32()
		{
			return mResReader.ReadInt32(mEndian);
		}

		public uint ReadUInt32()
		{
			return mResReader.ReadUInt32(mEndian);
		}

		public float ReadSingle()
		{
			return mResReader.ReadSingle(mEndian);
		}

		public Vector3 ReadVector3()
		{
			Vector3 result = default(Vector3);
			result.x = mResReader.ReadSingle(mEndian);
			result.y = mResReader.ReadSingle(mEndian);
			result.z = mResReader.ReadSingle(mEndian);
			return result;
		}

		public string ReadDictString(uint inEntryLength, uint inEncoding)
		{
			int num = (int)mResReader.ReadUInt32(mEndian);
			byte[] array;
			if (num < 256)
			{
				array = mStringBuffer;
				mResReader.Read(array, 0, num);
			}
			else
			{
				array = mResReader.ReadBytes(num);
			}
			int num2 = (int)(inEntryLength - (4 + num));
			if (num2 != 0)
			{
				mResReader.Advance(num2);
			}
			return UtStringEncoding.GetDotNetEncoding(inEncoding, mEndian).GetString(array, 0, num);
		}

		public string ReadString()
		{
			mResReader.ReadUInt32(mEndian);
			uint num = mResReader.ReadUInt32(mEndian);
			uint num2 = mResReader.ReadUInt32(mEndian);
			if (num2 == uint.MaxValue)
			{
				if (num != 0)
				{
					Debug.LogError("ERROR: Null string entry is not empty!!");
				}
				return null;
			}
			byte[] array;
			if ((int)num < 256)
			{
				array = mStringBuffer;
				mResReader.Read(array, 0, (int)num);
			}
			else
			{
				array = mResReader.ReadBytes((int)num);
			}
			int num3 = (int)((num % 4u != 0) ? (4 - num % 4u) : 0);
			if (num3 != 0)
			{
				mResReader.Advance(num3);
			}
			return UtStringEncoding.GetDotNetEncoding(num2, mEndian).GetString(array, 0, (int)num);
		}

		public UtTable ReadTable()
		{
			UtTable utTable = new UtTable();
			uint inEntryCount = mResReader.ReadUInt32(mEndian);
			TableResAssignValues(this, inEntryCount, utTable);
			return utTable;
		}

		public UtDict ReadDict()
		{
			UtDict utDict = new UtDict();
			uint inEntryCount = mResReader.ReadUInt32(mEndian);
			DictResAssignValues(this, inEntryCount, utDict);
			return utDict;
		}

		public UtData ReadData()
		{
			UtData utData = new UtData();
			uint inEntryCount = mResReader.ReadUInt32(mEndian);
			DataResAssignValues(this, inEntryCount, utData);
			return utData;
		}

		public object Read(Type inType)
		{
			if (inType.Equals(typeof(int)))
			{
				return ReadInt32();
			}
			if (inType.Equals(typeof(uint)))
			{
				return ReadUInt32();
			}
			if (inType.Equals(typeof(float)))
			{
				return ReadSingle();
			}
			if (inType.Equals(typeof(Vector3)))
			{
				return ReadVector3();
			}
			if (inType.Equals(typeof(string)))
			{
				return ReadString();
			}
			if (inType.Equals(typeof(UtTable)))
			{
				return ReadTable();
			}
			if (inType.Equals(typeof(UtDict)))
			{
				return ReadDict();
			}
			if (inType.Equals(typeof(UtData)))
			{
				return ReadData();
			}
			Debug.LogError("ERROR: UtDataIO::ResourceReadStream --> unsupported data type: " + inType.ToString());
			return null;
		}

		public TYPE Read<TYPE>()
		{
			return (TYPE)Read(typeof(TYPE));
		}
	}

	public class BinaryResourceWriter : BinaryWriter
	{
		public byte mVersion;

		public byte mAttributes;

		public BinaryResourceWriter(MemoryStream inStream)
			: base(inStream)
		{
		}

		public BinaryResourceWriter(FileStream inStream)
			: base(inStream)
		{
		}

		public override void Write(short value)
		{
			Write(BitConverter.GetBytes(value), 0, 2);
		}

		public override void Write(ushort value)
		{
			Write(BitConverter.GetBytes(value), 0, 2);
		}

		public override void Write(int value)
		{
			Write(BitConverter.GetBytes(value), 0, 4);
		}

		public override void Write(uint value)
		{
			Write(BitConverter.GetBytes(value), 0, 4);
		}

		public override void Write(float value)
		{
			Write(BitConverter.GetBytes(value), 0, 4);
		}
	}

	public class ResourceWriteStream
	{
		public BinaryResourceWriter mResWriter;

		public UtEndian mEndian;

		public ResourceWriteStream(BinaryResourceWriter inResWriter, UtEndian inEndian)
		{
			mResWriter = inResWriter;
			mEndian = inEndian;
		}

		public void Write(byte[] inValue)
		{
			mResWriter.Write(inValue);
		}

		public void Write(byte inValue)
		{
			mResWriter.Write(inValue);
		}

		public void Write(int inValue)
		{
			mEndian.Swap(ref inValue);
			mResWriter.Write(inValue);
		}

		public void Write(uint inValue)
		{
			mEndian.Swap(ref inValue);
			mResWriter.Write(inValue);
		}

		public void Write(float inValue)
		{
			mEndian.Swap(ref inValue);
			mResWriter.Write(inValue);
		}

		public void Write(Vector3 inValue)
		{
			Vector3 vector = default(Vector3);
			vector.x = inValue.x;
			vector.y = inValue.y;
			vector.z = inValue.z;
			mEndian.Swap(ref vector.x);
			mEndian.Swap(ref vector.y);
			mEndian.Swap(ref vector.z);
			mResWriter.Write(vector.x);
			mResWriter.Write(vector.y);
			mResWriter.Write(vector.z);
		}

		public void Write(string inValue)
		{
			bool flag = false;
			string text = inValue;
			if (text == null)
			{
				text = "";
				flag = true;
				Debug.Log("NOTE: NULL STRING WRITTEN AS EMPTY WITH INVALID ENCODING.");
			}
			byte[] bytes = UtStringEncoding.GetDotNetEncoding(3u, mEndian).GetBytes(text.ToCharArray());
			int num = ((bytes.Length % 4 != 0) ? (4 - bytes.Length % 4) : 0);
			uint ioValue = (uint)(8 + bytes.Length + num);
			mEndian.Swap(ref ioValue);
			mResWriter.Write(ioValue);
			uint ioValue2 = (uint)bytes.Length;
			mEndian.Swap(ref ioValue2);
			mResWriter.Write(ioValue2);
			uint ioValue3 = (flag ? uint.MaxValue : 3u);
			mEndian.Swap(ref ioValue3);
			mResWriter.Write(ioValue3);
			mResWriter.Write(bytes);
			for (int i = 0; i < num; i++)
			{
				mResWriter.Write((byte)0);
			}
		}

		public void Write(UtTable inValue)
		{
			uint ioValue = TableGetResFieldCount(inValue);
			mEndian.Swap(ref ioValue);
			mResWriter.Write(ioValue);
			TableResFill(inValue, this);
		}

		public void Write(UtDict inValue)
		{
			uint ioValue = DictGetResEntryCount(inValue);
			mEndian.Swap(ref ioValue);
			mResWriter.Write(ioValue);
			DictResFill(inValue, this);
		}

		public void Write(UtData inValue)
		{
			uint ioValue = DictGetResEntryCount(inValue);
			mEndian.Swap(ref ioValue);
			mResWriter.Write(ioValue);
			DictResFill(inValue, this);
		}

		public void Write<TYPE>(TYPE inValue)
		{
			object obj = inValue;
			Type typeFromHandle = typeof(TYPE);
			if (typeFromHandle.Equals(typeof(int)))
			{
				Write((int)obj);
			}
			else if (typeFromHandle.Equals(typeof(uint)))
			{
				Write((uint)obj);
			}
			else if (typeFromHandle.Equals(typeof(float)))
			{
				Write((float)obj);
			}
			else if (typeFromHandle.Equals(typeof(Vector3)))
			{
				Write((Vector3)obj);
			}
			else if (typeFromHandle.Equals(typeof(string)))
			{
				Write((string)obj);
			}
			else if (typeFromHandle.Equals(typeof(UtDict)))
			{
				Write((UtDict)obj);
			}
			else if (typeFromHandle.Equals(typeof(UtTable)))
			{
				Write((UtTable)obj);
			}
			else if (typeFromHandle.Equals(typeof(UtData)))
			{
				Write((UtData)obj);
			}
			else if (typeFromHandle.Equals(typeof(byte[])))
			{
				Write((byte[])obj);
			}
			else
			{
				Debug.LogError("ERROR: UtDataIO::ResourceWriteStream --> unsupported data type: " + typeFromHandle.ToString());
			}
		}
	}

	private static uint mParentKey = UtKey.Get("__parent");

	private static uint mSchemaKey = UtKey.Get("__schema");

	private static uint mStringsKey = UtKey.Get("__strings");

	public static byte[] DictResCreate(UtDict inDict, UtEndian inEndian)
	{
		byte mEndianFlags = ((inEndian.pEndian == 0) ? UtEndian.pNative : inEndian.pEndian);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryResourceWriter inResWriter = new BinaryResourceWriter(memoryStream);
		UtEntryListHeader inEntryListHeader = default(UtEntryListHeader);
		inEntryListHeader.mEntryCount = DictGetResEntryCount(inDict);
		inEntryListHeader.mRuntimeFlags = 0u;
		inEntryListHeader.mEndianFlags = mEndianFlags;
		inEntryListHeader.mVersion = 0;
		inEntryListHeader.mAttributes = 0;
		inEntryListHeader.mReserved3 = 0;
		ResourceWriteStream inResWriter2 = new ResourceWriteStream(inResWriter, inEndian);
		EntryListHeaderWrite(inEntryListHeader, inResWriter2);
		DictResFill(inDict, inResWriter2);
		return memoryStream.ToArray();
	}

	private static uint DictGetResEntryCount(UtDict inDict)
	{
		uint num = 0u;
		foreach (KeyValuePair<object, object> item in inDict)
		{
			UtType type = UtTypeConvert.GetType(item.Value.GetType());
			if (type != 0 && (type != UtType.UTDATA || mParentKey != (uint)item.Key))
			{
				num++;
			}
		}
		return num;
	}

	private static void DictResFill(UtDict inDict, ResourceWriteStream inResWriter)
	{
		uint num = DictGetResEntryCount(inDict);
		uint num2 = 0u;
		foreach (KeyValuePair<object, object> item in inDict)
		{
			UtDictResEntry inDictEntry = default(UtDictResEntry);
			object value = item.Value;
			inDictEntry.mKey = (uint)item.Key;
			inDictEntry.mType = (uint)UtTypeConvert.GetType(value.GetType());
			switch ((UtType)inDictEntry.mType)
			{
			case UtType.ULONG:
			{
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 4u;
				inDictEntry.mData = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				uint inValue = (uint)value;
				inResWriter.Write(inValue);
				num2++;
				break;
			}
			case UtType.SLONG:
			{
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 4u;
				inDictEntry.mData = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				int inValue2 = (int)value;
				inResWriter.Write(inValue2);
				num2++;
				break;
			}
			case UtType.FLOAT:
			{
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 4u;
				inDictEntry.mData = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				float inValue3 = (float)value;
				inResWriter.Write(inValue3);
				num2++;
				break;
			}
			case UtType.VOIDPTR:
			{
				byte[] array = (byte[])value;
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = (uint)array.Length;
				inDictEntry.mData = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				inResWriter.Write(array);
				num2++;
				break;
			}
			case UtType.VECTOR3D:
			{
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 16u;
				DictResEntryWrite(inDictEntry, inResWriter);
				inDictEntry.mData = 3u;
				inResWriter.Write(inDictEntry.mData);
				Vector3 vector = (Vector3)value;
				inResWriter.Write(vector.x);
				inResWriter.Write(vector.y);
				inResWriter.Write(vector.z);
				num2++;
				break;
			}
			case UtType.STRING:
			{
				string text = (string)value;
				if (text == null)
				{
					text = "";
					Debug.Log("NOTE: NULL STRING WRITTEN AS EMPTY.");
				}
				byte[] bytes = UtStringEncoding.GetDotNetEncoding(3u, inResWriter.mEndian).GetBytes(text.ToCharArray());
				inDictEntry.mTypeFlags = 3u;
				inDictEntry.mData = (uint)bytes.Length;
				int num6 = ((bytes.Length % 4 != 0) ? (4 - bytes.Length % 4) : 0);
				inDictEntry.mLength = (uint)(inDictEntry.mData + num6 + 4);
				DictResEntryWrite(inDictEntry, inResWriter);
				inResWriter.Write(inDictEntry.mData);
				inResWriter.Write(bytes);
				for (int i = 0; i < num6; i++)
				{
					inResWriter.Write((byte)0);
				}
				num2++;
				break;
			}
			case UtType.UTDICT:
			{
				UtDict inDict3 = (UtDict)value;
				int num5 = (int)inResWriter.mResWriter.BaseStream.Position;
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				inDictEntry.mData = DictGetResEntryCount(inDict3);
				inResWriter.Write(inDictEntry.mData);
				DictResFill(inDict3, inResWriter);
				inDictEntry.mLength = (uint)(inResWriter.mResWriter.BaseStream.Position - num5 - 16);
				inResWriter.mResWriter.Seek(num5, SeekOrigin.Begin);
				DictResEntryWrite(inDictEntry, inResWriter);
				inResWriter.mResWriter.Seek(0, SeekOrigin.End);
				num2++;
				break;
			}
			case UtType.UTTABLE:
			{
				UtTable inTable = (UtTable)value;
				int num4 = (int)inResWriter.mResWriter.BaseStream.Position;
				inDictEntry.mTypeFlags = 0u;
				inDictEntry.mLength = 0u;
				DictResEntryWrite(inDictEntry, inResWriter);
				inDictEntry.mData = TableGetResFieldCount(inTable);
				inResWriter.Write(inDictEntry.mData);
				TableResFill(inTable, inResWriter);
				inDictEntry.mLength = (uint)(inResWriter.mResWriter.BaseStream.Position - num4 - 16);
				inResWriter.mResWriter.Seek(num4, SeekOrigin.Begin);
				DictResEntryWrite(inDictEntry, inResWriter);
				inResWriter.mResWriter.Seek(0, SeekOrigin.End);
				num2++;
				break;
			}
			case UtType.UTDATA:
				if (mParentKey != inDictEntry.mKey)
				{
					UtDict inDict2 = (UtDict)value;
					int num3 = (int)inResWriter.mResWriter.BaseStream.Position;
					inDictEntry.mTypeFlags = 0u;
					inDictEntry.mLength = 0u;
					DictResEntryWrite(inDictEntry, inResWriter);
					inDictEntry.mData = DictGetResEntryCount(inDict2);
					inResWriter.Write(inDictEntry.mData);
					DictResFill(inDict2, inResWriter);
					inDictEntry.mLength = (uint)(inResWriter.mResWriter.BaseStream.Position - num3 - 16);
					inResWriter.mResWriter.Seek(num3, SeekOrigin.Begin);
					DictResEntryWrite(inDictEntry, inResWriter);
					inResWriter.mResWriter.Seek(0, SeekOrigin.End);
					num2++;
				}
				break;
			default:
				Debug.Log("WARNING: DictResFill, skipping unsupported type " + value.GetType().ToString());
				break;
			}
		}
		if (num != num2)
		{
			Debug.Log("ERROR: DictResFill, entry count mismatch. Expected " + num + ", actual " + num2);
		}
	}

	public static UtStatus DictResAssign(byte[] inDictRes, UtDict ioDict)
	{
		try
		{
			using MemoryStream inStream = new MemoryStream(inDictRes);
			using BinaryResourceReader binaryResourceReader = new BinaryResourceReader(inStream);
			UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
			UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
			DictResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioDict);
			binaryResourceReader.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtDict parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.NO_ERROR;
	}

	public static UtStatus DictFileRead(string inFile, UtDict ioDict)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(inFile);
			if (fileStream != null)
			{
				using (BinaryResourceReader binaryResourceReader = new BinaryResourceReader(fileStream))
				{
					UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
					UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
					DictResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioDict);
					binaryResourceReader.Close();
					return UtStatus.NO_ERROR;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtDict parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.ERROR_NOT_FOUND;
	}

	private static void DictResAssignValues(ResourceReadStream inResReader, uint inEntryCount, UtDict ioDict)
	{
		UtDictResEntry ioDictEntry = default(UtDictResEntry);
		for (uint num = 0u; num < inEntryCount; num++)
		{
			DictResEntryAssign(inResReader, ref ioDictEntry);
			switch ((UtType)ioDictEntry.mType)
			{
			case UtType.ULONG:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect ULONG resource length: " + ioDictEntry.mLength);
				}
				ioDict[ioDictEntry.mKey] = inResReader.ReadUInt32();
				break;
			case UtType.SLONG:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect SLONG resource length: " + ioDictEntry.mLength);
				}
				ioDict[ioDictEntry.mKey] = inResReader.ReadInt32();
				break;
			case UtType.FLOAT:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect FLOAT resource length: " + ioDictEntry.mLength);
				}
				ioDict[ioDictEntry.mKey] = inResReader.ReadSingle();
				break;
			case UtType.VOIDPTR:
			{
				byte[] value = inResReader.ReadBytes((int)ioDictEntry.mLength);
				ioDict[ioDictEntry.mKey] = value;
				break;
			}
			case UtType.VECTOR3D:
			{
				if (ioDictEntry.mLength != 12)
				{
					Debug.LogError("ERROR: Incorrect VECTOR3D resource length: " + ioDictEntry.mLength);
				}
				uint num2 = inResReader.ReadUInt32();
				if (num2 != 3)
				{
					Debug.LogError("ERROR: Incorrect VECTOR3D resource element count: " + num2);
				}
				ioDict[ioDictEntry.mKey] = inResReader.ReadVector3();
				break;
			}
			case UtType.STRING:
				ioDict[ioDictEntry.mKey] = inResReader.ReadDictString(ioDictEntry.mLength, ioDictEntry.mTypeFlags);
				break;
			case UtType.UTDICT:
			{
				UtDict utDict = new UtDict();
				uint inEntryCount4 = inResReader.ReadUInt32();
				DictResAssignValues(inResReader, inEntryCount4, utDict);
				ioDict[ioDictEntry.mKey] = utDict;
				break;
			}
			case UtType.UTTABLE:
			{
				UtTable utTable = new UtTable();
				uint inEntryCount3 = inResReader.ReadUInt32();
				ioDict[ioDictEntry.mKey] = utTable;
				TableResAssignValues(inResReader, inEntryCount3, utTable);
				ioDict[ioDictEntry.mKey] = utTable;
				break;
			}
			case UtType.UTDATA:
			{
				UtData utData = new UtData();
				uint inEntryCount2 = inResReader.ReadUInt32();
				DataResAssignValues(inResReader, inEntryCount2, utData);
				ioDict[ioDictEntry.mKey] = utData;
				break;
			}
			default:
			{
				UtType mType = (UtType)ioDictEntry.mType;
				Debug.LogError("ERROR: Unknown resource type: " + mType);
				break;
			}
			}
		}
	}

	public static byte[] TableResCreate(UtTable inTable, UtEndian inEndian)
	{
		byte mEndianFlags = ((inEndian.pEndian == 0) ? UtEndian.pNative : inEndian.pEndian);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryResourceWriter binaryResourceWriter = new BinaryResourceWriter(memoryStream);
		binaryResourceWriter.mVersion = 0;
		binaryResourceWriter.mAttributes = 2;
		UtEntryListHeader inEntryListHeader = default(UtEntryListHeader);
		inEntryListHeader.mEntryCount = TableGetResFieldCount(inTable);
		inEntryListHeader.mRuntimeFlags = 0u;
		inEntryListHeader.mEndianFlags = mEndianFlags;
		inEntryListHeader.mVersion = binaryResourceWriter.mVersion;
		inEntryListHeader.mAttributes = binaryResourceWriter.mAttributes;
		inEntryListHeader.mReserved3 = 0;
		ResourceWriteStream inResWriter = new ResourceWriteStream(binaryResourceWriter, inEndian);
		EntryListHeaderWrite(inEntryListHeader, inResWriter);
		TableResFill(inTable, inResWriter);
		return memoryStream.ToArray();
	}

	public static byte[] TableCollectionResCreate(KeyValuePair<string, UtTable>[] inCollection, UtEndian inEndian)
	{
		byte mEndianFlags = ((inEndian.pEndian == 0) ? UtEndian.pNative : inEndian.pEndian);
		using MemoryStream memoryStream = new MemoryStream();
		using BinaryResourceWriter binaryResourceWriter = new BinaryResourceWriter(memoryStream);
		binaryResourceWriter.mVersion = 0;
		binaryResourceWriter.mAttributes = 2;
		UtEntryListHeader inEntryListHeader = default(UtEntryListHeader);
		inEntryListHeader.mEntryCount = (uint)inCollection.Length;
		inEntryListHeader.mRuntimeFlags = 0u;
		inEntryListHeader.mEndianFlags = mEndianFlags;
		inEntryListHeader.mVersion = binaryResourceWriter.mVersion;
		inEntryListHeader.mAttributes = binaryResourceWriter.mAttributes;
		inEntryListHeader.mReserved3 = 0;
		ResourceWriteStream resourceWriteStream = new ResourceWriteStream(binaryResourceWriter, inEndian);
		EntryListHeaderWrite(inEntryListHeader, resourceWriteStream);
		for (int i = 0; i < inCollection.Length; i++)
		{
			KeyValuePair<string, UtTable> keyValuePair = inCollection[i];
			if (keyValuePair.Key == null)
			{
				Debug.LogError("ERROR: TableCollectionResCreate --> Name entry is null!");
				continue;
			}
			if (keyValuePair.Value == null)
			{
				Debug.LogError("ERROR: TableCollectionResCreate --> Table entry is null!");
				continue;
			}
			resourceWriteStream.Write(keyValuePair.Key);
			resourceWriteStream.Write(keyValuePair.Value);
		}
		return memoryStream.ToArray();
	}

	private static uint TableGetResFieldCount(UtTable inTable)
	{
		for (int i = 0; i < inTable.GetFieldCount(); i++)
		{
			if (inTable.GetFieldType(i) == UtType.NONE)
			{
				return (uint)i;
			}
		}
		return (uint)inTable.GetFieldCount();
	}

	private static void TableResFill(UtTable inTable, ResourceWriteStream inResWriter)
	{
		uint num = TableGetResFieldCount(inTable);
		for (int i = 0; i < num; i++)
		{
			TableArrayResFill((UtTable.IArrayUntyped)inTable.pFieldList[i], inResWriter);
			if ((inResWriter.mResWriter.mAttributes & 2u) != 0)
			{
				string inValue = "";
				switch (UtTypeConvert.GetType(inTable.pDataList[i].GetType()))
				{
				case UtType.STRING:
					inValue = (string)inTable.pDataList[i];
					break;
				case UtType.ULONG:
				case UtType.SLONG:
					inValue = inTable.pDataList[i].ToString();
					break;
				}
				inResWriter.Write(inValue);
			}
			else
			{
				uint inValue2 = 0u;
				switch (UtTypeConvert.GetType(inTable.pDataList[i].GetType()))
				{
				case UtType.STRING:
					inValue2 = UtKey.Get((string)inTable.pDataList[i]);
					break;
				case UtType.ULONG:
				case UtType.SLONG:
					inValue2 = (uint)inTable.pDataList[i];
					break;
				}
				inResWriter.Write(inValue2);
			}
		}
	}

	private static void TableArrayResFill(UtTable.IArrayUntyped inArray, ResourceWriteStream inResWriter)
	{
		UtTableArrayResEntry inResEntry = default(UtTableArrayResEntry);
		inResEntry.mFlags = 1u;
		inResEntry.mType = (uint)inArray.ElementType();
		inResEntry.mElementCount = (uint)inArray.SizeOf();
		int num = (int)inResWriter.mResWriter.BaseStream.Position;
		inResEntry.mLength = 0u;
		TableArrayResEntryWrite(inResEntry, inResWriter);
		switch (inArray.ElementType())
		{
		case UtType.ULONG:
			TableArrayResFillElements((UtTable.TypedArray<uint>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.SLONG:
			TableArrayResFillElements((UtTable.TypedArray<int>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.FLOAT:
			TableArrayResFillElements((UtTable.TypedArray<float>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.VECTOR3D:
			TableArrayResFillElements((UtTable.TypedArray<Vector3>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.STRING:
			TableArrayResFillElements((UtTable.TypedArray<string>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTDICT:
			TableArrayResFillElements((UtTable.TypedArray<UtDict>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTTABLE:
			TableArrayResFillElements((UtTable.TypedArray<UtTable>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTDATA:
			TableArrayResFillElements((UtTable.TypedArray<UtData>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.ULONG_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<uint>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.SLONG_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<int>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.FLOAT_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<float>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.VECTOR3D_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<Vector3>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.STRING_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<string>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTTABLE_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<UtTable>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTDICT_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<UtDict>>)inArray, ref inResEntry, inResWriter);
			break;
		case UtType.UTDATA_ARRAY:
			TableNArrayResFillElements((UtTable.TypedArray<UtTable.TypedArray<UtData>>)inArray, ref inResEntry, inResWriter);
			break;
		default:
			Debug.Log("ERROR: TableArrayResFill - Unknown type " + inArray.ElementType());
			break;
		}
		inResEntry.mLength = (uint)(inResWriter.mResWriter.BaseStream.Position - num - 16);
		inResWriter.mResWriter.Seek(num, SeekOrigin.Begin);
		TableArrayResEntryWrite(inResEntry, inResWriter);
		inResWriter.mResWriter.Seek(0, SeekOrigin.End);
	}

	private static void TableArrayResFillElements<TYPE>(UtTable.TypedArray<TYPE> inArray, ref UtTableArrayResEntry inResEntry, ResourceWriteStream ioResStream)
	{
		if ((inResEntry.mFlags & (true ? 1u : 0u)) != 0)
		{
			bool flag = true;
			if (!typeof(TYPE).IsValueType() && inArray.pDefault == null)
			{
				flag = false;
				inResEntry.mFlags &= 4294967294u;
			}
			if (flag)
			{
				ioResStream.Write(inArray.pDefault);
			}
		}
		foreach (TYPE item in inArray)
		{
			ioResStream.Write(item);
		}
	}

	private static void TableNArrayResFillElements<TYPE>(UtTable.TypedArray<UtTable.TypedArray<TYPE>> inArray, ref UtTableArrayResEntry inResEntry, ResourceWriteStream inResWriter)
	{
		if ((inResEntry.mFlags & (true ? 1u : 0u)) != 0)
		{
			if (inArray.pDefault != null)
			{
				TableArrayResFill(inArray.pDefault, inResWriter);
			}
			else
			{
				inResEntry.mFlags &= 4294967294u;
			}
		}
		foreach (UtTable.TypedArray<TYPE> item in inArray)
		{
			TableArrayResFill(item, inResWriter);
		}
	}

	public static UtStatus TableResAssign(byte[] inTableRes, UtTable ioTable)
	{
		try
		{
			using MemoryStream inStream = new MemoryStream(inTableRes);
			using BinaryResourceReader binaryResourceReader = new BinaryResourceReader(inStream);
			UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
			UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
			TableResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioTable);
			binaryResourceReader.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtTable parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.NO_ERROR;
	}

	public static UtStatus TableFileRead(string inFile, UtTable ioTable)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(inFile);
			if (fileStream != null)
			{
				using (BinaryResourceReader binaryResourceReader = new BinaryResourceReader(fileStream))
				{
					UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
					UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
					TableResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioTable);
					binaryResourceReader.Close();
					return UtStatus.NO_ERROR;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtTable parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.ERROR_NOT_FOUND;
	}

	public static UtStatus TableCollectionResAssign(byte[] inCollectionRes, Dictionary<string, UtTable> ioCollection)
	{
		string text = "<?xml version=\"1.0\"?>";
		string @string = Encoding.UTF8.GetString(inCollectionRes, 0, text.Length);
		if (@string.Contains(text) || text.Contains(@string))
		{
			Debug.LogError("ERROR: Binary UtTableCollection parse failed! Non-binary XML data format!");
			return UtStatus.ERROR_PARAMS;
		}
		try
		{
			using MemoryStream inStream = new MemoryStream(inCollectionRes);
			using BinaryResourceReader binaryResourceReader = new BinaryResourceReader(inStream);
			UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
			UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
			ResourceReadStream resourceReadStream = new ResourceReadStream(binaryResourceReader, inEndian);
			for (int i = 0; i < ioEntryListHeader.mEntryCount; i++)
			{
				string key = resourceReadStream.ReadString();
				UtTable value = resourceReadStream.ReadTable();
				ioCollection[key] = value;
			}
			binaryResourceReader.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtTableCollection parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.NO_ERROR;
	}

	public static UtStatus TableCollectionFileRead(string inFile, Dictionary<string, UtTable> ioCollection)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(inFile);
			if (fileStream != null)
			{
				using (BinaryResourceReader binaryResourceReader = new BinaryResourceReader(fileStream))
				{
					UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
					UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
					ResourceReadStream resourceReadStream = new ResourceReadStream(binaryResourceReader, inEndian);
					for (int i = 0; i < ioEntryListHeader.mEntryCount; i++)
					{
						string key = resourceReadStream.ReadString();
						UtTable value = resourceReadStream.ReadTable();
						ioCollection[key] = value;
					}
					binaryResourceReader.Close();
					return UtStatus.NO_ERROR;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtTable parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.ERROR_NOT_FOUND;
	}

	private static void TableResAssignValues(ResourceReadStream inResReader, uint inEntryCount, UtTable ioTable)
	{
		int num = 0;
		if (ioTable.GetFieldCount() != 0)
		{
			ioTable.Clear();
		}
		if (inEntryCount == 0)
		{
			return;
		}
		for (num = 0; num < inEntryCount; num++)
		{
			UtTableArrayResEntry ioArrayEntry = default(UtTableArrayResEntry);
			TableArrayResEntryAssign(inResReader, ref ioArrayEntry);
			UtType mType = (UtType)ioArrayEntry.mType;
			switch (mType)
			{
			case UtType.ULONG:
			{
				UtTable.TypedArray<uint> typedArray16 = new UtTable.TypedArray<uint>();
				ioTable.pFieldList.Add(typedArray16);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray16);
				break;
			}
			case UtType.SLONG:
			{
				UtTable.TypedArray<int> typedArray15 = new UtTable.TypedArray<int>();
				ioTable.pFieldList.Add(typedArray15);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray15);
				break;
			}
			case UtType.FLOAT:
			{
				UtTable.TypedArray<float> typedArray14 = new UtTable.TypedArray<float>();
				ioTable.pFieldList.Add(typedArray14);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray14);
				break;
			}
			case UtType.VECTOR3D:
			{
				UtTable.TypedArray<Vector3> typedArray13 = new UtTable.TypedArray<Vector3>();
				ioTable.pFieldList.Add(typedArray13);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray13);
				break;
			}
			case UtType.STRING:
			{
				UtTable.TypedArray<string> typedArray12 = new UtTable.TypedArray<string>();
				ioTable.pFieldList.Add(typedArray12);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray12);
				break;
			}
			case UtType.UTTABLE:
			{
				UtTable.TypedArray<UtTable> typedArray11 = new UtTable.TypedArray<UtTable>();
				ioTable.pFieldList.Add(typedArray11);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray11);
				break;
			}
			case UtType.UTDICT:
			{
				UtTable.TypedArray<UtDict> typedArray10 = new UtTable.TypedArray<UtDict>();
				ioTable.pFieldList.Add(typedArray10);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray10);
				break;
			}
			case UtType.UTDATA:
			{
				UtTable.TypedArray<UtData> typedArray9 = new UtTable.TypedArray<UtData>();
				ioTable.pFieldList.Add(typedArray9);
				TableArrayResAssignElements(inResReader, ioArrayEntry, typedArray9);
				break;
			}
			case UtType.ULONG_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<uint>> typedArray8 = new UtTable.TypedArray<UtTable.TypedArray<uint>>(new UtTable.TypedArray<uint>());
				ioTable.pFieldList.Add(typedArray8);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray8);
				break;
			}
			case UtType.SLONG_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<int>> typedArray7 = new UtTable.TypedArray<UtTable.TypedArray<int>>(new UtTable.TypedArray<int>());
				ioTable.pFieldList.Add(typedArray7);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray7);
				break;
			}
			case UtType.FLOAT_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<float>> typedArray6 = new UtTable.TypedArray<UtTable.TypedArray<float>>(new UtTable.TypedArray<float>());
				ioTable.pFieldList.Add(typedArray6);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray6);
				break;
			}
			case UtType.VECTOR3D_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<Vector3>> typedArray5 = new UtTable.TypedArray<UtTable.TypedArray<Vector3>>(new UtTable.TypedArray<Vector3>());
				ioTable.pFieldList.Add(typedArray5);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray5);
				break;
			}
			case UtType.STRING_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<string>> typedArray4 = new UtTable.TypedArray<UtTable.TypedArray<string>>(new UtTable.TypedArray<string>());
				ioTable.pFieldList.Add(typedArray4);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray4);
				break;
			}
			case UtType.UTTABLE_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<UtTable>> typedArray3 = new UtTable.TypedArray<UtTable.TypedArray<UtTable>>(new UtTable.TypedArray<UtTable>());
				ioTable.pFieldList.Add(typedArray3);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray3);
				break;
			}
			case UtType.UTDICT_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<UtDict>> typedArray2 = new UtTable.TypedArray<UtTable.TypedArray<UtDict>>(new UtTable.TypedArray<UtDict>());
				ioTable.pFieldList.Add(typedArray2);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray2);
				break;
			}
			case UtType.UTDATA_ARRAY:
			{
				UtTable.TypedArray<UtTable.TypedArray<UtData>> typedArray = new UtTable.TypedArray<UtTable.TypedArray<UtData>>(new UtTable.TypedArray<UtData>());
				ioTable.pFieldList.Add(typedArray);
				TableNArrayResAssignElements(inResReader, ioArrayEntry, typedArray);
				break;
			}
			default:
				Debug.LogError("ERROR: Unknown resource type: " + mType);
				break;
			}
			if ((inResReader.mResReader.mAttributes & 2u) != 0)
			{
				string value = inResReader.ReadString();
				ioTable.pDataList.Add(value);
			}
			else
			{
				uint num2 = inResReader.ReadUInt32();
				ioTable.pDataList.Add(num2);
			}
		}
		if (inEntryCount != ioTable.pFieldList.Count)
		{
			Debug.LogError("ERROR: Field count mismatch in UtTable resource!!");
		}
		ioTable.pRecordCount = ((UtTable.IArrayUntyped)ioTable.pFieldList[0]).SizeOf();
		for (num = 1; num < inEntryCount; num++)
		{
			if (((UtTable.IArrayUntyped)ioTable.pFieldList[num]).SizeOf() != ioTable.pRecordCount)
			{
				Debug.LogError("ERROR: Record count mismatch in UtTable resource!!");
			}
		}
	}

	private static void TableArrayResAssignElements<TYPE>(ResourceReadStream inResStream, UtTableArrayResEntry inResEntry, UtTable.TypedArray<TYPE> inArray)
	{
		if ((inResEntry.mFlags & (true ? 1u : 0u)) != 0)
		{
			inArray.pDefault = inResStream.Read<TYPE>();
		}
		if (inResEntry.mElementCount != 0)
		{
			for (uint num = 0u; num < inResEntry.mElementCount; num++)
			{
				inArray.AddElement();
				inArray[(int)num] = inResStream.Read<TYPE>();
			}
		}
	}

	private static void TableNArrayResAssignElements<TYPE>(ResourceReadStream inResStream, UtTableArrayResEntry inResEntry, UtTable.TypedArray<UtTable.TypedArray<TYPE>> inNArray)
	{
		UtTableArrayResEntry ioArrayEntry = default(UtTableArrayResEntry);
		if ((inResEntry.mFlags & (true ? 1u : 0u)) != 0)
		{
			inNArray.pDefault = new UtTable.TypedArray<TYPE>();
			TableArrayResEntryAssign(inResStream, ref ioArrayEntry);
			TableArrayResAssignElements(inResStream, ioArrayEntry, inNArray.pDefault);
		}
		if (inResEntry.mElementCount != 0)
		{
			for (uint num = 0u; num < inResEntry.mElementCount; num++)
			{
				TableArrayResEntryAssign(inResStream, ref ioArrayEntry);
				inNArray.AddElement();
				TableArrayResAssignElements(inResStream, ioArrayEntry, inNArray[(int)num]);
			}
		}
	}

	public static UtStatus DataResAssign(byte[] inDataRes, UtData ioData)
	{
		try
		{
			using MemoryStream inStream = new MemoryStream(inDataRes);
			using BinaryResourceReader binaryResourceReader = new BinaryResourceReader(inStream);
			UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
			UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
			DataResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioData);
			binaryResourceReader.Close();
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtData parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.NO_ERROR;
	}

	public static UtStatus DataFileRead(string inFile, UtData ioData)
	{
		try
		{
			using FileStream fileStream = File.OpenRead(inFile);
			if (fileStream != null)
			{
				using (BinaryResourceReader binaryResourceReader = new BinaryResourceReader(fileStream))
				{
					UtEntryListHeader ioEntryListHeader = default(UtEntryListHeader);
					UtEndian inEndian = EntryListHeaderAssign(binaryResourceReader, ref ioEntryListHeader);
					DataResAssignValues(new ResourceReadStream(binaryResourceReader, inEndian), ioEntryListHeader.mEntryCount, ioData);
					binaryResourceReader.Close();
					return UtStatus.NO_ERROR;
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("ERROR: Binary UtData parse failed!, exception: " + ex.ToString());
			return UtStatus.ERROR_UNKNOWN;
		}
		return UtStatus.ERROR_NOT_FOUND;
	}

	private static void DataResAssignValues(ResourceReadStream inResReader, uint inEntryCount, UtData ioData)
	{
		UtDictResEntry ioDictEntry = default(UtDictResEntry);
		for (uint num = 0u; num < inEntryCount; num++)
		{
			DictResEntryAssign(inResReader, ref ioDictEntry);
			switch ((UtType)ioDictEntry.mType)
			{
			case UtType.ULONG:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect ULONG resource length: " + ioDictEntry.mLength);
				}
				ioData[ioDictEntry.mKey] = inResReader.ReadUInt32();
				break;
			case UtType.SLONG:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect SLONG resource length: " + ioDictEntry.mLength);
				}
				ioData[ioDictEntry.mKey] = inResReader.ReadInt32();
				break;
			case UtType.FLOAT:
				if (ioDictEntry.mLength != 4)
				{
					Debug.LogError("ERROR: Incorrect FLOAT resource length: " + ioDictEntry.mLength);
				}
				ioData[ioDictEntry.mKey] = inResReader.ReadSingle();
				break;
			case UtType.VOIDPTR:
			{
				byte[] value = inResReader.ReadBytes((int)ioDictEntry.mLength);
				ioData[ioDictEntry.mKey] = value;
				break;
			}
			case UtType.VECTOR3D:
			{
				if (ioDictEntry.mLength != 12)
				{
					Debug.LogError("ERROR: Incorrect VECTOR3D resource length: " + ioDictEntry.mLength);
				}
				uint num2 = inResReader.ReadUInt32();
				if (num2 != 3)
				{
					Debug.LogError("ERROR: Incorrect VECTOR3D resource element count: " + num2);
				}
				ioData[ioDictEntry.mKey] = inResReader.ReadVector3();
				break;
			}
			case UtType.STRING:
				ioData[ioDictEntry.mKey] = inResReader.ReadDictString(ioDictEntry.mLength, ioDictEntry.mTypeFlags);
				break;
			case UtType.UTDICT:
			{
				uint inEntryCount4 = inResReader.ReadUInt32();
				if (ioDictEntry.mKey == mSchemaKey || ioDictEntry.mKey == mStringsKey)
				{
					UtDict utDict = new UtDict();
					DictResAssignValues(inResReader, inEntryCount4, utDict);
					ioData[ioDictEntry.mKey] = utDict;
				}
				else
				{
					UtData utData2 = new UtData();
					DataResAssignValues(inResReader, inEntryCount4, utData2);
					utData2[mParentKey] = ioData;
					ioData[ioDictEntry.mKey] = utData2;
				}
				break;
			}
			case UtType.UTTABLE:
			{
				UtTable utTable = new UtTable();
				uint inEntryCount3 = inResReader.ReadUInt32();
				ioData[ioDictEntry.mKey] = utTable;
				TableResAssignValues(inResReader, inEntryCount3, utTable);
				ioData[ioDictEntry.mKey] = utTable;
				break;
			}
			case UtType.UTDATA:
			{
				UtData utData = new UtData();
				uint inEntryCount2 = inResReader.ReadUInt32();
				DataResAssignValues(inResReader, inEntryCount2, utData);
				utData[mParentKey] = ioData;
				ioData[ioDictEntry.mKey] = utData;
				break;
			}
			default:
			{
				UtType mType = (UtType)ioDictEntry.mType;
				Debug.LogError("ERROR: Unknown resource type: " + mType);
				break;
			}
			}
		}
		ioData.Resolve();
	}

	private static void EntryListHeaderWrite(UtEntryListHeader inEntryListHeader, ResourceWriteStream inResWriter)
	{
		inResWriter.Write(inEntryListHeader.mEntryCount);
		inResWriter.Write(inEntryListHeader.mRuntimeFlags);
		inResWriter.Write(inEntryListHeader.mEndianFlags);
		inResWriter.Write(inEntryListHeader.mVersion);
		inResWriter.Write(inEntryListHeader.mAttributes);
		inResWriter.Write(inEntryListHeader.mReserved3);
	}

	private static void DictResEntryWrite(UtDictResEntry inDictEntry, ResourceWriteStream inResWriter)
	{
		inResWriter.Write(inDictEntry.mKey);
		inResWriter.Write(inDictEntry.mType);
		inResWriter.Write(inDictEntry.mTypeFlags);
		inResWriter.Write(inDictEntry.mLength);
	}

	private static void TableArrayResEntryWrite(UtTableArrayResEntry inArrayEntry, ResourceWriteStream inResWriter)
	{
		inResWriter.Write(inArrayEntry.mFlags);
		inResWriter.Write(inArrayEntry.mType);
		inResWriter.Write(inArrayEntry.mLength);
		inResWriter.Write(inArrayEntry.mElementCount);
	}

	private static UtEndian EntryListHeaderAssign(BinaryResourceReader ioResReader, ref UtEntryListHeader ioEntryListHeader)
	{
		ioEntryListHeader.mEntryCount = ioResReader.ReadUInt32();
		ioEntryListHeader.mRuntimeFlags = ioResReader.ReadUInt32();
		ioEntryListHeader.mEndianFlags = ioResReader.ReadByte();
		ioEntryListHeader.mVersion = ioResReader.ReadByte();
		ioEntryListHeader.mAttributes = ioResReader.ReadByte();
		ioEntryListHeader.mReserved3 = ioResReader.ReadByte();
		UtEndian utEndian = UtEndian.Get(ioEntryListHeader.mEndianFlags);
		if (utEndian.pSwap)
		{
			utEndian.Swap(ref ioEntryListHeader.mEntryCount);
			utEndian.Swap(ref ioEntryListHeader.mRuntimeFlags);
		}
		ioResReader.mVersion = ioEntryListHeader.mVersion;
		ioResReader.mAttributes = ioEntryListHeader.mAttributes;
		return utEndian;
	}

	private static void DictResEntryAssign(ResourceReadStream inResReader, ref UtDictResEntry ioDictEntry)
	{
		ioDictEntry.mKey = inResReader.ReadUInt32();
		ioDictEntry.mType = inResReader.ReadUInt32();
		ioDictEntry.mTypeFlags = inResReader.ReadUInt32();
		ioDictEntry.mLength = inResReader.ReadUInt32();
	}

	private static void TableArrayResEntryAssign(ResourceReadStream inResReader, ref UtTableArrayResEntry ioArrayEntry)
	{
		ioArrayEntry.mFlags = inResReader.ReadUInt32();
		ioArrayEntry.mType = inResReader.ReadUInt32();
		ioArrayEntry.mLength = inResReader.ReadUInt32();
		ioArrayEntry.mElementCount = inResReader.ReadUInt32();
	}
}
