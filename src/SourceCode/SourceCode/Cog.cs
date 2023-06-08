using System;
using System.Xml.Serialization;

[Serializable]
public class Cog
{
	[XmlElement(ElementName = "AssetName")]
	public string AssetName;

	[XmlElement(ElementName = "Asset")]
	public string Asset;

	[XmlElement(ElementName = "RatchetAttached")]
	public bool RatchetAttached;

	[XmlElement(ElementName = "CogType")]
	public CogType CogType;

	[XmlElement(ElementName = "RotateDirection")]
	public RotateDirection RotateDirection;

	[XmlElement(ElementName = "AngularSpeed")]
	public float AngularSpeed;

	[XmlElement(ElementName = "Transform")]
	public TransformData Transform;

	[XmlElement(ElementName = "Icon")]
	public string Icon;
}
