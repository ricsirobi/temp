using System;

public class KAUITreeListEvents
{
	public event Action<KAWidget, object> OnClick;

	public void ProcessClickEvent(KAWidget inWidget, object inObj)
	{
		if (this.OnClick != null)
		{
			this.OnClick(inWidget, inObj);
		}
	}
}
