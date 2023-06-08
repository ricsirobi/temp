namespace Xsolla;

public class ValidatorMonth : ValidatorBase
{
	public ValidatorMonth()
	{
		_errorMsg = "Invalid month";
	}

	public ValidatorMonth(string s)
		: base(s)
	{
	}

	public override bool Validate(string s)
	{
		if (intValidator.isInteger(s))
		{
			int num = int.Parse(s);
			if (num <= 12)
			{
				return num > 0;
			}
			return false;
		}
		return false;
	}
}
