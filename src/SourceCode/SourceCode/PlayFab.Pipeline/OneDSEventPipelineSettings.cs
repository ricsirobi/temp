using System;

namespace PlayFab.Pipeline;

public class OneDSEventPipelineSettings
{
	public const int DefaultEventBufferSize = 100;

	public const int DefaultBatchBufferSize = 20;

	public const int MinBatchSize = 1;

	public const int MaxBatchSize = 25;

	public const int DefaultBatchSize = 25;

	public const int DefaultMaxHttpAttempts = 3;

	public static readonly TimeSpan MinBatchFillTimeout = TimeSpan.FromMilliseconds(100.0);

	public static readonly TimeSpan MaxBatchFillTimeout = TimeSpan.FromHours(1.0);

	public static readonly TimeSpan DefaultBatchFillTimeout = TimeSpan.FromSeconds(5.0);

	private int batchSize = 25;

	private TimeSpan batchFillTimeout = DefaultBatchFillTimeout;

	public int EventBufferSize { get; set; } = 100;


	public int BatchBufferSize { get; set; } = 20;


	public int BatchSize
	{
		get
		{
			return batchSize;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException("BatchSize", $"The batch size setting cannot be less than {1}");
			}
			if (value > 25)
			{
				throw new ArgumentOutOfRangeException("BatchSize", $"The batch size setting cannot be greater than {25}");
			}
			batchSize = value;
		}
	}

	public TimeSpan BatchFillTimeout
	{
		get
		{
			return batchFillTimeout;
		}
		set
		{
			if (value < MinBatchFillTimeout)
			{
				throw new ArgumentOutOfRangeException("BatchFillTimeout", $"The batch fill timeout setting cannot be less than {MinBatchFillTimeout}");
			}
			if (value > MaxBatchFillTimeout)
			{
				throw new ArgumentOutOfRangeException("BatchFillTimeout", $"The batch fill timeout setting cannot be greater than {MaxBatchFillTimeout}");
			}
			batchFillTimeout = value;
		}
	}

	public int MaxHttpAttempts { get; set; } = 3;

}
