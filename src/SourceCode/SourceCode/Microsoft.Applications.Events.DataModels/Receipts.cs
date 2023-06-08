using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Receipts
{
	public long originalTime { get; set; }

	public long uploadTime { get; set; }

	public Receipts()
		: this("Microsoft.Applications.Events.DataModels.Bond.Receipts", "Receipts")
	{
	}

	protected Receipts(string fullName, string name)
	{
	}
}
