using System;

public class DateMenu : IDisposable
{
	private KAUIMenu mYearMenu;

	private KAUIMenu mMonthMenu;

	private KAUIMenu mDaysMenu;

	private string mYearTemplate;

	private string mMonthTemplate;

	private string mDayTemplate;

	private int mPrvYearSelected;

	private int mPrvMonthSelected;

	private int mNumOfYear;

	private DateMenu()
	{
	}

	public DateMenu(KAUI inInterface, KAUIMenu inYearMenu, KAUIMenu inMonthMenu, KAUIMenu inDaysMenu, string inYearTemplate, string inMonthTemplate, string inDayTemplate, int inNumOfYear)
	{
		mYearMenu = inYearMenu;
		mMonthMenu = inMonthMenu;
		mDaysMenu = inDaysMenu;
		mYearTemplate = inYearTemplate;
		mMonthTemplate = inMonthTemplate;
		mDayTemplate = inDayTemplate;
		mNumOfYear = inNumOfYear;
		DateTime now = DateTime.Now;
		mYearMenu.ClearItems();
		mMonthMenu.ClearItems();
		mDaysMenu.ClearItems();
		int num = 0;
		int num2 = 0;
		for (num = 0; num < mNumOfYear + num2; num++)
		{
			KAWidget kAWidget = mYearMenu.AddWidget(mYearTemplate);
			int value = -(num % mNumOfYear);
			int year = now.AddYears(value).Year;
			kAWidget.SetText(year.ToString());
			kAWidget.name = "YearItem_" + year;
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtName");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(year.ToString());
			}
			kAWidget.SetVisibility(inVisible: true);
		}
		mYearMenu.SetSelectedItem(mYearMenu.FindItemAt(num - 1));
		now = new DateTime(2000, 1, 1);
		num2 = 0;
		for (num = 0; num < 12 + num2; num++)
		{
			KAWidget kAWidget3 = mMonthMenu.AddWidget(mMonthTemplate);
			string text = now.ToString("MMMM");
			kAWidget3.SetText(text);
			kAWidget3.name = "MonthItem_" + text;
			KAWidget kAWidget4 = kAWidget3.FindChildItem("TxtName");
			if (kAWidget4 != null)
			{
				kAWidget4.SetText(text);
			}
			now = now.AddMonths(1);
			kAWidget3.SetVisibility(inVisible: true);
		}
		num = 1;
		mMonthMenu.SetSelectedItem(mMonthMenu.FindItemAt(num - 1));
		AddDays(now.Year, 1, 100);
	}

	public void Dispose()
	{
		mYearMenu = null;
		mMonthMenu = null;
		mDaysMenu = null;
		mYearTemplate = null;
		mMonthTemplate = null;
		mDayTemplate = null;
	}

	public void OnDateChange(int inYear, int inMonth, int inSelDay)
	{
		if (inYear != mPrvYearSelected || inMonth != mPrvMonthSelected)
		{
			mPrvYearSelected = inYear;
			mPrvMonthSelected = inMonth;
			AddDays(inYear, inMonth, inSelDay);
		}
	}

	private void AddDays(int inYear, int inMonth, int inSelDay)
	{
		int num = DateTime.DaysInMonth(inYear, inMonth);
		mDaysMenu.ClearItems();
		int num2 = 0;
		for (int i = 0; i < num + num2; i++)
		{
			KAWidget kAWidget = mDaysMenu.AddWidget(mDayTemplate);
			int num3 = i;
			num3 = num3 % num + 1;
			kAWidget.name = "DayItem_" + num3;
			kAWidget.SetText(num3.ToString());
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtName");
			if (kAWidget2 != null)
			{
				kAWidget2.SetText(num3.ToString());
			}
			kAWidget.SetVisibility(inVisible: true);
		}
		if (num < inSelDay)
		{
			inSelDay = 1;
		}
		mDaysMenu.SetSelectedItem(mDaysMenu.FindItemAt(inSelDay - 1));
	}
}
