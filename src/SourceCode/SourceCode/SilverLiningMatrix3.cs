using System;
using UnityEngine;

public class SilverLiningMatrix3
{
	public double[,] elem;

	public SilverLiningMatrix3()
	{
		elem = new double[3, 3];
	}

	public SilverLiningMatrix3(double e11, double e12, double e13, double e21, double e22, double e23, double e31, double e32, double e33)
	{
		elem = new double[3, 3];
		elem[0, 0] = e11;
		elem[0, 1] = e12;
		elem[0, 2] = e13;
		elem[1, 0] = e21;
		elem[1, 1] = e22;
		elem[1, 2] = e23;
		elem[2, 0] = e31;
		elem[2, 1] = e32;
		elem[2, 2] = e33;
	}

	public void FromRx(double rad)
	{
		double num = Math.Sin(rad);
		double num2 = Math.Cos(rad);
		elem[0, 0] = 1.0;
		elem[0, 1] = 0.0;
		elem[0, 2] = 0.0;
		elem[1, 0] = 0.0;
		elem[1, 1] = num2;
		elem[1, 2] = num;
		elem[2, 0] = 0.0;
		elem[2, 1] = 0.0 - num;
		elem[2, 2] = num2;
	}

	public void FromRy(double rad)
	{
		double num = Math.Sin(rad);
		double num2 = Math.Cos(rad);
		elem[0, 0] = num2;
		elem[0, 1] = 0.0;
		elem[0, 2] = 0.0 - num;
		elem[1, 0] = 0.0;
		elem[1, 1] = 1.0;
		elem[1, 2] = 0.0;
		elem[2, 0] = num;
		elem[2, 1] = 0.0;
		elem[2, 2] = num2;
	}

	public void FromRz(double rad)
	{
		double num = Math.Sin(rad);
		double num2 = Math.Cos(rad);
		elem[0, 0] = num2;
		elem[0, 1] = num;
		elem[0, 2] = 0.0;
		elem[1, 0] = 0.0 - num;
		elem[1, 1] = num2;
		elem[1, 2] = 0.0;
		elem[2, 0] = 0.0;
		elem[2, 1] = 0.0;
		elem[2, 2] = 1.0;
	}

	public void FromXYZ(double Rx, double Ry, double Rz)
	{
		SilverLiningMatrix3 silverLiningMatrix = new SilverLiningMatrix3();
		SilverLiningMatrix3 silverLiningMatrix2 = new SilverLiningMatrix3();
		SilverLiningMatrix3 silverLiningMatrix3 = new SilverLiningMatrix3();
		silverLiningMatrix.FromRx(Rx);
		silverLiningMatrix2.FromRy(Ry);
		silverLiningMatrix3.FromRz(Rz);
		SilverLiningMatrix3 silverLiningMatrix4 = silverLiningMatrix * (silverLiningMatrix2 * silverLiningMatrix3);
		elem = silverLiningMatrix4.elem;
	}

	public static Vector3 operator *(SilverLiningMatrix3 m, Vector3 rkPoint)
	{
		Vector3 result = default(Vector3);
		result.x = (float)m.elem[0, 0] * rkPoint.x + (float)m.elem[0, 1] * rkPoint.y + (float)m.elem[0, 2] * rkPoint.z;
		result.y = (float)m.elem[1, 0] * rkPoint.x + (float)m.elem[1, 1] * rkPoint.y + (float)m.elem[1, 2] * rkPoint.z;
		result.z = (float)m.elem[2, 0] * rkPoint.x + (float)m.elem[2, 1] * rkPoint.y + (float)m.elem[2, 2] * rkPoint.z;
		return result;
	}

	public static SilverLiningMatrix3 operator *(SilverLiningMatrix3 m1, SilverLiningMatrix3 m2)
	{
		SilverLiningMatrix3 silverLiningMatrix = new SilverLiningMatrix3();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				silverLiningMatrix.elem[i, j] = m1.elem[i, 0] * m2.elem[0, j] + m1.elem[i, 1] * m2.elem[1, j] + m1.elem[i, 2] * m2.elem[2, j];
			}
		}
		return silverLiningMatrix;
	}

	public SilverLiningMatrix3 Transpose()
	{
		SilverLiningMatrix3 silverLiningMatrix = new SilverLiningMatrix3();
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				silverLiningMatrix.elem[i, j] = elem[j, i];
			}
		}
		return silverLiningMatrix;
	}

	public static Vector3 operator *(Vector3 rkPoint, SilverLiningMatrix3 rkMatrix)
	{
		Vector3 result = default(Vector3);
		result.x = rkPoint.x * (float)rkMatrix.elem[0, 0] + rkPoint.y * (float)rkMatrix.elem[1, 0] + rkPoint.z * (float)rkMatrix.elem[2, 0];
		result.y = rkPoint.x * (float)rkMatrix.elem[0, 1] + rkPoint.y * (float)rkMatrix.elem[1, 1] + rkPoint.z * (float)rkMatrix.elem[2, 1];
		result.z = rkPoint.x * (float)rkMatrix.elem[0, 2] + rkPoint.y * (float)rkMatrix.elem[1, 2] + rkPoint.z * (float)rkMatrix.elem[2, 2];
		return result;
	}
}
