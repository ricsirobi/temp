using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Data
{
	public Dictionary<string, Value> properties { get; set; }

	public Data()
		: this("Microsoft.Applications.Events.DataModels.Data", "Data")
	{
	}

	protected Data(string fullName, string name)
	{
		properties = new Dictionary<string, Value>();
	}

	internal void Release()
	{
		if (properties != null)
		{
			foreach (KeyValuePair<string, Value> property in properties)
			{
				property.Value.Release();
			}
		}
		properties?.Clear();
		properties = null;
	}
}
