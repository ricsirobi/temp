using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "AssignRoleResult", IsNullable = true)]
public class AssignRoleResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "InitiatorNewRole", IsNullable = true)]
	public UserRole? InitiatorNewRole;

	[XmlElement(ElementName = "Status")]
	public AssignRoleStatus Status;
}
