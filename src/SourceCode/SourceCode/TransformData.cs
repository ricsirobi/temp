using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "TransformData", Namespace = "")]
public class TransformData
{
	[XmlElement(ElementName = "Position")]
	public Vector3 Position;

	[XmlElement(ElementName = "Rotation")]
	public Vector3 Rotation;

	[XmlElement(ElementName = "Scale")]
	public Vector3 Scale;
}
