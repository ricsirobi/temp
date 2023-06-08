using System;
using UnityEngine;

public class LabColorAgentTestObject : LabSolidObject
{
	[Serializable]
	public class PHColorBracket
	{
		public int _PHValue;

		public Color _Color;

		public MinMax _ColorBracket;
	}

	public PHColorBracket[] _PHColorBracket;
}
