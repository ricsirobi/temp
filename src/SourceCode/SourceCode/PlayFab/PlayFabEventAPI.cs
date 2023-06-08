using System.Collections.Generic;
using System.Threading.Tasks;
using PlayFab.Logger;

namespace PlayFab;

public class PlayFabEventAPI
{
	public IPlayFabEventRouter EventRouter { get; private set; }

	public PlayFabEventAPI(ILogger logger = null)
	{
		if (logger == null)
		{
			logger = new DebugLogger();
		}
		EventRouter = new PlayFabEventRouter(logger);
	}

	public IEnumerable<Task<IPlayFabEmitEventResponse>> EmitEvent(IPlayFabEvent playFabEvent)
	{
		PlayFabEmitEventRequest request = new PlayFabEmitEventRequest
		{
			Event = (playFabEvent as PlayFabEvent)
		};
		return EventRouter.RouteEvent(request);
	}
}
