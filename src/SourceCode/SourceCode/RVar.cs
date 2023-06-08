using System.Reflection;

internal class RVar
{
	public string name;

	public float threshold;

	public object lastValue;

	public object origValue;

	public bool isStatic;

	public FieldInfo field;
}
