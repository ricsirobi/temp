using System;

public class GlowEffect
{
	public double Duration;

	public DateTime EndTime;

	public string GlowColor;

	public string Save()
	{
		return Save(Duration.ToString(), GlowColor, EndTime);
	}

	public string Save(string duration, string color)
	{
		return Save(duration, color, ServerTime.pCurrentTime.AddHours(double.Parse(duration)));
	}

	public string Save(string duration, string color, DateTime endTime)
	{
		Duration = float.Parse(duration);
		GlowColor = color;
		EndTime = endTime;
		return UtUtilities.SerializeToString(this);
	}
}
