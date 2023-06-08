namespace JSGames.GlyphMapping;

public abstract class GlyphCorrector
{
	protected GlyphConfiguration mGlyphConfiguration;

	protected virtual string pGlyphConfigBundlePath => null;

	public void Initialize()
	{
		if (!string.IsNullOrEmpty(pGlyphConfigBundlePath))
		{
			string[] array = pGlyphConfigBundlePath.Split('/');
			if (array.Length == 3)
			{
				RsResourceManager.Load(array[0] + "/" + array[1], OnGlyphMappingLoaded);
			}
		}
	}

	private void OnGlyphMappingLoaded(string url, RsResourceLoadEvent loadEvent, float progress, object obj, object inUserData)
	{
		switch (loadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mGlyphConfiguration = (GlyphConfiguration)RsResourceManager.LoadAssetFromBundle(pGlyphConfigBundlePath);
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Failed to load the glyph mapping " + url);
			break;
		}
	}

	public abstract string GetCharMapping(string text);
}
