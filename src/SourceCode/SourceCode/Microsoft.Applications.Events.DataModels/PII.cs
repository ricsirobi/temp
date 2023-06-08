using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class PII
{
	public PIIKind Kind { get; set; }

	public PII()
		: this("Microsoft.Applications.Events.DataModels.Bond.PII", "PII")
	{
	}

	protected PII(string fullName, string name)
	{
		Kind = PIIKind.NotSet;
	}

	public static implicit operator List<object>(PII v)
	{
		throw new NotImplementedException();
	}
}
