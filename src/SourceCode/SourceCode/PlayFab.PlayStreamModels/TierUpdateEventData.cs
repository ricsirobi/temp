using System.Collections.Generic;

namespace PlayFab.PlayStreamModels;

public class TierUpdateEventData : PlayStreamEventBase
{
	public string ContactCompanyName;

	public bool IsPayAsYouGo;

	public bool IsReservedCapacity;

	public bool IsReservedCapacityAnnual;

	public double MonthlyMinimumUSD;

	public List<PaymentOptionPerMauPriceTier> OveragePricePerMauTiers;

	public string PaymentSystemAccountId;

	public List<PaymentOptionPerMauPriceTier> PricePerMauTiers;

	public int? ReservedMAU;

	public IEnumerable_String StudioIds;

	public string TierDisplayName;

	public string TierId;

	public string TransactionId;
}
