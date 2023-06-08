namespace JSGames.GlyphMapping;

public class ThaiGlyphCorrector : GlyphCorrector
{
	protected override string pGlyphConfigBundlePath => null;

	public override string GetCharMapping(string text)
	{
		return ThaiFontAdjuster.Adjust(text);
	}
}
