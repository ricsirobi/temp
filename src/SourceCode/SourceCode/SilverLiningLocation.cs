public class SilverLiningLocation
{
	private double latitude;

	private double longitude;

	private double altitude;

	public SilverLiningLocation()
	{
	}

	public SilverLiningLocation(SilverLiningLocation l)
	{
		latitude = l.GetLatitude();
		longitude = l.GetLongitude();
		altitude = l.GetAltitude();
	}

	public void SetLatitude(double l)
	{
		latitude = l;
	}

	public void SetLongitude(double l)
	{
		longitude = l;
	}

	public void SetAltitude(double a)
	{
		altitude = a;
	}

	public double GetLatitude()
	{
		return latitude;
	}

	public double GetLongitude()
	{
		return longitude;
	}

	public double GetAltitude()
	{
		return altitude;
	}

	public override int GetHashCode()
	{
		return (int)(latitude * 1000.0) ^ (int)(longitude * 1000.0) ^ (int)(altitude * 1000.0);
	}

	public override bool Equals(object t)
	{
		return this == (SilverLiningLocation)t;
	}

	public static bool operator ==(SilverLiningLocation l1, SilverLiningLocation l2)
	{
		if (l1.GetLatitude() == l2.GetLatitude() && l1.GetLongitude() == l2.GetLongitude())
		{
			return l1.GetAltitude() == l2.GetAltitude();
		}
		return false;
	}

	public static bool operator !=(SilverLiningLocation l1, SilverLiningLocation l2)
	{
		if (l1.GetLatitude() == l2.GetLatitude() && l1.GetLongitude() == l2.GetLongitude())
		{
			return l1.GetAltitude() != l2.GetAltitude();
		}
		return true;
	}
}
