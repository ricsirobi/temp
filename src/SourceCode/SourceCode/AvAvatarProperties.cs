using UnityEngine;

public class AvAvatarProperties : MonoBehaviour
{
	public Texture _DefaultSkin;

	public GameObject _NavTarget;

	public GameObject _NavArrow;

	public bool _SaveIfStateBlocked;

	public bool _DisableAvatarScale;

	public bool _ResetAvatarScale;

	public string _DefaultCountryURL = "";

	public string _DefaultGenderURL = "";

	public GameObject _BusyProp;

	public Color _DisplayNameTextDefaultColor = Color.white;

	public Color _DisplayNameTextNicknameColor = Color.yellow;

	public Color _DisplayNameShadowColor = Color.black;

	public PropStates[] _PropStates;

	public bool IsScaleDisabled()
	{
		return _DisableAvatarScale;
	}
}
