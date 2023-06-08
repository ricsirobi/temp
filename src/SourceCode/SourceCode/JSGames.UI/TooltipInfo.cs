using System;
using UnityEngine;

namespace JSGames.UI;

[Serializable]
public class TooltipInfo
{
	public bool _HideTooltip;

	public float _Duration = 0.5f;

	public float _InitialAlpha = 0.1f;

	public float _InitialScale = 0.1f;

	public float _FinalScale = 1f;

	public Vector2 _Offset = Vector2.zero;

	public LocaleString _Text = new LocaleString("");

	public Font _Font;

	public int _FontSize = 20;

	public Color _TextColor = Color.white;

	public SnSound _Sound;

	public Sprite _BackgroundImage;

	public Color _Color = Color.white;

	public TooltipStyle _Style;
}
