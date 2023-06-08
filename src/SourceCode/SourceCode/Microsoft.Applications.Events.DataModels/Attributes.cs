using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Attributes
{
	public List<PII> pii { get; set; }

	public List<CustomerContent> customerContent { get; set; }

	public Attributes()
		: this("Microsoft.Applications.Events.DataModels.Bond.Attributes", "Attributes")
	{
	}

	protected Attributes(string fullName, string name)
	{
	}

	internal void Release()
	{
		customerContent = null;
	}
}
