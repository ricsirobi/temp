using System.Threading.Tasks;

namespace PlayFab;

public class PlayFabEmitEventRequest : IPlayFabEmitEventRequest
{
	public PlayFabEvent Event { get; set; }

	public string TitleId { get; set; } = "default";


	public TaskCompletionSource<IPlayFabEmitEventResponse> ResultPromise { get; set; }
}
