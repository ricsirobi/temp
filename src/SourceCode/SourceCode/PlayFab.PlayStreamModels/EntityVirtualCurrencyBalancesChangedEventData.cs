using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

public class EntityVirtualCurrencyBalancesChangedEventData : PlayStreamEventBase
{
	public string EntityChain;

	public EntityLineage EntityLineage;

	public string SequenceId;

	public Dictionary<string, int> VirtualCurrencyBalances;

	public string VirtualCurrencyContainerId;

	public Dictionary<string, int> VirtualCurrencyPreviousBalances;
}
