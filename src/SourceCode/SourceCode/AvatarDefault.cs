public class AvatarDefault
{
	public const Gender DEFAULT_GENDER = Gender.Female;

	public static AvatarIDefault GetDefaultParts()
	{
		return GetDefaultParts(Gender.Female);
	}

	public static AvatarIDefault GetDefaultParts(Gender gender)
	{
		if (gender == Gender.Female)
		{
			return new AvatarFemaleDefault();
		}
		return new AvatarMaleDefault();
	}
}
