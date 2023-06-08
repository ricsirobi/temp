using UnityEngine;

public class AvPhotoSetter
{
	private KAWidget mItem;

	public AvPhotoSetter(KAWidget item)
	{
		mItem = item;
	}

	public void PhotoCallback(Texture tex, object inUserData)
	{
		if (mItem != null)
		{
			mItem.SetTexture(tex);
		}
	}
}
