using System.IO;
using System.Xml.Serialization;

namespace CI.WSANative.Serialisers;

public static class WSANativeSerialisation
{
	public static string SerialiseToXML<T>(T item)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(item.GetType());
		using StringWriter stringWriter = new StringWriter();
		xmlSerializer.Serialize(stringWriter, item);
		return stringWriter.ToString();
	}

	public static T DeserialiseXML<T>(string xml)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using StringReader textReader = new StringReader(xml);
		return (T)xmlSerializer.Deserialize(textReader);
	}
}
