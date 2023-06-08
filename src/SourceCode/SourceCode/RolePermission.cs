using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "RP", IsNullable = true)]
public class RolePermission
{
	[XmlElement(ElementName = "G")]
	public GroupType GroupType;

	[XmlElement(ElementName = "R")]
	public UserRole Role;

	[XmlElement(ElementName = "P")]
	public List<string> Permissions;
}
