using UnityEngine;

public class MazeLoader : MonoBehaviour
{
	[Tooltip("Set this to the name of the current event's maze.")]
	public string _SceneToLoad;

	private void OnLevelReady()
	{
		try
		{
			UtUtilities.LoadLevel(_SceneToLoad);
		}
		catch
		{
			UtUtilities.LoadLevel("HubBerkDO");
		}
	}
}
