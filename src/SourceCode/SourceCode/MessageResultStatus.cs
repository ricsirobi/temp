using System;

[Serializable]
public enum MessageResultStatus
{
	PASSED = 0,
	FAILED = 2,
	BLOCKED = 3,
	ChatBanned = 4,
	ChatNotAllowed = 5,
	NoPermission = 6
}
