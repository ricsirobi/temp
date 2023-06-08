using System;

[Serializable]
public class PointTypeLockText
{
	public int _PointTypeID;

	public LocaleString _LockText = new LocaleString("You need {{Points}} points to buy this {{Item}}.");
}
