using System.Collections.Generic;
using System.Text;
using SimpleJSON;

namespace Xsolla;

public class XsollaForm : IParseble
{
	public enum CurrentCommand
	{
		FORM,
		CREATE,
		STATUS,
		CHECKOUT,
		CHECK,
		ACCOUNT,
		UNKNOWN
	}

	private string xpsPrefix = "xps_";

	private int size;

	private string account;

	private string accountXsolla;

	private string currentCommand;

	private string title;

	private string iconUrl;

	private string currency;

	private string sum;

	private string instruction;

	private int pid;

	private bool skipForm;

	private bool skipCheckout;

	private string checkoutToken;

	private XsollaError xsollaError;

	private XsollaMessage xsollaMessage;

	private XsollaSummary summary;

	private XsollaCheckout chekout;

	private BuyData buyData;

	private List<XsollaFormElement> elements;

	private List<XsollaFormElement> elementsVisible;

	private Dictionary<string, XsollaFormElement> map;

	private Dictionary<string, object> xpsMap;

	public XsollaForm()
	{
		elements = new List<XsollaFormElement>();
		elementsVisible = new List<XsollaFormElement>();
		map = new Dictionary<string, XsollaFormElement>();
		xpsMap = new Dictionary<string, object>();
	}

	public void AddElement(XsollaFormElement xsollaFormElement)
	{
		elements.Add(xsollaFormElement);
		if (xsollaFormElement.IsVisible())
		{
			elementsVisible.Add(xsollaFormElement);
		}
		map.Add(xsollaFormElement.GetName(), xsollaFormElement);
		if (xsollaFormElement.GetName() != null)
		{
			xpsMap.Add(xpsPrefix + xsollaFormElement.GetName(), xsollaFormElement.GetValue());
		}
		size++;
	}

	public void UpdateElement(string name, string newValue)
	{
		if (map.ContainsKey(name))
		{
			GetItem(name).SetValue(newValue);
			string key = xpsPrefix + name;
			xpsMap[key] = newValue;
		}
		else
		{
			string key2 = xpsPrefix + name;
			xpsMap.Add(key2, newValue);
		}
	}

	public int getPid()
	{
		return pid;
	}

	public bool Contains(string name)
	{
		return map.ContainsKey(name);
	}

	public void Clear()
	{
		elements.Clear();
		elementsVisible.Clear();
		map.Clear();
		xpsMap.Clear();
	}

	public XsollaFormElement GetItem(string name)
	{
		if (map.ContainsKey(name))
		{
			return map[name];
		}
		return null;
	}

	public List<XsollaFormElement> GetVisible()
	{
		List<XsollaFormElement> resList = new List<XsollaFormElement>();
		XsollaFormElement couponCode = null;
		elementsVisible.ForEach(delegate(XsollaFormElement item)
		{
			if (item.GetName().Equals("couponCode"))
			{
				couponCode = item;
			}
			else
			{
				resList.Add(item);
			}
		});
		if (couponCode != null)
		{
			resList.Add(couponCode);
		}
		return resList;
	}

	public Dictionary<string, object> GetXpsMap()
	{
		return xpsMap;
	}

	public XsollaSummary GetSummary()
	{
		return summary;
	}

	public XsollaCheckout GetCheckout()
	{
		return chekout;
	}

	public bool IsCardPayment()
	{
		if (pid != 26 && pid != 490)
		{
			return pid == 1380;
		}
		return true;
	}

	public bool IsValidPaymentSystem()
	{
		if (pid != 26)
		{
			return pid == 1380;
		}
		return true;
	}

	public bool IsCreditCard()
	{
		if (map.ContainsKey("card_number"))
		{
			return map.ContainsKey("card_year");
		}
		return false;
	}

	public string GetAccount()
	{
		return account;
	}

	public string GetAccountXsolla()
	{
		return accountXsolla;
	}

	public CurrentCommand GetCurrentCommand()
	{
		Logger.Log("GetCurrentCommand " + currentCommand);
		return currentCommand switch
		{
			"form" => CurrentCommand.FORM, 
			"create" => CurrentCommand.CREATE, 
			"checkout" => CurrentCommand.CHECKOUT, 
			"check" => CurrentCommand.CHECK, 
			"status" => CurrentCommand.STATUS, 
			"account" => CurrentCommand.ACCOUNT, 
			_ => CurrentCommand.UNKNOWN, 
		};
	}

	public string GetInstruction()
	{
		return instruction;
	}

	public XsollaError GetError()
	{
		return xsollaError;
	}

	public XsollaMessage GetMessage()
	{
		return xsollaMessage;
	}

	public string GetTitle()
	{
		if (title == null)
		{
			return "";
		}
		return title;
	}

	public string GetSumTotal()
	{
		if (buyData != null && buyData.sum != null && buyData.currency != null)
		{
			return PriceFormatter.Format(buyData.sum, buyData.currency);
		}
		if (summary != null)
		{
			return PriceFormatter.Format(summary.GetFinance().total.amount, summary.GetFinance().total.currency);
		}
		if (sum != null && currency != null)
		{
			return PriceFormatter.Format(sum, currency);
		}
		return "";
	}

	public string GetNextStepString()
	{
		if ("1".Equals(buyData.isVisible))
		{
			return buyData.value;
		}
		return "Continue";
	}

	public string GetIconUrl()
	{
		if (iconUrl.StartsWith("https:"))
		{
			return iconUrl;
		}
		return "https:" + iconUrl;
	}

	public string GetCheckoutToken()
	{
		if (checkoutToken == null)
		{
			return "";
		}
		return checkoutToken.ToString();
	}

	public bool GetSkipForm()
	{
		return skipForm;
	}

	public bool GetSkipChekout()
	{
		return skipCheckout;
	}

	private void SetAccount(string account)
	{
		this.account = account;
	}

	private void SetAccountXsolla(string accountXsolla)
	{
		this.accountXsolla = accountXsolla;
	}

	private void SetCurrentCommand(string currentCommand)
	{
		this.currentCommand = currentCommand;
	}

	private void SetTitle(string title)
	{
		this.title = title;
	}

	private void SetIconUrl(string iconUrl)
	{
		this.iconUrl = iconUrl;
	}

	private void SetCurrency(string currency)
	{
		this.currency = currency;
	}

	private void SetSum(string sum)
	{
		this.sum = sum;
	}

	private void SetInstruction(string instruction)
	{
		this.instruction = instruction;
	}

	private void SetPid(int pid)
	{
		this.pid = pid;
	}

	private void SetSkipForm(bool pValue)
	{
		skipForm = pValue;
	}

	private void SetSkipCheckout(bool pValue)
	{
		skipCheckout = pValue;
	}

	public IParseble Parse(JSONNode rootNode)
	{
		JSONNode jSONNode = rootNode["errors"];
		if (jSONNode.Count > 0)
		{
			xsollaError = new XsollaError().ParseNew(jSONNode[0]) as XsollaError;
		}
		JSONNode jSONNode2 = rootNode["messages"];
		if (jSONNode2.Count > 0)
		{
			xsollaMessage = new XsollaMessage().Parse(jSONNode2[0]) as XsollaMessage;
		}
		SetAccount(rootNode["account"]);
		SetAccountXsolla(rootNode["accountXsolla"]);
		SetCurrentCommand(rootNode["currentCommand"]);
		SetTitle(rootNode["title"]);
		SetIconUrl(rootNode["iconUrl"]);
		SetCurrency(rootNode["currency"]);
		SetSum(rootNode["buyData"]["sum"]);
		SetInstruction(rootNode["instruction"]);
		SetPid(rootNode["pid"].AsInt);
		SetSkipForm(rootNode["skipForm"].AsBool);
		SetSkipCheckout(rootNode["skipCheckout"].AsBool);
		checkoutToken = rootNode["checkoutToken"];
		JSONNode jSONNode3 = rootNode["buyData"];
		if (jSONNode3 != null && !"null".Equals(jSONNode3))
		{
			buyData = new BuyData().Parse(jSONNode3) as BuyData;
		}
		JSONNode jSONNode4 = rootNode["summary"];
		if (jSONNode4 != null && !"null".Equals(jSONNode4))
		{
			summary = new XsollaSummary().Parse(jSONNode4) as XsollaSummary;
		}
		JSONNode jSONNode5 = rootNode["checkout"];
		if (jSONNode5 != null && !"null".Equals(jSONNode5))
		{
			chekout = new XsollaCheckout().Parse(jSONNode5) as XsollaCheckout;
		}
		IEnumerator<JSONNode> enumerator = rootNode["form"].Childs.GetEnumerator();
		Clear();
		while (enumerator.MoveNext())
		{
			AddElement((XsollaFormElement)new XsollaFormElement().Parse(enumerator.Current));
		}
		return this;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (XsollaFormElement element in elements)
		{
			stringBuilder.Append("\n/").Append(element.ToString());
		}
		return string.Format("[XsollaForm]\n currentCommand= " + currentCommand + "\n title= " + title + "\n iconUrl= " + iconUrl + "\n currency= " + currency + "\n sum= " + sum + "\n instruction= " + instruction + "\n pid= " + pid + "\n xsollaError= " + xsollaError?.ToString() + "\n elements= " + stringBuilder.ToString());
	}
}
