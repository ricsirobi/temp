namespace Xsolla;

public struct XComponent
{
	public string Name;

	public bool IsEnabled;

	public XComponent(string newName, bool isEnabled)
	{
		Name = newName;
		IsEnabled = isEnabled;
	}
}
