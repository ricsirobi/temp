using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Serialization;

public class UtSerialization
{
	private sealed class DeserializationBinder : SerializationBinder
	{
		public override Type BindToType(string assemblyName, string typeName)
		{
			return Type.GetType(typeName + ", " + Assembly.GetExecutingAssembly().FullName);
		}
	}

	public static string GenerateXML<TYPE>(TYPE inObject)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter textWriter = new StreamWriter(memoryStream, uTF8Encoding);
		try
		{
			new XmlSerializer(typeof(TYPE)).Serialize(textWriter, inObject);
			return uTF8Encoding.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
		}
		catch (Exception ex)
		{
			UtDebug.LogError("ERROR: UtSerialization.GenerateXML failed to serailize object. Reason: " + ex.Message);
			return null;
		}
	}

	public static byte[] GenerateBinary<TYPE>(TYPE inObject)
	{
		byte[] result = null;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			try
			{
				new BinaryFormatter().Serialize(memoryStream, inObject);
				result = memoryStream.ToArray();
			}
			catch (Exception ex)
			{
				UtDebug.LogError("ERROR: UtSerialization.GenerateBinary failed to serailize object. Reason: " + ex.Message);
			}
		}
		return result;
	}

	public static TYPE DeserializeXML<TYPE>(string inText)
	{
		if (inText == null)
		{
			UtDebug.LogError("ERROR: UtSerialization.DeserializeXML failed, inText is null!!");
			return default(TYPE);
		}
		using StringReader textReader = new StringReader(inText);
		try
		{
			return (TYPE)new XmlSerializer(typeof(TYPE)).Deserialize(textReader);
		}
		catch (Exception ex)
		{
			UtDebug.LogError("ERROR: UtSerialization.DeserializeXML failed to deserialize text stream. Reason: " + ex.Message);
			return default(TYPE);
		}
	}

	public static TYPE DeserializeBinary<TYPE>(byte[] inBytes)
	{
		if (inBytes == null)
		{
			UtDebug.LogError("ERROR: UtSerialization.DeserializeBinary failed, inBytes is null!!");
			return default(TYPE);
		}
		using MemoryStream serializationStream = new MemoryStream(inBytes);
		try
		{
			return (TYPE)new BinaryFormatter
			{
				Binder = new DeserializationBinder()
			}.Deserialize(serializationStream);
		}
		catch (Exception ex)
		{
			UtDebug.LogError("ERROR: UtSerialization.DeserializeBinary failed to deserialize byte stream. Reason: " + ex.Message);
			return default(TYPE);
		}
	}
}
