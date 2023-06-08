public class Bait
{
	public float _NibbleModifier = 1f;

	public int _PreferredFishItemID = 7144;

	public ItemData mData;

	public Bait(ItemData data)
	{
		mData = data;
	}

	public float GetModifier(string fish)
	{
		if (mData != null)
		{
			return mData.GetAttribute(fish, 1f);
		}
		return 1f;
	}
}
