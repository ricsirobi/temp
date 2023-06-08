public interface SnIStream
{
	bool pPlaying { get; }

	bool pPaused { get; set; }

	bool pActive { get; }

	uint pPosition { get; }

	uint pLength { get; }

	float pVolume { get; set; }

	bool pLoop { get; set; }

	void Stop();
}
