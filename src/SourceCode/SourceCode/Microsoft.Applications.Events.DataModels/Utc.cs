using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Utc
{
	public string stId { get; set; }

	public string aId { get; set; }

	public string raId { get; set; }

	public string op { get; set; }

	public long cat { get; set; }

	public long flags { get; set; }

	public string sqmId { get; set; }

	public string mon { get; set; }

	public int cpId { get; set; }

	public string bSeq { get; set; }

	public string epoch { get; set; }

	public long seq { get; set; }

	public Utc()
		: this("Microsoft.Applications.Events.DataModels.Bond.Utc", "Utc")
	{
	}

	protected Utc(string fullName, string name)
	{
	}
}
