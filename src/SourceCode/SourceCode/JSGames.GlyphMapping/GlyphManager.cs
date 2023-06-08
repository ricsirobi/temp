namespace JSGames.GlyphMapping;

public class GlyphManager
{
	public GlyphCorrector GetGlyphCorrector(string languageCode)
	{
		GlyphCorrector result = null;
		if (languageCode.Equals("th-TH"))
		{
			result = new ThaiGlyphCorrector();
		}
		return result;
	}
}
