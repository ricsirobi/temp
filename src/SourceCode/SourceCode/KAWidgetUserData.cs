public class KAWidgetUserData
{
	public KAWidget _Item;

	public int _Index;

	public KAWidgetUserData()
	{
		_Index = -1;
	}

	public KAWidgetUserData(int i)
	{
		_Index = i;
	}

	public KAWidgetUserData(int i, KAWidget inItem)
	{
		_Index = i;
		_Item = inItem;
	}

	public KAWidget GetItem()
	{
		return _Item;
	}

	public virtual KAWidgetUserData CloneObject()
	{
		return (KAWidgetUserData)MemberwiseClone();
	}

	public virtual void Destroy()
	{
	}
}
