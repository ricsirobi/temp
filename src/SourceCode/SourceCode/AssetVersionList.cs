using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AVL")]
public class AssetVersionList
{
	[XmlElement("A")]
	public AssetVersion[] AssetVersions;
}
