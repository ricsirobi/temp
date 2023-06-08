using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Device
{
	public string id { get; set; }

	public string localId { get; set; }

	public string authId { get; set; }

	public string authSecId { get; set; }

	public string deviceClass { get; set; }

	public string orgId { get; set; }

	public string make { get; set; }

	public string orgAuthId { get; set; }

	public string model { get; set; }

	public Device()
		: this("Microsoft.Applications.Events.DataModels.Bond.Device", "Device")
	{
	}

	protected Device(string fullName, string name)
	{
	}
}
