using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class CustomerContent
{
	public CustomerContentKind Kind { get; set; }

	public CustomerContent()
		: this("Microsoft.Applications.Events.DataModels.Bond.CustomerContent", "CustomerContent")
	{
	}

	protected CustomerContent(string fullName, string name)
	{
		Kind = CustomerContentKind.NotSet;
	}
}
