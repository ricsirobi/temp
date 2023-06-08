namespace Xsolla;

public class ValidatorCvv : ValidatorBase
{
	private bool _isMaestro;

	public ValidatorCvv()
	{
		_errorMsg = "Invalid Cvv";
	}

	public ValidatorCvv(string s)
		: base(s)
	{
	}

	public ValidatorCvv(string s, bool isMaestro)
		: base(s)
	{
		_isMaestro = isMaestro;
	}

	public override bool Validate(string s)
	{
		if (!_isMaestro)
		{
			if (s.Length > 2)
			{
				return s.Length < 5;
			}
			return false;
		}
		return true;
	}
}
