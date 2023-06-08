using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class KAUIVisibilityNotify : KAMonoBase
{
	public OnVisibilityChanged _OnVisibilityChanged;

	private UIWidget mWidget;

	private bool mIsVisible;

	private void Start()
	{
		mWidget = GetComponent<UIWidget>();
		if (mWidget == null)
		{
			UtDebug.LogError("Could not find component of type UIWidget under : " + base.name);
			base.enabled = false;
		}
		else
		{
			mIsVisible = mWidget.isVisible;
		}
	}

	private void Update()
	{
		if (mWidget.isVisible != mIsVisible)
		{
			mIsVisible = mWidget.isVisible;
			if (_OnVisibilityChanged != null)
			{
				_OnVisibilityChanged(mWidget, mIsVisible);
			}
		}
	}
}
