using System;

namespace KA.Framework;

[Serializable]
public enum Environment
{
	DEV,
	QA,
	SODSTAGING,
	STAGING,
	LIVE,
	ALL,
	UNKNOWN
}
