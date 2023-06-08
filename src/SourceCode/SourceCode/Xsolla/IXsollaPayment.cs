using System.Collections.Generic;

namespace Xsolla;

public interface IXsollaPayment
{
	void InitPaystation(XsollaWallet wallet);

	void NextStep(Dictionary<string, object> requestParams);

	void Status(string token, long invoice);

	void SetModeSandbox(bool isSandbox);
}
