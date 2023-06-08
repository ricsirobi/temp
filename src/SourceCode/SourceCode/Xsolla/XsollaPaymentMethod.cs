using System.Collections.Generic;
using SimpleJSON;

namespace Xsolla;

public class XsollaPaymentMethod : IXsollaObject, IParseble
{
	public enum TypePayment
	{
		QUICK,
		REGULAR
	}

	public long id { get; private set; }

	public int isHidden { get; private set; }

	public string aliases { get; private set; }

	public int isRecommended { get; private set; }

	public int[] cat { get; private set; }

	public string recurrentType { get; private set; }

	public string name { get; private set; }

	public string imgUrl { get; private set; }

	public string imgUrl2x { get; private set; }

	public bool isVisible { get; private set; }

	public TypePayment typePayment { get; private set; }

	public string GetImageUrl()
	{
		if (imgUrl2x.StartsWith("https:"))
		{
			return imgUrl2x;
		}
		return "https:" + imgUrl2x;
	}

	public string GetKey()
	{
		return id.ToString();
	}

	public string GetName()
	{
		return name;
	}

	public IParseble Parse(JSONNode paymentMethodNode)
	{
		id = paymentMethodNode["id"].AsInt;
		isHidden = paymentMethodNode["hidden"].AsInt;
		aliases = paymentMethodNode["aliases"];
		isRecommended = paymentMethodNode["recommended"].AsInt;
		IEnumerator<JSONNode> enumerator = paymentMethodNode["cat"].Childs.GetEnumerator();
		cat = new int[paymentMethodNode["cat"].Count];
		int num = 0;
		while (enumerator.MoveNext())
		{
			cat[num] = enumerator.Current.AsInt;
			num++;
		}
		recurrentType = paymentMethodNode["recurrentType"];
		name = paymentMethodNode["name"];
		imgUrl = paymentMethodNode["image_url"];
		imgUrl2x = paymentMethodNode["image_2x_url"];
		isVisible = paymentMethodNode["is_visible"].AsBool;
		return this;
	}

	public void SetType(TypePayment pType)
	{
		typePayment = pType;
	}

	public override string ToString()
	{
		return $"[XsollaPaymentMethod: id={id}, isHidden={isHidden}, aliases={aliases}, isRecommended={isRecommended}, cat={cat}, recurrentType={recurrentType}, name={name}, imgUrl={imgUrl}]";
	}
}
