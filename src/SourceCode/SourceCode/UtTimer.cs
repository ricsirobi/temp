public class UtTimer
{
	public delegate void ElapsedEventHandler(object inTimer);

	internal class Timer
	{
		internal ElapsedEventHandler mElapsed;

		internal float mInterval;

		internal bool mIsRunning;

		internal Timer()
		{
		}

		internal Timer(float inInterval, ElapsedEventHandler inElapsedHandler)
		{
			mInterval = inInterval;
			mElapsed = inElapsedHandler;
		}
	}

	internal Timer mTimer = new Timer();

	public ElapsedEventHandler Elapsed
	{
		get
		{
			return mTimer.mElapsed;
		}
		set
		{
			mTimer.mElapsed = value;
		}
	}

	public float Interval
	{
		get
		{
			return mTimer.mInterval;
		}
		set
		{
			mTimer.mInterval = value;
		}
	}

	public UtTimer()
	{
	}

	public UtTimer(float inInterval)
	{
		Interval = inInterval;
	}

	public void Start()
	{
		UtTimerManager.StartTimer(this);
	}

	public void Stop()
	{
		Timer timer = new Timer(Interval, Elapsed);
		mTimer.mIsRunning = false;
		mTimer = timer;
	}
}
