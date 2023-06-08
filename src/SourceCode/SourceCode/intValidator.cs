using UnityEngine;

public class intValidator : MonoBehaviour
{
	public static bool isInteger(string s)
	{
		try
		{
			int.Parse(s);
		}
		catch
		{
			return false;
		}
		return true;
	}
}
