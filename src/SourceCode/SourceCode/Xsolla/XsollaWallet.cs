using System.Text;
using UnityEngine;

namespace Xsolla;

public class XsollaWallet : MonoBehaviour
{
	public static class Factory
	{
		public static XsollaWallet CreateWallet(string access_token)
		{
			return new XsollaWallet(access_token);
		}
	}

	private string token;

	private string userId;

	private string userName;

	private string userEmail;

	private string userCountry;

	private long projectId;

	private string language;

	private string currency;

	private XsollaWallet(string token)
	{
		this.token = token;
	}

	public string GetToken()
	{
		return token;
	}

	public string GetPrepared()
	{
		if (token != null)
		{
			return token;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{").Append("\"user\":{").Append("\"id\":{")
			.Append("\"value\":")
			.Append("")
			.Append("}")
			.Append("\"email\":{")
			.Append("\"value\":")
			.Append("")
			.Append("}")
			.Append("\"country\":{")
			.Append("\"value\":")
			.Append("")
			.Append("}")
			.Append("}")
			.Append("\"settings\":{")
			.Append("\"project_id\":")
			.Append("")
			.Append("\"language\":")
			.Append("")
			.Append("\"currency\":")
			.Append("")
			.Append("\"mode\":")
			.Append("")
			.Append("}")
			.Append("}");
		return stringBuilder.ToString();
	}
}
