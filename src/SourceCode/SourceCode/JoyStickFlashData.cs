using System;

[Serializable]
public class JoyStickFlashData
{
	public string _FlashWidgetName;

	[NonSerialized]
	public KAWidget _FlashWidget;
}
