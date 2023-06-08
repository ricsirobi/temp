using UnityEngine;
using UnityEngine.SceneManagement;

public class ShowcaseGUI : MonoBehaviour
{
	private static ShowcaseGUI instance;

	private int levels = 10;

	private void Start()
	{
		if ((bool)instance)
		{
			Object.Destroy(base.gameObject);
		}
		instance = this;
		Object.DontDestroyOnLoad(base.gameObject);
		ActivateSurroundings();
		SceneManager.sceneLoaded += OnLevelLoaded;
	}

	private void OnLevelLoaded(Scene scene, LoadSceneMode mode)
	{
		ActivateSurroundings();
	}

	private void ActivateSurroundings()
	{
		GameObject gameObject = GameObject.Find("Floor_Tile");
		if (!gameObject)
		{
			return;
		}
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.SetActive(value: true);
		}
	}

	private void OnGUI()
	{
		int width = Screen.width;
		int num = 30;
		int num2 = 40;
		Rect rect = new Rect(width - num * 2 - 70, 10f, num, num2);
		if (SceneManager.GetActiveScene().buildIndex > 0 && GUI.Button(rect, "<"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
		}
		else if (GUI.Button(new Rect(rect), "<"))
		{
			SceneManager.LoadScene(levels - 1);
		}
		GUI.Box(new Rect(width - num - 70, 10f, 60f, num2), "Scene:\n" + (SceneManager.GetActiveScene().buildIndex + 1) + " / " + levels);
		Rect source = new Rect(width - num - 10, 10f, num, num2);
		if (SceneManager.GetActiveScene().buildIndex < levels - 1 && GUI.Button(new Rect(source), ">"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
		else if (GUI.Button(new Rect(source), ">"))
		{
			SceneManager.LoadScene(0);
		}
		GUI.Box(new Rect(width - 130, 50f, 120f, 55f), "Example scenes\nmust be added\nto Build Settings.");
	}
}
