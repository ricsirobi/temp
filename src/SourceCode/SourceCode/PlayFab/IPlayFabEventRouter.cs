using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab.Pipeline;

namespace PlayFab;

public interface IPlayFabEventRouter
{
	IDictionary<EventPipelineKey, IEventPipeline> Pipelines { get; }

	System.Threading.Tasks.Task AddAndStartPipeline(EventPipelineKey eventPipelineKey, IEventPipeline eventPipeline);

	IEnumerable<Task<IPlayFabEmitEventResponse>> RouteEvent(IPlayFabEmitEventRequest request);
}
