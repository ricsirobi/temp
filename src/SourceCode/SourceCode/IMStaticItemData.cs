using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IMStaticItemData", Namespace = "")]
public class IMStaticItemData
{
	[XmlElement(ElementName = "AssetName")]
	public string AssetName;

	[XmlElement(ElementName = "Asset")]
	public string Asset;

	[XmlElement(ElementName = "Position")]
	public string Position;

	[XmlElement(ElementName = "Rotation")]
	public string Rotation;

	[XmlElement(ElementName = "Scale")]
	public string Scale;

	[XmlElement(ElementName = "PivotPosition")]
	public float? PivotPosition;

	[XmlElement(ElementName = "PivotRotation")]
	public float? PivotRotation;

	[XmlElement(ElementName = "AngleLimit")]
	public float? PivotAngleLimit;

	[XmlElement(ElementName = "IsKinematic")]
	public bool IsKinematic;

	[XmlElement(ElementName = "ObjectProperty")]
	public GoalManager.ObjectProperty ObjectProperty;
}
