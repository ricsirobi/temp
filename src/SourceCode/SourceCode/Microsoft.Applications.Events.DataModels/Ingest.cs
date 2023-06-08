using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Ingest
{
	public long time { get; set; }

	public string clientIp { get; set; }

	public long auth { get; set; }

	public long quality { get; set; }

	public long uploadTime { get; set; }

	public string userAgent { get; set; }

	public string client { get; set; }

	public Ingest()
		: this("Microsoft.Applications.Events.DataModels.Bond.Ingest", "Ingest")
	{
	}

	protected Ingest(string fullName, string name)
	{
		clientIp = "";
	}
}
