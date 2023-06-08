using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class CsEvent
{
	public string ver { get; set; }

	public string name { get; set; }

	public long time { get; set; }

	public double popSample { get; set; }

	public string iKey { get; set; }

	public long flags { get; set; }

	public string cV { get; set; }

	public List<Ingest> extIngest { get; set; }

	public List<Protocol> extProtocol { get; set; }

	public List<User> extUser { get; set; }

	public List<Device> extDevice { get; set; }

	public List<Os> extOs { get; set; }

	public List<App> extApp { get; set; }

	public List<Utc> extUtc { get; set; }

	public List<Xbl> extXbl { get; set; }

	public List<Javascript> extJavascript { get; set; }

	public List<Receipts> extReceipts { get; set; }

	public List<Net> extNet { get; set; }

	public List<Sdk> extSdk { get; set; }

	public List<Loc> extLoc { get; set; }

	public List<Cloud> extCloud { get; set; }

	public List<Data> ext { get; set; }

	public Dictionary<string, string> tags { get; set; }

	public string baseType { get; set; }

	public List<Data> baseData { get; set; }

	public List<Data> data { get; set; }

	public CsEvent()
		: this("Microsoft.Applications.Events.DataModels.CsEvent", "CsEvent")
	{
	}

	protected CsEvent(string fullName, string name)
	{
		ver = "";
		this.name = "";
		popSample = 100.0;
		tags = new Dictionary<string, string>();
	}

	internal void Reset()
	{
		if (ext != null)
		{
			foreach (Data item in ext)
			{
				item.Release();
			}
		}
		if (data != null)
		{
			foreach (Data datum in data)
			{
				datum.Release();
			}
		}
		if (baseData != null)
		{
			foreach (Data baseDatum in baseData)
			{
				baseDatum.Release();
			}
		}
		ver = string.Empty;
		name = string.Empty;
		time = 0L;
		popSample = 0.0;
		iKey = string.Empty;
		flags = 0L;
		cV = string.Empty;
		ext?.Clear();
		extIngest?.Clear();
		extProtocol?.Clear();
		extUser?.Clear();
		extDevice?.Clear();
		extOs?.Clear();
		extApp?.Clear();
		extUtc?.Clear();
		extNet?.Clear();
		extDevice?.Clear();
		extCloud?.Clear();
		extLoc?.Clear();
		extXbl?.Clear();
		extSdk?.Clear();
		extJavascript?.Clear();
		extReceipts?.Clear();
		ext?.Clear();
		tags?.Clear();
		baseType = string.Empty;
		baseData?.Clear();
		data?.Clear();
		ext = null;
		extIngest = null;
		extProtocol = null;
		extUser = null;
		extDevice = null;
		extOs = null;
		extApp = null;
		extUtc = null;
		extNet = null;
		extDevice = null;
		extCloud = null;
		extLoc = null;
		extXbl = null;
		extSdk = null;
		extJavascript = null;
		extReceipts = null;
		ext = null;
		tags = null;
		baseType = string.Empty;
		baseData = null;
		data = null;
	}
}
