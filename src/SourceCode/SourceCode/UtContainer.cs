public class UtContainer : UtDict
{
	public const string NAME_KEY = "__name";

	public const string PARENT_KEY = "__parent";

	public const string TABLE_KEY = "__table";

	protected static uint mParent = UtKey.Get("__parent");

	protected static uint mName = UtKey.Get("__name");

	protected static uint mTable = UtKey.Get("__table");

	protected bool mResolved;

	public UtContainer pParent
	{
		get
		{
			object outValue = null;
			TryGetValue(mParent, out outValue);
			if (outValue.GetType().Equals(typeof(UtData)))
			{
				return (UtContainer)outValue;
			}
			return null;
		}
	}

	public string pName
	{
		get
		{
			object outValue = null;
			TryGetValue(mName, out outValue);
			return (string)outValue;
		}
	}

	public UtTable pTable
	{
		get
		{
			object outValue = null;
			TryGetValue(mTable, out outValue);
			return (UtTable)outValue;
		}
	}

	public bool pResolved
	{
		get
		{
			return mResolved;
		}
		set
		{
			mResolved = value;
		}
	}

	public UtContainer this[string inKey]
	{
		get
		{
			object outValue = null;
			if (TryGetValue(UtKey.Get(inKey), out outValue) && outValue.GetType().Equals(typeof(UtData)))
			{
				return (UtContainer)outValue;
			}
			return null;
		}
	}
}
