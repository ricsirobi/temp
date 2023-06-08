using UnityEngine;

public class UWSwimmingData : KAMonoBase
{
	public AvAvatarUWSwimmingData _UWSwimmingData;

	public static AvAvatarUWSwimmingData GetSwimmingData(GameObject uwSwimmingObject)
	{
		UWSwimmingData component = uwSwimmingObject.GetComponent<UWSwimmingData>();
		if (component._UWSwimmingData == null)
		{
			Debug.LogError("Attempting to get flight data from object " + uwSwimmingObject?.ToString() + " , but no flight data component is attached!!!");
			return null;
		}
		return component._UWSwimmingData.Clone();
	}
}
