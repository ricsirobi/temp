using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace PlayFab.Pipeline;

internal abstract class PipelineStageBase<TInputItem, TOutputItem> : IPipelineStage<TInputItem, TOutputItem>
{
	protected CancellationTokenSource cts;

	protected BlockingCollection<TInputItem> input;

	protected BlockingCollection<TOutputItem> output;

	protected virtual void InitStage()
	{
	}

	public virtual void RunStage(BlockingCollection<TInputItem> input, BlockingCollection<TOutputItem> output, CancellationTokenSource cts)
	{
		this.cts = cts;
		this.input = input;
		this.output = output;
		try
		{
			CancellationToken token = cts.Token;
			IEnumerable<TInputItem> inputConsumingEnumerable = GetInputConsumingEnumerable();
			InitStage();
			foreach (TInputItem item in inputConsumingEnumerable)
			{
				if (!token.IsCancellationRequested)
				{
					OnNextInputItem(item);
					continue;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			cts.Cancel();
			if (!(ex is OperationCanceledException))
			{
				throw;
			}
		}
		finally
		{
			output.CompleteAdding();
		}
	}

	protected virtual IEnumerable<TInputItem> GetInputConsumingEnumerable()
	{
		return input.GetConsumingEnumerable();
	}

	protected virtual void StoreOutput(TOutputItem outputItem)
	{
		output.Add(outputItem, cts.Token);
	}

	protected abstract void OnNextInputItem(TInputItem inputItem);
}
