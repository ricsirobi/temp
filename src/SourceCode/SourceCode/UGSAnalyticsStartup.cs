using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UGSAnalyticsStartup : MonoBehaviour
{
	private string env;

	private async void Start()
	{
		_ = 1;
		try
		{
			InitializationOptions initializationOptions = new InitializationOptions();
			env = "production";
			initializationOptions.SetEnvironmentName(env);
			await UnityServices.InitializeAsync(initializationOptions);
			foreach (string item in await AnalyticsService.Instance.CheckForRequiredConsents())
			{
				_ = item;
			}
		}
		catch (ConsentCheckException ex)
		{
			UtDebug.LogError($"Issue initializing UGS Analytics: {ex.Reason} || {ex.Message}");
		}
	}
}
