using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Applications.Events;

internal class CompactBinaryProtocolWriter
{
	private List<byte> _output;

	public List<byte> Data => _output;

	public CompactBinaryProtocolWriter()
	{
		_output = new List<byte>();
	}

	public CompactBinaryProtocolWriter(List<byte> output)
	{
		_output = output;
	}

	public void WriteStructBegin(object nullptr, bool isBase)
	{
	}

	public void writeVarint(ushort value)
	{
		while (value > 127)
		{
			_output.Add((byte)((value & 0x7Fu) | 0x80u));
			value = (ushort)(value >> 7);
		}
		_output.Add((byte)(value & 0x7Fu));
	}

	public void writeVarint(short value)
	{
		while (value > 127)
		{
			_output.Add((byte)(((uint)value & 0x7Fu) | 0x80u));
			value = (short)(value >> 7);
		}
		_output.Add((byte)((uint)value & 0x7Fu));
	}

	public void writeVarint(int value)
	{
		while (value > 127)
		{
			_output.Add((byte)(((uint)value & 0x7Fu) | 0x80u));
			value >>= 7;
		}
		_output.Add((byte)((uint)value & 0x7Fu));
	}

	public void writeVarint(uint value)
	{
		while (value > 127)
		{
			_output.Add((byte)((value & 0x7Fu) | 0x80u));
			value >>= 7;
		}
		_output.Add((byte)(value & 0x7Fu));
	}

	public void writeVarint(long value)
	{
		while (value > 127)
		{
			_output.Add((byte)((value & 0x7F) | 0x80));
			value >>= 7;
		}
		_output.Add((byte)(value & 0x7F));
	}

	public void writeVarint(ulong value)
	{
		while (value > 127)
		{
			_output.Add((byte)((value & 0x7F) | 0x80));
			value >>= 7;
		}
		_output.Add((byte)(value & 0x7F));
	}

	public void WriteBlob(List<byte> data, int size)
	{
		_output.AddRange(data);
	}

	public void WriteBlob(byte[] data, int size)
	{
		_output.AddRange(data);
	}

	public void WriteBool(bool value)
	{
		_output.Add((byte)(value ? 1u : 0u));
	}

	public void WriteUInt8(byte value)
	{
		_output.Add(value);
	}

	public void WriteUInt16(ushort value)
	{
		writeVarint(value);
	}

	public void WriteUInt32(uint value)
	{
		writeVarint(value);
	}

	public void WriteUInt64(ulong value)
	{
		writeVarint(value);
	}

	public void WriteInt8(sbyte value)
	{
		byte value2 = (byte)value;
		WriteUInt8(value2);
	}

	public void WriteInt16(short value)
	{
		ushort value2 = (ushort)((value << 1) ^ (value >> 15));
		WriteUInt16(value2);
	}

	public void WriteInt32(int value)
	{
		uint value2 = (uint)((value << 1) ^ (value >> 31));
		WriteUInt32(value2);
	}

	public void WriteInt64(long value)
	{
		ulong value2 = (ulong)((value << 1) ^ (value >> 63));
		WriteUInt64(value2);
	}

	public void WriteDouble(double value)
	{
		WriteBlob(BitConverter.GetBytes(value), 8);
	}

	public void WriteString(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			WriteUInt32(0u);
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		WriteUInt32((uint)bytes.Length);
		WriteBlob(bytes, bytes.Length);
	}

	public void WriteWString(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			WriteUInt32(0u);
			return;
		}
		byte[] bytes = Encoding.UTF8.GetBytes(value);
		WriteUInt32((uint)bytes.Length);
		WriteBlob(bytes, bytes.Length);
	}

	public void WriteContainerBegin(ushort size, byte elementType)
	{
		WriteUInt8(elementType);
		WriteUInt32(size);
	}

	public void WriteMapContainerBegin(ushort size, byte keyType, byte valueType)
	{
		WriteUInt8(keyType);
		WriteUInt8(valueType);
		WriteUInt32(size);
	}

	public void WriteContainerEnd()
	{
	}

	public void WriteFieldBegin(byte type, ushort id)
	{
		if (id <= 5)
		{
			_output.Add((byte)(type | ((byte)id << 5)));
		}
		else if (id <= 255)
		{
			_output.Add((byte)(type | 0xC0u));
			_output.Add((byte)(id & 0xFFu));
		}
		else
		{
			_output.Add((byte)(type | 0xE0u));
			_output.Add((byte)(id & 0xFFu));
			_output.Add((byte)(id >> 8));
		}
	}

	public void WriteFieldEnd()
	{
	}

	public void WriteStructEnd(bool isBase)
	{
		WriteUInt8((byte)(isBase ? 1u : 0u));
	}

	internal void WriteFieldOmitted(byte bT_STRING, int v)
	{
	}
}
