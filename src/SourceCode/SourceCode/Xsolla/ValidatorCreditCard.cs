using System.Linq;

namespace Xsolla;

public class ValidatorCreditCard : ValidatorBase
{
	public ValidatorCreditCard()
	{
		_errorMsg = "Invalid Credit Card";
	}

	public ValidatorCreditCard(string s)
		: base(s)
	{
	}

	public override bool Validate(string s)
	{
		s = s.Replace("\\s", "");
		s = s.Replace(" ", "");
		return checkLuhn(strToIntArr(s));
	}

	public int[] strToIntArr(string intString)
	{
		if (intString.Length < 12)
		{
			return null;
		}
		int i;
		return (from s in intString.ToCharArray()
			select int.TryParse(s.ToString(), out i) ? i : 0).ToArray();
	}

	public static bool checkLuhn(int[] digits)
	{
		if (digits == null || digits.Length < 12)
		{
			return false;
		}
		int num = 0;
		int num2 = digits.Length;
		for (int i = 0; i < num2; i++)
		{
			int num3 = digits[num2 - i - 1];
			if (i % 2 == 1)
			{
				num3 *= 2;
			}
			num += ((num3 > 9) ? (num3 - 9) : num3);
		}
		return num % 10 == 0;
	}
}
