using System.Xml.Serialization;

[XmlRoot(ElementName = "NameValidationResponse", Namespace = "")]
public class NameValidationResponse
{
	[XmlElement(ElementName = "ErrorMessage")]
	public string ErrorMessage;

	[XmlElement(ElementName = "Category")]
	public NameValidationResult Result;
}
