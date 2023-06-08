using System;

namespace KA.Framework.ThirdParty;

[Serializable]
public class PluginKey
{
	public ProductPlatform _Platform;

	public Environment _Environment;

	public BuildType _Type;

	public int _MaxAge = int.MaxValue;

	public KeyData _KeyData;

	public PluginParam[] _Params;
}
