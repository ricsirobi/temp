namespace Xsolla;

public abstract class AXsollaShopItem : IXsollaObject
{
	public enum AdType
	{
		NONE,
		SPECIAL_OFFER,
		RECCOMENDED,
		BEST_DEAL
	}

	public string label { get; protected set; }

	public string offerLabel { get; protected set; }

	public AdType advertisementType { get; protected set; }

	public abstract string GetKey();

	public abstract string GetName();

	public string GetLabel()
	{
		if ("".Equals(offerLabel) || "null".Equals(offerLabel))
		{
			if ("null".Equals(label))
			{
				return "SPECIAL OFFER";
			}
			return label;
		}
		return offerLabel;
	}

	public AdType GetAdvertisementType()
	{
		return advertisementType;
	}

	public override string ToString()
	{
		return $"[AXsollaShopItem: label={label}, offerLabel={offerLabel}, advertisementType={advertisementType}]";
	}
}
