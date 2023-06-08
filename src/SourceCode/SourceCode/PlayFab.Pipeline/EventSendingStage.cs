using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlayFab.EventsModels;
using PlayFab.Logger;
using PlayFab.SharedModels;

namespace PlayFab.Pipeline;

internal class EventSendingStage : PipelineStageBase<TitleEventBatch, PlayFabResult<WriteEventsResponse>>
{
	private OneDSEventsAPI oneDSEventsApi;

	private ILogger logger;

	public EventSendingStage(ILogger logger)
	{
		this.logger = logger;
		oneDSEventsApi = new OneDSEventsAPI();
	}

	protected override void OnNextInputItem(TitleEventBatch batch)
	{
		WriteEventsRequest request = new WriteEventsRequest
		{
			Events = (from PlayFabEmitEventRequest x in batch.Events
				select x.Event.EventContents).ToList()
		};
		if (!oneDSEventsApi.IsOneDSAuthenticated)
		{
			Task<PlayFabResult<TelemetryIngestionConfigResponse>> telemetryIngestionConfigAsync = OneDSEventsAPI.GetTelemetryIngestionConfigAsync(new TelemetryIngestionConfigRequest());
			try
			{
				telemetryIngestionConfigAsync.Wait(cts.Token);
				TelemetryIngestionConfigResponse result = telemetryIngestionConfigAsync.Result.Result;
				if (result == null)
				{
					throw new Exception("Failed to get OneDS authentication token from PlayFab service");
				}
				oneDSEventsApi.SetCredentials("o:" + result.TenantId, result.IngestionKey, result.TelemetryJwtToken, result.TelemetryJwtHeaderKey, result.TelemetryJwtHeaderPrefix);
			}
			catch (Exception ex)
			{
				foreach (PlayFabEmitEventRequest @event in batch.Events)
				{
					@event.ResultPromise?.SetCanceled();
				}
				logger.Error($"Exception in OnNextInputItem {ex.Source} with message: {ex}.");
			}
		}
		Task<PlayFabResult<WriteEventsResponse>> task = oneDSEventsApi.WriteTelemetryEventsAsync(request);
		try
		{
			task.Wait(cts.Token);
			FulfillPromises(batch.Events, task.Result);
		}
		catch (Exception ex2)
		{
			foreach (PlayFabEmitEventRequest event2 in batch.Events)
			{
				event2.ResultPromise?.SetCanceled();
			}
			logger.Error($"Exception in OnNextInputItem {ex2.Source} with message: {ex2}. This was an unhandled exception, please contact the dev team.");
		}
	}

	private void FulfillPromises(List<IPlayFabEmitEventRequest> batch, PlayFabResult<WriteEventsResponse> playFabResult)
	{
		for (int i = 0; i < batch.Count; i++)
		{
			PlayFabEmitEventRequest playFabEmitEventRequest = (PlayFabEmitEventRequest)batch[i];
			if (playFabEmitEventRequest.ResultPromise != null)
			{
				PlayFabEmitEventResponse result = new PlayFabEmitEventResponse
				{
					Event = playFabEmitEventRequest.Event,
					EmitEventResult = EmitEventResult.Success,
					WriteEventsResponse = playFabResult.Result,
					PlayFabError = playFabResult.Error,
					Batch = batch
				};
				playFabEmitEventRequest.ResultPromise.SetResult(result);
			}
		}
	}
}
