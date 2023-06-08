using System;
using UnityEngine;

public class SilverLiningLuminanceMapper
{
	private static double brightness = 0.8;

	public static double Ldmax = 100.0;

	public static double mR = 1.0;

	public static double mC = 1.0;

	public static double k = 1.0;

	public static double LsavgR = 100.0;

	public static double LsavgC = 100.0;

	public static bool disableToneMapping = false;

	public static void SetMaxDisplayLuminance(double nits)
	{
		if (nits > 0.0)
		{
			Ldmax = nits;
		}
	}

	public static void EnableToneMapping(bool enabled)
	{
		disableToneMapping = !enabled;
	}

	public static void SetSceneLogAvg(double rodNits, double coneNits)
	{
		LsavgR = rodNits / brightness;
		LsavgC = coneNits / brightness;
		ComputeScaleFactors();
	}

	public static void DurandMapper(ref double x, ref double y, ref double Y)
	{
		if (!disableToneMapping && y > 0.0)
		{
			double num = x * (Y / y);
			double num2 = (1.0 - x - y) * (Y / y);
			double num3 = num * -0.702 + Y * 1.039 + num2 * 0.433;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			double num4 = Y * mC;
			double num5 = num3 * mR;
			Y = num4 + k * num5;
			Y /= Ldmax;
		}
	}

	public static void DurandMapperXYZ(ref Vector3 XYZ)
	{
		if (!disableToneMapping)
		{
			double num = (double)XYZ.x * -0.702 + (double)XYZ.y * 1.039 + (double)XYZ.z * 0.433;
			if (num < 0.0)
			{
				num = 0.0;
			}
			Vector3 vector = new Vector3((float)(num * 0.3), (float)(num * 0.3), (float)(num * 0.4));
			XYZ *= (float)((1.0 - k) * mC);
			XYZ += vector * (float)(k * mR);
			XYZ *= (float)(1.0 / Ldmax);
		}
	}

	public static void GetLuminanceScales(out double rodSF, out double coneSF)
	{
		rodSF = mR;
		coneSF = mC;
	}

	public static double GetMaxDisplayLuminance()
	{
		return Ldmax;
	}

	public static double GetBurnoutLuminance()
	{
		return Ldmax / (mC + k * mR);
	}

	public static double GetRodConeBlend()
	{
		return k;
	}

	public static void GetSceneLogAvg(out double rodNits, out double coneNits)
	{
		rodNits = LsavgR;
		coneNits = LsavgC;
	}

	private static double RodThreshold(double LaR)
	{
		double num = Math.Log(LaR);
		double d = ((num <= -3.94) ? (-2.86) : ((!(num >= -1.44)) ? (Math.Pow(0.405 * num + 1.6, 2.18) - 2.86) : (num - 0.395)));
		return Math.Exp(d);
	}

	private static double ConeThreshold(double LaC)
	{
		double num = Math.Log(LaC);
		double d = ((num <= -2.6) ? (-0.72) : ((!(num >= 1.9)) ? (Math.Pow(0.249 * num + 0.65, 2.7) - 0.72) : (num - 1.255)));
		return Math.Exp(d);
	}

	private static void ComputeScaleFactors()
	{
		Ldmax = 100.0;
		double ldmax = Ldmax;
		double lsavgR = LsavgR;
		double lsavgC = LsavgC;
		double num = ConeThreshold(ldmax);
		mR = num / RodThreshold(lsavgR);
		mC = num / ConeThreshold(lsavgC);
		k = (100.0 - 0.25 * lsavgR) / (100.0 + lsavgR);
		if (k < 0.0)
		{
			k = 0.0;
		}
	}
}
