public class ObCouchIcon : ObClickable
{
	private ObCouchAttributes mCouchAttributes;

	public ObCouchAttributes pCouchAttributes
	{
		get
		{
			return mCouchAttributes;
		}
		set
		{
			mCouchAttributes = value;
		}
	}

	public void SetActive(bool active)
	{
		if (!active)
		{
			OnMouseExit();
		}
		base.gameObject.SetActive(active);
	}
}
