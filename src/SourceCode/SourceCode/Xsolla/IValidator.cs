namespace Xsolla;

public interface IValidator
{
	bool Validate(string s);

	string GetErrorMsg();
}
