using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "EditGroupResult", IsNullable = true)]
public class EditGroupResult
{
	[XmlElement(ElementName = "Success")]
	public bool Success;

	[XmlElement(ElementName = "Status")]
	public EditGroupStatus Status;

	[XmlElement(ElementName = "NewRolePermissions", IsNullable = true)]
	public List<RolePermission> NewRolePermissions;
}
