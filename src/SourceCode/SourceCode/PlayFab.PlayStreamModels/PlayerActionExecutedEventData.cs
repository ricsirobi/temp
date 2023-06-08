using System;

namespace PlayFab.PlayStreamModels;

public class PlayerActionExecutedEventData : PlayStreamEventBase
{
	public string ActionName;

	public ActionExecutionError Error;

	public double ExecutionDuration;

	public object ExecutionResult;

	public DateTime ScheduledTimestamp;

	public string TitleId;

	public DateTime TriggeredTimestamp;

	public EventRuleMatch TriggeringEventRuleMatch;

	public SegmentMembershipChange TriggeringSegmentMembershipChange;
}
