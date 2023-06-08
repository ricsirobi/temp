using System;

namespace PlayFab;

[Serializable]
public sealed class AuthKeys
{
	public AuthTypes AuthType;

	public string AuthTicket;

	public string OpenIdConnectionId;

	public string WindowsHelloChallengeSignature;

	public string WindowsHelloPublicKeyHint;
}
