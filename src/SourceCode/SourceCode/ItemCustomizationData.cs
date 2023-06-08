using System;

[Serializable]
public class ItemCustomizationData
{
	public string _ShaderPropName;

	public CustomizePropertyType _Type;

	public bool _UseGroupLogo;

	public LogoColorSource _LogoColorSource = LogoColorSource.BACKGROUND_COLOR;

	[NonSerialized]
	public string _Value = "";

	public ItemCustomizationData GetCopy()
	{
		return new ItemCustomizationData
		{
			_ShaderPropName = _ShaderPropName,
			_Type = _Type,
			_UseGroupLogo = _UseGroupLogo,
			_LogoColorSource = _LogoColorSource,
			_Value = _Value
		};
	}
}
