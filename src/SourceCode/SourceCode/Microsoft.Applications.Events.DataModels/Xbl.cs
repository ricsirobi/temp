using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Xbl
{
	public Dictionary<string, string> claims { get; set; }

	public string nbf { get; set; }

	public string exp { get; set; }

	public string sbx { get; set; }

	public string dty { get; set; }

	public string did { get; set; }

	public string xid { get; set; }

	public ulong uts { get; set; }

	public string pid { get; set; }

	public string dvr { get; set; }

	public uint tid { get; set; }

	public string tvr { get; set; }

	public string sty { get; set; }

	public string sid { get; set; }

	public long? eid { get; set; }

	public string ip { get; set; }

	public Xbl()
		: this("Microsoft.Applications.Events.DataModels.Bond.Xbl", "Xbl")
	{
	}

	protected Xbl(string fullName, string name)
	{
	}
}
