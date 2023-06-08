using System;

namespace KA.Framework;

[Serializable]
public class ProductInfo
{
	public int ProductID = -1;

	public string Version = "";

	public UpgradeStatus UpgradeState;
}
