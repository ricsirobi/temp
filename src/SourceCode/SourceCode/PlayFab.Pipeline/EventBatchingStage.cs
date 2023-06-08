using System;
using System.Collections.Generic;
using System.Diagnostics;
using PlayFab.Logger;

namespace PlayFab.Pipeline;

internal class EventBatchingStage : PipelineStageBase<IPlayFabEmitEventRequest, TitleEventBatch>
{
	public const int DefaultBatchSize = 10;

	private const int MaxPayloadSizeBytes = 1024;

	private const int PFGenericErrorCodeOverLimit = 1214;

	public static readonly TimeSpan DefaultBatchFillTimeout = TimeSpan.FromSeconds(5.0);

	private static readonly int EventAvailabilityCheckTimeoutInMs = 100;

	private Dictionary<string, List<IPlayFabEmitEventRequest>> batches;

	private Stopwatch stopwatch;

	private ILogger logger;

	private int BatchSize { get; set; }

	private TimeSpan BatchFillTimeout { get; set; }

	public EventBatchingStage(ILogger logger)
		: this(10, DefaultBatchFillTimeout, logger)
	{
	}

	public EventBatchingStage(int batchSize, TimeSpan batchFillTimeout, ILogger logger)
	{
		this.logger = logger;
		BatchSize = batchSize;
		BatchFillTimeout = batchFillTimeout;
		stopwatch = new Stopwatch();
		batches = new Dictionary<string, List<IPlayFabEmitEventRequest>>();
	}

	protected override void OnNextInputItem(IPlayFabEmitEventRequest request)
	{
		PlayFabEmitEventRequest playFabEmitEventRequest = (PlayFabEmitEventRequest)request;
		if (ValidationCheck(playFabEmitEventRequest))
		{
			string titleId = playFabEmitEventRequest.TitleId;
			if (string.IsNullOrWhiteSpace(titleId))
			{
				logger.Error("Event " + playFabEmitEventRequest.Event.Name + " has null or empty title id");
				return;
			}
			InitNewBatch(titleId);
			batches[titleId].Add(playFabEmitEventRequest);
			if (batches[titleId].Count == 1)
			{
				stopwatch.Restart();
			}
			if (batches[titleId].Count >= BatchSize)
			{
				StoreBatch(titleId);
				CreateNewBatch(titleId);
				stopwatch.Reset();
			}
		}
		else
		{
			logger.Error("Event " + playFabEmitEventRequest.Event.Name + " failed validation check and was ignored");
		}
	}

	protected override IEnumerable<IPlayFabEmitEventRequest> GetInputConsumingEnumerable()
	{
		while (!input.IsCompleted)
		{
			IPlayFabEmitEventRequest item;
			bool flag = input.TryTake(out item, EventAvailabilityCheckTimeoutInMs, cts.Token);
			if (cts.Token.IsCancellationRequested)
			{
				break;
			}
			if (stopwatch.Elapsed > BatchFillTimeout)
			{
				StoreAllBatches();
			}
			if (flag)
			{
				yield return item;
			}
		}
		if (!cts.Token.IsCancellationRequested)
		{
			StoreAllBatches();
		}
		stopwatch.Stop();
	}

	private void StoreBatch(string titleId)
	{
		if (batches[titleId].Count > 0)
		{
			StoreOutput(new TitleEventBatch(titleId, batches[titleId]));
		}
	}

	private void StoreAllBatches()
	{
		foreach (KeyValuePair<string, List<IPlayFabEmitEventRequest>> batch in batches)
		{
			StoreBatch(batch.Key);
		}
		batches.Clear();
		stopwatch.Reset();
	}

	private void CreateNewBatch(string titleId)
	{
		batches[titleId] = new List<IPlayFabEmitEventRequest>(BatchSize);
	}

	private void InitNewBatch(string titleId)
	{
		if (!batches.ContainsKey(titleId))
		{
			CreateNewBatch(titleId);
		}
	}

	private static bool ValidationCheck(IPlayFabEmitEventRequest request)
	{
		return true;
	}
}
