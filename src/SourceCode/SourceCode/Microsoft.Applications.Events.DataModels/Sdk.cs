using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Sdk
{
	public string libVer { get; set; }

	public string epoch { get; set; }

	public long seq { get; set; }

	public string installId { get; set; }
}
