using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaPurchase : IParseble
{
	public class VirtualCurrency : IParseble
	{
		public int quantity { get; private set; }

		public bool allowModify { get; private set; }

		public IParseble Parse(JSONNode virtualCurrencyNode)
		{
			quantity = virtualCurrencyNode["quantity"].AsInt;
			allowModify = virtualCurrencyNode["allow_modify"].AsBool;
			return this;
		}
	}

	public class Subscription : IParseble
	{
		public string id { get; private set; }

		public bool allowModify { get; private set; }

		public IParseble Parse(JSONNode subscriptionNode)
		{
			id = subscriptionNode["plan_id"];
			allowModify = subscriptionNode["allow_modify"].AsBool;
			return this;
		}
	}

	public class VirtualItems : IParseble
	{
		public struct Item
		{
			public long sku;

			public int amount;
		}

		public bool allowModify { get; private set; }

		public List<Item> items { get; private set; }

		public IParseble Parse(JSONNode itemsNode)
		{
			allowModify = itemsNode["allow_modify"].AsBool;
			IEnumerator<JSONNode> enumerator = itemsNode["items"].Childs.GetEnumerator();
			items = new List<Item>();
			while (enumerator.MoveNext())
			{
				Item item = default(Item);
				item.sku = enumerator.Current["sku"].AsInt;
				item.amount = enumerator.Current["amount"].AsInt;
				items.Add(item);
			}
			return this;
		}
	}

	public class Checkout : IParseble
	{
		public int amount { get; private set; }

		public string currency { get; private set; }

		public IParseble Parse(JSONNode virtualCurrencyNode)
		{
			amount = virtualCurrencyNode["amount"].AsInt;
			currency = virtualCurrencyNode["currency"].Value;
			return this;
		}
	}

	public class PaymentSystem : IParseble
	{
		public long id { get; private set; }

		public bool allowModify { get; private set; }

		public IParseble Parse(JSONNode paymentSystemNode)
		{
			id = paymentSystemNode["id"].AsInt;
			allowModify = paymentSystemNode["allow_modify"].AsBool;
			return this;
		}
	}

	public Checkout checkout;

	public VirtualCurrency virtualCurrency { get; private set; }

	public VirtualItems virtualItems { get; private set; }

	public Subscription subscription { get; private set; }

	public PaymentSystem paymentSystem { get; private set; }

	public bool IsPurchase()
	{
		if (virtualCurrency == null && virtualItems == null && subscription == null)
		{
			return checkout != null;
		}
		return true;
	}

	public bool IsPaymentSystem()
	{
		return paymentSystem != null;
	}

	public IParseble Parse(JSONNode purchaseNode)
	{
		if (purchaseNode.Count == 0)
		{
			return null;
		}
		if (purchaseNode["virtual_currency"] != null)
		{
			virtualCurrency = new VirtualCurrency().Parse(purchaseNode["virtual_currency"]) as VirtualCurrency;
		}
		if (purchaseNode["virtual_items"] != null)
		{
			virtualItems = new VirtualItems().Parse(purchaseNode["virtual_items"]) as VirtualItems;
		}
		if (purchaseNode["subscription"] != null)
		{
			subscription = new Subscription().Parse(purchaseNode["subscription"]) as Subscription;
		}
		if (purchaseNode["payment_system"] != null)
		{
			paymentSystem = new PaymentSystem().Parse(purchaseNode["payment_system"]) as PaymentSystem;
		}
		if (purchaseNode["checkout"] != null)
		{
			checkout = new Checkout().Parse(purchaseNode["checkout"]) as Checkout;
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaPurchase: virtualCurrency={virtualCurrency}, virtualItems={virtualItems}, subscription={subscription}, paymentSystem={paymentSystem}]";
	}
}
