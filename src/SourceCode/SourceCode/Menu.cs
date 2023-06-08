using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
	private void OnGUI()
	{
		if (GUI.Button(new Rect(Screen.width - 100, 10f, 70f, 30f), "Scene 1"))
		{
			SceneManager.LoadScene(0);
		}
		if (GUI.Button(new Rect(Screen.width - 100, 50f, 70f, 30f), "Scene 2"))
		{
			SceneManager.LoadScene(1);
		}
		if (GUI.Button(new Rect(Screen.width - 100, 90f, 70f, 30f), "Scene 3"))
		{
			SceneManager.LoadScene(2);
		}
	}
}
