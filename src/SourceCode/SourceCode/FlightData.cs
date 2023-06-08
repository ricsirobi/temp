using System.Collections.Generic;
using UnityEngine;

public class FlightData : MonoBehaviour
{
	public List<FlightInformation> _FlightInformation = new List<FlightInformation>();

	public static AvAvatarFlyingData GetFlightData(GameObject flyingObject, FlightDataType flightDataType)
	{
		FlightData component = flyingObject.GetComponent<FlightData>();
		if (component == null)
		{
			Debug.LogError("Attempting to get flight data from object " + flyingObject?.ToString() + " , but no flight data component is attached!!!");
			return null;
		}
		for (int i = 0; i < component._FlightInformation.Count; i++)
		{
			if (component._FlightInformation[i]._FlightType == flightDataType)
			{
				return component._FlightInformation[i]._FlightData.Clone();
			}
		}
		Debug.LogError("No flight type of " + flightDataType.ToString() + " was found for: " + flyingObject);
		return null;
	}
}
