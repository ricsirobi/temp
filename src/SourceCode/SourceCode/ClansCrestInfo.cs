using UnityEngine;

public class ClansCrestInfo
{
	public string Logo;

	public Texture CrestIcon;

	public Color ColorFG;

	public Color ColorBG;

	public void Copy(ClansCrestInfo inInfo)
	{
		Logo = inInfo.Logo;
		CrestIcon = inInfo.CrestIcon;
		ColorFG = inInfo.ColorFG;
		ColorBG = inInfo.ColorBG;
	}
}
