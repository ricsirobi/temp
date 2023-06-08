public class SilverLiningLunarSpectrum : SilverLiningSpectrum
{
	public SilverLiningLunarSpectrum()
	{
		double num = 700.0;
		double num2 = 1350.0;
		for (int i = 0; i < 81; i++)
		{
			double num3 = (double)i / 81.0;
			powers[i] = num * (1.0 - num3) + num2 * num3;
		}
	}
}
