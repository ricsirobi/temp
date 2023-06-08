using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace SerializableDictionary;

[XmlRoot(ElementName = "KeyValuePair")]
public class KeyValuePair<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
{
	public void ReadXml(XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			return;
		}
		object obj = default(TKey);
		object obj2 = default(TValue);
		while (reader.Read())
		{
			if (reader.NodeType != XmlNodeType.Element)
			{
				continue;
			}
			XmlReader xmlReader = reader.ReadSubtree();
			while (xmlReader.Read())
			{
				if (xmlReader.NodeType == XmlNodeType.Element || xmlReader.NodeType == XmlNodeType.Text || xmlReader.NodeType == XmlNodeType.EndElement)
				{
					if (xmlReader.Name == "Key")
					{
						obj = xmlReader.ReadElementContentAsObject();
					}
					if (xmlReader.Name == "Value")
					{
						obj2 = xmlReader.ReadElementContentAsObject();
					}
				}
			}
		}
		Add((TKey)obj, (TValue)obj2);
	}

	public void WriteXml(XmlWriter writer)
	{
		foreach (TKey key in base.Keys)
		{
			writer.WriteElementString("Key", key.ToString());
			writer.WriteElementString("Value", base[key].ToString());
		}
	}

	public XmlSchema GetSchema()
	{
		throw new NotImplementedException();
	}
}
