using System;
using UnityEngine;

public class SilverLiningSpectrum
{
	protected const int NSAMPLES = 81;

	protected const int SAMPLEWIDTH = 5;

	public int redshift;

	public double airMassMultiplier = 1.0;

	public double H = 8435.0;

	public double O3 = 0.35;

	public double W = 2.5;

	public double rho;

	protected double[] powers = new double[81];

	protected double PI = 3.14159265;

	protected double[,] cie_colour_match = new double[81, 3]
	{
		{ 0.0014, 0.0, 0.0065 },
		{ 0.0022, 0.0001, 0.0105 },
		{ 0.0042, 0.0001, 0.0201 },
		{ 0.0076, 0.0002, 0.0362 },
		{ 0.0143, 0.0004, 0.0679 },
		{ 0.0232, 0.0006, 0.1102 },
		{ 0.0435, 0.0012, 0.2074 },
		{ 0.0776, 0.0022, 0.3713 },
		{ 0.1344, 0.004, 0.6456 },
		{ 0.2148, 0.0073, 1.0391 },
		{ 0.2839, 0.0116, 1.3856 },
		{ 0.3285, 0.0168, 1.623 },
		{ 0.3483, 0.023, 1.7471 },
		{ 0.3481, 0.0298, 1.7826 },
		{ 0.3362, 0.038, 1.7721 },
		{ 0.3187, 0.048, 1.7441 },
		{ 0.2908, 0.06, 1.6692 },
		{ 0.2511, 0.0739, 1.5281 },
		{ 0.1954, 0.091, 1.2876 },
		{ 0.1421, 0.1126, 1.0419 },
		{ 0.0956, 0.139, 0.813 },
		{ 0.058, 0.1693, 0.6162 },
		{ 0.032, 0.208, 0.4652 },
		{ 0.0147, 0.2586, 0.3533 },
		{ 0.0049, 0.323, 0.272 },
		{ 0.0024, 0.4073, 0.2123 },
		{ 0.0093, 0.503, 0.1582 },
		{ 0.0291, 0.6082, 0.1117 },
		{ 0.0633, 0.71, 0.0782 },
		{ 0.1096, 0.7932, 0.0573 },
		{ 0.1655, 0.862, 0.0422 },
		{ 0.2257, 0.9149, 0.0298 },
		{ 0.2904, 0.954, 0.0203 },
		{ 0.3597, 0.9803, 0.0134 },
		{ 0.4334, 0.995, 0.0087 },
		{ 0.5121, 1.0, 0.0057 },
		{ 0.5945, 0.995, 0.0039 },
		{ 0.6784, 0.9786, 0.0027 },
		{ 0.7621, 0.952, 0.0021 },
		{ 0.8425, 0.9154, 0.0018 },
		{ 0.9163, 0.87, 0.0017 },
		{ 0.9786, 0.8163, 0.0014 },
		{ 1.0263, 0.757, 0.0011 },
		{ 1.0567, 0.6949, 0.001 },
		{ 1.0622, 0.631, 0.0008 },
		{ 1.0456, 0.5668, 0.0006 },
		{ 1.0026, 0.503, 0.0003 },
		{ 0.9384, 0.4412, 0.0002 },
		{ 0.8544, 0.381, 0.0002 },
		{ 0.7514, 0.321, 0.0001 },
		{ 0.6424, 0.265, 0.0 },
		{ 0.5419, 0.217, 0.0 },
		{ 0.4479, 0.175, 0.0 },
		{ 0.3608, 0.1382, 0.0 },
		{ 0.2835, 0.107, 0.0 },
		{ 0.2187, 0.0816, 0.0 },
		{ 0.1649, 0.061, 0.0 },
		{ 0.1212, 0.0446, 0.0 },
		{ 0.0874, 0.032, 0.0 },
		{ 0.0636, 0.0232, 0.0 },
		{ 0.0468, 0.017, 0.0 },
		{ 0.0329, 0.0119, 0.0 },
		{ 0.0227, 0.0082, 0.0 },
		{ 0.0158, 0.0057, 0.0 },
		{ 0.0114, 0.0041, 0.0 },
		{ 0.0081, 0.0029, 0.0 },
		{ 0.0058, 0.0021, 0.0 },
		{ 0.0041, 0.0015, 0.0 },
		{ 0.0029, 0.001, 0.0 },
		{ 0.002, 0.0007, 0.0 },
		{ 0.0014, 0.0005, 0.0 },
		{ 0.001, 0.0004, 0.0 },
		{ 0.0007, 0.0002, 0.0 },
		{ 0.0005, 0.0002, 0.0 },
		{ 0.0003, 0.0001, 0.0 },
		{ 0.0002, 0.0001, 0.0 },
		{ 0.0002, 0.0001, 0.0 },
		{ 0.0001, 0.0, 0.0 },
		{ 0.0001, 0.0, 0.0 },
		{ 0.0001, 0.0, 0.0 },
		{ 0.0, 0.0, 0.0 }
	};

	protected double[] Ao = new double[81]
	{
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.003, 0.004, 0.006, 0.007, 0.009, 0.011,
		0.014, 0.017, 0.021, 0.025, 0.03, 0.035, 0.04, 0.044, 0.048, 0.055,
		0.063, 0.071, 0.075, 0.08, 0.085, 0.091, 0.12, 0.12, 0.12, 0.12,
		0.12, 0.12, 0.12, 0.119, 0.12, 0.12, 0.12, 0.1, 0.09, 0.09,
		0.085, 0.08, 0.075, 0.07, 0.07, 0.065, 0.06, 0.055, 0.05, 0.045,
		0.04, 0.035, 0.028, 0.25, 0.023, 0.02, 0.018, 0.016, 0.012, 0.012,
		0.012, 0.012, 0.01, 0.01, 0.01, 0.008, 0.007, 0.006, 0.005, 0.003,
		0.0
	};

	protected double[] Au = new double[81]
	{
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.15, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 4.0, 0.0, 0.0, 0.0,
		0.0
	};

	protected double[] Aw = new double[81]
	{
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.075, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
		0.0, 0.0, 0.016, 0.015, 0.014, 0.013, 0.0125, 1.8, 2.3, 2.5,
		2.3, 1.8, 0.061, 0.003, 0.0008, 0.0001, 1E-05, 1E-05, 0.0001, 0.0003,
		0.0006
	};

	protected double DEGREES(double x)
	{
		return x * (180.0 / PI);
	}

	protected double RADIANS(double x)
	{
		return x * (PI / 180.0);
	}

	public SilverLiningSpectrum()
	{
		H *= SilverLining.unitScale;
	}

	public Vector3 ToXYZ()
	{
		Vector3 result = new Vector3(0f, 0f, 0f);
		for (int i = 1; i < 81; i++)
		{
			int num = i + redshift;
			if (num > 80)
			{
				num = 80;
			}
			if (num < 1)
			{
				num = 1;
			}
			result.x += (float)(powers[num] * cie_colour_match[i, 0]);
			result.y += (float)(powers[num] * cie_colour_match[i, 1]);
			result.z += (float)(powers[num] * cie_colour_match[i, 2]);
		}
		return result;
	}

	public static SilverLiningSpectrum operator *(SilverLiningSpectrum s1, SilverLiningSpectrum s2)
	{
		SilverLiningSpectrum silverLiningSpectrum = new SilverLiningSpectrum();
		for (int i = 0; i < 81; i++)
		{
			silverLiningSpectrum.powers[i] = s1.powers[i] * s2.powers[i];
		}
		return silverLiningSpectrum;
	}

	public void ApplyAtmosphericTransmittance(double zenithAngle, double cosZenith, double T, double alt, ref SilverLiningSpectrum directIrradiance, ref SilverLiningSpectrum scatteredIrradiance)
	{
		double num = 0.04608 * T - 0.04586;
		double num2 = DEGREES(zenithAngle);
		if (num2 < 90.0)
		{
			double num3 = 1.0 / (cosZenith + 0.50572 * Math.Pow(96.07995 - num2, -1.6364));
			num3 *= airMassMultiplier;
			H *= SilverLining.unitScale;
			double num4 = Math.Exp(0.0 - alt / H);
			num3 *= num4;
			double num5 = 1.003454 / Math.Sqrt(cosZenith * cosZenith + 0.006908);
			double num6 = Math.Log(0.35);
			double num7 = num6 * (1.459 + num6 * (0.1595 + num6 * 0.4129));
			double num8 = num6 * (0.0783 + num6 * (-0.3824 - num6 * 0.5874));
			double num9 = 1.0 - 0.5 * Math.Exp((num7 + num8 / 1.8) / 1.8);
			double num10 = 1.0 - 0.5 * Math.Exp((num7 + num8 * cosZenith) * cosZenith);
			for (int i = 0; i < 81; i++)
			{
				double num11 = 0.38 + (double)i * 0.005;
				double num12 = Math.Exp((0.0 - num3) / (Math.Pow(num11, 4.0) * (115.6406 - 1.3366 / (num11 * num11))));
				double num13 = ((!(num11 < 0.5)) ? 1.206 : 1.0274);
				double num14 = num * Math.Pow(2.0 * num11, 0.0 - num13);
				double num15 = Math.Exp((0.0 - num14) * num3);
				double num16 = Aw[i] * W * num3;
				double num17 = Math.Exp(-0.2385 * num16 / Math.Pow(1.0 + 20.07 * num16, 0.45));
				double num18 = Math.Exp((0.0 - Ao[i]) * O3 * num5);
				double num19 = Math.Exp(-1.41 * Au[i] * num3 / Math.Pow(1.0 + 118.3 * Au[i] * num3, 0.45));
				double num20 = Math.Log(num11 / 0.4);
				double num21 = 0.945 * Math.Exp(-0.095 * num20 * num20);
				double num22 = Math.Exp((0.0 - num21) * num14 * num3);
				double num23 = Math.Exp((num21 - 1.0) * num14 * num3);
				double num24 = Math.Exp(-1.8 / (num11 * num11 * num11 * num11) * (115.6406 - 1.3366 / (num11 * num11)));
				double num25 = Math.Exp(-0.4293 * Aw[i] * W / Math.Pow(1.0 + 36.126 * Aw[i] * W, 0.45));
				double num26 = Math.Exp(-2.538 * Au[i] / Math.Pow(1.0 + 212.94 * Au[i], 0.45));
				double num27 = Math.Exp((0.0 - num21) * num14 * 1.8);
				double num28 = Math.Exp((num21 - 1.0) * num14 * 1.8);
				double num29 = num12 * num15 * num17 * num18 * num19;
				directIrradiance.powers[i] = powers[i] * num29;
				double num30 = powers[i] * num18 * num17 * num19 * cosZenith * num23;
				double num31 = 1.0;
				if (num11 <= 0.45)
				{
					num31 = Math.Pow(num11 + 0.55, 1.8);
				}
				double num32 = num25 * num26 * num28 * (0.5 * (1.0 - num24) + (1.0 - num9) * num24 * (1.0 - num27));
				double num33 = num30 * (1.0 - Math.Pow(num12, 0.95)) / 2.0;
				double num34 = num30 * Math.Pow(num12, 1.5) * (1.0 - num22) * num10;
				double num35 = (directIrradiance.powers[i] * cosZenith + num33 + num34) * rho * num32 / (1.0 - rho * num32);
				scatteredIrradiance.powers[i] = (num33 + num34 + num35) * num31;
				if (scatteredIrradiance.powers[i] < 0.0)
				{
					scatteredIrradiance.powers[i] = 0.0;
				}
			}
		}
		else
		{
			for (int j = 0; j < 81; j++)
			{
				directIrradiance.powers[j] = 0.0;
				scatteredIrradiance.powers[j] = 0.0;
			}
		}
	}
}
