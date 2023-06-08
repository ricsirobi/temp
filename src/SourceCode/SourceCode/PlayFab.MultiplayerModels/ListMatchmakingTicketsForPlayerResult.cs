using System;
using System.Collections.Generic;
using PlayFab.SharedModels;

namespace PlayFab.MultiplayerModels;

[Serializable]
public class ListMatchmakingTicketsForPlayerResult : PlayFabResultCommon
{
	public List<string> TicketIds;
}
