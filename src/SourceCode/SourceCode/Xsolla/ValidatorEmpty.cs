namespace Xsolla;

public class ValidatorEmpty : ValidatorBase
{
	public ValidatorEmpty()
	{
		_errorMsg = "Can't be empty";
	}

	public ValidatorEmpty(string s)
		: base(s)
	{
	}

	public override bool Validate(string s)
	{
		return !"".Equals(s);
	}
}
