using System.CodeDom.Compiler;

namespace Microsoft.Applications.Events.DataModels;

[GeneratedCode("gbc", "0.10.0.0")]
internal class User
{
	public string id { get; set; }

	public string localId { get; set; }

	public string authId { get; set; }

	public string locale { get; set; }

	public User()
		: this("Microsoft.Applications.Events.DataModels.Bond.User", "User")
	{
	}

	protected User(string fullName, string name)
	{
	}
}
