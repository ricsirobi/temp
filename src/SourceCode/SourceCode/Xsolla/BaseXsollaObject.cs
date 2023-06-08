namespace Xsolla;

public class BaseXsollaObject : IXsollaObject
{
	public virtual string GetKey()
	{
		return "key";
	}

	public virtual string GetName()
	{
		return "name";
	}
}
