using System;

namespace KA.Framework;

[Serializable]
public class TextDataByLoadType
{
	public LoadingTextType _Type;

	public TextData[] _DataBySceneName;

	public float _DelayBwLoadTextsInSecs;
}
