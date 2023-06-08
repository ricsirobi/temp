using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Os
{
	public string locale { get; set; }

	public string expId { get; set; }

	public int bootId { get; set; }

	public string name { get; set; }

	public string ver { get; set; }

	public Os()
		: this("Microsoft.Applications.Events.DataModels.Bond.Os", "Os")
	{
	}

	protected Os(string fullName, string name)
	{
	}
}
