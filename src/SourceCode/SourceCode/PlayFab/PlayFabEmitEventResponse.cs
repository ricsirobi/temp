using System.Collections.Generic;
using PlayFab.EventsModels;

namespace PlayFab;

public class PlayFabEmitEventResponse : IPlayFabEmitEventResponse
{
	public PlayFabEvent Event { get; set; }

	public EmitEventResult EmitEventResult { get; set; }

	public PlayFabError PlayFabError { get; set; }

	public WriteEventsResponse WriteEventsResponse { get; set; }

	public IList<IPlayFabEmitEventRequest> Batch { get; set; }

	private ulong BatchNumber { get; set; }
}
