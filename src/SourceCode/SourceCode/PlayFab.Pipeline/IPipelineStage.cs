using System.Collections.Concurrent;
using System.Threading;

namespace PlayFab.Pipeline;

internal interface IPipelineStage<TInputItem, TOutputItem>
{
	void RunStage(BlockingCollection<TInputItem> input, BlockingCollection<TOutputItem> output, CancellationTokenSource cts);
}
