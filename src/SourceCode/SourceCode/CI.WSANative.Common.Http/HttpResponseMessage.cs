using System.Net;

namespace CI.WSANative.Common.Http;

public class HttpResponseMessage
{
	public string Data { get; set; }

	public HttpStatusCode StatusCode { get; set; }

	public bool IsSuccessStatusCode
	{
		get
		{
			if (StatusCode >= HttpStatusCode.OK)
			{
				return StatusCode <= (HttpStatusCode)299;
			}
			return false;
		}
	}
}
