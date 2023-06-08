using System;

public class SilverLiningTime
{
	private int localYear;

	private int localMonth;

	private int localDay;

	private int localHours;

	private int localMinutes;

	private double zoneCorrection;

	private double localSeconds;

	private bool observingDST;

	public SilverLiningTime()
	{
		localYear = 2011;
		localMonth = 9;
		localDay = 17;
		observingDST = true;
		localHours = 15;
		localMinutes = 50;
		localSeconds = 0.0;
		zoneCorrection = -8.0;
	}

	public SilverLiningTime(SilverLiningTime t)
	{
		localYear = t.GetYear();
		localMonth = t.GetMonth();
		localDay = t.GetDay();
		observingDST = t.GetDST();
		localHours = t.GetHour();
		localMinutes = t.GetMinute();
		localSeconds = t.GetSeconds();
		zoneCorrection = t.GetTimeZone();
	}

	public override int GetHashCode()
	{
		return localYear ^ localMonth ^ localDay ^ (observingDST ? 1 : 0) ^ localHours ^ localMinutes ^ (int)localSeconds ^ (int)zoneCorrection;
	}

	public override bool Equals(object t)
	{
		return this == (SilverLiningTime)t;
	}

	public static bool operator ==(SilverLiningTime t1, SilverLiningTime t2)
	{
		if (t1.GetYear() == t2.GetYear() && t1.GetMonth() == t2.GetMonth() && t1.GetDay() == t2.GetDay() && t1.GetDST() == t2.GetDST() && t1.GetHour() == t2.GetHour() && t1.GetMinute() == t2.GetMinute() && t1.GetSeconds() == t2.GetSeconds())
		{
			return t1.GetTimeZone() == t2.GetTimeZone();
		}
		return false;
	}

	public static bool operator !=(SilverLiningTime t1, SilverLiningTime t2)
	{
		if (t1.GetYear() == t2.GetYear() && t1.GetMonth() == t2.GetMonth() && t1.GetDay() == t2.GetDay() && t1.GetDay() == t2.GetDay() && t1.GetHour() == t2.GetHour() && t1.GetMinute() == t2.GetMinute() && t1.GetSeconds() == t2.GetSeconds())
		{
			return t1.GetTimeZone() != t2.GetTimeZone();
		}
		return true;
	}

	public bool SetDate(int year, int month, int day)
	{
		if (month > 0 && month <= 12 && day > 0 && day <= 31)
		{
			localYear = year;
			localMonth = month;
			localDay = day;
			return true;
		}
		return false;
	}

	public bool SetTime(int hour, int minutes, double seconds, double timeZone, bool dst)
	{
		if (hour >= 0 && hour < 24 && minutes >= 0 && minutes < 60 && seconds >= 0.0 && seconds < 60.0)
		{
			localHours = hour;
			localMinutes = minutes;
			localSeconds = seconds;
			zoneCorrection = timeZone;
			observingDST = dst;
			return true;
		}
		return false;
	}

	public double GetJulianDate(bool terrestrialTime)
	{
		double num = (double)localHours + ((double)localMinutes + localSeconds / 60.0) / 60.0;
		if (observingDST)
		{
			num -= 1.0;
		}
		num -= zoneCorrection;
		double num2 = (double)localDay + num / 24.0;
		double num3;
		double num4;
		if (localMonth < 3)
		{
			num3 = localYear - 1;
			num4 = localMonth + 12;
		}
		else
		{
			num3 = localYear;
			num4 = localMonth;
		}
		double num5 = 1720996.5 - Math.Floor(num3 / 100.0) + Math.Floor(num3 / 400.0) + 365.0 * num3 + Math.Floor(30.6001 * (num4 + 1.0)) + num2;
		if (terrestrialTime)
		{
			num5 += 0.0007523148148148147;
		}
		return num5;
	}

	public double GetEpoch2000Centuries(bool terrestrialTime)
	{
		double num = (double)localHours + ((double)localMinutes + localSeconds / 60.0) / 60.0;
		if (observingDST)
		{
			num -= 1.0;
		}
		num -= zoneCorrection;
		double num2 = (double)localDay + num / 24.0;
		double num3;
		double num4;
		if (localMonth < 3)
		{
			num3 = localYear - 1;
			num4 = localMonth + 12;
		}
		else
		{
			num3 = localYear;
			num4 = localMonth;
		}
		double num5 = 1720996.5 - Math.Floor(num3 / 100.0) + Math.Floor(num3 / 400.0) + Math.Floor(365.25 * num3) + Math.Floor(30.6001 * (num4 + 1.0)) - 2451545.0 + num2;
		if (terrestrialTime)
		{
			num5 += 0.0007523148148148147;
		}
		return num5 / 36525.0;
	}

	public double GetEpoch1990Days(bool terrestrialTime)
	{
		double num = (double)localHours + ((double)localMinutes + localSeconds / 60.0) / 60.0;
		if (observingDST)
		{
			num -= 1.0;
		}
		num -= zoneCorrection;
		double num2 = (double)localDay + num / 24.0;
		double num3;
		double num4;
		if (localMonth < 3)
		{
			num3 = localYear - 1;
			num4 = localMonth + 12;
		}
		else
		{
			num3 = localYear;
			num4 = localMonth;
		}
		double num5 = 1720996.5 - Math.Floor(num3 / 100.0) + Math.Floor(num3 / 400.0) + Math.Floor(365.25 * num3) + Math.Floor(30.6001 * (num4 + 1.0)) - 2447891.5 + num2;
		if (terrestrialTime)
		{
			num5 += 0.0007523148148148147;
		}
		return num5;
	}

	public int GetYear()
	{
		return localYear;
	}

	public int GetMonth()
	{
		return localMonth;
	}

	public int GetDay()
	{
		return localDay;
	}

	public int GetHour()
	{
		return localHours;
	}

	public int GetMinute()
	{
		return localMinutes;
	}

	public double GetTimeZone()
	{
		return zoneCorrection;
	}

	public double GetSeconds()
	{
		return localSeconds;
	}

	public bool GetDST()
	{
		return observingDST;
	}
}
