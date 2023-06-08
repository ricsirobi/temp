using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Value
{
	public ValueKind type { get; set; }

	public List<Attributes> attributes { get; set; }

	public string stringValue { get; set; }

	public long longValue { get; set; }

	public double doubleValue { get; set; }

	public List<List<byte>> guidValue { get; set; }

	public List<List<string>> stringArray { get; set; }

	public List<List<long>> longArray { get; set; }

	public List<List<double>> doubleArray { get; set; }

	public List<List<List<long>>> guidArray { get; set; }

	public Value()
		: this("Microsoft.Applications.Events.DataModels.Bond.Value", "Value")
	{
	}

	protected Value(string fullName, string name)
	{
		type = ValueKind.ValueString;
	}

	internal void Release()
	{
		if (attributes != null)
		{
			foreach (Attributes attribute in attributes)
			{
				attribute.Release();
			}
		}
		attributes = null;
		guidValue = null;
		stringArray = null;
		longArray = null;
		doubleArray = null;
		guidArray = null;
	}
}
