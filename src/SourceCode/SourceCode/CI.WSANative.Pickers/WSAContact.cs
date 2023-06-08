using System.Collections.Generic;

namespace CI.WSANative.Pickers;

public class WSAContact
{
	public string DisplayName { get; set; }

	public string FullName { get; set; }

	public string FirstName { get; set; }

	public string MiddleName { get; set; }

	public string LastName { get; set; }

	public string Nickname { get; set; }

	public IEnumerable<string> Emails { get; set; }

	public IEnumerable<string> Phones { get; set; }
}
