using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "US", Namespace = "")]
public class UserStaff
{
	[XmlElement(ElementName = "id", IsNullable = true)]
	public int? UserStaffID;

	[XmlElement(ElementName = "uid", IsNullable = true)]
	public Guid? UserID;

	[XmlElement(ElementName = "suid", IsNullable = true)]
	public Guid? StaffUserID;

	[XmlElement(ElementName = "jid", IsNullable = true)]
	public int? JobTypeID;

	[XmlElement(ElementName = "uipid", IsNullable = true)]
	public int? AssociatedUserItemPositionID;

	[XmlElement(ElementName = "r", IsNullable = true)]
	public int? RankHiredAt;

	[XmlElement(ElementName = "ca", IsNullable = true)]
	public bool? IsCurrentlyUnavailable;

	[XmlElement(ElementName = "urid", IsNullable = true)]
	public int? UnavailableReasonID;

	[XmlElement(ElementName = "ad", IsNullable = true)]
	public DateTime? AvailableDate;

	[XmlElement(ElementName = "sd", IsNullable = true)]
	public DateTime? LastSpecialDate;

	[XmlElement(ElementName = "att", IsNullable = true)]
	public UserStaffAttribute[] Attributes;

	public string DefaultID => GetAttribute<string>("DID", null);

	public string FacebookID => GetAttribute<string>("FID", null);

	public UserStaff()
	{
		Attributes = new UserStaffAttribute[0];
	}

	public TYPE GetAttribute<TYPE>(string attribute, TYPE defaultValue)
	{
		if (Attributes == null)
		{
			return defaultValue;
		}
		UserStaffAttribute[] attributes = Attributes;
		foreach (UserStaffAttribute userStaffAttribute in attributes)
		{
			if (!(userStaffAttribute.AttributeKey == attribute))
			{
				continue;
			}
			Type typeFromHandle = typeof(TYPE);
			if (typeFromHandle.Equals(typeof(int)))
			{
				return (TYPE)(object)int.Parse(userStaffAttribute.AttributeValue);
			}
			if (typeFromHandle.Equals(typeof(float)))
			{
				return (TYPE)(object)float.Parse(userStaffAttribute.AttributeValue);
			}
			if (typeFromHandle.Equals(typeof(bool)))
			{
				if (userStaffAttribute.AttributeValue.Equals("t", StringComparison.OrdinalIgnoreCase) || userStaffAttribute.AttributeValue.Equals("1", StringComparison.OrdinalIgnoreCase) || userStaffAttribute.AttributeValue.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)true;
				}
				if (userStaffAttribute.AttributeValue.Equals("f", StringComparison.OrdinalIgnoreCase) || userStaffAttribute.AttributeValue.Equals("0", StringComparison.OrdinalIgnoreCase) || userStaffAttribute.AttributeValue.Equals("false", StringComparison.OrdinalIgnoreCase))
				{
					return (TYPE)(object)false;
				}
			}
			else
			{
				if (typeFromHandle.Equals(typeof(string)))
				{
					return (TYPE)(object)userStaffAttribute.AttributeValue;
				}
				if (typeFromHandle.Equals(typeof(DateTime)))
				{
					return (TYPE)(object)DateTime.Parse(userStaffAttribute.AttributeValue, UtUtilities.GetCultureInfo("en-US"));
				}
			}
		}
		return defaultValue;
	}

	public void SetAttribute(string key, string value)
	{
		if (Attributes == null)
		{
			Attributes = new UserStaffAttribute[1];
			Attributes[0] = new UserStaffAttribute();
			Attributes[0].AttributeKey = key;
			Attributes[0].AttributeValue = value;
			return;
		}
		UserStaffAttribute[] attributes = Attributes;
		foreach (UserStaffAttribute userStaffAttribute in attributes)
		{
			if (userStaffAttribute.AttributeKey == key)
			{
				userStaffAttribute.AttributeValue = value;
				return;
			}
		}
		List<UserStaffAttribute> list = new List<UserStaffAttribute>(Attributes);
		UserStaffAttribute userStaffAttribute2 = new UserStaffAttribute();
		userStaffAttribute2.AttributeKey = key;
		userStaffAttribute2.AttributeValue = value;
		list.Add(userStaffAttribute2);
		Attributes = list.ToArray();
	}
}
