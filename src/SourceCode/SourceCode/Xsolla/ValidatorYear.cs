using System;

namespace Xsolla;

public class ValidatorYear : ValidatorBase
{
	public ValidatorYear()
	{
		_errorMsg = "Invalid Year";
	}

	public ValidatorYear(string s)
		: base(s)
	{
	}

	public override bool Validate(string s)
	{
		if (intValidator.isInteger(s))
		{
			int num = int.Parse(s);
			num = ((num < 1000) ? (2000 + num) : num);
			int year = DateTime.Now.Year;
			int num2 = year + 50;
			if (num >= year)
			{
				return num <= num2;
			}
			return false;
		}
		return false;
	}
}
