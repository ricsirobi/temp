using System;
using System.Collections.Generic;

[Serializable]
public class GiftMessageData
{
	public List<string> MessageTagData { get; set; }

	public GiftMessageData()
	{
		MessageTagData = new List<string>();
	}
}
