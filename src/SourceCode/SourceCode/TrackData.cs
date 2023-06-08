using System;
using UnityEngine;

[Serializable]
public class TrackData
{
	public LocaleString _TrackNameText;

	public LocaleString _TrackDescriptionText;

	public Themes _TrackTheme;

	public Texture _TrackIcon;

	public int _ItemID;

	public int _GameLevelID;

	public string _TrackSceneName;

	public bool _Visible = true;
}
