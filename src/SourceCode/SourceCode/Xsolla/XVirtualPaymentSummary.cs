using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XVirtualPaymentSummary : IParseble
{
	public struct SimpleVItem
	{
		public string Name { get; private set; }

		public string ImageUrl { get; private set; }

		public int Quantity { get; private set; }

		public string GetImage()
		{
			if (!ImageUrl.StartsWith("http"))
			{
				return "https:" + ImageUrl;
			}
			return ImageUrl;
		}

		public SimpleVItem(string name, string imageUrl, int quantity)
		{
			Name = name;
			ImageUrl = imageUrl;
			Quantity = quantity;
		}

		public override string ToString()
		{
			return $"[SimpleVItem: name={Name}, imageUrl={ImageUrl}, quantity={Quantity}]";
		}
	}

	public int TotalWithoutDiscount { get; private set; }

	public int Total { get; private set; }

	public bool IsSkipConfirmation { get; private set; }

	public List<SimpleVItem> Items { get; private set; }

	public XVirtualPaymentSummary()
	{
		Items = new List<SimpleVItem>();
	}

	public IParseble Parse(JSONNode rootNode)
	{
		TotalWithoutDiscount = rootNode["finance"]["total_without_discount"]["vc_amount"].AsInt;
		Total = rootNode["finance"]["total"]["vc_amount"].AsInt;
		IsSkipConfirmation = rootNode["skip_confirmation"].AsBool;
		IEnumerator<JSONNode> enumerator = rootNode["purchase"]["virtual_items"].AsArray.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode current = enumerator.Current;
			SimpleVItem item = new SimpleVItem(current["name"].Value, current["image_url"].Value, current["quantity"].AsInt);
			Items.Add(item);
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XVirtualPaymentSummary: totalWithoutDiscount={TotalWithoutDiscount}, total={Total}, isSkipConfirmation={IsSkipConfirmation}, items={Items}]";
	}
}
