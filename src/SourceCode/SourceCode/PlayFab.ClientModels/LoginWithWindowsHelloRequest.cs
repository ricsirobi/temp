using System;
using PlayFab.SharedModels;

namespace PlayFab.ClientModels;

[Serializable]
public class LoginWithWindowsHelloRequest : PlayFabRequestCommon
{
	public string ChallengeSignature;

	public GetPlayerCombinedInfoRequestParams InfoRequestParameters;

	public string PublicKeyHint;

	public string TitleId;
}
