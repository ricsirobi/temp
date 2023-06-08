using SimpleJSON;

namespace Xsolla;

public class XsollaSavedPaymentMethod : IXsollaObject, IParseble
{
	private long id { get; set; }

	private string type { get; set; }

	private string currency { get; set; }

	private string name { get; set; }

	private long pid { get; set; }

	private string reccurentType { get; set; }

	private SavedMethodForm form { get; set; }

	private bool replaced { get; set; }

	private string psName { get; set; }

	private string iconSrc { get; set; }

	private bool isSelected { get; set; }

	public string GetImageUrl()
	{
		if (iconSrc.StartsWith("https:"))
		{
			return iconSrc;
		}
		return "https:" + iconSrc;
	}

	public string GetKey()
	{
		return id.ToString();
	}

	public string GetMethodType()
	{
		return type;
	}

	public long GetPid()
	{
		return pid;
	}

	public string GetName()
	{
		return name;
	}

	public string GetPsName()
	{
		return psName;
	}

	public object GetForm()
	{
		return form;
	}

	public string GetFormSid()
	{
		if (form != null)
		{
			return form.getPaymentSid();
		}
		return "";
	}

	public string GetCurrency()
	{
		return currency;
	}

	public IParseble Parse(JSONNode pJsonNode)
	{
		id = pJsonNode["id"].AsInt;
		type = pJsonNode["type"];
		currency = pJsonNode["currency"];
		name = pJsonNode["name"];
		pid = pJsonNode["pid"].AsInt;
		reccurentType = pJsonNode["reccurentType"];
		form = new SavedMethodForm().Parse(pJsonNode["form"]) as SavedMethodForm;
		replaced = pJsonNode["replaced"].AsBool;
		psName = pJsonNode["psName"];
		iconSrc = pJsonNode["iconSrc"];
		isSelected = pJsonNode["isSelected"].AsBool;
		return this;
	}

	public override string ToString()
	{
		return $"[XsollaSavedPaymentMethod: id={id}, type={type}, currency={currency}, name={name}, pid={pid}, reccurentType={reccurentType}, form={form}, replaced={replaced}, psName={psName}, iconSrc={iconSrc}, isSelected={isSelected}]";
	}

	public override bool Equals(object obj)
	{
		if (id == (obj as XsollaSavedPaymentMethod).id)
		{
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
