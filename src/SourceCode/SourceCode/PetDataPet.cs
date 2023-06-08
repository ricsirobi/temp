using System.Xml.Serialization;

[XmlRoot(ElementName = "PetDataPet", Namespace = "")]
public class PetDataPet
{
	public string Geometry;

	public string Texture;

	public string Type;

	public string Name;

	public float Dirtiness;

	public string AccessoryGeometry;

	public string AccessoryTexture;
}
