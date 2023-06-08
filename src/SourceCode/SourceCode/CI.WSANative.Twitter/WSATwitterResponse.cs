namespace CI.WSANative.Twitter;

public class WSATwitterResponse
{
	public bool Success { get; set; }

	public string Data { get; set; }

	public WSATwitterError Error { get; set; }
}
