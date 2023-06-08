using UnityEngine;
using UnityEngine.SceneManagement;

namespace ShatterToolkit.Examples;

public class SceneGUI : MonoBehaviour
{
	protected int toolbarSelection;

	protected string[] toolbarLabels = new string[3] { "Basic scene", "UvMapping scene", "Wall scene" };

	public void Awake()
	{
		toolbarSelection = SceneManager.GetActiveScene().buildIndex;
	}

	public void OnGUI()
	{
		toolbarSelection = GUI.Toolbar(new Rect(10f, Screen.height - 30, Screen.width - 20, 20f), toolbarSelection, toolbarLabels);
		if (GUI.changed)
		{
			SceneManager.LoadScene(toolbarSelection);
		}
	}
}
