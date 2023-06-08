using System;
using UnityEngine;

[Serializable]
public class KATooltipInfo
{
	public bool _ShowUI;

	public bool _UpdatePosition = true;

	public float _Duration = 0.5f;

	public float _InitialAlpha = 0.1f;

	public float _InitialScale = 0.1f;

	public float _FinalScale = 1f;

	public LocaleString _Text = new LocaleString("");

	public SnSound _Sound;

	public Vector2 _Offset = Vector2.zero;

	public Vector2 _Padding = Vector2.zero;

	public UIAtlas _Atlas;

	public string _BackgroundSprite;

	public Color _Color = Color.white;

	public UIFont _Font;

	public Color _TextColor = Color.white;

	public TooltipStyle _Style;
}
