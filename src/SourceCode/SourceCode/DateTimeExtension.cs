using System;

public static class DateTimeExtension
{
	public static double GetOffsetTime(this DateTime time, string addTimeString)
	{
		char[] separator = new char[5] { ' ', ',', '.', ':', '\t' };
		DateTime dateTime = time;
		if (!string.IsNullOrEmpty(addTimeString))
		{
			string[] array = addTimeString.Split(separator);
			int result = 0;
			for (int i = 0; i < array.Length; i++)
			{
				if (int.TryParse(array[i], out result))
				{
					switch (i)
					{
					case 0:
						dateTime = dateTime.AddYears(result);
						break;
					case 1:
						dateTime = dateTime.AddMonths(result);
						break;
					case 2:
						dateTime = dateTime.AddDays(result);
						break;
					case 3:
						dateTime = dateTime.AddHours(result);
						break;
					case 4:
						dateTime = dateTime.AddMinutes(result);
						break;
					case 5:
						dateTime = dateTime.AddSeconds(result);
						break;
					}
				}
			}
		}
		return (dateTime - time).TotalSeconds;
	}

	public static double GetOffsetTime(this DateTime time, int[] addTimeArray)
	{
		DateTime dateTime = time;
		for (int i = 0; i < addTimeArray.Length; i++)
		{
			switch (i)
			{
			case 0:
				dateTime = dateTime.AddYears(addTimeArray[i]);
				break;
			case 1:
				dateTime = dateTime.AddMonths(addTimeArray[i]);
				break;
			case 2:
				dateTime = dateTime.AddDays(addTimeArray[i]);
				break;
			case 3:
				dateTime = dateTime.AddHours(addTimeArray[i]);
				break;
			case 4:
				dateTime = dateTime.AddMinutes(addTimeArray[i]);
				break;
			case 5:
				dateTime = dateTime.AddSeconds(addTimeArray[i]);
				break;
			}
		}
		return (dateTime - time).TotalSeconds;
	}
}
