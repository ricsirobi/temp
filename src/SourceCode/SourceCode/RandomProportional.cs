using System;

public class RandomProportional : Random
{
	protected override double Sample()
	{
		return Math.Sqrt(base.Sample());
	}

	public override int Next()
	{
		return (int)(Sample() * 2147483647.0);
	}
}
