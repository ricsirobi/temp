using System;
using System.Collections.Generic;

[Serializable]
public class GiftRule
{
	public List<GiftPrerequisite> Prerequisites;

	public GiftRule()
	{
		Prerequisites = new List<GiftPrerequisite>();
	}
}
