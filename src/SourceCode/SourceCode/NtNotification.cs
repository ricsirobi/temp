using System.Collections;

public class NtNotification
{
	public enum Type
	{
		NONE,
		KA_MESSAGE
	}

	public Type mType;

	public int mBadgeCount;

	public string mAlertText;

	public string mSound;

	public Hashtable mData;
}
