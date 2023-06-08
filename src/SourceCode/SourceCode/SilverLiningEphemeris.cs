using System;
using UnityEngine;

public class SilverLiningEphemeris
{
	private enum Planets
	{
		MERCURY,
		VENUS,
		EARTH,
		MARS,
		JUPITER,
		SATURN,
		NUM_PLANETS
	}

	protected struct Planet
	{
		public double lastEpochDaysCalculated;

		public double rightAscension;

		public double declination;

		public double visualMagnitude;
	}

	protected struct OrbitalElements
	{
		public double period;

		public double epochLongitude;

		public double perihelionLongitude;

		public double eccentricity;

		public double semiMajorAxis;

		public double inclination;

		public double longitudeAscendingNode;

		public double angularDiameter;

		public double visualMagnitude;

		public OrbitalElements(double pPeriod, double pEpochLongitude, double pPerihelionLongitude, double pEccentricity, double pSemiMajorAxis, double pInclination, double pLongitudeAscendingNode, double pAngularDiameter, double pVisualMagnitude)
		{
			period = pPeriod;
			epochLongitude = pEpochLongitude;
			perihelionLongitude = pPerihelionLongitude;
			eccentricity = pEccentricity;
			semiMajorAxis = pSemiMajorAxis;
			inclination = pInclination;
			longitudeAscendingNode = pLongitudeAscendingNode;
			angularDiameter = pAngularDiameter;
			visualMagnitude = pVisualMagnitude;
		}
	}

	private double PI = 3.14159265;

	private double PLANET_DELTA_DAYS = 1.0 / 24.0;

	private SilverLiningMatrix3 equatorialToHorizon;

	private SilverLiningMatrix3 eclipticToHorizon;

	private SilverLiningMatrix3 eclipticToEquatorial;

	private SilverLiningMatrix3 equatorialToGeographic;

	private SilverLiningMatrix3 horizonToEquatorial;

	private SilverLiningMatrix3 horizonToGeographic;

	private SilverLiningMatrix3 geographicToEquatorial;

	private SilverLiningMatrix3 geographicToHorizon;

	private SilverLiningMatrix3 precession;

	private double sunEclipticLongitude;

	private double moonPhase;

	private double moonPhaseAngle;

	private double moonDistance;

	private double GMST;

	private double LMST;

	private double T;

	private double Tuncorr;

	private double epochDays;

	private Vector3 moonEq;

	private Vector3 sunEq;

	private Vector3 moonEcl;

	private Vector3 sunEcl;

	private Vector3 sunHoriz;

	private Vector3 moonHoriz;

	private Vector3 moonGeo;

	private Vector3 sunGeo;

	private bool geoZUp;

	private double R;

	private double L;

	private double e;

	private SilverLiningTime lastTime;

	private SilverLiningLocation lastLocation;

	private Planet[] planets;

	private OrbitalElements[] planetElements;

	protected double DEGREES(double x)
	{
		return x * (180.0 / PI);
	}

	protected double RADIANS(double x)
	{
		return x * (PI / 180.0);
	}

	public SilverLiningEphemeris()
	{
		equatorialToHorizon = new SilverLiningMatrix3();
		eclipticToHorizon = new SilverLiningMatrix3();
		eclipticToEquatorial = new SilverLiningMatrix3();
		equatorialToGeographic = new SilverLiningMatrix3();
		horizonToEquatorial = new SilverLiningMatrix3();
		horizonToGeographic = new SilverLiningMatrix3();
		geographicToEquatorial = new SilverLiningMatrix3();
		geographicToHorizon = new SilverLiningMatrix3();
		precession = new SilverLiningMatrix3();
		moonEq = default(Vector3);
		sunEq = default(Vector3);
		moonEcl = default(Vector3);
		sunEcl = default(Vector3);
		sunHoriz = default(Vector3);
		moonHoriz = default(Vector3);
		moonGeo = default(Vector3);
		sunGeo = default(Vector3);
		geoZUp = true;
		lastLocation = new SilverLiningLocation();
		lastTime = new SilverLiningTime();
		planets = new Planet[6];
		planetElements = new OrbitalElements[6];
		planetElements[0] = new OrbitalElements(0.240852, RADIANS(60.750646), RADIANS(77.299833), 0.205633, 0.387099, RADIANS(7.00454), RADIANS(48.21274), 6.74, -0.42);
		planetElements[1] = new OrbitalElements(0.615211, RADIANS(88.455855), RADIANS(131.430236), 0.006778, 0.723332, RADIANS(3.394535), RADIANS(76.58982), 16.92, -4.4);
		planetElements[2] = new OrbitalElements(1.00004, RADIANS(99.403308), RADIANS(102.768413), 0.016713, 1.0, 0.0, 0.0, 0.0, 0.0);
		planetElements[3] = new OrbitalElements(1.880932, RADIANS(240.739474), RADIANS(335.874939), 0.093396, 1.523688, RADIANS(1.849736), RADIANS(49.480308), 9.36, -1.52);
		planetElements[4] = new OrbitalElements(11.863075, RADIANS(90.638185), RADIANS(14.170747), 0.048482, 5.202561, RADIANS(1.303613), RADIANS(100.353142), 196.74, -9.4);
		planetElements[5] = new OrbitalElements(29.471362, RADIANS(287.690033), RADIANS(92.861407), 0.055581, 9.554747, RADIANS(2.48898), RADIANS(113.576139), 165.6, -8.88);
	}

	public void Update(SilverLiningTime time, SilverLiningLocation location)
	{
		bool flag = false;
		bool flag2 = false;
		if (time != lastTime)
		{
			flag = true;
			lastTime = new SilverLiningTime(time);
		}
		if (location != lastLocation)
		{
			flag2 = true;
			lastLocation = new SilverLiningLocation(location);
		}
		if (!(flag || flag2))
		{
			return;
		}
		T = time.GetEpoch2000Centuries(terrestrialTime: true);
		Tuncorr = time.GetEpoch2000Centuries(terrestrialTime: false);
		epochDays = time.GetEpoch1990Days(terrestrialTime: false);
		SilverLiningMatrix3 silverLiningMatrix = new SilverLiningMatrix3();
		SilverLiningMatrix3 silverLiningMatrix2 = new SilverLiningMatrix3();
		SilverLiningMatrix3 silverLiningMatrix3 = new SilverLiningMatrix3();
		silverLiningMatrix.FromRx(-0.1118 * T);
		silverLiningMatrix2.FromRy(0.00972 * T);
		silverLiningMatrix3.FromRz(-0.01118 * T);
		precession = silverLiningMatrix3 * (silverLiningMatrix2 * silverLiningMatrix);
		GMST = 4.894961 + 230121.675315 * Tuncorr;
		LMST = GMST + RADIANS(location.GetLongitude());
		double num = RADIANS(location.GetLatitude());
		e = 0.409093 - 0.000227 * T;
		silverLiningMatrix2.FromRy(0.0 - (num - PI / 2.0));
		silverLiningMatrix3.FromRz(LMST);
		silverLiningMatrix.FromRx(0.0 - e);
		equatorialToHorizon = silverLiningMatrix2 * silverLiningMatrix3 * precession;
		eclipticToHorizon = silverLiningMatrix2 * silverLiningMatrix3 * silverLiningMatrix * precession;
		eclipticToEquatorial = silverLiningMatrix;
		equatorialToGeographic.FromRz(GMST);
		geographicToEquatorial = equatorialToGeographic.Transpose();
		horizonToEquatorial = equatorialToHorizon.Transpose();
		horizonToGeographic = equatorialToGeographic * horizonToEquatorial;
		geographicToHorizon = equatorialToHorizon * geographicToEquatorial;
		ComputeSunPosition();
		ComputeMoonPosition();
		ComputeEarthPosition();
		for (int i = 0; i < 6; i++)
		{
			if (i != 2)
			{
				ComputePlanetPosition(i);
			}
		}
	}

	public Vector3 GetSunPositionEquatorial()
	{
		return sunEq;
	}

	public Vector3 GetMoonPositionEquatorial()
	{
		return moonEq;
	}

	public Vector3 GetSunPositionGeographic()
	{
		return sunGeo;
	}

	public Vector3 GetMoonPositionGeographic()
	{
		return moonGeo;
	}

	public Vector3 GetSunPositionEcliptic()
	{
		return sunEcl;
	}

	public Vector3 GetMoonPositionEcliptic()
	{
		return moonEcl;
	}

	public Vector3 GetSunPositionHorizon()
	{
		return sunHoriz;
	}

	public Vector3 GetMoonPositionHorizon()
	{
		return moonHoriz;
	}

	public double GetMoonPhaseAngle()
	{
		return moonPhaseAngle;
	}

	public double GetMoonPhase()
	{
		return moonPhase;
	}

	public double GetMoonDistanceKM()
	{
		return moonDistance;
	}

	public SilverLiningMatrix3 GetEclipticToHorizonMatrix()
	{
		return eclipticToHorizon;
	}

	public SilverLiningMatrix3 GetEquatorialToGeographicMatrix()
	{
		return equatorialToGeographic;
	}

	public SilverLiningMatrix3 GetEquatorialToHorizonMatrix()
	{
		return equatorialToHorizon;
	}

	public SilverLiningMatrix3 GetHorizonToEquatorialMatrix()
	{
		return horizonToEquatorial;
	}

	public SilverLiningMatrix3 GetHorizonToGeographicMatrix()
	{
		return horizonToGeographic;
	}

	public SilverLiningMatrix3 GetGeographicToHorizonMatrix()
	{
		return geographicToHorizon;
	}

	public double GetEpochCenturies()
	{
		return T;
	}

	private Vector3 ToCartesian(double r, double latitude, double longitude)
	{
		Vector3 result = default(Vector3);
		result.x = (float)(r * Math.Cos(longitude) * Math.Cos(latitude));
		result.y = (float)(r * Math.Sin(longitude) * Math.Cos(latitude));
		result.z = (float)(r * Math.Sin(latitude));
		return result;
	}

	private double Refract(double elevation)
	{
		double x;
		if (elevation > RADIANS(85.0))
		{
			x = 0.0;
		}
		else
		{
			double num = Math.Tan(elevation);
			if (elevation >= RADIANS(5.0))
			{
				x = 58.1 / num - 0.07 / Math.Pow(num, 3.0) + 8.6E-05 / Math.Pow(num, 5.0);
			}
			else if (elevation >= RADIANS(-0.575))
			{
				double num2 = DEGREES(elevation);
				x = 1735.0 + num2 * (-518.2 + num2 * (103.4 + num2 * (-12.79 + num2 * 0.711)));
			}
			else
			{
				x = -20.774 / num;
			}
			x /= 3600.0;
		}
		return elevation + RADIANS(x);
	}

	private Vector3 ConvertAxes(Vector3 v)
	{
		Vector3 result = default(Vector3);
		result.x = v.y;
		result.y = v.z;
		result.z = v.x;
		return result;
	}

	private void ComputeSunPosition()
	{
		double num = 6.24 + 628.302 * T;
		double longitude = 4.895048 + 628.331951 * T + (0.033417 - 8.4E-05 * T) * Math.Sin(num) + 0.000351 * Math.Sin(2.0 * num);
		double latitude = 0.0;
		double r = 1.00014 - (0.016708 - 4.2E-05 * T) * Math.Cos(num) - 0.000141 * Math.Cos(2.0 * num);
		sunEclipticLongitude = longitude;
		sunEcl = ToCartesian(r, latitude, longitude);
		sunEq = eclipticToEquatorial * sunEcl;
		if (geoZUp)
		{
			sunGeo = equatorialToGeographic * sunEq;
		}
		else
		{
			sunGeo = ConvertAxes(equatorialToGeographic * sunEq);
		}
		sunHoriz = ConvertAxes(eclipticToHorizon * sunEcl);
		Vector3 vector = sunHoriz;
		double num2 = vector.magnitude;
		vector.Normalize();
		double elevation = Math.Asin(vector.y);
		elevation = Refract(elevation);
		sunHoriz.y = (float)(num2 * Math.Sin(elevation));
	}

	private void InRange(ref double d)
	{
		while (d > 2.0 * PI)
		{
			d -= 2.0 * PI;
		}
		while (d < 0.0)
		{
			d += 2.0 * PI;
		}
	}

	private void InRangef(ref float d)
	{
		while (d > 2f * (float)PI)
		{
			d -= 2f * (float)PI;
		}
		while (d < 0f)
		{
			d += 2f * (float)PI;
		}
	}

	private void ComputeMoonPosition()
	{
		double num = 3.8104 + 8399.7091 * T;
		double num2 = 6.23 + 628.3019 * T;
		double num3 = 1.628 + 8433.4663 * T;
		double num4 = 2.3554 + 8328.6911 * T;
		double num5 = 5.1985 + 7771.3772 * T;
		double d = num + 0.1098 * Math.Sin(num4) + 0.0222 * Math.Sin(2.0 * num5 - num4) + 0.0115 * Math.Sin(2.0 * num5) + 0.0037 * Math.Sin(2.0 * num4) - 0.0032 * Math.Sin(num2) - 0.002 * Math.Sin(2.0 * num3) + 0.001 * Math.Sin(2.0 * num5 - 2.0 * num4) + 0.001 * Math.Sin(2.0 * num5 - num2 - num4) + 0.0009 * Math.Sin(2.0 * num5 + num4) + 0.0008 * Math.Sin(2.0 * num5 - num2) + 0.0007 * Math.Sin(num4 - num2) - 0.0006 * Math.Sin(num5) - 0.0005 * Math.Sin(num2 + num4);
		double latitude = 0.0895 * Math.Sin(num3) + 0.0049 * Math.Sin(num4 + num3) + 0.0048 * Math.Sin(num4 - num3) + 0.003 * Math.Sin(2.0 * num5 - num3) + 0.001 * Math.Sin(2.0 * num5 + num3 - num4) + 0.0008 * Math.Sin(2.0 * num5 - num3 - num4) + 0.0006 * Math.Sin(2.0 * num5 + num3);
		double num6 = 0.016593 + 0.000904 * Math.Cos(num4) + 0.000166 * Math.Cos(2.0 * num5 - num4) + 0.000137 * Math.Cos(2.0 * num5) + 4.9E-05 * Math.Cos(2.0 * num4) + 1.5E-05 * Math.Cos(2.0 * num5 + num4) + 9E-06 * Math.Cos(2.0 * num5 - num2);
		double r = 1.0 / num6;
		moonEcl = ToCartesian(r, latitude, d);
		moonEq = eclipticToEquatorial * moonEcl;
		if (geoZUp)
		{
			moonGeo = equatorialToGeographic * moonEq;
		}
		else
		{
			moonGeo = ConvertAxes(equatorialToGeographic * moonEq);
		}
		moonHoriz = ConvertAxes(eclipticToHorizon * moonEcl);
		InRange(ref d);
		InRange(ref sunEclipticLongitude);
		moonPhaseAngle = d - sunEclipticLongitude;
		InRange(ref moonPhaseAngle);
		moonPhase = 0.5 * (1.0 - Math.Cos(moonPhaseAngle));
		Vector3 vector = new Vector3(0f, 0f, 1f);
		moonDistance = (double)(moonHoriz - vector).magnitude * 6378.137;
	}

	private void ComputeEarthPosition()
	{
		double d = 2.0 * PI / 365.242191 * (epochDays / planetElements[2].period);
		InRange(ref d);
		double a = d + planetElements[2].epochLongitude - planetElements[2].perihelionLongitude;
		L = d + 2.0 * planetElements[2].eccentricity * Math.Sin(a) + planetElements[2].epochLongitude;
		InRange(ref L);
		double d2 = L - planetElements[2].perihelionLongitude;
		R = planetElements[2].semiMajorAxis * (1.0 - planetElements[2].eccentricity * planetElements[2].eccentricity) / (1.0 + planetElements[2].eccentricity * Math.Cos(d2));
	}

	private void GetPlanetPosition(int planet, ref double ra, ref double dec, ref double visualMagnitude)
	{
		if (planet < 6)
		{
			ra = planets[planet].rightAscension;
			dec = planets[planet].declination;
			visualMagnitude = planets[planet].visualMagnitude;
		}
	}

	private void ComputePlanetPosition(int planet)
	{
		if (!(epochDays - planets[planet].lastEpochDaysCalculated < PLANET_DELTA_DAYS))
		{
			planets[planet].lastEpochDaysCalculated = epochDays;
			double d = 2.0 * PI / 365.242191 * (epochDays / planetElements[planet].period);
			InRange(ref d);
			double a = d + planetElements[planet].epochLongitude - planetElements[planet].perihelionLongitude;
			double d2 = d + 2.0 * planetElements[planet].eccentricity * Math.Sin(a) + planetElements[planet].epochLongitude;
			InRange(ref d2);
			double d3 = d2 - planetElements[planet].perihelionLongitude;
			double num = planetElements[planet].semiMajorAxis * (1.0 - planetElements[planet].eccentricity * planetElements[planet].eccentricity) / (1.0 + planetElements[planet].eccentricity * Math.Cos(d3));
			double d4 = Math.Asin(Math.Sin(d2 - planetElements[planet].longitudeAscendingNode) * Math.Sin(planetElements[planet].inclination));
			InRange(ref d4);
			double y = Math.Sin(d2 - planetElements[planet].longitudeAscendingNode) * Math.Cos(planetElements[planet].inclination);
			double x = Math.Cos(d2 - planetElements[planet].longitudeAscendingNode);
			double num2 = Math.Atan2(y, x) + planetElements[planet].longitudeAscendingNode;
			double num3 = num * Math.Cos(d4);
			double d5 = ((planet <= 2) ? (PI + L + Math.Atan(num3 * Math.Sin(L - num2) / (R - num3 * Math.Cos(L - num2)))) : (Math.Atan(R * Math.Sin(num2 - L) / (num3 - R * Math.Cos(num2 - L))) + num2));
			InRange(ref d5);
			double num4 = Math.Atan(num3 * Math.Tan(d4) * Math.Sin(d5 - num2) / (R * Math.Sin(num2 - L)));
			double rightAscension = Math.Atan2(Math.Sin(d5) * Math.Cos(e) - Math.Tan(num4) * Math.Sin(e), Math.Cos(d5));
			double declination = Math.Asin(Math.Sin(num4) * Math.Cos(e) + Math.Cos(num4) * Math.Sin(e) * Math.Sin(d5));
			double num5 = Math.Sqrt(R * R + num * num - 2.0 * R * num * Math.Cos(d2 - L));
			double num6 = d5 - d2;
			double d6 = 0.5 * (1.0 + Math.Cos(num6));
			double visualMagnitude;
			if (planet == 1)
			{
				num6 = DEGREES(num6);
				visualMagnitude = -4.34 + 5.0 * Math.Log10(num * num5) + 0.013 * num6 + 4.2E-07 * num6 * num6 * num6;
			}
			else
			{
				visualMagnitude = 5.0 * Math.Log10(num * num5 / Math.Sqrt(d6)) + planetElements[planet].visualMagnitude;
			}
			planets[planet].rightAscension = rightAscension;
			planets[planet].declination = declination;
			planets[planet].visualMagnitude = visualMagnitude;
		}
	}
}
