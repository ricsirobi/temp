using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "GetGroupsResult", IsNullable = true)]
public class GetGroupsResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Groups")]
	public Group[] Groups;

	[XmlElement(ElementName = "RolePermissions", IsNullable = true)]
	public List<RolePermission> RolePermissions;
}
