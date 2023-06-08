using System;

namespace CI.WSANative.FileStorage;

public class WSAStorageFile
{
	public DateTimeOffset DateCreated { get; set; }

	public string DisplayName { get; set; }

	public string FileType { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public string DisplayType { get; set; }

	public bool IsAvailable { get; set; }

	public byte[] ReadBytes()
	{
		return new byte[0];
	}

	public string ReadText()
	{
		return string.Empty;
	}

	public void WriteBytes(byte[] bytes)
	{
	}

	public void WriteText(string text)
	{
	}
}
