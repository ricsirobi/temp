using System;
using System.Collections.Generic;

[Serializable]
public class StateDetails
{
	public int _ID;

	public string _Name;

	public int _Order;

	public ItemStateCriteriaLength _CriteriaLength;

	public List<ItemStateCriteriaConsumable> _CriteriaConsumables = new List<ItemStateCriteriaConsumable>();

	public ItemStateCriteriaReplenishable _CriteriaReplenishable;

	public ItemStateCriteriaExpiry _CriteriaExpiry;

	public CompletionAction _CompletionAction;
}
