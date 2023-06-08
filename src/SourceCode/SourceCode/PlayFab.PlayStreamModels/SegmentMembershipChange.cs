using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class SegmentMembershipChange
{
	public SegmentMembershipChangeType? Change;

	public string EventId;

	public string SegmentId;
}
