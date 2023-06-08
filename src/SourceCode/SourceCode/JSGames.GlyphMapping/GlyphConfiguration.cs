using System;
using System.Collections.Generic;
using UnityEngine;

namespace JSGames.GlyphMapping;

public class GlyphConfiguration : ScriptableObject
{
	[Serializable]
	public class CharacterMapping
	{
		public string _Char;

		public string _UnicodeHex;
	}

	public List<CharacterMapping> _CharacterMapping = new List<CharacterMapping>();
}
