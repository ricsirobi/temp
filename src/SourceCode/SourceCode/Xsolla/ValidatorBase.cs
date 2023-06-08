namespace Xsolla;

public abstract class ValidatorBase : IValidator
{
	protected string _errorMsg;

	public ValidatorBase()
	{
		_errorMsg = "";
	}

	public ValidatorBase(string errorMsg)
	{
		_errorMsg = errorMsg;
	}

	public string GetErrorMsg()
	{
		return _errorMsg;
	}

	public abstract bool Validate(string s);
}
