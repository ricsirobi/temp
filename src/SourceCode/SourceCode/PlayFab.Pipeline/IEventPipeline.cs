using System.Threading.Tasks;

namespace PlayFab.Pipeline;

public interface IEventPipeline
{
	System.Threading.Tasks.Task StartAsync();

	bool IntakeEvent(IPlayFabEmitEventRequest request);

	Task<IPlayFabEmitEventResponse> IntakeEventAsync(IPlayFabEmitEventRequest request);
}
