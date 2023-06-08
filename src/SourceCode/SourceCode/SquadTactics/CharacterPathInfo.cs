using System;

namespace SquadTactics;

[Serializable]
public class CharacterPathInfo
{
	public Character _TargetCharacter;

	public Path _Path;

	public CharacterPathInfo(Character targetCharacter, Path path)
	{
		_TargetCharacter = targetCharacter;
		_Path = path;
	}
}
