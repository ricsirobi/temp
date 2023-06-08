using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Cloud
{
	public string fullEnvName { get; set; }

	public string location { get; set; }

	public string environment { get; set; }

	public string deploymentUnit { get; set; }

	public string name { get; set; }

	public string roleInstance { get; set; }

	public string role { get; set; }
}
