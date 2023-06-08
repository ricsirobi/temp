using System;

namespace PlayFab.PlayStreamModels;

[Serializable]
public class PaymentOptionPerMauPriceTier
{
	public int? LowerBoundInclusive;

	public string Name;

	public MetricUnit? PriceUnit;

	public double? PriceUnitSize;

	public double? PriceUSD;

	public string PriceUSDFormatted;

	public int? UpperBoundInclusive;
}
