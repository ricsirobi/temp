using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class App
{
	public string expId { get; set; }

	public string userId { get; set; }

	public string env { get; set; }

	public int asId { get; set; }

	public string id { get; set; }

	public string ver { get; set; }

	public string locale { get; set; }

	public string name { get; set; }

	public App()
		: this("Microsoft.Applications.Events.DataModels.Bond.App", "App")
	{
	}

	protected App(string fullName, string name)
	{
	}
}
