using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab.Logger;
using PlayFab.Pipeline;

namespace PlayFab;

public class PlayFabEventRouter : IPlayFabEventRouter
{
	private ILogger logger;

	public IDictionary<EventPipelineKey, IEventPipeline> Pipelines { get; private set; }

	public PlayFabEventRouter(ILogger logger = null)
	{
		if (logger == null)
		{
			logger = new DebugLogger();
		}
		this.logger = logger;
		Pipelines = new Dictionary<EventPipelineKey, IEventPipeline>();
	}

	public System.Threading.Tasks.Task AddAndStartPipeline(EventPipelineKey eventPipelineKey, IEventPipeline eventPipeline)
	{
		Pipelines.Add(eventPipelineKey, eventPipeline);
		return eventPipeline.StartAsync();
	}

	public IEnumerable<Task<IPlayFabEmitEventResponse>> RouteEvent(IPlayFabEmitEventRequest request)
	{
		List<Task<IPlayFabEmitEventResponse>> list = new List<Task<IPlayFabEmitEventResponse>>();
		if (!(request is PlayFabEmitEventRequest playFabEmitEventRequest) || playFabEmitEventRequest.Event == null)
		{
			return list;
		}
		foreach (KeyValuePair<EventPipelineKey, IEventPipeline> pipeline in Pipelines)
		{
			PlayFabEventType eventType = playFabEmitEventRequest.Event.EventType;
			if ((uint)eventType <= 1u)
			{
				if (pipeline.Key == EventPipelineKey.OneDS)
				{
					list.Add(pipeline.Value.IntakeEventAsync(request));
				}
			}
			else
			{
				logger.Error($"Not supported event type {playFabEmitEventRequest.Event.EventType}.");
			}
		}
		return list;
	}
}
