using System.Collections.Generic;
using Zendesk.UI;

public class ZendeskHtmlComponent
{
	public ZendeskHtmlComponentType zendeskHtmlComponentType;

	public string text;

	public string link;

	public Dictionary<ZendeskHtmlAttributeType, string> attributes;

	public ZendeskHtmlComponent(ZendeskHtmlComponentType zendeskHtmlComponentType, string text, string link, Dictionary<ZendeskHtmlAttributeType, string> attributes = null)
	{
		this.zendeskHtmlComponentType = zendeskHtmlComponentType;
		this.text = text;
		this.link = link;
		this.attributes = attributes;
	}
}
