using System;
using System.Xml.Serialization;

[Serializable]
public class AssetVersion
{
	public class Variant
	{
		[XmlAttribute(AttributeName = "L")]
		public string Locale = "";

		[XmlAttribute(AttributeName = "N")]
		public int Version;

		[XmlAttribute(AttributeName = "S")]
		public float FileSize;
	}

	[XmlAttribute(AttributeName = "N")]
	public string AssetName;

	[XmlElement(ElementName = "V")]
	public Variant[] Variants;

	public static AssetVersion CreateDefault(string assetName)
	{
		AssetVersion obj = new AssetVersion
		{
			AssetName = assetName,
			Variants = new Variant[1]
		};
		obj.Variants[0] = new Variant();
		obj.Variants[0].Locale = string.Empty;
		return obj;
	}

	public Variant GetClosestVariant(string locale)
	{
		if (Variants == null)
		{
			return null;
		}
		if (locale.Equals("en-us", StringComparison.OrdinalIgnoreCase))
		{
			locale = string.Empty;
		}
		Variant result = null;
		Variant[] variants = Variants;
		foreach (Variant variant in variants)
		{
			if (locale.Equals(variant.Locale, StringComparison.OrdinalIgnoreCase))
			{
				return variant;
			}
			if (string.IsNullOrEmpty(variant.Locale))
			{
				result = variant;
			}
		}
		return result;
	}

	public string GetAssetNameForLocale(string locale)
	{
		Variant closestVariant = GetClosestVariant(locale);
		if (closestVariant != null && !string.IsNullOrEmpty(closestVariant.Locale))
		{
			int num = AssetName.LastIndexOf('.');
			if (num != -1)
			{
				return AssetName.Substring(0, num) + "." + closestVariant.Locale + AssetName.Substring(num, AssetName.Length - num);
			}
			return AssetName + "." + closestVariant.Locale;
		}
		return AssetName;
	}

	public float GetFileSize(string locale)
	{
		return GetClosestVariant(locale)?.FileSize ?? 0f;
	}

	public int GetVersion(string locale)
	{
		return GetClosestVariant(locale)?.Version ?? 0;
	}
}
