using System;

[Serializable]
public class KASkinSpriteInfo
{
	public bool _UseSprite;

	public KASkinSprite[] _Sprites;

	public void ChangeWidgetSprite(bool inShowEffect, KAWidget widget = null)
	{
		if (!_UseSprite || _Sprites == null)
		{
			return;
		}
		KASkinSprite[] sprites = _Sprites;
		foreach (KASkinSprite kASkinSprite in sprites)
		{
			UISprite uISprite = kASkinSprite._ApplyTo as UISprite;
			if (uISprite != null)
			{
				uISprite.spriteName = (inShowEffect ? kASkinSprite._SpriteName : uISprite.pOrgSprite);
			}
			else
			{
				UtDebug.LogWarning("missing reference in SpriteInfo for " + ((widget == null) ? string.Empty : widget.name));
			}
		}
	}
}
