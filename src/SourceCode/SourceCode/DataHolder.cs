using UnityEngine;

public class DataHolder
{
	public string _Name = "";

	public string _Size = "";

	public TextureFormat _Format = TextureFormat.BGRA32;

	public DataHolder()
	{
	}

	public DataHolder(string inName, string inSize, TextureFormat inTexFormat)
	{
		_Name = inName;
		_Size = inSize;
		_Format = inTexFormat;
	}
}
