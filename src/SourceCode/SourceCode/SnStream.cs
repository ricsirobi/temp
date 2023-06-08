using UnityEngine;

public class SnStream
{
	public static SnIStream Create(string inStreamName)
	{
		return null;
	}

	public static SnIStream Create(string inStreamName, int inPriority)
	{
		return null;
	}

	public static SnIStream Create(string inStreamName, int inPriority, float inVolume)
	{
		return null;
	}

	public static SnIStream Create(AudioSource inSource, SnChannel inChannel)
	{
		return new SnBundleSource(inSource, inChannel);
	}
}
