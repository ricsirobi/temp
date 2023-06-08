using System;

namespace tv.superawesome.sdk.publisher;

[Serializable]
public class GetIsMinorModel
{
	public string country;

	public int consentAgeForCountry;

	public int age;

	public bool isMinor;

	public GetIsMinorModel(string country, int consentAgeForCountry, int age, bool isMinor)
	{
		this.country = country;
		this.consentAgeForCountry = consentAgeForCountry;
		this.age = age;
		this.isMinor = isMinor;
	}
}
