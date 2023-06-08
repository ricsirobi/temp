using PlayFab.SharedModels;

namespace PlayFab.EventsModels;

public class TelemetryIngestionConfigResponse : PlayFabResultCommon
{
	public string IngestionKey;

	public string TelemetryJwtHeaderKey;

	public string TelemetryJwtHeaderPrefix;

	public string TelemetryJwtToken;

	public string TenantId;
}
