using System;
using System.Xml.Serialization;

namespace KA.Framework;

[Serializable]
public class ProductVersion
{
	[XmlElement(ElementName = "Product")]
	public ProductInfo[] Versions;

	[XmlElement(ElementName = "UpgradeStatus", IsNullable = true)]
	public UpgradeString[] UpgradeStatus;

	public string GetProductVersion()
	{
		if (Versions != null && Versions.Length != 0)
		{
			for (int i = 0; i < Versions.Length; i++)
			{
				if (ProductConfig.pProductID == Versions[i].ProductID)
				{
					return Versions[i].Version;
				}
			}
		}
		return string.Empty;
	}

	public string GetStatusString(VersionStatus status)
	{
		if (UpgradeStatus != null && UpgradeStatus.Length != 0)
		{
			for (int i = 0; i < UpgradeStatus.Length; i++)
			{
				if (status != UpgradeStatus[i].VersionState)
				{
					continue;
				}
				for (int j = 0; j < UpgradeStatus[i].ProductID.Length; j++)
				{
					if (UpgradeStatus[i].ProductID[j] == ProductConfig.pProductID)
					{
						return UpgradeStatus[i].UpgradeText.GetLocalizedString();
					}
				}
			}
		}
		return string.Empty;
	}

	public ProductInfo GetProductInfo()
	{
		if (Versions != null && Versions.Length != 0)
		{
			for (int i = 0; i < Versions.Length; i++)
			{
				if (ProductConfig.pProductID == Versions[i].ProductID)
				{
					return Versions[i];
				}
			}
		}
		return null;
	}
}
