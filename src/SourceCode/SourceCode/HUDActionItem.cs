using System;

[Serializable]
public class HUDActionItem
{
	public int _Task;

	public string _ItemName;

	public KAWidget _Widget;

	public HudActions _Action;

	public int _Time;

	public float _RepeatTimeInterval;

	[NonSerialized]
	public Task pTask;

	[NonSerialized]
	public KAWidget pWidget;
}
