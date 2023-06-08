using System;

[Serializable]
public class BuddyInviteMessageTypeData
{
	public BuddyMessageType _MessageType;

	public LocaleString _DefaultMessageText;

	public BuddyInviteMessageZoneData[] _ZoneSpecificData;
}
