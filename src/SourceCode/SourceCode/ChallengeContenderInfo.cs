using System;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "ChallengeContenderInfo", IsNullable = false)]
public class ChallengeContenderInfo
{
	[XmlElement(ElementName = "UserId")]
	public Guid UserID;

	[XmlElement(ElementName = "ChallengeID")]
	public int ChallengeID;

	[XmlElement(ElementName = "Points")]
	public int Points;

	[XmlElement(ElementName = "ChallengeState")]
	public ChallengeState ChallengeState;

	[XmlElement(ElementName = "ExpirationDate")]
	public DateTime ExpirationDate;
}
