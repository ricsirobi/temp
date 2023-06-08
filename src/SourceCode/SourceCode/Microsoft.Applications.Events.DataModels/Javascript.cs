using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class Javascript
{
	public string libVer { get; set; }

	public string osName { get; set; }

	public string browser { get; set; }

	public string browserVersion { get; set; }

	public string platform { get; set; }

	public string make { get; set; }

	public string model { get; set; }

	public string screenSize { get; set; }

	public string mc1Id { get; set; }

	public ulong mc1Lu { get; set; }

	public bool isMc1New { get; set; }

	public string ms0 { get; set; }

	public string anid { get; set; }

	public string a { get; set; }

	public string msResearch { get; set; }

	public string csrvc { get; set; }

	public string rtCell { get; set; }

	public string rtEndAction { get; set; }

	public string rtPermId { get; set; }

	public string r { get; set; }

	public string wtFpc { get; set; }

	public string omniId { get; set; }

	public string gsfxSession { get; set; }

	public string domain { get; set; }

	public string dnt { get; set; }

	public Javascript()
		: this("Microsoft.Applications.Events.DataModels.Bond.Javascript", "Javascript")
	{
	}

	protected Javascript(string fullName, string name)
	{
		libVer = "";
		osName = "";
		browser = "";
		browserVersion = "";
		platform = "";
		make = "";
		model = "";
		screenSize = "";
		mc1Id = "";
		ms0 = "";
		anid = "";
		a = "";
		msResearch = "";
		csrvc = "";
		rtCell = "";
		rtEndAction = "";
		rtPermId = "";
		r = "";
		wtFpc = "";
		omniId = "";
		gsfxSession = "";
		domain = "";
		dnt = "";
	}
}
