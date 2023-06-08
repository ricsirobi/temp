using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PlayFab.EventsModels;
using PlayFab.Logger;
using PlayFab.SharedModels;

namespace PlayFab.Pipeline;

public class OneDSEventPipeline : IEventPipeline, IDisposable
{
	private volatile bool isActive;

	private object isActiveLock = new object();

	private OneDSEventPipelineSettings settings;

	private CancellationToken externalCancellationToken;

	private CancellationTokenSource pipelineCancellationTokenSource;

	private BlockingCollection<IPlayFabEmitEventRequest> eventBuffer;

	private BlockingCollection<TitleEventBatch> batchBuffer;

	private BlockingCollection<PlayFabResult<WriteEventsResponse>> sendResultBuffer;

	private IPipelineStage<IPlayFabEmitEventRequest, TitleEventBatch> batchingStage;

	private IPipelineStage<TitleEventBatch, PlayFabResult<WriteEventsResponse>> sendingStage;

	private System.Threading.Tasks.Task batchingTask;

	private System.Threading.Tasks.Task sendingTask;

	private System.Threading.Tasks.Task pipelineTask;

	private ILogger logger;

	private bool disposed;

	public OneDSEventPipelineSettings Settings => settings;

	public System.Threading.Tasks.Task PipelineTask => pipelineTask;

	public OneDSEventPipeline(OneDSEventPipelineSettings settings, ILogger logger)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		this.settings = settings;
		if (logger == null)
		{
			throw new ArgumentNullException("logger");
		}
		this.logger = logger;
		batchingStage = new EventBatchingStage(this.settings.BatchSize, this.settings.BatchFillTimeout, logger);
		sendingStage = new EventSendingStage(logger);
	}

	public async System.Threading.Tasks.Task StartAsync()
	{
		try
		{
			ThrowIfDisposed();
			await StartAsync(default(CancellationToken));
		}
		catch (Exception ex)
		{
			logger.Error("Exception in StartAsync (without cancellation token) from " + ex.Source + " with message: " + ex.Message);
		}
	}

	public async System.Threading.Tasks.Task StartAsync(CancellationToken cancellationToken)
	{
		try
		{
			ThrowIfDisposed();
			lock (isActiveLock)
			{
				if (isActive)
				{
					logger.Error("Event pipeline is already active");
					return;
				}
				eventBuffer = new BlockingCollection<IPlayFabEmitEventRequest>(settings.EventBufferSize);
				batchBuffer = new BlockingCollection<TitleEventBatch>(settings.BatchBufferSize);
				sendResultBuffer = new BlockingCollection<PlayFabResult<WriteEventsResponse>>(1);
				externalCancellationToken = cancellationToken;
				pipelineCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(externalCancellationToken);
				ResetPipelineTask();
				isActive = true;
			}
			await pipelineTask;
		}
		catch (Exception ex)
		{
			logger.Error("Exception in StartAsync (with cancellation token) from " + ex.Source + " with message: " + ex.Message);
		}
	}

	public void Stop()
	{
		try
		{
			ThrowIfDisposed();
			lock (isActiveLock)
			{
				if (!isActive)
				{
					logger.Warning("Event pipeline is already not active");
				}
				Cancel();
				isActive = false;
			}
		}
		catch (Exception ex)
		{
			logger.Error("Exception in Stop from " + ex.Source + " with message: " + ex.Message);
		}
	}

	public void Complete()
	{
		try
		{
			ThrowIfDisposed();
			lock (isActiveLock)
			{
				if (!isActive)
				{
					logger.Warning("Event pipeline is already not active");
				}
				eventBuffer.CompleteAdding();
				isActive = false;
			}
		}
		catch (Exception ex)
		{
			logger.Error("Exception in Complete from " + ex.Source + " with message: " + ex.Message);
		}
	}

	public bool IntakeEvent(IPlayFabEmitEventRequest request)
	{
		try
		{
			ThrowIfDisposed();
			if (request == null)
			{
				logger.Error("Request passed to event pipeline is null");
				return false;
			}
			if (!isActive)
			{
				logger.Warning("Event pipeline is not active");
				return false;
			}
			if (!eventBuffer.TryAdd(request))
			{
				logger.Error("Event buffer is full or complete and event {0} cannot be added", ((PlayFabEmitEventRequest)request).Event.Name);
				return false;
			}
			return true;
		}
		catch (Exception ex)
		{
			logger.Error("Exception in synchronous WriteEvent from " + ex.Source + " with message: " + ex.Message);
		}
		return false;
	}

	public async Task<IPlayFabEmitEventResponse> IntakeEventAsync(IPlayFabEmitEventRequest request)
	{
		IPlayFabEmitEventResponse result = default(IPlayFabEmitEventResponse);
		object obj;
		int num;
		try
		{
			ThrowIfDisposed();
			TaskCompletionSource<IPlayFabEmitEventResponse> taskCompletionSource = new TaskCompletionSource<IPlayFabEmitEventResponse>();
			if (request == null)
			{
				logger.Error("Request passed to event pipeline is null");
				taskCompletionSource.SetCanceled();
			}
			else if (!isActive)
			{
				logger.Warning("Event pipeline is not active");
				taskCompletionSource.SetCanceled();
			}
			else
			{
				((PlayFabEmitEventRequest)request).ResultPromise = taskCompletionSource;
				if (!eventBuffer.TryAdd(request))
				{
					logger.Error("Event buffer is full or complete and event {0} cannot be added", ((PlayFabEmitEventRequest)request).Event.Name);
					taskCompletionSource.SetCanceled();
				}
			}
			result = await taskCompletionSource.Task;
			return result;
		}
		catch (Exception ex)
		{
			obj = ex;
			num = 1;
		}
		if (num != 1)
		{
			return result;
		}
		Exception ex2 = (Exception)obj;
		logger.Error("Exception in IntakeEventAsync from " + ex2.Source + " with message: " + ex2.Message);
		TaskCompletionSource<IPlayFabEmitEventResponse> taskCompletionSource2 = new TaskCompletionSource<IPlayFabEmitEventResponse>();
		taskCompletionSource2.SetResult(new PlayFabEmitEventResponse());
		return await taskCompletionSource2.Task;
	}

	private void Cancel()
	{
		eventBuffer?.CompleteAdding();
		pipelineCancellationTokenSource?.Cancel();
	}

	private void ThrowIfDisposed()
	{
		if (disposed)
		{
			throw new ObjectDisposedException("OneDSEventPipeline");
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}
		if (disposing)
		{
			Cancel();
			if (pipelineCancellationTokenSource != null)
			{
				pipelineCancellationTokenSource.Dispose();
			}
		}
		pipelineCancellationTokenSource = null;
		disposed = true;
	}

	public void Dispose()
	{
		try
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		catch (Exception ex)
		{
			logger.Error("Exception in Dispose from " + ex.Source + " with message: " + ex.Message);
		}
	}

	private void ResetPipelineTask()
	{
		ResetBatchingTask();
		ResetSendingTask();
		pipelineTask = System.Threading.Tasks.Task.WhenAll(batchingTask, sendingTask);
	}

	private void ResetBatchingTask()
	{
		batchingTask = System.Threading.Tasks.Task.Run(delegate
		{
			batchingStage.RunStage(eventBuffer, batchBuffer, pipelineCancellationTokenSource);
		});
	}

	private void ResetSendingTask()
	{
		sendingTask = System.Threading.Tasks.Task.Run(delegate
		{
			sendingStage.RunStage(batchBuffer, sendResultBuffer, pipelineCancellationTokenSource);
		});
	}
}
