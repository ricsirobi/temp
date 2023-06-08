namespace StatsMonitor;

internal sealed class SMAnchors
{
	internal SMAnchor upperLeft = new SMAnchor(0f, 0f, 0f, 1f, 0f, 1f, 0f, 1f);

	internal SMAnchor upperCenter = new SMAnchor(0f, 0f, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f);

	internal SMAnchor upperRight = new SMAnchor(0f, 0f, 1f, 1f, 1f, 1f, 1f, 1f);

	internal SMAnchor middleRight = new SMAnchor(0f, 0f, 1f, 0.5f, 1f, 0.5f, 1f, 0.5f);

	internal SMAnchor lowerRight = new SMAnchor(0f, 0f, 1f, 0f, 1f, 0f, 1f, 0f);

	internal SMAnchor lowerCenter = new SMAnchor(0f, 0f, 0.5f, 0f, 0.5f, 0f, 0.5f, 0f);

	internal SMAnchor lowerLeft = new SMAnchor(0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);

	internal SMAnchor middleLeft = new SMAnchor(0f, 0f, 0f, 0.5f, 0f, 0.5f, 0f, 0.5f);
}
