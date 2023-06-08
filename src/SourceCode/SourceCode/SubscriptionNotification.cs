using System.Xml.Serialization;

public class SubscriptionNotification
{
	[XmlElement(ElementName = "Type")]
	public SubscriptionNotificationType Type;
}
