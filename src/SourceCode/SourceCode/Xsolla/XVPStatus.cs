using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XVPStatus : IParseble
{
	public struct SimpleVItem
	{
		public string Name { get; private set; }

		public string Description { get; private set; }

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

		public SimpleVItem(string name, string description, string imageUrl, int quantity)
		{
			Name = name;
			Description = description;
			ImageUrl = imageUrl;
			Quantity = quantity;
		}

		public override string ToString()
		{
			return $"[SimpleVItem: name={Name}, imageUrl={ImageUrl}, quantity={Quantity}]";
		}
	}

	public struct SimpleVCur
	{
		public string vcAmount;

		public SimpleVCur(string pVcAmount)
		{
			vcAmount = pVcAmount;
		}
	}

	public struct XStatus
	{
		public string Code { get; private set; }

		public string Description { get; private set; }

		public string Header { get; private set; }

		public string HeaderDescription { get; private set; }

		public XStatus(string code, string description, string header, string headerDescription)
		{
			Code = code;
			Description = description;
			Header = header;
			HeaderDescription = headerDescription;
		}
	}

	public string OperationId { get; private set; }

	public string OperationType { get; private set; }

	public string OperationCreated { get; private set; }

	public string VcAmount { get; private set; }

	public string BackUrl { get; private set; }

	public string ReturnTegion { get; private set; }

	public string BackUrlCaption { get; private set; }

	public XStatus Status { get; private set; }

	public List<SimpleVItem> Items { get; private set; }

	public List<SimpleVCur> vCurr { get; private set; }

	public XVPStatus()
	{
		Items = new List<SimpleVItem>();
		vCurr = new List<SimpleVCur>();
	}

	public XsollaStatus.Group GetGroup()
	{
		return Status.Code switch
		{
			"invoice" => XsollaStatus.Group.INVOICE, 
			"done" => XsollaStatus.Group.DONE, 
			"delivering" => XsollaStatus.Group.DELIVERING, 
			"trobled" => XsollaStatus.Group.TROUBLED, 
			_ => XsollaStatus.Group.UNKNOWN, 
		};
	}

	public Dictionary<string, object> GetPurchaseList()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (Items.Count > 0)
		{
			dictionary.Add("items", Items);
		}
		if (vCurr.Count > 0)
		{
			dictionary.Add("vcur", vCurr);
		}
		return dictionary;
	}

	public string GetPurchase(int i)
	{
		if (Items.Count > 0)
		{
			SimpleVItem simpleVItem = Items[i];
			return simpleVItem.Quantity + " x " + simpleVItem.Name;
		}
		return "";
	}

	public string GetVCPurchase(int i)
	{
		if (vCurr.Count > 0)
		{
			return vCurr[i].vcAmount;
		}
		return "";
	}

	public IParseble Parse(JSONNode rootNode)
	{
		OperationId = rootNode["operation_id"].Value;
		OperationType = rootNode["operation_type"].Value;
		OperationCreated = rootNode["operation_created"].Value;
		VcAmount = rootNode["finance"]["total"]["vc_amount"].Value;
		BackUrl = rootNode["back_url"].Value;
		ReturnTegion = rootNode["return_region"].Value;
		BackUrlCaption = rootNode["back_url_caption"].Value;
		JSONNode jSONNode = rootNode["status"];
		if (jSONNode != null)
		{
			Status = new XStatus(jSONNode["code"].Value, jSONNode["description"].Value, jSONNode["header"].Value, jSONNode["header_description"].Value);
		}
		IEnumerator<JSONNode> enumerator = rootNode["purchase"]["virtual_items"].AsArray.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode current = enumerator.Current;
			SimpleVItem item = new SimpleVItem(current["name"].Value, current["description"].Value, current["image_url"].Value, current["quantity"].AsInt);
			Items.Add(item);
		}
		enumerator = rootNode["purchase"]["virtual_currency"].AsArray.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode current2 = enumerator.Current;
			SimpleVCur item2 = new SimpleVCur(current2["vc_amount"].Value);
			vCurr.Add(item2);
		}
		return this;
	}

	public override string ToString()
	{
		return $"[XVPStatus: OperationId={OperationId}, OperationType={OperationType}, OperationCreated={OperationCreated}, VcAmount={VcAmount}, BackUrl={BackUrl}, ReturnTegion={ReturnTegion}, BackUrlCaption={BackUrlCaption}, Status={Status}, Items={Items}]";
	}
}
