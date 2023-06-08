using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Protocol
{
	public int metadataCrc { get; set; }

	public List<List<string>> ticketKeys { get; set; }

	public Protocol()
		: this("Microsoft.Applications.Events.DataModels.Bond.Protocol", "Protocol")
	{
	}

	protected Protocol(string fullName, string name)
	{
	}
}
