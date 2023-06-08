using System;

[AttributeUsage(AttributeTargets.Field)]
public class Replicated : Attribute
{
	public float threshold;

	public Replicated()
	{
		threshold = 0f;
	}

	public Replicated(float T)
	{
		threshold = T;
	}
}
